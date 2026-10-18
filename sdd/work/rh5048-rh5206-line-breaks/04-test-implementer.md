Role: test implementer
Commit: 35809ea (tests written on top of it; analyzer, code-fix, and spec sources unchanged)
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Lane: specs and tests for existing behavior. Every new test passes on the unchanged code, and so does every tagged existing test. Expected values come from the spec examples and requirements of `03-spec-author.md`. In one case the spec of a neighboring phase gives the value (RH5112, below).

## Result

- `scripts/test.sh --project analyzer`: 4795 passed, 0 failed.
- `scripts/test.sh --project architecture`: 72 passed, 0 failed. This includes `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered`.
- Before this run, that coverage test failed on 87 IDs: the 74 rule IDs, SHARED-CHAIN-001 to 009, SHARED-JOIN-001/002, SHARED-USING-010, and SHARED-WS-003. All 87 are now covered.
- `scripts/format.sh` over the 22 changed files formatted 0 of them. No build warnings in the changed files.
- 113 new tests. 749 `[Requirement]` attributes were added to new and existing tests.
- Only test files under `src/Reihitsu.Analyzer.Test/Formatting/` changed.
- In `RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs`, the private helper `FormatWithLineFeed` moved unchanged to the end of the `Tests` region, so the new public tests come before it (RH7110).

## Tests

New tests, by file (all in `src/Reihitsu.Analyzer.Test/Formatting/`). Existing tests of each file were tagged with the requirements they verify; the shared IDs they carry are listed in the last column.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5048…: `VerifyCommentBeforeWrappedConditionalFirstLinkIsReported`, `VerifyCommentBeforeWrappedNullForgivingFirstLinkIsReported` | RH5048-001, SHARED-CHAIN-001 | `Verify(source, diag)` | pass |
| RH5048…: `VerifyEveryCommentBeforeWrappedFirstLinkIsReportedSeparately` | RH5048-001, SHARED-COMMENT-003 | `Verify` | pass |
| RH5048…: `VerifyCommentInsideAttachedArgumentListOfRootIsNotReported`, `VerifyCommentBeforeAttachedElementAccessOfRootIsNotReported` | RH5048-001, SHARED-CHAIN-001 (skeptic A2) | `Verify` | pass |
| RH5048…: `VerifyInlineCommentBeforeFirstLinkOnRootLineIsNotReported` | RH5048-001 | `Verify` | pass |
| RH5101: implicit object creation, constructor initializer, primary-constructor base type reported on the first argument | RH5101-001, -002 | `Verify` | pass |
| RH5101: `VerifyFirstArgumentOnNewLineIsFixedWithCrLf` | RH5101-003, FC-010 | `Verify(src, fixed)` CRLF | pass |
| RH5101: comment on own line, `/* */` ending before the first argument, `#if false` disabled text in the gap → not reported | SHARED-JOIN-001, FC-019/020/021 | `Verify` | pass |
| RH5101: comment inside the list, directive inside the list → reported, no fix; comment behind the token before `(` → fix offered | RH5101-004, FC-002/003 (skeptic A1) | `Verify` + `GetCodeFixActionsAsync` | pass |
| RH5102: lambda block argument sharing a line reported on the whole list; own-line lambda not reported | RH5102-001, -002 | `Verify` | pass |
| RH5102: CRLF fix; directive inside the list withholds the fix; comment behind the token before `(` offers the fix | RH5102-003, -004, FC-010, FC-002 | `Verify`, `GetCodeFixActionsAsync` | pass |
| RH5103: behind an empty statement (reported, no fix); behind a leading `;` (not reported); first statement on the `case` line (not reported); behind `/* */` (reported, no fix); statement behind the `case`-line statement fixed to the section indentation | RH5103-001, -003, -004 | `Verify`, `GetCodeFixActionsAsync` | pass |
| RH5104: field trailing comment fixed; leading `/* lead */ M();` fixed | RH5104-001, -003, -004 | `Verify(src, fixed)` | pass |
| RH5104: comment behind `#if` not reported | RH5104-002, SHARED-COMMENT-002 | `Verify` | pass |
| RH5104: no blank line below a `//` line, below a switch label, below a directive | RH5104-005 | `Verify(src, fixed)` | pass |
| RH5105: comment behind the name; disabled text in the gap → not reported | RH5105-001, SHARED-JOIN-001 | `Verify` | pass |
| RH5105: fix keeps the rest of the list's layout; CRLF fix | RH5105-003, SHARED-WS-003, FC-010 | `Verify(src, fixed)` | pass |
| RH5106: empty list `(` / `)` reported and fixed; `/* */ )` and disabled text in the gap not reported; CRLF fix | RH5106-001…003, SHARED-JOIN-001, SHARED-WS-003 | `Verify` | pass |
| RH5107: two commas in one list each reported; anonymous method; primary constructor; leading comma of an argument list not reported; comment behind the previous parameter; disabled text | RH5107-001, -002, SHARED-JOIN-001 | `Verify` | pass |
| RH5108: `/* */` before the first parameter; disabled text → not reported; CRLF fix | RH5108-001, -003, SHARED-JOIN-001, SHARED-WS-003 | `Verify` | pass |
| RH5109: primary constructor; parenthesized lambda; single-line list not reported; one diagnostic for two shared lines, fixed; CRLF fix; directive withholds the fix; comment behind the token before `(` offers it | RH5109-001…004 | `Verify`, `GetCodeFixActionsAsync` | pass |
| RH5110: `where` in the wrong column fixed; `where` at the declaration's column; second clause sharing a line; one diagnostic per declaration, fixed; CRLF fix | RH5110-001…003 | `Verify` | pass |
| RH5111: `=` below the target in an object initializer, fixed; array initializer and collection-initializer member below `=` reported; `=` and value both on own lines reported once and fixed | RH5111-001…004 | `Verify` | pass |
| RH5111: multi-line property with multi-line raw-string value | RH5111-004, FC-029 (fourth combination), FC-028 | `AssertCodeFixMatchesFormatterAsync` | pass |
| RH5111: comment in a property, comment in an assignment, directive in a declarator → no fix; comment in front of an enum member / a parameter → fixed, comment kept; CRLF fix | RH5111-005, -002, -004 (skeptic A5) | `Verify`, `GetCodeFixActionsAsync` | pass |
| RH5112: `?.` reported on `?`; `!?.` reported on `!`; comment behind the root not reported; comment inside the root's argument list does not block, fixed; first link behind `?[0]` reported; long one-line chain not reported; CRLF fix; fix leaves `+ d+1` behind the chain as written | RH5112-001…003, SHARED-CHAIN-001…004, -008, -009 | `Verify` | pass |
| RH5113: `/* */ ;` and disabled text in the gap → not reported; CRLF fix | RH5113-001, -003, SHARED-JOIN-001 | `Verify` | pass |
| RH5201: prefix links and call-less chain links not reported; link behind a commented root aligned to the root; block comment starting the link line; wrapped first link not reported for its column; split `?` of a call-less chain reported; comment-held break not reported; comment-held break in a later link reported; link misplaced two ways reported once; two misplaced links fixed under CRLF | RH5201-001…005, SHARED-CHAIN-004…008 | `Verify` | pass |
| RH5202: relative indentation of content kept; CRLF fix | RH5202-001, -003 | `Verify` | pass |
| RH5203: both arguments below `(` reported; fix changes only the indentation (`3+4` stays); CRLF fix | RH5203-001…003 | `Verify` | pass |
| RH5204: `case` label, enum member, labeled statement, `else` and closing brace reported; comment before `}` expected one level deeper; tab-indented line and lambda block body not reported; comment fixed under CRLF; field fixed | RH5204-001, -002, -004…006 | `Verify` | pass |
| RH5205: brace on the arrow line not reported; CRLF fix | RH5205-001…003 | `Verify` | pass |
| RH5206: one-line switch expression reported; misindented arm reported once; closing brace off the anchor reported; CRLF fix | RH5206-001…003 | `Verify` | pass |

