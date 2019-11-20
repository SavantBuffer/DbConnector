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
        ///  <para>Creates an <see cref="IDbJob"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>       
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>)> Read<T1, T2>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>), TDbConnection>
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
                                d.Item1 = p.IsBuffered ? odr.ToList<T1>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = p.IsBuffered ? odr.ToList<T2>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T2>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }

                        return d;
                    }
                )
                .SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>()))
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.Read, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (IEnumerable<T1>)data[0].Item2,
                        (IEnumerable<T2>)data[1].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>       
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> Read<T1, T2, T3>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>), TDbConnection>
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
                                d.Item1 = p.IsBuffered ? odr.ToList<T1>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = p.IsBuffered ? odr.ToList<T2>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = p.IsBuffered ? odr.ToList<T3>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T3>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }

                        return d;
                    }
                )
                .SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>()))
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.Read, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (IEnumerable<T1>)data[0].Item2,
                        (IEnumerable<T2>)data[1].Item2,
                        (IEnumerable<T3>)data[2].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>       
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>)> Read<T1, T2, T3, T4>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>), TDbConnection>
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
                                d.Item1 = p.IsBuffered ? odr.ToList<T1>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = p.IsBuffered ? odr.ToList<T2>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = p.IsBuffered ? odr.ToList<T3>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = p.IsBuffered ? odr.ToList<T4>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T4>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }

                        return d;
                    }
                )
                .SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>()))
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.Read, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (IEnumerable<T1>)data[0].Item2,
                        (IEnumerable<T2>)data[1].Item2,
                        (IEnumerable<T3>)data[2].Item2,
                        (IEnumerable<T4>)data[3].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>       
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>)> Read<T1, T2, T3, T4, T5>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>), TDbConnection>
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
                                d.Item1 = p.IsBuffered ? odr.ToList<T1>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = p.IsBuffered ? odr.ToList<T2>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = p.IsBuffered ? odr.ToList<T3>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = p.IsBuffered ? odr.ToList<T4>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T4>(p.Token, p.JobCommand);
                                break;
                            case 5:
                                d.Item5 = p.IsBuffered ? odr.ToList<T5>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T5>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }

                        return d;
                    }
                )
                .SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>(), Enumerable.Empty<T5>()))
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.Read, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (IEnumerable<T1>)data[0].Item2,
                        (IEnumerable<T2>)data[1].Item2,
                        (IEnumerable<T3>)data[2].Item2,
                        (IEnumerable<T4>)data[3].Item2,
                        (IEnumerable<T5>)data[4].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>       
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>)> Read<T1, T2, T3, T4, T5, T6>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>), TDbConnection>
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
                                d.Item1 = p.IsBuffered ? odr.ToList<T1>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = p.IsBuffered ? odr.ToList<T2>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = p.IsBuffered ? odr.ToList<T3>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = p.IsBuffered ? odr.ToList<T4>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T4>(p.Token, p.JobCommand);
                                break;
                            case 5:
                                d.Item5 = p.IsBuffered ? odr.ToList<T5>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T5>(p.Token, p.JobCommand);
                                break;
                            case 6:
                                d.Item6 = p.IsBuffered ? odr.ToList<T6>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T6>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }

                        return d;
                    }
                )
                .SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>(), Enumerable.Empty<T5>(), Enumerable.Empty<T6>()))
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.Read, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (IEnumerable<T1>)data[0].Item2,
                        (IEnumerable<T2>)data[1].Item2,
                        (IEnumerable<T3>)data[2].Item2,
                        (IEnumerable<T4>)data[3].Item2,
                        (IEnumerable<T5>)data[4].Item2,
                        (IEnumerable<T6>)data[5].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>       
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>, IEnumerable<T7>)> Read<T1, T2, T3, T4, T5, T6, T7>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>, IEnumerable<T7>), TDbConnection>
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
                                d.Item1 = p.IsBuffered ? odr.ToList<T1>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = p.IsBuffered ? odr.ToList<T2>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = p.IsBuffered ? odr.ToList<T3>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = p.IsBuffered ? odr.ToList<T4>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T4>(p.Token, p.JobCommand);
                                break;
                            case 5:
                                d.Item5 = p.IsBuffered ? odr.ToList<T5>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T5>(p.Token, p.JobCommand);
                                break;
                            case 6:
                                d.Item6 = p.IsBuffered ? odr.ToList<T6>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T6>(p.Token, p.JobCommand);
                                break;
                            case 7:
                                d.Item7 = p.IsBuffered ? odr.ToList<T7>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T7>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }

                        return d;
                    }
                )
                .SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>(), Enumerable.Empty<T5>(), Enumerable.Empty<T6>(), Enumerable.Empty<T7>()))
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.Read, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (IEnumerable<T1>)data[0].Item2,
                        (IEnumerable<T2>)data[1].Item2,
                        (IEnumerable<T3>)data[2].Item2,
                        (IEnumerable<T4>)data[3].Item2,
                        (IEnumerable<T5>)data[4].Item2,
                        (IEnumerable<T6>)data[5].Item2,
                        (IEnumerable<T7>)data[6].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>       
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>, IEnumerable<T7>, IEnumerable<T8>)> Read<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>, IEnumerable<T7>, IEnumerable<T8>), TDbConnection>
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
                                d.Item1 = p.IsBuffered ? odr.ToList<T1>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = p.IsBuffered ? odr.ToList<T2>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = p.IsBuffered ? odr.ToList<T3>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = p.IsBuffered ? odr.ToList<T4>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T4>(p.Token, p.JobCommand);
                                break;
                            case 5:
                                d.Item5 = p.IsBuffered ? odr.ToList<T5>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T5>(p.Token, p.JobCommand);
                                break;
                            case 6:
                                d.Item6 = p.IsBuffered ? odr.ToList<T6>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T6>(p.Token, p.JobCommand);
                                break;
                            case 7:
                                d.Item7 = p.IsBuffered ? odr.ToList<T7>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T7>(p.Token, p.JobCommand);
                                break;
                            case 8:
                                d.Item8 = p.IsBuffered ? odr.ToList<T8>(p.Token, p.JobCommand)
                                        : odr.ToEnumerable<T8>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }

                        return d;
                    }
                )
                .SetOnError((d, e) => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>(), Enumerable.Empty<T5>(), Enumerable.Empty<T6>(), Enumerable.Empty<T7>(), Enumerable.Empty<T8>()))
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.Read, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (IEnumerable<T1>)data[0].Item2,
                        (IEnumerable<T2>)data[1].Item2,
                        (IEnumerable<T3>)data[2].Item2,
                        (IEnumerable<T4>)data[3].Item2,
                        (IEnumerable<T5>)data[4].Item2,
                        (IEnumerable<T6>)data[5].Item2,
                        (IEnumerable<T7>)data[6].Item2,
                        (IEnumerable<T8>)data[7].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }
    }
}
