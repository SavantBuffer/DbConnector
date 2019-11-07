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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;

namespace DbConnector.Core
{
    public interface IDbJobCommand
    {
        /// <summary>
        /// Use to set the command type and override the default Text CommandType.
        /// </summary>
        CommandType CommandType { get; set; }

        /// <summary>
        /// Gets or sets the text command to run against the data source.
        /// </summary>
        string CommandText { get; set; }

        CommandBehavior? CommandBehavior { get; set; }

        /// <summary>
        /// The time in seconds to wait for the command to execute.
        /// </summary>
        int CommandTimeout { get; set; }

        DbJobParameterCollection Parameters { get; }

        IColumnMapSetting MapSettings { get; }

        DbJobCommandFlags Flags { get; set; }

        /// <summary>
        /// Get the referenced <see cref="DbParameterCollection"/>. This can be useful when wanting to cast and insert type specific parameters.
        /// </summary>
        /// <returns><see cref="DbParameterCollection"/></returns>
        DbParameterCollection GetDbParameters();

        /// <summary>
        /// Get the referenced <see cref="DbCommand"/>.
        /// </summary>
        /// <returns><see cref="DbCommand"/></returns>
        DbCommand GetDbCommand();
    }

    public interface IDbJobCommand<TStateParam> : IDbJobCommand
    {
        TStateParam StateParam { get; }
    }


    public interface IDbJobSetting
    {
        string ConnectionString { get; set; }

        bool IsThrowExceptions { get; set; }

        bool IsCacheEnabled { get; set; }

        Type DbConnectionType { get; set; }

        IDbConnectorLogger Logger { get; set; }

        IDbJobSetting Clone();
    }


    public interface IColumnMapSetting
    {
        ConcurrentBag<string> NamesToInclude { get; }

        ConcurrentBag<string> NamesToExclude { get; }

        ConcurrentDictionary<Type, string> Joins { get; }

        ConcurrentDictionary<Type, Dictionary<string, string>> Aliases { get; }

        bool HasNamesToInclude { get; }

        bool HasNamesToExclude { get; }

        bool HasAliases { get; }

        bool HasJoins { get; }

        /// <summary>
        /// Set the starting/locator inclusive column name to use when mapping an object of <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">The object type to use.</typeparam>
        /// <param name="columnName">The column name to use.</param>
        /// <returns>The current <see cref="IColumnMapSetting"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="columnName"/> is null.</exception>
        IColumnMapSetting WithSplitOn<T>(string columnName);

        /// <summary>
        /// Set the starting/locator inclusive column name to use when mapping an object of <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">The object type to use.</typeparam>
        /// <param name="expression">The expression to use.</param>
        /// <param name="isUseColumnAttribute">Set to false to not use the <see cref="System.ComponentModel.DataAnnotations.Schema.ColumnAttribute"/> for names. (Optional)</param>
        /// <returns>The current <see cref="IColumnMapSetting"/>.</returns>
        /// <exception cref="MemberAccessException">Thrown when the provided expression's property is not a member of <typeparamref name="T"/>.</exception>
        IColumnMapSetting WithSplitOnFor<T>(Expression<Func<T, object>> expression, bool isUseColumnAttribute = true);

        /// <summary>
        /// Set the starting/locator inclusive column name to use when mapping an object of the specified type.
        /// </summary>
        /// <param name="tType">The object <see cref="Type"/> to use.</param>
        /// <param name="columnName">The column name to use.</param>
        /// <returns>The current <see cref="IColumnMapSetting"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="columnName"/> is null.</exception>
        IColumnMapSetting WithSplitOn(Type tType, string columnName);

        IColumnMapSetting WithAlias<T>(string alias, ColumnAliasMode mode, bool isUseColumnAttribute = true);

        IColumnMapSetting WithAlias(Type tType, string alias, ColumnAliasMode mode, bool isUseColumnAttribute = true);

        IColumnMapSetting WithAliasFor<T>(string alias, ColumnAliasMode mode, bool isUseColumnAttribute, params string[] propertyNames);

        IColumnMapSetting WithAliasFor(Type tType, string alias, ColumnAliasMode mode, bool isUseColumnAttribute, params string[] propertyNames);

        IColumnMapSetting WithAliasFor<T>(string alias, ColumnAliasMode mode, bool isUseColumnAttribute, IEnumerable<string> propertyNames);

        IColumnMapSetting WithAliasFor(Type tType, string alias, ColumnAliasMode mode, bool isUseColumnAttribute, IEnumerable<string> propertyNames);

