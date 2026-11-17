Role: skeptic
Commit: c920452
Verdict: proceed narrower

My prompt named the issue, the slug, and the handovers to read. It carried no intended fix or suspected cause. I treated the issue's "The maintainer decided" as a claim (see *Premise*).

"Narrower" does not mean a smaller shape. It means the spec diff has to absorb two unchanged requirements that now contradict FMT-UNSUP-011 (RH2001-007, RH7101-007) and one undecided input (`is` followed by a constant name) before tests start. The shape and its deletions hold.

## Premise

- **The issue is real, and callers reach it.** Whole-document formatting (CLI) reaches it. The RH5302 fix reaches it through `FormatNodeInDocumentAsync` on the outermost `&&`/`||` chain. Every fix that formats a node holding a binary expression reaches it too. Fixture `F1.cs` is the issue's input, verbatim. `scripts/format.sh <dir>` on `c920452` (production code = `main`) printed `Formatted 3 of 3`. The diff on `F1.cs` was `-                b;` / `+        b;`: the right operand lands in column 8, the statement column, as the issue states.
- **The mechanism matches the code.**
  - `BinaryOperatorLineBreakRewriter.NormalizeBinaryOperatorPosition` returns early when `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(operatorToken, node.Right.GetFirstToken())` is true. That helper delegates to `SyntaxTriviaUtilities.ContainsUnjoinableTrivia`, which tests for comments, directives, and disabled text (`src/Reihitsu.Core/SyntaxTriviaUtilities.cs:213-234`).
  - RH5302's fix carries a copy: `IsTrailingOperator(...) && SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(...)` (`RH5302LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider.cs:109-110`). `IsTrailingOperator` has no other caller.
- **The maintainer's answer is not traceable in the repository.** The escalation-10 answer exists only as a sentence in the `agent-filed` issue body. The issue has no comments, and nothing under `sdd/changes/` or `squad/notes/` records it. The change doc's visible changes go to the maintainer as output questions anyway. The lead should make that batch read as a confirmation of the declaration itself, not only of its consequences.
- **Before this change, RH5302 contradicted SHARED-DIAG-010.** The rule reported `a && // note⏎b`, a gap the formatter keeps. After the change, the shape is refused as unsupported, and SHARED-DIAG-010 lets RH5302 report it. That is consistent.

## Mechanism and candidates

_N/A as a bug._ This is a behavior change.

The dispatch is `BinaryExpressionSyntax`. It is what the precheck will iterate, what `BinaryOperatorLineBreakRewriter.VisitBinaryExpression` visits, and what `CommentPositionUtilities.IsBinaryOperator` (RH5049) tests.

**Candidates (the operator kinds):**
- `Add`, `Subtract`, `Multiply`, `Divide`, `Modulo`
- `LeftShift`, `RightShift`, `UnsignedRightShift`
- `LessThan`, `LessThanOrEqual`, `GreaterThan`, `GreaterThanOrEqual`
- `Equals`, `NotEquals`
- `BitwiseAnd`, `BitwiseOr`, `ExclusiveOr`
- `LogicalAnd`, `LogicalOr`
- `Coalesce`
- `IsExpression`, `AsExpression`

**Neighbors that are not binary expressions:**
- `IsPatternExpressionSyntax`
- `BinaryPatternSyntax`, `RelationalPatternSyntax`, `UnaryPatternSyntax`
- `AssignmentExpressionSyntax` (including `??=` and compound assignments)
- `RangeExpressionSyntax`
- `ConditionalExpressionSyntax` (FMT-UNSUP-008)

**Settled fact: `is` followed by a constant name is a binary expression.** Fixture `F2.cs`:

```csharp
enum Color
{
    Red
}

class C
{
    bool M(object o)
    {
        var a = o is
            Color.Red;
        var b = o is
            string;
        var c = o is
            5;

        return a && b && c;
    }
}
```

`scripts/format.sh` turned `a` into `o⏎                is Color.Red;` and `b` into `o⏎                is string;`. It left `c` as `o is⏎        5;`. So the line-break phase handles `o is Color.Red` as a binary `is`, exactly like `o is string`, and handles `o is 5` as a pattern.

## Breaking inputs

**Failure classes that reach the change:** FC-038, FC-039, FC-040, FC-041, FC-042, FC-043, FC-003, FC-005, FC-019, FC-020, FC-021, FC-027, FC-013.

**FC-038 (a phase produces the shape) — probed on `main`, none found.** Fixture `G.cs`, run through `scripts/format.sh` twice. The second pass printed `Formatted 0 of 1`.

```csharp
class C
{
    bool _r;

    void M(bool a, bool b, bool c, int n)
    {
        _r = a &&
             b /** d1 */ && c;
        _r = a &&
             b /// d2
             && c;
        _r = a
             && b /// d3
             && c;
        _r = n + n /** d4 */ * n > 0;
        _r = a &&
             b /** d5 */;
        _r = a /** d6 */
             && b;
    }
}
```

