Role: cleaner
Commit: ad2763e
Verdict: clean

## Whole run
### Formatted
_None._ (formatter changed 0 of 20 changed C# files)

### Warnings resolved
_None._

### Unresolved
_None._ (S1125 warnings printed during format.sh were in unchanged Cli files; the Release build shows none.)

### Checks
- scripts/build.sh: Build succeeded. 0 Warning(s), 0 Error(s)
- scripts/test.sh --project analyzer --filter "FullyQualifiedName~Naming": Passed! Failed: 0, Passed: 519, Total: 519
