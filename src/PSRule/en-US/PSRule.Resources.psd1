# Copyright (c) Bernie White. All rights reserved.

ConvertFrom-StringData @'
###PSLOC
PathNotFound=Path not found
YamlContainsComments=The YAML options file contains comments. Comments will be lost if you choose to continue. To overwrite comments use the -AllowClobber switch.
KeywordOutsideEngine=This keyword can only be called within PSRule. To call rules use Invoke-PSRule.
###PSLOC
'@
