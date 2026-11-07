Role: test implementer
Commit: the commit that adds this file (tests and handover in one commit), on top of b9adb61
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from FMT-SPACE-009 (changed), FMT-BREAK-037 (its new example), change-doc entries 14 and 15, and, for D2, the lambda layout that unchanged requirements give (decision 7). No production code was touched.

All tests were run on this head (b9adb61, the implementer's da0e710 code). Every failing test fails on the output difference it is written for: a missing space at the constraint colon, or a clause behind a kept colon left on one line. None fails on a compile error, a skipped syntax error, a phase-stability check, or idempotency.

## D1–D3 confirmed

- **D1** (7 tests expecting `where T : X` after a join): FMT-SPACE-009 now covers the constraint colon, "also where a join wrote it without a space". The tests match the spec; unchanged. On this head they fail only on the missing space (`where T :IDisposable`, `where T: class`).
- **D2** (`ConstraintClauseAlignmentTests.LocalFunctionInLambdaClauseAligns`): expected output corrected per decision 7. The lambda block stands at the lambda's `(` (column 26), the local function at 30, `where` at 34, `new()` under `class` at 44. Input unchanged. Passes on this head.
- **D3** (`RH5110…VerifyFixIsOfferedWhereCommentKeepsColonOnItsOwnLine`): its expected `: class,⏎  new()` is exactly change-doc entry 15's fix result. Unchanged; fails on this head on the one-line clause behind the kept colon.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| ConstraintClauseAlignmentTests.LocalFunctionInLambdaClauseAligns (expected corrected) | FMT-INDENT-032 | AssertRuleResult | pass |
| HorizontalSpacingRequirementTests.OneSpaceBeforeBracesAndAroundDeclarationColons (spec example extended with `N<T>() where T:class`) | FMT-SPACE-009 | AssertRuleResult | fail (`where T:class` kept) |
| ConstraintClauseColonSpacingTests.ConstraintColonsGetOneSpaceOnEachSide (change-doc entry 14) | FMT-SPACE-009 | AssertRuleResult | fail |
| …SpacedConstraintColonStaysAndWiderGapsBecomeOneSpace | FMT-SPACE-009 (stable side; wider gap) | AssertRuleResult | pass |
| …ConstraintColonsOfLocalFunctionDelegateAndMemberWithoutBodyAreSpaced | FMT-SPACE-009 | AssertRuleResult | fail |
| …BaseListColonAndConstraintColonsOfOneDeclarationAreSpaced | FMT-SPACE-009 | AssertRuleResult | fail (base-list colon already spaced; constraint colons not) |
| …TightColonOfWrappedClauseIsSpacedAndConstraintsAlignBehindIt | FMT-SPACE-009, FMT-INDENT-032 | AssertRuleResult | fail (`new()` at 16, not 18) |
| …CommentInFrontOfConstraintColonKeepsItsPlace | FMT-SPACE-009, FMT-SPACE-013 | AssertRuleResult | fail |
| …CommentBehindConstraintColonKeepsItsPlace | FMT-SPACE-009, FMT-SPACE-013 | AssertRuleResult | fail |
| …ConstraintColonStartingALineIsSpacedOnlyBehindIt | FMT-SPACE-009 | AssertRuleResult | fail (`:class`) |
| …NameColonNextToConstraintClauseKeepsItsSpacing | FMT-SPACE-009, FMT-SPACE-019 | AssertRuleResult | fail (constraint colon only; `a: 1` already right) |
| ConstraintClauseLineBreakTests.ColonKeptOnItsOwnLineByCommentWrapsClauseWrittenOnOneLine (spec example: `M` wraps, `N` joins) | FMT-BREAK-037 | AssertRuleResult | fail (`M` stays on one line; `N` gives `where T:` without space, D1) |
| …ColonKeptOnItsOwnLineByCommentWrapsEveryConstraint | FMT-BREAK-037 | AssertRuleResult | fail |
| …ColonKeptOnItsOwnLineByDirectiveWrapsClause | FMT-BREAK-037 | AssertRuleResult | fail |
| …ColonKeptOnItsOwnLineByDisabledTextWrapsClause | FMT-BREAK-037 | AssertRuleResult | fail |
| …ColonKeptOnItsOwnLineByCommentKeepsSingleConstraintOnItsLine | FMT-BREAK-037 (single-constraint side) | AssertRuleResult | pass |
| …TightColonKeptOnItsOwnLineByCommentIsSpacedAndWrapsClause | FMT-BREAK-037, FMT-SPACE-009 | AssertRuleResult | fail |
| RH5110…AnalyzerTests.VerifyFixSpacesTightConstraintColon | RH5110-003 | Verify (BatchCodeFixTestsBase) | fail (fix result `where T:class`) |

`AssertRuleResult` runs every formatter test under LF and CRLF, with a second pass, the per-phase stability check, and the phase round trip. Combined-pipeline coverage is therefore built in: the line-break phase and spacing in `Tight…` tests, and spacing ahead of indentation in `TightColonOfWrappedClause…`, where the alignment column depends on the space.

## Not covered, and why

- **Owner dimension of the colon spacing:** type, method, local function, delegate, and interface member without a body are covered. Extension blocks and record/struct/interface owners are not tested separately for spacing. The constraint colon is the `ColonToken` of `TypeParameterConstraintClauseSyntax` for every owner, and the prompt names no owner-specific spacing. The implementer's rule may still key on the owner; if it does, the auditor should ask for one more owner test.
- **A directive in a one-line colon gap:** impossible, because a directive ends its line, and then FMT-SPACE-001 leaves the gap alone. The directive cases are the FMT-BREAK-037 tests with a kept colon (`…ByDirectiveWrapsClause`, `…ByDisabledTextWrapsClause`).
- **A multi-line block comment keeping the colon on its own line:** dropped. The column of the comment's continuation line is not decided by this run's specs. A single-line `/* */` ending the line is the same gap shape as the `//` case.
- **RH6021:** unchanged. RH6021-002's example `where T:class` is still not reported (analyzer-only `Verify`; no formatter step). The new-rule candidate is lead follow-up 8.
- **FC-032:** `rg -n "where [A-Za-z_@]+ ?:[^ :]|where [A-Za-z_@]+: "` over `src`, `documentation`, and `tests` finds no other test or doc asserting a tight constraint colon in formatter output. The only hit outside this run's files is the analyzer-only RH6021 test above. Self-hosting is CI's.
- **FC-004/FC-005/FC-019–FC-021** for the kept colon: comment, directive, and disabled text are covered. The blank-line variant of a kept colon is not added, because FMT-BREAK-030 is unchanged and `BlankLineBehindCommaIsKept` covers kept-gap blank lines.
- **Other failure classes:** same as `05-test-implementer.md`. Nothing in this repair changes their reach.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Spacing.ConstraintClauseColonSpacingTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Spacing.HorizontalSpacingRequirementTests.OneSpaceBeforeBracesAndAroundDeclarationColons"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.LineBreaks.ConstraintClauseLineBreakTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Indentation.ConstraintClauseAlignmentTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5110"
```
