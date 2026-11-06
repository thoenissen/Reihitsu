Role: auditor
Commit: 2284decb
Verdict: findings — #1257: findings (#6); #1279: findings (#6, #7, #8, #9); #1281: findings (#5, #9); #1282: pass; #1417: pass; #1422: pass

My prompt named the run, the SHAs, and the handovers to read. It carried no intended fix, suspected cause, or hoped-for verdict, so I had nothing to set aside.

## Scope

Head `2284decb52d1490dfcb44d3ca56051cf061d8591` (checked out, clean). Base `origin/main` = `0fa1778c875793aaba22d10fef14747d4988f2ed`, equal to `git ls-remote origin refs/heads/main`, and an ancestor of head. The repair diff `af76b48e..2284decb` matches handovers 09–13:
- specs RH3004, RH3005, RH3101, RH3105, RH3106, and SHARED-FIX-002;
- the new `DocumentOrderFixAllProvider`, wired into RH3005, RH3101, RH3105, and the empty-list base (RH3106, RH3107);
- the RH3004 moved-code path (`MoveStatementsAsync` and its helpers);
- the RH3106 analyzer (`TakesNaturalType`, `GetPositionExpression`, `HasDelegateTypedNeighbor`) and the RH3106 fix (operators in `KeepsMeaningAsync`);
- the bounded `RewriteParseUtilities.ParsesAsWritten`;
- tests in five classes.

**The scope grew.** The repair changed specs: SHARED-FIX-002, RH3004-003/-006, RH3005-008, RH3101-005, RH3105-005, RH3106-002/-003. So I audited every changed requirement in full against the code, and every source file the repair touched in full. For the files the repair left byte-identical, I reused the evidence of `08-auditor.md`: the RH3001, RH3103, RH3104, RH3107, RH3201, and RH5417 analyzers and fixes, RH3101-007, and the shared comment guards.

I trusted the cleaner's build and the roles' focused test results. Method:
- static tracing of every changed guard against its counterpart, then constructed inputs;
- a scratch file-based app outside the repository that references `src/Reihitsu.Tooling`, run against three trees: this head, `af76b48e`, and `main` (temporary worktrees, removed afterwards). For each diagnostic it reports whether a fix is registered, applies the single fix or the provider's own `GetFixAllProvider()`, compiles the result, and re-analyzes it;
- one value check of a raw string literal, compiled and run.

Every input below compiles without errors. The errors named are those of the fixed output.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | RH3005, RH3101, RH3105, and RH3106 use `DocumentOrderFixAllProvider`. The four interacting-instance tests assert the audit's inputs exactly, plus one holding test each. Re-probed `M(Max > G, (7) < B)`: the second instance stays reported and the output compiles. The audit had listed RH3004 under Fix All as holding, and RH3004 fails on nested instances: see #6, which repeats #1. |
| 2 | closed | Every member #2 listed is now not reported or withheld, as the new tests assert: `var a = (delegate () { });`, `new[] { … }`, an inferred lambda return as an expression body or a `return`, and both operand sides of competing user-defined operators. Members #2 did not name fail: see #5, which repeats #2. |
| 3 | closed | Every member #3 listed holds under exact-text tests (`FixLeavesStatementsTheFormatterWouldRewriteAsWritten`, `FixMovesAMultiLineDocumentationCommentWithTheStatements`, `FixKeepsTheLayoutOfTheStatementsItMoves` LF and CRLF). The `///` restore is removed. A member #3 did not name fails in the new line-moving code: see #7, which repeats #3. |
| 4 | closed | `ParsesAsWritten` re-parses only the smallest changed node, incrementally, and compares the innermost member declaration. The implementer measured 0.6 ms per diagnostic (the audit measured 7.4 ms). The static trace agrees: the cost is bounded by the member around the change. |

## thoenissen/Reihitsu#1257

### Findings

#6, RH3005 member (under *Whole run*).

### Checked and holding

- RH3005, RH3101, and RH3105 Fix All on the interacting inputs of SHARED-FIX-002's new example: the instance whose fix would complete the generic reading stays reported, and the others are fixed. The `> D` / `> 7` holding variants are fixed completely.
- RH3105 nested `this.F(this.A + this.F(this.B))` and RH3101 by the same trace: every qualifier is removed in one application.
- `ParsesAsWritten`, bounded:
  - The change is confined to the smallest node that differs. That node lies inside the innermost member declaration the comparison uses.
  - The reparsed member's full span is checked against the expected span, so a re-lex that crosses the changed node's edge is still caught, for example `int/**/x` → `intx`: the walk stops at the `PredefinedType`, the member compares `intx` against `int` `x`, and returns `false`.
  - A change above every member (several members, or the compilation unit itself) compares the whole root.
  - The 14 existing Core tests run through the new path.

