Role: spec author
Commit: 4714448b
Verdict: spec written

This handover covers the structural change of `00-lead.md` row 24 (`20-auditor.md` finding 6 and its architecture finding). My prompt carried no intended fix, no suspected cause, and no hoped-for verdict. No decided output changes.

## What I measured

- **Builds.** I built the head (`4714448b`) and `15fc57c` in scratch worktrees under the scratchpad. `15fc57c`'s code equals `main`; it is the commit the change doc's "before" outputs come from.
- **Document-level formatting.** I formatted the auditor's members with the head's `scripts/format.sh`: d1, d2, d3, d4, d5, d6, d7, g1, g2, and g2's behind form g3. I also formatted six combined inputs, c1–c6, which put each shape in both gaps.
- **Fix outputs.** I ran `scripts/apply-fix.sh RH7101 --max-iterations 1` on the head and on `15fc57c`. Every output was the same under LF and CRLF.
- **Result for d1, d2, d4, d5, d6, g1, g2, g3.**
  - Formatter: the documentation comment starts its own line, with a blank line above it, and its member goes with it. A comment that trails the code in front of it stays behind that code. The later line of a documentation comment that spans lines keeps its column, for example `       b */ private int _x;`.
  - Head fix: leaves the member where it was, which is finding 6.
  - `main`'s fix: also leaves the member where it was, and writes `_y` in column 0.
- **Result for c5 and c6.** In these, the first token behind the gap already starts its line: a spanning `/** */` with a line break behind it, and `///`. The formatter moves the documentation comment. The fix keeps it where it stands, on both the head and `main`. FMT-PIPE-018 and FMT-PIPE-010 keep that output, and this is the auditor's holding members d8–d10.

## Changed requirements

- **FMT-PIPE-029 — The context entry point writes back every field the split makes of its target — clarified.**
  - The fifth gap bullet now states the decision as a result of the phases, read from the context as they lay it out. The entry point does not decide it again from the gap in the document.
  - The question arises only where the first token behind the gap does not start its line in the document. Where it does, FMT-PIPE-018 and FMT-PIPE-010 keep the trivia.
  - Where it arises, the first field, or the member behind the target, starts the next line exactly when, in the laid-out context, its first token or a documentation comment in its leading trivia starts a line. Two phases start such a line: the documentation-comment phase (FMT-DOC-001) and the line-break phase (FMT-BREAK-007).
  - The Why names both phases. It says why a prediction from the input misses what an earlier phase changed. It also says why the documentation comment stays where it stands when the first token already starts its line.
  - Five new examples, one per shape, each with a gap in front of and behind the target:
    - a spanning `/** */` (d1+d2);
    - a spanning `/* */` then a one-line `/** */` (d5+d6);
    - a one-line `/* */` then a spanning `/** */` (d4+g1);
    - a spanning `/* */` then a spanning `/** */` (g2+g3);
    - the other side of the condition, the c5 input, with c6's `///` named in its title (FC-002).
  - The outputs are the head formatter's layout without the blank line above the documentation comment.
- **Change doc `sdd/changes/2026-11/rh7101-fix-through-context-entry-point.md`.** Case 7 gains the c1 and c2 inputs, with "before" (measured on `15fc57c`) and "after". A sentence covers the other two shapes and the unchanged side of the condition. Status stays `Accepted`.
- No other requirement changed. RH7101-004 already says a documentation comment goes with the member it documents.

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"`: 37 passed.

## Examples for the tests

- **FMT-PIPE-029.** Use the five new examples at the entry point and as RH7101 fix tests. Split them per gap where that helps, which gives the auditor's d1, d2, d4, g1, d5, d6, g2, and g3.
  - Assert the exact text under LF and CRLF.
  - The last example is the holding side and must keep its output, as must c6 (`///` in both gaps). Today's fix already gives c6's output.
  - Formatting the fix result adds only the blank line above each moved documentation comment. A test that checks "formatting changes nothing" must allow that blank line, as the existing `/** X. */` example does (SHARED-FIX-006).
- The existing holding members stay: `p`, `q`, `pq`, f1, f4, d3, d7, and the one-line `/* c */` / `/* d */` example.

## Output questions

_None._ Each new "after" output is the formatter's layout, which answer 1a–1f A makes the fix result, without the blank line, which SHARED-FIX-006 and the existing FMT-PIPE-029 bullet rule out. Where a comment and a documentation comment stand in the gap, the existing FMT-PIPE-029 bullet already decides where each one goes: the comment stays behind the code in front of it, and the documentation comment goes with its member. The holding side keeps today's output and `main`'s.

## Internal choices

- **The question is stated as "does the first token behind the gap start its line in the document" plus "in the laid-out context, does its first token or a documentation comment in its leading trivia start a line".**
  - Alternative: "the phases wrote a line break in the gap that the document lacks". I rejected it because a `///` comment carries its own line break, so that wording would need a definition of which line breaks count.
  - The chosen wording can be checked on the formatted context the entry point already holds (row 24).
- **The holding side (c5, c6) is written into the spec as an example**, although no output changes there. FC-002 asks for an example on each side of the new condition.

## Follow-up candidates

_None._ The scratch worktrees under the scratchpad are removed.
