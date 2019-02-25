# Kubernetes resource validation example

This is an example of how PSRule can be used to validate Kubernetes resources to match an internal metadata and configuration standard.

This scenario covers the following:

- Defining a basic rule.
- Using custom binding options.
- Running rules using YAML input.

In this scenario we will use a YAML file:

- [`azure-vote-all-in-one-redis.yaml`](azure-vote-all-in-one-redis.yaml) - A Kubernetes manifest containing deployments and services.

## Define rules

To validate our Kubernetes resources, we need to define some rules. Rules are defined by using the `Rule` keyword in a file ending with the `.Rule.ps1` extension.

Our business rules for Kubernetes resource metadata can be defined with the following dot points:

- The following [recommended](https://kubernetes.io/docs/concepts/overview/working-with-objects/common-labels/) labels will be used on all services and deployments:
  - `app.kubernetes.io/name` - the name of the application/ service.
  - `app.kubernetes.io/version` - a semver compliant version number.
  - `app.kubernetes.io/component` - identifies the type of component, valid options are `web`, `api`, `database` and `gateway`
- For `web` or `api` deployments, a minimum of two (2) replicas must be used.
- Deployments must use container images with a specific version tag, and not `latest`.
- Deployments must declare minimum and maximum memory/ CPU resources.

In the example below:

- We use `metadata.Name` directly after the `Rule` keyword to name the rule definition. Each rule must be named uniquely.
- The `# Description: ` comment is used to add additional metadata interpreted by PSRule.
- One or more conditions are defined within the curly braces `{ }`.
- The rule definition is saved within a file named `kubernetes.Rule.ps1`.

```powershell
# Description: Must have the app.kubernetes.io/name label
Rule 'metadata.Name' {
    # Rule conditions go here
}
```

### Check that the label exists

In the next step, we define one or more conditions.

In the example below:

- We define a condition that checks for the existence of the label and ensures that the label value is set.

```powershell
# Description: Must have the app.kubernetes.io/name label
Rule 'metadata.Name' {
    Exists 'metadata.labels.''app.kubernetes.io/name'''
}
```

## Use custom binding

Before processing rules, PSRule binds `TargetName` and `TargetType` properties to the pipeline object. These properties are used for filtering and displaying results.

The default properties that PSRule binds are different from how Kubernetes resources are structured. Kubernetes uses:

- `metadata.name` to store the name of a resource.
- `kind` to store the type of resource.

The default bindings can be updated by providing custom property names or a custom script. To change binding property names set the `Binding.TargetName` and `Binding.TargetType` configuration options.

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

Alternatively these options can be set at runtime using the hashtable syntax.

```powershell
$option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'metadata.name'; 'Binding.TargetType' = 'kind' };
```

These options will be passed to `Invoke-PSRule` using the `-Option` parameter in a later step.

### Define preconditions

Currently the `metadata.Name` rule defined in a previous step will be executed for any type of object. Kubernetes has many type of built-in resource types such as _Services_, _Deployments_, _Namespaces_, _Pods_ and _ClusterRoles_.

By defining a precondition we can ensure that the rule is only processed for _Services_ or _Deployments_.

PSRule supports two types of preconditions, either type (`-Type`) or script block (`-If`).

Type preconditions are one or more type names that PSRule compares to the `TargetType` binding, where:

- One of the type names names equal `TargetType` the rule will be processed.
- None of the type names equal `TargetType` the rule be skipped.

Script block preconditions is a PowerShell script block that returns _true_ or _false_, where:

- _True_ - Continue processing the rule.
- _False_ - Skip processing the rule.

Preconditions are evaluated once per rule for each object.

In the example below:

- We use the `-Type` parameter to specify a type precondition of either _Deployment_ or _Service_.
- In a previous step, `TypeName` was bound to the `kind` property which will continue _Deployment_ or _Service_ for these resource types.

```powershell
# Description: Must have the app.kubernetes.io/name label
Rule 'metadata.Name' -Type 'Deployment', 'Service' {
    Exists 'metadata.labels.''app.kubernetes.io/name'''
}
```

Using a type precondition satisfies our business rules, and will delivery faster performance then using a script block. However for the example using a script block precondition is also shown below.

```powershell
# Description: Must have the app.kubernetes.io/name label
Rule 'metadata.Name' -If { $TargetObject.kind -eq 'Deployment' -or $TargetObject.kind -eq 'Service' } {
    Exists 'metadata.labels.''app.kubernetes.io/name'''
}
```

## Execute rules

With a rule defined, the next step is to execute it. To execute rules, pipe the target object to `Invoke-PSRule`.

For example:

```powershell
# Validate resources from file
$option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'metadata.name' }
Invoke-PSRule -InputObject (Get-Content azure-vote-all-in-one-redis.yaml -Raw) -Format Yaml -Option $option;
```

```powershell
# Validate resources directly from kubectl output
$option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'metadata.name' }
Invoke-PSRule -InputObject (kubectl get services -o yaml | Out-String) -Format Yaml -Option $option -ObjectPath items;
```

For this example we ran:

```powershell
$option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'metadata.name' }
Invoke-PSRule -InputObject (Get-Content docs/scenarios/kubernetes-resources/azure-vote-all-in-one-redis.yaml -Raw) -Path docs/scenarios/kubernetes-resources -Format Yaml -Option $option;
```

---

```powershell
# Description: Deployments use a minimum of 2 replicas
Rule 'deployment.HasMinimumReplicas' -If { $TargetObject.kind -eq 'Deployment' } {
    Exists 'spec.replicas'
    $TargetObject.spec.replicas -ge 2
}
```

```powershell
# Description: Services should not have a load balancer configured
Rule 'service.NotLoadBalancer' -If { $TargetObject.kind -eq 'Service' } {
    AnyOf {
        Exists 'spec.type' -Not
        $TargetObject.spec.type -ne 'LoadBalancer'
    }
}
```
