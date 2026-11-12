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

- Batch 1, Q1 (RH5011-001 doc-comment clause): **A** — keep reporting a `break` followed by a `///` comment with no blank line; same fix as today; delete the "holds no documentation comment" clause from RH5011-001.
- Batch 1, Q2 (SHARED-STMT-004 column of the moved statement inside an initializer after a multi-line `case … when` label): **A** — the column of the section's other statements as written (column of `break;`), no prediction of the formatter.
- Batch 1, Q3 (RH3208 `int L() => 1; /* a⏎    b */ L();`): **A** — `} /* a⏎    b */⏎⏎    L();` (blank line, `L();` at the block's indentation, no trailing whitespace); produced through the #1626 single owner of the blank line in front of a statement.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's required branch `claude/nice-thompson-cq0rqf` instead of `claude/batch-946-<slug>` | The execution environment allows pushing only to that branch | Branch names are internal | claim commit |
| 2 | Spec author | No change doc | Accepted: no change doc | #1076/#1096 decided as bugs by the maintainer; #946/#972 required by shared specs | Bug fixes back to decided requirements | 176b419 |
| 3 | Spec author | Add RH5011-008 exception to the RH5011 rule page? | No | Rule pages list no exceptions or corner cases (AGENTS.md key conventions) | Rule page unchanged | – |
| 4 | Spec author | Remove the object-initializer limitation paragraph from 20 shared-base rule pages | Accepted | #1096's decision asks to remove the sentence documenting the limitation; it is also implementation detail (AGENTS.md rule-page convention; PR states that reason) | Covered by the maintainer's #1096 decision and the rule-page convention | 176b419 |
| 5 | Skeptic | 01-spec-author OQ1: RH5005/RH5006 diagnostic location on `await using`/`await foreach` | Not asked; keep it on `using`/`foreach`, record the location in RH5005-002/RH5006-002 | Today's output, consistent with RH7004 | Location unchanged from `main` | – |
| 6 | Skeptic | 02 OQ3: RH5026/RH5027 report the blank line below a region directive (cycle with RH5032) | Not asked; take in as adjacent fix: not reported, same as RH5024/RH5025 | Same class as the #1076 region instances the maintainer decided (the blank line SHARED-BLANK-006 requires stays); same analyzers the run already changes | Brings the code back to unchanged SHARED-BLANK-006, as the maintainer's #1076 answer decided for the sibling rules | – |
| 7 | Skeptic | #972 goal names RH5001–RH5010, RH5012–RH5019 only | Widen to every rule on the base (RH5021, RH5029 too) | Same base, same defect | SHARED-STMT-004 already demands one blank line | – |
| 8 | Skeptic | Architecture finding 1: two blank-line measures (`ContainsBlankLine` vs `TokenGapAnalysis`) | Change the structure in this run: the two analyzer bases measure blank lines with the one measure; implementer reports if it exceeds ~100 production lines or needs a new type | The failing class includes common code (`#endif⏎⏎// note⏎if`); a narrow fix keeps two statements of SHARED-BLANK-001 | Brings analyzers back to unchanged SHARED-BLANK-001 | – |
| 9 | Skeptic | Architecture finding 2: code fix predicts the indentation column (#1096 Q1) | Fix narrowly (add the level only on a line that starts with the label's first token); structural cause becomes a follow-up | Rare class (RH5103); maintainer decided the output | Depends on OQ2 answer | – |
| 10 | Skeptic | Architecture finding 3: region exemption missing in four deleting rules | One shared cheap check used by RH5024–RH5027 | Avoids four copies of one rule | Same as #6 | – |
| 11 | Skeptic | Architecture finding 4: four answers to "statement behind a multi-line comment" | Run takes #1626 only as far as OQ4 needs; unifying RH5011/RH5030 is a follow-up | Keeps scope bound to the run's issues | RH5030-003 narrowing is visible and not asked here | – |
| 13 | Spec author 03 | Initializer column wording: "as written" vs decision #9's algorithm | Accept "as written"; decision #9's narrow fix must produce that column (read from a statement that starts its own line in the section) | Maintainer answer 2A | Answer 2A covers it | 7df80b0 |
| 14 | Spec author 03 | SHARED-BLANK-001 Why names analyzers as users of `TokenGapAnalysis` | Accept; reverts in a repair round if decision #8 is over budget | Follows decision #8 | Internal wording | 7df80b0 |
| 12 | Skeptic | Spec-diff wording findings (SHARED-STMT-004 undecided-case note, SHARED-BLANK-001 `///`→ also directives, comment sentence wording, initializer example, RH5023 spaces case) | Route all to a fresh spec author after the maintainer's answers | Internal wording; batched with the answers' spec changes | No output changes | – |

## Follow-up candidates

- RH5103's fix over-indents a statement after a multi-line `case` label; fix like #1096 and remove the RH5103 page sentence (01-spec-author)
- A switch written on one line inside an initializer is undecided by SHARED-STMT-004 and RH5103 (01-spec-author)
- Narrow RH5030-003 once the gap can be rebuilt (#1626 says "later"; visible, maintainer decides) (01-spec-author)
- (taken into run as decision #6) RH5026/RH5027 region cycle
- Formatter: a wrapped `when` condition of a `case` label is put in the column of `case` — carries an output question (A: one level deeper than `case`), 02-skeptic #1096
- Structural: statement blank-line code fix predicts the indentation phase's column (`ComputeAnchorDerivedIndentation`, shared with RH5103) (02-skeptic, decision #9)
- One answer for "statement behind a multi-line comment" across RH5011, RH5030, the code-fix base (02-skeptic, decision #11)

## Worklist / next step

- Output-question batch 1 asked (before test implementer): Q1 RH5011-001 doc-comment clause (02 OQ1), Q2 SHARED-STMT-004 column in an initializer (02 OQ2), Q3 RH3208 output (02 OQ4, replaces 01 OQ2). Answered 1A, 2A, 3A.
- PR note: the RH5002 non-goal #972's comment mentions is already absent on main.
- Spec amendment done (03). Code owners rows wait for the implementer's choice of owner type (repair round for spec author).
- Next: test implementer (04).
