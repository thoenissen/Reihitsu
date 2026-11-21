Role: auditor
Commit: 56b341e
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Scope

Full audit of head `56b341e` (checked out, clean tree) against `origin/main` `080a2dc` (equal to `git ls-remote origin refs/heads/main`; an ancestor of HEAD, merged by `5569be0`). The diff `080a2dc..HEAD` outside `sdd/work/` is 27 files: `Release.ruleset`, the change doc `sdd/changes/2026-11/enforce-complexity-rule.md`, 15 production files (Core 2, Analyzer 5, CodeFixes 3, Formatter 4, Tooling 1), and 10 test files. This matches handovers 04 and 05 one for one. `scripts/lane-check.sh behavior-change` confirms the lane. No compiled file changed after `c861527`: the merge brought only `.claude/commands/*` and `CLAUDE.md`. So the implementer's full self-hosting run at `c861527` (every analyzer row passes; the only failure is `WorkingDocumentsDirectoryContainsNoFiles`, which `sdd/work/` causes) and the cleaner's Release build (0 warnings) apply to this tree unchanged. I re-ran no test, because no finding depended on one.

Issue thoroughly checked against the change doc: thoenissen/Reihitsu#1915 (`agent-filed`).
- The self-hosting scan no longer ignores RH2009: `_ignoredDiagnosticIds` and its filter are gone from `AnalyzerViolationReport.Collect`, and no other ignore exists.
- `Release.ruleset:114` reads `Error`.
- `Debug.ruleset` has no RH2009 entry. This is a deliberate absence, which `DebugRulesetHasNoEntryForComplexityRule` pins.
- The root `reihitsu.json` sets no complexity limit. `BuildConfigurationTests.RootConfigurationSetsOnlyTheProductionTypeLimitOfFiveHundred` pins that.
- The limit stays 15 and is recorded in the change doc. RH2009-001, `documentation/rules/RH2009.md`, and `documentation/cognitive-complexity.md` are untouched.
- The dropped and adjusted goals (a Debug entry, a repository limit, the tooling lane) are recorded in `00-lead.md` decisions 1–5. No visible output is in question.
- I recomputed the change doc's numbers from the per-score counts in `01-spec-author.md`: every bucket, the counts over 10/12/15/20/25, 0.73 %, 0.05 %, and the p99 of 14 (24 production members over 14 < 26 = 1 %, 33 over 13). All match.

Each of the 27 restructurings traced statically against `080a2dc`. All preserve behavior: they evaluate the same conditions in the same order, with the same side effects and the same results.
- **Core.**
  - `RemoveDirectiveLines`: the run bounds still read the growing `removedLines`, and the touch/doubled scans read `removedLineIndices`.
  - `ReplaceAccessibilityModifiers`: the branch chain moved verbatim, with `carriedLeadingTrivia` passed by `ref`.
- **Tooling.** `TryGetRequiredLane`: the same lane and reason on every path. On failure `lane` stays `Docs`; on success `reason` is `null`.
- **Analyzer.**
  - `OnTypeDeclaration`: `!relevant || (partial && reportedPart != decl)`, short-circuit order kept.
  - RH3002: `IsPrecededByOpeningArgument` yields the same `declarationEnd` and the same `false` when the backward scan runs out. `ScanTypeArgumentSuffixes` gives `-1` exactly where the inline loop returned it. `GetWholeName` is moved verbatim.
  - RH8203: `GetInheritdocOfPart` leaves `laterInheritdocNode` null on the include path, as before.
  - RH8303: `continue` became `return`, and the call is the last statement of the loop.
  - RH5204: moved verbatim.
- **CodeFixes.**
  - Attribute placement: `WithTrailingTrivia(TriviaList(Space))` equals `WithTrailingTrivia(Space)`. `GetScopeBraces` is moved verbatim. `IsOffItsDepthColumn` only moves a pure line read behind `checksColumn = false`.
  - RH8201: the index form matches the iterator for every list. The indentation is the trivia directly in front of the first documentation comment. Behind a `/** */` the whitespace is skipped, then a line break is dropped, or the indentation is written in front of the next kept trivia, or at the end of the list. Nothing is replaced a second time. A list without a documentation comment is returned as is.
  - RH8402: `isBehindLastComment = IsKind(Whitespace)` before the `if` equals the old `if/else`.
- **Formatter.**
  - `WriteTriviaLineStart`: `WriteLineStart` is still called first and unconditionally. `lastLine` is still read from the input-tree trivia (`triviaItem` is `trivia[triviaIndex]` at the call), before the markers are aligned.
  - `ApplyLeadingTriviaEdit`: moved verbatim, with `continue` → `return` at the end of the loop body.
  - `CreateSplitAttributeList`: `lastIndex` is recomputed with the same expression.
  - `FindLeadingBlankLineRunEnd`: `endOfLineCount > 0` holds exactly when `afterEndOfLine` did.
  - `RemoveBlankLinesDirectlyBeforeToken`: moved verbatim, with the same `previousToken` and `_context`.
