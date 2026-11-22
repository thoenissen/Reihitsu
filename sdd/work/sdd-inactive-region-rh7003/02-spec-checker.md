Role: spec checker
Commit: 4cde65f
Verdict: consistent

No intended fix, suspected cause, or hoped-for verdict was in the prompt.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

## Checked

### thoenissen/Reihitsu#1240
1. ok: no change doc needed; a non-goal is narrowed, no requirement changes.
2. ok: the new non-goal in `sdd/specs/shared/diagnostics.md` agrees with SHARED-DIAG-008 and the region-rule specs.
3. ok: no IDs added or changed.
4. ok: no spec header touched.
5. ok: no rule page affected.
6. ok: no open output questions.
7. ok: `scripts/test.sh --project architecture`: Passed 198, Failed 0; no new requirement.
8. ok: no tests added.
9. ok: only this run's `sdd/work/`.

### thoenissen/Reihitsu#1591
1. ok: no change doc needed; reported behavior unchanged.
2. ok: each statement of the new `## Description` in `documentation/rules/RH7003.md` matches RH7003-001, -002, -003, -005 (unchanged).
3. ok: no IDs touched.
4. ok: no spec touched.
5. ok: page is short, no formatter mention, Exceptions section gone, footer kept, example unchanged, no code fix so no `### Correction` change; ID in the Organization range and page exists.
6. ok: no output questions.
7. ok: see above.
8. ok: no tests added.
9. ok: only this run's `sdd/work/`; the lead deletes it before publishing.
