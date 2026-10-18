Role: test implementer
Commit: 96fbfea
Verdict: tests written

Lane: specs and tests for existing behavior; no production code touched. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples and requirement text of `sdd/specs/analyzer/RH8020.md`–`RH8034.md` and `sdd/specs/shared/element-documentation.md` at 14878f1, not from the code.

Every test in the 15 classes passes on the unchanged analyzers, so no test is a finding against a spec. Every requirement of RH8020–RH8034 and SHARED-DOC-009 to SHARED-DOC-013 now has at least two tagged tests; SHARED-DOC-001 to SHARED-DOC-008 were already covered by the part-1 tests and are tagged again here where a test in these classes verifies them. `RequirementCoverageTests` (architecture project) passes.

Spec example blocks are used verbatim, with each `{|RHxxxx:…|}` span turned into a numbered `{|#n:…|}` marker on the same text. The SHARED-DOC-012 example carries RH8032 and RH8033 marks in one block, so each class runs it with its own rule's marks only.

## Tests

All tests are in `src/Reihitsu.Analyzer.Test/Documentation/RH80xx…AnalyzerTests.cs`, on `AnalyzerTestsBase<TAnalyzer>.Verify`. The 52 existing tests are tagged and their assertions are unchanged; the other 114 are new. In the Helper column, "None mode" or "Parse mode" means `ApplyDocumentationMode{None,Parse}ToTestProject`, "include resolver" means `CreateIncludedDocumentationTransform`, "second file" means `TestState.Sources.Add`, and "CRLF" means `source.ReplaceLineEndings("\r\n")`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH8020.VerifyDiagnosticForMethodWithoutDocumentation` (tagged) | RH8020-001, RH8020-002 | Verify | pass |
| `RH8020.VerifyNoDiagnosticsWhenDocumentationModeIsNone` (tagged) | RH8020-001, SHARED-DOC-008 | Verify, None mode | pass |
| `RH8020.VerifyNoDiagnosticForImplicitlyPrivateMethod` (tagged) | RH8020-001, SHARED-DOC-002 | Verify | pass |
| `RH8020.VerifyDiagnosticForUndocumentedExtensionMemberMethod` (tagged) | RH8020-001, RH8020-002, SHARED-DOC-001 | Verify | pass |
| `RH8020.VerifyNoDiagnosticForImplicitlyPrivateExtensionMemberMethod` (tagged) | RH8020-001, SHARED-DOC-002 | Verify | pass |
| `RH8020.VerifySpecificationExampleReportsUndocumentedNonPrivateMethods` (new) | RH8020-001, RH8020-002 | Verify | pass |
| `RH8020.VerifyDiagnosticForGenericMethodIsLocatedOnNameWithoutTypeParameterList` (new) | RH8020-002 | Verify | pass |
| `RH8020.VerifyDiagnosticForMethodWithVerbatimIdentifierCoversTheAtSign` (new) | RH8020-002 | Verify | pass |
| `RH8020.VerifyDiagnosticForInterfaceMethodsWithoutAccessModifier` (new) | RH8020-001, SHARED-DOC-002 | Verify | pass |
| `RH8020.VerifyDiagnosticForMethodsOfStructRecordAndRecordStruct` (new) | RH8020-001, SHARED-DOC-001 | Verify | pass |
| `RH8020.VerifyDiagnosticForMethodsWhateverTheirBodyForm` (new) | RH8020-001 | Verify | pass |
| `RH8020.VerifyDiagnosticForPrivateProtectedAndProtectedInternalMethods` (new) | RH8020-001, SHARED-DOC-002 | Verify | pass |
| `RH8020.VerifyDiagnosticForNonPrivateMethodsOfPrivateAndFileLocalTypes` (new) | RH8020-001, SHARED-DOC-002 | Verify | pass |
| `RH8020.VerifySpecificationExampleExplicitInterfaceImplementationInInterfaceIsNotReported` (new) | RH8020-001, SHARED-DOC-009 | Verify | pass |
| `RH8020.VerifyDiagnosticForMethodDocumentedOnlyAfterItsAttributeList` (new) | RH8020-001, SHARED-DOC-004 | Verify | pass |
| `RH8020.VerifyDiagnosticForMethodWithSummaryNestedInRemarks` (new) | RH8020-001, SHARED-DOC-005 | Verify | pass |
| `RH8020.VerifyMethodSummaryInLaterDocumentationCommentCounts` (new) | RH8020-001, SHARED-DOC-006 | Verify | pass |
| `RH8020.VerifyMethodSummaryInIncludedFileCounts` (new) | RH8020-001, SHARED-DOC-007 | Verify, include resolver | pass |
| `RH8020.VerifyDiagnosticForUndocumentedMethodInParseMode` (new) | RH8020-001, SHARED-DOC-008 | Verify, Parse mode | pass |
| `RH8020.VerifyExtensionBlockMembersAreDecidedOnTheirOwn` (new) | RH8020-001, SHARED-DOC-001 | Verify | pass |
| `RH8021.VerifyDiagnosticForPrivateMethodWithoutDocumentation` (tagged) | RH8021-001, RH8021-002 | Verify | pass |
| `RH8021.VerifyDiagnosticForImplicitlyPrivateMethodWithoutDocumentation` (tagged) | RH8021-001, SHARED-DOC-002 | Verify | pass |
| `RH8021.VerifyNoDiagnosticsWhenDocumentationModeIsNone` (tagged) | RH8021-001, SHARED-DOC-008 | Verify, None mode | pass |
| `RH8021.VerifyDiagnosticForImplicitlyPrivateExtensionMemberMethod` (tagged) | RH8021-001, SHARED-DOC-002 | Verify | pass |
| `RH8021.VerifySpecificationExampleReportsUndocumentedPrivateMethods` (new) | RH8021-001, RH8021-002 | Verify | pass |
| `RH8021.VerifySpecificationExampleExplicitInterfaceImplementationInClassIsNotReported` (new) | RH8021-001, SHARED-DOC-009 | Verify | pass |
| `RH8021.VerifyNoDiagnosticForExplicitGenericMethodImplementation` (new) | RH8021-001, SHARED-DOC-009 | Verify | pass |
| `RH8021.VerifySpecificationExamplePrivateGroupInExtensionBlock` (new) | RH8021-001, SHARED-DOC-002 | Verify | pass |
| `RH8021.VerifyDiagnosticForImplicitlyPrivateMethodsOfStructRecordAndRecordStruct` (new) | RH8021-001, SHARED-DOC-002 | Verify | pass |
| `RH8021.VerifyNoDiagnosticForNonPrivateMethods` (new) | RH8021-001, SHARED-DOC-002 | Verify | pass |
| `RH8021.VerifyDiagnosticForPrivateMethodsOfFileLocalAndPublicNestedTypes` (new) | RH8021-001, SHARED-DOC-002 | Verify | pass |
| `RH8021.VerifyDiagnosticForGenericPrivateMethodIsLocatedOnNameWithoutTypeParameterList` (new) | RH8021-002 | Verify | pass |
| `RH8022.VerifyDiagnosticForPropertyWithoutDocumentation` (tagged) | RH8022-001, RH8022-002 | Verify | pass |
| `RH8022.VerifyNoDiagnosticsWhenDocumentationModeIsNone` (tagged) | RH8022-001, SHARED-DOC-008 | Verify, None mode | pass |
| `RH8022.VerifyDiagnosticForUndocumentedExtensionMemberProperty` (tagged) | RH8022-001, RH8022-002, SHARED-DOC-001 | Verify | pass |
| `RH8022.VerifySpecificationExampleReportsUndocumentedNonPrivateProperties` (new) | RH8022-001, RH8022-002 | Verify | pass |
| `RH8022.VerifyGroupIsDecidedByPropertyModifiersNotByAccessorModifiers` (new) | RH8022-001, SHARED-DOC-002 | Verify | pass |
| `RH8022.VerifyDiagnosticForInterfacePropertiesWithoutAccessModifier` (new) | RH8022-001, SHARED-DOC-002 | Verify | pass |
| `RH8022.VerifyDiagnosticForPropertiesWhateverTheirForm` (new) | RH8022-001 | Verify | pass |
| `RH8022.VerifyDiagnosticForPropertiesOfStructRecordAndRecordStruct` (new) | RH8022-001, SHARED-DOC-001 | Verify | pass |
| `RH8022.VerifyNoDiagnosticForExplicitPropertyImplementationInInterface` (new) | RH8022-001, SHARED-DOC-009 | Verify | pass |
| `RH8022.VerifyDiagnosticForPropertyWithVerbatimIdentifierCoversTheAtSign` (new) | RH8022-002 | Verify | pass |
| `RH8022.VerifyGroupOfExtensionBlockProperties` (new) | RH8022-001, SHARED-DOC-002 | Verify | pass |
| `RH8023.VerifyDiagnosticForPrivatePropertyWithoutDocumentation` (tagged) | RH8023-001, RH8023-002 | Verify | pass |
| `RH8023.VerifyNoDiagnosticsWhenDocumentationModeIsNone` (tagged) | RH8023-001, SHARED-DOC-008 | Verify, None mode | pass |
| `RH8023.VerifySpecificationExampleReportsUndocumentedPrivateProperties` (new) | RH8023-001, RH8023-002 | Verify | pass |
| `RH8023.VerifyNoDiagnosticForExplicitPropertyImplementationInClass` (new) | RH8023-001, SHARED-DOC-009 | Verify | pass |
| `RH8023.VerifyDiagnosticForImplicitlyPrivatePropertiesOfStructRecordRecordStructAndExtensionBlock` (new) | RH8023-001, SHARED-DOC-002 | Verify | pass |
| `RH8023.VerifyNoDiagnosticForNonPrivateProperties` (new) | RH8023-001, SHARED-DOC-002 | Verify | pass |
| `RH8023.VerifyDiagnosticForPrivateInterfaceProperty` (new) | RH8023-001, SHARED-DOC-002 | Verify | pass |
| `RH8023.VerifyNoDiagnosticForPrivateIndexer` (new) | RH8023-001 | Verify | pass |
| `RH8024.VerifyDiagnosticForIndexerWithoutDocumentation` (tagged) | RH8024-001, RH8024-002 | Verify | pass |
| `RH8024.VerifyNoDiagnosticsWhenDocumentationModeIsNone` (tagged) | RH8024-001, SHARED-DOC-008 | Verify, None mode | pass |
| `RH8024.VerifySpecificationExampleReportsUndocumentedNonPrivateIndexers` (new) | RH8024-001, RH8024-002 | Verify | pass |
| `RH8024.VerifyDiagnosticForIndexersOfStructRecordAndRecordStruct` (new) | RH8024-001, SHARED-DOC-001 | Verify | pass |
| `RH8024.VerifyGroupIsDecidedByIndexerModifiersNotByAccessorModifiers` (new) | RH8024-001, SHARED-DOC-002 | Verify | pass |
| `RH8024.VerifyNoDiagnosticForExplicitIndexerImplementationInInterface` (new) | RH8024-001, SHARED-DOC-009 | Verify | pass |
| `RH8024.VerifyNoDiagnosticForProperty` (new) | RH8024-001 | Verify | pass |
| `RH8025.VerifyDiagnosticForPrivateIndexerWithoutDocumentation` (tagged) | RH8025-001, RH8025-002 | Verify | pass |
| `RH8025.VerifyNoDiagnosticsWhenDocumentationModeIsNone` (tagged) | RH8025-001, SHARED-DOC-008 | Verify, None mode | pass |
| `RH8025.VerifySpecificationExampleReportsUndocumentedPrivateIndexers` (new) | RH8025-001, RH8025-002 | Verify | pass |
| `RH8025.VerifyNoDiagnosticForExplicitIndexerImplementationInClass` (new) | RH8025-001, SHARED-DOC-009 | Verify | pass |
| `RH8025.VerifyDiagnosticForImplicitlyPrivateIndexersOfStructRecordAndRecordStruct` (new) | RH8025-001, SHARED-DOC-002 | Verify | pass |
| `RH8025.VerifyNoDiagnosticForNonPrivateIndexers` (new) | RH8025-001, SHARED-DOC-002 | Verify | pass |
| `RH8025.VerifyDiagnosticForPrivateInterfaceIndexer` (new) | RH8025-001, SHARED-DOC-002 | Verify | pass |
| `RH8026.VerifyDiagnosticForFieldWithoutDocumentation` (tagged) | RH8026-001, RH8026-002 | Verify | pass |
| `RH8026.VerifyNoDiagnosticsWhenDocumentationModeIsNone` (tagged) | RH8026-001, SHARED-DOC-008 | Verify, None mode | pass |
| `RH8026.VerifySpecificationExampleReportsUndocumentedNonPrivateFields` (new) | RH8026-001, RH8026-002 | Verify | pass |
| `RH8026.VerifyDiagnosticForDeclarationOfSeveralFieldsIsLocatedOnFirstName` (new) | RH8026-001, RH8026-002 | Verify | pass |
| `RH8026.VerifyDiagnosticForFieldsOfStructRecordAndRecordStruct` (new) | RH8026-001, SHARED-DOC-001 | Verify | pass |
| `RH8026.VerifyNoDiagnosticForPrivateFieldsEventsAndEnumMembers` (new) | RH8026-001, SHARED-DOC-002 | Verify | pass |
| `RH8026.VerifyDiagnosticForPrivateProtectedFieldAndPublicFieldOfPrivateType` (new) | RH8026-001, SHARED-DOC-002 | Verify | pass |
| `RH8026.VerifyDiagnosticForFieldWithVerbatimIdentifierCoversTheAtSign` (new) | RH8026-002 | Verify | pass |
| `RH8027.VerifyDiagnosticForPrivateFieldWithoutDocumentation` (tagged) | RH8027-001, RH8027-002 | Verify | pass |
| `RH8027.VerifyNoDiagnosticsWhenDocumentationModeIsNone` (tagged) | RH8027-001, SHARED-DOC-008 | Verify, None mode | pass |
| `RH8027.VerifySpecificationExampleReportsUndocumentedPrivateFields` (new) | RH8027-001, RH8027-002 | Verify | pass |
| `RH8027.VerifyDiagnosticForImplicitlyPrivateFieldsOfStructRecordAndRecordStruct` (new) | RH8027-001, SHARED-DOC-002 | Verify | pass |
| `RH8027.VerifyNoDiagnosticForNonPrivateFieldsAndPrivateEvents` (new) | RH8027-001, SHARED-DOC-002 | Verify | pass |
| `RH8027.VerifyDiagnosticForPrivateStaticInterfaceField` (new) | RH8027-001, SHARED-DOC-002 | Verify | pass |
| `RH8028.VerifyDiagnosticForEventWithoutDocumentation` (tagged) | RH8028-001, RH8028-002 | Verify | pass |
| `RH8028.VerifyNoDiagnosticForEventWithSummary` (tagged) | RH8028-001 | Verify | pass |
| `RH8028.VerifyNoDiagnosticForExplicitEventWithSummary` (tagged) | RH8028-001 | Verify | pass |
| `RH8028.VerifyNoDiagnosticsWhenDocumentationModeIsNone` (tagged) | RH8028-001, SHARED-DOC-008 | Verify, None mode | pass |
| `RH8028.VerifySpecificationExampleReportsUndocumentedNonPrivateEvents` (new) | RH8028-001, RH8028-002 | Verify | pass |
| `RH8028.VerifyDiagnosticForFieldLikeDeclarationOfSeveralEventsIsLocatedOnFirstName` (new) | RH8028-001, RH8028-002 | Verify | pass |
| `RH8028.VerifyDiagnosticForEventsOfStructRecordRecordStructAndInterface` (new) | RH8028-001, SHARED-DOC-001, SHARED-DOC-002 | Verify | pass |
| `RH8028.VerifyNoDiagnosticForExplicitEventImplementationInInterface` (new) | RH8028-001, SHARED-DOC-009 | Verify | pass |
| `RH8028.VerifyGroupOfEvents` (new) | RH8028-001, SHARED-DOC-002 | Verify | pass |
| `RH8029.VerifyDiagnosticForPrivateEventWithoutDocumentation` (tagged) | RH8029-001, RH8029-002 | Verify | pass |
| `RH8029.VerifyNoDiagnosticsWhenDocumentationModeIsNone` (tagged) | RH8029-001, SHARED-DOC-008 | Verify, None mode | pass |
| `RH8029.VerifySpecificationExampleReportsUndocumentedPrivateEvents` (new) | RH8029-001, RH8029-002 | Verify | pass |
| `RH8029.VerifyNoDiagnosticForExplicitEventImplementationInClass` (new) | RH8029-001, SHARED-DOC-009 | Verify | pass |
| `RH8029.VerifyDiagnosticForImplicitlyPrivateEventsOfStructRecordAndRecordStruct` (new) | RH8029-001, RH8029-002, SHARED-DOC-002 | Verify | pass |
| `RH8029.VerifyNoDiagnosticForNonPrivateEvents` (new) | RH8029-001, SHARED-DOC-002 | Verify | pass |
| `RH8030.VerifyDiagnosticForEmptySummary` (tagged) | RH8030-001, RH8030-002, SHARED-DOC-010 | Verify | pass |
| `RH8030.VerifyDiagnosticForEnumMemberWithEmptySummary` (tagged) | RH8030-001 | Verify | pass |
| `RH8030.VerifyDiagnosticForOperatorWithEmptySummary` (tagged) | RH8030-001 | Verify | pass |
| `RH8030.VerifyDiagnosticForConversionOperatorWithEmptySummary` (tagged) | RH8030-001 | Verify | pass |
| `RH8030.VerifyNoDiagnosticForClassWithNonEmptySummary` (tagged) | RH8030-001, SHARED-DOC-010 | Verify | pass |
| `RH8030.VerifyNoDiagnosticForEmptySummaryNestedInRemarks` (tagged) | RH8030-001 | Verify | pass |
| `RH8030.VerifyNoDiagnosticsWhenDocumentationModeIsNone` (tagged) | RH8030-001, SHARED-DOC-008 | Verify, None mode | pass |
| `RH8030.VerifySpecificationExampleReportsSummariesWithoutText` (new) | RH8030-001, RH8030-002 | Verify | pass |
| `RH8030.VerifySpecificationExampleOfSummaryWithoutTextWithLineFeeds` (new) | RH8030-001, RH8030-002, SHARED-DOC-010 | Verify | pass |
| `RH8030.VerifySpecificationExampleOfSummaryWithoutTextWithCarriageReturnLineFeeds` (new) | RH8030-001, RH8030-002, SHARED-DOC-010 | Verify, CRLF | pass |
| `RH8030.VerifyDiagnosticForSummaryOfUnicodeWhiteSpaceOnly` (new) | RH8030-001, SHARED-DOC-010 | Verify | pass |
| `RH8030.VerifyStarsOfBlockDocumentationComments` (new) | RH8030-001, SHARED-DOC-010 | Verify | pass |
| `RH8030.VerifyDiagnosticForSummariesWithoutTextOfEveryTypeKind` (new) | RH8030-001 | Verify | pass |
| `RH8030.VerifyDiagnosticForSummariesWithoutTextOfEveryMemberKind` (new) | RH8030-001 | Verify | pass |
| `RH8030.VerifyDiagnosticForSummariesWithoutTextWhateverAccessibilityAndNesting` (new) | RH8030-001 | Verify | pass |
| `RH8030.VerifyNoDiagnosticForExtensionDeclarationButForItsMember` (new) | RH8030-001 | Verify | pass |
| `RH8030.VerifyDiagnosticForDeclarationOfSeveralFieldsOrEventsIsReportedOnce` (new) | RH8030-001, RH8030-002 | Verify | pass |
| `RH8030.VerifySummaryPlacementFollowsTheDocumentationCommentsOfTheDeclaration` (new) | RH8030-001, SHARED-DOC-004 | Verify | pass |
| `RH8030.VerifyDiagnosticForSummaryWithoutTextInParseMode` (new) | RH8030-001, SHARED-DOC-008 | Verify, Parse mode | pass |
| `RH8031.VerifyDiagnosticForMissingDocumentationOnExtensionDeclaration` (tagged) | RH8031-001, RH8031-003 | Verify | pass |
| `RH8031.VerifyDiagnosticForEmptySummaryOnExtensionDeclaration` (tagged) | RH8031-002, RH8031-003, SHARED-DOC-010 | Verify | pass |
| `RH8031.VerifyNoDiagnosticForGenericExtensionDeclarationWithSummary` (tagged) | RH8031-001 | Verify | pass |
| `RH8031.VerifyDiagnosticForSummaryNestedInRemarks` (tagged) | RH8031-001, RH8031-003 | Verify | pass |
| `RH8031.VerifyNoDiagnosticsWhenDocumentationModeIsNone` (tagged) | RH8031-001, SHARED-DOC-008 | Verify, None mode | pass |
| `RH8031.VerifySpecificationExampleReportsExtensionDeclarationsWithoutSummary` (new) | RH8031-001, RH8031-003 | Verify | pass |
| `RH8031.VerifySpecificationExampleReportsExtensionDeclarationSummariesWithoutText` (new) | RH8031-002, RH8031-003 | Verify | pass |
| `RH8031.VerifyDiagnosticForExtensionDeclarationsWithoutSummaryElement` (new) | RH8031-001, RH8031-003 | Verify | pass |
| `RH8031.VerifyDiagnosticForExtensionDeclarationSummaryFormsWithoutTextWithLineFeeds` (new) | RH8031-002, RH8031-003, SHARED-DOC-010 | Verify | pass |
| `RH8031.VerifyDiagnosticForExtensionDeclarationSummaryFormsWithoutTextWithCarriageReturnLineFeeds` (new) | RH8031-002, RH8031-003, SHARED-DOC-010 | Verify, CRLF | pass |
| `RH8031.VerifyNoDiagnosticForExtensionDeclarationSummariesWithText` (new) | RH8031-002, SHARED-DOC-010 | Verify | pass |
| `RH8031.VerifyNoDiagnosticForDocumentedExtensionDeclarationOfUndocumentedClass` (new) | RH8031-001 | Verify | pass |
| `RH8031.VerifyDocumentationCommentBeforeDirectiveOrOrdinaryCommentBelongsToExtensionDeclaration` (new) | RH8031-001, RH8031-002, SHARED-DOC-004 | Verify | pass |
| `RH8031.VerifyDiagnosticForExtensionDeclarationWithoutSummaryInParseMode` (new) | RH8031-001, SHARED-DOC-008 | Verify, Parse mode | pass |
| `RH8032.VerifyDiagnosticForMissingExtensionDeclarationParameterDocumentation` (tagged) | RH8032-001, RH8032-002, SHARED-DOC-012 | Verify | pass |
| `RH8032.VerifyNoDiagnosticsForDocumentedGenericExtensionDeclaration` (tagged) | RH8032-001, SHARED-DOC-012 | Verify | pass |
| `RH8032.VerifyNoDiagnosticsWhenDocumentationModeIsNone` (tagged) | RH8032-001, SHARED-DOC-008 | Verify, None mode | pass |
| `RH8032.VerifySpecificationExampleReportsUndocumentedExtensionParameter` (new) | RH8032-001, RH8032-002 | Verify | pass |
| `RH8032.VerifySpecificationExampleParametersAreCheckedOnlyWithoutInheritdoc` (new) | RH8032-001, SHARED-DOC-011 | Verify | pass |
| `RH8032.VerifyInheritdocInIncludedFileExemptsParameter` (new) | RH8032-001, SHARED-DOC-011 | Verify, include resolver | pass |
| `RH8032.VerifySpecificationExampleParameterIsDocumentedByTopLevelElementWithItsName` (new) | RH8032-001, RH8032-002, SHARED-DOC-012 | Verify | pass |
| `RH8032.VerifySpecificationExampleParameterIsDocumentedByIncludedFile` (new) | RH8032-001, SHARED-DOC-012 | Verify, include resolver | pass |
| `RH8032.VerifyParameterDocumentedInLaterCommentOrWithoutTextCounts` (new) | RH8032-001, SHARED-DOC-012 | Verify | pass |
| `RH8032.VerifySpecificationExampleMergedExtensionDeclarationsShareDocumentation` (new) | RH8032-001, SHARED-DOC-011, SHARED-DOC-012, SHARED-DOC-013 | Verify | pass |
| `RH8032.VerifyMergedExtensionDeclarationsAcrossFilesOfPartialClassShareDocumentation` (new) | RH8032-001, SHARED-DOC-013 | Verify, second file | pass |
| `RH8032.VerifyUndocumentedMergedExtensionDeclarationIsNotReported` (new) | RH8032-001, SHARED-DOC-011, SHARED-DOC-013 | Verify | pass |
| `RH8032.VerifyExtensionDeclarationsWithDifferentReferenceModifiersDoNotShareDocumentation` (new) | RH8032-001, SHARED-DOC-013 | Verify | pass |
| `RH8032.VerifyExtensionDeclarationsWithDifferentParameterAttributesDoNotShareDocumentation` (new) | RH8032-001, SHARED-DOC-013 | Verify | pass |
| `RH8032.VerifyAnnotatedAndObliviousReceiverTypesDoNotShareDocumentation` (new) | RH8032-001, SHARED-DOC-013 | Verify | pass |
| `RH8032.VerifyDiagnosticForUndocumentedExtensionParameterInParseMode` (new) | RH8032-001, SHARED-DOC-008 | Verify, Parse mode | pass |
| `RH8033.VerifyDiagnosticForMissingTypeParameterDocumentationOnExtensionDeclaration` (tagged) | RH8033-001, RH8033-002, SHARED-DOC-012 | Verify | pass |
| `RH8033.VerifyNoDiagnosticsForDocumentedTypeParameterOnExtensionDeclaration` (tagged) | RH8033-001, SHARED-DOC-012 | Verify | pass |
| `RH8033.VerifyNoDiagnosticsWhenDocumentationModeIsNone` (tagged) | RH8033-001, SHARED-DOC-008 | Verify, None mode | pass |
| `RH8033.VerifySpecificationExampleReportsUndocumentedExtensionTypeParameter` (new) | RH8033-001, RH8033-002 | Verify | pass |
| `RH8033.VerifySpecificationExampleTypeParameterIsDocumentedByTopLevelElementWithItsName` (new) | RH8033-001, RH8033-002, SHARED-DOC-012 | Verify | pass |
| `RH8033.VerifySpecificationExampleTypeParameterIsDocumentedByIncludedFile` (new) | RH8033-001, SHARED-DOC-012 | Verify, include resolver | pass |
| `RH8033.VerifyTypeParametersAreCheckedOnlyWithoutInheritdoc` (new) | RH8033-001, SHARED-DOC-011 | Verify, include resolver | pass |
| `RH8033.VerifyDiagnosticForEachUndocumentedTypeParameter` (new) | RH8033-001, RH8033-002, SHARED-DOC-012 | Verify | pass |
| `RH8033.VerifyMergedExtensionDeclarationsShareTypeParameterDocumentation` (new) | RH8033-001, SHARED-DOC-013 | Verify | pass |
| `RH8033.VerifyExtensionDeclarationsWithDifferentTypeParametersDoNotShareDocumentation` (new) | RH8033-001, SHARED-DOC-013 | Verify | pass |
| `RH8033.VerifyDiagnosticForUndocumentedExtensionTypeParameterInParseMode` (new) | RH8033-001, SHARED-DOC-008 | Verify, Parse mode | pass |
| `RH8034.VerifyDiagnosticForOperatorWithoutDocumentation` (tagged) | RH8034-001, RH8034-002 | Verify | pass |
| `RH8034.VerifyNoDiagnosticForOperatorWithSummary` (tagged) | RH8034-001 | Verify | pass |
| `RH8034.VerifyNoDiagnosticForOperatorWithInheritdoc` (tagged) | RH8034-001, SHARED-DOC-005 | Verify | pass |
| `RH8034.VerifyDiagnosticForConversionOperatorWithoutDocumentation` (tagged) | RH8034-001, RH8034-002 | Verify | pass |
| `RH8034.VerifyNoDiagnosticForExplicitlyImplementedInterfaceOperator` (tagged) | RH8034-001, SHARED-DOC-009 | Verify | pass |
| `RH8034.VerifyNoDiagnosticsWhenDocumentationModeIsNone` (tagged) | RH8034-001, SHARED-DOC-008 | Verify, None mode | pass |
| `RH8034.VerifySpecificationExampleReportsUndocumentedOperators` (new) | RH8034-001, RH8034-002 | Verify | pass |
| `RH8034.VerifyDiagnosticForUnaryAndTrueFalseOperatorsIsLocatedOnOperatorToken` (new) | RH8034-001, RH8034-002 | Verify | pass |
| `RH8034.VerifyDiagnosticForConversionOperatorsIsLocatedOnOperatorKeyword` (new) | RH8034-001, RH8034-002 | Verify | pass |
| `RH8034.VerifyDiagnosticForOperatorsOfClassRecordRecordStructAndExtensionBlock` (new) | RH8034-001, SHARED-DOC-001 | Verify | pass |
| `RH8034.VerifyDiagnosticForInterfaceOperatorsWhateverTheirModifiers` (new) | RH8034-001 | Verify | pass |
| `RH8034.VerifyNoDiagnosticForExplicitlyImplementedConversionOperator` (new) | RH8034-001, SHARED-DOC-009 | Verify | pass |

Failure classes the tests target, as `02-skeptic.md` and `04-skeptic.md` listed them:

- FC-002: "top level" (summary, inheritdoc, param, and typeparam nested in remarks); first versus later comment; tag-name case (`name="Value"`, `name="t"`).
- FC-003, both sides of each condition:
  - empty versus text for every SHARED-DOC-010 form (`<summary/>`, empty element, `///` continuation, `&#32;`, `&#9;`, `&#160;`, `&#xA0;`, `&#x2003;`, `&#x85;`, `*` and `***` continuations; text: `&#x200B;`, ` * * `, mid-line `*`, `* Text`);
  - inheritdoc present versus absent (RH8030 still reports; RH8032 and RH8033 are exempt);
  - the modifier-less group in class, struct, record, record struct, interface, and extension block;
  - `private protected` versus `private`; accessor versus member modifiers.
