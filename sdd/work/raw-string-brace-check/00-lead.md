# Lead — raw-string-brace-check

- Issue: thoenissen/Reihitsu#1722 (`agent-filed`: goals are proposals) — RawStringLineShifts uses the shared interpolation-brace check from Reihitsu.Core.
- Slug: raw-string-brace-check
- Branch: claude/adoring-rubin-zjg10x (session-designated)
- PR: thoenissen/Reihitsu#1952 (draft claim)
- Lane: bug-fix — a production-only consolidation of a duplicated policy statement (FC-015) with no intended change of formatter output; no change doc expected. If a test is the only outcome, the lane still admits it.
- Roles run: skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Spec author: only for a clarification (skipped unless needed).
- Maintainer clarifications: none.

## Maintainer answers

(none)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Switch `RawStringLineShifts.IsHoleOpeningLine` to `IsInterpolationBrace`? | No: keep the open-brace-only test and state in its doc comment why it is not the Core check (the issue's own fallback); `Closes #1722` | The skeptic's swap broke `RawStringMovedLineStartTests.HoleLineStartingWithTheHolesClosingBraceAtColumnZeroMovesRight`; the checks differ by design | A comment only; output unchanged, proven by verify-text-only for that file | 8ef5087 |
| 2 | Skeptic | Take in the adjacent fix: `SingleSpaceSpacingRule.RequiresSpaceBeforeBrace` restates the Core check | Yes, adjacent fix under Scope | One line, same component, existing tests pin it (test implementer confirms or adds one), meets the issue's "one statement per policy" goal | Same predicate after the brace-kind guard; no output change | 8ef5087 |

## Follow-up candidates

- `IsOwningConstruct` duplicated in `src/Reihitsu.Analyzer/Core/CommentPositionUtilities.cs` and `src/Reihitsu.Formatter/Pipeline/Core/Utilities/ListElementInteriorUtilities.cs`; shared owner would be `Reihitsu.Core` (01-skeptic.md)

## State

- Last step: cleaner (04-cleaner.md, clean).
- Next: spec checker, then auditor.
