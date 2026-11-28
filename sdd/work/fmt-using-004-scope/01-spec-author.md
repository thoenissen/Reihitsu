Role: spec author
Commit: d346a7f
Verdict: text written

## Changed requirements

- FMT-USING-004 — Unsafe blocks only get one directive per line — clarified: the requirement sentence now binds "the using phase" instead of "the formatter", and states that blank lines another requirement writes inside such a block, such as the frame of a `#region` / `#endregion` (FMT-BLANK-013), are not affected. Why gains one sentence on the reason for the scope. FMT-BLANK-013 added to the spec's `Uses` row. Example unchanged.
- No change doc: a clarification of existing behavior (`sdd/README.md`, *When a change doc is required*). No visible output changes.

Checked against today's code: formatting the block
`using System.Linq; #region Usings / using System; / #endregion / using Microsoft.CodeAnalysis;` keeps the order and adds blank lines around both region directives (and the `// Usings` end comment), as the new sentence states.

`scripts/lane-check.sh docs --head worktree`:
- `TEXT-ONLY PROOF: PASS — base=707d4b8 head=worktree — 2 changed file(s) (2 non-compiled, 0 C# comment/layout-only); xml-doc impact: none`
- `LANE CHECK: confirmed — claimed docs, diff needs docs`

`scripts/test.sh --project architecture`: 198 passed.

## Examples for the tests

None — no test changes (the existing FMT-USING-004 tests stay valid).

## Output questions

_None._

## Internal choices

- Followed the maintainer's narrowed scope: SHARED-USING-009 in `sdd/specs/shared/using-directive-ordering.md` already scopes the blank-line statement to the using phase and names FMT-BLANK-013, so it was left unchanged; the issue's original mention of `sdd/specs/shared/code-fixes.md` is superseded by the comment (SHARED-USING-009 is not in that file).
- Kept the requirement title as is; alternative was to add "in the using phase" to it, rejected as the title already describes what the phase does.

## Follow-up candidates

_None._
