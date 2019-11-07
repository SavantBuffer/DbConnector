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
using System.Dynamic;
using System.Linq;
using System.Threading;

namespace DbConnector.Core.Extensions
{
    public static partial class DbDataReaderExtensions
    {
        /// <summary>
        /// Reads a single row data into an object of <typeparamref name="T"/> type.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The generic type to use.</typeparam>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="cmd">The <see cref="IDbJobCommand"/> to use for data projection and caching. (Optional)</param>
        /// <returns>The <typeparamref name="T"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> is missing a parameterless constructor.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> is assignable from <see cref="IEnumerable"/>.</exception>
        public static T Single<T>(this DbDataReader odr, CancellationToken token = default, IDbJobCommand cmd = null)
        {
            if (odr.HasRows)
            {
                T projectedData = odr.ToFirst<T>(token, cmd);

                if (odr.Read())
                {
                    throw new InvalidOperationException("The query result has more than one result.");
                }

                return projectedData;
            }
            else
            {
                throw new InvalidOperationException("The query result is empty.");
            }
        }

        /// <summary>
        /// Reads a single row data into an object.
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="objType">The <see cref="Type"/> to use.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="cmd">The <see cref="IDbJobCommand"/> to use for data projection and caching. (Optional)</param>
        /// <returns>The object.</returns>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when <paramref name="objType"/> is missing a parameterless constructor.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when <paramref name="objType"/> is assignable from <see cref="IEnumerable"/>.</exception>
        public static object Single(this DbDataReader odr, Type objType, CancellationToken token = default, IDbJobCommand cmd = null)
        {
            if (odr.HasRows)
            {
                object projectedData = odr.ToFirst(objType, token, cmd);

                if (odr.Read())
                {
                    throw new InvalidOperationException("The query result has more than one result.");
                }

                return projectedData;
            }
            else
            {
                throw new InvalidOperationException("The query result is empty.");
            }
        }

        /// <summary>
        /// Reads a single row data into a dynamic object.       
        /// </summary>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="cmd">The <see cref="IDbJobCommand"/> to use for data projection and caching. (Optional)</param>
        /// <returns>The dynamic object.</returns>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public static dynamic Single(this DbDataReader odr, CancellationToken token = default, IDbJobCommand cmd = null)
        {
            dynamic projectedData;

            if (odr.HasRows)
            {
                projectedData = odr.ToFirst(cmd);

                if (odr.Read())
                {
                    throw new InvalidOperationException("The query result has more than one result.");
                }
            }
            else
            {
                throw new InvalidOperationException("The query result is empty.");
            }

            return projectedData;
        }

        /// <summary>
        /// Reads a single row data into an object of <typeparamref name="T"/> type.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The generic type to use.</typeparam>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="cmd">The <see cref="IDbJobCommand"/> to use for data projection and caching. (Optional)</param>
        /// <returns>The <typeparamref name="T"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> is missing a parameterless constructor.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> is assignable from <see cref="IEnumerable"/>.</exception>
        public static T SingleOrDefault<T>(this DbDataReader odr, CancellationToken token = default, IDbJobCommand cmd = null)
        {
            if (odr.HasRows)
            {
                T projectedData = odr.ToFirst<T>(token, cmd);

                if (odr.Read())
                {
                    throw new InvalidOperationException("The query result has more than one result.");
                }

                return projectedData;
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Reads a single row data into an object.
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="objType">The <see cref="Type"/> to use.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="cmd">The <see cref="IDbJobCommand"/> to use for data projection and caching. (Optional)</param>
        /// <returns>The object.</returns>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when <paramref name="objType"/> is missing a parameterless constructor.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when <paramref name="objType"/> is assignable from <see cref="IEnumerable"/>.</exception>
        public static object SingleOrDefault(this DbDataReader odr, Type objType, CancellationToken token = default, IDbJobCommand cmd = null)
        {
            if (odr.HasRows)
            {
                object projectedData = odr.ToFirst(objType, token, cmd);

                if (odr.Read())
                {
                    throw new InvalidOperationException("The query result has more than one result.");
                }

                return projectedData;
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
        /// Reads a single row data into a dynamic object.        
        /// </summary>
        /// <param name="odr">The <see cref="DbDataReader"/> to use.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="cmd">The <see cref="IDbJobCommand"/> to use for data projection and caching. (Optional)</param>
        /// <returns>The dynamic object.</returns>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public static dynamic SingleOrDefault(this DbDataReader odr, CancellationToken token = default, IDbJobCommand cmd = null)
        {
            dynamic projectedData = null;

            if (odr.HasRows)
            {
                projectedData = odr.ToFirst(cmd);

                if (odr.Read())
                {
                    throw new InvalidOperationException("The query result has more than one result.");
                }
            }

            return projectedData;
        }
    }
}
