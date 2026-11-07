Role: test implementer
Commit: bc05c66 (production code at 317a6a5, unchanged)
Verdict: tests written

Correction round for decisions 12 and 13. My prompt carried no intended fix or hoped-for verdict. I worked from `00-lead.md`, `04-test-implementer.md`, `06-implementer.md`, the changed specs, and the change doc `sdd/changes/2026-11/raw-string-tab-one-column.md`. Every expected value below was read from a requirement first; I then confirmed it against the Debug CLI output at 317a6a5. No production file was touched.

## Decision 12 — the three disputed #1243 tests

All three disputes hold. The implementer was right: the tests asserted outputs that unchanged requirements decide otherwise.

| Test | Requirement that decides it | Old expected | New expected |
|---|---|---|---|
| `LoneCoalesceOperatorIsJoinedWithoutBlankLine` | FMT-INDENT-018: "A `??` operator MUST be indented one level deeper than that column" | `?? "x"` at column 16, under `s` | `?? "x"` at column 20 |
| `LoneOperatorBelowCommentLineIsJoinedWithoutBlankLine` → renamed `LoneOperatorBelowCommentLineIsJoinedAndCommentGetsOneBlankLine` | FMT-BLANK-011: `// between` is on its own line (FMT-BLANK-018), so it is not glued to `&&`. It is not the first content of a block, and no directive precedes it, so it gets exactly one blank line | no blank line above `// between` | one blank line above `// between` |
| `LoneConditionalColonBelowCommentLineIsJoinedWithoutBlankLine` → renamed `LoneConditionalColonBelowCommentLineIsJoinedAndCommentGetsOneBlankLine` | FMT-BLANK-011, same reasoning for `// c` above `:` | no blank line above `// c` | one blank line above `// c` |

The comment columns stay as before (FMT-INDENT-006). The two comment tests were renamed because "WithoutBlankLine" would now describe the opposite of what they assert.

**Coverage I restored.** With the blank line now expected, the two comment tests also pass on the pre-fix production code (`d0e2102` formatter and Core in a scratch worktree): the blank-line phase gives exactly one blank line either way. So for these two shapes the whole-pipeline tests no longer reproduce FMT-BREAK-030. I added two phase-level tests to `LineBreakPhaseBlankLineTests`. Each runs `LineBreakPhase` alone under LF and CRLF and asserts that the phase joined the lone operator and wrote no blank line. On the pre-fix code both fail for the right reason ("The phase inserted a blank line": `var p = a⏎⏎// between⏎&&b;`). At 317a6a5 both pass.

## Decision 13 — existing tests the changed specs decide

The implementer's handover lists fifteen tests: 4 indexer-layout, 7 + 2 collection-bracket comment column, 1 enum member, and 1 RH5601 tab test. I checked each one against the spec text and changed only the values those requirements decide. Each now carries the deciding requirement's tag.

