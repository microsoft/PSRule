# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

ARG MODULE_VERSION=1.1.0

FROM mcr.microsoft.com/powershell:7.1.2-alpine-3.12-20210211@sha256:08e7e927bf8647c960401c44107ad120b54da74e1eb60a709bf9f38f5d5bf925
SHELL ["pwsh", "-Command"]
RUN $ProgressPreference = [System.Management.Automation.ActionPreference]::SilentlyContinue; \
    $Null = New-Item -Path /ps_modules/ -ItemType Directory -Force; \
    Save-Module -Name PSRule -RequiredVersion ${MODULE_VERSION} -Force -Path /ps_modules/;

COPY LICENSE README.md powershell.ps1 /
CMD ["pwsh", "-File", "/powershell.ps1"]
