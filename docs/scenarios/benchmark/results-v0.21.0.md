``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
Intel Core i7-1065G7 CPU 1.30GHz, 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.403
  [Host]     : .NET Core 3.1.9 (CoreCLR 4.700.20.47201, CoreFX 4.700.20.47203), X64 RyuJIT
  DefaultJob : .NET Core 3.1.9 (CoreCLR 4.700.20.47201, CoreFX 4.700.20.47203), X64 RyuJIT


```
|                   Method |         Mean |       Error |      StdDev |      Gen 0 |    Gen 1 | Gen 2 |   Allocated |
|------------------------- |-------------:|------------:|------------:|-----------:|---------:|------:|------------:|
|                   Invoke |  41,409.3 μs |   743.11 μs | 1,089.24 μs |  3916.6667 | 500.0000 |     - | 16124.02 KB |
|                 InvokeIf |  43,138.3 μs |   510.44 μs |   426.24 μs |  4416.6667 |  83.3333 |     - | 18374.86 KB |
|               InvokeType |  41,511.3 μs |   703.93 μs |   963.55 μs |  3923.0769 | 230.7692 |     - | 16144.62 KB |
|            InvokeSummary |  40,319.9 μs |   795.95 μs |   705.59 μs |  3900.0000 | 500.0000 |     - | 16124.26 KB |
|                      Get |   9,873.7 μs |   392.08 μs | 1,149.89 μs |    46.8750 |        - |     - |   253.44 KB |
|                  GetHelp |   9,943.1 μs |   406.36 μs | 1,198.17 μs |    46.8750 |        - |     - |   251.84 KB |
|                   Within |  76,627.6 μs | 1,527.91 μs | 1,759.54 μs |  7800.0000 |        - |     - | 32460.47 KB |
|               WithinBulk | 115,374.0 μs | 2,279.41 μs | 3,269.07 μs | 14333.3333 |        - |     - | 59488.54 KB |
|               WithinLike | 102,684.3 μs | 1,482.11 μs | 1,313.85 μs | 11500.0000 | 750.0000 |     - |  46983.1 KB |
| DefaultTargetNameBinding |     673.8 μs |     4.27 μs |     3.79 μs |    38.0859 |        - |     - |   156.25 KB |
|  CustomTargetNameBinding |     888.9 μs |    15.31 μs |    12.78 μs |    85.9375 |        - |     - |   351.56 KB |
|  NestedTargetNameBinding |     901.3 μs |     9.04 μs |     8.01 μs |    85.9375 |        - |     - |   351.56 KB |
