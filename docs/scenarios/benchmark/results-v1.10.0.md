``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
Intel Core i7-1065G7 CPU 1.30GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.404
  [Host]     : .NET Core 3.1.22 (CoreCLR 4.700.21.56803, CoreFX 4.700.21.57101), X64 RyuJIT
  DefaultJob : .NET Core 3.1.22 (CoreCLR 4.700.21.56803, CoreFX 4.700.21.57101), X64 RyuJIT


```
|                   Method |         Mean |       Error |      StdDev |      Gen 0 |     Gen 1 | Allocated |
|------------------------- |-------------:|------------:|------------:|-----------:|----------:|----------:|
|                   Invoke |  50,742.5 μs |   908.47 μs |   709.27 μs |  4100.0000 |  400.0000 | 17,758 KB |
|                 InvokeIf |  53,048.6 μs |   698.34 μs |   619.06 μs |  4500.0000 |  200.0000 | 20,008 KB |
|               InvokeType |  50,575.6 μs |   794.27 μs |   663.25 μs |  4000.0000 |  200.0000 | 17,760 KB |
|            InvokeSummary |  50,449.0 μs |   698.80 μs |   619.47 μs |  4100.0000 |  400.0000 | 17,758 KB |
|                   Assert |  52,152.6 μs |   765.95 μs |   678.99 μs |  4200.0000 |  300.0000 | 18,462 KB |
|                      Get |   5,793.8 μs |    86.70 μs |    81.10 μs |    78.1250 |         - |    364 KB |
|                  GetHelp |   5,799.6 μs |    76.72 μs |    71.77 μs |    85.9375 |    7.8125 |    364 KB |
|                   Within |  89,538.2 μs | 1,754.26 μs | 1,555.11 μs |  8000.0000 | 1000.0000 | 34,102 KB |
|               WithinBulk | 128,126.9 μs | 1,928.80 μs | 1,709.83 μs | 14666.6667 | 1333.3333 | 61,131 KB |
|               WithinLike | 112,174.1 μs | 1,132.30 μs | 1,003.76 μs | 11666.6667 | 1666.6667 | 48,258 KB |
| DefaultTargetNameBinding |     695.6 μs |    13.57 μs |    14.52 μs |    38.0859 |         - |    156 KB |
|  CustomTargetNameBinding |     851.0 μs |    10.35 μs |     8.64 μs |    85.9375 |         - |    352 KB |
|  NestedTargetNameBinding |     961.5 μs |    17.83 μs |    15.80 μs |    85.9375 |         - |    352 KB |
|      AssertHasFieldValue |   3,033.5 μs |    60.15 μs |    66.85 μs |   253.9063 |    7.8125 |  1,040 KB |
