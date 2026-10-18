Role: skeptic
Commit: 4965050b6c42d3e823ee5986fb2af443b8350412
Verdict: proceed narrower

The spec diff mostly states what the code does. Six statements need a repair before tests are written: two say something the code does not do, three specify output that looks wrong, and one is ambiguous where the code is not. The repairs make the specs narrower or more precise. None needs the maintainer before the run goes on; the new cases are open questions to file. Neither the lead's handover nor my prompt carried an intended fix or a hoped-for verdict.

All probes ran through `scripts/apply-fix.sh <id> <dir>` (LF and CRLF arms; the results matched) or `scripts/format.sh` on copies, in a scratchpad outside the repository. Every fixture also declared `class A : System.Attribute { }` (and `B` where used); that declaration is left out below.

## Premise

thoenissen/Reihitsu#980 asks for specs of existing behavior and makes no behavior claim, so the premise is sound. The question here is whether each requirement matches the code. The code that decides it: `TargetAttributePlacementAnalyzerBase.OnAttributeList`/`ResolveEnforcedPlacementMode`, `AttributeTargetUtilities.TryResolveTarget`/`TryResolveLayoutTarget`/`TryResolveImplicitTarget`/`IsAttributeListOnInlineAutoPropertyAccessor`, `TargetAttributePlacementCodeFixProviderBase.TryGetFixableAttributeList`/`ApplyCodeFixAsync`/`HasIncompleteEnclosingScope`, `SyntaxIndentationUtilities.ComputeBaseIndentLevel`/`HasAnchorScopeAncestor`, and the RH5513 and RH5530 overrides.

### Finding 1 — RH5530-001 states reading (a) of open question 1, against the code and the rule page (blocking)

RH5530-001 says RH5530 reports "as SHARED-ATTR-004, SHARED-ATTR-005, and SHARED-ATTR-006 specify". SHARED-ATTR-006 ends with "Every other accessor attribute list of the rule's target is checked". So the spec as written requires RH5530 to report these lists:

```csharp
class C
{
    int Q { [A] get => 1; }
}
```

```csharp
class C
{
    int S { [A] get; /* c */ set; }
}
```

Observed (`apply-fix.sh RH5530`): `no-diagnostic` for both, and also for `event System.Action E { [A] add { } remove { } }`. The control `int Q` / `{` / `[A] get => 1;` / `}` is reported and fixed. The cause is that RH5530's `ResolvePlacementMode` returns `SingleLine` for every single-line property, indexer, or event (`IsAttributeListOnSingleLinePropertyAccessor`). On a single-line declaration, a single-line placement can never be violated.

The rule page `documentation/rules/RH5530.md`, section *Exceptions*, documents the wide exemption: "A property whose declaration is single line, such as `internal int Value { [First] get; set; }`, is never reported." So the spec author's statement "every page is consistent with its spec" does not hold for RH5530 as the spec stands.

Repair: RH5530-001 must not apply SHARED-ATTR-006's last sentence to RH5530. The single-line-with-body and single-line-with-comment cases must stay out of the RH5530 spec until the open question is decided. Only the shape both readings agree on (an inline auto-property is not reported) may stay in.

### Finding 2 — The non-goal "Attribute lists of type parameters — no placement rule checks them" is false (shared spec and RH5523)

Input (`apply-fix.sh RH5523`):

```csharp
class C<[param: A]
        T>
{
}
```

Observed: `fixed (1 action(s), 1 iteration(s))`, with output `class C<[param: A] T>`. `TryResolveLayoutTarget` returns early for any list whose specifier already resolves to `Parameter` or `GenericParameter`, whatever it is attached to, so RH5523 checks this list. The non-goal must be reworded, for example to lists that resolve to the type-parameter target, or dropped. The `param:`-on-a-type-parameter case belongs with open question 2 (see below).

### Finding 3 — SHARED-ATTR-009 (and SHARED-ATTR-012) name only object initializers and anonymous objects; the code covers every `InitializerExpressionSyntax`

`HasAnchorScopeAncestor` and `HasIncompleteEnclosingScope` match `InitializerExpressionSyntax`: object, collection, array, complex-element, and `with` initializers. Input (`apply-fix.sh RH5513`):

```csharp
class C
{
    System.Action[] _a =
    {
        () =>
        {
                [A] void L()
                {
                }
        }
    };
}
```

