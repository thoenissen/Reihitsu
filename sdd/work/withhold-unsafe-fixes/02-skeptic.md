Role: skeptic
Commit: f71365b7
Verdict: #1257: proceed (wider: the cast reading belongs to the class); #1279: output question (Q1), proceed; #1281: output question (Q2), proceed, RH3106-002 and RH3201-006 need widening; #1282: proceed; #1417: proceed; #1422: proceed, RH3104-005 needs explicit wording

My prompt carried no intended fix or hoped-for verdict. It asked me to check the premises against the current tree and to give a view on two output questions and two items the spec author left for me to confirm; I did that and nothing else.

Method for every claim below: fixtures in a scratch directory outside the repository, run through `scripts/apply-fix.sh <ID> <dir>` (LF and CRLF, `LanguageVersion.Latest`). Each input and each fixed output was compiled with Roslyn 5.9.0 (the version the repository references) at the language version named. The code fixes and analyzers of the run's rules have not changed since the claim commit: `git show --stat 61718e25 0fa1778c` touches none of RH3001, RH3004, RH3005, RH3101, RH3103–RH3107, RH3201, RH5417. Every instance below reproduces on f71365b7 unless I say otherwise.

Caller for every issue: the analyzer's code fix (single fix and Fix All) and the analyzer's diagnostic in the IDE and the build. These are ordinary user-written expressions, and no direct-call-only input is involved.

## thoenissen/Reihitsu#1257

### Premise

Real. The fix output for each instance does not compile (CS0307 + CS0246):

| Rule | Input | Fix output today |
|---|---|---|
| RH3001 | `M(G < A, B > !x switch { true => 1, false => 0 })` | `M(G < A, B > (x == false) switch …)` |
| RH3001 | `M(G < A, H < B >> !x switch { … })` | `… H < B >> (x == false) switch …` |
| RH3001 | `M(G < A, H < A, J < B >>> !x switch { … })` | `… J < B >>> (x == false) switch …` |
| RH3001 | `M(G < A, B > !(x) switch { … })` | `… B > ((x) == false) switch …` |
| RH3005 | `M(Max > G, B > (7))` | `M(G < Max, B > (7))` |
| RH3101 | `M(A < base.B, D > (7))` | `M(A < B, D > (7))` |
| RH3105 | `M(A < this.B, D > (7))` and `M(A < B, this.D > (7))` | `M(A < B, D > (7))` |

The issue's first RH3001 example, `M(G < A, B > !x)` with `bool x`, does not compile as input (`int > bool`), so it is no test input. The `switch` form is the valid shape, and the spec uses it. The `!x + t` continuation in the issue comment also needs a user-defined operator to compile, and the class matrix below covers it.

### Mechanism and candidates

Mechanism: each fix changes the tokens next to a `<` … `>` pair, or turns a qualified member access into a simple name, and never checks that the rewritten expression parses into the same operators and operands. C#'s disambiguation then reads the new token sequence as a different construct.

The dispatch is the parser's disambiguation, not the rules. Two disambiguations reach these fixes:

1. **Generic name.** `identifier < … >` followed by one of the disambiguating tokens is read as a type argument list (ECMA §6.2.5). All four rules fail. The spec covers this member.
2. **Cast.** `( identifier )` followed by `(`, an identifier, a literal, `!`, or a keyword is read as a cast. **RH3101 and RH3105 fail. The spec does not cover this member.**
   - `(this.A)(5)` with a delegate field `A` becomes `(A)(5)`: a cast, CS0246.
   - `(this.S)!.Length` becomes `(S)!.Length`, which does not parse (CS1525).
   - `(base.A)(5)` becomes `(A)(5)`, CS0246.
   - Parsing also shows `(R) with { X = 1 }` as a cast, so `(this.R) with { … }` fails the same way.
   - RH3001 does not fail here (`(T)!x` becomes `(T)(x == false)`, still a cast of the same operand), and neither does RH3005. RH3101-004 and RH3105-004 currently say MUST rewrite these inputs, because no `<`/`>` is involved.

### Breaking inputs

FC-006 (rewrite does not compile) and FC-016 (sibling shapes). Matrix:

