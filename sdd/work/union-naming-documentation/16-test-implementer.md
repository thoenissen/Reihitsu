Role: test implementer
Commit: 1a32759
Verdict: tests written

Architecture-finding refusal (lead decision 6, `00-lead.md`), for audit finding #2 (`14-auditor.md`) against SHARED-DOC-019 as widened in 1a32759 (`15-spec-author.md`). Not a repair round. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from SHARED-DOC-019's new example ("a tag name in another letter case"), its definition of a parameter element (any letter case, prefix ignored), and its paragraph (b) (no exception when the expansion fails; a union whose documentation the compiler expands is judged).

Every new test adds `UnionSupport`; the "not reported" tests set `CompilerDiagnostics = CompilerDiagnostics.None` (CS1569 only), so `AD0001` is still compared. The two "reported" tests with a `Parameter` element keep compiler diagnostics on. Mode `Diagnose` is the library default; `…InParseMode` tests add `ApplyDocumentationModeParseToTestProject`. Include tests use the repository's `CreateIncludedDocumentationTransform`.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH8035…VerifyNoDiagnosticForUnionsWhoseParameterElementsAreWrittenInAnotherLetterCase` (spec example: `<Param>`, `<PARAMREF/>`, `<pAram>`) | RH8035-001, SHARED-DOC-019 | `Verify` | fails: AD0001 (NullReferenceException) from RH8035 |
| `RH8035…VerifyNoDiagnosticForUnionsWhoseParameterElementsAreWrittenInAnotherLetterCaseInParseMode` | RH8035-001, SHARED-DOC-019 | `Verify` + Parse | fails: RH8035 reported on `Shape` (2,14) |
| `RH8035…VerifyNoDiagnosticForUnionWhoseDocumentationHoldsUpperCaseParamElement` (`<PARAM>`) | RH8035-001, SHARED-DOC-019 | `Verify` | fails: AD0001 |
| `RH8035…VerifyNoDiagnosticForUnionWhoseDocumentationHoldsPrefixedParamElement` (`<x:param name=…/>`) | RH8035-001, SHARED-DOC-019 | `Verify` | passes (must stay; prefix already refused, as the auditor probed for RH8203) |
| `RH8035…VerifyNoDiagnosticForPartialUnionWhosePartInAnotherFileHoldsUpperCaseParamrefElement` (`<remarks>` part in Test0, `<PARAMREF/>` part in Test1) | RH8035-001, SHARED-DOC-019 | `Verify` + second source | fails: RH8035 reported on both parts (Test0 2,22; Test1 2,22) |
| `RH8035…VerifyDiagnosticForUnionWhoseDocumentationHoldsElementNamedParameter` (other side: `<Parameter name=…>` is no parameter element) | RH8035-001, SHARED-DOC-019 | `Verify` with markup | passes (must stay) |
| `RH8035…VerifyNoDiagnosticForUnionWhoseIncludedDocumentationHoldsSummaryAndParamElement` (`<include>` brings `<summary>` and `<param>`; (b): no exception, and no report whether or not the expansion fails) | RH8035-001, SHARED-DOC-019 | `Verify` + include transform | passes (must stay) |
| `RH8035…VerifyDiagnosticForUnionWhoseIncludedDocumentationHoldsOnlyParamElement` (other side of (b): `<include>` brings only `<param>`; the comment holds no parameter element and the expansion succeeds, so the union is judged) | RH8035-001, SHARED-DOC-019 | `Verify` + include transform, markup | passes (must stay) |
| `RH8036…VerifyNoDiagnosticForPrivateUnionWhoseDocumentationHoldsMixedCaseParamrefElement` (`<ParamRef/>` in `<remarks>`) | RH8036-001, SHARED-DOC-019 | `Verify` | fails: AD0001 from RH8036 |
| `RH8036…VerifyNoDiagnosticForPrivateUnionWhoseDocumentationHoldsParamElementInAnotherLetterCase` (`<Param>`) | RH8036-001, SHARED-DOC-019 | `Verify` | fails: AD0001 |
| `RH8036…VerifyNoDiagnosticForPrivateUnionWhoseDocumentationHoldsParamElementInAnotherLetterCaseInParseMode` | RH8036-001, SHARED-DOC-019 | `Verify` + Parse | fails: RH8036 reported on `Inner` (7,19) |
| `RH8108…VerifyNoDiagnosticForUnionsWhoseParameterElementsAreWrittenInAnotherLetterCase` (spec example) | RH8108-001, SHARED-DOC-019 | `Verify` | fails: AD0001 from RH8108 (line 64) |
| `RH8108…VerifyNoDiagnosticForUnionsWhoseParameterElementsAreWrittenInAnotherLetterCaseInParseMode` (spec example) | RH8108-001, SHARED-DOC-019 | `Verify` + Parse | passes; does not falsify the guard, because the example's `Result` documents `T` — the next-but-one test does |
| `RH8108…VerifyNoDiagnosticForUndocumentedTypeParameterOfUnionWhoseDocumentationHoldsParamElementInAnotherLetterCase` (`<pAram>`, no `<typeparam>`) | RH8108-001, SHARED-DOC-019 | `Verify` | fails: AD0001 |
| `RH8108…VerifyNoDiagnosticForUndocumentedTypeParameterOfUnionWhoseDocumentationHoldsParamElementInAnotherLetterCaseInParseMode` | RH8108-001, SHARED-DOC-019 | `Verify` + Parse | fails: RH8108 reported on `T` (5,21) |
| `RH8203…VerifyNoDiagnosticForUnionsWhoseParameterElementsAreWrittenInAnotherLetterCase` (spec example) | RH8203-001, SHARED-DOC-019 | `Verify` | fails: three AD0001 from RH8203 (one per union) |
| `RH8203…VerifyNoDiagnosticForUnionsWhoseParameterElementsAreWrittenInAnotherLetterCaseInParseMode` | RH8203-001, SHARED-DOC-019 | `Verify` + Parse | fails: RH8203 reported on `Holder`'s `<inheritdoc/>` (4,5) |
| `RH8203…VerifyNoDiagnosticForInheritdocOnUnionWhoseDocumentationHoldsParamElementInAnotherLetterCase` (auditor counterexample: `<inheritdoc/>` + `<Param>`) | RH8203-001, SHARED-DOC-019 | `Verify` | fails: AD0001 |
| `RH8203…VerifyNoDiagnosticForInheritdocOnPartialUnionWhosePartInAnotherFileHoldsParamElementInAnotherLetterCase` (`<inheritdoc/>` in Test0, `<Param>` in Test1) | RH8203-001, SHARED-DOC-019 | `Verify` + second source | fails: RH8203 reported at Test0 (1,5) and at Test1 (2,22) |
| `RH8203…VerifyDiagnosticForInheritdocOnUnionWhoseDocumentationHoldsElementNamedParameter` (other side: `<inheritdoc/>` + `<Parameter>`) | RH8203-001, SHARED-DOC-019 | `Verify` with markup | passes (must stay) |

