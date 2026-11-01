Role: spec author
Commit: 6bed9b25f9cb3151d413c4b579d2b7c88ba76b48
Verdict: text written

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond routing audit finding 8 (`17-auditor.md`) to me under decision 23. I worked from that finding, FMT-STRUCT-028, FMT-STRUCT-029, and FMT-BREAK-032 as they stand.

Repair of finding 8 (FC-031, text only):

- `sdd/changes/2026-11/attribute-list-structural.md`, *The merge keeps one line break of its stretch*, last sentence: "The accessor merge keeps none, since it runs only for an accessor list that stays on one line." becomes "The accessor merge keeps none of the stretch's line breaks, in either layout (FMT-BREAK-032)." The accessor merge now also runs for `param:`/`typevar:` lists in an accessor list over several lines; FMT-BREAK-032 ("In either layout, a merge of an accessor's lists keeps no line break …") already says so and is unchanged.
- `AGENTS.md`, `StructuralTransformPhase` row: "merged lists of one target on parameters and type parameters" is extended with ", and of `param:`/`typevar:` lists on any non-accessor owner", matching FMT-STRUCT-029 and the structural merge's extent after round 2. No other document repeats that row (`CLAUDE.md` imports `AGENTS.md`).

No requirement, requirement ID, rule page, or code changed. The change doc's `Status` is untouched.

## Changed requirements

_None._ (Change-doc rationale and `AGENTS.md` phase table only.)

## Examples for the tests

_None._

## Output questions

_None._

## Internal choices

- Used the auditor's suggested wording for both texts; the alternative, restating the accessor layouts in the change doc, would duplicate FMT-BREAK-032.

## Follow-up candidates

_None._
