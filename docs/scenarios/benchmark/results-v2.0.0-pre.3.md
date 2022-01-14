``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
Intel Core i7-1065G7 CPU 1.30GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.404
  [Host]     : .NET Core 3.1.22 (CoreCLR 4.700.21.56803, CoreFX 4.700.21.57101), X64 RyuJIT
  DefaultJob : .NET Core 3.1.22 (CoreCLR 4.700.21.56803, CoreFX 4.700.21.57101), X64 RyuJIT


```
|                   Method |             Mean |           Error |          StdDev |      Gen 0 |     Gen 1 | Allocated |
|------------------------- |-----------------:|----------------:|----------------:|-----------:|----------:|----------:|
|                   Invoke |  53,201,973.3 ns |   965,400.91 ns |   903,036.61 ns |  4100.0000 |  400.0000 | 17,758 KB |
|                 InvokeIf |  54,266,264.0 ns |   516,824.36 ns |   483,437.83 ns |  4500.0000 |  200.0000 | 20,008 KB |
|               InvokeType |  53,299,662.9 ns | 1,041,990.81 ns | 1,240,415.94 ns |  4000.0000 |  400.0000 | 17,758 KB |
|            InvokeSummary |  52,364,196.7 ns |   741,213.07 ns |   693,331.17 ns |  4100.0000 |  400.0000 | 17,758 KB |
|                   Assert |  53,926,665.9 ns | 1,048,995.55 ns |   929,907.24 ns |  4111.1111 |  222.2222 | 18,461 KB |
|                      Get |   6,213,169.1 ns |   119,598.34 ns |   117,461.56 ns |    85.9375 |    7.8125 |    366 KB |
|                  GetHelp |   6,185,384.7 ns |   121,492.72 ns |   144,628.44 ns |    85.9375 |    7.8125 |    366 KB |
|                   Within |  91,044,076.7 ns | 1,250,486.23 ns | 1,169,705.59 ns |  8250.0000 | 1000.0000 | 34,198 KB |
|               WithinBulk | 131,636,590.3 ns | 2,585,126.29 ns | 3,361,394.39 ns | 14666.6667 | 1666.6667 | 61,224 KB |
|               WithinLike | 117,834,564.4 ns | 2,208,102.68 ns | 2,065,460.62 ns | 11666.6667 | 1666.6667 | 48,352 KB |
| DefaultTargetNameBinding |     696,752.1 ns |     9,049.16 ns |     8,021.84 ns |    38.0859 |         - |    156 KB |
|  CustomTargetNameBinding |     881,022.4 ns |    12,940.79 ns |    11,471.67 ns |    85.9375 |         - |    352 KB |
|  NestedTargetNameBinding |     876,321.2 ns |     7,650.01 ns |     6,781.54 ns |    85.9375 |         - |    352 KB |
|      AssertHasFieldValue |   3,076,694.5 ns |    30,768.35 ns |    25,692.97 ns |   253.9063 |    7.8125 |  1,040 KB |
|             PathTokenize |         869.1 ns |        16.88 ns |        25.27 ns |     0.2632 |         - |      1 KB |
|      PathExpressionBuild |         529.8 ns |         8.39 ns |         7.44 ns |     0.3500 |         - |      1 KB |
|        PathExpressionGet |     358,678.4 ns |     4,814.17 ns |     4,020.05 ns |    17.0898 |         - |     70 KB |
