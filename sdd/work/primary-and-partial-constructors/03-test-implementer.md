Role: test implementer
Commit: c85f4df
Verdict: #1680: reproduced; #1742: tests written (characterization, all green)

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the unchanged requirements named below, SHARED-DIAG-013, and lead decision 4 (00-lead.md); none from a patched copy of the code.

## thoenissen/Reihitsu#1680

All tests use `Verify(source, diagnostics)` on the analyzer verifier (FC-033: a command-line build hides the duplicate), with `{|#0:…|}` markup, so a second diagnostic on the same span fails the count. Two tests also run the code fix (`Verify(source, fixedSource, …)`).

### Tests

Paths are under `src/Reihitsu.Analyzer.Test/`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Documentation/RH8308…AnalyzerTests.VerifyTextAfterClosingTagOfPositionalRecordIsReportedOnce` | RH8308-001, RH8308-003 | `Verify` | fail (1 expected, 2 actual) |
| `RH8308….VerifyTextAfterClosingTagOfClassWithPrimaryConstructorIsReportedOnce` | RH8308-001, RH8308-003 | `Verify` | fail (2) |
| `RH8308….VerifyTextAfterClosingTagOfNestedPositionalRecordIsReportedOnce` | RH8308-001, RH8308-003 | `Verify` | fail (2) |
| `RH8308….VerifyTextAfterClosingTagOfClassWithPrimaryConstructorIsRemoved` | RH8308-001, RH8308-003, RH8308-004 | `Verify` with fixed source | fail (2, diagnostics of test state) |
| `RH8308….VerifyTextAfterClosingTagInsideParameterListOfPositionalRecordIsReportedOnce` | RH8308-003 | `Verify` | pass (ctor-only node, must stay once) |
| `RH8308….VerifyTextAfterClosingTagInFrontOfAttributeListOfPositionalRecordIsReportedOnce` | RH8308-001, RH8308-003 | `Verify` | pass (trivia before an attribute list is visited once) |
| `Documentation/RH8304….VerifyElementOnLineOfPreviousElementOfPositionalRecordIsReportedOnce` | RH8304-001, RH8304-003 | `Verify` | fail (2) |
| `RH8304….VerifyElementOnLineOfPreviousElementOfClassWithPrimaryConstructorIsReportedOnce` | RH8304-001, RH8304-003 | `Verify` | fail (2) |
| `Documentation/RH8309….VerifyElementOutOfOrderOfPositionalRecordIsReportedOnce` | RH8309-001, RH8309-003 | `Verify` | fail (2) |
| `RH8309….VerifyElementOutOfOrderOfClassWithPrimaryConstructorIsReportedOnce` | RH8309-001, RH8309-003 | `Verify` | fail (2) |
| `Documentation/RH8204….VerifyPlaceholderElementOfPositionalRecordIsReportedOnce` | RH8204-001, RH8204-002 | `Verify` | fail (2) |
| `RH8204….VerifyPlaceholderElementOfClassWithPrimaryConstructorIsReportedOnce` | RH8204-001, RH8204-002 | `Verify` | fail (2) |
| `RH8204….VerifyPlaceholderElementInDelimitedCommentOfPositionalRecordStructIsReportedOnce` | RH8204-001, RH8204-002 | `Verify` | fail (2) |
| `Documentation/RH8305….VerifySummaryInsideParameterListOfMethodIsReportedOnce` | RH8305-003 | `Verify` | pass (sibling anchor: ordinary method) |
| `RH8305….VerifySummaryInsideParameterListOfPositionalRecordIsReportedOnce` | RH8305-003 | `Verify` | fail (1 expected, **0** actual) |
| `Documentation/RH8307….VerifyElementInsideParameterListOfMethodIsReportedOnce` | RH8307-003 | `Verify` | pass (sibling anchor) |
| `RH8307….VerifyElementInsideParameterListOfPositionalRecordIsReportedOnce` | RH8307-003 | `Verify` | fail (1 expected, **0** actual) |
| `Formatting/RH7302….VerifyLowercaseRegionInFrontOfClosingBraceOfClassWithPrimaryConstructorIsReportedOnce` | RH7302-001, RH7302-003 | `Verify` | fail (2) |
| `RH7302….VerifyLowercaseRegionInFrontOfClosingBraceOfClassWithPrimaryConstructorIsFixed` | RH7302-001, RH7302-003 | `Verify` with fixed source | fail (2, diagnostics of test state) |
| `RH7302….VerifyLowercaseRegionInFrontOfStructWithPrimaryConstructorIsReportedOnce` | RH7302-001, RH7302-003 | `Verify` | fail (2) |
| `RH7302….VerifyLowercaseRegionInFrontOfPositionalRecordIsReportedOnce` | RH7302-001, RH7302-003 | `Verify` | fail (2) |
| `RH7302….VerifyLowercaseRegionAfterAttributeListOfClassWithPrimaryConstructorIsReportedOnce` | RH7302-001, RH7302-003 | `Verify` | fail (2) |
| `RH7302….VerifyLowercaseRegionInFrontOfNestedPositionalRecordIsReportedOnce` | RH7302-001, RH7302-003 | `Verify` | fail (2) |
| `RH7302….VerifyLowercaseRegionInsideParameterListOfPositionalRecordIsReportedOnce` | RH7302-001, RH7302-003 | `Verify` | pass (ctor-only node, must stay once) |
| `Formatting/RH7310….VerifyEmptyRegionInFrontOfClosingBraceOfClassWithPrimaryConstructorIsReportedOnce` | RH7310-001, RH7310-003 | `Verify` | fail (2) |
| `RH7310….VerifyEmptyRegionInFrontOfPositionalRecordIsReportedOnce` | RH7310-001, RH7310-003 | `Verify` | fail (2) |
| `Formatting/RH7306….VerifyEndRegionInFrontOfClosingBraceOfClassWithPrimaryConstructorIsReportedOnce` | RH7306-001, SHARED-REGDESC-002 | `Verify` | fail (2) |
| `RH7306….VerifyRegionInFrontOfPositionalRecordIsReportedOnce` | RH7306-001, SHARED-REGDESC-002 | `Verify` | fail (2) |
| `Formatting/RH7307….VerifyEndRegionInFrontOfClosingBraceOfClassWithPrimaryConstructorIsReportedOnce` | RH7307-001, SHARED-REGDESC-002 | `Verify` | fail (2) |
| `RH7307….VerifyRegionInFrontOfPositionalRecordIsReportedOnce` | RH7307-001, SHARED-REGDESC-002 | `Verify` | fail (2) |

The existing tests `RH8305….VerifySummaryOfTypeWithPrimaryConstructorIsReportedOnce` and `RH8307….VerifyElementOfTypeWithPrimaryConstructorIsReportedOnce` (record, record struct, record class, class, struct, nested, in a namespace) already cover the issue's reported case for the two guarded analyzers and pass; they must keep passing once the private guards are replaced.

Every failing test was watched failing on the issue's own difference: `Mismatch between number of diagnostics returned, expected "1" actual "2"`, except the two RH8305/RH8307 parameter-list tests, which fail with `expected "1" actual "0"` — the guarded analyzers drop the node only the constructor run sees, while the same comment in a method's parameter list is reported once (the passing sibling tests). The implementer's guard must turn both directions green.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `///` comment with an RH8308/RH8304/RH8309/RH8204 violation in front of `public record R(int A);` | LF | analyzer | reproduced | 2 vs 1 |
| same in front of `public class Service(int value) { … }` | LF | analyzer | reproduced | 2 vs 1 |
| `/** */` comment with `<placeholder>` in front of `public record struct P(int A);` | LF | analyzer | reproduced | 2 vs 1 |
| `#endregion // Member Implementation` / `// Members` in front of `}` of a primary-constructor class (RH7306, RH7307) | LF | analyzer | reproduced | 2 vs 1 |
| `#region lower` / empty region in front of `}` of a primary-constructor class (RH7302, RH7310) | LF | analyzer | reproduced | 2 vs 1 |
| `#region …` in front of a positional record / struct with primary constructor / nested record (RH7302, RH7306, RH7307, RH7310) | LF | analyzer | reproduced | 2 vs 1 |
| `#region lower` between `[System.Obsolete]` and `public class Service(int value)` | LF | analyzer | reproduced | 2 vs 1 |
| RH8308, RH7302 code-fix path on the primary-constructor class | LF | code fix (`Verify` with fixed source) | reproduced | 2 vs 1 in the test state |
| doc comment / `#region` inside the parameter list of a positional record (RH8308, RH7302) | LF | analyzer | holds | 1 vs 1 |
| doc comment inside the parameter list of a positional record (RH8305, RH8307) | LF | analyzer | reproduced (guard edge) | 0 vs 1 |
| doc comment in front of the attribute list of a positional record (RH8308) | LF | analyzer | holds | 1 vs 1 |

