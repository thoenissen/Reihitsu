Role: test implementer
Commit: cbcaad8
Verdict: tests written

Repair round 1, for lead decisions 10 and 11 (auditor finding #1). The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the cases to cover. Expected values come from FMT-INDENT-031's examples and its normative sentences (as changed in 13-spec-author.md) and from change-doc entry 8. They do not come from code output. For the `static` and `async` members, and for the two-level nesting, I derived the columns from the anchor rule: `async` if present, otherwise `delegate`; braces under the anchor; statements one level deeper.

## Tests

All tests are new, in `src/Reihitsu.Formatter.Test/Regression/Indentation/AnonymousMethodHoleAlignmentTests.cs`, and all use `AssertRuleResult`. That helper checks the exact output under LF and CRLF, a second pass, the phase check, and the tree round-trip. "Expected now" is on cbcaad8.

"Moved" means the literal's content stands in column 4 and moves 13 columns to the right. "Aligned" means it already stands in column 17.

Every failing test fails only on the block's column. Under "Expected now", `13` means `{`, `return 1;`, and `})}…` stand in column 13 (moved literal), and `0` means they stand in column 0 (aligned literal). The output expects them under the anchor. Everything else in the output matches: the break behind `delegate`, the hole text, and the closing quotes.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| SpecExampleAnonymousMethodInStringsNestedInHoleOfMovedLiteralIsAlignedUnderDelegate | FMT-INDENT-031 | `AssertRuleResult` | fail: 13 |
| SpecExampleAnonymousMethodInStringsNestedInHoleOfAlignedLiteralIsAlignedUnderDelegate | FMT-INDENT-031 | `AssertRuleResult` | fail: 0 |
| AnonymousMethodInRegularStringNestedInHoleOfMovedLiteralIsAlignedUnderDelegate | FMT-INDENT-031; FC-016/FC-022 | `AssertRuleResult` | fail: 13 |
| AnonymousMethodInRegularStringNestedInHoleOfAlignedLiteralIsAlignedUnderDelegate | FMT-INDENT-031; FC-016/FC-022 | `AssertRuleResult` | fail: 0 |
| AnonymousMethodInVerbatimStringNestedInHoleOfMovedLiteralIsAlignedUnderDelegate | FMT-INDENT-031; FC-016/FC-022 | `AssertRuleResult` | fail: 13 |
| AnonymousMethodInVerbatimStringNestedInHoleOfAlignedLiteralIsAlignedUnderDelegate | FMT-INDENT-031; FC-016/FC-022 | `AssertRuleResult` | fail: 0 |
| AnonymousMethodInSingleLineRawStringNestedInHoleOfMovedLiteralIsAlignedUnderDelegate | FMT-INDENT-031; FC-016/FC-022 | `AssertRuleResult` | fail: 13 |
| AnonymousMethodInSingleLineRawStringNestedInHoleOfAlignedLiteralIsAlignedUnderDelegate | FMT-INDENT-031; FC-016/FC-022 | `AssertRuleResult` | fail: 0 |
| StaticAnonymousMethodInRegularStringNestedInHoleOfMovedLiteralIsAlignedUnderDelegate | FMT-INDENT-031 (answer 4); FC-003 | `AssertRuleResult` | fail: 13 |
| StaticAnonymousMethodInSingleLineRawStringNestedInHoleOfAlignedLiteralIsAlignedUnderDelegate | FMT-INDENT-031 (answer 4); FC-003 | `AssertRuleResult` | fail: 0 |
| AsyncAnonymousMethodInVerbatimStringNestedInHoleOfAlignedLiteralIsAlignedUnderAsync | FMT-INDENT-031 (answer 6); FC-003 | `AssertRuleResult` | fail: 0 |
| AsyncAnonymousMethodInSingleLineRawStringNestedInHoleOfMovedLiteralIsAlignedUnderAsync | FMT-INDENT-031 (answer 6); FC-003 | `AssertRuleResult` | fail: 13 |
| AnonymousMethodInRegularStringNestedTwoLevelsInHoleOfMovedLiteralIsAlignedUnderDelegate | FMT-INDENT-031; FC-002 (one level deeper) | `AssertRuleResult` | fail: 13 |
| BlockLambdaInRegularStringNestedInHoleOfMovedLiteralIsAlignedAtItsParenthesis | FMT-INDENT-028, FMT-INDENT-031 | `AssertRuleResult` | pass |
| BlockLambdaInRegularStringNestedInHoleOfAlignedLiteralIsAlignedAtItsParenthesis | FMT-INDENT-028, FMT-INDENT-031 | `AssertRuleResult` | pass |
| SpecExampleAnonymousMethodInSingleLineRawStringNoMultiLineHoleEnclosesKeepsScopeDepth | FMT-INDENT-031; FC-003 | `AssertRuleResult` (unchanged) | pass |
| AnonymousMethodInRegularStringNoMultiLineHoleEnclosesKeepsScopeDepth | FMT-INDENT-031; FC-003 | `AssertRuleResult` (unchanged) | pass |
| AnonymousMethodInVerbatimStringNoMultiLineHoleEnclosesKeepsScopeDepth | FMT-INDENT-031; FC-003 | `AssertRuleResult` (unchanged) | pass |
| SpecExampleAnonymousMethodInRegularStringInsideBlockLambdaBodyInHoleKeepsScopeDepth | FMT-INDENT-031; FC-003 | `AssertRuleResult` (unchanged) | pass |
| AnonymousMethodInSingleLineRawStringInsideBlockLambdaBodyInHoleKeepsScopeDepth | FMT-INDENT-031; FC-003 | `AssertRuleResult` (unchanged) | pass |
| AnonymousMethodInRegularStringInsideAnonymousMethodOfTheSameHoleTakesScopeDepthOfTheOuterBody | FMT-INDENT-031; FC-003 ("or of another anonymous method") | `AssertRuleResult` | pass |

On cbcaad8 the test class has 40 tests: 13 fail (exactly the 13 marked "fail" above) and 27 pass.

### Auditor's member list

| Member | Status |
|---|---|
| Regular, verbatim, and single-line raw string, each with an aligned and a moved literal (the six failing cells) | One test per cell, plus the spec example in both literal states |
| A nested multi-line raw literal | Already covered by `AnonymousMethodInNestedLiteralInsideBlockLambdaOfOuterHoleIsAligned`; not duplicated |
| A block lambda in the same nested position | The two new `BlockLambdaInRegularString…` tests |
| An anonymous method inside a block body | Covered by the existing tests and by the three new exception tests with a nested string |
| A single-line raw literal not nested in a multi-line one | The new spec-example test, plus the existing `AnonymousMethodInHoleOfSingleLineLiteralTakesScopeDepth` |

## Not covered, and why

- **The block-lambda counterpart in a verbatim or single-line raw nested string.** Only the regular-string member has its own test. The spec example tests include the `L` line in both literal states, but they also fail on the anonymous-method lines. Lambda alignment in holes is existing FMT-INDENT-028 behavior that this run does not change.
- **The shared-statement refactor (lead decision 11).** It has no output of its own beyond finding #1's repair. The tests above pin both sides of the shared decision:
  - nested strings that do not end the hole;
  - a nested multi-line raw literal that does end it (existing test);
  - strings that no multi-line raw hole encloses.
- **FC-020 (directive in a hole).** No such input exists.
- **FC-028 (node-level entry points).** Unchanged; see 06-test-implementer.md.
- **FC-007/FC-008.** No code fix changes.
- **Language versions below C# 11.** A multi-line hole of a regular string does not parse there. This is follow-up candidate 1, and the tests use the helper's default (latest) parse options.
- **CRLF and second-pass idempotency.** `AssertRuleResult` covers both in every test.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~AnonymousMethodHoleAlignmentTests"
```
