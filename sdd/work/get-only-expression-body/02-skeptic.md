Role: skeptic
Commit: 89106f5
Verdict: #849: proceed (output question); #850: proceed (output question)

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The facts below come from the issues, `00-lead.md`, `01-spec-author.md`, the spec diff of 89106f5, and the code. All fixtures were run in the scratchpad and are quoted here in full. Neither rule exists yet, so every claim about the analyzer or the fix is a requirement of the spec. None of it is an observation.

## thoenissen/Reihitsu#849

### Premise

The premise is real. Whole-document formatting (the CLI, `FormatDocumentAsync`) reaches the case. `StructuralTransformPhase.CreateRewriters` adds `GetOnlyMemberExpressionBodyTransform` from C# 6 on, behind `ConfigurableStructuralTransforms.AccessorExpressionBody`. Only the node-level entry points switch that off (`ReihitsuFormatter.CodeFixDisabledStructuralTransforms`), so users cannot turn it off. `AccessorExpressionBodyTransform` runs before it from C# 7 on. No analyzer reports a get-only accessor list: `rg` over `src/Reihitsu.Analyzer/Rules` finds only RH5401, which reports expression bodies, and the auto-property rules RH5408 and RH5047. The spec author's runs of the positive and negative examples hold, and I confirmed the following with `scripts/format.sh <dir>` (a second pass reports `Formatted 0 of N`, so each result is idempotent). Each result was compiled with `dotnet build` of a scratch `net11.0` library at `LangVersion` 14: `Build succeeded`, 0 errors.

Fixture `partial.cs` (FC-024):

```csharp
partial class C
{
    public partial int P { get; }

    public partial int this[int i] { get; }
}

partial class C
{
    private int _p;

    public partial int P { get => _p; }

    public partial int this[int i] { get { return i; } }
}
```

The formatter writes `public partial int P => _p;` and `public partial int this[int i] => i;`, and both compile.

Fixture `iface.cs`:

```csharp
interface I
{
    static virtual int S { get { return 1; } }

    int this[int i] { get => i; }

    abstract int A { get; }
}
```

The formatter writes `static virtual int S => 1;` and `int this[int i] => i;`. It keeps `A`. The result compiles.

The remaining fixtures, each as input → formatter output (every one compiles):

- `fieldkw.cs`: `public int P { get => field; }` → `public int P => field;`.
- `override.cs`: `public override int P { get { return 1; } }` with a base of `{ get; set; }` → `public override int P => 1;`.
- `structs.cs`: in a struct, `public int P { get { return _v; } }` → `public int P => _v;`, and `public readonly int Q { get { return _v; } }` → `public readonly int Q => _v;`.
- `ext.cs`: in a C# 14 `extension(string s)` block, `public int Len { get { return s.Length; } }` → `public int Len => s.Length;`, and `public static int Zero { get => 0; }` → `public static int Zero => 0;`.
- `ext.cs`: `get { throw _flag ? new System.InvalidOperationException() : new System.ArgumentException(); }` → `public int P => throw (_flag ? new System.InvalidOperationException() : new System.ArgumentException());` (FMT-STRUCT-011).
- `ext.cs`: `get { return _flag ? 1 : 2; // a }` (block on its own lines) → `public int Q => _flag ? 1 : 2; // a`.
- `regionin.cs`: `#region R` / `#endregion` inside the accessor list. The list is kept, and only the region text and blank lines change.

Language versions, run as `scripts/format.sh <dir> --lang-version 6` and `--lang-version 5` on this input:

```csharp
using System;

class C
{
    private int _value;

    public int B
    {
        get
        {
            return _value;
        }
    }

    public int D { get { throw new InvalidOperationException(); } }

    public int this[int i] { get { return i; } }
}
```

- C# 6 gives `public int B => _value;` and `public int this[int i] => i;`. `D` keeps its accessor list, which is laid out over lines.
- C# 5 keeps every accessor list.

This confirms the C# 6 and C# 7 gates that the spec author left unrun, on the formatter side.

Compile check of the spec's own examples at C# 14 (`Build succeeded`): SHARED-GETONLY-002 (including `public int F { get => field; } = 5;`), RH3209-001 (an interface member with a body, a static member, and an explicit implementation), and RH3210-001.

