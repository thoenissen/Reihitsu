Role: test implementer
Commit: 99d3045
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore. Expected values come from the spec examples and from scores derived by hand from RH2009-003/004/005 as they stand at 99d3045 (maintainer answers 1–25 applied); no implementation exists (FC-035).

## Tests

All new RH2009 tests are in `src/Reihitsu.Analyzer.Test/Design/RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzerTests.cs` (91 test methods, 174 cases with data rows), on `AnalyzerTestsBase<RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer>.Verify` through the local helpers `VerifyComplexity` (adds `reihitsu.json`), `Complexity(key, score, limit)` (exact message), `Limit(n)`. Most scoring tests use limit 1, so the exact score is asserted through the message.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| ScoreExampleIsReportedAgainstTheDefaultLimit | RH2009-001, RH2009-006, SHARED-LIMIT-001, SHARED-LIMIT-002 | Verify | does not compile |
| NestedBodyAtTheDefaultLimitIsNotReported | RH2009-001, SHARED-LIMIT-002; FC-003 | Verify | does not compile |
| ConfiguredLimitEqualToTheComplexityIsNotReported | RH2009-001, SHARED-LIMIT-002; FC-003 | Verify | does not compile |
| ConfiguredLimitBelowTheComplexityIsReported | RH2009-001, SHARED-LIMIT-001, SHARED-LIMIT-002 | Verify | does not compile |
| ConfiguredLimitAppliesToTestCode (2 rows) | RH2009-001 | Verify + global config | does not compile |
| DefaultLimitAppliesToTestCode | RH2009-001 | Verify + global config | does not compile |
| UnknownTestSettingDoesNotChangeTheLimitOfTestCode | RH2009-001 | Verify + global config | does not compile |
| SettingWithoutValidValueKeepsTheDefaultLimit (16 rows) | RH2009-001, SHARED-LIMIT-001 | Verify | does not compile |
| ProblemElsewhereDoesNotKeepValidLimitFromTakingEffect (4 rows) | RH2009-001, SHARED-LIMIT-001 | Verify | does not compile |
| LineLimitsDoNotChangeTheComplexityLimit (2 rows) | RH2009-001 | Verify | does not compile |
| LargestValidLimitTakesEffect | SHARED-LIMIT-001 | Verify | does not compile |
| FlatBodyFarAboveTheDefaultLimitIsNotReported | RH2009-001, RH2009-004; FC-003 | Verify | does not compile |
| SameBodyWithOneNestedIfIsReportedByItsWholeComplexity | RH2009-001, RH2009-004; FC-003 | Verify | does not compile |
| AccessorPropertyAndOperatorAreJudgedByTheirBody | RH2009-002, RH2009-006 | Verify | does not compile |
| PartialMethodIsJudgedByItsImplementingPart | RH2009-002; FC-024 | Verify | does not compile |
| PartialPropertyIsJudgedByItsImplementingPart | RH2009-002; FC-024 | Verify | does not compile |
| ExpressionBodiedMembersAreJudgedByTheirExpression | RH2009-002, RH2009-006; FC-025 | Verify | does not compile |
| EveryAccessorKindIsJudged | RH2009-002, RH2009-006 | Verify | does not compile |
| AccessorsAtTheLimitAreNotSummed | RH2009-002 | Verify | does not compile |
| AccessorsAboveTheLimitAreReportedEachWithItsOwnComplexity | RH2009-002, RH2009-006 | Verify | does not compile |
| StaticConstructorAsyncMethodAndConversionOperatorAreJudged | RH2009-002, RH2009-006; FC-025 | Verify | does not compile |
| CompoundAssignmentOperatorIsJudged | RH2009-002, RH2009-006 | Verify | does not compile |
| MemberOfExtensionBlockIsJudged | RH2009-002 | Verify | does not compile |
| ExplicitInterfaceImplementationIsJudged | RH2009-002, RH2009-006; FC-024 | Verify | does not compile |
| InterfaceMemberWithDefaultImplementationIsJudged | RH2009-002 | Verify | does not compile |
| MembersOfStructAndRecordAreJudged | RH2009-002 | Verify | does not compile |
| ConstructorInitializerParameterDefaultsAndAttributeArgumentsAreNotPartOfTheBody | RH2009-002 | Verify | does not compile |
| DeeperNestingCostsMore | RH2009-003 | Verify | does not compile |
| SequentialIfsAboveTheLimitAreNotReported | RH2009-003, RH2009-004 | Verify | does not compile |
| IncrementsOfTheSecondExampleAddUpToEleven | RH2009-003 | Verify | does not compile |
| SecondExampleAtItsComplexityIsNotReported | RH2009-003; FC-003 | Verify | does not compile |
| SequencesOfLogicalOperatorsAndPatternCombinatorsExample | RH2009-003 | Verify | does not compile |
| LogicalOperatorSequencesAreCountedPerTree (21 rows) | RH2009-003; FC-002 | Verify | does not compile |
| CompoundLogicalAssignmentsScoreNothing | RH2009-003 | Verify | does not compile |
| ChainedConditionalExpressionScoresLikeElseIfChain | RH2009-003, RH2009-004 | Verify | does not compile |
| ConditionalChainIsNotNested (3 rows) | RH2009-003, RH2009-004; FC-002 | Verify | does not compile |
| ConditionalBehindAnotherOperatorInFalseBranchNests | RH2009-003; FC-002, FC-003 | Verify | does not compile |
| IncrementsWithoutNestingStayOneInsideNestedPart | RH2009-003, RH2009-004 | Verify | does not compile |
| SwitchScoresOnceAndItsNestedPartNests | RH2009-003 | Verify | does not compile |
| SwitchScoresOnceForAllItsSectionsAndArms | RH2009-003 | Verify | does not compile |
| CombinatorsOfCaseLabelsAndArmsScoreNothing | RH2009-003 | Verify | does not compile |
| ConditionalInWhenClauseIsNestedInTheSwitch | RH2009-003, RH2009-006 | Verify | does not compile |
| SwitchExpressionInWhenClauseIsNestedInTheSwitch | RH2009-003, RH2009-004; FC-039 | Verify | does not compile |
| EveryFormOfGotoScoresOne | RH2009-003 | Verify | does not compile |
| EveryCatchClauseScoresAndTryAndFinallyDoNot | RH2009-003 | Verify | does not compile |
| RecursionScoresOnceAndOnlyForDirectSelfCalls | RH2009-003 | Verify | does not compile |
| OnlyAnInvocationOfTheMemberItselfIsRecursion (9 rows) | RH2009-003; FC-016 | Verify | does not compile |
| QualifiedAndGenericSelfCallsAreRecursion | RH2009-003 | Verify | does not compile |
| NewOfOwnTypeAndCompoundAssignmentScoreNothingExample | RH2009-003 | Verify | does not compile |
| NewOfOwnTypeAndCompoundAssignmentScoreNothing | RH2009-003 | Verify | does not compile |
| OwnPropertyInAccessorAndOwnOperatorInOperatorAreNotRecursion | RH2009-003 | Verify | does not compile |
| StatementsThatDoNotNestExample | RH2009-003 | Verify | does not compile |
| StatementsThatDoNotNestLeaveTheInnerIfAtNestingOne | RH2009-003 | Verify | does not compile |
| StatementThatDoesNotNestLeavesTheNestingUnchanged (11 rows) | RH2009-003 | Verify + unsafe | does not compile |
| NullHandlingOperatorsScoreNothing | RH2009-003 | Verify | does not compile |
| JumpsOtherThanGotoScoreNothing | RH2009-003 | Verify | does not compile |
| QueryClausesNeitherScoreNorNest | RH2009-003 | Verify | does not compile |
| DeconstructingAndAwaitedForeachAreLoops | RH2009-003 | Verify | does not compile |
| EveryLoopNestsItsBody (4 rows) | RH2009-003 | Verify | does not compile |
| IfWithoutBracesInsideAnotherNests | RH2009-003, RH2009-004; FC-027 | Verify | does not compile |
| IfElseInElseBlockNests | RH2009-003, RH2009-004; FC-002 | Verify | does not compile |
| StructureInConditionIsOutsideTheNestedPartOfItsConstruct (12 rows) | RH2009-003; FC-001, FC-039 | Verify + unsafe | does not compile |
| ConditionalDirectiveNeitherScoresNorNests | RH2009-003; FC-020 | Verify + symbols | does not compile |
| ConditionalDirectiveDoesNotMakeItsCodeNested | RH2009-003, RH2009-004; FC-020 | Verify + symbols | does not compile |
| BodyWithoutNestingIncrementIsNotReported | RH2009-004 | Verify | does not compile |
| SameBodyWithOneNestedStructureIsReportedByItsWholeComplexity | RH2009-004; FC-003 | Verify | does not compile |
| IfInElseBlockMakesTheMemberReportable | RH2009-004 | Verify | does not compile |
| ElseIfChainIsNotReported | RH2009-004; FC-003 | Verify | does not compile |
| IfInCatchBlockNestsAndIfInTryBlockDoesNot | RH2009-004 | Verify | does not compile |
| StructureInConditionDoesNotMakeTheMemberReportable (10 rows) | RH2009-004; FC-003 | Verify | does not compile |
| NestingOnlyThroughLambdaIsNotReported | RH2009-004 | Verify | does not compile |
| NestingOnlyThroughBlockLambdaOrAnonymousMethodIsNotReported | RH2009-004 | Verify | does not compile |
| NestingInsideLambdaMakesTheMemberReportable | RH2009-004, RH2009-005; FC-003 | Verify | does not compile |
| NestingInExcludedCodeDoesNotMakeTheMemberReportable | RH2009-004, RH2009-006; FC-020 | Verify | does not compile |
| LambdaBodyCountsTowardTheMemberOneLevelDeeper | RH2009-005 | Verify | does not compile |
| AnonymousMethodBodyCountsTowardTheMemberOneLevelDeeper | RH2009-005 | Verify | does not compile |
| ExpressionLambdaInsideNestedPartAddsItsLevel | RH2009-005 | Verify | does not compile |
| LambdaInsideLambdaAddsAnotherLevel | RH2009-005; FC-027, FC-039 | Verify | does not compile |
| LocalFunctionIsJudgedOnItsOwn | RH2009-005, RH2009-006 | Verify | does not compile |
| LocalFunctionAtTheLimitIsNotReported | RH2009-005 | Verify | does not compile |
| LocalFunctionInNestedPartStartsAtNestingZero | RH2009-005, RH2009-006 | Verify | does not compile |
| RecursionOfLocalFunctionDoesNotCountTowardItsHolder | RH2009-003, RH2009-005 | Verify | does not compile |
| LocalFunctionInsideLocalFunctionIsJudgedOnItsOwn | RH2009-005; FC-039 | Verify | does not compile |
| LocalFunctionInsideLambdaIsJudgedOnItsOwn | RH2009-005; FC-039 | Verify | does not compile |
| LocalFunctionInsideFieldInitializerLambdaIsJudged | RH2009-005; FC-039 | Verify | does not compile |
| LocalFunctionInTopLevelStatementsIsJudged | RH2009-005 | Verify + console output | does not compile |
| LambdaInFieldInitializerIsNotJudged | RH2009-005 | Verify | does not compile |
| LambdaOutsideEveryJudgedBodyIsNotJudged | RH2009-005 | Verify | does not compile |
| MembersWithoutIdentifierAreReportedOnTheirNamingToken | RH2009-006 | Verify + symbols | does not compile |
| ExcludedCodeCountsAsAbsent | RH2009-006, RH2009-004; FC-020 | Verify | does not compile |
| VerbatimIdentifierIsReportedOnTheWholeToken | RH2009-006; FC-023 | Verify | does not compile |