| Test | Requirement | What changed in the expected output |
|---|---|---|
| `ParameterContinuationLineAlignmentTests.IndexerLeadingCommaStaysAligned` → `IndexerLeadingCommaMovesToTheEndOfThePreviousParameterLine` | FMT-BREAK-011 (indexer clause, FMT-PIPE-014) | `this[int first⏎, int second]` → `this[int first,⏎int second]`, `int second` under `int first` |
| `…ContinuationOfParameterAfterLeadingCommaIsAlignedWithItsFirstToken` → `ContinuationOfParameterAfterMovedLeadingCommaIsAlignedWithItsFirstToken` | FMT-BREAK-011 | The comma moves behind `int first`. `[Obsolete] // keep` and `int second` are both under `int first`, so the continuation stays under the parameter's first token |
| `…IndexerParameterOnSharedLineKeepsContinuationUnderItsFirstToken` → `IndexerParameterOnSharedLineMovesToItsOwnLineAndKeepsContinuationUnderItsFirstToken` | FMT-BREAK-012 (indexer included) | `[Obsolete] // keep` moves to its own line under `int first`, and `int second` stays under it |
| `AttributeListStructuralFinalOutputTests.LineBreakBetweenMergedListsOfBracketedListsIsJoined` | FMT-BREAK-013 (indexer clause), together with the FMT-STRUCT-029 example `P([A]⏎[B] int x, int y)` → `P([A, B] int x,⏎int y)` | `int this[[A, B] int i, int j]` → `int this[[A, B] int i,⏎int j]`. The type-parameter half (`G<[A, B] T, U>`) is unchanged |
| `LineBreakRequirementTests.EnumMemberWrapBeforeEqualsIsKeptAndWrapAfterEqualsIsJoined` → `EnumMemberWrapBeforeEqualsAndWrapAfterEqualsAreJoined` | FMT-BREAK-016 (enum member's value) | `A⏎= 1,` → `A = 1,`. The old summary called the test an untagged characterization pending this change; it now carries the tag |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBetweenCommentAndBraceOrBracket` | FMT-INDENT-006 (comment before a collection expression's `]`) | `// end of list` moves from the `]` column to the elements' column (+4) |
| `…RemovesBlankLineBetweenCommentAndEveryNamedBraceOrBracket` | FMT-INDENT-006 | `// end list` moves +4. `// end pattern` (list pattern) is unchanged: FMT-INDENT-006 names only the collection expression (follow-up 3) |
| `…RemovesBlankLineBetweenCommentBehindDirectiveAndBraceOrBracket` | FMT-INDENT-006 | `// c` in front of `];` moves +4 |
| `…RemovesBlankLineBehindLineOfTwoDelimitedComments`, `…OfDelimitedCommentAndLineComment`, `…LineCommentThatFollowsEndOfMultiLineComment`, `…LineCommentThatFollowsDelimitedDocumentationComment` | FMT-INDENT-006; FMT-INDENT-007 for the multi-line comment | Only the comment line in front of `F`'s `]` moves +4. In the shared fixture, a new placeholder form `@@+4` moves the first comment line 4 columns further and leaves every further line where it was. This is how FMT-INDENT-007 lays out `/* x⏎   y */ // b`: only the first line moves. Every other placeholder is unchanged |
| `MultiLineGluedCommentBlankLineTests.InsertsNoBlankLineAboveMultiLineCommentGluedToCollectionExpressionBracket` | FMT-INDENT-006, FMT-INDENT-007 | Used to assert unchanged input. Now `/* x` moves to the elements' column (18) and `*/ ];` keeps its column. The blank-line assertion is unchanged |
| `…RemovesAuthorBlankLineAboveMultiLineCommentGluedToCollectionExpressionBracket` | FMT-INDENT-006, FMT-INDENT-007 | `/* x` moves +4 and `*/ ];` is unchanged. The removed blank line is unchanged |
| `RH5601UseTabsCorrectlyFormatterTests.VerifyFormatterKeepsTheClosingLineTabOfAnInterpolatedRawStringAndItIsNotReported` (analyzer project) | FMT-RAW-001 as changed by Q2 (tab = one column) | Used to assert unchanged input. The closing line `16 spaces + tab` measures 17 columns, and the first `"` of `$"""` is at column 16. So both lines move left by one: `15 spaces + tab + a` and `15 spaces + tab + """`. The tab stays (FMT-RAW-002) and RH5601 still reports nothing (RH5601-002, which stays tagged). The test now uses `VerifyFormatter(source, fixedSource)` with no diagnostics |