The output only moved the trailing `&&` to the line start (`_r = a⏎     && b /** d1 */ && c;` and the same for d2 and d5). The documentation-comment phase relocated no comment into an operator gap.

Fixture `F3.cs` checked comments in front of an operator. `a⏎/// doc⏎&& b`, `a /** doc3 */ &&⏎d`, and `a // c4⏎&&⏎b` kept the comment in front of the operator. The output was `/** doc3 */ && d` and `// c4⏎&& b`, plus the FMT-BLANK-011 blank line in front of the comment line.

I read the structural transforms (`StructuralTransformPhase.CreateRewriters`). None builds a binary expression or carries trivia into an operator gap. `FormatterTestsBase` already asserts that every test's output is not refused, so FC-038 is checked across the suite once the precheck exists.

**Class matrix for FMT-UNSUP-011.** The test implementer settles these after implementation.

- **Operator axis:** each group of the candidate list above (arithmetic, shift, relational, equality, bitwise, logical, `??`, `is` type, `as`), plus `is` followed by a constant name.
- **Trivia axis:**
  - `//` and `/* */`
  - `///` and `/** */`, in each documentation mode (FC-040). The gap is the operator's trailing trivia plus the right operand's first-token leading trivia, so it does not matter which token the parser attaches the text to.
  - Directives: `#if`/`#elif`/`#else`/`#endif`, `#pragma`, `#region`, `#nullable` (FC-005, FC-020)
  - Disabled text, with the symbol defined and with it undefined
