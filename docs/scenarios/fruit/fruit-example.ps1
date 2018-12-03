
# Define objects
$items = @();
$items += [PSCustomObject]@{ Name = 'Fridge' };
$items += [PSCustomObject]@{ Name = 'Apple' };

# Validate each item using rules saved in current working path
# Results can be filtered with -Status Failed to return only non-fruit results
$items | Invoke-PSRule;
