# Lead — cli-stack-overflow

| | |
|---|---|
| Issue | thoenissen/Reihitsu#1887 (`agent-filed`: yes — goals are proposals) |
| Slug | cli-stack-overflow |
| Branch | claude/dazzling-clarke-n7m17j |
| Pull request | thoenissen/Reihitsu#1893 (draft claim) |
| Lane | Bug fix |

## Lane and reason

Bug fix: the issue reports an observed difference — a file whose expression nesting overflows the formatter's stack aborts the whole CLI process — against an unchanged requirement, CLI-016 (a failing file does not stop the run), `sdd/specs/cli/reihitsu-format.md`. The maintainer's triage comment on the issue states the expected behavior is the issue's option A, implemented as a bug fix against CLI-016, with no maintainer question open.

Roles run: skeptic → test implementer (reproduction first) → implementer → cleaner → spec checker → auditor → validator. Skipped: spec author (bug-fix lane, only for a clarification; none needed so far).

## Maintainer clarifications

- Triage (issue comment, 2026-11-20): critical bug for 1.5.0; expected behavior is option A — the file is reported as failed (`Error processing <path>: ...`), the other files are processed, the run ends with the error summary and exit code 2. Treated as already answered by CLI-016.

## Maintainer answers

_None yet._

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Fix only `LayoutComputer.ComputeBlockIndentation`, or every failing member of the class the skeptic enumerated (also `ControlFlowBraceTransform.GetTokenBehindIfStatement`, `FluentChain.CollectMemberAccess`)? | All three members; scope growth accepted | Same mechanism and same requirement (CLI-016); fixing one leaves two known violations (FC-016) | Output is the one CLI-016 and the triage answer already decide: the file is reported as failed, the run continues | — |
| 2 | Skeptic | Is declaring deep nesting unsupported an option (escalation 10)? | No — not an option under the contract | The failing depth depends on the runtime stack and build, not on the input, and the declaration would delete no special case (`AGENTS.md`: a shape that deletes nothing is not declared) | Nothing declared; nothing changes | — |
| 3 | Skeptic | Roslyn parser/lexer overflows (16000-deep generics, nested interpolated strings) before Reihitsu code runs | Dropped, not a follow-up | Rare input class that no cheap guard in Reihitsu code reaches; handling it needs CLI-level machinery (`AGENTS.md`, *Special cases do not grow the source*) | Nothing changes | — |
| 4 | Skeptic | Performance of `GetTokenBehindIfStatement` on 6400-branch `else if` ladders and of deeply nested interpolated strings | Dropped, not a follow-up | Only serves a rare input class (thousands of nested levels) | Nothing changes | — |
| 5 | Skeptic | Issue goals: affected surface and fixed depths | Adjusted: CLI error handling needs no change; the fix lies in the formatter's own recursion; tests do not treat 6400/16000 as fixed thresholds | `agent-filed` goals are proposals; the skeptic showed the CLI's catch already meets CLI-016 for catchable exceptions | Visible output is option A, as decided | — |

## Follow-up candidates

- Analyzer path `FluentChainAnalyzerBase` → `FluentChain.Create` could overflow in the compiler/IDE host on extreme chains (01-skeptic.md). Covered by the run if `FluentChain` itself is fixed; otherwise reconsider (likely drop as rare input class).

## Worklist / next step

- Done: claim (0bd9f1b), draft PR #1893, `scripts/prepare.sh`.
- Done: skeptic (01-skeptic.md, verdict proceed, no output questions; handover commit 4c6ad52).
- Next: test implementer (reproduction).
