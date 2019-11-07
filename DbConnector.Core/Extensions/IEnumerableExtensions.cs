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

namespace DbConnector.Core.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Creates a <see cref="DataTable"/> from an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The element type of the enumerable.</typeparam>
        /// <param name="data">The <see cref="System.Collections.Generic.IEnumerable{T}"/> data.</param>
        /// <param name="isUseColumnAttribute">Set to false to not use the <see cref="System.ComponentModel.DataAnnotations.Schema.ColumnAttribute"/> for names. (Optional)</param>
        /// <param name="tableName">The table name to use for the <see cref="System.Data.DataTable"/>. (Optional)</param>
        /// <returns>The <see cref="DataTable"/> created from the data.</returns>        
        public static DataTable ToDataTable<T>(this IEnumerable<T> data, bool isUseColumnAttribute = true, string tableName = null)
            where T : new()
        {
            DataTable dt = string.IsNullOrWhiteSpace(tableName) ? new DataTable() : new DataTable(tableName);


            T obj = default;
            Type tType = obj == null ? typeof(T) : obj.GetType();


            var props = tType.GetProperties(DbConnectorUtilities._bindingFlagInstancePublic);


            if (isUseColumnAttribute)
            {
                foreach (var item in props)
                {
                    dt.Columns.Add(item.GetColumnAttributeName(), item.PropertyType);
                }
            }
            else
            {
                foreach (var item in props)
                {
                    dt.Columns.Add(item.Name, item.PropertyType);
                }
            }


            if (dt.Columns.Count > 0)
                foreach (T item in data)
                {
                    DataRow row = dt.NewRow();

                    for (int i = 0; i < props.Length; i++)
                    {
                        row[i] = props[i].GetValue(item);
                    }

                    dt.Rows.Add(row);
                }


            return dt;
        }
    }
}
