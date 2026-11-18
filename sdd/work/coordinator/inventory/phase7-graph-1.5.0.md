# Phase 7 · 1.5.0 run graph (milestone 7)

## Update 2026-11-18

Base: `claude/sdd-coordinator` a17ce501 (contains `origin/main` e91afdf7). No open pull requests. Open issues of milestone 7: #352 #849 #850 #851 #892 #893 #894 #895 #896 #897 #1277 #1539 #1803 #1865. Premises re-checked by reading code on this base (no SDK, nothing run).

### Merged since 2026-11-17

| PR | Run | Issues |
|---|---|---|
| #1856 | R1 | #1851 (FMT-UNSUP-013) |
| #1857 | R3 | #889 #1476 |
| #1858 | R2 | #1640 #1651 |
| #1859 | R4 | #1820 (`scripts/benchmark.sh`; report on #1820; follow-up #1862) |
| #1860 | R5 | #353 #354 (RH2008, RH2111) |

Closed without a run: #1439, #1515. Maintainer decision: #894 declares the operator-trivia shape unsupported in its own change, no new rule, after #895. #1865 goes into 1.5.0 (maintainer).

**Correction to the registry set.** #1860 also edited `Debug.ruleset` and `src/Reihitsu.Analyzer.Package/README.MD` (it has a rule list). Registry files of a new-rule run are therefore: `AnalyzerResources.resx`/`.cs`, `CodeFixResources.resx`/`.cs` (with a fix), `Debug.ruleset`, `Release.ruleset`, `sdd/specs/README.md`, `src/Reihitsu.Analyzer.Package/README.MD`.

### Triage of the issues without milestone (proposal; #1865 decided)

| # | Group | Premise (by reading) | Reachability | Proposed milestone | Reason | Lane / files (1.5.0 only) |
|---|---|---|---|---|---|---|
| #1861 FluentChainCodeFixHelper doc comments | Maintenance | Holds: `FluentChainCodeFixHelper.cs:53`, `:72` still say "positional context" | none needed (comments only, text-only) | **1.5.0** | Small follow-up of the 1.5.0 run #1857, triggers nothing | docs · `src/Reihitsu.Analyzer.CodeFixes/Core/FluentChainCodeFixHelper.cs` |
| #1862 whole-tree `ReplaceTokens` quadratic | Maintenance (performance, output unchanged) | Holds: `IndentationRewriter.cs:39`, `CleanupPhase.cs:33` replace every token; `TabReplacementPhase.cs:151`; measured in #1820's report | CLI/whole document, every file | **1.5.0** | The #1820 benchmark's only hot spot; #1803 re-measures after it (#1803 comment 2026-11-17) | bug fix · `Pipeline/Indentation/Utilities/IndentationRewriter.cs`, `Pipeline/Cleanup/CleanupPhase.cs`, `Pipeline/TabReplacement/TabReplacementPhase.cs`, `Pipeline/LineEndings/LineEndingNormalizationPhase.cs`, their unit tests under `Formatter.Test/Unit/{Indentation,Cleanup,TabReplacement,LineEndings}`; no spec, no `AGENTS.md` (phase-table guard names `CleanWhitespaceBeforeEndOfLine`… only if renamed) |
| #1863 accessor `=>` on its own line | Extension, OQ | Plausible: no line-break rewriter joins an accessor's arrow; FMT-BREAK-026 covers properties/indexers only. Issue says observed | CLI/whole document | 1.7.0 | T11 unowned joins (OQ batch with #1648 lambda arrow, #1653); not a defect of #1851's shape, which the change only found it next to | — |
| #1864 SHARED-COMMENT-012 token behind `*/` | Minor bug, OQ (FMT-BREAK-037) | Plausible: only `ChainLineBreakRewriter.IsBehindMultiLineCommentTail` has the branch; `EndLineInFrontOf` none | CLI; RH5302/RH5308 fixes make no progress, RH5203 no fix | 1.9.0 | Must be settled with #1572 (1.9.0, T10); no code damaged; touches `LineBreakTriviaUtilities.cs`, `ChainLineBreakRewriter.cs` and every list/operator rewriter | — |
| #1865 RH2008/RH2111 for this repository | Maintenance | Holds: both `Action="None"` (`Debug.ruleset:98-99`, `Release.ruleset:113,124`); no `CompilerVisibleProperty` in `Directory.Build.props`; no `reihitsu.json` in the repository (and none fed as `AdditionalFiles`). **New fact:** `binaries/Reihitsu.Analyzer.dll` is the 1.4.0 build (refreshed only by version bumps) and holds no RH2008/RH2111, so the build cannot report them before the 1.5.0 bump; only the self-hosting scans can verify the limits now | repository build | **1.5.0** (decided) | — | tooling · `Debug.ruleset`, `Release.ruleset`, `Directory.Build.props` (or new `Directory.Build.targets`: `CompilerVisibleProperty`, `AdditionalFiles` for `reihitsu.json`), new `reihitsu.json`, `src/Reihitsu.SelfHosting.Test/Analyzers/AnalyzerSelfHostingTests.cs`, `AnalyzerViolationReportTests.cs`, `Utilities/SelfHostingProject.cs`/`SelfHostingSolution.cs` (global options, config file). Only if limits are chosen, not refactors; refactoring the 19 methods / 5 types would touch formatter and analyzer sources across every other run |
| #1866 `RH08402` in Debug.ruleset | Maintenance | Holds: `Debug.ruleset:101` `RH08402`; `Release.ruleset:431` has `RH8402` None | repository build | **1.5.0, joined to #1865** | Same files and lane as #1865, one line, triggers nothing | (in `/squad #1865 #1866`) |
| #1867 RH0001 duplicate key | Extension (if A), OQ | Plausible: `ConfigurationManager` reads with a streaming `Utf8JsonReader`; each occurrence assigns, the last wins, nothing reports. The issue's "A needs a reader that sees duplicates" is wrong: the streaming reader sees every property, a per-section seen-set is a cheap check | analyzer, any `reihitsu.json` | 1.7.0 | General RH0001 gap, not specific to the 1.5.0 `design` section; T7 small widenings | — |

