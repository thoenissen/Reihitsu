<#
.SYNOPSIS
    Recursively removes all "bin" and "obj" directories under the specified path (default: repository root).
.PARAMETER Path
    Base directory to search under. Defaults to the parent directory of this script (assumed repo root).
.PARAMETER DryRun
    Lists directories that would be deleted without removing them.
.EXAMPLE
    .\scripts\clean-bin-obj.ps1 -DryRun
#>
param(
    [Parameter(Position=0)]
    [string]$Path = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path,

    [switch]$DryRun
)

Write-Host "Search path: $Path"

# Find 'bin' or 'obj' directories recursively
$targets = Get-ChildItem -Path $Path -Directory -Recurse -Force -ErrorAction SilentlyContinue |
           Where-Object { $_.Name -ieq 'bin' -or $_.Name -ieq 'obj' }

if (-not $targets) {
    Write-Host "No 'bin' or 'obj' directories found under $Path."
    exit 0
}

# Remove from deepest paths first so parents are not removed before children
$targets = $targets | Sort-Object { $_.FullName.Length } -Descending

foreach ($d in $targets) {
    if ($DryRun) {
        Write-Host "DRYRUN: $($d.FullName)"
    } else {
        Write-Host "Removing: $($d.FullName)"
        try {
            Remove-Item -LiteralPath $d.FullName -Recurse -Force -ErrorAction Stop
        } catch {
            Write-Warning "Error removing $($d.FullName): $_"
        }
    }
}

Write-Host "Done."
