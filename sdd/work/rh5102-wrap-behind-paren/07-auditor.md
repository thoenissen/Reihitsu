Role: auditor
Commit: faad5a17dd4c2874108175be617c5b2e92c1bd05
Verdict: pass, text cleanup

My prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing to ignore.

## Scope

- Full audit. Head `faad5a1`, checked out, clean tree. Base `origin/main` = `6a3d59f10512b42152ddfbf9c127225c11bac181`, which matches the prompt and is an ancestor of head.
- The diff matches the handovers. Production code: `RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.OnArgumentList`, `RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.OnParameterList`, and the new `SyntaxNodeUtilities.IsPartlyWrapped<TNode>`. Specs: RH5102-001 and the Why of FMT-BREAK-012, plus the `Uses` rows. One change doc. Tests: the RH5102 analyzer, the RH5102 formatter parity tests, and `SyntaxNodeUtilitiesTests`.
- No C# changed after the cleaner's commit `44e0f2e`. `git diff 44e0f2e..HEAD` touches only the change doc's `Status` and `sdd/work/`.
- `scripts/lane-check.sh behavior-change`: confirmed.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `sdd/changes/2026-11/rh5102-arguments-on-one-line.md:12` (*What*, last sentence) | – | – | PR-introduced | The sentence ends "…was enough to report a list whose arguments share one line, and the fix split it." That is false for every member of the removed class on `main`. For `M(⏎1, 2)` the fix joins the list. For `M(1, 2⏎)` it changes nothing. For the comment and directive shapes no fix is offered (02-skeptic *Premise*). The doc's own *Why* (line 18) and the option-B row (line 302) say the opposite. The skeptic's amendment corrected *Why* but missed this sentence. Text only: it cannot reach compiled behavior, diagnostics, or rule metadata. | Drop ", and the fix split it", or replace it with what *Why* states. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| The list is partly wrapped (RH5102-001, RH5109-001): `SyntaxNodeUtilities.IsPartlyWrapped` | Each element's `Span` (`GetLocation().GetLineSpan()`, which is unmapped and so ignores `#line`). Start line, and whether start and end line differ. Trivia around elements, `(`, and `)` is not inspected. | (a) The old RH5109 loop, byte-for-byte the same expression: `isMultiLine \|= start != end`, `sharesLine \|= !Add(start)`, `(isMultiLine \|\| Count > 1) && sharesLine`. (b) FMT-BREAK-012's single-line reading, `LineBreakListRewriter.HasElementSplitSignal`. It reads elements too, plus a leading end-of-line on the first element and `IsMultiLineAfterAttributeFormatting`. | (a) Exact. RH5109's behavior is unchanged. (b) Agrees on the removed class: a line break behind `(` or in front of `)` only. It differs in two places, both pre-existing and outside the change's decision. First, a held first element: the formatter splits it, the rule does not report it. The spec decides this under P11 with the maintainer's answer, change-doc entry 4. Second, a line break inside a multi-line comment: the analyzer counts raw lines, the formatter counts as SHARED-BLANK-008 does. The skeptic noted this, and the lead dropped it as rare (decision 6). | holds |
| New reports are a subset of the old ones | Both readings | Old reading: report when `(` and `)` are on different lines and two arguments start on one line | Proved. If `(` and `)` share a line, no argument spans lines and every argument starts on that one line, so the new reading does not report. Every list the new reading reports has `sharesLine` and, by the step above, `(` and `)` on different lines, which is exactly the old condition. The change only removes diagnostics. It adds no FC-013, FC-007, or FC-009 member. | holds |
| RH5102-004 conditional-directive exclusion | `ContainsConditionalCompilationBoundaryInElementGaps(Arguments.GetWithSeparators(), CloseParenToken)` | Unchanged | Unchanged, evaluated before the new predicate | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| The partly-wrapped list reading by elements | `rg -n "startLines\|sharesLine\|HashSet<int>\|PartlyWrapped" src --glob '!*Test*'`: `SyntaxNodeUtilities.IsPartlyWrapped` only, called by RH5102 and RH5109. The other `HashSet<int>` hits are other decisions: `InitializerLayoutAnalysisUtilities` and RH5303 (initializer element lines and columns), RH7303 and RH7310 (removed lines), and formatter text analysis. | one owner |
| A multi-line argument list read by the lines of its parentheses | `RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.OnArgumentList:55-62`. Its decision is the column of an argument that starts a line (RH5203-001), not whether arguments share a line. Its spec defers that question to RH5102 (RH5203 non-goals). | different decision; not a second owner |
| RH5102 fix layout | `RH5102…CodeFixProvider.ApplyCodeFixAsync` delegates to `ReihitsuFormatter.FormatNodeInDocumentAsync`. It reads no lines itself. | unchanged, one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 11 `Verify…IsNotReported` analyzer tests and the spec's not-reported example (`M(⏎1, 2)`, `M(1, 2⏎)`, `M(⏎1, 2⏎)`, `// c⏎)`, `( // c⏎`, `(⏎/* c */`, `( /* a⏎b */`, `#pragma`, `#region`, five owners, named and `out` arguments) | No RH5102 diagnostic | Any RH5102 diagnostic. The verifier compares the exact set, and the input has no markup. | `Verify(source)`, LF and CRLF `DataRow`s (FC-033) | adequate |
| `VerifyListsWithEveryArgumentOnItsOwnLineAreNotReported` | `M(1,⏎2)` and `M(⏎1,⏎2)` are not reported (the issue's "Done when") | A diagnostic | `Verify`, LF and CRLF | adequate |
| The reported boundary: the spec's reported example, `(⏎` with a lambda, verbatim, raw, interpolated, or nested spanning argument, and `M(⏎1, 2,⏎3)` | Exactly the marked spans are reported. This is the other side of the boundary, behind `(⏎`, so the old paren reading cannot be what reports them (FC-003). | Missing or extra diagnostic | `Verify` with markup | adequate |
| `VerifyListWithArgumentsOnTwoLinesBelowOpeningParenthesisIsFixed`, `…WithASpanningLambdaIsFixed` | The fix gives the exact layout, and formatting it again changes nothing (RH5102-003) | Different text, or a second formatting changes it | `Verify(source, fixed)` from the batch base, plus `AssertFormattingChangesNothing`, LF and CRLF | adequate |
| `VerifyFixLeavesListsWhoseArgumentsAllStartOnOneLineAsWritten` | Fix and Fix All change only the reported list. The unreported neighbors keep their text. | A neighbor changes, or it gets reported | `Verify(source, fixed)` from the batch base, LF and CRLF | adequate (FC-008, FC-030) |
| `…WithACommentSpanningAnArgumentIsReportedWithoutCodeFix` | Reported, no fix (RH5102-004) | No report, or a fix offered | `VerifyNoCodeFixIsOffered`, LF and CRLF | adequate |
| Formatter parity: `VerifyFormatterJoinsListWrappedOnlyBehindOpeningParenthesisWithoutAReport`, `VerifyListWrappedOnlyInFrontOfClosingParenthesisIsKeptWithoutAReport`, `VerifyListWhoseClosingParenthesisACommentHoldsIsKeptWithoutAReport` | The input is not reported. The formatter output is exact. The output is not reported. A second pass is stable. | Any diagnostic before or after, a text mismatch, or an unstable second pass | `VerifyFormatter`: `Verify`, then format, `Verify`, then reformat and `Verify`, under both line endings (`FormatterTestsBase{TAnalyzer}.cs:75-88`) | adequate (FC-013, FC-017) |
| `SyntaxNodeUtilitiesTests.IsPartlyWrapped…` (5 tests) | The helper's true and false sides for arguments and parameters, including both sides of the `(⏎` boundary | A wrong boolean | `Assert.IsTrue` and `Assert.IsFalse`. Each row runs under LF and again under CRLF, except the fewer-than-two-arguments rows, where the layout is irrelevant. | adequate |

Visible output (check 10): every new expected value is covered. The not-reported values are change-doc entries 1–4. Entry 1 is the maintainer's answer A (`00-lead.md`, *Maintainer answers*, and the change doc's *Decisions*). Entries 2–4 are covered by the answer's wording and the RH5109 precedent, recorded in the change doc's *Decisions* and in lead decision 2. The reported values and the fix outputs are unchanged from `main`, which the skeptic confirmed with `apply-fix.sh`, and RH5102-003 demands them. The formatter parity outputs are demanded by unchanged requirements. FMT-BREAK-012 requires that a line break in front of an argument list's `)` stays. FMT-INDENT requires a closing delimiter that starts a line to sit in the column after `(` (`indentation.md:544`), and the parity input puts `)` at column 13, behind `Call(`. No rule page changed.

