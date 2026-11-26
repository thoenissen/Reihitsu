Role: auditor
Commit: 8b6a8d6f012e185fc723357ab82bcf643f335930
Verdict: pass, text cleanup

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The handovers carried two that I did not use: the spec author's notes on where the guard goes (01, 03) and the skeptic's FC-015 remark on keeping one predicate (02).

## Scope

Full audit. Head `8b6a8d6f012e185fc723357ab82bcf643f335930` (checked out, tree clean). Base `origin/main` `eed7195c0b6fb583ab4271173b7a4ad022851c5f`, which equals `git ls-remote origin refs/heads/main` and is an ancestor of the head, so it is merged. The diff matches the handovers:
- 2 production files: `TrailingCommaCodeFixHelper`, `TrailingCommaRemovalCodeFixProviderBase`.
- 8 test files.
- The specs, `requirement-ids.txt`, and the change doc `sdd/changes/2026-11/trailing-comma-branch-end.md`.
- `sdd/work/`.

`scripts/lane-check.sh behavior-change` reports: confirmed. I trusted the build results of the cleaner (06) and the implementer (05), and the focused test results of 04 and 05.

Probe sweep, run on the head with fixtures in the scratchpad outside the repository: `scripts/apply-fix.sh RH5409` over 9 fixtures, each under LF and CRLF. 16 arms ended `fixed (1 action, 1 iteration)` and 2 ended `no-fix-offered`, with no drift and no invalid result. The fixtures:
- comma in front of a multi-line `/* */` comment and then `#endif`;
- `#endif // X`;
- `#endif` between two `#pragma warning` directives;
- `#else` whose excluded branch holds a `#region` pair;
- an `#if` nested two levels deep inside an excluded `#else` (no fix offered, as SHARED-TRAIL-002 requires);
- an `#elif`/`#elif`/`#else` chain;
- a comma on its own line behind an `#if` in its leading trivia;
- blank lines around the `#endif`;
- an empty `#else`.

