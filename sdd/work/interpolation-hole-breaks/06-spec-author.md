Role: spec author
Commit: e7a934c
Verdict: spec written

Clarification for decision 6 in `00-lead.md`; no change doc (C# 11+ output unchanged; the C# 10 output is the one FMT-PIPE-003 already demands). The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored.

## Changed requirements

- FMT-SWITCH-017 — A switch inside a non-verbatim interpolation is left unbraced before C# 11 — added (clarification) in `sdd/specs/formatter/switch-case-braces.md`; ID added to `sdd/specs/requirement-ids.txt` at its sorted position; `FMT-PIPE-008` and `FMT-BREAK-044` added to the spec's `Uses`. No new code owner (`SwitchCaseBracePhase`/`SwitchCaseBraceRewriter` already own the spec).
- FMT-BREAK-044 — clarified: its "every other phase's requirements still apply inside the interpolation" now adds that the switch-case brace phase braces no switch there (FMT-SWITCH-017); `FMT-SWITCH-017` added to the line-break spec's `Uses`.

The position and its conditions are referenced from FMT-BREAK-044, not restated. Qualifiers with both sides in one example (FC-002): *not verbatim* (`a` vs `b`), *inside the interpolation* (`a` switch in a lambda in the hole vs `c` switch that merely contains `$"{x}"`), *C# 10 or earlier* (C# 10 vs C# 11 output). The whole switch stays unbraced, not only the multi-line section, because FMT-SWITCH-001 gives all sections one shape.

Example checked without building (per the lead's instruction), with the CLI already built under `src/Reihitsu.Cli/bin/Debug/net11.0` (built 18:49 from the current head, before any fix), copied to the scratchpad and run with `--lang-version 10` and `--lang-version 11`: the C# 11 block is exactly today's output; the C# 10 output today equals the C# 11 one, and the spec's C# 10 block differs from it only on line `a`, which keeps the input as written (the expected value the test pins). The input compiles under C# 10.

Not run: `scripts/test.sh --project architecture` (builds). Expected: `EveryRequirementOfAnEnforcedSpecIsCovered` fails for FMT-SWITCH-017 until a test carries its tag; Uses rows were updated by hand to match the references.

## Examples for the tests

- FMT-SWITCH-017 — add `[Requirement("FMT-SWITCH-017")]` to `NonVerbatimInterpolationLineBreakTests.SwitchSectionWithTwoStatementsInInterpolationStaysAsWrittenInCSharp10` (keep FMT-BREAK-044; FMT-PIPE-003 may stay). The spec's `b`/`c`/C# 11 sides are today's output and are optional extra tests.

## Output questions

_None._

## Internal choices

1. New requirement FMT-SWITCH-017 in the switch-case brace spec rather than a sentence under FMT-SWITCH-001 or FMT-BREAK-044 (alternatives): the test needs an ID of the phase that leaves the switch alone, as the guards FMT-SWITCH-010/011/014 have theirs.
2. Referenced FMT-BREAK-044 for the position ("at any depth", verbatim nesting) instead of restating it.

## Follow-up candidates

_None_ beyond those already in `00-lead.md`.
