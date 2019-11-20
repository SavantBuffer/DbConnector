``` ini

BenchmarkDotNet=v0.11.1, OS=Windows 10.0.18362
Intel Core i7-8750H CPU 2.20GHz (Max: 2.21GHz) (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=2.2.102
  [Host]   : .NET Core ? (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT
  ShortRun : .NET Core 2.1.13 (CoreCLR 4.6.28008.01, CoreFX 4.6.28008.01), 64bit RyuJIT


```
|         ORM |                                            Method |  Return |      Mean |  Gen 0 |  Gen 1 |  Gen 2 | Allocated |
|------------ |-------------------------------------------------- |-------- |----------:|-------:|-------:|-------:|----------:|
|  Hand Coded |                                        SqlCommand |    Post |  64.57 us | 1.7500 | 0.8750 | 0.1250 |   10.2 KB |
|  Hand Coded |                                         DataTable | dynamic |  68.46 us | 1.6250 | 0.5000 |      - |  10.42 KB |
| DbConnector |                             ReadFirstOrDefault&lt;T&gt; |    Post |  72.95 us | 1.8750 | 0.8750 |      - |  11.95 KB |
| DbConnector |                       ReadFirstOrDefault&lt;dynamic&gt; | dynamic |  73.26 us | 2.0000 | 1.0000 |      - |   12.7 KB |
| DbConnector |                              &#39;Read&lt;T&gt; (buffered)&#39; |    Post |  73.91 us | 1.8750 | 0.8750 |      - |  12.05 KB |
|      Dapper |                       &#39;Query&lt;dynamic&gt; (buffered)&#39; | dynamic |  74.93 us | 1.8750 | 0.8750 |      - |  11.84 KB |
| DbConnector |                  &#39;ReadFirstOrDefault&lt;T&gt; (cached)&#39; |    Post |  74.94 us | 1.8750 | 0.8750 |      - |  11.66 KB |
| DbConnector |                            &#39;Read&lt;T&gt; (unbuffered)&#39; |    Post |  75.05 us | 2.1250 | 1.0000 | 0.1250 |  12.45 KB |
|      Dapper |                             &#39;Query&lt;T&gt; (buffered)&#39; |    Post |  77.35 us | 1.8750 | 0.8750 |      - |  11.76 KB |
| DbConnector |                        &#39;Read&lt;dynamic&gt; (buffered)&#39; | dynamic |  77.95 us | 2.0000 | 1.0000 | 0.1250 |  12.72 KB |
| DbConnector |                      &#39;Read&lt;dynamic&gt; (unbuffered)&#39; | dynamic |  80.60 us | 2.1250 | 1.0000 | 0.1250 |  13.08 KB |
|      Dapper |                                  &#39;Contrib Get&lt;T&gt;&#39; |    Post |  80.77 us | 2.6250 | 1.2500 | 0.3750 |  12.41 KB |
|      Dapper |                            QueryFirstOrDefault&lt;T&gt; |    Post |  81.60 us | 1.8750 | 0.8750 | 0.1250 |  11.45 KB |
|      Dapper |                      QueryFirstOrDefault&lt;dynamic&gt; | dynamic |  84.53 us | 2.3750 |      - |      - |  11.49 KB |
|      Dapper |        &#39;QueryFirstOrDefault&lt;T&gt; (auto connection)&#39; |    Post |  98.88 us | 2.2500 | 1.1250 | 0.2500 |  12.88 KB |
|      Dapper |                           &#39;Query&lt;T&gt; (unbuffered)&#39; |    Post |  99.35 us | 1.8750 | 0.8750 |      - |  11.87 KB |
| DbConnector |         &#39;ReadFirstOrDefault&lt;T&gt; (auto connection)&#39; |    Post |  99.66 us | 2.5000 | 1.2500 | 0.5000 |  13.37 KB |
|      Dapper |                     &#39;Query&lt;dynamic&gt; (unbuffered)&#39; | dynamic | 106.84 us | 2.0000 | 1.0000 | 0.1250 |  11.91 KB |
| DbConnector | &#39;ReadFirstOrDefault&lt;T&gt; (cached)(auto connection)&#39; |    Post | 111.63 us | 2.2500 | 1.1250 | 0.2500 |  13.09 KB |
|     EF Core |                                &#39;First (Compiled)&#39; |    Post | 137.56 us | 3.0000 | 0.2500 |      - |  13.98 KB |
|     EF Core |                                             First |    Post | 151.05 us | 3.7500 |      - |      - |  17.97 KB |
|     EF Core |                                          SqlQuery |    Post | 158.25 us | 4.0000 |      - |      - |   19.1 KB |
|     EF Core |                             &#39;First (No Tracking)&#39; |    Post | 187.25 us | 3.5000 | 0.7500 | 0.2500 |  19.84 KB |
