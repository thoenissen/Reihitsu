Role: skeptic
Commit: 34ce68ed3aae4175b17207a05ccbf87465d1c9bd
Verdict: proceed narrower

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the issue's own questions; nothing to ignore.

## Premise

The issue's mechanism is wrong; the gap it describes exists, but for a different reason, and it is smaller than the issue states.

**Raising the warning level alone changes nothing.** Both verifiers (`src/Reihitsu.Analyzer.Test/Verifiers/CSharpAnalyzerVerifierTest{TAnalyzer}.cs`, `CSharpCodeFixVerifierTest{TAnalyzer,TCodeFix}.cs`) leave `CompilerDiagnostics` at the testing library's default, which a probe printed as `Errors`: compiler warnings are not compared in any state, at any level. Only nullable warnings reach the comparison, because `CSharpVerifierHelper.GetNullableWarnings` raises them to errors. Probe (scratch worktree, analyzer verifier, RH3002 as the analyzer, `scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~WarningLevelProbeTests"`), input:

```csharp
using System;

public class QueryableException : Exception
{
    public QueryableException Select(Func<QueryableException, QueryableException> selector) => this;
}

public class Test
{
    private QueryableException _exception;

    public int X => throw from exception in _exception select exception;
}
```

| Compilation options | `CompilerDiagnostics` | Result |
|---|---|---|
| default (level 4) | default (`Errors`) | passed |
| `WithWarningLevel(9999)` | default (`Errors`) | **passed** |
| level 4 | `Warnings` | failed: CS1591 ×4, CS0649 (no CS8848) |
| level 5 / 9 / 9999 | `Warnings` | failed: the same five, plus `warning CS8848: Operator 'from' cannot be used here due to precedence` at (12,27) |

So the issue's claim that the fixed state "fails at `WithWarningLevel(9999)`" does not hold for this harness: the probe behind the issue must have changed `CompilerDiagnostics` as well, or compiled the text outside the verifier. The first acceptance criterion, read literally (only the level changes), is a measured no-op. CS8848 is a wave-5 warning (it appears from level 5).

**No code fix today introduces a warning-wave diagnostic.** A scratch harness compiled the source and the fixed text of every `AnalyzerTestsBase<,>.Verify(source, fixedSource, …)` call at level 9999 against `Net90` reference assemblies and failed when the fixed text had more of a warning ID than the source. Full analyzer suite (10404 tests; baseline 10404 passed): 163 failures, **none with a wave ID**. Every introduced warning is an ordinary level-1–4 warning that the rule's own purpose produces:

| Tests | Introduced | Why it is intended |
|---|---|---|
| RH2001 (96) | CS0649, CS0169, CS0414, CS0108 | an auto-property becomes a field that the fixture never assigns or reads; CS0108 next to an inherited member is specified by RH2001-011 |
| RH2005 (50) | CS0169, CS0649 | a field becomes private and the fixture never reads it |
| RH3204 (8) | CS0219 | dropping `$` makes the string a constant, so an unused local now warns |
| RH5518 (4) | CS0657 | the input's one invalid `[field: …]` list on an accessor is split into two lists, so the existing warning counts twice |
| RH4103 `FixRenamesAMethodThatHidesAnInheritedMethodCalledOnlyOnTheBaseType`, RH4114 `FixRenamesAPropertyThatHidesAnInheritedPropertyReadOnlyOnTheBaseType` | CS0108 | specified by SHARED-CASING-009: the rename hides an inherited member |
| 3 Fix All scenarios (base class) | CS0219, CS0649, CS0169 | the same rules |

A check of "any warning the fix introduces" is therefore wrong for this repository: it contradicts SHARED-CASING-009 and the purpose of RH2001, RH2005, and RH3204.

**The one known instance is gone.** The issue's example (RH3002 unwrapping `throw (from …)`) was excluded by PR thoenissen/Reihitsu#863; `RH3002…AnalyzerTests.NecessaryParenthesesAroundQueryInThrowExpressionAreNotReported` now keeps the parentheses. The specs already treat CS8848 as a reason to refuse or parenthesize (`RH3004-005` Why, `FMT-STRUCT-011` Why). A spot probe of other fixes that remove parentheses around lambdas and anonymous methods in front of `+` (`scripts/apply-fix.sh RH3002` and `RH3106` on `Action a = (() => { }) + g;`, `Func<int> b = (delegate () { return 1; }) + h;`, `Action c = (delegate { }) + g;`) produced no unparenthesized operand: RH3002 reported nothing, RH3106 only dropped the empty parameter list inside the parentheses.

