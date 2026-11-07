Role: auditor
Commit: ee680a32
Verdict: findings — #1257: pass; #1279: findings (#17, #18); #1281: findings (#19); #1282: pass; #1417: pass; #1422: pass

My prompt named the run, the SHAs, the handovers to read, the delta (`13de50fe..ee680a32`, rows 40–43), and asked me to check the `IsInBranch` follow-up candidate of `00-lead.md`. It carried no intended fix, suspected cause, or hoped-for verdict, so I had nothing to set aside.

## Scope

Head `ee680a3296ce0090e1ac25882339228bcfd41c88`, checked out and clean. Base `origin/main` = `0fa1778c875793aaba22d10fef14747d4988f2ed` (`git ls-remote` agrees), an ancestor of head.

Repair delta against `13de50fe`. It matches handovers 27–31 and rows 40–43:
- `src/Reihitsu.Core/RewriteBindingUtilities.cs`: `CountErrors` descends only into path nodes and the expression's content (new `IsInside`); summaries.
- `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs`: a string is one line element (`GetOutermostString`, `IsString`, `AddTriviaElements`); RH3004-008 guard (`LeavesStringOpen`, `IsOpenStringToken`, `IsInBranch`, `ContainsLineStartingWithHash`).
- Specs RH3004 (-006 stays bullet, -007 Why, new -008, -003 list) and RH3106 (-003 `for` example); `requirement-ids.txt`.
- Tests in the RH3004 and RH3106 classes and in `RewriteBindingUtilitiesTests` (two summaries reworded).

Scope did not grow beyond the two components rows 40–41 name. I trusted the cleaner's build and the roles' focused test results.

Method: static tracing of every changed guard against its counterpart; `scripts/apply-fix.sh RH3004` and `RH3106` on fixtures in the session scratchpad (`audit5/`), LF and CRLF; the scratch Roslyn compile check (`audit4/cc`) on every fixture I cite, in every symbol configuration I cite; `scripts/format.sh` on copies of fixtures and fixed outputs.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 14 | closed | The `for` members #14 listed are tested and pass (method body, lambda body, implicitly typed array; condition and incrementor; holding: body-only use). `CountErrors` now skips every child operation that is neither a path node nor inside the expression, whatever its kind. A member #14 did not name still fails: see #19, which repeats #14. |
| 15 | closed | Every member #15 listed keeps its columns and is tested: `{name}` in column 0 (LF and CRLF), `";` in column 0, the shared brace line, and the same lines in disabled text. The string is one element from its first character through its closing quote, so a hole line and a closing-quote line start inside it. A member #15 did not name fails: see #17, which repeats #15. |
| 16 | closed | Both summaries now say the sibling statement lies outside the bound node. Comment-only. |

## thoenissen/Reihitsu#1257

Pass. Untouched by the delta.

