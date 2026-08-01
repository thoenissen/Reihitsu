<#
.SYNOPSIS
    Formats the given paths with the repository's own CLI formatter.
.DESCRIPTION
    Paths are resolved against the caller's working directory, not the
    repository root, so a relative path means what it looks like it means.

    Run this over every changed C# path before running tests, as AGENTS.md and
    CLAUDE.md require.
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

. (Join-Path (Join-Path $PSScriptRoot 'lib') 'dotnet-env.ps1')

$arguments = @()

foreach ($path in $Paths)
{
    if ($path.StartsWith('-'))
    {
        $arguments += $path
    }
    elseif (Test-Path -LiteralPath $path)
    {
        $arguments += (Resolve-Path -LiteralPath $path).Path
    }
    else
    {
        $arguments += $path
    }
}

Initialize-ReihitsuDotnet -NoInstall:$NoInstall -Quiet

$cli = Join-Path (Get-ReihitsuRepositoryRoot) 'Reihitsu.Cli'

& dotnet run --project $cli -- @arguments

exit $LASTEXITCODE
