Role: Lead
Commit: 8e0f35dac358ca06f977629db45a2be1fd28c830
Verdict: Lane "tests" claimed

# Run: thoenissen/Reihitsu#1139 — line-leading comments and comment kinds (P12, P13)

- Issue: thoenissen/Reihitsu#1139. Parent thoenissen/Reihitsu#899; question tracker thoenissen/Reihitsu#912 (at its sub-issue limit).
- Slug: `comment-line-starts-kinds`.
- Branch: `claude/beautiful-thompson-j29fus` — the session pins this branch name, so the `claude/issue-<N>-<slug>` naming of `squad/lead.md` is not used. Claim: draft PR thoenissen/Reihitsu#1361, based on `main`.
- Lane: **Specs and tests for existing behavior** (`tests`). Reason: the run records decided principles as shared requirements and covers them with tests of behavior that already conforms; no production code changes, no change doc.
- Roles: spec author → skeptic (spec against the code) → test implementer → cleaner → spec checker → validator. Skipped: implementer (no production change in this lane), auditor (not part of the tests lane).

## Maintainer's clarifications for this run (summarized)

1. Record P12 and P13 as the issue body states them. The principle texts are decided (thoenissen/Reihitsu#912); write them, do not reopen them.
   - P12: a rule that checks where a line starts judges the line's first element, a comment included. A line whose first element is a comment is not reported for the position of the token behind it; other rules forbid the comment itself.
   - P13: every ordinary comment kind is treated alike, in every documentation mode. A `///` or a `/** */` that documents nothing is an ordinary comment. A line holding only a `/* */` comment counts like a line holding only a `//` comment wherever a rule distinguishes comment lines from code lines.
2. Home of P12: `sdd/specs/shared/comment-placement.md` as **SHARED-COMMENT-011** — not `shared/diagnostics.md` (other runs mint SHARED-DIAG IDs there). SHARED-COMMENT-011 is the only ID this run may mint.
3. P13 amends SHARED-COMMENT-001 and the existing P6 wording (a `/** */` that documents nothing is a block comment).
4. Add SHARED-COMMENT-011 to `Uses` of the rule specs that judge where a line starts: RH5110, RH5203, RH5204, RH5206, and the other RH51xx/RH52xx layout specs the issue's scope (line breaks, alignment, indentation) names.
5. Where a principle states behavior the code does not yet meet, the requirement still states the principle; its covering tests pin only shapes that already conform. Non-conforming shapes are tracked by the follow-up bugs — no open question for them, and no characterization test that contradicts the requirement. If a requirement has no conforming shape to test at all, the run stops and the lead escalates before it is written.
6. Known follow-ups that depend on this run (do not implement them): thoenissen/Reihitsu#1147 (RH5110/RH5203/RH5206 P12 instances), thoenissen/Reihitsu#1146 (RH5104 incl. comment kinds), thoenissen/Reihitsu#1341 (documentation layout rules treat a `///` that documents nothing as an ordinary comment), thoenissen/Reihitsu#1346 (P30). P29/P30 are not written here (thoenissen/Reihitsu#1331).
7. A further instance of an existing follow-up gets a comment by the lead on that issue, not a new question. A new instance of a decided principle is filed in the spec's open-questions issue, marked "Decided by principle Pn (#912)". Other analyzer/formatter disagreements (P4) are open questions per `sdd/README.md`.
8. Must not: change production code or behavior; fix any bug; write an open question into a spec; retire, move, or renumber an existing requirement ID; mint IDs of other runs; touch files owned by concurrent runs: `shared/code-fixes.md` (#1138), `sdd/README.md` (#1161), `shared/diagnostics.md`, `type-regions.md`, `using-directive-ordering.md`, `element-documentation.md` (#1260), Formatter.Test/Playground.Test tags (#939). Text conflicts in `sdd/specs/requirement-ids.txt` and `sdd/specs/README.md` are resolved by keeping both sides.
9. Issue comment on thoenissen/Reihitsu#1139 lists round-7a P13 instances already implemented elsewhere (RH5306 in #1272, RH3201 in #1280) and one decided (RH8204, `/** */` documenting nothing like other block comments).

## Lane widening (before the implementer)

`scripts/lane-check.sh tests --head worktree` → `LANE CHECK: mismatch — claimed tests, diff needs tooling`, because the decisions entry `squad/decisions/2026-10-21-1139-shared-comment-001-contradicting-tests.md` (escalation 4) is a tooling path. Widened tests → tooling, as in earlier runs (known tool gap thoenissen/Reihitsu#1054). The tooling lane adds the auditor; the implementer is still skipped because the run changes no production or tooling code. Order now: skeptic → test implementer → cleaner → spec checker → auditor → validator.

## Escalations

1. Escalation 4 (existing SHARED-COMMENT-001 tests contradict P13): the maintainer chose to remove the contradicting tests and the contradicting half of the two mixed tests, keeping conforming parts, and to have spec examples show only conforming shapes. Recorded in the decisions entry above. The lead applied the same example rule to SHARED-COMMENT-011 (its non-conforming RH5110/RH5203/RH5206 shapes stay tracked by thoenissen/Reihitsu#1147).

## Lead actions so far

- Open questions filed: thoenissen/Reihitsu#1362 (`SDD open questions: sdd/specs/shared/comment-placement.md`, Q1 marked "Decided by principle P13 (#912)", Q2, Q3).
- Further-instance comments: thoenissen/Reihitsu#1147 (RH5205 comment-led brace, P12), thoenissen/Reihitsu#1077 (RH5033 `/** */` behind an attribute list, P6).
