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
using System.Data.Common;
using System.Threading;

namespace DbConnector.Core.Extensions
{
    public static partial class DbDataReaderExtensions
    {
        /// <summary>
        ///  Reads the first column of each row from the query result into a <see cref="HashSet{T}"/>. All other columns are ignored.
        ///  <para>Valid <typeparamref name="T"/> types: Any .NET built-in type or ADO.NET data provider supported type.</para>
        /// </summary>
        /// <remarks>
        /// Note: <see cref="DBNull"/> values will be excluded.
        /// </remarks>
        /// <typeparam name="T">The generic type to use.</typeparam>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="HashSet{T}"/>.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> is not supported.</exception>
        public static HashSet<T> ToHashSet<T>(this DbDataReader odr, CancellationToken token = default)
        {
            HashSet<T> projectedData = new HashSet<T>();

            if (odr.HasRows)
            {
                Type tType = typeof(T);
                Type nonNullableObjType = tType.IsValueType ? (Nullable.GetUnderlyingType(tType) ?? tType) : tType;

                while (odr.Read())
                {
                    if (token.IsCancellationRequested)
                        return projectedData;

                    object value = odr.GetValue(0);

                    if (value != DBNull.Value)
                    {
                        projectedData.Add((T)(DbConnectorUtilities.ThrowIfFailedToMatchColumnType(tType, nonNullableObjType, value, () => odr.GetName(0))));
                    }
                }
            }

            return projectedData;
        }

        /// <summary>
        ///  Reads the first column of each row from the query result into a <see cref="HashSet{object}"/>. All other columns are ignored.
        /// </summary>
        /// <remarks>
        /// Note: <see cref="DBNull"/> values will be excluded.
        /// </remarks>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="HashSet{object}"/>.</returns>
        public static HashSet<object> ToHashSet(this DbDataReader odr, CancellationToken token = default)
        {
            HashSet<object> projectedData = new HashSet<object>();

            if (odr.HasRows)
            {
                while (odr.Read())
                {
                    if (token.IsCancellationRequested)
                        return projectedData;

                    object value = odr.GetValue(0);

                    if (value != DBNull.Value)
                        projectedData.Add(value);
                }
            }

            return projectedData;
        }
    }
}