## thoenissen/Reihitsu#1279

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 17 | high | `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs` `LeavesStringOpen`/`IsInBranch` (branch starts are collected from `block.DescendantTrivia()` only) | FC-001, FC-002, FC-022, FC-016 | #15 | new mechanism (RH3004-008, row 41/43) | This is the `IsInBranch` follow-up candidate. It is reachable with compiling code, and it breaks a string's value in another build. The guard sees only the conditional directives inside the block. A string of active code that lies in a branch opened before the anonymous method is never "in a branch". If that string holds a line that is a directive under other symbols, the other build's text after that directive is code there, and the fix moves those lines. **Probed (LF and CRLF; each input compiles with and without `A`):** (a) chain closing inside the block, `#endif` in the block: `Action a =⏎#if A⏎ delegate ()⏎ {⏎ var s = @"a⏎#else⏎ () =>⏎ {⏎ var s = @"⏎ ; // ";⏎#endif⏎ Console.WriteLine(s);⏎ };` with `#define A`. The fix is offered and moves `; // ";` from column 20 to 16. Without `A`, that line is inside the verbatim string `@"⏎…; // "`, so `s` loses four spaces at run time. (b) chain wholly outside the block, so the block holds no directive at all: `#if A⏎ Action a = delegate ()⏎ {⏎ var s = @"x⏎#else⏎ Action a = () => { var s = @"⏎ ; // ";⏎ Console.WriteLine(s); };⏎#endif`. The fix moves `; // ";` from column 20 to 27, so without `A` `s` gains seven spaces. SHARED-FIX-003 forbids both: a run-time difference in another build configuration, and a changed literal. RH3004-008's Why says the same ("its outcome must not depend on which preprocessor symbols are defined"). Its sentence says "an active `#if`, `#elif`, or `#else` branch inside the block", which this branch only partly is. `main` does not break this: its fix kept the block's text and moved no line. **Holding:** a branch whose `#if` is inside the block (`stringhash_inside.cs`: no fix offered); disabled text inside the block that leaves a string open (lexed whatever opened the branch); a string outside every branch that holds a `#` line (the implementer's probe). A string in a later branch of a chain opened outside cannot be active, because the anonymous method starts in the earlier branch. | Decide "the string lies in a conditional branch" on the document's whole directive chain, not on the block's directives. One way: count the depth from the conditional directives of the root that precede the string (`root.DescendantTrivia()` up to the string, or the directive chain from `GetFirstDirective`). The rest of the RH3004-008 check stays as it is. The spec author may state in RH3004-008 that the branch can open outside the anonymous method. Tests: (a) and (b), each with `A` defined, no code fix offered. |
| 18 | high | same file, `ApplyCodeFixAsync`: the lambda is written by `SyntaxFactory.ParseExpression($"… => {Block}")` and then formatted with `FormatNodeInDocumentAsync` | FC-020, FC-028 | – | same requirement (RH3004-006, maintainer answer 1; issue #1279 "lay out the result as the formatter does") | Found while checking #17's shape. The block holds a conditional directive whose partner lies outside the anonymous method. The fragment parse then holds an unmatched `#else`/`#endif`, the formatter does not lay out the lambda, and the opening brace stays on the `=>` line. **Probed, compiles in both configurations:** `Action a =⏎#if A⏎ delegate ()⏎ {⏎ Console.WriteLine(1);⏎#else⏎ () =>⏎ {⏎ Console.WriteLine(2);⏎#endif⏎ Console.WriteLine(3);⏎ };` with `A` gives `() => {` and the statements left at column 20. `#if A` (the lambda) / `#else` (the anonymous method) / `#endif` inside its block, without `A`, gives `() => {` with the statements shifted. #17's (a) also gives `() => {`. Whole-document formatting of each fixed output puts `{` on its own line, so the fix's output is not the formatter's (RH3004-006: "the braces of a block body each start their own line"; SHARED-FIX-007). The brace placement equals `main`'s output for these inputs, so nothing the formatter decided is lost; the requirement the run states is not met. **Holding:** a chain wholly outside the anonymous method (`balanced_outside.cs`, #17's (b)): the brace gets its own line. | Owner's choice. Build the replacement so its text never has to parse on its own, for example `SyntaxFactory.ParenthesizedLambdaExpression` with the original block node, or parse the whole changed document text. Or withhold the fix when the block holds a conditional directive whose partner lies outside the anonymous method, and record that gap in RH3004's spec. Tests: the two inputs above. |

### Checked and holding

- **String elements (row 41).** `GetOutermostString` walks to the block, so a string nested in a hole belongs to the outermost string. Trivia inside the string is not an element of its own; the leading trivia of its first token and the trailing trivia of its last token are. Strings in disabled text are single tokens from `ParseTokens`. `IsMovingLine` is unchanged and decides on the string elements. `GetFirstStatementLinePosition` can never measure on a line that a string from an earlier line covers. `GetMovingLineColumns` and the copy loop use the same `IsMovingLine`.
- **RH3004-008, block-internal side.**
  - Disabled text that leaves a string open is withheld (tests, without symbols and with `A`).
  - A string that disabled text closes before its directive keeps its lines (test, with and without `DEBUG`).
  - Nested chains: `depth` counts `#if`/`#endif`, and `#elif`/`#else` at depth > 0 stay in a branch.
  - `ContainsLineStartingWithHash` skips the string's first line, which cannot start inside the string.
- **Expression-body path:** untouched by RH3004-008, which checks only a block that stays a block.

## thoenissen/Reihitsu#1281

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 19 | high | `src/Reihitsu.Core/RewriteBindingUtilities.cs` `BindsAsBefore` (errors and symbols of every path node count), as `src/Reihitsu.Analyzer/Rules/Clarity/RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer.cs` `TakesNaturalType` uses it | FC-027, FC-016, FC-001 | #14 | same requirement (RH3106-003, maintainer answer 2) | Row 40 left out everything beside the path, but a node on the path can also only *use* the natural type rather than choose for it. Example: the element access and the delegate invocation above an element of an implicitly typed array. With the probe's `Func<int, int>`, the invocation fails on the path, so the anonymous method counts as not taking its natural type. It is reported, and the removal is withheld. RH3106-003 says not reported: no delegate type is its target, nothing chooses among candidates, and "an element of an implicitly typed array creation" is a listed place. **Probed, LF and CRLF, each compiles, all reported with no fix:** `var r = new[] { delegate () { return 1; } }[0]();`; the statement `new[] { delegate () { } }[0]();`; `new[] { delegate () { } }[0].Invoke();` (the `Invoke` symbol differs between `Action` and `Action<int>`). **Holding (not reported):** `.Length` on the array; `[0].Method`; `[0] is Action`; the `for` shapes of #14. **Decided by RH3106-002's inference wording, so holding as written:** `new[] { delegate () { } }.ToList()` and query clauses over the array (`from f in new[] { … } select f()`, `where f() > 0`) are reported with no fix, because type inference of `ToList<T>`, `Select`, and `Where` takes its type argument from the array. If the lead reads RH3106-003's "chooses among candidates for it" as the anonymous method alone, these fail too. **Not reachable:** invoking a conditional over the anonymous method directly (CS0149). | Stop the decision where the expression's value stops being *converted or chosen for*: a path node in which that value, through parentheses, conditional or switch branches, or an array element, is the receiver of a member access, the target of an element access, or the invoked delegate does not decide. Its errors and its symbol do not count, and neither do those of the nodes above it. Argument, operand, initializer, and element positions keep deciding through their symbols. The owner checks that the RH3106-002 tests (overloads, operators, `Add`, inference) still hold. Tests: the three probed inputs, not reported. |

### Checked and holding

- **`CountErrors` (row 40).** The walk descends from the bound node only into children whose syntax is a path node or lies inside the expression. Implicit operations keep the syntax of a path node or of the expression, so they are counted:
  - argument conversions (syntax: the expression);
  - the `params` array and the collection-initializer `Add` (syntax: the invocation or the element);
  - declarator and initializer operations (syntax: on the path).

  The other arm of a conditional or switch expression and the other elements of an array are off the path. Where their delegate type becomes the target, the error lands on the path node (the conditional, switch, or array type) or on the expression's own conversion. The row 21/23 tests pass on this head.
- **`IsInside`:** compares within one tree. The speculative root and the replaced expression share their tree.
- **Fix side (`KeepsMeaningAsync`):** it still requires the same converted type, so the narrower count cannot let a removal through that changes what code beside the path sees. Probed: `var f = () => { if (b) { return delegate () { }; } return s; };` is reported and fixed (the other `return` makes `Action` the target), and the result compiles.

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
| RH3106 probe error count (`CountErrors`) | path nodes and the expression's content | RH3106-003: the anonymous method's own conversion and the choices among candidates for it | wider: path nodes that only use the value (delegate invocation, element access, `Invoke`) count | fails (#19) |
| RH3004 line inside a string (`GetLineElements`/`IsMovingLine`) | the span of the outermost string literal or interpolated string expression; one lexed token in disabled text | RH3004-006 stays bullet | identical | holds |
| RH3004-008, disabled text (`IsOpenStringToken`) | each disabled-text trivia lexed alone; a string token that reaches its end with an error | RH3004-008 first sentence | agrees; also withholds an unterminated regular string on the last disabled line (Hint 1) | holds |
| RH3004-008, active branch (`IsInBranch`) | the conditional directives inside the block | RH3004-008 second sentence and Why: the branch the string lies in, wherever its directives stand | narrower: a branch opened before the anonymous method is not seen | fails (#17) |
| RH3004 block-body path gate | `GetExpressionBody == null` for both RH3004-007 and RH3004-008 | `ApplyCodeFixAsync` path split | identical | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Rebinding a rewrite or probe | `rg -n "TryGetSpeculativeSemanticModel\(" src` (production): only `RewriteBindingUtilities` | one owner |
| Moving or keeping an RH3004 line | `rg -n "IsMovingLine\|GetOutermostString\|KeepsColumns\|StaysInColumnZero" src`: `IsMovingLine` with its three callers, fed by `GetOutermostString` | one owner |
| Outermost string of the block | `GetOutermostString` (line elements) and the inline `Ancestors().TakeWhile(… != block).Any(IsString) == false` in `LeavesStringOpen` | two expressions in one class with the same semantics (both stop at the block and both use `IsString`); parity holds by reading, so not a finding |
| Lexing disabled text | `rg -n "ParseTokens" src`: RH3004 (three uses), `TokenSeparationUtilities`, `CasingCodeFixProviderBase` | the other two lex for other purposes; unchanged from `29-implementer.md` |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH3106, 5 `for` tests (incl. the spec example) | not reported; the holding body-only use not reported | a report at `()` | `Verify`, exact | adequate. No path-use shape (#19). |
| `RewriteBindingUtilitiesTests`, 3 new | the probe binds as before beside the `for` condition and incrementor; the removal still fails on the path | the result flips | direct call | adequate; the holding test pins both sides of row 40 |
| RH3004, 13 new layout tests | exact columns from RH3004-006 (string lines stay, hole lines stay, line where the string starts moves, shared brace line, disabled text with and without `DEBUG`, CRLF) | a different column | `Verify`, `ApplyCodeFixAsync` (CRLF, `DEBUG`), exact text | adequate |
| RH3004, 2 RH3004-008 tests | no code fix, without symbols and with `A` | a fix offered | `VerifyNoCodeFixIsOffered` (symbol `A`) | adequate for block-internal branches. No branch opened outside the anonymous method (#17). |

### Failure classes dismissed

- FC-003: the RH3004 string rule and the row-40 path rule are tested on both sides.
- FC-004/FC-005/FC-019/FC-021: comments and directives keep their own elements outside strings. Inside a string they are part of its text and stay with it.
- FC-006: the RH3106 fix decision is unchanged in kind (same converted type, same path symbols). No probe produced an `invalid-result`.
- FC-007/FC-009: every RH3004 and RH3106 probe converged in one iteration.
- FC-008/FC-011: no change to `DocumentOrderFixAllProvider`.
- FC-010, FC-012–FC-014: no formatter phase changed.
- FC-015: see *Policy ownership*.
- FC-017: see *Assertion adequacy*.
- FC-018/FC-023/FC-024/FC-025/FC-026/FC-029/FC-036: no rename, identifier, member-kind, token re-resolution, or key/value logic changed.
- FC-030: the RH3004 fix formats only the lambda.
- FC-031: the summaries of `BindsAsBefore`, `CountErrors`, `IsInside`, `GetFirstStatementLinePosition`, `GetLineElements`, `GetOutermostString`, `IsString`, `AddLineElements`, `AddTriviaElements`, `IsCode`, `IsMovingLine`, `LeavesStringOpen`, `IsOpenStringToken`, `IsInBranch`, and `ContainsLineStartingWithHash` describe the code. `IsInBranch` says "a conditional directive of the block", which is what it does; that is the defect of #17, not a stale comment. No `TODO` and no tracker number.
- FC-032: every probe ran on a fresh build of `ee680a32`.
- FC-033: every probe ran under LF and CRLF with the same outcome.
- FC-034/FC-035: the new tests use the audit's shapes and the spec's values.
- FC-037: the only comment-only change (row 42) is in test summaries.

### Visible output is decided

Every expected value the delta adds or changes is covered:
- RH3004-006 and -008: rows 41 and 43, decided on recommendation under the standing instruction (`27-spec-author.md`), and the spec text;
- RH3106-003: answer 2 and the unchanged first bullet.

The outputs the new findings need are already demanded:
- #17: no code fix, under SHARED-FIX-003 and RH3004-008's Why;
- #18: the RH3004-006 layout, or a withheld fix decided on recommendation;
- #19: not reported, under RH3106-003 and answer 2.

None is an undecided output.

### Scope

Every changed path is named in handovers 27–31 or rows 40–43. Nothing changed outside the run.

### Architecture findings

**Repeated classes.** #17 repeats #15 (and through it #7). #19 repeats #14 (and through it #10, #5, and #2). The lead decides both under `squad/README.md`, *Architecture findings* and *Repairs*. `00-lead.md` says a further block is narrowed or withheld.

- **#17: the RH3004 moved-code path against literal values (SHARED-FIX-003).** Each round decided "this line belongs to a string, in some build" on a smaller unit than the one that decides it:
  - raw-string shifting copied from the formatter (#7);
  - single tokens (#15);
  - now the block's own directives, where the branch is set by the document's whole conditional chain.

  A narrowing that ends the class: withhold the block-body fix whenever an active string of the block holding a line that starts with `#` lies in any conditional branch of the document, or whenever the block holds a string or disabled text and the anonymous method lies in any conditional branch. Either uses the document's directives, not the block's.
- **#19: the RH3106 analyzer against RH3106-003, through `RewriteBindingUtilities`.** The probe decides on everything the probe changes between the anonymous method and the binding root. Each round narrowed which nodes count:
  - positions (#2);
  - sibling statements (#10);
  - non-statement children beside the path (#14);
  - now nodes on the path that only use the value.

  RH3106-003 defines the outcome as "a delegate type is its target, or something chooses among candidates for it". The path holds both nodes that choose for the value (argument, operand, element, initializer under overloads, operators, `Add`, inference) and nodes that consume it (receiver, element-access target, invoked delegate). Error counting cannot tell the two apart. A decision that ends the path at the first consuming node, and judges the choosing nodes by their symbols, would follow the spec's definition. Narrowing: keep the report for what RH3106-002 covers, and do not report where the only probe difference lies at or above a consuming node.
- **#18** is not a repeated class. Its structural cause is that the fix reparses a text fragment of the block, so a directive whose partner lies outside the fragment makes the formatter refuse the node. It is the RH3004 fix's own construction, not a second owner of a decision.

### Hints

1. `IsOpenStringToken` also withholds when the last line of a disabled branch holds an unterminated *regular* string (`Console.WriteLine("abc` in front of `#endif`). That text does not compile in the build that enables it, so the guard gives away no ground any compiling build reaches.
2. #17's and #18's fixtures place the lambda at column 12 under `Action a =⏎#if A`, while whole-document formatting puts it at column 8. That is the node-level entry keeping the target's first column (FMT-PIPE-017), unchanged by the run and not a finding.
3. The formatter's whole-document pass shows #17's value break as well: formatting (a) with `#define A` moves `; // ";` from column 20 to 12. It is a pre-existing formatter defect for code that is a string in one build and code in another, outside this run. It is a possible follow-up, filed only if the maintainer wants that class handled in the formatter.
4. Hints of `26-auditor.md` are unchanged by the delta.
