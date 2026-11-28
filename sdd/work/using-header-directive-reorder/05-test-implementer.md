Role: test implementer
Commit: 8d097e1
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples, the change doc's cases, and the requirement text.

## Tests

"Fails now" means: fails on 8d097e1 (code equals `main`) and must pass after the implementation. "Passes now" means: pins behavior that must stay. Every analyzer test marked LF/CRLF runs both line endings through `[DataRow]`; every formatter test through `AssertRuleResult` or `AssertOrganizes` runs LF and CRLF, and `AssertRuleResult` also checks a second pass and phase stability.

### Core — `tests/Reihitsu.Core.Test/UsingDirectiveOrderingSafetyTests.cs` (the predicate, LF/CRLF each)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NullableDirectiveBeforeFirstDirectiveIsSafe` (flipped from `…IsUnsafe`) | SHARED-USING-009; FC-003 | `CanSafelyReorder` | fails now |
| `NullableDirectiveFollowedByBlankLineBeforeFirstDirectiveIsSafe` | SHARED-USING-009 | same | fails now |
| `PragmaDirectiveBeforeFirstDirectiveIsSafe` | SHARED-USING-009; FC-002 (non-conditional kinds) | same | fails now |
| `DefineDirectiveBeforeFirstDirectiveIsSafe` | SHARED-USING-009; FC-002 | same | fails now |
| `RegionDirectiveBeforeFirstDirectiveIsSafe` | SHARED-USING-009; FC-002 | same | fails now |
| `HeaderCommentAndTwoDirectivesBeforeFirstDirectiveAreSafe` | SHARED-USING-009 | same | fails now |
| `NullableDirectiveBehindExternAliasBeforeFirstDirectiveIsSafe` | SHARED-USING-009 | same | fails now |
| `NullableDirectiveBeforeFirstGlobalDirectiveIsSafe` | SHARED-USING-009 | same | fails now |
| `NullableDirectiveBeforeFirstDirectiveOfBlockNamespaceIsSafe` | SHARED-USING-009; FC-027 | same | fails now |
| `NullableDirectiveBeforeFirstDirectiveOfFileScopedNamespaceIsSafe` | SHARED-USING-009; FC-027 | same | fails now |
| `HeaderDirectiveWithDirectiveInLeadingTriviaOfLaterDirectiveIsUnsafe` | SHARED-USING-009; FC-001 | same | passes now |
| `PragmaBetweenTokensOfFirstDirectiveIsUnsafe` | SHARED-USING-009; FC-001, FC-003 | same | passes now |
| `HeaderDirectiveWithPragmaBetweenTokensOfFirstDirectiveIsUnsafe` | SHARED-USING-009; FC-001 | same | passes now |
| `ConditionalGroupBeforeFirstDirectiveIsUnsafe` | SHARED-USING-009; FC-002 | same | passes now |
| `ConditionalGroupEnclosingTheBlockIsUnsafe` | SHARED-USING-009; FC-002 | same | passes now |
| `EndIfDirectiveBeforeFirstDirectiveIsUnsafe` | SHARED-USING-009; FC-002 | same | passes now |
| `ElseDirectiveBeforeFirstDirectiveIsUnsafe` | SHARED-USING-009; FC-002 | same | passes now |
| `ElifDirectiveBeforeFirstDirectiveIsUnsafe` | SHARED-USING-009; FC-002 | same | passes now |

