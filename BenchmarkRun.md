``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19043.2130/21H1/May2021Update)
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=6.0.402
  [Host]   : .NET 6.0.0 (6.0.21.35212), X64 RyuJIT AVX2
  ShortRun : .NET 6.0.10 (6.0.1022.47605), X64 RyuJIT AVX2


```
|         ORM |                                            Method |  Return |      Mean |   StdDev |    Error |   Gen0 |   Gen1 |   Gen2 | Allocated |
|------------ |-------------------------------------------------- |-------- |----------:|---------:|---------:|-------:|-------:|-------:|----------:|
|  Hand Coded |                                        SqlCommand |    Post |  45.98 μs | 0.048 μs | 0.092 μs | 1.1875 | 0.5625 |      - |  10.09 KB |
|      Dapper |                            QueryFirstOrDefault&lt;T&gt; |    Post |  48.02 μs | 0.677 μs | 1.138 μs | 1.3750 | 0.6875 |      - |  11.34 KB |
|      Dapper |                       &#39;Query&lt;dynamic&gt; (buffered)&#39; | dynamic |  48.05 μs | 0.190 μs | 0.287 μs | 1.3750 | 0.6875 |      - |   11.7 KB |
|      Dapper |                      QueryFirstOrDefault&lt;dynamic&gt; | dynamic |  48.15 μs | 1.286 μs | 2.161 μs | 1.3750 |      - |      - |  11.38 KB |
| DbConnector |                             ReadFirstOrDefault&lt;T&gt; |    Post |  48.52 μs | 0.144 μs | 0.218 μs | 1.4375 | 0.6875 |      - |  11.83 KB |
| DbConnector |                  &#39;ReadFirstOrDefault&lt;T&gt; (cached)&#39; |    Post |  48.67 μs | 0.295 μs | 0.446 μs | 1.3750 | 0.6875 |      - |  11.55 KB |
| DbConnector |                              &#39;Read&lt;T&gt; (buffered)&#39; |    Post |  48.71 μs | 0.111 μs | 0.186 μs | 1.4375 | 0.6875 |      - |  11.92 KB |
|      Dapper |                             &#39;Query&lt;T&gt; (buffered)&#39; |    Post |  49.01 μs | 0.607 μs | 0.918 μs | 1.3750 | 0.6875 |      - |  11.63 KB |
|      Dapper |                                  &#39;Contrib Get&lt;T&gt;&#39; |    Post |  49.18 μs | 0.239 μs | 0.361 μs | 1.5000 | 0.7500 |      - |  12.27 KB |
| DbConnector |                            &#39;Read&lt;T&gt; (unbuffered)&#39; |    Post |  50.40 μs | 0.143 μs | 0.241 μs | 1.5000 | 0.7500 |      - |  12.27 KB |
| DbConnector |                       ReadFirstOrDefault&lt;dynamic&gt; | dynamic |  50.70 μs | 0.197 μs | 0.376 μs | 1.5000 | 0.7500 |      - |  12.58 KB |
| DbConnector |                        &#39;Read&lt;dynamic&gt; (buffered)&#39; | dynamic |  51.04 μs | 0.277 μs | 0.529 μs | 1.5000 | 0.7500 |      - |  12.59 KB |
| DbConnector |           &#39;ReadAsAsyncEnumerable&lt;T&gt; (unbuffered)&#39; |  Task`1 |  51.78 μs | 0.876 μs | 1.473 μs | 1.5000 | 0.7500 |      - |  12.63 KB |
|  Hand Coded |                                         DataTable | dynamic |  52.98 μs | 2.163 μs | 3.270 μs | 1.2500 | 0.5625 |      - |  10.31 KB |
| DbConnector |                      &#39;Read&lt;dynamic&gt; (unbuffered)&#39; | dynamic |  54.87 μs | 2.794 μs | 4.224 μs | 1.5625 | 0.7500 |      - |   12.9 KB |
|     EF Core |                                &#39;First (Compiled)&#39; |    Post |  63.72 μs | 0.875 μs | 1.470 μs | 0.8750 | 0.1250 |      - |   7.44 KB |
|      Dapper |        &#39;QueryFirstOrDefault&lt;T&gt; (auto connection)&#39; |    Post |  65.96 μs | 0.299 μs | 0.452 μs | 1.5000 | 0.7500 |      - |  12.63 KB |
|      Dapper |                     &#39;Query&lt;dynamic&gt; (unbuffered)&#39; | dynamic |  67.59 μs | 0.556 μs | 0.840 μs | 1.3750 | 0.6250 |      - |  11.78 KB |
|      Dapper |                           &#39;Query&lt;T&gt; (unbuffered)&#39; |    Post |  67.68 μs | 0.201 μs | 0.304 μs | 1.3750 | 0.6250 |      - |  11.74 KB |
| DbConnector |         &#39;ReadFirstOrDefault&lt;T&gt; (auto connection)&#39; |    Post |  68.03 μs | 0.064 μs | 0.122 μs | 1.5000 | 0.7500 |      - |  13.12 KB |
| DbConnector | &#39;ReadFirstOrDefault&lt;T&gt; (cached)(auto connection)&#39; |    Post |  69.13 μs | 3.032 μs | 4.583 μs | 1.5000 | 0.7500 |      - |  12.84 KB |
|     EF Core |                                             First |    Post |  94.84 μs | 0.521 μs | 0.876 μs | 1.2500 |      - |      - |  11.87 KB |
|     EF Core |                                          SqlQuery |    Post | 106.67 μs | 0.789 μs | 1.325 μs | 2.0000 |      - |      - |  18.13 KB |
|     EF Core |                             &#39;First (No Tracking)&#39; |    Post | 106.79 μs | 0.219 μs | 0.368 μs | 2.2500 | 1.1250 | 0.1250 |  18.99 KB |
