Role: test implementer
Commit: 49b48aa
Verdict: tests written

Repair round. Inputs: `git show 79f098d` (the amended RH7104-001, RH7104-004, RH8401-001, RH8402-003, RH8501-001, and the `Uses` lines), `07-spec-author.md`, and `00-lead.md`. I re-derived which tests cover each amended clause from the clauses themselves and used the spec author's suggestions only as candidates. The coordinator's message carried no intended fix or verdict. Every test pins a conforming shape and passes on this head. No assertion of an existing test changed.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7104…Tests.PartThatOmitsTheAccessibilityIsReportedAndFixedAloneWhenAnotherPartDeclaresIt` (existing from round 1; tags added) | RH7104-001: the omitting part is reported although another part states `public`. RH7104-004: the fix writes the stated accessibility on the reported part and leaves the other part as it is. Also SHARED-DIAG-004 | `Verify(source, fixed, …)` | pass |
| `RH7104…Tests.NestedPartIsFixedWithTheCompoundAccessibilityAnotherPartStates` (new) | RH7104-001 and RH7104-004. A nested part (whose default would be `private`) is reported, and the fix writes the `protected internal` another part states. FC-024, FC-003: the stated accessibility differs from the default, so the test tells "stated" apart from "default". Member bodies avoid the empty-body shape of thoenissen/Reihitsu#1335 | `Verify(source, fixed, …)` | pass |
| `RH8501…Tests.VerifyDiagnosticWhenDocumentationModeIsNone` / `…IsParse` (existing from round 1; tag added) | RH8501-001 every-mode sentence. The transform records the applied mode and the test asserts it (FC-033) | `Verify` + solution transform | pass |
| `RH8401…Tests.VerifyDiagnosticWhenDocumentationModeIsParse` (new) | RH8401-001 covered scope: mode `Parse`. The other existing tests run in the default mode `Diagnose` | `Verify` + `ApplyDocumentationModeParseToTestProject` | pass |
| `RH8402…Tests.FileNotStartingWithHeaderWhenDocumentationModeIsParse` (new) | RH8402-003 covered scope: mode `Parse`, using the spec's file and configuration | `Verify` + `ApplyDocumentationModeParseToTestProject` | pass |

## Not covered, and why

- **RH8401-001 and RH8402-003 in mode `None`.** The amended sentences make what these rules report in mode `None` "not covered here". Today both rules stay silent there, which SHARED-DOC-008 contradicts (thoenissen/Reihitsu#1346). No test pins either side. The untagged RH8401 characterization test is unchanged; its round-1 summary already names SHARED-DOC-008 and the follow-up.
- **RH7104 with empty bodies.** `PartialTypesWithPublicAndCompoundAccessibilityAreFixed` (untagged) is left untouched and untagged. Its empty bodies collapse to `;`, which is the P14d shape of #1335, so it does not pin a conforming result for RH7104-004.
- **CRLF, Fix All, comment and directive gaps.** The amendments change no layout and no Fix All behavior. RH7104's Fix All runs in `FixAllCorrectsEveryDiagnostic` of the base class.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7104Partial|FullyQualifiedName~RH8501Code|FullyQualifiedName~RH8401Single|FullyQualifiedName~RH8402File"
scripts/test.sh --project architecture
```

Results on 49b48aa:
- Analyzer filter: 66/66 passed. The 6 new or retagged tests also pass when run by name.
- Architecture: 72/72 passed.
- `scripts/format.sh` over the 4 changed files changed nothing.
