#
# A set of benchmark rules for testing PSRule performance
#

Rule 'BenchmarkOdd' -If { ($TargetObject.Name % 2) -gt 0 } {
    Hint 'Odd message'
}

Rule 'BenchmarkEven' -If { ($TargetObject.Name % 2) -ge 0 } {
    Hint 'Even message'
}