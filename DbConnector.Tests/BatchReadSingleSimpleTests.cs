using DbConnector.Tests.Entities;
using DbConnector.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace DbConnector.Tests
{
    public class BatchReadSingleSimpleTests : TestBase
    {
        [Fact]
        public void BatchReadSingle_2()
        {
            var result = _dbConnector.ReadSingle<Currency, Person>("SELECT TOP(1) * FROM [Sales].[Currency]; SELECT TOP(1) * FROM [Person].[Person];").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
        }

        [Fact]
        public void BatchReadSingle_3()
        {
            var result = _dbConnector.ReadSingle<Currency, Person, Person>(@"
                    SELECT TOP(1) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person];
                ").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
        }

        [Fact]
        public void BatchReadSingle_4()
        {
            var result = _dbConnector.ReadSingle<Currency, Person, Person, Person>(@"
                    SELECT TOP(1) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                ").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
        }

        [Fact]
        public void BatchReadSingle_5()
        {
            var result = _dbConnector.ReadSingle<Currency, Person, Person, Person, Person>(@"
                    SELECT TOP(1) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                ").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
            Assert.Equal(5, result.Item5.BusinessEntityId);
        }

        [Fact]
        public void BatchReadSingle_6()
        {
            var result = _dbConnector.ReadSingle<Currency, Person, Person, Person, Person, Person>(@"
                    SELECT TOP(1) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                ").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
            Assert.Equal(5, result.Item5.BusinessEntityId);
            Assert.Equal(6, result.Item6.BusinessEntityId);
        }

        [Fact]
        public void BatchReadSingle_7()
        {
            var result = _dbConnector.ReadSingle<Currency, Person, Person, Person, Person, Person, Person>(@"
                    SELECT TOP(1) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 7;
                ").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
            Assert.Equal(5, result.Item5.BusinessEntityId);
            Assert.Equal(6, result.Item6.BusinessEntityId);
            Assert.Equal(7, result.Item7.BusinessEntityId);
        }

        [Fact]
        public void BatchReadSingle_8()
        {
            var result = _dbConnector.ReadSingle<Currency, Person, Person, Person, Person, Person, Person, Person>(@"
                    SELECT TOP(1) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 7;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 8;
                ").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
            Assert.Equal(5, result.Item5.BusinessEntityId);
            Assert.Equal(6, result.Item6.BusinessEntityId);
            Assert.Equal(7, result.Item7.BusinessEntityId);
            Assert.Equal(8, result.Item8.BusinessEntityId);
        }

        [Fact]
        public void BatchReadSingle_8_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _dbConnector.ReadSingle<Currency, Person, Person, Person, Person, Person, Person, Person>(@"
                        SELECT TOP(1) * FROM [Sales].[Currency];
                        SELECT TOP(1) * FROM [Person].[Person];
                        SELECT TOP(1) * FROM [Person].[Person];
                        SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                        SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                        SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                        SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 7;
                        SELECT TOP(1) * FROM [Person].[Person] WHERE 1 = 2;
                    ").Execute();

            });
        }

        [Fact]
        public void BatchReadSingle_8_Exception_MoreThanOne()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _dbConnector.ReadSingle<Currency, Person, Person, Person, Person, Person, Person, Person>(@"
                        SELECT TOP(3) * FROM [Sales].[Currency];
                        SELECT TOP(1) * FROM [Person].[Person];
                        SELECT TOP(1) * FROM [Person].[Person];
                        SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                        SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                        SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                        SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 7;
                        SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 8;
                    ").Execute();

            });
        }

        [Fact]
        public void BatchReadSingleOrDefault_2()
        {
            var result = _dbConnector.ReadSingleOrDefault<Currency, Person>("SELECT TOP(1) * FROM [Sales].[Currency]; SELECT TOP(1) * FROM [Person].[Person];").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
        }

        [Fact]
        public void BatchReadSingleOrDefault_3()
        {
            var result = _dbConnector.ReadSingleOrDefault<Currency, Person, Person>(@"
                    SELECT TOP(1) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person];
                ").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
        }

        [Fact]
        public void BatchReadSingleOrDefault_4()
        {
            var result = _dbConnector.ReadSingleOrDefault<Currency, Person, Person, Person>(@"
                    SELECT TOP(1) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                ").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
        }

        [Fact]
        public void BatchReadSingleOrDefault_5()
        {
            var result = _dbConnector.ReadSingleOrDefault<Currency, Person, Person, Person, Person>(@"
                    SELECT TOP(1) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                ").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
            Assert.Equal(5, result.Item5.BusinessEntityId);
        }

        [Fact]
        public void BatchReadSingleOrDefault_6()
        {
            var result = _dbConnector.ReadSingleOrDefault<Currency, Person, Person, Person, Person, Person>(@"
                    SELECT TOP(1) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                ").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
            Assert.Equal(5, result.Item5.BusinessEntityId);
            Assert.Equal(6, result.Item6.BusinessEntityId);
        }

        [Fact]
        public void BatchReadSingleOrDefault_7()
        {
            var result = _dbConnector.ReadSingleOrDefault<Currency, Person, Person, Person, Person, Person, Person>(@"
                    SELECT TOP(1) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 7;
                ").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
            Assert.Equal(5, result.Item5.BusinessEntityId);
            Assert.Equal(6, result.Item6.BusinessEntityId);
            Assert.Equal(7, result.Item7.BusinessEntityId);
        }

        [Fact]
        public void BatchReadSingleOrDefault_8()
        {
            var result = _dbConnector.ReadSingleOrDefault<Currency, Person, Person, Person, Person, Person, Person, Person>(@"
                    SELECT TOP(1) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 7;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 8;
                ").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
            Assert.Equal(5, result.Item5.BusinessEntityId);
            Assert.Equal(6, result.Item6.BusinessEntityId);
            Assert.Equal(7, result.Item7.BusinessEntityId);
            Assert.Equal(8, result.Item8.BusinessEntityId);
        }
    }
}
