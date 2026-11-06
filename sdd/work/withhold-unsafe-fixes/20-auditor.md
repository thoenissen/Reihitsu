Role: auditor
Commit: c1db67e2
Verdict: findings — #1257: pass; #1279: findings (#12, #13); #1281: findings (#10, #11); #1282: pass; #1417: pass; #1422: pass

My prompt named the run, the SHAs, the handovers to read, and the scope (repair round 2 and the architecture changes of rows 26–28). It carried no intended fix, suspected cause, or hoped-for verdict, so I had nothing to set aside.

## Scope

Head `c1db67e24ebac5d9329a9f4d03afc097d08614fa` (checked out, clean). Base `origin/main` = `0fa1778c875793aaba22d10fef14747d4988f2ed`, equal to `git ls-remote origin refs/heads/main`, and an ancestor of head. Repair delta `2284decb..c1db67e2`. It matches handovers 15–19 and rows 26–35:
- specs RH3004 (-003, -006, new -007), RH3106 (-002, -003), SHARED-FIX-002, `requirement-ids.txt`;
- the new public `Reihitsu.Core.RewriteBindingUtilities`, the rewritten RH3106 analyzer (`TakesNaturalType` with a probe), the RH3106 fix (`KeepsMeaningAsync` as one call);
- `DocumentOrderFixAllProvider` with re-analysis, now also for RH3004, and the analyzer parameter of the empty-list base;
- RH3004 `MoveStatementsAsync` (leftmost-line rules, `StaysInColumnZero`, `KeepsColumns`) and `ContainsMultiLineRawStringLiteral`;
- tests in the RH3004, RH3005, and RH3106 classes, and the new `RewriteBindingUtilitiesTests`.

**The scope grew.** The repair changed specs and replaced the RH3106 decision with a new mechanism (rows 26–28). So I audited every changed requirement and every changed source file in full. For files the repair left byte-identical (RH3001, RH3103, RH3104, RH3107, RH3201, RH5417, the shared comment guards, `RewriteParseUtilities`), I reused the evidence of `08-auditor.md` and `14-auditor.md`.

I trusted the cleaner's build and the roles' focused test results. Method:
- static tracing of every changed guard against its counterpart;
- `scripts/apply-fix.sh RH3106` and `RH3004` on fixtures in the session scratchpad, LF and CRLF, on this head and on a temporary worktree at `2284decb` (removed afterwards);
- a scratch file-based app outside the repository that references `src/Reihitsu.Tooling` and times the RH3106 analyzer through `CompilationWithAnalyzers`.

Every input below compiles without errors.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 5 | closed | Every member #5 listed holds, as the new tests assert: (a) static `+=` with two operators, (b) C# 14 instance `operator +=`, (c) collection initializer with two `Add` methods are reported with no fix; (d) `Coll e = [delegate () { }]` and (e) both anonymous-method returns under `var` are not reported; (f) `var f = Action () => delegate () { };`, the block form, and `object o = Action () => …` are reported and fixed. A member #5 did not name fails in the new mechanism: see #10, which repeats #5. |
| 6 | closed | RH3004 is on `DocumentOrderFixAllProvider`. Nested instances are found again by re-analysis after a pass that lost one. The SHARED-FIX-002 example and the curried base scenario pass in one Fix All iteration. RH3005 `1 < (1 < x ? 2 : 3)` is fixed in one application. Three nesting levels trace to three passes, within the bound of three starting diagnostics. |
| 7 | closed | The private raw-string shifting is gone. A block with a multi-line raw string (plain, `$"""`, `"""u8`, also in disabled text) withholds the fix. The guard now covers more than the hazard: see #12 (not a repeat: #7 was a value change in moved code, #12 is a guard that refuses a path the formatter handles). |
| 8 | closed | Both listed inputs (`Run(delegate () { Foo();⏎…` and the `Action<int> c` assignment form) and the row 31/33 shapes pass under exact-text tests, LF and CRLF for the first. A member #8 did not name fails: see #13, which repeats #8. |
| 9 | closed | (a) `object o = b ? delegate () { } : someAction;` is reported and fixed (`AnonymousMethodInAConditionalWithADelegateTypedOtherBranchUnderObjectIsReportedAndFixed`). (b) The disabled-before-active example is tested with and without `DEBUG`. |

