Role: skeptic
Commit: 49cf6f9
Verdict: proceed (with one more output question, Q5, and one architecture finding)

Prompt note: the prompt carried no intended fix. The lead's summary of the agent comment on the issue was treated as a hypothesis and checked against FMT-BREAK-039/040, FMT-UNSUP-014, and the formatter on this commit.

## Premise

The premise is real, but the issue's scope is out of date.

- **Caller.** The analyzer runs on user code. The formatter joins the same gap through whole-document formatting and the CLI (FMT-BREAK-040, `AttachedPartGapUtilities.StartsAttachedPart` in `src/Reihitsu.Formatter/Pipeline/Core/Utilities/AttachedPartGapUtilities.cs`, used by `AttachedPartLineStartRewriter`).
- **No rule reports the shape today.** Fixture `v1.cs` below, run with `scripts/apply-fix.sh <ID> <dir>` for RH6007, RH5307, RH6024, RH5201, and RH5112, gives `no-diagnostic` for every ID under LF and CRLF:

```csharp
#nullable enable

class C
{
    void M(int[] arr, int[]? n, string? s, int[][] grid)
    {
        var a = arr
            [0];
        var b = n
            ?[0];
        var c = n?
            [0];
        var d = s
            ! + "";
        var e = grid
            [0]
            [1];
        var f = s
            !
            ?[0];
    }
}
```

  `scripts/format.sh` turns the body into `arr[0]`, `n?[0]`, `n?[0]`, `s! + ""`, `grid[0][1]`, `s!?[0]`.
- **The formatter joins everywhere, as the issue comment says.** A context fixture puts a part below its expression in 24 contexts. `scripts/format.sh` joins every one of them in one pass, and a second pass changes nothing. The contexts: field initializer, property initializer, expression-bodied property, constructor initializer argument, switch-expression arm, query `in` clause, expression lambda, interpolation hole with format, raw interpolated hole, `ref` local, cast-parenthesized expression, `[^1]`, `[1..]`, `d["k"]`, `new int[] {…}[0]`, `(f ? arr : arr)[0]`, `s! is { }`, `foreach` header, `fixed` header, tuple deconstruction target, array size `new int[arr[0]]`, raw string literal `"""…"""[0]`, `if` condition, and `throw new Exception(s!)`. So no reported part is one the formatter keeps, and RH5115-001 meets SHARED-DIAG-010.
- **In a chain with links, RH5201 already reports some inputs.** For the two inputs of RH5115-005's example, `apply-fix.sh RH5201` reports `.C` (misaligned) and its fix gives exactly RH5115-005's expected output (`x.B()[0]⏎                 .C();`, `x[0].B()⏎                    .C();`). RH5112 reports neither. RH5201 does not report a chain whose later links happen to line up, or a chain with no later link (`x.B()⏎[0]`). So RH5115 adds the report there. In the RH5115-005 inputs the two rules report different tokens and their fixes give the same output. That is not a conflict.

## Mechanism and candidates

_N/A_: this is new behavior, not a bug.

The candidate set comes from `AttachedPartGapUtilities.StartsAttachedPart`/`IsAttachedPartGap`, the formatter's single statement of the gaps, minus the `(` case:

- `[` of `BracketedArgumentListSyntax { Parent: ElementAccessExpressionSyntax }`;
- `?` of `ConditionalAccessExpressionSyntax` whose next token is the `[` of an `ElementBindingExpressionSyntax`, plus the `?`→`[` gap;
- `!` of `SuppressNullableWarningExpression` that is not the first token of a link operator, decided through `FluentChain.GetLinkOperatorTokens`.

RH5115-001 lists the same three parts. Its exclusions match the code, including `!?[` (a part) versus `!?.` (a link).

## Breaking inputs

These failure classes can reach the change. Each item names the inputs the test implementer must cover.

