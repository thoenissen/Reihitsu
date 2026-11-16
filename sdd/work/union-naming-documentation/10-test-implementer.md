Role: test implementer
Commit: d90f54e
Verdict: tests written

Repair round 1, audit finding #1 (`08-auditor.md`), against SHARED-DOC-019 and the clarified RH8035-001, RH8036-001, RH8108-001, RH8203-001 (`09-spec-author.md`, 85c14f1). The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the SHARED-DOC-019 examples and its normative sentences, and from the rules' -001 requirements for the other side of the condition.

Every new test adds `UnionSupport` and sets `CompilerDiagnostics = CompilerDiagnostics.None`, so the compiler's CS1569 for a `param` on a union does not mask the analyzer result (as the auditor's counterexamples did). `AD0001` is still compared, because the testing library reports analyzer exceptions as analyzer diagnostics. Mode `Diagnose` is the test library's default; `Parse` tests add `ApplyDocumentationModeParseToTestProject`.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH8035…VerifyNoDiagnosticForUnionWhoseDocumentationHoldsParamElement` (spec example, Diagnose) | RH8035-001, SHARED-DOC-019; FC-033 | `Verify` | fails: AD0001 (NullReferenceException) |
| `RH8035…VerifyNoDiagnosticForUnionWhoseDocumentationHoldsParamElementInParseMode` | RH8035-001, SHARED-DOC-019 | `Verify` + Parse | fails: RH8035 reported on `Shape` |
| `RH8035…VerifyNoDiagnosticForUnionWhoseDocumentationHoldsNestedParamrefElement` (`<remarks><para><paramref/></para></remarks>`, depth) | RH8035-001, SHARED-DOC-019 | `Verify` | fails: AD0001 |
| `RH8035…VerifyNoDiagnosticForUnionWhoseDocumentationHoldsParamElementWithoutName` (`<param>` without `name`) | RH8035-001, SHARED-DOC-019 | `Verify` | fails: RH8035 reported (no crash without `name`; the spec refuses it anyway, 09 internal choice 3) |
| `RH8035…VerifyNoDiagnosticForUnionWhoseLaterDocumentationCommentHoldsParamElement` (SHARED-DOC-006 later comment) | RH8035-001, SHARED-DOC-019 | `Verify` | fails: AD0001 |
| `RH8035…VerifyDiagnosticForUnionWhoseDocumentationHoldsNoParameterElement` (other side: `<remarks>`, `<typeparamref>`, `<typeparam>`, no `<summary>`, Diagnose, expander reached) | RH8035-001, SHARED-DOC-019 | `Verify` | passes (must stay) |
| `RH8035…VerifyDiagnosticForUndocumentedUnionInBodyOfUnionWhoseDocumentationHoldsParamElement` (nested union judged on its own) | RH8035-001, SHARED-DOC-019 | `Verify` | fails: `Inner` reported as expected, plus AD0001 on the outer union |
| `RH8036…VerifyNoDiagnosticForPrivateUnionWhoseDocumentationHoldsParamrefElement` (spec example, Diagnose) | RH8036-001, SHARED-DOC-019 | `Verify` | fails: AD0001 |
| `RH8036…VerifyNoDiagnosticForPrivateUnionWhoseDocumentationHoldsParamrefElementInParseMode` | RH8036-001, SHARED-DOC-019 | `Verify` + Parse | fails: RH8036 reported on `Inner` |
| `RH8036…VerifyNoDiagnosticForPrivateUnionWhoseDocumentationHoldsParamElement` | RH8036-001, SHARED-DOC-019 | `Verify` | fails: AD0001 |
| `RH8036…VerifyDiagnosticForPrivateUnionWhoseDocumentationHoldsNoParameterElement` (other side: `<remarks>` with `<see cref>`) | RH8036-001, SHARED-DOC-019 | `Verify` | passes (must stay) |
| `RH8036…VerifyDiagnosticForUndocumentedUnionInBodyOfUnionWhoseDocumentationHoldsParamElement` | RH8036-001, SHARED-DOC-019 | `Verify` | passes (must stay) |
| `RH8108…VerifyNoDiagnosticForUnionWhoseDocumentationHoldsParamElement` (spec example, Diagnose) | RH8108-001, SHARED-DOC-019 | `Verify` | fails: AD0001 |
| `RH8108…VerifyNoDiagnosticForUnionWhoseDocumentationHoldsParamElementInParseMode` | RH8108-001, SHARED-DOC-019 | `Verify` + Parse | fails: RH8108 reported on `T` |
| `RH8108…VerifyNoDiagnosticForUnionWhoseSummaryHoldsParamrefElement` (`<paramref>` nested in summary, `<typeparam>` for one of two) | RH8108-001, SHARED-DOC-019 | `Verify` | fails: AD0001 |
| `RH8108…VerifyDiagnosticForMethodInBodyOfUnionWhoseDocumentationHoldsParamElement` (spec example: `TItem` reported, `T` not) | RH8108-001, SHARED-DOC-019 | `Verify` | fails: `TItem` reported as expected, plus AD0001 |
| `RH8203…VerifyNoDiagnosticForInheritdocOnUnionWhoseDocumentationHoldsParameterElement` (spec example, both unions, Diagnose) | RH8203-001, SHARED-DOC-019 | `Verify` | fails: two AD0001 |
| `RH8203…VerifyNoDiagnosticForInheritdocOnUnionWhoseDocumentationHoldsParameterElementInParseMode` | RH8203-001, SHARED-DOC-019 | `Verify` + Parse | fails: RH8203 reported twice |
| `RH8203…VerifyNoDiagnosticForInheritdocOnUnionWhoseLaterDocumentationCommentHoldsParamElement` (private nested union with body; `<param>` in a later comment) | RH8203-001, SHARED-DOC-019 | `Verify` | fails: AD0001 |
| `RH8203…VerifyDiagnosticForInheritdocOnUnionWithoutParameterElementAndInBodyOfRefusedUnion` (other side: `<remarks>` with `<typeparamref>`; and a nested union in the body of a refused union) | RH8203-001, SHARED-DOC-019 | `Verify` | fails: both expected RH8203 reported, plus AD0001 on `Holder` |
| `RH8030…VerifyDiagnosticForUnionWithEmptySummaryWhoseDocumentationHoldsParamElement` | RH8030-001 | `Verify` | passes (must stay: RH8030 does not use SHARED-DOC-019) |
| `RH8109…VerifyDiagnosticsForUnionTypeParamElementsInWrongOrderWhoseDocumentationHoldsParamElement` | RH8109-001 | `Verify` | passes (must stay) |
| `RH8110…VerifyDiagnosticForUnionTypeParamElementWithoutNameWhoseDocumentationHoldsParamElement` | RH8110-001 | `Verify` | passes (must stay) |
| `RH8111…VerifyDiagnosticForUnionTypeParamElementWithoutTextWhoseDocumentationHoldsParamElement` | RH8111-001 | `Verify` | passes (must stay) |

