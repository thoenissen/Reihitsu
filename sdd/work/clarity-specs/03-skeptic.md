Role: skeptic
Commit: be00606
Verdict: proceed narrower

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Most requirements state current behavior, but ten passages do not hold or should not be in the specs as written (A–C below), and the run found eight more open-question instances (D). Each of them goes back to the spec author before the test implementer starts. One point is a split between two principles that the maintainer has not decided (see *Questions for the maintainer*). Under the open-question rule it does not block the run.

Fixtures were run with `scripts/apply-fix.sh <rule> <dir>` (LF and CRLF, same result in both unless stated). Compile errors were checked with the SDK's `csc.dll` against the net10.0 reference pack, because `apply-fix` reports `invalid-result` only for syntax errors, not for semantic errors. So the handovers' "0 mismatches" proves the results parse, not that they compile.

## Premise

### A. Requirements that do not state current behavior

1. **RH3002-006, and RH3002-002's content list.** A pair whose content is a cast, a lambda, an anonymous method, or a `switch` expression is never reported (`IsUnnecessary` returns `false` before any other check, RH3002-003). So when nested pairs surround such content, the **innermost** pair stays and the outer pairs are reported, which is the opposite of RH3002-006 ("MUST NOT report the outermost pair and MUST report the pairs inside it"). Fixtures:
   - `var x = (((int)o));` becomes `var x = ((int)o);`
   - `return (((int)o)).ToString();` becomes `return ((int)o).ToString();`
   - `Func<int> f = ((() => a));` becomes `Func<int> f = (() => a);`

   For the receiver case, `IsSafeChainExpression` counts a pair that is not reported itself as safe content. RH3002-002's list (name, generic name, literal, …) does not include it, and the requirement says that any other content MUST NOT be reported.
2. **RH3002-001's position list.** Three places do not match the code:
   - `[Obsolete(("x"))]` is not reported. An attribute argument is an `AttributeArgumentSyntax`, not an `ArgumentSyntax`, but the spec says "an argument".
   - `void M(int a = (1))` is reported and fixed. A parameter's default value is not "a variable or member initializer".
   - `Func<int, int> f = x => (x);` and `a switch { _ => (a) }` are not reported. Only an `ArrowExpressionClauseSyntax` counts, but the spec says "an expression body (`=>`)".
3. **RH3005-005.** The fix's guard binds the text `Right op Left`, written without parentheses. `IsBuiltInComparison` treats a binding with no symbol and no candidates as built in. With `bool b`, `if (1 == 1 == b)` is reported, the fix is offered, and it writes `if (b == 1 == 1)`, which csc rejects with `error CS0019: Operator '==' cannot be applied to operands of type 'bool' and 'int'`. RH3005-005 says the fix is withheld when "the swapped comparison does not bind to a built-in operator". This is also a P1 instance (see D).
4. **SHARED-BODY-003, last sentence and first example.** "The rest of the member is laid out by the formatter in the same step" and the example that moves `: base(value)` onto its own line contradict SHARED-BODY-002's own `Why:` ("the signature of the member stays"). The behavior is failure class FC-030. Fixture for RH3203:

   ```csharp
   ///<summary>x</summary>
   [Obsolete] public  C( int value ) => _value = value;
   public C(string s) : this(s.Length) => Console.WriteLine( s );
   ```

   The fix rewrites the documentation comment to three lines and puts `[Obsolete]` on its own line. It normalizes `public  C( int value )` to `public C(int value)`, and puts `: this(s.Length)` on its own line. The diagnostic covers only `=> …`. The sentence comes from the retired RH3202-004. Whether it may stay is the open question below.
5. **SHARED-BODY-001 restates FMT-STRUCT-007.** It repeats the list of conditions (a directive before the expression, an unbalanced conditional, a region in the span or in the leading trivia of `=>`, the balanced-group exception). `sdd/README.md` says an analyzer spec references a formatter-owned policy by its FMT ID instead of restating it. The retired RH3202-002 only referenced FMT-STRUCT-007.
6. **SHARED-EMPTY-002, "keep the trivia in front of `(` and after `)`".** For `delegate () { }`, keeping the whitespace on both sides is exactly the double space of the spec author's SHARED-EMPTY Q2, which is marked P10. As a general sentence, it states the outcome that P10 decides against. Keeping comments is not affected.

