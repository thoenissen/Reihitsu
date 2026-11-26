Role: auditor
Commit: 689a88dfa36de0a9558977d2b72357eb660e8c8a
Verdict: pass

## Scope

Repair delta against 29481888d82e0a752340dae52b5d2f8ce5bb9d32, the commit audited in `10-auditor.md`. Head 689a88dfa36de0a9558977d2b72357eb660e8c8a is the checkout. Base `origin/main` is eed7195c0b6fb583ab4271173b7a4ad022851c5f, fetched, and an ancestor of head (merged). `scripts/lane-check.sh behavior-change` confirms the lane.

The repair diff is what handovers 11–14 describe: `getInnermostNodeForTie: true` in the fix's registration lookup, the analyzer's one-line guard removed, three summaries changed (analyzer class, `CheckRawStringAlignment`, `RawStringLiteralUtilities.HasTabInLeadingWhitespace`), the new `RH5202RawStringLiteralsShouldBeFormattedCorrectlyLiteralPositionTests`, and one test doc comment in the TabIndentation tests. No spec, change doc, rule page, or ID ledger changed. Scope did not grow beyond the RH5202 fix provider, analyzer, and Core helper summary, so this is a delta audit.

Inputs: thoenissen/Reihitsu#1925, `00-lead.md` (decisions 6 and 7), `10-auditor.md`, handovers 11–14, and RH5202-003/-008/-009. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Evidence. All fixtures live outside the repository, under `/tmp/claude-0/-home-user-Reihitsu/8454a2c2-597e-59f6-a041-7ba5648efd58/scratchpad/aud15`, and each ran under LF and CRLF.

- `scripts/apply-fix.sh RH5202 …/pos`: 12 fixtures, 24 × `fixed`. They are audit 10's position fixtures and its counterexample (named argument, collection element, tuple element, constant pattern, element access, object-creation argument, `return`, positional argument), plus four new ones:
  - an interpolated literal as an attribute argument, aligned behind tabs;
  - a literal as a switch-expression arm's constant pattern, with tab and misalignment;
  - a literal in a property subpattern behind tabs;
  - a misaligned object-creation argument.
- `…/nest`: a tab-aligned literal passed as an argument inside a hole of a space-aligned outer literal is now `fixed` (one iteration). Its fix writes the RH5202-003 shift and RH5202-009 spaces. When the outer literal is tab-aligned too, the result is `no-fix-offered`. That is expected: the runner tries only the first (outer) diagnostic, which RH5202-008 withholds while the nested literal holds tabs. The nested literal now has its own fix, so the withheld state is no longer permanent.
- Roslyn 5.12.0 probe of code with errors, for the removed guard: an unterminated `$"""`, `"""`, `$$"""` at end of file, with and without a hole or a trailing newline, and `$"""` with whitespace before the newline.
  - Every literal of a multi-line raw kind has its span end on a later line than its start. The `InterpolatedMultiLineRawStringStartToken` includes the line break.
  - `$"""` with no line break behind it lexes as `InterpolatedSingleLineRawStringStartToken`, which the analyzer's kind filter skips.
  - So no input, compiling or not, reached the removed guard.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | `RegisterCodeFixesCoreAsync` now calls `FindNode(span, getInnermostNodeForTie: true)`. A `LiteralExpressionSyntax` has no child node. An `InterpolatedStringExpressionSyntax` has none with its full span. So the innermost node with the diagnostic's span is always the reported literal, whatever wraps it. Every member audit 10 listed now gets a fix: positional, object-creation, and bracketed argument; tuple and collection element; constant pattern; attribute argument; plain and interpolated. So do the members added here (interpolated attribute argument, switch-arm pattern, property subpattern). Tests cover each wrapper kind at least once on each side. |
