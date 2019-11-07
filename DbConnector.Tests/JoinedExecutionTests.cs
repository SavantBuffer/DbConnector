using Xunit;
using DbConnector.Core;
using System.Linq;
using System;

namespace DbConnector.Tests
{
    public class JoinedExecutionTests : TestBase
    {
        [Fact]
        public void JoinedExecution()
        {
            IDbJob<int?> jobInsert = _dbConnector.NonQuery((cmd) =>
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
                    Name = "9100",
                    CostRate = 5
                });

            });

            IDbJob<int?> jobDelete = _dbConnector.NonQuery((cmd) =>
            {
                cmd.CommandText = @"
                    DELETE FROM [Production].[Location] WHERE Name = @Name
                ";

                cmd.Parameters.AddFor(new
                {
                    Name = "9100",
                    CostRate = 5
                });

            });

            var result = DbJob.ExecuteAll(jobInsert, jobDelete);

            Assert.NotNull(result);
            Assert.Equal(1, result[0].Item2);
            Assert.Equal(1, result[1].Item2);
        }

        [Fact]
        public void JoinedExecution_Exception()
        {
            Assert.Throws<Exception>(() =>
            {
                IDbJob<int?> jobInsert = _dbConnector.NonQuery((cmd) =>
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
                            Name = "9200",
                            CostRate = 5
                        });

                    });

                IDbJob<int?> jobDelete = _dbConnector.NonQuery((cmd) =>
                {
                    cmd.CommandText = @"
                        DELETE FROM [Production].[Location] WHERE Name = @Name
                    ";

                    cmd.Parameters.AddFor(new
                    {
                        Name = "9200",
                        CostRate = 5
                    });

                    throw new System.Exception("Test Exception!");

                });

                var result = DbJob.ExecuteAll(jobInsert, jobDelete);
            });
        }

        [Fact]
        public void JoinedExecution_Handled()
        {
            IDbJob<int?> jobInsert = _dbConnector.NonQuery((cmd) =>
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
                    Name = "9300",
                    CostRate = 5
                });

            });

            IDbJob<int?> jobDelete = _dbConnector.NonQuery((cmd) =>
            {
                cmd.CommandText = @"
                    DELETE FROM [Production].[Location] WHERE Name = @Name
                ";

                cmd.Parameters.AddFor(new
                {
                    Name = "9300",
                    CostRate = 5
                });

            });

            var result = DbJob.ExecuteAllHandled(jobInsert, jobDelete);

            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data[0].Item2);
            Assert.Equal(1, result.Data[1].Item2);
        }

        [Fact]
        public void JoinedExecution_Handled_ExceptionThrow()
        {
            IDbJob<int?> jobInsert = _dbConnector.NonQuery((cmd) =>
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
                    Name = "9400",
                    CostRate = 5
                });

            });

            IDbJob<int?> jobDelete = _dbConnector.NonQuery((cmd) =>
            {
                cmd.CommandText = @"
                    DELETE FROM [Production].[Location] WHERE Name = @Name
                ";

                cmd.Parameters.AddFor(new
                {
                    Name = "9400",
                    CostRate = 5
                });

                throw new System.Exception("Test Exception!");

            });

            var result = DbJob.ExecuteAllHandled(jobInsert, jobDelete);

            Assert.NotNull(result);
            Assert.Equal("Test Exception!", result.Error.Message);
        }
    }
}
