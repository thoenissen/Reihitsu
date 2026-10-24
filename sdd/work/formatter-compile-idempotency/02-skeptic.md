Role: skeptic
Commit: 016b33f
Verdict: #948: proceed; #949: proceed; #954: proceed; #1390: proceed (one non-blocking question for the maintainer); #888: proceed

How the statements below were established: `scripts/trace.sh` (2–3 passes), `scripts/format.sh` (run twice), and `scripts/apply-fix.sh` (LF and CRLF) over fixtures in a scratch directory outside the repository, and a scratch `net10.0` library built with `dotnet build` for the C# scope and `async` claims. All fixtures were deleted afterwards. `src/` at 016b33f is identical to `origin/main` (f3d514d); only specs, rule pages, and `sdd/work/` differ.

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The issues carry cause hypotheses of their own (#949: "the blank-line phase does not insert the blank line"; #954: `TokenGapNormalizer`/`HasOwnLinePlacementOwner`; #888 as filed: RH5201 measures the wrong column). I treated each one as a claim and checked it. The results are under each issue.

## thoenissen/Reihitsu#948

### Premise

The bug is real. `format.sh` braces both sections of the issue input. `SwitchCaseBraceRewriter.VisitSwitchStatement` checks only `HasCrossSectionGotoTarget` before `BuildSections`, and nothing in the phase looks at names.

The C# scope facts come from the scratch library, built unbraced and braced:

- These forms compile unbraced, so the name reaches the other sections: `return F(out var o);`, `throw … F(out var o) …;`, `yield return F(out var o);`, `switch (F(out var o))` (the governing expression of a nested switch), `lock (… F(out var o) …)`, `var (a, b) = …;`, `(var a, var b) = …;`, labeled `L: int y = 1;`, labeled `L: F(out var y);`, `const int K` used as `case K:` of another section, a local function called in another section's `when L() > 0`, and `if (p is int n)` used inside a nested block of another section.
- Braced, the uses fail with CS0103. Verified for the `return`, `case K:`, and `when L()` forms.
- These forms are not visible in another section even unbraced (CS0103): `using (… out var o …)`, `foreach (… out var o …)`, `do … while (F(out var w) …)`, an embedded `if (c) F(out var z);`, and `else if (p is int m)`. `using var` directly in a section is CS8647 in any case.

Today `format.sh` braces the issue input, the `case K:` input, the `return … out var` input, the deconstruction input, the labeled declaration input, and the `when L()` input. None of them would compile after formatting.

### Mechanism and candidates

Mechanism: the phase decides from line shape alone whether to brace a switch, and guards only `goto` targets. When a statement that a section owns directly declares a name, C# scopes that name to the whole switch block, but the phase never checks whether another section references the name. A block around the declaring section then narrows the name's scope below that reference.

Dispatch: `VisitSwitchStatement` → `HasAnyMultiLineSection` → `HasCrossSectionGotoTarget` → `BuildSections` → `BuildSection` → `AddBraces`.

Candidates, taken from the `StatementSyntax` kinds a `SwitchSectionSyntax` owns directly:

- **Declare at switch-block scope.** These must trigger the guard when another section uses the name.
  - `LocalDeclarationStatementSyntax`, `const` included.
  - `LocalFunctionStatementSyntax`.
  - `ExpressionStatementSyntax` with a `DeclarationExpressionSyntax` (`out var`, deconstruction `var (a, b)` / `(var a, var b)`) or a pattern designation (`is int n`, `is { } x`, `is var v`).
  - The condition of an `IfStatementSyntax` the section owns. An `else if` condition does not count.
  - `ReturnStatementSyntax`, `ThrowStatementSyntax`, and `YieldStatementSyntax` (`yield return`).
  - The governing expression of a `SwitchStatementSyntax`, and the expression of a `LockStatementSyntax`.
  - A `LabeledStatementSyntax` that wraps any of these.
- **Scoped to themselves.** These must not trigger the guard: `While`, `Do`, `For`, `ForEach`/`ForEachVariable`, `Using`, and `Fixed` statements; the embedded statements of `if`/`else`; `BlockSyntax`; `Try`; `checked`/`unsafe` blocks; lambda, anonymous-method, and local-function bodies; and the sections of a nested switch.
- **Uses.** An `IdentifierNameSyntax` anywhere in another section counts: in statements, nested blocks, lambdas, nested switches, and labels (a `case K:` constant pattern or a `when` clause). A pattern variable declared in a `case` label is scoped to its own section and never crosses sections.

### Breaking inputs

- **FC-034:** the issue input exactly, the issue comment's local function (declared in section 1, called in section 2), and the spec's reverse direction (declared in section 2, called in section 1).
- **FC-016 / FC-002:** the declaration forms FMT-SWITCH-014 names, plus the forms C# also scopes to the switch block, which the requirement's list omits: `return`/`throw`/`yield return` with `out var`, deconstruction declarations, a labeled declaration, a `const` used in another section's `case` label, and a local function used in another section's `when` clause. See the spec note under *Simpler option*.
- **FC-002 (the qualifier "directly").** These must not trigger: a declaration in a nested block (spec example), in a lambda, in a local-function body, in an `else if` condition, or in a loop, `using`, or `foreach` header, and a declaration in a section of a nested switch. For the nested-switch case: an inner section declares `y` and outer section 2 reads field `y`. The outer switch is braced and compiles; today's output already is braced.
- **FC-003:** both sides of the guard. A local used in another section leaves the switch unchanged; a local used only in its own section is braced (spec example). Cover a use before the declaring section and a use after it.
- **FC-027:**
  - The outer switch is guarded while an inner switch is still braced by its own decision.
  - A local of an outer section that is used only inside an inner switch of the same outer section must not trigger.
- **Interplay.** A section that already holds a block (FMT-SWITCH-008) and contains the use (`case 2: { y = 2; } break;`) is still guarded. A section with a directive stays unbraced on its own; that does not change the whole-switch decision.
- **FC-006 / FC-017:** assert the exact output (the input unchanged) plus a second pass. The FMT-BLANK-002 blank line after a local declaration is already in the spec example inputs.
- **FC-026:** two switches in one method, one guarded and one not.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter `SwitchCaseBraceRewriter` | Must change: a whole-switch guard next to the `goto` guard |
| `Reihitsu.Core` | Not involved. There is no shared C# scope helper. `RegionDirectiveUtilities` and `OrderingMoveSafety` mention local functions, but they belong to run R4 and must not be touched |
| Analyzers | Not involved. No rule asks for switch-section braces; RH7501 only reports a `break` inside a block |
| Code fixes, rule docs | Not involved |
| Tests | Formatter regression tests with a second pass, and FMT-SWITCH-014 coverage |

### Simpler option

None. Leaving the whole switch unchanged is the decided outcome (thoenissen/Reihitsu#919 Q1, like FMT-SWITCH-011).

Spec note for the spec author (precision only, not a maintainer question):

- FMT-SWITCH-014 counts `out var` and pattern variables only in a local declaration, an expression statement, or an `if` condition. Its coverage sentence calls the other statement kinds ones "whose scope C# may limit to that statement".
- The compile evidence above shows that `return`, `throw`, `yield return`, a nested switch's governing expression, `lock`, a labeled statement, and deconstruction declarations do reach the other sections.
- FMT-PIPE-003 already decides those inputs: the output must compile, and only the unchanged switch compiles. The coverage sentence therefore leaves open what another requirement decides.
- Recommended: widen the clarification. Otherwise the test implementer covers these inputs under FMT-PIPE-003.

### Questions for the maintainer

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#949

### Premise

The bug is real, but the issue's cause statement is wrong. Trace of the issue input:

1. Pass 1, `BlankLinePhase`, inserts the blank line before `// before break`.
2. Pass 1, `SwitchCaseBracePhase`, removes it again.
3. Pass 2, `BlankLinePhase`, inserts it again; the text is stable after pass 3.

The same happens:

- under CRLF;
- with a `/* … */` comment;
- in a section braced only because another section is multi-line (`case 2:` / `case 3:` / `C();` / `// before break` / `break;`).

RH5020, run through `apply-fix`, reports today's pass-1 output (`}`, then the comment, then `break`) under LF and CRLF, and reports nothing on the spec's expected output. Today's pass-1 output is therefore an instance of FC-013.

### Mechanism and candidates

Mechanism: when `AddBraces` moves the terminal `break` behind the new block, `BuildLeadingTrivia` rebuilds the break's leading trivia from its comments alone: one line break, then each comment followed by one line break. Every blank line in that trivia is dropped: the one `BlankLinePhase` placed above the first comment, and any line between two comments. No phase after `SwitchCaseBracePhase` inserts blank lines in the same pass.

`BuildLeadingTrivia` rebuilds two trivia lists:

1. The leading trivia of the terminal `break` (`BlankLineSpacingPolicy.IsTerminalDirectSwitchSectionBreak`). This is the issue.
2. The leading trivia of the first statement moved into the block. This is a sibling; see *Follow-up candidates*.

Not affected:

- sections that end in `return`, `throw`, `goto`, or `continue`: every statement goes into the block and the inner statements keep their trivia (verified with `return`);
- sections that are already braced;
- sections with directives, which stay unbraced;
- sections holding only `break`, which stay unbraced.

### Breaking inputs

- **FC-034:** the issue input exactly.
- **FC-016:** two comments in front of the `break`, separated by a blank line (`// one`, blank, `// two`, `break;`). Today the blank line between them is lost for good: pass 2 restores only the line above `// one`. FMT-PIPE-013 ("the blank lines between the comments that lead a statement") already requires that line to stay.
- Further shapes:
  - a `/* … */` comment;
  - a trailing comment on the last statement plus an own-line comment before the `break` (`C(); // trailing` / `// own line` / `break;`);
  - a section with several labels;
  - a section braced only because another section is multi-line.
- **FC-010 / FC-017:** LF and CRLF, with two passes.
- **FC-003 boundaries.** These already work and must stay:
  - a comment before the `break` in a switch that gets no braces: the blank line is inserted and the output is stable;
  - a comment that is the first content of a section holding only `break` (`case 1:` / `// c` / `break;`): no blank line, and no braces.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter `SwitchCaseBraceRewriter` | Must change |
| Formatter `BlankLinePhase` | Must stay: before bracing, its pass-1 output for the issue input is already what the spec asks |
| RH5020 analyzer | Must stay: it agrees with the expected output |
| RH7501 analyzer and fix | Not involved: the fix does not call the formatter |

### Simpler option

_None._

### Questions for the maintainer

_None._

### Follow-up candidates

- **Same helper, other trivia.** The blank lines between comments that lead the first statement of a braced section are dropped too. `case 1:` / `// one` / blank / `// two` / `A();` becomes `// one` / `// two` inside the block, and the result is stable. That is an FMT-PIPE-013 violation today, from the same `BuildLeadingTrivia`. It lies outside #949's construct. It belongs in this run only if the lead widens the scope; FMT-PIPE-013 already decides the output.
- **RH7501's fix.** Its output `}` / `// comment` / `break;` is pinned by the fixed code in `RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzerTests` (the "Exit this case" and "leave the section" cases). RH5020 reports that shape, so the fix raises another RH diagnostic (FC-007 across rules). The fix does not call the formatter, so this run leaves it unchanged.

## thoenissen/Reihitsu#954

### Premise

The bug is real, but the title overstates it. The text is the same after every pass (`trace.sh` reports "Stable after pass 2"), so `--check` is stable. The defects are:

- the output carries a blank line after `{` that FMT-BLANK-007 forbids. RH5022, run through `apply-fix`, reports today's output under LF and CRLF (FC-013);
- two phases undo each other's edit in every pass (FC-012).

The issue's sentence "No phase undoes another phase's edit in any pass" is not a requirement of any spec. I agree with the spec author's handling.

When the blank line after `{` appears (from the trace):

- **Triggered:**
  - a class body (the issue), a namespace, an enum, a method body, a switch statement block;
  - with a comment first (`// c`) or `#pragma` first;
  - with a comment inside the region, with nested regions, and with a blank line already between the directives.
- **Not triggered:**
  - a member or statement after the region (`#endregion` is then that token's trivia);
  - a token before the region (`int _x;`, region, `}`);
  - a comment, or `#if` / `#endif`, after `#endregion`;
  - a region in front of a statement in an accessor body;
  - a body of comments only, with a blank line between them;
  - `#if X`, a blank line, `#endif`.

On the comment variant the maintainer answered Q2 with A (fix it in this run). RH5031, run through `apply-fix` on today's formatter output, reports more than the spec example (`{`, `// c`, `#region A`, `#endregion`, `}`). It also reports the cases where:

- the region holds a member;
- the region holds a statement in a method body;
- two comments stand above the region;
- a `#pragma` is the first content after `{`;
- a `///` comment is the first content after `{`.

With a member above the comment or the directive (`int _y;` / `#pragma …` / `#region A`), the line is inserted today and RH5031 is clean.

### Mechanism and candidates

There are two mechanisms.

**(a) Blank line after `{`.** When the body holds no token, all content between `{` and `}` is the leading trivia of `}`. If the last significant trivia there is a region directive, the line-break phase treats the gap in front of `}` as having no content of its own: `TokenGapNormalizer.HasOwnLinePlacementOwner` excludes region directives, so `NormalizeLeadingGapWithoutContent` keeps the whole trivia and prepends a line break. Meanwhile `{` keeps its own trailing line break. The result is a blank line after `{`, which `BlankLinePhase` removes in the next pass and the line-break phase restores.

The membership condition is established by the trace table above; the code path is my reading of `TokenGapNormalizer` and agrees with the issue's hypothesis. Condition: the token before `}` is its own `{`, and the last significant trivia of `}` is an active `#region` or `#endregion`.

Candidates: every brace owner in FMT-BREAK-002's list, each with an empty body:

- namespace;
- class, struct, record, interface, enum;
- method, constructor, operator, and accessor bodies;
- local function and lambda block;
- statement block;
- switch statement block.

The first trivia in the body can be nothing, a comment, or a non-region directive. Test under LF and CRLF.

**(b) Comment, directive, or doc comment directly above `#region` after `{`.** `BlankLinePhase` decides SHARED-BLANK-005's opening-brace exemption from the token in front of the directive's trivia, not from the line directly above the directive. The predicate is `BlankLineEditor.IsExemptFromPrecedingBlankLineBeforeRegionDirective`: the previous token is `{` without a trailing comment, or there is no previous token. Any trivia line between `{` and the first region directive therefore inherits the exemption. The predicate has two consumers: `BlankLineRegionDirectiveRewriter` and `BlankLineTriviaBoundaryRewriter`.

Candidates: between `{` and `#region`, a `//` comment, a `/* */` comment, several comments, a non-region directive (`#pragma`, `#nullable`), or a documentation comment; and the region empty, holding a member, or holding a statement.

### Breaking inputs

- **FC-034:** the issue input exactly, plus the variants the issue lists (method body, blank line already between the directives, `// c` above the region).
- **FC-012 / FC-017:** the text-level two-pass check cannot catch (a), because the text is the same after every pass. Assert the exact FMT-BLANK-007 output of the first pass.
- **FC-016**, for (b): a region holding a member, a region holding a statement, two comments, `#pragma`, and `/* */`.
- **FC-003 boundaries** that work today and must stay:
  - a trailing comment on the brace line (`{ // x`) followed by `#region`: the blank line is inserted (SHARED-BLANK-005: such a brace line is not exempt);
  - a member above the comment: the blank line is inserted.
- **FC-020:** `#if` / `#endif` inside the braces (no change); `#pragma` followed by a region (triggers both (a) and (b)).
- **SHARED-BLANK-007:** a region inside an inactive `#if` is not padded.
- **FC-010:** CRLF.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Line-break gap normalization (`TokenGapNormalizer`, `BracePlacer`) | Must change for (a). `TokenGapNormalizer` places every brace and every initializer brace of `LineBreakPhase`, so a change there touches a helper with many consumers. The lead should weigh it against escalation 5 |
| `BlankLineEditor.IsExemptFromPrecedingBlankLineBeforeRegionDirective` and its two consumers | Must change for (b) |
| RH5022, RH5031, RH5032 analyzers | Must stay: `apply-fix` reports nothing on either spec example (LF and CRLF) |
| RH5031 and RH5032 code fixes | Not involved |
| Core `RegionDirectiveUtilities` | Not involved; it belongs to run R4 |
| `RegionFormattingFullPipelineTests.EmptyRegionInsideBlockIsPreservedAndFormatted` | Asserts the defect (a blank line after `{` in a method body). The test implementer must correct its expected value |

### Simpler option

_None._

### Questions for the maintainer

_None._ SHARED-BLANK-005 ("Only the single line directly above counts") already decides the siblings of (b); only their scope is open.

### Follow-up candidates

- **Start of file.** A file that opens with `// header` then `#region A`: the formatter keeps the two lines adjacent and RH5031 reports it (`apply-fix`: fixed under LF and CRLF). It goes through the same predicate (no previous token, so exempt), so a fix in the predicate changes it too. The lead decides the scope.
- thoenissen/Reihitsu#1263 (RH7303/RH7310), named in the issue comment, is unrelated to this defect.

## thoenissen/Reihitsu#1390

### Premise

All three instances are real.

- **`async` alias.** `format.sh`, `apply-fix.sh RH3202`, and `apply-fix.sh RH3208` turn `async T RunAsync() => await T.Delay(1);` (with `T` an alias of `Task`) into `return await T.Delay(1);`, which is CS1997, for methods and for local functions alike. I found one more failing input: `async Task? P() => await Task.Delay(1);`. With `#nullable enable` it compiles as written, but `format.sh` turns it into `return await Task.Delay(1);`, and that fails with CS1997 (verified in the scratch library). `async Task<int>? N() => await …` becomes `return await …`, which compiles.
- **Comments around the arrow.** `format.sh` output today (stable on a second run):
  - a comment on its own line before the arrow ends up between the signature and `{`. Seen for a method, a local function, a constructor with `: base()`, a finalizer, an operator, a conversion operator, a method with a `where` clause, and a multi-line `/* */`;
  - a comment on its own line after the arrow becomes `return`, blank line, comment, expression;
  - with an expression statement or a `throw` statement, the comment is glued behind `{` (`{        // run`);
  - two comments around the arrow with a blank line between them keep the blank line between `return` and the value.
- **`with` initializer.** Trace: pass 1 breaks the elements and `}` onto their own lines but keeps `{` on the `with` line. Pass 2 moves `{`. The text is stable after pass 3.

### Mechanism and candidates

**(1) `async` return types.** `ExpressionBodiedTransformUtilities.UsesExpressionStatement` chooses an expression statement only for `void`, or for `async` with a return type spelled `Task` or `ValueTask`: an `IdentifierName`, the right part of a `QualifiedName`, or the name of an `AliasQualifiedName`, never generic. Every other `async` return type falls through to a `return` statement. That fails with CS1997 whenever the type is a task without a result.

The dispatch is on the return type's `TypeSyntax`:

- `PredefinedTypeSyntax`: `void` or another keyword;
- `IdentifierNameSyntax`: `Task`/`ValueTask`, an alias, or a custom type;
- `GenericNameSyntax`: `Task<…>`/`ValueTask<…>`, or another type such as `MyTask<int>`;
- `QualifiedNameSyntax`, with an identifier or a generic name on the right;
- `AliasQualifiedNameSyntax`;
- `NullableTypeSyntax`: `Task?`, `Task<int>?`;
- array, tuple, and pointer types, which are invalid under `async`.

Each applies to methods (generic ones and explicit interface implementations included) and to local functions (`static` included) (FC-025). Constructors, finalizers, operators, and conversion operators cannot be `async`.

**(2) Comments around the arrow.** `ExpressionBodyToBlockConverter.CreateBlock` gives the new `{` the arrow's leading trivia and the significant part of the arrow's trailing trivia. It leaves the expression's own leading trivia (or the `throw` keyword's) where it is. So:

- a comment on its own line before the arrow stays in front of `{`;
- a comment on its own line after the arrow stays between `return` and the expression;
- when the statement starts with the expression itself or with `throw`, the comment stays directly behind `{`.

All six transforms share `CreateBlock` (method, local function, constructor, finalizer, operator, conversion operator; FC-025).

**(3) `with` initializer.** `LineBreakInitializerRewriter.VisitInitializerExpression` places an initializer's `{` through `BracePlacer.EnsureBraceOnOwnLine` → `TokenGapNormalizer.NormalizeGapBeforeOwnedTokenPreservingPreviousTrivia`. That method leaves the brace alone when it finds no previous token.

My reading of the code: once an earlier edit in the same visit has replaced the initializer node — elements moved onto their own lines, or a nested element rewritten — the `with` keyword is no longer reachable from the brace. The brace then stays, and the next pass, which makes no earlier edit, places it. An object creation escapes this because its own visit places the brace from the parent node.

Membership, established by the evidence below: the initializer's `{` is placed only by this visit (a `with` initializer, or an object initializer assigned to a member, `Inner = { … }`), and that visit makes an earlier edit.

| Input | Passes until the layout is reached |
|---|---|
| `p with { A = 1, B = 2 }` (also inside `return`) | 2 |
| `p with { A = 1 }` | 1 |
| `p with { A = new X(1, 2) { B = 1, C = 2 } }` (one element, with a nested rewrite) | 2 |
| `new Foo { Inner = { A = 1, B = 2 } }` | 2 |
| `new { … }`, `new() { … }`, `new Foo { … }`, `new List<int> { … }`, a dictionary indexer initializer | 1 |
| `p with { }` | 1 |

### Breaking inputs

- **FC-034:** the issue's exact shapes:
  - alias `T` with `RunAsync`;
  - `int Other()` / `// before arrow` / `=> 1;`;
  - `int Get() =>` / `// c` / `42;`;
  - `R P => _r with { A = 1, B = 2 };`.
- **(1), FC-025 / FC-016:**
  - every `TypeSyntax` row above, for both a method and a local function, and a `static` local function;
  - the FMT-STRUCT-025 example inputs: alias `T`, generic alias `V`, custom `MyTask`, and the qualified `System.Threading.Tasks.Task` and `Task<int>`.
- **FC-009 / FC-008:**
  - for a member that FMT-STRUCT-025 keeps, RH3202 and RH3208 offer no action (`apply-fix` status `no-fix-offered`);
  - Fix All over a document with one convertible member and one kept member converts only the convertible one.
- **FC-015:** the keep predicate for FMT-STRUCT-025 needs one owner, read by the formatter and the code-fix base but not by the analyzers' reporting predicate (SHARED-BODY-004). See *Surfaces*.
- **(2):**
  - every `CreateBlock` kind: a constructor with `: base()`, a finalizer, an operator, a conversion operator, a method with a constraint clause, and a local function;
  - the `return`, expression-statement, and `throw` forms;
  - one comment on its own line before the arrow, one after it, and both together (source order);
  - two comments separated by a blank line (`int Two() =>` / `// a` / blank / `// b` / `42;`). FMT-PIPE-013 keeps the blank line between them in front of the created statement;
  - a multi-line `/* */`;
  - `int Value() =>` / `/* c */ 42;`, which becomes `return /* c */ 42;`.
- **(2), unspecified parts.** Do not pin a comment on the arrow's line, which is thoenissen/Reihitsu#963. `int Get3()` / `// own` / `=> /* glued */ 3;` mixes a specified part and an unspecified part; assert only the own-line comment.
- **(2), members that keep their body.** A member that FMT-STRUCT-007 or FMT-STRUCT-025 keeps leaves its comments untouched.
- **(2), code fixes.** RH3202–RH3208 output follows the formatter (SHARED-BODY-003; FC-032), so add at least one code-fix test with a comment around the arrow.
- **(3):** a two-element `with`; one element with a nested rewritten element; `with` in an expression-bodied property, a local, a `return`, and an argument; CRLF; a second pass (FC-017).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter `ExpressionBodiedTransformUtilities` (via the method and local-function transforms) | Must change |
| Formatter `ExpressionBodyToBlockConverter` | Must change |
| Formatter `LineBreakInitializerRewriter` (or the gap normalizer under it) | Must change. `TokenGapNormalizer` has many consumers; the lead should weigh it against escalation 5 |
| Core `ExpressionBodyRewriteUtilities.BlocksRewrite` | Must stay. `ExpressionBodyAnalyzerBase` and the RH3202 and RH3203 analyzers call it. Adding FMT-STRUCT-025 there would stop the RH3202/RH3208 reports, which SHARED-BODY-004 forbids |
| RH3202 and RH3208 analyzers | Must stay |
| `ExpressionBodyToBlockCodeFixProviderBase` | Must change. It registers an action for every diagnostic, so once the formatter keeps an FMT-STRUCT-025 member, that action would change nothing (FC-009) |
| RH3203, RH3205, RH3206, RH3207 fixes (same base) | Code must stay; their output changes with (2) (FC-032) |
| RH5301, RH5303, RH5304 | Must stay. If (3) is fixed in the shared visit, the output of RH5304's fix, which formats nested initializers, may change |
| Rule docs RH3202, RH3208 | Already changed by the spec author |
| Existing tests that pin the old layout for (2): `ExpressionBodiedMethodTests.PreservesCommentBeforeArrow`, `ExpressionBodiedConstructorTests.PreservesCommentBeforeArrow`, `ExpressionBodyNextLineExpressionTests.LineCommentBeforeNextLineExpressionKeepsOwnLine` | Their expected values contradict the clarified FMT-STRUCT-006. The test implementer must correct them |

### Simpler option

_None._

### Questions for the maintainer

**Q1 (not blocking): does a nullable-annotated task type count as "written as" a task type under FMT-STRUCT-025?**

**Context.** FMT-STRUCT-025 keeps the expression body unless the return type is written as `Task`, `ValueTask`, `Task<…>`, or `ValueTask<…>`. Two forms compile with `#nullable enable` (verified):

```csharp
async Task? P() => await Task.Delay(1);
async Task<int>? N() => await Task.FromResult(1);
```

Today `P` becomes `return await Task.Delay(1);`, which is CS1997. `N` becomes `return await …`, which compiles.

- **A** — The `?` means the type is not written as a task type. Both members keep their expression body; RH3202/RH3208 report them and offer no fix. `N` loses the conversion it gets today.
- **B** — Look through the `?`. `P` becomes `{ await Task.Delay(1); }` and `N` becomes `{ return await Task.FromResult(1); }`, and the fix is offered. The text of FMT-STRUCT-025 and the two rule pages then names the nullable form.

**Recommendation: B.** The type is known from the syntax, so FMT-STRUCT-025's own reason (the formatter cannot see behind an alias) does not apply. A would leave a permanent RH3202 diagnostic without a fix on code that the formatter can convert safely. I would pick A if the maintainer wants the predicate to stay the literal list of spellings.

**Why it does not block.** Both answers produce compiling code. No other #1390 test depends on it; only a `Task?` / `Task<int>?` test has to wait.

### Follow-up candidates

- **Nested object initializer.** `new Foo { Inner = { A = 1, B = 2 } }`, an object initializer assigned to a member, needs two passes (FMT-PIPE-004) through the same mechanism as (3). FMT-BREAK-021 does not name the layout of this initializer. It is outside #1390's construct, unless the fix for (3) lands in the shared visit, which would change its pass-1 output as well.
- **Alias named `Task`.** An alias named `Task` for a type that carries a result (`using Task = System.Threading.Tasks.ValueTask<int>;`) is converted under FMT-STRUCT-025's spelling rule into a body without `return`, which is CS0161. This is a residual risk of a formatter that sees only syntax. It is only worth noting.
- **Documentation comment before the arrow.** A `///` comment on its own line before the arrow ends up between the signature and `{` today, and the documentation-comment phase (run R2's files) reformats it. The new paragraph of FMT-STRUCT-006 says "comment" without saying whether a documentation comment counts. Do not pin it in this run.
- **SHARED-BODY-001 title.** The title, "A member the formatter cannot convert is not reported", now reads wider than its text, because under SHARED-BODY-004 such a member is reported. The spec author may align the title or the `Why:` (precision only).

## thoenissen/Reihitsu#888

### Premise

The bug is real, as the maintainer re-scoped it: the formatter is at fault, and the RH5201 report is correct. In the trace of the issue input, the only change is the FMT-BLANK-011 blank line above `/* x`; `.D()` stays behind `*/`. `apply-fix.sh RH5201` on the issue input ends in `no-progress (2 iteration(s))` under LF and CRLF. On the spec's expected output it reports `no-diagnostic`. SHARED-COMMENT-012, as the maintainer decided, supersedes the issue's original framing (RH5201 measures the wrong column).

### Mechanism and candidates

Mechanism: `ChainLineBreakRewriter.EnsureLineBreak` skips a link whose gap holds any end-of-line trivia (`HasLineBreak`: a trailing end-of-line on the previous token, or an end-of-line in the operator's leading trivia). It does not check whether a multi-line delimited comment stands between that line break and the operator. A later link behind the closing `*/` therefore counts as already starting a line.

When the comment starts on the previous link's line, it sits in the trailing trivia of `)` and the gap holds no end-of-line trivia. The break is inserted today: `.C() /* x` / `*/` / `.D()` with `.D` at column 17, which is correct.

Candidates:

- operator kinds `.`, `?.`, `!.`, and `!?.` (`FluentChainLink`);
- link kinds after the first invoked link: a call, and a property read such as `.E`. SHARED-CHAIN-005 covers every link of the chain part after the first invoked link;
- trivia owner: the operator's leading trivia (the issue) or the previous token's trailing trivia (works today);
- comment kinds: `/* */` and `/** */`.

### Breaking inputs

- **FC-034:** the issue input exactly (`*/` at column 8), plus the issue's variant with the last line indented further (`y */` at column 20).
- **`?.` and property links.** `.C()` / `/* x` / `*/ ?.D()` / `/* y` / `*/ .E;`: neither link is broken today, and RH5201 ends in `no-progress`.
- **FC-003 boundaries** that work today and must stay:
  - a single-line `/* x */ .D()` on its own line: unchanged, RH5201-clean;
  - `/* x` / `*/` with `.D()` on the next line: unchanged;
  - a `//` comment above `.D`;
  - a comment that starts on the `.C()` line.
- **FC-021:** two comments on the last line (`*/ /* y */ .D()`).
- **FC-007 / FC-017:** one application of the RH5201 code fix converges, under LF and CRLF (SHARED-CHAIN-008 and SHARED-CHAIN-009).
- **FC-013:** the formatter's output is RH5201-clean.
- **FC-010:** CRLF inside the comment.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter `ChainLineBreakRewriter` | Must change |
| Core `FluentChain`, `FluentChainLink.StartsLine`, `SyntaxTokenPositionUtilities.IsFirstOnLine` | Must stay. RH5112, RH5201, and RH5048 share them, and many other callers use `IsFirstOnLine`. The maintainer kept RH5201 unchanged. Changing wrap detection only in the formatter would produce output that RH5201 reports (see *Follow-up candidates*) |
| `IndentationPhase` | Not involved: it aligns `.D` once `.D` starts a line (column 17 in the case that works today) |
| RH5201 analyzer | Must stay |
| RH5201 code fix | Code must stay; after the formatter change it must converge in one application |
| RH5112 code fix | Code must stay; it delegates to the same chain layout (FC-032) |
| `sdd/specs/analyzer/RH5201.md` | Not changed in this run (the spec author lists it as a follow-up) |

### Simpler option

_None._

Spec note for the spec author (precision only): the new sentence in FMT-BREAK-019 says "A later call". SHARED-CHAIN-005, which the sentence invokes, covers every link of the chain part after the first invoked link, a property read included (`.ToList()` / `/* x` / `*/ .Count`). "A later link" would make the cited text cover the `.E` case.

### Questions for the maintainer

_None._

### Follow-up candidates

- **Wrap detection against SHARED-COMMENT-012.** Take `var r = a.B().C()` / `/* x` / `*/ .D();`.
  - By SHARED-CHAIN-002 together with SHARED-COMMENT-012, no link operator starts a line. The chain is not wrapped, and nothing should break.
  - Core's `FluentChain.IsWrapped` counts `.D` as starting a line. So today the formatter breaks before `.C`, and RH5201 reports the chain (`apply-fix`: `no-progress`).
  - After this run's fix the formatter would also break before `.D`. That output is RH5201-clean and consistent with the analyzers.
  - Aligning wrap detection with the spec text would change Core and RH5112, RH5201, and RH5048 together, which this run excludes. No test in this run should pin either output for this input.
- `sdd/specs/analyzer/RH5201.md` does not list SHARED-COMMENT-012 under `Uses` (already the spec author's follow-up).
- thoenissen/Reihitsu#1382 (P33) covers the other formatter requirements that place a token behind a comment's closing line.

## Whole run

- **Existing tests whose expected values the specs now contradict.** The test implementer must correct them; the implementer must not:
  - `RegionFormattingFullPipelineTests.EmptyRegionInsideBlockIsPreservedAndFormatted` (#954);
  - `ExpressionBodiedMethodTests.PreservesCommentBeforeArrow` (#1390);
  - `ExpressionBodiedConstructorTests.PreservesCommentBeforeArrow` (#1390);
  - `ExpressionBodyNextLineExpressionTests.LineCommentBeforeNextLineExpressionKeepsOwnLine` (#1390).

  I found these by searching, not by running the suite, so the suite may turn up more.
- **Shared helper.** #954 (a) and #1390 (3) both sit in `TokenGapNormalizer`, which places every brace and initializer brace in `LineBreakPhase`.
- **Concurrent runs.** No surface named above belongs to run R2, R3, R4, R5, or R7. Run R7 owns the RH3xxx code-fix base, which the maintainer released to this run (Q1 answered B).
