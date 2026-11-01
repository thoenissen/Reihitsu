Role: cleaner
Commit: 292b2ef
Verdict: clean

## Formatted
_None._ All 56 changed C# files were already formatted.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/format.sh <56 changed .cs files> --check`: `0 of 56 file(s) need formatting.`
- `scripts/build.sh`: `Build succeeded. 0 Warning(s) 0 Error(s)`.
- Comment drift (FC-031): the summaries and inline comments of the changed formatter code (`HorizontalSpacingRewriter.NormalizeGapBehindLeadingComment`, the 12 new rules, `SpacingPolicy` precedence list vs. its registration order, `KeywordSpacingRule`, `NoSpaceSpacingRule`, `AccessorListLayout`, `PropertyLayoutLineBreakRewriter`, the join rewriters) describe the new code and FMT-SPACE-002/-016 to -027 and FMT-BREAK-031. No drift found, no comment changed.
- No code changed, so no focused tests were re-run.
