Role: test implementer
Commit: 4c142e9
Verdict: #353: tests written; #354: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples and requirements (`shared/line-count.md`, RH2008, RH2111, RH0001-004/007, RH8402-001, change doc *Visible output changes* 4–7), counted by hand under SHARED-LINES-001; none was taken from code.

**How the tests were watched.** The analyzer test project does not compile on this head: `RH2008MethodsShouldNotExceedTheLineLimitAnalyzer` and `RH2111TypesShouldNotExceedTheLineLimitAnalyzer` (namespace `Reihitsu.Analyzer.Rules.Design`, each with `public const string DiagnosticId`) do not exist. To watch every test fail for the right reason, I built a throw-away worktree (scratchpad, deleted afterwards, nothing of it committed) with two stub analyzers of those names that declare their ID and report nothing. Results there:

- RH2008 and RH2111 classes: 172 test cases, 43 passed (every "not reported" case), 129 failed, every one with `Mismatch between number of diagnostics returned, expected "N" actual "0"` — so no test source produces a compiler error (the union cases with `UnionSupport` included), and every failure is the missing diagnostic.
- RH0001/RH8402/`RuleDocumentationExampleTests`: 46 failed, all new: the RH0001 cases fail because the loader reports "Unknown configuration section 'design'." at (1,4) instead of the expected output; `ConfigurationWithValidDesignSection` (RH8402) expects 1 diagnostic and gets 0. Every pre-existing test there passed, the rule-page example tests included.
- `AnalyzerViolationReportTests`: 2 failed as expected (below), 5 passed.
- `scripts/test.sh --project architecture`: 198 passed with the stubs present — `EveryRequirementOfAnEnforcedSpecIsCovered` finds a tagged test for every requirement of SHARED-LINES, RH2008, RH2111, and RH0001-007.

Messages are asserted as the literal text of the specs ("This member has {count} lines of code, which exceeds the limit of {limit}.", "This type has …"), not through a resource name, and every reported case asserts the exact count through the message, so a count off by one fails even where the limit is crossed either way.

Test-code recognition is passed as a global analyzer config, `test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", "is_global = true\n<lines>\n"))`, the lines being `build_property.IsTestProject = …` and/or `build_property.IsTestingPlatformApplication = …` as MSBuild writes them (an empty property as `key = `).

## thoenissen/Reihitsu#353

### Tests

