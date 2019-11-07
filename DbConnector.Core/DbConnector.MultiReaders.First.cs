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

namespace DbConnector.Core
{
    public partial class DbConnector<TDbConnection> : IDbConnector<TDbConnection>
       where TDbConnection : DbConnection
    {
        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{T1,T2}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{T1,T2}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2)> ReadFirst<T1, T2>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(T1, T2), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 2 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
                        p.DeferDisposable(odr);


                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.First<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.First<T2>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }


                        return d;
                    }
                )
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadFirst, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (T1)data[0].Item2,
                        (T2)data[1].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{T1,T2,T3}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{T1,T2,T3}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3)> ReadFirst<T1, T2, T3>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(T1, T2, T3), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 3 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
                        p.DeferDisposable(odr);


                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.First<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.First<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = odr.First<T3>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }


                        return d;
                    }
                )
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadFirst, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (T1)data[0].Item2,
                        (T2)data[1].Item2,
                        (T3)data[2].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{T1,T2,T4}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{T1,T2,T4}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4)> ReadFirst<T1, T2, T3, T4>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 4 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
                        p.DeferDisposable(odr);


                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.First<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.First<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = odr.First<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = odr.First<T4>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }


                        return d;
                    }
                )
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadFirst, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (T1)data[0].Item2,
                        (T2)data[1].Item2,
                        (T3)data[2].Item2,
                        (T4)data[3].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{T1,T2,T4,T5}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{T1,T2,T4,T5}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4, T5)> ReadFirst<T1, T2, T3, T4, T5>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4, T5), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 5 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
                        p.DeferDisposable(odr);


                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.First<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.First<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = odr.First<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = odr.First<T4>(p.Token, p.JobCommand);
                                break;
                            case 5:
                                d.Item5 = odr.First<T5>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }


                        return d;
                    }
                )
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadFirst, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (T1)data[0].Item2,
                        (T2)data[1].Item2,
                        (T3)data[2].Item2,
                        (T4)data[3].Item2,
                        (T5)data[4].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{T1,T2,T4,T5,T6}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{T1,T2,T4,T5,T6}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6)> ReadFirst<T1, T2, T3, T4, T5, T6>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4, T5, T6), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 6 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
                        p.DeferDisposable(odr);


                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.First<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.First<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = odr.First<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = odr.First<T4>(p.Token, p.JobCommand);
                                break;
                            case 5:
                                d.Item5 = odr.First<T5>(p.Token, p.JobCommand);
                                break;
                            case 6:
                                d.Item6 = odr.First<T6>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }


                        return d;
                    }
                )
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadFirst, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (T1)data[0].Item2,
                        (T2)data[1].Item2,
                        (T3)data[2].Item2,
                        (T4)data[3].Item2,
                        (T5)data[4].Item2,
                        (T6)data[5].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{T1,T2,T4,T5,T6,T7}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{T1,T2,T4,T5,T6,T7}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6, T7)> ReadFirst<T1, T2, T3, T4, T5, T6, T7>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4, T5, T6, T7), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 7 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
                        p.DeferDisposable(odr);


                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.First<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.First<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = odr.First<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = odr.First<T4>(p.Token, p.JobCommand);
                                break;
                            case 5:
                                d.Item5 = odr.First<T5>(p.Token, p.JobCommand);
                                break;
                            case 6:
                                d.Item6 = odr.First<T6>(p.Token, p.JobCommand);
                                break;
                            case 7:
                                d.Item7 = odr.First<T7>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }


                        return d;
                    }
                )
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadFirst, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (T1)data[0].Item2,
                        (T2)data[1].Item2,
                        (T3)data[2].Item2,
                        (T4)data[3].Item2,
                        (T5)data[4].Item2,
                        (T6)data[5].Item2,
                        (T7)data[6].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{T1,T2,T4,T5,T6,T7,T8}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{T1,T2,T4,T5,T6,T7,T8}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6, T7, T8)> ReadFirst<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4, T5, T6, T7, T8), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 8 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
                        p.DeferDisposable(odr);


                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.First<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.First<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = odr.First<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = odr.First<T4>(p.Token, p.JobCommand);
                                break;
                            case 5:
                                d.Item5 = odr.First<T5>(p.Token, p.JobCommand);
                                break;
                            case 6:
                                d.Item6 = odr.First<T6>(p.Token, p.JobCommand);
                                break;
                            case 7:
                                d.Item7 = odr.First<T7>(p.Token, p.JobCommand);
                                break;
                            case 8:
                                d.Item8 = odr.First<T8>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }


                        return d;
                    }
                )
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadFirst, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (T1)data[0].Item2,
                        (T2)data[1].Item2,
                        (T3)data[2].Item2,
                        (T4)data[3].Item2,
                        (T5)data[4].Item2,
                        (T6)data[5].Item2,
                        (T7)data[6].Item2,
                        (T8)data[7].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{T1,T2}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{T1,T2}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(T1, T2)> ReadFirstOrDefault<T1, T2>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(T1, T2), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 2 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
                        p.DeferDisposable(odr);


                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.FirstOrDefault<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.FirstOrDefault<T2>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }


                        return d;
                    }
                )
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadFirstOrDefault, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (T1)data[0].Item2,
                        (T2)data[1].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{T1,T2,T3}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{T1,T2,T3}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(T1, T2, T3)> ReadFirstOrDefault<T1, T2, T3>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(T1, T2, T3), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 3 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
                        p.DeferDisposable(odr);


                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.FirstOrDefault<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.FirstOrDefault<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = odr.FirstOrDefault<T3>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }


                        return d;
                    }
                )
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadFirstOrDefault, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (T1)data[0].Item2,
                        (T2)data[1].Item2,
                        (T3)data[2].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{T1,T2,T4}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{T1,T2,T4}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(T1, T2, T3, T4)> ReadFirstOrDefault<T1, T2, T3, T4>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 4 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
                        p.DeferDisposable(odr);


                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.FirstOrDefault<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.FirstOrDefault<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = odr.FirstOrDefault<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = odr.FirstOrDefault<T4>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }


                        return d;
                    }
                )
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadFirstOrDefault, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (T1)data[0].Item2,
                        (T2)data[1].Item2,
                        (T3)data[2].Item2,
                        (T4)data[3].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{T1,T2,T4,T5}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{T1,T2,T4,T5}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(T1, T2, T3, T4, T5)> ReadFirstOrDefault<T1, T2, T3, T4, T5>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4, T5), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 5 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
                        p.DeferDisposable(odr);


                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.FirstOrDefault<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.FirstOrDefault<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = odr.FirstOrDefault<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = odr.FirstOrDefault<T4>(p.Token, p.JobCommand);
                                break;
                            case 5:
                                d.Item5 = odr.FirstOrDefault<T5>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }


                        return d;
                    }
                )
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadFirstOrDefault, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (T1)data[0].Item2,
                        (T2)data[1].Item2,
                        (T3)data[2].Item2,
                        (T4)data[3].Item2,
                        (T5)data[4].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{T1,T2,T4,T5,T6}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{T1,T2,T4,T5,T6}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6)> ReadFirstOrDefault<T1, T2, T3, T4, T5, T6>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4, T5, T6), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 6 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
                        p.DeferDisposable(odr);


                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.FirstOrDefault<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.FirstOrDefault<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = odr.FirstOrDefault<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = odr.FirstOrDefault<T4>(p.Token, p.JobCommand);
                                break;
                            case 5:
                                d.Item5 = odr.FirstOrDefault<T5>(p.Token, p.JobCommand);
                                break;
                            case 6:
                                d.Item6 = odr.FirstOrDefault<T6>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }


                        return d;
                    }
                )
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadFirstOrDefault, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (T1)data[0].Item2,
                        (T2)data[1].Item2,
                        (T3)data[2].Item2,
                        (T4)data[3].Item2,
                        (T5)data[4].Item2,
                        (T6)data[5].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{T1,T2,T4,T5,T6,T7}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{T1,T2,T4,T5,T6,T7}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6, T7)> ReadFirstOrDefault<T1, T2, T3, T4, T5, T6, T7>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4, T5, T6, T7), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 7 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
                        p.DeferDisposable(odr);


                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.FirstOrDefault<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.FirstOrDefault<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = odr.FirstOrDefault<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = odr.FirstOrDefault<T4>(p.Token, p.JobCommand);
                                break;
                            case 5:
                                d.Item5 = odr.FirstOrDefault<T5>(p.Token, p.JobCommand);
                                break;
                            case 6:
                                d.Item6 = odr.FirstOrDefault<T6>(p.Token, p.JobCommand);
                                break;
                            case 7:
                                d.Item7 = odr.FirstOrDefault<T7>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }


                        return d;
                    }
                )
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadFirstOrDefault, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (T1)data[0].Item2,
                        (T2)data[1].Item2,
                        (T3)data[2].Item2,
                        (T4)data[3].Item2,
                        (T5)data[4].Item2,
                        (T6)data[5].Item2,
                        (T7)data[6].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{T1,T2,T4,T5,T6,T7,T8}}"/> able to execute readers based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query into a result of T.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Func delegate that is used to configure all the <see cref="IDbJobCommand"/>.</param>      
        /// <param name="withIsolatedConnections">By default, one database connection per command will be created/opened thus potentially returning a faster result. See also: <see cref="DbConnectorFlags.NoIsolatedConnectionPerCommand"/>. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{ValueTuple{T1,T2,T4,T5,T6,T7,T8}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6, T7, T8)> ReadFirstOrDefault<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<(Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>, Action<IDbJobCommand>)> onInit, bool? withIsolatedConnections = null)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4, T5, T6, T7, T8), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorDynamicState { Flags = _flags, OnInit = onInit, Count = 8 },
                    onCommands: (conn, state) => BuildJobMultiReaderCommands(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
                        p.DeferDisposable(odr);


                        switch ((p.Index + 1))
                        {
                            case 1:
                                d.Item1 = odr.FirstOrDefault<T1>(p.Token, p.JobCommand);
                                break;
                            case 2:
                                d.Item2 = odr.FirstOrDefault<T2>(p.Token, p.JobCommand);
                                break;
                            case 3:
                                d.Item3 = odr.FirstOrDefault<T3>(p.Token, p.JobCommand);
                                break;
                            case 4:
                                d.Item4 = odr.FirstOrDefault<T4>(p.Token, p.JobCommand);
                                break;
                            case 5:
                                d.Item5 = odr.FirstOrDefault<T5>(p.Token, p.JobCommand);
                                break;
                            case 6:
                                d.Item6 = odr.FirstOrDefault<T6>(p.Token, p.JobCommand);
                                break;
                            case 7:
                                d.Item7 = odr.FirstOrDefault<T7>(p.Token, p.JobCommand);
                                break;
                            case 8:
                                d.Item8 = odr.FirstOrDefault<T8>(p.Token, p.JobCommand);
                                break;
                            default:
                                break;
                        }


                        return d;
                    }
                )
                .WithIsolatedConnections(_flags.IsIsolatedConnectionPerCommand ? (withIsolatedConnections ?? true) : false)
                .OnBranch((d, p, job) =>
                {
                    var stateParam = p.JobState as DbConnectorDynamicState;
                    var data = OnBranchMultiReader(MultiReaderTypes.ReadFirstOrDefault, ref d, p, job, GetMultiReaderActions(stateParam.Count, stateParam.OnInit));

                    d.Data.Source = (
                        (T1)data[0].Item2,
                        (T2)data[1].Item2,
                        (T3)data[2].Item2,
                        (T4)data[3].Item2,
                        (T5)data[4].Item2,
                        (T6)data[5].Item2,
                        (T7)data[6].Item2,
                        (T8)data[7].Item2
                    );

                    OnBranchExecuted(ref d, job as IDbJob, data);

                    return d;
                });
        }
    }
}