| 2 | closed | The guard is gone. `GetClosingLineShift` (Core) and `RawStringLineShifts` (formatter, pre-existing) still own the one-line rule. No input reached the guard (probe above). The TabIndentation test's stated purpose is corrected. |
| 3 | closed | The class summary names the closing-column and the tab condition (RH5202-009). The method summary states why no one-line check is needed, and the probe confirms that holds also in code with errors. The `HasTabInLeadingWhitespace` summary names the analyzer's report decision. |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_N/A — no new finding._

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Fix registration: which node the diagnostic names | `root.FindNode(span, getInnermostNodeForTie: true)`, the innermost node with exactly the diagnostic span | Analyzer location `literal.GetLocation()` on the `LiteralExpressionSyntax` / `InterpolatedStringExpressionSyntax` | Same node in every position. No descendant of either kind has the same span, and no wrapper is returned on a tie any more | ok |
| Literal on one line is not judged (analyzer copy removed) | – | Caller kind filters (`MultiLineRawStringLiteralToken`, `Utf8MultiLineRawStringLiteralToken`, `InterpolatedMultiLineRawStringStartToken`); `GetClosingLineShift`'s own check | Kind filter implies a later closing line, also in code with errors (probe) | ok |
| Report a literal aligned behind tabs | Unchanged since audit 10 (`closingLine` is still `GetLineFromPosition(literal.Span.End)`) | Unchanged | Byte-identical evidence to audit 10 | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Literal on one line is not judged | `rg -n "LineNumber == closingLine.LineNumber" src` → `RawStringLiteralUtilities.GetClosingLineShift`, `RawStringLineShifts` (pre-existing) | analyzer copy gone; #2 closed |
| Diagnosed-node lookup in the RH5202 fix | `rg -n "FindNode" src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5202…CodeFixProvider.cs` → one site | one owner |
| Closing whitespace holds a tab | `HasTabInLeadingWhitespace` (Core) only; consumers unchanged since audit 10 | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Position, 6 × `VerifyMisaligned…IsFixed` (positional argument, interpolated positional argument, attribute argument, tuple element, collection element, constant pattern; RH5202-003) | Reported at the literal's span; fix offered; exact shifted text; re-analysis clean | No fix offered (red at 11), or any differing character | `VerifyCodeFixWithEndOfLine` under LF and CRLF: `Verify(source, fixed)` plus `ApplyCodeFixAsync` exact text | adequate; I traced every fixture column against the opening quotes' column |
| Position, 7 × `VerifyLiteralAlignedBehindTabs…IsFixed` (positional, interpolated positional, bracketed, attribute, tuple, collection, pattern; RH5202-009) | Same, with every tab written as one space | Same; a tab left in the result | same | adequate |
| Position `VerifyLiteralAlignedBehindTabsPassedAsNamedArgumentIsFixed` | The other side: the argument is wider than the literal | Fix missing | same | adequate (green before and after, as a boundary should be) |
| Position `FixAllCorrectsEveryDiagnostic` | Two literals in argument positions of one call, one tab-aligned and one misaligned, both fixed in one pass | Either literal unfixed | `GetFixAllScenario` on the batch base | adequate |
| TabIndentation `VerifySingleLineRawStringLiteralsOnLinesIndentedWithTabsAreNotReported` (doc comment only) | Single-line literals are not reported by the rule | A diagnostic | `Verify(source)` LF + CRLF | adequate; its stated purpose now matches what it pins |

Visible output (check 10): the new expected values are fixes offered and shifted for literals in argument, element, and pattern positions. RH5202-003 and RH5202-009 demand them unchanged: the fix MUST write the shift and the spaces for every reported literal. The tab-side values are also covered by answer A of #1925. Nothing undecided.

## Failure classes dismissed

Audit 10's dismissals are reused. The analyzer's report condition, the fix's rewrite, the formatter, the specs, and the rule page are byte-identical since 2948188 in everything those dismissals rest on. The delta adds only the following.

- FC-027, FC-016: closed by the innermost-for-tie lookup (#1).
- FC-001, FC-002: the deleted guard inspected nothing a caller reaches. The remaining kind filter and `GetClosingLineShift` agree on the boundary (probe).
- FC-004, FC-005, FC-019 to FC-021: the lookup reads no trivia.
- FC-008: Fix All with two argument-position literals is pinned.
- FC-009: unchanged. Every reported literal still yields a change.
- FC-013: the fix result is unchanged, as audit 10 found. The registration change only makes the existing rewrite reachable.
- FC-015: one owner per policy (table above).
- FC-031: all three changed summaries describe the new code.
- FC-038 to FC-044: no shape declared unsupported.

## Architecture findings

_None._

## Hints

- Other code fixes may share the pre-existing pattern of a diagnostic located on an expression and a `FindNode(span)` without `getInnermostNodeForTie`. I did not survey them. That is outside this run and not a finding.

## Follow-up candidates

_None._ The follow-up candidate of audit 10 is no longer needed: finding #1 was repaired in this run.
