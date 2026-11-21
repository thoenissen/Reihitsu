Role: test implementer
Commit: 67c71cf
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing ignored.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5201MethodChainsShouldBeAlignedAnalyzerTests.VerifyTitleNamesMemberAccessChain` | — (wording; change doc change 1) | descriptor `Title` | fails: title is "Method chains should be aligned." |
| `RH5201MethodChainsShouldBeAlignedAnalyzerTests.VerifyMisalignedLinkOfCallLessChainIsReportedWithMemberAccessChainMessage` | — (wording; change doc change 1, its example) | `Verify(source, fixedSource, Diagnostics(id, literal message))` | fails only on the message: "Member-access chains should be aligned." expected, "Method chains should be aligned." reported. With the old message the same test passes, so location and fix output (`.City` under `.Address`) are unchanged |
| `RH5201MethodChainsShouldBeAlignedAnalyzerTests.VerifyCodeFixTitleNamesMemberAccessChain` | — (wording; change doc change 2) | `GetCodeFixActionsAsync` | fails: one action, titled "Align method chain" |
| `RH5201CommentExemptChainWithNonInvokedPrefixDotTests.VerifyFormatterFixesCommentExemptChainWithNonInvokedPrefixAndStaysIdempotent` (changed: expected message) | — (pre-existing; untagged by the change) | `VerifyFormatter` | fails only on the message |

No test carries a requirement tag: the change doc changes no requirement, and the wording is no requirement's statement. The message is asserted as a literal, not through `MessageFormat`, as `02-skeptic.md` asks, so a wording regression fails.

Existing tests asserting a changed output (`rg -i "method chains? should be aligned|align method chain" src --glob '*.cs'`): only `RH5201CommentExemptChainWithNonInvokedPrefixDotTests.cs:159` (changed) and the analyzer's `///` summary (production, the implementer's). `AnalyzerPackageMetadataTests` (`AnalyzerTitleResourcesMatchRuleDocumentationTitles`, `MessageFormatResourcesMatchTitleWhenNoPlaceholders`, `PackageReadmeDescriptionsMatchRuleDocumentationTitles`) compare resources and README against the rule page heading, which already says the new title; they fail until the implementer changes the resources and the README row, and are left as they are.

## Scenarios (bug fix)

_N/A — behavior change._

## Not covered, and why

- Failure classes: none reaches a text-only change; no reporting condition, location, guard, or rewrite changes (`02-skeptic.md`, *Breaking inputs*). Both sides of a condition (FC-003): no condition is added, removed, or moved.
- CRLF variant and Fix All: the layout and the Fix All path are unchanged and already covered by the existing suite (`GetFixAllScenario`, the `endOfLine` data rows); the new tests pin only the text, which does not depend on line endings.
- Rule page body (change 4): pinned by no test, as `03-spec-author.md` states; documentation, not output a test can compare beyond the heading the metadata tests already pin.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5201MethodChainsShouldBeAlignedAnalyzerTests.VerifyTitleNamesMemberAccessChain|FullyQualifiedName~RH5201MethodChainsShouldBeAlignedAnalyzerTests.VerifyMisalignedLinkOfCallLessChainIsReportedWithMemberAccessChainMessage|FullyQualifiedName~RH5201MethodChainsShouldBeAlignedAnalyzerTests.VerifyCodeFixTitleNamesMemberAccessChain|FullyQualifiedName~RH5201CommentExemptChainWithNonInvokedPrefixDotTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~AnalyzerPackageMetadataTests"
```

## Follow-up candidates

_None._
