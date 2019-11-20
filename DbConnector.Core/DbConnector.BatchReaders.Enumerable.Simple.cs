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
using System.Linq;

namespace DbConnector.Core
{
    public partial class DbConnector<TDbConnection> : IDbConnector<TDbConnection>
       where TDbConnection : DbConnection
    {
        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>, IEnumerable<T7>, IEnumerable<T8>)> Read<T1, T2, T3, T4, T5, T6, T7, T8>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>, IEnumerable<T7>, IEnumerable<T8>), TDbConnection>
              (
                  setting: _jobSetting,
                  state: new DbConnectorSimpleState { Flags = _flags },
                  onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                  onExecute: (d, p) => OnExecuteRead(d, p)
              ).SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>(), Enumerable.Empty<T5>(), Enumerable.Empty<T6>(), Enumerable.Empty<T7>(), Enumerable.Empty<T8>()));
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>, IEnumerable<T7>, IEnumerable<T8>)> Read<T1, T2, T3, T4, T5, T6, T7, T8>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>, IEnumerable<T7>, IEnumerable<T8>), TDbConnection>
              (
                  setting: _jobSetting,
                  state: new DbConnectorSimpleState { Flags = _flags },
                  onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                  onExecute: (d, p) => OnExecuteRead(d, p)
              ).SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>(), Enumerable.Empty<T5>(), Enumerable.Empty<T6>(), Enumerable.Empty<T7>(), Enumerable.Empty<T8>()));
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>, IEnumerable<T7>)> Read<T1, T2, T3, T4, T5, T6, T7>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>, IEnumerable<T7>), TDbConnection>
              (
                  setting: _jobSetting,
                  state: new DbConnectorSimpleState { Flags = _flags },
                  onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                  onExecute: (d, p) => OnExecuteRead(d, p)
              ).SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>(), Enumerable.Empty<T5>(), Enumerable.Empty<T6>(), Enumerable.Empty<T7>()));
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>, IEnumerable<T7>)> Read<T1, T2, T3, T4, T5, T6, T7>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>, IEnumerable<T7>), TDbConnection>
              (
                  setting: _jobSetting,
                  state: new DbConnectorSimpleState { Flags = _flags },
                  onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                  onExecute: (d, p) => OnExecuteRead(d, p)
              ).SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>(), Enumerable.Empty<T5>(), Enumerable.Empty<T6>(), Enumerable.Empty<T7>()));
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>)> Read<T1, T2, T3, T4, T5, T6>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>), TDbConnection>
              (
                  setting: _jobSetting,
                  state: new DbConnectorSimpleState { Flags = _flags },
                  onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                  onExecute: (d, p) => OnExecuteRead(d, p)
              ).SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>(), Enumerable.Empty<T5>(), Enumerable.Empty<T6>()));
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>)> Read<T1, T2, T3, T4, T5, T6>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>), TDbConnection>
              (
                  setting: _jobSetting,
                  state: new DbConnectorSimpleState { Flags = _flags },
                  onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                  onExecute: (d, p) => OnExecuteRead(d, p)
              ).SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>(), Enumerable.Empty<T5>(), Enumerable.Empty<T6>()));
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>)> Read<T1, T2, T3, T4, T5>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>), TDbConnection>
              (
                  setting: _jobSetting,
                  state: new DbConnectorSimpleState { Flags = _flags },
                  onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                  onExecute: (d, p) => OnExecuteRead(d, p)
              ).SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>(), Enumerable.Empty<T5>()));
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>)> Read<T1, T2, T3, T4, T5>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>), TDbConnection>
              (
                  setting: _jobSetting,
                  state: new DbConnectorSimpleState { Flags = _flags },
                  onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                  onExecute: (d, p) => OnExecuteRead(d, p)
              ).SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>(), Enumerable.Empty<T5>()));
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>)> Read<T1, T2, T3, T4>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>), TDbConnection>
              (
                  setting: _jobSetting,
                  state: new DbConnectorSimpleState { Flags = _flags },
                  onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                  onExecute: (d, p) => OnExecuteRead(d, p)
              ).SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>()));
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>)> Read<T1, T2, T3, T4>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>), TDbConnection>
              (
                  setting: _jobSetting,
                  state: new DbConnectorSimpleState { Flags = _flags },
                  onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                  onExecute: (d, p) => OnExecuteRead(d, p)
              ).SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>()));
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> Read<T1, T2, T3>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>), TDbConnection>
              (
                  setting: _jobSetting,
                  state: new DbConnectorSimpleState { Flags = _flags },
                  onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                  onExecute: (d, p) => OnExecuteRead(d, p)
              ).SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>()));
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> Read<T1, T2, T3>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>), TDbConnection>
              (
                  setting: _jobSetting,
                  state: new DbConnectorSimpleState { Flags = _flags },
                  onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                  onExecute: (d, p) => OnExecuteRead(d, p)
              ).SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>()));
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>)> Read<T1, T2>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>), TDbConnection>
              (
                  setting: _jobSetting,
                  state: new DbConnectorSimpleState { Flags = _flags },
                  onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                  onExecute: (d, p) => OnExecuteRead(d, p)
              ).SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>()));
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>)> Read<T1, T2>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>), TDbConnection>
              (
                  setting: _jobSetting,
                  state: new DbConnectorSimpleState { Flags = _flags },
                  onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                  onExecute: (d, p) => OnExecuteRead(d, p)
              ).SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>()));
        }
    }
}
