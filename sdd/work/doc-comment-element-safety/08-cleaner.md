Role: cleaner
Commit: 145086ea8cc7a450db4d7549e9bdbc1287e217de
Verdict: clean

## Formatted
_None._ `scripts/format.sh --force` over the 30 changed C# files: "Formatted 0 of 30 file(s)." (The format script's own CLI build prints S1125 warnings for Reihitsu.Cli; those files are not in the run's diff and the Release solution build is warning-free.)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).

### #1329
Formatter DocumentationCommentWithoutOwnEndTagTests: part of 56 passed, 0 failed (formatter filter below).

### #1389
Formatter StrandedDocumentationCommentTextTests, ByteOrderMarkTests, CleanupPhaseTests: 56 passed, 0 failed combined with #1329.

### #1241
Analyzer RH8107/RH8202/RH8204/RH8305/RH8307 filters: part of 205 passed, 0 failed.

### #1344
Core DocumentationCommentUtilitiesTests: 44 passed, 0 failed. Analyzer RH8304/RH8309 in the 205 above.

### #1242
Spec-only; no C# in scope. Nothing to format or test.

## Whole run
Format: 0 of 30 files changed. Build: 0 warnings, 0 errors. Tests: formatter 56 passed, core 44 passed, analyzer (RH8107, 8202, 8204, 8301, 8304, 8305, 8307, 8309) 205 passed; none failed.
