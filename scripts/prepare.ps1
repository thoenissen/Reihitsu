<#
.SYNOPSIS
    Prepares the shell for building, testing, and formatting Reihitsu.
.DESCRIPTION
    Installs the required .NET SDK only when it is missing, so the script is a
    no-op verification on machines that already ship it.
.PARAMETER NoInstall
    Fail instead of installing when the SDK is missing.
.PARAMETER Quiet
    Suppress the status line.
.EXAMPLE
    .\scripts\prepare.ps1
#>
param(
    [switch]$NoInstall,
    [switch]$Quiet
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'lib' 'dotnet-env.ps1')

Initialize-ReihitsuDotnet -NoInstall:$NoInstall -Quiet:$Quiet
