# Lead — statement-blank-line-fixes

- Branch: `claude/nice-thompson-cq0rqf` (the session's required development branch, used instead of `claude/batch-946-...`; see Decisions taken #1)
- Draft PR: thoenissen/Reihitsu#1683
- Slug: `statement-blank-line-fixes`
- Lane: **bug fix**

## Issues (in prompt order)

| Issue | agent-filed | Lane reason |
|---|---|---|
| thoenissen/Reihitsu#946 | yes | Observed vs. expected: RH5005/RH5006 fix does not converge on `await using` / `await foreach` (SHARED-STMT-004, SHARED-FIX-001) |
| thoenissen/Reihitsu#972 | yes | Observed vs. expected: statement blank-line fixes insert two blank lines when a block comment shares the statement's line (SHARED-BLANK-001). A maintainer comment asks to delete the matching non-goal in `sdd/specs/analyzer/RH5002.md` once fixed |
| thoenissen/Reihitsu#1096 | yes | Two decided bugs: (1) indentation of the moved statement after a `case … when` guard spanning lines (SHARED-STMT-004; remove any rule-page sentence documenting it as a limitation); (2) a whitespace-only line after `///` counts as blank, nothing reported (SHARED-BLANK-001; RH5011 and SHARED-STMT rules) |
| thoenissen/Reihitsu#1076 | yes | Decided bugs against SHARED-FIX-001/004/005: RH5022–RH5027 runs of blank lines removed in one application; RH5023 not reported at end of file; RH5011 not reported before `else`/`while` that continue the enclosing statement; RH5024/RH5025 must not remove the blank line below `#region` that SHARED-BLANK-006 requires (cycle with RH5032). The RH5025/RH5030 instance is already resolved on main. Blocker #1068 is closed |
| thoenissen/Reihitsu#1626 | yes | Refactor with lane stated as bug fix: one owner for inserting the blank line in front of a statement (base class vs. `BlankLineCodeFixUtilities.InsertBlankLineBefore`); narrowing RH5030-003 would be visible and is the maintainer's |
| thoenissen/Reihitsu#1625 | yes | Observed vs. expected: RH3208 fix leaves trailing whitespace and column-0 statement behind a multi-line comment; fix correctly or withhold (spec says so). Maintainer triage: #1626 is the planned fix for #1625; they run together |

All issues are `agent-filed`: their goals are proposals.

## Roles

Bug-fix lane: spec author (clarifications only: RH5002 non-goal deletion per #972's comment, any rule-page limitation sentence per #1096, any spec text a decided instance needs), skeptic, test implementer, implementer, cleaner, spec checker, auditor, validator. Issue manager once before publishing if follow-up candidates exist.

## Maintainer clarifications (summarized)

- #972 comment: when fixed, delete the RH5002 non-goal about a block comment in front of the `if`; SHARED-STMT-004 then covers it, no replacement requirement.
- #1625/#1626 triage: the #1626 refactor is the planned fix for #1625; both run together.
- #1076: decided readings — each blank-line fix removes the whole run it reports; end-of-file position is not a line (not reported); RH5011 not reported when the next token continues the enclosing statement; RH5024/RH5025 bug for the `#region` cycle (the blank line stays).
- #1096: the moved statement is indented like the statement it follows; the line after `///` holding only whitespace counts as blank, nothing reported.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's required branch `claude/nice-thompson-cq0rqf` instead of `claude/batch-946-<slug>` | The execution environment allows pushing only to that branch | Branch names are internal | claim commit |

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: spawn spec author (clarifications), then skeptic.
