Role: spec author
Commit: 5a58686
Verdict: #948: spec written; #949: spec written; #954: spec written; #1390: spec written; #888: spec written

Clarifications only, no change doc (bug-fix lane). New IDs: FMT-SWITCH-014, FMT-STRUCT-025, both inserted into `sdd/specs/requirement-ids.txt` at their sorted positions. The lead prompt carried no intended fix or verdict; the issue texts' own cause hypotheses (#949, #954) were not taken over — see the facts below.

How the statements were established: `scripts/trace.sh` over each issue input and its variants at `5a58686`, `scripts/format.sh` for converged outputs, `scripts/apply-fix.sh` for RH5031 and RH3202, and a scratch `net10.0` library compiled with `dotnet build` for the C# scope claims of FMT-SWITCH-014. `SpecStructureTests` and `SpecConsistencyTests` pass on the spec diff; `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails only for FMT-SWITCH-014 and FMT-STRUCT-025 (no covering test yet — the test implementer's part).

## thoenissen/Reihitsu#948

### Changed requirements

- FMT-SWITCH-014 — A local used by another section leaves the switch alone — added (`sdd/specs/formatter/switch-case-braces.md`). Clarification of FMT-PIPE-003 as decided in the maintainer's #919 Q1 answer (whole switch unchanged, like FMT-SWITCH-011), extended by the issue comment to local functions.

### Facts

- Reproduced: the issue input is braced and section 2 no longer compiles.
- The issue's "expected output = the input unchanged" is not reachable: FMT-BLANK-002 inserts a blank line after `int y = 1;` (trace: `BlankLinePhase`). The spec example therefore carries that blank line in its input, so "is left unchanged" holds for the whole pipeline.
- Compiled (scratch project): `F(out var o);` (expression statement), `var b = F(out var o);` (local declaration) and `if (p is int n)` directly in a section are visible in another section; a local function declared in section 2 can be called from section 1; once braced, the use fails with CS0103. A pattern variable in a `while` condition is not visible in another section (CS0103 unbraced) — hence the requirement names only local declaration, expression statement and `if` condition, with a coverage sentence for other statement kinds.
- The nested-block counter-example (field `y` used in section 2, local `y` inside an `if` block in section 1) compiles both unbraced and braced.

### Examples for the tests

FMT-SWITCH-014 (three unchanged inputs: local, pattern variable, local function used above its declaration; two braced: local used only in its own section, local in a nested block). Reproduction tags FMT-SWITCH-014 and FMT-PIPE-003.

### Non-goals

- Bracing only the sections that are not involved (the decision is the whole switch).
- Uses by a name that is not a simple name (`o.y`), and expression variables of other statement kinds — left unspecified by the coverage sentence.

### Open questions for the maintainer

_None._

## thoenissen/Reihitsu#949

### Changed requirements

- FMT-SWITCH-007 — Comments survive brace insertion — clarified: a comment leading the terminal `break` stays in front of it, behind the new block's closing brace, with the FMT-BLANK-011 blank line in the output of the pass that inserts the braces; new example (the issue input and expected output). `Uses` gains FMT-BLANK-011 and FMT-PIPE-004.

### Facts

- Reproduced (trace, issue input): pass 1 output has no blank line between `}` and `// before break`; pass 2 `BlankLinePhase` inserts it (FMT-PIPE-004 violated).
- The issue states the blank-line phase does not insert the line. The trace shows otherwise: pass 1 `BlankLinePhase` does insert the blank line before `// before break`, and the `SwitchCaseBracePhase` diff of the same pass shows it gone.
- Unbraced shape (`case 1: A(); // before break break;`, section not multi-line): pass 1 inserts the blank line, pass 2 stable — correct today.
- Already-braced input (`{ A(); B(); }` then the comment): pass 1 inserts the blank line, stable — correct today.
- RH5020 agrees with the expected output: RH5020-001/-002 report a line-leading comment whose preceding token is `}` (not `{` or a label colon), and do not report the comment when its line above is blank.

### Examples for the tests

FMT-SWITCH-007 (new example). Reproduction tags FMT-BLANK-011, FMT-PIPE-004, FMT-SWITCH-007.

### Open questions for the maintainer

_None._

## thoenissen/Reihitsu#954

### Changed requirements

- FMT-BLANK-007 — No blank lines inside braces or at the start of a file — clarified with an example: an empty region directly after `{` (the issue input and expected output).

### Facts

- Reproduced (trace): pass 1 `LineBreakPhase` adds a blank line after `{`; pass 2 `BlankLinePhase` removes it and `LineBreakPhase` adds it again. The final text is the same in every pass, so `trace.sh` reports "Stable after pass 2" — the text-level FMT-PIPE-004 check does not catch this; FMT-BLANK-007 on the output does.
- Same in a method body with a blank line already between the directives, and with `// c` above the region (all three verified). `#if X` / `#endif` directly inside braces: no change (verified).
- `line-breaks.md` already states in its non-goals: "Where this phase moves a token onto a new line it inserts no blank line."
- The issue's sentence "No phase undoes another phase's edit in any pass" is not a requirement of any spec and was not added; the observable requirement is FMT-BLANK-007.

