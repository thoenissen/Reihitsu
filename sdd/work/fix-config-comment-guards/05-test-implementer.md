Role: test implementer
Commit: beeac8e
Verdict: #1595: reproduced, tests written; #1593: reproduced, tests written; #1616: reproduced, tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict; it pointed at decisions 3, 6, 8, 9, 12–16 of `00-lead.md`, which shaped which rows are tested. Expected values come from the spec examples, the change doc, and the requirements, never from patched code. Every test below was run on beeac8e (production code unchanged): "red" tests fail on the stated difference, "green" tests are counterparts that hold today. Each `[DataRow("\n")]`/`[DataRow("\r\n")]` test runs both line endings; both rows behave the same in every test. A small private `WithEndOfLine(text, endOfLine)` helper was added to each of the five test classes (`#region Methods`). `scripts/format.sh` changed nothing in the five files; `scripts/test.sh --project architecture` passes (115/115, `RequirementCoverageTests` now finds RH2001-009 tagged).

## thoenissen/Reihitsu#1595

File: `src/Reihitsu.Analyzer.Test/Ordering/RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests.cs` (class `RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests`). Inputs use `private const int K` after `private` fields (decision 3); the issue's own `public const` inputs are not reported and are not tested.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NoCodeFixWhenExcludedCodeInFrontOfTheConstantWouldReorderTheInstanceFieldsOfAStruct` (row a2, spec example 1) | RH7102-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| `NoCodeFixWhenExcludedCodeInFrontOfTheConstantWouldReorderTheInstanceFieldsOfARecordStruct` (row o) | RH7102-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| `NoCodeFixWhenExcludedCodeInFrontOfTheConstantWouldReorderTheInstanceFieldsOfASequentialClass` (row i) | RH7102-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| `NoCodeFixWhenAnAutoPropertyOfExcludedCodeInFrontOfTheConstantWouldReorderTheInstanceFieldsOfAStruct` (row g) | RH7102-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| `FixCarriesAnInstanceFieldOfExcludedCodeWithTheConstantInAnExplicitLayoutStruct` (row h, counterpart) | RH7102-006, SHARED-ORDER-007 | `Verify(source, fixed)` | green |
| `FixCarriesAStaticFieldOfExcludedCodeWithTheConstantInAStruct` (row f, counterpart) | RH7102-006, SHARED-ORDER-007 | `Verify(source, fixed)` | green |
| `FixCrossesAnInstanceFieldOfExcludedCodeInAStruct` (row k, counterpart) | RH7102-006, SHARED-ORDER-007 | `Verify(source, fixed)` | green |
| `NoCodeFixWhenAnInitializedFieldOfExcludedCodeInFrontOfTheConstantWouldCrossAnInitializedField` (row b2, spec example 2) | RH7102-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| `FixCarriesAnInitializedFieldOfExcludedCodeWithTheConstantAcrossAFieldWithoutInitializer` (row p, spec example 3, counterpart) | RH7102-006, SHARED-ORDER-007 | `Verify(source, fixed)` | green |
| `FixCarriesAFieldOfExcludedCodeWithoutInitializerWithTheConstant` (row c2, counterpart) | RH7102-006, SHARED-ORDER-007 | `Verify(source, fixed)` | green |
| `NoCodeFixWhenAnInitializedStaticFieldOfExcludedCodeInFrontOfTheConstantWouldCrossAnInitializedStaticField` (row d) | RH7102-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| `FixCarriesAnInitializedStaticFieldOfExcludedCodeWithTheConstantAcrossAnInstanceInitializer` (row e, counterpart) | RH7102-006, SHARED-ORDER-007 | `Verify(source, fixed)` | green |
| `NoCodeFixWhenAnInitializedPropertyOfExcludedCodeInFrontOfTheConstantWouldCrossAnInitializedField` (row q) | RH7102-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| `NoCodeFixWhenAnInitializedEventFieldOfExcludedCodeInFrontOfTheConstantWouldCrossAnInitializedField` (row n) | RH7102-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| `NoCodeFixWhenAnInitializedFieldOfExcludedCodeInFrontOfTheConstantWouldCrossAnInitializedPropertyBehindTheTarget` (row r) | RH7102-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| `NoCodeFixWhenAnInitializedFieldOfAnIfFalseBlockInFrontOfTheConstantWouldCrossAnInitializedField` (row l) | RH7102-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| `NoCodeFixWhenAnInitializedFieldOfAnExcludedElifBranchInFrontOfTheConstantWouldCrossAnInitializedField` (row m) | RH7102-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| `NoCodeFixWhenTheMoveCrossesADeclarationThatIsConstOnlyInSomeConfigurations` (join of #1595 and #1593: a crossed `public` declaration that is `readonly int J = 1` under `#if DEBUG` and `const` otherwise, plus an initialized excluded field in front of `K`) | RH7102-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: fix offered. A guard that does not count the split `J` still offers the fix here, since `_i` has no initializer |
| `ConstantWhoseModifiersAnIfSplitsIntoAStaticFieldIsNotReported` (row s1, decision 16) | RH7102-001, SHARED-DIAG-008 | `Verify(source)` | red: 1 diagnostic reported, 0 expected |
| `ConstantWhoseModifiersAnIfSplitsIntoAnInstanceFieldIsNotReported` (row s2, decision 16) | RH7102-001, SHARED-DIAG-008 | `Verify(source)` | red: 1 diagnostic reported, 0 expected |
| `FixAllSkipsTheConstantWhoseMoveWouldReorderTheInitializersOfAnotherConfiguration` (`K` withheld as in b2, `protected const L` fixable in the same document) | RH7102-006, SHARED-FIX-002 | `Verify(source, fixed)` with `MarkupMode.Allow` on fixed/batch state | red: `K` is moved too |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| a2 (struct layout), o, i, g | LF, CRLF | RH7102 single fix | reproduced | one code action registered vs none |
| b2, d, q, n, r, l, m (initializer order) | LF, CRLF | RH7102 single fix | reproduced | one code action registered vs none |
| split crossed `J` | LF, CRLF | RH7102 single fix | reproduced | one code action vs none |
| s1, s2 | LF, CRLF | RH7102 analyzer | reproduced | diagnostic at `K` vs no diagnostic |
| b2 next to a fixable constant | LF | RH7102 Fix All (and the verifier's one-by-one application) | reproduced | `K` moved in front of `_i` vs `K` left in place and still reported |
| h, f, k, p, c2, e | LF, CRLF | RH7102 single fix and Fix All | holds | output equals the expected text |

### Not covered, and why

- Row s3 (s1 crossing an uninitialized static field, "keeps the fix"): with decision 16, the s1 shape is no longer reported, so no fix exists to keep; s1 already asserts "not reported".
- Formatter counterpart: the formatter does not reorder members (P11), so no formatter test applies.
- FC-030 / decision 15: every fixed counterpart uses a block-bodied `Log` and no continuation line, so no expected output locks in the whole-type formatting the follow-up may change.
- FC-008 (overlapping spans): the existing `FixAllCorrectsEveryDiagnostic` scenario already covers two constants moved in front of one field; unchanged.
- FC-004/FC-019/FC-021 (comments): the guard adds no comment handling; the existing comment and directive tests of RH7102 are unchanged.
- Not tested, not verified: a `const` whose initializer (not its modifiers) holds `#if`, which RH7102-001 also excludes from reporting. Decision 16 names only split modifiers.

## thoenissen/Reihitsu#1593

Files: `src/Reihitsu.Analyzer.Test/Formatting/RH7309RegionsShouldFollowCategoryOrderAnalyzerTests.cs`, `src/Reihitsu.Analyzer.Test/Ordering/RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.cs`, `src/Reihitsu.Analyzer.Test/Ordering/RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.cs`. The RH7102 consumer is covered by `NoCodeFixWhenTheMoveCrossesADeclarationThatIsConstOnlyInSomeConfigurations` under #1595.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7309 `NoCodeFixWhenAFieldOfTheMovedBlockHasAnInitializerOnlyInAnotherConfiguration` (row 1593/b) | RH7309-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| RH7309 `NoCodeFixWhenExcludedCodeInsideAFieldOfTheMovedBlockDeclaresAnInitializedField` (row 1593/c: `B` / `#if DEBUG` / `;` / `Z = Log("z")` / `#endif` / `;`) | RH7309-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| RH7309 `NoCodeFixWhenAPropertyOfTheMovedBlockHasAnInitializerBehindItsAccessorListOnlyInAnotherConfiguration` (row 1593/e) | RH7309-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| RH7309 `NoCodeFixWhenAFieldOfTheMovedBlockIsAnInstanceFieldOfAStructOnlyInAnotherConfiguration` (row 1593/g, spec example) | RH7309-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| RH7309 `NoCodeFixWhenAFieldOfTheMovedBlockIsAnInstanceFieldOfAStructInTheActiveCode` (row 1593/f, counterpart) | RH7309-006 | `VerifyNoCodeFixIsOffered` | green |
| RH7309 `NoCodeFixWhenACrossedFieldBetweenTheBlocksHasAnInitializerOnlyInAnotherConfiguration` (row 1593d/b: a static field outside every region between `IDisposable` and `Fields`, its initializer under `#if DEBUG`) | RH7309-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| RH7309 `NoCodeFixWhenExcludedCodeThatEndsInTheHeadOfAFieldDeclaresAnInitializedField` (row 1593d/a, counterpart: the issue's shape with `Y` uninitialized and `A = Log("a")` in `IDisposable`) | RH7309-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | green |
| RH7309 `CodeFixMovesABlockWithAMemberSplitAcrossConfigurationsWhenNothingInitializedIsCrossed` (row 1593/a, the issue's example, spec counterpart) | RH7309-006, SHARED-ORDER-007 | `Verify(source, fixed)` | green |
| RH7109 `NoCodeFixWhenTheMovedFieldHasAnInitializerOnlyInAnotherConfiguration` (row 1593b/a, spec example) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| RH7109 `NoCodeFixWhenTheCrossedFieldHasAnInitializerOnlyInAnotherConfiguration` (row 1593b/b) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| RH7109 `NoCodeFixWhenACrossedPropertyHasAnInitializerBehindItsAccessorListOnlyInAnotherConfiguration` (row 1593b/c; the fragment sits in the leading trivia of a further crossed field `_c`, so it is crossed, not moved) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| RH7103 `StaticMemberWhoseStaticModifierAnIfSplitsIsNotReported` (row 1593c/a, decision 16) | SHARED-DIAG-008 | `Verify(source)` | red: 1 diagnostic reported, 0 expected |
| RH7103 `NoCodeFixWhenTheMovedStaticFieldHasAnInitializerOnlyInAnotherConfiguration` (RH7103 member of the class that decision 16 leaves reported: `#if` in the initializer, crossing `public static int Shared = Log("p")`) | SHARED-DIAG-008, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: fix offered (the report itself already holds: the other side of SHARED-DIAG-008's judged span) |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| 1593/b, /c, /e, /g, 1593d/b | LF, CRLF | RH7309 single fix | reproduced | one code action vs none |
| 1593b/a, /b, /c | LF, CRLF | RH7109 single fix | reproduced | one code action vs none |
| RH7103 split initializer | LF, CRLF | RH7103 single fix | reproduced | one code action vs none |
| 1593c/a | LF, CRLF | RH7103 analyzer | reproduced | diagnostic at `_b` vs none |
| issue's own example (1593/a) | LF, CRLF | RH7309 single fix and Fix All | not reproduced, as `01-skeptic.md` found | fixed, output equals the spec example; kept as counterpart (decision 6) |
| 1593d/a, 1593/f | LF, CRLF | RH7309 single fix | holds | no fix offered |

### Not covered, and why

- Direct unit tests of `OrderingDeclarationUtilities.ChangesMemberOrder` in `Reihitsu.Core.Test`: every failing row reaches it through a consumer test above, and the implementer may restructure the helper's internals; the consumer tests pin the visible outcome.
- RH7109/RH7103 counterparts that keep the fix with a split member: their outputs carry a continuation line (lone `;`, split modifier) whose column the "member-moving fixes format the whole type" follow-up may change (decision 15). The RH7309 counterpart (1593/a) covers "a split member keeps the move".
- A split `const` that the active code declares non-`const` (the opposite direction of s1/s2): only withholds more, which is safe (`01-skeptic.md`).
- Formatter counterpart: the formatter does not reorder members or region blocks (P11).
- Fix All on RH7309/RH7109/RH7103: the guard decides per diagnostic inside `RegisterCodeFixesAsync`, the same call Fix All uses; the RH7102 Fix All test above exercises the shared base path (`TypeMemberOrderingCodeFixProviderBase`). RH7309 has its own provider; its existing Fix All scenario is unchanged.

## thoenissen/Reihitsu#1616

File: `src/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs` (class `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests`). Every multi-diagnostic `Verify(source, fixed)` also runs the verifier's Fix All checks.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFixWritesReorderedModifiersOnOneLineBehindTheLeadingTrivia` (RH2001-004 spec example: comment + blank line, doc comment + initializer + trailing comment, `{ get; set; }` counterpart; fixtures c, d, j, l, g) | RH2001-004 | `Verify(source, fixed)` | red |
| `VerifyFixKeepsACommentInFrontOfTheFirstModifierOnceInFrontOfTheReorderedModifiers` (`/* lead */`, spec example, decision 14) | RH2001-004 | `Verify(source, fixed)` | red: no fix offered |
| `VerifyFixWritesReorderedModifiersWithOneSpaceBetweenThem` (fixture a) | RH2001-004 | `Verify(source, fixed)` | red: gap |
| `VerifyFixWritesThreeReorderedModifiersInTheOrderOfRh7105` (fixture i, `static new private` → `private static new readonly`) | RH2001-004 | `Verify(source, fixed)` | red |
| `VerifyFixKeepsATrailingCommentWhenItReordersTheModifiers` (fixture k) | RH2001-004 | `Verify(source, fixed)` | red |
| `VerifyFixKeepsTheBlankLineAboveAndTheReorderedModifiersOnOneLine` (fixture j, change-doc example, answer 2A) | RH2001-004 | `Verify(source, fixed)` | red: `private` / newline / `static readonly int _value;` |
| `VerifyFixKeepsAPragmaDirectiveAboveOnceInFrontOfTheReorderedModifiers` (fixture e, decision 8) | RH2001-004 | `Verify(source, fixed)` | red: no fix offered |
| `VerifyFixKeepsARegionDirectiveAboveOnceInFrontOfTheReorderedModifiers` (fixture n, decision 8) | RH2001-004 | `Verify(source, fixed)` | red: no fix offered |
| `VerifyFixWritesReadonlyInFrontOfUnsafe` (fixtures u1 and g-`unsafe`, decision 14; `AllowUnsafeBlocks`) | RH2001-004 | `Verify(source, fixed)` with `ApplyAllowUnsafeToTestProject` | red: `unsafe private string Second` below a blank line becomes `private readonly` / newline / `unsafe string _second;`; `First` holds |
| `VerifyNoFixIsOfferedForACommentThatAReorderedModifierWouldCross` (RH2001-009 spec example: fixtures b, f, m; answer 1A) | RH2001-009 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| `VerifyNoFixIsOfferedForADirectiveThatAReorderedModifierWouldCross` (`#pragma` between `static` and `private`, spec example) | RH2001-009 | `VerifyNoCodeFixIsOffered` | green (today's output) |
| `VerifyFixKeepsACommentThatNoModifierCrosses` (RH2001-009 spec example: fixtures o, `/* b */ new static`, `static private /* c */` (batch 2 A), `private static /* d */`) | RH2001-009 | `Verify(source, fixed)` | red only on `Third`: `private /* c */` / newline / `static readonly int _third;` vs `private static /* c */ readonly int _third;` |
| `VerifyFixWritesReadonlyBehindACommentInFrontOfUnsafe` (fixture u2, batch 2 A) | RH2001-009 | `Verify(source, fixed)` with `ApplyAllowUnsafeToTestProject` | green |
| `VerifyFixKeepsAnEndOfLineCommentBetweenModifiersThatKeepTheirOrder` (fixture h) | RH2001-009 | `Verify(source, fixed)` | green |
| `VerifyFixKeepsADirectiveBetweenModifiersThatKeepTheirOrder` (fixture c of `03-skeptic.md`: `private` / `#pragma` / `static`, decision 14) | RH2001-009 | `Verify(source, fixed)` | green |
| `VerifyFixAllSkipsThePropertyWithACommentThatAReorderedModifierWouldCross` (one withheld, two reordered) | RH2001-004, RH2001-009, SHARED-FIX-002 | `Verify(source, fixed)` with `MarkupMode.Allow` | red: `First` fixed with a gap; `Second`/`Third` split over two lines |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| a, i, k | LF, CRLF | RH2001 single fix | reproduced | `private␣␣␣␣␣static …` vs `private static …` |
| j (blank line above) | LF, CRLF | RH2001 single fix | reproduced | line break inside the modifiers vs one line |
| c, d (spec example 1), e, n, `/* lead */` | LF, CRLF | RH2001 single fix | reproduced | no fix offered vs field with the trivia once in front |
| b, f, m | LF, CRLF | RH2001 single fix | reproduced | fix offered (comment travels) vs none |
| `static private /* c */` | LF, CRLF | RH2001 single fix | reproduced | `private /* c */` + newline + `static readonly` vs `private static /* c */ readonly` |
| mix of withheld and reordered | LF | RH2001 Fix All and one-by-one | reproduced | as above, per property |
| h, o, `/* b */ new static`, `/* d */`, u2, `#pragma` kept order, `#pragma` crossed, `{ get; set; }` | LF, CRLF (u2: LF) | RH2001 single fix | holds | output equals the expected text |

### Not covered, and why

- `#if` between modifiers (fixtures p of `01-skeptic.md` and d of `03-skeptic.md`): a conditional directive in the modifier list puts the RH2001 report itself under SHARED-DIAG-008, which no decision of this run settles for RH2001; a test with `VerifyNoCodeFixIsOffered` or `Verify(source, fixed)` would lock in that report. The `#pragma` examples cover the directive arms of RH2001-009.
- A directive at the place where `readonly` goes (`private` / `#pragma` / `unsafe string`): outside RH2001-009's coverage by its own text.
- FC-007 (RH7105 on the fix result): every expected output is written in RH7105-001 order, so an exact-output assertion already excludes an RH7105 report.
- Formatter counterpart: the formatter does not turn properties into fields; whole-document formatting of today's gap output is in `01-skeptic.md` and is not the surface in question.
- FC-018 (Renamer): references are renamed in the spec example (`Read()` and the Fix All test), the existing rename tests are unchanged.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7309RegionsShouldFollowCategoryOrderAnalyzerTests|FullyQualifiedName~RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests|FullyQualifiedName~RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project architecture
```

## Whole run

- Red at beeac8e: RH7102 29 rows (15 tests), RH7309/RH7109/RH7103 20 rows (10 tests), RH2001 22 rows (12 tests). Every other test of the five classes passes (RH7102 26, the three ordering classes 173, RH2001 75).
- `scripts/format.sh` on the five files: 0 of 5 changed. `scripts/test.sh --project architecture`: 115 passed.
