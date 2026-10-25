Role: test implementer
Commit: 69e28ac (tests written and run against this head; `src/` production code unchanged since `origin/main`)
Verdict: #948: reproduced; #949: reproduced; #954: reproduced; #1390: reproduced; #888: reproduced

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The issues' own cause hypotheses (#949, #954, #888 as filed) were not used; expected values come from the spec examples and requirement texts on this branch (`git diff origin/main -- sdd/specs documentation`), never from a patched copy of the code.

Every formatter test below uses `Formatter.Test/Helpers/FormatterTestsBase.AssertRuleResult`, which runs the full pipeline under LF and CRLF and, when an expected output is given, asserts it after the first pass and again after a second pass on that output (the idempotency check the maintainer asked for). An "unchanged" test (`AssertRuleResult(input)`) asserts that one pass leaves the input byte-identical under LF and CRLF, which makes the input its own fixed point. "Fails now" means it fails on this head for the reason in the `Scenarios` table, checked in each test's failure output; "passes now" is a characterization of behavior that must stay.

## thoenissen/Reihitsu#948

### Tests

File `src/Reihitsu.Formatter.Test/Regression/SwitchCaseBraces/SwitchSectionSharedLocalTests.cs` (new), helper `AssertRuleResult`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ReportedLocalUsedInAnotherSectionLeavesTheSwitchUnbraced` (issue input; expected = input plus the FMT-BLANK-002 blank line, as in the spec example) | FMT-SWITCH-014, FMT-PIPE-003; FC-034 | `AssertRuleResult(input, expected)` | fails |
| `LocalUsedInAnotherSectionLeavesTheSwitchUnchanged` (spec example 1) | FMT-SWITCH-014 | `AssertRuleResult(input)` | fails |
| `PatternVariableUsedInAnotherSectionLeavesTheSwitchUnchanged` (spec example 2) | FMT-SWITCH-014 | unchanged | fails |
| `LocalFunctionCalledAboveItsDeclarationLeavesTheSwitchUnchanged` (spec example 3) | FMT-SWITCH-014 | unchanged | fails |
| `LocalFunctionCalledBelowItsDeclarationLeavesTheSwitchUnchanged` (issue comment's direction, block-bodied) | FMT-SWITCH-014; FC-034 | unchanged | fails |
| `ConstantUsedAsAnotherSectionLabelLeavesTheSwitchUnchanged` (spec example 4) | FMT-SWITCH-014 | unchanged | fails |
| `LocalFunctionCalledInAnotherSectionWhenClauseLeavesTheSwitchUnchanged` | FMT-SWITCH-014; FC-016 | unchanged | fails |
| `OutVariableOfReturnStatementUsedInAnotherSectionLeavesTheSwitchUnchanged` | FMT-SWITCH-014; FC-016 | unchanged | fails |
| `OutVariableOfExpressionStatementUsedInAnotherSectionLeavesTheSwitchUnchanged` | FMT-SWITCH-014 | unchanged | fails |
| `DeconstructionVariableUsedInAnotherSectionLeavesTheSwitchUnchanged` | FMT-SWITCH-014; FC-016 | unchanged | fails |
| `LabeledLocalDeclarationUsedInAnotherSectionLeavesTheSwitchUnchanged` | FMT-SWITCH-014; FC-016 | unchanged | fails |
| `OutVariableOfThrowStatementUsedInAnotherSectionLeavesTheSwitchUnchanged` | FMT-SWITCH-014; FC-016 | unchanged | fails |
| `OutVariableOfYieldReturnStatementUsedInAnotherSectionLeavesTheSwitchUnchanged` | FMT-SWITCH-014; FC-016 | unchanged | fails |
| `OutVariableOfLockExpressionUsedInAnotherSectionLeavesTheSwitchUnchanged` | FMT-SWITCH-014; FC-016 | unchanged | fails |
| `OutVariableOfNestedSwitchGoverningExpressionUsedInAnotherSectionLeavesTheSwitchUnchanged` | FMT-SWITCH-014; FC-016 | unchanged | fails |
| `PatternVariableOfExpressionStatementUsedInAnotherSectionLeavesTheSwitchUnchanged` | FMT-SWITCH-014; FC-016 | unchanged | fails |
| `LabeledOutVariableUsedInAnotherSectionLeavesTheSwitchUnchanged` | FMT-SWITCH-014; FC-016 | unchanged | fails |
| `LocalUsedInLambdaOfAnotherSectionLeavesTheSwitchUnchanged` | FMT-SWITCH-014; FC-027 | unchanged | fails |
| `LocalUsedInExistingBlockOfAnotherSectionLeavesTheSwitchUnchanged` (use inside an existing FMT-SWITCH-008 block) | FMT-SWITCH-014; FC-027 | unchanged | fails |
| `GuardedOuterSwitchKeepsInnerSwitchDecision` (outer guarded, inner braced on its own) | FMT-SWITCH-014; FC-027 | `AssertRuleResult(input, expected)` | fails |
| `TwoSwitchesInOneMethodAreDecidedSeparately` (one guarded, one braced) | FMT-SWITCH-014; FC-026 | `AssertRuleResult(input, expected)` | fails |
| `LocalUsedOnlyInItsOwnSectionDoesNotBlockTheBraces` (spec example 5) | FMT-SWITCH-014; FC-003 | `AssertRuleResult(input, expected)` | passes |
| `LocalsInNestedBlockAndForeachHeaderDoNotBlockTheBraces` (spec example 6) | FMT-SWITCH-014; FC-002 | `AssertRuleResult(input, expected)` | passes |
| `LocalInLambdaBodyDoesNotBlockTheBraces` | FMT-SWITCH-014; FC-002 | `AssertRuleResult(input, expected)` | passes |
| `PatternVariableInElseIfConditionDoesNotBlockTheBraces` | FMT-SWITCH-014; FC-002 | `AssertRuleResult(input, expected)` | passes |
| `LocalInNestedSwitchSectionDoesNotBlockTheOuterBraces` | FMT-SWITCH-014; FC-002, FC-027 | `AssertRuleResult(input, expected)` | passes |
| `LocalUsedOnlyInInnerSwitchOfItsOwnSectionDoesNotBlockTheBraces` | FMT-SWITCH-014; FC-027 | `AssertRuleResult(input, expected)` | passes |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue input (`int y = 1;` in section 1, `y = 2;` in section 2) | LF, CRLF | formatter, full pipeline | reproduced | Observed: both sections braced (`{ int y = 1; … }` / `{ y = 2; … }`), `y` out of scope in section 2. Expected: switch unbraced, only the blank line after `int y = 1;` |
| Every other "unchanged" input above (pattern variable, local functions in both directions, `const` as `case K:`, `when L()`, `out var` in `return`/`throw`/`yield return`/`lock`/nested `switch`/expression statement, deconstruction, labeled forms, use in a lambda or existing block of another section) | LF, CRLF | formatter | reproduced | Observed in each failure output: the only difference from the input is that every section is braced. Expected: input unchanged |
| Guarded outer switch with an inner multi-line switch; two switches in one method | LF, CRLF | formatter | reproduced | Observed: the guarded outer switch is braced as well. Expected: only the inner / the second switch braced |

No analyzer reports switch-section braces, so there is no counterpart analyzer surface and no code fix (the skeptic's surface table, `02-skeptic.md`).

### Not covered, and why

- A member name behind `.` that looks like the local (`o.y`): left unspecified by FMT-SWITCH-014's coverage sentence; not pinned.
- `using`/`fixed`/`for`/`while`/`do` headers and embedded `if` statements as declaring places: the self-scoped side is covered by the `foreach` header, the nested block, the lambda, the `else if` condition, and the nested switch section. The other self-scoped headers are not each tested, because no production code exists yet whose dispatch could be read; the implementer's predicate may need one more test per header kind if it dispatches on statement kind (FC-002). Flagged for the auditor.
- Anonymous-method and local-function bodies as declaring places: covered by the same "nested body" side as the lambda test; not tested separately for the same reason.
- FC-004/FC-019/FC-020 (comments, directives): the guard adds no edit; FMT-SWITCH-010 and FMT-SWITCH-007 tests already cover directives and comments in braced sections.

### Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~SwitchSectionSharedLocalTests"
```

## thoenissen/Reihitsu#949

