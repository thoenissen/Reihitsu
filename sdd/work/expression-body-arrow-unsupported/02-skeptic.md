Role: skeptic
Commit: b57af4d
Verdict: proceed

Inputs: thoenissen/Reihitsu#1851 (`agent-filed`) with both comments, `00-lead.md`, `01-spec-author.md`, the change doc `sdd/changes/2026-11/expression-body-arrow-leading-comment-unsupported.md`, and `git diff origin/main -- sdd AGENTS.md`. My prompt carried no intended fix, suspected cause, or hoped-for verdict. Production code on this branch equals `main`, so every "today" result below is `main`'s behavior. Fixtures lived in the scratchpad, outside the repository.

## Premise

Real, and reached by the CLI and whole-document formatting (any `.cs` file holding the shape), and by every formatter-delegating code fix whose formatted node holds a member with the shape (RH3202–RH3208 format the member itself: `ExpressionBodyToBlockCodeFixProviderBase.GetFormattingInput` returns the member and `ExpressionBodyCodeFixUtilities.ConvertBodyAsync` formats that member, so the fix asks about the node the formatter receives; FC-043 holds).

`scripts/format.sh` on the issue's input reproduces the issue's output byte for byte: `int P // c⏎=> 1;`, `get // c⏎=> _v;`, and `int Q⏎/* c */ => 2;` put the arrow line in the member's or accessor's column; `int B()⏎/* c */ => 1;` becomes `/* c */ {`; the `#if`-chosen property arrow's active branch moves to column 4.

The escalation-10 answer (declare, in a change of its own) is the maintainer's and is not re-argued here. Checked instead: that the declaration deletes what it claims, that no phase produces the shape, and what it refuses that the change doc does not list.

- **Rarity holds by the repository's definition.** RH5040's analyzer (`IsMisplaced` → `IsMemberArrow(nextToken)`) reports every comment whose next token is a member's or an accessor's `=>`, i.e. both the trailing trivia of the token in front of it and the arrow's leading trivia (RH5040-001). Local functions are excluded from RH5040 (RH5040-002) but RH3208 reports every expression-bodied local function (RH3208-001). RH3202, RH3203, RH3205–RH3208, and RH5401 are all enabled by default.
- **SHARED-DIAG-010 settles RH5401 already.** Its last sentence: a shape the formatter refuses as unsupported is not one it keeps, so a layout rule reports it and its fix is withheld. The issue's claim that RH5401's report conflicts with SHARED-DIAG-010 is wrong once the shape is declared.
- **The getter defect is not the comment's.** The issue (and FMT-UNSUP-013's *Why*, and the change doc's *Why*) say the line-break phase "refused the join of FMT-BREAK-026" for an accessor. FMT-BREAK-026 covers properties and indexers only (`PropertyLayoutLineBreakRewriter` visits `PropertyDeclaration` and `IndexerDeclaration`); no phase joins an accessor's arrow. Without any comment:

  ```csharp
  class C
  {
      private int _v;

      int V
      {
          get
              => _v;
          set
              => _v = value;
      }
  }
  ```

  `scripts/format.sh` output: both `=> _v;` and `=> _v = value;` land in column 8, the accessors' column. The declaration refuses `get // c⏎=> _v;`, but the comment-free getter keeps this layout. Follow-up candidate below; not a reason to stop.

## Mechanism and candidates

_N/A_ (behavior change). The dispatch the precheck must cover is every parent of `ArrowExpressionClauseSyntax`: `MethodDeclarationSyntax`, `ConstructorDeclarationSyntax`, `DestructorDeclarationSyntax`, `OperatorDeclarationSyntax`, `ConversionOperatorDeclarationSyntax`, `PropertyDeclarationSyntax`, `IndexerDeclarationSyntax`, `AccessorDeclarationSyntax` (get, set, init, add, remove), `LocalFunctionStatementSyntax`. The token in front of `=>` is `ArrowToken.GetPreviousToken()`: a name (also an explicit-interface name `I.P`), an indexer's `]`, a parameter list's `)` (method, operator, conversion operator, finalizer, local function), a constructor initializer's `)`, a constraint clause's last token (`)` of `new()`, `class`, `struct`, `notnull`, `unmanaged`, a type name), or an accessor keyword. The spec's list matches this set.

