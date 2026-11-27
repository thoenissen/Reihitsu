# Lead — interp-hole-csharp10

- Issue: thoenissen/Reihitsu#1948 — `agent-filed` (goals are proposals). Labels: bug, analyzer. Milestone 1.6.0.
- Slug: `interp-hole-csharp10`
- Branch: `claude/squad-1948-336ja3` (session-designated)
- Draft PR: #1976
- Lane: **bug-fix** — the issue reports observed diagnostics (RH5301, RH5303, RH5206, RH5103, RH5008, RH5513, RH5104 inside a hole of a non-verbatim interpolated string under C# 10) against an existing shared requirement (SHARED-DIAG-011). No new behavior beyond the spec's existing principle.
- Roles: skeptic (premise + reachability) → spec author (clarification only: the rules' specs state the position, if the skeptic confirms) → test implementer (reproduction) → implementer → cleaner → spec checker → auditor → validator. Issue manager before publish if follow-up candidates exist.
- Maintainer clarifications: none.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Lane | Order of spec author and skeptic | Skeptic first, then spec author for the clarification | In the bug-fix lane the spec author runs only for a clarification; whether one is needed (and for which rules) follows from the premise check | Ordering only | — |
| 2 | Skeptic | Scope: issue lists 7 rules; skeptic found ~25 with one mechanism | Take all members the skeptic names (RH5001–RH5019, RH5021, RH5029, RH5103, RH5110, RH5206, RH5301, RH5303, RH5403, RH5513, RH5527) into this run | Same mechanism and shared predicate; SHARED-DIAG-011 is the unchanged requirement for all; agent-filed goals are proposals | SHARED-DIAG-011 demands the outputs unchanged | — |
| 3 | Skeptic | RH5104 in the issue | Dropped: its change can be written (comment moves above the line, compiles under C# 10); RH5104-001 decides it | Premise wrong for RH5104 | Output stays as on `main` | — |
| 4 | Skeptic | RH3004 / RH3208 fixes write CS8967 code when the fixed node lies inside a C# 10 `$"` hole (node-level entry point detaches the target; FC-028 architecture finding) | Refuse the class: withhold the RH3004 and RH3208 fixes when the fixed node lies inside such a hole, a cheap check in each fix via the existing Core predicate; no structural change to the node-level entry points | Rare input class (code in an interpolation hole, C# < 11); refuse-first per README *Architecture findings*; meets the issue's "no fix formats a target inside such a hole" | Withholding a fix for a rare class puts nothing in question (README *What reaches the maintainer*) | — |
| 5 | Skeptic | Escalation 10 (declare unsupported) | Not an option: the failing input (a detached node-level target) does not hold the shape, so the precheck could not refuse it; whole-document formatting already handles it | AGENTS.md *Special cases*: precheck checks exactly what the phases receive | — | — |
| 6 | Skeptic | Analyzer check placement | Shared base where a rule family has one, otherwise per rule; reuse `IsInsideSingleLineInterpolation` from Core | Avoid a second statement of the rule | Internal | — |
| 7 | Skeptic | Spec wording | Spec author (clarification): state the position once per shared owner (statement blank lines, attribute placement, initializers) and per rule only where no shared owner covers it (RH5103, RH5110, RH5206, RH5403) | Clarifies SHARED-DIAG-011 per rule, as the issue's Done-when asks | Spec text is internal | — |
| 8 | Skeptic | Output questions | None: SHARED-DIAG-011 and RH5104-001 decide every visible output | — | — | — |
| 9 | Spec author | RH3004 withholding scope | Withhold on the block path only (RH3004-010); the expression-body result compiles in the hole and stays offered | Narrows decision 4 to the unsafe output; keeps the fix `main` offers where it is safe | The expression-path fix stays as on `main`; the block path is a rare class | cf073d8 |
| 10 | Spec author | Position statement, placement, SHARED-ATTR-024 users | Accept: restated per requirement, appended at spec end, listed by RH5513/RH5527 only | Follows decision 7 and existing precedent | Spec text is internal | cf073d8 |
| 11 | Before tests | Output-question batch | No stop: no visible output change listed, no question raised | — | — | — |

## Follow-up candidates

- `scripts/apply-fix.sh` does not flag fix results that fail to compile under the chosen `--lang-version` (e.g. CS8967); tooling — from 01-skeptic.md

## State

- Last step: spec author (02-spec-author.md, spec written, cf073d8).
- Next: test implementer, then implementer.
