Role: test implementer
Commit: a3674ac (tests written against 3fcf0de)
Verdict: reproduced

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions.

All tests live in `tests/Reihitsu.Analyzer.Test/` in each rule's existing test class (`Formatting/RH50xx…Tests.cs`, `Formatting/RH5103…`, `RH5110…`, `RH5206…`, `RH5301…`, `RH5303…`, `RH5403…`, `RH5513…`, `RH5527…`, `Formatting/RH3208…Tests.cs`, `Clarity/RH3004UseLambdaSyntaxAnalyzerTests.cs`). Two test-base helpers were added; no production code changed:

- `AnalyzerTestsBase<TAnalyzer>.CreateLanguageVersionTransform(LanguageVersion)`: a solution transform that sets the test project's language version.
- `AnalyzerTestsBase<TAnalyzer, TCodeFix>.VerifyNoCodeFixIsOffered(string, LanguageVersion)`: the existing no-fix check, with the ad-hoc document parsed at that language version. The existing overloads always parsed at `Latest`, so a C# 10 no-fix check was not possible. `CreateTestDocument` and the private core gained an optional `languageVersion` parameter (default `Latest`), so every existing caller behaves as before.

Analyzer fixtures follow the spec examples. In each rule's C# 10 test, `M` is the direct `$"` hole and `N` (or `Q`) is `$@"x {$"a {…}"} y"`; both must not be reported. `O` (or `N`) is the same construct outside every hole that only contains a `$"` string, and it stays reported. The C# 11 test marks `M` and `N`. The verbatim test marks a `$@"` member and an `@$"` member under C# 10.

## Tests

