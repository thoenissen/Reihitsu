Role: auditor
Commit: 7b22e79
Verdict: pass

The prompt carried no intended fix, no suspected cause, and no hoped-for verdict.

## Scope

Full audit of head `7b22e79e156bb586928bc55a510281c4eb70b279` against base `origin/main` `99ae644d69a2f520c3a8786fdb601389819ff267`. The checkout is the head, `origin/main` resolves to the base, and the base is an ancestor of the head. The diff matches the handovers: Core `RegionDirectiveBlankLineUtilities`, formatter `BlankLineEditor`, `BlankLineRegionDirectiveRewriter`, `BlankLineTriviaBoundaryRewriter`, the new `RegionDirectiveLineAboveTests`, additions to the RH5031 analyzer and formatter tests, the changed expected value and new tags in `DisabledRegionLayoutTests`, and `sdd/work/`. `scripts/lane-check.sh bug-fix`: confirmed.

Evidence gathered in this audit (fixtures in the scratchpad, outside the repository):

- **Reachability of the trailing-trivia `#endregion` path.** I built an instrumented copy of the head tree in which `BlankLineTriviaBoundaryRewriter.VisitToken` throws when a token's trailing trivia holds an `#endregion`. With it, the formatter tests passed 4402/4402, the analyzer tests 6411/6411 (every code fix that delegates to the formatter included), the CLI tests 251/251, and `scripts/format.sh src --check` over the repository's 1843 files raised no exception. No caller reaches the path (see *Hints*, 1).
- **Breadth probe.** I wrote 17 fixtures beyond the skeptic's matrix. They put a region directive below `#if`/`#else`/nested `#if` directly after the `{` of a collection initializer, an anonymous object, a switch expression, an enum, a nested block statement, a `do` body before `} while`, an empty switch block, an `if` block before `else`, a namespace holding a using directive, and an accessor list after another accessor. They also cover an empty region inside a conditional after `{`, a comment between `#if` and `#region` after `{`, a file-scoped namespace followed by `#if`, a switch-section label, and a file-start header comment with a blank line written. Each was formatted twice on the head (second pass: 0 files changed) and then run through `scripts/apply-fix.sh` for RH5020–RH5032 under LF and CRLF: every run gave `no-diagnostic`. The same fixtures were also formatted on `main` (built from `99ae644`). The only differences between the two outputs, in 11 of 17 fixtures, are single blank lines between a conditional directive directly after `{` and the region directive below it, which is the change the maintainer's answer to Q1 decided. Nothing else differs.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| SHARED-BLANK-005, blank line above an active region directive (formatter) | `BlankLineEditor.IsMissingRequiredBlankLineBeforeRegionDirective`: only the one line directly above the directive. If the directive's line starts the trivia list, that line is the preceding token's line. If the trivia in front is a directive, `///`, or disabled text, that trivia's line is the one above. If the trivia in front is an EOL, the method skips whitespace back to the next trivia (`EndsLine`, blank) or to the start of the list (blank only when the list starts a line) | Core `RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore(SourceText, int, ISet<int>)`: one text line above the directive, `IsBlankLine`, and `GetLineIndicesEndingWithToken(OpenBraceToken)` | Both feed the one decision `IsMissingRequiredBlankLineBefore(bool, bool, bool)`. I traced "line above ends with `{`" for a brace with trailing whitespace (both exempt), a `//` or `/* */` comment behind the brace (both not exempt: `HasTrailingComment` against text after the brace), a multi-line comment that starts behind the brace and ends on the next line (both not exempt), and the `{` of an initializer, anonymous object, switch expression, or accessor list (both use `OpenBraceToken`; probes are clean). I traced "line above is blank" for an EOL behind a `///` or a directive (both blank), the end of a multi-line comment or a literal line (both not blank), and a blank first line of the file (both blank). A switch-label colon is not exempt on either side | agree |
| SHARED-BLANK-005, first line of the file | Formatter: `hasTokenBefore == false` with the directive's line starting the list; `PrecedingTokenFacts.Resolve` gives a node-level root its document's preceding token | Core: `directiveLineIndex > 0` | Agree for whole documents and for `FormatNodeInDocument*`. A detached `FormatNode` root without `RootPrecedingToken` counts as a file start, as on `main` (FC-028 dismissed below) | agree |
| SHARED-BLANK-005, which directives are decided | Formatter: every directive of the kind in the leading trivia, visited from last to first (`EnsureBlankLineBeforeRegionDirectives`) | Analyzer: every active region directive in the tree | Agree. The first-of-kind restriction is gone. An insertion lands only at its own directive's line start, which is below every directive still to be visited | agree |
| SHARED-BLANK-007, inactive directives | Formatter: `SyntaxTriviaUtilities.IsInactiveDirective` skip in both loops | Analyzer: `IsActive: true` filter | Agree. The test `VerifyFormatterLeavesRegionInInactiveConditionalAfterBraceUnchanged` and fixture p07 (both branches of `#if`/`#else`) pin it | agree |
| Trailing-trivia `#endregion` (formatter only) | `EnsureBlankLineBeforeEndRegionsInTrailingTrivia`, with the owning token as the token in front of the list | Same Core predicate | No caller reaches it (instrumented run above). See *Hints*, 1 | not a finding |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| SHARED-BLANK-005 decision | `rg -n "IsMissingRequiredBlankLineBefore" src --glob '!*Test*'`: Core `RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore(bool, bool, bool)` is the one decision. Its text overload delegates to it and is called by `RegionDirectiveBlankLineAnalyzerBase` and `RegionDirectiveBlankLineCodeFixProviderBase`. Formatter: `BlankLineEditor.IsMissingRequiredBlankLineBeforeRegionDirective` calls it and is called by `EnsureBlankLineBeforeRegionDirectives` (both rewriters) and `EnsureBlankLineBeforeEndRegionsInTrailingTrivia`. `rg -n "IsExemptFromPrecedingBlankLineBeforeRegionDirective\|HasBlankLineImmediatelyBeforeIndex\|EnsureBlankLineBeforeFirstDirective\|EnsureBlankLineBeforeFirstEndRegion" src sdd/specs documentation AGENTS.md`: nothing. Other region references in `BlankLines/` (`BlankLineTokenCleanupRewriter` lines 229, 610, 621; `BlankLineEditor.HasBlankLineBeforeIndex`) decide FMT-BLANK-009/FMT-BLANK-013 removals and SHARED-BLANK-006, not this rule | one owner of the decision. Two fact readers remain because the inputs differ (text against trivia mid-rewrite); the guard-scope row shows that they agree. Not a finding |
| SHARED-BLANK-006 (blank line after) | `RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineAfter` and `BlankLineRegionDirectiveRewriter.EnsureBlankLineAfterRegionDirectives` | outside the run's change. Lead follow-up candidate 2 already records it |
| SHARED-BLANK-007 skip | Analyzer `IsActive` filter, `BlankLineEditor.EnsureBlankLineBeforeRegionDirectives`, `BlankLineTriviaBoundaryRewriter.EnsureBlankLineBeforeEndRegionsInTrailingTrivia` | pre-existing, with the same predicate on each side. Not a finding |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RegionDirectiveLineAboveTests` (17 `AssertRuleResult` tests: f3, f7, f5, g5, g4, f6, g6, g1, g2, k4, first line, h1, h2, h3, h4, k1, k2) | Exact output, with the blank line above every active region directive per SHARED-BLANK-005 | Any byte of output differs, a second pass changes it, or one phase alone changes it, under LF or CRLF | `AssertRuleResult`: exact text, second pass, `PhaseStabilityCheck`, both line endings | adequate. Each failed on 8ac4176 by the one missing line (02-test-implementer.md) |
| `RegionDirectiveLineAboveTests.RegionBelowIfDirectiveAfterSwitchBlockBraceGetsBlankLine`, `…AfterAccessorListBraceGetsBlankLine` (g7, g8) | No blank line above `#if`, a blank line above `#region A`, and a stable result | The blank-line state differs, or a second pass or a lone phase changes the output | `AssertBlankLinesAbove` (blank-line state plus `AssertSecondPassIsStable`) | adequate. These fixtures carry layout the run does not decide: in g8 the accessor is rewritten to an expression body (fixture p11 shows it), and region columns in a switch section are outside the run. The blank-line state is the whole invariant of FMT-BLANK-013 here |
| `DisabledRegionLayoutTests.RegionInsideActiveConditionalIsStillFormatted` | Exact output with `DEBUG` defined, now with the blank line between `#if DEBUG` and `#region Test` (Q1 A) | Output differs | `AssertRuleResult(…, _withDebug)` | adequate |
| `RH5031…FormatterTests.VerifyFormatterSeparates*` (3) | RH5031 reports the input; the formatter's output equals the expected text, RH5031 does not report it, and a second pass keeps it, under LF and CRLF | A diagnostic on the output, or an output difference | `VerifyFormatter(source, fixed, diagnostics)`: `Verify` input, format, `Verify` output, reformat, `Verify` | adequate. This is the FC-013 check on an active branch (decision 5) |
| `RH5031…FormatterTests.VerifyFormatterLeavesRegionInInactiveConditionalAfterBraceUnchanged` | The issue's literal input (`#if DEBUG`, not taken) is neither reported nor changed | A diagnostic, or an output change | `VerifyFormatter(source)` | adequate |
| `RH5031…AnalyzerTests` (5 new) | RH5031's analyzer and fix behavior is unchanged for the shapes the formatter now handles | A diagnostic count or location changes, or the fixed text differs | `Verify(source, fixed, diagnostics)`, which re-analyzes the fixed source | adequate. Together with the unchanged existing RH5031 tests, they pin the text overload's delegation |

