# Suggested Commands

## Build
```powershell
dotnet build Reihitsu.sln -c Release --verbosity minimal
```

## Test
```powershell
dotnet test Reihitsu.Analyzer.Test\Reihitsu.Analyzer.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Formatter.Test\Reihitsu.Formatter.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Core.Test\Reihitsu.Core.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Cli.Test\Reihitsu.Cli.Test.csproj -c Release --verbosity minimal
```

## Full Solution Build
```powershell
dotnet build Reihitsu.sln -c Release --verbosity minimal
```

## Git
```powershell
git --no-pager status
git --no-pager diff
```