### Formatter, whole document — `tests/Reihitsu.Formatter.Test/Regression/UsingDirectives/UsingDirectiveHeaderDirectiveTests.cs` (new)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NullableHeaderStaysAtTheTopOfTheReorderedBlock` (case 1) | SHARED-USING-009, FMT-USING-012 | `AssertRuleResult` | fails now |
| `HeaderDirectiveBlockInCanonicalOrderGetsItsGroupBlankLine` (case 2) | SHARED-USING-009, FMT-USING-002 | same | fails now |
| `OrganizedBlockLedByAHeaderDirectiveIsUnchanged` | FMT-USING-012 (first directive keeps its position) | same | passes now |
| `DirectiveBehindAHeaderBlankLineStaysAtTheTop` (case 4, spec example) | FMT-USING-012 | same | fails now |
| `BlankLineBehindAHeaderDirectiveStaysBehindIt` (spec example) | FMT-USING-012 | same | fails now |
| `CommentDirectlyBelowAHeaderDirectiveStaysAtTheTop` (case 5, spec example) | FMT-USING-012 | same | fails now |
| `CommentBehindABlankLineBelowAHeaderDirectiveStaysAtTheTop` (case 6, spec example) | FMT-USING-012 | same | fails now |
| `RegionOpeningTheBlockIsFramedAndStaysAtTheTop` (case 3, spec example) | FMT-USING-012 | same | fails now |
| `TwoHeaderDirectivesSeparatedByABlankLineStayAtTheTop` | FMT-USING-012 | same | fails now |
| `CommentsAndBlankLineInFrontOfAHeaderDirectiveStayAtTheTop` | FMT-USING-012 | same | fails now |
| `CopyrightHeaderAndTwoDirectivesStayAtTheTopAndADirectiveBehindTheBlockStaysBehindIt` | SHARED-USING-009, FMT-USING-012 | same | fails now |
| `DefineHeaderStaysInFrontOfTheFirstToken` | SHARED-USING-009, FMT-USING-012 | same | fails now |
| `HeaderDirectiveInFrontOfGlobalDirectivesStaysAtTheTop` | SHARED-USING-009, FMT-USING-012 | same | fails now |
| `HeaderDirectiveOfANamespaceWithABodyStaysAtTheTop` | SHARED-USING-009, FMT-USING-012; FC-027 | same | fails now |
| `HeaderDirectiveOfAFileScopedNamespaceStaysAtTheTop` | SHARED-USING-009, FMT-USING-012; FC-027 | same | fails now |
| `HeaderDirectiveBlockWithSharedLineIsReorderedAndSplit` | SHARED-USING-009, FMT-USING-003 | same | fails now |
| `HeaderDirectiveDoesNotMakeABlockWithALaterDirectiveSafe` | SHARED-USING-009, FMT-USING-004; FC-001 | same | passes now |
| `DirectiveBetweenTokensOfTheFirstDirectiveKeepsTheBlock` | SHARED-USING-009, FMT-USING-004 | same | passes now |
| `HeaderDirectiveDoesNotMakeABlockWithADirectiveBetweenTokensOfItsFirstDirectiveSafe` | SHARED-USING-009, FMT-USING-004; FC-001 | same | passes now |
| `ConditionalGroupInFrontOfTheBlockKeepsIt` (case 7) | SHARED-USING-009, FMT-USING-004 | same | passes now |
| `ConditionalGroupInFrontOfACanonicalBlockAddsNoGroupBlankLine` | SHARED-USING-009, FMT-USING-004 | same | passes now |
| `ConditionalGroupInFrontOfTheBlockOnlySplitsASharedLine` (FMT-USING-004 example) | SHARED-USING-009, FMT-USING-004 | same | passes now |
| `ConditionalGroupEnclosingTheBlockKeepsIt` (case 8) | SHARED-USING-009, FMT-USING-004 | same | passes now |
| `ConditionalGroupInFrontOfANamespaceBlockKeepsIt` | SHARED-USING-009, FMT-USING-004; FC-027 | same | passes now |
| `HeaderDirectiveAndConditionalGroupInFrontOfTheBlockKeepIt` | SHARED-USING-009, FMT-USING-004; FC-001 | same | passes now |

### Formatter, code-fix path (FMT-PIPE-030) — `tests/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterUsingDirectivesTests.cs`

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RegionOpeningTheBlockStaysAsWrittenAtTheTopOfTheReorderedBlock` (case 3, fix output) | FMT-PIPE-030, FMT-USING-012; FC-028 | `AssertOrganizes` | fails now |
| `HeaderDirectiveBehindAHeaderBlankLineStaysAtTheTopOfTheReorderedBlock` (case 4) | FMT-PIPE-030, FMT-USING-012 | same | fails now |
| `CommentBehindABlankLineBelowAHeaderDirectiveStaysAtTheTopOfTheReorderedBlock` (case 6) | FMT-PIPE-030, FMT-USING-012 | same | fails now |
| `HeaderDirectiveChangesNoDirectiveIndentationInANamespaceWithABody` | FMT-PIPE-030, FMT-USING-011, FMT-USING-012 | same | fails now |
| `HeaderDirectiveBlockGetsItsGroupBlankLine` (case 2) | FMT-PIPE-030, SHARED-USING-009 | same | fails now |
| `BlockBehindAConditionalGroupIsNotReordered` | FMT-PIPE-030, SHARED-USING-009 | same | passes now |

### Formatter, changed existing test — `UsingDirectiveRequirementTests.cs`

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `UnsafeBlockOnlyGetsOneDirectivePerLine` — input changed from the `#nullable enable` header to the new spec example (`#pragma` between the second and third directive) | FMT-USING-004 | `AssertRuleResult` | passes now (its old input becomes safe and would be reordered) |

