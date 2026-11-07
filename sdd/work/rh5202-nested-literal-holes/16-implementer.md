Role: implementer
Commit: see the commit that adds this file (parent d79f2d7)
Verdict: tests pass

This is repair round 2, for the 4 red tests of `15-test-implementer.md` under decision 10 of `00-lead.md`. Decision 2 (one owner for the line decision) shapes the change. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.cs` | The line loop now skips a line when `RawStringLineUtilities.MovesLine(root, line)` is false. That is the indentation phase's own decision: an empty line is never written, and neither is a line that starts inside nested text. Decision 10 makes the fix's empty-line decision equal to the formatter's, so the private `IsEmptyTextLine` (decision 7's split between literal text and hole code) was removed. A whitespace-only line still has a non-empty span, so it still shifts, as decision 11 keeps it. The summary of `ApplyCodeFixAsync` was rewritten. | RH5202-003, RH5202-004, RH5202-005 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineUtilities.cs` | Class summary only: `MovesLine` is again the one statement that both the phase and the fix follow. No code change, and formatter output is unchanged. | FMT-RAW-002, FMT-RAW-009 |

## Policy owners
- Lines left as written: `rg -n "IsEmptyTextLine|StartsInsideNestedText|MovesLine\(" src --glob '!*Test*'`. `MovesLine` and `StartsInsideNestedText` are defined only in `RawStringLineUtilities`. `MovesLine` is called by `RawStringLineShifts.cs:236` and by the fix (`:80`). `StartsInsideNestedText` is also called by `RawStringLineShifts.cs:75`, which builds the nested-literal shift map. No private copy remains.
- Closing width: unchanged since `09-implementer.md` (`RawStringLiteralUtilities.GetLeadingWhitespaceLength`).

## Tests run
```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5202"
#   Passed! - Failed: 0, Passed: 73, Total: 73
scripts/test.sh --configuration Debug --project analyzer
#   Passed! - Failed: 0, Passed: 7059, Total: 7059
scripts/test.sh --configuration Debug --project formatter
#   Passed! - Failed: 0, Passed: 4519, Total: 4519
scripts/test.sh --configuration Debug --project architecture
#   Passed! - Failed: 0, Passed: 115, Total: 115
scripts/test.sh --configuration Debug --project core
#   Passed! - Failed: 0, Passed: 660, Total: 660
```
`scripts/format.sh` changed nothing in either file. The Debug build has 0 errors and no warning in the changed files.

## Disputed tests or scope
_None._ The change shrinks the fix and adds no member.

## Architecture findings
_None._ The empty-line decision has one owner again.

## Follow-up candidates
_None_ beyond `00-lead.md`.
