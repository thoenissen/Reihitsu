Role: spec author
Commit: 3f24ccd9 (code as of 47345884)
Verdict: spec written

This handover covers `00-lead.md` row 25, from `23-implementer.md` (*Reading of the spec's precondition*). My prompt named the change and the shape to add; it carried no suspected cause and no hoped-for verdict. No decided output changes.

## What I measured

- Head build in a scratch worktree (removed afterwards), `scripts/apply-fix.sh RH7101 --max-iterations 1`, LF and CRLF identical.
- `_p;⏎    /** x */ private int _x, _y;⏎    /** q */ private int _q;` gives `/** x */ private int _x;⏎    private int _y;⏎    /** q */ private int _q;`: the first field keeps the target's line and column behind `/** x */`, and the member behind keeps its trivia. The same without `_q`, and with `/** a */` trailing `_p;`.

## Changed requirements

- **FMT-PIPE-029 — The context entry point writes back every field the split makes of its target — clarified.**
  - The fifth gap bullet now asks the shared-line question only where nothing in the gap starts a line in the document: no line break separates the token in front of the gap from the token behind it. The line breaks that count are those of SHARED-BLANK-008 (end-of-line, the line break ending `///` or a directive); one inside a spanning comment does not (SHARED-BLANK-002). This is the implementation's `AreSeparatedByEndOfLine` precondition.
  - Where something in the gap starts a line, the trivia stays, also where the token behind the gap does not start its line (`private` behind `/** x */`).
  - The Why says why a documentation comment that starts its line counts like the token: the phases start that comment's line, so asking the question would write a second line break.
  - New example: the shape above, with the gap in front of the target and the gap behind it, output as measured.
  - `Uses` gains SHARED-BLANK-008.
- **Change doc `sdd/changes/2026-11/rh7101-fix-through-context-entry-point.md`**: the one sentence on the unchanged side of the condition (case 7) uses the same wording. No output in it changes; Status stays `Accepted`.

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"`: 37 passed.

## Examples for the tests

- **FMT-PIPE-029**, the new example (documentation comment that starts its line in front of the target and of `_q`): at the entry point and as an RH7101 fix test, exact text under LF and CRLF. Split per gap if useful. Head already gives this output.

## Output questions

_None._ The output is the one FMT-PIPE-018 and FMT-PIPE-010 already decided (trivia kept as in the document), and the head gives it.

## Internal choices

- The precondition is stated by the line breaks that count (via SHARED-BLANK-008) rather than as "the token behind the gap starts its line". Alternative: keep the token wording and add a documentation-comment exception; rejected, because the gap-based wording is what the code checks and also covers a `///` or a directive in the gap without further exceptions.

## Follow-up candidates

_None._
