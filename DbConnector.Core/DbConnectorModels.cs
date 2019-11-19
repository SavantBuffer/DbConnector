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
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace DbConnector.Core
{
    [Flags]
    public enum DbConnectorFlags : byte
    {
        None = 0,

        /// <summary>
        /// By default, the DbConnector will throw exeptions when using the non-handled execution <see cref="IDbJob{T}"/> functions.
        /// </summary>
        NoExceptionThrowingForNonHandledExecution = 1,

        /// <summary>
        /// By default, the DbConnector will optimize all <see cref="DbCommand"/> read executions by configuring the <see cref="CommandBehavior.SequentialAccess"/> to reduce memory usage.
        /// </summary>
        NoAutoSequentialAccessCommandBehavior = 2,

        /// <summary>
        /// By default, the DbConnector will optimize all <see cref="DbCommand"/> executions by configuring the <see cref="CommandBehavior"/>.
        /// </summary>
        NoCommandBehaviorOptimization = 4,

        /// <summary>
        /// By default, one database connection per command will be created/opened thus potentially returning a faster result.
        /// </summary>
        NoIsolatedConnectionPerCommand = 8,

        /// <summary>
        /// By default, the DbConnector will cache all <see cref="DbConnection"/> instance builders for faster executions.
        /// <para>Note: This flag should, on rare occasions, only be used to disable this feature when running an <see cref="IDbConnector{TDbConnection}"/> just once. 
        /// Also, the <see cref="DbConnectorFlags.NoCache"/> flag has higher priority and will disable this feature if used.</para>
        /// </summary>
        NoDbConnectionInstanceBuilderCaching = 16,

        /// <summary>
        /// Use this flag to "globally" disable the caching of query mappings and types.
        /// </summary>
        NoCache = 32
    }

    [Flags]
    public enum DbJobCommandFlags : byte
    {
        None = 0,

        /// <summary>
        /// By default, the DbConnector will optimize all <see cref="DbCommand"/> read executions by configuring the <see cref="CommandBehavior.SequentialAccess"/> to reduce memory usage.
        /// <para>Use this flag to disable this behavior for a specific <see cref="DbJobCommand"/>.</para>
        /// </summary>
        NoAutoSequentialAccessCommandBehavior = 2,

        /// <summary>
        /// Use this flag to disable the caching of query mappings and types for a specific <see cref="DbJobCommand"/>.
        /// </summary>
        NoCache = 16,
    }

    public enum EventSetting : byte
    {
        Subscribe = 0,
        Unsubscribe = 1,
        Replace = 2
    }

    internal enum MultiReaderTypes : byte
    {
        Read = 0,
        ReadToList = 1,
        ReadFirst = 2,
        ReadFirstOrDefault = 4,
        ReadSingle = 8,
        ReadSingleOrDefault = 16
    }

    public readonly struct CalculatedDbConnectorFlags
    {
        public readonly bool IsCommandBehaviorOptimization;

        public readonly bool IsAutoSequentialAccessCommandBehavior;

        public readonly bool IsIsolatedConnectionPerCommand;

        public CalculatedDbConnectorFlags(
            bool isCommandBehaviorOptimization,
            bool isAutoSequentialAccessCommandBehavior,
            bool isIsolatedConnectionPerCommand)
        {
            IsCommandBehaviorOptimization = isCommandBehaviorOptimization;
            IsAutoSequentialAccessCommandBehavior = isAutoSequentialAccessCommandBehavior;
            IsIsolatedConnectionPerCommand = isIsolatedConnectionPerCommand;
        }
    }


    public class DbJobCommand : IDbJobCommand
    {
        private readonly DbCommand _dbCommand;

        /// <summary>
        /// Get the referenced <see cref="DbParameterCollection"/>. This can be useful when wanting to cast and insert type specific parameters.
        /// </summary>
        /// <returns><see cref="DbParameterCollection"/></returns>
        public DbParameterCollection GetDbParameters() { return _dbCommand.Parameters; }

        /// <summary>
        /// Get the referenced <see cref="DbCommand"/>.
        /// </summary>
        /// <returns><see cref="DbCommand"/></returns>
        public DbCommand GetDbCommand() { return _dbCommand; }

        public DbJobCommand(DbCommand dbCommand)
        {
            //Init param list
            this._dbCommand = dbCommand;
            this.Parameters = new DbJobParameterCollection(dbCommand);
            this.MapSettings = new ColumnMapSetting();
        }

        public DbJobCommand(DbCommand dbCommand, IColumnMapSetting mapSettings)
        {
            //Init param list
            this._dbCommand = dbCommand;
            this.Parameters = new DbJobParameterCollection(dbCommand);
            this.MapSettings = mapSettings ?? new ColumnMapSetting();
        }

        /// <summary>
        /// Use to set the command type and override the default Text CommandType.
        /// </summary>
        public CommandType CommandType { get { return _dbCommand.CommandType; } set { _dbCommand.CommandType = value; } }

        /// <summary>
        /// Gets or sets the text command to run against the data source.
        /// </summary>
        public string CommandText { get { return _dbCommand.CommandText; } set { _dbCommand.CommandText = value; } }

        public CommandBehavior? CommandBehavior { get; set; }

        /// <summary>
        /// The time in seconds to wait for the command to execute.
        /// </summary>
        public int CommandTimeout { get { return _dbCommand.CommandTimeout; } set { _dbCommand.CommandTimeout = value; } }

        public DbJobParameterCollection Parameters { get; private set; }

        public IColumnMapSetting MapSettings { get; private set; }

        public DbJobCommandFlags Flags { get; set; }
    }

    public class DbJobCommand<TStateParam> : DbJobCommand, IDbJobCommand<TStateParam>
    {
        public TStateParam StateParam { get; }

        public DbJobCommand(DbCommand dbCommand, TStateParam stateParam)
            : base(dbCommand)
        {
            StateParam = stateParam;
        }

        public DbJobCommand(DbCommand dbCommand, TStateParam stateParam, IColumnMapSetting mapSettings)
            : base(dbCommand, mapSettings)
        {
            StateParam = stateParam;
        }
    }


    internal class DbJobSetting : IDbJobSetting
    {
        public string ConnectionString { get; set; }

        public bool IsThrowExceptions { get; set; }

        public bool IsCacheEnabled { get; set; }

        public bool IsCloningEnabled { get; set; }

        public Type DbConnectionType { get; set; }

        public IDbConnectorLogger Logger { get; set; }

        public IDbJobSetting Clone()
        {
            return this;
        }
    }


    public class DbJobParameterCollection : DbParameterCollection
    {
        internal readonly DbCommand _dbCommand;

        internal readonly DbParameterCollection _collection;

        /// <summary>
        /// Get the referenced <see cref="DbParameterCollection"/>. This can be useful when wanting to cast and insert type specific parameters.
        /// </summary>
        /// <returns><see cref="DbParameterCollection"/></returns>
        public DbParameterCollection GetDbParameters()
        {
            return _collection;
        }

        public DbJobParameterCollection(DbCommand dbCommand)
        {
            _dbCommand = dbCommand;
            _collection = dbCommand.Parameters;
        }

        public DbParameter Add(string parameterName, DbType dbType)
        {
            var toAdd = _dbCommand.CreateParameter();

            toAdd.ParameterName = parameterName;
            //toAdd.Direction = ParameterDirection.Input;
            toAdd.DbType = dbType;

            _collection.Add(toAdd);

            return toAdd;
        }

        public DbParameter Add(string parameterName, DbType dbType, int size)
        {
            var toAdd = _dbCommand.CreateParameter();

            toAdd.ParameterName = parameterName;
            toAdd.Size = size;
            //toAdd.Direction = ParameterDirection.Input;
            toAdd.DbType = dbType;

            _collection.Add(toAdd);

            return toAdd;
        }

        public DbParameter Add(string parameterName, DbType dbType, int size, string sourceColumn)
        {
            var toAdd = _dbCommand.CreateParameter();

            toAdd.ParameterName = parameterName;
            toAdd.Size = size;
            toAdd.SourceColumn = sourceColumn;
            //toAdd.Direction = ParameterDirection.Input;
            toAdd.DbType = dbType;

            _collection.Add(toAdd);

            return toAdd;
        }

        public DbParameter AddWithValue(string parameterName, object value)
        {
            var toAdd = _dbCommand.CreateParameter();

            toAdd.ParameterName = parameterName;
            toAdd.Value = value ?? DBNull.Value;
            //toAdd.Direction = ParameterDirection.Input;

            _collection.Add(toAdd);

            return toAdd;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal void AddParameterByEnumArray<T, T2>(Type enumUnderlyingType, T[] values, DbParameter toAdd)
        {
            var valuesToInsert = new T2[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                valuesToInsert[i] = (T2)Convert.ChangeType(values[i], enumUnderlyingType);
            }

            toAdd.Value = valuesToInsert;
        }

        /// <summary>
        /// Add parameters using the properties of the <paramref name="param"/> object. These default to <see cref="ParameterDirection.Input"/>.
        /// <para>Valid <typeparamref name="T"/> types: anonymous, or any struct or class that is not a .NET built-in type and is not assignable from <see cref="System.Collections.IEnumerable"/> or <see cref="IListSource"/>.</para>
        /// <para><see cref="System.Enum"/> properties, including <see cref="System.Enum"/> arrays, will be changed to the applicable underlying type.</para>
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="param">The object whose properties will be mapped.</param>
        /// <param name="isUseColumnAttribute">Set to false to not use the <see cref="System.ComponentModel.DataAnnotations.Schema.ColumnAttribute"/> for names. (Optional)</param>
        /// <param name="paramsPrefix">The prefix to use for all column names. (Optional)</param>
        /// <param name="paramsSuffix">The suffix to use for all column names. (Optional)</param>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> is .NET built-in type.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> is assignable from <see cref="System.Collections.IEnumerable"/>.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> is assignable from <see cref="IListSource"/>.</exception>
        public void AddFor<T>(T param, bool isUseColumnAttribute = true, string paramsPrefix = null, string paramsSuffix = null)
        {
            if (param != null)
            {
                ParameterCacheModel cacheModel = new ParameterCacheModel(typeof(T), isUseColumnAttribute, paramsPrefix, paramsSuffix);

                if (!DbConnectorCache.ParameterCache.TryGetValue(cacheModel, out Action<DbJobParameterCollection, object> onAddFor))
                {
                    onAddFor = DynamicParameterBuilder.CreateBuilderAction(param, typeof(T), this, isUseColumnAttribute, paramsPrefix, paramsSuffix);

                    DbConnectorCache.ParameterCache.TryAdd(cacheModel, onAddFor);
                }
                else
                {
                    onAddFor(this, param);
                }
            }
        }

        /// <summary>
        /// Add parameters using the properties of the <paramref name="param"/> object. These default to <see cref="ParameterDirection.Input"/>.
        /// <para>Valid object types: anonymous, or any struct or class that is not a .NET built-in type and is not assignable from <see cref="System.Collections.IEnumerable"/> or <see cref="IListSource"/>.</para>
        /// <para><see cref="System.Enum"/> properties, including <see cref="System.Enum"/> arrays, will be changed to the applicable underlying type.</para>
        /// </summary>
        /// <param name="param">The object whose properties will be mapped.</param>
        /// <param name="isUseColumnAttribute">Set to false to not use the <see cref="System.ComponentModel.DataAnnotations.Schema.ColumnAttribute"/> for names. (Optional)</param>
        /// <param name="paramsPrefix">The prefix to use for all column names. (Optional)</param>
        /// <param name="paramsSuffix">The suffix to use for all column names. (Optional)</param>
        /// <exception cref="System.InvalidCastException">Thrown when object type is .NET built-in type.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when object type is assignable from <see cref="System.Collections.IEnumerable"/>.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when object type is assignable from <see cref="IListSource"/>.</exception>
        public void AddFor(object param, bool isUseColumnAttribute = true, string paramsPrefix = null, string paramsSuffix = null)
        {
            if (param != null)
            {
                ParameterCacheModel cacheModel = new ParameterCacheModel(param.GetType(), isUseColumnAttribute, paramsPrefix, paramsSuffix);

                if (!DbConnectorCache.ParameterCache.TryGetValue(cacheModel, out Action<DbJobParameterCollection, object> onAddFor))
                {
                    onAddFor = DynamicParameterBuilder.CreateBuilderAction(param, param.GetType(), this, isUseColumnAttribute, paramsPrefix, paramsSuffix);

                    DbConnectorCache.ParameterCache.TryAdd(cacheModel, onAddFor);
                }
                else
                {
                    onAddFor(this, param);
                }
            }
        }

        //
        // Summary:
        //     Specifies whether the collection is synchronized.
        //
        // Returns:
        //     true if the collection is synchronized; otherwise false.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool IsSynchronized { get { return _collection.IsSynchronized; } }

        //
        // Summary:
        //     Specifies whether the collection is read-only.
        //
        // Returns:
        //     true if the collection is read-only; otherwise false.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool IsReadOnly { get { return _collection.IsReadOnly; } }

        //
        // Summary:
        //     Specifies whether the collection is a fixed size.
        //
        // Returns:
        //     true if the collection is a fixed size; otherwise false.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool IsFixedSize { get { return _collection.IsFixedSize; } }

        //
        // Summary:
        //     Specifies the number of items in the collection.
        //
        // Returns:
        //     The number of items in the collection.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override int Count { get { return _collection.Count; } }

        //
        // Summary:
        //     Specifies the System.Object to be used to synchronize access to the collection.
        //
        // Returns:
        //     A System.Object to be used to synchronize access to the System.Data.Common.DbParameterCollection.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override object SyncRoot { get { return _collection.SyncRoot; } }

        //
        // Summary:
        //     Gets and sets the System.Data.Common.DbParameter at the specified index.
        //
        // Parameters:
        //   index:
        //     The zero-based index of the parameter.
        //
        // Returns:
        //     The System.Data.Common.DbParameter at the specified index.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The specified index does not exist.
        public new DbParameter this[int index] { get { return _collection[index]; } set { _collection[index] = value; } }

        //
        // Summary:
        //     Gets and sets the System.Data.Common.DbParameter with the specified name.
        //
        // Parameters:
        //   parameterName:
        //     The name of the parameter.
        //
        // Returns:
        //     The System.Data.Common.DbParameter with the specified name.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The specified index does not exist.
        public new DbParameter this[string parameterName] { get { return _collection[parameterName]; } set { _collection[parameterName] = value; } }


        //
        // Summary:
        //     Adds the specified System.Data.Common.DbParameter object to the System.Data.Common.DbParameterCollection.
        //
        // Parameters:
        //   value:
        //     The System.Data.Common.DbParameter.Value of the System.Data.Common.DbParameter
        //     to add to the collection.
        //
        // Returns:
        //     The index of the System.Data.Common.DbParameter object in the collection.
        public override int Add(object value)
        {
            return _collection.Add(value);
        }

        //
        // Summary:
        //     Adds an array of items with the specified values to the System.Data.Common.DbParameterCollection.
        //
        // Parameters:
        //   values:
        //     An array of values of type System.Data.Common.DbParameter to add to the collection.
        public override void AddRange(Array values)
        {
            _collection.AddRange(values);
        }

        //
        // Summary:
        //     Removes all System.Data.Common.DbParameter values from the System.Data.Common.DbParameterCollection.
        public override void Clear()
        {
            _collection.Clear();
        }

        //
        // Summary:
        //     Indicates whether a System.Data.Common.DbParameter with the specified name exists
        //     in the collection.
        //
        // Parameters:
        //   value:
        //     The name of the System.Data.Common.DbParameter to look for in the collection.
        //
        // Returns:
        //     true if the System.Data.Common.DbParameter is in the collection; otherwise false.
        public override bool Contains(object value)
        {
            return _collection.Contains(value);
        }

        //
        // Summary:
        //     Indicates whether a System.Data.Common.DbParameter with the specified System.Data.Common.DbParameter.Value
        //     is contained in the collection.
        //
        // Parameters:
        //   value:
        //     The System.Data.Common.DbParameter.Value of the System.Data.Common.DbParameter
        //     to look for in the collection.
        //
        // Returns:
        //     true if the System.Data.Common.DbParameter is in the collection; otherwise false.
        public override bool Contains(string parameterName)
        {
            return _collection.Contains(parameterName);
        }

        //
        // Summary:
        //     Copies an array of items to the collection starting at the specified index.
        //
        // Parameters:
        //   array:
        //     The array of items to copy to the collection.
        //
        //   index:
        //     The index in the collection to copy the items.
        public override void CopyTo(Array array, int index)
        {
            _collection.CopyTo(array, index);
        }

        //
        // Summary:
        //     Exposes the System.Collections.IEnumerable.GetEnumerator method, which supports
        //     a simple iteration over a collection by a .NET Framework data provider.
        //
        // Returns:
        //     An System.Collections.IEnumerator that can be used to iterate through the collection.
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override IEnumerator GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        //
        // Summary:
        //     Returns the index of the specified System.Data.Common.DbParameter object.
        //
        // Parameters:
        //   value:
        //     The System.Data.Common.DbParameter object in the collection.
        //
        // Returns:
        //     The index of the specified System.Data.Common.DbParameter object.
        public override int IndexOf(object value)
        {
            return _collection.IndexOf(value);
        }

        //
        // Summary:
        //     Returns the index of the System.Data.Common.DbParameter object with the specified
        //     name.
        //
        // Parameters:
        //   parameterName:
        //     The name of the System.Data.Common.DbParameter object in the collection.
        //
        // Returns:
        //     The index of the System.Data.Common.DbParameter object with the specified name.
        public override int IndexOf(string parameterName)
        {
            return _collection.IndexOf(parameterName);
        }

        //
        // Summary:
        //     Inserts the specified index of the System.Data.Common.DbParameter object with
        //     the specified name into the collection at the specified index.
        //
        // Parameters:
        //   index:
        //     The index at which to insert the System.Data.Common.DbParameter object.
        //
        //   value:
        //     The System.Data.Common.DbParameter object to insert into the collection.
        public override void Insert(int index, object value)
        {
            _collection.Insert(index, value);
        }

        //
        // Summary:
        //     Removes the specified System.Data.Common.DbParameter object from the collection.
        //
        // Parameters:
        //   value:
        //     The System.Data.Common.DbParameter object to remove.
        public override void Remove(object value)
        {
            _collection.Remove(value);
        }

        //
        // Summary:
        //     Removes the System.Data.Common.DbParameter object at the specified from the collection.
        //
        // Parameters:
        //   index:
        //     The index where the System.Data.Common.DbParameter object is located.
        public override void RemoveAt(int index)
        {
            _collection.RemoveAt(index);
        }

        // Summary:
        //     Removes the System.Data.Common.DbParameter object with the specified name from
        //     the collection.
        //
        // Parameters:
        //   parameterName:
        //     The name of the System.Data.Common.DbParameter object to remove.
        public override void RemoveAt(string parameterName)
        {
            _collection.RemoveAt(parameterName);
        }

        protected override DbParameter GetParameter(int index)
        {
            return _collection[index];
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            return _collection[parameterName];
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            _collection[index] = value;
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            _collection[parameterName] = value;
        }
    }


    public class ColumnParentMap
    {
        public MethodInfo SetMethod { get; set; }

        public PropertyInfo PropInfo { get; set; }

        public IEnumerable<ColumnMap> Children { get; set; }
    }


    public class ColumnMap
    {
        public ColumnParentMap ParentMap { get; set; }

        public bool IsChildMap { get { return ParentMap != null; } }

        public Type UnderlyingType { get; set; }

        public MethodInfo SetMethod { get; set; }

        public PropertyInfo PropInfo { get; set; }

        public OrdinalColumnMap Column { get; set; }
    }


    public class OrdinalColumnMap
    {
        public int Ordinal { get; set; }
        public string Name { get; set; }
        public Type FieldType { get; set; }
        protected internal bool IsMapped { get; set; }
    }


    internal class OrdinalColumnMapLite
    {
        public int Ordinal { get; set; }
        public string Name { get; set; }
    }


    public enum ColumnAliasMode : byte
    {
        Equals = 0,
        StartsWith = 1,
        EndsWith = 2
    }


    public class ColumnMapSetting : IColumnMapSetting
    {
        public ConcurrentBag<string> NamesToInclude { get; internal set; }

        public ConcurrentBag<string> NamesToExclude { get; internal set; }

        public ConcurrentDictionary<Type, string> Joins { get; internal set; }

        public ConcurrentDictionary<Type, Dictionary<string, string>> Aliases { get; internal set; }

        public bool HasNamesToInclude { get { return NamesToInclude?.IsEmpty == false; } }

        public bool HasNamesToExclude { get { return NamesToExclude?.IsEmpty == false; } }

        public bool HasAliases { get { return Aliases?.IsEmpty == false; } }

        public bool HasJoins { get { return Joins?.IsEmpty == false; } }

        /// <summary>
        /// Set the starting/locator inclusive column name to use when mapping an object of <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">The object type to use.</typeparam>
        /// <param name="columnName">The column name to use.</param>
        /// <returns>The current <see cref="IColumnMapSetting"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="columnName"/> is null.</exception>
        public IColumnMapSetting WithSplitOn<T>(string columnName)
        {
            return WithSplitOn(typeof(T), columnName);
        }

        /// <summary>
        /// Set the starting/locator inclusive column name to use when mapping an object of <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">The object type to use.</typeparam>
        /// <param name="expression">The expression to use.</param>
        /// <param name="isUseColumnAttribute">Set to false to not use the <see cref="System.ComponentModel.DataAnnotations.Schema.ColumnAttribute"/> for names. (Optional)</param>
        /// <returns>The current <see cref="IColumnMapSetting"/>.</returns>
        /// <exception cref="MemberAccessException">Thrown when the provided expression's property is not a member of <typeparamref name="T"/>.</exception>
        public IColumnMapSetting WithSplitOnFor<T>(Expression<Func<T, object>> expression, bool isUseColumnAttribute = true)
        {
            Type tType = typeof(T);
            string propName = expression.Body.GetMemberName();

            var p = tType.GetProperty(propName);

            if (p != null)
            {
                string keyName = isUseColumnAttribute ? p.GetColumnAttributeName() : p.Name;

                return WithSplitOn(tType, keyName);
            }
            else
            {
                throw new MemberAccessException("Failed to find property " + propName + " in type " + tType);
            }
        }

        /// <summary>
        /// Set the starting/locator inclusive column name to use when mapping an object of the specified type.
        /// </summary>
        /// <param name="tType">The object <see cref="Type"/> to use.</param>
        /// <param name="columnName">The column name to use.</param>
        /// <returns>The current <see cref="IColumnMapSetting"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="columnName"/> is null.</exception>
        public IColumnMapSetting WithSplitOn(Type tType, string columnName)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException("columnName");
            }

            if (Joins == null)
            {
                Joins = new ConcurrentDictionary<Type, string>();
            }

            Joins.TryAdd(tType, columnName);

            return this;
        }

        public IColumnMapSetting WithAlias<T>(string alias, ColumnAliasMode mode, bool isUseColumnAttribute = true)
        {
            return WithAlias(typeof(T), alias, mode, isUseColumnAttribute);
        }

        public IColumnMapSetting WithAlias(Type tType, string alias, ColumnAliasMode mode, bool isUseColumnAttribute = true)
        {
            if (Aliases == null)
            {
                Aliases = new ConcurrentDictionary<Type, Dictionary<string, string>>();
            }

            Dictionary<string, string> aliasMap = new Dictionary<string, string>();

            var props = tType.GetProperties(DbConnectorUtilities._bindingFlagInstancePublic);

            foreach (var p in props)
            {
                string keyName = isUseColumnAttribute ? p.GetColumnAttributeName() : p.Name;

                switch (mode)
                {
                    case ColumnAliasMode.Equals:
                        aliasMap.Add(keyName, alias);
                        break;
                    case ColumnAliasMode.StartsWith:
                        aliasMap.Add(keyName, alias + keyName);
                        break;
                    case ColumnAliasMode.EndsWith:
                        aliasMap.Add(keyName, keyName + alias);
                        break;
                    default:
                        break;
                }
            }

            Aliases.TryAdd(tType, aliasMap);

            return this;
        }

        public IColumnMapSetting WithAliasFor<T>(string alias, ColumnAliasMode mode, bool isUseColumnAttribute, params string[] propertyNames)
        {
            return WithAliasFor(typeof(T), alias, mode, isUseColumnAttribute, propertyNames);
        }

        public IColumnMapSetting WithAliasFor(Type tType, string alias, ColumnAliasMode mode, bool isUseColumnAttribute, params string[] propertyNames)
        {
            return WithAliasFor(tType, alias, mode, isUseColumnAttribute, propertyNames as IEnumerable<string>);
        }

        public IColumnMapSetting WithAliasFor<T>(string alias, ColumnAliasMode mode, bool isUseColumnAttribute, IEnumerable<string> propertyNames)
        {
            return WithAliasFor(typeof(T), alias, mode, isUseColumnAttribute, propertyNames);
        }

        public IColumnMapSetting WithAliasFor(Type tType, string alias, ColumnAliasMode mode, bool isUseColumnAttribute, IEnumerable<string> propertyNames)
        {
            if (Aliases == null)
            {
                Aliases = new ConcurrentDictionary<Type, Dictionary<string, string>>();
            }

            bool isContainsType = false;
            Dictionary<string, string> aliasMap = new Dictionary<string, string>();

            if (Aliases.ContainsKey(tType))
            {
                aliasMap = Aliases[tType];
                isContainsType = true;
            }


            foreach (var propName in propertyNames)
            {
                var p = tType.GetProperty(propName);

                if (p != null)
                {
                    string keyName = isUseColumnAttribute ? p.GetColumnAttributeName() : p.Name;

                    if (!aliasMap.ContainsKey(keyName))
                    {
                        switch (mode)
                        {
                            case ColumnAliasMode.Equals:
                                aliasMap.Add(keyName, alias);
                                break;
                            case ColumnAliasMode.StartsWith:
                                aliasMap.Add(keyName, alias + keyName);
                                break;
                            case ColumnAliasMode.EndsWith:
                                aliasMap.Add(keyName, keyName + alias);
                                break;
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    throw new MemberAccessException("Failed to find property " + propName + " in type " + tType);
                }
            }


            if (aliasMap.Count > 0 && !isContainsType)
            {
                Aliases.TryAdd(tType, aliasMap);
            }

            return this;
        }

        public IColumnMapSetting WithAliasFor<T>(string alias, ColumnAliasMode mode, bool isUseColumnAttribute, params Expression<Func<T, object>>[] expressions)
        {
            if (Aliases == null)
            {
                Aliases = new ConcurrentDictionary<Type, Dictionary<string, string>>();
            }

            Type tType = typeof(T);
            bool isContainsType = false;
            Dictionary<string, string> aliasMap = new Dictionary<string, string>();

            if (Aliases.ContainsKey(tType))
            {
                aliasMap = Aliases[tType];
                isContainsType = true;
            }


            for (int i = 0; i < expressions.Length; i++)
            {
                string propName = expressions[i].Body.GetMemberName();

                var p = tType.GetProperty(propName);

                if (p != null)
                {
                    string keyName = isUseColumnAttribute ? p.GetColumnAttributeName() : p.Name;

                    if (!aliasMap.ContainsKey(keyName))
                    {
                        switch (mode)
                        {
                            case ColumnAliasMode.Equals:
                                aliasMap.Add(keyName, alias);
                                break;
                            case ColumnAliasMode.StartsWith:
                                aliasMap.Add(keyName, alias + keyName);
                                break;
                            case ColumnAliasMode.EndsWith:
                                aliasMap.Add(keyName, keyName + alias);
                                break;
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    throw new MemberAccessException("Failed to find property " + propName + " in type " + tType);
                }
            }


            if (aliasMap.Count > 0 && !isContainsType)
            {
                Aliases.TryAdd(tType, aliasMap);
            }

            return this;
        }

        public IColumnMapSetting IncludeNamesFor<T>(bool isUseColumnAttribute, params Expression<Func<T, object>>[] expressions)
        {
            Type tType = typeof(T);

            if (NamesToInclude == null)
            {
                NamesToInclude = new ConcurrentBag<string>();
            }

            for (int i = 0; i < expressions.Length; i++)
            {
                string propName = expressions[i].Body.GetMemberName();

                var p = tType.GetProperty(propName);

                if (p != null)
                {
                    string cName = isUseColumnAttribute ? p.GetColumnAttributeName() : p.Name;

                    if (!NamesToInclude.Contains(cName))
                    {
                        NamesToInclude.Add(cName);
                    }
                }
                else
                {
                    throw new MemberAccessException("Failed to find property " + propName + " in type " + tType);
                }
            }

            return this;
        }

        public IColumnMapSetting IncludeNames(params string[] columnNamesToInclude)
        {
            return IncludeNames(columnNamesToInclude as IEnumerable<string>);
        }

        public IColumnMapSetting IncludeNames(IEnumerable<string> columnNamesToInclude)
        {
            if (NamesToInclude == null)
            {
                NamesToInclude = new ConcurrentBag<string>();
            }

            foreach (var cName in columnNamesToInclude)
            {
                if (!NamesToInclude.Contains(cName))
                {
                    NamesToInclude.Add(cName);
                }
            }

            return this;
        }

        public IColumnMapSetting ExcludeNamesFor<T>(bool isUseColumnAttribute, params Expression<Func<T, object>>[] expressions)
        {
            Type tType = typeof(T);

            if (NamesToExclude == null)
            {
                NamesToExclude = new ConcurrentBag<string>();
            }

            for (int i = 0; i < expressions.Length; i++)
            {
                string propName = expressions[i].Body.GetMemberName();

                var p = tType.GetProperty(propName);

                if (p != null)
                {
                    string cName = isUseColumnAttribute ? p.GetColumnAttributeName() : p.Name;

                    if (!NamesToExclude.Contains(cName))
                    {
                        NamesToExclude.Add(cName);
                    }
                }
                else
                {
                    throw new MemberAccessException("Failed to find property " + propName + " in type " + tType);
                }
            }

            return this;
        }

        public IColumnMapSetting ExcludeNames(params string[] columnNamesToExclude)
        {
            return ExcludeNames(columnNamesToExclude as IEnumerable<string>);
        }

        public IColumnMapSetting ExcludeNames(IEnumerable<string> columnNamesToExclude)
        {
            if (NamesToExclude == null)
            {
                NamesToExclude = new ConcurrentBag<string>();
            }

            foreach (var cName in columnNamesToExclude)
            {
                if (!NamesToExclude.Contains(cName))
                {
                    NamesToExclude.Add(cName);
                }
            }

            return this;
        }
    }


    public class DbCollectionSet : IDbCollectionSet
    {
        public List<List<Dictionary<string, object>>> Items { get; internal set; }

        public IEnumerable<ColumnMap> GetColumnMaps<T>(int index)
        {
            Type tType = typeof(T);

            if (
                DbConnectorUtilities._directTypeMap.Contains(tType)
                || (tType.IsValueType && (tType.IsEnum || (Nullable.GetUnderlyingType(tType)?.IsEnum ?? false)))
                || tType.IsArray
             )
            {
                throw new InvalidCastException("The type " + tType + " is not supported");
            }

            return GetColumnMaps(tType, index, null);
        }

        public IEnumerable<ColumnMap> GetColumnMaps<T>(int index, IColumnMapSetting settings)
        {
            Type tType = typeof(T);

            if (
                DbConnectorUtilities._directTypeMap.Contains(tType)
                || (tType.IsValueType && (tType.IsEnum || (Nullable.GetUnderlyingType(tType)?.IsEnum ?? false)))
                || tType.IsArray
             )
            {
                throw new InvalidCastException("The type " + tType + " is not supported");
            }

            return GetColumnMaps(tType, index, settings);
        }

        public IEnumerable<ColumnMap> GetColumnMaps(Type tType, int index)
        {
            if (
                DbConnectorUtilities._directTypeMap.Contains(tType)
                || (tType.IsValueType && (tType.IsEnum || (Nullable.GetUnderlyingType(tType)?.IsEnum ?? false)))
                || tType.IsArray
             )
            {
                throw new InvalidCastException("The type " + tType + " is not supported");
            }

            return GetColumnMaps(tType, index, null);
        }

        private IEnumerable<ColumnMap> GetColumnMaps(Type tType, int index, IColumnMapSetting settings)
        {
            if (typeof(IEnumerable).IsAssignableFrom(tType))
            {
                throw new InvalidCastException("The type " + tType + " is not supported");
            }
            else if (tType.IsClass && tType.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new InvalidCastException("The type " + tType + " is missing a parameterless constructor");
            }

            var data = Items.ElementAtOrDefault(index);

            if (data?.Count > 0)
            {
                var item = data.First();
                var keys = item.Keys;
                var values = item.Values;

                OrdinalColumnMap[] ordinalColumnMap = null;

                if (settings == null || (!settings.HasNamesToInclude && !settings.HasNamesToExclude))
                {
                    ordinalColumnMap = new OrdinalColumnMap[keys.Count];

                    for (int i = 0; i < keys.Count; i++)
                    {
                        ordinalColumnMap[i] = new OrdinalColumnMap { Ordinal = i, Name = keys.ElementAt(i), FieldType = values.ElementAt(i).GetType() };
                    }
                }
                else
                {
                    bool hasNamesToInclude = settings.HasNamesToInclude;
                    bool hasNamesToExclude = settings.HasNamesToExclude;

                    var tempMap = new Queue<OrdinalColumnMap>(keys.Count);

                    for (int i = 0; i < keys.Count; i++)
                    {
                        string colName = keys.ElementAt(i);

                        if (!DbConnectorUtilities.IsColumnNameExcluded(colName, hasNamesToInclude, hasNamesToExclude, settings))
                        {
                            tempMap.Enqueue(new OrdinalColumnMap { Ordinal = i, Name = colName, FieldType = values.ElementAt(i).GetType() });
                        }
                    }

                    if (tempMap.Count > 0)
                    {
                        ordinalColumnMap = new OrdinalColumnMap[tempMap.Count];

                        for (int i = 0; tempMap.Count > 0; i++)
                        {
                            ordinalColumnMap[i] = tempMap.Dequeue();
                        }
                    }
                }

                return DbConnectorUtilities.GetColumnMaps(tType, ordinalColumnMap, settings);
            }

            return Enumerable.Empty<ColumnMap>();
        }

        public IEnumerable<string> GetColumnNames(int index)
        {
            var data = Items.ElementAtOrDefault(index);

            if (data?.Count > 0)
            {
                return data.First().Keys.AsEnumerable();
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }

        public DataSet ToDataSet(bool isDequeueEnabled = true)
        {
            return ToDataSet(CancellationToken.None, isDequeueEnabled);
        }

        public DataSet ToDataSet(CancellationToken token, bool isDequeueEnabled = true)
        {
            var projectedData = new DataSet();

            var dataIndex = 0;
            while (dataIndex < Items.Count)
            {
                var data = Items[dataIndex];

                if (token.IsCancellationRequested)
                    return projectedData;

                DataTable dt = new DataTable();

                var colNames = data.First().Keys.ToArray();

                if (colNames.Length > 0)
                {
                    foreach (var c in colNames)
                    {
                        dt.Columns.Add(c);
                    }

                    foreach (var item in data)
                    {
                        DataRow row = dt.NewRow();

                        for (int i = 0; i < item.Count; i++)
                        {
                            row[i] = item.ElementAt(i).Value;
                        }

                        dt.Rows.Add(row);

                        if (token.IsCancellationRequested)
                            break;
                    }

                    projectedData.Tables.Add(dt);
                }

                if (isDequeueEnabled)
                {
                    Items.RemoveAt(dataIndex);
                }
                else
                {
                    dataIndex++;
                }
            }


            return projectedData;
        }

        /// <summary>
        /// Removes and returns the first element of the data collection.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <returns>The object of <typeparamref name="T"/> type.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> does not match the value type or <typeparamref name="T"/> is a class without a parameterless constructor.</exception>
        public T DequeueFirstOrDefault<T>()
        {
            T obj = ElementsAt<T>(0, true, null).FirstOrDefault();

            return obj;
        }

        /// <summary>
        /// Removes and returns the first element of the data collection.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="settings">The <see cref="IColumnMapSetting"/> to use.</param>
        /// <returns>The object of <typeparamref name="T"/> type.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> does not match the value type or <typeparamref name="T"/> is a class without a parameterless constructor.</exception>
        public T DequeueFirstOrDefault<T>(IColumnMapSetting settings)
        {
            T obj = ElementsAt<T>(0, true, settings).FirstOrDefault();

            return obj;
        }

        /// <summary>
        /// Removes and returns the data as a <see cref="IEnumerable{T}"/> collection.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <returns>The <see cref="IEnumerable{T}"/> result.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> does not match the value type or <typeparamref name="T"/> is a class without a parameterless constructor.</exception>
        public IEnumerable<T> Dequeue<T>()
        {
            return ElementsAt<T>(0, true, null);
        }

        /// <summary>
        /// Removes and returns the data as a <see cref="IEnumerable{T}"/> collection.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="settings">The <see cref="IColumnMapSetting"/> to use.</param>
        /// <returns>The <see cref="IEnumerable{T}"/> result.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> does not match the value type or <typeparamref name="T"/> is a class without a parameterless constructor.</exception>
        public IEnumerable<T> Dequeue<T>(IColumnMapSetting settings)
        {
            return ElementsAt<T>(0, true, settings);
        }

        /// <summary>
        /// Returns the first element of the data collection.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="index">The index to use.</param>
        /// <returns>The object of <typeparamref name="T"/> type.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> does not match the value type or <typeparamref name="T"/> is a class without a parameterless constructor.</exception>
        public T ElementAt<T>(int index)
        {
            T obj = ElementsAt<T>(index, false, null).FirstOrDefault();

            return obj;
        }

        /// <summary>
        /// Returns the first element of the data collection.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="index">The index to use.</param>
        /// <param name="settings">The <see cref="IColumnMapSetting"/> to use.</param>
        /// <returns>The object of <typeparamref name="T"/> type.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> does not match the value type or <typeparamref name="T"/> is a class without a parameterless constructor.</exception>
        public T ElementAt<T>(int index, IColumnMapSetting settings)
        {
            T obj = ElementsAt<T>(index, false, settings).FirstOrDefault();

            return obj;
        }

        /// <summary>
        /// Returns the data as a <see cref="IEnumerable{T}"/> collection.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="index">The index to use.</param>
        /// <returns>The <see cref="IEnumerable{T}"/> result.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> does not match the value type or <typeparamref name="T"/> is a class without a parameterless constructor.</exception>
        public IEnumerable<T> ElementsAt<T>(int index)
        {
            return ElementsAt<T>(index, false, null);
        }

        /// <summary>
        /// Returns the data as a <see cref="IEnumerable{T}"/> collection.
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="index">The index to use.</param>
        /// <param name="settings">The <see cref="IColumnMapSetting"/> to use.</param>
        /// <returns>The <see cref="IEnumerable{T}"/> result.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when <typeparamref name="T"/> does not match the value type or <typeparamref name="T"/> is a class without a parameterless constructor.</exception>
        public IEnumerable<T> ElementsAt<T>(int index, IColumnMapSetting settings)
        {
            return ElementsAt<T>(index, false, settings);
        }

        internal IEnumerable<T> ElementsAt<T>(int index, bool isDequeueEnabled, IColumnMapSetting settings)
        {
            var data = Items.ElementAtOrDefault(index);

            if (data?.Count > 0)
            {
                Type tType = typeof(T);

                if (!(DbConnectorUtilities._directTypeMap.Contains(tType) || (tType.IsValueType && (tType.IsEnum || (Nullable.GetUnderlyingType(tType)?.IsEnum ?? false))) || tType.IsArray))
                {
                    var map = GetColumnMaps(tType, index, settings);

                    var itemIndex = 0;
                    if (tType.IsClass)
                    {
                        while (itemIndex < data.Count)
                        {
                            var item = data[itemIndex];

                            if (isDequeueEnabled)
                                data.RemoveAt(itemIndex);
                            else
                                itemIndex++;

                            yield return DbConnectorUtilities.GetMappedObject<T>(map, m => { return item[m.Column.Name]; });
                        }
                    }
                    else
                    {
                        while (itemIndex < data.Count)
                        {
                            var item = data[itemIndex];

                            if (isDequeueEnabled)
                                data.RemoveAt(itemIndex);
                            else
                                itemIndex++;

                            yield return (T)DbConnectorUtilities.GetMappedObject(tType, map, m => { return item[m.Column.Name]; });
                        }
                    }
                }
                else if (tType == typeof(DataTable) || tType == typeof(DataRow) || tType == typeof(DataSet))
                {
                    DataTable dt = new DataTable();

                    var colNames = data.First().Keys.ToArray();

                    if (colNames.Length > 0)
                    {
                        foreach (var c in colNames)
                        {
                            dt.Columns.Add(c);
                        }

                        var itemIndex = 0;
                        while (itemIndex < data.Count)
                        {
                            var item = data[itemIndex];

                            if (isDequeueEnabled)
                                data.RemoveAt(itemIndex);
                            else
                                itemIndex++;

                            DataRow row = dt.NewRow();

                            for (int i = 0; i < item.Count; i++)
                            {
                                row[i] = item.ElementAt(i).Value;
                            }

                            if (tType == typeof(DataRow))
                            {
                                yield return (T)Convert.ChangeType(row, tType);
                            }
                            else
                            {
                                dt.Rows.Add(row);
                            }
                        }

                        if (tType == typeof(DataTable))
                        {
                            yield return (T)Convert.ChangeType(dt, tType);
                        }
                        else if (tType == typeof(DataSet))
                        {
                            var projectedData = new DataSet();

                            projectedData.Tables.Add(dt);

                            yield return (T)Convert.ChangeType(projectedData, tType);
                        }
                    }
                }
                else if (tType == typeof(Dictionary<string, object>))
                {
                    var itemIndex = 0;
                    while (itemIndex < data.Count)
                    {
                        var items = data[itemIndex];

                        if (isDequeueEnabled)
                            data.RemoveAt(itemIndex);
                        else
                            itemIndex++;

                        yield return (T)Convert.ChangeType(items, tType);
                    }
                }
                else
                {
                    var itemIndex = 0;
                    Type nonNullableObjType = tType.IsValueType ? (Nullable.GetUnderlyingType(tType) ?? tType) : tType;

                    while (itemIndex < data.Count)
                    {
                        var item = data[itemIndex];

                        if (isDequeueEnabled)
                            data.RemoveAt(itemIndex);
                        else
                            itemIndex++;

                        T obj = default;

                        object value = item.FirstOrDefault().Value;

                        if (value != DBNull.Value)
                        {
                            obj = (T)(DbConnectorUtilities.ThrowIfFailedToMatchColumnType(tType, nonNullableObjType, value, () => item.FirstOrDefault().Key ?? ""));
                        }

                        yield return obj;
                    }
                }


                if (isDequeueEnabled && data.Count == 0)
                {
                    Items.RemoveAt(index);
                }
            }
            else
            {
                yield return default;
            }
        }

        public DbCollectionSet()
        {
            Items = new List<List<Dictionary<string, object>>>();
        }
    }



    #region State Models
    internal abstract class DbJobState : IDbJobState
    {
        public virtual IDbJobState Clone()
        {
            return this;
        }

        public virtual IDbJobState<TStateParamValue> Clone<TStateParamValue>(TStateParamValue value)
        {
            return new DbJobState<TStateParamValue> { StateParam = value };
        }
    }

    internal class DbJobState<TStateParam> : DbJobState, IDbJobState<TStateParam>
    {
        public TStateParam StateParam { get; set; }
    }

    internal abstract class DbConnectorStateBase : DbJobState, IDbConnectorState
    {
        public CalculatedDbConnectorFlags Flags { get; set; }

        public virtual IDbJobCommand CreateDbJobCommand(DbCommand cmd)
        {
            return new DbJobCommand(cmd);
        }

        public virtual IDbJobCommand CreateDbJobCommand(DbCommand cmd, IColumnMapSetting mapSettings)
        {
            return new DbJobCommand(cmd, mapSettings);
        }
    }


    internal class DbConnectorSimpleState : DbConnectorStateBase
    {
        public override IDbJobState<TStateParamValue> Clone<TStateParamValue>(TStateParamValue value)
        {
            return new DbConnectorSimpleState<TStateParamValue> { Flags = Flags, StateParam = value };
        }
    }

    internal class DbConnectorSimpleState<TStateParam> : DbConnectorSimpleState, IDbJobState<TStateParam>
    {
        public TStateParam StateParam { get; set; }

        public override IDbJobCommand CreateDbJobCommand(DbCommand cmd)
        {
            return new DbJobCommand<TStateParam>(cmd, StateParam);
        }

        public override IDbJobCommand CreateDbJobCommand(DbCommand cmd, IColumnMapSetting mapSettings)
        {
            return new DbJobCommand<TStateParam>(cmd, StateParam, mapSettings);
        }
    }


    internal class DbConnectorState : DbConnectorStateBase, IDbConnectorState<Action<IDbJobCommand>>
    {
        public Action<IDbJobCommand> OnInit { get; set; }

        public override IDbJobState<TStateParamValue> Clone<TStateParamValue>(TStateParamValue value)
        {
            return new DbConnectorState<TStateParamValue> { Flags = Flags, OnInit = OnInit, StateParam = value };
        }
    }

    internal class DbConnectorState<TStateParam> : DbConnectorState, IDbJobState<TStateParam>
    {
        public TStateParam StateParam { get; set; }

        public override IDbJobCommand CreateDbJobCommand(DbCommand cmd)
        {
            return new DbJobCommand<TStateParam>(cmd, StateParam);
        }
    }


    internal class DbConnectorQueuedState : DbConnectorStateBase, IDbConnectorState<Queue<Action<IDbJobCommand>>>
    {
        public Queue<Action<IDbJobCommand>> OnInit { get; set; }

        public override IDbJobState<TStateParamValue> Clone<TStateParamValue>(TStateParamValue value)
        {
            return new DbConnectorQueuedState<TStateParamValue> { Flags = Flags, OnInit = OnInit, StateParam = value };
        }
    }

    internal class DbConnectorQueuedState<TStateParam> : DbConnectorQueuedState, IDbJobState<TStateParam>
    {
        public TStateParam StateParam { get; set; }

        public override IDbJobCommand CreateDbJobCommand(DbCommand cmd)
        {
            return new DbJobCommand<TStateParam>(cmd, StateParam);
        }
    }


    internal class DbConnectorActionQueuedState : DbConnectorStateBase, IDbConnectorState<Action<Queue<Action<IDbJobCommand>>>>
    {
        public Action<Queue<Action<IDbJobCommand>>> OnInit { get; set; }

        public override IDbJobState<TStateParamValue> Clone<TStateParamValue>(TStateParamValue value)
        {
            return new DbConnectorActionQueuedState<TStateParamValue> { Flags = Flags, OnInit = OnInit, StateParam = value };
        }
    }

    internal class DbConnectorActionQueuedState<TStateParam> : DbConnectorActionQueuedState, IDbJobState<TStateParam>
    {
        public TStateParam StateParam { get; set; }

        public override IDbJobCommand CreateDbJobCommand(DbCommand cmd)
        {
            return new DbJobCommand<TStateParam>(cmd, StateParam);
        }
    }


    internal class DbConnectorDynamicState : DbConnectorStateBase, IDbConnectorDynamicState
    {
        public int Count { get; set; }

        public dynamic OnInit { get; set; }

        public override IDbJobState<TStateParamValue> Clone<TStateParamValue>(TStateParamValue value)
        {
            return new DbConnectorDynamicState<TStateParamValue> { Flags = Flags, Count = Count, OnInit = OnInit, StateParam = value };
        }
    }

    internal class DbConnectorDynamicState<TStateParam> : DbConnectorDynamicState, IDbJobState<TStateParam>
    {
        public TStateParam StateParam { get; set; }

        public override IDbJobCommand CreateDbJobCommand(DbCommand cmd)
        {
            return new DbJobCommand<TStateParam>(cmd, StateParam);
        }
    }
    #endregion



    #region Execution Models
    public class DbExecutionModel : DbExecutedModel, IDbExecutionModel
    {
        /// <summary>
        /// The DbConnection which will be disposed internally within a "finally" statement.
        /// </summary>
        public DbConnection Connection { get; internal set; }

        /// <summary>
        /// The DbTransaction which will be "committed" (or "rolled back" on exceptions) automatically and disposed internally within a "finally" statement.
        /// </summary>
        public DbTransaction Transaction { get; internal set; }

        /// <summary>
        /// <para>
        /// The <see cref="DbCommand"/> which will be disposed internally within a "finally" statement.        
        /// </para>
        ///  See also:
        ///  <seealso cref="DbConnection.CreateCommand"/>
        /// </summary>
        public DbCommand Command { get; internal set; }

        /// <summary>
        /// The <see cref="IDbJobCommand"/> being executed.
        /// </summary>
        public IDbJobCommand JobCommand { get; internal set; }

        protected internal IDisposable DeferrableDisposable { get; set; }

        /// <summary>
        /// Externally created <see cref="IDisposable"/> objects (e.g. <see cref="DbDataReader"/>) which have to be disposed internally within a "finally" statement.
        /// <para>This Queue has to be used in order to support the non-buffered or disposable execution of an <see cref="IDbJob{T}"/>.</para>
        /// <para>Note: This will be <see cref="null"/> for buffered/non-disposable <see cref="IDbJob{T}"/> executions. In this case, use either the <see cref="IDbExecutionModel.DeferDisposable(IDisposable)"/> function, a "try/finally" block, or an "using" statement instead.</para>
        /// </summary>
        public Queue<IDisposable> DeferrableDisposables { get; internal set; }

        /// <summary>
        /// Used to defer the disposal of an <see cref="IDisposable"/> object to the current <see cref="IDbJob{T}"/>'s "finally" statement.
        /// <para>This can only be used for a single disposable, instead of the <see cref="IDbExecutionModel.DeferrableDisposables"/> property, in order to reduce memory usage for buffered/non-disposable <see cref="IDbJob{T}"/> execution.</para>
        /// <para>Note: Use either a "try/finally" block or an "using" statement instead.</para>
        /// </summary>
        /// <param name="disposable">The <see cref="IDisposable"/> object to defer.</param>
        public void DeferDisposable(IDisposable disposable)
        {
            if (DeferrableDisposables == null)
            {
                DeferrableDisposable = disposable;
            }
            else
            {
                DeferrableDisposables.Enqueue(disposable);
            }
        }

        /// <summary>
        /// The current <see cref="IDbJob{T}"/> state.
        /// </summary>
        public virtual IDbJobState JobState { get; protected internal set; }

        public virtual DbExecutedModel CreateExecutedModel()
        {
            return new DbExecutedModel(IsBuffered, IsDisposable, Token)
            {
                Index = this.Index,
                NumberOfRowsAffected = this.NumberOfRowsAffected,
                Parameters = this.Parameters
            };
        }

        public DbExecutionModel(bool isBuffered, bool isDisposable, DbConnection connection, DbTransaction transaction, IDbJobState jobState, CancellationToken token)
            : base(isBuffered, isDisposable, token)
        {
            Connection = connection;
            Transaction = transaction;
            JobState = jobState;
        }
    }

    public class DbExecutionModel<TStateParam> : DbExecutionModel, IDbExecutionModel<TStateParam>
    {
        public TStateParam StateParam { get { return JobState == null ? default : (JobState as IDbJobState<TStateParam>).StateParam; } }

        public override DbExecutedModel CreateExecutedModel()
        {
            return new DbExecutedModel<TStateParam>(IsBuffered, IsDisposable, StateParam, Token)
            {
                Index = this.Index,
                NumberOfRowsAffected = this.NumberOfRowsAffected,
                Parameters = this.Parameters
            };
        }

        public DbExecutionModel(bool isBuffered, bool isDisposable, DbConnection connection, DbTransaction transaction, IDbJobState<TStateParam> jobState, CancellationToken token)
            : base(isBuffered, isDisposable, connection, transaction, jobState, token)
        {
        }
    }

    public class DbExecutedModel : IDbExecutedModel
    {
        /// <summary>
        /// Return true if <see cref="IDbJob{T}"/> was executed as "disposable".
        /// </summary>
        public bool IsDisposable { get; internal set; }

        public bool IsBuffered { get; internal set; }

        public CancellationToken Token { get; internal set; }

        public int? NumberOfRowsAffected { get; set; }

        public DbParameterCollection Parameters { get; set; }

        /// <summary>
        /// The current executed <see cref="IDbJobCommand"/> index. This is internally assigned and can be used for tracking purposes.
        /// </summary>
        public int Index { get; internal set; }

        internal DbExecutedModel(bool isBuffered, bool isDisposable, CancellationToken token)
        {
            IsBuffered = isBuffered;
            IsDisposable = isDisposable;
            Token = token;
        }
    }

    public class DbExecutedModel<TStateParam> : DbExecutedModel, IDbExecutedModel<TStateParam>
    {
        public TStateParam StateParam { get; }

        public DbExecutedModel(bool isBuffered, bool isDisposable, TStateParam stateParam, CancellationToken token)
            : base(isBuffered, isDisposable, token)
        {
            StateParam = stateParam;
        }
    }
    #endregion



    public class DbResult<T> : IDbResult<T>
    {
        public bool HasError
        {
            get
            {
                return this.Error != null;
            }
        }

        public Exception Error { get; set; }

        public T Data { get; set; }
    }


    /// <summary>
    /// An instance of this class needs to be disposed. Otherwise, the <see cref="DbConnection"/> will not be closed and other disposable objects will remain in memory.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    public class DbDisposable<T> : IDbDisposable<T>
    {
        public T Source { get; set; }

        /// <summary>
        /// The DbConnection of the DbJob which will be disposed internally when calling the <see cref="IDisposable.Dispose"/> function.
        /// </summary>
        internal DbConnection Connection { get; set; }

        /// <summary>
        /// The DbTransaction of the DbJob which will be disposed internally when calling the <see cref="IDisposable.Dispose"/> function.
        /// </summary>
        internal DbTransaction Transaction { get; set; }


        private readonly object _disposingLock = new object();
        internal Queue<IDbDisposable<T>> Childs;
        internal Queue<DbCommand> Commands;
        internal Queue<IDisposable> DisposableObjects;
        internal bool IsAutoConnection = true;
        internal bool WasConnectionClosed;
        private readonly bool _isLoggingEnabled;
        private readonly IDbConnectorLogger _logger;
        private bool _isDisposed;


        internal DbDisposable(bool isLoggingEnabled, IDbConnectorLogger logger)
        {
            _isLoggingEnabled = isLoggingEnabled;
            _logger = logger;

            DisposableObjects = new Queue<IDisposable>();
            Commands = new Queue<DbCommand>();
            Childs = new Queue<IDbDisposable<T>>();
        }


        protected virtual void Dispose(bool disposing, bool isCommitTransaction)
        {
            //Lock here to prevent lunacy
            lock (_disposingLock)
            {
                if (_isDisposed)
                    return;

                //if (disposing)
                //{
                //Console.WriteLine("Not in destructor, OK to reference other objects");
                //}            

                try
                {
                    if (Childs != null)
                    {
                        while (Childs.Count > 0)
                        {
                            Childs.Dequeue()?.Dispose(isCommitTransaction);
                        }

                        Childs = null;
                    }

                    if (DisposableObjects != null)
                    {
                        while (DisposableObjects.Count > 0)
                        {
                            DisposableObjects.Dequeue()?.Dispose();
                        }

                        DisposableObjects = null;
                    }

                    if (Commands != null)
                    {
                        while (Commands.Count > 0)
                        {
                            Commands.Dequeue()?.Dispose();
                        }

                        Commands = null;
                    }

                    if (Transaction != null)
                    {
                        if (isCommitTransaction)
                        {
                            Transaction.Commit();
                        }
                        else
                        {
                            Transaction.Rollback();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Log(_logger, _isLoggingEnabled);

                    throw;
                }
                finally
                {
                    if (Transaction != null)
                    {
                        Transaction.Dispose();
                        Transaction = null;
                    }

                    if (IsAutoConnection && Connection != null)
                    {
                        Connection.Dispose();
                        Connection = null;
                    }
                    else if (!IsAutoConnection && WasConnectionClosed && Connection != null)
                    {
                        Connection.Close();
                    }

                    this._isDisposed = true;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true, true);
            // tell the GC not to finalize
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool isCommitTransaction)
        {
            Dispose(true, isCommitTransaction);
            // tell the GC not to finalize
            GC.SuppressFinalize(this);
        }

        ~DbDisposable()
        {
            Dispose(false, true);
        }
    }


    public class DbBranchResult<T> : IDisposable
    {
        public IDbDisposable<T> Data { get; set; }

        public List<IDbExecutedModel> ExecutedModels { get; set; }


        public DbBranchResult()
        {
            ExecutedModels = new List<IDbExecutedModel>();
        }


        private readonly object _disposingLock = new object();
        private bool _isDisposed;


        protected virtual void Dispose(bool disposing, bool isCommitTransaction)
        {
            //Lock here to prevent lunacy
            lock (_disposingLock)
            {
                if (_isDisposed)
                    return;

                //if (disposing)
                //{
                //Console.WriteLine("Not in destructor, OK to reference other objects");
                //}     

                if (Data != null)
                {
                    Data.Dispose(isCommitTransaction);
                }

                ExecutedModels = null;

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true, true);
            // tell the GC not to finalize
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool isCommitTransaction)
        {
            Dispose(true, isCommitTransaction);
            // tell the GC not to finalize
            GC.SuppressFinalize(this);
        }

        ~DbBranchResult()
        {
            Dispose(false, true);
        }
    }
}