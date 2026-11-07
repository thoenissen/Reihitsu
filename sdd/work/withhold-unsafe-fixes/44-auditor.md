Role: auditor
Commit: f1e3f033
Verdict: findings — #1257: pass; #1279: findings (#23, #24); #1281: pass; #1282: pass; #1417: pass; #1422: pass

My prompt named the run, the SHAs, the delta (`3108e585..f1e3f033`, rows 46–49), the handovers to read, and the checks to run. It carried no intended fix, suspected cause, or hoped-for verdict, so I had nothing to set aside.

## Scope

Head `f1e3f03350342e4a36b053d647bbb1f2414bb0c7`, checked out and clean. Base `origin/main` = `0fa1778c875793aaba22d10fef14747d4988f2ed`, fetched; it is an ancestor of head.

Repair delta against `3108e585`. It matches handovers 39–43 and rows 46–49:
- `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs`:
  - new `ContainsDirectiveOrDisabledText`, which calls `SyntaxTriviaUtilities.ContainsDirectives` over the anonymous method's span, on both paths;
  - new `ContainsStringOrCommentSpanningLines` (block path) with a private `SpansLines`;
  - eight helpers deleted;
  - `GetLineElements`, `AddTriviaElements`, `IsMovingLine` and `GetOpenBraceTail` simplified.
- Specs: RH3004-003, -006, -007, -008, -009; RH3106-003.
- Tests: 30 RH3004 tests flipped to withheld, 4 retagged, 18 new RH3004 tests, 12 new RH3106 tests.

Scope did not grow. I trusted the cleaner's build and the roles' focused test results.

Method:
- static tracing of the two new guards and the simplified line rule against RH3004-006/-007/-008/-009 and SHARED-FIX-003;
- `scripts/apply-fix.sh RH3004` and `RH3106` on fixtures in the session scratchpad (`audit7/`), LF and CRLF, on head and, for the new RH3004 fixtures, on a temporary worktree of `origin/main` (removed afterwards);
- a scratch Roslyn check (`audit7/sv`) that compiles each fixture with and without `A` and prints every string literal's value;
- a re-run of all 12 RH3004 fixtures of audit 6 (`audit6/rh3004*`).

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 20 | closed | (a)–(d) are withheld. Each has a test that reads the result: `FixWithheldForAMultiLineCommentThatHidesTheElseOfAChainAroundTheAnonymousMethod`, `…DocumentationComment…`, `…InsideTheBlock`, `FixWithheldForAStringWhoseDirectiveLineStartsWithAFormFeed`. The audit-6 fixtures `p1`, `p5`, `p6` and `p10` give `no-fix-offered` on head. Members #20 did not name still fail; see #23, which repeats #20. |
| 21 | closed | (a)–(d) are withheld by RH3004-009 and tested (`FixWithheldForAChainFromTheParameterListIntoTheBlock`, `…PastTheAnonymousMethod`, `…OnTheExpressionBodyPath`, `FixWithheldForARegionBeforeTheAnonymousMethodThatEndsInsideItsBlock`). The audit-6 fixtures `p3`, `p7`, `p7b`, `p7c`, `p8`, `p9` and `p9b` give `no-fix-offered`. With no directive left in the anonymous method, the reparse of `{ParameterList} => {body}` without symbols sees the same tokens as the document, so no member of this class remains. |
| 22 | closed | RH3106-003 now records the gap by outcome: MUST NOT report in the listed shapes, MAY report everywhere else, never with a fix. My probes find no output that contradicts it; see *thoenissen/Reihitsu#1281*. |

## thoenissen/Reihitsu#1257

Pass. The delta does not touch it.

