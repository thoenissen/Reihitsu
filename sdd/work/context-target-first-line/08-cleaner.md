Role: cleaner
Commit: see git log (this handover's commit)
Verdict: clean

## Formatted
_None._ The formatter changed 0 of 9 changed C# files.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| none | | |

Comment corrected: the summary of `LayoutModel.UseTargetFirstToken` now says the target's lines are laid out as if its first token stood in the given column, and that a target starting its line gets that column from the entry point's write-back of the leading trivia.

## Unresolved
_None._

## Checks
- scripts/build.sh: 0 Warning(s), 0 Error(s).
- Formatter node-column tests: see run line in the report.
- Text-only: `--strict-docs` reports FAIL by design (public API documentation changed in LayoutModel.cs); the non-strict run is the proof that no compiled code changed.
