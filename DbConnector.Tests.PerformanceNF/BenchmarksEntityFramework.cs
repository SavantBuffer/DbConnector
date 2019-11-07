using BenchmarkDotNet.Attributes;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;

namespace DbConnector.Tests.PerformanceNF
{
    public class EFContext : DbContext
    {
        public EFContext(DbConnection connection, bool owned = false) : base(connection, owned)
        {
        }

        public DbSet<Post> Posts { get; set; }
    }

    [Description("EF 6")]
    public class BenchmarksEntityFramework : BenchmarksBase
    {
        private EFContext Context;

        [GlobalSetup]
        public void Setup()
        {
            BaseSetup();
            Context = new EFContext(_connection);
        }

        [Benchmark(Description = "First")]
        public Post First()
        {
            Step();
            return Context.Posts.First(p => p.Id == i);
        }

        [Benchmark(Description = "SqlQuery")]
        public Post SqlQuery()
        {
            Step();
            return Context.Database.SqlQuery<Post>("select * from Post where Id = {0}", i).First();
        }

        [Benchmark(Description = "First (No Tracking)")]
        public Post NoTracking()
        {
            Step();
            return Context.Posts.AsNoTracking().First(p => p.Id == i);
        }
    }
}
