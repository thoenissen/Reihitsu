Role: test implementer
Commit: 5d5cd34 (base; this handover is committed with the removal)
Verdict: tests written

Repair after maintainer answer 3 (`00-lead.md`, *Maintainer answers*): "A" — remove the new test class entirely and keep only the one-line update of the existing message test. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond that answer; nothing ignored.

## Change

- Deleted `src/Reihitsu.Analyzer.Test/Formatting/RH5201MemberAccessChainWordingTests.cs` (the three wording tests moved there in `11-test-implementer.md` and its Fix All scenario). No other file referenced the class.
- Kept: `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5201CommentExemptChainWithNonInvokedPrefixDotTests.cs`, whose expected message changes from "Method chains should be aligned." to "Member-access chains should be aligned." — the only test change of the run against the merge base with `origin/main`.
- `RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs` stays at its content on `main` (as left by `11-test-implementer.md`).

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5201CommentExemptChainWithNonInvokedPrefixDotTests` (expected message updated) | — | `FormatterTestsBase<TAnalyzer>.VerifyFormatter(source, fixedSource, Diagnostics(id, message, 3))` | passes |

Title and message remain pinned by the architecture tests (title vs rule page heading and README row, message vs title), per the maintainer's reason.

## Scenarios (bug fix)

_N/A — behavior change._

## Not covered, and why

The code-fix title "Align member-access chain" has no test after this removal, by the maintainer's decision: it is a resource string visible in the diff.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5201"
scripts/test.sh --project architecture
```

The first: 142 passed, 0 failed. The second: 198 passed, 0 failed.

## Follow-up candidates

_None._
