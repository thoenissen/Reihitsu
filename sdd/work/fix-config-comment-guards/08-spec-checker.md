Role: spec checker
Commit: 2231d24
Verdict: consistent

The prompt carried no intended fix, suspected cause, or verdict; none was used. Inputs: `00-lead.md`, `02`/`04-spec-author.md`, `05-test-implementer.md`, `git diff 57acf0d..HEAD`.

## thoenissen/Reihitsu#1595

### Findings
_None._

### Checked
- Check 1: no change doc is needed for this issue on its own. RH7102-006 and SHARED-ORDER-007 state that the fix withholds where SHARED-FIX-003 and the unchanged SHARED-ORDER-007 ("a member reordering fix") already required it (decisions 4, 5, 12, 16 of `00-lead.md`). Observation, not a finding: the RH7102-006 normative sentence now names SHARED-ORDER-007, so the lead's reading "clarification of an unchanged requirement" is what carries it; the maintainer may read it as a changed requirement.
- Check 2: the diff (guard for every member-moving fix, split `const` no longer reported by RH7102/RH7103) matches RH7102-006, SHARED-ORDER-007, and the unchanged RH7102-001 / SHARED-DIAG-008. Format holds (`Why:` present, examples with `{|RH7102:K|}` markup).
- Check 3: no new ID for this issue.
- Check 4: `SpecConsistencyTests` pass inside the architecture run; spec headers touched (RH7102 `Uses`) name existing owners; `sdd/specs/README.md` unchanged and still matches.
- Check 5: no rule page is affected (`documentation/rules/RH7102.md` untouched; the withheld cases are covered by `general-notes.md`).
- Check 6: no open output question; the standing answer and decisions cover it.
- Check 7: no new requirement. The tests carry `[Requirement("RH7102-006")]`/`SHARED-ORDER-007` (and `RH7102-001` + `SHARED-DIAG-008` for s1/s2).
- Check 8: every test in the `Tests` table carries the tags the table lists.

## thoenissen/Reihitsu#1593

### Findings
_None._

### Checked
- Check 1: no change doc needed; the split-member counting in SHARED-ORDER-007 is a clarification of the unchanged requirement (decisions 6, 13).
- Check 2: the added bullets and examples (RH7109 `Log("b")` split initializer, RH7309 split `static`, RH7309 counterpart that keeps the fix) match the tests and the guard in `OrderingDeclarationUtilities`; "leading trivia or inside it" removes the earlier contradiction.
- Check 3: no new ID.
- Check 4: ok (see #1595).
- Check 5: no rule page affected.
- Check 6: ok; the lone-`;` column question is under Follow-up candidates (decision 15), not in the specs.
- Check 7: no new requirement; RH7309-006, SHARED-ORDER-007, RH7109-006, SHARED-DIAG-008 tags are on the added tests.
- Check 8: ok.

## thoenissen/Reihitsu#1616

### Findings
_None._

### Checked
- Check 1: `sdd/changes/2026-11/rh2001-modifier-reorder-trivia.md` is required (RH2001-009 withdraws a fix offered today; RH2001-004 is changed). Status `Accepted`, has `Visible output changes` (Before/After for each), `Decisions` records maintainer answers batch 1 Q1 A, Q2 A, batch 2 A. Every listed visible change has an answer. No task list.
- Check 2: RH2001-004 and RH2001-009 match the change doc and the tests (examples, `{|RH2001:…|}` markup, `Why:` present). `Uses` lists RH7105-001 and SHARED-MODORDER-002, both existing.
- Check 3: `RH2001-009` is the next free number and sits in sorted position (after RH2001-008, before RH2002-001); nothing reused or removed.
- Check 4: ok; RH2001 owners unchanged.
- Check 5: `documentation/rules/RH2001.md` is unaffected (no corner-case text, no formatter mention; the withheld fix is covered by `general-notes.md`). Its Correction `private int _count;` still equals the fix output.
- Check 6: both batches answered under `Maintainer answers`; none written into the spec as open.
- Check 7: new active requirement RH2001-009 has covering tests (`VerifyNoFixIsOfferedForACommentThatAReorderedModifierWouldCross`, `VerifyFixKeepsACommentThatNoModifierCrosses`, and others).
- Check 8: every test carries its tag; the Fix All test carries RH2001-004, RH2001-009, SHARED-FIX-002 as the table lists; the requirements named under `Examples for the tests` (RH2001-004, RH2001-009) are tagged on tests the diff adds.

## Whole run

- Check 7 command: `scripts/test.sh --project architecture` -> `Passed!  - Failed: 0, Passed: 115, Skipped: 0, Total: 115` (includes `SpecConsistencyTests` and `RequirementCoverageTests`).
- Check 9: `sdd/work/` holds only `fix-config-comment-guards`; the lead deletes it before publishing.
- Follow-up candidates in `00-lead.md` are all outside the run's issues and recorded there; none is written into a spec.
- No findings for any role.