## thoenissen/Reihitsu#1257

Pass. The repair changed only `GetFixAllProvider` of RH3005, RH3101, and RH3105 (`DocumentOrderFixAllProvider.Create(new <analyzer>())`). The nested RH3005 test and the holding equality variant pin the new pass. The interacting-instance tests of round 1 pass unchanged.

## thoenissen/Reihitsu#1279

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 12 | medium | `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs` `RegisterCodeFixesAsync` (`ContainsMultiLineRawStringLiteral` is checked before the expression-body and block-body paths split); `sdd/specs/analyzer/RH3004.md` RH3004-007 | FC-001 (guard wider than the decision it protects) | – | PR-introduced (repair, row 28) | Row 28 withholds the fix because the moved-code path would need a private copy of the formatter's raw-string line shifting. The expression-body path never moves code itself: `FormatNodeInDocumentAsync` lays out the whole lambda, and the formatter moves raw-string lines with their closing quotes. The guard is checked at registration, so it also refuses that path. Input: `Func<string> f = delegate ()⏎{⏎    return """⏎        a⏎          b⏎        """;⏎};`. At `2284decb` the fix wrote `Func<string> f = () => """⏎<31>a⏎<31>  b⏎<31>""";`: same value `a\n  b`, laid out by the formatter (probed). Now no fix is offered, and `FixWithheldForASingleReturnOfAMultiLineRawStringLiteral` pins the refusal. AGENTS.md *Code-fix coverage*: a guard that refuses inputs the fix can rewrite safely is worth narrowing. Holding: the block-body path withholds as row 28 intends (plain, `$"""`, `"""u8`, whitespace-only line, disabled text). | Spec author: limit RH3004-007 to the block body the fix moves as code (a block that does not become an expression body). Implementer: check the guard only on that path. Test implementer: turn the single-`return` test into a fix test whose expected value comes from the spec (the raw string keeps its value, laid out by the formatter). |
| 13 | medium | same file, `MoveStatementsAsync` / `GetFirstStatementLinePosition` (the delta is measured on the first statement line even when `StaysInColumnZero` keeps that line in column 0) | FC-002, FC-027 | #8 | PR-introduced (repair, row 33) | RH3004-006 measures the shift on the first statement line. Row 33 keeps a line in column 0 when its first character is a comment. A statement line can start with a comment in column 0, so the measured line itself does not move. Input: `Action<int> b = delegate (int x)⏎        {⏎/* note */ Console.WriteLine(x);⏎            Console.WriteLine(x + 1);⏎        };` gives `Action<int> b = (int x) =>⏎<24>{⏎/* note */ Console.WriteLine(x);⏎<40>Console.WriteLine(x + 1);⏎<24>};` (probed, LF and CRLF). The delta is 28 − 0, taken from a line that stays at 0. The first statement stands left of the braces and the second 12 columns right of the statement column. At `2284decb` both lines moved together (28 and 40). Row 33 made the same argument for a block without statements ("would be measured on a line that does not move"), but did not apply it to a statement line. Its Why says a column-0 comment "does not push the statements to the right", and here it does. Failing members (traced, same branch): an active statement line that starts with `/* … */` in column 0 (probed); a disabled-text statement line that starts with `/*` in column 0 (`IsDisabledStatementLine` counts it, `StaysInColumnZero` keeps it); both whether the first statement shares the brace line or not (the shared-brace branch cannot reach column 0, so only the own-line branch). Holding: `//` and `///` lines (they hold no statement); a column-0 comment line in front of the first statement (row 33 tests). | Spec author decides on recommendation, under the standing instruction, whether a line that holds a statement stays in column 0. The simplest option, keeping one rule: the column-0 comment rule applies only to lines that hold no code, so the statement line moves and is measured as before. The alternative is to measure on the first character behind the comment. Then a test for the shape. |

### Checked and holding

