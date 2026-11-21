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

Batch 1 (questions 1-7), first reply, summarized: the maintainer doubts #953 is still current. Q1: asks whether the anonymous-object case (`A // c` then `= 1`) is not unsupported anyway; a parenthesized pattern's content should be one indent inside `(`; `is` followed by `1` on the next line should be joined; a switch arm's `=> 2` on the next line should be joined. Q2: A. Q3: A. Q4: declare unsupported. Q5: the `return` value should be joined onto the `return` line. Q6: A. Q7: these lines should be joined too. Several points are ambiguous -> asked back (batch 1b).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | FC-028: code fixes measure the FMT-INDENT-034 column from a node starting mid-line; how to make fix and CLI agree | Add a document fact (column where the node's first line starts) to FMT-PIPE-024 and use it in FMT-INDENT-034; test RH5302/RH5308 against the CLI | RH5302/RH5308 already require "as the formatter formats it in place"; the fact is part of the issue's own fix, not a rare-class special case | Unchanged requirements already demand CLI-equal output | (spec amendment) |
| 2 | skeptic | SHARED-ATTR-022 example moved to the new column | Revert; drop RH5514 from the change doc | RH5514 does not call the formatter; SHARED-ATTR-017 unchanged | Reverting to today's output | (spec amendment) |
| 3 | skeptic | Drop #953 goals: FMT-BLANK-013 clarification, FMT-STRUCT-007/023 refusal examples, issue's own example | Drop | Inputs unreachable under FMT-UNSUP-008/012/013 | Files holding these shapes are refused and unchanged | — |

## Follow-up candidates

- Comment on open issues #1633, #1634, #1905, #1542: their "Before" output changes under FMT-INDENT-034; an alignment chosen there later takes precedence (01-spec-author.md).
- Whether a wrapped `for` header should align under the token after `(` instead of FMT-INDENT-034's column (02-skeptic.md).

## Audited SHA

(none yet)

## Open worklist / next step

- Claim done (draft PR #1920, `scripts/prepare.sh` passed).
- Spec author done (fc44967): 2 Draft change docs, FMT-INDENT-034/035; 4 output questions in 01-spec-author.md.
- Skeptic done (5f6af8f): all four reproduce; 5 amendments routed to a fresh spec author.
- Spec-author amendment done (5fe7f28), 03-spec-author.md supersedes 01 for output questions.
- Output-question batch 1 sent to the maintainer (questions 1-7: reference line, blank-line join, query clauses, omitted type args, return value, #endif declarator, other applications). Waiting for the answer.
- Batch 1b (clarification of the join principle, comment-kept lines, Q4 declaration, parenthesized pattern) sent. Waiting.
- Next: route the answers to the spec author (status Accepted), then the test implementer.
