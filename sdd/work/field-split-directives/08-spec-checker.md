Role: spec checker
Commit: c4c7a37
Verdict: #1728: consistent; #1586: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1728

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | None | - |

### Checked
1. ok: `sdd/changes/2026-11/field-declaration-directives.md` is required (new FMT-UNSUP-010, a retired requirement, changed output), has `Visible output changes` and `Decisions`, no task list, `Status: Accepted`; both visible changes carry the maintainer's answers (escalation 10, question 2 A).
2. ok: FMT-UNSUP-010 (format, `Why:`, both-sided examples), FMT-UNSUP-009's tie reason, FMT-PIPE-031, SHARED-FIX-011, RH7101-005 and the `AGENTS.md` PrecheckPhase row (fourth shape) match the diff and the change doc's Affected requirements.
3. ok: `FMT-UNSUP-010` is the next free number, inserted at its sorted position in `requirement-ids.txt`; `FMT-STRUCT-023` stays in the ledger and is retired in place (`Status: Retired`). The remaining mentions of it are in older change docs (history).
4. ok: no code owner added, renamed, or removed; `SpecConsistencyTests` ran inside the architecture run; `sdd/specs/README.md` unchanged and still matches.
5. ok: no rule page is affected (`documentation/rules/RH7101.md` says nothing about directives; the change doc says why no page changes).
6. ok: both output questions are answered under `Maintainer answers` in `00-lead.md` (1A, 2A); none sits in a spec; the one follow-up outside the run (a declarator behind an `#endif` loses its continuation indent) is under `Follow-up candidates`.
7. ok: `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 153, Skipped: 0, Total: 153`. FMT-UNSUP-010 is tagged on 32 added or changed tests, so it is covered.
8. ok: all 58 test names in the `Tests` table (`05-test-implementer.md`) exist in the diff and carry a `[Requirement]` tag. `DirectiveAtEveryPlaceOfTheDeclarationIsUnsupported` has its tag above its `[DataRow]` list. The requirements under `Examples for the tests` (FMT-UNSUP-010, FMT-PIPE-031, RH7101-005) are tagged.
9. ok: `sdd/work/` holds only `field-split-directives/`; the lead deletes it before publishing.

## thoenissen/Reihitsu#1586

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | None | - |

### Checked
1. ok: covered by the same change doc; the output change "A directive in front of the declaration no longer keeps it whole" is answered by question 1 A.
2. ok: FMT-STRUCT-022 (directive stays on the first field; disabled text in the leading trivia keeps the declaration whole), FMT-STRUCT-024, FMT-PIPE-029, FMT-PIPE-031 (second example) and RH7101-005 (region/`#if !RELEASE` example with the fixed state; disabled-text example) agree with the diff and with each other.
3. ok: no new ID; FMT-STRUCT-022 changed in place.
4. ok: owners unchanged (`FieldDeclarationSplitTransform` already listed).
5. ok: no rule page change needed (RH7101's page, `### Correction`, and footer are untouched and still true).
6. ok: see #1728 (answers 1A, 2A recorded).
7. ok: FMT-STRUCT-022 is tagged on 10 added tests, FMT-PIPE-031 on 2, RH7101-004/005 on 3 and 9; the architecture result is quoted under #1728.
8. ok: see #1728; the `Tests` table rows for #1586 all match tagged tests.
9. ok: see #1728.

## Whole run

- Architecture: 153/153 passed on c4c7a37.
- Not re-run, because outside my checks: formatter, analyzer, CLI, and tooling suites (the implementer and cleaner handovers hold them).
- Follow-up candidates: none new.
