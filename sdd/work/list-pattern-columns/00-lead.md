# Lead — list-pattern-columns

- Branch: `claude/squad-tickets-review-w5g0uk` (session-designated)
- PR: thoenissen/Reihitsu#1933 (draft)
- Base at claim: `75cc39f` (origin/main)
- Lane: **behavior-change**

## Issues

| Issue | agent-filed | Own lane | Subject |
|---|---|---|---|
| thoenissen/Reihitsu#1633 | yes | behavior-change | Base list wrapped behind its colon (column of the base types; comma-first shape in comment) |
| thoenissen/Reihitsu#1634 | yes | behavior-change | Comment-held line inside an angle-bracket list's first element or a qualified name |
| thoenissen/Reihitsu#1647 | yes | behavior-change | Comment in front of a list pattern's closing `]` |
| thoenissen/Reihitsu#1685 | yes | behavior-change | Wrapped `when` condition of a `case` label |
| thoenissen/Reihitsu#1542 | yes | behavior-change | Content of a wrapped parenthesized or positional pattern |
| thoenissen/Reihitsu#1929 | yes | bug-fix (FMT-INDENT-026, possible wording clarification) | Property-pattern brace behind `and`/`not` in a switch arm |

Lane reason: five issues ask for new formatter layout decided by output questions (change doc required). #1929 alone would be a bug fix against FMT-INDENT-026; it was placed in this run by the milestone coordinator because it shares `PatternAnchor` and FMT-INDENT-026 with #1542, and the behavior-change lane admits a bug-fix diff (lanes widen bug fix → behavior change). Recorded as decision 1.

All roles of the lane run: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist.

Issue comments relevant to the run (summarized):
- #1633: the merged change #1920 (FMT-INDENT-034 continuation indent, FMT-BREAK-041 joins) joins a base list's `:` onto the type line; the issue's no-comment "Before" may no longer occur; restate against current behavior. Also a comma-first base-list shape (`class K : IA⏎, IB`) was proposed to be decided together.
- #1634: after #1920 the "Before" changed: unowned continuation lines now sit one level deeper than their construct's line; an alignment this issue chooses takes precedence over FMT-INDENT-034.
- #1542: further members (`is not⏎(...)`, `is C⏎(1)`, `{ X:⏎1 }`, `(1⏎or 2)`, `(int⏎or long)`, case-label form); comma-first positional pattern (`(1⏎, 2)`); after #1920 parenthesized-pattern content sits four columns right of `(` by a maintainer answer there; positional patterns still undecided.
- #1929: coordinator note: FMT-INDENT-026 already requires anchoring past `and`/`or`/`not`, so no output question is expected.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | #1929 is a bug fix while the other issues are behavior changes; split the run? | Keep all six in one behavior-change run | The maintainer named all six in the prompt; the coordinator batched #1929 here for the shared `PatternAnchor`/FMT-INDENT-026 mechanism; the behavior-change lane admits a bug-fix diff | Lane choice is internal | claim |

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: spawn spec author (01).
