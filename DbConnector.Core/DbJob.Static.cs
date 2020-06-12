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
using System.Threading;
using System.Threading.Tasks;

namespace DbConnector.Core
{
    /// <summary>
    /// Used to execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
    /// </summary>
    public static partial class DbJob
    {
        private static void ExecuteAllImplementation(
            ref List<(List<IDbExecutedModel>, dynamic)> result,
            ref IDbConnectorLogger logger,
            IEnumerable<IDbJob> items,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken token = default,
            DbConnection dbConnection = null,
            DbTransaction dbTransaction = null)
        {
            int itemCount;

            if (items != null && (itemCount = items.Count()) != 0)
            {
                result = new List<(List<IDbExecutedModel>, dynamic)>(itemCount);

                dynamic firstItem = items.First();

                if (firstItem == null)
                {
                    throw new NullReferenceException("IDbJob items cannot be null!");
                }

                lock (firstItem._executionLock)
                {
                    logger = logger ?? firstItem._settings.Logger;
                }


                //Configure DbConnection.
                DbConnection conn = dbConnection;
                bool wasConnectionClosed = false;
                try
                {
                    if (dbConnection == null)
                    {
                        //Create new connection
                        conn = firstItem.CreateConnectionInstance();
                        conn.ConnectionString = firstItem.ConnectionString;

                        //Open connection
                        wasConnectionClosed = true;
                        conn.Open();
                    }
                    else if (conn.State == ConnectionState.Closed)
                    {
                        //Open param connection
                        wasConnectionClosed = true;
                        conn.Open();
                    }

                    if (token.IsCancellationRequested)
                        return;

                    //Configure DbTransaction
                    DbTransaction transaction = dbTransaction;
                    try
                    {
                        if (dbTransaction == null)
                        {
                            //Begin new transaction.
                            transaction = conn.BeginTransaction(isolationLevel);
                        }

                        foreach (dynamic item in items)
                        {
                            if (item == null)
                            {
                                throw new NullReferenceException("IDbJob items cannot be null!");
                            }

                            lock (item._executionLock)
                            {
                                if (token.IsCancellationRequested)
                                    return;


                                bool isCacheEnabled = item._isCacheEnabled;
                                dynamic currentResult = item.DefaultValueOfT;


                                //Call onInit
                                if (item._onInit != null)
                                {
                                    currentResult = item._onInit();
                                }


                                IDbJobCommand[] dbJobCommands = null;
                                try
                                {
                                    //Set settings and commands.
                                    dbJobCommands = item._isCreateDbCommand ? item._onCommands(conn, item._state) : new IDbJobCommand[] { null };

                                    if (dbJobCommands == null)
                                    {
                                        dbJobCommands = new IDbJobCommand[] { item._isCreateDbCommand ? item.CreateDbJobCommand(conn.CreateCommand(), item._state) : null };
                                    }

                                    var executedModels = new List<IDbExecutedModel>(dbJobCommands.Length);
                                    DbExecutionModel eParam = item.CreateDbExecutionModel(true, false, conn, transaction, item._state, token);


                                    //Execute all commands
                                    for (int i = 0; i < dbJobCommands.Length; i++)
                                    {
                                        if (token.IsCancellationRequested)
                                        {
                                            if (item._isCreateDbCommand)
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
                                        eParam.JobState = item._state;

                                        try
                                        {
                                            if (item._isCreateDbCommand)
                                            {
                                                eParam.Command = cmd = jCommand.GetDbCommand();
                                                cmd.Connection = conn;
                                                cmd.Transaction = transaction;

                                                if (!isCacheEnabled)
                                                {
                                                    jCommand.Flags |= DbJobCommandFlags.NoCache;
                                                }
                                            }

                                            currentResult = item._onExecute(currentResult, eParam);

                                            if (item._onExecuted != null)
                                            {
                                                eParam.Parameters = cmd?.Parameters;

                                                currentResult = item._onExecuted(currentResult, eParam.CreateExecutedModel());
                                            }

                                            //Fail-safe for potential deferred IEnumerables;
                                            currentResult = ((currentResult != null && DbConnectorUtilities.IsEnumerable(currentResult.GetType())) ? Enumerable.ToList(currentResult) : currentResult);

                                            executedModels.Add(eParam.CreateExecutedModel());
                                        }
                                        finally
                                        {
                                            eParam.DeferrableDisposable?.Dispose();
                                            cmd?.Dispose();
                                        }
                                    }


                                    result.Add((executedModels, currentResult));
                                }
                                catch (Exception)
                                {
                                    if (item._isCreateDbCommand && dbJobCommands != null)
                                    {
                                        for (int x = 0; x < dbJobCommands.Length; x++)
                                        {
                                            dbJobCommands[x]?.GetDbCommand()?.Dispose();
                                        }
                                    }

                                    throw;
                                }
                            }
                        }

                        if (dbTransaction == null)
                        {
                            transaction.Commit();
                        }
                    }
                    catch (Exception)
                    {
                        if (transaction != null && dbTransaction == null)
                        {
                            transaction.Rollback();
                        }

                        throw;
                    }
                    finally
                    {
                        if (transaction != null && dbTransaction == null)
                        {
                            transaction.Dispose();
                        }
                    }
                }
                finally
                {
                    if (dbConnection == null)
                    {
                        conn.Dispose();
                    }
                    else if (wasConnectionClosed && dbConnection != null && conn != null)
                    {
                        conn.Close();
                    }
                }


                //Call onCompleted
                int index = 0;
                foreach (dynamic item in items)
                {
                    lock (item._executionLock)
                    {
                        if (item._onCompleted != null)
                        {
                            var itemResult = result.ElementAtOrDefault(index);

                            if (!itemResult.Equals(default))
                            {
                                itemResult.Item2 = item._onCompleted(itemResult.Item2);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    index++;
                }
            }
            else
            {
                result = new List<(List<IDbExecutedModel>, dynamic)>();
            }
        }



        private static List<(List<IDbExecutedModel>, dynamic)> TryExecuteAll(
            IEnumerable<IDbJob> items,
            DbConnection connection = null,
            DbTransaction transaction = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            bool isLoggingEnabled = true,
            IDbConnectorLogger logger = null,
            CancellationToken token = default,
            bool isThrowExceptions = true)
        {
            List<(List<IDbExecutedModel>, dynamic)> data = null;

            try
            {
                ExecuteAllImplementation(ref data, ref logger, items, isolationLevel, token, connection, transaction);
            }
            catch (Exception ex)
            {
                ex.Log(logger, isThrowExceptions ? false : isLoggingEnabled);


                if (items != null && data != null)
                {
                    try
                    {
                        int index = 0;
                        foreach (dynamic item in items)
                        {
                            if (item == null)
                            {
                                throw new NullReferenceException("IDbJob items cannot be null!");
                            }

                            lock (item._executionLock)
                            {
                                if (item._onError != null)
                                {
                                    var itemResult = data.ElementAtOrDefault(index);

                                    if (!itemResult.Equals(default))
                                    {
                                        itemResult.Item2 = item._onError(itemResult.Item2, ex);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            index++;
                        }
                    }
                    catch (Exception exInner)
                    {
                        exInner.Log(logger, isThrowExceptions ? false : isLoggingEnabled);

                        if (isThrowExceptions)
                        {
                            throw;
                        }
                    }
                }


                if (isThrowExceptions)
                {
                    throw;
                }
            }

            return data;
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
        /// </summary>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <param name="connection">The <see cref="DbConnection"/> to use for all <see cref="IDbJob{T}"/> executions. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to use. (Optional)</param>
        /// <param name="isLoggingEnabled">Use to disable logging. (Optional)</param>
        /// <param name="logger">The <see cref="IDbConnectorLogger"/> to use. The logger from the first provided <see cref="IDbJob"/> item will be used by default when null. (Optional)</param>      
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to disable the throwing of transactions. (Optional)</param>
        /// <returns>The <see cref="List{ValueTuple{List{IDbExecutedModel}, dynamic}}"/> with all the executed results.</returns>
        public static List<(List<IDbExecutedModel>, dynamic)> ExecuteAll(
            IEnumerable<IDbJob> items,
            DbConnection connection = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            bool isLoggingEnabled = true,
            IDbConnectorLogger logger = null,
            CancellationToken token = default,
            bool isThrowExceptions = true)
        {
            return TryExecuteAll(items, connection, null, isolationLevel, isLoggingEnabled, logger, token, isThrowExceptions);
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
        /// </summary>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for all <see cref="IDbJob{T}"/> executions.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="isLoggingEnabled">Use to disable logging. (Optional)</param>
        /// <param name="logger">The <see cref="IDbConnectorLogger"/> to use. The logger from the first provided <see cref="IDbJob"/> item will be used by default when null. (Optional)</param>      
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to disable the throwing of transactions. (Optional)</param>
        /// <returns>The <see cref="List{ValueTuple{List{IDbExecutedModel}, dynamic}}"/> with all the executed results.</returns>
        public static List<(List<IDbExecutedModel>, dynamic)> ExecuteAll(
            IEnumerable<IDbJob> items,
            DbTransaction transaction,
            bool isLoggingEnabled = true,
            IDbConnectorLogger logger = null,
            CancellationToken token = default,
            bool isThrowExceptions = true)
        {
            if (transaction == null)
            {
                return TryExecuteAll(items, isLoggingEnabled: isLoggingEnabled, logger: logger, token: token, isThrowExceptions: isThrowExceptions);
            }
            else
            {
                return TryExecuteAll(items, transaction.Connection, transaction, transaction.IsolationLevel, isLoggingEnabled, logger, token, isThrowExceptions);
            }
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
        /// <para>Note: The <see cref="DbConnection"/> will be created based on the first provided <see cref="IDbJob"/> item.</para>
        /// </summary>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="List{ValueTuple{List{IDbExecutedModel}, dynamic}}"/> with all the executed results.</returns>
        public static List<(List<IDbExecutedModel>, dynamic)> ExecuteAll(
            params IDbJob[] items)
        {
            return TryExecuteAll(items);
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
        /// <para>Note: The <see cref="DbConnection"/> will be created based on the first provided <see cref="IDbJob"/> item.</para>
        /// </summary>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to use.</param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="List{ValueTuple{List{IDbExecutedModel}, dynamic}}"/> with all the executed results.</returns>
        public static List<(List<IDbExecutedModel>, dynamic)> ExecuteAll(
            IsolationLevel isolationLevel,
            params IDbJob[] items)
        {
            return TryExecuteAll(items, isolationLevel: isolationLevel);
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for all <see cref="IDbJob{T}"/> executions. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="List{ValueTuple{List{IDbExecutedModel}, dynamic}}"/> with all the executed results.</returns>
        public static List<(List<IDbExecutedModel>, dynamic)> ExecuteAll(
            DbConnection connection = null,
            params IDbJob[] items)
        {
            return TryExecuteAll(items, connection: connection);
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for all <see cref="IDbJob{T}"/> executions. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to use.</param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="List{ValueTuple{List{IDbExecutedModel}, dynamic}}"/> with all the executed results.</returns>
        public static List<(List<IDbExecutedModel>, dynamic)> ExecuteAll(
            DbConnection connection = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            params IDbJob[] items)
        {
            return TryExecuteAll(items, connection: connection, isolationLevel: isolationLevel);
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
        /// </summary>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for all <see cref="IDbJob{T}"/> executions.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="List{ValueTuple{List{IDbExecutedModel}, dynamic}}"/> with all the executed results.</returns>
        public static List<(List<IDbExecutedModel>, dynamic)> ExecuteAll(
            DbTransaction transaction,
            params IDbJob[] items)
        {
            if (transaction == null)
            {
                return TryExecuteAll(items);
            }
            else
            {
                return TryExecuteAll(items, connection: transaction.Connection, transaction: transaction, isolationLevel: transaction.IsolationLevel);
            }
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for all <see cref="IDbJob{T}"/> executions. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to use. (Optional)</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to disable the throwing of transactions. (Optional)</param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="List{ValueTuple{List{IDbExecutedModel}, dynamic}}"/> with all the executed results.</returns>
        public static List<(List<IDbExecutedModel>, dynamic)> ExecuteAll(
            DbConnection connection = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken token = default,
            bool isThrowExceptions = true,
            params IDbJob[] items)
        {
            return TryExecuteAll(items, connection: connection, isolationLevel: isolationLevel, token: token, isThrowExceptions: isThrowExceptions);
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
        /// </summary>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for all <see cref="IDbJob{T}"/> executions.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to disable the throwing of transactions. (Optional)</param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="List{ValueTuple{List{IDbExecutedModel}, dynamic}}"/> with all the executed results.</returns>
        public static List<(List<IDbExecutedModel>, dynamic)> ExecuteAll(
            DbTransaction transaction,
            CancellationToken token = default,
            bool isThrowExceptions = true,
            params IDbJob[] items)
        {
            if (transaction == null)
            {
                return TryExecuteAll(items, token: token, isThrowExceptions: isThrowExceptions);
            }
            else
            {
                return TryExecuteAll(items, connection: transaction.Connection, transaction: transaction, isolationLevel: transaction.IsolationLevel, token: token, isThrowExceptions: isThrowExceptions);
            }
        }



        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/> asynchronously.
        /// </summary>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <param name="connection">The <see cref="DbConnection"/> to use for all <see cref="IDbJob{T}"/> executions. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to use. (Optional)</param>
        /// <param name="isLoggingEnabled">Use to disable logging. (Optional)</param>
        /// <param name="logger">The <see cref="IDbConnectorLogger"/> to use. The logger from the first provided <see cref="IDbJob"/> item will be used by default when null. (Optional)</param>      
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to disable the throwing of transactions. (Optional)</param>
        /// <returns>The <see cref="Task{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}"/> with all the executed results.</returns>
        public static Task<List<(List<IDbExecutedModel>, dynamic)>> ExecuteAllAsync(
            IEnumerable<IDbJob> items,
            DbConnection connection = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            bool isLoggingEnabled = true,
            IDbConnectorLogger logger = null,
            CancellationToken token = default,
            bool isThrowExceptions = true)
        {
            return Task.Run(() => TryExecuteAll(items, connection, null, isolationLevel, isLoggingEnabled, logger, token, isThrowExceptions), token);
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/> asynchronously.
        /// </summary>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for all <see cref="IDbJob{T}"/> executions.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="isLoggingEnabled">Use to disable logging. (Optional)</param>
        /// <param name="logger">The <see cref="IDbConnectorLogger"/> to use. The logger from the first provided <see cref="IDbJob"/> item will be used by default when null. (Optional)</param>      
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to disable the throwing of transactions. (Optional)</param>
        /// <returns>The <see cref="Task{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}"/> with all the executed results.</returns>
        public static Task<List<(List<IDbExecutedModel>, dynamic)>> ExecuteAllAsync(
            IEnumerable<IDbJob> items,
            DbTransaction transaction,
            bool isLoggingEnabled = true,
            IDbConnectorLogger logger = null,
            CancellationToken token = default,
            bool isThrowExceptions = true)
        {
            if (transaction == null)
            {
                return Task.Run(() => TryExecuteAll(items, isLoggingEnabled: isLoggingEnabled, logger: logger, token: token, isThrowExceptions: isThrowExceptions), token);
            }
            else
            {
                return Task.Run(() => TryExecuteAll(items, transaction.Connection, transaction, transaction.IsolationLevel, isLoggingEnabled, logger, token, isThrowExceptions), token);
            }
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/> asynchronously.
        /// <para>Note: The <see cref="DbConnection"/> will be created based on the first provided <see cref="IDbJob"/> item.</para>
        /// </summary>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="Task{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}"/> with all the executed results.</returns>
        public static Task<List<(List<IDbExecutedModel>, dynamic)>> ExecuteAllAsync(
            params IDbJob[] items)
        {
            return Task.Run(() => TryExecuteAll(items));
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/> asynchronously.
        /// <para>Note: The <see cref="DbConnection"/> will be created based on the first provided <see cref="IDbJob"/> item.</para>
        /// </summary>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to use.</param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="Task{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}"/> with all the executed results.</returns>
        public static Task<List<(List<IDbExecutedModel>, dynamic)>> ExecuteAllAsync(
            IsolationLevel isolationLevel,
            params IDbJob[] items)
        {
            return Task.Run(() => TryExecuteAll(items, isolationLevel: isolationLevel));
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/> asynchronously.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for all <see cref="IDbJob{T}"/> executions. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="Task{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}"/> with all the executed results.</returns>
        public static Task<List<(List<IDbExecutedModel>, dynamic)>> ExecuteAllAsync(
            DbConnection connection = null,
            params IDbJob[] items)
        {
            return Task.Run(() => TryExecuteAll(items, connection: connection));
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/> asynchronously.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for all <see cref="IDbJob{T}"/> executions. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to use.</param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="Task{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}"/> with all the executed results.</returns>
        public static Task<List<(List<IDbExecutedModel>, dynamic)>> ExecuteAllAsync(
            DbConnection connection = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            params IDbJob[] items)
        {
            return Task.Run(() => TryExecuteAll(items, connection: connection, isolationLevel: isolationLevel));
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/> asynchronously.
        /// </summary>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for all <see cref="IDbJob{T}"/> executions.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="Task{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}"/> with all the executed results.</returns>
        public static Task<List<(List<IDbExecutedModel>, dynamic)>> ExecuteAllAsync(
            DbTransaction transaction,
            params IDbJob[] items)
        {
            if (transaction == null)
            {
                return Task.Run(() => TryExecuteAll(items));
            }
            else
            {
                return Task.Run(() => TryExecuteAll(items, connection: transaction.Connection, transaction: transaction, isolationLevel: transaction.IsolationLevel));
            }
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/> asynchronously.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for all <see cref="IDbJob{T}"/> executions. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to use. (Optional)</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to disable the throwing of transactions. (Optional)</param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="Task{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}"/> with all the executed results.</returns>
        public static Task<List<(List<IDbExecutedModel>, dynamic)>> ExecuteAllAsync(
            DbConnection connection = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken token = default,
            bool isThrowExceptions = true,
            params IDbJob[] items)
        {
            return Task.Run(() => TryExecuteAll(items, connection: connection, isolationLevel: isolationLevel, token: token, isThrowExceptions: isThrowExceptions), token);
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/> asynchronously.
        /// </summary>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for all <see cref="IDbJob{T}"/> executions.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to disable the throwing of transactions. (Optional)</param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="Task{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}"/> with all the executed results.</returns>
        public static Task<List<(List<IDbExecutedModel>, dynamic)>> ExecuteAllAsync(
            DbTransaction transaction,
            CancellationToken token = default,
            bool isThrowExceptions = true,
            params IDbJob[] items)
        {
            if (transaction == null)
            {
                return Task.Run(() => TryExecuteAll(items, token: token, isThrowExceptions: isThrowExceptions), token);
            }
            else
            {
                return Task.Run(() => TryExecuteAll(items, connection: transaction.Connection, transaction: transaction, isolationLevel: transaction.IsolationLevel, token: token, isThrowExceptions: isThrowExceptions), token);
            }
        }
    }
}
