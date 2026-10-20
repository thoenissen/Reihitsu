Role: spec author
Commit: c685b96d6934e7d8b36e4368ffbc52a2b4d1567b
Verdict: spec written

This is the second repair round (`squad/decisions/2026-10-20-997-second-repair-round.md`). It covers findings 1–4 and 6 of `20-auditor.md`. Each finding was re-derived against the code. The auditor's suggested changes were treated as suggestions only. The prompt carried no intended fix or hoped-for verdict.

No ID was added, retired, or renumbered. No test and no rule page changed. The rule pages for RH3001, RH3002, RH3005, RH3101, RH3103, and RH3105 state nothing the narrowing contradicts.

## How it was established

- Probes ran with `scripts/apply-fix.sh <rule> <dir>`, under LF and CRLF, with the same result for both.
- Compile results come from the SDK's `csc.dll` against the net10.0 reference pack. Both the original text and the fixed text were compiled.
- Every input below that is called "compiles" compiled without errors. The error codes quoted are the ones csc reported for the fixed text.
- The code was read where needed:
  - `RH3103UseShorthandForNullableTypesAnalyzer.ShouldSkip` and `OnGenericName`, and `RH3103UseShorthandForNullableTypesCodeFixProvider.ApplyCodeFixAsync`;
  - `RH3001NotOperatorShouldNotBeUsedCodeFixProvider.RegisterCodeFixesAsync`;
  - `RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.IsUnnecessary`.

## Changed requirements

- **RH3103-001 — clarified.** The last sentence ("The `nameof` argument itself MUST NOT be reported (RH3103-002).") is removed. It was false for the dotted forms. The positions that -001 lists never included the `nameof` argument itself.
- **RH3103-002 — clarified.** It now reads: "MUST NOT report `Nullable<T>` when it is, written unqualified, the argument of `nameof`". The title is now "An unqualified argument of `nameof` is not reported".
  - The qualified clause is gone.
  - `nameof(System.Nullable<int>)` and `nameof(global::System.Nullable<int>)` are reported. `ShouldSkip` walks up only through `QualifiedNameSyntax` and `AliasQualifiedNameSyntax` parents. In expression context, a dotted name's parent is a `MemberAccessExpressionSyntax`.
  - The alias-only form is not kept either. `nameof(S::Nullable<int>)` with `using S = System;` does not compile: `CS8083: An alias-qualified name is not an expression`. Its `no-diagnostic` result therefore describes invalid code and gets neither a requirement nor a test.
- **RH3103-003 — clarified.** It is scoped to "one diagnostic per type in a position of RH3103-001". Without that scope it claimed the whole-name span for `nameof(System.Nullable<int>)`, whose diagnostic is on `Nullable<int>` alone (`genericName.Parent` is a member access, not a `QualifiedNameSyntax`).
- **RH3103-004 — unchanged.** Its positions do not include the `nameof` argument.
- **RH3005-004 — clarified.** It now holds only where "the largest expression that contains the comparison — the comparison itself, when it is not part of a larger expression — holds no `<` or `>` operator besides the comparison's own".
  - A new generic reading needs both a `<` token and a later `>` token. The fix contributes one of them, so the other has to come from a second operator in the same expression.
  - A chain such as `Max > G > (7)` cannot reach the fix. Its outer comparison would need a user-defined operator on `bool`, and C# does not allow one.
- **RH3001-004 — clarified.** It now holds "for a negation that is neither the whole expression of an argument or a tuple element nor the right operand of a `>` operator".
  - **The `>` part.** The only hazard is the `(` that the fix adds directly after the parent's operator. `>` is the only operator that can close a type argument list there, and only a user-defined `>` takes a `bool` operand.
  - **The argument part.** This is a separate defect found while narrowing (Q-RH3001-2 below). The fix is not offered when `!x` is the whole expression of an unnamed argument or of a tuple element. `RegisterCodeFixesAsync` calls `root.FindNode` without `getInnermostNodeForTie`, so the found node is the `ArgumentSyntax` and not the `PrefixUnaryExpressionSyntax`.
  - Probed with no fix offered: `M(!x)`, `(!x, y)`, `new C(!y)`, `this[!y]`, and `[A(!true)]`. The named argument `M(x: !y)` is fixed.
  - So -004 as written (an unconditional MUST) was also false here.
