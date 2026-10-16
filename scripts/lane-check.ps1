<#
.SYNOPSIS
    Confirms the squad lane a run claimed against the paths its diff changes.
.DESCRIPTION
    The diff's paths decide, not the claim: each changed path is classified, and
    the narrowest lane that admits all of them is compared with the claimed one.
    C# files count as text only when verify-text-only proves the whole diff
    text-only, so this script runs that proof first and prints its line.

    The single "LANE CHECK: ..." line is the quotable evidence the run report records.

    Exit codes: 0 the claimed lane covers the diff, 1 it does not (the verdict
    line names the lane the diff needs), 2 tool or setup error, 3 no lane admits
    the diff — the lead escalates that.
.PARAMETER Lane
    Claimed lane: docs, tests, bug-fix, behavior-change, or tooling.
.PARAMETER Base
    Base revision. Defaults to the merge base with origin/main.
.PARAMETER Head
    Head revision, or "worktree" to include uncommitted changes. Defaults to HEAD.
.PARAMETER NoInstall
    Fail instead of installing when the SDK is missing.
.EXAMPLE
    .\scripts\lane-check.ps1 bug-fix
.EXAMPLE
    .\scripts\lane-check.ps1 tooling -Base a1b2c3d -Head worktree
#>
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet('docs', 'tests', 'bug-fix', 'behavior-change', 'tooling')]
    [string]$Lane,

    [string]$Base,

    [string]$Head = 'HEAD',

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
    # Write-Error would terminate under the Stop preference and lose the documented exit code
    [Console]::Error.WriteLine($_.Exception.Message)

    exit 2
}

$repositoryRoot = Get-ReihitsuRepositoryRoot

if (-not $Base)
{
    $Base = (& git -C $repositoryRoot merge-base origin/main HEAD)

    if ($LASTEXITCODE -ne 0 -or -not $Base)
    {
        [Console]::Error.WriteLine('lane-check.ps1: could not resolve a base revision. Pass -Base explicitly.')

        exit 2
    }
}

$diffArguments = @('diff', '--name-status', '--no-renames', $Base)

if ($Head -ne 'worktree') { $diffArguments += $Head }

$changes = @(& git -C $repositoryRoot @diffArguments)

if ($LASTEXITCODE -ne 0)
{
    [Console]::Error.WriteLine('lane-check.ps1: git diff failed.')

    exit 2
}

# Only exit code 0 proves anything: a failed proof and a tool error both leave every C# file classified by its path
$proofOutput = @(& (Join-Path $PSScriptRoot 'verify-text-only.ps1') -Base $Base -Head $Head -NoInstall:$NoInstall 2>&1)
$proofExitCode = $LASTEXITCODE
$proofLine = $proofOutput | Where-Object { "$_" -like 'TEXT-ONLY PROOF:*' } | Select-Object -First 1

if ($proofLine) { Write-Output "$proofLine" } else { Write-Output "TEXT-ONLY PROOF: not available (exit code $proofExitCode)" }

$textOnly = if ($proofExitCode -eq 0) { 'pass' } else { 'fail' }

$checker = Join-Path (Join-Path (Join-Path $repositoryRoot 'scripts') 'lane-check') 'lane-check.cs'

$changes | & dotnet run $checker --verbosity quiet --property Configuration=Debug --property NoWarn=RH0002 -- --lane $Lane --text-only $textOnly

switch ($LASTEXITCODE)
{
    0 { exit 0 }
    11 { exit 1 }
    13 { exit 3 }
    default { exit 2 }
}
