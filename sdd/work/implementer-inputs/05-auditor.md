Role: auditor
Commit: 906098b
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing to ignore.

## Scope
Full audit. Head `906098b55765e61b1540eeb37ede92e177de55a0` (checked out, working tree clean); base `origin/main` `f21c69c2719144828edd92d5b43385508608563e`, an ancestor of the head (merged). The diff is what the handovers describe: one sentence added to `squad/roles/implementer.md`, *Inputs*, plus the run's handovers under `sdd/work/implementer-inputs/`. `scripts/lane-check.sh tooling`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.

Against issue #1981 ("Done when"): *Inputs* now names every issue of the run by number as an input in the tooling lane, read through `issue_read`, which agrees with `tools:` in `.claude/agents/implementer.md` (`mcp__github__issue_read`) and with `.claude/agents/README.md` ("Every role that reads the run's issues by number gets `issue_read`"). The lane clause matches the lane table in `squad/README.md` (Tooling: spec author –, test implementer "when the tool has tests"). The issue's own premise said the tooling lane has "no test implementer"; the table says it has one when the tool has tests, and the diff follows the table, not the issue. The lead's prompt carries every issue number (`squad/lead.md`, section 4, and the implementer agent wrapper), so the implementer has the numbers the clause relies on. No spec covers role files, so no change doc is required (`sdd/README.md`, *When a change doc is required*).

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope
_N/A — the diff adds or changes no predicate, guard, or exemption; it changes role-file text only._

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which inputs the implementer reads | `squad/roles/implementer.md` *Inputs* is the only statement; `rg -n -i implementer squad/lead.md squad/README.md .claude/agents/README.md` finds the lane table, the role table, and the tool table, none of which restates the implementer's inputs | one owner |
| Which roles read the issues by number and get `issue_read` | `.claude/agents/README.md` (tool rule and table), `tools:` of `spec-author`, `skeptic`, `test-implementer`, `implementer`, `auditor`, `issue-manager` under `.claude/agents/`; the role files of those roles each name the issues as input | consistent: the implementer's role file now names the issues, matching its tool list |

## Assertion adequacy
_N/A — the diff adds or changes no test; no test covers role-file text (`SquadNoteReferenceTests` only bans references to `squad/notes/`, and the diff adds none)._

## Failure classes dismissed
- FC-001, FC-002, FC-003, FC-005, FC-016, FC-044 — no guard or predicate changed.
- FC-004, FC-019, FC-020, FC-021 — no edit of token gaps, trivia, or line joins.
- FC-006, FC-022–FC-030, FC-036 — no rewrite, code fix, or formatter change.
- FC-007–FC-009, FC-018 — no code fix.
- FC-010–FC-014 — no phase or analyzer change.
- FC-038–FC-043 — no precheck or unsupported-shape change.
- FC-015 — checked under *Policy ownership*: one owner.
- FC-017, FC-033–FC-035 — no test added or changed.
- FC-031 — no method changed; the edited text is the role file itself, and the rest of *Inputs* stays accurate.
- FC-032 — no compiled change; no earlier result is reused for a compiled tree.
- FC-037 — the text-only claim rests on `scripts/lane-check.sh` (its text-only proof), not on `grep`; no `.cs` file changed.

## Architecture findings
_None._

## Hints
- Pre-existing, outside the issue (the spec author recorded it as a non-goal): in a tooling run of a tool without tests, the first sentence of *Inputs* still lists "the test implementer's handover" and "the spec diff and change doc" unconditionally, and *Job* step 1 says "Run the handed-over tests and see them fail". An implementer in such a run has neither. Not a defect of this diff; a candidate only if the lead wants the role file to read correctly for that case too.
- `00-lead.md`, *Open worklist / next step*, still says "Spawn the implementer"; the lead keeps it current.
- The PR may state that the shipped clause follows the lane table ("a test implementer only when the tool has tests") rather than the issue's "no test implementer".

## Follow-up candidates
_None._
