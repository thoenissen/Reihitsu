Role: spec author
Commit: 6b07459
Verdict: text written

Final narrowing round, for lead decision 15 (audit 3, finding #6). My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements

- **RH5202-006 — The fix is withheld when a leftward shift would change the value of a raw string nested in a hole — clarified (Why text only).** The last sentence of *Why* claimed for every input that the outer fix is offered once the nested literal's fix has run. It now states that only for the case where it holds: the nested opening quotes stand at least as many columns from their line start as the shift (always so when the nested literal opens on a content line of the outer literal, as in every example). The nested fix aligns the closing quotes under the opening quotes, so the closing whitespace becomes at least as wide as the shift. Otherwise — the nested literal opens on a line that starts in the outer literal's hole code at a low column, the audit's members (a) and (b) — the outer diagnostic stays without a fix. The normative sentence, the conditions, and the examples are unchanged; no requirement ID added.

The rule page is unchanged: a withheld fix in a corner case is not a page change.

## Examples for the tests

_None._ No test depends on the changed text; the behavior it describes is unchanged. Members (a) and (b) of finding #6 end in the supported state "diagnostic without a fix", and no test is required for them.

## Output questions

_None._ SHARED-FIX-003 and maintainer answer Q2 = A decide the withheld side.

## Internal choices

- Kept the release sentence and added the condition and the other side, rather than deleting the release claim. Alternative: drop the sentence; but the release is what the Fix All tests pin and is worth stating where it holds.

## Follow-up candidates

_None._