All in `src/Reihitsu.Analyzer.Test/Design/RH2008MethodsShouldNotExceedTheLineLimitAnalyzerTests.cs` (helper: `Verify` of `AnalyzerTestsBase<RH2008…Analyzer>`, through the local `VerifyLines(source, configuration, globalConfiguration, expected)`), unless named otherwise. "Expected now" is on this head: the project does not compile; with the analyzer present each test passes only if it behaves as stated.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `TriviaLinesAreNotCountedAndVerbatimStringLinesAre` (SHARED-LINES-001 example 1, limit 3 → 4 lines) | SHARED-LINES-001; FC-019/020/021/022 | `Verify` | compile error; then reported 4/3 |
| `TriviaExampleAtTheLimitIsNotReported` (example 2, 3 lines at 3) | SHARED-LINES-001; FC-003 | `Verify` | not reported |
| `LineIncludedByDefinedSymbolIsCounted` (`FEATURE` defined → 5) | SHARED-LINES-001; SHARED-DIAG-008 other side | `Verify` + preprocessor transform | reported 5/3 |
| `CountIsTheSameForEveryLineEnding` ×3 (CRLF, LF, CR alone) | SHARED-LINES-001; FC-033 | `Verify` | reported 4/3 each |
| `EveryLineOfRawStringLiteralIsCounted` (example 3, 5 at 4) | SHARED-LINES-001; FC-022 | `Verify` | reported 5/4 |
| `RawStringLiteralAtTheLimitIsNotReported` (5 at 5) | SHARED-LINES-001, SHARED-LINES-002; FC-003 | `Verify` | not reported |
| `BlankLineInsideVerbatimStringIsCounted` (3) | SHARED-LINES-001; FC-022 | `Verify` | reported 3/1 |
| `EveryLineOfInterpolatedRawStringWithMultiLineHoleIsCounted` (4) | SHARED-LINES-001; FC-022 | `Verify` | reported 4/1 |
| `CommentAndDirectiveLookingTextInsideLiteralIsCounted` (`"// x"`, `#if X` and `/* y */` inside a verbatim string → 5) | SHARED-LINES-001; FC-022, FC-037 | `Verify` | reported 5/1 |
| `MultiLineBlockCommentBetweenTokensCountsOnlyItsLinesWithTokens` (2) | SHARED-LINES-001; FC-021 | `Verify` | reported 2/1 |
| `RegionAndPragmaLinesAreNotCounted` (2) | SHARED-LINES-001; FC-020 | `Verify` | reported 2/1 |
| `InactiveIfFalseBranchIsNotCountedAndActiveElseBranchIs` (2) | SHARED-LINES-001; FC-020 | `Verify` | reported 2/1 |
| `DocumentationCommentInsideBodyIsNotCountedInEveryDocumentationMode` ×3 (None, Parse, Diagnose; `///` and `/** */`; local 2, member 5) | SHARED-LINES-001; FC-040 | `Verify` + parse-options transform | reported 2/1 and 5/1 |
| `ConfiguredLimitReportsOnlyTheBodyAboveIt` (SHARED-LINES-002 example) | SHARED-LINES-002, RH2008-001; FC-003 | `Verify` | `N` reported 3/2, `M` not |
| `ProblemElsewhereDoesNotKeepValidLimitFromTakingEffect` ×5 (`"formatting": 1`, invalid `maximumTypeLines`, invalid `maximumTestMethodLines`, unknown `maximumFileLines`, invalid `copyright`) | SHARED-LINES-002 | `Verify` | `N` reported 3/2 |
| `DefaultProductionLimitAppliesWithoutValidSetting` ×8 (`{ }`, `{ "design": { } }`, `0`, `"2"`, empty file, `{ } junk`, test setting only, `"design": [ ]`) | SHARED-LINES-002, RH2008-001 | `Verify` | not reported |
| `DefaultProductionLimitAppliesWithoutConfigurationFile` | SHARED-LINES-002, RH2008-001 | `Verify` | not reported |
| `ProductionSettingDoesNotApplyToTestCode` / `TestSettingAppliesToTestCode` (test-project part of the example) | SHARED-LINES-002, RH2008-001 | `Verify` + global config | not reported / `N` reported 3/2 |
| `DefaultProductionLimitIsSixty` (60 not, 61 reported, no file) | RH2008-001, SHARED-LINES-002; FC-003 | `Verify` | reported 61/60 |
| `DefaultTestLimitIsOneHundred` (100 not, 101 reported, `IsTestProject`) | RH2008-001, SHARED-LINES-002; FC-003 | `Verify` | reported 101/100 |
| `ConfiguredProductionSettingLeavesTestDefault` (`maximumMethodLines` 40, test project: 70 not, 101 reported) | RH2008-001, SHARED-LINES-002 (15A) | `Verify` | reported 101/100 |
| `ConfiguredTestSettingLeavesProductionDefault` (`maximumTestMethodLines` 30, production: 60 not, 61 reported) | SHARED-LINES-002, RH2008-001 (15A) | `Verify` | reported 61/60 |
| `ProductionSettingReportedByRH0001KeepsDefault` (`"40"`, 70 lines) | RH2008-001, SHARED-LINES-002 (16A) | `Verify` | reported 70/60 |
| `InvalidSettingKeepsDefaultProductionLimit` ×13 (`"40"`, `0`, `-5`, `50.0`, `40.5`, `5e1`, `null`, `true`, `2147483648`, empty, white space, `{ } junk`, truncated JSON; 60 not, 61 reported) | SHARED-LINES-002, RH2008-001 (16A) | `Verify` | reported 61/60 |
| `InvalidTestSettingKeepsDefaultTestLimit` ×3 (`0`, `"40"`, `{ } junk`; test project) | SHARED-LINES-002, RH2008-001 (16A) | `Verify` | reported 101/100 |
| `LargestValidLimitTakesEffect` (2147483647) | SHARED-LINES-002 | `Verify` | not reported |
| `MethodInTestProjectIsJudgedAgainstTestLimit` ×11 (each property `true` alone, with the other `false` or empty, both `true`, `True`/`TRUE` for each) | SHARED-LINES-003 (12D, both directions) | `Verify` + global config | not reported |
| `MethodOutsideTestProjectIsJudgedAgainstProductionLimit` ×13 (global config without either key, both `false`, both empty, each `false`/empty/`yes`/`1` alone, `IsTestApplication`/`TestProject` = `true`) | SHARED-LINES-003 | `Verify` + global config | reported 3/2 |
| `MethodWithoutGlobalOptionsIsProductionCode` (no global config at all) | SHARED-LINES-003 | `Verify` | reported 3/2 |
| `EveryFileOfTestProjectIsTestCode` (helper type in a second file, `IsTestingPlatformApplication`) | SHARED-LINES-003 | `Verify` | not reported |
| `MembersWithBodyAreJudgedByTheirBody` (RH2008-002 example 1: ctor, `Sum`, `get`, `+` reported 3/2; `Short` not) | RH2008-002, RH2008-005 | `Verify` | 4 diagnostics |
| `SignatureAndBracesAreNotCounted` (example 2) / `SignatureExampleIsReportedBelowItsCount` (type parameters, constraint, limit 1 → 2) | RH2008-002 | `Verify` | not reported / reported 2/1 |
| `ConstructorInitializerIsNotCounted` | RH2008-002 | `Verify` | not reported |
| `TokensOnTheLinesOfTheBracesAreCounted` (2) / `OneLineBodyAndEmptyBodyAreAtOrBelowLimitOfOne` | RH2008-002, SHARED-LINES-001 | `Verify` | reported 2/1 / not reported |
| `MethodWithBlockBodyIsJudged`, `AsyncMethodIsJudged` (`Task<T>`, `ValueTask<T>` expression body), `FinalizerIsJudged`, `ConversionOperatorIsJudged` (implicit, explicit), `SetAndInitAccessorsAreJudged`, `EventAccessorsAreJudged` | RH2008-002 (+RH2008-005 for accessors); FC-025 | `Verify` | each reported 2/1 |
| `ExpressionBodiedMembersAreJudged` (ctor, finalizer, method, unary operator, conversion, `get =>`, `set =>`), `ExpressionBodiedIndexerIsJudged` | RH2008-002; FC-025 | `Verify` | each reported 2/1 |
| `SemicolonAloneBehindExpressionBodyIsNotCounted` (`=>` and `;` on lines of their own) / `OneLineExpressionBodyIsAtTheLimitOfOne` | RH2008-002; FC-025 | `Verify` | reported 2/1 / not reported |
| `MemberOfExtensionBlockIsJudged`, `ExplicitInterfaceImplementationIsJudged`, `InterfaceMemberWithDefaultImplementationIsJudged` (+ `static abstract` without body), `MembersOfEveryTypeKindAreJudged` (generic, struct, record, `file`) | RH2008-002 (+RH2008-004, RH2008-005); FC-024 | `Verify` | each reported 2/1 |
| `NestedFunctionsCountTowardTheMember` (RH2008-003 example, 10 at 3) / `LocalFunctionIsJudgedOnItsOwn` (at 2: 10/2 and 3/2) | RH2008-003 (+RH2008-005); FC-003 | `Verify` | as stated |
| `LambdaInFieldInitializerIsNotJudged` (example 2) / `LambdaOutsideEveryBodyIsNotJudged` (property initializer, anonymous method in field initializer, lambda in `: base(…)`) | RH2008-003 | `Verify` | not reported |
| `LocalFunctionInLocalFunctionCountsTowardBoth` (8, 5, 2), `FunctionsInsideLambdasCountTowardTheMember` (18 and 2), `LocalFunctionInAccessorCountsTowardTheAccessor` (6 and 2) | RH2008-003; FC-027 | `Verify` | as stated |
| `LocalFunctionInTopLevelStatementsIsJudged` (console application) | RH2008-002, RH2008-003; FC-027 | `Verify`, `OutputKind.ConsoleApplication` | reported 2/1 |
| `MembersWithoutBodyAreNotReported` (RH2008-004 example 1) / `AccessorsAreNotSummed` (example 2) / `AccessorsOverTheLimitAreReportedOnTheirKeywords` | RH2008-004 (+RH2008-005) | `Verify` | partial `M` reported 2/1 / none / `get`, `set` 3/2 |
| `ExternAbstractAndInterfaceMembersAreNotReported` | RH2008-004 | `Verify` | not reported |
| `PartialMembersAreJudgedByTheImplementingPart` (partial property, constructor, event) | RH2008-004; FC-024 | `Verify` | 3 diagnostics on the implementing parts |
| `MembersWithoutIdentifierAreReportedOnTheirNamingToken` (RH2008-005 example), `EqualityOperatorIsReportedOnItsToken`, `VerbatimIdentifierIsReportedOnTheWholeToken` | RH2008-005; FC-023 | `Verify` | as stated |
| `src/Reihitsu.Analyzer.Test/Analyzer/RH0001ConfigurationFileMustBeValidAnalyzerTests.cs`: `DesignSectionWithEveryLineLimitIsValid`, `EmptyDesignSectionIsValid`, `DesignSectionNextToOtherSectionsIsValid` | RH0001-004, RH0001-007 | `VerifyConfiguration` | fail now ("Unknown configuration section 'design'."); then no diagnostic |
| RH0001: `LineLimitAtTheBoundsOfTheRangeIsValid` ×8 (1 and 2147483647 for each setting), `TestLimitIsCheckedIndependentlyOfProductionLimit` ×4 | RH0001-007; FC-003 | `VerifyConfiguration` | fail now; then no diagnostic |
| RH0001: `InvalidDesignSettingsAreReported` (spec example: (1,37), (1,64), (1,94), unknown setting at (1,98)) | RH0001-002, RH0001-007 | `VerifyConfiguration` | fail now; then 4 diagnostics |
| RH0001: `NonWholeNumberValueIsReported` ×17 (`0`, `-1`, `-60`, `2.5`, `50.0`, `1.0`, `1e2`, `5e1`, `5E1`, `1e+2`, `2147483648`, a 20-digit number, `null`, `true`, `false`, `[ 50 ]`, `{ }`; at (1,37)), `StringValueIsReported` ×2 (`"50"`, `"fifty"`; at (1,38)), `InvalidValueIsReportedForEverySetting` ×4 | RH0001-007; FC-003 | `VerifyConfiguration` | fail now; then the message on the value |
| RH0001: `UnknownDesignSettingIsReported` ×3 (`maximumFileLines`, `MaximumMethodLines`, `allowedNamespaceDeclarations`; at (1,16)) | RH0001-007 | `VerifyConfiguration` | fail now; then "Unknown configuration setting 'design.…'." |
| RH0001: `DesignSectionMustBeObject` ×3 (`1`, `[ ]`, `null`; at (1,13)) | RH0001-004 | `VerifyConfiguration` | fail now; then "The 'design' section must be a JSON object." |
| `src/Reihitsu.Analyzer.Test/Documentation/RH8402…AnalyzerTests.cs`: `ConfigurationWithValidDesignSection` | RH8402-001 (change doc change 7) | `VerifySpecificationFile` | fail now (silenced); then reported at the start of the file |
| RH8402: `ConfigurationWithInvalidDesignSetting` (`maximumMethodLines: 0` next to `copyright`) | RH8402-001 | `VerifySpecificationFile` | passes now and after (a problem still silences RH8402) |

