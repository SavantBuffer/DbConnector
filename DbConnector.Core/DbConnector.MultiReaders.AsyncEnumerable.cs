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

namespace DbConnector.Core
{
    public partial class DbConnector<TDbConnection> : IDbConnector<TDbConnection>
       where TDbConnection : DbConnection
    {
        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute readers, with an un-buffered (deferred/yielded) approach, based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
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
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IAsyncEnumerable<T1>, IAsyncEnumerable<T2>)> ReadAsAsyncEnumerable<T1, T2>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(IAsyncEnumerable<T1>, IAsyncEnumerable<T2>), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 2 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
                        p.DeferDisposable(odr);

                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.AsAsyncEnumerable<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.AsAsyncEnumerable<T2>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }

                        return d;
                    }
                )
                .SetOnError((d, e) => (AsyncEnumerable.Empty<T1>(), AsyncEnumerable.Empty<T2>()))
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadAsAsyncEnumerable, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (IAsyncEnumerable<T1>)data[0].Item2,
                        (IAsyncEnumerable<T2>)data[1].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                }).WithoutBuffering();
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute readers, with an un-buffered (deferred/yielded) approach, based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
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
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IAsyncEnumerable<T1>, IAsyncEnumerable<T2>, IAsyncEnumerable<T3>)> ReadAsAsyncEnumerable<T1, T2, T3>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(IAsyncEnumerable<T1>, IAsyncEnumerable<T2>, IAsyncEnumerable<T3>), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 3 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
                        p.DeferDisposable(odr);

                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.AsAsyncEnumerable<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.AsAsyncEnumerable<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = odr.AsAsyncEnumerable<T3>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }

                        return d;
                    }
                )
                .SetOnError((d, e) => (AsyncEnumerable.Empty<T1>(), AsyncEnumerable.Empty<T2>(), AsyncEnumerable.Empty<T3>()))
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadAsAsyncEnumerable, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (IAsyncEnumerable<T1>)data[0].Item2,
                        (IAsyncEnumerable<T2>)data[1].Item2,
                        (IAsyncEnumerable<T3>)data[2].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                }).WithoutBuffering();
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute readers, with an un-buffered (deferred/yielded) approach, based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
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
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IAsyncEnumerable<T1>, IAsyncEnumerable<T2>, IAsyncEnumerable<T3>, IAsyncEnumerable<T4>)> ReadAsAsyncEnumerable<T1, T2, T3, T4>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(IAsyncEnumerable<T1>, IAsyncEnumerable<T2>, IAsyncEnumerable<T3>, IAsyncEnumerable<T4>), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 4 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
                        p.DeferDisposable(odr);

                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.AsAsyncEnumerable<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.AsAsyncEnumerable<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = odr.AsAsyncEnumerable<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = odr.AsAsyncEnumerable<T4>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }

                        return d;
                    }
                )
                .SetOnError((d, e) => (AsyncEnumerable.Empty<T1>(), AsyncEnumerable.Empty<T2>(), AsyncEnumerable.Empty<T3>(), AsyncEnumerable.Empty<T4>()))
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadAsAsyncEnumerable, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (IAsyncEnumerable<T1>)data[0].Item2,
                        (IAsyncEnumerable<T2>)data[1].Item2,
                        (IAsyncEnumerable<T3>)data[2].Item2,
                        (IAsyncEnumerable<T4>)data[3].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                }).WithoutBuffering();
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute readers, with an un-buffered (deferred/yielded) approach, based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
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
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IAsyncEnumerable<T1>, IAsyncEnumerable<T2>, IAsyncEnumerable<T3>, IAsyncEnumerable<T4>, IAsyncEnumerable<T5>)> ReadAsAsyncEnumerable<T1, T2, T3, T4, T5>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(IAsyncEnumerable<T1>, IAsyncEnumerable<T2>, IAsyncEnumerable<T3>, IAsyncEnumerable<T4>, IAsyncEnumerable<T5>), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 5 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
                        p.DeferDisposable(odr);

                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.AsAsyncEnumerable<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.AsAsyncEnumerable<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = odr.AsAsyncEnumerable<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = odr.AsAsyncEnumerable<T4>(p.Token, p.JobCommand);
                                break;
                            case 5:
                                d.Item5 = odr.AsAsyncEnumerable<T5>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }

                        return d;
                    }
                )
                .SetOnError((d, e) => (AsyncEnumerable.Empty<T1>(), AsyncEnumerable.Empty<T2>(), AsyncEnumerable.Empty<T3>(), AsyncEnumerable.Empty<T4>(), AsyncEnumerable.Empty<T5>()))
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadAsAsyncEnumerable, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (IAsyncEnumerable<T1>)data[0].Item2,
                        (IAsyncEnumerable<T2>)data[1].Item2,
                        (IAsyncEnumerable<T3>)data[2].Item2,
                        (IAsyncEnumerable<T4>)data[3].Item2,
                        (IAsyncEnumerable<T5>)data[4].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                }).WithoutBuffering();
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute readers, with an un-buffered (deferred/yielded) approach, based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
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
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IAsyncEnumerable<T1>, IAsyncEnumerable<T2>, IAsyncEnumerable<T3>, IAsyncEnumerable<T4>, IAsyncEnumerable<T5>, IAsyncEnumerable<T6>)> ReadAsAsyncEnumerable<T1, T2, T3, T4, T5, T6>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(IAsyncEnumerable<T1>, IAsyncEnumerable<T2>, IAsyncEnumerable<T3>, IAsyncEnumerable<T4>, IAsyncEnumerable<T5>, IAsyncEnumerable<T6>), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 6 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
                        p.DeferDisposable(odr);

                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.AsAsyncEnumerable<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.AsAsyncEnumerable<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = odr.AsAsyncEnumerable<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = odr.AsAsyncEnumerable<T4>(p.Token, p.JobCommand);
                                break;
                            case 5:
                                d.Item5 = odr.AsAsyncEnumerable<T5>(p.Token, p.JobCommand);
                                break;
                            case 6:
                                d.Item6 = odr.AsAsyncEnumerable<T6>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }

                        return d;
                    }
                )
                .SetOnError((d, e) => (AsyncEnumerable.Empty<T1>(), AsyncEnumerable.Empty<T2>(), AsyncEnumerable.Empty<T3>(), AsyncEnumerable.Empty<T4>(), AsyncEnumerable.Empty<T5>(), AsyncEnumerable.Empty<T6>()))
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadAsAsyncEnumerable, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (IAsyncEnumerable<T1>)data[0].Item2,
                        (IAsyncEnumerable<T2>)data[1].Item2,
                        (IAsyncEnumerable<T3>)data[2].Item2,
                        (IAsyncEnumerable<T4>)data[3].Item2,
                        (IAsyncEnumerable<T5>)data[4].Item2,
                        (IAsyncEnumerable<T6>)data[5].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                }).WithoutBuffering();
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute readers, with an un-buffered (deferred/yielded) approach, based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
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
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IAsyncEnumerable<T1>, IAsyncEnumerable<T2>, IAsyncEnumerable<T3>, IAsyncEnumerable<T4>, IAsyncEnumerable<T5>, IAsyncEnumerable<T6>, IAsyncEnumerable<T7>)> ReadAsAsyncEnumerable<T1, T2, T3, T4, T5, T6, T7>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(IAsyncEnumerable<T1>, IAsyncEnumerable<T2>, IAsyncEnumerable<T3>, IAsyncEnumerable<T4>, IAsyncEnumerable<T5>, IAsyncEnumerable<T6>, IAsyncEnumerable<T7>), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 7 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
                        p.DeferDisposable(odr);

                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.AsAsyncEnumerable<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.AsAsyncEnumerable<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = odr.AsAsyncEnumerable<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = odr.AsAsyncEnumerable<T4>(p.Token, p.JobCommand);
                                break;
                            case 5:
                                d.Item5 = odr.AsAsyncEnumerable<T5>(p.Token, p.JobCommand);
                                break;
                            case 6:
                                d.Item6 = odr.AsAsyncEnumerable<T6>(p.Token, p.JobCommand);
                                break;
                            case 7:
                                d.Item7 = odr.AsAsyncEnumerable<T7>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }

                        return d;
                    }
                )
                .SetOnError((d, e) => (AsyncEnumerable.Empty<T1>(), AsyncEnumerable.Empty<T2>(), AsyncEnumerable.Empty<T3>(), AsyncEnumerable.Empty<T4>(), AsyncEnumerable.Empty<T5>(), AsyncEnumerable.Empty<T6>(), AsyncEnumerable.Empty<T7>()))
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadAsAsyncEnumerable, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (IAsyncEnumerable<T1>)data[0].Item2,
                        (IAsyncEnumerable<T2>)data[1].Item2,
                        (IAsyncEnumerable<T3>)data[2].Item2,
                        (IAsyncEnumerable<T4>)data[3].Item2,
                        (IAsyncEnumerable<T5>)data[4].Item2,
                        (IAsyncEnumerable<T6>)data[5].Item2,
                        (IAsyncEnumerable<T7>)data[6].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                }).WithoutBuffering();
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute readers, with an un-buffered (deferred/yielded) approach, based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
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
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IAsyncEnumerable<T1>, IAsyncEnumerable<T2>, IAsyncEnumerable<T3>, IAsyncEnumerable<T4>, IAsyncEnumerable<T5>, IAsyncEnumerable<T6>, IAsyncEnumerable<T7>, IAsyncEnumerable<T8>)> ReadAsAsyncEnumerable<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(IAsyncEnumerable<T1>, IAsyncEnumerable<T2>, IAsyncEnumerable<T3>, IAsyncEnumerable<T4>, IAsyncEnumerable<T5>, IAsyncEnumerable<T6>, IAsyncEnumerable<T7>, IAsyncEnumerable<T8>), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 8 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
                        p.DeferDisposable(odr);

                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.AsAsyncEnumerable<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.AsAsyncEnumerable<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = odr.AsAsyncEnumerable<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = odr.AsAsyncEnumerable<T4>(p.Token, p.JobCommand);
                                break;
                            case 5:
                                d.Item5 = odr.AsAsyncEnumerable<T5>(p.Token, p.JobCommand);
                                break;
                            case 6:
                                d.Item6 = odr.AsAsyncEnumerable<T6>(p.Token, p.JobCommand);
                                break;
                            case 7:
                                d.Item7 = odr.AsAsyncEnumerable<T7>(p.Token, p.JobCommand);
                                break;
                            case 8:
                                d.Item8 = odr.AsAsyncEnumerable<T8>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }

                        return d;
                    }
                )
                .SetOnError((d, e) => (AsyncEnumerable.Empty<T1>(), AsyncEnumerable.Empty<T2>(), AsyncEnumerable.Empty<T3>(), AsyncEnumerable.Empty<T4>(), AsyncEnumerable.Empty<T5>(), AsyncEnumerable.Empty<T6>(), AsyncEnumerable.Empty<T7>(), AsyncEnumerable.Empty<T8>()))
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadAsAsyncEnumerable, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (IAsyncEnumerable<T1>)data[0].Item2,
                        (IAsyncEnumerable<T2>)data[1].Item2,
                        (IAsyncEnumerable<T3>)data[2].Item2,
                        (IAsyncEnumerable<T4>)data[3].Item2,
                        (IAsyncEnumerable<T5>)data[4].Item2,
                        (IAsyncEnumerable<T6>)data[5].Item2,
                        (IAsyncEnumerable<T7>)data[6].Item2,
                        (IAsyncEnumerable<T8>)data[7].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                }).WithoutBuffering();
        }
    }
}