RH0001 (`src/Reihitsu.Analyzer.Test/Analyzer/RH0001ConfigurationFileMustBeValidAnalyzerTests.cs`, `VerifyConfiguration`):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| DesignSectionWithEverySettingIsValid (renamed from DesignSectionWithEveryLineLimitIsValid; the RH0001-007 valid example now holds `maximumMethodComplexity: 20`) | RH0001-004, RH0001-007 | VerifyConfiguration | fails: "Unknown configuration setting 'design.maximumMethodComplexity'." |
| LimitAtTheBoundsOfTheRangeIsValid (renamed from LineLimitAtTheBoundsOfTheRangeIsValid; rows 1 and 2147483647 for `maximumMethodComplexity` added) | RH0001-007 | VerifyConfiguration | new rows fail (unknown setting); old rows pass |
| InvalidDesignSettingsAreReported (rewritten to the changed RH0001-007 invalid example: `-1` at column 149, unknown `maximumTestMethodComplexity` at 154) | RH0001-002, RH0001-007 | VerifyConfiguration | fails (unknown setting at 123 instead of the value problem at 149) |
| NonWholeNumberComplexityLimitIsReported (10 rows, column 42) | RH0001-007 | VerifyConfiguration | fails (unknown setting at 16) |
| StringComplexityLimitIsReported (column 43) | RH0001-007 | VerifyConfiguration | fails (unknown setting at 16) |
| InvalidValueIsReportedForEverySetting (row `maximumMethodComplexity`, 42 added) | RH0001-007 | VerifyConfiguration | new row fails; old rows pass |
| UnknownDesignSettingIsReported (rows `maximumTestMethodComplexity`, `MaximumMethodComplexity`, `maximumComplexity` added) | RH0001-007 | VerifyConfiguration | passes (already unknown on `main`; pins that only the exact name is accepted) |
| DesignSectionNextToOtherSectionsIsValid (`"maximumMethodComplexity": 15` added to the design section) | RH0001-004, RH0001-007 | VerifyConfiguration | fails (unknown setting) |

