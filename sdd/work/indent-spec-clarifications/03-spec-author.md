Role: spec author
Commit: 0272bbb
Verdict: needs a test

This is the amendment for decisions 5, 6, and 7 in `00-lead.md`. It changes spec text only, in `sdd/specs/formatter/indentation.md` and `sdd/specs/formatter/region-formatting.md`, and no intended behavior. So no change doc is needed. I ran every example, new or changed, through `scripts/format.sh` (whole document, LF) on `0272bbb`. Each output shown is today's output, and the only other change the run made was removing the final newline. `scripts/test.sh --project architecture`: 247 passed, 0 failed. `scripts/lane-check.sh tests --head worktree`: `LANE CHECK: confirmed — claimed tests, diff needs only docs`.

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## #1400 — formatter-spec clarifications from round 9

### Changed requirements

- FMT-INDENT-011 — retitled **Disabled text, inactive directives, and literal content are never re-indented** — clarified (decision 5, choice 2 option (a)).
  - New paragraph: a directive in a branch the build does not compile, region or not (`#pragma`, a nested `#if`/`#endif`, …), MUST NOT be re-indented and keeps its column. It is not disabled text, so FMT-TAB-001/-002 replace a tab in front of it or between its tokens, and it then keeps the column the replacement gives it. FMT-INDENT-009 and FMT-INDENT-010 give a column only to an active directive.
  - "Only the other lines that start with … a directive are re-indented" now says "an active directive".
  - The `Why:` gained the reason.
  - Two examples were added: fixture d1 as code blocks ("is left unchanged"), and fixture d2 (tabs) in prose.
- FMT-INDENT-010 — Region directives are indented like the members around them — clarified (decision 5, choices 1, 2, and 3).
  - First paragraph: for the inactive case it now refers to FMT-INDENT-011, between members or inside a member, and keeps only what is particular to regions. That is the text under FMT-REGION-006/-007, and the fact that no blank line is padded around it (SHARED-BLANK-007). The overstated phrase "the whitespace and text rules for directives apply to it as to an active one" is gone.
  - Second paragraph: "active" now qualifies both sentences, and the paragraph adds that an inactive region inside a member is not refused and keeps its column.
  - `Why:`: the tab reason moved to FMT-INDENT-011.
  - New example: fixture r3, an inactive region inside a method body, with columns 2 and 10 kept and `#region Inner` / `#endregion // Inner`.
  - The tab example now cites FMT-INDENT-011 for the kept column.
- FMT-INDENT-028 — Lambda block bodies align to the lambda — clarified (decision 5, choice 4). The anchor sentence now says "the opening parenthesis of its parameter list or its single parameter". It also names `static () => …` under `(`. The second example gained `d = static async x` (block under `async`) and `e = static () =>` (block under `(`), and `using System.Threading.Tasks;`.
- `indentation.md` `Uses` row: SHARED-BLANK-007 added.
- `region-formatting.md`, Non-goals and FMT-REGION-005 — clarified (decision 7). "A region directive inside a member is refused / reaches no phase" now reads "an active region directive". An inactive one is not refused: it is written under FMT-REGION-006/-007 (Non-goals), and it stays at its position (FMT-REGION-005).

### Statements that need a characterization test and are not pinned

Tag each with the requirement named, and apply FC-017 (every line ending, second pass) and FC-033 (write tabs as `\t`).

1. FMT-INDENT-010, the code-block example with inactive regions between members at columns 2 and 10 (fixture r1).
2. FMT-INDENT-010, the tab example (fixture r2): tabs in front of the inactive `#region`/`#endregion` become four spaces, and the tab in the disabled text is kept.
3. FMT-INDENT-010, the new example of an inactive region inside a method body at columns 2 and 10 (fixture r3).
4. FMT-INDENT-011, the new code-block example: an inactive `#pragma` at column 6 and a nested `#if`/`#endif` at column 3 are left unchanged (fixture d1).
5. FMT-INDENT-011, the new tab example: `\t#pragma\twarning disable CS0169` inside `#if NEVER` becomes `    #pragma    warning disable CS0169` (fixture d2).
6. FMT-INDENT-028, the second example: `int (x)`, `static int (x)`, `static x`, `static async x`, and `static () =>`, as one whole-pipeline test. `int (x)` is covered only by a test tagged FMT-BREAK-045. `static async x` and `static ()` with a block body are pinned by no test I know of. The skeptic's l1 shows `static ()` under `(`, but names no test for it.

Already pinned: region text in inactive branches (`DisabledRegionLayoutTests.RegionInsideDisabledCodeIsRenamedWithoutBlankLinePadding`, which also shows SHARED-BLANK-007), and item 24 (`MultiLineCommentGapSpacingTests`).

## #1936 — FMT-INDENT-021 Why text

### Changed requirements

- FMT-INDENT-021 — A wrapped `when` clause of a case label is indented one level — clarified, `Why:` only (decision 6). "the colon that ends the label on the `when` line" is now "the colon that ends the label". Fixture w2 confirms this: when the guard wraps, `when Check(x)` lands in column 16, `&& Check(value):` in column 21, and `{` in column 16, so the colon is not on the `when` line.

### Statements that need a characterization test and are not pinned

_None._ The change is text only, and `ListAndHeaderAlignmentTests.WrappedWhenClauseIsIndentedOneLevelDeeperThanCase` and `CaseWhenClauseContributorTests.AlignsWrappedWhenKeywordToCasePlusIndent` pin the requirement.

## Examples for the tests

FMT-INDENT-010 (three examples: r1, r2, r3), FMT-INDENT-011 (the two new examples: d1, d2), FMT-INDENT-028 (the second example).

## Output questions

_None._ Every statement describes today's output, which the maintainer confirmed in round 9.

## Internal choices

- FMT-INDENT-011 is retitled to name inactive directives, because it now owns them. The alternative was to keep the title. I found no test or document that quotes the old title.
- FMT-REGION-005 carried the same over-generalization as the Non-goals line that decision 7 names, so I qualified it with "active" too. The alternative was to fix only the Non-goals line, which would leave a false statement beside the fixed one.
- I added `static () =>` to the FMT-INDENT-028 example, because it is the case the new anchor wording exists for. The alternative was to state it in prose only.
- The tab example for an inactive non-region directive is prose, like the existing region tab example, because a spec code block cannot show a tab reliably.

## Follow-up candidates

_None._