### Analyzer and code fixes

All in `tests/Reihitsu.Analyzer.Test/`. `VerifyCodeFixWithEndOfLine` runs the Roslyn verifier: incremental fixes, Fix All, a re-analysis of the result with no diagnostic left, and an exact-text check of one fix application.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7201/RH7202/RH7204/RH7205/RH7206 `BlockLedByANullableDirectiveIsFixedWithTheDirectiveAtTheTop` (the -004 spec example of each rule, LF/CRLF; RH7201 adds a `namespace Microsoft.CodeAnalysis` placeholder so that the example compiles) | RH7201-004 … RH7206-004 (one each), SHARED-USING-009; FC-007 | `VerifyCodeFixWithEndOfLine` | fails now |
| RH7203 `BlockLedByANullableDirectiveIsReportedAndFixed` (flipped from `…WithoutCodeFix`, LF/CRLF) | RH7203-001, RH7203-004, SHARED-USING-009 | `VerifyCodeFixWithEndOfLine` | fails now |
| RH7203 `CodeFixOfABlockLedByANullableDirectiveMatchesTheFormatter` | RH7203-004, SHARED-USING-009; FC-013, FC-028 | `VerifyCodeFixMatchesFormatter` | fails now |
| RH7203 `BlockLedByANullableDirectiveAndABlankLineIsFixed`, `BlockLedByAPragmaDirectiveIsFixed`, `BlockLedByADefineDirectiveIsFixed`, `BlockOpenedByARegionIsFixedWithTheRegionAsWritten`, `CommentHeaderAboveAHeaderDirectiveStaysAtTheTop`, `CommentDirectlyBelowAHeaderDirectiveStaysAtTheTop`, `CommentBehindABlankLineBelowAHeaderDirectiveStaysAtTheTop`, `CopyrightHeaderAndTwoDirectivesStayAtTheTopOfTheFixedBlock`, `HeaderDirectiveOfANamespaceWithABodyIsFixed`, `HeaderDirectiveOfAFileScopedNamespaceIsFixed`, `HeaderDirectiveInFrontOfGlobalDirectivesIsFixed` (each LF/CRLF) | RH7203-004, SHARED-USING-009; FC-005, FC-020, FC-027 | `VerifyCodeFixWithEndOfLine` | fails now |
| RH7203 `HeaderDirectiveWithADirectiveInFrontOfALaterDirectiveIsReportedWithoutCodeFix`, `PragmaBetweenTokensOfTheFirstDirectiveIsReportedWithoutCodeFix`, `HeaderDirectiveWithPragmaBetweenTokensOfTheFirstDirectiveIsReportedWithoutCodeFix`, `ConditionalGroupInFrontOfTheBlockIsReportedWithoutCodeFix`, `ConditionalGroupInFrontOfTheBlockIsReportedWithoutCodeFixInABuildThatCompilesIt` (`DEBUG`), `ConditionalGroupEnclosingTheBlockIsReportedWithoutCodeFix`, `HeaderDirectiveInFrontOfAConditionalGroupInFrontOfTheBlockIsReportedWithoutCodeFix` (each LF/CRLF) | RH7203-001, RH7203-004, SHARED-USING-009; FC-001, FC-003, FC-009 | `VerifyNoCodeFixIsOffered` | passes now |
| RH7203 `FixAllOrganizesTheHeaderDirectiveBlockAndKeepsTheBlockWithADirectiveBetweenItsDirectives` | RH7203-004, SHARED-USING-009; FC-008 | `Verify` (incremental 1, Fix All 1, remaining diagnostic marked) | fails now |
| RH7203 `FixAllOrganizesTwoBlocksLedByHeaderDirectives` | RH7203-004, SHARED-USING-009; FC-008 | `Verify` (incremental 2, Fix All 1) | fails now |
| RH7207 `BlockLedByANullableDirectiveIsReportedAndFixed` (flipped from `…WithoutCodeFix`, LF/CRLF) | RH7207-001, RH7207-004, SHARED-USING-009 | `VerifyCodeFixWithEndOfLine` | fails now |
| RH7207 `BlockLedByANullableDirectiveInCanonicalOrderIsReportedForAMissingBlankLineAndFixed` (case 2, LF/CRLF) | RH7207-001, RH7207-004, SHARED-USING-009 | same | fails now |
| RH7207 `BlockOpenedByARegionInCanonicalOrderIsReportedForAMissingBlankLineAndFixed` (LF/CRLF) | RH7207-001, RH7207-004, SHARED-USING-009 | same | fails now |
| RH7207 `OrganizedBlockLedByANullableDirectiveIsNotReported` (LF/CRLF) | RH7207-002, SHARED-USING-009 | `Verify` | passes now |
| RH7207 `BlockLedByANullableDirectiveWithADirectiveInFrontOfALaterDirectiveIsNotReportedForAMissingBlankLine`, `ConditionalGroupInFrontOfABlockInCanonicalOrderIsNotReportedForAMissingBlankLine`, `ConditionalGroupEnclosingABlockInCanonicalOrderIsNotReportedForAMissingBlankLine` (each LF/CRLF) | RH7207-001, SHARED-USING-009; FC-001, FC-003 | `Verify` | passes now |
| RH7207 `ConditionalGroupInFrontOfTheBlockIsReportedWithoutCodeFix`, `ConditionalGroupEnclosingTheBlockIsReportedWithoutCodeFix`, `PragmaBetweenTokensOfTheFirstDirectiveIsReportedWithoutCodeFix` (each LF/CRLF) | RH7207-001, RH7207-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | passes now |
| RH7207 `FixAllAddsTheGroupBlankLinesOfTwoBlocksLedByHeaderDirectives` | RH7207-004, SHARED-USING-009; FC-008 | `Verify` (incremental 2, Fix All 1) | fails now |
| RH7207 formatter `FormatterOrganizesAReportedBlockLedByANullableDirective` (flipped from `FormatterKeepsAReportedBlockLedByANullableDirective`) | SHARED-USING-009; FC-013 | `VerifyFormatter` | fails now |
| RH7207 formatter `FormatterAddsTheGroupBlankLineToABlockLedByANullableDirective`, `FormatterOrganizesAReportedBlockOpenedByARegion`, `FormatterKeepsTheHeaderOfAReportedBlockLedByANullableDirectiveAtTheTop` | SHARED-USING-009; FC-013 | `VerifyFormatter` | fails now |
| RH7207 formatter `FormatterKeepsAReportedBlockBehindAConditionalGroup`, `FormatterKeepsAReportedBlockEnclosedByAConditionalGroup` | SHARED-USING-009 | `VerifyFormatterKeepsReportedShape` | passes now |