- Leftmost-line rules (rows 29, 31):
  - shared brace line with shallower later lines, LF and CRLF;
  - the `Foo(x,` continuation that keeps its offset;
  - own-line blocks whose shift would cross column 0, with a comment or `#region` in column 2;
  - a column-0 `#if` that neither moves nor counts.
- The shared-brace branch cannot put a line left of column 0: when no later line is left of the first statement, every later line lands at or right of the statement column.
- A verbatim continuation line keeps its columns and does not count for the leftmost line, after a shared brace line (probed) and in an own-line block with a negative shift (probed).
- Under a negative shift that crosses no column, a later line can land in the brace column (probed). RH3004-006 says so explicitly ("also where it moves a line left of the braces").
- Tabs in later lines: `ShiftIndentation` puts the spaces in front of the tab, and `GetColumn` counts the tab as four columns. That agrees with the formatter's tab replacement (probed).
- `StaysInColumnZero` keeps the right lines: a `///` line is checked on its own text; a `/* */` or `/** */` continuation line is not kept (its trivia does not start at the line); a verbatim continuation line that starts with `//` is not a comment (no trivia there) and keeps its columns.
- Nested Fix All: two levels by test, three levels by trace (one pass per level, within the bound).

## thoenissen/Reihitsu#1281

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 10 | high | `src/Reihitsu.Core/RewriteBindingUtilities.cs` `BindsAsBefore` (`CountErrors` over the operation tree of the whole outermost binding root from `GetBindingRoot`), as `src/Reihitsu.Analyzer/Rules/Clarity/RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer.cs` `TakesNaturalType` uses it | FC-001, FC-027, FC-016 | #5 | PR-introduced (repair, row 26) | RH3106-003 MUST NOT report an anonymous method that takes its natural type, first of all under `var`. The analyzer decides by binding a probe with another natural type (`Action<int>`) and asking whether the code "binds as before". The helper rebinds the outermost statement around the expression, past blocks and anonymous functions, and compares the error count of that whole statement. When a later statement in the same root uses the local whose type follows the natural type, the probe's `Action<int>` makes that use an error. The anonymous method then counts as not taking its natural type, so it is reported with no fix. The error comes from the natural type itself, not from the position. Reported now, not reported at `2284decb` (probed on both): `if (b) { var a = delegate () { }; a(); }`; the same in `using (…) { … }` with `Action b = a;`; `foreach (…) { var arr = new[] { delegate () { } }; arr[0](); }`; `if (b) { var g = () => delegate () { }; g()(); }`; inside a lambda body (`Action x = () => { var a = delegate () { }; a(); };`), an anonymous method body, and a local function body; an `if` in top-level statements; and in async, iterator, and `goto` methods. Holding: the same declaration directly in the method body (the root is the declaration alone); an unused local; `object o = delegate () { }; ((Action)o)();`; a tuple element and an anonymous-object member under `var` with a later use. The fix is not affected: removing the list keeps the converted type, so later uses bind as before. | Count errors and compare bound symbols only on the path from the expression to the root: each ancestor's own operation, without sibling statements of a block or other statements of an anonymous function's body. The return conversion of an enclosing anonymous function with an inferred return type stays on the path, because the `return` is an ancestor. Tests for the failing shapes (one in an `if` block, one in a lambda body, one in top-level statements), each with the later use, and their holding counterpart at method level. |
| 11 | medium | `src/Reihitsu.Core/RewriteBindingUtilities.cs` `GetBindingRoot` (outermost statement other than a block, below the member) | performance (hot path: analyzer), FC-001 | – (the same kind of unbounded check scope as #4, in the other helper; see *Architecture findings*) | PR-introduced (repair, row 26) | The analyzer speculatively rebinds the outermost statement for every empty parameter list of an anonymous method, and counts errors over its operation tree twice. Anonymous methods in one large statement, such as a method body wrapped in `try`, `using`, or a loop, make the analysis quadratic. Timings for `Run(delegate () { Console.WriteLine(i); });` × n with two `Run` overloads, analyzer only: unwrapped n = 100 / 200 / 400 → 97 / 80 / 82 ms; wrapped in one `try` → 1571 / 1939 / 7288 ms. A statement outside every anonymous function around the expression cannot change how the expression binds. Only the code up to the statement that contains the outermost enclosing anonymous function (or the innermost statement or initializer when there is none) has to be rebound. The fix side pays the same per registration. | Bind the smallest root that decides: the innermost statement or initializer around the outermost anonymous function that encloses the expression, or around the expression itself when no anonymous function encloses it. A local declaration needs its statement, not the `EqualsValueClause`, as the round-2 mutation test shows. Same repair window as #10. A test that pins the root, such as a direct helper test of a deep `if` that only binds the inner statement, or a bounded timing check, is the owner's choice. |

### Checked and holding

- Probe and fix decisions on the members #5 listed, and on further positions, LF and CRLF:
  - reported and fixed: `(Action, int) t = (delegate () { }, 1);`, an expression-bodied `Action N() => delegate () { };`, a `goto` method with a delegate target;
  - reported, no fix: a generic local function `L<T>(T)`;
  - not reported: an interpolation hole `$"{delegate () { }}"` and `select delegate () { }` under `var` (`2284decb` reported both and wrote `delegate { }`, which does not compile there); `var t = (delegate () { }, 1);`; `new { A = delegate () { } }`, also inside `new[] { … }`.
- `BindsAsBefore` symbol comparison: the original and the replaced root have the same shape, because the replacement replaces exactly one node. The ancestor lists therefore correspond one to one. Anonymous-function symbols are skipped, so a lambda whose signature follows the probe does not count as a changed binding.
- A missing root (attribute argument, parameter default) returns `false`. Such an input does not compile with an anonymous method in it.

## thoenissen/Reihitsu#1282

Pass. RH3107 only passes its analyzer to the base constructor. Its instances never overlap, so the provider never re-analyzes them. The comment guards are unchanged.

## thoenissen/Reihitsu#1417

Pass. Untouched by the repair.

## thoenissen/Reihitsu#1422

Pass. Untouched by the repair.

## Whole run

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH3106: does the anonymous method take its natural type (`TakesNaturalType` → `BindsAsBefore` with the probe) | the outermost statement or initializer around the expression: errors counted over all of it; symbols compared on the ancestor path | RH3106-003: the anonymous method converts to its own natural type, and no candidate choice depends on it | wider: errors in sibling statements that depend on the natural type count | fails (#10) |
| RH3106 fix: removal keeps meaning (`KeepsMeaningAsync` → `BindsAsBefore` + same converted type) | the same root; converted type of the anonymous method | RH3106-002 (outcome-based), SHARED-FIX-003 | agrees: the converted type is unchanged, so sibling statements cannot change | holds |
| Binding root (`GetBindingRoot`) | the outermost non-block statement, initializer, expression body, or constructor initializer below the member | the code that can decide the expression's binding: up to the outermost enclosing anonymous function | wider than needed | fails on cost (#11) and feeds #10 |
| Fix All re-analysis (`DocumentOrderFixAllProvider.ApplyFixAllAsync`) | re-analyzes after a pass that fixed and lost a diagnostic; at most one pass per starting diagnostic | SHARED-FIX-002 nested sentence | agrees; each further pass applies at least one fix (FC-011) | holds |
| RH3004 multi-line raw string (`ContainsMultiLineRawStringLiteral`) | the block, active and disabled text, checked before the path split | row 28: moved code would need the formatter's raw-string shifting | wider: also refuses the expression-body path, which the formatter lays out | fails (#12) |
| RH3004 measured line vs lines kept in column 0 | measured on the first statement line; `StaysInColumnZero` on the line's first character | RH3004-006: the first statement line stands one level inside the braces; column-0 lines do not move | they conflict when the first statement line starts with a comment in column 0 | fails (#13) |
| RH3004 leftmost-line rules (`GetMovedLineColumns`) | lines with text from the first moved line; excludes column-0 and kept lines | RH3004-006 (rows 29, 31, 33) | identical | holds |
| RH3004 `KeepsColumns` | a token that spans the line start (`findInsideTrivia`) | SHARED-FIX-003 (literal values) | identical for verbatim strings; raw strings never reach it | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Rebinding a rewrite or a probe | `rg -n TryGetSpeculativeSemanticModel src` (excluding tests, bin, obj): only `RewriteBindingUtilities`. Both RH3106 consumers decide through it; the position lists are gone. | one owner (closes the second-owner row of `14-auditor.md`) |
| Fix All for guarded or moving fixes | `DocumentOrderFixAllProvider`: RH3004, RH3005, RH3101, RH3105, the empty-list base (RH3106, RH3107). `rg -n "WithAnalyzers\("`: the provider and `Reihitsu.Tooling/FixtureRunner.cs` only. | one owner. Each provider names its own analyzer at the call site; all five pair correctly. |
| Raw-string lines moving with their code | `rg -l MultiLineRawStringLiteralToken src`: formatter `RawStringLineShifts`, RH5202, RH5601, RH3004 (presence check only) | one owner of the movement (#7 closed). The shared helper is a follow-up candidate. |
| Lines kept in column 0 | RH3004 `StaysInColumnZero` (directives and, since row 33, comments); formatter `IndentationRewriter.IsNonRegionDirectiveTrivia` (active directives) | different decisions; the comment part is fix-only by row 33. Follow-up candidate already recorded. |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RewriteBindingUtilitiesTests` (21) | each criterion (conversion, new errors, symbols, `Add`, root choice) on both sides | the mutation table of `16-test-implementer.md`: each criterion's removal fails a named test | direct call; `CompileRoot` asserts the input compiles | adequate for the criteria. No test puts the expression in a nested statement with a sibling statement that uses its type (#10), and none pins the root's extent (#11). The `Probe` helper restates the analyzer's probe construction. |
| RH3106 new tests (16) | outcome per position, both sides | a natural-type position reported, or a binding-changing fix offered | `Verify`, `VerifyNoCodeFixIsOffered`, exact text | exact, but every `var` case sits directly in the method body, with no later use inside a compound statement or function body (#10) |
| RH3004 raw-string tests (5 withheld, 1 fixed) | the fix is withheld for a block holding a multi-line raw string | a fix offered | `VerifyNoCodeFixIsOffered`, `Verify` | adequate to the spec. `FixWithheldForASingleReturnOfAMultiLineRawStringLiteral` pins the over-wide guard (#12). |
| RH3004 layout tests (rows 29, 31, 33; DEBUG pair) | exact columns from the spec | a different column | `Verify`, `ApplyCodeFixAsync` (CRLF), exact text | adequate. No statement line starts with a column-0 comment (#13). |
| RH3004 and RH3005 nested Fix All, `FixAllCorrectsEveryDiagnostic` (iterations 2 → 1) | one Fix All application corrects nested instances | an inner instance left reported | `Verify` with `NumberOfFixAllIterations = 1` | adequate. Three levels are not tested; the trace holds. |

### Failure classes dismissed

- FC-002/FC-003 beyond #10 and #13: every changed condition has a test on both sides (RH3106 operators, `Add`, declared return types; RH3004 crossing and non-crossing shifts).
- FC-004/FC-005/FC-019/FC-020/FC-021: RH3004 copies comments, directives, and disabled text as text, and only indentation changes. The RH3106 removal is unchanged in text.
- FC-006: the RH3106 fix now withholds on any new error or changed binding. No fixed output in my probes failed to compile (`apply-fix.sh`: no `invalid-result`).
- FC-007/FC-009: each registered action changes the document and silences its diagnostic (one iteration per probe).
- FC-008: nested instances are handled by re-analysis. Two edges do not reach the five providers (Hint 1).
- FC-010–FC-014: no formatter phase changed.
- FC-011: Fix All passes are bounded by the starting diagnostics, and each further pass applied a fix.
- FC-015: see *Policy ownership*.
- FC-017: see *Assertion adequacy*.
- FC-018/FC-036: no rename.
- FC-022: raw strings withhold the block-body fix. Verbatim continuation lines keep their columns (probed under positive and negative shifts).
- FC-023/FC-024/FC-025/FC-029: no identifier, member-kind, or key/value logic changed.
- FC-026: the provider maps spans by offset or re-finds them by analysis; no token is re-resolved by text.
- FC-028: no detached-node formatting.
- FC-030: the RH3004 fix formats only the lambda and writes the block's text back.
- FC-031: the summaries of `RewriteBindingUtilities`, `TakesNaturalType`, `KeepsMeaningAsync`, `DocumentOrderFixAllProvider` and its methods, `MoveStatementsAsync`, `GetFirstStatementLinePosition`, `StaysInColumnZero`, `GetMovedLineColumns`, `KeepsColumns`, `ContainsMultiLineRawStringLiteral`, and the base `GetFixAllProvider` remarks describe the new code. No `TODO`, no tracker number.
- FC-032: every probe ran on a fresh build of the tree it names.
- FC-033: the RH3004 layout probes ran under LF and CRLF with the same result. The CRLF test exists for the shared-brace shape.
- FC-034/FC-035: the tests use the audit's shapes and the spec's values.
- FC-037: no comment-only change claimed.

### Visible output is decided

Every expected value the repair adds or changes is covered:
- RH3004-006/-007: rows 28, 29, 31, and 33, decided on recommendation under the standing instruction, and spec text;
- RH3106 positions: row 26 with answers 2 and 4;
- nested Fix All: SHARED-FIX-002, row 27.

#12 and #13 are outputs no maintainer answer covers. #12 narrows a decided withholding back to the fix `main` gives. #13 is a shape rows 29–33 did not decide. Both go to the lead to decide on recommendation.

### Scope

Every changed path is named in handovers 15–19 or in rows 26–35. `RewriteBindingUtilities` is the new structure of row 26, and the provider's re-analysis is that of row 27. Nothing changed outside the run.

### Architecture findings

**Repeated classes.** #10 repeats #5 (and #2), and #13 repeats #8.
- #10, the RH3106 analyzer against RH3106-003, now through `RewriteBindingUtilities`. The structure of row 26 replaced the position lists with one semantic decision, as decided. The decision's error criterion covers the wrong span: it counts errors anywhere in the outermost statement, so a sibling statement that depends on the natural type itself turns a natural-type position into "binds differently". The decision is one place now, so the repair is local to the helper (the span it counts) and does not reopen the structure.
- #11 has the same root cause as #10: the binding root is wider than the decision needs. That is the same kind of unbounded check scope as #4 had in `RewriteParseUtilities`, but it is a different helper and mechanism, so I do not mark it as a repeat. Both run-added Core helpers would share one rule: re-examine the smallest code that can decide.
- #13, the RH3004 moved-code delta against RH3004-006. Each round has found another line whose column is measured or moved by a rule written for a different line kind:
  - round 1: a directive measured as the first statement line;
  - round 2: a shared brace line, then a negative shift;
  - this round: a statement line kept in column 0.

  The structure has one measured line and one delta, with exception classes added per round. The exception classes are line kinds that do not move: column-0 directives, column-0 comments, verbatim continuation lines. A rule that measures only on lines that move, and gives every line kind exactly one of "moves" or "stays", would close the class. Today a line can be both the measured line and a line that stays.

### Hints

1. `DocumentOrderFixAllProvider` fixes nested instances in a further pass after every later sibling of the first pass. That pass retries every reported diagnostic, including one withheld on the starting code. Strict one-after-another document order would fix the inner instance before the later siblings, and would leave a fix withheld at the start alone (SHARED-FIX-002's Why). For the five providers no fix releases another's guard: RH3005, RH3101, and RH3105 rewrites can only complete a generic reading, never break one; RH3106 removals only widen applicability; RH3004's guards are local. Only RH3004 and RH3005 lose spans. Not constructible today; it matters for the next guarded fix put on the provider.
2. The re-analysis runs only the rule's analyzer, with no `DiagnosticSuppressor` of the project. A diagnostic a suppressor hides could be fixed in a second pass. No RH suppressor exists.
3. RH3004-006 lets a later line of an own-line block land in the brace column when the measured shift is negative and crosses no column (`Run(⏎    delegate ()⏎        {⏎            <24>Foo(@"a⏎b");⏎<20>Foo(1);…` → `Foo(1);` at 12, the brace column). The spec decides this ("also where it moves a line left of the braces"). It is noted only because row 29 treats the same left-of-the-first-statement layout differently after a shared brace line.
