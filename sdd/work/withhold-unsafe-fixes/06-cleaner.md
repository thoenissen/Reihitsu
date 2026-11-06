Role: cleaner
Commit: 09956d97
Verdict: clean

## Whole run
Issues: #1257, #1279, #1281, #1282, #1417, #1422. The tree is clean as a whole; no per-issue differences.

## Formatted
_None._ `scripts/format.sh --force` over the 29 changed C# files: "Formatted 0 of 29 file(s)."

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- scripts/build.sh: Build succeeded. 0 Warning(s), 0 Error(s).
- analyzer focused (RH3001, 3004, 3005, 3101, 3103-3107, 3201, 5417): Passed! Failed 0, Passed 382.
- core focused (RewriteParseUtilitiesTests): Passed! Failed 0, Passed 14.
- Text-only proof: not applicable (no comment-only fix).
