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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DbConnector.Core
{
    /// <summary>
    /// Represents a configurable and executable database job.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DbJobBase<T> : IDbJob<T>
    {
        #region Properties

        protected internal readonly object _executionLock = new object();
        protected internal readonly object _cloneLock = new object();
        protected internal readonly Func<T> _onInit;
        protected internal readonly Func<DbConnection, IDbJobState, IDbJobCommand[]> _onCommands;
        protected internal readonly Func<T, IDbExecutionModel, T> _onExecute;
        protected internal Func<T, Exception, T> _onError;
        protected internal Func<T, IDbExecutedModel, T> _onExecuted;
        protected internal Func<T, T> _onCompleted;
        protected internal Func<DbBranchResult<T>, IDbExecutionModel, IDbJob<T>, DbBranchResult<T>> _onBranch;
        protected internal readonly IDbJobSetting _settings;
        protected internal IDbJobState _state;
        protected internal IsolationLevel? _isolationLevel;
        protected internal bool _isCacheEnabled = true;
        protected internal bool _isLoggingEnabled = true;
        protected internal bool _isCreateDbCommand = true;
        protected internal bool _isDeferredExecution;
        protected internal bool _isIsolatedConnections;



        internal T DefaultValueOfT
        {
            get { return default; }
        }

        /// <summary>
        /// Gets the string used to open the connection.
        /// </summary>
        public string ConnectionString
        {
            get { return _settings.ConnectionString; }
        }

        /// <summary>
        /// Gets the type of <see cref="DbConnection"/> being used.
        /// </summary>
        public Type ConnectionType
        {
            get { return _settings.DbConnectionType; }
        }

        #endregion


        #region Constructors

        /// <summary>
        /// Copy Constructor
        /// <para>Note: This will use a clone lock in order to prevent race conditions.</para>
        /// </summary>
        /// <param name="jobToCopy"></param>
        public DbJobBase(DbJobBase<T> jobToCopy)
        {
            lock (jobToCopy._cloneLock)
            {
                _settings = jobToCopy._settings.Clone();
                _state = jobToCopy._state?.Clone();
                _isolationLevel = jobToCopy._isolationLevel;

                //booleans
                _isCacheEnabled = jobToCopy._isCacheEnabled;
                _isCreateDbCommand = jobToCopy._isCreateDbCommand;
                _isLoggingEnabled = jobToCopy._isLoggingEnabled;
                _isDeferredExecution = jobToCopy._isDeferredExecution;
                _isIsolatedConnections = jobToCopy._isIsolatedConnections;

                //Events
                _onInit = jobToCopy._onInit;
                _onCommands = jobToCopy._onCommands;
                _onExecute = jobToCopy._onExecute;
                _onExecuted = jobToCopy._onExecuted;
                _onCompleted = jobToCopy._onCompleted;
                _onError = jobToCopy._onError;
                _onBranch = jobToCopy._onBranch;
            }
        }

        public DbJobBase(
            IDbJobSetting setting,
            IDbJobState state,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute)
        {
            _settings = setting;
            _isCacheEnabled = setting.IsCacheEnabled;
            _state = state;
            _onCommands = onCommands ?? throw new NullReferenceException("The onCommands delegate cannot be null!");
            _onExecute = onExecute ?? throw new NullReferenceException("The onExecute delegate cannot be null!");
        }

        public DbJobBase(
            IDbJobSetting setting,
            IDbJobState state,
            Func<T> onInit,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute)
            : this(setting, state, onCommands, onExecute)
        {
            _onInit = onInit;
        }

        public DbJobBase(
            IDbJobSetting setting,
            IDbJobState state,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute,
            bool isCreateDbCommand)
            : this(setting, state, onCommands, onExecute)
        {
            _isCreateDbCommand = isCreateDbCommand;
        }

        public DbJobBase(
            IDbJobSetting setting,
            IDbJobState state,
            Func<T> onInit,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute,
            bool isCreateDbCommand)
            : this(setting, state, onCommands, onExecute)
        {
            _onInit = onInit;
            _isCreateDbCommand = isCreateDbCommand;
        }

        #endregion


        #region Implementation

        protected abstract DbJobBase<T> Clone();

        protected internal virtual DbConnection CreateConnectionInstance()
        {
            if (!DbConnectorCache.DbConnectionBuilderCache.TryGetValue(_settings.DbConnectionType, out Func<DbConnection> onCreateInstance))
            {
                return Activator.CreateInstance(_settings.DbConnectionType) as DbConnection;
            }
            else
            {
                return onCreateInstance();
            }
        }

        protected internal virtual DbExecutionModel CreateDbExecutionModel(bool isBuffered, bool isDisposable, DbConnection connection, DbTransaction transaction, IDbJobState jobState, CancellationToken token)
        {
            return new DbExecutionModel(isBuffered, isDisposable, connection, transaction, jobState, token);
        }

        protected internal virtual IDbJobCommand CreateDbJobCommand(DbCommand cmd, IDbJobState jobState)
        {
            return new DbJobCommand(cmd);
        }

        protected internal virtual bool IsWithStateParam()
        {
            return false;
        }



        protected virtual void ExecuteImplementation(ref T result, DbConnection connection, DbTransaction externalTransaction, CancellationToken cancellationToken, IDbJobState state)
        {
            if (_isDeferredExecution && DbConnectorUtilities.IsEnumerableOrAsyncEnumerable(typeof(T), out bool isAsyncEnumerable))
            {
                Type dbJobType = GetType();

                DbJobCacheModel cacheModel = new DbJobCacheModel(dbJobType, (isAsyncEnumerable ? nameof(this.ExecuteDeferredAsync) : nameof(this.ExecuteDeferred)));

                if (!DbConnectorCache.DbJobCache.TryGetValue(cacheModel, out Func<IDbJob, DbConnection, DbTransaction, CancellationToken, IDbJobState, dynamic> onExecuteDeferred))
                {
                    MethodInfo mi = dbJobType.GetMethod((isAsyncEnumerable ? nameof(this.ExecuteDeferredAsync) : nameof(this.ExecuteDeferred)), BindingFlags.NonPublic | BindingFlags.Instance);
                    MethodInfo genericExecuteDeferred = mi.MakeGenericMethod(typeof(T).GetGenericArguments().Single());

                    onExecuteDeferred = DynamicDbJobMethodBuilder.CreateBuilderFunction(dbJobType, genericExecuteDeferred);

                    DbConnectorCache.DbJobCache.TryAdd(cacheModel, onExecuteDeferred);
                }

                //Invoke ExecuteDeferred
                result = onExecuteDeferred(this, connection, externalTransaction, cancellationToken, state);
            }
            else if (_isIsolatedConnections && _onBranch != null)
            {
                if (_onInit != null)
                {
                    result = _onInit();
                }

                using (DbBranchResult<T> branchResult =
                    _onBranch(
                        new DbBranchResult<T> { Data = new DbDisposable<T>(_isLoggingEnabled, _settings.Logger) { Source = result } },
                        CreateDbExecutionModel(!_isDeferredExecution, false, null, null, state, cancellationToken),
                        this
                    )
                )
                {
                    result = branchResult.Data.Source;
                }
            }
            else
            {
                if (_onInit != null)
                {
                    result = _onInit();
                }

                //Create and open the connection.
                DbConnection conn = connection;
                bool wasConnectionClosed = false;

                IDbJobCommand[] dbJobCommands = null;
                DbTransaction transaction = externalTransaction;
                try
                {
                    if (connection == null)
                    {
                        conn = CreateConnectionInstance();
                        conn.ConnectionString = _settings.ConnectionString;
                    }

                    //Set settings and commands.
                    dbJobCommands = _isCreateDbCommand ? _onCommands(conn, state) : new IDbJobCommand[] { null };

                    if (dbJobCommands == null)
                    {
                        //Safety blanket!
                        dbJobCommands = new IDbJobCommand[] { _isCreateDbCommand ? CreateDbJobCommand(conn.CreateCommand(), state) : null };
                    }


                    if (connection == null || conn.State == ConnectionState.Closed)
                    {
                        wasConnectionClosed = true;
                        conn.Open();
                    }


                    //Begin transaction.
                    if (_isolationLevel.HasValue && externalTransaction == null)
                    {
                        transaction = conn.BeginTransaction(_isolationLevel.Value);
                    }

                    DbExecutionModel eParam = CreateDbExecutionModel(true, false, conn, transaction, state, cancellationToken);

                    for (int i = 0; i < dbJobCommands.Length; i++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            if (_isCreateDbCommand)
                            {
                                for (int x = i; x < dbJobCommands.Length; x++)
                                {
                                    dbJobCommands[x]?.GetDbCommand()?.Dispose();
                                }
                            }

                            return;
                        }

                        DbCommand cmd = null;
                        var jCommand = dbJobCommands[i];

                        eParam.Index = i;
                        eParam.Command = null;
                        eParam.JobCommand = jCommand;

                        try
                        {
                            if (_isCreateDbCommand)
                            {
                                eParam.Command = cmd = jCommand.GetDbCommand();
                                cmd.Connection = conn;
                                cmd.Transaction = transaction;

                                if (!_isCacheEnabled)
                                {
                                    jCommand.Flags |= DbJobCommandFlags.NoCache;
                                }
                            }

                            result = _onExecute(result, eParam);

                            if (_onExecuted != null)
                            {
                                eParam.Parameters = cmd?.Parameters;

                                result = _onExecuted(result, eParam.CreateExecutedModel());
                            }
                        }
                        finally
                        {
                            eParam.DeferrableDisposable?.Dispose();
                            cmd?.Dispose();
                        }
                    }

                    if (transaction != null && externalTransaction == null)
                    {
                        transaction.Commit();
                    }
                }
                catch (Exception)
                {
                    if (_isCreateDbCommand && dbJobCommands != null)
                    {
                        for (int x = 0; x < dbJobCommands.Length; x++)
                        {
                            dbJobCommands[x]?.GetDbCommand()?.Dispose();
                        }
                    }

                    if (transaction != null && externalTransaction == null)
                    {
                        transaction.Rollback();
                    }

                    throw;
                }
                finally
                {
                    if (transaction != null && externalTransaction == null)
                    {
                        transaction.Dispose();
                    }

                    if (connection == null)
                    {
                        conn.Dispose();
                    }
                    else if (wasConnectionClosed && connection != null && conn != null)
                    {
                        conn.Close();
                    }
                }
            }


            if (_onCompleted != null)
            {
                result = _onCompleted(result);
            }
        }

        internal IEnumerable<TChild> ExecuteDeferred<TChild>(DbConnection connection, DbTransaction externalTransaction, CancellationToken cancellationToken, IDbJobState state)
        {
            T result = _onInit != null ? _onInit() : default;
            DbConnection conn = null;
            DbTransaction transaction = externalTransaction;
            IDbJobCommand[] dbJobCommands = null;
            bool wasConnectionClosed = false;

            try
            {
                //Set connection
                conn = connection ?? CreateConnectionInstance();
                if (connection == null)
                {
                    conn.ConnectionString = _settings.ConnectionString;
                }


                //Set settings and commands.
                dbJobCommands = _isCreateDbCommand ? _onCommands(conn, state) : new IDbJobCommand[] { null };

                if (dbJobCommands == null)
                {
                    dbJobCommands = new IDbJobCommand[] { _isCreateDbCommand ? CreateDbJobCommand(conn.CreateCommand(), state) : null };
                }


                if (connection == null || conn.State == ConnectionState.Closed)
                {
                    wasConnectionClosed = true;
                    conn.Open();
                }


                //Set transaction
                if (_isolationLevel.HasValue && externalTransaction == null)
                {
                    transaction = conn.BeginTransaction(_isolationLevel.Value);
                }
                Queue<IDisposable> commands = (_isCreateDbCommand) ? new Queue<IDisposable>(dbJobCommands.Length) : null;
                DbExecutionModel eParam = CreateDbExecutionModel(false, false, conn, transaction, state, cancellationToken);

                eParam.DeferrableDisposables = new Queue<IDisposable>(dbJobCommands.Length);

                var deferrableDisposablesQueue = eParam.DeferrableDisposables;

                try
                {
                    for (int i = 0; i < dbJobCommands.Length; i++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            yield break;

                        DbCommand cmd = null;
                        var jCommand = dbJobCommands[i];

                        eParam.Index = i;
                        eParam.Command = null;
                        eParam.JobCommand = jCommand;

                        if (_isCreateDbCommand)
                        {
                            eParam.Command = cmd = jCommand.GetDbCommand();
                            cmd.Connection = conn;
                            cmd.Transaction = transaction;
                            commands.Enqueue(cmd);

                            if (!_isCacheEnabled)
                            {
                                jCommand.Flags |= DbJobCommandFlags.NoCache;
                            }
                        }

                        result = _onExecute(result, eParam);

                        if (_onExecuted != null)
                        {
                            eParam.Parameters = cmd?.Parameters;

                            result = _onExecuted(result, eParam.CreateExecutedModel());
                        }
                    }


                    if (result != null)
                    {
                        var resultAsEnumerable = result as IEnumerable<TChild>;

                        foreach (var item in resultAsEnumerable)
                        {
                            yield return item;
                        }
                    }
                }
                finally
                {
                    while (deferrableDisposablesQueue.Count != 0)
                    {
                        deferrableDisposablesQueue.Dequeue()?.Dispose();
                    }

                    if (commands != null)
                    {
                        while (commands.Count != 0)
                        {
                            commands.Dequeue()?.Dispose();
                        }
                    }

                    try
                    {
                        if (transaction != null && externalTransaction == null)
                        {
                            transaction.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_isCreateDbCommand && dbJobCommands != null)
                        {
                            for (int x = 0; x < dbJobCommands.Length; x++)
                            {
                                dbJobCommands[x]?.GetDbCommand()?.Dispose();
                            }
                        }

                        ex.Log(_settings.Logger, false);

                        if (transaction != null && externalTransaction == null)
                        {
                            transaction.Rollback();
                        }

                        throw;
                    }
                }
            }
            finally
            {
                if (transaction != null && externalTransaction == null)
                {
                    transaction.Dispose();
                }

                if (connection == null && conn != null)
                {
                    conn.Dispose();
                }
                else if (wasConnectionClosed && connection != null && conn != null)
                {
                    conn.Close();
                }
            }
        }

        internal async IAsyncEnumerable<TChild> ExecuteDeferredAsync<TChild>(DbConnection connection, DbTransaction externalTransaction, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken, IDbJobState state)
        {
            T result = _onInit != null ? _onInit() : default;
            DbConnection conn = null;
            DbTransaction transaction = externalTransaction;
            IDbJobCommand[] dbJobCommands = null;
            bool wasConnectionClosed = false;

            try
            {
                //Set connection
                conn = connection ?? CreateConnectionInstance();
                if (connection == null)
                {
                    conn.ConnectionString = _settings.ConnectionString;
                }


                //Set settings and commands.
                dbJobCommands = _isCreateDbCommand ? _onCommands(conn, state) : new IDbJobCommand[] { null };

                if (dbJobCommands == null)
                {
                    dbJobCommands = new IDbJobCommand[] { _isCreateDbCommand ? CreateDbJobCommand(conn.CreateCommand(), state) : null };
                }


                if (connection == null || conn.State == ConnectionState.Closed)
                {
                    wasConnectionClosed = true;
                    await conn.OpenAsync(cancellationToken);
                }


                //Set transaction
                if (_isolationLevel.HasValue && externalTransaction == null)
                {
                    transaction = conn.BeginTransaction(_isolationLevel.Value);
                }
                Queue<IDisposable> commands = (_isCreateDbCommand) ? new Queue<IDisposable>(dbJobCommands.Length) : null;
                DbExecutionModel eParam = CreateDbExecutionModel(false, false, conn, transaction, state, cancellationToken);

                eParam.DeferrableDisposables = new Queue<IDisposable>(dbJobCommands.Length);

                var deferrableDisposablesQueue = eParam.DeferrableDisposables;

                try
                {
                    for (int i = 0; i < dbJobCommands.Length; i++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            yield break;

                        DbCommand cmd = null;
                        var jCommand = dbJobCommands[i];

                        eParam.Index = i;
                        eParam.Command = null;
                        eParam.JobCommand = jCommand;

                        if (_isCreateDbCommand)
                        {
                            eParam.Command = cmd = jCommand.GetDbCommand();
                            cmd.Connection = conn;
                            cmd.Transaction = transaction;
                            commands.Enqueue(cmd);

                            if (!_isCacheEnabled)
                            {
                                jCommand.Flags |= DbJobCommandFlags.NoCache;
                            }
                        }

                        result = _onExecute(result, eParam);

                        if (_onExecuted != null)
                        {
                            eParam.Parameters = cmd?.Parameters;

                            result = _onExecuted(result, eParam.CreateExecutedModel());
                        }
                    }


                    if (result != null)
                    {
                        var resultAsAsyncEnumerable = result as IAsyncEnumerable<TChild>;

                        await foreach (var item in resultAsAsyncEnumerable)
                        {
                            yield return item;
                        }
                    }
                }
                finally
                {
                    while (deferrableDisposablesQueue.Count != 0)
                    {
                        deferrableDisposablesQueue.Dequeue()?.Dispose();
                    }

                    if (commands != null)
                    {
                        while (commands.Count != 0)
                        {
                            commands.Dequeue()?.Dispose();
                        }
                    }

                    try
                    {
                        if (transaction != null && externalTransaction == null)
                        {
                            transaction.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_isCreateDbCommand && dbJobCommands != null)
                        {
                            for (int x = 0; x < dbJobCommands.Length; x++)
                            {
                                dbJobCommands[x]?.GetDbCommand()?.Dispose();
                            }
                        }

                        ex.Log(_settings.Logger, false);

                        if (transaction != null && externalTransaction == null)
                        {
                            transaction.Rollback();
                        }

                        throw;
                    }
                }
            }
            finally
            {
                if (transaction != null && externalTransaction == null)
                {
                    transaction.Dispose();
                }

                if (connection == null && conn != null)
                {
                    conn.Dispose();
                }
                else if (wasConnectionClosed && connection != null && conn != null)
                {
                    conn.Close();
                }
            }
        }

        private T TryExecute(DbConnection connection, DbTransaction transaction, CancellationToken token, bool? isThrowExceptions)
        {
            object executionLock = _executionLock;
            bool isLockAcquired = false;
            T result = default;
            DbJobBase<T> job = null;
            try
            {
                Monitor.TryEnter(executionLock, ref isLockAcquired);

                job = isLockAcquired ? this : Clone();

                job.ExecuteImplementation(ref result, connection, transaction, token, job._state);
            }
            catch (Exception ex)
            {
                isThrowExceptions = isThrowExceptions ?? (job?._settings.IsThrowExceptions ?? true);

                ex.Log(job?._settings.Logger, isThrowExceptions.Value ? false : job?._isLoggingEnabled ?? false);

                if (job?._onError != null)
                {
                    try
                    {
                        result = job._onError(result, ex);
                    }
                    catch (Exception exInner)
                    {
                        exInner.Log(job._settings.Logger, isThrowExceptions.Value ? false : job._isLoggingEnabled);

                        if (isThrowExceptions.Value)
                        {
                            throw;
                        }
                    }
                }

                if (isThrowExceptions.Value)
                {
                    throw;
                }
            }
            finally
            {
                if (isLockAcquired)
                {
                    Monitor.Exit(executionLock);
                }
            }

            return result;
        }

        /// <summary>
        /// Execute the <see cref="IDbJob{T}"/>.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJob{T}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>       
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The T result.</returns>
        public virtual T Execute(DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            return TryExecute(connection, null, token, isThrowExceptions);
        }

        /// <summary>
        /// Execute the <see cref="IDbJob{T}"/>.
        /// </summary>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for this <see cref="IDbJob{T}"/> execution.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Note: This will override the use of <see cref="IDbJob{T}.WithIsolationLevel(IsolationLevel?)"/>.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The T result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="transaction"/> is null.</exception>
        public virtual T Execute(DbTransaction transaction, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            return TryExecute(transaction?.Connection, transaction, token, isThrowExceptions);
        }

        /// <summary>
        /// Execute the <see cref="IDbJob{T}"/> asynchronously.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJob{T}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The <see cref="Task{T}"/>.</returns>
        public virtual Task<T> ExecuteAsync(DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            return Task.Run(() => TryExecute(connection, null, token, isThrowExceptions), token);
        }

        /// <summary>
        /// Execute the <see cref="IDbJob{T}"/> asynchronously.
        /// </summary>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for this <see cref="IDbJob{T}"/> execution.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Note: This will override the use of <see cref="IDbJob{T}.WithIsolationLevel(IsolationLevel?)"/>.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The <see cref="Task{T}"/>.</returns>
        public virtual Task<T> ExecuteAsync(DbTransaction transaction, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            return Task.Run(() => TryExecute(transaction?.Connection, transaction, token, isThrowExceptions), token);
        }



        private IDbResult<T> TryExecuteHandled(DbConnection connection, DbTransaction transaction, CancellationToken token)
        {
            object executionLock = _executionLock;
            bool isLockAcquired = false;
            IDbResult<T> result = new DbResult<T>();
            T data = default;
            DbJobBase<T> job = null;
            try
            {
                Monitor.TryEnter(executionLock, ref isLockAcquired);

                job = isLockAcquired ? this : Clone();

                job.ExecuteImplementation(ref data, connection, transaction, token, job._state);

                result.Data = data;
            }
            catch (Exception ex)
            {
                ex.Log(job?._settings.Logger, job?._isLoggingEnabled ?? false);

                if (job?._onError != null)
                {
                    try
                    {
                        result.Data = job._onError(data, ex);
                    }
                    catch (Exception exInner)
                    {
                        exInner.Log(job._settings.Logger, job._isLoggingEnabled);

                        ex = exInner;
                    }
                }

                result.Error = ex;
            }
            finally
            {
                if (isLockAcquired)
                {
                    Monitor.Exit(executionLock);
                }
            }

            return result;
        }

        /// <summary>
        /// Execute the <see cref="IDbJob{T}"/> and handle any exceptions while opening the <see cref="DbConnection"/>.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJob{T}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbResult{T}"/>.</returns>
        public virtual IDbResult<T> ExecuteHandled(DbConnection connection = null, CancellationToken token = default)
        {
            return TryExecuteHandled(connection, null, token);
        }

        /// <summary>
        /// Execute the <see cref="IDbJob{T}"/> and handle any exceptions while opening the <see cref="DbConnection"/>.
        /// </summary>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for this <see cref="IDbJob{T}"/> execution.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Note: This will override the use of <see cref="IDbJob{T}.WithIsolationLevel(IsolationLevel?)"/>.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbResult{T}"/>.</returns>
        public virtual IDbResult<T> ExecuteHandled(DbTransaction transaction, CancellationToken token = default)
        {
            return TryExecuteHandled(transaction?.Connection, transaction, token);
        }

        /// <summary>
        /// Execute the <see cref="IDbJob{T}"/> and handle any exceptions while opening the <see cref="DbConnection"/> asynchronously.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJob{T}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="Task{IDbResult{T}}"/>.</returns>
        public virtual Task<IDbResult<T>> ExecuteHandledAsync(DbConnection connection = null, CancellationToken token = default)
        {
            return Task.Run(() => TryExecuteHandled(connection, null, token), token);
        }

        /// <summary>
        /// Execute the <see cref="IDbJob{T}"/> and handle any exceptions while opening the <see cref="DbConnection"/> asynchronously.
        /// </summary>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for this <see cref="IDbJob{T}"/> execution.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Note: This will override the use of <see cref="IDbJob{T}.WithIsolationLevel(IsolationLevel?)"/>.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="Task{IDbResult{T}}"/>.</returns>
        public virtual Task<IDbResult<T>> ExecuteHandledAsync(DbTransaction transaction, CancellationToken token = default)
        {
            return Task.Run(() => TryExecuteHandled(transaction?.Connection, transaction, token), token);
        }



        protected virtual void ExecuteDisposableImplementation(ref DbDisposable<T> result, DbConnection connection, DbTransaction externalTransaction, CancellationToken cancellationToken, IDbJobState state)
        {
            result = new DbDisposable<T>(_isLoggingEnabled, _settings.Logger);

            if (_onInit != null)
            {
                result.Source = _onInit();
            }


            if (_isIsolatedConnections && _onBranch != null)
            {
                DbBranchResult<T> branchResult = _onBranch(
                        new DbBranchResult<T> { Data = new DbDisposable<T>(_isLoggingEnabled, _settings.Logger) { Source = result.Source } },
                        CreateDbExecutionModel(!_isDeferredExecution, true, null, null, state, cancellationToken),
                        this
                );
                result.Source = branchResult.Data.Source;
                result.Childs.Enqueue(branchResult.Data);
            }
            else
            {
                //Create and open the connection.
                DbConnection conn;
                IDbJobCommand[] dbJobCommands = null;
                try
                {
                    //Create and open the connection.
                    result.Connection = conn = connection ?? CreateConnectionInstance();
                    if (connection == null)
                    {
                        conn.ConnectionString = _settings.ConnectionString;
                    }
                    else
                    {
                        result.IsAutoConnection = false;
                    }


                    //Set settings and commands.
                    dbJobCommands = _isCreateDbCommand ? _onCommands(conn, state) : new IDbJobCommand[] { null };

                    if (dbJobCommands == null)
                    {
                        dbJobCommands = new IDbJobCommand[] { _isCreateDbCommand ? CreateDbJobCommand(conn.CreateCommand(), state) : null };
                    }


                    if (connection == null || conn.State == ConnectionState.Closed)
                    {
                        result.WasConnectionClosed = connection != null;
                        conn.Open();
                    }


                    //Begin transaction.
                    DbTransaction transaction = result.Transaction = (_isolationLevel.HasValue && externalTransaction == null) ? conn.BeginTransaction(_isolationLevel.Value) : externalTransaction;

                    DbExecutionModel eParam = CreateDbExecutionModel(!_isDeferredExecution, true, conn, transaction, state, cancellationToken);

                    eParam.DeferrableDisposables = new Queue<IDisposable>(1);

                    var deferrableDisposablesQueue = eParam.DeferrableDisposables;

                    for (int i = 0; i < dbJobCommands.Length; i++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            if (_isCreateDbCommand)
                            {
                                for (int x = i; x < dbJobCommands.Length; x++)
                                {
                                    dbJobCommands[x]?.GetDbCommand()?.Dispose();
                                }
                            }

                            return;
                        }

                        DbCommand cmd = null;
                        var jCommand = dbJobCommands[i];

                        eParam.Index = i;
                        eParam.Command = null;
                        eParam.JobCommand = jCommand;

                        if (_isCreateDbCommand)
                        {
                            eParam.Command = cmd = jCommand.GetDbCommand();
                            cmd.Connection = conn;
                            cmd.Transaction = transaction;
                            result.Commands.Enqueue(cmd);

                            if (!_isCacheEnabled)
                            {
                                jCommand.Flags |= DbJobCommandFlags.NoCache;
                            }
                        }


                        try
                        {
                            result.Source = _onExecute(result.Source, eParam);
                        }
                        finally
                        {
                            while (deferrableDisposablesQueue.Count != 0)
                            {
                                result.DisposableObjects.Enqueue(deferrableDisposablesQueue.Dequeue());
                            }
                        }


                        if (_onExecuted != null)
                        {
                            eParam.Parameters = cmd?.Parameters;

                            result.Source = _onExecuted(result.Source, eParam.CreateExecutedModel());
                        }
                    }
                }
                catch (Exception)
                {
                    if (_isCreateDbCommand && dbJobCommands != null)
                    {
                        for (int x = 0; x < dbJobCommands.Length; x++)
                        {
                            dbJobCommands[x]?.GetDbCommand()?.Dispose();
                        }
                    }

                    throw;
                }
            }


            if (_onCompleted != null)
            {
                result.Source = _onCompleted(result.Source);
            }
        }

        private IDbDisposable<T> TryExecuteDisposable(DbConnection connection, DbTransaction transaction, CancellationToken token, bool? isThrowExceptions)
        {
            object executionLock = _executionLock;
            bool isLockAcquired = false;
            DbDisposable<T> result = default;
            DbJobBase<T> job = null;
            try
            {
                Monitor.TryEnter(executionLock, ref isLockAcquired);

                job = isLockAcquired ? this : Clone();

                job.ExecuteDisposableImplementation(ref result, connection, transaction, token, job._state);
            }
            catch (Exception ex)
            {
                result?.Dispose(false);

                isThrowExceptions = isThrowExceptions ?? (job?._settings.IsThrowExceptions ?? true);

                ex.Log(job?._settings.Logger, isThrowExceptions.Value ? false : job?._isLoggingEnabled ?? false);

                if (result != null && job?._onError != null)
                {
                    try
                    {
                        result.Source = job._onError(result.Source, ex);
                    }
                    catch (Exception exInner)
                    {
                        exInner.Log(job._settings.Logger, isThrowExceptions.Value ? false : job._isLoggingEnabled);

                        if (isThrowExceptions.Value)
                        {
                            throw;
                        }
                    }
                }

                if (isThrowExceptions.Value)
                {
                    throw;
                }
            }
            finally
            {
                if (isLockAcquired)
                {
                    Monitor.Exit(executionLock);
                }
            }

            return result;
        }

        /// <summary>
        /// Use this function when a serialized data extraction is required. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJob{T}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The <see cref="IDbDisposable{T}"/>.</returns>
        public virtual IDbDisposable<T> ExecuteDisposable(DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            return TryExecuteDisposable(connection, null, token, isThrowExceptions);
        }

        /// <summary>
        /// Use this function when a serialized data extraction is required. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for this <see cref="IDbJob{T}"/> execution.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Note: This will override the use of <see cref="IDbJob{T}.WithIsolationLevel(IsolationLevel?)"/>.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The <see cref="IDbDisposable{T}"/>.</returns>
        public virtual IDbDisposable<T> ExecuteDisposable(DbTransaction transaction, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            return TryExecuteDisposable(transaction?.Connection, transaction, token, isThrowExceptions);
        }

        /// <summary>
        /// Use this asynchronous function when a serialized data extraction is required. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJob{T}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The <see cref="Task{IDbDisposable{T}}"/>.</returns>
        public virtual Task<IDbDisposable<T>> ExecuteDisposableAsync(DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            return Task.Run(() => TryExecuteDisposable(connection, null, token, isThrowExceptions), token);
        }

        /// <summary>
        /// Use this asynchronous function when a serialized data extraction is required. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for this <see cref="IDbJob{T}"/> execution.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Note: This will override the use of <see cref="IDbJob{T}.WithIsolationLevel(IsolationLevel?)"/>.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The <see cref="Task{IDbDisposable{T}}"/>.</returns>
        public virtual Task<IDbDisposable<T>> ExecuteDisposableAsync(DbTransaction transaction, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            return Task.Run(() => TryExecuteDisposable(transaction?.Connection, transaction, token, isThrowExceptions), token);
        }



        private IDbResult<IDbDisposable<T>> TryExecuteDisposableHandled(DbConnection connection, DbTransaction transaction, CancellationToken token)
        {
            object executionLock = _executionLock;
            bool isLockAcquired = false;
            IDbResult<IDbDisposable<T>> result = new DbResult<IDbDisposable<T>>();
            DbDisposable<T> disposableResult = default;
            DbJobBase<T> job = null;
            try
            {
                Monitor.TryEnter(executionLock, ref isLockAcquired);

                job = isLockAcquired ? this : Clone();

                job.ExecuteDisposableImplementation(ref disposableResult, connection, transaction, token, job._state);

                result.Data = disposableResult;
            }
            catch (Exception ex)
            {
                disposableResult?.Dispose(false);

                ex.Log(job?._settings.Logger, job?._isLoggingEnabled ?? false);

                if (disposableResult != null && job?._onError != null)
                {
                    try
                    {
                        disposableResult.Source = job._onError(disposableResult.Source, ex);

                        result.Data = disposableResult;
                    }
                    catch (Exception exInner)
                    {
                        exInner.Log(job._settings.Logger, job._isLoggingEnabled);

                        ex = exInner;
                    }
                }

                result.Error = ex;
            }
            finally
            {
                if (isLockAcquired)
                {
                    Monitor.Exit(executionLock);
                }
            }

            return result;
        }

        /// <summary>
        /// Use this function when a serialized data extraction is required and to handle any exceptions while opening the <see cref="DbConnection"/>. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJob{T}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbResult{IDbDisposable{T}}"/>.</returns>
        public virtual IDbResult<IDbDisposable<T>> ExecuteDisposableHandled(DbConnection connection = null, CancellationToken token = default)
        {
            return TryExecuteDisposableHandled(connection, null, token);
        }

        /// <summary>
        /// Use this function when a serialized data extraction is required and to handle any exceptions while opening the <see cref="DbConnection"/>. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for this <see cref="IDbJob{T}"/> execution.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Note: This will override the use of <see cref="IDbJob{T}.WithIsolationLevel(IsolationLevel?)"/>.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbResult{IDbDisposable{T}}"/>.</returns>
        public virtual IDbResult<IDbDisposable<T>> ExecuteDisposableHandled(DbTransaction transaction, CancellationToken token = default)
        {
            return TryExecuteDisposableHandled(transaction?.Connection, transaction, token);
        }

        /// <summary>
        /// Use this asynchronous function when a serialized data extraction is required and to handle any exceptions while opening the <see cref="DbConnection"/>. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJob{T}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="Task{IDbResult{IDbDisposable{T}}}"/>.</returns>
        public virtual Task<IDbResult<IDbDisposable<T>>> ExecuteDisposableHandledAsync(DbConnection connection = null, CancellationToken token = default)
        {
            return Task.Run(() => TryExecuteDisposableHandled(connection, null, token), token);
        }

        /// <summary>
        /// Use this asynchronous function when a serialized data extraction is required and to handle any exceptions while opening the <see cref="DbConnection"/>. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for this <see cref="IDbJob{T}"/> execution.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Note: This will override the use of <see cref="IDbJob{T}.WithIsolationLevel(IsolationLevel?)"/>.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="Task{IDbResult{IDbDisposable{T}}}"/>.</returns>
        public virtual Task<IDbResult<IDbDisposable<T>>> ExecuteDisposableHandledAsync(DbTransaction transaction, CancellationToken token = default)
        {
            return Task.Run(() => TryExecuteDisposableHandled(transaction?.Connection, transaction, token), token);
        }



        /// <summary>
        /// Use this function to disable or enable error logging (this is enabled by default). Disabling this can be useful when logging errors in order to prevent an infinite loop.
        /// </summary>
        /// <returns><see cref="IDbJob{T}"/></returns>
        public virtual IDbJob<T> WithLogging(bool isEnabled)
        {
            lock (_executionLock)
            {
                lock (_cloneLock)
                {
                    _isLoggingEnabled = isEnabled;
                }
            }

            return this;
        }

        /// <summary>
        /// Use this function to set the <see cref="IsolationLevel"/> and enable the use of a <see cref="DbTransaction"/> for this <see cref="IDbJob{T}"/>.
        /// </summary>
        /// <returns><see cref="IDbJob{T}"/></returns>
        public virtual IDbJob<T> WithIsolationLevel(IsolationLevel? level)
        {
            lock (_executionLock)
            {
                lock (_cloneLock)
                {
                    _isolationLevel = level;
                }
            }

            return this;
        }

        /// <summary>
        /// Use this function to set the <see cref="IsolationLevel"/> for this <see cref="IDbJob{T}"/>.
        /// <para>Note: Should only be called when initiating this <see cref="IDbJob{T}"/>.</para>
        /// </summary>
        /// <returns><see cref="IDbJob{T}"/></returns>
        protected internal virtual DbJobBase<T> SetWithIsolationLevel(IsolationLevel? level)
        {
            _isolationLevel = level;
            return this;
        }

        /// <summary>
        /// <para>Use this function to disable or enable buffered (non-deferred/non-yielded) execution when reading data (this is enabled by default).</para>
        /// <para>Note: The use of deferred execution is only possible for <see cref="System.Collections.IEnumerable"/> types during individual transactions. Normal execution will be used when encountering non <see cref="System.Collections.IEnumerable"/> types or Batch-Reading implementations.</para>
        /// </summary>
        /// <remarks>
        /// <para>Warning: Deferred execution leverages "yield statement" logic and postpones the disposal of database connections and related resources. 
        /// Always perform an iteration of the returned <see cref="System.Collections.IEnumerable"/> by either implementing a "for-each" loop or a data projection (e.g. invoking the <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})"/> extension). You can also dispose the enumerator as an alternative.
        /// Not doing so will internally leave disposable resources opened (e.g. database connections) consequently creating memory leak scenarios.
        /// </para>
        /// <para>Warning: Exceptions may occur while looping deferred <see cref="System.Collections.IEnumerable"/> types because of the implicit database connection dependency.</para>
        /// </remarks>
        /// <returns><see cref="IDbJob{T}"/></returns>
        public virtual IDbJob<T> WithBuffering(bool isEnabled)
        {
            lock (_executionLock)
            {
                lock (_cloneLock)
                {
                    if (!isEnabled || DbConnectorUtilities.IsAsyncEnumerable(typeof(T)))
                    {
                        _isDeferredExecution = true;
                    }
                    else
                    {
                        _isDeferredExecution = false;
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// <para>Use this function to disable or enable the caching of query mappings and types for a all <see cref="DbJobCommand"/> owned by this <see cref="IDbJob{T}"/>.</para>
        /// </summary>
        /// <returns><see cref="IDbJob{T}"/></returns>
        public virtual IDbJob<T> WithCache(bool isEnabled)
        {
            lock (_executionLock)
            {
                if (_settings.IsCacheEnabled)
                {
                    lock (_cloneLock)
                    {
                        _isCacheEnabled = isEnabled;
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// Use this function to set branched properties for this <see cref="IDbJob{T}"/>.
        /// <para>Note: Should only be called when initiating this <see cref="IDbJob{T}"/>.</para>
        /// </summary>
        /// <returns><see cref="IDbJob{T}"/></returns>
        protected internal virtual IDbJob<T> SetBranchedProperties(
            bool isBufferingEnabled,
            bool isLoggingEnabled,
            bool isCacheEnabled,
            IsolationLevel? level)
        {
            _isolationLevel = level;
            _isDeferredExecution = (isBufferingEnabled == false);
            _isLoggingEnabled = isLoggingEnabled;

            if (_settings.IsCacheEnabled)
            {
                _isCacheEnabled = isCacheEnabled;
            }

            return this;
        }

        /// <summary>
        /// Use this function when wanting to use isolated connections (a.k.a branching) for this <see cref="IDbJob{T}"/>.
        /// </summary>
        /// <param name="isEnabled">Enable or disable the use of isolated connections.</param>
        /// <param name="isUseLock">True to use locking. (Optional)</param>
        /// <returns><see cref="IDbJob{T}"/></returns>
        protected internal virtual DbJobBase<T> WithIsolatedConnections(bool? isEnabled, bool isUseLock = false)
        {
            if (isUseLock)
            {
                lock (_executionLock)
                {
                    lock (_cloneLock)
                    {
                        if (isEnabled.HasValue)
                        {
                            _isIsolatedConnections = (isEnabled.Value);
                        }
                    }
                }
            }
            else
            {
                if (isEnabled.HasValue)
                {
                    _isIsolatedConnections = (isEnabled.Value);
                }
            }

            return this;
        }

        /// <summary>
        /// Use this function to set the <see cref="IDbJobState"/> for this <see cref="IDbJob{T}"/>.
        /// </summary>
        /// <param name="state">The <see cref="IDbJobState"/> to use.</param>
        /// <param name="isUseLock">Set to true in order to use locking. (Optional)</param>
        /// <returns><see cref="IDbJob{T}"/></returns>
        protected internal virtual IDbJob<T> WithState(IDbJobState state, bool isUseLock = false)
        {
            if (isUseLock)
            {
                lock (_executionLock)
                {
                    lock (_cloneLock)
                    {
                        _state = state;
                    }
                }
            }
            else
            {
                _state = state;
            }

            return this;
        }

        /// <summary>
        /// Action to call before each connection is opened in order to "branch" the result. Note: Should only be called when initiating this <see cref="IDbJob{T}"/>.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="setting">The event setting to use. (Optional)</param>
        /// <returns><see cref="IDbJob{T}"/></returns>
        protected internal virtual DbJobBase<T> OnBranch(Func<DbBranchResult<T>, IDbExecutionModel, IDbJob<T>, DbBranchResult<T>> action, EventSetting setting = EventSetting.Subscribe)
        {
            switch (setting)
            {
                case EventSetting.Subscribe:
                    _onBranch += action;
                    break;
                case EventSetting.Unsubscribe:
                    _onBranch -= action;
                    break;
                case EventSetting.Replace:
                    _onBranch = action;
                    break;
                default:
                    break;
            }

            return this;
        }

        /// <summary>
        /// Action to call after a DbCommand is executed. Note: Should only be called when initiating this <see cref="IDbJob{T}"/>.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="setting">The event setting to use. (Optional)</param>
        /// <returns><see cref="IDbJob{T}"/></returns>
        protected internal virtual IDbJob<T> OnExecuted(Func<object, IDbExecutedModel, object> action, EventSetting setting = EventSetting.Subscribe)
        {
            switch (setting)
            {
                case EventSetting.Subscribe:
                    _onExecuted += (d, e) => (T)action(d, e);
                    break;
                case EventSetting.Unsubscribe:
                    _onExecuted -= (d, e) => (T)action(d, e);
                    break;
                case EventSetting.Replace:
                    _onExecuted = (d, e) => (T)action(d, e);
                    break;
                default:
                    break;
            }

            return this;
        }

        /// <summary>
        /// Use this to set the delegate to call after a DbCommand is executed. You can use this to change the <see cref="{T}"/> result based on the event.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="setting">The event setting to use. (Optional)</param>
        /// <returns><see cref="IDbJob{T}"/></returns>
        public virtual IDbJob<T> OnExecuted(Func<T, IDbExecutedModel, T> action, EventSetting setting = EventSetting.Subscribe)
        {
            lock (_executionLock)
            {
                lock (_cloneLock)
                {
                    switch (setting)
                    {
                        case EventSetting.Subscribe:
                            _onExecuted += action;
                            break;
                        case EventSetting.Unsubscribe:
                            _onExecuted -= action;
                            break;
                        case EventSetting.Replace:
                            _onExecuted = action;
                            break;
                        default:
                            break;
                    }
                }

                return this;
            }
        }

        /// <summary>
        /// Use this to set the delegate to call after all DbCommands are executed without errors. You can use this to change the <see cref="{T}"/> result based on the event.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="setting">The event setting to use. (Optional)</param>
        /// <returns><see cref="IDbJob{T}"/></returns>
        public virtual IDbJob<T> OnCompleted(Func<T, T> action, EventSetting setting = EventSetting.Subscribe)
        {
            lock (_executionLock)
            {
                lock (_cloneLock)
                {
                    switch (setting)
                    {
                        case EventSetting.Subscribe:
                            _onCompleted += action;
                            break;
                        case EventSetting.Unsubscribe:
                            _onCompleted -= action;
                            break;
                        case EventSetting.Replace:
                            _onCompleted = action;
                            break;
                        default:
                            break;
                    }
                }

                return this;
            }
        }

        /// <summary>
        /// Use this to set the delegate to call when an error occurs. You can use this to change the <see cref="{T}"/> result based on the event.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="setting">The event setting to use. (Optional)</param>
        /// <returns><see cref="IDbJob{T}"/></returns>
        public virtual IDbJob<T> OnError(Func<T, Exception, T> action, EventSetting setting = EventSetting.Subscribe)
        {
            lock (_executionLock)
            {
                switch (setting)
                {
                    case EventSetting.Subscribe:
                        _onError += action;
                        break;
                    case EventSetting.Unsubscribe:
                        _onError -= action;
                        break;
                    case EventSetting.Replace:
                        _onError = action;
                        break;
                    default:
                        break;
                }

                return this;
            }
        }

        /// <summary>
        /// Use this to set the delegate to call when an error occurs. You can use this to change the <see cref="{T}"/> result based on the event.
        /// <para>Note: Should only be called when initiating this <see cref="IDbJob{T}"/>.</para>
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <returns><see cref="IDbJob{T}"/></returns>
        protected internal virtual DbJobBase<T> SetOnError(Func<T, Exception, T> action)
        {
            _onError = action;
            return this;
        }

        /// <summary>
        /// <para>Use this function to disable buffered execution when reading data (this is enabled by default).</para>
        /// <para>Note: Should only be called when initiating this <see cref="IDbJob{T}"/>.</para>
        /// <para>Note: The use of deferred execution is only possible for <see cref="System.Collections.IEnumerable"/> types during individual transactions. Normal execution will be used when encountering non <see cref="System.Collections.IEnumerable"/> types or Batch-Reading implementations.</para>
        /// </summary>
        /// <remarks>
        /// <para>Warning: Deferred execution leverages "yield statement" logic and postpones the disposal of database connections and related resources. 
        /// Always perform an iteration of the returned <see cref="System.Collections.IEnumerable"/> by either implementing a "for-each" loop or a data projection (e.g. invoking the <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})"/> extension). You can also dispose the enumerator as an alternative.
        /// Not doing so will internally leave disposable resources opened (e.g. database connections) consequently creating memory leak scenarios.
        /// </para>
        /// <para>Warning: Exceptions may occur while looping deferred <see cref="System.Collections.IEnumerable"/> types because of the implicit database connection dependency.</para>
        /// </remarks>
        /// <returns><see cref="DbJobBase{T}"/></returns>
        protected internal virtual DbJobBase<T> WithoutBuffering()
        {
            _isDeferredExecution = true;
            return this;
        }

        /// <summary>
        /// Use this to create an <see cref="IDbJobCacheable{T, TStateParamValue}"/>.
        /// <para>Note: This should only be used when wanting to cache the current <see cref="IDbJob{T}"/> (e.g. when caching in a static field).</para>
        /// </summary>
        /// <typeparam name="TStateParamValue">The state parameter type to use.</typeparam>
        /// <param name="value">The state parameter value to use</param>
        /// <returns>The new <see cref="IDbJobCacheable{T, TStateParamValue}"/>.</returns>
        public abstract IDbJobCacheable<T, TStateParamValue> ToCacheable<TStateParamValue>(TStateParamValue value);

        /// <summary>
        /// Use this to create an <see cref="IDbJobCacheable{T, TStateParamValue}"/>.
        /// <para>Note: This should only be used when wanting to cache the current <see cref="IDbJob{T}"/> (e.g. when caching in a static field).</para>
        /// </summary>
        /// <typeparam name="TStateParamValue">The state parameter type to use.</typeparam>
        /// <returns>The new <see cref="IDbJobCacheable{T, TStateParamValue}"/>.</returns>
        public IDbJobCacheable<T, TStateParamValue> ToCacheable<TStateParamValue>()
        {
            return ToCacheable<TStateParamValue>(default);
        }

        #endregion        
    }

    /// <summary>
    /// Represents a configurable and executable database job.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TStateParam"></typeparam>
    public abstract class DbJobBase<T, TStateParam> : DbJobBase<T>
    {
        #region Properties

        protected internal new IDbJobState<TStateParam> _state;

        #endregion


        #region Constructors

        /// <summary>
        /// Copy Constructor
        /// <para>Note: This will use a clone lock in order to prevent race conditions.</para>
        /// </summary>
        /// <param name="jobToCopy"></param>
        public DbJobBase(DbJobBase<T, TStateParam> jobToCopy)
            : base(jobToCopy)
        {
        }

        public DbJobBase(
            IDbJobSetting setting,
            IDbJobState<TStateParam> state,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute)
            : base(setting, state, onCommands, onExecute)
        {
            _state = state;
        }

        public DbJobBase(
            IDbJobSetting setting,
            IDbJobState<TStateParam> state,
            Func<T> onInit,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute)
            : base(setting, state, onInit, onCommands, onExecute)
        {
            _state = state;
        }

        public DbJobBase(
            IDbJobSetting setting,
            IDbJobState<TStateParam> state,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute,
            bool isCreateDbCommand)
            : base(setting, state, onCommands, onExecute, isCreateDbCommand)
        {
            _state = state;
        }

        public DbJobBase(
            IDbJobSetting setting,
            IDbJobState<TStateParam> state,
            Func<T> onInit,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute,
            bool isCreateDbCommand)
            : base(setting, state, onInit, onCommands, onExecute, isCreateDbCommand)
        {
            _state = state;
        }

        #endregion


        #region Implementation

        protected internal override DbExecutionModel CreateDbExecutionModel(bool isBuffered, bool isDisposable, DbConnection connection, DbTransaction transaction, IDbJobState jobState, CancellationToken token)
        {
            return new DbExecutionModel<TStateParam>(isBuffered, isDisposable, connection, transaction, (jobState as IDbJobState<TStateParam>), token);
        }

        protected internal override IDbJobCommand CreateDbJobCommand(DbCommand cmd, IDbJobState jobState)
        {
            return new DbJobCommand<TStateParam>(cmd, jobState == null ? default : (jobState as IDbJobState<TStateParam>).StateParam);
        }

        protected internal override bool IsWithStateParam()
        {
            return true;
        }

        #endregion
    }




    internal class DbJob<T, TDbConnection> : DbJobBase<T>
        where TDbConnection : DbConnection
    {
        #region Constructors

        public DbJob(
            IDbJobSetting setting,
            IDbJobState state,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute)
            : base(setting, state, onCommands, onExecute)
        {

        }

        public DbJob(
            IDbJobSetting setting,
            IDbJobState state,
            Func<T> onInit,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute)
            : base(setting, state, onInit, onCommands, onExecute)
        {

        }

        public DbJob(
            IDbJobSetting setting,
            IDbJobState state,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute,
            bool isCreateDbCommand)
            : base(setting, state, onCommands, onExecute, isCreateDbCommand)
        {

        }

        public DbJob(
            IDbJobSetting setting,
            IDbJobState state,
            Func<T> onInit,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute,
            bool isCreateDbCommand)
            : base(setting, state, onInit, onCommands, onExecute, isCreateDbCommand)
        {

        }

        /// <summary>
        /// Copy Constructor
        /// <para>Note: This will use a clone lock in order to prevent race conditions.</para>
        /// </summary>
        /// <param name="jobToCopy"></param>
        public DbJob(DbJobBase<T> jobToCopy)
            : base(jobToCopy)
        {

        }

        #endregion

        protected override DbJobBase<T> Clone()
        {
            //Clone the job using the "Copy Constructor" since the developer might try to get fancy.
            //This clone is required since connection execution is serialized internally by a "lock".
            //Note: the developer is responsible of making the delegate events "thread safe"!
            return new DbJob<T, TDbConnection>(this);
        }

        public override IDbJobCacheable<T, TStateParamValue> ToCacheable<TStateParamValue>(TStateParamValue value)
        {
            return ToCacheable(value, this);
        }

        internal static IDbJobCacheable<T, TStateParamValue> ToCacheable<TStateParamValue>(TStateParamValue value, DbJobBase<T> job)
        {
            lock (job._executionLock)
            {
                lock (job._cloneLock)
                {
                    return new DbJobCacheable<T, TStateParamValue, TDbConnection>(
                                job._settings.Clone(),
                                job._state == null ? new DbJobState<TStateParamValue>() { StateParam = value } : job._state.Clone(value),
                                job._onInit,
                                job._onCommands,
                                job._onExecute,
                                job._isCreateDbCommand)
                    {
                        _isolationLevel = job._isolationLevel,

                        //booleans
                        _isCacheEnabled = job._isCacheEnabled,
                        _isLoggingEnabled = job._isLoggingEnabled,
                        _isDeferredExecution = job._isDeferredExecution,
                        _isIsolatedConnections = job._isIsolatedConnections,

                        //Events
                        _onExecuted = job._onExecuted,
                        _onCompleted = job._onCompleted,
                        _onError = job._onError,
                        _onBranch = job._onBranch
                    };
                }
            }
        }
    }


    internal class DbJob<T, TStateParam, TDbConnection> : DbJobBase<T, TStateParam>
        where TDbConnection : DbConnection
    {
        #region Constructors

        public DbJob(
            IDbJobSetting setting,
            IDbJobState<TStateParam> state,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute)
            : base(setting, state, onCommands, onExecute)
        {

        }

        public DbJob(
            IDbJobSetting setting,
            IDbJobState<TStateParam> state,
            Func<T> onInit,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute)
            : base(setting, state, onInit, onCommands, onExecute)
        {

        }

        public DbJob(
            IDbJobSetting setting,
            IDbJobState<TStateParam> state,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute,
            bool isCreateDbCommand)
            : base(setting, state, onCommands, onExecute, isCreateDbCommand)
        {

        }

        public DbJob(
            IDbJobSetting setting,
            IDbJobState<TStateParam> state,
            Func<T> onInit,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute,
            bool isCreateDbCommand)
            : base(setting, state, onInit, onCommands, onExecute, isCreateDbCommand)
        {

        }

        /// <summary>
        /// Copy Constructor
        /// <para>Note: This will use a clone lock in order to prevent race conditions.</para>
        /// </summary>
        /// <param name="jobToCopy"></param>
        public DbJob(DbJobBase<T, TStateParam> jobToCopy)
            : base(jobToCopy)
        {

        }

        #endregion

        protected override DbJobBase<T> Clone()
        {
            //Clone the job using the "Copy Constructor" since the developer might try to get fancy.
            //This clone is required since connection execution is serialized internally by a "lock".
            //Note: the developer is responsible of making the delegate events "thread safe"!
            return new DbJob<T, TStateParam, TDbConnection>(this);
        }

        public override IDbJobCacheable<T, TStateParamValue> ToCacheable<TStateParamValue>(TStateParamValue value)
        {
            return DbJob<T, TDbConnection>.ToCacheable(value, this);
        }
    }


    internal class DbJobCacheable<T, TStateParam, TDbConnection> : DbJob<T, TStateParam, TDbConnection>, IDbJobCacheable<T, TStateParam>
        where TDbConnection : DbConnection
    {
        #region Constructors

        public DbJobCacheable(
            IDbJobSetting setting,
            IDbJobState<TStateParam> state,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute)
            : base(setting, state, onCommands, onExecute)
        {
            Compile();
        }

        public DbJobCacheable(
            IDbJobSetting setting,
            IDbJobState<TStateParam> state,
            Func<T> onInit,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute)
            : base(setting, state, onInit, onCommands, onExecute)
        {
            Compile();
        }

        public DbJobCacheable(
            IDbJobSetting setting,
            IDbJobState<TStateParam> state,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute,
            bool isCreateDbCommand)
            : base(setting, state, onCommands, onExecute, isCreateDbCommand)
        {
            Compile();
        }

        public DbJobCacheable(
            IDbJobSetting setting,
            IDbJobState<TStateParam> state,
            Func<T> onInit,
            Func<DbConnection, IDbJobState, IDbJobCommand[]> onCommands,
            Func<T, IDbExecutionModel, T> onExecute,
            bool isCreateDbCommand)
            : base(setting, state, onInit, onCommands, onExecute, isCreateDbCommand)
        {
            Compile();
        }

        public DbJobCacheable(DbJobBase<T, TStateParam> jobToCopy)
            : base(jobToCopy)
        {
            Compile();
        }

        #endregion


        #region Implementation

        protected internal Func<DbConnection> DbConnectionBuilder { get; set; }

        protected internal void Compile()
        {
            //DbConnectionBuilder
            if (!DbConnectorCache.DbConnectionBuilderCache.TryGetValue(_settings.DbConnectionType, out Func<DbConnection> onCreateInstance))
            {
                //Compile and cache the DbConnectionBuilder
                DbConnectionBuilder = DynamicInstanceBuilder.CreateBuilderFunction<DbConnection>(_settings.DbConnectionType);
            }
            else
            {
                //Cache the DbConnectionBuilder
                DbConnectionBuilder = onCreateInstance;
            }

            //TODO: Compile and cache the execution implementations using expression trees?
        }

        protected internal override DbConnection CreateConnectionInstance()
        {
            return DbConnectionBuilder();
        }



        private T TryExecute(TStateParam parameter, DbConnection connection, DbTransaction transaction, CancellationToken token, bool? isThrowExceptions)
        {
            object cloneLock = _cloneLock;
            bool isLockAcquired = false;
            T result = default;
            try
            {
                Monitor.TryEnter(cloneLock, ref isLockAcquired);

                if (isLockAcquired)
                {
                    _state.StateParam = parameter;
                    ExecuteImplementation(ref result, connection, transaction, token, _state);
                }
                else
                {
                    ExecuteImplementation(ref result, connection, transaction, token, _state.Clone(parameter));
                }
            }
            catch (Exception ex)
            {
                isThrowExceptions = isThrowExceptions ?? _settings.IsThrowExceptions;

                ex.Log(_settings.Logger, isThrowExceptions.Value ? false : _isLoggingEnabled);

                if (_onError != null)
                {
                    try
                    {
                        result = _onError(result, ex);
                    }
                    catch (Exception exInner)
                    {
                        exInner.Log(_settings.Logger, isThrowExceptions.Value ? false : _isLoggingEnabled);

                        if (isThrowExceptions.Value)
                        {
                            throw;
                        }
                    }
                }

                if (isThrowExceptions.Value)
                {
                    throw;
                }
            }
            finally
            {
                if (isLockAcquired)
                {
                    Monitor.Exit(cloneLock);
                }
            }

            return result;
        }

        /// <summary>
        /// Execute the <see cref="IDbJobCacheable{T, TStateParam}"/>.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>      
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The T result.</returns>
        public virtual T Execute(TStateParam parameter, DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            return TryExecute(parameter, connection, null, token, isThrowExceptions);
        }

        /// <summary>
        /// Execute the <see cref="IDbJobCacheable{T, TStateParam}"/>.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Note: This will override the use of <see cref="IDbJob{T}.WithIsolationLevel(IsolationLevel?)"/>.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>    
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The T result.</returns>
        public virtual T Execute(TStateParam parameter, DbTransaction transaction, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            return TryExecute(parameter, transaction?.Connection, transaction, token, isThrowExceptions);
        }

        /// <summary>
        /// Execute the <see cref="IDbJobCacheable{T, TStateParam}"/> asynchronously.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The <see cref="Task{T}"/>.</returns>
        public virtual Task<T> ExecuteAsync(TStateParam parameter, DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            return Task.Run(() => TryExecute(parameter, connection, null, token, isThrowExceptions), token);
        }

        /// <summary>
        /// Execute the <see cref="IDbJobCacheable{T, TStateParam}"/> asynchronously.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Note: This will override the use of <see cref="IDbJob{T}.WithIsolationLevel(IsolationLevel?)"/>.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>  
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The <see cref="Task{T}"/>.</returns>
        public virtual Task<T> ExecuteAsync(TStateParam parameter, DbTransaction transaction, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            return Task.Run(() => TryExecute(parameter, transaction?.Connection, transaction, token, isThrowExceptions), token);
        }



        private IDbResult<T> TryExecuteHandled(TStateParam parameter, DbConnection connection, DbTransaction transaction, CancellationToken token)
        {
            object cloneLock = _cloneLock;
            bool isLockAcquired = false;
            IDbResult<T> result = new DbResult<T>();
            T data = default;
            try
            {
                Monitor.TryEnter(cloneLock, ref isLockAcquired);

                if (isLockAcquired)
                {
                    _state.StateParam = parameter;
                    ExecuteImplementation(ref data, connection, transaction, token, _state);
                }
                else
                {
                    ExecuteImplementation(ref data, connection, transaction, token, _state.Clone(parameter));
                }

                result.Data = data;
            }
            catch (Exception ex)
            {
                ex.Log(_settings.Logger, _isLoggingEnabled);

                if (_onError != null)
                {
                    try
                    {
                        result.Data = _onError(data, ex);
                    }
                    catch (Exception exInner)
                    {
                        exInner.Log(_settings.Logger, _isLoggingEnabled);

                        ex = exInner;
                    }
                }

                result.Error = ex;
            }
            finally
            {
                if (isLockAcquired)
                {
                    Monitor.Exit(cloneLock);
                }
            }

            return result;
        }

        /// <summary>
        /// Execute the <see cref="IDbJobCacheable{T, TStateParam}"/> and handle any exceptions while opening the <see cref="DbConnection"/>.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbResult{T}"/>.</returns>
        public virtual IDbResult<T> ExecuteHandled(TStateParam parameter, DbConnection connection = null, CancellationToken token = default)
        {
            return TryExecuteHandled(parameter, connection, null, token);
        }

        /// <summary>
        /// Execute the <see cref="IDbJobCacheable{T, TStateParam}"/> and handle any exceptions while opening the <see cref="DbConnection"/>.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Note: This will override the use of <see cref="IDbJob{T}.WithIsolationLevel(IsolationLevel?)"/>.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>  
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbResult{T}"/>.</returns>
        public virtual IDbResult<T> ExecuteHandled(TStateParam parameter, DbTransaction transaction, CancellationToken token = default)
        {
            return TryExecuteHandled(parameter, transaction?.Connection, transaction, token);
        }

        /// <summary>
        /// Execute the <see cref="IDbJobCacheable{T, TStateParam}"/> and handle any exceptions while opening the <see cref="DbConnection"/> asynchronously.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="Task{IDbResult{T}}"/>.</returns>
        public virtual Task<IDbResult<T>> ExecuteHandledAsync(TStateParam parameter, DbConnection connection = null, CancellationToken token = default)
        {
            return Task.Run(() => TryExecuteHandled(parameter, connection, null, token), token);
        }

        /// <summary>
        /// Execute the <see cref="IDbJobCacheable{T, TStateParam}"/> and handle any exceptions while opening the <see cref="DbConnection"/> asynchronously.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Note: This will override the use of <see cref="IDbJob{T}.WithIsolationLevel(IsolationLevel?)"/>.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>  
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="Task{IDbResult{T}}"/>.</returns>
        public virtual Task<IDbResult<T>> ExecuteHandledAsync(TStateParam parameter, DbTransaction transaction, CancellationToken token = default)
        {
            return Task.Run(() => TryExecuteHandled(parameter, transaction?.Connection, transaction, token), token);
        }



        private IDbDisposable<T> TryExecuteDisposable(TStateParam parameter, DbConnection connection, DbTransaction transaction, CancellationToken token, bool? isThrowExceptions)
        {
            object cloneLock = _cloneLock;
            bool isLockAcquired = false;
            DbDisposable<T> result = default;
            try
            {
                Monitor.TryEnter(cloneLock, ref isLockAcquired);

                if (isLockAcquired)
                {
                    _state.StateParam = parameter;
                    ExecuteDisposableImplementation(ref result, connection, transaction, token, _state);
                }
                else
                {
                    ExecuteDisposableImplementation(ref result, connection, transaction, token, _state.Clone(parameter));
                }
            }
            catch (Exception ex)
            {
                result?.Dispose(false);

                isThrowExceptions = isThrowExceptions ?? (_settings.IsThrowExceptions);

                ex.Log(_settings.Logger, isThrowExceptions.Value ? false : _isLoggingEnabled);

                if (result != null && _onError != null)
                {
                    try
                    {
                        result.Source = _onError(result.Source, ex);
                    }
                    catch (Exception exInner)
                    {
                        exInner.Log(_settings.Logger, isThrowExceptions.Value ? false : _isLoggingEnabled);

                        if (isThrowExceptions.Value)
                        {
                            throw;
                        }
                    }
                }

                if (isThrowExceptions.Value)
                {
                    throw;
                }
            }
            finally
            {
                if (isLockAcquired)
                {
                    Monitor.Exit(cloneLock);
                }
            }

            return result;
        }

        /// <summary>
        /// Use this function when a serialized data extraction is required. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The <see cref="IDbDisposable{T}"/>.</returns>
        public virtual IDbDisposable<T> ExecuteDisposable(TStateParam parameter, DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            return TryExecuteDisposable(parameter, connection, null, token, isThrowExceptions);
        }

        /// <summary>
        /// Use this function when a serialized data extraction is required. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Note: This will override the use of <see cref="IDbJob{T}.WithIsolationLevel(IsolationLevel?)"/>.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param> 
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The <see cref="IDbDisposable{T}"/>.</returns>
        public virtual IDbDisposable<T> ExecuteDisposable(TStateParam parameter, DbTransaction transaction, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            return TryExecuteDisposable(parameter, transaction?.Connection, transaction, token, isThrowExceptions);
        }

        /// <summary>
        /// Use this asynchronous function when a serialized data extraction is required. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The <see cref="Task{IDbDisposable{T}}"/>.</returns>
        public virtual Task<IDbDisposable<T>> ExecuteDisposableAsync(TStateParam parameter, DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            return Task.Run(() => TryExecuteDisposable(parameter, connection, null, token, isThrowExceptions), token);
        }

        /// <summary>
        /// Use this asynchronous function when a serialized data extraction is required. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Note: This will override the use of <see cref="IDbJob{T}.WithIsolationLevel(IsolationLevel?)"/>.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param> 
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The <see cref="Task{IDbDisposable{T}}"/>.</returns>
        public virtual Task<IDbDisposable<T>> ExecuteDisposableAsync(TStateParam parameter, DbTransaction transaction, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            return Task.Run(() => TryExecuteDisposable(parameter, transaction?.Connection, transaction, token, isThrowExceptions), token);
        }



        private IDbResult<IDbDisposable<T>> TryExecuteDisposableHandled(TStateParam parameter, DbConnection connection, DbTransaction transaction, CancellationToken token)
        {
            object cloneLock = _cloneLock;
            bool isLockAcquired = false;
            IDbResult<IDbDisposable<T>> result = new DbResult<IDbDisposable<T>>();
            DbDisposable<T> disposableResult = default;
            try
            {
                Monitor.TryEnter(cloneLock, ref isLockAcquired);

                if (isLockAcquired)
                {
                    _state.StateParam = parameter;
                    ExecuteDisposableImplementation(ref disposableResult, connection, transaction, token, _state);
                }
                else
                {
                    ExecuteDisposableImplementation(ref disposableResult, connection, transaction, token, _state.Clone(parameter));
                }

                result.Data = disposableResult;
            }
            catch (Exception ex)
            {
                disposableResult?.Dispose(false);

                ex.Log(_settings.Logger, _isLoggingEnabled);

                if (disposableResult != null && _onError != null)
                {
                    try
                    {
                        disposableResult.Source = _onError(disposableResult.Source, ex);

                        result.Data = disposableResult;
                    }
                    catch (Exception exInner)
                    {
                        exInner.Log(_settings.Logger, _isLoggingEnabled);

                        ex = exInner;
                    }
                }

                result.Error = ex;
            }
            finally
            {
                if (isLockAcquired)
                {
                    Monitor.Exit(cloneLock);
                }
            }

            return result;
        }

        /// <summary>
        /// Use this function when a serialized data extraction is required and to handle any exceptions while opening the <see cref="DbConnection"/>. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbResult{IDbDisposable{T}}"/>.</returns>
        public virtual IDbResult<IDbDisposable<T>> ExecuteDisposableHandled(TStateParam parameter, DbConnection connection = null, CancellationToken token = default)
        {
            return TryExecuteDisposableHandled(parameter, connection, null, token);
        }

        /// <summary>
        /// Use this function when a serialized data extraction is required and to handle any exceptions while opening the <see cref="DbConnection"/>. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Note: This will override the use of <see cref="IDbJob{T}.WithIsolationLevel(IsolationLevel?)"/>.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param> 
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbResult{IDbDisposable{T}}"/>.</returns>
        public virtual IDbResult<IDbDisposable<T>> ExecuteDisposableHandled(TStateParam parameter, DbTransaction transaction, CancellationToken token = default)
        {
            return TryExecuteDisposableHandled(parameter, transaction?.Connection, transaction, token);
        }

        /// <summary>
        /// Use this asynchronous function when a serialized data extraction is required and to handle any exceptions while opening the <see cref="DbConnection"/>. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="Task{IDbResult{IDbDisposable{T}}}"/>.</returns>
        public virtual Task<IDbResult<IDbDisposable<T>>> ExecuteDisposableHandledAsync(TStateParam parameter, DbConnection connection = null, CancellationToken token = default)
        {
            return Task.Run(() => TryExecuteDisposableHandled(parameter, connection, null, token), token);
        }

        /// <summary>
        /// Use this asynchronous function when a serialized data extraction is required and to handle any exceptions while opening the <see cref="DbConnection"/>. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// <para>Note: This will override the use of <see cref="IDbJob{T}.WithIsolationLevel(IsolationLevel?)"/>.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param> 
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="Task{IDbResult{IDbDisposable{T}}}"/>.</returns>
        public virtual Task<IDbResult<IDbDisposable<T>>> ExecuteDisposableHandledAsync(TStateParam parameter, DbTransaction transaction, CancellationToken token = default)
        {
            return Task.Run(() => TryExecuteDisposableHandled(parameter, transaction?.Connection, transaction, token), token);
        }

        #endregion


        #region Safety Overrides

        public override IDbResult<T> ExecuteHandled(DbConnection connection = null, CancellationToken token = default)
        {
            throw new InvalidOperationException("This function is not supported for the current IDbJob state!");
        }


        public override Task<IDbResult<T>> ExecuteHandledAsync(DbConnection connection = null, CancellationToken token = default)
        {
            throw new InvalidOperationException("This function is not supported for the current IDbJob state!");
        }


        public override T Execute(DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            throw new InvalidOperationException("This function is not supported for the current IDbJob state!");
        }


        public override Task<T> ExecuteAsync(DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            throw new InvalidOperationException("This function is not supported for the current IDbJob state!");
        }


        public override IDbResult<IDbDisposable<T>> ExecuteDisposableHandled(DbConnection connection = null, CancellationToken token = default)
        {
            throw new InvalidOperationException("This function is not supported for the current IDbJob state!");
        }


        public override Task<IDbResult<IDbDisposable<T>>> ExecuteDisposableHandledAsync(DbConnection connection = null, CancellationToken token = default)
        {
            throw new InvalidOperationException("This function is not supported for the current IDbJob state!");
        }


        public override IDbDisposable<T> ExecuteDisposable(DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            throw new InvalidOperationException("This function is not supported for the current IDbJob state!");
        }


        public override Task<IDbDisposable<T>> ExecuteDisposableAsync(DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null)
        {
            throw new InvalidOperationException("This function is not supported for the current IDbJob state!");
        }


        public override IDbJob<T> WithLogging(bool isEnabled)
        {
            throw new InvalidOperationException("This function is not supported for the current IDbJob state!");
        }


        public override IDbJob<T> WithIsolationLevel(IsolationLevel? level)
        {
            throw new InvalidOperationException("This function is not supported for the current IDbJob state!");
        }


        public override IDbJob<T> WithBuffering(bool isEnabled)
        {
            throw new InvalidOperationException("This function is not supported for the current IDbJob state!");
        }


        public override IDbJob<T> WithCache(bool isEnabled)
        {
            throw new InvalidOperationException("This function is not supported for the current IDbJob state!");
        }


        public override IDbJob<T> OnExecuted(Func<T, IDbExecutedModel, T> action, EventSetting setting = EventSetting.Subscribe)
        {
            throw new InvalidOperationException("This function is not supported for the current IDbJob state!");
        }


        public override IDbJob<T> OnCompleted(Func<T, T> action, EventSetting setting = EventSetting.Subscribe)
        {
            throw new InvalidOperationException("This function is not supported for the current IDbJob state!");
        }


        public override IDbJob<T> OnError(Func<T, Exception, T> action, EventSetting setting = EventSetting.Subscribe)
        {
            throw new InvalidOperationException("This function is not supported for the current IDbJob state!");
        }

        #endregion
    }
}
