Role: spec author
Commit: 9b433f1
Verdict: needs a test

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Worked from `00-lead.md` (decision 8, open worklist), `05-spec-author.md`, and `06-test-implementer.md` (*Follow-up candidates*).

## Changed requirements

- FMT-BREAK-030 — The phase keeps the blank lines of every gap it does not join — clarified (example only). The carrier `Child =⏎⏎/* c */ {` holds a comment behind an assignment's `=`, which the whole pipeline now refuses (FMT-UNSUP-020), so the closing sentence about FMT-BLANK no longer held. Replaced by the creation-initializer carrier FMT-BLANK-018 uses: `var p = new P⏎⏎/* c */ {⏎X = 2⏎};`. The lead-in now says "the comment glued to the brace of the creation's initializer"; the closing sentence is unchanged and true again.
- FMT-INDENT-030 — Anchors are measured at their final column — clarified: the list of anchors that start their line behind a leading comment drops "or of a nested initializer"; it now names the opening brace of a complex element initializer only (FMT-INDENT-023). A comment in front of a nested initializer's brace stands behind the assignment's `=` and is refused (FMT-UNSUP-020), as FMT-INDENT-023 already says. No example of FMT-INDENT-030 used that case.

No requirement ID, `Uses` row, or code owner changed, so the architecture spec tests are unaffected.

## Examples for the tests

- FMT-BREAK-030: `tests/Reihitsu.Formatter.Test/Unit/LineBreaks/LineBreakPhaseBlankLineTests.cs`, `KeepsBlankLinesAboveBlockStatementAndGluedComment` pins the example and must move with it: replace its `var p = new P { Child =⏎⏎/* c */ { X = 2 } };` block with the new one (from `sdd/specs/formatter/line-breaks.md`, FMT-BREAK-030):

```csharp
        var p = new P

                /* c */ {
                    X = 2
                };
```

  The test passes either way today (the phase alone does not refuse); the move keeps the test equal to the spec. Its summary may say "glued to the brace of the creation's initializer".
- FMT-INDENT-030: no test pins the dropped wording.

Checked with `scripts/trace.sh` on the new FMT-BREAK-030 input at this commit: `LineBreakPhase` makes no change; the whole pipeline does not refuse it, `BlankLinePhase` removes only the blank line above `/* c */` and keeps the one above the block statement, and pass 2 is stable — as the closing sentence states.

## Output questions

_None._

## Internal choices

- **The FMT-BLANK-018 carrier** for FMT-BREAK-030 rather than dropping the glued-comment half of the example: it keeps the example showing a blank line the phase keeps and FMT-BLANK later removes. Alternative: keep only the block-statement half.
- **FMT-INDENT-030 narrowed by deleting the nested-initializer mention**, rather than adding a sentence that it is refused: FMT-INDENT-023 already states the refusal, and FMT-INDENT-030 lists anchors that occur.

## Follow-up candidates

_None._
