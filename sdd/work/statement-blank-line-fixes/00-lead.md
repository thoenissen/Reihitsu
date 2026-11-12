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
| 15 | Test implementer 04 | SHARED-AFTERBRACE-002 condition also matches a gap with no line break (` /* a */ L();`) | Spec author adds "at least one line break" in the post-implementer spec round (with the Code owners rows) | Matches today's tested output (`L();` stays on the brace's line) | Wording narrows to the tested, unchanged output | – |
| 16 | Test implementer 04 | Analyzer blank-line check tested through RH5002, RH5029, RH5011 only | Accept | All rules reach it through the two bases' one call | Internal | cfdf0b1 |
| 17 | Implementer 05 | 5 disputed `VerifyFormatter` tests (RH5011 else/do, RH5024–RH5027 region) whose inputs the formatter changes for unrelated reasons | Tests are wrong: test implementer makes each input formatter-stable, keeping what each asserts about the rule | The formatter output on these inputs is unchanged from `main`; only the test inputs are unstable | Formatter output unchanged; the asserted analyzer outcome is the decided one | – |
| 18 | Implementer 05 | Base inserts above a directive, RH3208/RH5030 behind it (`behindLeadingDirectives` flag) | Keep both (today's outputs); unifying is a follow-up carrying an output question | Unifying changes visible output, outside the issues | Both placements are `main`'s output | eca4a0d |
| 19 | Implementer 05 | `SyntaxTriviaUtilities.ContainsBlankLine` now has no production caller | Delete it and its Core tests in this run (test implementer: tests; implementer: method) | Cleanup the run's change touches; prevents the second measure from coming back (decision #8) | No caller, no output | – |
| 20 | Implementer 05 | SHARED-BLANK-006 `#endregion`-before-`}` exemption stated twice in `RegionDirectiveBlankLineUtilities` | Change structure now: the new check reuses RH5032's statement of the exemption | Narrow fix added a copy of a rule (README, *Architecture findings*) | Both agree on every input; no output change | – |
| 21 | Test implementer 06 | RH5011 formatter test: no formatter-stable `break;`-before-`else` input exists | Accept: test reduced to the do-footer case; `else` stays covered by the analyzer tests | The formatter braces unbraced if/else | Asserted outputs unchanged | 859aced |
| 22 | Spec author 08 | Code owner of `BlankLineCodeFixUtilities` (one spec per type): SHARED-STMT vs SHARED-AFTERBRACE | SHARED-STMT; SHARED-AFTERBRACE has no single-type owner (`—`) | Most of its code implements SHARED-STMT-004; SHARED-AFTERBRACE uses SHARED-STMT-004 | Internal ownership | 27237e0 |
| 23 | Auditor 11 | Finding #1: initializer + multi-line label continuation line, no other section statement starts its own line → wrong column | Refuse the rare class: withhold the fix there (code-fix only; diagnostic stays), record the gap in SHARED-STMT-004, tests for both cells (continuation deeper / at the label's column) | Rare class (RH5103 violation inside an initializer); answer 2A gives no column when no section statement starts a line | Withholding a fix for a rare class puts nothing in question (README) | – |
| 24 | Auditor 11 | Findings #2, #3 (text) and the "one owner" summary hint | Repair: spec author (SHARED-WS-001 admits a whole whitespace-only run; SHARED-BLANK-001 excludes a line inside disabled text), implementer (the two base-class summaries, `BlankLineCodeFixUtilities` summary) | Text only | No output change | – |
| 25 | Implementer 14 | RH3208 in the decision #23 shape | ~~Drop as follow-up~~ — superseded by #26: the premise ("keeps main's column") was wrong per 17-auditor | – | – | – |
| 26 | Auditor 17 | Finding #4 repeats #1's class (RH3208 calls the owner without the refusal check) — architecture finding, not a repair round | Refuse the class for RH3208 too, structurally: the owner of the blank line must not hand a fallback column to any caller in the refused cell, and RH3208 withholds its fix there (no no-op fix, SHARED-FIX-005); record the gap in SHARED-AFTERBRACE-002; RH3208 tests for both cells; also pin SHARED-BLANK-001's disabled-text example with a test | Rare class (README, *Refuse the class first*); a repeated class must not be repaired member by member | Withholding a fix for a rare class puts nothing in question | – |
| 27 | Implementer 20 | `CanRegisterCodeFix` hook on `ExpressionBodyToBlockCodeFixProviderBase`; RH3208's original-tree line check restates one condition of `RequiresBlankLineAfterClosingBrace` | Accept both; auditor 23 checks the restated condition | `RegisterCodeFixesAsync` is sealed; the registration-time check needs the original tree | Internal | 9989052 |
| 28 | Auditor 23 | Finding #5 repeats #4/#1: inside an initializer the owner reads the column from the line the preceding statement starts on without checking what starts that line (comment tail, wrapped argument, nested unbraced statement) | Architecture decision, not a repair round: widen the refusal in the owner — inside an initializer or anonymous object, a column is given only when the line starts with the first token of a statement of the target's own list, or a label of its switch section; otherwise the owner refuses and every caller (statement base, RH3208) withholds its fix. Record the widened gap in SHARED-STMT-004 and SHARED-AFTERBRACE-002; tests for each failing member (both callers) | Third member of one class: refuse the class instead of repairing members (README, *A repeated class*) | Rare class (two statements on one line inside an initializer, RH5103); withholding a fix for a rare class puts nothing in question | – |
| 29 | Auditor 23 | Finding #6: no test pins "no statement of the section other than the moved one" across several sections | Test implementer adds the SHARED-AFTERBRACE-002 example verbatim as an RH3208 test plus an RH5029 variant; implementer clarifies the inline comment in `ExpressionBodyToBlockCodeFixProviderBase.RegisterCodeFixesAsync` | Same requirement | No output change | – |
| 30 | Spec author 24 | Literal reading of decision #28 (own section's label only; only whitespace before the leading token; fallback only on label continuations; a line started by an earlier block's `}` refused) withdraws the fix from a few cells offered today | Accept as written | All cells hold two statements on one line inside an initializer (RH5103) — rare; a simple cheap check beats a wider exception list | Withholding a fix for a rare class puts nothing in question | 2ea0336 |
| 31 | Test implementer 25 | Spec 24's literal wording refuses 4 existing RH5029 fix tests (`main` outputs): comment in front of the leading token on its own line; another section's label or statement starting the line | Narrow the spec wording, keep the 4 tests: a line counts as started by a statement or label when only whitespace and comments that start on that same line stand in front of its first token, and inside a switch, statements and labels of any section of the same switch count; a comment's tail from an earlier line still refuses. Amends decision #30 | Keeps correct `main` outputs; the failing members of finding #5 stay refused; still a cheap trivia check | Keeps `main`'s output for those inputs | – |
| 32 | Spec author 26 | Fallback uses the comment-tolerant "starts its line"; fallback limited to the own section; another section's label continuation counts | Accept all three | Consistent with decision #31 and `main`'s label-region handling | Rare class; no output in question | 9c87631 |
| 33 | Implementer 27 | `VerifyDiagnosticDoesNotCompensateWhenSwitchLabelItselfIsNotFirstOnItsLine` (switch on one line inside an initializer) expects a guessed fix the spec now refuses | (a): the test becomes "no code fix offered" | Rare class (RH5103 inside an initializer); the test itself called the column undecided; refusing needs no code | Withholding a fix for a rare class puts nothing in question | – |
| 12 | Skeptic | Spec-diff wording findings (SHARED-STMT-004 undecided-case note, SHARED-BLANK-001 `///`→ also directives, comment sentence wording, initializer example, RH5023 spaces case) | Route all to a fresh spec author after the maintainer's answers | Internal wording; batched with the answers' spec changes | No output changes | – |

## Follow-up candidates

- RH5103's fix over-indents a statement after a multi-line `case` label; fix like #1096 and remove the RH5103 page sentence (01-spec-author)
- A switch written on one line inside an initializer is undecided by SHARED-STMT-004 and RH5103 (01-spec-author)
- Narrow RH5030-003 once the gap can be rebuilt (#1626 says "later"; visible, maintainer decides) (01-spec-author)
- (taken into run as decision #6) RH5026/RH5027 region cycle
- Formatter: a wrapped `when` condition of a `case` label is put in the column of `case` — carries an output question (A: one level deeper than `case`), 02-skeptic #1096
- Structural: statement blank-line code fix predicts the indentation phase's column (`ComputeAnchorDerivedIndentation`, shared with RH5103) (02-skeptic, decision #9)
- Unify the blank line's placement around a directive in the gap: statement base inserts above, RH3208/RH5030 behind (05-implementer, decision #18; visible, carries an output question)
- Formatter's `BlankLineRegionDirectiveRewriter.EnsureBlankLineAfterRegionDirectives` states the SHARED-BLANK-006 `#endregion`-before-`}` exemption a third time; could call `RegionDirectiveBlankLineUtilities.RequiresBlankLineBelow` (07-implementer; pre-existing, no output change)
- One answer for "statement behind a multi-line comment" across RH5011, RH5030, the code-fix base (02-skeptic, decision #11)

## Worklist / next step

- Output-question batch 1 asked (before test implementer): Q1 RH5011-001 doc-comment clause (02 OQ1), Q2 SHARED-STMT-004 column in an initializer (02 OQ2), Q3 RH3208 output (02 OQ4, replaces 01 OQ2). Answered 1A, 2A, 3A.
- PR note: the RH5002 non-goal #972's comment mentions is already absent on main.
- Spec amendment done (03). Code owners rows wait for the implementer's choice of owner type (repair round for spec author).
- Tests written (04, cfdf0b1): 94 failing tests reproduce #946, #972, #1096, #1076, #1625.
- Open spec-author worklist after implementer: Code owners rows for the #1626 owner; SHARED-AFTERBRACE-002 "at least one line break" (decision #15); SHARED-BLANK-001 Why revert only if decision #8 is not done.
- Implementer done (05, eca4a0d): 89/94 pass; owner of the blank line in front of a statement = `BlankLineCodeFixUtilities.InsertBlankLineBefore` (no new type). Escalation 9 not reached.
- Follow-up candidate added: unify the blank-line placement around directives between the statement base and RH3208/RH5030 (decision #18).
- Test implementer 06 done (859aced).
- Implementer 07 (0420faf) and spec author 08 (27237e0) done; all test projects green in Debug.
- Cleaner 09 (47b0eda): clean. Spec checker 10 (0451767): consistent, no findings.
- Audit 1 (11, e996463) at cf077d0: findings #1 high, #2/#3 low text.
- Repair round 1 done; audit 17 (23c967f): #1–#3 closed; #4 repeats #1 → architecture decision #26 (does not use a repair round).
- Decision #26 done; cleaner 21, spec checker 22 clean; audit 23 (6cffb22) at 9906bb7: #4 closed; #5 repeats class → decision #28; #6 → decision #29.
- Repair rounds used: 1 (architecture decisions #26, #28 do not count).
- Spec author 24 (2ea0336), test implementer 25 (526ebbf: 12 failing reproductions) done. Decision #31 narrows the wording.
- Spec author 26 (9c87631) done.
- Implementer 27 (663da84) done; one test disputed → decision #33.
- Test implementer 28 (fc5ff89), cleaner 29 (af60316) clean, spec checker 30 (d028e40) consistent.
- Next: auditor 31 (since 9906bb7).
- Follow-up candidate added: inside an initializer, a statement moved inside a block on a `case` label's line takes the label's column (pre-existing; fits decision #9's follow-up).
