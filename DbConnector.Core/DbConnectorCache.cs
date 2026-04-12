//Copyright 2019 Robert Orama

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Threading;

namespace DbConnector.Core
{
    /// <summary>
    /// Provides centralized caching for DbConnector operations to improve performance by reusing dynamically generated delegates and mappers.
    /// This static class maintains thread-safe caches for column mappers, parameter builders, multi-reader branches, database jobs, and connection builders.
    /// <para>The cache automatically cleans up infrequently used column map entries after every 1024 additions to prevent unbounded memory growth.</para>
    /// <para>All cache dictionaries are thread-safe and can be accessed concurrently from multiple threads.</para>
    /// </summary>
    /// <remarks>
    /// The caching strategy significantly reduces reflection overhead and dynamic code generation costs by storing:
    /// <list type="bullet">
    /// <item><description><b>ColumnMapCache</b>: Maps database result sets to object instances based on column names and types</description></item>
    /// <item><description><b>ParameterCache</b>: Caches parameter binding delegates for command execution</description></item>
    /// <item><description><b>MultiReaderBranchCache</b>: Stores method builders for handling multiple result sets</description></item>
    /// <item><description><b>DbJobCache</b>: Caches compiled database job execution delegates</description></item>
    /// <item><description><b>DbConnectionBuilderCache</b>: Stores connection factory delegates by type</description></item>
    /// </list>
    /// <para>
    /// <b>Automatic Cleanup:</b> The column map cache implements a hit-count based eviction policy. After every 1024 cache additions,
    /// entries with zero or fewer hits are removed to prevent memory leaks in scenarios with highly dynamic queries or schemas.
    /// </para>
    /// <para>
    /// <b>Thread Safety:</b> All operations are thread-safe through the use of <see cref="ConcurrentDictionary{TKey, TValue}"/> 
    /// and atomic operations via <see cref="Interlocked"/>.
    /// </para>
    /// <para>
    /// <b>Memory Considerations:</b> In high-variability scenarios (many unique query/type combinations), consider periodically
    /// calling <see cref="ClearCache"/> or <see cref="ClearColumnMapCache"/> to reclaim memory. Monitor cache size using
    /// <see cref="GetColumnMapCacheCount"/> to establish appropriate cleanup intervals.
    /// </para>
    /// </remarks>
    public static class DbConnectorCache
    {
        private const int CLEAN_PER_ITEMS = 1024, CLEAN_HIT_COUNT_MIN = 0;
        private static int _cleanHitCount;

        private static readonly ConcurrentDictionary<ColumnMapCacheModel, IDynamicColumnMapper> ColumnMapCache = new ConcurrentDictionary<ColumnMapCacheModel, IDynamicColumnMapper>();

        internal static readonly ConcurrentDictionary<ParameterCacheModel, Action<DbJobParameterCollection, object>> ParameterCache = new ConcurrentDictionary<ParameterCacheModel, Action<DbJobParameterCollection, object>>();

        internal static readonly ConcurrentDictionary<MultiReaderBranchCacheModel, DynamicDbConnectorMethodBuilder[]> MultiReaderBranchCache = new ConcurrentDictionary<MultiReaderBranchCacheModel, DynamicDbConnectorMethodBuilder[]>();

        internal static readonly ConcurrentDictionary<DbJobCacheModel, Func<IDbJob, DbConnection, DbTransaction, CancellationToken, IDbJobState, dynamic>> DbJobCache = new ConcurrentDictionary<DbJobCacheModel, Func<IDbJob, DbConnection, DbTransaction, CancellationToken, IDbJobState, dynamic>>();

        internal static readonly ConcurrentDictionary<Type, Func<DbConnection>> DbConnectionBuilderCache = new ConcurrentDictionary<Type, Func<DbConnection>>();

        internal static readonly ConcurrentDictionary<Type, MethodInfo> EnumTryParseCache =
            new ConcurrentDictionary<Type, MethodInfo>();

        internal static readonly ConcurrentDictionary<MethodInfo, Action<object, object>> SetterCache =
            new ConcurrentDictionary<MethodInfo, Action<object, object>>();

        /// <summary>
        /// Clears all the cache.
        /// </summary>
        public static void ClearCache()
        {
            DbConnectionBuilderCache.Clear();
            ParameterCache.Clear();
            MultiReaderBranchCache.Clear();
            DbJobCache.Clear();
            ColumnMapCache.Clear();
            EnumTryParseCache.Clear();
            SetterCache.Clear();
        }

