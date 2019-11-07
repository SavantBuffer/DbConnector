using DbConnector.Core;
using DbConnector.Core.Extensions;
using DbConnector.Tests.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Xunit;

namespace DbConnector.Tests
{
    public class FlagTest : TestBase
    {
        [Fact]
        public void Flag_NoExceptionThrowingForNonHandledExecution()
        {
            _dbConnector = new DbConnector<SqlConnection>(ConnectionString, DbConnectorFlags.NoExceptionThrowingForNonHandledExecution);

            var result = _dbConnector.ReadTo<Currency>(
            (cmdActions) =>
            {
                cmdActions.Enqueue((cmd) => cmd.CommandText = "SELECT TOP(3) * FROM [Sales].[Currency]");
            },
            (d, e, odr) =>
            {
                throw new Exception("Something happened!");
            }
            ).Execute();


            Assert.Null(result);
        }

        [Fact]
        public void Flag_Other()
        {
            //Other flags can only be tested internally.
            //_dbConnector = new DbConnector<SqlConnection>(ConnectionString, DbConnectorFlags.None);

            //var result = _dbConnector.Read<Currency, Person>(
            //    (cmd, cmd2) =>
            //    {
            //        cmd.CommandText = @"
            //            SELECT TOP(3) * FROM [Sales].[Currency]; 
            //        ";

            //        cmd2.CommandText = @"
            //            SELECT TOP(10) * FROM [Person].[Person];
            //        ";
            //    }
            //).Execute();

            //Assert.Equal(3, result.Item1.Count());
            //Assert.Equal(10, result.Item2.Count());
        }
    }
}
