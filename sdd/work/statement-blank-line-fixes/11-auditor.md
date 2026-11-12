Role: auditor
Commit: cf077d09a951958263b6257a37a9881d2d476fb2
Verdict: findings (#946: pass; #972: pass; #1096: findings; #1076: findings; #1626: pass; #1625: pass)

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Finding numbers start at #1 (first audit of the run).

Evidence: static tracing of the production diff (24 files) and of the counterparts it reaches (`SyntaxIndentationUtilities`, `TokenGapAnalysis`, `SyntaxTriviaUtilities.AreSeparatedByEndOfLine`, `BlankLineSpacingPolicy.RequiresBlankLineAfterClosingBrace`, `FormattingTextAnalysisUtilities`, the RH5022–RH5032 analyzers, RH5011/RH5030/RH5103 fixes). Fixtures in the scratchpad, run with `scripts/apply-fix.sh <ID> <dir> --no-install` (LF and CRLF arms) and `scripts/format.sh` on copies; fixtures deleted afterwards. No test suite re-run: no finding turns on a test result, and no compiled file changed after 0420faf (`git diff --stat 0420faf HEAD -- src` is empty), so 07-implementer's full Debug runs and 09-cleaner's Release build stand.

## thoenissen/Reihitsu#946

### Findings
_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Token whose gap the fix fills (`StatementShouldBePrecededByABlankLineCodeFixProviderBase.ApplyCodeFixAsync`) | `FirstAncestorOrSelf<StatementSyntax>()` of the diagnostic token, then `GetFirstToken()` | Each analyzer's `GetPreviousToken` override (gap in front of the statement's first token; RH5005/RH5006 from `AwaitKeyword` when present) | Same token for all 20 rules: every diagnostic token is a keyword or first token whose parent is the reported statement itself; labeled statements are excluded by SHARED-STMT-002 | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Token in front of which the statement blank line goes | `rg -n "GetFirstToken\(\)|GetPreviousToken" src/Reihitsu.Analyzer/Rules/Layout/RH50*.cs src/Reihitsu.Analyzer.CodeFixes/Base/StatementShouldBePrecededByABlankLineCodeFixProviderBase.cs`: analyzer overrides measure, fix derives from the statement | one owner per side, derived from the same statement; ok |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5005/RH5006 `VerifyFixInsertsBlankLineAboveAwaitOf…`, `…SharingLine…`, `…CommentLeading…`, `…WithCarriageReturnLineFeed` | One blank line above `await`, diagnostic on `using`/`foreach` | A line break inserted between `await` and the keyword, or a second iteration | `Verify(source, fixed)` exact text, 1 incremental + 1 Fix All iteration, LF and CRLF | adequate |
| `VerifyFixAllInsertsBlankLinesAboveAwaitOfAdjacent…` | Fix All corrects two adjacent statements in one iteration (FC-008) | Overlapping or stale edits | Fix All 1 iteration | adequate |
| RH5005/RH5006 formatter parity tests | Formatter output equals the fix output, diagnostic located on the keyword | Formatter puts the blank line elsewhere | `VerifyFormatter(source, fixed, diagnostic)` | adequate |

## thoenissen/Reihitsu#972

### Findings
_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Insert one line break vs. rebuild the gap (`BlankLineCodeFixUtilities.InsertBlankLineBefore`) | `previousToken.TrailingTrivia` + `token.LeadingTrivia`, any trivia that ends its line (`AreSeparatedByEndOfLine`) | RH5030 `CanRegisterCodeFix` (same predicate, withholds when false); RH3208 caller `RequiresBlankLineAfterClosingBrace` (`lastLine < nextLine`, counts line breaks inside comments) | RH5030: withholds exactly where the owner would rebuild, so it reaches only the insert path. RH3208: calls the owner when any line break stands in the gap; a gap whose only line breaks lie in comments takes the rebuild path (SHARED-AFTERBRACE-002), a gap with no line break never calls it. Boundaries traced and covered by tests on both sides (`VerifyStatementBehindSingleLineCommentStaysOnClosingBraceLine`, `…HoldingEveryLineBreakMovesBelowComment`) | holds |

