Role: skeptic
Commit: d7445fb
Verdict: proceed narrower

I re-checked only the revision: commit d7445fb against 97c6e64, the per-finding table in `03-spec-author.md`, the new requirements SHARED-TYPEREGION-005 and SHARED-TYPEREGION-006, and the revised question texts. The coordinator's message named the scope only; it carried no intended fix or hoped-for verdict.

Evidence:
- `scripts/apply-fix.sh` (LF and CRLF) and `scripts/format.sh` on scratch fixtures, since deleted. Rules without a code fix are marked *code reading*.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"` on d7445fb: 26 passed.

## Premise

Most findings are resolved:
- **Fully:** A1, A2, A3, A5, A8, A9, A10, A11, and the pairing restatement.
- **Partly:** A4, A6, and A7. In each, the narrowed sentence still covers one input that is an open case.
- **Not as written:** the two new shared requirements. On d7445fb they contradict the code for two inputs that `RegionDirectiveUtilities` decides differently, and the RH7309 fix breaks the code on both (S1, S2).

The spec author has to make one more round of narrowing. Nothing here needs the maintainer before the run can continue.

## Mechanism and candidates

_N/A._ This is not a bug fix. The candidates for S1–S4 come from the dispatch code:
- `GetTopLevelRegions` excludes the spans of nested types through `DescendantNodes().OfType<TypeDeclarationSyntax>()`. An enum is a `BaseTypeDeclarationSyntax` but not a `TypeDeclarationSyntax`, so its span is not excluded.
- `GetTopLevelRegions` keeps every directive for which `IsWithinElementBody` is false, which includes the Q6 places.
- `IsWithinElementBody` decides by the token that owns the directive's trivia: the token after the directive. It does not decide by the directive's position.

## Breaking inputs

### Status of the earlier findings

| Finding | Status | Remaining point |
|---|---|---|
| A1 | Resolved | — |
| A2 | Resolved | — |
| A3 | Resolved | — |
| A4 | Partly | RH7303-004 still covers a region with a blank line on both sides between two statements (S3) |
| A5 (a), (b) | Resolved | Pairs as FMT-REGION-003 does; the first token must start its line; no directive in between may lie outside "between declarations". The filtered pairing of the code equals compiler pairing under that condition |
| A6 | Partly | RH7309-004 still covers a comment above the first block (S4) |
| A7 | Partly | The non-goal is removed. RH7310-004's "belong to a declaration" still covers a type's own `{` and `}` (S5) |
| A8 | Resolved | — |
| A9 | Resolved | — |
| A10 | Resolved | — |
| A11 | Resolved | — |
| B: top-level regions | Not as written | SHARED-TYPEREGION-006 contradicts the code (S1, S2) |
| B: executable code | Partly | SHARED-TYPEREGION-005 is ambiguous for top-level statements (S6) |
| B: pairing | Resolved | — |

### S1 — SHARED-TYPEREGION-006: a region inside a nested enum is a top-level region of the outer type

Input: `public class C : IDisposable` containing `#region IDisposable` … `#endregion // IDisposable`, followed by `enum E { #region Values` / `A,` / `#endregion // Values }`.

Real output: RH7309 reports `#region Values` against the interface region. Its fix swaps the two blocks, moving `A,` into the class body and `Dispose` into the enum. The run gives `invalid-result`.

The requirement's precondition holds here, because no pair crosses a member boundary. It says the enum's pair lies inside a nested type and is not top-level, so the code contradicts it.

The same mechanism, by code reading:
- RH7311 compares the enum's region descriptions with those of the outer type.
- RH7305 and RH7305A count the enum's region.

SHARED-TYPEREGION-006 has to leave declarations with a region inside a nested enum out. The fix part is decided by principle P1 (SHARED-FIX-003, compile error). The analyzer part is a new question (N7).

### S2 — SHARED-TYPEREGION-006 decides Q6 for the top-level-region rules

Input: the same class, with `private int[] _a = new[] { #region Values` / `1,` / `#endregion // Values };` in place of the enum.

Real output: RH7309 reports `#region Values`. Its fix moves `#region Values` / `1,` / `#endregion` out of the initializer, giving `invalid-result`.

SHARED-TYPEREGION-006 says that only pairs between members are top-level, so the code contradicts it. Q6 itself says that it touches SHARED-TYPEREGION-006, so the requirement must also leave out declarations with a region directive that is neither inside executable code nor between declarations. The RH7309 fix part is decided by principle P1. Add it to Q6.

### S3 — RH7303-004 still covers a double blank line

Input: `var x = 1;` / blank / `#region Work` / blank / `x++;` / `#endregion // Work` / `x--;`.

Both directives are reported and lie between two statements, so the narrowed sentence covers the input. The fix leaves `var x = 1;` / blank / blank / `x++;`, and `format.sh` collapses the two blank lines. This is the same P10 question as N6. Either narrow RH7303-004 further, or add the case to N6.

