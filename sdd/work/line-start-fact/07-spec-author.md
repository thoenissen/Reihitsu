Role: spec author
Commit: d5ce186
Verdict: change doc written

This repair implements the maintainer's answer 2B in `squad/decisions/2026-10-30-line-start-fact-implementer-findings.md` and the `AGENTS.md` item of `06-implementer.md`. The change doc went back to `Status: Draft` for this amendment. Its intro carries an amendment note, and each changed passage is marked *(amended)*. The maintainer approved the amended passages (`squad/decisions/2026-10-30-line-start-fact-amended-change-doc.md`), and the status is `Accepted` again.

## Re-derived on the branch (cb59e98 production code), `scripts/format.sh --force`

| Input (line break at `/`) | Branch output today | After 2B |
|---|---|---|
| `typeof(…Dictionary<,` / `>)` | stays wrapped, `>` in column 8 | joined: `typeof(…Dictionary<,>)` (FMT-BREAK-014) |
| `typeof(System.Func<` / `,,>)` | stays wrapped, `,` in column 8 | joined: `typeof(System.Func<,,>)` |
| `typeof(…Dictionary<, // c` / `>)` | stays wrapped, `>` in column 8 | unchanged: the comment blocks the join |
| `new …List<int // c` / `>()` (control with text) | stays wrapped, `>` aligned to `int` | unchanged |
| `int[,` / `] a = null;` | stays wrapped, `]` in column 8 | unchanged: no requirement joins a rank specifier |
| `int[, // c` / `] a = null;` | stays wrapped, `]` in column 8 | unchanged |

So the zero-width rule still decides two shapes:
- a type argument list of omitted type arguments that a comment or a directive keeps wrapped;
- every wrapped rank specifier with an omitted size.

On `main` the commented list puts `>` in column 16 (skeptic matrix, `02-skeptic.md`).

## Changed requirements

- **Change doc** `sdd/changes/2026-10/line-start-fact.md` is back to Draft.
  - The intro has an amendment note.
  - `Affected requirements` adds FMT-INDENT-015.
  - Amended passages:
    - the *Why* bullet on tokens without text;
    - two bullets under *Outputs that change*: the join, and what stays wrapped;
    - the *Bugs fixed* intro and its FMT-BREAK-014 and FMT-INDENT-015 entries.
- **FMT-INDENT-015** — Attribute lists and angle-bracket lists align to their first element — clarified.
  - Block indentation now applies to a list of omitted type arguments only "that stays wrapped, because a comment or a directive in it keeps FMT-BREAK-014 from joining it".
  - New example: `Dictionary<, // c` / `>` goes from column 64 to column 8.
  - `Uses` adds FMT-BREAK-014.
- **SHARED-BLANK-008** — its omitted-type-argument example now carries `// c`, so the list stays wrapped. The input has `>` in column 0 and the output has `>` in column 8.
- **FMT-BREAK-014** — text unchanged. It already requires the join; the branch violates it, and the change doc lists this under *Bugs fixed*.
- **`AGENTS.md`**, `IndentationPhase` row. The guards now read `LayoutComputer.Compute`; `IndentationRewriter.Apply`; `LineStartIndex`; `RawStringLineShifts`. Removed: "previous-token trailing-EOL detection" and `RebuildLeadingTrivia`, which does not exist in `src/`. I checked every other name in the table against `src/` and all of them exist.

Architecture project (`scripts/test.sh --project architecture`): 89 passed, 0 failed.

## Examples for the tests

- FMT-BREAK-014: `typeof(Dictionary<,` / `>)` becomes `typeof(Dictionary<,>)`, and `typeof(Func<` / `,,>)` becomes `typeof(Func<,,>)`. These are the change doc's *Outputs that change* and *Bugs fixed* entries.
- FMT-INDENT-015: the new example, `Dictionary<, // c` / `>` with `>` going to column 8.
- SHARED-BLANK-008: the amended example.
- These tests now expect output this amendment changes:
  - `ZeroWidthTokenLineStartTests` rows without a comment in the list, such as `ClosingAngleBracketBehindOmittedTypeArgumentTakesBlockColumn`, and the `Dictionary<` / `,>` and `Func` variants;
  - `WrappedAngleBracketListTests.UnboundGenericWithZeroWidthAnchorIsNotAlignedToADegenerateColumn`.

  The omitted-array-size rows stand.

## Open questions for the maintainer

_None._ The amendment itself needs approval (escalation 2).

## Notes for the lead

- The issue's *Desired Formatted Output* for its second example shows `>` wrapped in column 8. After 2B the run ships `typeof(Dictionary<,>)` instead, and the PR description has to say so.
- `LineStartIndex` lives in `Pipeline/Indentation/Utilities/`, and `BlankLines/Rewriter/BlankLineTriviaBoundaryRewriter` and `LineBreaks/Utilities/TokenGapNormalizer` reference it. That is a type from another phase's folder, which the issue's *Done when* rules out. This is for the implementer; I named it as it stands in `AGENTS.md`.