- **RH3101-004 — clarified.** It now reads "MUST replace `base[…]` with `this[…]`, and `base.X` with `X` where the largest expression that contains the access does not hold both a `<` operator and a `>` operator".
  - The element-access part keeps no condition. `this` is a keyword and cannot start a type argument.
  - The fix adds no `<` or `>` token, so both must already be there.
- **RH3105-004 — clarified.** It has the same condition as RH3101-004: "Where the largest expression that contains the access does not hold both a `<` operator and a `>` operator".
  - A list-based scope would not be enough. The chained form `A < this.B > (7)`, written with user-defined operators, breaks without any comma (Q-RH3105 input 4).
  - So would an access that is not an operand of `<` or `>`, as in `M(A < B, this.X, D > (7))`.
- **RH3002-001 — wording (finding 6).** "directly inside another pair of parentheses" now reads "directly inside another parenthesized expression". The behavior is unchanged: `IsUnnecessary` reports only for a `ParenthesizedExpressionSyntax` parent.

For each of RH3005-004, RH3001-004, RH3101-004, and RH3105-004, no example was added on the excluded side of the new condition. That side is the open question, and it stays out of the spec. RH3004-003 set the precedent for this. Each requirement's existing example lies inside the condition.

### In-scope inputs, probed

| Input | Rule | Result |
|---|---|---|
| `M(0 < a, 1)` | RH3005 | `M(a > 0, 1)` |
| `if (0 < a && b == 1)` | RH3005 | `if (a > 0 && b == 1)` |
| `M(A < this.B, D)` | RH3105 | `M(A < B, D)` |
| `if (this._a > 0)` | RH3105 | `if (_a > 0)` |
| `M(this.A > B, D)` | RH3105 | `M(A > B, D)` |
| `M(A < base.B, D)` | RH3101 | `M(A < B, D)` |
| `if (base.B > 0)` | RH3101 | `if (B > 0)` |
| `var q = !a && B > 1;` | RH3001 | `var q = a == false && B > 1;` |
| `M(x: !y)` | RH3001 | `M(x: y == false)` |

### Considered and left unchanged

- **RH3001-005, RH3001-006 (offered side), and RH3005-006 (offered side).** They state comment placement, or that a comment or directive does not withhold the fix. They do not restate the rewrite, which -004 now scopes. In the excluded inputs they still hold literally: the comment lands where stated, and the directive or comment is not what withholds or permits the fix.
- **RH3101-001/-002 and RH3105-001/-002.** The analyzer binds the bare name alone, as -001 says. The report in the generic-reading inputs is therefore what -001 states. Whether the report should stand is part of the questions below.

### Checks

`scripts/test.sh --project architecture`: 72/72 passed.

## Examples for the tests

No new requirement example. The existing tagged tests still fit:

- **RH3005-004.** Every tagged test has a single comparison per expression.
- **RH3001-004, RH3101-004, RH3105-004.** No tagged test has a `<` or `>` operator, a negation that is a whole argument, or a negation that is the right operand of `>`.
- **RH3103-002.** Both tagged tests (`NullableGenericInNameofIsNotReported`, `NullableGenericAsNameofArgumentInExpressionBodyIsNotReported`) use the unqualified form.

The test implementer adds these **untagged** characterization tests, one per open question below. Each pins the real output quoted there.

| Test file under `src/Reihitsu.Analyzer.Test/Clarity/` | Input (from the question) | Pins |
|---|---|---|
| `RH3103…Tests.cs` | Q-RH3103 inputs 1 and 2 | The report on `Nullable<int>`. The fix must not be applied: it throws `InvalidCastException`. Use an analyzer-only check, or an equivalent that does not run the code action. |
| `RH3005…Tests.cs` | Q-RH3005 inputs 1–3 | The fixed text shown |
| `RH3001…Tests.cs` | Q-RH3001-1 inputs 1 and 2 | The fixed text shown |
| `RH3001…Tests.cs` | Q-RH3001-2, at least `M(!x)` and `(!x, y)` | Reported, no code fix offered |
| `RH3105…Tests.cs` | Q-RH3105 inputs 1–4 | The fixed text shown |
| `RH3101…Tests.cs` | Q-RH3101 inputs 1 and 2 | The fixed text shown |

