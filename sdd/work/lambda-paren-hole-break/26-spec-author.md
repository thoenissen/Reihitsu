Role: spec author
Commit: 2fe257d
Verdict: change doc written

Structural round after audit 25, fresh agent, for lead decision 17 (auditor finding #5) and the change-doc side of decision 15 (finding #3). The prompt named the change asked for; it carried no suspected cause or hoped-for verdict beyond that.

## Changed requirements

_None._ `requirement-ids.txt` is untouched.

Decision 15 needs no new sentence. The output it restores is stated by requirements that already hold without regard to whether an alignment changes a column:
- FMT-INDENT-028 and FMT-INDENT-031 say the block MUST start in the anchor's column, measured after the literal moved (FMT-INDENT-030). Nothing in them depends on the column the block had before.
- FMT-RAW-007 decides between "keep the alignment" (bullet 1) and "move only with the literal" (bullet 3) by whether an alignment requirement places the line relative to a token inside the literal, not by whether that alignment writes a different column. A block whose anchor stands in the column block indentation gives its brace is placed by FMT-INDENT-028/031 under bullet 1.
- "A delta-0 alignment still records a placement" is a statement about `LayoutModel`, not about visible output, so it does not belong in a spec.

Change doc `sdd/changes/2026-11/lambda-paren-hole-break.md` (status stays `Accepted`):
- Entry 9 is retitled *An alignment whose anchor stands on a hole line that does not move lands under its anchor on the first pass* and split into two cases.
  - **First case, anchor left of the literal's rightward shift.** It now names every alignment, not only blocks: the block of a block lambda, wrapped arguments (FMT-INDENT-012), creation initializers (FMT-INDENT-022), collection expressions (FMT-INDENT-025), switch expressions (FMT-INDENT-027), member-access chains (FMT-INDENT-029), and conditional operators (FMT-INDENT-020). The existing block example is kept, and a second example adds a wrapped argument list and a chain behind a comment end line. Here "before" is `main`'s first pass and "after" is `main`'s second pass.
  - **Second case, anchor in the statement's column (finding #3).** This case applies to blocks only. Its "before" is `main`'s output, which is stable but unplaced: column 13 when the literal moves right, column 0 when it is aligned. The example shows both. Its "after" is the output FMT-INDENT-028/031 demand, with the brace under the anchor and the body one level deeper. This "after" is not `main`'s second pass, and the entry says so.
  - The closing paragraph adds `delegate` in the statement's column to the anonymous-method sentence. It also corrects the list of members that keep their output: the statement's column is excluded.
- The intro to *Visible output changes* says where each case's "before" and "after" come from.
- *Unchanged*, bullet 2: the exception now names the statement's column as well.

Evidence. I built the CLI at `main` (e094895) and at head (2fe257d) in scratchpad worktrees and ran each fixture twice:
- **First-case example (arguments and chain).**
  - `main` pass 1: `2),` and the links in column 13.
  - `main` pass 2: `2),` in column 9 and the links in column 8.
  - Head pass 1 equals `main` pass 2, and head is stable.
- **Sweep, first case.** Arguments, chain, ternary, creation initializer, switch expression, and collection expression, each with the anchor in columns 5–11 behind a `/* */` end line, literal moved right: 42 fixtures.
  - Head pass 1 equals `main` pass 2 in all 42, and head is stable.
  - `main` is unstable in 38. In the 4 stable ones the aligned column happens to coincide with the shift's column.
  - With the anchor in column 8, the statement's column, the creation initializer, switch expression, collection expression, and chain already land under their anchor at head. So the second case is limited to blocks.
- **Second-case example.** On `main` and on head the output is identical and stable: block lines in column 13 for the moving literal and column 0 for the aligned literal. The anonymous method in the same position gives column 13 on both. The "after" is derived from FMT-INDENT-028/031. Head does not produce it yet, because decision 15 is not implemented at 2fe257d.

## Examples for the tests

- Second case of entry 9: the two inputs and the "after" output (`() =>` with `(` in column 8, moving literal and aligned literal). The anonymous-method variant `aaaaa */delegate { return 1; })}` gives the same block columns under `delegate`. These are finding #3's boundary tests (decision 15). The test implementer derives them together with the auditor's enumeration (anchor in the statement's column and one column on either side, statement one block deeper, moved/aligned/left-moving literal).
- First case, arguments and chain example: optional. It pins the FMT-INDENT-030 members of entry 9 that the existing `HoleAnchorOnUnmovedLineAlignmentTests` cover only for blocks.

## Output questions

_None._ Every new output is the one unchanged requirements demand (FMT-INDENT-012/020/022/025/027/028/029/030/031, FMT-RAW-007).

Visible output change beyond decisions 15 and 17, for the lead to weigh: in the second case, `main`'s output is **stable**. A block lambda whose anchor stands in the statement's column on an unmoved hole line keeps its unplaced lines on every pass on `main`. The fix therefore changes a fixed point of `main`, not only a first pass. Decision 14's reasoning ("`main`'s output there is not idempotent") does not cover this member. It rests on FMT-INDENT-028 alone, as decision 15's "Why nothing visible is in question" already states ("block lambdas fail the same way on `main`"). I found no other visible change.

## Internal choices

- No spec sentence for decision 15 (see *Changed requirements*). Alternative: add to FMT-RAW-007 bullet 1 that an alignment places its lines also when the anchor's column already equals the column the lines would get otherwise. Rejected: the bullet already decides by "an alignment requirement places the line", and the sentence would state a mechanism.
- Entry 9 lists the FMT-INDENT-030 alignments as "among them …" with the six kinds checked, rather than claiming every alignment. Alternative: "every alignment". Rejected: only these six kinds and blocks were run.
- Creation initializers are cited as FMT-INDENT-022 only. The auditor's "object initializers" fixtures use `new`. FMT-INDENT-023 (initializers without a keyword) was not run, so it is not named.

## Follow-up candidates

_None._
