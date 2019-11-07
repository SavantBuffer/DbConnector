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
    public class LoggingTest : TestBase
    {
        private class DbLogger : IDbConnectorLogger
        {
            public Action<Exception> OnLog;

            public void Log(Exception ex)
            {
                OnLog?.Invoke(ex);
            }
        }


        [Fact]
        public void Log_ByClass()
        {
            DbLogger logger = new DbLogger
            {
                OnLog = (ex) =>
                {
                    Assert.NotNull(ex);
                    Assert.Equal("Something happened!", ex.Message);
                }
            };

            _dbConnector = new DbConnector<SqlConnection>(ConnectionString, logger);

            var result = _dbConnector.ReadTo<IEnumerable<Currency>>(
            (cmdActions) =>
            {
                cmdActions.Enqueue((cmd) => cmd.CommandText = "SELECT TOP(3) * FROM [Sales].[Currency]");
            },
            (d, e, odr) =>
            {
                throw new Exception("Something happened!");

            }
            ).ExecuteHandled();

        }

        [Fact(Skip = "Might catch global Exceptions from other tests!")]
        public void Log_ByGlobalExtension()
        {
            _dbConnector = new DbConnector<SqlConnection>(ConnectionString);

            DbConnector.Core.Extensions.ExceptionExtensions.OnError = (ex) =>
            {
                Assert.NotNull(ex);
                Assert.Equal("Something happened!", ex.Message);
            };

            var result = _dbConnector.ReadTo<IEnumerable<Currency>>(
            (cmdActions) =>
            {
                cmdActions.Enqueue((cmd) => cmd.CommandText = "SELECT TOP(3) * FROM [Sales].[Currency]");
            },
            (d, e, odr) =>
            {
                throw new Exception("Something happened!");

            }
            ).ExecuteHandled();

            DbConnector.Core.Extensions.ExceptionExtensions.OnError = null;
        }
    }
}
