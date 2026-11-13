Role: spec author
Commit: 70537f8
Verdict: spec written

Decisions 26 and 27 recorded in the specs and the change doc. No new requirement IDs. The prompt carried no intended fix beyond the lead's decisions.

## Changed requirements

- RH3004-006 — The lambda is laid out as the formatter lays it out — changed: on the block-body path, a tab in the whitespace in front of a moving line withholds the fix, whatever the shift (decision 26); a tab inside a comment or string, between tokens, in front of the anonymous method's first line, or on a blank line does not. Clarified: a blank line holding whitespace is written empty (decision 27). Why gains one sentence for each. Two examples added.
- SHARED-FIX-007 — moved-code bullet — clarified: a blank line of moved code (only whitespace, outside a string, comment, and disabled text) is written empty and stays a blank line; it neither keeps its column nor moves. The tab-counting sentence now says RH3004 withholds instead of counting (RH3004-006). Why gains one sentence.
- Change doc `sdd/changes/2026-11/moved-code-keeps-its-lines.md` — Affected requirements row for RH3004-006 updated; new visible output change "A tab in front of a moved line (RH3004)": on main, the fix keeps the tab and writes 16 spaces in front of it; after, no fix is offered. Two decisions and two rejected alternatives added. Status stays Draft.

Checked on main (4bdcd13) with `scripts/apply-fix.sh`: a whitespace-only line among moved lines comes out empty for RH3004 (12 spaces), RH7004 (8 spaces), and an RH7109 member move. Decision 27 therefore changes nothing visible against main. The RH3004 tab example was run on main too, and its output is the Before block in the change doc.

## Examples for the tests

- RH3004-006: "A blank line that holds whitespace inside the block is written empty" (fix offered, the blank line comes out empty), and "A tab in front of a moving line withholds the block-body fix" (reported, no fix). Both examples hold literal whitespace: 12 spaces on one line, and a tab followed by 8 spaces on the other.
- SHARED-FIX-007 blank line: one RH7004 test with a whitespace-only line between moved statements is enough to cover it.

## Output questions

_None._ Decision 26 refuses a rare class, and decision 27 keeps main's output.

## Internal choices

- The tab refusal is recorded in RH3004-006, which owns the moved lines. RH3004-007/008/009 are about strings and directives, and none of them fits. The alternative was a new requirement, RH3004-010.
- The refusal applies to "a moving line, whatever the shift", not only to lines with a non-zero shift. The check stays cheap, and a zero shift is coincidence in this fix. The alternative was refusing only when the shift is non-zero.
- Lines that stay (column-0 comment lines) never have leading whitespace, so they need no exception.

## Follow-up candidates

_None._

## Notes

- `scripts/test.sh --project architecture` (SDD consistency) could not run: the Release build fails on RH7305 in `RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.cs` (decision 28, cleaner).
- These are the first literal tabs in `sdd/` Markdown (RH3004.md and the change doc). Keep them as they are, because the example depends on the tab.
- Rule page `documentation/rules/RH3004.md` is unchanged: a withheld fix is covered by general-notes.

## Correction

- The Why of RH3004-006 referenced FMT-PIPE-017 without listing it in the spec's `Uses` row, so `SpecStructureTests.UsesListsExactlyTheReferencedRequirements` failed. I dropped the reference, because SHARED-FIX-007 already states the counting rule. `scripts/test.sh --project architecture --configuration Debug --filter "FullyQualifiedName~Sdd"`: 51 passed.
