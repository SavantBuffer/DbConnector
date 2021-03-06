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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DbConnector.Core
{
    public partial class DbConnector<TDbConnection> : IDbConnector<TDbConnection>
       where TDbConnection : DbConnection
    {
        #region ReadSingle

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6, T7, T8)> ReadSingle<T1, T2, T3, T4, T5, T6, T7, T8>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(T1, T2, T3, T4, T5, T6, T7, T8), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6, T7, T8)> ReadSingle<T1, T2, T3, T4, T5, T6, T7, T8>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(T1, T2, T3, T4, T5, T6, T7, T8), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6, T7)> ReadSingle<T1, T2, T3, T4, T5, T6, T7>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(T1, T2, T3, T4, T5, T6, T7), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6, T7)> ReadSingle<T1, T2, T3, T4, T5, T6, T7>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(T1, T2, T3, T4, T5, T6, T7), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6)> ReadSingle<T1, T2, T3, T4, T5, T6>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(T1, T2, T3, T4, T5, T6), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6)> ReadSingle<T1, T2, T3, T4, T5, T6>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(T1, T2, T3, T4, T5, T6), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4, T5)> ReadSingle<T1, T2, T3, T4, T5>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(T1, T2, T3, T4, T5), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4, T5)> ReadSingle<T1, T2, T3, T4, T5>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(T1, T2, T3, T4, T5), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4)> ReadSingle<T1, T2, T3, T4>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(T1, T2, T3, T4), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4)> ReadSingle<T1, T2, T3, T4>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(T1, T2, T3, T4), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3)> ReadSingle<T1, T2, T3>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(T1, T2, T3), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3)> ReadSingle<T1, T2, T3>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(T1, T2, T3), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2)> ReadSingle<T1, T2>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(T1, T2), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2)> ReadSingle<T1, T2>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(T1, T2), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        #endregion



        #region ReadSingleOrDefault

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6, T7, T8)> ReadSingleOrDefault<T1, T2, T3, T4, T5, T6, T7, T8>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(T1, T2, T3, T4, T5, T6, T7, T8), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6, T7, T8)> ReadSingleOrDefault<T1, T2, T3, T4, T5, T6, T7, T8>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(T1, T2, T3, T4, T5, T6, T7, T8), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6, T7)> ReadSingleOrDefault<T1, T2, T3, T4, T5, T6, T7>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(T1, T2, T3, T4, T5, T6, T7), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6, T7)> ReadSingleOrDefault<T1, T2, T3, T4, T5, T6, T7>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(T1, T2, T3, T4, T5, T6, T7), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6)> ReadSingleOrDefault<T1, T2, T3, T4, T5, T6>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(T1, T2, T3, T4, T5, T6), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6)> ReadSingleOrDefault<T1, T2, T3, T4, T5, T6>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(T1, T2, T3, T4, T5, T6), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2, T3, T4, T5)> ReadSingleOrDefault<T1, T2, T3, T4, T5>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(T1, T2, T3, T4, T5), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2, T3, T4, T5)> ReadSingleOrDefault<T1, T2, T3, T4, T5>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(T1, T2, T3, T4, T5), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2, T3, T4)> ReadSingleOrDefault<T1, T2, T3, T4>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(T1, T2, T3, T4), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2, T3, T4)> ReadSingleOrDefault<T1, T2, T3, T4>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(T1, T2, T3, T4), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2, T3)> ReadSingleOrDefault<T1, T2, T3>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(T1, T2, T3), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2, T3)> ReadSingleOrDefault<T1, T2, T3>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(T1, T2, T3), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2)> ReadSingleOrDefault<T1, T2>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<(T1, T2), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
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
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2)> ReadSingleOrDefault<T1, T2>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<(T1, T2), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        #endregion
    }
}
