Role: skeptic
Commit: 38d3ded
Verdict: #1283: proceed (narrower tags: SHARED-FIX-006 governs the reported line break); #1284: proceed; #1285: output question (one unlisted position), otherwise proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. This handover states none.

All probes ran with `scripts/apply-fix.sh` (every fixture under LF and CRLF) or `scripts/format.sh`, on fixtures in the scratchpad, outside the repository. The fixtures have been deleted. `apply-fix` takes the first diagnostic of a fixture only, so each position below was its own fixture.

## thoenissen/Reihitsu#1283

### Premise

Real, and reached by a caller: the RH3006 code fix, applied in an IDE or by `apply-fix`. Reproduced on `38d3ded` with the issue's input in a CRLF document:

```csharp
        return a ||
            /* x */ b && c;
```

Result: `return a ||` / `/* x */ (b && c);`, flagged `line-ending-drift`, `CRLF=7, LF=1`. The LF arm is clean.

**The issue names the wrong requirement for its own input.** The one LF is the line break that ends `return a ||`. That line break is the trailing trivia of `||`, so it lies outside the node the fix replaces, which is the operand with its own trivia (`WithTriviaFrom`). SHARED-FIX-008 says itself that a line break outside the changed span "is not covered by this requirement". The governing requirement is unchanged SHARED-FIX-006: the fix changed code its diagnostic is not about. SHARED-FIX-008 governs only the member where a line break inside the operand's leading trivia also flips (the blank-line row below). The spec author's test example tags SHARED-FIX-008 and RH3006-003. It should tag SHARED-FIX-006 and RH3006-003, plus SHARED-FIX-008 for the blank-line case. No spec text has to change.

### Mechanism and candidates

Mechanism, as observed: **when the leading trivia of the operand that the RH3006 fix puts in parentheses holds a comment, every line break between the preceding token and that comment is written as LF in a CRLF document.** Those line breaks are the preceding token's trailing line break and the line break of any blank line in front of the comment.

`ApplyCodeFixAsync` in `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3006ConditionalExpressionsMustDeclarePrecedenceCodeFixProvider.cs` writes no line break itself. It only wraps the node with `SyntaxFactory.ParenthesizedExpression`/`ParenthesizedPattern(...).WithTriviaFrom(...)`. So the LF appears while the code action's operations are computed, after the fix's own code returns. That is an inference from the code plus the runner (`FixtureRunner.ApplyAsync` reads `GetOperationsAsync`). The written LF matches the host's line break on this Linux runner.

The fix code dispatches on `node switch { ExpressionSyntax, PatternSyntax }`. The axes are the branch, the operand side, and where the trivia sits. Each member below was probed under CRLF:

