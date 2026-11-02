Role: skeptic
Commit: de0d4dd
Verdict: output question

The prompt carried no intended fix, no suspected cause, and no hoped-for verdict. Every probe below ran `scripts/format.sh` twice (the second pass formatted 0 files in every set, so all outputs quoted are fixed points) and `scripts/apply-fix.sh <ID>` over the formatted output. Fixtures lived in the scratchpad and are deleted.

## Premise

The issue was written against `735d0dba`. On `de0d4dd` (= `main` 99ae644 + run claim) its three parts stand as follows.

**1. The `while` defect (FMT-BLANK-008) does not reproduce; it was fixed by thoenissen/Reihitsu#1541.**

- `BraceBlankLinePolicy.RemovesBlankLinesDirectlyBefore` (`src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BraceBlankLinePolicy.cs`) now reads `SyntaxKind.WhileKeyword => token.Parent is DoStatementSyntax`, and `BlankLineTokenCleanupRewriter.VisitToken` calls `RemoveBlankLinesAfterTrailingDirective` only for those tokens. `git log -S"token.Parent is DoStatementSyntax"` names b27f329 (#1541).
- The issue's input, formatted: unchanged (only the final newline differs, by CleanupPhase).
- The regression test already exists with the issue's exact input: `BlankLineBehindDirectiveBeforeTokenTests.KeepsBlankLineBetweenDirectiveAndWhileStatement` (`src/Reihitsu.Formatter.Test/Regression/BlankLines/BlankLineBehindDirectiveBeforeTokenTests.cs`), tagged `FMT-BLANK-008` and `FMT-BLANK-009`; its counterpart `RemovesBlankLineBetweenDirectiveAndDoStatementWhile` covers the do-statement `while`. Change doc `sdd/changes/2026-11/blank-line-sole-owner.md`, visible output change 21, records it.
- `WhileKeyword` has two parents (`WhileStatementSyntax`, `DoStatementSyntax`); both are settled by the two tests. Nothing left to build.

**2. The region rule restatement is real, and RH5031 reports the formatter's output. The drift is wider than the issue says.**

- Statements checked: the phase restates SHARED-BLANK-005 in `BlankLineEditor.IsExemptFromPrecedingBlankLineBeforeRegionDirective` (both overloads) and `BlankLineEditor.EnsureBlankLineBeforeFirstDirective`, called from `BlankLineRegionDirectiveRewriter` (`#region`) and `BlankLineTriviaBoundaryRewriter.VisitToken` (`#endregion`, leading and trailing trivia). Core's `RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore` is named only in `<see cref>`s. True as the issue says.
- The conditional-directive exemption was added in de4c614 (#1455) without a spec sentence; before it the exemption covered every gap behind `{`. No spec states it: SHARED-BLANK-005 (`sdd/specs/shared/blank-line-spacing.md`) says "Analyzer and formatter MUST require the line directly above an active `#region` or `#endregion` directive to be blank", exempting only the first line and a line that ends with `{`; FMT-BLANK-013 says the formatter "MUST apply SHARED-BLANK-005"; RH5031-001 reports per SHARED-BLANK-005.
- One existing test asserts the formatter's current output: `DisabledRegionLayoutTests.RegionInsideActiveConditionalIsStillFormatted` (`src/Reihitsu.Formatter.Test/Regression/Regions/DisabledRegionLayoutTests.cs`, parse options with `DEBUG`, no requirement tag) expects `#if DEBUG` directly above `#region Test`.
- **The issue's literal characterization input shows nothing.** The CLI parses with `CSharpParseOptions.Default` (`src/Reihitsu.Cli/FormatCommandHandler.cs:125`) and the analyzer test verifiers define no `DEBUG`, so `#if DEBUG` is not taken, the region is disabled text, and both sides are silent by SHARED-BLANK-007 (fixture f2: RH5031 `no-diagnostic`). A characterization with that input would wrongly conclude "no conflict". The conflict appears only for an active branch (`#if !DEBUG`, a `#define DEBUG`, or `DEBUG` in the parse options).
- Reachability: the CLI reaches every failing member below (each was formatted by `scripts/format.sh`, which runs the CLI); a code fix that formats a type or member whose leading trivia holds such a branch reaches the same phase. Region directives at the start of a file behind a header comment are a common shape.

**3. The restructuring (one ordered rule list) is a prescribed solution whose quantitative premise is unverified on current `main` and whose method contradicts an accepted decision.**

- `BlankLinePhase.CreateRewriters` still runs six rewriters in order — true.
- "Reversing the list changes 175 of 794 outputs" and "hundreds of blank lines removed and re-added" were measured at `735d0dba`, before #1541 rewrote the cleanup rewriter (133 lines) and made the phase the sole owner of brace-adjacent blank lines. No existing entry point measures sub-phase churn, so I did not re-measure; the figure is unverified.
- One live remove/re-add on formatted input exists (fixture d1, below): `RemoveBlankLinesAfterLeadingDocumentationComments` (FMT-BLANK-010) removes the blank line between a documentation comment and a `#region` directly below it, and `BlankLineRegionDirectiveRewriter` (FMT-BLANK-013) re-inserts it. The output is decided by the order of the two rewriters.
- The accepted change doc of thoenissen/Reihitsu#1541, `sdd/changes/2026-11/blank-line-sole-owner.md`, *Precedence* and *Why*, rejected exactly the issue's method: "**One precedence ranking for all insertion and removal rules** — rejected: after narrowing FMT-BLANK-008 and FMT-BLANK-011, no blank line is decided by two rules, so a ranking would decide nothing and invite overlaps." It resolves overlaps by narrowing scopes instead. The issue's "the precedence it encodes is the one C3 (#1496) states" is therefore stale: C3 states disjoint scopes, not a ranking. Its list of removal rules omits FMT-BLANK-010, which is how d1 slipped through.
- "The blank-line phase run on its own output changes nothing" is already enforced: `FormatterTestsBase` runs `PhaseStabilityCheck.Verify` on every formatter fixture (every production phase alone on the final output), and `PhaseReversalExceptionList` holds no blank-line entry.

## Mechanism and candidates

Mechanism (region drift): the formatter decides whether the line above an active region directive must be blank from the trivia between the token in front of the directive's token and the *first* directive of that kind in the token's leading trivia, while SHARED-BLANK-005 (Core, RH5031 and its fix) decides it for *every* active region directive from the one physical line directly above it; a directive fails whenever the two views disagree.

Dispatch: `BlankLineRegionDirectiveRewriter.VisitToken` → `BlankLineEditor.EnsureBlankLineBeforeFirstDirective(token, RegionDirectiveTrivia, …)`; `BlankLineTriviaBoundaryRewriter.VisitToken` → `EnsureBlankLineBeforeFirstDirective(token, EndRegionDirectiveTrivia, …)` for leading trivia and `EnsureBlankLineBeforeFirstEndRegionInTrailingTrivia` behind the one-argument exemption for trailing trivia. The exemption is `IsExemptFromPrecedingBlankLineBeforeRegionDirective(PrecedingTokenFacts, SyntaxTriviaList, int)`: exempt when there is no preceding token (any trivia), or the preceding token is `{` without a trailing comment and only whitespace, line breaks, `IfDirectiveTrivia`, `ElifDirectiveTrivia`, `ElseDirectiveTrivia`, `EndIfDirectiveTrivia`, `DisabledTextTrivia` precede the directive. `EnsureBlankLineBeforeFirstDirective` breaks at the first active directive of the kind, so later ones are never examined.

Candidates (defining type of the directive's holder: the next token's `SyntaxTriviaList`): `RegionDirectiveTriviaSyntax`, `EndRegionDirectiveTriviaSyntax`; the line above is one of blank, `{`-only line, `IfDirectiveTriviaSyntax`/`ElifDirectiveTriviaSyntax`/`ElseDirectiveTriviaSyntax`/`EndIfDirectiveTriviaSyntax`, another directive (`PragmaWarningDirectiveTriviaSyntax`, `NullableDirectiveTriviaSyntax`, region directive), comment, code.

## Breaking inputs

Failure classes walked from `squad/failure-classes.md` that reach the changed code: FC-015 (a private copy of a Core policy drifts — the cause here), FC-016 (sibling shapes keep the hazard — the matrix below), FC-013 (RH5031 reports the formatter's output), FC-020 (`#if`/`#elif`/`#else`/`#endif`, `#pragma`, `#region`, inactive blocks in the gap — axis 1), FC-019 (a gap tested without comment or directive — the matrix carries both), FC-010 (a phase on its own output — every set formatted 0 files on the second pass), FC-012 inside the phase (d1, follow-up 1). FC-004/FC-005 do not apply: the rule only inserts a line break between existing lines, it moves no trivia. FC-028 is not reached (see *Not probed*).

Matrix (axes: line directly above · preceding token · first or later directive of its kind in the trivia · `#region`/`#endregion`). FAIL = formatter keeps the shape and RH5031 reports it (`apply-fix.sh RH5031`: `fixed`).

| Fixture | Line above | Preceding token | Position | Kind | Result |
|---|---|---|---|---|---|
| f3 | `#if !DEBUG` | `{` (type) | first | `#region` | FAIL |
| f4 | `#if DEBUG` + `#define DEBUG` | `{` (type) | first | `#region` | FAIL |
| f5 | `#else` (if-branch disabled) | `{` | first | `#region` | FAIL |
| g5 | `#elif !DEBUG` | `{` | first | `#region` | FAIL |
| g4 | `#endif` (branch disabled) | `{` | first | `#region` | FAIL |
| f6 | `#if !DEBUG` | `{` (method body) | first | `#region` | FAIL |
| g6 | `#if !DEBUG` | `{` (namespace) | first | `#region` | FAIL |
| g7 | `#if !DEBUG` | `{` (switch block) | first | `#region` | FAIL |
| g8 | `#if !DEBUG` | `{` (accessor list) | first | `#region` | FAIL |
| g1 | `// header` | none (file start) | first | `#region` | FAIL |
| g2 | `#if !DEBUG` | none (file start) | first | `#region` | FAIL |
| k4 | `#nullable enable` | none (file start) | first | `#region` | FAIL |
| h1 | `// c` | `;` | later (behind `#region A`) | `#region` | FAIL |
| h2 | `#pragma warning disable` | `;` | later | `#region` | FAIL |
| h3 | `#if !DEBUG` | `;` | later | `#region` | FAIL |
| k1 | `// c` | `;` | later (behind `#endregion // B`) | `#endregion` | FAIL |
| k2 | `#pragma warning disable` | `;` | later | `#endregion` | FAIL |
| h4 | `#if !DEBUG` | `;` | first | `#region` | holds (blank inserted) |
| g9 | `#pragma warning disable` | `{` | first | `#region` | holds |
| k5 | `// c` | `;` | first | `#region` | holds |
| f7 | blank line written below `#if !DEBUG` | `{` | first | `#region` | holds (kept, RH5031 silent) |
| f2 | `#if DEBUG`, DEBUG undefined | `{` | — inactive | `#region` | holds (both silent, SHARED-BLANK-007) |
| g3 | `#endregion` behind `{` + `#if` | — | — | `#endregion` | parse error; no caller reaches it |

Not probed: a later `#region` directly below another region directive (SHARED-BLANK-006 already inserts that line); an `#endregion` in trailing trivia (the parser never puts a directive there; only an earlier rewrite could); the start-of-file exemption for a detached node-level root with no preceding token — I found no code fix that formats a detached node whose first token's leading trivia holds a region directive behind other trivia, so I treat it as unreached.

Example of a failing output (f3, today's output on `main`; RH5031 reports `#region A`):

```csharp
class C
{
#if !DEBUG
    #region A

    void M()
    {
    }

    #endregion // A

#endif
}
```

Example (h1, today's output; RH5031 reports `#region B`):

```csharp
class C
{
    int _a;

    #region A

    // c
    #region B

    int _b;

    #endregion // B

    #endregion // A
}
```

Inputs the test implementer must cover, should the region rule change: every FAIL row (at least one per value of each axis: the four conditional directives, the four brace contexts, the three file-start shapes, the five "later" rows), every "holds" row as a guard, and the RH5031 characterization on an *active* branch. The natural home for the characterization is the existing analyzer-side `FormatterTestsBase<RH5031…Analyzer>` (`src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5031RegionDirectivesShouldBePrecededByABlankLineFormatterTests.cs`), which formats an input and verifies the analyzer on the output. RH5032 reports nothing on any of these outputs (`apply-fix.sh RH5032`: 8 `no-diagnostic` on h-set).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter blank-line phase, region rule (`BlankLineEditor`, `BlankLineRegionDirectiveRewriter`, `BlankLineTriviaBoundaryRewriter`) | Must change under output question 1 A / 2 A / 3 A; must stay under B |
| `BlankLineTokenCleanupRewriter` / `BraceBlankLinePolicy` (`while`) | Must stay — already correct and tested |
| Other blank-line rewriters, `BlankLineCollapser` | Not involved unless the lead keeps the restructuring goal |
| `Reihitsu.Core` `RegionDirectiveBlankLineUtilities` | Must stay in behavior. Its API takes a `SourceText` and a set of `{`-ending line indices; the formatter works on trivia mid-rewrite, so "the phase calls the Core rule" needs either a text view of the tree in the phase or a new Core entry point shared with RH5031's analyzer and code fix — a shared helper with other consumers (scope growth to report) |
| RH5031 analyzer and code fix | Must stay under A; must change (new exemption) under B |
| RH5032 | Not involved (no diagnostic on any probe output) |
| RH8302 | Not involved in this run (see follow-ups) |
| Specs | SHARED-BLANK-005, FMT-BLANK-013, RH5031-001 unchanged under A; all three change under B. `blank-lines.md` code-owner list changes only if the rewriter set changes |
| Rule pages | Not involved under A; RH5031's page changes under B |
| Tests | `DisabledRegionLayoutTests.RegionInsideActiveConditionalIsStillFormatted` expected value changes under 1 A |
| CLI, Fix All | Not involved |

## Simpler option

Fix the defect that exists and drop the rest: make the formatter's region rule match SHARED-BLANK-005 for every active region directive (both drift classes), with the RH5031 characterization on an active branch; do not rebuild the phase into one ordered rule list. The `while` goal needs no work. This removes the second statement of the rule only if the phase decides it through one shared predicate; a narrowed exemption inside `BlankLineEditor` would keep the restatement (see *Architecture findings*).

## Output questions

**1 · Formatter — region directive below a conditional directive, directly after `{`**

Input:
```csharp
class C
{
#if !DEBUG
    #region A
    void M() { }
    #endregion
#endif
}
```

Before (`main`), which RH5031 reports at `#region A`:
```csharp
class C
{
#if !DEBUG
    #region A

    void M()
    {
    }

    #endregion // A

#endif
}
```

After, A (recommended):
```csharp
class C
{
#if !DEBUG

    #region A

    void M()
    {
    }

    #endregion // A

#endif
}
```

After, B: the formatter's output stays as Before, and RH5031 no longer reports a region directive whose line above is `#if`, `#elif`, `#else`, or `#endif` with only conditional directives and disabled text between it and `{`.

Why A: SHARED-BLANK-005 and FMT-BLANK-013 already demand the blank line, and RH5031 and its fix already produce it; B adds an exemption no spec has.

**2 · Formatter — region directive at the start of a file behind a comment or directive**

Input:
```csharp
// header
#region A
class C
{
}
#endregion
```

Before (`main`), which RH5031 reports at `#region A`:
```csharp
// header
#region A

class C;

#endregion // A
```

After, A (recommended):
```csharp
// header

#region A

class C;

#endregion // A
```

After, B: Before stays, and RH5031 no longer reports a region directive above the file's first token, whatever stands above it.

Why A: SHARED-BLANK-005 exempts only a directive on the first line; a header comment is content like any other.

**3 · Formatter — a second region directive in one gap, behind a comment**

In my reading this is not in question: SHARED-BLANK-005 and FMT-BLANK-013 demand A, and no test asserts today's output. It is listed because the issue routes every formatter/RH5031 disagreement to the maintainer; the lead decides whether to ask it.

Input (already formatted except for this line):
```csharp
class C
{
    int _a;

    #region A

    // c
    #region B

    int _b;

    #endregion // B

    #endregion // A
}
```

Before (`main`): unchanged; RH5031 reports `#region B`.

After, A (recommended):
```csharp
class C
{
    int _a;

    #region A

    // c

    #region B

    int _b;

    #endregion // B

    #endregion // A
}
```

After, B: Before stays, and RH5031 reports only the first region directive of each kind in a gap.

Why A: the first region directive behind the same comment already gets the blank line (fixture k5); position in the gap is not a reason to differ.

## Internal choices

- **Where the region rule lives.** (a) The phase decides each directive by the one-line-above rule through a predicate shared with Core (one statement of SHARED-BLANK-005); (b) the formatter's `BlankLineEditor` exemption is narrowed and its loop extended to every directive (two statements remain, drift can recur). Same visible output under 1 A–3 A. Recommend (a); it is the issue's stated aim and removes the architecture finding's cause. Consequence of (a): a Core API change consumed by RH5031's analyzer and code fix (scope growth to record).
- **Characterization input.** Use `#if !DEBUG` (active under the CLI's and the verifiers' default options) rather than the issue's `#if DEBUG`, or set `DEBUG` in the parse options as `DisabledRegionLayoutTests` does. Recommend `#if !DEBUG`, plus the issue's input as the inactive guard.

## Issue goals

- **"Replace the six rewriters with one ordered rule list" / Done when 1** — drop from this run. It is a prescribed solution; its measured premise predates #1541 and is unverified now; its method (a precedence ranking) is the alternative the accepted change doc of #1541 rejected in favor of disjoint scopes; the one live overlap found (d1) is a spec conflict that needs an output decision, not a ranking (follow-up 1). No visible output changes by dropping it. If the lead keeps it, it should be stated as disjoint rules decided once per gap, not as a ranking, so it does not contradict the accepted change doc.
- **The `while` spec bug / Done when 2** — drop as already met on `main` (#1541); the regression test tagged `FMT-BLANK-008` exists with the issue's input. No visible change.
- **RH5031 characterization / Done when 3** — keep, adjusted: the literal input is inactive and shows nothing; use an active branch. Its result: RH5031 reports the output, so the conflict branch applies — output questions 1–3.
- **Done when 4** — "the phase run on its own output changes nothing" is already enforced for every formatter fixture by the phase-stability check; "the suite keeps its outputs" cannot hold together with 1 A, which changes one expected value.

## Architecture findings

- **One rule stated in more than one place.** SHARED-BLANK-005 is stated in Core `RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore` (read by `RegionDirectiveBlankLineAnalyzerBase` and `RegionDirectiveBlankLineCodeFixProviderBase`) and again in the formatter: `BlankLineEditor.IsExemptFromPrecedingBlankLineBeforeRegionDirective` (two overloads), `BlankLineEditor.EnsureBlankLineBeforeFirstDirective` and `BlankLineEditor.HasBlankLineImmediatelyBeforeIndex`, used from `BlankLineRegionDirectiveRewriter` and `BlankLineTriviaBoundaryRewriter`. The copies have drifted twice: the exemption scope (f3–f6, g1, g2, g4–g8, k4) and first-of-kind only (h1–h3, k1, k2). This defect is the run's issue's own, so it reaches the run.
- **Two rewriters decide one gap, by order.** `BlankLineTokenCleanupRewriter.RemoveBlankLinesAfterLeadingDocumentationComments` (FMT-BLANK-010) and `BlankLineRegionDirectiveRewriter` (FMT-BLANK-013) both decide the gap between a documentation comment and a region directive directly below it; the later one wins (d1). The accepted change doc's *Precedence* omits FMT-BLANK-010 from its removal rules. Not a defect of the run's issues as I scope them (see follow-up 1).

## Follow-up candidates

**1 · Formatter, RH8302 · Element documentation headers must not be followed by blank line, RH5031 · Region directives should be preceded by a blank line — documentation comment directly above a region directive**

No output satisfies both rules: RH8302 reports the blank line, RH5031 reports its absence, and each fix undoes the other. The formatter keeps or inserts the blank line only because its region rewriter runs after its cleanup rewriter.

Input:
```csharp
class C
{
    int _a;

    /// <summary>
    /// Doc
    /// </summary>
    #region A

    void M()
    {
    }

    #endregion // A
}
```

Before (`main`): a blank line is inserted between `/// </summary>` and `#region A`; RH8302 reports the output.

After, A (recommended): as Before, and RH8302 does not report a blank line that SHARED-BLANK-005 requires above a region directive.

After, B: no blank line between the documentation comment and `#region A`, and RH5031 does not report a region directive directly below a documentation comment.

Why A: the formatter's output stays, and a documentation comment that a region directive separates from its member is better fixed by hand than glued to the directive.

**2** — none else. The `#endregion // A` written into disabled text (fixture f2) is intended by FMT-REGION-006 (RH7301/RH7302 report those directives too); not a find.