- **Position axis:** the operator's trailing trivia on the same line; a line of its own in front of the right operand; a multi-line `/* */`.
- **Enclosing-construct axis (FC-027/FC-039):**
  - expression statement
  - local declaration value (FMT-UNSUP-004 excepts it, so FMT-UNSUP-011's reason applies)
  - field with one declarator
  - field with several declarators, directive in a value (FMT-UNSUP-010's reason)
  - attribute argument of a declaration (FMT-UNSUP-004's reason)
  - attribute argument of a method (FMT-UNSUP-011's reason)
  - lambda body inside a value
  - interpolation hole
  - query clause
  - second operator of a chain
  - binary expression nested in another binary's operand
- **Input-level axis (FC-041):** document; `FormatNode` on the binary expression; `FormatNode` on the right operand alone, which is not refused; a context entry point whose context holds the binary expression.
- **Other side (FC-003):** a comment behind the left operand; a comment on its own line above the operator; `a && (/* c */ b)`; `n + M(/* c */ 1)`; `n is > /* c */ 1 or /* c */ 3`; `n += /* c */ 1`; `c ? /* c */ a + b : d` (FMT-UNSUP-008 only); the leading-operator `#if` layout.

**Ties.** FMT-UNSUP-008 and FMT-UNSUP-009 cannot start at the same trivia as FMT-UNSUP-011:
- FMT-UNSUP-008: the token in front of the right operand's first token is the binary operator, so it is never `?` or `:`.
- FMT-UNSUP-009: its gap ends at a comma behind the element's last token, so it never covers the gap behind an operator.

Only FMT-UNSUP-004 and FMT-UNSUP-010 can tie, as the spec states. Today the tie order lives in `PrecheckPhase.FindUnsupportedSyntax` (the candidate lists at lines 86-102). The new candidate has to rank after `firstComment` and `firstFieldDirective` in both lists.

**Unreachable, dropped:** a block comment inside an `#if` condition. Fixture `D.cs`, `#if DEBUG && /* c */ TRACE`, gave `Skipped 1 file(s) with syntax errors.`

**Existing tests whose input or output holds the shape** (found by `rg` over `src/`). These flip or need a different construct:

- **Formatter:**
  - `MultiLineCommentGapSpacingTests.SpaceBehindMultiLineCommentInFrontOfBinaryOperandStaysAsBehindSingleLineComment` and `SpaceDemandedBehindMultiLineCommentIsInsertedAsBehindSingleLineComment` (FMT-SPACE-001, FMT-SPACE-013)
  - `FieldDeclarationSplitTests.CommentInsideTheValueOfALaterDeclaratorStaysInThatValue` (FMT-STRUCT-022)
  - `DelimitedDocumentationInExpressionTests.MovesDelimitedDocumentationBehind{Addition,Coalesce,ConditionalAnd}Operator`
  - `CommentJoinTests.BinaryOperatorIsNotCollapsedIntoTrailingComment` and `DirectiveJoinTests.BinaryOperatorIsNotCollapsedAcrossDirective` (FMT-BREAK-001)
  - `IdempotencyTests` around line 904 (`First() + // binary comment`)
  - `JoinSeparatorPhaseTests.CommentInTheGapStillRefusesEveryJoin` (FMT-BREAK-031). It runs the phase alone: once the guard is deleted, its binary-operator member is unreachable input for the phase.
- **Analyzer:** `RH2001...VerifyFixIsOfferedForACommentInsideTheInitializerValue` (RH2001-007), see *Output questions* 2. The RH3003, RH3005, RH3006, RH5306, and RH6024 tests that hold such comments are unaffected: those fixes do not format through the formatter, and `v is // note` is a pattern `is`.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `PrecheckPhase` (new shape, reason constant, tie order) | must change |
| `BinaryOperatorLineBreakRewriter` join refusal | must change (delete) |
| RH5302 code fix: the guard and `IsTrailingOperator` | must change (delete) |
| `SyntaxTriviaUtilities`/`LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` | must stay (other callers) |
| `ListElementInteriorJoinRewriter`, `BinaryExpressionContributor`, `NormalizePatternOperatorPosition` | must stay |
| RH5302, RH5049, and other analyzers | must stay (analyzers do not consult the precheck; SHARED-DIAG-010 allows them to report refused shapes) |
| Formatting code fixes | no code change. Their offering changes through SHARED-FIX-010. RH2001 and RH7101 specs must change (see *Output questions*). |
| CLI | no code change. The message comes from the reason constant. |
| `Reihitsu.Core` | not involved |
| Rule pages | not involved (general-notes covers a diagnostic without a fix) |
| Specs | RH2001-007 and RH7101-007 must change. FMT-UNSUP-011 must decide `is` followed by a constant name. `indentation.md` non-goal: internal, see below. |
| Tests | the list above |

## Refuse option

- **Requested change (the declaration).** One finder in the precheck plus one reason constant, an estimated 20-40 lines. It deletes about 15 lines (the line-break guard, RH5302's guard, and `IsTrailingOperator`). No new type, no new mechanism.
- **Refusing locally** is today's behavior: RH5302's fix is withheld, and the formatter leaves the operand at the statement column. It costs nothing and keeps two copies of one decision (FC-015/FC-042).
- **Rarity.** RH5049 reports every ordinary comment behind a binary operator, so ordinary comments are rare by definition. Documentation comments documenting nothing are also reported, because SHARED-COMMENT-001 counts them as ordinary. Directives and disabled text are **not reported by any rule**, so their rarity is an empirical claim, not a definition. The change doc's *Why* says "RH5049 already reports a comment behind a binary operator, so the shape is rare". That covers comments only, and the text should say so. I judge directives behind a trailing operator (change 4, for example `RegexOptions` flags under `#if`) uncommon, not common, so escalation 9 does not apply.
- **The same-line member (change 3, `x * /* scale */ 2`) deletes nothing.** Both guards act only when a line break separates the operator from the operand: the line-break join needs a trailing end of line, and RH5302 reports only an operator that ends or starts a line. Including this member rests only on the FMT-UNSUP-008 precedent and on keeping one rule. A narrower shape that requires a line break in the gap is cheap to check and would delete the same guards. The lead's output question for change 3 should state that this member deletes nothing.
- **Shape as specified:** from the trailing trivia of a binary expression's operator through the leading trivia of its right operand's first token. Everything in front of the operator and everything inside the right operand behind its first token is excepted. The special cases it deletes are named above.
- **Common code it refuses:** `a && // explanation⏎b` (reported by RH5049) and trailing-operator `#if` operand lists (not reported).

## Output questions

**1 · Formatter · `is` followed by a constant name, with a comment behind `is`**

FMT-UNSUP-011 defines the operator by reference to SHARED-COMMENT-007. That requirement says it "does not cover an `is` followed by a name that denotes a constant". The precheck sees syntax only, and fixture `F2.cs` shows the formatter handles `o is Color.Red` as a binary `is`. Reading B would need a semantic model the precheck does not have. It would also keep the line-break guard for this one case, which the guard deletion cannot then remove.

```csharp
enum Color
{
    Red
}

class C
{
    bool M(object o) => o is /* c */ Color.Red;
}
```

Before (`main`, `scripts/format.sh`):

```csharp
enum Color
{
    Red
}

class C
{
    bool M(object o)
    {
        return o is /* c */ Color.Red;
    }
}
```

After, A (recommended): the file is unchanged, and the CLI reports `error: <file>(8,30): unsupported syntax: a comment or directive between a binary operator and its right operand. The file was left unchanged.`

After, B: formatted as on `main`.

Why A: it is the only reading a syntax check can decide. The spec needs one sentence saying so, instead of relying on SHARED-COMMENT-007.

**2 · RH2001 · Private auto-properties should not be used — a comment behind an operator in the initializer**

RH2001-007 (unchanged) says "A comment inside the initializer's value, between its first and its last token, does not withhold the fix". It is tested by `VerifyFixIsOfferedForACommentInsideTheInitializerValue`. The fix asks `FindUnsupportedSyntax` about the field it builds (`RH2001...CodeFixProvider.cs:717`). That field holds the shape, so after the change the fix is withheld. The change doc lists neither RH2001-007 nor RH2001.

```csharp
class C
{
    private int Value { get; set; } = 2 + /* three */ 3;

    int M()
    {
        return Value;
    }
}
```

Before (`main`, `scripts/apply-fix.sh RH2001`, LF and CRLF): fixed.

```csharp
class C
{
    private int _value = 2 + /* three */ 3;

    int M()
    {
        return _value;
    }
}
```

After, A (recommended): no fix offered; the diagnostic stays.

Why A: it follows from the declaration and SHARED-FIX-010. RH2001-007's sentence then needs the exception, the change doc needs RH2001-007 under *Affected requirements*, and the test needs a different value comment, such as `M(/* three */ 3)`.

**3 · RH7101 · Do not combine fields — a comment behind an operator anywhere in the type**

RH7101 asks `FindUnsupportedSyntax(fieldDeclaration.Parent)` (`RH7101DoNotCombineFieldsCodeFixProvider.cs:113`). Its fix is therefore withheld for every combined field when **anything in the type** holds the shape, including a method far from the field. RH7101-007 (unchanged) also says "neither does a comment inside an initializer value" withholds the fix. `private int _a = 1 + /* c */ 2, _b;` was FMT-UNSUP-005's old example and is now withheld. RH7101's spec states this type-wide refusal for FMT-UNSUP-004 and FMT-UNSUP-010, but not for the new shape.

```csharp
class C
{
    private int _a, _b;

    int M(int x)
    {
        return x + /* c */ 1;
    }
}
```

Before (`main`, `scripts/apply-fix.sh RH7101`, LF and CRLF): fixed, `private int _a;⏎    private int _b;`.

After, A (recommended): no fix offered; the diagnostic stays.

Why A: RH7101 formats the whole type (RH7101-004). This is the existing SHARED-FIX-010 consequence, the same as for the earlier shapes. The change doc's change 6 should show this case explicitly, because the visible effect lands far from the comment. RH7101-007 and RH7101's `Uses` need FMT-UNSUP-011.

## Internal choices

- **RH5302-005, rewrite in place or retire.** I agree with rewriting it in place, following the RH5308-005 precedent. Retiring it would leave the existing `[Requirement("RH5302-005")]` tests without a home. No visible difference.
- **`indentation.md` non-goal (line 19).** It names "the right operand of a wrapped binary expression" as an unowned continuation column. After the change, no supported input leaves a right operand at a line start. The join runs whenever the operator has a trailing end of line, and a blank line in the right operand's leading trivia is removed by `RemoveLeadingEndOfLineAndWhitespace`. Fixtures `G.cs` and `F2.cs` show the join applied. I recommend dropping that example from the non-goal so the spec does not name an unreachable case. Leaving it is harmless.
- **Change doc rarity sentence.** Its *Why* should say that RH5049 covers comments, and that directives and disabled text are unreported but rare.

## Issue goals

- "Delete requirement RH5302-005": changed to "rewrite in place". No visible output change.
- "The `b + // c` column-8 layout stops being reachable and needs no repair": true for binary right operands. It is overstated as a claim about the layout. The same statement-column fallback stays for pattern operators. Fixture `H.cs` on `main` produced `var h1 = o is⏎        not null;`, `var h2 = n is > 1 or⏎        3;`, and `var h4 = o is⏎        int i;`. thoenissen/Reihitsu#953 already tracks that fallback. No visible change from this run.
- "Every code fix whose formatted node holds the shape is withheld": this needs no code, but it does need the RH2001-007 and RH7101-007 spec changes (*Output questions* 2 and 3).
- The other goals stand. The CLI line `(5,22)` matches the column of `// c1` in the issue's input.

## Architecture findings

_None new._ The duplicated join refusal in RH5302's fix (FC-015/FC-042) is a pre-existing case of two owners for one decision, and this change removes it.

## Follow-up candidates

- **Update thoenissen/Reihitsu#953 (issue manager).** Its input example (`var y = 1 + // c⏎2;`) and its listed members `1 + // c⏎2` and `a && // c⏎b` are refused as unsupported after this change. Its remaining members include the pattern continuations in fixture `H.cs` (`o is⏎not null`, `o is⏎int i`), which the issue does not list explicitly yet.
- **RH7101's spec is silent about FMT-UNSUP-008 and FMT-UNSUP-009.** Its `Uses` lists only FMT-UNSUP-004, -007, and -010, although its fix also asks the precheck about the whole type for the conditional and comma shapes. This is pre-existing spec under-statement. It can be folded in if the spec author amends RH7101 for *Output questions* 3; otherwise it becomes a follow-up.
