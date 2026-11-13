Role: spec author
Commit: 8bb338a
Verdict: spec written

Repair round 1 for the findings of `08-spec-checker.md`. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements
None. Only the `Uses` header rows changed, re-derived from the references outside each spec's header and non-goals:

- `sdd/specs/analyzer/RH4001.md` — `Uses`: `SHARED-DIAG-002` (RH4001-002 and its example). Finding 1.
- `sdd/specs/analyzer/RH4009.md` — `Uses`: `RH0001-003, SHARED-DIAG-002` (RH4009-001, RH4009-002). RH0001-005 is referenced only in a non-goal, so it is not listed. Finding 1.
- `sdd/specs/analyzer/RH2003.md` — `Uses`: `SHARED-DIAG-002` (RH2003-001). Finding 2.
- `sdd/specs/analyzer/RH2109.md` — `Uses`: `SHARED-DIAG-003` (RH2109-001). Finding 3.

Finding 4 is accepted by the lead (decisions 7 and 8); no action.

`scripts/test.sh --project architecture`: `Passed!  - Failed:     0, Passed:   117, Skipped:     0, Total:   117`.

## Examples for the tests
None new.

## Output questions
_None._

## Internal choices
- The `sdd/README.md` sentence in *Analyzer specs* says `shared/diagnostics.md` and `shared/code-fixes.md` apply "without being listed under `Uses`", but `SpecStructureTests.UsesListsExactlyTheReferencedRequirements` requires every referenced SHARED-DIAG ID to be listed, and other specs (for example RH3106) already list them. I followed the test; the sentence reads as "need not be listed when not referenced". Left the rulebook text unchanged; alternative: reword it in this run (docs-only, outside the issues).

## Follow-up candidates
- Reword the `sdd/README.md` sentence above so it says the two shared specs apply without being listed, but an ID of them that a spec references is listed under `Uses` like any other.
