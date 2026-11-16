Role: cleaner
Commit: 18da494
Verdict: clean

## Whole run

### Formatted
_None._ `scripts/format.sh` on the four changed C# files: "Formatted 0 of 1 file(s)" and "Formatted 0 of 3 file(s)".

### Warnings resolved
_None needed._

### Unresolved
_None._

### Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project tooling`: Passed 133, Failed 0.
- `scripts/test.sh --project architecture`: Passed 119, Failed 0.

### Follow-up candidates
`src/Reihitsu.Cli/FormatCommandHandler.cs(501,41)`: S1125 appears as a warning when `scripts/format.sh` builds the CLI; a file this run did not change. The Release build is clean.

## thoenissen/Reihitsu#1470
Formatted: _None._ (docs only). Warnings: _None._ Checks: see Whole run.

## thoenissen/Reihitsu#1471
Formatted: _None._ `LaneClassifier.cs` already formatted. Warnings: _None._ Tooling tests pass (133).

## thoenissen/Reihitsu#1479
Docs only; nothing to format or build. _None._

## thoenissen/Reihitsu#1544
Docs only; architecture tests pass (119). _None._
