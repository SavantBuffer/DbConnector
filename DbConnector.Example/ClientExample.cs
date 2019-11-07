using DbConnector.Core;
using DbConnector.Example.Entities;
using DbConnector.Example.Repositories;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DbConnector.Example
{
    public class ClientExample
    {
        IEmployeeRepository _repoEmployee;

        public ClientExample()
        {
            //TODO: Potentially use dependency injection to do the following:

            //Also, note that you can use any type of data provider adapter that implements a DbConnection.
            //E.g. PostgreSQL, Oracle, MySql, SQL Server

            //Example using SQL Server connection
            IDbConnector<SqlConnection> dbConnector = new DbConnector<SqlConnection>("connection string goes here");

            _repoEmployee = new EmployeeRepository<SqlConnection>(dbConnector);
        }

        public async Task<Employee> GetEmployee(int id)
        {
            //The use of Task/Async allows us to make multiple asynchronous calls
            //to the database leveraging the Task.WaitAll architecture

            return await _repoEmployee.Get(id);
        }
    }
}