Production code has no get-only accessor list that RH3209 or RH3210 would report: a multi-line `rg` over every non-test project hits only `<c>` text in a doc comment. Adding the rules to `Release.ruleset` does not break the repository's own build.

### Mechanism and candidates

_N/A_ for a bug. For a new rule, the candidate set comes from the dispatch:

- **Member kinds.** `BasePropertyDeclarationSyntax` has three subtypes. `GetOnlyMemberExpressionBodyTransform` overrides `VisitPropertyDeclaration` and `VisitIndexerDeclaration`. `EventDeclarationSyntax` is not visited and has only `add`/`remove`, so it is not involved.
- **Accessor kinds.** `get`, `set`, `init`, `add`, `remove`. Only a lone `get` qualifies (`TryGetExpressionBodiedGetter`).
- **Getter statement kinds** (`AccessorExpressionBodyTransform.TryGetConvertibleExpression`):
  - converted: `ReturnStatementSyntax` with a value (`return ref` included, `return throw` excluded), and `ThrowStatementSyntax` with an operand;
  - not converted: everything else (a rethrow, `yield`, declarations, compound statements).
- **Containers.** class, struct, record, record struct, interface (members with a body), C# 14 `extension` blocks, nested types.

### Breaking inputs

The central class is that **the analyzer's predicate and the fix's rewrite drift from the formatter's** (FC-015, FC-016, FC-044). The rule's contract is "exactly what FMT-STRUCT-014 converts and FMT-STRUCT-015 does not keep", so every cell below has to give the formatter's answer. Tests must cover the axes this class spans:

- **Member:** property, indexer (FC-025).
- **Getter:** `get => e;`, `get { return e; }`, `get { return ref e; }`, `get { throw e; }`, `get { throw <conditional | assignment | lambda | query>; }` (parenthesized, FMT-STRUCT-011), `get { throw a ?? b; }` (bare), `get;`, `get { throw; }`, `get { yield return e; }`, two statements, a statement with attributes.
- **List:** the getter alone; with `set`; with `init`; a getter with `[A]`; a getter with `readonly`; a property with an initializer.
- **Trivia seam** (FC-004, FC-019, FC-021). Each seam needs a `//` comment, a `/* */` comment, and a directive (FC-005, FC-020: an `#if` group, `#pragma`, `#region`, and disabled text) on both sides of the boundary (FC-003). The seams are:
  - the trailing trivia of the token in front of `{`;
  - both sides of `{`;
  - the leading trivia of `get`;
  - for a block getter, the FMT-STRUCT-012 seams (behind `get`, around the block's `{`, in front of the statement, behind `return`, in front of the expression, in front of `;`, in front of the block's `}`, and a `//` comment behind the expression in front of `;`);
  - the leading trivia of the getter's `;`;
  - the leading trivia of the list's `}`;
  - a comment behind the getter's `;` only, behind the list's `}` only, and behind both;
  - for an expression-bodied getter, the two gaps around `=>` (refused, SHARED-GETONLY-006).
- **Language version:** C# 5, C# 6 (a throw is not reported), C# 7, C# 14 `field` (FC-006).
- **Declaration shapes:** `partial` (both parts), explicit interface implementation, `static` and `static virtual` interface members, `override`, a `readonly` member of a struct, an `extension` block (FC-024).
- **Layout:** the accessor list on one line and over lines; an expression over several lines, such as an argument list wrapped onto a second line (FC-027).

What the test implementer must cover beyond the spec examples:

- A balanced `#if` group inside the getter's expression is converted and kept (FMT-STRUCT-013). The formatter's output, as observed (fixture `ifdir.cs`):

  ```csharp
  public int P
  {
      get
      {
          return M(
  #if DEBUG
              1
  #else
              2
  #endif
              );
      }
  }
  ```

  becomes

  ```csharp
  public int P => M(
  #if DEBUG
                  1
  #else
                        2
  #endif
                        );
  ```

  The columns of the active and the disabled branch differ. That is formatter indentation and out of scope, but a fix test must assert the formatter's result here, not a hand-written one (FC-035).
