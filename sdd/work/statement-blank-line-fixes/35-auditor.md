Role: auditor
Commit: e24ebec6d4266063b4af39e51b4a489cd033f845
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope
Repair delta against 31-auditor (audited at 7f5f965): `7f5f9654280e2cfa1ee1118aacde94f10a2ed08d..e24ebec6d4266063b4af39e51b4a489cd033f845`. Not a full audit; the scope did not grow. The delta changes one compiled file, `src/Reihitsu.Analyzer.Test/Formatting/RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests.cs` (+147 lines, three tests), plus handovers 31–34 and `00-lead.md`. No production code, spec, or rule page changed.

State checks:
- The checkout is e24ebec and the tree is clean.
- `git ls-remote origin refs/heads/main` is c884fcc2548db02e608e82fc8e5f386029e34439, the base in my prompt, and it is an ancestor of the head.
- `scripts/lane-check.sh bug-fix`: confirmed.
- The diff matches 32-test-implementer (the three tests), 33-cleaner (no formatting change), and 34-spec-checker (no spec change).

Evidence:
- Static reading of the three tests against `BlankLineCodeFixUtilities.TryGetListIndentation`/`StartsLineBehindComments` (unchanged since 663da84).
- Two temporary mutants of `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs`, each run against the whole `Reihitsu.Analyzer.Test.Formatting` namespace (4055 tests) and then restored from a copy. The tree is clean again.
- Two fixtures in the scratchpad through `scripts/apply-fix.sh RH5029 … --no-install`, LF and CRLF, deleted afterwards.

Previous findings:
- **#7: closed.** `VerifyNoCodeFixWhenPrecedingStatementStartsOnLastLineOfMultiLineCommentStartingItsOwnLineInsideObjectInitializer` puts the comment in the leading trivia of `Method(1)` (the comment starts its own line). 32's mutant C (line starter checked with `SyntaxTokenPositionUtilities.IsFirstOnLine`) fails only this test. The RH3208 counterpart stays optional: RH3208 reaches the same line-starter check through the shared owner.
- **#8: closed** for both members it named. The strict-fallback mutant (A) fails `VerifyMovedStatementTakesColumnOfSectionStatementLineStartingWithCommentWhenLabelSpansMultipleLines`. The trivia-blind mutant (B) fails `VerifyNoCodeFixWhenOnlySectionStatementBelowLabelContinuationStandsBehindTailOfMultiLineComment`. Two members its enumeration did not name are still untested; they are reported as #9.
- **#1–#6: closed** in earlier audits. The delta changes no compiled production code, so nothing reopens them.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 9 | medium | `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs:228-239` (the label-continuation fallback); no test in the RH5029, RH3208 or RH5002 classes | FC-017, FC-003 | #8 (member (b) also crosses #7's axis) | same requirement (SHARED-STMT-004, maintainer answer 2A) | **What it is.** The fallback for a label continuation line is tested along only some of its axes. On the head the code holds on every member. Two members have no test, and a mutant on each survives all 4055 `Formatting` analyzer tests.<br><br>**(a) Which column the fallback reads.** Answer 2A and SHARED-STMT-004 say the moved statement takes "the column of the section's statements as written, the column of `break;`". In every fallback test of the run, `break;`/`return;`/`/* c */ break;` stands exactly one level deeper than the `case` keyword. That includes the new test 2 and the existing `VerifyMovedStatementTakesSectionColumn…` tests in RH5029, RH5002 and RH3208. So the column as written equals "label column plus one level", the column the formatter would predict. The tests distinguish it only from "continuation line plus one level".<br>- Mutant E: line 236 reads `GetLineIndentation(sourceText, sourceText.Lines.GetLineFromPosition(label.SpanStart)) + "    "`. Formatting tests: 4055 passed, 0 failed.<br>- Fixture H01: `case 1 when⏎    value > 0: Method(1); var y = 1;⏎      break;`, with `break;` two spaces deeper than the canonical column. Head: `var y = 1;` lands in `break`'s column (as written), LF and CRLF, 1 iteration. Mutant E would put it two columns left, the formatter-predicted column that answer 2A rejected ("no prediction of the formatter").<br><br>**(b) Which statement qualifies: a comment tail in leading trivia.** This is #7's axis (where the multi-line comment sits) applied to the fallback site instead of the line-starter site. The new test 3 covers a comment tail only in the *trailing* trivia of the statement before it (`var y = 1; /* c⏎ d */ break;`). It does not cover a comment that starts its own line and ends in front of the fallback statement (`var y = 1;⏎ /* c⏎ d */ break;`).<br>- Mutant D: line 232 uses `SyntaxTokenPositionUtilities.IsFirstOnLine(statement.GetFirstToken())`, the same Core-helper substitution #7 guarded against at line 223. Formatting tests: 4055 passed, 0 failed.<br>- Fixture H02 (`case 1 when⏎    value > 0: Method(1); var y = 1;⏎    /* c⏎            d */ break;`): head `no-fix-offered`, LF and CRLF, as SHARED-STMT-004 requires. Mutant D offers a fix that puts `var y = 1;` in the column of the `d */` line, which is the comment's layout.<br><br>**Members that hold, with a test:**<br>- strict vs comment-tolerant: test 2<br>- trivia-blind, tail in trailing trivia: test 3<br>- own section only (#6)<br>- no other line-starting statement: the existing refusal tests<br>- not "continuation plus one level": `VerifyMovedStatementTakesSectionColumnWhenLabelSpansMultipleLinesAndPrecedingStatementSharesItsContinuationLine` | Test implementer: add two RH5029 tests inside an object initializer, each with a `case … when` label continuation and no other statement of the section in front:<br>(a) `break;` at a column other than the `case` keyword's column plus one level, such as two spaces deeper; the fix is offered and `var y = 1;` takes `break`'s column exactly.<br>(b) a multi-line comment that starts its own line below the continuation line, with `break;` behind its last line; no code fix is offered. |

