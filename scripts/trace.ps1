<#
.SYNOPSIS
    Traces which top-level formatter phases change one C# file across repeated passes.
.DESCRIPTION
    The input file is never modified. Relative paths are resolved against the caller's
    working directory, not the repository root.
.PARAMETER File
    C# file to trace.
.PARAMETER Passes
    Maximum number of formatting passes. Defaults to 3.
.PARAMETER NoInstall
    Fail instead of installing when the SDK is missing.
.EXAMPLE
    .\scripts\trace.ps1 example.cs
.EXAMPLE
    .\scripts\trace.ps1 example.cs -Passes 5 -NoInstall
#>
$ErrorActionPreference = 'Stop'

. (Join-Path (Join-Path $PSScriptRoot 'lib') 'dotnet-env.ps1')

$noInstall = $false
$pathsOnly = $false
$traceArguments = @()

for ($argumentIndex = 0; $argumentIndex -lt $args.Count; $argumentIndex++)
{
    $argument = [string]$args[$argumentIndex]

    if (-not $pathsOnly -and $argument -eq '--')
    {
        $pathsOnly = $true
        $traceArguments += $argument

        continue
    }

    if (-not $pathsOnly -and $argument -ieq '-NoInstall')
    {
        $noInstall = $true

        continue
    }

    if (-not $pathsOnly -and $argument -ieq '-Passes')
    {
        $traceArguments += '--passes'

        continue
    }

    $traceArguments += $argument
}

try
{
    Initialize-ReihitsuDotnet -NoInstall:$noInstall -Quiet
}
catch
{
    [Console]::Error.WriteLine($_.Exception.Message)

    exit 2
}

$repositoryRoot = Get-ReihitsuRepositoryRoot
$traceApp = Join-Path (Join-Path (Join-Path $repositoryRoot 'scripts') 'trace') 'trace.cs'

& dotnet run $traceApp --verbosity quiet --property NoWarn=RH0002 -- @traceArguments

$runnerExitCode = $LASTEXITCODE

switch ($runnerExitCode)
{
    0 { exit 0 }
    11 { exit 1 }
    2 { exit 2 }
    default { exit 2 }
}