| Member | Input (shape) | Result |
|---|---|---|
| expression, right operand, comment on the continuation line (the issue) | `a \|\|` ⏎ `/* x */ b && c` | **fails**, 1 LF |
| expression, right operand, comment on its own line | `a \|\|` ⏎ `// x` ⏎ `b && c` | **fails**, 2 LF |
| expression, right operand, blank line then comment | `a \|\|` ⏎ ⏎ `/* x */ b && c` | **fails**, 2 LF (one inside the operand's trivia → SHARED-FIX-008) |
| expression, right operand as in the issue, `;` on its own line | `a \|\|` ⏎ `/* x */ b && c` ⏎ `;` | **fails**, 1 LF |
| expression, left operand, comment after `return` ⏎ | `return` ⏎ `/* x */ a && b \|\| c` | **fails**, 1 LF |
| expression, left operand, whole argument | `M(` ⏎ `/* x */ a && b \|\| c)` | **fails**, 1 LF |
| pattern operand | `i is 1 or` ⏎ `/* x */ 2 and 3` | **fails**, 1 LF |
| comment on the same line as the preceding token | `a \|\| /* x */ b && c;` | holds |
| line comment trailing the preceding token | `a \|\| // x` ⏎ `b && c` | holds |
| comment ends the previous line, operand clean | `return /* x */` ⏎ `a && b \|\| c` | holds |
| no comment | `a \|\|` ⏎ `b && c` | holds |
| comment inside the operand | `b /* x */` ⏎ `&& c` | holds |

Not settled here: a document whose line breaks are LF on a host whose line break is CRLF (Windows). Only Linux was available. CI runs on `ubuntu-latest`, so a CRLF test fails there before the fix.

### Breaking inputs

- FC-016: sibling shapes. Every failing row above, including the pattern branch, is a candidate input for the test implementer.
- FC-017 and FC-033: assert the exact text with its line endings in both an LF and a CRLF document. `VerifyCodeFixWithEndOfLine` in `src/Reihitsu.Analyzer.Test/Base/AnalyzerTestsBase{TAnalyzer,TCodeFix}.cs` already does that.
- FC-034: use the issue's exact shape: a block comment on the continuation line in front of the right operand.
- FC-019: a gap that holds a comment.
- FC-008: Fix All, with two reported operands in one document, each led by a comment on its own line.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH3006 code fix | must change |
| RH3006 analyzer | must stay |
| Fix All (BatchFixer) | must stay; covered by a test |
| Formatter | not involved (the fix does not call it) |
| `Reihitsu.Core` | not involved |
| Rule page `RH3006.md` | must stay |
| Specs | must stay (SHARED-FIX-006 and SHARED-FIX-008 already demand it) |
| Tests | must change (regression) |

### Refuse option

The refuse option is to withhold the RH3006 fix when the operand's leading trivia holds a comment and a line break stands between the preceding token and the operand. The guard is cheap, and the class is rare: a comment in front of a wrapped operand.

Withholding removes a fix offered today, though, also in LF documents, where it works. That is a visible change, so it would be an output question. Under the requested change the fix is still offered, and the only defect is the line ending. I would not refuse.

Declaring a shape unsupported is not an option: the fix does not go through the formatter. No simpler option exists. The test base already has the CRLF helper the issue asks for.

### Output questions

_None._ The expected output is demanded by unchanged SHARED-FIX-006, and by SHARED-FIX-008 for the blank-line member.

### Internal choices

- Requirement tags: SHARED-FIX-006 plus RH3006-003 for the issue's input, and SHARED-FIX-008 for the blank-line member. Recommended over the spec author's SHARED-FIX-008 tag, which that requirement's own scope sentence excludes.

### Issue goals

- "Add a CRLF check to the fix test base": drop it. It is stale. `VerifyCodeFixWithEndOfLine` exists and asserts the exact text, line endings included. No visible output changes.
- "Requirements: RH3006-003, SHARED-FIX-008": change to SHARED-FIX-006, RH3006-003, and SHARED-FIX-008 for the blank-line member. No visible output changes.

### Architecture findings

**Components:** the RH3006 fix, and the processing that runs after its code returns. **Decision they share:** the line ending of the token gaps next to the replaced node. The fix decides that by SHARED-FIX-006/-008. The post-processing rewrites gaps that hold a comment with the host's line break.

The same shape fails in RH3003, which is outside the run: `return a +` ⏎ `/* x */ "";` gives `CRLF=7, LF=1` under CRLF. The RH3001 fix, which builds its `(` with explicit trivia, held in three probes:

- `a &&` ⏎ `/* x */ !b`
- `a ==` ⏎ `/* x */ !b`
- `!` ⏎ `/* x */ b`

These are the only three fixes under `src/Reihitsu.Analyzer.CodeFixes` that build tokens with `SyntaxFactory` and do not call `ReihitsuFormatter`. The lead decides how to repair RH3006: narrowly here and RH3003 as a follow-up, or both together.

### Follow-up candidates

- RH3003 code fix writes LF into a CRLF document: `return a +` / `/* x */ "";` → `/* x */ string.Empty;` with the line break after `+` turned to LF. It is a bug against SHARED-FIX-006 with the same mechanism as this issue. It is not in a component this run changes.

## thoenissen/Reihitsu#1284

### Premise

Real, and reached by a caller: the expression-body code fixes. `ExpressionBodyToBlockCodeFixProviderBase<TNode>.ApplyCodeFixAsync` calls `ReihitsuFormatter.FormatNodeInDocumentAsync(document, node)` with the whole member as `node` (line 61). Every phase therefore lays out the member's documentation comment, attributes, signature, initializer, and the blank lines in front of it.

Reproduced under LF and CRLF on `38d3ded`. No line-ending drift appeared:

- RH3202: `public int  GetValue( ) => _value;` gives `public int GetValue()` plus the block. The signature spacing is rewritten (the issue comment's instance).
- RH3202, `///<summary>x</summary>` ⏎ `[Obsolete] public int GetValue() => _value;`: the doc comment becomes three lines, `[Obsolete]` moves to its own line, and the block follows.
- RH3202, two blank lines above the method: one blank line is removed. This is the issue's second instance and is against RH3202-002 and the SHARED-FIX-003 example.
- RH3203, the issue's input: the doc comment becomes three lines, the attribute gets its own line, and `public  C( int value )` becomes `public C(int value)`.
- RH3203, `public C( int value ) :  base( value ) => ...` becomes `public C(int value)` ⏎ `: base(value)`. The initializer moves to its own line. `scripts/format.sh` does the same to a block-bodied constructor, so this is the formatter's layout applied to code outside the replaced node.
- RH3205 (`static C  operator +( C a, C b )`), RH3206 (`implicit  operator int( C a )`), and RH3207 (`~C( )`): the signature spacing is rewritten.
- RH3208, `int  Local( int  v ) => v + 1;` inside `int  Run( int a )`: the local function's signature is rewritten. The containing method and its other statements (`int  x = a;`) stay. So the scope is the local function, not the method.

### Mechanism and candidates

Mechanism: **the fix hands the whole reported member to the formatter rather than only the body it replaces, so every phase's decision about the member's other parts lands in the fix output.**

Dispatch: `ExpressionBodyToBlockCodeFixProviderBase<TNode>`, with six providers.

| Rule | `TNode` | Status |
|---|---|---|
| RH3202 | `MethodDeclarationSyntax` | fails |
| RH3203 | `ConstructorDeclarationSyntax` | fails |
| RH3205 | `OperatorDeclarationSyntax` | fails |
| RH3206 | `ConversionOperatorDeclarationSyntax` | fails |
| RH3207 | `DestructorDeclarationSyntax` | fails |
| RH3208 | `LocalFunctionStatementSyntax` | fails |

Axes of the member's parts outside the body: documentation comment, attribute on the member's line, spacing between modifiers, signature spacing, constructor initializer, constraint clause, blank lines above, and a line break between the signature and `=>`. Every one that was probed fails, except the wrapped arrow (`GetValue()` ⏎ `=> _value;`). The wrapped arrow gives the same output as `main`, and SHARED-BODY-003 keeps that output: the whitespace in front of `=>` goes with the arrow.

### Breaking inputs

- FC-030: the defect itself.
- FC-025: the six kinds above, with `async`/`Task<T>` for RH3202 and RH3208.
- FC-021: a comment between `)` and `=>`, placed by FMT-STRUCT-006.
- FC-027: a constructor initializer, a `where` constraint clause, and the wrapped `=>`.
- FC-008: Fix All with two members in one type.
- FC-043: a body that holds the FMT-UNSUP-004 shape, such as a lambda inside the expression that declares `int /* c */ x = 1;`. Today the fix is withheld because `FindUnsupportedSyntax(GetFormattingInput(node))` sees the member. Whatever node the formatter receives after the change is the node to ask about, so this input must stay withheld.
- FC-017: the output under LF and CRLF.

Test candidates beyond the spec examples:

- RH3203 `public C( int value ) :  base( value ) => _value = value;` keeps `:  base( value )` on the signature line, as written. SHARED-FIX-006 demands this: the initializer is outside the replaced node, and the fix must not format the member.
- A method with a constraint clause, `T M<T>( )  where T : class => default;`.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `ExpressionBodyToBlockCodeFixProviderBase<TNode>` and its six providers | must change |
| Analyzers (`ExpressionBodyAnalyzerBase<TNode>`) | must stay |
| Formatter phases | must stay (FMT-STRUCT-004/-005/-006 own the block) |
| `ReihitsuFormatter` node-level entry points | must stay; see the prediction below |
| Fix All | must stay; covered by a test |
| `Reihitsu.Core` (`ExpressionBodyRewriteUtilities`) | not involved, as far as the defect shows |
| Rule pages | must stay |
| Specs | SHARED-BODY-003 and the RH3202 introduction, already done by the spec author |
| Tests | must change |

### Refuse option

No refuse option fits. Withholding these fixes when the rest of the member is not already formatted would withhold them for code people commonly write, and that is not covered by *Special cases do not grow the source*. Declaring a shape unsupported is not an option, because no shape costs a special case here.

The simpler option is the existing node-level entry point `FormatNodeInDocumentWithContextAsync`, which formats a target inside a context (FMT-PIPE-017/-018). I name it as an existing helper, not as a design.

### Does an entry point predict a phase's decision?

Yes, this is a risk. Once the fix formats only the new block, two decisions belong to phases:

- the line break and column of the block's `{`, which belong to the line-break phase and the indentation phase;
- the removal of whitespace at the end of the signature's line, which belongs to the cleanup phase.

A node-level entry point writes back its target's leading trivia as in the document (FMT-PIPE-017). An implementation that builds that leading trivia itself, as a line break plus the member's indentation, predicts the line-break and indentation phases. The safe shape reads the phases' result. Name each such computation in the implementer's handover.

### Output questions

_None._ Every changed output is demanded by unchanged SHARED-FIX-006, which states P14d (#912 round 7a), and by RH3202-002. The RH3203 initializer case is not listed in the issue, but SHARED-FIX-006 *Formatting* forbids formatting the member, so it is not in question either.

### Internal choices

- SHARED-BODY-003 wording: "The whitespace between the last token of the signature and `=>`" does not name the initializer or the constraint clause, which can stand between the signature and `=>`. Recommend "the last token in front of `=>`". Both readings give the same output under SHARED-FIX-007, because the edited line ends without whitespace either way.
- SHARED-BODY-003 lists "its documentation comment, its attributes, its modifiers, and its signature". Recommend adding "its constructor initializer and constraint clauses", or making "the rest of the member" plainly the governing phrase, so the RH3203 initializer case reads as decided.

### Issue goals

- "SHARED-FIX-003 layout example is reworded": drop it. The example already says the two blank lines "are outside the node the fix replaces (SHARED-FIX-006)" (`sdd/specs/shared/code-fixes.md:455`). No visible output changes.
- "Blocked by #1138": stale. SHARED-FIX-006/-007/-008 exist.
- "Requirements: SHARED-FIX-007": the governing requirement is SHARED-FIX-006 (*Formatting*). SHARED-FIX-007 governs the layout of the block only.

### Architecture findings

_None._ This is one owner, the shared base, formatting too large a node: FC-030, a local scope error.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1285

### Premise

Real, and reached by a caller: the RH3001 code fix. The provider registers only when `root.FindNode(diagnostic.Location.SourceSpan) is PrefixUnaryExpressionSyntax`. The diagnostic is on the `!` token, and `FindNode` without `getInnermostNodeForTie` climbs to the outermost node with the same full span. So every syntax node that wraps the negation without adding a token hides it.

Probed on `38d3ded` (LF and CRLF agree):

| Position | Today |
|---|---|
| `M(!x);`, `N(!x, y);` | no fix |
| `(!x, y)` | no fix |
| `C k = new(!y);` (implicit object creation) | no fix |
| `public C(bool b, int i) : this(!b)` | no fix |
| `o?[!y]` (element binding) | no fix |
| `new E { [!x] = true }` (implicit element access) | no fix |
| `class D(bool x) : Base(!x)` (primary-constructor base) | no fix |
| `[A(!true)]` | no fix |
| `bool[] a = [!x, y];` | no fix |
| **`from a in v orderby !a select a`** (no direction) | **no fix** |
| `x is !true`, `x switch { !true => 1, … }` | no fix |
| `t is (!true, _)`, `a is [!true]`, `x is not !true`, `x is !true or false`, `o is C { P: !true }`, `case !true when x:` | no fix (all constant patterns) |
| `M(b: !x)`, `case !true:`, `$"{!x}"`, `z => !z`, `new { A = !x }`, `new bool[] { !x }`, `return !x`, `=> !x`, `c ? !x : y`, `!x ? 1 : 2`, `n ?? !x`, `y = !x`, `y &= !x`, `where !a` | fixed, no parentheses |
| `y == !x`, `!x is bool`, `!x switch {…}` | fixed with parentheses (binding needs them) |
| `_b ??= !x`, `[A(P = !true)]`, `[A(b: !true)]`, `goto case !true`, `select !a`, `group !a by a`, `group a by !a`, `orderby !a descending` | fixed with parentheses that group nothing |

Neither RH3002 nor RH3005 reports `[A(true == false)]` or `M(x == false)` (`apply-fix.sh RH3005`/`RH3002` → `no-diagnostic`).

### Mechanism and candidates

Mechanism: **the fix is withheld exactly when the negation's full span equals that of the node around it.**

Dispatch is the Roslyn node kinds that wrap an expression without a token of their own:

- `ArgumentSyntax` (every argument list: invocation, object creation and implicit object creation, element access and element binding, implicit element access, constructor initializer, primary-constructor base type, tuple);
- `AttributeArgumentSyntax` (positional);
- `ExpressionElementSyntax`;
- `OrderingSyntax` without a direction keyword;
- `ConstantPatternSyntax`;
- `SubpatternSyntax` without a name, which holds a constant pattern.

`AnonymousObjectMemberDeclaratorSyntax` without a name cannot hold `!x` in compiling code (CS0746). The new RH3001-004 ("wherever the negation stands … withheld only by RH3001-006, -007, or -008") turns every one of these into "fixed", except the constant patterns, which RH3001-008 keeps withheld.

### Breaking inputs

- FC-001 and FC-002: the guard's span. Every member of the dispatch list above is a test candidate.
- FC-008: Fix All with `M(!x, !y)`, and the nested `M(!(!x))`, where the outer negation is a whole argument and the inner one is not.
- FC-003: both sides of the boundary. These are the RH3001-008 pairs:
  - `case !true:` stays fixed and `case !true when x:` stays withheld;
  - `true => !y` behind an arm's `=>` is fixed;
  - `!true => 1` is withheld.
- FC-019 and FC-021: a whole argument with a comment after `!` (RH3001-005) and with a directive after `!` (RH3001-006 withholds).
- FC-017: CRLF for a whole argument led by a comment on its own line: `M(` ⏎ `/* x */ !x)`. It cannot be probed today because no fix is offered. It is the shape that drifts in RH3006.
- FC-006: `[A(!true)]` becomes `[A(true == false)]`, a constant expression that compiles. Covering it needs a compile check.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH3001 code fix | must change (node lookup, constant-pattern guard, parentheses) |
| RH3001 analyzer | must stay (RH3001-001: patterns stay reported) |
| Fix All | must stay; covered by a test |
| Rule page `RH3001.md` | must stay |
| RH3002, RH3005 | not involved (they do not report the new output) |
| Formatter, `Reihitsu.Core` | not involved |
| Spec, change doc | RH3001-004 and -008, plus the additions below |

### Refuse option

The refuse option keeps the fix withheld for whole-node negations and records the gap. The maintainer rejected it in #1245 Q2 (round 7a), and the class is common code (`M(!x)`), so it is not open. Declaring a shape unsupported is not an option, because the formatter is not involved.

### Verification of the change doc's maintainer coverage

`sdd/changes/2026-11/rh3001-fix-scope.md` lists three visible changes. Each was checked against the GitHub record:

1. Fix offered for `M(!x)`, `(!x, y)`, `new C(!y)`, `this[!y]`, `[A(!true)]`, and `[!x, y]`. **Covered.** The #1245 decision comment of round 7a (2026-10-20) records Q2 as a guard-scope defect: "a named argument gets a fix and so does a plain one, and `[!x, y]`". The #912 round-7a summary lists #1245 with only Q3 left open. The issue body of #1285 carries the same.
2. No parentheses behind `>>>=` and `??=`, in named attribute arguments (`=` and `:`), behind `goto case`, and in `select`, `group`, and `orderby` with a direction. **Covered.** The #1245 round-7b comment records Q3, option A, "parentheses only where binding needs them". The #912 round-7b bookkeeping records "all 23 items answered, option A each". The #1285 comment repeats it. Every position in the change doc was shown in #1245 Q3 and its continuations (`select !v into w`, `ascending` included). The "Before" outputs the change doc shows match the probes above.
3. Constant patterns stay without a fix. **No visible change**, as probed. P23 (round 7a) covers it.

The record on GitHub is agent-written under the maintainer's account (each comment ends "Generated by Claude Code"), summarizing answers the maintainer gave in the question rounds. I took it as the repository's record of those answers.

**Visible output changes the change doc does not list:**

- **a. More argument positions:** `new(!y)`, `o?[!y]`, `new E { [!x] = true }`, `class D(bool x) : Base(!x)`, and `N(!x, y)`. Today none gets a fix; after the change all do.
  - These are plain arguments, so the Q2 answer "a plain one [gets a fix]" covers them. Not in question.
  - The change doc's list and RH3001-004's position list ("an invocation, an object creation, an element access, a constructor initializer, or a tuple") should name them, or say "such as". That is internal.
- **b. `orderby !a` without a direction** (`OrderingSyntax`). Today: reported, no fix. After the change: `orderby a == false`.
  - It is not an argument, tuple element, or collection element, and it appears in none of the inputs shown in #1245 Q2.
  - The Q2 continuation did name the mechanism ("withheld exactly when the negation is the whole of the syntax node around it"), and the answer called the guard's scope a defect. So the answer may cover it at class level. But no maintainer answer shows this input.
  - Asked below.

No other visible change follows from the spec diff. Every position where the fix writes parentheses today was probed, and the extra parentheses appear only in the positions Q3 lists.

### Output questions

**1 · RH3001 · Not operator should not be used — negation as an `orderby` key without a direction**

Input:
```csharp
using System.Linq;

class C
{
    void Run(bool[] v)
    {
        var q = from a in v
                orderby !a
                select a;
    }
}
```

Before (`main`): reported, no fix offered.

After, A (recommended):
```csharp
using System.Linq;

class C
{
    void Run(bool[] v)
    {
        var q = from a in v
                orderby a == false
                select a;
    }
}
```

After, B: reported, no fix offered (unchanged). RH3001-004 then has to name this position as an exception.

Why A: it is the same accident as a whole argument, the same rewrite is safe, and `orderby !a descending` already gets `orderby a == false descending` once the parentheses are dropped.

The lead may instead judge A as covered by the Q2 answer and record it in the change doc. That is a judgment about coverage, which is why I list it here rather than decide it.

### Internal choices

- RH3001-004's list of positions: recommend "such as" plus the additional argument owners. Either way the output is the same.
- RH3001-008 is keyed on the constant-pattern node, so `case !true:` stays fixed and `case !true when x:` stays withheld. Both are today's output. Recommend stating the second one in RH3001-008's example or prose, so a test pins the boundary.

### Issue goals

- The body's "#1245 Q3 (the parentheses of the fix output) is still open" is stale. It was decided in round 7b, and the comment on #1285 carries it. Keep the goal as the comment states it. No visible output changes.
- "Requirements: RH3001-001, RH3001-004": RH3001-001 is unchanged. The run changes RH3001-004 and adds RH3001-008. No visible output changes.
- All "Done when" items stand.

### Architecture findings

_None._ The guard and its accident sit in one provider.

### Follow-up candidates

- **RH3001 fix leaves whitespace at the end of a line when `!` ends it.** This is outside the run's issues. It is a rare input, a bug against SHARED-FIX-007, and its refuse option is to withhold the fix when a line break follows `!`.

  Input:
  ```csharp
  class C
  {
      bool Run(bool a, bool b)
      {
          return a && !
              /* x */ b;
      }
  }
  ```

  Fix today (LF and CRLF): `return a && ` (with a trailing space) ⏎ `/* x */ b == false;`.

## Whole run

- No build or test run. The skeptic ran only `scripts/apply-fix.sh` (RH3001, RH3002, RH3003, RH3005, RH3006, RH3202, RH3203, RH3205–RH3208) and `scripts/format.sh` on temporary fixtures, all exit 0.
- Scope: #1283's mechanism also reaches RH3003, which is not a component the run changes. Whether that becomes a follow-up or joins the run is the lead's decision.
