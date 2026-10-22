Role: test implementer
Commit: 6776347
Verdict: tests written

Inputs: thoenissen/Reihitsu#1331, `00-lead.md`, `01-spec-author.md`, `02-skeptic.md`, `03-spec-author.md`, `git diff origin/main -- sdd/specs`, the decision entries named in `00-lead.md`. My prompt carried no intended fix or verdict. Expected values come from the spec examples and from the rule requirements they rely on (RH8304-004, RH8307-004, RH8308-004, RH7004-004, RH7309-004), not from running the code first. The only shapes pinned are the ones the handovers list as conforming. Every new test passes on this head.

## Tests

All tests are in `src/Reihitsu.Analyzer.Test`. "Spec example" means the requirement's own example, cut down to its conforming part where the handovers say so.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7409…Tests.VerifyMethodImplementingTwoInterfacesInTheRegionOfTheFirstInBaseListIsNotReported` (new) | SHARED-DIAG-009, spec example, `class D : IB, IA` only | `Verify(source)` | pass |
| `RH7409…Tests.VerifyBaseListInAnotherPartialDeclarationDecidesTheVerdict` (new) | SHARED-DIAG-009, second spec example, both arms (`C.M` not reported, `D.M` reported); FC-024, FC-003 | `Verify(source, diagnostics)` | pass |
| `RH7104…Tests.PartThatOmitsTheAccessibilityIsReportedAndFixedAloneWhenAnotherPartDeclaresIt` (new) | SHARED-DIAG-004 per-part paragraph, spec example with member bodies; FC-024 | `Verify(source, fixed, …)` (one pass, re-analysis, Fix All via base) | pass |
| `RH7103…Tests.OrderRuleReportsAShapeTheFormatterKeeps` (new) | SHARED-DIAG-010 "order rule not covered" example; asserts the formatter leaves the input unchanged, then report and fix | `AssertFormattingChangesNothing` + `Verify(source, fixed, …)` | pass |
| `RH8307…Tests.VerifyHalfWrappedElementThatTheFormatterChangesIsReportedAndFixed` (new) | SHARED-DIAG-010, `///` element of the spec example; asserts the formatter output of the input equals the fix result; FC-013 | formatter + `Verify(source, fixed, …)` | pass |
| `RH5105…Tests.VerifyNoDiagnosticWhenCommentIsInGap` / `VerifyNoDiagnosticWhenDirectiveIsInGap` (existing, tag added) | SHARED-DIAG-010 via its named instance SHARED-JOIN-001; FC-019, FC-005 | `Verify(source)` | pass |
| `RH7309…Tests.MovedRegionBlocksKeepTheirLayout` (new) | SHARED-FIX-007 moved code, spec example; first asserts the formatter would change the expected output, so the test can tell kept layout from formatted layout (FC-033) | formatter + `Verify(source, fixed)` | pass |
| `RH7309…Tests.MovedRegionBlocksKeepTheirLayoutInCarriageReturnLineFeedDocument` (new) | SHARED-FIX-007, same example, CRLF; FC-017 | `NormalizeToCarriageReturnLineFeed` + `Verify` | pass |
| `RH7004…Tests.FixOnlyReindentsTheStatementsItWrapsIntoTheNewBlock` (new) | SHARED-FIX-007 moved statements: a multi-line `if` re-indented line by line, the blank line between the moved statements kept; FC-027 | `Verify(source, fixed)` | pass |
| `RH7103…Tests.MembersAreComparedOnlyWithinTheirOwnTopLevelRegion` (new) | SHARED-ORDER-004 first spec example (RH7103 arm) | `Verify(source)` | pass |
| `RH7110…Tests.MembersAreComparedOnlyWithinTheirOwnTopLevelRegion` (new) | SHARED-ORDER-004 first spec example (RH7110 arm) | `Verify(source)` | pass |
| `RH7103…Tests.StaticMemberInANestedRegionIsComparedWithTheRestOfItsTopLevelRegion` (new) | SHARED-ORDER-004 nested-region side; FC-003 | `Verify(source, diagnostics)` | pass |
| `RH7103…Tests.StaticMemberOutsideEveryRegionIsComparedWithInstanceMembersOutsideEveryRegion` (new) | SHARED-ORDER-004 outside-every-region side, with a region in between; FC-003 | `Verify(source, diagnostics)` | pass |
| `RH7110…Tests.MemberOutsideEveryRegionIsComparedWithMembersOutsideEveryRegion` (new) | same as above, RH7110 | `Verify(source, diagnostics)` | pass |
| RH7103 `NoDiagnosticWhenStaticAndInstanceLiveInSeparateRegions`, `NoDiagnosticWhenInstanceIsOutsideRegionAndStaticIsInsideRegion`, `NoDiagnosticWhenStaticIsOutsideRegionAndInstanceIsInsideRegion`, `StaticMembersAreReportedWhenTheyAppearAfterInstanceMembersInSameRegion` (existing, tag added) | SHARED-ORDER-004, the remaining boundary sides | `Verify` | pass |
| RH7110 `PublicMemberAfterPrivateMemberOfSameKindWithinSameRegionIsReported`, `MemberInANestedRegionIsComparedWithinItsTopLevelRegion`, `MemberInARegionIsNotComparedWithAMemberOutsideEveryRegion` (existing, tag added) | SHARED-ORDER-004 | `Verify` | pass |
| `RH8026…Tests.VerifyDocumentationCommentBehindCodeDocumentsTheFollowingField` (new) | SHARED-DOC-004, RH8026 spec example (`A` reported, `B` not) | `Verify(source, diagnostics)` | pass |
| `RH8304…Tests.VerifyElementsOnSameLineInCommentBehindCodeAreDetectedAndFixed` (new) | SHARED-DOC-004: a `///` comment behind code is judged, and the fix leaves it behind `int A;` | `Verify(source, fixed, …)` | pass |
| `RH8308…Tests.VerifyTextAfterClosingTagInCommentBehindCodeIsDetectedAndFixed` (new) | same, RH8308 | `Verify(source, fixed, …)` | pass |
| `RH8501…Tests.VerifyDiagnosticWhenDocumentationModeIsNone` (new) | SHARED-DOC-008 every-mode clause, RH8501 spec example; the transform records the project's applied `DocumentationMode`, and the test asserts it is `None` (FC-033) | `Verify` + solution transform | pass |
| `RH8501…Tests.VerifyDiagnosticWhenDocumentationModeIsParse` (new) | SHARED-DOC-008, the paired `Parse` run of the same example | same | pass |

