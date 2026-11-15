Role: auditor
Commit: 4cd553b
Verdict: #1147: findings; #1152: pass; #1386: findings; #1387: pass; #1594: pass; #1655: pass; #1736: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I read `00-lead.md`, handovers 01–13, both change docs, the changed specs, the rule page, and the whole diff `3b3639e...4cd553b`, and opened the counterparts the checks name (`TokenGapAnalysis`, `SyntaxTokenPositionUtilities`, `SyntaxTriviaUtilities`, `DocumentationCommentUtilities`, the RH7304 and list-shape code fixes, RH5204, `FluentChainLink`).

Evidence beyond static tracing comes from a scratch console probe outside the repository (deleted with the scratchpad). It runs the shipped analyzers through `CompilationWithAnalyzers` in `DocumentationMode.Parse` and `None`, once against HEAD's Release `Reihitsu.Analyzer.dll` (built 15:03, after the last `src/` commit b3e3360) and once against a Release build of `3b3639e` from a temporary worktree (removed). I also ran `scripts/format.sh` on two fixtures. LF and CRLF were both run where a finding turns on them.

## Whole run

### Scope
- Head 4cd553b, checked out and clean. `origin/main` = `git ls-remote origin refs/heads/main` = 3b3639e2e056e8748b970f30d9aa7ee297a29779, and it is an ancestor of HEAD.
- Full audit of `3b3639e...4cd553b`: 105 files. The production part is 18 files under `src/` outside test projects. The diff matches what 06/07/08/10/12 describe.
- `scripts/lane-check.sh behavior-change` → "LANE CHECK: confirmed — claimed behavior-change, diff needs behavior-change".

