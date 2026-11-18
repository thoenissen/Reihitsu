Role: skeptic
Commit: abae9f8
Verdict: proceed

The prompt carried no intended fix or hoped-for verdict. The issue's owner hint (`ChainLineBreakRewriter.JoinConditionalElementAccesses`, `NormalizeChain`) is a prescribed solution and is ignored here, as the spec author also ignored it.

## Premise

Real, for the element access and the null-forgiving operator. The argument-list case and the `?[` case outside chains are already joined on `main` (FMT-BREAK-039; `ChainLineBreakRewriter.JoinConditionalElementAccesses`), as the issue's 2026-11-17 comment and the spec author's table say.

Callers that reach it: the CLI and whole-document formatting (`scripts/format.sh`, below), and every code fix whose formatted node holds such a part (the spec author's RH5302 runs with `scripts/apply-fix.sh`, change doc change 5).

Evidence. All runs used `scripts/format.sh <file>` on `main` code (HEAD abae9f8 changes nothing under `src/`). The fixtures were in the scratchpad, outside the repository.

Fixture `A.cs`, the relevant lines of the input and then the output:

```csharp
// input (inside a method, 8 columns deep)
int P => arr
    [0];
var f = a
    [
        0
    ];
var g = a
    [^1];
var i = p          // int* p
    [0];
string j = default
    !;
var l = s
    ![0];
var r = """
    raw
    """
    [0];
var t = new C { arr = { [0] = a
    [0] } };
var v = o switch
{
    int[] w => w
        [0],
    _ => 0
};
var y = (s
    !).Length;
var d = new Dictionary<string, int>
{
    ["a"] = 1,
    ["b"] = 2
};
```

```csharp
// main
int P => arr
[0];
var f = a
[0];
var g = a
[^1];
var i = p
[0];
string j = default
!;
var l = s
![0];
var r = """
        raw
        """
[0];
var t = new C
        {
            arr =
            {
                [0] = a
[0]
            }
        };
var v = o switch
        {
            int[] w => w
[0],
            _ => 0
        };
var y = (s
!).Length;
var d = new Dictionary<string, int>
        {
            ["a"] = 1,
            ["b"] = 2
        };
```

Every part outside a chain with links stays below its element, at the enclosing statement's or member's column. The dictionary initializer's `["a"]` (an implicit element access) is not touched today.

Fixture `D.cs` confirmed the change doc's *before* blocks for change 2 (`c ? arr⏎[0] : 1`) and change 3 (lines 1–3: `Baz(s⏎!, 2)` → one argument per line, `s` and `!` still split; `Baz(n⏎?[0], 2)` → `Baz(n?[0],⏎2)`; `Baz(list.Count.ToString()⏎!, 2)` → `Baz(list.Count.ToString()!,⏎2)`). It also showed `Baz(arr⏎[0], 2)` → `Baz(arr[0], 2)`: an element access directly in a list element is already joined before the list is laid out.

Existing test that asserts today's non-join and will change: `Reihitsu.Formatter.Test.Regression.LineBreaks.ChainLinkLayoutTests.ElementAccessWithoutMemberAccessKeepsItsBreak` (`list⏎[0]` kept, `[0]` at the statement column). It carries no requirement tag. Its expected value changes under change 1 of the change doc.

Hand-joined forms, run twice with `scripts/format.sh` (fixture `B.cs`), were stable on the second pass ("Formatted 0 of 1 file(s)"):

```csharp
var r = """
        raw
        """[0];
var w = new Dictionary<string, int[]>
        {
            ["a"] = new[] { 1 }
        }["a"];
var v = (o switch
         {
             int[] x => x,
             _ => a
         })[0];
var x2 = s!!;
```

So the result of joining onto the closing line of a multi-line receiver (a raw string, an initializer, a switch expression) is a fixed point today. `s!!` is formatted, which means it parses without a syntax error: two `!` tokens joined without a separator read back as two tokens.

Severity: the issue is an `enhancement`, and that is right. The input class (a line break typed in front of `[` or a trailing `!`) is uncommon in hand-written code. No production file under `src/` holds it: `rg -U` for a split element access and for a line that starts with `!` found only test inputs. Today's output is still poor: the part lands at the statement's column and reads as a collection expression, an attribute, or a negation. The same part is joined in a chain, and the maintainer already chose the join for argument lists (FMT-BREAK-039, `sdd/changes/2026-11/argument-list-below-name.md`).

## Mechanism and candidates

_N/A_ (behavior change). Facts about today's dispatch, for whoever enumerates the inputs:

- The candidate set comes from SHARED-CHAIN-001's attached parts other than an argument list. The defining types are `ElementAccessExpressionSyntax` (its `ArgumentList.OpenBracketToken`), a `ConditionalAccessExpressionSyntax` whose `WhenNotNull` starts with `ElementBindingExpressionSyntax` (the `?` token and the `[`), and `PostfixUnaryExpressionSyntax` of kind `SuppressNullableWarningExpression` that is not followed by `.` or `?.` (followed by `[`, `?[`, `(`, `;`, an operator, or `)`).
- Not in the set, though `[` follows an expression-like token: `ImplicitElementAccessSyntax` (`["a"] = 1` in an object or dictionary initializer, which also has a `BracketedArgumentListSyntax`), `ImplicitArrayCreationExpressionSyntax` / `ImplicitStackAllocArrayCreationExpressionSyntax` (`new⏎[]`, `stackalloc⏎[]`), `ArrayRankSpecifierSyntax` (`new int⏎[3]`; `new int[3]⏎[0]` parses as a jagged array type, not as an element access), collection expressions, list patterns, attribute lists, an indexer's `BracketedParameterListSyntax`, and a function pointer's calling-convention list.
- On `main`, three rewriters of `LineBreakPhase` join these gaps, in this order (`src/Reihitsu.Formatter/Pipeline/LineBreaks/LineBreakPhase.cs:42-52`):
  - `ListElementInteriorJoinRewriter` (43) joins an element access directly in a list element, before `LineBreakListRewriter` (44). It excludes postfix operators (`ListElementInteriorJoinRewriter.cs:234`), so `s⏎!` there is not joined.
  - `ChainLineBreakRewriter` (51) joins the parts in a chain with links (`NormalizeChain`) and `?[` everywhere (`JoinConditionalElementAccesses`), after the list layout. Its decision therefore does not count for the list.
  - `TernaryLineBreakRewriter` (52) runs after both.

  This is the cause of the part-dependent list counting that the change doc's change 3 describes. See *Architecture findings*.

## Breaking inputs

Failure classes that reach the change, with the members the tests must cover:

- **FC-004 / FC-019 / FC-020 / FC-021 (trivia in the gap).** The gap axis is: whitespace only, a blank line, a `//` comment behind the element, a `/* */` comment behind the element, a `/* */` comment in front of the part on its line, `#if`/`#else`/`#endif` with disabled text, and `#pragma`. Each applies to each gap kind: in front of `[`, in front of `?`, between `?` and `[`, and in front of `!`. Every member with a comment, a directive, or disabled text stays unjoined (FMT-BREAK-001). On `main` today (fixture `D.cs`), `var k1 = s // keep⏎!;` gives `!` at the statement column, `arr /* b */⏎[0]` gives `[0]` at the statement column, and `[0]` under `#if DEBUG` keeps its input column. A comment on the part's own line behind it must stay there after the join: `var a = arr⏎[0] // c⏎+ 1;` is `var a = arr⏎[0] // c⏎        + 1;` on `main` (fixture `F.cs`), and is expected as `var a = arr[0] // c⏎...+ 1;`.
- **FC-002 / FC-003 (the qualifier "not followed by `.` or `?.`").** Test both sides:
  - attached `!`: `s⏎!;`, `s⏎! + ""`, `(s⏎!).Length`, `s⏎![0]`, `s⏎!?[0]`, `f⏎!(1)`;
  - link `!.` / `!?.`: `s⏎!.Length` and `s⏎!?.Length`, which are already `s!.Length` and `s!?.Length` on `main` (fixture `F.cs`, `D.cs`) and are not this requirement's.

  Also test the double `!`: `s⏎!⏎!` becomes `s!!`, which must read back as two tokens (see *Premise*).
- **FC-027 (only the single-line or one-level form).** Members to cover:
  - multi-line parts: `a⏎[⏎0⏎]`, which `main` collapses to `a⏎[0]`;
  - nested parts: `d⏎["k"]⏎[0]` and `n⏎?⏎[0]`;
  - multi-line receivers: a raw string, an initializer, a switch expression in parentheses (hand-joined results above are stable);
  - positions: an expression-bodied member (`int P => arr⏎[0];`), a switch-expression arm, an object-initializer value (`["a"] =⏎arr⏎[0]` and `{ [0] = a⏎[0] }`), a lambda expression body, and a pointer element access (`p⏎[0]`);
  - index and range arguments: `a⏎[^1]` and `a⏎[1..]`.
- **Counting (FMT-BREAK-039 precedent, change doc changes 2–4).** The axes are part kind × position: directly in a list element, an interpolation hole in a list element, a query clause after the first, an array or `stackalloc` initializer in a list element, a conditional's branch and condition, a collection expression, and a collection expression nested in a list element. For the last one, `main` gives `Baz([Bar⏎(1)], 2)` → a collection with one element per line and the list one argument per line. Today `Baz([arr⏎[0]], 2)` gives the same shape with `arr` and `[0]` still split (fixture `F.cs`), so the FMT-BREAK-039 reading makes it `Baz([⏎        arr[0]⏎    ],⏎    2);`. The test should lock that.
- **Not an attached part, must stay as today (FC-016 sibling shapes).** The dictionary initializer `{⏎["a"] = 1,⏎["b"] = 2⏎}` must not join `[` onto `{` or `,`. The same holds for `new⏎[] { 1, 2 }`, `stackalloc⏎[] { 1 }`, `new int⏎[3]`, `[A]⏎[B]` attribute lists, and a conditional with a collection-expression branch (`c ? [1] : a⏎[0]` joins only the `[0]`).
- **FC-010 / FC-017.** Run every new case under LF and CRLF, and run two passes for convergence.
- **FC-028 / FC-041 (node-level).** The code-fix path via RH5302 (change doc change 5) covers `FormatNodeInDocumentAsync`. A part whose first token lies outside the formatted node is outside the phase's input and stays unjoined. No caller-specific question arises.
- **FC-013.** No analyzer rule reports `arr[0]` or `s!` after the join. RH5307 reports multi-line indexer arguments, not the gap in front of `[`. RH6007 and RH6008 are about the spaces inside brackets. No spacing rule covers `!`. There is no conflict.
- **FC-038.** The join moves no comment, because the gap is guarded. No declared FMT-UNSUP shape is produced. No member found.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter `LineBreakPhase` | must change (the join and its counting) |
| Formatter `IndentationPhase` | must stay (no column changes; the comment-held column is the output question below) |
| Other formatter phases | not involved |
| Code fixes | must stay in code; their output changes through the formatter (change 5) |
| Fix All | not involved |
| Analyzers | not involved (P11; no rule reports the shape) |
| `Reihitsu.Core` | not involved |
| Rule pages | not involved |
| CLI | not involved beyond the formatter's output |
| Tests | must change: new FMT-BREAK-040 tests; `ChainLinkLayoutTests.ElementAccessWithoutMemberAccessKeepsItsBreak` changes its expected value; the in-chain tests tagged FMT-BREAK-020 that cover attached parts are retagged (spec author's note) |

## Refuse option

- **Refuse: keep `main`'s output outside chains.** It costs 0 production lines. The class is rare, as stated under *Premise*. It keeps the part-dependent layout (chain vs no chain, `[` vs `!` vs `?[` inside a list) and keeps the poor column. The requested change costs one gap owner in the line-break phase. It is not a special case for a rare input: it replaces two partial owners, so it is no growth that *Special cases do not grow the source* forbids. Refusing is not recommended, given the maintainer's FMT-BREAK-039 decision for the sibling part.
- **Declare a shape unsupported.** The only candidate is a comment, directive, or disabled text between an element and its attached part (from the last token of the element through the first token of the part, excepting nothing). It would refuse code like `var a = arr // c⏎[0];`, which is rare. It would delete no special case: the guard that refuses these joins is FMT-BREAK-001's, which every join of the phase shares and keeps. Under `AGENTS.md` ("A shape that deletes nothing is not declared") this is not an option, and I name it only to record that.
- **Simpler option.** None found that keeps one owner. Joining only outside lists, while keeping the list-time differences, is the alternative the change doc already rejects.

## Output questions

1. **Formatter · element access or `!` kept below its element by a comment.** The spec author raised this, and I carry it unchanged with one added fact. Input `var a = arr // first⏎    [0];`. On `main`, `[0]` is at the statement's column outside a chain and at the chain anchor's column inside one. Option A keeps today's column; option B (the issue's goal) uses the column of the element's first token.

   Added fact: FMT-INDENT-013 already gives such a comment-held line inside a list element the column of the element's first token. Its `Why:` is that the line "would fall back to the statement's column and look like a new statement". That argues for B. Against B: it is an indentation change of its own, it would have to cover the argument list kept by a comment (which FMT-BREAK-039 left as today), and the input is rare.

   I agree with the spec author's recommendation A. The issue's goal for B can be dropped, and the column belongs in a follow-up of its own if the maintainer wants it.

The change doc's visible output changes 1–5 are output questions for the lead to ask. I verified the *before* blocks of changes 2 and 3 (lines 1–3) myself, and they match.

## Internal choices

- **FMT-BREAK-040's exclusion list should name the implicit element access.** The requirement defines the `[` as "the `[` of an element access's bracketed argument list". Its list of `[` that are not attached parts omits `ImplicitElementAccessSyntax` (`["a"] = 1` in a dictionary or object initializer), which also has a `BracketedArgumentListSyntax`. The codebase already treats both as element accesses: `LineBreakListRewriter.IsElementAccessArgumentList` returns true for `ElementAccessExpressionSyntax or ImplicitElementAccessSyntax`. A reading that includes it would join `{⏎["a"] = 1` and `,⏎["b"] = 2`, which is common code. I recommend that the spec author add "an implicit element access in an initializer (`["a"] = 1`)" and "the brackets of an implicit array creation (`new[]`, `stackalloc[]`)" to that list, and that a test lock the dictionary initializer as unchanged. No visible output is in question, because today's output stays.
- **FMT-BREAK-015 still says** that "postfix" operator wraps belong to FMT-BREAK-016 to FMT-BREAK-020. None of those owns a `++`/`--` wrap (see *Follow-up candidates*). The `!` is now named under FMT-BREAK-040, so the two exclusions overlap harmlessly. I recommend no change in this run.

## Issue goals

The issue is `agent-filed`.

- "Join `[`, `(`, `?[`, trailing `!` everywhere." Keep it for `[` and `!`. `(` is done (FMT-BREAK-039) and `?[` is done outside chains. Only the list counting changes (change 3).
- "A part kept on its own line by such trivia goes to the root column." Drop it, pending the maintainer: it changes visible output (output question 1).
- "Converges in one pass, same output under LF and CRLF." This is a general pipeline invariant, kept as test discipline (FC-010, FC-017). It is not a requirement of FMT-BREAK-040.
- "Owner: `JoinConditionalElementAccesses` and `NormalizeChain`." Drop it: it is a prescribed solution. On `main`, `ChainLineBreakRewriter` runs after `LineBreakListRewriter`, so a join placed there does not count for a list the way FMT-BREAK-040 (by FMT-BREAK-039's reading) requires. No visible output is in question in dropping the hint.
- "Analyzer counterpart." Out of scope: it would be a new rule (escalation 5 if taken in). It is a follow-up candidate only if no issue exists.
- Comment of 2026-10-22, "Requirements: FMT-BREAK-011, FMT-BREAK-012, FMT-BREAK-020". This is stale: the gap is FMT-BREAK-039's and FMT-BREAK-040's.

## Architecture findings

- **Two components own the attached-part join** (FC-015; `README.md`, two components own the same decision):
  - `ListElementInteriorJoinRewriter` (an element access directly in a list element, before the list layout);
  - `ChainLineBreakRewriter` (parts in a chain with links, and `?[` everywhere, after the list layout).

  The inputs that show it, from fixture `D.cs` on `main`: `Baz(arr⏎[0], 2)` → `Baz(arr[0], 2)` (one line), but `Baz(n⏎?[0], 2)` → `Baz(n?[0],⏎2)` and `Baz(list.Count.ToString()⏎!, 2)` → `Baz(list.Count.ToString()!,⏎2)`. The list layout depends on which rewriter joined. `Baz(s⏎!, 2)` is joined by neither.

  The spec diff already resolves this by making FMT-BREAK-040 the one owner, and change 3 shows the visible effect. A narrow fix that adds a third join site for the missing cases would keep the split, so this is a structural change within the line-break phase, in the run's own component. Its size is the implementer's to report against escalation 9.

## Follow-up candidates

Each is outside the run's issues, and the visible output looks wrong. Each would be an `agent-filed` follow-up carrying the question.

1. **Formatter · postfix `++`/`--` below its operand.** Fixture `E.cs`, `scripts/format.sh`.

   Input:
   ```csharp
   i
       ++;
   Baz(i
       ++, 2);
   ```
   `main`:
   ```csharp
   i
   ++;

   Baz(i
       ++,
       2);
   ```
   The candidate output is `i++;` and `Baz(i++, 2);`. FMT-BREAK-015 names FMT-BREAK-016 to FMT-BREAK-020 as owners of postfix wraps, but none of them states one. This confirms the spec author's follow-up with observed output.

2. **Formatter · brackets of an array creation below `new` or `stackalloc`.** Fixtures `A.cs`, `B.cs`, and `E.cs`.

   Input:
   ```csharp
   var e = new
       [] { 1, 2 };
   var f = new int
       [3];
   var y = stackalloc
       [] { 1 };
   ```
   `main`: each `[` lands at the statement's column (`new⏎[] { 1, 2 }`, `new int⏎[3]`, `stackalloc⏎[] { 1 }`). The candidate output is `new[] { 1, 2 }`, `new int[3]`, `stackalloc[] { 1 }`. These are not attached parts (SHARED-CHAIN-001), so FMT-BREAK-040 does not cover them. RH6022 reports a space between `new` and `[`; whether it reports a line break there was not checked.

3. **Formatter · a lambda's expression body on the line below `=>` at the statement's column.** Fixture `B.cs`: `System.Func<int> u2 = () =>⏎    a.Length;` gives `a.Length;` at the statement's column (8). The same happens for `a[0]`. I found no requirement that decides that column. It is unrelated to attached parts, and I note it only.

4. **Analyzer counterpart** (report a line break in front of an attached part). This would be a new rule. The issue says one already exists, so the issue manager checks for duplicates (already in `00-lead.md`).