Existing tests re-tagged for SHARED-LIMIT (the shared statements moved out of SHARED-LINES-002; the SHARED-LINES-002 tags stay, since SHARED-LINES-002 still states the comparison by reference). All pass on this head:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH2008 ConfiguredLimitReportsOnlyTheBodyAboveIt (the SHARED-LIMIT-001 and -002 example) | + SHARED-LIMIT-001, SHARED-LIMIT-002 | VerifyLines | passes |
| RH2008 ProblemElsewhereDoesNotKeepValidLimitFromTakingEffect, DefaultProductionLimitAppliesWithoutValidSetting, DefaultProductionLimitAppliesWithoutConfigurationFile, ProductionSettingReportedByRH0001KeepsDefault, InvalidSettingKeepsDefaultProductionLimit, InvalidTestSettingKeepsDefaultTestLimit, LargestValidLimitTakesEffect | + SHARED-LIMIT-001 | VerifyLines | passes |
| RH2008 DefaultProductionLimitIsSixty, DefaultTestLimitIsOneHundredTwenty | + SHARED-LIMIT-002 | VerifyLines | passes |
| RH2111 DefaultProductionLimitIsFifteenHundred, DefaultTestLimitIsThreeThousand, ConfiguredLimitReportsOnlyTheBodyAboveIt | + SHARED-LIMIT-002 | VerifyLines | passes |
| RH2111 InvalidSettingKeepsDefaultProductionLimit, InvalidTestSettingKeepsDefaultTestLimit, ProblemElsewhereDoesNotKeepValidLimitFromTakingEffect | + SHARED-LIMIT-001 | VerifyLines | passes |

