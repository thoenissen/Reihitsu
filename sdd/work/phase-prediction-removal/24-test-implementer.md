Role: test implementer
Commit: b089af3
Verdict: tests written

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It asked me to check the claim in `23-implementer.md` that `ReihitsuFormatterHelpers.AdjustNodeIndentation` has no production caller left, and, if the claim holds, to remove the tests that exist only to exercise it.

## The claim, checked

The claim holds. Evidence:

- `rg -n "AdjustNodeIndentation|AdjustLeadingWhitespace" src --glob '*.cs'` on b089af3 finds:
  - the declaration `ReihitsuFormatterHelpers.AdjustNodeIndentation` (internal), at `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs:175`;
  - its private helper `AdjustLeadingWhitespace`, at line 341, which is called only from `AdjustNodeIndentation` (line 205);
  - six tests in `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterHelpersTests.cs`.
- No other source file calls either method, and nothing outside `src` references them as code. The remaining mentions are prose in the change doc, the decisions, and the handovers.
- So none of these tests reaches the method through a public entry point. Since 9081b4c, `FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync` no longer call it (FMT-PIPE-017).
- Node-level behavior is covered through the public entry points by `ReihitsuFormatterNodeColumnTests`, the node-level tests in `ReihitsuFormatterNodeBlankLineTests`, and the code-fix tests listed in `22-test-implementer.md`.

`ComputeTokenColumn` is a different case. It still has a production caller, `TargetAttributePlacementCodeFixProviderBase.cs:111`, so its tests stay.

## Tests

Removed from `Reihitsu.Formatter.Test.Unit.ReihitsuFormatterHelpersTests`. All six call `AdjustNodeIndentation` directly and carry no requirement tag:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `AdjustNodeIndentationShiftsIndentationRightByOffset` | — | direct call | removed |
| `AdjustNodeIndentationShiftsIndentationLeftByOffset` | — | direct call | removed |
| `AdjustNodeIndentationReturnsUnchangedNodeWhenNoTokensOnNewLines` | — | direct call | removed |
| `AdjustNodeIndentationDoesNotProduceNegativeIndentation` | — | direct call | removed |
| `AdjustNodeIndentationInsertsWhitespaceWhenLineStartsAtColumnZero` | — | direct call | removed |
| `AdjustNodeIndentationDoesNotInsertWhitespaceForFirstTokenNotGenuinelyAfterEndOfLine` | — | direct call | removed |

Nothing else in the file changed, and no other test references the method. The production method and `AdjustLeadingWhitespace` are left for the implementer to delete.

## Not covered, and why

Nothing is lost. The behavior these tests checked is no longer reachable from any entry point.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterHelpersTests"
```

On b089af3 plus this change: `Passed! - Failed: 0, Passed: 27, Total: 27`.
