Role: spec author
Commit: 948b71e
Verdict: spec written

Repair round 2, the spec conflicts in `16-test-implementer.md` (findings A and B, and the parenthesized-pattern `)` tests), decided by the lead in `00-lead.md` rows 24–26. The prompt named the changes it wanted. It carried no suspected cause and no verdict to aim for. I checked each change against the specs and against formatter runs before writing it. The runs used the CLI built from `main` (741157c) and from the branch's production code (39521e5; every later `src` change is test-only or a comment), two passes each, inputs in the session scratchpad under `r17/`. `scripts/trace.sh` confirmed the phase reversal in finding A.

## Changed requirements

- FMT-BLANK-020 — Behind a comment on its own line, no blank line before a brace or closing bracket — example corrected. The third example's output now reads `L =⏎⏎// c⏎{`: FMT-BLANK-011 puts a blank line above the comment that follows `=`. The caption says so. The normative text is unchanged. FMT-BLANK-011 is unchanged and has no exemption for a comment after `=`.
- FMT-SWITCH-007 — Comments survive brace insertion — changed (clarified, example corrected):
  - A comment written in front of the terminal `break` on the `break`'s line is glued to it (FMT-BLANK-018) and gets no FMT-BLANK-011 blank line.
  - No statement requirement inserts one above the terminal `break` (SHARED-BLANK-004), so only an author's blank line is kept there.
  - The Why says the same.
  - The example "The gaps in front of trailing comments …" loses the blank line between `}` and `/* b1 */ /* b2 */ break; /* x */ // y`.
- Change doc `sdd/changes/2026-11/blank-line-sole-owner.md`:
  - FMT-SWITCH-007 (changed) is added to *Affected requirements*.
  - Entry 13 is rewritten: title, attribution, After `L =⏎⏎// c⏎{`, and the input without the author's blank line, which gives the same output.
  - New entry 19 covers a comment glued to the terminal `break` of a section the formatter braces.
  - The intro of *Visible output changes*, the FMT-BLANK-018 and FMT-BREAK-030 bullets of *What*, and the guidance paragraph of *Decisions* name entries 13 (the blank line above the comment) and 19 as covered by the guidance.
  - Status stays `Accepted`, because rows 24 and 25 record both changes as covered by the maintainer's guidance. Setting the status is the lead's call.

### Row 24 — comment behind the `=` of an assigned collection initializer

| Input | `main` | Branch production | Spec now |
|---|---|---|---|
| `L =⏎// c⏎⏎{` | unchanged | `L =⏎// c⏎{` (pass 2: BlankLinePhase inserts the line above `// c`, LineBreakPhase removes it) | `L =⏎⏎// c⏎{` |
| `L =⏎// c⏎{` | unchanged | unchanged (same reversal in every pass) | `L =⏎⏎// c⏎{` |
| `Child =⏎// note⏎{` (object initializer) | `Child =⏎⏎// note⏎{` | same | same |
| `int[] G =⏎// c⏎{ 1, 2 };`, `int H =⏎// c⏎1;`, `var y =⏎// c⏎1;`, `x =⏎// c⏎2;`, `X =⏎// c⏎1,` and `L =⏎// c⏎new List<int> {…}` in an object initializer | blank line above `// c` | same | same |

Only the assigned collection initializer's `{` lacked the blank line on `main`, and only because the line-break phase removed it. FMT-BREAK-030 already forbids that removal. With it gone, FMT-BLANK-011 applies as it does behind every other `=`. Both directions of entry 13 are visible changes from `main`. The blank line between `// c` and `{` is answer 2A. The blank line above `// c` falls under the guidance (a comment between `=` and the assigned value).

### Row 25 — glued comment in front of the terminal `break`

On `main` and the branch, `B();⏎/* b */ break;` in a section that gets braced gives `}⏎⏎/* b */ break;`. FMT-BLANK-018 decides the line as the one directly before `break`. SHARED-BLANK-003 and FMT-BLANK-004 exempt the terminal `break` (SHARED-BLANK-004), so no blank line is inserted: `}⏎/* b */ break;` (entry 19). An author's blank line there is kept, and `main` keeps it too. An own-line `// before break` keeps its FMT-BLANK-011 blank line, as in FMT-SWITCH-007's second example.

