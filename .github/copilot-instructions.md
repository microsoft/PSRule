# PSRule Development Instructions

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

PSRule is a cross-platform PowerShell module for validating infrastructure as code (IaC) and objects using PowerShell rules. The project includes .NET libraries, a PowerShell module, CLI tools, VS Code extension, and comprehensive documentation.

### Prerequisites and Setup
- .NET SDK 8.0.400+ (specified in global.json)
- PowerShell 7.0+ (tested with 7.4.10)
- Node.js 20+ for VS Code extension (tested with 20.19.4)
- Python 3.11+ for documentation (tested with 3.12.3)

### Bootstrap and Build Process
**NEVER CANCEL builds or long-running commands.** All timing values below include 50% buffer for safety.

1. **Install PowerShell dependencies** (3 seconds, NEVER CANCEL):
   ```bash
   pwsh ./scripts/pipeline-deps.ps1
   ```

2. **Restore .NET dependencies** (2 seconds, NEVER CANCEL):
   ```bash
   dotnet restore
   ```

3. **Build PowerShell module** (35 seconds, NEVER CANCEL, set timeout to 60+ minutes):
   ```bash
   pwsh -c "Invoke-Build BuildModule -Configuration Release"
   ```

4. **Build CLI tool** (25 seconds, NEVER CANCEL, set timeout to 60+ minutes):
   ```bash
   pwsh -c "Invoke-Build BuildCLI -Configuration Release"
   ```

5. **Build VS Code extension** (15 seconds, NEVER CANCEL, set timeout to 30+ minutes):
   ```bash
   npm install
   npm run build
   ```

6. **Build documentation** (13 seconds Python deps + 5 seconds build, NEVER CANCEL, set timeout to 30+ minutes):
   ```bash
   python3 -m pip install -r requirements-docs.txt
   mkdocs build
   ```

### Testing
**NEVER CANCEL test runs.** Some tests require network access and may fail in restricted environments.

- **Unit tests for Types** (5 seconds, NEVER CANCEL, set timeout to 30+ minutes):
  ```bash
  dotnet test tests/PSRule.Types.Tests/
  ```

- **All .NET tests** (may fail due to network restrictions, 60+ seconds, NEVER CANCEL, set timeout to 60+ minutes):
  ```bash
  dotnet test
  ```

- **Format validation** (50 seconds, NEVER CANCEL, set timeout to 60+ minutes):
  ```bash
  dotnet format --verify-no-changes
  ```

## Validation Scenarios

Always test functionality after making changes:

1. **Verify PowerShell module loads correctly**:
   ```bash
   pwsh -c "Import-Module ./out/modules/PSRule/PSRule.psd1 -Force; Get-Command -Module PSRule | Measure-Object | Select Count"
   ```
   Expected: Should return 11 commands without errors.

2. **Test CLI tool functionality**:
   ```bash
   ./out/cli/build/Microsoft.PSRule.Tool --help
   ```
   Expected: Should display help text starting with "PSRule CLI v0.0.1".

3. **Verify VS Code extension builds**:
   ```bash
   npm run package
   ```
   Expected: Creates `out/package/vscode-ps-rule-0.0.1.vsix` (~29MB).

## Project Structure

### Key Directories
- `src/` - Source code for all components
  - `PSRule/` - Core PowerShell module
  - `PSRule.Tool/` - CLI application
  - `PSRule.Types/` - Type definitions
  - `PSRule.CommandLine/` - Command line interface
  - `PSRule.EditorServices/` - VS Code language server
- `tests/` - Unit tests for all components
- `docs/` - Documentation source (MkDocs)
- `out/` - Build output directory
- `scripts/` - Build and utility scripts

### Key Files
- `pipeline.build.ps1` - Main build script using InvokeBuild
- `build.ps1` - Simple wrapper for pipeline.build.ps1
- `global.json` - .NET SDK version specification
- `package.json` - VS Code extension configuration
- `requirements-docs.txt` - Python documentation dependencies
- `mkdocs.yml` - Documentation site configuration

## Common Tasks

### Working with PowerShell Module
- Module manifest: `src/PSRule/PSRule.psd1`
- Main module file: `src/PSRule/PSRule.psm1`
- Built module location: `out/modules/PSRule/`

### Working with CLI Tool
- Source: `src/PSRule.Tool/`
- Built executable: `out/cli/build/Microsoft.PSRule.Tool`
- Test with: `./out/cli/build/Microsoft.PSRule.Tool --help`

### Working with VS Code Extension
- Source: `src/vscode-ps-rule/`
- Build with: `npm run build` or `npm run package`
- Package output: `out/package/vscode-ps-rule-*.vsix`

### Linting and Code Quality
- **ALWAYS run these before committing** or CI will fail:
  ```bash
  dotnet format --verify-no-changes
  ```

### Documentation
- Source files in `docs/` directory
- Build with `mkdocs build`
- Serve locally with `mkdocs serve`
- May show API rate limit warnings (normal in CI environments)

## Build Targets

Use `Invoke-Build` with these common targets:
- `BuildModule` - Build PowerShell module only
- `BuildCLI` - Build CLI tool
- `Test` - Run all tests (may fail due to network restrictions)
- `TestDotNet` - Run .NET unit tests only
- `BuildHelp` - Generate help documentation

## Development Context

- When implementing logging use `ILogger` from `src/PSRule.Types/Runtime/ILogger.cs`.
- When creating new files, always add a trailing newline before the end of the file.

## Troubleshooting

### Common Issues
1. **Tests fail with network errors**: Normal in restricted environments. Focus on core functionality tests.
2. **PowerShell execution policy errors**: Use `pwsh` instead of `powershell` for cross-platform compatibility.
3. **Documentation build API errors**: Normal without GitHub token. Use `mkdocs build` without `--strict`.
4. **Long build times**: Builds can take 60+ seconds total. NEVER cancel early.

### Performance Expectations
- Complete build cycle: ~80 seconds
- Incremental module build: ~25 seconds  
- Unit tests: ~5 seconds (Types), ~60+ seconds (full suite)
- Documentation build: ~5 seconds
- VS Code extension: ~15 seconds

Always use appropriate timeouts (60+ minutes for builds, 30+ minutes for tests) and NEVER cancel long-running operations.