Every new test was run on 8d097e1. Each "fails now" test fails on the expected difference: `CanSafelyReorder` returns false; the formatter or organize output keeps the original order or the missing blank line; no code fix is registered; RH7207 reports no diagnostic for case 2. None fails on a compile error or an unrelated layout difference. Each "passes now" test passes. `scripts/test.sh --project architecture` passes, so the earlier failure "FMT-USING-012: no test covers it" is gone.

## Existing tests the change flips

To find every existing test whose expectation the change alters, I checked out a scratch worktree with the narrowed predicate only. The FMT-USING-012 split was not implemented there. I ran the core, formatter, analyzer, and CLI projects on it, then deleted the worktree. Its code supplied no expected value (FC-035): it only showed which tests flip. Only these tests failed:

- Core: `NullableDirectiveBeforeFirstDirectiveIsUnsafe` (LF/CRLF)
- Formatter: `UnsafeBlockOnlyGetsOneDirectivePerLine`
- Analyzer: RH7203 `BlockLedByANullableDirectiveIsReportedWithoutCodeFix`, RH7207 `BlockLedByANullableDirectiveIsReportedWithoutCodeFix`, RH7207 formatter `FormatterKeepsAReportedBlockLedByANullableDirective`
- CLI: no test failed (467 passed)

