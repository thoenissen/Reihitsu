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

Batch 1b answers: 1b A (join every listed row where no comment/directive sits in the gap: return value, `is` pattern, switch arm `=>`, base list `:`, modifier/type, catch `when`, for header, lambda body, `with`, throw/await/new). 1c: neither option — the pattern chain must be rewritten so that `or` stands under `is` (asked back for the exact layout, batch 1c). 1d: declare comment-kept lines in those gaps unsupported (scope asked back, batch 1c). 1e A (parenthesized-pattern content four columns right of `(`). 4b A: declare a comment inside an omitted type-argument list unsupported, in this run. Earlier: 2A, 3A, 6A.

Batch 1c answers (final):
- 1c A: a pattern chain is rewritten so that each combinator starts its line, aligned under `is`: `var k = x is⏎1 or⏎2 or⏎3;` becomes `var k = x is 1` / `or 2` / `or 3;` with each `or` in the column of `is`. Applies to `and` as well, to `case` labels (`case 1 or⏎2:` becomes `case 1` / `or 2:` with `or` under the first pattern), and to switch arms.
- 1d A: comments AND directives in every gap that 1b joins are declared unsupported in this run: between `return`/`throw`/`await`/`new` and what follows; between `is` and its pattern; between a switch arm's pattern and its `=>`; between a type and its base list's `:`; between a modifier and the type; in front of `catch … when`; inside a `for` header behind a `;`; behind a lambda's `=>`; in front of `with`; between an anonymous-object member name and its `=`. The CLI leaves such a file unchanged and reports it; code fixes formatting such code are withheld. The declarator behind `#endif` (6A) is not in this list and keeps the continuation indent.

Consolidated final answers: Q1 -> replaced by 1b A (join), 1c A (pattern chain rewrite), 1d A (unsupported), 1e A (parenthesized-pattern content four columns right of `(`). Q2 A (expression body joins across a blank line). Q3 A (query clauses under `from`). Q4 -> 4b A (comment inside an omitted type-argument list declared unsupported in this run; the FMT-INDENT-015 exception is deleted). Q5 -> covered by 1b A (`return` value joined). Q6 A (declarator behind `#endif` gets the continuation indent). Q7 -> 1b A for the joinable rows; lines that cannot be joined (a `;` behind `#endif` or a stray `///`, the hole of a verbatim interpolated string) keep the continuation indent of FMT-INDENT-034 (stated in batch 1b, not objected to — confirm in a later batch if an output there is in doubt).