Observed on 1a32759 (Release, `scripts/test.sh`): 20 new tests, 14 fail, 6 pass, each as the last column says. Every failure is either an `AD0001` NullReferenceException from the rule under test (Diagnose) or the rule's own report on the refused union (Parse, or a partial union, which takes no expansion path for the analyzed part); no compiler diagnostic and no unexpected report elsewhere. The earlier SHARED-DOC-019 tests (`10-test-implementer.md`) are unchanged.

## Not covered, and why

- **Part (b) with an expansion failure that holds no parameter element** — no such input is known: the compiler binds `name` attributes only on parameter and type-parameter elements, and a type-parameter element binds on a union. The include route does not fail: `<include>` bringing `<param name="Circle">` under Diagnose expands without an exception (`…IncludedDocumentationHoldsOnlyParamElement` reports RH8035, no AD0001). The two include tests are the only (b)-side tests; the safety net itself has no falsifying input today and is left to review.
- **Include tests for RH8036, RH8108, RH8203** — same expansion call path (`XmlDocumentationExpander` → `GetDocumentationCommentXml(expandIncludes: true)`, `rg -n "GetDocumentationCommentXml" src/Reihitsu.Analyzer` → one call site, `Core/XmlDocumentationExpander.cs:169`); the include input does not throw there, so one rule shows it.
- **Prefixed tag name for RH8036, RH8108, RH8203** — one guard (`DocumentationAnalysisUtilities.IsUnionWithParameterElement`, `rg -n "IsUnionWithParameterElement" src` → helper and its three callers), and the prefix already holds; one test (RH8035).
- **Mode None** — the rules do not run (SHARED-DOC-008); existing `…WhenDocumentationModeIsNone` tests cover it.
- **LF/CRLF, comment/directive in a gap, code-fix convergence, Fix All, formatter/CLI/precheck classes (FC-004, FC-005, FC-007, FC-008, FC-010–FC-014, FC-019–FC-021, FC-028, FC-029, FC-038–FC-043)** — the four rules only report; no layout, token gap, or fix is involved.
- **RH8030, RH8109–RH8111** — do not call the expansion and do not use SHARED-DOC-019; their `param` tests from round 1 stand.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~AnotherLetterCase|FullyQualifiedName~UpperCaseParam|FullyQualifiedName~MixedCaseParamref|FullyQualifiedName~PrefixedParamElement|FullyQualifiedName~ElementNamedParameter|FullyQualifiedName~IncludedDocumentationHolds"
```
