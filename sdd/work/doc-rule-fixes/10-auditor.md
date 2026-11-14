Role: auditor
Commit: 31a774b2b19bd9063fcfc05bd4065940ecb0b6e4
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `31a774b` (checked out), base `origin/main` `cf6d26b` (read with `git fetch`, an ancestor of head). Diff `git diff cf6d26b HEAD -- . ':(exclude)sdd/work'`: 49 files, matching what handovers 01–09 describe (17 production files, 15 test files, specs, change doc, the RH8307 rule page). `scripts/lane-check.sh behavior-change`: confirmed. I trusted the cleaner's Release build and the roles' focused test runs. I ran the analyzers through a scratch harness (`Compilation.WithAnalyzers`, outside the repository) against head and against a `cf6d26b` worktree. I also ran `scripts/apply-fix.sh RH8303` and `scripts/format.sh` on scratch fixtures.

Each issue section below lists only the findings, guards, and tests that belong to it. The tables that cover the whole tree are in *Whole run*.

## thoenissen/Reihitsu#1345

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | low | `src/Reihitsu.Analyzer/Core/DocumentationAnalysisUtilities.cs` `GetDocumentationCommentsBehindCode` summary | FC-031 | – | PR-introduced | The summary says the method returns comments that "document the declaration that follows them (SHARED-DOC-004)". The predicate is `DocumentsFollowingCode`, meaning the next token opens its node, so the method also returns `///` behind code in front of a statement (`M(); /// <summary>note</summary>⏎return;`), an argument, a parameter, an enum member, and a declaration's type behind its attribute list. SHARED-DOC-004 says a comment after an attribute list does not belong to the declaration. Behavior is consistent: the RH8301–RH8303 specs leave these places uncovered, main already reports their own-line counterparts (RH8303 on lines 12 and 20 of my fixture, on main and on head), and the formatter moves them the same way (checked with `scripts/format.sh`). Only the summary is wrong. | Reword the summary so it describes the predicate: comments behind code whose next token opens its syntax node. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A behind-code `///` is judged (RH8301, RH8302, RH8303) | `GetDocumentationCommentsBehindCode`: `IsWrittenBehindSource` (non-white-space, non-BOM text in front of the `///` on its line) and `DocumentsFollowingCode(trivia.Token)` | Formatter `DocumentationCommentFormattingPhase.RegisterRelocation`: the same two Core predicates | Same predicates, same spans | holds |
| RH8303 continuation lines of a behind-code comment are skipped | `IsWrittenBehindSource` on the trivia at the line's `///`, without `DocumentsFollowingCode` | `DocumentationCommentUtilities.IsLeftAsWritten`/`IsStrandedBehindCode` (the formatter keeps a stranded comment as written) | A documenting comment is judged once, on its first line. A stranded comment is reported on no line. | holds (see Hints: main reported a stranded comment's continuation line) |
| RH8303 fix chooses between the own-line path and the move | `GetLeadingWhitespaceRunStart` (spaces and tabs back to the line start) | Analyzer `IsWrittenBehindSource` (any non-white-space character) | The two differ only for another white-space character, such as a form feed, in front of an own-line `///`. That input takes the move path and leaves a line holding only that character. Nothing is lost. | holds (rare, non-destructive) |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH8303 `VerifySpecificationExampleCommentWrittenBehindCodeIsMovedOntoItsOwnLine` | Reported on the first `///` only, and moved in one pass | A diagnostic on line 2, or a second iteration | `Verify` with 1 incremental and 1 Fix All iteration | adequate |
| RH8303 `VerifyFixAllMovesCommentsWrittenBehindCodeNextToCommentOnItsOwnLine` | Fix All handles three diagnostics of both fix paths | A stale span or overlapping edits | `Verify(source, fixed)` | adequate (FC-008) |
| RH8303 `VerifyCommentWrittenBehindCodeIsMovedWithCarriageReturnLineFeed`, RH8301/RH8302 CRLF tests | The fix writes the detected line ending | An LF in CRLF output | `VerifyCodeFixWithEndOfLine` | adequate |
| RH8301 `VerifyCommentWrittenBehindCodeIsNotReportedWhenDocumentationModeIsNone` | Mode `None` reports nothing | A report in mode `None` | `Verify` + `ApplyDocumentationModeNoneToTestProject` | adequate (FC-033, real mode) |

