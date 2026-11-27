Role: auditor
Commit: f371753c7c411c08d8a63898342088c2fcab07c2
Verdict: pass, text cleanup (#1568: pass; #1698: pass; #1567: pass, text cleanup)

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against 498009c1169899ea8721d18776d8dae16ec03dae, repair round 1. Head f371753c7c411c08d8a63898342088c2fcab07c2 is checked out and the tree is clean. Base `origin/main` is fe7b8f85d45a85e3354da783ca73a20b3e39eb6d, freshly fetched, and it is an ancestor of the head. The diff matches handovers 12–17:
- `ListElementInteriorJoinRewriter.IsOwnedGap`: one condition changed, plus its comment.
- `LineBreakListRewriter.VisitParenthesizedLambdaExpression`: comment only.
- Four `ContinuationJoinTests.EachGapIsJoined` rows.
- FMT-BREAK-015, FMT-BREAK-045, and FMT-RAW-007 wording.
- Two change-doc rows and one new change-doc decision.

`scripts/lane-check.sh behavior-change`: confirmed. The scope did not grow: no new surface, and no spec change beyond the wording that follows the single owner and the FMT-RAW-007 deferral.

Evidence beyond reading: one fixture in the session scratchpad (`r1/A.cs`, outside the repository). It holds a lambda with a return type in these positions: an index argument (with `static` and a blank line), a tuple element, an argument with a multi-line parameter list, a named argument, a block body holding a nested return-type lambda, a later argument, the first argument behind a line break, a line comment, a delimited comment, `#if/#else` in the gap, a conditional branch inside an argument, an attribute list in front, an argument of a `new` expression, a regular interpolation hole, and a raw interpolation hole. I formatted it twice at the head and twice in a worktree at 498009c (worktree removed afterwards). Both runs were stable on the second pass and their outputs are byte-identical.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Four rows under `[Requirement("FMT-BREAK-041")]` cover the issue's three inputs (`static` value, `async` value, conditional branch) and the issue's "Before" layout with `(` in the statement's column. Each expected value differs from its input, so a lost join fails the row (`AssertRuleResult`, LF/CRLF, second pass). |
| 2 | closed | The member it named (`IsOwnedGap` comment, lines 165–167) now states the ownership the code implements: `ListElementInteriorJoinRewriter` refuses the `(` of every parameter list except that of `ParenthesizedLambdaExpressionSyntax { ReturnType: null }`, and `LineBreakListRewriter` joins the gap behind a return type. The class-level remark of the same file was not named in #2 and is still stale; see #4. |
| 3 | closed | The FMT-RAW-007 second bullet now defers to FMT-INDENT-006 ("the column FMT-INDENT-006 gives it in front of the token it precedes") without a paraphrase. `rg` finds no other restatement of "one level deeper than a closing brace" in `sdd/specs` or `src`. The change doc's Affected row lists `FMT-RAW-007 (clarified)`. |

## thoenissen/Reihitsu#1568

No file of this issue changed in the repair: `LambdaAlignmentContributor`, `LayoutModel`, FMT-INDENT-031, the change doc, and the tests are byte-identical to 498009c. The first audit's evidence and verdict stand.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

### Guard scope
_N/A — no guard of this issue changed in the repair._

### Policy ownership
_N/A — unchanged since 498009c (one owner, `LambdaAlignmentContributor`)._

### Assertion adequacy
_N/A — no test of this issue changed in the repair._

### Failure classes dismissed
All dismissals of `11-auditor.md` hold unchanged: the code they were traced against is byte-identical.

### Architecture findings
_None._

### Hints
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1698

The repair changed text only: the FMT-RAW-007 bullet and one Affected row in the change doc. `CommentIndentationContributor` and its tests are byte-identical to 498009c.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._ (#3 closed.)

The new wording was checked against the code. A comment line in a raw hole takes the column FMT-INDENT-006 gives it in front of the token it precedes, measured after FMT-RAW-007 placed that token. `CommentIndentationContributor.AlignCommentsBeforeToken` with `ClosesContentOneLevelDeeper` does this: brace column for a nested regular string's `}`, one level deeper for the raw hole's own `}}` (first-audit fixture `f3/H.cs`; `InterpolationClosingBraceCommentTests`). The deferral also covers the collection-expression and pattern closers that the old paraphrase omitted (14).

### Guard scope
_N/A — no predicate changed; spec text only._

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Column of a comment in front of a closing delimiter (spec statement) | FMT-INDENT-006 only; FMT-RAW-007 now references it. RH5204-002 and SHARED-INIT restate it only for braces where FMT-INDENT-006 still gives one level deeper (14, checked) | one statement |

### Assertion adequacy
_N/A — no test changed._

### Failure classes dismissed
- FC-015: the duplicate statement was removed (finding #3 closed).
- All other dismissals of `11-auditor.md` hold: no compiled code of this issue changed.

### Architecture findings
_None._

### Hints
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1567

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 4 | low | `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ListElementInteriorJoinRewriter.cs:32` (class `<remarks>`, second paragraph) | FC-031 | #2 | PR-introduced | The class remark lists among the gaps "another owner already lays out" "the opening parenthesis of a parameter list other than a lambda's". After 8fcadf5, the `(` of a parenthesized lambda with an explicit return type is also owned elsewhere (by `LineBreakListRewriter`, FMT-BREAK-045). The remark therefore says this rewriter joins that gap, while the code and FMT-BREAK-015 (`line-breaks.md:1083`) say it does not. This is the same stale ownership statement for the same gap in the same class as #2, in a member #2 did not name. Text only: an XML-doc remark on an internal class; it cannot reach compiled behavior, diagnostics, or rule metadata. | Reword to "other than that of a parenthesized lambda without an explicit return type", as FMT-BREAK-015 reads. |

Code checked against FMT-BREAK-015 and FMT-BREAK-045 (repair wording):
- `IsOwnedGap` refuses the `(` of a lambda's parameter list exactly when `ReturnType != null`.
- `VisitParenthesizedLambdaExpression` joins exactly when `ReturnType != null`.
- The two conditions are complements, so each gap has one owner.
- `LineBreakListRewriter` reaches every lambda that `ListElementInteriorJoinRewriter` reached: all its overrides call `base` first.
- The list decisions that read the written node still decide the same way. `VisitArgumentList` and `VisitAttributeArgumentList` decide on the visited node. `CanSafelyCollapseInteriorList` refuses only when the written element and the visited element are both multi-line, so a lambda the visit joined onto one line still collapses. The type-argument and type-parameter lists cannot hold a lambda.
- A node-level root that `RootConstruct` marks as lying in a list element joins the same way: the return type is inside the root, which both rewriters require.
- The fixture above confirms byte-identical output.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether the list-element join leaves a lambda's `(` gap to another owner (`ListElementInteriorJoinRewriter.IsOwnedGap`) | the `(` token, its `ParameterListSyntax` parent, and that list's owner's `ReturnType` | `LineBreakListRewriter.VisitParenthesizedLambdaExpression` (`node?.ReturnType == null ? node : CollapseOwnedOpenParenToDeclarationLine`) | complementary on the same node property. Both refuse trivia through `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia`, and their line-break tests (`AreSeparatedByEndOfLine` vs `HasLineBreakBetween`) differ only for trivia that the shared guard refuses | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| The return type → `(` gap of a parenthesized lambda | `LineBreakListRewriter.VisitParenthesizedLambdaExpression` → `CollapseOwnedOpenParenToDeclarationLine` (`rg -n "CollapseOwnedOpenParenToDeclarationLine" src`: one implementation, reused by `DeclarationBraceLineBreakRewriter`). `ListElementInteriorJoinRewriter` now refuses it | one owner (the overlap of #2 is gone) |
| Statements of that ownership | Spec: FMT-BREAK-011 (`line-breaks.md:590`), FMT-BREAK-015 (`:1083`), FMT-BREAK-045 (`:4574`), and the change-doc decision "One owner for the gap, also inside a list element". Code comments: `IsOwnedGap`, `LineBreakListRewriter:638-640`, the `CollapseOwnedOpenParenToDeclarationLine` summary, and the `ListElementInteriorJoinRewriter` class remark | all agree except the class remark (#4). The change doc's lines 15 and 141 describe base fe7b8f8, and the new decision explains the shift |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ContinuationJoinTests.EachGapIsJoined` rows "Modifier of a parenthesized lambda", "…whose parenthesis starts in the statement's column", "Async modifier…", "…in a conditional branch" | FMT-BREAK-041 joins a lambda's modifier with its `(` as a whole value (finding #1) | `static⏎(int x)` kept on two lines | `AssertRuleResult` (exact output, LF/CRLF, second pass, phase stability) | adequate. Each expected value differs from its input, and the rows cover the issue's three inputs and its "Before" layout |
| Return-type lambda inside a list element after the ownership move | joined by FMT-BREAK-045 alone | `N(int⏎(x) => x)` kept on two lines | existing `LambdaReturnTypeSpacingTests.KeywordArrayAndGenericReturnTypesGetOneSpace` / `NullableAndTupleReturnTypesGetOneSpace` (FMT-BREAK-031 tag, `AssertRuleResult`), green in the implementer's formatter run (7541/7541) | adequate. The moved case is pinned by existing tests, so no new test is required |

### Failure classes dismissed
- FC-001/002: the changed predicate and its counterpart key on the same property (`ReturnType`) and are exact complements (guard-scope row).
- FC-003: both sides (with and without a return type) inside a list element are pinned (`LambdaReturnTypeSpacingTests`, `ListElementInteriorJoinTests`), and both are in the fixture.
- FC-004/005/019/020/021: a line comment, a delimited comment, and `#if/#else` in the gap inside an argument keep their line breaks at head as at 498009c (fixture). Both owners use the same trivia guard.
- FC-010–012: the fixture is stable on the second pass at head.
- FC-015: one owner now (policy row).
- FC-017: see assertion adequacy.
- FC-028: the node-level root in a list element is traced above. The RH5111 fix path test is unchanged and green (13).
- FC-034: closed with #1.
- FC-006–009, FC-013, FC-014, FC-018, FC-022–027, FC-029, FC-030, FC-036, FC-038–044: no token, literal, rule, code-fix, precheck, or modeled-result change in the repair; the first audit's dismissals hold.

### Architecture findings
_None._ #4 repeats #2 (a stale ownership statement in a comment). It has no structural cause: the code and the spec agree on one owner.

### Hints
- Pre-existing at fe7b8f8 and unchanged by the run: for a parenthesized lambda without a return type that has a modifier and stands inside a list element (`N(static⏎(int x) => x)`), both `ContinuationJoinRewriter` (FMT-BREAK-041) and `ListElementInteriorJoinRewriter` (FMT-BREAK-015) can join the modifier → `(` gap. The continuation join runs first, and the precheck refuses trivia there (FMT-UNSUP-017), so the output is the same either way. The new `LineBreakListRewriter` comment names both requirements. Not a finding.

### Follow-up candidates
_None._

## Whole run (repair delta)

- **Visible output is decided (check 10).** The four new expected values are FMT-BREAK-041's join, which the run does not change. They characterize behavior that already exists. No other expected value changed, and no rule page changed.
- **Cost of special cases (check 11).** The repair adds no special case. It narrows one exemption, `{ ReturnType: null }`, to remove a duplicate owner.
- **Scope (check 9).** Every edit serves findings #1–#3.
- **Comments (check 8).** There is no `TODO` and no tracker reference in the added lines.
- **Verdict.** `pass, text cleanup` applies because #4 is confined to an XML-doc remark on an internal class and cannot reach compiled behavior, diagnostics, or rule metadata.