Shared IDs on existing tests:

- SHARED-CHAIN-004 to 009: RH5112 and RH5201 directive, disabled-text, misspaced-declaration, conditional-expression, and inner-chain tests.
- SHARED-USING-010: the six RH5114 indentation tests.
- SHARED-JOIN-001: the comment and directive gap tests of RH5101 and RH5105–RH5108, RH5113.
- SHARED-JOIN-002: the `_broken = ;` tests of RH5105 and RH5106 (CS1525).
- SHARED-WS-003: the basic fix tests of RH5105, RH5106, and RH5108.

## Findings

1. **SHARED-FIX-002 against existing Fix All scenarios (pre-existing, also on `main`).**
   - The `GetFixAllScenario` of RH5104, RH5110, and RH5204 sets `NumberOfFixAllIterations = 2`. `FixAllCorrectsEveryDiagnostic` carries `SHARED-FIX-002` ("in one Fix All application").
   - Observed: for RH5204, a misindented field plus a misindented comment inside a method of the same class needs 2 Fix All iterations. The verifier reports "Expected '1' iterations but found '2' iterations."
   - I did not keep a failing test. The new CRLF test uses one diagnostic, and the field case is its own test (`VerifyFieldIndentationIsFixed`).
   - The same configuration appears in many test files outside this batch, for example RH3001, RH7102, and RH5518. This matches skeptic candidate FC-008 for RH5204.