Batch 2 answers (final; question numbers as in the lead's batch-2 message, which maps 04-spec-author Q1-Q7 and 05-skeptic's three as follows: lead 1 = 04 Q1 plus 05's extra rows; lead 2 = 05 modifier-without-type; lead 3 = 05 directive confirmation; lead 4 = 05 `is` type test; lead 5 = 04 Q2; lead 6 = 04 Q3; lead 7 = 04 Q4; lead 8 = 04 Q5; lead 9 = 04 Q6; lead 10 = 04 Q7):
- 1 B for every row (the maintainer noted the example was incomplete but chose B): join `private static⏎readonly`, `public⏎event`, `foreach (… in⏎…)`, `await⏎foreach`/`await⏎using`, cast `(int)⏎x`, `new⏎[]`/`new⏎()`, `this⏎int x`, a switch arm's `when`, anonymous member `A =⏎1`, `o is not⏎null`, `let y =⏎v` and `let y⏎= v`, `yield return⏎x`; a comment or directive in each of these gaps is declared unsupported in this run, like 1d A.
- 2 B: a modifier at a line end with no type after it (type declarations, constructors, conversion operators, delegates) joins: `public class N { }`.
- 3 A: directives in every declared gap are refused, including conditional accessibility (`#if … public #else internal #endif class Foo`) and `return⏎#if …`; the CLI leaves such a file unchanged and reports it.
- 4 B: a type test `o is⏎string` joins to `o is string`.
- 5 B: once a pattern chain wraps, every combinator starts its own line under `is` (`x is 1 or 2 or⏎3` gives `x is 1` / `or 2` / `or 3`).
- 6 B: a comment or directive between a pattern combinator and its right pattern is declared unsupported in this run.
- 7 A: a directive inside a list of omitted type arguments is refused like a comment (FMT-UNSUP-018 covers both).
- 8: neither A nor B — the maintainer answered "unsupported": the shape that leaves a continuation line inside a switch arm or an anonymous-object member (example `A = x += // c⏎3`) is declared unsupported in this run instead of choosing a column.
- 9 A: one reason text per refused gap.
- 10 A: `is` followed by a multi-line parenthesized pattern keeps `(` on its own line below `is`; only the content moves (1e).

Standing instruction (maintainer, after batch 2): the maintainer cannot answer further questions for the next hours. Later output questions are decided by the lead following the scheme of the answers so far (join where nothing stands in the gap; declare a comment or directive in a joined gap unsupported; continuation indent only where a line cannot be joined and is not refused), recorded under Decisions taken, and named in the PR's Review notes.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | FC-028: code fixes measure the FMT-INDENT-034 column from a node starting mid-line; how to make fix and CLI agree | Add a document fact (column where the node's first line starts) to FMT-PIPE-024 and use it in FMT-INDENT-034; test RH5302/RH5308 against the CLI | RH5302/RH5308 already require "as the formatter formats it in place"; the fact is part of the issue's own fix, not a rare-class special case | Unchanged requirements already demand CLI-equal output | (spec amendment) |
| 2 | skeptic | SHARED-ATTR-022 example moved to the new column | Revert; drop RH5514 from the change doc | RH5514 does not call the formatter; SHARED-ATTR-017 unchanged | Reverting to today's output | (spec amendment) |
| 4 | answers | The maintainer's answers widen the run to line-break joins, a pattern-chain rewrite and new unsupported shapes; does the skeptic run again? | Fresh spec author amends; then a fresh skeptic checks the amended spec diff before the test implementer | The amendment adds new mechanisms (line-break phase, precheck) the first skeptic never saw | Process only | — |
| 3 | skeptic | Drop #953 goals: FMT-BLANK-013 clarification, FMT-STRUCT-007/023 refusal examples, issue's own example | Drop | Inputs unreachable under FMT-UNSUP-008/012/013 | Files holding these shapes are refused and unchanged | — |

## Follow-up candidates

- Comment on open issues #1633, #1634, #1905, #1542: their "Before" output changes under FMT-INDENT-034; an alignment chosen there later takes precedence (01-spec-author.md).
- ~~Whether a wrapped `for` header should align under the token after `(`~~ — dropped: `for` headers are now joined (04-spec-author.md).
- #1633: its case no longer exists because the base list `:` joins the type's line (04-spec-author.md).
- `case (⏎…⏎):` puts `case` alone on its line on `main` — looks wrong, outside the run (04-spec-author.md).

## Audited SHA

(none yet)

## Open worklist / next step

- Claim done (draft PR #1920, `scripts/prepare.sh` passed).
- Spec author done (fc44967): 2 Draft change docs, FMT-INDENT-034/035; 4 output questions in 01-spec-author.md.
- Skeptic done (5f6af8f): all four reproduce; 5 amendments routed to a fresh spec author.
- Spec-author amendment done (5fe7f28), 03-spec-author.md supersedes 01 for output questions.
- Output-question batch 1 sent to the maintainer (questions 1-7: reference line, blank-line join, query clauses, omitted type args, return value, #endif declarator, other applications). Waiting for the answer.
- Batch 1c answered (1c A, 1d A).
- Batch 1b (clarification of the join principle, comment-kept lines, Q4 declaration, parenthesized pattern) sent. Waiting.
- Spec author amendment done (be4ccf4, 04-spec-author.md): change docs Accepted; 7 new output questions; AGENTS.md phase table needs the new shapes/joins (route to spec author with the next amendment).
- Skeptic 05 done (3b2af6d): spec amendments (findings 2, 7, 8, 9, AGENTS.md table) pending for the spec author; 3 new output questions.
- Batch 2 answered (see Maintainer answers).
- Next: one fresh spec author for the answers AND the 05 amendments (06); then test implementer (note 05's list of existing tests that change).
