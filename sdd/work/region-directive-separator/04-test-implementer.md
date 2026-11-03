Role: test implementer
Commit: 0f6f380
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md` (maintainer answers 1–7, decisions 1–7), `02-skeptic.md`, `03-spec-author.md`, `sdd/changes/2026-11/region-directive-separator.md`, `git diff f8dc7ef..HEAD -- sdd/specs`. Expected values come from the spec examples and requirement text; none were taken from a patched copy of the code.

All "Fails now" results below were run on `0f6f380`; every failure was checked to be the requirement's own difference (separator, trailing whitespace, `#endregion` text, or region text rewritten by a node-level entry point), not unrelated layout.

## Tests

New file `src/Reihitsu.Formatter.Test/Regression/Regions/RegionLineFormTests.cs` (helper `AssertRuleResult`: LF and CRLF, second pass, every phase alone on the output, tree round-trip):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| SeveralSpacesAndTrailingWhitespaceOfRegionLinesAreNormalized (spec example) | FMT-REGION-007 | AssertRuleResult | fails |
| RegionLineInCanonicalFormIsLeftUnchanged (spec example) | FMT-REGION-007 | AssertRuleResult | passes |
| SeveralSpacesInRegionLineOfDisabledBranchBecomeOneSpace (spec example) | FMT-REGION-007 | AssertRuleResult | fails |
| TrailingWhitespaceOfRegionLineInDisabledBranchIsRemoved | FMT-REGION-007 | AssertRuleResult | fails |
| TrailingWhitespaceBehindDescriptionAfterOneSpaceIsRemoved | FMT-REGION-007 | AssertRuleResult | fails |
| SeveralSpacesBeforeUpperCaseDescriptionBecomeOneSpace | FMT-REGION-007 | AssertRuleResult | fails |
| SeveralSpacesAndTrailingWhitespaceAroundLowerCaseDescriptionAreNormalized | FMT-REGION-001, FMT-REGION-007 | AssertRuleResult | passes (FC-003 other side) |
| SeveralSpacesBeforeDigitLedDescriptionBecomeOneSpace | FMT-REGION-007 | AssertRuleResult | fails |
| SeveralSpacesBeforeUnderscoreLedDescriptionBecomeOneSpace | FMT-REGION-007 | AssertRuleResult | fails |
| WhitespaceInsideDescriptionIsKept | FMT-REGION-007 | AssertRuleResult | fails (on the separator; inner whitespace must stay) |
| TabMixedWithSpacesBeforeDescriptionBecomesOneSpace | FMT-REGION-007 | AssertRuleResult | fails |
| SeveralSpacesInRegionLineInsideMethodBodyBecomeOneSpace | FMT-REGION-007 | AssertRuleResult | fails |
| SeveralSpacesInRegionLinesOfNestedPairBecomeOneSpace | FMT-REGION-007 | AssertRuleResult | fails |
| SeveralSpacesInRegionLineOfStraddlingPairBecomeOneSpace | FMT-REGION-005, FMT-REGION-007 | AssertRuleResult | fails |
| RegionLookingTextInsideStringLiteralIsKept | FMT-REGION-007 | AssertRuleResult | fails (on the directive; literals must stay, FC-022) |
| WhitespaceAroundTextOfEndRegionOfUndescribedRegionIsNormalized (spec example) | FMT-REGION-004 | AssertRuleResult | fails |
| TextDirectlyBehindEndRegionOfUndescribedRegionGetsOneSpace (spec example) | FMT-REGION-004 | AssertRuleResult | fails |
| SeveralSpacesBeforeTextOfEndRegionOfUndescribedRegionBecomeOneSpace | FMT-REGION-004 | AssertRuleResult | fails |
| TrailingWhitespaceBehindTextOfEndRegionOfUndescribedRegionIsRemoved | FMT-REGION-004 | AssertRuleResult | fails |
| TabBeforeTextOfEndRegionOfUndescribedRegionBecomesOneSpace | FMT-REGION-004 | AssertRuleResult | fails |
| BlockCommentBehindEndRegionOfUndescribedRegionIsKeptWithOneSpace | FMT-REGION-004 | AssertRuleResult | fails |
| BareEndRegionOfUndescribedRegionStaysBare | FMT-REGION-004 | AssertRuleResult | passes |
| RegionWithoutDescriptionAndTrailingWhitespaceIsKept | FMT-REGION-004 | AssertRuleResult | passes |
| WhitespaceAroundTextOfEndRegionOfUndescribedRegionInDisabledBranchIsNormalized | FMT-REGION-004 | AssertRuleResult | fails |

