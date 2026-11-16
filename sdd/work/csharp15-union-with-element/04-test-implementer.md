Role: test implementer
Commit: 4713a0c
Verdict: #1762: tests written; #1763: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples and the change docs' *after* outputs; where neither states an output (a constructor initializer, a comment or directive in a gap the change touches), the expected value is the output today's CLI gives the class analog with a primary constructor, which the change doc names as the oracle ("each after is the output a class gets today"); those probes ran at 4713a0c on unchanged code.

Every failing test below was run at 4713a0c and fails on the specified-versus-actual difference (union left in its interim layout, `with (` kept, RH7101 fix not offered), not on a compile error or unrelated layout. Every formatter test uses `AssertRuleResult`, so each covers LF and CRLF, a second pass, and the per-phase stability check.

## thoenissen/Reihitsu#1762

### Tests

Formatter tests parse with explicit C# 15 options (`LanguageVersion.CSharp15`).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `UnionDeclarationLayoutTests.UnionBracesStartTheirOwnLines` | FMT-BREAK-002 (union example) | `AssertRuleResult` | fail |
| `UnionDeclarationLayoutTests.UnionMembersStartTheirOwnLines` | FMT-BREAK-007 (union example) | `AssertRuleResult` | fail |
| `UnionDeclarationLayoutTests.OneLineUnionsGetTheirBracesAndMembersOnTheirOwnLines` | FMT-BREAK-002, FMT-BREAK-007 (change 1: U1, U4 with base list and block body) | `AssertRuleResult` | fail |
| `UnionDeclarationLayoutTests.UnionWithAConstructorIsLaidOutAsAClass` | FMT-BREAK-002; FC-025 | `AssertRuleResult` | fail |
| `UnionDeclarationLayoutTests.ReadonlyUnionIsLaidOut` | FMT-BREAK-002, FMT-BREAK-007; FC-024 sibling modifier | `AssertRuleResult` | fail |
| `UnionDeclarationLayoutTests.NestedUnionsAreLaidOut` | FMT-BREAK-002, FMT-BREAK-007; FC-027 | `AssertRuleResult` | fail |
| `UnionDeclarationLayoutTests.UnionWithARegionGetsItsFirstMemberOnTheLineAfterItsBrace` | FMT-BREAK-002; FC-020 (`#region` in the body) | `AssertRuleResult` | fail |
| `UnionDeclarationLayoutTests.DirectiveInFrontOfTheOpeningBraceOfAUnionStays` | FMT-BREAK-002; FC-020 | `AssertRuleResult` | fail |
| `UnionDeclarationLayoutTests.DirectiveInFrontOfAUnionConstraintClauseStays` | FMT-BREAK-002, FMT-BREAK-023; FC-020 | `AssertRuleResult` | fail |
| `UnionDeclarationLayoutTests.LaidOutUnionStaysUnchanged` | FMT-BREAK-002, FMT-BREAK-007 (stable valid code) | `AssertRuleResult` (no expected) | pass |
| `UnionDeclarationLayoutTests.UnionCaseTypeListIsLaidOutAsAPrimaryConstructorParameterList` | FMT-BREAK-011 (union example) | `AssertRuleResult` | fail |
| `UnionDeclarationLayoutTests.UnionCaseTypeListOnTheNextLineJoinsTheNameOfAUnionWithABody` | FMT-BREAK-011 (change 2, braced sibling) | `AssertRuleResult` | fail |
| `UnionDeclarationLayoutTests.WrappedUnionCaseTypeListKeepsItsAlignedLayout` | FMT-BREAK-011 (change 3: no change) | `AssertRuleResult` | pass |
| `UnionDeclarationLayoutTests.BlockCommentBetweenUnionNameAndCaseTypeListStays` | FC-019/FC-021 (other side of the `(`-join) | `AssertRuleResult` (no expected) | pass |
| `UnionDeclarationLayoutTests.EndOfLineCommentBetweenUnionNameAndCaseTypeListKeepsItsLineBreak` | FC-021 | `AssertRuleResult` | pass |
| `UnionDeclarationLayoutTests.UnionConstraintClausesStartTheirOwnLines` | FMT-BREAK-023 (union example) | `AssertRuleResult` | fail |
| `UnionDeclarationLayoutTests.OneLineUnionWithConstraintClausesIsLaidOut` | FMT-BREAK-002, FMT-BREAK-023 (change 4: U6, U8) | `AssertRuleResult` | fail |
| `UnionDeclarationLayoutTests.ConstraintClauseBehindABlockCommentStartsItsOwnLine` | FMT-BREAK-023; FC-021 | `AssertRuleResult` | fail |
| `UnionDeclarationLayoutTests.ConstraintClauseBehindAnEndOfLineCommentStaysUnchanged` | FMT-BREAK-023 (already-placed side); FC-021 | `AssertRuleResult` (no expected) | pass |
| `UnionDeclarationStructuralTests.EmptyUnionBodyBecomesASemicolon` | FMT-STRUCT-017 (union example) | `AssertRuleResult` | fail |
| `UnionDeclarationStructuralTests.EmptyUnionBodiesOnOneAndTwoLinesBecomeASemicolon` | FMT-STRUCT-017 (change 5) | `AssertRuleResult` | fail |
| `UnionDeclarationStructuralTests.EmptyPartialUnionBodyBecomesASemicolon` | FMT-STRUCT-017; FC-024 | `AssertRuleResult` | fail |
| `UnionDeclarationStructuralTests.UnionDeclaredWithASemicolonStaysUnchanged` | FMT-STRUCT-017 (stable side) | `AssertRuleResult` (no expected) | pass |
| `UnionDeclarationStructuralTests.EmptyUnionBodyWithABlockCommentKeepsItsBraces` | FMT-STRUCT-018, FMT-BREAK-002 (change 5, U9) | `AssertRuleResult` | fail |
| `UnionDeclarationStructuralTests.EmptyUnionBodyWithAnEndOfLineCommentKeepsItsBraces` | FMT-STRUCT-018, FMT-BREAK-002; FC-021 | `AssertRuleResult` | fail |
| `UnionDeclarationStructuralTests.EmptyUnionBodyWithADirectiveKeepsItsBraces` | FMT-STRUCT-018; FC-020 | `AssertRuleResult` (no expected) | pass |
| `UnionDeclarationStructuralTests.StaticFieldInAUnionIsSplit` | FMT-STRUCT-021 (union example; change 6) | `AssertRuleResult` | fail |
| `UnionDeclarationStructuralTests.StaticFieldInAOneLineUnionIsSplit` | FMT-STRUCT-021, FMT-BREAK-002 | `AssertRuleResult` | fail |
| `UnionDeclarationStructuralTests.ConstantInAOneLineUnionIsSplit` | FMT-STRUCT-021, FMT-BREAK-002 | `AssertRuleResult` | fail |
| `ReihitsuFormatterSplitsFieldDeclarationTests.ReturnsTrueForACombinedDeclarationInAUnion` | FMT-PIPE-031 (static and const) | direct call | fail |
| `ReihitsuFormatterSplitsFieldDeclarationTests.ReturnsFalseForADeclarationWithOneDeclaratorInAUnion` | FMT-PIPE-031 (other side) | direct call | pass |
| `RH7101DoNotCombineFieldsAnalyzerTests.VerifyCombinedStaticFieldInAUnionIsSplit` | RH7101-004 (change 6) | `Verify` (one pass + re-analysis) | fail (fix not offered) |
| `RH7101DoNotCombineFieldsAnalyzerTests.VerifyCombinedStaticFieldInAUnionIsSplitWithCarriageReturnLineFeed` | RH7101-004; FC-017 | `Verify` + `ApplyCodeFixAsync` on CRLF text | fail (fix not offered) |
| `RH7101DoNotCombineFieldsAnalyzerTests.VerifyCombinedFieldsInAUnionAreSplitTogether` | RH7101-004; FC-007, FC-008 (Fix All over two diagnostics in one union) | `Verify` | fail (fix not offered) |
| `RH5404ElementMustNotBeOnSingleLineAnalyzerTests.VerifyFixLaysOutAUnionInsideTheFixedType` | RH5404-003 (change 7); FC-028, FC-032 | `Verify` | fail (union left on one line) |
| `CliLanguageVersionEndToEndTests.UnionWithOneLineBracesIsLaidOutWithoutLangVersion` (was `UnionWithOneLineBracesKeepsThemWithoutLangVersion`) | FMT-BREAK-002, FMT-BREAK-007 | CLI end-to-end | fail (interim layout) |

