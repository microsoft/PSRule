#
# Validation rules for Kubernetes objects
#

# Description: Deployments use a minimum of 2 replicas
Rule 'deployment.HasMinimumReplicas' -If { $TargetObject.kind -eq 'Deployment' } {
    Exists 'spec.replicas'
    $TargetObject.spec.replicas -ge 2
}

# Description: Services should not have a load balancer configured
Rule 'service.NotLoadBalancer' -If { $TargetObject.kind -eq 'Service' } {
    AnyOf {
        Exists 'spec.type' -Not
        $TargetObject.spec.type -ne 'LoadBalancer'
    }
}