- Directive placements, observed with `scripts/format.sh`:
  - `get` followed by `#if DEBUG` / `=> 1;` / `#else` / `=> 2;` / `#endif` is refused (`Skipped 1 file(s) with unsupported syntax`), so it is reported with no fix offered.
  - `get =>` followed by `#if DEBUG` / `1;` / `#else` / `2;` / `#endif` is refused the same way.
  - An `#if` / `#else` group around two whole `get => …;` lines is kept, so it is not reported.
- Fix All (FC-008): several properties and indexers in one document, members in nested types, and RH3209 next to RH3210 in one document.
- The fix converges in one pass and raises no other RH diagnostic in its span (FC-007). In particular RH5401 must not report the result: the arrow and the first expression token must stay on the signature's line.
- LF and CRLF (FC-010, FC-033), including a `//` comment behind the getter's `;` that moves behind the new `;`.
- The fix keeps the signature as written (FC-030). The SHARED-GETONLY-005 example with `public  int  A` covers this.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzer (`Reihitsu.Analyzer`, Clarity) | must change: new `RH3209…Analyzer` |
| Code fix (`Reihitsu.Analyzer.CodeFixes`) | must change: new `RH3209…CodeFixProvider`, Fix All |
| `Reihitsu.Core` | must change if the shared owner under *Architecture findings* is chosen (recommended) |
| Formatter `GetOnlyMemberExpressionBodyTransform` / `AccessorExpressionBodyTransform` | output must stay. The code may change only to delegate to the shared owner, and the existing formatter tests must stay green unchanged |
| Precheck / `FindUnsupportedSyntax` | not involved (used as is) |
| Node-level entry points (FMT-PIPE-011, FMT-STRUCT-016) | must stay: they keep accessor shapes |
| Rule page `documentation/rules/RH3209.md` | new (written). `general-notes.md` is not involved |
| `AnalyzerResources.resx`, `CodeFixResources.resx` | must change |
| `Release.ruleset`, `Reihitsu.sln` (rule-page solution item) | must change: both list every rule |
| CLI | not involved |
| `README.md` | not involved: it has no rule table |

### Refuse option

- **Withhold the fix for the reported class.** This is already the spec for refused shapes (SHARED-GETONLY-006): rare, and it costs no production lines.
- **Ship the analyzer without a code fix.** This saves the two providers and the splicing (about 100 to 150 lines), but the analyzer's predicate would still need the formatter's conditions (*Architecture findings*). With a shared converter, the fix can be complete (`AGENTS.md`, *Code-fix coverage*). I do not recommend it.
- **Declare a shape unsupported (escalation 10, named as the role requires).** The shape would be a comment, directive, or disabled text in a seam of a get-only member's accessor list. It would run from the trailing trivia of the token in front of `{` through the leading trivia of `}`, and except the getter's expression and the trailing trivia of the getter's `;` and of `}`.
  - It would delete the six-seam `HasLayoutOnlySeams` in `GetOnlyMemberExpressionBodyTransform` and the analyzer's copy of it. The FMT-STRUCT-012 seams and `TryBuildSemicolonTrailingTrivia` stay, because the accessor-level transform needs them.
  - The precheck cannot decide "the getter is convertible" with a cheap check. A cheap shape would therefore refuse every single-`get` accessor list with such a comment, multi-statement getters included. That refuses common code, for example:

    ```csharp
    public int Count
    {
        // number of live items
        get { … several statements … }
    }
    ```
  - Recommendation: do not declare.
- **Simpler options.** Answer A of the spec author's output question (do not report what the formatter keeps for a comment) is already the narrower option. There is no other.

### Output questions

1. The spec author's question on comments the formatter keeps the accessor list for (`01-spec-author.md`). I agree it belongs to the maintainer, and I agree with **A (not reported)**: RH3209 then reports exactly what formatting clears.
   - The issue states both readings: its *Proposed Solution* asks to "match the formatter's collapse exactly, including its trivia and directive guards", which is A, and its code-fix section says "reported without a fix", which is B.
   - Against B: FMT-STRUCT-015 keeps those lists on purpose, so under B the diagnostic would stay after every format run.

