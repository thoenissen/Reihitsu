Role: spec author
Commit: c77e49a
Verdict: text written

Text-only repair of audit 17 finding 2 (FC-031). No requirement changes meaning; no test, code, or `.cs` file touched (`scripts/verify-text-only.sh` not needed: no C# file in the diff). `scripts/lane-check.sh behavior-change --head worktree`: confirmed.

- `sdd/specs/formatter/line-breaks.md`, FMT-BREAK-039 *Why*: the sentence claiming a joined argument list "never decides the list's layout" is bounded to the list itself — the removed line break never counts for the list; it reaches the list only through a construct of the first bullet nested in the element (a collection expression, judged as written). It also names "a query clause after the first" and "an array or `stackalloc` initializer", as the second bullet does. The example heading before the nested-construct example now says "a query clause after the first" (its query example uses `select`).
- `sdd/changes/2026-11/argument-list-below-name.md`: "a query clause" → "a query clause after the first" in all five places that described change 7 (*What* line 13, change 7's heading, *Decisions*, the one-owner paragraph, *Alternatives*), matching line 11 and FMT-BREAK-039.

## Changed requirements
FMT-BREAK-039 — *Why* wording and an example heading — clarified (text only).

## Examples for the tests
_None_ — no example changed.

## Output questions
_None._

## Internal choices
- Applied "after the first" to every change-7 mention in the change doc (five), not only the two the auditor named, so the doc states one scope throughout; alternative was the two named places only.

## Follow-up candidates
_None._
