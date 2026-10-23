Role: spec checker
Commit: d33799b
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The checks use `git diff origin/main` (26 files: specs, ledger, eight analyzer test files, this run's `sdd/work/`, four decisions entries) and the maintainer's answers in `squad/decisions/2026-10-23-1415-*.md`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| — | — | — | None. | — |

## Observations (not findings)

These are known, decided, and tracked; the lead may want them in the run report.

- Three active requirements now contradict a new one, as the maintainer's Q4 (b) answer accepts until the code follow-ups land:
  - RH3103-006 says a dotted `Nullable<T>` in `nameof` is still reported, while SHARED-DIAG-011 says the construct is not reported. Its two tests keep the RH3103-006 tag and pass on today's code. Tracked by thoenissen/Reihitsu#1417.
  - RH7101-004 keeps the comment on the first field of a split, while the fifth condition of SHARED-FIX-003 withholds a split that keeps documentation on one part only. Tracked by the RH7101 comment on thoenissen/Reihitsu#1336.
  - RH7309-004 has no guard for a `///` comment left behind a moved region, while SHARED-DOC-004 and SHARED-FIX-003 withhold such a fix. Tracked by thoenissen/Reihitsu#1262.
- The tests of SHARED-DIAG-011 and the fifth condition cover only today's conforming shapes. The withheld side of the fifth condition has no test; the test implementer's handover gives the reason.

## Checked
1. Change doc: ok. No change doc, as the issue states and as the precedent runs for P25–P37 did. The spec text records the maintainer's decisions; SHARED-DIAG-011 is new wording of decided principle P38 and the rest are clarifications. See the observations for the code that still differs.
2. Specs match the diff: ok. SHARED-DIAG-011, the fifth condition and **Documentation** bullet of SHARED-FIX-003, the SHARED-FIX-008 clarification, and SHARED-DOC-004 (mode `Parse`/`Diagnose` carry, mode `None` ordinary comment) match the issue as the maintainer's four answers revised it. The four Why-only changes cite SHARED-DIAG-011. Each requirement has the normative sentence first, `Why:`, and examples with `{|RH####:…|}` markup; the RH3103, RH3001 and RH5412 diagnostic locations (generic name, operator, class name) match the examples. No rule page, formatter spec, or code owner changed.
3. IDs: ok. SHARED-DIAG-011 is the next free ID after SHARED-DIAG-010 and sits at its sorted position in `requirement-ids.txt`; nothing was removed, renumbered, or reused.
4. Code owners and `Uses`: ok. `Uses` of `diagnostics.md`, `code-fixes.md`, `element-documentation.md`, RH2004, RH3003, RH3103, RH5412 are exact (`SpecConsistencyTests` passes). No owner was renamed, added, or removed, so `sdd/specs/README.md` is unchanged and correct.
5. User documentation: ok. No `documentation/rules/` page is affected, because no rule behavior changes in this run, and `general-notes.md` stays true. The code follow-ups update the pages.
6. Open questions: ok. The only question, the formatter splitting a documented field, is decided and tracked by thoenissen/Reihitsu#1384 (P36: the formatter refuses the split, change doc required), so nothing is left to file. The follow-ups were filed as comments on #1417, #1262, #1336, #1388 and as thoenissen/Reihitsu#1422, per `00-lead.md`.
7. Coverage and consistency: ok. `scripts/test.sh --project architecture`: `Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72`. SHARED-DIAG-011 is covered by tagged tests of RH3103, RH3003, RH2004, RH5412, RH3001 and RH4106; SHARED-DOC-004, SHARED-ORDER-006 and SHARED-FIX-008 each gained a covering test in the diff.
8. Nothing left behind: ok. `sdd/work/` holds only `principles-p38-p39`, which the lead deletes before publishing. `scripts/lane-check.sh tooling`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
