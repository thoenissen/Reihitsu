Role: test implementer
Commit: c286825
Verdict: tests written

Lane: specs and tests for existing behavior (widened to tooling for a `squad/` path; no production code changed). The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore. Inputs: `00-lead.md`, `01-spec-author.md`, `02-skeptic.md`, `03-spec-author.md` (where it supersedes 01), and `git diff origin/main -- sdd/specs`.

## What was established, and how

- Every test passes on the unchanged production code at c286825: `Reihitsu.Analyzer.Test.Documentation` 747/747, `Reihitsu.ArchitectureTests` 72/72, `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` included. No test failed, so there is no finding against the specs.
- Each of the 37 new IDs and SHARED-DOC-014 to SHARED-DOC-016 is tagged on at least three test methods. Tag counts per ID (`rg 'Requirement\("<ID>"\)'`): lowest are RH8202-004 = 3, RH8204-003 = 3, and RH8108-002 = 3.
- Each `VerifySpecificationExample…` test uses its spec block verbatim, extracted mechanically from the spec file. The only change is the markup: `{|RH####:…|}` became `{|#n:…|}` with the message checked. In a test of one rule, the markup of the other rules in a SHARED-DOC example was removed.
  - The fix examples (RH8201-005, RH8202-003, RH8204-003) run through the code-fix verifier, which checks one fix at a time and Fix All, and checks that no diagnostic is left.
  - RH8202-004 uses `VerifyNoCodeFixIsOffered`.
- Mutation checks, all reverted, to show that the new tests can fail:
  - Dropping the `#0` marker in `RH8104…VerifyParameterElementWithNoBreakSpaceOrTabIsWithoutText` fails, because an unexpected diagnostic is reported.
  - Adding an end tag in `RH8202…VerifySpecificationExampleWithholdsFixForValueElementWithoutEndTag` fails with "No code fix may be offered".
  - Turning the expected output of `RH8202…InCarriageReturnLineFeedDocument` into LF fails with "content … did not match". So the verifier compares line endings exactly.
- `scripts/format.sh` over the 14 test files changed nothing on the final run. The new CRLF test was aligned once while it was written.
- RH8202 tests are not tagged RH8107-006/RH8107-007. Those are RH8107's requirements, and RH8107's own tests cover them. RH8202-003 and RH8202-004 carry the RH8202 side.

## Tests

Existing tests that were only tagged:

| Test class | Tags added |
|---|---|
| RH8101 (12) | RH8101-001/002 on each diagnostic test; SHARED-DOC-008 on the documentation-mode-none test; SHARED-DOC-012 on `VerifyVerbatimParameterIsMatchedToItsDocumentationByItsValue` |
| RH8102 (7), RH8109 (5) | RH8102-001/002 and RH8109-001/002; SHARED-DOC-015 on the order and nesting tests; SHARED-DOC-008 |
| RH8103 (5), RH8110 (3) | RH8103-001/002 and RH8110-001/002; SHARED-DOC-016 on the missing-name and nesting tests; SHARED-DOC-008 |
| RH8104 (6), RH8106 (5), RH8111 (3) | RH8104-001/002, RH8106-001/002, RH8111-001/002; SHARED-DOC-014 on the empty-element test; SHARED-DOC-008 |
| RH8105 (6), RH8108 (3) | RH8105-001/002 and RH8108-001/002; SHARED-DOC-008 |
| RH8201 (66) | RH8201-001/004/005 on override and fix tests; RH8201-002 on the "only resembles" tests; RH8201-003 on the 15 `#if`/`#region`/`#pragma` tests; SHARED-DOC-008 |
| RH8202 (15) | RH8202-001 through RH8202-004 by shape; SHARED-DOC-008 |
| RH8203 (7), RH8204 (3) | RH8203-001/002 and RH8204-001/002/003; SHARED-DOC-008 |

