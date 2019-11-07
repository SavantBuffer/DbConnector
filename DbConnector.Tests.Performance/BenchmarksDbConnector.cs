using BenchmarkDotNet.Attributes;
using DbConnector.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;

namespace DbConnector.Tests.Performance
{
    [Description("DbConnector")]
    public class BenchmarksDbConnector : BenchmarksBase
    {
        [GlobalSetup]
        public void Setup()
        {
            BaseSetup();
            DbConnectorBaseSetup();
        }

        [Benchmark(Description = "Read<T> (buffered)")]
        public Post ReadBuffered()
        {
            Step();
            return _dbConnector.Read<Post>(
                (cmd) =>
                {
                    cmd.CommandText = "select * from Post where Id = @Id";
                    cmd.Parameters.AddFor(new { Id = i });//cmd.Parameters.AddWithValue("Id", i);
                })
                .Execute(_connection)
                .First();
        }

        [Benchmark(Description = "Read<dynamic> (buffered)")]
        public dynamic ReadBufferedDynamic()
        {
            Step();
            return _dbConnector.Read((cmd) => { cmd.CommandText = "select * from Post where Id = @Id"; cmd.Parameters.AddFor(new { Id = i }); })
                .Execute(_connection)
                .First();
        }

        [Benchmark(Description = "Read<T> (unbuffered)")]
        public Post ReadUnbuffered()
        {
            Step();
            return _dbConnector.Read<Post>((cmd) => { cmd.CommandText = "select * from Post where Id = @Id"; cmd.Parameters.AddFor(new { Id = i }); })
                .WithBuffering(false)
                .Execute(_connection)
                .First();
        }

        [Benchmark(Description = "Read<dynamic> (unbuffered)")]
        public dynamic ReadUnbufferedDynamic()
        {
            Step();
            return _dbConnector.Read((cmd) => { cmd.CommandText = "select * from Post where Id = @Id"; cmd.Parameters.AddFor(new { Id = i }); })        //        
                .WithBuffering(false)
                .Execute(_connection)
                .First();
        }

        [Benchmark(Description = "ReadFirstOrDefault<T>")]
        public Post ReadFirstOrDefault()
        {
            Step();
            return _dbConnector.ReadFirstOrDefault<Post>((cmd) => { cmd.CommandText = "select * from Post where Id = @Id"; cmd.Parameters.AddFor(new { Id = i }); })
                  .Execute(_connection);
        }

        [Benchmark(Description = "ReadFirstOrDefault<dynamic>")]
        public dynamic ReadFirstOrDefaultDynamic()
        {
            Step();
            return _dbConnector.ReadFirstOrDefault((cmd) => { cmd.CommandText = "select * from Post where Id = @Id"; cmd.Parameters.AddFor(new { Id = i }); })
                .Execute(_connection);
        }

        private static readonly IDbJobCacheable<Post, int> _readFirstOrDefault =
            new DbConnector<SqlConnection>(ConnectionString)
            .ReadFirstOrDefault<Post>(
                (IDbJobCommand cmd) =>
                {
                    cmd.CommandText = "select * from Post where Id = @Id";
                    cmd.Parameters.AddFor(new { Id = (cmd as IDbJobCommand<int>).StateParam });
                }
            ).ToCacheable<int>();

        [Benchmark(Description = "ReadFirstOrDefault<T> (cached)")]
        public Post ReadFirstOrDefaultCached()
        {
            Step();
            return _readFirstOrDefault
                  .Execute(i, _connection);
        }

        [Benchmark(Description = "ReadFirstOrDefault<T> (cached)(auto connection)")]
        public Post ReadFirstOrDefaultCachedAutoConnection()
        {
            Step();
            return _readFirstOrDefault
                  .Execute(i);
        }

        [Benchmark(Description = "ReadFirstOrDefault<T> (auto connection)")]
        public Post ReadFirstOrDefaultAutoConnection()
        {
            Step();
            return _dbConnector.ReadFirstOrDefault<Post>((cmd) => { cmd.CommandText = "select * from Post where Id = @Id"; cmd.Parameters.AddFor(new { Id = i }); })
                  .Execute();
        }

        //[Benchmark(Description = "ReadToListOfDictionaries")]
        //public List<Dictionary<string, object>> ReadToListOfDictionaries()
        //{
        //    Step();
        //    return _dbConnector.ReadToListOfDictionaries((cmd) => { cmd.CommandText = "select * from Post where Id = @Id"; cmd.Parameters.AddFor(new { Id = i }); })
        //         .Execute(_connection);
        //}

        //[Benchmark(Description = "ReadToListOfKeyValuePairs")]
        //public List<List<KeyValuePair<string, object>>> ReadToListOfKeyValuePairs()
        //{
        //    Step();
        //    return _dbConnector.ReadToListOfKeyValuePairs((cmd) => { cmd.CommandText = "select * from Post where Id = @Id"; cmd.Parameters.AddFor(new { Id = i }); })
        //         .Execute(_connection);
        //}

        //[Benchmark(Description = "Read<T> (buffered)(auto connection)")]
        //public Post ReadBufferedAutoConnection()
        //{
        //    Step();
        //    return _dbConnector.Read<Post>((cmd) => { cmd.CommandText = "select * from Post where Id = @Id"; cmd.Parameters.AddFor(new { Id = i }); })
        //        .Execute()
        //        .First();
        //}

        //[Benchmark(Description = "MultiRead<T,T,T,T,T,T,T,T> (buffered)")]
        //public Post MultiReadBuffered()
        //{
        //    Step();
        //    return _dbConnector.Read<Post, Post, Post, Post, Post, Post, Post, Post>(
        //        () =>
        //        {
        //            void OnInit1(IDbJobCommand cmd)
        //            {

        //                cmd.CommandText = "select * from Post where Id = @Id";
        //                cmd.Parameters.AddFor(new { Id = i });
        //            }
        //            void OnInit2(IDbJobCommand cmd)
        //            {

        //                cmd.CommandText = "select * from Post where Id = @Id";
        //                cmd.Parameters.AddFor(new { Id = i });
        //            }
        //            void OnInit3(IDbJobCommand cmd)
        //            {

        //                cmd.CommandText = "select * from Post where Id = @Id";
        //                cmd.Parameters.AddFor(new { Id = i });
        //            }
        //            void OnInit4(IDbJobCommand cmd)
        //            {

        //                cmd.CommandText = "select * from Post where Id = @Id";
        //                cmd.Parameters.AddFor(new { Id = i });
        //            }
        //            void OnInit5(IDbJobCommand cmd)
        //            {

        //                cmd.CommandText = "select * from Post where Id = @Id";
        //                cmd.Parameters.AddFor(new { Id = i });
        //            }
        //            void OnInit6(IDbJobCommand cmd)
        //            {

        //                cmd.CommandText = "select * from Post where Id = @Id";
        //                cmd.Parameters.AddFor(new { Id = i });
        //            }
        //            void OnInit7(IDbJobCommand cmd)
        //            {

        //                cmd.CommandText = "select * from Post where Id = @Id";
        //                cmd.Parameters.AddFor(new { Id = i });
        //            }
        //            void OnInit8(IDbJobCommand cmd)
        //            {

        //                cmd.CommandText = "select * from Post where Id = @Id";
        //                cmd.Parameters.AddFor(new { Id = i });
        //            }

        //            return (OnInit1, OnInit2, OnInit3, OnInit4, OnInit5, OnInit6, OnInit7, OnInit8);
        //        })
        //        .Execute().Item1.First();
        //}
    }
}
