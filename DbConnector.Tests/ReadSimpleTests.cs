using DbConnector.Tests.Entities;
using DbConnector.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;
using System.Data.SqlClient;
using System.Threading;

namespace DbConnector.Tests
{
    public class ReadSimpleTests : TestBase
    {
        [Fact]
        public void Read()
        {
            var result = _dbConnector.Read<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]").Execute();

            Assert.Equal(3, result.Count());

            var values = (result as List<Currency>);

            Assert.Equal("AED", values[0].CurrencyCode);
            Assert.Equal("AFA", values[1].CurrencyCode);
            Assert.Equal("ALL", values[2].CurrencyCode);
        }

        [Fact]
        public async void Read_Async()
        {
            var result = await _dbConnector.Read<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]").ExecuteAsync();

            Assert.Equal(3, result.Count());

            var values = (result as List<Currency>);

            Assert.Equal("AED", values[0].CurrencyCode);
            Assert.Equal("AFA", values[1].CurrencyCode);
            Assert.Equal("ALL", values[2].CurrencyCode);
        }

        [Fact]
        public void Read_Handled()
        {
            var result = _dbConnector.Read<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]").ExecuteHandled();

            Assert.Equal(3, result.Data.Count());

            var values = (result.Data as List<Currency>);

            Assert.Equal("AED", values[0].CurrencyCode);
            Assert.Equal("AFA", values[1].CurrencyCode);
            Assert.Equal("ALL", values[2].CurrencyCode);
        }

        [Fact]
        public async void Read_HandledAsync()
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = await _dbConnector.Read<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]").ExecuteHandledAsync(conn);

                Assert.Equal(3, result.Data.Count());

                var values = (result.Data as List<Currency>);

                Assert.Equal("AED", values[0].CurrencyCode);
                Assert.Equal("AFA", values[1].CurrencyCode);
                Assert.Equal("ALL", values[2].CurrencyCode);
            }
        }

        [Fact]
        public void Read_Dispoable()
        {
            IDbJob<IEnumerable<Currency>> job = _dbConnector.Read<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]");

            using (IDbDisposable<IEnumerable<Currency>> result = job.ExecuteDisposable())
            {
                Assert.Equal(3, result.Source.Count());

                var values = (result.Source as List<Currency>);

                Assert.Equal("AED", values[0].CurrencyCode);
                Assert.Equal("AFA", values[1].CurrencyCode);
                Assert.Equal("ALL", values[2].CurrencyCode);
            }
        }

        [Fact]
        public async void Read_DispoableAsync()
        {
            IDbJob<IEnumerable<Currency>> job = _dbConnector.Read<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]");

            using (IDbDisposable<IEnumerable<Currency>> result = await job.ExecuteDisposableAsync())
            {
                Assert.Equal(3, result.Source.Count());

                var values = (result.Source as List<Currency>);

                Assert.Equal("AED", values[0].CurrencyCode);
                Assert.Equal("AFA", values[1].CurrencyCode);
                Assert.Equal("ALL", values[2].CurrencyCode);
            }
        }

        [Fact]
        public void Read_DispoableHandled()
        {
            IDbJob<IEnumerable<Currency>> job = _dbConnector.Read<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]");

            using (IDbDisposable<IEnumerable<Currency>> result = job.ExecuteDisposableHandled().Data)
            {
                Assert.Equal(3, result.Source.Count());

                var values = (result.Source as List<Currency>);

                Assert.Equal("AED", values[0].CurrencyCode);
                Assert.Equal("AFA", values[1].CurrencyCode);
                Assert.Equal("ALL", values[2].CurrencyCode);
            }
        }

        [Fact]
        public async void Read_DispoableHandledAsync()
        {
            IDbJob<IEnumerable<Currency>> job = _dbConnector.Read<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]");

            using (IDbDisposable<IEnumerable<Currency>> result = (await job.ExecuteDisposableHandledAsync()).Data)
            {
                Assert.Equal(3, result.Source.Count());

                var values = (result.Source as List<Currency>);

                Assert.Equal("AED", values[0].CurrencyCode);
                Assert.Equal("AFA", values[1].CurrencyCode);
                Assert.Equal("ALL", values[2].CurrencyCode);
            }
        }

        [Fact]
        public void ReadAsAsyncEnumerable()
        {
            var result = _dbConnector.ReadAsAsyncEnumerable<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]").Execute().ToListAsync().Result;

            Assert.Equal(3, result.Count());

            var values = (result as List<Currency>);

            Assert.Equal("AED", values[0].CurrencyCode);
            Assert.Equal("AFA", values[1].CurrencyCode);
            Assert.Equal("ALL", values[2].CurrencyCode);
        }

        [Fact]
        public async void ReadAsAsyncEnumerable_Async()
        {
            var result = await (await _dbConnector.ReadAsAsyncEnumerable<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]").ExecuteAsync()).ToListAsync();

            Assert.Equal(3, result.Count());

            var values = (result as List<Currency>);

            Assert.Equal("AED", values[0].CurrencyCode);
            Assert.Equal("AFA", values[1].CurrencyCode);
            Assert.Equal("ALL", values[2].CurrencyCode);
        }

        [Fact]
        public void ReadAsAsyncEnumerable_Handled()
        {
            var result = _dbConnector.ReadAsAsyncEnumerable<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]").ExecuteHandled().Data.ToListAsync().Result;

            Assert.Equal(3, result.Count());

            var values = result;

            Assert.Equal("AED", values[0].CurrencyCode);
            Assert.Equal("AFA", values[1].CurrencyCode);
            Assert.Equal("ALL", values[2].CurrencyCode);
        }

        [Fact]
        public async void ReadAsAsyncEnumerable_HandledAsync()
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = await (await _dbConnector.ReadAsAsyncEnumerable<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]").ExecuteHandledAsync(conn)).Data.ToListAsync();

                Assert.Equal(3, result.Count());

                var values = result;

                Assert.Equal("AED", values[0].CurrencyCode);
                Assert.Equal("AFA", values[1].CurrencyCode);
                Assert.Equal("ALL", values[2].CurrencyCode);
            }
        }

        [Fact]
        public void ReadAsAsyncEnumerable_Dispoable()
        {
            IDbJob<IAsyncEnumerable<Currency>> job = _dbConnector.ReadAsAsyncEnumerable<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]");

            using (IDbDisposable<IAsyncEnumerable<Currency>> result = job.ExecuteDisposable())
            {
                var source = result.Source.ToListAsync().Result;

                Assert.Equal(3, source.Count());

                var values = source;

                Assert.Equal("AED", values[0].CurrencyCode);
                Assert.Equal("AFA", values[1].CurrencyCode);
                Assert.Equal("ALL", values[2].CurrencyCode);
            }
        }

        [Fact]
        public async void ReadAsAsyncEnumerable_DispoableAsync()
        {
            IDbJob<IAsyncEnumerable<Currency>> job = _dbConnector.ReadAsAsyncEnumerable<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]");

            using (IDbDisposable<IAsyncEnumerable<Currency>> result = await job.ExecuteDisposableAsync())
            {
                var source = await result.Source.ToListAsync();

                Assert.Equal(3, source.Count());

                var values = source;

                Assert.Equal("AED", values[0].CurrencyCode);
                Assert.Equal("AFA", values[1].CurrencyCode);
                Assert.Equal("ALL", values[2].CurrencyCode);
            }
        }

        [Fact]
        public void ReadAsAsyncEnumerable_DispoableHandled()
        {
            IDbJob<IAsyncEnumerable<Currency>> job = _dbConnector.ReadAsAsyncEnumerable<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]");

            using (IDbDisposable<IAsyncEnumerable<Currency>> result = job.ExecuteDisposableHandled().Data)
            {
                var source = result.Source.ToListAsync().Result;

                Assert.Equal(3, source.Count());

                var values = source;

                Assert.Equal("AED", values[0].CurrencyCode);
                Assert.Equal("AFA", values[1].CurrencyCode);
                Assert.Equal("ALL", values[2].CurrencyCode);
            }
        }

        [Fact]
        public async void ReadAsAsyncEnumerable_DispoableHandledAsync()
        {
            IDbJob<IAsyncEnumerable<Currency>> job = _dbConnector.ReadAsAsyncEnumerable<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]");

            using (IDbDisposable<IAsyncEnumerable<Currency>> result = (await job.ExecuteDisposableHandledAsync()).Data)
            {
                var source = await result.Source.ToListAsync();

                Assert.Equal(3, source.Count());

                var values = source;

                Assert.Equal("AED", values[0].CurrencyCode);
                Assert.Equal("AFA", values[1].CurrencyCode);
                Assert.Equal("ALL", values[2].CurrencyCode);
            }
        }

        [Fact]
        public void ReadFirst()
        {
            var result = _dbConnector.ReadFirst<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]")
            .OnExecuted((d, e) =>
            {
                d.Name = "TestingOnExecuted";
                return d;
            })
            .Execute();

            Assert.NotNull(result);
            Assert.Equal("AED", result.CurrencyCode);
            Assert.Equal("TestingOnExecuted", result.Name);
        }

        [Fact]
        public void ReadFirst_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _dbConnector.ReadFirst<Currency>("SELECT TOP(3) * FROM [Sales].[Currency] where 50 = 20").Execute();

            });
        }

        [Fact]
        public void ReadFirstOrDefault()
        {
            var result = _dbConnector.ReadFirstOrDefault<Currency>("SELECT TOP(1) * FROM [Sales].[Currency] where 50 = 20").Execute();

            Assert.Null(result);
        }

        [Fact]
        public void ReadFirstOrDefault_Top10()
        {
            var result = _dbConnector.ReadFirstOrDefault<Currency>("SELECT TOP(10) * FROM [Sales].[Currency]").Execute();

            Assert.NotNull(result);
        }

        [Fact]
        public void ReadSingle()
        {
            var result = _dbConnector.ReadSingle<Currency>("SELECT TOP(1) * FROM [Sales].[Currency]").Execute();

            Assert.NotNull(result);
            Assert.Equal("AED", result.CurrencyCode);
        }

        [Fact]
        public void ReadSingle_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _dbConnector.ReadSingle<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]").Execute();

            });
        }

        [Fact]
        public void ReadSingleOrDefault()
        {
            var result = _dbConnector.ReadSingleOrDefault<Currency>("SELECT TOP(1) * FROM [Sales].[Currency] where 50 = 20").Execute();

            Assert.Null(result);
        }

        [Fact]
        public void ReadSingleOrDefault_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _dbConnector.ReadSingleOrDefault<Currency>("SELECT TOP(3) * FROM [Sales].[Currency]").Execute();

            });
        }

        [Fact]
        public void ReadToDataTable()
        {
            var result = _dbConnector.ReadToDataTable("SELECT TOP(1) * FROM [Sales].[Currency]").Execute();

            Assert.NotNull(result);
            Assert.Equal("AED", result.Rows[0][0]);
        }

        [Fact]
        public void ReadToDataSet()
        {
            var result = _dbConnector.ReadToDataSet("SELECT TOP(5) * FROM [Sales].[Currency];SELECT TOP(5) * FROM [Sales].[Currency]").Execute();

            Assert.NotNull(result);

            foreach (DataTable item in result.Tables)
            {
                Assert.Equal("AED", item.Rows[0][0]);
                Assert.Equal("AFA", item.Rows[1][0]);
            }
        }

        [Fact]
        public void ReadToDictionaries()
        {
            var result = _dbConnector.ReadToDictionaries("SELECT TOP(5) * FROM [Sales].[Currency]").Execute();

            Assert.NotNull(result);

            var item = result.First();

            Assert.Equal("AED", item["CurrencyCode"]);
            Assert.Equal("Emirati Dirham", item["Name"]);
        }

        [Fact]
        public void ReadToKeyValuePairs()
        {
            var result = _dbConnector.ReadToKeyValuePairs("SELECT TOP(5) * FROM [Sales].[Currency]").Execute();

            Assert.NotNull(result);

            var item = result.First();

            Assert.Equal("AED", item.First(i => i.Key == "CurrencyCode").Value);
            Assert.Equal("Emirati Dirham", item.First(i => i.Key == "Name").Value);
        }

        [Fact]
        public void ReadToListOfDictionaries()
        {
            var result = _dbConnector.ReadToListOfDictionaries("SELECT TOP(5) * FROM [Sales].[Currency]").Execute();

            Assert.NotNull(result);

            var item = result.First();

            Assert.Equal("AED", item["CurrencyCode"]);
            Assert.Equal("Emirati Dirham", item["Name"]);
        }

        [Fact]
        public void ReadToListOfKeyValuePairs()
        {
            var result = _dbConnector.ReadToListOfKeyValuePairs("SELECT TOP(5) * FROM [Sales].[Currency]").Execute();

            Assert.NotNull(result);

            var item = result.First();

            Assert.Equal("AED", item.First(i => i.Key == "CurrencyCode").Value);
            Assert.Equal("Emirati Dirham", item.First(i => i.Key == "Name").Value);
        }

        [Fact]
        public void ReadToDbCollectionSet()
        {
            var result = _dbConnector.ReadToDbCollectionSet("SELECT TOP(5) * FROM [Sales].[Currency]; SELECT TOP(5) * FROM [Sales].[Currency]").Execute();


            Assert.NotNull(result);
            var item = result.Items.First().First();
            Assert.Equal("AED", item["CurrencyCode"]);
            Assert.Equal("Emirati Dirham", item["Name"]);


            var mappedItem = result.ElementAt<Currency>(0);
            Assert.NotNull(mappedItem);
            Assert.Equal("AED", mappedItem.CurrencyCode);
            Assert.Equal("Emirati Dirham", mappedItem.Name);


            var mappedItems = result.ElementsAt<Currency>(0);
            Assert.Equal(5, mappedItems.Count());


            var mappedDataSet = result.ToDataSet(false);
            Assert.NotNull(mappedDataSet);
            Assert.Equal(2, mappedDataSet.Tables.Count);
            Assert.Equal(5, mappedDataSet.Tables[0].Rows.Count);
            Assert.Equal(5, mappedDataSet.Tables[1].Rows.Count);


            var mappedDequeuedItems = result.Dequeue<Currency>();
            Assert.Equal(5, mappedDequeuedItems.Count());
            Assert.Single(result.Items);


            var mappedDequeuedItems2 = result.Dequeue<Currency>();
            Assert.Equal(5, mappedDequeuedItems2.Count());
            Assert.Empty(result.Items);
        }

        [Fact]
        public void Scalar()
        {
            var result = _dbConnector.Scalar<int?>("SELECT 20;").Execute();

            Assert.NotNull(result);

            Assert.Equal(20, result.Value);
        }

        [Fact]
        public void Scalar_NonNullable()
        {
            var result = _dbConnector.Scalar<int>("SELECT 20;").Execute();

            Assert.Equal(20, result);
        }

        [Fact]
        public void ScalarDirect()
        {
            var result = _dbConnector.Scalar("SELECT 20;").Execute();

            Assert.NotNull(result);

            Assert.Equal(20, (int)result);
        }

        [Fact]
        public void Read_NoBuffer()
        {
            var result = _dbConnector.Read<Currency>(
                    new ColumnMapSetting().ExcludeNames("CurrencyCode"),
                    "SELECT TOP(50) * FROM [Sales].[Currency]")
                .WithBuffering(false).Execute();

            int count = 0;

            foreach (var item in result)
            {
                count++;
                Assert.NotNull(item.Name);
            }

            Assert.Equal(50, count);
        }

        [Fact]
        public async void Read_NoBufferAsync()
        {
            var result = await _dbConnector.Read<Currency>(
                    new ColumnMapSetting().ExcludeNames("CurrencyCode"),
                    "SELECT TOP(50) * FROM [Sales].[Currency]")
                .WithBuffering(false).ExecuteAsync();

            int count = 0;

            foreach (var item in result)
            {
                count++;
                Assert.NotNull(item.Name);
            }

            Assert.Equal(50, count);
        }

        [Fact]
        public async void Read_NoBufferAsyncEnumerable()
        {
            var result = _dbConnector.ReadAsAsyncEnumerable<Currency>(
                    "SELECT TOP(50) * FROM [Sales].[Currency]").Execute();

            int count = 0;

            await foreach (var item in result)
            {
                count++;
                Assert.NotNull(item.Name);
            }

            Assert.Equal(50, count);
        }

        [Fact]
        public async void Read_Buffered_GetFirst()
        {
            var result = await _dbConnector.Read<Currency>(
                    new ColumnMapSetting().ExcludeNames("CurrencyCode"),
                    "SELECT TOP(5) * FROM [DatabaseLog]")
                .ExecuteAsync();

            Assert.NotNull(result.First());
        }

        [Fact]
        public async void Read_Buffered_GetFirst_AsAsyncEnumerable()
        {
            CancellationToken token = new CancellationToken();
            var result = await _dbConnector.ReadAsAsyncEnumerable<Currency>(
                    "SELECT TOP(5) * FROM [DatabaseLog]").ExecuteAsync(token: token);

            await foreach (var item in result)
            {
                Assert.NotNull(item);
                break;
            }
        }

        [Fact]
        public void Read_WithParameters()
        {
            DateTime now = DateTime.Now;

            var result = _dbConnector.Read(
                @"SELECT @t1 as 't1', @t2 as 't2', @t3 as 't3', @t4 as 't4', @Name as 'Name', @ModifiedDate as 'ModifiedDate', @id as 'id' 
                 FROM [Person].[Person] WHERE [rowguid] = @rowguid;",
                new
                {
                    t1 = 1,
                    t2 = 2,
                    t3 = 3,
                    rowguid = new Guid("92C4279F-1207-48A3-8448-4636514EB7E2"),
                    t4 = 'a',
                    Name = "this is a name",
                    ModifiedDate = now,
                    id = "9000"
                }
            ).Execute();

            Assert.Single(result);

            var values = (result as List<dynamic>);

            Assert.Equal(1, values[0].t1);
            Assert.Equal(2, values[0].t2);
            Assert.Equal(3, values[0].t3);
            Assert.Equal("a", values[0].t4);
            Assert.Equal("this is a name", values[0].Name);
            Assert.Equal(now.ToShortDateString(), values[0].ModifiedDate.ToShortDateString());
            Assert.Equal("9000", values[0].id);
        }

        [Fact]
        public void Read_WithColumnMapSettings_ExcludeNames()
        {
            var result = _dbConnector.Read<Currency>(
                        new ColumnMapSetting().ExcludeNames("CurrencyCode"),
                        "SELECT TOP(3) * FROM [Sales].[Currency]"
                    ).Execute();

            Assert.Equal(3, result.Count());

            foreach (var item in result)
            {
                Assert.Null(item.CurrencyCode);
                Assert.NotNull(item.Name);
            }
        }

        [Fact]
        public void Read_String()
        {
            var result = _dbConnector.Read<string>("SELECT TOP(3) CurrencyCode FROM [Sales].[Currency] ORDER BY CurrencyCode;").Execute();

            Assert.Equal(3, result.Count());

            var values = (result as List<string>);

            Assert.Equal("AED", values[0]);
            Assert.Equal("AFA", values[1]);
            Assert.Equal("ALL", values[2]);
        }

        [Fact]
        public void ReadToHashSet()
        {
            var result = _dbConnector.ReadToHashSet<string>("SELECT TOP(10) CurrencyCode FROM [Sales].[Currency] ORDER BY CurrencyCode;").Execute();

            Assert.NotNull(result);

            Assert.Equal(10, result.Count());

            Assert.Contains("AED", result);
        }

        [Fact]
        public void ReadToHashSet_BadWithBuffering()
        {
            var result = _dbConnector.ReadToHashSet<string>("SELECT TOP(10) CurrencyCode FROM [Sales].[Currency] ORDER BY CurrencyCode;").WithBuffering(false).Execute();

            Assert.NotNull(result);

            Assert.Equal(10, result.Count());

            Assert.Contains("AED", result);
        }

        [Fact]
        public void ReadToHashSet_Param()
        {
            var result = _dbConnector.ReadToHashSet<string>("SELECT TOP(10) CurrencyCode FROM [Sales].[Currency] WHERE @test = 1 ORDER BY CurrencyCode;", new { test = 1 }).Execute();

            Assert.NotNull(result);

            Assert.Equal(10, result.Count());

            Assert.Contains("AED", result);
        }

        [Fact]
        public void ReadToHashSetOfObject()
        {
            var result = _dbConnector.ReadToHashSet("SELECT TOP(10) CurrencyCode FROM [Sales].[Currency] ORDER BY CurrencyCode;").Execute();

            Assert.NotNull(result);

            Assert.Equal(10, result.Count());

            Assert.Contains("AED", result);
        }

        [Fact]
        public void ReadToHashSetOfObject_Param()
        {
            var result = _dbConnector.ReadToHashSet("SELECT TOP(10) CurrencyCode FROM [Sales].[Currency] WHERE @test = 1 ORDER BY CurrencyCode;", new { test = 1 }).Execute();

            Assert.NotNull(result);

            Assert.Equal(10, result.Count());

            Assert.Contains("AED", result);
        }
    }
}
