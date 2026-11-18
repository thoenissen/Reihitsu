Role: test implementer
Commit: c0414e2
Verdict: #849: tests written; #850: tests written

My prompt named the issues, the slug, and the handovers to read. It carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples. For cases with no spec example, I took the formatter's output on `main`: SHARED-GETONLY-005 makes the fix write exactly what FMT-STRUCT-014 writes, and the formatter is unchanged specified behavior, not a patched copy.

Files written:

- `src/Reihitsu.Analyzer.Test/Formatting/RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzerTests.cs`
  - Base: `BatchCodeFixTestsBase`.
  - 50 tests, plus the inherited `FixAllCorrectsEveryDiagnostic` and `CodeFixProvidesFixAllProvider`.
- `src/Reihitsu.Analyzer.Test/Formatting/RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzerTests.cs`
  - Base: `BatchCodeFixTestsBase`.
  - 18 tests, plus the same two inherited tests.
- `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH3209GetOnlyPropertiesShouldUseAnExpressionBodyFormatterTests.cs` (3 tests)
- `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH3210GetOnlyIndexersShouldUseAnExpressionBodyFormatterTests.cs` (3 tests)

The tests depend on these names, which the specs and the change doc fix:

- `RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzer` and `RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzer`, in `Reihitsu.Analyzer.Rules.Clarity`, each with a `DiagnosticId` constant.
- The matching `…CodeFixProvider` types, in `Reihitsu.Analyzer.CodeFixes.Rules.Clarity`. Each must support Fix All.
- `AnalyzerResources.RH3209MessageFormat` and `AnalyzerResources.RH3210MessageFormat`. One test per rule (`DiagnosticCarriesTheRuleMessageOnTheAccessorList`) and each Fix All scenario expects the message to be exactly that resource text, with no format arguments.

## thoenissen/Reihitsu#849

### Tests

