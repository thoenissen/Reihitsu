Role: auditor
Commit: c79f4bf
Verdict: #1147: pass; #1152: pass; #1386: pass; #1387: pass; #1594: pass; #1655: pass; #1736: pass

This audit covers repair round 3 only. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the recorded decisions 17–20. I read `00-lead.md` (decisions 17–20), `14-auditor.md`, handovers 15–19, the two amended shared specs, and the repair diff `f808874..c79f4bf`. I also opened the counterparts the checks name: `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex`/`AnalyzeTriviaList`, `CommentPositionUtilities.TryGetLineStartComment`/`GetLineStartColumn`/`GetCommentLocation`, every `IsAlignedAt` caller (RH5301, RH5303, RH5304), every reporting site of the 17 SHARED-COMMENT-003 rules, `DocumentationCommentUtilities`, and SHARED-CHAIN-002/-008. Evidence comes from static tracing. No finding depended on a run, so I executed no test.

## Whole run

### Scope
- Head c79f4bf is checked out. `origin/main` is 3b3639e2e056e8748b970f30d9aa7ee297a29779, the SHA in my prompt, and it is an ancestor of HEAD.
- Repair delta against f808874, which holds the first audit (4cd553b plus handover commits). Production changes are 5 files: `CommentPositionUtilities`, `CommentPositionAnalyzerBase`, RH5048, `InitializerLayoutAnalysisUtilities`, RH5201. Specs: SHARED-COMMENT-003 and SHARED-INIT-001. Tests: 10 test files, additions only. The tests are unchanged between 98aeef9 and 5b8fbaf, and nothing under `src/` changed after 5b8fbaf. The diff matches 15/16/17.
- `git diff 3b3639e c79f4bf -- src/Reihitsu.Analyzer/Rules/Layout/RH5201MethodChainsShouldBeAlignedAnalyzer.cs sdd/specs/analyzer/RH5201.md` is empty, so RH5201 is back to `main` byte for byte.
- The scope did not grow: the repair stays inside the three findings' surfaces, and no requirement ID or change doc was added.
- `scripts/lane-check.sh behavior-change` → confirmed.

### Previous findings
| # | Status | Evidence |
|---|---|---|
| 1 | closed | RH5201 is `main`'s code again. The private `GetLineStartColumn` measures the first non-whitespace character. The counterexample (`.D` in the anchor column behind a misaligned `*/` tail) is reported again and is tested: analyzer LF/CRLF with one fix iteration, and a formatter test at (8,18). The reverse member (`*/` in the anchor column, `.` elsewhere) was not reported on `main` and is not reported now. Decision 17 drops it, so it is not a finding. |
| 2 | closed | `GetCommentLocations` yields one location per `///` line of a `SingleLineDocumentationCommentTrivia`, which is what None mode produces line by line. Both report sites of the 17 rules use it: `CommentPositionAnalyzerBase.OnSyntaxTree` (16 rules) and RH5048 (`rg ReportDiagnostic` over RH503x/RH504x and `Base/Comment*`). Tests: RH5038 (spec example, 3 diagnostics) and RH5048 (2 diagnostics) in Parse and None, LF and CRLF, with exact spans. |
| 3 | closed | The earlier-line loop of `IsAlignedAt` now skips a comment that does not start its line, using the same Core predicate that `TryGetLineStartComment` uses. The counterexample `/* a */ /* b⏎ */ Q` and the own-line `/* a */ /* b */` are tested not-reported and misaligned-reported for RH5301, RH5303, and RH5304, as analyzer tests and formatter tests. |

