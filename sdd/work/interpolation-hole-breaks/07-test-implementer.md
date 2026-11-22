Role: test implementer
Commit: eecea05
Verdict: tests written

Corrections for decisions 7 and 8 in `00-lead.md`, plus the FMT-SWITCH-017 tag from `06-spec-author.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored. Only the six tests below changed; every other test from `03-test-implementer.md` and `04-test-implementer.md` is unchanged.

## Tests

File `tests/Reihitsu.Formatter.Test/Regression/LineBreaks/NonVerbatimInterpolationLineBreakTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| IfStatementInInterpolationGetsBracesOnItsLineInCSharp10 → expected `if (b) {return 1; }return 2;` (decision 7) | FMT-BREAK-044 | `AssertResult` → `AssertRuleResult` | pass |
| LocalFunctionInInterpolationGetsBlockBodyOnItsLineInCSharp10 → `int G() {return 1; } return G();` (decision 7) | FMT-BREAK-044 | `AssertResult` | pass |
| LocalFunctionConstraintInInterpolationKeepsItsLineInCSharp10 → `int G<T>() where T : class {return 1; } return G<string>();` (decision 7) | FMT-BREAK-044 | `AssertResult` | pass |
| LocalFunctionAttributeInInterpolationKeepsItsLineInCSharp10 → `[A] int G() {return 1; } return G();` (decision 7) | FMT-BREAK-044 | `AssertResult` | pass |
| UsingStatementInInterpolationKeepsItsLineInCSharp10 (renamed from `UsingStatementInInterpolationGetsBracesOnItsLineInCSharp10`, since it no longer gets braces) → input unchanged (decision 8) | FMT-BREAK-044 | `AssertKept` | pass |
| SwitchSectionWithTwoStatementsInInterpolationStaysAsWrittenInCSharp10 — tag added, assertion unchanged | FMT-BREAK-044, FMT-PIPE-003, FMT-SWITCH-017 | `AssertKept` | pass |

Derivation of the decision-7 outputs, from the requirements alone (not from the implementer's observed output, which they match):

- FMT-STRUCT-026: the transform writes no separator between an inserted `{` and `return` (they read as two tokens), none behind `)` before `{`, and none between an inserted `}` and a following statement; the braced statement's trailing space stays in front of the inserted `}` (`if`), and the trailing space of the converted body's `;` moves to the created `}` (local functions).
- FMT-SPACE-009: one space before every non-interpolation `{` and `}` — hence `) {` and `; }`; it does not decide the space after `{`.
- FMT-SPACE-002: `{`→`return` and `}`→`return` are pairs no rule covers (FMT-SPACE-012 governs the space *after* `return`), so no gap is inserted; `} return G()` in the local-function tests keeps the carried space, `}return 2;` in the `if` test has none to keep.
- Decision 8: FMT-STRUCT-001 braces only `if`/`else` bodies, and FMT-BREAK-044 forbids every line break in the interpolation, so the `using` statement stays as written.

## Scenarios (bug fix)

_N/A — corrections only; the reproductions from `03-test-implementer.md` are unchanged._

## Not covered, and why

- The `}return 2;` / `{return` gaps at C# 11: there the braces always get their own lines (FMT-BREAK), so the gap never shows; unchanged by the run.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~NonVerbatimInterpolationLineBreakTests"
scripts/test.sh --project architecture
```

Results on this head plus this change (Debug): formatter 7229 passed, 0 failed (all 41 `NonVerbatimInterpolationLineBreakTests` pass); architecture 198 passed, 0 failed (`EveryRequirementOfAnEnforcedSpecIsCovered` now finds FMT-SWITCH-017 covered).

## Follow-up candidates

_None._
