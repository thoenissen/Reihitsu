Role: spec checker
Commit: 5d25217f
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md`, handovers 01 to 11, and `git diff 158e8fc..HEAD`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 5 | `documentation/rules/RH7101.md` line 20; change doc visible change 7 ("Rule pages: None") | The page says the code fix "carries the comments written on the declaration and on the individual variables over to the fields it creates". RH7101-007 now withholds the fix for a comment inside the declaration (at a name, an `=`, a `,`, in front of a later declarator), and FMT-UNSUP-004 refuses such input. The sentence promises a carry-over the fix no longer offers, and the change doc's claim that no rule page changes is not true. Reword it so it names only what is carried (comments on the declaration, i.e. around it) without a list of withheld cases, per `create-rule-doc`. | spec author |
| 2 | 8 | `FieldDeclarationSplitTests.DirectiveBeforeFirstDeclaratorLeavesTheFieldIntact` and `DisabledTextBeforeFirstDeclaratorLeavesTheFieldIntact` (`[Requirement("FMT-STRUCT-023")]`); `ReihitsuFormatterSplitsFieldDeclarationTests`-area test `CombinedStaticInterfaceFieldsAreSplit` (`[Requirement("FMT-STRUCT-021")]`) | The diff adds these tags, but `05-test-implementer.md` and `08-test-implementer.md` name FMT-STRUCT-023 and FMT-STRUCT-021 in no `Tests` table row. The table must name every requirement a test the diff adds or newly tags carries. | test implementer |

## Checked
1. Change doc: ok. It is required (new outcome, new CLI option, new spec, new result shape). It has no task list, has `Visible output changes` (1 to 7), and is `Accepted`. Every listed visible change has a `Decisions` entry carrying the maintainer's answers (Q1 to Q11). Visible change 7 is wrong, see finding 1.
2. Specs match the diff: ok. Every requirement the diff touches is in the change doc's `Affected requirements`. (CLI-018, PLAY-007, and SHARED-FIX-009 show as touched only because the new requirement follows them.) New FMT-UNSUP-001 to -007, CLI-019, CLI-020, PLAY-008, and SHARED-FIX-010 each have the normative sentence and `Why:`, and examples where the behavior is visible. FMT-UNSUP-006 has no example (a refusal produced during a run, no visible input), which I accept.
3. IDs: ok. The 11 new IDs (CLI-019, CLI-020, FMT-UNSUP-001 to -007, PLAY-008, SHARED-FIX-010) are inserted at sorted positions, and nothing is removed or retired.
4. Code owners: ok. `unsupported-syntax.md` header lists `PrecheckPhase`, `FormattingResult<T>`, `UnsupportedSyntax`, `SyntaxNotSupportedException`, and `sdd/specs/README.md` lists the spec with the same owners.
5. User documentation: finding 1. `documentation/rules/RH2001.md` is consistent. The CLI README and `--help` carry `--suppress-unsupported` and exit code 2, matching CLI-001 and CLI-017. The `AGENTS.md` architecture text and phase table are updated.
6. Open questions: ok. Every output question in 01, 02, 03, 04 has an answer in `Maintainer answers` (Q1 to Q11, escalation 9 as Q8). 07 and 09 report none open. The follow-up candidates of 06 are resolved by Decisions 13 and 15, and `00-lead.md` lists none outstanding.
7. Coverage: `scripts/test.sh --project architecture` gives `Passed!  - Failed:     0, Passed:   117, Skipped:     0, Total:   117`. That covers `SpecConsistencyTests`, `SpecStructureTests`, and `RequirementCoverageTests`. Every new active requirement has tagged tests: FMT-UNSUP-001 to -007 (14, 10, 5, 69, 2, 2, 4), CLI-019 (13), CLI-020 (4), PLAY-008 (2), SHARED-FIX-010 (12).
8. Tags: finding 2. All other tags added by the diff (CLI-001, CLI-014, CLI-017, FMT-PIPE-001, FMT-STRUCT-030, RH2001-007, RH2001-009, RH7101-004, -006, -007, SHARED-FIX-002) are named in the tables. The requirements the spec authors named under `Examples for the tests` (FMT-STRUCT-022, FMT-STRUCT-026, FMT-PIPE-029, FMT-PIPE-031, FMT-PIPE-017, FMT-INDENT-015, FMT-BLANK-018, FMT-SPACE-012, FMT-SPACE-014, FMT-DOC-003) are carried by tests that keep their existing tags and whose bodies the diff rewrites or adds, as the 05 table lists; I treat that as covered.
9. Nothing left behind: ok. `sdd/work/` holds only `unsupported-precheck/`, which the lead deletes before publishing.
