Role: spec author
Commit: a4160a0
Verdict: change doc amended (Status stays Accepted, amended passages marked); spec written; no open question

Inputs: `02-skeptic.md` and the five decisions `squad/decisions/2026-10-31-phase-tree-round-trip-{member-body-braces,conditional-trim,carried-comment-whitespace,annotation-scope,trailing-comma}.md` (all answered A, except member-body braces and conditional trim, answered B). My prompt carried no intended fix to ignore.

## Change doc status

`sdd/README.md` says `Accepted` once the maintainer has approved. The amendments carry answers the maintainer already gave, so the status stays `Accepted`. As in `sdd/changes/2026-10/line-start-fact.md`, an amendment note under the header names the decision records, and every changed passage is marked *(amended 2026-10-31)*.

One part of the amendment is derived rather than chosen by the maintainer: the final-output list under *What* follows from the answers and from the skeptic's evidence. The lead may want to show the maintainer that list before the PR is marked ready, in particular the `return/* c */ 42;` case below.

## Decisions carried in

- **Member-body braces (B):** the change doc now states that one path places member-body braces and the final output does not change. I added no spec statement, because which rewriter places them is not observable; the FMT-BREAK brace requirements keep deciding the output. Code owners are unchanged. If the implementer removes a rewriter type, `line-breaks.md` and `sdd/specs/README.md` must drop it.
- **Conditional trim (B):** FMT-BREAK-029 is now a MUST NOT. When the phase writes a line break in front of a moved token and leaves the trailing trivia of the token before in place, it does not remove the whitespace at the end of that trivia, wherever that token stands. FMT-CLEAN-001 is the only owner. A line break the phase moves out of the previous token's trailing trivia (list rewrapping) is outside the requirement.
  - The example asserts that the line-break phase's output keeps the space behind `if (true)` and behind `void M()`.
  - Today the trace trims the one behind `void M()` (see `01-spec-author.md`), so this is the phase-output test that fails first.
- **Carried-comment whitespace (A):** FMT-STRUCT-026 now states that the whitespace in front of and between carried comments stays as the author wrote it. I removed the earlier "may be dropped" for it. The only things that may still be dropped are the line breaks in front of the first leading comment and the trivia of a removed token where no carried comment stands next to it. The final outputs that change are listed in the change doc's *What* and shown in FMT-STRUCT-026's third example.
- **FMT-PIPE-022 scope (A):** the requirement now covers nodes and tokens of the code only, not trivia and not structured trivia. A created token that replaces a removed one of the same kind (the expression body's `;`) carries none of the input's annotations. The raw-string sentence now names the start, content, and closing tokens and every node and token inside the holes, and no longer names hole whitespace.
- **Trailing comma (A):** new requirement **FMT-STRUCT-027**. When the formatter removes a trailing comma, a comment of that comma that stands below the line the last element ends on stays on its own line and leads the closing brace. I derived this from the skeptic's evidence:
  - The text already gives such a comment to `}`. A comment behind `B // about B` cannot join that line, and joining one behind `2 /* two */` would move a comment between lines, which no requirement asks for.
  - FMT-STRUCT-027's expected output is today's converged output. I confirmed it by running `scripts/format.sh` three times on the skeptic's two inputs; the result is stable after pass 2.
  - With the comment led by `}` in the transform's tree, the phase output's text is identical to today's pass-1 text, so one pass gives that output.

## Skeptic corrections S1–S4

- **S1 (confirmed, fixed):** the separator list of FMT-STRUCT-026 now requires a line break between an inserted `}` and a comment that leads the next token (`// about Bar` before `Bar();`). The "none behind an inserted `}`" sentence names only `else`, a statement without a leading comment, and the enclosing `}`. Added to the second example, with its final output.
- **S2 (confirmed, fixed):** FMT-PIPE-021 now reparses with the parse options of the syntax tree the formatter was given: its language version and preprocessor symbols as they stand, not as FMT-PIPE-008 resolves them. The Why says why (`#if DEBUG`), and so does the change doc (also the `Preview` clamp).
- **S3 (confirmed, fixed):** *What* lists `Foo(); /* a */ // b` (today `/* a */// b`). It also lists the gap changes `Foo();    // x` and `Foo();// y`, and `int Get() =>` followed by an unindented `/* c */ 42;` becoming `return/* c */ 42;`. That last case follows from decision A and the skeptic's probe that the spacing phase keeps a zero gap in front of a block comment. The indented form stays `return /* c */ 42;`.
- **S4 (resolved without a maintainer choice):** both rules keep what the author wrote, so neither is a new layout decision. Today's code already does the expression-body part (`ExpressionBodyToBlockConverter.CreateBlock`).
  - FMT-STRUCT-026 states that a braced statement keeps its own trailing trivia, line break included, so a line break ends its line in front of the inserted `}` exactly where the author wrote one.
  - The created `}` of a converted expression body takes the replaced `;`'s trailing trivia, line break included.

Checked by running `scripts/format.sh` (to a stable result) on every input block of FMT-STRUCT-026 and FMT-STRUCT-027 at a4160a0. The outputs match the spec's blocks except for the intended changes: `/* x */ /* y */` on one line, and the three trailing-comment gaps. In code that is already braced, today's formatter keeps `/* x */ /* y */ Foo();` and a blank line between two leading `//` comments. That shows the target outputs are what the later phases produce once the transform keeps the trivia.

## Changed requirements

- FMT-STRUCT-026 — The phase writes only the separators its text needs — changed (S1, S4, carried whitespace, field-split exception now names its comment spaces, examples 2–4)
- FMT-STRUCT-027 — A removed comma's comment on a line of its own stays there — added
- FMT-PIPE-021 — Every phase returns a tree its text parses back to — changed (parse options, Why)
- FMT-PIPE-022 — Every phase keeps the annotations it does not own — changed (scope: nodes and tokens of the code)
- FMT-BREAK-029 — Whitespace in front of an inserted line break is left to the cleanup phase — changed (MUST NOT trim; example asserts the phase output)
- The ledger has FMT-STRUCT-027.

## Examples for the tests

- FMT-STRUCT-026: all four examples. Examples 2 and 3 need final-output tests, and the phase-output statements need tests on `StructuralTransformPhase` output. Example 4 (`return` with eight spaces vs. none) needs both.
- FMT-STRUCT-027: the enum/array example. Assert both one-pass output and idempotency.
- FMT-PIPE-021: unchanged examples, plus a reparse with a preprocessor symbol (`#if DEBUG` with `DEBUG` defined in the parse options) for S2.
- FMT-PIPE-022: the raw-string example, with annotations on `Call` and the content text token. Tag the FMT-PIPE-017 third-case test with it as before.
- FMT-BREAK-029: the `void M() {` / `if (true) {` example, on `LineBreakPhase` output.

## Whole run

`scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"`: 26/26 passed.

## Open questions for the maintainer

_None._

## Follow-up candidates

The skeptic's two candidates (`SwitchCaseBraceRewriter` line breaks around inserted braces; `int Get2() => /* d */ 42;` gluing the comment behind `{`) are untouched by this round. FMT-STRUCT-026's carried-whitespace rule does not decide the second one's line placement.
