# Lead — local-casing-rules

- Issues (prompt order): thoenissen/Reihitsu#1087, thoenissen/Reihitsu#1088, thoenissen/Reihitsu#1978 — all three carry `agent-filed` (goals are proposals; maintainer decisions recorded in the issues and their comments are answers and bind).
- Branch: `claude/quirky-tesla-apcack` (session-designated). Draft PR: #1987.
- Slug: `local-casing-rules`
- Lane: **behavior-change** (all three)
  - #1087: changes what RH4115/RH4117 report (exempt `_`, drop local constants, all deconstruction variables to RH4117) — requested changed behavior.
  - #1088: a new rule (casing of local constants) — new rule; the rule itself is the issue, decided by the maintainer, so not escalation 5.
  - #1978: RH4105 widened to query range variables, title/message widened — requested changed behavior.
  - Shared mechanism: the casing rules for locals/parameters (RH41xx naming analyzers, shared casing behavior).
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager if follow-up candidates exist. None skipped.

## Maintainer answers

Recorded before this run, in the issues (summarized):

- #1087 (round 3, 2026-10-18): `_` is exempt from RH4115; RH4115 does not check local constants; RH4117 owns every variable a deconstruction declares, in assignments and patterns alike (`(var A, var B) = p`, `var (A, B) = p`, `p is var (A, B)`, `case var (L, R)`); a simple pattern variable (`is int x`) stays with RH4115.
- #1088 (2026-11-21 comment): local constants follow **camelCase** (replaces the PascalCase starting point). `const int maxValue = 3;` not reported; `const int MaxCount = 5;` reported. Rule ID: the next free number in RH41xx. Release coordinated with #1087 so local constants are checked by exactly one rule. Fix only if comprehensive.
- #1978 (2026-11-27 comment): option A — RH4105 reports query range variables (`from`, `let`, `into`, `join` identifiers) that are not camelCase; its title and message widen to cover them (e.g. "Parameter and query range variable names should be in camelCase"). Rename fix offered only if shown comprehensive and safe for range variables; otherwise no fix. `_` stays unreported.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | spec author | Rule ID for #1088 | RH4128 | RH4119 is retired (shipped in betas); RH4120–RH4127 taken; RH4128 is the next free number, as the maintainer asked; reuse would let old suppressions silence the new rule | The maintainer's answer fixes the ID as the next free RH41xx number | f9c9426f |
| 2 | skeptic | Arch finding 1: RH4115/RH4117 each state half of the deconstruction-variable boundary | One shared check that RH4117 includes and RH4115 excludes, built in this run | The #1087 gap is exactly the two statements drifting apart; small, within the components the run changes | Internal structure; outputs follow the maintainer's answers | pending |
| 3 | skeptic | Arch finding 2: casing fixes rename an anonymous-object member whose whole expression is the renamed name (SHARED-FIX-003, on main for RH4105/4106/4115/4117) | Fix in this run in the shared casing fix base, stated once in shared/casing.md; RH4105-006 refers to it. How (withhold vs explicit member name) is output question 3 of batch 1 | Run edits the shared base anyway and ships three new fixes carrying the defect | Visible part asked (batch 1, Q3) | pending |
| 4 | skeptic | Rule ID RH4128 vs RH4119 | Not asked; decision 1 stands | Maintainer said next free number | — | f9c9426f |
| 5 | skeptic | Spec amendments: RH4115-002 refer to RH4117-001/-003 instead of restating; RH4128-001 'wherever a const local declaration stands' | Route to spec author with the answers | Self-contradiction / incomplete list | Wording only, no output change beyond answered ones | pending |

## Follow-up candidates

- Shared casing rename renames an anonymous-object member whose whole expression is the renamed name (`new { Value }` → `new { value }`), on `main` for RH4105/RH4115 fixes; SHARED-FIX-003 — 01-spec-author.md (Whole run). Taken into the run (decision 3).

## State

- Last step: skeptic done (aa6f3222). Batch 1 output questions posted: Q1 positional/element subpatterns to RH4117, Q2 RH4128 `_`, Q3 anonymous-member rename. Waiting for answers.
- Earlier: spec author done (f9c9426f, pushed). Open output question: RH4117 positional subpatterns (A recommended); change docs list visible changes (local-casing-rule-split.md 7 cases, range-variable-casing.md 5 cases).
- Next: skeptic, then output-question batch.
