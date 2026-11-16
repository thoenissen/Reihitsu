Role: auditor
Commit: 41d705395bdaa3bb93722bc8df41bf17a495de0b
Verdict: findings

## Scope

Full audit. Head 41d705395bdaa3bb93722bc8df41bf17a495de0b (checked out); `origin/main` and `git ls-remote origin refs/heads/main` both eb89003591b8371d91e4a14b8590d764bdf6d605, an ancestor of head. The diff (`git diff origin/main`, without `sdd/work/`) is what 01–10 describe: FMT-BREAK-039 added, FMT-BREAK-015/020/031 clarified, code owners, change doc, `AGENTS.md` phase row, `ArgumentListLineStartRewriter` (new), `LineBreakPhase`, `ChainLineBreakRewriter`, `ListElementInteriorJoinRewriter`, and the tests of `05-test-implementer.md`. `scripts/lane-check.sh behavior-change`: confirmed. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Evidence: `scripts/build.sh` (Release, 0 warnings); `scripts/format.sh` over scratch fixtures with the branch's CLI and with `main`'s CLI (a worktree of eb89003 in the scratchpad), outputs diffed; second pass under LF and CRLF on every branch output: `Formatted 0 of 3 file(s)` both times.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `LineBreakPhase.CreateRewriters` (`ArgumentListLineStartRewriter` now runs before `LineBreakListRewriter`; `ChainLineBreakRewriter` no longer joins attached argument lists); `sdd/changes/2026-11/argument-list-below-name.md` (*What*, change 3's last sentence, *Decisions* "One requirement owns the gap…"); FMT-BREAK-039 second bullet and *Why* | FC-027, FC-016 | – | undecided output | An argument list (FMT-BREAK-012) whose only line break was an argument-list gap inside an element construct that FMT-BREAK-015 does not join — an interpolation hole, a query clause after the first, an array initializer, a `stackalloc` initializer — was laid out one argument per line on `main`; now it becomes one line. **Chain member (FMT-BREAK-020's join on `main`, so the change doc says this output does not change):** `Baz($"{x.ToString⏎()}", 2);` → `main`: `Baz($"{x.ToString()}",⏎    2);` (`2` aligned under `$`), branch: `Baz($"{x.ToString()}", 2);`. Same for `Baz(from v in list select v.ToString⏎(), 2);`, `Baz(new[] { x.GetHashCode⏎() }, 2);`, `Baz(stackalloc int[] { x.GetHashCode⏎() }.Length, 2);`, and `Baz(2, $"{x.ToString⏎()}");`. On `main` the chain join ran after the list was decided, so the change doc's "Their output does not change" / "No output differs from the three statements it replaces" and FMT-BREAK-039's "as it is after a join of FMT-BREAK-015 or FMT-BREAK-020 inside it" are false for these members. **Non-chain member (new join):** `Baz($"{Bar⏎(1)}", 2);` → `main`: `Baz($"{Bar⏎(1)}",⏎    2);` (`(1)` at the statement's column), branch: `Baz($"{Bar(1)}", 2);`; same for `Baz(from v in list select Bar⏎(v), 2);`. The join is answer 1A; the list collapsing to one line was never shown (change 3 only shows `Baz(Get⏎(1), 2)`, already one line on `main`). No test asserts any member. Members that hold (same output on `main` and branch): tuple element, lambda expression body, collection expression in an argument (FMT-BREAK-022, judged as written both times), conditional and binary expressions, property and indexer arrows, `[Obsolete]` on an expression-bodied property, `case … when`, element-access argument, anonymous-object/lambda-block/switch-expression arguments (multi-line regardless), attribute argument `nameof⏎(P)`. Stable on a second pass under LF and CRLF. The inputs are rare (a line break before `(` inside a hole, a query clause, or an initializer inside an argument) | Ask it as an output question (Before = `main` above, A = one line as now, B = one argument per line); record the answer in the change doc as a visible output change and correct the false "does not change" statements and FMT-BREAK-039's precedent sentence; add a test that locks the answered output for the chain member and the non-chain member (interpolation hole is enough as representative plus one of query/initializer) |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether the gap in front of an argument list's `(` is joined (`ArgumentListLineStartRewriter.CanJoin`) | The trivia between `token.GetPreviousToken()` and the `(`: `AreSeparatedByEndOfLine` + `WouldJoinAcrossUnjoinableTrivia`; previous token non-empty and inside `root.FullSpan` | `ChainLineBreakRewriter.CloseGap` (same gap for chain attached argument lists on `main`): `HasLineBreak` + `WouldJoinAcrossUnjoinableTrivia`; `ListElementInteriorJoinRewriter.CanJoin` (same gap inside list elements on `main`): same root-bound, empty-span, line-break and blocking-trivia checks | Same span and blocking set. `ListElementInteriorJoinRewriter`'s extra `SpansLines(previousToken)` has no reachable counterpart: a multi-line literal in front of an argument list's `(` is a semantic error, and the join would leave the literal's lines alone anyway. Behind a node-level root inside a chain that is not the outermost node, `main`'s chain rewriter did not join and the new rewriter does — document and node formatting now agree (FC-028 improved), within change 6 | holds |
| Which `(` is an argument list's (`IsArgumentListOpenParenthesis`) | `token.Parent is ArgumentListSyntax` with parent invocation, object creation, implicit object creation, constructor initializer, primary-constructor base type, `with` element | Roslyn's owners of `ArgumentListSyntax` (`BaseObjectCreationExpressionSyntax`, `ConstructorInitializerSyntax`, `ImplicitObjectCreationExpressionSyntax`, `InvocationExpressionSyntax`, `ObjectCreationExpressionSyntax`, `PrimaryConstructorBaseTypeSyntax`, plus `WithElementSyntax`); `FluentChain`'s `ArgumentList` attached part (always an invocation's) | Complete; bracketed and attribute argument lists excluded as FMT-BREAK-039 states | holds |
| `ListElementInteriorJoinRewriter.IsOwnedGap` defers the gap | Calls the same `IsArgumentListOpenParenthesis` | The new rewriter's own selector | One predicate, one owner | holds |
| `ChainLineBreakRewriter.NormalizeChain` skips `FluentChainAttachedPartKind.ArgumentList` | Skips `CloseGap` and `CloseInnerGaps` for that part | `FluentChain.Collect` builds that part from the single `OpenParenToken`, so `CloseInnerGaps` was a no-op; `IsWrapped`/`GetWrappedLayoutLineBreaks` read link operators only | No chain layout decision read the skipped gap | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Join of the gap in front of an argument list's `(` | `rg "ArgumentList" src/Reihitsu.Formatter/Pipeline/LineBreaks` (excluding bracketed/attribute/type-argument lists): `ArgumentListLineStartRewriter` (owner); `ChainLineBreakRewriter.NormalizeChain:341` and `ListElementInteriorJoinRewriter.IsOwnedGap:173` defer; `LineBreakListRewriter.VisitArgumentList`/`EnsureArgumentsOnSeparateLines` decide separators and arguments, not this gap. No code fix edits that gap (`rg OpenParenToken src/Reihitsu.Analyzer.CodeFixes`: RH3001/3002/3006/5109/5203, other constructs) | one owner |
| Blocking trivia for a join | `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` → `SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` (shared, reused) | one owner |
| Join separator | `LineBreakTriviaUtilities.JoinGaps`/`CreateJoinedTrailingTrivia`/`CreateJoinSeparator` (shared, reused) | one owner |
| Statement of the join in the specs | FMT-BREAK-039 owns it; FMT-BREAK-020 and FMT-BREAK-015 point to it; FMT-BREAK-031 lists it | one statement (its precedent sentence is wrong, finding 1) |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ArgumentListLineStartTests` exact-output tests (spec examples 1–4, owners, nesting, blank lines, chains, holes, arms, lambdas, comments outside the gap, doc-comment modes) | Exact output, second pass, per-phase stability, LF and CRLF | Any byte of the joined or kept layout differing, or a second pass moving it | `AssertRuleResult` (± parse options) | holds |
| `ArgumentListLineStartTests.CommentInTheGapKeepsTheLineBreakForEveryOwner` | `(1)`…`(4)` each start a line, no blank line above, stable | `(` joined onto the commented line | `AssertBlankLinesAbove` + `AssertSecondPassIsStable` | holds: exact columns are undecided (F2), so a weaker helper is right; comment survival for these owners rests on the shared blocking predicate the exact-output example 4 pins |
| `TypeofOperandBelowTheKeywordKeepsItsLineBreak`, `StatementStartingWithParenthesizedExpressionIsNotJoined` | A `(` that is not an argument list's is not joined (other side of the boundary, FC-003) | The join reaching `typeof` or a statement's `(` | `AssertRuleResult` (unchanged) | holds |
| `JoinSeparatorPhaseTests.ArgumentListJoinsWriteNoWhitespace` | The line-break phase alone joins four owners with no whitespace, tokens unchanged, LF and CRLF | A space or a kept line break at `base(1)`, `B(2)`, `Foo(3)`, `new(4)` | `ExecuteLineBreakPhase` + `Contains` + `AssertSameTokens` | holds |
| `ReihitsuFormatterNodeColumnTests.StatementTargetJoins…`, `CollectionExpressionTargetJoins…`; `ReihitsuFormatterDetachedRootTests.FormatNodeOnStatementJoins…` | Node-level output equals document output (FC-028) | Target output differing from `AssertRuleResult`'s | `AssertFormatsTarget` + `AssertRuleResult`; `AssertFormatNode` | holds |
| `ReihitsuFormatterNodeColumnTests.ArgumentListTargetBelowItsNameKeepsItsOpeningParenthesisInPlace`; RH5101/RH5102 `VerifyFixKeepsOpeningParenthesisBelowTheInvokedName` | The target's first `(` keeps line and column (FMT-PIPE-017) | `(` joined or moved | `AssertFormatsTarget`; `VerifyCodeFixWithEndOfLine` (LF, CRLF) | holds |
| RH5305/RH5303/RH5301/RH5302/RH5308 `VerifyFixJoins…` (LF, CRLF), RH5305 two-collection Fix All | Fix output carries the join; iterative and Fix All | Fix output keeping `(` below its name, or a second iteration changing it | `VerifyCodeFixWithEndOfLine`, `Verify` | holds |
| `ChainLinkLayoutTests.ArgumentListWithoutMemberAccessJoinsItsName` (changed expected value) | `var r = Foo(1);` | Line break kept | `AssertRuleResult` | holds; covered by answer 1A |
| Missing | The list layout of finding 1 | — | — | missing (finding 1) |

Visible output (check 10): every added or changed expected value is the join of answers 1A–4A or `main`'s output (FC-035 derivation per `05-test-implementer.md`); none locks finding 1's list layout.

## Failure classes dismissed

- FC-001, FC-002 — the guard is the same span and predicate as its two predecessors (Guard scope).
- FC-005 — `WouldJoinAcrossUnjoinableTrivia` covers comments, directives, and disabled text alike; tests pin `#if`, `#pragma`, `#region`, `#if false`.
- FC-006 — the join removes only whitespace and line breaks; tokens unchanged (`AssertSameTokens`), generic `<…>(` disambiguation is token-based and unaffected by whitespace.
- FC-007, FC-008, FC-009, FC-030 — code fixes unchanged; their outputs change through the formatter only (change 6); iterative and Fix All tests green per 05/06.
- FC-011 — single pass over `DescendantTokens`, no loop.
- FC-012 — no later phase re-breaks an argument list's `(`.
- FC-013, FC-014 — no rule reports the shape (RH5105 parameter lists only); the joined output is reported by nothing.
- FC-018, FC-023, FC-024, FC-025 (covered by field/arrow test), FC-029, FC-036, FC-044 — cannot reach a whitespace join or a node-kind predicate.
- FC-022 — interpolation hole tested; raw strings: the gap cannot sit inside the literal's text.
- FC-026 — replacements keyed by token identity in `JoinGaps`, and `make()⏎(2)⏎(3)` tested.
- FC-038, FC-039, FC-041, FC-042, FC-043 — no shape declared; the join carries no comment into an FMT-UNSUP-004/008/009 span (the gap in front of `(` is never between an element and its comma, nor between `?`/`:` and a branch's first token).
- FC-031 — summaries of `LineBreakPhase`, `ChainLineBreakRewriter`, `ListElementInteriorJoinRewriter`, and the new rewriter describe the new code; the false precedent claim is in the spec and change doc (finding 1), not in code comments.
- FC-032, FC-033, FC-034, FC-035, FC-037 — tests derived from `main` output (05), not from patched code; no text-only claim.
- FC-010, FC-017 — second pass under LF and CRLF in every helper and in my fixtures.

## Architecture findings

_None._ The one-owner structure of decision 3 holds in code. Finding 1 is a consequence of moving the chain join ahead of the list decision, not two components owning one decision.

## Hints

- Pre-existing, identical on `main` and branch, outside the run: a query whose clauses are written on lines of their own gets its continuation clauses at the statement's column (`var q = from v in list⏎        where v.Equals(1)⏎        select v;`); and `a switch { true => x.GetHashCode(), _ => 2 }` inside an argument keeps both arms on one line inside the braces. Not verified against a spec.

## Follow-up candidates

_None_ beyond F1–F3 and F5 in `00-lead.md`.
