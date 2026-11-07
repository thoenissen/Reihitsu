Role: spec author
Commit: b4b2526
Verdict: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the maintainer's answers in `00-lead.md`, which the spec follows.

## Changed requirements

- RH5202-003 — The fix shifts the lines after the first by the same amount — clarified: the normative sentence now excepts the lines RH5202-004 and RH5202-005 leave as written; its `Why` no longer claims that shifting every line keeps the value. Title changed from "…every line after the first…". Its example is unchanged.
- RH5202-004 — The fix leaves the lines of text nested in a hole as written — added (Q1 = A; uses FMT-RAW-009). Covers the skeptic's whole class: `@"…"`, `@"…"u8`, `$@"…"`, `@$"…"`, `/* */` behind or in front of a token, `/** */`, both shift directions, every depth. States each qualifier with a case on the other side: a line after the nested text has ended shifts; the line on which it begins shifts; the lines of a nested raw string shift with the outer literal and the nested literal stays reported for its own fix.
- RH5202-005 — The fix leaves an empty content line empty — added (decision 4, B5; uses FMT-RAW-002). "Empty" is no character at all; a whitespace-only content line shifts. Restricted to content lines (the literal's own text), not hole lines.
- Header `Uses` gains FMT-RAW-009, SHARED-FIX-003, SHARED-FIX-007 (the last two because the new `Why` texts cite them; `UsesListsExactlyTheReferencedRequirements` requires it). `sdd/specs/requirement-ids.txt` gains RH5202-004 and RH5202-005.

No change doc: intended behavior does not change. SHARED-FIX-003 (no change to a literal's value or a comment's text) and SHARED-FIX-007 with FMT-RAW-002/FMT-RAW-009 (the formatter's layout) already applied to this fix and already decided both outputs; RH5202-003's wording contradicted them and its own `Why`. The maintainer's answer to Q1 picked that existing reading. The spec change brings the rule spec in line with them, which is a clarification under `sdd/README.md`, *When a change doc is required*. The code is the defect, so this is the bug-fix lane.

Rule page `documentation/rules/RH5202.md` is unchanged: what is reported and how to fix it do not change in the normal case.

Verified on b4b2526 with `scripts/apply-fix.sh` and `scripts/format.sh` (scratch fixtures, deleted afterwards):
- B5 (plain, rightward): the fix writes 12 spaces onto the empty line and the formatter leaves it empty; a 6-space whitespace-only line becomes 18 spaces under both. Leftward, the fix already leaves the empty line empty.
- Empty content line in an interpolated literal: the fix writes 13 spaces and the formatter leaves it empty.
- A nested raw string in a hole (the RH5202-004 last example): one fix application moves the inner lines by the outer amount and the inner literal stays reported. This is today's output. It must keep holding (SHARED-FIX-002 covers the nested diagnostic in Fix All).

`SpecStructureTests` and `SpecConsistencyTests` pass. `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails for RH5202-004 and RH5202-005 until the test implementer tags their tests.

## Examples for the tests

- RH5202-004: every example (rightward verbatim, leftward verbatim, comment with the shifting `}` line, empty line inside a verbatim, nested raw string). Beyond the examples, the requirement text names `@"…"u8`, `$@`/`@$`, `/** */`, a leading comment, and the deeper nestings. The skeptic's fixtures A5, A8, B1, B3, B4, B7, B8, B9 in `01-skeptic.md` cover them. Use CRLF variants as the skeptic asks.
- RH5202-005: both examples (plain and interpolated).
- RH5202-003: existing tests keep their tag.

Note for the implementer (decision 2): if the fix takes its line shifts from `RawStringLineShifts`, it must not take FMT-RAW-008's added distance for a nested raw string. The RH5202-004 nested-raw example fixes the single-fix output with the inner literal unaligned and still reported.

## Output questions

_None._ Q1 and Q2 are answered. No input with a known unsafe rewrite was found, so Q2 adds no requirement. A withheld fix stays under SHARED-FIX-003 by reference.

## Internal choices

- Two new requirements instead of rewriting RH5202-003 into one long sentence. They are separate decisions with separate reasons: nested text versus empty lines. The alternative was a single amended RH5202-003.
- No change doc (reason above). The alternative was a change doc recording Q1.
- RH5202-005 is limited to content lines. On a blank line inside a hole, the formatter also re-wraps the hole's expression (`name +⏎⏎ 1` becomes `name⏎ + 1`), so the formatter's layout there is not the shift alone. That layout is not specified here. The fix's own SHARED-FIX-006 scope covers it.
- RH5202-003 still states that hole lines (other than RH5202-004's) shift by the same amount. This is unchanged existing behavior. Where FMT-RAW-007 would re-align a hole line, it is not addressed.

## Follow-up candidates

_None._