### Row 26 — own-line comment and a blank line before a parenthesized pattern's `)`

FMT-BLANK-020 does not change `main`'s output here. It restores that output. On `main`, the blank line between an own-line comment and the `)` of a parenthesized pattern is removed for every comment shape:
- `// c`, `/* a */ /* b */`, `/* a */ // b`, `/* x⏎ y */ // b`, `/** d */ // c`;
- with the content `1 or 2`, `>= 1`, or `not null`;
- in `return o is (…);`, in `if (o is (…))`, in `is not (…)`, and in a switch-expression arm.

The branch's production code keeps the line for the four multi-comment shapes. That is auditor finding 2, which the implementer still has to repair. The branch removes it for `// c`. The auditor's "Hold: a parenthesized-pattern `)` (both keep the line)" fits only `o is (⏎1⏎// c⏎⏎)`. There, `(1)` is a constant pattern over a parenthesized *expression*, not a parenthesized pattern. FMT-BLANK-019 does not reach it either: with no comment, `o is (⏎1⏎⏎)` keeps its blank line on `main` and on the branch, while `o is (⏎>= 1⏎⏎)` loses it. FMT-BLANK-020 names only the parenthesized pattern, so that case keeps `main`'s output too. No spec text changed for row 26.

## Examples for the tests

The test expectations that change:

1. `CommentBeforeBraceBlankLineTests.RemovesBlankLineBehindCommentLineBeforeAssignedCollectionInitializerAndOneLineComplexElement` (the FMT-BLANK-020 spec example): the expected output gets one blank line between `L =` and `// c`. Nothing else changes.
2. `CommentBeforeBraceBlankLineTests.RemovesBlankLineBetweenCommentAndOpeningBraceOfAssignedCollectionInitializer` (entry 13): the expected output gets one blank line between `L =` and `// c`. Its summary should also say that FMT-BLANK-011 puts a blank line above the comment.
3. New test, entry 13, tags FMT-BLANK-011 and FMT-BREAK-030: input `L =⏎// c⏎{⏎1⏎}` with no blank line anywhere. Expected `L =⏎⏎// c⏎{⏎1⏎}`, the same layout as test 2.
4. `BracedSwitchSectionCommentLayoutTests.TwoCommentsInFrontOfTheTerminalBreakStayOnTheBreaksLine`: remove the blank line between `}` and `/* b1 */ /* b2 */ break;`. The summary loses "and its blank line".
5. `BracedSwitchSectionCommentLayoutTests.CommentInFrontOfTheTerminalBreakStaysOnTheBreaksLine` (entry 19's input): remove the blank line between `}` and `/* b2 */ break;`. The summary changes the same way.
6. `BracedSwitchSectionCommentLayoutTests.SpecificationExampleKeepsEveryCommentLayoutOfTheBracedSections` (the FMT-SWITCH-007 spec example): remove the blank line between `}` and `/* b1 */ /* b2 */ break; /* x */ // y`.
7. Optional, the other side of FMT-SWITCH-007's new sentence: `B();⏎⏎/* b */ break;` gives `}⏎⏎/* b */ break;`. The author's blank line is kept.

Unchanged: the four `CommentBeforeBraceBlankLineTests.RemovesBlankLineBehind…BeforeParenthesizedPatternParenthesis` tests. They expect `main`'s output, and the implementer's finding-2 repair makes them pass.

For the implementer:
- With FMT-BREAK-030, the line-break phase must stop removing the blank line above `// c` behind `L =`.
- The switch-case-brace phase must not insert the FMT-BLANK-011 blank line above a comment glued to the terminal `break`, using the one glued-comment predicate.

## Output questions

_None._ The lead's rows 24 and 25 decide both visible changes as covered by the maintainer's guidance. Row 26 changes nothing visible.

## Internal choices

- Entry 19 is a new change-doc entry rather than part of entry 17. Entry 17 shows comments glued in ordinary blocks, while entry 19 comes from brace insertion, which has its own requirement (FMT-SWITCH-007). The alternative was one more paragraph in entry 17.
- Row 26 adds no sentence to FMT-BLANK-019 or FMT-BLANK-020 about `o is (1)`. The requirements already name the parenthesized pattern by syntax, and a parenthesized expression is not one. The alternative was a clarifying sentence that would only restate the syntax.

## Follow-up candidates

_None new._