## Breaking inputs

Failure classes that reach the change: FC-003, FC-005, FC-019, FC-020, FC-021, FC-025, FC-027, FC-038, FC-039, FC-040, FC-041, FC-042, FC-010/FC-017 (CRLF, second pass), FC-032 (analyzer tests flip because the fixes delegate to the formatter).

### A — Not in the change doc: a conditional group around the signature or a constraint clause, ending in front of the arrow (FC-020)

The change doc's directive case (change 9) is a group that chooses the arrow, and FMT-UNSUP-013's *Why* says "a conditional group around the whole member is formatted". A group around only the signature or a constraint clause also puts a directive (or disabled text) into the arrow's leading trivia, and `main` formats these members well today. Command: `scripts/format.sh <dir>` (no symbols defined), then `scripts/apply-fix.sh RH3202 <dir>`.

`d1.cs`:

```csharp
using System.Numerics;

class C
{
#if NET8_0
    public int M(int a)
#else
    public int M(long a)
#endif
        => 1;
}
```

`main` output: `=> 1;` becomes

```csharp
#endif
    {
        return 1;
    }
}
```

with every other line unchanged. Only `#endif` (line 9, column 1) is in the gap: `#if NET8_0`, the disabled signature, and `#else` are in the leading trivia of the active `public`.

`d2.cs`:

```csharp
using System.Numerics;

class C
{
    T M<T>()
#if NET7_0_OR_GREATER
        where T : INumber<T>
#endif
        => default;
}
```

`main` output: `=> default;` becomes `    {⏎        return default;⏎    }`. Without the symbol, `#if NET7_0_OR_GREATER` (line 6, column 1), the disabled constraint, and `#endif` are all in the arrow's leading trivia.

