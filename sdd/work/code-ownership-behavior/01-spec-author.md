Role: spec author
Commit: 39d7d96
Verdict: text written

## Changed requirements

None. No spec, requirement ID, test, code, or code-owner row changed. No change doc: intended behavior does not change.

Text changes:

- `sdd/README.md`, *Spec format → Code owners* bullet: after "A type is a code owner of at most one spec: … and a general-purpose utility to none." added the issue's clarification. Wording polished only: the first two of the issue's sentences are joined with a colon, and "each such caller's spec" reads "the spec of each such caller". Meaning unchanged: ownership follows the behavior a type implements, not its callers; callers' specs list the relied-on IDs under `Uses`; a general-purpose utility is a type whose behavior no spec specifies (syntax or trivia helper).
- `sdd/changes/2026-10/introduce-sdd.md` is left untouched. Its code-owner bullet (*Spec format → Rules*) says only "Code owners are type names only, never file paths or line numbers. A type is a code owner of at most one spec." It does not draw the shared-spec / general-purpose-utility distinction that this run clarifies, so it does not state that rule in other words, and the issue's amend clause does not apply. The bullet stays consistent with the clarified rulebook. An earlier commit of this run had amended it; that edit was reverted to `origin/main`, because the issue's clause did not cover it and it pushed the run out of the docs lane.

## Examples for the tests

_None._ Docs lane; no compiled file changed, so `scripts/verify-text-only.sh` does not apply.

## Open questions to file

_None._

## Non-goals

- No existing code-owner row moves (for example `CasingUtilities`, the documentation helpers of #982, `ExpressionBodyRewriteUtilities` of #997); re-assigning them is left to their own runs.
- No change to the spec checker's list in `sdd/README.md` (it already checks "a type is a code owner of at most one spec").
