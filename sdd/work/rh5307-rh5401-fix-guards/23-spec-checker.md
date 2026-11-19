Role: spec checker
Commit: 704072c
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Scope: the change after 5e2f356 apart from the merge of `origin/main` (63a4fb5): commits 26dc238, 926712f, eb8475c, 9075fc0 (decisions 12, 13).

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 1, 2 | `sdd/changes/2026-11/rh5307-rh5401-fix-the-reported-construct.md`, *Visible output changes*, the offered block (RH5401 "before: no fix is offered / after: the fix gives") lists A, B, L, E, J, V | Example `Y` (`[Obsolete("a", // c⏎ false)]⏎int Y⏎    => 1;`) is in the RH5401-003 offered example and has a test (`VerifyFixKeepsALineCommentBehindCodeInsideTheArgumentsOfAnAttribute`), but the change doc lists it nowhere. On `main` a property with attribute lists got no fix, so `Y` goes from no fix to a fix, like `J` and `V`, and belongs in that block. (The earlier `Q` there was moved to the withheld block and is gone.) | spec author |

## Checked
1. Change doc: finding 1. Otherwise ok: still `Accepted`, has `Visible output changes`, no task list. Decisions 12 and 13 are recorded in the question-6 decision as run decisions, not maintainer answers. `N`, `Q`, `T` in the "unchanged" block are correct against `main`, which withheld every attributed property. `U` is stated as fixed before and after, which is true on `main`. Two alternatives (whole-region count, own-line-comment-only variant) are listed.
2. Specs match the diff: finding 1 only. RH5401-004 states the new refusal (blank line or own-line comment between the brackets, between two tokens from `[` through `]`, also inside attribute arguments, split or not). It states the outside of the condition (trivia before `[`, behind `]`), directives and disabled text being excluded, and the offered side (`U`, `V`, `Y`, `J`, `L`, `E`, `F`). The code does exactly this: `HoldsBlankLineOrOwnLineCommentBetweenBrackets` checks the gap blank-line count and own-line comments in leading trivia for each gap from `[` to `]`, and is applied to every list before formatting. The whole-region count and the false "each condition catches what the other does not" sentence are gone from spec, change doc, and code; no `S` example remains. RH5401-003's condition wording agrees. Format (normative sentence first, `Why:`, examples, markup `{|RH5401:…|}`) holds.
3. IDs: ok. No ID added or touched; `sdd/specs/requirement-ids.txt` has no diff against `origin/main`.
4. Code owners: ok. Spec headers untouched by this round; `scripts/test.sh --no-install --project architecture`: "Passed!  - Failed: 0, Passed: 198, Skipped: 0, Total: 198".
5. User documentation: ok. `documentation/rules/RH5401.md` is unchanged this round; the new no-fix inputs are a withheld-fix corner case that the page must not list.
6. Open questions: ok. The round raised no output question; decisions 12 and 13 are in `00-lead.md` and, by the rare-class rule, local and silent. `U` is explicitly kept as on `main` (decision 13). The spec author's follow-up candidates: none.
7. Coverage: ok. Architecture result quoted in 4. Every example the amendment adds or moves (`J`, `U`, `V`, `Y` offered; `N`, `O`, `P`, `Q`, `R`, `T`, `W` withheld; `L`, `E`, `F`) has a covering test.
8. Tags: ok. All added or changed tests carry `[Requirement("RH5401-004")]`; the offered-side tests (`J`/`U`/`V`/`Y`/`E`/`F` and the renamed combined test) also carry `RH5401-003`; table 20 names each, and every example under *Examples for the tests* of 19 (as amended by decision 13) is tagged on a test.
9. Nothing left behind: ok for now. `sdd/work/` holds only `rh5307-rh5401-fix-guards`; the lead deletes it before publishing.
