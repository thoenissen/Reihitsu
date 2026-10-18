Role: test implementer
Commit: 1c9491d
Verdict: tests written

Lane: specs and tests for existing behavior. I read `00-lead.md`, `01-spec-author.md`, `02-skeptic.md`, `03-spec-author.md` (03 supersedes 01), `sdd/specs/shared/attribute-targets.md`, and the 15 analyzer specs. The prompt carried no intended fix, suspected cause, or hoped-for verdict. No production code changed.

Every expected value comes from a spec example or requirement. Every test passes on the unchanged code, and no test contradicts a spec.

## Summary

- Files: the 15 `src/Reihitsu.Analyzer.Test/Formatting/RH55{01,03,05,07,09,11,13,15,17,19,21,23,25,27,30}…PlacementRulesAnalyzerTests.cs`. Nothing else was touched.
- 91 existing test methods were tagged.
- 41 focused tests were added, in RH5501 (1), RH5505 (7), RH5507 (1), RH5511 (2), RH5513 (14), RH5515 (1), RH5517 (2), RH5519 (1), RH5523 (5), RH5527 (5), and RH5530 (2).
- All 74 IDs (RH55xx-00n and SHARED-ATTR-001 to -012) have at least one tagged test. SHARED-ATTR-012 has exactly one.
- Formatter parity tests under `src/Reihitsu.Analyzer.Test/Formatter/Formatting/` were not tagged. They verify FMT behavior and analyzer–formatter parity, and the analyzer requirements only incidentally. This follows the existing precedent: no test under that folder carries a tag.
- `FixAllCorrectsEveryDiagnostic` keeps its inherited SHARED-FIX tags. None of these rules has a Fix All requirement of its own.

## Tests

### Existing tests tagged

| Test (per file) | Requirements | Helper | Expected now |
|---|---|---|---|
| `VerifyDiagnosticAndCodeFixForPolicyViolation`, every file | `<rule>-001`, `-002`, and the fix ID (`-003`, or `-004` for RH5513 and RH5530). RH5513 and RH5530 tag `-003` (location). RH5523 also tags SHARED-ATTR-010, RH5527 SHARED-ATTR-002, RH5530 SHARED-ATTR-008 | `Verify(src, fixed, …)` | pass |
| `VerifyNoDiagnosticForCompliantCode`, every file | `<rule>-001` | `Verify(src)` | pass |
| `VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent`, every file | the withheld ID (`-004`, or `-005` for RH5513 and RH5530), SHARED-ATTR-011 | `Verify` + `GetCodeFixActionsAsync` | pass |
| RH5501 `…WhenDeclarationHasBody`, `…CanonicalIndentation…`; RH5503 `…WhenDeclarationHasBody` | `-003`, SHARED-ATTR-008 | `Verify` | pass |
| RH5501 `…WhenSecondAttributeListViolates` | RH5501-001, SHARED-ATTR-004, -008 | `Verify` | pass |
| RH5501 `…TwoViolatingAttributeListsOnOneLine`, `…MultiLineAttributeList`; RH5511 `…SecondAttributeListStartsOnMultiLineListsContinuationLine` | SHARED-ATTR-004, -007, -008 | `Verify` | pass |
| RH5501, RH5503, RH5515, RH5517, RH5519 `…SpecifierOnSingleLinePropertyAccessor` | `-001`, SHARED-ATTR-006 | `Verify(src)` | pass |
| RH5505 `VerifyInsertedLineBreakUsesDetectedCarriageReturnLineFeedEndOfLine` | RH5505-003, SHARED-ATTR-008 (line-ending clause) | `ApplyCodeFixAsync` | pass |
| RH5505 `VerifyCommentBetweenAttributeListAndMemberIsNotOfferedACodeFix`; RH5523 `VerifyNoCodeFixWhenTrailingCommentBlocksJoin`, `…FollowedByLineComment` | SHARED-ATTR-005 | `GetCodeFixActionsAsync` / `Verify(src)` | pass |
| RH5505 `VerifyNoDiagnosticWhenCommentSitsBetween…`; RH5523 `…CommentBlocksJoin`, `…TrailingCommentBlocksJoin`, `…DirectiveBlocksJoin`; RH5530 `…CommentSitsBetweenAttributeListAndAccessor` | `-001`, SHARED-ATTR-005 | `Verify(src)` | pass |
| RH5511 `VerifyNoCodeFixWhenEnclosingTypeIsMissingClosingBrace` | RH5511-004, SHARED-ATTR-012 | `GetCodeFixActionsAsync` | pass |
| RH5511 `…RatherThanPrecedingDirectiveIndentation`, `…CanonicalIndentation…`, `…ForMultiLineAttributeList` (the last also SHARED-ATTR-004) | RH5511-003, SHARED-ATTR-008 | `Verify` | pass |
| RH5513 `…InsideObjectInitializer`, `…InsideAnonymousObject`; RH5527 `…InsideObjectInitializer` | `-004` / RH5527-003, SHARED-ATTR-009 | `Verify` | pass |
| RH5515 `…PropertyTargetedAttributeOnRecordParameter`; RH5517 `…FieldTargetedAttributeOnRecordParameter` | `-001`, SHARED-ATTR-003 | `Verify(src)` | pass |
| RH5523 `VerifyDiagnosticAndCodeFixForPropertyTargetedAttributeOnRecordParameter` | RH5523-001, -003, SHARED-ATTR-003 | `Verify` | pass |
| RH5523 `…OnRecordParameterLine` | RH5523-001, SHARED-ATTR-003 | `Verify(src)` | pass |
| RH5527: the six single-line no-diagnostic tests (plain, CRLF, interface, initializer, trailing comment, property list above), `…WithExpressionBody`, `…WithCommentInsideAccessorList` | RH5527-001, SHARED-ATTR-006 (FC-003, both sides) | `Verify` | pass |
| RH5527 `…OnMultiLinePropertyAccessor` | RH5527-001, -003, SHARED-ATTR-006, -008 | `Verify` | pass |
| RH5530 `…SingleLinePropertyAccessorAttribute`, `…WithPropertyAttribute`, `…InterfaceMemberAccessorAttributeWithPropertyAttribute` | RH5530-001, SHARED-ATTR-006 | `Verify(src)` | pass |
| RH5530 `…PolicyViolationWithPropertyAttribute` | RH5530-001, -004 | `Verify` | pass |
| RH5530 `VerifyNoDiagnosticForMethodAttributes` | RH5530-002 | `Verify(src)` | pass |

