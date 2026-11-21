Role: implementer
Commit: 4b68a4df
Verdict: test disputed

Inputs read: `00-lead.md`, `01-spec-author.md` to `04-test-implementer.md`, the change doc `sdd/changes/2026-11/unsupported-link-dot-name-trivia.md` (with *Special cases*), FMT-UNSUP-015/016. The prompt carried no intended fix, cause, or verdict.

Every handed-over test passes. One existing test outside the handover fails because it asserts the output this change removes (below, *Disputed tests or scope*); it was not changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` | New reason `LinkDotNameCommentReason`; new `FindTriviaBetweenLinkDotAndName` (trailing trivia of the `.` of a `SimpleMemberAccessExpression` or `MemberBindingExpressionSyntax`, then the leading trivia of its name's first token), asked in the existing member-access/member-binding case of `FindUnsupportedSyntax`; the candidate is listed last, so FMT-UNSUP-004 and FMT-UNSUP-010 keep their reason for the same trivia; the precedence comment updated | FMT-UNSUP-016, FMT-UNSUP-001, FMT-UNSUP-003 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ChainLineBreakRewriter.cs` | `NormalizeChain` selects the gap between a link's dot and its name on the line-break check alone instead of through `CloseGap` (which also ran `WouldJoinAcrossUnjoinableTrivia`); comment rewritten to name FMT-UNSUP-016 | FMT-BREAK-020, change doc *Special cases / Deleted* |

## Policy owners

- What a link's dot-to-name gap is: `rg -n "GetLinkOperatorTokens|DotToken" src/Reihitsu.Core src/Reihitsu.Formatter` — `FluentChain.GetLinkOperatorTokens` / `FluentChainLink.DotToken` (Core) own the chain's operator tokens, but `GetLinkOperatorTokens` returns nothing for a member binding outside a conditional access; FMT-UNSUP-016 decides "by the syntax tree alone" on the node kinds, so the precheck reads `OperatorToken`/`Name` of the two kinds directly, as the spec lists them. Chain links are exactly these two kinds (`FluentChain.cs:248`, `:256`), so the precheck covers every gap `NormalizeChain` closes.
- Which trivia refuses: `rg -n "FindFirstUnsupportedOperatorTrivia" src/Reihitsu.Formatter` — the precheck's one helper, reused.
- Join refusal: `rg -n "WouldJoinAcrossUnjoinableTrivia" src/Reihitsu.Formatter` — `CloseGap` keeps it for the gap in front of the first link (kept, per the change doc); the other callers are unrelated rewriters.

## Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 2 files changed, 49 insertions(+), 7 deletions(-). No new type, no new public member.

## Deleted special cases

- `ChainLineBreakRewriter.NormalizeChain`: the `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` refusal for the dot-to-name gap (formerly through `CloseGap`) — fulfills *Special cases / Deleted*, "The refusal to close the gap between a link's dot and its member name". `CloseGap` itself stays for the first link's gap (*Checked and kept*).
- Further candidate found, not deleted: see *Disputed tests or scope*, item 2.

## Tests run

```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~UnsupportedLinkDotNameCommentTests|FullyQualifiedName~ChainGapJoinTests|FullyQualifiedName~ChainSplitMemberNameRejoinTests|FullyQualifiedName~UnsupportedLinkOperatorCommentTests|FullyQualifiedName~UnsupportedConditionalOperatorCommentTests"
```
Passed! - Failed: 0, Passed: 685

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~BetweenTheDotAndTheMemberName"
```
Passed! - Failed: 0, Passed: 17

```shell
scripts/test.sh --configuration Debug --project cli --filter "FullyQualifiedName~CliHiddenCodeEndToEndTests.CommentOrDirectiveBetweenADotAndItsMemberNameIsReportedAsUnsupported"
```
Passed! - Failed: 0, Passed: 4

Full projects (Debug): formatter Failed 1, Passed 6825 (the test below); analyzer Passed 11635; cli Passed 425; core Passed 812; architecture Passed 198.

## Disputed tests or scope

1. **`MethodChainAlignmentTests.ConditionalAccessAnchorOnLaterLineThanInitializerCloseBraceStaysIdempotent`** (`src/Reihitsu.Formatter.Test/Regression/Indentation/MethodChainAlignmentTests.cs:1605`) fails: `AssertRuleResult` asserts a formatted output for an input with `}.⏎// keep⏎Choices?.Select(…)` — an own-line comment between a member access's `.` and its name `Choices`. FMT-UNSUP-016 refuses that input (`The formatter must not refuse the input as unsupported: a comment or directive between a . and its member name`). The test implementer's search (`[\w\)\]\?!]\.` in front of the comment) did not match a dot behind `}`, so the test was not in the handover. It needs the test implementer: rewrite it to the refusal, or remove it with item 2.
2. **Further special case, likely unreachable now** — the test's own summary says the comment between the dot and the name "is the only remaining way to reach the correction's unequal-line branch": `MethodChainAlignmentContributor.GetChainAnchorColumn`'s condition `LayoutComputer.GetLine(anchorDot) == LayoutComputer.GetLine(firstDot)` (`src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/MethodChainAlignmentContributor.cs`, around line 74) and the matching part of its summary. With the dot-to-name gap always joined, that branch may no longer be reached. I did not verify it is unreachable and did not delete it; it is not in the change doc's *Special cases*, so it is the spec author's to list (deleted, or checked and kept with the input that still reaches it).

## Architecture findings

_None._

## Follow-up candidates

_None._
