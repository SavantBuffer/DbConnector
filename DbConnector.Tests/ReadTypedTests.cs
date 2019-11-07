using DbConnector.Tests.Entities;
using DbConnector.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace DbConnector.Tests
{
    public class ReadTypedTests : TestBase
    {
        [Fact]
        public void Read_Typed()
        {
            var result = _dbConnector.Read(typeof(Currency), (cmd) =>
             {
                 cmd.CommandText = "SELECT TOP(3) * FROM [Sales].[Currency]";
             }).Execute();

            Assert.Equal(3, result.Count());

            var values = result.Cast<Currency>().ToList();

            Assert.Equal("AED", values[0].CurrencyCode);
            Assert.Equal("AFA", values[1].CurrencyCode);
            Assert.Equal("ALL", values[2].CurrencyCode);
        }

        [Fact]
        public void ReadFirst_Typed()
        {
            var result = _dbConnector.ReadFirst(typeof(Currency), (cmd) =>
            {
                cmd.CommandText = "SELECT TOP(3) * FROM [Sales].[Currency]";
            }).Execute();

            Assert.NotNull(result);
            Assert.Equal("AED", (result as Currency).CurrencyCode);
        }

        [Fact]
        public void ReadFirstOrDefault_Typed()
        {
            var result = _dbConnector.ReadFirst(typeof(Currency), (cmd) =>
            {
                cmd.CommandText = "SELECT TOP(3) * FROM [Sales].[Currency]";
            }).Execute();

            Assert.NotNull(result);
            Assert.Equal("AED", (result as Currency).CurrencyCode);
        }

        [Fact]
        public void ReadFirst_Typed_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var result = _dbConnector.ReadFirst(typeof(Currency), (cmd) =>
                {
                    cmd.CommandText = "SELECT TOP(3) * FROM [Sales].[Currency] WHERE 1 = 2";
                }).Execute();
            });
        }

        [Fact]
        public void ReadSingle_Typed()
        {
            var result = _dbConnector.ReadSingle(typeof(Currency), (cmd) =>
            {
                cmd.CommandText = "SELECT TOP(1) * FROM [Sales].[Currency]";
            }).Execute();

            Assert.NotNull(result);
            Assert.Equal("AED", (result as Currency).CurrencyCode);
        }

        [Fact]
        public void ReadSingleOrDefault_Typed()
        {
            var result = _dbConnector.ReadSingleOrDefault(typeof(Currency), (cmd) =>
            {
                cmd.CommandText = "SELECT TOP(1) * FROM [Sales].[Currency] where 1 = 2";
            }).Execute();

            Assert.Null(result);
        }

        [Fact]
        public void ReadSingle_Typed_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _dbConnector.ReadSingle(typeof(Currency), (cmd) =>
                {
                    cmd.CommandText = "SELECT TOP(1) * FROM [Sales].[Currency] where 1 = 2";
                }).Execute();
            });
        }

        [Fact]
        public void ReadSingle_Typed_Exception_MoreThanOne()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _dbConnector.ReadSingle(typeof(Currency), (cmd) =>
                {
                    cmd.CommandText = "SELECT TOP(10) * FROM [Sales].[Currency]";
                }).Execute();
            });
        }

        [Fact]
        public void ReadToList_Typed()
        {
            var result = _dbConnector.ReadToList(typeof(Currency), (cmd) =>
            {
                cmd.CommandText = "SELECT TOP(10) * FROM [Person].[Person]";
            }).Execute();

            Assert.Equal(10, result.Count);
        }
    }
}
