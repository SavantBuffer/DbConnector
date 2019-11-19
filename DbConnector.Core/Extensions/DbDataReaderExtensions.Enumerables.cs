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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Threading;

namespace DbConnector.Core.Extensions
{
    public static partial class DbDataReaderExtensions
    {
        /// <summary>
        /// Reads data into an <see cref="IEnumerable{T}"/> in a deferred/yielded manner.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The generic type to use.</typeparam>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="cmd">The <see cref="IDbJobCommand"/> to use for data projection and caching. (Optional)</param>
        /// <returns>The <see cref="IEnumerable{T}"/>.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> is missing a parameterless constructor.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> is assignable from <see cref="System.Collections.IEnumerable"/>.</exception>
        public static IEnumerable<T> ToEnumerable<T>(this DbDataReader odr, CancellationToken token = default, IDbJobCommand cmd = null)
        {
            if (odr.HasRows)
            {
                Type tType = typeof(T);

                if (!DbConnectorUtilities._directTypeMap.Contains(tType) && !(tType.IsValueType && (tType.IsEnum || (Nullable.GetUnderlyingType(tType)?.IsEnum ?? false))) && !tType.IsArray)
                {
                    //Dynamic MSIL cached version is around 30% faster and uses up to 57% less memory.
                    if (cmd != null && (cmd.Flags & DbJobCommandFlags.NoCache) == DbJobCommandFlags.None)
                    {
                        ColumnMapCacheModel cacheModel = new ColumnMapCacheModel(tType, cmd, odr.GetOrdinalColumnNamesHash());

                        if (!DbConnectorCache.TryGetColumnMap(cacheModel, out IDynamicColumnMapper mapper))
                        {
                            mapper = DynamicColumnMapper.CreateMapper<T>(odr.GetOrdinalColumnNames(cmd.MapSettings), cmd.MapSettings);

                            DbConnectorCache.SetColumnMap(cacheModel, mapper);
                        }

                        var genericMapper = mapper as DynamicColumnMapper<T>;

                        while (odr.Read())
                        {
                            if (token.IsCancellationRequested)
                                yield break;

                            yield return genericMapper.OnBuild(odr);
                        }
                    }
                    else
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(tType))
                        {
                            throw new InvalidCastException("The type " + tType + " is not supported");
                        }
                        else if (tType.IsClass && tType.GetConstructor(Type.EmptyTypes) == null)
                        {
                            throw new InvalidCastException("The type " + tType + " is missing a parameterless constructor");
                        }

                        var columnMaps = odr.GetColumnMaps(tType, cmd?.MapSettings);

                        if (tType.IsClass)
                        {
                            while (odr.Read())
                            {
                                if (token.IsCancellationRequested)
                                    yield break;

                                yield return odr.GetMappedObject<T>(columnMaps);
                            }
                        }
                        else
                        {
                            while (odr.Read())
                            {
                                if (token.IsCancellationRequested)
                                    yield break;

                                yield return (T)odr.GetMappedObject(tType, columnMaps);
                            }

                        }
                    }
                }
                else if (tType == typeof(Dictionary<string, object>))
                {
                    foreach (var item in odr.ToEnumerableDictionaries(false, token, cmd))
                    {
                        if (token.IsCancellationRequested)
                            yield break;

                        yield return (T)Convert.ChangeType(item, tType);
                    }
                }
                else if (tType == typeof(List<KeyValuePair<string, object>>))
                {
                    foreach (var item in odr.ToEnumerableKeyValuePairs(false, token, cmd))
                    {
                        if (token.IsCancellationRequested)
                            yield break;

                        yield return (T)Convert.ChangeType(item, tType);
                    }
                }
                else if (tType == typeof(DataTable))
                {
                    yield return (T)Convert.ChangeType(odr.ToDataTable(false, token, cmd.MapSettings), tType);
                }
                else if (tType == typeof(DataSet))
                {
                    yield return (T)Convert.ChangeType(odr.ToDataSet(false, token, cmd.MapSettings), tType);
                }
                else if (tType == typeof(object))
                {
                    foreach (var item in odr.ToEnumerable(token, cmd))
                    {
                        if (token.IsCancellationRequested)
                            yield break;

                        yield return item;
                    }
                }
                else
                {
                    Type nonNullableObjType = tType.IsValueType ? (Nullable.GetUnderlyingType(tType) ?? tType) : tType;

                    while (odr.Read())
                    {
                        if (token.IsCancellationRequested)
                            yield break;

                        object value = odr.GetValue(0);

                        if (value != DBNull.Value)
                        {
                            yield return (T)(DbConnectorUtilities.ThrowIfFailedToMatchColumnType(tType, nonNullableObjType, value, () => odr.GetName(0)));
                        }
                        else
                        {
                            yield return default;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reads data into an <see cref="IEnumerable{object}"/> in a deferred/yielded manner.
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>        
        /// </summary>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="objType">The <see cref="Type"/> to use.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="cmd">The <see cref="IDbJobCommand"/> to use for data projection and caching. (Optional)</param>
        /// <returns>The <see cref="IEnumerable{object}"/>.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <paramref name="objType"/> is missing a parameterless constructor.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when <paramref name="objType"/> is assignable from <see cref="System.Collections.IEnumerable"/>.</exception>
        public static IEnumerable<object> ToEnumerable(this DbDataReader odr, Type objType, CancellationToken token = default, IDbJobCommand cmd = null)
        {
            if (odr.HasRows)
            {
                if (!DbConnectorUtilities._directTypeMap.Contains(objType) && !(objType.IsValueType && (objType.IsEnum || (Nullable.GetUnderlyingType(objType)?.IsEnum ?? false))) && !objType.IsArray)
                {
                    //Dynamic MSIL cached version is around 30% faster and uses up to 57% less memory.
                    if (cmd != null && (cmd.Flags & DbJobCommandFlags.NoCache) == DbJobCommandFlags.None)
                    {
                        ColumnMapCacheModel cacheModel = new ColumnMapCacheModel(objType, cmd, odr.GetOrdinalColumnNamesHash());

                        if (!DbConnectorCache.TryGetColumnMap(cacheModel, out IDynamicColumnMapper mapper))
                        {
                            mapper = DynamicColumnMapper.CreateMapper(objType, odr.GetOrdinalColumnNames(cmd.MapSettings), cmd.MapSettings);

                            DbConnectorCache.SetColumnMap(cacheModel, mapper);
                        }

                        while (odr.Read())
                        {
                            if (token.IsCancellationRequested)
                                yield break;

                            yield return mapper.Build(odr);
                        }
                    }
                    else
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(objType))
                        {
                            throw new InvalidCastException("The type " + objType + " is not supported");
                        }
                        else if (objType.IsClass && objType.GetConstructor(Type.EmptyTypes) == null)
                        {
                            throw new InvalidCastException("The type " + objType + " is missing a parameterless constructor");
                        }

                        var columnMaps = odr.GetColumnMaps(objType, cmd?.MapSettings);

                        while (odr.Read())
                        {
                            if (token.IsCancellationRequested)
                                yield break;

                            yield return odr.GetMappedObject(objType, columnMaps);
                        }
                    }
                }
                else if (objType == typeof(Dictionary<string, object>))
                {
                    foreach (var item in odr.ToEnumerableDictionaries(false, token, cmd))
                    {
                        if (token.IsCancellationRequested)
                            yield break;

                        yield return Convert.ChangeType(item, objType);
                    }
                }
                else if (objType == typeof(List<KeyValuePair<string, object>>))
                {
                    foreach (var item in odr.ToEnumerableKeyValuePairs(false, token, cmd))
                    {
                        if (token.IsCancellationRequested)
                            yield break;

                        yield return Convert.ChangeType(item, objType);
                    }
                }
                if (objType == typeof(DataTable))
                {
                    yield return Convert.ChangeType(odr.ToDataTable(false, token, cmd.MapSettings), objType);
                }
                else if (objType == typeof(DataSet))
                {
                    yield return Convert.ChangeType(odr.ToDataSet(false, token, cmd.MapSettings), objType);
                }
                else
                {
                    Type nonNullableObjType = objType.IsValueType ? (Nullable.GetUnderlyingType(objType) ?? objType) : objType;

                    while (odr.Read())
                    {
                        if (token.IsCancellationRequested)
                            yield break;

                        object value = odr.GetValue(0);

                        if (value != DBNull.Value)
                        {
                            yield return DbConnectorUtilities.ThrowIfFailedToMatchColumnType(objType, nonNullableObjType, value, () => odr.GetName(0));
                        }
                        else if (objType.IsValueType)
                        {
                            yield return Activator.CreateInstance(objType);
                        }
                        else
                        {
                            yield return null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reads data into an <see cref="IEnumerable{dynamic}"/> in a deferred/yielded manner.
        /// </summary>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="cmd">The <see cref="IDbJobCommand"/> to use for data projection and caching. (Optional)</param>
        /// <returns>The <see cref="IEnumerable{dynamic}"/>.</returns>
        public static IEnumerable<dynamic> ToEnumerable(this DbDataReader odr, CancellationToken token = default, IDbJobCommand cmd = null)
        {
            if (odr.HasRows)
            {
                if (cmd != null && (cmd.Flags & DbJobCommandFlags.NoCache) == DbJobCommandFlags.None)
                {
                    ColumnMapCacheModel cacheModel = new ColumnMapCacheModel(typeof(ExpandoObject), cmd, odr.GetOrdinalColumnNamesHashLite());

                    if (!DbConnectorCache.TryGetColumnMap(cacheModel, out IDynamicColumnMapper mapper))
                    {
                        mapper = DynamicColumnMapper.CreateExpandoObjectMapper(odr.GetOrdinalColumnNamesLite(cmd.MapSettings), odr, out bool isRead, out ExpandoObject mappedRow);

                        DbConnectorCache.SetColumnMap(cacheModel, mapper);

                        if (isRead)
                        {
                            yield return mappedRow;
                        }
                        else
                        {
                            yield break;
                        }
                    }

                    while (odr.Read())
                    {
                        if (token.IsCancellationRequested)
                            yield break;

                        yield return mapper.Build(odr);
                    }
                }
                else
                {
                    var ordinalColumnMap = odr.GetOrdinalColumnNamesLite(cmd?.MapSettings);

                    if (ordinalColumnMap?.Length > 0)
                    {
                        while (odr.Read())
                        {
                            if (token.IsCancellationRequested)
                                yield break;

                            yield return odr.ToFirst(ordinalColumnMap);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reads data into an <see cref="IEnumerable{List{KeyValuePair{string, object}}}"/> in a deferred/yielded manner.
        /// </summary>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="isFirstResult">Set to true if only the first row should be loaded.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="cmd">The <see cref="IDbJobCommand"/> to use for data projection and caching. (Optional)</param>
        /// <returns>The <see cref="IEnumerable{List{KeyValuePair{string, object}}}"/>.</returns>
        public static IEnumerable<List<KeyValuePair<string, object>>> ToEnumerableKeyValuePairs(
            this DbDataReader odr,
            bool isFirstResult,
            CancellationToken token = default,
            IDbJobCommand cmd = null)
        {
            if (odr.HasRows)
            {
                if (cmd != null && (cmd.Flags & DbJobCommandFlags.NoCache) == DbJobCommandFlags.None)
                {
                    ColumnMapCacheModel cacheModel = new ColumnMapCacheModel(typeof(List<KeyValuePair<string, object>>), cmd, odr.GetOrdinalColumnNamesHashLite());

                    if (!DbConnectorCache.TryGetColumnMap(cacheModel, out IDynamicColumnMapper mapper))
                    {
                        mapper = DynamicColumnMapper.CreateKeyValuePairsMapper(odr.GetOrdinalColumnNamesLite(cmd.MapSettings), odr, out bool isRead, out List<KeyValuePair<string, object>> mappedRow);

                        DbConnectorCache.SetColumnMap(cacheModel, mapper);

                        if (isRead)
                        {
                            yield return mappedRow;

                            if (isFirstResult)
                            {
                                yield break;
                            }
                        }
                        else
                        {
                            yield break;
                        }
                    }

                    var genericMapper = mapper as DynamicColumnMapper<List<KeyValuePair<string, object>>>;

                    if (isFirstResult)
                    {
                        if (odr.Read())
                        {
                            yield return genericMapper.OnBuild(odr);
                        }
                    }
                    else
                    {
                        while (odr.Read())
                        {
                            if (token.IsCancellationRequested)
                                yield break;

                            yield return genericMapper.OnBuild(odr);
                        }
                    }
                }
                else
                {
                    var ordinalColumnMap = odr.GetOrdinalColumnNamesLite(cmd?.MapSettings);

                    if (ordinalColumnMap?.Length > 0)
                    {
                        if (isFirstResult)
                        {
                            if (odr.Read())
                            {
                                var row = new List<KeyValuePair<string, object>>(ordinalColumnMap.Length);

                                foreach (var map in ordinalColumnMap)
                                {
                                    object data = odr.GetValue(map.Ordinal);
                                    row.Add(new KeyValuePair<string, object>(map.Name, data == DBNull.Value ? null : data));
                                }

                                yield return row;
                            }
                        }
                        else
                        {
                            while (odr.Read())
                            {
                                if (token.IsCancellationRequested)
                                    yield break;

                                var row = new List<KeyValuePair<string, object>>(ordinalColumnMap.Length);

                                foreach (var map in ordinalColumnMap)
                                {
                                    object data = odr.GetValue(map.Ordinal);
                                    row.Add(new KeyValuePair<string, object>(map.Name, data == DBNull.Value ? null : data));
                                }

                                yield return row;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reads data into an <see cref="IEnumerable{Dictionary{string, object}}"/> in a deferred/yielded manner.
        /// </summary>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="isFirstResult">Set to true if only the first row should be loaded.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="cmd">The <see cref="IDbJobCommand"/> to use for data projection and caching. (Optional)</param>
        /// <returns>The <see cref="IEnumerable{Dictionary{string, object}}"/>.</returns>
        public static IEnumerable<Dictionary<string, object>> ToEnumerableDictionaries(
            this DbDataReader odr,
            bool isFirstResult,
            CancellationToken token = default,
            IDbJobCommand cmd = null)
        {
            if (odr.HasRows)
            {
                if (cmd != null && (cmd.Flags & DbJobCommandFlags.NoCache) == DbJobCommandFlags.None)
                {
                    ColumnMapCacheModel cacheModel = new ColumnMapCacheModel(typeof(Dictionary<string, object>), cmd, odr.GetOrdinalColumnNamesHashLite());

                    if (!DbConnectorCache.TryGetColumnMap(cacheModel, out IDynamicColumnMapper mapper))
                    {
                        mapper = DynamicColumnMapper.CreateDictionaryMapper(odr.GetOrdinalColumnNamesLite(cmd.MapSettings), odr, out bool isRead, out Dictionary<string, object> mappedRow);

                        DbConnectorCache.SetColumnMap(cacheModel, mapper);

                        if (isRead)
                        {
                            yield return mappedRow;

                            if (isFirstResult)
                            {
                                yield break;
                            }
                        }
                        else
                        {
                            yield break;
                        }
                    }

                    var genericMapper = mapper as DynamicColumnMapper<Dictionary<string, object>>;

                    if (isFirstResult)
                    {
                        if (odr.Read())
                        {
                            yield return genericMapper.OnBuild(odr);
                        }
                    }
                    else
                    {
                        while (odr.Read())
                        {
                            if (token.IsCancellationRequested)
                                yield break;

                            yield return genericMapper.OnBuild(odr);
                        }
                    }
                }
                else
                {
                    var ordinalColumnMap = odr.GetOrdinalColumnNamesLite(cmd?.MapSettings);

                    if (ordinalColumnMap?.Length > 0)
                    {
                        if (isFirstResult)
                        {
                            if (odr.Read())
                            {
                                var row = new Dictionary<string, object>(ordinalColumnMap.Length);

                                foreach (var map in ordinalColumnMap)
                                {
                                    object data = odr.GetValue(map.Ordinal);
                                    row[map.Name] = data == DBNull.Value ? null : data;
                                }

                                yield return row;
                            }
                        }
                        else
                        {
                            while (odr.Read())
                            {
                                if (token.IsCancellationRequested)
                                    yield break;

                                var row = new Dictionary<string, object>(ordinalColumnMap.Length);

                                foreach (var map in ordinalColumnMap)
                                {
                                    object data = odr.GetValue(map.Ordinal);
                                    row[map.Name] = data == DBNull.Value ? null : data;
                                }

                                yield return row;
                            }
                        }
                    }
                }
            }
        }
    }
}
