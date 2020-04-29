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
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DbConnector.Core
{
    public interface IDbJob
    {
        string ConnectionString { get; }

        /// <summary>
        /// Returns the type of DbConnection being used.
        /// </summary>
        Type ConnectionType { get; }
    }

    public interface IDbJob<T> : IDbJob
    {
        /// <summary>
        /// Execute the <see cref="IDbJob{T}"/>.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJob{T}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// </param>        
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The T result.</returns>
        T Execute(DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null);

        /// <summary>
        /// Execute the <see cref="IDbJob{T}"/> asynchronously.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJob{T}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The <see cref="Task{T}"/>.</returns>
        Task<T> ExecuteAsync(DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null);

        /// <summary>
        /// Execute the <see cref="IDbJob{T}"/> and handle any exceptions while opening the <see cref="DbConnection"/>.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJob{T}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbResult{T}"/>.</returns>
        IDbResult<T> ExecuteHandled(DbConnection connection = null, CancellationToken token = default);

        /// <summary>
        /// Execute the <see cref="IDbJob{T}"/> and handle any exceptions while opening the <see cref="DbConnection"/> asynchronously.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJob{T}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="Task{IDbResult{T}}"/>.</returns>
        Task<IDbResult<T>> ExecuteHandledAsync(DbConnection connection = null, CancellationToken token = default);

        /// <summary>
        /// Use this function when a serialized data extraction is required and to handle any exceptions while opening the <see cref="DbConnection"/>. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJob{T}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbResult{IDbDisposable{T}}"/>.</returns>
        IDbResult<IDbDisposable<T>> ExecuteDisposableHandled(DbConnection connection = null, CancellationToken token = default);

        /// <summary>
        /// Use this asynchronous function when a serialized data extraction is required and to handle any exceptions while opening the <see cref="DbConnection"/>. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJob{T}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="Task{IDbResult{IDbDisposable{T}}}"/>.</returns>
        Task<IDbResult<IDbDisposable<T>>> ExecuteDisposableHandledAsync(DbConnection connection = null, CancellationToken token = default);

        /// <summary>
        /// Use this function when a serialized data extraction is required. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJob{T}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The <see cref="IDbDisposable{T}"/>.</returns>
        IDbDisposable<T> ExecuteDisposable(DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null);

        /// <summary>
        /// Use this asynchronous function when a serialized data extraction is required. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJob{T}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The <see cref="Task{IDbDisposable{T}}"/>.</returns>
        Task<IDbDisposable<T>> ExecuteDisposableAsync(DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null);



        /// <summary>
        /// <para>Use this function to disable or enable buffered (non-deferred/non-yielded) execution when reading data (this is enabled by default).</para>
        /// <para>Note: Deferred execution is only possible for <see cref="System.Collections.IEnumerable"/> types during individual transactions. Consequently, normal execution will be used when encountering non <see cref="System.Collections.IEnumerable"/> types or Batch-Reading implementations.</para>
        /// Warning: Exceptions may occur while looping deferred <see cref="System.Collections.IEnumerable"/> types because of the implicit database connection dependency.
        /// </summary>
        /// <returns><see cref="IDbJob{T}"/></returns>
        IDbJob<T> WithBuffering(bool isEnabled);

        /// <summary>
        /// <para>Use this function to disable or enable the caching of query mappings and types for a all <see cref="DbJobCommand"/> owned by this <see cref="IDbJob{T}"/>.</para>
        /// </summary>
        /// <returns><see cref="IDbJob{T}"/></returns>
        IDbJob<T> WithCache(bool isEnabled);

        /// <summary>
        /// Use this function to disable or enable error logging (this is enabled by default). Disabling this can be useful when logging errors in order to prevent an infinite loop.
        /// </summary>
        /// <returns><see cref="IDbJob{T}"/></returns>
        IDbJob<T> WithLogging(bool isEnabled);

        /// <summary>
        /// Use this function to globally set the <see cref="DbConnection"/> for this <see cref="IDbJob{T}"/>. 
        /// Note: A new <see cref="DbConnection"/> will be created automatically by default if not provided.
        /// </summary>
        /// <returns><see cref="IDbJob{T}"/></returns>
        //IDbJob<T> WithConnection(DbConnection connection);

        /// <summary>
        /// Use this function to set the <see cref="IsolationLevel"/> for this <see cref="IDbJob{T}"/>.
        /// </summary>
        /// <returns><see cref="IDbJob{T}"/></returns>
        IDbJob<T> WithIsolationLevel(IsolationLevel? level);



        /// <summary>
        /// Use this to set the delegate to call after a DbCommand is executed. You can use this function to change the <see cref="{T}"/> result.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="setting">The event setting to use. (Optional)</param>
        /// <returns><see cref="IDbJob{T}"/></returns>
        IDbJob<T> OnExecuted(Func<T, IDbExecutedModel, T> action, EventSetting setting = EventSetting.Subscribe);

        /// <summary>
        /// Use this to set the delegate to call after all DbCommands are executed without errors. You can use this function to change the <see cref="{T}"/> result.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="setting">The event setting to use. (Optional)</param>
        /// <returns><see cref="IDbJob{T}"/></returns>
        IDbJob<T> OnCompleted(Func<T, T> action, EventSetting setting = EventSetting.Subscribe);

        /// <summary>
        /// Use this to set the delegate to call when an error occurs. You can use this function to change the <see cref="{T}"/> result.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="setting">The event setting to use. (Optional)</param>
        /// <returns><see cref="IDbJob{T}"/></returns>
        IDbJob<T> OnError(Func<T, Exception, T> action, EventSetting setting = EventSetting.Subscribe);

        /// <summary>
        /// Use this to create an <see cref="IDbJobCacheable{T, TStateParamValue}"/>.
        /// <para>Note: This should only be used when wanting to cache the current <see cref="IDbJob{T}"/> (e.g. when caching in a static field).</para>
        /// </summary>
        /// <typeparam name="TStateParamValue">The state parameter type to use.</typeparam>
        /// <param name="value">The state parameter value to use</param>
        /// <returns>The new <see cref="IDbJobCacheable{T, TStateParamValue}"/>.</returns>
        IDbJobCacheable<T, TStateParamValue> ToCacheable<TStateParamValue>(TStateParamValue value);

        /// <summary>
        /// Use this to create an <see cref="IDbJobCacheable{T, TStateParamValue}"/>.
        /// <para>Note: This should only be used when wanting to cache the current <see cref="IDbJob{T}"/> (e.g. when caching in a static field).</para>
        /// </summary>
        /// <typeparam name="TStateParamValue">The state parameter type to use.</typeparam>
        /// <returns>The new <see cref="IDbJobCacheable{T, TStateParamValue}"/>.</returns>
        IDbJobCacheable<T, TStateParamValue> ToCacheable<TStateParamValue>();
    }

    public interface IDbJobCacheable<T, TStateParam>
    {
        /// <summary>
        /// Execute the <see cref="IDbJobCacheable{T, TStateParam}"/>.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// </param>        
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The T result.</returns>
        T Execute(TStateParam parameter, DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null);

        /// <summary>
        /// Execute the <see cref="IDbJobCacheable{T, TStateParam}"/> asynchronously.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The <see cref="Task{T}"/>.</returns>
        Task<T> ExecuteAsync(TStateParam parameter, DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null);

        /// <summary>
        /// Execute the <see cref="IDbJobCacheable{T, TStateParam}"/> and handle any exceptions while opening the <see cref="DbConnection"/>.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbResult{T}"/>.</returns>
        IDbResult<T> ExecuteHandled(TStateParam parameter, DbConnection connection = null, CancellationToken token = default);

        /// <summary>
        /// Execute the <see cref="IDbJobCacheable{T, TStateParam}"/> and handle any exceptions while opening the <see cref="DbConnection"/> asynchronously.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="Task{IDbResult{T}}"/>.</returns>
        Task<IDbResult<T>> ExecuteHandledAsync(TStateParam parameter, DbConnection connection = null, CancellationToken token = default);

        /// <summary>
        /// Use this function when a serialized data extraction is required and to handle any exceptions while opening the <see cref="DbConnection"/>. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbResult{IDbDisposable{T}}"/>.</returns>
        IDbResult<IDbDisposable<T>> ExecuteDisposableHandled(TStateParam parameter, DbConnection connection = null, CancellationToken token = default);

        /// <summary>
        /// Use this asynchronous function when a serialized data extraction is required and to handle any exceptions while opening the <see cref="DbConnection"/>. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <returns>The <see cref="Task{IDbResult{IDbDisposable{T}}}"/>.</returns>
        Task<IDbResult<IDbDisposable<T>>> ExecuteDisposableHandledAsync(TStateParam parameter, DbConnection connection = null, CancellationToken token = default);

        /// <summary>
        /// Use this function when a serialized data extraction is required. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The <see cref="IDbDisposable{T}"/>.</returns>
        IDbDisposable<T> ExecuteDisposable(TStateParam parameter, DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null);

        /// <summary>
        /// Use this asynchronous function when a serialized data extraction is required. E.g. When using <see cref="CommandBehavior.SequentialAccess"/> to get a <see cref="System.IO.Stream"/>.
        /// This function returns an <see cref="IDisposable"/> object that must me disposed in order to commit the transaction and prevent leaks.
        /// </summary>
        /// <param name="parameter">The state parameter to use.</param>
        /// <param name="connection">The <see cref="DbConnection"/> to use for this <see cref="IDbJobCacheable{T, TStateParam}"/> execution. (Optional)
        /// <para>Note: A new <see cref="DbConnection"/> will be created automatically by default if this parameter is null.</para>
        /// </param>
        /// <param name="token">The <see cref="CancellationToken"/> to use. (Optional)</param>
        /// <param name="isThrowExceptions">Use to override the <see cref="DbConnectorFlags.NoExceptionThrowingForNonHandledExecution"/>. (Optional)</param>
        /// <returns>The <see cref="Task{IDbDisposable{T}}"/>.</returns>
        Task<IDbDisposable<T>> ExecuteDisposableAsync(TStateParam parameter, DbConnection connection = null, CancellationToken token = default, bool? isThrowExceptions = null);
    }
}
