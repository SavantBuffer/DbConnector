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
    public partial interface IDbConnector
    {
        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IEnumerable{dynamic}}"/> able to execute a reader based on the configured parameters.</para>   
        ///  <para>Use this to dynamically load the query results into an IEnumerable of <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
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
        IDbJob<IEnumerable<dynamic>> Read(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IEnumerable{dynamic}}"/> able to execute a reader based on the configured parameters.</para>   
        ///  <para>Use this to dynamically load the query results into an IEnumerable of <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>        
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IEnumerable{dynamic}}"/>.</returns>
        IDbJob<IEnumerable<dynamic>> Read(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IAsyncEnumerable{dynamic}}"/> able to execute a reader, with an un-buffered (deferred/yielded) approach, based on the configured parameters.</para>   
        ///  <para>Use this to dynamically load the query results into an IAsyncEnumerable of <see cref="System.Dynamic.ExpandoObject"/>.</para>
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
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IAsyncEnumerable{dynamic}}"/>.</returns>
        IDbJob<IAsyncEnumerable<dynamic>> ReadAsAsyncEnumerable(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IAsyncEnumerable{dynamic}}"/> able to execute a reader, with an un-buffered (deferred/yielded) approach, based on the configured parameters.</para>   
        ///  <para>Use this to dynamically load the query results into an IAsyncEnumerable of <see cref="System.Dynamic.ExpandoObject"/>.</para>
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
        /// <param name="sql">The query text command to run against the data source.</param>        
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IAsyncEnumerable{dynamic}}"/>.</returns>
        IDbJob<IAsyncEnumerable<dynamic>> ReadAsAsyncEnumerable(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{dynamic}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to dynamically load only the first row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>        
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
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
        IDbJob<dynamic> ReadFirst(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{dynamic}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to dynamically load only the first row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>        
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>  
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{dynamic}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        IDbJob<dynamic> ReadFirst(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{dynamic}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to dynamically load only the first row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
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
        IDbJob<dynamic> ReadFirstOrDefault(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{dynamic}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to dynamically load only the first row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{dynamic}"/>.</returns>
        IDbJob<dynamic> ReadFirstOrDefault(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{dynamic}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to dynamically load only a single row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
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
        IDbJob<dynamic> ReadSingle(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{dynamic}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to dynamically load only a single row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
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
        IDbJob<dynamic> ReadSingle(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{dynamic}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to dynamically load only a single row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>        
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
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
        IDbJob<dynamic> ReadSingleOrDefault(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{dynamic}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to dynamically load only a single row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>        
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{dynamic}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        IDbJob<dynamic> ReadSingleOrDefault(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{List{dynamic}}"/> able to execute a reader based on the configured parameters.</para>   
        ///  <para>Use this to dynamically load the query results into a List of <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
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
        IDbJob<List<dynamic>> ReadToList(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{List{dynamic}}"/> able to execute a reader based on the configured parameters.</para>   
        ///  <para>Use this to dynamically load the query results into a List of <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{List{dynamic}}"/>.</returns>
        IDbJob<List<dynamic>> ReadToList(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);
    }
}
