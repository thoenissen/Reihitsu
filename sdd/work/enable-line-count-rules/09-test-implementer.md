Role: test implementer
Commit: b9548430
Verdict: #1865: tests written (decision 20 done); #1866: no work in this stage

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Worklist: decision 20 of `00-lead.md`.

## thoenissen/Reihitsu#1865

### Decision 20 — the 11 test types split by topic, no `partial`

Every `<Class>.Part2.cs`/`.Part3.cs` is gone, and no test class is `partial`. Each of the 11 classes became separate, non-partial classes, one per topic (the construct or concern its tests cover). The class with the original name keeps the rule's core topic, so most fully qualified names of the basic tests are unchanged. The new classes follow the existing pattern of sibling test classes for one rule (`RH5302LogicalExpressionsShouldBeFormattedCorrectlyRepeatedFixTests`): the full rule name, then the topic, then `Tests`, with the file named after the class.

| Original class | Classes now (test methods, lines of code by RH2111's count) |
|---|---|
| `RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzerTests` | the same name: listed positions, throw expressions, keyword spacing, comments and directives (85, 2044); `…AssignmentTargetTests` (53, 1325); `…ReceiverTests`: nested pairs and receivers (42, 1132); `…AmbiguousShapeTests`: pointer-, generic- and tuple-shaped code the parser would read differently (72, 2534) |
| `RH3004UseLambdaSyntaxAnalyzerTests` | the same name: reports, the lambda and the block layout (43, 1290); `RH3004UseLambdaSyntaxStatementShiftTests` (26, 1014); `RH3004UseLambdaSyntaxMultiLineTokenAndDirectiveTests` (84, 2176) |
| `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests` | the same name (47, 1291); `…TriviaTests`: comments, directives, modifier reordering (84, 1711); `…NameBindingTests` (81, 2343) |
| `RH8201InheritdocShouldBeUsedAnalyzerTests` | the same name: overrides, the replaced comment, partial members (43, 2296); `…InterfaceImplementationTests` (51, 1703) |
| `RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests` | the same name (68, 1906); `…ConditionalAccessTests` (30, 1380); `…LaterLineTests` (41, 1282) |
| `RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests` | the same name (44, 1241); `…HoleTests` (56, 1920) |
| `RH7101DoNotCombineFieldsAnalyzerTests` | the same name: reports, fields and layout the fix writes (51, 1171); `…CommentAndDirectiveTests` (95, 1935) |
| `RH7309RegionsShouldFollowCategoryOrderAnalyzerTests` | the same name (33, 1514); `…InitializationOrderTests`: initializer order and struct layout (44, 1895); `…TriviaTests` (30, 1234) |
| `RH4103MethodNameCasingAnalyzerTests` | the same name: reports, cascades, native entry points (44, 1417); `RH4103MethodNameCasingNameBindingTests` (51, 1931) |
| `RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests` | the same name (78, 1577); `…ExcludedCodeTests` (76, 2532); `…TypeDeclarationGroupTests` (46, 1376) |
| `HoleAnchorInStatementColumnAlignmentTests` (formatter) | the same name: anonymous methods (38, 1527); `LambdaHoleAnchorInStatementColumnAlignmentTests`: block lambdas (50, 1995) |

What each new class needed, and the choices made:

- **Base class and Fix All scenario.** Every new analyzer class derives from `BatchCodeFixTestsBase<TAnalyzer, TCodeFix>` like its original and carries a `GetFixAllScenario` override. That override is a verbatim copy of the original class's scenario, so every class runs the base tests `FixAllCorrectsEveryDiagnostic` and `CodeFixProvidesFixAllProvider` on a scenario already known to pass; 17 new classes add 34 base-test results. I did not write topic-specific scenarios: in 10 of the 17 classes no test of the topic reports two or more fixable diagnostics in one document, so a topic scenario would be new test content with an expected output derived by me rather than from a spec example. Whether the duplicated scenario should later give way to topic scenarios is the lead's call; it changes no assertion of this stage.
- **Helpers.** A class carries exactly the non-test members its tests reference (computed by identifier over the members, transitively). Shared private helpers are therefore copied into each class that uses them, unchanged: `WithEndOfLine` (RH2001, RH7109: 2 copies each), `VerifyUnderBothLineEndings` (RH5111: 3), `AssertParseIsPreserved` and `GetSyntaxKindsWithoutParentheses` (RH3002: 2), `AssertSingleFixUnderBothLineEndings` (both overloads) and `ApplyFixOfDeclarationAsync` (RH7101: 2), `DefineDebug` (RH7309: 2), `CreateMessage` (RH7309: 3, one used by the copied scenario). Copying keeps every test method's text unchanged; a shared helper class would have needed qualified calls in the tests. Small per-class copies of `WithEndOfLine` and `DefineDebug` already exist in 6 and 7 test classes of the repository.
- **Summaries.** Each class has a summary naming the analyzer and/or code-fix provider and the topic. The original classes' summaries gained the topic; the formatter class's summary now says "anonymous method" instead of "anonymous method or a block lambda", the lambda half moved to the new class.
- **Usings.** Each file keeps the usings of its original, minus those it no longer needs (CS8019, checked by compiling each file against the test assembly).
- **Order and regions.** Members keep their original relative order and their region (`Constants`, `Properties`, `Methods`, `Test data`, `Tests`, `BatchCodeFixTestsBase`).

The 9 test-method fixes of stage 07 (constants moved to class level) are untouched, as decision 20 says.

### How I proved that assertions, inputs, expected outputs and test names are unchanged

1. *Member texts.* A Roslyn tool (scratch, not committed) listed every member of every class, from the start of its documentation comment to the end of its node, as (SHA-256 of the exact text, name, test or not). For each of the 11 rules I compared the files at b9548430 (main file plus parts) with the new files after `scripts/format.sh` (which changed nothing: "Formatted 0 of 29 file(s)"):
   - the set of test members is identical, and every test member occurs exactly once before and after (1586 test methods: 252, 153, 212, 94, 139, 100, 146, 107, 95, 200, 88);
   - the set of distinct member texts is identical — nothing is new, nothing is lost;
   - the only multiplicity changes are the copies of non-test members listed above.
   So no test's attributes, `[Requirement]` tags, name, input, expected output, or assertion changed.
2. *Test results.* I ran the affected analyzer classes (filter: the ten rule-name prefixes, which also catch three sibling classes that did not change) and the formatter class with a TRX logger on b9548430 and on this tree, both Release. The multisets of (display name including data rows, outcome) differ only by 17 × `FixAllCorrectsEveryDiagnostic` and 17 × `CodeFixProvidesFixAllProvider`, all passed: analyzer 1828 → 1862 passed, 13 → 30 classes; formatter 88 → 88 passed, 1 → 2 classes. Only the class part of the fully qualified names changed, for the moved tests.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| the 1586 moved test methods | unchanged tags | unchanged | pass, identical outcome to before |
| `FixAllCorrectsEveryDiagnostic`, `CodeFixProvidesFixAllProvider` in the 17 new analyzer classes | `SHARED-FIX-001`, `SHARED-FIX-002` (inherited tags of the base test) | `BatchCodeFixTestsBase` | pass |
| `AnalyzerSelfHostingTests.AnalyzerReportsNoViolationsOnOwnSourceCode` RH2008, RH2111 rows (unchanged) | — | `Collect` | pass (see Commands) |

### Not covered, and why

No production code and no assertion changes in this stage; FC-017 (not weakening) is the only applicable class, proven above. No new test content was written beyond the copied scenarios.

### Commands

```shell
scripts/build.sh
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3002StatementMustNotUseUnnecessaryParentheses|FullyQualifiedName~RH3004UseLambdaSyntax|FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsed|FullyQualifiedName~RH8201InheritdocShouldBeUsed|FullyQualifiedName~RH5111AssignmentsMustHaveProperLineBreaks|FullyQualifiedName~RH5202RawStringLiteralsShouldBeFormattedCorrectly|FullyQualifiedName~RH7101DoNotCombineFields|FullyQualifiedName~RH7309RegionsShouldFollowCategoryOrder|FullyQualifiedName~RH4103MethodNameCasing|FullyQualifiedName~RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElements"
scripts/test.sh --project formatter --filter "FullyQualifiedName~HoleAnchorInStatementColumnAlignmentTests"
scripts/test.sh --project selfhosting --filter "FullyQualifiedName~AnalyzerSelfHostingTests|FullyQualifiedName~LineCountSelfHostingTests|FullyQualifiedName~BuildConfigurationTests"
scripts/test.sh --project architecture
```

Runs at this tree (Release): `scripts/build.sh` 0 warnings, 0 errors; analyzer filter Passed 1862; formatter filter Passed 88; selfhosting filter Passed 361 (its RH2008 and RH2111 rows included); architecture Passed 198.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1866

No work in this stage.
