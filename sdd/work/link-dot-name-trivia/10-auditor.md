Role: auditor
Commit: d94d4f58d64a9bbcd6420063b24a11a843506146
Verdict: pass, text cleanup

## Scope

Full audit of head `d94d4f58` against base `origin/main` = `b69a6ae3` (`git ls-remote` confirms; `git merge-base --is-ancestor` confirms it is merged). The merge `a2c2a9f7` brought in text only (`indentation.md`, `diagnostics.md`, a squad note); every commit after the spec checker's `a6e19053` is text. The diff is what `04`–`09` describe: production `PrecheckPhase.cs` and `ChainLineBreakRewriter.cs`; tests, specs, change doc, `AGENTS.md` row, handovers. `scripts/lane-check.sh behavior-change`: confirmed. The prompt carried no intended fix, cause, or verdict.

Inputs: thoenissen/Reihitsu#1888, `00-lead.md` (maintainer answers pre-run, 1A, 2A; decisions 1–5), `01`–`09`, `sdd/changes/2026-11/unsupported-link-dot-name-trivia.md`, FMT-UNSUP-015/016, FMT-BREAK-001/020, FMT-INDENT-029, RH2001-007, RH7101-007, `squad/failure-classes.md`.

Counterparts opened outside the diff: `FluentChain.Collect`/`CollectMemberAccess`/`GetLinkOperatorTokens`, `FluentChainLink`, `LineBreakTriviaUtilities.JoinGaps`/`CreateJoinedTrailingTrivia`, `ListElementInteriorJoinRewriter.CanJoin`/`IsOwnedGap`/`IsOperatorWrap`, `MethodChainAlignmentContributor.GetChainAnchorColumn`, `NoSpaceSpacingRule`, `AttachedPartGapUtilities.StartsLinkOperator`, `FormattingPipeline` (precheck is a pipeline phase), `RH5112`/`RH5201`/`RH3004` code-fix providers.

