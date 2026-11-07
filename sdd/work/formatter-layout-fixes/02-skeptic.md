Role: skeptic
Commit: d8f92a2
Verdict: #1243: proceed; #1383: proceed; #1149: proceed; #1391: output question; #952: proceed narrower (regression test only); #1611: output question

Every observation below was made on `d8f92a2`. Formatter outputs come from `scripts/format.sh` on fixtures in a temporary directory; every fixture was formatted twice, and the second pass changed nothing. Code-fix observations come from `scripts/apply-fix.sh`, which runs each fixture under LF and CRLF. All six issues are `agent-filed`. I checked the spec author's claims against the code instead of taking them as given. The prompt only named the points `00-lead.md` lists, so it carried no intended fix to ignore.

## thoenissen/Reihitsu#1243

### Premise
The issue is real. The CLI and whole-document formatting reach it, and so does every code fix that formats a node holding such an expression. The issue's input reproduces exactly: `var x = a⏎⏎&& b;` and `var y = a⏎⏎? 1⏎: 2;`. `scripts/trace.sh` shows that `LineBreakPhase` writes the blank line and that later phases keep it. FMT-BREAK-030 forbids this phase to insert a blank line in a gap it leaves on separate lines. No requirement changes; this is a plain bug.

### Mechanism and candidates
Mechanism: when an operator ends its line, the rewriter that moves it to the start of the continuation line appends a line break behind the token in front of the operator. It never checks whether that gap already holds one. Whenever the operand in front already ends its line, a second line break is written, which is a blank line.

The `AppendEndOfLine` call sites in `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/` are the candidates:
- `BinaryOperatorLineBreakRewriter.NormalizeBinaryOperatorPosition` (`BinaryExpressionSyntax`, every operator kind): no check. **Fails.**
- `TernaryLineBreakRewriter.MoveQuestionTokenToNextLine` (`ConditionalExpressionSyntax.QuestionToken`): no check. **Fails.**
- `TernaryLineBreakRewriter.NormalizeColonTokenPosition` (`ConditionalExpressionSyntax.ColonToken`): no check. **Fails.** The issue does not name this site.
- `ChainLineBreakRewriter.EnsureLineBreak`, `LineBreakListRewriter` (two sites), and `GenericConstraintLineBreakRewriter` each check for an existing line break first, so they hold. `BinaryOperatorLineBreakRewriter.NormalizePatternOperatorPosition` (`and`/`or`) uses `MoveTokenToNewLine` only when no line break exists, so it holds too.

### Breaking inputs
Failure classes: FC-016 (sibling shapes), FC-019/FC-021 (a comment on the line being joined, or between), FC-020 (a directive in the gap), FC-010/FC-017 (CRLF, two passes), FC-012 (the line-break phase and the blank-line phase).

The class axes are which operator moves (each binary operator kind, `?`, `:`) and what ends the line in front of it (a plain line break, an end-of-line comment, or a comment line between the operand and the operator). Every cell I ran fails with a blank line:

| Input (in a method body) | Today |
|---|---|
| `var p = i⏎+⏎1;` (also `??`, `is string`, `as string`) | blank line above `+ 1` |
| `var p = a // c⏎&&⏎b;` | blank line above `&& b` |
| `var p = a⏎// between⏎&&⏎b;` | blank line between `a` and `// between` |
| `var p = a⏎\|\|⏎b⏎\|\|⏎a;` | a blank line above each `\|\|` |
| `var p = a ? 1⏎:⏎2;` and `a⏎? 1⏎:⏎2;` | blank line above `: 2` |
| `var p = a // c⏎?⏎1⏎: 2;` | blank line above `? 1` |
| `var p = a⏎? 1⏎// c⏎:⏎2;` | blank line above `// c` |

These cells must hold and must stay as they are: an operator at the end of the operand's line (`a &&⏎b` → `a⏎&& b`, no blank line today), an operator followed by a comment (`a⏎&& // c⏎b`: the join is withheld), and a blank line the author wrote above the operator, which FMT-BREAK-030 keeps. The test implementer covers each failing row under LF and CRLF, plus the issue's own input (FC-034). The blank line above `return` in the issue's expected output belongs to FMT-BLANK and stays.

