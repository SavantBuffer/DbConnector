using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DbConnector.Core;
using System;
using System.Data.SqlClient;

namespace DbConnector.Tests.Performance
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [BenchmarkCategory("ORM")]
    public abstract class BenchmarksBase : IDisposable
    {
        public static string ConnectionString => "Data Source=localhost\\SQLEXPRESS;Initial Catalog=AdventureWorks2017;Integrated Security=True";

        protected IDbConnector<SqlConnection> _dbConnector;
        protected static readonly Random _rand = new Random();
        protected SqlConnection _connection;
        protected int i;

        protected void BaseSetup()
        {
            i = 0;
            _connection = new SqlConnection(ConnectionString);
            _connection.Open();
        }

        protected void DbConnectorBaseSetup()
        {
            i = 0;
            _dbConnector = new DbConnector<SqlConnection>(ConnectionString);
        }

        protected void Step()
        {
            i++;
            if (i > 5000) i = 1;
        }

        public void Dispose()
        {
            try
            {
                _connection?.Dispose();
            }
            catch (Exception)
            {
            }
        }

        ~BenchmarksBase()
        {
            Dispose();
        }
    }
}
