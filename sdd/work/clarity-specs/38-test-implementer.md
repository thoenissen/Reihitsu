Role: test implementer
Commit: 5fb61fb38a93734a29403a24ac6fb89da18595aa
Verdict: tests written

## Scope

The lead asked for one more untagged characterization test. It covers the `>>>=` input from `35-spec-author.md` (*Open questions to file*), which the lead says is filed as Q3 in thoenissen/Reihitsu#1245. No tags changed and no production code changed. The lead's message gave an expected fixed text. I did not take it on trust: I built my own minimal input and checked its output with `scripts/apply-fix.sh RH3001` before pinning it.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3001NotOperatorShouldNotBeUsedAnalyzerTests.FixOfNegationAfterUnsignedRightShiftAssignmentAddsParentheses` | Untagged characterization. The input lies inside RH3001-004 (the `!` follows `>>>=`), but the output contradicts that requirement's "otherwise it MUST NOT add any" parentheses, so the test is left untagged. | `Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode))`: whole document, no normalization | Passes. The fixed text is `B >>>= (x == false);` |

The input has these declarations:

- `class T { public static T operator >>>(T a, bool b) => a; }`
- `class C { T B; void Run(bool x) { B >>>= !x; } }`

Each member sits on its own line. `ApplyCodeFixAsync` parses with `LanguageVersion.Latest`, which accepts the `>>>` operator (it needs C# 11 or later).

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `B >>>= !x;`, multi-line layout of the test | LF | code fix (`scripts/apply-fix.sh RH3001`) | fixed, 1 action, 1 iteration | `B >>>= (x == false);`, the same text the lead and `35-spec-author.md` gave |
| same | CRLF | code fix (`scripts/apply-fix.sh RH3001`) | fixed, 1 action, 1 iteration | same text, no line-ending drift |
| Original text | — | csc 10.0.402, net10.0 reference pack | compiles (exit 0) | — |
| Fixed text | — | csc 10.0.402, net10.0 reference pack | compiles (exit 0) | — |

**Mutation check.** I changed the expected line to `B >>>= x == false;`. The test then failed with `Assertion failed. Expected strings to be equal.` I restored the file before committing.

## Not covered, and why

- **A built-in `>>>=` (for example on `int`).** It is not an input: `!x` is `bool`, and no built-in `>>>` takes a `bool` right operand.
- **The `>>=` contrast.** `35-spec-author.md` reports that `>>=` gets no parentheses. That case is outside this task and is not tested here.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3001NotOperatorShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3001NotOperatorShouldNotBeUsedAnalyzerTests.FixOfNegationAfterUnsignedRightShiftAssignmentAddsParentheses"
scripts/test.sh --project architecture
```

**Results on 5fb61fb.** The RH3001 class passed 33/33 and the architecture tests passed 72/72. `scripts/format.sh` changed nothing in the test file (0 of 1 formatted).