- **FC-019, FC-020, FC-021, FC-005: trivia in the gap.**
  - `//` behind the token in front.
  - `/* */` ending mid-line in front of the part.
  - A comment on its own line in the gap.
  - A comment between `?` and `[` (`n? // c⏎[0]`): reported on `?`.
  - `#if X / #else / #endif`, as in RH5115-002 `b`. In a build that defines `X`, the `[` of `[0]` is reported as well.
  - `#if false … #endif` (disabled text only), `#pragma warning disable`, and `#region`/`#endregion` in the gap.

  Each of these is reported and gets no fix (RH5115-002/006).
- **FC-003: both sides of the line-break condition.** `arr /* c */ [0]` and `n? /* c */ [0]` on one line are not reported. The same with a line break is reported. Cover blank lines in the gap (RH5115-001 `e`). Cover a token in front that spans lines, such as the closing `"""` of a raw string: `"""⏎abc⏎"""⏎[0]` is reported.
- **FC-002: the "not followed by `.` or `?.`" qualifier.** `s⏎!.Length` and `s⏎!?.Length` are not reported. `s⏎!?[0]` is reported on `!`. `s!⏎?[0]` is reported on `?`. `s⏎!⏎?[0]` gives two diagnostics, on `!` and on `?`. `f⏎!(1)` is reported on `!` only, not on `(`.
- **FC-027: enclosing contexts.** Use the 24 contexts listed under *Premise*, each one positive diagnostic, with a few in fix tests. Also `this⏎[0]`, `base⏎[0]`, `d["k"]⏎[0]`, and `(arr)⏎[0]`.
- **FC-026: identical tokens on one line.** `grid⏎[0]⏎[0]` gives two diagnostics. Fix All must join both.
- **FC-008: Fix All on overlapping spans.**
  - Two reported parts in one chain with links (`x.B()⏎[0]⏎[1]⏎.C()`): both diagnostics lay out the same chain.
  - A mixed case (`x.B(arr⏎[0])⏎[1]⏎.C()`): `[0]` takes the join (RH5115-004, because `arr[0]` has no link), and `[1]` takes the chain layout of a chain that contains `[0]`'s gap.
  - One Fix All application must clear every RH5115 diagnostic in the document (SHARED-FIX-002).
- **FC-043 / SHARED-FIX-010.** In `x.B(Foo /* c */ (1))⏎[0]⏎.C()`, the reported gap is clean, but the chain the fix formats holds an FMT-UNSUP-014 shape in an argument. The RH5115-005 fix is therefore withheld and the diagnostic stays. By RH5115-004 the join formats nothing and asks nothing, so `var v = arr⏎[0] + Foo /* c */ (1);` must still be joined. This is a consequence of the spec, not an observed result.
- **FC-007: the fix raises another rule's diagnostic.** See Q5. The test implementer locks the answer in. Without a change, the join leaves RH5201 and RH5203 diagnostics behind, which `apply-fix.sh` showed (quoted under Q5).
- **FC-007 / SHARED-FIX-004, observed.** `int[] e = [arr⏎            [0], 2];` is reported by RH5305 today. RH5305's fix gives the multi-line layout `[⏎ arr[0],⏎ 2⏎]`, which matches FMT-BREAK-040's "a construct FMT-BREAK-022 decides still counts it". RH5115's join gives `int[] e = [arr[0], 2];`. Whichever fix runs first decides the result, and neither undoes the other: after the join RH5305 has nothing to report, and after RH5305's fix RH5115 has nothing to report. So no requirement is broken. A test should assert that the RH5115 join result leaves no RH5305 diagnostic.
- **FC-013: formatter output flagged.** Run the analyzer over the formatted output of the context battery: no RH5115 diagnostic.
- **FC-017, FC-033: line endings.** Run CRLF and LF in the fix tests, with a CRLF line break in the gap.
- **FC-040: documentation mode.** `arr⏎/// c⏎[0]` must be reported in every documentation mode, with no fix.
- **FC-015: predicate copy.** See *Architecture findings*.

