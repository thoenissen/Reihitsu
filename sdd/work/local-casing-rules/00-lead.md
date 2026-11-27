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

- Batch 1 (2026-11-27), maintainer answered "as recommended" = **1B, 2A, 3B**:
  - 1B: RH4117 reports every variable that receives a whole element of a positional pattern (`(var Third, int Fourth)`, `(int Number and > 0, _)`, `(string { Length: > 0 } Text, _)`, `({ } Any, _)`); RH4115 keeps only a variable receiving a property of an element (`Size` in `(string { Length: var Size }, _)`). Fix result unchanged (camelCase rename).
  - 2A: `const int _ = 1;` is not reported (RH4128 exempts `_`).
  - 3B: every casing fix (RH4105, RH4106, RH4115, RH4117, RH4128) keeps an anonymous-object member name explicit when the renamed name is the member's whole expression: `Select(Value => new { Value })` → `Select(value => new { Value = value })`; `from Value in values select new { Value }` → `from value in values select new { Value = value }`.
- Batch 1b Q4 (2026-11-27), answered "as recommended" = **4A**: SHARED-CASING-015 applies to every fix using the shared rename (also RH4101, RH4102, RH4107–RH4114, RH4120): e.g. RH4111 `private int value {get;set;}` + `new { value }` → `private int Value` + `new { value = Value }`.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | spec author | Rule ID for #1088 | RH4128 | RH4119 is retired (shipped in betas); RH4120–RH4127 taken; RH4128 is the next free number, as the maintainer asked; reuse would let old suppressions silence the new rule | The maintainer's answer fixes the ID as the next free RH41xx number | f9c9426f |
| 2 | skeptic | Arch finding 1: RH4115/RH4117 each state half of the deconstruction-variable boundary | One shared check that RH4117 includes and RH4115 excludes, built in this run | The #1087 gap is exactly the two statements drifting apart; small, within the components the run changes | Internal structure; outputs follow the maintainer's answers | 9808734b |
| 3 | skeptic | Arch finding 2: casing fixes rename an anonymous-object member whose whole expression is the renamed name (SHARED-FIX-003, on main for RH4105/4106/4115/4117) | Fix in this run in the shared casing fix base, stated once in shared/casing.md; RH4105-006 refers to it. How (withhold vs explicit member name) is output question 3 of batch 1 | Run edits the shared base anyway and ships three new fixes carrying the defect | Visible part asked (batch 1, Q3) | 9808734b |
| 4 | skeptic | Rule ID RH4128 vs RH4119 | Not asked; decision 1 stands | Maintainer said next free number | — | f9c9426f |
| 5 | skeptic | Spec amendments: RH4115-002 refer to RH4117-001/-003 instead of restating; RH4128-001 'wherever a const local declaration stands' | Route to spec author with the answers | Self-contradiction / incomplete list | Wording only, no output change beyond answered ones | pending |
| 6 | implementer | Three new internal helper types (DeconstructionVariableUtilities, RangeVariableDeclarationUtilities, CasingAnonymousMemberUtilities); RH4124 moved onto the range-variable helper | Keep; not escalation 9 | Each implements an issue's own fix (#1087 boundary, #1978 clauses, answers 3B/4A); RH4124 reuse avoids a second statement of the clause list | RH4124 unchanged in behavior (its tests pass) | 9808734b |
| 7 | implementer | Two disputed tests: multi-line anonymous-object test applies only first action with two diagnostics; RH4105 same-new-name test needs NumberOfFixAllIterations for a no-op Fix All | Route to a fresh test implementer to correct the test mechanics; asserted outputs stay | Test-harness mechanics, implementer shows outputs are as specified | Asserted visible outputs unchanged | pending |

## Follow-up candidates

- Shared casing rename renames an anonymous-object member whose whole expression is the renamed name (`new { Value }` → `new { value }`), on `main` for RH4105/RH4115 fixes; SHARED-FIX-003 — 01-spec-author.md (Whole run). Taken into the run (decision 3).
- RH2001-011 withholds RH2001's fix when a property reference names an anonymous member; could keep the name explicitly like SHARED-CASING-015 — 03-spec-author.md (Whole run).

## State

- Last step: implementer done (9808734b, handover 60cba3bd); 3 disputed test cases. Next: fresh test implementer (06) fixes the 2 disputed tests, then implementer check if needed, then cleaner.
- Earlier: spec author done (f9c9426f, pushed). Open output question: RH4117 positional subpatterns (A recommended); change docs list visible changes (local-casing-rule-split.md 7 cases, range-variable-casing.md 5 cases).
- Next: skeptic, then output-question batch.