`scripts/apply-fix.sh RH3202` on `d1.cs` and `d2.cs` today: `4 fixed` (both fixtures, LF and CRLF), each producing the same block as the formatter. After the change both are refused (locations derived from FMT-UNSUP-013's text, not observed: `(9,1)` and `(6,1)`), and RH3202's fix is withheld; RH3202 still reports both (`BlocksRewrite` blocks only a region directive in the arrow's leading trivia and directives whose group reaches the span from the arrow's `SpanStart`), so the class stays rare by definition. The property variant is already badly laid out today (`d4.cs`, `#if NET8_0⏎public int P⏎#else⏎public long P⏎#endif⏎=> 1;`: `=> 1;` moves to column 4; `apply-fix.sh RH5401`: `2 no-fix-offered`), as is `d3.cs` (`int P⏎#if DEBUG⏎{ get; set; }⏎#else⏎=> 1;⏎#endif`: `=> 1;` to column 4).

Change 9's method (`int M()⏎#if DEBUG⏎=> 1;⏎#else⏎=> 0;⏎#endif`) is reported and fixed by RH3202 today (`apply-fix.sh`: fixed, `=> 0;` → a block holding `return 0;`); after, reported with the fix withheld. The region case (`int M()⏎#region R⏎=> 1;⏎#endregion`) is `no-diagnostic` today and stays so (SHARED-BODY-001).

These are members of the shape the maintainer decided, so they are no reason to narrow it, but they are visible output changes the change doc does not list (see *Output questions*).

### B — FC-038: no phase produces the shape

Producers that could write into the gap are the transforms that create an arrow: `AccessorExpressionBodyTransform.ConvertToExpressionBody` (sets the keyword's trailing trivia to empty and creates the arrow without trivia) and `GetOnlyMemberExpressionBodyTransform` (calls `ConvertToExpressionBody` for a block getter, creates the member arrow without trivia, and strips the signature's trailing trivia after its kept seams pass). No code fix creates an `ArrowExpressionClause` (`rg` over `Reihitsu.Analyzer.CodeFixes`: only `ExpressionBodyCodeFixUtilities`, which removes one). Probe through `scripts/format.sh` (`p.cs`; one member per seam, no input holds the shape):

```csharp
class C
{
    private int _v;

    int V1
    {
        get // c1
        {
            return _v;
        }
    }

    int V2 // c2
    {
        get
        {
            return 1;
        }
    }

    int V3
    {
        // c3
        get
        {
            return 1;
        }
    }

    int this[int i] /* c4 */ { get { return i; } }

    int V5 { get { return 1; } } // c5

    int V6
    /* c6 */ { get => 1; }

    int V7
    {
        get
        // c7
        {
            return _v;
        }
        set /* c8 */ { _v = value; }
    }

    int V9
    {
        get
        {
            return _v;
        } // c9
    }

    int V10
    {
        get
        {
            return _v; // c10
        }
    }

    int V11
    {
        get => _v; // c11
    }

    int V12
    {
        get
        {
            return _v;
        }
        /* c12 */
    }
}
```

Output: `V1` and `V7` keep their block getters; `V2` → `int V2 // c2⏎{⏎get => 1;⏎}`; `V3` → `// c3⏎get => 1;` inside the list; the indexer keeps its list with `get => i;`; `V5` → `int V5 => 1; // c5`; `V9`, `V10`, `V11` → `int Vn => _v; // cn`; `V12` keeps its list. No output holds a comment, directive, or disabled text between an expression body's `=>` and the token in front of it. These are the test implementer's FC-038 inputs; after the deletions in `GetOnlyMemberExpressionBodyTransform.HasLayoutOnlySeams` they must stay so on two passes, LF and CRLF.

### C — the precheck's own matrix

Axes, each cell to be tested on both sides (FC-003) and under LF and CRLF:

- **Owner** (FC-025): method, constructor (with and without `: base(...)`/`: this(...)`), finalizer, operator, conversion operator, property (also `int I.P`), indexer, accessor get/set/init/add/remove, local function (also with a constraint clause), method with a constraint clause.
- **Place**: trailing trivia of the token in front of `=>` (same line), leading trivia of `=>` on a line of its own, leading trivia of `=>` on the arrow's line (`/* c */ =>`), and one line (`int A /* c */ => 1;`).
- **Trivia kind** (FC-005, FC-020, FC-021, FC-040): `//`, `/* */`, `///` and `/** */` in `DocumentationMode.None` and `Parse`/`Diagnose` (in leading position the parser makes them structured trivia; `SyntaxTriviaUtilities.IsCommentTrivia` covers both kinds, but the test has to show it), `#if`/`#else`/`#endif` with the partner in front of the gap (`d1`), inside it, and behind it (change 9), `#region`/`#endregion`, `#pragma warning`, `#nullable`, disabled text, and a fully inactive `#if false … #endif` in the gap.
- **Other side** (not refused): a comment in the leading trivia of the member's first token, its documentation comment included; a comment inside the parameter list (`int F(/* c */ int a) => a;`); a comment behind the arrow (FMT-UNSUP-012's reason, not this one); `() /* c */ => 1` and `1 /* c */ => "one"`; an expression body inside disabled text.
- **Nesting** (FC-039, FC-027): a local function in a lambda body, in an accessor body, in another local function; an accessor of a property in a nested type; a local function in a lambda in a field initializer with one declarator (this reason, also for a directive) and with several declarators (a directive gives FMT-UNSUP-010's reason; a comment gives this one, because FMT-UNSUP-010 covers directives and disabled text only).
- **Two shapes in one member** (FMT-UNSUP-001): `int A /* a */ => /* b */ 1;` reports `/* a */` with this requirement's reason.
- **Node-level** (FC-041, FMT-UNSUP-003): the arrow clause alone as input, with a comment in its own leading trivia, is not refused; the check must not read the trailing trivia of a previous token that lies outside the input. The whole member as a detached node, as a node in its document, and as a document root give the same answer (FC-028).
- **Code fixes** (FC-043, FC-032): RH3202 on `d1`, `d2`, and change 9's method; RH3203 with an initializer; RH3208 on a local function; RH5401 on `int P // c⏎=> 1;` (reported, no fix, before and after); RH5110 and RH7101 as in change doc 11.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `PrecheckPhase` (FMT-UNSUP-013 check and reason) | Must change |
| `PropertyLayoutLineBreakRewriter.CollapseExpressionBody` refusal | Must change (delete); FMT-UNSUP-003 holds: the rewriter visits whole properties and indexers only |
| `ExpressionBodyToBlockConverter` own-line comment move | Must change (delete); its only callers are the six `ExpressionBodied*Transform`s, which see whole members |
| `GetOnlyMemberExpressionBodyTransform.HasLayoutOnlySeams` (keyword trailing, getter arrow leading) | Must change (delete those two); the other seams must stay (*B*) |
| `AccessorExpressionBodyTransform` seam checks | Must stay: they protect trivia the conversion drops (`WithTrailingTrivia(default)` on the keyword) and FMT-UNSUP-005 |
| `ExpressionBodyRewriteUtilities.BlocksRewrite` (Core) | Must stay (analyzers of RH3202–RH3208) |
| Analyzers RH5040, RH5401, RH3202–RH3208 | Must stay |
| Code fixes RH3202–RH3208, RH5401, RH5110, RH7101, other formatter-delegating fixes | Must stay in code; their results change through the precheck (withheld) |
| Fix All | Not involved |
| CLI | Not involved in code; it reports the new reason through the existing unsupported path |
| Rule pages, `general-notes.md` | Not involved |
| `AGENTS.md` precheck row | Changed by the spec author |
| Tests | Must change: the existing tests the spec author lists hold the shape and now expect refusal or no fix |

## Refuse option

The requested change is itself the third refusal (declaring the shape), already chosen by the maintainer. Shape: from the trailing trivia of the token in front of an expression body's `=>` through the `=>`'s leading trivia; comments, directives, disabled text; no exception inside that span; lambdas and switch-expression arms excluded. It deletes the property/indexer join refusal, the converter's own-line comment move (`ArrowLineHoldsComment`, `TrySplitOwnLineComments`, and the inserted line break), and two seams of the get-only lift, and adds one check of two trivia lists to the precheck.

The local refusals would cost more: leaving a property's arrow behind a comment "as written" needs the indentation phase to skip that line (a column decided by another phase), and keeping a method's expression body for a comment in front of the arrow needs a new guard in the conversion while the comment move stays. So the declaration is the cheapest option.

Common code it refuses: none by the policy's definition. Every comment cell is reported by RH5040 or RH3208, and the directive cells are reported by RH3202–RH3208 or RH5401 for methods and properties (*A*). The most realistic refused class is *A*'s multi-targeting signature or constraint group in front of a shared method expression body, which `main` formats correctly today. Indexers, operators, and accessors with a directive in the gap may be reported by no rule; they are rare in practice.

Simpler options: none. Comments only (no directives) would keep the join refusal and the lift seams, which test directives alike, so it would delete nothing that matters.

## Output questions

The change doc's *Visible output changes* 1–11 are asked as listed. In addition:

**1 · Formatter and RH3202 · Expression-style methods should not be used — conditional group around the signature, in front of a shared arrow**

Input:
```csharp
class C
{
#if NET8_0
    public int M(int a)
#else
    public int M(long a)
#endif
        => 1;
}
```

Before (`main`), formatter and RH3202's fix alike:
```csharp
class C
{
#if NET8_0
    public int M(int a)
#else
    public int M(long a)
#endif
    {
        return 1;
    }
}
```

After, A (recommended): input unchanged. The CLI reports `error: <file>(7,1): unsupported syntax: a comment or directive between the arrow of an expression body and the code in front of it. The file was left unchanged.` RH3202's fix, and every other fix that formats this method, is no longer offered on it; RH3202 still reports it. Declared in this work.

After, B: as before (`main`): a directive or disabled text in front of the arrow is not part of the shape.

Why A: the maintainer's answer covers directives in this place, and B would keep the property join refusal and the get-only lift's seam checks, which test directives and comments alike.
Special cases the shape deletes: the line-break phase's refusal to put a property's arrow on the property's line, the conversion's move of comments in front of the arrow into the new block, and the get-only property lift's checks of the trivia behind `get` and in front of the getter's arrow.

(Location `(7,1)` is `#endif` in this six-line input, derived from FMT-UNSUP-013, not observed. The constraint-clause variant `d2.cs` above behaves the same way.) I recommend the spec author add this case to the change doc's *Visible output changes* and correct FMT-UNSUP-013's "a conditional group around the whole member is formatted" to say that only a group around the whole member is, so the question is asked with the change doc rather than on its own.

On the spec author's three questions: question 2 (reason wording) is a real output question — I recommend its option A, which pairs with FMT-UNSUP-012's "between the arrow of an expression body and its expression". Question 1 (RH5401) changes nothing visible under its option A, which SHARED-DIAG-010's last sentence already demands; its option B would contradict that sentence, so I see nothing to ask. Question 3 (one line) is inside the span the maintainer answered (two trivia lists, no line-break condition) and is listed as change doc changes 2, 4, and 6; I see no separate question.

## Internal choices

1. **FMT-UNSUP-013 *Why* and the change doc's *Why* overstate the accessor case.** They say the line-break phase refused the FMT-BREAK-026 join for "a property, an indexer, and an accessor". FMT-BREAK-026 joins properties and indexers only; an accessor's arrow on its own line lands in the accessor's column with or without a comment (*Premise*). The accessor's cost is the get-only lift's seams. Recommend rewording both to name only properties and indexers for the join refusal. No visible output changes.
2. **SHARED-BODY-001 leaves the non-region directive in front of `=>` unspecified.** Observed today: RH3202 reports and fixes `d1`, `d2`, and change 9's method. `BlocksRewrite` blocks a region directive from the arrow's full-span start, but a conditional directive only when its group reaches the span that starts at the arrow's `SpanStart`. Recommend that SHARED-BODY-001 state the observed cases — a conditional group that closes in front of `=>`, or one that chooses `=>` and closes behind the `;` — as reported with the fix withheld, with one example, so the tests can tag them. The alternative, leaving it unspecified as the spec author chose, leaves those analyzer tests untagged. Reporting is unchanged either way.
3. The spec author's other internal choices (a requirement of its own, both tokens needed for a node-level input, the region case moved to SHARED-BODY-001, FMT-UNSUP-010 keeps its reason) — I agree with all of them.

## Issue goals

- "RH5401 no longer reports `int B // note⏎=> 1;`" (issue's recommended reading): drop. SHARED-DIAG-010's last sentence asks for the report. Dropping keeps today's output, so nothing visible is in question.
- "Declaring removes three layout defects at once": overstated. The getter defect is not removed for a getter without a comment (*Premise*). No visible effect on this run; follow-up below.
- "the diagnostics stay (RH5040 already reports every comment at an arrow)": RH5040 does not cover local functions (RH5040-002); RH3208 does. A wording point only.
- Reason text "between an expression body's arrow and the code in front of it": the issue itself asks to fit it to FMT-UNSUP-012's wording; the spec author did. Visible, so it is output question 2.
- Every other goal (declare, the owners, the deletions, keeping guards that serve FMT-UNSUP-003 inputs) stands.

## Architecture findings

_None._ The deleted refusals are guards for the shape that the precheck now owns (FC-042), which is exactly what the declaration removes. No two components keep owning the decision after the change.

## Follow-up candidates

1 · Formatter · an accessor's arrow on a line of its own (outside #1851: no comment involved; no rule reports it, so this is not P11's rule gap)

Input:
```csharp
class C
{
    private int _v;

    int V
    {
        get
            => _v;
        set
            => _v = value;
    }
}
```

Before (`main`):
```csharp
class C
{
    private int _v;

    int V
    {
        get
        => _v;
        set
        => _v = value;
    }
}
```

After, A (recommended): `get => _v;` and `set => _v = value;`, joined onto the keyword's line as FMT-BREAK-026 joins a property's arrow.

After, B: the arrow stays on its own line, indented one level deeper than the accessor (`get⏎            => _v;`).

Why A: it matches the property's one-line rule and the form the accessor conversion creates (`get => _v;`).
