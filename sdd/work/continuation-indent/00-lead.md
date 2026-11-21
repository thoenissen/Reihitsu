# Lead — continuation-indent

- Branch: `claude/squad-issues-953-1852-439khw` (designated by the session)
- PR: thoenissen/Reihitsu#1920 (draft)
- Slug: `continuation-indent`
- Lane: `behavior-change`

## Issues

| Issue | agent-filed | Lane | Reason |
|---|---|---|---|
| thoenissen/Reihitsu#953 | yes | behavior-change | Requests a new general indentation rule for continuation lines no alignment contributor owns (new FMT-INDENT requirement replacing a non-goal) |
| thoenissen/Reihitsu#1852 | yes | behavior-change | Requests a continuation indent for a `return` value on the next line, and a decision for the expression-body conversion across a blank line — a visible output change |
| thoenissen/Reihitsu#1896 | yes | behavior-change | Requests query clauses outside a list element to align under `from` — a visible output change |
| thoenissen/Reihitsu#1818 | yes | behavior-change | Requests a continuation indent for a declarator behind `#endif` in event fields and locals — a visible output change |

All four share one mechanism: the column the indentation phase gives a continuation line that no alignment contributor owns. All are `agent-filed`: goals are proposals.

## Maintainer clarifications (summarized from issue comments)

- #953: further instances were added as comments (compound assignment `x +=`/value; interpolated-string holes; query clauses; `o is (1,⏎2)`; switch-expression arm `when`/`=>` wrap; anonymous-object member `A⏎= 1`; `x ??=⏎2`; modifier `public⏎int B`; primary-constructor base argument list; attribute on lambda; `new[] {1,⏎2}` in a split argument list; `return⏎(1)`; lambda body after `=>`; parenthesized expression after `(`; `with` expression; base list whose `:` starts its own line (also touched by #1633); declarator lists of fields/locals kept whole; collection-expression element where CLI and code fix disagree). Comment-carried instances already tracked by #1383, #895, #1397 stay there.
- #1852, #1896, #1818: triage placed them in 1.6.0 with #953; their output questions are asked in this run.

## Roles

All roles of the behavior-change lane run: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing when there are follow-up candidates.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## Audited SHA

(none yet)

## Open worklist / next step

- Claim done (draft PR #1920, `scripts/prepare.sh` passed).
- Next: spawn the spec author.
