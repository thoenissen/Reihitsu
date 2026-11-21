Role: test implementer
Commit: 48aace7
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples and the change doc; where neither gives a fix result (the skeptic's extra candidates), the expected text is the formatter's layout (RH5201-005), and I checked each against `scripts/format.sh` on the unchanged formatter. Every new or changed analyzer test fails on this head only because the new diagnostics are missing (`expected "n" actual "m"`, the missing ones being the first call or the link behind `*/`). Every other new test passes.

## Tests

New class `src/Reihitsu.Analyzer.Test/Formatting/RH5201MethodChainsShouldBeAlignedFirstCallTests.cs` (`BatchCodeFixTestsBase`). Every method runs under LF and CRLF (`DataRow`). Fix tests use `Verify(source, fixed, …)` with `NumberOfFixAllIterations = 1` and an exact `NumberOfIncrementalIterations` (one per chain), so one fix application per chain must clear every diagnostic of that chain (FC-007, FC-008).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFirstCallBehindWrappedPrefixOrHeldFirstLinkIsReportedAndFixed` — RH5201-006 example 1 (`a`, `b`, `d` reported; `e`, `f`, `h` not), fix results from the spec and change doc 2, 3 | RH5201-006, RH5201-005, SHARED-CHAIN-005; FC-003 (`e` beside `b`) | `Verify` + fix | fail (3 expected, 0 reported) |
| `VerifyFirstCallBehindSplitPrefixOperatorOrDirectiveHeldFirstLinkIsReportedAndFixed` — RH5201-006 example 2 | RH5201-006, RH5201-003, RH5201-005, SHARED-CHAIN-005; FC-020 | `Verify` + fix | fail (3 expected, 1) |
| `VerifyFirstCallBehindCommentHeldSplitOfPrefixOperatorIsNotReported` — RH5201-006 example 3 (`g`) | RH5201-006; FC-003 against example 2 `a` | `Verify` | pass |
| `VerifyFirstCallBehindCommentHeldSplitOfFirstLinkOperatorIsNotReported` — `c? // keep` ⏎ `.Next.Self()` (skeptic row 10, F05) | RH5201-006 | `Verify` | pass |
| `VerifyLinksBehindOwnLineCommentTailInAnchorColumnAreReportedAndFixed` — RH5201-002 `*/` example (`a`, `b`, `d` reported, `e` not), fix results from the spec | RH5201-002, RH5201-006, RH5201-005, SHARED-COMMENT-012; FC-021 | `Verify` + fix | fail (4 expected, 0) |
| `VerifyFirstCallBehindFirstLinkHeldByBlockCommentOnRootLineIsReportedAndFixed` — F01 | RH5201-006, RH5201-005; FC-021 | `Verify` + fix | fail (1, 0) |
| `VerifyFirstCallBehindFirstLinkHeldByBlockCommentInFrontOfItIsReportedAndFixed` — F02 | RH5201-006, RH5201-005; FC-021 | `Verify` + fix | fail (1, 0) |
| `VerifyFirstCallBehindWrappedPrefixLinkBehindWrappedFirstLinkIsReportedAndFixed` — F06 (fix also joins the first link) | RH5201-006, RH5201-005 | `Verify` + fix | fail (1, 0) |
| `VerifyConditionalFirstCallBehindWrappedConditionalPrefixLinkIsReportedAndFixed` — F07, reported on `?` | RH5201-006, RH5201-005 | `Verify` + fix | fail (1, 0) |
| `VerifyFirstCallBehindDirectiveWrappedPrefixLinkIsReportedAndFixedInTheActiveBranch` — F08, `!.` link in `#else` | RH5201-006, RH5201-005; FC-005, FC-020 | `Verify` + fix | fail (1, 0) |
| `VerifyFirstCallBehindWrappedPrefixLinkOfParenthesizedRootIsReportedAndFixed` — F13 | RH5201-006, RH5201-005 | `Verify` + fix | fail (1, 0) |
| `VerifyFirstCallBehindSplitNullForgivingPrefixOperatorIsReportedAndFixed` — F16, two diagnostics in one chain | RH5201-006, RH5201-003, RH5201-005; FC-007, FC-008 | `Verify` + fix | fail (2, 1) |
| `VerifyFirstCallBehindSplitNullForgivingConditionalPrefixOperatorIsReportedAndFixed` — H2 (`!?.`) | RH5201-006, RH5201-003, RH5201-005; FC-007, FC-008 | `Verify` + fix | fail (2, 1) |
| `VerifyFirstCallBehindSplitFirstLinkOperatorIsNotReported` — F15: only the split `!` is reported; the fix joins to one line | RH5201-006, RH5201-003, RH5201-005; FC-003 against F16 | `Verify` + fix | pass |
| `VerifyFirstCallBehindPrefixLinkBehindTailOfCommentStartingBehindCodeIsNotReported` — F03 shape (`c.Next /* x` ⏎ `*/.Next.Self()` ⏎ `.Self()` under the first call) | RH5201-006; FC-003 against F11/`d` | `Verify` | pass |
| `VerifyCallLessChainBehindCommentHeldFirstLinkIsNotReported` — F12 | RH5201-001 | `Verify` | pass |
| `GetFixAllScenario` (runs as the inherited `FixAllCorrectsEveryDiagnostic`) — two chains, one with two diagnostics, Fix All in one iteration | FC-008 | `BatchCodeFixTestsBase` | fail (3, 1) |

Added to `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5201MethodChainsShouldBeAlignedFormatterTests.cs` (`VerifyFormatter`: diagnostics before, formatter output exact, analyzer-clean and stable on a second pass, LF and CRLF; FC-010, FC-013):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFormatterBreaksFirstCallBehindWrappedPrefixLink` — the issue's own shape (`declaration.Declaration` ⏎ `.Variables.Select(…)`) | RH5201-006; FC-013, FC-034 | `VerifyFormatter` | fail (1, 0) |
| `VerifyFormatterBreaksFirstCallBehindCommentHeldFirstLink` — change doc 2 | RH5201-006; FC-013 | `VerifyFormatter` | fail (1, 0) |
| `VerifyFormatterBreaksLinkBehindOwnLineCommentTailInAnchorColumn` — skeptic G1 | RH5201-002; FC-013 | `VerifyFormatter` | fail (1, 0) |

Added to `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ChainLinkLayoutTests.cs`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FirstCallTheRootLineDoesNotReachStartsItsOwnLine` — the new SHARED-CHAIN-005 example (`a`, `b` broken, `c` joined) | SHARED-CHAIN-005, FMT-BREAK-019; FC-010 | `AssertRuleResult` (LF, CRLF, second pass, phase stability) | pass (characterization: formatter unchanged) |

Existing tests whose asserted output the change alters — changed:

| Test | Change | Expected now |
|---|---|---|
| `RH5201CommentExemptChainWithNonInvokedPrefixDotTests.VerifyFormatterFixesCommentExemptChainWithNonInvokedPrefixAndStaysIdempotent` | `a` ⏎ `// keep wrapped` ⏎ `.Prop.Foo()`: the comment holds the first link below the root, so `.Foo` is now marked (`{|#3:.|}`), count 3 → 4; summary updated; tagged RH5201-006 | fail (4, 3) |
| `RH5201FluentChainLinkFormatterTests.VerifyCommentKeptFirstLinkAlignsChainToRootColumn` | same shape (`// Comment` ⏎ `.Prop.Foo()`): `.Foo` marked (`{|#1:.|}`), count 1 → 2; summary updated; tagged RH5201-006 | fail (2, 1) |

Existing tests checked and left unchanged (still assert the right output): `RH5201FluentChainLinkFormatterTests.VerifyCallsBehindWrappedPrefixFirstLinkAreReported` (`a` ⏎ `.Prop.Foo().Bar()`: plain break in front of the first link, not counted), `VerifyLinksAlignedUnderTheLastPrefixLinkStayUnchanged`, `VerifyMisalignedLinksOfCallLessChainAreReported`, `VerifyMisalignedPrefixLinkIsReported` (call-less); `RH5201MethodChainsShouldBeAlignedFormatterTests.VerifyFormatterBreaksPrefixLinkBehindMultiLineCommentTail` (example `r` shape; first call already starts a line, `*/` off the anchor column, one diagnostic before and after); `RH5201MethodChainsShouldBeAlignedAnalyzerTests` `VerifyExampleOfLaterCallSharingALineIsReported`, `VerifyLinkWrappedBehindPrefixIsReportedUnlessUnderTheLastPrefixLink` (`f`: first call starts a line), `VerifyLinksOfCallLessChainSharingALineAreNotReported`, `VerifyLinkBehindCommentThatStartsBehindCodeIsTheAnchor` (comment starts behind code), `VerifyFixPreservesDirectiveInTokenGap` and `VerifyChainWithDisabledLinkReportsNoDiagnostic` (first call starts a line), `VerifyNoDiagnosticForLinkBehindBlockCommentStartingTheLineInTheAnchorColumn(WithCrLf)` (single-line `/* all */`, the boundary of the RH5201-002 `*/` clause, which needs a comment spanning several lines). RH5112, RH5111, RH5038 tests that matched the search run only their own analyzer.

How I searched: `rg --pcre2` over `src/Reihitsu.Analyzer.Test` for lines that start with a link operator (markup and `?.`, `!.`, `!?.`, element access allowed) followed by a second link on the same line, and for lines starting with `*/` followed by a link operator. Formatter tests are unaffected because the formatter does not change.

Fixture deviation from the spec: RH5201-006 example 2 and example `g` declare `C? Next`, so `c.Next?.Next.Self()` raises CS8602, which the verifier counts as a diagnostic. The tests use `C Next` plus `C? Opt` and write `c.Opt?` ⏎ `.Next.Self()`: same tokens, operators, columns, and layout.

## Not covered, and why

- FC-001, FC-002, FC-015, FC-016, FC-044 (guard scope, shared owner): code-shape classes. The tests cover both sides of each condition the spec adds (rows 1–3, 5, 7 reported; rows 4, 6, 8, 9, 10 not; the first call starting a line; a call-less chain). Whether the predicate lives in `FluentChain` (lead decision 3) is the auditor's to check.
- FC-004, FC-006, FC-022–FC-026, FC-029, FC-036, FC-018: the change adds a diagnostic and no rewrite; the fix code is unchanged and its output is the formatter's.
- FC-009, FC-043: no fix is offered on `g`/F05, because nothing is reported there. The existing `NoFixIsOfferedForACommentInsideALinkOperator` covers the refused shape.
- FC-011, FC-012: formatter unchanged.
- FC-027: nesting. The existing nested-chain and argument tests keep running. A first call behind a wrapped prefix inside a lambda argument is the same predicate on another `FluentChain` instance (one dispatch, `FluentChainAnalyzerBase.AnalyzeChain`), so I wrote no extra test.
- FC-028: the fix formats through the existing path; no new entry point.
- FC-030, FC-031–FC-033, FC-035, FC-037: process classes. Expected values come from the spec and the unchanged formatter, not from patched code.
- FC-038–FC-043: no shape is declared unsupported.
- RH5112 + RH5201 combined convergence on F06 / change doc `e`: not added. RH5112's fix output is the formatter's too, and only RH5201's fix is in question here.
- A `*/` followed by another comment in front of the link (the RH5201-002 clause requires only whitespace between `*/` and the operator): no spec example decides its output beyond SHARED-COMMENT-011, so I did not test it.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5201"
scripts/test.sh --project formatter --filter "FullyQualifiedName~ChainLinkLayoutTests"
scripts/test.sh --project architecture
```

On this head: analyzer RH5201 filter 28 failed / 153 passed (exactly the tests listed as failing above, under both line endings); formatter `ChainLinkLayoutTests` 85 passed; architecture 198 passed (RH5201-006 now has covering tests); analyzer `Reihitsu.Analyzer.Test.SelfHosting` 16 passed.

## Follow-up candidates

_None._
