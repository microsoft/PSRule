# Copyright Bernie White

# Synopsis: Check file includes copyright header
Rule 'file.Header' -If { $TargetObject.Extension -in '.ps1', '.psm1', '.psd1', '.yaml', '.yml' } {
    $fileContent = Get-Content -Path $TargetObject.FullName -Raw;
    $fileContent -match '(# Copyright \(c\) Bernie White\. All rights reserved\.)';
}

# Synopsis: File encoding should be UTF-8
Rule 'file.Encoding' -If { $TargetObject.Extension -in '.ps1', '.psm1', '.psd1', '.yaml', '.yml' } {
    try {
        $reader = New-Object -TypeName System.IO.StreamReader -ArgumentList @($TargetObject.FullName, [System.Text.Encoding]::UTF8, $True);
        $Null = $reader.Peek();
        $reader.CurrentEncoding -eq [System.Text.Encoding]::UTF8;
    }
    finally {
        $reader.Close();
    }
}
