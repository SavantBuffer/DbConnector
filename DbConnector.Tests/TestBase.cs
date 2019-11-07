using DbConnector.Core;
using System;
using System.Data.SqlClient;
using Xunit;

namespace DbConnector.Tests
{
    public abstract class TestBase : IDisposable
    {
        public static readonly string ConnectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=AdventureWorks2017;Integrated Security=True";

        protected IDbConnector<SqlConnection> _dbConnector;

        public TestBase()
        {
            //Using SQL Server connection
            _dbConnector = new DbConnector<SqlConnection>(ConnectionString);
        }

        [Fact]
        protected virtual void IsConnected()
        {
            Assert.True(_dbConnector.IsConnected().Execute());
        }

        public void Dispose()
        {

        }
    }
}
