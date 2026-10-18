Role: test implementer
Commit: f352dea
Verdict: tests written

This is the repair round for finding 3 of `07-auditor.md`, plus a check of every tag against the coverage that `08-spec-author.md` narrowed. The lead's message offered the auditor's change only as a suggestion and carried no intended fix or verdict, so there was nothing to ignore. I did not touch finding 4 (the missing BOM), which goes to the cleaner.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH8001NonPrivateClassesMustBeDocumentedAnalyzerTests.VerifyNoDiagnosticForClassWithMalformedAmpersandInDocumentation` | none (open question Q2 of thoenissen/Reihitsu#1037; tags unchanged) | `Verify` + `ApplyDocumentationModeParseToTestProject` | pass |
| `RH8001NonPrivateClassesMustBeDocumentedAnalyzerTests.VerifyNoDiagnosticForClassWithUnclosedTagInDocumentation` | none (Q2; tags unchanged) | same | pass |
| `RH8013NonPrivateEnumMembersMustBeDocumentedAnalyzerTests.VerifyNoDiagnosticForEnumMemberWithMalformedAmpersandInDocumentation` | none (Q2; tags unchanged) | same | pass |

### Finding 3 — the duplicated Parse-mode setup

I re-derived the finding:

- The three local `ApplyParseDocumentationMode` functions had the same body as the base helper `AnalyzerTestsBase<TAnalyzer>.ApplyDocumentationModeParseToTestProject`, statement for statement. All three test methods share that base class.
- `rg "ApplyParseDocumentationMode" src/Reihitsu.Analyzer.Test` showed no other caller.

Change:

- The three tests now pass the base helper, and the local functions are deleted.
- The usings `Microsoft.CodeAnalysis` and `Microsoft.CodeAnalysis.CSharp` became unused in both files and are removed.
- Nothing else changed: source text, assertions, `[Requirement]` attributes, and the files' BOM are all as before. `git diff` shows no `Requirement` or `public async Task` line among the changed lines.
- I left `RH0002…Tests.ApplyDocumentationMode` alone. That class is not one this run edited, and its helper takes the mode as a parameter.

### Tags checked against the narrowed coverage

The new coverage sentences exclude partial types declared in several parts (SHARED-DOC-002), tag names in upper case or in an XML namespace (SHARED-DOC-005 and SHARED-DOC-006), and included content with such names (SHARED-DOC-007).

- **Partial types.** `rg -n "partial"` over the R01–R19 test classes finds only `VerifyNoDiagnosticForPartialClassWithSummary` (RH8001) and `VerifyNoDiagnosticForPartialRecordWithSummary` (RH8007). Each declares a single part, so both stay covered. Neither carries SHARED-DOC-002.
- **Tag names in comments.** No test source in these classes has an upper-case `summary` or `inheritdoc` tag name, a namespace prefix, or an `xmlns` attribute.
- **Included content.** The included XML in the RH8001 and RH8013 tests uses only lower-case, namespace-free `summary`, `remarks`, and `inheritdoc`, and is well-formed.
- **Malformed XML.** The three malformed-XML tests carry no SHARED-DOC tag.
- **Result:** no tag needed to change, and none was changed.

## Not covered, and why

- **Finding 4** (the BOM of `InMemoryXmlReferenceResolver.cs`) belongs to the cleaner, as the lead routed it.
- **The new cases outside the specs** in `08-spec-author.md` are open questions, so no test asserts them:
  - the group of a modifier-less part of a partial type;
  - upper-case and namespaced tag names, in comments and in included files;
  - an undeclared namespace prefix.
- **The auditor's other hints** (the repeated included-XML literal, the doc comment between modifiers) are not findings. I left them alone.

## Commands

```shell
scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Documentation.RH80|FullyQualifiedName~RH8107"
scripts/test.sh --project architecture --configuration Debug
```

Results on this tree:

- Analyzer filter: 305 passed (the RH80* documentation classes and RH8107).
- Architecture project: 72 passed.
- `scripts/format.sh` on the two changed files: no change.
