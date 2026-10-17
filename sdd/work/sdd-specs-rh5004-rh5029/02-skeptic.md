Role: skeptic
Commit: c4361d3 (production code unchanged since 33a29f7; specs from cf18737)
Verdict: proceed narrower

The narrowing applies to RH5005-002 and RH5006-002 only, and only to the `await` form (see Premise, finding 2). It reduces to plain "proceed" if thoenissen/Reihitsu#946 already keeps the diagnostic on the `using` / `foreach` keyword for that form. I could not check that because this run must not touch GitHub, so the lead has to look at #946. Every other requirement states what the code does today.

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

Lane: specs and tests for existing behavior. The question is whether each new requirement states current behavior, and which inputs the code decides that the specs leave unmentioned.

**What I read.** The base analyzer `StatementShouldBePrecededByABlankLineAnalyzerBase<TStatement>` (`IsRelevant`, `IsWithinStatementList`, previous token not `{`/`:`/None, `ContainsBlankLine`, `IsPrecededByDirective`), the 16 analyzers (registered kinds, `GetPreviousToken`, `GetLocation`, `IsRelevant`), `StatementShouldBePrecededByABlankLineCodeFixProviderBase`, and the formatter's `BlankLineStatementSpacingRewriter.NeedsBlankLineForStatementKind`.

**How the code compares with the specs:**

- Registered kinds match each spec's statement kind:
  - RH5006: `ForEachStatement` and `ForEachVariableStatement`.
  - RH5009: `GotoStatement`, `GotoCaseStatement`, and `GotoDefaultStatement`.
  - RH5015: `CheckedStatement` only.
  - RH5016: `UncheckedStatement` only.
  - RH5019: `YieldReturnStatement` and `YieldBreakStatement`.
- Locations:
  - The keyword for RH5004–RH5019.
  - `GetFirstToken()` for RH5021 and RH5029.
- `IsRelevant`:
  - RH5019: the previous statement in the list is not a `YieldStatementSyntax`.
  - RH5021: the previous statement is a `LocalDeclarationStatementSyntax`, and the expression is not an `AssignmentExpressionSyntax`. That covers every assignment kind, compound and `??=` included.
  - RH5029: the previous statement is not null and is not a `LocalDeclarationStatementSyntax`.
- All of this matches the requirement text.
- The formatter uses the same predicates, so FC-013 parity holds for every case below.

**Examples, re-verified independently.** I extracted every `csharp` block of the 16 specs. There are 18, not the 19 that 01-spec-author.md states: one block each, plus a second for RH5019 and RH5021. I stripped the markup, ran `scripts/apply-fix.sh --no-install <ID> <dir>` under LF and CRLF, applied the LF diff with `patch`, and compared the result with "the input plus one blank line above each marked line".

- 18 of 18 match.
- Every run reports `fixed`, with 0 `not-converged` and 0 `line-ending-drift`.
- The fixtures were in the scratchpad, outside the repository.

**Existing tests.** Their markup pins the keyword in every class: `{|#0:do|}`, `{|#0:using|}`, `{|#0:var|}`, `{|#0:Consume|}` for RH5021, and so on. No `const` or `using` span exists in the RH5029 class, which matches the spec author's gap list.

**Finding 1: deconstruction declarations are decided by the code but not mentioned in the specs.** `var (b, c) = t;` and `(int e, int f) = t;` are `ExpressionStatementSyntax` nodes holding an `AssignmentExpressionSyntax`, not a `LocalDeclarationStatementSyntax`. Observed with apply-fix and the formatter, which give identical results and are stable on a second pass:

- RH5029:
  - `a++; var (b, c) = t;` is not reported.
  - `var d = b + c;` directly after `var (b, c) = t;` is reported.
  - The fix puts the blank line between the two declarations.
- RH5021:
  - `var b = a; var (c, d) = t;` is exempt as an assignment.
  - `M(c + d, t);` after the deconstruction is not reported.

Neither spec says whether a deconstruction counts as a "local declaration". The case is left out, but the output looks wrong. See *Questions for the maintainer*.