## thoenissen/Reihitsu#1279

### Findings

#7 and #8 (below), #6 (RH3004 member), and #9 (b).

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 7 | high | `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs` `MoveStatementsAsync` (line loop: when `textStart` reaches the line's end, `copyStart = textStart` drops the line's whitespace) together with `CanMoveLine` | FC-022, FC-015, FC-006 | #3 | PR-introduced (repair) | The fix changes the value of a multi-line raw string literal in the block it moves. A whitespace-only content line that is wider than the closing quotes' indentation carries its extra whitespace in the literal's value, and the fix writes the line empty. Input: a block holding `var s = """⏎    a⏎          ⏎    b⏎    """;` (closing quotes at column 16, middle line 22 spaces). The value is `a\n......\nb` (checked by compiling and running), and after the fix it is `a\n\nb`. At `af76b48e` and on `main` the line was kept. The formatter's counterpart, `RawStringLineShifts.CollectEdits`, skips only zero-length lines and moves whitespace-only lines with the literal. The fix holds a private copy of that rule, and the copy diverges (FC-015). Failing members: a whitespace-only raw-string line wider than the closing indentation, under a move to the right (reproduced) or to the left (traced: same branch), and the same line in an interpolated raw string `$"""` (traced: same branch). Holding: whitespace-only raw lines at most as wide as the closing indentation (empty value either way); raw content lines and the closing line; a move to the left wider than the closing indentation (all lines kept); verbatim and interpolated verbatim continuation lines (`CanMoveLine` false, the line is copied whole); tabs in the raw-string prefix (every line's prefix goes through the same `ShiftIndentation`). | Move a whitespace-only line inside a raw string literal like any other raw content line, never trimming it, or leave it as written. Better, keep one owner of "how a raw string's lines move with their code" for the formatter and the fix (see *Architecture findings*). Add a test: an RH3004 block with a raw string holding a whitespace-only line wider than the closing quotes' indentation. |
| 8 | medium | same file, `MoveStatementsAsync` / `ShiftIndentation` (a move to the left is clamped at column 0) | FC-027 | – | PR-introduced (repair) | When the block's first statement shares the opening brace's line and later lines are indented less than the leftward delta, those lines lose their indentation relative to the first statement and fall to a column left of the braces. RH3004-006 requires the other lines to keep their indentation relative to the first statement line, which cannot hold here, and the spec does not decide this case. Input: `Run(delegate () { Foo();⏎            Foo();⏎        });` → `Run(() =>⏎            {⏎                Foo();⏎  Foo();⏎            });`, with the second `Foo();` at column 2. Same shape with an assignment anchor: `Action<int> c = delegate (int x) { Foo(x);⏎            Foo(x + 1); };` puts `Foo(x + 1);` at column 0. `main` kept the block verbatim. The output compiles. Holding: a block whose first statement starts on its own line, which is the common case (every delta is then positive or keeps the relative layout); a one-line block (answer 6); later lines aligned under the first statement. | Spec author decides how lines after a shared brace line are measured, for example their own delta to one level inside the braces, or a floor at the content column. The lead decides it on recommendation (standing instruction). Then a test for the shape. |

### Checked and holding

- RH3004-006 examples `b` and `c` (answer 6 A), LF and CRLF. One-line blocks with several statements go through the general path, so no special case was added.
- Directives (answer 7, rows 24 and 25):
  - `#if`/`#else`/`#endif` and `#nullable` in column 0 stay in column 0.
  - `#region`/`#endregion` in column 0, and directives in other columns, move with the statements.
  - The delta is measured on the first statement line, active or disabled, so the `#region` example gives the same output with and without `DEBUG`.
  - A comment line in front of the first statement keeps its offset. A block that holds only a comment is measured on its first line.
- `GetOpenBraceTail`: a `// c` behind `{` stays behind the new `{`. A multi-line `/* */` behind `{` moves with the content.
- Verbatim and interpolated verbatim strings keep their continuation lines. A line inside an interpolation hole moves (its whitespace is trivia).
- Answer 6's condition holds: a one-line block needs no dedicated path.

