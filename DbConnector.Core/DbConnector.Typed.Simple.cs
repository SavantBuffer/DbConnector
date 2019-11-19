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
        ///  <para>Creates a <see cref="IDbJob{IEnumerable{object}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IEnumerable{object}}"/>.</returns>
        public IDbJob<IEnumerable<object>> Read(
            Type type,
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type cannot be null!");
            }

            return new DbJob<IEnumerable<object>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteRead(type, p)
                ).SetOnError((d, e) => Enumerable.Empty<object>());
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{IEnumerable{object}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
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
                throw new ArgumentNullException("type cannot be null!");
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
        ///  <para>Creates a <see cref="IDbJob{object}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<object> ReadFirst(
            Type type,
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onInit: () => type.IsValueType ? Activator.CreateInstance(type) : null,
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadFirst(type, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{object}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
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
                throw new ArgumentNullException("type cannot be null!");
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
        ///  <para>Creates a <see cref="IDbJob{object}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        public IDbJob<object> ReadFirstOrDefault(
            Type type,
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onInit: () => type.IsValueType ? Activator.CreateInstance(type) : null,
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadFirstOrDefault(type, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{object}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
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
                throw new ArgumentNullException("type cannot be null!");
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
        ///  <para>Creates a <see cref="IDbJob{object}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
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
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onInit: () => type.IsValueType ? Activator.CreateInstance(type) : null,
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadSingle(type, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{object}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
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
                throw new ArgumentNullException("type cannot be null!");
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
        ///  <para>Creates a <see cref="IDbJob{object}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<object> ReadSingleOrDefault(
            Type type,
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onInit: () => type.IsValueType ? Activator.CreateInstance(type) : null,
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadSingleOrDefault(type, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{object}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
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
                throw new ArgumentNullException("type cannot be null!");
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
        ///  <para>Creates a <see cref="IDbJob{List{object}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{List{object}}"/>.</returns>
        public IDbJob<List<object>> ReadToList(
            Type type,
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type cannot be null!");
            }

            return new DbJob<List<object>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadToList(type, p)
                ).SetOnError((d, e) => new List<object>());
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{List{object}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
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
                throw new ArgumentNullException("type cannot be null!");
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
        ///  <para>Creates a <see cref="IDbJob{object}"/> to get the first column of the first row in the result
        ///  set returned by the query. All other columns and rows are ignored.</para>        
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteScalar"/>
        /// </summary>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        public IDbJob<object> Scalar(
            Type type,
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) =>
                    {
                        object scalar = p.Command.ExecuteScalar();

                        return scalar != DBNull.Value ? scalar : null;
                    }
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{object}"/> to get the first column of the first row in the result
        ///  set returned by the query. All other columns and rows are ignored.</para>        
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteScalar"/>
        /// </summary>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        public IDbJob<object> Scalar(
            Type type,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type cannot be null!");
            }

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
