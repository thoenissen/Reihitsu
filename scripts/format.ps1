<#
.SYNOPSIS
    Formats the given paths with the repository's own CLI formatter.
.DESCRIPTION
    Run this over every changed C# path before running tests, as CLAUDE.md and
    AGENTS.md require.
.PARAMETER Paths
    Files or directories to format.
.PARAMETER NoInstall
    Fail instead of installing when the SDK is missing.
.EXAMPLE
    .\scripts\format.ps1 Reihitsu.Formatter\Pipeline\FormattingPipeline.cs
#>
param(
    [Parameter(Mandatory = $true, Position = 0, ValueFromRemainingArguments = $true)]
    [string[]]$Paths,

    [switch]$NoInstall
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'lib' 'dotnet-env.ps1')

Initialize-ReihitsuDotnet -NoInstall:$NoInstall -Quiet

Push-Location (Get-ReihitsuRepositoryRoot)

try
{
    & dotnet run --project Reihitsu.Cli -- @Paths

    if ($LASTEXITCODE -ne 0)
    {
        exit $LASTEXITCODE
    }
}
finally
{
    Pop-Location
}
