``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.450 (2004/?/20H1)
Intel Core i7-1065G7 CPU 1.30GHz, 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.401
  [Host]     : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
  DefaultJob : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT


```
|                   Method |         Mean |       Error |      StdDev |      Gen 0 |     Gen 1 | Gen 2 |   Allocated |
|------------------------- |-------------:|------------:|------------:|-----------:|----------:|------:|------------:|
|                   Invoke |  42,162.8 μs |   827.36 μs | 1,263.47 μs |  3833.3333 |         - |     - |    15952 KB |
|                 InvokeIf |  45,646.4 μs |   912.31 μs | 1,924.38 μs |  4416.6667 |  416.6667 |     - | 18202.98 KB |
|               InvokeType |  41,825.5 μs |   810.73 μs |   901.12 μs |  3833.3333 |         - |     - |    15952 KB |
|            InvokeSummary |  41,133.3 μs |   777.97 μs |   895.91 μs |  3833.3333 |  500.0000 |     - | 15897.56 KB |
|                      Get |  10,054.3 μs |   396.83 μs | 1,170.07 μs |    46.8750 |         - |     - |   252.11 KB |
|                  GetHelp |  10,581.4 μs |   448.15 μs | 1,321.38 μs |    46.8750 |         - |     - |   250.51 KB |
|                   Within |  81,215.1 μs | 1,532.85 μs | 1,433.83 μs |  7750.0000 |  250.0000 |     - | 32290.62 KB |
|               WithinBulk | 123,301.6 μs | 2,451.51 μs | 3,958.73 μs | 14000.0000 | 1000.0000 |     - | 59317.29 KB |
|               WithinLike | 109,738.9 μs | 1,933.95 μs | 1,809.02 μs | 11333.3333 | 1000.0000 |     - | 46811.07 KB |
| DefaultTargetNameBinding |     696.0 μs |    12.06 μs |    10.69 μs |    38.0859 |         - |     - |   156.25 KB |
|  CustomTargetNameBinding |     845.6 μs |    11.75 μs |    10.42 μs |    85.9375 |         - |     - |   351.56 KB |
|  NestedTargetNameBinding |     856.0 μs |    12.29 μs |    10.90 μs |    85.9375 |         - |     - |   351.56 KB |