These existing tests were deliberately left untagged, because they lie outside the coverage of the specs:
- `RH8201…VerifyOnlyFirstDocumentationCommentIsReplaced` and `…VerifyOnlyFirstMultiLineDocumentationCommentIsReplaced`: two documentation comments.
- The two RH8201 partial-method tests: several partial parts.
- `RH8201…UsesEnvironmentEndOfLine` (two tests): these assert `Environment.NewLine`. That is shared behavior, not RH8201-005.

New tests, all in `src/Reihitsu.Analyzer.Test/Documentation/`. Every one passes now, as the lane requires.

| Test | Requirement / failure class | Helper |
|---|---|---|
| RH8101 `VerifySpecificationExampleReportsUndocumentedParameters` | RH8101-001, RH8101-002, SHARED-DOC-009, SHARED-DOC-011 | `Verify` (analyzer) |
| RH8101 `VerifyNoDiagnosticForParametersOfExplicitInterfaceImplementation` | RH8101-001, SHARED-DOC-009, FC-024 | `Verify` |
| RH8101 `VerifyNoDiagnosticWhenLaterDocumentationCommentHasInheritdoc` | RH8101-001, SHARED-DOC-011, FC-003 (top-level vs nested) | `Verify` |
| RH8101 `VerifyParameterIsDocumentedByLaterDocumentationComment` | RH8101-001, SHARED-DOC-012, FC-003 | `Verify` |
| RH8101 `VerifyIncludedDocumentationIsReadForParameters` | RH8101-001, SHARED-DOC-011, SHARED-DOC-012 | `Verify` + `CreateIncludedDocumentationTransform` |
| RH8101 `VerifyDiagnosticForInterfaceMethodParameter` | RH8101-001, RH8101-002 | `Verify` |
| RH8101 `VerifyNoDiagnosticForDocumentedLocalFunctionAndLambda` | RH8101-001 | `Verify` |
| RH8108 `VerifySpecificationExampleReportsUndocumentedTypeParameters` | RH8108-001, RH8108-002, SHARED-DOC-009, SHARED-DOC-011 | `Verify` |
| RH8108 `VerifyNoDiagnosticForTypeParametersOfExplicitInterfaceImplementation` | RH8108-001, SHARED-DOC-009, FC-024 | `Verify` |
| RH8108 `VerifyDiagnosticForStructAndInterfaceTypeParameters` | RH8108-001, RH8108-002 | `Verify` |
| RH8108 `VerifyLaterDocumentationCommentIsReadForTypeParameters` | RH8108-001, SHARED-DOC-011, SHARED-DOC-012 | `Verify` |
| RH8108 `VerifyIncludedDocumentationIsReadForTypeParameters` | RH8108-001, SHARED-DOC-011, SHARED-DOC-012 | `Verify` + include |
| RH8108 `VerifyNoDiagnosticForDocumentedLocalFunction` | RH8108-001 | `Verify` |
| RH8102 `VerifySpecificationExampleReportsParameterElementsThatDoNotFollowTheParameters` | RH8102-001, RH8102-002 | `Verify` |
| RH8102 `VerifySpecificationExamplePairsParameterElementsByPosition` | RH8102-001/002, SHARED-DOC-015, FC-023 (`@event`) | `Verify` |
| RH8102 `VerifyNoDiagnosticForEmptyOrWhiteSpaceNameKeepingItsPosition` | RH8102-001, SHARED-DOC-015 | `Verify` |
| RH8102 `VerifyNoDiagnosticForParameterWithoutElement` | RH8102-001, SHARED-DOC-015 | `Verify` |
| RH8102 `VerifyDiagnosticForConstructorDelegateAndIndexerParameterElements` | RH8102-001, RH8102-002 | `Verify` |
| RH8109 `VerifySpecificationExampleReportsTypeParameterElementsThatDoNotFollowTheTypeParameters` | RH8109-001, RH8109-002 | `Verify` |
| RH8109 `VerifySpecificationExamplePairsTypeParameterElementsByPosition` | RH8109-001/002, SHARED-DOC-015, FC-016 | `Verify` |
| RH8109 `VerifyTypeParameterElementsOfMethodsAreCheckedIncludingExplicitImplementations` | RH8109-001/002, SHARED-DOC-015, FC-024 | `Verify` |
| RH8103 `VerifySpecificationExampleReportsParameterElementsWithoutName` | RH8103-001, RH8103-002 | `Verify` |
| RH8103 `VerifySpecificationExampleReportsMissingEmptyAndWhiteSpaceNames` | RH8103-001/002, SHARED-DOC-016 | `Verify` |
| RH8103 `VerifyDiagnosticForUnnamedParameterElementsOfOtherParameterOwners` | RH8103-001/002, FC-024 | `Verify` |
| RH8110 `VerifySpecificationExampleReportsTypeParameterElementsWithoutName` | RH8110-001, RH8110-002 | `Verify` |
| RH8110 `VerifySpecificationExampleReportsTypeParameterElementWithoutNameAttribute` | RH8110-001/002, SHARED-DOC-016, FC-016 | `Verify` |
| RH8110 `VerifyDiagnosticForWhiteSpaceAndDifferentlyCasedTypeParameterNames` | RH8110-001/002, SHARED-DOC-016 | `Verify` |
| RH8110 `VerifyDiagnosticForUnnamedTypeParameterElementsOfOtherTypeParameterOwners` | RH8110-001/002, FC-024 | `Verify` |
| RH8104 `VerifySpecificationExampleReportsParameterElementsWithoutText` | RH8104-001, RH8104-002, FC-027 (multi-line) | `Verify` |
| RH8104 `VerifySpecificationExampleJudgesParameterTextLikeSummaryText` | RH8104-001/002, SHARED-DOC-014, FC-027 (`/** */`) | `Verify` |
| RH8104 `VerifyDiagnosticForParameterElementWithoutTextWhateverItNames` | RH8104-001/002, FC-024 | `Verify` |
| RH8104 `VerifyParameterElementWithNoBreakSpaceOrTabIsWithoutText` | RH8104-001, SHARED-DOC-014, FC-003 | `Verify` |
| RH8106 `VerifySpecificationExampleReportsReturnsElementsWithoutText` | RH8106-001, RH8106-002, FC-024 | `Verify` |
| RH8106 `VerifySpecificationExampleJudgesReturnsTextLikeSummaryText` | RH8106-001/002, SHARED-DOC-014, FC-016 | `Verify` |
| RH8106 `VerifyDiagnosticForReturnsElementWithoutTextInDelimitedComment` | RH8106-001/002, SHARED-DOC-014, FC-027 | `Verify` |
| RH8111 `VerifySpecificationExampleReportsTypeParameterElementsWithoutText` | RH8111-001, RH8111-002 | `Verify` |
| RH8111 `VerifySpecificationExampleJudgesTypeParameterTextLikeSummaryText` | RH8111-001/002, SHARED-DOC-014, FC-016 | `Verify` |
| RH8111 `VerifyDiagnosticForTypeParameterElementWithoutTextOnOtherDeclarations` | RH8111-001/002, SHARED-DOC-014, FC-024, FC-027 | `Verify` |
| RH8105 `VerifySpecificationExampleReportsMissingReturnValueDescriptions` | RH8105-001, RH8105-002, SHARED-DOC-009 | `Verify` |
| RH8105 `VerifySpecificationExampleReadsIncludedReturnValueDescription` | RH8105-001, RH8105-002 | `Verify` + include |
| RH8105 `VerifyInheritdocInLaterCommentOrIncludedContentCounts` | RH8105-001, FC-003 | `Verify` + include |
| RH8105 `VerifyDiagnosticIsOnRefReadonlyReturnType` | RH8105-001, RH8105-002 | `Verify` |
| RH8105 `VerifyDiagnosticForEveryReturnTypeOtherThanVoid` | RH8105-001/002 (`ValueTask`, `T`, `Task<int>`, interface, extension block, documented local function) | `Verify` |
| RH8201 `VerifySpecificationExampleReportsDocumentedMembersThatInheritTheirDocumentation` | RH8201-001, RH8201-004 | `Verify` (analyzer) |
| RH8201 `VerifySpecificationExampleDoesNotReportMembersThatOnlyResembleInheritedOnes` | RH8201-002 | `Verify` |
| RH8201 `VerifySpecificationExampleConditionalInterfaceListSuppressesOnlyImplicitImplementations` | RH8201-003, RH8201-004, FC-020 | `Verify` |
| RH8201 `VerifySpecificationExampleRegionDirectiveDoesNotSuppressImplicitImplementation` | RH8201-003, RH8201-004, FC-020 | `Verify` |
| RH8201 `VerifySpecificationExampleReplacesCommentWithInheritdoc` | RH8201-004, RH8201-005, FC-021 (comment before, comment after `*/`), FC-007/FC-008 | `Verify(source, fixedSource, …)` |
| RH8201 `VerifyFixKeepsColumnZeroDirectivesAroundComment` | RH8201-005, FC-019/FC-020 (`#pragma` before and after the comment) | `Verify(source, fixedSource, …)` |
| RH8202 `VerifySpecificationExampleReportsValueElementsOnPropertiesAndIndexers` | RH8202-001, RH8202-002, FC-003 (sample before nested) | `Verify` |
| RH8202 `VerifySpecificationExampleRemovesValueElement` | RH8202-002, RH8202-003, FC-007/FC-008 | `Verify(source, fixedSource, …)` |
| RH8202 `VerifySpecificationExampleRemovesValueElementInCarriageReturnLineFeedDocument` | RH8202-003, FC-017/FC-033 (CRLF) | `Verify` + `NormalizeToCarriageReturnLineFeed` |
| RH8202 `VerifySpecificationExampleWithholdsFixForValueElementWithoutEndTag` | RH8202-002, RH8202-004, FC-009 | `VerifyNoCodeFixIsOffered` |
| RH8202 `VerifyDiagnosticForValueElementWhateverTheAccessibilityAndAccessors` | RH8202-001, RH8202-002 | `Verify` |
| RH8203 `VerifySpecificationExampleReportsInheritdocOnDeclarationsThatInheritNothing` | RH8203-001, RH8203-002, FC-024 (explicit `static abstract` operator) | `Verify` |
| RH8203 `VerifySpecificationExampleReportsLaterCommentOnName` | RH8203-001, RH8203-002 | `Verify` |
| RH8203 `VerifySpecificationExampleReportsIncludedInheritdocOnIndexerKeyword` | RH8203-001, RH8203-002 | `Verify` + include |
| RH8203 `VerifyDiagnosticIsOnNameOfDeclarationsDocumentedByLaterComment` | RH8203-002 (field `a`, event `E1`, `+`, `operator`) | `Verify` |
| RH8203 `VerifyDiagnosticForDestructorAndStaticConstructor` | RH8203-001/002, FC-003 (parameterless class and struct constructors not reported) | `Verify` |
| RH8203 `VerifyNoDiagnosticForTypesWithBaseListAndInheritingMembers` | RH8203-001, FC-024 (explicit conversion operator implementation) | `Verify` |
| RH8203 `VerifyIncludedInheritdocWithCrefIsNotReported` | RH8203-001/002, FC-003 | `Verify` + include |
| RH8204 `VerifySpecificationExampleReportsPlaceholderElements` | RH8204-001, RH8204-002 | `Verify` |
| RH8204 `VerifySpecificationExampleReportsNestedPlaceholderElements` | RH8204-001, RH8204-002 | `Verify` (analyzer only, see below) |
| RH8204 `VerifySpecificationExampleReplacesPlaceholderWithItsContent` | RH8204-002, RH8204-003, FC-007/FC-008 | `Verify(source, fixedSource, …)` |
| RH8204 `VerifyDiagnosticForPlaceholderInDelimitedCommentAndOtherDeclarations` | RH8204-001, RH8204-002 | `Verify` |

