Role: spec author
Commit: 0f775ef
Verdict: spec written

Repair round for the two nits of `08-spec-checker.md` on SHARED-FIX-003, and a check of the spec text against `squad/decisions/2026-10-19-agents-sentence-and-auto-layout.md`. My prompt carried no intended fix, suspected cause, or hoped-for verdict. It carried constraints, and I followed them: one requirement, the `AGENTS.md` sentence unchanged, no code, no tests, nothing else changed.

Files changed: `sdd/specs/shared/code-fixes.md` only (two lines of SHARED-FIX-003). `sdd/specs/requirement-ids.txt`, `sdd/specs/README.md` and `AGENTS.md` are unchanged.

Check: `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecStructureTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~RequirementCoverageTests"` gives `Passed!  - Failed:     0, Passed:    39, Skipped:     0, Total:    39`.

## Changed requirements

SHARED-FIX-003 — A fix that could break the code is withheld — clarified (wording only; no condition, bound, or example changed).

- **Nit 1.** "The second and third condition are bounded" now reads "The second and third conditions are bounded".
- **Nit 2, one `Why:` for four conditions.** Kept as one requirement, as the issue prescribes. I checked the `Why:` against each condition and bound:
  - compile error, run-time change, and edited content text: covered ("breaks the build, changes behavior, or edits text the user wrote as content");
  - `#if`-excluded code: covered (its own sentence);
  - rename reaching doc-comment names and the `#endregion` comment, and layout not counting: covered;
  - the run-time bounds were not argued: why the new name or accessibility does not count, and why field order counts where it sets the layout. Those are the edge cases `sdd/README.md` says the `Why:` decides. I added one sentence after "in exchange for a warning": "The new name or accessibility is the correction itself, not a change in behavior; a field order that sets a type's layout is behavior, because native code and code that reads the memory directly see the fields in that order."

  The sentence also covers the Auto and Explicit exclusions by the same criterion: where the field order does not set the layout, reordering it is not behavior.
- **Auto decision.** The spec text matches answer 2 ("exclude Auto: only types whose field order actually sets the layout count"):
  - the run-time bullet names a struct "which has sequential layout unless it declares `LayoutKind.Auto` or `LayoutKind.Explicit`", and other types only with `[StructLayout(LayoutKind.Sequential)]`, so an Auto struct and a class without the attribute are outside;
  - the RH7109 example sentence says the fix is offered for an Auto struct and for a class without `StructLayout`, "whose fields the runtime lays out in an order of its own".
  No change was needed.
- **Answer 1 (`AGENTS.md` sentence).** Unchanged, as decided. The relocation and reordering gap (FC-004) stays a follow-up that extends SHARED-FIX-003, drafted for the maintainer's approval.

## Examples for the tests

None new. The examples of SHARED-FIX-003 are unchanged, and the covering tests from `06-test-implementer.md` still apply.

## Open questions to file

_None._ thoenissen/Reihitsu#1126 (RH7302 `#endregion` substring rename) is already filed.

## Decisions needed

_None._
