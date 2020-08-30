``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.450 (2004/?/20H1)
Intel Core i7-1065G7 CPU 1.30GHz, 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.401
  [Host]     : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
  DefaultJob : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT


```
|                   Method |         Mean |       Error |      StdDev |      Gen 0 |    Gen 1 | Gen 2 |   Allocated |
|------------------------- |-------------:|------------:|------------:|-----------:|---------:|------:|------------:|
|                   Invoke |  40,943.5 μs |   581.23 μs |   515.25 μs |  4000.0000 | 500.0000 |     - | 16452.28 KB |
|                 InvokeIf |  42,806.0 μs |   477.29 μs |   423.11 μs |  4500.0000 | 500.0000 |     - | 18703.12 KB |
|               InvokeType |  40,470.1 μs |   484.16 μs |   429.19 μs |  4000.0000 | 538.4615 |     - | 16452.27 KB |
|            InvokeSummary |  39,768.8 μs |   462.14 μs |   385.91 μs |  4000.0000 | 153.8462 |     - | 16397.82 KB |
|                      Get |  11,145.4 μs |   402.59 μs | 1,187.03 μs |    46.8750 |        - |     - |   252.11 KB |
|                  GetHelp |  10,169.1 μs |   625.02 μs | 1,842.88 μs |    46.8750 |        - |     - |   250.51 KB |
|                   Within |  78,993.5 μs |   799.51 μs |   667.63 μs |  8000.0000 | 400.0000 |     - | 32791.83 KB |
|               WithinBulk | 118,800.8 μs | 1,637.36 μs | 1,531.59 μs | 14333.3333 | 333.3333 |     - | 59817.29 KB |
|               WithinLike | 106,796.3 μs | 2,067.20 μs | 2,538.71 μs | 11333.3333 |        - |     - | 47311.07 KB |
| DefaultTargetNameBinding |     698.2 μs |     7.51 μs |     7.02 μs |    38.0859 |        - |     - |   156.25 KB |
|  CustomTargetNameBinding |     884.7 μs |     7.11 μs |     6.65 μs |    85.9375 |        - |     - |   351.56 KB |
|  NestedTargetNameBinding |     883.9 μs |    14.44 μs |    12.80 μs |    85.9375 |        - |     - |   351.56 KB |