I then ran `scripts/format.sh` over each fix result. The only change in every file was the final newline, plus the blank-line collapse and the `#endregion // R` naming that the probe inputs themselves called for. Each removal therefore leaves the line the way the formatter leaves it (SHARED-FIX-007).

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `src/Reihitsu.Analyzer/Base/TrailingCommaAnalyzerBase{TNode}.cs:73-75`; `tests/Reihitsu.Analyzer.Test/Formatting/RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzerTests.cs:1022` (summary of `VerifyCommaInFrontOfElseAndElifBranchesIsReportedWithoutFix`) and `:1186` (summary of `VerifyCommaInFrontOfInactiveIfFalseBlockIsReportedWithoutFix`) | FC-031 | – | PR-introduced | These comments state the withholding rule from before the change, which the run made false. Analyzer base: "a comma … is reported also when a conditional directive or excluded code follows it, and the fix withholds itself there". Under SHARED-TRAIL-004 the fix withholds itself only for an `#if` in the gap. Test at 1022: "Verifies that "#else" and "#elif" branches after the comma withhold the fix". The fix is withheld there only because of the `#if SYMBOL` directly behind the comma, and the new `VerifyCommaInFrontOfElifAndElseBranchesWithoutIfIsReportedAndFixed` asserts the opposite for `#elif`/`#else`. Test at 1186 names "excluded code" in the gap as what withholds the fix, but the `#if false` does. Members that hold: the rewritten summary of `CommaPrecedesIfDirective` and the inline comment in `RegisterCodeFixesCoreAsync`; the summary at line 1000 (an `#if`/`#endif` pair); the RH5410/RH5411 summaries of `…WithConditionalBlockAfterFinalItemIsReportedWithoutFix` and of the Fix All tests ("conditional block" means an `#if` block); the `StructuralTransformPhaseIntegrationTests` summaries (formatter, unchanged under answer C). All three locations are comments (`//` and XML `<summary>`), so the finding cannot reach compiled behavior, diagnostics, or rule metadata. | Reword the three comments so the `#if` in the gap is what withholds the fix (SHARED-TRAIL-002) and an `#elif`, `#else`, or `#endif` alone leaves it offered (SHARED-TRAIL-004). Leave the code unchanged. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Fix withheld for a trailing comma (`TrailingCommaCodeFixHelper.CommaPrecedesIfDirective`) | `[comma.Span.End, nextToken.SpanStart)`: the comma's trailing trivia and the closing brace's leading trivia, descending into structured trivia, filtered by `gap.Contains(trivia.SpanStart)`; withholds on `IfDirectiveTrivia`, whether active or parsed inside an excluded branch | `SyntaxTriviaUtilities.ContainsConditionalCompilationBoundary` in `EnumTrailingCommaRemovalTransform:55` and `InitializerTrailingCommaRemovalTransform:58`, over `[lastSeparator.Span.End, CloseBraceToken.SpanStart)` | The span is the same, and it is the gap SHARED-TRAIL-002 defines. The kind sets differ on purpose: the formatter keeps the comma for all four conditional kinds plus disabled text (maintainer answer C, FMT-STRUCT-019), while the fix withholds only for `#if`. | ok. Near boundary: an `#if` in the comma's leading trivia is outside the gap, pinned by `VerifyCommaOnOwnLineInFrontOfEndIfIsReportedAndFixed` (fixed). Far boundary: the closing brace's trailing trivia starts after `gap.End`. An `#if` in excluded code is parsed as `IfDirectiveTrivia`, pinned by `VerifySpecificationExampleIfInExcludedCodeWithholdsTheFix` and `VerifyIfInCodeAnElifExcludesWithholdsTheFix`; the probe with the `#if` two levels deep gave `no-fix-offered`. Safety argument traced: with no `#if` in the gap, every `#elif`/`#else`/`#endif` there belongs to a group open at the comma, so every build that compiles the comma takes the same branches and reads the same text up to the brace. The only exception is SHARED-FIX-012's decided gap, and hiding carriers are refused by `ReihitsuCodeFixProviderBase`. |
| Fix All | the same guard, because `WellKnownFixAllProviders.BatchFixer` registers through `RegisterCodeFixesCoreAsync` | — | — | ok. `VerifyFixAllRemovesCommasAtTheEndOfActiveBranches` and `VerifyFixAllRemovesCommaInFrontOfEndIfAndKeepsCommaInFrontOfIf` pin it |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether the trailing-comma fix is offered with directives in the gap | `rg -n "CommaPrecedes\|ContainsConditionalCompilationBoundary" src tests`: one caller, `TrailingCommaRemovalCodeFixProviderBase:75`, which calls `TrailingCommaCodeFixHelper.CommaPrecedesIfDirective`. `rg -il "trailing comma\|TrailingComma" src`: no other code fix removes a trailing comma by editing the text. | one owner, ok |
| Whether the formatter keeps the trailing comma | `SyntaxTriviaUtilities.ContainsConditionalCompilationBoundary`; its callers are the two structural transforms | one owner, unchanged. It is a different decision from the fix's by maintainer answer C, stated in FMT-STRUCT-019 and SHARED-TRAIL-004, so this is not FC-015 |
| `IfDirectiveTrivia` checks elsewhere (`rg -n IfDirectiveTrivia src`) | `SyntaxTriviaUtilities:347,675`, `OrderingDeclarationUtilities:624`, `ConditionalGroupUnitUtilities`, `DirectivePairSplitScan`, `ReihitsuFormatterHelpers:238`, `RH2001…:500`, `RH7004…:197-199`, `RH8201…:53` | each answers a different question; none is a copy of the trailing-comma decision |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5409/RH5410/RH5411 `…IsReportedAndFixed`, `…RemovesComma…`, `…IsFixedWhereTheSymbolIsDefined` (SHARED-TRAIL-004) | The diagnostic is on exactly the marked comma, and the fix result is byte-exact, with every directive and every excluded line kept | A fix withheld, any other edit, a lost directive or excluded line, or a second diagnostic | `Verify` (code-fix verifier with iteration and Fix All); `VerifyUnderBothLineEndings` or explicit CRLF variants for the spec and change-doc examples; a `SolutionTransforms` parse-option transform for the symbol cases (the marked span sits behind `B`/`2` only when the symbol is defined, so the transform is shown to apply) | ok |
| `VerifyFixAllRemovesCommasAtTheEndOfActiveBranches`, `VerifyFixAllRemovesCommaInFrontOfEndIfAndKeepsCommaInFrontOfIf`, RH5410 `VerifyInnerCommaInFrontOfEndIfAndOuterCommaAreReportedAndFixed` (FC-008) | Fix All removes every fixable comma in one document and leaves a withheld comma reported and in place | A stale position, or a removed withheld comma | `Verify`, with markup allowed in the fixed and batch-fixed states | ok |
| RH5409 `…FurtherIfIsReportedWithoutFix`, `…IfInExcludedCodeWithholdsTheFix`, `VerifyIfInCodeAnElifExcludesWithholdsTheFix`, and the three `…WithConditionalBlockAfterFinal…WithoutFix` (SHARED-TRAIL-002) | Reported, and no action registered for any reported diagnostic | Any action registered | `VerifyNoCodeFixIsOffered` (analyzer verify, then a direct `RegisterCodeFixesAsync` check of every diagnostic) | ok |
| The four `…KeptByTheFormatter` tests (FMT-STRUCT-019/020, answer C) | The analyzer reports exactly the marked comma; formatting changes nothing under LF and CRLF; no phase alone changes it | The formatter removes the comma or moves anything | `VerifyFormatterKeepsReportedShape` (exact equality with the source stripped of markup, plus `PhaseStabilityCheck`) | ok |
| RH5303 `VerifyCodeFixKeepsTrailingCommaInFrontOfEndIfBehindActiveElseBranch` | A formatter-delegating fix keeps `3,` | The comma is dropped | `Verify`, exact output | ok |
| `StructuralTransformRequirementTests.SpecificationExampleOfEnumTrailingCommasKeepsCommasInFrontOfConditionalDirectives` | The whole FMT-STRUCT-019 example, `Mode` and `Level` included | Any difference | `AssertRuleResult` (LF, CRLF, second pass, phase stability) | ok |

