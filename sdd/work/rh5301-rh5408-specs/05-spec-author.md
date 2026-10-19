Role: spec author
Commit: 67b874fed7a8629831820a5f45238c0015b2fb17
Verdict: spec written

This round repairs the spec examples that `04-test-implementer.md` reported as not compiling, and checks the last sentence of SHARED-INIT-002. I re-derived each item myself. No requirement ID was added, retired, or renumbered, and `requirement-ids.txt`, `sdd/specs/README.md`, and the rule pages are unchanged.

## Findings

- **RH5304-002 example (CS1912) — applied.** It initialized `L` four times in one object initializer. The three reported `L` assignments now target `A`, `B`, and `E`, each declared as `List<int>`. The names have the same length as `L`, so all columns are unchanged and the same four assignments are reported.
- **RH5408-001 and RH5408-005 examples (CS0273) — applied.** `int B { get; private set; } = 5;` declares a private property with a `private set`. The property is now `public int B`, so the reported span starts at `public`. The fix result is `public int B { get; private set; } = 5;`.
- **SHARED-INIT-002, last sentence — removed.** The sentence was "A complex element nested inside it MUST be checked in the same way, one level deeper." It describes behavior no source can reach. Roslyn does not parse a complex element initializer as an expression of another complex element: `new List<int[]> { { { 1, 2 } } }` gives CS1513 and CS1003, and the analyzer then sees no nested complex element. The recursive branch in `InitializerLayoutAnalysisUtilities.FindFirstComplexElementMisalignment` is therefore dead code. That is a follow-up candidate outside this lane, not a specified behavior. The rest of SHARED-INIT-002 is unchanged and still covered by the RH5303 and RH5304 complex-element tests.

## Changed requirements

Examples corrected, statements unchanged: RH5304-002, RH5408-001, RH5408-005.

SHARED-INIT-002 is clarified: the sentence about unreachable behavior is removed.

## Examples for the tests

The tests named in `04-test-implementer.md` already use the corrected shapes:

- `VerifyWrappedCollectionInitializerThatIsNotABlockIsReported` uses `A`, `B`, `E`, and `L`.
- `VerifySimpleAutoPropertyOnSeveralLinesIsReported` and `VerifyFixPutsDeclarationOnOneLine` use `public int B`.

The test implementer should still check that these tests match the corrected examples exactly.

## Checks

- All 55 example blocks were re-run with the scratch harness, which uses the real analyzers and providers. Every input and every result block now compiles with no errors; this compile check is new in this round and was confirmed to catch CS0273. Every markup span was reproduced exactly, and every result block matched the single fixes applied one after another.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RequirementCoverageTests"`: 39 passed.

## Open questions to file

Unchanged from `03-spec-author.md`.

## Decisions needed

_None._
