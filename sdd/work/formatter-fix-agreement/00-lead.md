# Lead — formatter-fix-agreement

- Issues: thoenissen/Reihitsu#1529 (`agent-filed`), thoenissen/Reihitsu#1543 (`agent-filed`). Both goals are proposals.
- Slug: `formatter-fix-agreement`
- Branch: `claude/squad-1529-1543-wokrft` (the session's designated branch; see Decisions #1)
- PR: thoenissen/Reihitsu#1697 (draft)
- Lane: behavior change
  - #1529: an open output question on which layout the RH5111/RH6023 fixes and whole-document formatting share for a wrapped member of an assigned conditional access; whichever reading is chosen changes the formatter's or the fixes' visible output, so a spec change and likely a change doc.
  - #1543: an open question whether the blank-line phase removes blank lines between an own-line comment and further braces (and chained keywords, per the issue's comment), or whether RH5024/RH5025/RH5026 exempt them; either reading changes formatter output or diagnostics and amends FMT-BLANK-020.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if there are follow-up candidates. None skipped.
- Maintainer clarifications: none beyond the prompt.

## Maintainer answers

- Q1 (#1529, RH5111 wrapped member behind a conditional access): **B** — the RH5111 fix adopts whole-document formatting's layout (`.C` under the `?` column as the formatter places it; element binding joined `a?[0].C = 5;`; nested case column 9). Formatter, RH5201, RH5112 unchanged. Options D (withhold) and E (declare unsupported — escalation 10) not chosen: the shape is **not** declared unsupported. This supersedes the earlier answer recorded in `sdd/changes/2026-10/node-root-detached.md`.
- Q2 (#1543, blank line between own-line comment and brace/chained keyword): **A** — the formatter removes the blank line for every brace and for `else`/`catch`/`finally`/do-`while`; RH5024–RH5027 unchanged.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 2 | Spec | #1529 issue goals: RH6023 test and "record C in the formatter spec" | Drop both | RH6023 fix only touches operator gaps (SHARED-OPSPACE-004), already pinned; C not chosen | No output changes from dropping them | — |
| 3 | Spec | #1543 issue goals: issue's own input (now refused by FMT-UNSUP-004) and complex element initializer | Replace input with assignment/initializer-interior forms; keep existing element-initializer coverage; extend round trips to RH5026/RH5027 | Skeptic findings | Test selection is internal | — |
| 1 | Claim | Branch name | Use the session's designated branch `claude/squad-1529-1543-wokrft` instead of `claude/issues-1529-1543-<slug>` | The session environment permits pushes only to the designated branch | Branch names are internal | claim commit |

## Follow-up candidates

- Comment in front of the closing `}` of a multi-line interpolation hole stays in column 12 while the hole's other lines move to column 8 (outside #1543; carries an open question, readings A move with the hole's other lines / B keep; refined by `02-skeptic.md`: in a regular interpolated string the formatter moves the comment to column 12 even from column 8) — `01-spec-author.md`, `02-skeptic.md`.

## Worklist / next step

- Done: claim, draft PR #1697, `scripts/prepare.sh` (SDK 10.0.404 installed).
- Done: spec author (`01-spec-author.md`, a49e965): two draft change docs, two output questions (#1529 rec. B, #1543 rec. A).
- Done: skeptic (`02-skeptic.md`, 7cfa03f): #1529 rec. B, adds the column-9 input and option D (withhold fix); names escalation 10 (declare the null-conditional assignment with a wrapped target unsupported), not recommended. #1543 rec. A; change-doc corrections (oscillation claim, field array initializer `}`, nested array `{`, RH5027 for do-while).
- Answers received (1B, 2A).
- Next after answers: spec author amends change docs + specs per answers and skeptic's corrections; then test implementer.
