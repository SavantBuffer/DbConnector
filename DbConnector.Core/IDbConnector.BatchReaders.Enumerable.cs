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
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>       
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}}}"/>.</returns>
        IDbJob<(IEnumerable<T1>, IEnumerable<T2>)> Read<T1, T2>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>    
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}}}"/>.</returns>
        IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> Read<T1, T2, T3>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>          
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}}}"/>.</returns>
        IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>)> Read<T1, T2, T3, T4>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}, IEnumerable{T5}}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>                 
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}, IEnumerable{T5}}}"/>.</returns>
        IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>)> Read<T1, T2, T3, T4, T5>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}, IEnumerable{T5}, IEnumerable{T6}}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>                        
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}, IEnumerable{T5}, IEnumerable{T6}}}"/>.</returns>
        IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>)> Read<T1, T2, T3, T4, T5, T6>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}, IEnumerable{T5}, IEnumerable{T6}, IEnumerable{T7}}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>                        
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}, IEnumerable{T5}, IEnumerable{T6}, IEnumerable{T7}}}"/>.</returns>
        IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>, IEnumerable<T7>)> Read<T1, T2, T3, T4, T5, T6, T7>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}, IEnumerable{T5}, IEnumerable{T6}, IEnumerable{T7}, IEnumerable{T8}}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>                        
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}, IEnumerable{T5}, IEnumerable{T6}, IEnumerable{T7}, IEnumerable{T8}}}"/>.</returns>
        IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>, IEnumerable<T7>, IEnumerable<T8>)> Read<T1, T2, T3, T4, T5, T6, T7, T8>(Action<IDbJobCommand> onInit);
    }
}