### S4 — RH7309-004 still covers a comment above the first block

Input: `// The disposal logic` directly above `#region IDisposable` (the first block), then `#region Fields`, with no comment between the two blocks.

Real output: the fix moves `Fields` first. The comment stays at the top and now sits above `#region Fields`.

The narrowing ("no comment lies between two top-level region blocks") does not exclude this input. Since "the code before the first block and after the last MUST stay in place", the requirement leaves no gap for it. Either narrow the requirement to declarations with no comment before, between, or after the top-level region blocks, or add this input to N3.

### S5 — RH7310-004: "belong to a declaration" includes the type's own braces

Input: `int _a;` / blank / `#region Methods` / blank / `#endregion // Methods` / `}`. The nearest line below is the class's `}`, which belongs to the class declaration. So the sentence still covers the brace case from N6 (the fix leaves `int _a;` / blank / `}`). It should name a sibling member or declaration, not the declaration whose braces enclose the region.

### S6 — SHARED-TYPEREGION-005 is ambiguous for top-level statements

A directive between top-level statements lies "in a statement" and also "at the top level of a file".

Input: `#region Setup` / `var x = 1;` / `System.Console.WriteLine(x);` / `#endregion`.

Real output: RH7303 reports, and the fix removes both directives (run). By code reading, only `#region Setup` is reported. It is owned by the statement after it. The `#endregion` is owned by the end-of-file token, whose parent is the compilation unit, so it is not reported. The fix therefore removes a directive it does not report, which is decided by principle P1, like Q5.

SHARED-TYPEREGION-005 should leave top-level statements out of both lists. Read as "at the top level of a file", RH7303-002 forbids reporting `#region Setup`, which the code reports. The analyzer part is a gap inside RH7303's own construct (P11). File both parts as N8.

A related input the definition leaves open (*code reading*, not a contradiction): a directive between a member's header and the `{` of its body (`void M()` / `#region X` / `{`) is owned by `{` and counts as executable code.

### Inputs for the test implementer

These come in addition to the list in `02-skeptic.md`:
- S1 and S2 for RH7309 (fix withheld, characterization only), RH7311, and RH7305.
- S6 for RH7303.
- S3 under CRLF as well (FC-033).
- Failure classes FC-027 (another enclosing scope) and FC-024 (nested enum).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers, code fixes, `Reihitsu.Core`, formatter | Must stay (no production change in this lane) |
| `shared/type-regions.md` | Must change: SHARED-TYPEREGION-005 (S6) and SHARED-TYPEREGION-006 (S1, S2) |
| RH7303, RH7309, RH7310 specs | Must change (S3, S4, S5), or the inputs go to N3 and N6 |
| Rule docs | Not involved by this revision; A10 and A11 are correct |
| Tests | Must change, as before |

## Simpler option

For S1 and S2, a single precondition on SHARED-TYPEREGION-006 covers both inputs: every region directive of the declaration lies either inside executable code or between declarations, and none lies inside a nested enum. That leaves the questioned inputs out, and RH7305, RH7305A, RH7308, RH7309, and RH7311 inherit the narrowing.

## Questions for the maintainer

None blocks the run. Corrections to the question texts in `03-spec-author.md`:

- **Q1:** the severity line says "RH7201 and RH7207 report every input". RH7201 reports only Inputs B and D, which contain a regular `System` directive. Say "RH7207 reports every input". Q2 and Q3 inherit the line, and the corrected form holds for them too.
- **Q4, Q5, Q7, Q8, N1, N2, N4, N5:** accurate. The principle markings are right:
  - Q4: #1043 Q1 and P7.
  - Q5: P1.
  - Q7, second part: P7 (SHARED-FIX-005).
  - N1: P12 and P1.
  - N2: P7 and P9.
- **Q6:** accurate and correctly marked P11. Add the P1 part from S2: the RH7309 fix moves a region out of a field initializer and the result does not parse.
- **N3:** add the comment above the first block (S4), unless RH7309-004 is narrowed to exclude it.
- **N6:** add the double blank line (S3) and the brace case that RH7310-004 still covers (S5), unless those sentences are narrowed.

New questions to file:

- **N7 — RH7309, RH7311, RH7305, RH7305A: a region inside a nested enum counts as a region of the outer type** (S1).
  - Analyzer part: an open question. Reading (a): the enum's regions belong to the enum, as SHARED-TYPEREGION-006's Why says for nested types. Reading (b): current.
  - Fix part: RH7309 produces code that does not parse. Decided by principle P1 (#912), SHARED-FIX-003.
- **N8 — RH7303 in a file with top-level statements** (S6).
  - Analyzer part: `#region` before a statement is reported, `#endregion` after the last statement is not. A gap inside the construct the rule names, decided by principle P11 (#912).
  - Fix part: the fix removes the unreported `#endregion`. Decided by principle P1 (#912), SHARED-FIX-003, as in Q5.