        IColumnMapSetting WithAliasFor<T>(string alias, ColumnAliasMode mode, bool isUseColumnAttribute, params Expression<Func<T, object>>[] expressions);

        IColumnMapSetting IncludeNamesFor<T>(bool isUseColumnAttribute, params Expression<Func<T, object>>[] expressions);

        IColumnMapSetting IncludeNames(params string[] columnNamesToInclude);

        IColumnMapSetting IncludeNames(IEnumerable<string> columnNamesToInclude);

        IColumnMapSetting ExcludeNamesFor<T>(bool isUseColumnAttribute, params Expression<Func<T, object>>[] expressions);

        IColumnMapSetting ExcludeNames(params string[] columnNamesToExclude);

        IColumnMapSetting ExcludeNames(IEnumerable<string> columnNamesToExclude);
    }


    public interface IDbCollectionSet
    {
        List<List<Dictionary<string, object>>> Items { get; }

        IEnumerable<ColumnMap> GetColumnMaps<T>(int index);

        IEnumerable<ColumnMap> GetColumnMaps<T>(int index, IColumnMapSetting settings);

        IEnumerable<ColumnMap> GetColumnMaps(Type tType, int index);

        IEnumerable<string> GetColumnNames(int index);

        DataSet ToDataSet(bool isDequeueEnabled = true);

        DataSet ToDataSet(CancellationToken token = default, bool isDequeueEnabled = true);

        /// <summary>
        /// Removes and returns the first element of the data collection.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <returns>The object of <typeparamref name="T"/> type.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> does not match the value type or <typeparamref name="T"/> is a class without a parameterless constructor.</exception>
        T DequeueFirstOrDefault<T>();

        /// <summary>
        /// Removes and returns the first element of the data collection.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="settings">The <see cref="IColumnMapSetting"/> to use.</param>
        /// <returns>The object of <typeparamref name="T"/> type.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> does not match the value type or <typeparamref name="T"/> is a class without a parameterless constructor.</exception>
        T DequeueFirstOrDefault<T>(IColumnMapSetting settings);

        /// <summary>
        /// Removes and returns the data as a <see cref="IEnumerable{T}"/> collection.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <returns>The <see cref="IEnumerable{T}"/> result.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> does not match the value type or <typeparamref name="T"/> is a class without a parameterless constructor.</exception>
        IEnumerable<T> Dequeue<T>();

        /// <summary>
        /// Removes and returns the data as a <see cref="IEnumerable{T}"/> collection.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="settings">The <see cref="IColumnMapSetting"/> to use.</param>
        /// <returns>The <see cref="IEnumerable{T}"/> result.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> does not match the value type or <typeparamref name="T"/> is a class without a parameterless constructor.</exception>
        IEnumerable<T> Dequeue<T>(IColumnMapSetting settings);

        /// <summary>
        /// Returns the first element of the data collection.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="index">The index to use.</param>
        /// <returns>The object of <typeparamref name="T"/> type.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> does not match the value type or <typeparamref name="T"/> is a class without a parameterless constructor.</exception>
        T ElementAt<T>(int index);

        /// <summary>
        /// Returns the first element of the data collection.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="index">The index to use.</param>
        /// <param name="settings">The <see cref="IColumnMapSetting"/> to use.</param>
        /// <returns>The object of <typeparamref name="T"/> type.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> does not match the value type or <typeparamref name="T"/> is a class without a parameterless constructor.</exception>
        T ElementAt<T>(int index, IColumnMapSetting settings);

        /// <summary>
        /// Returns the data as a <see cref="IEnumerable{T}"/> collection.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="index">The index to use.</param>
        /// <returns>The <see cref="IEnumerable{T}"/> result.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> does not match the value type or <typeparamref name="T"/> is a class without a parameterless constructor.</exception>
        IEnumerable<T> ElementsAt<T>(int index);

        /// <summary>
        /// Returns the data as a <see cref="IEnumerable{T}"/> collection.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="index">The index to use.</param>
        /// <param name="settings">The <see cref="IColumnMapSetting"/> to use.</param>
        /// <returns>The <see cref="IEnumerable{T}"/> result.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> does not match the value type or <typeparamref name="T"/> is a class without a parameterless constructor.</exception>
        IEnumerable<T> ElementsAt<T>(int index, IColumnMapSetting settings);

    }


    public interface IDbJobState
    {
        IDbJobState Clone();

        IDbJobState<TStateParamValue> Clone<TStateParamValue>(TStateParamValue value);
    }

