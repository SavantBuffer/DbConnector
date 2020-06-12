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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DbConnector.Core
{
    /// <summary>
    /// A performance-driven and ADO.NET data provider-agnostic ORM library.
    /// <para>Note: This class should be used as a singleton for optimal performance.</para>
    /// </summary>
    /// <typeparam name="TDbConnection">The <see cref="DbConnection"/> type to use.</typeparam>
    public partial class DbConnector<TDbConnection> : IDbConnector<TDbConnection>
       where TDbConnection : DbConnection
    {
        #region Main

        private readonly DbJobSetting _jobSetting;
        private readonly CalculatedDbConnectorFlags _flags;
        private static readonly CommandBehavior _commandBehaviorSingleResultOrSingleRow;
        private static readonly MethodInfo[] _multiReaderMethods;
        private static readonly MethodInfo[] _multiReaderMethodsByState;

        /// <summary>
        /// Static constructor
        /// </summary>
        static DbConnector()
        {
            //Cache default CommandBehavior
            _commandBehaviorSingleResultOrSingleRow = CommandBehavior.SingleResult | CommandBehavior.SingleRow;

            //Cache MethodInfos for "multi reader" use.
            Type dcType = typeof(DbConnector<TDbConnection>);

            _multiReaderMethods = dcType
                                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                .Where(m => m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 1).ToArray();

            _multiReaderMethodsByState = dcType
                                        .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                        .Where(m => m.Name.EndsWith("ByState")).ToArray();
        }

        /// <summary>
        /// A generic database connector and lightweight ORM.
        /// <para>Note: This class should be used as a singleton for optimal performance.</para>
        /// </summary>
        /// <param name="connectionString"> The connection string to use.</param>
        public DbConnector(string connectionString)
            : this(connectionString, null, DbConnectorFlags.None)
        {
        }

        /// <summary>
        /// A generic database connector and lightweight ORM.
        /// <para>Note: This class should be used as a singleton for optimal performance.</para>
        /// </summary>
        /// <param name="connectionString"> The connection string to use.</param>
        /// <param name="flags">The <see cref="DbConnectorFlags"/> to use.</param>
        public DbConnector(string connectionString, DbConnectorFlags flags)
            : this(connectionString, null, flags)
        {
        }

        /// <summary>
        /// A generic database connector and lightweight ORM.
        /// <para>Note: This class should be used as a singleton for optimal performance.</para>
        /// </summary>
        /// <param name="connectionString"> The connection string to use.</param>
        /// <param name="logger">The <see cref="IDbConnectorLogger"/> to use for error logging.</param>
        public DbConnector(string connectionString, IDbConnectorLogger logger)
            : this(connectionString, logger, DbConnectorFlags.None)
        {
        }

        /// <summary>
        /// A generic database connector and lightweight ORM.
        /// <para>Note: This class should be used as a singleton for optimal performance.</para>
        /// </summary>
        /// <param name="connectionString"> The connection string to use.</param>
        /// <param name="logger">The <see cref="IDbConnectorLogger"/> to use for error logging.</param>
        /// <param name="flags">The <see cref="DbConnectorFlags"/> to use.</param>
        public DbConnector(string connectionString, IDbConnectorLogger logger, DbConnectorFlags flags)
        {
            _jobSetting = new DbJobSetting
            {
                DbConnectionType = typeof(TDbConnection),
                ConnectionString = connectionString,
                Logger = logger
            };

            if ((flags & DbConnectorFlags.NoCache) == DbConnectorFlags.None)
            {
                _jobSetting.IsCacheEnabled = true;
            }

            if ((flags & DbConnectorFlags.NoExceptionThrowingForNonHandledExecution) == DbConnectorFlags.None)
            {
                _jobSetting.IsThrowExceptions = true;
            }

            if (_jobSetting.IsCacheEnabled && (flags & DbConnectorFlags.NoDbConnectionInstanceBuilderCaching) == DbConnectorFlags.None)
            {
                if (!DbConnectorCache.DbConnectionBuilderCache.ContainsKey(_jobSetting.DbConnectionType))
                {
                    DbConnectorCache.DbConnectionBuilderCache.TryAdd(_jobSetting.DbConnectionType, DynamicInstanceBuilder.CreateBuilderFunction<DbConnection>(_jobSetting.DbConnectionType));
                }
            }

            _flags = new CalculatedDbConnectorFlags(
                (flags & DbConnectorFlags.NoCommandBehaviorOptimization) == DbConnectorFlags.None,
                (flags & DbConnectorFlags.NoAutoSequentialAccessCommandBehavior) == DbConnectorFlags.None,
                (flags & DbConnectorFlags.NoIsolatedConnectionPerCommand) == DbConnectorFlags.None
                );
        }

        private static CommandBehavior ConfigureCommandBehavior(IDbExecutionModel p, CommandBehavior defaultBehavior)
        {
            var flags = (p.JobState as IDbConnectorState).Flags;
            IDbJobCommand jobCommand = p.JobCommand;

            if (!flags.IsCommandBehaviorOptimization)
            {
                return jobCommand.CommandBehavior ?? CommandBehavior.Default;
            }
            else
            {
                CommandBehavior jobCommandBehavior = jobCommand.CommandBehavior ?? defaultBehavior;

                if (
                       flags.IsAutoSequentialAccessCommandBehavior
                    && (jobCommand.Flags & DbJobCommandFlags.NoAutoSequentialAccessCommandBehavior) == DbJobCommandFlags.None
                    && (jobCommandBehavior == defaultBehavior || (
                           (jobCommandBehavior & CommandBehavior.SchemaOnly) == CommandBehavior.Default
                        && (jobCommandBehavior & CommandBehavior.KeyInfo) == CommandBehavior.Default
                       ))
                )
                {
                    jobCommandBehavior = CommandBehavior.SequentialAccess | jobCommandBehavior;
                }

                return jobCommandBehavior;
            }
        }

        private static IDbJobCommand[] BuildJobCommand(DbConnection conn, IDbJobState state)
        {
            var currentState = (state as DbConnectorState);
            var cmdModel = currentState.CreateDbJobCommand(conn.CreateCommand());

            currentState.OnInit(cmdModel);

            return new IDbJobCommand[1] { cmdModel };
        }

        private static IDbJobCommand[] BuildJobCommandForSimpleState(
           DbConnection conn,
           IDbJobState state,
           string commandText,
           object param = null,
           CommandType commandType = CommandType.Text)
        {
            var currentState = (state as DbConnectorSimpleState);
            var cmd = currentState.CreateDbJobCommand(conn.CreateCommand());

            cmd.CommandType = commandType;
            cmd.CommandText = commandText;

            cmd.Parameters.AddFor(param);

            return new IDbJobCommand[1] { cmd };
        }

        private static IDbJobCommand[] BuildJobCommandForSimpleState(
            DbConnection conn,
            IDbJobState state,
            IColumnMapSetting mapSettings,
            string commandText,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            var currentState = (state as DbConnectorSimpleState);
            var cmd = currentState.CreateDbJobCommand(conn.CreateCommand(), mapSettings);

            cmd.CommandType = commandType;
            cmd.CommandBehavior = commandBehavior;
            cmd.CommandText = commandText;

            if (commandTimeout.HasValue)
            {
                cmd.CommandTimeout = commandTimeout.Value;
            }

            cmd.Flags = flags;
            cmd.Parameters.AddFor(param);

            return new IDbJobCommand[1] { cmd };
        }

        private static IDbJobCommand[] BuildJobActionCommands(DbConnection conn, IDbJobState state)
        {
            var currentState = (state as DbConnectorActionQueuedState);
            var actions = GetReaderActions(currentState.OnInit);

            IDbJobCommand[] commands = new IDbJobCommand[actions.Count];

            for (int i = 0; actions.Count > 0; i++)
            {
                var cmdModel = currentState.CreateDbJobCommand(conn.CreateCommand());

                actions.Dequeue()(cmdModel);

                commands[i] = cmdModel;
            }

            return commands;
        }

        private static IDbJobCommand[] BuildJobCommands(DbConnection conn, IDbJobState state)
        {
            var currentState = (state as DbConnectorQueuedState);
            var actions = currentState.OnInit;

            IDbJobCommand[] commands = new IDbJobCommand[actions.Count];

            for (int i = 0; actions.Count > 0; i++)
            {
                var cmdModel = currentState.CreateDbJobCommand(conn.CreateCommand());

                actions.Dequeue()(cmdModel);

                commands[i] = cmdModel;
            }

            return commands;
        }

        private static IDbJobCommand[] BuildJobMultiReaderCommands(DbConnection conn, IDbJobState state)
        {
            var currentState = (state as DbConnectorDynamicState);

            Action<IDbJobCommand>[] actions = GetMultiReaderActions(currentState.Count, currentState.OnInit);

            IDbJobCommand[] commands = new IDbJobCommand[actions.Length];

            for (int i = 0; i < actions.Length; i++)
            {
                var cmdModel = currentState.CreateDbJobCommand(conn.CreateCommand());

                actions[i](cmdModel);

                commands[i] = cmdModel;
            }

            return commands;
        }

        private static Queue<Action<IDbJobCommand>> GetReaderActions(Action<Queue<Action<IDbJobCommand>>> onInit)
        {
            Queue<Action<IDbJobCommand>> commandActions = new Queue<Action<IDbJobCommand>>();

            onInit(commandActions);

            if (commandActions.Contains(null))
            {
                throw new NullReferenceException("DbJobCommand actions cannot be null!");
            }

            return commandActions;
        }

        private static Action<IDbJobCommand>[] GetMultiReaderActions(int count, dynamic onInit)
        {
            Action<IDbJobCommand>[] cmdActions = new Action<IDbJobCommand>[count];

            switch (count)
            {
                case 2:
                    {
                        (
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>
                        ) tuple = onInit.Invoke();

                        cmdActions[0] = tuple.Item1;
                        cmdActions[1] = tuple.Item2;
                    }
                    break;
                case 3:
                    {
                        (
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>
                        ) tuple = onInit.Invoke();

                        cmdActions[0] = tuple.Item1;
                        cmdActions[1] = tuple.Item2;
                        cmdActions[2] = tuple.Item3;
                    }
                    break;
                case 4:
                    {
                        (
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>
                        ) tuple = onInit.Invoke();

                        cmdActions[0] = tuple.Item1;
                        cmdActions[1] = tuple.Item2;
                        cmdActions[2] = tuple.Item3;
                        cmdActions[3] = tuple.Item4;
                    }
                    break;
                case 5:
                    {
                        (
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>
                        ) tuple = onInit.Invoke();

                        cmdActions[0] = tuple.Item1;
                        cmdActions[1] = tuple.Item2;
                        cmdActions[2] = tuple.Item3;
                        cmdActions[3] = tuple.Item4;
                        cmdActions[4] = tuple.Item5;
                    }
                    break;
                case 6:
                    {
                        (
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>
                        ) tuple = onInit.Invoke();

                        cmdActions[0] = tuple.Item1;
                        cmdActions[1] = tuple.Item2;
                        cmdActions[2] = tuple.Item3;
                        cmdActions[3] = tuple.Item4;
                        cmdActions[4] = tuple.Item5;
                        cmdActions[5] = tuple.Item6;
                    }
                    break;
                case 7:
                    {
                        (
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>
                        ) tuple = onInit.Invoke();

                        cmdActions[0] = tuple.Item1;
                        cmdActions[1] = tuple.Item2;
                        cmdActions[2] = tuple.Item3;
                        cmdActions[3] = tuple.Item4;
                        cmdActions[4] = tuple.Item5;
                        cmdActions[5] = tuple.Item6;
                        cmdActions[6] = tuple.Item7;
                    }
                    break;
                case 8:
                    {
                        (
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>,
                            Action<IDbJobCommand>
                        ) tuple = onInit.Invoke();

                        cmdActions[0] = tuple.Item1;
                        cmdActions[1] = tuple.Item2;
                        cmdActions[2] = tuple.Item3;
                        cmdActions[3] = tuple.Item4;
                        cmdActions[4] = tuple.Item5;
                        cmdActions[5] = tuple.Item6;
                        cmdActions[6] = tuple.Item7;
                        cmdActions[7] = tuple.Item8;
                    }
                    break;
                default:
                    break;
            }


            if (
                 cmdActions.Contains(null)
              )
            {
                throw new NullReferenceException("DbJobCommand actions cannot be null!");
            }

            return cmdActions;
        }



        private (IDbExecutedModel, object)[] OnBranchMultiReader<T>(MultiReaderTypes readerType, ref DbBranchResult<T> branchResult, IDbExecutionModel p, IDbJob<T> job, Action<IDbJobCommand>[] jobCmds)
        {
            //Holy moly!!!
            IEnumerable<Task> tasks = null;
            ConcurrentBag<IDbExecutedModel> executedModels = new ConcurrentBag<IDbExecutedModel>();

            try
            {
                //Variables
                dynamic j = (job as dynamic);
                bool isWithStateParam = j.IsWithStateParam();
                Type dataType = branchResult.Data.Source.GetType();
                CancellationToken token = p.Token;
                MultiReaderBranchCacheModel cacheModel = new MultiReaderBranchCacheModel(dataType, readerType, isWithStateParam);
                DbConnection connectionNullCache = null;

                if (!DbConnectorCache.MultiReaderBranchCache.TryGetValue(cacheModel, out DynamicDbConnectorMethodBuilder[] methodBuilders))
                {
                    var tempTasks = new Queue<Task>(8);
                    var tempMethodBuilders = new Queue<DynamicDbConnectorMethodBuilder>();
                    Type dcType = GetType();
                    bool isEnumerable = (readerType == MultiReaderTypes.Read || readerType == MultiReaderTypes.ReadToList);
                    string methodName = Enum.GetName(typeof(MultiReaderTypes), readerType);

                    //Get the method
                    MethodInfo mi;
                    if (!isWithStateParam)
                    {
                        mi = _multiReaderMethods.First(m => m.Name == methodName);
                    }
                    else
                    {
                        methodName += "ByState";
                        mi = _multiReaderMethodsByState.First(m => m.Name == methodName);
                    }


                    for (int i = 0; i < 8; i++)
                    {
                        FieldInfo fi = i < 7 ? dataType.GetField("Item" + (i + 1)) : dataType.GetField("Rest")?.FieldType.GetField("Item1");

                        if (fi == null)
                        {
                            break;
                        }

                        //MakeGenericMethod
                        MethodInfo genericReadMethod;
                        object[] args;
                        if (!isWithStateParam)
                        {
                            genericReadMethod = mi.MakeGenericMethod(!isEnumerable ? fi.FieldType : fi.FieldType.GetGenericArguments()[0]);
                            args = new object[] { jobCmds[i] };

                            //Cache the builder
                            tempMethodBuilders.Enqueue(DynamicDbConnectorMethodBuilder.CreateBuilder(dcType, genericReadMethod));
                        }
                        else
                        {
                            dynamic stateParam = j._state.StateParam;
                            Type stateParamType = stateParam.GetType();

                            genericReadMethod = mi.MakeGenericMethod(!isEnumerable ? fi.FieldType : fi.FieldType.GetGenericArguments()[0], stateParamType);
                            args = new object[] { jobCmds[i], stateParam };

                            //Cache the builder
                            tempMethodBuilders.Enqueue(DynamicDbConnectorMethodBuilder.CreateBuilder(dcType, genericReadMethod, stateParamType));
                        }

                        //Invoke
                        dynamic jobItem = (IDbJob)genericReadMethod.Invoke(this, args);

                        if (p.IsBuffered)
                        {
                            int index = i;

                            jobItem.OnExecuted((Func<object, IDbExecutedModel, object>)((data, e) =>
                            {
                                (e as DbExecutedModel).Index = index;

                                executedModels.Add(e);

                                return data;
                            }));
                        }

                        //Pass the old values
                        jobItem
                            .SetBranchedProperties(p.IsBuffered, j._isLoggingEnabled, j._isCacheEnabled, j._isolationLevel);

                        //Execute the job
                        if (p.IsDisposable)
                        {
                            tempTasks.Enqueue(jobItem.ExecuteDisposableAsync(connectionNullCache, token, true));
                        }
                        else
                        {
                            tempTasks.Enqueue(jobItem.ExecuteAsync(connectionNullCache, token, true));
                        }
                    }

                    DbConnectorCache.MultiReaderBranchCache.TryAdd(cacheModel, tempMethodBuilders.ToArray());
                    tasks = tempTasks.ToArray();
                }
                else
                {
                    var tempTasks = new Task[methodBuilders.Length];

                    for (int i = 0; i < methodBuilders.Length; i++)
                    {
                        dynamic jobItem = !isWithStateParam
                            ? methodBuilders[i].OnBuild(this, jobCmds[i])
                            : (methodBuilders[i] as dynamic).OnBuildByState(this, jobCmds[i], j._state.StateParam);

                        if (p.IsBuffered)
                        {
                            int index = i;

                            jobItem.OnExecuted((Func<object, IDbExecutedModel, object>)((data, e) =>
                            {
                                (e as DbExecutedModel).Index = index;

                                executedModels.Add(e);

                                return data;
                            }));
                        }

                        //Pass the old values
                        jobItem
                            .SetBranchedProperties(p.IsBuffered, j._isLoggingEnabled, j._isCacheEnabled, j._isolationLevel);

                        //Execute the job
                        if (p.IsDisposable)
                        {
                            tempTasks[i] = jobItem.ExecuteDisposableAsync(connectionNullCache, token, true);
                        }
                        else
                        {
                            tempTasks[i] = jobItem.ExecuteAsync(connectionNullCache, token, true);
                        }
                    }

                    tasks = tempTasks;
                }
            }
            catch (Exception)
            {
                if (p.IsDisposable && tasks != null)
                {
                    foreach (dynamic taskItem in tasks)
                        taskItem?.Result?.Dispose(false);
                }

                throw;
            }


            return OnBranchExecuting(ref branchResult, p, (Task[])tasks, ref executedModels);
        }

        private static (IDbExecutedModel, object)[] OnBranchExecuting<T>(ref DbBranchResult<T> branchResult, IDbExecutionModel p, Task[] tasks, ref ConcurrentBag<IDbExecutedModel> executedModels)
        {
            (IDbExecutedModel, object)[] result = new (IDbExecutedModel, object)[tasks.Length];

            try
            {
                Task.WaitAll(tasks);

                dynamic disposable = (branchResult.Data as dynamic);

                if (p.IsBuffered)
                {
                    //Order results since they might've shifted
                    var eModels = executedModels.OrderBy(o => o.Index).ToArray();

                    for (int i = 0; i < tasks.Length; i++)
                    {
                        dynamic taskItem = tasks[i];

                        if (!p.IsDisposable)
                        {
                            result[i] = (eModels[i], taskItem.Result);
                        }
                        else
                        {
                            result[i] = (eModels[i], taskItem.Result.Source);
                            disposable.Childs.Add(taskItem.Result);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < tasks.Length; i++)
                    {
                        dynamic taskItem = tasks[i];

                        if (!p.IsDisposable)
                        {
                            result[i] = (null, taskItem.Result);
                        }
                        else
                        {
                            result[i] = (null, taskItem.Result.Source);
                            disposable.Childs.Add(taskItem.Result);
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (p.IsDisposable)
                {
                    foreach (dynamic taskItem in tasks)
                        taskItem?.Result?.Dispose(false);
                }

                throw;
            }

            return result;
        }

        private static void OnBranchExecuted<T>(ref DbBranchResult<T> branchResult, IDbJob job, IEnumerable<(IDbExecutedModel, object)> data)
        {
            IDbDisposable<T> disposable = branchResult.Data;

            dynamic j = (job as dynamic);

            foreach (var result in data)
            {
                if (result.Item1 != null)
                {
                    branchResult.ExecutedModels.Add(result.Item1);

                    if (j._onExecuted != null)
                    {
                        disposable.Source = j._onExecuted(disposable.Source, result.Item1);
                    }
                }
            }
        }

        #endregion        

        #region Implementation

        /// <summary>
        /// Gets the string used to open the connection.
        /// </summary>
        public string ConnectionString
        {
            get { return _jobSetting.ConnectionString; }
        }

        /// <summary>
        /// Gets the type of <see cref="DbConnection"/> being used.
        /// </summary>
        public Type ConnectionType
        {
            get { return _jobSetting.DbConnectionType; }
        }

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
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<IEnumerable<T>> Read<T>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<IEnumerable<T>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state), //https://github.com/dotnet/roslyn/issues/5835
                    onExecute: (d, p) => OnExecuteRead(d, p)
                ).SetOnError((d, e) => Enumerable.Empty<T>());
        }

        protected internal IDbJob<IEnumerable<T>> ReadByState<T, TStateParam>(Action<IDbJobCommand> onInit, TStateParam stateParam)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<IEnumerable<T>, TStateParam, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState<TStateParam> { Flags = _flags, OnInit = onInit, StateParam = stateParam },
                    onCommands: (conn, state) => BuildJobCommand(conn, state), //https://github.com/dotnet/roslyn/issues/5835
                    onExecute: (d, p) => OnExecuteRead(d, p)
                ).SetOnError((d, e) => Enumerable.Empty<T>());
        }

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
        public IDbJob<T> ReadFirst<T>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadFirst(d, p)
                );
        }

        protected internal IDbJob<T> ReadFirstByState<T, TStateParam>(Action<IDbJobCommand> onInit, TStateParam stateParam)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<T, TStateParam, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState<TStateParam> { Flags = _flags, OnInit = onInit, StateParam = stateParam },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadFirst(d, p)
                );
        }

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
        public IDbJob<T> ReadFirstOrDefault<T>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadFirstOrDefault(d, p)
                );
        }

        protected internal IDbJob<T> ReadFirstOrDefaultByState<T, TStateParam>(Action<IDbJobCommand> onInit, TStateParam stateParam)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<T, TStateParam, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState<TStateParam> { Flags = _flags, OnInit = onInit, StateParam = stateParam },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadFirstOrDefault(d, p)
                );
        }

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
        public IDbJob<T> ReadSingle<T>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadSingle(d, p)
                );
        }

        protected internal IDbJob<T> ReadSingleByState<T, TStateParam>(Action<IDbJobCommand> onInit, TStateParam stateParam)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<T, TStateParam, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState<TStateParam> { Flags = _flags, OnInit = onInit, StateParam = stateParam },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadSingle(d, p)
                );
        }

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
        public IDbJob<T> ReadSingleOrDefault<T>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadSingleOrDefault(d, p)
                );
        }

        protected internal IDbJob<T> ReadSingleOrDefaultByState<T, TStateParam>(Action<IDbJobCommand> onInit, TStateParam stateParam)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<T, TStateParam, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState<TStateParam> { Flags = _flags, OnInit = onInit, StateParam = stateParam },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadSingleOrDefault(d, p)
                );
        }

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
        public IDbJob<List<T>> ReadToList<T>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<List<T>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadToList(d, p)
                ).SetOnError((d, e) => new List<T>());
        }

        protected internal IDbJob<List<T>> ReadToListByState<T, TStateParam>(Action<IDbJobCommand> onInit, TStateParam stateParam)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<List<T>, TStateParam, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState<TStateParam> { Flags = _flags, OnInit = onInit, StateParam = stateParam },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadToList(d, p)
                ).SetOnError((d, e) => new List<T>());
        }

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
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<DataTable> ReadToDataTable(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<DataTable, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadToDataTable(d, p)
                );
        }

        protected internal IDbJob<DataTable> ReadToDataTableByState<TStateParam>(Action<IDbJobCommand> onInit, TStateParam stateParam)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<DataTable, TStateParam, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState<TStateParam> { Flags = _flags, OnInit = onInit, StateParam = stateParam },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadToDataTable(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{System.Data.DataSet}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <param name="onInit">Action that is used to configure and enqueue all the <see cref="IDbJobCommand"/>.</param>  
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{System.Data.DataSet}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<DataSet> ReadToDataSet(Action<Queue<Action<IDbJobCommand>>> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            //Load Actions
            Queue<Action<IDbJobCommand>> jobCommandActions = GetReaderActions(onInit);

            return new DbJob<DataSet, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorQueuedState { Flags = _flags, OnInit = jobCommandActions },
                    onInit: () => new DataSet(),
                    onCommands: (conn, state) => BuildJobCommands(conn, state),
                    onExecute: (d, p) => OnExecuteReadToDataSet(d, p)
                )
                .WithIsolatedConnections(jobCommandActions.Count > 1 ? (_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false) : false)
                .OnBranch((d, p, job) =>
                {
                    //Create Jobs
                    var actions = (p.JobState as DbConnectorQueuedState).OnInit;
                    Task[] tasks = new Task[actions.Count];
                    ConcurrentBag<IDbExecutedModel> executedModels = new ConcurrentBag<IDbExecutedModel>();

                    try
                    {
                        dynamic j = (job as dynamic);
                        bool isWithStateParam = j.IsWithStateParam();
                        CancellationToken token = p.Token;
                        DbConnection connectionNullCache = null;

                        for (int i = 0; actions.Count > 0; i++)
                        {
                            var jobItem = !isWithStateParam ? ReadToDataTable(actions.Dequeue()) : ReadToDataTableByState(actions.Dequeue(), j._state.StateParam);

                            if (p.IsBuffered)
                            {
                                int index = i;

                                jobItem.OnExecuted((Func<DataTable, IDbExecutedModel, DataTable>)((data, e) =>
                                {
                                    (e as DbExecutedModel).Index = index;

                                    executedModels.Add(e);

                                    return data;
                                }));
                            }

                            jobItem
                                .SetBranchedProperties(p.IsBuffered, j._isLoggingEnabled, j._isCacheEnabled, j._isolationLevel);

                            if (p.IsDisposable)
                            {
                                tasks[i] = jobItem.ExecuteDisposableAsync(connectionNullCache, token, true);
                            }
                            else
                            {
                                tasks[i] = jobItem.ExecuteAsync(connectionNullCache, token, true);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        if (p.IsDisposable)
                        {
                            foreach (dynamic taskItem in tasks)
                                taskItem.Result?.Dispose(false);
                        }

                        throw;
                    }

                    var executionResults = OnBranchExecuting(ref d, p, tasks, ref executedModels);


                    foreach (var result in executionResults)
                    {
                        d.Data.Source.Tables.Add((DataTable)result.Item2);
                    }


                    OnBranchExecuted(ref d, job as IDbJob, executionResults);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{HashSet{T}}"/> able to read the first column of each row from the query result based on the <paramref name="onInit"/> action. All other columns are ignored.</para>
        ///  <para>Valid <typeparamref name="T"/> types: any .NET built-in type or ADO.NET data provider supported type.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default. <see cref="DBNull"/> values will be excluded.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{HashSet{T}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidCastException">Thrown when <typeparamref name="T"/> is not supported.</exception>
        public IDbJob<HashSet<T>> ReadToHashSet<T>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<HashSet<T>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadToHashSet(d, p)
                );
        }

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
        public IDbJob<IEnumerable<List<KeyValuePair<string, object>>>> ReadToKeyValuePairs(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<IEnumerable<List<KeyValuePair<string, object>>>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadToKeyValuePairs(d, p)
                ).SetOnError((d, e) => Enumerable.Empty<List<KeyValuePair<string, object>>>());
        }

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
        public IDbJob<IEnumerable<Dictionary<string, object>>> ReadToDictionaries(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<IEnumerable<Dictionary<string, object>>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadToDictionaries(d, p)
                ).SetOnError((d, e) => Enumerable.Empty<Dictionary<string, object>>());
        }

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
        public IDbJob<List<List<KeyValuePair<string, object>>>> ReadToListOfKeyValuePairs(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<List<List<KeyValuePair<string, object>>>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadToListOfKeyValuePairs(d, p)
                ).SetOnError((d, e) => new List<List<KeyValuePair<string, object>>>());
        }

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
        public IDbJob<List<Dictionary<string, object>>> ReadToListOfDictionaries(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<List<Dictionary<string, object>>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadToListOfDictionaries(d, p)
                ).SetOnError((d, e) => new List<Dictionary<string, object>>());
        }

        protected internal IDbJob<List<Dictionary<string, object>>> ReadToListOfDictionariesByState<TStateParam>(Action<IDbJobCommand> onInit, TStateParam stateParam)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<List<Dictionary<string, object>>, TStateParam, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState<TStateParam> { Flags = _flags, OnInit = onInit, StateParam = stateParam },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadToListOfDictionaries(d, p)
                ).SetOnError((d, e) => new List<Dictionary<string, object>>());
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IDbCollectionSet}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>This is usefull when wanting to create a concrete object from multiple/different queries.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <param name="onInit">Action that is used to configure and enqueue all the <see cref="IDbJobCommand"/>.</param>    
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IDbCollectionSet}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<IDbCollectionSet> ReadToDbCollectionSet(Action<Queue<Action<IDbJobCommand>>> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            //Load Actions
            Queue<Action<IDbJobCommand>> jobCommandActions = GetReaderActions(onInit);

            return new DbJob<IDbCollectionSet, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorQueuedState { Flags = _flags, OnInit = jobCommandActions },
                    onInit: () => new DbCollectionSet(),
                    onCommands: (conn, state) => BuildJobCommands(conn, state),
                    onExecute: (d, p) => OnExecuteReadToDbCollectionSet(d, p)
                )
                .WithIsolatedConnections(jobCommandActions.Count > 1 ? (_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false) : false)
                .OnBranch((d, p, job) =>
                {
                    //Create Jobs
                    var actions = (p.JobState as DbConnectorQueuedState).OnInit;
                    Task[] tasks = new Task[actions.Count];
                    ConcurrentBag<IDbExecutedModel> executedModels = new ConcurrentBag<IDbExecutedModel>();

                    try
                    {
                        dynamic j = (job as dynamic);
                        bool isWithStateParam = j.IsWithStateParam();
                        CancellationToken token = p.Token;
                        DbConnection connectionNullCache = null;

                        for (int i = 0; actions.Count > 0; i++)
                        {
                            var jobItem = !isWithStateParam ? ReadToListOfDictionaries(actions.Dequeue()) : ReadToListOfDictionariesByState(actions.Dequeue(), j._state.StateParam);

                            if (p.IsBuffered)
                            {
                                int index = i;

                                jobItem.OnExecuted((Func<List<Dictionary<string, object>>, IDbExecutedModel, List<Dictionary<string, object>>>)((data, e) =>
                                {
                                    (e as DbExecutedModel).Index = index;

                                    executedModels.Add(e);

                                    return data;
                                }));
                            }

                            jobItem
                                .SetBranchedProperties(p.IsBuffered, j._isLoggingEnabled, j._isCacheEnabled, j._isolationLevel);

                            if (p.IsDisposable)
                            {
                                tasks[i] = jobItem.ExecuteDisposableAsync(connectionNullCache, token, true);
                            }
                            else
                            {
                                tasks[i] = jobItem.ExecuteAsync(connectionNullCache, token, true);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        if (p.IsDisposable)
                        {
                            foreach (dynamic taskItem in tasks)
                                taskItem.Result?.Dispose(false);
                        }

                        throw;
                    }

                    var executionResults = OnBranchExecuting(ref d, p, tasks, ref executedModels);


                    foreach (var result in executionResults)
                    {
                        d.Data.Source.Items.Add((List<Dictionary<string, object>>)result.Item2);
                    }


                    OnBranchExecuted(ref d, job as IDbJob, executionResults);

                    return d;
                });
        }

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
        public IDbJob<T> ReadTo<T>(Action<Queue<Action<IDbJobCommand>>> onInit,
            Func<T, IDbExecutionModel, DbDataReader, T> onLoad)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            if (onLoad == null)
            {
                throw new ArgumentNullException(nameof(onLoad), "The onLoad delegate cannot be null!");
            }

            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorActionQueuedState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobActionCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(p.JobCommand.CommandBehavior ?? CommandBehavior.Default);
                        p.DeferDisposable(odr);

                        return onLoad(d, p, odr);
                    }
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> to get the first column of the first row from the result
        ///  set returned by the query. All other columns and rows are ignored.</para>
        ///  <para>Valid <typeparamref name="T"/> types: any .NET built-in type or ADO.NET data provider supported type.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteScalar"/>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidCastException">Thrown when <typeparamref name="T"/> is not supported.</exception>
        public IDbJob<T> Scalar<T>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteScalar(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{int?}"/> able to execute a non-query based on the <paramref name="onInit"/> action.</para>
        ///  <para>The result will be null if the non-query fails. Otherwise, the result will be the number of rows affected if the non-query ran successfully.</para>
        ///  <para>Note: A <see cref="DbTransaction"/> with <see cref="IsolationLevel.ReadCommitted"/> will be used by default.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteNonQuery"/>
        /// </summary>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>        
        /// <returns>The <see cref="IDbJob{int?}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<int?> NonQuery(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<int?, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteNonQuery(d, p)
                )
                .SetOnError((d, ex) => null)
                .SetWithIsolationLevel(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to execute a non-query based on the <paramref name="onInit"/> action.</para>
        ///  <para>Note: A <see cref="DbTransaction"/> with <see cref="IsolationLevel.ReadCommitted"/> will be used by default.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteNonQuery"/>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<T> NonQuery<T>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteNonQuery(d, p)
                )
                .SetWithIsolationLevel(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{int?}"/> able to execute all non-queries based on the <paramref name="onInit"/> action.</para>
        ///  <para>The result will be null if a non-query fails. Otherwise, the result will be the number of rows affected if all non-queries ran successfully.</para>
        ///  <para>Note: A <see cref="DbTransaction"/> with <see cref="IsolationLevel.ReadCommitted"/> will be used by default.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteNonQuery"/>
        /// </summary>
        /// <param name="onInit">Action that is used to configure and enqueue all the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{int?}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<int?> NonQueries(Action<Queue<Action<IDbJobCommand>>> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<int?, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorActionQueuedState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobActionCommands(conn, state),
                    onExecute: (d, p) => OnExecuteNonQueries(d, p)
                ).SetOnError((d, ex) => null)
                .SetWithIsolationLevel(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{T}"/> able to execute all non-queries based on the <paramref name="onInit"/> action.</para>
        ///  <para>Note: A <see cref="DbTransaction"/> with <see cref="IsolationLevel.ReadCommitted"/> will be used by default.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteNonQuery"/>
        /// </summary>
        /// <param name="onInit">Action that is used to configure and enqueue all the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<T> NonQueries<T>(Action<Queue<Action<IDbJobCommand>>> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorActionQueuedState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobActionCommands(conn, state),
                    onExecute: (d, p) => OnExecuteNonQueries(d, p)
                )
                .SetWithIsolationLevel(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        ///  Creates a <see cref="IDbJob{T}"/> which can be controlled 
        ///  by the <see cref="IDbExecutionModel"/> properties of the <see cref="IDbJob{T}.OnExecuted(Func{T, IDbExecutionModel, T})"/> function.
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="onInit">Action that is used to configure and enqueue all the <see cref="IDbJobCommand"/>.<para>Note: This can only be null if <paramref name="isCreateDbCommand"/> is set to false.</para></param>
        /// <param name="onExecute">Function that will be invoked for each <see cref="IDbJobCommand"/> and can be used to execute database calls and set the <typeparamref name="T"/> result.</param>
        /// <param name="isCreateDbCommand">Set this to false to disable the auto creation of a <see cref="DbCommand"/>. (Optional)</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when onExecute is null.</exception>
        public IDbJob<T> Build<T>(Action<Queue<Action<IDbJobCommand>>> onInit, Func<T, IDbExecutionModel, T> onExecute, bool isCreateDbCommand = true)
        {
            if (onInit == null && isCreateDbCommand)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            if (onExecute == null)
            {
                throw new ArgumentNullException(nameof(onExecute), "The onExecute delegate cannot be null!");
            }

            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorActionQueuedState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobActionCommands(conn, state),
                    onExecute: (d, p) => onExecute(d, p),
                    isCreateDbCommand: isCreateDbCommand
                );
        }

        /// <summary>
        /// Check if the database is available based on the provided connection string.
        /// </summary>
        /// <returns>The <see cref="IDbJob{bool}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<bool> IsConnected()
        {
            return new DbJob<bool, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = cmd => { } },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) =>
                    {
                        return true;
                    }
                );
        }

        #endregion

        #region Executions

        private static IEnumerable<T> OnExecuteRead<T>(IEnumerable<T> d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return p.IsBuffered ? odr.ToList<T>(p.Token, p.JobCommand)
                                : odr.ToEnumerable<T>(p.Token, p.JobCommand);
        }

        private static T OnExecuteReadFirst<T>(T d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
            p.DeferDisposable(odr);

            return odr.First<T>(p.Token, p.JobCommand);
        }

        private static T OnExecuteReadFirstOrDefault<T>(T d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
            p.DeferDisposable(odr);

            return odr.FirstOrDefault<T>(p.Token, p.JobCommand);
        }

        private static T OnExecuteReadSingle<T>(T d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return odr.Single<T>(p.Token, p.JobCommand);
        }

        private static T OnExecuteReadSingleOrDefault<T>(T d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return odr.SingleOrDefault<T>(p.Token, p.JobCommand);
        }

        private static List<T> OnExecuteReadToList<T>(List<T> d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return odr.ToList<T>(p.Token, p.JobCommand);
        }

        private static DataTable OnExecuteReadToDataTable(DataTable d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return odr.ToDataTable(false, p.Token, p.JobCommand.MapSettings);
        }

        private static HashSet<T> OnExecuteReadToHashSet<T>(HashSet<T> d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return odr.ToHashSet<T>(p.Token);
        }

        private static IEnumerable<List<KeyValuePair<string, object>>> OnExecuteReadToKeyValuePairs(IEnumerable<List<KeyValuePair<string, object>>> d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return p.IsBuffered ? odr.ToKeyValuePairs(false, p.Token, p.JobCommand) : odr.ToEnumerableKeyValuePairs(false, p.Token, p.JobCommand);
        }

        private static IEnumerable<Dictionary<string, object>> OnExecuteReadToDictionaries(IEnumerable<Dictionary<string, object>> d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return p.IsBuffered ? odr.ToDictionaries(false, p.Token, p.JobCommand) : odr.ToEnumerableDictionaries(false, p.Token, p.JobCommand);
        }

        private static List<List<KeyValuePair<string, object>>> OnExecuteReadToListOfKeyValuePairs(List<List<KeyValuePair<string, object>>> d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return odr.ToKeyValuePairs(false, p.Token, p.JobCommand);
        }

        private static List<Dictionary<string, object>> OnExecuteReadToListOfDictionaries(List<Dictionary<string, object>> d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return odr.ToDictionaries(false, p.Token, p.JobCommand);
        }

        private static DataSet OnExecuteReadToDataSet(DataSet d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.Default));
            p.DeferDisposable(odr);

            return odr.ToDataSet(false, p.Token, p.JobCommand.MapSettings, d);
        }

        private static IDbCollectionSet OnExecuteReadToDbCollectionSet(IDbCollectionSet d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.Default));
            p.DeferDisposable(odr);

            bool hasNext = true;

            while ((hasNext && odr.HasRows) || odr.NextResult())
            {
                if (p.Token.IsCancellationRequested)
                    break;

                d.Items.Add(odr.ToDictionaries(false, p.Token, p.JobCommand));

                hasNext = odr.NextResult();
            }

            return d;
        }

        private static int? OnExecuteNonQuery(int? d, IDbExecutionModel p)
        {
            int numberOfRowsAffected = p.Command.ExecuteNonQuery();

            p.NumberOfRowsAffected = numberOfRowsAffected;

            return numberOfRowsAffected;
        }

        private static T OnExecuteNonQuery<T>(T d, IDbExecutionModel p)
        {
            int numberOfRowsAffected = p.Command.ExecuteNonQuery();

            p.NumberOfRowsAffected = numberOfRowsAffected;

            return d;
        }

        private static int? OnExecuteNonQueries(int? d, IDbExecutionModel p)
        {
            int numberOfRowsAffected = p.Command.ExecuteNonQuery();

            if (!p.NumberOfRowsAffected.HasValue)
            {
                p.NumberOfRowsAffected = 0;
            }

            p.NumberOfRowsAffected = p.NumberOfRowsAffected.Value + numberOfRowsAffected;

            return p.NumberOfRowsAffected;
        }

        private static T OnExecuteNonQueries<T>(T d, IDbExecutionModel p)
        {
            int numberOfRowsAffected = p.Command.ExecuteNonQuery();

            if (!p.NumberOfRowsAffected.HasValue)
            {
                p.NumberOfRowsAffected = 0;
            }

            p.NumberOfRowsAffected = p.NumberOfRowsAffected.Value + numberOfRowsAffected;

            return d;
        }

        private static T OnExecuteScalar<T>(T d, IDbExecutionModel p)
        {
            object scalar = p.Command.ExecuteScalar();

            if (scalar != null && scalar != DBNull.Value)
            {
                Type tType = typeof(T);

                return (T)(DbConnectorUtilities.ThrowIfFailedToMatchColumnType(tType, (Nullable.GetUnderlyingType(tType) ?? tType), scalar));
            }
            else
            {
                return default;
            }
        }

        #endregion
    }
}
