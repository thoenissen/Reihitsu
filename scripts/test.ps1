<#
.SYNOPSIS
    Runs the Reihitsu test projects in Release configuration.
.DESCRIPTION
    Test projects are addressed by absolute path, so the caller's working
    directory is never changed and relative arguments keep their meaning.
.PARAMETER Project
    Which project to run: analyzer, formatter, core, cli, architecture, tooling, playground, or all (default).
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
    [ValidateSet('analyzer', 'formatter', 'core', 'cli', 'architecture', 'tooling', 'playground', 'all')]
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
    'analyzer' { @('src/Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj') }
    'formatter' { @('src/Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj') }
    'core' { @('src/Reihitsu.Core.Test/Reihitsu.Core.Test.csproj') }
    'cli' { @('src/Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj') }
    'architecture' { @('src/Reihitsu.ArchitectureTests/Reihitsu.ArchitectureTests.csproj') }
    'tooling' { @('src/Reihitsu.Tooling.Test/Reihitsu.Tooling.Test.csproj') }
    'playground' { @('src/Reihitsu.Playground.Test/Reihitsu.Playground.Test.csproj') }
    'all'
    {
        @('src/Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj',
          'src/Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj',
          'src/Reihitsu.Core.Test/Reihitsu.Core.Test.csproj',
          'src/Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj',
          'src/Reihitsu.ArchitectureTests/Reihitsu.ArchitectureTests.csproj',
          'src/Reihitsu.Tooling.Test/Reihitsu.Tooling.Test.csproj',
          'src/Reihitsu.Playground.Test/Reihitsu.Playground.Test.csproj')
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
