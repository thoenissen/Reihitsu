Role: auditor
Commit: b4f2c1dcf634c6013342127a6ac92f6d4a982991
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit of thoenissen/Reihitsu#1935. Head b4f2c1d (checked out, clean tree); `origin/main` 1788f94d265c4e13052d5145404ae3ffd21e6acc, an ancestor of the head. Diff `git diff 1788f94 -- . ':(exclude)sdd/work'`: the change doc, `sdd/specs/formatter/blank-lines.md`, `BraceBlankLinePolicy.cs`, `CommentIndentationContributor.cs`, the new `PositionalPatternClosingParenthesisBlankLineTests.cs`, and one RH3202 test. This matches handovers 01–07. No path outside `sdd/work` changed after 3a0c01e. `scripts/lane-check.sh behavior-change` confirms the lane.

Counterparts I opened outside the diff:
- `BlankLineTokenCleanupRewriter.VisitToken` and `RemoveBlankLinesDirectlyBeforeToken`;
- `BlankLineRegionDirectiveRewriter:90`;
- `PrecedingTokenFacts` and `SyntaxTriviaUtilities.IsContentOpeningToken`;
- `ListElementInteriorUtilities.IsOwningConstruct` and `CommentPositionUtilities.IsOwningConstruct`;
- `FormatterTestsBase.AssertRuleResult` and `AssertBlankLinesAbove`.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

**Against the specs.** FMT-BLANK-019, -009, -018, and -020 hold for every member of the skeptic's matrix (02, members 1–18). The new tests pin members 1–16 and 18 exactly or by blank lines. Member 17 is pinned by `KeepsBlankLineBeforeClosingParenthesisOfParenthesizedVariableDesignation`.

I also formatted a corpus outside the repository with `scripts/format.sh` on b4f2c1d. Each positional shape sits next to its list-pattern analogue. Every shape gave the list analogue's blank lines, and a second pass changed nothing. The shapes:
- first subpattern on the `(` line;
- two subpatterns on one line;
- empty clause with a comment between blank lines;
- `#if false` disabled text, and a `#pragma`, before `)`;
- positional inside a list pattern, and inside a property pattern;
- `P(…) or P(…)`;
- an end-of-line comment behind `(`;
- a whitespace-only blank line;
- `case P(…) when …:`;
- `#if`/`#endif` and `#region`/`#endregion` before `)` with no author blank line, where nothing is inserted in front of `#if`, as for `]`;
- a comment, then a directive, then a blank line, then `)`.

A tuple expression `(⏎1,⏎2⏎⏎)` keeps its blank line, so the Unchanged list holds.

**Visible output is decided.** Every expected value the diff adds falls into one of these groups:
- the option-A answer (`00-lead.md`, *Maintainer answers* 1; change doc, *Decisions*): FMT-BLANK-019 positional, the empty clause, the comment, glued-comment, and directive cases, and the RH3202 result;
- unchanged requirements: FMT-BLANK-014 for the argument list and the `var (x, y)` designation, FMT-BLANK-013/SHARED-BLANK-006 for `#endregion`, and FMT-BREAK-022/FMT-INDENT-025 for the columns.

No rule page changed.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| No blank line directly before the token (FMT-BLANK-019) | The token's kind and its direct parent: `CloseParenToken` with parent `ParenthesizedPatternSyntax` or `PositionalPatternClauseSyntax` | `RemovesBlankLinesDirectlyBefore` (the default arm reads this predicate), which feeds FMT-BLANK-018 and FMT-BLANK-020 in `VisitToken` | Same predicate, same span | ok |
| Comment in front of a closing delimiter indented one level deeper (FMT-INDENT-006) | Same predicate plus `CloseBraceToken` | The pre-change private list in `CommentIndentationContributor` (`]` of a collection expression or list pattern; `)` of a positional or parenthesized pattern) | Identical set; the closing brace is kept as its own check. Output unchanged | ok |
| Author's blank line behind the opening token is kept (FMT-BLANK-019, FMT-BLANK-011) | `PrecedingTokenFacts.OpensContent` → `IsContentOpeningToken`, which covers every `(` | The positional `(` | Covered (corpus shape: empty clause with a comment) | ok |
| Blank line below `#endregion` (FMT-BLANK-013) | `BlankLineRegionDirectiveRewriter:90`, closing brace only | FMT-BLANK-019 spec text: not removed below a region directive | Agrees (test `KeepsAndInsertsBlankLineBelowEndRegion…` covers kept and inserted) | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Closing delimiters of the one-element-per-line patterns and collection expressions | `rg "ListPatternSyntax\|PositionalPatternClauseSyntax\|ParenthesizedPatternSyntax\|CollectionExpressionSyntax" src`: `BraceBlankLinePolicy.IsClosingTokenOfPatternOrCollectionExpression` is the one owner; `CommentIndentationContributor.AlignCommentsBeforeToken` reads it. The other hits decide something else on the node: `ListElementInteriorUtilities.IsOwningConstruct` and `CommentPositionUtilities.IsOwningConstruct` (owns the line, already includes `RecursivePatternSyntax`), `BinaryOperatorLineBreakRewriter:91`, `PatternLineStartUtilities:57`, `SwitchCaseBraceRewriter:416`, the indentation contributors, and `DeclaredNameSpacingRule:65` | One owner (FC-015 closed by decision 3) |
| Closing-token checks in the blank-line phase | `rg "CloseBraceToken\|CloseBracketToken\|CloseParenToken" src/Reihitsu.Formatter/Pipeline/BlankLines`: only `BraceBlankLinePolicy`, `BlankLineTokenCleanupRewriter:657` (`#endregion` before a brace), and `BlankLineRegionDirectiveRewriter:90`. The last two are brace-only by SHARED-BLANK-006 | ok |
| Spec statements of the set | FMT-BLANK-019, restated in FMT-BLANK-009, -018, and -020, all updated. `rg "closing parenthesis of a parenthesized pattern" sdd/specs documentation` finds no stale enumeration | ok |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 14 `AssertRuleResult` tests of `PositionalPatternClosingParenthesisBlankLineTests` (spec examples, several blank lines, `case`, nested, `not`, property clause, designation, one subpattern, end-of-line comment, blank behind `(`, own-line comment, glued comment, directive) | Exact output | Any blank line or column differs | `AssertRuleResult`: LF and CRLF, exact text, second pass equals expected, `PhaseStabilityCheck` | ok |
| `RemovesBlankLineAboveMultiLineCommentGluedTo…` | No blank line above `/* z` | Blank line above it | `AssertBlankLinesAbove` (LF/CRLF) + `AssertSecondPassIsStable`. Columns intentionally not pinned (no requirement decides them) | ok |
| `KeepsAndInsertsBlankLineBelowEndRegion…` | Blank line above `);` is kept when authored and inserted when not | No blank line above either `)` | `AssertBlankLinesAbove`; both sides of the boundary (FC-003) | ok |
| `KeepsBlankLineBeforeClosingParenthesisOfParenthesizedVariableDesignation` | Not-named `)` keeps its blank line | Blank line removed | `AssertBlankLinesAbove` | ok |
| `SpecExampleKeepsBlankLineBeforeClosingParenthesisOfArgumentList` | Unchanged | Any change | `AssertRuleResult(input)` (fixed point + stability) | ok |
| RH3202 `VerifyFixRemovesBlankLineBeforeClosingParenthesisOfPositionalPatternClause` (LF, CRLF) | Fix result exact through the node-in-document path | Blank line kept before `)` | `Verify` of `BatchCodeFixTestsBase` (iterations and Fix All per base) | ok |

