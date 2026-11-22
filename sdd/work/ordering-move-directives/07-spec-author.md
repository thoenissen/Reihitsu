Role: spec author
Commit: e4f5859
Verdict: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore; it asked for the sentence of decision 9, which this handover writes from answer 1668-1 B and the interpretation note in `06-implementer.md`.

## Changed requirements

- SHARED-ORDER-008 — A conditional directive in the moved or the crossed span refuses a member move — clarified: for RH7309 the spans are every region block and every piece of code between two consecutive blocks that a block is moved across (a block in front of it before the reorder and behind it afterwards, or the other way round). A block that keeps its place among the blocks is in the spans when blocks are moved across it (`A B C` → `C B A`: `B`); code that no block is moved across is not (`A B C D` → `B A D C`: the code between `B` and `C`). The *In the reordered body, in front of the spans* item now points to that definition instead of repeating the old wording.
- RH7309-005 — The fix is withheld when a directive would be moved across — clarified to match: the conditional refusal names a kept block that blocks are moved across; the exemption reads "a block, or code between blocks, that no block is moved across"; *Why* says why a crossed kept block is refused. Two examples added (below); the lead-in of the existing "keeps its place" example now says "and that no block is moved across".
- Change doc `sdd/changes/2026-11/ordering-move-refuses-conditional-groups.md` (Accepted): the RH7309 bullet of *Visible output changes* 1 names the crossed kept block and the uncrossed gap. No new decision: answer 1668-1 B (moved or crossed code) covers it, as decision 9 records.

No requirement ID, `Uses` row, or code owner was added or removed.

## Examples for the tests

- RH7309-005, new example 1: blocks `IDisposable`, `Object`, `Methods` (reordered to `Methods`, `Object`, `IDisposable`); `#if DEBUG` group inside `ToString` of the kept, crossed `Object` block — both `#region Object` and `#region Methods` reported, no code fix. This is the case no current test distinguishes (`06-implementer.md`, *Interpretation note*).
- RH7309-005, new example 2: blocks `Object`, `Methods`, `IDisposable`, `Base` in `class C : Base, IDisposable` (reordered to `Methods`, `Object`, `Base`, `IDisposable`); a `#if DEBUG` group of a whole method in the code between `Methods` and `IDisposable` — fix offered, output given. The sentence behind it states the negative twin: the same group between `Object` and `Methods` withholds the fix.

Neither example was run: the lead's prompt forbids building or running tests while the test implementer works in the tree. Both compile by inspection; the categories follow RH7309-001 (`Object` and `Base` base-type, `IDisposable` interface, `Methods` custom) and the order RH7309-004's stable sort. The test implementer should confirm the after-output of example 2 against `scripts/apply-fix.sh RH7309`, in particular the blank lines around the group, which come from the gaps staying in place.

## Output questions

_None._ Decision 9 records the reading as covered by answer 1668-1 B.

## Internal choices

- The definition of the RH7309 spans is stated once in SHARED-ORDER-008, by its cause (a block moved across the part), and RH7309-005 references it rather than restating it; alternative was a full restatement in RH7309-005.
- The rule page `documentation/rules/RH7309.md` is unchanged: a withheld fix in a corner case.

## Follow-up candidates

_None._