| Axis | Members | Result |
|---|---|---|
| Closing token the generic reading needs | `>` / `>>` / `>>>` (RH3001); `>` written or kept by the swap (RH3005); `>` after a qualifier removed before it or after it (RH3101, RH3105) | all fail |
| Token after `>` | `(` (written by the fix or kept), `&` (unsafe address-of on a pointer comparison: `M(G < Max, p > &x)` parses as `G<Max, p> & x`) | `(` fails in every rule. `&` parses as generic, but I did not build a compiling pointer-comparison input. The spec's "followed by `(`" leaves it out. |
| Position | argument list, array/collection element (existing test `…StaticReadonlyLeftElementBeforeAGreaterThanElement…`), `&&` chain | fail |
| RH3001 operand shape | plain, parenthesized `!(x)`, `switch` | fail (the fix always writes `(` after `>`, since `==` binds weaker than `>`) |
| Cast reading after qualifier removal (RH3101, RH3105) | followed by `(`, `!` (null-forgiving), `with` | fail |
| Cast reading, other followers | `-`, `[`, `++` | hold (parsed as parenthesized expression or element access) |
| Parenthesized `<`/`>` operands that already read generic in the input | e.g. `M(G < A, B > (!x ? 1 : 0))` | not a member: the input itself does not compile |

The test implementer must cover every failing row per rule, and one holding counter-example per requirement (the "parses the same" clause): RH3001 `B > !x` without an open `<`, and RH3105 `(this.A) - 1`.

### Surfaces

| Surface | |
|---|---|
| RH3001, RH3005, RH3101, RH3105 code fixes | must change (withhold) |
| Analyzers | must stay (report stays: SHARED-FIX-003; the change can be written by hand, so SHARED-DIAG-011 does not apply) |
| Fix All | must change with the single fix (the same guard) |
| Rule pages | not involved |
| `Reihitsu.Core` | possibly; see Architecture findings |

### Simpler option

A re-parse check is the narrowest correct guard: parse the rewritten largest expression and compare its shape with the original. It covers both readings and the `&` follower without enumerating tokens. Hand-written token conditions per rule are what produced the gap.

### Output questions

_None._ Withholding is demanded by SHARED-FIX-003 (compile error). The cast member is the same demand.

### Internal choices

- **Widen RH3101-005 and RH3105-005 to cover the cast reading** (recommended), for example "would parse as a generic name or a cast". Otherwise RH3101-004/RH3105-004 keep demanding a rewrite that does not compile, and the spec contradicts SHARED-FIX-003. Alternative: leave the cast member to a follow-up. That leaves a requirement the shipped code violates.
- **Phrase the generic condition as "would parse as a generic name" with `(` as the example, not as the only follower** (recommended for RH3001-007, RH3005-008, RH3101-005, RH3105-005). The `&` follower is real for the parser. A re-parse guard handles it at no cost.
- RH3005-008's sentence "one of the two the operator the fix writes" is garbled. The spec checker will catch it; I name it so the spec author can fix it in the same amendment.

### Issue goals

- "Withheld (or rewrites differently)": withholding is enough. Writing extra parentheses is a different output and not needed. Drop "rewrites differently"; this changes no visible output beyond what SHARED-FIX-003 demands.
- "A shared check could serve all four providers": a proposal. See Architecture findings.

### Architecture findings

**Components:** RH3002's analyzer (`RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.cs`, the helpers around line 271 and line 787, which decide whether removing parentheses would form a generic name) and the four code fixes of this issue (RH3001, RH3005, RH3101, RH3105).

**Shared decision:** "does the rewritten token sequence still parse as the same operators and operands?"