New file `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterNodeRegionTests.cs` (LF and CRLF, a second pass through the same entry point):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| FormatNodeInDocumentAsyncKeepsRegionDirectivesOfTheSpecExample (spec example) | FMT-PIPE-028 | FormatNodeInDocumentAsync | fails |
| FormatNodeInDocumentWithContextAsyncKeepsRegionDirectives | FMT-PIPE-028 | FormatNodeInDocumentWithContextAsync | fails |
| FormatNodeKeepsRegionDirectives | FMT-PIPE-028 | FormatNode | fails |
| FormatNodeInDocumentAsyncKeepsSeveralSpacesBeforeUpperCaseDescription | FMT-PIPE-028 | FormatNodeInDocumentAsync | fails (`#endregion    Bar` rewritten) |
| FormatNodeInDocumentAsyncKeepsWhitespaceBehindRegionDescription | FMT-PIPE-028 | FormatNodeInDocumentAsync | passes (guards FMT-REGION-007 from reaching node level) |
| FormatNodeInDocumentAsyncKeepsWhitespaceAroundTextOfEndRegionOfUndescribedRegion | FMT-PIPE-028 | FormatNodeInDocumentAsync | passes (guards FMT-REGION-004) |
| FormatNodeInDocumentAsyncKeepsRegionInsideFormattedMethod | FMT-PIPE-028 | FormatNodeInDocumentAsync | fails |
| FormatNodeInDocumentAsyncKeepsBothHalvesOfRegionOpenedInFrontOfTopLevelType | FMT-PIPE-028 (FC-028, skeptic follow-up 1) | FormatNodeInDocumentAsync | fails (writes `#endregion // Bar`, keeps `#region    bar`) |
| FormatNodeInDocumentAsyncKeepsRegionWhoseEndRegionLiesOutsideTheTarget | FMT-PIPE-028 | FormatNodeInDocumentAsync | passes |
| FormatDocumentAsyncAppliesRegionRequirements | FMT-PIPE-028 | FormatDocumentAsync | passes (other side of the entry-point condition) |
| FormatSyntaxTreeAppliesRegionRequirements | FMT-PIPE-028 | FormatSyntaxTree | fails (separator and trailing whitespace) |

Code-fix tests (analyzer project, `Verify` on `BatchCodeFixTestsBase`, one pass plus re-analysis):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7105…Tests.FixOfTypeWithRegionPairKeepsTheRegionDirectives (change-doc example) | FMT-PIPE-028, SHARED-FIX-006 | Verify | fails (`#region Bar`, `#endregion // Bar`) |
| RH7105…Tests.FixOfTypeWithRegionPairKeepsTheSeparatorOfAnUpperCaseDescription | FMT-PIPE-028, SHARED-FIX-006 | Verify | passes (guards the separator at node level) |
| RH7104…Tests.FixOfPartialTypeWithRegionPairKeepsTheRegionDirectives | FMT-PIPE-028, SHARED-FIX-006 | Verify | fails (`#endregion // Bar`) |
| RH7104…Tests.FixOfTopLevelPartialTypeKeepsRegionOpenedInFrontOfTheType | FMT-PIPE-028, SHARED-FIX-006 (FC-028, FC-030) | Verify | fails (half of the pair rewritten) |

### Existing tests whose expectation changed

| Test | Change | Demanded by |
|---|---|---|
| TabReplacementTests.TabBetweenRegionKeywordAndDescriptionKeepsFourSpacesUnlessTheRegionLineIsRewritten → renamed TabBetweenRegionKeywordAndDescriptionBecomesOneSpace | `#region    Bar` → `#region Bar`; tag FMT-REGION-007 added | FMT-TAB-002's changed example, FMT-REGION-007 |
| TabReplacementTests.TabInsideRegionDescriptionIsKeptInBothDirectives | `#region    Baz\tqux` → `#region Baz\tqux`; tag FMT-REGION-007 added | FMT-TAB-002 prose, FMT-REGION-007 |
| ReihitsuFormatterTests.FormatNodePreservesRegionsInsideDetachedNode | `#region Inner` / `#endregion // Inner` → `#region inner` / `#endregion`; tag FMT-PIPE-028 added | FMT-PIPE-028 |
| ReihitsuFormatterTests.FormatNodeFormatsDetachedNodeHoldingAForeignLineBreak | same region text change (regions incidental; no tag) | FMT-PIPE-028 |
| ReihitsuFormatterTests.FormatNodeInDocumentAsyncPreservesRegionsInsideTargetRoot | same; tag FMT-PIPE-028 added | FMT-PIPE-028 |
| ReihitsuFormatterNodeBlankLineTests.KeepsBlankLineAbovePropertyWhenRegionInsideIsRewritten → renamed KeepsBlankLineAbovePropertyHoldingARegion | `#region Assign` / `#endregion // Assign` → `#region assign` / `#endregion` | FMT-PIPE-028 |
| RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.CodeFixReordersStaticWithinRegionAndIgnoresInstanceMembersInOtherRegions | fixed code keeps bare `#endregion` for `Decoy` and `Factories` (was `#endregion // Decoy`, `// Factories`) | FMT-PIPE-028, SHARED-FIX-006; matches the RH7103 spec example, which shows bare `#endregion` |

