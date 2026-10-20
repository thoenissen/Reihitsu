Role: test implementer
Commit: dfc3318c20bdc743835b5bf81d48ea237490a4c4
Verdict: tests written

## Scope

This round has a single task, from `squad/decisions/2026-10-20-997-rh3001-targeted-fix.md` and finding 1 of `30-auditor.md`: pin the `switch` input with one untagged characterization test that asserts the exact fixed text. No tags changed. No production code changed. The lead's message named an expected fixed text. It was not taken on trust: the text was checked with `scripts/apply-fix.sh RH3001` before it was pinned, and it matched.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3001NotOperatorShouldNotBeUsedAnalyzerTests.FixOfNegationStartingASwitchAfterGreaterThanInAnArgumentWritesAGenericReading` | untagged characterization; FC-027, outside RH3001-004 as restated in b03db37 (`!` is the next token after a `>` operator) | `Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode))`, whole document, no normalization | pass; the fixed text is `M(G < A, B > (x == false) switch { true => 1, false => 0 });` |

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `M(G < A, B > !x switch { true => 1, false => 0 });` (fields `int G, A, B`) | LF | code fix (`scripts/apply-fix.sh RH3001`) | fixed, 1 action, 1 iteration | `M(G < A, B > (x == false) switch { true => 1, false => 0 });`, as the auditor reported |
| same | CRLF | code fix (`scripts/apply-fix.sh RH3001`) | fixed, 1 action, 1 iteration | same text |

Mutation check: changing the expected line to `(!x) switch` makes the test fail with `Assertion failed. Expected strings to be equal.` The file was restored afterwards.

## Not covered, and why

- **The user-defined `operator +(bool, T)` instance from `30-auditor.md`** (`M(G < A, B > !x + t);`). The decision asks for the `switch` input only, and the lead files both inputs in #1245 Q1.
- **The "similar shapes" hint from the auditor** (left operand of a user-defined `*`, `<<`, `..`). Not probed. It is outside this task.
- **Compile check of the fixed text.** Not repeated here. The auditor recorded CS0307/CS0246 from csc.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3001NotOperatorShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3001NotOperatorShouldNotBeUsedAnalyzerTests.FixOfNegationStartingASwitchAfterGreaterThanInAnArgumentWritesAGenericReading"
scripts/test.sh --project architecture
```

Results on dfc3318: the RH3001 class passed 31/31 and architecture passed 72/72. `scripts/format.sh` changed nothing in the test file (0 of 1 formatted).
