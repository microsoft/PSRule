---
resource: Storage Account
online version: https://github.com/Microsoft/PSRule/blob/main/docs/scenarios/rule-module/Enterprise.Rules/en/Org.Az.Storage.UseHttps.md
---

# Enforce encrypted Storage connections

## SYNOPSIS

Storage accounts should only accept encrypted connections.

## DESCRIPTION

An Azure Storage Account is configured to allow unencrypted connections.
This does not indicate that unencrypted connections are being used.

Unencrypted communication to storage accounts could allow disclosure of information to an untrusted party.

Storage Accounts can be configured to require encrypted connections, by setting the _Secure transfer required_ option.
If _secure transfer required_ is not enabled (the default), unencrypted and encrypted connections are permitted.

When _secure transfer required_ is enabled, attempts to connect to storage using HTTP or unencrypted SMB connections are rejected.

## RECOMMENDATION

Storage accounts should only accept secure traffic.
Consider _setting secure transfer_ required if there is no requirement to access storage over unencrypted connections.
Also consider using Azure Policy to audit or enforce this configuration.

## LINKS

- [Require secure transfer in Azure Storage](https://docs.microsoft.com/en-au/azure/storage/common/storage-require-secure-transfer)
- [Sample policy for ensuring https traffic](https://docs.microsoft.com/en-au/azure/governance/policy/samples/ensure-https-stor-acct)