Observed on d90f54e (Release, `scripts/test.sh`): 24 new tests, 17 fail, 7 pass, each as the last column says. Every failure is either an `AD0001` NullReferenceException from the rule under test (Diagnose) or the rule's own report on the refused union (Parse, or `<param>` without `name`); no compiler diagnostic and no unexpected report elsewhere.

## Not covered, and why

- **`<include>` bringing in a `param` element** — outside SHARED-DOC-019's coverage, so no test is committed. Probed once (throwaway test, removed): a union whose `<include>` brings in `<inheritdoc/>` and `<param name="x">` under Diagnose gives RH8203 on `Shape` and no AD0001. So the include route does not crash; it is no new member of the class (answers the open point in `09-spec-author.md`).
- **Partial union** — not covered by SHARED-DOC-019.
- **Upper-case or prefixed tag names (`<Param>`)** — not covered by SHARED-DOC-019.
- **Mode None** — the rules do not run (SHARED-DOC-008); the existing `…WhenDocumentationModeIsNone` tests cover it.
- **RH8101–RH8105** — do not check unions (`ParameterOwnerKinds`); unchanged by this repair, characterized by the round-0 tests.
- **LF/CRLF, comment/directive in a gap, code-fix convergence, Fix All (FC-004, FC-005, FC-019–FC-021, FC-007, FC-008)** — the four rules report only; no layout, token gap, or fix is involved.
- **Formatter, CLI, precheck failure classes (FC-010–FC-014, FC-028, FC-029, FC-038–FC-043)** — not involved.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~ParamElement|FullyQualifiedName~ParamrefElement|FullyQualifiedName~ParameterElement"
```

The filter also matches 26 pre-existing tests (RH8101–RH8104 and others with `ParamElement` in the name), which pass.

## Addendum — partial unions (lead decision 5, SHARED-DOC-019 as widened in 97c083f)

Commit: 97c083f (head when run). Implementer's note in `11-implementer.md` (partial unions, guard reads only the analyzed part) taken as the place to look, not as an expected result. Every test: default documentation mode (`Diagnose`), `UnionSupport`, `CompilerDiagnostics.None`; both parts analyzed. Two of them put the second part in a second document (`TestState.Sources`, `/0/Test1.cs`).

| Test | Requirement / failure class | Helper | Observed on 97c083f |
|---|---|---|---|
| `RH8035…VerifyNoDiagnosticForPartialUnionWhoseOtherPartHoldsParamElement` (spec example: `<inheritdoc/>` part, `<param>` part) | RH8035-001, SHARED-DOC-019 | `Verify` | passes (the `<inheritdoc/>` part documents the union and the expander is not reached) |
| `RH8035…VerifyNoDiagnosticForPartialUnionWhoseFirstPartHoldsParamrefElement` (`<paramref>` in the first part, `<remarks>` only in the second) | RH8035-001, SHARED-DOC-019 | `Verify` | fails: AD0001 (NullReferenceException) from RH8035 |
| `RH8035…VerifyNoDiagnosticForPartialUnionWhosePartInAnotherFileHoldsParamElement` (`<remarks>` part in Test0, `<param>` part in Test1) | RH8035-001, SHARED-DOC-019 | `Verify` + second source | fails: RH8035 reported on `Shape` in Test0 (2,22) |
| `RH8036…VerifyNoDiagnosticForPrivatePartialUnionWhoseOtherPartHoldsParamrefElement` (both parts in `partial class Outer`, `<paramref>` in the second) | RH8036-001, SHARED-DOC-019 | `Verify` | fails: RH8036 reported on the first part `Inner` (7,27) |
| `RH8108…VerifyNoDiagnosticForPartialUnionWhoseOtherPartHoldsParamElement` (`<summary>` part, `<param>` part, no `<typeparam>`) | RH8108-001, SHARED-DOC-019 | `Verify` | fails: RH8108 reported on `T` of the second part (4,29) |
| `RH8203…VerifyNoDiagnosticForInheritdocOnPartialUnionWhoseOtherPartHoldsParamElement` (spec example) | RH8203-001, SHARED-DOC-019 | `Verify` | fails: RH8203 reported on the first part's `<inheritdoc/>` (1,5) |
| `RH8203…VerifyNoDiagnosticForInheritdocOnPartialUnionWhosePartInAnotherFileHoldsParamrefElement` (`<inheritdoc/>` in Test0, `<paramref>` in Test1) | RH8203-001, SHARED-DOC-019 | `Verify` + second source | fails: RH8203 reported in Test0 (1,5) |

Observed: 7 tests, 6 fail, 1 passes. In five failures, the part without the element is still judged and reported. In the sixth (RH8035, first part holds `<paramref>`), analyzing the `<remarks>`-only part still crashes: the compiler joins every part's documentation for the union symbol. No compiler diagnostic and no unexpected report elsewhere.

Not covered: a partial union with no `param`/`paramref` element in any part. SHARED-DOC-005 and the RH8203 coverage sentence still leave partial types out of scope, so a test there would assert behavior no spec states.

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~PartialUnion"
```

The filter also matches the existing passing `RH4012…FixRenamesEveryPartOfAPartialUnion`.