## thoenissen/Reihitsu#1279

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 23 | high | `RH3004UseLambdaSyntaxCodeFixProvider.cs` `RegisterCodeFixesAsync`: `ContainsStringOrCommentSpanningLines` checks only the block, and only on the block path. Against it stand the spec texts: RH3004-008's Why ("An expression body moves no line …, so the hazard does not arise there"; "ends that class"), RH3004-007's Why ("an input the fix rewrites safely"), and SHARED-FIX-003, *Build configurations* | FC-016, FC-001, FC-022 | #20 | same requirement (SHARED-FIX-003 build configurations; RH3004-007/-008 as the delta words them). All members are pre-existing: `main` gives the same changed text | **Class.** A line whose `#` the current build hides inside a string or a delimited comment is a directive in another build. The fix's text then lies inside a string of that build, and any edit changes the string's value. Row 47 closed only the carrier "string or comment in the block, on the block path". Each fixture below compiles with and without `A`, is fixed under LF and CRLF, and changes a string's value without `A`; the values come from `audit7/sv`. **(a) Expression-body path, the carrier is the returned string** (`audit7/rh3004/e2_expression_body.cs`): `#if A⏎        Func<string> f = delegate () { return @"⏎#else⏎        Func<string> f = () =>⏎"; }; //";⏎#endif`. With `A`, the block is one `return` with no comment, so the fix writes the expression body. It deletes ` }` behind the closing quote: the last line becomes `"; //";`. Without `A`, that line is the string `"; }; //"`, which becomes `"; //"`. **(b) Parameter-list comment, block path** (`e3_param_comment.cs`): `Action<int> a = delegate (int x /*⏎#else⏎        var s = @"⏎        */)⏎        {⏎            Console.WriteLine(x);⏎        }; //";`. The comment lies in the parameter list, not in the block, so the guard does not see it. The fix writes `*/) =>` and moves the block. Without `A`, both edits are inside `s`. **(c) The same on the expression-body path** (`e3b_param_comment_expr.cs`): `*/) { return x; }; //";` becomes `*/) => x; //";`, and `s` changes. **(d) The carrier stands before the anonymous method** (`e1_comment_before.cs`): `#if A⏎        /*⏎#else⏎        var s = @"⏎        */⏎        Action a = delegate ()⏎            {⏎                Console.WriteLine(1);⏎            }; //";⏎#endif`. The anonymous method holds no directive and no multi-line string or comment. The fix rewrites it, and without `A` all of it is the text of `s`. Unlike (a)–(c), the hidden line lies outside the anonymous method. The same input breaks any code fix of any rule that edits code there, and whole-document formatting too (32-auditor hint 3). **Holding:** every audit-6 input; a multi-line raw or verbatim string returned on the expression-body path with no `#` line in it; a chain wholly outside the anonymous method with no hidden line; `return @"a⏎#b";` with no conditional directive in the document. That last one is safe here, but the same carrier in (a) is not. **Not reachable:** a multi-line string in the parameter list. An attribute on an anonymous-method parameter is CS7014, and a default value is CS1065. | For (a)–(c), carriers inside the anonymous method: withhold the fix on both paths when a string literal or delimited comment anywhere in the anonymous method, the parameter list included, spans lines and holds a line whose first character other than lexer whitespace (`SyntaxFacts.IsWhitespace`) is `#`. Alternatively, extend row 47's "any string or delimited comment that spans lines" to the parameter list on both paths, and add the `#`-line condition on the expression-body path. RH3004-007's offered raw-string cases stay offered either way. `FixRewritesASingleReturnOfAStringThatHoldsALineStartingWithHashAsAnExpressionBody`, RH3004-008's last example, becomes withheld; that is the lead's call under the standing instruction. Correct the Why sentences of RH3004-007 and -008. Tests: (a), (b), (c), no code fix offered. For (d): the hazard is not RH3004's alone, so record it as a known limit in RH3004-008. Also correct the claim "ends that class without knowledge of the directives around the anonymous method", and file a repository-wide follow-up (see *Architecture findings*). The other option is a document-wide guard: withhold when a string or delimited comment before the anonymous method holds such a `#` line. |
| 24 | medium | `RH3004UseLambdaSyntaxCodeFixProvider.cs:508` `SpansLines(SourceText, TextSpan)` | FC-015 | – | PR-introduced | The private helper decides "a span starts and ends on different lines". `Reihitsu.Core` already owns that decision: `SyntaxNodeUtilities.IsSingleLineSpan(SyntaxTree, TextSpan)` and `IsSingleLine(SyntaxNode)` (`src/Reihitsu.Core/SyntaxNodeUtilities.cs:189,205`), callable from a code fix. Handover 41 justified the private copy only against the two formatter helpers (`ListElementInteriorJoinRewriter.SpansLines`, `LineBreakDetection.IsMultiLine`) and missed the Core owner. The two agree today: both measure the start and end line of the unmapped span. | Replace the helper with `SyntaxNodeUtilities.IsSingleLineSpan(block.SyntaxTree, span) == false` and delete `SpansLines`. No test change. |

