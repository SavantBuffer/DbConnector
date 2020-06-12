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
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2)> ReadSingle<T1, T2>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(T1, T2), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3)> ReadSingle<T1, T2, T3>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(T1, T2, T3), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4)> ReadSingle<T1, T2, T3, T4>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4, T5)> ReadSingle<T1, T2, T3, T4, T5>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4, T5), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6)> ReadSingle<T1, T2, T3, T4, T5, T6>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4, T5, T6), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6, T7)> ReadSingle<T1, T2, T3, T4, T5, T6, T7>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4, T5, T6, T7), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6, T7, T8)> ReadSingle<T1, T2, T3, T4, T5, T6, T7, T8>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4, T5, T6, T7, T8), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadOne(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2)> ReadSingleOrDefault<T1, T2>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(T1, T2), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2, T3)> ReadSingleOrDefault<T1, T2, T3>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(T1, T2, T3), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2, T3, T4)> ReadSingleOrDefault<T1, T2, T3, T4>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2, T3, T4, T5)> ReadSingleOrDefault<T1, T2, T3, T4, T5>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4, T5), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6)> ReadSingleOrDefault<T1, T2, T3, T4, T5, T6>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4, T5, T6), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6, T7)> ReadSingleOrDefault<T1, T2, T3, T4, T5, T6, T7>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4, T5, T6, T7), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }

        /// <summary>
        ///  <para>Creates an <see cref="IDbJob"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Use this to load only a single row from the query into a result of T.</para>
        ///  <para>Valid T types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader()"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<(T1, T2, T3, T4, T5, T6, T7, T8)> ReadSingleOrDefault<T1, T2, T3, T4, T5, T6, T7, T8>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException(nameof(onInit), "The onInit delegate cannot be null!");
            }

            return new DbJob<(T1, T2, T3, T4, T5, T6, T7, T8), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) => OnExecuteReadOneOrDefault(d, p, false)
                );
        }
    }
}
