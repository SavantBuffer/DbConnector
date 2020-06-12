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
using System.Data;
using System.Data.Common;
using System.Threading;

namespace DbConnector.Core
{
    public static class DbConnectorCache
    {
        private const int CLEAN_PER_ITEMS = 1024, CLEAN_HIT_COUNT_MIN = 0;
        private static int _cleanHitCount;

        private static readonly ConcurrentDictionary<ColumnMapCacheModel, IDynamicColumnMapper> ColumnMapCache = new ConcurrentDictionary<ColumnMapCacheModel, IDynamicColumnMapper>();

        internal static readonly ConcurrentDictionary<ParameterCacheModel, Action<DbJobParameterCollection, object>> ParameterCache = new ConcurrentDictionary<ParameterCacheModel, Action<DbJobParameterCollection, object>>();

        internal static readonly ConcurrentDictionary<MultiReaderBranchCacheModel, DynamicDbConnectorMethodBuilder[]> MultiReaderBranchCache = new ConcurrentDictionary<MultiReaderBranchCacheModel, DynamicDbConnectorMethodBuilder[]>();

        internal static readonly ConcurrentDictionary<DbJobCacheModel, Func<IDbJob, DbConnection, DbTransaction, CancellationToken, IDbJobState, dynamic>> DbJobCache = new ConcurrentDictionary<DbJobCacheModel, Func<IDbJob, DbConnection, DbTransaction, CancellationToken, IDbJobState, dynamic>>();

        internal static readonly ConcurrentDictionary<Type, Func<DbConnection>> DbConnectionBuilderCache = new ConcurrentDictionary<Type, Func<DbConnection>>();


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
        public readonly int HashCode;

        public readonly Type Ttype;

        //public readonly string CommandText;

        //public readonly CommandType CommandType;

        readonly CommandBehavior? CommandBehavior;

        readonly int ParameterCount;

        public override int GetHashCode()
        {
            return HashCode;
        }

        public bool Equals(ColumnMapCacheModel other)
        {
            return Ttype == other.Ttype
                //&& CommandText == other.CommandText
                //&& CommandType == other.CommandType
                && CommandBehavior == other.CommandBehavior
                && ParameterCount == other.ParameterCount;
        }

        public ColumnMapCacheModel(Type tType, IDbJobCommand jobCommand, int ordinalColumnNamesHash)
        {
            Ttype = tType;
            //CommandText = jobCommand.CommandText;
            //CommandType = jobCommand.CommandType;
            CommandBehavior = jobCommand.CommandBehavior;
            ParameterCount = jobCommand.Parameters.Count;

            unchecked
            {
                int hash = 5;
                int localHash = 0;

                hash = (hash * 11) + Ttype.GetHashCode();
                hash = (hash * 11) + ordinalColumnNamesHash;
                //hash = (hash * 11) + (byte)CommandType;
                //hash = (hash * 11) + (CommandText?.GetHashCode() ?? 0);
                hash = (hash * 11) + (CommandBehavior == null ? 0 : (int)CommandBehavior);

                if (ParameterCount != 0)
                {
                    for (int i = 0; i < ParameterCount; i++)
                    {
                        var item = jobCommand.Parameters._collection[i];

                        localHash +=
                                (item.Value?.GetType().GetHashCode() ?? 0)
                                + (int)item.Direction
                                + (item.ParameterName?.GetHashCode() ?? 0);
                    }

                    hash = (hash * 11) + localHash;
                    localHash = 0;
                }

                if (jobCommand.MapSettings.HasNamesToInclude)
                {
                    foreach (var item in jobCommand.MapSettings.NamesToInclude)
                    {
                        localHash += item.GetHashCode();
                    }

                    hash = (hash * 11) + localHash + 1;
                    localHash = 0;
                }

                if (jobCommand.MapSettings.HasNamesToExclude)
                {
                    foreach (var item in jobCommand.MapSettings.NamesToExclude)
                    {
                        localHash += item.GetHashCode();
                    }

                    hash = (hash * 11) + localHash;
                    localHash = 0;
                }

                if (jobCommand.MapSettings.HasSplits)
                {
                    foreach (var item in jobCommand.MapSettings.Splits)
                    {
                        localHash += (item.Key.GetHashCode() + item.Value.GetHashCode());
                    }

                    hash = (hash * 11) + localHash;
                    localHash = 0;
                }

                if (jobCommand.MapSettings.HasAliases)
                {
                    foreach (var item in jobCommand.MapSettings.Aliases)
                    {
                        foreach (var valueItem in item.Value)
                        {
                            localHash += (valueItem.Key.GetHashCode() + valueItem.Value.GetHashCode());
                        }

                        localHash += (item.Key.GetHashCode() + localHash);
                    }

                    hash = (hash * 11) + localHash;
                }

                HashCode = hash;
            }
        }
    }

    internal readonly struct ParameterCacheModel : IEquatable<ParameterCacheModel>
    {
        public readonly int HashCode;

        public readonly Type Ttype;

        public override int GetHashCode()
        {
            return HashCode;
        }

        public bool Equals(ParameterCacheModel other)
        {
            // Check for null
            //if (other == null)
            //    return false;

            return Ttype == other.Ttype;
        }

        public ParameterCacheModel(Type tType, bool isUseColumnAttribute, string paramsPrefix, string paramsSuffix)
        {
            Ttype = tType;

            unchecked
            {
                int hash = 7;
                hash = (hash * 11) + tType.GetHashCode();
                hash = (hash * 11) + (isUseColumnAttribute ? 11 : 7);
                hash = (hash * 11) + (paramsPrefix?.GetHashCode() ?? 0);
                hash = (hash * 11) + (paramsSuffix?.GetHashCode() ?? 0);

                HashCode = hash;
            }
        }
    }

    internal readonly struct MultiReaderBranchCacheModel : IEquatable<MultiReaderBranchCacheModel>
    {
        public readonly int HashCode;

        public readonly Type Ttype;

        public override int GetHashCode()
        {
            return HashCode;
        }

        public bool Equals(MultiReaderBranchCacheModel other)
        {
            // Check for null
            //if (other == null)
            //    return false;

            return Ttype == other.Ttype;
        }

        public MultiReaderBranchCacheModel(Type tType, MultiReaderTypes readerType, bool isWithStateParam)
        {
            Ttype = tType;

            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + tType.GetHashCode();
                hash = (hash * 23) + (byte)readerType;
                hash += (isWithStateParam ? 1 : 0);

                HashCode = hash;
            }
        }
    }

    internal readonly struct DbJobCacheModel : IEquatable<DbJobCacheModel>
    {
        public readonly int HashCode;

        public readonly Type Ttype;

        public override int GetHashCode()
        {
            return HashCode;
        }

        public bool Equals(DbJobCacheModel other)
        {
            // Check for null
            //if (other == null)
            //    return false;

            return Ttype == other.Ttype;
        }

        public DbJobCacheModel(Type tType, string methodName)
        {
            Ttype = tType;

            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + tType.GetHashCode();
                hash = (hash * 23) + methodName.GetHashCode();

                HashCode = hash;
            }
        }
    }
}
