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
- Next: spec author repair, then spec checker again, then auditor.