**Evidence:** today one copy exists (RH3002) and four places omit it. A narrow per-provider guard would add four more copies of one rule, which is the case `squad/README.md`, *Architecture findings*, routes to a structural change: "the narrow fix would itself add a copy". RH3201 (#1281, token merge) needs the same question at token level: "does the text re-lex to the same tokens?"

### Follow-up candidates

_None_ beyond what is folded in above.

## thoenissen/Reihitsu#1279

### Premise

Real, within the scope the triage comment left (cast and binary operands, and layout; `static`/`async` and parenthesizing are #1436).

| Input | Fix output today | Compiles |
|---|---|---|
| `(Func<int, int>)delegate (int x) { return x + 1; }` | `(Func<int, int>)(int x) => x + 1` | no (parse errors) |
| `delegate () { return 1; } + h` (`Func<int> h`) | `() => 1 + h` | no (CS0019) |
| `g + delegate () { }` (`Action g`) | `g + () => { }` | no (CS1525) |
| `g ?? delegate () { }` | `g ?? () => { }` | no (CS1525) |
| `delegate () { } + g` | `() => { } + g` | yes, same tree, but a new warning CS8848 ("Operator '+' cannot be used here due to precedence") |
| multi-line `= delegate (int x)` / `{` on the next line | `= (int x) => {`, block kept at its old columns | layout differs from the formatter (`scripts/format.sh` puts `{` on the next line, aligned under `(int x)`) |

Spec author's item 1, confirmed: the right-operand case `g + delegate () { }` → `g + () => { }` does not parse (CS1525 at `)`), and neither does `??`.

### Mechanism and candidates

A lambda is admitted only at the assignment-expression level of the grammar, while an anonymous method is a primary expression. Wherever the anonymous method is an operand of a cast or a binary operator, the lambda either does not parse (cast operand, right operand) or takes the rest of the expression into its expression body (left operand). Dispatch: the parent node of `AnonymousMethodExpressionSyntax`.

| Parent | Result |
|---|---|
| `CastExpression` | fails |
| `BinaryExpression`, right operand (`+`, `-`, `??`) | fails |
| `BinaryExpression`, left operand, expression body | fails |
| `BinaryExpression`, left operand, block body | compiles with CS8848 |
| `is`/`as` operand | not a member: CS0837 already in the input |
| `ConditionalExpression` branch (`b ? () => 1 : h`, `b ? h : () => 1`) | holds |
| switch-expression arm, `ParenthesizedExpression`, simple and compound assignment (`=`, `+=`, `??=`) | hold |
| unary operand, member access, invocation of the anonymous method | not members: the input does not compile |

### Breaking inputs

FC-006, FC-027 (multi-line form for the layout part). The test implementer needs one test per failing row and the holding rows that RH3004-005 names as counter-examples: parenthesized operand and compound assignment. Add a conditional branch, so a guard that inspects "any expression parent" is caught (FC-001).

### Surfaces

| Surface | |
|---|---|
| RH3004 code fix | must change (withhold; layout per Q1 and SHARED-FIX-007) |
| Analyzer | must stay |
| Rule page | not involved: `documentation/rules/RH3004.md` shows only an expression-bodied correction, which neither Q1 option changes |

### Simpler option

_None._ RH3004-003 already excludes these parents from its MUST, so the guard follows the existing condition.

### Output questions

**Q1 (spec author's question 1): I agree it is an output question, and I agree with A.**

Two unchanged requirements disagree:

- RH3004-003 says "the block, unchanged" and its example keeps `(int x) => { Console.WriteLine(x); }` on one line.
- SHARED-FIX-007 says the code a fix changes is laid out as the formatter lays it out.

The multi-line case the issue reports shows that the block is changed code: to reach the formatter layout, the fix must move every line of the block to the lambda's alignment column. A block on one line is the same node, so SHARED-FIX-007 reads A. The moved-code bullet does not apply: the block is not moved to another position or into another block; the node around it is replaced.

Additional evidence for the maintainer:

- The input `delegate (int x) { Console.WriteLine(x); }` is itself reported by RH5417 today, with no fix (RH5417-004). Under B the RH5417 report survives the RH3004 fix unchanged. Under A it is gone.
- Formatting the B output changes it (verified with `scripts/format.sh`: it breaks and aligns the block exactly as in A).

Whichever is chosen, RH3004-003's "the block, unchanged" must become "the block's statements, unchanged". Even under B, the multi-line block's columns change.

### Internal choices

- **Spec author's item 2, RH3004-005 withholding every binary operand.** I confirm the choice. The only binary-operand member that compiles today, the left operand with a block body, gains warning CS8848. Withholding it is the conservative reading of the triage comment ("cast and binary operands"), and a guard that keeps it would ship a new compiler warning, which fails a `TreatWarningsAsErrors` build. This member does change visible output (fix offered today, withheld after). I consider it covered by the triage comment. If the lead reads the triage comment more narrowly, the alternative is to except the block-bodied left operand, which keeps today's output and its CS8848.
- RH3004-005's "Why" mentions only compile errors and the swallowed body. Adding "or draws CS8848" for the left block-bodied operand makes the requirement match its own scope.

### Issue goals

- `static` kept and parentheses written: already moved to #1436 by triage. No change.
- "Each input behaves as stated": the cast and binary inputs are withheld rather than parenthesized (triage). The layout input follows Q1.

### Architecture findings

_None._

### Follow-up candidates

_None._ #1436 already holds the rest.

## thoenissen/Reihitsu#1281

### Premise

Real for all four rules (RH3103 part is #1417):

| Rule | Input | Fix output today | Effect |
|---|---|---|---|
| RH3005 | `if (1 == 1 == b)` | `if (b == 1 == 1)` | CS0019 |
| RH3101 | `base.M()`, `M` virtual, unsealed `D` | `M()` | compiles, but now dispatches virtually: a type derived from `D` that overrides `M` changes what runs |
| RH3106 | `Run(delegate () { })` with `Run(Action)` and `Run(Action<int>)` | `Run(delegate  { })` | CS0121 |
| RH3106 | `var a = delegate () { };` | `var a = delegate  { };` | CS8917 |
| RH3201 | `return/**/a;` | `returna;` | CS0103 |

### Mechanism and candidates

- **RH3005.** The swap regroups when the left operand is an unparenthesized comparison of the same precedence level as the outer comparison. Equality inside equality is the only level that compiles with `bool`; relational inside relational needs user-defined operators.

  | Operand shape | Result |
  |---|---|
  | `1 == 1 == b`, `1 != 2 == b` | fail |
  | `true == a < b` → `a < b == true` (verified compiles) | holds |
  | `1 < 2 == b` → `b == 1 < 2` (same grouping) | holds |
  | right operand that is a comparison of the same level | impossible without parentheses (left-associative) |

  RH3005-007 as written is right.

- **RH3101.** `base.X` is a non-virtual call and `X` a virtual one. They differ when X is overridable and the containing type can be derived from. RH3101-007's condition (virtual or non-sealed override, containing type not sealed) is right.

  | Case | Result |
  |---|---|
  | `sealed` type, virtual member and virtual indexer (`base[0]` → `this[0]`) | holds; the fix output compiles and keeps dispatch |
  | struct (implicitly sealed), `base.GetHashCode()` | holds |
  | intermediate override in a base class | holds: both calls reach the most-derived override, which is the same one |

- **RH3106.** Without a parameter list, an anonymous method has no natural type and converts to every delegate parameter list. Two positions differ:
  1. Where binding depends on the parameter list (arguments), the fix changes overload resolution.
  2. Where the position needs a natural type (`var`, `Delegate`, `object`), the fix cannot compile.

  Argument members:

  | Input | Result | Covered by RH3106-002? |
  |---|---|---|
  | `Run(Action)` + `Run(Action<int>)` | CS0121 | yes |
  | `new Thread(delegate () { })` | CS0121 | yes |
  | `base(delegate () { })` with two constructors | CS0121 | **no: a constructor initializer is not "an invocation or an object creation"** |
  | `Run(Action<int>)` + `Run(object)` | **compiles, binds to the other overload** (`Run(object)` before, `Run(Action<int>)` after: behavior change, no error) | yes ("another method") |
  | `T Run<T>(T t)` called as `Run(delegate () { })` | **CS0411, type inference fails** | **no: neither another method nor an ambiguity** |
  | `Run(Action)` + `Run(Func<int, int>)` | same binding; the fix is safe | holds |

  Natural-type members (Q2): `var a = …`, `Delegate d = …`, `object o = …` all give CS8917. The target-typed conditional `Action a = b ? delegate () { } : null` holds.

- **RH3201.** Removing the comment makes the tokens on both sides touch; when they lex as different tokens, the code changes. The class has three axes:

  | Axis | Members | Result |
  |---|---|---|
  | Token pair | `return/**/a` → `returna`, `int/**/x` → `intx`, `a+/**/+b` → `a++b` | all fail |
  | Token pair | `a +/* */b` → `a +b` | holds |
  | Comment shape | `/**/` on one line | fails |
  | Comment shape | **`/*` line break `*/` between tokens (`return/*⏎*/a` → `returna`)** | **fails; RH3201-006 says "on its line" and so leaves it out** |
  | Number of comments | one | fails |
  | Number of comments | **two adjacent empty comments `return/**//**/a`** | **both reported; one by one, the second fix produces `returna`** (verified). With Fix All, both removals are computed on the original text, and with the current wording neither comment is "all that separates" the tokens, so neither is withheld. |

### Breaking inputs

FC-006, FC-008 (Fix All, RH3201), FC-016. Required tests beyond the spec examples:

- RH3106: the overload that silently changes (`Action<int>` + `object`); the generic-inference case; a constructor initializer.
- RH3201: the multi-line empty comment; two adjacent comments through Fix All.
- RH3005: the holding counter-example `true == a < b`.
- RH3101: the struct and the sealed-indexer holds.

### Surfaces

| Surface | |
|---|---|
| RH3005, RH3101, RH3106, RH3201 code fixes | must change |
| Analyzers | must stay, except RH3106 if Q2 is answered A (then the analyzer must change for natural-type positions) |
| Rule page RH3101.md | must change (see Internal choices) |
| Fix All (RH3201) | must change (see above) |

### Simpler option

RH3201: the decidable condition is "the tokens on both sides would re-lex differently once every comment between them is removed". It is simpler than "all that separates … on its line" and covers both the multi-line and the Fix All member. It is the token-level twin of the #1257 re-parse check.

### Output questions

**Q2 (spec author's question 2): I agree it is an output question, and I agree with A.**

SHARED-DIAG-011 names "a position that requires a compile-time constant" as a position that rules a change out by itself. An implicitly typed local, or a local of type `Delegate` or `object`, requires a natural type in the same way, and the error does not come from another declaration. The argument positions are different: there the error comes from other overloads or a generic signature, so they stay reported, fix withheld.

It is still a question because the issue, and the 2026-10-23 triage comment on #1281 ("the RH3005, RH3101, RH3106 and RH3201 instances are unchanged"), keep "report stays, fix withheld" for `var`. Departing from that changes a diagnostic.

Two facts for the maintainer:

- The case exists only from C# 10. Before C# 10, `var a = delegate () { };` is itself CS0815.
- The answer also decides `Delegate d = delegate () { };` and `object o = delegate () { };`, both verified CS8917 after the fix.

### Internal choices

- **Widen RH3106-002 (recommended)**, in two respects:
  - Positions: any argument whose target is chosen by overload resolution or type inference — invocation, object creation including target-typed `new(…)`, constructor initializer, element access.
  - Condition: "would not bind to the same method or constructor: another one, an ambiguity, or none (failed type inference)".

  As written, the spec would let the fix produce CS0411 and CS0121 (constructor initializer), which SHARED-FIX-003 forbids.
- **Widen RH3201-006 (recommended)**: drop "on its line" and condition on "only comments separate the two tokens". Otherwise the multi-line and Fix All members ship broken.
- **RH3101-007 (recommended as written).** Spec-author choice, confirmed.
- **RH3101.md.** I agree with removing `virtual` from both blocks. The alternative, sealing `Derived`, keeps `virtual` but makes the example about sealing, which is not the rule's point. See the follow-up about CS0060 in the same example.
- **RH3005-007 regroup-only, and RH3201-006 merge-only.** Spec-author choices, confirmed: both keep today's safe fixes.

### Issue goals

- RH3106 `var` "report stays": in question, Q2.
- RH3103 instances: moved to #1417 by triage. No change.

### Architecture findings

RH3201 joins the #1257 finding: the same "does the rewritten text still lex and parse the same?" decision.

### Follow-up candidates

- Spec completeness, nothing visible in question: RH3106-001 states the report only for initializers of delegate-typed variables, while the analyzer reports every empty list. The spec author already names this. After Q2 it should be widened to today's scope.
- RH3201-005 gaps the spec author named. I add nothing.

## thoenissen/Reihitsu#1282

### Premise

Real. Every comment, and a directive pair, is deleted:

| Rule | Input | Fix output today |
|---|---|---|
| RH3101 | `base /* a */ . /* b */ M()` | `M()` |
| RH3104 | `new int(/* x */)` | `default(int)` |
| RH3105 | `this /* a */ . /* b */ _a` | `_a` |
| RH3106 | `delegate ( /* none */ ) { }` | `delegate  { }` |
| RH3107 | `[Obsolete(/* none */)]` | `[Obsolete]` |
| RH3107 | `[Obsolete(` + `#if DEBUG` + `#endif` + `)]` | `[Obsolete]` (both directives dropped) |

Counter-examples that hold today:

- `base /* a */ [0]` → `this /* a */ [0]` (the comment stays).
- `[Obsolete /* x */ ()]` → `[Obsolete /* x */ ]` (the comment stays).
- `delegate () /* y */ { }` → `delegate  /* y */ { }` (the comment stays).

### Mechanism and candidates

Each fix rebuilds the construct from some of its tokens: `RemoveNode(…, KeepExteriorTrivia)` for the parameter and argument lists, and a new node from the member name or the type for the others. Trivia interior to the dropped tokens is discarded. The dispatch is the trivia position relative to the dropped tokens:

| Trivia position | Result |
|---|---|
| leading or trailing on a dropped token (`base`, `this`, `.`, `new`, `(`, `)`) | dropped: fails |
| exterior of the replaced node | kept: holds |

The spec's three per-rule requirements and SHARED-PARENS-003 cover exactly the interior positions. The directive member is covered by "comment or a preprocessor directive" in each.

### Breaking inputs

FC-004, FC-005 (directives, not only comments: the RH3107 directive case above), FC-019, FC-020. Each requirement needs a directive test next to the comment test, and the exterior counter-examples above.

### Surfaces

| Surface | |
|---|---|
| RH3101, RH3104, RH3105, RH3106, RH3107 code fixes | must change |
| Analyzers, rule pages | not involved |

### Simpler option

The issue proposes doing RH3106 and RH3107 on their shared removal. SHARED-PARENS-003 does that on the spec side.

### Output questions

_None._ SHARED-FIX-003 demands withholding.

### Internal choices

_None._

### Issue goals

_N/A._ The goals stand.

### Architecture findings

_None._

### Follow-up candidates

- **RH3106 and RH3107 fixes leave the whitespace of the removed list behind.**
  - `delegate () { }` → `delegate  { }` (two spaces). This output is pinned by the existing tests at `RH3106…AnalyzerTests.cs` lines 153, 184, 219, and 253.
  - `[Obsolete /* x */ ()]` → `[Obsolete /* x */ ]`.

  The formatter removes the extra space (verified with `scripts/format.sh`), so the outputs violate SHARED-FIX-007, an unchanged requirement. Nothing is in question. The fix is small and lies in the two components this run changes, so it qualifies as an adjacent fix (`squad/README.md`, *Scope*) if the lead wants it. Otherwise it is a follow-up.

## thoenissen/Reihitsu#1417

### Premise

Real. Today's outputs for each position:

| Input | Today |
|---|---|
| `nameof(System.Nullable<int>)`, `nameof(global::System.Nullable<int>)`, `nameof(System.Nullable<int>.HasValue)`, `System.Nullable<int>.Equals(1, 2)`, `global::System.Nullable<int>.Equals(1, 2)` | reported, no fix |
| `Nullable<int>.Equals(1, 2)` | fix writes `int?.Equals(1, 2)` (invalid result) |
| `nameof(Nullable<int>.HasValue)` | fix writes `nameof(int?.HasValue)` (invalid) |
| `typeof(Nullable<>)` | fix writes `typeof(?)` (invalid) |
| `using static System.Nullable<int>;` | fix writes `using static int?;` (invalid) |
| `using N = System.Nullable<int>;` | reported and fixed to `using N = int?;`. The analyzer has no language-version check, so it is reported below C# 12 too; verified that `using N = int?;` is CS9058 in C# 11 and compiles in C# 12. |

Stays correctly reported and fixed:

- `nameof(List<Nullable<int>>)`
- `typeof(Dictionary<Nullable<int>, int>)`
- `using L = …List<System.Nullable<int>>` (`List<int?>` compiles in a `using static` and an alias in C# 11)
- unqualified `nameof(Nullable<int>)`: not reported (RH3103-002)

### Mechanism and candidates

Dispatch: the parent of the outermost name around `GenericName` (`RH3103UseShorthandForNullableTypesAnalyzer.ShouldSkip`). Only the unqualified `nameof` argument is skipped. I walked the other parents where a type can stand:

| Parent | `T?` there | Result |
|---|---|---|
| `MemberAccessExpression` receiver | invalid | fails |
| `QualifiedName` inside a `nameof` argument | invalid | fails |
| `TypeOf` with an omitted argument | invalid | fails |
| `UsingDirective` with `static` | invalid | fails |
| `UsingDirective` alias target below C# 12 | invalid | fails |
| `is` type operand (`o is int?`) | compiles | holds |
| `is` with a conditional after it (`o is int? ? 1 : 0`) | compiles | holds |
| `as` operand | compiles | holds |
| cast | compiles | holds |
| `default` | compiles | holds |
| array element | compiles | holds |
| type argument | compiles | holds |
| a type pattern (`o switch { Nullable<int> => … }`) | — | not a member: the input is already CS8116 |
| `cref="Nullable{Int32}"` | — | not reported (no diagnostic) |

RH3103-007's five bullets are exactly the failing set.

### Breaking inputs

FC-006, FC-003 (the alias at both sides of C# 12). Tests needed:

- one test per bullet;
- a type-argument counter-example inside each failing position (the spec's `nameof(List<Nullable<int>>)` and alias `List<…>`);
- the `is`/`as` holds, so a guard that skips "any non-declaration position" is caught (FC-001);
- C# 11 and C# 12 runs for the alias.

### Surfaces

| Surface | |
|---|---|
| RH3103 analyzer | must change |
| RH3103 code fix | must stay for the remaining positions; its member-access guard becomes unreachable, and whether to remove it is the implementer's call |
| Rule page | the change is right: a withheld-fix sentence does not belong there (`AGENTS.md`, rule pages) |

### Simpler option

_None._

### Output questions

_None._ SHARED-DIAG-011 is unchanged and demands it, and triage moved it to the bug-fix lane.

### Internal choices

- **Retiring RH3103-006 in the bug-fix lane.** `sdd/README.md` lists retiring a requirement among the triggers for a change doc. Here the retirement only removes a contradiction with the unchanged SHARED-DIAG-011, and the triage comment ruled out a change doc explicitly. I recommend the lead record that as a decision, so the spec checker does not flag it.
- New RH3103-007 rather than widening RH3103-002 (spec-author choice): confirmed.

### Issue goals

- "Change doc": dropped by triage.
- "Rule page states the exempt positions": drop. Rule pages carry no exceptions (`AGENTS.md`). Nothing visible beyond removing the stale sentence.
- "The fix is withheld for `typeof(Nullable<>)` / `Equals`": a stale statement. Today the fix is offered and writes invalid code (the issue comment says so too). The new requirement removes the report, so this does not matter.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1422

### Premise

Real.

**RH3104:** the fix writes `default(S)` in every statement-expression position, each CS0201:

- `new S();`
- `for (new S(); b; new S())`
- `Action a = () => new S();`
- `void N() => new S();`
- `void L() => new S();`
- `C() => new S();`
- `set => new S();`
- `Func<Task> t = async () => new S();`

The analyzer has no guard for these positions.

**RH5417:** the analyzer has no interpolation or language-version check. Compiled in C# 10 and C# 11:

| Body position | C# 10 | C# 11 |
|---|---|---|
| broken body in a `$"…"` hole | CS8967 | compiles |
| broken body in a `$@"…"` hole | compiles | compiles |
| `$@` inside a `$` hole | CS8967 | compiles |
| `$` inside a `$@` hole | CS8967 | compiles |

This confirms RH5417-005 exactly, including the spec author's request to confirm the verbatim case against C# 10.

### Mechanism and candidates

- **RH3104.** `default(S)` is not a statement expression. Every position whose grammar demands a statement expression fails. Candidates, from the places a `statement_expression` or a void-returning expression body appears:

  | Position | Result |
  |---|---|
  | expression statement; `for` initializer; `for` iterator | fail |
  | expression body of a void method, local function, or lambda | fail |
  | expression body of a constructor, a finalizer, a `set`/`init`/`add`/`remove` accessor | fail |
  | **expression body of an async function whose return type carries no value (`Task`, `ValueTask`, `async void`)** | **fails (verified for `Func<Task>`)** |
  | `_ = new S()`, `Func<S> f = () => new S()`, `return new S()` | hold |

  RH3104-005 lists "a method, a local function, an accessor, a lambda, or another function that returns no value". An async `Task` lambda "returns" a `Task`, so a reader can exclude it, and accessors that return a value (`get`) must not be covered.

- **RH5417.** Covered completely by "a non-verbatim interpolation at any depth".

### Breaking inputs

FC-002 (a qualifier — "returns no value" — that must become an explicit predicate), FC-003, FC-022 (verbatim and nested interpolated strings). Tests:

- RH3104: one per position row, including the async `Task` lambda, a constructor, and a setter.
- RH5417: C# 10 and C# 11 for each of the four nesting rows.

### Surfaces

| Surface | |
|---|---|
| RH3104 analyzer, RH5417 analyzer | must change |
| Code fixes | not involved; RH5417 offers none for lambdas |
| Rule pages | not involved |

### Simpler option

_None._

### Output questions

_None._ SHARED-DIAG-011 demands it, and triage moved it to the bug-fix lane.

### Internal choices

- **Reword RH3104-005 (recommended)** to name its scope explicitly:
  - "the expression body of a function whose body is a statement: one that returns `void`, an `async` function that returns `Task`, `ValueTask`, or another type that carries no value, a constructor, a finalizer, and a `set`, `init`, `add`, or `remove` accessor";
  - add an async `Task` lambda and a constructor to the example.
- RH3104-005 covering every statement-expression position (spec-author choice): confirmed. SHARED-DIAG-011 excludes them all.

### Issue goals

"Change doc": dropped by triage. "Both rule specs state the exempt positions": done in the specs, not on the rule pages, as is correct.

### Architecture findings

_None._

### Follow-up candidates

_None._

## Whole run

- Output questions for the lead's batch: Q1 (#1279) and Q2 (#1281), both as the spec author wrote them. I recommend A for both, with the added evidence above.
- Spec amendments I recommend before the test implementer starts (all internal, nothing visible in question):
  - RH3101-005 and RH3105-005: add the cast reading.
  - RH3001-007, RH3005-008, RH3101-005, RH3105-005: phrase the generic reading without limiting it to `(`.
  - RH3005-008: repair the garbled sentence.
  - RH3106-002: every overload-resolved argument; "binds to the same, or not at all".
  - RH3201-006: multi-line comment and adjacent comments.
  - RH3104-005: explicit async, constructor, and accessor wording.
  - RH3004-005 Why: mention CS8848.
- Architecture finding: one "rewrite re-parses (and re-lexes) the same" decision, needed by RH3001, RH3005, RH3101, RH3105 and RH3201 and already made privately in RH3002's analyzer. The lead decides whether to change the structure in this run.
- Follow-up candidates, outside the issues:
  1. **RH3106/RH3107 fixes leave a double space or a space before `]`** (SHARED-FIX-007). Nothing visible in question, and it qualifies as an adjacent fix.
  2. **`documentation/rules/RH3101.md` example does not compile.** `public class Derived : Base` with `internal class Base` is CS0060 (verified), in both blocks, before and after the spec author's edit. This is a rule-page text change. It is a visible surface, but nothing about the rule's behavior is in question; making both classes `internal` is the minimal correction. The lead decides between an adjacent fix and a follow-up.
  3. The two spec-completeness gaps the spec author named: RH3106-001's scope, and RH3201-005's line-start comment.
- No repository files besides this handover were touched. Fixtures and the Roslyn compile check lived in the session scratchpad.
