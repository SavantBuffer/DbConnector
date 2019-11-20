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
        ///  <para>Creates an <see cref="IDbJob{IEnumerable{T}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IEnumerable{T}}"/>.</returns>
        IDbJob<IEnumerable<T>> Read<T>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IEnumerable{T}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param>        
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{IEnumerable{T}}"/>.</returns>
        IDbJob<IEnumerable<T>> Read<T>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only the first row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param>        
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        IDbJob<T> ReadFirst<T>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only the first row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        IDbJob<T> ReadFirst<T>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only the first row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param>        
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        IDbJob<T> ReadFirstOrDefault<T>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only the first row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        IDbJob<T> ReadFirstOrDefault<T>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param>        
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        IDbJob<T> ReadSingle<T>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        IDbJob<T> ReadSingle<T>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param>        
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        IDbJob<T> ReadSingleOrDefault<T>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        IDbJob<T> ReadSingleOrDefault<T>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{List{T}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IEnumerable{T}}"/>.</returns>
        IDbJob<List<T>> ReadToList<T>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{List{T}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param>        
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{IEnumerable{T}}"/>.</returns>
        IDbJob<List<T>> ReadToList<T>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{System.Data.DataTable}"/> able to execute a reader based on the configured parameters.</para>
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
        /// <returns>The <see cref="IDbJob{System.Data.DataTable}"/>.</returns>
        IDbJob<DataTable> ReadToDataTable(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{System.Data.DataTable}"/> able to execute a reader based on the configured parameters.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>        
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{System.Data.DataTable}"/>.</returns>
        IDbJob<DataTable> ReadToDataTable(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{System.Data.DataSet}"/> able to execute a reader based on the configured parameters.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{System.Data.DataSet}"/>.</returns>
        IDbJob<DataSet> ReadToDataSet(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{System.Data.DataSet}"/> able to execute a reader based on the configured parameters.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{System.Data.DataSet}"/>.</returns>
        IDbJob<DataSet> ReadToDataSet(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IEnumerable{List{KeyValuePair{string, object}}}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>This is usefull when requiring a generic data list from the query result.</para>
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
        /// <returns>The <see cref="IDbJob{IEnumerable{List{KeyValuePair{string, object}}}}"/>.</returns>
        IDbJob<IEnumerable<List<KeyValuePair<string, object>>>> ReadToKeyValuePairs(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IEnumerable{List{KeyValuePair{string, object}}}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>This is usefull when requiring a generic data list from the query result.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>       
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{IEnumerable{List{KeyValuePair{string, object}}}}"/>.</returns>
        IDbJob<IEnumerable<List<KeyValuePair<string, object>>>> ReadToKeyValuePairs(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IEnumerable{Dictionary{string, object}}}"/> able to execute a reader based on the configured parameters.
        ///  This is usefull when requiring a non-concrete data list from unique columns of the query result.</para>
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
        /// <returns>The <see cref="IDbJob{IEnumerable{Dictionary{string, object}}}"/>.</returns>
        IDbJob<IEnumerable<Dictionary<string, object>>> ReadToDictionaries(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IEnumerable{Dictionary{string, object}}}"/> able to execute a reader based on the configured parameters.
        ///  This is usefull when requiring a non-concrete data list from unique columns of the query result.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{IEnumerable{Dictionary{string, object}}}"/>.</returns>
        IDbJob<IEnumerable<Dictionary<string, object>>> ReadToDictionaries(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{List{List{KeyValuePair{string, object}}}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>This is usefull when requiring a generic data list from the query result.</para>
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
        /// <returns>The <see cref="IDbJob{List{List{KeyValuePair{string, object}}}}"/>.</returns>
        IDbJob<List<List<KeyValuePair<string, object>>>> ReadToListOfKeyValuePairs(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{List{List{KeyValuePair{string, object}}}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>This is usefull when requiring a generic data list from the query result.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{List{List{KeyValuePair{string, object}}}}"/>.</returns>
        IDbJob<List<List<KeyValuePair<string, object>>>> ReadToListOfKeyValuePairs(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{List{Dictionary{string, object}}}"/> able to execute a reader based on the configured parameters.
        ///  This is usefull when requiring a non-concrete data list from unique columns of the query result.</para>
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
        /// <returns>The <see cref="IDbJob{List{Dictionary{string, object}}}"/>.</returns>
        IDbJob<List<Dictionary<string, object>>> ReadToListOfDictionaries(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{List{Dictionary{string, object}}}"/> able to execute a reader based on the configured parameters.
        ///  This is usefull when requiring a non-concrete data list from unique columns of the query result.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{List{Dictionary{string, object}}}"/>.</returns>
        IDbJob<List<Dictionary<string, object>>> ReadToListOfDictionaries(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IDbCollectionSet}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>This is usefull when wanting to create a concrete object from multiple/different queries.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IDbCollectionSet}"/>.</returns>
        IDbJob<IDbCollectionSet> ReadToDbCollectionSet(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IDbCollectionSet}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>This is usefull when wanting to create a concrete object from multiple/different queries.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{IDbCollectionSet}"/>.</returns>
        IDbJob<IDbCollectionSet> ReadToDbCollectionSet(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to load the data based on the <paramref name="onLoad"/> action.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param>        
        /// <param name="param">The parameter to use (or null for empty). <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply.</param> 
        /// <param name="onLoad">Function that is used to access the generated <see cref="DbDataReader"/> and transform the <typeparamref name="T"/> result.</param>
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onLoad is null.</exception>
        IDbJob<T> ReadTo<T>(
            IColumnMapSetting mapSettings,
            string sql,
            object param,
            Func<T, IDbExecutionModel, DbDataReader, T> onLoad,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to load the data based on the <paramref name="onLoad"/> action.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param>     
        /// <param name="param">The parameter to use (or null for empty). <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply.</param> 
        /// <param name="onLoad">Function that is used to access the generated <see cref="DbDataReader"/> and transform the <typeparamref name="T"/> result.</param>
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onLoad is null.</exception>
        IDbJob<T> ReadTo<T>(
            string sql,
            object param,
            Func<T, IDbExecutionModel, DbDataReader, T> onLoad,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> to get the first column of the first row in the result
        ///  set returned by the query. All other columns and rows are ignored.</para>
        ///  <para>Valid <typeparamref name="T"/> types: any .NET built-in type, or any non-reference type that is not assignable from <see cref="System.Collections.IEnumerable"/> or <see cref="IListSource"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteScalar"/>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="InvalidCastException">Thrown when <typeparamref name="T"/> is not supported.</exception>
        IDbJob<T> Scalar<T>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> to get the first column of the first row in the result
        ///  set returned by the query. All other columns and rows are ignored.</para>
        ///  <para>Valid <typeparamref name="T"/> types: any .NET built-in type, or any non-reference type that is not assignable from <see cref="System.Collections.IEnumerable"/> or <see cref="IListSource"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteScalar"/>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="InvalidCastException">Thrown when <typeparamref name="T"/> is not supported.</exception>
        IDbJob<T> Scalar<T>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{int?}"/> able to execute a non-query based on the configured parameters.</para>
        ///  <para> The result will be null if the non-query fails. Otherwise, the result will be the number of rows affected if the non-query ran successfully.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteNonQuery"/>
        /// </summary>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{int?}"/>.</returns>
        IDbJob<int?> NonQuery(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{int?}"/> able to execute a non-query based on the configured parameters.</para>
        ///  <para> The result will be null if the non-query fails. Otherwise, the result will be the number of rows affected if the non-query ran successfully.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteNonQuery"/>
        /// </summary>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{int?}"/>.</returns>
        IDbJob<int?> NonQuery(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to execute a non-query based on the configured parameters.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteNonQuery"/>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        IDbJob<T> NonQuery<T>(
            IColumnMapSetting mapSettings,
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to execute a non-query based on the configured parameters.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteNonQuery"/>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        IDbJob<T> NonQuery<T>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        ///  Creates a <see cref="IDbJob{T}"/> which can be controlled 
        ///  by the <see cref="IDbExecutionModel"/> properties of the <see cref="IDbJob{T}.OnExecuted(Func{T, IDbExecutionModel, T})"/> function.
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="mapSettings">The <see cref="IColumnMapSetting"/> to use.</param> 
        /// <param name="sql">The query text command to run against the data source.</param>        
        /// <param name="param">The parameter to use (or null for empty). <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply.</param> 
        /// <param name="onExecute">Function that will be invoked for each <see cref="IDbJobCommand"/> and can be used to execute database calls and set the <typeparamref name="T"/> result.</param>
        /// <param name="isCreateDbCommand">Set this to false to disable the auto creation of a <see cref="DbCommand"/>. (Optional)</param>
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onExecute is null.</exception>
        IDbJob<T> Build<T>(
            IColumnMapSetting mapSettings,
            string sql,
            object param,
            Func<T, IDbExecutionModel, T> onExecute,
            bool isCreateDbCommand = true,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None);

        /// <summary>
        ///  Creates a <see cref="IDbJob{T}"/> which can be controlled 
        ///  by the <see cref="IDbExecutionModel"/> properties of the <see cref="IDbJob{T}.OnExecuted(Func{T, IDbExecutionModel, T})"/> function.
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param>   
        /// <param name="param">The parameter to use (or null for empty). <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply.</param> 
        /// <param name="onExecute">Function that will be invoked for each <see cref="IDbJobCommand"/> and can be used to execute database calls and set the <typeparamref name="T"/> result.</param>
        /// <param name="isCreateDbCommand">Set this to false to disable the auto creation of a <see cref="DbCommand"/>. (Optional)</param>
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onExecute is null.</exception>
        IDbJob<T> Build<T>(
            string sql,
            object param,
            Func<T, IDbExecutionModel, T> onExecute,
            bool isCreateDbCommand = true,
            CommandType commandType = CommandType.Text);
    }
}