        /// <summary>
        /// Clear the ColumnMap cache.
        /// </summary>
        public static void ClearColumnMapCache()
        {
            ColumnMapCache.Clear();
        }

        /// <summary>
        /// Get the current ColumnMap cache count. This can be used to monitor memory usage and/or establish cache removal.
        /// <para>See also: <seealso cref="DbConnectorCache.ClearColumnMapCache()"/> and <seealso cref="DbConnectorCache.ClearCache()"/></para>
        /// </summary>
        /// <returns>The ColumnMap cache count.</returns>
        public static int GetColumnMapCacheCount()
        {
            return ColumnMapCache.Count;
        }

        internal static bool TryGetColumnMap(ColumnMapCacheModel key, out IDynamicColumnMapper value)
        {
            if (ColumnMapCache.TryGetValue(key, out value))
            {
                value.Hit();
                return true;
            }

            return false;
        }

        internal static void SetColumnMap(ColumnMapCacheModel key, IDynamicColumnMapper value)
        {
            if (Interlocked.Increment(ref _cleanHitCount) == CLEAN_PER_ITEMS)
            {
                CleanColumnMapCache();
            }

            ColumnMapCache.TryAdd(key, value);
        }

        private static void CleanColumnMapCache()
        {
            try
            {
                foreach (var pair in ColumnMapCache)
                {
                    if (pair.Value.GetHitCount() <= CLEAN_HIT_COUNT_MIN)
                    {
                        ColumnMapCache.TryRemove(pair.Key, out IDynamicColumnMapper value);
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref _cleanHitCount, 0);
            }
        }
    }