### Tests

File `src/Reihitsu.Formatter.Test/Regression/SwitchCaseBraces/TerminalBreakCommentTests.cs` (new), helper `AssertRuleResult`; RH5020 parity in `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5020CommentsShouldBePrecededByABlankLineFormatterTests.cs` (two methods added), helper `FormatterTestsBase<TAnalyzer>.VerifyFormatter` (LF and CRLF, second pass, re-analysis of the output).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `CommentBeforeTerminalBreakGetsBlankLineInThePassThatInsertsTheBraces` (issue input and expected output = spec example 1) | FMT-SWITCH-007, FMT-BLANK-011, FMT-PIPE-004; FC-034, FC-017 | `AssertRuleResult(input, expected)` | fails |
| `BlankLineBetweenCommentsBeforeTerminalBreakIsKept` (spec example 2, maintainer answer to skeptic Q2) | FMT-SWITCH-007 | `AssertRuleResult(input, expected)` | fails |
| `DelimitedCommentBeforeTerminalBreakGetsBlankLineInThePassThatInsertsTheBraces` | FMT-SWITCH-007, FMT-BLANK-011, FMT-PIPE-004; FC-016 | `AssertRuleResult(input, expected)` | fails |
| `CommentBeforeTerminalBreakOfSectionBracedByAnotherSectionGetsBlankLine` (one-line section with two labels, braced because another section is multi-line) | FMT-SWITCH-007, FMT-BLANK-011, FMT-PIPE-004; FC-016 | `AssertRuleResult(input, expected)` | fails |
| `TrailingCommentAndOwnLineCommentBeforeTerminalBreakAreBothKept` | FMT-SWITCH-007, FMT-BLANK-011, FMT-PIPE-004; FC-021 | `AssertRuleResult(input, expected)` | fails |
| `CommentBeforeTerminalBreakInUnbracedSwitchGetsBlankLine` | FMT-BLANK-011; FC-003 | `AssertRuleResult(input, expected)` | passes |
| `CommentAsFirstContentOfBreakOnlySectionGetsNoBlankLine` | FMT-BLANK-011; FC-003 | `AssertRuleResult(input)` | passes |
| `AlreadyBracedSectionWithCommentBeforeTerminalBreakIsUnchanged` (braced form of spec example 2) | FMT-SWITCH-007; FC-003 | `AssertRuleResult(input)` | passes |
| `RH5020…FormatterTests.VerifyFormatterSeparatesCommentBeforeTerminalBreakOfBracedSection` | FMT-SWITCH-007, FMT-BLANK-011; FC-013 | `VerifyFormatter(input, fixed, diagnostic)` | fails |
| `RH5020…FormatterTests.VerifyFormatterSeparatesCommentBeforeTerminalBreakOfUnbracedSection` | FMT-BLANK-011; FC-013 | `VerifyFormatter(input, fixed, diagnostic)` | passes |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue input (`A(); B(); // before break` / `break;`, other section `C();`) | LF, CRLF | formatter | reproduced | Observed pass 1: `}` directly followed by `// before break`. Expected pass 1: blank line between `}` and the comment |
| Same with `/* before break */`; with a trailing comment on `B();`; in a one-line section braced by another section | LF, CRLF | formatter | reproduced | Same difference: no blank line between `}` and the comment |
| `// one` / blank / `// two` / `break;` | LF, CRLF | formatter | reproduced | Observed: `}` / `// one` / `// two` (both blank lines lost). Expected: `}` / blank / `// one` / blank / `// two` |
| Issue input on the RH5020 parity surface | LF (CRLF not reached, the LF arm fails first) | analyzer `FormatterTestsBase.VerifyFormatter` | reproduced | Formatter output differs from the expected output by the missing blank line in front of `// before break` |
| Unbraced shape (`A(); // before break` / `break;`, no multi-line section) | LF, CRLF | formatter and RH5020 parity | not reproduced (correct today) | Blank line inserted in pass 1, stable, RH5020-clean — kept as characterization, as the issue asks for both shapes |

### Not covered, and why

- RH7501's fix output `}` / `// comment` / `break;` (skeptic follow-up): the fix does not call the formatter; outside #949's construct. Follow-up candidate, not tested.
- Blank lines between comments leading the *first* statement moved into the block (skeptic follow-up, FMT-PIPE-013): outside #949's construct and not routed into the run; not tested.
- A directive in front of the terminal `break` (FC-019/FC-020): a section with a directive is not braced (FMT-SWITCH-010), so the rebuilt `break` trivia never carries one; existing FMT-SWITCH-010 tests cover it.

### Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~TerminalBreakCommentTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5020CommentsShouldBePrecededByABlankLineFormatterTests"
```

## thoenissen/Reihitsu#954

### Tests

File `src/Reihitsu.Formatter.Test/Regression/BlankLines/RegionDirectlyAfterOpeningBraceTests.cs` (new), helper `AssertRuleResult`. Corrected existing test: `src/Reihitsu.Formatter.Test/Regression/FullPipeline/RegionFormattingFullPipelineTests.cs` `EmptyRegionInsideBlockIsPreservedAndFormatted` — it pinned the defect (a blank line after `{`); the expected output now has none, and the test is tagged FMT-BLANK-007. Parity: one method added to each of `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5022OpeningBraceMustNotBeFollowedByBlankLineFormatterTests.cs` and `RH5031RegionDirectivesShouldBePrecededByABlankLineFormatterTests.cs`.

Note for every "unchanged" test of mechanism (a): the text is the same after every pass on this head, so a text-level second-pass check cannot see the defect (FC-012); these tests assert the exact FMT-BLANK-007 output of the first pass instead, which fails on this head.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `EmptyRegionAfterTypeBodyBraceGetsNoBlankLineAfterTheBrace` (issue input / expected) | FMT-BLANK-007, SHARED-BLANK-005, SHARED-BLANK-006; FC-034 | `AssertRuleResult(input, expected)` | fails |
| `FormattedEmptyRegionAfterTypeBodyBraceIsUnchanged` (FMT-BLANK-007 example output) | FMT-BLANK-007, SHARED-BLANK-005; FC-012 | `AssertRuleResult(input)` | fails |
| `EmptyRegionAfterMethodBodyBraceGetsNoBlankLineAfterTheBrace` | FMT-BLANK-007, SHARED-BLANK-005; FC-016 | unchanged | fails |
| `EmptyRegionAfterNamespaceBraceGetsNoBlankLineAfterTheBrace` | FMT-BLANK-007, SHARED-BLANK-005; FC-016 | unchanged | fails |
| `EmptyRegionAfterEnumBraceGetsNoBlankLineAfterTheBrace` | FMT-BLANK-007, SHARED-BLANK-005; FC-016 | unchanged | fails |
| `EmptyRegionAfterStatementBlockBraceGetsNoBlankLineAfterTheBrace` | FMT-BLANK-007, SHARED-BLANK-005; FC-016 | unchanged | fails |
| `EmptyRegionAfterSwitchStatementBraceGetsNoBlankLineAfterTheBrace` | FMT-BLANK-007, SHARED-BLANK-005; FC-016 | unchanged | fails |
| `EmptyRegionAfterAccessorBodyBraceGetsNoBlankLineAfterTheBrace` | FMT-BLANK-007, SHARED-BLANK-005; FC-016 | unchanged | fails |
| `CommentFirstAfterBraceIsSeparatedFromEmptyRegionBelow` (FMT-BLANK-013 example, maintainer answer Q2) | FMT-BLANK-013, FMT-BLANK-007, SHARED-BLANK-005; FC-034 | `AssertRuleResult(input, expected)` | fails |
| `CommentFirstAfterBraceIsSeparatedFromRegionHoldingMember` | FMT-BLANK-013, SHARED-BLANK-005; FC-016 | `AssertRuleResult(input, expected)` | fails |
| `CommentFirstAfterMethodBraceIsSeparatedFromRegionHoldingStatement` | FMT-BLANK-013, SHARED-BLANK-005; FC-016 | `AssertRuleResult(input, expected)` | fails |
| `TwoCommentsFirstAfterBraceAreSeparatedFromRegionBelow` | FMT-BLANK-013, SHARED-BLANK-005; FC-016 | `AssertRuleResult(input, expected)` | fails |
| `DelimitedCommentFirstAfterBraceIsSeparatedFromEmptyRegionBelow` | FMT-BLANK-013, FMT-BLANK-007, SHARED-BLANK-005; FC-016 | `AssertRuleResult(input, expected)` | fails |
| `PragmaFirstAfterBraceIsSeparatedFromEmptyRegionBelow` (both mechanisms) | FMT-BLANK-013, FMT-BLANK-007, SHARED-BLANK-005; FC-020 | `AssertRuleResult(input, expected)` | fails |
| `EmptyRegionBelowBraceWithTrailingCommentGetsBlankLine` | SHARED-BLANK-005; FC-003 | `AssertRuleResult(input, expected)` | passes |
| `InactiveEmptyRegionDirectlyAfterOpeningBraceIsUnchanged` | SHARED-BLANK-007, FMT-BLANK-007; FC-020 | `AssertRuleResult(input)` | passes |
| `ConditionalDirectivesDirectlyInsideBracesAreUnchanged` | FMT-BLANK-007; FC-003, FC-020 | `AssertRuleResult(input)` | passes |
| `RegionFormattingFullPipelineTests.EmptyRegionInsideBlockIsPreservedAndFormatted` (corrected) | FMT-BLANK-007 | `AssertRuleResult(input, expected)` | fails |
| `RH5022…FormatterTests.VerifyFormatterKeepsEmptyRegionDirectlyAfterOpeningBrace` | FMT-BLANK-007; FC-013 | `VerifyFormatter(source)` | fails |
| `RH5031…FormatterTests.VerifyFormatterSeparatesCommentFirstAfterBraceFromRegion` | FMT-BLANK-013, SHARED-BLANK-005; FC-013 | `VerifyFormatter(input, fixed, diagnostics)` | fails |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue input (`{` / `#region A` / `#endregion // A` / `}`) | LF, CRLF | formatter | reproduced | Observed: `{` / blank / `#region A` / blank / `#endregion // A`. Expected: no blank line after `{` |
| Already formatted empty region after `{` in a type, method, namespace, enum, statement block, switch block, accessor body | LF, CRLF | formatter | reproduced | Observed: a blank line is inserted after `{` |
| `// c` (or `/* c */`, or `#pragma`) first after `{`, then an empty region | LF, CRLF | formatter | reproduced | Observed: blank line after `{`, and the comment / pragma directly above `#region`. Expected: no blank line after `{`, one between the comment / pragma and `#region` |
| `// c` (or two comments) first after `{`, then a region holding a member or a statement | LF, CRLF | formatter | reproduced | Observed: comment directly above `#region`. Expected: blank line between them |
| Formatted empty region, RH5022 parity | LF (CRLF not reached) | analyzer `VerifyFormatter(source)` | reproduced | Formatter adds a blank line after `{` to RH5022-clean code |
| `// c` first after `{`, RH5031 parity | LF (CRLF not reached) | analyzer `VerifyFormatter` | reproduced | Formatter output `{` / blank / `// c` / `#region A`, still reported by RH5031 |
| `{ // x` then empty region; `#if X` / `#endif` inside braces; inactive region inside `#if NEVER` | LF, CRLF | formatter | not reproduced (correct today) | Kept as boundaries (FC-003) |

### Not covered, and why

- A `///` documentation comment first after `{` above `#region` (skeptic, mechanism b): the documentation-comment phase belongs to concurrent run R2; not pinned.
- Start of file (`// header` then `#region A`, skeptic follow-up): outside #954's construct; the lead decides the scope.
- Lambda and local-function block bodies, struct/record/interface bodies, constructors and operators (mechanism a candidates): the brace owners tested above span the type-declaration, namespace, enum, method/accessor body, statement-block, and switch-block forms; the remaining owners are not each tested, because no production code exists yet whose dispatch could prove one call site. Flagged for the auditor.

### Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~RegionDirectlyAfterOpeningBraceTests|FullyQualifiedName~EmptyRegionInsideBlockIsPreservedAndFormatted"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5022OpeningBraceMustNotBeFollowedByBlankLineFormatterTests|FullyQualifiedName~RH5031RegionDirectivesShouldBePrecededByABlankLineFormatterTests"
```

## thoenissen/Reihitsu#1390

### Tests

New files, helper `AssertRuleResult`:

- `src/Reihitsu.Formatter.Test/Regression/Structural/AsyncUnknownTaskTypeExpressionBodyTests.cs` (FMT-STRUCT-025, `async` alias)
- `src/Reihitsu.Formatter.Test/Regression/Structural/ExpressionBodyArrowCommentTests.cs` (FMT-STRUCT-006, comments around the arrow)
- `src/Reihitsu.Formatter.Test/Regression/LineBreaks/WithInitializerLayoutTests.cs` (FMT-BREAK-021, `with` initializer)

Corrected existing tests whose expected values contradicted the clarified FMT-STRUCT-006 (named by the skeptic), now tagged FMT-STRUCT-006:

- `src/Reihitsu.Formatter.Test/Regression/Structural/ExpressionBodiedMethodTests.cs` `PreservesCommentBeforeArrow` — was `int M()` / blank / `// note` / `{`; now `{` / `// note` / `return 42;`.
- `src/Reihitsu.Formatter.Test/Regression/Structural/ExpressionBodiedConstructorTests.cs` `PreservesCommentBeforeArrow` — same correction with `_x = 1;`.
- `src/Reihitsu.Formatter.Test/Regression/Structural/ExpressionBodyNextLineExpressionTests.cs` `LineCommentBeforeNextLineExpressionKeepsOwnLine` — was `return` / blank / `// c` / `42;`; now `// c` / `return 42;`.
- (revision 2) Same file, `BlockCommentLeadingNextLineExpressionKeepsOwnLine` — was `return` / blank / `/* c */ 42;`; now `return /* c */ 42;` (FMT-STRUCT-006 as answered by the maintainer, Q3 → A). Missed in revision 1.

Code-fix tests added to `src/Reihitsu.Analyzer.Test/Formatting/RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.cs` and `RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests.cs` (both `BatchCodeFixTestsBase`).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `AsyncMethodReturningTaskAliasKeepsExpressionBody` (issue input) | FMT-STRUCT-025, FMT-PIPE-003; FC-034 | `AssertRuleResult(input)` | fails |
| `AsyncMethodsReturningAliasesAndCustomTaskTypeKeepExpressionBodies` (spec example 1) | FMT-STRUCT-025 | unchanged | fails |
| `AsyncMethodReturningGenericCustomTaskTypeKeepsExpressionBody` (`MyTask<int>`) | FMT-STRUCT-025; FC-016 | unchanged | fails |
| `AsyncLocalFunctionReturningTaskAliasKeepsExpressionBody` | FMT-STRUCT-025, FMT-PIPE-003; FC-025 | unchanged | fails |
| `StaticAsyncLocalFunctionReturningTaskAliasKeepsExpressionBody` | FMT-STRUCT-025; FC-025 | unchanged | fails |
| `GenericAsyncMethodReturningTaskAliasKeepsExpressionBody` | FMT-STRUCT-025; FC-025 | unchanged | fails |
| `AsyncMethodReturningTaskAliasWithThrowExpressionKeepsExpressionBody` | FMT-STRUCT-025 | unchanged | fails |
| `AsyncMethodsReturningNullableTaskTypesAreConverted` (spec example 3, maintainer answer to skeptic Q1) | FMT-STRUCT-025, FMT-STRUCT-004, FMT-PIPE-003 | `AssertRuleResult(input, expected)` | fails |
| `AsyncLocalFunctionReturningNullableTaskIsConverted` | FMT-STRUCT-025, FMT-STRUCT-004; FC-025 | `AssertRuleResult(input, expected)` | fails |
| `AsyncMethodsReturningQualifiedTaskTypesAreConverted` (spec example 2) | FMT-STRUCT-025, FMT-STRUCT-004; FC-003 | `AssertRuleResult(input, expected)` | passes |
| `AsyncMethodReturningGlobalQualifiedTaskIsConverted` | FMT-STRUCT-025, FMT-STRUCT-004; FC-003 | `AssertRuleResult(input, expected)` | passes |
| `NonAsyncMethodReturningTaskAliasIsConvertedToReturn` | FMT-STRUCT-004; FC-003 | `AssertRuleResult(input, expected)` | passes |
| `AsyncVoidMethodIsConvertedToExpressionStatement` | FMT-STRUCT-004; FC-003 | `AssertRuleResult(input, expected)` | passes |
| `CommentsAroundTheArrowLeadTheCreatedStatement` (spec example, four members) | FMT-STRUCT-006 | `AssertRuleResult(input, expected)` | fails |
| `CommentBeforeArrowLeadsTheCreatedReturnStatement` (issue shape) | FMT-STRUCT-006; FC-034 | `AssertRuleResult(input, expected)` | fails |
| `CommentAfterArrowLeadsTheCreatedReturnStatement` (issue shape) | FMT-STRUCT-006; FC-034 | `AssertRuleResult(input, expected)` | fails |
| `CommentAfterArrowLeadsTheCreatedExpressionStatement` | FMT-STRUCT-006; FC-016 | `AssertRuleResult(input, expected)` | fails |
| `CommentAfterArrowLeadsTheCreatedThrowStatement` | FMT-STRUCT-006; FC-016 | `AssertRuleResult(input, expected)` | fails |
| `CommentInFrontOfExpressionOnItsLineJoinsTheReturnKeyword` (maintainer answer Q3) | FMT-STRUCT-006 | `AssertRuleResult(input, expected)` | fails |
| `CommentsBeforeAndAfterArrowLeadTheStatementInSourceOrder` | FMT-STRUCT-006 | `AssertRuleResult(input, expected)` | fails |
| `BlankLineBetweenTwoCommentsAfterArrowIsKept` | FMT-STRUCT-006, FMT-PIPE-013 | `AssertRuleResult(input, expected)` | fails |
| `MultiLineDelimitedCommentBeforeArrowLeadsTheCreatedStatement` | FMT-STRUCT-006; FC-021 | `AssertRuleResult(input, expected)` | fails |
| `CommentBeforeArrowOfLocalFunctionLeadsTheCreatedStatement` | FMT-STRUCT-006; FC-025 | `AssertRuleResult(input, expected)` | fails |
| `CommentAfterArrowOfLocalFunctionLeadsTheCreatedStatement` | FMT-STRUCT-006; FC-025 | `AssertRuleResult(input, expected)` | fails |
| `CommentBeforeArrowOfFinalizerLeadsTheCreatedStatement` | FMT-STRUCT-006; FC-025 | `AssertRuleResult(input, expected)` | fails |
| `CommentBeforeArrowOfOperatorLeadsTheCreatedStatement` | FMT-STRUCT-006; FC-025 | `AssertRuleResult(input, expected)` | fails |
| `CommentBeforeArrowOfConversionOperatorLeadsTheCreatedStatement` | FMT-STRUCT-006; FC-025 | `AssertRuleResult(input, expected)` | fails |
| `CommentBeforeArrowOfMethodWithConstraintClauseLeadsTheCreatedStatement` | FMT-STRUCT-006; FC-027 | `AssertRuleResult(input, expected)` | fails |
| `ExpressionBodiedMethodTests.PreservesCommentBeforeArrow` (corrected) | FMT-STRUCT-006 | `AssertRuleResult(input, expected)` | fails |
| `ExpressionBodiedConstructorTests.PreservesCommentBeforeArrow` (corrected) | FMT-STRUCT-006; FC-025 | `AssertRuleResult(input, expected)` | fails |
| `ExpressionBodyNextLineExpressionTests.LineCommentBeforeNextLineExpressionKeepsOwnLine` (corrected) | FMT-STRUCT-006 | `AssertRuleResult(input, expected)` | fails |
| `ExpressionBodyNextLineExpressionTests.BlockCommentLeadingNextLineExpressionJoinsReturnKeyword` (corrected, revision 2; renamed in revision 3) | FMT-STRUCT-006 | `AssertRuleResult(input, expected)` | fails on 69e28ac, passes on 2dea131 |
| `WithInitializersAreLaidOutOverSeveralLinesInTheFirstPass` (spec example) | FMT-BREAK-021, FMT-PIPE-004 | `AssertRuleResult(input, expected)` | fails |
| `WithInitializerOfExpressionBodiedPropertyIsLaidOutInTheFirstPass` (issue input) | FMT-BREAK-021, FMT-PIPE-004; FC-034 | `AssertRuleResult(input, expected)` | fails |
| `WithInitializerOfReturnStatementIsLaidOutInTheFirstPass` | FMT-BREAK-021, FMT-PIPE-004; FC-027 | `AssertRuleResult(input, expected)` | fails |
| `WithInitializerOfConvertedExpressionBodiedMethodIsLaidOutInTheFirstPass` | FMT-BREAK-021, FMT-PIPE-004; FC-027 | `AssertRuleResult(input, expected)` | fails |
| `WithInitializerWithNestedObjectInitializerIsLaidOutInTheFirstPass` | FMT-BREAK-021, FMT-PIPE-004; FC-027 | `AssertRuleResult(input, expected)` | fails |
| `OneElementWithInitializerIsLaidOutOverSeveralLines` | FMT-BREAK-021; FC-003 | `AssertRuleResult(input, expected)` | passes |
| `LaidOutWithInitializerIsUnchanged` | FMT-BREAK-021; FC-003 | `AssertRuleResult(input)` | passes |
| `RH3202…VerifyAsyncMethodReturningTaskAliasIsReportedWithoutCodeFix` (SHARED-BODY-004 example) | SHARED-BODY-004; FC-009 | `VerifyNoCodeFixIsOffered` | fails |
| `RH3202…VerifyAsyncMethodReturningGenericTaskAliasIsReportedWithoutCodeFix` | SHARED-BODY-004; FC-009 | `VerifyNoCodeFixIsOffered` | fails |
| `RH3202…VerifyAsyncMethodsReturningNullableTaskTypesAreFixed` | SHARED-BODY-004, SHARED-BODY-003; FC-006 | `Verify(source, fixed, …)` | fails |
| `RH3202…VerifyFixLeavesAsyncMethodReturningTaskAliasNextToConvertedMethods` (two convertible members and one kept member; incremental and Fix All) | SHARED-BODY-004; FC-008 | `Verify(source, fixed, config)` with the kept diagnostic marked in the fixed code and counted (`FixedState` / `BatchFixedState.MarkupHandling = MarkupMode.Allow`, revision 2) | fails |
| `RH3202…VerifyCommentAfterArrowLeadsTheCreatedStatement` | SHARED-BODY-003; FC-032 | `Verify(source, fixed, …)` | fails |
| `RH3202…VerifyCommentBeforeArrowLeadsTheCreatedStatement` | SHARED-BODY-003; FC-032 | `Verify(source, fixed, …)` | fails |
| `RH3208…VerifyAsyncLocalFunctionReturningTaskAliasIsReportedWithoutCodeFix` (SHARED-BODY-004 example) | SHARED-BODY-004; FC-009 | `VerifyNoCodeFixIsOffered` | fails |
| `RH3208…VerifyStaticAsyncLocalFunctionReturningTaskAliasIsReportedWithoutCodeFix` | SHARED-BODY-004; FC-009, FC-025 | `VerifyNoCodeFixIsOffered` | fails |
| `RH3208…VerifyAsyncLocalFunctionReturningNullableTaskIsFixed` | SHARED-BODY-004, SHARED-BODY-003; FC-006 | `Verify(source, fixed, …)` | fails |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `using T = System.Threading.Tasks.Task;` / `async T RunAsync() => await T.Delay(1);` (method, local function, `static` local function, generic method) | LF, CRLF | formatter | reproduced | Observed: `{ return await T.Delay(1); }` (CS1997). Expected: expression body kept |
| Alias `V` of `ValueTask<int>`, custom `MyTask`, `MyTask<int>` | LF, CRLF | formatter | reproduced | Observed: converted to `return await …`. Expected: kept (FMT-STRUCT-025) |
| `async T` with a throw expression | LF, CRLF | formatter | reproduced | Observed: converted to a `throw` statement. Expected: kept (FMT-STRUCT-025 names no exception for throw expressions) |
| `async Task? RunAsync() => await Task.Delay(1);` (method and local function) | LF, CRLF | formatter | reproduced | Observed: `return await Task.Delay(1);` (CS1997). Expected: `await Task.Delay(1);` |
| Alias `T` on RH3202 / RH3208 | LF (code-fix verifier) | analyzer + code fix | reproduced | Reported (correct); observed one code action registered, expected none (SHARED-BODY-004) |
| `Task?` on RH3202 / RH3208 | LF | code fix | reproduced | Observed fix output `return await Task.Delay(1);`; expected `await Task.Delay(1);` |
| Mixed document, Fix All | LF | code fix | reproduced | Observed: the alias member is converted to `{ return await T.Delay(1); }`; expected: it stays as written and stays reported |
| Comment on its own line before the arrow (method, local function, constructor, finalizer, operator, conversion operator, constraint clause, multi-line `/* */`) | LF, CRLF | formatter | reproduced | Observed: `signature` / blank / comment / `{`. Expected: comment inside the block in front of the statement |
| Comment on its own line after the arrow, `return` form (and two comments with a blank line between them) | LF, CRLF | formatter | reproduced | Observed: `return` / blank / comment(s) / `42;`. Expected: comment(s) / `return 42;` |
| Comment after the arrow, expression-statement and `throw` forms | LF, CRLF | formatter | reproduced | Observed: `{        // run` (comment glued to `{`). Expected: `{` / `// run` / `Foo();` |
| `int Value() =>` / `/* c */ 42;` | LF, CRLF | formatter | reproduced | Observed: `return` / blank / `/* c */ 42;`. Expected: `return /* c */ 42;` |
| Comments around the arrow on RH3202 | LF | code fix | reproduced | Same differences as the formatter (the fix delegates to it) |
| `R P => _r with { A = 1, B = 2 };`, and the `with` in a local, a `return`, a converted method, and with a nested object initializer | LF, CRLF | formatter | reproduced | Observed pass 1: `with {` on one line, elements and `}` already on their own lines. Expected pass 1: `{` on its own line under `with` |
| One-element `with`, already laid-out `with`, qualified / `global::` task types, non-`async` alias, `async void` | LF, CRLF | formatter | not reproduced (correct today) | Kept as boundaries (FC-003) |

### Not covered, and why

- A comment on the arrow's line (`int Get() => // why`): thoenissen/Reihitsu#963, unspecified by FMT-STRUCT-006; the existing `PreservesCommentAfterArrow` (`{// why`) and the RH3208 `VerifyCommentsAroundExpressionBodySurviveFix` tests keep pinning today's output and were not touched.
- A comment in front of the expression on its own line where the expression becomes an expression statement or `throw` (`void Run() =>` / `/* c */ Foo();`) and a `///` comment around the arrow: unspecified by FMT-STRUCT-006; not pinned.
- `int Get3()` / `// own` / `=> /* glued */ 3;` (mixed specified and unspecified parts): not pinned.
- A constructor with `: base()` and a comment before the arrow: the constructor initializer's own layout is not specified in a form I could derive the expected output from without running the code; the constructor kind is covered by the corrected `ExpressionBodiedConstructorTests.PreservesCommentBeforeArrow`.
- Members kept by FMT-STRUCT-007 / FMT-STRUCT-025 with a comment before the arrow: the layout of a kept, wrapped expression body is not specified; not pinned.
- `ValueTask?` / `ValueTask<int>?` under `async`: `Nullable<ValueTask>` is not a task-like type, so the input does not compile (CS1983); not tested. The spec's `Task?` / `Task<int>?` forms are tested.
- An alias named `Task` for a result-carrying type (skeptic follow-up, residual risk of a syntax-only check): not tested.
- `new Foo { Inner = { A = 1, B = 2 } }` (skeptic follow-up, same two-pass mechanism, outside #1390's construct): not tested.
- FC-015 (one owner for the FMT-STRUCT-025 predicate, shared by the formatter and the code-fix base but not by the analyzers' reporting predicate): structural, not testable by behavior beyond the tests above (formatter keeps, analyzer reports, fix withheld). Flagged for the auditor.
- RH3203/RH3205/RH3206/RH3207 fixes with comments around the arrow (FC-032): their output follows the formatter; the formatter tests above cover the constructor, operator, conversion-operator and finalizer kinds. The existing tests of those fixes will show whether their pinned outputs change.

### Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~AsyncUnknownTaskTypeExpressionBodyTests|FullyQualifiedName~ExpressionBodyArrowCommentTests|FullyQualifiedName~WithInitializerLayoutTests|FullyQualifiedName~PreservesCommentBeforeArrow|FullyQualifiedName~LineCommentBeforeNextLineExpressionKeepsOwnLine"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests|FullyQualifiedName~RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests"
```

## thoenissen/Reihitsu#888

### Tests

File `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ChainLinkBehindMultiLineCommentTests.cs` (new), helper `AssertRuleResult`. RH5201 code fix: two methods and two constants added to `src/Reihitsu.Analyzer.Test/Formatting/RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs`, using `Verify(source, fixed, config, …)` with `NumberOfIncrementalIterations = 1` and `NumberOfFixAllIterations = 1`, once with LF and once with `NormalizeToCarriageReturnLineFeed`. Formatter parity: one method added to `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5201MethodChainsShouldBeAlignedFormatterTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ReportedLinkBehindMultiLineCommentTailStartsItsOwnLine` (issue input exactly) | FMT-BREAK-019, SHARED-COMMENT-012; FC-034 | `AssertRuleResult(input, expected)` | fails |
| `LinkBehindMultiLineCommentTailStartsItsOwnLine` (spec example) | FMT-BREAK-019 | `AssertRuleResult(input, expected)` | fails |
| `LinkBehindIndentedMultiLineCommentTailStartsItsOwnLine` (issue variant, `y */` at column 20) | FMT-BREAK-019, SHARED-COMMENT-012; FC-034 | `AssertRuleResult(input, expected)` | fails |
| `PropertyLinkBehindMultiLineCommentTailStartsItsOwnLine` (`.E`) | FMT-BREAK-019, SHARED-COMMENT-012; FC-016 | `AssertRuleResult(input, expected)` | fails |
| `ConditionalAccessLinkBehindMultiLineCommentTailStartsItsOwnLine` (`?.D()`) | FMT-BREAK-019, SHARED-COMMENT-012; FC-016 | `AssertRuleResult(input, expected)` | fails |
| `LinkBehindCommentStartingOnPreviousLinkLineStartsItsOwnLine` | FMT-BREAK-019; FC-003 | `AssertRuleResult(input, expected)` | passes |
| `LinkBehindSingleLineCommentOnItsOwnLineIsUnchanged` | FMT-BREAK-019; FC-003 | `AssertRuleResult(input)` | passes |
| `RH5201…AnalyzerTests.VerifyFixBreaksLinkBehindMultiLineCommentTailInOnePass` | SHARED-COMMENT-012, SHARED-CHAIN-008; FC-007 | `Verify` with one iteration, LF | fails |
| `RH5201…AnalyzerTests.VerifyFixBreaksLinkBehindMultiLineCommentTailInOnePassWithCarriageReturnLineFeed` | SHARED-COMMENT-012, SHARED-CHAIN-008; FC-007, FC-010 | `Verify` with one iteration, CRLF | fails |
| `RH5201…FormatterTests.VerifyFormatterBreaksLinkBehindMultiLineCommentTail` (issue input, diagnostic on `.` of `.D` at 8:12) | FMT-BREAK-019, SHARED-COMMENT-012; FC-013 | `VerifyFormatter(input, fixed, diagnostic)` | fails |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue input (`.C()` / `/* x` / `*/ .D();`, `*/` at column 8) | LF, CRLF | formatter | reproduced | Observed: blank line above `/* x` inserted, `*/ .D();` kept. Expected: `*/` / `.D();` at column 17 |
| `y */ .D()` at column 20; `*/ .E`; `*/ ?.D()` | LF, CRLF | formatter | reproduced | Same: the link stays behind the comment's last line |
| Spec example input, RH5201 code fix | LF and CRLF (separate tests) | code fix (`Verify`, one iteration) | reproduced | Observed fixed text keeps `*/ .D();`. Expected: `*/` / `.D();` in one application |
| Issue input, RH5201 parity | LF (CRLF not reached) | analyzer `VerifyFormatter` | reproduced | Formatter output keeps `*/ .D();` |
| Comment starting on the `.C()` line; single-line `/* x */ .D()` on its own line | LF, CRLF | formatter | not reproduced (correct today) | Kept as boundaries (FC-003) |

### Not covered, and why

- `var r = a.B().C()` / `/* x` / `*/ .D();` (an unwrapped chain whose only "line start" is behind a multi-line comment tail): the skeptic recommends pinning neither output, because Core's wrap detection (`FluentChain.IsWrapped`) disagrees with SHARED-CHAIN-002 + SHARED-COMMENT-012 and is shared with RH5112/RH5201/RH5048; not tested.
- Two comments on the last line (`*/ /* y */ .D()`, FC-021): where `/* y */` goes is not specified; not pinned.
- `!.` / `!?.` operators and `/** */` comments: same link model (`FluentChainLink`) as `.` and `?.`, but no production code exists yet to prove one call site; not tested separately. Flagged for the auditor.
- RH5112's fix (FC-032): delegates to the same chain layout; its existing tests will show any change.

### Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ChainLinkBehindMultiLineCommentTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5201MethodChainsShouldBeAlignedAnalyzerTests|FullyQualifiedName~RH5201MethodChainsShouldBeAlignedFormatterTests"
```

## Whole run

- Every new or changed test file was formatted with `scripts/format.sh` before the runs below.
- `scripts/test.sh --project formatter` on this head, run before the last six #948 declaration-form tests and the inactive-region test were added: 72 failed, 2699 passed. All 72 failures are new or corrected tests listed above as "fails"; no other formatter test fails. The seven tests added afterwards were run on their own: the six #948 tests fail (bracing only), the inactive-region test passes. Total formatter tests expected to fail before the fix: 78.
- `scripts/test.sh --project architecture`: 84 passed (`RequirementCoverageTests` now finds covering tests for FMT-SWITCH-014, FMT-STRUCT-025, and SHARED-BODY-004).
- `scripts/test.sh --project analyzer`: 15 failed, 6051 passed. The 15 failures are exactly the new analyzer-project tests listed above as "fails" (RH3202 ×6, RH3208 ×3, RH5201 code fix ×2, RH5201 / RH5020 / RH5022 / RH5031 parity ×1 each); no existing analyzer test fails.
- No production code was touched. No file of a concurrent run (00-lead.md list) was touched; the RH3202/RH3208 test files belong to the RH3xxx area the maintainer released to this run (Escalation 1, Q1).

### Failure classes walked and left out

- FC-018, FC-036 (renames): no rename in the run.
- FC-022, FC-023 (literals, identifiers): no rewrite in the run touches literal contents or identifier spelling.
- FC-024 (`partial`, explicit interface implementations): FMT-STRUCT-025 decides from the return-type syntax only; the generic-method test covers a non-plain method signature. Not tested further.
- FC-028 (`FormatNode` vs `FormatNodeInDocumentAsync`): the code-fix tests (RH3202/RH3208/RH5201) exercise the document-scoped path, the formatter tests the pipeline path.
- FC-029 (key/value layout): not reached.
- FC-030 (fix formats more than its node): the RH3202 mixed-document test checks that the kept member is not changed by the fix of its neighbours.
- FC-011 (loop without decreasing measure): no loop-until-stable is touched by these tests.
- FC-014: no new rule.
- FC-031, FC-035, FC-037: concern the implementation and evidence, not tests to write; expected values here come from the spec examples.

### Follow-up candidates (outside the run)

- The skeptic's follow-ups stand (RH7501 fix output, FMT-PIPE-013 for the first moved statement, start-of-file comment above `#region`, nested `Inner = { … }` initializer, `RH5201.md` `Uses`).

## Revision 2

Commit: 2dea131 (production), routed by the lead from the implementer's "Disputed tests" (`04-implementer.md`, #1390). The implementer's suggested changes were treated as suggestions; each test was re-derived from the specs on this branch.

1. `ExpressionBodyNextLineExpressionTests.BlockCommentLeadingNextLineExpressionKeepsOwnLine` (existing test, untagged). Input `int M() =>` / `/* c */ 42;`. FMT-STRUCT-006 states that a comment that starts the line after the arrow and is followed on that line by the expression stays directly in front of the expression, which joins `return` with it (`return /* c */ 42;`). This is the maintainer's Q3 → A answer (`00-lead.md`). The test still asserted the old output `return` / blank / `/* c */ 42;`, so the test was wrong. I missed it in revision 1 because I searched by the skeptic's list only. Corrected expected output to `return /* c */ 42;`, tagged FMT-STRUCT-006, summary updated. It now passes on 2dea131.
2. `RH3202…AnalyzerTests.VerifyFixLeavesAsyncMethodReturningTaskAliasNextToConvertedMethods`. SHARED-BODY-004 requires the kept member to stay reported, and the fixed source marks it with `{|RH3202:…|}`. The verifier's default markup handling for the fixed state did not count that markup as an expected diagnostic, so it reported "expected 0, actual 1" although the fixed text matched. The assertion was right; the verifier configuration was wrong. Added `FixedState.MarkupHandling = MarkupMode.Allow` and `BatchFixedState.MarkupHandling = MarkupMode.Allow` (as `RH4107ProtectedFieldCasingAnalyzerTests` does) and `using Microsoft.CodeAnalysis.Testing;`. The expected text, the iteration counts, and the marked diagnostic are unchanged.

Search for further existing tests that pin behavior this run's specs contradict:

- Full formatter suite on 2dea131 plus these corrections: 2778 passed, 0 failed.
- Full analyzer suite on 2dea131 plus these corrections: 6066 passed, 0 failed.
- Static `rg` over the test projects for the pre-fix shapes: `return` followed by a blank line and a comment; a signature, a blank line, a comment, then `{`; `with {` at a line end; `{`, a blank line, then `#region`; `}` directly followed by a comment and `break;`; `/* c */` in front of an expression on the line after `=>`; `async` with a task-type alias or `AsyncMethodBuilder`. There are two hits outside this run's tests, and neither is contradicted:
  - `ExpressionBodiedAccessorTests.CommentBeforeReturnedExpressionKeepsBlock`, FMT-STRUCT-012: block-bodied accessor input that is kept, not an expression body that gets converted.
  - `RH5025…AnalyzerTests.VerifyBlankLineSeparatedByCommentOrBraceOutsideCodeIsNotReported`: analyzer-only input, no formatter involved.

Commands:

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~BlockCommentLeadingNextLineExpressionJoinsReturnKeyword"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~VerifyFixLeavesAsyncMethodReturningTaskAliasNextToConvertedMethods"
```

## Revision 3 — repair round 1

Commit: 1199b0c (tests; production 2dea131; specs at 18b4688, spec author revision 4). Items A1, A2, A3, A5, A6, A7 of `07-auditor.md`, routed by the lead (`00-lead.md`, "Repair round 1"). Each test was re-derived from the specs on this branch; the auditor's suggestions were treated as suggestions. Every formatter test runs under LF and CRLF with a second pass unless stated otherwise.

### thoenissen/Reihitsu#949 — A1

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `TerminalBreakCommentTests.DocumentationCommentBeforeTerminalBreakIsLaidOutInThePassThatInsertsTheBraces` | FMT-SWITCH-007, FMT-BLANK-011, FMT-PIPE-004; FC-016, FC-017 | `AssertRuleResult(input, expected)` | fails |

Derivation: FMT-SWITCH-007 covers every comment that leads the terminal `break`, and FMT-BLANK-011 applies "to ordinary and documentation comments alike". So pass 1 must give `}` / blank / `/// doc` / `break;`.

Observed on 2dea131 (LF): `}` / blank / `/// doc` / **blank** / `break;`. Pass 1 therefore does not match the final layout, and the first-pass assertion fails on the extra blank line below `/// doc`.

### thoenissen/Reihitsu#948 — A2, A3

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SwitchSectionSharedLocalTests.LambdaParameterShadowingTheLocalInAnotherSectionDoesNotBlockTheBraces` | FMT-SWITCH-014; FC-002 | `AssertRuleResult(input, expected)` | fails |
| `SwitchSectionSharedLocalTests.LocalFunctionParameterShadowingTheLocalInAnotherSectionDoesNotBlockTheBraces` | FMT-SWITCH-014; FC-002 | `AssertRuleResult(input, expected)` | fails |
| `SwitchSectionSharedLocalTests.PatternVariableInQueryClauseDoesNotBlockTheBraces` | FMT-SWITCH-014; FC-002 | `AssertRuleResult(input, expected)` | fails |
| `SwitchSectionSharedLocalTests.OutVariableOfLocalDeclarationInitializerUsedInAnotherSectionLeavesTheSwitchUnchanged` | FMT-SWITCH-014; FC-016 | `AssertRuleResult(input)` | passes |
| `SwitchSectionSharedLocalTests.PatternVariableOfLocalDeclarationInitializerUsedInAnotherSectionLeavesTheSwitchUnchanged` | FMT-SWITCH-014; FC-016 | `AssertRuleResult(input)` | passes |

Derivations:

- **A2, shadowing parameters.** Section 2's `y` is bound by the lambda's (or the local function's) own parameter, so it is not a use of section 1's local. FMT-SWITCH-014 says "a local that is used only in the section that declares it MUST NOT trigger this guard". Expected: both sections braced (FMT-SWITCH-001/003). Observed on 2dea131: the whole switch is left unbraced. That is the only difference.
- **A2, query clause.** C# scopes a pattern variable in a query clause to that clause, not to the switch block. The `n` used in section 2 is the field. FMT-SWITCH-014's `Why:` bases the guard on switch-block scoping, so the switch is braced. Observed: left unbraced.
  - Spec note for the lead: read literally, the third bullet ("a variable that … a pattern declares in the initializer of a local declaration") also covers `var q = from a in xs where a is int n select n;`, and the "MUST NOT trigger" list does not name query clauses. If the lead reads the bullet literally, this test asserts against the spec and goes back as escalation 4. I recommend that the spec author name query-clause bodies next to lambda bodies.
- **A3.** The local-declaration initializer form is now named in FMT-SWITCH-014's third bullet (revision 4). These two tests pin the branch the auditor found untested. Both pass on 2dea131.

### thoenissen/Reihitsu#1390 — A5, A6

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ExpressionBodyArrowCommentTests.OwnLineCommentStaysOutOfTheBlockWhenTheArrowLineCarriesALineComment` (`int A2()` / `// a` / `=> // b` / `42;`) | FMT-STRUCT-006; FC-004, FC-021 | `AssertCommentsKeepOrderOutsideTheBlock` (LF, CRLF, two passes) | fails |
| `ExpressionBodyArrowCommentTests.OwnLineCommentStaysOutOfTheBlockWhenADelimitedCommentIsGluedBehindTheArrow` (`int Get3()` / `// own` / `=> /* glued */ 3;`) | FMT-STRUCT-006; FC-004, FC-021 | same | fails |
| `ExpressionBodyArrowCommentTests.OwnLineCommentStaysOutOfTheBlockWhenACommentStandsInFrontOfTheArrow` (`int A4()` / `// a` / `/* c */ => 42;`) | FMT-STRUCT-006; FC-003 | same | passes |
| `RH3202…AnalyzerTests.VerifyFixKeepsCommentOrderWhenTheArrowLineCarriesAComment` (`// a` / `=> // b` / `42;`) | SHARED-BODY-003; FC-004, FC-032 | `ApplyCodeFixAsync`, LF and CRLF, same assertions | fails |
| `ExpressionBodyNextLineExpressionTests.BlockCommentLeadingNextLineExpressionJoinsReturnKeyword` (A6, renamed from `…KeepsOwnLine`; assertion unchanged) | FMT-STRUCT-006; FC-031 | `AssertRuleResult(input, expected)` | passes |

Derivation (A5): FMT-STRUCT-006 (revision 4) says that when the arrow's line holds a comment, the conversion must not move the own-line comments in front of the created statement, and all comments must keep their source order. The placements themselves are unspecified (#963), so these tests assert no layout. The private helper `AssertCommentsKeepOrderOutsideTheBlock` formats with `ReihitsuFormatter.FormatSyntaxTree` and asserts:

- the member is converted (no `=>` left);
- each comment appears exactly once;
- own-line comment < arrow-line comment < statement, by text position;
- the own-line comment does not stand between the block's opening brace and the statement;
- a second pass changes nothing.

Observed on 2dea131:

- `{// b` / `// a` / `return 42;` and `{/* glued */` / `// own` / `return 3;`. The order assertion fails because `// b` comes before `// a`.
- The RH3202 fix produces the same reversal.
- The variant with the comment in front of the arrow already keeps the order.

### thoenissen/Reihitsu#888 — A7

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ChainLinkBehindMultiLineCommentTests.NullForgivingLinkBehindMultiLineCommentTailStartsItsOwnLine` (`*/ !.D()`) | FMT-BREAK-019, SHARED-COMMENT-012; FC-016 | `AssertRuleResult(input, expected)` | passes |
| `ChainLinkBehindMultiLineCommentTests.LinkBehindMultiLineDocumentationStyleCommentTailStartsItsOwnLine` (`/** x` / `*/ .D()`) | FMT-BREAK-019, SHARED-COMMENT-012; FC-016 | `AssertRuleResult(input, expected)` | passes |

Not written, and why: two cases have no test.

- A **prefix link** behind a comment tail (`var r = a.B` / `/* x` / `*/ .B` / `.C()` / `.D();`).
- A **first invoked link** behind a comment tail (`var r = a.B` / `/* x` / `*/ .C()` / `.D();`).

The specs do not decide either output, and the requirements that touch them point in opposite directions:

- **Leave the link behind `*/`.**
  - FMT-BREAK-019's new sentence and SHARED-CHAIN-005 ask for a break only in front of a *later* link, one after the first invoked link.
  - SHARED-CHAIN-005 says prefix links "MUST keep the line the author put them on", and no requirement asks the first invoked link to start a line.
  - SHARED-COMMENT-012 lets the formatter break only "where a requirement of the formatter asks that token to start its own line".
- **Break in front of the link.**
  - SHARED-COMMENT-012's analyzer bullet requires a rule such as RH5201-002 to report such a token whatever its column.
  - SHARED-DIAG-010 requires the formatter's output to be free of such reports.

Observed on 2dea131: the formatter breaks in front of `.B` and in front of `.C()`, and aligns each at the anchor column.

This is a spec question for the lead (escalation 4 / spec author); a test can only be written once it is decided. The `!.` and `/** */` tests are at the later-link site, where the spec is clear.

### Runs

- `scripts/test.sh --project formatter --filter "FullyQualifiedName~TerminalBreakCommentTests|FullyQualifiedName~SwitchSectionSharedLocalTests|FullyQualifiedName~ExpressionBodyArrowCommentTests|FullyQualifiedName~ExpressionBodyNextLineExpressionTests|FullyQualifiedName~ChainLinkBehindMultiLineCommentTests"`: 86 tests; 6 failed, 80 passed. The 6 failures are A1 ×1, A2 ×3, A5 ×2.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests"`: 18 tests; 1 failed (`VerifyFixKeepsCommentOrderWhenTheArrowLineCarriesAComment`, A5).
- `scripts/test.sh --project architecture`: 84 passed.

Expected to fail until the implementer repairs: A1 (1 test), A2 (3 tests), A5 (2 formatter tests and 1 RH3202 test). The A2 query-clause test is subject to the spec note above.

## Revision 4 — maintainer answer on chain links behind a comment tail

Tests written against production 2dea131 and specs at ce7abdd (`01-spec-author.md` revision 5). Sources: the maintainer's answer A, and the routed query-clause clarification, in `00-lead.md`, last section. The expected values come from the new SHARED-CHAIN-005 exception, its example, and FMT-BREAK-019.

### thoenissen/Reihitsu#888 — A7, prefix link and first invoked link

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ChainLinkBehindMultiLineCommentTests.PrefixAndFirstInvokedLinksBehindMultiLineCommentTailStartTheirOwnLines` (SHARED-CHAIN-005 example, verbatim) | SHARED-CHAIN-005, FMT-BREAK-019 | `AssertRuleResult(input, expected)` | passes |
| `ChainLinkBehindMultiLineCommentTests.PrefixLinkBehindMultiLineCommentTailStartsItsOwnLine` (`var r = a.B` / `/* x` / `*/ .B` / `.C()` / `.D();`) | SHARED-CHAIN-005, FMT-BREAK-019, SHARED-COMMENT-012; FC-016 | `AssertRuleResult(input, expected)` | passes |
| `ChainLinkBehindMultiLineCommentTests.FirstInvokedLinkBehindMultiLineCommentTailStartsItsOwnLine` (`var r = a.B` / `/* x` / `*/ .C()` / `.D();`) | SHARED-CHAIN-005, FMT-BREAK-019, SHARED-COMMENT-012; FC-016 | `AssertRuleResult(input, expected)` | passes |
| `ChainLinkBehindMultiLineCommentTests.LinkOfChainWithoutInvokedLinkBehindMultiLineCommentTailStartsItsOwnLine` (`*/ .C` / `.D;`; the exception names this case, and the anchor is the chain's first link `.B`, SHARED-CHAIN-007) | SHARED-CHAIN-005, SHARED-COMMENT-012; FC-016 | `AssertRuleResult(input, expected)` | passes |
| `RH5201…FormatterTests.VerifyFormatterBreaksPrefixLinkBehindMultiLineCommentTail` (RH5201 reports `.B` behind `*/` at 8:12; the formatter output is clean and stable, LF and CRLF) | SHARED-CHAIN-005, SHARED-COMMENT-012; FC-013 | `VerifyFormatter(input, fixed, diagnostic)` | passes |

All five pass on 2dea131: the implementation already breaks at these call sites, as the auditor's probes showed. They now pin behavior that the spec decides.

Not pinned:
- The chain's **first** link behind a comment tail. The exception leaves it to SHARED-CHAIN-003/004, and the spec author's Q4 (revision 5) is open.
- A chain whose only line breaks lie inside such a comment. The exception leaves this unspecified.

### thoenissen/Reihitsu#948 — A2 query clause

`SwitchSectionSharedLocalTests.PatternVariableInQueryClauseDoesNotBlockTheBraces` matches the revised FMT-SWITCH-014. The requirement now names query clauses next to lambda bodies, and its "MUST NOT trigger" list includes "query clause (`from a in xs where a is int n select n`)". So the expected output (both sections braced) stands, unchanged. It still fails on 2dea131 until the implementer repairs A2. The note on this test in revision 3 is resolved.

### Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ChainLinkBehindMultiLineCommentTests|FullyQualifiedName~PatternVariableInQueryClauseDoesNotBlockTheBraces"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5201MethodChainsShouldBeAlignedFormatterTests"
```

Results: formatter 14 tests, 13 passed, 1 failed (the query-clause test, awaiting the A2 repair). Analyzer 8 passed.

## Revision 5 — repair round 2

These tests ran against the production code at 27c08b3, the head of the auditor's repair-round-1 revision (`07-auditor.md`, the note at the end). They cover findings A8 and A2 (narrowed), routed in `00-lead.md` under "Repair round 2". Expected values come from FMT-SWITCH-014 and FMT-PIPE-003. The auditor's suggestions were treated as suggestions.

**Compile check.** Every input of the tests below compiles in its unbraced form, and every braced expected output compiles too. I checked this with a scratch `net10.0` library (`AllowUnsafeBlocks` for the `fixed` case), with helper stubs for `Use`, `A`, `F`, `Next`, `Open`, and `MyAttribute`.

I also compiled the braced form of each A8 input. All four fail with CS0103, at the attribute argument or the default value. So the name there binds to the switch-scoped `const y`, not to the parameter, and the switch must stay unbraced.

### thoenissen/Reihitsu#948 — A8 (switch must stay unchanged)

All four tests are in `SwitchSectionSharedLocalTests`. In each, section 1 is `const int y = 2;` / `Use(y);`.

| Test | Section 2 | Requirement / failure class | Helper | Expected now |
|---|---|---|---|---|
| `AttributeArgumentOfLocalFunctionUsesTheLocalAndLeavesTheSwitchUnchanged` | `[My(y)]` / `int L(int y) { return y; }` / `Use(L(1));` | FMT-SWITCH-014, FMT-PIPE-003; FC-001, FC-006 | `AssertRuleResult(input)` | fails |
| `DefaultValueOfLocalFunctionParameterUsesTheLocalAndLeavesTheSwitchUnchanged` | `int L(int y, int b = y) { return b; }` / `Use(L(1));` | FMT-SWITCH-014, FMT-PIPE-003; FC-001, FC-006 | `AssertRuleResult(input)` | fails |
| `AttributeArgumentOfLambdaUsesTheLocalAndLeavesTheSwitchUnchanged` | `Func<int, int> f = [My(y)] (int y) => y;` / `Use(f(1));` | FMT-SWITCH-014, FMT-PIPE-003; FC-001, FC-006 | `AssertSwitchSectionsStayUnbraced` (LF, CRLF, second pass, no `{` directly after a `case …:` line) | fails |
| `DefaultValueOfLambdaParameterUsesTheLocalAndLeavesTheSwitchUnchanged` | `var f = (int y, int b = y) => b;` / `Use(f(1, 2));` | FMT-SWITCH-014, FMT-PIPE-003; FC-001, FC-006 | `AssertRuleResult(input)` | fails |

Observed at 27c08b3:

- All four switches are braced, which makes the output fail with CS0103.
- For the three exact-output tests, the bracing is the only difference.
- The attributed lambda is checked by sections only, because the layout of a lambda's attribute list is not specified: FMT-BREAK-027 names declarations, not lambdas. Inside the braced output the formatter puts `[My(y)]` and `(int y) => y;` on separate lines. That layout is outside this run and is not pinned.

### thoenissen/Reihitsu#948 — A2 narrowed (both sections braced)

All ten tests are in `SwitchSectionSharedLocalTests`, tagged FMT-SWITCH-014, using `AssertRuleResult(input, expected)`. In each, section 1 is `int y = 1;` / `Use(y);`, and section 2 declares `y` inside a function. The failure classes are FC-002 and FC-016.

| Test | Binder in section 2 | Expected now |
|---|---|---|
| `ForeachVariableInLambdaShadowingTheLocalDoesNotBlockTheBraces` | `Action a = () => { foreach (var y in ys) { Use(y); } };` (the auditor's counterexample) | fails |
| `ForeachVariableInLocalFunctionShadowingTheLocalDoesNotBlockTheBraces` | `foreach (var y in ys)` in a local function | fails |
| `ForVariableInLocalFunctionShadowingTheLocalDoesNotBlockTheBraces` | `for (var y = 0; …)` | fails |
| `UsingVariableInLocalFunctionShadowingTheLocalDoesNotBlockTheBraces` | `using (var y = Open())` | fails |
| `FixedVariableInLocalFunctionShadowingTheLocalDoesNotBlockTheBraces` | `fixed (int* y = ys)` in an `unsafe` local function | fails |
| `CatchVariableInLocalFunctionShadowingTheLocalDoesNotBlockTheBraces` | `catch (Exception y)` | fails |
| `WhileConditionDesignationInLocalFunctionShadowingTheLocalDoesNotBlockTheBraces` | `while (o is int y && y > 0)` | fails |
| `DoConditionDesignationInLocalFunctionShadowingTheLocalDoesNotBlockTheBraces` | `do { … } while (F(out var y) && y > 0);` | fails |
| `SwitchExpressionArmDesignationInLambdaShadowingTheLocalDoesNotBlockTheBraces` | `o => o switch { int y => y, _ => 0 }` (the auditor's counterexample) | fails |
| `NestedSwitchSectionLocalInLocalFunctionShadowingTheLocalDoesNotBlockTheBraces` | `case 1: int y = 2; break;` of a switch inside a local function | passes |

Derivation: in each case the `y` in section 2 is bound by a declaration inside a lambda or local function of that section, so the shared local is used only in section 1. FMT-SWITCH-014 says such a local "MUST NOT trigger this guard", so both multi-line sections are braced (FMT-SWITCH-001/003).

Observed at 27c08b3 for the nine failing tests: the switch is left unbraced, and that is the only difference from the expected output (a diff of expected against actual for each). The nested-switch-section form is already handled and is pinned as passing.

### Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~SwitchSectionSharedLocalTests"
```

Result: 46 tests, 13 failed (A8 ×4, A2 ×9), 33 passed.

`sdd/specs/formatter/structural-transforms.md` has an uncommitted change in the worktree that is not mine (A9 belongs to the spec author). It was not staged.