## Scenarios (bug fix)

_N/A_ — no bug premise in this lane.

## Not covered, and why

Kept out because the specs exclude the input from coverage (`03-spec-author.md`), and it is filed or is to be filed as an open question:
- RH8201-005: any test of indented directives, re-indented comments or attributes, or a member on the `*/` line (RH8201 Q1, P9). Also any CRLF output for RH8201 and RH8204-003 (`[line-ending-drift]`).
- RH8202: explicit implementations, two `<value>` elements, and methods (RH8202 Q1–Q3).
- RH8203: two top-level `<inheritdoc>` elements, a field-like event with several events, and loose constructor matching (RH8203 Q1, Q2).
- RH8204: a nested placeholder under Fix All (Q2). That is why the RH8204-002 example runs through the analyzer only. Also re-serialized content (`&#32;`, `>`, nested elements) (Q1), and comments that document nothing (Q3).
- SHARED-DOC-015: a setter indexer's `value` (S4). Shared rules: the second documentation comment (S1), declarations without parameters for RH8102/RH8109 (S2), and upper-case tag names (S3).

Failure classes walked:
- FC-001, FC-002, FC-015, FC-016:
  - FC-016 is covered: every shared requirement is tested in each sibling rule (SHARED-DOC-014 in RH8104/RH8106/RH8111, SHARED-DOC-015 in RH8102/RH8109, SHARED-DOC-016 in RH8103/RH8110).
  - FC-001, FC-002, and FC-015 concern guards that a change narrows or duplicates. No guard changes in this lane.
