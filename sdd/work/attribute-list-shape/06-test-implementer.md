Role: test implementer
Commit: 1f2fbfb
Verdict: tests written

Lane: specs and tests for existing behavior. No production code was touched. Every test below passes on the unchanged code. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

All tests are in `src/Reihitsu.Analyzer.Test/Formatting/RH55xx…ShapeRulesAnalyzerTests.cs`, one class per rule. Every active requirement this run adds has at least one covering test: 67 rule requirements and SHARED-ATTR-013 to SHARED-ATTR-020. `RequirementCoverageTests`, `SpecConsistencyTests` and `SpecStructureTests` pass, along with the rest of the architecture project (72 tests).

## How the tests were derived and checked

- Inputs and expected outputs come from the spec examples, copied into the tests as written. The only additions are the attribute class declarations `sealed class A : System.Attribute` and so on, at the end of each fixture. No value came from running the code.
- Every code-fix test compares the whole document. Each split and merge example runs twice: once with LF, and once with source and expected output both normalized to CRLF (`…WithCarriageReturnLineFeed`). The testing library compares line endings exactly. I checked this by mutation: a CRLF source against an LF expected output fails with a `<CR><LF>` diff.
- The no-fix tests call `GetCodeFixActionsAsync` on each reported list by index. I checked the indices by mutation: with the comment removed from `codeFixData`, all four no-fix tests (RH5506, RH5524, RH5529, RH5531) fail with "Expected collection to be empty". So each one checks the list it is meant to check. All mutations were reverted.
- When a `Verify` call has two or more diagnostics, the library also runs Fix All with its default iteration bound. New tests of this kind include the SHARED-ATTR-017 namespace example, the RH5524 merge examples, and RH5531-005. All of them pass, so they converge in one Fix All pass.

## Tests

Tags added to existing tests (unchanged otherwise):

