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

Output questions of 01-skeptic, asked before the test implementer:

1. Gap in front of a trailing comment in a braced switch section (label, last wrapped statement, terminal `break`) — **A**: the gap stays as written, as in a plain block and a braced `if`/`else` body (FMT-SPACE-001). The maintainer added that no extra logic is wanted: the wrap does not normalize this gap at all; it carries it.
2. Comments around the terminal `break` of a braced switch section — **A**: their layout is kept as written (`/* b1 */ /* b2 */ break; /* x */ // y` stays on one line with its spaces).
3. (asked at the audit, 09-auditor finding 1) Blank line between the last leading comment and its statement — first wrapped statement and terminal `break` of a braced switch section — awaiting answer.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name: `squad/lead.md` asks for `claude/issue-1490-<slug>`, the session is bound to `claude/dazzling-planck-2e0bj3` | Use the session-designated branch | The session may push only to its designated branch | Branch name is internal | e796687 |
| 2 | Claim | `sdd/work/node-root-detached/` is left on `main` from merged PR #1520 | Delete it in this run's publish commit together with this run's `sdd/work/` | A published PR must not leave `sdd/work/`; the stale directory belongs to a merged run, and deleting it is a commit that only deletes `sdd/work/`, which the lead writes | Working files only | (publish commit) |
| 3 | Skeptic | Goal "directive guards shrink" | Drop | FMT-STRUCT-003 and FMT-SWITCH-010 are unchanged MUST NOT requirements; shrinking the guards is a behavior change outside FMT-PIPE-013 | Dropping keeps the output the unchanged specs demand | — |
| 4 | Skeptic | `if`/`else` half of the issue | Already met on `main` since #1513; no production change, `else` gets characterization tests tagged FMT-PIPE-013 | Skeptic reproduced the issue's required `if` output and all three layouts in `else` on the current head | Output unchanged | — |
| 5 | Skeptic | Architecture finding: `ControlFlowBraceTransform` carries comment trivia as written, `SwitchCaseBraceRewriter` rebuilds it; one rule, two owners | Change the structure in this run: one shared carrier of a wrap's comment trivia, taking the separators each caller needs as input, used by both components; `ControlFlowBraceTransform`'s output stays unchanged | A switch-local fix would itself add a second implementation of the same decision (`squad/README.md`, *Architecture findings*); both components are the run's component and its counterpart | The carrier choice is internal; visible output is fixed by FMT-PIPE-013, FMT-SWITCH-007, and the maintainer's answers | — |
| 6 | Skeptic | Idempotency defect: `///` leading the first statement of a braced section gets a blank line in pass 1, removed in pass 2 | In scope: same cause, same component; gets a test | FMT-PIPE-004 demands idempotency | An unchanged requirement decides the output | — |
| 7 | Spec author | Where the two maintainer answers live | FMT-SWITCH-007, not FMT-PIPE-013 | The maintainer answered for the braced switch section only; FMT-PIPE-013 would also bind the expression-body conversion, which was not traced | The answers are recorded where they take effect; no other output changes | a245f0c |
| 8 | Spec author | Lane after the answers | Keep bug fix, no change doc | FMT-SWITCH-007 already applied FMT-PIPE-013 to these comments and FMT-PIPE-013's `Why:` already rules out a wrap that loses the layout; the answers settle the reading | Both outputs are maintainer answers | a245f0c |
| 9 | Repair 1 | Spec checker finding 1 (code owner of `BlockWrapTriviaUtilities`); spec author lists it under FMT-PIPE rather than the two caller specs | Accept; skip the cleaner for this repair round | One type owns at most one spec (`sdd/README.md`); the type implements FMT-PIPE-013. The repair changes only Markdown, so the cleaner has nothing to format or build | Spec headers only | 50bf596 |

## Follow-up candidates

- Narrow the directive guards of `ControlFlowBraceTransform` and `SwitchCaseBraceRewriter` to the cases where a directive cannot move into a block, as a behavior change with its own change doc (01-skeptic).
- FMT-STRUCT-003's `Why:` is stale since #1513 (text-only) (01-skeptic).
- A comment directly at an expression body's arrow lands on the new brace's line as `{/* one */`; FMT-STRUCT-006 leaves it unspecified — file only if not already tracked (01-skeptic).
- `ExpressionBodyToBlockConverter` is a third carrier of comment trivia across inserted braces, not routed through `BlockWrapTriviaUtilities` because its trivia shape differs (04-implementer; also 01-skeptic, architecture finding).
- A section that gets braces with `case 1: /* c */ A();` leaves `/* c */` on the label line and moves `A();` into the block (same on `main`); FMT-SWITCH-007's first sentence demands it, FMT-PIPE-013 could be read otherwise — follow-up carrying an output question (09-auditor, hint).
