Role: auditor
Commit: 0260784
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head 0260784 (`claude/dreamy-curie-x5c135`, checked out, clean), base `origin/main` e3924e22321a0b8a0c0b72ce185c2ed6fc81f9b7 (`git ls-remote` agrees, merged into the head). The diff is what the handovers describe: three production files (`GenericConstraintLineBreakRewriter`, `GenericConstraintContributor`, `SingleSpaceSpacingRule`), the formatter and RH5110 tests, the specs FMT-BREAK-037, FMT-INDENT-032, FMT-SPACE-009/019, RH5110, RH6021, the change doc, and `sdd/work/`. `scripts/lane-check.sh behavior-change`: confirmed.

Evidence came from static tracing and from `scripts/format.sh` (twice, LF and CRLF), `scripts/apply-fix.sh RH5110`, and a worktree of e3924e2 for the `main` comparisons. Fixtures lived in the scratchpad and are gone.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/GenericConstraintLineBreakRewriter.cs`, `IsWrapped` and the separator test in `PlaceOneConstraintPerLine` | FC-002, FC-015, FC-019/FC-021 | – | PR-introduced | The rewriter decides "wrapped" and "already broken" with `TokenGapUtilities.HasLineBreakBetween`, which also counts a line break inside a multi-line `/* */` comment. The spec's qualifier is a line start ("its colon starts a line"). The counterparts decide on line starts too: `GenericConstraintContributor` (`IsFirstOnLine`/`SetIfFirstOnLine`) and the parameter-list layout (`LineBreakListRewriter.EnsureSeparatorsHaveEndOfLine`/`HasElementSplitSignal`, `HasLeadingEndOfLine`). See the members below. | Decide each member on line-start facts (`SyntaxTokenPositionUtilities.IsFirstOnLine`/`LineBreakTriviaUtilities.HasLeadingEndOfLine` of the colon, the first constraint, the commas, and the constraints' tokens), as the contributor and the parameter-list layout do. Then correct the `IsWrapped` summary and the class remarks: "a colon still starts a line only where a comment … keeps it" is false today. Add one test per member. The lead decides member (b) (see its row). |

Members of #1. Axes: which gap holds the multi-line comment (type parameter–colon, colon–first constraint, in front of a comma, behind a comma, inside a type argument list), and whether the comment is single-line or multi-line. Every single-line-comment cell holds and is tested. The multi-line cells, observed on 0260784 (all stable on a second pass, LF and CRLF identical) and on e3924e2:

| Member | Input | Head output | Verdict |
|---|---|---|---|
| (a) type parameter–colon | `where T /* a⏎            b */ : class, new()` | `where T /* a⏎            b */ : class,⏎                   new()` | **fails**. The colon does not start a line, no line break stands behind it, and the constraints are on one line, so FMT-BREAK-037 says "not wrapped … changes nothing". `main` leaves it unchanged. |
| (b) colon–first constraint | `where T : /* a⏎            b */ class, new()` | `where T : /* a⏎            b */ class,⏎                 new()` | **two readings.** "A line break stands behind its colon" reads either way for a line break inside a comment. Under the line-start reading, and under the parameter-list counterpart (`void N(int a, /* x⏎ y */ int b, int c)` stays on one line), it is not wrapped. The visible output differs, so the lead decides whether to ask. |
| (c) in front of a comma | `where T : class /* a⏎            b */, new()` | unchanged | Under the literal "constraints span more than one line" reading, the clause is wrapped and `new()` MUST start a line, which it does not. Under the line-start reading it holds. |
| (d) behind a comma | `where T : class, /* a⏎            b */ new(), System.IDisposable` | `… b */ new(),⏎                  System.IDisposable` | **fails under either reading.** The clause is split partway: `System.IDisposable` moves, `new()` stays behind the comment. The line-start reading gives one line, as for parameters. The literal reading needs `new()` to start a line. |
| (e) inside a type argument list | `IFoo</* a⏎            b */ T>` | clause wrapped, constraint aligned | holds: a constraint the comment keeps over two lines (FMT-BREAK-037, Q5). |

All these inputs reach `main`'s user through the CLI and through whole-document formatting. RH5110's fix reaches them when `where` shares the declaration's line. No test covers a multi-line comment in a clause gap (`08-test-implementer.md` dropped the colon case).

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | low | `GenericConstraintLineBreakRewriter.JoinUnstructuredWraps`, lines from `var replacements = new Dictionary…` to `ReplaceTokens` | FC-015 (mechanism) | – | new mechanism | The batch join loop copies `ListElementInteriorJoinRewriter.JoinInteriorWraps` almost line for line: the `currentPrevious`/`currentToken` replacement map, `CreateJoinedTrailingTrivia`, `RemoveLeadingEndOfLineAndWhitespace`, and `ReplaceTokens`. The join's policy is still shared, through the `LineBreakTriviaUtilities` helpers. The bookkeeping of chained joins now exists twice. | Move the replacement-map join over a list of `(previous, token)` gaps into `LineBreakTriviaUtilities`, and let both rewriters call it. Each rewriter keeps its own gap selection. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Join the colon onto the type parameter's line | `Name.GetLastToken()` trailing + colon leading: `AreSeparatedByEndOfLine` and not `WouldJoinAcrossUnjoinableTrivia` | `LineBreakTriviaUtilities.CollapseTokenToSameLine` (same two predicates) | agree | holds |
| Join a wrap inside a constraint | every token pair of one constraint after its first token, minus pairs whose behind-token has a `TypeArgumentListSyntax` ancestor (below the constraint) that contains the front token | FMT-BREAK-014 (`LineBreakListRewriter.CollapseAngleBracketListToSingleLine`, gaps between delimiters and elements) and FMT-BREAK-015 (`ListElementInteriorJoinRewriter.CanJoin`), which run earlier in the phase | agree. The gap in front of `<` is this rewriter's alone, and FMT-BREAK-014 rewrites only delimiter–element gaps. Probed: `System.⏎IDisposable` inside and outside `<>`, `<⏎T>`, `<T⏎>`, nested `IEquatable<⏎T>>`, `global::⏎`, `class⏎?`, `allows⏎ref struct`, `IEquatable⏎<T>`; all joined, stable | holds |
| Clause wrapped: colon member | `HasLineBreakBetween(Name.Last, Colon)`, which counts comment interiors | spec "colon starts a line"; `GenericConstraintContributor` `SetIfFirstOnLine(colon)` | disagree on a multi-line comment | #1 (a) |
| Clause wrapped: behind the colon | `HasLineBreakBetween(Colon, first constraint)` | contributor `IsFirstOnLine(firstConstraintToken)` picks the column | disagree on a multi-line comment | #1 (b) |
| Clause wrapped: constraints span lines | `HasLineBreakBetween` over every token pair from the first constraint on | parameter-list wrap signal `HasLeadingEndOfLine`/`LineBreakDetection.IsMultiLine` (`LineBreakListRewriter.HasElementSplitSignal`) | disagree on a multi-line comment | #1 (c), (d) |
| Next constraint already starts a line | `HasLineBreakBetween(separator, next)` or `HasLineBreakBetween(previous, separator)` | `EnsureSeparatorsHaveEndOfLine`: `HasTrailingEndOfLine(separator)` or `HasLeadingEndOfLine(next)`; contributor `AlignSeparators` (first on line) | disagree on a multi-line comment | #1 (d) |
| Constraint colon spaced | `token.Parent is TypeParameterConstraintClauseSyntax` (the clause's only `ColonToken`) | RH6021 `GetColonToken` (base list, constructor initializer) | deliberately different; recorded as an RH6021 non-goal (P11, follow-up 8) | holds |
| Lines the contributor places | `SetIfFirstOnLine` on the colon, each constraint's first token, separators via `ListElementAligner.AlignSeparators` | FMT-INDENT-032 "starts a line" | agree | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Line layout of a constraint clause | `rg -ln "TypeParameterConstraintClause\|ConstraintClauses\|WhereKeyword"` over Analyzer, CodeFixes, Core, Formatter: `GenericConstraintLineBreakRewriter` (layout). RH5110 analyzer (judges `where` only). RH5110 fix (delegates to `FormatNodeInDocumentAsync`). | one owner |
| Columns of constraint-clause lines | `GenericConstraintContributor` | one owner |
| Constraint-colon spacing | `SingleSpaceSpacingRule.IsSpacedColonToken` (`rg -n "Parent is BaseListSyntax"` outside tests: only this file); RH6021 is a separate surface by its non-goal | one owner |
| Join text and refusal | `LineBreakTriviaUtilities.CreateJoinedTrailingTrivia`, `RemoveLeadingEndOfLineAndWhitespace`, `WouldJoinAcrossUnjoinableTrivia` (shared) | shared; the batch bookkeeping is duplicated, see #2 |
| "Is a separated list wrapped / does the next element start a line" | `LineBreakListRewriter.HasElementSplitSignal`/`EnsureSeparatorsHaveEndOfLine` (line-start facts) and `GenericConstraintLineBreakRewriter.IsWrapped`/`PlaceOneConstraintPerLine` (gap analysis) | divergent predicates for the same kind of decision, see #1 |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ConstraintClauseLineBreakTests` (all) | FMT-BREAK-037 exact output | a different output text, a phase that changes it alone, a second pass that changes it, a CRLF difference | `AssertRuleResult` (LF and CRLF, second pass, `PhaseStabilityCheck`) | adequate |
| `ConstraintClauseAlignmentTests` (all) | FMT-INDENT-032 exact columns, also after FMT-INDENT-017 moves `where` left or right, and in lambdas and file-scoped namespaces | a column off | `AssertRuleResult` | adequate |
| `ConstraintClauseColonSpacingTests`, `HorizontalSpacingRequirementTests.OneSpaceBeforeBracesAndAroundDeclarationColons` | FMT-SPACE-009 for the constraint colon; comments in the colon gap keep their place (FMT-SPACE-013); a name colon nearby keeps its spacing | `where T:class` kept; a comment moved | `AssertRuleResult` | adequate |
| `RH5110…AnalyzerTests.VerifyFix*` (6) | RH5110-003: the fix result equals the formatter layout; the fix is offered where comments or directives block joins | a different fixed text, no fix offered, non-convergence, a Fix All difference | `Verify` of `BatchCodeFixTestsBase` (iterative and Fix All; two diagnostics in two of the tests; one CRLF test) | adequate |
| `RH5110…FormatterTests.VerifyFormatterFixesViolationWithWrappedConstraints` | the formatter output carries no RH5110 diagnostic (FC-013) | a diagnostic on the output | `VerifyFormatter` | adequate |
| (missing) multi-line `/* */` comment in a clause gap | #1 | – | – | missing; part of #1 |

