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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DbConnector.Core
{
    public partial interface IDbConnector<TDbConnection>
       where TDbConnection : DbConnection
    {
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
        IDbJob<IEnumerable<dynamic>> Read(Action<IDbJobCommand> onInit);

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
        IDbJob<dynamic> ReadFirst(Action<IDbJobCommand> onInit);

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
        IDbJob<dynamic> ReadFirstOrDefault(Action<IDbJobCommand> onInit);

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
        IDbJob<dynamic> ReadSingle(Action<IDbJobCommand> onInit);

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
        IDbJob<dynamic> ReadSingleOrDefault(Action<IDbJobCommand> onInit);

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
        IDbJob<List<dynamic>> ReadToList(Action<IDbJobCommand> onInit);
    }
}