Visible output (check 10):
- Test 2's expected column, the line of `/* c */ break;`, is demanded by SHARED-STMT-004 and answer 2A. Tests 1 and 3 assert a withheld fix, which SHARED-STMT-004 states; that is the rule's local decision for a rare class (decisions #28, #31, #34).
- No rule page changed. No `undecided output`.

## Guard scope
_N/A — the repair adds no predicate, guard, or exemption. 31-auditor's guard-scope trace of `TryGetListIndentation`/`StartsLineBehindComments` is byte-identical evidence (no compiled production change since 663da84). Its fallback-qualifier members are tested as described under #9._

## Policy ownership
_N/A — the repair changes no policy owner. 31-auditor's table stands. Mutants D and C show that two places would silently accept the Core helper `IsFirstOnLine` in place of the private `StartsLineBehindComments`: line 223 is now pinned (#7 closed), line 232 is not (#9 b)._

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyNoCodeFixWhenPrecedingStatementStartsOnLastLineOfMultiLineCommentStartingItsOwnLineInsideObjectInitializer` | Line starter behind the tail of a comment in leading trivia: refused | Any registered action. Mutant C (`IsFirstOnLine` at line 223) fails it, per 32. | `VerifyNoCodeFixIsOffered` (exact diagnostic, then no action) | adequate |
| `VerifyMovedStatementTakesColumnOfSectionStatementLineStartingWithCommentWhenLabelSpansMultipleLines` | The fallback admits a statement behind a comment that starts on its line, and the moved statement takes that line's column | No fix (mutant A fails it); another column; a missing blank line; trailing whitespace; a second iteration | `Verify(source, fixed)`, exact text, 1/1 iterations | adequate for the qualifier. It cannot falsify a label-derived column, because the line's column equals the label column plus one level (#9 a). |
| `VerifyNoCodeFixWhenOnlySectionStatementBelowLabelContinuationStandsBehindTailOfMultiLineComment` | The fallback refuses a statement behind the tail of a comment that starts earlier | Any action (mutant B fails it) | `VerifyNoCodeFixIsOffered` | adequate for trailing trivia. The leading-trivia placement is untested (#9 b, mutant D survives). |

## Failure classes dismissed
- **FC-017, FC-003:** reported as #9.
- **Everything else:** the repair adds tests only. Every other class (FC-001/002 guard spans, FC-004–FC-016 and FC-018–FC-043 rewrites, convergence, directives, formatter phases, policy owners, comments) cannot be reached by a change that adds no compiled production code. 31-auditor's dismissals hold unchanged for the production code, which is unchanged since 663da84.
- **FC-031:** the three test summaries describe what each test asserts. There is no `TODO` and no tracker number. Each test carries `[Requirement("SHARED-STMT-004")]`.

## Architecture findings
_None._ #9 is a test gap on a predicate whose code holds on every member. Nothing is refused and no structure fails. It does repeat #6 → #8 → #9: a qualifier of the same fallback predicate left untested, now for the third time. The full axis list is in #9, so the remaining members can be closed in one step.

## Hints
- The run's fallback tests, which I counted, all place the section's statement at the canonical column. A fixture-driven sweep of the fallback (statement column × comment placement × section) would have shown #6, #8 and #9 together. That observation is only about the method; it is not a finding.
- 32-test-implementer's mutant table names mutant C at line 223 only. The equivalent substitution at line 232 (mutant D) was not tried; see #9 b.
