using DbConnector.Core.Extensions;
using DbConnector.Tests.Entities;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace DbConnector.Tests
{
    public class ReadCustomTests : TestBase
    {
        [Fact]
        public void ReadTo()
        {
            var result = _dbConnector.ReadTo<IEnumerable<Currency>>(
            (cmdActions) =>
            {
                cmdActions.Enqueue((cmd) => cmd.CommandText = "SELECT TOP(3) * FROM [Sales].[Currency]");
            },
            (d, e, odr) =>
            {
                return e.IsBuffered ? odr.ToList<Currency>(e.Token, e.JobCommand)
                : odr.ToEnumerable<Currency>(e.Token, e.JobCommand);

            }
            ).Execute();

            Assert.Equal(3, result.Count());

            var values = (result as List<Currency>);

            Assert.Equal("AED", values[0].CurrencyCode);
            Assert.Equal("AFA", values[1].CurrencyCode);
            Assert.Equal("ALL", values[2].CurrencyCode);
        }

        [Fact]
        public void ReadTo_Deferred()
        {
            var result = _dbConnector.ReadTo<IEnumerable<Currency>>(
            (cmdActions) =>
            {
                cmdActions.Enqueue((cmd) => cmd.CommandText = "SELECT TOP(3) * FROM [Sales].[Currency]");
            },
            (d, e, odr) =>
            {
                return e.IsBuffered ? odr.ToList<Currency>(e.Token, e.JobCommand)
                : odr.ToEnumerable<Currency>(e.Token, e.JobCommand);
            }
            ).WithBuffering(false).Execute();

            Assert.Equal(3, result.Count());

            var values = result.ToList();

            Assert.Equal("AED", values[0].CurrencyCode);
            Assert.Equal("AFA", values[1].CurrencyCode);
            Assert.Equal("ALL", values[2].CurrencyCode);
        }

        [Fact]
        public void Build()
        {
            var result = _dbConnector.Build<IEnumerable<Currency>>(
            (cmdActions) =>
            {
                cmdActions.Enqueue((cmd) => cmd.CommandText = "SELECT TOP(3) * FROM [Sales].[Currency]");
            },
            (d, e) =>
            {
                using (var odr = e.Command.ExecuteReader(e.JobCommand.CommandBehavior ?? (CommandBehavior.SequentialAccess | CommandBehavior.Default)))
                {
                    return odr.ToList<Currency>(e.Token, e.JobCommand);
                }
            }
            ).Execute();

            Assert.Equal(3, result.Count());

            var values = (result as List<Currency>);

            Assert.Equal("AED", values[0].CurrencyCode);
            Assert.Equal("AFA", values[1].CurrencyCode);
            Assert.Equal("ALL", values[2].CurrencyCode);
        }

        [Fact]
        public void ReadTo_Simple()
        {
            var result = _dbConnector.ReadTo<IEnumerable<Currency>>("SELECT TOP(3) * FROM [Sales].[Currency]", null,
            (d, e, odr) =>
            {
                return e.IsBuffered ? odr.ToList<Currency>(e.Token, e.JobCommand)
                : odr.ToEnumerable<Currency>(e.Token, e.JobCommand);

            }).Execute();

            Assert.Equal(3, result.Count());

            var values = (result as List<Currency>);

            Assert.Equal("AED", values[0].CurrencyCode);
            Assert.Equal("AFA", values[1].CurrencyCode);
            Assert.Equal("ALL", values[2].CurrencyCode);
        }

        [Fact]
        public void ReadTo_Simple_Deferred()
        {
            var result = _dbConnector.ReadTo<IEnumerable<Currency>>("SELECT TOP(3) * FROM [Sales].[Currency]", null,
            (d, e, odr) =>
            {
                return e.IsBuffered ? odr.ToList<Currency>(e.Token, e.JobCommand)
                : odr.ToEnumerable<Currency>(e.Token, e.JobCommand);
            }).WithBuffering(false).Execute();

            Assert.Equal(3, result.Count());

            var values = result.ToList();

            Assert.Equal("AED", values[0].CurrencyCode);
            Assert.Equal("AFA", values[1].CurrencyCode);
            Assert.Equal("ALL", values[2].CurrencyCode);
        }

        [Fact]
        public void Build_Simple()
        {
            var result = _dbConnector.Build<IEnumerable<Currency>>("SELECT TOP(3) * FROM [Sales].[Currency]", null,
            (d, e) =>
            {
                using (var odr = e.Command.ExecuteReader(e.JobCommand.CommandBehavior ?? (CommandBehavior.SequentialAccess | CommandBehavior.Default)))
                {
                    return odr.ToList<Currency>(e.Token, e.JobCommand);
                }
            }).Execute();

            Assert.Equal(3, result.Count());

            var values = (result as List<Currency>);

            Assert.Equal("AED", values[0].CurrencyCode);
            Assert.Equal("AFA", values[1].CurrencyCode);
            Assert.Equal("ALL", values[2].CurrencyCode);
        }
    }
}
