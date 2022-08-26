---
reviewed: 2022-02-11
author: BernieWhite
---

# Analysis output

PSRule supports generating and saving output in a number of different formats.

!!! Abstract
    This topic covers the supported formats and options for presenting output from a PSRule run.

## Setting the output format

The output format can be configuring by setting the `Output.Format` option to one the following:

- `Yaml` - Output is serialized as YAML.
- `Json` - Output is serialized as JSON.
- `Markdown` - Output is serialized as Markdown.
- `NUnit3` - Output is serialized as NUnit3 (XML).
- `Csv` - Output is serialized as a comma-separated values (CSV).
- `Sarif` - Output is serialized as SARIF.

!!! Tip
    To write output to a file, also set the `Output.Path` option to the file path to save.

=== "GitHub Actions"

    ```yaml hl_lines="5-6"
    # Analyze and save results
    - name: Analyze repository
      uses: microsoft/ps-rule@v2.3.2
      with:
        outputFormat: Sarif
        outputPath: reports/ps-rule-results.sarif
    ```

=== "Azure Pipelines"

    ```yaml hl_lines="6-7"
    # Analyze and save results
    - task: ps-rule-assert@2
      displayName: Analyze repository
      inputs:
        inputType: repository
        outputFormat: Sarif
        outputPath: reports/ps-rule-results.sarif
    ```

=== "PowerShell"

    ```powershell title="Invoke-PSRule"
    Invoke-PSRule -OutputFormat Sarif -OutputPath reports/ps-rule-results.sarif
    ```

    ```powershell title="Assert-PSRule"
    Assert-PSRule -OutputFormat Sarif -OutputPath reports/ps-rule-results.sarif
    ```

=== "Options file"

    ```yaml title="ps-rule.yaml" hl_lines="2-3"
    output:
      format: 'Sarif'
      path: reports/ps-rule-results.sarif
    ```

## Formatting as YAML

When using the YAML output format, results a serialized as YAML.
Two spaces are used to indent properties of objects.

??? Example "Example output"

    ```yaml
    - data: {}
      info:
        displayName: Local.PS.RequireTLS
        name: Local.PS.RequireTLS
        synopsis: An example rule to require TLS.
      level: Error
      outcome: Fail
      outcomeReason: Processed
      reason:
      - The field 'configure.supportsHttpsTrafficOnly' is set to 'False'.
      - The field 'configure.minTLSVersion' does not exist.
      ruleName: Local.PS.RequireTLS
      runId: 16b0534165ffb5279beeb1672a251fc1ff3124b6
      source:
      - file: C:\Dev\Workspace\PSRule\docs\authoring\writing-rules\settings.json
        line: 2
        position: 11
        type: File
      targetName: 1fe7c0f476b11301402d5017d87424c36ff085a8
      targetType: app1
      time: 0
    ```

## Formatting as JSON

When using the JSON output format, results are serialized as JSON.
By default, no indentation is used.

??? Example "Example output"

    ```json
    [{"data":{},"info":{"displayName":"Local.PS.RequireTLS","name":"Local.PS.RequireTLS","synopsis":"An example rule to require TLS."},"level":1,"outcome":"Fail","outcomeReason":"Processed","reason":["The field 'configure.supportsHttpsTrafficOnly' is set to 'False'.","The field 'configure.minTLSVersion' does not exist."],"ruleName":"Local.PS.RequireTLS","runId":"df662aad3ae7adee6f35b9733c7aaa53dc4d6b96","source":[{"file":"C:\\Dev\\Workspace\\PSRule\\docs\\authoring\\writing-rules\\settings.json","line":2,"position":11,"type":"File"}],"targetName":"1fe7c0f476b11301402d5017d87424c36ff085a8","targetType":"app1","time":0}]
    ```

### Configuring JSON indentation

:octicons-milestone-24: v1.8.0

The number of spaces used to indent properties and elements is configurable between `0` to `4` spaces.
By default, no indentation is used.

??? Example "Example output with 2 spaces"

    ```yaml
    [
      {
        "data": {},
        "info": {
          "displayName": "Local.PS.RequireTLS",
          "name": "Local.PS.RequireTLS",
          "synopsis": "An example rule to require TLS."
        },
        "level": 1,
        "outcome": "Fail",
        "outcomeReason": "Processed",
        "reason": [
          "The field 'configure.supportsHttpsTrafficOnly' is set to 'False'.",
          "The field 'configure.minTLSVersion' does not exist."
        ],
        "ruleName": "Local.PS.RequireTLS",
        "runId": "3afadfed32e57f5283ad71c1aa496da822ff0c84",
        "source": [
          {
            "file": "C:\\Dev\\Workspace\\PSRule\\docs\\authoring\\writing-rules\\settings.json",
            "line": 2,
            "position": 11,
            "type": "File"
          }
        ],
        "targetName": "1fe7c0f476b11301402d5017d87424c36ff085a8",
        "targetType": "app1",
        "time": 0
      }
    ]
    ```

