Role: spec author
Commit: cf12a81
Verdict: spec written

Repair round 1, closing spec-checker finding 1 (`13-spec-checker.md`) under Decision 7 of `00-lead.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements
- PLAY-012 — Lines do not wrap — clarified: added the sentence "The output pane MUST be a stop in the keyboard's tab order, so a user without a pointer can reach it and scroll it; the input text area is one already." The example gains "Pressing Tab from the input reaches the output pane, and the arrow keys then scroll it." (in `Home.razor` the output `pre` with `tabindex="0"` follows the input text area and comes before the Format button). No new ID; `requirement-ids.txt` untouched.
- Change doc `sdd/changes/2026-11/playground-highlighting.md`, *What*: the PLAY-012 bullet now also says the output pane can be reached with the keyboard to scroll it, as the output text area could before. Status stays Accepted; no visible output change is added, because on `main` the output is a focusable text area and this sentence keeps that reachability.

## Examples for the tests
PLAY-012 — already covered by `LayoutTests.OutputIsReachableByKeyboard`; no new test needed.

## Output questions
_None._

## Internal choices
- Stated the requirement as "a stop in the keyboard's tab order" (what the test asserts: `tabindex >= 0` on the box that scrolls the output) rather than naming the attribute or the focus outline; the outline (`:focus-visible` in `app.css`) is styling, which the spec leaves out as a non-goal. Alternative: also require a visible focus indicator — not taken, no test pins it.
- Left the spec checker's optional PLAY-014 note (listing U+0085, U+2028, U+2029 and surrogate pairs as examples) alone: the wording already covers them and the finding did not ask for it.

## Follow-up candidates
_None._
