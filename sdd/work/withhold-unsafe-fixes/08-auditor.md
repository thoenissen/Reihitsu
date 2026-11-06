Role: auditor
Commit: af76b48e
Verdict: findings — #1257: findings (#1); #1279: findings (#3); #1281: findings (#1, #2); #1282: pass; #1417: pass; #1422: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict; there was nothing to set aside.

## Scope

Full audit. Head `af76b48eefea804727569ae1346179238e71931b` (checked out, clean). Base `origin/main` = `0fa1778c875793aaba22d10fef14747d4988f2ed`, equal to `git ls-remote origin refs/heads/main`, and an ancestor of head. The diff (52 files) matches handovers 01–07: the shared re-parse check `RewriteParseUtilities`, the shared empty-list fix base, the guards in the RH3001/RH3004/RH3005/RH3101/RH3104/RH3105/RH3106/RH3107/RH3201 fixes, the analyzer changes of RH3103/RH3104/RH3106/RH5417, specs, two rule pages, and tests. I trusted the cleaner's build and the roles' focused test results.

Method: static tracing of every changed guard against the code it protects and its counterpart, then constructed inputs. Fixtures live in my scratchpad outside the repository. Single fixes ran through `scripts/apply-fix.sh <ID> <dir>` (LF and CRLF). Fix All and compile checks ran through a scratch file-based app that references `src/Reihitsu.Tooling` (so the analyzers and fixes of this head): it reports every diagnostic, whether a fix is registered for it, applies the provider's own `GetFixAllProvider()` (BatchFixer) to the document, and compiles the result. All inputs below compile without errors; the errors named are those of the fixed output.

## thoenissen/Reihitsu#1257

### Findings

See #1 under *Whole run*: the per-instance re-parse guard of RH3005, RH3101 and RH3105 does not hold under Fix All when two instances together form the generic reading.

### Checked and holding

- Single fix, every member of 02-skeptic's matrix (`>`, `>>`, `>>>`, `(` follower, cast reading followed by `(`, `!`, `with`): withheld by `RewriteParseUtilities.ParsesAsWritten` on the exact replacement the fix writes. Spot-checked `M(A < this.B, D > (7))`: reported, no fix registered.
- RH3001 guard checks the parenthesized replacement while the applied fix lets the simplifier drop the parentheses. The guard is the stricter of the two, and Roslyn's simplifier keeps parentheses whose removal is ambiguous, so the narrower output never escapes the guard.
- `ParsesAsWritten` compares token kind, text and missing flag plus node kinds, not trivia. A token merge or a new comment changes the token stream and is caught; a change confined to trivia cannot change meaning in the shapes these fixes write.

## thoenissen/Reihitsu#1279

### Findings

#3 (below).

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | high | `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs` `ApplyCodeFixAsync` (`ReihitsuFormatter.FormatNodeInDocumentAsync` on the whole lambda) and `RestoreDocumentationCommentsAsync` | FC-030, FC-016, FC-027 | – | PR-introduced | RH3004-003 keeps "the block's statements, unchanged", and SHARED-FIX-007's moved-code bullet keeps their braces, token spacing, line breaks and blank lines (only indentation follows). The fix now formats the whole lambda with every formatter phase, so it rewrites the statements inside the block. On `main` the block text was copied verbatim. Input, single fix: `Action<int> b = delegate (int x) { int L() => x+1; if(x>1) Console.WriteLine(L()); switch (x) { case 1: A(); B(); break; … } var q = new { A = 1, B = 2 }; ⏎⏎⏎ Console.WriteLine(q); };` with each statement on its own line. Output: `int L() { return x + 1; }` (expression body turned into a block body), `if (x > 1) { … }` (braces added), braces added around the `case 1:` statements, the anonymous object wrapped over four lines, three blank lines collapsed to one, `x+1` → `x + 1`. Failing members: structural transform (expression-bodied local function), embedded-statement braces, switch-section braces, initializer wrap, blank-line collapse, token spacing, and a multi-line `///` comment inside a moved block: the restore writes the original trivia back, so its continuation lines stay at their old column (`/// <summary>` at column 28, `/// Text that continues` and `/// </summary>` left at column 12). Holding: a one-line `///` comment (restored, test `DocumentationCommentedAnonymousMethodKeepsItsBody`); statements the formatter leaves as they are; the lambda header, braces and expression body. No maintainer answer covers rewriting the statements: answer 1 decided the block's position, not its contents. No test has a statement the formatter would change. | Lay out only what the fix creates: the lambda header and the braces at the anchor column, and shift the block's lines by the column delta as moved code, without running the other phases on the statements. That is the layout-only entry point Decision 12 put into a follow-up, or an equivalent that formats the header and braces only. Then drop the documentation-comment restore. Add tests: a block with a statement each formatter phase would rewrite, and a multi-line `///` comment in a block that moves. See *Architecture findings*. |

