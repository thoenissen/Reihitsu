Role: test implementer
Commit: 0f8a53e
Verdict: tests written

This is the auditor repair round on `13-auditor.md` (finding 3 is mine) and `14-spec-author.md`. The spec under test is `sdd/specs/shared/element-documentation.md` at c8de18b. The lead's message carried no intended fix, suspected cause, or hoped-for verdict. No production code is touched.

The escapes below are the six literal source characters of a C# Unicode escape. The test sources contain the literal backslash. I confirmed with `od -c` that it is there after `scripts/format.sh`, in the tests and in the spec example.

## Tests

All tests are on `AnalyzerTestsBase.Verify`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzerTests.VerifySpecificationExampleParameterIsDocumentedByTopLevelElementWithItsName` (changed) | RH8032-001, RH8032-002, SHARED-DOC-012 | Verify | pass |
| `RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzerTests.VerifySpecificationExampleTypeParameterIsDocumentedByTopLevelElementWithItsName` (changed) | RH8033-001, RH8033-002, SHARED-DOC-012 | Verify | pass |
| `RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzerTests.VerifyParameterDeclaredWithUnicodeEscapeIsDocumentedByItsDecodedName` (new) | RH8032-001, RH8032-002, SHARED-DOC-012 / FC-023 | Verify | pass |
| `RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzerTests.VerifyTypeParameterDeclaredWithUnicodeEscapeIsDocumentedByItsDecodedName` (new) | RH8033-001, RH8033-002, SHARED-DOC-012 / FC-023 | Verify | pass |
| `RH8030ElementDocumentationMustHaveSummaryTextAnalyzerTests.VerifyDiagnosticForSummaryOfUnicodeWhiteSpaceOnly` (summary text only) | RH8030-001, SHARED-DOC-010 / FC-031 | Verify | pass |

What changed:

- **Both SHARED-DOC-012 example tests** now carry the first example block as revised. The added declarations are `extension(ushort \u0061)` with `<param name="a">`, and `extension<\u0055>(Queue<U> queue)` with `<typeparam name="U">`, placed after the `HashSet` block as the spec places them. Neither is reported, and the expected diagnostic counts are unchanged.
- **The new escape tests check both sides of the qualifier "with a Unicode escape sequence decoded":**
  - RH8032: `extension(byte \u0061)` documented by `name="a"` is not reported. `extension(sbyte \u0062)` documented by `name="a"` is reported on `\u0062`, and the span covers the escape text.
  - RH8033: `extension<\u0055>(List<U> items)` documented by `name="U"` is not reported. `extension<\u0056>(Stack<V> items)` documented by `name="U"` is reported on `\u0056`.
  - In both, every `name` attribute is plain text, so each case is within SHARED-DOC-012's coverage.
- **Auditor finding 3:** the summary of `VerifyDiagnosticForSummaryOfUnicodeWhiteSpaceOnly` now reads "written as character references". The content was right, so nothing else changed. I did not add literal non-ASCII white space.

Coverage check:

- `grep -nE 'name="[^"]*(\\u|&#x(200B|FEFF|2060|AD);)'` over `src/Reihitsu.Analyzer.Test/Documentation/*.cs` finds nothing.
- So no test has a `name` attribute with an escape or a format character. Such a `name` is the shape SHARED-DOC-012 now leaves out of coverage and that #1130 Q5 files.

## Scenarios (bug fix)

_N/A._ The lane writes specs for existing behavior.

## Not covered, and why

- A `name` attribute that contains a Unicode escape or a format character is out of SHARED-DOC-012's coverage (shared Q5), so neither side is asserted.
- Directly written non-ASCII white space in a summary is not tested. The auditor's harness confirms that the code reports it. SHARED-DOC-010 is already covered by the character-reference forms, which reach the same token `ValueText`.
- Everything else listed under "Not covered" in `06-test-implementer.md` and `10-test-implementer.md` still holds.

## Commands

```shell
scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~RH802|FullyQualifiedName~RH803"
scripts/test.sh --project architecture --configuration Debug
```

Results on 0f8a53e:

- Analyzer filter: 171 passed, 0 failed.
- Architecture: 72 passed, 0 failed, `RequirementCoverageTests` included.
- `scripts/format.sh` was run over the three changed files before the commit.
