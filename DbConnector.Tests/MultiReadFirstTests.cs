using DbConnector.Tests.Entities;
using DbConnector.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace DbConnector.Tests
{
    public class MultiReadFirstTests : TestBase
    {
        [Fact]
        public void MultiReadFirst_2()
        {
            var result = _dbConnector.ReadFirst<Currency, Person>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                    SELECT TOP(1) * FROM [Sales].[Currency];  
                                ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                    SELECT TOP(1) * FROM [Person].[Person];
                                ";
                        }
                    );
                }
            ).Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
        }

        [Fact]
        public void MultiReadFirst_3()
        {
            var result = _dbConnector.ReadFirst<Currency, Person, Person>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                    SELECT TOP(1) * FROM [Sales].[Currency];  
                                ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                    SELECT TOP(1) * FROM [Person].[Person];
                                ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                    SELECT TOP(1) * FROM [Person].[Person];
                                ";
                        }
                    );
                }
            ).Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
        }

        [Fact]
        public void MultiReadFirst_4()
        {
            var result = _dbConnector.ReadFirst<Currency, Person, Person, Person>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                    SELECT TOP(1) * FROM [Sales].[Currency];  
                                ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                    SELECT TOP(1) * FROM [Person].[Person];
                                ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                    SELECT TOP(1) * FROM [Person].[Person];
                                ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                                ";
                        }
                    );
                }
            ).Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
        }

        [Fact]
        public void MultiReadFirst_5()
        {
            var result = _dbConnector.ReadFirst<Currency, Person, Person, Person, Person>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                    SELECT TOP(1) * FROM [Sales].[Currency];  
                                ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                    SELECT TOP(1) * FROM [Person].[Person];
                                ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                    SELECT TOP(1) * FROM [Person].[Person];
                                ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                                ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                                ";
                        }
                    );
                }
            ).Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
            Assert.Equal(5, result.Item5.BusinessEntityId);
        }

        [Fact]
        public void MultiReadFirst_6()
        {
            var result = _dbConnector.ReadFirst<Currency, Person, Person, Person, Person, Person>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                    SELECT TOP(1) * FROM [Sales].[Currency];  
                                ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                    SELECT TOP(1) * FROM [Person].[Person];
                                ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                    SELECT TOP(1) * FROM [Person].[Person];
                                ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                                ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                                ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                    SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                                ";
                        }
                    );
                }
            ).Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
            Assert.Equal(5, result.Item5.BusinessEntityId);
            Assert.Equal(6, result.Item6.BusinessEntityId);
        }

        [Fact]
        public void MultiReadFirst_7()
        {
            var result = _dbConnector.ReadFirst<Currency, Person, Person, Person, Person, Person, Person>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Sales].[Currency];  
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 7;
                            ";
                        }
                    );
                }
            ).Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
            Assert.Equal(5, result.Item5.BusinessEntityId);
            Assert.Equal(6, result.Item6.BusinessEntityId);
            Assert.Equal(7, result.Item7.BusinessEntityId);
        }

        [Fact]
        public void MultiReadFirst_8()
        {
            var result = _dbConnector.ReadFirst<Currency, Person, Person, Person, Person, Person, Person, Person>(
             () =>
             {
                 return (
                     (cmd) =>
                     {
                         cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Sales].[Currency];  
                            ";
                     }
                 ,
                     (cmd) =>
                     {
                         cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                     }
                 ,
                     (cmd) =>
                     {
                         cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                     }
                 ,
                     (cmd) =>
                     {
                         cmd.CommandText = @"                   
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                            ";
                     }
                 ,
                     (cmd) =>
                     {
                         cmd.CommandText = @"                   
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                            ";
                     }
                 ,
                     (cmd) =>
                     {
                         cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                            ";
                     }
                 ,
                     (cmd) =>
                     {
                         cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 7;
                            ";
                     }
                 ,
                     (cmd) =>
                     {
                         cmd.CommandText = @"
                               SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 8;
                            ";
                     }
                 );
             }
            ).Execute();

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
        public void MultiReadFirst_8_Aggregate_Exception()
        {
            Assert.Throws<AggregateException>(() =>
            {
                _dbConnector.ReadFirst<Currency, Person, Person, Person, Person, Person, Person, Person>(
                    () =>
                     {
                         return (
                             (cmd) =>
                             {
                                 cmd.CommandText = @"
                                        SELECT TOP(1) * FROM [Sales].[Currency];  
                                    ";
                             }
                         ,
                             (cmd) =>
                             {
                                 cmd.CommandText = @"
                                        SELECT TOP(1) * FROM [Person].[Person];
                                    ";
                             }
                         ,
                             (cmd) =>
                             {
                                 cmd.CommandText = @"
                                        SELECT TOP(1) * FROM [Person].[Person];
                                    ";
                             }
                         ,
                             (cmd) =>
                             {
                                 cmd.CommandText = @"                   
                                        SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                                    ";
                             }
                         ,
                             (cmd) =>
                             {
                                 cmd.CommandText = @"                   
                                        SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                                    ";
                             }
                         ,
                             (cmd) =>
                             {
                                 cmd.CommandText = @"
                                        SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                                    ";
                             }
                         ,
                             (cmd) =>
                             {
                                 cmd.CommandText = @"
                                        SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 7;
                                    ";
                             }
                         ,
                             (cmd) =>
                             {
                                 cmd.CommandText = @"
                                       SELECT TOP(1) * FROM [Person].[Person] where 1 = 8;
                                    ";
                             }
                         );
                     }
                ).Execute();

            });
        }

        [Fact]
        public void MultiReadFirst_8_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _dbConnector.ReadFirst<Currency, Person, Person, Person, Person, Person, Person, Person>(
                    () =>
                    {
                        return (
                            (cmd) =>
                            {
                                cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Sales].[Currency];  
                            ";
                            }
                        ,
                            (cmd) =>
                            {
                                cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                            }
                        ,
                            (cmd) =>
                            {
                                cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                            }
                        ,
                            (cmd) =>
                            {
                                cmd.CommandText = @"                   
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                            ";
                            }
                        ,
                            (cmd) =>
                            {
                                cmd.CommandText = @"                   
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                            ";
                            }
                        ,
                            (cmd) =>
                            {
                                cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                            ";
                            }
                        ,
                            (cmd) =>
                            {
                                cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 7;
                            ";
                            }
                        ,
                            (cmd) =>
                            {
                                cmd.CommandText = @"
                               SELECT TOP(1) * FROM [Person].[Person] where 1 = 8;
                            ";
                            }
                        );
                    }
                , withIsolatedConnections: false).Execute();

            });
        }

        [Fact]
        public void MultiReadFirstOrDefault_2()
        {
            var result = _dbConnector.ReadFirstOrDefault<Currency, Person>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Sales].[Currency];  
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                        }
                    );
                }
            ).Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
        }

        [Fact]
        public void MultiReadFirstOrDefault_3()
        {
            var result = _dbConnector.ReadFirstOrDefault<Currency, Person, Person>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Sales].[Currency];  
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                        }
                    );
                }
            ).Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
        }

        [Fact]
        public void MultiReadFirstOrDefault_4()
        {
            var result = _dbConnector.ReadFirstOrDefault<Currency, Person, Person, Person>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Sales].[Currency];  
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                            ";
                        }
                    );
                }
            ).Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
        }

        [Fact]
        public void MultiReadFirstOrDefault_5()
        {
            var result = _dbConnector.ReadFirstOrDefault<Currency, Person, Person, Person, Person>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Sales].[Currency];  
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                            ";
                        }
                    );
                }
            ).Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
            Assert.Equal(5, result.Item5.BusinessEntityId);
        }

        [Fact]
        public void MultiReadFirstOrDefault_6()
        {
            var result = _dbConnector.ReadFirstOrDefault<Currency, Person, Person, Person, Person, Person>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Sales].[Currency];  
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                            ";
                        }
                    );
                }
            ).Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
            Assert.Equal(5, result.Item5.BusinessEntityId);
            Assert.Equal(6, result.Item6.BusinessEntityId);
        }

        [Fact]
        public void MultiReadFirstOrDefault_7()
        {
            var result = _dbConnector.ReadFirstOrDefault<Currency, Person, Person, Person, Person, Person, Person>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Sales].[Currency];  
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 7;
                            ";
                        }
                    );
                }
            ).Execute();

            Assert.Equal("AED", result.Item1.CurrencyCode);
            Assert.Equal(1, result.Item2.BusinessEntityId);
            Assert.Equal(1, result.Item3.BusinessEntityId);
            Assert.Equal(4, result.Item4.BusinessEntityId);
            Assert.Equal(5, result.Item5.BusinessEntityId);
            Assert.Equal(6, result.Item6.BusinessEntityId);
            Assert.Equal(7, result.Item7.BusinessEntityId);
        }

        [Fact]
        public void MultiReadFirstOrDefault_8()
        {
            var result = _dbConnector.ReadFirstOrDefault<Currency, Person, Person, Person, Person, Person, Person, Person>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Sales].[Currency];  
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 7;
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                               SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 8;
                            ";
                        }
                    );
                }
            ).Execute();

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
        public void MultiReadFirstOrDefault_8_Empty()
        {
            var result = _dbConnector.ReadFirstOrDefault<Currency, Person, Person, Person, Person, Person, Person, Person>(
                () =>
                {
                    return (
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Sales].[Currency] WHERE 1 = 2;  
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person];
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 4;
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"                   
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 5;
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 6;
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                                SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 7;
                            ";
                        }
                    ,
                        (cmd) =>
                        {
                            cmd.CommandText = @"
                               SELECT TOP(1) * FROM [Person].[Person] where BusinessEntityId = 8;
                            ";
                        }
                    );
                }
            ).Execute();

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
