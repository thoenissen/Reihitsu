Role: test implementer
Commit: 1e6b365
Verdict: tests written

Inputs:
- `00-lead.md` through `04-skeptic.md`.
- `squad/decisions/2026-10-21-1139-shared-comment-001-contradicting-tests.md` and `squad/decisions/2026-10-22-1139-p12-scope-rh53xx.md`.
- `git diff origin/main -- sdd/specs`.

My prompt carried no intended fix or verdict. Lane: specs and tests for existing behavior. No production file is touched. Every test below passes on the unchanged code.

## Removed and trimmed (decisions entry 2026-10-21, option a)

- `RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzerTests`
  - Removed: `VerifyNoDiagnosticForDocumentationCommentAfterAttribute`, `…WithoutDocumentationParsing`, and `…WithDocumentationParseMode`.
  - Trimmed: `VerifyEmptyBlockCommentAfterAttributeIsReportedButBlockDocumentationCommentIsNot` and its `…WithoutDocumentationParsing` variant. I dropped the `/** <summary>Method</summary> */` half and kept the `/**/` half unchanged. Because the old names describe the dropped half, the tests are renamed `VerifyEmptyBlockCommentAfterAttributeIsReported` and `VerifyEmptyBlockCommentAfterAttributeWithoutDocumentationParsingIsReported`. Their tags (SHARED-COMMENT-001, RH5033-001) are unchanged.
- `RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzerTests`
  - Removed: `VerifyNoDiagnosticForDocumentationCommentInArgumentList` and `…WithoutDocumentationParsing`.

## Tests

All tests are in `src/Reihitsu.Analyzer.Test/Formatting/`. Helper: `Verify` on the rule's test base (`AnalyzerTestsBase<T>`, or `BatchCodeFixTestsBase<,>` for RH520x/RH530x).

| Test | Requirement / failure class | Expected now |
|---|---|---|
| RH5033 `VerifyQuadrupleSlashCommentAfterAttributeIsReported` (new) | SHARED-COMMENT-001, RH5033-001; FC-033: the documentation-parsed side of the existing `DocumentationMode.None` test | pass: `//// note` reported |
| RH5034 `VerifyNoDiagnosticForBlockDocumentationCommentAfterDocumentationComment` (new) | SHARED-COMMENT-001, RH5034-001 (spec example) | pass: not reported |
| RH5034 `…WithoutDocumentationParsing` (new) | the same; FC-033 doc mode | pass |
| RH5204 `VerifyNoDiagnosticForMemberBehindBlockCommentStartingTheLine` (new) | SHARED-COMMENT-011, RH5204-001 (spec example, `/* a */   private int _a;`) | pass |
| RH5204 `…WithoutDocumentationParsing`, `…WithCrLf` (new) | the same; doc mode, CRLF | pass |
| RH5204 `VerifyNoDiagnosticForMemberBehindDelimitedDocumentationCommentStartingTheLine` (new) | SHARED-COMMENT-011 "of any kind", RH5204-001 (spec example); FC-002 | pass |
| RH5204 `…WithoutDocumentationParsing` (new) | the same; doc mode | pass |
| RH5204 `VerifyMemberBelowBlockCommentLineIsDetectedAndFixed` (new) | SHARED-COMMENT-011 "on the comment's first line", RH5204-001, RH5204-006; FC-003, other side | pass: `private` reported, fix re-indents it |
| RH5201 `VerifyNoDiagnosticForLinkBehindBlockCommentStartingTheLineInTheAnchorColumn` (new) | SHARED-COMMENT-011, RH5201-001, RH5201-002 (spec example, `/* all */   .ToList()`) | pass |
| RH5201 `…WithCrLf` (new) | the same; CRLF | pass |
| RH5201 `VerifyBlockCommentStartingTheLinkLineIsAlignedWithTheLink` (existing, tag added) | SHARED-COMMENT-011 | pass |
| RH5203 `VerifyArgumentBelowTrailingCommentIsDetectedAndFixed` (new) | SHARED-COMMENT-011 counterpart "behind code on the line above", RH5203-001, RH5203-003; FC-003 | pass: reported and aligned, comment kept |
| RH5203 `…WithCrLf` (new) | the same; CRLF | pass |
| RH5203 `VerifyArgumentBelowCommentLineIsDetected` (new, diagnostic only) | SHARED-COMMENT-011 counterpart "on a line of its own above", RH5203-001; FC-003 | pass: reported. The fix is not asserted; see Findings |
| RH5301 `VerifyNoDiagnosticForCommentPrefixedOpeningBrace`, `…ClosingBrace`, `…Assignment` (existing, tag added) | SHARED-COMMENT-011 | pass |
| RH5301 `VerifyNoDiagnosticForDelimitedDocumentationCommentPrefixedAssignment` (new) | SHARED-COMMENT-011 "of any kind", RH5301-002; FC-002 | pass |
| RH5301 `…WithoutDocumentationParsing` (new) | the same; doc mode | pass |
| RH5301 `VerifyNoDiagnosticForCommentPrefixedAssignmentWithCrLf` (new) | SHARED-COMMENT-011, RH5301-002; CRLF | pass |
| RH5303 `VerifyNoDiagnosticForCommentPrefixedCollectionOpeningBrace`, `…ClosingBrace`, `VerifyNoDiagnosticForCommentPrefixedComplexElementTokens` (existing, tag added) | SHARED-COMMENT-011 | pass |
| RH5303 `VerifyNoDiagnosticForCommentPrefixedCollectionElement` (new) | SHARED-COMMENT-011, RH5303-002 | pass |

