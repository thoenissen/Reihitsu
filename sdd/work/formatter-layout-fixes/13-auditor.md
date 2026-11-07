Role: auditor
Commit: eeba52ae3a3316a692531ac5a84ddc2f25ccaa43
Verdict: pass, text cleanup (#1243: pass; #1383: pass, text cleanup; #1149: pass, text cleanup; #1391: pass; #952: pass; #1611: pass, text cleanup)

My prompt named the issues, the slug, the head and base SHAs, and the handovers to read. It carried no intended fix, suspected cause, or hoped-for verdict. Every finding below is confined to spec text or source comments and cannot reach compiled behavior, diagnostics, public API documentation, or rule metadata (reasons per finding). Run-wide facts are in *Whole run* at the end.

## thoenissen/Reihitsu#1243

### Scope
Full audit at `eeba52a` against `origin/main` `6ecc978`. Production: `LineBreakTriviaUtilities.EndLineInFrontOf`, `BinaryOperatorLineBreakRewriter.NormalizeBinaryOperatorPosition`, and `TernaryLineBreakRewriter.MoveQuestionTokenToNextLine`/`NormalizeColonTokenPosition`. Tests: `LoneOperatorJoinBlankLineTests`, plus the two new `LineBreakPhaseBlankLineTests` cases.

### Findings
_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Keep an existing line break in front of a moved operator (`EndLineInFrontOf`) | `previousToken.TrailingTrivia` + `token.LeadingTrivia`, through `SyntaxTriviaUtilities.AreSeparatedByEndOfLine` (EOL trivia, directive, disabled text, `///`) | `ChainLineBreakRewriter.HasLineBreak` (same Core fact); `GenericConstraintLineBreakRewriter` (same); `LineBreakListRewriter` separator sites (`HasTrailingEndOfLine(sep) \|\| IsFirstOnLine(next)`); every `PrependEndOfLine`/`MoveTokenToNewLine` site (`HasLeadingEndOfLine` guard: `DeclarationBrace…:124`, `LineBreakInitializer…:160/186/246/473`, `SwitchCaseWhen…:158`, `GenericConstraint…:291`, `Ternary…:126/218`, `BinaryOperator…:153`) | Same gap and the same notion of "ends the line". None of them counts a line break inside a multi-line comment, so `a /* x⏎ y */ &&⏎b` behaves the same in all of them | holds |
| Join of the operator with the right operand | `WouldJoinAcrossUnjoinableTrivia(operator, right first token)`, unchanged | — | unchanged | holds |

Probed at `eeba52a` (CLI, LF and CRLF, two passes; base CLI built from `6ecc978` for comparison): a directive between the operand and a lone `&&`, a multi-line comment ending the operand's line, the lone `?` and the lone `:` together, and `/* k */ :` or `/* k */ &&` in front of the moved operator. Each one loses the blank line `main` writes, gains no other blank line, stays stable on the second pass, and gives the same output under CRLF as under LF.

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| "Line break in front of a token moved to a line start, keeping one already there" | `LineBreakTriviaUtilities.EndLineInFrontOf` (3 call sites); private copies in `ChainLineBreakRewriter.EnsureLineBreak`/`HasLineBreak`, `LineBreakListRewriter.cs:225`/`:571`, `GenericConstraintLineBreakRewriter.cs:238` | More than one owner. The difference is justified by lead decision 4, and follow-up 9 migrates the copies. Parity: the chain and constraint copies use the same Core fact. The list copy uses `IsFirstOnLine`, which gives the same answer for every gap one of these rewriters reaches. Not a new finding |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `LoneOperatorJoinBlankLineTests` (11 join cells, `AssertRuleResult`) | Exact output, no blank line above the joined operator | Any extra blank line or column change, under LF or CRLF, or on the second pass | `AssertRuleResult` (LF+CRLF, second pass, per-phase stability) | adequate |
| `…BelowCommentLine…AndCommentGetsOneBlankLine` (2) | Whole-pipeline output, with the blank line from FMT-BLANK-011 | Pass also on pre-fix code (07 says so) | `AssertRuleResult` | weak on its own, closed by the two phase-level tests below |
| `LineBreakPhaseBlankLineTests.InsertsNoBlankLineAboveCommentLineWhereItJoins…` (2) | `LineBreakPhase` alone writes no blank line (FMT-BREAK-030) | Fail on pre-fix code with "The phase inserted a blank line" (07) | Phase alone, LF+CRLF | adequate |
| `OperatorAtEndOfOperandLineMovesWithoutBlankLine` / `AuthorBlankLineAboveOperatorStays` / `LoneOperatorFollowedByCommentGainsNoBlankLine` | Other side of the condition (FC-003): no line break in the gap; author blank line kept; join withheld | Blank line added or removed | `AssertRuleResult` / `AssertBlankLinesAbove` | adequate |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None_ beyond decision 4 (carried out; follow-up 9).

### Hints
_None._

## thoenissen/Reihitsu#1383

### Scope
Production: `LineBreakListRewriter.VisitBracketedParameterList` and the helpers made generic over `BaseParameterListSyntax`, `DeclarationBraceLineBreakRewriter` (indexer `[` join), `LineBreakAssignmentRewriter` (compound kinds), and `CommentIndentationContributor` (collection-expression `]`). Tests: `IndexerParameterListLayoutTests`, `CompoundAssignmentLayoutTests`, `CollectionExpressionClosingBracketCommentTests`, and the updated `ParameterContinuationLineAlignmentTests`, `AttributeListStructuralFinalOutputTests`, `CommentBeforeBraceBlankLineTests`, and `MultiLineGluedCommentBlankLineTests`.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | low | `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakListRewriter.cs`, `EnsureSeparatorsHaveEndOfLine` `<param name="isWrappedAfterOpener">` | FC-031 | – | PR-introduced | The doc says "Whether the caller already joined a wrap behind the list's opening delimiter". `VisitBracketedParameterList` computes the flag before `CollapseFirstElementToSameLine`, and that call refuses the join when a comment or directive sits behind `[` (`int this[ // c⏎long a, int b] => 0;`). The flag is then true although nothing was joined. The output is right (the list stays multi-line and is split, as `M( // c⏎long a, int b)` is), so only the description is wrong. Text cleanup: an XML doc comment on a private method. | Word it as `EnsureParametersOnSeparateLines` does: whether the list was wrapped behind its opening delimiter as written |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Indexer list wrapped behind `[` splits (`isWrappedAfterOpener`) | `IsFirstOnLine(Parameters[0].GetFirstToken())`, before the first-element collapse | `HasElementSplitSignal` (same predicate on every element, evaluated after the collapse); `VisitParameterList`, which has no such signal | Same predicate within the indexer path. The method path differs: `void M(⏎int a, int b)` joins to one line, `int this[⏎int a, int b]` gets one parameter per line | holds for indexers. The family divergence is lead decision 14 (follow-up 12): the indexer follows the unchanged FMT-BREAK-012 text, and the method side is a pre-existing defect |
| Indexer `[` joins `this` | `HasLineBreakBetween(this, [)` + `WouldJoinAcrossUnjoinableTrivia` (`CollapseOwnedOpenParenToDeclarationLine`) | Method `(` join, the same method | identical | holds |
| Compound assignment operator and value placement | Guard `IsKind(SimpleAssignmentExpression)` removed. The remaining guards are `HasLeadingEndOfLine(op)`, `HasTrailingEndOfLine(op)`, and `WouldJoinAcrossUnjoinableTrivia` | `ListElementInteriorJoinRewriter.IsOperatorWrap` excludes every assignment operator; RH5111 reports only `=` (P11) | single owner for every assignment kind | holds. Boundary `a⏎+ b` against `a⏎+= b` tested (FC-002) |
| Comment before `]` one level deeper | `CloseBracketToken && Parent is CollectionExpressionSyntax` | `CloseBraceToken` branch (same code); FMT-PIPE-014 family; list-pattern, element-access, and attribute `]` excluded | agree | holds. Probed declarator, `=⏎[` value, argument, nested `[[`, and `/* c */ ];` on the bracket's line (stays, as `/* c */ }` does) |

Probed: `this⏎[`, `this[⏎`, `this[ // c⏎`, a leading comma behind `// c` (kept, as for methods), `#if/#else` inside the list, an interface indexer, and compound assignments with a comment, a directive, a nested value, `z[0]⏎^= 3`, and a lambda body. All are stable under LF and CRLF. `x +=⏎// c⏎1;` keeps `1` at the statement column, as `x =⏎// c⏎1;` does on `main` (thoenissen/Reihitsu#953).

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Parameter-list opener join | `LineBreakListRewriter.CollapseOwnedOpenParenToDeclarationLine` (now `BaseParameterListSyntax`), called from `DeclarationBraceLineBreakRewriter` for methods, operators, destructors, local functions, delegates, and indexers | one owner |
| Split of a list wrapped behind its opener | indexer path only (`VisitBracketedParameterList`); `VisitParameterList`, `VisitArgumentList` lack it | justified by decision 14, follow-up 12; not a new finding |
| Assignment operator line placement | `LineBreakAssignmentRewriter` only | one owner |
| Comment one level deeper before a closing token | `CommentIndentationContributor.AlignCommentsBeforeToken` only | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `IndexerParameterListLayoutTests` (11) | Exact output for each indexer cell, including FC-024/FC-025 shapes | Comma, bracket, or split left as written | `AssertRuleResult` | adequate |
| `CompoundAssignmentLayoutTests` join cells (9) | Exact joined output | Wrap kept | `AssertRuleResult` | adequate |
| `…KeepsItsWrap` (comment, `#pragma`) | Wrap kept, no blank line; the column is deliberately not asserted (thoenissen/Reihitsu#953) | No line starts with `??= 4;`/`+= 1;`, or a blank line appears | `AssertBlankLinesAbove` + second pass | adequate for the invariant it states |
| `CollectionExpressionClosingBracketCommentTests` (8) | Exact output, including the list-pattern boundary (FC-003) | Comment column off by 4 | `AssertRuleResult` | adequate |
| Updated existing tests (07, decision 13) | Expected values re-derived from FMT-BREAK-011/-012/-013, FMT-INDENT-006/-007; blank-line assertions unchanged | — | unchanged helpers | adequate. 07 records that each fails on pre-fix code |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._

### Hints
_None._

## thoenissen/Reihitsu#1149

### Scope
Production: `LineBreakAssignmentRewriter.VisitEnumMemberDeclaration`/`NormalizeEnumMemberValueOperatorPlacement`. Specs: FMT-PIPE-014, FMT-BREAK-015/-016 (decision 5), RH5111-001/-002. Tests: `EnumMemberValueLayoutTests`, the 4 added RH5111 analyzer and code-fix tests, the 1 added RH5111 formatter test, and the updated `LineBreakRequirementTests.EnumMemberWrapBeforeEqualsAndWrapAfterEqualsAreJoined`.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `sdd/specs/formatter/line-breaks.md`, FMT-BREAK-015 (last sentence) and FMT-BREAK-016 ("Inside a parameter list, this requirement does not decide the gaps around a parameter's default `=`: FMT-BREAK-015 owns them") | FC-015, FC-001 | – | PR-introduced (decision 5 text) | The spec names FMT-BREAK-015 the only owner of both gaps around a parameter's default `=`. In the code, the gap **behind** the `=` is joined by `LineBreakAssignmentRewriter.VisitEqualsValueClause` → `NormalizeEqualsValuePlacement`, the FMT-BREAK-016 rewriter, for every `EqualsValueClauseSyntax` parameters included. It runs before `ListElementInteriorJoinRewriter`. Only the gap in front of the `=` is FMT-BREAK-015's code. The two owners differ where FMT-BREAK-015 excludes a gap. With a multi-line literal as the default, `void M(string s =⏎    """⏎    x⏎    """)` (and the indexer form `int this[string s =⏎"""…"""]`), `ListElementInteriorJoinRewriter.IsOwnedGap` (`SpansLines(token)`) keeps the gap, and FMT-BREAK-015's exclusion "multi-line literals" says the same. FMT-BREAK-016 now says it does not decide the gap. Read literally, no requirement joins it, yet the formatter writes `void M(string s = """` (observed at `eeba52a`, the same on `6ecc978`). The output does not change, and it equals the declarator's (`string t = """`, FMT-PIPE-014), so nothing visible is in question. The defect is that the spec's statement of ownership is false for this gap. Text cleanup: a spec wording change only; no code, test, or output changes. | Spec author: FMT-BREAK-016 defers only the gap **in front of** a parameter's default `=` to FMT-BREAK-015. Its value placement (the gap behind the `=`) applies to a parameter's default as to every equals-value clause. FMT-BREAK-015's last sentence names only the gap in front |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Enum member `=` joins the name | `HasLeadingEndOfLine(=)`; `WouldJoinAcrossUnjoinableTrivia(name, =)` (`NormalizeOperatorPlacement`, shared with declarators and properties) | RH5111 `CheckAssignment` (target ends with the name; RH5111-001); RH5111-005 (a comment or directive withholds the fix) | Same gap. A comment or directive keeps the wrap in the formatter and withholds the fix | holds. `apply-fix.sh RH5111`: `A⏎=⏎1,` and `B⏎= 2,` give `fixed` under LF and CRLF |
| Gap behind a parameter's default `=` | `NormalizeEqualsValuePlacement`: `HasTrailingEndOfLine(=)`, `HasLeadingEndOfLine(value)`, not a collection expression, `WouldJoinAcrossUnjoinableTrivia` | `ListElementInteriorJoinRewriter.CanJoin`/`IsOwnedGap` (excludes multi-line literals, owning constructs) | Spans differ at a multi-line literal value | Finding #1 |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| `=` placement onto its target's line | `LineBreakAssignmentRewriter.NormalizeOperatorPlacement` (assignment, declarator, property, enum member); `ListElementInteriorJoinRewriter` for the parameter's `=` | one owner per gap, matching decision 5 |
| Value onto the `=` line | `LineBreakAssignmentRewriter.NormalizeEqualsValuePlacement` (every equals-value clause, parameters included) **and** `ListElementInteriorJoinRewriter` (parameters, where the first did not join) | Two code owners. The spec names one (FMT-BREAK-015). Finding #1 |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `EnumMemberValueLayoutTests` (9) | Exact output; comment cell keeps the wrap | Wrap kept or comment moved | `AssertRuleResult` / `AssertBlankLinesAbove` | adequate |
| RH5111 code-fix tests (3) | Fix converges in one iteration (incremental and Fix All), `A = 1,` with comma kept, LF and CRLF | A second iteration needed, or the comma dropped | `Verify` (code fix) | adequate (FC-007, FC-008) |
| `VerifyFormatterFixesEnumMemberEqualsBelowName` | Formatter output clears RH5111, second pass stable | Diagnostic left | `VerifyFormatter` | adequate (FC-013) |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
The gap behind a parameter's default `=` is decided by two components: `LineBreakAssignmentRewriter.NormalizeEqualsValuePlacement` and `ListElementInteriorJoinRewriter`. The structure is pre-existing on `main`. The run's spec text (decision 5) now states a single owner that is not the one deciding (finding #1). The two disagree only at FMT-BREAK-015's exclusions. Example: the multi-line literal default above, which the first joins and the second would keep. Narrow repair: correct the spec text. A structural repair would route parameter defaults through one rewriter. The lead decides.

### Hints
- Pre-existing, unrelated to the run's change: RH5111 reports `A // c⏎    = 1,` (also a declarator `int x // c⏎= 1;`), and the formatter keeps it because of the comment (`apply-fix.sh RH5111`: `no-fix-offered`). RH5111-005 ("The diagnostic is still reported") and SHARED-DIAG-010 ("MUST NOT report … a gap that the formatter does not join because a comment or a directive sits in it") disagree. The run changes neither the analyzer nor this cell. Follow-up candidate.

## thoenissen/Reihitsu#1391

### Scope
Production: `ArgumentAlignmentContributor.AlignToOpenToken` (empty list) and `BaseTypeListContributor` (`AlignContinuations` for every base list). Tests: `BaseListContinuationAlignmentTests`, `EmptyWrappedListClosingDelimiterTests`, `RawStringOtherLineTerminatorAlignmentTests`, and `ZeroWidthTokenLineStartTests.OmittedTypeArgumentBehindCommentTakesBlockColumn`.

### Findings
_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Closing token of an empty list that starts a line | `items.Count == 0` return removed; `firstLine == lastLine` guard stays | `AlignDictionaryKey` keeps its empty return (an empty `[]` key is a syntax error, which the formatter skips) | agree | holds. Probed `M2(⏎);` with no comment, `M2(⏎/* c */ );`, `M2( // a⏎// b⏎);`, and `new C(⏎);`: `)` after `(`, which FMT-INDENT-012 (unchanged) demands for any multi-line list |
| Line inside a base type | `Types.Count >= 2` now gates only FMT-INDENT-016. `ListElementAligner.AlignContinuations` runs for every base list | `ListElementInteriorUtilities.FindOwningConstruct` skips nested lists (primary-constructor base arguments, type argument lists); `SetUnlessPlaced` | agree | holds. Probed `System.⏎IDisposable` with no comment, `System // c⏎.IDisposable`, `Dictionary<int, // c⏎string>` (owned by the type-argument list, unchanged), record and enum base lists, and a namespace. All stable |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Continuation inside a list element | `ListElementAligner.AlignContinuations` (argument, attribute, angle-bracket, and now base lists) | one owner, reused |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `BaseListContinuationAlignmentTests` (8) | Exact output, Q1 cells and two-type cells, primary-constructor boundary | Continuation at the block column | `AssertRuleResult` | adequate |
| `EmptyWrappedListClosingDelimiterTests` (12) | Exact output; lambda and anonymous method by column plus second pass | `)` at the block column | `AssertRuleResult` / column check | adequate. The two column-only tests are justified in 04 (the rest of the lambda layout is outside the run) |
| `RawStringOtherLineTerminatorAlignmentTests` | Byte-exact, both file endings, second pass | Misaligned literal | byte-exact | adequate (regression) |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._

### Hints
- Pre-existing, unrelated to the run's change: a base list whose `:` starts its own line puts `:` at column 0 (`class F⏎    : System.IDisposable` gives `: System.IDisposable` in column 0, the same on `6ecc978`). No requirement I found decides it. Follow-up candidate.

## thoenissen/Reihitsu#952

### Scope
Tests only: `DirectiveBehindDisabledTextTests` (2), no production change (decisions 7, 11).

### Findings
_None._

### Guard scope
_N/A — no predicate changed._

### Policy ownership
_N/A — no policy changed._

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `EndIfBehindDisabledBranchInMethodBodyMovesToColumnZero` (issue input), `DirectivesBehindDisabledBranchesAtMemberLevelMoveToColumnZero` | Directives at column 0 behind disabled text | Directive indented | `AssertRuleResult` | adequate (FC-034: the issue's exact input) |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._

### Hints
_None._

## thoenissen/Reihitsu#1611

### Scope
Production: `RawStringLineShifts` (measure through Core `GetLeadingWhitespaceLength`, shift through `GetLineShiftEdit`), Core `RawStringLiteralUtilities.GetLineShiftEdit`, and the RH5202 fix refactored onto it. Specs: FMT-RAW-001/-002, the FMT-RAW and FMT-TAB non-goals, and the change doc. Tests: `RawStringClosingWhitespaceMeasureTests` (17), the 5 added RH5202 formatter tests, and the updated `RH5601UseTabsCorrectlyFormatterTests` case.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | low | `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLiteral.cs:11` and `:33` (`ClosingColumn`: "The number of spaces in front of the closing marker"); `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs:376` (`MoveLineInsideTrivia`, `<param name="edit">`: "the number of characters of indentation it replaces, and the number of spaces that replace them") | FC-031 | – | PR-introduced | The diff changed what both describe without touching the comments. `ClosingColumn` is now filled from `GetLeadingWhitespaceLength` and counts every whitespace character, a tab and U+00A0 included. `MoveLineInsideTrivia` now moves its line through `ReplaceIndentation`, that is, through `GetLineShiftEdit`. Its `NewLength` is the column after the move, and the whitespace it does not remove stays; it no longer "replaces" the indentation with spaces. Text cleanup: XML docs of internal members. | `ClosingColumn`: the width of the whitespace in front of the closing marker, each whitespace character one column (FMT-RAW-001). `edit`: as `ReplaceIndentation`'s `edit` param now reads |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Closing-marker width | `RawStringLiteralUtilities.GetLeadingWhitespaceLength(closingLine)` (`char.IsWhiteSpace`) | RH5202 analyzer (2 sites) and fix (4 sites), the same Core method | identical | holds |
| Content-line indentation length | `GetIndentationLength`: whitespace trivia as a whole (hole lines), otherwise `GetLeadingWhitespaceLength(line)` | RH5202 fix: `GetLeadingWhitespaceLength(line)` | identical for literal text | holds |
| Shift edit | `GetLineShiftEdit(oldLength, newLength - oldLength)` | RH5202 fix `GetLineShiftEdit(leadingWhitespaceLength, delta)` | identical; `delta == 0` and empty removals skipped as before | holds |

Tab matrix (`failure-classes.md`, *Tab width across formatter phases*), re-walked for the changed measure. Hole lines keep whitespace trivia, which holds no tab after the preparation phase, so their columns are unchanged. Probed under LF and CRLF, two passes:
- a hole line behind a tab-indented content line;
- a nested raw string whose opening quotes stand behind tabs of literal text, shifted right (`→→→→{"""`) and left (`20sp→{"""`);
- a wrapped argument in a hole anchored to `(` on a tab-indented content line;
- `.ToString()` anchored to `.Trim()` on a tab-indented closing line;
- leftward shifts smaller than the spaces in front of the tabs, and mixed `  →  `.

Every anchor reads the same one-column offset (`(` at 13 sp + 4 tabs + `{M2` gives `2)` at 21), every nested literal closes under its own quotes by the same measure, and `apply-fix.sh RH5202` on the formatted fixtures gives `no-diagnostic` (4/4). Lines inside a `///` comment in a hole: the tab is already replaced (no change). Disabled text in a hole: the file does not parse, so the formatter skips it.

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Raw-string leading-whitespace measure | `RawStringLiteralUtilities.GetLeadingWhitespaceLength`; `RawStringLineShifts.GetLeadingWhitespaceLength` is a private wrapper that delegates to it | one owner (decision 3); `GetLeadingSpaceCount` removed, no caller left |
| Raw-string line shift | `RawStringLiteralUtilities.GetLineShiftEdit`, called from `RawStringLineShifts.ReplaceIndentation` and from the RH5202 fix `ApplyCodeFixAsync` | one owner (decision 15). Hole whitespace-trivia lines are rewritten whole, which decision 16 accepts |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RawStringClosingWhitespaceMeasureTests` (17) | Exact output for every shift (right, none, left fewer/exact/more), character (NBSP, em space, VT, FF, tab, mixed), literal kind (`$`, `u8`), and anchoring | Any column or character difference | `AssertRuleResult` (LF+CRLF, second pass) | adequate. The FC-003 boundary (left by exactly the spaces) is pinned |
| RH5202 formatter tests (5) | Formatter output is not reported by RH5202, and the RH5202 fix's output stays | Diagnostic left, or the fix's output moved | `VerifyFormatter` | adequate (FC-013, SHARED-FIX-007) |
| `RH5601…KeepsTheClosingLineTab…` (updated) | Q2 output (15 sp + tab), RH5601 silent | Old 16-space value | `VerifyFormatter(source, fixed)` | adequate; covered by Q2 |
| `GetLineShiftEdit` has no direct Core test | — | — | — | Accepted by decision 17; both callers' tests exercise right shift, left shift within, and left shift beyond the whitespace |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None_ beyond decisions 3 and 15, which are carried out.

### Hints
_None._

## Whole run

### State
- `git rev-parse HEAD` = `eeba52ae3a3316a692531ac5a84ddc2f25ccaa43` on `claude/serene-cori-ckkue0`; the working tree is clean.
- `git rev-parse origin/main` = `6ecc978240f9d644be07f79b1baa3937eb4173de`. `git merge-base --is-ancestor` confirms it is merged.
- The diff is what 06/07/08/09/11 describe. It changes 12 production files (the 10 of 06 plus Core `RawStringLiteralUtilities` and the RH5202 fix from 08), the tests of 04/07, the cleaner's form-only edits (09), the specs of 01/03/05, and the change doc (03, 11).
- `scripts/lane-check.sh behavior-change`: `LANE CHECK: confirmed`.

### Visible output is decided
Every expected value the diff adds or changes is covered:
- Lone-operator blank line: FMT-BREAK-030. The `??` column: FMT-INDENT-018. The comment blank line: FMT-BLANK-011.
- Indexer list, compound assignment, and `]` comment: FMT-PIPE-014 as it stood before the run (02-skeptic).
- Enum `=`: RH5111-004 and the earlier maintainer answer cited in 02.
- Base list with two types and empty list `)`: FMT-INDENT-013/-012 unchanged. Base list with one type: Q1.
- Tab measure: Q2. Other whitespace characters: RH5202-003 and SHARED-FIX-007 (change doc).
- `#952`: FMT-INDENT-009.

No rule page changed. Outputs the change produces without a test assert the same requirements and need no new question: `M(⏎)` with no comment, `System.⏎IDisposable` with no comment, `System // c⏎.IDisposable`, and `this[ // c⏎long a, int b]` split one per line.

### Comments and scope
- No `TODO` and no tracker reference in the added source lines (`git diff … -- src | grep '^+'`).
- No edit outside the run's components; `DeclarationBraceLineBreakRewriter` is 06's recorded addition.
- Changed summaries describe the new code, except findings #2 and #3.

### Failure classes dismissed (whole catalog)
- FC-001, FC-002, FC-003: traced in each issue's *Guard scope*. The FC-002 boundary of `a⏎+ b` against `a⏎+= b` and the FC-003 two-sided tests exist for #1243 and #1611. The span mismatch is finding #1.
- FC-004, FC-005, FC-019, FC-020, FC-021: every join the run adds goes through `WouldJoinAcrossUnjoinableTrivia`, which covers directives as well as comments. Probed with a directive (`#if`/`#pragma`), an end-of-line comment, and `/* */` mid-line in each gap the run touches: the wrap is kept, nothing is dropped or moved.
- FC-006, FC-022: the raw-string move keeps the literal's value because every line of one literal gets the same edit on an identical whitespace prefix. Probed nested and interpolated literals, which compile and parse back. The run has no semantic rewrite otherwise.
- FC-023, FC-036: no identifier handling or rename changed.
- FC-024, FC-025: an explicit-interface indexer and expression-bodied and accessor indexers are tested. The run changes no expression-body transform.
- FC-026: binary chains with several lone `||` are tested; the token re-resolution uses `ReplaceTokens` on the node it visits.
- FC-027: compound assignments in a for incrementor, a lambda body, an argument, and nested; base lists in a namespace and in a record; the collection `]` in a declarator, an argument, a nested collection, and a field.
- FC-028: every change sits inside a phase. The RH5111 fix (node-level) converges in its tests, and the indexer join reads only tokens inside the declaration.
- FC-029: no paired key/value layout changed.
- FC-007, FC-008, FC-009: RH5111 enum cells converge in one iteration with Fix All (tests and `apply-fix.sh`). The RH5202 fix refactor keeps its edit set identical (`shift == 0` gives no change, as `delta == 0` did).
- FC-018: no naming fix.
- FC-030: no code fix's formatted span changed.
- FC-010, FC-011, FC-012: every probe is stable on a second pass under LF and CRLF. No loop-until-stable was added. FC-012 between the line-break phase and the blank-line phase is pinned by the phase-level tests.
- FC-013, FC-014: RH5202 parity probed (`no-diagnostic`) and RH5111 parity tested. RH5106/RH5107/RH5111 do not cover indexers or compound assignments (P11). RH5043/RH5035 are comment-placement rules (content, SHARED-DIAG-010 second paragraph). No new rule.
- FC-015, FC-016: see the *Policy ownership* tables. Sibling line-break insertion sites are enumerated under #1243.
- FC-017: see the *Assertion adequacy* tables. No helper is weaker than its stated invariant.
- FC-031: findings #2 and #3.
- FC-032: the formatter changes reach the code fixes. 08 ran the analyzer project after the last production change (7350/7350), and 09 ran the RH5111/RH5202/RH5601 filter after its own form-only edit.
- FC-033, FC-034: the issue inputs are used verbatim (#1243, #1149, #1391, #952, #1611), under LF and CRLF.
- FC-035: expected values come from spec examples and requirements (04, 07). Spot-checked against the base CLI output, where every red test differs from `main`.
- FC-037: no text-only claim was made.

### Commands
```shell
git rev-parse HEAD; git rev-parse origin/main; git merge-base --is-ancestor 6ecc978240f9d644be07f79b1baa3937eb4173de HEAD
scripts/lane-check.sh behavior-change
dotnet build src/Reihitsu.Cli -c Debug -o <scratch>/headcli          # head; base the same from a temporary worktree at 6ecc978 (removed afterwards)
dotnet <scratch>/{head,base}cli/Reihitsu.Cli.dll <fixture>            # LF and CRLF, head twice
scripts/apply-fix.sh RH5202 <scratch>/rh5202                          # 0 fixed, 4 no-diagnostic
scripts/apply-fix.sh RH5111 <scratch>/rh5111                          # enum joins fixed; comment cells no-fix-offered (hint under #1149)
```
No test was re-run: no finding turns on a test result.
