Role: spec author
Commit: 938b76f
Verdict: change doc amended (Status stays Accepted, implementer-stage passages marked); spec written; no open question

Inputs: `05-implementer.md` and `squad/decisions/2026-10-31-phase-tree-round-trip-{accessor-arrow,elastic-trivia,break-residue-reach}.md`, all answered A. My prompt carried no intended fix to ignore.

## Decisions carried in

- **Accessor arrow (A):** FMT-STRUCT-026's normative sentence now allows, besides the parse separators, "the spaces around a created `=>`". A new paragraph names the space: the accessor and get-only-member transforms (FMT-STRUCT-009, FMT-STRUCT-014) write one space on each side of the `=>` they create. It also gives the reason: no spacing rule covers the gap between an accessor keyword, a member name, or an indexer's `]` and that `=>`, so FMT-SPACE-002 keeps the gap and the final output would read `get=> …`. The space lasts until a spacing rule owns that gap; the maintainer approved that rule in advance as a follow-up. `Uses` gains FMT-SPACE-002. No spec example asserted no space there. FMT-STRUCT-009 and FMT-STRUCT-014 show the final output with spaces and are unchanged.
- **Elastic trivia (A):** FMT-STRUCT-026 now states that a token a structural transform creates MUST carry no elastic trivia; its trivia is exactly what the requirement allows. The requirement says it does not decide this for later phases. The Why explains the mechanism with `get=> "";` versus a hand-written `get=>"";`. The field split stays outside FMT-STRUCT-026 as a whole, as before.
- **FMT-BREAK-029 reach (A):** the requirement covers only a line break written into the moved token's leading trivia while the previous token's trailing trivia stays in place. A second paragraph states the reach: where the phase rewrites that trailing trivia, it may remove the whitespace with it, and the final output is the same through FMT-CLEAN-001. Two such rewrites are named: moving a line break out of it when rewrapping a list, and normalizing the gap in front of a token it places, such as `}` behind `M();` or an initializer brace. The example (`void M() {` / `if (true) {`) still holds: the implementer reports that the member-body path keeps the whitespace in front of the open brace.
- **Change doc:** the amendment note names the three decision records. *What*, the decision on separators, the decision on FMT-BREAK-029, and *Alternatives considered* (four rejected or deferred options) carry the changes, each marked *(amended 2026-10-31, implementer stage)*. The status stays `Accepted`, as in round 3, because the amendments carry answers the maintainer gave.

## Changed requirements

- FMT-STRUCT-026 — The phase writes only the separators its text needs — changed (spaces around a created `=>`; no elastic trivia on created tokens; Why)
- FMT-BREAK-029 — Whitespace in front of an inserted line break is left to the cleanup phase — changed (reach stated)

## Examples for the tests

- FMT-STRUCT-026, created `=>`: the test implementer has to rework `AccessorTransformWritesNoSpaceAroundTheCreatedArrow` (named in `05-implementer.md`). Phase output: one space on each side of the created `=>`. Final output: `CompactAccessorsAndGetOnlyPropertyKeepTheirFinalOutput` as it stands.
- FMT-STRUCT-026, elastic trivia: assert on the StructuralTransformPhase output that the tokens created for a converted accessor, a braced `if` body, and a converted expression body carry no trivia with Roslyn's elastic annotation. A hand-written `get=>"";` keeps its zero gaps, while a converted accessor gets `get => "";`.
- FMT-BREAK-029: unchanged. The negative side, `M(); }` keeping or losing its space through the gap normalizer, is outside the requirement and needs no test.

## Whole run

`scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"`: 26/26 passed.

## Open questions for the maintainer

_None._

## Follow-up candidates

These come from the decisions; the lead drafts them for the maintainer.

- A spacing rule for the `=>` of an expression-bodied accessor or member. The maintainer approved it in advance. Once it exists, the spaces named in FMT-STRUCT-026 go.
- A pipeline-wide rule that no phase output carries elastic trivia.
- FMT-BREAK-029's reach to `TokenGapNormalizer` and `LineBreakInitializerRewriter`.