## Visible output is decided

- The changed expected value in `DisabledRegionLayoutTests` and the expected values of f3, f5, g5, g4, f6, g6, g7, g8, and `VerifyFormatterSeparatesActiveConditionalAfterBraceFromRegion` (a conditional directive directly after `{`) are covered by the maintainer's answer to Q1 (A) under `Maintainer answers` in `00-lead.md`.
- g1, g2, k4, and `VerifyFormatterSeparatesHeaderCommentAtFileStartFromRegion` (file start) are covered by the answer to Q2 (A).
- h1, h2, h3, k1, k2, and `VerifyFormatterSeparatesCommentFromSecondRegionInGap` (a region directive later in its gap) are demanded by the unchanged SHARED-BLANK-005 and FMT-BLANK-013, "the line directly above an active `#region` or `#endregion`" (lead decision 7).
- f7, h4, the first-line test, and the inactive guard keep `main`'s output.
- The RH5031 analyzer expectations are demanded by the unchanged RH5031-001.
- No rule page changed.
- The main-against-head probe shows no visible change outside Q1 A.

## Failure classes dismissed

- FC-001, FC-002: covered under *Guard scope*. The qualifiers "directly above", "first line", "followed by nothing but whitespace", and "active" are each explicit conditions.
- FC-003: both sides of each boundary are tested: f7 against f3, the first line against g1/g2/k4, h4 against h3, and the existing `EmptyRegionBelowBraceWithTrailingCommentGetsBlankLine` and `RegionDirectlyAfterOpeningBraceTests` for the brace.
- FC-004, FC-005, FC-021: the change only inserts an `EndOfLineTrivia` at a directive's line start. It moves, joins, or deletes nothing, and no comment changes its line.
- FC-006, FC-022 to FC-027, FC-029, FC-036: the change rewrites no code, literal, identifier, or name. For FC-027, the probes covered every enclosing `{` scope I could construct.
- FC-007, FC-008, FC-009, FC-018, FC-030: RH5031's code fix is unchanged and text-based, and its existing Fix All scenario is untouched. The analyzer suite (6411) passed on the head through the instrumented build.
- FC-010, FC-011, FC-012: every formatter test runs `PhaseStabilityCheck`. Every probe was stable on the second pass, with RH5020–RH5032 clean on LF and CRLF. The insertion loop has no loop-until-stable. The documentation-comment interplay (d1) is unchanged and is lead follow-up 1.
- FC-013: RH5031 and RH5032 report none of the formatter outputs (tests and probes).
- FC-014: no new rule.
- FC-015, FC-016: see *Policy ownership*. The sibling shapes are the skeptic's matrix plus the 17 probes.
- FC-017: see *Assertion adequacy*.
- FC-019, FC-020: the matrix carries comments, `#pragma`, `#nullable`, all four conditional directives, inactive branches, and nested `#if`.
- FC-028: the facts come from `PrecedingTokenFacts.Resolve`, which falls back to `RootPrecedingToken` for a node-level root. A detached root without one counts as a file start, as on `main`. The change only adds insertions there (behind a comment or directive in the root's leading trivia), where SHARED-BLANK-005 asks for them in the document too.
- FC-031: the summaries of `IsMissingRequiredBlankLineBefore` (both overloads), `IsMissingRequiredBlankLineBeforeRegionDirective`, `EnsureBlankLineBeforeRegionDirectives`, and both inline comments describe the new code, and no summary or spec text names a removed symbol. One remark on the unreached trailing path overclaims (*Hints*, 1).
- FC-032: I re-ran the analyzer, formatter, and CLI suites after the compiled change, on the head plus instrumentation.
- FC-033, FC-034: the reproduction uses an active branch (the issue's literal input is inactive and kept as a guard), runs on LF and CRLF, and goes through the full pipeline and the RH5031 re-analysis.
- FC-035: the expected values were fixed on 8ac4176, before the implementation.
- FC-037: no text-only claim is made.

## Architecture findings

_None._ The skeptic's finding (SHARED-BLANK-005 stated in Core and again in the formatter) is resolved: one decision in Core and two fact readers that agree (*Guard scope*). The FMT-BLANK-010 against FMT-BLANK-013 order dependency is unchanged and is lead follow-up 1.

## Hints

1. **The trailing-trivia `#endregion` path is unreached, and its remark overclaims.** In the instrumented run (formatter 4402, analyzer 6411, CLI 251, and the CLI over the 1843 repository files), no token reached `BlankLineTriviaBoundaryRewriter.VisitToken` with an `#endregion` in its trailing trivia. The remark on `EnsureBlankLineBeforeEndRegionsInTrailingTrivia` says that "A directive that does not yet start its own line gets the line break that starts it together with the blank line". That does not hold when the token is `{` without a comment and the directive is its first trailing trivia: `lineStartIndex == 0` reaches `IsMissingRequiredBlankLineBefore(true, false, true)`, which returns `false`, so nothing is inserted and `{ #endregion` would stay on one line. On `main` the same input was decided from the token before the `{`. Because no caller reaches it, this is not a finding (*Supported inputs*). This evidence answers the check that the implementer said deleting the path would need (03-implementer.md, *Disputed tests or scope*), in case the lead wants to revisit decision 10.
2. **Duplicated line-start walk.** `EnsureBlankLineBeforeEndRegionsInTrailingTrivia` repeats the whitespace back-walk of the existing private `FindLineStartIndex` in the same class (`BlankLineTriviaBoundaryRewriter.cs`), and `BlankLineEditor.EnsureBlankLineBeforeRegionDirectives` repeats it as well. `main` already had both inline loops. Low value, pre-existing.
3. **Region directive columns inside expression braces (uncertain, pre-existing).** On both `main` and the head, a region directive inside a collection initializer, anonymous object, or switch expression lands at the enclosing statement's indentation (column 8) while the elements stand at column 20. In a switch section, the `#endregion` stands four columns left of its `#region` (fixture p13). The change does not touch it (identical columns on `main`). I did not check whether an indentation requirement decides these columns.
4. `RegionDirectiveBlankLineUtilities.IsAdjacentToNonFormattableLine` has no production caller (`rg` outside tests finds only its declaration). Pre-existing and unrelated.
