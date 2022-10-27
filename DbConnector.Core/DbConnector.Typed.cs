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

        private static IEnumerable<object> OnExecuteRead(Type type, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return p.IsBuffered ? odr.ToList(type, p.Token, p.JobCommand)
                                : odr.AsEnumerable(type, p.Token, p.JobCommand);
        }

        private static IAsyncEnumerable<object> OnExecuteReadAsAsyncEnumerable(Type type, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return odr.AsAsyncEnumerable(type, p.Token, p.JobCommand);
        }

        private static object OnExecuteReadFirst(Type type, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
            p.DeferDisposable(odr);

            return odr.First(type, p.Token, p.JobCommand);
        }

        private static object OnExecuteReadFirstOrDefault(Type type, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
            p.DeferDisposable(odr);

            return odr.FirstOrDefault(type, p.Token, p.JobCommand);
        }

        private static object OnExecuteReadSingle(Type type, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return odr.Single(type, p.Token, p.JobCommand);
        }

        private static object OnExecuteReadSingleOrDefault(Type type, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return odr.SingleOrDefault(type, p.Token, p.JobCommand);
        }

        private static List<object> OnExecuteReadToList(Type type, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return odr.ToList(type, p.Token, p.JobCommand);
        }

        private static HashSet<object> OnExecuteReadToHashSetOfObject(HashSet<object> d, IDbExecutionModel p)
        {
            DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
            p.DeferDisposable(odr);

            return odr.ToHashSet(p.Token);
        }

        #endregion

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IEnumerable{object}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>        
        /// <returns>The <see cref="IDbJob{IEnumerable{object}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<IEnumerable<object>> Read(Type type, Action<IDbJobCommand> onInit)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<IEnumerable<object>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteRead(type, p)
                ).SetOnError((d, e) => Enumerable.Empty<object>());
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{IAsyncEnumerable{object}}"/> able to execute a reader, with an un-buffered (deferred/yielded) approach, based on the <paramref name="onInit"/> action.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
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
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>        
        /// <returns>The <see cref="IDbJob{IAsyncEnumerable{object}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<IAsyncEnumerable<object>> ReadAsAsyncEnumerable(Type type, Action<IDbJobCommand> onInit)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<IAsyncEnumerable<object>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadAsAsyncEnumerable(type, p)
                ).SetOnError((d, e) => AsyncEnumerable.Empty<object>()).WithoutBuffering();
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{object}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>       
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<object> ReadFirst(Type type, Action<IDbJobCommand> onInit)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onInit: () => type.IsValueType ? Activator.CreateInstance(type) : null,
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadFirst(type, p)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{object}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>       
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<object> ReadFirstOrDefault(Type type, Action<IDbJobCommand> onInit)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onInit: () => type.IsValueType ? Activator.CreateInstance(type) : null,
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadFirstOrDefault(type, p)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{object}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>       
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<object> ReadSingle(Type type, Action<IDbJobCommand> onInit)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onInit: () => type.IsValueType ? Activator.CreateInstance(type) : null,
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadSingle(type, p)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{object}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>       
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<object> ReadSingleOrDefault(Type type, Action<IDbJobCommand> onInit)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onInit: () => type.IsValueType ? Activator.CreateInstance(type) : null,
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadSingleOrDefault(type, p)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{List{object}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to use.</param>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>        
        /// <returns>The <see cref="IDbJob{List{object}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<List<object>> ReadToList(Type type, Action<IDbJobCommand> onInit)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "The type cannot be null!");
            }

            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<List<object>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadToList(type, p)
                ).SetOnError((d, e) => new List<object>());
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{HashSet{object}}"/> able to read the first column of each row from the query result based on the <paramref name="onInit"/> action. All other columns are ignored.</para>  
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default. <see cref="DBNull"/> values will be excluded.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{HashSet{object}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<HashSet<object>> ReadToHashSet(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<HashSet<object>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadToHashSetOfObject(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob{object}"/> to get the first column of the first row from the result
        ///  set returned by the query. All other columns and rows are ignored.</para>        
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteScalar"/>
        /// </summary>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{object}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<object> Scalar(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) =>
                    {
                        object scalar = p.Command.ExecuteScalar();

                        return scalar != DBNull.Value ? scalar : null;
                    }
                );
        }
    }
}
