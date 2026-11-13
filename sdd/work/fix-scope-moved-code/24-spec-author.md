Role: spec author
Commit: 4dd7b38
Verdict: spec written

Correction from lead decision 42 (spec checker 23).

## Changed requirements
- SHARED-FIX-008 — A fix keeps the document's line ending — changed: the refusal of a fix that moves code without the formatter now counts every line break in the moved or rewritten span that is not the detected one, including line breaks inside a literal or a comment. Why: telling those apart would need more than a cheap check, and the class is rarer still.
- Change doc `sdd/changes/2026-11/moved-code-keeps-its-lines.md`: the decision text now says SHARED-FIX-008 is "changed", matching the header. The What bullet states that the refusal covers any foreign line break.

## Examples for the tests
SHARED-FIX-008 (unchanged examples; any foreign line break withholds, also one inside a verbatim string).

## Output questions
_None._

## Internal choices
_None._

## Follow-up candidates
_None._
