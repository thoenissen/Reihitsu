Role: implementer
Commit: 368d22a
Verdict: tests pass

Inputs read: `00-lead.md` (maintainer answers 1A, 2A; decisions 1–7, decision 5 in particular), `01-spec-author.md`, `02-skeptic.md`, `03-test-implementer.md`, the change doc `sdd/changes/2026-11/unsupported-link-operator-trivia.md` (*Special cases*), FMT-UNSUP-015 in `sdd/specs/formatter/unsupported-syntax.md`. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Before the change, the handed-over formatter filter ran 350 tests with 168 failing, all on the refusal assertions the test implementer named.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/FluentChain.cs` | New `public static IReadOnlyList<SyntaxToken> GetLinkOperatorTokens(ExpressionSyntax node)`: the one statement of which tokens form a link operator (simple member access: `.` or `!` `.`; member binding: `?` `.` or `!` `?` `.`, the conditional access taken from the token in front of the binding's `.`). `Collect` and `CollectMemberAccess` now build every link's `OperatorTokens` from it; the links built are unchanged (for a member binding, the conditional access in front of its `.` is exactly the `pendingConditional` `Collect` passes down). | SHARED-CHAIN-001, FMT-UNSUP-015 (decision 5, option A) |
| `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` | `LinkOperatorCommentReason`; `FindTriviaInsideLinkOperator` checks every member access and member binding of the input: the trailing trivia of each operator token but the last and the leading trivia of each but the first, taken from `FluentChain.GetLinkOperatorTokens`; an operator whose first token lies outside the input is skipped (FMT-UNSUP-003). The candidate is listed last, so FMT-UNSUP-010 keeps its reason for a directive it refuses too; the ordering comment says so. | FMT-UNSUP-015, FMT-UNSUP-001, FMT-UNSUP-003 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ChainLineBreakRewriter.cs` | `CloseInnerGaps` selects a gap inside a link operator by its line break alone; it no longer goes through `CloseGap`'s `WouldJoinAcrossUnjoinableTrivia` check. Class summary and method summary updated (FC-031). | FMT-BREAK-019, FMT-UNSUP-015 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/MethodChainAlignmentContributor.cs` | `AlignRestOfRun` and its call deleted; the now unused `System.Collections.Generic` using removed; class summary updated (FC-031). | FMT-UNSUP-015 |

## Policy owners

