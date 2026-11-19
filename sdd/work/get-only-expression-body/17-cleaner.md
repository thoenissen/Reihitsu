Role: cleaner
Commit: see git log (this commit)
Verdict: clean

## Formatted
_None._ `scripts/format.sh` formatted 0 of 17 changed C# files.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| RH7110 (error, 3x) | `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` | The private `IsGapToSiblingMember` and `IsCommentGapToSiblingMember` sat between public methods; moved them behind the last public method. Members only reordered, bodies unchanged. |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter RH3209|RH3210|RH3202`: Passed 208 of 208.
- Text-only proof: not applicable as a pure text proof; `verify-text-only.sh` reports BEHAVIORAL for the member reorder (token order differs), which is expected for a move of members.

## Follow-up candidates
_None._