### B. Requirements or examples that state behavior a decided principle calls a bug

7. **RH3101-004's example.** It uses `public virtual int M()` in `B` with an unsealed `D`, and the fix writes `base.M()` → `M()`. That is exactly the RH3101 Q1 case (P1, run time) that the handover files as a bug. As an example of a fix, it must use a member that is not virtual, or a sealed type. Reporting `base.M()` (RH3101-001) is fine, because the diagnostic stays under SHARED-FIX-003.
8. **RH3201-002, "A `///` line does not count as such a line".** Under P13, a `///` that documents nothing is an ordinary comment. Fixture inside a method body:

   ```csharp
   // First.
   //
   /// Second.
   M();
   ```

   The empty `//` is reported and the fix removes it. The test is `DocumentationAnalysisUtilities.IsDocumentationLine`, which looks at the text of the line, not at whether the comment documents anything. File the case as "Decided by principle P13 (#912)", and narrow the sentence to a `///` comment that documents a declaration. The `RH3201-002` example (`/// <summary>` above a field) is unaffected.

### C. Cases the specs state as not reported that look wrong (P11, the rule's own gap)

9. **RH3003-002, "a `case` label".** `case var x when x == "":` is not reported, because any `CasePatternSwitchLabelSyntax` ancestor exempts the literal, including its `when` clause. The same literal in a switch-expression arm, `s switch { var y when y == "" => 2, _ => 3 }`, is reported and fixed to `string.Empty`. The `Why:` ("these positions need a compile-time constant") does not hold for a `when` clause. Narrow the wording to the constant or pattern of a `case` label, and file the `when` clause marked P11.
10. **RH3201-002 and a trailing empty `//`.** In the code below, the empty `//` after `int _a;` is not reported:

    ```csharp
    // First.
    int _a; //
    // Second.
    ```

    The check only requires the lines above and below to start with `//`, not that the empty comment stands alone on its line. The `Why:` (a paragraph break) does not hold here. Narrow the requirement to an empty `//` alone on its line, and file the trailing case marked P11.

### D. Open-question instances that are not in the handovers

Reproduced unless a bullet says otherwise. P1 means the fix part is decided by P1 / SHARED-FIX-003.

- **RH3004, P1 (compile error).** `var f = (Func<int, int>)delegate (int x) { return x + 1; };` becomes `var f = (Func<int, int>)(int x) => x + 1;`, which `apply-fix` reports as `invalid-result`. A lambda cannot stand as a cast operand without parentheses. Touches RH3004-003.
- **RH3005, P1 (compile error).** The `1 == 1 == b` case from A.3. Touches RH3005-004 and RH3005-005.
- **RH3103, P1 (fix part).** These have the same mechanism as Q1 (`T?` written where it cannot stand) and can be added to it:
  - `nameof(Nullable<int>.HasValue)` becomes `nameof(int?.HasValue)` (`invalid-result`);
  - `bool M() => Nullable<int>.Equals(1, 2);` becomes `int?.Equals(1, 2)` (`invalid-result`);
  - `using N = System.Nullable<int>;` becomes `using N = int?;`, which csc rejects under C# 11 with `CS9058: Feature 'using type alias' is not available in C# 11.0` (a language-version gate, FC-006).

  Touches RH3103-001 and RH3103-004.
- **RH3106, P1 (fix part).** `var a = delegate () { }; a();` becomes `var a = delegate  { };`, and csc reports `error CS8917: The delegate type could not be inferred`. This is the same mechanism as Q1 (the conversion target is not fixed) and can be added to it. Here the parentheses are needed, so the report part is open as well. Touches RH3106-001 and SHARED-EMPTY-002.
- **RH3201, P1 (compile error).** `return/**/a;` becomes `returna;`. Removing a comment that separates two tokens joins them. Touches RH3201-005.
- **RH3002 Q1 (P11), more instances of the same gap.** None of these is reported: an attribute argument `[Obsolete(("x"))]`, a lambda body `x => (x)`, and a switch-expression arm `_ => (a)` (all run). From the code, a receiver whose content is not in `IsSafeChainExpression`'s list is not reported either, for example `(typeof(int)).Name`.
- **Optional, `shared/code-fixes.md` (not run).** RH3001 (`a => !a` → `a => a == false`), RH3005, RH3003, and RH3104 also rewrite inside expression trees, where the tree that the program inspects changes shape (`Not` → `Equal`, `New` → `Default`). Whether SHARED-FIX-003's "run time" covers this is a reading for the maintainer, not a principle instance. A question about shared behavior belongs in the shared spec's issue.

