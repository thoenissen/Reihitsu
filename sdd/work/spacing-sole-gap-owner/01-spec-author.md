Role: spec author
Commit: cf3e5f1 (code and test statements: `main` at e3842c8 plus the claim commits, no code change); the spec diff and change doc are in the commits after it, up to the one that adds this handover
Verdict: change doc written

The prompt carried no intended fix. The issue prescribes a solution and a target behavior; it is `agent-filed`, so both were treated as proposals, and where this handover recommends otherwise it says so under `Output questions`. Every "today" output below was produced by running the Debug build of `Reihitsu.Cli` on the input (probe harness in the scratchpad, not committed), or by `scripts/trace.sh` for phase-level text.

## Before the run — the issue's statements against current `main`

| # | Statement of thoenissen/Reihitsu#1493 | On `main` at e3842c8 |
|---|---|---|
| 1 | `ListElementInteriorJoinRewriter.CreateSeparator` asks `SpacingPolicy` and falls back to `IsTightBefore`/`IsTightAfter`/`WouldMergeWhenTight` | Partly moot. It no longer asks `SpacingPolicy`; it chooses none or one space from its own tables `IsTightBefore`/`IsTightAfter`, guarded by the shared lexer helper `TokenSeparationUtilities.WouldMergeIntoOneToken` (made by #1513, the B3 run). `WouldMergeWhenTight` is gone. The per-kind choice is still there. |
| 2 | Five rewriters write `SyntaxFactory.Space`: `LineBreakAssignmentRewriter`, `TernaryLineBreakRewriter`, `SwitchCaseWhenLineBreakRewriter`, `PropertyLayoutLineBreakRewriter`, `AccessorListLayout` | True. A sixth writer is reached from the phase: `AttributeTargetUtilities.MergeAttributeListGroup` in `Reihitsu.Core` (see the inventory, row 11). The assignment rewriter's comment about leaving the space to the spacing phase is still there. |
| 3 | For a pair without a rule, the spacing phase keeps the gap and collapses runs (FMT-SPACE-002), so the joiner's choice becomes the output | True. |
| 4 | `int P =>1;` becomes `int P => 1;`, while `y =>y` and `1 =>1` stay | True (reproduced; also `int Q=>1;` → `int Q => 1;`, `int V =>(1);` → `int V => (1);`). |
| 5 | `[return :Obsolete]` becomes `[return:Obsolete]`, `[field :NonSerialized]` keeps its space; the join writes the colon tight | True (reproduced; `[method : Obsolete]`, `[param :Obsolete]`, `[assembly :X]`, `[typevar :X]` also keep theirs). |
| 6 | FMT-SPACE's fourth non-goal leaves the arrow without a rule until #958 | True before this diff. #958 now holds only the arrow part (the rest moved to thoenissen/Reihitsu#1440), and thoenissen/Reihitsu#1514 is the approved follow-up for expression-body arrows from #1513. |
| 7 | Done when: "`SpacingPolicy` is consulted only by the spacing phase" | Already true: no type outside `Pipeline/HorizontalSpacing` references it (`HorizontalSpacingRewriter.TrimGapBeforeToken` is inside the phase). |
| 8 | Done when: `NoSpaceSpacingRule`'s remark that "the chain rewriter already closes the gap that follows a conditional access or an arrow" is re-checked, "since the chain rewriter no longer closes any gap" | The remark is inaccurate, but not for the issue's reason: `ChainLineBreakRewriter.CloseGap` still closes gaps — only gaps that hold a line break (FMT-BREAK-020), never a same-line gap — and `FluentChain` does not cover the pointer arrow `->` at all. The rule's comment should state why its pairs are tight (FMT-SPACE-006's Why) instead of pointing at the chain rewriter. This is a comment-only item for the implementer or cleaner; no spec change. |
| 9 | Blocked by B3 (#1488) | Closed by #1513. Its contract (FMT-STRUCT-026: a phase writes only what its text needs; FMT-PIPE-021) is what FMT-BREAK-031 extends to the line-break phase. |

## Inventory — every place outside the spacing phase that writes, chooses, or rewrites a same-line gap

Line-break phase (all reached by whole-document formatting, the CLI, and every code fix that formats through a node-level entry point):

| # | Place | What it writes today | Pairs |
|---|---|---|---|
| 1 | `ListElementInteriorJoinRewriter.JoinInteriorWraps` / `CreateSeparator` (FMT-BREAK-015) | Joins a wrap inside an argument, parameter, attribute-argument, tuple, or type-argument element. None if `IsTightBefore(token)` or `IsTightAfter(previous)` and the lexer helper says the pair stays two tokens; otherwise one space. | Tight today: before `(` of an argument list, attribute argument list, `typeof`/`default`/`sizeof`/`checked`/`unchecked`/`__reftype`/`__refvalue`/`__makeref`; before `[` of a bracketed argument list, array rank, function-pointer calling-convention list; before `.`, `::`, `..`; before a nullable `?`, a pointer or function-pointer `*`, the `:` of a name colon, expression colon, or attribute target, the `<` of a type-argument, type-parameter, or function-pointer list; after `(`, `[`, `.`, `::`, `..`, such a `<`, a cast's `)`, a prefix unary operator. Spaced today: every other pair — a name colon and its value, both sides of an arrow, a keyword and the next token (`out var`, `await x`, `static (`, `in (`, `var (`, `stackalloc [`), a type and its name (`int? a`, `int[] b`, `List<int> c`, `(int, int) e`), a lambda's `)` and `=>`, an explicit lambda return type and its `(`, a relational pattern's operator and operand, a parameter's name and `=`. |
| 2 | `LineBreakTriviaUtilities.CollapseTokenToSameLine`, called by `LineBreakListRewriter` (first element after `(`/`[`/`<`, each element after `,`, separators, `]`/`>`), `DeclarationSemicolonLineBreakRewriter` (`;`), `PropertyLayoutLineBreakRewriter` (arrow, first expression token), `AttributeTargetFormattingRewriter` (token after a list), `AccessorListLayout` (`{`, `}`, an accessor's `[`, modifiers, keyword, `;`) | No whitespace, with no lexer guard. | Every pair those joins produce. **Defect:** `protected` and `internal` on two lines of a collapsible accessor list give `protectedinternal set;` in the final output, which does not compile (change doc, entry 10). |
| 3 | `LineBreakListRewriter.CollapseOwnedOpenParenToDeclarationLine`, `CollapseOpenParenToDeclarationLine`, `CollapseSeparatorsToPreviousParameterLine`, `CollapseCloseParenToParameterLine` (FMT-BREAK-011) | No whitespace. | A declaration's name, `>`, `operator +`, `operator int`, `delegate`, `~C`, record/class/struct/interface name and `(`; the last token of a parameter and `,` or `)`. |
| 4 | `ChainLineBreakRewriter.CloseGap`/`CloseInnerGaps` (FMT-BREAK-019, FMT-BREAK-020) | No whitespace; only where the gap holds a line break. | A link's `.` and its name; an element and an attached `(`, `[`, `?[`, `!`; the tokens of a link operator (`?` and `.`); the root and the first link's operator. |
| 5 | `BinaryOperatorLineBreakRewriter.NormalizeBinaryOperatorPosition` (FMT-BREAK-017) | No whitespace between the moved operator and the right operand, with no lexer guard. | Every binary operator and its right operand. **Phase-level defect (FMT-PIPE-021):** `a -⏎-b` gives `--b`, `a /⏎*p` gives `/*p;` (a comment start), `o as⏎string` gives `asstring` in the phase's text; the final output is repaired by FMT-SPACE-010 only. Confirmed with `scripts/trace.sh`. |
| 6 | `TernaryLineBreakRewriter.MoveTokenOntoOperatorLine` (FMT-BREAK-018) | One space behind `?` and `:` when no whitespace is left. | `?`/`:` of a conditional and the first token of the branch behind it. |
| 7 | `SwitchCaseWhenLineBreakRewriter.CollapseWhenClauseOntoPatternLine` (FMT-BREAK-028) | Replaces the pattern's trailing trivia by exactly one space. | The last token of a `case` pattern (identifier, `)`, `}`, `]`, literal, keyword) and `when`. |
| 8 | `LineBreakAssignmentRewriter.NormalizeTrailingTriviaToSameLine` (FMT-BREAK-016) | One space behind the target's last token or `=` when none is left; none in front of the collection-initializer `{`. | Target and `=`, `=` and value, `=` and `[`, `=` and `{`. All covered by FMT-SPACE-010 and FMT-SPACE-009, so no final output depends on it. |
| 9 | `PropertyLayoutLineBreakRewriter.CollapseExpressionBody` (FMT-BREAK-026) | Exactly one space on each side of the arrow of an expression-bodied property or indexer, **also when nothing is joined**; removes the whitespace behind the token in front of the arrow and in front of the expression. | The member name or `]` and `=>`; `=>` and the expression. |
| 10 | `AccessorListLayout.CollapseAutoAccessorList` / `BuildAccessorTriviaReplacementMap` (FMT-BREAK-024) | One space in front of `{`, behind `{`, behind each accessor's `;`, and behind the token in front of each accessor keyword; clears the leading trivia of `}`, of each accessor's `[`, of each keyword, and the keyword's trailing trivia — **also when the list is already on one line**. | `P {`, `{ get`, `get;`, `; set`, `private set`, `] get`, `; }`. A one-line list with an attributed accessor is left as written (`{[A] get;set; }` today). |
| 11 | `AttributeTargetUtilities.MergeAttributeListGroup` (`Reihitsu.Core`), called by `AttributeTargetFormattingRewriter` (FMT-BREAK-027) and by `TargetAttributeListShapeCodeFixProviderBase` (RH5524-style merge fixes) | One space behind the merged list's `]` and behind a token left touching a joined token; clears the trivia of every merged attribute. | `]` and the declaration's next token (FMT-SPACE-003 decides 1 too); attribute and `,` (FMT-SPACE-004/008). No final output depends on it. |
| 12 | `LineBreakInitializerRewriter` | Removes leading line break and whitespace of an assigned collection initializer's `{` (a join of `=` and `{`, no whitespace); `CleanupTrailingWhitespaceBeforeToken` trims whitespace in front of a line break. | `=` and `{` (FMT-SPACE-010, FMT-SPACE-009 decide). The trim is FMT-BREAK-029's permitted case. |
| 13 | `TokenGapNormalizer`, `LineBreakContainedBlockRewriter` (comment moved above `if`) | Remove whitespace in front of a line break they write. | No same-line gap (FMT-BREAK-029). |

Other phases:

| # | Place | Same-line gap? |
|---|---|---|
| 14 | Structural transforms (FMT-STRUCT-026): `ExpressionBodyToBlockConverter` (space behind a created `return` only where the pair would fuse), `AccessorExpressionBodyTransform` and `GetOnlyMemberExpressionBodyTransform` (one space on each side of a created `=>`), `FieldDeclarationSplitTransform` (recorded exception, thoenissen/Reihitsu#1502) | Yes. The created-arrow spaces are dropped by this diff (FMT-SPACE-017 owns the gap); the rest is unchanged. |
| 15 | `TabReplacementPhase` | Replaces tabs in whitespace by four spaces before any other phase; the spacing phase collapses the run (FMT-TAB, FMT-SPACE-002). Not a spacing decision; unchanged. |
| 16 | `RegionNamingRewriter` | Writes the text of an `#endregion` directive; no gap between tokens (thoenissen/Reihitsu#1486). |
| 17 | `UsingDirectiveOrderingRewriter`, `IndentationRewriter`, `RawStringLineShifts`, `CleanupPhase`, `DocCommentElementNormalizer` | Line starts, line ends, or comment text only. |
| 18 | `SwitchCaseBraceRewriter`, blank-line rewriters | Write no spaces (searched). |

### Arrow constructs

Every `=>` token's parent is one of `SimpleLambdaExpressionSyntax`, `ParenthesizedLambdaExpressionSyntax` (also with attributes, modifiers, or an explicit return type), `ArrowExpressionClauseSyntax` (method, operator, conversion operator, constructor, destructor, local function, property, indexer, and the accessors `get`, `set`, `init`, `add`, `remove`), and `SwitchExpressionArmSyntax`. Today: property and indexer arrows are forced to one space each side (row 9); arrows the accessor and get-only-member transforms create get one space each side (row 14); method-like expression bodies are converted to blocks except where `RequiresExpressionBodyPreservation` keeps them, and keep the author's spacing then; lambdas, switch arms, and hand-written accessor arrows keep the author's spacing; a joined arrow inside a list element gets one space on the joined side.

### Attribute-target constructs

`AttributeTargetSpecifierSyntax` with a keyword target (`assembly`, `module`, `event`, `return`) or an identifier target (`field`, `method`, `param`, `property`, `type`, `typevar`, or any other identifier, which the parser accepts). Today only `return` gets no space in front of the colon (FMT-SPACE-012's keyword list); every target keeps the author's gap behind the colon, collapsed; a list-element join writes the colon tight to its target (`[param⏎: Obsolete]` → `[param: Obsolete]`). A target wrapped in a declaration's attribute list (`[return⏎:Obsolete]`) is not joined at all; see `Follow-up candidates`.

### Existing tests whose expected output depends on the changed gaps

Searched in every test project (formatter, analyzer including code-fix tests, CLI, Core, self-hosting sources) for tight or irregular arrows, attribute-target colons, conditional operators, name colons, `when`, types glued to names, relational patterns, and one-line accessor lists. Repository production code holds none of these forms, so the self-hosting scans are unaffected.

Final outputs that change under the recommended options:

- `StructuralTransformSeparatorFinalOutputTests.HandWrittenArrowKeepsItsGapsWhileAConvertedAccessorGetsSpacesAroundItsArrow` (FMT-STRUCT-026): `get=>"";` becomes `get => "";` (change doc, entry 2).
- `ListElementInteriorJoinTests.LambdaParameterListWrappedAfterStaticModifierKeepsItsSpace`: `N(static (int value) => value);` becomes `N(static(int value) => value);` (entry 9).

Phase-level expectations that change while the final output does not:

- `LineBreakRewriterTests` (runs the line-break phase alone): `CollapsesExpressionBodiedProperty` asserts `Value => 42;` (phase text becomes `Value=> 42;`); `CollapsesGetOnlyAutoPropertyToSingleLine`, `CollapsesGetSetAutoPropertyToSingleLine`, `CollapsesPropertyAttributedAutoPropertyToSingleLine`, and `CollapsesAutoPropertyWithJoinableWrappedSignature` assert `Prop { get; }` / `Prop { get; set; }` for a list the phase collapses (phase text becomes `Prop{get;}`-like). `HandlesPropertyAccessors` and `KeepsAutoPropertyAccessorListInlineWhenInitializerWraps` start from a spaced one-line list and keep passing.
- `StructuralTransformSeparatorTests.AccessorTransformWritesOneSpaceOnEachSideOfTheCreatedArrow`, `GetOnlyMemberTransformWritesOneSpaceOnEachSideOfTheCreatedArrow`, `GetOnlyMemberTransformWritesOneSpaceOnEachSideOfTheArrowBehindAnIndexer` (FMT-STRUCT-026, phase-level), and possibly `TokensCreatedForConvertedAccessorsCarryNoElasticTrivia`, which inspects the created arrow's trivia.

Tests that cover the join separators and keep their final output under the recommended options (they become coverage for FMT-BREAK-031 and the new FMT-SPACE rules): every other test of `ListElementInteriorJoinTests` (out/await/new/static modifiers, named argument and attribute argument values, tuple element names, invocation, `typeof`, cast, range, `::`, `^`, function-pointer types, attribute target `[param⏎: Obsolete]`, anonymous-method parameter list), `LineBreakRewriterTests.CollapsesParameterListClosingGapsForEveryDirectParentAndIsIdempotent`, the FMT-BREAK-011, -015, -016, -018, -020, -024, -026, -028 example tests, `AutoAccessorListLayoutTests` (its one-line attributed lists are written spaced), and `HorizontalSpacingRequirementTests` for FMT-SPACE-012's `[return : System.Obsolete]` example. No analyzer, code-fix, or CLI test expectation holds a form that changes.

Under option A of question 1, the tests that change instead are the tight-family tests of `ListElementInteriorJoinTests` (invocation, `typeof`, cast, range ×3, `::`, function-pointer type, attribute argument list `Obsolete⏎("x")`, anonymous method), `LineBreakRewriterTests.CollapsesParameterListClosingGapsForEveryDirectParentAndIsIdempotent`, and the example tests of FMT-BREAK-011 and FMT-BREAK-020, unless rules are added for those pairs.

## Changed requirements

- FMT-SPACE-002 — Whitespace no rule covers is only collapsed — changed (a joined gap is decided like a written one; precedence list gains the new rules)
- FMT-SPACE-012 — One space after statement keywords, `new`, and `operator` — changed (the attribute-target exception moves to FMT-SPACE-016)
- FMT-SPACE-016 — Attribute-target colons: no space before, one space after — added
- FMT-SPACE-017 — One space on each side of every arrow — added
- FMT-SPACE-018 — One space on each side of the conditional operator — added
- FMT-SPACE-019 — Name colons: no space before, one space after — added
- FMT-SPACE-020 — One space in front of `when` — added
- FMT-SPACE-021 — One space between a type and the name it declares — added
- FMT-SPACE-022 — One space inside a one-line accessor list — added
- FMT-SPACE-023 — One space behind a relational pattern's operator — added
- `horizontal-spacing.md`, fourth non-goal — narrowed to the pairs thoenissen/Reihitsu#1440 still holds
- FMT-BREAK-026 — Expression-bodied properties and indexers are on one line with their arrow — changed (the arrow is spaced by FMT-SPACE-017)
- FMT-BREAK-031 — A join writes the minimal separator, and the phase writes no other space — added
- FMT-STRUCT-026 — The phase writes only the separators its text needs — changed (no spaces around a created `=>`; Why reworded)
- Change doc: `sdd/changes/2026-11/spacing-sole-gap-owner.md`, `Status: Draft`

The spec diff is written on the recommended answer of every question below. Another answer changes FMT-SPACE-016 to -023, FMT-BREAK-031, FMT-STRUCT-026, and the change doc; the spec author amends them.

## Examples for the tests

FMT-SPACE-016, FMT-SPACE-017, FMT-SPACE-018, FMT-SPACE-019, FMT-SPACE-020, FMT-SPACE-021, FMT-SPACE-022, FMT-SPACE-023, FMT-BREAK-031 (phase-level text and final output; also the fusing pairs `protected`/`internal`, `-`/`-b`, `/`/`*p`, `as`/`string`, `?`/`.5`), FMT-BREAK-026, FMT-STRUCT-026 (created `=>` without whitespace in the phase's output), FMT-SPACE-002 (a joined pair without a rule keeps the minimal separator: change doc entry 9).

Each new FMT-SPACE rule needs its counter-side (FC-002/FC-003): the nullable `?` and `?.` next to FMT-SPACE-018, the `case` label colon and interpolation format colon next to FMT-SPACE-019, the space behind `when` next to FMT-SPACE-020, a member's return type next to FMT-SPACE-021, an initializer's braces next to FMT-SPACE-022, a binary `<` next to FMT-SPACE-023; and a comment in the gap (FMT-SPACE-013, FMT-SPACE-014, FC-019) for at least the arrow, the attribute-target colon, and the name colon.

## Output questions

The questions are numbered so that one answer such as `1B, 2B, 3B, 4B` covers them; 4 to 9 only arise with 1B. Inputs are verbatim; "today" is the output of `main`.

### 1 · Formatter — the separator of a join (FMT-BREAK-031, FMT-SPACE-002)

Why it is open: the issue leaves it to the change doc. Today the separator depends on the token kind, so a joined pair and the same pair on one line are spaced differently. Neither uniform separator keeps every output of today; each changes the pairs the other keeps.

Input:

```csharp
class C
{
    void M
        (int a)
    {
        var x = items.Where
            (y => y);

        N(Create
          (1), (int)
          a);
        N(name:
          "x");

        var z = a ?
                1 :
                2;
    }
}
```

Today:

```csharp
class C
{
    void M(int a)
    {
        var x = items.Where(y => y);

        N(Create(1), (int)a);
        N(name: "x");

        var z = a
                    ? 1
                    : 2;
    }
}
```

After, A — one space at every join:

```csharp
class C
{
    void M (int a)
    {
        var x = items.Where (y => y);

        N(Create (1), (int) a);
        N(name: "x");

        var z = a
                    ? 1
                    : 2;
    }
}
```

After, B (recommended) — no space unless the two tokens would read as one, with the rules of questions 4 to 9:

```csharp
class C
{
    void M(int a)
    {
        var x = items.Where(y => y);

        N(Create(1), (int)a);
        N(name: "x");

        var z = a
                    ? 1
                    : 2;
    }
}
```

After, B without the rules of questions 4 to 9: `N(name:"x");`, `? 1` becomes `?1`, `: 2` becomes `:2`, and the arrow follows question 2.

Under A, every other tight join also gets a space: `typeof (C)`, `a [0]`, `start ..end`, `global ::System`, `delegate * unmanaged [Cdecl]<int, void>`, `operator + (C a, C b)`, `record R (int A)`, `Obsolete ("x")`, `delegate (int x)`. Under B, the joins that read as one token get a space, so `protected⏎internal set;` gives `protected internal set;` (today `protectedinternal set;`, which does not compile; both A and B fix it).

Why B: it keeps every tight join of today, including the examples of FMT-BREAK-011 and FMT-BREAK-020, and the pairs it would space differently are ones the maintainer already decided outputs for (#958, #1440); A needs undecided rules for parameter lists, invocations, casts, ranges, and pointers.

### 2 · Formatter — every arrow `=>` (FMT-SPACE-017, FMT-BREAK-026, FMT-STRUCT-026)

Why it is open: the issue proposes keeping the author's spacing until thoenissen/Reihitsu#958; #958 and #1514 already decided one space for every arrow kind.

Input:

```csharp
class C
{
    int _p;

    int A
    {
        get =>_p;
        set=>_p = value;
    }

    int P =>1;

    void M(int x)
    {
        System.Func<int, int> f = y=>y;
        var s = x switch
                {
                    1=>2,
                    _ =>3
                };

        N(z =>
          z);
    }
}
```

Today: unchanged, except `int P =>1;` becomes `int P => 1;` and `N(z =>⏎z);` becomes `N(z => z);`.

After, A — the author's spacing, collapsed (issue): unchanged, except `N(z =>⏎z);` becomes `N(z => z);` under 1A and `N(z =>z);` under 1B; `int P =>1;` stays `int P =>1;`, and a joined `int T =>⏎5;` becomes `int T =>5;` under 1B.

After, B (recommended):

```csharp
class C
{
    int _p;

    int A
    {
        get => _p;
        set => _p = value;
    }

    int P => 1;

    void M(int x)
    {
        System.Func<int, int> f = y => y;
        var s = x switch
                {
                    1 => 2,
                    _ => 3
                };

        N(z => z);
    }
}
```

Why B: it is the output already decided for every arrow kind, it keeps `int P => 1;`, and it makes FMT-PIPE-014's "every arrow is spaced alike" true without a regression. It completes thoenissen/Reihitsu#958 and thoenissen/Reihitsu#1514 (scope: lead).

### 3 · Formatter — the attribute-target colon (FMT-SPACE-016)

Why it is open: the issue's target is `[field:NonSerialized]` (no space behind); thoenissen/Reihitsu#1383 records `[return: Obsolete]`, `[assembly: X]`, `[field: X]` "whatever the input spacing".

Input:

```csharp
using System;

[assembly:CLSCompliant(true)]

class C
{
    [field :NonSerialized]
    int _f;

    [return:Obsolete]
    int M()
    {
        return 1;
    }

    [method : Obsolete]
    void N()
    {
    }
}
```

Today: unchanged (`[return :Obsolete]` would become `[return:Obsolete]`).

After, A — no space before, the author's gap behind (issue):

```csharp
using System;

[assembly:CLSCompliant(true)]

class C
{
    [field:NonSerialized]
    int _f;

    [return:Obsolete]
    int M()
    {
        return 1;
    }

    [method: Obsolete]
    void N()
    {
    }
}
```

After, B (recommended) — no space before, one space behind:

```csharp
using System;

[assembly: CLSCompliant(true)]

class C
{
    [field: NonSerialized]
    int _f;

    [return: Obsolete]
    int M()
    {
        return 1;
    }

    [method: Obsolete]
    void N()
    {
    }
}
```

Why B: it is the form #1383 records and the examples of FMT-SPACE-012 and FMT-BREAK-027 show; under A the output still depends on how the author wrote the colon.

### 4 · Formatter — the conditional operator (FMT-SPACE-018; only with 1B)

Input:

```csharp
class C
{
    int M(bool b)
    {
        var r = b?1:2;
        var z = b ?
                1 :
                2;

        return z;
    }
}
```

Today: `var r = b?1:2;` unchanged; `z` becomes `b`, `? 1`, `: 2` on three lines.

After, A — no rule: `var r = b?1:2;` unchanged; `z` becomes `b`, `?1`, `:2` on three lines.

After, B (recommended): `var r = b ? 1 : 2;`; `z` becomes `b`, `? 1`, `: 2` on three lines.

Why B: one space on each side is decided (#1440), and A puts FMT-BREAK-018's own example out of shape.

### 5 · Formatter — name colons of arguments, tuple elements, subpatterns (FMT-SPACE-019; only with 1B)

Input:

```csharp
class C
{
    void M(bool b, int c, object o)
    {
        M(b:b, c :1, o: o);

        var t = (A:1, B :2);
        var p = o is string { Length:1 };

        N(name:
          "x");
    }
}
```

Today: unchanged, except `N(name:⏎"x");` becomes `N(name: "x");`.

After, A — no rule: unchanged, and `N(name:⏎"x");` becomes `N(name:"x");`.

After, B (recommended): `M(b: b, c: 1, o: o);`, `(A: 1, B: 2)`, `{ Length: 1 }`, `N(name: "x");`.

Why B: `b: b` is decided (#1440), and A puts FMT-BREAK-015's own example (`message: "x"`) out of shape. B includes subpattern names (`ExpressionColonSyntax`, `{ A.B: 1 }`) as the same family; #1440 names only named arguments.

### 6 · Formatter — the space in front of `when` (FMT-SPACE-020; only with 1B)

Input:

```csharp
class C
{
    void M(object o, bool b)
    {
        switch (o)
        {
            case (1, 2)
                when b:
                break;

            case "x"when b:
                break;
        }
    }
}
```

Today: `case (1, 2) when b:` and `case "x"when b:`.

After, A — no rule: `case (1, 2)when b:` and `case "x"when b:`.

After, B (recommended): `case (1, 2) when b:` and `case "x" when b:`.

Why B: it keeps today's joined output and spaces the guard alike whether joined or not; no output for this pair is decided anywhere yet. The space behind `when` (`when(x)`) stays undecided under both.

### 7 · Formatter — a type and the name it declares (FMT-SPACE-021; only with 1B)

Input:

```csharp
class C
{
    void M(int?
           a, int[]b)
    {
        int?z = null;
    }
}
```

Today: `void M(int? a, int[]b)` and `int?z = null;`.

After, A — no rule: `void M(int?a, int[]b)` and `int?z = null;`.

After, B (recommended): `void M(int? a, int[] b)` and `int? z = null;`.

Why B: A turns a joined `int?⏎a` into `int?a`; no output for this pair is decided anywhere yet. B covers parameters, tuple elements, variable declarations, declaration expressions, and declaration patterns, and leaves a member's return type unspecified.

### 8 · Formatter — a one-line accessor list (FMT-SPACE-022; needed with 1A and 1B)

Input:

```csharp
using System;

class C
{
    int P {get;set;}

    int Q {[Obsolete] get;set;}
}
```

Today: `int P { get; set; }` and `int Q {[Obsolete] get;set; }`.

After, A — no rule: `int P {get;set; }` and `int Q {[Obsolete] get;set; }`.

After, B (recommended): `int P { get; set; }` and `int Q { [Obsolete] get; set; }`.

Why B: it keeps today's output of every list the formatter collapses; only the attributed one-line list, which today is spaced on one side only, changes.

### 9 · Formatter — a relational pattern's operator (FMT-SPACE-023; only with 1B)

Input:

```csharp
class C
{
    bool M(int x)
    {
        return x is >5 and <10 || N(x is >
                                     1);
    }
}
```

Today: `x is >5 and <10` unchanged; the joined `>⏎1` becomes `> 1`.

After, A — no rule: unchanged, and the joined one becomes `>1`.

After, B (recommended): `x is > 5 and < 10` and `> 1`.

Why B: `x is > 5` is decided (#1440).

### 10 · Formatter — joined keyword and parenthesis or bracket (entry 9 of the change doc; only with 1B)

Input:

```csharp
class C
{
    unsafe void M(int a, object o)
    {
        N(static
          (int value) => value);
        N(in
          (a));
        N(o is var
          (x, y));
        N(stackalloc
          [] { 1 });
    }
}
```

Today: `N(static (int value) => value);`, `N(in (a));`, `N(o is var (x, y));`, `N(stackalloc [] { 1 });`.

After, A (recommended) — no rule; the joined pair keeps the minimal separator: `N(static(int value) => value);`, `N(in(a));`, `N(o is var(x, y));`, `N(stackalloc[] { 1 });`.

After, B — a rule "one space behind a modifier, `var`, or `stackalloc` in front of `(` or `[`": today's output, and the same pairs written on one line get the space too.

Why A: no output for these pairs is decided anywhere, the inputs are rare, and a rule would also change the one-line forms; an explicit lambda return type (`int⏎(x) => x` → `int(x) => x`) belongs to the same family.

## Internal choices

- The spec is written on the recommended answer of every output question; FMT-SPACE-016 to -023 are written now so the skeptic can attack them before the batch is asked. Alternative: write only the change doc and wait.
- The new rules are keyed by construct (`AttributeTargetSpecifierSyntax`, every `=>`, `ConditionalExpressionSyntax`, `NameColonSyntax`/`ExpressionColonSyntax`, `WhenClauseSyntax`, the listed declarations, `AccessorListSyntax`, `RelationalPatternSyntax`) and apply to every same-line gap, never only to joined gaps. Alternative: a join-only table in the spacing phase — rejected in the change doc.
- Precedence: FMT-SPACE-016 goes in front of the keyword rule (so `return` before `:` is decided by it), the other new rules after it. No pair is decided by two rules with different results (checked against FMT-SPACE-003 to -012 pair by pair).
- FMT-STRUCT-026's created-arrow spaces are retired in the same diff (closes the approved follow-up thoenissen/Reihitsu#1514 under 2B); the elastic-trivia rule stays and its Why no longer uses the arrow example.
- `AttributeTargetUtilities.MergeAttributeListGroup` (row 11) keeps its spaces: it lives in `Reihitsu.Core`, is shared with the merge code fixes, writes exactly what FMT-SPACE-003 and FMT-SPACE-008 decide, and its move into the structural transforms is thoenissen/Reihitsu#1492. FMT-BREAK-031's sentence "the phase MUST NOT write ... whitespace in any other gap" does not cover a gap that a merge creates by removing tokens. Alternative: strip its spaces in this run.
- The spec names no new code owner: the rule types do not exist yet. The implementer adds each new rule type to `horizontal-spacing.md`'s `Code owners` and to `sdd/specs/README.md`.
- The AGENTS.md phase-ownership rows of `LineBreakPhase` and `HorizontalSpacingPhase` name guards that change with the implementation; they are left for the implementer or cleaner to update with the final guard names.
- No rule page changes: no analyzer reports any of the affected pairs (RH6021 covers only base-list and constructor-initializer colons).
- Scope: under the recommended answers the run also completes thoenissen/Reihitsu#958 and thoenissen/Reihitsu#1514, the attribute-target part of thoenissen/Reihitsu#1383, and the conditional, named-colon, and relational-pattern parts of thoenissen/Reihitsu#1440. The lead decides the links.

## Follow-up candidates

- **A wrap inside a declaration's attribute list is kept** (FMT-BREAK-027 / FMT-PIPE-014; outside the run: line breaks, not spacing). Input `[return⏎    :Obsolete]` on a method, or `[return:⏎    Obsolete]`: today the line break stays, and the second line is re-indented to the member's column (`    :Obsolete]`). The same wrap inside a parameter's attribute list is joined (FMT-BREAK-015). Reachable by whole-document formatting. Readings: A — join it like a parameter's attribute (`[return: Obsolete]`); B — keep it. Recommendation: A.
- **Wrapped pattern parts lose their column** (FMT-INDENT; possibly covered by thoenissen/Reihitsu#1542). `N(o is not⏎(1 or 2));`, `N(o is C⏎(1));`, and `N(o is { X:⏎1 });` put the continuation at the statement's column (`        (1 or 2));`, `        1`) instead of under the pattern; `N(o is (1⏎or 2));` is spread over five lines with `1` and `or 2` at the statement's column. Reachable by whole-document formatting. Recommendation: check against #1542 before filing.