Traced members of the #972 matrix (02-skeptic): `⏎/* block */`, comment line above plus block comment, two block comments, multi-line comment starting its own line, trailing `// x` then `/* b */`, `⏎/** d */` — all take the insert path (blank line above the comment's first line); ` /* c */ ` and ` /* a⏎b */ ` take the rebuild path. Fixture `a++; /* x */ /* y⏎z */ var b = a;⏎a++;⏎/* p */⏎/* q */ var c = a;` (RH5029): rebuild for `var b`, insert above `/* p */` for `var c`; converges, LF and CRLF.

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| "Is the statement on a later line than the token before it" for the blank-line fixes | `rg -n "AreSeparatedByEndOfLine" src --glob '!*Test*'`: `BlankLineCodeFixUtilities.InsertBlankLineBefore`, `RH5030…CodeFixProvider.CanRegisterCodeFix` (formatter uses are its own phases) | one predicate; ok |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5002/RH5008/RH5021/RH5029 `VerifyFixInsertsOneBlankLineAbove…` (block comment, two block comments, comment line above, multi-line comment, line comment ending previous line, delimited doc comment) + CRLF variants | Exactly one blank line, above the comment's first line, in one application | Two blank lines; blank line between comment and statement | exact text, 1/1 iterations | adequate |
| `VerifyFixMovesStatementBehindCommentStartingBehindPreviousStatement` (+CRLF), `VerifyFixAllMovesStatementsBehindCommentsHoldingEveryLineBreakInOneIteration` | Rebuild path: statement below the comment, no trailing whitespace | Trailing space, column 0, second iteration | exact text | adequate |

