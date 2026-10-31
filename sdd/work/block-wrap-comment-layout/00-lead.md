Role: lead
Commit: e796687
Verdict: lane claimed — bug fix

# Run: block-wrap-comment-layout

## Issues

- thoenissen/Reihitsu#1490 — "FMT-PIPE-013: wrapping statements in a block keeps their comments' layout". Labels: `bug`, `formatter`, `agent-filed`. Its premise, goals, "Done when" items, target behavior, and prescribed solution are proposals (`squad/README.md`, *Issues are hypotheses*). Its blocker, thoenissen/Reihitsu#1488 (B3), is closed (merged as PR #1513). The issue names code as of `735d0dba`; every statement about today's code is to be checked against current `main` (`fbf5b6f`).
- Draft PR: thoenissen/Reihitsu#1535. Branch: `claude/dazzling-planck-2e0bj3` (session-designated branch).

## Lane

**Bug fix.** The issue reports an observed-versus-expected difference against an existing requirement, FMT-PIPE-013 (`sdd/specs/formatter/`), in shipped formatter source (`ControlFlowBraceTransform`, `SwitchCaseBraceRewriter`).

Roles run: skeptic (premise check) → test implementer (reproduction, then tests) → implementer → cleaner → spec checker → auditor → validator.
Roles skipped: spec author — runs in the bug-fix lane only for a clarification; spawned only if the skeptic or a later role shows FMT-PIPE-013 or a neighbouring requirement needs one.

## Maintainer clarifications

None.

## Maintainer answers

Output questions 1 and 2 of 01-skeptic asked before the test implementer; awaiting answers.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name: `squad/lead.md` asks for `claude/issue-1490-<slug>`, the session is bound to `claude/dazzling-planck-2e0bj3` | Use the session-designated branch | The session may push only to its designated branch | Branch name is internal | e796687 |
| 2 | Claim | `sdd/work/node-root-detached/` is left on `main` from merged PR #1520 | Delete it in this run's publish commit together with this run's `sdd/work/` | A published PR must not leave `sdd/work/`; the stale directory belongs to a merged run, and deleting it is a commit that only deletes `sdd/work/`, which the lead writes | Working files only | (publish commit) |
| 3 | Skeptic | Goal "directive guards shrink" | Drop | FMT-STRUCT-003 and FMT-SWITCH-010 are unchanged MUST NOT requirements; shrinking the guards is a behavior change outside FMT-PIPE-013 | Dropping keeps the output the unchanged specs demand | — |
| 4 | Skeptic | `if`/`else` half of the issue | Already met on `main` since #1513; no production change, `else` gets characterization tests tagged FMT-PIPE-013 | Skeptic reproduced the issue's required `if` output and all three layouts in `else` on the current head | Output unchanged | — |
| 5 | Skeptic | Architecture finding: `ControlFlowBraceTransform` carries comment trivia as written, `SwitchCaseBraceRewriter` rebuilds it; one rule, two owners | Change the structure in this run: one shared carrier of a wrap's comment trivia, taking the separators each caller needs as input, used by both components; `ControlFlowBraceTransform`'s output stays unchanged | A switch-local fix would itself add a second implementation of the same decision (`squad/README.md`, *Architecture findings*); both components are the run's component and its counterpart | The carrier choice is internal; visible output is fixed by FMT-PIPE-013, FMT-SWITCH-007, and the maintainer's answers | — |
| 6 | Skeptic | Idempotency defect: `///` leading the first statement of a braced section gets a blank line in pass 1, removed in pass 2 | In scope: same cause, same component; gets a test | FMT-PIPE-004 demands idempotency | An unchanged requirement decides the output | — |

## Follow-up candidates

- Narrow the directive guards of `ControlFlowBraceTransform` and `SwitchCaseBraceRewriter` to the cases where a directive cannot move into a block, as a behavior change with its own change doc (01-skeptic).
- FMT-STRUCT-003's `Why:` is stale since #1513 (text-only) (01-skeptic).
- A comment directly at an expression body's arrow lands on the new brace's line as `{/* one */`; FMT-STRUCT-006 leaves it unspecified — file only if not already tracked (01-skeptic).
