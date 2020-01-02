---
category: Security
severity: Critical
---

# Use specific tags

## SYNOPSIS

Containers should use specific tags instead of latest.

## DESCRIPTION

Containers should use specific tags instead of latest.

## RECOMMENDATION

Deployments or pods should identify a specific tag to use for container images instead of `latest`.
When `latest` is used it may be hard to determine which version of the image is running.

When using variable tags such as v1.0 (which may refer to v1.0.0 or v1.0.1) consider using `imagePullPolicy: Always` to ensure that the an out-of-date cached image is not used.

The `latest` tag automatically uses `imagePullPolicy: Always` instead of the default `imagePullPolicy: IfNotPresent`.

## LINKS

- [Stable tags](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-image-tag-version#stable-tags)