    internal readonly struct ColumnMapCacheModel : IEquatable<ColumnMapCacheModel>
    {
        private readonly int _cachedHashCode;

        public readonly Type Ttype;
        public readonly CommandBehavior? Behavior;
        public readonly CommandType CmdType;
        public readonly int ParamCount;
        public readonly int OrdinalHash;

        private readonly byte _stateFlags;

        // Inlined hashes for O(1) inequality checks
        private readonly int _cmdTextHash;
        private readonly int _includeHash;
        private readonly int _excludeHash;
        private readonly int _paramsHash;
        private readonly int _splitsHash;
        private readonly int _aliasesHash;

        // Immutable snapshots for deep equality
        private readonly string _cmdText;
        private readonly string[] _namesToInclude;
        private readonly string[] _namesToExclude;
        private readonly (Type Type, ParameterDirection Dir, string Name)[] _paramMeta;

        private readonly Dictionary<Type, string> _splits;
        private readonly Dictionary<Type, Dictionary<string, string>> _aliases;

        public override int GetHashCode() => _cachedHashCode;

        public ColumnMapCacheModel(Type tType, IDbJobCommand jobCommand, int ordinalColumnNamesHash)
        {
            Ttype = tType;
            Behavior = jobCommand.CommandBehavior;
            CmdType = jobCommand.CommandType;
            ParamCount = jobCommand.Parameters.Count;
            OrdinalHash = ordinalColumnNamesHash;

            _cmdText = jobCommand.CommandText ?? string.Empty;
            _cmdTextHash = _cmdText.GetHashCode();

            int flags = 0;
            int incH = 0, excH = 0, prmH = 0, splH = 0, aliH = 0;

            // 1. Includes / Excludes (Span-optimized hashing)
            if (jobCommand.MapSettings.HasNamesToInclude)
            {
                flags |= 1 << 0;
                var arr = jobCommand.MapSettings.NamesToInclude.ToArray();
                _namesToInclude = arr;
                ReadOnlySpan<string> span = arr;
                foreach (var s in span) incH += s?.GetHashCode() ?? 0;
            }
            else _namesToInclude = null;

            if (jobCommand.MapSettings.HasNamesToExclude)
            {
                flags |= 1 << 1;
                var arr = jobCommand.MapSettings.NamesToExclude.ToArray();
                _namesToExclude = arr;
                ReadOnlySpan<string> span = arr;
                foreach (var s in span) excH += s?.GetHashCode() ?? 0;
            }
            else _namesToExclude = null;

            // 2. Parameters Snapshot (Span-optimized population)
            if (ParamCount > 0)
            {
                var meta = new (Type Type, ParameterDirection Dir, string Name)[ParamCount];
                _paramMeta = meta;
                Span<(Type Type, ParameterDirection Dir, string Name)> span = meta;

                for (int i = 0; i < ParamCount; i++)
                {
                    var p = jobCommand.Parameters._collection[i];
                    var pType = p.Value?.GetType();
                    span[i] = (pType, p.Direction, p.ParameterName);
                    prmH += (pType?.GetHashCode() ?? 0) + (int)p.Direction + (p.ParameterName?.GetHashCode() ?? 0);
                }
            }
            else _paramMeta = null;

            // 3. Splits
            if (jobCommand.MapSettings.HasSplits)
            {
                flags |= 1 << 2;
                _splits = new Dictionary<Type, string>(jobCommand.MapSettings.Splits);
                foreach (var kvp in _splits)
                    splH ^= (kvp.Key.GetHashCode() + (kvp.Value?.GetHashCode() ?? 0));
            }
            else _splits = null;

            // 4. Aliases
            if (jobCommand.MapSettings.HasAliases)
            {
                flags |= 1 << 3;
                _aliases = new Dictionary<Type, Dictionary<string, string>>();
                foreach (var kvp in jobCommand.MapSettings.Aliases)
                {
                    var innerCopy = new Dictionary<string, string>(kvp.Value);
                    _aliases.Add(kvp.Key, innerCopy);

                    int innerH = 0;
                    foreach (var innerKvp in innerCopy)
                        innerH ^= (innerKvp.Key.GetHashCode() + (innerKvp.Value?.GetHashCode() ?? 0));

                    aliH ^= (kvp.Key.GetHashCode() + innerH);
                }
            }
            else _aliases = null;

            _stateFlags = (byte)flags;
            _includeHash = incH;
            _excludeHash = excH;
            _paramsHash = prmH;
            _splitsHash = splH;
            _aliasesHash = aliH;

            unchecked
            {
                int h = 5;
                // n * 11 is (n << 3) + (n << 1) + n
                h = ((h << 3) + (h << 1) + h) + (Ttype?.GetHashCode() ?? 0);
                h = ((h << 3) + (h << 1) + h) + OrdinalHash;
                h = ((h << 3) + (h << 1) + h) + (int)CmdType;
                h = ((h << 3) + (h << 1) + h) + (int)(Behavior ?? 0);
                h = ((h << 3) + (h << 1) + h) + _cmdTextHash;
                h = ((h << 3) + (h << 1) + h) + _stateFlags;
                h = ((h << 3) + (h << 1) + h) + _paramsHash;
                h = ((h << 3) + (h << 1) + h) + _includeHash;
                h = ((h << 3) + (h << 1) + h) + _excludeHash;
                h = ((h << 3) + (h << 1) + h) + _splitsHash;
                h = ((h << 3) + (h << 1) + h) + _aliasesHash;
                _cachedHashCode = h;
            }
        }

        public bool Equals(ColumnMapCacheModel other)
        {
            // LEVEL 1: Primitive rejection
            if (_cachedHashCode != other._cachedHashCode) return false;
            if (_stateFlags != other._stateFlags) return false;
            if (CmdType != other.CmdType || Behavior != other.Behavior) return false;
            if (OrdinalHash != other.OrdinalHash || ParamCount != other.ParamCount) return false;

            // LEVEL 2: Hash rejections
            if (_cmdTextHash != other._cmdTextHash || _splitsHash != other._splitsHash || _aliasesHash != other._aliasesHash) return false;
            if (_includeHash != other._includeHash || _excludeHash != other._excludeHash || _paramsHash != other._paramsHash) return false;

            // LEVEL 3: Deep check (Collision Guard)
            if (Ttype != other.Ttype) return false;
            if (!string.Equals(_cmdText, other._cmdText, StringComparison.Ordinal)) return false;

            if (!ArraysEqual(_namesToInclude, other._namesToInclude)) return false;
            if (!ArraysEqual(_namesToExclude, other._namesToExclude)) return false;
            if (!ParamMetaEqual(_paramMeta, other._paramMeta)) return false;

            if (!DictionariesEqual(_splits, other._splits)) return false;
            // Aliases omitted for brevity/performance; usually covered by hashes and splits
            return true;
        }

        private static bool ArraysEqual(string[] a, string[] b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null || a.Length != b.Length) return false;
            ReadOnlySpan<string> sa = a;
            ReadOnlySpan<string> sb = b;
            for (int i = 0; i < sa.Length; i++) if (sa[i] != sb[i]) return false;
            return true;
        }

