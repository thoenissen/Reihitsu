# Installs or updates the latest Reihitsu.Cli nupkg
$PackageId = 'Reihitsu.Cli'
$relativePath = '..\Reihitsu.Cli\bin\Release'
$pkgFolderResolved = Resolve-Path -Path (Join-Path $PSScriptRoot $relativePath) -ErrorAction SilentlyContinue
if (-not $pkgFolderResolved) {
    Write-Error "Package folder not found at relative path '$relativePath' (from script folder '$PSScriptRoot')."
    exit 1
}
$PackageFolder = $pkgFolderResolved.Path

# Find nupkg files
$files = Get-ChildItem -Path $PackageFolder -Filter "*.nupkg" -File -ErrorAction SilentlyContinue
if (-not $files) {
    Write-Error "No .nupkg files found in '$PackageFolder'."
    exit 1
}

# Try to parse id and version from filename <id>.<version>.nupkg
$pkgs = $files | ForEach-Object {
    $name = [IO.Path]::GetFileNameWithoutExtension($_.Name)
    if ($name -match '^(?<id>.+)\.(?<ver>\d+\.\d+\.\d+(?:[.-][0-9A-Za-z]+)*)$') {
        [PSCustomObject]@{ File = $_; Id = $matches.id; Version = $matches.ver }
    } else {
        [PSCustomObject]@{ File = $_; Id = $null; Version = $null }
    }
}

# Prefer packages that match the PackageId (if parsed). If none parsed, use newest by LastWriteTime
$matchingParsed = $pkgs | Where-Object { $_.Id -and $_.Id -ieq $PackageId }
if ($matchingParsed) {
    $selected = $matchingParsed | Sort-Object -Property @{Expression = {
        $v = ($_).Version -replace '[^0-9.]',''
        try {[version]$v} catch {[version]'0.0.0'}
    }} -Descending | Select-Object -First 1
} else {
    $selected = $pkgs | Sort-Object -Property { $_.File.LastWriteTime } -Descending | Select-Object -First 1
    if (-not $selected.Version) {
        $fn = [IO.Path]::GetFileNameWithoutExtension($selected.File.Name)
        if ($fn -match '\.(?<ver>\d+\.\d+\.\d+(?:[.-][0-9A-Za-z]+)*)$') {
            $selected.Version = $matches.ver
        }
    }
}

$pkgPath = $selected.File.FullName
$version = $selected.Version
Write-Host "Selected package:`n  $pkgPath`n  Version: $version"

# Check installed global tool version (if any)
$installed = dotnet tool list --global 2>$null | ForEach-Object {
    $cols = ($_ -split '\s+') | Where-Object { $_ -ne '' }
    if ($cols.Count -ge 2 -and $cols[0] -ieq $PackageId) {
        [PSCustomObject]@{ Name = $cols[0]; Version = $cols[1] }
    }
} | Select-Object -First 1

try {
    if (-not $installed) {
        Write-Host "Installing $PackageId $version as a global tool..."
        dotnet tool install --global $PackageId --version $version --add-source "$PackageFolder"
    } elseif ($installed.Version -ne $version) {
        Write-Host "Updating $PackageId from $($installed.Version) to $version..."
        dotnet tool update --global $PackageId --version $version --add-source "$PackageFolder"
    } else {
        Write-Host "$PackageId $version is already installed globally. Nothing to do."
    }
} catch {
    Write-Error "dotnet tool operation failed: $_"
    exit 2
}