### E. The handovers' open questions

I checked every question against the code. The markings hold:

| Question | Marking | Note |
|---|---|---|
| RH3002 Q1 | P11 | |
| RH3004 Q1 | none | Re-run. Dropping `static` changes neither compilation nor run time, so P1 does not apply. |
| RH3004 Q2 | P10 | |
| RH3006 Q1 | none | No shared requirement states the line ending of text inserted by this kind of fix. |
| RH3101 Q1, Q2 | P1 | |
| RH3102 Q1 | P10 | P9 would fit as well, since the next statement joins the line. |
| RH3103 Q1 | P1 for the fix part | |
| RH3104 Q1 | P1 | |
| RH3105 Q1 | P1 | |
| RH3106 Q1 | P1 for the fix part | |
| SHARED-EMPTY Q1 | P1 | |
| SHARED-EMPTY Q2 | P10 | |
| RH3201 Q1 | P13 | |
| RH3201 Q2 | P9 | |
| RH3201 Q3 | P10 | |
| RH3202 Q1 | none | As the maintainer decided (decision 4). |

### F. Claims I tried to break that hold

- **RH3001-004.** `!a == b` becomes `a == false == b`, and `!a != b` becomes `a == false != b`. No parentheses are added where the grouping is unchanged.
- **RH3005-004/-005.**
  - `string` operands (`null == s`, `"x" == s`, `string.Empty == s`) bind to a built-in operator and get the fix.
  - `DateTime.MinValue < d` is reported without a fix.
  - `1 + 2 == a` becomes `a == 1 + 2`.
- **RH3104.** `int? a = new();` becomes `default(int)`, and `S? b = new();` becomes `default(S)`. The value is the same, because the created type is the underlying type.
- **RH3003-001.** An empty raw string literal does not parse, so "regular or verbatim" is complete.
- **SHARED-BODY.** RH3202 and RH3203 keep private copies of the analyzer and fix and do not derive from the base classes. They match the bases line for line: the same `BlocksRewrite` guard, `ArrowExpressionClause` location, `FormatNodeInDocumentAsync`, and `BatchFixer`. FC-015 has no drift today.
- **SHARED-BRACE-001.** `BlankLineCodeFixUtilities.InsertBlankLineBefore` inserts after the last directive in the leading trivia (`FindIndexAfterLeadingDirectives`), with `DetectEndOfLine`. That matches the text, and the text is RH5030-003's unchanged.
- **Ownership.** No new spec claims `SyntaxNodeUtilities`, `SpeculativeRebindingHelper`, or `DocumentationAnalysisUtilities`. `EmptyParenthesesAnalyzerBase<TNode>`, `ExpressionBodyAnalyzerBase<TNode>`, `ExpressionBodyToBlockCodeFixProviderBase<TNode>`, and `BlankLineCodeFixUtilities` have no consumers outside RH3106/RH3107, RH3205–RH3208, and RH5030/RH3208.
- **Coverage notes in `01-spec-author.md`.** They hold: RH3106 and RH3107 have one test each, and there is no test for a labeled RH3102 statement, for `b == (a == false)`, or for trivia around RH3006's wrapped operand.

## Mechanism and candidates

_N/A_: this is a spec of existing behavior, not a bug. For the dispatch-derived candidate sets:

- RH3002 decides by the parent of the `ParenthesizedExpressionSyntax`: `ParenthesizedExpression`, `Return`, `Throw`, `ThrowExpression`, `EqualsValueClause`, `ArrowExpressionClause`, `Argument`, `AssignmentExpression`, the receiver of a `MemberAccess`, `Invocation`, or `ElementAccess`, and `Await`. Any other parent is not reported.
- RH3003 skips the ancestors `AttributeArgument`, `ConstantPattern`, `CaseSwitchLabel`, `CasePatternSwitchLabel`, and `GotoStatement`, `const` field and `const` local declarations, and a parameter's `EqualsValueClause`.
- RH3103 reports every `GenericNameSyntax` that binds to `Nullable<T>`, whatever its syntactic position. That is the source of the expression-position instances in D.