All tests below are in the RH3209 classes. "Expected now" means on this head: the analyzer test project does not compile, because the four rule types do not exist yet (CS0246 on exactly those four names, and nothing else).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| DiagnosticCarriesTheRuleMessageOnTheAccessorList | SHARED-GETONLY-004 | `Verify(source, fixed, Diagnostics(…))` | fails: types missing |
| GetOnlyPropertiesAreReportedAndConverted | SHARED-GETONLY-001, SHARED-GETONLY-004, SHARED-GETONLY-005, RH3209-001 | `Verify` | fails: types missing |
| FixWritesTheFormattersExpressionBody | SHARED-GETONLY-005 | `Verify` | fails: types missing |
| FixKeepsTheSignatureAsWritten | SHARED-GETONLY-005 (FC-030) | `Verify` | fails: types missing |
| FixKeepsDocumentationAttributesAndCommentsInFrontOfTheAccessorList | SHARED-GETONLY-005 (FC-030) | `Verify` | fails: types missing |
| FixJoinsTheArrowOntoTheSignaturesLine | SHARED-GETONLY-005 | `Verify` | fails: types missing |
| FixParenthesizesAThrownConditionalExpression | SHARED-GETONLY-005 | `Verify` | fails: types missing |
| FixLeavesAThrownCoalescingExpressionBare | SHARED-GETONLY-005 | `Verify` | fails: types missing |
| FixOfAMultiLineExpressionMatchesTheFormatter | SHARED-GETONLY-005 (FC-027) | `VerifyCodeFixMatchesFormatter` | fails: types missing |
| CommentBehindOnlyOneOfSemicolonAndBraceFollowsTheNewSemicolon | SHARED-GETONLY-003, SHARED-GETONLY-005 | `Verify` | fails: types missing |
| CommentBehindTheReturnStatementFollowsTheNewSemicolon | SHARED-GETONLY-003, SHARED-GETONLY-005 | `Verify` | fails: types missing |
| DelimitedCommentTrailingTheExpressionStaysBehindIt | SHARED-GETONLY-005 (see *Reading to confirm*) | `Verify` | fails: types missing |
| FixKeepsCarriageReturnLineFeedLineEndings | SHARED-GETONLY-005 (FC-010) | `VerifyCodeFixWithEndOfLine` | fails: types missing |
| FixKeepsLineFeedLineEndings | SHARED-GETONLY-005 (FC-010) | `VerifyCodeFixWithEndOfLine` | fails: types missing |
| InterfaceStaticAndExplicitImplementationPropertiesAreReported | RH3209-001, SHARED-GETONLY-001 | `Verify` | fails: types missing |
| StaticVirtualInterfacePropertyIsReportedAndConverted | RH3209-001 (FC-024) | `Verify` | fails: types missing |
| PropertiesInEveryContainerAreReportedAndConverted (override, nested, record, record struct, `readonly` property) | RH3209-001 (FC-024) | `Verify` | fails: types missing |
| ImplementingPartOfAPartialPropertyIsReportedAndConverted | RH3209-001 (FC-024) | `Verify` | fails: types missing |
| ExtensionBlockPropertyIsReportedAndConverted | RH3209-001 (FC-024) | `Verify` | fails: types missing |
| FieldKeywordGetterWithoutInitializerIsReportedAndConverted | SHARED-GETONLY-001 (FC-006) | `Verify` | fails: types missing |
| IndexersAreNotReported | RH3209-001 (FC-025) | `Verify` (analyzer) | fails: types missing |
| UnderCSharp6OnlyAReturningGetterIsReported | SHARED-GETONLY-001 (FC-006) | `Verify` + C# 6 | fails: types missing |
| UnderCSharp7AThrowingGetterIsReported | SHARED-GETONLY-001 (FC-006) | `Verify` + C# 7 | fails: types missing |
| UnderCSharp5NothingIsReported | SHARED-GETONLY-001 (FC-006) | `Verify` + C# 5 | fails: types missing |
| MembersWhoseShapeTheFormatterKeepsAreNotReported (spec example) | SHARED-GETONLY-002 | `Verify` | fails: types missing |
| GetterNextToInitAccessorIsNotReported | SHARED-GETONLY-002 | `Verify` | fails: types missing |
| BlockGetterWithAttributeIsNotReported | SHARED-GETONLY-002 | `Verify` | fails: types missing |
| GetterWithAStatementWithoutExpressionFormIsNotReported (`yield return`; `try` with a rethrow) | SHARED-GETONLY-002 | `Verify` | fails: types missing |
| PropertyWithInitializerIsNotReported (`{ get; } = 1`; a `field` block getter with an initializer) | SHARED-GETONLY-002 | `Verify` | fails: types missing |
| MembersWithCommentsTheFormatterKeepsAreReportedWithoutFix (spec example A, B, D, E) | SHARED-GETONLY-003 | `VerifyNoCodeFixIsOffered` | fails: types missing |
| CommentBehindOpeningBraceWithholdsFix (`//` and `/* */`) | SHARED-GETONLY-003 (FC-004) | `VerifyNoCodeFixIsOffered` | fails: types missing |
| CommentInFrontOfTheGettersSemicolonWithholdsFix (`// c` with `;` on the next line) | SHARED-GETONLY-003 | `VerifyNoCodeFixIsOffered` | fails: types missing |
| CommentInFrontOfTheClosingBraceWithholdsFix | SHARED-GETONLY-003 | `VerifyNoCodeFixIsOffered` | fails: types missing |
| DelimitedCommentsBehindSemicolonAndBraceWithholdFix | SHARED-GETONLY-003 | `VerifyNoCodeFixIsOffered` | fails: types missing |
| CommentBehindGetKeywordOfABlockGetterWithholdsFix | SHARED-GETONLY-003 | `VerifyNoCodeFixIsOffered` | fails: types missing |
| CommentBehindReturnWithholdsFix | SHARED-GETONLY-003 | `VerifyNoCodeFixIsOffered` | fails: types missing |
| SingleLineCommentInFrontOfTheReturnSemicolonWithholdsFix | SHARED-GETONLY-003 | `VerifyNoCodeFixIsOffered` | fails: types missing |
| CommentInFrontOfTheGetterBlocksClosingBraceWithholdsFix | SHARED-GETONLY-003 | `VerifyNoCodeFixIsOffered` | fails: types missing |
| CommentsBehindReturnStatementAndGetterBlockBraceWithholdFix | SHARED-GETONLY-003 | `VerifyNoCodeFixIsOffered` | fails: types missing |
| GetterTheFormatterRefusesIsReportedWithoutFix (spec example) | SHARED-GETONLY-006 | `VerifyNoCodeFixIsOffered` | fails: types missing |
| ConditionalGroupBehindTheArrowIsReportedWithoutFix | SHARED-GETONLY-006 (FC-005) | `VerifyNoCodeFixIsOffered` | fails: types missing |
| SingleLineCommentInFrontOfTheArrowIsReportedWithoutFix | SHARED-GETONLY-006 | `VerifyNoCodeFixIsOffered` | fails: types missing |
| RefusedShapeInsideTheExpressionIsReportedWithoutFix (`_value + /* c */ 1`) | SHARED-GETONLY-006 | `VerifyNoCodeFixIsOffered` | fails: types missing |
| RefusedShapeInTheSignatureIsReportedWithoutFix (`[Obsolete(true ? /* c */ "a" : "b")]`, the SHARED-FIX-010 side) | SHARED-GETONLY-006 | `VerifyNoCodeFixIsOffered` | fails: types missing |
| MembersWithDirectivesTheFormatterKeepsAreReportedWithoutFix (spec example, without `WRITABLE`) | SHARED-GETONLY-007 | `VerifyNoCodeFixIsOffered` | fails: types missing |
| MemberWithActiveSetterInConditionalGroupIsNotReported (with `WRITABLE`) | SHARED-GETONLY-007, SHARED-GETONLY-002 | `VerifyWithPreprocessorSymbols` | fails: types missing |
| RegionInsideTheAccessorListIsReportedWithoutFix | SHARED-GETONLY-007 (FC-020) | `VerifyNoCodeFixIsOffered` | fails: types missing |
| PragmaInFrontOfTheGetterIsReportedWithoutFix | SHARED-GETONLY-007 (FC-020) | `VerifyNoCodeFixIsOffered` | fails: types missing |
| DisabledTextInFrontOfTheAccessorListIsReportedWithoutFix | SHARED-GETONLY-007 (FC-005) | `VerifyNoCodeFixIsOffered` | fails: types missing |
| MemberWithDirectiveAndCommentIsReportedWithoutFix | SHARED-GETONLY-007 | `VerifyNoCodeFixIsOffered` | fails: types missing |
| ConditionalGroupInsideTheExpressionIsReportedAndFixedAsTheFormatterFormats (spec example) | SHARED-GETONLY-007, SHARED-GETONLY-005 | `VerifyCodeFixMatchesFormatter` | fails: types missing |
| NoFixIsOfferedInAFileWithASyntaxError | SHARED-FIX-011 | `VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded` | fails: types missing |
| FixAllCorrectsEveryDiagnostic (inherited; scenario has 3 properties, one in a nested type, and an indexer the rule leaves alone) | SHARED-FIX-001, SHARED-FIX-002 (base tags; FC-008) | `BatchCodeFixTestsBase` | fails: types missing |
| VerifyFormatterFixesViolation (formatter test) | SHARED-GETONLY-001 | `FormatterTestsBase.VerifyFormatter(source, fixed)` (LF, CRLF, second pass) | fails: types missing |
| VerifyFormatterKeepsPropertiesWithCommentsAndDirectives (formatter test) | SHARED-GETONLY-003, SHARED-GETONLY-007 | `VerifyFormatterKeepsReportedShape` | fails: types missing |
| VerifyFormatterKeepsUnreportedProperties (formatter test) | SHARED-GETONLY-002 | `VerifyFormatter(source)` | fails: types missing |

