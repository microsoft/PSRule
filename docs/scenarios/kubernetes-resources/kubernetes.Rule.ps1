#
# Validation rules for Kubernetes resources
#

# Description: Must have the app.kubernetes.io/name label
Rule 'metadata.Name' -Type 'Deployment', 'Service' {
    Exists "metadata.labels.'app.kubernetes.io/name'"
}

# Description: Must have the app.kubernetes.io/version label
Rule 'metadata.Version' -Type 'Deployment', 'Service' {
    Exists 'metadata.labels.''app.kubernetes.io/version'''
}

# Description: Must have the app.kubernetes.io/component label
Rule 'metadata.Component' -Type 'Deployment', 'Service' {
    Exists 'metadata.labels.''app.kubernetes.io/component'''
    Within 'metadata.labels.''app.kubernetes.io/component''' 'web', 'api', 'database', 'gateway' -CaseSensitive
}

# Description: Deployments use a minimum of 2 replicas
Rule 'deployment.HasMinimumReplicas' -Type 'Deployment' {
    Exists 'spec.replicas'
    $TargetObject.spec.replicas -ge 2
}

# Description: Deployments use specific tags
Rule 'deployment.NotLatestImage' -Type 'Deployment' {
    foreach ($container in $TargetObject.spec.template.spec.containers) {
        $container.image -like '*:*' -and 
        $container.image -notlike '*:latest'
    }
}

# Description: Services should not have a load balancer configured
Rule 'service.NotLoadBalancer' -Type 'Service' {
    AnyOf {
        Exists 'spec.type' -Not
        $TargetObject.spec.type -ne 'LoadBalancer'
    }
}