How the affected existing tests were found: in a throwaway worktree outside the branch (deleted), the region phase was disabled for contexts built by the node-level entry points, and the formatter (4368 tests) and analyzer (6411 tests) projects were run in full. Exactly the five formatter-side and one analyzer test above failed; their new expected values were then written from FMT-PIPE-028 (directive text as in the input), not copied from that run. Tests that run a phase alone keep `#region    …` and were left unchanged: `TabReplacementPhaseTests.TabBetweenRegionKeywordAndDescriptionIsReplacedAndTabsInsideFreeTextAreKept`, `FormattingPipelineTests.PreparationPhaseExecutedDirectlyReplacesTabsAndRewritesLineBreaks`. Analyzer fixtures with wide separators (RH7301, RH7302, RH7311, RH7401) belong to fixes that do not call the formatter and stay as they are.

`scripts/test.sh --project architecture`: 92 passed — `EveryRequirementOfAnEnforcedSpecIsCovered` now finds tests for FMT-PIPE-028 and FMT-REGION-007.

## Not covered, and why

- FC-007 / FC-008 (convergence, Fix All): no code-fix provider changes. `Verify` re-analyzes after one pass, and each provider's existing `GetFixAllScenario` holds no region, so the change cannot reach it.
- FC-009, FC-018, FC-036: no action registration and no rename are involved.
- FC-019 / FC-021 (comment in the gap): a `#region` line's gap holds no trivia besides whitespace. Text there is the directive's message (the description). The `#endregion` side is covered with a line comment, a block comment, and text written directly behind the keyword.
- FC-020 (directive in the gap): a directive line cannot hold another directive. `#if false` is covered for both requirements.
- FC-006, FC-023–FC-027, FC-029: no syntax rewrite. The change touches only the text of region directives.
- FC-012 (neighbor phases): the tab phase → region phase interaction is covered by the tab tests. The cleanup phase is checked by `AssertRuleResult`'s phase-alone check on every output.
- FC-015 / FC-016 (shared helper): `RegionDirectiveUtilities.GetRegionDescription` (Core) is shared with RH7301/7302/7308/7309/7311. The analyzer project's full run is the guard, and no new test is needed.
- Unpaired region directives (FMT-REGION-007's pairing scope, FMT-PIPE-005): a document with one is a syntax error and is returned unchanged. That is pinned by the existing FMT-PIPE-005 entry-point tests, and in a document no paired directive goes without a partner, so no test was added.

## Findings

1. **Spec, minor.** FMT-REGION-006 lists only FMT-REGION-001 and FMT-REGION-002 as applying in disabled branches. FMT-REGION-007 states its own disabled-branch reach, but FMT-REGION-004's new normalization is unqualified, so it applies there only by implication. `WhitespaceAroundTextOfEndRegionOfUndescribedRegionInDisabledBranchIsNormalized` asserts the normalization inside `#if false`, reading FMT-REGION-004 as written. The spec author may want to state this explicitly. No output beyond what FMT-REGION-004 already says is asserted.
2. **Handover inaccuracy.** `03-spec-author.md` says the member-ordering fixes (RH7102, RH7103, RH7109) withhold themselves for a type with a region. RH7103 does not: `CodeFixReordersStaticWithinRegionAndIgnoresInstanceMembersInOtherRegions` applies the fix inside a type with two regions, and its expected output changed under FMT-PIPE-028 (row above).
3. **Characterization note.** On `0f6f380`, `RegionWithoutDescriptionAndTrailingWhitespaceIsKept` passes: `#region···` keeps its trailing whitespace, as FMT-REGION-004 ("MUST NOT change a `#region` without a description") demands. Maintainer answer 7's general statement ("trailing whitespace is always removed") is recorded as a follow-up for other directives. This undescribed `#region` line falls under FMT-REGION-004, not under that follow-up. The lead may want to confirm this reading if the follow-up is filed.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~RegionLineFormTests|FullyQualifiedName~ReihitsuFormatterNodeRegionTests|FullyQualifiedName~TabReplacementTests|FullyQualifiedName~ReihitsuFormatterTests.FormatNode|FullyQualifiedName~KeepsBlankLineAbovePropertyHoldingARegion"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests|FullyQualifiedName~RH7104PartialElementsMustDeclareAccessModifierAnalyzerTests|FullyQualifiedName~RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests"
scripts/test.sh --project architecture
```