- FC-016: `EventDeclaration` and `EventFieldDeclaration`; operator, conversion, checked operator and checked conversion, compound assignment, `true`/`false`/`>>>`/`++`/`!`; declarations of several fields or events; explicit implementation of a method, generic method, property, indexer, and event in a class and in an interface, and of an operator and a conversion in a struct.
- FC-023: `@event`, `@class` member names (the span includes `@`); `extension(byte @class)` with `name="class"`, `name="@class"`; `<@T>` with `name="T"` and `name="@T"`.
- FC-024: static abstract interface members (method, property, event, operator, conversion); a private interface method, property, indexer, and static field; `file` class members; no test asserts a per-part verdict of a partial element.
- FC-025: abstract, expression-bodied, block-bodied, and extern methods; auto-implemented, expression-bodied, accessor-bodied, abstract, override, and static properties.
- FC-033: no test source repeats an extension receiver except the SHARED-DOC-013 tests about merging; SHARED-DOC-010 runs under LF and CRLF for RH8030 and RH8031; SHARED-DOC-008 runs in `None` mode for every rule, and in `Parse` mode for RH8020, RH8030, RH8031, RH8032, and RH8033.
- FC-034: every spec example block is a test.

## Scenarios (bug fix)

_N/A._ The lane writes specs for existing behavior.

