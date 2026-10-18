Role: spec author
Commit: 7272d98
Verdict: spec written

This round writes the spec diff that `01-spec-author.md` held back. The maintainer's two answers are in `squad/decisions/2026-10-18-shared-fix-003-own-reported-text.md` and `squad/decisions/2026-10-18-shared-fix-003-rename-not-runtime-change.md`. The 00-lead.md clarifications still hold:

- no change doc;
- no tab exception for region names;
- covering tests pass today;
- only the changes the issue lists.

Files changed:

- `sdd/specs/shared/code-fixes.md`
- `sdd/specs/requirement-ids.txt` (SHARED-FIX-003, -004, -005 inserted after SHARED-FIX-002)
- `AGENTS.md`: the issue's sentence, appended to the *Code-fix coverage* bullet "Prefer no fix over an unsafe one. …"

`sdd/specs/README.md` needs no change: the prefix and the code owners (`—`) are unchanged. `Uses` stays `—`, because the spec names no requirement ID of another spec outside its non-goals.

Checked with `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecStructureTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~RequirementCoverageTests"`: 38 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, which reports SHARED-FIX-003, SHARED-FIX-004 and SHARED-FIX-005 as uncovered. That is expected until the covering tests exist.

## Changed requirements

- SHARED-FIX-001 — One application corrects a diagnostic — clarified. It holds for each rule on its own. The corrected code is everything the rule reports at the location, a whole run of blank lines included. Another rule's fix does not count.
- SHARED-FIX-003 — A fix that could break the code is withheld — added. It has four conditions, scoped by the two decisions:
  - Q1: the fix may change text that a diagnostic of its own rule reports.
  - Q2: the new name or accessibility the fix exists to produce is not a run-time change.
- SHARED-FIX-004 — The fixes of two rules do not undo each other — added.
- SHARED-FIX-005 — An offered fix changes the document — added.
- Non-goals: the no-op class is removed, and the cycle class is now SHARED-FIX-004. "A fix leaves diagnostics of other rules behind" stays a non-goal.

Wording note for the skeptic: decision Q1 says "its own diagnostic reports". I wrote "a diagnostic of the fix's own rule reports". The RH5604 fix rewrites every differing line break of the file, inside documentation comments too, whichever diagnostic it is applied to. The decision's question names that fix as one the scoped answer keeps. A per-diagnostic reading would make it a violation.

## Examples for the tests

Examples in the spec that match today's code, and the existing tests that show it:

- SHARED-FIX-001:
  - RH5028, one diagnostic for a whole run, removed in one application. Behavior of RH5028-003, already tested.
  - RH5023, one diagnostic per surplus line, cleared by one application of each fix. Behavior of RH5023-004, already tested.
  - The RH5023 half of the two-rule example, `{⏎⏎⏎M();` (thoenissen/Reihitsu#1066 Q1 records that RH5023 reports only the second line). I have not run it.
- SHARED-FIX-003:
  - RH4106 `Value` beside `_value`, no fix (`RH4106PrivateFieldCasingAnalyzerTests`, tagged SHARED-CASING-009).
  - RH4106 renaming inside `nameof` (the existing RH4106 test whose fixed code is `nameof(_value)`).
  - RH7103 `_a` and `_b` (`RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests`, tagged SHARED-ORDER-007; same shape, there the class is named `TestClass`).
  - RH8107 removing a single-line `<returns>` element (RH8107-006).
  - The RH5604 sentence (RH5604-003, RH5604-005).
- SHARED-FIX-004: the RH2005 fix followed by the RH4106 fix. Not run as a chain. Each fix alone is tested.
- SHARED-FIX-005: RH4106 `_1st`, no fix (`NoFixIsOfferedWhenTheDerivedNameIsTheOldName`).

Examples that state decided intent the code violates today. They are open bugs, and a test from them fails today:

- SHARED-FIX-001: the RH5022 half of the two-rule example — thoenissen/Reihitsu#1076 (#1066 Q1).
- SHARED-FIX-003: the `#if NEVER` branch that names `Value` — thoenissen/Reihitsu#1073 (#1009 Q4).
- SHARED-FIX-004: the RH5024/RH5032 `#region R` cycle — thoenissen/Reihitsu#1076 (#1065 Q1).
- SHARED-FIX-005: the RH5023 fix at the end of `"class C\n{\n}\n\n"` — thoenissen/Reihitsu#1076 (#1063 Q1).

Known spec-to-spec inconsistency: RH5022-003/-004 say one diagnostic on the first blank line and a fix that deletes that line. The matching requirements of RH5024 to RH5027 say the same. Both contradict the per-rule reading of SHARED-FIX-001 for a run. The maintainer decided this in thoenissen/Reihitsu#1076, which brings those rule specs in line.

## Open questions to file

### RH7302's fix changes an `#endregion` comment its diagnostic does not report

Belongs to the `SDD open questions: sdd/specs/shared/code-fixes.md` issue. thoenissen/Reihitsu#1066 is that issue, and it is closed, so the lead decides whether to reopen it or open a new one. It touches SHARED-FIX-003.

**Input** (`RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzerTests.VerifyCodeFixUpdatesMatchingEndRegionComment`):

```csharp
class C
{
    {|RH7302:#region members|}
    #endregion // members
}
```

**Real output:** the fix rewrites the directive to `#region Members` and the comment to `#endregion // Members`.

**Why it looks wrong:** the diagnostic spans only the `#region` directive. The `// members` comment is text that no RH7302 diagnostic reports, and SHARED-FIX-003 says such text stays unchanged.

The other reading: the matching `#endregion` comment repeats the region name, and updating it keeps RH7301 from reporting a mismatch. Under that reading, it is part of what the fix exists to produce.

Either way, the decision is whether this is a bug against SHARED-FIX-003, or a clarification that a fix may also update the `#endregion` comment of the region it renames.

Follow-up candidate, out of scope: `sdd/specs/analyzer/RH2005.md` keeps a non-goal saying it is open whether the fix should be withheld when references from outside the type stop compiling. The first condition of SHARED-FIX-003 now decides that case.

## Decisions needed

_None._
