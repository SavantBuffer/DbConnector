using DbConnector.Core;
using DbConnector.Tests.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace DbConnector.Tests
{
    public class NonQueryTests : TestBase
    {
        [Fact]
        public void NonQuery()
        {
            var result = _dbConnector.NonQuery((cmd) =>
            {
                cmd.CommandText = @"
                    INSERT INTO [Production].[Location]
                        ([Name]
                        ,[CostRate])
                    VALUES
                        (@Name
                        ,@CostRate)
                ";

                cmd.Parameters.AddFor(new
                {
                    Name = "4100",
                    CostRate = 5
                });

            }).Execute();

            Assert.NotNull(result);
            Assert.Equal(1, result);

            var deleteResult = _dbConnector.NonQuery((cmd) =>
            {
                cmd.CommandText = @"
                    DELETE FROM [Production].[Location] WHERE Name = @Name
                ";

                cmd.Parameters.AddFor(new
                {
                    Name = "4100",
                    CostRate = 5
                });

            }).Execute();

            Assert.NotNull(deleteResult);
            Assert.Equal(1, deleteResult);
        }

        [Fact]
        public void NonQuery_Generic()
        {
            var result = _dbConnector.NonQuery<bool?>((cmd) =>
            {
                cmd.CommandText = @"
                    INSERT INTO [Production].[Location]
                        ([Name]
                        ,[CostRate])
                    VALUES
                        (@Name
                        ,@CostRate)
                ";

                cmd.Parameters.AddFor(new
                {
                    Name = "8002",
                    CostRate = 5
                });

            }).OnCompleted(d => true).Execute();

            Assert.NotNull(result);
            Assert.Equal(true, result);

            var deleteResult = _dbConnector.NonQuery((cmd) =>
            {
                cmd.CommandText = @"
                    DELETE FROM [Production].[Location] WHERE Name = @Name
                ";

                cmd.Parameters.AddFor(new
                {
                    Name = "8002",
                    CostRate = 5
                });

            }).Execute();

            Assert.NotNull(deleteResult);
            Assert.Equal(1, deleteResult);
        }

        [Fact]
        public void NonQueries()
        {
            var jobInsert = _dbConnector.NonQueries((cmds) =>
            {
                cmds.Enqueue((cmd) =>
                {

                    cmd.CommandText = @"
                            INSERT INTO [Production].[Location]
                                ([Name]
                                ,[CostRate])
                            VALUES
                                (@Name
                                ,@CostRate)
                        ";

                    cmd.Parameters.AddFor(new
                    {
                        Name = "9021",
                        CostRate = 5
                    });

                });

                cmds.Enqueue((cmd) =>
                {

                    cmd.CommandText = @"
                            INSERT INTO [Production].[Location]
                                ([Name]
                                ,[CostRate])
                            VALUES
                                (@Name
                                ,@CostRate)
                        ";

                    cmd.Parameters.AddFor(new
                    {
                        Name = "9031",
                        CostRate = 6
                    });

                });
            });

            var jobDelete = _dbConnector.NonQuery((cmd) =>
            {
                cmd.CommandText = @"
                    DELETE FROM [Production].[Location] WHERE Name = @Name OR Name = @Name2;
                ";

                cmd.Parameters.AddFor(new
                {
                    Name = "9021",
                    Name2 = "9031",
                });

            });

            var results = DbJob.ExecuteAll(jobInsert, jobDelete);

            Assert.NotNull(results.First().Item2);
            Assert.Equal(2, results.First().Item2);

            Assert.NotNull(results[1].Item2);
            Assert.Equal(2, results[1].Item2);
        }

        [Fact]
        public void NonQueries_Generic()
        {
            var result = _dbConnector.NonQueries<bool?>((cmds) =>
            {
                cmds.Enqueue((cmd) =>
                {
                    cmd.CommandText = @"
                            INSERT INTO [Production].[Location]
                                ([Name]
                                ,[CostRate])
                            VALUES
                                (@Name
                                ,@CostRate)
                        ";

                    cmd.Parameters.AddFor(new
                    {
                        Name = "9081",
                        CostRate = 5
                    });

                });

                cmds.Enqueue((cmd) =>
                {
                    cmd.CommandText = @"
                            INSERT INTO [Production].[Location]
                                ([Name]
                                ,[CostRate])
                            VALUES
                                (@Name
                                ,@CostRate)
                        ";

                    cmd.Parameters.AddFor(new
                    {
                        Name = "9091",
                        CostRate = 6
                    });

                });

            }).OnCompleted(d => true).Execute();

            Assert.NotNull(result);
            Assert.Equal(true, result);

            var deleteResult = _dbConnector.NonQuery((cmd) =>
            {
                cmd.CommandText = @"
                    DELETE FROM [Production].[Location] WHERE Name = @Name OR Name = @Name2;
                ";

                cmd.Parameters.AddFor(new
                {
                    Name = "9081",
                    Name2 = "9091",
                });

            }).Execute();

            Assert.NotNull(deleteResult);
            Assert.Equal(2, deleteResult);
        }
    }
}
