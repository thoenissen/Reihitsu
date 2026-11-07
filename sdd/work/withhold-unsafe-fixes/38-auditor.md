Role: auditor
Commit: 3108e585
Verdict: findings — #1257: pass; #1279: findings (#20, #21); #1281: findings (#22); #1282: pass; #1417: pass; #1422: pass

My prompt named the run, the SHAs, the handovers to read, the delta (`ee680a32..3108e585`, rows 44–45 for findings 17–19), and what the narrowing is meant to achieve. It carried no intended fix, suspected cause, or hoped-for verdict, so I had nothing to set aside.

## Scope

Head `3108e585d1bacddbba59b9f4ca7f498bb2b60100`, checked out and clean. Base `origin/main` = `0fa1778c875793aaba22d10fef14747d4988f2ed`; `git ls-remote` agrees, and it is an ancestor of head.

Repair delta against `ee680a32`. It matches handovers 33–37 and rows 44–45:
- `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs`: `LeavesStringOpen` becomes `ContainsStringWithDirectiveLine` (any `#` line in a string, no branch logic); `IsInBranch` is removed; new `ContainsCrossingDirectiveChain`.
- Specs: RH3004-003, -006 (a pointer), -008 (rewritten), -009 (new); RH3106-003 (deliberate gap); `requirement-ids.txt`.
- Tests: 13 in the RH3004 class, 5 in the RH3106 class.

Scope did not grow. I trusted the cleaner's build and the roles' focused test results.

Method:
- static tracing of the changed guards against RH3004-006/-008/-009, SHARED-FIX-003 and RH3106-003;
- `scripts/apply-fix.sh RH3004` and `RH3106` on fixtures in the session scratchpad (`audit6/`), LF and CRLF;
- the scratch Roslyn compile check (`audit4/cc`) on every fixture I cite, with and without `A`;
- `scripts/format.sh` on a copy of one fixed output.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 17 | closed | Both listed members are withheld and tested: (a) `FixWithheldForAChainOpenedBeforeTheAnonymousMethodWhoseElseLiesInAStringOfTheBlock` and (b) `FixWithheldForAStringOfAnActiveBranchOpenedBeforeTheAnonymousMethodThatHoldsADirectiveLine`. Members #17 did not name still fail; see #20, which repeats #17. |
| 18 | closed | Both listed inputs are withheld by RH3004-009 and tested (`FixWithheldForAChainOpenedBeforeTheAnonymousMethodThatEndsInsideItsBlock`, `FixWithheldForABlockThatHoldsOnlyTheEndifOfAChainOpenedBeforeTheAnonymousMethod`). Members #18 did not name still fail; see #21, which repeats #18. |
| 19 | closed | The three listed inputs are now the RH3106-003 deliberate gap: reported, no code fix, tested. Spec and code agree for them. Other members of the class are reported but lie outside the gap's wording; see #22, which repeats #19. |

## thoenissen/Reihitsu#1257

Pass. Untouched by the delta.

