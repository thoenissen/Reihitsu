Role: auditor
Commit: c3a58b23215b0d921f4ad3024d4f9b6f12f5ec25
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict. It limited this audit to decision #35: whether finding #9 is closed.

## Scope
This is a limited repair-delta audit (decision #35) against 35-auditor, which audited at e24ebec. Head c3a58b23215b0d921f4ad3024d4f9b6f12f5ec25, delta `e24ebec6d4266063b4af39e51b4a489cd033f845..c3a58b23215b0d921f4ad3024d4f9b6f12f5ec25`. Base `origin/main` c884fcc2548db02e608e82fc8e5f386029e34439. The scope did not grow.

State checks:
- The checkout is c3a58b2 and the tree is clean.
- `git ls-remote origin refs/heads/main` is c884fcc, the base in my prompt, and it is an ancestor of the head.
- `scripts/lane-check.sh bug-fix`: confirmed.

What the delta changes:
- One compiled file, `src/Reihitsu.Analyzer.Test/Formatting/RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests.cs`. It adds two tests (+112 lines).
- Handovers 35–38 and `00-lead.md`.
- No production file, spec, ledger, or rule page.
- `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` was last changed in 663da84.

The diff matches 36-test-implementer (the two tests), 37-cleaner (nothing to format, build clean), and 38-spec-checker (no spec change).

Evidence:
- I read both tests statically against SHARED-STMT-004 and against `TryGetListIndentation`/`StartsLineBehindComments`.
- I re-ran mutants D and E myself, each as a temporary edit of `BlankLineCodeFixUtilities.cs`, against the RH5029 test class (52 tests). After each run I restored the file from a copy and checked it with `cmp`. Its SHA-256 is 485b00e3… before and after, and `git status` is clean.

| Mutant | Edit | Result |
|---|---|---|
| D | line 232: `StartsLineBehindComments(sourceText, statement.GetFirstToken())` → `SyntaxTokenPositionUtilities.IsFirstOnLine(statement.GetFirstToken())` | 1 failed / 51 passed. Only failure: `VerifyNoCodeFixWhenOnlySectionStatementBelowLabelContinuationStandsBehindTailOfMultiLineCommentStartingItsOwnLine` |
| E | line 236: `GetLineFromPosition(lineStartingStatement.SpanStart))` → `GetLineFromPosition(label.SpanStart)) + "    "` | 1 failed / 51 passed. Only failure: `VerifyMovedStatementTakesColumnOfSectionStatementAsWrittenWhenLabelSpansMultipleLines` |

Previous findings:
- **#9: closed**, both members.
  - **(a)**, the column is read as written. In the test, `break;` stands at relative column 22, two spaces deeper than the canonical column 20 (`case` is at 16). The expected output puts `var y = 1;` at column 22, `break`'s column. SHARED-STMT-004 demands that column: "the moved statement takes the column of another statement of its own switch section that starts its line, such as a `break;` below", and "not a column read from the label's continuation line". Answer 2A demands the same: no prediction of the formatter. Mutant E (label column plus one level) puts it at 20 and fails.
  - **(b)**, a comment tail in leading trivia. `/* c⏎ d */` starts its own line below the continuation line, so it belongs to the leading trivia of `break`. Under SHARED-STMT-004's definition, `break` does not start its line: a comment whose text starts on an earlier line stands in front of it. `Method(1);` shares the label's line. So no statement of the section other than the reported one starts its line, and the requirement says "the fix MUST NOT be offered", with the diagnostic still reported. The test asserts exactly that. Mutant D makes `break` qualify and fails it.
- **#1–#8: closed** in earlier audits. The delta changes no compiled production code, so nothing reopens them.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

Visible output (check 10):
- Test (a)'s expected column is demanded by SHARED-STMT-004, which the run does not change in this delta, and by maintainer answer 2A.
- Test (b) asserts a withheld fix, which SHARED-STMT-004 states. That is the rule's local decision for a rare class.
- No rule page changed. Nothing is `undecided output`.

## Guard scope
_N/A — the delta adds no predicate, guard, or exemption. The guard-scope trace in 31-auditor stands as byte-identical evidence: no compiled production change since 663da84._

## Policy ownership
_N/A — the delta changes no policy owner. The table in 31-auditor stands. Line 232 is now pinned against the Core-helper substitution (mutant D), as line 223 already was (mutant C)._

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyMovedStatementTakesColumnOfSectionStatementAsWrittenWhenLabelSpansMultipleLines` | Below a label continuation line, the moved statement takes the column of the section's line-starting statement as written. Here that column is not the label column plus one level (22 vs 20). | No fix offered; any other column (mutant E gives 20); a missing blank line; trailing whitespace (the blank line in the expected text is empty); a second iteration | `Verify(source, fixed)`: exact text, diagnostic at `{|RH5029:var|}`, 1 incremental / 1 Fix All iteration | adequate |
| `VerifyNoCodeFixWhenOnlySectionStatementBelowLabelContinuationStandsBehindTailOfMultiLineCommentStartingItsOwnLine` | Below a label continuation line, a section statement behind the tail of a comment that starts its own line does not qualify. With no other qualifying statement, the fix is withheld and the diagnostic is reported. | Any registered action (mutant D offers one) | `VerifyNoCodeFixIsOffered` (exact diagnostic, then no action) | adequate |

With mutants A, B, C (32) and D, E (36, re-run here), every member of the axis list in #9 now has a test that fails on its own mutant. Both tests carry `[Requirement("SHARED-STMT-004")]`. Their summaries describe what each test asserts (FC-031), with no `TODO` and no tracker number.

## Failure classes dismissed
- **FC-017, FC-003:** the gap reported as #9 is closed (see above).
- **Every other class:** cannot be reached by a delta that adds two tests and no compiled production code. The dismissals in 31-auditor and 35-auditor hold for the production code, which is unchanged since 663da84.

## Architecture findings
_None._ The repeat chain #6 → #8 → #9 is closed. No qualifier of the label-continuation fallback is left without a killing test.

## Hints
- The new tests are LF only and RH5029 only, like their neighbours. 35-auditor ran H01 and H02 under CRLF with the same results. RH3208 and RH5002 reach the same `TryGetListIndentation` through the shared owner, so the mutants are pinned for them as well. This is not a finding.