Duplicates: none. Related, not duplicates: #1862 ↔ #1803 (#1803 keeps the spine-depth part); #1864 ↔ #1572 (opposite direction, settle together); #1863 ↔ #1648.

### Re-verified runs

- **#1277** — unchanged: RH5401 `CanCollapse` still rejects any `\n` in `FormatNode(property)` (attribute line included); RH5307 fix unchanged. Files: `RH5307IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider.cs`, `RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider.cs`, `sdd/specs/analyzer/RH5307.md`, `RH5401.md`, both analyzer test files. Page goal dropped (rule-page convention).
- **#1539 — close, no run.** After #1856 the converter's own-line split (`TrySplitOwnLineComments`) and arrow carrier are gone; FMT-UNSUP-012/013 keep both arrow gaps free of comments. What is left is `ExtractSignificantTrivia` on the semicolon's leading trivia, which stays in front of the same `;` and crosses no inserted brace, so `BlockWrapTriviaUtilities` (carrying across inserted braces) does not apply; #1856's change doc (*Special cases*) already records keeping it as serving trivia outside the gap. Done-when "a recorded decision says why" is met by that change doc.
- **#849 #850** — ready (R1, R5 merged). Files as §1 plus `Debug.ruleset` and the package README; `GetOnlyMemberExpressionBodyTransform.HasLayoutOnlySeams` (changed by #1856), `AccessorExpressionBodyTransform.HasLayoutOnlySeams`, `ReihitsuFormatter.CodeFixDisabledStructuralTransforms` still gate the fix. New IDs after RH3208. Must not edit `AGENTS.md` in this batch (owned by #895); if the shared predicate moves and the `StructuralTransformPhase` row has to change, that edit waits until #895 has merged.
- **#895** — ready (R1, R2, R3 merged). After #1858, `ChainLineBreakRewriter.JoinConditionalElementAccesses` still joins only `?[` in chains without links; #1813 covers `(`. Remaining: element access `arr⏎[0]` and trailing `!` outside chains. Files: `ChainLineBreakRewriter.cs`, `line-breaks.md` (FMT-BREAK-020), `AGENTS.md` (`LineBreakPhase` row), `Formatter.Test/Regression/LineBreaks/ChainGapJoinTests.cs` or a new regression class, `ChainLinkLayoutTests.cs`, change doc; possibly RH5201/RH5112 test expectations.
- **#1803** — not ready: its 2026-11-17 comment says re-measure after #1862 and attribute only what remains. #1820's profile names no file beyond #1862's two call sites; the spine-depth inputs (chain, `?:`, concatenation) were not profiled. Start after #1862 and #895 (`ChainLineBreakRewriter` is a suspect); re-derive files from a trace at start, and if they name `FluentChain`/`MethodChainAlignmentContributor`/`LayoutComputer`, also after #896.
- **#1865 #1866** — ready, but `Debug.ruleset`/`Release.ruleset` are registry files shared with #849 #850. Needs the maintainer's limits-vs-refactor choice before it starts (issue item 1).