## thoenissen/Reihitsu#1281

### Findings

#5 (below) and #9 (a).

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 5 | high | `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3106UnnecessaryDelegateParenthesesShouldBeRemovedCodeFixProvider.cs` `KeepsMeaningAsync`: it forks only when an `ArgumentSyntax` or `BinaryExpressionSyntax` encloses the node. `src/Reihitsu.Analyzer/Rules/Clarity/RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer.cs` `TakesNaturalType`: its lambda cases ignore an explicit return type and cover lambdas only. | FC-001, FC-002, FC-016, FC-006, FC-027 | #2 | same requirement (RH3106-001/-002/-003, SHARED-FIX-003); member (f) PR-introduced (repair) | The decision is still made by position. The repair added `BinaryExpressionSyntax` to the fork's gate, where #2 suggested gating on the outcome. So every other place where overload resolution, operator resolution, or the natural type decides is fixed and broken. Single fix, failing members: (a) `c += delegate () { };` where `C` declares `operator +(C, Action)` and `operator +(C, Action<int>)` → `c += delegate { };`, CS9342; (b) the same with C# 14 instance `operator +=(Action)` / `operator +=(Action<int>)` → CS0121; (c) a collection initializer `new Coll { delegate () { } }` with `Add(Action)` / `Add(Action<int>)` → CS0121; (d) a collection expression `Coll d = [delegate () { }];` over the same type → CS8917; (e) an anonymous method returned by an anonymous method under `var`: `var k = delegate () { return delegate () { }; };` and `var n = delegate (int x) { if (x > 0) return delegate () { }; return null; };` → CS8917, because `TakesNaturalType` recognizes only a `LambdaExpressionSyntax` around the `return`; (f) a missed report: `var f = Action () => delegate () { };` and `var g = Action () => { return delegate () { }; };` are no longer reported. The lambda's return type is explicit, so the anonymous method's target is `Action`. RH3106-003 exempts only a lambda "whose return type is inferred", a qualifier the predicate never checks (FC-002). At `af76b48e` both were reported and fixed to code that compiles. Traced on the same branch: `object o = Action () => delegate () { };`. Holding: `c + delegate () { }` and `delegate () { } + c` with two operators (withheld) and with one (fixed); a chain `c + d1 + d2`; `(…)` under `var`; `new[] { … }`; inferred lambda returns; `var x = b ? delegate () { } : someAction;`, the switch arm and the array element with a delegate-typed neighbour, under `var` and under `object` (fixed, compiles); `E += delegate () { };` on an event (fixed, compiles); nested anonymous methods; the Fix All arguments test. | Fix: decide on the outcome, not the position. Fork for every reported list, and withhold when the anonymous method's converted type changes or the enclosing statement or member gains a semantic error after the removal. Analyzer: apply the lambda cases only when the lambda declares no return type (`ParenthesizedLambdaExpressionSyntax.ReturnType == null`). Spec author: say whether an anonymous method with a parameter list that returns one counts like a lambda under RH3106-003 (if not, it stays reported with the fix withheld). Name compound assignment and collection initializer or collection-expression elements in RH3106-002. Tests for (a)–(f). |

### Checked and holding

- `HasDelegateTypedNeighbor` agrees with RH3106-003's neighbour sentence. It covers the other branch, the other arm, and the other element (anonymous functions excluded, parentheses unwrapped), nested conditionals in parentheses, and a lambda return through a conditional with a delegate-typed branch. A `null` or lambda neighbour gives no type, so the result is not reported, which matches CS0173 without the list.
- `KeepsMeaningAsync` for an operator that binds to no method (`??`): the binding stays unbound and the converted type is unchanged.

## thoenissen/Reihitsu#1282

Nothing to audit. The repair did not touch the comment guards. The wiring of the empty-list base to `DocumentOrderFixAllProvider` leaves the guard of SHARED-PARENS-003 as `08-auditor.md` verified it. RH3107 instances do not interact, and its tests pass unchanged.

## thoenissen/Reihitsu#1417

Nothing to audit. The repair did not touch RH3103.

## thoenissen/Reihitsu#1422

Nothing to audit. The repair did not touch RH3104 or RH5417.