How I watched the tests fail for the right reason: in a throwaway `git worktree` of this head (since removed), I added the test files and ran them twice.

1. **Stub types that report nothing and fix nothing.** The test project compiled.
   - 16 tests passed: every "not reported" test and `CodeFixProvidesFixAllProvider`.
   - 65 failed. All but one failed on a missing diagnostic. The exception was a getter with a `readonly` modifier, which hit CS8664 (see below).
2. **A crude analyzer prototype** (a single `get`, no attributes or modifiers, no initializer, C# 6/7 gates, comments ignored) **with a fix that registers nothing.**
   - 43 passed. That includes every `VerifyNoCodeFixIsOffered` test, so the markup spans match the accessor list, and all four formatter tests, so the formatter keeps and clears what the tests say.
   - 38 failed, each with "Iterative code fix application", "No code fix action was registered", or "the code fix must be offered on the source without the syntax error". That is, they failed only because no fix existed.
   - The remaining failure was the CS8664 test, which I then deleted.

Separately, I ran `scripts/format.sh --force` on the plain source of every `Verify(source, fixedSource)` pair (30 pairs) and compared the result with the expected value. Every expected fix result equals the formatter's output, with these deliberate exceptions:

- the two `FixKeepsTheSignatureAsWritten` tests and `FixKeepsDocumentationAttributesAndCommentsInFrontOfTheAccessorList`, where the signature stays as written (SHARED-GETONLY-005);
- the C# 6 tests, which the CLI formatted at its default language version.

Existing tests whose asserted output this change alters: none.

- The change doc changes no formatter output and no other rule's output.
- `rg "FMT-STRUCT-01[45]|GetOnlyMemberExpressionBody"` over the test projects finds `src/Reihitsu.Formatter.Test/Regression/Structural/GetOnlyMemberExpressionBodyTests.cs` and `EmptyTypeDeclarationSemicolonLanguageVersionAfterEarlierTransformTests.cs`. Both must stay green unchanged through the Core move. I left them untouched.
- No other test file names RH3202 to RH3208 outside their own classes, so no catalog test lists the rules by ID.

#### Reading to confirm (SHARED-GETONLY-003 versus SHARED-GETONLY-005)

`DelimitedCommentTrailingTheExpressionStaysBehindIt` asserts that the following is reported, and that the fix writes `public int A => _value /* c */;` and `public int B => _value /* d */;`:

```csharp
public int A { get => _value /* c */; }
public int B { get { return _value /* d */; } }
```

On this head the formatter converts both members exactly so (scripts/format.sh). SHARED-GETONLY-003 lists "in front of the getter's semicolon" as a place where a comment withholds the fix, but it qualifies the list with "trivia the conversion would delete or join onto the signature's line". A `/* */` comment on the expression's own line is trailing trivia of the expression: the conversion keeps it in place and neither deletes it nor joins it. A comment on a line of its own in front of `;` does keep the list, and `CommentInFrontOfTheGettersSemicolonWithholdsFix` and `SingleLineCommentInFrontOfTheReturnSemicolonWithholdsFix` cover that case.

The test follows the formatter's reading, which SHARED-GETONLY-005 demands. If the lead reads SHARED-GETONLY-003 the other way, that is two readings giving different output, so the expected value of this one test is the lead's to decide.

### Not covered, and why

- **A getter with a modifier (SHARED-GETONLY-002).**
  - A modifier on the only accessor of a property does not compile. `readonly get` alone gives CS8664, observed, and an access modifier needs both accessors.
  - So every compiling input with a modified getter also has a second accessor, which the init and set tests already cover. No test is written for the modifier alone.
- **`return ref` (SHARED-GETONLY-001 example E)** is covered in `GetOnlyPropertiesAreReportedAndConverted`. The issue's `throw` with a lambda or query operand is not tested separately: FMT-STRUCT-011 is the formatter's, and the fix takes it from the shared conversion. The conditional operand (wrapped) and `??` (bare) cover both sides of that condition.
- **RH5401 on the fix result (FC-007).**
  - Every fix test asserts exact text in which `=>` and the first expression token stay on the signature's line.
  - The verifier re-analyzes with RH3209 only, and no helper runs another analyzer on a fix result.
- **FC-033, CRLF on the formatter surface.** `VerifyFormatter` and `VerifyFormatterKeepsReportedShape` run LF and CRLF themselves.
- **SHARED-FIX-012 (hiding carrier).** `ReihitsuCodeFixProviderBase` handles it for every fix, and its own tests cover that. It is not in SHARED-GETONLY's `Uses`.
- **Partial properties (SHARED-DIAG-004 and SHARED-DIAG-012).** Only the implementing part has a body, so only one part can be reported. The test asserts that one diagnostic and does not tag those requirements, which the spec does not use.
- **`GetOnlyMemberExpressionBodyRewriteUtilities` in `Reihitsu.Core`.** I wrote no Core unit tests: the type's API is not specified, and the analyzer, fix, and formatter tests above reach it through every caller. The existing formatter regression tests guard the formatter's side of the move.
- **Failure classes that do not apply:**
  - The naming and Renamer classes: the fix renames nothing.
  - Token-gap joins inside list elements: the only join is the arrow onto the signature's line. `FixJoinsTheArrowOntoTheSignaturesLine` covers it, and the comment and directive in that gap are covered by `MembersWithCommentsTheFormatterKeepsAreReportedWithoutFix` (A) and `DisabledTextInFrontOfTheAccessorListIsReportedWithoutFix`.
  - FC-038 to FC-043: the run declares no shape unsupported (Q5).

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3209"
```

### Follow-up candidates

_None_ beyond the one in `00-lead.md`.

## thoenissen/Reihitsu#850

### Tests

All tests below are in the RH3210 classes. "Expected now" has the same meaning as under #849.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| DiagnosticCarriesTheRuleMessageOnTheAccessorList | SHARED-GETONLY-004 | `Verify(source, fixed, Diagnostics(…))` | fails: types missing |
| GetOnlyIndexersAreReportedAndConverted (spec example) | RH3210-001, SHARED-GETONLY-001, SHARED-GETONLY-002, SHARED-GETONLY-004, SHARED-GETONLY-005 | `Verify` | fails: types missing |
| InterfaceAndExplicitImplementationIndexersAreReportedAndConverted | RH3210-001, SHARED-GETONLY-002 | `Verify` | fails: types missing |
| RefAndParamsIndexersAreReportedAndConverted (record, nested type) | RH3210-001 (FC-024) | `Verify` | fails: types missing |
| FixKeepsTheSignatureAsWritten | SHARED-GETONLY-005 (FC-030) | `Verify` | fails: types missing |
| CommentBehindTheClosingBraceFollowsTheNewSemicolon | SHARED-GETONLY-003, SHARED-GETONLY-005 | `Verify` | fails: types missing |
| FixKeepsCarriageReturnLineFeedLineEndings | SHARED-GETONLY-005 (FC-010) | `VerifyCodeFixWithEndOfLine` | fails: types missing |
| FixKeepsLineFeedLineEndings | SHARED-GETONLY-005 (FC-010) | `VerifyCodeFixWithEndOfLine` | fails: types missing |
| PropertiesAreNotReported | RH3210-001 (FC-025) | `Verify` (analyzer) | fails: types missing |
| UnderCSharp6OnlyAReturningGetterIsReported | SHARED-GETONLY-001 (FC-006) | `Verify` + C# 6 | fails: types missing |
| UnderCSharp5NothingIsReported | SHARED-GETONLY-001 (FC-006) | `Verify` + C# 5 | fails: types missing |
| IndexersWhoseShapeTheFormatterKeepsAreNotReported (init, attribute, two statements) | SHARED-GETONLY-002 | `Verify` | fails: types missing |
| IndexersWithCommentsTheFormatterKeepsAreReportedWithoutFix | SHARED-GETONLY-003 | `VerifyNoCodeFixIsOffered` | fails: types missing |
| IndexerGetterTheFormatterRefusesIsReportedWithoutFix | SHARED-GETONLY-006 | `VerifyNoCodeFixIsOffered` | fails: types missing |
| IndexersWithDirectivesTheFormatterKeepsAreReportedWithoutFix | SHARED-GETONLY-007 | `VerifyNoCodeFixIsOffered` | fails: types missing |
| IndexerWithActiveSetterInConditionalGroupIsNotReported | SHARED-GETONLY-007 | `VerifyWithPreprocessorSymbols` | fails: types missing |
| ConditionalGroupInsideTheExpressionIsReportedAndFixedAsTheFormatterFormats | SHARED-GETONLY-007, SHARED-GETONLY-005 | `VerifyCodeFixMatchesFormatter` | fails: types missing |
| NoFixIsOfferedInAFileWithASyntaxError | SHARED-FIX-011 | `VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded` | fails: types missing |
| FixAllCorrectsEveryDiagnostic (inherited; 3 indexers, one in a nested type, and a property the rule leaves alone) | SHARED-FIX-001, SHARED-FIX-002 (base tags; FC-008) | `BatchCodeFixTestsBase` | fails: types missing |
| VerifyFormatterFixesViolation (formatter test) | RH3210-001 | `VerifyFormatter(source, fixed)` | fails: types missing |
| VerifyFormatterKeepsIndexersWithCommentsAndDirectives (formatter test) | SHARED-GETONLY-003, SHARED-GETONLY-007 | `VerifyFormatterKeepsReportedShape` | fails: types missing |
| VerifyFormatterKeepsUnreportedIndexers (formatter test) | SHARED-GETONLY-002 | `VerifyFormatter(source)` | fails: types missing |

These went through the same stub and prototype checks as #849, with the same outcome: each failed only because no diagnostic or no fix existed. Their expected values match the formatter, except for the signature-kept and C# 6 cases.

### Not covered, and why

- **The C# 7 gate for a throwing indexer.** It shares the throw gate with properties: SHARED-GETONLY-001 states one rule for both kinds. The C# 6 test does cover that the throwing indexer is not reported.
- **An indexer in an `extension` block.** C# 14 extension blocks have no indexers, so no caller can produce one.
- **Static indexers.** They do not exist.
- **Initializers.** Indexers have none, so that axis does not apply.
- **The remaining SHARED-GETONLY-003, -006, and -007 seams.** They are tested once on properties (above). The indexer tests cover one representative per requirement. I have not proved that both rules share one call site, because the shared type does not exist yet. The implementer's shared Core owner (Q6 A) is meant to make it so, and the auditor can check that.

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3210"
```

### Follow-up candidates

_None._

## Whole run

- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3209|FullyQualifiedName~RH3210"` on this head fails to build. It gives CS0246 for the four rule types and no other error.
- `scripts/test.sh --project architecture` on this head also fails to build, because it builds the analyzer test project. In the stub worktree it gave 197 passed and 1 failed.
  - The failure was `EveryCodeOwnerNamesAnExistingType`, because the Core type is missing.
  - `EveryRequirementOfAnEnforcedSpecIsCovered` passes: every SHARED-GETONLY, RH3209, and RH3210 requirement is now tagged.
- All four files were formatted with `scripts/format.sh`. Each starts with a UTF-8 BOM, like its neighbours.
- `RuleDocumentationExampleTests` and `AnalyzerPackageMetadataTests` cover the rule pages and the package README row once the rules exist. They are the implementer's to keep green, and I wrote no extra test for them.
