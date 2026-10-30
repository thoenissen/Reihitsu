Role: implementer
Commit: 449e3bf
Verdict: tests pass

Second implementer pass, on `00-lead.md`, `07-spec-author.md`, `08-test-implementer.md`, and the decisions `squad/decisions/2026-10-30-line-start-fact-{implementer-findings,amended-change-doc,contributor-and-spacing}.md`. The prompt carried no intended fix beyond the maintainer's answers it relayed, and those are recorded in the decision files.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/LineBreakTriviaUtilities.cs` | `CollapseTokenToSameLine` joins across tokens without text. When the directly preceding token has no text and the gap to it holds no line break, the gap it joins is the one to the token with text in front. That token must lie inside the node, and the gap must hold a line break and no comment, directive, or disabled text (`WouldJoinAcrossUnjoinableTrivia`). This replaces the refusal of the first pass. `Dictionary<,` / `>` and `Func<` / `,,>` are now joined, while a list a comment or directive keeps wrapped stays as it is. The `StripTrailingWhitespace` forwarder is deleted | FMT-BREAK-014, FMT-INDENT-015 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutModel.cs` | New `SetUnlessPlaced`, a fallback layout. It places a line only while no alignment contributor has placed it through `Set`, and a later `Set` takes the line over. `GetAlignmentContributors` returns the `Set` contributors, or the fallback contributor when there is none | issue *Done when* (answer 1A) |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/ListElementAligner.cs` | `AlignContinuations` writes the list-element continuation through `SetUnlessPlaced`. A wrapped binary operator, conditional, chain link, or angle-bracket list inside an argument is now the only owner of its line. This was the source of all 172 overlaps (every pair in `08-test-implementer.md` was `ArgumentAlignmentContributor` and an inner construct) | issue *Done when* |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ListElementInteriorJoinRewriter.cs` | `CreateSeparator` no longer asks `SpacingPolicy`. The horizontal-spacing phase spaces the joined gap for every pair it has a rule for. The join still writes the gap for pairs no rule covers (its existing `IsTightBefore`/`IsTightAfter` table; that phase keeps such a gap). It keeps the fuse guard `WouldMergeWhenTight`, which includes the `Reihitsu.Core` `UnaryOperatorSpacingUtilities` check. The `HorizontalSpacing.Utilities` using is gone | answer 2B |
| `src/Reihitsu.Formatter/Pipeline/Core/Utilities/LineStartIndex.cs` | Moved from `Pipeline/Indentation/Utilities/`; the namespace is now `Pipeline.Core.Utilities` | issue *Done when* |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/TokenLocator.cs` | The `ContainsToken` forwarder is deleted; its own calls go to `TokenContainmentUtilities` | — |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutComputer.cs` | Adds the `Pipeline.Core.Utilities` using | — |

How the formatter output was kept unchanged under answer 2B: I tried two simpler separators first, and each broke existing tests.
- Always tight: 12 tests broke, for example `NamedArgumentValueWrappedAfterColonIsJoinedAndTheListStaysOnOneLine` and `LambdaExpressionBodyWrappedAfterArrowIsJoined`.
- Always one space: 11 tests broke, for example `TypeofWrappedBeforeOpenParenthesisIsJoinedWithoutSpace` and `RangeWrappedBeforeRangeOperatorIsJoinedWithoutSpace`.

In both, the failing pairs are ones that no horizontal-spacing rule covers, so the phase keeps whatever gap the join wrote. The join therefore still has to write those pairs. The shipped version writes them from the join's own table and leaves every other pair to the horizontal-spacing phase. The fallback (moving `SpacingPolicy` to `Pipeline/Core`) was not needed.

Formatter output on shipped sources: `scripts/format.sh --check src` reports `0 of 1753 file(s) need formatting`, both before these changes (stash of the working tree) and after.

## Policy owners

- **Spacing of a joined gap.** `rg -n "SpacingPolicy" src/Reihitsu.Formatter --glob '*.cs'` finds only the `HorizontalSpacing` phase.
  - The join's `IsTightBefore`/`IsTightAfter` covers only pairs that `SpacingPolicy` has no rule for. That narrows them to what the phase leaves to the join. The table is a second statement of "which uncovered pairs are tight", and nothing else states it; see *Architecture findings*.
- **Line-continuation fallback.** The only `SetUnlessPlaced` caller is `ListElementAligner.AlignContinuations` (`rg -n "SetUnlessPlaced" src`).
- **Cross-folder check.** I scanned every `Reihitsu.Formatter.Pipeline.<X>` reference in each phase folder. Remaining dependencies are on `Pipeline/Core` only, plus:
  - `Data/RootListPosition` → `LineBreaks` (`TokenGapNormalizer.HasOwnLinePlacementOwner`; `Data` is not a phase);
  - `Utilities/ReihitsuFormatterHelpers` → `TabReplacement` (entry-point code, not a phase);
  - the orchestrators `FormattingPipeline` and `PreparationPhase`.

  No phase uses a type from another phase's folder.

## Tests run

All on 449e3bf, Debug (`scripts/build.sh --configuration Debug`: 0 warnings, 0 errors).

- `scripts/test.sh --configuration Debug --project formatter`: `Passed: 3254, Failed: 0`. This includes the seven join tests and both `LayoutContributorOwnershipTests`.
- `--project analyzer`: `Passed: 6264, Failed: 0`.
- `--project core`: `Passed: 533`.
- `--project cli`: `Passed: 251`.
- `--project architecture`: `Passed: 89`.
- `--project tooling`: `Passed: 125`.

## Disputed tests or scope

_None._ The deleted forwarders had no test references (`08-test-implementer.md`).

## Architecture findings

- **The join and the horizontal-spacing phase still split one decision.** For token pairs no spacing rule covers, the horizontal-spacing phase keeps the gap. The line-break join therefore decides those pairs with its own table (`ListElementInteriorJoinRewriter.IsTightBefore`/`IsTightAfter`). Answer 2B removed the join's use of `SpacingPolicy`, but the join cannot leave these pairs to the phase without changing output (the "always one space" and "always tight" variants above). A complete fix would give `SpacingPolicy` rules for those pairs, which changes FMT-SPACE-002 behavior. I left that for the maintainer and did not change it.

## Follow-up candidates

- Unchanged from `06-implementer.md`: `AttributeTargetUtilities.StartsLine`, and the analyzer-side terminator sets. Both are already follow-ups under answer 3A.
- `Data/RootListPosition` depends on `LineBreaks/Utilities/TokenGapNormalizer.HasOwnLinePlacementOwner`. That is a blank-line placement policy living in the line-break phase's folder and read by `Data`.