### Surfaces
| Surface | Must change / must stay / not involved |
|---|---|
| Formatter `LineBreakPhase` (binary and ternary rewriters) | must change |
| Other formatter phases | must stay |
| Analyzers RH5302/RH5308 | not involved (thoenissen/Reihitsu#1273 depends on this fix but is outside the run) |
| Code fixes that delegate to the formatter | not changed; any expected test value that held the blank line changes with the formatter (FC-032) |
| Rule docs, specs | not involved (FMT-BREAK-030 already decides) |

### Simpler option
_None._ The change cannot be narrower than the three sites. `LineBreakTriviaUtilities.HasLeadingEndOfLine` and `TokenGapUtilities.HasLineBreakBetween` already state the check the other sites make.

### Output questions
_None._ FMT-BREAK-030 decides every row.

### Internal choices
_None._

### Issue goals
"Both inputs keep no blank line, under a test tagged FMT-BREAK-030": keep this, but widen it to the class above. The `:` site and the comment rows are the same defect. Widening changes no output that a requirement leaves open.

### Architecture findings
The decision "a line break in front of this token, keeping one that is already there" is implemented once per rewriter: in `ChainLineBreakRewriter.EnsureLineBreak`, in `LineBreakListRewriter`, and in `GenericConstraintLineBreakRewriter`, each with its own check, and in three places without one (above). This is one rule (FMT-BREAK-030) stated in several places. A narrow fix adds three more copies of the check, so the lead decides under `squad/README.md`, *Architecture findings*.

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1383

### Premise
The issue is real for all three open instances. The CLI and whole-document formatting reach each of them. The attribute-target colon is resolved on main and covered by `src/Reihitsu.Formatter.Test/Regression/Spacing/AttributeTargetColonSpacingTests.cs`. Before this run, FMT-PIPE-014 already named the indexer parameter list, every compound assignment operator, and the comment in front of the collection-expression `]`. All three are bugs against an unchanged requirement, so I confirm the triage's bug-fix lane without a change doc. Today's output:
- `int this[int a⏎, int b] => a;` is unchanged. `int this⏎[string a]` leaves `[` on its own line. `int this[long a, string b⏎]` keeps the break and aligns `]` after `[`. As a cross-check, the method forms of the same three inputs give exactly the indexer outputs that the new FMT-BREAK-011/-012 examples show.
- `x⏎+= 1;` and `y ??=⏎2;` keep their wraps, and the continuation lands in the statement column.
- A comment in front of a collection expression's `]` sits in the `]` column. One in front of an initializer's `}` sits one level deeper.

### Mechanism and candidates
- Indexer: `LineBreakListRewriter` handles `ParameterListSyntax` (`VisitParameterList`) and has no `VisitBracketedParameterList`. `BaseParameterListSyntax` has exactly two subtypes, so `BracketedParameterListSyntax` (indexers: plain, explicit interface, interface member) is the only missing member. `ListElementInteriorJoinRewriter` (FMT-BREAK-015) and `ArgumentAlignmentContributor` (FMT-INDENT-012) already handle it.
- Compound assignment: `LineBreakAssignmentRewriter.NormalizeSimpleAssignmentOperatorPlacement` and `NormalizeSimpleAssignmentValuePlacement` return early unless the node is `SimpleAssignmentExpression`. The members of `AssignmentExpressionSyntax` are `+= -= *= /= %= &= ^= \|= <<= >>= >>>= ??=`, event `+=`/`-=` included. `ListElementInteriorJoinRewriter.IsOperatorWrap` excludes every assignment operator, so a compound assignment inside an argument is joined by no one today either. The collection-expression and collection-initializer paths of the same rewriter already accept every kind.
- Comment before `]`: `CommentIndentationContributor.AlignCommentsBeforeToken` adds one level only for `CloseBraceToken`. A `CloseBracketToken` has these parents: collection expression (in scope), list pattern, attribute list, bracketed argument list, bracketed parameter list, array rank specifier, and calling-convention list.

### Breaking inputs
Failure classes: FC-016, FC-002 (the boundary between a binary operator and an assignment operator), FC-003, FC-019/FC-021/FC-020 (a comment or directive in the gap keeps the wrap), FC-024/FC-025 (explicit-interface and expression-bodied indexers), FC-027 (other enclosing scopes), FC-010.

- Indexer cells: `this⏎[`, a leading `, `, `]` on its own line, a break after `[`, a wrap between two parameters (one per line), an explicit interface indexer `int I.this[…]`, and an indexer with an accessor block. I ran every compound-assignment cell below and each fails today.
- Compound cells: `x⏎-= 1;`, `x <<=⏎2;`, the lone operator `x⏎>>>=⏎3;`, the event `E⏎+= h;`, a for incrementor `for (…; i⏎+= 1)`, a lambda body `() => x⏎\|= 1`, and the nested `var z = x = y⏎??= 5;`. Further cells: a compound assignment in an argument (`M(x⏎+= 1)`), and `y // c⏎??= 4`, which must keep its wrap. Its continuation column belongs to thoenissen/Reihitsu#953.
- Must stay: `a = a⏎+ 1;` keeps its wrap (FMT-BREAK-017).
- `]` cells, all at the `]` column today: a declarator value, a field initializer, an argument (`M([⏎1,⏎// c⏎])`), a nested collection expression, a spread element, a `/* */` comment, an expression-bodied member's value, and an empty collection expression `[⏎// c⏎]`. The empty one has no elements, so only "one level deeper" decides it. In every cell I ran, the elements stand one level deeper than `]`, so "one level deeper" and "the elements' column" never disagree.
- Must stay in the `]` column: a comment in front of the `]` of a list pattern, an element access, and an attribute list (FMT-INDENT-006 names only the collection expression).

### Surfaces
| Surface | Must change / must stay / not involved |
|---|---|
| `LineBreakListRewriter` (indexer lists) | must change |
| `LineBreakAssignmentRewriter` (compound kinds) | must change |
| `CommentIndentationContributor` (collection-expression `]`) | must change |
| `ArgumentAlignmentContributor`, `ListElementInteriorJoinRewriter` | must stay |
| RH5106/RH5107/RH5111 | not involved (P11; the formatter may do more; see follow-ups) |
| RH5204 | not involved (it checks comments only in front of the braces RH5204-001 names, no `]`) |
| Rule docs | not involved |

### Simpler option
_None._

### Output questions
_None._ FMT-PIPE-014 decided all three instances before the run, and the spec author's examples match the method-list outputs I ran.

### Internal choices
_None._ The spec author's widening of FMT-BREAK-013 to indexers states what FMT-PIPE-014 already demands, so I agree with it.

### Issue goals
- "A change doc records the decision": drop it. The work is a bug fix against FMT-PIPE-014 (triage of 2026-10-24), and nothing visible changes beyond what that requirement demands.
- "FMT-INDENT-025 … names the whole family": not needed. FMT-INDENT-025 already treats collection expressions and list patterns alike, and FMT-INDENT-006 carries the comment rule.
- "Each input above has a regression test": keep it for the three open instances. The attribute colon is already covered (above).

### Architecture findings
_None._

### Follow-up candidates
See *Whole run*, items 3 and 4.

## thoenissen/Reihitsu#1149

### Premise
The issue is real. The CLI reaches it, and so does the RH5111 code fix on an enum member.
- The formatter keeps `A⏎    = 1,` and only re-indents `= 1` to the member column. It joins `B =⏎2` (value placement) and `int a⏎= 1` (FMT-BREAK-015). `B⏎=⏎2` becomes `B⏎= 2`.
- `scripts/apply-fix.sh RH5111` on `enum E { A⏎        = 1, }` gives `no-progress (2 iteration(s))` under LF and CRLF. The diagnostic stays, and formatted code is reported, which breaks SHARED-DIAG-010 (FC-013) and SHARED-FIX-001 (FC-007).
- Parameters are already fine. `scripts/apply-fix.sh RH5111` on `void M(int a⏎= 1)` and on a later parameter gives `fixed`.

Spec author's point (`00-lead.md`): the enum-member `=` was already required. **Confirmed.** Before this run, RH5111-004 already required the fix to format an enum member "as the formatter formats it in place, which joins the `=` to the line of the target … (FMT-BREAK-016)". The analyzer already reports the shape (`OnEnumMemberDeclaration` → `CheckAssignment`). SHARED-DIAG-010 forbids RH5111 to report formatter output. And the maintainer decided the output in question round 5 (thoenissen/Reihitsu#1120, Q1: the formatter joins `A⏎= 1` to `A = 1`). No change doc is needed, and nothing visible is in question.

**Correction to `01-spec-author.md`.** It says the RH5111 fix of `enum E { A⏎    = 1, }` should give `A = 1`, with the trailing comma removed by FMT-STRUCT-019. That is wrong for the fix. The fix formats only the enum member (SHARED-FIX, "A fix MUST format only the node it replaces"; the RH5111-004 example keeps `Third,`). The trailing comma belongs to the enum, not the member. Evidence: `scripts/apply-fix.sh RH5111` on `enum E { A =⏎        1, }` gives `A = 1,` today. The fix result to assert is `A = 1,`, as the issue states. Only whole-document formatting drops the comma.

### Mechanism and candidates
Mechanism: `LineBreakAssignmentRewriter` joins an `=` below its target only for the owners it visits (`AssignmentExpression` of the simple kind, `VariableDeclarator`, `PropertyDeclaration`). `VisitEqualsValueClause` places only the value. `EqualsValueClauseSyntax` has four owners: `VariableDeclaratorSyntax` and `PropertyDeclarationSyntax` hold; `ParameterSyntax` holds through FMT-BREAK-015; `EnumMemberDeclarationSyntax` **fails**.

### Breaking inputs
Failure classes: FC-007, FC-008, FC-013, FC-019/FC-021/FC-020, FC-032, FC-010.

Formatter cells: `A⏎= 1,`, `A⏎=⏎1,` (the issue's third input), a last member without a trailing comma, a member with an attribute, and a member behind a documentation comment. `A // c⏎= 1` must keep its wrap.

RH5111 fix cells, each converging in one iteration: `A⏎= 1,` → `A = 1,`, `A⏎=⏎1,` → `A = 1,`, and Fix All over two enum members in one enum. The RH5111-004 column cases (a member that starts a later line keeps its column) stay as they are.

### Surfaces
| Surface | Must change / must stay / not involved |
|---|---|
| `LineBreakAssignmentRewriter` (enum member `=`) | must change |
| RH5111 analyzer | must stay (it already reports the shape) |
| RH5111 code fix | must stay; its output follows by delegation |
| `ListElementInteriorJoinRewriter` (parameter `=`) | must stay |
| Rule page RH5111 | not involved |

### Simpler option
For parameters, no production change: they already join, in the formatter and in the fix.

### Output questions
_None._

### Internal choices
- **FMT-BREAK-015 and FMT-BREAK-016 now both claim the gap in front of a parameter's default `=`.** The new FMT-BREAK-016 names "a parameter's default value". FMT-BREAK-015's exclusion list ("operator wraps of … assignment expressions (FMT-BREAK-016 to FMT-BREAK-020)") does not cover an equals-value clause, so -015 still joins that gap, and the code does it (`ListElementInteriorJoinRewriter.IsOperatorWrap` has no `EqualsValueClauseSyntax`). Both give the same output. Recommendation: the spec names one owner. Either -015's exclusion gains "the `=` of a parameter's default value (FMT-BREAK-016)" and the code moves the decision, or -016 says that inside a parameter list FMT-BREAK-015 performs that join. I recommend the second, because it needs no production change for parameters.

### Issue goals
- "A change doc records the extension": drop it (above).
- "The RH5111 fix clears the enum-member diagnostic in one application": keep it, with the result `A = 1,`.
- The rest: keep.

### Architecture findings
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1391

### Premise
Two instances are real; the CLI and whole-document formatting reach both. Two are resolved on main.
- Base-list continuation: `class C : System.IDisposable,⏎          System. // c⏎    ICloneable` puts `ICloneable` at column 0 (column 4 inside a namespace).
- Empty wrapped list: `M( // c⏎              );` puts `)` at column 8. FMT-INDENT-012 places it in the column after `(`.
- Resolved: the omitted type argument (`,>);` at column 8, block indentation per FMT-INDENT-015, covered by `ZeroWidthTokenLineStartTests`). The raw string with a lone `\r` is aligned, and so are U+0085, U+2028, and U+2029 (16 spaces, which I ran). Per FMT-PIPE-007, the code line breaks of a file that uses only `\r` become `\n`. The test implementer adds regression tests for the lone-`\r` and Unicode line-break inputs, which the issue's "Done when" asks for, if none exists.

### Mechanism and candidates
- Empty list: `ArgumentAlignmentContributor.AlignToOpenToken` returns at `items.Count == 0` before it places the closing delimiter, so a closing delimiter that starts a line in an empty list is never aligned. The members are the `AlignToOpenToken` cases whose empty form compiles. `ArgumentListSyntax` covers an invocation, an object creation, a constructor initializer `: this( // c⏎)`, and a primary-constructor base `: Base( // c⏎)`. `ParameterListSyntax` covers a method, constructor, local function, delegate, parenthesized lambda, anonymous method, and primary constructor. `AttributeArgumentListSyntax` covers `[A( // c⏎)]`. Every one I ran **fails**: `)` at the block or statement column, and at column 5 for the attribute, where the attribute-list continuation alignment places it. Bracketed arguments and parameters, tuples, tuple types, and calling conventions cannot be empty without a syntax error, which the formatter skips.
- Base list: `BaseTypeListContributor` aligns only later base types that start a line. Unlike the other list contributors, it never calls `ListElementAligner.AlignContinuations`. A line inside a base type (FMT-INDENT-013) therefore falls back to the block column.

### Breaking inputs
Failure classes: FC-016, FC-021 (a comment before the closing delimiter), FC-020, FC-027, FC-010.

- Base-list cells, all at the block column today: inside a later type of a two-type list (the issue's input); inside the first type of a two-type list (`class D : System. // c⏎IDisposable, System.ICloneable`); an interface base list; a struct or record base list. The single-type base list and the enum base (`enum E : System. // c⏎Int32`) are the output question below.
- Must stay: the primary-constructor base argument list (`: Base( // c⏎x)` aligns after `(` today).
- Out of scope: a type argument list nested in a base type (`IEnumerable< // c⏎int>` at the block column). It fails the same way outside base lists (*Whole run*, item 1).
- Empty-list cells: every member listed above. Must stay: a non-empty list whose `)` starts a line (`M(1 // c⏎)` aligns after `(` today).

### Surfaces
| Surface | Must change / must stay / not involved |
|---|---|
| `ArgumentAlignmentContributor` (empty list) | must change |
| `BaseTypeListContributor` (continuations) | must change |
| Attribute-list continuation alignment | must stay for non-empty attributes; the `[A( // c⏎)]` cell needs its `)` after `(` |
| RH5203 | not involved (it checks arguments, not `)`) |
| Rule docs, specs | not involved, unless the output question is answered A (then FMT-INDENT-013 gets a clarification) |

### Simpler option
_None._

### Output questions
**1 · Formatter · a line inside the only base type of a base list**

FMT-INDENT-013 aligns a line inside an element of "a list from FMT-INDENT-016", and FMT-INDENT-016 speaks of base lists "with two or more types". One reading includes a base list with one type, the other excludes it, and the two give different output. The case belongs to the same class as the issue's base-list instance.

Input:
```csharp
class B : System. // c
    IDisposable
{
    public void Dispose()
    {
    }
}

enum E : System. // c
    Int32
{
    A,
}
```

Before (`main`): `IDisposable` and `Int32` at column 0.

After, A (recommended):
```csharp
class B : System. // c
          IDisposable
{
    public void Dispose()
    {
    }
}

enum E : System. // c
         Int32
{
    A,
}
```

After, B: unchanged, column 0.

Why A: a line at column 0 reads as a new declaration, and the two-type list's continuation is aligned the same way.

### Internal choices
_None._

### Issue goals
- "Each input behaves as stated and has a regression test with the idempotency check": keep it for all four inputs, the two resolved ones as regression tests.
- "Check U+0085, U+2028 and U+2029 line breaks in the same pass": done, all aligned. Keep the tests.

### Architecture findings
_None._

### Follow-up candidates
See *Whole run*, item 1.

## thoenissen/Reihitsu#952

### Premise
Already resolved on main, as the triage says. The issue's exact input gives `#endif` at column 0. A wider matrix also holds: `#elif` and `#else` after a disabled branch at member level, `#endif` after disabled text in a method body and inside an argument list, `#pragma` behind `#endif`, and `#endregion` after a disabled `#if` block (region indentation). The production change of thoenissen/Reihitsu#1510 introduced the shared line-start fact.

### Mechanism and candidates
_N/A._ Resolved.

### Breaking inputs
FC-034/FC-033: the regression test uses the issue's exact input (indented `#if !NEVER`, `#else`, and `#endif` behind a disabled `#else` branch), under LF and CRLF, with two passes. FC-020: add the member-level `#elif`/`#else`/`#endif` cell.

### Surfaces
| Surface | Must change / must stay / not involved |
|---|---|
| Formatter indentation | must stay |
| Tests | a regression test is added (FMT-INDENT-009) |

### Simpler option
Regression test only, which is what the issue's comments ask for.

### Output questions
_None._

### Internal choices
_None._

### Issue goals
_N/A_ beyond the regression test. The issue can close with `Closes #952` once the test lands.

### Architecture findings
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1611

### Premise
The issue is real; the CLI and whole-document formatting reach it. With four U+00A0 in front of the content and the closing quotes (opening quotes at column 16), the formatter writes 16 spaces in front of them. `scripts/apply-fix.sh RH5202` on that output gives `fixed`, so RH5202 reports formatter output (FC-013). Its result is 12 spaces + 4 U+00A0, which matches the new FMT-RAW-002 example.

The severity is higher than the issue states, because the formatter also undoes the RH5202 fix. Input with 12 spaces + 4 U+00A0, which RH5202 finds aligned (`no-diagnostic`), comes out as 16 spaces + 4 U+00A0, which RH5202 reports again. The fix and the formatter undo each other's output.

The issue's requirement ID is wrong (FMT-RAW-001, not -009), and so is its claim that tabs are excluded "after tab replacement". I confirm both of the spec author's corrections (FMT-TAB-003).

Spec author's point (`00-lead.md`): a leftward shift removes non-space leading whitespace. **Confirmed**, with one caveat for tabs (output question 2). RH5202-003 already states that the fix "add[s] spaces in front of each line or remov[es] that much leading whitespace from it — all of it, when the line has less". `RH5202…CodeFixProvider` does exactly that (`GetLeadingWhitespaceLength`, then a removal or a space insertion). SHARED-FIX-007 requires the fix and the formatter to agree, and FMT-RAW-001 requires the marker to end up under the quotes. The spec diff therefore states behavior that existing requirements demand, and nothing visible is in question for non-tab whitespace.

### Mechanism and candidates
Mechanism: `RawStringLineShifts` measures a literal's closing column and rewrites its content and closing lines through `GetLeadingSpaceCount`, which counts only U+0020. RH5202 and its fix measure with `RawStringLiteralUtilities.GetLeadingWhitespaceLength` (`char.IsWhiteSpace`). Any whitespace character other than a space in front of the closing marker is therefore missing from the formatter's width, and a leftward shift cannot remove it. Both callers of `GetLeadingSpaceCount` decide here: the closing width (`RawStringLineShifts` constructor) and the edit length of a content or closing line (`GetIndentationLength`). The indentation of a hole line is whitespace trivia, which `GetIndentationLength` rewrites as a whole, so the measure does not decide it (read from the code; the anchored hole cell below is the test for it).

### Breaking inputs
Failure classes: FC-015, FC-016, FC-013, FC-022, FC-003, FC-017, FC-010. The matrix follows the shape of `failure-classes.md`, *Tab width across formatter phases*, with the tab replaced by another whitespace character. Every cell is checked against FMT-RAW-001/-002, after one pass and after a second, and RH5202 must not report the result.

| Axis | Members |
|---|---|
| Shift | right; none (aligned by RH5202's measure); left by fewer than the spaces in front of the other whitespace; left by exactly those spaces; left by more than those spaces |
| Character | U+00A0, U+2003, vertical tab, form feed; mixed with spaces in either order; tab (output question 2) |
| Literal kind | plain, `u8`, `$`/`$$` interpolated (closing line in `InterpolatedRawStringEndToken`), with a wrapped interpolation-hole line |
| Anchoring | none; a chained call on the closing line (`""".Trim()⏎.ToString()`), whose dot must stay under the moved `.Trim`; a hole line anchored to a token on a shifted content line |
| Line ending | LF, CRLF |

Cells I ran. Every one fails today, with the formatter result first and FMT-RAW's result second:

| Cell | Today | Required |
|---|---|---|
| right, 4 U+00A0 | 16 spaces + 4 U+00A0 | 12 spaces + 4 U+00A0 |
| none, 12 spaces + 4 U+00A0 | 16 spaces + 4 U+00A0 | unchanged |
| left fewer, 18 spaces + 4 U+00A0 | 16 spaces + 4 U+00A0 | 12 spaces + 4 U+00A0 |
| left exact, 4 spaces + 16 U+00A0 | 16 spaces + 16 U+00A0 | 16 U+00A0 |
| left more, 2 spaces + 20 U+00A0 | 16 spaces + 20 U+00A0 | 16 U+00A0 |
| left, 20 U+00A0 | 16 spaces + 20 U+00A0 | 16 U+00A0 |
| U+2003, VT, FF (4 each) | 16 spaces + 4 of them | 12 spaces + 4 of them |
| `  ··` / `··  ` | 16 spaces + `··` / 16 spaces + `··  ` | 14 spaces + `··` / 12 spaces + `··  ` |
| `$"""` with 4 U+00A0 | 17 spaces + 4 U+00A0 | 13 spaces + 4 U+00A0 |
| `"""u8` with 4 U+00A0 | 16 spaces + 4 U+00A0 | 12 spaces + 4 U+00A0 |

Today the anchored cells stay consistent with the closing line: `.ToString()` stands under `.Trim()`, and the hole line `2)}` stands after `M2(`. They have to stay consistent once the shift changes (FMT-INDENT-030).

### Surfaces
| Surface | Must change / must stay / not involved |
|---|---|
| `RawStringLineShifts` (measure and leftward edit) | must change |
| `RawStringLiteralUtilities.GetLeadingWhitespaceLength` (Core) | must stay; it is the shared measure |
| RH5202 analyzer and fix | must stay |
| Rule page RH5202 | not involved |
| Preparation phase (tab replacement) | not involved (it does not touch literal text) |

### Simpler option
The formatter can use the existing `RawStringLiteralUtilities.GetLeadingWhitespaceLength` from `Reihitsu.Core`, which it already references (it calls `RawStringLiteralUtilities.GetQuoteOffset`), instead of widening its private counter.

### Output questions
**2 · Formatter · a raw string whose closing line is indented with tabs**

FMT-RAW leaves this case out (Non-goals, thoenissen/Reihitsu#1432), and the issue excludes it. But the measure the run adopts changes its output, unless the tab is excluded on purpose. Today the formatter's output is reported by RH5202, and the formatter moves the RH5202 fix's output (12 spaces + 4 tabs) back to 16 spaces + 4 tabs. Both behaviors were observed.

Input (`→` is a tab):
```csharp
class C
{
    void M()
    {
        var a = """
→→→→x
→→→→""";
    }
}
```

Before (`main`):
```csharp
        var a = """
                →→→→x
                →→→→""";
```
(16 spaces + 4 tabs; RH5202 reports it.)

After, A (recommended): the tab counts as one column, as RH5202 counts it:
```csharp
        var a = """
            →→→→x
            →→→→""";
```
(12 spaces + 4 tabs; RH5202 no longer reports it; the same as the RH5202 fix's output.)

After, B: unchanged from `main` (16 spaces + 4 tabs; RH5202 keeps reporting it, and the formatter and the fix keep undoing each other).

Why A: the formatter then agrees with the analyzer and its fix for every whitespace character, and thoenissen/Reihitsu#1432 can still decide tab-width alignment later. If the maintainer is not asked, B is the reading that puts nothing visible in question, but it needs a deliberate tab exception in the new measure.

### Internal choices
_None_ besides the question above.

### Issue goals
- "FMT-RAW-009": the requirement is FMT-RAW-001/-002 (already corrected).
- "Tabs are not part of this because the formatter measures after tab replacement": the reason is wrong (output question 2).
- "Check first that the formatter may count a non-space whitespace character": done; FMT-RAW-001 now states it.
- "LF and CRLF, a second pass changes nothing, no RH5202 diagnostic": keep it, widened to the matrix above, including the "none" cell, which shows the fix/formatter conflict.

### Architecture findings
The width of the whitespace in front of a raw string's closing marker, and the shift applied to its lines, are decided in two components with two implementations:
- `Reihitsu.Formatter` uses `RawStringLineShifts.GetLeadingSpaceCount` (spaces only) and rewrites only leading spaces (`GetIndentationLength`, `ReplaceIndentation`).
- RH5202 and its fix use `Reihitsu.Core` `RawStringLiteralUtilities.GetLeadingWhitespaceLength` (every `char.IsWhiteSpace`), and the fix applies its own shift (`RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider`, an insertion of spaces or a removal of leading whitespace).

The disagreement shows in every cell of the table above. This is a shared helper with a private copy next to it (FC-015). A narrow fix that widens the private counter keeps two statements of the measure. The lead decides under `squad/README.md`, *Architecture findings*.

### Follow-up candidates
_None_ beyond the output question.

## Whole run

### Spec diff check (b023a19)
- FMT-PIPE-014 and FMT-BREAK-011/-012/-013/-016, FMT-INDENT-006, FMT-RAW-001/-002, and RH5111-001/-002 agree with the code's dispatch and with every output I ran. The one exception is the RH5111 enum fix result (`A = 1,`, see #1149). That is a handover claim, not spec text.
- FMT-INDENT-006: in every cell I ran, "one level deeper than the bracket" and "the column of the expression's elements" coincide. For an empty collection expression only the first phrase applies, which is consistent.
- FMT-BREAK-016's boundary sentence (`a⏎+ b` against `a⏎+= b`) keeps FC-002 explicit.
- Parameter `=` ownership between FMT-BREAK-015 and -016: see #1149, *Internal choices*.
- No conflict with SHARED-DIAG-010: no layout rule reports any output the run produces. RH5204 checks no `]`, RH5203 no `)`, RH6004 is satisfied by column 0, and RH5106/RH5107/RH5111 are silent on indexers and compound assignments (P11).

### Entry-point predictions
None. Every change sits inside a phase (line-break or indentation). The RH5111 fix keeps calling the node-level entry point unchanged, and the RH5202 fix does not use the formatter.

### Follow-up candidates
Each item is outside the run's issues. The issue manager checks for duplicates; thoenissen/Reihitsu#953 covers the column of kept continuations.

**1 · Formatter · the first type argument pushed onto its own line by a comment**

Input:
```csharp
class C
{
    System.Collections.Generic.List< // c
        int> _x;
}
```

Today: `int> _x;` at column 4, the block column. FMT-INDENT-015 places only later elements, and FMT-INDENT-013 leaves a nested list to the list itself, so no requirement decides this line.

A (recommended): `int>` in the column after `<`, as FMT-INDENT-012 does for an argument that a comment pushes down.

B: unchanged.

Why A: the block column reads as a new member.

**2 · Formatter · a lambda arrow alone on its line**

Input: `System.Func<int> f = ()⏎    =>⏎    1;`. Today `=>` and `1` stand alone in the statement column.

A (recommended): `System.Func<int> f = () => 1;`

B: unchanged.

Why A: FMT-PIPE-014 makes every `=>` one family, and the expression-bodied member's arrow joins its line (FMT-BREAK-026).

**3 · RH5111 · Assignments must have proper line breaks: compound assignments** (P11, the rule's own gap). The formatter now joins `x⏎+= 1`, and RH5111 lists compound assignments as a non-goal. If the rule should report them, that is a behavior change of RH5111.

**4 · RH5106/RH5107 · indexer parameter lists** (P11). thoenissen/Reihitsu#1383 already calls this a separate question. File it only if no issue exists.

**5 · Formatter · the clauses of a query outside a list element** (`var q = from x in s⏎let y = x⏎select y;` puts `let` and `select` in the statement column; `let y⏎= x` keeps the wrap). FMT-INDENT-013 aligns query clauses only inside list elements. Check thoenissen/Reihitsu#953 and the existing issues before filing.

The spec author's three follow-up candidates (anonymous-object `=`, using-alias `=`, list-pattern `]`) stand as they are. The list-pattern `]` comment stays in the `]` column today (I ran it).
