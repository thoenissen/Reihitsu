Role: spec author
Commit: 5033454
Verdict: spec written

Architecture repair, decision 17 in `00-lead.md` (supersedes decision 16), auditor finding #3 in `17-auditor.md`. Clarification only, no change doc: C# 11+ output is unchanged, and the C# 10 refusal is a local refusal for a rare class (`AGENTS.md`, *Code-fix coverage*). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decision 17 itself; nothing was ignored. No new requirement ID, no `Uses` or code-owner change.

## Changed requirements

- FMT-BREAK-044 — No line break is written into a non-verbatim interpolation before C# 11 — clarified in `sdd/specs/formatter/line-breaks.md`:
  - *Structural transforms* part rewritten: below C# 11 the FMT-STRUCT-001 brace transform wraps no `if` or `else` body when an interpolation of a non-verbatim interpolated string that contains the body holds a documentation comment anywhere between its braces; the body stays as written at every depth, also when the comment is nowhere near it. States the cause (FMT-STRUCT-026's separator is a line break, and for a nested body the next token depends on the outer wrap) and the holding sides: no doc comment in such an interpolation (braced), only another interpolation of the same string holds one (braced), only ordinary comments (braced). The general sentence for any other transform that would write a line break there is kept. The round-1 per-brace wording is gone, not kept beside it.
  - *Why*: one clause added — the brace transform refuses the whole interpolation by that cheap check instead of deciding each brace.
  - Second example extended: `u` (nested `if (b) if (!b) return 1; /** d */ return 2;`, finding #3 member 1), `v` (doc comment in the hole next to no brace: `… }) /** d */}`), `t` (`/* c */`, holding), `w` (doc comment only in a sibling interpolation `{/** d */ 1}`, holding); `x`, `y`, `z` unchanged; the C# 10 prose explains each.
- `sdd/specs/formatter/structural-transforms.md`, non-goal *Line breaks, spacing, and indentation…*: pointer reworded to name the brace transform's interpolation-level refusal.

Examples checked without building: the Release CLI built 19:41 (after a0888e4, the last `src` commit at that time), copied to the scratchpad, run with `--lang-version 10` and `--lang-version 11`. The C# 11 block is exactly today's output. Today's C# 10 output differs from the spec's C# 10 block only in `u` (`if (!b) {return 1; }/** d */`) and `v` (`if (b) {return 1; }return 2;`) — the two members decision 17 changes; every other line, including `t` and `w`, is today's output. Fences balanced. `scripts/test.sh --project architecture` not run (no build per prompt; no ID, `Uses`, or owner changed).

## Examples for the tests

- FMT-BREAK-044 — the second example: failing members `u` and `v` (C# 10 output = input), holding members `t` and `w` (exact C# 10 output above), plus finding #3 members (2) nested `else` and (3) three levels, which under the spec keep their input. The round-1 structural expectations stay as they are for `y`/`z`; any round-1 holding test whose hole also holds a `/** */` comment elsewhere now expects the `if` unbraced.

## Output questions

_None._ Decision 17 is a local refusal for a rare class.

## Internal choices

1. "Holds a documentation comment" is read for **any** non-verbatim interpolation that contains the body (so effectively the outermost such hole, which also covers a `"` hole nested inside a `$"` hole), not only the nearest enclosing interpolation. Alternative: the nearest non-verbatim interpolation. They differ only when a `$"` hole nests another `$"` hole and the doc comment stands in the outer one outside the inner one; there the alternative braces the inner `if`. Chosen because it matches decision 17's "anywhere in it" and FMT-BREAK-044's "at any depth"; if the implementer finds only the nearest-hole check cheap, the lead decides and I adjust the sentence (no example depends on it).
2. A doc comment in another interpolation of the same string does not refuse (decision 17 says "a doc comment in the hole"); pinned by `w`.

## Follow-up candidates

_None_ beyond those already in `00-lead.md`.