### Not covered, and why

- CRLF variants: the defect is the number of analyzer callbacks per trivia node, decided by the declaration's shape, not by its text; no layout or line-ending logic is in the changed path. The analyzer verifier does not change line endings either.
- Formatter counterpart (FC-013), formatter tests: the formatter is not involved (skeptic, *Surfaces*); the duplicate is analyzer dispatch.
- Fix All, convergence beyond one pass (FC-007, FC-008): the skeptic ran `scripts/apply-fix.sh` for RH8308, RH8304, RH7302, RH7310 (1 action, 1 iteration, LF and CRLF); the two code-fix tests above run the batch verifier on the reproduced shape. The fixes themselves do not change.
- The ~50 declaration-kind documentation analyzers: dropped by lead decision 2 (not handed a node twice).
- Every trivia-kind candidate × every type kind: the arms reach one call site per analyzer — the analyzer's single `RegisterSyntaxNodeAction` on the trivia kind (skeptic's `rg` over `Register*Action`, table in 01-skeptic.md) — and the type kind only decides whether the driver repeats the callback. The tests vary record, record struct, class, struct, nested record, and the attachment points (before the first token, after an attribute list, before `}`, inside the parameter list, before an attribute list) across the candidates; RH8305/RH8307 already have the type-kind matrix.
- A doc comment inside a base-type argument list `: B(/// …)`: the same constructor-only run as the parameter list (skeptic), one representative (parameter list) kept per guarded analyzer and per region rule.
- FC-019/FC-020/FC-021 (comment or directive in a gap the edit touches): no edit of token gaps; directives are the subject of the region tests themselves (FC-005 covered by testing region rules next to doc rules).
- FC-024 partial type with the parameter list on one part: the other part is visited once (skeptic); not separately tested.

