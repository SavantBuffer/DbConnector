using DbConnector.Core;
using DbConnector.Tests.Entities;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DbConnector.Tests
{
    public class ReadCachedTests : TestBase
    {
        private static readonly IDbJobCacheable<IEnumerable<Currency>, int> _read_Cached =
            new DbConnector<SqlConnection>(ConnectionString)
            .Read<Currency>((cmd) =>
            {
                int value = (cmd as IDbJobCommand<int>).StateParam;
                cmd.CommandText = "SELECT TOP(" + value + ") * FROM [Sales].[Currency]";
            })
            .ToCacheable<int>();

        [Fact]
        public void Read_Cached()
        {
            var result = _read_Cached.Execute(3);

            Assert.Equal(3, result.Count());

            var values = (result as List<Currency>);

            Assert.Equal("AED", values[0].CurrencyCode);
            Assert.Equal("AFA", values[1].CurrencyCode);
            Assert.Equal("ALL", values[2].CurrencyCode);
        }

        [Fact]
        public void Read_Cached2()
        {
            var result = _read_Cached.Execute(5);

            Assert.Equal(5, result.Count());

            var values = (result as List<Currency>);

            Assert.Equal("AED", values[0].CurrencyCode);
            Assert.Equal("AFA", values[1].CurrencyCode);
            Assert.Equal("ALL", values[2].CurrencyCode);
        }

        [Fact]
        public void Read_Cached_MultiThreaded()
        {
            var result1 = _read_Cached.ExecuteAsync(3);
            var result2 = _read_Cached.ExecuteAsync(5);
            
            Task.WaitAll(result1, result2);

            Assert.Equal(3, result1.Result.Count());

            var values1 = (result1.Result as List<Currency>);

            Assert.Equal("AED", values1[0].CurrencyCode);
            Assert.Equal("AFA", values1[1].CurrencyCode);
            Assert.Equal("ALL", values1[2].CurrencyCode);

            Assert.Equal(5, result2.Result.Count());

            var values2 = (result2.Result as List<Currency>);

            Assert.Equal("AED", values2[0].CurrencyCode);
            Assert.Equal("AFA", values2[1].CurrencyCode);
            Assert.Equal("ALL", values2[2].CurrencyCode);
        }

        private static readonly IDbJobCacheable<(IEnumerable<Currency>, IEnumerable<Person>), (int, int)> _multiRead_2_Cached =
            new DbConnector<SqlConnection>(ConnectionString)
            .Read<Currency, Person>(() =>
            {
                return (
                    (cmd) =>
                    {
                        int value = (cmd as IDbJobCommand<(int, int)>).StateParam.Item1;
                        cmd.CommandText = "SELECT TOP(" + value + ") * FROM [Sales].[Currency];";
                    }
                ,
                    (cmd) =>
                    {
                        int value = (cmd as IDbJobCommand<(int, int)>).StateParam.Item2;
                        cmd.CommandText = "SELECT TOP(" + value + ") * FROM [Person].[Person];";
                    }
                );
            })
            .ToCacheable<(int, int)>();

        [Fact]
        public void MultiRead_2_Cached()
        {
            var result = _multiRead_2_Cached.Execute((3, 10));

            Assert.Equal(3, result.Item1.Count());
            Assert.Equal(10, result.Item2.Count());
        }

        private static readonly IDbJobCacheable<DataSet, (int, int)> _readToDataSet_Cached =
            new DbConnector<SqlConnection>(ConnectionString)
            .ReadToDataSet((cmds) =>
            {
                cmds.Enqueue((cmd) =>
                {
                    int value = (cmd as IDbJobCommand<(int, int)>).StateParam.Item1;
                    cmd.CommandText = "SELECT TOP(" + value + ") * FROM [Sales].[Currency]";
                });

                cmds.Enqueue((cmd) =>
                {
                    int value = (cmd as IDbJobCommand<(int, int)>).StateParam.Item2;
                    cmd.CommandText = "SELECT TOP(" + value + ") * FROM [Sales].[Currency]";
                });

            })
            .ToCacheable<(int, int)>();

        [Fact]
        public void ReadToDataSet_Cached()
        {
            var result = _readToDataSet_Cached.Execute((5, 6));

            Assert.NotNull(result);
            Assert.Equal(5, result.Tables[0].Rows.Count);
            Assert.Equal(6, result.Tables[1].Rows.Count);

            foreach (DataTable item in result.Tables)
            {
                Assert.Equal("AED", item.Rows[0][0]);
                Assert.Equal("AFA", item.Rows[1][0]);
            }
        }
    }
}