My own checks: `scripts/apply-fix.sh RH8303` over a method-body, enum-member, two-comment, and `/* x */ ///` fixture, under LF and CRLF, gave 6 fixed, 2 no-diagnostic (the `/* */` case), and 0 not converged. `scripts/format.sh` moves the same shapes with a blank line above them.

## thoenissen/Reihitsu#1346

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH8401 lines in mode `None` (RH8401-006) | The tree parsed again with mode `Parse` (`GetRootWithParsedDocumentation`), with spans mapped back to the analyzed tree | RH8401's mode-`Parse` walk | One walk serves every mode. A probe with plain, behind-code, `////`, `#if false`, and blank-line-split comments, and a comment at end of file without a newline, gave identical reports in `Parse`, `None`, and `Diagnose`. | holds |
| RH8402 run of comments in mode `None` (RH8402-007) | `GetLeadingHeaderSpan` on the parsed-again root | The same method in mode `Parse` | Same root shape | holds |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH8401 mode-`None` tests (8) | Same lines and the same fix as mode `Parse` | A missing or extra report, or a different fix | `Verify` + mode-`None` transform; CRLF; `VerifyNoCodeFixIsOffered` | adequate |
| RH8402 `…WhenDocumentationModeIsNone` (9 rows) | Same report and fix as mode `Parse` | The fix takes in a `///` or `/** */` comment | `VerifySpecificationFileFixWhenDocumentationModeIsNone` (mode `None` on the test and the fixed state) | adequate |

## thoenissen/Reihitsu#1347

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | low | `src/Reihitsu.Analyzer.CodeFixes/Core/DocumentationCommentCodeFixUtilities.cs:96-102` `CanLayOutElement` | dead branch (decision 19) | – | PR-introduced | The conjunct `documentationComment.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)` can no longer be false. The utility has two callers. RH8305's analyzer registers only on `SingleLineDocumentationCommentTrivia`. RH8307's analyzer now skips every element whose parent is not a `///` comment (`RH8307…Analyzer.cs:154`). The `IsWrittenBehindSource` conjunct is still reachable, because RH8305 and RH8307 report non-stranded behind-code comments. | Drop the dead conjunct and its wording in the summary, or keep it deliberately; the owner decides. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH8307 judges only top-level elements | `element.Parent` is a `SingleLineDocumentationCommentTrivia` | RH8307-001 (no other element encloses it) and RH8307-002 (`/** */` not reported) | Both qualifiers are explicit conditions (FC-002). Tests cover two levels of nesting and a top-level element whose content starts with a nested element. | holds |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH8307 `VerifySpecificationExampleOnlyTheTopLevelElementIsReported`, `VerifyTopLevelElementStartingWithHalfWrappedNestedElementIsReportedAlone`, `VerifyElementsNestedTwoLevelsDeepDoNotProduceDiagnostics` | Both sides of "top level" | A nested report, or a missing top-level report | `Verify(source)` with exact markup | adequate (FC-003, FC-027) |

## thoenissen/Reihitsu#1348

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH8302 leaves a blank-line run in front of a region directive unreported | The first non-white-space trivia of the line after the run is an active `RegionDirectiveTriviaSyntax` or `EndRegionDirectiveTriviaSyntax` | `RegionDirectiveBlankLineAnalyzerBase` (RH5031), SHARED-BLANK-005 | Same directive kinds, active only. `#if` stays reported (FC-005). | holds; the RH8302 ↔ RH5031 loop is closed (FC-012) |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `BlankLineRequirementTests` FMT-BLANK-010 rows (5) | Exactly one blank line above `#region`/`#endregion` behind a documentation comment, for none, one, or several blank lines in the input | Zero or two blank lines; instability | `AssertRuleResult` (LF, CRLF, idempotency, phase stability) | adequate |
| RH8302 `VerifyBlankLineBeforeRegionDirectiveIsNotReportedBeforeOrAfterFormatting` | No RH8302 on the input or on the formatter output | RH8302 on either side | `VerifyFormatter`, LF and CRLF, second pass | adequate (FC-013) |

