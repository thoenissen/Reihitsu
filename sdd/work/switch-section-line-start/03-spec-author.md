Role: spec author
Commit: 6a84dcb
Verdict: spec written

Amendment after the maintainer's answers (`00-lead.md`, *Maintainer answers*) and the skeptic's handover (`02-skeptic.md`). Questions 1A, 2A, and 3B were carried in; nothing remains open.

## Changes

- `sdd/changes/2026-11/switch-section-line-start.md`:
  - `Status: Accepted`. Every visible output change it lists now has an answer, and the lead asked for this.
  - The pending wording is removed, and a `Maintainer answers` decision is added.
  - The labels decision's false FMT-SWITCH-006 claim is corrected: on `main`, a braced section keeps `case 1: case 2:` on one line.
  - A new decision covers the placement of the `/** */` comment.
  - Visible change 6 is added: a `/** */` comment in front of a first statement or a later label.
  - Entry 3 now says "ordinary comment".
  - Entry 5 now also names RH5403 and RH3208.
  - Alternatives are updated with the rejected options.
- `sdd/specs/formatter/line-breaks.md`:
  - FMT-BREAK-033 states the `/** */` rule once. A `/** */` comment on the line in front of the token, with nothing but whitespace between them, belongs to that token. The token starts its line when the comment does, and the inserted line break goes in front of the comment. `Why:` is extended. A new example covers `return 1; /** d */ case 2:`.
  - FMT-BREAK-034 refers to that rule. Its `Why:` no longer cites FMT-SWITCH-006, because the claim was false; it now cites FMT-BREAK-033. Its example adds `case 4: /** f */ case 5:`, which gives `case 4:` and then `/** f */ case 5:`.
  - FMT-BREAK-035 refers to that rule. Its example adds `case 2: /** d */ A();`, which gives `case 2:` and then `/** d */ A();`.
  - `Uses`: added FMT-BLANK-012 and FMT-DOC-001; removed FMT-SWITCH-006, which is no longer referenced.
- No other spec changed, and no new requirement IDs.

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"`: 36 passed. The one failure is the same expected coverage finding for FMT-BREAK-033, 034 and 035, which have no tests yet. The `Uses`, ledger and structure checks pass.

## Facts established for the `/** */` cases

All on `main` at 6a84dcb, using `scripts/trace.sh` and `scripts/format.sh`.

| Input shape | Output on `main` |
|---|---|
| `return 1; /** d */ case 2:` | `DocumentationCommentFormattingPhase` moves the comment onto its own line. `BlankLinePhase` adds the FMT-BLANK-012 blank line. The final line is `/** d */ case 2:`, stable after two passes. FMT-BREAK-033 keeps this, so the output does not change. |
| `case 4: /** f */ case 5:` and `case 1: /** d */ A();` | Unchanged. The documentation phase does not move them; that is thoenissen/Reihitsu#1533. |
| Target shapes `case 4:` then `/** f */ case 5:`, and `case 1:` then `/** d */ A();`, given as input | Unchanged. No blank line is added, so the new output is stable. |

The FMT-DOC-003 check: in both moved cases, the token behind the comment (`case`, `A`) opens the node the comment belongs to. That means FMT-DOC-003, which says stranded documentation is not moved, does not apply. FMT-DOC-001's intended placement is the same as the new output, so a later fix of thoenissen/Reihitsu#1533 would not conflict.

## Examples for the tests

- FMT-BREAK-033: the third example, which is new. It checks that the shape stays as it is on `main`, with the blank line, in the whole pipeline.
- FMT-BREAK-034: the extended example, with the `/** f */` section.
- FMT-BREAK-035: the extended example, with the `/** d */` section.
- Change-doc entry 6.

## Output questions

_None._ Under the lead's standing instruction, this amendment raises none.

## Internal choices

- The `/** */` rule is stated once, in FMT-BREAK-033, and FMT-BREAK-034 and FMT-BREAK-035 refer to it. The alternative was to repeat it in each requirement.
- "Belongs to the token" covers only a `/** */` comment on the token's own line with nothing but whitespace between them. A `///` comment already ends its line, so the token behind it starts its line (SHARED-BLANK-008), and nothing is moved.

## Follow-up candidates

Unchanged from `01-spec-author.md`. thoenissen/Reihitsu#1533 stays relevant: once FMT-DOC-001 moves these comments in the documentation phase, the line-break phase finds them already starting their line.
