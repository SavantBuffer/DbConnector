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
    /// Used to execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
    /// </summary>
    public static partial class DbJob
    {
        private static IDbResult<List<(List<IDbExecutedModel>, dynamic)>> TryExecuteAllHandled(
            IEnumerable<IDbJob> items,
            DbConnection connection = null,
            DbTransaction transaction = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            bool isLoggingEnabled = true,
            IDbConnectorLogger logger = null,
            CancellationToken token = default)
        {
            IDbResult<List<(List<IDbExecutedModel>, dynamic)>> result = new DbResult<List<(List<IDbExecutedModel>, dynamic)>>();
            List<(List<IDbExecutedModel>, dynamic)> data = null;
            try
            {
                ExecuteAllImplementation(ref data, ref logger, items, isolationLevel, token, connection, transaction);

                result.Data = data;
            }
            catch (Exception ex)
            {
                ex.Log(logger, isLoggingEnabled);

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
                        exInner.Log(logger, isLoggingEnabled);

                        ex = exInner;
                    }
                }

                result.Error = ex;
            }

            return result;
        }



        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
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
        /// <returns>The <see cref="IDbResult{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}"/> with all the executed results.</returns>
        public static IDbResult<List<(List<IDbExecutedModel>, dynamic)>> ExecuteAllHandled(
            IEnumerable<IDbJob> items,
            DbConnection connection = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            bool isLoggingEnabled = true,
            IDbConnectorLogger logger = null,
            CancellationToken token = default)
        {
            return TryExecuteAllHandled(items, connection, null, isolationLevel, isLoggingEnabled, logger, token);
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
        /// </summary>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for all <see cref="IDbJob{T}"/> executions.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="isLoggingEnabled">Use to disable logging. (Optional)</param>
        /// <param name="logger">The <see cref="IDbConnectorLogger"/> to use. The logger from the first provided <see cref="IDbJob"/> item will be used by default when null. (Optional)</param>      
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbResult{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}"/> with all the executed results.</returns>
        public static IDbResult<List<(List<IDbExecutedModel>, dynamic)>> ExecuteAllHandled(
            IEnumerable<IDbJob> items,
            DbTransaction transaction,
            bool isLoggingEnabled = true,
            IDbConnectorLogger logger = null,
            CancellationToken token = default)
        {
            if (transaction == null)
            {
                return TryExecuteAllHandled(items, isLoggingEnabled: isLoggingEnabled, logger: logger, token: token);
            }
            else
            {
                return TryExecuteAllHandled(items, transaction.Connection, transaction, transaction.IsolationLevel, isLoggingEnabled, logger, token);
            }
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
        /// <para>Note: The <see cref="DbConnection"/> will be created based on the first provided <see cref="IDbJob"/> item.</para>
        /// </summary>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="IDbResult{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}"/> with all the executed results.</returns>
        public static IDbResult<List<(List<IDbExecutedModel>, dynamic)>> ExecuteAllHandled(
            params IDbJob[] items)
        {
            return TryExecuteAllHandled(items);
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
        /// <para>Note: The <see cref="DbConnection"/> will be created based on the first provided <see cref="IDbJob"/> item.</para>
        /// </summary>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to use.</param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="IDbResult{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}"/> with all the executed results.</returns>
        public static IDbResult<List<(List<IDbExecutedModel>, dynamic)>> ExecuteAllHandled(
            IsolationLevel isolationLevel,
            params IDbJob[] items)
        {
            return TryExecuteAllHandled(items, isolationLevel: isolationLevel);
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for all <see cref="IDbJob{T}"/> executions. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="IDbResult{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}"/> with all the executed results.</returns>
        public static IDbResult<List<(List<IDbExecutedModel>, dynamic)>> ExecuteAllHandled(
            DbConnection connection = null,
            params IDbJob[] items)
        {
            return TryExecuteAllHandled(items, connection: connection);
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for all <see cref="IDbJob{T}"/> executions. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to use.</param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="IDbResult{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}"/> with all the executed results.</returns>
        public static IDbResult<List<(List<IDbExecutedModel>, dynamic)>> ExecuteAllHandled(
            DbConnection connection = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            params IDbJob[] items)
        {
            return TryExecuteAllHandled(items, connection: connection, isolationLevel: isolationLevel);
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
        /// </summary>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for all <see cref="IDbJob{T}"/> executions.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="IDbResult{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}"/> with all the executed results.</returns>
        public static IDbResult<List<(List<IDbExecutedModel>, dynamic)>> ExecuteAllHandled(
            DbTransaction transaction,
            params IDbJob[] items)
        {
            if (transaction == null)
            {
                return TryExecuteAllHandled(items);
            }
            else
            {
                return TryExecuteAllHandled(items, connection: transaction.Connection, transaction: transaction, isolationLevel: transaction.IsolationLevel);
            }
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for all <see cref="IDbJob{T}"/> executions. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to use. (Optional)</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="IDbResult{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}"/> with all the executed results.</returns>
        public static IDbResult<List<(List<IDbExecutedModel>, dynamic)>> ExecuteAllHandled(
            DbConnection connection = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken token = default,
            params IDbJob[] items)
        {
            return TryExecuteAllHandled(items, connection: connection, isolationLevel: isolationLevel, token: token);
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.
        /// </summary>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for all <see cref="IDbJob{T}"/> executions.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="IDbResult{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}"/> with all the executed results.</returns>
        public static IDbResult<List<(List<IDbExecutedModel>, dynamic)>> ExecuteAllHandled(
            DbTransaction transaction,
            CancellationToken token = default,
            params IDbJob[] items)
        {
            if (transaction == null)
            {
                return TryExecuteAllHandled(items, token: token);
            }
            else
            {
                return TryExecuteAllHandled(items, connection: transaction.Connection, transaction: transaction, isolationLevel: transaction.IsolationLevel, token: token);
            }
        }



        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/> asynchronously.
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
        /// <returns>The <see cref="Task{IDbResult{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}}"/> with all the executed results.</returns>
        public static Task<IDbResult<List<(List<IDbExecutedModel>, dynamic)>>> ExecuteAllHandledAsync(
            IEnumerable<IDbJob> items,
            DbConnection connection = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            bool isLoggingEnabled = true,
            IDbConnectorLogger logger = null,
            CancellationToken token = default)
        {
            return Task.Run(() => TryExecuteAllHandled(items, connection, null, isolationLevel, isLoggingEnabled, logger, token), token);
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/> asynchronously.
        /// </summary>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for all <see cref="IDbJob{T}"/> executions.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="isLoggingEnabled">Use to disable logging. (Optional)</param>
        /// <param name="logger">The <see cref="IDbConnectorLogger"/> to use. The logger from the first provided <see cref="IDbJob"/> item will be used by default when null. (Optional)</param>      
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="Task{IDbResult{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}}"/> with all the executed results.</returns>
        public static Task<IDbResult<List<(List<IDbExecutedModel>, dynamic)>>> ExecuteAllHandledAsync(
            IEnumerable<IDbJob> items,
            DbTransaction transaction,
            bool isLoggingEnabled = true,
            IDbConnectorLogger logger = null,
            CancellationToken token = default)
        {
            if (transaction == null)
            {
                return Task.Run(() => TryExecuteAllHandled(items, isLoggingEnabled: isLoggingEnabled, logger: logger, token: token), token);
            }
            else
            {
                return Task.Run(() => TryExecuteAllHandled(items, transaction.Connection, transaction, transaction.IsolationLevel, isLoggingEnabled, logger, token), token);
            }
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/> asynchronously.
        /// <para>Note: The <see cref="DbConnection"/> will be created based on the first provided <see cref="IDbJob"/> item.</para>
        /// </summary>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="Task{IDbResult{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}}"/> with all the executed results.</returns>
        public static Task<IDbResult<List<(List<IDbExecutedModel>, dynamic)>>> ExecuteAllHandledAsync(
            params IDbJob[] items)
        {
            return Task.Run(() => TryExecuteAllHandled(items));
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/> asynchronously.
        /// <para>Note: The <see cref="DbConnection"/> will be created based on the first provided <see cref="IDbJob"/> item.</para>
        /// </summary>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to use.</param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="Task{IDbResult{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}}"/> with all the executed results.</returns>
        public static Task<IDbResult<List<(List<IDbExecutedModel>, dynamic)>>> ExecuteAllHandledAsync(
            IsolationLevel isolationLevel,
            params IDbJob[] items)
        {
            return Task.Run(() => TryExecuteAllHandled(items, isolationLevel: isolationLevel));
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/> asynchronously.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for all <see cref="IDbJob{T}"/> executions. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="Task{IDbResult{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}}"/> with all the executed results.</returns>
        public static Task<IDbResult<List<(List<IDbExecutedModel>, dynamic)>>> ExecuteAllHandledAsync(
            DbConnection connection = null,
            params IDbJob[] items)
        {
            return Task.Run(() => TryExecuteAllHandled(items, connection: connection));
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/> asynchronously.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for all <see cref="IDbJob{T}"/> executions. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to use.</param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="Task{IDbResult{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}}"/> with all the executed results.</returns>
        public static Task<IDbResult<List<(List<IDbExecutedModel>, dynamic)>>> ExecuteAllHandledAsync(
            DbConnection connection = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            params IDbJob[] items)
        {
            return Task.Run(() => TryExecuteAllHandled(items, connection: connection, isolationLevel: isolationLevel));
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/> asynchronously.
        /// </summary>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for all <see cref="IDbJob{T}"/> executions.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="Task{IDbResult{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}}"/> with all the executed results.</returns>
        public static Task<IDbResult<List<(List<IDbExecutedModel>, dynamic)>>> ExecuteAllHandledAsync(
            DbTransaction transaction,
            params IDbJob[] items)
        {
            if (transaction == null)
            {
                return Task.Run(() => TryExecuteAllHandled(items));
            }
            else
            {
                return Task.Run(() => TryExecuteAllHandled(items, connection: transaction.Connection, transaction: transaction, isolationLevel: transaction.IsolationLevel));
            }
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/> asynchronously.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for all <see cref="IDbJob{T}"/> executions. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to use. (Optional)</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="Task{IDbResult{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}}"/> with all the executed results.</returns>
        public static Task<IDbResult<List<(List<IDbExecutedModel>, dynamic)>>> ExecuteAllHandledAsync(
            DbConnection connection = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken token = default,
            params IDbJob[] items)
        {
            return Task.Run(() => TryExecuteAllHandled(items, connection: connection, isolationLevel: isolationLevel, token: token), token);
        }

        /// <summary>
        /// Execute all the <see cref="IDbJob"/> items and handle any exceptions in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/> asynchronously.
        /// </summary>
        /// <param name="transaction">The <see cref="DbTransaction"/> to use for all <see cref="IDbJob{T}"/> executions.
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default, from the first provided <see cref="IDbJob"/> item, if this parameter is null.</para>
        /// <para>Warning: Multiple Active Result Sets (MARS) needs to be supported and enabled when executing multiple <see cref="DbCommand"/> simultaneously in the same <see cref="DbConnection"/>. Regardless, the use of isolated connections is encouraged.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="items">The <see cref="IDbJob"/> items to execute in the same <see cref="DbConnection"/> and <see cref="DbTransaction"/>.</param>
        /// <returns>The <see cref="Task{IDbResult{List{ValueTuple{List{IDbExecutedModel}, dynamic}}}}"/> with all the executed results.</returns>
        public static Task<IDbResult<List<(List<IDbExecutedModel>, dynamic)>>> ExecuteAllHandledAsync(
            DbTransaction transaction,
            CancellationToken token = default,
            params IDbJob[] items)
        {
            if (transaction == null)
            {
                return Task.Run(() => TryExecuteAllHandled(items, token: token), token);
            }
            else
            {
                return Task.Run(() => TryExecuteAllHandled(items, connection: transaction.Connection, transaction: transaction, isolationLevel: transaction.IsolationLevel, token: token), token);
            }
        }
    }
}
