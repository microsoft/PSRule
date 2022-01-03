
# Synopsis: An example rule to require TLS.
Rule 'Local.PS.RequireTLS' {
    $Assert.HasFieldValue($TargetObject, 'configure.supportsHttpsTrafficOnly', $True)
    $Assert.HasFieldValue($TargetObject, 'configure.minTLSVersion', '1.2')
}
