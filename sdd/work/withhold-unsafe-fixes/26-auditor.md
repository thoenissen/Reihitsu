Role: auditor
Commit: 13de50fe
Verdict: findings — #1257: pass; #1279: findings (#15); #1281: findings (#14, #16); #1282: pass; #1417: pass; #1422: pass

My prompt named the run, the SHAs, the handovers to read, and the scope (repair round 3, the last, and the architecture changes of rows 36 and 39). It carried no intended fix, suspected cause, or hoped-for verdict, so I had nothing to set aside.

## Scope

Head `13de50fe8809a25141f1b7fc4894b1ab48611aef`, checked out and clean. Base `origin/main` = `0fa1778c875793aaba22d10fef14747d4988f2ed`, freshly fetched and an ancestor of head.

Repair delta `c1db67e2..13de50fe`. It matches handovers 21–25 and rows 36–39:
- spec RH3004 (-006 rewritten as one stays/moves rule, -007 narrowed to the block body);
- `src/Reihitsu.Core/RewriteBindingUtilities.cs` (`GetBindingRoot`, `CountErrors` on the path);
- `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs` (`GetExpressionBody`, `IsMovingLine` and the line elements replace `StaysInColumnZero`, `KeepsColumns`, and `IsDisabledStatementLine`);
- tests in the RH3004 and RH3106 classes and in `RewriteBindingUtilitiesTests`.

**The scope grew** within the two components: row 39 rewrote RH3004-006 and replaced the RH3004 line rule, and row 36 changed what `BindsAsBefore` counts. I audited both changed decisions in full. For everything else I reused the evidence of `20-auditor.md`, which is byte-identical outside these files.

I trusted the cleaner's build and the roles' focused test results. Method:
- static tracing of every changed guard against its counterpart;
- `scripts/apply-fix.sh RH3106` and `RH3004` on fixtures in the session scratchpad (`audit4/`), LF and CRLF;
- a scratch Roslyn compile check (`audit4/cc`) that confirms every fixture below compiles without errors;
- `scripts/format.sh` on a copy of one fixed output.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 10 | closed | Every member #10 listed is now not reported, as the 11 new RH3106 tests assert: `if`, `using`, and `foreach` blocks, a lambda under `var` in an `if`, a lambda body, an anonymous-method body, a local-function body, a top-level `if`, and async, iterator, and `goto` methods. The holding members (unused local, `object` with a cast) are tested. A member #10 did not name still fails: see #14, which repeats #10. |
| 11 | closed | `GetBindingRoot` now binds the innermost statement, or initializer outside a statement, around the outermost enclosing anonymous function (traced). The implementer's timings (`23-implementer.md`) show the same cost for the wrapped and the unwrapped shape. No test pins the root's extent (Hint 2). |
| 12 | closed | `GetExpressionBody` is the one decision on the path, used by both registration and `ApplyCodeFixAsync`. The guard runs only when the block stays a block. Probed: a raw string nested in a call (`return Foo("""…""");`) and an interpolated raw string as a lambda argument are fixed, the values are unchanged, and a formatter pass leaves the raw-string lines alone. |
| 13 | closed | The measured line is the first line on which a code element starts. `IsMovingLine` returns "moves" for any line whose first overlapping code element starts on it. So the measured line always moves, and no line is both measured and kept. Probed: `/* note */ Console.WriteLine(x);` in column 0 gives 28/40, and an indented `#if` moves with the statements. |

## thoenissen/Reihitsu#1257

Pass. Untouched by the repair.

## thoenissen/Reihitsu#1279

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 15 | high | `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs` `IsMovingLine` (a line stays only when its first overlapping code element is a *token* that began on an earlier line); before round 3 the same in `KeepsColumns` | FC-022, FC-004 (literal changed), FC-016 | #7 | PR-introduced (moved-code path, round 1; carried into row 39) | An interpolated verbatim string (`$@"…"`, `@$"…"`) is several tokens. Its text token ends right before a hole's `{` or before the closing `"`. A continuation line whose first character in column 0 is such a `{` or `"` starts with a token that begins on that line. The line therefore "holds code" and moves, and the spaces the fix puts in front of it become part of the string's text. **Probed, LF and CRLF.** (a) Own-line block: `var s = $@"Hello⏎{name}⏎done";`. Output: `{name}` at column 19, so the value gains 19 spaces. (b) Closing quote: `var s = $@"Hello {name}⏎";`. Output: `";` at column 19, so the value ends in 19 spaces. (c) Shared brace line: `delegate (string name) { var s = $@"Hello⏎{name}"; … };`. Output: `{name}"` at column 31. **Traced:** the same lines in disabled text (`ParseTokens` splits them the same way), and any shift: a moving line in column 0 forces the leftmost-line rule, so the shift is always to the right. On `main` the fix let the formatter lay out the block, and the formatter never changes a literal. RH3004-006's stays list says "a line that starts inside a verbatim string literal begun on an earlier line … so the literal's value stays as written (SHARED-FIX-003)". **Holding:** a plain verbatim string with `"` in column 0 (one token, probed); a continuation line that starts with string text or with whitespace (the text token covers it); the expression-body path (the formatter lays it out). Also against the spec's wording, with no value change: a line that starts inside a hole begun on an earlier line moves (the implementer noted this in `23-implementer.md`). | Decide "starts inside a string literal begun on an earlier line" on the literal, not on its tokens. Use the span of the verbatim or interpolated verbatim string expression, which runs from the start token to the end token, also when the literal is split into the tokens of disabled text. That one decision also covers hole lines, as the spec words them. Tests: `{name}` in column 0, `";` in column 0, and the shared-brace form, one of them under CRLF. Each asserts that the line keeps its columns. |

