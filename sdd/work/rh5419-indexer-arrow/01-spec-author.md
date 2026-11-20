Role: spec author
Commit: aee6820
Verdict: change doc written

Inputs: thoenissen/Reihitsu#851 (no comments, label `agent-filed`), `00-lead.md` (rule ID RH5419). The prompt carried no intended fix or verdict.

## Files

- Change doc (Draft): `sdd/changes/2026-11/rh5419-indexer-arrow-single-line.md` — six visible output changes, each with input, before, after.
- New spec: `sdd/specs/analyzer/RH5419.md`; index row in `sdd/specs/README.md`; ledger `RH5419-001` to `RH5419-004` in `sdd/specs/requirement-ids.txt`.
- Pointer updates only (no behavior change): RH5401 non-goal now names RH5419 for indexers; FMT-BREAK-026 `Why:` names RH5419 for indexers; SHARED-GETONLY non-goal names RH5419.
- Rule page `documentation/rules/RH5419.md` (with BOM, like its neighbors); package README row in `src/Reihitsu.Analyzer.Package/README.MD` (Analyzer ✔, Code Fix ✔, Formatter ✔).
- Code owners named in the spec (do not exist yet; the implementer creates them under these names): `RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzer`, `RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedCodeFixProvider`. Header: Layout, Warning, enabled, Fix All.
- Resource texts (analyzer title/message, code-fix title) are not written by this role; they belong to the implementer.

## Changed requirements

- RH5419-001 — An arrow or expression below the closing bracket is reported — added
- RH5419-002 — The diagnostic is on the indexer — added
- RH5419-003 — The fix puts the arrow and the expression on the line of the closing bracket — added
- RH5419-004 — The fix is withheld when the expression stays on several lines — added

## Examples for the tests

RH5419-001, RH5419-002, RH5419-003, RH5419-004. The rule page's Violation/Correction pair is checked by `RuleDocumentationExampleTests` once the fix exists.

## How the examples were checked

- The rule does not exist yet, so `apply-fix.sh` cannot run. Every fix result in the spec and the change doc was produced by `scripts/format.sh` on a fixture holding the same input; the fix formats the indexer as the formatter does in place (RH5419-003), so its result is the formatter's result for the indexer:
  - `int this[int i]\n    => i;` and `int this[long i] =>\n    1;` → one line; `int this[\n    uint a,\n    uint b]\n    => 1;` → `int this[uint a,\n         uint b] => 1;`; `int this[short a,\n         short b]\n    => a;` → `int this[short a,\n         short b] => a;`; `[Obsolete] int this[int i]\n    => i;` → `[Obsolete]\nint this[int i] => i;`; `[Obsolete, CLSCompliant(false)]` → split into two lists.
  - `int this[int i] =>\n    i\n    + 1;` → `int this[int i] => i\n                   + 1;` (expression stays on two lines: fix withheld under RH5419-004).
  - `[Obsolete]\n// note\nint this[byte i]` → formatter inserts a blank line behind `[Obsolete]` (gap-count condition of RH5401-004 applies).
  - `[Obsolete,\n\n CLSCompliant(false)]` → formatter drops the blank line (between-brackets condition of RH5401-004 applies).
  - `int this[long i] // note\n    => 1;` and `int this[short i] => // note\n    1;` → CLI reports unsupported syntax (FMT-UNSUP-013 and FMT-UNSUP-012) and leaves the file unchanged.
  - The formatter's indexer join is `PropertyLayoutLineBreakRewriter.VisitIndexerDeclaration`, which joins onto the line of the token in front of the `=>` — always the parameter list's `]`.
- `scripts/test.sh --project architecture`: 195 passed, 3 failed, all three expected until the rule is built and tested: `AnalyzerSpecHeaderMatchesItsRule` (no RH5419 rule yet), `EveryCodeOwnerNamesAnExistingType` (the two RH5419 types), `EveryRequirementOfAnEnforcedSpecIsCovered` (RH5419-001 to RH5419-004 untested). The `Uses` check and the ledger sort pass.

## Output questions

**Q1 · RH5419 · Expression style get-only indexers should be single lined — parameter list on several lines**

The issue proposes the fix only "when the whole declaration fits on one line". The spec is written on reading A.

Input:

```csharp
class C
{
    int this[short a,
             short b]
        => a;
}
```

Before (`main`): not reported.

After, A (recommended) — reported; the fix is offered and gives:

```csharp
class C
{
    int this[short a,
             short b] => a;
}
```

After, B — reported; no fix offered; the code stays as written.

Why A: the formatter keeps the parameter list as written and clears the diagnostic; only a wrapped expression is a shape the fix should not decide for the author (as for RH5401). Requirement touched: RH5419-004 (its sentence "A parameter list that the formatter keeps on several lines does not count"); under B, the condition becomes "the indexer's own code, from its first token behind its attribute lists through its `;`, spans several lines", and example 2 of the change doc changes accordingly.

Every other listed visible output change (change doc items 1, 3 to 6) is the new rule itself, which the issue asks for; the lead asks it as the output questions it decides.

## Internal choices

- RH5419-003 and RH5419-004 reference RH5401-003 and RH5401-004 for the attribute-list layout and the attribute-list refusals ("with the indexer in the property's place") instead of restating them. Alternative: move RH5401-003/-004 into a new shared spec for both rules, which retires two RH5401 requirements and re-tags RH5401's tests with no behavior change.
- RH5401's `Uses`/requirements unchanged; only its non-goal pointer names RH5419.
- Rule page leaves the withheld-fix cases out (create-rule-doc: no withheld fixes on a rule page); RH5401's page mentions its withheld case, RH5419's does not.
- Rule page example uses `=> index * 2;` rather than the issue's `_values[index]`, so the example needs no field.

## Follow-up candidates

_None._