2. **RH3209 · Get-only properties should use an expression body — directive between `get` and `=>`.** The change doc's visible change 5 shows only a comment. SHARED-GETONLY-006 also reports directives and disabled text there. The sibling rule family decides the analogous directive case the other way: SHARED-BODY-001 does not report a member with a directive between `=>` and its expression. The maintainer should see the directive input.

   Input:
   ```csharp
   class C
   {
       public int A
       {
           get
   #if DEBUG
               => 1;
   #else
               => 2;
   #endif
       }
   }
   ```

   Before (`main`): not reported. The formatter refuses the file (`Skipped 1 file(s) with unsupported syntax`).

   After, A (recommended, the spec as drafted): RH3209 is reported on the accessor list, and no code fix is offered.

   After, B: not reported.

   Why A: the shape is refused, not kept (SHARED-DIAG-010), and the user can write the change by hand by putting the whole member into each branch. SHARED-BODY-001's reason for B, that no fix can convert the member, is a fix reason, and a diagnostic without a fix is a supported end state.

   The same answer applies to RH3210 and to a directive between `=>` and the expression.

Completeness point for the change doc, not a separate question: the SHARED-GETONLY-005 example shows that the fix keeps `public  int  A` as written, while the RH5401 fix formats the whole property (`ReihitsuFormatter.FormatNode(propertyDeclaration)`). The change doc's *Visible output changes* should show that example, so the maintainer approves it with the rest.

### Internal choices

- **SHARED-GETONLY-006's "whose declaration holds a refused shape".** This asks `FindUnsupportedSyntax` about a node the formatter does not receive. That is FC-043-shaped against SHARED-FIX-010 ("about exactly the node the formatter would receive", "MUST NOT decide the question by a check of its own"). The second query is still needed:
  - The conversion deletes `get` and builds a bare `=>`, and `TryRehostExpression` drops the expression's leading trivia. So `get /* c */ => x` and `get => /* c */ x` lose the comment in the built node, which then holds no refused shape. SHARED-FIX-010 alone would offer a fix that drops a comment (SHARED-FIX-003, FC-004).
  - Asking the precheck about the original node reuses its one predicate, instead of copying the arrow-seam check (FC-042).
  - Recommendation: keep the query, but narrow it to **the getter** (the `AccessorDeclarationSyntax`) instead of the whole declaration, and state its reason as SHARED-FIX-003 (the conversion deletes those gaps). That keeps it from being read as an exception to SHARED-FIX-010. The two scopes give the same visible result: a shape elsewhere in the declaration, such as in an attribute, is also in the built node.
- **Who writes the gap in front of `=>`.** The transform builds `=>` with no trivia, and the spacing phase spaces it (FMT-STRUCT-026, FMT-SPACE-017). A fix that writes `" "` in front of `=>` on its own predicts the spacing phase (question 10). The safe shape follows the RH3202 precedent (`ExpressionBodyCodeFixUtilities.ConvertBodyAsync`): build the converted member with the shared converter, format it at node level, then splice the formatter's arrow clause, semicolon, and the trivia in front of `=>` onto the signature as written. Recommendation: that shape.
- **Code owners of SHARED-GETONLY.** These follow from the architecture decision below. The rule IDs and type names are as the spec author lists them; no objection.

### Issue goals

These are the goals I would drop or change. None of them changes visible output beyond what the change doc already lists.

- "Inputs where moving the getter would drop or relocate comments … are reported without a fix, **and the rule doc records that**." Drop the rule-doc part: rule pages list no withheld fixes (`AGENTS.md`, *Key conventions*), and `general-notes.md` covers it. The "reported without a fix" part is output question 1.
- "README row." Drop: `README.md` has no rule table. `Release.ruleset` and `Reihitsu.sln` are where every rule is listed.
- "Coordinate with the IDs #628 allocates." Stale: thoenissen/Reihitsu#628 is closed (RH3205 to RH3208), and RH3209 is free.
- "Not reported: abstract/interface declarations without a body." It holds as `get;` (SHARED-GETONLY-002). No change.
- "The C# 14 `field` keyword needs no exclusion." It holds: `public int P => field;` compiles at C# 14, and the initializer case `{ get => field; } = 5` is excluded by SHARED-GETONLY-002.

### Architecture findings

