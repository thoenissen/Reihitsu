Role: test implementer
Commit: f8c19c5acd4c69fec0399c884e8826b6c1074c3f
Verdict: tests written

## Scope

This round has one task, set by `squad/decisions/2026-10-20-997-rh3001-shift-operators.md` and finding 1 of `34-auditor.md`: pin counterexample 1 with one untagged characterization test that asserts the exact fixed text. No tags changed and no production code changed. The lead's message gave an expected fixed text, taken from the auditor. I did not take it on trust. I checked it with `scripts/apply-fix.sh RH3001` before pinning it, and it matched.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3001NotOperatorShouldNotBeUsedAnalyzerTests.FixOfNegationAfterShiftRightInAnArgumentWritesAGenericReading` | Untagged characterization. Relevant failure classes: FC-002, FC-016, FC-027. The input lies outside RH3001-004 once `>>` is excluded as the decision states. | `Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode))`: whole document, no normalization | Passes. The fixed text is `M(G < A, H < B >> (x == false));` |

The input is the auditor's counterexample 1, laid out one member per line like the other `…WritesAGenericReading` tests. The declarations and the call are unchanged. `ApplyCodeFixAsync` parses with `LanguageVersion.Latest`, so the `>>` operator with a `bool` right operand is accepted (it needs C# 11 or later).

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Counterexample 1, multi-line layout of the test | LF | code fix (`scripts/apply-fix.sh RH3001`) | fixed, 1 action, 1 iteration | `M(G < A, H < B >> (x == false));`, as the auditor reported |
| same | CRLF | code fix (`scripts/apply-fix.sh RH3001`) | fixed, 1 action, 1 iteration | same text, no line-ending drift |
| Original text | — | csc 10.0.402, net10.0 reference pack | compiles (exit 0) | — |
| Fixed text | — | csc 10.0.402, net10.0 reference pack | CS0307 (`C.G`) plus CS0246 for `A`, `H<>` and `B` | matches the auditor's report |

**Mutation check.** I changed the expected line to `M(G < A, H < B >> (!x));`. The test then failed with `Assertion failed. Expected strings to be equal.` I restored the file before committing.

## Not covered, and why

- **Counterexamples 2 (`>>>`) and 3 (comparison chain with `>>`).** The decision asks for a test of counterexample 1 only. The other two go into #1245 through the lead.
- **The contrast inputs** (`M(G < A, B >> !x)`, `M(G < A, H < B >>> !x)`) are not tested. They are outside this task.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3001NotOperatorShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3001NotOperatorShouldNotBeUsedAnalyzerTests.FixOfNegationAfterShiftRightInAnArgumentWritesAGenericReading"
scripts/test.sh --project architecture
```

**Results on f8c19c5.** The RH3001 class passed 32/32 and the architecture tests passed 72/72. `scripts/format.sh` changed nothing in the test file (0 of 1 formatted).
