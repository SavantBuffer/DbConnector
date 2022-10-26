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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DbConnector.Core.Extensions
{
    /// <summary>
    /// Extends <see cref="DbDataReader"/> with data mapping functions.
    /// </summary>
    public static partial class DbDataReaderExtensions
    {
        /// <summary>
        /// Creates an <see cref="IEnumerable{ColumnMap}"/> based on the type of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to use.</typeparam>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="settings">The <see cref="IColumnMapSetting"/> to use. (Optional)</param>
        /// <returns>The <see cref="IEnumerable{ColumnMap}"/>.</returns>
        public static IEnumerable<ColumnMap> GetColumnMaps<T>(this DbDataReader odr, IColumnMapSetting settings = null)
        {
            return DbConnectorUtilities.GetColumnMaps(typeof(T), odr.GetOrdinalColumnNames(settings), settings);
        }

        /// <summary>
        /// Creates an <see cref="IEnumerable{ColumnMap}"/> based on the provided type.
        /// </summary>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="objType">The <see cref="Type"/> to use.</param>
        /// <param name="settings">The <see cref="IColumnMapSetting"/> to use. (Optional)</param>
        /// <returns>The <see cref="IEnumerable{ColumnMap}"/>.</returns>
        public static IEnumerable<ColumnMap> GetColumnMaps(this DbDataReader odr, Type objType, IColumnMapSetting settings = null)
        {
            return DbConnectorUtilities.GetColumnMaps(objType, odr.GetOrdinalColumnNames(settings), settings);
        }



        public static T GetMappedObject<T>(this DbDataReader odr, IColumnMapSetting settings = null)
        {
            var columnMaps = DbConnectorUtilities.GetColumnMaps(typeof(T), odr.GetOrdinalColumnNames(settings), settings);

            return odr.GetMappedObject<T>(columnMaps);
        }

        public static T GetMappedObject<T>(this DbDataReader odr, IEnumerable<ColumnMap> columnMaps)
        {
            T obj = Activator.CreateInstance<T>();

            foreach (var map in columnMaps)
            {
                if (map.IsChildMap)
                {
                    odr.SetParentProperties(obj, map.ParentMap);
                }
                else
                {
                    DbConnectorUtilities.SetChildProperty(obj, map, odr.GetValue(map.Column.Ordinal));
                }
            }

            return obj;
        }

        public static object GetMappedObject(this DbDataReader odr, Type objType, IColumnMapSetting settings = null)
        {
            var columnMaps = DbConnectorUtilities.GetColumnMaps(objType, odr.GetOrdinalColumnNames(settings), settings);

            return odr.GetMappedObject(objType, columnMaps);
        }

        public static object GetMappedObject(this DbDataReader odr, Type objType, IEnumerable<ColumnMap> columnMaps)
        {
            object obj = Activator.CreateInstance(objType);

            foreach (var map in columnMaps)
            {
                if (map.IsChildMap)
                {
                    odr.SetParentProperties(obj, map.ParentMap);
                }
                else
                {
                    DbConnectorUtilities.SetChildProperty(obj, map, odr.GetValue(map.Column.Ordinal));
                }
            }

            return obj;
        }

        private static object GetMappedParentObject(this DbDataReader odr, object obj, IEnumerable<ColumnMap> columnMaps)
        {
            foreach (var map in columnMaps)
            {
                if (map.IsChildMap)
                {
                    odr.SetParentProperties(obj, map.ParentMap);
                }
                else
                {
                    DbConnectorUtilities.SetChildProperty(obj, map, odr.GetValue(map.Column.Ordinal));
                }
            }

            return obj;
        }

        private static void SetParentProperties(this DbDataReader odr, object obj, ColumnParentMap childMap)
        {
            PropertyInfo pInfo = obj.GetType().GetProperty(childMap.PropInfo.Name);

            if (pInfo != null && pInfo.CanWrite && pInfo.ReflectedType.FullName == childMap.PropInfo.ReflectedType.FullName)
            {
                object childObj = pInfo.CanRead && pInfo.PropertyType.IsClass ? pInfo.GetValue(obj) : null;

                if (childObj == null)
                {
                    childObj = Activator.CreateInstance(pInfo.PropertyType);
                }

                odr.GetMappedParentObject(childObj, childMap.Children);

                childMap.SetMethod.Invoke(obj, new object[] { childObj });
            }
        }



        /// <summary>
        /// Reads data into a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="isFirstResult">Set to true if only the first row should be loaded.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="settings">The <see cref="IColumnMapSetting"/> to use. (Optional)</param>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public static DataTable ToDataTable(this DbDataReader odr, bool isFirstResult, CancellationToken token, IColumnMapSetting settings)
        {
            var dt = new DataTable();

            if (odr.HasRows)
            {
                if (isFirstResult || (settings != null && (settings.HasNamesToInclude || settings.HasNamesToExclude)))
                {
                    var ordinalColumnMap = odr.GetOrdinalColumnNamesLite(settings);

                    if (ordinalColumnMap?.Length > 0)
                    {
                        foreach (var m in ordinalColumnMap)
                        {
                            dt.Columns.Add(m.Name);
                        }

                        if (isFirstResult)
                        {
                            if (odr.Read())
                            {
                                DataRow row = dt.NewRow();

                                for (int i = 0; i < ordinalColumnMap.Length; i++)
                                {
                                    row[i] = odr.GetValue(ordinalColumnMap[i].Ordinal);
                                }

                                dt.Rows.Add(row);
                            }
                        }
                        else
                        {
                            while (odr.Read())
                            {
                                if (token.IsCancellationRequested)
                                    break;

                                DataRow row = dt.NewRow();

                                for (int i = 0; i < ordinalColumnMap.Length; i++)
                                {
                                    row[i] = odr.GetValue(ordinalColumnMap[i].Ordinal);
                                }

                                dt.Rows.Add(row);
                            }
                        }
                    }
                }
                else
                {
                    dt.Load(odr);
                }
            }

            return dt;
        }

        /// <summary>
        /// Reads data into a <see cref="DataTable"/> asynchronously.
        /// </summary>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="isFirstResult">Set to true if only the first row should be loaded.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="settings">The <see cref="IColumnMapSetting"/> to use. (Optional)</param>
        /// <returns>The <see cref="DataTable"/> inside a <see cref="Task"/>.</returns>
        public async static Task<DataTable> ToDataTableAsync(this DbDataReader odr, bool isFirstResult, CancellationToken token, IColumnMapSetting settings)
        {
            var dt = new DataTable();

            if (odr.HasRows)
            {
                if (isFirstResult || (settings != null && (settings.HasNamesToInclude || settings.HasNamesToExclude)))
                {
                    var ordinalColumnMap = odr.GetOrdinalColumnNamesLite(settings);

                    if (ordinalColumnMap?.Length > 0)
                    {
                        foreach (var m in ordinalColumnMap)
                        {
                            dt.Columns.Add(m.Name);
                        }

                        if (isFirstResult)
                        {
                            if (await odr.ReadAsync(token))
                            {
                                DataRow row = dt.NewRow();

                                for (int i = 0; i < ordinalColumnMap.Length; i++)
                                {
                                    row[i] = odr.GetValue(ordinalColumnMap[i].Ordinal);
                                }

                                dt.Rows.Add(row);
                            }
                        }
                        else
                        {
                            while (await odr.ReadAsync(token))
                            {
                                if (token.IsCancellationRequested)
                                    break;

                                DataRow row = dt.NewRow();

                                for (int i = 0; i < ordinalColumnMap.Length; i++)
                                {
                                    row[i] = odr.GetValue(ordinalColumnMap[i].Ordinal);
                                }

                                dt.Rows.Add(row);
                            }
                        }
                    }
                }
                else
                {
                    dt.Load(odr);
                }
            }

            return dt;
        }

        /// <summary>
        /// Reads data into a <see cref="DataSet"/>.
        /// </summary>
        /// <param name="odr">The <see cref="DataSet"/> to use.</param>
        /// <param name="isFirstResult">Set to true if only the first item result should be loaded.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="settings">The <see cref="IColumnMapSetting"/> to use. (Optional)</param>
        /// <returns>The <see cref="DataSet"/>.</returns>
        public static DataSet ToDataSet(this DbDataReader odr, bool isFirstResult, CancellationToken token, IColumnMapSetting settings, DataSet projectedDataSet = null)
        {
            projectedDataSet = projectedDataSet ?? new DataSet();

            bool hasNext = true;

            while ((hasNext && odr.HasRows) || odr.NextResult())
            {
                if (token.IsCancellationRequested)
                    return projectedDataSet;

                projectedDataSet.Tables.Add(odr.ToDataTable(isFirstResult, token, settings));

                hasNext = odr.NextResult();
            }

            return projectedDataSet;
        }

        /// <summary>
        /// Reads data into a <see cref="DataSet"/> asynchronously.
        /// </summary>
        /// <param name="odr">The <see cref="DataSet"/> to use.</param>
        /// <param name="isFirstResult">Set to true if only the first item result should be loaded.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="settings">The <see cref="IColumnMapSetting"/> to use. (Optional)</param>
        /// <returns>The <see cref="DataSet"/> inside a <see cref="Task"/>.</returns>
        public async static Task<DataSet> ToDataSetAsync(this DbDataReader odr, bool isFirstResult, CancellationToken token, IColumnMapSetting settings, DataSet projectedDataSet = null)
        {
            projectedDataSet = projectedDataSet ?? new DataSet();

            bool hasNext = true;

            while ((hasNext && odr.HasRows) || await odr.NextResultAsync(token))
            {
                if (token.IsCancellationRequested)
                    return projectedDataSet;

                projectedDataSet.Tables.Add(await odr.ToDataTableAsync(isFirstResult, token, settings));

                hasNext = await odr.NextResultAsync(token);
            }

            return projectedDataSet;
        }

        internal static int GetOrdinalColumnNamesHash(this DbDataReader odr)
        {
            int hash = 0;

            unchecked
            {
                for (int i = 0; i < odr.FieldCount; i++)
                {
                    hash += (i + (odr.GetName(i)?.GetHashCode() ?? 0) + (odr.GetFieldType(i)?.GetHashCode() ?? 0));
                }
            }

            return hash;
        }

        internal static int GetOrdinalColumnNamesHashLite(this DbDataReader odr)
        {
            int hash = 0;

            unchecked
            {
                for (int i = 0; i < odr.FieldCount; i++)
                {
                    hash += (i + (odr.GetName(i)?.GetHashCode() ?? 0));
                }
            }

            return hash;
        }

        internal static OrdinalColumnMap[] GetOrdinalColumnNames(this DbDataReader odr, IColumnMapSetting settings = null)
        {
            if (settings == null || (!settings.HasNamesToInclude && !settings.HasNamesToExclude))
            {
                var ordinalColumnMap = new OrdinalColumnMap[odr.FieldCount];

                for (int i = 0; i < odr.FieldCount; i++)
                {
                    ordinalColumnMap[i] = new OrdinalColumnMap { Ordinal = i, Name = odr.GetName(i), FieldType = odr.GetFieldType(i) };
                }

                return ordinalColumnMap;
            }
            else
            {
                bool hasNamesToInclude = settings.HasNamesToInclude;
                bool hasNamesToExclude = settings.HasNamesToExclude;

                var tempMap = new Queue<OrdinalColumnMap>(odr.FieldCount);

                for (int i = 0; i < odr.FieldCount; i++)
                {
                    string colName = odr.GetName(i);

                    if (!DbConnectorUtilities.IsColumnNameExcluded(colName, hasNamesToInclude, hasNamesToExclude, settings))
                    {
                        tempMap.Enqueue(new OrdinalColumnMap { Ordinal = i, Name = colName, FieldType = odr.GetFieldType(i) });
                    }
                }

                if (tempMap.Count > 0)
                {
                    var ordinalColumnMap = new OrdinalColumnMap[tempMap.Count];

                    for (int i = 0; tempMap.Count > 0; i++)
                    {
                        ordinalColumnMap[i] = tempMap.Dequeue();
                    }

                    return ordinalColumnMap;
                }
                else
                {
                    return null;
                }
            }
        }

        internal static OrdinalColumnMapLite[] GetOrdinalColumnNamesLite(this DbDataReader odr, IColumnMapSetting settings = null)
        {
            if (settings == null || (!settings.HasNamesToInclude && !settings.HasNamesToExclude))
            {
                var ordinalColumnMap = new OrdinalColumnMapLite[odr.FieldCount];

                for (int i = 0; i < odr.FieldCount; i++)
                {
                    ordinalColumnMap[i] = new OrdinalColumnMapLite { Ordinal = i, Name = odr.GetName(i) };
                }

                return ordinalColumnMap;
            }
            else
            {
                bool hasNamesToInclude = settings.HasNamesToInclude;
                bool hasNamesToExclude = settings.HasNamesToExclude;

                var tempMap = new Queue<OrdinalColumnMapLite>(odr.FieldCount);

                for (int i = 0; i < odr.FieldCount; i++)
                {
                    string colName = odr.GetName(i);

                    if (!DbConnectorUtilities.IsColumnNameExcluded(colName, hasNamesToInclude, hasNamesToExclude, settings))
                    {
                        tempMap.Enqueue(new OrdinalColumnMapLite { Ordinal = i, Name = colName });
                    }
                }

                if (tempMap.Count > 0)
                {
                    var ordinalColumnMap = new OrdinalColumnMapLite[tempMap.Count];

                    for (int i = 0; tempMap.Count > 0; i++)
                    {
                        ordinalColumnMap[i] = tempMap.Dequeue();
                    }

                    return ordinalColumnMap;
                }
                else
                {
                    return null;
                }
            }
        }

        public static IEnumerable<string> GetColumnNames(this DbDataReader odr)
        {
            for (int i = 0; i < odr.FieldCount; i++)
            {
                yield return odr.GetName(i);
            }
        }

        public static IEnumerable<Type> GetFieldTypes(this DbDataReader odr)
        {
            for (int i = 0; i < odr.FieldCount; i++)
            {
                yield return odr.GetFieldType(i);
            }
        }
    }
}