Observed: `void L()` goes to column 16, aligned with `[A]`. That is the anchor path. Under SHARED-ATTR-008, which the spec applies to an array initializer, the result would be column 12 (class, initializer, block). The same happens for a collection initializer (`new System.Collections.Generic.List<System.Action> { () => { [A] void L() ... } }`, column 16) and a `with` initializer (`_x with { H = () => { [A] void L() ... } }`, column 16). The control without an initializer (`System.Action _a = () => {` with `[A] void L()` at column 12) takes the depth path: `void` goes to column 8. Repair: name every initializer kind in SHARED-ATTR-009 and SHARED-ATTR-012.

### Finding 4 — SHARED-ATTR-008 specifies an out-dent that looks wrong: a block indented by continuation, not by nesting

SHARED-ATTR-008 counts enclosing braces and says the indentation "MUST NOT depend on the column of the list or on the indentation of other lines". Its `Why:` claims this is "the one the declaration has at its nesting depth". That does not hold when a block's opening brace sits at a continuation column. Input (`apply-fix.sh RH5513`):

```csharp
class C
{
    void M()
    {
        Run(1,
            () =>
            {
                [A] void L()
                {
                }
            });
    }

    void Run(int a, System.Action b)
    {
    }
}
```

Observed: `[A]` stays at column 16 and `void L()` moves to column 12, the column of the lambda's own `{`. `scripts/format.sh` puts `void L()` of the same input at column 16. The same happens in a collection expression: `System.Action[] _a =` / `[` / `() =>` / `{` (column 8) / `[A] void L()` (column 12) gives `void L()` at column 8. RH5204 reports neither result, so there is no FC-007 cross-diagnostic. The output still looks wrong, and the spec states it as intended.

Repair: leave this case out of SHARED-ATTR-008 and file it as an open question in the shared spec's issue (see *Questions for the maintainer*). The mechanism, decidable for any construct: the depth path counts the braces that enclose the list, so a declaration in a block whose opening brace is indented by continuation rather than by nesting (a lambda body in an argument or a collection-expression element that starts its own line) gets the column of that brace instead of one level deeper. The switch-section control is correct: `case 1:` / `{` / `[A] void L()` gives column 20, consistent with SHARED-ATTR-008.

### Finding 5 — SHARED-ATTR-009 "column" is a character offset; with tab indentation the result looks wrong

Input (`apply-fix.sh RH5513`, tabs shown as `\t`):

```text
class C
{
\tD _d = new D
\t{
\t\tH = () =>
\t\t{
\t\t\t[A] void L()
\t\t\t{
\t\t\t}
\t\t}
\t};
}
```

Observed: `void L()` gets three spaces of indentation (`ComputeTokenColumn` returns the character offset 3), while `[A]` sits at visual column 12 with a tab width of 4. The depth path writes canonical spaces whatever the tabs (`VerifyCodeFixUsesSpacesRegardlessOfSurroundingTabIndentation`); the anchor path does not. Repair: leave tab indentation inside an initializer out of SHARED-ATTR-009 and file it with finding 4.

### Finding 6 — SHARED-ATTR-012's "encloses" does not match the guard's scope (from code, not executed)

`HasIncompleteEnclosingScope` walks from `attributeList.Parent`, so the declaration that owns the list counts too: a type or enum whose own brace is missing withholds the fix for its own attribute list.

```csharp
[A] class C
{
```

From the code: RH5505 reports `[A]` (`]` and `class` share a line, and the gap has no trivia), and `ClassDeclarationSyntax.CloseBraceToken.IsMissing` withholds the fix. `apply-fix.sh` refuses unparseable fixtures, so this was not run. SHARED-ATTR-012's `Why:` ("counts the enclosing braces") and SHARED-ATTR-008's "whose braces enclose the list" read as brace enclosure. Under that reading this fix would be offered, because the class's braces do not change the list's indentation level. The spec must either state the owner case explicitly or leave it out. The guard is wider than its rationale. That is a guard-scope follow-up candidate (`AGENTS.md`, *Code-fix coverage*), not a defect in the output; whether to lock it in with a MUST is the spec author's call.

### Minor precision items (not blocking)

