Role: spec author
Commit: 66739fe
Verdict: change doc written

Change doc: `sdd/changes/2026-11/enforce-complexity-rule.md` (Status: Draft). No spec diff: on the recommended answer the default of RH2009-001 stays 15, and no spec covers the repository's own rulesets, self-hosting scan, or root `reihitsu.json`. Rule page `documentation/rules/RH2009.md` and the scoring guide `documentation/cognitive-complexity.md` left unchanged; both change only if the maintainer picks B or C below.

The prompt carried no intended fix or verdict to ignore. The issue's `Lane: tooling` is superseded by the lead's `behavior-change` claim.

## Evidence — the self-hosting scan with RH2009

Method, reproducible: a detached worktree of `66739fe`; in `AnalyzerViolationReport._ignoredDiagnosticIds` the entry `"RH2009"` replaced by a dummy ID; root `reihitsu.json` with `"maximumMethodComplexity": 1` (so every member with nesting is reported with its score); `AnalyzerSelfHostingTests.AnalyzerNames` narrowed to the RH2009 analyzer; `scripts/build.sh` first (the scan needs every project's `obj/project.assets.json`), then `scripts/test.sh --project selfhosting --filter "FullyQualifiedName~AnalyzerReportsNoViolationsOnOwnSourceCode"` (35 s for the one analyzer; all 353 analyzers take more than 10 minutes). The denominator comes from the same run for RH2008 with both method-line limits at 1: the members RH2008-002 judges with more than one line of code. Test code = the projects whose name contains `Test` (as the scan judges them). Projects scanned: Analyzer, Analyzer.CodeFixes, Cli, Core, Formatter, Tooling, and their test projects, ArchitectureTests, SelfHosting.Test.

| Code | Members judged | Reported at limit 1 | 11–15 | 16–20 | 21–25 | 26–30 | >30 | >15 | Highest |
|---|---|---|---|---|---|---|---|---|---|
| production | 2609 | 627 | 77 | 14 | 4 | 1 | 0 | 19 | 29 |
| test | 16909 | 183 | 16 | 5 | 2 | 1 | 0 | 8 | 27 |

Exact counts by score (8 and up):

- production: 8:54, 9:46, 10:26, 11:32, 12:16, 13:15, 14:9, 15:5, 16:3, 17:5, 18:3, 19:2, 20:1, 22:3, 23:1, 29:1
- test: 8:4, 9:5, 10:3, 11:9, 12:2, 13:2, 14:2, 15:1, 16:1, 17:3, 19:1, 23:2, 27:1

Members over a limit: 10 → 96 + 24 = 120; 12 → 48 + 13 = 61; **15 → 19 + 8 = 27**; 20 → 5 + 3 = 8; 25 → 1 + 1 = 2. Production 99th percentile: 14.

The 27 members over 15 (the implementer's worklist at the default):

| Score | Code | Member at |
|---|---|---|
| 29 | prod | src/Reihitsu.Core/RegionDirectiveBlankLineUtilities.cs:29 |
| 27 | test | src/Reihitsu.SelfHosting.Test/Utilities/SelfHostingSolution.cs:336 |
| 23 | test | src/Reihitsu.ArchitectureTests/Sdd/SpecCatalog.cs:256 |
| 23 | test | src/Reihitsu.Formatter.Test/Helpers/ObservedPipeline.cs:118 |
| 23 | prod | src/Reihitsu.Tooling/LaneClassifier.cs:95 |
| 22 | prod | src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider.cs:119 |
| 22 | prod | src/Reihitsu.Core/DeclarationModifierUtilities.cs:215 |
| 22 | prod | src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/IndentationRewriter.cs:67 |
| 20 | prod | src/Reihitsu.Analyzer/Rules/Clarity/RH3002TypeArgumentListUtilities.cs:148 |
| 19 | prod | src/Reihitsu.Analyzer/Rules/Layout/RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.cs:74 |
| 19 | test | src/Reihitsu.Formatter.Test/Unit/Pipeline/PhaseLineBreakOwnershipTests.cs:440 |
| 19 | prod | src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTokenCleanupRewriter.cs:145 |
| 18 | prod | src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributePlacementCodeFixProviderBase.cs:208 |
| 18 | prod | src/Reihitsu.Analyzer/Rules/Clarity/RH3002TypeArgumentListUtilities.cs:292 |
| 18 | prod | src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Utilities/AttributeListShapeUtilities.cs:139 |
| 17 | prod | src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8201InheritdocShouldBeUsedCodeFixProvider.cs:112 |
| 17 | prod | src/Reihitsu.Analyzer/Base/NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase.cs:106 |
| 17 | prod | src/Reihitsu.Analyzer/Rules/Documentation/RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.cs:62 |
| 17 | prod | src/Reihitsu.Analyzer/Rules/Documentation/RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer.cs:171 |
| 17 | test | src/Reihitsu.ArchitectureTests/Formatter/PhaseOwnershipDocumentationTests.cs:647 |
| 17 | test | src/Reihitsu.ArchitectureTests/Sdd/SpecCatalog.cs:160 |
| 17 | test | src/Reihitsu.Cli.Test/EndToEnd/CliDeepNestingEndToEndTests.cs:130 |
| 17 | prod | src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTokenCleanupRewriter.cs:560 |
| 16 | prod | src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributePlacementCodeFixProviderBase.cs:108 |
| 16 | prod | src/Reihitsu.Analyzer/Rules/Clarity/RH3002TypeArgumentListUtilities.cs:457 |
| 16 | test | src/Reihitsu.ArchitectureTests/General/RoslynVersionOwnership.cs:98 |
| 16 | prod | src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs:298 |

Line numbers are the reported name's line at `66739fe`. Refactors of formatter and code-fix members change no output by design; the existing tests guard that.

## Changed requirements

_None._ RH2009-001 is unchanged on the recommended answer. On answer B or C: RH2009-001 changed (default, its `Why`, and its example's message and limits), plus `documentation/rules/RH2009.md` (text and configuration example) and `documentation/cognitive-complexity.md` (line 411 and the example total at line 486), and the change doc's `Visible output changes` gains the change.

## Examples for the tests

No new requirement. What the tests check is repository configuration, not a spec example:

- `RulesetTests.ReleaseRulesetSwitchesOffComplexityRule` (asserts `None`) becomes an assertion of `Error` — RH2009 joins the line-count rules' Release test.
- `RulesetTests.DebugRulesetHasNoEntryForLineCountRule` gains RH2009 (Debug ruleset has no entry).
- `AnalyzerViolationReportTests` test at line 119 ("the scan leaves out what RH2009 reports") no longer holds; see Internal choices.
- The self-hosting scan itself (`AnalyzerReportsNoViolationsOnOwnSourceCode` for RH2009) is the check that the 27 members are restructured.

## Output questions

**1 · RH2009 · Methods should not exceed the complexity limit — the default limit after trying it on this repository**

Context for the lead (not part of the question to the maintainer beyond one line): 27 of about 19,500 judged members exceed 15 (19 production of 2609, 8 test of 16909, highest 29); 8 exceed 20; 120 exceed 10. The maintainer asked that the trial's evidence come back with a proposed default.

Input — a method without a `reihitsu.json`, cognitive complexity 16 (the example of RH2009-001):

```csharp
class C
{
    public int Score(int[] values)
    {
        int total = 0;

        foreach (var value in values)
        {
            if (value > 0)
            {
                if (value % 2 == 0)
                {
                    for (int i = 0; i < value; i++)
                    {
                        if (i % 3 == 0)
                        {
                            total += i;
                        }
                        else if (i % 5 == 0)
                        {
                            total -= i;
                        }
                    }
                }
            }
        }

        return total;
    }
}
```

Before (`main`): reported on `Score`, "This member has a cognitive complexity of 16, which exceeds the limit of 15."

After, A (recommended) — default stays 15: unchanged, reported on `Score`, "This member has a cognitive complexity of 16, which exceeds the limit of 15." This repository restructures 27 members.

After, B — default 20: not reported. A member of complexity 21 is reported, "… of 21, which exceeds the limit of 20." This repository restructures 8 members; rule page and scoring guide state 20.

After, C — default 10: reported on `Score`, "This member has a cognitive complexity of 16, which exceeds the limit of 10." This repository restructures 120 members; rule page and scoring guide state 10.

Why A: 15 already sits just above the repository's 99th percentile of production members (14) and flags 0.7 % of them — about the share the line-count roll-out kept with its production-method default — so it prompts a design decision without being a frequent obstacle.

The run can write its tests without the answer (the default is not asserted by this run's tests beyond the existing RH2009 tests), but the implementer's refactor worklist depends on it: 27 members (A), 8 (B), or 120 (C).

## Internal choices

- No complexity limit in the root `reihitsu.json` (decision 2A of the line-count roll-out allows only limits lower than the default; nothing calls for one). Alternative: a repository limit of 10–12, which would add 34–93 members to restructure without telling more about the default.
- `Debug.ruleset` gets no RH2009 entry, matching RH2008/RH2111 (Debug lists Reihitsu rules only to switch them off; a Debug build reports the Warning). Alternative: `Action="Warning"`, which restates the shipped severity. This departs from the issue's wording "add the RH2009 entry"; its goal, "a deliberate entry", is met by the deliberate absence, which a test pins.
- With RH2009 removed, `_ignoredDiagnosticIds` in `AnalyzerViolationReport` is empty; recommend removing the field and its filter, and the `AnalyzerViolationReportTests` test that asserts RH2009 is left out (replace it by nothing: the scan then ignores no rule). Alternative: keep an empty ignore set as a hook for future roll-outs — rejected as dead code; the next roll-out can add it back.
- Restructuring only — no `[SuppressMessage]`, no `.editorconfig` severity override, no `#pragma` — for every reported member (escalation 6 would apply to any member that can only be resolved by a suppression).
- Change-doc header `Affected requirements: none — RH2009-001 keeps its default of 15` (no requirement changes on the recommended answer).

## Follow-up candidates

_None._ Note for the lead (from the precedent): `binaries/` predate RH2009, so `Release.ruleset`'s `Error` is inert until `/bump-version`; the self-hosting scan is the effective check in this run. No ruleset or build change is blocked by it, since the repository sets no complexity limit in `reihitsu.json`.
