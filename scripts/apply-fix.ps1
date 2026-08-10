<#
.SYNOPSIS
    Runs the code-fix surface of one diagnostic ID over a directory of fixtures.
.DESCRIPTION
    Every C# file below the directory is analyzed and fixed under LF and under CRLF, and each
    arm reports a status token plus the unified diff the fix produced. The fixtures are only
    read. Relative paths are resolved against the caller's working directory, not the
    repository root, so the fixture directory can live anywhere — a read-only workflow keeps
    it outside the repository and removes it afterwards.

    Exit codes: 0 every fixture reached a supported end state (including "no diagnostic" and
    "no fix offered"), 1 a fixture never stopped reporting the diagnostic, 2 the command could
    not run. A tool failure therefore never reads as a clean sweep.
.PARAMETER DiagnosticId
    Diagnostic ID whose analyzer and code fix provider are exercised, for example RH7101.
.PARAMETER FixtureDirectory
    Directory holding the fixtures. Searched recursively for *.cs.
.PARAMETER MaxIterations
    Maximum number of code actions applied per fixture. Defaults to 10.
.PARAMETER NoInstall
    Fail instead of installing when the SDK is missing.
.EXAMPLE
    .\scripts\apply-fix.ps1 RH7101 C:\Temp\sweep
.EXAMPLE
    .\scripts\apply-fix.ps1 RH7101 C:\Temp\sweep -MaxIterations 3 -NoInstall
#>
$ErrorActionPreference = 'Stop'

. (Join-Path (Join-Path $PSScriptRoot 'lib') 'dotnet-env.ps1')

$noInstall = $false
$positionalOnly = $false
$runnerArguments = @()

for ($argumentIndex = 0; $argumentIndex -lt $args.Count; $argumentIndex++)
{
    $argument = [string]$args[$argumentIndex]

    if (-not $positionalOnly -and $argument -eq '--')
    {
        $positionalOnly = $true
        $runnerArguments += $argument

        continue
    }

    if (-not $positionalOnly -and $argument -ieq '-NoInstall')
    {
        $noInstall = $true

        continue
    }

    if (-not $positionalOnly -and $argument -ieq '-MaxIterations')
    {
        $runnerArguments += '--max-iterations'

        continue
    }

    $runnerArguments += $argument
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
$runnerApp = Join-Path (Join-Path (Join-Path $repositoryRoot 'scripts') 'apply-fix') 'apply-fix.cs'

& dotnet run $runnerApp --verbosity quiet --property Configuration=Debug --property NoWarn=RH0002 -- @runnerArguments

$runnerExitCode = $LASTEXITCODE

switch ($runnerExitCode)
{
    0 { exit 0 }
    11 { exit 1 }
    2 { exit 2 }
    default { exit 2 }
}
