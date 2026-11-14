# Lead — spacing-and-list-wraps

- Issues (prompt order): thoenissen/Reihitsu#1642 (`agent-filed`), thoenissen/Reihitsu#1635 (`agent-filed`). Goals of both are proposals.
- Slug: `spacing-and-list-wraps`
- Branch: `claude/keen-goldberg-e84ipm` (session-mandated branch, see decision 1)
- Draft PR: thoenissen/Reihitsu#1733
- Lane: **behavior change** (widened from bug fix after the maintainer's answers, decision 6)
  - #1642: reported observed-versus-expected difference in the formatter's horizontal spacing (space behind a multi-line `*/` removed before some tokens).
  - #1635: reported difference against FMT-BREAK-012 in the line-break phase (argument list wrapped in front of a comma is not laid out one element per line).
- Roles run: skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Spec author: only for a clarification (skipped unless a role reports a spec gap).

## Maintainer clarifications

- #1635 triage comment (maintainer): stays a bug against FMT-BREAK-012 for 1.2.2; related to the vNext issue about a leading comma in an argument list (thoenissen/Reihitsu#956) but not folded into it.
- #1635 second comment (agent-written): two related observations — a list wrapped only after `(` is joined to one line (`M(⏎a, b)` → `M(a, b)`), and a trailing line comment that pins a leading comma keeps two elements on one line. Both proposed to be settled together with the issue's input; proposals, not binding.

## Maintainer answers

- Batch 1 (before test implementer), #1635:
  - Q1 `Foo(1⏎, 2, 3)` → **C**: one argument per line with trailing commas: `Foo(1,⏎    2,⏎    3)`. Applies to attribute argument lists too.
  - Q2 `Foo(1⏎, 2,⏎3)` → **like 1C**: `Foo(1,⏎    2,⏎    3)`.
  - Clarification (summarized): a list is multi-line when its elements are on different lines and single-line when all elements are on the same line. Consequence: a list wrapped only behind `(` with all elements on one line (`M(⏎a, b)`) is single-line, so today's join to `M(a, b)` is the intended output — this changes FMT-BREAK-012's "wrapped after its opening parenthesis" clause.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's mandated branch `claude/keen-goldberg-e84ipm` instead of `claude/issues-1642-1635-<slug>` | The environment allows pushing only to that branch | Branch name is internal | claim commit |
| 2 | Skeptic | #1642: make the change? | Do not change behavior. Add only a characterization test for FMT-SPACE-001's sentence on multi-line comments; drop the issue's expected output; link `Refs #1642` | FMT-SPACE-001 and FMT-SPACE-013 require today's output, and the issue's premise that a single-line comment keeps the space is false (skeptic 01). The issue reproduces as reported, so it is not escalation 4 | The output stays as on `main`, which unchanged requirements demand | – |
| 3 | Skeptic | #1635: fix the wrap-only-behind-`(` case (W1) and the comment-pinned leading comma (W2c) in this run? | W1: yes — unchanged FMT-BREAK-012 names it, same mechanism. W2c: only if it falls out of the same mechanism with no extra guard; otherwise left as written (rare class, RH5035/RH5038 report it) | FC-016; *Special cases do not grow the source* | W1 is required by unchanged FMT-BREAK-012; W2c is a rare class either way | – |
| 4 | Skeptic | Architecture finding: the "wrapped behind the opening delimiter" signal exists only in the indexer visitor | Change the structure in this run: one shared signal for all four list visitors in `LineBreakListRewriter`, within the component the run changes | The class includes common code (W1), and per-visitor copies are the defect | Internal structure | – |
| 6 | Answers | Lane after the answers | Widen to behavior change: change doc + FMT-BREAK-012 diff (multi-line decided by elements on different lines; leading commas become trailing in multi-line argument/attribute argument lists) by the spec author, then a fresh skeptic on the spec diff | Maintainer's answers change visible output beyond the unchanged spec | Decided by the maintainer | – |
| 7 | Answers | Decision 3's W1 | Superseded: W1 (`M(⏎a, b)`) stays joined to one line per the maintainer's clarification; W2c (comment-pinned leading comma) is a rare class: rewrite only when the comma move is safe with a cheap check, otherwise leave as written | Maintainer answer; *Special cases do not grow the source* | Maintainer answer / rare class | – |
| 5 | Skeptic | Refuse leading-comma argument lists instead? | No | Common code; maintainer triage keeps it a bug for 1.2.2; declaring unsupported deletes nothing | – | – |

## Follow-up candidates

- `Foo(1, 2, 3⏎)`: RH5102 reports it, its fix makes no progress, the formatter keeps it; #956 decides the target — extend thoenissen/Reihitsu#956 with this note (01-skeptic).
- thoenissen/Reihitsu#956: this run moves leading commas behind the previous element in multi-line argument and attribute argument lists (maintainer answer 1C); extend #956 so it reflects what remains (lead).

## Worklist / next step

- Done: skeptic (01).
- Answers received (see above).
- Next: spec author (02) → skeptic on spec diff (03) → test implementer.
