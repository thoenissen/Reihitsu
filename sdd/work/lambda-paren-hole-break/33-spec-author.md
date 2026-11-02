Role: spec author
Commit: 71d12cc
Verdict: text written

This is a documentation repair after audit 32 passed, for lead decision 18 (auditor finding #6). I am a fresh agent. The prompt named the change it asked for. It carried no suspected cause or hoped-for verdict. Only `sdd/changes/2026-11/lambda-paren-hole-break.md` changed. No spec, code, test, or rule page changed.

## Changed requirements

_None._ `requirement-ids.txt` is untouched. The output described is the one unchanged FMT-INDENT-028/031 demand, with the anchor measured after the move (FMT-INDENT-030).

Change doc (status stays `Accepted`):
- **Entry 9, second case.** A new paragraph and example follow the existing example. They cover an anchor on a hole line that moves but that a first-case alignment places in the statement's column, from an anchor of its own on an unmoved line. The examples are a lambda argument after a wrapped first argument (`aaa */N(1,⏎() => …`) and an initializer member `F = () => …` of a `new D` on such a line.
  - Before, literal aligned or moving left: `main` places the anchor line and leaves the block in column 0, stable.
  - Before, literal moving right: `main`'s pass 1 puts the anchor line and the block in column 13. Pass 2 moves only the anchor line, and the block stays in column 13.
  - After: the block stands under the anchor on the first pass, with the body one level deeper.
- **Closing sentence of entry 9.** It is rewritten. The "keeps its output" members still hold. For a block, they hold only while the anchor is not in the statement's column. A block whose anchor stands there is the second case, whether its line does not move or an alignment places it there.
- **Intro of *Visible output changes*.** "`main` is stable on the before output" in the second case now carries the qualifier "from its second pass on where an alignment places the anchor's line".
- ***Unchanged*, bullet 2.** It now names this member too. The prompt said "entry 9 only", but this bullet restates entry 9's exception and would otherwise be incomplete.

Evidence. I built the Debug CLI at `main` (9327619, the current `origin/main`) and at head (71d12cc) in scratchpad worktrees, now removed.
- **The doc example.** I extracted the input from the doc and ran it three passes on each version.
  - `main` pass 2 equals the "Before" block. Pass 1 differs only in `() =>` of `a`, which is in column 13. Pass 3 is stable.
  - Head pass 1 equals the "After" block, and head is stable.
- **A literal moving left** (content right of the quotes) gives the same `main` output as the aligned literal: block in column 0, stable. Head gives the same "After".
- **Initializer member.** The input is `a */new D {⏎F = () => { return 1; } })}`, with the statement in column 12 inside an `if`, and `(` lands in column 12.
  - Aligned literal: `main` leaves the block in column 0, stable.
  - Literal moving right: `main` pass 1 puts everything in column 13. Pass 2 places the initializer lines and leaves the block in column 13.
  - Head puts `{`/`}` in column 12 and `return 1;` in column 16, stable.
- **Control.** In the same input with `aaaaa */new D`, `(` lands in column 16, not the statement's column. There `main` already places the block, and only the first case applies. So the member is bounded by the statement's column, as written.
- I did not run the `$@` continuation variant. The auditor's corpus 2 covers it.

## Examples for the tests

The `largs` example of entry 9's new paragraph (input, "After") is the regression test the auditor's hint 1 suggests, if the lead wants this member pinned. It is optional. Families A–D already exercise the same code path.

## Output questions

_None._

Visible output change beyond decision 18: none in the code, since this round changed only text. One point for the lead, recorded in the doc: on `main` with a literal that moves right, this member is **not** fixed at `main`'s second pass. The block stays in the shift's column after the anchor line moves. So the first case's statement "the after output is `main`'s second pass" does not hold for it, and the doc places it under the second case, not the first. The auditor's remark "which the first case already covers" holds only for the anchor line, not for the block.

## Internal choices

- The title of entry 9 is unchanged ("anchor stands on a hole line that does not move"). The new member's alignment chain roots in such a line, and the paragraph says so. The alternative was to retitle the entry. I rejected it because the entry's subject stays the unmoved line.
- I used the auditor's `largs` input as the example and named the initializer member in prose only. The alternative was a second example block. I rejected it because the mechanism and output shape are the same.

## Follow-up candidates

_None._
