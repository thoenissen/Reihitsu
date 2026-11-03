Role: auditor
Commit: f85a90a
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: thoenissen/Reihitsu#1486, `00-lead.md` (maintainer answers 1–7, decisions 1–12), handovers 01–10, `sdd/changes/2026-11/region-directive-separator.md`, `git diff f8dc7ef..f85a90a`, and the counterparts named below.

## Scope

Full audit. Head `f85a90a` (checked out, clean tree). Base `f8dc7ef7a71bffbf373675d0613e32a77a1ef5a6` = `git ls-remote origin refs/heads/main` = merge base, so `origin/main` is merged. The diff matches the handovers: production changes in `RegionNamingRewriter`, `RegionFormattingPhase`, `FormattingContext`, `ReihitsuFormatter`, `RegionDirectiveUtilities` (07); tests as listed in 04, 06, 08; specs and change doc as in 03 and 05. `scripts/lane-check.sh behavior-change`: confirmed. Build and focused test results of the cleaner (09) and test implementer (08, full formatter/analyzer/cli/architecture runs after the last production commit `ac171f5`) are taken as given.

Issue goals ("Done when"): the issue input gives the desired output (`SeveralSpacesAndTrailingWhitespaceOfRegionLinesAreNormalized`), a tab separator does (`TabBetweenRegionKeywordAndDescriptionBecomesOneSpace`, `TabMixedWithSpacesBeforeDescriptionBecomesOneSpace`), and FMT-TAB-002's example and test show one space. The `#endregion` half of the proposed behavior is met for undescribed regions (FMT-REGION-004); every other `#endregion` already has one form (FMT-REGION-002).

Evidence gathered by execution (fixtures under the scratchpad, outside the repository):

