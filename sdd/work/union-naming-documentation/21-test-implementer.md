Role: test implementer
Commit: 3cba685
Verdict: tests written

Architecture-finding refusal (lead decision 8, `00-lead.md`) for audit finding #3 (`20-auditor.md`), with #4 handled as decision 8 states. Not a repair round. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from SHARED-DOC-019 (`sdd/specs/shared/element-documentation.md:1517`): nothing is reported for a union when "a documentation comment of one of its declarations" holds a parameter element, "in every documentation mode in which the rule runs"; no spec change was made, as decision 8 states.

Every test adds `UnionSupport` and sets `CompilerDiagnostics = CompilerDiagnostics.None` (the inputs carry CS0260 or CS0101, and CS1569 under Diagnose), so `AD0001` is still compared. Mode `Diagnose` is the library default; `…InParseMode` tests add `ApplyDocumentationModeParseToTestProject`. In every test the analyzed declaration carries no `partial` modifier and its own comment holds no parameter element; the parameter element is only in the other declaration.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH8035…VerifyNoDiagnosticForUnionWithoutPartialModifierWhosePartialDeclarationInAnotherFileHoldsParamElement` (auditor counterexample: Test0 `public union Shape;` with `<remarks>`, Test1 `public partial union Shape(int, string);` with `<param>`; CS0260) | RH8035-001, SHARED-DOC-019 | `Verify` + second source | passes (refused by the expansion probe only, per `20-auditor.md`; AD0001 with the probe removed — the (b) test #4 asks for) |
| `…InParseMode` | RH8035-001, SHARED-DOC-019 | `Verify` + second source + Parse | **fails**: RH8035 reported at `Test0.cs(2,14)` |
| `RH8035…VerifyNoDiagnosticForUnionDeclaredTwiceWithoutPartialModifierWhoseOtherDeclarationHoldsParamElement` (neither declaration `partial`; CS0101) | RH8035-001, SHARED-DOC-019 | `Verify` + second source | passes (probe; AD0001 without it per `20-auditor.md`) |
| `…InParseMode` | RH8035-001, SHARED-DOC-019 | `Verify` + second source + Parse | **fails**: RH8035 reported at `Test0.cs(2,14)` |
| `RH8036…VerifyNoDiagnosticForPrivateUnionWithoutPartialModifierWhosePartialDeclarationHoldsParamrefElement` (`private union Inner;` with `<remarks>` and `private partial union Inner(int, string);` with `<paramref>`, both in `Outer`) | RH8036-001, SHARED-DOC-019 | `Verify` | passes |
| `…InParseMode` | RH8036-001, SHARED-DOC-019 | `Verify` + Parse | **fails**: RH8036 reported at `Test0.cs(7,19)` (`Inner` without `partial`) |
| `RH8108…VerifyNoDiagnosticForUndocumentedTypeParameterOfUnionWithoutPartialModifierWhosePartialDeclarationInAnotherFileHoldsParamElement` (Test0 `public union Result<T>;` with `<summary>`, no `<typeparam>`; Test1 partial with `<param>`) | RH8108-001, SHARED-DOC-019 | `Verify` + second source | passes |
| `…InParseMode` | RH8108-001, SHARED-DOC-019 | `Verify` + second source + Parse | **fails**: RH8108 reported on `T` at `Test0.cs(4,21)` |
| `RH8203…VerifyNoDiagnosticForInheritdocOnUnionWithoutPartialModifierWhosePartialDeclarationInAnotherFileHoldsParamElement` (Test0 `<inheritdoc/>` on `public union Shape;`; Test1 partial with `<param>`) | RH8203-001, SHARED-DOC-019 | `Verify` + second source | passes |
| `…InParseMode` | RH8203-001, SHARED-DOC-019 | `Verify` + second source + Parse | **fails**: RH8203 reported at `Test0.cs(1,5)` |

Observed on 3cba685 (`scripts/test.sh`): 10 new tests, 5 fail, 5 pass. Every failure is the rule's own report on the declaration without `partial` under Parse — finding #3, exactly as the auditor's enumeration traced it for all four rules; no AD0001, no compiler diagnostic, no report elsewhere (the Test1 declarations, which hold the element themselves, are refused syntactically). The Diagnose tests pass on the head only through `XmlDocumentationExpander.CanExpandDocumentation`; they must keep passing once the declarations are read whatever their modifier, which then refuses them syntactically. They are the falsifying input of SHARED-DOC-019 (b) that finding #4 names only while that probe is the sole thing refusing them; I did not remove the probe myself (production code), the auditor's probe in `20-auditor.md` observed AD0001 for the RH8035 pair.

## Not covered, and why

- **The other side of the dropped `partial` precondition** — a union declared more than once whose declarations hold no parameter element, judged by the rule. SHARED-DOC-001 and SHARED-DOC-005 state they do not cover an element declared in several parts, so no spec example or requirement gives an expected value for which declarations are reported; writing one would take it from the code (FC-035). The judged side of the refusal stays covered by the single-declaration `…ElementNamedParameter` and `…IncludedDocumentationHoldsOnlyParamElement` tests (`16-test-implementer.md`), and the existing partial-union tests whose parts both carry `partial` stay unchanged.
- **CS0101 variant (no declaration `partial`) for RH8036, RH8108, RH8203** — one call site: `rg -n "IsUnjudgedUnion" src` gives the helper and its three callers (RH8035/RH8036 base, RH8108, RH8203); the variant differs from the CS0260 one only in the other declaration's modifier, which the helper never inspects (it inspects the analyzed declaration's). Tested once, for RH8035, in both modes.
- **A merged class part (CS0261)** — not a member of the class; the auditor probed it (`20-auditor.md`, class enumeration).
- **Mode None** — the rules do not run (SHARED-DOC-008); existing `…WhenDocumentationModeIsNone` tests cover it.
- **LF/CRLF, comment/directive in a gap, code-fix convergence, Fix All, formatter/CLI/precheck classes (FC-004, FC-005, FC-007, FC-008, FC-010–FC-014, FC-019–FC-021, FC-028, FC-029, FC-038–FC-043)** — the four rules only report; no layout, token gap, or fix is involved.
- **RH8030, RH8109–RH8111** — do not use SHARED-DOC-019.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~WithoutPartialModifier"
```
