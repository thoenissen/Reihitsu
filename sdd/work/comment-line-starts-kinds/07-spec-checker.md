Role: spec checker
Commit: f8aec36
Verdict: findings

Inputs: `00-lead.md` through `06-cleaner.md`, the decisions entries `squad/decisions/2026-10-21-1139-shared-comment-001-contradicting-tests.md` and `squad/decisions/2026-10-22-1139-p12-scope-rh53xx.md`, `git diff origin/main`, and thoenissen/Reihitsu#1362 with its three comments. My prompt carried no intended fix or verdict.

All three findings are low severity. None blocks the run, and none needs a code change.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 1 | `sdd/specs/shared/comment-placement.md`: SHARED-COMMENT-001 (changed) and SHARED-COMMENT-011 (new) | Both change intended behavior, and `sdd/README.md` requires a change doc for "a requirement added with new behavior, changed". The diff has none. This looks like an accepted exception rather than an error: the lane check confirms `tooling` without one, and `78af33e` (#1099, SHARED-DIAG-002/003) recorded decided principles the same way. The decision is recorded only in the lead's handover and in the issue text of thoenissen/Reihitsu#912 and #1139, not in a decisions entry. | lead: confirm with the maintainer that decided principles P12 and P13 need no change doc, and write it into the PR run report |
| 2 | 2 | SHARED-COMMENT-001, third paragraph ("Wherever a rule distinguishes a line that holds only a comment from a line that holds code, a line holding only a `/* */` comment MUST count like a line holding only a `//` comment") | `sdd/README.md` wants one decision per requirement, each with its own `Why:`. SHARED-COMMENT-001 now carries three decisions: (a) every ordinary comment kind alike, a `///` or `/** */` that documents nothing included; (b) the verdict does not depend on the documentation mode; (c) a `/* */`-only line counts like a `//`-only line. (c) has a different reason, and no spec lists SHARED-COMMENT-001 for it, so it has no consumer in `Uses`. It has no example and no test. The lead capped new IDs at SHARED-COMMENT-011, so splitting is not possible in this run. The clause is correct per P13; this only records the gap. | spec author, if the maintainer wants (c) split into its own requirement in a later run (RH5104, thoenissen/Reihitsu#1146) |
| 3 | 6 | thoenissen/Reihitsu#1362, Q2 | The issue text says "SHARED-COMMENT-011 currently leaves this shape out ('does not cover')". That note was removed from the spec in `03-spec-author.md` (F2/F3), so the sentence describes text that no longer exists. Q2 itself still stands: the spec says nothing about a leading comment that is out of place. | lead: edit the sentence in the issue or add a comment |

## Checked
1. Change doc: finding 1. Everything else about the run fits no-change-doc: the diff touches no production code, and the maintainer decided P12/P13 in thoenissen/Reihitsu#912.
2. Specs match the diff: finding 2 only.
   - SHARED-COMMENT-001 states P13, with the CS1587 definition of "documents nothing".
   - SHARED-COMMENT-011 states P12. The `Why:` argues from P12, and the sentence "does not make the line start with a comment" leaves RH5203/RH5112/RH5201 to their own requirements.
   - The examples carry only shapes that conform today, as the 2026-10-21 decision requires. They use `{|RH####:…|}` markup, and the mixed RH5201/RH5204 block is a negative case with no fix, so no result block is due.
   - The reference sentence is added to all 19 requirements named in handover 03: RH5110-001, RH5112-001, RH5201-001/-002, RH5203-001, RH5204-001, RH5205-001, RH5206-001, RH5301-001/-002, RH5302-002, RH5303-001/-002, RH5305-001, RH5306-001, RH5308-002. `Uses` matches them, and `SpecStructureTests` agrees.
   - The RH5033-RH5049 specs, RH5034-002 included, are consistent with the P13 wording.
3. IDs: ok. `SHARED-COMMENT-011` is the only new ID, inserted after `SHARED-COMMENT-010` and before `SHARED-DEBUG-001` in `requirement-ids.txt`. Nothing is removed, moved, or renumbered. `git diff origin/main -- sdd/specs/requirement-ids.txt` adds one line.
4. Code owners: ok. `comment-placement.md` keeps its four owners, `Uses` gains `SHARED-BLANK-002`, and `sdd/specs/README.md` needs no row change. The architecture project passed (see 7).
5. User documentation: ok. The rule pages RH5033, RH5034, RH5038, RH5201, RH5204 mention neither the documentation-comment exemption nor comment-led lines, and the diff changes no code-fix output. No page needs a change.
6. Open questions: finding 3. The spec author found no further question (`03-spec-author.md`). All questions of `01-spec-author.md` are filed in thoenissen/Reihitsu#1362: Q1 (P13), Q2, Q3, then Q4 and Q5 (RH5302/RH5308, P12), Q6 (RH5305, P12) and Q7 (RH5301/RH5303, P12), plus the precedents for Q2/Q4. The further-instance comments on thoenissen/Reihitsu#1147 and #1077 are recorded in `00-lead.md`; I did not re-read them. The RH5203 own-line-comment fix finding is reused from thoenissen/Reihitsu#1144, as `00-lead.md` states. The two decisions entries follow the `squad/decisions/README.md` format.
7. Coverage and consistency: `scripts/test.sh --project architecture` gives `Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72`.
   - SHARED-COMMENT-011 has 22 tagged tests across RH5201, RH5203, RH5204, RH5301 and RH5303. Their names and shapes match the spec examples. None tags a shape that `03-spec-author.md` says to leave out (RH5112, RH5110, RH5205, RH5206, RH5302, RH5308, RH5305, multi-line tails).
   - SHARED-COMMENT-001, the changed requirement, keeps its covering tests. The P13 clauses ("documents nothing" and the `/* */`-only line) have no conforming shape and no test. The 2026-10-21 decision accepts this, and the shapes are tracked by #1362 Q1 (see finding 2).
   - Lane check: `scripts/lane-check.sh tooling --head worktree` gives `confirmed — claimed tooling, diff needs tooling`.
8. Nothing left behind: ok. `sdd/work/` holds only `comment-line-starts-kinds/`, which the lead deletes before publishing. The decisions entries stay.

Follow-up candidates (none filed by me): none beyond finding 2.