| Test | Requirement | Helper | Expected now |
|---|---|---|---|
| `VerifyDiagnosticAndCodeFixForPolicyViolation` (RH5502, 5504, 5506, 5508, 5510, 5512, 5516, 5518, 5520, 5522, 5526, 5528) | RHxxxx-001, -002, -003, SHARED-ATTR-013, -016, -017 | `Verify(source, fixed)` | pass |
| `VerifyDiagnosticAndCodeFixForPolicyViolation` (RH5514) | RH5514-001, -003, -004, SHARED-ATTR-013, -016, -017 | `Verify(source, fixed)` | pass |
| `VerifyNoDiagnosticForCompliantCode` (all 14 split classes) | RHxxxx-001, SHARED-ATTR-013 | `Verify(source)` | pass |
| `VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent` (all 14 split classes) | RHxxxx-004 (RH5514-005), SHARED-ATTR-019 | `Verify` + `GetCodeFixActionsAsync` | pass |
| RH5506 `VerifyInsertedLineBreakUsesDetectedCarriageReturnLineFeedEndOfLine` | RH5506-003, SHARED-ATTR-017 | `ApplyCodeFixAsync` | pass |
| RH5506 `VerifyDocumentedClassAttributeListIsOfferedACodeFix` | RH5506-004, SHARED-ATTR-019 (comment in front does not withhold) | `GetCodeFixActionsAsync` | pass |
| RH5516 `VerifyNoDiagnosticForPropertyTargetedAttributeListOnRecordParameter`; RH5518 `VerifyNoDiagnosticForFieldTargetedAttributeListOnRecordParameter` | RH5516-001; RH5518-001 | `Verify(source)` | pass |
| RH5528 `VerifyNoDiagnosticForMergedReturnSpecifierListOnSingleLinePropertyAccessor` (+`CrLf`) | RH5528-001, SHARED-ATTR-014 | `Verify(source)` | pass |
| RH5528 `VerifyDiagnosticForMergedReturnSpecifierListOnMultiLinePropertyAccessor` | RH5528-001, -003, SHARED-ATTR-014, -017 | `Verify(source, fixed)` | pass |
| RH5531 `VerifyDiagnosticAndCodeFixForMultilineAccessorPolicyViolation` | RH5531-001, -004, -005, SHARED-ATTR-013, -016, -017 | `Verify(source, fixed)` | pass |
| RH5531 `VerifyNoDiagnosticForCompliantMultilineAccessorCode`, `…WithPropertyAttribute` | RH5531-001 (+ SHARED-ATTR-013 on the first) | `Verify(source)` | pass |
| RH5531 `VerifyDiagnosticAndCodeFixForSingleLineAccessorPolicyViolation` | RH5531-002, -004, -005, SHARED-ATTR-015, -016, -018 | `Verify(source, fixed)` | pass |
| RH5531 `VerifyNoDiagnosticForCompliantSingleLineAccessorCode` | RH5531-002, SHARED-ATTR-014 | `Verify(source)` | pass |
| RH5531 `VerifyDiagnosticAndCodeFixForSingleLineAccessorPolicyViolationWithPropertyAttribute` | RH5531-002, -005 | `Verify(source, fixed)` | pass |
| RH5531 `VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent` | RH5531-006, SHARED-ATTR-019 | `Verify` + `GetCodeFixActionsAsync` | pass |
| RH5524 and RH5529 `VerifyDiagnosticAndCodeFixForPolicyViolation` | RH55xx-001, -002, -003, SHARED-ATTR-015, -016, -018 | `Verify(source, fixed)` | pass |
| RH5524 and RH5529 `VerifyNoDiagnosticForCompliantCode` | RH55xx-001, SHARED-ATTR-015 | `Verify(source)` | pass |
| RH5524 and RH5529 `VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent` | RH55xx-004, SHARED-ATTR-020 | `Verify` + `GetCodeFixActionsAsync` | pass |
| RH5524 `…PropertyTargetedAttributeListsOnRecordParameter`, `…MergeOnlyListsOfTheSameTarget` | RH5524-001, -003, SHARED-ATTR-015, -018 | `Verify(source, fixed)` | pass |
| RH5524 `VerifyNoDiagnosticForAttributeListsOfDifferentTargets`, `…PropertyAndFieldTargetedAttributeLists`, `…TypeTargetedAttributeList` | RH5524-001, SHARED-ATTR-015 | `Verify(source)` | pass |
| RH5524 `…OnSeparateLines`, `…KeepTokensSeparatedAfterListWithoutTrailingSpace`, and the nine `VerifyCodeFixJoins…`/`VerifyCodeFixSeparates…`/`VerifyCodeFixKeepsBlankLine…` tests | RH5524-003, SHARED-ATTR-018 | `Verify(source, fixed)` (one CRLF) | pass |
| RH5524 `…WhenCommentFollowsTheGroup`, `…CommentFollowsBlankLineAfterTheGroup`, `…ConditionalDirectiveFollowsBlankLineAfterTheGroup`, `…PragmaFollowsBlankLineAfterTheGroup` | RH5524-004, SHARED-ATTR-020 | `Verify` + `GetCodeFixActionsAsync` | pass |
| RH5529 `VerifyCodeFixJoinsTypeParameterWhenBlankLineFollowsTheGroup`, `…VariantTypeParameter…` | RH5529-003, SHARED-ATTR-018 | `Verify(source, fixed)` | pass |
| RH5529 `VerifyDiagnosticWithoutCodeFixWhenPragmaFollowsBlankLineAfterTheGroup` | RH5529-004, SHARED-ATTR-020 | `Verify` + `GetCodeFixActionsAsync` | pass |

