# Lead — line-break-gap-helpers

- Issues: thoenissen/Reihitsu#1640 (`agent-filed`), thoenissen/Reihitsu#1651 (`agent-filed`). Goals of both are proposals.
- Slug: `line-break-gap-helpers`
- Branch: `claude/loving-mayer-uxnwuu` (session-designated). Draft PR: #1858.
- Lane: **bug fix**. Both issues ask to move private copies of a line-break-phase decision onto the shared owner in `LineBreakTriviaUtilities` (#1640: batch gap joins onto `JoinGaps`; #1651: "line break in front of a moved token, keeping an existing one" onto `EndLineInFrontOf`), with no change of formatter output. Production paths only, no change doc expected. Shared mechanism: `LineBreakTriviaUtilities` and `ChainLineBreakRewriter`, which both issues touch.
- Roles run: skeptic (premise), test implementer (characterization/coverage of the shared helpers and the rewriters' outputs; no reproduction exists for a pure refactor), implementer, cleaner, spec checker, auditor, validator. Spec author skipped unless a clarification is needed.
- Maintainer clarifications: none.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## State

- Last step: claim, prepare.sh green.
- Next: skeptic.