## Not covered, and why

- Cases left out of coverage by the specs' coverage sentences and filed as open questions (`05-spec-author.md`). No test asserts their current verdict on either side:
  - partial members (shared Q1);
  - markup-only summaries (Q2);
  - upper-case or namespaced `param`/`typeparam`/`inheritdoc`/`summary` (Q3, Q4, RH8030 Q2, RH8031 Q1);
  - padded `name` (Q5);
  - a malformed sibling of a merged extension (Q6), and malformed XML anywhere;
  - explicit implementations under RH8030 (RH8030 Q1);
  - several comments, a second summary, an include, or partial parts under RH8030/RH8031 (RH8030 Q3, RH8031 Q2), and inheritdoc under RH8031 (RH8031 Q3);
  - the unnamed receiver `extension(long)` (RH8032 Q1).

  One input of mine fell into this group, an unclosed `/** <summary>` (malformed). I removed it before committing.
- SHARED-DOC-004 to SHARED-DOC-008 in every presence rule. These are shared arms. `rg -n "HasRequiredDocumentation|MatchesAccessibilityGroup|GetDiagnosticLocation|NeedsDocumentation" src/Reihitsu.Analyzer/Base/SplitElementDocumentationAnalyzerBase.cs` shows that RH8020–RH8029 and RH8034 all decide in `SplitElementDocumentationAnalyzerBase.OnDeclaration` through `DocumentationAnalysisUtilities.HasRequiredDocumentation(MemberDeclarationSyntax, …)`. That method takes no parameter for the syntax kind. SHARED-DOC-004 to SHARED-DOC-007 therefore have one focused test each in RH8020 next to the part-1 tests, and SHARED-DOC-008 is tested in every class. The kind-specific parts are tested per rule: the kind switch in `GetDiagnosticLocation`, the kind switch in `IsExplicitInterfaceImplementation`, and the registered syntax kinds.
- Declarations the specs leave out of their kind lists, because they do not compile:
  - an indexer in an extension block (CS8652, preview);
  - a field, event, or conversion operator in an extension block (CS9282).
