Role: implementer
Commit: 953e1c5
Verdict: tests pass

My prompt named the issue, the handovers, and decision #12 (also #8) as the change asked for; it carried no suspected cause or hoped-for verdict. This round carries out decision #12, a structural change with no behavior change. No test was added or changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Core/Utilities/BlockStartUtilities.cs` | New neutral owner `BlockStartUtilities.IsFirstInBlock(PrecedingTokenFacts)`: no preceding token, an opening brace, or a switch-label colon. The body is moved unchanged from `BlankLineEditor`. | FMT-DOC-001, FMT-BLANK-012 (decision #12) |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BlankLineEditor.cs` | Both `IsFirstInBlock` overloads removed. The `SyntaxToken` overload had no caller in `src` or in the tests. The `cref` in `IsExemptFromPrecedingBlankLineBeforeRegionDirective`'s summary now points at the new owner. | — |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTriviaBoundaryRewriter.cs` | `VisitToken` calls `BlockStartUtilities.IsFirstInBlock`. | FMT-BLANK-012 |
| `src/Reihitsu.Formatter/Pipeline/DocumentationComments/DocumentationCommentFormattingPhase.cs` | `IsMovableDelimitedDocumentation` calls `BlockStartUtilities.IsFirstInBlock`; the `using Reihitsu.Formatter.Pipeline.BlankLines.Utilities` and the summary's `cref` are updated. The documentation phase now uses no type from another phase's folder. | FMT-DOC-001 |

`AGENTS.md` is unchanged: the phase-table guard names `IsInlineDocumentation` and `IsMovableDelimitedDocumentation` still exist under those names, and `IsFirstInBlock` is not in the table.

## Policy owners

- "Token is first in a block or switch section": `rg -n "IsFirstInBlock|IsSwitchLabelColon" src/Reihitsu.Formatter --type cs` now finds one owner, `BlockStartUtilities.IsFirstInBlock`. Its two callers are the blank-line boundary rewriter and the documentation phase, and `PrecedingTokenFacts` only defines the `IsSwitchLabelColon` value. RH8303's private `IsFirstInBlock` is on the analyzer side and predates this run (05-implementer.md).
- Cross-phase imports: no `using Reihitsu.Formatter.Pipeline.<Phase>` in `Pipeline/` names another phase's folder. The exceptions are `FormattingPipeline.cs` (the orchestrator) and the preparation phase's own sub-phases (`LineEndings`, `TabReplacement`), both pre-existing. One remaining reference is a `cref` in a comment: `BlankLineTokenCleanupRewriter` mentions `Pipeline.LineBreaks.Utilities.TokenGapNormalizer`. It is pre-existing and not a dependency this run touches.

## Tests run

`scripts/format.sh` on the four files formatted 1 of 4 (the new file). `scripts/build.sh --configuration Debug` gave 0 warnings and 0 errors.

```shell
scripts/test.sh --configuration Debug
```
- Analyzer: `Passed: 6330, Failed: 0`
- Formatter: `Passed: 3494, Failed: 0`
- Core: `Passed: 535, Failed: 0`
- CLI: `Passed: 251, Failed: 0`
- Architecture: `Passed: 92, Failed: 0`
- Tooling: `Passed: 125, Failed: 0`
- Playground: `Passed: 17, Failed: 0`

These are the same counts as in 05-implementer.md, which is expected for a move without a behavior change.

## Disputed tests or scope

_None._ One new internal file with two consumers, both changed by this run. One dead overload was removed.

## Architecture findings

_None._ Decision #12 is resolved.

## Follow-up candidates

_None._