### Checked and holding

- `IsOperandOfCastOrBinaryOperator` inspects `Parent` only. That is the span RH3004-005 names ("directly the operand"). Parenthesized operand, compound assignment, and conditional branch hold, as 02-skeptic's table shows.
- Layout of the lambda header and braces (`= delegate (int x)` with `{` on the next line, one-line block): matches the spec examples, LF and CRLF tested.

## thoenissen/Reihitsu#1281

### Findings

#2 (below) and #1 (RH3106 member, under *Whole run*).

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | high | `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3106UnnecessaryDelegateParenthesesShouldBeRemovedCodeFixProvider.cs` `KeepsMeaningAsync` (forks the document only when an `ArgumentSyntax` encloses the anonymous method); `src/Reihitsu.Analyzer/Rules/Clarity/RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer.cs` `TakesNaturalType` (looks only at the direct parent) | FC-001, FC-016, FC-027, FC-006 | – | same requirement (RH3106-002, RH3106-003, SHARED-FIX-003) | The decision is whether the anonymous method's conversion or a binding depends on its parameter list. The analyzer checks only the anonymous method's direct parent, and the fix checks binding only under an argument. Every other position where inference or operator resolution decides is reported, and the fix is offered and breaks the code. Failing, single fix: `var a = (delegate () { });` → `var a = (delegate { });`, CS8917; `var a = new[] { delegate () { } };` → CS0826; `var f = () => delegate () { };` (inferred lambda return type) → CS8917; `c + delegate () { }` where `C` declares `operator +(C, Action)` and `operator +(C, Action<int>)` → `c + delegate { }`, CS9342 (operator resolution ambiguous). Holding: `var a = delegate () { };`, `object o = …`, `Delegate d = …`, a cast to a non-delegate type, `object o = (delegate () { });`, `Run((delegate () { }))` with `Run(object)` (all not reported); `b ? delegate () { } : delegate () { }` and switch-expression arms under `var` (not reported); arguments with competing overloads, constructor initializers, inferred generic methods (withheld); `new Action(delegate () { })` (withheld). | Gate the fix on the outcome, not on the position. Run the fork for every reported list, and withhold when the anonymous method's converted type, or the symbol of any enclosing invocation, creation, initializer, element access, or user-defined operator, differs or fails after the removal. Separately, the lead decides whether the three natural-type members are "not reported" under RH3106-003 (as answer 2 decided for `var`; the spec's headline already says "takes its natural type") or only lose the fix. Whichever way the lead decides, SHARED-FIX-003 forbids the fix. The spec author widens RH3106-002 to operator operands. Tests for each failing member. |

### Checked and holding

- RH3005 regrouping (`1 == 1 == b`) is withheld: the swap is now built as nodes, so `ParsesAsWritten` sees `(b == 1) == 1`.
- RH3101-007, `BindsToOverridableMember`: virtual, abstract, and non-sealed override members, properties, and indexers are withheld from an unsealed containing type. Sealed containing type, sealed override, struct, and an access inside a lambda or local function hold (`GetEnclosingSymbol(...).ContainingType` reaches the type).
- RH3201-006/-007: the guard removes every comment between the two tokens and keeps the whitespace, and the applied removal keeps the whitespace that separates the tokens, so guard and rewrite agree. Probed `return a/**/⏎/**/+1`, `a +/**/⏎/**/+a`, adjacent comments, a comment inside a directive (structured trivia, skipped). No merge.

## thoenissen/Reihitsu#1282

### Findings

_None._

### Checked and holding