**Finding 2: the `await` form of RH5005 and RH5006 (thoenissen/Reihitsu#946).** RH5005-002 says "MUST report the diagnostic on the `using` keyword", and RH5006-002 says the same for `foreach`. Neither is limited, so both also cover `await using (…)` and `await foreach (…)`.

What happens with that form:

- `GetLocation` returns `UsingKeyword` / `ForEachKeyword`.
- The code fix acts on the token at the diagnostic location. Its previous token is `await`, so it inserts the blank line between `await` and `using`.
- Observed: `scripts/apply-fix.sh RH5005` and `RH5006` on `a++; await using (resource) { … }` and `a++; await foreach (…) { … }` end `not-converged (10 iteration(s))` under LF and CRLF, with ten blank lines between `await` and the keyword.
- The formatter puts one blank line before `await`, and that output is not flagged.
- RH5029 on `a++; await using var other = resource;` reports on `await` (the first token) and is `fixed` in one iteration, so RH5029-002 and RH5021-001 are not affected.

Whether the reported token for the `await` form is part of the #946 defect is #946's decision. As worded, RH5005-002 and RH5006-002 decide it in favour of today's token. If #946's fix moves the diagnostic to `await`, that fix would contradict an active requirement. The maintainer's instruction "Must not: write current-but-defective behavior into a spec" applies.

The lead should check #946:

- If #946 keeps the location on the keyword, the requirements stand.
- If it does not, or is undecided, the spec author words RH5005-002 and RH5006-002 so they do not decide the `await` form, within the rulebook's "no requirement, no non-goal, no note" constraint.

RH5005-001 and RH5006-001 (the `await` form is reported) are not in question: the formatter separates it too.

**Q1 of 01-spec-author.md.** The over-indent of the multi-line `case … when` continuation line is real, and the test `VerifyDiagnosticOverIndentsWhenLabelSpansMultipleLinesAndPrecedingStatementSharesItsContinuationLine` pins it. That test's own XML summary calls it "a known, documented limitation (see RH5103.md) … today's accepted behavior". The question should quote that sentence: it may be answered as a clarification of SHARED-STMT-004 rather than as a bug. I agree it belongs to `shared/statement-blank-lines.md` and that the test must not be tagged SHARED-STMT-004.

**Shared behavior.** None is restated. The rule-specific sentences ("a `return` that ends a switch section is reported like any other", RH5009 likewise) contrast with SHARED-BLANK-004, which exempts only `break`. They do not repeat it. No SHARED-* requirement needs to change.

**Rule pages.** The rule-page edits match the code:

- `documentation/rules/RH5019.md`: the `yield`-after-`yield` exemption.
- `documentation/rules/RH5005.md`: a `using` declaration is checked by RH5029.

## Mechanism and candidates

_N/A._ This is not a bug fix. Candidate inputs come from the dispatch, by registered kind and defining type:

| Rule | Kinds (defining type) | Forms the code decides |
|---|---|---|
| RH5004 | `DoStatement` (`DoStatementSyntax`) | — |
| RH5005 | `UsingStatement` (`UsingStatementSyntax`) | `using (expr)`, `using (var x = …)`, `await using` (#946). `using var` is a `LocalDeclarationStatementSyntax`, so it is not dispatched |
| RH5006 | `ForEachStatement`, `ForEachVariableStatement` (`CommonForEachStatementSyntax`) | plain, deconstructing, `await foreach` (#946) |
| RH5007 | `ForStatement` | — |
| RH5008 | `ReturnStatement` | `return;`, `return e;`, terminal in a switch section |
| RH5009 | `GotoStatement`, `GotoCaseStatement`, `GotoDefaultStatement` (`GotoStatementSyntax`) | label, `case`, `default` |
| RH5012 | `ContinueStatement` | — |
| RH5013 | `ThrowStatement` | `throw e;`, `throw;`. A `ThrowExpression` is not dispatched |
| RH5014 | `SwitchStatement` | a `SwitchExpression` is not dispatched |
| RH5015 / RH5016 | `CheckedStatement` / `UncheckedStatement` (`CheckedStatementSyntax`) | `Checked`/`UncheckedExpression` are not dispatched |
| RH5017 | `FixedStatement` | — |
| RH5018 | `LockStatement` | — |
| RH5019 | `YieldReturnStatement`, `YieldBreakStatement` (`YieldStatementSyntax`) | previous statement a `YieldStatementSyntax` or not |
| RH5021 | `ExpressionStatement` (`ExpressionStatementSyntax`) | previous `LocalDeclarationStatementSyntax` (`var`, typed, `const`, `using`, `await using`) or not; expression an `AssignmentExpressionSyntax` (simple, compound, `??=`, tuple/deconstruction) or not (invocation, `++x`/`x++`, `await`, invocation with `out var`) |
| RH5029 | `LocalDeclarationStatement` (`LocalDeclarationStatementSyntax`) | first token `var`/type/`const`/`using`/`await`/`ref`/`scoped`; previous statement a declaration, another statement, a local function, or none |

## Breaking inputs

These are for the test implementer. Each qualifier of a requirement needs a test on both sides (FC-002, FC-003). The location needs a markup span on the token, not just a diagnostic count (FC-017).

- RH5005-001: `using var x = r;` after a statement, not reported. The other side, `using (r)`, is reported (existing test).
- RH5006-001: the deconstructing `foreach` is reported (existing test `VerifyDiagnosticForDeconstructionForeachStatementWithoutPrecedingBlankLine`).
- RH5008-001: a `return` that ends a non-first-statement switch section (e.g. `default: a++; return 0;`) is reported. The other side, a `return` that is the first statement after its label, is not reported (existing test `VerifyNoDiagnosticForReturnStatementAfterCaseLabel`).
- RH5009-001: the `goto` label, `goto case`, and `goto default` tests exist, and their gotos are terminal in their sections.
- RH5013-001: a `throw` expression (`?? throw`) is not reported.
- RH5014-001: a `switch` expression is not reported.
- RH5015-001: neither an `unchecked` statement nor a `checked(…)` expression is reported by RH5015.
- RH5016-001: the mirror case for RH5016.
- RH5019-002, the "directly" qualifier (FC-002):
  - a `yield` after `a++` is reported even when a `yield` statement appears earlier in the list;
  - `yield break` after `yield return` is exempt.
- RH5021-001:
  - an expression statement after a `using` declaration is reported (on its first token);
  - an expression statement after an expression statement is not reported.
- RH5021-002, both sides: a compound assignment (`c += 3;`) is exempt, and `d++;` is reported.
- RH5029-001/-002:
  - `const` and `using` declarations after a non-declaration are reported, on `const` and on `using`;
  - a `const` after a declaration is not reported.

Inputs that must **not** get a test in this run:

- `await using` / `await foreach` (thoenissen/Reihitsu#946).
- A block comment on the statement's line (thoenissen/Reihitsu#972).
- Deconstruction declarations (open question below).
- The over-indent test is not tagged SHARED-STMT-004 (Q1 of 01-spec-author.md).

Failure classes that reach this run:

- FC-002 and FC-003: the qualifiers above.
- FC-017: the location is pinned by markup.
- FC-034: tests take the spec examples' shape.
- FC-013: parity was checked; the formatter and the analyzers use the same predicates. No test is needed in this lane.

Classes that do not reach it: FC-004 to FC-012, FC-018, FC-020 to FC-030, and FC-036, because no rewrite, phase, or code fix changes. FC-019 and FC-005, the comment/directive gap, are shared behavior (SHARED-STMT-001/-003) and are already exercised per class by the `…WhenCommentLineDirectlyPrecedesIt` tests.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| 16 analyzers and `StatementShouldBePrecededByABlankLineAnalyzerBase<TStatement>` | must stay |
| 16 code-fix providers and `StatementShouldBePrecededByABlankLineCodeFixProviderBase` | must stay |
| Fix All (`WellKnownFixAllProviders.BatchFixer`, shared) | must stay |
| Formatter `BlankLinePhase` / `BlankLineStatementSpacingRewriter` | not involved (parity confirmed on the probed inputs) |
| `Reihitsu.Core` (`SyntaxTriviaUtilities`) | not involved |
| `shared/statement-blank-lines.md`, `shared/blank-line-spacing.md` | must stay (Q1 is filed, not edited) |
| `sdd/specs/analyzer/RH5005.md` (RH5005-002), `RH5006.md` (RH5006-002) | must change unless thoenissen/Reihitsu#946 confirms the keyword location for the `await` form |
| The other 14 new specs, `requirement-ids.txt`, `sdd/specs/README.md` | must stay as written |
| `documentation/rules/RH5005.md`, `RH5019.md` | must stay as changed (consistent with the code) |
| Tests under `src/Reihitsu.Analyzer.Test/Formatting/RH50xx…Tests.cs` | must change: `[Requirement]` tags and the focused tests listed above |

## Simpler option

_None._

## Questions for the maintainer

Neither question blocks the run. The specs leave both cases out, so they are filed as open questions per `sdd/README.md`.

### Does a deconstruction declaration count as a local declaration for RH5029 and RH5021?

Belongs to `SDD open questions: sdd/specs/analyzer/RH5029.md`. It touches RH5029-001, RH5021-001, RH5021-002, and FMT-BLANK-002, because the formatter uses the same predicate.

Input:

```csharp
class C
{
    void M(int a, (int, int) t)
    {
        a++;
        var (b, c) = t;
        var d = b + c;
        M(d, t);
    }
}
```

- **Reading A (today; analyzer and formatter agree):**
  - `var (b, c) = t;` is an expression statement, so RH5029 does not report it.
  - RH5029 reports `var d`.
  - RH5021 reports `M(d, t);` after `var d`.
  - Fixed and formatted output:
    ```csharp
            a++;
            var (b, c) = t;

            var d = b + c;

            M(d, t);
    ```
- **Reading B (a deconstruction declaration is a local declaration):**
  - RH5029 reports `var (b, c)`.
  - `var d` follows a declaration and is not reported.
  - Output:
    ```csharp
            a++;

            var (b, c) = t;
            var d = b + c;

            M(d, t);
    ```

Why reading A looks wrong: the blank line splits a run of two declarations, and the first declaration after `a++` stays unseparated. That works against the `Why:` of RH5029-001 ("A run of declarations reads as one unit").

### Q1 of 01-spec-author.md

I concur, with one addition. The question should quote the pinning test's own summary, which calls the over-indent "a known, documented limitation (see RH5103.md) … today's accepted behavior", so the maintainer can answer it as a clarification of SHARED-STMT-004.

## Follow-up candidates (out of scope)

- FC-015: the statement-kind predicates exist twice.
  - The analyzers' registrations and `IsRelevant` (RH5019, RH5021, RH5029).
  - `BlankLineStatementSpacingRewriter.NeedsBlankLineForStatementKind`.

  They agree today, but FC-013 parity for these rules rests on keeping both copies in step.
