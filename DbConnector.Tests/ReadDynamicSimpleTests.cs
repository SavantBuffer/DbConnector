using DbConnector.Tests.Entities;
using DbConnector.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace DbConnector.Tests
{
    public class ReadDynamicSimpleTests : TestBase
    {
        [Fact]
        public void Read_Dynamic()
        {
            var result = _dbConnector.Read("SELECT 10 as count FROM [Person].[Person]").Execute();

            Assert.Equal(10, result.First().count);
        }

        [Fact]
        public void Read_Dynamic_WithData()
        {
            var result = _dbConnector.Read("SELECT TOP(10) * FROM [Person].[Person] ORDER BY BusinessEntityID").Execute() as List<dynamic>;

            Assert.Equal(10, result.Count);
            Assert.Equal(1, result.First().BusinessEntityID);
        }

        [Fact]
        public void ReadFirst_Dynamic()
        {
            var result = _dbConnector.ReadFirst("SELECT 10 as count FROM [Person].[Person]").Execute();

            Assert.NotNull(result);
            Assert.Equal(10, result.count);
        }

        [Fact]
        public void ReadFirstOrDefault_Dynamic()
        {
            var result = _dbConnector.ReadFirstOrDefault("SELECT 10 as count FROM [Person].[Person]").Execute();

            Assert.NotNull(result);
            Assert.Equal(10, result.count);
        }

        [Fact]
        public void ReadFirst_Dynamic_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _dbConnector.ReadFirst("SELECT 10 as count FROM [Person].[Person] WHERE 1=2").Execute();
            });
        }

        [Fact]
        public void ReadSingle_Dynamic()
        {
            var result = _dbConnector.ReadSingle("SELECT TOP(1) 10 as count FROM [Person].[Person]").Execute();

            Assert.NotNull(result);
            Assert.Equal(10, result.count);
        }

        [Fact]
        public void ReadSingleOrDefault_Dynamic()
        {
            var result = _dbConnector.ReadSingleOrDefault("SELECT 10 as count FROM [Person].[Person] WHERE 1=2").Execute();

            Assert.Null(result);
        }

        [Fact]
        public void ReadSingle_Dynamic_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _dbConnector.ReadSingle("SELECT 10 as count FROM [Person].[Person] WHERE 1=2").Execute();
            });
        }

        [Fact]
        public void ReadSingle_Dynamic_Exception_MoreThanOne()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _dbConnector.ReadSingle("SELECT 10 as count FROM [Person].[Person]").Execute();
            });
        }

        [Fact]
        public void ReadToList_Dynamic()
        {
            var result = _dbConnector.ReadToList("SELECT 10 as count FROM [Person].[Person]").Execute();

            Assert.Equal(10, result.First().count);
        }

        [Fact]
        public void ReadByBatch_Dynamic()
        {
            var result = _dbConnector.Read<dynamic, dynamic>("SELECT TOP(3) * FROM [Sales].[Currency]; SELECT TOP(10) * FROM [Person].[Person];").Execute();

            Assert.Equal(3, result.Item1.Count());
            Assert.Equal(10, result.Item2.Count());
        }

        [Fact]
        public void ReadToListByBatch_Dynamic()
        {
            var result = _dbConnector.ReadToList<dynamic, dynamic>("SELECT TOP(3) * FROM [Sales].[Currency]; SELECT TOP(10) * FROM [Person].[Person];").Execute();

            Assert.Equal(3, result.Item1.Count());
            Assert.Equal(10, result.Item2.Count());
        }

        [Fact]
        public void ReadFirstOrDefaultByBatch_Dynamic()
        {
            var result = _dbConnector.ReadFirstOrDefault<dynamic, dynamic>("SELECT TOP(3) * FROM [Sales].[Currency]; SELECT TOP(10) * FROM [Person].[Person];").Execute();

            Assert.NotNull(result.Item1);
            Assert.NotNull(result.Item2);
        }

        [Fact]
        public void ReadSingleOrDefaultByBatch_Dynamic()
        {
            var result = _dbConnector.ReadSingleOrDefault<dynamic, dynamic>("SELECT TOP(1) * FROM [Sales].[Currency]; SELECT TOP(1) * FROM [Person].[Person];").Execute();

            Assert.NotNull(result.Item1);
            Assert.NotNull(result.Item2);
        }
    }
}
