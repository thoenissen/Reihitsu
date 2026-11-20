Role: auditor
Commit: 8a333426dd61460dd10428a7d59b7a2401db6082
Verdict: pass

Prompt note: the prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the lane, the slug, the SHAs, and the handovers to read.

## Scope

Full audit of thoenissen/Reihitsu#892 on head `8a333426dd61460dd10428a7d59b7a2401db6082` against `origin/main` `9a89c85e2cdd66a0d55cdfd821c5a6fde6d5c5e2`.

State checks:
- The checkout is the head SHA, with a clean working tree.
- `git fetch origin main` gives `9a89c85`, and `git merge-base --is-ancestor` confirms it is merged.
- The diff (35 files) matches handovers 01 to 07. It holds two analyzers, a shared fix base with two thin providers, resources, the ruleset, sln and package README rows, two specs, two rule pages, the change doc, two reworded spec paragraphs, `AttachedPartGapUtilities` moved to `Reihitsu.Core` (rename at 91% similarity, visibility only), and two removed `using`s in the formatter.
- `scripts/lane-check.sh behavior-change`: confirmed.

Evidence I gathered (fixtures in my scratchpad, deleted afterwards):
- `scripts/apply-fix.sh RH5115` and `RH5116`, under LF and CRLF, on 22 shapes beyond the test suite:
  - `d["k"]⏎[0]`, `list⏎.First()⏎[0]`, `this.Arr⏎?[0]`, `(arr)⏎[0]`, `arr⏎[0] = 5;`, `n?⏎[0].ToString()⏎.Trim();`
  - a part inside a lambda argument, `new[] {…}⏎[0]`, `x?.B⏎[0]`, a wrapped single-link chain followed by `[0]`
  - a field initializer `[0..]`, an access inside a nested chain in a lambda argument of an outer wrapped chain, a conditional-expression branch
  - an invoked cast lambda block `…}))()⏎[0]`, `this⏎[1]`, `n⏎?[0]⏎?.ToString()`, `() => arr⏎[0]++`
  - `x⏎.B()⏎!`, `s?.Length⏎.ToString()⏎!`, `arr!.First()⏎!`, `s⏎![0]`
  - and the negatives `s⏎!⏎.Length`, `arr⏎!⏎.First()`.
- Every arm was `fixed` or `no-diagnostic`. None was `not-converged`, `invalid-result`, or `line-ending-drift`.
- `scripts/format.sh` on the same fixtures gives, for every reported part, the text the fix gives. The only difference is one blank line that the blank-line phase adds behind a multi-line statement, which no rule of this run decides.

## Findings

