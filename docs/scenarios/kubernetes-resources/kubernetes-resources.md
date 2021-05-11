# Kubernetes resource validation example

This is an example of how PSRule can be used to validate Kubernetes resources to match an internal metadata and configuration standard.

> [!NOTE]
> A pre-built module to validate Kubernetes resources already exists.
> This scenario demonstrates the process and features of PSRule for illustration purposes.
>
> Consider using or contributing these pre-built rule modules instead:
>
> - [PSRule.Rules.Kubernetes]

This scenario covers the following:

- Defining a basic rule.
- Configuring custom binding.
- Using a type precondition.
- Running rules using YAML input.

In this scenario we will use a YAML file:

- [`resources.yaml`](resources.yaml) - A Kubernetes manifest containing deployments and services.

## Define rules

To validate our Kubernetes resources, we need to define some rules.
Rules are defined by using the `Rule` keyword in a file ending with the `.Rule.ps1` extension.

Our business rules for configuration Kubernetes resources can be defined with the following dot points:

- The following [recommended](https://kubernetes.io/docs/concepts/overview/working-with-objects/common-labels/) labels will be used on all services and deployments:
  - `app.kubernetes.io/name` - the name of the application/ service.
  - `app.kubernetes.io/version` - the version of the service.
  - `app.kubernetes.io/component` - identifies the type of component, valid options are `web`, `api`, `database` and `gateway`
- For `web` or `api` deployments, a minimum of two (2) replicas must be used.
- Deployments must use container images with a specific version tag, and not `latest`.
- Deployments must declare minimum and maximum memory/ CPU resources.

In the example below:

- We use `metadata.Name` directly after the `Rule` keyword to name the rule definition.
Each rule must be named uniquely.
- The `# Synopsis: ` comment is used to add additional metadata interpreted by PSRule.
- One or more conditions are defined within the curly braces `{ }`.
- The rule definition is saved within a file named `kubernetes.Rule.ps1`.

```powershell
# Synopsis: Must have the app.kubernetes.io/name label
Rule 'metadata.Name' {
    # Rule conditions go here
}
```

### Check that the label exists

In the next step, we define one or more conditions.

Conditions can be:

- Any valid PowerShell that returns a _true_ (pass) when the condition is met or _false_ (fail) when the condition is not met.
- More than one condition can be defined, if any condition returns _false_ then the whole rule fails.

PSRule includes several convenience keywords such as `AllOf`, `AnyOf`, `Exists`, `Match`, `TypeOf` and `Within` that make conditions faster to define, easier to understand and troubleshoot. However, use of these keywords is optional.

In the example below:

- We use the `Exists` keyword to check that the resource has the `app.kubernetes.io/name` label set.
  - By default, PSRule will step through nested properties separated by a `.`.
  i.e. `labels` is a property of `metadata`.
  - Kubernetes supports and recommends label namespaces, which often use `.` in their name.
  PSRule supports this by enclosing the field name (`app.kubernetes.io/name`) in apostrophes (`'`) so that `app.kubernetes.io/name` is checked instead of `app`.

```powershell
# Synopsis: Must have the app.kubernetes.io/name label
Rule 'metadata.Name' {
    Exists "metadata.labels.'app.kubernetes.io/name'"
}
```

We have also defined something similar for the _version_ and _component_ labels.

In the example below:

- Double apostrophes (`''`) are used to enclose `app.kubernetes.io/name` because the field name uses `'` at the start and end of the string instead of `"` in the previous example.
- The `Within` keyword is used to validate that the `app.kubernetes.io/component` only uses one of four (4) allowed values.

```powershell
# Synopsis: Must have the app.kubernetes.io/version label
Rule 'metadata.Version' {
    Exists 'metadata.labels.''app.kubernetes.io/version'''
}

# Synopsis: Must have the app.kubernetes.io/component label
Rule 'metadata.Component' {
    Exists 'metadata.labels.''app.kubernetes.io/component'''
    Within 'metadata.labels.''app.kubernetes.io/component''' 'web', 'api', 'database', 'gateway' -CaseSensitive
}
```

## Use custom binding

Before processing rules, PSRule binds `TargetName` and `TargetType` properties to the pipeline object.
These properties are used for filtering and displaying results.

The default properties that PSRule binds are different from how Kubernetes resources are structured.
Kubernetes uses:

- `metadata.name` to store the name of a resource.
- `kind` to store the type of resource.

The default bindings can be updated by providing custom property names or a custom script.
To change binding property names set the `Binding.TargetName` and `Binding.TargetType` configuration options.

The following example shows how to set the options using a YAML configuration file:

- TargetName is bound to `metadata.name`
- TargetType is bound to `kind`

```yaml
binding:
  targetName:
  - metadata.name
  targetType:
  - kind
```

These options can be set in the file `.\ps-rule.yaml` to be automatically loaded at when PSRule cmdlets are called.
To set these configuration options either edit the file manually or use the following command.

```powershell
# Set options in ps-rule.yaml
Set-PSRuleOption -TargetName 'metadata.Name' -TargetType 'kind';
```

Alternatively, these options can be set at runtime using the hashtable syntax.

```powershell
# Save options to a variable
$option = New-PSRuleOption -TargetName 'metadata.Name' -TargetType 'kind';
```

These options will be passed to `Invoke-PSRule` using the `-Option` parameter in a later step.

## Define preconditions

Currently the `metadata.Name` rule defined in a previous step will be executed for any type of object.
Kubernetes has many types of built-in resource such as _Services_, _Deployments_, _Namespaces_, _Pods_ and _ClusterRoles_.

By defining a precondition, we can ensure that the rule is only processed for _Services_ or _Deployments_ to match our business rules.

PSRule supports two types of preconditions, either type (`-Type`) or script block (`-If`).

- **Type** preconditions are one or more type names that PSRule compares to the `TargetType` binding, where:
  - One of the type names names equal `TargetType` the rule will be processed.
  - None of the type names equal `TargetType` the rule be skipped.
- **Script** block preconditions is a PowerShell script block that returns _true_ or _false_, where:
  - _True_ - Continue processing the rule.
  - _False_ - Skip processing the rule.

Preconditions are evaluated once per rule for each object.

In the example below:

- We update our `metadata.Name` rule to use the `-Type` parameter to specify a type precondition of either _Deployment_ or _Service_.
- In a previous step, `TypeName` was bound to the `kind` property which will be _Deployment_ or _Service_ for these resource types.

```powershell
# Synopsis: Must have the app.kubernetes.io/name label
Rule 'metadata.Name' -Type 'Deployment', 'Service' {
    Exists "metadata.labels.'app.kubernetes.io/name'"
}
```

Using a type precondition satisfies our business rules and will deliver faster performance then using a script block.
An example using a script block precondition is also shown below.

```powershell
# Synopsis: Must have the app.kubernetes.io/name label
Rule 'metadata.Name' -If { $TargetObject.kind -eq 'Deployment' -or $TargetObject.kind -eq 'Service' } {
    Exists "metadata.labels.'app.kubernetes.io/name'"
}
```

## Complete remaining rules

The remaining rule definitions from our defined business rules are included below.
Each follows a similar pattern and builds on the previous sections.

In the example below:

- The built-in variable `$TargetObject` is used to get the current pipeline object.
  - Built-in keywords like `Exists` automatically default to `$TargetObject`, but can be piped alternative input as shown in the rule definition named `deployment.ResourcesSet`.

```powershell
# Synopsis: Deployments use a minimum of 2 replicas
Rule 'deployment.HasMinimumReplicas' -Type 'Deployment' {
    Exists 'spec.replicas'
    $TargetObject.spec.replicas -ge 2
}

# Synopsis: Deployments use specific tags
Rule 'deployment.NotLatestImage' -Type 'Deployment' {
    foreach ($container in $TargetObject.spec.template.spec.containers) {
        $container.image -like '*:*' -and
        $container.image -notlike '*:latest'
    }
}

# Synopsis: Resource requirements are set for each container
Rule 'deployment.ResourcesSet' -Type 'Deployment' {
    foreach ($container in $TargetObject.spec.template.spec.containers) {
        $container | Exists 'resources.requests.cpu'
        $container | Exists 'resources.requests.memory'
        $container | Exists 'resources.limits.cpu'
        $container | Exists 'resources.limits.memory'
    }
}
```

## Execute rules

With some rules defined, the next step is to execute them.
For this example, we'll use `Invoke-PSRule` to get the result for each rule.
The `Test-PSRuleTarget` cmdlet can be used if only a _true_ or _false_ is required.

In our example we are using the YAML format to store Kubernetes resources.
PSRule has built-in support for YAML so we can import these files directly from disk or process output from a command such as `kubectl`.

In the examples below:

- The `-InputPath` parameter is used to load objects from disk as YAML.
YAML is automatically detected based on the `.yaml` file extension.
Alternatively the `-Foramt Yaml` parameter can be used.
- Binding parameters are read from `ps-rule.yaml` in the current working path.
Alternatively the `-Option` parameter could be used to specify an alternative file path.
- `kubectl` is called with the `-o yaml` to output resources as YAML.
- `kubectl` is piped to `Out-String` to convert the multi-line output to a single string.
- The `-Format` parameter informs PSRule that the string is YAML and it should convert the string into structured objects.
- The `-ObjectPath` parameter is used with the output from `kubectl`.
This is required because the output from `kubectl` is a collection of resources instead of individual resources.
Specifically `-ObjectPath items` gets the resources from the `items` property of the output.

```powershell
# Validate resources from file
Invoke-PSRule -InputPath resources.yaml;
```

```powershell
# Validate resources directly from kubectl output
kubectl get services -o yaml | Out-String | Invoke-PSRule -Format Yaml -ObjectPath items;
```

For this example, we limited the output to failed results with the following command:

```powershell
# Validate resources from file
Invoke-PSRule -Path docs/scenarios/kubernetes-resources -InputPath docs/scenarios/kubernetes-resources/resources.yaml -Option docs/scenarios/kubernetes-resources/ps-rule.yaml -Outcome Fail;
```

The resulting output is:

```text
   TargetName: app1-cache

RuleName                            Outcome    Recommendation
--------                            -------    --------------
deployment.HasMinimumReplicas       Fail       Deployments use a minimum of 2 replicas
deployment.NotLatestImage           Fail       Deployments use specific tags
deployment.ResourcesSet             Fail       Resource requirements are set for each container


   TargetName: app1-cache-service

RuleName                            Outcome    Recommendation
--------                            -------    --------------
metadata.Name                       Fail       Must have the app.kubernetes.io/name label
metadata.Version                    Fail       Must have the app.kubernetes.io/version label
metadata.Component                  Fail       Must have the app.kubernetes.io/component label


   TargetName: app1-ui

RuleName                            Outcome    Recommendation
--------                            -------    --------------
metadata.Version                    Fail       Must have the app.kubernetes.io/version label
```

## More information

- [kubernetes.Rule.ps1](kubernetes.Rule.ps1) - Example rules for validating Kubernetes resources.
- [resources.yaml](resources.yaml) - An example Kubernetes manifest.
- [ps-rule.yaml](ps-rule.yaml) - PSRule options configuration file.

[PSRule.Rules.Kubernetes]: https://github.com/Microsoft/PSRule.Rules.Kubernetes
