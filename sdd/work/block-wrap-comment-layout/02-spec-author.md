Role: spec author
Commit: e56e8e4
Verdict: spec written

## Changed requirements

- FMT-SWITCH-007 — Comments survive brace insertion — clarified. It now records both maintainer answers from `00-lead.md`:
  - Answer 1A: brace insertion carries as written the gap between the last label and a comment behind it, and between the last wrapped statement and a comment behind it. It does not normalize that gap (FMT-SPACE-001). The `Why:` says why: the same statement keeps the gap in a plain block and in a braced `if`/`else` body.
  - Answer 2A: the comments around the terminal `break` keep the author's layout, as FMT-PIPE-013 keeps it for wrapped code. This covers the blank lines between leading comments (already stated before this change), the space between two comments on one line, a comment written in front of the `break` on its line, and the trailing comments behind the `break` with their gaps. One sentence ties this to FMT-SWITCH-002: the `break`'s line still starts after the closing brace, and only comments the author wrote on that line share it.
  - The `Why:` sentence "Brace insertion rebuilds the trivia" became "moves the trivia". The old wording described the mechanism being removed (decision 5).
  - New example: one switch with every member of both answers, input and output.
  - `FMT-SPACE-001` was added to the spec's `Uses` header.

FMT-PIPE-013 is unchanged. It neither decides nor contradicts either answer: its three listed layouts already cover the comments of wrapped code, and the two answers only concern the switch phase. FMT-SWITCH-002's "on its own line" is unchanged. FMT-SWITCH-007 now says what it means when comments share the `break`'s line. No requirement IDs were added, so `requirement-ids.txt` is unchanged. `scripts/test.sh --project architecture --filter Sdd` passes (51/51).

## Examples for the tests

- FMT-SWITCH-007, the new last example. It covers the label gap `case 1:    // label`, `/* lead */ A();` on one line, the last-statement gap `B();    // four`, `/* b1 */ /* b2 */ break; /* x */ // y` after the blank line, and the terminal-`break` trailing gap `break;    // gap` in a second section. Tests should check it as a full pipeline over two passes, with LF and CRLF (01-skeptic, FC-010/FC-017).
- FMT-PIPE-013 and FMT-SWITCH-007's existing examples are unchanged. The demanded members from the skeptic's matrix (L1, L2, and L3 at sites 1–3, and the `///` idempotency member under FMT-PIPE-004) still follow from those requirements as they stand.

## Output questions

_None._

## Internal choices

1. **Where the answers are recorded**: in FMT-SWITCH-007, not as a fourth item in FMT-PIPE-013's list. Alternative: add "the gap in front of a comment behind the code" to FMT-PIPE-013. That would also bind the expression-body conversion (`ExpressionBodyToBlockConverter`), which the skeptic did not trace for this layout (matrix: "not traced"), and the maintainer's answers name only the braced switch section. Braced `if`/`else` bodies already keep the gap through FMT-STRUCT-026 and the change doc `phase-tree-round-trip.md`.
2. **Lane classification.** I wrote this as a clarification of FMT-SWITCH-007 with no change doc, as the prompt asked. The lead should know that both answers change output on `main` for the inputs in 01-skeptic's questions 1 and 2. `sdd/README.md` asks for a change doc only when intended behavior changes. My recommendation is that the lane stays as it is: FMT-SWITCH-007 already applied FMT-PIPE-013 to these comments, and FMT-PIPE-013's `Why:` already rules out a wrap that loses layout only because it rebuilt trivia, so the answers decide a reading the spec left open rather than reversing a requirement. If the lead reads it differently, the change doc would record the two answers under `Decisions`.

## Follow-up candidates

_None_ beyond those already listed in `00-lead.md`. FMT-STRUCT-003's stale `Why:` was not touched, because it lies outside the two answers.