- RH5527-001 says "on a method, … accessor, or any other declaration", and RH5501-001 and RH5503-001 say "every attribute list with the `assembly:`/`module:` specifier". None of them uses SHARED-ATTR-003. Observed: `void M([return: A] int x)` gives `no-diagnostic` for RH5527, and `void M([assembly: A] int x)` gives `no-diagnostic` for RH5501. Both are correct per SHARED-ATTR-002's exception, but the rule sentences read as including parameters.
- `type:` is enumerated in RH5505, RH5507, RH5509, RH5513, RH5521, RH5525, and RH5530-001, but not in RH5511, RH5515, RH5517, or RH5519. Observed: `[type: A] C()` is reported and fixed by RH5511. SHARED-ATTR-001 covers it by reference; the enumerations are inconsistent.
- SHARED-ATTR-008's "The list itself stays as written" and SHARED-ATTR-004's `Why:` ("The placement is about what follows the list") read as deciding open question 3 for reading (b). Check that neither pre-decides it.
- SHARED-ATTR-008's "the line ending the document already uses" is FMT-PIPE-007's detection (`LineEndingUtilities`: predominant ending, a tie goes to CRLF, and `Environment.NewLine` when there is no line break). Observed: `[A] class C { }` with no line break at all gets LF on Linux; the CRLF arm flags it as `[line-ending-drift]`. `shared/statement-blank-lines.md` uses the same wording, so this follows precedent; a reference to FMT-PIPE-007 would be exact.
- SHARED-ATTR-009 is listed under `Uses` (and named in -003) for RH5505, RH5507, RH5509, RH5511, RH5521, and RH5525. Their declarations cannot occur inside an initializer, so no test can show it for those rules. RH5513 tests still cover the ID.

## Mechanism and candidates

_N/A_ — no bug is being fixed. The dispatch the tests must span:

- `TryResolveTarget`: `assembly`, `module`, `field`, `event`, `method`, `param`, `property`, `return`, `typevar`, `type` (→ implicit), unknown (→ none), and no specifier (→ implicit).
- `TryResolveImplicitTarget`: `ClassDeclarationSyntax`, `StructDeclarationSyntax`, `RecordDeclarationSyntax` (class/struct keyword), `InterfaceDeclarationSyntax`, `EnumDeclarationSyntax`, `DelegateDeclarationSyntax`, `MethodDeclarationSyntax`, `OperatorDeclarationSyntax`, `ConversionOperatorDeclarationSyntax`, `DestructorDeclarationSyntax`, `LocalFunctionStatementSyntax`, `LambdaExpressionSyntax`, `AccessorDeclarationSyntax`, `ConstructorDeclarationSyntax`, `PropertyDeclarationSyntax`, `IndexerDeclarationSyntax`, `FieldDeclarationSyntax`, `EnumMemberDeclarationSyntax`, `EventDeclarationSyntax`, `EventFieldDeclarationSyntax`, `ParameterSyntax`, `TypeParameterSyntax`; anything else resolves to none.
- `TryResolveLayoutTarget`: `Parameter`/`GenericParameter` return early whatever the owner; otherwise a `ParameterSyntax` or `TypeParameterSyntax` owner overrides the target.
- Indentation: `IsIndentingAncestor` (block, type, namespace, enum, switch statement, switch section, accessor list, initializer, anonymous object); `HasAnchorScopeAncestor` (initializer, anonymous object); `HasIncompleteEnclosingScope` (block, type, namespace, enum, switch statement, accessor list, initializer, anonymous object, starting at the owner).

Inputs the code decides that the specs do not mention: `param:` and `typevar:` on owners they do not name (open question 2, widened below); an attribute list on a statement other than a local function (`[A] int x = 1;` in a method gives `no-diagnostic` for RH5513; no target); the last list before end of file with no token after it (not reported).

## Breaking inputs

For the test implementer, derived from the dispatch above, not from the issue:

- **FC-003** — SHARED-ATTR-006 on both sides: an inline auto-property (not reported), an accessor with a body or an expression body, and a comment inside the accessor list (reported by RH5527). A comment after the accessor list's `}` and a property attribute list on its own line keep the exemption.
- **FC-005, FC-019, FC-020, FC-021** — SHARED-ATTR-005 with `//`, `/* */`, `#region`, and `#if false` disabled text in the gap, for a separate-line rule and for RH5523. SHARED-ATTR-011 with a comment and with a directive inside the list (`[A` / `#if X` / `, B` / `#endif` / `]`), plus a documentation comment in front (the fix is still offered).
- **FC-027** — SHARED-ATTR-008 scopes: block-scoped and file-scoped namespace, nested type, accessor list, switch section with and without a block, lambda block. SHARED-ATTR-009: object initializer and anonymous object. Do not tag cases that depend on findings 3–5 until the spec is repaired.
- **FC-008, FC-026** — Fix All over `[A] [B] void M()`, over two identical lists `[A] [A] void M()`, and over RH5523 `[A]` / blank line / `[B]` / `int a`. The last converges in 2 iterations to `void M([A] [B] int a)`.
- **FC-017, FC-033** — the inserted line break under LF and CRLF (the existing RH5505 CRLF test).
- **FC-013** — the open-question shapes (RH5530 single-line with body or comment; a list preceded by source on its line; `param:`/`typevar:` on other owners) must not be tagged as requirements.
- SHARED-ATTR-002/-003 — an unknown specifier; `[field: A]`, `[method: A]`, and `[property: A]` on a property; `[return: A]` on a delegate, a local function, and a lambda; `[return: A]`, `[assembly: A]`, `[property: A]`, and `[type: A]` on a parameter (RH5523 only).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Specs (`shared/attribute-targets.md`, RH5523, RH5530, and the precision items) | Must change — findings 1–6 |
| 15 analyzers and the two analyzer bases | Must stay |
| 15 code-fix providers and `TargetAttributePlacementCodeFixProviderBase`, Fix All (`BatchFixer`) | Must stay |
| `Reihitsu.Core` (`AttributeTargetUtilities`, `SyntaxIndentationUtilities`, `LineEndingUtilities`) | Must stay |
| Formatter (`AttributeTargetFormattingRewriter`, LineBreakPhase) | Not involved; must stay |
| Rule pages | Must stay. `RH5530.md` *Exceptions* documents the wide exemption; the spec must not contradict it while question 1 is open |
| Tests | Test implementer: tags plus focused tests, per *Breaking inputs* |

