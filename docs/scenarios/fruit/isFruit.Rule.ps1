# Description: An example rule
Rule 'isFruit' {
    # An additional message to display in output
    Hint 'Fruit is only Apple, Orange and Pear'

    # Condition to determine if the object is fruit
    $TargetObject.Name -in 'Apple', 'Orange', 'Pear'
}
