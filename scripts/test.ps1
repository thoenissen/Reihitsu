<#
.SYNOPSIS
    Runs the Reihitsu test projects in Release configuration.
.DESCRIPTION
    Test projects are addressed by absolute path, so the caller's working
    directory is never changed and relative arguments keep their meaning.
.PARAMETER Project
    Which project to run: analyzer, formatter, core, cli, architecture, tooling, or all (default).
.PARAMETER Filter
    Test filter expression for a focused run.
.PARAMETER NoBuild
    Reuse the previous Release build.
.PARAMETER NoInstall
    Fail instead of installing when the SDK is missing.
.EXAMPLE
    .\scripts\test.ps1
.EXAMPLE
    .\scripts\test.ps1 -Project analyzer -Filter "FullyQualifiedName~RH3204"
#>
param(
    [ValidateSet('analyzer', 'formatter', 'core', 'cli', 'architecture', 'tooling', 'all')]
    [string]$Project = 'all',

    [string]$Filter,

    [switch]$NoBuild,

    [switch]$NoInstall,

    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$TestArguments = @()
)

$ErrorActionPreference = 'Stop'

. (Join-Path (Join-Path $PSScriptRoot 'lib') 'dotnet-env.ps1')

$projects = switch ($Project)
{
    'analyzer' { @('Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj') }
    'formatter' { @('Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj') }
    'core' { @('Reihitsu.Core.Test/Reihitsu.Core.Test.csproj') }
    'cli' { @('Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj') }
    'architecture' { @('Reihitsu.ArchitectureTests/Reihitsu.ArchitectureTests.csproj') }
    'tooling' { @('Reihitsu.Tooling.Test/Reihitsu.Tooling.Test.csproj') }
    'all'
    {
        @('Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj',
          'Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj',
          'Reihitsu.Core.Test/Reihitsu.Core.Test.csproj',
          'Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj',
          'Reihitsu.ArchitectureTests/Reihitsu.ArchitectureTests.csproj',
          'Reihitsu.Tooling.Test/Reihitsu.Tooling.Test.csproj')
    }
}

Initialize-ReihitsuDotnet -NoInstall:$NoInstall -Quiet

$repositoryRoot = Get-ReihitsuRepositoryRoot

foreach ($testProject in $projects)
{
    Write-Host "==> $testProject"

    $arguments = @((Join-Path $repositoryRoot $testProject), '-c', 'Release', '--verbosity', 'minimal')

    if ($NoBuild) { $arguments += '--no-build' }
    if ($Filter) { $arguments += @('--filter', $Filter) }

    & dotnet test @arguments @TestArguments

    if ($LASTEXITCODE -ne 0)
    {
        exit $LASTEXITCODE
    }
}
