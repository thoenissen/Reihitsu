Role: spec checker
Commit: d04d169f
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Checked the whole run (`git diff 57acf0d..HEAD`), not only the last range.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## thoenissen/Reihitsu#1595
- RH7102 spec (RH7102-001, RH7102-006) names SHARED-FIX-003, SHARED-ORDER-007 and SHARED-ORDER-008; RH7102-001 states the judged span (modifier list) per SHARED-DIAG-008, matching the analyzer change (decision 16). No change doc is needed for it (decision 19: withholding is demanded by unchanged SHARED-FIX-003). The base-class guard (decision 4) is stated by SHARED-ORDER-007/-008 for every member-moving fix; the `IsMoveSafe` summary names SHARED-ORDER-008.
- Tests carry `RH7102-006` (20), `RH7102-001` (2), `SHARED-ORDER-008`, `SHARED-DIAG-008`.
- The `#if false` test is green again under main's whole-member reading.

## thoenissen/Reihitsu#1593
- SHARED-FIX-009 and SHARED-ORDER-008 added; SHARED-ORDER-006/-007, RH7102-006, RH7103-007, RH7109-006, RH7309-005 changed. The spec no longer mentions the per-configuration machinery (no match for symbol assignment, configuration limit, `OrderedMember`, `ConditionalGroup` in `sdd/specs`; the change doc names them only as the replaced reading). `OrderedMember.cs` is gone from `src`.
- Change doc `member-move-conditional-groups.md`: Accepted, has *Visible output changes* and *Decisions* (batches 3, 4, 5, 6, accessor backing field, comment-only branches, replaced parts); every listed output change has a recorded maintainer answer in `00-lead.md` (batches 3-6, standing answer). No task list.
- Tests: `SHARED-ORDER-008` (81), `SHARED-FIX-009` (3), `SHARED-ORDER-007` (28), `RH7109-006` (64), `RH7103-007` (12), `RH7309-005` (10), `RH7309-006` (5). Each new requirement has at least one covering `[Requirement]` test; the tags match the `36-test-implementer.md` tables (spot-checked).

## thoenissen/Reihitsu#1616
- RH2001-004 changed, RH2001-009 added; change doc `rh2001-modifier-reorder-trivia.md` Accepted, with Decisions for batch 1 Q1/Q2, batch 2 and decision 36 (readonly behind a conditional group, standing answer). Its measured main/head outputs for the group are recorded.
- Tests: `RH2001-009` (8), `RH2001-004` (10); the decision 36 example is pinned in `VerifyFixIsOfferedForAConditionalModifierBetweenTheModifiersAndTheType` (full output).
- The rule page `documentation/rules/RH2001.md` is unchanged and still consistent (Correction `private int _count;` equals the fix output; no corner case, no formatter mention).

## Whole run
- Verdict: consistent, no findings.

## Checked
1. Change doc exactly when required: ok. Two docs (`member-move-conditional-groups.md`, `rh2001-modifier-reorder-trivia.md`), both Accepted, both with *Visible output changes* and answered *Decisions*. None for RH7102 withholding or the RH7102/RH7103 analyzer fix (bug fixes against unchanged SHARED-FIX-003, SHARED-DIAG-008, RH7102-001).
2. Specs match the diff: ok. Format spot-checked (one requirement per heading, `Why:`, examples with markup).
3. IDs: ok. `requirement-ids.txt` gains RH2001-009, SHARED-FIX-009, SHARED-ORDER-008 at sorted positions; nothing reused, renumbered, or deleted.
4. Code owners exist: ok. `scripts/test.sh --project architecture` (includes `SpecConsistencyTests`): `Passed!  - Failed: 0, Passed: 115, Skipped: 0, Total: 115`. `sdd/specs/README.md` row for RH2001 lists existing types; ordering-move-safety owners exist.
5. User documentation: ok. No `documentation/` change in the diff; none needed.
6. Open questions routed: ok. All output questions of the spec author's and skeptic's handovers (33-38) have answers in `00-lead.md` (batches 3-6, standing answer, decisions 33-36); out-of-run items are under *Follow-up candidates*.
7. Requirements covered and specs consistent: ok (result line above).
8. New tests carry their requirement's tag: ok (counts above; decision 36 example tagged `RH2001-009`; examples for the tests of 33/38 are tagged on tests the diff adds or retags).
9. Nothing left behind: ok. Only `sdd/work/fix-config-comment-guards/` is under `sdd/work/`; the lead deletes it before publishing.

Note for the lead (not a finding): `00-lead.md` *Worklist* is current through the cleaner; decisions 21-31 are marked superseded by 32 only in decision 32's text.
