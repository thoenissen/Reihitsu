Role: spec checker
Commit: e402e1d
Verdict: consistent

Re-check of the routing of the four findings in `24-spec-checker.md`: commits 44f1249 (spec author, `25-spec-author.md`), d035585 (test implementer, `26-test-implementer.md`), e402e1d (cleaner, `27-cleaner.md`). Only the changes since 6d5d906 were checked. The prompt carried the task only.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| — | — | — | None. | — |

## Checked
Finding 1 (RH3001-004 guard too narrow): resolved.
- The requirement now says "a negation that covers less text than the syntax node directly around it, and that is not the right operand of a `>` operator". The list of constructs is gone.
- It matches the code. The diagnostic is on the `!` token, and `RegisterCodeFixesAsync` calls `root.FindNode` without `getInnermostNodeForTie`, so it climbs through every ancestor of the same full span and registers a fix only when the node it ends on is the `PrefixUnaryExpressionSyntax`. Wrappers made of the negation alone (argument, tuple element, collection element, constant pattern) lose the fix.
- The earlier probes still sort correctly under the new wording: no fix for `[!x, y]`, `x is !true`, `x switch { !true => 1, _ => 2 }`; fixed for `new bool[] { !x }`, `[y, !x && y]`, `x switch { true => !y, _ => y }`, `case !true:`, `$"{!x}"`.
- The filed Q2 of thoenissen/Reihitsu#1245 is covered by its new comment "Q2, continued", which lists the three new inputs, the contrast inputs, and the competing readings, and states the same condition.
- No open question or excluded case is named in the requirement.

Finding 2 (RH3103-003 names an excluded case): resolved. The last sentence is removed. The requirement still holds for `S::Nullable<int>`: the analyzer reports the generic name alone, which the first clause states. The alias test keeps RH3103-001/-004.

Finding 3 (Q2 inputs not pinned): resolved. Six untagged `VerifyNoCodeFixIsOffered` tests were added in the RH3001 test class: object creation argument, element access argument, attribute argument, collection element, `is` pattern, switch-arm pattern. With the two earlier pins, all five Q2 inputs and the three continued inputs are pinned.

Finding 4 (RH3002-001 positions): resolved. Two tests tagged RH3002-001 and RH3002-007 now cover the expression body of a local function and the left side of an assignment. With the earlier three, every position RH3002-001 lists has a tagged test.

New in this round:
- The only `[Requirement]` lines added in `src` are the four on the two RH3002 tests. The RH3001 pins are untagged.
- No production file changed: `git diff 6d5d906..HEAD --name-only` outside `sdd/` and `squad/` lists only the two test files `RH3001…Tests.cs` and `RH3002…Tests.cs`.
- No ID, rule page, or code owner changed. `sdd/work/` holds only `clarity-specs/`. The working tree was clean.
- Nothing new introduced.

Tests:
- `scripts/test.sh --project architecture`: Passed! Failed: 0, Passed: 72, Skipped: 0, Total: 72.
- The RH3001, RH3002, and RH3103 analyzer test classes: Passed 299, Failed 0.
