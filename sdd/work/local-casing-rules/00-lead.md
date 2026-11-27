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

## Follow-up candidates

- Shared casing rename renames an anonymous-object member whose whole expression is the renamed name (`new { Value }` → `new { value }`), on `main` for RH4105/RH4115 fixes; SHARED-FIX-003 — 01-spec-author.md (Whole run). Undecided: adjacent fix vs follow-up; decide after skeptic.

## State

- Last step: spec author done (f9c9426f, pushed). Open output question: RH4117 positional subpatterns (A recommended); change docs list visible changes (local-casing-rule-split.md 7 cases, range-variable-casing.md 5 cases).
- Next: skeptic, then output-question batch.
