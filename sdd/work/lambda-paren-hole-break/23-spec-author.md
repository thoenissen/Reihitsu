Role: spec author
Commit: 01ab12c
Verdict: change doc written

Repair round 2, fresh agent, for spec-checker finding 1 (22-spec-checker.md) and lead decisions 12 and 13. The prompt named the change asked for; it carried no suspected cause or hoped-for verdict beyond that.

## Changed requirements

_None._ FMT-INDENT-028, FMT-INDENT-030, FMT-INDENT-031, and FMT-RAW-007 already demand the output (the anchor is measured after the literal moved); FMT-RAW-009 states which hole lines do not move. `requirement-ids.txt` is untouched.

Change doc `sdd/changes/2026-11/lambda-paren-hole-break.md` (status stays `Accepted`):
- New entry *9. A block whose anchor stands on a hole line that does not move lands under its anchor on the first pass*: input, before, and after in C#, for a block lambda on a `$@` continuation line and behind the end line of a `/* */` comment, anchor in column 12, literal moving 13 columns right. A closing paragraph states that the anonymous method in the same position aligns under `delegate` on the first pass (on `main` it is not aligned at all, entries 1, 2, and 8), and which neighbours keep their output (anchor in or right of the shift's column, anchor on a moving line, aligned literal, literal moving left).
- The intro paragraph of *Visible output changes* says where entry 9's outputs come from.
- *Unchanged*, bullet 2, now names entry 9 as its one exception.

Evidence, taken this round with `scripts/format.sh` on the entry's input:
- `main` (worktree at e094895), pass 1: the block lines `{`, `return 1;`, `})…` all in column 13, the body not deeper. This is the entry's "before".
- `main`, pass 2 on that output: braces in column 12, body in column 16. Not idempotent.
- Branch head 01ab12c, pass 1: braces in column 12, body in column 16, the same as `main`'s pass 2. This is the entry's "after".

## Examples for the tests

Entry 9 is already pinned by `HoleAnchorOnUnmovedLineAlignmentTests` (19-test-implementer.md): `BlockLambdaOnVerbatimContinuationLineOneColumnLeftOfShiftIsAlignedAtItsParenthesis` and `BlockLambdaBehindCommentEndLineOneColumnLeftOfShiftIsAlignedAtItsParenthesis` carry exactly its two inputs and its "after" output. No new test is needed.

## Output questions

_None._ The new output is the one unchanged requirements demand, and the only one-pass output that satisfies them (decision 12). It is a visible output change, so the lead decides whether to put it to the maintainer; I see nothing in it that the requirements leave open.

No visible output change beyond decision 12 was found: decision 13 (floor applied once, after the move) produces the same outputs as entry 9 and the existing entries.

## Internal choices

- Entry 9 shows the block lambda only, with the anonymous method described in a sentence, rather than a second full example. Alternative: a second input/before/after for the anonymous method. Rejected because its "before" on `main` is the unaligned output entries 1, 2, and 8 already list, so the anonymous-method change is not new relative to `main`; it is new only relative to an earlier round of this run.

## Follow-up candidates

_None._
