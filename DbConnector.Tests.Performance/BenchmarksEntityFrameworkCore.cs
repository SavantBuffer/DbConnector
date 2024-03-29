﻿using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.Linq;

namespace DbConnector.Tests.Performance
{
    public class EFCoreContext : DbContext
    {
        private readonly string _connectionString;

        public EFCoreContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer(_connectionString);

        public DbSet<Post> Posts { get; set; }
    }

    [Description("EF Core")]
    public class BenchmarksEntityFrameworkCore : BenchmarksBase
    {
        private EFCoreContext Context;

        private static readonly Func<EFCoreContext, int, Post> compiledQuery =
            EF.CompileQuery((EFCoreContext ctx, int id) => ctx.Posts.First(p => p.Id == id));

        [GlobalSetup]
        public void Setup()
        {
            BaseSetup();
            Context = new EFCoreContext(_connection.ConnectionString);
        }

        [Benchmark(Description = "First")]
        public Post First()
        {
            Step();
            return Context.Posts.First(p => p.Id == i);
        }

        [Benchmark(Description = "First (Compiled)")]
        public Post Compiled()
        {
            Step();
            return compiledQuery(Context, i);
        }

        [Benchmark(Description = "SqlQuery")]
        public Post SqlQuery()
        {
            Step();
            return Context.Posts.FromSqlRaw("select * from Post where Id = {0}", i).First();
        }

        [Benchmark(Description = "First (No Tracking)")]
        public Post NoTracking()
        {
            Step();
            return Context.Posts.AsNoTracking().First(p => p.Id == i);
        }
    }
}