### Checked and holding

- **`ContainsDirectiveOrDisabledText`.** It inspects `anonymousMethodExpression.Span`, from `async`/`delegate` through `}`, with `descendIntoTrivia`. Every directive kind counts (`IsDirective`), inactive branches included. Disabled text always lies in a token's leading trivia, between two directives of the same list. Inside the span, that token belongs to the anonymous method, so the directive behind the disabled text is inside the span as well. The summary states this correctly. The leading trivia of the first token lies outside the span and is kept by `WithTriviaFrom`. RH3004-004 (`ContainsDiscardedTrivia`) still covers the gaps between the leading keywords, the list and the block.
- **`ContainsStringOrCommentSpanningLines`.** Strings: `IsString` covers regular, verbatim and raw literals (`StringLiteralExpression`), UTF-8 and UTF-8 raw literals (`Utf8StringLiteralExpression`), and every interpolated form (`InterpolatedStringExpressionSyntax`). A hole that spans lines makes the outer string span lines. Comments: `MultiLineCommentTrivia` and `MultiLineDocumentationCommentTrivia` inside `block.Span`. The trailing trivia of `{` is included. The leading trivia of `{` is not, but RH3004-004 withholds there. Parity with RH3004-008 item for item.
- **The simplified line rule.** For every input that is still fixed, it gives the same decision as at `3108e585`:
  - `IsCode → true` replaces `element.Span.Start >= line.Start`, and the two differ only for a code element that starts on an earlier line, which is now always a withheld string;
  - the `first.Span.Start < line.Start` branch is reached only by a `///` run, the one remaining trivia that spans lines, and it decides on the marker as before;
  - a `///` line that starts the trivia uses its full span start, the marker, which equals `SkipIndentation`;
  - the removed directive, disabled-text, string-continuation and comment-continuation branches had only inputs that are now withheld;
  - the multi-line-comment condition removed from `GetOpenBraceTail` covers trivia inside `block.Span`, which the new guard withholds.
- **Deleted code.** `rg` finds no remaining reference to any of the eight deleted helpers outside `sdd/work/`. Every remaining branch of `MoveStatementsAsync`, `GetFirstStatementLinePosition` (its second loop serves a block without statements) and `IsMovingLine` has a reachable input. Nothing deleted served an input that is still fixed.
- **Breaks the code in the analyzed build.** With no directive in the anonymous method, the fragment reparse cannot become unbalanced. The remaining rewrites are the guards of earlier rounds (RH3004-004, -005), which the delta does not change.

## thoenissen/Reihitsu#1281

Pass. No production change. I probed RH3106-003 as now written with 18 fixtures (`audit7/rh3106/`), LF and CRLF, each compiling:
- **MUST NOT report, not reported:**
  - `object o = new[] { delegate () { } };`
  - `new[] { … } is Action`
  - `var u = new[] { delegate () { }, delegate () { } };` for the second element after the first is fixed
  - `var k = new[] { … }; k[0]();`
  - `object o = new[] { … }[0];`
  - `[0] is Action a2`
  - `[0].DynamicInvoke()`
- **Elsewhere, not reported (allowed: MAY):** `foreach`, `Take(object)`, a tuple, a nested jagged array under `var`, `.ToString()`, a conditional with `null`.
- **Elsewhere, reported with no code fix:** `new List<Action>(new[] { … })`, `Action z = new[] { … }.First();`, `Func<Action[]> f = () => new[] { … };`.

No probe offered a fix inside the gap. The fix replaces the element with `delegate { }`, which has no natural type, and no other element gives the array a delegate type, so `BindsAsBefore` always fails there. "Never with a fix" therefore holds by construction. In `new[] { delegate () { }, delegate () { } }`, the other element gives the array `Action`, so it lies outside the gap: the first element is reported and fixed, as RH3106-003 (row 23) requires.

