Role: spec author
Commit: 50e11a9
Verdict: spec written

This repair round addresses Finding 1 of `06-spec-checker.md` and its observation on RH4122-003. My prompt carried no intended fix or hoped-for verdict. No test file and no requirement outside this run's new specs changed.

## Changed requirements

- **RH4121-004 — The fix is withheld when no usable name can be derived — clarified, wording only.**
  - **What the checker found:** the first condition, "the old name has no letter or digit", is the first condition of SHARED-CASING-008. That condition depends only on the old name, not on the SHARED-CASING-006 derivation, so it applies to RH4121's own derivation too. RH4121-004 restated it.
  - **What I did:** the requirement now references it ("when SHARED-CASING-008 withholds it because the old name has no letter or digit, such as `_`"), and SHARED-CASING-008 is added to the `Uses` row of `sdd/specs/analyzer/RH4121.md`.
  - **What stays RH4121's own:** the second condition, a derived body that is neither PascalCase nor digits only, such as `T1Key`. It is not SHARED-CASING-008's second clause, because that clause is tied to the SHARED-CASING-006 derivation of the whole name.
  - **Behavior:** unchanged; the example is unchanged.
- **RH4122-003 — Record, local function, lambda, and anonymous method parameters are not checked — clarified.**
  - **What changed:** the list of record forms now includes `readonly record struct`.
  - **Why it is in scope:** it is this run's own spec, and the list as written read as exhaustive while leaving out a form that is exempt today.
  - **Verified with the probe:**
    - `public readonly record struct P(int X, int y);` gives no RH4122 diagnostic;
    - `public record struct Q(int a);` gives none either;
    - `public struct S(int b);` reports `b`.
  - **Example:** unchanged, so it still matches its covering test.

## Coverage

No requirement ID gained or lost a covering test.

- **RH4121-004:** its existing tagged test still covers it. SHARED-CASING-008 is already covered by the tests of the other casing rules.
- **RH4122-003:** its existing tagged test still covers the requirement. The new `readonly record struct` qualifier has no test of its own. The test implementer may add one line to that test. It is not needed for `RequirementCoverageTests`.

`scripts/test.sh --project architecture --configuration Debug --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage"`: passed.

## Examples for the tests

None new.

## Open questions to file

_None._ The eleven questions of `03-spec-author.md` are filed, as `06-spec-checker.md` records.