### Added tests

| Test | Requirements | Helper | Expected now |
|---|---|---|---|
| RH5501 `VerifyNoDiagnosticForAssemblySpecifierOnParameter` | RH5501-001, SHARED-ATTR-003 | `Verify(src)` | pass |
| RH5505 `VerifyDiagnosticAndCodeFixForRecordDeclaration`, `…RecordClassDeclaration`, `…TypeSpecifier` | RH5505-001, SHARED-ATTR-001 | `Verify` | pass |
| RH5505 `VerifyNoDiagnosticForRecordStruct` | RH5505-001, SHARED-ATTR-001 | `Verify(src)` | pass |
| RH5505 `VerifyCodeFixIndentsInsideBlockScopedNamespace`, `VerifyCodeFixDoesNotIndentInsideFileScopedNamespace` | RH5505-003, SHARED-ATTR-008 (FC-027) | `Verify` | pass |
| RH5505 `VerifyDiagnosticWithoutCodeFixWhenDirectiveIsInsideAttributeList` | RH5505-004, SHARED-ATTR-011 (FC-005, directive arm) | `Verify` + `GetCodeFixActionsAsync` | pass |
| RH5507 `VerifyDiagnosticAndCodeFixForRecordStruct` | RH5507-001, SHARED-ATTR-001 | `Verify` | pass |
| RH5511 `VerifyDiagnosticAndCodeFixForTypeSpecifier` | RH5511-001, SHARED-ATTR-001 | `Verify` | pass |
| RH5511 `VerifyDiagnosticAndCodeFixForStaticConstructor` | RH5511-001 | `Verify` | pass |
| RH5513 `VerifyDiagnosticAndCodeFixForLocalFunction` | RH5513-001, SHARED-ATTR-001, -008 | `Verify` | pass |
| RH5513 `…ForLambda`, `…ForOperator`, `…ForConversionOperator`, `…ForDestructor`, `…ForTypeSpecifier` | RH5513-001, SHARED-ATTR-001 | `Verify` | pass |
| RH5513 `VerifyDiagnosticAndCodeFixForMethodSpecifierOnProperty` | RH5513-001, SHARED-ATTR-002 | `Verify` | pass |
| RH5513 `VerifyNoDiagnosticForUnknownSpecifier` | RH5513-001, SHARED-ATTR-002 | `Verify(src)` | pass |
| RH5513 `VerifyNoDiagnosticForAccessorAttributeList` | RH5513-002 | `Verify(src)` | pass |
| RH5513 `VerifyDiagnosticAndCodeFixForTwoListsInsideNamespace` (the SHARED-ATTR-008 spec example) | RH5513-003, -004, SHARED-ATTR-004, -007, -008 | `Verify`, 2 diagnostics | pass |
| RH5513 `VerifyCodeFixIndentsInsideSwitchSection`, `…InsideBlockOfSwitchSection` | RH5513-004, SHARED-ATTR-008 (FC-027) | `Verify` | pass |
| RH5513 `VerifyCodeFixAlignsToDeclarationColumnInsideArrayInitializer` (spec example) | RH5513-004, SHARED-ATTR-009 | `Verify` | pass |
| RH5513 `VerifyDiagnosticAndCodeFixWithDocumentationCommentInFrontOfList` (spec example) | RH5513-004, SHARED-ATTR-011 | `Verify` | pass |
| RH5515 `VerifyDiagnosticAndCodeFixForIndexer` | RH5515-001, SHARED-ATTR-001 | `Verify` | pass |
| RH5517 `VerifyDiagnosticAndCodeFixForEnumMember` | RH5517-001, SHARED-ATTR-001, -008 | `Verify` | pass |
| RH5517 `VerifyDiagnosticAndCodeFixForFieldSpecifierOnProperty` | RH5517-001, SHARED-ATTR-002 | `Verify` | pass |
| RH5519 `VerifyDiagnosticAndCodeFixForEventWithAccessors` | RH5519-001, SHARED-ATTR-001 | `Verify` | pass |
| RH5523 `…ForPrimaryConstructorParameter`, `…ForLambdaParameter` | RH5523-001, SHARED-ATTR-001 | `Verify` | pass |
| RH5523 `…ForPropertyTargetedAttributeInMultiParameterRecord` (spec example) | RH5523-001, SHARED-ATTR-003, -004 | `Verify` | pass |
| RH5523 `VerifyCodeFixJoinsListsSeparatedByBlankLine` (spec example) | RH5523-003, SHARED-ATTR-010 | `Verify`, 2 diagnostics, default iteration settings | pass |
| RH5523 `VerifyNoDiagnosticWhenDisabledTextBlocksJoin` | RH5523-001, SHARED-ATTR-005 (FC-021) | `Verify(src)` | pass |
| RH5527 `…ForLocalFunction`, `…ForLambda`, `…ForDelegate` | RH5527-001, SHARED-ATTR-002 | `Verify` | pass |
| RH5527 `VerifyNoDiagnosticForMethodAttributeWithoutReturnSpecifier` | RH5527-001, SHARED-ATTR-002 | `Verify(src)` | pass |
| RH5527 `VerifyNoDiagnosticForReturnSpecifierOnParameter` | RH5527-001, SHARED-ATTR-003 | `Verify(src)` | pass |
| RH5530 `VerifyDiagnosticAndCodeFixForMethodSpecifierOnInitAccessor` | RH5530-001, SHARED-ATTR-002 | `Verify` | pass |
| RH5530 `VerifyDiagnosticAndCodeFixForAddAccessor` | RH5530-001, SHARED-ATTR-001 | `Verify` | pass |

