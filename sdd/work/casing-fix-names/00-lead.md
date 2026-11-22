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

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Start | Split the batch (escalation 8)? #1434 changes reporting, the others the fix's name derivation, and #1434 names six rules. | Keep one run. | All four sit in the shared casing component: the rules all derive from `CasingAnalyzerBase`, the fixes share `CasingUtilities`, and all change `sdd/specs/shared/casing.md`. The maintainer composed the batch. | Process only. | claim |

## Follow-up candidates

(none yet)

## Worklist / next step

- Claim done (empty commit, draft PR #1934), SDK installed.
- Next: spec author (change doc + spec diff for all four issues).

## Progress log

- Spec author done (`8939ef7`, `a351dd2`): change docs written for all four; nine output questions (handover `01-spec-author.md`, *Whole run*). New IDs SHARED-CASING-013 (keyword `@`) and SHARED-CASING-014 (dictated names).
- Follow-up candidate: RH4120 positional record parameter whose generated property implicitly implements a referenced-assembly property is still reported (from `01-spec-author.md`).
- Next: skeptic, then output-question batch.
