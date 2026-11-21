Role: skeptic
Commit: 5252d6b
Verdict: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Premise

The issue is real, and its numbers check out. thoenissen/Reihitsu#1915 is `agent-filed` and is a roll-out, not a bug report. Every "stand-in" it describes holds at `5252d6b`:

- `src/Reihitsu.SelfHosting.Test/Utilities/AnalyzerViolationReport.cs:25-28` puts `"RH2009"` in `_ignoredDiagnosticIds`, and line 78 filters on it. This is the only consumer.
- `Release.ruleset:114` is `<Rule Id="RH2009" Action="None" />`. RH2008 and RH2111 are at `Error` (lines 113 and 125).
- `Debug.ruleset` has no entry for RH2009, RH2008, or RH2111.
- The root `reihitsu.json` sets only `design.maximumTypeLines: 500`.

**Independent re-run of the spec author's count at the default.** I made a detached worktree of `5252d6b` and replaced `"RH2009"` in `_ignoredDiagnosticIds` with a dummy ID. I changed nothing else: the root `reihitsu.json` stayed as is, so the limit was the default 15. Then I ran `scripts/build.sh` (Release, 0 warnings) and `scripts/test.sh --no-build --project selfhosting --filter "Name~RH2009"`. Result: `Failed AnalyzerReportsNoViolationsOnOwnSourceCode ("RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer") [36 s]` with **27 `[RH2009]` lines**. They match the 27 members, scores, and lines in `01-spec-author.md` one for one, for example `src/Reihitsu.Core/RegionDirectiveBlankLineUtilities.cs:29 [RH2009] … complexity of 29, which exceeds the limit of 15.`. The other 6 rows the filter selects passed. I removed the worktree afterwards.

**Enforcement path.** `binaries/Reihitsu.Analyzer.dll` (last changed in the 1.4.0 bump) contains neither the string `RH2009` nor `RH2008`/`RH2111`, in UTF-8 or UTF-16. So the `Error` in `Release.ruleset` does nothing until `/bump-version`. Until then the self-hosting scan is the only check, and CI runs it on every PR (`scripts/README.md:99`). Release sets `TreatWarningsAsErrors` (`Directory.Build.props:48`) and Debug does not, so the change doc's statement that a Debug build reports a Warning and not an error is correct.

**Scan coverage compared with the build.** The scan skips projects whose SDK is not `Microsoft.NET.Sdk` (`SelfHostingSolution.IsSelfHostable`, line 190), so it never sees `Reihitsu.Playground` (Razor) or `Reihitsu.Playground.Test`, which depends on it. Once `binaries/` carries RH2009, those projects are judged only by the Release build. Checked statically: none of their 10 `.cs` files has a structure at 12 or more spaces of indentation, so no member nests a structure and RH2009-004 leaves every one unreported. No file has more than 7 decision tokens either. That build cannot fail on RH2009 at today's code. No real `#if` exists in `src/` outside string literals, so the scan and the build see the same code.

**Is 15 well chosen?** The evidence supports keeping it.
- 19 of 2609 production members are over it (0.73 %). The line-count roll-out had 18 of 2514 (0.72 %, `enable-line-count-rules.md:40`), and the change doc's comparison matches that.
- The production 99th percentile is 14, as the per-score counts show: 24 members are above 14, which is less than 1 % of 2609.
- No member is far out (the highest is 29).
- From documentation rather than a run: 15 is also the default threshold of SonarSource's cognitive-complexity rule S3776.

So the maintainer's condition "if the default turns out poorly chosen" (triage comment on #1915) is not met. See *Internal choices* for whether question 1 needs asking at all.

## Mechanism and candidates

_N/A._ This is a roll-out, not a bug. The work list is the 27 members above, all reported against the default limit (RH2009-001). By role:

- **Production (19):** 2 in `Reihitsu.Core`, 8 in `Reihitsu.Analyzer`, 4 in `Reihitsu.Analyzer.CodeFixes`, 4 in `Reihitsu.Formatter`, 1 in `Reihitsu.Tooling`.
- **Test code (8), all helpers and none a test method:**
  - `SelfHostingSolution.AddPackageReferencesFromAssets`
  - `SpecCatalog.ReadMentionedRequirementIds` and `ReadRequirements`
  - `ObservedPipeline.FindTabsInCodeAndDirectiveWhitespace`
  - `PhaseLineBreakOwnershipTests.GetTokenLineShape`
  - `PhaseOwnershipDocumentationTests.CollectFormatterDeclarations`
  - `CliDeepNestingEndToEndTests.CreateSource`
  - `RoslynVersionOwnership.Check`

## Breaking inputs

**Production refactors.** No refactored production member may change behavior. Each one already has test coverage, so the run needs no new test for them (`README.md`, *Coverage first*):

