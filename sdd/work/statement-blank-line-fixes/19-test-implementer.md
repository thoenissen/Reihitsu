Role: test implementer
Commit: (this commit; parent 1b173d6, the spec author's Markdown-only commit on top of 3a16754)
Verdict: tests written

Scope: architecture decision #26 of `00-lead.md` (auditor 17, finding #4, and its hint about SHARED-BLANK-001). No production code touched. My prompt named the decision's asks (tests for both cells, SHARED-BLANK-001 pin) and no intended fix or suspected cause beyond it.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH3208 `VerifyNoCodeFixWhenStatementBehindCommentSharesLabelContinuationLineAtLabelColumn` | SHARED-AFTERBRACE-002 | `VerifyNoCodeFixIsOffered(source)` | fails until RH3208 withholds its fix in the cell |
| RH3208 `VerifyNoCodeFixWhenStatementBehindCommentSharesLabelContinuationLineDeeperThanLabel` | SHARED-AFTERBRACE-002 | `VerifyNoCodeFixIsOffered(source)` | fails until RH3208 withholds its fix in the cell |
| RH3208 `VerifyStatementBehindCommentOnLabelContinuationLineTakesColumnOfLineStartingStatement` | SHARED-AFTERBRACE-002 | `Verify(source, fixed, 1/1 iterations)` | passes (other side of the condition, FC-003) |
| RH5002 `VerifyDiagnosticForSpecExampleEmptyLineInsideDisabledText` | SHARED-BLANK-001 | analyzer `Verify(source)` | passes (`main`'s behavior, spec example) |

Files: `src/Reihitsu.Analyzer.Test/Formatting/RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests.cs`, `src/Reihitsu.Analyzer.Test/Formatting/RH5002IfStatementsShouldBePrecededByABlankLineAnalyzerTests.cs`.

Input of the two refusal tests (inside an object initializer's lambda; the section's only statements stand on the label's continuation line, and the next statement stands behind a multi-line comment, as in #1625):

```csharp
case 2 when
Check(value): int M() {|RH3208:=> 1|}; /* a
b */ M(); return;
```

The first test puts the continuation line (and the comment's last line) in the column of `case`, auditor 17's counterexample; the second puts them one level deeper. `VerifyNoCodeFixIsOffered` first runs `Verify(source)` (exactly the marked RH3208 diagnostic), then asserts that the provider registers no action for it.

Fail for the right reason on head 3a16754: the diagnostic check passes, then `Assert.IsEmpty(actions)` fails with "No code fix may be offered for the diagnostic at [223..227)" (label column) and "[227..231)" (deeper).

The counterpart test is the same section with a `break;` that starts its own line at column 20: the fix is offered and `M();` lands in `break;`'s column with one blank line in front and no trailing whitespace behind `b */`, as SHARED-AFTERBRACE-002 states. The layout of the converted body (`{ return 1; }` anchored under `int`) is not stated by SHARED-AFTERBRACE-002; it is head's output from the existing body conversion, which this run does not change (confirmed LF and CRLF with `scripts/apply-fix.sh RH3208`). If the implementer's change moves it, that is a finding to raise, not a test to adjust.

## Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| #1625 cell, continuation at label column | LF | RH3208 code fix | reproduced | fix offered (moves `M();` to column 16 per auditor 17) vs no fix offered |
| #1625 cell, continuation deeper | LF | RH3208 code fix | reproduced | fix offered vs no fix offered |
| Same with a line-starting `break;` | LF, CRLF | RH3208 code fix (`apply-fix.sh`) | holds | `M();` at column 20, as specified |

## Not covered, and why
- CRLF variant of the refusal tests: the decision is whether an action is registered, taken on syntax; no text is produced. The counterpart was checked under CRLF with `apply-fix.sh`; the existing RH3208 CRLF test of SHARED-AFTERBRACE-002 stays.
- Fix All / convergence on the refusal tests: no action is offered; `VerifyNoCodeFixIsOffered` checks every reported diagnostic.
- RH5030 caller: cannot reach the rebuild path (`CanRegisterCodeFix`, 11-auditor / 17-auditor); no test.
- Pre-existing block-branch case from auditor 17's hint (`case 2 when⏎Check(value): { … }`): recorded follow-up candidate, not part of decision #26.
- RH5002 disabled-text example code-fix output: the spec example states only that RH5002 reports; the fix's placement around the directive is decision #18's follow-up, so it is not pinned.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests.VerifyNoCodeFixWhenStatementBehindCommentSharesLabelContinuationLine|FullyQualifiedName~RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests.VerifyStatementBehindCommentOnLabelContinuationLineTakesColumnOfLineStartingStatement|FullyQualifiedName~RH5002IfStatementsShouldBePrecededByABlankLineAnalyzerTests.VerifyDiagnosticForSpecExampleEmptyLineInsideDisabledText"
```
