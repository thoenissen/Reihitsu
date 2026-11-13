Role: test implementer
Commit: 212fe27
Verdict: tests written

Repair round 1 (decision 25, spec checker 09 findings 1 and 2). Tags only; no assertion, input, or expected output changed. Prompt carried no intended fix or verdict.

- thoenissen/Reihitsu#1140: nothing to do.
- thoenissen/Reihitsu#1144: nothing to do.
- thoenissen/Reihitsu#1145: nothing to do.
- thoenissen/Reihitsu#1393: nothing to do.
- thoenissen/Reihitsu#1511: nothing to do.
- thoenissen/Reihitsu#1561: nothing to do.
- thoenissen/Reihitsu#1141: finding 1, see *Whole run*.
- thoenissen/Reihitsu#1094: finding 2, see *Whole run*.

## Whole run

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH6023…Tests.VerifyFixKeepsTheWrappedLinkOfAnAssignedMemberBehindAMemberBinding` | RH6023-002, SHARED-OPSPACE-004 (was RH6023-002, FMT-PIPE-024) | `Verify`, both | pass |
| `RH6023…Tests.VerifyFixKeepsTheWrappedLinkOfAnAssignedMemberBehindAnElementBinding` | RH6023-002, SHARED-OPSPACE-004 (was RH6023-002, FMT-PIPE-024) | `Verify`, both | pass |
| `RH6023…Tests.VerifyFixKeepsTheWrappedLinksOfALongerAssignedChain` | RH6023-002, SHARED-OPSPACE-004 (was RH6023-002, FMT-PIPE-024) | `Verify`, both | pass |
| `RH6023…Tests.VerifyFixKeepsAnAssignedMemberOnOneLineBehindAMemberBinding` | RH6023-002, SHARED-OPSPACE-004 (was RH6023-002, FMT-PIPE-024) | `Verify`, both | pass |
| `RH6023…Tests.VerifyFixOnAnAssignedMemberBindingKeepsItOnItsLine` | RH6023-002, SHARED-OPSPACE-004 (was RH6023-002, FMT-PIPE-024) | `Verify`, both | pass |
| `RH6023…Tests.VerifyFixKeepsTheWrappedLinkOfAnAssignedMemberWhoseValueIsWrapped` | RH6023-002, SHARED-OPSPACE-004 (was also FMT-PIPE-024) | `Verify`, both | pass |
| `RH5513…Tests.FixInTabIndentedCodeIndentsTheDeclarationWithSpacesToTheAttributeListsColumn` | RH5513-004, SHARED-ATTR-008 (was untagged) | `ApplyCodeFixAsync`, both | pass |

Derivations:
- **FMT-PIPE-024 removed.** It states the facts a node-level entry point passes to the phases; the RH6023 fix no longer calls one (`OperatorSpacingCodeFixProviderBase` decides the gaps with `FormattingTextAnalysisUtilities.IsOperatorGapViolation`), so none of the six tests exercises it.
- **SHARED-OPSPACE-004 added to five** (the spec checker suggested only removing the tag). Each test asserts one space in each operator gap without a line break and everything else unchanged: the wrapped chain links keep their line and column, the member stays on its line. That is SHARED-OPSPACE-004's normative sentence ("exactly one space in each gap ... MUST NOT change anything else ... the operands keep their layout"), which RH6023-002 delegates to. The sibling tests of the same shape already carry it (`…OfACompoundAssignedMemberBehindAMemberBinding`, `…InANestedConditionalAccess`), so the class is now consistent.
- **RH5513 tab test: RH5513-004 and SHARED-ATTR-008, not SHARED-FIX-007.** The expected output (only the gap behind `]` replaced, the tabs in front of the list kept, `void L()` at twelve spaces for the namespace, class, and method-block braces) is exactly SHARED-ATTR-008's sentence: indent with spaces, four for each enclosing brace, independent of the list's column and of other lines. RH5513-004 delegates to it. SHARED-FIX-007 is not tagged: its tab bullet, read with the list as the changed code's first token, gives column 3, the reading the skeptic (01, Q4) flagged and #1030 Q4 rejected; tagging it would assert a contested reading. The summary's "Characterizes" became "Verifies" and its column wording now names the depth rule; the assertion is unchanged.

## Not covered, and why
- Observation for the lead, not a change: SHARED-ATTR-021 says the brace's line must be "indented four spaces for each such brace"; it does not say how a tab counts. The fix (`TargetAttributePlacementCodeFixProviderBase`, comment "a tab counted as four spaces") counts a tab as four, so `\t\t{` passes and the fix is offered in the tab test. Counted as one column (FMT-PIPE-017) the brace would be off its depth column and the fix withheld. The RH5513 test therefore relies on the code's reading; I did not tag SHARED-ATTR-021 on it for that reason.

## Commands
scripts/format.sh src/Reihitsu.Analyzer.Test/Formatting/RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzerTests.cs src/Reihitsu.Analyzer.Test/Formatting/RH5513MethodAttributesMustFollowPlacementRulesAnalyzerTests.cs  (0 of 2 changed)
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzerTests|FullyQualifiedName~RH5513MethodAttributesMustFollowPlacementRulesAnalyzerTests"  (52 passed)
scripts/test.sh --project architecture  (117 passed)

## Amendment

After d50e9f8 (11-spec-author, lead decision 26), SHARED-ATTR-021 says a tab in the brace's line counts as four spaces. Its new example, a method in a class in a namespace with tab indentation, is the input of `RH5513…Tests.FixInTabIndentedCodeIndentsTheDeclarationWithSpacesToTheAttributeListsColumn`. The brace line `→→{` counts as eight columns, its depth column, so the fix is offered. The test now carries RH5513-004, SHARED-ATTR-008 and SHARED-ATTR-021. I changed no assertion, which closes the observation under *Not covered*.

Commands: `scripts/format.sh` on the RH5513 test file (0 of 1 changed); `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5513MethodAttributesMustFollowPlacementRulesAnalyzerTests"` (25 passed); `scripts/test.sh --project architecture` (117 passed).
