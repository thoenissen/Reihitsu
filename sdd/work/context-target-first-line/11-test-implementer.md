Role: test implementer
Commit: eeb13da (tests written on it; this handover's commit follows)
Verdict: tests written

Repair round 1, auditor finding #1 (`10-auditor.md`, decision 10 of `00-lead.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the finding itself; I did not use the finding's suggested change. Expected values are `main`'s output (`origin/main` = `21c2ff4`), which FMT-PIPE-017 ("shifted by the node's offset"; a target that starts the context's first line gets the context's layout), SHARED-CHAIN-009 (the fix changes only the chain when the chain alone gets the context's layout), and the change doc's *Unchanged* line require. Every input is one the RH5201 or RH5112 fix reaches: `FluentChainCodeFixHelper.FormatChainAsync` passes the outermost chain as target and the expression statement that starts its line as context. The formatter tests call `FormatNodeInDocumentWithContextAsync` with exactly that pair.

Each test was run on a temporary worktree of `origin/main` (`21c2ff4`, test files copied in): all 10 pass. On head `eeb13da`: 8 fail, 2 (the at-depth side) pass. The fixtures were also run through `scripts/apply-fix.sh` on both commits under LF and CRLF, and the expected values match `main`'s fix output exactly.

## Tests

Files:
- `src/Reihitsu.Analyzer.Test/Formatting/RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs` (new constants `ChainOfStatementOffItsDepthTestData`/`…ResultData`)
- `src/Reihitsu.Analyzer.Test/Formatting/RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests.cs` (new `Constants` region with `ChainOfStatementInTwoSpaceIndentedFileTestData`/`…ResultData`)
- `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterNodeColumnTests.cs`

Every analyzer input has two blank lines above the statement (one of which whole-statement formatting removes), an anonymous-method argument whose block the depth places, and ` ;` (which whole-statement formatting joins).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5201 `VerifyFixOfChainOfStatementOffItsDepthChangesOnlyTheChain` (auditor e1: statement at column 10, depth 8) | FMT-PIPE-017, SHARED-CHAIN-009; FC-001, FC-032 | Verify, LF | fail: a blank line removed and `.C() ;` becomes `.C();` |
| RH5201 `VerifyFixOfChainOfStatementOffItsDepthChangesOnlyTheChainWithCarriageReturnLineFeed` | FMT-PIPE-017, SHARED-CHAIN-009; FC-010 | Verify, CRLF | fail: same |
| RH5201 `VerifyFixOfChainOfStatementInTwoSpaceIndentedFileChangesOnlyTheChain` (auditor e2) | FMT-PIPE-017, SHARED-CHAIN-009; FC-032 | Verify, LF | fail: same |
| RH5201 `VerifyFixOfChainOfStatementAtItsDepthChangesOnlyTheChain` (auditor e3, other side) | FMT-PIPE-017, SHARED-CHAIN-009; FC-003 | Verify, LF | pass |
| RH5112 `VerifyFixOfChainOfStatementInTwoSpaceIndentedFileChangesOnlyTheChain` (auditor g1) | FMT-PIPE-017, SHARED-CHAIN-009; FC-032 | Verify, LF | fail: a blank line removed and `}) ;` becomes `});` |
| RH5112 `VerifyFixOfChainOfStatementInTwoSpaceIndentedFileChangesOnlyTheChainWithCrLf` | FMT-PIPE-017, SHARED-CHAIN-009; FC-010 | Verify, CRLF | fail: same |
| RH5112 `VerifyFixOfChainOfStatementOffItsDepthChangesOnlyTheChain` (statement at column 10, depth 8; my addition, the RH5112 analogue of e1) | FMT-PIPE-017, SHARED-CHAIN-009; FC-032 | Verify, LF | fail: same |
| Formatter `ChainTargetStartingItsStatementOffItsDepthShiftsTheBlockByTheContextsOffset` | FMT-PIPE-017; FC-001 | AssertFormatsTargetWithContext (LF, CRLF, second pass) | fail: block `{`/`M(b);` at columns 8/12, want 10/14 |
| Formatter `ChainTargetStartingItsStatementInTwoSpaceIndentedFileShiftsTheBlockByTheContextsOffset` | FMT-PIPE-017 | same | fail: block at the depth columns, want 4/8 |
| Formatter `ChainTargetStartingItsStatementAtItsDepthPlacesTheBlockByTheDepth` (other side, offset zero) | FMT-PIPE-017; FC-003 | same | pass |

The analyzer failures are all in "Iterative code fix application" on exactly the lines named (observed diff on RH5201 e1: `-` blank line, `-           .C() ;`, `+           .C();`); the chain's own lines match. The formatter failure on e1 is the block at `        {` / `            M(b);` instead of `          {` / `              M(b);`.

## Not covered, and why

- Holding members the auditor listed (tab-indented file f1, lambda block d2/d4, switch expression d5, query f2, comment above/behind f3): identical to `main` on head per the auditor; the existing RH5201/RH5112 tests cover chains whose lines are all anchored. I added only the at-depth side (e3) as the FC-003 counterpart, at both surfaces.
- Fix All: the new inputs carry one diagnostic each; the verifier still runs the Fix All path on them, and the existing `GetFixAllScenario` covers several diagnostics.
- Comment or directive in a token gap: the change is about the root offset, not a gap; no rule here touches a gap.
- RH5111 and RH3208: the auditor showed they never pass a target whose first token is the context's first token, so they cannot reach this.
- RH5112 off-depth has no CRLF test of its own: the RH5112 2-space fixture has one, and `apply-fix.sh` gave identical LF and CRLF results for both fixtures on both commits. The formatter tests run both endings.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~ChainOfStatement"
scripts/test.sh --project formatter --filter "FullyQualifiedName~ChainTargetStartingItsStatement"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5112|FullyQualifiedName~RH5201"
```