The FMT-BLANK-020 tags of the comment tests stay: their blank-line assertions are unchanged.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `LoneOperatorJoinBlankLineTests.LoneCoalesceOperatorIsJoinedWithoutBlankLine` | FMT-BREAK-030, FMT-INDENT-018 | `AssertRuleResult` | pass; fails on pre-fix code |
| `LoneOperatorJoinBlankLineTests.LoneOperatorBelowCommentLineIsJoinedAndCommentGetsOneBlankLine` | FMT-BREAK-030, FMT-BLANK-011 | `AssertRuleResult` | pass; also passes on pre-fix code (see above) |
| `LoneOperatorJoinBlankLineTests.LoneConditionalColonBelowCommentLineIsJoinedAndCommentGetsOneBlankLine` | FMT-BREAK-030, FMT-BREAK-018, FMT-BLANK-011 | `AssertRuleResult` | pass; also passes on pre-fix code |
| `LineBreakPhaseBlankLineTests.InsertsNoBlankLineAboveCommentLineWhereItJoinsLoneBinaryOperator` (new) | FMT-BREAK-030 | `LineBreakPhase` alone, LF + CRLF | pass; fails on pre-fix code |
| `LineBreakPhaseBlankLineTests.InsertsNoBlankLineAboveCommentLineWhereItJoinsLoneConditionalColon` (new) | FMT-BREAK-030 | `LineBreakPhase` alone, LF + CRLF | pass; fails on pre-fix code |
| `ParameterContinuationLineAlignmentTests.IndexerLeadingCommaMovesToTheEndOfThePreviousParameterLine` | FMT-BREAK-011 | `AssertRuleResult` | pass; fails on pre-fix code |
| `ParameterContinuationLineAlignmentTests.ContinuationOfParameterAfterMovedLeadingCommaIsAlignedWithItsFirstToken` | FMT-BREAK-011 | `AssertRuleResult` | pass; fails on pre-fix code |
| `ParameterContinuationLineAlignmentTests.IndexerParameterOnSharedLineMovesToItsOwnLineAndKeepsContinuationUnderItsFirstToken` | FMT-BREAK-012 | `AssertRuleResult` | pass; fails on pre-fix code |
| `AttributeListStructuralFinalOutputTests.LineBreakBetweenMergedListsOfBracketedListsIsJoined` | FMT-STRUCT-029, FMT-BREAK-013 | `AssertRuleResult` | pass; fails on pre-fix code |
| `LineBreakRequirementTests.EnumMemberWrapBeforeEqualsAndWrapAfterEqualsAreJoined` | FMT-BREAK-016 | `AssertRuleResult` | pass; fails on pre-fix code |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBetweenCommentAndBraceOrBracket` | FMT-BLANK-020, FMT-INDENT-006 | `AssertRuleResult` | pass; fails on pre-fix code |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBetweenCommentAndEveryNamedBraceOrBracket` | FMT-BLANK-020, FMT-INDENT-006 | `AssertRuleResult` | pass; fails on pre-fix code |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBetweenCommentBehindDirectiveAndBraceOrBracket` | FMT-BLANK-020, FMT-INDENT-006 | `AssertRuleResult` | pass; fails on pre-fix code |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBehindLineOfTwoDelimitedComments` | FMT-BLANK-020, FMT-INDENT-006 | `AssertRuleResult` via `AssertRemovesBlankLineBehindCommentLine` | pass; fails on pre-fix code |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBehindLineOfDelimitedCommentAndLineComment` | FMT-BLANK-020, FMT-INDENT-006 | same | pass; fails on pre-fix code |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBehindLineCommentThatFollowsEndOfMultiLineComment` | FMT-BLANK-020, FMT-INDENT-006 | same | pass; fails on pre-fix code |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBehindLineCommentThatFollowsDelimitedDocumentationComment` | FMT-BLANK-020, FMT-INDENT-006 | same | pass; fails on pre-fix code |
| `MultiLineGluedCommentBlankLineTests.InsertsNoBlankLineAboveMultiLineCommentGluedToCollectionExpressionBracket` | FMT-BLANK-018, FMT-INDENT-006 | `AssertRuleResult` | pass; fails on pre-fix code |
| `MultiLineGluedCommentBlankLineTests.RemovesAuthorBlankLineAboveMultiLineCommentGluedToCollectionExpressionBracket` | FMT-BLANK-018, FMT-INDENT-006 | `AssertRuleResult` | pass; fails on pre-fix code |
| `RH5601UseTabsCorrectlyFormatterTests.VerifyFormatterKeepsTheClosingLineTabOfAnInterpolatedRawStringAndItIsNotReported` | RH5601-002, FMT-RAW-001 | `VerifyFormatter(source, fixedSource)` (LF + CRLF, second pass) | pass; fails on pre-fix code |

FMT-INDENT-007 decides the unmoved continuation lines of the multi-line comments. I left it untagged because each of those tests touches it only incidentally.

`AssertRuleResult` runs every formatter test above under LF and CRLF and checks the second pass.

## Not covered, and why
- **No test was found that a requirement does not decide.** All fifteen tests the implementer routed are decided as shown above.
- **Observation, unchanged, outside this round:** `IndexerLeadingCommaAfterSharedLineIsAlignedWithTheFirstParameter` (`this[int first, int second // keep⏎, int third]`) passes unchanged. The method form `M(int first, int second // keep⏎, int third)` gives the same output with the CLI. Indexer and method agree (FMT-PIPE-014). Whether FMT-BREAK-012 should still split `int first, int second` there, when the comment pins the comma, is the method side's question, not this run's. No test was changed for it.
- **Phase-level tests run LF and CRLF only.** No other failure class reaches a test-only correction round.

## Commands
```shell
scripts/build.sh --configuration Debug
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~LoneOperatorJoinBlankLineTests|FullyQualifiedName~LineBreakPhaseBlankLineTests|FullyQualifiedName~MultiLineGluedCommentBlankLineTests|FullyQualifiedName~CommentBeforeBraceBlankLineTests|FullyQualifiedName~LineBreakRequirementTests|FullyQualifiedName~AttributeListStructuralFinalOutputTests|FullyQualifiedName~ParameterContinuationLineAlignmentTests"
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5601"
```

Results at 317a6a5 with these tests: formatter 4692/4692, analyzer 7350/7350, architecture 115/115 passed.