New tests (spec examples unless stated). "LF+CRLF" means a plain test and a `…WithCarriageReturnLineFeed` twin.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5502 `VerifyDiagnosticOnlyForMultiAttributeAssemblyList`; RH5504 `…ModuleList` | RH5502/RH5504-001, -002, SHARED-ATTR-013, -016 | `Verify(source)` | pass |
| RH5502/RH5504 `VerifyCodeFixGivesEach{Assembly,Module}AttributeAListOfItsOwn` LF+CRLF | -003, SHARED-ATTR-017; FC-017 | `Verify(source, fixed)` | pass |
| RH5506 `VerifyDiagnosticForMultiAttributeListsOfClassesAndRecords` | RH5506-001, -002, SHARED-ATTR-013, -016 | `Verify(source)` | pass |
| RH5506 `VerifyCodeFixGivesEachClassAttributeAListOfItsOwn` LF+CRLF | RH5506-003, SHARED-ATTR-017 | `Verify(source, fixed)` | pass |
| RH5506 `VerifyCodeFixSplitsEveryClassAttributeListOnItsOwnLine` (not a spec example: `[A, B]` / `[D, E]` / `class C`) | RH5506-003, SHARED-ATTR-017; FC-008 (Fix All on two lists of one declaration on separate lines, one pass) | `Verify(source, fixed)` | pass |
| RH5506 `VerifyDiagnosticWithoutCodeFixWhenCommentOrDirectiveIsInsideTheList` | RH5506-004, SHARED-ATTR-019 (comment and `#if`/`#endif` arms); FC-005, FC-020 | `Verify` + `GetCodeFixActionsAsync` | pass |
| RH5506 `VerifyDiagnosticForEachOfTwoIdenticalListsOnOneLine` (not a spec example: `[A, B] [A, B]`, attributes with `AllowMultiple = true`) | RH5506-001, -002, SHARED-ATTR-013, -016; FC-026 | `Verify(source)` | pass |
| RH5508 `…ListsOfStructsAndRecordStructs`; RH5510 `…EnumListButNotForEnumMemberList`; RH5512 `…InstanceAndStaticConstructors`; RH5516 `…PropertiesAndIndexers`; RH5518 `…FieldsAndEnumMembers`; RH5520 `…ListsOfEvents`; RH5522 `…InterfaceList`; RH5526 `…DelegateLists` | RHxxxx-001, -002, SHARED-ATTR-013, -016 | `Verify(source)` | pass |
| `VerifyCodeFixGivesEach{Struct,Enum,Constructor,Property,Field,Event,Interface,Delegate}AttributeAListOfItsOwn` LF+CRLF | RHxxxx-003, SHARED-ATTR-017 | `Verify(source, fixed)` | pass |
| RH5514 `VerifyDiagnosticForMultiAttributeListsOfEveryMethodKind` | RH5514-001, -003, SHARED-ATTR-013, -016; FC-027 (local function, lambda in an equals-value clause) | `Verify(source)` | pass |
| RH5514 `VerifyNoDiagnosticForMultiAttributeAccessorList` | RH5514-002 | `Verify(source)` | pass |
| RH5514 `VerifyDiagnosticForMultiAttributeListWhetherOnItsOwnLineOrSharingIt` | RH5514-001, SHARED-ATTR-013 | `Verify(source)` | pass |
| RH5514 `VerifyCodeFixGivesEachMethodAttributeAListOfItsOwn` LF+CRLF | RH5514-004, SHARED-ATTR-017 | `Verify(source, fixed)` | pass |
| RH5514 `VerifyCodeFixKeepsDocumentationCommentAndTrailingCommentAroundTheSplitLists` LF+CRLF | RH5514-004, SHARED-ATTR-017 (comment in front and trailing comment); FC-021 | `Verify(source, fixed)` | pass |
| RH5514 `VerifyCodeFixSplitsLocalFunctionListInsideSwitchSection` LF+CRLF | RH5514-004, SHARED-ATTR-017; FC-027 | `Verify(source, fixed)` | pass |
| RH5528 `VerifyDiagnosticForMultiAttributeReturnValueListsOutsideInlineAutoProperties` | RH5528-001, -002, SHARED-ATTR-013, -014, -016 | `Verify(source)` | pass |
| RH5528 `VerifyDiagnosticForMultiAttributeAccessorListUnlessThePropertyIsAnInlineAutoProperty` (SHARED-ATTR-014 example) | RH5528-001, SHARED-ATTR-014; FC-003 (inline, expression body, multi-line) | `Verify(source)` | pass |
| RH5528 `VerifyDiagnosticForMultiAttributeAccessorListWhenCommentSitsBetweenTheAccessors` (not a spec example: `int S { [return: A, B] get; /* c */ set; }`) | RH5528-001, SHARED-ATTR-014 via the comment condition of SHARED-ATTR-006; FC-003 | `Verify(source)` | pass |
| RH5528 `VerifyNoDiagnosticForInlineAutoPropertyWithOwnAttributeListAndTrailingComment` (not a spec example: `[D]` / `int Q { [return: A, B] get; set; } = 1; // c`) | RH5528-001, SHARED-ATTR-014; FC-003 | `Verify(source)` | pass |
| RH5528 `VerifyCodeFixGivesEachReturnValueAttributeAListOfItsOwn` LF+CRLF | RH5528-003, SHARED-ATTR-017 | `Verify(source, fixed)` | pass |
| RH5524 `VerifyDiagnosticForEveryFollowingListOfOneParameterAndTarget` | RH5524-001, -002, SHARED-ATTR-015, -016 | `Verify(source)` | pass |
| RH5524 `VerifyDiagnosticForListsOfOneTargetAcrossListsOfAnotherTarget` (SHARED-ATTR-015 example) | RH5524-001, SHARED-ATTR-015, -016 | `Verify(source)` | pass |
| RH5524 `VerifyCodeFixMergesEveryListOfTheTargetIntoTheFirst` LF+CRLF | RH5524-003, SHARED-ATTR-018 | `Verify(source, fixed)` | pass |
| RH5524 `VerifyCodeFixRemovesMergedListsTogetherWithTheirLineBreaks` LF+CRLF; `VerifyCodeFixJoinsListOfAnotherTargetAndKeepsParameterOnItsLine` LF+CRLF (SHARED-ATTR-018 examples) | RH5524-003, SHARED-ATTR-018; FC-004 | `Verify(source, fixed)` | pass |
| RH5524 `VerifyDiagnosticWithoutCodeFixWhenCommentOrDirectiveLiesAroundTheGroup` (SHARED-ATTR-020 example: comment between, after the last list, before the token, `#if` in front, comment inside a list) | RH5524-004, SHARED-ATTR-020; FC-005, FC-019, FC-020, FC-021 | `Verify` + `GetCodeFixActionsAsync` | pass |
| RH5524 `VerifyCodeFixIsOfferedWhenCommentsLieOutsideTheGroup` LF+CRLF (SHARED-ATTR-020 example, the other side of the boundary) | RH5524-003, -004, SHARED-ATTR-018, -020; FC-003 | `Verify(source, fixed)` | pass |
| RH5529 `VerifyDiagnosticForEveryFollowingListOfOneTypeParameterAndTarget` | RH5529-001, -002, SHARED-ATTR-015, -016 | `Verify(source)` | pass |
| RH5529 `VerifyCodeFixMergesTypeParameterListsIntoTheFirst` LF+CRLF | RH5529-003, SHARED-ATTR-018 | `Verify(source, fixed)` | pass |
| RH5529 `VerifyDiagnosticWithoutCodeFixWhenFirstListHoldsComment` | RH5529-004, SHARED-ATTR-020 | `Verify` + `GetCodeFixActionsAsync` (index of the reported list) | pass |
| RH5531 `VerifyDiagnosticForMultiAttributeListsOfAccessorsInMultiLineAccessorLists` | RH5531-001, -004, SHARED-ATTR-013, -016 | `Verify(source)` | pass |
| RH5531 `VerifyDiagnosticForFollowingListsOfInlineAutoPropertyAccessors` | RH5531-002, -004, SHARED-ATTR-014, -015, -016 | `Verify(source)` | pass |
| RH5531 `VerifyNoDiagnosticForMultiAttributeMethodList` | RH5531-003 | `Verify(source)` | pass |
| RH5531 `VerifyCodeFixSplitsMultiLineAccessorListAndMergesInlineAccessorLists` LF+CRLF | RH5531-005, SHARED-ATTR-017, -018 | `Verify(source, fixed)` | pass |
| RH5531 `VerifyDiagnosticWithoutCodeFixWhenCommentIsInsideOrBetweenTheLists` | RH5531-006, SHARED-ATTR-019, -020 | `Verify` + `GetCodeFixActionsAsync` | pass |

