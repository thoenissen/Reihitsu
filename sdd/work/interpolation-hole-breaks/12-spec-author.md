Role: spec author
Commit: 3e617ef
Verdict: spec written

Repair round 1, decisions 12 and 13 in `00-lead.md`; clarification only, no change doc (C# 11+ output unchanged; the C# 10 output is the one FMT-BREAK-044 and FMT-PIPE-003 already demand). The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored. No new requirement ID.

## Changed requirements

- FMT-BREAK-044 — No line break is written into a non-verbatim interpolation before C# 11 — clarified in `sdd/specs/formatter/line-breaks.md`. The sentence after the FMT-BREAK precedence is now a list of the other phases' parts, each taking precedence over that phase's requirements:
  - *Structural transforms* (new): a transform that would write a line break there — the FMT-STRUCT-026 separator between a token it creates and a comment that leads the next token, such as the `{`/`}` of the FMT-STRUCT-001 brace transform in front of `/** d */` — is not applied to that construct, which stays as written; constructs that need no line break there are still rewritten (`if (b) return 1;` is still braced).
  - *Documentation-comment phase* (new): moves no documentation comment onto a new line there (FMT-DOC-001); the comment stays where written.
  - *Switch-case brace phase*: unchanged content (FMT-SWITCH-017).
  - *Blank-line phase*: unchanged content.
  - The *Why* states the cause: in such an interpolation a documentation comment is the only trivia that can lead a token on the line (an ordinary `/* */` trails the token before it; `//`, directives, and disabled text end the line), and it is rare, so both phases leave the construct as written.
  - A second example: `x` (doc comment after a statement), `y` (`if (b) /** d */ return 1;`), `z` (`if (b) return 1; /** d */ return 2;`), `w` (no comment, still braced), under C# 10 and C# 11.
  - `Uses` gains FMT-SPACE-009 and FMT-STRUCT-001 (now referenced).
- `sdd/specs/formatter/structural-transforms.md`, non-goal *Line breaks, spacing, and indentation…*: pointer to FMT-BREAK-044 (a transform that would write a line break there leaves the construct as written). Pointer in a non-goal, not a use; `Uses` unchanged.
- `sdd/specs/formatter/documentation-comments.md`: new non-goal pointing to FMT-BREAK-044 (no comment moved onto a new line inside such an interpolation). Same; `Uses` unchanged.
- `sdd/specs/formatter/switch-case-braces.md` (decision 13, finding #2): the closing fence of FMT-SWITCH-017's C# 11 example is on its own line. All four touched specs have balanced fences.

Example checked without building: the Release CLI under `src/Reihitsu.Cli/bin/Release/net11.0` (built 19:12 from e9476ab, the last `src`/`tests` commit), copied to the scratchpad, run with `--lang-version 10` and `--lang-version 11`. The C# 11 block is exactly today's output. Today's C# 10 output breaks `x`, `y`, `z` (CS8967, the auditor's finding reproduces); the spec's C# 10 block keeps them as written (the expected value after the fix) and its `w` line is exactly today's output.

Not run: `scripts/test.sh --project architecture` (builds). Two `Uses` entries were added by hand to match new references; no requirement ID or code owner changed.

## Examples for the tests

- FMT-BREAK-044 — the new example (`x`/`y`/`z` failing members, `w` holding member, exact C# 10 outputs), plus the auditor's member list in `11-auditor.md` finding #1. Under the spec, every structural member (`if (b) /** d */ return 1;`, `else /** d */ return 2;`, `if (b) return 1; /** d */ return 2;`) keeps its `if`/`else` unbraced as written, and every doc-comment-phase member keeps its comment where written. Tag FMT-BREAK-044.

## Output questions

_None._ Leaving the construct as written for a rare class is a local refusal (AGENTS.md, *Code-fix coverage*; decision 12).

## Internal choices

1. Structural part stated as "the construct stays as written" (the brace wrap is withheld), not as "the brace is inserted without the line break" (alternative). Decision 12 asks for a refusal, not modelling, and the alternative would contradict FMT-STRUCT-026's decided separator with a second layout. If the implementer finds only the alternative cheap, the lead decides and I adjust the example.
2. The parts stay in FMT-BREAK-044 with pointers from the sibling specs' non-goals, as for the blank-line phase (08-spec-author.md), rather than new FMT-STRUCT/FMT-DOC requirements; the existing tag covers them.
3. Not changed: the auditor's hint that FMT-SWITCH-017 says "MUST NOT brace" while the code also skips unbracing; they agree through FMT-SWITCH-008, and no finding asked for it.

## Follow-up candidates

_None_ beyond those already in `00-lead.md`.
