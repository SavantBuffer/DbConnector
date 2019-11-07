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

namespace DbConnector.Core
{
    public partial interface IDbConnector<TDbConnection>
       where TDbConnection : DbConnection
    {
        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{List{T1}, List{T2}}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>       
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param>      
        /// <returns>The <see cref="IDbJob{ValueTuple{List{T1}, List{T2}}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<(List<T1>, List<T2>)> ReadToList<T1, T2>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null);

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{List{T1}, List{T2}, List{T3}}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>       
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{List{T1}, List{T2}, List{T3}}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<(List<T1>, List<T2>, List<T3>)> ReadToList<T1, T2, T3>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null);

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{List{T1}, List{T2}, List{T3}, List{T4}}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>       
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{List{T1}, List{T2}, List{T3}, List{T4}}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<(List<T1>, List<T2>, List<T3>, List<T4>)> ReadToList<T1, T2, T3, T4>(
           Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null);

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{List{T1}, List{T2}, List{T3}, List{T4}, List{T5}}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>       
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{List{T1}, List{T2}, List{T3}, List{T4}, List{T5}}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<(List<T1>, List<T2>, List<T3>, List<T4>, List<T5>)> ReadToList<T1, T2, T3, T4, T5>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null);

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{List{T1}, List{T2}, List{T3}, List{T4}, List{T5}, List{T6}}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>       
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{List{T1}, List{T2}, List{T3}, List{T4}, List{T5}, List{T6}}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<(List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>)> ReadToList<T1, T2, T3, T4, T5, T6>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null);

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{List{T1}, List{T2}, List{T3}, List{T4}, List{T5}, List{T6}, List{T7}}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>       
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{List{T1}, List{T2}, List{T3}, List{T4}, List{T5}, List{T6}, List{T7}}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<(List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>)> ReadToList<T1, T2, T3, T4, T5, T6, T7>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null);

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{List{T1}, List{T2}, List{T3}, List{T4}, List{T5}, List{T6}, List{T7}, List{T8}}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>       
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{List{T1}, List{T2}, List{T3}, List{T4}, List{T5}, List{T6}, List{T7}, List{T8}}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<(List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>, List<T8>)> ReadToList<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null);
    }
}
