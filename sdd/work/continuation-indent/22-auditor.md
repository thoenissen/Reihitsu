Role: auditor
Commit: 9df98aa
Verdict: #953: findings; #1852: pass; #1896: pass; #1818: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `9df98aa` (the prompt named `f434b0b`; `9df98aa` is one later commit that changes only `sdd/work/continuation-indent/00-lead.md`, so the audited code tree is `f434b0b`'s). `origin/main` = `git ls-remote origin refs/heads/main` = `ab547b7cdad3279b17a9cfb87428dff6e56314b7`, an ancestor of HEAD (merged in `9f1eb7d`). `scripts/lane-check.sh behavior-change`: confirmed. The diff matches handovers 10, 12, 14, 18, 20: 21 production files (6 new types), specs, change docs, tests; no edit outside the run's scope found (`ModifierOrderingUtilities.cs` and the RH2001 provider change only doc comments).

Evidence: the Release CLI built from the head (`src/Reihitsu.Cli/bin/Release/net11.0`, built after the last `src` commit) run twice over fixtures in the scratchpad (`fx/*.cs`), LF and CRLF; `main`'s CLI built from a temporary worktree of `ab547b7` (removed again) for before/after; `scripts/apply-fix.sh RH5302` on a tab-indented and a space-indented fixture (4 fixed, converged in 1 iteration, tab counted as one column on both sides). Branch CLI over a copy of `main`'s whole `src/` (2034 files): `Formatted 0 of 2034`, nothing refused, so the repository's own code is a fixed point of the new formatter.