Findings 5 and 7 of `20-auditor.md` belong to the test implementer, and this handover does not cover them. The RH3002-001 example is unchanged. A separate test tagged RH3002-001 can cover the three missing positions without copying the spec example.

## Open questions to file

**Q-RH3103 — in thoenissen/Reihitsu#1231 (`SDD open questions: sdd/specs/analyzer/RH3103.md`). Fix part decided by principle P1 (#912), SHARED-FIX-003 (crash); report part open.**

- Inputs:
  1. `using System; class C { string M() => nameof(System.Nullable<int>); }`
  2. `class C { string M() => nameof(global::System.Nullable<int>); }`
- Real output:
  - Both are reported. The diagnostic is on `Nullable<int>` only, not on the whole dotted name.
  - Applying the offered fix throws `InvalidCastException: Unable to cast object of type 'NullableTypeSyntax' to type 'SimpleNameSyntax'`. `ApplyCodeFixAsync` replaces the `Name` of a member access with a `NullableTypeSyntax`.
- Why it looks wrong:
  - The unqualified `nameof(Nullable<int>)` is exempt because `nameof(int?)` does not compile. The dotted forms have the same constraint, so the report has no correction.
  - The fix crashes instead of being withheld.
- Competing readings for the report:
  - exempt the dotted forms like the unqualified one;
  - or report them without a fix.
- Touches RH3103-002 and RH3103-003.

**Q-RH3005 — in thoenissen/Reihitsu#1227 (`SDD open questions: sdd/specs/analyzer/RH3005.md`). Decided by principle P1 (#912), SHARED-FIX-003 (compile error).**

Each input compiles and is reported on its constant-left comparison. RH3005-005 does not withhold the fix, because both comparisons are built-in.

| # | Input | Fixed text | Why it no longer compiles |
|---|---|---|---|
| 1 | `class C { const int Max = 1; int G; int B; void M(bool x, bool y) { } void Run() { M(Max > G, B > (7)); } }` | `M(G < Max, B > (7));` | Parses as the generic invocation `G<Max, B>(7)`: `CS0307` (field `C.G` used with type arguments), `CS0246` for `Max` and `B` |
| 2 | `class C { static readonly int Min = 1; int G; int B; void Run() { var a = new[] { Min > G, B > (7) }; } }` | `new[] { G < Min, B > (7) }` | Same errors |
| 3 | `class C { int G; int A; int B; void M(bool x, bool y) { } void Run() { M(G < A, (7) < B); } }` | `M(G < A, B > (7));` | The mirrored `>` closes `G<A, B>` before `(7)`: `CS0307`, `CS0246` |

Input 3 is reported on the second comparison. In input 3 the new token is the `>`, not the `<`.

- Why it looks wrong: the swap turns two comparisons into a generic invocation, and the fix breaks the build.
- Touches RH3005-004.

