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
using System.Collections;
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
        ///  <para>Creates an <see cref="IDbJob{IEnumerable{object}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IEnumerable{object}}"/>.</returns>
        public IDbJob<IEnumerable<object>> Read(
            Type type,
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            return new DbJob<IEnumerable<object>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteRead(type, p)
                ).SetOnError((d, e) => Enumerable.Empty<object>());
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IEnumerable{object}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="sql">The query text command to run against the data source.</param>        
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IEnumerable{object}}"/>.</returns>
        public IDbJob<IEnumerable<object>> Read(
            Type type,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            return new DbJob<IEnumerable<object>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteRead(type, p)
                ).SetOnError((d, e) => Enumerable.Empty<object>());
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IAsyncEnumerable{object}}"/> able to execute a reader, with an un-buffered (deferred/yielded) approach, based on the configured parameters.</para>
        ///  <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// <para>This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.</para>
        /// <para>Warning: Deferred execution leverages "yield statement" logic and postpones the disposal of database connections and related resources. 
        /// Always perform an iteration of the returned <see cref="IAsyncEnumerable{T}"/> by either implementing a "for-each" loop or a data projection (e.g. invoking the <see cref="System.Linq.AsyncEnumerable.ToListAsync{TSource}(IAsyncEnumerable{TSource}, System.Threading.CancellationToken)"/> extension). You can also dispose the enumerator as an alternative.
        /// Not doing so will internally leave disposable resources opened (e.g. database connections) consequently creating memory leak scenarios.
        /// </para>
        /// <para>Warning: Exceptions may occur while looping deferred <see cref="IAsyncEnumerable{T}"/> types because of the implicit database connection dependency.</para>
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IAsyncEnumerable{object}}"/>.</returns>
        public IDbJob<IAsyncEnumerable<object>> ReadAsAsyncEnumerable(
            Type type,
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            return new DbJob<IAsyncEnumerable<object>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadAsAsyncEnumerable(type, p)
                ).SetOnError((d, e) => AsyncEnumerable.Empty<object>()).WithoutBuffering();
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IAsyncEnumerable{object}}"/> able to execute a reader, with an un-buffered (deferred/yielded) approach, based on the configured parameters.</para>
        ///  <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// <para>This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.</para>
        /// <para>Warning: Deferred execution leverages "yield statement" logic and postpones the disposal of database connections and related resources. 
        /// Always perform an iteration of the returned <see cref="IAsyncEnumerable{T}"/> by either implementing a "for-each" loop or a data projection (e.g. invoking the <see cref="System.Linq.AsyncEnumerable.ToListAsync{TSource}(IAsyncEnumerable{TSource}, System.Threading.CancellationToken)"/> extension). You can also dispose the enumerator as an alternative.
        /// Not doing so will internally leave disposable resources opened (e.g. database connections) consequently creating memory leak scenarios.
        /// </para>
        /// <para>Warning: Exceptions may occur while looping deferred <see cref="IAsyncEnumerable{T}"/> types because of the implicit database connection dependency.</para>
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="sql">The query text command to run against the data source.</param>        
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IAsyncEnumerable{object}}"/>.</returns>
        public IDbJob<IAsyncEnumerable<object>> ReadAsAsyncEnumerable(
            Type type,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            return new DbJob<IAsyncEnumerable<object>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadAsAsyncEnumerable(type, p)
                ).SetOnError((d, e) => AsyncEnumerable.Empty<object>()).WithoutBuffering();
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{object}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only the first row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<object> ReadFirst(
            Type type,
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onInit: () => type.IsValueType ? Activator.CreateInstance(type) : null,
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadFirst(type, p)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{object}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only the first row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<object> ReadFirst(
            Type type,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onInit: () => type.IsValueType ? Activator.CreateInstance(type) : null,
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadFirst(type, p)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{object}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only the first row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        public IDbJob<object> ReadFirstOrDefault(
            Type type,
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onInit: () => type.IsValueType ? Activator.CreateInstance(type) : null,
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadFirstOrDefault(type, p)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{object}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only the first row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        public IDbJob<object> ReadFirstOrDefault(
            Type type,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onInit: () => type.IsValueType ? Activator.CreateInstance(type) : null,
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadFirstOrDefault(type, p)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{object}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<object> ReadSingle(
            Type type,
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onInit: () => type.IsValueType ? Activator.CreateInstance(type) : null,
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadSingle(type, p)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{object}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<object> ReadSingle(
            Type type,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onInit: () => type.IsValueType ? Activator.CreateInstance(type) : null,
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadSingle(type, p)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{object}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<object> ReadSingleOrDefault(
            Type type,
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onInit: () => type.IsValueType ? Activator.CreateInstance(type) : null,
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadSingleOrDefault(type, p)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{object}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<object> ReadSingleOrDefault(
            Type type,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onInit: () => type.IsValueType ? Activator.CreateInstance(type) : null,
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadSingleOrDefault(type, p)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{List{object}}"/> able to execute a reader based on the configured parameters.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{List{object}}"/>.</returns>
        public IDbJob<List<object>> ReadToList(
            Type type,
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            return new DbJob<List<object>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadToList(type, p)
                ).SetOnError((d, e) => new List<object>());
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{List{object}}"/> able to execute a reader based on the configured parameters.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{List{object}}"/>.</returns>
        public IDbJob<List<object>> ReadToList(
            Type type,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            return new DbJob<List<object>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadToList(type, p)
                ).SetOnError((d, e) => new List<object>());
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{HashSet{object}}"/> able to read the first column of each row from the query result based on the configured parameters. All other columns are ignored.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default. <see cref="DBNull"/> values will be excluded.
        /// </remarks>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{HashSet{object}}"/>.</returns>
        public IDbJob<HashSet<object>> ReadToHashSet(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<HashSet<object>, TDbConnection>
               (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadToHashSetOfObject(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{HashSet{object}}"/> able to read the first column of each row from the query result based on the configured parameters. All other columns are ignored.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default. <see cref="DBNull"/> values will be excluded.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>        
        /// <returns>The <see cref="IDbJob{HashSet{object}}"/>.</returns>
        public IDbJob<HashSet<object>> ReadToHashSet(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<HashSet<object>, TDbConnection>
               (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadToHashSetOfObject(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{object}"/> to get the first column of the first row from the result
        ///  set returned by the query. All other columns and rows are ignored.</para>        
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteScalar"/>
        /// </summary>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        public IDbJob<object> Scalar(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) =>
                    {
                        object scalar = p.Command.ExecuteScalar();

                        return scalar != DBNull.Value ? scalar : null;
                    }
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{object}"/> to get the first column of the first row from the result
        ///  set returned by the query. All other columns and rows are ignored.</para>        
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteScalar"/>
        /// </summary>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        public IDbJob<object> Scalar(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) =>
                    {
                        object scalar = p.Command.ExecuteScalar();

                        return scalar != DBNull.Value ? scalar : null;
                    }
                );
        }
    }
}
