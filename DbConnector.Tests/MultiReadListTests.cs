using DbConnector.Tests.Entities;
using DbConnector.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace DbConnector.Tests
{
    public class MultiReadListTests : TestBase
    {
        [Fact]
        public void MultiReadList_2()
        {
            var result = _dbConnector.ReadToList<Currency, Person>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(3) * FROM [Sales].[Currency];  
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(10) * FROM [Person].[Person];
                            ";
                        }
                    );
                }
            ).Execute();

            Assert.Equal(3, result.Item1.Count());
            Assert.Equal(10, result.Item2.Count());
        }

        [Fact]
        public void MultiReadList_3()
        {
            var result = _dbConnector.ReadToList<Currency, Person, Person>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(3) * FROM [Sales].[Currency];  
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(10) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(100) * FROM [Person].[Person];
                            ";
                        }
                    );
                }
            ).Execute();

            Assert.Equal(3, result.Item1.Count());
            Assert.Equal(10, result.Item2.Count());
            Assert.Equal(100, result.Item3.Count());
        }

        [Fact]
        public void MultiReadList_4()
        {
            var result = _dbConnector.ReadToList<Currency, Person, Person, Currency>(
               () =>
               {
                   return (
                       (cmd) =>
                       {
                           cmd.CommandText = @"
                                SELECT TOP(3) * FROM [Sales].[Currency];  
                            ";
                       }
                   ,
                       (cmd) =>
                       {
                           cmd.CommandText = @"
                                SELECT TOP(10) * FROM [Person].[Person];
                            ";
                       }
                   ,
                       (cmd) =>
                       {
                           cmd.CommandText = @"
                                SELECT TOP(100) * FROM [Person].[Person];
                            ";
                       }
                   ,
                       (cmd) =>
                       {
                           cmd.CommandText = @"                   
                                SELECT TOP(5) * FROM [Sales].[Currency];
                            ";
                       }
                   );
               }
            ).Execute();

            Assert.Equal(3, result.Item1.Count());
            Assert.Equal(10, result.Item2.Count());
            Assert.Equal(100, result.Item3.Count());
            Assert.Equal(5, result.Item4.Count());
        }

        [Fact]
        public void MultiReadList_5()
        {
            var result = _dbConnector.ReadToList<Currency, Person, Person, Currency, Culture>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(3) * FROM [Sales].[Currency];  
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(10) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(100) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(5) * FROM [Sales].[Currency];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(8) * FROM [Production].[Culture];
                            ";
                        }
                    );
                }
            ).Execute();

            Assert.Equal(3, result.Item1.Count());
            Assert.Equal(10, result.Item2.Count());
            Assert.Equal(100, result.Item3.Count());
            Assert.Equal(5, result.Item4.Count());
            Assert.Equal(8, result.Item5.Count());
        }

        [Fact]
        public void MultiReadList_6()
        {
            var result = _dbConnector.ReadToList<Currency, Person, Person, Currency, Culture, Address>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(3) * FROM [Sales].[Currency];  
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(10) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(100) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(5) * FROM [Sales].[Currency];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(8) * FROM [Production].[Culture];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP (20) * FROM [Person].[Address];
                            ";
                        }
                    );
                }
            ).Execute();

            Assert.Equal(3, result.Item1.Count());
            Assert.Equal(10, result.Item2.Count());
            Assert.Equal(100, result.Item3.Count());
            Assert.Equal(5, result.Item4.Count());
            Assert.Equal(8, result.Item5.Count());
            Assert.Equal(20, result.Item6.Count());
        }

        [Fact]
        public void MultiReadList_7()
        {
            var result = _dbConnector.ReadToList<Currency, Person, Person, Currency, Culture, Address, Address>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(3) * FROM [Sales].[Currency];  
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(10) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(100) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(5) * FROM [Sales].[Currency];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(8) * FROM [Production].[Culture];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP (20) * FROM [Person].[Address];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP (500) * FROM [Person].[Address];
                            ";
                        }
                    );
                }
            ).Execute();

            Assert.Equal(3, result.Item1.Count());
            Assert.Equal(10, result.Item2.Count());
            Assert.Equal(100, result.Item3.Count());
            Assert.Equal(5, result.Item4.Count());
            Assert.Equal(8, result.Item5.Count());
            Assert.Equal(20, result.Item6.Count());
            Assert.Equal(500, result.Item7.Count());
        }

        [Fact]
        public void MultiReadList_8()
        {
            var result = _dbConnector.ReadToList<Currency, Person, Person, Currency, Culture, Address, Address, Person>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(3) * FROM [Sales].[Currency];  
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(10) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(100) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(5) * FROM [Sales].[Currency];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(8) * FROM [Production].[Culture];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP (20) * FROM [Person].[Address];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP (500) * FROM [Person].[Address];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                               SELECT TOP (1000) * FROM [Person].[Person];
                            ";
                        }
                    );
                }
            ).Execute();

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
