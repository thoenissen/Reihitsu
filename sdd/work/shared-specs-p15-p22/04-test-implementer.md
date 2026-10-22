Role: test implementer
Commit: 63956ec
Verdict: tests written

Lane: specs and tests for existing behavior. Every test below passes on the unchanged production code. The tests are derived from `git diff origin/main -- sdd/specs`, `03-spec-author.md` ("Examples for the tests", which supersedes the list in `01-spec-author.md`), `02-skeptic.md`, and the decisions `squad/decisions/2026-10-21-1260-*.md`. They pin only shapes that already conform. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests

New tests are marked "new". All others are existing tests that gained tags. All are analyzer tests under `src/Reihitsu.Analyzer.Test/`. Helpers: `Verify(source[, fixed])` on the analyzer or code-fix base, and `VerifyNoCodeFixIsOffered`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH2003…Tests.VerifyCreationThroughNamespaceAliasIsReportedLikeTheTypeName` (new) | SHARED-DIAG-002, RH2003-001; FC-003: `S.ArgumentException` is not reported | `Verify` | pass |
| `RH1002…Tests.VerifyParenthesesNullForgivingOperatorAndThrowBranchDoNotChangeTheJudgedValue` (new, spec example) | SHARED-DIAG-005 | `Verify` | pass |
| `RH2102…Tests.VerifyPartialClassDeclaredInTwoPartsIsReportedOnce` (new, spec example) | SHARED-DIAG-004, RH2102-001; FC-024 | `Verify` | pass (cannot fail, CS8863; decision `…-diag004-weak-coverage`) |
| `RH2004…Tests.VerifyPartialTypeWithAccessibilityOnAnotherPartDoesNotTriggerDiagnostic` | SHARED-DIAG-004 | `Verify` | pass (cannot fail, RH2004-002; same decision) |
| `RH4107ProtectedFieldCasingAnalyzerTests.EveryDeclaratorIsCheckedWhereverItStandsInItsDeclaration` (new, spec example, with fix) | SHARED-DIAG-006, RH4107-001/-002; FC-003: valid declarator before and after; FC-007: one-pass fix | `Verify(source, fixed)` | pass |
| `RH8101…Tests.VerifyUndocumentedParameterInTheMiddleOfTheListIsReported` (new, spec example) | SHARED-DIAG-006, RH8101-001 | `Verify` | pass |
| `RH8101…Tests.VerifyParametersDocumentedInAnotherOrderAreNotReported` (new) | SHARED-DIAG-006 (order independence), RH8101-001; FC-003 | `Verify` | pass |
| `RH2004…Tests.VerifyDiagnosticIsOnFirstDeclaratorAndIndexerKeyword` | SHARED-DIAG-006 (declaration-wide verdict reported once at the first declarator; the spec's RH2004 example), SHARED-DIAG-007 (indexer at `this`) | `Verify` | pass |
| `RH2004…Tests.VerifyOperatorsWithoutAccessibilityModifierAreReportedOnTheirToken` | SHARED-DIAG-007 (nameless declaration at the token that marks it) | `Verify` | pass |
| `RH8031…Tests.VerifySpecificationExampleReportsExtensionDeclarationsWithoutSummary` | SHARED-DIAG-007 (spec example, at `extension`) | `Verify` | pass |
| `RH8031…Tests.VerifySpecificationExampleReportsExtensionDeclarationSummariesWithoutText` | SHARED-DIAG-007, other side: the location on an element is kept | `Verify` | pass |
| `RH5409…Tests.VerifySpecificationExampleReportsCommaInFrontOfRegionButNotInFrontOfConditionalBlock` | SHARED-DIAG-008: `#if` in the span means not reported; `#region` keeps the report and the fix (reading B) | `Verify(source, fixed)` | pass |
| `RH5409…Tests.VerifyCommaInFrontOfPragmaDirectivesIsReportedAndFixed` | SHARED-DIAG-008, `#pragma` with reading B | `Verify(source, fixed)` | pass |
| `RH5409…Tests.VerifyCommaInFrontOfEmptyConditionalBlockIsNotReported` | SHARED-DIAG-008, a directive with no excluded code | `Verify` | pass |
| `RH5409…Tests.VerifyCommaInFrontOfInactiveIfFalseBlockIsNotReported` (new) | SHARED-DIAG-008, SHARED-TRAIL-002; FC-020 (`#if false`) | `Verify` | pass |
| `RH5412…Tests.VerifyRegionDirectivesInBodyAreReportedWithoutCodeFix` (new) | SHARED-DIAG-008 (`#region` in the judged body), RH5412-001 | `VerifyNoCodeFixIsOffered` | pass |
| `RH5412…Tests.VerifyPragmaDirectiveInBodyIsReportedWithoutCodeFix` (new) | SHARED-DIAG-008 (`#pragma` in the judged body), RH5412-001 | `VerifyNoCodeFixIsOffered` | pass |
| `RH7106…Tests.PragmaDirectiveInFrontOfALaterModifierWithholdsTheFix` | SHARED-DIAG-008 (spec example) | `VerifyNoCodeFixIsOffered` | pass |
| `RH7003…Tests.VerifyDirectiveContainingNamespaceIsReported` | SHARED-DIAG-008 (`#if` in a body the rule does not judge; spec example shape) | `Verify` | pass |
| `RH7305TypesShouldBeOrganizedWithRegionsAnalyzerTests.NestedEnumInARegionIsOneMemberWhateverRegionsItHasAmongItsValues` (new, spec example) | SHARED-TYPEREGION-002 | `Verify` | pass (cannot fail, per `03-spec-author.md`). Other side: the existing `NestedTypesAndEnumsAreNotCheckedButCountAsMembers`, where a nested type outside a region gets its container reported |
| `RH7110…Tests.MemberInANestedRegionIsComparedWithinItsTopLevelRegion` | SHARED-TYPEREGION-006 (a region used only as a scope; the spec's RH7110 example with methods in place of fields) | `Verify` | pass |
| `RH7110…Tests.MembersAreComparedWithTheLatestAccessibilitySeenInTheirGroup` | SHARED-USING-007 (the spec's RH7110 `_a`/`B`/`D` example). Its summary no longer says "no requirement states" | `Verify` | pass |
| `RH7203…Tests.DirectiveIsComparedWithAnEarlierDirectiveOfItsGroupAcrossAnotherGroup` (new, spec example, with fix) | SHARED-USING-007, RH7203-001/-004; FC-007 | `Verify(source, fixed)` | pass |
| `RH7203…Tests.DirectiveIsComparedWithAnEarlierDirectiveOfItsGroupAcrossAnotherKind` (new) | SHARED-USING-007 (`using static` between) | `Verify` | pass |
| `RH7203…Tests.OrderedDirectivesOfAGroupAreNotReportedAcrossAnotherGroup` (new) | SHARED-USING-007; FC-003 | `Verify` | pass |
| `RH8107…Tests.VerifyCodeFixRemovesEmptyReturnsElement` (new, spec example) | SHARED-DOC-017 (an empty element keeps its fix), RH8107-006; FC-002 | `Verify(source, fixed)` | pass |
| `RH8107…Tests.VerifyNoCodeFixWhenEndTagIsMissing` | SHARED-DOC-017 (no end tag, no fix) | `Verify(source, source)` | pass |
| `RH8202…Tests.VerifyCodeFixRemovesEmptyValueElement` (new) | SHARED-DOC-017, RH8202-003 | `Verify(source, fixed)` | pass |
| `RH8202…Tests.VerifyNoCodeFixForValueElementWithoutEndTagOfItsOwn` (new, spec example) | SHARED-DOC-017, RH8202-004 | `Verify(source, source)` | pass |
| `RH8202…Tests.VerifyNoCodeFixWhenEndTagIsMissing` | SHARED-DOC-017 | `Verify(source, source)` | pass |
| `RH8204…Tests.VerifySpecificationExampleReplacesPlaceholderWithItsContent` | SHARED-DOC-017 (`<placeholder/>` keeps its fix) | `Verify(source, fixed)` | pass |

### Contradicting characterization tests (decision `…-contradicting-characterization-tests.md`)

All tests listed in `02-skeptic.md` were kept with their assertions, names, and attributes unchanged. Only their `<summary>` changed. It now reads "This test pins today's behavior; the decided behavior is stated by <requirement ID> … and its change belongs to an existing follow-up". The summary names no tracker number, because `AGENTS.md` and the architecture test forbid tracker numbers in source comments. The requirement ID is the durable pointer.

- `RH2003…Tests.VerifyCreationThroughUsingAliasIsNotReported` (SHARED-DIAG-002)
- `RH1002…Tests.VerifyArgumentThatCanOnlyProduceNullOrThrowCountsAsCustom` (SHARED-DIAG-005)
- `RH2002…Tests.VerifyAsyncVoidPartialMethodIsReportedOnBothDeclarations` (SHARED-DIAG-004)
- `RH2004…Tests.VerifyPartialMemberPartsAreReported` (SHARED-DIAG-004)
- `RH2004…Tests.VerifyOneFixApplicationChangesOnePartOfPartialProperty` (SHARED-DIAG-004)
- `RH2101…Tests.VerifyPartWithoutStaticOfNestedStaticPartialClassIsReported` (SHARED-DIAG-004)
- `RH7401…Tests.VerifyPartialOverrideMethodIsReportedOncePerPart` (SHARED-DIAG-004)
- `RH7409…Tests.VerifyPartialImplementationMethodIsJudgedAtItsDefiningPart` (SHARED-DIAG-004)
- `RH7403…Tests.VerifyFieldLikeOverrideEventWithSeveralDeclaratorsIsJudgedByItsFirstDeclarator` (SHARED-DIAG-006)
- `RH7411…Tests.VerifyFieldLikeImplementationEventWithSeveralDeclaratorsIsJudgedByItsFirstDeclarator` (SHARED-DIAG-006)
- `RH7411…Tests.VerifyFieldLikeImplementationEventWithSeveralDeclaratorsIsReportedOnceAtFirstDeclarator` (SHARED-DIAG-006)
- `RH7311…Tests.SameDescriptionInIfAndElseBranchIsCurrentlyReported` (SHARED-TYPEREGION-006)
- `RH7401…Tests.VerifyOverrideInNestedRegionIsJudgedByItsTopLevelRegion` (SHARED-TYPEREGION-006)
- `RH7405…Tests.VerifyMethodInNestedRegionIsJudgedByItsTopLevelRegion` (SHARED-TYPEREGION-006)
- `RH5416…Tests.VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix` (SHARED-DIAG-008). Its summary previously called the behavior intended. It is now labelled a characterization.

The three RH2109 Razor characterization tests are unchanged, per decision `…-razor-content-no-conforming-shape.md`.

### Further contradicting tests found in passing

The lead confirmed that the decision covers every existing test that pins the opposite of a new requirement, not only the skeptic's list. A follow-up commit applied it to two more tests. Again, only the `<summary>` changed; assertions, names, and attributes are untouched, and neither test is tagged.

- `RH1002…Tests.VerifyWrappedArgumentThatCanOnlyProduceNullOrDefaultComparerIsNotReported` (SHARED-DIAG-005). It pins `(flag ? null : EqualityComparer<S>.Default)`, and the same expression behind `!`, as not reported, while the unwrapped form is reported. Its summary no longer says "open question" and now uses the wording above.
- `RH7106ProtectedMustComeBeforeInternalAnalyzerTests.MisorderedCompoundAccessModifiersWithDirectiveAreReportedWithoutFix` (SHARED-DIAG-008). It pins `internal` / `#if FEATURE` / `#endif` / `protected` as reported. It is now labelled a characterization test with the same wording. Its earlier summary presented the behavior as intended.

Left unchanged, as the lead decided, because the spec checker judges them: `RH5111…Tests.VerifyDeclaratorWithDirectiveIsReportedWithoutCodeFix` (RH5111-005, `#if` between the declarator and `=`, reported) and `RH3001…Tests.DirectiveAfterTheOperatorIsReportedWithoutACodeFix` (RH3001-006, `#if/#else/#endif` between `!` and its operand, reported). Whether those gaps are "the span the rule judges" under SHARED-DIAG-008 is a question of one spec against another.

## Scenarios (bug fix)

_N/A._ This lane pins existing behavior and has no reproduction.

## Not covered, and why

- **SHARED-DIAG-003** — the existing C# literal and comment tests already cover it. The Razor sentence has no conforming shape (decision `…-razor-content-no-conforming-shape.md`, thoenissen/Reihitsu#1289).
- **SHARED-USING-004** item 18 — no new test (decision `…-using004-item18.md`, thoenissen/Reihitsu#1267). The existing -004 tests cover the unchanged part.
- **Not pinned because they do not conform**, per `03-spec-author.md` and `02-skeptic.md`:
  - `new NIE()` through an alias (thoenissen/Reihitsu#1268).
  - `flag ? null : throw …` (thoenissen/Reihitsu#1290).
  - RH2002, RH2101, the RH2004 partial members, and RH7401/RH7409 parts (thoenissen/Reihitsu#1287).
  - RH7403/RH7407/RH7411 declarators (thoenissen/Reihitsu#1333).
  - RH5404 extension block (thoenissen/Reihitsu#1276).
  - RH5412 with `#if DEBUG`/`#endif` in the body (thoenissen/Reihitsu#1275).
  - RH7101 and RH7105/RH7106 with `#if` in the span (thoenissen/Reihitsu#1337).
  - RH7201–RH7207 with `#pragma` or `#region` in the block (the P22 instance the lead filed as thoenissen/Reihitsu#1374).
  - The RH7311 nested enum, the RH7308 nested `Properties`, and RH7311 `#if`/`#else` (thoenissen/Reihitsu#1264).
  - The RH7203 `Microsoft.B; Newtonsoft.X; Microsoft.A` and `Microsoft.C; Microsoft.A; Microsoft.B` cases (thoenissen/Reihitsu#1266).
  - RH8107 `<remarks><returns>Nothing</remarks>` (thoenissen/Reihitsu#1241).
- **SHARED-DOC-017 for RH8204 on an element without an end tag** — not pinned. Static evidence: `rg -n "CanRemoveElement" src/Reihitsu.Analyzer.CodeFixes` finds the end-tag guard called only from `RH8107…CodeFixProvider` and `RH8202…CodeFixProvider`. `RH8204DoNotUsePlaceholderElementsCodeFixProvider` registers its fix with no end-tag check, so the shape is not expected to conform. I did not run it.
- **Weak evidence (FC-017), kept by decision:**
  - the two SHARED-DIAG-004 tests cannot fail;
  - the `throw` line of the SHARED-DIAG-005 test cannot fail, because `comparer` is custom on its own;
  - the RH7305 nested-enum test cannot fail.
- **FC-010, FC-011, FC-012, FC-013, FC-014, FC-028** (formatter stability and parity) cannot be reached: no formatter phase, code fix, or analyzer changes. The spec diff states analyzer reporting and fix-offering only.
- **CRLF variants.** No requirement in the diff concerns layout or line endings. The fix tests above only show that a fix is offered or withheld, plus its existing output. The CRLF behavior of those fixes belongs to RH8107-006, RH8202-003, RH4107-002, and RH7203-004, which this run does not change.
- **FC-008 (Fix All).** No Fix All requirement changed. The existing `FixAllCorrectsEveryDiagnostic` scenarios of the touched batch classes still pass.
- **FC-018 (Renamer).** The RH4107 test checks only that each declarator is renamed. Retargeting references is covered by the existing `FixRenamesEveryDeclaratorOfAProtectedFieldAndEveryReference`.
- **FC-019/FC-020/FC-021** (comment or directive in a gap) are covered for SHARED-DIAG-008 by the RH5409, RH5412, RH7106, and RH7003 tests above: `#if` (including `#if false`), `#region`, and `#pragma` in the judged span, and `#if` outside it. Comments are not the subject of any changed requirement.

## Commands

All of the following passed:

```shell
scripts/format.sh --force <the 26 changed test files>        # Formatted 0 of 26
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2002AsyncVoidShouldNotBeUsedAnalyzerTests|…"   # the 26 touched classes: 451 passed
scripts/test.sh --project analyzer --no-build --filter "TestCategory=SHARED-DIAG-004"   # 2 passed
scripts/test.sh --project analyzer --no-build --filter "TestCategory=SHARED-DIAG-005"   # 1 passed
scripts/test.sh --project analyzer --no-build --filter "TestCategory=SHARED-DIAG-006"   # 4 passed
scripts/test.sh --project analyzer --no-build --filter "TestCategory=SHARED-DIAG-007"   # 4 passed
scripts/test.sh --project analyzer --no-build --filter "TestCategory=SHARED-DIAG-008"   # 8 passed
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"   # 39 passed
scripts/test.sh --project architecture --no-build                                        # 72 passed
```

Follow-up (the two reworded summaries above), all passed:

```shell
scripts/format.sh --force <RH1002 and RH7106 test files>        # Formatted 0 of 2
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzerTests|FullyQualifiedName~RH7106ProtectedMustComeBeforeInternalAnalyzerTests"   # 66 passed
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"   # 39 passed
```
