Role: skeptic
Commit: fff0d5d
Verdict: #946: proceed; #972: proceed (wider class); #1096: proceed wider, output question; #1076: proceed, output question; #1626: output question (depends on #1625); #1625: output question

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The issues' own "Cause" paragraphs (#946, #972, #1096) are hypotheses; every mechanism below was established from the code and from runs, not taken from them.

Evidence: fixtures under the scratchpad, run with `scripts/apply-fix.sh <ID> <dir>`, which runs each fixture under LF and CRLF, and `scripts/format.sh <dir>` on copies. Every result quoted holds for LF and CRLF alike. Fixtures were deleted afterwards.

Reachability: every reported case below is reached by an analyzer, which runs on any source the compiler sees, and by its code fix, a caller under `AGENTS.md`, *Supported inputs*. No case in this run depends on a direct call of a formatter entry point.

## thoenissen/Reihitsu#946

### Premise

Real. RH5005 `var x = 1;⏎await using (resource) { … }` and RH5006 with `await foreach`: `not-converged (10 iteration(s))` in every arm. Each application inserts one more line break between `await` and `using`/`foreach`. Also `not-converged`: the same-line form `var x = 1; await using (…)` and a `// note` line above `await using`. The formatter puts one blank line above `await`, which is the expected output.

### Mechanism and candidates

Mechanism: the analyzer measures the gap in front of the token that `GetPreviousToken` starts from. The fix inserts the line break in front of the token at the diagnostic location (`RegisterCodeFixesAsync` → `root.FindToken(diagnostic.Location.SourceSpan.Start)`). Wherever the two tokens differ, the line break lands inside the statement and the measured gap never changes. The analyzer and its fix decide the token independently.

Dispatch: the overrides of `StatementShouldBePrecededByABlankLineAnalyzerBase<TStatement>`, `GetPreviousToken` against `GetLocation`, for each rule. RH5001–RH5004, RH5007–RH5010, and RH5012–RH5019 use the same keyword for both. RH5021 and RH5029 use `GetFirstToken()` for both. Only two rules differ:
- RH5005 (`UsingStatementSyntax.AwaitKeyword` against `UsingKeyword`);
- RH5006 (`CommonForEachStatementSyntax.AwaitKeyword` against `ForEachKeyword`).

No other member exists:
- a labeled statement's parent is not a block or switch section, so it is excluded (SHARED-STMT-002);
- `await using var` is a local declaration, so RH5029 measures it from its first token.

### Breaking inputs

FC-007 (convergence) and FC-017 (LF and CRLF). Every cell below fails:

| Rule | Statement starts its own line | Shares the previous statement's line | Comment line above it |
|---|---|---|---|
| RH5005 `await using (…)` | not-converged | not-converged | not-converged |
| RH5006 `await foreach (…)` | not-converged | not-converged | not tested; same path |

Tests: RH5005 and RH5006, the three columns, LF and CRLF, one application, and Fix All (the base uses `BatchFixer`). Mark the diagnostic where it is today, on `using`/`foreach`.

### Surfaces

| Surface | |
|---|---|
| Analyzer RH5005/RH5006 | must stay (location unchanged, see *Internal choices*) |
| Code-fix base | must change (insertion token) |
| Formatter | not involved (already correct) |
| Rule pages | not involved |
| Tests | must change: add the `await` forms |

### Refuse option

Withhold the fix for `await` statements. That would be a visible loss for common code (`await foreach` is ordinary), and the requested change is a token choice of a few lines, so refusing costs more than fixing. Declaring a shape unsupported is not an option: the formatter handles the shape, so declaring it would delete nothing. _No simpler option._

### Output questions

_None._ See below for 01-spec-author's question 1.

### Internal choices

- **01-spec-author question 1 (diagnostic location on `await using`/`await foreach`) is not an output question.** Keeping the diagnostic on `using`/`foreach` is today's output and does not look wrong; RH7004 makes the same choice. Recording today's location in RH5005-002/RH5006-002 changes no visible output, so the lead can decide it (`sdd/README.md`, *Open questions*: a question that changes no visible output is not an open question). Recommendation: keep it on the keyword, decide it without asking, and write the one sentence into RH5005-002/RH5006-002 so the tests can mark it.

### Issue goals

The "Expected" text (blank line in front of `await`) stands. The "Cause" paragraph is accurate. _No goal to drop._

### Architecture findings

A small one: the analyzer and its code fix each decide, independently, the token whose gap is measured and filled. Input: `var x = 1;⏎await using (r) { }`. A narrow fix is enough. The base is the one code fix owner, so after the fix nothing remains in two places as long as the fix derives its token from the statement, not from the diagnostic.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#972

### Premise