**What remains real:** a future code fix that writes a wave warning (CS8848, CS8981, …) into its result passes its own tests. It is a regression guard for one class of audit finding, not a present defect.

Reachability: the harness has no product caller; the question is only whether a code fix can write such a result. RH3002 could before thoenissen/Reihitsu#863, so the class is reachable in principle.

## Mechanism and candidates

Mechanism: the verifiers compare only diagnostics with error severity, so no warning of any level is checked in any state; the warning level decides which warnings exist, not which are checked.

Paths that compile a state and compare compiler diagnostics, all through the two verifier classes' constructors and `OnTransform`:

- `AnalyzerTestsBase<TAnalyzer>.Verify` → `CSharpAnalyzerVerifierTest` (analyzer-only tests; also the fixed text in `VerifyCodeFixIsWithheldOrProducesOneOf`, and the three passes of `FormatterTestsBase.Verify…`);
- `AnalyzerTestsBase<TAnalyzer, TCodeFix>.Verify` → `CSharpCodeFixVerifierTest` (source state, iterative and Fix All fixed states), reached by `VerifyCodeFixWithEndOfLine` and `BatchCodeFixTestsBase.FixAllCorrectsEveryDiagnostic`;
- direct `new CSharpCodeFixVerifierTest`/`CSharpAnalyzerVerifierTest` in `Naming/RH4102EventNameCasingAnalyzerTests.cs` and `Shared/CodeFixInteractionTests.cs`, which bypass the base classes.

The ad hoc paths (`TryApplyCodeFixAsync`, `TryApplyCodeFixAtAsync`, `GetCodeFixActionsAsync` through `CreateTestDocument`) compare no compiler diagnostics at all.

Warning waves known to the compiler in use (Roslyn 5.12.0, read by reflection from `ErrorFacts.GetWarningLevel`): wave 5 — CS7023, CS8073, CS8848, CS8880–CS8887, CS8892, CS8897, CS8898, CS9016, CS9017; wave 6 — CS8826; wave 7 — CS8981; wave 8 — CS9123, CS9125; wave 9 — CS9270; wave 10 — CS9265.

## Breaking inputs

FC-033 (a reproduction stays green because the harness normalized the scenario away) is the class of the issue itself: the proof test must be shown red before the harness change and green after, and it must fail on the **fixed** state, not on the source state.

Measured outcome of raising every wave ID to an error in **all** states (both verifiers, level 9999): 69 of 10404 tests fail, **all in "Diagnostics of test state"** (the input the user wrote), none in a fixed state:

- CS8981 (lower-case type name), 58 tests: RH4001, RH4002 (20), RH4003, RH4004, RH4005, RH4006, RH4010, RH4011, RH4012 (incl. its Fix All scenario), RH4105 (1), RH4121 (incl. its Fix All scenario), RH7204 (2) — the naming rules' fixtures are lower-case on purpose;
- CS8826 (partial method parts with different parameter names), 10 tests: RH4105 (8), RH4121 (2) — the fixtures' subject;
- CS8848, 5 tests: RH3004 `LeftBinaryOperandIsReportedWithoutACodeFix`, `LeftBinaryOperandWithBlockBodyIsReportedWithoutACodeFix`, `CastAndBinaryOperandsAreReportedWithoutACodeFix`, `FixAllLeavesABinaryOperandReported`; RH3106 `AnonymousMethodAsTheLeftOperandOfCompetingUserDefinedOperatorsIsReportedWithoutACodeFix` — the inputs for which RH3004-005 withholds the fix because of this warning.

None reveals a code-fix defect. Checking every compiler warning (`CompilerDiagnostics.Warnings`, level 9999) fails 3864 of 10404 tests.

Inputs the test implementer must cover for the guard, whatever its shape:

