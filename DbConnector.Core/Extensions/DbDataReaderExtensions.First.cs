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
using System.Linq;
using System.Threading;

namespace DbConnector.Core.Extensions
{
    public static partial class DbDataReaderExtensions
    {
        internal static T ToFirst<T>(this DbDataReader odr, CancellationToken token = default, IDbJobCommand cmd = null)
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

                    odr.Read();

                    return (mapper as DynamicColumnMapper<T>).OnBuild(odr);
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

                    odr.Read();

                    if (tType.IsClass)
                    {
                        return odr.GetMappedObject<T>(columnMaps);
                    }
                    else
                    {
                        return (T)odr.GetMappedObject(tType, columnMaps);
                    }
                }
            }
            else if (tType == typeof(Dictionary<string, object>))
            {
                var loadedData = odr.ToDictionaries(true, token, cmd).FirstOrDefault();

                if (loadedData != null)
                {
                    return (T)Convert.ChangeType(loadedData, tType);
                }
            }
            else if (tType == typeof(List<KeyValuePair<string, object>>))
            {
                var loadedData = odr.ToKeyValuePairs(true, token, cmd).FirstOrDefault();

                if (loadedData != null)
                {
                    return (T)Convert.ChangeType(loadedData, tType);
                }
            }
            else if (tType == typeof(DataTable))
            {
                return (T)Convert.ChangeType(odr.ToDataTable(true, token, cmd.MapSettings), tType);
            }
            else if (tType == typeof(DataSet))
            {
                return (T)Convert.ChangeType(odr.ToDataSet(true, token, cmd.MapSettings), tType);
            }
            else if (tType == typeof(object))
            {
                return odr.ToFirst(cmd);
            }
            else
            {
                odr.Read();

                object value = odr.GetValue(0);
                Type nonNullableObjType = tType.IsValueType ? (Nullable.GetUnderlyingType(tType) ?? tType) : tType;

                if (value != DBNull.Value)
                {
                    return (T)(DbConnectorUtilities.ThrowIfFailedToMatchColumnType(tType, nonNullableObjType, value, () => odr.GetName(0)));
                }
            }

            return default;
        }

        internal static object ToFirst(this DbDataReader odr, Type objType, CancellationToken token = default, IDbJobCommand cmd = null)
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

                    odr.Read();

                    return mapper.Build(odr);
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

                    IColumnMapSetting settings = cmd?.MapSettings;

                    var columnMaps = odr.GetColumnMaps(objType, settings);

                    odr.Read();

                    return odr.GetMappedObject(objType, columnMaps);
                }
            }
            else if (objType == typeof(DataTable))
            {
                return Convert.ChangeType(odr.ToDataTable(true, token, cmd.MapSettings), objType);
            }
            else if (objType == typeof(DataSet))
            {
                return Convert.ChangeType(odr.ToDataSet(true, token, cmd.MapSettings), objType);
            }
            else if (objType == typeof(List<KeyValuePair<string, object>>))
            {
                var loadedData = odr.ToKeyValuePairs(true, token, cmd).FirstOrDefault();

                if (loadedData != null)
                {
                    return Convert.ChangeType(loadedData, objType);
                }
            }
            else if (objType == typeof(Dictionary<string, object>))
            {
                var loadedData = odr.ToDictionaries(true, token, cmd).FirstOrDefault();

                if (loadedData != null)
                {
                    return Convert.ChangeType(loadedData, objType);
                }
            }
            else
            {
                odr.Read();

                object value = odr.GetValue(0);
                Type nonNullableObjType = objType.IsValueType ? (Nullable.GetUnderlyingType(objType) ?? objType) : objType;

                if (value != DBNull.Value)
                {
                    return DbConnectorUtilities.ThrowIfFailedToMatchColumnType(objType, nonNullableObjType, value, () => odr.GetName(0));
                }
            }

            return objType.IsValueType ? Activator.CreateInstance(objType) : null;
        }

        internal static dynamic ToFirst(this DbDataReader odr, IEnumerable<OrdinalColumnMapLite> ordinalColumnMap)
        {
            dynamic row = new ExpandoObject();

            var expandoDict = row as IDictionary<string, object>;

            foreach (var map in ordinalColumnMap)
            {
                object data = odr.GetValue(map.Ordinal);
                expandoDict[map.Name] = data == DBNull.Value ? null : data;
            }

            return row;
        }

        internal static dynamic ToFirst(this DbDataReader odr, IColumnMapSetting settings = null)
        {
            dynamic row = new ExpandoObject();

            var expandoDict = row as IDictionary<string, object>;

            if (settings == null || (!settings.HasNamesToInclude && !settings.HasNamesToExclude))
            {
                for (int i = 0; i < odr.FieldCount; i++)
                {
                    object data = odr.GetValue(i);
                    expandoDict[odr.GetName(i)] = data == DBNull.Value ? null : data;
                }
            }
            else
            {
                bool hasNamesToInclude = settings.HasNamesToInclude;
                bool hasNamesToExclude = settings.HasNamesToExclude;

                for (int i = 0; i < odr.FieldCount; i++)
                {
                    string colName = odr.GetName(i);

                    if (!DbConnectorUtilities.IsColumnNameExcluded(colName, hasNamesToInclude, hasNamesToExclude, settings))
                    {
                        object data = odr.GetValue(i);
                        expandoDict[colName] = data == DBNull.Value ? null : data;
                    }

                }
            }

            return row;
        }

        internal static dynamic ToFirst(this DbDataReader odr, IDbJobCommand cmd = null)
        {
            if (cmd != null && (cmd.Flags & DbJobCommandFlags.NoCache) == DbJobCommandFlags.None)
            {
                ColumnMapCacheModel cacheModel = new ColumnMapCacheModel(typeof(ExpandoObject), cmd, odr.GetOrdinalColumnNamesHashLite());

                if (!DbConnectorCache.TryGetColumnMap(cacheModel, out IDynamicColumnMapper mapper))
                {
                    mapper = DynamicColumnMapper.CreateExpandoObjectMapper(odr.GetOrdinalColumnNamesLite(cmd.MapSettings), odr, out bool isRead, out ExpandoObject mappedRow);

                    DbConnectorCache.SetColumnMap(cacheModel, mapper);

                    return isRead ? mappedRow : null;
                }

                odr.Read();

                return mapper.Build(odr);
            }
            else
            {
                odr.Read();

                return odr.ToFirst(cmd?.MapSettings);
            }
        }

        /// <summary>
        /// Reads the first row data into an object of <typeparamref name="T"/> type.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The generic type to use.</typeparam>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="cmd">The <see cref="IDbJobCommand"/> to use for data projection and caching. (Optional)</param>
        /// <returns>The <typeparamref name="T"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public static T First<T>(this DbDataReader odr, CancellationToken token = default, IDbJobCommand cmd = null)
        {
            if (odr.HasRows)
            {
                return odr.ToFirst<T>(token, cmd);
            }
            else
            {
                throw new InvalidOperationException("The query result is empty.");
            }
        }

        /// <summary>
        /// Reads the first row data into an object.
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="objType">The <see cref="Type"/> to use.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="cmd">The <see cref="IDbJobCommand"/> to use for data projection and caching. (Optional)</param>
        /// <returns>The object.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when <paramref name="objType"/> is missing a parameterless constructor.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when <paramref name="objType"/> is assignable from <see cref="System.Collections.IEnumerable"/>.</exception>
        public static object First(this DbDataReader odr, Type objType, CancellationToken token = default, IDbJobCommand cmd = null)
        {
            if (odr.HasRows)
            {
                return odr.ToFirst(objType, token, cmd);
            }
            else
            {
                throw new InvalidOperationException("The query result is empty.");
            }
        }

        /// <summary>
        /// Reads the first row data into a dynamic object.        
        /// </summary>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="cmd">The <see cref="IDbJobCommand"/> to use for data projection and caching. (Optional)</param>
        /// <returns>The dynamic object.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public static dynamic First(this DbDataReader odr, CancellationToken token = default, IDbJobCommand cmd = null)
        {
            if (odr.HasRows)
            {
                return odr.ToFirst(cmd);
            }
            else
            {
                throw new InvalidOperationException("The query result is empty.");
            }
        }

        /// <summary>
        /// Reads the first row data into an object of <typeparamref name="T"/> type.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The generic type to use.</typeparam>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="cmd">The <see cref="IDbJobCommand"/> to use for data projection and caching. (Optional)</param>
        /// <returns>The <typeparamref name="T"/>.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> is missing a parameterless constructor.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> is assignable from <see cref="System.Collections.IEnumerable"/>.</exception>
        public static T FirstOrDefault<T>(this DbDataReader odr, CancellationToken token = default, IDbJobCommand cmd = null)
        {
            if (odr.HasRows)
            {
                return odr.ToFirst<T>(token, cmd);
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Reads the first row data into an object.
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="objType">The <see cref="Type"/> to use.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="cmd">The <see cref="IDbJobCommand"/> to use for data projection and caching. (Optional)</param>
        /// <returns>The object.</returns>
        public static object FirstOrDefault(this DbDataReader odr, Type objType, CancellationToken token = default, IDbJobCommand cmd = null)
        {
            if (odr.HasRows)
            {
                return odr.ToFirst(objType, token, cmd);
            }
            else
            {
                if (objType.IsValueType)
                {
                    return Activator.CreateInstance(objType);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Reads the first row data into a dynamic object.      
        /// </summary>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="cmd">The <see cref="IDbJobCommand"/> to use for data projection and caching. (Optional)</param>
        /// <returns>The dynamic object.</returns>
        public static dynamic FirstOrDefault(this DbDataReader odr, CancellationToken token = default, IDbJobCommand cmd = null)
        {
            if (odr.HasRows)
            {
                return odr.ToFirst(cmd);
            }
            else
            {
                return null;
            }
        }
    }
}