## thoenissen/Reihitsu#1096

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs:186-192` (`GetSwitchSectionIndentation`, `lineStartingStatement == null` branch) | FC-027, FC-016, FC-003 | – | PR-introduced | When the anchor line is a `case` label's continuation line and no other statement of the section starts its own line, the fix returns the continuation line's own whitespace. SHARED-STMT-004 says the moved statement takes the column of the section's statements, "not a column read from the label's continuation line". Counterexample (RH5029, inside an object initializer; the continuation at the label's column, which is the layout the formatter gives a wrapped `when` condition): `case 1 when⏎                    Check(value): Method(1); var y = 1; return;` → `var y = 1; return;` lands at column 20, the column of `case`, one level shallower than the section's statements; `main` put it at column 24. Enumeration of the class (axes: continuation column = deeper than the label / at the label's column; a statement of the section that starts its own line = present / absent): deeper + present (all 10 tests) holds; at the label's column + present (fixture, `break;` below) holds, column of `break;`; deeper + absent gives the continuation column, right only when the continuation is exactly one level deeper and a column read from the continuation line in every case; at the label's column + absent fails as above. No test covers the absent branch. | Refuse this class: withhold the fix when the anchor line is a label continuation and no other statement of the section starts its own line (a rare class: an RH5103 violation inside an initializer), record the gap in SHARED-STMT-004, and add tests for both absent cells. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Anchor line is a label continuation (`IsWithinLabelContinuation`) | `(label.SpanStart, label.Span.End)` over every label of the enclosing switch statement | `SyntaxIndentationUtilities.IsWithinSwitchSectionLabelRegion`: `[label.FullSpan.Start, label.Span.End)` over the same labels | The continuation span is the tail of the region span; a line led by the label's first token or by trivia in front of it (`contentStart <= SpanStart`) still goes to `ComputeAnchorDerivedIndentation` and gets the added level, a line inside the label behind its first token does not. Boundary `contentStart == SpanStart` is on the region side in both. | holds |
| Statement whose column is read (`GetSwitchSectionIndentation`) | First statement of the section other than the target whose first token starts its line | SHARED-STMT-004 (answer 2A): column of the section's statements as written | Agrees when such a statement exists; the fallback without one does not (#1) | finding #1 |
| Blank line counted by the analyzers (`TokenGapAnalysis.Between(...).BlankLineCount` in both analyzer bases) | `previousToken.TrailingTrivia` + `firstToken.LeadingTrivia` (and `lastToken`/`nextToken` for RH5011) — the same trivia `ContainsBlankLine` read | Formatter `BlankLineEditor` (same `TokenGapAnalysis`); directive exemptions `IsPrecededByDirective`/`IsFollowedByDirective` on the same concatenated trivia | Same span as before; `TokenGapAnalysis` counts every line `ContainsBlankLine` counted, plus a whitespace-only line below a trivia that carries its own line break. Analyzer and formatter now share one measure. A whitespace-only line inside disabled text is not counted by either (see #3) | holds (wording: #3) |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| SHARED-BLANK-001 measure | `rg -n "ContainsBlankLine" src` finds nothing; `rg -n "TokenGapAnalysis.Between" src/Reihitsu.Analyzer src/Reihitsu.Analyzer.CodeFixes`: the two analyzer bases; formatter `BlankLineEditor` | one measure; ok (decision #8 done, `ContainsBlankLine` deleted) |
| Column of a statement moved inside an initializer after a statement on a label line | `BlankLineCodeFixUtilities.GetIndentation`/`GetSwitchSectionIndentation`; `RH5103…CodeFixProvider.GetIndentation` → `SyntaxIndentationUtilities.ComputeAnchorDerivedIndentation` | Two owners that now differ for the continuation line; justified by decision #9 (narrow fix, RH5103 stays, follow-up recorded in `00-lead.md`). Not a new finding |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5029/RH5002 `VerifyMovedStatementTakesSectionColumn…` (guard on 2 and 3 lines, `when` starting the continuation, two labels, anonymous object, `if` variant, CRLF, spec example), `…KeepsSectionColumnWhenPrecedingStatementStartsItsOwnLine…`, `…TakesNestingColumn…InMethodBody` | Moved statement in the column of `break;` | Column read from the continuation line | exact text, 1/1 iterations | adequate for the cells they hold; every one has a line-starting `break;` and a continuation deeper than the label, so the absent-statement cells are untested (part of #1) |
| `VerifyNoDiagnosticWhenLineBelowDocumentationCommentHoldsSpaces` (+CRLF, +`DocumentationModeNone`) for RH5002 and RH5011; `…EmptyLineFollowsEndIfDirective`, `…PragmaDirective…`, `…LineBelowEndIfDirectiveHoldsSpaces` (+CRLF) | The line below a `///` comment or directive counts as blank | A diagnostic | `Verify(source)` (no diagnostics), both documentation modes (FC-040) | adequate |
| `VerifyDiagnosticAndFixWhenDocumentationCommentDirectlyFollowsBreak`, `VerifyDiagnosticWhenDocumentationCommentDirectlyPrecedesIfStatement` | `///` without a blank line is still reported (answer 1A) | No diagnostic | exact fix text | adequate |
| `RH5002…FormatterTests.VerifyFormatterKeepsBlankLineBelowEndIfDirectiveAndAnalyzerReportsNothing` | Analyzer/formatter parity on the directive row | Analyzer reports formatter output | `VerifyFormatter(source)` | adequate |

