Role: spec author
Commit: a010df3
Verdict: spec written

Carries out decision 38 of `00-lead.md`, which extends maintainer answer 2 to switch sections and case labels. The prompt contained only the lead's decision: no intended fix, suspected cause, or hoped-for verdict. No code or test changed.

I checked the decided behavior with the CLI, built once from head a010df3 and once from the `main` merge base aec8dc6. Both outputs are identical, and head's output is stable on a second run. The behavior now stated in the specs:

- A section or a label behind the closing `*/` of a multi-line delimited comment **stays behind the `*/`** when the comment starts on a line of its own.
- It **moves** when the comment starts behind the token in front of it: the previous section's last token, the previous label, or the switch's opening brace. The line break goes in directly below the comment's last line, whatever statement ends the previous section. For example, `return 2; /* c⏎ */ case 3:` moves even though no blank-line requirement applies.
- Statements depend on two conditions (FMT-BREAK-006). Sections and labels depend on one: where the comment starts.
- A multi-line `/** */` comment written behind code starts a line of its own first, so the section or label stays behind its `*/`.
- The FMT-BLANK-004 blank line in front of a section behind `break` goes below the comment where the line breaks, and above the comment where the section stays.

## Changed requirements
- FMT-BREAK-033 — Every switch section starts its own line — changed.
  - New paragraph: how a section behind a multi-line comment's closing line is placed, with both sides of the condition (where the comment starts), the `/** */` case, and the blank line of FMT-BLANK-004.
  - The `Why` now cites maintainer answer 2.
  - New example (`case 1:` to `case 5:`): the first section, later sections, and labels, on both sides of the condition.
- FMT-BREAK-034 — Each label of a switch section starts its own line — changed: a label behind such a comment is placed as FMT-BREAK-033 places a section.
- FMT-BREAK-002 — Braces start their own line — clarified: the first section of a switch behind such a comment is placed as FMT-BREAK-033 states.
- SHARED-COMMENT-012 — A comment that spans several lines belongs to the line it starts on — changed.
  - The formatter clause now names a statement, a switch section, and a switch label as its exceptions, each with its condition.
  - The `Why` is updated the same way.
  - `Uses` gains FMT-BREAK-033 and FMT-BREAK-034.
  - The clause for analyzer rules is unchanged.
- Change doc `sdd/changes/2026-11/phase-order-line-breaks.md`:
  - Maintainer answer 2 gains a paragraph on the lead's extension to sections and labels, with an example of unchanged output.
  - The decision section *A statement behind a multi-line comment's closing line* gains a paragraph on sections and labels.
  - FMT-BREAK-033 and FMT-BREAK-034 are added to `Affected requirements`.
  - `Visible output changes` is unchanged because no output changes.

`scripts/test.sh --project architecture`: Passed 92, Failed 0.

## Examples for the tests
Tag each test FMT-BREAK-033 or FMT-BREAK-034 as listed, plus SHARED-COMMENT-012. Tests the existing tagged suite already covers need no second test. Outputs are from head, the same on `main`, and stable on a second run.

The "stays" side (the comment starts on a line of its own) is new and needs tests:
1. A section behind `*/` after a `return` section: `return 1;⏎/* b⏎ */ case 2:` stays (FMT-BREAK-033).
2. A section behind `*/` after a `break` section: `break;⏎/* z⏎ */ case 2:` stays, and a blank line is inserted above `/* z` (FMT-BREAK-033 with FMT-BLANK-004/018).
3. A label behind `*/`: `case 1:⏎/* l⏎ */ case 2:` stays (FMT-BREAK-034).
4. The first section behind `*/`: `{⏎/* f⏎ */ case 1:` stays (FMT-BREAK-033/FMT-BREAK-002).
5. A multi-line `/** */` comment behind a label, `case 1: /** l⏎ */ case 2:`, becomes `case 1:⏎/** l⏎ */ case 2:`. The label stays behind `*/` (FMT-BREAK-034).

The "moves" side (the comment starts behind the token in front) may already be covered. FMT-BREAK-033's existing example covers item 6.
6. A section after `return`: `return 1; /* a⏎ b */ case 2:` becomes `b */⏎case 2:`. No blank line is inserted (FMT-BREAK-033).
7. A section after `break`: `break; /* a⏎ b */ case 2:` becomes `b */⏎⏎case 2:`, with the blank line below the comment (FMT-BREAK-033 with FMT-BLANK-004).
8. A label: `case 1: /* l⏎ */ case 2:` becomes `*/⏎case 2:` (FMT-BREAK-034).
9. The first section behind the opening brace: `{ /* f⏎ */ case 1:` becomes `*/⏎case 1:` (FMT-BREAK-033/FMT-BREAK-002).

FMT-BREAK-033's new example, covering items 1, 3, 6, 8, and 9 together, can be one full-pipeline test. Add a second pass if the test class checks idempotency.

## Output questions
_None._

## Internal choices
- I stated the placement once, in FMT-BREAK-033. FMT-BREAK-034 and FMT-BREAK-002 point to it. Alternative: restate the condition in each requirement.
- I stated the FMT-BLANK-004 blank-line placement in FMT-BREAK-033 because both sides are visible in the example inputs. Alternative: leave it to FMT-BLANK-018 alone.

## Follow-up candidates
- **Formatter · a member behind the closing line of a multi-line comment.** This is outside decision 38, which covers sections and labels.
  - FMT-BREAK-007 says every member starts its own line, and SHARED-COMMENT-012's formatter clause tells the formatter to break in front of a member behind `*/`. The formatter on head keeps such a member behind the `*/` whether the comment starts on its own line or behind the previous member's code. Only head was checked, not `main`.

    Input and real output (unchanged):
    ```csharp
    class C
    {
        /* a
        */ int _a;
        int _b; /* b
        */ int _c;

        void M()
        {
        }
        /* m
        */ void N()
        {
        }
    }
    ```
  - Reading A (recommended): keep the members behind the `*/`, as answer 2's reasoning suggests, and state it in FMT-BREAK-007 and SHARED-COMMENT-012. Output is unchanged.
  - Reading B: follow the spec as written and break in front of `int _a;`, `int _c;`, and `void N()`, for example `int _b; /* b⏎ */⏎int _c;`.
  - Recommendation: A, because it changes no output. The lead decides whether answer 2 covers this or it is filed. SHARED-COMMENT-012's formatter clause stays general, so other constructs a requirement asks to start their line may diverge the same way. I did not survey them.