### Checked and holding

- RH3004-007 narrowed (row 38): the expression-body path is offered for a single `return` of a multi-line raw string, also nested in a call and as an interpolated raw string argument. The block-body path still withholds. The four existing withheld tests are unchanged.
- One rule per line (row 39):
  - a directive's element starts at `#`, so an indented `#if` moves and a column-0 `#if` stays (probed);
  - `#region` moves in any column;
  - a code-free `///` line is decided on its own text;
  - a code-free continuation of a comment follows the line on which the comment starts, through the recursion, also when that line is the opening brace line (`{` is code there, so it moves);
  - the leftmost-line lists (`GetMovingLineColumns`) and the copy loop use the same `IsMovingLine`.
- The disabled-text column-0 comment in front of code moves to 28/40 (the implementer's probe; traced on the same branch).

## thoenissen/Reihitsu#1281

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 14 | high | `src/Reihitsu.Core/RewriteBindingUtilities.cs` `CountErrors` (it leaves out only a child operation whose syntax is a `StatementSyntax` off the path), as `src/Reihitsu.Analyzer/Rules/Clarity/RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer.cs` `TakesNaturalType` uses it | FC-001, FC-027, FC-016, FC-031 | #10 | PR-introduced (row 26 mechanism, row 36 scope) | Row 36 decides on the path, but the predicate skips only *statements* beside the path. A `for` statement declares its implicitly typed local in the initializer and uses it in the condition and the incrementors. Those are expressions under the binding root, off the path, and still counted. With the probe's `Action<int>`, such a use becomes an error, so the anonymous method counts as not taking its natural type. It is reported with no fix (the removal is withheld by the error on the path). RH3106-003, first bullet, and maintainer answer 2 say it is not reported. **Probed, LF and CRLF, each compiles:** `for (var a = delegate () { }; a != null; a()) { }` in a method body; the same inside a lambda body; `for (var arr = new[] { delegate () { } }; arr.Length > 0; arr[0]()) { }`. **Holding:** the same `for` with the use only in its body (a statement, skipped; probed); every statement-sibling shape of #10 (tests). **Not reachable:** a switch expression or an `is` pattern over the anonymous method itself (CS8117). Comments: `BindsAsBefore` says "the path from the expression to that node has no error the original path did not have", and `CountErrors` begins "Counts the errors … on the path". Both overstate what is counted, because every non-statement child of a path node counts. | Make the decision what the summary says: leave out every child operation whose syntax is neither on the path nor inside the replaced expression, not only statements. Bound symbols on the path still decide every candidate choice (overloads, operators, `Add`, inferred type arguments, including their display with type arguments). The owner checks that the RH3106-002 tests and the Fix All interacting-instance tests hold. The narrower alternative, leaving out a `for` statement's condition and incrementors as well, keeps a syntax-kind list instead of the path. Tests: the `for` initializer with a use in the incrementor, in a method body and in a lambda body, not reported. |
| 16 | low | `src/Reihitsu.Core.Test/RewriteBindingUtilitiesTests.cs` `BindsAsBeforeReturnsTrueForProbeWhoseNaturalTypeBreaksSiblingStatementInBlock`, `BindsAsBeforeReturnsTrueForProbeReturnedByLambdaWhoseResultBreaksSiblingStatement` (summaries) | FC-031 | – | PR-introduced (row 36/37 tests) | The summaries say the sibling statement's error does not count "because the sibling statement is not on the path from the expression to its binding root". Under the new root, the declaration statement inside the `if` block is the binding root itself, so `a();` and `g()();` are not bound at all. These two tests pass by the root's extent, not by the path rule. The lambda-body test is the one that exercises the path rule. | Reword both summaries: the sibling statement lies outside the bound node. Or move the declaration into a lambda body if the test is meant to pin the path rule. |

### Checked and holding

- `GetBindingRoot`:
  - an `EqualsValueClause` or `ArrowExpressionClause` counts as the root only outside a statement, so a local's initializer is bound with its declaration statement and a local function's expression body with the local function;
  - field, property, and accessor initializers and bodies, constructor initializers, and primary-constructor base types are found before the member stops the search;
  - an expression in a local function with a parameter (`void L(int p) { Run(delegate () { Console.WriteLine(p); }); }`) binds speculatively and is fixed (probed).
- `Run(() => { var a = delegate () { }; return a; })` with `Run(Func<Action>)` and `Run(Func<Action<int>>)`: reported, no fix (probed). The `Run` overload on the path changes with the probe, which is RH3106-002's competing-overload case.
- The fix side (`KeepsMeaningAsync`): it also requires the same converted type, so a statement beside the path cannot bind differently after a removal. The row-36 scope change does not let a fix through that breaks code (traced on returns beside the path in an inferred-return lambda).

## thoenissen/Reihitsu#1282

Pass. Untouched by the repair.

## thoenissen/Reihitsu#1417

Pass. Untouched by the repair.

## thoenissen/Reihitsu#1422

Pass. Untouched by the repair.

## Whole run

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH3106 probe error count (`CountErrors`) | every operation under the binding root except statements off the path | RH3106-003: the anonymous method's own conversion and the candidate choices on its path | wider: non-statement children off the path (a `for` condition and incrementors) count | fails (#14) |
| Binding root (`GetBindingRoot`) | the innermost non-block statement, or initializer, expression body, or constructor initializer outside a statement, around the outermost enclosing anonymous function | the code that can decide the expression's binding | agrees | holds |
| RH3004 block-body guard (`GetExpressionBody == null && ContainsMultiLineRawStringLiteral`) | the block, only when it stays a block | the `ApplyCodeFixAsync` path split (the same `GetExpressionBody`) | identical | holds |
| RH3004 "starts inside a literal begun earlier" (`IsMovingLine`) | the token spans of the block and of its disabled text | RH3004-006 stays list and SHARED-FIX-003: the span of the verbatim string literal | narrower: an interpolated verbatim literal's token boundaries at `{` and `"` | fails (#15) |
| RH3004 measured line vs moving lines | the first line on which a code element starts; `IsMovingLine` | RH3004-006: the first statement line always moves | identical by construction | holds |
| RH3004 column-0 directive | the directive element from `#` | RH3004-006 stays list | identical (indented `#if` probed) | holds |
| RH3004 comment continuation | recursion to the line on which the comment starts | RH3004-006 stays/moves lists | identical; a continuation line that holds code moves, as the spec says (Hint 1) | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Rebinding a rewrite or a probe | `rg -n "TryGetSpeculativeSemanticModel\(" src` (production): only `RewriteBindingUtilities` | one owner |
| Expression body or block body in RH3004 | `GetExpressionBody` (registration and `ApplyCodeFixAsync`) | one owner; the private inline copy is gone |
| Moving or keeping a line of RH3004's moved code | `rg -n "StaysInColumnZero\|KeepsColumns\|IsDisabledStatementLine\|IsMovingLine" src`: `IsMovingLine` and its three callers | one owner; the per-kind predicates are gone |
| Raw-string lines moving with their code | formatter `RawStringLineShifts`; RH3004 only checks for presence on the block-body path | unchanged from `20-auditor.md` |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH3106, 14 new tests | not reported under `var` with a later use in each enclosing shape; reported and fixed with a delegate target | a report at `()`, or no fix | `Verify`, exact text | adequate for the listed shapes. No `for` shape (#14). |
| `RewriteBindingUtilitiesTests`, 4 new tests | the probe binds as before beside a sibling statement; the removal still fails on the path | `BindsAsBefore` result flips | direct call | the lambda-body test pins the path rule. The two `if`-block tests pass by the root's extent and their summaries say otherwise (#16). No test pins the root's extent (Hint 2). |
| RH3004 `FixRewritesASingleReturnOfAMultiLineRawStringLiteralAsAnExpressionBody` | the expression-body path is offered and keeps the value | no fix, or other columns | `Verify`, exact text | adequate |
| RH3004, 5 new layout tests (column-0 comment before code, LF and CRLF; block comments starting in column 0 and in another column, each alone and together) | exact columns from RH3004-006 | a different column | `Verify`, `ApplyCodeFixAsync` (CRLF), exact text | adequate. No interpolated verbatim line in column 0 (#15). |

### Failure classes dismissed

- FC-002/FC-003: every changed RH3004 outcome has a test on both sides (code-holding vs code-free, column 0 vs another column). The RH3106 path rule has a holding and a failing side.
- FC-005/FC-019/FC-020/FC-021: directives and comments are elements of their own in `IsMovingLine`. The moved text is copied, and only indentation changes, apart from #15.
- FC-006: the RH3106 fix decision is unchanged in kind. No fixed output in my probes failed to compile (`apply-fix.sh`: no `invalid-result`).
- FC-007/FC-009: every probe converged; the one fixture with two diagnostics (the expression-body raw strings) took two iterations, one per diagnostic.
- FC-008/FC-011: no change to `DocumentOrderFixAllProvider`.
- FC-010, FC-012–FC-014: no formatter phase changed.
- FC-015: see *Policy ownership*.
- FC-017: see *Assertion adequacy*.
- FC-018/FC-036: no rename.
- FC-023/FC-024/FC-025/FC-029: no identifier, member-kind, or key/value logic changed.
- FC-026: `IsMovingLine` decides on spans, not on re-resolved text.
- FC-028/FC-030: the RH3004 fix still formats only the lambda.
- FC-031: the summaries of `GetBindingRoot`, `GetExpressionBody`, `MoveStatementsAsync`, `GetFirstStatementLinePosition`, `GetLineElements`, `AddLineElements`, `IsCode`, `GetMovingLineColumns`, and `ContainsMultiLineRawStringLiteral` describe the new code. The exceptions are `BindsAsBefore`/`CountErrors` (#14) and two test summaries (#16). `IsMovingLine`'s "starts inside a string literal" is the defect of #15, not a stale comment. No `TODO` and no tracker number.
- FC-032: every probe ran on a fresh build of `13de50fe`.
- FC-033: the RH3004 and RH3106 probes ran under LF and CRLF with the same outcome.
- FC-034/FC-035: the tests use the audit's shapes and the spec's values.
- FC-037: no comment-only change claimed.

### Visible output is decided

Every expected value the repair adds or changes is covered:
- RH3004-006 and -007: rows 38 and 39, decided on recommendation under the standing instruction (`21-spec-author.md`, items 1–3), and the spec text;
- RH3106: RH3106-003, first bullet, unchanged, with maintainer answer 2.

The outputs #14 and #15 need are already demanded:
- #14: RH3106-003 and answer 2, not reported;
- #15: RH3004-006's stays list and SHARED-FIX-003, the literal keeps its value.

Neither is an undecided output.

### Scope

Every changed path is named in handovers 21–25 or in rows 36–39. Nothing changed outside the run.

### Architecture findings

**Repeated classes.** #14 repeats #10 (and through it #5 and #2), and #15 repeats #7. This was the last repair round, so the lead decides both under `squad/README.md`, *Architecture findings* and *Repairs*.

- **#14.** The RH3106 analyzer against RH3106-003, through `RewriteBindingUtilities`. Row 36 put the decision on "the path", but the implementation keys the exclusion on a syntax kind (statements). Every round has found code that depends on the natural type but is not part of the decision:
  - round 1: positions;
  - round 2: sibling statements;
  - now: the non-statement parts of a `for` header.

  A rule that counts only the path and the replaced expression would close the class by construction. Candidate choices are already decided by the bound symbols on the path.
- **#15.** The RH3004 moved-code path against literal values (SHARED-FIX-003):
  - #7: raw strings (now withheld);
  - now: interpolated verbatim strings.

  Both times the component decided "this line is part of a literal" on a smaller unit than the literal: line shifting copied from the formatter, then single tokens. Deciding on the literal expression's span, for active and disabled text alike, is one decision that covers every literal kind. Together with the follow-up candidate for a shared "move lines keeping literal values" helper, it is the structure that would let RH3004 stop special-casing literals.

### Hints

1. RH3004, a multi-line comment that starts in column 0 and whose continuation line holds code (`/* a⏎   b */ Console.WriteLine(x);`). The first line stays in column 0 and the continuation moves with the statement, which splits one comment across columns (probed). RH3004-006 decides this as written: a line that holds code moves. Its Why ("together with the rest of its comment") does not hold for this shape. Not a finding.
2. No test pins the binding root's extent (row 37). Reverting `GetBindingRoot` to the outermost statement would keep every test green, because the path rule covers the sibling statements. `20-auditor.md` left this to the owner, and the test implementer declined with reasons. Noted for FC-017.
3. Correction to `20-auditor.md`: `var t = (delegate () { }, 1);` (CS0815) and `new { A = delegate () { } }` (CS0828) do not compile, as `22-test-implementer.md` found. I listed them as compiling holding inputs. They are not inputs of the rule.
4. Hints 1–3 of `20-auditor.md` are unchanged by the repair.