Locations in RH0001 follow the loader's existing convention, which the RH0001 spec does not spell out beyond "on the value"/"on its property name": a property name and a string value are located after their opening quote, every other value on its first character (the existing `naming`/`copyright` tests locate the same way).

### Not covered, and why

- An unreadable configuration file (SHARED-LINES-002 names it): the verifier's additional files are in-memory texts and cannot fail to read.
- A value with surrounding white space (`build_property.IsTestProject =  true `) and the letter case of the key: SHARED-LINES-003 does not decide them (05-spec-author.md: a value MSBuild writes carries no white space).
- The package's `buildTransitive/Reihitsu.Analyzer.targets` (`CompilerVisibleProperty` items, SHARED-LINES-003's second sentence): no test project builds a package; the auditor checks the file by hand (05-spec-author.md).
- FC-004 to FC-012, FC-018, FC-026, FC-028 to FC-030, FC-036, FC-038 to FC-043: no code fix, no formatter phase, no rewrite, no unsupported shape in this change. FC-013/FC-014: the rules judge size, not layout (02-skeptic.md, *Formatter interaction*); no formatter parity test.
- A duplicate key in the `design` section: follow-up candidate already in `00-lead.md`; no requirement.
- Static abstract interface operators implemented explicitly (`static C I<C>.operator +`): the explicit method and accessor cases cover the explicit-implementation location; an operator's location is its token whatever its qualifier.

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2008MethodsShouldNotExceedTheLineLimitAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH0001ConfigurationFileMustBeValidAnalyzerTests|FullyQualifiedName~RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzerTests|FullyQualifiedName~RuleDocumentationExampleTests"
```

### Follow-up candidates

_None._

## thoenissen/Reihitsu#354

### Tests

All in `src/Reihitsu.Analyzer.Test/Design/RH2111TypesShouldNotExceedTheLineLimitAnalyzerTests.cs`, same helpers as under #353.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `DefaultProductionLimitIsFiveHundred` (500 not, 501 reported) / `DefaultTestLimitIsOneThousand` (1000 not, 1001 reported) | RH2111-001, SHARED-LINES-002; FC-003 | `Verify` | compile error; then 501/500 / 1001/1000 |
| `ConfiguredProductionSettingLeavesTestDefault` (`maximumTypeLines` 400, `IsTestingPlatformApplication`: 700 not, 1001 reported) / `ConfiguredTestSettingLeavesProductionDefault` (`maximumTestTypeLines` 300: 501 reported) | RH2111-001, SHARED-LINES-002 (15A) | `Verify` | as stated |
| `InvalidSettingKeepsDefaultProductionLimit` ×7 (`0` from the spec example, `"400"`, `400.0`, `4e2`, `null`, empty, `{ } junk`) / `InvalidTestSettingKeepsDefaultTestLimit` | RH2111-001, SHARED-LINES-002 (16A) | `Verify` | 501/500 / 1001/1000 |
| `ConfiguredLimitReportsOnlyTheBodyAboveIt`, `TestSettingAndNotProductionSettingAppliesToTestCode`, `ProblemElsewhereDoesNotKeepValidLimitFromTakingEffect` ×3 | RH2111-001, SHARED-LINES-002 | `Verify` | as stated |
| `ExampleTypesAreNotReportedAtTheDefault` (RH2111-001 last sentence) | RH2111-001 | `Verify` | not reported |
| `TypeInTestProjectIsJudgedAgainstTestLimit` ×6 / `TypeOutsideTestProjectIsJudgedAgainstProductionLimit` ×5 / `TypeWithoutGlobalOptionsIsProductionCode` | SHARED-LINES-003 (both directions for each property) | `Verify` + global config | not reported / reported 3/2 |
| `TypeIsJudgedByTheLinesBetweenItsBraces` (RH2111-002 example: `C` 6/4, `S` 4 not) / `CountIsTheSameForEveryLineEnding` ×3 | RH2111-002, RH2111-006, SHARED-LINES-001; FC-020, FC-033 | `Verify` | reported 6/4 |
| `HeaderIsNotCounted` (attributes, base list, constraints, type parameters; 3), `ParameterListOfPositionalRecordIsNotCounted` (2) | RH2111-002 (+RH2111-006) | `Verify` | as stated |
| `EveryTypeKindWithBraceBodyIsJudged` (struct, interface, record, record class, record struct), `UnionIsJudgedAndEnumIsNot` (change doc change 4, with `UnionSupport`), `FileTypeAndStaticClassAreJudged` | RH2111-002; FC-024 | `Verify` | each reported |
| `ExcludedMemberIsNotCounted` ×2 (`FEATURE` undefined 2 / defined 3; `#pragma`, `/** */`) | SHARED-LINES-001; FC-020, SHARED-DIAG-008 | `Verify` | reported 2/1, 3/1 |
| `EveryLineOfRawStringInFieldCountsTowardTheType` (5), `MemberAttributeLineIsCounted` (4), `LambdaInFieldInitializerCountsTowardTheType` (6), `ExtensionBlockCountsTowardItsTypeAndIsNotJudged` (5, block not reported), `OneLineBodyAndEmptyBodyAreAtOrBelowLimitOfOne` | RH2111-002, SHARED-LINES-001; FC-022, FC-027 | `Verify` | as stated |
| `PartInGeneratedFileIsNotReported` (`Generated.g.cs` part) | RH2111-004; SHARED-DIAG-001 | `Verify` | only the ordinary part reported |
| `NestedTypeCountsTowardTheOuterType` (RH2111-003 example, 5 at 3), `EveryLevelOfNestingIsJudged` (8, 5, 2) | RH2111-003; FC-027 | `Verify` | as stated |
| `EachPartIsJudgedOnItsOwn` (RH2111-004 example), `EveryPartOverTheLimitIsReportedOnThatPart` (parts in two files, nested type in a part) | RH2111-004, RH2111-003; FC-024 | `Verify` | as stated |
| `TypeWithoutBraceBodyIsNotReported` (RH2111-005 example, record struct, `class C;`, `struct S;`, delegate), `UnionWithoutBraceBodyIsNotReported` | RH2111-005 | `Verify` | not reported |
| `VerbatimIdentifierIsReportedOnTheWholeToken` | RH2111-006; FC-023 | `Verify` | reported 2/1 |

