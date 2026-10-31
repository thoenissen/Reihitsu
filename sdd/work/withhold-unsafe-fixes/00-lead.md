Role: Lead
Commit: 735d0db
Verdict: lane claimed — bug fix

# Run

- Issues, in prompt order: thoenissen/Reihitsu#1136, #1134, #1135, #1262, #1332, #1336, #1340.
- Slug: `withhold-unsafe-fixes`.
- Branch: `claude/sweet-johnson-msb5hu`. The session environment fixes this branch name and forbids pushing to any other branch, so the run does not use the `claude/batch-1136-<slug>` name of `squad/lead.md`, section 2.
- Draft PR: thoenissen/Reihitsu#1481.
- Blockers named by the issues are closed: #1138 (PR #1357), #1260 (PR #1359), #1415 (PR #1418).

# Lane: bug fix

- #1136 — spec clarification of SHARED-FIX-003 (relocating or reordering comments and directives; dropping, editing, or relocating disabled text) with covering tests. On its own it would be the tests lane; the maintainer's triage places it as a clarification without change doc in the same run as #1332 and #1336, and the tests lane widens to bug fix.
- #1134 — RH7109 fix reorders fields of sequential-layout types: observed versus expected, bug fix.
- #1135 — RH7309 fix changes static initializer execution order: bug fix.
- #1262 — region fixes (RH7303, RH7304, RH7306, RH7309) that break code or separate a declaration from its documentation: bug fix; triage states no change doc, RH7309-004 aligned as a clarification.
- #1332 — RH7501 fix withheld when it breaks code, moves directives, or rearranges shared lines: bug fix; RH7501-004 gains the "withheld otherwise" sentence as a clarification.
- #1336 — RH7004, RH7101, RH7105, RH7106 fixes: bug fix; RH7101-004 changes as a clarification.
- #1340 — RH7003 reports a namespace containing a namespace: triage states the behavior is already required by RH7003-001 on main, so bug fix without change doc; the rule's own spec text and page are aligned as a clarification.

Roles run: spec author (clarifications of SHARED-FIX-003, RH7309-004, RH7501-004, RH7101-004, RH7003-001/-002, and the rule page of RH7003), skeptic, test implementer, implementer, cleaner, spec checker, auditor, validator. Roles skipped: none.

# Maintainer clarifications (summarized)

- #1136: runs as a clarification without change doc; fixes that relocate comments or directives are bugs, fixed in this run for the cases #1332 and #1336 name. A directive pair split across a brace (one directive moved, its partner left behind) counts as reordering a directive.
- #1262: bug-fix lane, no change doc; RH7309-004 is aligned to SHARED-FIX-003 as a clarification. Additional instances from later comments belong to the issue: RH7304 analyzer pairing like the compiler for a pair with a directive in executable code; RH7306 fix withheld when nothing remains of the name; RH7309 fix withheld when a comment stands directly above a region block; RH7309 fix withheld in documentation mode `Parse`/`Diagnose` when it would separate a declaration from its `///` (three shapes listed on the issue), unchanged in mode `None`.
- #1332: the blank-line part (former Q10) moved to #1437 and is not part of this run. The empty block that remains after the fix is kept (Q9).
- #1336: an additional instance — RH7101 in mode `Parse` on a multi-declarator field with a `///` or `/** */` comment: fix withheld, diagnostic stays; RH7101-004 changes with this issue.
- #1340: bug-fix lane, no change doc; where the rule's spec text disagrees, the same PR aligns it.

# Lane widened: behavior change

After the spec author's repair round, `scripts/lane-check.sh bug-fix` failed because the run's decisions entry lies under `squad/` (tooling). The maintainer reclassified the run as a behavior change and allowed change docs (`squad/decisions/2026-10-27-withhold-unsafe-fixes-lane.md`), before the implementer started. The run now claims the behavior-change lane; the spec author writes the change doc, and the maintainer approves its What and Why before the test implementer starts (escalation 2). Roles run: all eight, as before. The maintainer asked the lead to pause after every role because of the run's budget.