## thoenissen/Reihitsu#1282

Pass. The delta does not touch it.

## thoenissen/Reihitsu#1417

Pass. The delta does not touch it.

## thoenissen/Reihitsu#1422

Pass. The delta does not touch it.

## Whole run

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH3004-009 (`ContainsDirectiveOrDisabledText`) | every directive trivia whose start lies in `anonymousMethodExpression.Span`, both paths | what the fix reassembles and reparses: the parameter-list text, `=>`, the body text | identical: the reassembled text lies inside the span; disabled text implies a directive in the span | holds |
| RH3004-008 (`ContainsStringOrCommentSpanningLines`) | strings and delimited comments inside `block.Span`, block path only | RH3004-008's Why and SHARED-FIX-003 build configurations: every place in the anonymous method where a hidden `#` line makes the fix's text another build's string | narrower: the parameter list on both paths, and the returned expression on the expression-body path, are not inspected | fails (#23 a–c) |
| RH3004-008's Why: "ends that class without knowledge of the directives around" | — | a hidden `#` line before the anonymous method | the claim is wider than any guard on the anonymous method | fails (#23 d) |
| RH3004-006 line rule (`IsMovingLine`) | tokens and comments of the block; `///` runs as the only element that spans lines | RH3004-006's two outcomes | identical | holds |
| RH3106-003 gap | outcome-worded, MUST NOT shapes listed, MAY elsewhere, never a fix | `TakesNaturalType` / `BindsAsBefore` | the code meets the wording in every probe | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| A span holds a directive | `rg -n "ContainsDirectives\(" src`: `SyntaxTriviaUtilities.ContainsDirectives` (Core), reused by RH3004 and `ExpressionBodyRewriteUtilities`; the `MoveRangeContainsDirectives` helpers ask about a move range | one owner |
| A span spans lines | `rg -n "static bool \w*(MultiLine\|SpansLines\|SingleLine)\w*\(" src`: `SyntaxNodeUtilities.IsSingleLineSpan`/`IsSingleLine` (Core), RH3004's private `SpansLines`, and the formatter's `ListElementInteriorJoinRewriter.SpansLines` and `LineBreakDetection.IsMultiLine` | two owners reachable from code fixes (#24) |
| A node is a string | RH3004's private `IsString`, already at `3108e585`; RH1003's `IsStringExpression` asks a semantic question | unchanged by the delta |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 30 flipped RH3004 tests | reported as marked, no code fix, in the configured build (`DEBUG` where the input needs it) | a fix offered, or a missing or extra report | `VerifyNoCodeFixIsOffered` | adequate |
| 15 new RH3004 withheld tests (20 a–d, 21 a–d, spec examples, CRLF) | same | same | `VerifyNoCodeFixIsOffered` (the form feed is inserted by `Replace`, ordinal) | adequate. None covers a carrier in the parameter list or in a returned string (#23). |
| `FixRewritesABlockWhoseStringsAndCommentsEachStandOnOneLine` (+ CRLF), `FixRewritesAnAnonymousMethodInsideARegionThatLiesWhollyOutsideIt` | exact output | any other text, or no fix | `Verify`, `ApplyCodeFixAsync` (CRLF) | adequate. They are the other side of RH3004-008/-009 (FC-003). |
| `FixRewritesASingleReturnOfAStringThatHoldsALineStartingWithHashAsAnExpressionBody` | exact output | a different text | `Verify` | adequate for its input, which has no conditional directive. It pins the expression-body side that #23 (a) shows is unsafe once a chain is around it. |
| 12 new RH3106 tests | reported with no fix / not reported, per RH3106-003 as written | a fix offered, or a missing or extra report | `VerifyNoCodeFixIsOffered`, `Verify` | adequate |

### Failure classes dismissed

- FC-002: RH3004-009's "from its first token" equals `anonymousMethodExpression.Span`, and RH3004-008's "opening through closing brace" equals `block.Span`.
- FC-003: both sides are tested: a delimited comment over lines and on one line; a directive inside the anonymous method and wholly outside it; a conditional chain and a region; the block path and the expression-body path.
- FC-004/FC-005/FC-019/FC-020/FC-021: every directive and every disabled text in the anonymous method now withholds the fix. Comments in the gaps are RH3004-004, unchanged.
- FC-006: with no directive in the fragment, the reparse is faithful. Cast and binary operands are RH3004-005, unchanged.
- FC-007/FC-009: every probe that was fixed converged in one iteration. A withheld fix registers no action.
- FC-008/FC-011: `DocumentOrderFixAllProvider` is unchanged, and a withheld single fix is withheld under Fix All.
- FC-010, FC-012–FC-014, FC-028: no formatter code changed. The fix still formats only the lambda (FC-030).
- FC-017: see *Assertion adequacy*.
- FC-018/FC-023/FC-024/FC-025/FC-026/FC-029/FC-036: no rename, identifier handling, member kind, token re-resolution, or key/value layout changed.
- FC-027: the nesting axis holds. An outer anonymous method that holds an inner one with a directive is withheld; the inner one, without a directive, is fixed.
- FC-031: the summaries of `MoveStatementsAsync`, `GetFirstStatementLinePosition`, `GetLineElements`, `AddTriviaElements`, `IsCode`, `IsMovingLine`, `ContainsDirectiveOrDisabledText` and `ContainsStringOrCommentSpanningLines` describe the new code. No `TODO` and no tracker number.
- FC-032: every probe ran on a fresh build of `f1e3f033`, and the `main` comparison on a fresh build of `0fa1778c`.
- FC-033: every probe ran under LF and CRLF with the same outcome.
- FC-034/FC-035: the tests use the audit-6 inputs and the spec's values.
- FC-037: the delta makes no comment-only change to a `.cs` file.

### Visible output is decided

Every expected value the delta adds or changes is covered:
- the 30 RH3004 tests flipped to withheld and the 15 new withheld tests: row 47 (and row 46), decided under the maintainer's standing instruction, with SHARED-FIX-003 behind them;
- the two new offered RH3004 outputs: RH3004-006/-008/-009 examples, which the delta does not change in substance;
- the RH3106 tests: rows 48–49, unchanged outputs.

No rule page changed. #23's withheld fixes would be decided the same way as row 47. None is an undecided output.

### Scope

Every changed path is named in handovers 39–43 or rows 46–49.

### Architecture findings

**Repeated class, for the fourth time.** #23 repeats #20, which repeats #17 and #15 (and through them #7). Each round closed the hidden-`#`-line hazard at the carriers the previous audit named:
- first, strings in the block that start a line;
- then strings with a `#` line;
- then every string and comment over several lines in the block, on the block path.

Each time, the members were defined by where the carrier stands and which path runs. The decision underneath is not RH3004's. It is whether a span the fix edits is the same code in every build. A hidden `#` line decides that wherever it stands before or inside the edited span: in the block, in the parameter list, in the returned expression, or anywhere earlier in the document.

The components that share the decision:
- the RH3004 fix (both paths);
- every other code fix that edits code;
- whole-document formatting (32-auditor hint 3, which the lead listed as a follow-up candidate).

None of them owns it, and no Core helper answers it.

For RH3004, a narrowing on the whole anonymous method ends members (a)–(c): withhold when a string or delimited comment in the anonymous method holds a `#` line. Member (d) can only be ended by a document-level answer. That answer is a Core helper, "does a string or delimited comment before or inside this span hold a line whose first non-whitespace character is `#`", which every fix and the formatter could consult. Building it changes components this run does not own. So the lead decides between three options for (d):
- a recorded limit with a repository-wide follow-up;
- an RH3004-local document scan;
- the structural helper.

### Hints

1. Row 47 withholds the fix for a block with a column-0 conditional chain. Maintainer answer 7 had chosen a layout for exactly that block (option B). The standing instruction covers further questions; this reverses an answer already given. Row 47 says the PR states it. It should be stated as a reversal of answer 7, not only as a narrowing.
2. A `///` run in a moved block still keeps its old continuation columns (follow-up candidate, Decision 12). The delta does not change this.
3. #23 (d) also hits whole-document formatting and every other fix. The follow-up candidate "Formatter: whole-document formatting moves a line inside a verbatim string …" in `00-lead.md` is the formatter's instance of the same structure, and could be widened to it.
