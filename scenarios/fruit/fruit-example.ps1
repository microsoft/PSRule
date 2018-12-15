
# Define objects
$items = @();
$items += [PSCustomObject]@{ Name = 'Fridge' };
$items += [PSCustomObject]@{ Name = 'Apple' };

# Validate each item using rules saved in current working path
$items | Invoke-PSRule;

# Only show non-fruit results
$items | Invoke-PSRule -Outcome Fail;

# Show rule summary
$items | Invoke-PSRule -As Summary;
