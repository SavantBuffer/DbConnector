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
        #region Executions

        private static IEnumerable<dynamic> OnExecuteReadDynamic(IEnumerable<dynamic> d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return p.IsBuffered ? odr.ToList(p.Token, p.JobCommand)
                                : odr.ToEnumerable(p.Token, p.JobCommand);
        }

        private static dynamic OnExecuteReadFirstDynamic(dynamic d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
            p.DeferDisposable(odr);

            return odr.First(p.Token, p.JobCommand);
        }

        private static dynamic OnExecuteReadFirstOrDefaultDynamic(dynamic d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
            p.DeferDisposable(odr);

            return odr.FirstOrDefault(p.Token, p.JobCommand);
        }

        private static dynamic OnExecuteReadSingleDynamic(dynamic d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return odr.Single(p.Token, p.JobCommand);
        }

        private static dynamic OnExecuteReadSingleOrDefaultDynamic(dynamic d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return odr.SingleOrDefault(p.Token, p.JobCommand);
        }

        private static List<dynamic> OnExecuteReadToListDynamic(List<dynamic> d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return odr.ToList(p.Token, p.JobCommand);
        }

        #endregion

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{IEnumerable{dynamic}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>   
        ///  <para>Use this to dynamically load the query results into an IEnumerable of <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>        
        /// <returns>The <see cref="IDbJob{IEnumerable{dynamic}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<IEnumerable<dynamic>> Read(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<IEnumerable<dynamic>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadDynamic(d, p)
                ).SetOnError((d, e) => Enumerable.Empty<dynamic>());
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{dynamic}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to dynamically load only the first row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>        
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>       
        /// <returns>The <see cref="IDbJob{dynamic}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<dynamic> ReadFirst(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<dynamic, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadFirstDynamic(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{dynamic}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to dynamically load only the first row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>       
        /// <returns>The <see cref="IDbJob{dynamic}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<dynamic> ReadFirstOrDefault(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<dynamic, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadFirstOrDefaultDynamic(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{dynamic}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to dynamically load only a single row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>       
        /// <returns>The <see cref="IDbJob{dynamic}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<dynamic> ReadSingle(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<dynamic, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadSingleDynamic(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{dynamic}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to dynamically load only a single row from the query result into a <see cref="System.Dynamic.ExpandoObject"/>.</para>        
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>       
        /// <returns>The <see cref="IDbJob{dynamic}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<dynamic> ReadSingleOrDefault(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<dynamic, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadSingleOrDefaultDynamic(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{List{dynamic}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>   
        ///  <para>Use this to dynamically load the query results into a List of <see cref="System.Dynamic.ExpandoObject"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>        
        /// <returns>The <see cref="IDbJob{List{dynamic}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<List<dynamic>> ReadToList(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<List<dynamic>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadToListDynamic(d, p)
                ).SetOnError((d, e) => new List<dynamic>());
        }
    }
}