## Failure classes dismissed

- FC-001, FC-002: the predicate's span is the elements, as RH5102-001 states. Its qualifiers, "start", "spans several lines", and "at least two", are each an explicit condition. The count guard stays in the analyzer, and the helper returns false for fewer than two elements on its own.
- FC-003: both sides are covered (above).
- FC-004, FC-005, FC-006, FC-018, FC-022–FC-026, FC-029, FC-036: no rewrite changed. The fix is untouched and only receives fewer diagnostics. FC-022 is still tested on the report side (verbatim, raw, interpolated).
- FC-007, FC-008, FC-009: the reported set is a strict subset of `main`'s (proof under *Guard scope*). The `no-progress` members, `M(1, 2⏎)` and `M(⏎1, 2⏎)`, leave the set. No list is newly reported.
- FC-010–FC-012, FC-014, FC-028, FC-030: no formatter code changed. Only FMT-BREAK-012's Why changed.
- FC-013: no new member, since the change only removes reports. One pre-existing member stays: `M(1, 2, /* a⏎b */ 3)`, which the lead dropped as rare (decision 6). The held-first-element difference is decided under P11 (change-doc entry 4).
- FC-015, FC-016: the RH5102 and RH5109 copies were merged into one helper (decision 8). No other copy exists (*Policy ownership*).
- FC-017: see *Assertion adequacy*.
- FC-019–FC-021: comments and directives in the `(` and `)` gaps are tested: `//`, `/* */`, multi-line `/* */`, `#pragma`, `#region`/`#endregion`, and conditional directives through the unchanged RH5102-004 tests.
- FC-027: five owners, named and `out` arguments, and a nested call are covered. The nested same-kind case `M(⏎1, P(⏎2, 3))` was traced: the outer list is reported because `P(…)` spans lines, and the inner list is not reported. On `main` both were reported. The outer fix formats the inner list too, so Fix All does nothing more for it.
- FC-031: the RH5102 and RH5109 inline comments and the helper's XML summary describe the new code. The `OnArgumentList` summary still holds.
- FC-032: no compiled change after the cleaner's build and test run.
- FC-033: CRLF `DataRow`s on every layout test.
- FC-034, FC-035: the expected values come from the spec, the change doc, and the skeptic's `main` runs, recorded before the implementation (03-test-implementer: 28 runs failed before 37c9930).
- FC-037: no text-only claim is made. The lane is behavior change.
- FC-038–FC-043: no shape declared, and the precheck is untouched.

## Architecture findings

_None._ The duplicate loop that the narrow fix created in RH5102 and RH5109 was resolved in this run (decision 8, `5c8c36c`). Both analyzers call `SyntaxNodeUtilities.IsPartlyWrapped` and keep no copy.

## Hints

- Analyzer cost on a hot path. `OnArgumentList` runs for every argument list in a compilation. On `main` it returned after two `GetLineSpan` calls when `(` and `)` shared a line, which is the common case. Now, for every list with at least two arguments, it allocates a `Location` and computes a line span per argument, allocates a `HashSet<int>`, and boxes the `SeparatedSyntaxList` enumerator, because the helper takes `IEnumerable<TNode>`. The cost is O(k log n) per list, of the same order as the RH5102-004 trivia walk that already runs, so it is not confirmed as a problem. An exact shortcut exists: a list whose `(` and `)` share a line is never partly wrapped. Not a finding.
- RH5203 still reads a multi-line argument list by its parentheses (`RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.cs:55-62`). That decision is the column of an argument that starts a line, and it is unaffected by this run. Recorded only because the code looks like the old RH5102 reading.
