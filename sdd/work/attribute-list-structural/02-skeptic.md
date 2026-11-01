Role: skeptic
Commit: 577e812
Verdict: output question

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I read thoenissen/Reihitsu#1492 and its parent thoenissen/Reihitsu#1482, `00-lead.md`, `01-spec-author.md`, and the `sdd/` diff of 91e64ce against `origin/main`. Every output below was measured on this branch with `scripts/format.sh` and `scripts/trace.sh`. The branch has no production change yet, so this is `main`'s behavior. Fixtures were kept in the session scratchpad, outside the repository.

## Premise

- **Real.** `AttributeTargetFormattingRewriter.FormatAttributeLists` (`src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/AttributeTargetFormattingRewriter.cs`) runs `SplitAttributeLists` (which calls `BuildSplitAttributeLists`, built with `SyntaxFactory.AttributeList`), then `MergeAttributeLists` (which calls Core's `AttributeTargetUtilities.MergeAttributeListGroup`), then `ApplyPlacement`. It runs as the twelfth rewriter of `LineBreakPhase.CreateRewriters`. Whole-document formatting reaches it for every owner that `AttributeTargetUtilities.GetAttributeLists` dispatches. The node-level entry points reach it the same way, because `FormattingPipeline.CreatePhases` runs every phase in every entry point.
- **The issue's stated purpose is met only in part.** The motivation is that no check mapping tokens across the line-break phase should have to special-case this rule. That stays unmet for accessors: FMT-BREAK-032 keeps their split and merge in the line-break phase, and FMT-BREAK-031 keeps its exception (lead decision 2). I checked the accessor measurements in `01-spec-author.md` and agree with them. `int Q { [A, B] get => 1; }` is expanded and split. `int S { [A] [B] get; set; } =⏎ 1;` is merged. The shape follows layout that the line-break phase produces. What the run still delivers is that the shape of declaration, parameter, and type-parameter lists is decided before any layout.
- **The change doc says no visible output changes. That is not true for the code being moved** (see *Breaking inputs* 1 and 2). Parent thoenissen/Reihitsu#1482 rule 3 requires that "a package changes no output, or exactly the outputs its change doc names".

## Mechanism and candidates

_N/A_ for the change itself. It is a behavior change. The two defects below have their own mechanism sentences.

Owners, taken from the dispatch in `AttributeTargetUtilities.GetAttributeLists` / `WithAttributeLists` and from `TryResolveImplicitTarget`:

| Owner (defining type) | Shape today | Owner in the spec diff |
|---|---|---|
| `CompilationUnitSyntax` (`assembly:`, `module:`) | split | FMT-STRUCT-028 |
| `MemberDeclarationSyntax`: class, struct, interface, record (class/struct), enum, delegate, method, constructor, destructor, operator, conversion, property, indexer, event, event field, field | split | FMT-STRUCT-028 |
| `EnumMemberDeclarationSyntax` | split | FMT-STRUCT-028 |
| `LocalFunctionStatementSyntax` (also top-level) | split | FMT-STRUCT-028 |
| `LambdaExpressionSyntax` | split (target `Method`) | **none**: FMT-STRUCT-028 leaves it unspecified (see amendment 3) |
| `AccessorDeclarationSyntax` | split or merge, by layout | FMT-BREAK-032 |
| `ParameterSyntax` (method, constructor, delegate, local function, lambda, indexer, record primary constructor) | merge | FMT-STRUCT-029 |
| `TypeParameterSyntax` | merge | FMT-STRUCT-029 |
| `BaseNamespaceDeclarationSyntax`, `GlobalStatementSyntax`, `IncompleteMemberSyntax` (are `MemberDeclarationSyntax`) | no implicit target, so neither split nor merged | not mentioned; no caller reaches them (attributes there do not compile, or are a syntax error that the formatter skips) |

## Breaking inputs

### 1. The split leaves an attribute's own whitespace and line breaks inside the bracket it creates (pre-existing, inside the moved code)

Mechanism: `BuildSplitAttributeLists` wraps each attribute together with its own leading and trailing trivia in new brackets and drops the commas with their trivia. When the separator's line break was trivia of the attribute rather than of the comma, the created list keeps a line break or whitespace inside its brackets. `HorizontalSpacingRewriter` only rewrites a gap through the left token's trailing trivia (or behind a leading comment). Whitespace that the right token holds as leading trivia on the same line is therefore never normalized.

Class matrix. Axes: which side of the attribute (leading trivia of an attribute after the first; trailing trivia of an attribute before the last) × what the trivia holds (whitespace only; a line break; a blank line) × owner (declaration list, FMT-STRUCT-028; accessor in a multi-line accessor list, FMT-BREAK-032, which runs the same code).

| Input (method; accessor) | Pass 1 | Pass 2 | Result |
|---|---|---|---|
| `[A,⏎ B] void T()`; `[C,⏎ D] get;` | `[A]⏎[     B]` (the whitespace that was B's indentation) | `[A]⏎[B]` | **fails**: FMT-PIPE-004 (idempotency) and FMT-SPACE-004. Fails for both owners. |
| `[A,⏎⏎ B] void T()` | `[A]⏎[⏎ B]` | same | stable, looks wrong (output question 1) |
| `[A⏎ , B] void T()`; `[C⏎ , D] get => 1;` | `[A⏎ ]⏎[B]` | same | stable, looks wrong (output question 1). Same as a list written `[A⏎ ]`, which today is also kept. |
| `[A , B]`, `[ A , B ]`, `[Obsolete( "x" ) , B( 1 )]` | `[A]⏎[B]`, `[Obsolete("x")]⏎[B(1)]` | same | holds (a whitespace-only gap that the left token holds) |
| `[A, B⏎] void T()` | `[A]⏎[B⏎]` | same | holds as written: the `]` was already on its own line |

The first row is a safety-rail defect (escalation 2 class: idempotency) in exactly the code this run moves. Because FMT-STRUCT-026 says "the trivia it carries over keeps what the author wrote", a faithful port reproduces it. The fix is demanded by unchanged requirements (FMT-PIPE-004, FMT-SPACE-004), so it puts nothing in question. The change doc must still name it, because parent rule 3 requires every changed output to be named.

### 2. The merge removes, before the line-break phase, a line break that today's line-break phase reads as a wrap (new with the move)

Mechanism: today `LineBreakListRewriter` runs before the attribute rewriter. It sees the parameter's lists as written, so a line break between or behind the lists of one parameter makes the element multi-line (FMT-BREAK-013), and the parameter list gets one element per line. The structural merge now runs first. Whether that line break survives it decides the output, and FMT-STRUCT-029 does not say. FMT-STRUCT-026 says only "MAY drop" for trivia of removed tokens, and Core's merge, which the issue suggests reusing, drops it.

Class matrix. Axes: where the line break is (behind the first list of the group, which is kept; behind a removed list; behind the last list) × enclosing list (parenthesized parameter list of a method, local function, lambda nested in an argument list, or record; bracketed indexer parameter list; type parameter list).

| Input | Today | If the merge drops the break |
|---|---|---|
| `void P([A]⏎ [B] int x, int y)` | `void P([A, B] int x,⏎ int y)` | `void P([A, B] int x, int y)`: **changes** |
| `void P([A] [B]⏎ int x, int y)` | same as above | **changes** |
| `record R([A]⏎ [B] int X, int Y);`, `record R2([A]⏎ [property: B] [C] int X, int Y);` | wrapped, one parameter per line | **changes** |
| `void L([A]⏎ [B] int x, int y)` (local function) | wrapped | **changes** |
| `Use(([A]⏎ [B] int x, int y) => x, 1);` | the lambda's parameter list and the argument list both wrapped | **changes** |
| `int this[[A]⏎ [B] int i, int j]`, `class G<[A]⏎ [B] T, U>` | joined onto one line (FMT-BREAK-014) | holds |
| single parameter `void Q([A]⏎ [B] int x)`, and RH5524's `[First]⏎[Second]⏎⏎int id` | one line | holds |

For comparison, the placement-only case `void P([A]⏎ int x, int y)` gives `void P([A] int x,⏎ int y)` today and does not change. If the merge dropped the break, inputs 1 and 2 of the table would format differently from this neighbor. That is amendment 2.

### Failure classes that reach the change

- FC-015 / FC-016: two owners of the split (structural for declarations, line-break for accessors) and of the merge (Core's merge for code fixes and accessors, plus a new one). See *Architecture findings*. Defect 1 already fails in both copies.
- FC-004 / FC-005 / FC-019 / FC-020 / FC-021, guards of both transforms:
  - split: interior `/* */`, `//`, `///` (the doc phase normalizes it but does not move it out: `[A, /// x⏎ B]` stays one list), interior `#if`;
  - comment or doc comment in front of the list, comment behind it, `#if`/`#region` around it (all split);
  - merge: comment in the stretch, comment leading the token behind the group, directive leading the first list (blocks), comment in front of the first list (merges), `///` between the lists of a parameter (blocks; today `[A]⏎⏎/// …⏎[B] int p` stays unmerged).
- FC-006: `[type: A] [B] int d` is not merged. `[unknown: A, B]` is neither split nor merged. `[type: A, B] void M()` is split (`TryResolveTarget` falls back to the implicit target). `[method: A, B]` and `[return: A, B]` are split.
- FC-010 / FC-017: every test of both transforms needs a two-pass and a CRLF check. Defect 1 passes a one-pass test that only checks tokens.
- FC-027: lambda attribute lists as a variable initializer and as an argument (`Use([A, B] (int x) => x, 1)` today gives `Use([A]⏎[B]⏎(int x) => x,⏎1)`). Lambda parameters inside an argument list (defect 2).
- FC-028: `FormatNode` / `FormatNodeInDocumentAsync` on a method whose list holds `[A, B]` and whose parameter has `[A] [B]`. FMT-STRUCT-016 keeps both transforms in every entry point. Caller: every code fix that formats a member through `FormatNodeInDocumentAsync`, e.g. the RH3202 expression-body fix on an attributed method.
- Phase order inside the structural phase: `[A, B] int a, b;` with FMT-STRUCT-021 gives `[A]⏎[B]⏎int a;⏎[A]⏎[B]⏎int b;` today, in either order of the two transforms.
- FC-013: no change. The RH55xx shape rules keep reporting the same final shapes.

Inputs the test implementer must cover (besides the spec examples): every row of both matrices, under LF and CRLF, two passes; one owner of each row of the owner table; the guard list above.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `StructuralTransformPhase` (new split and merge transforms) | must change |
| `AttributeTargetFormattingRewriter` | must change: keeps placement and the accessor shape only |
| `LineBreakListRewriter.IsMultiLineAfterAttributeFormatting` | must stay. It runs the attribute rewriter to read an element as the pass emits it, an existing prediction of a later rewriter in the same phase. After the move it covers less (placement and accessor shape), and stays correct only because it calls the real rewriter. |
| Core `AttributeTargetUtilities` (`MergeAttributeListGroup`, guards, targets) | must stay for the code fixes (SHARED-ATTR-017/018); see *Architecture findings* for the merge |
| `HorizontalSpacingPhase` | not involved, provided the split leaves no same-line leading whitespace (defect 1) |
| Analyzers RH55xx and their code fixes | must stay |
| Rule pages | not involved |
| `AGENTS.md` ownership rows, code-owner lists | must change (lead decision 3) |
| `PhaseReversalExceptionList` | must stay. Its one attribute entry (`MinimalApiAttributedLambdaSettlesInOnePass`) has a single attribute, so neither transform touches it. |

## Simpler option

The issue's own alternative: keep the split and merge in FMT-BREAK and record that this rule changes tokens. With accessors left in the line-break phase anyway, that special case survives the run in a narrower form. The move is still worth making, but only if each transform has one implementation (see *Architecture findings*). If the run would need a second copy of the split or of Core's merge, the simpler option is better: parent rule 2 says "no package adds a copy".

## Spec amendments needed (before tests)

1. **FMT-STRUCT-028: what a created list holds between its brackets.** State the trivia of each attribute in a created list. Whatever the answer to output question 1, `[A,⏎ B]` must give `[A]⏎[B]` in one pass. The change doc's *Visible output changes* must name that row as a fix demanded by FMT-PIPE-004 and FMT-SPACE-004, plus whatever output question 1 adds. FMT-BREAK-032 inherits it ("as FMT-STRUCT-028 states").
2. **FMT-STRUCT-029: what the merge does with a line break in the stretch it rewrites.** For the change doc's "no visible output change" to hold, the merge must keep a line break the author wrote anywhere in the stretch, whether it is trivia of the first list or of a removed list. The line-break phase then still sees the parameter as written over several lines, and placement joins it (FMT-BREAK-027). This also keeps defect 2's rows the same as the placement-only neighbor. Recommended. If the lead instead lets the merge drop the line break, the five "changes" rows of defect 2 become a visible change and must go to the maintainer as an output question.
3. **Change doc: which phase splits a lambda's lists.** The change doc says the line-break phase keeps only placement and the accessor shape, and that all output stays unchanged. Together these force the structural transform to split lambda lists, which FMT-STRUCT-028 leaves unspecified. State it in the change doc: the lambda split moves with the declarations and keeps today's output, pending follow-up 1 of `01-spec-author.md`. Otherwise the implementer has to make the choice silently.

## Output questions

**1 · Formatter · splitting an attribute list written over several lines**

Input:
```csharp
class C
{
    [A
     , B] void M() { }

    [A,

     B] void N() { }
}
```

Before (`main`):
```csharp
class C
{
    [A
     ]
    [B]
    void M()
    {
    }

    [A]
    [
     B]
    void N()
    {
    }
}
```

After, A (recommended):
```csharp
class C
{
    [A]
    [B]
    void M()
    {
    }

    [A]
    [B]
    void N()
    {
    }
}
```

After, B: unchanged from `main`. In both options, `[A,⏎ B] void O() { }` becomes `[A]⏎[B]⏎void O()…` in one pass (today the first pass gives `[     B]`).

Why A: the split builds new brackets, so a line break that only separated the attributes has nothing left to separate; the merge already drops each attribute's own trivia.

## Internal choices

1. **One split, one merge.** See *Architecture findings*. Recommend that the structural transform and the accessor path share a single split, and that the structural merge and Core's merge be one merge whose callers differ only in the separators FMT-STRUCT-026 and SHARED-ATTR-017/018 give them. Nothing visible is in question either way.
2. **The spec author's internal choice 3 (no FMT-STRUCT-026 exception).** Agree, with amendment 2 added. The space Core writes is spacing-neutral (FMT-SPACE-003/008). The line breaks it drops are not neutral (defect 2).

## Issue goals

- "No line-break rewriter changes the token sequence": narrowed to every owner except accessors. This is already lead decision 2, and nothing visible changes.
- "The guards stay as FMT-BREAK-027 states them": keep.
- "Formatter output is unchanged" (Proposed Behavior) and "existing attribute-list tests pass unchanged" (Done when): keep, except for the one-pass output of defect 1's first row, which unchanged requirements demand, and output question 1 if the maintainer picks A. No existing test asserts any of those outputs: I searched the formatter and analyzer tests for wrapped attribute lists, and none splits a list whose separator's line break is attribute trivia.
- The issue names `ApplyListShape`, which does not exist; the entry is `FormatAttributeLists` (already noted by the spec author).

## Architecture findings

1. **The split would have two owners.** The structural transform would split the lists of declarations (FMT-STRUCT-028). `AttributeTargetFormattingRewriter` would still split accessor lists (FMT-BREAK-032) through its own `BuildSplitAttributeLists`. The decision they share is how one list becomes several: the guards, and the trivia of each created list. The spec already states it once (FMT-BREAK-032 refers to FMT-STRUCT-028). The input that shows the risk is defect 1, which fails today in both places (`[A,⏎ B] void T()` and `[C,⏎ D] get;`). If it is repaired in only one copy, the other keeps it. This is FC-015 and FC-016.
2. **The merge would have two owners.** Core's `AttributeTargetUtilities.MergeAttributeListGroup` writes a space behind the merged list and drops the line breaks. It is used by the code fixes and by the accessor merge. Under FMT-STRUCT-026 and amendment 2, the structural merge must write no whitespace and must keep line breaks. A narrow implementation is a second merge next to the shared helper, which `squad/README.md` (*Architecture findings*) names, and parent thoenissen/Reihitsu#1482 rule 2 forbids ("no package adds a copy"). The decision they share is which lists merge, and what happens to the trivia of the stretch. Defect 2's first row shows the two disagreeing.

Both findings fall within the components the run changes (the formatter's line-break and structural phases, and Core's merge, the counterpart). The narrow fix would add the copy itself. Per `squad/README.md`, that is the case for changing the structure in this run.

## Follow-up candidates

1. **The spacing phase does not normalize same-line whitespace held as the right token's leading trivia.** `HorizontalSpacingRewriter.VisitToken` rewrites only the left token's trailing whitespace. The *Why* of FMT-PIPE-021 says "which token holds a line break or whitespace decides nothing a phase relies on", and this contradicts it. Any phase that leaves a token's line-indentation whitespace behind on the same line as the previous token gives a result that the second pass changes. Defect 1 is one instance. Structural cause: a stated rule (FMT-PIPE-021) that a component does not keep. Outside the run once defect 1 is fixed in the split.
2. **Line breaks inside an attribute list's brackets are never joined.** Whole-document formatting reaches this. Input `[A⏎ ] void M() { }`; output (`main`) keeps `[A⏎ ]` on two lines. Reading A (recommended): `[A]`. Reading B: today's output. Not part of this run unless output question 1's answer covers the split's case only.
3. **A blank line is inserted in front of a `///` comment inside a parameter's attribute group.** Input `void M([A] /// <summary>x</summary>⏎ [B] int p, int q) { }`. Output (`main`): `void M([A]⏎⏎ /// <summary>…` with a blank line inside the parameter list. Reading A: no blank line inside a parameter list. Reading B: today's output. The compiler warns (CS1587) on such a comment anyway, so the input is rare.
4. The two follow-ups of `01-spec-author.md` (lambda placement; accessor attribute on a one-line auto-property whose initializer spans several lines) stand. I did not re-derive them.
