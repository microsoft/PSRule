``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
Intel Core i7-1065G7 CPU 1.30GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.404
  [Host]     : .NET Core 3.1.22 (CoreCLR 4.700.21.56803, CoreFX 4.700.21.57101), X64 RyuJIT
  DefaultJob : .NET Core 3.1.22 (CoreCLR 4.700.21.56803, CoreFX 4.700.21.57101), X64 RyuJIT


```
|                   Method |         Mean |       Error |      StdDev |      Gen 0 |     Gen 1 | Allocated |
|------------------------- |-------------:|------------:|------------:|-----------:|----------:|----------:|
|                   Invoke |  50,529.4 μs | 1,006.40 μs |   941.38 μs |  4000.0000 |  444.4444 | 17,758 KB |
|                 InvokeIf |  51,974.4 μs |   667.26 μs |   591.51 μs |  4500.0000 |  200.0000 | 20,008 KB |
|               InvokeType |  49,901.2 μs |   679.83 μs |   567.69 μs |  4000.0000 |  363.6364 | 17,758 KB |
|            InvokeSummary |  51,198.9 μs |   862.22 μs |   922.57 μs |  4000.0000 |  363.6364 | 17,758 KB |
|                   Assert |  52,136.6 μs |   588.93 μs |   550.88 μs |  4100.0000 |  300.0000 | 18,461 KB |
|                      Get |   5,710.0 μs |   111.69 μs |   104.47 μs |    85.9375 |    7.8125 |    364 KB |
|                  GetHelp |   5,777.4 μs |    97.83 μs |    91.51 μs |    85.9375 |    7.8125 |    364 KB |
|                   Within |  88,106.3 μs | 1,752.66 μs | 1,799.86 μs |  8000.0000 | 1000.0000 | 34,102 KB |
|               WithinBulk | 125,319.9 μs | 2,303.80 μs | 2,154.98 μs | 14666.6667 | 1000.0000 | 61,133 KB |
|               WithinLike | 115,376.3 μs | 1,866.04 μs | 1,654.20 μs | 11666.6667 | 1666.6667 | 48,258 KB |
| DefaultTargetNameBinding |     669.5 μs |     6.52 μs |     6.10 μs |    38.0859 |         - |    156 KB |
|  CustomTargetNameBinding |     837.6 μs |     6.70 μs |     6.27 μs |    85.9375 |         - |    352 KB |
|  NestedTargetNameBinding |     854.1 μs |     9.50 μs |     7.42 μs |    85.9375 |         - |    352 KB |
|      AssertHasFieldValue |   2,967.0 μs |    38.88 μs |    34.47 μs |   253.9063 |    7.8125 |  1,040 KB |