- Extension-block indexers and properties beyond what RH8022/RH8023 need: RH8024/RH8025 do not list the extension block.
- Failure classes that cannot reach the run. These rules have no code fix, no Fix All, no rewrite, and no formatter phase, and the run changes no production code: FC-001, FC-004 to FC-014, FC-015, FC-017 (no output to assert other than the diagnostic set, which the markup asserts exactly), FC-018 to FC-022, FC-026 to FC-032, FC-035 (values from the specs), FC-036, and FC-037.
- I checked that a test can fail with one mutation. I renamed the `<param>` in the first file of `VerifyMergedExtensionDeclarationsAcrossFilesOfPartialClassShareDocumentation` to `other`. The test then failed with `expected "1" actual "3"` RH8032 diagnostics. I restored the file before committing.

## Commands

```shell
scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~RH802|FullyQualifiedName~RH803"
scripts/test.sh --project analyzer --configuration Debug --filter "TestCategory=SHARED-DOC-013"
scripts/test.sh --project architecture --configuration Debug
```

Results on 96fbfea:

- `FullyQualifiedName~Reihitsu.Analyzer.Test.Documentation`: 675 passed, 0 failed.
- Architecture: 72 passed, 0 failed.
- `scripts/format.sh` was run over the 15 files before the commit.
