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
        ///  <para>Creates a <see cref="IDbJob{IEnumerable{dynamic}}"/> able to execute a reader based on the configured parameters.</para>   
        ///  <para>Use this to dynamically load the query results into an IEnumerable of <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IEnumerable{dynamic}}"/>.</returns>
        public IDbJob<IEnumerable<dynamic>> Read(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<IEnumerable<dynamic>, TDbConnection>
                   (
                       setting: _jobSetting,
                       state: new DbConnectorSimpleState { Flags = _flags },
                       onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                       onExecute: (d, p) => OnExecuteReadDynamic(d, p)
                   ).SetOnError((d, e) => Enumerable.Empty<dynamic>());
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{IEnumerable{dynamic}}"/> able to execute a reader based on the configured parameters.</para>   
        ///  <para>Use this to dynamically load the query results into an IEnumerable of <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>        
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IEnumerable{dynamic}}"/>.</returns>
        public IDbJob<IEnumerable<dynamic>> Read(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<IEnumerable<dynamic>, TDbConnection>
                   (
                       setting: _jobSetting,
                       state: new DbConnectorSimpleState { Flags = _flags },
                       onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                       onExecute: (d, p) => OnExecuteReadDynamic(d, p)
                   ).SetOnError((d, e) => Enumerable.Empty<dynamic>());
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{dynamic}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to dynamically load only the first row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>        
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>      
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{dynamic}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<dynamic> ReadFirst(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<dynamic, TDbConnection>
                   (
                       setting: _jobSetting,
                       state: new DbConnectorSimpleState { Flags = _flags },
                       onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                       onExecute: (d, p) => OnExecuteReadFirstDynamic(d, p)
                   );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{dynamic}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to dynamically load only the first row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>        
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>  
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{dynamic}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<dynamic> ReadFirst(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<dynamic, TDbConnection>
                   (
                       setting: _jobSetting,
                       state: new DbConnectorSimpleState { Flags = _flags },
                       onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                       onExecute: (d, p) => OnExecuteReadFirstDynamic(d, p)
                   );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{dynamic}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to dynamically load only the first row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{dynamic}"/>.</returns>
        public IDbJob<dynamic> ReadFirstOrDefault(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<dynamic, TDbConnection>
                   (
                       setting: _jobSetting,
                       state: new DbConnectorSimpleState { Flags = _flags },
                       onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                       onExecute: (d, p) => OnExecuteReadFirstOrDefaultDynamic(d, p)
                   );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{dynamic}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to dynamically load only the first row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{dynamic}"/>.</returns>
        public IDbJob<dynamic> ReadFirstOrDefault(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<dynamic, TDbConnection>
                   (
                       setting: _jobSetting,
                       state: new DbConnectorSimpleState { Flags = _flags },
                       onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                       onExecute: (d, p) => OnExecuteReadFirstOrDefaultDynamic(d, p)
                   );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{dynamic}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to dynamically load only a single row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{dynamic}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<dynamic> ReadSingle(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<dynamic, TDbConnection>
                   (
                       setting: _jobSetting,
                       state: new DbConnectorSimpleState { Flags = _flags },
                       onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                       onExecute: (d, p) => OnExecuteReadSingleDynamic(d, p)
                   );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{dynamic}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to dynamically load only a single row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{dynamic}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<dynamic> ReadSingle(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<dynamic, TDbConnection>
                   (
                       setting: _jobSetting,
                       state: new DbConnectorSimpleState { Flags = _flags },
                       onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                       onExecute: (d, p) => OnExecuteReadSingleDynamic(d, p)
                   );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{dynamic}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to dynamically load only a single row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>        
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{dynamic}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<dynamic> ReadSingleOrDefault(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<dynamic, TDbConnection>
                   (
                       setting: _jobSetting,
                       state: new DbConnectorSimpleState { Flags = _flags },
                       onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                       onExecute: (d, p) => OnExecuteReadSingleOrDefaultDynamic(d, p)
                   );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{dynamic}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to dynamically load only a single row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>        
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{dynamic}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<dynamic> ReadSingleOrDefault(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<dynamic, TDbConnection>
                   (
                       setting: _jobSetting,
                       state: new DbConnectorSimpleState { Flags = _flags },
                       onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                       onExecute: (d, p) => OnExecuteReadSingleOrDefaultDynamic(d, p)
                   );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{List{dynamic}}"/> able to execute a reader based on the configured parameters.</para>   
        ///  <para>Use this to dynamically load the query results into a List of <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{List{dynamic}}"/>.</returns>
        public IDbJob<List<dynamic>> ReadToList(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<List<dynamic>, TDbConnection>
                   (
                       setting: _jobSetting,
                       state: new DbConnectorSimpleState { Flags = _flags },
                       onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, mapSettings, sql, param, commandType, commandBehavior, commandTimeout, flags),
                       onExecute: (d, p) => OnExecuteReadToListDynamic(d, p)
                   ).SetOnError((d, e) => new List<dynamic>());
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{List{dynamic}}"/> able to execute a reader based on the configured parameters.</para>   
        ///  <para>Use this to dynamically load the query results into a List of <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{List{dynamic}}"/>.</returns>
        public IDbJob<List<dynamic>> ReadToList(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<List<dynamic>, TDbConnection>
                   (
                       setting: _jobSetting,
                       state: new DbConnectorSimpleState { Flags = _flags },
                       onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                       onExecute: (d, p) => OnExecuteReadToListDynamic(d, p)
                   ).SetOnError((d, e) => new List<dynamic>());
        }
    }
}
