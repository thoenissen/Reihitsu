Role: spec author
Commit: 5e00f22 (statements derived from it; spec and change doc committed in 5a13874)
Verdict: change doc written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the maintainer's recorded answer (option A), which is binding and which this handover follows.

## Changed requirements

- RH2001-010 — The fix is withheld for a conditional directive in front of the first modifier or behind a property without an initializer — changed (title and text). Any `#if`, `#elif`, `#else`, or `#endif` in the leading trivia of the first modifier of a property that has a modifier, or in the leading trivia of the token behind a property without an initializer, withholds the fix, whatever the group holds. The whole-member test, its "no group inside it" clause, the nesting paragraph, and the excluded-branch parse are gone. The boundary is stated by the token whose leading trivia holds the directive, with an example on each side.
- RH2001-004 — clarified: the coverage sentence now points to the new RH2001-010 wording; "the comments and directives on the lines above it" now reads "the comments and the directives other than `#if`, `#elif`, `#else`, and `#endif` on the lines above it".
- RH2001-009 — clarified: its pointer to RH2001-010 now says the directive withholds the fix as well; its last example (fix kept for a group of whole members above a property with a modifier) is removed and replaced by one sentence pointing to RH2001-010.
- SHARED-FIX-009 — clarified: the "member next to a property that RH2001-010 reads" illustration is removed, the RH2001 sentence names both RH2001-009 and RH2001-010 as reading no excluded code, and the RH2001 example now says the fix reads no excluded branch.
- Change doc: `sdd/changes/2026-11/rh2001-conditional-group-refusal.md` (Status: Draft).
- Rule page `documentation/rules/RH2001.md` unchanged: a withheld fix is covered by `general-notes.md`.

## Examples for the tests

RH2001-010 (all examples). Mapping to existing tests in `tests/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests.cs`. These tests keep the fix on `main` and must now expect no fix offered:

- `VerifyFixKeepsAConditionalGroupInFrontOfTheFirstModifier` (tagged RH2001-009; it is the issue's example)
- `VerifyFixKeepsAGroupOfAWholeMemberInFrontOfTheFirstModifier`
- `VerifyFixKeepsAGroupWhoseExcludedBranchInFrontOfTheActiveElseMemberHoldsAWholeMember`
- `VerifyFixKeepsAGroupInFrontOfTheFirstModifierWhoseExcludedBranchHoldsOnlyAComment`
- `VerifyFixKeepsAGroupOfAWholeMemberBehindAPropertyWithoutAnInitializer`
- `VerifyFixKeepsAGroupOfAWholeMemberInsideTheActiveBranchOfAGroupThatWrapsTheFile`

These tests are new seeds from the spec:

- An `#endif` alone in front of the first modifier whose `#if` holds the active member (`#if !DEBUG` / `private int _trace;` / `#endif`, second example). It is withheld.
- A group in front of an earlier member, with `_other` between the group and the property. It keeps the fix, and the output is in the spec.
- A group that wraps the whole type (`#define NET` / `#if NET` around `class C`), with no directive next to the property. It keeps the fix, and the output is in the spec.

These existing refusal tests and the test that keeps the fix for an initializer stay as they are: `VerifyFixKeepsAConditionalModifierBehindAPropertyWithAnInitializer`, the extern/readonly/static/attribute/text/nested/initializer refusals, and the two Fix All tests.

I ran the examples that keep the fix through `scripts/apply-fix.sh RH2001`: the initializer group, the earlier member, and the whole-type wrap. Each gives today the output the spec states. The issue's example, the `#endif`-only example, and the whole member behind the property are all fixed on `main`, which confirms the visible change. The extern/readonly example and the excluded-initializer example are already withheld on `main`. Architecture tests pass.

## Output questions

_None._ The maintainer settled the run's only output question (option A, issue comment of 2026-11-22). I read "directly in front of / directly behind" as the leading trivia of the first modifier and of the token behind the property. RH2001-010 already reads those two places, so no boundary moves.

## Internal choices

- RH2001-009 stays separate from RH2001-010. The no-modifier case keeps its own ID and reason. The alternative was to merge it into RH2001-010.
- Non-conditional directives (`#region`, `#pragma`, …) in front of the first modifier still keep the fix, as RH2001-004 states. The alternative was to refuse every directive there.
- I dropped my sentence comparing this rule to SHARED-ORDER-008 from the RH2001-010 *Why*. Keeping it would have required a Uses row for an ordering requirement RH2001 does not use. The comparison is in the change doc instead.
- SHARED-FIX-009 keeps its general bullet allowing a fix to read whole units of an excluded branch. Names in excluded code still use it (SHARED-FIX-003's RH4106 example, the RH2001-004 rename). The alternative was to narrow that bullet to names only.
- The second block of RH2001-004's last example (`#if NEVER` / `private int _value;` / `#endif` above the property) is now withheld by RH2001-010 as well as by the rename clause. I left it unchanged because its stated outcome, no fix, still holds. The alternative was to put a member between the group and the property so that the example tests only the rename.

## Follow-up candidates

_None._ Note for the implementer, not a candidate: once `OrderingDeclarationUtilities.ConditionalGroupsWrapWholeMembers` is gone, `ConditionalGroupUnitUtilities.ParseInTypeBody`, `WrapsWholeMembers`, `GetExcludedText`, and `ReadsAsWholeMembers` look unused. `GetConditionalDirectives`, `IsConditionalDirective`, and `IsWithoutBody` still have callers in `OrderingMoveSafety` and `OrderingDeclarationUtilities`. The type's summary names RH2001-010 and needs updating.
