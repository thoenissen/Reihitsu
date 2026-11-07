Role: implementer
Commit: see the commit that adds this file (parent 2917e96)
Verdict: tests pass

Repair round 3, for the 12 red tests of `21-test-implementer.md` under decision 12 (audit 2, finding #3; maintainer answer Q2 = A) and RH5202-006. Decisions 2 and 6 still hold: the fix's line decision stays `RawStringLineUtilities.MovesLine`, and the closing width stays `RawStringLiteralUtilities.GetLeadingWhitespaceLength`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.cs` | The shift computation moved out of `ApplyCodeFixAsync` into `GetShift`, which both registration and application use, so the guard and the rewrite measure one shift. New `ChangesValueOfNestedRawString`: for a leftward shift `d`, it visits every multi-line raw string literal among the outer literal's descendants (so every hole, every depth, plain/`u8`/interpolated with any number of `$`). It skips a nested literal whose closing whitespace `k >= d`; otherwise it reports a value change when a line strictly between the nested opening and closing lines, not starting in one of the nested literal's own holes (`StartsInOwnHole`), has more than `k` leading whitespace characters (a whitespace-only line counts with all of them, because `GetLeadingWhitespaceLength` counts the whole line). `RegisterCodeFixesAsync` does not register the action for an interpolated outer literal when it does. The rewrite itself is unchanged. Summaries of `ApplyCodeFixAsync` and the new methods describe the code. | RH5202-006, RH5202-003, RH5202-004 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineUtilities.cs` | Gains `IsMultiLineRawStringLiteral`, moved unchanged from `RawStringLineShifts`; class summary mentions it. | FMT-RAW-008 (unchanged behavior) |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs` | `IsMultiLineRawStringLiteral` removed; its three callers call `RawStringLineUtilities.IsMultiLineRawStringLiteral`. No behavior change; formatter tests unchanged and green. | — |

## Policy owners
- Multi-line raw string predicate: `rg -n "Utf8MultiLineRawStringLiteralToken" src --glob '!*Test*'` found `RawStringLineShifts.IsMultiLineRawStringLiteral` (now in `RawStringLineUtilities`), `RH5601UseTabsCorrectlyAnalyzer.cs:66` (token-level, tab rule), the RH5202 analyzer `:55` (token-level report filter), and the RH5202 fix's own registration branches. The fix first called `RawStringLineShifts.IsMultiLineRawStringLiteral` directly, which `CodeFixesDoNotDependOnFormatterPhasesTransformsOrThePipeline` rejects (FMT-PIPE-009: `RawStringLineShifts` is run by the indentation phase). Instead of a private copy, the predicate moved to `RawStringLineUtilities`, the helper the fix already calls. The registration branches in the fix and the analyzer's token filter are pre-existing and untouched.
- Closing width: `RawStringLiteralUtilities.GetLeadingWhitespaceLength` only (`rg -n "GetLeadingWhitespace" src --glob '!*Test*'`); the guard reuses it for closing and content lines.
- Lines the rewrite writes: unchanged, `RawStringLineUtilities.MovesLine`.
- "Starts in one of its own holes": `rg -n "StartsInOwnHole|InterpolationSyntax" src/Reihitsu.Analyzer.CodeFixes src/Reihitsu.Formatter/Pipeline/Indentation` found no other statement of "line start within an `InterpolationSyntax` of a given literal"; `StartsInOwnHole` is private to the fix. It only filters lines whose start lies in a hole; such a line holding text of a deeper nested raw string is checked when that deeper literal is visited.

## Tests run
```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5202"
#   Passed! - Failed: 0, Passed: 93, Total: 93
scripts/test.sh --configuration Debug --project analyzer
#   Passed! - Failed: 0, Passed: 7079, Total: 7079
scripts/test.sh --configuration Debug --project formatter
#   Passed! - Failed: 0, Passed: 4519, Total: 4519
scripts/test.sh --configuration Debug --project architecture
#   Passed! - Failed: 0, Passed: 115, Total: 115
scripts/test.sh --configuration Debug --project core
#   Passed! - Failed: 0, Passed: 660, Total: 660
```
`scripts/format.sh` changed nothing in the three files. The Debug build reports no warning in them.

## Disputed tests or scope
- **Scope:** `IsMultiLineRawStringLiteral` moved from `RawStringLineShifts` (formatter) to `RawStringLineUtilities`, a shared helper with other consumers. It is a move, not a new member; formatter output is unchanged. Alternative was a private copy in the fix (FC-015), which I rejected.
- **Input dimensions the tests do not vary:** the guard does not branch on the number of `$` (the predicate keys on `InterpolatedMultiLineRawStringStartToken`, which covers every `$` count), so the missing `$$"""` test exercises no separate path. It also does not branch on the line ending (it reads `SourceText.Lines`), so the missing CRLF variants for `u8` and depth 2 exercise no separate path. A nested literal whose content line starts with a hole at column 0 is classified as hole-start; its width 0 can never exceed `k`, so the classification does not affect the result.
- No test disputed.

## Architecture findings
_None._ The guard and the rewrite share `GetShift`, the closing width, and the raw-string predicate; the only new decision (line starts in the literal's own hole) has one owner.

## Follow-up candidates
_None_ beyond `00-lead.md`.
