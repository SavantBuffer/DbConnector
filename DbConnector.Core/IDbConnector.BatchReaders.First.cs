﻿//Copyright 2019 Robert Orama

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
using System.Data;
using System.Data.Common;

namespace DbConnector.Core
{
    public partial interface IDbConnector
    {
        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        IDbJob<(T1, T2)> ReadFirst<T1, T2>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        IDbJob<(T1, T2, T3)> ReadFirst<T1, T2, T3>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        IDbJob<(T1, T2, T3, T4)> ReadFirst<T1, T2, T3, T4>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        IDbJob<(T1, T2, T3, T4, T5)> ReadFirst<T1, T2, T3, T4, T5>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        IDbJob<(T1, T2, T3, T4, T5, T6)> ReadFirst<T1, T2, T3, T4, T5, T6>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        IDbJob<(T1, T2, T3, T4, T5, T6, T7)> ReadFirst<T1, T2, T3, T4, T5, T6, T7>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        IDbJob<(T1, T2, T3, T4, T5, T6, T7, T8)> ReadFirst<T1, T2, T3, T4, T5, T6, T7, T8>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<(T1, T2)> ReadFirstOrDefault<T1, T2>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<(T1, T2, T3)> ReadFirstOrDefault<T1, T2, T3>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<(T1, T2, T3, T4)> ReadFirstOrDefault<T1, T2, T3, T4>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<(T1, T2, T3, T4, T5)> ReadFirstOrDefault<T1, T2, T3, T4, T5>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<(T1, T2, T3, T4, T5, T6)> ReadFirstOrDefault<T1, T2, T3, T4, T5, T6>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<(T1, T2, T3, T4, T5, T6, T7)> ReadFirstOrDefault<T1, T2, T3, T4, T5, T6, T7>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<(T1, T2, T3, T4, T5, T6, T7, T8)> ReadFirstOrDefault<T1, T2, T3, T4, T5, T6, T7, T8>(Action<IDbJobCommand> onInit);
    }
}
