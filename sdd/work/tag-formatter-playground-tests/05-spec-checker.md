Role: spec checker
Commit: ebc5423
Verdict: findings

The diff is `git diff origin/main...HEAD` (merge base 8e0f35d; origin/main has since moved to d70f487). It touches no file under `sdd/specs/`, `sdd/changes/`, `documentation/`, or any production project. The only non-test-project files are `src/Reihitsu.ArchitectureTests/Sdd/RequirementCoverageTests.cs`, `squad/decisions/2026-10-22-939-playground-component-tests.md`, and this run's `sdd/work/` files. The prompt carried no intended fix or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 2, 7 | `sdd/README.md` lines 76 and 79 (*Tests and requirements*) | The sentence "The attribute is declared in `Reihitsu.Analyzer.Test`, `Reihitsu.Cli.Test`, and `Reihitsu.Core.Test`" is now false: the diff adds `RequirementAttribute` to `Reihitsu.Formatter.Test` and `Reihitsu.Playground.Test`. "Enforced today: `analyzer/`, `cli/`, `shared/code-fixes.md`, `shared/diagnostics.md`, ..." is stale: `EnforcedSpecPaths` now also lists `formatter/` and `playground/`. Both implementers and the lead already flagged this. The file is outside the tests/tooling lane and other squads edit it in parallel (origin/main added 23 lines to it), so it is a text change for the lead to route (spec author or the lead after merging main), not a test implementer fix. | lead / spec author |

No other finding.

## Checked
1. Change doc exactly when required: ok. The run changes no intended behavior: no requirement is added, changed, or retired, and no rule, phase, or surface is added or removed. No change doc is required and none is in the diff.
2. Specs match the diff: finding 1 only. The run edits no spec text. No requirement contradicts a test: every tagged test passes on unchanged production code (cleaner: formatter 2664, playground 17, architecture 72). Spot check of tests that the group B and C handovers list as untagged because a filed question disputes their input (`KeepsMemberAfterMultiLineCommentEndingMidLine`, `AccessorAfterMultiLineCommentEndingMidLineRemainsUnchanged`, `CommentInSignatureGapIsNotCrossed`, `ElementAccessWithoutMemberAccessKeepsItsBreak`, `SeparatesMembersInExtensionBlock`, `PreservesCommentBeforeArrow`, `DoesNotInsertBlankLineAboveBlockStatementAfterClosingBrace`): none carries a `[Requirement]`. `RecursivePatternAlignmentTests.ParenthesizedRecursivePatternBreaksParenthesisOntoOwnLine` carries FMT-INDENT-026 (its spec example), not FMT-BREAK-022 or FMT-SWITCH-004, so it does not pin the LB-Q7 dispute under the disputed IDs.
3. IDs: ok. `sdd/specs/requirement-ids.txt` is unchanged; no ID added, reused, renumbered, or deleted. `origin/main` added only SHARED-FIX-006 to -008 since the base; no FMT or PLAY ID.
4. Code owners: ok. `SpecConsistencyTests` passed inside the full architecture run; no owner type is renamed, added, or removed (no production change), and `sdd/specs/README.md` is unchanged and still correct.
5. User documentation: ok. No rule page is affected; no analyzer, code fix, or diagnostic changes.
6. Open questions recorded: ok. The ten per-spec skeptic question sections (indentation, cleanup, raw-string-alignment, pipeline, line-breaks, switch-case-braces, horizontal-spacing, structural-transforms, blank-lines, documentation-comments) map one to one to the issues the lead lists (#1370, #1364, #1365, #1366, #1373, #1368, #1369, #1371, #1372, #1367). The skeptic handovers hold no question section for line-endings, playground, region-formatting, or using-directives, and the lead's list names none. The test implementer's FMT-INDENT-015 finding is filed as Q-I8 (comment on #1370, read on GitHub: text matches the finding). #1373 was read and holds LB-Q1 to LB-Q10. The further-instance comments on existing follow-ups are the lead's record; I did not re-read each of them.
7. Coverage: ok. `scripts/test.sh --project architecture`: "Passed! - Failed: 0, Passed: 72, Skipped: 0, Total: 72". `EnforcedSpecPaths` now lists `formatter/` and `playground/`, so `EveryRequirementOfAnEnforcedSpecIsCovered` proves every active FMT and PLAY requirement (and the SHARED ones the formatter specs list under `Uses`) has a covering `[Requirement]` test in the diff. `EveryRequirementReferenceNamesAnActiveRequirement` passes. The changed `CoverageReportCountsUncoveredRequirementsPerSpecNotYetEnforced` keeps its assertions; only the fixture moved to `shared/casing.md`. Merging origin/main later brings only a `Why:` edit of FMT-PIPE-010 and a `Uses` line in pipeline.md and no new FMT or PLAY requirement, so it cannot leave a requirement uncovered.
8. Nothing left behind: ok for the branch contents. `sdd/work/` holds only `tag-formatter-playground-tests/`, which the lead deletes before publishing. The `squad/decisions/` file is a recorded decision, not working material.

## Repair round
Commit: 022968c
Verdict: consistent

Scope: `git diff f88035c HEAD` (the test implementer's repair 963611e and the cleaner's 022968c). It changes three test files (`PipelineInvariantTests.cs`, `ReihitsuFormatterEntryPointTests.cs`, `PlaygroundFormatterTests.cs`) and two handovers. No spec, change doc, production file, rule page, or `requirement-ids.txt` is touched, so checks 1, 3, 4, and 5 are unaffected. The `sdd/README.md` finding stays recorded by the lead as a follow-up owned by another squad.

- **Tags (checks 2, 7):** the diff adds and removes no `[Requirement]` line and no `[TestMethod]`. The renamed test `EveryCommentDirectiveDisabledTextAndLiteralTextSurvives` keeps its FMT-PIPE-002 tag, and no reference to the old name remains. FMT-PIPE-002, FMT-PIPE-012, and PLAY-005 each still have covering tagged tests.
- **Agreement with the requirement text:** the strengthened FMT-PIPE-002 test compares the literal token text (value text only for multi-line raw string content, which FMT-RAW may re-indent) and disabled text without line-ending normalization, as FMT-PIPE-002 and FMT-PIPE-007 state. The FMT-PIPE-012 test adds the `indentLevel: 0` case ("zero or greater"). The PLAY-005 tests add a C# 14 extension block, an empty class, and a get-only property, matching "Roslyn's default parse options" and the default language version of FMT-PIPE-008. None of the three pins a disputed input.
- **Coverage (check 7):** `scripts/test.sh --project architecture`: "Passed! - Failed: 0, Passed: 72, Skipped: 0, Total: 72", so `EveryRequirementOfAnEnforcedSpecIsCovered` and `EveryRequirementReferenceNamesAnActiveRequirement` still hold.
- **Nothing left behind (check 8):** unchanged; `sdd/work/` holds only this run's directory.

No new findings.