## Failure classes dismissed
- FC-002: the spec's qualifier "between the comma and the closing brace" is the explicit span, and "no `#if`, neither read nor excluded" is the explicit kind test.
- FC-005: the fix's guard covers directives only. Comments never withheld the fix and still do not (`VerifyCommaWithCommentInFrontOfEndIfIsReportedAndFixed`).
- FC-006, FC-022–FC-026, FC-029, FC-036, FC-018: the edit deletes one comma's text (plus the whitespace in front of it at the end of the line, or the comma-only line). It touches no literal, identifier, rename, or expression body.
- FC-007, FC-009: the fix converges in one iteration in every test and in every probe arm, and the removal always changes the text.
- FC-010, FC-011, FC-012, FC-038–FC-043: no formatter phase, precheck, or unsupported shape changed. The fix does not format through the formatter, so SHARED-FIX-010 and FMT-UNSUP-009 (a comma on its own line behind a directive) do not apply to it. The same comma-only-line shape in front of a `#region` was already fixed on `main`.
- FC-013, FC-014: the analyzer reports a comma that the formatter keeps, which maintainer answer C decides. SHARED-DIAG-010 does not cover these rules, because their fix edits the text directly (the second paragraph, stated in SHARED-TRAIL-004 and FMT-STRUCT-019). The same disagreement already exists on `main` for a comma in front of `#if`.
- FC-015, FC-016: see *Policy ownership*. No sibling fix removes trailing commas by itself, and the formatter-delegating fixes are unchanged by answer C, pinned by RH5303.
- FC-001, FC-003, FC-004, FC-019, FC-020, FC-021, FC-027: traced under *Guard scope* and covered by the tests and probes above: both sides of the `#if` boundary, a comment on the comma, a multi-line comment, `#pragma` and `#region` mixed with `#endif`, two nested `#endif`, a nested initializer, and blank lines around the directive.
- FC-008: covered (*Assertion adequacy*).
- FC-017: every new assertion compares exact text under both line endings where the spec or change doc gives the example. The remaining tests are LF only, but they reach the same text edit, whose CRLF behavior the paired tests pin.
- FC-028, FC-030: no formatter entry point is involved.
- FC-032: the RH5303 result was checked on the head (by 05's analyzer run and by its test).
- FC-033, FC-034, FC-035: this is a behavior change, not a reproduction. Expected values come from the spec and change-doc examples, and the "before" values were observed on `main` (01, 02).
- FC-037: no text-only claim is made.
- FC-044: the guard reads directive kinds only. It evaluates no condition and pairs no directives.

## Architecture findings
_None._ The fix and the formatter decide this comma differently by maintainer answer C, which FMT-STRUCT-019 and SHARED-TRAIL-004 record. By `README.md`, *Architecture findings*, an analyzer/formatter disagreement is not by itself an architecture finding.

## Hints
- `src/Reihitsu.Core/SyntaxTriviaUtilities.cs:446-458`: the summary of `ContainsConditionalCompilationBoundary` justifies withholding on every conditional directive with the argument that disabled text "can become active and follow that element without a separator". SHARED-TRAIL-004's Why now shows that this does not hold for an `#elif`/`#else`/`#endif`-only gap, where the formatter keeps the comma by maintainer decision instead (FMT-STRUCT-019 Why). The method is outside the diff and its behavior is what its callers need, so I do not count it as a finding. The cleanup for finding #1 could say so in one clause.
- The run adds two more copies of the inline parse-option `SolutionTransforms` lambda (RH5410/RH5411 tests) and a private `VerifyFixWithPreprocessorSymbols` (RH5409 tests). This follows an existing pattern of about 25 inline copies on `main`. `AnalyzerTestsBase<TAnalyzer, TCodeFix>` has no `Verify` overload that takes symbols.

## Follow-up candidates
_None._
