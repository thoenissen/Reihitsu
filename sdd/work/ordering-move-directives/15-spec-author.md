Role: spec author
Commit: eab9a97
Verdict: spec written

Repair round 1, finding #1 of `14-auditor.md` (#1668). The prompt pointed at the auditor's suggested change; it was treated as a suggestion and the boundary re-derived from answer 1668-2 A ("any `#if` block in the class in front of the moved fields") and the *Why* of SHARED-ORDER-008. Nothing was built or run (another role works in the tree); the examples were not run through `scripts/apply-fix.sh`.

## Derivation

- The layout clause exists for instance fields whose order changes. Every such field lies in the moved or the crossed span (a member in code that no block is moved across keeps its order against every other).
- A group in front of such a field, outside the spans, can close the type and open a struct whose fields it and the members behind it become, so it must count. A group behind the last instance field of the spans cannot change the type or the order of any reordered field, and every instance field behind it keeps its order, so it need not count.
- The anchor is therefore the last member of the spans that counts as an instance field of the layout (SHARED-ORDER-007), not the first position of the spans (HEAD, too narrow for RH7309's several stretches) and not the last position of the spans (auditor's suggestion, which also refuses a group behind every moved field — the answer keeps that case, since it is not "in front of the moved fields").
- For a member move the spans are one stretch, so every position between the start of the crossed span and that field lies in the spans and refuses already: the new anchor gives exactly today's result for RH7102, RH7103, RH7109. Only RH7309 changes.
- For the implementer: the place is a conditional directive outside the spans, in the body in front of the last member in the spans that counts as an instance field of the layout — a cheap positional check; the private `ChangesMemberOrder` summary ("in front of the first such position") follows it.

## Changed requirements

- SHARED-ORDER-008 — changed: first paragraph's last sentence; third item renamed *In the reordered body, in front of the moved fields* and anchored at the last instance field of the spans, with an `A B C D` → `B A D C` example on both sides of the qualifier; the "not read" list; two *Why* sentences (why a group behind that field is harmless, and why the first position of the spans is not enough for RH7309); a pointer example to RH7309-005.
- SHARED-ORDER-007 — clarified: its reference to the third place of SHARED-ORDER-008 uses the new anchor.
- RH7309-005 — changed: the sentence on a pair in code that no block is moved across gains the exception for a pair in front of reordered instance fields; new example (the auditor's counterexample shape, with a `#if DEBUG` group of a whole method) plus the opposite side in prose (fields in the first pair, group behind them: fix offered).
- Change doc `sdd/changes/2026-11/ordering-move-refuses-conditional-groups.md` — the clause repeated in *Affected requirements*, *What*, and *Where the type-layout check reads*; new decision paragraph on the reading of "in front of the moved fields"; new visible output change for RH7309 (whole-member group between two reordered pairs whose second pair reorders instance fields: `main` reorders, now no fix). No requirement ID added; no rule page changes (a withheld fix in a corner case).

## Examples for the tests

- RH7309-005, new example "The same pair withholds the fix when it stands in front of instance fields …": no fix offered. Its prose counterpart (`_o` in front of `ToString`, `_m` in front of `M`, `IDisposable`/`Base` hold only `Dispose`/`Reset`): fix offered, `Methods`, `Object`, `Base`, `IDisposable`.
- The auditor's counterexample (a `#if X` group closing the class and opening `struct S`) is the same position and may serve as the no-fix test as well.

## Output questions

_None._ The new no-fix case is decided by answer 1668-2 A (a group in front of the moved fields withholds an instance-field reorder) and restores `main`'s refusal of the auditor's counterexample; the whole-member variant, which `main` fixed, is listed in the change doc as covered by the same answer. The case kept (a group behind every instance field of the spans) is what the answer's wording keeps.

## Internal choices

- Anchor at the last instance field of the spans rather than the auditor's last position of the spans: the latter refuses a group behind every moved field, which 1668-2 A does not ask for and the *Why* of SHARED-ORDER-008 shows to be harmless. Alternative: the auditor's anchor (one bound, slightly simpler, refuses one more rare class).
- The RH7309-005 example uses a whole-member `#if DEBUG` group rather than the struct-opening group: the rule is positional, and the ordinary shape makes a clearer seed.

## Follow-up candidates

_None._