_N/A — no confirmed finding; see Hints._

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which token starts an index access (RH5115) | `[` whose parent is `BracketedArgumentList { Parent: ElementAccess }`. `?` of a `ConditionalAccess` (`OperatorToken`) whose next token is an element-binding `[`. Both through `AttachedPartGapUtilities.StartsAttachedPart` (`src/Reihitsu.Core/AttachedPartGapUtilities.cs`) | Same method in `AttachedPartLineStartRewriter.CanJoin` (via `IsAttachedPartGap`) and `PrecheckPhase.FindTriviaInAttachedPartGap` | Same code, one copy | holds |
| Which `!` starts a part (RH5116) | `!` of `SuppressNullableWarningExpression`, minus the first token of a link operator (`StartsLinkOperator` → `FluentChain.GetLinkOperatorTokens`) | Same method in the formatter's join and precheck | Same code. Probes `s⏎!⏎.Length` and `arr⏎!⏎.First()` gave no RH5116 diagnostic, and `s⏎![0]` gave one | holds |
| A line break separates the part (analyzer) | RH5115: the `[`/`?` start line is greater than the end line of `GetPreviousToken`, plus, for `?`, the `[` start line is greater than the `?` line. RH5116: the same comparison for `!` | Formatter `CanJoin`: `SyntaxTriviaUtilities.AreSeparatedByEndOfLine(previousToken, token)` | Equal on every gap without a comment, directive, or disabled text: those gaps hold only whitespace and end-of-line trivia. They differ only for a multi-line `/* */` that carries the line break without end-of-line trivia (`arr /* a⏎b */ [0]`). There the analyzer reports, as RH5115-001/RH5116-001 define it ("starts on a later line than the token in front of it ends on"), and the formatter refuses the input (FMT-UNSUP-014). SHARED-DIAG-010 has a layout rule report a refused shape | holds |
| Fix withheld for trivia in the gap (RH5115-006, RH5116-006) | `SpanContainsCommentOrDirective(root, gap)` over `[previousToken.Span.End, firstToken.SpanStart)` and, for `?`, `[?.Span.End, [.SpanStart)`, with `descendIntoTrivia: true` | Precheck: comment or directive or disabled text in `previousToken.TrailingTrivia` or `token.LeadingTrivia` for every `IsAttachedPartGap` token | The same two gaps, as the same trivia sets. Disabled text can only sit between directives of the same gap, so `IsCommentOrDirective` without a disabled-text check still covers it | holds |
| Join versus chain layout (RH5115-004/005, RH5116-004/005) | Part node (`ElementAccess`, `ConditionalAccess`, `PostfixUnary`), then `FluentChain.GetOutermostChainNode`, then `FluentChain.Create(...) != null` (requires a link) | SHARED-CHAIN-001 "attached part of a chain with at least one link". The part node is always on the spine: `ContinuesChain` is true for each of the three kinds as a child of a spine node | Agrees. The probes `Foo(arr⏎[0]).ToString()⏎.Trim()` (tests), `list.Where(v => v.Skip(1).ToArray()⏎[0] > 1)⏎.ToList()`, and `x?.B⏎[0]` take the expected branch | holds |
| Chain fix withheld for an unsupported shape (SHARED-FIX-010) | `FluentChainCodeFixHelper.IsFormattingRefused(chainNode)`, which checks the context node | `FormatChainAsync` formats that same context node | Same node (FC-043) | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which token starts an attached part, and its gaps | `src/Reihitsu.Core/AttachedPartGapUtilities.cs` is the only owner. Callers: `PrecheckPhase`, `AttachedPartLineStartRewriter`, `ListElementInteriorJoinRewriter`, both analyzers, and `AttachedPartLineBreakCodeFixProviderBase`. `rg "SuppressNullableWarningExpression\|ElementBindingExpression\|OpenBracketToken" src` also hits these: `FluentChain` (the chain model's attached-part list, a different decision; decision #2), RH5305/RH5306/RH5035/RH5038/RH5044/RH5419 (other brackets), RH3002, `EqualityComparerArgumentUtilities`, and `FluentChainAnalyzerBase` (registration only). None of them restates the gap. `rg "Utilities/AttachedPartGapUtilities\|Pipeline.Core.Utilities.AttachedPartGap"` finds no stale reference | one owner |
| A line break in the gap | RH5115 `StartsLaterLine`, RH5116 inline, formatter `AreSeparatedByEndOfLine` | Parity proven under *Guard scope*. The one divergence is on a shape the formatter refuses, and the specs define the analyzer side. Not a finding. See *Hints* 1 |
| Gap deletion (SHARED-WS-003) and its comment guard | `CollapseTokenGapCodeFixProviderBase` (RH5105, RH5106, RH5108) and `AttachedPartLineBreakCodeFixProviderBase.JoinAsync`/`GetGaps`. Both use the shared `SyntaxNodeUtilities.SpanContainsCommentOrDirective` | Two implementations of a three-line span deletion. The difference is justified: the new base deletes two gaps for `?[`, branches to the chain layout, and needs `DocumentOrderFixAllProvider`, and `CommentSafeSpanReplacementCodeFixProviderBase` has one replacement span and a fixed `BatchFixer`. The guard predicate is shared. Not a finding |
| Chain layout in a fix | `FluentChainCodeFixHelper.FormatChainAsync`/`IsFormattingRefused`, reused by RH5112, RH5201, and the new base. No copy | one owner |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5115/RH5116 `SpecExample…AreReported`, `OtherSideOfEachCondition…`, `BracketsThatStartNoIndexAccess…`, `IndexOnTheLineOfAWrappedNullForgivingOperator…`, `…WithCommentOnItsExpressionLineIsNotReported` | Exact diagnostic set and location (first token) | A missing, extra, or misplaced diagnostic | `Verify(source)`, exact markup | adequate |
| All `…ReportedWithoutFix` (comment, `#if`, `#if false`, `#pragma`, `#region`, `///` in three documentation modes, `?`→`[` gap, defined symbol `X`) | Reported, and no action registered for any diagnostic | Any registered action | `VerifyNoCodeFixIsOffered`, which runs the analyzer on the unmarked source and asserts empty actions per diagnostic | adequate |
| Join tests (`SpecExampleFix…`, `FixJoins…`, LF and CRLF) | Exact output text, nothing outside the gap changed (004 `c`, 004 `b` keep later columns) | Any other text change, or line-ending drift | `VerifyCodeFixWithEndOfLine`: verifier run plus an exact `Assert.AreEqual` on the applied fix, under both endings | adequate |
| Chain tests (`SpecExampleFixLaysOutTheChain…`, `FixLaysOutAChain…`) | Exact chain layout, equal to formatter output on an unchanged formatter (04 states the source of the expected values; FC-035 does not apply) | Misaligned link | `VerifyCodeFixWithEndOfLine` | adequate |
| `FixAllLaysOutOneChainWithTwoReportedAccesses`, `FixAllCorrectsAJoinedAccessInsideAChainItLaysOut`, base `FixAllCorrectsEveryDiagnostic` | One-at-a-time and Fix All both converge to the expected text in the verifier's checked iteration count | Overlap dropped (the implementer saw "Expected 1 iterations but found 2" with `BatchFixer`) | `Verify(source, fixed)` | adequate |
| `ChainFixIsWithheldWhenTheChainHoldsAnUnsupportedShape` / `JoinIsOfferedNextToAnUnsupportedShapeOutsideTheGap` | Both sides of SHARED-FIX-010 for the two branches | A fix offered on a refused context, or a join withheld by an unrelated shape | `VerifyNoCodeFixIsOffered` / `VerifyCodeFixWithEndOfLine` | adequate |
| `JoinInsideACollectionExpressionLeavesNothingForRH5305` | SHARED-FIX-004: the join result carries no RH5305 diagnostic | An RH5305 diagnostic on the result | Exact fix text plus `CSharpAnalyzerVerifierTest<RH5305…>` expecting none | adequate. LF only, but its subject is the rule interaction; the line-ending axis is covered by every other join test |
| `UnrelatedSyntaxErrorSuppressesTheWholeFile` / `SameAccessIsReportedWithoutTheUnrelatedSyntaxError` (both rules) | SHARED-JOIN-002, both sides | A report in an erroneous file | `Verify` with `CompilerDiagnostics.None` | adequate |
| Formatter parity (`VerifyFormatterJoins…`, `VerifyFormattedContextsStayUnchangedAndUnreported`, both rules) | Formatter output exact, second pass stable, no diagnostic on the output, under both endings | Formatter output flagged (FC-013), or unstable | `FormatterTestsBase<TAnalyzer>.VerifyFormatter` | adequate |

## Failure classes dismissed

- **FC-001, FC-002, FC-003.** Traced in *Guard scope*. Both sides of each qualifier are tested: one line versus a line break, `!.`/`!?.` versus `!?[`/`![`, `?`-gap versus `?`→`[` gap.
- **FC-004, FC-005, FC-019, FC-020, FC-021.** Every trivia kind in either gap withholds the fix (tests listed above). The guard reads directives as well as comments.
- **FC-006.** The join deletes only whitespace between the last token of a primary expression and `[`, `?`, or `!`. No such pair lexes into a different token (`!!`, `!?`, `>?`, and `)[` are not single C# tokens), and every `Verify(source, fixed)` compiles the result.
- **FC-007.** The answer 5A decides the follow-on RH5201/RH5203 diagnostics. RH5305 is tested. The chain branch is the existing RH5201/RH5112 layout.
- **FC-008.** Tested with overlapping chain fixes and with a join nested in a laid-out chain, using `DocumentOrderFixAllProvider`.
- **FC-009.** The join removes a non-empty gap. The chain fix formats a context that holds the gap, so the formatter's join (`CanJoin`: previous token inside the root) changes it. Hidden carriers and syntax errors are excluded by the base (SHARED-FIX-011, SHARED-FIX-012), and generated code by `GeneratedCodeAnalysisFlags.None`.
- **FC-010, FC-011, FC-012, FC-038, FC-039, FC-041, FC-042 (formatter side).** The formatter is unchanged apart from the namespace of the moved type. The implementer ran the formatter and core suites, 6405 and 747 passed.
- **FC-013.** Parity tests exist, and my probes matched `scripts/format.sh`.
- **FC-014.** FMT-BREAK-040 owns the join everywhere.
- **FC-015, FC-016.** One predicate. Sibling parts: the argument list's `(` is out of scope by answer 1A.
- **FC-017.** See *Assertion adequacy*.
- **FC-018, FC-036.** No rename.
- **FC-022, FC-027.** A raw string in front of the part, holes, lambdas, conditionals, field initializers, `this`/`base`, and nested chains are tested or probed.
- **FC-023, FC-024, FC-025, FC-029.** No identifier-, declaration-kind-, or key/value-dependent logic.
- **FC-026.** `grid⏎[0]⏎[1]` is tested.
- **FC-028.** The chain fix uses the existing `FormatChainAsync` (context entry point).
- **FC-030.** The chain fix formats the chain as SHARED-CHAIN-009 and answer 3A require. The join changes only the gap.
- **FC-031.** The summaries of all changed and new methods describe the new code. `AttachedPartGapUtilities`' summary names the new consumers. The formatter remarks remain true.
- **FC-032.** The cleaner's build and tests ran after the last compiled change. Commits after `6a29416` are handovers only.
- **FC-033, FC-034, FC-035, FC-037.** A new rule, with no reproduction or comment-only claim.
- **FC-040.** All three documentation modes are tested.
- **FC-042 (fix side).** The gap guard is not redundant on the join path, which bypasses the formatter. On the chain path, `IsFormattingRefused` would refuse the same input, but the guard is one shared check before the branch, not a second statement of a declared shape in a phase.
- **FC-043.** `IsFormattingRefused` and `FormatChainAsync` use the same context node.
- **FC-044.** No computed result is modelled.
- **Matrix "Tab width across formatter phases."** The run computes no column. The join deletes the gap wholesale, and the chain layout is the formatter's.

Visible output (check 10):
- The RH5115/RH5116 diagnostics are covered by answers 1A, 2A, and 4A with rule identity A.
- The join outputs are covered by 5A (SHARED-WS-003).
- The chain outputs are covered by 3A.
- The formatter-parity expected values follow from FMT-BREAK-040 and other formatter requirements the run does not change. Only the Why of FMT-BREAK-040 is reworded.
- The RH5115 page's example was shown in question 4.
- The RH5116 page follows from the two-rule answer and 5A. Its `!.`/`!?.` sentence describes an input whose output does not change from `main` (no RH5116 diagnostic), and decision #3 records it. See *Hints* 3.

## Architecture findings

_None._ The one shared decision (attached-part gap) has one owner after the move (decision #1).

## Hints

1. **Duplicated line comparison.** RH5115 has a private `StartsLaterLine` (`src/Reihitsu.Analyzer/Rules/Layout/RH5115IndexAccessMustStayOnExpressionLineAnalyzer.cs:49`), and RH5116 repeats the same expression inline (`src/Reihitsu.Analyzer/Rules/Layout/RH5116NullForgivingOperatorMustStayOnExpressionLineAnalyzer.cs:63`). Both are identical to the specs' wording, so I did not count it as a second owner. An earlier run's audit flagged a one-line check duplicated between two analyzers, so the lead may want them to share one helper.
2. **Untested boundary member.** No test pins the one input where the analyzer's line comparison and the formatter's end-of-line check differ: a multi-line block comment that carries the only line break of the gap, as in `var a = arr /* a⏎ b */ [0];`. Today it is reported without a fix, as RH5115-001/-002 state, and the formatter refuses the input. A switch to `AreSeparatedByEndOfLine` would silently stop reporting it. The shape is rare, and the current behavior matches the spec.
3. **RH5116 rule page not shown verbatim.** The maintainer did not see the page's example (`return value⏎    !;` → `return value!;`) or its sentence about `!.`/`!?.`. They follow from the two-rule answer, 5A, and an unchanged `main` output, and decision #3 records the sentence. The lead may list the page under *Review notes*.

## Follow-up candidates

_None._
