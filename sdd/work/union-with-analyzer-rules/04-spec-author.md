Role: spec author
Commit: c471d88
Verdict: spec written

Carries out the maintainer's answers Q1–Q4 (`00-lead.md`, *Maintainer answers*). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements

- RH5418-001 — An empty union is reported from C# 15 on — added (new spec `sdd/specs/analyzer/RH5418.md`; Layout, Warning, enabled, Fix All; uses SHARED-EMPTY-001 to SHARED-EMPTY-004).
- RH2110-001 — A nested union is reported — added (new spec `sdd/specs/analyzer/RH2110.md`; Design, Warning, enabled, no fix; uses SHARED-NESTED-001, SHARED-NESTED-002).
- FMT-STRUCT-017 — clarified: its `Why:` names RH5418 among the rules that report the braced form and drops "no analyzer rule reports the braced union". Normative text and output unchanged.
- RH5404 — clarified: non-goal "Union declarations." removed (it contradicted RH5404-001, which names the union since 01); "RH5412 to RH5416" becomes "RH5412 to RH5416 and RH5418".
- Non-goal pointers to the new rules: RH2101, RH2104, RH2105, RH2106, RH2107, RH2108, RH7002, RH5412 to RH5416, `shared/empty-type-declarations.md`.
- `requirement-ids.txt`: RH2110-001 and RH5418-001 inserted at their sorted positions. `sdd/specs/README.md`: index rows for both.
- Change doc `sdd/changes/2026-11/union-analyzer-rules.md`: affected list gains the new specs and FMT-STRUCT-017 (clarified); *What* gains the two rules; *Clarifications* gains the RH5404 non-goal, FMT-STRUCT-017 `Why:`, and non-goal pointers; visible output changes 11 (RH5418) and 12 (RH2110) added, rule pages renumbered to 13; *Decisions* records Q1–Q4 with the maintainer's answers and the "rules of their own" reasoning; *Alternatives* records Q3 B, Q4 B, and widening RH5413/RH2104 as rejected. Status stays Draft.
- Rule pages: new `documentation/rules/RH5418.md` and `documentation/rules/RH2110.md` (UTF-8 BOM, modeled on RH5416 and RH2104). Package rule list `src/Reihitsu.Analyzer.Package/README.MD` gains both rows (RH5418: analyzer, fix, formatter; RH2110: analyzer only).

## Examples for the tests

- RH5418-001: the example reports `A`, `B`, `F` (nested, one-line `{ }`), not `C`, `D`, `E` (struct); fix output for `A` and `F`, no fix for `B` (comment, SHARED-EMPTY-004); under C# 14 nothing is reported. The test class should derive from the batch base like RH5416's tests (Fix All), and a formatter test like `RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsFormatterTests` fits the sibling pattern.
- RH2110-001: reported `Inner` (in a union), `Choice` and `Result` (in a class), `Value` (in an interface); not reported `Top`, `Free` (top level, namespace), and the nested struct `Entry`.
- The rule pages' Violation/Correction pairs are checked by `RuleDocumentationExampleTests`; the RH5418 correction is the fix output.

## Output questions

_None._ Q3 and Q4 were answered A; the specs follow the shapes asked (C# 15+, fix for RH5418; no fix for RH2110).

## Internal choices

- The one-line empty union `union V(int, string) { }` is reported by both RH5418 and RH5404, as `struct S { }` is by RH5413 and RH5404 today (SHARED-EMPTY-001 already reports a one-line `class B { }`). Stated in change doc change 11. Alternative: exclude the one-line form from RH5418 — rejected, it would diverge from the siblings.
- RH5418-001 keeps the sibling's "Below C# 15 it MUST NOT report" clause, although unions do not exist below C# 15, so the version gate stays uniform with SHARED-EMPTY-001. Alternative: drop the clause; the test implementer may find the C# 14 case untestable without compiler errors and may then tag it as a characterization of the gate only.
- RH2110 examples avoid `partial` unions; SHARED-NESTED-002 already covers each part of a partial type.
- I removed RH5404's stale non-goal "Union declarations." although it predates this amendment, because it contradicts RH5404-001 as amended in 01.
- I added the package README rows (documentation) but not `Release.ruleset` or the `Reihitsu.sln` solution items; both are build files, left to the implementer, as #1784 added them for RH4012.

## Expected failing checks until implementation

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"`: 48 pass, 3 fail only on the new rules — `EveryCodeOwnerNamesAnExistingType` (`RH2110NestedUnionsShouldNotBeUsedAnalyzer`, `RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzer`, `RH5418EmptyUnionsShouldUseSemicolonDeclarationsCodeFixProvider`), `AnalyzerSpecHeaderMatchesItsRule`, and `EveryRequirementOfAnEnforcedSpecIsCovered` (RH2110-001, RH5418-001). The implementer must use these exact type names, the titles "Empty unions should use semicolon declarations" and "Nested unions should not be used", and add resources, `Release.ruleset` entries, and solution items.

## Follow-up candidates

_None._
