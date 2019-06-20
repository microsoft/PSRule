# Copyright (c) Bernie White. All rights reserved.

ConvertFrom-StringData @'
###PSLOC
PathNotFound=Path not found
RulePathNotFound=No matching .Rule.ps1 files were found. Rule definitions should be saved into script files with the .Rule.ps1 extension.
YamlContainsComments=The YAML options file contains comments. Comments will be lost if you choose to continue. To overwrite comments use the -AllowClobber switch.
KeywordOutsideEngine=This keyword can only be called within PSRule. Add rule definitions to .Rule.ps1 files, then execute them with Invoke-PSRule or Test-PSRuleTarget.
###PSLOC
'@
