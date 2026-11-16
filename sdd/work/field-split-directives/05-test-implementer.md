Role: test implementer
Commit: 707b6b8
Verdict: #1728: tests written; #1586: tests written

Behavior-change lane. Expected values come from the accepted change doc `sdd/changes/2026-11/field-declaration-directives.md` and the specs it changes (FMT-UNSUP-010, FMT-UNSUP-009, FMT-STRUCT-022, FMT-PIPE-031, RH7101-005), never from a patched copy of the code (FC-035). The prompt carried no intended fix, suspected cause, or hoped-for verdict. Every test below was run on 707b6b8 (production code unchanged): each "fails now" test fails on the change's own difference (the formatter formats instead of refusing, the split does not happen, `SplitsFieldDeclaration` answers false, or a fix is offered or withheld the other way), never on a compile error or unrelated layout. "Passes now" tests cover behavior that must stay.

New reason constant for tests: `FormatterTestsBase.FieldDeclarationDirectiveReason` = `a directive inside a field declaration with several declarators` (FMT-UNSUP-010's reason text, copied from the spec as the other three reasons are).

## thoenissen/Reihitsu#1728

### Tests

Formatter, `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedFieldDeclarationDirectiveTests.cs` (new). `AssertUnsupportedFor` / `AssertSupported` run LF and CRLF; `AssertSupported` adds the second-pass check.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecExampleDirectivesInsideCombinedFieldsAreUnsupported` (X defined / not defined) | FMT-UNSUP-010 | `AssertUnsupportedFor` | fails |
| `EachDeclarationOfTheSpecExampleIsUnsupported` (4 rows: group between declarators, pragma between declarators, group between attribute list and type, group inside a lambda value) | FMT-UNSUP-010 | `AssertUnsupportedFor` | fails |
| `DirectiveAtEveryPlaceOfTheDeclarationIsUnsupported` (16 rows: between attribute lists, region between attribute list and modifier, `#nullable` between modifiers, disabled modifier, group inside the type, type/first name, name/`=`, `#if false` between `=` and value, pragma deep in a statement lambda, group in a lambda nested in a collection expression, group in the last value, region between comma and declarator, in front of `;`, inside an attribute argument list, `#if`/`#elif`/`#else`, empty group) | FMT-UNSUP-010; FC-020, FC-039 | `AssertUnsupportedFor` | fails |
| `ActiveConditionalGroupBetweenTheDeclaratorsIsUnsupported` (symbol from the build; `#define` in the file) | FMT-UNSUP-010 | `AssertUnsupportedFor` | fails |
| `ShapeIsUnsupportedInEveryKindOfType` (struct, record, record struct, interface static, constants, fixed-size buffers, nested type) | FMT-UNSUP-010; FC-027 | `AssertUnsupportedFor` | fails |
| `ShapeIsUnsupportedInAUnion` | FMT-UNSUP-010 | `AssertUnsupportedFor` | fails |
| `ShapeIsUnsupportedInAnExtensionBlock` (no syntax error; the declaration's own tokens decide) | FMT-UNSUP-010 | `AssertUnsupportedFor` | fails |
| `DirectiveInsideAFieldWithOneDeclaratorIsSupported` (3 rows, both builds of X) | FMT-UNSUP-010; FC-003 | `AssertSupported` | passes |
| `DeclaratorInDisabledTextDoesNotCountAndAtTheCommaTheReasonOfAListSeparatorWins` | FMT-UNSUP-010, FMT-UNSUP-009 | `AssertSupported`, `AssertUnsupportedFor` | passes |
| `ThirdDeclaratorInAConditionalGroupIsUnsupportedInEveryBuild` (010 reason without X, 009 reason with X) | FMT-UNSUP-010, FMT-UNSUP-009 | `AssertUnsupportedFor` | fails |
| `DirectiveInFrontOfADeclaratorCommaOnALaterLineKeepsTheReasonOfAListSeparator` | FMT-UNSUP-010, FMT-UNSUP-009 | `AssertUnsupportedFor` | passes |
| `EventFieldAndLocalDeclarationAreSupported` (both builds of X) | FMT-UNSUP-010; FC-003 | `AssertSupported` | passes |
| `DirectiveInTheLeadingTriviaOfTheFirstTokenIsSupported` (8 rows: region, group around, active branch before an `#else`, pragma around, `#nullable`, disabled attribute list, disabled modifier, group behind `;`) | FMT-UNSUP-010; FC-003, FC-001 | `AssertSupported` | passes |
| `FirstOfACommentAndADirectiveInTheDeclarationIsReported` | FMT-UNSUP-010, FMT-UNSUP-001 | `AssertUnsupportedFor` | fails (second assertion: today FMT-UNSUP-004 wins) |
| `DocumentedDeclarationWithADirectiveIsUnsupportedInEveryDocumentationMode` (Parse, None, Diagnose) | FMT-UNSUP-010; FC-040 | `AssertUnsupportedFor` | fails |
| `FormatDocumentAsyncRefusesTheShape` | FMT-UNSUP-010, FMT-UNSUP-002 | `FormatDocumentAsync`, LF/CRLF | fails |
| `NodeLevelEntryPointsRefuseATargetThatHoldsTheShape` (`FormatNode`, `FormatNodeInDocumentAsync`, `FormatNodeInDocumentWithContextAsync`; method beside is formatted) | FMT-UNSUP-010, FMT-UNSUP-002; FC-028 | entry points, LF/CRLF | fails |
| `TargetThatHoldsOnlyPartOfTheDeclarationIsNotRefused` (variable declaration, each declarator, the value; as target and detached) | FMT-UNSUP-010, FMT-UNSUP-003; FC-041 | entry points | passes |
| `FindUnsupportedSyntaxAnswersForTheDeclarationAndNotForPartOfIt` | FMT-UNSUP-010, FMT-UNSUP-003, FMT-UNSUP-007 | `FindUnsupportedSyntax` | fails |
| `FindUnsupportedSyntaxReportsTheNestedTypeAndTheOuterType` | FMT-UNSUP-010, FMT-UNSUP-007; FC-039 | `FindUnsupportedSyntax` | fails |
| `FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck` (5 rows) | FMT-UNSUP-010, FMT-UNSUP-007 | `FindUnsupportedSyntax` vs `FormatSyntaxTree` | passes (must keep passing) |
| `OutputOfFormattedDeclarationsIsNotUnsupported` (6 rows: split and kept-whole outputs formatted again) | FMT-UNSUP-005, FMT-UNSUP-010; FC-038 | `FormatSyntaxTree` twice, LF/CRLF | passes (must keep passing) |

Existing tests changed (each asserted an output the change doc changes):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FieldDeclarationSplitTests.DirectiveBeforeFirstDeclaratorIsUnsupported` (was `DirectiveBeforeFirstDeclaratorLeavesTheFieldIntact`, FMT-STRUCT-023) | FMT-UNSUP-010; FC-042 | `AssertUnsupportedFor` | fails |
| `FieldDeclarationSplitTests.DisabledTextBeforeFirstDeclaratorIsUnsupported` (was `DisabledTextBeforeFirstDeclaratorLeavesTheFieldIntact`, FMT-STRUCT-023) | FMT-UNSUP-010; FC-042 | `AssertUnsupportedFor` | fails |
| `DirectivePreservationTests.FieldDeclarationWithDirectivesInDeclarationIsRefused` (was `FieldDeclarationSplitKeepsDirectivesInDeclaration`, FMT-STRUCT-023) | FMT-UNSUP-010; FC-042 | `FormattingPipeline.Execute`, reason and location | fails |
| `UnsupportedVariableDeclarationTests.DirectiveInsideADeclarationIsNoComment` (was `DirectiveInsideADeclarationIsSupported`) — field refused with FMT-UNSUP-010's reason, the same directive in a local not refused | FMT-UNSUP-004, FMT-UNSUP-010 | `AssertUnsupportedFor`, `AssertSupported` | fails |
| `UnsupportedVariableDeclarationTests.CommentLookingDisabledTextInsideADeclarationIsNoComment` (was `...IsSupported`) | FMT-UNSUP-004, FMT-UNSUP-010 | `AssertUnsupportedFor`, `AssertSupported` | fails |
| `ReihitsuFormatterSplitsFieldDeclarationTests.SpecExampleReturnsTrueForACombinedDeclarationAlsoWithADirectiveBetweenItsDeclarators` (was `ReturnsTrueForACombinedDeclarationAndFalseForOneWithADirective`; FMT-PIPE-031's changed first example, `FindUnsupportedSyntax` returns `#if DEBUG` for the declaration and the class) | FMT-PIPE-031 | `SplitsFieldDeclaration`, `FindUnsupportedSyntax` | fails |
| `Tooling CodeFixRunCommandTests.ExecuteAsyncReportsNoFixOfferedForDirectiveBearingFieldAtSuccessExitCode` — fixture moved from `#if !NEVER` around the field (now fixed) to a pragma between the declarators (withheld under FMT-UNSUP-010); untagged, it tests the tool | — | tool run | passes (must keep passing) |

Code-fix tests:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7101...SpecExampleDirectiveInsideAnotherDeclarationOfTheTypeWithholdsTheFix` | RH7101-005, SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | fails |
| `RH7101...DirectiveInsideTheDeclarationWithholdsTheFix` (pragma between modifier and type; group in the last declarator's value) | RH7101-005 | `VerifyNoCodeFixIsOffered` | passes |
| `RH7101...DirectiveInANestedTypeWithholdsTheFixOfADeclarationOfTheOuterType` | RH7101-005; FC-043 | `VerifyNoCodeFixIsOffered` | fails |
| `RH7101...DirectiveInTheOuterTypeDoesNotWithholdTheFixOfADeclarationOfANestedType` | RH7101-005, SHARED-FIX-010; FC-043, FC-041 | `Verify` (one pass, Fix All) | passes |
| `RH7101...FixAllSplitsTheDeclarationsBehindDirectivesAndLeavesTheTypeWithADirectiveInsideADeclaration` | RH7101-005, SHARED-FIX-010, SHARED-FIX-002; FC-008 | `Verify` with markup in fixed state | fails |
| `RH7101...PragmaDirectiveBetweenTheDeclaratorsWithholdsTheFix` (existing, unchanged; RH7101-005 example 1) | RH7101-005 | `VerifyNoCodeFixIsOffered` | passes |
| `RH5113...NoFixIsOfferedForACombinedFieldWithADirectiveBetweenItsDeclarators` | SHARED-FIX-010, FMT-UNSUP-010 | `VerifyNoCodeFixIsOffered` | fails |
| `RH5110...NoFixIsOfferedWhenTheConstrainedTypeHoldsACombinedFieldWithADirectiveInside` | SHARED-FIX-010, FMT-UNSUP-010 | `VerifyNoCodeFixIsOffered` | fails |
| `RH5303...FixIsOfferedForAnInitializerInsideACombinedFieldWithADirectiveBetweenItsDeclarators` (value-only fix not refused; pragma and declarators kept) | FMT-UNSUP-003, FMT-UNSUP-010; FC-041 | `TryApplyCodeFixAsync` | passes |

CLI, `src/Reihitsu.Cli.Test/EndToEnd/CliHiddenCodeEndToEndTests.cs`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `DirectiveBetweenTheDeclaratorsOfAFieldIsReportedAsUnsupported` (write mode, `(4,1)`, file unchanged, exit 2) | FMT-UNSUP-010 | `Program.Main` | fails |
| `DirectiveInsideAValueOfACombinedFieldIsReportedAsUnsupported` (`--check`, `(7,1)`) | FMT-UNSUP-010 | `Program.Main` | fails |

### Scenarios (bug fix)

_N/A_ (behavior change).

### Not covered, and why

- **Throw sites of `SyntaxNotSupportedException`** — the change declares no phase refusal (FMT-UNSUP-006 untouched), so there is no throw site to test.
- **RH5111, RH5112, RH5201, RH5404 withheld on code that holds the shape** (change doc, *Code fixes on code that holds the shape are withheld*) — not tested one by one. RH5110 (type as formatted node) and RH5113 (declaration) are; RH5404 formats the type like RH5110, and RH5111/RH5112/RH5201 pass the declaration as context. This is no single-call-site proof: each provider calls `FindUnsupportedSyntax` itself. The predicate they share is covered by the precheck tests, and each rule's existing SHARED-FIX-010 tests cover its own call. The auditor may ask for these four.
- **FC-019** (comment and directive in a gap): the gaps the change touches are the shape itself; directives are covered in every gap, and FMT-UNSUP-004 comments in the same declaration by `FirstOfACommentAndADirectiveInTheDeclarationIsReported`.
- **FC-004, FC-021**: no tokens are joined or moved by the refusal; the split side is #1586's.
- **FC-006, FC-022–FC-026, FC-029, FC-036, FC-018**: no rewrite of semantics, literals, identifiers, members, key/value layout, or names is added.
- **FC-010–FC-014**: phase stability is covered through `AssertRuleResult` (which runs `PhaseStabilityCheck`) on every split output; no new rule or phase.
- **FC-031, FC-037**: documentation comments in production code are not testable; the auditor checks them.
- **FC-032**: every result above was gathered on 707b6b8.
- **FC-042** in production code (`CarriesDirective` and its operand, doc sentences): code-level. The tests that see it: `ReturnsTrueForACombinedDeclarationWithADirectiveAndNoDisabledTextInItsLeadingTrivia` rows "Pragma directive between the declarators" and "Disabled text in the leading trivia of a later token" fail as long as the directive operand covers more than the first token's leading trivia.

### Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~UnsupportedFieldDeclarationDirectiveTests|FullyQualifiedName~FieldDeclarationSplitTests|FullyQualifiedName~DirectivePreservationTests|FullyQualifiedName~ReihitsuFormatterSplitsFieldDeclarationTests|FullyQualifiedName~UnsupportedVariableDeclarationTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7101DoNotCombineFieldsAnalyzerTests|FullyQualifiedName~RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzerTests|FullyQualifiedName~RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzerTests|FullyQualifiedName~RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests"
scripts/test.sh --project cli --filter "FullyQualifiedName~CliHiddenCodeEndToEndTests"
scripts/test.sh --project tooling --filter "FullyQualifiedName~ExecuteAsyncReportsNoFixOfferedForDirectiveBearingFieldAtSuccessExitCode"
```

### Follow-up candidates

_None_ beyond the spec author's (event field / local declarator behind an `#endif` loses its continuation indent); `EventFieldAndLocalDeclarationAreSupported` asserts only that those inputs are not refused and are stable, not their columns.

## thoenissen/Reihitsu#1586

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FieldDeclarationSplitTests.SpecExampleRegionAndGroupAroundTheDeclarationAreSplit` | FMT-STRUCT-022 | `AssertRuleResult` (LF/CRLF, second pass, phase stability) | fails |
| `FieldDeclarationSplitTests.SpecExampleDisabledTextInTheLeadingTriviaKeepsTheDeclarationWhole` (`_a, _b` and `_c, _d` whole; `_m, _n` behind an active `#if D` split) | FMT-STRUCT-022; FC-003 | `AssertRuleResult` | fails (`_m, _n` not split) |
| `FieldDeclarationSplitTests.DisabledTextInTheLeadingTriviaKeepsTheDeclarationWhole` (5 rows: L9 `#if`/`#elif` attributes, L3 member of its own, L4 active `#else`, `#if false` attribute, region plus inactive member) | FMT-STRUCT-022; FC-044, FC-020 | `AssertRuleResult` | passes |
| `FieldDeclarationSplitTests.PragmaDirectiveInFrontOfTheDeclarationStaysOnTheFirstField` (L7) | FMT-STRUCT-022 | `AssertRuleResult` | fails |
| `FieldDeclarationSplitTests.DeclarationInTheActiveBranchInFrontOfADisabledElseBranchIsSplit` | FMT-STRUCT-022; FC-003 | `AssertRuleResult` | fails |
| `FieldDeclarationSplitTests.RegionInFrontOfAnAttributedDeclarationStaysInFrontOfTheFirstField` | FMT-STRUCT-022 | `AssertRuleResult` | fails |
| `FieldDeclarationSplitTests.RegionAndCommentInFrontOfTheDeclarationStayInFrontOfTheFirstField` | FMT-STRUCT-022 | `AssertRuleResult` | fails |
| `ReihitsuFormatterSplitsFieldDeclarationTests.SpecExampleReturnsFalseForDisabledTextInTheLeadingTrivia` (new FMT-PIPE-031 example) | FMT-PIPE-031 | `SplitsFieldDeclaration`, `FindUnsupportedSyntax` | fails |
| `ReihitsuFormatterSplitsFieldDeclarationTests.ReturnsTrueForACombinedDeclarationWithADirectiveAndNoDisabledTextInItsLeadingTrivia` (6 rows; replaces `ReturnsFalseForACombinedDeclarationThatCarriesADirective`) | FMT-PIPE-031; FC-001, FC-003 | `SplitsFieldDeclaration` | fails |
| `ReihitsuFormatterSplitsFieldDeclarationTests.ReturnsFalseForACombinedDeclarationWithDisabledTextInItsLeadingTrivia` (7 rows) | FMT-PIPE-031; FC-003, FC-044 | `SplitsFieldDeclaration` | passes |
| `RH7101...VerifyFixSplitsAFieldInsideAnActiveConditionalGroup` (replaces `VerifyFixIsNotOfferedWhenFieldCarriesDirective`; DEBUG defined) | RH7101-005, RH7101-004 | `Verify` (one pass, Fix All) | fails |
| `RH7101...SpecExampleDirectiveInFrontOfTheDeclarationDoesNotWithholdTheFix` (LF, CRLF; replaces `RegionDirectiveInTheDeclarationWithholdsTheFix`) | RH7101-005, RH7101-004; FC-007 | `Verify` | fails |
| `RH7101...RegionDirectiveDirectlyInFrontOfTheDeclarationDoesNotWithholdTheFix` (the old test's input, no blank lines) | RH7101-005, RH7101-004 | `Verify` | fails |
| `RH7101...SpecExampleDisabledTextInFrontOfTheDeclarationWithholdsTheFix` | RH7101-005 | `VerifyNoCodeFixIsOffered` | passes |
| `RH7101...DisabledTextInFrontOfTheDeclarationWithholdsTheFix` (5 rows: L6, L9, L3, L4, `#if false`) | RH7101-005; FC-044 | `VerifyNoCodeFixIsOffered` | passes |
| `RH5110...FixSplitsACombinedFieldBehindARegionOfTheConstrainedType` (a fix that formats the type splits it, change doc) | FMT-STRUCT-022 | `Verify` | fails |
| `Cli...CombinedFieldsBehindDirectivesAreSplit` | FMT-STRUCT-022 | `Program.Main` | fails |
| `Cli...CombinedFieldBehindAnInactiveAttributeListStaysWhole` (no symbols: kept whole, not refused) | FMT-STRUCT-022 | `Program.Main` | passes |

The closure of these outputs (FMT-UNSUP-005) is in #1728's `OutputOfFormattedDeclarationsIsNotUnsupported`, and the FMT-UNSUP-010 other side (leading trivia not refused) in `DirectiveInTheLeadingTriviaOfTheFirstTokenIsSupported`; the Fix All with a region-led type is #1728's `FixAllSplits...` test.

### Scenarios (bug fix)

_N/A_ (behavior change).

### Not covered, and why

- **RH5404 split through the type**: same type-level entry point as RH5110, which is covered; not one call site, so the auditor may ask for it.
- **FC-018, FC-036** (naming): no rename.
- **The disabled-text check's exact location** (only the first token's leading trivia): covered from both sides by the FMT-PIPE-031 rows "Disabled text in the leading trivia of a later token" (true) and the `ReturnsFalse...` rows; a mixed region-plus-disabled-text trivia is a row on both the formatter and the FMT-PIPE-031 side.

### Commands

As under #1728 (same filters).

### Follow-up candidates

_None._

## Whole run

- Production code unchanged; tests only. `scripts/format.sh` was run on every changed test file.
- Full runs on 707b6b8: formatter 60 failed / 5295 passed, every failure one of the "fails now" tests above; CLI 3 failed / 304 passed, all three listed above; architecture 153/153 passed (the three retired FMT-STRUCT-023 tags are gone, FMT-UNSUP-010 is covered); tooling test passes; analyzer 10 failed / 10427 passed, every failure one of the "fails now" code-fix tests above.
- Stale-output scan: a throwaway Roslyn scan of every string literal in `src/*Test*` for a field declaration with several declarators and a directive or disabled text in or in front of it, under no symbols, `DEBUG`, and `X`, found the tests above and one more, the tooling test, which was changed. No other test asserts an output the change doc changes.
