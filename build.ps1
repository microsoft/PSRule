# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Note:
# This manually builds the project locally

. ./scripts/pipeline-deps.ps1
Invoke-Build Test -AssertStyle Client

Write-Host "If no build errors occured. The module has been saved to out/modules/PSRule"
