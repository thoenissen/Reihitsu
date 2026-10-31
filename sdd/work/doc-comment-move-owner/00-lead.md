# Lead — issue thoenissen/Reihitsu#1491

## Run

- Issue: thoenissen/Reihitsu#1491 — "FMT-DOC: the documentation phase moves every documentation comment written behind code". Label `agent-filed`: its goals, "Done when" items, and target behavior are proposals. Parent: thoenissen/Reihitsu#1482 (package C2). Its blockers thoenissen/Reihitsu#1487 (PR #1510) and thoenissen/Reihitsu#1488 (PR #1513) are closed and merged.
- Slug: `doc-comment-move-owner`
- Branch: `claude/hopeful-dirac-8nqwtv` (the session's designated branch; see Decisions taken #1). Draft PR: thoenissen/Reihitsu#1532.
- Lane: **behavior change**. The issue asks to move the ownership of a formatter decision between phases and requires a change doc that widens FMT-DOC-001 and narrows FMT-BLANK-012.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.
- The issue was written against an older `main` (`735d0dba`); every statement about today's code is checked against current `main` first. The issue states that a statement an earlier package made true or moot is noted and dropped, and that a difference that changes the target goes to the maintainer.
- Issue comments: none.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/hopeful-dirac-8nqwtv` instead of `claude/issue-1491-<slug>` | The session environment forbids pushing to any other branch | Branch names are internal | ebba7a2 |
| 2 | Spec author | Widen FMT-DOC-001 to `/** */` with the reach FMT-BLANK-012 gives them today, including the block-statement case, as the issue proposes? | Widen FMT-DOC-001 only to the delimited comments moved today; the statement / using-directive / expression cases stay open for both comment forms | Deciding the block-statement case for `/** */` alone would split one open question by comment form; the issue's goal is ownership, not a new decision | Every new spec example matches today's output; no output changes | 998810a |
| 3 | Spec author | The issue's using-directive question (C4, #1497 input) | A `/** */` leading a using directive behind another is moved, as today and as `///` | Keeps today's output; the blank-line flip is #1497's case | Unchanged output | 998810a |
| 4 | Spec author | The issue's statements about current code | Recorded in 01-spec-author.md: the blank-line phase does not move every `/** */` today; the `///` paths in the blank-line and brace phases are moot in today's order | The issue's "Before the run" section asks for this check | Facts only | 998810a |

## Follow-up candidates

1. Should a delimited documentation comment behind `{`, behind a switch-label colon, or behind an earlier comment on its line move like `///`? Output question carried by the issue (changes FMT-INDENT-006's example). — 01-spec-author.md
2. A documentation comment before a statement, behind an attribute, or inside an expression is moved with a blank line inserted inside code (`[Obsolete] /** d */ void M()`, `N(1, /** d */ 2)`); proposal: treat as documenting nothing and leave in place. Search duplicates first. — 01-spec-author.md