**The decision "does the formatter convert this get-only member, and to what" is needed by three components that cannot reach its owner today.**

- The owner is the formatter: `GetOnlyMemberExpressionBodyTransform.TryCreateExpressionBody` and the `AccessorExpressionBodyTransform` members it calls (`ConvertToExpressionBody`, `TryGetConvertibleExpression`, `RequiresThrowExpressionParentheses`, `HasLayoutOnlySeams`, `TryRehostExpression`, `TryBuildSemicolonTrailingTrivia`, `ContainsBlockingDirective`), which use `TriviaListUtilities.RemoveTrailingWhitespace`, a type in the formatter.
- The analyzer references only `Reihitsu.Core`.
- The code fix may not call a member of a transform (FMT-PIPE-009), and the node-level entry points do not convert get-only members (FMT-PIPE-011).

Without a shared owner, the analyzer and the fix each restate FMT-STRUCT-009 to FMT-STRUCT-015, which is one rule in three places (FC-015, FC-044). The cells under *Breaking inputs* are where such copies drift. Precedent for one owner in `Reihitsu.Core` exists: `ExpressionBodyRewriteUtilities` for SHARED-BODY (the analyzer, the fix, and the formatter, "one owner for the conversion keeps the fixes and the formatter from drifting apart", SHARED-BODY-003) and `EmptyTypeDeclarationSemicolonAnalysisUtilities` for SHARED-EMPTY.

Recommendation: move the predicate and the conversion into one `Reihitsu.Core` type, which both transforms, both analyzers, and both fixes call. The analyzer uses its success, the fix and the formatter its result. This moves roughly 250 existing lines and adds a Core type. Whether that counts as the issues' own implementation or as escalation 9 is the lead's call.

### Follow-up candidates

- **Formatter indentation of a balanced `#if` group inside a wrapped argument list in an expression body.** The active branch `1` stays in column 16, while the disabled `2` and the `)` move to column 22 (fixture `ifdir.cs` above). Its output looks inconsistent, and nothing in this run decides it. Before filing, the issue manager should check whether a spec decides disabled-text columns.

## thoenissen/Reihitsu#850

### Premise

The premise is real, through the same path as #849 (`VisitIndexerDeclaration`). Observed: `public partial int this[int i] { get { return i; } }` → `… => i;`, `int this[int i] { get => i; }` in an interface → `int this[int i] => i;`, and under C# 6 `public int this[int i] { get { return i; } }` → `public int this[int i] => i;`. The RH3210-001 example compiles at C# 14. The issue's claim that the formatter no longer writes `{ get => e; }` for indexers agrees with the maintainer's comment on thoenissen/Reihitsu#628.

### Mechanism and candidates

As under #849.

### Breaking inputs

As under #849, with the member axis on the indexer. The indexer-specific cells:

- the explicit implementation `int I.this[int i] { get { … } }`;
- a `ref` return;
- a `params` parameter;
- an indexer in an interface;
- an indexer in an `extension` block, if Roslyn accepts it; I did not run this.

Indexers have no initializer, so that axis does not apply (`VisitIndexerDeclaration` has no initializer check).

### Surfaces

As under #849, for `RH3210…Analyzer` and `RH3210…CodeFixProvider`, with `documentation/rules/RH3210.md`.

### Refuse option

As under #849.

### Output questions

Both questions under #849 apply to RH3210 alike. There is no question of its own.

### Internal choices

As under #849.

### Issue goals

- "#628 lists indexers … that row … must be dropped there." Stale: the maintainer already dropped the indexer row on thoenissen/Reihitsu#628, and the issue is closed. Drop the goal. It has no visible output.
- The rule-doc and README goals: as under #849.

### Architecture findings

The same finding as under #849, with one owner for both rules.

### Follow-up candidates

_None._

## Whole run

- Fixtures live in the scratchpad, under `sk/` (`a/`, `v6/`, `v5/`, `d1/` to `d3/`, `ext/`, `ex/`, `build/b.csproj`). They are not part of the repository.
- The commands were `scripts/format.sh <dir>`, `scripts/format.sh <dir> --lang-version 6|5`, and `dotnet build -v q` of the scratch project at `LangVersion` 14.
