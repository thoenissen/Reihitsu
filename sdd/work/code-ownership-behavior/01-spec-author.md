Role: spec author
Commit: 39d7d96
Verdict: text written

## Changed requirements

None. No spec, requirement ID, test, code, or code-owner row changed. No change doc: intended behavior does not change.

Text changes:

- `sdd/README.md`, *Spec format → Code owners* bullet: after "A type is a code owner of at most one spec: … and a general-purpose utility to none." added the issue's clarification. Wording polished only: the first two of the issue's sentences are joined with a colon, and "each such caller's spec" reads "the spec of each such caller". Meaning unchanged: ownership follows the behavior a type implements, not its callers; callers' specs list the relied-on IDs under `Uses`; a general-purpose utility is a type whose behavior no spec specifies (syntax or trivia helper).
- `sdd/changes/2026-10/introduce-sdd.md`, *Spec format → Rules*: the code-owner bullet said only "A type is a code owner of at most one spec." Amended in place (the rulebook allows this for the SDD-setup change docs) so it agrees with the clarified rule: the owning spec is the one that specifies the behavior the type implements, not the spec of a caller, and a type whose behavior no spec specifies owns none.

## Examples for the tests

_None._ Docs lane; no compiled file changed, so `scripts/verify-text-only.sh` does not apply.

## Open questions to file

_None._

## Non-goals

- No existing code-owner row moves (for example `CasingUtilities`, the documentation helpers of #982, `ExpressionBodyRewriteUtilities` of #997); re-assigning them is left to their own runs.
- No change to the spec checker's list in `sdd/README.md` (it already checks "a type is a code owner of at most one spec").