### Findings
Listed under their issues: #1 and #3 under #1147, #2 under #1386.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Comment that starts a token's line (`CommentPositionUtilities.TryGetLineStartComment`) | The token's leading trivia, scanned backward from the token over whitespace and `/* */`/`/** */` trivia, up to the first other trivia. The found comment must start its line (`TokenGapAnalysis.StartsLineAtLeadingTriviaIndex`). Without the flag it must also start on the token's line | SHARED-COMMENT-011 and SHARED-COMMENT-012. RH5201's deleted private `GetLineStartColumn` (first non-whitespace character on the token's line) | Agrees for one or more one-line comments (`/* a */ /* b */ x`, `/** d */ x`), Parse and None. **Disagrees for a line that starts with a multi-line comment's tail**: the flag-less helper returns the token's column, the deleted RH5201 copy returned the tail's column, and SHARED-COMMENT-012 asks for a report whatever the column | finding #1 |
| Which comments `InitializerLayoutAnalysisUtilities.IsAlignedAt` checks | Every comment in the leading trivia that starts on an earlier line, plus the line-start comment on the token's line | SHARED-INIT-001 as amended; SHARED-COMMENT-011 (only the line's first element counts); SHARED-DIAG-010 | Agrees on the token's line. On an earlier line every comment is still checked, also one behind the comment that starts that line | finding #3 |
| RH5305 rule-2 exemption | `TryGetLineStartComment(closeBracket, true)`: a comment in the bracket's leading trivia that starts its line, or a tail of one | RH5305-001, "no comment in front of it on the bracket's line" | Agrees except for a comment tail in the previous token's *trailing* trivia (`1 /* a⏎ */ ]`) | hint |
| RH7304 expected indentation (analyzer and fix) | `TryGetLineStartComment(first token, true)`, measured from the comment's full-span start | `TryGetRegionIndentation` calls the analyzer's `GetExpectedIndentation`, so it is one owner | Agree. Checked `Fields`, `Lines`, `Two` (`/* a */ /* b */`), `Doc` (`/** d */`), and `Own` (own-line `//`) on HEAD and main | ok |
| Split-mode report guard (`ContainsConditionalDirectiveInShape`) | Leading trivia of the target's tokens, of each attribute's first token, of each comma, and of `]` | `SyntaxTriviaUtilities.ContainsConditionalCompilationBoundaryInElementGaps`, as RH5102 and RH5109 use it. The fix guard `InteriorContainsCommentOrDirective` covers the whole interior | Every gap of SHARED-ATTR-019's shape is covered, because a directive always lands in the leading trivia of the token behind it. The fix guard is a superset by spec. Probe: `[A,⏎#if DEBUG⏎ B,⏎#endif⏎ D]` not reported; `#if` inside `A(…)` still reported | ok |
| Merge-mode report guard | `DescendantTrivia` of the list's parent over `[first.OpenBracket.End, reported.CloseBracket.Start)` | SHARED-ATTR-020's stretch. The fix's `CanMergeAttributeListGroup` covers the whole group | Agrees. Probe: `[A]⏎#if⏎[B]⏎#endif⏎[D]` not reported; `[A] [B]⏎#if⏎[A]⏎#endif⏎[D]` reports `[B]` only | ok |
| `IsDocumentingComment` | The declaration's first-token leading trivia, forward from the comment. It is separated by a comment or directive, followed by a later documentation comment | The compiler's reverse scan, in which any non-whitespace trivia in front of a collected documentation comment detaches the earlier ones. `GetDeclarationStartingWith` (moved from RH5034) | Agrees in both modes on every probe input (`[Obsolete]⏎/// a`, `/// a⏎// note⏎/// b`, `/// a⏎#region⏎/// b`, accessor, parameter, `/** c */` in arguments, a trailing `/// t`, an `if` header). The count of diagnostics on a run of several `///` lines differs by mode | finding #2 |
| RH5109 "multi-line" | Every parameter's span (attributes and default included) | FMT-BREAK-012 by elements; RH5109-004's conditional guard (unchanged) | Agrees, except entry 4, where the maintainer decided (P11). The new reported set is a subset of `main`'s (a parameter that spans lines, or parameters on two lines, implies that the parentheses are on two lines), so the unchanged fix sees no new input | ok |
| Comment diagnostic span (`GetCommentLocation`) | From the full-span start to the end of `ToFullString()` with trailing CR/LF trimmed | The RH5204 fix's `FindTrivia(location.Start)` | Agree. The start lies in the trivia's full span, Parse and None | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Column of a line's first element (SHARED-COMMENT-011) | `CommentPositionUtilities.TryGetLineStartComment`/`GetLineStartColumn`. Callers: RH5110, RH5201, RH5203, RH5205, RH5206, RH5302, RH5305, RH5308, `InitializerLayoutAnalysisUtilities`, RH7304. `RH5204…Analyzer.AlignCommentIndentation` is kept (decision 15: line text plus SHARED-COMMENT-012, another question). `Reihitsu.Formatter …/LineStartIndex.TryGetLineStartColumn` belongs to another component. `rg "char.IsWhiteSpace\(\|GetLineFromPosition" src/Reihitsu.Analyzer` finds no other copy | One analyzer owner. Moving RH5201 onto it changed RH5201's tail semantics (finding #1). `IsAlignedAt` still decides earlier-line comments on its own (finding #3) |
| Whether a `///`/`/** */` documents a declaration (SHARED-COMMENT-001) | `CommentPositionUtilities.IsDocumentingComment` + `GetDeclarationStartingWith`, moved from RH5034 with no copy left. `Reihitsu.Core DocumentationCommentUtilities.DocumentsFollowingCode` answers a different question (whether the token opens its node, for the formatter's relocation and SHARED-DOC) | ok: different decisions |
| Span of a comment diagnostic | `CommentPositionUtilities.GetCommentLocation`. Callers: `CommentPositionAnalyzerBase`, RH5048, RH5204 | ok |
| Conditional directive in a list's gaps | `SyntaxTriviaUtilities` (Core), reused | ok |
| Ordinary comment | `CommentPositionUtilities.IsOrdinaryComment` (analyzer). `Reihitsu.Formatter …/TriviaListUtilities.IsOrdinaryComment` is the formatter's own blank-line and gap decision and is unchanged | ok |

### Failure classes dismissed
- FC-004, FC-006, FC-018, FC-022–FC-026, FC-029, FC-036: no rewrite changes. The only newly offered fix (RH7304) replaces only the directives' leading whitespace, and the merge/split fixes are unchanged.
- FC-005, FC-019, FC-020, FC-021: comment and directive variants are tested side by side (attribute rules `#if`/`#pragma`/`#region`, RH5109, RH5111, RH7304).
- FC-007, FC-008, FC-009: the RH7304 fix is offered only for whitespace indentation, is tested with Fix All and with a single action over two regions, and converges. The newly reported misplaced-comment inputs use `VerifyCodeFixIsWithheldOrProducesOneOf`. The merge fix still guards the whole group.
- FC-010, FC-011, FC-012, FC-028, FC-030: no formatter change.
- FC-014: no new rule.
- FC-027: every RH5109 owner kind is tested. RH7304 runs on regions in the type body only (RH7304-005 is unchanged).
- FC-032: the probe used the Release DLL built after the last `src/` commit.
- FC-033, FC-034, FC-035: LF/CRLF rows; the issue inputs; expected values from the specs (04, 07).
- FC-037: no comment-only proof is claimed.
- FC-038–FC-043: no unsupported shape is declared or changed. RH5111-005 avoids an FMT-UNSUP-004 copy (OQ3 B).
- FC-001, FC-002, FC-003, FC-013, FC-015, FC-016, FC-040: reached. See findings #1–#3.

### Architecture findings
_None._ Decision 4's consolidation holds. Finding #1 is an ordinary defect of that move: the one owner decides a comment-tail line differently from the copy it replaced. Because the owner is now single, SHARED-COMMENT-012's "report whatever column" for a token behind a tail is decided in one place for every caller. See the first hint.

### Hints
- **The one owner gives a token behind a multi-line comment's tail its own column, for every caller.** This is pre-existing and not caused by the run, apart from finding #1's RH5201 member. SHARED-COMMENT-012 says a rule that checks line starts MUST report such a token "whatever column it reaches". On HEAD and on `main` alike, `P(1,⏎  /* a⏎     */   2);` with `2` in the argument column is not reported by RH5203, which is the spec's own example shape. RH5110's `/* a⏎*/      where T : class` with `where` in the constraint column is not reported either. Follow-up candidate: implement SHARED-COMMENT-012 once in `CommentPositionUtilities` (the line is out of place when it starts with a tail). It is the same mechanism as finding #1, so the lead may fold it into that repair.
- RH5305: `[⏎    1 /* a⏎ */ ]`, with the comment as trailing trivia of `1` and its tail in front of `]`, is reported by rule 2 when the columns differ. RH5305-001's "no comment in front of it on the bracket's line" reads as not reported. SHARED-COMMENT-012's "token behind code on the comment's first line" reads as reported. `main` reports it too. It is rare, and the specs disagree, so this is not a finding.
- The `RH5305-001` inline comment in `RH5305…Analyzer.cs` ("unless a comment stands in front of the closing bracket on its line") is broader than the check, which covers only a comment that starts the bracket's line or the tail of one. It matches the spec text, so I raise no FC-031 finding.

## thoenissen/Reihitsu#1147

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Analyzer/Rules/Layout/RH5201MethodChainsShouldBeAlignedAnalyzer.cs:74` (`CommentPositionUtilities.GetLineStartColumn(link.OperatorToken)`), `src/Reihitsu.Analyzer/Core/CommentPositionUtilities.cs` `TryGetLineStartComment` with `isEarlierLineAllowed: false` | FC-015, FC-016, FC-013, FC-003 | – | PR-introduced | The deleted private `GetLineStartColumn` measured the line's first non-whitespace character, as RH5201-002 states ("a link that starts a line whose first character that is not whitespace is not in the column of the chain's anchor"). The shared helper returns the operator's column when the link's line starts with the tail of a multi-line comment. Counterexample (anchor = `.B` at column 17): `var r = a.B()⏎                 .C()⏎                 /* x⏎        */       .D();`. `main` reports `.D` (RH5201 at 8,18). HEAD reports nothing, in Parse and None, LF and CRLF. `scripts/format.sh` changes the shape (it breaks the line in front of `.D()`), so SHARED-DIAG-010 and SHARED-COMMENT-012 ("MUST report the token, whatever column it reaches") both demand the report. Members (axis: what leads the link's line × where the `.` and the line's first character stand), each probed on `main` and HEAD: one `/* */` at the anchor holds; a misplaced `/* */` holds (reported); two comments hold; `/** d */` holds; tail with `.` at the anchor **fails** (newly unreported); tail with `*/` at the anchor and `.` elsewhere **changed**: newly reported on HEAD, not reported on `main`. SHARED-COMMENT-012 demands that report, RH5201-002's "first character that is not whitespace" forbids it, and no test asserts it. A tail with both columns elsewhere holds (reported). The existing tail test (`VerifyFixBreaksLinkBehindMultiLineCommentTailInOnePass`) has neither column at the anchor, so it cannot see this | Make RH5201 report a link whose line starts with a comment tail, whatever its column (SHARED-COMMENT-012). Either the shared owner exposes "the line starts with a tail" and the callers treat it as misplaced (see the first hint under *Whole run*: this also closes RH5203/RH5110's pre-existing gap), or narrowly in RH5201. Align RH5201-002's "first character that is not whitespace" with SHARED-COMMENT-012 for the tail line. Add tests for both changed members under LF and CRLF |
| 3 | medium | `src/Reihitsu.Analyzer/Core/InitializerLayoutAnalysisUtilities.cs` `IsAlignedAt` (earlier-line loop); `sdd/specs/shared/initializer-layout.md` SHARED-INIT-001, first bullet | FC-016, FC-013 | – | same requirement | The amendment stops checking a further comment behind the line-start comment on the token's line, but on an earlier line every comment is still checked. Counterexample: `new C⏎{⏎    P = 1,⏎    /* a */ /* b⏎    */ Q = 2⏎};`, with `/* a */` in the member column. RH5301 reports `Q = 2` (HEAD and `main`, Parse and None) because `/* b` does not start at the member column. `scripts/format.sh` leaves the input unchanged, so formatter output is reported (SHARED-DIAG-010). This is the same decision as the run's amendment (only the comment that starts a line is that line's first element, SHARED-COMMENT-011), on the sibling member "the further comment starts on an earlier line and its tail leads the token's line". Members: `/* a */ /* b */ P` holds; `/* a⏎ */ /* b */ P` holds; one comment holds; `/* a */ /* b⏎ */ Q` fails. The probe used RH5301; RH5303 and RH5304 call the same `IsAlignedAt` | Check only the earlier-line comments that start their line (as `TryGetLineStartComment` decides), and word SHARED-INIT-001's first bullet the same way. Add a formatter test for the member. The shape is rare; if the lead prefers, record it as a decision instead, since only a spec reading separates the two outcomes |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `(F) …BehindLeadingComment…IsNotReported` (RH5110, RH5203, RH5205, RH5206 ×2, RH5302, RH5305, RH5306, RH5308) and `(F) …TwoLeadingComments…` (RH5301 ×2, RH5303, RH5304), `…CommentTailAndFurtherComment…` | Formatter output with a comment-led line is unreported and stable | A diagnostic, or a formatter change, LF or CRLF | `VerifyFormatter` (formatter pass, analyzer re-run, LF/CRLF) | ok |
| `…BehindMisplacedLeadingComment(s)IsReported` (RH5110, RH5203, RH5205, RH5206 ×2, RH5302, RH5308, RH5301, RH5303, RH5304) | Reported where the rule locates it; the fix is withheld or writes exactly the formatter's layout in one pass | No diagnostic, a different fix text, or a second iteration | `VerifyCodeFixIsWithheldOrProducesOneOf(…, true, …)` | ok: no requirement fixes the output beyond the formatter's layout |
| RH5305/RH5306 `…MisplacedLeadingCommentIsNotReported` | The rule's own comment exemption still applies (decision 11) | A diagnostic | `Verify` LF/CRLF | ok |
| RH5201 tail at the anchor | — | — | — | missing (finding #1) |

### Failure classes dismissed
See *Whole run*.

## thoenissen/Reihitsu#1152

### Findings
_None._ The RH5204 comment location is `GetCommentLocation`: from `///`/`/**` to the end of the text without the line break. The fix finds the trivia at `location.Start`. Tests cover Parse and None, single- and multi-line `///`, and `/** */`, LF/CRLF. The formatter twin's span was corrected in 07.

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 8 rewritten RH5204 tests, `VerifyDocumentationCommentDiagnosticStartsAtExteriorAndExcludesLineBreak`, `…EndsAtLastLineText`, `…StartsAtOpeningDelimiter`, `…IsTheSameWithoutDocumentationParsing`, formatter `VerifyFormatterFixesDelimitedDocumentationCommentOnOwnLine` | Exact span per RH5204-005, the same in both modes; the fix still re-indents the whole block | A start behind `///`, an end on the next line, a mode difference, or a partial re-indent | Exact markup or `ExpectedDiagnostic` spans; `VerifyCodeFixWithEndOfLine` | ok |

## thoenissen/Reihitsu#1386

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | medium | `src/Reihitsu.Analyzer/Base/CommentPositionAnalyzerBase.cs` `OnSyntaxTree` with `CommentPositionUtilities.IsOrdinaryComment`/`GetCommentLocation`; RH5048 | FC-040 | – | PR-introduced | A run of several `///` lines that documents nothing is one `SingleLineDocumentationCommentTrivia` when documentation comments are parsed, but one `SingleLineCommentTrivia` per line when they are not. Each of the 17 rules therefore reports one diagnostic in Parse mode and one per line in None mode. Counterexample (RH5038): `var result = Run(⏎    /// first⏎    /// second⏎    1,⏎    2);`. Parse: `RH5038 (6,13)-(7,23)`, one diagnostic spanning both lines. None: `RH5038 (6,13)-(6,22)` and `RH5038 (7,13)-(7,23)`. `main` reports neither. SHARED-COMMENT-001 says the verdict MUST NOT depend on documentation parsing, and SHARED-COMMENT-003 asks for one diagnostic per comment. The documentation mode is a project setting, so the same code shows a different number of diagnostics depending on that setting. 10-spec-author recorded the gap as an internal choice. No test has a multi-line `///` run in a comment-placement position | Make the count independent of the mode: in Parse mode, report each `///` line of a structured run as its own diagnostic (its exterior through the end of its text), as None mode does. Or state in SHARED-COMMENT-003 which count holds and implement it in both modes. Add a two-line `///` test in Parse and None for one rule of the base and for RH5048. Which count is right is a diagnostics question; if the lead finds no spec reading that decides it, it is an output question |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 32 per-rule `VerifyTripleSlash…`/`VerifyDelimitedDocumentationStyle…IsReported`, RH5033 issue inputs | A non-documenting `///`/`/** */` is reported like `//`, with the same span in Parse and None | Missing in one mode, or a span that differs | `Verify` with Parse/None transforms, exact markup | ok for single-line comments. A multi-line run is missing (finding #2) |
| RH5034 `…DetachedAndLaterDocumentationComment` (×3), `…SeparatedByBlankLineIsReported` | The compiler's detachment rule (comment, `/* */`, directive); a blank line does not detach | `// note` reported, or the blank-line case unreported | `Verify` Parse/None | ok |

## thoenissen/Reihitsu#1387

### Findings
_None._ Both report sites of `TargetAttributeListShapeAnalyzerBase` are guarded as SHARED-ATTR-019/020 state (see *Guard scope*). Every split rule has a conditional test and a `#region` test (05). The merge rules are covered by RH5524/RH5529. RH5102, RH5109, and RH5111 are characterized.

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyNoDiagnosticWhenConditionalDirectiveIsBetweenTheAttributes` (14 rules), `…BetweenTheLists` (RH5524, RH5529), `VerifyListInFrontOfConditionalDirectiveIsStillReported`, `VerifyDiagnosticWhenConditionalDirectiveIsInFrontOfTheList`, `…DisabledTextHoldsAnAttribute…` | Not reported inside the span; reported at its boundary | Any diagnostic inside, or none at the boundary | `Verify` LF/CRLF | ok |
| `VerifyDiagnosticWithoutCodeFixWhen…` (`#region`, `#pragma`, `#if` in an argument) | Reported, no fix | A fix offered or no diagnostic | `VerifyNoCodeFixIsOffered` | ok |

## thoenissen/Reihitsu#1594

### Findings
_None._ RH7304-001/-004 hold for `Fields`, `Values`, `Names`, `Others`, `Lines`, two comments, and `/** */`. The fix is offered only when the indentation is whitespace, and it changes only the reported pair (single-action assertion over two regions). The change doc lists the newly offered fix, answered OQ1 A.

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyDirectivesAreComparedWithTheCommentThatLeadsTheFirstLineOfCode`, `…FirstOfSeveralComments…`, `…OwnLineAboveTheFirstToken…`, `VerifyFixIndents…` ×2, `(F) VerifyCommentLedRegionsAreNotReportedAndStayAsWritten` | Directives compared with the line's first element; the fix indents exactly the reported pair | Extra or missing diagnostics; a fix that touches the other region | `Verify`, `VerifyCodeFixWithEndOfLine`, `ApplyCodeFixAsync`, `VerifyFormatter`; LF/CRLF | ok |

## thoenissen/Reihitsu#1655

### Findings
_None._ No production change. RH5111-005 and SHARED-DIAG-010 record OQ2 B and OQ3 B, and the two characterization tests pin `main`'s output (reported, no fix).

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyCommentOrPragmaInEveryWrappedGapKeepsTheReportWithoutCodeFix`, `VerifyRegionDirectiveOrCommentInTheWrappedGapKeepsTheReportWithoutCodeFix` | Reported, no fix, for every construct kind | Missing diagnostic or an offered fix | `VerifyNoCodeFixIsOffered` | ok |

## thoenissen/Reihitsu#1736

### Findings
_None._ The probe confirms every case: `A(⏎int a, int b)` and `B(int a, int b⏎)` are no longer reported. `D`, `G` (a spanning parameter that shares a line), and `H` stay reported. `F` (`[Obsolete]⏎int a, int b`) is reported neither on `main` nor on HEAD. The fix is unchanged, and its input set shrinks. The rule page matches change-doc entry 6 (OQ4 A).

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Entries 1–4 tests, `…ForEveryOwner`, `…WithASpanningParameterIsReported`, `VerifyListWithParametersOnTwoLinesBelowOpeningParenthesisIsFixed` | Parameters alone decide; the fix result is stable under the formatter | A diagnostic on a one-line parameter set; none on a spanning one; a formatter change on the fix result | `Verify` LF/CRLF; Fix All plus `AssertFormattingChangesNothing` | ok |
