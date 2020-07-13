# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

ConvertFrom-StringData @'
###PSLOC
PathNotFound=Path not found
YamlContainsComments=The YAML options file contains comments. Comments will be lost if you choose to continue. To overwrite comments use the -AllowClobber switch.
KeywordOutsideEngine=This keyword can only be called within PSRule. Add rule definitions to .Rule.ps1 files, then execute them with Invoke-PSRule or Test-PSRuleTarget.
###PSLOC
'@
