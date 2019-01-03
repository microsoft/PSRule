``` ini

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17763.195 (1809/October2018Update/Redstone5)
Intel Core i7-6600U CPU 2.60GHz (Skylake), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=2.2.100
  [Host]     : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT


```
|        Method |       Mean |     Error |    StdDev | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|-------------- |-----------:|----------:|----------:|------------:|------------:|------------:|--------------------:|
|        Invoke | 117.257 ms | 2.1959 ms | 2.1567 ms |   8400.0000 |    400.0000 |           - |         17355.83 KB |
|      InvokeIf | 128.418 ms | 3.0122 ms | 3.8095 ms |   9750.0000 |    500.0000 |           - |         20301.73 KB |
| InvokeSummary | 116.479 ms | 1.9241 ms | 1.7998 ms |   8400.0000 |           - |           - |         17301.03 KB |
|           Get |   8.921 ms | 0.0864 ms | 0.0766 ms |     93.7500 |           - |           - |           203.82 KB |