### Start now (pairwise no shared file, no dependency)

`/squad #849 #850` · `/squad #895` · `/squad #1277` · `/squad #1862` · `/squad #1861`

Pairwise: registry files only in #849 #850; `AGENTS.md` only in #895; #895 line-break files vs #1862 indentation/cleanup/tab/line-ending files; #1277 RH5307/RH5401 fix files vs #849 #850 (no RH5401 edit) and #895 (RH5201/RH5112 tests only); #1861 one code-fix comment file. Only `requirement-ids.txt` may be shared.

### After X merges, start Y (replaces §4 for the remaining runs)

| When merged | Start | Reason |
|---|---|---|
| #849 #850 (+ maintainer's limits choice) | `/squad #1865 #1866` | `Debug.ruleset`, `Release.ruleset` (registry) |
| #1277 + #849 #850 + #1865 | `/squad #851` | #1277: RH5401's final guard (logical); registry files |
| #1862 + #895 | `/squad #1803` | logical (#1803 re-measures after #1862) and `IndentationRewriter.cs`/`CleanupPhase.cs`; `ChainLineBreakRewriter.cs`; also after #896 if the trace names chain anchor or `LayoutComputer` files |
| #895 | `/squad #894` (declare unsupported, decided) | `ChainLineBreakRewriter.cs`, `AGENTS.md`, `line-breaks.md`; precheck files. Unsupported-shape run first when it collides with #896 |
| #894 | `/squad #896` | `MethodChainAlignmentContributor.cs` (`AlignRestOfRun` guards #894 deletes), `indentation.md`, `ChainLinkLayoutTests.cs` |
| #895 + #851 | `/squad #892` | #895 decides the scope (P11); registry files |
| #892 + #896 + #894 | `/squad #893` | `FluentChain` use, RH5201 files and anchor; #894 makes case 2 moot (narrow to case 1) |
| #892 | `/squad #897` | registry files (`AnalyzerResources.resx`, `CodeFixResources.resx`) |
| #897 | `/squad #352` | registry chain end; also `ConfigurationManager.cs`/`ConfigurationCategoryDesign.cs` for its limit option |

Registry chain: #849 #850 → #1865 #1866 → #851 → #892 → #897 → #352 (recommended order; whichever is otherwise ready first may take the next slot). Critical paths: #895 → #894 → #896 → #893; #1862 → #1803.

### Needs the maintainer