**What makes the negative tests discriminating.** Every added no-diagnostic test puts the list on the same line as the token after it: a parameter list with `assembly:` or `return:`, an unknown specifier, an accessor list for RH5513, a `record struct` for RH5505, and `[First]` for RH5527. A separate-line rule that checked the list would report it. The disabled-text test puts the parameter on a later line, which RH5523 would report.

**Lambda fixes.** RH5513 and RH5527 `…ForLambda` expect `System.Func<int> f = [First]`, then `() => 1;` at column 8. That is SHARED-ATTR-008's depth inside a method body, derived from the spec, not from running the code.

## Scenarios (bug fix)

_N/A_. This run is in the existing-behavior lane.

## Not covered, and why

Deliberately left untagged, because they embody open questions in `03-spec-author.md`. Each still passes and characterizes current behavior.

- **Question 1:** RH5530 `VerifyNoDiagnosticForSingleLineIndexerAccessorAttributeWithPropertyAttribute` and `VerifyNoDiagnosticForSingleLineEventAccessorAttributeWithPropertyAttribute`. Their accessors have bodies.
- **Question 3:** the seven RH5511 tests that put other source in front of the list on its line:
  - `…PrecededByOtherSourceOnSameLine`
  - `VerifyCodeFixUsesSpacesRegardlessOfSurroundingTabIndentation`
  - `…LineBeginsInsideMultiLineComment`
  - `…LineBeginsInsideMultiLineVerbatimString`
  - `…TokenPrecedesMultiLineCommentOnSameLine`
  - `…FollowsMultiLineParameterList`
  - `…FollowsMultiLineInitializer`

  RH5511 `…SecondAttributeListStartsOnMultiLineListsContinuationLine` is tagged. What precedes its second list is the first list of the same declaration, the shape the SHARED-ATTR-004 and SHARED-ATTR-008 examples (`[A] [B] void M()`) specify.