    public interface IDbJobState<TStateParam> : IDbJobState
    {
        TStateParam StateParam { get; set; }
    }

    internal interface IDbConnectorState
    {
        CalculatedDbConnectorFlags Flags { get; set; }
    }

    internal interface IDbConnectorState<T> : IDbConnectorState
    {
        T OnInit { get; set; }
    }

    internal interface IDbConnectorDynamicState : IDbConnectorState
    {
        int Count { get; set; }

        dynamic OnInit { get; set; }
    }


    public interface IDbExecutionModel : IDbExecutedModel
    {
        /// <summary>
        /// The DbConnection which will be disposed internally within a "finally" statement.
        /// </summary>
        DbConnection Connection { get; }

        /// <summary>
        /// The DbTransaction which will be "committed" (or "rolled back" on exceptions) automatically and disposed internally within a "finally" statement.
        /// </summary>
        DbTransaction Transaction { get; }

        /// <summary>
        /// <para>
        /// The <see cref="DbCommand"/> which will be disposed internally within a "finally" statement.        
        /// </para>
        ///  See also:
        ///  <seealso cref="DbConnection.CreateCommand"/>
        /// </summary>
        DbCommand Command { get; }

        /// <summary>
        /// The <see cref="IDbJobCommand"/> being executed.
        /// </summary>
        IDbJobCommand JobCommand { get; }

        /// <summary>
        /// Externally created <see cref="IDisposable"/> objects (e.g. <see cref="DbDataReader"/>) which have to be disposed internally within a "finally" statement.
        /// <para>This Queue has to be used in order to support the non-buffered or disposable execution of an <see cref="IDbJob{T}"/>.</para>
        /// <para>Note: This will be <see cref="null"/> for buffered/non-disposable <see cref="IDbJob{T}"/> executions. In this case, use either the <see cref="IDbExecutionModel.DeferDisposable(IDisposable)"/> function, a "try/finally" block, or an "using" statement instead.</para>
        /// </summary>
        Queue<IDisposable> DeferrableDisposables { get; }

        /// <summary>
        /// Used to defer the disposal of an <see cref="IDisposable"/> object to the current <see cref="IDbJob{T}"/>'s "finally" statement.
        /// <para>This can only be used for a single disposable, instead of the <see cref="IDbExecutionModel.DeferrableDisposables"/> property, in order to reduce memory usage for buffered/non-disposable <see cref="IDbJob{T}"/> execution.</para>
        /// <para>Note: Use either a "try/finally" block or an "using" statement instead.</para>
        /// </summary>
        /// <param name="disposable">The <see cref="IDisposable"/> object to defer.</param>
        void DeferDisposable(IDisposable disposable);

        /// <summary>
        /// The current <see cref="IDbJob{T}"/> state.
        /// </summary>
        IDbJobState JobState { get; }
    }

    public interface IDbExecutedModel
    {
        /// <summary>
        /// Return true if <see cref="IDbJob{T}"/> was executed as "disposable".
        /// </summary>
        bool IsDisposable { get; }

        bool IsBuffered { get; }

        /// <summary>
        /// The current executed <see cref="IDbJobCommand"/> index. This is internally assigned and can be used for tracking purposes.
        /// </summary>
        int Index { get; }

        int? NumberOfRowsAffected { get; set; }

        DbParameterCollection Parameters { get; set; }

        //IDbJobCommand JobCommand { get; }

        CancellationToken Token { get; }
    }

    public interface IDbExecutionModel<TStateParam> : IDbExecutionModel, IDbExecutedModel<TStateParam>
    {
    }

    public interface IDbExecutedModel<TStateParam> : IDbExecutedModel
    {
        TStateParam StateParam { get; }
    }


    public interface IDbResult<T>
    {
        bool HasError
        {
            get;
        }

        Exception Error { get; set; }

        T Data { get; set; }
    }


    /// <summary>
    /// An instance of this implementation needs to be disposed. Otherwise, the <see cref="DbConnection"/> will not be closed and other disposable objects will remain in memory.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    public interface IDbDisposable<T> : IDisposable
    {
        T Source { get; set; }

        /// <summary>
        /// The DbConnection of the DbJob which will be disposed internally when calling the <see cref="IDisposable.Dispose"/> function.
        /// </summary>
        //DbConnection Connection { get; }

        /// <summary>
        /// The DbTransaction of the DbJob which will be disposed internally when calling the <see cref="IDisposable.Dispose"/> function.
        /// </summary>
        //DbTransaction Transaction { get; }

        void Dispose(bool isCommitTransaction);
    }
}