2. **RH5103-003 versus `VerifySplitStatementOverIndentsWhenLabelSpansMultipleLinesAndStatementSharesItsContinuationLine` (existing, not tagged).**
   - Input: `case 1 when` / `Check(value): Method(1); Method(2);`.
   - The test pins `Method(2);` one level deeper than the section's statements. The test calls this a documented limitation.
   - RH5103-003, read literally ("shares the line of a `case` label" → the section's statement indentation), would expect the section's column. The spec should exclude multi-line labels, or the case needs an open question.
3. **RH5112 fix and comment spacing (no finding, recorded for the spec checker).**
   - The fix turns `Get(/* value */ 1)` into `Get(/* value */1)`. `scripts/format.sh` on the same file gives the same output.
   - FMT-SPACE-013 specifies that result (`M( /* four */a)`), and SHARED-CHAIN-008 requires the formatted layout. `VerifyCommentInsideRootArgumentListDoesNotKeepFirstLinkInPlace` asserts it.

## Not covered, and why

- **Inputs of open questions, deliberately left untagged.** Their existing tests were left untagged:
  - RH5101/RH5102 raw-string arguments (D2, #1112).
  - RH5104 literal and comment lines (D1, #1117 Q1); `DocumentationMode.None` (#1117 Q2/Q3, FC-033: default mode only); two comments on one line (Q4); a comment behind a multi-line interpolated string (Q5); a `/* */` line above (Q6).
  - RH5103 top-level statements, a block on one line, and an embedded or labeled predecessor (#1116 Q2–Q4). Untagged existing test: `VerifySplitStatementDoesNotCompensateWhenSwitchLabelIsNotFirstOnItsLine`.
  - RH5108 primary constructor, lambda, anonymous method, extension (#1118).
  - RH5110 record struct and `/* c */ where` (#1119).
  - RH5111 enum `=` below its target, an assignment used as a value, doc comments (#1120), and D8.
  - RH5113 doc comment (#1121).
  - RH5114 `/** */` before the moved directive (#1122). Untagged existing test: `FixIsNotOfferedWhenDocumentationCommentLeadsTheMovedDirective`.
  - RH5203 comment in front of the argument (#1123).
  - RH5204 type-, identifier-, or attribute-led lines and the `///` diagnostic location (#1124). The documentation-comment tests of RH5204 are untagged, because their markup pins the location after `///` (Q2).
  - RH5206 block comment before an arm (#1125).
  - Fix scope beyond the diagnostic (#1113), for example RH5204 `else`.
- **Existing tests outside the specs, untagged:**
  - RH5103 `FixIsNotOfferedAcrossDirectiveAndSkippedSyntax`: `#if` mid-line is not a directive.
  - RH5114 `FixIsNotOfferedWhenSyntaxErrorSitsBetweenTheDirectives` and `NoDiagnosticForExternAliasDirectives`.
  - RH5101 `VerifyCommentedArgumentListIsNotOfferedACodeFix`: its input is not reported.
- **RH5101 `WithElementSyntax`** (`[with(` / `10), 1]`, skeptic): RH5101-001 does not list it, and the test project references `Net90`. Left untested on purpose.
- **RH5103 directive or disabled text in the gap:** not constructible. A directive ends its line, so the reported statement cannot start on its predecessor's line behind one. The comment and the empty statement are covered.
- **FC-008 Fix All with overlapping scopes:**
  - RH5101 and RH5102 are covered by their existing nested-list scenarios.
  - RH5204 is finding 1.
  - RH5110 (class fix containing a method's diagnostic) and RH5112/RH5201 (chain inside an argument of a chain) got no new Fix All test. Their existing scenarios already need or permit more than one iteration, or cover a single diagnostic (`VerifyFixAlignsInnerChainInsideWrappedOuterChain`). Adding one would pin finding 1.
- **FC-026 (three on a line):** existing `ThreeStatementChainFixAllKeepsBlockIndentation` (RH5103) and `ChainOfDirectivesOnOneLineReportsEveryFollowingDirective` (RH5114).
- **Not applicable:**
  - Formatter idempotency, combined pipeline, and `SelfHostingTests`: no formatter change.
  - FC-018 and FC-036: no rename fix.
  - FC-024 and FC-025: no rule here depends on `partial` or expression-bodied kinds, beyond the RH5113 expression-bodied property, which is covered.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5048|FullyQualifiedName~RH5101|FullyQualifiedName~RH5102|FullyQualifiedName~RH5103|FullyQualifiedName~RH5104"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5105|FullyQualifiedName~RH5106|FullyQualifiedName~RH5107|FullyQualifiedName~RH5108|FullyQualifiedName~RH5109|FullyQualifiedName~RH5110"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5111|FullyQualifiedName~RH5112|FullyQualifiedName~RH5113|FullyQualifiedName~RH5114"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5201|FullyQualifiedName~RH5202|FullyQualifiedName~RH5203|FullyQualifiedName~RH5204|FullyQualifiedName~RH5205|FullyQualifiedName~RH5206"
scripts/test.sh --project analyzer --filter "TestCategory=SHARED-CHAIN-009"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests"
```
