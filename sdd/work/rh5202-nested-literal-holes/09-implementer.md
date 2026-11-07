Role: implementer
Commit: a56c9f1
Verdict: tests pass

Repair round 1, for the red tests of `08-test-implementer.md` under decisions 6, 7 and 8 of `00-lead.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/RawStringLiteralUtilities.cs` | New `GetLeadingWhitespaceLength(string lineText)`: the number of leading characters for which `char.IsWhiteSpace` holds. The one statement of the width of the whitespace in front of the closing quotes, shared by the analyzer and the fix (decision 6). | RH5202-001, RH5202-003 |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.cs` | Both closing-column measurements (`Length - TrimStart().Length`, interpolated and plain/`u8`) call `GetLeadingWhitespaceLength`. Same result: `TrimStart()` trims exactly the `char.IsWhiteSpace` characters. | RH5202-001 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.cs` | The private `GetLeadingWhitespaceCount` (spaces and tabs only) was removed. The closing width and the leftward removal both use `GetLeadingWhitespaceLength`, so a closing line that starts with U+00A0, U+000B, U+000C, U+2003 converges in one application in both directions (decisions 6, 8). Lines are skipped when `RawStringLineUtilities.StartsInsideNestedText` holds or when the new private `IsEmptyTextLine` holds: an empty line that lies within a token's span, i.e. an empty line of literal text (the outer literal's content, or a nested raw string's content). An empty line in the trivia of hole code is shifted again, as on `main` (decision 7). Summary of `ApplyCodeFixAsync` rewritten. | RH5202-003, RH5202-004, RH5202-005 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineUtilities.cs` | Class summary only: `StartsInsideNestedText` is the shared statement; `MovesLine` (which skips every empty line) is now the indentation phase's decision alone. No code change; formatter output unchanged. | FMT-RAW-002, FMT-RAW-009 |

## Policy owners

- Closing width: `rg -n "TrimStart\(\)|GetLeadingWhitespace|GetLeadingSpaceCount" src --glob '!*Test*'` around raw strings finds `RawStringLiteralUtilities.GetLeadingWhitespaceLength` (Core, called by the RH5202 analyzer twice and the RH5202 fix twice) and `RawStringLineShifts.GetLeadingSpaceCount` (formatter, spaces only after tab replacement). The formatter's copy stays, per decision 6; it is the follow-up candidate already recorded in `00-lead.md`.
- Lines left as written: `rg -n "StartsInsideNestedText|MovesLine\(" src --glob '!*Test*'` — defined only in `RawStringLineUtilities`; called by `RawStringLineShifts` (`:75`, `:236`) and the fix (`StartsInsideNestedText`). The empty-line decision now differs by component as decision 7 sets: the formatter keeps every empty line empty (`MovesLine`), the fix keeps only empty lines of literal text empty (`IsEmptyTextLine`, its only owner).

## Tests run

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5202"
#   Passed! - Failed: 0, Passed: 67, Total: 67
scripts/test.sh --configuration Debug --project analyzer
#   Passed! - Failed: 0, Passed: 7053, Total: 7053
scripts/test.sh --configuration Debug --project formatter
#   Passed! - Failed: 0, Passed: 4519, Total: 4519
scripts/test.sh --configuration Debug --project core
#   Passed! - Failed: 0, Passed: 649, Total: 649
scripts/test.sh --configuration Debug --project architecture
#   Passed! - Failed: 0, Passed: 115, Total: 115
```

`scripts/format.sh` over the four changed files changed nothing; the Debug build shows no warning or error in them.

## Disputed tests or scope

- New public member `RawStringLiteralUtilities.GetLeadingWhitespaceLength` in `Reihitsu.Core` (decision 6 asks for the shared statement; Core already holds `GetQuoteOffset`, which both the analyzer and the fix use). It has no direct Core unit test; the RH5202 analyzer tests cover it.
- `IsEmptyTextLine` treats an empty line inside a nested raw string's content (in a hole) as literal text, so it stays empty, matching the formatter. No test pins that cell; on `main` the fix wrote spaces onto it.

## Architecture findings

_None._ The finding behind decision 6 is resolved for the analyzer and the fix; the formatter's measurement is the recorded follow-up candidate.

## Follow-up candidates

_None_ beyond the one in `00-lead.md`.