- `scripts/format.sh` on CRLF and LF fixtures: `#region    Bar···` → `#region Bar`; `#region···` → `#region`; `#endregion<tab>// z<tab><space>` → `#endregion // z`; `#endregion//Stale···` → `#endregion //Stale`; `#region    inner···` inside a method body → `#region Inner` / `#endregion // Inner`; a second run changes nothing (byte-identical), under CRLF too.
- The change doc's pairing claim holds: an `#if A`/`#else` pair of `#region`s sharing one `#endregion`, a region closed by an `#endregion` in each `#if false`/`#else` branch, and a lone `#region` inside `#if false` are all reported as syntax errors and skipped by the CLI.
- `scripts/apply-fix.sh RH7105` on a type holding `#region<tab>bar` … `#endregion<tab>// Stale`: see finding #1.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `sdd/specs/formatter/pipeline.md` FMT-PIPE-028 (first sentence); `sdd/changes/2026-11/region-directive-separator.md` *What*, third bullet; `ReihitsuFormatter.FormatNode`/`FormatNodeInDocumentAsync`/`FormatNodeInDocumentWithContextAsync` `<remarks>`; `FormattingContext.FormatsRegionDirectives` summary; `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterNodeRegionTests.cs` | FC-028, FC-012, FC-003 | – | PR-introduced | FMT-PIPE-028 says the node-level entry points "MUST NOT change the text of a `#region` or `#endregion` directive"; the change doc says a code fix "leaves their text as it was: the separator, …"; the public remarks say the text "is kept as written". The preparation phase still runs at node level and replaces a tab between directive tokens with four spaces, as unchanged FMT-PIPE-019 / FMT-TAB-002 demand. Counterexample (`scripts/apply-fix.sh RH7105`, LF and CRLF): input `static public class C { #region<tab>bar … #endregion<tab>// Stale }` → fix writes `#region····bar` and `#endregion····// Stale` (tab became four spaces). Members (axis: where the tab stands × entry point): tab between `region` and the description — fails; tab between `endregion` and its text — fails; tab behind a bare `region`/`endregion` keyword — fails (same replacement); tab between `#` and the keyword — fails (same replacement); tab inside the description or inside the `#endregion` comment — holds (free text, FMT-TAB-003); directive in the leading trivia of a `FormatNodeInDocumentAsync` target that keeps its leading trivia, and in the target's leading trivia under `FormatNodeInDocumentWithContextAsync` — holds (written back as in the document). `FormatNode` and `FormatNodeInDocumentWithContextAsync` share the mechanism (`FormattingPipeline.Execute` runs the preparation phase for every context), traced, not run. The output equals `main`'s for an upper-case description and is decided by the unchanged FMT-PIPE-019; what is wrong is the new requirement's wording, which a second reading (keep the tab) would turn into different output. No node-level test holds a tab, so neither reading is pinned. | State in FMT-PIPE-028 that the node-level entry points apply none of the FMT-REGION requirements, while the preparation phase still replaces a tab between directive tokens (FMT-TAB-002, FMT-PIPE-019); align the change-doc bullet and the three `<remarks>` and the `FormattingContext` summary ("the region phase leaves directives as written", not "the text is kept"); add a node-level test with a tab separator that pins the four spaces. If the lead prefers the other reading (tabs kept in region directives at node level), that is an output question, since it departs from FMT-TAB-002 at node level. |
| 2 | low | `AGENTS.md`, *Formatter phase ownership*, row `RegionFormattingPhase` | FC-031 | – | PR-introduced | The row still lists only "Region-name capitalization and matching `#endregion` comments while preserving every directive". The phase now also owns the line form of a described `#region` (FMT-REGION-007), the space in front of and the whitespace behind the text of every region directive line (FMT-REGION-004), and is skipped by the node-level entry points (FMT-PIPE-028, guard `FormattingContext.FormatsRegionDirectives`). Every other row names its full decision set and its guard. | Extend the row's decisions and add `FormattingContext.FormatsRegionDirectives` as its guard. |
| 3 | low | `src/Reihitsu.Formatter/Data/FormattingContext.cs`, `FormatsRegionDirectives` summary | FC-031 | – | PR-introduced | "Only `FormatDocumentAsync` and `FormatSyntaxTree` leave it at `true`" is not so: `PipelineTraceCommand` (whole-document trace, deliberately), `UsingDirectiveCodeFixUtilities.OrganizeScopeUsingsAsync`, and `RH7101DoNotCombineFieldsCodeFixProvider` also construct a context with the default `true`. The two code fixes run only the using-ordering phase and the field-split transform, so no region text changes; the statement is inaccurate, not the behavior. | Say that the node-level entry points set it to `false` and every other context keeps the default, instead of "only". |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Region phase runs at all (FMT-PIPE-028) | `context.FormatsRegionDirectives`, set `false` by `FormatNode`, `FormatNodeInDocumentAsync`, `FormatNodeInDocumentWithContextAsync` (and its `target == context` delegation), default `true` elsewhere | `CodeFixDisabledStructuralTransforms` (FMT-PIPE-011), passed by the same three entry points | Same three entry points, same boundary; whole-document (`FormatDocumentAsync`, `FormatSyntaxTree`, trace) keeps both off/on alike | ok (wording issue in #1, #3) |
| Described vs undescribed region | `GetRegionDescription(regionDirective).Length == 0` — the trimmed `PreprocessingMessage` behind `region` | RH7301 analyzer (`GetRegionDescription` vs `GetEndRegionDescription`), RH7301 fix `TryGetStartDescription` (`Length > 0`), RH7302/RH7306/RH7307/RH7311 read the same helper | Same helper, same trimming | ok |
| Text behind `#endregion` | `GetEndRegionText`: keyword trailing trivia + `EndOfDirectiveToken` leading trivia, trimmed | `GetEndRegionDescription` (now built on it; its result unchanged: same concatenation, then `//` stripped) | One read, one owner | ok |
| Replace only when changed | `AddReplacementIfChanged`: `ToFullString` of the whole directive trivia, including its end of line | Previous `endRegion.ToFullString() != canonical` check for the `#endregion` side | Same comparison, now also for `#region`; the canonical builders keep `EndOfDirectiveToken` trailing trivia, so a directive already in canonical form is never replaced | ok |
| Which directives are rewritten | `GetRegionPairs` over `DescendantTrivia(descendIntoTrivia: true)`, LIFO, active and inactive | FMT-REGION-007/004 pairing scope; FMT-PIPE-005 | Unpaired region directives are syntax errors in every shape tried (see *Scope*), so the scope sentence holds | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| `#region` line form (FMT-REGION-001/007) | `RegionNamingRewriter.BuildRegionTrivia` (only writer). `RH7302RegionsShouldStartWithAUpperCaseLetterCodeFixProvider` writes its own message text but keeps the separator as written (RH7302-004), a different decision | one owner |
| `#endregion // Description` (FMT-REGION-002) | `RegionDirectiveUtilities.CreateCanonicalEndRegionTrivia`, used by `RegionNamingRewriter` and `RH7301RegionsShouldMatchCodeFixProvider` | one owner (unchanged) |
| `#endregion` of an undescribed region (FMT-REGION-004) | `RegionNamingRewriter.BuildUndescribedEndRegionTrivia` + `RegionDirectiveUtilities.GetEndRegionText` | one owner |
| Trailing whitespace on a region directive line | `RegionNamingRewriter` (inside the directive trivia). `CleanupPhase` strips whitespace trivia at line ends but does not reach directive message text (on `main` `#region Bar···` and `#region···` kept their whitespace, 01/06) | disjoint spans, one owner per span |
| Node-level skip of region rewriting | `FormattingContext.FormatsRegionDirectives`, read only by `RegionFormattingPhase` | one owner |
| Phase ownership statement | `AGENTS.md` table row, `region-formatting.md` intro, `RegionFormattingPhase`/`RegionNamingRewriter` summaries | AGENTS.md row stale (#2) |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RegionLineFormTests` (29 tests) | Exact whole-document output for FMT-REGION-007/004 | Any byte difference, LF or CRLF; a second pass that changes the output; a phase that changes it alone | `AssertRuleResult` (exact, LF+CRLF, second pass, `PhaseStabilityCheck`) | adequate |
| `ReihitsuFormatterNodeRegionTests` node-level tests | Region directive text unchanged at node level, code around formatted | Any byte difference, LF or CRLF, first or second pass through the same entry point | `AssertNodeInDocument` / `FormatNode` exact text | adequate for what they cover; no tab member (#1) |
| `ReihitsuFormatterNodeRegionTests.FormatDocumentAsync…`/`FormatSyntaxTree…AppliesRegionRequirements` | Other side of the entry-point boundary | Region text left unnormalized | exact text | adequate (FC-003) |
| `RH7104…FixOfPartialTypeWithRegionPairKeepsTheRegionDirectives`, `…FixOfTopLevelPartialTypeKeepsRegionOpenedInFrontOfTheType`, `RH7105…FixOfTypeWithRegionPairKeepsTheRegionDirectives`, `…KeepsTheSeparatorOfAnUpperCaseDescription` | Code fix keeps region directives | Fixed code differs; diagnostic remains after the fix | `Verify` on `BatchCodeFixTestsBase` (exact fixed code, re-analysis) | adequate |
| `RH7103…CodeFixReordersStaticWithinRegionAndIgnoresInstanceMembersInOtherRegions` (changed) | Fix keeps bare `#endregion` | `#endregion // Decoy` written | `Verify` | adequate |
| `TabReplacementTests` (2 changed), `RegionFormattingTests` (2 changed), `RH7301RegionsShouldMatchFormatterTests` (2 changed), `ReihitsuFormatterTests` (3 changed), `ReihitsuFormatterNodeBlankLineTests` (1 changed) | Expected values moved to FMT-REGION-007 / FMT-PIPE-028 | Old output | unchanged helpers (exact) | adequate |

Visible output is decided: every changed expected value is covered — whole-document outputs by answers 1, 2, 3, 4, 5, 7 and decisions 8, 9 (recorded under the change doc's *Decisions*); node-level and code-fix outputs (RH7103, RH7104, RH7105, `ReihitsuFormatterTests`, node blank-line test) by answer 6. No rule page changed.

## Failure classes dismissed
- FC-001 — no guard was narrowed; the node-level guard covers the whole phase, the described/undescribed split covers every pair.
- FC-002 — "paired", "inside a disabled branch", "has a description" are explicit in the code (pair iteration, `descendIntoTrivia: true`, `Length == 0`).
- FC-004, FC-005, FC-019, FC-021 — no join, move, or gap edit; only the text of a directive is rewritten, and a directive line holds no other trivia than its message. Comments behind `#endregion` (line and block) are kept verbatim (tests and CLI fixtures).
- FC-006, FC-022–FC-027, FC-029, FC-036 — no syntax rewrite; region-looking text in a literal is covered (`RegionLookingTextInsideStringLiteralIsKept`).
- FC-007, FC-008, FC-009, FC-018 — no code-fix provider changed; the affected fixes reach a supported end state in one pass (`Verify`, `apply-fix.sh`), and no Fix All scenario holds a region.
- FC-010, FC-011 — idempotency checked under LF and CRLF by the helpers and by CLI reruns; no loop added.
- FC-013, FC-014 — no rule reports the separator or trailing whitespace (RH7302-002); RH7301 reads trimmed text, so formatter output gains no diagnostic; `#endregion //Stale` under an undescribed region was already reported by RH7301 on `main` (content rule, P11).
- FC-015, FC-016 — see *Policy ownership*; the only shared helper change (`GetEndRegionText`) leaves `GetEndRegionDescription`'s result unchanged, and the full analyzer run after the change passed (08).
- FC-017 — see *Assertion adequacy*.
- FC-020 — `#if false` branches covered for both requirements; mixed active/inactive pairings are syntax errors (verified).
- FC-028 — the three node-level entry points set the same flag and have one test each; the remaining disagreement with the spec text is #1.
- FC-030 — the change reduces what fixes format; no fix gains an edit.
- FC-031 — reported as #2, #3, and the remarks part of #1; the other changed summaries (`RegionNamingRewriter`, `Rewrite`, `BuildRegionTrivia`, `BuildUndescribedEndRegionTrivia`, `RegionFormattingPhase`, `GetEndRegionText`, `GetEndRegionDescription`) describe the new code.
- FC-032 — the last production commit is `ac171f5`; the full project runs in 08 are after it, and the cleaner changed nothing.
- FC-033, FC-034, FC-035 — expected values come from spec examples (04, 06, 08); the issue's own input is tested verbatim.
- FC-037 — no text-only claim is made for a C# file.
- Tab-width matrix — the region phase measures no column; the four spaces the tab phase writes are removed, not measured.

## Architecture findings
_None._ The line form has one writer and one statement (FMT-REGION-007); the node-level skip is one flag read by one phase. Finding #1 is a wording overreach of a new requirement against FMT-PIPE-019, not a second owner of a decision.

## Hints
- `BuildRegionTrivia` and `BuildUndescribedEndRegionTrivia` repeat the same "keyword without trailing trivia, optional `PreprocessingMessage(" " + text)`" construction; a shared helper would state the one-space form once in code. Not a defect.
- Follow-up candidates unchanged from `00-lead.md` (2: trailing whitespace behind other directives; 3: whitespace between `#` and the keyword). Candidate 1 is confirmed absorbed: `FormatNodeInDocumentAsyncKeepsBothHalvesOfRegionOpenedInFrontOfTopLevelType` and the RH7104 top-level test pin both halves unchanged.