### Examples for the tests

FMT-BLANK-007 (new region example). Reproduction tags FMT-BLANK-007 (SHARED-BLANK-005 for the brace exemption, SHARED-BLANK-006 for the kept line between the directives).

### Open questions for the maintainer

Q2 below concerns the issue's own `// c` variant.

## thoenissen/Reihitsu#1390

### Changed requirements

- FMT-STRUCT-025 — An `async` function of an unknown task type keeps its expression body — added (`structural-transforms.md`). Instance of FMT-STRUCT-024 / FMT-PIPE-003 as decided in #1371 Q-S1 (P36).
- FMT-STRUCT-004 — Expression-bodied methods and local functions get a block body — clarified: the `return … otherwise` bullet no longer covers an `async` function of an unrecognized return type; one sentence points to FMT-STRUCT-025.
- FMT-STRUCT-006 — The converted body keeps the expression's trivia — clarified: a comment on a line of its own between the token before the arrow and the arrow, or between the arrow and the expression, leads the created statement on a line of its own, in source order; the expression still joins `return`. Coverage sentence leaves comments sharing a line with the arrow or the expression unspecified. New example (before-arrow, after-arrow with `return`, after-arrow with an expression statement).
- FMT-BREAK-021 — Object initializers are always laid out over several lines — clarified: names the `with` initializer; new example (the issue's expression-bodied property and a local). The `Why` notes no rule reports a `with` initializer (RH5301 lists it as a non-goal).

### Facts

- Async alias (`using T = System.Threading.Tasks.Task;`, `async T RunAsync() => await T.Delay(1);`): today `return await T.Delay(1);` (CS1997). The current check recognizes `Task`/`ValueTask` as a simple name or the right part of a (alias-)qualified name; every other `async` return type gets `return`.
- FMT-STRUCT-025 follows the issue's decision literally: an `async` function returning a generic alias (`using V = ValueTask<int>`) or a custom task-like type, which today converts to compiling code, keeps its expression body. The example shows this on purpose.
- Qualified `System.Threading.Tasks.Task` / `Task<int>` under `async`: converted today as the FMT-STRUCT-025 example shows (verified with `format.sh`).
- Comment before the arrow: today `int Other()`, blank line, `// before arrow`, `{` — the comment stands between the signature and the brace. Comment after the arrow: today `return`, blank line, `// c`, `42;`. Same shape with an expression statement (`void Run() =>` / `// run` / `Foo();`): today `{        // run` on the brace line. All three are in the FMT-STRUCT-006 example; the third is not named in the issue but is the same position with the other statement form, and FMT-PIPE-013 already requires a comment on a line of its own to stay on one.
- `with` initializer: a one-line initializer with two or more elements keeps `{` on the `with` line in pass 1 and gets its own line in pass 2 (FMT-PIPE-004 violated); a one-element initializer and an already wrapped one reach the layout in pass 1. Verified the same for a converted expression-bodied method returning a `with` expression. The FMT-BREAK-021 example output is the converged output of `format.sh` run twice.

### Examples for the tests

FMT-STRUCT-025, FMT-STRUCT-006 (new example), FMT-BREAK-021 (new `with` example). Reproductions tag FMT-STRUCT-025 + FMT-PIPE-003 (alias), FMT-STRUCT-006 (comments), FMT-BREAK-021 + FMT-PIPE-004 (`with`).

### Non-goals

- A comment behind `=>` on the arrow's line (`int Get() => // why`), which is thoenissen/Reihitsu#963 (milestone 1.2.1), and field splitting from the same issue.
- Analyzer side of FMT-STRUCT-025 — see Q1.
- Other FMT-BREAK-021 text changes asked by thoenissen/Reihitsu#1291 (another issue edits the same requirement; expect a textual merge there).

### Open questions for the maintainer

Q1 and Q3 below.

## thoenissen/Reihitsu#888

### Changed requirements

- FMT-BREAK-019 — Chains follow the shared chain layout — clarified: a later call behind the closing `*/` of a multi-line comment does not start its line (SHARED-COMMENT-012), so the formatter breaks in front of its operator; new example. `Uses` of `line-breaks.md` gains SHARED-COMMENT-012, as the maintainer asked.

### Facts

- Reproduced (trace, issue input): the only change is the FMT-BLANK-011 blank line above `/* x`; `.D()` stays behind `*/`. The example input already carries that blank line so the example shows only the line break.
- The expected `.D()` column (17, the anchor `.B`) follows SHARED-CHAIN-007; the comment's last line `*/` keeps its column (comment content).
- RH5201 analyzer unchanged per the maintainer. `sdd/specs/analyzer/RH5201.md` does not list SHARED-COMMENT-012 under `Uses`; not changed here (follow-up candidate).
- Related open issue outside the run: thoenissen/Reihitsu#1382 (P33, token behind a multi-line comment tail: formatter breaks before it, RH5203/RH5204).

### Examples for the tests

FMT-BREAK-019 (new example). Reproduction tags SHARED-COMMENT-012 and FMT-BREAK-019; the RH5201 code-fix test tags SHARED-CHAIN-008 / SHARED-FIX-001 (one-pass convergence), under LF and CRLF as the issue asks.

### Open questions for the maintainer

_None._

## Whole run

### Open questions for the maintainer

**Q1 — #1390: RH3202 / RH3208 on an `async` member the formatter now keeps.** Found in passing; it does not change any formatter output of the run, but answer A adds production paths outside the formatter (escalation 5), so it is worth asking before the implementer finishes.

Input: `using T = System.Threading.Tasks.Task;` and `async T RunAsync() => await T.Delay(1);`. Real output today (`scripts/apply-fix.sh RH3202`): reported, "fixed" in one iteration into `{ return await T.Delay(1); }`, which does not compile. `ExpressionBodyToBlockCodeFixProviderBase` delegates to `ReihitsuFormatter.FormatNodeInDocumentAsync`, and `ExpressionBodyAnalyzerBase` skips only what `ExpressionBodyRewriteUtilities.BlocksRewrite` (FMT-STRUCT-007) refuses. Once FMT-STRUCT-025 holds, the diagnostic stays and its fix changes nothing. Touches SHARED-BODY-001, SHARED-BODY-003, SHARED-FIX-001, FMT-STRUCT-025. RH3xxx analyzers and fixes are owned by concurrent run R7.

- **A** — The rules do not report such a member: SHARED-BODY-001 also names FMT-STRUCT-025 (as its `Why` already argues for FMT-STRUCT-007). RH3202 is silent on the input above. Changes the shared analyzer predicate and `shared/expression-body-conversion.md`; must be coordinated with R7, or done as a follow-up issue.
- **B** — The rules keep reporting, and the code fix is withheld for that input (no fix offered, a supported end state). Changes the code-fix base; same R7 overlap.
- **C** — Leave both as they are in this run: the fix makes no progress (violates SHARED-FIX-001) until a follow-up issue settles A or B.

Recommendation: **A** — a diagnostic the formatter will never satisfy is the case SHARED-BODY-001 exists for; do it in this run if R7 does not touch `ExpressionBodyAnalyzerBase` / `ExpressionBodyRewriteUtilities`, otherwise as an approved follow-up issue with A.

**Q2 — #954 variant: no blank line between a comment and `#region` directly after `{`.** Found in passing; the spec already decides the intended output, so this is a scope question.

```csharp
class C
{
    // c
    #region A
    int _b;
    #endregion // A
}
```

Real output: blank lines are added inside the region, but none between `// c` and `#region A`. `scripts/apply-fix.sh RH5031` reports `#region A` on that output and inserts the line. SHARED-BLANK-005 ("a comment there does not satisfy the requirement") and FMT-BLANK-013 require it. With a member above the comment (`int _a;`, `// c`, `#region A`) the formatter inserts it. It occurs without the #954 defect (input above has no empty region and gets no blank line after `{`), but the issue's own `// c` variant of #954 shows both.

- **A** — Fix it in this run; the #954 regression test of the `// c` variant asserts the full spec output (`{`, `// c`, blank, `#region A`, blank, `#endregion // A`, `}`).
- **B** — Follow-up issue; this run's #954 tests use the inputs without a comment above the region.

Recommendation: **B** — it is a separate defect that shows without #954's, and the run must not change behavior beyond its issues.

**Q3 — #1390: a delimited comment that starts its line followed by the expression on the same line.** Found in passing; left out of FMT-STRUCT-006 by its coverage sentence.

```csharp
class C
{
    int B() =>
        /* c */ 42;
}
```

Real output: `return`, a blank line, then `/* c */ 42;` inside the block — the statement is split by a blank line, like the issue's `//` instance. Touches FMT-STRUCT-006, FMT-PIPE-013, FMT-STRUCT-024. Readings:

- **A** — `return /* c */ 42;` — the expression joins `return` (FMT-STRUCT-006) and the comment stays directly in front of the code it was written next to.
- **B** — `/* c */ return 42;` — the comment keeps its place at the start of the statement's line (FMT-PIPE-013 "in front of the statement").
- **C** — `/* c */` on a line of its own, then `return 42;` — treated like the `//` instance.

Recommendation: **A** — it keeps the comment next to `42` and the statement on one line, and needs no new layout rule; C would move a comment off its line.

### Follow-up candidates

- `sdd/specs/analyzer/RH5201.md` could list SHARED-COMMENT-012 under `Uses` (RH5201 checks line starts); not requested in this run.
