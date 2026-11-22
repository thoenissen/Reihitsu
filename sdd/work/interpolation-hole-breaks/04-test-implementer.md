Role: test implementer
Commit: 6914c6c
Verdict: tests written

Amendment for decision 6 in `00-lead.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored. The only change is the one test below; every other test from `03-test-implementer.md` is unchanged.

## Tests

File `tests/Reihitsu.Formatter.Test/Regression/LineBreaks/NonVerbatimInterpolationLineBreakTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| SwitchSectionWithTwoStatementsInInterpolationStaysAsWrittenInCSharp10 (renamed from `SwitchSectionWithTwoStatementsInInterpolationGetsNoLineBreakInCSharp10`; input `var s = $"a {F(() => { switch (x) { case 1: G(); G(); return 1; default: return 2; } })} b";`, `int x`) — now asserts the exact output: the input unchanged, switch unbraced, everything on the statement's line | FMT-BREAK-044, FMT-PIPE-003 | `AssertKept` → `AssertRuleResult` (LF+CRLF, second pass, phase stability) | fail |

Expected value per decision 6: the switch-case brace phase leaves such a switch as written below C# 11, and FMT-BREAK-044 forbids every line break inside the interpolation; spacing and blank lines are already canonical, so the exact output equals the input. Tag FMT-BREAK-044 for the "no line break" half; FMT-PIPE-003 kept as the reason decision 6 gives for inserting no braces (braces on their own lines would be CS8967). No spec requirement states the switch-phase exemption itself; the spec checker should note that decision 6 is pinned only by this test.

Observed on this head (fails for the right reason): first difference is the line break written after `=>` inside the interpolation; the actual output also braces both sections (`case 1:⏎{⏎G();⏎G();⏎⏎return 1;⏎}`), which this test now forbids.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Multi-statement switch section in a lambda in a `$"` interpolation, C# 10 | LF, CRLF | formatter pipeline | reproduced | block, switch and sections broken and braced; expected the input unchanged |

## Not covered, and why

- The verbatim (`$@"`) and C# 11 sides of this shape: unchanged by the run, and the existing switch-case brace tests pin braced output for multi-line sections outside such interpolations.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~NonVerbatimInterpolationLineBreakTests.SwitchSection"
```

## Follow-up candidates

_None._