- Which tokens form a link operator: `rg -n "OperatorTokens|GetConditionalTokens|StartsLinkOperator" src --glob '!*Test*'` →
  - `FluentChain.GetLinkOperatorTokens` (new owner; `Collect`/`CollectMemberAccess` and `PrecheckPhase.FindTriviaInsideLinkOperator` read it);
  - `FluentChain.GetConditionalTokens` (private builder the owner uses; also builds the `?[` attached part's tokens, which are FMT-UNSUP-014's);
  - `ChainLineBreakRewriter.CloseInnerGaps` reads `link.OperatorTokens` (consumer);
  - `AttachedPartGapUtilities.StartsLinkOperator` — a pre-existing private copy of "a `!` starts a link operator" (FMT-UNSUP-014's complement). Not changed; see *Disputed tests or scope*.
- Comment/directive/disabled-text test in a gap: `SyntaxTriviaUtilities.IsCommentTrivia`/`IsDirectiveOrDisabledTextTrivia` through the precheck's existing `FindFirstUnsupportedOperatorTrivia` (reused, no new copy).

## Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` after my commit: 4 files changed, 108 insertions(+), 42 deletions(-). No new type; one new public member (`FluentChain.GetLinkOperatorTokens`).

## Deleted special cases

- `MethodChainAlignmentContributor.AlignRestOfRun` and its call in `Contribute` — fulfils *Deleted*, "The alignment of a link operator's rest under its first token".
- In `ChainLineBreakRewriter.CloseInnerGaps`, the `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` refusal for gaps inside a link operator (formerly reached through `CloseGap`) — fulfils *Deleted*, "The refusal to close a gap inside a link operator". `CloseInnerGaps` itself stays, as the change doc says, selecting gaps by line break only.
- Kept as the change doc's *Checked and kept* lists: `CloseGap`'s check (dot-to-name gap, front gap of the first link), the root-line check in `GetWrappedLayoutLineBreaks`, `IsBehindMultiLineCommentTail`/`EnsureLineBreak`, `FluentChainLink.HasInnerLineBreak(isJoinableOnly)`, `FluentChain.IsFirstLinkBlocked`, `ListElementInteriorJoinRewriter.CanJoin`. I found no further special case for the shape.

## Tests run

```text
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~UnsupportedLinkOperatorCommentTests|FullyQualifiedName~ConditionalAccessWrapBetweenOperatorTokensTests|FullyQualifiedName~ChainGapJoinTests|FullyQualifiedName~ChainLinkLayoutTests"
  before: Failed: 168, Passed: 182, Total: 350
  after:  Passed! - Failed: 0, Passed: 350, Total: 350
scripts/test.sh --configuration Debug --project formatter     Passed! - Failed: 0, Passed: 6321, Total: 6321
scripts/test.sh --configuration Debug --project core          Passed! - Failed: 0, Passed: 738, Total: 738
scripts/test.sh --configuration Debug --project architecture  Passed! - Failed: 0, Passed: 198, Total: 198
scripts/test.sh --configuration Debug --project analyzer      Passed! - Failed: 0, Passed: 11299, Total: 11299
scripts/test.sh --configuration Debug --project cli           Passed! - Failed: 0, Passed: 407, Total: 407
scripts/test.sh --configuration Debug --project tooling       Passed! - Failed: 0, Passed: 136, Total: 136
scripts/format.sh --check src                                 0 of 2000 file(s) need formatting (no file refused for the new shape)
```

## Disputed tests or scope

- **New public member in Core** (`FluentChain.GetLinkOperatorTokens`) — decision 5 option A: the precheck and the chain model read one statement of the operator's tokens. `Reihitsu.Core` exposes its internals only to `Reihitsu.Core.Test`, so the member is public. It is a shared helper with three consumers (chain model, precheck, and through `OperatorTokens` every analyzer and formatter phase that reads links). The chain model's output is unchanged (core, analyzer, and formatter suites green). If the lead prefers option B, the helper's body can move into the precheck and `Collect` revert to its previous inline lists; the full matrix tests the test implementer wrote cover either.
- **The precheck's set is a superset of the chain model's links**, as FMT-UNSUP-015 states: the precheck checks every member binding, also one the chain model does not collect because the conditional access's branch is no chain node (for example a postfix increment around a member binding). That direction only refuses more and is what the spec says; no test pins such a case and none is needed for safety.
- **Adjacent-fix candidate (small, ~10 lines):** `AttachedPartGapUtilities.StartsLinkOperator` is a private copy of "this `!` is the first token of a link operator", which `FluentChain.GetLinkOperatorTokens` now states. It could read the helper (`!` is the first of the operator tokens of the member access or member binding behind it). Equivalent today; not changed, because it belongs to FMT-UNSUP-014/FMT-BREAK-040 and the run's tests do not need it. Lead's call (FC-015).

## Architecture findings

_None._ The change removes the shared decision the line-break and indentation phases made for gaps inside a link operator; the precheck and the chain model now read one token definition.

## Follow-up candidates

_None._

## Addendum — adjacent fix (lead decisions 8–10)

Commit: 09255b9

- Decision 8: `FluentChain.GetLinkOperatorTokens` stays public (option A). Decision 9: the precheck's superset of the chain's links is accepted as specified.
- Decision 10, the adjacent fix: `AttachedPartGapUtilities.StartsLinkOperator` (`src/Reihitsu.Formatter/Pipeline/Core/Utilities/AttachedPartGapUtilities.cs`) no longer carries its own copy of the rule. It finds the member access or member binding whose `.` follows the `!` (directly, or behind the `?` of a conditional access), and it answers whether the `!` is the first of that link's `FluentChain.GetLinkOperatorTokens`. The behavior is unchanged. The summary is updated (FC-031). Requirements: FMT-UNSUP-014, FMT-BREAK-040, SHARED-CHAIN-001.
- Round delta: 1 file, +19 −7. Total production diff (`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`): 5 files changed, 127 insertions(+), 49 deletions(-). No new type.
- Policy owners after the round: the rule for which tokens form a link operator is stated only in `FluentChain.GetLinkOperatorTokens`. The chain model, the precheck (FMT-UNSUP-015), and `AttachedPartGapUtilities.StartsLinkOperator` (FMT-UNSUP-014, FMT-BREAK-039/040) read it.

Tests run (Debug):

```text
formatter, filter over ArgumentListLineStartTests, AttachedPartLineStartTests, ChainGapJoinTests, ChainLinkLayoutTests,
  ConditionalAccessWrapBetweenOperatorTokensTests, JoinSeparatorPhaseTests, ReihitsuFormatterDetachedRootTests,
  ReihitsuFormatterNodeColumnTests, UnsupportedAttachedPartCommentTests, UnsupportedLinkOperatorCommentTests,
  WithElementSpacingTests                                   Passed! - Failed: 0, Passed: 694, Total: 694
formatter project                                           Passed! - Failed: 0, Passed: 6321, Total: 6321
architecture project                                        Passed! - Failed: 0, Passed: 198, Total: 198
cli, CliHiddenCodeEndToEndTests                             Passed! - Failed: 0, Passed: 48, Total: 48
analyzer, RH3202 | RH5112                                   Passed! - Failed: 0, Passed: 170, Total: 170
```