## Scenarios (bug fix)

_N/A_: there is no bug fix in this lane.

## Not covered, and why

- **Open question thoenissen/Reihitsu#1109** (what the split fix changes outside the reported list, including LF in a CRLF document). No new test asserts a split outside SHARED-ATTR-017's five scope conditions. Two existing tests already assert out-of-scope output, and I left them untagged and unchanged:
  - RH5528 `VerifyDiagnosticForMergedReturnSpecifierListOnSingleLinePropertyAccessorWithExpressionBody` asserts `public int Value {` / `[return: First]` / `[return: Second]` / `get => 1; }`;
  - RH5518 `VerifyDiagnosticForMergedFieldSpecifierListOnSingleLinePropertyAccessorWithExpressionBody` asserts the same kind of output.

  Whatever the maintainer decides on #1109 will change what these two tests assert.
- **Open question thoenissen/Reihitsu#1110** (Fix All of a split rule needs two passes for two lists on one line). I did not touch any `GetFixAllScenario`; the 14 split scenarios keep `NumberOfFixAllIterations = 2`. The neighbouring shape is covered instead: two lists on separate lines converge in one pass, in RH5506 `VerifyCodeFixSplitsEveryClassAttributeListOnItsOwnLine`.
- **Instances of thoenissen/Reihitsu#1093.** I left these existing tests untagged:
  - RH5524 `VerifyDiagnosticForSplitParameterSpecifierListsOnSingleLinePropertyAccessor` (`param:` lists on an accessor);
  - RH5528 `VerifyNoDiagnosticForSeparateReturnSpecifierListsOnSingleLinePropertyAccessor` and its `CrLf` twin (`[return: A] [return: B] get;`, reported by no rule).

  I added no test for RH5531 merging on a one-line property whose accessor list SHARED-ATTR-006 does not exempt.
