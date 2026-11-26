Role: auditor
Commit: 29481888d82e0a752340dae52b5d2f8ce5bb9d32
Verdict: findings

## Scope

Full audit. Head 29481888d82e0a752340dae52b5d2f8ce5bb9d32, the checkout. Base `origin/main` eed7195c0b6fb583ab4271173b7a4ad022851c5f, fetched, and an ancestor of head (merged). The diff is what the handovers describe: two production files (+15/-5: the analyzer's report condition and the removed `delta == 0` early return in the fix), three RH5202 test classes, the RH5202 spec, `requirement-ids.txt`, the change doc, the rule page sentence of answer 2B, and this run's `sdd/work/` directory. Inputs: thoenissen/Reihitsu#1925 (`agent-filed`) and its comment, `00-lead.md`, handovers 01–09, the change doc, RH5202 / FMT-RAW-010 / RH5601-002 / SHARED-DIAG-003. My prompt carried no intended fix, cause, or hoped-for verdict.

Fixtures (outside the repository, `$S=/tmp/claude-0/-home-user-Reihitsu/8454a2c2-597e-59f6-a041-7ba5648efd58/scratchpad/aud`), each run under LF and CRLF:

- `scripts/apply-fix.sh RH5202 $S/fix`: an aligned-behind-tabs literal with an empty content line, an interpolated one with a multi-line `/* */` comment in a hole, a raw literal in a multi-line verbatim interpolated string's hole: fixed in one iteration, the empty line and the comment line left as they are. Nested cases (outer and nested both aligned behind tabs; three levels with all tab-aligned; outer tab-aligned, nested tab-aligned): `no-fix-offered`, as expected, because the runner tries only the first diagnostic, the outer literal, whose fix RH5202-008 withholds. The nested fix order is pinned by the HoleTests, and I traced it for three levels: inner, then middle, then outer.
- `scripts/apply-fix.sh RH5202 $S/pos` and `$S/arg2`: the same tab-aligned literal in seven expression positions. See finding #1.
- `scripts/format.sh` twice over copies of all 15 fixtures: 15 formatted, then 0. `apply-fix.sh RH5202` on the formatted output: 30 × `no-diagnostic` (FC-013 holds).

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.cs`, `RegisterCodeFixesCoreAsync`: `var node = root.FindNode(diagnostic.Location.SourceSpan);` | FC-027 (enclosing scope), FC-016 | – | same requirement | RH5202-009 says the fix MUST write the RH5202-007 spaces for a reported literal. When the literal's span equals its parent's span, `FindNode` without `getInnermostNodeForTie` returns the parent, so neither `is LiteralExpressionSyntax` nor `is InterpolatedStringExpressionSyntax` matches and no fix is registered. Counterexample: `N("""` / `→→x` / `→→""");` written as `        N("""`, then `        →→x` and `        →→""");` (quotes in column 10, eight spaces and two tabs in front of the closing quotes) is reported, but `apply-fix.sh` answers `no-fix-offered` under LF and CRLF. Failing members, all tab-aligned and reported without a fix: positional method argument, object-creation argument (`new string("""…""")`), bracketed element-access argument (`d["""…"""]`), tuple element (`("""…""", 1)`), collection-expression element (`["""…"""]`), constant pattern (`o is """…"""`), positional attribute argument (`[Obsolete("""…""")]`), both plain and interpolated (`N($"""…""")`). Members that hold: named argument (`N(s: """…""")`), `return`, local initializer, nested directly in a hole (the braces widen the parent). The defect predates the run: a misaligned literal in the same positions gets no fix on `main` either (RH5202-003; `b_arg_misaligned.cs`, `c_attr_misaligned.cs` at head, registration code unchanged by the diff). The run newly exposes it, because every tab-aligned literal in these positions is now reported for the first time and has no fix. A nested literal in such a position inside a hole never gets a fix, so RH5202-008 keeps the outer literal's fix withheld for good. No test of the four RH5202 classes has the literal in an argument, element, or pattern position. | Look the node up with `root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true)`. Add one focused test per wrapper kind (argument, attribute argument, collection element, tuple element, constant pattern), plain and interpolated, under LF and CRLF. The repair covers RH5202-003 as well. If the lead treats it as out of scope, it is a follow-up candidate, and RH5202-009 has to record the gap. |
| 2 | low | `src/Reihitsu.Analyzer/Rules/Layout/RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.cs`, `CheckRawStringAlignment`, the `openingLine.LineNumber == closingLine.LineNumber` guard | FC-015, FC-017 | – | PR-introduced | The guard is a second statement of the one-line rule that `RawStringLiteralUtilities.GetClosingLineShift` already owns. No compiling input reaches it: `OnStringLiteralExpression` and `OnInterpolatedStringExpression` filter out single-line raw tokens by kind before it runs. `VerifySingleLineRawStringLiteralsOnLinesIndentedWithTabsAreNotReported`, which the handovers name as its boundary test, passes with the guard removed. | Drop the guard. Or state the whole tab-aligned check in Core next to `GetClosingLineShift` (one helper that finds the closing line and reads its whitespace), so the analyzer calls one owner. Correct the test's stated purpose. |
| 3 | low | `RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer` class summary; `RawStringLiteralUtilities.HasTabInLeadingWhitespace` summary | FC-031 | – | PR-introduced | The class summary still says only that the analyzer "verifies raw string literals have their opening and closing quote markers aligned at the same column". It no longer covers the tab condition. The helper's summary says the predicate decides whether the stripped indentation is written as spaces. It now also decides whether RH5202 reports. Comment-only. | Name the tab condition (RH5202-009) in the class summary, and the analyzer as a consumer in the helper summary. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Report a literal aligned behind tabs (analyzer) | Leading whitespace of the line holding `literal.Span.End` (`HasTabInLeadingWhitespace(sourceText.ToString(closingLine.Span))`) | Fix: `GetStrippedIndentationSpacesWidth(closingLineText, …)` on `GetLineFromPosition(node.Span.End)`. Formatter: `RawStringLineShifts.GetSpacesWidth` on `text.Lines.GetLineFromPosition(literal.Span.End)`. RH5202-008: `HasTabIndentedNestedRawString`, the same line | Same line, same predicate, same line-break recognition (`SourceText.Lines`) | ok |
| Literal on one line is not judged (analyzer, new) | Line numbers of `Span.Start` and `Span.End` | `GetClosingLineShift`'s own check; `RawStringLineShifts` constructor | Same boundary | ok in behavior; duplicate owner (#2) |
| Fix proceeds at zero shift (removed `delta == 0` return) | Whole literal; `changes.Count == 0` still returns the document | Analyzer reports only when shift ≠ 0 or closing whitespace holds a tab | Every reported literal yields a change: the closing line is always written (`MovesLine` holds for it, since it starts in the raw token or the end token). With a tab, its whole whitespace becomes spaces; with a shift, it moves | ok (FC-009 holds) |
| Fix registration: which node the diagnostic names | `root.FindNode(span)`, the outermost node with that span | Analyzer location is `literal.GetLocation()` | Disagree when a wrapper has the same span | #1 |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Closing whitespace holds a tab | `RawStringLiteralUtilities.HasTabInLeadingWhitespace` (Core). Consumers: analyzer `CheckRawStringAlignment`, fix `HasTabIndentedNestedRawString`, `GetStrippedIndentationSpacesWidth` (fix `ApplyCodeFixAsync`, formatter `RawStringLineShifts.GetSpacesWidth`). `SyntaxIndentationUtilities.HoldsTabInFrontOfMovingLine` is a different policy (RH3004/RH7004) | one owner |
| Width of the stripped indentation to write as spaces | `GetStrippedIndentationSpacesWidth`, `GetShiftedIndentation` (Core); fix and formatter consume them | one owner |
| Alignment measure | `GetClosingLineShift`, unchanged | one owner |
| Literal on one line is not judged | `GetClosingLineShift`, analyzer `CheckRawStringAlignment` (new copy), `RawStringLineShifts` constructor (pre-existing) | analyzer copy is #2 |
| Closing-line lookup (`GetLineFromPosition(Span.End)`) | Repeated as an identical expression in Core, the analyzer, the fix (3×), and the formatter (2×) | parity by identical code; see #2's suggested Core helper |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| TabIndentation: issue input, interpolated, smart-tab, tab between spaces, `u8`, tab plus no-break space, short whitespace-only line, hole line, lone `\r` | Reported; exact fixed text; re-analysis clean; Fix All | Missing diagnostic, any differing character, a line ending changed | `VerifyFixUnderBothLineEndings` / `VerifyCodeFixWithEndOfLine` (exact text, LF + CRLF; `\r` alone for the lone-CR test) | adequate |
| TabIndentation `$$` | Same | Same | `Verify` with explicit span, `MarkupMode.None`, LF + CRLF; no `ApplyCodeFixAsync` raw-text assertion, but `Verify` compares the fixed state | adequate |
| TabIndentation no tab in front of the closing quotes, tab on a content line | Not reported | A diagnostic | `Verify(source)` LF + CRLF | adequate |
| TabIndentation whitespace-only line with a tab (CS9003) | Not reported on code that does not compile | An RH5202 diagnostic | `Verify` with the compiler error | adequate as characterization; the input does not compile |
| TabIndentation single-line literals on tab lines | Not reported | A diagnostic | `Verify(source)` | adequate for the kind filter; does not reach the new guard (#2) |
| TabIndentation Fix All | Two literals fixed in one pass | Either literal unfixed | `Verify(…, 2 diagnostics)` on the batch base | adequate |
| TabIndentation `VerifyFixMatchesFormatter…` | Fix result equals the whole-document format; result clean | Any difference | `VerifyCodeFixMatchesFormatter` | adequate |
| Hole: rewritten example-1, outer and nested both tab-aligned, two-deep, interpolated nested, `u8` nested, shift-left | Outer withheld, nested offered, exact intermediate, then outer fix offered and exact result | Outer offered early, wrong intermediate, outer never offered | `VerifyOuterLiteralFixIsWithheldAndNestedLiteralFixIsOffered`, `TryApplyCodeFixAtAsync`, `Verify(intermediate, result)` or `VerifyNestedLiteralFixRunsBeforeTheOuterLiteralFix`, LF + CRLF | adequate |
| Hole convergence | Fix All converges in two iterations | Not converged | `NumberOfFixAllIterations = 2` | adequate |
| Hole: outer tab-aligned, nested without a tab; verbatim and regular holes | Exact fixed text | Nested lines touched | `VerifyCodeFixWithEndOfLine` LF + CRLF | adequate |
| FormatterTests `…ClearsTheDiagnostic` | Input reported; formatted output exact, clean, stable, and no phase changes it alone | Missing diagnostic; output reported | `VerifyFormatter` (analyzer on input, output, and second pass; `PhaseStabilityCheck`) LF + CRLF | adequate |

Missing: no test with the literal in an argument, element, or pattern position (#1).

Visible output (check 10): every expected value the diff adds or changes is covered. Plain, interpolated, `u8`, `$$`, and nested literals reported, with the formatter's layout and a zero shift, are covered by answer A of #1925. The smart-tab literal is covered by answer 1A. The rule-page sentence is answer 2B, verbatim. The nested intermediates and the hole line kept as written are demanded by RH5202-003/-004/-008, applied with a zero shift. All of these are recorded in `00-lead.md` and the change doc's *Decisions*.

## Failure classes dismissed

- FC-001, FC-002: the condition is the spec's ("a tab in the whitespace in front of the closing quotes"), on exactly that span; see *Guard scope*.
- FC-003: both sides are pinned. A tab versus none (`…WithoutATabInFrontOfTheClosingQuotes…`, the no-break-space-only formatter test), and in the column versus not (the existing RH5202-007 shift tests).
- FC-004, FC-005, FC-019, FC-020, FC-021: the whitespace in front of the closing quotes and of content lines is literal text, where no comment or directive can stand. A `/* */` continuation in a hole stays as written (traced, and run as fixture `j_holecomment.cs`).
- FC-006, FC-022: the fix writes only whitespace the compiler strips, one space per character, so the value is unchanged. Whitespace behind the width stays (interpolated test).
- FC-007: the result's closing whitespace holds no tab and stands in the same column. Not reported again (all fix tests re-analyze). RH5601 reports no tab inside literal text (RH5601-002).
- FC-008: Fix All is covered for siblings and nested literals.
- FC-009: every reported literal yields a change (*Guard scope*).
- FC-010, FC-011, FC-012: the formatter is unchanged. Formatting twice over 15 fixtures gave 0 changes on the second pass.
- FC-013: the formatter's output is clean for all 15 fixtures, LF and CRLF. A precheck-refused file keeps its reported literal, as misaligned literals already do. Not formatter output.
- FC-014: FMT-RAW-010 owns the construct.
- FC-018, FC-023, FC-024, FC-025, FC-026, FC-029, FC-036: no renames, identifiers, member kinds, re-resolved tokens, or paired layouts.
- FC-028, FC-030: the fix does not call the formatter.
- FC-032: no project result reused across a compiled change.
- FC-033: line endings are covered (LF, CRLF, lone `\r`). Tabs are written by `Expand` and `ExpandWhitespaceMarkers`, not normalized away.
- FC-034: the issue's own input is the first test.
- FC-035: expected values come from the spec examples. The formatter cross-check was run on unchanged formatter code.
- FC-037: no text-only claim.
- FC-038 to FC-044: no shape declared unsupported, and no modeled compiler result.
- Tab-width matrix: the decision is a character count. Each whitespace character stays one column in the fix and in the formatter (FMT-RAW-010), and nothing moves at zero shift. The "shift: none" row of the matrix is this run's case and holds in every fixture.

## Architecture findings

_None._ Finding #1 is a local lookup defect in one component, not two owners of one decision.

## Hints

- `RawStringLiteralUtilities.GetClosingLineShift`'s summary calls it "the one measure by which the RH5202 analyzer decides whether a literal is aligned". It still is for alignment, but the report decision now has a second input. #3 covers the comments that need it.
- With RH5601 on, the smart-tab literal converges either way. The RH5202 fix first, then RH5601's fix, makes the literal misaligned, and RH5202 then shifts it. The reverse order shifts once. Both end at the formatter's layout (answer 1A accepted the RH5202-alone result).

## Follow-up candidates

- If the lead does not repair #1 in this run: the RH5202 code fix is not registered for a raw string literal whose span equals its parent's (positional argument, attribute argument, bracketed argument, tuple element, collection element, constant pattern). The cause is `FindNode` without `getInnermostNodeForTie: true` in `RegisterCodeFixesCoreAsync`. It is pre-existing for misaligned literals (RH5202-003).
