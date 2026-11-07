Role: cleaner
Commit: see git log (this commit)
Verdict: clean

## Formatted
- src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs (after the member move below)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| RH7110 (accessibility order) x2 | RawStringLineShifts.cs, `GetLeadingWhitespaceLength` stood before public `MoveLines`/`GetShift` | Moved the private static helper behind the public members |
| RH1003 (use interpolation) | Reihitsu.Formatter.Test/Regression/BlankLines/CommentBeforeBraceBlankLineTests.cs:968 | Concatenation replaced by an interpolated string, same value |

## Unresolved
_None._

## Checks
- scripts/build.sh: 0 Warning(s), 0 Error(s)
- scripts/test.sh --project formatter: Passed 4692, Failed 0
- scripts/test.sh --project analyzer (RH5202|RH5111|RH5601): Passed 241, Failed 0
- Other 31 changed C# files: formatter changed nothing.