The test implementer rebuilds the context fixture from the list under *Premise*. Every fixture lived in my scratchpad and is deleted.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzer (new RH5115) | must change: new analyzer, resources, `Release.ruleset` entry |
| Code fix (new RH5115) | must change: join (SHARED-WS-003) outside a chain with links; `FluentChainCodeFixHelper` chain layout inside one; Fix All |
| Fix All | must change (FC-008 inputs above) |
| `Reihitsu.Core` | must change if the architecture finding is taken: the attached-part predicate moves here |
| Formatter (`AttachedPartGapUtilities`, precheck, line-break phase) | behavior must stay; changes only to use the moved predicate, if the finding is taken |
| RH5201, RH5112 | must stay (they report links; RH5115-005's inputs are reported by both RH5201 and RH5115, with the same fix output) |
| RH5105, #1827's future rule | not involved (argument lists are a non-goal under Q1 A) |
| Rule page `documentation/rules/RH5115.md` | must change (new page; already written) |
| Specs | `RH5115.md` may get a sentence for Q5's answer; nothing else |

## Refuse option

- **No rule at all.** This is the issue's own first alternative. P11 makes the formatter-only join intended, and the CLI already joins every such part. It costs nothing, and analyzer-only users stay unaware of the layout. RH5105 and RH5113 are the precedent for reporting a token the formatter joins onto the line in front. The issue is milestoned (1.5.0) and the class is not rare for an analyzer-only user who wraps chains. I would not take this option, but the maintainer can.
- **Rule without a fix.** Withholding the fix for every input saves only the join (an existing SHARED-WS-003 pattern, as in RH5105) and the delegation to the existing `FluentChainCodeFixHelper`, so almost nothing. Not worth it.
- **Withhold the fix for the comment/directive class.** This is already in the spec (RH5115-006), with a cheap trivia check on the gap. The class is rare: it is FMT-UNSUP-014's.
- **Declare a shape unsupported.** No new shape. The only shape this rule meets, a comment, directive, or disabled text in the gap, is already declared (FMT-UNSUP-014). No special case in this change would be deleted by declaring anything else.
- **Withhold the join when a later line is aligned to code behind the part (Q5).** This would be a prediction of the indentation phase's columns (FC-044) and is not a cheap check. I reject it as an option.

## Output questions

I agree with the spec author's four recommendations: **1A, 2A, 3A, 4** as proposed. For 3A, RH5201's existing fix already gives exactly that output on the same inputs. One more question:

**5 · RH5115 · Index and null-forgiving operator must stay on the line of their expression: the fix outside a chain, with later lines aligned to code behind the part**

Input:
```csharp
var b = s
    ! + Foo(1,
            2);
var c = Foo(arr
        [0]).ToString()
            .Trim();
```

Before (`main`): not reported.

After, A (recommended): only the gap is deleted (SHARED-WS-003, as the RH5105 fix does for `void M⏎    (int a,⏎     int b)`). The lines behind keep their columns. RH5203 then reports `2`, and RH5201 then reports `.Trim`. Their fixes realign the lines, as `apply-fix.sh RH5203` and `apply-fix.sh RH5201` showed on this output:
```csharp
var b = s! + Foo(1,
            2);
var c = Foo(arr[0]).ToString()
            .Trim();
```

After, B: in `c`, where the reported gap lies inside a chain with links that the part is not attached to, the fix lays out that outermost chain. `b` stays as in A, because no chain holds it.
```csharp
var b = s! + Foo(1,
            2);
var c = Foo(arr[0]).ToString()
                   .Trim();
```

Why A: it is the RH5105 precedent, it changes nothing but the reported gap (SHARED-FIX-006), and the input class (a part below its expression with an aligned continuation behind it) is rare. B fixes only the chain half and adds a second chain condition to the fix. If the maintainer picks A, the spec author adds one sentence to RH5115-004 so the misalignment reads as a decision.

## Internal choices

- **Where the attached-part predicate lives.** See *Architecture findings*. I recommend moving it to `Reihitsu.Core`.
- **How the fix chooses between join and chain layout.** I recommend `FluentChain.Create` on the part's outermost chain node, `Links.Count > 0` (SHARED-CHAIN-001). `FluentChain` already lives in Core, so this restates nothing.

## Issue goals

The issue is agent-filed. Goals I would drop or change:

- **"Inside a chain that has at least one link"** and **"Do not report chains without a member access, except `?[`"**: drop. They are stale since FMT-BREAK-040. Dropping them widens the diagnostic (visible), which is covered by Q1.
- **"Do not report when a comment, directive or disabled text sits in the gap"**: drop. It is stale since FMT-UNSUP-014, and SHARED-DIAG-010 now asks for the report. Visible; covered by Q2.
- **The argument list `x.B⏎(1)` in the example**: drop and leave to thoenissen/Reihitsu#1827. Visible; covered by Q1.
- **"Use the shared `FluentChain` model (`AttachedParts`) so analyzer and formatter cannot disagree"**: change it to "use the formatter's attached-part predicate, moved to Core". The formatter's join no longer reads `FluentChain.AttachedParts`; it reads `AttachedPartGapUtilities`. The two classify `x!?[0]` differently: `FluentChain` folds the `!` into the conditional element access's tokens, and the join treats it as a part of its own. Internal; no visible output.
- **"Formatter owner: `ChainLineBreakRewriter.NormalizeChain`"**: stale. The owner is the attached-part gap in the line-break phase. Internal.
- **Code fix "delegate to `FluentChainCodeFixHelper.FormatChainAsync`"**: keep it only for parts in a chain with links (RH5115-005). Outside a chain the join is the whole fix. Visible; covered by Q3 and the change doc.

Severity Warning, on by default, with Fix All, matches RH5105, RH5112, RH5113, and RH5201.

## Architecture findings

**One rule stated in more than one place (FC-015).** The decision is which token starts an element access, a conditional element access, or a null-forgiving operator part, and which trivia form its gap.

- It is stated once today, in `AttachedPartGapUtilities` (`internal`, in `Reihitsu.Formatter`). Two components use it: the precheck (FMT-UNSUP-014) and the line-break phase's join (FMT-BREAK-040).
- The RH5115 analyzer must make the same decision. `Reihitsu.Analyzer` references only `Reihitsu.Core` (`src/Reihitsu.Analyzer/Reihitsu.Analyzer.csproj:23`), and `Reihitsu.Formatter`'s `InternalsVisibleTo` does not name it (`src/Reihitsu.Formatter/Properties/AssemblyInfo.cs`). So the analyzer cannot call the formatter's helper and would need a private copy.
- The copy would have to restate the `!`-is-not-a-link-operator rule and the `?`→`[` gap. That is where a drift would show: for example, the analyzer reporting `x⏎!?.y`, or missing `x!⏎?[0]`.
- The helper's dependencies are already in Core (`FluentChain.GetLinkOperatorTokens`, `SyntaxTokenNavigationUtilities`). Moving it, or its `[`/`?`/`!` cases, into `Reihitsu.Core` is a move of about 117 lines with no new logic. It stays within the analyzer and its formatter counterpart.
- The RH5115-004 fix's trivia check for RH5115-006 can then read the gap from the same predicate.

## Follow-up candidates

- **thoenissen/Reihitsu#1827 (as the spec author noted).** If 1A stands, comment that this run leaves every argument list's `(` to it. Also note that its proposal ("withheld where a comment … stands in the gap", modelled on RH5105's SHARED-JOIN-001) predates FMT-UNSUP-014. For the `(` of an argument list, FMT-UNSUP-014 refuses such a gap, so SHARED-DIAG-010 asks for a report without a fix, as RH5115-002/006 do, not for silence.
- **Formatter: the continuation clauses of a query expression outside a list element are put at the statement's column.** No requirement decides this; FMT-INDENT's list-element rule (`sdd/specs/formatter/indentation.md:588`) covers only queries inside a list element. It reproduces without any attached part, so it is not this run's.

  **Formatter · query clause below `from`, outside a list element**

  Input:
  ```csharp
  var b = from v in arr
          select v;
  ```
  Before (`main`, `scripts/format.sh`):
  ```csharp
  var b = from v in arr
  select v;
  ```
  After, A (recommended):
  ```csharp
  var b = from v in arr
          select v;
  ```
  After, B: unchanged from today.

  Why A: the clauses of one query read as a column under `from`, which is what the list-element rule already demands inside an argument.
