Role: test implementer
Commit: 3d2f680
Verdict: tests written

This is the repair round on `08-spec-checker.md` and `09-spec-author.md`. The spec under test is `sdd/specs/shared/element-documentation.md` at 4c0e98c. The lead's message carried no intended fix, suspected cause, or hoped-for verdict. No production code is touched.

## Tests

All tests are on `AnalyzerTestsBase.Verify`. "Include resolver" means `CreateIncludedDocumentationTransform`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH8030ElementDocumentationMustHaveSummaryTextAnalyzerTests.VerifySpecificationExampleOfSummaryWithoutTextWithLineFeeds` (changed) | RH8030-001, RH8030-002, SHARED-DOC-010 | Verify | pass |
| `RH8030ElementDocumentationMustHaveSummaryTextAnalyzerTests.VerifySpecificationExampleOfSummaryWithoutTextWithCarriageReturnLineFeeds` (changed) | RH8030-001, RH8030-002, SHARED-DOC-010 | Verify, CRLF | pass |
| `RH8031ExtensionDeclarationsMustHaveSummaryTextAnalyzerTests.VerifyNoDiagnosticForExtensionDeclarationSummariesWithText` (changed) | RH8031-002, SHARED-DOC-010 | Verify | pass |
| `RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzerTests.VerifySpecificationExampleInheritdocInIncludedFileExemptsParameter` (new) | RH8032-001, SHARED-DOC-011 | Verify, include resolver | pass |
| `RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzerTests.VerifySpecificationExampleMergeQualifiersOfParameters` (new) | RH8032-001, SHARED-DOC-013 | Verify | pass |
| `RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzerTests.VerifySpecificationExampleMergeQualifiersOfTypeParameters` (new) | RH8033-001, SHARED-DOC-013 | Verify | pass |

What changed:

- The two RH8030 SHARED-DOC-010 example tests no longer contain class `I` (`<summary>&#x200B;</summary>`), which the spec removed. Each still expects 7 diagnostics, because `I` was never reported.
- The RH8031 test no longer contains the `&#x200B;` block. It keeps the `* Text` and mid-line `*` blocks, and its summary text no longer mentions a zero-width space.
- The new tests use the spec's new example blocks as written: the SHARED-DOC-011 `<include>` block and `docs.xml`, and the second SHARED-DOC-013 block, `Example — RH8032 and RH8033`. That block carries the marks of both rules. RH8032 runs it with its own marks only (`span`, `number`, `value`), and RH8033 likewise (`U`, `T` with `struct`, `T` with `[Mark(0)]`).
- Neither of the tests that `09-spec-author.md` named for the zero-width space contains one: `RH8030…VerifyDiagnosticForSummaryOfUnicodeWhiteSpaceOnly` uses `&#9;`, `&#x2003;`, `&#x85;`, `&#xA0;`, `&#32;`, and `&#160;`, all Unicode White_Space and none in category Cf, and `VerifyStarsOfBlockDocumentationComments` contains no format character. Both stay unchanged.
- After the edit, `rg -i "x200B|xFEFF|x2060|xAD;|x200C|x180E" src/Reihitsu.Analyzer.Test/Documentation` finds nothing. So no test asserts a summary with a Cf character, which SHARED-DOC-010 now leaves out and shared Q7 files.

## Scenarios (bug fix)

_N/A._ The lane writes specs for existing behavior.

## Not covered, and why

- Summaries that contain a Unicode Cf character: they are out of SHARED-DOC-010's coverage (shared Q7), so neither side is asserted.
- Everything else listed under "Not covered" in `06-test-implementer.md` still holds.

## Commands

```shell
scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~RH802|FullyQualifiedName~RH803"
scripts/test.sh --project architecture --configuration Debug
```

Results on 3d2f680:

- Analyzer filter: 169 passed, 0 failed.
- Architecture: 72 passed, 0 failed, `RequirementCoverageTests` included.
- `scripts/format.sh` was run over the four changed files before the commit.