## thoenissen/Reihitsu#1279

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 20 | high | `RH3004UseLambdaSyntaxCodeFixProvider.cs` `ContainsStringWithDirectiveLine` (checks only `IsString` nodes) and `ContainsLineStartingWithHash` (skips only spaces and tabs via `SkipIndentation`) | FC-016, FC-002, FC-022, FC-001 | #17 | same requirement (SHARED-FIX-003, RH3004-008's Why; PR-introduced moved-code path) | A line that is a directive in another build can also sit somewhere other than a string. The two checks miss it in two ways. In both cases the fix still moves a line that is string text in that build, so the string's value changes there. `main` moved no line. All inputs compile with and without `A`, and LF and CRLF give the same result. **(a) Multi-line comment, chain wholly outside the anonymous method** (`audit6/rh3004/p1_comment_outside.cs`): `#define A … #if A⏎        Action a = delegate ()⏎        {⏎            /* x⏎#else⏎        Action a = () => { var s = @"⏎            */ ";//".ToString();⏎            Console.WriteLine(1); };⏎#endif`. With `A`, `#else` is comment text, the block holds no directive and no multi-line string, and the fix is offered. It moves `            */ ";//".ToString();` from column 12 to 23. Without `A`, that line is inside the verbatim string `s`, so `s` gains 11 spaces. **(b) The same with `/** */`** (`p6_doccomment_outside.cs`): same result. **(c) Multi-line comment, chain wholly inside the block** (`p5_comment_inside.cs`): `{⏎#if A⏎            /* x⏎#else⏎            var s = @"⏎            */ ";//".ToString();⏎#endif⏎            Console.WriteLine(1);⏎        }` with `A`. RH3004-009 does not apply: in build `A` the chain is `#if A`/`#endif` inside the block. The fix moves the `*/ ";//"` line from 12 to 23, and without `A` `s` changes. It also moves the column-0 `#else` to column 11. **(d) Whitespace other than space or tab in front of `#`** (`audit6/rh3004e/p10_formfeed.cs`): #17(b)'s input, but the `#else` line inside `@"x` starts with a form feed (`\f#else`). It is still a directive without `A`: a probe with `int q = "x";` in that branch fails with CS0029 without `A`. `SkipIndentation` stops at `\f`, so the string counts as having no `#` line. The fix moves `; // ";` from 12 to 23, and without `A` `s` changes. RH3004-008 says "first character other than whitespace", so the code is also narrower than the spec here. **Holding:** a string that holds a `#` line behind spaces or tabs (tests); disabled text that leaves a string open (tests); `p5` without `A`, where the `*/` line is in the active string and stays; a comment `#` line in no branch, which cannot change a value. | Decide RH3004-008 item 1 on every construct of active code that hides a line's `#` from the directive scanner: a string literal, an interpolated string, and a multi-line comment, including a multi-line documentation comment. Measure "first character other than whitespace" with the lexer's whitespace (`SyntaxFacts.IsWhitespace`), not spaces and tabs only. The spec author widens RH3004-008 item 1 to name comments. Tests: (a), (c) with `A`, and (d), no code fix offered. |
| 21 | high | same file, `ContainsCrossingDirectiveChain` (directives inside `block.Span` only, "outside" meaning outside the anonymous method) against `ApplyCodeFixAsync` (`SyntaxFactory.ParseExpression($"{ParameterList} => {body}")`, which parses with no preprocessor symbols, then `.WithTriviaFrom(…)`) | FC-020, FC-004, FC-006, FC-028, FC-001 | #18 | same requirement (SHARED-FIX-003, RH3004-006/-009; #1279 "withhold the fix where the rewrite would not compile") | RH3004-009 inspects only conditional directives inside the block and only partners before `delegate` or after `}`. The fix rebuilds text in two pieces, the parameter list and the body, inserts `=>` between them, and reparses the result with no symbols. A directive structure split by that seam, or unmatched in the fragment, still reaches the reparse. **(a) Code and directive loss, chain from the parameter list into the block** (`audit6/rh3004b/p7_param_chain.cs`): `Action<int> a = delegate (⏎#if A⏎            int x)⏎        {⏎            Console.WriteLine(x);⏎#else⏎            int x) { Console.WriteLine(-x);⏎#endif⏎        };` with `#define A` compiles in both builds. `apply-fix` reports `invalid-result`. The output ends in `int x);`: `#endif`, the `#else` branch's text, and `};` are deleted. The partner `#if A` lies inside the anonymous method, so RH3004-009 does not withhold. **(b) Chain from the parameter list to after the anonymous method** (`audit6/rh3004d/p7b_param_chain_after.cs`): the block holds no directive. `invalid-result`: `delegate (⏎#if A⏎ int x)⏎ { … };` becomes `(⏎;`. **(c) The same on the expression-body path** (`p7c_param_chain_return.cs`, `{ return x; }`): `invalid-result`, `(⏎;`. **(d) `#endregion` in the block whose `#region` stands before `delegate`** (`audit6/rh3004/p3_region_before.cs`; compiles): the fix is offered and writes `() => {` with the brace on the arrow's line. Whole-document formatting of that output puts `{` on its own line, so this is #18's layout defect again (RH3004-006, SHARED-FIX-007); it equals `main`'s output. Members (a)–(c) are not new: `main`'s fix builds and reparses the same fragment and drops the skipped text through `WithTriviaFrom` the same way. They are in the run's requirement surface, though: issue #1279, SHARED-FIX-003, and the class RH3004-009 was written to close. **Holding:** `#region` in the block with `#endregion` after the anonymous method (`p4_region_after.cs`: laid out); a conditional chain wholly inside the block, also with branches whose braces differ between builds (`audit6/rh3004c/p9_inside_unbalanced.cs`, `p9b_…`: both builds compile after the fix); the inputs of #18. | Final round, so narrow: withhold the fix, on both paths, when any conditional directive in the anonymous method's parameter list or block has a chain partner outside the block (that is, before the block's `{`), and when a `#region`/`#endregion` inside the anonymous method has its partner outside it. The second withholding can alternatively be a layout gap recorded in RH3004-006. Or decide RH3004-009 on "every directive of the anonymous method whose chain is not wholly inside the block, or wholly inside the parameter list". The spec author words RH3004-009 accordingly. Tests: (a), (b), (c) with `A`, and (d). Record in the RH3004-009 follow-up that the reparse ignores the document's symbols. |