Comment-only summary changes, following the precedent `squad/decisions/2026-10-21-1260-contradicting-characterization-tests.md`. The assertions are unchanged. Each summary now says the test pins today's behavior, names the requirement that states the decided behavior, and says the change belongs to an existing follow-up. No tracker number appears in source.

- RH7409 `VerifyMethodImplementingTwoInterfacesIsJudgedByTheFirstInBaseListOrder` → SHARED-DIAG-009
- RH7405 `VerifyOverrideOfOtherMemberKindDoesNotMakeMethodOverrideRegion` and `VerifyOverrideInOtherPartialDeclarationDoesNotMakeOverrideRegion` → SHARED-DIAG-009
- RH7109 `MixedAccessibilityInterfaceViolationWithDirectivesHasNoCodeFix` → SHARED-ORDER-004
- RH8401 `VerifyNoDiagnosticsWhenDocumentationModeIsNone` → SHARED-DOC-008

None of these tests is tagged.

`RequirementCoverageTests` passes, as does the whole architecture project (72/72). SHARED-DIAG-009 and SHARED-DIAG-010 are now covered.

## Scenarios (bug fix)

_N/A._ This run writes specs and tests for existing behavior.

## Not covered, and why

- **Non-conforming shapes**, left out by the lead's instruction and pinned nowhere. Each is tracked by the follow-up named in the handovers:
  - RH7409 `class C : IA, IB` (thoenissen/Reihitsu#1334).
  - The RH8307 `/** */` element (#1347).
  - The RH7004 `s.Flush( );` example and the brace fixes (#1335 and the SHARED-BRACE-002 P26 instance).
  - RH7102 and RH7109 region scope (#1339).
  - RH8301, RH8302 and RH8303 behind-code shapes (#1345), and the RH8305 and RH8307 behind-code fixes (the new P29 instance).
  - RH8401 and RH8402 in mode `None` (#1346).
- **SHARED-FIX-003**: the change is wording only, and its existing tagged tests stay. No new test.
- **SHARED-DIAG-009, third bullet** (a member of another kind): its only shape is the RH7405 one, which does not conform. The requirement is covered by its RH7409 shapes.
- **SHARED-DIAG-004, symbol paragraph**: unchanged in substance, and its RH2102 and RH2004 tests stay. A nested-type variant of the RH7104 per-part shape (the skeptic's "not verified" candidate) is not added. It reaches the same `DeclaredAccessibility` call site as the added test, and this run is not about the nested-default-accessibility branch, which RH7104-004 already covers.
- **SHARED-DOC-008 gate side**: covered by the existing RH8001 and RH8301–RH8303 mode-`None` tests, which conform under the reworded requirement (decision `2026-10-22-1331-p30-reach.md`).
- **CRLF**: run only for the RH7309 moved-code example, where layout is the subject. The RH8304 and RH8308 behind-code fixes insert or remove text on one line. Their CRLF behavior is the subject of SHARED-FIX-008, which a concurrent run owns, so it is not re-tested here.
- **Fix All across several diagnostics**: each base class's `FixAllCorrectsEveryDiagnostic` already runs it. None of the new requirements changes Fix All.
- **Failure classes that cannot reach this run**: no production code changes. FC-006, FC-018, FC-022, FC-023, FC-025, FC-026, FC-028, FC-029, FC-030, FC-036 and FC-011/FC-012 concern rewrites, renames or formatter phases this run does not touch. FC-031 applies only to the reworded summaries, which were updated. FC-035 does not apply: the expected values are spec after-states, and for the RH8304, RH8308 and RH7004 shapes they follow from those rules' fix requirements.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7409InterfaceMethods|FullyQualifiedName~RH7405NonOverride|FullyQualifiedName~RH7104Partial|FullyQualifiedName~RH7103Static|FullyQualifiedName~RH7110Members|FullyQualifiedName~RH7309Regions|FullyQualifiedName~RH7004Using|FullyQualifiedName~RH8307TextAfter|FullyQualifiedName~RH8304Xml|FullyQualifiedName~RH8308NoContent|FullyQualifiedName~RH8026NonPrivate|FullyQualifiedName~RH8501Code|FullyQualifiedName~RH5105Opening|FullyQualifiedName~RH8401Single|FullyQualifiedName~RH7109Readonly"
scripts/test.sh --project analyzer --filter "TestCategory=SHARED-DIAG-009|TestCategory=SHARED-DIAG-010|TestCategory=SHARED-FIX-007|TestCategory=SHARED-ORDER-004|TestCategory=SHARED-DOC-004|TestCategory=SHARED-DOC-008|TestCategory=SHARED-DIAG-004"
scripts/test.sh --project architecture
```

Results on 6776347: class filter 299/299 passed; requirement-category filter 121/121 passed; architecture 72/72 passed. `scripts/format.sh` over the 15 changed test files reported 0 files changed.