## thoenissen/Reihitsu#1270

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A field-like event inherits documentation | `CanInheritDocumentation`: `override` checked syntactically, then `All` events | RH8201's former private copy (`All`, after its own `override` check) | Parity holds. The copy is gone (FC-015). | holds |
| RH8106, RH8305, and RH8306 judge every top-level element | `GetDirectTags`, and `Content.OfType<XmlElementSyntax>()` for RH8305 | Each rule's -001 and -002/-004 | Every element is judged. RH8306 still reports a partial property once, through `GetReportedPart`. | holds |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH8106, RH8306 `…InEitherOrder`, `…EveryEmpty…/EverySentence…` | Reporting does not depend on position, with one diagnostic per element | Only the first element is reported | `Verify(source)` with exact markup | adequate (FC-003) |
| RH8305 `VerifyTwoOneLineSummariesOfCommentAreDetectedAndFixed` | Fix All lays out both summaries | One summary is left, or a stale span | `Verify(source, fixed)` | adequate (FC-008) |

## thoenissen/Reihitsu#1092

_N/A._ No production change (decision 8). The characterization test `VerifyUndocumentedPartWithoutAccessibilityOfPublicPartialClassIsReported` pins main's RH8002 output, and nothing else in the diff touches the group decision.

## thoenissen/Reihitsu#1153

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH8032 skips a parameter without a name | `parameter.Identifier.Span.IsEmpty` | RH8032-001 ("a parameter without a name") and SHARED-DIAG-007 | Agree | holds |

## thoenissen/Reihitsu#1617

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Analyzer/Rules/Documentation/RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.cs:75-80` (`IsPartOfPartialMember` → expanded documentation `null`); `sdd/specs/analyzer/RH8203.md` RH8203-001 | FC-001, FC-024 | – | PR-introduced | For a part of a partial member, RH8203 now reads only the part's first documentation comment. RH8203-001 says the element considered is the top-level `<inheritdoc>` of the declaration's documentation, "in a later documentation comment, or in the content that an `<include>` element brings in". It also says each part of a partial member "is covered as a declaration of its own", judged on "each part's own documentation". The code reads less than the spec and drops reports that main makes. **Failing members** (head versus `cf6d26b`, harness, mode `Parse`; every member inherits nothing): (a) the part's `<inheritdoc/>` sits in its own later comment, below a summary and separated by a blank line: method defining part (main reports at `N`, head silent); method implementing part whose comments are both its own (main reports, head silent); property `P`, indexer `this`, and event `E` defining parts (main reports at the name, head silent). (b) The part's own comment is `/// <include file='docs.xml' path='…'/>` and the included file holds `<inheritdoc/>`: method defining part (main reports at `N`, head silent). Property, indexer, and event fail in the same way, because the same branch skips expansion for every partial kind. **Holding members:** `<inheritdoc/>` in the part's first own comment (the run's tests), and a non-partial declaration with a later comment or an include (main and head both report `Plain`). The change doc's *Visible output changes* lists none of these lost reports, and no test covers RH8203-001's later-comment or include clause for a partial part. Inputs: two `///` comments separated by a blank line (which RH8302 reports) or an `<include>` on a partial member. Both are rare. | Refuse the class. Narrow RH8203-001's coverage so that, for a part of a partial member, a later documentation comment and `<include>` content are not covered. List the lost reports under the change doc's *Visible output changes*; main reports them, so this is an output question. Pin today's output with a characterization test. Alternatively, make the code read the part's own later comments and include content. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which documentation a part of a partial member has (RH8203) | `GetDocumentationComment(declaration)` (the first doc comment only) when `IsPartOfPartialMember` holds; no symbol expansion | RH8203-001: every documentation comment of the declaration and its `<include>` content, per part | The code reads a narrower span than the spec | finding #1 |
| A partial member inherits documentation (RH8201, RH8203) | `OverridesOrImplementsMember`: the symbol, or `PartialDefinitionPart ?? PartialImplementationPart`, implements an interface member; `override` checked syntactically | RH8201-001 and RH8203-001 (decided for the member as a whole) | Agree for methods, properties, indexers, and events | holds |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH8201 `VerifyFixOfImplementingPartLeavesDefiningPartReported` | The fix of one part changes only that part, and the other part stays reported | The fix touches both parts, or the other diagnostic disappears | `Verify` of the marked input, `TryApplyCodeFixAtAsync` at the implementing span, then `Verify` of the result | adequate (FC-007) |
| RH8201 `VerifyPartsOfPartialMethodInTwoDocumentsAreDetectedAndFixed` | Fix All across two documents | A stale span or a missed document | `Verify` with a second source in both the test and the fixed state | adequate (FC-008) |
| RH8203 `VerifyPartial{Method,Property,Indexer,Event}IsJudgedPartByPart` | A part's direct `<inheritdoc/>` is judged on that part only | A report taken from the other part | `Verify(source)` with exact markup | adequate for direct tags; nothing covers the later-comment or `<include>` clause per part (finding #1) |

