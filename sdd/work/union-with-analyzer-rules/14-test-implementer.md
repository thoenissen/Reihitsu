Role: test implementer
Commit: 8da4e05 (tests written on this head; the commit carrying this handover follows it)
Verdict: tests written

Repair round 2 for auditor finding #1 (13-auditor), lead decision 17. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the examples and requirements of `sdd/specs/analyzer/RH5505.md`, `RH5507.md`, and `RH5521.md` (RH55xx-001 example, RH55xx-003 fix), never from the patched code. No production code or documentation was touched.

## Tests

All tests are `[DataRow]`s over LF and CRLF with `ConvertEndOfLine`, on `Verify(source, fixedSource)` of the existing batch-base test classes; each test with two diagnostics also runs the framework's Fix All passes.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5505ClassAttributesMustFollowPlacementRulesAnalyzerTests.SpecificationExampleIsReportedAndFixed` | RH5505-001, RH5505-002, RH5505-003 | Verify, LF+CRLF | pass (NRE without the guard) |
| `RH5505ClassAttributesMustFollowPlacementRulesAnalyzerTests.ClassDeclaredWithSemicolonAfterOtherCodeIsReportedAndMoved` | RH5505-001, RH5505-003 | Verify, LF+CRLF | pass (NRE without the guard) |
| `RH5507StructAttributesMustFollowPlacementRulesAnalyzerTests.StructSpecificationExampleIsReportedAndFixed` | RH5507-001, RH5507-002, RH5507-003 | Verify, LF+CRLF | pass (NRE without the guard) |
| `RH5507StructAttributesMustFollowPlacementRulesAnalyzerTests.StructDeclaredWithSemicolonAfterOtherCodeIsReportedAndMoved` | RH5507-001, RH5507-003 | Verify, LF+CRLF | pass (NRE without the guard) |
| `RH5521InterfaceAttributesMustFollowPlacementRulesAnalyzerTests.InterfaceDeclaredWithSemicolonAfterOtherCodeIsReportedAndMoved` | RH5521-001, RH5521-003 | Verify, LF+CRLF | pass (NRE without the guard) |

The two `SpecificationExample` tests are the RH5505-001 and RH5507-001 examples verbatim (plus the attribute class declarations they need), with the fix of RH55xx-003 applied: the second list of each (`[A] record R;`, `[A] record struct R;`) belongs to a type declared with a semicolon behind other code, the auditor's crash shape, so the spec examples themselves crashed on `main`. The other three are the auditor's fixtures for class, struct, and interface after `using System;`. Together they cover the class members the finding enumerates: class, record, struct, record struct, interface (the union member is the existing `UnionAttributeListOnDeclarationLineIsReportedAndMoved`).

**Watched fail for the right reason.** On head 8da4e05, all 38 tests of the three classes pass. With the guard `braces is { Open.RawKind: (int)SyntaxKind.None }` in `TargetAttributePlacementCodeFixProviderBase.HasUnusableEnclosingScope` disabled locally (restored with `git checkout`, never committed), 12 tests fail, each with `System.NullReferenceException`: the 5 new tests under LF and CRLF, and the existing union test. All other tests of the classes stay green.

## Scenarios (bug fix)

Not applicable; this is a missing-test repair of a fix already in the branch.

## Not covered, and why

- RH5521 spec example (RH5521-001): its interfaces have braces, and the existing `VerifyDiagnosticAndCodeFixForPolicyViolation` already covers a braced interface; only the semicolon interface reaches the guard, and it has its own test.
- `record class` declared with a semicolon behind other code: covered as `Q` in the RH5505 spec example only on the not-reported side; `record` and `record class` both parse to `RecordDeclarationSyntax` and reach the same `TypeDeclarationSyntax` arm of the brace switch (`rg -n "TypeDeclarationSyntax typeDeclaration =>" src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributePlacementCodeFixProviderBase.cs`), so the reported `record R` covers the arm.
- Comment, directive, missing-brace, and off-column cases (RH55xx-004): the guard does not change them; the existing withheld-fix tests remain.
- The other failure classes: the repair adds tests only, for one guard already reviewed under *Guard scope* in 13-auditor.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5505ClassAttributesMustFollowPlacementRulesAnalyzerTests|FullyQualifiedName~RH5507StructAttributesMustFollowPlacementRulesAnalyzerTests|FullyQualifiedName~RH5521InterfaceAttributesMustFollowPlacementRulesAnalyzerTests"
```
