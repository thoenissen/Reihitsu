Role: test implementer
Commit: (this commit; parent 2f6a269)
Verdict: tests written

Scope: repair round 1, decision #23 of `00-lead.md` (auditor finding #1). No production code touched. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decision #23.

## Tests
Both in `src/Reihitsu.Analyzer.Test/Formatting/RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyNoCodeFixWhenOnlyStatementsOfSectionShareLabelContinuationLineDeeperThanLabel` | SHARED-STMT-004 | `VerifyNoCodeFixIsOffered(source)` | fails until the fix is withheld |
| `VerifyNoCodeFixWhenOnlyStatementsOfSectionShareLabelContinuationLineAtLabelColumn` | SHARED-STMT-004 | `VerifyNoCodeFixIsOffered(source)` | fails until the fix is withheld |

Input of both (inside an object initializer's lambda, a `switch` section whose only statements all stand on the label's continuation line):

```csharp
case 1 when
    Check(value): Method(1); {|RH5029:var|} y = 1; return;
```

The first test puts the continuation one level (4 columns) deeper than `case`; the second puts it in the column of `case`, the auditor's counterexample. "One level deeper" is the cell where the current fallback gives a plausible column by coincidence, so the test pins the withholding decision rather than the column. `VerifyNoCodeFixIsOffered` first runs `Verify(source)` (the diagnostic on `var` is reported, and nothing else), then asserts that the provider registers no action for it.

Fails for the right reason, on head 2f6a269: `Verify` passes (diagnostic reported as marked), then `Assert.IsEmpty(actions)` fails with "No code fix may be offered for the diagnostic at [226..229)" (at the label's column) and "[230..233)" (deeper).

## Scenarios (bug fix)
_None new_; finding #1's counterexample is the second test's input.

## Not covered, and why
- CRLF variant: the decision is to register or withhold an action, taken on syntax (whether the anchor line is a label continuation and whether another statement of the section starts its own line); no text is produced, so the line ending cannot change it. The existing CRLF test of the present-statement cell stays.
- Fix All / convergence: no action is offered, so there is nothing to converge; `VerifyNoCodeFixIsOffered` checks every reported diagnostic.
- The other side of the new condition (a statement of the section that starts its own line is present) is already covered by the ten existing `VerifyMovedStatementTakesSectionColumn…`/`…KeepsSectionColumn…` tests, which must keep their fix.
- Other SHARED-STMT-004 rules (RH5002 etc.): they share `StatementShouldBePrecededByABlankLineCodeFixProviderBase` → `BlankLineCodeFixUtilities`; decision #23 asks for the two cells, covered on RH5029. A guard placed in a per-rule override instead of the shared utility would not be covered for the other rules.
- Formatter parity: decision #23 changes the code fix only; the formatter path is untouched.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests.VerifyNoCodeFixWhenOnlyStatementsOfSectionShareLabelContinuationLine"
```
