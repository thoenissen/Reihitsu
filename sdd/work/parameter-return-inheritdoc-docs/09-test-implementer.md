Role: test implementer
Commit: 2e73c50
Verdict: tests written

This is the repair round for `07-auditor.md`: finding 7, and the test side of finding 6. I re-derived the tests from the specs at ab16ad4, which carry the narrowed coverage from `08-spec-author.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore. No assertion about current behavior changed, and no production code was touched.

## What changed, and how it was established

- **Finding 6 (examples copied verbatim).**
  - `RH8103…VerifySpecificationExampleReportsParameterElementsWithoutName` and `RH8202…VerifySpecificationExampleReportsValueElementsOnPropertiesAndIndexers` lost the empty line before the class's closing `}`. They now equal the RH8103-001 and RH8202-001 blocks at ab16ad4.
  - A mechanical check extracted every `csharp` block of the 14 specs, plus the per-rule subsets of SHARED-DOC-014 to SHARED-DOC-016, converted the markup to `{|#n:…|}`, and searched each test file for it.
  - Result: 34 of 35 blocks are present verbatim. The one that is not is RH8202-004's unmarked block. Its test adds the span markup that the prose describes, as before.
- **Finding 7 (FC-031).** The summaries of `RH8101…VerifyNoDiagnosticForParametersOfExplicitInterfaceImplementation` and `RH8108…VerifyNoDiagnosticForTypeParametersOfExplicitInterfaceImplementation` now call the reported sibling "a public method with the same signature". Before, they said "an implicit implementation".
- **Tags re-checked against the narrowed coverage.**
  - **RH8201-005** (four spaces per enclosing type and block-bodied namespace; comments and attributes at that indentation; directives at column 0).
    - A script computed the brace depth of every member, `///`, `/**`, `//`, attribute, and directive line in the input of all 16 tests tagged RH8201-005.
    - Two inputs place the member at column 8 inside a single type, where the expected column is 4: `VerifyDiagnosticForFieldLikeOverrideEvent` and `VerifyMultipleFieldLikeOverrideEventsAreFixedInOneFixAllIteration`. Their expected output re-indents the member to column 4. That is the RH8201 Q1 (P9) mechanism.
    - Both tests lost the RH8201-005 tag. They keep RH8201-001 (and RH8201-004 on the first). Their fix assertions stay as characterization.
    - The other 14 tests are inside coverage.
  - **RH8202-001** (no `<value>` closed by a containing element's end tag). None of the 20 tagged RH8202 tests has such an input. The two missing-end-tag tests have a `<value>` that no end tag closes at all, so they are inside coverage.
  - **RH8204-002/003** (empty, or closed by its own `</placeholder>`; for 003, no attributes and non-white-space text). All 7 tagged RH8204 tests are inside coverage:
    - `a & b` in the existing malformed-content test and in the spec example is an unescaped `&`, not a reference.
    - `<placeholder/>` is empty.
- **Formatting.** `scripts/format.sh` over the five changed files changed nothing.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH8103…VerifySpecificationExampleReportsParameterElementsWithoutName` | RH8103-001, RH8103-002 (example updated) | `Verify` | pass |
| `RH8202…VerifySpecificationExampleReportsValueElementsOnPropertiesAndIndexers` | RH8202-001, RH8202-002 (example updated) | `Verify` | pass |
| `RH8101…VerifyNoDiagnosticForParametersOfExplicitInterfaceImplementation` | RH8101-001, SHARED-DOC-009, FC-031 (summary only) | `Verify` | pass |
| `RH8108…VerifyNoDiagnosticForTypeParametersOfExplicitInterfaceImplementation` | RH8108-001, SHARED-DOC-009, FC-031 (summary only) | `Verify` | pass |
| `RH8201…VerifyDiagnosticForFieldLikeOverrideEvent` | RH8201-001, RH8201-004; RH8201-005 tag removed (outside coverage) | `Verify(source, fixedSource, …)` | pass |
| `RH8201…VerifyMultipleFieldLikeOverrideEventsAreFixedInOneFixAllIteration` | RH8201-001; RH8201-005 tag removed (outside coverage) | `Verify(source, fixedSource, …)` | pass |

Results at 2e73c50:
- `Reihitsu.Analyzer.Test.Documentation`: 747 of 747 pass.
- `Reihitsu.ArchitectureTests`: 72 of 72 pass, including `RequirementCoverageTests`. RH8201-005 is still tagged on 14 tests.

## Not covered, and why

No new test was added. None of the shapes that `08-spec-author.md` excludes is tested:
- an unclosed `<placeholder>`, or one closed by another element's end tag;
- a white-space-only placeholder, or a placeholder with attributes;
- a `<value>` closed by `</remarks>`;
- for RH8201-005, a member indented other than four spaces per level.

Those inputs are open questions. The auditor's hints are out of scope for this round:
- kind coverage that relies on sibling rules;
- the `*` text rule of `/** */` comments for RH8104;
- the truncated summary of a pre-existing RH8201 test.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Documentation"
scripts/test.sh --project architecture
```