## Breaking inputs

For the test implementer:

- **FC-003.** Test each fix-withholding guard on both sides of its boundary: RH3001-006 (a directive before the operand versus inside its parentheses), RH3002-009, RH3004-004, RH3005-006, RH3102-004, and RH3103-005 (a block comment on the same line versus on its own line).
- **FC-019, FC-020, FC-021.** Put a comment and a directive in each touched gap. Include RH3201-005 with a comment that starts a line before code (Q2), and SHARED-BODY-001 with `#region` in the leading trivia of `=>`.
- **FC-006.**
  - RH3005-005: a user-defined operator, `dynamic`, and a `string` operand (fix offered).
  - RH3204-001: `FormattableString`, `IFormattable`, and `object`.
  - RH3104-002: a struct with an explicit parameterless constructor, and `new T()`.
  - RH3001-002: `bool?` and a type with its own `operator !`.
- **FC-022.** RH3204-003 with verbatim strings, raw strings, `$$` raw strings, and `{{`.
- **FC-025.** SHARED-BODY across all six reported kinds: an `async` method, an instance compound-assignment operator, `explicit checked`, a static constructor, and a local function in a lambda and in top-level statements.
- **FC-027.** RH3002 in each position of the dispatch list above, and the nested shapes of A.1 after the spec is corrected.
- **FC-030.** Pin the current member-wide layout of the SHARED-BODY fix (A.4) only as an untagged characterization test until the open question is decided.
- **FC-033.** Run RH3006 Q1 under CRLF. The drift is in the CRLF arm only.
- **FC-007, FC-008.** Keep the second Fix All pass of RH3001, RH3002, RH3004, and RH3102 (thoenissen/Reihitsu#1143) as characterization tests, without a requirement tag.
- **Instances from B–D.** Do not tag any instance of B–D as a requirement. A test that pins one is a characterization test.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers RH3001–RH3208 | must stay |
| Code fixes and Fix All | must stay |
| `Reihitsu.Core` (`ExpressionBodyRewriteUtilities`, `SyntaxNodeUtilities`) | must stay |
| Formatter | not involved (must stay) |
| Specs | must change: A.1–A.6 (corrections), B.7–B.8 (example and narrowing), C.9–C.10 (narrowing) |
| Rule pages | `documentation/rules/RH3201.md` must stay consistent with the narrowed RH3201-002. `documentation/rules/RH3101.md` has the virtual-member example of B.7 (follow-up candidate; it matches today's fix). |
| Tests | must change: tags, new tests, and re-tagging as listed in the handovers |
| Open-question issues | must change: add the instances from C and D |

## Simpler option

SHARED-BODY-001 can reference FMT-STRUCT-007 instead of restating its conditions (A.5). Otherwise _None._

## Questions for the maintainer

**Is the rest of the member code that the SHARED-BODY fix's diagnostic is about?** The diagnostic covers `=> expression`. Input (RH3203):

```csharp
[Obsolete] public  C( int value ) => _value = value;
```

- **Reading (a), P9 (fix scope).** Only `=> expression ;` becomes the block, and the rest keeps its layout: `[Obsolete] public  C( int value )`, followed by the block on the next lines.
- **Reading (b), P10 (fix layout), the current behavior.** The member is the code the fix changes and is laid out as a whole: `[Obsolete]`, then `public C(int value)` on its own line, then the block. The documentation comment and the constructor initializer are relaid out too.

This is the same split as RH3202 Q1 (two blank lines above the member), which the maintainer chose to file without a principle marking (decision 4). By that precedent, it is non-blocking: SHARED-BODY-003 leaves out its last sentence, picks an example without relayout outside the body, and the question is filed in `SDD open questions: sdd/specs/shared/expression-body-conversion.md`.

The retired RH3202-004 carried the same sentence before P9 was decided. If the lead reads dropping the sentence as narrowing a statement the maintainer approved for retirement into SHARED-BODY (`squad/decisions/2026-10-19-997-rh3202-shared-body.md`), this is escalation 1 instead.
