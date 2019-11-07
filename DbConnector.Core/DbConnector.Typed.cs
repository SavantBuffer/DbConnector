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
        ///  <para>Creates a <see cref="IDbJob{IEnumerable{object}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
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
                throw new ArgumentNullException("type cannot be null!");
            }

            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<IEnumerable<object>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onInit: () => Enumerable.Empty<object>(),
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
                        p.DeferDisposable(odr);

                        return p.IsBuffered ? odr.ToList(type, p.Token, p.JobCommand)
                                            : odr.ToEnumerable(type, p.Token, p.JobCommand);
                    }
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{object}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
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
                throw new ArgumentNullException("type cannot be null!");
            }

            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onInit: () =>
                    {
                        if (type.IsValueType)
                        {
                            return Activator.CreateInstance(type);
                        }
                        else
                        {
                            return null;
                        }
                    },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
                        p.DeferDisposable(odr);

                        return odr.First(type, p.Token, p.JobCommand);
                    }
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{object}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only the first row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
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
                throw new ArgumentNullException("type cannot be null!");
            }

            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onInit: () =>
                    {
                        if (type.IsValueType)
                        {
                            return Activator.CreateInstance(type);
                        }
                        else
                        {
                            return null;
                        }
                    },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, _commandBehaviorSingleResultOrSingleRow));
                        p.DeferDisposable(odr);

                        return odr.FirstOrDefault(type, p.Token, p.JobCommand);
                    }
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{object}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
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
                throw new ArgumentNullException("type cannot be null!");
            }

            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onInit: () =>
                    {
                        if (type.IsValueType)
                        {
                            return Activator.CreateInstance(type);
                        }
                        else
                        {
                            return null;
                        }
                    },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
                        p.DeferDisposable(odr);

                        return odr.Single(type, p.Token, p.JobCommand);
                    }
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{object}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query result into an object.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
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
                throw new ArgumentNullException("type cannot be null!");
            }

            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<object, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onInit: () =>
                    {
                        if (type.IsValueType)
                        {
                            return Activator.CreateInstance(type);
                        }
                        else
                        {
                            return null;
                        }
                    },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
                        p.DeferDisposable(odr);

                        return odr.SingleOrDefault(type, p.Token, p.JobCommand);
                    }
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{List{object}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        /// <para>Valid types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
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
                throw new ArgumentNullException("type cannot be null!");
            }

            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<List<object>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onInit: () => new List<object>(),
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.SingleResult));
                        p.DeferDisposable(odr);

                        return odr.ToList(type, p.Token, p.JobCommand);
                    }
                );
        }
    }
}