### Checked and holding

- **`ContainsCrossingDirectiveChain`.** `DescendantTrivia()` returns every directive trivia of the block, inactive branches included. `GetRelatedDirectives()` returns the whole chain for `#if`, `#elif`, `#else` and `#endif`. The span filter leaves out the leading trivia of `{`, which `ContainsDiscardedTrivia` already withholds for. Nested chains wholly inside the block do not withhold. The check runs before the path split. As the spec checker showed, it is equivalent to running it on the block path only, except that it does not see directives of the parameter list (#21).
- **`ContainsStringWithDirectiveLine`.** It keeps the disabled-text item unchanged. It visits only the outermost string (the `Ancestors().TakeWhile(...)` filter) and stays gated to the block path. `return @"a⏎#b";` keeps the expression-body fix, which moves no line (test).
- **#17 and #18 inputs.** All are withheld, by tests that read the output.
- **Removal of `IsInBranch`.** No caller remains, and the `using`s it needed are still used elsewhere (the cleaner's build is clean).

## thoenissen/Reihitsu#1281

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 22 | high | `sdd/specs/analyzer/RH3106.md` RH3106-003 deliberate gap (names only "invoked after an element access in the same expression, directly or through `Invoke`") against `RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer.TakesNaturalType` / `RewriteBindingUtilities.BindsAsBefore` (unchanged) | FC-016, FC-027, FC-002 | #19 | same requirement (RH3106-003, maintainer answer 2) | Row 45 keeps today's output and records it as a gap. The recorded gap is narrower than today's output. The analyzer reports, with no code fix, every implicitly typed array element whose array or element value meets a path node that fails with the probe's `Action<int>`. Invocation is only one such node. For the members below, RH3106-003 still says MUST NOT report, because the anonymous method takes its natural type and nothing chooses for it. The gap does not name them, so spec and code disagree: a false-positive diagnostic against the spec. **Probed (`audit6/rh3106/q*.cs`; each compiles; LF and CRLF; all reported with no fix offered):** `Action x = new[] { delegate () { } }[0];` · `Run(new[] { delegate () { } }[0]);` with `Run(Action)` as the only `Run` · `Action y; y = new[] { delegate () { } }[0];` · `Func<int> g = new[] { delegate () { return 1; } }[0];` · `new[] { delegate () { } }[0].BeginInvoke(null, null);` · `Action[] arr2 = new[] { delegate () { } };` · `IEnumerable<Action> e = new[] { delegate () { } };`. The `Action[] arr2` and `IEnumerable<Action>` shapes are ordinary code. **Holding (not reported):** `[0].DynamicInvoke()`, `object o = …[0]`, `Delegate d = …[0]`, and the spec's `.Length`, `.Method` and `is Action`. **Covered by the gap as written:** `[0]?.Invoke()` and `(new[] { … })[0]()`. | The narrowing is final, so record the gap for the whole class rather than per node: an anonymous method that takes its natural type as an element of an implicitly typed array creation is reported, with no code fix, when the array, or a value taken from it, is converted to a delegate type or an array or collection type of delegates, assigned, passed as an argument, or invoked (directly, `Invoke`, `BeginInvoke`). Not reported: initializers of `var`, conversions to `object` or `Delegate`, and member access or `is` that binds for any delegate type. Alternatively, word the gap by its mechanism-free outcome: "where any expression around the array creation would bind differently for another delegate type". Widen the RH3106 follow-up candidate in `00-lead.md` from "invocation, element access, `Invoke`" to conversions as well. Tests: at least `Action x = new[] { … }[0];`, `Run(new[] { … }[0])`, and `Action[] arr2 = new[] { … };`, reported with no code fix. |

### Checked and holding

- **No RH3106 code change in the delta.** Today's outputs were already audited at `ee680a32`. The five new tests pass and characterize the recorded gap and its not-reported side.
- **Spec text.** The deliberate-gap paragraph follows SHARED-DIAG-011's rule that a corner case may be recorded as a gap. A withheld fix changes no code, so nothing unsafe is offered.

## thoenissen/Reihitsu#1282

Pass. Untouched by the delta.

## thoenissen/Reihitsu#1417

Pass. Untouched by the delta.

## thoenissen/Reihitsu#1422

Pass. Untouched by the delta.

## Whole run

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH3004-008 item 1 (`ContainsStringWithDirectiveLine`) | outermost string nodes of the block; lines after the first whose first non-space, non-tab character is `#` | the lexer's directive rule (first non-whitespace `#` outside strings and comments) and RH3004-008's Why (a line that is string text in one build and a directive in another) | narrower: misses multi-line and documentation comments as carriers, and whitespace other than space or tab | fails (#20) |
| RH3004-008 item 2 (disabled text) | each disabled-text trivia lexed alone; a string token running open to its end | RH3004-008 second item | identical (unchanged) | holds |
| RH3004-009 (`ContainsCrossingDirectiveChain`) | conditional directives inside `block.Span`; partners outside the anonymous method | what the fix rebuilds: the parameter-list text, `=>`, and the body text, reparsed with no symbols | narrower: parameter-list directives and chains split at the `=>` seam are not seen; `#region`/`#endregion` are not seen | fails (#21) |
| RH3106-003 deliberate gap | invocation after element access, directly or `Invoke` | `TakesNaturalType`: any path node above the array creation that fails or changes symbol under the probe | narrower than the code | fails (#22) |
| RH3004 block-path gate | `GetExpressionBody == null` for RH3004-007/-008; RH3004-009 on both paths | `ApplyCodeFixAsync` path split | identical | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| A string line that starts with `#` | `rg -n "ContainsLineStartingWithHash" src`: RH3004 only | one owner |
| A conditional chain crossing a span | `rg -n "GetRelatedDirectives" src`: RH3004 (new), RH7004, RH7302, RH7306; each asks a different question | not a duplicated policy |
| Outermost string of the block | `GetOutermostString` and the inline `Ancestors().TakeWhile(...).Any(IsString) == false` in `ContainsStringWithDirectiveLine` | same semantics as at `ee680a32`; parity holds by reading |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH3004, 4 "string holds a `#` line" withheld tests (17(b), no directive, CRLF, `#` behind spaces) | no code fix | a fix offered | `VerifyNoCodeFixIsOffered` | adequate. No comment carrier and no other whitespace (#20). |
| RH3004, 2 "`#` behind text" tests and the expression-body test | exact output | a different text | `Verify`, `ApplyCodeFixAsync` (CRLF) | adequate |
| RH3004, 5 RH3004-009 withheld tests | no code fix, with and without `A` as the input needs | a fix offered | `VerifyNoCodeFixIsOffered` (symbol `A` where needed) | adequate for chains crossing at `delegate` or `}`. None crosses the parameter-list seam, and none is a region (#21). |
| RH3004, `FixRewritesAnAnonymousMethodInsideAChainThatLiesWhollyOutsideIt` | exact output | a different text or no fix | `Verify` | adequate |
| RH3106, 5 gap tests | reported with no fix / not reported | a fix offered, a missing or extra report | `VerifyNoCodeFixIsOffered`, `Verify` | adequate for the shapes named. None for a conversion or argument (#22). |

### Failure classes dismissed

- FC-003: both sides of each new condition are tested (`#` at line start against `#` behind text; a crossing chain against one wholly outside; block path against expression-body path).
- FC-005: RH3004-009 is about directives only, and the comment hazard is #20.
- FC-007/FC-009: every probe that was fixed converged in one iteration. The `invalid-result` cases are #21.
- FC-008/FC-011: no change to `DocumentOrderFixAllProvider`. A withheld single fix is withheld in Fix All.
- FC-010, FC-012–FC-014: no formatter phase changed.
- FC-015: see *Policy ownership*.
- FC-017: see *Assertion adequacy*.
- FC-018/FC-023/FC-024/FC-025/FC-026/FC-029/FC-036: no rename, identifier, member-kind, token re-resolution, or key/value logic changed.
- FC-030: unchanged; the fix formats only the lambda.
- FC-031: the summaries of `ContainsStringWithDirectiveLine` and `ContainsCrossingDirectiveChain` describe the code. No `TODO` and no tracker number.
- FC-032: every probe ran on a fresh build of `3108e585`.
- FC-033: every RH3004 and RH3106 probe ran under LF and CRLF with the same outcome.
- FC-034/FC-035: the tests use the audit's inputs and the spec's values.
- FC-037: no comment-only change in the delta.

### Visible output is decided

Every expected value the delta adds or changes is covered:
- RH3004-008/-009 withheld fixes: row 44, decided on recommendation under the standing instruction, and SHARED-FIX-003;
- RH3106-003 gap: row 45, likewise.

No rule page changed. The outputs the new findings need:
- #20 and #21: withheld fixes under SHARED-FIX-003, decided the same way as row 44;
- #22: the observed output stays, and the gap's wording catches up;
- #21(d): a withheld fix, or the layout gap recorded.

None is an undecided output.

### Scope

Every changed path is named in handovers 33–37 or rows 44–45.

### Architecture findings

**Repeated classes.** #20 repeats #17 (and through it #15 and #7). #21 repeats #18. #22 repeats #19 (and through it #14, #10, #5 and #2). The narrowing narrowed each class to the members the previous audit named, and stopped short of the class itself.

- **#20, the RH3004 moved-code path against literal values in another build.** The class is "a line that the directive scanner skips in this build but sees in another". A string is one way to hide a `#` from it; a multi-line comment is the other, and the scanner's whitespace is wider than spaces and tabs. A narrowing that ends the class decides on every token or trivia of active code that spans lines (string literals, interpolated strings, multi-line and documentation comments) and on the lexer's whitespace.
- **#21, the RH3004 fix's text reassembly (`ParseExpression` of `{ParameterList} => {body}`, no symbols, `WithTriviaFrom`) against the document's directive structure.** #18 named the structural cause: the fix reparses a fragment. RH3004-009 closes only the chains that cross at `delegate` or `}`. Any directive of the anonymous method not balanced within the block reaches the reparse, and the parameter-list members lose code. A narrowing that ends the class withholds whenever a directive of the anonymous method has a partner outside the block. The structural fix, building the lambda from the original nodes (`SyntaxFactory.ParenthesizedLambdaExpression` with the original block), is already the follow-up candidate of row 44.
- **#22, the RH3106 analyzer's probe against RH3106-003.** Error counting cannot tell a node that consumes the value from one that chooses for it (#19's analysis). A gap recorded per node then repeats per node. A gap worded on the array-element class (or on the outcome) ends the class without code change.

### Hints

1. #20(c) also moves a column-0 `#else` that is comment text in build `A` to column 11. In the other build it stays a directive (leading whitespace is allowed), so this is layout only, and it is closed together with #20.
2. RH3004-008 item 1 now withholds a `#` line in a string with no conditional directive anywhere in the document (`@"a⏎#b"`). That is safe ground given away on purpose (row 44(b), the spec says so), so it is not a finding.
3. Hints of `32-auditor.md` are unchanged by the delta; hint 3 there (whole-document formatting changing a verbatim string's value across builds) applies to #20's comment carrier as well.