- RH3101/RH3105: the comment span `FromBounds(Expression.Span.End, Name.SpanStart)` is exactly what the rewrite drops. `Name.WithTriviaFrom(memberAccess)` keeps the leading trivia of `this`/`base` and the trailing trivia of the name.
- RH3104: `InteriorContainsCommentOrDirective(creation)` covers every token gap that `default(S)` drops.
- Empty-list base: `InteriorContainsCommentOrDirective(list)` covers the removed interior. The trivia in front of `(` and behind `)` is carried into the separating trivia, comments included. Probed `delegate () /* b */ { }` and `delegate () // c⏎{`: comment kept, no trailing whitespace, one space before `{`.

## thoenissen/Reihitsu#1417

### Findings

_None._

### Checked and holding

- `ShouldSkip` walks up through qualified, alias-qualified, and member-access names and skips the five positions RH3103-007 lists. The holding positions in 02-skeptic's table (`is`, `is … ? :`, `as`, cast, `default`, array, type argument, `typeof(Nullable<int>)`, `new Nullable<int>(…)`) are still reported. The alias gate reads the tree's parse options, and tests run on both sides of C# 12 (FC-003).
- Hint 2 covers the code fix's member-access arm.

## thoenissen/Reihitsu#1422

### Findings

_None._

### Checked and holding

- RH3104 `StandsAsStatement` covers: expression statement; `for` initializer and incrementor (a `for` declaration stays reported, where `default(S)` is valid); arrow bodies of void methods, constructors, finalizers, `set`/`init`/`add`/`remove` accessors and local functions; expression lambdas whose symbol returns void, and async `Task`/`ValueTask`. Value positions stay reported.
- RH5417 `IsInsideSingleLineInterpolation`: any non-verbatim `$"` ancestor below C# 11, nested in either order. The `$@"` holes, which allow line breaks before C# 11, stay reported. C# 10/11 tests on both sides.

