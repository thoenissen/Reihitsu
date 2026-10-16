<#
.SYNOPSIS
    Builds the Reihitsu solution, in Release configuration unless told otherwise.
.DESCRIPTION
    The solution is addressed by absolute path, so the caller's working
    directory is never changed and relative arguments keep their meaning.
.PARAMETER Configuration
    Build configuration: Release (default) or Debug.
.PARAMETER NoInstall
    Fail instead of installing when the SDK is missing.
.PARAMETER BuildArguments
    Additional arguments passed to dotnet build.
.EXAMPLE
    .\scripts\build.ps1
.EXAMPLE
    .\scripts\build.ps1 -Configuration Debug
#>
param(
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',

    [switch]$NoInstall,

    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$BuildArguments = @()
)

$ErrorActionPreference = 'Stop'

. (Join-Path (Join-Path $PSScriptRoot 'lib') 'dotnet-env.ps1')

Initialize-ReihitsuDotnet -NoInstall:$NoInstall -Quiet

$solution = Join-Path (Get-ReihitsuRepositoryRoot) 'Reihitsu.sln'

& dotnet build $solution -c $Configuration --verbosity minimal @BuildArguments

exit $LASTEXITCODE
