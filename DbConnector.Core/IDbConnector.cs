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
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace DbConnector.Core
{
    /// <summary>
    /// A performance-driven and ADO.NET data provider-agnostic ORM library.
    /// </summary>
    public partial interface IDbConnector<TDbConnection> : IDbConnector
       where TDbConnection : DbConnection
    {
    }

    /// <summary>
    /// A performance-driven and ADO.NET data provider-agnostic ORM library.
    /// </summary>
    public partial interface IDbConnector
    {
        /// <summary>
        /// Gets the string used to open the connection.
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Gets the type of <see cref="DbConnection"/> being used.
        /// </summary>
        Type ConnectionType { get; }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IEnumerable{T}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>        
        /// <returns>The <see cref="IDbJob{IEnumerable{T}}"/>.</returns>
        IDbJob<IEnumerable<T>> Read<T>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>       
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        IDbJob<T> ReadFirst<T>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>       
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<T> ReadFirstOrDefault<T>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>       
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        IDbJob<T> ReadSingle<T>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>       
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        IDbJob<T> ReadSingleOrDefault<T>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{List{T}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>        
        /// <returns>The <see cref="IDbJob{List{T}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<List<T>> ReadToList<T>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{System.Data.DataTable}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>        
        /// <returns>The <see cref="IDbJob{System.Data.DataTable}"/>.</returns>
        IDbJob<DataTable> ReadToDataTable(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{System.Data.DataSet}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <param name="onInit">Action that is used to configure and enqueue all the <see cref="IDbJobCommand"/>.</param>     
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{System.Data.DataSet}"/>.</returns>
        IDbJob<DataSet> ReadToDataSet(Action<Queue<Action<IDbJobCommand>>> onInit, bool? withIsolatedConnections = null);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IEnumerable{List{KeyValuePair{string, object}}}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>This is usefull when requiring a generic data list from the query result.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{IEnumerable{List{KeyValuePair{string, object}}}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<IEnumerable<List<KeyValuePair<string, object>>>> ReadToKeyValuePairs(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IEnumerable{Dictionary{string, object}}}"/> able to execute a reader based on the <paramref name="onInit"/> action.
        ///  This is usefull when requiring a non-concrete data list from unique columns of the query result.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>        
        /// <returns>The <see cref="IDbJob{IEnumerable{Dictionary{string, object}}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<IEnumerable<Dictionary<string, object>>> ReadToDictionaries(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{List{List{KeyValuePair{string, object}}}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>This is usefull when requiring a generic data list from the query result.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{List{List{KeyValuePair{string, object}}}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<List<List<KeyValuePair<string, object>>>> ReadToListOfKeyValuePairs(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{List{Dictionary{string, object}}}"/> able to execute a reader based on the <paramref name="onInit"/> action.
        ///  This is usefull when requiring a non-concrete data list from unique columns of the query result.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>        
        /// <returns>The <see cref="IDbJob{List{Dictionary{string, object}}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<List<Dictionary<string, object>>> ReadToListOfDictionaries(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IDbCollectionSet}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>This is usefull when wanting to create a concrete object from multiple/different queries.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <param name="onInit">Action that is used to configure and enqueue all the <see cref="IDbJobCommand"/>.</param>        
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IDbCollectionSet}"/>.</returns>
        IDbJob<IDbCollectionSet> ReadToDbCollectionSet(Action<Queue<Action<IDbJobCommand>>> onInit, bool? withIsolatedConnections = null);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to load the data based on the <paramref name="onLoad"/> action.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="onInit">Action that is used to configure and enqueue all the <see cref="IDbJobCommand"/>.</param>
        /// <param name="onLoad">Function that is used to access the generated <see cref="DbDataReader"/> and transform the <typeparamref name="T"/> result.</param>        
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when onLoad is null.</exception>
        IDbJob<T> ReadTo<T>(Action<Queue<Action<IDbJobCommand>>> onInit,
            Func<T, IDbExecutionModel, DbDataReader, T> onLoad);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{int?}"/> able to execute a non-query based on the <paramref name="onInit"/> action.</para>
        ///  <para> The result will be null if the non-query fails. Otherwise, the result will be the number of rows affected if the non-query ran successfully.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteNonQuery"/>
        /// </summary>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>        
        /// <returns>The <see cref="IDbJob{int?}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<int?> NonQuery(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to execute a non-query based on the <paramref name="onInit"/> action.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteNonQuery"/>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<T> NonQuery<T>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{int?}"/> able to execute all non-queries based on the <paramref name="onInit"/> action.</para>
        ///  <para>The result will be null if a non-query fails. Otherwise, the result will be the number of rows affected if all non-queries ran successfully.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteNonQuery"/>
        /// </summary>
        /// <param name="onInit">Action that is used to configure and enqueue all the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{int?}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<int?> NonQueries(Action<Queue<Action<IDbJobCommand>>> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to execute all non-queries based on the <paramref name="onInit"/> action.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteNonQuery"/>
        /// </summary>
        /// <param name="onInit">Action that is used to configure and enqueue all the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        IDbJob<T> NonQueries<T>(Action<Queue<Action<IDbJobCommand>>> onInit);

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> to get the first column of the first row in the result
        ///  set returned by the query. All other columns and rows are ignored.</para>
        ///  <para>Valid <typeparamref name="T"/> types: any .NET built-in type, or any non-reference type that is not assignable from <see cref="System.Collections.IEnumerable"/> or <see cref="IListSource"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteScalar"/>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidCastException">Thrown when <typeparamref name="T"/> is not supported.</exception>
        IDbJob<T> Scalar<T>(Action<IDbJobCommand> onInit);

        /// <summary>
        ///  Creates a <see cref="IDbJob{T}"/> which can be controlled 
        ///  by the <see cref="IDbExecutionModel"/> properties of the <see cref="IDbJob{T}.OnExecuted(Func{T, IDbExecutionModel, T})"/> function.
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="onInit">Action that is used to configure and enqueue all the <see cref="IDbJobCommand"/>.<para>Note: This can only be null if <paramref name="isCreateDbCommand"/> is set to false.</para></param>
        /// <param name="onExecute">Function that will be invoked for each <see cref="IDbJobCommand"/> and can be used to execute database calls and set the <typeparamref name="T"/> result.</param>
        /// <param name="isCreateDbCommand">Set this to false to disable the auto creation of a <see cref="DbCommand"/>. (Optional)</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        IDbJob<T> Build<T>(Action<Queue<Action<IDbJobCommand>>> onInit, Func<T, IDbExecutionModel, T> onExecute, bool isCreateDbCommand = true);

        /// <summary>
        /// Check if the database is available based on the provided connection string.
        /// </summary>
        /// <returns>The <see cref="IDbJob{bool}"/>.</returns>
        IDbJob<bool> IsConnected();
    }
}