1. a fixed state that holds CS8848 where the source did not — must fail (the issue's proof);
2. a source state that holds a wave warning the fix leaves or removes (e.g. a lower-case type name renamed by RH4002) — must pass;
3. a fixed state that holds an ordinary level-1–4 warning the fix introduces (e.g. RH2005's CS0169) — must pass;
4. the iterative and the Fix All fixed state both reach the check.

No production code fix produces case 1, so its test needs a test-local analyzer and code fix that write such a result.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzer | not involved |
| Formatter phase | not involved |
| Code fix | not involved (no fix introduces a wave warning today) |
| Fix All | not involved as production code; its fixed state must be covered by the check |
| `Reihitsu.Core` | not involved |
| Rule docs | not involved |
| Tests: verifier classes / `AnalyzerTestsBase<,>` | must change — the only surface of the run |
| Existing test fixtures | must stay — under a fixed-state-introduced check, zero existing tests change outcome (measured) |
| Specs | not involved; no spec covers the harness, and SHARED-FIX-003 names compile errors only |

## Refuse option

Refusing is not applicable in the product sense (no fix and no formatter construct is involved); declaring a shape unsupported is not an option, because no formatter or code-fix shape is in question.

Options for the run, by cost:

- **No change** (`Refs #867`). Cost zero. The issue's prescribed change is a no-op, no fix introduces a wave warning, the one known instance is fixed and covered, and the specs refuse CS8848-producing rewrites case by case. Loses only the regression guard.
- **Narrow guard (recommended):** fail a code-fix test only when its fixed state holds more warning-wave diagnostics than its source state. Measured: zero existing tests change outcome. Cost: one check in the test harness plus its self-test; no fixture changes.
- **The issue's shape:** raise wave warnings in the source and the fixed state. Cost: 69 existing tests must declare compiler diagnostics on inputs that are lower-case or partial on purpose, and every future naming fixture must as well, for no fix defect found; the source-state half can never catch a fix defect, since the user wrote that input.
- **Check all warnings:** 3864 failures, and it contradicts SHARED-CASING-009 and RH2001/RH2005/RH3204. Rejected.

## Output questions

_None._ The harness change alters no formatter output, diagnostic, code-fix result, rule page, or CLI behavior; under the narrow guard no existing test's expected output changes.

## Internal choices

1. **Which states the check covers.** (a) fixed state only, counting what the fix introduces; (b) source and fixed state (the issue's goal). Recommendation: (a) — the issue's purpose is "a code fix that introduces a warning-wave diagnostic fails its own tests"; (b) adds 69 fixture edits and catches nothing a fix does.
2. **Which warnings count.** (a) warning-wave diagnostics only; (b) every warning. Recommendation: (a) — (b) contradicts specified behavior (SHARED-CASING-009) and the purpose of three rules. How the wave set is determined (an ID list that drifts with Roslyn upgrades, or the difference between a high level and level 4) is the test implementer's choice; the measurement above used a fixed list of the 22 IDs read from Roslyn 5.12.
3. **Which level.** The issue asks for the level of `net10.0`; that premise is stale: the test project targets `net11.0`, and the fixtures compile against `ReferenceAssemblies.Net.Net90`, whose SDK-derived level would be 9 (`Microsoft.NET.Sdk.Analyzers.targets`: a .NET project's `WarningLevel` is its TFM's major version; `AnalysisLevel=preview` gives 9999). Level 9 misses wave 10 (CS9265), which a user on `net10.0` or later sees. Recommendation: 9999 — a fix's output lands in projects of every TFM, and new waves of a later Roslyn are then covered without a change.
4. **Where.** Per-test-class opt-in defeats a guard; a global check is right. The base classes are not the single owner: `RH4102EventNameCasingAnalyzerTests` and `CodeFixInteractionTests` construct the verifiers directly. The code-fix verifier class owns every fixed-state compilation; the analyzer verifier needs no change under choice 1(a). Recommendation: the code-fix verifier path, so every code-fix test reaches the check.

## Issue goals

The issue is `agent-filed`; none of these changes visible output.

- "The verifier's compilation options use the chosen warning level for both the source and the fixed state" — change: a level change alone is a measured no-op; the goal that serves the purpose is a fixed-state check of introduced wave diagnostics. Dropping the source-state half avoids 69 fixture edits.
- "A test proves that a fixed state containing CS8848 fails verification" — keep; it needs a test-local fix, since no production fix writes CS8848.
- "Every existing test that starts failing is either fixed or its fixture is adjusted, with any real code-fix defect filed separately" — moot under the narrow guard (no test changes outcome); measured: no code-fix defect exists to file.
- Question "level for `net10.0`" — stale premise (see Internal choices 3).
- Question "globally or per test class" — globally (see Internal choices 4).

## Architecture findings

_None._ The harness owns compilation options in one place per verifier already; the direct verifier constructions in two test classes share it.

## Follow-up candidates

_None._ The ordinary warnings that fixes introduce (table under Premise) are intended or specified; RH5518's doubled CS0657 comes from an input that is already invalid.
