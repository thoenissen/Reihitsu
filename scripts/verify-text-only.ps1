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
.PARAMETER StrictDocs
    Also fail when public API documentation changed. Plain success does not
    establish that, because a documentation edit is a legitimate comment-only
    change.
.PARAMETER NoInstall
    Fail instead of installing when the SDK is missing.
.EXAMPLE
    .\scripts\verify-text-only.ps1
.EXAMPLE
    .\scripts\verify-text-only.ps1 -Base a1b2c3d -Head worktree -StrictDocs
#>
param(
    [string]$Base,

    [string]$Head,

    [switch]$StrictDocs,

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

$arguments = @()

if ($Base) { $arguments += @('--base', $Base) }
if ($Head) { $arguments += @('--head', $Head) }
if ($StrictDocs) { $arguments += '--strict-docs' }

$repositoryRoot = Get-ReihitsuRepositoryRoot
$proof = Join-Path (Join-Path $repositoryRoot 'scripts') (Join-Path 'proof' 'verify-text-only.cs')

# The tool inspects the repository this script belongs to, whatever the caller's working directory is
& dotnet run $proof -- --repository $repositoryRoot @arguments

exit $LASTEXITCODE
