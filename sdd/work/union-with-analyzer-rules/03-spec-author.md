Role: spec author
Commit: 339c549
Verdict: change doc written

Amendment. It carries out decisions 5, 6, and 7 of `00-lead.md` from the skeptic's findings in `02-skeptic.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Nothing is written for RH5418 or RH2110, and no other output changes: the maintainer's answers to Q1–Q4 are pending.

## Changed requirements

- RH7104-004 — `private` for a type nested in a union — reclassified from *changed* to *clarified* in the change doc. The spec text and example stay as they are: they already describe today's output for the nested `N` and the new output for the top-level `U`.
- RH7110-001 — "the previous member of the same group" becomes "any earlier member of the same group …, whatever members lie between them (SHARED-USING-007)". The rule's summary sentence now says "an earlier member of the same kind", and SHARED-USING-007 is added to *Uses*. A new example, `private _a; public B; internal D`, reports `B` and `D`. It matches the existing test `MembersAreComparedWithTheLatestAccessibilitySeenInTheirGroup`. This is a clarification: no output changes.
- RH2103 — new non-goal: "The case-type list of a union, which is not a primary constructor."
- RH2005 — new non-goal: "Fields of unions, which are structs."
- Change doc `sdd/changes/2026-11/union-analyzer-rules.md`:
  - **Affected list:** RH7311-002 added. RH7104-004 moved to the clarified list.
  - **What:** the RH7104 bullet now names only the top-level union and its `internal` fix. The *Clarifications* list gains three entries: RH7104 nested type, RH7110-001 wording, and the RH2103/RH2005 non-goals.
  - **Example 4:** "before" corrected to "RH7104 on `N` only; the fix writes `private partial class N`". The false paragraph at the old line 150 is replaced: only `U` is new.

No requirement ID was added. `requirement-ids.txt` and `sdd/specs/README.md` are unchanged. No rule page changes.

## Examples for the tests

- RH7110-001: the new "compared with every earlier member" example. The existing test already covers it, so the test implementer may tag that test with RH7110-001 as well instead of adding a duplicate.
- RH7104-004: the union example is unchanged. The nested-`N` half pins existing behavior, and the `U` half is new.

## Output questions

_None._ This amendment adds none. Q1's RH7104 "before" now matches `main` (example 4), as the skeptic asked before Q1 reaches the maintainer. If Q1 has already been sent with the old example 4, the lead should send the correction to the maintainer.

## Internal choices

- RH7110-001 is listed in the change doc's affected list only once, as *changed* (for the union). Its wording fix is explained under *Clarifications*. Alternative: list it under both *changed* and *clarified*.
- An FC-002 example was added to RH7110-001 for "any earlier member" vs. "the previous member". Alternative: change the wording only.

## Follow-up candidates

_None._ The RH7110-001 wording candidate in `02-skeptic.md` is resolved in this run.
