<#
.SYNOPSIS
    Builds the Reihitsu solution in Release configuration.
.PARAMETER NoInstall
    Fail instead of installing when the SDK is missing.
.PARAMETER BuildArguments
    Additional arguments passed to dotnet build.
.EXAMPLE
    .\scripts\build.ps1
#>
param(
    [switch]$NoInstall,

    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$BuildArguments = @()
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'lib' 'dotnet-env.ps1')

Initialize-ReihitsuDotnet -NoInstall:$NoInstall -Quiet

Push-Location (Get-ReihitsuRepositoryRoot)

try
{
    & dotnet build Reihitsu.sln -c Release --verbosity minimal @BuildArguments

    if ($LASTEXITCODE -ne 0)
    {
        exit $LASTEXITCODE
    }
}
finally
{
    Pop-Location
}
