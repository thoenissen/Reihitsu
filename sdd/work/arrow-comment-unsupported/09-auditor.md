Role: auditor
Commit: a23ec37
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit of thoenissen/Reihitsu#1732 (`agent-filed`), lane behavior change. Head a23ec37e6d7a6d885a3d1e80edea13a0959640c6 (checked out, tree clean); `origin/main` 71a89ec49a8806e9d4eed65ba8e2c229904eb465, an ancestor of the head (`git merge-base --is-ancestor`). `scripts/lane-check.sh behavior-change`: `LANE CHECK: confirmed — claimed behavior-change, diff needs behavior-change`. The diff is what 01–08 describe: four production files (`PrecheckPhase`, `PropertyLayoutLineBreakRewriter`, `ExpressionBodyToBlockConverter`, `GetOnlyMemberExpressionBodyTransform`), `AGENTS.md`'s precheck row, the specs and change doc, and the tests of 04. The merge of 71a89ec brought #1840 (precheck comma check) and #1842 (test helpers); neither touches a line this run changes, and none of the files they changed holds the shape (multiline `rg` for `=>` followed by a comment or directive over `git diff --name-only 17fb08c 71a89ec -- src`: no hit).

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Analyzer.Test/Formatting/RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.cs:77` `VerifyFixIsOfferedForAMethodBesideAMethodWithACommentBehindItsArrow` | FC-017 (FC-009, FC-043) | – | PR-introduced | The test is tagged SHARED-FIX-010 and states that the method beside the refused one "does" get a fix, but it only asserts `GetCodeFixActionsAsync(...)` is non-empty for `A`. SHARED-FIX-010 also forbids an action that, once invoked, finds the refusal and changes nothing. If the fix formatted `A` in a context that holds `B` (the line, the type) while still asking `FindUnsupportedSyntax` about `A` alone, the action would be offered, change nothing, and this test would still pass. The behavior holds today: `scripts/apply-fix.sh RH3202` on `int A() => 1; int B() => /* c */ 2;` converts `A` to a block and leaves `B` as written. The test cannot falsify a regression of it. | Apply the action registered for `A` and assert the result: `A` has a block body, `B` is unchanged, under LF and CRLF. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Refuse a comment, directive, or disabled text behind the `=>` (FMT-UNSUP-012), `PrecheckPhase.FindTriviaBehindExpressionBodyArrow` | `ArrowToken.TrailingTrivia`, then `Expression.GetFirstToken().LeadingTrivia`, of every `ArrowExpressionClauseSyntax` in `DescendantNodesAndSelf()`; comments (`IsCommentTrivia`, doc comments included), `IsDirective`, `DisabledTextTrivia` | `ExpressionBodyRewriteUtilities.BlocksRewrite`, first condition: `ContainsDirectives(member, [ArrowToken.SpanStart, Expression.SpanStart))` | The same gap: the span holds exactly the arrow's trailing trivia and the first token's leading trivia. Directives agree. Disabled text always follows its directive in the same leading-trivia list, so a gap with disabled text also has a directive. Comments are left out of `BlocksRewrite` on purpose (SHARED-BODY-001: reported, fix withheld) | agree |
| The same gap, reported by RH5040 | comments between a member/accessor arrow and the token behind it (`IsMisplaced`) | the precheck span above | Same gap for ordinary comments. Local-function arrows are excluded by RH5040-002 and are covered by RH3208, as FMT-UNSUP-012's Why states | agree (rarity argument holds) |
| Keep a property's layout when the arrow join crosses a comment or directive in front of it (`PropertyLayoutLineBreakRewriter.CollapseExpressionBody`, kept) | `GetPreviousToken().TrailingTrivia` and `ArrowToken.LeadingTrivia` | the precheck span | The two spans meet at the arrow token, with no overlap and no gap between them. The deleted second check read exactly the precheck span. The rewriter visits only property and indexer declarations, so it always sees whole arrow clauses | agree |
| Get-only lift seams (`HasLayoutOnlySeams`) | kept: `ArrowToken.LeadingTrivia`. Deleted: `ArrowToken.TrailingTrivia` and the expression's first-token leading trivia | the precheck span | The deleted checks equal the precheck span. A getter converted from a block gets an arrow with empty trivia and an expression without leading trivia (`AccessorExpressionBodyTransform.ConvertToExpressionBody`, `TryRehostExpression`), so no supported input loses a check | agree |
| A comment stands directly at the arrow (`ArrowLineHoldsComment`, narrowed) | `ArrowToken.LeadingTrivia` from its end back to the last line break | FMT-STRUCT-006's definition ("precedes the arrow on a line on which no token stands in front of the arrow, behind the last line break") | Same boundary. The deleted trailing-trivia half is refused by the precheck | agree |
| RH3202–RH3208 withhold the fix (SHARED-FIX-010) | `FindUnsupportedSyntax(GetFormattingInput(node))`, which is the member, or for RH3208 the label that starts the line | the node formatted by `FormatMemberOnLinesOfItsOwnAsync` → `ConvertBodyAsync`, or for RH3208 `FormatLocalFunctionAsync`, on the same node | Same node. Checked: `apply-fix.sh RH3202` converts the neighbour `A`, and `apply-fix.sh RH3208` converts `K` beside `L() => /* c */ 1` | agree (test strength: finding 1) |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| The gap between `=>` and the expression holds no comment, directive, or disabled text | `rg -n "ArrowToken\.TrailingTrivia\|Expression\.GetFirstToken\(\)\.LeadingTrivia\|Expression\.GetLeadingTrivia\(\)\|expression\.GetLeadingTrivia\(\)" src --type cs -g '!*Test*'` → `PrecheckPhase.cs:300-301` (the shape). `ExpressionBodyToBlockConverter.cs:205,213` reads it only as layout: whitespace is dropped, and a line break is kept as the author's layout. `RH3003…CodeFixProvider.cs:46` is unrelated. `rg -n "ArrowToken" src -g '!*Test*'` → `BlocksRewrite` (analyzer decision for directives, SHARED-BODY-001, different decision), `ExpressionBodyCodeFixUtilities:80` (signature trivia), the kept in-front refusal, the kept arrow-leading seam, and the six transforms that pass the arrow to the converter | one owner |
| Trivia that counts for the shape | `FindFirstUnsupportedOperatorTrivia` shared with FMT-UNSUP-008/011; `SyntaxTriviaUtilities.IsCommentTrivia` / `IsDirectiveOrDisabledTextTrivia` | one owner, no copy |
| Which reason wins when FMT-UNSUP-010 and -012 start at the same directive | candidate order in `FindUnsupportedSyntax` (strictly-earlier wins; 010 listed before 012 in both lists) | one owner. No other shape can share a trivia with this gap: 004 excepts nothing that holds an arrow clause, 008/009/011 gaps end at tokens that cannot be the `=>` or the expression's first token |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `UnsupportedExpressionBodyArrowCommentTests` refusal rows (spec examples, 24 kinds, 11 positions, doc modes, directives, disabled text, nested, first token kinds) | input refused, unchanged, reason and location exact | output differs, no report, wrong reason or span, other tree | `AssertUnsupportedFor` (LF and CRLF, `AreSame` tree) | adequate |
| `…IsSupported` / `CommentOutsideTheGapIsSupported` / lambda and switch-arm rows | not refused, output a fixed point | refusal, second pass differs | `AssertSupported` (LF/CRLF, two passes, phase stability) | adequate |
| `OutputOfFormattedExpressionBodiesIsNotUnsupported` (FMT-UNSUP-005) | formatted output is not refused | second pass refused or changes | two passes, LF/CRLF | adequate; extended by my probe below |
| Node-level, built-node, `FindUnsupportedSyntax` parity tests | FMT-UNSUP-003 locality, query = precheck | expression alone refused; answers differ | direct, LF/CRLF | adequate |
| CLI `CommentOrDirectiveBehindTheArrowOfAnExpressionBodyIsReportedAsUnsupported` | exit 2, stderr line, file unchanged | any differs | `Program.Main` | adequate |
| RH3202–RH3208, RH7101 `VerifyNoFixIsOffered…` | diagnostic stays, no fix for any diagnostic | a fix offered | `VerifyNoCodeFixIsOffered` | adequate |
| RH3202 `VerifyFixIsOfferedForAMethodBesideAMethodWithACommentBehindItsArrow` | clean neighbour gets a working fix | action offered that changes nothing | `GetCodeFixActionsAsync` only | **weak — finding 1** |
| RH320x `VerifyCommentTrailingTheSemicolonSurvivesFix` | the unchanged half of the removed test, same expected output | output differs | `Verify` | adequate |
| `StructuralTransformRequirementTests.ExpressionBodyWhoseConversionWouldSplitAConditionalGroupIsKept` (FMT-STRUCT-024) | `M` keeps its expression body in both builds | `M` converted | syntax-tree inspection once, then `AssertSecondPassIsStable` (LF/CRLF) | adequate for the requirement, which decides only whether the rewrite applies |
| Changed in-front tests (`CommentsInFrontOfTheArrowLeadTheCreatedStatement`, `BlankLineBetweenTwoCommentsBeforeArrowIsKept`, Comment/DirectiveJoin, separator observer tests, idempotency fixture) | behavior of the still-supported side, unchanged from `main` (passed on 425afc1 before the production commit) | output differs | `AssertRuleResult`, phase observer, `AssertIdempotentUnderBothEndings` | adequate |

Visible output (check 10): every changed expected value is either a refusal or a withheld fix, which maintainer answers 1–5 of batch 1 cover (`00-lead.md`, change doc `Decisions`), or an output `main` already produced that an unchanged requirement demands (the in-front FMT-STRUCT-006/FMT-PIPE-013 cases, FMT-STRUCT-024's unbalanced group, FMT-BREAK-001's in-front join refusal, RH5401-004's member `D`, reported without a fix before and after). No rule page changed. Nothing is undecided.

Probes run (fixtures under the scratchpad, outside the repository):
- `scripts/format.sh` twice over doc comments in front of the arrows of a property, an indexer, and a getter (`int P /** c */ => 1;`, `int this[int i] /** d */ => i;`, `get /** e */ => _v;`), `return /** i */ _v;` in a getter block, `/** j */ get { return _v; }`, and lambda attribute lists with comments (`[A, /* g */ B] () => 1`, `[A] /* h */ [B] () => 1`) behind a property arrow. Result: not refused, second pass `Formatted 0 of 1`. No phase moved a comment behind an arrow.
- `scripts/apply-fix.sh RH3202` on two methods on one line and on separate lines: the clean method is converted, and the method holding the shape gets no fix. `RH3208` on `K() => 2;` beside `L() => /* c */ 1;`: `K` converted, `L` no fix.
- `scripts/apply-fix.sh RH3002` on `int P => (/* c */ _a);`: no fix offered, so the fix that removes the parentheses does not create the shape.

## Failure classes dismissed
- FC-001, FC-002: the check's span is exactly the spec's ("trailing trivia of the `=>`, or leading trivia of the expression's first token"). No qualifier is missing. See *Guard scope*.
- FC-003: both sides are tested: in front of the arrow, inside or behind the expression, behind `ref`, `throw`, and unary, lambda and switch arrows.
- FC-004: the deleted converter cases carried trivia that only an input the precheck refuses can hold. Every pipeline entry runs `PrecheckPhase` first (`FormattingPipeline.CreatePhases`). The two entry points without it (`OrganizeUsingDirectivesInDocumentAsync`, `FormatDocumentationElementInDocumentAsync`) run no changed phase. Every node that holds the `=>` holds the whole arrow clause, so no node-level input meets a partial gap.
- FC-005, FC-019, FC-020, FC-021: directive, disabled-text, and comment-position rows are separate tests.
- FC-006, FC-022, FC-023, FC-036, FC-018: no rewrite, literal, identifier, or rename is added.
- FC-007, FC-008: fixes are only withheld. No new fix output exists. Fix All skips a withheld diagnostic (`VerifyNoCodeFixIsOffered` covers every diagnostic of the document).
- FC-009, FC-043: the fixes ask about the node they format (*Guard scope*). The test gap is finding 1.
- FC-010, FC-012: every deletion is equivalent on supported input. `moveComments`, `lineBreak`, the brace's trailing trivia, the `return` comment move, and `LeadsWithCommentOnItsLine` each needed a comment or directive in the precheck span, and the remaining branches give the same result for whitespace and line breaks. The six transforms pass the original member's expression after `base.Visit…`, and no earlier rewriter of the phase writes a comment in front of an expression's first token.
- FC-011: no loop changed.
- FC-013, FC-014: RH5401 and RH3202–RH3208 still report a member the formatter refuses, which SHARED-DIAG-010's last sentence allows. Directives keep `BlocksRewrite`'s non-report (SHARED-BODY-001).
- FC-015, FC-016: one owner (*Policy ownership*). The sibling shapes, the lambda and switch-arm arrows, are excluded on purpose. The in-front shape is the maintainer's separate change (batch 1, answer 6).
- FC-017: see finding 1. The other tests are adequate.
- FC-024, FC-025, FC-026, FC-027: every arrow-clause owner, `async` forms, explicit interface implementation, nested and one-line forms, and two methods on one line are tested. The check dispatches on `ArrowExpressionClauseSyntax`, so `partial` and `checked` forms take the same path.
- FC-028, FC-041: node-level entry points, built nodes, and the expression alone are tested under LF and CRLF.
- FC-029: N/A.
- FC-030: RH3202 formats only the member (apply-fix shows `B` untouched).
- FC-031: the summaries of `CreateBlock`, `ArrowLineHoldsComment`, `CreateStatement`, `CreateReturnStatement`, `HasLayoutOnlySeams`, `CollapseExpressionBody`, and the precheck's ordering comment describe the new code. `BlocksRewrite`'s remarks are unchanged and still true for the analyzers.
- FC-032: the roles' focused results predate the merge of 71a89ec. The merge changed no line this run changed and no test input that holds the shape, and the validator runs the default set on the merged tree. I re-ran no test because no finding turns on one.
- FC-033, FC-034, FC-035: the expected values come from the spec examples, and every refusal test failed on 425afc1 for the expected reason (04).
- FC-037: N/A, not a text-only change.
- FC-038: no phase writes into the gap. The arrows the get-only lift and the accessor transform create have empty trivia and an expression without leading trivia, and doc-comment relocation moves a comment in front of the arrow, never behind it. The closure test covers this, and my probe above extends it.
- FC-039: `DescendantNodesAndSelf` visits every arrow clause, and no excepted region of another shape applies to this one. The nested rows, including a field with one declarator and a field with several, pass.
- FC-040: covered in all three documentation modes. `IsCommentTrivia` includes documentation trivia.
- FC-042: every deletion the change doc lists under *Special cases → Deleted* is in the diff. No other guard reads the gap (*Policy ownership*). `BlocksRewrite`'s directive check stays for the analyzers, as the change doc records. The remaining spec mentions only point to FMT-UNSUP-012.
- FC-044: no modelling. The check is a trivia-kind test on two lists.

## Architecture findings
_None._

## Hints
- `src/Reihitsu.Formatter.Test/Unit/StructuralTransforms/ExpressionBodiedTransformTests.cs:807` `KeepsCommentTrailingArrowInIndexer` still feeds the structural phase an input that the precheck now refuses, an input no caller passes (`AGENTS.md`, *Supported inputs*). 04 deleted the other phase-level tests for that reason and kept this one because it tests no deleted case. It is harmless, but it is inconsistent with those deletions.
- Pre-existing and not traced: `Func<int> G => [A] /* h */ [B] () => 1;` formats as `Func<int> G => [A] /* h */⏎[B]⏎() => 1;`, with the lambda's attribute lists and parameter list each on lines of their own. The output is stable and not refused, and the run's code is not involved. I did not check it on `main`.

## Follow-up candidates
_None._ The probes found nothing new. The in-front shape and the skeptic's column follow-ups are already in `00-lead.md`.
