# Lead — rh4001-rh4009-names

- Issue: thoenissen/Reihitsu#1085 — `agent-filed` (labels: enhancement, analyzer, agent-filed). Its body records two decisions the maintainer took in question round 3 (2026-10-18); those decisions are maintainer answers and binding, the rest of the issue text is a proposal.
- Slug: `rh4001-rh4009-names`
- Branch: `claude/awesome-clarke-r1xscu` (session-designated). PR: #1986 (draft).
- Lane: **behavior-change**. Reason: neither case is decided by a spec today — RH4001-005 covers only file names without the 14-digit prefix, and RH4009-002 covers namespaces "declared at file level" — so both specs gain requirements with new behavior, which needs a change doc (`sdd/README.md`, *When a change doc is required*). The issue calls them bug fixes; the rule page of RH4009 already states the full-name comparison.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.

## Maintainer answers

1. (Question round 3, 2026-10-18, recorded in the issue body.) RH4001: for a file `20210501083748_InitialCreate.cs` that declares `class SomeMigration`, the code fix keeps the timestamp prefix and renames the file to `20210501083748_SomeMigration.cs`.
2. (Same round.) RH4009: a nested namespace `namespace A { namespace B {} }` is compared by its full name `A.B`; with the list `["A", "A.B"]` nothing is reported, and `["A", "B"]` no longer allows it.
3. The maintainer allowed splitting the two defects into separate PRs; the run keeps them in one PR (one issue, one run).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Start | Lane | behavior-change | Specs gain requirements with new behavior | Lane is internal | claim |
| 2 | Start | Split into two PRs? | One PR | One issue, small, shared area (naming rules) | Internal | claim |
| 3 | Spec author | New requirements RH4001-008 / RH4009-003 instead of widening 005/002 | Accept | Keeps unchanged requirements stable | Spec structure is internal | 9de335e |
| 4 | Spec author | "Nested" = block-scoped inside block-scoped only | Accept | Mixed file-scoped/block-scoped does not compile; no caller reaches it | Not reachable | 9de335e |
| 5 | Spec author | Rule-page sentences added on both pages | Accept | One sentence each, restates the maintainer's decisions | Covered by maintainer answers 1–2 | 9de335e |
| 6 | Spec author | RH4001-006 collision check follows the new name (prefixed target) | Accept | Direct consequence of answer 1: the collision is checked on the name the fix writes | Covered by answer 1 | 9de335e |
| 7 | Skeptic | EF Core rationale in RH4001-008 `Why`, change doc `Why`, and rule page is false (EF orders by `[Migration]` ID, not file name) | Spec author rewords spec and change doc to the true reason; the rule-page sentence states only the behavior (the fix keeps the prefix), with no rationale about migration order | Fix a false statement; behavior unchanged | Behavior is answer 1; rule page then only restates answer 1 | (pending) |
| 8 | Skeptic | Prefix digits in the fix | Fix keeps exactly what the analyzer strips (same digit test) | Analyzer and fix must agree; non-ASCII digits are rare | No spec change; converges | (implementer) |

## Follow-up candidates

(none yet)

## Worklist / next step

- Done: claim, draft PR #1986, prepare.sh.
- Done: spec author (01), change doc `sdd/changes/2026-11/rh4001-rh4009-names.md` Draft; its visible output changes are covered by maintainer answers 1–2 → no output-question stop.
- Done: skeptic (02, b884bf7) — proceed; no output questions, no follow-ups.
- Next: spec author amendment (decision 7), then test implementer.
