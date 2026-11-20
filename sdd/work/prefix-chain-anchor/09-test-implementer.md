Role: test implementer
Commit: 6691770 (tests written on this head; the commit carrying them follows it)
Verdict: tests written

Repair round 1, auditor finding #1 (`08-auditor.md`): no test had more than two links in front of the first wrapped link, so no test could tell the last link from the second. Issue thoenissen/Reihitsu#896, lane behavior change. My prompt carried no intended fix, suspected cause, or hoped-for verdict. The auditor's suggested change was used as a hint only. Every expected value is counted by hand from SHARED-CHAIN-007 (the anchor is the operator of the last link in front of the first link that starts a line, when no invoked link comes before it) and from RH5201-002/-005 and RH5111-006. Every unchanged line is the existing tests' input layout.

## Tests

"Expected now" means on the head with the run's shipped `GetAnchorLink`. "Second-link mutant" is a throw-away worktree under the scratchpad (since removed). Its `GetAnchorLink` stops updating the last link in front once it holds the second link, which is the implementation the auditor says every earlier test accepts. The checked-out production code was never touched.

| Test | Requirement / failure class | Helper | Expected now | Second-link mutant |
|---|---|---|---|---|
| Core `FluentChainTests.GetAnchorLinkReturnsLastLinkInFrontOfFirstWrappedLinkWithoutInvokedLink`, 8 new rows: `x.Next.Next.Next⏎.Self()` → 2, `x.Next.Next.Next.Next⏎.Next` → 3, `order.Customer.Address.Street⏎.Name` → 2, `x.Next?.Next.Next⏎.Self()` → 2, `a.B.D?.D.D⏎.E` → 3, `x.A.B.C⏎.D.E⏎.F` → 2 (a later line with two links does not count), `x.Next.Next.Next /* c⏎*/ .Next⏎.Self()` → 3 (the comment starts behind code), `x.Next.Next⏎.Next.Next⏎.Self()` → 1 (the far side: two in front stays index 1). The summary was extended | SHARED-CHAIN-007; FC-017, FC-003 | `FluentChain.Create` + `GetAnchorLink` | pass | the 7 rows with index 2 or 3 fail |
| Formatter `PrefixChainAnchorTests.ThreeOrMoreLinksInFrontOfTheFirstWrappedLinkAlignUnderTheLastOfThem` (new): three and four links in front, with and without a call, behind `?.`, `order.Customer.Address.Street`, and the root `x` ⏎ `.Next.Next.Next` whose first link joins the root's line | SHARED-CHAIN-007; FC-017 | `AssertRuleResult` (LF, CRLF, 2nd pass, phase stability) | pass | fail: every wrapped link lands under the second link |
| Analyzer `RH5201MethodChainsShouldBeAlignedAnalyzerTests.VerifyLinkWrappedBehindThreeOrMoreLinksIsReportedUnlessUnderTheLastOfThem` (new). Reported: under the second link (three links), under the first link, and under the third of four links. Silent: under the last of three, under the last of four, behind `?.`. Fix All moves all three under the last link | RH5201-002, RH5201-005, SHARED-CHAIN-007; FC-017, FC-007, FC-008 | `Verify(source, fixed, diagnostics)` (batch base) | pass | fail: 5 diagnostics instead of 3 |
| Analyzer `RH5111AssignmentsMustHaveProperLineBreaksConditionalAccessTests.VerifyFixAlignsAWrappedMemberBehindFourLinksAroundAConditionalAccessUnderTheLastOfThem` (new): `a.B.B?.B.B` ⏎ `.C =` ⏎ `5;`, with `.C` written under the second link and fixed under the last `.B` | RH5111-006; FC-017, FC-010 | `VerifyUnderBothLineEndings` | pass | fail: `.C` stays under the second link |

Against the mutant, exactly these new tests failed. Every test that existed before this round passed there too, which confirms the gap: Core 57/64, formatter `PrefixChainAnchorTests` and `ChainLinkLayoutTests` 99/100, analyzer RH5201/RH5112/RH5111 382/384. I read the formatter, RH5201, and RH5111 failure messages. Each one differs only in the anchor column (second link instead of last) or in the diagnostics that column causes.

`scripts/test.sh --project architecture`: 198 passed (requirement tags valid).

## Scenarios (bug fix)

_N/A_ (behavior change, repair round).

## Not covered, and why

- RH5112 fix with a prefix of three or more links: RH5112's fix delegates its layout to the formatter. The column is the same `GetAnchorLink` value the formatter test above exercises: `rg -n "GetAnchorLink" src --glob '!*Test*'` → `MethodChainAlignmentContributor.GetAnchorColumn` and `RH5201MethodChainsShouldBeAlignedAnalyzer.GetAnchorColumn` only. I added no separate RH5112 case; the RH5111 case covers the fix path through the formatter.
- Separate CRLF variants for the Core rows: line endings do not reach `StartsLine` differently for these shapes, and the formatter test and the RH5111 test run under both line endings.
- Every other failure class: unchanged from `04-test-implementer.md`. This round adds coverage of one dimension only (prefix length).

## Commands

```shell
scripts/test.sh --project core --filter "FullyQualifiedName~FluentChainTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~PrefixChainAnchorTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5201|FullyQualifiedName~RH5112|FullyQualifiedName~RH5111"
```

## Follow-up candidates

_None._