- FC-004, FC-005, FC-019–FC-021: they reach only RH8201's fix, which edits leading trivia.
  - Covered inside RH8201-005's coverage: a comment before the reported comment, a comment after `*/`, `#if` and `#pragma` at column 0, and an attribute list after the comment.
  - The RH8202 and RH8204 fixes edit inside the XML of one documentation comment, where no ordinary comment or directive can occur.
- FC-006, FC-022, FC-025, FC-026, FC-029, FC-036: none applies.
  - No rewrite changes code: the fixes edit documentation trivia only.
  - FC-022 does not apply because no literal is in the edited span.
  - RH8105's expression-bodied members appear only as declarations, and their return type is what the diagnostic marks.
- FC-023: `@class`/`@event` in the SHARED-DOC-015 example (RH8102) and in the existing RH8101 verbatim test.
- FC-024: explicit implementations in every rule that checks them (RH8101/RH8108 exempt them; the others report them). `static abstract` operators and conversion operators for RH8201/RH8203. Partial members in several parts are outside coverage.
- FC-007, FC-008, FC-009: covered by the code-fix verifier on the three fix examples and by the existing `FixAllCorrectsEveryDiagnostic` scenarios. FC-009 is covered by RH8202-004.
- FC-018, FC-030: no naming fix exists, so FC-018 does not apply. FC-030 is RH8201 Q1 (filed); it is not asserted.
- FC-010–FC-014, FC-028: not involved. The skeptic established that no formatter phase owns this content.
- FC-017, FC-033: CRLF is tested for RH8202-003, which has no line-ending restriction. For RH8201-005 and RH8204-003 the specs cover `\n` only.
- FC-031, FC-032, FC-035, FC-037: no production change, so FC-031 and FC-032 do not apply. Expected values come from the spec blocks (FC-035). No text-only proof is claimed (FC-037).

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Documentation"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH8101|FullyQualifiedName~RH8102|FullyQualifiedName~RH8103|FullyQualifiedName~RH8104|FullyQualifiedName~RH8105|FullyQualifiedName~RH8106|FullyQualifiedName~RH8108|FullyQualifiedName~RH8109|FullyQualifiedName~RH8110|FullyQualifiedName~RH8111|FullyQualifiedName~RH820"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests"
scripts/test.sh --project analyzer --filter "TestCategory=SHARED-DOC-015"
```