- **Lists the compiler ignores (CS0657).** 05-spec-author says a test of RH5518-001 must not use a `field:` list on an accessor. The same reasoning applies to the other specifiers, so I left these existing tests untagged:
  - `VerifyNoDiagnosticForMerged{Assembly,Module,Property,Field,Event}SpecifierListOnSingleLinePropertyAccessor` in RH5502, RH5504, RH5516, RH5518 and RH5520;
  - RH5516 `VerifyNoDiagnosticForPropertyTargetedAttributeListOnTypeParameter`;
  - RH5529 `VerifyDiagnosticAndCodeFixForPropertyTargetedAttributeListsOnTypeParameter` and `VerifyNoDiagnosticForPropertyTargetedAndImplicitAttributeListsOnTypeParameter`.

  For the same reason, my RH5529-001 test leaves out the spec example's `[A] [property: B] V` parameter: 05-spec-author's note to the lead says it may become a #1093 case. The rest of that example is in the test.

  Because of these exclusions, SHARED-ATTR-014 is exercised only on valid placements (RH5528 `return:` and RH5531). Static proof that the other split rules take the same exemption path:
  - `rg -n "IsAttributeListOnInlineAutoPropertyAccessor" src/Reihitsu.Analyzer src/Reihitsu.Analyzer.CodeFixes` shows one call in the split branch of the private `TargetAttributeListShapeAnalyzerBase.OnAttributeList`, and one in the private `TargetAttributeListShapeCodeFixProviderBase.TryGetFixableAttributeList`, which runs from the sealed `RegisterCodeFixesAsync`. The other hits are in the placement bases.
  - No list-shape rule overrides either base method. RH5514 and RH5531 override only `IsAttributeListInScope`, and RH5531 also overrides `ResolveListShapeMode`.
- **Directive arm of SHARED-ATTR-019** is tested only on RH5506. The comment arm is tested on every split rule. Static proof: the guard is the single `SyntaxNodeUtilities.InteriorContainsCommentOrDirective(attributeList)` call in `TargetAttributeListShapeCodeFixProviderBase.TryGetFixableAttributeList` (line 259), and the same check is repeated in `ApplySplitCodeFixAsync`. No provider overrides either.
- **Directives, re-indented comments, and trailing whitespace in front of a split list** (FC-020, FC-021) are outside SHARED-ATTR-017's scope, which is #1109.
- **Failure classes that cannot reach this change:**
  - FC-010, FC-011, FC-012: no formatter change.
  - FC-013, FC-014: the parity tests `RH55xx…FormatterTests` are unchanged, and the run adds no formatter requirement.
  - FC-018, FC-023, FC-036: no rename.
  - FC-022: no literal is rewritten.
  - FC-024, FC-025: the declarations are not rewritten.
  - FC-028: the providers call no formatter API, only `ReihitsuFormatterHelpers.DetectEndOfLine`.
  - FC-029: no key/value layout.
  - FC-031, FC-037: no production comment changed.
  - FC-032: no compiled production change.
  - FC-035: the expected values come from the spec examples.
- **FC-007** (one-pass convergence) is covered by `Verify`, which re-analyzes the fixed document. **FC-009** for exempt lists is the existing, untagged RH5528 `VerifyNoCodeFixForMergedReturnSpecifierListOnSingleLinePropertyAccessor`.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~ShapeRulesAnalyzerTests"
scripts/test.sh --project analyzer --filter "TestCategory=SHARED-ATTR-017"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"
scripts/test.sh --project analyzer
```

Results on this head: the 16 list-shape classes, 200 passed. The SDD architecture tests, 39 passed (the whole architecture project, 72 passed, on 1e3a895). The full analyzer project ran on 1e3a895, before `VerifyDiagnosticForEachOfTwoIdenticalListsOnOneLine` was added: 4753 passed, 0 failed. The only change since then is that one added test, which passes in the focused run.
