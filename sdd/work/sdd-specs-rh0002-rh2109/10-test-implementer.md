Role: test implementer
Commit: da825a9
Verdict: tests written

Repair after escalation 7 (`squad/decisions/2026-10-20-996-rh2001-002-second-spec-check.md`), lane Tooling (widened from tests, see `00-lead.md`). No production code was touched; both new tests pass on the unchanged code. No test contradicts a spec. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Inputs: `00-lead.md`, `04-test-implementer.md` (characterization convention: untagged, doc comment starts with "Characterizes …" and says the case is an open question, or one decided by principle), `08-spec-checker.md`, `09-spec-author.md` ("Examples for the tests", final RH2001 Q4 and RH2109 Q1 texts), `sdd/specs/analyzer/RH2001.md`.

## Tests

Both in `src/Reihitsu.Analyzer.Test/Design`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.VerifyPropertiesUsingFieldKeywordWithoutLogicAreReportedWithoutFix` (new) | Char. RH2001 Q4: `private int Other { get => field; }` and `private int Plain => field;`, the two Q4 inputs no test pinned (`Value` with setter logic stays in the existing `VerifyPropertyUsingFieldKeywordWithLogicIsReportedWithoutFix`) | `VerifyNoCodeFixIsOffered` | pass: both reported on the name, no fix |
| `RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests.VerifyCodeBlockTextInCSharpCommentAndStringLiteralsIsReported` (new) | Char. RH2109 Q1 (decided by principle P5): the exact `Counter.razor` input of the Q1 text, including the raw string literal | `Verify` + additional file, `RazorCodeBlockSpan` | pass: four diagnostics at 1:1, 3:5, 5:25, 7:1, each 5 characters |

The RH2109 test is a sibling of `VerifyCodeBlockTextInCommentsAndStringsIsReported`, which I left unchanged: that one also pins the HTML and Razor comment inputs of Q2, so the two questions stay separable. The Razor source is a four-quote raw string so the nested `"""` stays literal.

Watched fail: with the `{|RH2001:Plain|}` markup removed, the RH2001 test failed with "expected "1" actual "2""; with the raw-string expectation moved to 7:2, the RH2109 test failed with "Expected diagnostic to start at column "2" was actually at column "1"". Both files were restored before the final runs.

## Tagged tests against the narrowed RH2001-001/-002 wording

- RH2001-001 (coverage: no attribute, not expression-bodied, accessors without bodies): `VerifyPrivateAutoPropertyDiagnostic`, `VerifyPrivateAutoPropertiesOfEveryShapeAreReported`, `VerifyFixReplacesPropertiesWithFieldsAndRenamesReferences`, `VerifyNoFixIsOfferedForInitAndModifiedAccessors`. Every reported property uses `{ get; set; }`, `{ get; }`, `{ get; init; }` or `{ readonly get; set; }` (an accessor modifier, not an attribute or a body). None is expression-bodied or attributed. Match.
- RH2001-002 (not private; no backing field: expression body or accessor bodies not using `field`; property-targeted attribute; explicit interface implementation): `AttributedPrivateAutoPropertyIsNotReported` (`[Obsolete]`), `ExplicitInterfaceImplementationAutoPropertyIsNotReported`, `VerifyOtherPropertiesAreNotReported` (`Computed => 1`, `Body { get { return 1; } }`, neither uses `field`). Match. No tag change was needed.

## Not covered, and why

- No formatter, CRLF, Fix All, or gap tests: both tests characterize analyzer reporting only. The RH2001 one asserts that no fix is offered, and RH2109 has no code fix. The run's earlier matrix (`04-test-implementer.md`) is otherwise unchanged.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001|FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2109"
scripts/test.sh --project architecture
```

Results on this tree: focused run "Passed!  - Failed: 0, Passed: 41"; architecture "Passed!  - Failed: 0, Passed: 72". `scripts/format.sh` on both test files: "Formatted 0 of 2 file(s)".