The CLI test pinned the interim layout the skeptic named (`CliLanguageVersionEndToEndTests.cs`); its expected constant and name now state the specified layout.

The analyzer tests on unions set `CompilerDiagnostics.None`: against `ReferenceAssemblies.Net.Net90` every union raises CS0518 (`IUnion`) and CS0656 (`UnionAttribute..ctor`) — observed at 4713a0c, as thoenissen/Reihitsu#1766 reports. Each call carries a one-line comment stating this. Precedent: RH2004 and RH3201 tests use the same setting. The analyzer diagnostics and the fixed text are still verified exactly; only compiler errors are not.

### Not covered, and why

- **C# 14 gate of FMT-STRUCT-017 for unions:** no test, per lead decision 3 (no union node exists under C# 14; no caller reaches it).
- **RH7101 fix on a one-line union** (`union U(int, string) { public static int A = 1, B = 2; }`): no spec example states that output (RH7101-004 lays out only the created fields with the type as context, and whether the union's own braces move is not stated). Covered on the formatter side by `StaticFieldInAOneLineUnionIsSplit`.
- **Instance fields in a union:** CS9373, not compiled code (skeptic); only static and const fields are tested.
- **Attribute lists on a union, extension blocks, base list on its own line, semicolon on its own line:** outside the run (follow-up candidates and non-goals in `01-spec-author.md`, `02-skeptic.md`, lead decision 2). No test pins extension-block output.
- **Detached `FormatNode` on a union (FC-028):** the RH5404 fix covers the document-scoped node path; no code fix formats a detached union, so no caller reaches a detached-only path.
- **FC-022, FC-023, FC-026, FC-029, FC-036, FC-018:** no literal, identifier spelling, repeated-token, key/value, or rename decision is touched by adding a kind to the type-declaration lists.
- **FC-038 to FC-043:** no shape is declared unsupported in this run (lead decision 5).
- **FC-013:** the skeptic ran the after outputs through the RH55xx/RH7101 fixes with no diagnostic; the analyzer `Verify` calls above re-analyze each fixed text.
- **FC-012 / FC-010:** covered by `AssertRuleResult`'s second pass and per-phase stability check on every formatter test.

## thoenissen/Reihitsu#1763

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `WithElementSpacingTests.SpecExampleRemovesTheSpaceBetweenWithAndItsArgumentList` | FMT-SPACE-028 (spec example) | `AssertRuleResult` | fail |
| `WithElementSpacingTests.SpaceInFrontOfTheArgumentListIsRemoved` | FMT-SPACE-028 (change 1) | `AssertRuleResult` | fail |
| `WithElementSpacingTests.SpaceInFrontOfAnEmptyArgumentListIsRemoved` | FMT-SPACE-028 (`with()`) | `AssertRuleResult` | fail |
| `WithElementSpacingTests.TabInFrontOfTheArgumentListIsRemoved` | FMT-SPACE-028 | `AssertRuleResult` | fail |
| `WithElementSpacingTests.CommentInTheGapStaysAndTheSpaceBehindItIsRemoved` | FMT-SPACE-028 (change 2); FC-019 | `AssertRuleResult` | fail |
| `WithElementSpacingTests.SpaceBehindTheOpeningBracketAndInFrontOfTheArgumentListAreRemoved` | FMT-SPACE-028 | `AssertRuleResult` | fail |
| `WithElementSpacingTests.ElementWithoutASpaceStaysUnchanged` | FMT-SPACE-028 (stable side) | `AssertRuleResult` (no expected) | pass |
| `WithElementSpacingTests.WrappedCollectionExpressionStartingWithAWithElementGetsOneElementPerLine` | FMT-BREAK-022 (`with(…)` example) | `AssertRuleResult` | pass (no output change) |
| `WithElementSpacingTests.WrappedCollectionExpressionStartingWithASpacedWithElementIsLaidOut` | FMT-BREAK-022, FMT-SPACE-028; FC-012 | `AssertRuleResult` | fail (`with (4)` kept) |
| `RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixRemovesTheSpaceInFrontOfTheArgumentListOfAWithElement` | RH5305-004 (change 3); FC-032 | `Verify` | fail (`with (4)` kept) |

The other side of the parent condition (the `with` of a `with` expression keeps one space on each side, FMT-SPACE-025) is pinned by the existing `WithExpressionSpacingTests`, which stay green and must stay green.

`RequirementCoverageTests` now finds a test tagged FMT-SPACE-028; the architecture suite passes (118/118).

### Not covered, and why

- **A directive in the `with`–`(` gap (FC-020):** a directive needs a line break in the gap, and a gap with a line break is not FMT-SPACE-028's (FMT-SPACE-001). Its layout (`(4)` at the statement's column) is the spec author's follow-up candidate (b), not specified, so no test pins it.
- **A `with` expression on the same line as a `with(…)` element:** the rule keys on the token's parent; `WithExpressionSpacingTests` covers the `WithExpressionSyntax` side.
- **FC-038 to FC-043:** no shape declared. **FC-022/023/036:** not touched.

## Whole run

- Formatted with `scripts/format.sh` (all eight changed test files).
- New formatter tests at 4713a0c: 30 fail as listed, the remaining tests of the touched classes pass. New analyzer tests: 5 fail as listed. CLI: 1 fail as listed, 15 pass. Architecture: 118 pass.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~UnionDeclarationLayoutTests|FullyQualifiedName~UnionDeclarationStructuralTests|FullyQualifiedName~WithElementSpacingTests|FullyQualifiedName~ReihitsuFormatterSplitsFieldDeclarationTests|FullyQualifiedName~WithExpressionSpacingTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~VerifyCombinedStaticFieldInAUnion|FullyQualifiedName~VerifyCombinedFieldsInAUnionAreSplitTogether|FullyQualifiedName~VerifyFixLaysOutAUnionInsideTheFixedType|FullyQualifiedName~VerifyFixRemovesTheSpaceInFrontOfTheArgumentListOfAWithElement"
scripts/test.sh --project cli --filter "FullyQualifiedName~CliLanguageVersionEndToEndTests"
scripts/test.sh --project architecture
```
