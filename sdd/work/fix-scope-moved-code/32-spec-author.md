Role: spec author
Commit: 6cc515b
Verdict: text written

Text-only widening per lead decision 46 (spec checker 30 finding 1, auditor 31 finding #6). My prompt carried no intended fix or hoped-for verdict beyond those findings. No new or changed IDs, no code, tests, or rule page. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: Passed 51, Failed 0.

## Changed requirements
- RH7004-007 — The fix is withheld for lines it cannot change as whole lines — clarified: the foreign-line-break refusal covers the code the fix moves or rewrites, the using declaration with its trailing line break included, also where the declaration is the last statement and nothing moves.
- SHARED-FIX-008 — A fix keeps the document's line ending — clarified: the lone-`\r` sentence and the Why say "moves or rewrites" (the Why: "its trailing trivia included"), matching the paragraph's normative sentence.
- Change doc `sdd/changes/2026-11/moved-code-keeps-its-lines.md`: header entry for SHARED-FIX-008, *Visible output* bullet "Foreign line breaks in moved code", the paragraph under "Rare-class refusals of repair round 2", and decision "A foreign line break in moved code withholds the move" (now cites decision 43). Headings kept as they are.

## Examples for the tests
_None._ The existing five tests tagged RH7004-007 and SHARED-FIX-008 already cover the wording.

## Output questions
_None._

## Internal choices
- Kept the headings "Foreign line breaks in moved code" and "A foreign line break in moved code withholds the move" unchanged and widened only their bodies; alternative: retitle them.

## Follow-up candidates
_None._