## Visible output is decided

Every expected value the diff adds or changes traces to Q1–Q11 (`00-lead.md`, change doc `Decisions`) or to an unchanged requirement. `LocalFunctionInLambdaClauseAligns` follows the unchanged lambda layout (decision 7). The comment-line and blank-line cases follow FMT-BREAK-030 and FMT-BLANK. The comma-with-trailing-comment case follows FMT-BREAK-037 with Q4. No rule page changed. Member (b) of #1 is the one case where two readings give different output, and no test asserts it.

## Failure classes dismissed

- FC-001: the type-argument-list exemption in `JoinUnstructuredWraps` hands exactly the region that FMT-BREAK-014/015 own, and those rewriters run before it (`LineBreakPhase.CreateRewriters` order). Traced and probed (see *Guard scope*).
- FC-003: both sides are tested: one line and wrapped, comment and no comment, one constraint and several, a colon kept and a colon joined. The multi-line-comment side is #1.
- FC-004/FC-005/FC-020: no probe lost or moved a comment, a directive (`#if`, `#pragma`), or disabled text. Directive and disabled-text refusal goes through `ContainsUnjoinableTrivia` (`IsDirectiveOrDisabledTextTrivia`). Disabled text keeps its column, as everywhere.
- FC-006/FC-022: only whitespace and end-of-line trivia change, and the token sequence stays the same. A constraint clause holds no literal.
- FC-007/FC-008/FC-009: `scripts/apply-fix.sh RH5110` over seven clauses (comments, a directive, two clauses on one method, an expression-bodied method, a tight colon in a local function): fixed under LF and CRLF, with the iterations equal to the diagnostics. The batch tests cover Fix All. The offering condition is unchanged.
- FC-010/FC-011/FC-033: every probe was stable on a second `format.sh --check` pass, and its LF and CRLF results were identical. No loop-until-stable was added; the loops are bounded by the separator count.
- FC-012: the blank line before a comment line between two constraints is the blank-line phase's on `main` too (checked on e3924e2), for base lists as well. No phase undoes another.
- FC-013/FC-014: RH5110 judges only `where`. RH6002-001 exempts a comma that starts its line. RH6021 is narrowed by its non-goal. No new rule.
- FC-016: tests or probes cover every owner (class, struct, interface, record, record struct, method with and without a body, local function, delegate, extension block with `where` on its own line and on the declaration line, and an empty `partial` class turned into `;`) and every constraint kind.
- FC-017: see *Assertion adequacy*.
- FC-018/FC-036: no rename.
- FC-019/FC-021: single-line comments in every gap are tested. The multi-line `/* */` is #1.
- FC-023/FC-024: verbatim identifiers are tested. `partial` was probed. Other identifiers change only the width.
- FC-025: an expression-bodied method with a wrapped clause, through the RH5110 fix, gets the pre-existing block-body transform and an aligned clause.
- FC-026: several clauses per declaration are tested and probed (`M7`). Replacement maps are keyed by token identity, so identical texts do not collide.
- FC-027: lambda, local function, nested type, file-scoped namespace, and `where` moved left or right are tested.
- FC-028/FC-030: no caller formats a detached node holding a clause. `FormatNodeInDocumentAsync` is exercised by the RH5110 tests and `apply-fix.sh`. The fix formats the declaration, as RH5110-003 requires.
- FC-029/FC-037: not applicable.
- FC-031: summaries and remarks match the code, except the `IsWrapped` summary and the class remarks, which belong to #1. No `TODO` and no tracker number in the diff.
- FC-032: `09-implementer.md` ran every non-self-hosting suite after the last production change, and `10-cleaner.md` built Release with 0 warnings. `rg` finds no wrapped or tight constraint clause in repository source outside string literals, so the self-hosting formatter scan gains nothing to rewrite.
- FC-034/FC-035: the reproduction test uses the issue's shape. The expected values were written at 9a1da1d, before the implementation at da0e710.

## Architecture findings

_None._ The divergence behind #1 sits between a rewriter's guard and the counterparts it should agree with, and the fix is local. It is not two components owning one decision: FMT-BREAK-037 and FMT-BREAK-012 are separate requirements for separate constructs.

## Hints

- Pre-existing, unrelated to the change's mechanism: `where⏎    T : class, new()` puts `T` at the enclosing block column, one level left of `where` (the same on e3924e2). No requirement owns the gap between `where` and its type parameter. FMT-BREAK-037 joins only the type parameter–colon gap. It is the same kind of wrap that structures nothing as S2, so it is a follow-up candidate. Structural cause: a missing owner.
- Spec wording: FMT-BREAK-037's first step says "every line break inside one constraint … MUST be joined". A tuple wrapped inside a type argument list (`IEquatable<(int,⏎string)>`) keeps its wrap, as tuple layout and FMT-BREAK-014's "unless an element itself spans several lines" give, and the clause then wraps under Q5. The output is consistent. The sentence reads broader than the code and the requirements it cites.
- Pre-existing on e3924e2: `IFoo</* a⏎ b */ T>` loses the space between `*/` and `T` (horizontal spacing). Unrelated to this run.
