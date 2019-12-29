---
severity: Important
category: Resource management
online version: https://github.com/Microsoft/PSRule/blob/master/docs/scenarios/rule-docs/en-US/metadata.Name.md
---

# Use recommended name label

## SYNOPSIS

Deployments and services must use the app.kubernetes.io/name label.

## DESCRIPTION

Kubernetes defines a common set of labels that are recommended for tool interoperability.
These labels should be used to consistently apply standard metadata.

The `app.kubernetes.io/name` label should be used to specify the name of the application.

## RECOMMENDATION

Consider setting the recommended label `app.kubernetes.io/name` on deployment and service resources.