## thoenissen/Reihitsu#953

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/PatternLineStartUtilities.cs` `StartsOwnLine`, read by `ContinuationJoinRewriter.IsJoinedGap` and `BinaryOperatorLineBreakRewriter.NormalizePatternChain`; counterpart `LineBreakInitializerRewriter.VisitRecursivePattern` / `IsIntroducedByCaseLabel` | FC-001, FC-002, FC-015 | – | same requirement | FMT-BREAK-017 and FMT-BREAK-041 keep a pattern's first token below `not`/`and`/`or` only "where FMT-BREAK-022 starts its first token on a line of its own". FMT-BREAK-022 does not do that for a property pattern introduced by a `case` label (looking through `and`/`or`/`not`): the brace stays where it is. `StartsOwnLine` lacks that qualifier, so in a case label the join is skipped and the layout depends on where the author broke the line. Counterexample, inside `switch (o)` with `class P { public int A { get; set; } }`: `case P and⏎{⏎A: 1⏎}:` gives `case P` / `and` / `{` / `A: 1` / `}:`; `case 1 or not⏎{⏎A: 2⏎}:` gives `case 1` / `or not` / `{` …; but the same tree written `case not {⏎…⏎}:` / `case 1 or {⏎…⏎}:` gives `case not {` and `or {` (both forms stable on a second pass, LF and CRLF). So one tree has two fixed points, and the first form leaves a combinator alone on its line with the brace in the section body's column, against FMT-BREAK-017's "the right pattern of each combinator MUST join it". Failing members: a multi-line property pattern without a type behind `not`, `and`, or `or`, in a pattern a `case` label introduces directly or through combinators and `not`, with a line break in that gap. Members that hold: the same gaps with the brace already on the combinator's line; a multi-line parenthesized pattern anywhere (FMT-BREAK-022 has no case-label exception for `(`); the same patterns behind `is`, in a switch arm, in a `when` clause, and inside a parenthesized pattern in a case label (`case (1 or⏎{…}):`), where FMT-BREAK-022 does move the brace. `main` produces the same split layouts, so the divergence is between the run's rewritten FMT-BREAK-017/041 and code that copied FMT-BREAK-022's predicate without its exception. No test covers a case label with a multi-line property pattern behind a combinator (`PatternChainLayoutTests` has `case 1 or⏎2:` only). Note: FMT-BREAK-041's enumerating sentence ("the opening brace of a property pattern without a type that spans several lines … keep their own line below the `is`") gives a second reading for the `not` member; FMT-BREAK-017's text has only one. | Make `StartsOwnLine` read FMT-BREAK-022's own predicate (exclude a property pattern `LineBreakInitializerRewriter.IsIntroducedByCaseLabel` answers true for, with the `RootConstruct` fact for a node-level root), and add the case-label rows (LF/CRLF, both break positions) to `PatternChainLayoutTests`. If the lead prefers today's output instead, FMT-BREAK-017/041 must name the case-label exception, which is a visible output choice. |
| 2 | medium | `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` `FindTriviaBehindAssignmentInArm`; `sdd/specs/formatter/unsupported-syntax.md` FMT-UNSUP-019; `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedAssignmentInArmCommentTests.cs` | FC-027, FC-017 | – | same requirement | FMT-UNSUP-019 keys on `AssignmentExpressionSyntax`, which Roslyn also uses for the members of an object initializer, a `with` initializer, and an index initializer. So `x switch { 1 => new C2 { A = /* c */ 3 }, _ => 0 }` is refused (CLI: `(1,59): unsupported syntax: a comment or directive behind an assignment operator inside a switch arm or an anonymous-object member`), while `var o = new C2 { A = /* c */ 3 };` outside an arm is formatted. That follows the spec's wording (only declarator, property-initializer, enum, parameter, anonymous-member and `let` `=` are excluded) and maintainer answer 8's class, but neither the spec's conditions/examples nor any test names an initializer member; the ten refused rows of `EveryAssignmentInsideAnArmOrAMemberIsUnsupported` are all plain assignment expressions. A later narrowing to "assignment expression statement-like" would change which files the CLI refuses with no test failing. | Add refused rows for an object-initializer member, a `with`-initializer member, and an index initializer (`[0] = /* c */ 1`) inside an arm and inside an anonymous-object member, and an other-side row outside both; state in FMT-UNSUP-019's first condition that initializer members count. |
| 3 | low | `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/BinaryOperatorLineBreakRewriter.cs` summary of `NormalizeBinaryOperatorPosition` | FC-031 | – | PR-introduced | The inserted sentence splits the old one: "…and an `is` that starts its line stays there, unless a line break already separates the left operand from the operator, which then stays the gap's only one (FMT-BREAK-030)". The `unless` clause belonged to "moving end-of-line trivia to the left operand" and now reads as a condition on the type-test `is`, which the code returns unchanged unconditionally. | Restore the `unless …` clause to the first sentence and state the type-test exception as its own sentence. Comment-only. |
| 4 | low | `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs` `ComputeDocumentLineStartColumn` | FC-015 | – | PR-introduced | It re-implements `ComputeDocumentColumn`'s counting rule (each character one column, a leading byte order mark zero) with its own copy of the BOM decrement, and adds its own notion of leading whitespace (space, tab, BOM only). The summary claims parity ("counted as `ComputeDocumentColumn` counts"); today it holds, but the column policy of FMT-PIPE-017/024 now has two owners. | Compute the line-start column through the one counting rule, e.g. `ComputeDocumentColumn(token) - (token.SpanStart - position)`, so the BOM rule lives once. |

### Architecture findings

Finding #1: `LineBreakInitializerRewriter` (FMT-BREAK-022: `VisitRecursivePattern` with `IsIntroducedByCaseLabel`, `VisitParenthesizedPattern`) and the run's new `PatternLineStartUtilities.StartsOwnLine` both decide whether the opening delimiter of a multi-line pattern starts its own line; `StartsOwnLine` is a private restatement of that decision without the case-label exception, and `BinaryOperatorLineBreakRewriter.IsMultiLineDelimitedPattern` is a third, broader statement used for "is the chain wrapped". The inputs in #1 show the first two disagreeing.

### Hints

- Deep alternation of comment-kept continuation lines and nested constructs needs one alignment sweep per level, because `ContinuationLineContributor` runs first in a sweep and reads the construct's line from the previous sweep. With collection expressions nested in arguments (`[⏎x += // c⏎M([⏎x += // c⏎M([ …`), seven levels settle; nine levels hit `LayoutComputer.MaxAlignmentPasses` (8) and the two innermost `M([` lines stay misplaced (column 86 instead of 94), stable on a second pass but not FMT-INDENT-034's column. Adversarial nesting only; the bound's comment ("generated adversarial nesting four") predates this dependency.
- `ComputeDocumentLineStartColumn` counts the whitespace at the start of the document line of the root's first token, while whole-document formatting measures a line that begins inside a multi-line string or comment from the line where that text starts (`GetLineOfText`). A node-level root whose first line begins inside such text (e.g. behind `*/` or inside a verbatim string) could get a different column from a code fix than from the CLI. Not constructed through a fix; rare and possibly unreachable.
- `GetLineOfText` and the verbatim-interpolated-hole case it serves cost about 25 lines for a rare class (decision 7 kept it under the standing instruction). Not a finding: the spec demands it, but it is the kind of case escalation 10's question already covers.

### Follow-up candidates
_None._ (The `case⏎(` layout seen in `case (1 or⏎{…}):` is already in `00-lead.md`'s follow-up list.)

## thoenissen/Reihitsu#1852

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._ Checked: `ExpressionBodyToBlockConverter.CreateReturnStatement` carries only the arrow's non-whitespace trailing trivia (line breaks; FMT-UNSUP-012 leaves no comment there), and the `return` join removes them. Method, operator, destructor, local function, void and throw bodies with `=>⏎⏎expr` and `=>⏎expr` all convert to single-spaced statements in one pass, stable, LF and CRLF (`fx/eb.cs`). `return⏎;` keeps its break and becomes a continuation line, as FMT-BREAK-041 states.

### Architecture findings
_None._

### Hints
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1896

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._ `QueryClauseContributor` is the only owner of the clause column (the `QueryBodySyntax` anchor in `ListElementAligner.AlignContinuations` is deleted); clauses, `select`/`group`, and `into` align with `from` through continuations; a line inside a clause (`on x⏎equals b`) is a continuation of the clause; the `let` `=` joins both sides. Verified with the FMT-INDENT-035 example shapes and inside an argument.

### Architecture findings
_None._

### Hints
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1818

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._ Declarators behind `#endif`/`#else` in an event field and in locals (also nested in a block) take the declaration's line + 4; declarators in disabled text keep their column (FMT-INDENT-011); stable LF and CRLF (`fx/c1.cs`, `fx/lo.cs`). A multi-declarator field with a directive stays FMT-UNSUP-010's.

### Architecture findings
_None._

### Hints
_None._

### Follow-up candidates
_None._

## Whole run

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which gaps are joined / refused (FMT-BREAK-041, FMT-UNSUP-017) | `PrecheckPhase.FindTriviaInJoinedGap`: each pair of consecutive non-empty tokens of the node, `previous.TrailingTrivia` + `token.LeadingTrivia`, gap from `JoinedGapUtilities.GetGap` | `ContinuationJoinRewriter.SelectJoinableGaps`: same pairs, same `GetGap`, edits exactly those two trivia lists (`LineBreakTriviaUtilities.JoinGaps`) | Same span, same gap list; the join's extra `StartsOwnLine` condition only narrows the join, the refusal stays "on any line" | holds |
| Gaps joined by other owners (combinator, anonymous/`let` `=`, parameter modifier) | Precheck refuses `PatternCombinator`, `AnonymousMemberEquals`, `LetClauseEquals`, `ParameterModifier` | `NormalizePatternChain` (operator trailing + right first token leading), `LineBreakAssignmentRewriter.NormalizeValuePlacement`/`NormalizeOperatorPlacement`, `ListElementInteriorJoinRewriter` | Spans agree | holds |
| Type-test `is` | Join: `GetGap` → `TypeTest`, joined by `ContinuationJoinRewriter`; not refused by FMT-UNSUP-017 | FMT-UNSUP-011: binary operator trailing + right operand leading | Same gap | holds |
| Pattern delimiter keeps its own line | `StartsOwnLine`: `{` of a multi-line property clause of a recursive pattern without type and positional clause; `(` of a multi-line parenthesized pattern | `LineBreakInitializerRewriter.VisitRecursivePattern` (any multi-line property clause, except `IsIntroducedByCaseLabel`), `VisitParenthesizedPattern` (no exception) | `(` agrees; `{` disagrees in case labels | finding #1 |
| Chain is wrapped | `IsWrappedChain`: line break next to a combinator, or a multi-line property/list/parenthesized leaf next to one | FMT-BREAK-017's definition | Agrees | holds |
| FMT-UNSUP-019 scope | Operator trailing + right first token leading; walk to nearest arm / anonymous member / statement / member declaration, inside the node | FMT-UNSUP-019 text; FMT-UNSUP-003 | Agrees with the text; includes initializer members (finding #2) | holds (test gap #2) |
| FMT-UNSUP-018 vs `ListElementAligner` zero-width anchor | `<` trailing, commas both sides, `>` leading, all-omitted lists only | `AlignToFirstElement` now anchors on the zero-width token; wrapped all-omitted lists cannot reach it (refused or joined by FMT-BREAK-014) | Agrees | holds |
| Continuation vs other placements | `LayoutModel.SetContinuation`: skips lines in `_alignmentContributors` or with another fallback owner | `Set`, `SetUnlessPlaced` | Every other contributor wins in the same and later sweeps; pass-1 block indentation is overridden as FMT-INDENT-034 states | holds |
| Scope parts | `IsScopeToken`/`IsScopePart`/`IsFirstTokenBehindAttributeLists` | FMT-INDENT-034's list | Matches (extern alias directive is an extra, harmless scope part) | holds |
| Root line start | `TryGetLineColumn`: `RootLineStartColumn` for the root's first line | whole-document `GetLineOfText` + placed column | Agrees except a root line that begins inside multi-line text (hint) | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| The joined gaps | `JoinedGapUtilities.GetGap` (read by `PrecheckPhase.FindTriviaInJoinedGap`, `ContinuationJoinRewriter.IsJoinedGap`) | one owner |
| What a modifier is | `JoinedGapUtilities.IsModifier`; `ModifierOrderingUtilities`/`DeclarationModifierUtilities` (Core) decide order, not gaps | one owner |
| Multi-line pattern delimiter on its own line | `LineBreakInitializerRewriter.VisitRecursivePattern`/`IsIntroducedByCaseLabel`/`VisitParenthesizedPattern`; `PatternLineStartUtilities.StartsOwnLine`; `BinaryOperatorLineBreakRewriter.IsMultiLineDelimitedPattern` | three statements, drift shown — finding #1 |
| Query clause column | `QueryClauseContributor` (the copy in `ListElementAligner` deleted) | one owner |
| Continuation column | `ContinuationLineContributor` via `LayoutModel.SetContinuation` (only caller) | one owner |
| Document column counting (tab one, BOM zero) | `ReihitsuFormatterHelpers.ComputeDocumentColumn`, `ComputeDocumentLineStartColumn` | two owners — finding #4 |
| Operator spacing incl. combinators, pattern `is`, `let` `=` | `OperatorSpacingRule` | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ContinuationLineIndentationTests` (all) | exact columns of continuation lines | any other column, second pass change, CRLF difference, a phase alone changing the output | `AssertRuleResult` (LF+CRLF, two passes, phase stability) | adequate |
| `ContinuationJoinTests.EachGapIsJoined`, `ModifierGapsOfOtherDeclarationsAreJoined` | exact joined output | unjoined gap | `AssertRuleResult` | adequate |
| `ContinuationJoinTests` accessor-modifier test | `private set;` joined | `private` alone on its line | `Assert.Contains` + `AssertSecondPassIsStable` | weaker than exact output but the requirement under test is the join; acceptable |
| `ContinuationJoinTests` other-side rows | the line break stays | joined gap | `AssertLineBreakIsKept` (line ends with/starts with) + stability | adequate for "keeps its break" |
| `PatternChainLayoutTests` | exact chain layout | combinator not at line start | `AssertRuleResult` | adequate; missing the case-label property-pattern cells (finding #1) |
| `QueryClauseAlignmentTests` | exact clause columns | other column | `AssertRuleResult` | adequate |
| `UnsupportedJoinedGapCommentTests`, `UnsupportedOmittedTypeArgumentCommentTests` | refusal, reason, location; other side formatted; closure | supported input refused or vice versa | `AssertUnsupportedFor` / `AssertSupported` (LF+CRLF, doc modes) | adequate |
| `UnsupportedAssignmentInArmCommentTests` | refusal inside arm/member only | refusal outside, none inside | `AssertUnsupportedFor` / `AssertSupported` | adequate for its rows; initializer members missing (finding #2) |
| RH5302/RH5308 `VerifyFixIndentsAContinuationOnTheRootsFirstLineFromTheDocumentsLineStart` | fix output equals whole-document column | column from the root's first token | exact string, LF+CRLF | adequate |
| `ReihitsuFormatterDetachedRootTests.ChainPrefixTarget…`, `LayoutComputerTests.ComputeHandlesLinqExpression` | new continuation columns | old columns | exact | adequate |

### Visible output is decided

Sampled the changed expected values (`ZeroWidthTokenLineStartTests`, `WithExpressionSpacingTests`, `RegionFormattingFullPipelineTests`, `LayoutComputerTests`, `ReihitsuFormatterDetachedRootTests`, the RH5302 test) and the probes' before/after against `main`: each change is a join of FMT-BREAK-041/016/017, a refusal of FMT-UNSUP-017/018/019, a continuation column of FMT-INDENT-034/035, or the spacing of decision 20 — covered by maintainer answers 1b/1c/1d/1e/2/3/4b/5-10, batch 2, and the standing instruction (decisions 5-8, 11, 14, 18, 20). No uncovered expected value found. Finding #1 concerns an input no test pins.

### Failure classes dismissed

- FC-003: boundaries of the new guards are tested on both sides (other-side tests for every FMT-UNSUP-017 row, one-line vs multi-line, both builds of conditional modifiers).
- FC-004, FC-019, FC-020, FC-021: every join of the run meets no comment, directive, or disabled text, because the precheck refuses them on the same span (guard scope table); the field-split, attribute-list, doc-comment relocation, and expression-body conversion write no trivia into a joined gap.
- FC-005: `FindFirstUnsupportedOperatorTrivia` covers comments, directives, and disabled text alike.
- FC-006, FC-024, FC-025: joins and the `return` conversion change only whitespace and line breaks; the seven expression-bodied kinds convert in one pass (`fx/eb.cs`).
- FC-007, FC-008, FC-009, FC-018, FC-030, FC-043: no code-fix logic changed (RH2001 doc comment only); fixes delegate to the formatter, whose refusal withholds them (SHARED-FIX-010); RH5302 converges in one iteration with tabs and spaces.
- FC-010: every probe stable on a second pass under LF and CRLF.
- FC-011: the join and the chain layout run once per rewriter; the alignment sweeps stay bounded (see hint for deep nesting).
- FC-012: no later phase or rewriter re-breaks a joined gap (base list, `with`, switch arm, `catch … when`, `for` header probes stable).
- FC-013, FC-014: no layout rule's analyzer covers the joined gaps, pattern chains, or query clauses; RH6001/RH6006 do not report the new spacing; `main`'s whole `src/` is a fixed point of the new formatter.
- FC-016: sibling shapes enumerated for #1; FMT-UNSUP-017 table rows each tested.
- FC-017: see assertion table.
- FC-022: continuation lines next to raw strings and raw interpolation holes stable, the hole lines stay FMT-RAW's (`fx/rs2.cs`).
- FC-023, FC-036: no identifier handling or rename.
- FC-026: `JoinGaps` and `NormalizePatternChain` key replacements by token identity.
- FC-027: nested chains through parenthesized patterns, chains inside property/list/positional subpatterns, lambdas in arguments, initializers, switch arms checked (`fx/p1.cs`, `fx/c1.cs`); initializer members of FMT-UNSUP-019 are finding #2.
- FC-028: `FormatNode` measures from the root's first token by design (FMT-INDENT-034); the in-document entry points pass `RootLineStartColumn`; RH5401/RH5419 use `FormatNode` only as a one-line probe on member declarations, which start their line.
- FC-029: no key/value layout changed.
- FC-031: finding #3; other changed summaries (`ContinuationJoinRewriter`, `ListElementAligner`, `ExpressionBodyToBlockConverter`, `LayoutModel.SetContinuation`, `OperatorSpacingRule`, entry points) describe the new code.
- FC-032, FC-035: no project results reused; expected values checked against the spec examples and `main`'s CLI output.
- FC-033, FC-034: reproductions use the issues' shapes (comment-held `x +=`, `#endif` declarators, `return⏎x`, query clauses) under LF and CRLF.
- FC-037: no comment-only claim made by grep.
- FC-038: no phase writes a comment, directive, or disabled text into a declared gap (the `return` takes only line breaks; the chain layout appends a line break behind a comment in front of a combinator, which is outside the gap).
- FC-039: `FindTriviaInJoinedGap` walks every token, `FindTriviaInOmittedTypeArgumentListOrBehindAssignmentInArm` every node; the FMT-UNSUP-019 walk stops at the nearest arm/member/statement, so nesting is decided per assignment.
- FC-040: `///` and `/** */` rows in both documentation modes are tested.
- FC-041: the gap needs both tokens inside the node; the FMT-UNSUP-019 walk stops at the node.
- FC-042: the change docs' *Checked and kept* guards each serve another construct or the RH3209/RH3210 fixes on unformatted input; no guard found that only serves a declared shape.
- FC-044: no guard models a compiler or later-phase result; `IsIntroducedByCaseLabel` is a cheap ancestor check.
- Tab-width matrix: `ComputeDocumentLineStartColumn` counts a tab as one column like `ComputeDocumentColumn`; RH5302 fix with tab-indented method converges and keeps the target's lines consistent with the tab counted as one (FMT-PIPE-017).

### Commands

```text
scripts/lane-check.sh behavior-change                                  LANE CHECK: confirmed
Release CLI twice over fx/{b1,b1crlf,c1,d1,p1,s1,cl,cl2,cl3,eb,ebcr,lo,locr,rs2,rstab,deep3..9}.cs   all stable; findings above
Release CLI --suppress-unsupported over fx/u/u1..u20.cs                9 formatted, 11 refused, each as the spec decides
main CLI (ab547b7) over the same fixtures                              before/after compared
branch CLI over a copy of main's src/                                  Formatted 0 of 2034 file(s)
scripts/apply-fix.sh RH5302 af1/                                       4 fixed, 0 not-converged
```