## Whole run

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `GetFixAllProvider()` → `WellKnownFixAllProviders.BatchFixer` in `RH3005UseReadableConditionsCodeFixProvider.cs`, `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsCodeFixProvider.cs`, `RH3105DoNotPrefixLocalMembersWithThisCodeFixProvider.cs`, `EmptyParenthesesCodeFixProviderBase{TNode}.cs` (RH3106) | FC-008, FC-006, FC-016 | – | same requirement (RH3005-008, RH3101-005, RH3105-005, RH3106-002, SHARED-FIX-002, SHARED-FIX-003) | Each guard decides one instance on the original document. BatchFixer then merges the single rewrites, so two instances that are each safe alone can together write the reading the guard exists to prevent. Applied one after another, the later fix would be withheld, so the Fix All result also differs from the sequential one (SHARED-FIX-002). Failing: RH3105 `M(this.A < this.B, this.C2 > (this.D));` (four `int` fields): every fix registered, Fix All → `M(A < B, C2 > (D));`, CS0307/CS0246. RH3101 the same with `base.` on four fields of the base class: same result and errors. RH3005 `M(Max > G, (7) < B);` (`const int Max`): both registered, Fix All → `M(G < Max, B > (7));`, CS0307/CS0246. RH3106 `Run(delegate () { }, delegate () { });` with `Run(Action, Action)` and `Run(Action<int>, Action<int>)`: each removal alone keeps the binding, both registered, Fix All → `Run(delegate { }, delegate { });`, CS0121. Holding: RH3001 (its rewrite writes `x == false`, never a type-shaped operand, so it cannot complete another instance's type argument list); RH3201 (the guard removes every comment between the same token pair, and different pairs do not interact); RH3104, RH3107, RH3004 (their rewrites change neither the tokens nor the binding of a neighbouring instance). The nine `FixAll…Reported` tests put the withheld and the fixable instances in separate statements, so none of them combines interacting instances. | Give these four providers a Fix All provider that applies the document's fixes in document order on the evolving document and re-runs the rule's guard before each one, as `FixAllProvider.Create` allows (RH3002 already has its own Fix All provider). Alternatively, check the merged result and fall back to the sequential path when it fails. Add one interacting-instance Fix All test per rule. |
| 4 | medium | `src/Reihitsu.Core/RewriteParseUtilities.cs` `ParsesAsWritten`, called from `RegisterCodeFixesAsync` of RH3001, RH3005, RH3101, RH3105, RH3201 | — (code quality: hot path) | – | new mechanism | Each call serializes the whole root, scans the whole text for the common prefix and suffix, and walks the whole reparsed tree against the rewritten one. The cost is linear in the file for every diagnostic, so Fix All and the light bulb over a large file are quadratic. Measured on a 3,205-line file with 2,000 RH3105 diagnostics (`M(this.A + j);`), Debug build of this head: registering the fixes took 14.8 s, about 7.4 ms per diagnostic. `main` has no such per-diagnostic work. | Bound the comparison to the change: compare only the smallest node covering the changed span in both trees (the incremental parse reuses everything outside it), and derive the text change from that node's span instead of the whole text. |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH3001 rewrite parses as written | whole tree, with the parenthesized replacement | applied fix: same replacement, parentheses simplified by Roslyn | guard stricter; the simplifier keeps ambiguous parentheses | holds (single fix); Fix All: #1 holds for RH3001 |
| RH3005 swap parses as written | whole tree, with `BuildSwappedExpression(...).WithTriviaFrom(...)` | `ApplyCodeFixAsync`: same builder | identical | holds (single); fails under Fix All (#1) |
| RH3101 `CanRewrite` | comment span `Expression.Span.End..Name.SpanStart`; symbol and candidates; whole tree with `BuildReplacement` | `ApplyCodeFixAsync`: same `BuildReplacement` | identical | holds (single); fails under Fix All (#1) |
| RH3105 guard | same span; whole tree with `Name.WithTriviaFrom(memberAccess)` | `ApplyCodeFixAsync`: same replacement | identical | holds (single); fails under Fix All (#1) |
| RH3201 `KeepsTheTokensApart` | the previous token's trailing trivia and the next token's leading trivia, comments removed, whitespace kept | `ApplyCodeFixAsync` removal span (whitespace in front removed only when nothing touches the comment's end, or the comment starts its line) | the removal never takes the whitespace that separates the two tokens | holds |
| RH3104 comment guard | `creation.Span` | `default(S)` written from the type alone | covers every dropped gap | holds |
| SHARED-PARENS-003 | the list's interior | removal of `(`…`)`; outer trivia carried | covers the dropped interior | holds |
| RH3106 binding/conversion (fix) | only when an `ArgumentSyntax` encloses the node up to the statement | the analyzer's `TakesNaturalType` (direct parent) | neither covers parenthesized `var` initializers, implicitly typed array elements, inferred lambda returns, or operator operands | fails (#2) |
| RH3106 natural type (analyzer) | the direct parent; converted type; candidates of an enclosing argument's call | RH3106-003 "takes its natural type" | narrower than the requirement's headline | fails (#2) |
| RH3004 operand guard | `Parent` is cast or binary | RH3004-005 "directly the operand" | identical | holds |
| RH3101 overridable member | containing type of the enclosing symbol; bound symbol or candidates | RH3101-007 | identical | holds |
| RH3103 `ShouldSkip` | outermost qualified/alias/member-access name around the generic name | RH3103-007 bullets; the code fix's member-access arm | the analyzer is now strictly narrower than the fix's arm (Hint 2) | holds |
| RH3104 `StandsAsStatement` | parent: expression statement, `for` parts, arrow clause, expression lambda | RH3104-005 list | identical | holds |
| RH5417 interpolation gate | every `InterpolationSyntax` ancestor whose string starts with `$"`, below C# 11 | RH5417-005 | identical | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| A rewrite still parses as the same tokens and structure | `src/Reihitsu.Core/RewriteParseUtilities.cs` `ParsesAsWritten` (RH3001, RH3005, RH3101, RH3105, RH3201); RH3002 analyzer's private generic-name scan (`ScanTypeArgumentList`, `WouldDetachTypeArgumentList`) | Two owners. Justified by Decision 3, and the RH3002 consolidation is already a follow-up candidate. No new finding. |
| Two tokens would merge without the trivia between them | `src/Reihitsu.Core/TokenSeparationUtilities.cs` `WouldMergeIntoOneToken` (RH3002 fix, formatter); `ParsesAsWritten` (RH3201) | Two owners of overlapping scope. `ParsesAsWritten` is the wider check, chosen by Decision 3. Parity is not proven, but no divergence that corrupts output was found. Fold it into the RH3002 follow-up. |
| Empty-list removal | `src/Reihitsu.Analyzer.CodeFixes/Base/EmptyParenthesesCodeFixProviderBase{TNode}.cs` only (`rg SyntaxRemoveOptions.KeepExteriorTrivia` finds no RH3106/RH3107 copy) | one owner |
| Comment or directive in a span | `SyntaxNodeUtilities.SpanContainsCommentOrDirective` / `InteriorContainsCommentOrDirective` | reused, no copy |
| Language-version gate | `LanguageVersion: <` patterns in RH3103, RH5417, RH7003 | inline pattern, no shared helper exists; not a policy copy |
| Fix All behaviour of guarded fixes | per provider `GetFixAllProvider` | see #1 |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| nine `FixAll…Reported` (RH3001, RH3004, RH3005, RH3101, RH3104, RH3105, RH3106, RH3107, RH3201) | Fix All fixes the offered instances and leaves the withheld one reported | a withheld instance fixed, or a fixable one left | `Verify` (single, iterative, Fix All) with `MarkupMode.Allow` on the fixed states | exact output, adequate for what it checks. No test combines instances that interact, which is how #1 stayed green (FC-008) |
| RH3004 layout tests (`…OnItsOwnLines…`, `DocumentationCommentedAnonymousMethodKeepsItsBody`, CRLF variants) | lambda header and braces at the anchor; one-line `///` kept | different column, or the `///` text rewritten | `Verify`, exact text | adequate for what they cover; no block holds a statement the formatter would rewrite, nor a multi-line `///` (#3) |
| RH3106-002/-003 tests (`AnonymousMethodPassedTo…`, `…NaturalType…`, `…ObjectOrCastToDelegate…`, `…ThroughAConditional…`) | report or fix per position | a natural-type position reported, or a binding-changing fix offered | `Verify` | exact, but every natural-type input is the direct parent and every binding input an argument (#2) |
| `RewriteParseUtilitiesTests` (14) | generic and cast readings, token merges, directive-bearing trees, unchanged tree | `true` for a misparse, or `false` for a safe rewrite | `Assert.IsTrue/IsFalse` on built rewrites | adequate; both sides of each reading (FC-003) |
| RH3103 position tests, C# 11/12 alias | not reported where `T?` is invalid; reported in type arguments | a report in a skipped position | `Verify` with language version | adequate |
| RH3104 statement-position tests (`for`, arrow bodies of each function kind, lambdas, `async Task/ValueTask/void`) | not reported where `default(S)` would be CS0201 | a report at a statement position, or a missed value position | `Verify` | adequate (FC-025 members covered) |
| RH5417 C# 10/11 interpolation tests | not reported inside `$"` holes below C# 11 | a report there, or a missed report at C# 11 | `Verify` with language version | adequate (FC-003) |
| RH3201-006/-007 tests | no merge; whitespace kept | `returna`, `a ++b` | `Verify`, exact | adequate |

### Failure classes dismissed

- FC-002: no qualifier of a requirement was left out of its predicate. "Directly the operand" (RH3004-005), "below C# 11/12" (RH5417/RH3103), and "not sealed" (RH3101-007) are explicit conditions.
- FC-003: both sides are tested for each new boundary (language versions, sealed and unsealed, comment inside and outside).
- FC-004/FC-005/FC-019/FC-020/FC-021: every changed fix withholds on comments and directives in the gap it drops (verified per guard above). The RH3004 restore keeps the doc-comment text; its column defect is #3.
- FC-007: no fix leaves its own diagnostic. Fix All convergence is #1.
- FC-009: every registered action changes the document. The guards run before registration.
- FC-010/FC-011/FC-012/FC-013/FC-014: no formatter phase changed. RH5417 now reports less, and its formatter counterpart is unchanged. A fix that formats is covered by #3.
- FC-015: see *Policy ownership*.
- FC-017: see *Assertion adequacy*. Expected outputs are exact.
- FC-018/FC-036: no rename.
- FC-022: interpolated and verbatim strings tested for RH5417. No fix of the run edits a literal.
- FC-023: identifiers pass through unchanged (`Name.WithTriviaFrom`); verbatim identifiers do not affect the re-parse.
- FC-024: partial and explicit interface members do not reach `base.`/`this.` removal differently; sealed containing types are checked through the symbol.
- FC-025: RH3104 covers every function kind, async included. No other fix changes expression bodies.
- FC-026: the replacements are node-based (`ReplaceNode`, tracked nodes). Several identical tokens on a line are re-resolved only in Fix All, which is #1.
- FC-028: no detached-node formatting.
- FC-029: no key/value layout.
- FC-031: the summaries of every changed method describe the new code (`BuildReplacement`, `CanRewrite`, `GetMirroredKinds`, `BuildSwappedExpression`, `KeepsTheTokensApart`, `StandsAsStatement`, `TakesNaturalType`, `ShouldSkip`, the base class). No stale comment found.
- FC-032: no project result from before a compiled change was relied on; every probe ran on this head.
- FC-033: CRLF covered for the layout members (RH3004-006, SHARED-PARENS-002). `apply-fix.sh` ran LF and CRLF.
- FC-034: the reproductions use the issues' own shapes (the `switch` form for #1257, Decision 8).
- FC-035: expected values come from the specs and the maintainer's answers, not from patched code.
- FC-037: no comment-only change claimed.

### Visible output is decided

Every changed expected value in a test of a visible surface is covered:
- RH3004 layout: maintainer answer 1.
- RH3106 not reported at natural-type positions: answers 2 and 4. Reported with the fix withheld under competing overloads: RH3106-002, Decision 9.
- RH3101 rule page: answer 3.
- RH3201-007: answer 5.
- RH3103, RH3104 and RH5417 positions that are no longer reported: SHARED-DIAG-011, unchanged.
- Withheld fixes (RH3001, RH3004-005, RH3005, RH3101-005/-006/-007, RH3104-006, RH3105, RH3201-006, SHARED-PARENS-003): SHARED-FIX-003, unchanged.
- RH3106/RH3107 whitespace: SHARED-FIX-007, unchanged (Decision 7).
- The RH3103 page sentence describes a report that SHARED-DIAG-011 removes, and a withheld-fix note does not belong on a rule page (`AGENTS.md`).

#3 changes a visible output (statements inside the block) that no test asserts and nothing decides. It is filed as a requirement violation of RH3004-003, not as an undecided expected value.

### Scope

Every changed path is named in handovers 01–07 or in a lead decision. `RH5417…Analyzer.cs` gains a BOM through the cleaner's formatting run, consistent with the other source files. No edit outside the run.

### Architecture findings

**RH3004's code fix and the formatter's node-level entry point** (`ReihitsuFormatter.FormatNodeInDocumentAsync`) share the decision of what in the anonymous method's block may change. RH3004-003 and SHARED-FIX-007's moved-code bullet say the statements stay as written and only their indentation follows. The entry point runs every phase (structural transforms, line breaks, switch-case braces, blank lines, spacing, documentation comments) on everything inside the node. Decision 12 fixed one member narrowly: the fix copies the documentation-comment phase's output back, so the fix now replays a decision of the formatter. #3 shows the other members of the same class. Narrow repairs would add one restore per phase, each a further copy. The structural options stay as Decision 12 named them: a layout-only mode of the node-level entry point, or a fix that formats only the code it creates and shifts the moved lines itself.

**Guarded fixes under BatchFixer** (#1): each guard decides one instance on the starting document, and BatchFixer merges the results without asking the guard again. The same decision, "is this instance's rewrite still safe", is made per instance by the provider and implicitly for the combination by BatchFixer, which knows no guard. This affects four providers of the run. A shared sequential Fix All path for guarded fixes would put the decision in one place.

### Hints

1. `RewriteParseUtilities.HaveSameTokensAndStructure` recurses once per tree level, with no stack guard. A very deep expression, such as a generated chain of several thousand `+` operands in a document that also carries a diagnostic of one of the five rules, could overflow the stack in the analyzer host. Roslyn guards its own recursive walks; an iterative walk, or a walk bounded per #4, avoids the risk. Not reproduced.
2. `RH3103UseShorthandForNullableTypesCodeFixProvider.cs:181`: the `MemberAccessExpressionSyntax { Name: genericName } => null` arm is no longer reached, because the analyzer now skips every member-access receiver. Decision 14 keeps it as a guard.
3. Pre-existing, and already a follow-up candidate (RH3201-005): when an empty comment starts its line and code follows it directly (`            /**/+1;`), the fix removes the indentation with the comment and leaves `+1;` in column 0. There is no token merge.
