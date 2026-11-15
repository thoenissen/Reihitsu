Role: cleaner
Commit: d1442d6
Verdict: clean

## thoenissen/Reihitsu#1105
Nothing to clean; see `## Whole run`.

## thoenissen/Reihitsu#1266
Nothing to clean; see `## Whole run`.

## thoenissen/Reihitsu#1267
Nothing to clean; see `## Whole run`.

## Whole run

### Formatted
_None._ (`scripts/format.sh`: Formatted 0 of 11 file(s).)

### Warnings resolved
_None._ No warnings in the Release build. (The format script's own CLI build prints S1125 at `src/Reihitsu.Cli/DiffGenerator.cs:134`, a file this run did not change.)

### Unresolved
_None._

### Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH720"`: Passed 237, Failed 0, Total 237.
- `scripts/test.sh --project formatter --filter "FullyQualifiedName~UsingDirective"`: Passed 171, Failed 0, Total 171.