## Formatting as CSV

The output from analysis can be formatted as comma-separated values (CSV).
Formatting as CSV may be useful when manipulating output results by hand.
Output of CSV format varies depending on if detailed or summary output is used.

For detailed output, the following columns are added to CSV output for each processed object:

Column           | Description
------           | -----------
`RuleName`       | The name of the rule.
`TargetName`     | The name of the object that was analyzed.
`TargetType`     | The type of the object that was analyzed.
`Outcome`        | The outcome of the analysis, such as `Pass` or `Fail`.
`OutcomeReason`  | An additional reason for the outcome such as `Inconclusive`.
`Synopsis`       | A short description of the rule.
`Recommendation` | The recommendation of the rule.

For summary output, the following columns are used:

Column           | Description
------           | -----------
`RuleName`       | The name of the rule.
`Pass`           | The number of objects that passed.
`Fail`           | The number of objects that failed.
`Outcome`        | The worst case outcome of the analysis, such as `Pass` or `Fail`.
`Synopsis`       | A short description of the rule.
`Recommendation` | The recommendation of the rule.

??? Example "Example output"

    ```csv
    RuleName,TargetName,TargetType,Outcome,OutcomeReason,Synopsis,Recommendation
    "Local.PS.RequireTLS","1fe7c0f476b11301402d5017d87424c36ff085a8","app1","Fail","Processed","An example rule to require TLS.",
    "Local.YAML.RequireTLS","1fe7c0f476b11301402d5017d87424c36ff085a8","app1","Fail","Processed","An example rule to require TLS.",
    "Local.JSON.RequireTLS","1fe7c0f476b11301402d5017d87424c36ff085a8","app1","Fail","Processed","An example rule to require TLS.",
    ```

## Formatting as SARIF

:octicons-milestone-24: v2.0.0

Static Analysis Results Interchange Format (SARIF) is a standard output format for static analysis tools.
It enables various unrelated tools to consume analysis results from PSRule.
You can use SARIF to perform Static Analysis Security Testing (SAST) in DevOps environments at-scale.

### GitHub code scanning alerts

SARIF results from PSRule can be uploaded to GitHub to create code scanning alerts against a repository.
You can see these results in your repository visible under _Security_ > _Code scanning alerts_.

!!! Tip
    Code scanning is available for all public repositories,
    and for private repositories owned by organizations where GitHub Advanced Security is enabled.
    For more information, see [About GitHub Advanced Security][1].

To configure GitHub Actions, perform the following steps:

- Create a GitHub Actions workflow.
- Add a step using the `microsoft/ps-rule` action.
  - Configure the `outputFormat` and `outputPath` parameters.
- Add a step using the `github/codeql-action/upload-sarif` action.
  - Configure the `sarif_file` parameter to the same file path specified in `outputPath`.

!!! Example "Example `.github/workflows/analyze.yaml`"

    ```yaml
    name: Analyze
    on:
      push:
        branches: [ main ]
      schedule:
      - cron: '24 22 * * 0' # At 10:24 PM, on Sunday each week
      workflow_dispatch:

    jobs:
      oss:
        name: Analyze with PSRule
        runs-on: ubuntu-latest
        permissions:
          contents: read
          security-events: write
        steps:

        - name: Checkout
          uses: actions/checkout@v3

        - name: Run PSRule analysis
          uses: microsoft/ps-rule@v2.3.2
          with:
            outputFormat: Sarif
            outputPath: reports/ps-rule-results.sarif

        - name: Upload results to security tab
          uses: github/codeql-action/upload-sarif@v2
          with:
            sarif_file: reports/ps-rule-results.sarif
    ```

  [1]: https://docs.github.com/get-started/learning-about-github/about-github-advanced-security

### Azure DevOps scans tab

SARIF results from PSRule can be uploaded and viewed within Azure DevOps.
To add the scans tab to build results the [SARIF SAST Scans Tab][2] extension needs to be installed.

  [2]: https://marketplace.visualstudio.com/items?itemName=sariftools.scans

*[SARIF]: Static Analysis Results Interchange Format
