Role: spec author
Commit: 0784dcb
Verdict: text written

Narrowing round (`00-lead.md` decisions 41 and 42; `28-auditor.md` findings 8 and 9). Spec prose only; no code or tests touched. `scripts/verify-text-only.sh --base HEAD --head worktree --strict-docs`: `TEXT-ONLY PROOF: PASS — base=0784dcb head=worktree — 3 changed file(s) (3 non-compiled, 0 C# comment/layout-only); xml-doc impact: none`.

## Changed requirements
- FMT-BREAK-006 — One statement per line — clarified: a `/** */` comment written behind code counts as a comment that starts on a line of its own (FMT-DOC-001 behind a statement or brace, FMT-BREAK-035 behind a label), so the statement stays behind its `*/` and neither exception applies; "glued to the statement" dropped (finding 8).
- FMT-BREAK-033 — Why: the acceptance for sections and labels now cites maintainer answer 3 instead of answer 2.
- SHARED-COMMENT-012 — restated placement summary removed; the pointer to FMT-BREAK-006/033/034 stays (finding 9).
- Change doc `sdd/changes/2026-11/phase-order-line-breaks.md`, *Decisions*: answer 3 recorded as its own maintainer answer; the "lead's reading, not a separate answer" wording is gone (finding 7).

## Examples for the tests
_None._ The new sentence adds no example block; it states output the code already produces (28-auditor #8, members a–d). Per decision 41 tests are optional; I recommend none, since decision 42 closes the round.

## Output questions
_None._

## Internal choices
- Stated the `/** */` case as "is such a comment" (one that starts on a line of its own) rather than as a third exception; alternative was a separate bullet.

## Follow-up candidates
_None._
