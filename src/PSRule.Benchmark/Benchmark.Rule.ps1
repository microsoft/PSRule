# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# A set of benchmark rules for testing PSRule performance
#

# Synopsis: A rule for testing PSRule performance
Rule 'Benchmark' {
    1 -eq 1;
}

# Synopsis: A rule for testing PSRule performance
Rule 'BenchmarkIf' -If { 1 -eq 1 } {
    1 -eq 1;
}

Rule 'BenchmarkType' -Type 'PSRule.Benchmark.TargetObject' {
    1 -eq 1;
}

# Synopsis: A rule for testing PSRule performance
Rule 'BenchmarkHelp' {
    1 -eq 1;
}

# Synopsis: A rule for testing PSRule performance
Rule 'BenchmarkExists' {

}