Real. RH5002 `a++;⏎/* block */ if (a > 7) { }` gives two blank lines above the comment. RH5008 (`return`), RH5029 (local declaration), and RH5021 (expression statement after a declaration) show the same on the base. The formatter puts exactly one blank line there. The input violates RH5104 (`apply-fix.sh RH5104` reports and fixes it), so the class is rare by `AGENTS.md` (*Special cases do not grow the source*). The maintainer has decided it as a bug all the same (issue body, link to #929).

### Mechanism and candidates

Mechanism: `ApplyCodeFixAsync` takes the two-line-break rebuild (`ReplaceFormattingOnlyGap`) whenever the last line of the gap holds content in front of the statement (`TokenGapAnalysis.HasTerminalLineBreak == false`). It does not ask whether a line break outside any comment already ends the line above. When one does — the statement's line starts with the comment — the existing line break plus the two inserted ones make two blank lines.

`HasTerminalLineBreak` answers "does the gap's last line hold content", not "is the statement still on the previous code's line". Dispatch: the two branches of `StatementShouldBePrecededByABlankLineCodeFixProviderBase.ApplyCodeFixAsync`, shared by RH5001–RH5010, RH5012–RH5019, RH5021, and RH5029.

### Breaking inputs

FC-019, FC-021, FC-016. Class matrix, RH5002 with `if (a > 7) { }`. The axes are what ends the previous line, and the trivia on the statement's line in front of it:

| Gap between `a++;` and `if` | Result |
|---|---|
| `⏎/* block */ ` (issue) | **2 blank lines** |
| `⏎/* above */⏎/* block */ ` (comment line above, block comment on the statement's line) | **2 blank lines** above `/* above */` |
| `⏎/* x */ /* y */ ` | **2 blank lines** |
| `⏎/* a⏎b */ ` (multi-line comment starting on its own line, ending on the statement's line) | **2 blank lines** |
| ` // x⏎/* b */ ` (trailing comment on the previous line) | **2 blank lines** |
| `⏎/** d */ ` | **2 blank lines** |
| ` /* c */ ` (everything on one line) | holds: one blank line; `if` moves to its own line and the comment stays behind `a++;` |
| ` /* a⏎b */ ` (every line break inside the comment) | holds: one blank line, `if` moved below the comment |
| `⏎/* b */ // c⏎` (the comment line ends before the statement) | holds |

Tests: the failing rows for RH5002, plus at least one other rule (RH5029 or RH5008), LF and CRLF. Keep the holding rows as boundary tests (FC-003).

### Surfaces

| Surface | |
|---|---|
| Code-fix base | must change |
| Analyzers | not involved |
| Formatter | not involved (already gives one blank line) |
| `TokenGapAnalysis` (Core, shared with the formatter's `BlankLineEditor`) | must stay; its formatter consumers are correct |
| Rule pages | not involved |

### Refuse option

Withhold the fix when a comment shares the statement's line behind a line break outside comments. The class is rare (RH5104), so withholding would be legitimate. It would contradict the maintainer's decision ("exactly one blank line"), though, and the defect is a branch choice in existing code, with no new mechanism. Fixing is the cheaper option.

Declaring unsupported "a comment between two statements of one list that ends on the later statement's line" is not recommended. The formatter handles every row correctly, so no formatter special case would be deleted. It would refuse whole files with `a++; /* c */ b++;` and `/* note */ if (…)`, which the formatter formats today.

### Output questions

_None._ The issue gives the output, and SHARED-STMT-004's "exactly one blank line" already decides every failing row.

### Internal choices

- SHARED-STMT-004's new sentence covers "a comment that starts the statement's line". The row `⏎/* a⏎b */ if` is a comment that starts its own line and ends on the statement's line. Literally, it starts a line but not the statement's line. Its only output that keeps the comment and gives one blank line is the blank line above `/* a`. Recommendation: the spec author words the sentence as "a comment that starts a line and ends on the statement's line, in front of it", so the row is decided by text and not only by "exactly one".
- The RH5002 non-goal named in #972's comment is already absent (01-spec-author). The PR should say so.

### Issue goals

Keep. The rule list "RH5001–RH5010, RH5012–RH5019" omits RH5021 and RH5029, which share the base and fail too. Widen the goal to every rule on the base; that is internal.

### Architecture findings

_None_ beyond the #1626 finding below.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1096

### Premise

**Q1 — indentation after a multi-line `case` label.** Real, but only inside an object initializer or anonymous object, where `GetIndentation` takes its anchor-scope branch. In a plain method, `case 1 when⏎    Check(value): Method(1); var y = 1;` already puts `var y` in the column of `break;` (RH5029, `fixed`). The issue's "4 columns deeper" holds for the same input inside an initializer's lambda: `var y` lands at column 28 while `break;` is at 24. The existing test `RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests.VerifyDiagnosticOverIndentsWhenLabelSpansMultipleLinesAndPrecedingStatementSharesItsContinuationLine` pins the wrong column. Severity is low: the input already violates RH5103, so the class is rare.

**Q2 — the whitespace-only line below `///`.** Real for RH5002 and RH5011, with an empty line and with a line of spaces alike. The analyzers report, and the fix inserts a second blank line above `/// doc`. The issue's cause, `ContainsBlankLine` resetting on the doc-comment trivia, is accurate. The class is wider than `///` (below).

### Mechanism and candidates

**Q1.** In the anchor-scope branch, `SyntaxIndentationUtilities.ComputeAnchorDerivedIndentation` reads the leading whitespace of the line where the preceding sibling starts. It adds one level whenever that line's first content lies inside any label's span (`IsWithinSwitchSectionLabelRegion`). The added level is right only when the line starts with the label's first token (`case`/`default`). On a continuation line of a label, which is already indented, it overshoots by the continuation's extra indentation.

Dispatch:
- `GetIndentation` in `StatementShouldBePrecededByABlankLineCodeFixProviderBase` (all 20 base rules), with `HasAnchorScopeAncestor` (`InitializerExpressionSyntax`, `AnonymousObjectCreationExpressionSyntax`) choosing the branch.
- The same helper is used by `RH5103CodeMustNotContainMultipleStatementsOnOneLineCodeFixProvider`; that one is the spec author's follow-up.
- `TargetAttributePlacementCodeFixProviderBase` uses `HasAnchorScopeAncestor` but not the label region, so it is not affected.

**Q2.** `SyntaxTriviaUtilities.ContainsBlankLine` counts a blank line only between two `EndOfLineTrivia` with only whitespace between them. A trivia whose own text ends with the line break resets that state, so the line directly below it is never counted as blank, even when it holds only whitespace. Such trivia are a `///` comment parsed as structured trivia, every directive, and disabled text. Callers: `StatementShouldBePrecededByABlankLineAnalyzerBase` (20 rules) and `StatementShouldBeFollowedByABlankLineAnalyzerBase` (RH5011).

The formatter and the code-fix base measure with `TokenGapAnalysis`, which reads the line break inside a trivia's text and counts the line correctly. SHARED-BLANK-001's Why calls `TokenGapAnalysis` "the one measure", but the analyzers use a second one (see *Architecture findings*).

### Breaking inputs

FC-027 (initializer scope), FC-016, FC-020, FC-040, FC-034.

**Q1 matrix** (RH5029 unless noted; inside an initializer's lambda unless noted):

| Label shape and the line the preceding statement starts on | Result |
|---|---|
| `case 1 when⏎    Check(value): Method(1); var y = 1;` (issue) | **4 deeper** than `break;` |
| `case 1⏎    when Check(value): Method(1); var y = 1;` | **4 deeper** |
| `case 0:⏎case 1 when⏎    Check(value): …` (two labels) | **4 deeper** |
| guard spanning three lines, statement on the third | **4 deeper** |
| the same in an anonymous object (`new { A = (Action<int>)(value => { switch … }) }`) | **4 deeper** |
| RH5002 with `if (value > 2) { }` instead of `var y` | **4 deeper** (whole base) |
| `case 1: Method(1); var y = 1;` (single-line label) | holds |
| `case 1 when⏎    Check(value):⏎    Method(1); var y = 1;` (statement starts its own line) | holds |
| any of these in a plain method (no initializer) | holds (canonical depth) |

The regression test must use the initializer shape. The plain-method shape of the spec example already passes, so it is no reproduction (FC-034).

**Q2 matrix** (`a++;`/`break;` then the trivia shown, then the statement):

| Trivia | SHARED-STMT (RH5002) | RH5011 |
|---|---|---|
| `⏎/// doc⏎⏎` (issue) and `⏎/// doc⏎    ⏎` | **reported**, fix adds a 2nd blank line | **reported**, same |
| `⏎/// doc⏎⏎// note⏎` | **reported** | not tested |
| `⏎#endif⏎⏎// note⏎` (after an `#if` block) | **reported**, fix puts a blank line above `#if` | exempt (directive first, RH5011-002) |
| `⏎#pragma warning disable X⏎⏎// note⏎` | **reported** | exempt (directive first) |
| `⏎// c⏎#pragma warning disable X⏎⏎` | exempt (SHARED-STMT-003, directive last) | **reported** |
| `⏎/** doc */⏎⏎` | holds (not reported) | not tested |

Every **reported** row has a line holding only whitespace between two tokens, which the unchanged first sentence of SHARED-BLANK-001 already counts as blank. So the directive rows are code-against-spec bugs of the same mechanism, not new behavior. The `#endif` row is common code.

Documentation mode (FC-040): `///` reproduces under the documentation mode `apply-fix.sh` parses with. Under `DocumentationMode.None`, `///` is an ordinary comment without its line break; I did not run that mode. The tests should cover the `///` rows in both modes, so the analyzer and the formatter, which may parse with different modes, give one answer (FC-040).

### Surfaces

| Surface | |
|---|---|
| Code-fix base `GetIndentation` / `SyntaxIndentationUtilities` (Core) | must change (Q1) |
| `SyntaxTriviaUtilities.ContainsBlankLine` (Core) or its two analyzer bases | must change (Q2) |
| RH5103 code fix | must stay (follow-up); it shares `ComputeAnchorDerivedIndentation`, so a change inside that helper changes RH5103 too, which the lead must decide |
| Formatter | not involved |
| Rule pages | 20 shared-base pages already changed by 176b419 (see *Internal choices*); RH5103 page must stay |

### Refuse option

- **Q1.** Withhold the fix in the anchor-scope branch when the anchor line starts inside a label but not at its first token. The class is rare (an RH5103 violation inside an initializer), so withholding would be legitimate. It contradicts the maintainer's decision, though, and the existing branch already has the facts needed: where the label's first token stands. Fixing is cheap.
- **Q2.** No refuse option: it is an analyzer false positive, not a fix.
- Declaring a shape unsupported deletes nothing for either question, because the formatter handles both shapes.

### Output questions

**1 · RH5011 · break-Statement should be followed by a blank line — a documentation comment between `break` and the next statement, no blank line**

RH5011-001 says a `break` is reported only when the trivia "holds no documentation comment". 01-spec-author relies on that clause for the RH5011 half of #1096. The code has never implemented it, and the existing test `VerifyFixInsertsBlankLineBelowDelimitedDocumentationCommentSpanningBreakLine` (tagged RH5011-007) asserts the opposite: `break; /** a⏎b */⏎a++;` is reported and fixed. The clause has no Why. Implemented literally, it would also silence the input below, which RH5002 (SHARED-STMT-001) reports for the same trivia.

Input:
```csharp
while (true)
{
    break;
    /// doc
    a++;
}
```

Before (`main`): `break` reported; fix → `break;⏎⏎/// doc⏎a++;`.

After, A (recommended): unchanged (reported, same fix). RH5011-001 loses "holds no documentation comment", and #1096's case is decided by SHARED-BLANK-001 alone:
```csharp
while (true)
{
    break;

    /// doc
    a++;
}
```

After, B: not reported (the clause implemented as written); the existing RH5011-007 test's expected diagnostic changes too.

Why A: `///` inside a method documents nothing and is an ordinary comment everywhere else (comment-placement, SHARED-STMT-001). The clause contradicts the code, the tests, and RH5002.

**2 · SHARED-STMT-004 · the column of the moved statement inside an initializer — written relative to the label, or the formatter's column**

The spec text says the moved statement takes "the column in which the formatter puts the statements of their block or switch section". Inside an object initializer, the formatter's column comes from FMT-INDENT-022's anchoring of the whole initializer, which the fix cannot know without predicting the indentation phase. On unformatted input the two readings give different columns. On formatted input they agree.

Input (RH5029, initializer not laid out as the formatter would):
```csharp
var c = new C
{
    A = value =>
    {
        switch (value)
        {
            case 1 when
                Check(value): Method(1); var y = 1;
                break;
        }
    }
};
```

Before (`main`): `var y = 1;` 4 columns deeper than `break;`.

After, A (recommended): `var y = 1;` in the column of `break;`, the section's statements as written:
```csharp
            case 1 when
                Check(value): Method(1);

                var y = 1;
                break;
```

After, B: `var y = 1;` in the column the formatter would give after re-laying out the whole initializer, which is not the column of `break;` in this text.

Why A: it is what the maintainer decided ("indented like the statement it follows"), and it needs no prediction of the indentation phase (role question 10). The spec author then replaces "the column in which the formatter puts …" with "the column of the other statements of its block or switch section".

### Internal choices

- **SHARED-STMT-004's sentence "This requirement does not decide a switch section whose label stands on the line of the switch statement's opening brace"** is a note about an undecided case. `sdd/README.md`, *Open questions*, forbids that: "the case never goes into a spec — no requirement, no non-goal, no note". This is the same objection #972's maintainer comment raised against the RH5002 non-goal. Recommendation: delete the sentence; the follow-up issue carries the question.
- **SHARED-BLANK-001's new sentence names only `///`.** The failing class includes the line below every directive, and the first sentence already covers it. Recommendation: either word it as "the line directly below a trivia that ends its own line — a `///` comment or a directive" or drop the special sentence and keep the example. That is wording only; the directive rows need tests either way.
- **The SHARED-STMT-004 example** shows the plain-method shape, which passes on `main`. Recommendation: let the example, or a second one, show the initializer shape that fails, so the requirement's example is the decided case.
- **The paragraph removed from 20 rule pages** (176b419) does not describe #1096's shape. It describes the label-on-the-switch's-line shape that stays unfixed, and refers to RH5103's limitation. The maintainer's #1096 answer ("remove that sentence") covers it only indirectly. The removal rests on `AGENTS.md`'s rule-page convention: no implementation detail, no corner cases. Rule pages are a visible surface, so the PR should state that reason. I would keep the removal.
- The source comment on `GetIndentation` and the test comment that call the multi-line label "documented limitation (see RH5103.md)" go stale (FC-031); 01-spec-author noted it already.

### Issue goals

- Q1's goal, "remove any rule-page sentence documenting it", has no matching sentence on the 20 shared-base pages. `RH5103.md:24` describes RH5103's own fix and stays.
- Otherwise keep both goals.

### Architecture findings

1. **Two measures of "blank line" (FC-015).** `SyntaxTriviaUtilities.ContainsBlankLine` (the analyzers of RH5001–RH5019, RH5021, RH5029, RH5011) and `TokenGapAnalysis` (the formatter's blank-line phase and the code-fix base) both decide SHARED-BLANK-001. They disagree on the line below any trivia that carries its own line break. Inputs: `break;⏎/// doc⏎⏎a++;`, `a++;⏎#endif⏎⏎// note⏎if (…)`. This is a defect of a run issue, so it is an architecture finding of the run. Local fix and structural fix (analyzers measure with `TokenGapAnalysis`) both stay inside Core and the two analyzer bases.
2. **The code fix predicts the indentation phase's column (Q1).** `GetIndentation` / `ComputeStatementIndentLevel` / `ComputeAnchorDerivedIndentation` compute the moved statement's column, which `IndentationPhase` owns (FMT-INDENT-003). The multi-line label is where the prediction and the formatter disagree, and RH5103's fix shares the prediction. The class is rare, so refusing it or fixing it narrowly are both open; the lead decides.

### Follow-up candidates

- **Formatter · a wrapped `when` condition of a `case` label is put in the column of `case`.** Outside the run's issues; it appears in this run's inputs. FMT-INDENT-021's Why says the guard must not "sit at the label's own column, where it would look like another label", but only covers `when` starting a line.

  Input:
  ```csharp
  switch (value)
  {
      case 1 when
          Check(value):
          Method(1);
          break;
  }
  ```
  Before (`main`, `scripts/format.sh`, stable on a second pass):
  ```csharp
  switch (value)
  {
      case 1 when
      Check(value):
          Method(1);
          break;
  }
  ```
  After, A (recommended): `Check(value):` one level deeper than `case`, as written in the input. After, B: unchanged.
  Why A: the continuation belongs to the label, as FMT-INDENT-021 argues for `when`.

- The spec author's RH5103 candidates stand. The multi-line-label member there has the same mechanism as Q1, through the shared `ComputeAnchorDerivedIndentation`.

## thoenissen/Reihitsu#1076

### Premise

Every remaining instance is real:

- **Runs of blank lines.** RH5022, RH5024, RH5025, RH5026, and RH5027 each need two iterations for two blank lines (`fixed (… 2 iteration(s))`), also when the lines hold spaces (RH5022).
- **RH5023 at the end of the file.** `"class C\n{\n}\n\n"`: `no-progress (1 iteration(s))`.
- **RH5011 before `else` / do-footer `while`.** `if (a > 0) break; else a++;`, its multi-line form, `do break; while (…);`, and its multi-line form are all reported. The fix inserts a blank line before `else`/`while`; on one line it splits `else a++;` onto its own line below a blank line.
- **`#region` cycles.** RH5024 deletes the blank line in `#region R⏎⏎}` (inside a method and at type level), and RH5025 deletes it in `#region A⏎⏎{ "b", 2 },` and below `#endregion // A`. `apply-fix.sh RH5032` on each result inserts the line again: a confirmed SHARED-FIX-004 cycle.
- The RH5025/RH5030 instance is resolved on `main`, as the issue says. I did not re-run it.

### Mechanism and candidates

- **Runs.** Each fix deletes only the reported line's span (SHARED-WS-001), and each analyzer reports one line per brace or keyword, so a run of N lines needs N applications. Candidates: the five rules named, which are every blank-line-deleting rule anchored on a brace or keyword. RH5023 already reports every extra line itself, and RH5028 (start of file) is outside #1076.
- **End of file.** `FormattingTextAnalysisUtilities.EnumerateConsecutiveBlankLines` yields the last `TextLine`, which has no line break, as a blank line.
- **RH5011.** `StatementShouldBeFollowedByABlankLineAnalyzerBase` never asks whether the next token starts a new statement. The candidates are the tokens that can follow `break;` and continue its enclosing statement: `else` and the `while` of a `do`. `catch`/`finally` cannot follow `break;` directly, and `case`/`default` start a new section, which RH5011-001 reports on purpose.
- **Region cycles.** A blank-line-deleting rule reports a blank line without checking whether the nearest line above it holds a region directive that SHARED-BLANK-006 requires a blank line below. Candidates: RH5024 (`}`), RH5025 (`{`), RH5026 (`else`/`catch`/`finally`), RH5027 (do-footer `while`). RH5022 is no member: the line above its blank line ends with `{`.

### Breaking inputs

FC-007, FC-008 (Fix All over a run), FC-020, FC-013, FC-017.

| Instance | Member | Result on `main` |
|---|---|---|
| Runs | RH5022/24/25/26/27, two empty lines | 2 iterations |
| Runs | RH5022, two lines holding spaces | 2 iterations |
| End of file | `}\n\n` | reported, no-progress |
| End of file | `}\n\n\n` | 2nd line fixed, then end of file reported, no-progress |
| End of file | `}\n\n    ` (spaces on the last line, no line break) | reported; fix strips the spaces, then no-progress |
| End of file | `// c\n\n` and `#if X\n#endif\n\n` at the end | reported, no-progress |
| End of file | `}\n    ` (no blank line before) | holds |
| RH5011 | `else` one line / multi-line; `do … while` one line / multi-line | reported (4 failing) |
| RH5011 | `break;⏎while (…) …;` (a `while` statement) | reported, correctly (RH5011-008 keeps it) |
| Region | RH5024 `#region⏎⏎}` in a method / at type level | deleted, RH5032 re-inserts |
| Region | RH5025 `#region`/`#endregion` ⏎⏎ `{ … },` initializer element | deleted, RH5032 re-inserts |
| Region | RH5025 `void M()⏎#region R⏎⏎{` | deleted, RH5032 re-inserts |
| Region | **RH5026** `}⏎#region R⏎⏎else` | **deleted, RH5032 re-inserts** — not covered by the spec diff |
| Region | **RH5027** `}⏎#region R⏎⏎while (…);` | **deleted, RH5032 re-inserts** — not covered by the spec diff |

Tests add the spaces-on-the-last-line member to RH5023-001's end-of-file examples. RH5023-001's "a blank line that ends with a line break" already excludes it. The run tests need one application and Fix All in one iteration.

The formatter keeps the blank line below `#region R` in front of `else` and of the do-footer. So once its run is fixed, RH5026/RH5027 would still report formatter output (FC-013).

### Surfaces

| Surface | |
|---|---|
| RH5022/24/25/26/27 code fixes | must change (runs) |
| RH5023 analyzer (or `EnumerateConsecutiveBlankLines`, Core) | must change |
| RH5011 analyzer | must change |
| RH5024/RH5025 analyzers | must change (region) |
| RH5026/RH5027 analyzers | must change if output question 3 is answered A |
| RH5032, formatter | not involved |
| Rule pages | not involved (a narrowed report needs no page change) |

### Refuse option

- Runs, end of file, and RH5011 are not rare: files ending with `\n\n`, double blank lines, and `if (…) break; else` are ordinary code. No refuse option.
- The region members are rare: a region directive inside a statement or between a header and its body violates RH7303. Refusing the fix alone would not end the conflict, because RH5032 still inserts the line the other rule reports. The cheap guard ("nearest non-blank line above is an active region directive") is the requested change.
- Declaring the shape unsupported deletes nothing: the analyzers keep reporting, and the formatter has no guard for it.

### Output questions

**3 · RH5026 · Chained statement blocks must not be preceded by a blank line, and RH5027 · While-do footer must not be preceded by a blank line — a blank line below a region directive**

The same class as the RH5024/RH5025 instances the maintainer decided in #1076 (reading A: the blank line SHARED-BLANK-006 requires stays). RH5026-001 and RH5027-001 report it, RH5032 inserts it again, and the formatter keeps it. The lead may judge it covered by the #1076 answer. If not, it is asked:

Input:
```csharp
if (a > 0)
{
}
#region R

else
{
}
#endregion
```

Before (`main`): `RH5026` reports the blank line below `#region R`; its fix deletes it; RH5032 reports `#region R` and inserts it again.

After, A (recommended): not reported by RH5026, likewise for RH5027 in front of a `do` statement's `while`. The input stays as is.

After, B: unchanged; the two fixes keep alternating.

Why A: it is the decision the maintainer took for RH5024/RH5025, and the requirement and its guard repeat RH5025-007 for two more rules. These are the same analyzers the run already changes for runs of blank lines, so it is an adjacent fix (`squad/README.md`, *Scope*).

### Internal choices

- RH5024-001's text keeps "a directive other than `#region`" and adds the run sentence. Consistent with SHARED-BLANK-006 (`#endregion⏎⏎}` stays reported).
- RH5025-007's example uses an initializer element whose brace RH5025-001 does not list (#1081). The negative example is valid either way; no change.

### Issue goals

"Nothing is reported where no fix can change anything": the RH5023 end-of-file instance meets it. Keep every goal.

### Architecture findings

The region class is one rule — SHARED-BLANK-006's "a blank line follows a region directive" — that four deleting rules contradict, each without the exemption. That is a rule stated in one place and ignored in four (FC-015). Each analyzer can carry the same cheap check. A shared helper is the lead's choice.

### Follow-up candidates

_None_ if output question 3 is taken into the run. Otherwise RH5026/RH5027 become a follow-up in that form; it replaces 01-spec-author's candidate, which this run reproduced.

## thoenissen/Reihitsu#1626

### Premise

Partly real, and narrower than the code. More than two components decide what to do with a statement that follows a statement or a brace when a multi-line comment ends on the statement's line, and they give three different answers:

| Component | Rule | Answer |
|---|---|---|
| `StatementShouldBePrecededByABlankLineCodeFixProviderBase` | RH5001–RH5019, RH5021, RH5029 | moves the statement below the comment, one blank line (`a++; /* a⏎b */ if` → fixed) |
| RH5011 code fix | RH5011-007 | withholds ("where it goes relative to the comment is the author's decision") |
| `BlankLineCodeFixUtilities.InsertBlankLineBefore` | RH5030 | withholds via RH5030-003, with the same Why |
| `BlankLineCodeFixUtilities.InsertBlankLineBefore` | RH3208 | writes a trailing space and puts the statement at column 0 (#1625) |
| Formatter | — | moves the statement below the comment with one blank line, like the base |

The issue names only the base and the helper. RH5011's fix (RH5011-005/-006/-007) decides the same gap with its own code, and RH5103's fix moves same-line statements with the shared indentation helper. "One owner" across the base and the helper alone would not make one owner.

### Mechanism and candidates

_N/A_ as a defect of its own. The refactor has no visible output except through #1625.

### Breaking inputs

FC-015, FC-016. If the helper takes over the base's gap rebuild, it brings the base's #972 defect and its Q1 indentation prediction with it. Every row of the #972 and #1096 Q1 matrices then applies to RH5030 and RH3208 too, and the tests must cover them there.

### Surfaces

| Surface | |
|---|---|
| Code-fix base, `BlankLineCodeFixUtilities` | must change only if #1625 is answered A |
| RH5030 | must stay (RH5030-003 unchanged; narrowing it is the spec author's follow-up) |
| RH5011 code fix | not involved in this run (a third owner; follow-up) |

### Refuse option

Not doing the refactor. Its stated purpose is to fix RH3208 (#1625) through the base and to allow narrowing RH5030-003 later. If #1625 is answered D or C (below), nothing in this run needs it. #1626 then gets `Refs #1626`, and the consolidation becomes a follow-up that includes RH5011. Declaring a shape unsupported: see #972; it deletes no formatter special case.

### Output questions

_None of its own._ Its outcome follows output question 4.

### Internal choices

- If #1625 is answered A, the consolidation is the cheapest way to produce it, by reusing the base's rebuild. Then update the *Code owners* rows (01-spec-author).
- If D or C, skip the refactor.

### Issue goals

- "One component decides how the blank line in front of a statement is inserted": unreachable as stated without RH5011's fix. Narrow it to "the base and `BlankLineCodeFixUtilities`" or drop it with #1625's answer.
- "Every existing RH5001–RH5030 and RH3208 code-fix test still passes": keep.

### Architecture findings

**One decision, four answers (FC-015).** What a fix does with a statement that a multi-line comment ends in front of is stated in SHARED-STMT-004 (move), RH5011-007 (withhold), and RH5030-003 (withhold), and RH3208 does a fourth thing. The Whys of RH5011-007 and RH5030-003 ("the author decides where the statement goes relative to the comment") contradict what SHARED-STMT-004's fix and the formatter already do. This reaches the run through #1625.

### Follow-up candidates

- **One answer for "a statement behind a multi-line comment" across RH5011, RH5030, and the statement blank-line base.** It includes narrowing RH5030-003 and RH5011-007's withheld case, both visible. This supersedes 01-spec-author's RH5030-003 candidate, or extends it.

## thoenissen/Reihitsu#1625

### Premise

Real. `int L() => 1; /* a⏎b */ L();` gives `} /* a⏎    b */ ⏎L();`: a trailing space behind `b */` and `L();` at column 0, LF and CRLF.

Severity is overstated. The issue cites SHARED-FIX-003, but SHARED-FIX-003 excludes layout ("Layout does not count"). The comment's text and position stay intact and the result compiles. It is a layout defect of a rare input: RH5104 reports the trailing multi-line comment. No safety rail is hit.

### Mechanism and candidates

`InsertBlankLineBefore` inserts one line break at the start of the next statement's leading trivia, behind any directive. That separates the statements only when a line break outside any comment already ends the line above. When every line break of the gap lies inside a comment, the inserted line break lands behind the comment's trailing space, and the statement starts at column 0.

Dispatch:
- the callers of `InsertBlankLineBefore`: RH5030 withholds via RH5030-003; RH3208 calls it whenever `BlankLineSpacingPolicy.RequiresBlankLineAfterClosingBrace` holds.
- `RequiresBlankLineAfterClosingBrace` is true here, because the gap holds a line break, even if only inside the comment.

### Breaking inputs

FC-021, FC-016, FC-017.

| RH3208 input after `int L() => 1;` | Result on `main` |
|---|---|
| ` /* a⏎b */ L();` (issue) | **trailing space, `L();` at column 0** |
| ` /* a */ L();` | `} /* a */ L();` — `L();` stays on the brace's line, no blank line (holds as RH3208 treats a same-line statement) |
| ` L();` | `} L();` (holds, same) |
| ` /* a⏎b */⏎L();` (statement on its own line) | `} /* a⏎b */⏎⏎L();` (holds) |

The formatter, run on the issue's input, gives `} /* a⏎    b */⏎⏎    L();`, which is option A below.

### Surfaces

| Surface | |
|---|---|
| RH3208 code fix | must change |
| `BlankLineCodeFixUtilities` | must change for A (with #1626), or must stay for D |
| RH5030 | must stay |
| Formatter | not involved |
| RH3208 spec | must change: RH3208-002 or SHARED-AFTERBRACE-001 gets the decided case |

### Refuse option

Two refuse options, both cheap guards that reuse RH5030-003's check. The class is rare (RH5104).
- **C**: withhold the whole RH3208 fix for this input.
- **D**: convert the body but insert no blank line when every line break of the gap lies inside a comment.

D is the better of the two: the conversion is RH3208's own job, and D does for the gap exactly what RH3208 already does for a statement on the brace's line (`} L();`). The requested change (A) costs the #1626 consolidation, or a copy of the base's gap rebuild, which a copy must not be (FC-015).

Declaring unsupported deletes nothing: the formatter handles the shape (it outputs A).

### Output questions

**4 · RH3208 · Expression style local functions should not be used — next statement behind a multi-line comment**

This extends 01-spec-author's question 2 with option D.

Input:
```csharp
void M()
{
    int L() => 1; /* a
    b */ L();
}
```

Before (`main`):
```csharp
void M()
{
    int L()
    {
        return 1;
    } /* a
    b */ 
L();
}
```

After, A (recommended):
```csharp
void M()
{
    int L()
    {
        return 1;
    } /* a
    b */

    L();
}
```

After, B (the issue's suggestion; RH5030 then reports `}` and its fix inserts the blank line, as in A):
```csharp
    } /* a
    b */
    L();
```

After, C: no fix offered; the RH3208 diagnostic stays.

After, D: the body is converted, the statement stays where it is; RH5030 then reports `}` with no fix (RH5030-003):
```csharp
    } /* a
    b */ L();
```

Why A: it is the formatter's output for the same input and the statement blank-line fixes' output for the sibling shapes (`a++; /* a⏎b */ if …`, `var x = 1; /* a⏎b */ x++;`), and it needs no new mechanism if the base's rebuild is shared (#1626).

What would change the pick to D: if the lead does not want the #1626 consolidation in this run. D is then cheaper, consistent with RH5030-003 and with RH3208's treatment of a statement on the brace's line, and leaves #1626 as a follow-up.

### Internal choices

_None._

### Issue goals

"Fixed in one application … or the fix is withheld and the spec says so": both are within the options above. "Covered by a RH3208 code-fix test with LF and CRLF": keep.

### Architecture findings

See #1626.

### Follow-up candidates

_None._

## Whole run

- Output questions for the batch before tests: 1 (RH5011-001 doc-comment clause), 2 (SHARED-STMT-004 column wording, initializer), 3 (RH5026/RH5027 region, possibly covered by #1076's answer), 4 (RH3208, which replaces 01-spec-author's question 2 and adds D). 01-spec-author's question 1 needs no maintainer answer (see #946).
- Spec-diff findings for the spec author (internal):
  - delete the "does not decide a switch section whose label stands on the line…" sentence from SHARED-STMT-004;
  - widen SHARED-BLANK-001's `///` sentence to every trivia that ends its own line, or drop it;
  - word SHARED-STMT-004's comment sentence so a comment that starts a line and ends on the statement's line is covered;
  - let SHARED-STMT-004's example show the failing initializer shape;
  - add the spaces-on-the-last-line example to RH5023-001.
- Architecture findings: two blank-line measures (`ContainsBlankLine` against `TokenGapAnalysis`); the code fix's prediction of the indentation column (Q1, shared with RH5103); SHARED-BLANK-006 ignored by four deleting rules; one decision with four answers for a statement behind a multi-line comment (#1626/#1625).
- No production, test, or spec file was changed by this role.