- **Test helpers.** All match `04-test-implementer.md`. The `SpecCatalog` assignment `hasPassedHeaderTable = isInHeaderTable` holds only under `hasPassedHeaderTable == false`. `AddCompileAssets` keeps "the first existing path, skipped when the simple name is present". `CreateSource` is byte-compared over 144 cases.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| — | — | — | — | — | — | None. | — |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Partial member reported on this part (`IsReportedPart`, RH7405–RH7408 base) | the declaration's modifiers; `GetReportedPart` over the parts in override regions | `LiesInOverrideRegion`, unchanged | same span and order as the inline condition at `080a2dc` | holds |
| Argument joins a type argument list (`WouldJoinTypeArgumentListAcrossArguments` / `IsPrecededByOpeningArgument`) | the arguments in front of the argument up to the first opener or non-continuer; then those behind it | `ClosesPrecedingTypeArgumentList`, unchanged | same bounds; `declarationEnd` from the same argument | holds |
| Blank line removed with directive lines (`IsBlankLineLeftOverByRemoval`, `TouchesRemoval`) | the run of blank and removed lines around the line; the brace lines next to it | `IsLastTokenOfLine` / `IsFirstTokenOfLine`, unchanged | same bounds; same two sets (`removedLines` for bounds, `removedLineIndices` for touch) | holds |
| Enclosing scope unusable (`HasUnusableEnclosingScope` / `GetScopeBraces` / `IsOffItsDepthColumn`) | the ancestors' braces; the leading whitespace of the innermost opening brace's line | `SyntaxIndentationUtilities.IsIndentingAncestor` brace kinds (pre-existing documented mirror, unchanged set) | same kinds, same column check | holds |
| Blank lines before a token (`RemoveBlankLinesDirectlyBeforeToken`) | the token's leading trivia; `previousToken.OpensContent`/`EndsLine` | `CollapseLeadingBlankLines`, `RemoveBlankLinesAfterTrailingDirective`, unchanged | moved verbatim | holds |
| `///` continuation alignment (`WriteTriviaLineStart`) | the trivia's own span in the input tree | `WriteLineStart` result, unchanged | same span (`trivia[triviaIndex]`) | holds |
| Old header span (`ScanLeadingTrivia`) | the first token's leading trivia up to the first non-comment, non-whitespace trivia | the outer `hasComment`/`hasCopyrightComment` decision, unchanged | same | holds |
| `/** */` line break dropped (`DropLineBreakBehindComment`) | the whitespace and the line break directly behind the replaced comment | none | same as the iterator's `isLineBreakPending` | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| First documentation comment of a trivia list | `RH8201…CodeFixProvider.FindFirstDocumentationComment` only (`rg "IsDocumentationCommentTrivia\(triviaList\["`); it calls `SyntaxTriviaUtilities.IsDocumentationCommentTrivia` | single owner |
| Blank line test | `IsBlankOrRemovedLine` delegates to `FormattingTextAnalysisUtilities.IsBlankLine` | single owner |
| Widening a name to its member access / qualified name | `RH3002TypeArgumentListUtilities.GetWholeName` only (`rg "Parent is QualifiedNameSyntax \w+ && \w+\.Right =="`; RH3103's hit is a single-step check for a location, not this loop) | single owner, moved |
| Brace-scope kinds | `TargetAttributePlacementCodeFixProviderBase.GetScopeBraces` mirrors `SyntaxIndentationUtilities.IsIndentingAncestor` | pre-existing, documented mirror, moved unchanged; not introduced by the run |
| Self-hosting diagnostic filter | `AnalyzerViolationReport.Collect` (source-tree filter only) | single owner; no ignore list remains (`rg "ignoredDiagnostic"`: work files only) |
| Repository's RH2009 severity | `Release.ruleset:114` (Error), no `Debug.ruleset` entry, no root limit | as the change doc decides |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RulesetTests.ReleaseRulesetReportsRuleAsError("RH2009")` | Release sets RH2009 to `Error` | entry `None`, `Warning`, or missing | `LoadReihitsuEntries`, `Assert.AreEqual("Error", …)` | as strong as the guarantee |
| `RulesetTests.DebugRulesetHasNoEntryForComplexityRule` | Debug has no RH2009 entry | any entry, whatever its action | `LoadReihitsuEntries`, `Assert.IsFalse(ContainsKey)` | as strong as the guarantee |
| `RulesetTests.DebugRulesetHasNoEntryForLineCountRule` (moved only) | unchanged | unchanged | unchanged | unchanged |
| `AnalyzerSelfHostingTests.AnalyzerReportsNoViolationsOnOwnSourceCode("RH2009…")` (existing row, now effective) | no member of the scanned code exceeds 15 | any `[RH2009]` line | `AnalyzerViolationReport.Collect`, now unfiltered by ID | as strong as the guarantee; passed at `c861527` (identical sources) |
| Deleted `ReleaseRulesetSwitchesOffComplexityRule`, `ReportLeavesOutComplexityViolations` | pinned the state this change reverses | — | — | deletion correct (decisions 4, 7); see *Hints* |
| Restructured helpers `CreateSource`, `GetTokenLineShape`, `FindTabsInCodeAndDirectiveWhitespace`, the `SpecCatalog` readers, `CollectFormatterDeclarations`, `RoslynVersionOwnership.Check`, `AddPackageReferencesFromAssets` | results unchanged for every argument the tests pass | a changed fixture, observation, or checked set | — | traced equivalent; `CreateSource` also byte-compared (04); no assertion changed |

No expected value of a visible surface was added or changed. The only test-value changes concern repository build configuration, which is not a visible surface. Check 10 holds.

## Failure classes dismissed
- FC-001, FC-002, FC-003: no guard was narrowed, widened, or given a new qualifier. Every restructured predicate inspects the same span with the same boundaries (*Guard scope*).
- FC-004, FC-005, FC-019, FC-020, FC-021: the members that decide trivia (`RemoveDirectiveLines`, `CollapseLeadingBlankLines`, `RemoveBlankLinesDirectlyBeforeToken`, RH8201 `ReplaceDocumentation`, RH8402 `GetLeadingHeaderSpan`, `ReplaceAccessibilityModifiers`, `WriteTriviaLineStart`, `ApplyLeadingTriviaEdit`) keep or drop exactly the trivia they did before, traced above. Their existing tests (the skeptic's table) passed in the implementer's Debug runs.
- FC-006, FC-022–FC-027, FC-029: no rewrite was added or changed; the output of the code fixes and the formatter is unchanged by construction.
- FC-007, FC-008, FC-009, FC-018, FC-030: no code-fix registration, Fix All path, or formatted span changed.
- FC-010, FC-011, FC-012: no phase decision changed. The self-hosting formatter idempotency and stability rows passed over the changed files at `c861527`.
- FC-013, FC-014: no rule or formatter ownership changed.
- FC-015, FC-016: no new owner (*Policy ownership*).
- FC-017, FC-033, FC-034, FC-035: no assertion was weakened, and no expected value was taken from patched code. The new expected values come from the change doc's decisions.
- FC-028: no entry point changed.
- FC-031: every changed member's summary and inline comments were re-read. The moved comments describe the moved code. In `VisitToken`'s former branch, "the branch above" became "VisitToken has already removed it", which is correct. The summaries of the originating members (`RemoveDirectiveLines`, `ReplaceAccessibilityModifiers`, `CollapseLeadingBlankLines`, `ApplyEdits`, `BuildSplitAttributeLists`, `HasUnusableEnclosingScope`, `GetLeadingHeaderSpan`, `AlignCommentIndentation`, `TryGetRequiredLane`) still describe the whole behavior. No `TODO`, no commented-out code, and no tracker number in an added line.
- FC-032: no compiled change after `c861527`, where the full self-hosting run (every analyzer row, the formatter rows included) and the Release build were taken.
- FC-036: no member was renamed or removed. The phase-table names are intact (198 architecture tests passed, `07-spec-checker.md`).
- FC-037: not a text-only claim.
- FC-038–FC-044: no unsupported shape, precheck, or modelled computed result is involved.

## Architecture findings
_None._

## Hints
- No test pins that the scan now *reports* RH2009. `ReportLeavesOutComplexityViolations` was deleted, not inverted. Without an ID filter in `Collect`, the general `ReportNamesFileLineRuleAndAnalyzerOfAViolation` covers the pass-through, and lead decision 4 records the choice. Inverting the deleted test (assert `Collect` returns the sample's `[RH2009]` line) would pin the change doc's sentence cheaply. Not a finding.
- Until `/bump-version` rebuilds `binaries/`, neither `scripts/build.sh` nor the default test run judges RH2009. Only CI's self-hosting row does, and the validator role runs only the line-count rows. A later run that raises a member over 15 passes locally and fails CI. The change doc states this transitional state (*The build enforces the rule once `binaries/` is rebuilt*). It applies to every rule added since 1.4.0, so it is not specific to this run.
- `src/Reihitsu.Tooling/LaneClassifier.cs` gained a UTF-8 byte order mark. This conforms to `.editorconfig` (`charset = utf-8-bom`) and changes no compiled behavior.

## Follow-up candidates
_None._
