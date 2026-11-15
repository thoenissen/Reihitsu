Role: skeptic
Commit: 2272e74
Verdict: #1762: proceed (change doc needs one amendment, below); #1763: proceed (change doc needs one amendment, below)

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The issues' "Affected components" tables and cause statements were treated as hypotheses and checked against the code and against runs.

How facts were established (all at 2272e74, scratch fixtures under the session scratchpad, deleted afterwards):

- Formatter: `Reihitsu.Cli` (Debug, default language version = C# 15) over fixture directories, a second pass for idempotency.
- Code fixes and analyzers: `scripts/apply-fix.sh <RH####> <dir>` (Roslyn `LanguageVersion.Latest` = C# 15, LF and CRLF).
- Roslyn facts: a file-based probe referencing `Microsoft.CodeAnalysis.CSharp` 5.12.0 that reads the custom attributes of the union and `with`-element API, parses under C# 14 and C# 15, and compiles against the .NET 11 runtime's assemblies.

## thoenissen/Reihitsu#1762

### Premise

Real. Every "before" in the issue and in the change doc reproduces exactly (U1, U2, U4, U6/U8 constraints, empty bodies `{ }` and `{⏎}`, combined static field, `union U2⏎    (int, string)` → `union U2⏎(int, string);`). Today's union output is idempotent.

Callers that reach it:

- CLI and whole-document formatting: the CLI parses as C# 15 by default (CLI-004), so every file that declares a union reaches the phases.
- RH7101 code fix: the analyzer reports `public static int A = 1, B = 2;` in a union today, and the fix is not offered (`apply-fix.sh RH7101`: `no-fix-offered` under LF and CRLF, for a union with braces on their own lines and for a one-line union).
- RH5404 code fix on an enclosing type: `class C { union U(int, string) { public int X => 1; } }` → the fix formats the class and leaves the union on one line today (`apply-fix.sh RH5404`). After the change the same fix lays out the union too (see *Output questions*).

Facts that bound the input class:

- An instance field in a union does not compile: CS9373 "Instance fields, auto-properties or field-like events are not permitted in a 'union' declaration" (probe). So the field split reaches only `static` and `const` fields of a union. The spec examples use `static`; tests must not use instance fields where they compile.
- Roslyn 5.12 parses no `UnionDeclarationSyntax` under C# 14 (probe: 0 union nodes, 11 parse errors for a file of unions). Every `FormattingContext` is resolved from the tree's or project's parse options (`ReihitsuFormatter.cs:74/110/155/224/392`), and a detached node resolves to C# 15. So the C# 15 gate of FMT-STRUCT-017 for unions can never be false for any caller; see *Internal choices*.
- `partial union` and `readonly union` parse and compile (probe; only the usual "one parameter list per partial type" error). `union U { }` without case types parses and fails only semantically (CS9370), so the formatter sees it; output for it is garbage-in and needs no decision.
- Roslyn 5.12 marks neither `UnionDeclarationSyntax`, `WithElementSyntax`, `SyntaxKind.UnionDeclaration`, `SyntaxKind.WithElement`, nor `VisitUnionDeclaration`/`VisitWithElement` `[Experimental]` (probe: only nullable attributes). `UnionDeclarationSyntax` derives from `TypeDeclarationSyntax` and carries `ParameterList`, `ConstraintClauses`, `Members`, `SemicolonToken`.
- Analyzer tests reference `ReferenceAssemblies.Net.Net90` (`AnalyzerTestsBase{TAnalyzer,TCodeFix}.cs:68`). thoenissen/Reihitsu#1766 reports (not re-run here) that a union compiled against .NET 10 reference assemblies fails with CS0518 (`IUnion`) and CS0656 (`UnionAttribute..ctor`); against the .NET 11 runtime assemblies the probe compiled a union with static and const fields without error. An RH7101 code-fix test on a union has to deal with this.

### Mechanism and candidates

Not a bug against a spec; a behavior change. The structural reason it exists: each formatter rewriter that gives type declarations their layout or shape enumerates the concrete type-declaration kinds itself (per-kind `Visit*Declaration` overrides or type patterns), and `UnionDeclarationSyntax`, a new `TypeDeclarationSyntax` subtype, is in none of them. Membership is decidable: a pair (rewriter that enumerates kinds instead of dispatching on `TypeDeclarationSyntax`, `TypeDeclarationSyntax` subtype it omits).

Dispatch type: the `TypeDeclarationSyntax` hierarchy in Roslyn 5.12 — `ClassDeclarationSyntax`, `StructDeclarationSyntax`, `InterfaceDeclarationSyntax`, `RecordDeclarationSyntax` (two kinds), `UnionDeclarationSyntax`, `ExtensionBlockDeclarationSyntax` (`EnumDeclarationSyntax` is a `BaseTypeDeclarationSyntax` with its own handling). Kind-enumerating sites in `Reihitsu.Formatter` and `Reihitsu.Core` (`rg` for the concrete type names):

| Site | Decision | Class/Struct/Interface/Record | Union | Extension block |
|---|---|---|---|---|
| `LineBreaks/Rewriter/DeclarationBraceLineBreakRewriter.cs:170` (switch in `Visit`) | braces (FMT-BREAK-002) | handled | missing | missing — fails today (below) |
| `LineBreaks/Rewriter/MemberLineStartRewriter.cs:146-200` | one member per line (FMT-BREAK-007) | handled | missing | handled |
| `LineBreaks/Rewriter/LineBreakListRewriter.cs:563-611` | `(` joins the name (FMT-BREAK-011) | handled | missing | handled |
| `LineBreaks/Rewriter/GenericConstraintLineBreakRewriter.cs:74-83` | `where` on its own line (FMT-BREAK-023) | handled | missing | missing — fails today (below) |
| `StructuralTransforms/Rewriter/EmptyTypeDeclarationSemicolonTransform.cs:98-161` | empty body → `;` (FMT-STRUCT-017) | handled | missing | not applicable (no requirement) |
| `StructuralTransforms/Rewriter/FieldDeclarationSplitTransform.cs:51` (`Splits`) and `:170-230` | field split (FMT-STRUCT-021, FMT-PIPE-031) | handled | missing | not applicable (CS9282, RH7101-002) |
| `Reihitsu.Core/AttributeTargetUtilities.cs:476-496` | attribute-list target (SHARED-ATTR-001, FMT-STRUCT-028) | handled | missing | not checked |

Kind-agnostic, so already right for unions (read, and confirmed by output where noted): `EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely` (Core; takes the kind as a value), the precheck's FMT-UNSUP-004 scan (`PrecheckPhase.cs:282-287`, keyed on `BaseFieldDeclarationSyntax`, so a comment inside a union's combined field is refused today and after), `FieldDeclarationSplitTransform.CarriesDirective`, `ReihitsuFormatter.FormatSplitFieldInContext` (casts the parent to `TypeDeclarationSyntax`, `ReihitsuFormatter.cs:711`), the indentation and blank-line phases (no kind names; a union with its braces on their own lines gets members one level in, `where` one level in, region and doc-comment handling as a class — run), and attribute placement on its own line (run).

The issue's claim that "the CLI run indented the members of U4 relative to the wrong anchor" is not an indentation defect: the members are indented from the union's line because the braces stay on it; with the braces on their own lines the indentation is a class's (run on a nested union and a union in a namespace).

### Breaking inputs

Failure classes that reach the change: FC-016 (sibling kind: extension block, below), FC-019/FC-020/FC-021 (comments and directives in the gaps the brace, member, `(`-join, and constraint placement touch), FC-024 (`partial union`), FC-027 (nesting and enclosing scopes), FC-028 and FC-030 (code fixes that format a node holding a union: RH7101 on a union field, RH5404 on the enclosing type), FC-006 (language-version gate), FC-007 (RH7101 fix converges on a union field), FC-010 and FC-017 (idempotency under LF and CRLF), FC-031 (comments that state the old kind lists: `FieldDeclarationSplitTransform.Splits` summary, `ReihitsuFormatter.cs:711`), FC-032 (the RH5404/RH7101 fix outputs change because they delegate to the formatter).

Candidate inputs for the test implementer (each also with CRLF and a second pass):

- One-line union with expression-bodied members (U1), with a member that gets a block body and a base list (U4), with an explicit constructor and initializer (`public A7(long x) : this((int)x) { }`).
- `union U2⏎    (int, string);` and the leading-comma wrapped list (FMT-BREAK-011 example; today's output already matches its after for the wrapped part).
- Constraints: one clause on a semicolon union (U6 with `<T>`), two clauses on a braced union (U8), a semicolon union with a constraint and an empty body (FMT-STRUCT-017 example V).
- Empty body `{ }`, `{⏎}`, a body with a comment (`{ /* reserved */ }` → `{ /* reserved */⏎}`, as `class C9(int a)` gets today — run), a body with an end-of-line comment behind `{` (`{ // trailing⏎}`), a body holding `#if DEBUG⏎#endif` (FMT-STRUCT-018 keeps braces; then the braces are placed).
- `partial union` with an empty body; `readonly union`.
- Nesting: a union inside a class on one line; a union nested in a union; a union in a block namespace and at top level.
- Comments/directives: a comment between the name and `(` of the case-type list; a comment in front of `where`; a `#region` inside the body (already formatted as a class's — run).
- Field split: `static` and `const` combined fields in a union; RH7101 fix on a one-line union and on a union with braces on their own lines (fix offered after the change, converges in one iteration).
- RH5404 fix on `class C { union U(int, string) { public int X => 1; } }` (output changes; see *Output questions*).
- The CLI end-to-end test that pins the interim layout must change: `UnionWithOneLineBracesFormatted` in `src/Reihitsu.Cli.Test/EndToEnd/CliLanguageVersionEndToEndTests.cs:41-49`.

No analyzer flags the after outputs (FC-013): `apply-fix.sh` for RH5105, RH5106, RH5107, RH5108, RH5109, RH5110, RH5404, RH7101, RH5412 over the change doc's after outputs → no diagnostic. RH5035 has no fix provider, so the runner cannot check it.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `DeclarationBraceLineBreakRewriter`, `MemberLineStartRewriter`, `LineBreakListRewriter`, `GenericConstraintLineBreakRewriter` | must change (union) |
| `EmptyTypeDeclarationSemicolonTransform`, `FieldDeclarationSplitTransform` (incl. `Splits`, which `SplitsFieldDeclaration` uses) | must change (union) |
| `ReihitsuFormatter` node-level entry points | must stay; only the comment at `ReihitsuFormatter.cs:711` states the old kind list (FC-031). No entry point predicts a phase decision for this change: `FormatSplitFieldInContext` reads the phases' layout |
| Indentation, blank-line, spacing, cleanup, region, doc-comment phases | not involved (kind-agnostic; run) |
| Precheck | not involved (kind-agnostic) |
| `Reihitsu.Core` `AttributeTargetUtilities` | must stay in this run (follow-up candidate, spec author's (a)) |
| `Reihitsu.Core` `EmptyTypeDeclarationSemicolonAnalysisUtilities` | not involved (kind-agnostic) |
| Analyzers (RH5404, RH5110, RH5412–RH5416, RH7101-001) | must stay (thoenissen/Reihitsu#1767) |
| RH7101 code fix | not changed in code; its result changes through the formatter (fix offered) |
| RH5404, RH5110, and every other fix that formats a node holding a union | not changed in code; RH5404's result changes (run) |
| Rule pages | not involved |
| CLI | not involved in code; one end-to-end expectation changes |

### Refuse option

- **Leave unions as written** (today's interim layout): zero production lines. Not acceptable: a union with members is code people will commonly write once C# 15 ships, and today's output for it is visibly broken (`… CompareTo(object o)⏎    {⏎        return 0;⏎    } }`). Not a rare class.
- **Declare the shape unsupported**: the shape would be a whole `UnionDeclarationSyntax` (from its first token through its `}` or `;`, nothing excepted). It deletes no special case — the change adds a kind to existing lists and removes nothing — and it would refuse every file that declares a union, which is common code. Per `AGENTS.md` ("A shape that deletes nothing is not declared") it is not an option; named for completeness only.
- **Narrower options**: drop the field split for unions (reaches only `static`/`const` fields, a narrow class; but each site is a one-line kind addition, so dropping saves nothing worth a separate decision); drop the empty-body semicolon (that is the spec author's output question 2, option B). No narrower option recommended.

### Output questions

One addition; the spec author's questions 1–3 are otherwise sound and I agree with their recommendations (verified the before outputs and that a class gets each after today).

**1762-A · RH5404 · Element must not be on single line — a union inside the type the fix lays out**

The change doc lists the formatter's outputs and the RH7101 fix, but not the other code fixes whose result changes because they format a node that holds a union (`sdd/README.md`: `Visible output changes` lists every changed code-fix result). Run today with `apply-fix.sh RH5404`.

Input:
```csharp
class C { union U(int, string) { public int X => 1; } }
```

Before (`main`):
```csharp
class C
{
    union U(int, string) { public int X => 1; }
}
```

After, A (recommended):
```csharp
class C
{
    union U(int, string)
    {
        public int X => 1;
    }
}
```

After, B: the fix leaves the nested union as today.

Why A: the fix delegates layout to the formatter, so B would need the fix to model the formatter's old behavior. A follows from the answer to the spec author's question 3 and should be listed in the change doc as a consequence of change 1, not asked separately unless the lead prefers.

### Internal choices

- **The C# 15 gate of FMT-STRUCT-017 for unions.** It cannot be false for any caller (no union node exists under C# 14; contexts are resolved from the tree's options). Keeping it costs nothing and keeps one version per kind; dropping it removes a clause no test can exercise. Recommendation: keep the spec wording, and do not write a test for the gate's false branch — no caller produces that input (`AGENTS.md`, *Supported inputs*).
- **Per-kind additions versus dispatch on `TypeDeclarationSyntax`.** Both give the union the same output. They differ for extension blocks (below): a dispatch on `TypeDeclarationSyntax` in `DeclarationBraceLineBreakRewriter` and `GenericConstraintLineBreakRewriter` also changes extension-block output (visible), and in `FieldDeclarationSplitTransform.Splits` it would widen FMT-PIPE-031 to extension blocks. No recommendation on implementation from this role; the visible consequence is stated under *Architecture findings*.

### Issue goals

- "Also check the blank-line phase, the indentation phase and Core helpers": checked; nothing to change there for unions except `AttributeTargetUtilities` (follow-up). Drop as a goal of this run; no visible output.
- "The CLI run indented the members of U4 relative to the wrong anchor": a consequence of the braces, not a separate defect. Drop; no visible output beyond change 1.
- "`LineBreakListRewriter` … check the layout of the case-type list and the base list": the case-type list is change 2 and 3; a base list that starts its own line lands at the declaration's column, as a class's does today (run with `class A3(int a)⏎    : System.IComparable`). Keep the spec author's non-goal.
- The issue's U6 input `union U6(int,string)where T:class;` has no type parameter and does not compile; the spec uses `U6<T>`. No change.
- "Done when": specs, regression and idempotency tests — kept.

### Architecture findings

**The set of declarations that count as a type for layout and shape is stated in seven places, and the statements have drifted.** Components: `DeclarationBraceLineBreakRewriter`, `MemberLineStartRewriter`, `LineBreakListRewriter`, `GenericConstraintLineBreakRewriter`, `EmptyTypeDeclarationSemicolonTransform`, `FieldDeclarationSplitTransform`, `Reihitsu.Core/AttributeTargetUtilities` (table above). The union gap of #1762 is one member of the class; extension blocks are another, already failing today on `main`:

Input:
```csharp
public static class X
{
    extension(string s) { public int Len => s.Length; public int Two => 2; }
    extension<T>(T t)where T:class
    {
        public T Self => t;
    }
}
```

Output today (`main`):
```csharp
public static class X
{
    extension(string s) {
        public int Len => s.Length;
        public int Two => 2; }
    extension<T>(T t)where T : class
    {
        public T Self => t;
    }
}
```

`MemberLineStartRewriter` and `LineBreakListRewriter` list `ExtensionBlockDeclarationSyntax`; `DeclarationBraceLineBreakRewriter` and `GenericConstraintLineBreakRewriter` do not. Whether FMT-BREAK-002's "type" and FMT-BREAK-023's "type" include an extension block is not stated (FMT-BREAK-037 names extension blocks explicitly; FMT-BREAK-002/-023 do not), so the extension-block output is undecided, i.e. a visible output in question outside this run's issues. Any structural change that dispatches on `TypeDeclarationSyntax` in those two rewriters changes it. The lead decides; this role's facts: the union part alone is common code (refusing is not enough), and the extension-block part puts an output in question.

### Follow-up candidates

1. **Extension-block braces and constraints** (above). Output question for the follow-up:

   **Formatter · braces and `where` clauses of an extension block**

   Input:
   ```csharp
   extension(string s) { public int Len => s.Length; public int Two => 2; }
   extension<T>(T t)where T:class { public T Self => t; }
   ```
   Before (`main`):
   ```csharp
   extension(string s) {
       public int Len => s.Length;
       public int Two => 2; }
   extension<T>(T t)where T : class {
       public T Self => t; }
   ```
   After, A (recommended):
   ```csharp
   extension(string s)
   {
       public int Len => s.Length;
       public int Two => 2;
   }
   extension<T>(T t)
       where T : class
   {
       public T Self => t;
   }
   ```
   After, B: as today.

   Why A: the members are already placed one per line, so the braces glued to them are the only part left as written; a class gets A. (Before is the CLI's output at 2272e74 for exactly this input inside a static class.)

2. **The attribute lists of a union are not split** — the spec author's candidate (a); agreed, belongs with thoenissen/Reihitsu#1767.
3. **Stale facts in the RH5404 exclusion comment** (`RH5404ElementMustNotBeOnSingleLineAnalyzer.cs:89-91`): it says union APIs are experimental and need runtime types .NET 11 does not ship. Probe at Roslyn 5.12: no `[Experimental]` on the union API, and a union compiles against the .NET 11 runtime assemblies. Input for thoenissen/Reihitsu#1767 ("The RH5404 exclusion comment is resolved"), not a new issue.
4. Analyzer gaps the formatter's new union layout exposes are the rules' own gaps and already listed in thoenissen/Reihitsu#1767 (P11): RH5110 does not report `union U8<T, S>(T, S) where T : class where S : struct { … }` while it reports and fixes the class analog (run); RH5404 does not register unions; no RH5412–RH5416 rule reports an empty union body (new-rule candidate, #1767 question 2). No new issue needed.

## thoenissen/Reihitsu#1763

### Premise

Real. `[with ( StringComparer.OrdinalIgnoreCase ) , "Hello", "HELLO"]` → `[with (StringComparer.OrdinalIgnoreCase), "Hello", "HELLO"]`; `[with ()]` stays; `[with  (x)]` collapses to one space; `[with(4) , "x"]` → `[with(4), "x"]`. Cause confirmed in code: `WithSpacingRule.IsWithKeyword` requires `token.Parent is WithExpressionSyntax`; no other rule in `SpacingPolicy` decides the pair (today's output keeps both forms).

Callers that reach it: CLI and whole-document formatting; the RH5305 code fix and every fix whose formatted node holds a `with(…)` element. Run: `apply-fix.sh RH5305` on `[with (4),⏎    "a"]` → the fix writes `with (4)` on its own line today.

The issue's multi-line claim reproduces exactly; the spec author's FMT-BREAK-022 example output matches today's CLI output column for column (run).

### Mechanism and candidates

_N/A_ (behavior change). Candidate tokens: `SyntaxKind.WithKeyword` has two parents in Roslyn 5.12 — `WithExpressionSyntax` (FMT-SPACE-025) and `WithElementSyntax` (new FMT-SPACE-028). The rule keys on the parent node, so a method named `with` invoked elsewhere (an identifier token, not `WithKeyword`) is not affected.

### Breaking inputs

FC-019/FC-021 (a comment in the gap: `with /* size */ (4)` — in the spec), FC-010/FC-017 (CRLF, second pass), FC-012 (no other phase reinserts the gap), FC-030/FC-032 (code fixes that format the element change their result). A line break between `with` and `(` is not this rule's (FMT-SPACE-001); today `[with⏎    (4)]` lays out `(4)` at the statement's column — that is the spec author's follow-up candidate (b), not this rule.

Candidate inputs: the spec example's four lines; `[with(4)]` already tight (no change); `[ with (4)]` (the `[` gap is FMT-SPACE-004's); a `with` expression on the same line (`y with { A = 1 }` keeps its spaces, FMT-SPACE-025); a named argument `with (capacity : 4)`; the RH5305 fix on a wrapped collection expression that starts with `with (4)`.

No analyzer flags either form: `apply-fix.sh` RH6001 and RH6006 report nothing on `with(…)`, `with()`, `with /* size */(4)`, nor on `with (…)` (run). RH6001-002 already lists keywords it does not check (`new`, `typeof`, …), so `with` there is thoenissen/Reihitsu#1767's decision; no FC-013 conflict either way.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Horizontal-spacing phase (`WithSpacingRule` or a new rule in `SpacingPolicy`) | must change |
| Line-break phase (FMT-BREAK-022) | must stay; regression test only |
| RH5305 fix and other fixes formatting a `with(…)` element | not changed in code; result changes (below) |
| Analyzers (RH6001, RH6006, RH5305) | not involved (thoenissen/Reihitsu#1767) |
| Rule pages, CLI, Core | not involved |

### Refuse option

Keep the gap as written and collapsed (today, the spec author's option B): zero lines. The change itself is a predicate on the token's parent; not a rare class (the element is new, and both spellings will occur). Declaring a shape unsupported is not an option: there is no special case to delete. No simpler option.

### Output questions

**1763-A · RH5305 · Collection expressions should be formatted correctly — a `with (…)` element in the expression the fix lays out**

Not listed in the change doc; the fix delegates to the formatter, so its result changes with FMT-SPACE-028 (run today with `apply-fix.sh RH5305`).

Input:
```csharp
List<string> x = [with (4),
    "a"];
```

Before (`main`):
```csharp
List<string> x = [
                     with (4),
                     "a"
                 ];
```

After, A (recommended):
```csharp
List<string> x = [
                     with(4),
                     "a"
                 ];
```

After, B: the fix keeps `with (4)`.

Why A: it follows from the answer to the spec author's question 4; the change doc should list it as a consequence of change 1.

### Internal choices

- FMT-SPACE-028 as its own requirement versus a widened FMT-SPACE-025: agree with the spec author (own requirement; the two `with` tokens have opposite spacing). Position in FMT-SPACE-002's order decides nothing, since no earlier rule covers the pair (today's output proves it).

### Issue goals

Both "Done when" items serve the issue; keep them. No goal to drop.

### Architecture findings

_None._

### Follow-up candidates

_None_ beyond the spec author's candidate (b) (an argument list on the line after its invoked name), which I confirmed for `[with⏎    (4)]`: today `(4)` lands at the statement's column.

## Whole run

- Change-doc amendment for the spec author (both docs): list the code-fix results that change because the fixes format through the formatter — RH5404 on a type that holds a one-line union (1762-A), RH5305 on a collection expression that starts with `with (…)` (1763-A). The RH7101 change is already listed.
- No build or test run of the repository's suites was needed; the facts above come from the CLI, `scripts/apply-fix.sh`, and the Roslyn probe.