### Not covered, and why

As under #353. Also: an `interface I;` without a body (RH2111-005 lists only `record R(…);` and `class C;`; the case would only repeat `class C;`).

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2111TypesShouldNotExceedTheLineLimitAnalyzerTests"
```

### Follow-up candidates

_None._

## Whole run

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `src/Reihitsu.SelfHosting.Test/Analyzers/AnalyzerViolationReportTests.cs`: `LineCountAnalyzersHaveNoRow` (new) | — (maintainer answer 11A) | MSTest asserts on `AnalyzerSelfHostingTests.AnalyzerNames` | compile error now; with the analyzers and no exclusion: fails, "unexpected: RH2008MethodsShouldNotExceedTheLineLimitAnalyzer" |
| same file: `AnalyzerRowsMatchTheAnalyzersTheCompilerLoadsFromTheAnalyzerAssembly` (changed) | — (11A) | Roslyn `AnalyzerFileReference` | with the analyzers and no exclusion: fails, expected the two names, actual none |

Existing tests changed, and why:

- `AnalyzerRowsMatchTheAnalyzersTheCompilerLoadsFromTheAnalyzerAssembly` asserted that every analyzer the compiler loads has a row; 11A makes RH2008 and RH2111 deliberately row-less. It now asserts that the analyzers without a row are exactly those two (`_analyzersLeftOutOfTheScan`, ordered by name), so a third analyzer without a row still fails, and so does an exclusion that is removed or widened. Where the exclusion lives (`AnalyzerCatalog` or `AnalyzerSelfHostingTests`) is not pinned; only the row names are.
- `RuleDocumentationExampleOptOuts` (test utility) gains `RH2008` and `RH2111`: their page examples need a `reihitsu.json` with a limit far below the default, which the example test does not pass (01-/03-/05-spec-author.md). With the stubs, `RuleDocumentationExampleTests` passed in full, `OptedOutDocumentsStillFailVerification` included.
- No existing test asserts "Unknown configuration section 'design'." (`rg "'design'|\"design\"" src --glob '*.cs'` was empty before this change), so no existing RH0001 test changes.
- No existing RH8402 test uses a `design` section; `ConfigurationWithProblem` and its siblings stay as they are.
- `AnalyzerPackageMetadataTests`: not changed; `MessageFormatResourcesMatchTitleWhenNoPlaceholders` skips message formats with placeholders, which both new messages have.

### Not covered, and why

- `Release.ruleset`/`Debug.ruleset` entries (11A): no test reads the rulesets per rule; the spec checker and auditor check them in the diff.

### Commands

```shell
scripts/test.sh --project selfhosting --filter "FullyQualifiedName~AnalyzerViolationReportTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"
```

All changed test files were run through `scripts/format.sh` (no change) and built warning-free against the stubs (the build runs the repository's analyzers on test code).

### Follow-up candidates

_None._
