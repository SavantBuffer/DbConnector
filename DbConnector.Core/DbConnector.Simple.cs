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

using DbConnector.Core.Extensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DbConnector.Core
{
    public partial class DbConnector<TDbConnection> : IDbConnector<TDbConnection>
       where TDbConnection : DbConnection
    {
        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only the first row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<T> ReadFirstOrDefault<T>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            ColumnMapSetting mapSettings = null,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType, mapSettings, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadFirstOrDefault(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only the first row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<T> ReadFirstOrDefault<T>(string sql, object param)
        {
            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param),
                    onExecute: (d, p) => OnExecuteReadFirstOrDefault(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only the first row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param>         
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<T> ReadFirstOrDefault<T>(string sql)
        {
            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql),
                    onExecute: (d, p) => OnExecuteReadFirstOrDefault(d, p)
                );
        }

    }
}
