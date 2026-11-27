Role: spec author
Commit: 4188896 (the statements were derived from this head; auditor's evidence at 498009c)
Verdict: spec written

Repair round 1, auditor finding #3 (`11-auditor.md`, #1698). My prompt carried no intended fix or hoped-for verdict beyond the auditor's suggestion, which I re-derived rather than copied.

## Changed requirements

- FMT-RAW-007 — Hole lines move only with the literal unless aligned within it — clarified. The second bullet no longer paraphrases FMT-INDENT-006's columns. It now reads: "the formatter MUST give it the column FMT-INDENT-006 gives it in front of the token it precedes, measured from that token's column after this requirement placed it." The old paraphrase ("the column of the token it precedes, or one level deeper than a closing brace it precedes") contradicts FMT-INDENT-006 for a regular string's interpolation brace nested in a raw hole. It was also incomplete before this run: it left out the closing delimiters of a collection expression, list pattern, positional pattern (one level deeper), and parenthesized pattern (the inner pattern's column). Adding the interpolation exception would have restated a rule that still has three more cases, so referencing the rule is the fix (FC-015). No visible output changes: the requirement still defers to FMT-INDENT-006, and the code already follows that (auditor fixture `f3/H.cs`).
- `sdd/changes/2026-11/interpolation-close-brace-comment-column.md`: the Affected requirements row now lists `FMT-RAW-007 (clarified)`.

## Other restatements checked

`rg` across `sdd/specs` and `documentation` for phrases that restate the closing-brace comment rule:
- `sdd/specs/analyzer/RH5204.md` RH5204-002 ("in front of a closing brace, from one level deeper than the brace (FMT-INDENT-006)") still holds. It only covers comments in front of a line that RH5204-001 checks, and RH5204-001 checks only braces of a namespace, type, block, accessor list, or switch statement, never an interpolation brace. The auditor's guard-scope row (b) confirms the code path cannot reach it, and the RH5204 parity test pins it. Left unchanged.
- `sdd/specs/shared/initializer-layout.md` SHARED-INIT (line 23, "in front of a closing brace, one level (four columns) deeper") covers only initializer braces, where FMT-INDENT-006 still gives one level deeper. Left unchanged.
- `indentation.md:424` (the FMT-INDENT-006 example lead-in) already states the new rule. Nothing else restates it.

`scripts/test.sh --project architecture`: 198 passed.

## Examples for the tests

None new. The FMT-RAW-007 examples did not change, and the case is already pinned by `InterpolationClosingBraceCommentTests` (regular string nested in a raw hole, with the raw hole's own brace next to it).

## Output questions
_None._

## Internal choices
- Dropped the paraphrase instead of adding the interpolation exception to it, which was the auditor's alternative. A restatement would have to list every FMT-INDENT-006 delimiter case to be correct, and the rulebook says to reference rather than restate.

## Follow-up candidates
_None._