## Simpler option

_None._ The repairs narrow or correct sentences; no requirement needs to be added.

## Questions for the maintainer

None blocks the run. These are open questions to file per `sdd/README.md` (*Open questions*), in the shared spec's issue, not escalations.

**Open questions from the spec author, assessed:**

1. *RH5530's wider single-line exemption.* Real: RH5530 gives `no-diagnostic` and RH5527 reports `int R { [return: A] get => 1; }`. The formatter turns `int Q { [A] get => 1; }` into `int Q` / `{` / `[A]` / `get => 1;` / `}` (FC-013). It is correctly placed in RH5530's issue, but it is not left out of the specs yet (finding 1). The question should cite `documentation/rules/RH5530.md` *Exceptions* as documented support for reading (b), and say that reading (a) changes that page.
2. *`param:` on an accessor.* Real, but scoped too narrowly. The mechanism: a `param:` list resolves to the parameter target and a `typevar:` list to the type-parameter target, whatever it is attached to. Observed with RH5523: `[param: A]` / `void M(int x)` → `[param: A] void M(int x)`; `[param: A]` / `class C` → `[param: A] class C`; `class C<[param: A]` / `T>` → `class C<[param: A] T>`. `void M([typevar: A]` / `int x)` gives `no-diagnostic`. The formatter agrees (it keeps `[param: A] void M(int x)`, `[typevar: A] void N<T>()`, and `[param: A] class D` inline, and joins the two wrapped lists). That keeping is not what FMT-BREAK-027's "each attribute list of a … declaration MUST start its own line" says, so the question also touches FMT-BREAK-027; its formatter side belongs to the open-question issue for `formatter/line-breaks.md`. Widen the question to the mechanism, and fix the false non-goal (finding 2).
3. *A list preceded by source on its line.* Real and correctly scoped to the shared spec: no analyzer reports `int _a; [A]` / `void M()`, while FMT-BREAK-007 and FMT-BREAK-027 put `[A]` on its own line (FC-013/FC-014). It is left out of the requirements, apart from the two sentences under *Minor precision items*.

**New questions (findings 4 and 5):**

4. *Fix indentation inside a block indented by continuation.* Input: the `Run(1, () => { [A] void L() … })` example in finding 4.
   - (a) Indent to one level inside the enclosing block's brace: `void L()` at column 16, matching `[A]` and the formatter.
   - (b) The nesting-depth count is intended: `void L()` at column 12.
   Touches SHARED-ATTR-008.
5. *Anchor-path column under tab indentation.* Input: the tab example in finding 5.
   - (a) Indent to the visual column of the declaration's first token, in spaces: 12 spaces.
   - (b) The character offset is intended: 3 spaces.
   Touches SHARED-ATTR-009.

On the spec author's note about `AttributeTargetUtilities`: `sdd/README.md` (*Code owners*: "a type whose behavior a spec specifies belongs to that spec even when code outside that spec calls it") points to it as a code owner of the shared spec, because SHARED-ATTR-001 to -003 and -006 specify its resolution methods. The maintainer named the owners explicitly, so it stays the maintainer's call.

Follow-up candidates, out of scope: the guard scope of `HasIncompleteEnclosingScope` (finding 6), and the LineBreakPhase/IndentationPhase back-and-forth over a lambda attribute that the spec author reported.