### Findings
_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which earlier-line comments `IsAlignedAt` checks | Each comment in the token's leading trivia whose index passes `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex` (the previous token's trailing trivia plus the leading trivia before the index end with a line break and hold no content behind it; a multi-line comment's tail counts as content) and whose start line is not the token's line | `TryGetLineStartComment(token, true)`, which scans backward with the same predicate, skips comments that do not start their line, and stops at the first other trivia. SHARED-INIT-001's first bullet as amended | Agree. In leading trivia, a comment that does not start its line can only follow another comment or a comment's tail, because a directive and a `///` comment end their line. So the filter exempts exactly the spec's class ("behind a comment that starts that line or behind the end of a comment that started on a line above"). Traced: `/* a */ /* b */⏎P` (the first is checked), `/* a */ /* b⏎ */ Q` (`/* a */` is checked, and `Q` is not checked because the line-start comment starts on an earlier line), `/* a⏎ */ /* b */ Q` (`/* a` is checked), and `/* a */ // b⏎P` (`// b` is not checked) | ok |
| Diagnostic span per `///` line (`GetCommentLocations`) | Each line of the trivia's line span, clipped to `FullSpan`. The span starts after leading whitespace and ends at `line.End`, which excludes the line break. A line with nothing left is skipped: the line behind the trailing newline of the trivia, or a range made only of whitespace | `GetCommentLocation` for one-line `///` (from `FullSpan.Start` to the text with CR/LF trimmed) and for `/** */`. None mode's `SingleLineCommentTrivia` per line | Agree. A one-line doc comment gets the same start (`FullSpan.Start` is the `///`) and the same end (trailing spaces included in both). A run gives one span per line that equals None mode's trivia span. `/** */` stays one location in both modes. A trailing `///` behind code is lexed as an ordinary comment in both modes. None of the 17 rules has a code fix, so the extra diagnostics cannot overlap a Fix All | ok |
| RH5201 line-start column | Text of the operator's line up to the operator, first non-whitespace character (`main`) | `CommentPositionUtilities.GetLineStartColumn` (trivia-based). The two differ only for a line led by a comment's tail | They diverge for the tail-led line only, and that is intended by decision 17 (restore `main`, and drop the tail class with decision 20) | ok, by decision 17 |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Column of a line's first element | `CommentPositionUtilities.GetLineStartColumn`/`TryGetLineStartComment`, called by RH5110, RH5203, RH5205, RH5206, RH5302, RH5305, RH5308, RH7304, and `InitializerLayoutAnalysisUtilities`. RH5201's private `GetLineStartColumn` is again `main`'s copy | Two owners. The second one is pre-existing (`main`), and decision 17 restored it on purpose. It differs only for tail-led lines, which decisions 17 and 20 drop. Not a finding |
| Comment diagnostic location | `GetCommentLocation` (RH5204's whole documentation block, and every non-`///` comment) and `GetCommentLocations`, which builds on it, with callers `CommentPositionAnalyzerBase` and RH5048. No other reporting site among the 17 rules that use SHARED-COMMENT-003 (`rg -l SHARED-COMMENT-003 sdd/specs`). `DocumentationCommentUtilities` has no per-line span helper to reuse | ok |
| "Starts its line" for a leading-trivia comment | `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex` (Core), used by both `IsAlignedAt` and `TryGetLineStartComment` | ok: one owner |

### Failure classes dismissed
These are re-checked against the repair delta only. The rest is byte-identical to `14-auditor.md`.
- FC-004, FC-006, FC-018, FC-022–FC-026, FC-029, FC-030, FC-036: the repair changes no rewrite and no code fix.
- FC-007, FC-008, FC-009: the 17 comment-placement rules have no code fix, so per-line diagnostics cannot overlap in Fix All. The RH5301/RH5303/RH5304 fixes are unchanged. Their misaligned tests accept only the aligned shape in one pass, or a withheld fix. The RH5201 test runs one incremental iteration and one Fix All iteration.
- FC-005, FC-019, FC-020, FC-021: the repair decides comments only. Directives cannot precede a comment on its line, so they do not reach the new filter.
- FC-010, FC-011, FC-012, FC-028, FC-038–FC-043: no formatter change, and no unsupported shape is touched.
- FC-014: no new rule.
- FC-027: the multi-line form is now the subject of every new test (`///` runs, `/** */` over two lines, comment tails). The closing-brace earlier-line member (+4 columns) runs the same filtered loop as the member case. It is untested, which 16 states, and it holds by trace.
- FC-032: no result was reused across a compiled change. Nothing under `src/` changed after the implementer's full analyzer run (9742 passed, at 5b8fbaf).
- FC-033, FC-034: the tests use the auditor's counterexamples and the spec examples verbatim, with LF and CRLF rows.
- FC-035: expected values come from the specs. The misaligned fixed shapes are the not-reported shapes that the unchanged formatter writes.
- FC-037: no comment-only proof is claimed.
- FC-040: closed by finding #2's repair. The count and spans are now mode-independent, and both modes are asserted.
- FC-001, FC-002, FC-003, FC-013, FC-015, FC-016, FC-017, FC-031: reached. See *Guard scope*, *Policy ownership*, and the per-issue *Assertion adequacy* tables. All hold.

### Visible output
- The `///` run gets one diagnostic per line in Parse mode (`main` reported none). In None mode each line is a `//` comment that starts with three slashes (SHARED-COMMENT-001, unchanged text), and unchanged SHARED-COMMENT-003 reports each comment separately. SHARED-COMMENT-001's unchanged "the same source must not change its diagnostics" with the documentation mode then demands the same count in Parse mode. Decision 18 records this.
- SHARED-INIT-001's newly unreported shapes (`/* a */ /* b */` on its own line, `/* a */ /* b⏎ */ Q`) are formatter output that the formatter leaves unchanged, which `VerifyFormatter` asserts. Unchanged SHARED-DIAG-010 and SHARED-COMMENT-011 demand that they are not reported.
- RH5201's output equals `main`.
- No undecided output.

### Architecture findings
_None._ The repair resolved finding #1 by narrowing to `main`, finding #2 at the one reporting owner, and finding #3 with the shared Core predicate. No new finding repeats the class of #1–#3.

### Hints
- `InitializerLayoutAnalysisUtilities.IsAlignedAt` summary: the rewrite left an uneven line wrap ("On the token's line, the comment that starts" breaks early). This is cosmetic only, and its content matches the code.

## thoenissen/Reihitsu#1147

### Findings
_None._ Finding #1 is closed (RH5201 is restored to `main`) and finding #3 is closed (the `IsAlignedAt` earlier-line filter). See *Whole run*.

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5201 `VerifyLinkInAnchorColumnBehindMisalignedCommentTailIsReported` (LF, CRLF) | A link whose line starts with a misaligned tail is reported although its `.` is in the anchor column, and the fix breaks the line in one pass | No diagnostic at `#0`, a different fix text, or a second iteration | `Verify` with 1 incremental and 1 Fix All iteration | ok |
| RH5201 formatter `VerifyFormatterBreaksLinkInAnchorColumnBehindMisalignedCommentTail` | The input is reported at (8,18), and the formatter output is clean and stable | Missing diagnostic, or a diagnostic or change on the output | `VerifyFormatter` (LF/CRLF) | ok |
| RH5301/RH5303/RH5304 `…BelowLineOfTwoCommentsInExpectedColumnIsNotReported`, `…BehindFurtherCommentEndingOnItsLineIsNotReported` (LF, CRLF) | A further comment on an earlier line is not checked | Any diagnostic | `Verify` | ok |
| RH5301/RH5303/RH5304 `…OutsideExpectedColumnIsReported`, `…WhoseLineStartsOutsideExpectedColumnIsReported` (LF, CRLF) | The comment that starts the earlier line is still checked (the other side of the boundary) | No diagnostic, or a fix output other than the aligned shape | `VerifyCodeFixIsWithheldOrProducesOneOf(…, true, …)` | ok |
| RH5301 `VerifyFurtherCommentsBehindCommentsThatStartEarlierLinesAreNotChecked` (spec example) and the six `…IsFixed` formatter tests | Formatter output is unreported (SHARED-DIAG-010), and misaligned input reaches it | A diagnostic on the output, or a formatter change on aligned input | `VerifyFormatter` | ok |

## thoenissen/Reihitsu#1152

Nothing in this repair touches RH5204 or its span: `GetCommentLocation` is unchanged, and RH5204 does not call `GetCommentLocations`. The first audit's verdict stands.

## thoenissen/Reihitsu#1386

### Findings
_None._ Finding #2 is closed. See *Whole run*.

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5038 `VerifyEveryLineOfTripleSlashRunIsReportedOnItsOwnInEveryDocumentationMode` (LF, CRLF) | A two-line `///` run gives two diagnostics, each from its `///` to the end of its text. A two-line `/** */` gives one. The result is the same in Parse and None | 2 diagnostics in Parse, a span that includes the line break or leading whitespace, or a split `/** */` | `Verify` with Parse and None transforms, exact markup `#0`–`#2` | ok |
| RH5048 `VerifyEveryLineOfTripleSlashRunBeforeFirstCallOfWrappedChainIsReportedOnItsOwnInEveryDocumentationMode` (LF, CRLF) | The same per-line count on the separate RH5048 reporting path | 1 diagnostic in Parse | `Verify` Parse/None, exact markup | ok |

## thoenissen/Reihitsu#1387

Not touched by this repair. The first audit's verdict stands.

## thoenissen/Reihitsu#1594

Not touched by this repair. RH7304 still calls `TryGetLineStartComment(…, true, …)`, which is unchanged. The first audit's verdict stands.

## thoenissen/Reihitsu#1655

Not touched by this repair. The first audit's verdict stands.

## thoenissen/Reihitsu#1736

Not touched by this repair. The first audit's verdict stands.