Evidence that the not-reported tests depend on the comment: I temporarily replaced the leading comment with spaces in all 10 new not-reported SHARED-COMMENT-011 tests, so the token stayed in its off column. All 10 then failed on a diagnostic-count mismatch. I restored the files afterwards. With that, RequirementCoverage no longer reports SHARED-COMMENT-011 as uncovered, and the architecture project passes 72 of 72.

## Findings

**RH5203 fix moves a comment on its own line to column 0. This is pre-existing and contradicts RH5203-003 on `main`.**
- Input, LF, analyzer code-fix path:
  ```
          Call(1,
               // two
                  2);
  ```
  `(` is in column 12, the comment in column 13, the argument in column 16.
- Observed after the fix: `// two` in column 0, and `2` in column 13.
- Expected per RH5203-003 ("MUST NOT change anything else"): `// two` stays in column 13, and `2` moves to column 13.
- The fix relocates a user comment, which is FC-004 and SHARED-CODEFIX-003 territory.
- Following clarification 5, I pinned only the conforming diagnostic, so the test does not assert the fix.
- I found no open issue for it. I did not probe CRLF or a `/* */` comment in this shape.
- It is related to the skeptic's untracked RH5203 tail-led fix candidate (`02-skeptic.md`), but it is a different shape: a whole-line comment, not a multi-line comment tail. For the lead to file.

## Not covered, and why

- **SHARED-COMMENT-001, a `///` or `/** */` that documents nothing is reported (P13).** No conforming shape exists. Per the 2026-10-21 decision, it is tracked in thoenissen/Reihitsu#1362 Q1.
- **SHARED-COMMENT-001, a `/* */`-only line counts like a `//` line.** No rule that lists SHARED-COMMENT-001 distinguishes comment lines from code lines (`02-skeptic.md` F5). RH5104 is thoenissen/Reihitsu#1146.
- **SHARED-COMMENT-011 on RH5110, RH5203 (comment-led), RH5205, RH5206.** Non-conforming, tracked by thoenissen/Reihitsu#1147.
- **SHARED-COMMENT-011 on RH5302 and RH5308.** Non-conforming, tracked by thoenissen/Reihitsu#1362 Q5.
- **SHARED-COMMENT-011 on RH5305 (N1) and on RH5301/RH5303 with two leading comments (N2).** Non-conforming, not pinned.
- **SHARED-COMMENT-011 on RH5112 and RH5306.** Not tagged, because SHARED-CHAIN-004 and RH5306-001 decide those shapes more broadly (FC-033).
- **Two tests with comment shapes left untagged.**
  - RH5301 `VerifyCommentsInFrontOfTokensAnchorTheirLines` pins a tail-led line, which is #1362 Q4.
  - `VerifyCommentsOutsideTheirColumnAreReported` pins an off-column comment, which is #1362 Q2.
- **A line led by the tail of a multi-line comment, and a leading comment outside the rule's column.** Undecided (#1362 Q4, Q2), so not pinned.
- **FC-019/FC-020 (directive in the gap).** A directive cannot share a line with the token behind it, so a directive-led line is outside SHARED-COMMENT-011.
- **FC-004, FC-007–FC-009 on code-fix paths.** No fix behavior changes in this run. The RH5203 relocation above is reported, not pinned.
- **FC-010–FC-014 (formatter parity).** No formatter spec changes in this run. The skeptic's FC-013 observations stay observations, and I added no formatter test. Formatter.Test tags belong to thoenissen/Reihitsu#939.
- **FC-006, FC-018, FC-022–FC-029, FC-036.** No rewrite, rename, or literal handling is involved.
- **FC-031, FC-032, FC-035, FC-037.** No production code changes. Expected values come from the spec examples and requirement text.

## Commands

```shell
scripts/test.sh --project analyzer --filter "TestCategory=SHARED-COMMENT-011|TestCategory=SHARED-COMMENT-001"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5033|FullyQualifiedName~RH5034|FullyQualifiedName~RH5038|FullyQualifiedName~RH5201|FullyQualifiedName~RH5203|FullyQualifiedName~RH5204|FullyQualifiedName~RH5301|FullyQualifiedName~RH5303"
scripts/test.sh --project architecture
```

Results on this tree:
- 30 of 30 tagged tests pass.
- 327 of 327 tests of the touched rules pass.
- The architecture project passes 72 of 72.
- `scripts/format.sh` changed none of the 8 test files.