## Whole run

### Findings
See the issue sections: #1 (high, #1617), #2 (low, #1345), #3 (low, #1347).

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| A behind-code `///` that documents the following code | `rg -n "IsWrittenBehindSource\|DocumentsFollowingCode" src` finds the owner `Reihitsu.Core/DocumentationCommentUtilities`. Its consumers are `DocumentationCommentFormattingPhase.RegisterRelocation`, `DocumentationAnalysisUtilities.GetDocumentationCommentsBehindCode` (used by RH8301, RH8302, RH8303), the RH8303 analyzer's continuation check, `DocumentationCommentCodeFixUtilities.CanLayOutElement`, and the RH8309 fix. | one owner |
| Documentation mode `None` | `DocumentationModeAnalysisContextExtensions` (skip), `DocumentationAnalysisUtilities.GetRootWithParsedDocumentation` (parse again; RH8401 analyzer and fix, RH8402 fix), RH0002 | Different decisions; no copy |
| Whether a member inherits documentation | `DocumentationAnalysisUtilities.CanInheritDocumentation`/`OverridesOrImplementsMember`, used by RH8201 and RH8203. RH8201's private event-field copy is removed. | one owner |
| Blank line above a region directive | Analyzer: `RegionDirectiveBlankLineAnalyzerBase` (RH5031) owns it, and RH8302 only defers to it. Formatter: `BlankLineTokenCleanupRewriter` and `BlankLineRegionDirectiveRewriter`, which agree only because of their order. | Analyzer side has one owner. The formatter side is decision 9 and a listed follow-up; the run does not change it. |
| Which documentation comment a part of a partial member has | RH8203 `IsPartOfPartialMember` (private); RH8201 and RH8306 read part trivia; `XmlDocumentationExpander` reads symbol XML for its other users | RH8203 deliberately has its own reading; its gap is finding #1. The other symbol-XML readers are a listed follow-up. |

