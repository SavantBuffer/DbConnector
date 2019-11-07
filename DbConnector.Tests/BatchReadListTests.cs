using DbConnector.Tests.Entities;
using DbConnector.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace DbConnector.Tests
{
    public class BatchReadListTests : TestBase
    {
        [Fact]
        public void BatchReadList_2()
        {
            var result = _dbConnector.ReadToList<Currency, Person>((cmd) =>
             {
                 cmd.CommandText = "SELECT TOP(3) * FROM [Sales].[Currency]; SELECT TOP(10) * FROM [Person].[Person];";

             }).Execute();

            Assert.Equal(3, result.Item1.Count());
            Assert.Equal(10, result.Item2.Count());
        }

        [Fact]
        public void BatchReadList_3()
        {
            var result = _dbConnector.ReadToList<Currency, Person, Person>((cmd) =>
            {
                cmd.CommandText = @"
                    SELECT TOP(3) * FROM [Sales].[Currency]; 
                    SELECT TOP(10) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
                ";

            }).Execute();

            Assert.Equal(3, result.Item1.Count());
            Assert.Equal(10, result.Item2.Count());
            Assert.Equal(100, result.Item3.Count());
        }

        [Fact]
        public void BatchReadList_4()
        {
            var result = _dbConnector.ReadToList<Currency, Person, Person, Currency>((cmd) =>
            {
                cmd.CommandText = @"
                    SELECT TOP(3) * FROM [Sales].[Currency]; 
                    SELECT TOP(10) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
                    SELECT TOP(5) * FROM [Sales].[Currency];
                ";

            }).Execute();

            Assert.Equal(3, result.Item1.Count());
            Assert.Equal(10, result.Item2.Count());
            Assert.Equal(100, result.Item3.Count());
            Assert.Equal(5, result.Item4.Count());
        }

        [Fact]
        public void BatchReadList_5()
        {
            var result = _dbConnector.ReadToList<Currency, Person, Person, Currency, Culture>((cmd) =>
            {
                cmd.CommandText = @"
                    SELECT TOP(3) * FROM [Sales].[Currency]; 
                    SELECT TOP(10) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
                    SELECT TOP(5) * FROM [Sales].[Currency];
                    SELECT TOP(8) * FROM [Production].[Culture];
                ";

            }).Execute();

            Assert.Equal(3, result.Item1.Count());
            Assert.Equal(10, result.Item2.Count());
            Assert.Equal(100, result.Item3.Count());
            Assert.Equal(5, result.Item4.Count());
            Assert.Equal(8, result.Item5.Count());
        }

        [Fact]
        public void BatchReadList_6()
        {
            var result = _dbConnector.ReadToList<Currency, Person, Person, Currency, Culture, Address>((cmd) =>
            {
                cmd.CommandText = @"
                    SELECT TOP(3) * FROM [Sales].[Currency]; 
                    SELECT TOP(10) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
                    SELECT TOP(5) * FROM [Sales].[Currency];
                    SELECT TOP(8) * FROM [Production].[Culture];
                    SELECT TOP (20) * FROM [Person].[Address];
                ";

            }).Execute();

            Assert.Equal(3, result.Item1.Count());
            Assert.Equal(10, result.Item2.Count());
            Assert.Equal(100, result.Item3.Count());
            Assert.Equal(5, result.Item4.Count());
            Assert.Equal(8, result.Item5.Count());
            Assert.Equal(20, result.Item6.Count());
        }

        [Fact]
        public void BatchReadList_7()
        {
            var result = _dbConnector.ReadToList<Currency, Person, Person, Currency, Culture, Address, Address>((cmd) =>
            {
                cmd.CommandText = @"
                    SELECT TOP(3) * FROM [Sales].[Currency]; 
                    SELECT TOP(10) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
                    SELECT TOP(5) * FROM [Sales].[Currency];
                    SELECT TOP(8) * FROM [Production].[Culture];
                    SELECT TOP (20) * FROM [Person].[Address];
                    SELECT TOP (500) * FROM [Person].[Address];
                ";

            }).Execute();

            Assert.Equal(3, result.Item1.Count());
            Assert.Equal(10, result.Item2.Count());
            Assert.Equal(100, result.Item3.Count());
            Assert.Equal(5, result.Item4.Count());
            Assert.Equal(8, result.Item5.Count());
            Assert.Equal(20, result.Item6.Count());
            Assert.Equal(500, result.Item7.Count());
        }

        [Fact]
        public void BatchReadList_8()
        {
            var result = _dbConnector.ReadToList<Currency, Person, Person, Currency, Culture, Address, Address, Person>((cmd) =>
            {
                cmd.CommandText = @"
                    SELECT TOP(3) * FROM [Sales].[Currency]; 
                    SELECT TOP(10) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
                    SELECT TOP(5) * FROM [Sales].[Currency];
                    SELECT TOP(8) * FROM [Production].[Culture];
                    SELECT TOP (20) * FROM [Person].[Address];
                    SELECT TOP (500) * FROM [Person].[Address];
                    SELECT TOP (1000) * FROM [Person].[Person];
                ";

            }).Execute();

            Assert.Equal(3, result.Item1.Count());
            Assert.Equal(10, result.Item2.Count());
            Assert.Equal(100, result.Item3.Count());
            Assert.Equal(5, result.Item4.Count());
            Assert.Equal(8, result.Item5.Count());
            Assert.Equal(20, result.Item6.Count());
            Assert.Equal(500, result.Item7.Count());
            Assert.Equal(1000, result.Item8.Count());
        }
    }
}
