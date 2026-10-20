Role: test implementer
Commit: f16d7a0edb22864e696c4f0e840a61d4b3d72125
Verdict: tests written

## Scope

The lead sent two requests while this handover was being written, and each asked for one untagged characterization test:

- the `>>>=` input from `35-spec-author.md`, filed as Q3 in thoenissen/Reihitsu#1245;
- the `??=` input, filed as "Q3, continued" in thoenissen/Reihitsu#1245.

No tags changed and no production code changed. Each message gave an expected fixed text. I did not take either on trust: I built a minimal input for each and checked it with `scripts/apply-fix.sh RH3001` before pinning it.

**The spec changed during this round.** The spec author committed 47b1fad in parallel. Since then, RH3001-004 exempts "the right operand of an assignment operator other than `=`" from "MUST NOT add any" parentheses. Both inputs are such operands, so neither output contradicts the requirement as it now reads. Both tests stay untagged, as the lead asked. Each pins one concrete output, which the requirement now leaves open.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3001NotOperatorShouldNotBeUsedAnalyzerTests.FixOfNegationAfterUnsignedRightShiftAssignmentAddsParentheses` | untagged characterization | `Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode))`: whole document, no normalization | Passes. The fixed text is `B >>>= (x == false);` |
| `RH3001NotOperatorShouldNotBeUsedAnalyzerTests.FixOfNegationAfterNullCoalescingAssignmentAddsParentheses` | untagged characterization | same | Passes. The fixed text is `B ??= (x == false);` |

The inputs, each laid out one member per line:

- **`>>>=`:** `class T { public static T operator >>>(T a, bool b) => a; } class C { T B; void Run(bool x) { B >>>= !x; } }`. `ApplyCodeFixAsync` parses with `LanguageVersion.Latest`, which accepts the `>>>` operator (it needs C# 11 or later).
- **`??=`:** `class C { bool? B; void Run(bool x) { B ??= !x; } }`. This is the lead's input unchanged.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `B >>>= !x;` | LF and CRLF | code fix (`scripts/apply-fix.sh RH3001`) | fixed, 1 action, 1 iteration, no line-ending drift | `B >>>= (x == false);`, the text the lead gave |
| `B ??= !x;` | LF and CRLF | code fix (`scripts/apply-fix.sh RH3001`) | fixed, 1 action, 1 iteration, no line-ending drift | `B ??= (x == false);`, the text the lead gave |
| Both originals and both fixed texts | — | csc 10.0.402, net10.0 reference pack | all four compile (exit 0) | — |

**Mutation checks.** I changed the expected line to `B >>>= x == false;` in one test and to `B ??= x == false;` in the other. Each test then failed with `Assertion failed. Expected strings to be equal.` I restored the file each time before committing.

## Not covered, and why

- **A built-in `>>>=` (for example on `int`).** It is not an input: `!x` is `bool`, and no built-in `>>>` takes a `bool` right operand.
- **The other compound assignments** (`>>=`, `&=`, `|=`, and so on). They are outside this task. `35-spec-author.md` reports that `>>=` gets no parentheses.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3001NotOperatorShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3001NotOperatorShouldNotBeUsedAnalyzerTests.FixOfNegationAfterUnsignedRightShiftAssignmentAddsParentheses"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3001NotOperatorShouldNotBeUsedAnalyzerTests.FixOfNegationAfterNullCoalescingAssignmentAddsParentheses"
scripts/test.sh --project architecture
```

**Results on f16d7a0.** The RH3001 class passed 34/34 and the architecture tests passed 72/72. `scripts/format.sh` changed nothing in the test file (0 of 1 formatted, on each run).
