Role: test implementer
Commit: (this commit, on e8fb246; `src` unchanged since ab7af13)
Verdict: tests written

Repair round 3, for decision 23 / answer 2 and decision 24 / auditor finding #7 (`00-lead.md`, `22-auditor.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

All 8 tests are new and sit in `src/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs`, right before `#endregion // Tests`. Each is tagged `RH2001-011` only. No existing test changed. `rg` for `(Value, input)`, `((Value`, `((int, int))`, `? (Value`, and `Sample<T> other` found no existing coverage. Inputs are the auditor's fixtures (`scratchpad/fx22`, `fx22b`, `fx22c`) with `internal` modifiers dropped to match the class's style.

"`main`" is 4838e9a (`origin/main`). I ran the new tests against `main`'s production code in a temporary worktree (since removed): output `/tmp/claude-0/-home-user-Reihitsu/4271dfb1-87ee-51f4-9b4e-ea7505ebffbd/scratchpad/r24-main.txt`. Head output: `/tmp/claude-0/-home-user-Reihitsu/4271dfb1-87ee-51f4-9b4e-ea7505ebffbd/scratchpad/r24-head.txt`.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyNoFixIsOfferedWhenThePropertyIsAnElementOfATupleASwitchExpressionMatches` (a01) | RH2001-011 | `VerifyNoCodeFixIsOffered` | green on head; red on `main` (action registered) |
| `VerifyNoFixIsOfferedWhenThePropertyIsAnElementOfATupleASwitchStatementMatches` (a02) | RH2001-011 | `VerifyNoCodeFixIsOffered` | green on head; red on `main` |
| `VerifyNoFixIsOfferedWhenThePropertyIsAnElementOfATupleNestedInAnotherTuple` (a07) | RH2001-011; FC-016 | `VerifyNoCodeFixIsOffered` | green on head; red on `main` |
| `VerifyNoFixIsOfferedWhenThePropertyIsAnElementOfATupleThatIsTheOperandOfACast` (a15) | RH2001-011; FC-016 | `VerifyNoCodeFixIsOffered` | green on head; red on `main` |
| `VerifyNoFixIsOfferedWhenThePropertyIsAnElementOfATupleThatIsABranchOfAConditionalExpression` (b02) | RH2001-011; FC-016 | `VerifyNoCodeFixIsOffered` | green on head; red on `main` |
| `VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleNestedInADeconstructionTarget` (a18) | RH2001-011; FC-003 | `Verify` | green on head; green on `main` (exact output `((_value, other), x) = source;`, `return _value + other + x;`) |
| `VerifyFixIsOfferedWhenThePropertyIsReadThroughTheOwnConstructionOfItsGenericType` (c01) | RH2001-011; FC-003 | `Verify` | green on head; green on `main` (exact output `return other._value + _value;`) |
| `VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleThatIsABranchOfAConditionalExpressionWithDifferentlyNamedBranches` (a20 shape, coordinator addition; RH2001-011 ¶3 example `return flag ? (Value, 1) : (2, 3);` returning `(int, int)`) | RH2001-011; FC-003 | `Verify` | green on head; green on `main` (exact output `return flag ? (_value, 1) : (2, 3);`) |

Head: 8/8 green. `main`: 3 passed (a18, c01, the different-names conditional, so the asserted offered outputs are `main`'s exact output, matching the auditor's `fx22-main.txt` for a18), 5 failed with `Assert.IsEmpty(actions)` — `main` offers the fix, the head withholds it as answer 2 and decision 23 demand. No test failed on the head; no implementer work is needed.

The coordinator's addition: the head offers the different-names conditional, so the test was added; it is the offered side of the b02 boundary (FC-003). The spec's tuple-switch examples (RH2001.md, the example after "the governing tuple of a `switch` expression and of a `switch` statement") match the a01/a02 test inputs verbatim.

## Not covered, and why

- **Other Group A/B members (a03–a05, a08, a16, b03).** Decision 23 asked for one each of nested, cast, and conditional; a08 (nested in a deconstruction source) and b03 (switch arm) take the same withholding path as a07 and b02 under the restated rule, and a03–a05/a16 are rare natural-type or named-target members of the same refused class.
- **CRLF variants.** Binding check, no layout; the auditor's apply-fix runs gave identical LF and CRLF results for every fixture.
- **Fix All.** The guard is per diagnostic; t1 already runs the batch path.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~ASwitchExpressionMatches|FullyQualifiedName~ASwitchStatementMatches|FullyQualifiedName~NestedInAnotherTuple|FullyQualifiedName~OperandOfACast|FullyQualifiedName~BranchOfAConditionalExpression|FullyQualifiedName~NestedInADeconstructionTarget|FullyQualifiedName~OwnConstructionOfItsGenericType"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests"
```
