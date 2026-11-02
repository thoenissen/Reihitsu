Role: spec author
Commit: 066bcf6
Verdict: spec written

Repair round 1, findings F1 and F2 of `09-auditor.md` as `00-lead.md` decision 13 routes them, on maintainer answers 8 and 9 (decided under the standing instruction). My prompt carried no intended fix or hoped-for verdict beyond that routing.

## Changes

- `sdd/changes/2026-11/switch-section-line-start.md` (F1, F2):
  - The intro of *Visible output changes* no longer says the maintainer decided every entry; it says the first questions were answered by him and the later ones under his standing instruction.
  - *Maintainer answers* splits entry 6: the first-statement half answered by him; the label half (answer 8) and the rule for which comments move (answer 9) listed as decided under the standing instruction, with their outputs and the rejected output.
  - *What* and entry 6 state the comment run rule; entry 6's example gains `case 6: /* c */ /** d */ /* e */ A();` → `case 6: /* c */`, then `/** d */ /* e */ A();`.
  - The decision *A `/** */` comment moves with the token…* names who decided which half and explains the parser boundary; two rejected alternatives added.
- `sdd/specs/formatter/line-breaks.md` (F2):
  - FMT-BREAK-033 restates the rule: the comments that move with a token are its own leading trivia on its line (a delimited comment counts on the line it starts on); on the previous token's line that is every comment from the first `/** */` comment up to the token, as written and in order; an ordinary comment in front of that first `/** */` comment trails the previous token and stays (FMT-BREAK-001); the line break goes in front of the first moving comment. `Why:` extended.
  - FMT-BREAK-034 and FMT-BREAK-035 restate their pointer to that rule consistently.
  - New example in FMT-BREAK-034: `case 1: /* c */ /** d */ case 2:` and `case 3: /** a */ /** b */ case 4:`.
  - New example in FMT-BREAK-035: `case 1: /* c */ /** d */ A();`, `case 2: /** d */ /* c */ A();`, and the multi-line `case 3: /** a⏎b */ A();`.
- No new requirement IDs; no rule page (no diagnostic or fix changes).

Every new example output is the branch's real output (`scripts/format.sh` at 066bcf6, fixtures in the scratchpad). `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"`: 37 passed.

## Changed requirements

- FMT-BREAK-033 — Every switch section starts its own line — changed (which comments move with the token)
- FMT-BREAK-034 — Each label of a switch section starts its own line — changed (pointer wording, example added)
- FMT-BREAK-035 — The first statement of a switch section starts its own line — changed (pointer wording, example added)

## Examples for the tests

- FMT-BREAK-034: the new example (`/* c */ /** d */ case 2:`, `/** a */ /** b */ case 4:`).
- FMT-BREAK-035: the new example (`/* c */ /** d */ A();`, `/** d */ /* c */ A();`, multi-line `/** a⏎b */ A();`).
- Change-doc entry 6, `case 6`.

## Output questions

_None._ Answers 8 and 9 cover F1 and F2; the spec states exactly those outputs.

## Internal choices

- The rule is stated once in FMT-BREAK-033 in terms of the token's leading trivia and the parser's trailing-trivia boundary, with the user-visible consequence spelled out ("from the first `/** */` comment on"). Alternative: describe only the visible consequence; rejected because "first content on the token's line" is what decides a multi-line `/** */` comment too.

## Follow-up candidates

_None new._
