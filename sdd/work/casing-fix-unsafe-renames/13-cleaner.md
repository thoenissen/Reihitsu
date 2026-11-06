Role: cleaner
Commit: HEAD
Verdict: clean

## Whole run
### Formatted
_None._ (formatter changed 0 of 22 changed C# files)

### Warnings resolved
_None._

### Unresolved
_None._ (S1125 warnings printed by format.sh are in the unchanged Cli project; the Release build shows none.)

### Checks
- scripts/build.sh: 0 Warning(s), 0 Error(s)
- scripts/test.sh --project analyzer --filter "FullyQualifiedName~Naming": Passed! Failed: 0, Passed: 544, Total: 544
