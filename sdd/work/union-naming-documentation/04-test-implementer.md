Role: test implementer
Commit: efe4373
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the examples and requirement text of RH4012, RH8035, RH8036, the union examples added to RH8030, RH8108–RH8111, RH8203, SHARED-DOC-001/002, the change doc's visible output changes 1–6, and the skeptic's candidates (`02-skeptic.md`, *Breaking inputs* 2–6), under the maintainer's answers 1A–5A.

## Compiling a union in the tests

`src/Reihitsu.Analyzer.Test/Base/UnionSupport.cs` (new, test-only): compiles `System.Runtime.CompilerServices.IUnion` and `UnionAttribute` into a separate assembly with `CompiledAssemblyReference` and hands it out as a metadata reference (`GetReferenceAsync()`, and `GetReference()` for the synchronous `FixAllScenario.Configure`). Every union test adds it to `TestState.AdditionalReferences`; the fixed state inherits it. This is the skeptic's polyfill route (internal choice 2a), as a referenced assembly instead of an extra source, so the polyfill's declarations are never analyzed and need no markup. Observed: with it, a union compiles under `ReferenceAssemblies.Net.Net90` with no compiler diagnostic (the RH8030 failure lists `Diagnostics: NONE`).

Roslyn 5.12 fails with `CS1569` (NullReferenceException writing the XML documentation file) on a `<param name="…">` element on a union under the default documentation mode — observed in this run. The two tests that need such an element (RH8102, RH8104) therefore run in `DocumentationMode.Parse` and each carries a method whose `<param>` is reported in the same source, so the "not reported" on the union is not vacuous.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4012UnionNameCasingAnalyzerTests.VerifyDiagnosticForUnionWrongCasing` | RH4012-001, RH4012-002 | `Verify(source, fixed, …)` | compile error (type missing) |
| `RH4012UnionNameCasingAnalyzerTests.VerifyNoDiagnosticsForPascalCaseUnions` | RH4012-001 | `Verify(source, source)` | compile error |
| `RH4012UnionNameCasingAnalyzerTests.UnionsAreCheckedAndStructsAreNot` (spec example, with the fix applied) | RH4012-001, RH4012-002 | `Verify(source, fixed)` | compile error |
| `RH4012UnionNameCasingAnalyzerTests.UnionsAreCheckedWhateverTheirModifiersAndNesting` (`readonly`, `file`, in union body, `protected internal`, `private protected`, in interface) | RH4012-001, RH4012-002; FC-024, FC-027 | `Verify(source, fixed)` | compile error |
| `RH4012UnionNameCasingAnalyzerTests.FixRenamesUnionAndEveryReference` (spec example; case type of another union, field type) | RH4012-002; FC-018 | `Verify(source, fixed)` | compile error |
| `RH4012UnionNameCasingAnalyzerTests.VerbatimUnionNameIsReportedAndRenamed` (`@pet` → `Pet`, reference too) | RH4012-001, RH4012-002; FC-023 | `Verify(source, fixed)` | compile error |
| `RH4012UnionNameCasingAnalyzerTests.FixRenamesEveryPartOfAPartialUnion` | RH4012-001, RH4012-002; FC-024 | `Verify(source, fixed)` | compile error |
| `RH4012UnionNameCasingAnalyzerTests.NoFixIsOfferedWhenTheNewNameIsTheNameOfTheGeneratedValueMember` (`union value` → CS0542) | SHARED-CASING-009; FC-006 | `Verify(source, source)` | compile error |
| `RH4012UnionNameCasingAnalyzerTests.NoFixIsOfferedWhenTheNewNameWouldRebindACaseType` (`private union cat(Cat, Dog)` in `Outer`) | SHARED-CASING-009; FC-036 | `Verify(source, source)` | compile error |
| `RH4012UnionNameCasingAnalyzerTests.FixAllCorrectsEveryDiagnostic` (inherited; scenario: two unions, one a case type of the other, property types) | SHARED-FIX-001, SHARED-FIX-002 (base tags); FC-007, FC-008 | `GetFixAllScenario` | compile error |
| `RH4012UnionNameCasingAnalyzerTests.CodeFixProvidesFixAllProvider` (inherited) | — | base | compile error |
| `RH8035NonPrivateUnionsMustBeDocumentedAnalyzerTests.VerifyDiagnosticForPublicUnionWithoutDocumentation` (message) | RH8035-001, RH8035-002 | `Verify(source, …)` | compile error |
| `RH8035…VerifyDiagnosticsForUndocumentedNonPrivateUnions` (spec example) | RH8035-001, RH8035-002, SHARED-DOC-002, SHARED-DOC-005 | `Verify` | compile error |
| `RH8035…VerifyDiagnosticsForUndocumentedUnionsOfEveryNonPrivateKind` (`file`, `internal readonly`, `protected internal`, `private protected`, no modifier in interface) | RH8035-001, SHARED-DOC-002; FC-024, FC-027 | `Verify` | compile error |
| `RH8035…VerifyDiagnosticForUndocumentedUnionNestedInUndocumentedClass` | RH8035-001, SHARED-DOC-001 | `Verify` | compile error |
| `RH8035…VerifyDiagnosticForUndocumentedUnionWithBody` (SHARED-DOC-001 union example, RH8035 part) | RH8035-001, SHARED-DOC-001 | `Verify` | compile error |
| `RH8035…VerifyNoDiagnosticForDocumentedUnions` (summary; `<inheritdoc/>` only) | RH8035-001, SHARED-DOC-005 | `Verify` | compile error |
| `RH8035…VerifyNoDiagnosticForUndocumentedPrivateUnions` (no modifier in struct, record, union body) | RH8035-001, SHARED-DOC-002 | `Verify` | compile error |
| `RH8035…VerifyNoDiagnosticsWhenDocumentationModeIsNone` | RH8035-001, SHARED-DOC-008 | `Verify` + `ApplyDocumentationModeNoneToTestProject` | compile error |
| `RH8035…VerifyNoDiagnosticForStructDeclarations` | RH8035-001 | `Verify` | compile error |
| `RH8036PrivateUnionsMustBeDocumentedAnalyzerTests.VerifyDiagnosticForPrivateUnionWithoutDocumentation` (message) | RH8036-001, RH8036-002, SHARED-DOC-002 | `Verify` | compile error |
| `RH8036…VerifyDiagnosticsForUndocumentedPrivateUnions` (spec example) | RH8036-001, RH8036-002, SHARED-DOC-002, SHARED-DOC-005 | `Verify` | compile error |
| `RH8036…VerifyDiagnosticsForUndocumentedUnionsOfEveryPrivateKind` (no modifier in struct, record, union body; `private readonly`) | RH8036-001, SHARED-DOC-002; FC-027 | `Verify` | compile error |
| `RH8036…VerifyDiagnosticForUndocumentedUnionNestedInUndocumentedUnion` | RH8036-001, SHARED-DOC-001 | `Verify` | compile error |
| `RH8036…VerifyNoDiagnosticForDocumentedPrivateUnions` (summary; `<inheritdoc/>`) | RH8036-001, SHARED-DOC-005 | `Verify` | compile error |
| `RH8036…VerifyNoDiagnosticForUndocumentedNonPrivateUnions` (top level with/without modifier, `file`, in interface) | RH8036-001, SHARED-DOC-002 | `Verify` | compile error |
| `RH8036…VerifyNoDiagnosticsWhenDocumentationModeIsNone` | RH8036-001, SHARED-DOC-008 | `Verify` + mode None | compile error |
| `RH8036…VerifyNoDiagnosticForPrivateStructDeclarations` | RH8036-001 | `Verify` | compile error |
| `RH8030…VerifyDiagnosticForUnionWithEmptySummary` (spec example) | RH8030-001, RH8030-002 | `Verify` | fails: expected 1, actual 0 |
| `RH8030…VerifyDiagnosticForNestedPrivateUnionWithEmptySummary` (`<summary/>`) | RH8030-001 | `Verify` | fails: expected 1, actual 0 |
| `RH8108…VerifyDiagnosticForUndocumentedUnionTypeParameter` (spec example; undocumented union not reported) | RH8108-001, RH8108-002 | `Verify` | fails: expected 1, actual 0 |
| `RH8108…VerifyDiagnosticForUnionTypeParameterWhenTypeParamElementNamesAnother` (change doc item 4) | RH8108-001 | `Verify` | fails: expected 1, actual 0 |
| `RH8108…VerifyDiagnosticForUndocumentedTypeParameterOfConstrainedNestedUnion` (`where T : class`, body, private) | RH8108-001 | `Verify` | fails: expected 1, actual 0 |
| `RH8109…VerifyDiagnosticsForUnionTypeParamElementsInWrongOrder` (spec example) | RH8109-001, RH8109-002 | `Verify` | fails: expected 2, actual 0 |
| `RH8109…VerifyDiagnosticForUnionTypeParamElementNamingAnotherTypeParameter` (change doc item 4) | RH8109-001 | `Verify` | fails: expected 1, actual 0 |
| `RH8110…VerifyDiagnosticForUnionTypeParamElementWithoutName` (spec example) | RH8110-001, RH8110-002 | `Verify` | fails: expected 1, actual 0 |
| `RH8111…VerifyDiagnosticForUnionTypeParamElementWithoutText` (spec example) | RH8111-001, RH8111-002 | `Verify` | fails: expected 1, actual 0 |
| `RH8111…VerifyDiagnosticForUnionTypeParamElementWithoutTextNamingAnotherTypeParameter` (change doc item 4) | RH8111-001 | `Verify` | fails: expected 1, actual 0 |
| `RH8203…VerifyDiagnosticForInheritdocOnUnionWithoutBaseList` (spec example; base list not reported) | RH8203-001, RH8203-002 | `Verify` | fails: expected 1, actual 0 |
| `RH8203…VerifyDiagnosticForInheritdocOnNestedUnionWithBodyWithoutBaseList` | RH8203-001 | `Verify` | fails: expected 1, actual 0 |
| `RH8203…VerifyNoDiagnosticForInheritdocWithCrefOnUnion` | RH8203-001 | `Verify` | passes (must stay) |
| `RH8101…VerifyNoDiagnosticForUnionCaseTypes` | RH8101-001 | `Verify` | passes (must stay) |
| `RH8102…VerifyNoDiagnosticForParamElementOnUnion` (+ reported method sibling, mode Parse) | RH8102-001 | `Verify` + `ApplyDocumentationModeParseToTestProject` | passes (must stay) |
| `RH8103…VerifyNoDiagnosticForParamElementWithoutNameOnUnion` | RH8103-001 | `Verify` | passes (must stay) |
| `RH8104…VerifyNoDiagnosticForParamElementWithoutTextOnUnion` (+ reported method sibling, mode Parse) | RH8104-001 | `Verify` + mode Parse | passes (must stay) |
| `RH8020…VerifyDiagnosticForUndocumentedMethodInUnionBody` (SHARED-DOC-001 union example, RH8020 part) | SHARED-DOC-001 | `Verify` | passes (clarification) |
| `RH8020…VerifyNoDiagnosticForImplicitlyPrivateMethodInUnionBody` | SHARED-DOC-002 | `Verify` | passes (clarification) |
| `RH8021…VerifyDiagnosticsForPrivateMethodsInUnionBody` (SHARED-DOC-002 union example, RH8021 part) | SHARED-DOC-002 | `Verify` | passes (clarification) |
| `RH4105MethodParameterCasingAnalyzerTests.VerifyNoDiagnosticForUnionCaseTypes` | — (characterization, skeptic candidate 5) | `Verify(source, onConfigure)` | passes (must stay) |
| `RH4122SingleLetterParameterNamesAnalyzerTests.VerifyNoDiagnosticForUnionCaseTypes` | — (characterization, skeptic candidate 5) | `Verify` | passes (must stay) |

Spec examples whose markup names two rules are split by analyzer, each test carrying only its own analyzer's markup: SHARED-DOC-001 (RH8020 part in `RH8020…InUnionBody`, RH8035 part in `RH8035…WithBody`), SHARED-DOC-002 (RH8021 part in `RH8021…`, RH8036 part covered by `RH8036…OfEveryPrivateKind`, `InUnion`), change doc item 4 (one test each in RH8108, RH8109, RH8111). The RH8036 spec example and RH8035 spec example are used verbatim.

### Observed on this head (efe4373)

- New-rule test files (`RH4012…`, `RH8035…`, `RH8036…`): the test project does not compile until `RH4012UnionNameCasingAnalyzer`, `RH4012UnionNameCasingCodeFixProvider`, `RH8035NonPrivateUnionsMustBeDocumentedAnalyzer`, `RH8036PrivateUnionsMustBeDocumentedAnalyzer` (the spec's code owners) and `AnalyzerResources.RH4012MessageFormat`, `RH8035MessageFormat`, `RH8036MessageFormat` exist. That is the expected "red" for a new rule; it also blocks every other test of the project until the implementer adds them.
- With those three files moved aside, `--filter "FullyQualifiedName~Union"` on this head: 22 tests, 12 failed, 10 passed. Every failure is `Mismatch between number of diagnostics returned, expected "1"` (11) or `"2"` (1) `actual "0"` — no compiler diagnostic anywhere, so each fails on the missing report. The 10 passing ones are the must-stay rows above.
- Harness check, not a source of expected values (FC-035): in a throwaway worktree (deleted, nothing committed) I copied RH4011/RH8003/RH8004 to union stubs and added `SyntaxKind.UnionDeclaration` to `DocumentableDeclarationKinds` and `TypeParameterOwnerKinds`; all 50 tests matching `Union|RH4012|RH8035|RH8036` passed, including both withheld-fix cases, the verbatim rename, the partial union, and Fix All. So every test input compiles, every markup span is reachable, and no test depends on anything the spec does not state.

## Not covered, and why

- **LF/CRLF variants** — no layout is involved: RH4012's fix is a Renamer rename (no formatter call in `CasingCodeFixProviderBase`), and RH8035/RH8036/RH8030/RH8108–RH8111/RH8203 report only. The existing RH4002–RH4011 tests carry no line-ending variants either.
- **Comment/directive in a gap (FC-004, FC-005, FC-019–FC-021)** — no token gap is edited; a rename replaces identifier tokens only.
- **SHARED-CASING-010 (Fix All skips colliding names), SHARED-CASING-011/012, the `#if` coverage of SHARED-CASING-007** — the fix uses the shared casing fix: `rg -n "CasingCodeFixProviderBase<" src/Reihitsu.Analyzer.CodeFixes/Rules/Naming` lists every type-casing fix on the shared `CasingCodeFixProviderBase<TNode>`, whose collision, Fix All, and excluded-code checks are tested once per mechanism in the RH4002–RH4011/RH41xx suites; a union fix adds only `GetIdentifier`. The two union-specific collisions the skeptic found (generated `Value`, case-type rebinding) are tested.
- **FC-013/FC-014 (formatter parity)** — names and documentation presence are content the formatter does not judge (skeptic, *Failure classes kept*).
- **Formatter, CLI, FMT-UNSUP classes (FC-038–FC-043), FC-010–FC-012, FC-028/029** — formatter not involved; no shape declared unsupported.
- **FC-022, FC-025, FC-026** — no literal, expression body, or brace edit is involved.
- **A partial union in several parts for RH8035/RH8036** — SHARED-DOC-001/002 exclude partial elements declared in several parts.
- **`unsafe`, `static`, `sealed`, `abstract`, `ref` union** — the compiler rejects them (skeptic: CS0106/CS0227).
- **The rule pages' examples** — `RuleDocumentationExampleVerifier` (self-hosting) and `AnalyzerPackageMetadataTests` already cover new rule pages; no extra test.
- **`<param>` on a union under the default documentation mode** — Roslyn 5.12 fails with CS1569 there (outside this repository); tested under `Parse`.

## Follow-up candidates

None.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Union|FullyQualifiedName~RH4012|FullyQualifiedName~RH8035|FullyQualifiedName~RH8036"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4012UnionNameCasingAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Documentation.RH8035NonPrivateUnionsMustBeDocumentedAnalyzerTests|FullyQualifiedName~Reihitsu.Analyzer.Test.Documentation.RH8036PrivateUnionsMustBeDocumentedAnalyzerTests"
```