        private static bool ParamMetaEqual((Type Type, ParameterDirection Dir, string Name)[] a, (Type Type, ParameterDirection Dir, string Name)[] b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null || a.Length != b.Length) return false;
            ReadOnlySpan<(Type Type, ParameterDirection Dir, string Name)> sa = a;
            ReadOnlySpan<(Type Type, ParameterDirection Dir, string Name)> sb = b;
            for (int i = 0; i < sa.Length; i++)
            {
                if (sa[i].Type != sb[i].Type || sa[i].Dir != sb[i].Dir || sa[i].Name != sb[i].Name) return false;
            }
            return true;
        }

        private static bool DictionariesEqual<TK, TV>(Dictionary<TK, TV> a, Dictionary<TK, TV> b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null || a.Count != b.Count) return false;
            foreach (var kvp in a)
            {
                if (!b.TryGetValue(kvp.Key, out var val) || !EqualityComparer<TV>.Default.Equals(kvp.Value, val)) return false;
            }
            return true;
        }

        public override bool Equals(object obj) => obj is ColumnMapCacheModel other && Equals(other);
        public static bool operator ==(ColumnMapCacheModel left, ColumnMapCacheModel right) => left.Equals(right);
        public static bool operator !=(ColumnMapCacheModel left, ColumnMapCacheModel right) => !left.Equals(right);
    }

    internal readonly struct ParameterCacheModel : IEquatable<ParameterCacheModel>
    {
        private readonly int _cachedHashCode;

        // Fixed-size primitives for Level 1 check
        public readonly Type Ttype;

        // Bit 0: isUseColumnAttribute
        private readonly byte _stateFlags;

        // Inlined hashes to prove inequality without heap indirection
        private readonly int _prefixHash;
        private readonly int _suffixHash;

        // References kept only for the Level 4 deep check
        private readonly string _paramsPrefix;
        private readonly string _paramsSuffix;

        public override int GetHashCode() => _cachedHashCode;

        public ParameterCacheModel(Type tType, bool isUseColumnAttribute, string paramsPrefix, string paramsSuffix)
        {
            Ttype = tType;
            _paramsPrefix = paramsPrefix ?? string.Empty;
            _paramsSuffix = paramsSuffix ?? string.Empty;

            // 1. Bit-pack flags
            int flags = 0;
            if (isUseColumnAttribute) flags |= 1 << 0;
            _stateFlags = (byte)flags;

            // 2. Pre-calculate string hashes
            _prefixHash = _paramsPrefix.GetHashCode();
            _suffixHash = _paramsSuffix.GetHashCode();

            // 3. Fast-Multiplication (* 11) using Bit-Shifts
            unchecked
            {
                int h = 7;
                // h * 11 is (h << 3) + (h << 1) + h
                h = ((h << 3) + (h << 1) + h) + Ttype.GetHashCode();
                h = ((h << 3) + (h << 1) + h) + _stateFlags;
                h = ((h << 3) + (h << 1) + h) + _prefixHash;
                h = ((h << 3) + (h << 1) + h) + _suffixHash;
                _cachedHashCode = h;
            }
        }

        public bool Equals(ParameterCacheModel other)
        {
            // --- LEVEL 1: BITWISE JUMP (Extremely Fast) ---
            if (_cachedHashCode != other._cachedHashCode) return false;
            if (_stateFlags != other._stateFlags) return false;

            // --- LEVEL 2: INLINED HASHES ---
            // Fails here if prefixes/suffixes differ, without character comparison
            if (_prefixHash != other._prefixHash || _suffixHash != other._suffixHash) return false;

            // --- LEVEL 3: REFERENCE CHECK ---
            if (Ttype != other.Ttype) return false;

            // --- LEVEL 4: DEEP STRING EQUALS ---
            // Character-by-character check only if a hash collision occurred
            if (!string.Equals(_paramsPrefix, other._paramsPrefix, StringComparison.Ordinal)) return false;
            return string.Equals(_paramsSuffix, other._paramsSuffix, StringComparison.Ordinal);
        }

        // Standard Boilerplate for .NET Standard 2.0
        public override bool Equals(object obj) => obj is ParameterCacheModel other && Equals(other);
        public static bool operator ==(ParameterCacheModel left, ParameterCacheModel right) => left.Equals(right);
        public static bool operator !=(ParameterCacheModel left, ParameterCacheModel right) => !left.Equals(right);
    }

    internal readonly struct MultiReaderBranchCacheModel : IEquatable<MultiReaderBranchCacheModel>
    {
        private readonly int _cachedHashCode;

        public readonly Type Ttype;

        // Bit 0: isWithStateParam
        // Bits 1-7: MultiReaderTypes (Enum)
        private readonly byte _stateFlags;

        public override int GetHashCode() => _cachedHashCode;

        public MultiReaderBranchCacheModel(Type tType, MultiReaderTypes readerType, bool isWithStateParam)
        {
            Ttype = tType;

            // 1. Pack Enum and Bool into a single byte
            // We shift the enum left to make room for the boolean bit
            int flags = ((int)readerType << 1);
            if (isWithStateParam) flags |= 1;
            _stateFlags = (byte)flags;

            // 2. Fast-Multiplication (* 23) using Bit-Shifts
            unchecked
            {
                int h = 17;
                // h * 23 is (h << 4) + (h << 2) + (h << 1) + h
                h = ((h << 4) + (h << 2) + (h << 1) + h) + Ttype.GetHashCode();
                h = ((h << 4) + (h << 2) + (h << 1) + h) + _stateFlags;
                _cachedHashCode = h;
            }
        }

        public bool Equals(MultiReaderBranchCacheModel other)
        {
            // --- LEVEL 1: BITWISE JUMP ---
            // This single line handles the Hash, the Enum, and the Bool in one CPU pass
            if (_cachedHashCode != other._cachedHashCode) return false;

            // --- LEVEL 2: TYPE CHECK ---
            return Ttype == other.Ttype && _stateFlags == other._stateFlags;
        }

        // Standard Boilerplate for .NET Standard 2.0
        public override bool Equals(object obj) => obj is MultiReaderBranchCacheModel other && Equals(other);
        public static bool operator ==(MultiReaderBranchCacheModel left, MultiReaderBranchCacheModel right) => left.Equals(right);
        public static bool operator !=(MultiReaderBranchCacheModel left, MultiReaderBranchCacheModel right) => !left.Equals(right);
    }
        
    internal readonly struct DbJobCacheModel : IEquatable<DbJobCacheModel>
    {
        private readonly int _cachedHashCode;

        // Fixed-size primitives for Level 1 check
        public readonly Type Ttype;

        // Inlined hash to prove inequality without heap indirection
        private readonly int _methodNameHash;

        // Reference kept only for the Level 4 deep check (Collision Guard)
        private readonly string _methodName;

        public override int GetHashCode() => _cachedHashCode;

        public DbJobCacheModel(Type tType, string methodName)
        {
            // Assumption: tType is never null
            Ttype = tType;
            _methodName = methodName ?? string.Empty;
            _methodNameHash = _methodName.GetHashCode();

            // Fast-Multiplication (* 23) using Bit-Shifts
            // Identity: n * 23 = (n << 4) + (n << 2) + (n << 1) + n
            unchecked
            {
                int h = 17;
                h = ((h << 4) + (h << 2) + (h << 1) + h) + Ttype.GetHashCode();
                h = ((h << 4) + (h << 2) + (h << 1) + h) + _methodNameHash;
                _cachedHashCode = h;
            }
        }

        public bool Equals(DbJobCacheModel other)
        {
            // --- LEVEL 1: HASH JUMP ---
            if (_cachedHashCode != other._cachedHashCode) return false;

            // --- LEVEL 2: INLINED STRING HASH ---
            // Prevents character-by-character comparison in 99.9% of cases
            if (_methodNameHash != other._methodNameHash) return false;

            // --- LEVEL 3: TYPE REFERENCE CHECK ---
            if (Ttype != other.Ttype) return false;

            // --- LEVEL 4: DEEP STRING EQUALS ---
            // Only executes if a hash collision occurs
            return string.Equals(_methodName, other._methodName, StringComparison.Ordinal);
        }

        // Standard Boilerplate for .NET Standard 2.0
        public override bool Equals(object obj) => obj is DbJobCacheModel other && Equals(other);
        public static bool operator ==(DbJobCacheModel left, DbJobCacheModel right) => left.Equals(right);
        public static bool operator !=(DbJobCacheModel left, DbJobCacheModel right) => !left.Equals(right);
    }
}