## thoenissen/Reihitsu#1076

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | low | `sdd/specs/shared/whitespace-removal-fixes.md:20-24` (SHARED-WS-001); `src/Reihitsu.Analyzer.CodeFixes/Base/WhitespaceSpanRemovalCodeFixProviderBase.cs:15-18`; `src/Reihitsu.Analyzer.CodeFixes/Base/BlankLineSpanRemovalCodeFixProviderBase.cs:7-10` | FC-031, FC-015 | – | PR-introduced | The run made the RH5022/RH5024–RH5027 fixes delete the whole run of blank lines, wider than the reported span. Three statements still say the deleted span is the reported span: SHARED-WS-001 ("MUST delete the whitespace of the reported span … no whitespace outside the span is changed"), which RH5022-004 and RH5024–RH5027 now cite "for the span of that run"; the `WhitespaceSpanRemovalCodeFixProviderBase` summary ("Because the inspected span is exactly the deleted span, the guard can never withhold…"); and the `BlankLineSpanRemovalCodeFixProviderBase` summary ("whose fix removes the reported diagnostic span", "a named specialization"). Behavior is safe — `GetRemovalSpan` widens only by whole lines `IsBlankLine` accepts, so the removed span stays whitespace-only, and lines adjacent to a reported blank line cannot lie inside a literal, comment, or disabled text — but the shared requirement and the guard's stated invariant no longer match the code. Text only: no compiled behavior, diagnostic, or rule metadata changes, and no assembly here is a public API (`AGENTS.md`, *Supported inputs*). | Spec author: let SHARED-WS-001 admit a rule that names a wider whitespace-only span (the run) as its deleted span. Implementer: rewrite the two class summaries to say the guard inspects the reported span and `GetRemovalSpan` may widen it only by whitespace-only lines. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Removed span (`BlankLineSpanRemovalCodeFixProviderBase.GetRemovalSpan`, `removesRun`) | Reported line plus every adjacent line above it that `IsBlankLine` accepts, and every adjacent line below it that is blank and ends with a line break | Whitespace-only guard in `WhitespaceSpanRemovalCodeFixProviderBase.RegisterCodeFixesAsync` (inspects the reported span only); the analyzers' reported line (RH5022 below `{`, RH5024–RH5027 directly above the target) | The guard's span is smaller than the deleted span (FC-001 shape), but the widening adds whitespace-only lines by construction, so the deletion stays whitespace-only. Upward widening never applies to RH5022 (line above is `{`), downward never to RH5024–RH5027 (line below is the target). One run per reported token, so Fix All edits never overlap (tested for RH5022 and RH5024). Stated invariant stale: #2 | holds (text: #2) |
| Region exemption for deleting rules (`RegionDirectiveBlankLineUtilities.FollowsRegionDirectiveRequiringBlankLine`) | Nearest non-blank line above the target (`FindPreviousNonBlankLineIndex`): an active `#region`/`#endregion` at its content start; the target line's first token `}` at its content start | RH5032 `IsMissingRequiredBlankLineAfter` (line directly below the directive; `GetLineIndicesBeginningWithToken` for `}`); both apply `RequiresBlankLineBelow` | Same fact for `}` (only whitespace in front of the token). For a run below a directive, RH5032 sees a blank line below and is satisfied; the deleting rules see the directive as the nearest non-blank line and stay silent; RH5023 reports the extra lines. A comment between directive and blank line makes both sides report and fix without a cycle. A `#region` in disabled text or a `#region`-like comment is not a directive (`GetStructure`). | holds |
| End of file is not a line (`EnumerateConsecutiveBlankLines`) | `line.EndIncludingLineBreak > line.End` on the candidate line | RH5023-001 example, `GetRemovalSpan` downward condition (same test) | same condition | holds |
| `ContinuesEnclosingStatement` (RH5011) | Next token kind `else`, or `while` whose parent is `DoStatementSyntax` | RH5026 (`else`/`catch`/`finally` lines), RH5027 (do footer) | Every `else`/do-`while` that can follow `break;` belongs to a statement holding the `break` (also `do if (a) x++; else break; while (b);`); a `while` statement stays reported (tested) | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| SHARED-BLANK-006 `#endregion`-before-`}` exemption | `rg -n "RequiresBlankLineBelow|EndRegionDirectiveTrivia|closeBraceStartLineIndices" src --glob '!*Test*/**'`: `RegionDirectiveBlankLineUtilities.RequiresBlankLineBelow` (RH5032 and RH5024–RH5027 through it); formatter `BlankLineRegionDirectiveRewriter.EnsureBlankLineAfterRegionDirectives` | Analyzer side has one statement (decision #20). The formatter's statement predates the run and is a recorded follow-up candidate. ok |
| Run widening | `rg -n "removesRun" src`: the base plus seven providers | one owner; ok |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyFixDeletesRunOf…InOneApplication` (RH5022, RH5024–RH5027; +CRLF; whitespace-only lines for RH5022) | One application removes the whole run | "Expected 1 iteration but found 2" | exact text, 1/1 iterations | adequate |
| `VerifyFixAllDeletesRunsOfBlankLines…TwoClosingBraces/TwoOpeningBraces…` | Fix All over two runs, one iteration | Overlapping edits | Fix All 1 iteration | adequate |
| RH5023 `VerifyEndOfFile…IsNotReported` (+CRLF, comment, directive), `VerifyOnlySecondBlankLineAtEndOfFileIsReportedAndFixedInOneApplication` (+CRLF), `VerifyLastLineHoldingOnlySpacesWithoutLineBreakIsNotReported` | End of file is not a line | Zero-length or last-line diagnostic | `Verify` | adequate |
| RH5011 `VerifyBreakBefore{Else,DoStatementFooter}…IsNotReported` (+CRLF), `VerifyBreakBeforeWhileStatementIsReported`, spec example | RH5011-008 both sides of the boundary (FC-003) | Diagnostic on `else`/footer; no diagnostic before a `while` statement | `Verify` | adequate |
| RH5024–RH5027 region tests (`#region`, `#endregion`, runs, CRLF, `catch`, type level, initializer element), `…BelowCommentAbove…IsReported`, `…EndRegionDirectiveAboveFooterStartingWithClosingBraceIsReported`; formatter parity tests | Exemption holds below a region directive, not below a comment or `#endregion`-before-`}` | A diagnostic below `#region`; none below a comment | `Verify`, `VerifyFormatter(source)` | adequate |

## thoenissen/Reihitsu#1626

### Findings
_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Placement in the separated case (`behindLeadingDirectives`) | Leading trivia of the statement's first token: index 0 (base) or behind the last leading directive (RH3208, RH5030) | SHARED-STMT-004 bullet 1, SHARED-AFTERBRACE-001 | Each caller keeps its `main` placement (decision #18; unifying is a recorded follow-up with an output question) | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Blank line a code fix inserts in front of a statement | `rg -n "InsertBlankLineBefore|ReplaceFormattingOnlyGap|CreateBlankLineLeadingTrivia|InsertLeadingLineBreak" src --glob '!*Test*'`: `BlankLineCodeFixUtilities` and its three callers; RH5011's fix inserts below `break` with its own code | One owner for the base, RH3208, RH5030; RH5011 is the third owner the lead left as a follow-up (decision #11). ok |
| Trailing-whitespace trim | `BlankLineCodeFixUtilities.TrimTrailingWhitespace`, `DeclarationModifierUtilities.TrimTrailingWhitespace` (pre-existing, unrelated caller) | moved, not copied; ok |

### Assertion adequacy
The existing RH5001–RH5030 and RH3208 fix tests still pass (07-implementer); the one changed expected value is #1096's (answer 2A).

## thoenissen/Reihitsu#1625

### Findings
_None._

### Guard scope
See #972 (RH3208 caller against the owner's predicate). Fixtures: RH3208 inside a switch section and inside an object initializer's lambda, `int L() => 1; /* a⏎b */ L();` → `} /* a⏎b */⏎⏎L();` with `L();` in the block's column and no trailing whitespace, one application, LF and CRLF.

### Policy ownership
See #1626.

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH3208 `VerifyStatementBehindCommentHoldingEveryLineBreakMovesBelowComment` (+CRLF, +switch section), `VerifyStatementBelowCommentEndingItsLineGetsOneBlankLine`, `VerifyStatementBehindSingleLineCommentStaysOnClosingBraceLine`, `VerifyStatementOnLocalFunctionLineStaysOnClosingBraceLine` | Answer 3A output; no change where no line break stands in the gap | Trailing space, column 0, a moved statement on a no-line-break gap | exact text | adequate |

## Whole run

### Scope
Full audit. Head cf077d09a951958263b6257a37a9881d2d476fb2 (checked out, clean). Base `origin/main` c884fcc2548db02e608e82fc8e5f386029e34439, equal to `git ls-remote origin refs/heads/main`, and an ancestor of head. The production diff (24 files) matches the change tables of 05-implementer and 07-implementer. `scripts/lane-check.sh bug-fix`: confirmed.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | low | `sdd/specs/shared/blank-line-spacing.md:22` (SHARED-BLANK-001, sentence added by this run) | FC-020 | – | PR-introduced | The new sentence says the line directly below a preprocessor directive MUST count as blank when it holds nothing but whitespace. Below an `#if`/`#elif`/`#else` that opens an inactive branch, that line is disabled text, and neither the analyzers nor the formatter count it (`TokenGapAnalysis` marks a disabled-text trivia's line as content). Fixture (RH5002): `a++;⏎#if NEVER_DEFINED⏎⏎#endif⏎// note⏎if (a > 1) { }` is reported, and the formatter inserts a blank line below `#endif`. Analyzer and formatter agree, and the analyzer's answer is `main`'s; only the sentence overclaims. Rare (an empty first line of an inactive branch, then a comment, then the statement). Text only. | Spec author: say that a line inside disabled text does not count, like a line inside a comment's text. |

Visible output (check 10): the one changed expected value of an existing test (`RH5029…VerifyMovedStatementTakesSectionColumnWhenLabelSpansMultipleLinesAndPrecedingStatementSharesItsContinuationLine`, `var y` from the over-indented column to the column of `break;`) is answer 2A. Every new test's expected output follows an unchanged or clarified requirement, or answers 1A–3A. The 20 deleted rule-page paragraphs described how the base fix reads the column "the same way RH5103's code fix does", which is no longer true for a continuation line after this change. They also carried an implementation detail and a corner case that the rule-page convention in `AGENTS.md` excludes, and the maintainer's #1096 decision asks to remove the documented limitation (decision #4). Covered; no `undecided output`.

### Failure classes dismissed
- FC-002 — the qualifiers are explicit conditions: "active" (`IsActive`), "nearest non-blank line" (`FindPreviousNonBlankLineIndex`), "of a `do` statement" (`Parent is DoStatementSyntax`), "ends with a line break" (`EndIncludingLineBreak > End`), "behind the label's first token" (`> SpanStart`).
- FC-004, FC-019, FC-021 — the removal deletes whole whitespace-only lines; the rebuild path removes only whitespace trivia at the end of the previous token and keeps every comment in place; comment and directive rows of the #972 matrix are tested.
- FC-005 — the region guard covers region directives only; another directive above the run stays reported, as RH5024-001 states.
- FC-006, FC-018, FC-023, FC-024, FC-025, FC-026, FC-029, FC-036 — no semantic rewrite, rename, or token re-resolution; the changes insert or delete whitespace and line breaks only (`ReplaceTokens` on the two exact tokens).
- FC-007, FC-008, FC-009 — convergence and Fix All asserted with 1 iteration; probes converged in one application; RH5023's end-of-file no-op is no longer reported; no other fix is offered where it changes nothing.
- FC-010, FC-011, FC-012, FC-028, FC-038–FC-043 — no formatter or precheck code changed, no unsupported shape declared or touched; the RH3208 path to `FormatNodeInDocumentAsync` is unchanged.
- FC-013, FC-014 — formatter parity tests for every changed reporting decision; no new rule. The placement difference around a directive (fix above `#if`, formatter below `#endif`) predates the run and is decision #18's follow-up.
- FC-022 — a blank line adjacent to a reported blank line cannot lie inside a literal, a comment, or disabled text, because its closing delimiter or `#endif` would have to stand on a line between them; RH5025 excludes interpolation braces (RH5025-006), and lines behind a hole's `{` or in front of its `}` are hole trivia.
- FC-030 — RH3208 still formats only the local function; the added edit touches only the gap below its new brace.
- FC-032 — no compiled change after 0420faf.
- FC-033, FC-034, FC-035 — CRLF and `DocumentationMode.None` arms exist; the #1096 reproductions use the initializer shape; expected values come from spec examples and answers (04-test-implementer), the one changed value from answer 2A.
- FC-037 — no text-only claim in this run.

### Architecture findings
#1 belongs to the structure 02-skeptic reported as architecture finding 2 and the lead decided under decision #9: the statement blank-line fix computes the column of a moved statement, which the indentation phase owns, and `RH5103`'s fix shares the older computation. Decision #9 chose a narrow fix and a follow-up. #1 is the first audit finding of this class, so it is no repeat; the narrow fix's fallback is the member that fails. The input class is rare (an RH5103 violation inside an initializer), so *Refuse the class first* applies to it.

### Hints
- `BlankLineCodeFixUtilities`' class summary calls it "the one owner of the blank line a code fix puts in front of a statement". That holds for the spec IDs it names; RH5011's fix still inserts its own (decision #11 follow-up).
- Pre-existing, unchanged by the run: inside an initializer, a statement moved inside a block whose preceding sibling starts on a `case` label's line (`case 1: { a++; var y = 1; }`) takes the label's column, because the block branch reads the anchor line's whitespace unchanged, as `ComputeAnchorDerivedIndentation` did for a block parent. It is rare; it fits the decision #9 follow-up.
