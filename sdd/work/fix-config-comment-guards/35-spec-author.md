Role: spec author
Commit: e8e42798
Verdict: spec written

The prompt named the amendments to make (decisions 33 and 34, batches 3–5); it carried no suspected cause or hoped-for verdict beyond those recorded answers. Batch 6 (recorded in `00-lead.md` while I worked) is applied too.

Measurements: `scripts/apply-fix.sh` on a temporary worktree at `57acf0d` (LF and CRLF the same; worktree and fixtures removed). Fixed on main: attribute group on the moved member (both branches attribute lists), attribute group on a crossed member, `#if DEBUG static #endif` in front of the moved member, whole members in a crossed nested type, whole accessors (`get; #if DEBUG set; #endif`, RH7103 class), switch section, conditional argument in a statement, lambda in a field initializer, RH7309 struct with `field` in an accessor branch, RH7309 struct with `#if DEBUG get; #else get => 0; #endif`, with `get => 0; #if DEBUG set; #endif`, and with `get => 0; #if DEBUG set { } #endif`.

## Changed requirements

- SHARED-FIX-009 — A fix does not work out another build configuration — changed: monotonic wording ("what an excluded branch holds never makes the fix offer a change that it would not offer without that branch"); the withholding bullet now applies only to a fix that reads excluded code; a fix that reads none and leaves every directive in place is governed by SHARED-FIX-003 and SHARED-DIAG-008 alone (keeps RH2001-009); new: a fix decides which directives withhold it from where they stand in the syntax tree, never from their conditions, and a place the test does not classify withholds. `Uses` adds RH2001-009.
- SHARED-ORDER-008 — retitled "Only conditional groups of whole units keep a member move" — changed: one definition, *wraps whole units*, over four unit lists (members of a type body, the reordered type's or a nested type's in the spans; attribute lists of one member; accessors of one accessor list; statements of one block or switch section inside a member's body block, lambdas and local functions in it included). Conditions: place (every directive at a boundary of one list), no nesting, whole units in every branch (excluded text parsed once on its own as that list's content; comment/whitespace/`#region` only branches are zero units), no `field` token in accessor groups or statement groups in an accessor body. The two-list boundary at a member's first token is resolved by the excluded branches (whole members or whole attribute lists). Decision 34 is stated as the governing paragraph: a small structural test of directive places plus one parse per excluded branch (main's parse unchanged for type-body text, per batch 6); no condition evaluated, no branches combined, no configuration parse, no limit; anything unclassified refuses. Examples: kept — whole members in moved leading trivia (heading corrected), crossed nested type (1A), attribute lists on the moved member (2A, with output), whole accessors (3A, with output), switch section (with output); refused — initializer split, modifier split `K`, excluded `static` in front of the member, #1593's `X`/`Y` (wording corrected), nested group, type-closing branch, nested type opened, conditional argument, lambda in a field initializer, RH7309 `field` region example (bug fix), RH7103 `field` example reworded ("although … reorders no instance field in any build").
- SHARED-ORDER-007 — changed: groups in a nested type body are not counted (SHARED-ORDER-001); attribute, accessor, and statement groups declare no member; a property counts as declaring a backing field also when a branch of a group of whole accessors holds an accessor without a body or expression body. New example (RH7309, `#if DEBUG get; #else get => 0; #endif`: no fix) with the counterpart `get => 0; #if DEBUG set { } #endif` (fix offered, as on main).
- SHARED-ORDER-006 — unchanged in this round. Spec header `Uses` adds RH7309-005 (it was referenced already; `UsesListsExactlyTheReferencedRequirements` failed without it).
- RH7102-006 — changed: the constant has no conditional directive outside its leading trivia (RH7102-001); a group in its leading trivia keeps the move only when it wraps whole members or whole attribute lists of the constant.
- RH7103-007, RH7109-006, RH7309-005 — unchanged in this round (they reference SHARED-ORDER-008 by ID).
- Change doc `sdd/changes/2026-11/member-move-conditional-groups.md` — Status: Accepted; retitled; *What* rewritten; *Why* lists the bug-fix withholdings (incl. `field` and conditional bodiless accessor in a layout reorder); *Visible output changes* adds the excluded modifier (RH7109, full before), the conditional argument (RH7103), and the lambda in a field initializer (RH7109), plus a sentence for the remaining classes (expression body, parameter list, constructor initializer, base list, `field` in an accessor group where no build changes); the *Unchanged* paragraph lists nested type, attribute lists, accessors, switch section, comment-only branches. *Decisions*: batch 3 A, batch 4 A, batch 5 1A/2A/3A with the maintainability constraint, batch 6 A, the bodiless-accessor count, comment/region branches, the replaced reading. Affected requirements corrected to RH7103-007 (RH7103-006 was named by mistake).

`scripts/test.sh --project architecture --filter Sdd`: only `EveryRequirementOfAnEnforcedSpecIsCovered` fails (SHARED-FIX-009, SHARED-ORDER-008 have no tagged test yet) — the test implementer's.

No rule page changed: withheld fixes in corner cases.

## Examples for the tests

- SHARED-ORDER-008: every example (kept: moved-trivia whole members, nested type, attribute lists, accessors, switch section; refused: excluded `static`, `K`, `X`/`Y`, nested group, type-closing branch, nested type opened, conditional argument, lambda initializer, RH7309 `field`, RH7103 `field`).
- SHARED-ORDER-007: the new RH7309 conditional-accessor example and its `set { }` counterpart.
- SHARED-FIX-009: its initializer example; RH2001-009's `if_static` shape stays fixed (34-skeptic.md) as the scope counterpart.
- Change doc: the three new withholdings.

## Output questions

_None._ Batches 3–6 decide every visible case; the bodiless-accessor count withholds only where main's fix changes a build's layout (SHARED-FIX-003).

## Internal choices

- **Parse of excluded text inside a member.** Decision 34 says excluded text is read only by main's whole-member parse; batch 6 keeps that parse unchanged. Main's parse never reads disabled text inside a member's own tokens, so without another read an attribute, accessor, or statement group whose excluded branch holds `static`, or closes the method body and declares an initialized member, would be kept and could break that build (SHARED-FIX-003), and the `field` clause the maintainer required cannot be checked at all. I specified the same parse with the text placed into an empty list of the unit's kind, one parse per branch, result checked for whole units and `field`. Alternative: read nothing inside members except a search for `field` — smaller, but leaves those SHARED-FIX-003 holes as on main. Recommendation: the parse as specified; if the lead takes the alternative, drop "for a branch inside a member, …" from SHARED-ORDER-008's last paragraph and the "Whole units in every branch" bullet's scope narrows to type-body branches.
- **Conditional accessor without a body counted, not refused.** Keeping 3A admits `#if DEBUG get; #else get => 0; #endif` (decision 29's acc1), which main fixes against SHARED-FIX-003. Counting it as a backing field (SHARED-ORDER-007) withholds only where a layout reorder happens; refusing every bodiless accessor in a group would withdraw `get; #if DEBUG set; #endif`, which batch 5 keeps.
- **One definition over four lists** instead of separate "group of whole members / statements" definitions: shorter, and states decision 34's single test once.

## Follow-up candidates

_None new._ The `#pragma` in an excluded branch, the conditional `[StructLayout]` on the type header, and the RH2001 `extern` candidate stay as recorded in `00-lead.md`.
