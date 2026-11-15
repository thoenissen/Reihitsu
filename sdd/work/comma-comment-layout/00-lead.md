# Lead — comma-comment-layout

- Issues: thoenissen/Reihitsu#1737 (`agent-filed`), thoenissen/Reihitsu#1738 (`agent-filed`). Goals of both are proposals.
- Slug: `comma-comment-layout`
- Branch: `claude/adoring-shannon-yomd27` (the session's designated branch; the harness allows pushes only there, so the `claude/issues-1737-1738-...` name of `lead.md` is not used)
- PR: thoenissen/Reihitsu#1749 (draft claim)
- Lane: **behavior change**
  - #1737: the fix and the formatter disagree on where a comment behind a leading comma goes; one of them changes visible output and the spec has to state the placement → behavior change (change doc).
  - #1738: no spec requirement names a comma starting a line in an object initializer; deciding and specifying its layout is new specified behavior → behavior change.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.

## Maintainer answers

- Batch 1, Q1 (#1738, comma that starts a line inside initializer / collection expression / pattern): **A**, with one change — in an array initializer (case `e`, `new int[] { 1⏎, 2 }`) the comma also moves behind the previous element (`1,` ⏎ `2`), as in the other constructs. A comma held off the previous line by a `//` comment (case `c`) stays alone on its line at the elements' column, element on the next line, as in A.
- Batch 1, Q2 (escalation 10, declare "comma separated from the previous token by a line break" unsupported): **not declared (B)**; the comma keeps moving up behind the previous element as today.

- Batch 1b (asked at once, blocks tests): array initializer with a comma held off the previous line by a comment (`1 // k⏎, 2`) — answered **A** (keep `, 2`, column only). The maintainer is considering declaring unsupported the shape "a comment between a list element and its following comma" ("no comment belongs there") and asked which special cases it would delete → escalation 10, investigated (04-skeptic, 485162a): only a shape covering comments AND directives/disabled text deletes code. Asked as escalation 10: Q4 declare (A rec.) / not (B); Q5 in this run (A rec.) / own change (B). Awaiting answer.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch | Harness restricts pushes to it | Internal | claim commit |
| 2 | Spec author | One change doc per issue or one shared | One per issue | Causes differ (fix vs. formatter layout) | Doc structure is internal | 81bfa87 |
| 3 | Spec author | #1738 scope: object initializers only or the whole FMT-BREAK-021/022 family | Whole family, pending skeptic | Same mechanism; one class, not instances | The family's outputs are asked as output questions | 81bfa87 |
| 5 | Skeptic | #1737: fix follows formatter (A), formatter follows fix (B), or withhold | Withhold RH5107's fix when the reported comma's trailing trivia holds a comment; T (comment only on the next line) keeps its fix. Spec RH5107 records the withheld class; change doc reworded accordingly. Issue goal reworded to "no fix result disagrees with the formatter"; still `Closes #1737` | Class is rare by definition (RH5035 reports every comment in a parameter list); B would ship a SHARED-FIX-003 violation for Q/R; A copies FMT-BREAK-012 into a hand-written text edit (architecture finding → refuse first) | Withholding a fix for a rare class is not in question (README, What reaches the maintainer) | pending |
| 6 | Skeptic | #1737 architecture finding (fix and line-break phase own comma placement) | Refuse the class (decision 5); structural change (fix delegates to formatter) as follow-up | README, Architecture findings: refuse first | Internal | — |
| 7 | Skeptic | #1738 question shape | Use the skeptic's single reworded question (spec author's Q2 option A conflicted with RH5301/5303/5304; Q3 folded in) | Avoids a permanent diagnostic (FC-013) | Asked as output question | — |
| 4 | Spec author | New requirement IDs vs. extending existing ones | New requirements (FMT-BREAK-038, FMT-INDENT-033) | Keeps existing requirements stable | Wording/IDs internal | pending |

## Follow-up candidates

- RH5107 fix delegates its layout to the formatter instead of a hand-written text edit (02-skeptic, #1737 Follow-up candidates).
- Comma that starts a line between switch-expression arms falls to the statement column — carries an output question (02-skeptic, #1738).
- Comma that starts a line in a positional pattern — same (02-skeptic, #1738).
- Comma that starts a line in a base list — same (02-skeptic, #1738).

## Audited SHA

(none yet)

## Open worklist / next step

- Waiting for escalation-10 answer (Q4/Q5). On declare-in-run: fresh spec author amends change doc (Special cases section), unsupported-syntax spec (new FMT-UNSUP), deletes held-comma spec text per 04-skeptic §2, drops FMT-INDENT-033 + held half of FMT-BREAK-038. On not-declare: proceed with 19e8ee2 specs to test implementer.