| Member | Existing tests that pin it |
|---|---|
| `RegionDirectiveBlankLineUtilities.RemoveDirectiveLines` | RH7303 and RH7310 code-fix tests (its callers), 3 test files naming it |
| `DeclarationModifierUtilities.ReplaceAccessibilityModifiers` | `Reihitsu.Core.Test/DeclarationModifierUtilitiesTests.cs`; RH2004, RH2005, RH7105, RH7106 fix tests |
| `LaneClassifier.TryGetRequiredLane` | `Reihitsu.Tooling.Test` (3 files) |
| RH8402 fix `GetLeadingHeaderSpan`; RH8201 fix `ReplaceDocumentation` | RH8402 and RH8201 test classes |
| `TargetAttributePlacementCodeFixProviderBase.PlaceAttributeListAsync` and `HasUnusableEnclosingScope` | the RH55xx attribute-placement fix tests (16 files) |
| `RH3002TypeArgumentListUtilities` (3 members); RH5204 `AlignCommentIndentation`; `NonOverrideMembers…AnalyzerBase.OnTypeDeclaration`; RH8203 `OnDeclaration`; RH8303 `OnSyntaxTree` | RH3002, RH5204, RH7405–RH7408, RH8203, RH8303 analyzer tests |
| `IndentationRewriter.ApplyIndentation`, `RawStringLineShifts.ApplyEdits`, `BlankLineTokenCleanupRewriter.CollapseLeadingBlankLines` and `VisitToken`, `AttributeListShapeUtilities.BuildSplitAttributeLists` | `Reihitsu.Formatter.Test`, plus the self-hosting formatter idempotency and stability rows |

**Failure classes that reach this change:**

- **FC-031.** An extracted method or a restructured member keeps a summary or comment that describes its old shape. Likely on 27 members; the auditor should check every one.
- **FC-015/FC-016.** An extracted helper becomes a private copy of something `Reihitsu.Core` already provides. Especially at risk: the trivia and blank-line code in `RegionDirectiveBlankLineUtilities`, `BlankLineTokenCleanupRewriter`, and RH8303, and the modifier code in `DeclarationModifierUtilities`.
- **FC-036.** A rename treated as behavior-preserving. `AGENTS.md`'s phase-ownership table names members, and `PhaseOwnershipDocumentationTests` checks that they exist: `IndentationRewriter.Apply`, `RawStringLineShifts`, `AttributeListShapeUtilities.SplitAttributeLists`/`MergeAttributeLists`/`UsesMergedListShape`, `BlankLineEditor`. None of the 27 is named there. A restructuring that renames or removes one of those names breaks the table.
- **FC-004/FC-020.** Several members being restructured make trivia and directive decisions themselves (`RemoveDirectiveLines`, `CollapseLeadingBlankLines`, RH8201 `ReplaceDocumentation`, RH8303). A reordered condition can change which comment, directive, or blank line they keep. The existing tests of those members are the guard; no new test is needed unless the implementer changes the order of evaluation of a condition that has side effects.
- **FC-032.** A result gathered before the refactor is taken as still valid. The 27 count and the clean runs of every other analyzer were taken before the refactor. The extracted members are new code judged by every rule. The Release build loads 1.4.0 `binaries/`, which lack every rule added since 1.4.0 (RH2008, RH2111, and RH2009 at least). So the build alone does not prove the refactored tree is clean. The self-hosting rows for RH2008, RH2111, and RH2009 must run after the refactor (`--filter "Name~RH2008|Name~RH2111|Name~RH2009"`), and so must the whole scan, since about 25 files change. CI runs the whole scan; locally it takes more than 10 minutes.

**Test-helper refactors.** Three of the 8 helpers produce test inputs or observations: `CliDeepNestingEndToEndTests.CreateSource` builds fixtures, and `GetTokenLineShape` and `FindTabsInCodeAndDirectiveWhitespace` build the values the assertions compare. Restructuring them must leave their results unchanged for every argument the tests pass. If one changed, a test would silently assert something else (FC-017/FC-035 adjacent). The `SpecCatalog` readers, `CollectFormatterDeclarations`, `RoslynVersionOwnership.Check`, and `AddPackageReferencesFromAssets` decide what the architecture and self-hosting tests check. Losing an element there weakens a guard without any test failing.

**Pinned configuration, so no new tests:**

