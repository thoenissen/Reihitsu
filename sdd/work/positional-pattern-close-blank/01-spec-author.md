Role: spec author
Commit: 569824c
Verdict: change doc written

## Changed requirements

- FMT-BLANK-019 — No blank line before the closing bracket of a collection expression or pattern — changed: names the closing `)` of the positional pattern clause of a recursive pattern (`o is P(1, 2)`, `o is (1, 2)`, switch arm, `case` label), by token kind and parent node; states that a `)` owned by any other node (argument list) is not named; Why extended; two examples added (positional with/without type and in a switch arm; argument list unchanged).
- FMT-BLANK-009 — Around a directive, only the blank line next to the token is removed — changed: the list sentence names the positional pattern clause's `)`.
- FMT-BLANK-018 — A comment glued to a token belongs to the token's line — changed: first bullet names the positional pattern clause's `)`.
- FMT-BLANK-020 — Behind a comment on its own line, no blank line before a brace, closing bracket, or chained keyword — changed: last bullet names the positional pattern clause's `)`.
- `formatter/blank-lines.md` `Uses` row gains FMT-INDENT-025 (now referenced from FMT-BLANK-019's Why); `scripts/test.sh --project architecture` passes (198/198).

Change doc: `sdd/changes/2026-11/positional-pattern-close-blank.md` (Status: Draft). No new requirement IDs, no new code owners (`BraceBlankLinePolicy` is already an owner of `formatter/blank-lines.md`). No rule page changes: no analyzer rule reports a blank line in front of a pattern's closing delimiter.

## Examples for the tests

- FMT-BLANK-019: the new positional example (typed `P(...)`, tuple-shaped `(...)`, switch expression arm) and the argument-list negative example. The `case` label form is named in the text but has no example; it is the same node (`PositionalPatternClauseSyntax`).
- Change doc, visible output change 2: FMT-BLANK-020 (own-line `// last` then blank then `)`), FMT-BLANK-018 (blank then glued `/* last */ )`), FMT-BLANK-009 (`#if DEBUG`/`#endif` then blank then `)`). Each "After" equals `main`'s output for the same input with a list pattern `[`…`]` (observed).
- Change doc, visible output change 3: RH3202 code-fix result (observed "Before" with `scripts/apply-fix.sh RH3202`, LF and CRLF).
- Kept: a blank line behind the `(` (observed kept today, stays).

How established: every "Before" was produced with `scripts/format.sh` on fixtures in the scratchpad on commit 569824c and is stable on a second pass. Every "After" is the "Before" with only the stated blank lines removed; the comment/directive "Afters" were cross-checked against `main`'s list-pattern output for the same shapes. The argument-list example is unchanged by `scripts/format.sh` today (except the file's final newline, which the formatter strips).

## Output questions

_None._ The maintainer answered option A on thoenissen/Reihitsu#1935 (00-lead.md, answer 1) and named "the requirements that refer to" FMT-BLANK-019. Visible output change 2 of the change doc (behind an own-line comment, above a glued comment, behind a directive) follows from those requirements sharing FMT-BLANK-019's token set; the lead decides whether the answer covers it or whether it is asked in the batch.

## Internal choices

- The positional `)` joins all four requirements (FMT-BLANK-009/018/019/020) through the one shared token set rather than FMT-BLANK-019 alone; alternative: FMT-BLANK-019 only, which would give the positional `)` its own rule behind comments and directives (rejected in the change doc).
- Boundary stated by parent node (`)` whose parent is the positional pattern clause), with an argument list as the example on the other side; a parenthesized variable designation (`var (x, y)`) is named only in the change doc's "Unchanged" list, not in the spec.
- FMT-BLANK-019's title kept unchanged ("... collection expression or pattern" already covers it).
- The sentence in FMT-BLANK-009's region-directive example ("The same holds in front of the `]` of a collection expression and the `)` of a parenthesized pattern") left as is; it remains true and is not a token list.

## Follow-up candidates

_None._ (A blank line before the `)` of a multi-line parenthesized variable designation `o is var (⏎x,⏎y⏎⏎)` is kept today; no requirement lays that construct out and the shape is rare, so it is not proposed.)