### Failure classes dismissed
- FC-001, FC-002: traced for every changed guard (the guard-scope tables). Only RH8203's per-part span disagrees (#1).
- FC-003: both sides are tested for RH8302 (region versus `#if`), RH8307 (top-level versus nested), RH8106 and RH8306 (both orders), RH8303 (behind brace versus behind code), and RH8201/RH8203 (inherits versus inherits nothing).
- FC-004, FC-019, FC-020, FC-021: the RH8303 move removes only spaces and tabs between code and `///`. A `/* */` comment in that gap leaves the comment unreported (apply-fix: no diagnostic). A directive cannot stand behind code. The comment's text and other lines stay.
- FC-005: RH8302 exempts only `#region` and `#endregion`; `#if` stays reported (test).
- FC-006, FC-018, FC-022, FC-023, FC-025, FC-026, FC-029, FC-036: no rewrite of code, literal, identifier, or expression body; no rename.
- FC-007, FC-008, FC-009: RH8303 converges in one pass (apply-fix: 0 not converged), and the RH8303, RH8305, and RH8201 Fix All tests pass. The RH8303 fix always changes text: the own-line path inserts a line break, and the move path replaces at least the line end.
- FC-010, FC-011, FC-012: no formatter code changed. The RH8302 ↔ RH5031 loop is closed.
- FC-013: `scripts/format.sh` moves behind-code `///` in a method body, a parameter list, an enum, and behind an attribute list with one blank line above, as RH8303 now asks. The formatter keeps a stranded comment, which RH8303 no longer reports.
- FC-014: no new rule.
- FC-015: see Policy ownership.
- FC-016: siblings are recorded as follow-ups (RH8107 and RH8202 several elements; RH4105 with an unnamed receiver; other `XmlDocumentationExpander` readers).
- FC-017: see the assertion tables. Only #1's clause has no test.
- FC-024: finding #1.
- FC-027: RH8307 is tested two levels deep. RH8106 is tested with a nested `<returns>`.
- FC-028, FC-030: no node-level formatter change. The RH8303 fix is a text edit and formats nothing.
- FC-031: finding #2. The other changed summaries, `CanInheritDocumentation`, `OverridesOrImplementsMember`, the RH8303 fix, RH8302 `IsFollowedByRegionDirective`, RH8306 `GetSentenceSummaries`, and the RH8401 tree action, describe the new code.
- FC-032: the cleaner built and tested after the implementer's last change.
- FC-033: every mode-`None` test applies `ApplyDocumentationModeNoneToTestProject`.
- FC-034: #1617's own input is kept as a characterization test, and the shapes that reproduce are tested.
- FC-035: expected values come from the specs and the change doc (06).
- FC-037: no text-only claim in the run.
- FC-038, FC-039, FC-041, FC-042, FC-043: no shape is declared unsupported (Q3 B).
- FC-040: checked for RH8401 in `Parse`, `None`, and `Diagnose` (same reports), RH8402 (tests), and RH8301 (mode-`None` test).

### Visible output is decided
Every changed expected value is covered by an unchanged requirement or a recorded answer:
- RH8301–RH8303 behind code: SHARED-DOC-004, plus Q3 B for RH8303-006 and decision 16.
- RH8302 before a region directive: SHARED-BLANK-005 and SHARED-FIX-004.
- RH8401 and RH8402 in mode `None`: SHARED-DOC-008.
- RH8307 and its rule page: Q1 C and decision 11.
- RH8106 and RH8306: Q2 A.
- RH8305 and the RH8203 field-like event: SHARED-DIAG-006.
- RH8032: the P8b answer (decision 7).
- RH8201 and RH8203 partial members: Q4 B and decision 13.
- FMT-BLANK-010 rows: unchanged formatter output.

The only visible change no answer covers is the set of lost RH8203 reports in finding #1.

### Architecture findings
_None._ Finding #1 is a span gap inside one rule, not two components sharing a decision. The formatter's two blank-line rewriters are decision 9 and a listed follow-up.

### Hints
- RH8303 no longer reports the continuation line of a multi-line `///` stranded behind code in front of `}` (`int E; /// <summary>⏎/// stranded</summary>⏎}`). Main reported it on line 2. The change is not listed, but unchanged SHARED-DIAG-010 demands it, because the formatter keeps that comment as written (FMT-DOC-003 / `IsLeftAsWritten`). It is worth a line under the PR's `Review notes`.
- RH8301–RH8303 now also judge a behind-code `///` in a method body, an argument or parameter list, an enum member list, and behind an attribute list. Their specs leave these uncovered, but main already reports the own-line counterparts and the formatter moves them the same way, so nothing looks wrong. Worth a line under `Review notes` too.
- Decision 17 (RH8401 parses mode-`None` trees a second time) costs one extra parse per file on every build in mode `None`. That is the command-line default for projects without `GenerateDocumentationFile`, but RH0002 already reports that mode, so the cost lands only on projects that leave RH0002 unaddressed. No change suggested.