- **Questions 2, 4, 5:** no test exists or was added. That covers `param:`/`typevar:` on other owners, `[typevar: A]` on a parameter, a continuation-indented block, and tabs inside an initializer.

Tagged with a reduced set:

- RH5527 `…WithExpressionBody` and `…WithCommentInsideAccessorList` carry only RH5527-001 and SHARED-ATTR-006 (the reported side).
  - Their fix output (`get` at column 8) is not tagged with SHARED-ATTR-008 or RH5527-003.
  - The accessor list's opening brace sits mid-line, so the input falls outside SHARED-ATTR-008's coverage clause.
- RH5527 `VerifyNoCodeFixForReturnSpecifierOnSingleLinePropertyAccessor` is untagged.
  - Its input carries no diagnostic, so it asks the provider at a synthetic location.
  - No requirement states the provider's answer there; SHARED-ATTR-006 states only that nothing is reported.

Failure classes considered:

- **FC-003:** covered on both sides.
  - SHARED-ATTR-006: inline auto-property against an expression body, a comment inside the accessor list, and a multi-line property.
  - SHARED-ATTR-002/-003: a specifier on a declaration against the same specifier on a parameter.
  - SHARED-ATTR-001: `record struct` checked by RH5507, not RH5505.
- **FC-005, FC-019, FC-020, FC-021** (SHARED-ATTR-005):
  - Covered: comment arms (block comment for RH5505 and RH5530; line comment and own-line comment for RH5523), the `#if` directive arm, and the disabled-text arm (RH5523).
  - Left out: a `#region` case, and directive cases for a separate-line rule.
  - Proof for `#region`: the analyzer's only gap check is `SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` (`rg -n WouldJoinAcrossUnjoinableTrivia src/Reihitsu.Analyzer/Base/TargetAttributePlacementAnalyzerBase.cs`, line 99). It classifies directives through `IsDirectiveOrDisabledTextTrivia`, which is `trivia.IsDirective || DisabledTextTrivia`, true for every directive kind.
  - For a separate-line rule, a directive always starts its own line. The list's `]` and the next token are then on different lines anyway, so such a test could not tell the guard apart from the placement check.
- **FC-008, FC-026:** Fix All runs on `[A] [B]` on one line (every separate-line rule's scenario, plus RH5513 inside a namespace), and on RH5523 lists separated by a blank line. The verifier's default Fix All checks pass, with no extra iteration setting.
- **FC-017, FC-033:** the inserted line break under CRLF is covered by the RH5505 test. The fix text is built in the shared base, `TargetAttributePlacementCodeFixProviderBase`, which all separate-line providers derive from, so I did not add one per rule.
- **FC-027** (SHARED-ATTR-008 scopes): covered.
  - Indented: block-scoped namespace, nested namespace and type, enum, accessor list, method block, switch section with and without a block.
  - Not indented: top level and a file-scoped namespace.
  - SHARED-ATTR-009: object initializer, anonymous object, array initializer.
  - Collection and `with` initializers were not added. `HasAnchorScopeAncestor` matches `InitializerExpressionSyntax` for all of them (one call site), and the array case exercises it.
- **FC-013:** the open-question shapes are not tagged (see above).
- **Naming, Renamer, and formatter idempotency** do not apply: these are layout fixes, and the formatter is out of scope.

Observation: `02-skeptic.md` says the RH5523 blank-line case converges in 2 iterations. The verifier's default Fix All configuration accepts it, so no iteration override was needed and nothing contradicts SHARED-FIX-002.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~MustFollowPlacementRulesAnalyzerTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"
scripts/test.sh --project analyzer --filter "TestCategory=SHARED-ATTR-008"
```

Results on this tree:

- `MustFollowPlacementRulesAnalyzerTests`: 172 passed, 0 failed.
- The three Sdd architecture classes: 39 passed.
- The whole architecture project: 72 passed.
- `scripts/format.sh` over the 15 files changed nothing ("Formatted 0 of 15").