Executed: `scripts/format.sh` twice over four fixtures in a scratch directory (own-line `///` in front of a later link, own-line `/** */` in front of a split dot whose name is joined, `///` behind a joined name, `///` between the dot and the name in an expression body). The first refused only the last (`(4,27)`, this reason); the second pass changed nothing (`Formatted 0 of 3`). No phase produced the shape.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ChainLineBreakRewriter.cs:46-48` (class summary `<para>`) | FC-031 | – | PR-introduced | The class summary still says "A gap in front of a link operator or between its dot and the member name that holds a comment, a preprocessor directive or disabled text is never closed", describing the guard this change deleted from `NormalizeChain`; only the gap in front of a link keeps that refusal (`CloseGap`), and the dot-to-name gap now never holds such trivia because the precheck refuses it (FMT-UNSUP-016), like the operator gaps of FMT-UNSUP-015 named in the next sentence. | Reword the paragraph: the gap in front of a link operator that holds such trivia is never closed; a gap between two tokens of a link operator (FMT-UNSUP-015) or between a link's dot and its member name (FMT-UNSUP-016) never holds such trivia, because the precheck refuses an input that holds it. |

`pass, text cleanup`: finding 1 is an XML documentation comment on an internal type; it cannot reach compiled behavior, diagnostics, or rule metadata.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Refuse a comment, directive, or disabled text between a link's `.` and its name (`PrecheckPhase.FindTriviaBetweenLinkDotAndName`) | `OperatorToken.TrailingTrivia` and `Name.GetFirstToken().LeadingTrivia` of every `SimpleMemberAccessExpression` and every `MemberBindingExpressionSyntax` in `DescendantNodesAndSelf` of the input | `ChainLineBreakRewriter.NormalizeChain` joins `(link.DotToken, link.Name.GetFirstToken())` for every `FluentChainLink` when `AreSeparatedByEndOfLine` | The two tokens are adjacent, so the two trivia lists are the whole gap `JoinGaps` rewrites. Chain links are only those two kinds (`FluentChain.Collect`: `SimpleMemberAccessExpression`; `MemberBinding` with a pending conditional), `DotToken` is the last of `GetLinkOperatorTokens` = the node's `OperatorToken`, `Name` is the node's `Name`. The precheck set is a superset (every member binding, member accesses outside any chain), as the spec's *Every link* bullet requires. The precheck runs as the pipeline's first phase on the node the line-break phase receives, so no link the rewriter sees escaped the check. | holds |
| Same, trivia kinds | `SyntaxTriviaUtilities.IsCommentTrivia` or `IsDirectiveOrDisabledTextTrivia` (`FindFirstUnsupportedOperatorTrivia`, reused) | the deleted `WouldJoinAcrossUnjoinableTrivia` (comment, directive, disabled text) | Same three kinds; documentation-comment kinds included (tested in None/Parse/Diagnose) | holds |
| Precedence with FMT-UNSUP-004/009/010/011–015 | candidate list order; `FullSpan.Start` comparison | spec precedence paragraph | 016 is listed last; 004/010 win on the same trivia. 015 reads `.`'s leading trivia, 016 its trailing, 014 reads the token in front of `(`/`[`/`!`, which is never the `.`; 008/011/012 read the leading trivia of a branch, operand, or expression, whose first token is never a name behind a dot; 009's gap ends at a comma, which never follows a `.` or stands between `.` and name. No two of them share trivia except 004/010, as specified. | holds |
| Node-level locality (FMT-UNSUP-003) | `.` and name are tokens of the link node | `FindTriviaInsideLinkOperator`'s `node.Span.Contains(tokens[0])` | No containment check is needed: both tokens lie in the link node, which is the input or inside it; a name-only or receiver-only target holds no link node | holds |
| Join selection for the dot-to-name gap (`NormalizeChain`) | `HasLineBreak(DotToken, nameToken)` | `CloseInnerGaps` for operator gaps; `CloseGap` (kept) for the gap in front of the first link | Same pattern as `CloseInnerGaps`; `CloseGap` still refuses for its own gap, which is not in the shape | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which trivia makes an operator/link gap unsupported | `PrecheckPhase.FindFirstUnsupportedOperatorTrivia` (`rg -n "FindFirstUnsupportedOperatorTrivia" src/Reihitsu.Formatter`), used by 011, 012, 013, 014, 015, 016 | one owner |
| Which tokens are a link's `.` and name | precheck: `OperatorToken`/`Name` of the two node kinds; chain: `FluentChainLink.DotToken` (`GetLinkOperatorTokens`, `Reihitsu.Core/FluentChain.cs:248,256,416,450`) and `Name` | two readers of the same syntax-tree fact, not two policies: for every chain link both name the same tokens (traced above); the precheck is deliberately wider, as FMT-UNSUP-016 states. No drift possible short of Roslyn changing the node shape. |
| Refusal to join across comments/directives in the dot-to-name gap | formerly `CloseGap` in `NormalizeChain`; now only `PrecheckPhase` (`rg -n "WouldJoinAcrossUnjoinableTrivia" src/Reihitsu.Formatter`: remaining callers are `CloseGap` for the first link, `GetWrappedLayoutLineBreaks` root line, list-element interior join, assignment, list, accessor, constraint rewriters — none decides this gap; `ListElementInteriorJoinRewriter.IsOwnedGap` excludes it through `IsOperatorWrap`) | one owner |
| Code-fix guards for the gap | `rg` for `OperatorToken.TrailingTrivia`, `Name.GetFirstToken().LeadingTrivia` in `src/Reihitsu.Analyzer.CodeFixes`: none; RH5112/RH5201 providers have no trivia guard; RH3101/RH3105/RH6016 fixes do not call the formatter | no FC-042 survivor |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `UnsupportedLinkDotNameCommentTests` refusal tests (spec examples, 120-cell gap matrix, 96 doc-mode cells, conditional groups, 20 scopes, members, attribute precedence, directive precedence) | refused, output byte-identical, reason, location = first trivia, LF and CRLF | output changed; `Unsupported` null; other reason; other location | `AssertUnsupportedFor` (+ explicit `LinePosition` for spec examples) | adequate |
| same, other side (`CommentOutsideTheGapBehindTheDotIsSupported`, `CommentBehindADotThatIsNoLinkIsSupported`, `CommentBehindTheDotOfAQualifiedNameIsSupported`, `ConditionalGroupAroundTheWholeStatementIsSupported`, `SpecExampleCommentsOutsideTheDotToNameGapsAreSupported`) | not refused; second pass stable | `Unsupported` non-null; second pass changes output | `AssertSupported` | adequate for a precheck boundary; layout is not this requirement's guarantee |
| `NodeLevelEntryPointsRefuseATargetThatHoldsTheShape`, `NameOrExpressionOfAMemberAccessAloneDoesNotHoldTheShape`, `NameOfAMemberBindingAloneDoesNotHoldTheShape`, `FindUnsupportedSyntaxAnswersForABuiltNode`, `FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck`, `FormatDocumentAsyncRefusesTheShape` | FMT-UNSUP-002/003 per entry point; built-node answer equals precheck | target refused/not refused wrongly; answers differ | direct API, LF and CRLF | adequate (FC-028, FC-041, FC-043) |
| `OutputOfFormattedLinksIsNotUnsupported`, `OutputOfTheOtherSideExampleIsNotUnsupported` | FMT-UNSUP-005 | second pass refused or changed | two `FormatSyntaxTree` passes, LF and CRLF, three doc modes | adequate (FC-038, FC-010) |
| `ChainSplitMemberNameRejoinTests.SplitMemberNameRejoinsInEveryEnclosingConstruct` | FMT-BREAK-020 join in assignment target, `nameof`, interpolation hole, initializer | any name left on its line | `AssertRuleResult` (exact output, second pass, phase stability, LF/CRLF) | adequate |
| Rewritten `ChainGapJoinTests`/`ChainSplitMemberNameRejoinTests` refusal tests, `CommentBetweenDotBehindInitializerCloseBraceAndMemberNameIsUnsupported` | refusal with location | as above | `AssertUnsupportedFor` | adequate |
| `MethodChainAlignmentTests.AnchorBehindMultiLineIndexAfterInitializerCloseBraceKeepsItsOwnColumn` | the kept same-line condition: `.ToString()` aligns under the anchor `.Trim()` (FMT-INDENT-029) | `.ToString()` at column 0 on pass one (shown by the test implementer with the condition removed) | `AssertRuleResult` | adequate; expected value is FMT-INDENT-029's, an unchanged requirement |
| RH5201, RH5112, RH3202 (×2), RH2001 (×2), RH7101 (×2) no-fix tests | SHARED-FIX-010 / RH2001-007 / RH7101-007: diagnostic stays, no action | a fix offered | `VerifyNoCodeFixIsOffered`, LF and CRLF where the change-doc input is used | adequate |
| `CliHiddenCodeEndToEndTests.CommentOrDirectiveBetweenADotAndItsMemberNameIsReportedAsUnsupported` | exit code, exact stderr line with line/column, file unchanged | any differs | `Program.Main` end to end | adequate |
| `UnsupportedLinkOperatorCommentTests.CommentBetweenTheDotAndTheMemberNameGivesTheReasonOfTheDotToNameGap`, `UnsupportedConditionalOperatorCommentTests` row change | moved rows now refused with 016's reason; conditional-operator row keeps a `?` outside every shape | other reason; refusal | `AssertUnsupportedFor` / `AssertSupported` | adequate |

Visible output (check 10): every added or changed expected value is a refusal or withheld fix covered by the maintainer's pre-run answer and batch-1 answers 1A (reason text) and 2A (one-line form), recorded in the change doc's *Decisions* and `00-lead.md`; the join outputs are FMT-BREAK-020's and the alignment output FMT-INDENT-029's, both unchanged in substance (FMT-BREAK-020 gained only the sentence that the join never meets the shape). No rule page changed.

## Failure classes dismissed
- FC-002 — the spec's qualifiers ("simple member access", "member binding", "trailing trivia of the `.`", "leading trivia of the name's first token", "on any line") are each an explicit condition of the predicate.
- FC-004 — the join now writes into the gap without the refusal, but every link gap it joins was checked by the precheck on the same node (guard-scope table), and FC-038 found no producer; the second pass over the probes and the tests' two-pass checks agree.
- FC-006, FC-022, FC-023, FC-024, FC-025, FC-026, FC-029, FC-036 — the change rewrites no syntax; a literal or identifier is one token and cannot stand inside the gap; the join itself is unchanged for whitespace-only gaps.
- FC-007, FC-008, FC-009, FC-018, FC-030 — no code fix changed; every fix test withholds the fix.
- FC-011, FC-012 — no loop or new phase interaction; the precheck changes nothing.
- FC-013, FC-014 — no analyzer changed; refused files are reported, not left with a formatter-induced diagnostic.
- FC-016 — siblings: qualified-name `.` (no phase joins it; recorded follow-up), `->`, `::`, `..`, cref (structured trivia) — all outside the declared shape and tested as supported.
- FC-032, FC-033, FC-034, FC-035, FC-037 — results were gathered after the last compiled change (`4b68a4df`, then test-only `4e4af935`); the reproduction uses the issue's own input (CLI test row 1); expected values come from the spec and change doc, and the alignment test was shown to fail without the kept condition.
- FC-044 — the check is a syntactic read of two trivia lists; it models nothing another component computes.
- Reachability: the shape is reached through whole-document formatting and the CLI (issue input reproduced by the skeptic and pinned by the CLI test).

Covered and held, not dismissed: FC-001, FC-003, FC-005, FC-010, FC-015, FC-017, FC-019, FC-020, FC-021, FC-027, FC-028, FC-031 (one failure, finding 1), FC-038, FC-039, FC-040, FC-041, FC-042, FC-043.

## Architecture findings
_None._ The join refusal moved from the chain rewriter to the precheck, which is the declaration itself; no decision has two owners.

## Hints
- `ChainLineBreakRewriter.NormalizeChain`'s new dot-to-name loop repeats the body of `CloseInnerGaps` for a two-token list; `CloseInnerGaps([link.DotToken, nameToken], gaps)` would say the same. Not a finding: both call the same `HasLineBreak`, so nothing can drift.

## Follow-up candidates
_None new._ The qualified-name candidate in `00-lead.md` stands as recorded.
