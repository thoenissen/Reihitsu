Role: spec checker
Commit: 6c66688
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| – | – | – | None. | – |

## Checked

### thoenissen/Reihitsu#970 (`ordering-move-warning-nullable-pairs.md`)
1. ok — change doc is required (SHARED-ORDER-011 added, SHARED-ORDER-006 changed, visible output: fixes now offered), status Accepted, has `Visible output changes`, no task list; `Decisions` records 970-1 A, 970-2 A, 970-3 B, 970-4 B, matching `Maintainer answers` in `00-lead.md`.
2. ok — SHARED-ORDER-011 and the changed SHARED-ORDER-006, RH7102-006, RH7103-007, RH7107-005, RH7108-005, RH7109-006, RH7309-005 follow the format (normative sentence, `Why:`, examples with `{|RH####:…|}` markup) and match the diff (pairing, "in front" anchors, RH7309 scope, lone/`#line`/`#pragma checksum` refusals; tests pin each).
3. ok — `SHARED-ORDER-011` is the only new ID, sorted after `SHARED-ORDER-010` in `sdd/specs/requirement-ids.txt`; nothing reused, renumbered, or deleted. (Nit, not a finding: in the spec file the section sits before SHARED-ORDER-007; the ledger order is what the rule requires.)
4. ok — code owners unchanged (`OrderingMoveSafety`, `OrderingDeclarationUtilities`, `AccessorOrderingUtilities`, `TypeMemberOrderingCodeFixProviderBase` exist); `Uses` rows of the six rule specs and `ordering-move-safety.md` name SHARED-ORDER-011; `sdd/specs/README.md` needs no change.
5. ok — `documentation/rules/RH7102/7103/7107/7108/7109/7309.md` unchanged; they hold no directive or pragma text beyond the general-notes footer, so nothing to correct.
6. ok — output questions 970-1..4 answered under `Maintainer answers`; none open; no spec text holds an open question.
7. ok — `scripts/test.sh --project architecture`: "Passed!  - Failed: 0, Passed: 198, Skipped: 0, Total: 198". SHARED-ORDER-011 is covered by 45 `[Requirement("SHARED-ORDER-011")]` tests in the diff.
8. ok — checked every test named in the test implementer's `Tests` tables of 04, 08, 09 (94 rows): each carries the requirement tags its row lists (the one not found by name, `FixAllCorrectsEveryDiagnostic`, is the Fix All scenario method of the batch base, not a separate method).
9. ok — `sdd/work/` holds only `ordering-move-directives`; the lead deletes it before publishing.

### thoenissen/Reihitsu#1668 (`ordering-move-refuses-conditional-groups.md`)
1. ok — change doc required (SHARED-ORDER-008 changed, whole-unit test retired, SHARED-ORDER-007 changed, visible: fixes withheld that `main` offered); Accepted; `Visible output changes` present (items 1 and 2); `Decisions` records 1668-1 B and 1668-2 A, matching `Maintainer answers`. The RH7309 crossed-kept-block and uncrossed-gap reading (decision 9) is stated in the change doc and covered by 1668-1 B.
2. ok — SHARED-ORDER-008 (retitled "A conditional directive in the moved or the crossed span refuses a member move"), SHARED-ORDER-007, SHARED-ORDER-006 (accessor fixes keep the conditional pair), RH7102-006, RH7103-007, RH7309-005 and the `code-fixes.md` SHARED-FIX-009 example agree with each other and with the diff; no leftover "whole units" reading in the RH7102/RH7103/RH7109/RH7309 specs; RH7103's struct example and RH7102's replaced examples match the new rule.
3. ok — no new ID; SHARED-ORDER-008's ID and requirement kept in place (retitled, not retired).
4. ok — owners unchanged; `SpecConsistencyTests` pass (198 of 198); `Uses` of `code-fixes.md` (SHARED-ORDER-007 dropped, SHARED-ORDER-008 kept) and RH7309 match the text.
5. ok — rule pages need no change (corner-case withholdings stay out of them).
6. ok — 1668-1 and 1668-2 answered; the RH2001-010 follow-up is under `Follow-up candidates` in `00-lead.md`.
7. ok — 28 `SHARED-ORDER-008` and 12 `RH7309-005` tags in the diff cover the refusal, the front-of-spans clause, the `main` exception, and the RH7309 crossed/uncrossed examples.
8. ok — see #970 check 8 (04, 08, 09 tables cover both issues).
9. ok — see #970 check 9.

### Whole run
- Both change docs are Accepted and their `Affected requirements` do not overlap contradictorily (SHARED-ORDER-006, RH7102-006, RH7103-007, RH7309-005 are changed by both and the texts compose: SHARED-ORDER-006 names the pairs a member move keeps as region pairs plus SHARED-ORDER-011 pairs).
- `git diff --check origin/main...HEAD`: clean.
- No finding for any role.
