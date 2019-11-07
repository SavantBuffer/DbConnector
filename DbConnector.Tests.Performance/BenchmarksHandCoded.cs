using BenchmarkDotNet.Attributes;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;

namespace DbConnector.Tests.Performance
{
    public static class SqlDataReaderHelper
    {
        public static string GetNullableString(this SqlDataReader reader, int index)
        {
            object tmp = reader.GetValue(index);
            if (tmp != DBNull.Value)
            {
                return (string)tmp;
            }
            return null;
        }

        public static T? GetNullableValue<T>(this SqlDataReader reader, int index) where T : struct
        {
            object tmp = reader.GetValue(index);
            if (tmp != DBNull.Value)
            {
                return (T)tmp;
            }
            return null;
        }
    }

    [Description("Hand Coded")]
    public class BenchmarksHandCoded : BenchmarksBase
    {
        private SqlCommand _postCommand;
        private SqlParameter _idParam;
#if !NETCOREAPP1_0
        private DataTable _table;
#endif

        [GlobalSetup]
        public void Setup()
        {
            BaseSetup();
            _postCommand = new SqlCommand()
            {
                Connection = _connection,
                CommandText = @"select Id, [Text], [CreationDate], LastChangeDate, 
                Counter1,Counter2,Counter3,Counter4,Counter5,Counter6,Counter7,Counter8,Counter9 from Post where Id = @Id"
            };
            _idParam = _postCommand.Parameters.Add("@Id", SqlDbType.Int);
#if !NETCOREAPP1_0
            _table = new DataTable
            {
                Columns =
                    {
                        {"Id", typeof (int)},
                        {"Text", typeof (string)},
                        {"CreationDate", typeof (DateTime)},
                        {"LastChangeDate", typeof (DateTime)},
                        {"Counter1", typeof (int)},
                        {"Counter2", typeof (int)},
                        {"Counter3", typeof (int)},
                        {"Counter4", typeof (int)},
                        {"Counter5", typeof (int)},
                        {"Counter6", typeof (int)},
                        {"Counter7", typeof (int)},
                        {"Counter8", typeof (int)},
                        {"Counter9", typeof (int)},
                    }
            };
#endif
        }

        [Benchmark(Description = "SqlCommand")]
        public Post SqlCommand()
        {
            Step();
            _idParam.Value = i;

            using (var reader = _postCommand.ExecuteReader())
            {
                reader.Read();
                return new Post
                {
                    Id = reader.GetInt32(0),
                    Text = reader.GetNullableString(1),
                    CreationDate = reader.GetDateTime(2),
                    LastChangeDate = reader.GetDateTime(3),

                    Counter1 = reader.IsDBNull(4) ? null : (int?)reader.GetInt32(4),
                    Counter2 = reader.IsDBNull(5) ? null : (int?)reader.GetInt32(5),
                    Counter3 = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                    Counter4 = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7),
                    Counter5 = reader.IsDBNull(8) ? null : (int?)reader.GetInt32(8),
                    Counter6 = reader.IsDBNull(9) ? null : (int?)reader.GetInt32(9),
                    Counter7 = reader.IsDBNull(10) ? null : (int?)reader.GetInt32(10),
                    Counter8 = reader.IsDBNull(11) ? null : (int?)reader.GetInt32(11),
                    Counter9 = reader.IsDBNull(12) ? null : (int?)reader.GetInt32(12)
                };
            }
        }

        [Benchmark(Description = "DataTable")]
        public dynamic DataTableDynamic()
        {
            Step();
            _idParam.Value = i;
            var values = new object[13];
            using (var reader = _postCommand.ExecuteReader())
            {
                reader.Read();
                reader.GetValues(values);
                _table.Rows.Add(values);
                return _table.Rows[_table.Rows.Count - 1];
            }
        }
    }
}