Self-hosting (maintainer answer 11, decision 4; no requirement, so no tag):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RulesetTests.ReleaseRulesetSwitchesOffComplexityRule` — `Release.ruleset` lists RH2009 with `Action="None"` | — | `LoadReihitsuEntries` | fails: no RH2009 entry |
| `AnalyzerViolationReportTests.ComplexityAnalyzerHasARow` | — | `AnalyzerSelfHostingTests.AnalyzerNames` | does not compile |
| `AnalyzerViolationReportTests.ReportLeavesOutComplexityViolations` — RH2009 reports the Score sample directly (control), `AnalyzerViolationReport.Collect` returns nothing for it (RH2009 on `_ignoredDiagnosticIds`) | — | `CompilationWithAnalyzers`, `AnalyzerViolationReport.Collect` | does not compile |

`RulesetTests.ReleaseRulesetListsEveryShippedRule` (existing, unchanged) also starts failing once the analyzer ships without a `Release.ruleset` entry; it passes again with the entry above.

**Why they fail.** The analyzer test project and the self-hosting test project do not compile until `RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer` (namespace `Reihitsu.Analyzer.Rules.Design`, `public const string DiagnosticId`) exists. To watch the failure reasons, I compiled both projects against a do-nothing stub of that type placed temporarily in each test project (deleted, never committed): all 143 RH2009 cases that expect a diagnostic failed only on "Mismatch between number of diagnostics returned, expected N actual 0", with no compiler error from any test source (the C# 14 compound-assignment operator and extension block compile); the 31 "not reported" cases passed vacuously, as they must until the analyzer exists; the RH0001 failures are all "Unknown configuration setting 'design.maximumMethodComplexity'."; the three self-hosting tests failed on the missing ruleset entry, the missing row, and the control assertion "RH2009 does not report the sample". `RequirementCoverage` architecture tests passed with the new tags.

## Scenarios (bug fix)

_N/A_ — new rule.

## Not covered, and why

- **Line endings (CRLF).** No test varies the line ending: RH2009 decides on syntax nodes only, never on lines or columns, and SHARED-LINES-001 (which does) is not used by RH2009.
- **Documentation mode (FC-040).** A documentation comment is trivia and holds no statement; nothing RH2009-003 scores can sit in one.
- **Formatter parity, idempotency, code fix, Fix All, convergence (FC-007 to FC-014, FC-028, FC-030, FC-038 to FC-043 except FC-039).** No code fix and no formatter phase is involved (skeptic 02, *Surfaces*; spec non-goals). FC-039 applies to the analyzer's walk only and is covered by the nested-descent tests marked above.
- **Trivia and directives in gaps (FC-004, FC-005, FC-019, FC-021).** The rule touches no gap; directives matter only as SHARED-DIAG-008, covered by the `#if` tests.
- **FC-006, FC-022, FC-026, FC-029, FC-036, FC-018.** No rewrite, no rename, no literal handling.
- **FC-015 (shared member selection, decision 1) and FC-016.** Member selection is RH2008's, referenced by RH2009-002/006; the RH2009 member-kind tests mirror RH2008's member-kind tests (extension block, explicit interface implementation, interface default member, partial method and property, every accessor, every expression-bodied kind, struct, record), so drift between the two shows as a failing RH2009 test. No `rg` proof of one call site is possible before the implementation exists, so every member kind has its own test.
- **Configuration file "cannot be read"** (SHARED-LIMIT-001) — the test harness supplies additional files as text; an unreadable file cannot be produced there. RH2008 has no test for it either.
- **`#if` scoring as `if` (03-skeptic Q1 B), indirect recursion, `&`/`|`/`^` scoring nothing** — rejected by the maintainer; the tests assert the chosen behavior (directive scores nothing; `IsEven` 3; `&`, `|`, `^` on `bool`/`bool?` count).
- **Lambdas outside judged members reported** — follow-up per maintainer answer 22; `LambdaOutsideEveryJudgedBodyIsNotJudged` pins today's behavior.
- **Root `reihitsu.json`** — `BuildConfigurationTests.RootConfigurationSetsOnlyTheProductionTypeLimitOfFiveHundred` pins it to `maximumTypeLines` alone; the run does not change it (the self-hosting scan ignores RH2009 instead), so I left that test unchanged.
- **`Debug.ruleset`** — decision 4 names only `Release.ruleset`; no Debug expectation is specified, so none is tested.
- **Rule page examples** — `RuleDocumentationExampleTests` runs `documentation/rules/RH2009.md`'s Violation/Correction examples automatically once the analyzer ships (no opt-out needed: the violation is 16 against the default 15).

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH0001ConfigurationFileMustBeValidAnalyzerTests|FullyQualifiedName~RH2008MethodsShouldNotExceedTheLineLimitAnalyzerTests|FullyQualifiedName~RH2111TypesShouldNotExceedTheLineLimitAnalyzerTests"
scripts/test.sh --project selfhosting --filter "FullyQualifiedName~AnalyzerViolationReportTests|FullyQualifiedName~RulesetTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Requirement"
```

## Follow-up candidates

_None._
