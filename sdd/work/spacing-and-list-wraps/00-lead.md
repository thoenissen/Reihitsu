# Lead — spacing-and-list-wraps

- Issues (prompt order): thoenissen/Reihitsu#1642 (`agent-filed`), thoenissen/Reihitsu#1635 (`agent-filed`). Goals of both are proposals.
- Slug: `spacing-and-list-wraps`
- Branch: `claude/keen-goldberg-e84ipm` (session-mandated branch, see decision 1)
- Draft PR: thoenissen/Reihitsu#1733
- Lane: **bug fix**
  - #1642: reported observed-versus-expected difference in the formatter's horizontal spacing (space behind a multi-line `*/` removed before some tokens).
  - #1635: reported difference against FMT-BREAK-012 in the line-break phase (argument list wrapped in front of a comma is not laid out one element per line).
- Roles run: skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Spec author: only for a clarification (skipped unless a role reports a spec gap).

## Maintainer clarifications

- #1635 triage comment (maintainer): stays a bug against FMT-BREAK-012 for 1.2.2; related to the vNext issue about a leading comma in an argument list (thoenissen/Reihitsu#956) but not folded into it.
- #1635 second comment (agent-written): two related observations — a list wrapped only after `(` is joined to one line (`M(⏎a, b)` → `M(a, b)`), and a trailing line comment that pins a leading comma keeps two elements on one line. Both proposed to be settled together with the issue's input; proposals, not binding.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's mandated branch `claude/keen-goldberg-e84ipm` instead of `claude/issues-1642-1635-<slug>` | The environment allows pushing only to that branch | Branch name is internal | claim commit |
| 2 | Skeptic | #1642: make the change? | Do not change behavior. Add only a characterization test for FMT-SPACE-001's sentence on multi-line comments; drop the issue's expected output; link `Refs #1642` | FMT-SPACE-001 and FMT-SPACE-013 require today's output, and the issue's premise that a single-line comment keeps the space is false (skeptic 01). The issue reproduces as reported, so it is not escalation 4 | The output stays as on `main`, which unchanged requirements demand | – |
| 3 | Skeptic | #1635: fix the wrap-only-behind-`(` case (W1) and the comment-pinned leading comma (W2c) in this run? | W1: yes — unchanged FMT-BREAK-012 names it, same mechanism. W2c: only if it falls out of the same mechanism with no extra guard; otherwise left as written (rare class, RH5035/RH5038 report it) | FC-016; *Special cases do not grow the source* | W1 is required by unchanged FMT-BREAK-012; W2c is a rare class either way | – |
| 4 | Skeptic | Architecture finding: the "wrapped behind the opening delimiter" signal exists only in the indexer visitor | Change the structure in this run: one shared signal for all four list visitors in `LineBreakListRewriter`, within the component the run changes | The class includes common code (W1), and per-visitor copies are the defect | Internal structure | – |
| 5 | Skeptic | Refuse leading-comma argument lists instead? | No | Common code; maintainer triage keeps it a bug for 1.2.2; declaring unsupported deletes nothing | – | – |

## Follow-up candidates

- `Foo(1, 2, 3⏎)`: RH5102 reports it, its fix makes no progress, the formatter keeps it; #956 decides the target — extend thoenissen/Reihitsu#956 with this note (01-skeptic).

## Worklist / next step

- Done: skeptic (01).
- Open: output questions 1 and 2 for #1635 (01-skeptic, *Output questions*) asked to the maintainer; waiting.
- Next after answers: test implementer (02).
- Review notes to carry: W1 changes formatter output and RH5101/RH5102 fix results for common code, required by unchanged FMT-BREAK-012.