## Whole run

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 6 | high | `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs` `GetFixAllProvider` (`BatchFixer`). `src/Reihitsu.Analyzer.CodeFixes/Core/DocumentOrderFixAllProvider.cs` `MapSpan` (a later span that a change overlaps becomes `null` and is skipped). | FC-008, FC-027 | #1 | RH3004 member: PR-introduced; RH3005 member: new mechanism (same result on `main`) | SHARED-FIX-002 requires Fix All to correct every diagnostic whose fix is offered on the starting code, with the result of applying the single fixes one after another. The clarified sentence exempts only a fix that a guard withholds after the earlier fixes. Nested instances whose spans the outer fix's text change covers are left reported, and their fix is still offered on the result. (a) RH3004, nested anonymous methods (`Action<int> a = delegate (int x) { Action<int> b = delegate (int y) { … }; b(x); };`, multi-line): Fix All converts only the outer one, and the inner stays reported with its fix offered. `main` converts both in one application. At `af76b48e` it already failed, because the outer fix rewrites the whole block, so BatchFixer drops the conflicting inner change. #1 listed RH3004 as holding. (b) RH3005, `b = 1 < (1 < x ? 2 : 3);`: Fix All writes `(1 < x ? 2 : 3) > 1`, the inner comparison stays reported, and its fix is offered (`(x > 1 ? 2 : 3) > 1`). The new provider drops the overlapped span by design, and its summary says so. The result is the same as `BatchFixer` on `main`. Whether a nested instance survives depends on the text diff Roslyn computes for the outer change: `b = 2 == (3 > x ? 4 : 5);` holds. Holding: RH3106 nested anonymous methods (both lists removed); RH3105 nested qualifiers (all removed); every interacting-instance test of #1. | Track each later diagnostic through the earlier fixes instead of mapping its span by offset: re-locate it on the changed document, for example by annotating its node before the first fix, or by re-running the analyzer and matching diagnostics in order. Then give RH3004 the document-order provider: the outer fix moves the inner block verbatim, so a tracked inner diagnostic stays fixable. Add nested Fix All tests for RH3004 and RH3005. |
| 9 | medium | `src/Reihitsu.Analyzer.Test/Clarity/RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests.cs`; `src/Reihitsu.Analyzer.Test/Clarity/RH3004UseLambdaSyntaxAnalyzerTests.cs` | FC-003, FC-017 | – | same requirement | Two changed requirements are tested on one side only, as 13-spec-checker observed. (a) RH3106-003 reports a delegate-typed neighbour "in every position". Only `var` is tested. `object o = b ? delegate () { } : someAction;` is reported, and the fix compiles (probed), but no test pins it. (b) RH3004-006 says the disabled-before-active example gives the same output with and without `DEBUG`. Only the variant without `DEBUG` is tested. | Add both tests. |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Fix All decides each guard on the code the earlier fixes produced (`DocumentOrderFixAllProvider`) | the diagnostic's span mapped by offset through earlier changes; `null` when a change overlaps it | SHARED-FIX-002: sequential application in document order | agrees, except for overlapped spans | fails for nested instances (#6) |
| RH3004 Fix All | `BatchFixer` merge of per-instance changes on the starting document | SHARED-FIX-002 | the outer change overlaps the inner one | fails (#6) |
| RH3106 fix: does the binding depend on the list (`KeepsMeaningAsync`) | ancestors up to the statement; forks only under an argument or a binary expression | RH3106-002 and SHARED-FIX-003: any position where resolution or the natural type decides | narrower: misses compound assignment, collection initializers and expressions, and anonymous-method returns | fails (#5) |
| RH3106 analyzer, natural type (`TakesNaturalType`) | position expression; `var`, `new[]`, lambda expression body, or `return` in a lambda (recursive); cast; converted type | RH3106-003 bullets, including "return type is inferred" | the qualifier is not checked; anonymous-method returns are not covered | fails (#5 e, f) |
| RH3106 delegate-typed neighbour | other branch, arm, or element; parentheses unwrapped; anonymous functions excluded | RH3106-003 neighbour sentence | identical | holds |
| RH3004 measured line (`GetFirstStatementLinePosition`) | lines from the first content line to the line of `Statements[0]`; disabled lines with non-comment text | RH3004-006 (rows 22, 25) | identical | holds |
| RH3004 lines kept in column 0 (`IsColumnZeroDirectiveLine`) | trivia at the line start is a directive, other than `#region`/`#endregion` | RH3004-006 (answer 7, row 24) | identical; the formatter's `IsNonRegionDirectiveTrivia` counts only active directives (follow-up candidate already recorded) | holds |
| RH3004 lines that keep their columns (`CanMoveLine` and the whitespace-only branch) | the token at the line start inside a literal; raw strings move with the closing marker | formatter `RawStringLineShifts.CollectEdits`: moves every non-empty raw line | the fix trims whitespace-only raw lines | fails (#7) |
| RH3004 column of moved lines (`ShiftIndentation`) | one delta for every line, clamped at column 0 | RH3004-006: indentation relative to the first statement line | cannot hold for a shared brace line followed by shallower lines | fails (#8) |
| `ParsesAsWritten`, bounded | the smallest changed node is re-parsed; the innermost member around it is compared (span and tokens) | the whole-tree comparison of `af76b48e` | equivalent: the change and every re-lex it causes stay inside that member, which the span check enforces | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Sequential Fix All for guarded fixes | `DocumentOrderFixAllProvider` (RH3005, RH3101, RH3105, and the empty-list base for RH3106 and RH3107). `rg WellKnownFixAllProviders.BatchFixer src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity` still lists RH3004 and other non-interacting fixes. RH3002 and RH7101 keep their own batch rewrites (unchanged, outside the run). | one owner; RH3004 should join it (#6) |
| How the lines of a multi-line raw string move with their code | `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs`; `RH3004UseLambdaSyntaxCodeFixProvider.CanMoveLine`/`ShiftIndentation` (`rg -l MultiLineRawStringLiteralToken`). RH5202 reads the same token kind for its own rule. | two owners, and they diverge (#7) |
| Which directive lines belong in column 0 | `IndentationRewriter.IsNonRegionDirectiveTrivia` (formatter, final column); `RH3004…IsColumnZeroDirectiveLine` (which moved lines keep their column) | different decisions over the same directive set; follow-up candidate already recorded |
| Where an anonymous method takes its natural type | RH3106 analyzer only (`GetPositionExpression`, `HasDelegateTypedNeighbor`); the fix restates "where the binding depends" as a position gate | a second, narrower statement in the fix (#5) |
| A rewrite still parses as written | `RewriteParseUtilities.ParsesAsWritten` (`rg IsIncrementallyIdenticalTo\|WithChangedText` finds no other owner); RH3002's private scan (follow-up candidate) | unchanged from `08-auditor.md` |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH3005, RH3101, RH3105 `FixAllLeaves…Reported` and `FixAll…StillReadAsComparisons` | Fix All decides each guard on the evolving code | the completing instance fixed, or a safe one left | `Verify` (single, iterative, Fix All), exact text, `MarkupMode.Allow` on the fixed states | adequate; no nested instance (#6) |
| RH3106 Fix All, the operator tests, and the natural-type tests | report or withhold per position | a natural-type position reported, or a binding-changing fix offered | `Verify`, `VerifyNoCodeFixIsOffered` | exact, but compound assignment, collection initializers and expressions, anonymous-method returns, and explicit-return lambdas are missing (#5); delegate-typed neighbour only under `var` (#9 a) |
| RH3004 moved-code tests (`FixKeepsTheLayout…` LF and CRLF, `FixMovesTheStatementsOfAOneLineBlockOntoOneLine`, `FixLeavesStatementsTheFormatterWouldRewriteAsWritten`, `FixMovesAMultiLineDocumentationComment…`) | statements kept; one delta; braces at the anchor | a reformatted statement, or a different column | `Verify`, `ApplyCodeFixAsync` (CRLF), exact text | adequate for what they cover; no raw string (#7), no shared brace line with later lines (#8) |
| RH3004 directive and measurement tests (eight) | column-0 directives kept; delta measured on the first statement line | a directive moved, or the wrong delta | `Verify` with and without `DefineDebug` | adequate; the disabled-before-active example is untested with `DEBUG` (#9 b) |
| `RewriteParseUtilitiesTests` (14, unchanged) | as in `08-auditor.md` | as before | `Assert` | adequate. The bounded path is exercised by every test. A change in a top-level statement and the span-mismatch exit have no test of their own (Hint 1) |

### Failure classes dismissed

- FC-004/FC-005/FC-019/FC-020/FC-021: the RH3004 move copies comments, directives, and disabled text as text (only indentation changes, as RH3004-006 states). A comment behind `{` stays behind it. A directive at column 0 and a directive in another column both follow the requirement. No other changed guard drops trivia.
- FC-007/FC-009: every registered action changes the document; each single fix silences its own diagnostic (re-analysis after each probe).
- FC-010–FC-014: no formatter phase changed.
- FC-018/FC-036: no rename.
- FC-023/FC-024/FC-025: identifiers and member kinds do not reach the changed code differently. RH3104 is unchanged.
- FC-026: the document-order provider resolves each diagnostic by its own mapped span; where the text under a span is unchanged, the token is the same one. Overlap is #6.
- FC-028: no detached-node formatting.
- FC-029: no key/value layout.
- FC-031: every changed summary describes the new code: `DocumentOrderFixAllProvider`, `MoveStatementsAsync`, `GetFirstStatementLinePosition`, `IsColumnZeroDirectiveLine`, `CanMoveLine`, `TakesNaturalType`, `HasDelegateTypedNeighbor`, `KeepsMeaningAsync`, `ParsesAsWritten`. The removed `RestoreDocumentationCommentsAsync` left no reference. No `TODO`, no tracker number.
- FC-030: closed with #3. The fix formats the lambda, then replaces the block's interior with the original text.
- FC-032: every probe ran on a fresh build of the tree it names.
- FC-033: the CRLF variants exist for the RH3004 layout. The interacting Fix All inputs are single-line. #7 does not depend on the line ending.
- FC-034: the Fix All tests use the audit's own shapes.
- FC-035: expected values come from the spec examples (09-spec-author), not from the code.
- FC-037: no comment-only change claimed.

### Visible output is decided

Every changed expected value is covered:
- RH3004 moved code: RH3004-006 and SHARED-FIX-007 (unchanged), Decision 17, answer 6 (A, row 20), answer 7, and rows 24 and 25, decided on recommendation under the standing instruction in `Maintainer answers`.
- RH3106 positions: answers 2 and 4, rows 16, 21, and 23.
- Fix All results: SHARED-FIX-002/-003 and row 19.

#8 is visible output that nothing decides and no test asserts. It is filed as a requirement that cannot hold, for the lead to decide on recommendation.

### Scope

Every changed path is named in handovers 09–13 or in rows 15–25. The new `DocumentOrderFixAllProvider` is the shared Fix All path that #1 suggested; it reaches RH3107 through the shared base, with unchanged results. Nothing changed outside the run.

### Architecture findings

**Repeated classes.** #5, #6, and #7 repeat the classes of #2, #1, and #3.
- #5 (RH3106 analyzer and fix against RH3106-002/-003): the fix still decides "does the binding depend on the list" by position. The repair added one more position (binary operators) where #2 suggested gating on the outcome. Compound assignment, collection initializers and expressions, and anonymous-method returns are the next members. The analyzer's natural-type positions also gained a member that is now wrongly exempt (explicit return type).
- #6 (provider against BatchFixer / Fix All path): the shared document-order path decides interacting instances correctly but drops nested ones. RH3004 never moved off BatchFixer, so its own Fix All path still disagrees with SHARED-FIX-002.
- #7 (the RH3004 fix against the formatter): #3's structural option was taken. The fix now lays out only what it creates, and moves the original lines itself. In doing so it took over a decision the formatter's indentation phase owns, how the lines of a raw string literal move with their code, and its copy diverges on whitespace-only lines.

A shared "move these lines by a column delta, keeping literal values" helper, used by the formatter's `RawStringLineShifts` and by the fix, would put that decision in one place. The 11-implementer follow-up on RH7004 and `TypeMemberOrderingCodeFixProviderBase` needs the same helper.

### Hints

1. `ParsesAsWritten` has no test of its own for the new bounded path's edges: a rewrite in a top-level statement (a `GlobalStatementSyntax` member), a change above every member, and the exit when the reparsed member's span differs. The static trace holds. The recursion depth is now bounded by the member (follow-up candidate already recorded).
2. `MoveStatementsAsync` returns the formatter's output, with reformatted statements as in #3, when the formatted lambda has no block or its `}` does not start its line. I could not construct an input that reaches either exit, because FMT-BREAK-002 always puts `}` on its own line.
3. `DocumentOrderFixAllProvider` maps a span that no change overlaps by offset. The text under it is unchanged, but its parent node could in principle differ after an earlier fix regroups the code around it. None of the five providers writes such a regrouping (RH3005 regroupings are withheld by `ParsesAsWritten`). Not constructed.
