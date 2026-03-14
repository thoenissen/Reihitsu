# Task Completion Checklist

1. Build the analyzer project: `dotnet build Reihitsu.Analyzer\Reihitsu.Analyzer\Reihitsu.Analyzer.csproj`
2. Build the code fixes project if changed: `dotnet build Reihitsu.Analyzer\Reihitsu.Analyzer.CodeFixes\Reihitsu.Analyzer.CodeFixes.csproj`
3. Run all tests: `dotnet test Reihitsu.Analyzer\Reihitsu.Analyzer.Test\Reihitsu.Analyzer.Test.csproj`
4. If adding/modifying a rule, update README.MD rule table
5. Ensure no auto-generated files were manually edited