1. Confirm the triage proposal (#1861, #1862, #1866 → 1.5.0; #1863 → 1.7.0; #1864 → 1.9.0; #1867 → 1.7.0).
2. #1865: limits only (recommended; refactors as later issues) or refactor the production outliers; and accept that the build cannot report RH2008/RH2111 before the 1.5.0 bump refreshes `binaries/`.
3. #1539: close (no run).

---

Date 2026-11-17. Base: `claude/sdd-coordinator` 1fb77e0b, which contains `origin/main` **074661f9** (last merges #1854, #1853, #1839 FMT-UNSUP-012, #1849, #1840). Open PRs: only #1855 (version bump to 1.4.0, `Directory.Build.props`; touches no file of this plan).

Method: open issues of milestone 7 fetched from the REST API (`issues?milestone=7&state=open&per_page=100`), each body and all comments read (`issues/<n>/comments`). Premises checked by reading code on main (no SDK, nothing run, no trace). File sets derived from the issues, the code they name, and the file list of the comparable merged change (#1839 for #1851). Model: `phase6-graph-1.4.0.md`. Rules kept: one lane per run; runs open at the same time share no file except `sdd/specs/requirement-ids.txt`; code fixes that delegate layout follow formatter runs that change their expected output; unsupported-shape runs (T3 kind) first when two ready runs collide.

**Membership:** the 23 issues listed in the restart prompt are exactly the 23 open issues of milestone 7. No difference.

**Shared registry files of every new-rule run** (counted as shared): `src/Reihitsu.Analyzer/AnalyzerResources.resx` + `AnalyzerResources.cs`, `src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx` + `CodeFixResources.cs` (when the rule has a fix), `Release.ruleset`, `sdd/specs/README.md` (spec index). There is no README rule table and no localized `.resx`; the issues that ask for a "README row" are stale on that point.

## 1. Per issue

AF = labeled `agent-filed` (all 23 are). OQ = output question for the maintainer expected.

| # | Premise on main | Reachability | Lane | Real file set | OQ | Questionable goals (AF) |
|---|---|---|---|---|---|---|
| #892 new rule: line break in front of an attached part of a chain | Holds: no analyzer uses `FluentChain.AttachedParts`; RH5201 ignores attached parts (only `ChainLineBreakRewriter`, `MethodChainAlignmentContributor` read them) | Analyzer (new diagnostic); fix via `FluentChainCodeFixHelper.FormatChainAsync` on the chain's line-starting statement | behavior change | new `Rules/Layout/RH5xxx…Analyzer.cs` + fix, `sdd/specs/analyzer/RH5xxx.md`, `documentation/rules/RH5xxx.md`, new test class, registry files; possibly `Core/FluentChain.cs` (a "starts line" fact for attached parts) | yes, small (ID/title; scope outside chains depends on #895) | "Do not report chains without a member access except `?[`" mirrors today's formatter scope, which #895 changes; re-derive scope after #895 (P11: rule need not follow formatter beyond its construct) |
| #893 extend RH5201 (first call after kept prefix wrap; columns of trivia-kept pieces) | Case 1 plausible (`ApplyWrappedLayout` moves it, RH5201 does not check); case 2 plausible; issue itself says 0 occurrences in the repo | Analyzer + RH5201 fix | behavior change | `Rules/Layout/RH5201…Analyzer.cs`, `sdd/specs/analyzer/RH5201.md`, `RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs`, `Analyzer.Test/Formatter/Formatting/RH5201FluentChainLinkFormatterTests.cs`, maybe `Core/FluentChain.cs` | yes (new reports of RH5201) | Case 2 disappears if #894 declares the operator-trivia shape unsupported; case 1 interacts with #896's anchor change. "Rare in practice" by its own words |
| #894 trivia inside `?.`/`!.`/`!?.`/`?[` | Holds: no rule reports a directive/disabled text there; formatter keeps the gap (`ChainLineBreakRewriter.CloseGap`/`CloseInnerGaps`, `MethodChainAlignmentContributor.AlignRestOfRun`) | CLI/whole document (formatter keeps the split); analyzer for the rule variant | behavior change | **Declare variant:** `Precheck/PrecheckPhase.cs`, `unsupported-syntax.md` (FMT-UNSUP-0xx), `Formatter.Test/Helpers/FormatterTestsBase.cs` (reason constant), `Unit/Precheck/` new tests, guard deletions in `ChainLineBreakRewriter.cs`/`MethodChainAlignmentContributor.cs`, `line-breaks.md`/`indentation.md` as found, `CliHiddenCodeEndToEndTests.cs`, `AGENTS.md` (`PrecheckPhase` row), change doc. **Rule variant:** new analyzer, spec, page, tests, registry files | **yes, before the run** — declare unsupported vs. new rule (maintainer's output decision, themes: "escalated, not decided") | Proposes a new rule; AGENTS.md requires the declare option to be named and asked |
| #895 join attached parts outside chains | Partly fixed: #1813 joins an argument list's `(` outside chains (`ArgumentListLineStartRewriter`); open per the 2026-11-17 status comment: element access `arr⏎[0]` (and, to check, trailing `!`) | CLI/whole document; RH5201/RH5112 fixes format chains | behavior change | `ChainLineBreakRewriter.cs` (`JoinConditionalElementAccesses`, `NormalizeChain`) or a sibling of `ArgumentListLineStartRewriter`, `line-breaks.md` (FMT-BREAK-011/012/020), `ChainLinkLayoutTests.cs` + new regression tests, `AGENTS.md` (`LineBreakPhase` row, as #1813 did), possibly RH5201/RH5112 analyzer test expectations, change doc | maybe (trailing `!`, column of a trivia-kept part); the join itself is decided (round 9 comment) | Goal narrowed by #1813; keep only element access (+ `!` if it reproduces) |
| #896 anchor a wrapped first call under the last prefix link | Holds: `FluentChain.GetAnchorLink` returns `FirstLink` when no invoked link precedes; pinned by `ChainLinkLayoutTests.WrappedCallAfterPropertyPrefixStaysUnderPrefix` and `RH5201FluentChainLinkFormatterTests.VerifyMisalignedCallAfterPropertyPrefixIsReported` | CLI/whole document; RH5201 analyzer + fix | behavior change | `Core/FluentChain.cs`, `MethodChainAlignmentContributor.cs` (if it needs more than the anchor), `indentation.md` (chain anchor requirement) + `sdd/specs/analyzer/RH5201.md`, `ChainLinkLayoutTests.cs`, `RH5201FluentChainLinkFormatterTests.cs`, `RH5201…AnalyzerTests.cs`, change doc | **yes** (anchor; call-less chains `order.Customer.Address⏎.City`) | Whole change is a proposal; maintainer may keep today's anchor |
| #897 RH5112/RH5048 titles say "first member access" | Holds: `RH5112Title`/`RH5048Title` and page headings still say "call" | Analyzer (diagnostic title/message) | behavior change (visible text; change doc) | `AnalyzerResources.resx`, `CodeFixResources.resx` (if the fix title repeats it), `documentation/rules/RH5112.md`, `RH5048.md`, `sdd/specs/analyzer/RH5112.md`, `RH5048.md` (titles) | yes (wording) | "localized resources" do not exist |
| #889 tests: mid-line statement side of the chain fixes' context selection | Holds: `FluentChainCodeFixHelper` still picks the ancestor whose first token `IsFirstOnLine` (:80); no RH5201/RH5112 test starts a chain mid-line | RH5201/RH5112 code fixes | tests | `RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs`, `RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests.cs` | no | — |
| #1476 tests: RH5201 on a comment-led chain anchor | Holds: no RH5201 test has `/* c */ s.Trim()⏎.ToUpper()` | RH5201 analyzer + fix | tests | `RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs` | no (a disagreement becomes a follow-up) | — |
| #1640 chain gap joins through `JoinGaps` | Holds: `ChainLineBreakRewriter.CloseGap`/`GetPendingToken` (:94, :206) keep a third copy | CLI/whole document (refactor, output unchanged) | bug fix | `ChainLineBreakRewriter.cs`, `LineBreaks/Utilities/LineBreakTriviaUtilities.cs` | no | — |
| #1651 one "line break in front of a moved token" helper | Holds: `ChainLineBreakRewriter.EnsureLineBreak`/`HasLineBreak`, private checks in `LineBreakListRewriter`, `GenericConstraintLineBreakRewriter`; shared `EndLineInFrontOf` used by binary/ternary only | CLI/whole document (refactor) | bug fix | `LineBreakTriviaUtilities.cs`, `ChainLineBreakRewriter.cs`, `LineBreakListRewriter.cs`, `GenericConstraintLineBreakRewriter.cs`, `Formatter.Test/Unit/LineBreaks/LineBreakTriviaUtilitiesTests.cs` | no | — |
| #849 new rule: get-only properties should use an expression body | Holds: formatter lifts (`GetOnlyMemberExpressionBodyTransform`), no analyzer reports it | Analyzer; **fix:** the node-level entry points skip `AccessorExpressionBody` for code fixes (`ReihitsuFormatter.CodeFixDisabledStructuralTransforms`, which also gates the get-only lift), so the fix cannot simply delegate | behavior change | new `Rules/Clarity/RH32xx` analyzer (+ fix if comprehensive), the lift predicate shared with the formatter (moved to `Reihitsu.Core` or a formatter helper the fix can call: `GetOnlyMemberExpressionBodyTransform.cs`, `AccessorExpressionBodyTransform.cs` `HasLayoutOnlySeams`), maybe `ReihitsuFormatter.cs`, `structural-transforms.md`, `shared/expression-body-conversion.md`, specs, pages, tests, registry files | likely (fix offered or not; ID/title) | "rule doc records [withheld fixes]" contradicts the rule-page convention; "README row" and "coordinate with #628" stale |
| #850 new rule: get-only indexers should use an expression body | Holds, as #849 | as #849 | behavior change | as #849 (indexer) | likely, with #849 | as #849 |
| #851 new rule: expression-style get-only indexers single lined (RH5401 counterpart) | Holds: RH5401 registers `PropertyDeclaration` only; `PropertyLayoutLineBreakRewriter.VisitIndexerDeclaration` collapses indexer arrows | Analyzer; fix via `FormatNode` (as RH5401) | behavior change | new `Rules/Layout/RH5419…` analyzer + fix (RH5418 is taken by empty unions), spec, page, tests, registry files | probably no (mirrors RH5401) | "next free ID RH5418" stale; "mirror RH5401's comment/directive guards" is now FMT-UNSUP-012 (+ #1851) plus SHARED-FIX-010 |
| #1439 comment after `=>` becomes the block's first line | **Fails:** FMT-UNSUP-012 (#1839) refuses every comment/directive between `=>` and its expression; the converter never meets one (FMT-STRUCT-006 says so) | **No caller** (CLI refuses the file; fixes withheld by SHARED-FIX-010) | — | none | no | Whole issue obsolete |
| #1515 layout of comments at the arrow on conversion | Get/Get2 refused by FMT-UNSUP-012; Get3 (`/* c */ =>` on its own line in front of the arrow) still converted to `/* c */ {` today — exactly #1851's shape | After #1851: **no caller** | (behavior change) | none after #1851 | no | Its done-when item (`PhaseReversalExceptionList`) already dropped (comment) |
| #1539 converter takes comment trivia from `BlockWrapTriviaUtilities` | Holds today (`TrySplitOwnLineComments`, `ExtractSignificantTrivia` in `ExpressionBodyToBlockConverter`), but #1851 deletes the own-line split and `ArrowLineHoldsComment`; then only the semicolon's trivia is carried | CLI/whole document; RH3202–RH3208 fixes | bug fix | `ExpressionBodyToBlockConverter.cs`, `Core/Utilities/BlockWrapTriviaUtilities.cs` (callers `ControlFlowBraceTransform.cs`, `SwitchCaseBraceRewriter.cs` only if the API changes), structural tests | no | Likely ends as a recorded decision (one comment) or closes |
| #1277 RH5307/RH5401 fix guards | Holds by reading: RH5307 fix resolves the list from the token at the diagnostic start upward (outer list for `x[m[1,⏎2]]`); RH5401 `CanCollapse` rejects any newline in the formatted property, attribute line included; RH5401-003/004 say "for a property without attribute lists" | RH5307 and RH5401 code fixes | bug fix | `RH5307…CodeFixProvider.cs`, `RH5401…CodeFixProvider.cs`, `sdd/specs/analyzer/RH5307.md`, `RH5401.md`, both analyzer test files | no | "rule page carries the `?[ ]` clarification" contradicts the rule-page convention (spec only) |
| #1851 declare comment/directive between `=>` and the code in front of it unsupported | Holds: converter and `PropertyLayoutLineBreakRewriter.CollapseExpressionBody` still handle it; #1839's change doc names it as this follow-up (escalation 10, answer A) | CLI/whole document; every expression-body code fix; RH5401 | behavior change | `PrecheckPhase.cs`, `unsupported-syntax.md` (FMT-UNSUP-013), `FormatterTestsBase.cs`, `Unit/Precheck/` new test class, `ExpressionBodyToBlockConverter.cs`, `PropertyLayoutLineBreakRewriter.cs`, `GetOnlyMemberExpressionBodyTransform.cs` (`HasLayoutOnlySeams`), possibly `AccessorExpressionBodyTransform.cs`, `structural-transforms.md` (FMT-STRUCT-006), `line-breaks.md` (FMT-BREAK-026), `shared/expression-body-conversion.md`, RH5401 spec/analyzer/fix/tests, RH3202–RH3208 test files, Formatter `Structural/` tests, `CliHiddenCodeEndToEndTests.cs`, `AGENTS.md` (`PrecheckPhase` row), change doc | declaration decided; **one remains:** RH5401 stops reporting `int B // note⏎=> 1;` vs. reports it without a fix (issue: "decided in the change") | — |
| #352 cognitive-complexity rule | Holds (no such rule) | Analyzer | behavior change | new `Rules/Design/RH20xx`, spec, page, tests, registry files; uses the option reading #353/#354 introduce | yes (metric definition, default limit, members covered) | L; lambdas as "method-shaped"; option key shape is a new repository surface (no analyzer reads `AnalyzerConfigOptions` today) |
| #353 method line-count rule | Holds | Analyzer | behavior change | new `Rules/Design/` analyzer, first analyzer option reading (new helper in `Reihitsu.Analyzer/Core` or `Base`), spec(s) incl. a shared line-count definition, pages, tests, registry files | yes (counting definition, default limit, option key) | as #352 |
| #354 type line-count rule | Holds | Analyzer | behavior change | as #353 (same definition and option helper) | yes, with #353 | per-partial-declaration counting is a proposal |
| #1803 phases super-linear on long chains/lists | Holds by its measurements (precheck comma part fixed in #1840; remaining cost after the precheck) | CLI / whole document | bug fix | unknown until #1820's profile; suspects `ChainLineBreakRewriter`, `TernaryLineBreakRewriter`, `BinaryOperatorLineBreakRewriter`, list rewriters, `LayoutComputer`/`IndentationRewriter` | no (output unchanged) | — |
| #1820 benchmark 1.2.0 vs 1.3.0 + per-phase profile | Holds (not run) | CLI | tooling | report on the issue; optionally `scripts/benchmark.sh` + `scripts/README.md`; needs NuGet for the released tools | no | A run with no kept script has no diff to publish (see §5) |

## 2. Runs

| Run | Command | Lane | Mechanism | Files | OQ |
|---|---|---|---|---|---|
| R1 | `/squad #1851` | behavior change | precheck declaration, deletes arrow-front special cases | see #1851 row | RH5401 report option |
| R2 | `/squad #1640 #1651` | bug fix | line-break phase one-owner refactors (join loop, line-break-in-front helper), output unchanged | `ChainLineBreakRewriter.cs`, `LineBreakTriviaUtilities.cs`, `LineBreakListRewriter.cs`, `GenericConstraintLineBreakRewriter.cs`, `LineBreakTriviaUtilitiesTests.cs`. No spec edit expected (if `line-breaks.md` is needed, wait for R1) | no |
| R3 | `/squad #889 #1476` | tests | RH5201/RH5112 code-fix and anchor characterization | `RH5201…AnalyzerTests.cs`, `RH5112…AnalyzerTests.cs` | no |
| R4 | `/squad #1820` | tooling | benchmark + profile | `scripts/benchmark.sh`, `scripts/README.md` (only if kept) | no |
| R5 | `/squad #353 #354` | behavior change | size-metric rules, line-count definition, first analyzer option | `Rules/Design/` (2 analyzers), option helper, specs, pages, tests, registry files | yes |
| R6 | `/squad #1277` | bug fix | fix guards of RH5307 and RH5401 | RH5307/RH5401 fixes, specs, tests | no |
| R7 | `/squad #1539` | bug fix | expression-body converter's trivia carrier | `ExpressionBodyToBlockConverter.cs`, `BlockWrapTriviaUtilities.cs` | no |
| R8 | `/squad #849 #850` | behavior change | get-only lift predicate shared by formatter and two rules | see #849 row | likely |
| R9 | `/squad #851` | behavior change | RH5401 counterpart for indexers | new RH5419 files, registry files | probably no |
| R10 | `/squad #895` | behavior change | attached-part join outside chains (element access, `!`) | see #895 row | maybe |
| R11 | `/squad #1803` | bug fix | phase cost linear in spine/list length | from #1820's report | no |
| R12 | `/squad #896` | behavior change | chain anchor selection | see #896 row | yes |
| R13 | `/squad #894` | behavior change | operator-trivia shape: declare or rule (maintainer first) | see #894 row | yes, before start |
| R14 | `/squad #892` | behavior change | new attached-part rule on `FluentChain` | see #892 row | small |
| R15 | `/squad #893` | behavior change | RH5201 extension | see #893 row | yes |
| R16 | `/squad #897` | behavior change | RH5112/RH5048 wording | see #897 row | yes |
| R17 | `/squad #352` | behavior change | cognitive-complexity rule | see #352 row | yes |
| — | #1439 | — | no run: premise fails (FMT-UNSUP-012) | — | — |
| — | #1515 | — | no run: no caller after #1851 (or join R1, §5) | — | — |

## 3. Start now (pairwise disjoint, no dependency)

`/squad #1851` · `/squad #1640 #1651` · `/squad #889 #1476` · `/squad #1820` · `/squad #353 #354`

Pairwise check: R1 edits `PropertyLayoutLineBreakRewriter.cs` and `line-breaks.md`, R2 neither (and no spec); R1's RH5401/RH3202–RH3208 tests vs R3's RH5201/RH5112 tests; R2 formatter only, R3 analyzer tests only; R4 `scripts/` only; R5 is the only one of the five with registry files (R1 adds no rule). None touches #1855's `Directory.Build.props`.

## 4. After X merges, start Y

| When merged | Start | Reason |
|---|---|---|
| R1 (#1851) | `/squad #1277` | RH5401 spec, fix, and tests (RH5401-004's comment clause is deleted by R1) |
| R1 (#1851) | `/squad #1539` (re-check first; may close) | `ExpressionBodyToBlockConverter.cs`; R1 removes the arrow-crossing carrier the issue is about |
| R1 + R5 | `/squad #849 #850` | R1: `GetOnlyMemberExpressionBodyTransform.HasLayoutOnlySeams`, `structural-transforms.md`, `shared/expression-body-conversion.md`; R5: registry files |
| R1 + R2 + R3 | `/squad #895` | R1: `line-breaks.md`, `AGENTS.md`; R2: `ChainLineBreakRewriter.cs`; R3: RH5201/RH5112 test files whose fix expectations #895 may move |
| R4 (#1820) | `/squad #1803` | logical (#1820 feeds it). Its file set comes from #1820's report: if it names chain/list/line-break files, also after R2 and R10; if indentation/`LayoutComputer`, also after R12 |
| R6 + R8 | `/squad #851` | R6: RH5401's final fix guard, which #851 mirrors (logical); R8: registry files |
| R10 (#895) | `/squad #896` | `ChainLinkLayoutTests.cs`, chain output; (R3 already merged before R10) |
| R10 + maintainer's answer on #894 | `/squad #894` | declare: `ChainLineBreakRewriter.cs`, `AGENTS.md`, precheck files (R1 merged before R10); rule: also after R9 (registry files) |
| R10 + R9 | `/squad #892` | R10: P11 scope of the join decides the rule's scope; R9: registry files (and after R13 if #894 became a rule) |
| R14 + R12 + R13 | `/squad #893` | R14 theme order and `FluentChain` use; R12 anchor and RH5201 files; R13 decides case 2 |
| R14 (or the last open rule run) | `/squad #897` | registry files (`AnalyzerResources.resx`, `CodeFixResources.resx`) |
| R5 + R16 | `/squad #352` | R5: option helper and line-count design; R16: registry files (end of the registry chain) |

Registry chain (resource/ruleset/spec-index files only): R5 → R8 → R9 → R14 → R16 → R17 (R13 slots in after R9 if #894 becomes a rule). This is a recommended order, not a logical one: whichever of these runs is otherwise ready first may take the next slot.

Critical paths: R2 + R1 → R10 → R12 → R15; R1 → R6 → R9 → R14 → R15 / R16 → R17; R4 → R11.

## 5. Findings for the coordinator

1. **#1439 — premise fails, no caller.** FMT-UNSUP-012 (#1839) refuses every comment/directive behind `=>`; FMT-STRUCT-006 states the conversion never meets one. Recommend closing as resolved by #1839 (no run).
2. **#1515 — no caller once #1851 lands.** Get/Get2 are FMT-UNSUP-012 inputs; Get3 (`/* c */ =>` on its own line) is #1851's shape. Options: A close after R1 merges (recommended), B add it to R1 (`/squad #1851 #1515`, same lane and mechanism; the PR would carry `Refs #1515`).
3. **#1539 shrinks after #1851** to the semicolon-trivia carrier; likely a recorded decision. Keep after R1 and re-check, or close then.
4. **#894 needs the maintainer's output decision before it can be scheduled** (declare the operator-trivia shape unsupported vs. a new no-fix rule). Declaring deletes `CloseGap`/`CloseInnerGaps` trivia guards and `AlignRestOfRun`'s operator column, and makes #893 case 2 moot — recommended to ask now.
5. **#1851 still carries one output question:** RH5401 no longer reports `int B // note⏎=> 1;` (recommended in the issue) vs. reports it without a fix.
6. **#849/#850 code fix:** the node-level formatter skips the get-only lift for code fixes (`CodeFixDisabledStructuralTransforms`), so "delegate to the formatter" is not available as is; the run decides (own rewrite, a formatter entry point, or no fix). Expect a question. Their "rule doc records withheld fixes", "README row", "#628" goals are stale or against the rule-page convention.
7. **#851** ID is RH5419 (RH5418 = empty unions); its guards are now FMT-UNSUP-012/#1851 + SHARED-FIX-010.
8. **#1277**'s "rule page carries the `?[ ]` clarification" contradicts the rule-page convention; spec only.
9. **#895 is partly done** by #1813 (argument lists); remaining: element access, and `!` if it reproduces.
10. **Registry files serialize every new-rule run** (R5, R8, R9, R14, R16, R17, R13-as-rule). Appended `.resx`/ruleset/index rows rarely conflict textually; if the coordinator accepts that, the chain dissolves and only logical edges remain (R8 after R1; R9 after R6; R14 after R10; R17 after R5).
11. **#1820 may have no diff:** its product is a report on the issue plus follow-up issues. A squad run needs something to publish; either keep a `scripts/benchmark.sh` (tooling lane) or run it as an investigation outside the squad. Needs NuGet for the released 1.2.0/1.3.0 tools. Its follow-ups belong to 1.5.0 only via #1803 (follow-up rule); others go to their theme's release.
12. **Release fit (follow-up rule):** #352 (L, new metric, new option surface) is the largest and least connected item; it shares only the option design with #353/#354. Candidate to move to a later release if 1.5.0 should stay on T4/T5; #353/#354 are self-contained. #893 is "rare in practice" by its own text and depends on three runs; candidate to narrow to case 1 or defer.