All five tests are changed as listed above. No other test in the four projects asserts an output the predicate changes. That includes the `#nullable enable` headers the skeptic named in RH3004, RH5115, RH5116, RH5201, and `AttachedPartLineStartTests` (FC-032 is covered for the predicate). The header split itself can still change formatter output for a reordered block whose leading trivia holds a directive and a blank line. Only the new FMT-USING-012 tests have that shape. On that worktree, the new tests that failed were exactly the seven formatter and seven analyzer cases where FMT-USING-012 differs from FMT-USING-009: cases 4 and 6, two directives with a blank line, comment + blank + comment + directive, and the copyright header with two directives. Every other new expectation passed there. That rules out fixture mistakes such as the region framing, the column-0 directive in a namespace, or compile errors in the fix-path fixtures.

Tests the handovers name and that stay unchanged and green: `DirectiveInLeadingTriviaOfLaterDirectiveIsUnsafe`, `DirectiveAfterLastDirectiveIsSafe`, `DirectiveAfterTheBlockDoesNotPreventReportAndFix` (RH7203/RH7207), `BlockWithPreprocessorDirectiveInCanonicalOrderIsNotReportedForAMissingBlankLine`, `BlockWithRegionDirectiveInCanonicalOrderIsNotReportedForItsBlankLines`, `FixAllOrganizesOnlyTheBlockWithoutPreprocessorDirective`, and RH7201 `BlockLedByADefineDirectiveIsReportedWithoutCodeFix` (its `#if` stands between directives).

## Not covered, and why

- **`#else` or `#elif` alone in front of the block.** They cannot be isolated. An `#else` or `#elif` in the first token's leading trivia always comes with its `#endif` or its `#if` in the same trivia: a taken branch skips through `#endif`, and an untaken one puts its `#if` there. `ElseDirectiveBeforeFirstDirectiveIsUnsafe` and `ElifDirectiveBeforeFirstDirectiveIsUnsafe` hold `#else`/`#elif` together with `#endif` only, never `#if`. The extern-alias fixtures exist only in the core tests, because an unreferenced `extern alias` is error CS0430 in the analyzer verifier.
- **FMT-USING-012 per rule.** RH7201–RH7206 get the one -004 spec example each. Every rule's fix reaches the same `UsingDirectiveCodeFixUtilities.OrganizeScopeUsingsAsync`, which all seven providers call: `rg -n "OrganizeScopeUsingsAsync" src/Reihitsu.Analyzer.CodeFixes` finds it in `UsingDirectiveOrderingCodeFixProviderBase` and the RH7207 provider. So the FMT-USING-012 trivia variants run once, on RH7203, plus Fix All on RH7203 and RH7207.
- **Tab indentation in front of a header directive** — not taken up. The header lines stay as written (FMT-USING-011), and the tab path is already covered for comment headers (`TabIsKeptInFrontOfEveryDirectiveBelowAHeader…`).
- **FC-004 / FC-019 / FC-021** are covered: the new cases put comments and directives exactly in the trivia the reorder moves. FC-006 (meaning) is covered by the unsafe cases.
- **Failure classes that cannot reach the change:**
  - FC-022–FC-026, FC-029: no literals, identifiers, members, or paired layout are involved.
  - FC-036, FC-018: no rename.
  - FC-011, FC-012: no loop or neighbor phase is changed. Idempotency and phase stability are checked by `AssertRuleResult` and `VerifyFormatter`.
  - FC-014: no new rule.
  - FC-030: the fix scope is unchanged, and `CodeFixDoesNotFormatFollowingType` stays.
  - FC-038–FC-043: no shape is declared unsupported (maintainer answer 6A).
  - FC-044: the predicate reads trivia kinds only.
  - FC-031, FC-037: production comments are the implementer's and the cleaner's.

## Commands

```shell
scripts/test.sh --project core --filter "FullyQualifiedName~Reihitsu.Core.Test.UsingDirectiveOrderingSafetyTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~UsingDirectiveHeaderDirectiveTests|FullyQualifiedName~ReihitsuFormatterUsingDirectivesTests|FullyQualifiedName~UsingDirectiveRequirementTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Ordering.RH720|FullyQualifiedName~RH7207"
scripts/test.sh --project architecture
```

After the implementation, run the core, formatter, analyzer, and CLI projects in full (FC-032).

## Follow-up candidates

_None._
