using BenchmarkDotNet.Running;
using System;
using System.Data.SqlClient;
using static System.Console;

namespace DbConnector.Tests.PerformanceNF
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            //NOTE: Based on Dapper's Test for performance comparison reasons!

#if DEBUG
            WriteLineColor("Warning: DEBUG configuration; performance may be impacted!", ConsoleColor.Red);
            WriteLine();
#endif
            WriteLine("Welcome to DbConnector's ORM performance benchmark test, based on BenchmarkDotNet.");
            Write("  Please report any problems at: ");
            WriteLineColor("https://github.com/SavantBuffer/DbConnector", ConsoleColor.Blue);
            WriteLine();

            WriteLine("Using ConnectionString: " + BenchmarksBase.ConnectionString);
            EnsureDBSetup();
            WriteLine("Database setup complete.");

            WriteLine("Iterations: " + Config.Iterations);
            new BenchmarkSwitcher(typeof(BenchmarksBase).Assembly).Run(args, new Config());
        }


        private static void EnsureDBSetup()
        {
            using (var cnn = new SqlConnection(BenchmarksBase.ConnectionString))
            {
                cnn.Open();
                using (var cmd = cnn.CreateCommand())
                {
                    cmd.CommandText = @"
                        If (Object_Id('Post') Is Null)
                        Begin
	                        Create Table Post
	                        (
		                        Id int identity primary key, 
		                        [Text] varchar(max) not null, 
		                        CreationDate datetime not null, 
		                        LastChangeDate datetime not null,
		                        Counter1 int,
		                        Counter2 int,
		                        Counter3 int,
		                        Counter4 int,
		                        Counter5 int,
		                        Counter6 int,
		                        Counter7 int,
		                        Counter8 int,
		                        Counter9 int
	                        );
	   
	                        Set NoCount On;
	                        Declare @i int = 0;
	                        While @i <= 5001
	                        Begin
		                        Insert Post ([Text],CreationDate, LastChangeDate) values (replicate('x', 2000), GETDATE(), GETDATE());
		                        Set @i = @i + 1;
	                        End
                        End
                    ";
                    cmd.Connection = cnn;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void WriteLineColor(string message, ConsoleColor color)
        {
            var orig = ForegroundColor;
            ForegroundColor = color;
            WriteLine(message);
            ForegroundColor = orig;
        }

        public static void WriteColor(string message, ConsoleColor color)
        {
            var orig = ForegroundColor;
            ForegroundColor = color;
            Write(message);
            ForegroundColor = orig;
        }
    }
}