**Q-RH3001-1 — new issue needed: `SDD open questions: sdd/specs/analyzer/RH3001.md`. Decided by principle P1 (#912), SHARED-FIX-003 (compile error).**

| # | Input | Fixed text | Errors |
|---|---|---|---|
| 1 | `class T { public static bool operator >(T a, bool b) => b; public static bool operator <(T a, bool b) => b; }`<br>`class C { int G; int A; T B; void M(bool x, bool y) { } void Run(bool x) { M(G < A, B > !x); } }` | `M(G < A, B > (x == false));` | Parses as `G<A, B>(x == false)`: `CS0307`, `CS0246` |
| 2 | `class U { public static bool operator >(U a, bool b) => b; public static bool operator <(U a, bool b) => b; }`<br>`class T { public static U operator <(T a, int b) => null; public static U operator >(T a, int b) => null; }`<br>`class C { T G; int A; bool Run(bool x) { return G < A > !x; } }` | `return G < A > (x == false);` | Parses as `G<A>(x == false)`: `CS0307`, `CS0246` |

- Input 2 contains no comma.
- Why it looks wrong: the parentheses that keep `x == false` grouped follow a `>`, and the parser reads them as the argument list of a generic invocation. The fix breaks the build. Both inputs need a user-defined comparison that takes a `bool` operand.
- Touches RH3001-004.

**Q-RH3001-2 — same new RH3001 issue. Not a principle instance (no fix is offered, so nothing breaks). Open: the guard's scope.**

- Inputs:
  - `class C { void M(bool x) { } void Run(bool x) { M(!x); } }`
  - `class C { void Run(bool x, bool y) { var t = (!x, y); } }`
  - `class C { C(bool b) { } static C Run(bool y) { return new C(!y); } }`
  - `class C { int this[bool b] => 0; int Run(bool y) { return this[!y]; } }`
  - `class A : System.Attribute { public A(bool b) { } } [A(!true)] class C { }`
- Real output:
  - Each negation is reported, and no code fix is offered.
  - `M(x: !y)` is fixed to `M(x: y == false)`, and `M(!x && y, y)` to `M(x == false && y, y)`.
- Why it looks wrong:
  - No comment, directive, or binding concern is involved. The rewrite `M(x == false)` is as safe as the named-argument case.
  - The fix provider finds the reported node with `FindNode` without `getInnermostNodeForTie`. When `!x` is the whole argument, the node found is the `ArgumentSyntax`, which has the same span, and the provider registers nothing.
  - A diagnostic without a fix is a supported end state (AGENTS.md, *Code-fix coverage*). Withholding the fix here gives away safe ground for no stated reason: a guard-scope question, not a defect by itself.
- Competing readings:
  - offer the fix for a negation that is a whole argument or tuple element;
  - or keep it withheld and document the gap.
- Touches RH3001-004.

**Q-RH3105 — in thoenissen/Reihitsu#1233 (`SDD open questions: sdd/specs/analyzer/RH3105.md`). Fix part decided by principle P1 (#912), SHARED-FIX-003 (compile error).**

Each input compiles and is reported on `this`.

| # | Input | Fixed text | Errors |
|---|---|---|---|
| 1 | `class C { int A; int B; int D; void M(bool x, bool y) { } void Run() { M(A < this.B, D > (7)); } }` | `M(A < B, D > (7));` | Parses as `A<B, D>(7)`: `CS0307`, `CS0246` |
| 2 | `class C { int A; int B; int D; void M(bool x, bool y) { } void Run() { M(A < B, this.D > (7)); } }` | `M(A < B, D > (7));` | Same |
| 3 | `class C { int A; int B; int X; int D; void M(bool x, int z, bool y) { } void Run() { M(A < B, this.X, D > (7)); } }` | `M(A < B, X, D > (7));` | Parses as `A<B, X, D>(7)` |
| 4 | `class U { public static bool operator >(U a, int b) => true; public static bool operator <(U a, int b) => true; }`<br>`class T { public static U operator <(T a, int b) => null; public static U operator >(T a, int b) => null; }`<br>`class C { T A; int B; bool Run() { return A < this.B > (7); } }` | `return A < B > (7);` | Parses as `A<B>(7)`: `CS0307`, `CS0246` |

- In input 3, the access is not an operand of `<` or `>`. In input 4 there is no comma.
- Why it looks wrong: the qualifier is what keeps the parser from reading a type argument list. The fix breaks the build.
- The analyzer binds the bare name alone, finds the same field, and reports. Whether the report should stand is open: the qualifier is needed here, much as RH3105-002 says of a qualifier that selects the member.
- Touches RH3105-004, and RH3105-001/-002 for the report.

**Q-RH3101 — in thoenissen/Reihitsu#1229 (`SDD open questions: sdd/specs/analyzer/RH3101.md`). Fix part decided by principle P1 (#912), SHARED-FIX-003 (compile error).**

Each input compiles and is reported on `base`.

| # | Input | Fixed text | Errors |
|---|---|---|---|
| 1 | `class P { protected int B; } class C : P { int A; int D; void M(bool x, bool y) { } void Run() { M(A < base.B, D > (7)); } }` | `M(A < B, D > (7));` | `CS0307`, `CS0246` |
| 2 | `class P { protected int X; } class C : P { int A; int B; int D; void M(bool x, int z, bool y) { } void Run() { M(A < B, base.X, D > (7)); } }` | `M(A < B, X, D > (7));` | `CS0307`, `CS0246` |

- Why it looks wrong: the same mechanism as Q-RH3105. The qualifier is what prevents the generic reading.
- Touches RH3101-004, and RH3101-001/-002 for the report.

## Follow-up candidate (not filed)

The code side of findings 2–4 is a production change, and it is out of this lane:

- a shared guard against a rewrite that creates a generic-name reading;
- the RH3103 member-access crash;
- the RH3001 `FindNode` tie.

This handover only lists them.