- That the root `reihitsu.json` sets no complexity limit is already pinned by `BuildConfigurationTests.RootConfigurationSetsOnlyTheProductionTypeLimitOfFiveHundred`, which requires exactly `maximumTypeLines=500`.
- `RulesetTests.ReleaseRulesetSwitchesOffComplexityRule` (asserts `None`) and `AnalyzerViolationReportTests.ReportLeavesOutComplexityViolations` pin today's state and must change. `ComplexityAnalyzerHasARow` holds as it is.
- The Release `Error` fits the existing `ReleaseRulesetReportsRuleAsError` data rows. The Debug absence fits `DebugRulesetHasNoEntryForLineCountRule`, whose name then no longer fits. `scripts/README.md:109` names that test and counts the rows the `Name~RH2008|Name~RH2111` filter selects ("18 tests today"). A rename of the test or a new RH2009 data row in it requires updating that paragraph. An RH2009 data row in an existing method does not change the 18, because the filter matches by name.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH2009 analyzer, `CognitiveComplexityWalker` | must stay |
| Spec `sdd/specs/analyzer/RH2009.md` | must stay (with answer A or no question) |
| Rule page `documentation/rules/RH2009.md`, `documentation/cognitive-complexity.md` | must stay (with answer A or no question) |
| `Release.ruleset` | must change: RH2009 `None` → `Error` |
| `Debug.ruleset` | must stay: no entry, like RH2008 and RH2111 |
| Root `reihitsu.json` | must stay (pinned, see above) |
| `AnalyzerViolationReport` ignore list | must change: remove; the set is empty afterwards |
| `RulesetTests`, `AnalyzerViolationReportTests` | must change (see above) |
| 19 production members (Core, Analyzer, CodeFixes, Formatter, Tooling) | must change in structure and stay in behavior |
| 8 test helpers | must change in structure and stay in results |
| Code-fix results, Fix All, formatter output | must stay: no visible output changes |
| `AGENTS.md` phase table, `PhaseOwnershipDocumentationTests` | must stay: do not rename the members the table names |
| `scripts/README.md` line-count rows paragraph | not involved, unless `DebugRulesetHasNoEntryForLineCountRule` is renamed |
| `binaries/` | not involved (it is `/bump-version`'s job) |

## Refuse option

There is no code fix or formatter construct to withhold, and declaring a shape unsupported does not apply: the issue concerns an analyzer limit on the repository's own code, not a syntax shape the formatter or a fix refuses. The refusal-like alternatives all fail:

- **Keep RH2009 ignored.** This contradicts the maintainer's triage decision to try the default before 1.5.0.
- **A repository limit above 15.** Ruled out by decision 2A of `enable-line-count-rules.md` (a repository limit only below the default).
- **Suppressions or `.editorconfig` exemptions on the 27 members.** These hide exactly what the roll-out tests, and would need escalation 6.

The requested change costs 27 restructurings in about 25 files. It needs no new type and no shared owner, only extracted private methods and flattened conditions, so it is not escalation 9. There is no simpler option.

## Output questions

_None_ from me. Question 1 of `01-spec-author.md` is discussed under *Internal choices*.

## Internal choices

- **Whether to ask question 1 (default 15 / 20 / 10).** Keeping 15 changes no visible output: RH2009-001 stays as it is and demands it, so nothing is in question (`README.md`, *What reaches the maintainer*). The maintainer's triage comment asks for an output question only "if the default turns out poorly chosen", and the evidence above does not show that.
  - Recommendation: do not ask. Keep 15, record the evidence in the change doc (already there) and the PR, and have the lead set the change doc to `Accepted` with no visible output change.
  - Alternative: ask anyway, as the spec author wrote it. That costs one round trip, and the evidence supports A. Asking changes nothing that is shipped unless the maintainer picks B or C. Either way, the refactor work list (27 members) can start only after the choice is settled.
- **Who restructures the 8 test helpers.** The implementer must not change test inputs or assertions, and the test implementer must not touch production code. The 8 helpers are test code, but restructuring them changes no input or assertion. Recommendation: route them to the test implementer, with the requirement that each helper's result stays unchanged. The implementer takes the 19 production members. Alternative: the implementer does all 27 with the lead's explicit permission recorded in `00-lead.md`.
- **Test names.** Fold RH2009 into `ReleaseRulesetReportsRuleAsError` (a new data row) and delete `ReleaseRulesetSwitchesOffComplexityRule`. For Debug, either add a data row to `DebugRulesetHasNoEntryForLineCountRule`, accepting the misnomer, or rename it, which also requires the `scripts/README.md:109` text. Recommendation: rename it to a name that covers both, and update the README paragraph in the same change.
- I agree with the spec author's decisions 2–4 in `00-lead.md`: no root limit, no Debug entry, and removing the empty `_ignoredDiagnosticIds` and its test.

## Issue goals

- "`Debug.ruleset` — decide and add the RH2009 entry" and the done-when item "a deliberate entry" become a deliberate **absence**, pinned by a test, which matches RH2008/RH2111. This changes no visible output (repository build configuration only).
- "Set `design.maximumMethodComplexity` in the root `reihitsu.json` if the review settles on another limit" is dropped. Decision 2A allows a repository limit only below the default, and a different default belongs in RH2009-001, the rule page, and the scoring guide. No visible output change.
- "Refactor them, or change the limit": the option to change the limit only at repository level is dropped for the same reason. Changing the shipped default is the maintainer's call, and the evidence does not call for it.
- "Lane: tooling" is superseded by `behavior-change` (production paths next to ruleset paths need a change doc). This is internal.
- The rest stands: the self-hosting test passes without RH2009 on the ignore list, `Release.ruleset` sets `Error`, and the chosen limit is recorded in the change doc.

## Architecture findings

_None._

## Follow-up candidates

_None._ Note: the scan's exclusion of non-`Microsoft.NET.Sdk` projects (Playground) is intended and applies to every rule. For RH2009 I showed above that it is harmless at today's code.
