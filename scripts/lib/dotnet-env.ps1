<#
.SYNOPSIS
    Shared .NET environment helper for the Reihitsu repository scripts.
.DESCRIPTION
    Dot-source this file, do not execute it:

        . "$PSScriptRoot/lib/dotnet-env.ps1"
        Initialize-ReihitsuDotnet

    The helper keeps every repository script working the same way on a machine
    without an SDK and on one where the SDK is preinstalled: it probes first and
    installs only when the required SDK is genuinely missing.
#>

$script:ReihitsuDotnetChannel = '10.0'
$script:ReihitsuDotnetMajor = '10'
$script:ReihitsuDotnetRoot = if ($env:REIHITSU_DOTNET_ROOT) { $env:REIHITSU_DOTNET_ROOT } else { Join-Path $HOME '.dotnet' }

function Get-ReihitsuRepositoryRoot
{
    <#
    .SYNOPSIS
        Returns the absolute path of the repository root.
    #>
    (Resolve-Path -LiteralPath (Join-Path (Join-Path $PSScriptRoot '..') '..')).Path
}

function Test-ReihitsuDotnet
{
    <#
    .SYNOPSIS
        Returns $true when a dotnet on PATH reports an SDK of the required major version.
    #>
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue))
    {
        return $false
    }

    $sdks = & dotnet --list-sdks 2>$null

    return [bool]($sdks | Where-Object { $_ -like "$script:ReihitsuDotnetMajor.*" })
}

function Initialize-ReihitsuDotnet
{
    <#
    .SYNOPSIS
        Ensures a usable SDK is on PATH.
    .PARAMETER NoInstall
        Fail instead of installing when the SDK is missing.
    .PARAMETER Quiet
        Suppress the status line.
    #>
    param(
        [switch]$NoInstall,
        [switch]$Quiet
    )

    if (Test-ReihitsuDotnet)
    {
        if (-not $Quiet) { Write-Host "dotnet: using $((Get-Command dotnet).Source) ($(& dotnet --version))" }

        return
    }

    $privateDotnet = Join-Path $script:ReihitsuDotnetRoot 'dotnet.exe'

    if (-not (Test-Path -LiteralPath $privateDotnet))
    {
        $privateDotnet = Join-Path $script:ReihitsuDotnetRoot 'dotnet'
    }

    if (Test-Path -LiteralPath $privateDotnet)
    {
        $env:PATH = "$script:ReihitsuDotnetRoot$([IO.Path]::PathSeparator)$env:PATH"
        $env:DOTNET_ROOT = $script:ReihitsuDotnetRoot

        if (Test-ReihitsuDotnet)
        {
            if (-not $Quiet) { Write-Host "dotnet: using $privateDotnet ($(& dotnet --version))" }

            return
        }
    }

    if ($NoInstall)
    {
        throw "dotnet: no .NET $script:ReihitsuDotnetMajor SDK found and -NoInstall was requested."
    }

    Write-Host "dotnet: no .NET $script:ReihitsuDotnetMajor SDK found, installing channel $script:ReihitsuDotnetChannel into $script:ReihitsuDotnetRoot"

    $installer = Join-Path ([IO.Path]::GetTempPath()) 'dotnet-install.ps1'

    Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile $installer -UseBasicParsing
    & $installer -Channel $script:ReihitsuDotnetChannel -InstallDir $script:ReihitsuDotnetRoot
    Remove-Item -LiteralPath $installer -ErrorAction SilentlyContinue

    $env:PATH = "$script:ReihitsuDotnetRoot$([IO.Path]::PathSeparator)$env:PATH"
    $env:DOTNET_ROOT = $script:ReihitsuDotnetRoot

    if (-not (Test-ReihitsuDotnet))
    {
        throw "dotnet: installation finished but no $script:ReihitsuDotnetMajor.* SDK is visible."
    }

    if (-not $Quiet) { Write-Host "dotnet: installed $(& dotnet --version) in $script:ReihitsuDotnetRoot" }
}
