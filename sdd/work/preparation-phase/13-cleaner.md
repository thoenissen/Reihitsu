Role: cleaner
Commit: see the commit that adds this file
Verdict: clean

## Formatted
_None._ The formatter changed 0 of the 3 files the round touched, plus ControlFlowBraceTransform.cs.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| Auditor finding 1 (comment) | ControlFlowBraceTransform.cs:53 | `endOfLine` param doc now names the opening brace, each kept leading comment, and the closing brace |

## Unresolved
_None._

## Checks
scripts/build.sh: 0 Warning(s), 0 Error(s). The S1125 line in FormatCommandHandler.cs showed only in the formatter's own build output, not in the Release build.