## Failure classes dismissed
- FC-001, FC-002: the predicate's span is the token and its parent; the spec qualifies by "owning node", which is the explicit `token.Parent` check.
- FC-004, FC-005: the change removes only blank lines through the existing removal path, which keeps comments, directives, and disabled text. I checked each with a test or a corpus shape: comment, glued comment, `#if`, `#if false`, `#pragma`, `#region`.
- FC-006, FC-022, FC-023, FC-024, FC-025, FC-026, FC-029, FC-036: no syntax rewrite, literal, identifier, or member-kind decision is involved.
- FC-007, FC-008, FC-009, FC-018, FC-030: no code-fix code changed. RH3202 reaches the change only through the formatter, and its test runs the batch base.
- FC-010, FC-011, FC-012: covered by the helpers' second pass and phase stability under LF/CRLF, by my corpus second pass, and by the `#endregion` keep/insert test. Insertion and removal do not fight.
- FC-013, FC-014: no analyzer reports a blank line before a pattern's closing delimiter. RH5020 on the glued `/* last */ )` is intended (FMT-BLANK-018 Why) and already holds for `]`.
- FC-016: the skeptic's sibling list from FMT-BREAK-022 is complete with this run.
- FC-019, FC-020, FC-021: tested, plus the corpus shapes above.
- FC-027: `case`, switch arm, nested, `not`, `or`, inside a list or property pattern, followed by a property clause or a designation. Tested or corpus.
- FC-028: the decision reads only the token and its parent, which a detached tree also has. The node-in-document path is tested by RH3202.
- FC-031: both changed summaries and the inline comment describe the new code. The `AlignCommentsBeforeToken` summary still lists the same set, which is still true.
- FC-032: the implementer ran the full formatter and analyzer projects after the production change (05).
- FC-033, FC-034, FC-035: the tests use the change doc's and the spec's own shapes. Expected values are main's output with the stated blank lines removed (04); main does not produce them, and the tests failed before the fix.
- FC-037: not a text-only claim.
- FC-038 to FC-043: no unsupported shape is declared or touched.
- FC-044: no computed result is modelled.
- Tab-width matrix: no column decision changes.

## Architecture findings
_None._ The skeptic's FC-015 finding (two statements of the closing-delimiter set) is resolved in this run by decision 3: one predicate, two readers.

## Hints
- `BraceBlankLinePolicy.RemovesBlankLinesDirectlyBefore`'s summary breaks its line early after "pattern clause. The same set". This is a cosmetic wrap in a doc comment, and the content is correct.
- The indentation phase now reads a predicate that lives in the blank-line phase's `Utilities` namespace (decision 7). There is precedent for cross-phase utility use (`AttributeTargetFormattingRewriter` reads `StructuralTransforms`). No architecture test forbids it, and no class in `Pipeline/Core/Utilities` fits by purpose. I note it only as a design observation, not a defect.
- FMT-BLANK-009's region-directive example sentence ("The same holds in front of the `]` of a collection expression and the `)` of a parenthesized pattern") does not name the positional `)`. It is an illustrative sentence, not a token list, and it remains true (01, *Internal choices*).

## Follow-up candidates
_None._
