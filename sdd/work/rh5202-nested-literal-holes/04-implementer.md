Role: implementer
Commit: da441e9
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Decision 2 in `00-lead.md` (the code fix takes its line decision from the formatter's single owner, not from a private copy) set the shape of the change.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineUtilities.cs` (new, `internal static`) | The one statement of which lines after the opening line the move of a raw string literal writes. `MovesLine(root, line)` returns false for an empty line and for a line that `StartsInsideNestedText`. `StartsInsideNestedText` and `IsNonRawStringText` were moved here unchanged from `RawStringLineShifts`. | RH5202-004, RH5202-005, FMT-RAW-009, FMT-RAW-002 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs` | The two private predicates were removed. The constructor calls `RawStringLineUtilities.StartsInsideNestedText`. `CollectEdits` calls `RawStringLineUtilities.MovesLine` where it used to check `line.Span.Length == 0` on its own. Formatter behavior is the same: a nested-text line used to reach `CollectEdits` with a shift of 0 and was skipped as a no-op, and now it is skipped one check earlier. Summary of `CollectEdits` updated. | FMT-RAW-009 (unchanged behavior) |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.cs` | `RealignContent` (the private text-split copy) was removed. `ApplyCodeFixAsync` now walks the literal's lines in the document's `SourceText`. It skips each line for which `RawStringLineUtilities.MovesLine(root, line)` is false, and shifts every other line by the same delta as before: spaces inserted, or up to `-delta` leading spaces or tabs removed. It applies the shifts as `TextChange`s. The fix does not add FMT-RAW-008's distance for a nested raw string: it stays reported for its own fix. `GetLeadingWhitespaceCount` now reads from `SourceText`/`TextLine`. The summary was rewritten. | RH5202-003, RH5202-004, RH5202-005 |

The code fix does not use the full pipeline, so it applies no unrelated edits (FC-030; the B7 test pins the unwrapped argument list). `RawStringLineUtilities` is a `…Utilities` class used by no phase as a runner, so FMT-PIPE-009 allows it. `CodeFixFormatterAccessTests` passes. `RawStringLineShifts` itself stays banned for code fixes and is not used by them.

## Policy owners

- `rg -n "StartsInsideNestedText|IsNonRawStringText|MovesLine\(" src --glob '!*Test*'` finds the definitions only in `RawStringLineUtilities.cs`. The callers are `RawStringLineShifts.cs:75` and `:236`, and the code fix at `:77`. No other copy.
- `rg -ln "MultiLineRawStringLiteralToken" src --glob '!*Test*'` finds the RH5202 code fix and analyzer, `RawStringLineShifts`, and `RH5601UseTabsCorrectlyAnalyzer`. None of the others decides which lines of a literal move.

## Tests run

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5202"
#   Passed! - Failed: 0, Passed: 53, Total: 53
scripts/test.sh --configuration Debug --project analyzer
#   Passed! - Failed: 0, Passed: 7039, Total: 7039
scripts/test.sh --configuration Debug --project formatter
#   Passed! - Failed: 0, Passed: 4517, Total: 4517
scripts/test.sh --configuration Debug --project architecture
#   Passed! - Failed: 0, Passed: 115, Total: 115
```

`scripts/build.sh --configuration Debug` gives 0 errors. Its one warning (RH5603, the new file's final newline) was cleared by `scripts/format.sh` over the three changed files before the test runs.

## Disputed tests or scope

_None._ The scope stays within the components the handovers name, the code fix and the formatter's `RawStringLineShifts`. The only addition is the extracted utility class that decision 2 calls for. It has two consumers: the formatter and the RH5202 code fix.

## Architecture findings

_None._ The finding behind decision 2 is resolved: the line decision has one owner, `RawStringLineUtilities.MovesLine`, and both components call it.

## Follow-up candidates

_None._
