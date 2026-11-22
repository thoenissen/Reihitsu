# Lead — casing-fix-names

- Branch: `claude/squad-tickets-971-1072-8ywd6y` (session-designated)
- PR: thoenissen/Reihitsu#1934 (draft)
- Slug: `casing-fix-names`
- Lane: behavior change

## Issues

| Issue | agent-filed | Lane | Reason |
|---|---|---|---|
| thoenissen/Reihitsu#971 | yes | behavior change | Leading acronyms in camelCase / `_camelCase` targets (`HTTPClient` → `_httpClient`). Filed as a bug, but triage reclassified it as an extension: SHARED-CASING-006 does not cover names that start with two capitals, so the spec changes. |
| thoenissen/Reihitsu#1072 | yes | behavior change | Casing fixes withheld when the derived name is a reserved keyword (today `Class` → `@class`); changes SHARED-CASING-007/008. |
| thoenissen/Reihitsu#1451 | yes | behavior change | RH4005/RH4121 and casing fixes keep an existing prefix (`i_document` → `IDocument`, `T_Key` → `TKey`) and every identifier character (Mn, Mc, Nl, Pc); changes SHARED-CASING-006 and the RH4005/RH4121 specs. |
| thoenissen/Reihitsu#1434 | yes | behavior change | Casing rules do not report names dictated by metadata (override/implementation of a referenced-assembly member, tuple element names it must repeat) or by a native entry point (`extern` with `DllImport`/`LibraryImport` without `EntryPoint`); new shared casing requirement. Note: the issue proposes the ID SHARED-CASING-011, which is taken today; the ID is the spec author's choice. |

All four goals are proposals (agent-filed). Roles: all of the behavior-change lane — spec author, skeptic, test implementer, implementer, cleaner, spec checker, auditor, validator; issue manager before publishing if there are follow-up candidates.

## Maintainer answers

- Output batch 1 (Q1–Q12, numbering as in the Progress log): the maintainer chose the recommended option for every question — 1A, 2A, 3A, 4A, 5A, 6A, 7A, 8A, 9A, 10A, 11A, 12A.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Start | Split the batch (escalation 8)? #1434 changes reporting, the others the fix's name derivation, and #1434 names six rules. | Keep one run. | All four sit in the shared casing component: the rules all derive from `CasingAnalyzerBase`, the fixes share `CasingUtilities`, and all change `sdd/specs/shared/casing.md`. The maintainer composed the batch. | Process only. | claim |
| 2 | Implementer | Disputed test `RH4106…FixAllSkipsAFieldWithALeadingAcronymThatCollidesWithAnotherField`: the framework demands no Fix All action when Fix All changes nothing. | Test implementer adds a non-colliding field so Fix All changes something; the asserted skip of both colliding fields stays. | The asserted output (both colliding fields skipped, SHARED-CASING-010) is unchanged; only the test shape changes. Offering no Fix All action broke 7 RH4115 tests. | SHARED-CASING-010 decides the output; nothing visible changes. | 4022a0c |
| 3 | Audit 1 | Auditor finding 2 (`ToPascalCaseWord` upper-cases a non-letter first character, e.g. U+0345 → U+0399): repair or restate spec? | Repair the code to the unchanged SHARED-CASING-006 text (kept as written), with tests. | The spec already decides it; the derivation is a cheap check on the character's category. | An unchanged requirement demands the output. | 96f8123 |

## Follow-up candidates

- RH4120: a record's positional parameter whose generated property implicitly implements a property declared in a referenced assembly has a dictated name but is still reported (`01-spec-author.md`, #1434).

## Worklist / next step

- Claim done (empty commit, draft PR #1934), SDK installed.
- Next: spec author (change doc + spec diff for all four issues).

## Progress log

- Spec author done (`8939ef7`, `a351dd2`): change docs written for all four; nine output questions (handover `01-spec-author.md`, *Whole run*). New IDs SHARED-CASING-013 (keyword `@`) and SHARED-CASING-014 (dictated names).
- Follow-up candidate: RH4120 positional record parameter whose generated property implicitly implements a referenced-assembly property is still reported (from `01-spec-author.md`).
- Skeptic done (`ff32292`): #971 output question, #1072 proceed, #1451 proceed, #1434 two output questions. No architecture findings.
- Output batch 1 posted to the maintainer: Q1–Q9 from `01-spec-author.md` (Q1 with skeptic's `URLs` row), Q10 = skeptic #971 Q1 (first-word vs joined acronym run), Q11 = skeptic #1434 Q1 (base-class implementer), Q12 = skeptic #1434 Q2 (tuple names from a type argument).
- Answers received (all A). Spec author follow-up done (`f1a2cff`, `03-spec-author.md`): answers recorded, change docs Accepted, SHARED-CASING-006/014 amended for Q10–Q12.
- Test implementer done (`e33eea6`, `d76fdcb`, `04-test-implementer.md`): 58 analyzer + 39 core tests fail on unchanged code, exactly the new/rewritten ones; architecture green.
- Implementer done (`bc565c2`, `05-implementer.md`): all tests pass but one disputed (decision 2). Full suites green otherwise; self-hosting scan not run.
- Test repair done (`4022a0c`, `06-test-implementer.md`): disputed test passes.
- Cleaner done (`3acd20e`): clean, 0 warnings.
- Spec checker done (`2d321c1`, `08-spec-checker.md`): one low finding — `CasingRenameSafetyUtilities` missing from the code-owner lists in `sdd/specs/shared/casing.md` and `sdd/specs/README.md`. Repair round 1: spec author.
- Spec author repair done (`4da9fe1`, `4e658ab`, `09-spec-author.md`): type listed as SHARED-CASING owner.
- Spec checker repair check done (`8266151`, `10-spec-checker.md`): consistent.
- origin/main 75cc39f, branch not behind; build green.
- Auditor 1 done (`b0074e4`, `11-auditor.md`), audited HEAD 396887f: #971 pass, #1072 pass, #1451 finding 2 (low), #1434 finding 1 (RH4116 operators / conversion operators / multi-declarator events). Both same requirement.
- Repair round 2 (round 1 was the spec-checker code-owner repair): test implementer (tests for findings 1 and 2), then implementer, cleaner, spec checker, auditor on the repair, validator.
- Round 2 test implementer done (`d90597e`, `ff266a7`, `12-test-implementer.md`): 5 RH4116 + 12 core + 2 code-fix tests fail on current code. Next: implementer.
- Round 2 implementer done (`96f8123`, `13-implementer.md`): all green in Debug. Note for the run note: the implementer used `git stash` (no parallel role was running). Next: cleaner, spec checker, auditor on the repair.
- Round 2 cleaner done (`9a22054`): clean. Next: spec checker, then auditor on the repair.
- Round 2 spec checker done (`73fc031`): consistent. Next: auditor on the repair (396887f..HEAD).
