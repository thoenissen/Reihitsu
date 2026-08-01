<#
.SYNOPSIS
    Proves that a change carries no compiled behavior.
.DESCRIPTION
    Runs the syntax-aware proof over the changed files. The single
    "TEXT-ONLY PROOF: ..." line is the quotable evidence the gh-* skills record
    when they skip a preflight attempt or apply a non-blocking cleanup.

    Exit codes: 0 proven text-only, 1 not proven, 2 tool or setup error.
.PARAMETER Base
    Base revision. Defaults to the merge base with origin/main.
.PARAMETER Head
    Head revision, or "worktree" to include uncommitted changes. Defaults to HEAD.
.PARAMETER NoInstall
    Fail instead of installing when the SDK is missing.
.EXAMPLE
    .\scripts\verify-text-only.ps1
.EXAMPLE
    .\scripts\verify-text-only.ps1 -Base a1b2c3d -Head worktree
#>
param(
    [string]$Base,

    [string]$Head,

    [switch]$NoInstall
)

$ErrorActionPreference = 'Stop'

. (Join-Path (Join-Path $PSScriptRoot 'lib') 'dotnet-env.ps1')

try
{
    Initialize-ReihitsuDotnet -NoInstall:$NoInstall -Quiet
}
catch
{
    Write-Error $_
    exit 2
}

$arguments = @()

if ($Base) { $arguments += @('--base', $Base) }
if ($Head) { $arguments += @('--head', $Head) }

Push-Location (Get-ReihitsuRepositoryRoot)

try
{
    & dotnet run scripts/proof/verify-text-only.cs -- @arguments

    exit $LASTEXITCODE
}
finally
{
    Pop-Location
}
