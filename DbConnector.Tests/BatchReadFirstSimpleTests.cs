using DbConnector.Tests.Entities;
using DbConnector.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace DbConnector.Tests
{
    public class BatchReadFirstSimpleTests : TestBase
    {
        [Fact]
        public void BatchReadFirst_2()
        {
            var result = _dbConnector.ReadFirst<Currency, Person>("SELECT TOP(3) * FROM [Sales].[Currency]; SELECT TOP(10) * FROM [Person].[Person];").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
        }

        [Fact]
        public void BatchReadFirst_3()
        {
            var result = _dbConnector.ReadFirst<Currency, Person, Person>(@"
                    SELECT TOP(3) * FROM [Sales].[Currency]; 
                    SELECT TOP(10) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
                ").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
        }

        [Fact]
        public void BatchReadFirst_4()
        {
            var result = _dbConnector.ReadFirst<Currency, Person, Person, Person>(@"
                    SELECT TOP(3) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                ").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
        }

        [Fact]
        public void BatchReadFirst_5()
        {
            var result = _dbConnector.ReadFirst<Currency, Person, Person, Person, Person>(@"
                    SELECT TOP(3) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
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
        public void BatchReadFirst_6()
        {
            var result = _dbConnector.ReadFirst<Currency, Person, Person, Person, Person, Person>(@"
                    SELECT TOP(3) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
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
        public void BatchReadFirst_7()
        {
            var result = _dbConnector.ReadFirst<Currency, Person, Person, Person, Person, Person, Person>(@"
                    SELECT TOP(3) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
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
        public void BatchReadFirst_8()
        {
            var result = _dbConnector.ReadFirst<Currency, Person, Person, Person, Person, Person, Person, Person>(@"
                    SELECT TOP(3) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
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
        public void BatchReadFirst_8_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _dbConnector.ReadFirst<Currency, Person, Person, Person, Person, Person, Person, Person>(@"
                        SELECT TOP(3) * FROM [Sales].[Currency];
                        SELECT TOP(1) * FROM [Person].[Person];
                        SELECT TOP(100) * FROM [Person].[Person];
                        SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                        SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                        SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                        SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 7;
                        SELECT TOP(1) * FROM [Person].[Person] WHERE 1 = 2;
                    ").Execute();

            });
        }

        [Fact]
        public void BatchReadFirstOrDefault_2()
        {
            var result = _dbConnector.ReadFirstOrDefault<Currency, Person>("SELECT TOP(3) * FROM [Sales].[Currency]; SELECT TOP(10) * FROM [Person].[Person];").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
        }

        [Fact]
        public void BatchReadFirstOrDefault_3()
        {
            var result = _dbConnector.ReadFirstOrDefault<Currency, Person, Person>(@"
                    SELECT TOP(3) * FROM [Sales].[Currency]; 
                    SELECT TOP(10) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
                ").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
        }

        [Fact]
        public void BatchReadFirstOrDefault_4()
        {
            var result = _dbConnector.ReadFirstOrDefault<Currency, Person, Person, Person>(@"
                    SELECT TOP(3) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                ").Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
        }

        [Fact]
        public void BatchReadFirstOrDefault_5()
        {
            var result = _dbConnector.ReadFirstOrDefault<Currency, Person, Person, Person, Person>(@"
                    SELECT TOP(3) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
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
        public void BatchReadFirstOrDefault_6()
        {
            var result = _dbConnector.ReadFirstOrDefault<Currency, Person, Person, Person, Person, Person>(@"
                    SELECT TOP(3) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
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
        public void BatchReadFirstOrDefault_7()
        {
            var result = _dbConnector.ReadFirstOrDefault<Currency, Person, Person, Person, Person, Person, Person>(@"
                    SELECT TOP(3) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
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
        public void BatchReadFirstOrDefault_8()
        {
            var result = _dbConnector.ReadFirstOrDefault<Currency, Person, Person, Person, Person, Person, Person, Person>(@"
                    SELECT TOP(3) * FROM [Sales].[Currency]; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
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
        public void BatchReadFirstOrDefault_8_Empty()
        {
            var result = _dbConnector.ReadFirstOrDefault<Currency, Person, Person, Person, Person, Person, Person, Person>(@"
                    SELECT TOP(3) * FROM [Sales].[Currency] where 1 = 2; 
                    SELECT TOP(1) * FROM [Person].[Person];
                    SELECT TOP(100) * FROM [Person].[Person];
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 7;
                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 8;
                ").Execute();

            Assert.Null(result.Item1);
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