"Fails now (repro)" means the test fails on 3fcf0de for the issue's reason: the analyzer reports in `M`/`N` ("expected 1 actual 3"), or a code fix is offered or applied in the hole. Every failure was checked to be that reason, not a compile or markup error.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5001…RH5010, RH5012…RH5019, RH5021, RH5029 (20 classes): `<Kind>InsideANonVerbatimInterpolationIsNotReportedBeforeCSharp11` (`TryStatement`, `IfStatement`, `WhileStatement`, `DoStatement`, `UsingStatement`, `ForeachStatement`, `ForStatement`, `ReturnStatement`, `GotoStatement`, `BreakStatement`, `ContinueStatement`, `ThrowStatement`, `SwitchStatement`, `CheckedStatement`, `UncheckedStatement`, `FixedStatement`, `LockStatement`, `YieldStatement`, `ExpressionStatementAfterLocalDeclaration`, `LocalDeclarationAfterStatement`) | SHARED-STMT-005 | `Verify(source, C#10 transform)` | Fails now (repro), 20 tests |
| same 20 classes: `<Kind>InsideANonVerbatimInterpolationIsReportedFromCSharp11` | SHARED-STMT-005 | `Verify`, C# 11 | Passes |
| same 20 classes: `<Kind>InsideAVerbatimInterpolationIsReportedBeforeCSharp11` (`$@"` and `@$"`) | SHARED-STMT-005 | `Verify`, C# 10 | Passes |
| RH5008 `ReturnStatementInsideAnInterpolationWithAnAlignmentOrFormatClauseIsNotReportedBeforeCSharp11` (`,10` and `:D3`) | SHARED-STMT-005 | `Verify`, C# 10 | Fails now (repro) |
| RH5008 `ReturnStatementInsideAnInterpolationWithAnAlignmentOrFormatClauseIsReportedFromCSharp11` | SHARED-STMT-005 | `Verify`, C# 11 | Passes |
| RH5008 `ReturnStatementInsideANonVerbatimInterpolationIsNotReportedBeforeCSharp10` (C# 7.3) | SHARED-STMT-005 | `Verify`, C# 7.3 | Fails now (repro) |
| RH5008 `ReturnStatementInsideAnAnonymousMethodInsideANonVerbatimInterpolationIsNotReportedBeforeCSharp11` | SHARED-STMT-005 | `Verify`, C# 10 | Fails now (repro) |
| RH5008 `ReturnStatementBehindACommentInsideANonVerbatimInterpolationIsNotReportedBeforeCSharp11` (`/* c */`) | SHARED-STMT-005 | `Verify`, C# 10 | Fails now (repro) |
| RH5011 `BreakStatementInsideANonVerbatimInterpolationIsNotReportedBeforeCSharp11` / `…IsReportedFromCSharp11` / `BreakStatementInsideAVerbatimInterpolationIsReportedBeforeCSharp11` | RH5011-009 | `Verify` | Fails now (repro) / Passes / Passes |
| RH5103 `StatementInsideANonVerbatimInterpolationIsNotReportedBeforeCSharp11` (direct, nested, `,10` alignment) / `…IsReportedFromCSharp11` / `StatementInsideAVerbatimInterpolationIsReportedBeforeCSharp11` | RH5103-006 | `Verify` | Fails now (repro) / Passes / Passes |
| RH5110 `ConstraintClauseInside…` (three tests) | RH5110-004 | `Verify` | Fails now (repro) / Passes / Passes |
| RH5206 `SwitchExpressionInside…` (three tests) | RH5206-004 | `Verify` | Fails now (repro) / Passes / Passes |
| RH5403 `BlockInside…` (three tests) | RH5403-004 | `Verify` | Fails now (repro) / Passes / Passes |
| RH5301 `ObjectInitializerInside…` (three tests) | SHARED-INIT-003 | `Verify` | Fails now (repro) / Passes / Passes |
| RH5301 `ObjectInitializerWithCommentsInsideANonVerbatimInterpolationIsNotReportedBeforeCSharp11` (`/* c */`, `/** c */`) | SHARED-INIT-003 | `Verify`, C# 10 | Fails now (repro) |
| RH5301 `ObjectInitializerThreeInterpolationsDeepIsNotReportedBeforeCSharp11` (`$"{$@"{$"{…}"}"}"`) | SHARED-INIT-003 | `Verify`, C# 10 | Fails now (repro) |
| RH5301 `ObjectInitializerInsideAVerbatimInterpolationNextToANonVerbatimStringIsReportedBeforeCSharp11` (`$@"a {$"c {x}" + new C { P = 2 }.P} b"`) | SHARED-INIT-003 | `Verify`, C# 10 | Passes |
| RH5301 `NestedAndIndexerInitializersInsideANonVerbatimInterpolationAreNotReportedBeforeCSharp11` (`{ Q = { P = 1 } }`, `{ [1] = 2 }`, inside an anonymous object) | SHARED-INIT-003 | `Verify`, C# 10 | Fails now (repro) |
| RH5303 `CollectionInitializerInside…` (three tests) | SHARED-INIT-003 | `Verify` | Fails now (repro) / Passes / Passes |
| RH5303 `DictionaryCollectionInitializerInsideANonVerbatimInterpolationIsNotReportedBeforeCSharp11` | SHARED-INIT-003 | `Verify`, C# 10 | Fails now (repro) |
| RH5513 `LambdaAttributeListInside…` (three tests) | SHARED-ATTR-024 | `Verify` | Fails now (repro) / Passes / Passes |
| RH5513 `LocalFunctionAttributeListInsideANonVerbatimInterpolationIsNotReportedBeforeCSharp11` / `…IsReportedFromCSharp11` | SHARED-ATTR-024 | `Verify` | Fails now (repro) / Passes |
| RH5527 `LambdaReturnAttributeListInside…` (three tests) | SHARED-ATTR-024 | `Verify` | Fails now (repro) / Passes / Passes |
| RH3004 `BlockBodyInsideANonVerbatimInterpolationOffersNoFixBeforeCSharp11` | RH3004-010 | `VerifyNoCodeFixIsOffered(source, CSharp10)` | Fails now (repro): a fix is offered |
| RH3004 `BlockBodyInsideANonVerbatimInterpolationBehindAVerbatimOneOffersNoFixBeforeCSharp11` (`$"x {$@"a {G(delegate…)} b"} y"`) | RH3004-010 | same | Fails now (repro) |
| RH3004 `FixAllLeavesTheBlockBodyInsideANonVerbatimInterpolationBeforeCSharp11` (hole method stays reported, method `N` fixed) | RH3004-010 | `Verify(source, fixed, C#10)` | Fails now (repro): the hole is rewritten over lines |
| RH3004 `BlockBodyInsideANonVerbatimInterpolationIsFixedFromCSharp11` | RH3004-010 | `Verify(source, fixed, C#11)` | Passes |
| RH3004 `BlockBodyInsideAVerbatimInterpolationIsFixedBeforeCSharp11` | RH3004-010 | `Verify`, C# 10 | Passes |
| RH3004 `ExpressionBodyInsideANonVerbatimInterpolationIsFixedBeforeCSharp11` (`$"a {G((int p) => p)} b"`) | RH3004-010 | `Verify`, C# 10 | Passes |
| RH3004 `BlockBodyHoldingAnInterpolatedStringIsFixedBeforeCSharp11` | RH3004-010 | `Verify`, C# 10 | Passes |
| RH3208 `LocalFunctionInsideANonVerbatimInterpolationOffersNoFixBeforeCSharp11` | RH3208-004 | `VerifyNoCodeFixIsOffered(source, CSharp10)` | Fails now (repro) |
| RH3208 `LocalFunctionInsideANonVerbatimInterpolationBehindAVerbatimOneOffersNoFixBeforeCSharp11` | RH3208-004 | same | Fails now (repro) |
| RH3208 `FixAllLeavesTheLocalFunctionInsideANonVerbatimInterpolationBeforeCSharp11` | RH3208-004 | `Verify(source, fixed, C#10)` | Fails now (repro) |
| RH3208 `LocalFunctionInsideANonVerbatimInterpolationIsFixedFromCSharp11` | RH3208-004 | `Verify`, C# 11 | Passes |
| RH3208 `LocalFunctionInsideAVerbatimInterpolationIsFixedBeforeCSharp11` | RH3208-004 | `Verify`, C# 10 | Passes |
| RH3208 `LocalFunctionHoldingAnInterpolatedStringIsFixedBeforeCSharp11` | RH3208-004 | `Verify`, C# 10 | Passes |

Totals: 44 tests fail on 3fcf0de, all reproductions. Every other test in the analyzer project passes (12,210 passed). `scripts/test.sh --project architecture` passes, `RequirementCoverageTests` included, so all ten new IDs are covered.

The expected outputs of the RH3004 and RH3208 fix tests that pass (C# 11, `$@"`, contains-a-string, and the `N` side of the Fix All tests) are today's output on these cells. The run does not change these cells. I checked each against the specs:

- RH3004: braces in the column of `(`, statements kept as written (RH3004-006).
- RH3208: `} return L();` stays on one line, because no line break lies between the new brace and the next statement, so RH3208-002 inserts no blank line.

The specs give no literal output for these cells.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH5301 `$"a {new C { P = 1 }.P} b"`, C# 10 (issue example family) | LF | analyzer | reproduced | reported in the `$"` hole and in the `$@"{$"…"}` hole; expected not reported |
| RH5303 / RH5206 / RH5008 / RH5513 / RH5103 (issue rules), C# 10 | LF | analyzer | reproduced | each reported in the hole; expected not reported |
| RH5104 (issue rule) | — | — | not tested | dropped by lead decision 3; RH5104-001 keeps the report |
| Skeptic's widened members (RH5001–RH5007, RH5009–RH5019, RH5021, RH5029, RH5110, RH5403, RH5527) | LF | analyzer | reproduced | each reported in the hole; expected not reported |
| RH3004 block path in a `$"` hole, C# 10 | LF | code fix and Fix All | reproduced | a fix is offered and writes line breaks into the hole; expected no fix |
| RH3208 in a `$"` hole, C# 10 | LF | code fix and Fix All | reproduced | same |

## Not covered, and why

- **CRLF variants.** The analyzer cells report or do not report a construct that sits on one line, so line endings do not decide them. The fix-side reproductions assert "no action", which has no layout. The holding fix cells only pin outputs that the run does not change, and existing RH3004/RH3208 tests already cover their CRLF layout.
- **Formatter / node-level regression test (skeptic candidate 9).** Lead decision 4 places the check in each fix, not in `ReihitsuFormatter`'s node-level entry points. A test that pins the entry points' current output for a target inside the hole would assert the CS8967 output the run stops reaching. Whole-document formatting is already covered by `NonVerbatimInterpolationLineBreakTests`.
- **Code fixes of the analyzer members (RH5008, RH5301, …).** In the hole there is no diagnostic left to fix. Their fixes outside the hole are unchanged and already covered.
- **One-pass convergence.** The RH3004/RH3208 fixes are unchanged where they are still offered. Their existing tests pin convergence, and `Verify` re-analyzes the fixed code.
- **Per-rule C# 7.3/9, format clause, comment, and anonymous-method cells.** These are tested once, on RH5008. The SHARED-STMT rules share `StatementShouldBePrecededByABlankLineAnalyzerBase<TStatement>`. That base has per-rule overrides, which is not a one-call-site proof, so every rule has its own three cells. These extra cells only vary the position inside the hole, which the shared predicate `StringInterpolationUtilities.IsInsideSingleLineInterpolation` decides.
- **Comment or directive in the gap (FC-019/FC-020).** These rules judge whether a line break can be written at all, not a token gap. A directive cannot stand inside a one-line `$"` hole. The `/* c */` and `/** c */` cells (FC-019/FC-040) are covered on RH5008 and RH5301.
- **Target-typed `new() { … }` in a hole.** Left out because it has no target type inside an interpolation without a cast. The other initializer shapes the skeptic listed are covered.
- **Raw interpolated strings (FC-022).** `$"""…"""` needs C# 11, where every hole accepts line breaks, so the C# 10 condition cannot apply.
- **RH5017 at C# 11.** Also needs `ApplyAllowUnsafeToTestProject`; covered by its own three tests.
- **Failure classes walked and not applicable.** These have no rewrite, rename, phase, or unsupported shape in this run: FC-004/005/007/008/010–014/018/021/023–026/029/036/038–044. FC-028 (detached-node formatting) is reached only through RH3004 and RH3208, and their tests cover it at the fix.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~InterpolationIs|FullyQualifiedName~InterpolationOffersNoFix|FullyQualifiedName~InterpolationAre|FullyQualifiedName~ThreeInterpolationsDeep|FullyQualifiedName~BeforeCSharp10|FullyQualifiedName~HoldingAnInterpolatedStringIsFixed|FullyQualifiedName~FixAllLeaves|FullyQualifiedName~InterpolationNextToANonVerbatim|FullyQualifiedName~InterpolationBehindAVerbatimOne|FullyQualifiedName~AlignmentOrFormatClause"
scripts/test.sh --project architecture
```

## Follow-up candidates

_None_ beyond the lead's existing `apply-fix.sh` candidate.
