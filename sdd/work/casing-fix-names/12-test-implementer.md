Role: test implementer
Commit: d90597e
Verdict: tests written

Repair round 2, for auditor `11-auditor.md` finding 1 (#1434) and finding 2 (#1451, lead decision 3: repair the code to the unchanged SHARED-CASING-006). The requirements are unchanged; every expected value comes from SHARED-CASING-014 bullet 2 / RH4116-001 and from the SHARED-CASING-006 sentence "a letter number … and a combining mark … are kept as written". The prompt carried no intended fix or hoped-for verdict.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH4116 `TupleElementNamesInTheSignatureOfAnOperatorThatImplementsAReferencedAssemblyOperatorAreNotReported` (binary, checked binary, unary operator; tuple in return and parameter type; implicit implementation) | SHARED-CASING-014, RH4116-001 | analyzer `Verify` + `CompiledAssemblyReference` | fail: 8 diagnostics, expected 0 |
| RH4116 `TupleElementNamesInTheSignatureOfAConversionOperatorThatImplementsAReferencedAssemblyOperatorAreNotReported` (implicit conversion to a tuple, explicit conversion from a tuple) | SHARED-CASING-014, RH4116-001 | same | fail: 4, expected 0 |
| RH4116 `TupleElementNamesInTheSignatureOfAnExplicitImplementationOfAReferencedAssemblyOperatorAreNotReported` (`IOps<P>.operator +`, `implicit IOps<P>.operator (…)`) | SHARED-CASING-014, RH4116-001 | same | fail: 4, expected 0 |
| RH4116 `TupleElementNamesInTheSignatureOfACompoundAssignmentOperatorThatImplementsAReferencedAssemblyOperatorAreNotReported` (C# 14 instance `operator +=`, tuple parameter) | SHARED-CASING-014, RH4116-001 | same | fail: 2, expected 0 |
| RH4116 `TupleElementNamesOfAnEventFieldAreNotReportedWhenALaterDeclaratorIsDictated` (dictated declarator second of two, and middle of three) | SHARED-CASING-014, RH4116-001 | same | fail: 4, expected 0 |
| RH4116 `TupleElementNamesInTheSignatureOfAnOperatorThatIsNotDictatedAreReported` (operators/conversions implementing a source interface, implicitly and explicitly, the interface's own declarations, and operators implementing nothing) | SHARED-CASING-014, RH4116-001 | analyzer `Verify` | pass (other side, FC-003) |
| RH4116 `TupleElementNamesOfAnEventFieldAreReportedWhenNoDeclaratorIsDictated` (several declarators, one implementing a source interface; several implementing nothing) | SHARED-CASING-014, RH4116-001 | analyzer `Verify` | pass (other side, FC-003) |
| Core `ToPascalCaseKeepsALetterNumberOrCombiningMarkThatStartsALaterWordAsWritten` (`class_ⅻ`, `order_ⅻx`, `order_ⅰ_line`, `a_`U+0345`b`, `CLASS_ⅿ`) | SHARED-CASING-006 | `Assert.AreEqual` | fail, all 5 rows |
| Core `ToCamelCaseKeepsALetterNumberOrCombiningMarkThatStartsALaterWordAsWritten` | SHARED-CASING-006 | `Assert.AreEqual` | fail, all 3 rows |
| Core `ToUnderlineCamelCaseKeepsALetterNumberOrCombiningMarkThatStartsALaterWordAsWritten` | SHARED-CASING-006 | `Assert.AreEqual` | fail, all 3 rows |
| Core `EveryDerivationKeepsEachCharacterThatIsKeptAsWrittenWhenItStartsALaterWord` — every BMP character of category Nl, Mn, Mc, Pc (other than `_`) as the first character of the second word of `a_<c>b`, through PascalCase, camelCase, and `_camelCase` | SHARED-CASING-006 | `Assert.IsEmpty` over all failures | fail; lists exactly U+0345 and U+2170–U+217F, the auditor's class |
| RH4110 `FixKeepsALetterNumberOrCombiningMarkThatStartsALaterWordAsWritten` (`order_ⅻx`, `a_`U+0345`b` → `Orderⅻx`, `A`U+0345`b`, references retargeted) | SHARED-CASING-006, RH4110-002 | code-fix `Verify` | fail: `OrderⅫx`, `AΙb` (U+0399) |
| RH4105 `FixKeepsALetterNumberOrCombiningMarkThatStartsALaterWordAsWritten` (camelCase fix path, references retargeted) | SHARED-CASING-006, RH4105-003 | code-fix `Verify` | fail: `orderⅫx`, `aΙb` |

Every failing test fails on the expected-versus-observed difference the finding states; no compile error (the verifier shows only RH4116 diagnostics, no CS errors). Whole runs: `CasingUtilitiesTests` 12 failed / 132 passed (the 11 new rows/methods plus the exhaustive test, no existing test); `RH4116TupleElementCasingAnalyzerTests` 5 failed / 12 passed.

## Scenarios (bug fix)
_N/A — repair round for audit findings; the failures above are the reproductions._

## Not covered, and why
- Finding 1, other dictated-member kinds: methods, properties, indexers, events with accessors, the first event declarator, and nested positions are already pinned by the existing RH4116 tests (`…OverrideOfAReferencedAssemblyMember…`, `…ExplicitImplementation…`, `…NestedInTheSignatureOfADictatedMember…`) and by the auditor's probes; constructors, destructors, and the remaining `BaseMethodDeclarationSyntax` kinds cannot override or implement, so they can never be dictated.
- Finding 1, `override` on an event field with several declarators: every declarator must then override, so the "later declarator only" case cannot arise for overrides; the interface-implementation form covers it.
- Finding 2, the first word: SHARED-CASING-006 does not cover a first word that starts with a letter number (left to SHARED-CASING-008), and an identifier cannot start with a combining mark. Nd and Lo have no uppercase mapping and are covered by the existing rows.
- Finding 2, other fix rules (RH4106, RH4107, RH4115, RH4117, RH2001, PascalCase fixes besides RH4110): they all derive through `CasingUtilities.ToPascalCase`/`ToCamelCase`/`ToUnderlineCamelCase` (`rg "CasingUtilities\.To" src`, per the auditor's policy-ownership table); the Core exhaustive test covers all three derivations, and RH4110/RH4105 cover the PascalCase and camelCase fix paths end to end.
- Line endings: no layout is involved (analyzer reporting and identifier text only).
- Fix All: no new co-occurrence; the rename derivation is per name.

## Commands
```shell
scripts/test.sh --project core --filter "FullyQualifiedName~Reihitsu.Core.Test.CasingUtilitiesTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4116TupleElementCasingAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~FixKeepsALetterNumberOrCombiningMarkThatStartsALaterWordAsWritten"
```

## Follow-up candidates
_None._
