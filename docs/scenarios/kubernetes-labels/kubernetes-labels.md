# Kubernetes resource validation example

This is an example of how PSRule can be used to validate Kubernetes resources to match an internal standard.

//labels and annotations on 

This scenario covers the following:

- Defining a basic rule.
- Running rules using YAML input.
- Nested property TargetName binding.

In this scenario we will use a YAML file:

- [`azure-vote-all-in-one-redis.yaml`](azure-vote-all-in-one-redis.yaml) - A Kubernetes manifest containing deployments and services.

## Define rules

To validate our Kubernetes resources, we need to define some rules. Rules are defined by using the `Rule` keyword in a file ending with the `.Rule.ps1` extension.




We want to achieve:

```powershell
# Validate service objects
Invoke-PSRule -InputObject (kubectl get services -o yaml) -Format Yaml;
```

Or:

```powershell
# Validate service objects
Invoke-PSRule -InFile .\azure-vote-all-in-one-redis.yaml -Format Yaml;
```

For this example we ran:

```powershell
$option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'metadata.name' }
Invoke-PSRule -InputObject (Get-Content docs/scenarios/kubernetes-labels/azure-vote-all-in-one-redis.yaml) -Path docs/scenarios/kubernetes-labels -Format Yaml -Option $option;
```

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
