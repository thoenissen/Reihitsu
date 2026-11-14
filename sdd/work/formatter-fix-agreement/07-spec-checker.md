Role: spec checker
Commit: f38baf4
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1529

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

### Checked
1. ok: `conditional-access-assignment-fix-layout.md` is Accepted, lists visible output changes, has the maintainer's answer (1B) under Decisions; the supersession of the merged `node-root-detached.md` is recorded in the new doc only; no task list.
2. ok: RH5111-006 states the fix's new layout, matches the one-file production diff (outermost chain node as context); format (heading, normative sentence, Why, example with `{|RH5111:...|}` markup) follows.
3. ok: RH5111-006 added in sorted position in `requirement-ids.txt`; no other ledger line touched.
4. ok: `Uses` extended; code owners unchanged and exist.
5. ok: no rule page affected (normal case unchanged).
6. ok: no open output question; answers in `00-lead.md`.
7. ok: `scripts/test.sh --project architecture`: Passed 117, Failed 0 of 117. RH5111-006 is covered by tagged tests.
8. ok: all added RH5111, RH5201 and RH5112 tests carry `[Requirement("RH5111-006")]` or the RH5111-005 tag; both spec examples are tagged.
9. ok: `sdd/work/` holds only this run's directory (the lead deletes it before publishing).

## thoenissen/Reihitsu#1543

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

### Checked
1. ok: `own-line-comment-before-every-brace.md` is Accepted, has Visible output changes and the maintainer's answer (2A) under Decisions; FMT-BLANK-020 is changed, not added, so no ID change.
2. ok: FMT-BLANK-020 text, Why, and the added example match the diff (`BraceBlankLinePolicy` exclusion list deleted, one token set); the other requirements that reference FMT-BLANK-020 (017, 019 text, the directive gap requirement) stay true; no remaining spec mentions the removed exclusions.
3. ok: no ID added, renumbered or removed.
4. ok: no owner named in a touched spec header was renamed or removed (architecture run above).
5. ok: RH5024-RH5027 and their pages are unchanged by decision; no page changed.
6. ok: no open output question; the one follow-up candidate (interpolation hole `}` comment column) is under Follow-up candidates in `00-lead.md`.
7. ok: covered by 34 `[Requirement("FMT-BLANK-020")]` tests in `CommentBeforeBraceBlankLineTests` plus the RH5024-RH5027 round trips.
8. ok: every added test method in the formatter regression file and the RH5024-RH5027 round trips carries its tag; the new spec example is tagged on the verbatim-example test.
9. ok: as above.
