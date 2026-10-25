Role: spec author
Commit: f9dae52
Verdict: spec written

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It asked me to apply the lead's decisions on `18-spec-author.md`, which are recorded in `00-lead.md` (last section) and `squad/decisions/2026-10-25-phase-prediction-removal-third-amendment.md`. This round does that:
- the change doc is approved, so it becomes `Accepted`;
- Q1 is decided as B: a byte order mark counts as one column in the raw-string move;
- Q2 is decided as A: FMT-PIPE-017 already states it, so its text is unchanged.

## Change doc

`sdd/changes/2026-10/phase-prediction-removal.md` is now **`Status: Accepted`**. The only text change is in the paragraph that lists outputs that do not change. It adds one clause: a raw string on the first line of a text that keeps its byte order mark stays in column 9, under the opening quotes counted with the byte order mark, both today and after this change (FMT-RAW-005). This records unchanged output. What, Why, and Decisions are as approved.

## Changed requirements

- **FMT-RAW-005 — Alignment follows the final position of the opening quotes — clarified.**
  - The column of the opening quotes counts everything in front of them on their line. On the first line of a text that starts with a byte order mark, that includes the byte order mark as one column. This is the before-run behavior, restored by decision Q1 B, and it is the column RH5202 measures.
  - New prose example. It is prose because the byte order mark is invisible in a block. The input is the text `U+FEFF`, `var s = """`, `    a`, `    """;`, given as a string with the byte order mark kept. The output puts `a` and the closing marker in column 9: `U+FEFF` is in column 0, `var` in 1, the quotes in 9. The byte order mark stays at the start (FMT-CLEAN-009).
  - `Uses` now includes FMT-CLEAN-009.
  - The expected column comes from the auditor's observation on c49431e (`17-auditor.md`, finding 4). I did not re-run it, because the CLI strips the byte order mark when it decodes.

No new IDs.

`scripts/test.sh --configuration Debug --project architecture` gives 87 passed and 2 failed. The two failures are the same pending ones as in `18-spec-author.md`:
- FMT-PIPE-017 has no test yet;
- 11 tests are still tagged RH5303-006 or RH5304-005.

## Examples for the tests

- **FMT-RAW-005**: the byte order mark example. It has to go through `FormatSyntaxTree` or `FormatNode` with string input that keeps the U+FEFF. On head (f9dae52) the test fails: it puts the lines at 8. It should pass after the implementer restores column 9.
- Everything listed under "Examples for the tests" in `18-spec-author.md` still stands.

## Open questions for the maintainer

_None._