## thoenissen/Reihitsu#1742

Maintainer answer 1 (B): output unchanged; SHARED-DIAG-013 writes down today's behavior. The tests characterize it and pass on the unchanged code; a failing one would be a finding against the spec.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Naming/RH4122SingleLetterParameterNamesAnalyzerTests.PartialConstructorParameterNamedAlikeOnBothPartsIsReportedOnceOnTheImplementingPart` | SHARED-DIAG-013 | `Verify` (inline diagnostic markup) | pass |
| `RH4122….PartialConstructorParameterNamedOnlyOnTheDefiningPartIsNotReported` | SHARED-DIAG-013 | `Verify` | pass |
| `RH4122….PartialConstructorWithGeneratedImplementingPartIsNotReportedOnTheDefiningPart` | SHARED-DIAG-013 | `Verify` with a second source `/0/C.g.cs` | pass |
| `Formatting/RH7302….VerifyLowercaseRegionInFrontOfDefiningPartOfPartialConstructorIsNotReported` | SHARED-DIAG-013 | `Verify` | pass |
| `Design/RH2004AccessModifierMustBeDeclaredAnalyzerTests.VerifySpecificationExamplePartialConstructorIsReportedOnTheDefiningPart` | SHARED-DIAG-012, SHARED-DIAG-013 | `Verify` | pass |

The four examples of SHARED-DIAG-013 are tested verbatim; the generated-implementing-part test covers the requirement's "MUST NOT report in the defining part" for the shape the skeptic named (FC-033, two documents). The existing RH7105/RH7106 partial-constructor tests already cover the excepted per-part modifier rules.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH4122 `a` on both parts | LF | analyzer | holds (characterization) | once on implementing part, as specified |
| RH4122 `a` defining, `value` implementing | LF | analyzer | holds | not reported, as specified |
| RH4122 `a` defining, implementing part generated | LF | analyzer | holds | not reported, as specified |
| RH7302 `#region lower` in front of the defining part | LF | analyzer | holds | not reported, as specified |
| RH2004 neither part with accessibility | LF | analyzer | holds | defining part, as specified |

### Not covered, and why

- The ~35 per-part rules of the skeptic's table: the requirement is one shared statement about how the compiler hands elements to rules; one test per rule would test the analyzer host, not Reihitsu. The four spec examples plus the generated-file case represent its two bullets and its exception.
- CRLF, formatter, code fix: output unchanged and nothing in production changes for this issue.
- The RH8102 follow-up candidate: out of scope (00-lead.md, *Follow-up candidates*).

## Whole run

- Files: twelve test classes under `src/Reihitsu.Analyzer.Test/` (Documentation RH8204, RH8304, RH8305, RH8307, RH8308, RH8309; Formatting RH7302, RH7306, RH7307, RH7310; Naming RH4122; Design RH2004), additions only; `scripts/format.sh` changed nothing.
- Run of the twelve touched classes: 419 tests, 394 passed, 25 failed — exactly the 25 new #1680 reproductions above; every existing test in those classes passes.
- Test names collide across classes (`VerifyEndRegionInFrontOfClosingBraceOfClassWithPrimaryConstructorIsReportedOnce`, `VerifyRegionInFrontOfPositionalRecordIsReportedOnce` exist in both RH7306 and RH7307); filter by class.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~.RH8204DoNotUsePlaceholderElementsAnalyzerTests.|FullyQualifiedName~.RH8304XmlDocumentationElementsMustBeOnSeparateLinesAnalyzerTests.|FullyQualifiedName~.RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzerTests.|FullyQualifiedName~.RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzerTests.|FullyQualifiedName~.RH8308NoContentShouldAppearAfterClosingXmlTagsAnalyzerTests.|FullyQualifiedName~.RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzerTests.|FullyQualifiedName~.RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzerTests.|FullyQualifiedName~.RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzerTests.|FullyQualifiedName~.RH7307RegionDescriptionsShouldNotBeMemberOrMembersAnalyzerTests.|FullyQualifiedName~.RH7310EmptyRegionsShouldBeRemovedAnalyzerTests.|FullyQualifiedName~.RH4122SingleLetterParameterNamesAnalyzerTests.|FullyQualifiedName~.RH2004AccessModifierMustBeDeclaredAnalyzerTests."
scripts/test.sh --project analyzer --filter "Name~PrimaryConstructor|Name~PositionalRecord|Name~PartialConstructor"
```
