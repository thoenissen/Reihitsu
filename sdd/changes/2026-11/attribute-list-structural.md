# Attribute lists are split and merged by the structural transforms

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-027 (changed), FMT-BREAK-031 (changed), FMT-BREAK-032 (added), FMT-STRUCT-028 (added), FMT-STRUCT-029 (added), the introduction and non-goals of `structural-transforms.md` (extended), SHARED-ATTR-013 (reference changed) |

## What

The shape of an attribute list — how many attributes it holds, and whether lists of one target are merged — is decided by the structural-transform phase for every owner whose shape does not depend on layout. The line-break phase keeps only the placement of the lists, and the shape of accessor attribute lists, which follows the layout the line-break phase itself gives the accessor list.

- **The split moves.** An attribute list with several attributes on the compilation unit, a type, a delegate, a member, an enum member, or a local function is split into one list per attribute by a structural transform (FMT-STRUCT-028), with the guards FMT-BREAK-027 states today: no split for a list with a comment or directive between its brackets, and none for a list whose specifier names no known target. The transform writes nothing between the lists it creates (FMT-STRUCT-026); the line-break phase puts each of them on its own line (FMT-BREAK-027).
- **The split drops what separated two attributes.** The whitespace, line breaks, and blank lines between two attributes of the original list are dropped with the comma, so no created list keeps a line break or the next line's indentation between its brackets (FMT-STRUCT-028). Today the split keeps them whenever they belong to an attribute rather than to the comma; see *Visible output changes*.
- **The lambda split moves with it.** A lambda's attribute lists are split today as a declaration's are. They are split by the same structural transform, with the same guards and the same content of each created list, so their output stays the output of `main` apart from the changes listed below. FMT-STRUCT-028 still leaves them unspecified, because where they end up looks wrong (a follow-up of this change); the move only keeps the line-break phase from changing tokens for them.
- **The merge moves.** The lists of a parameter or type parameter that name the same target are merged into the first of them by a structural transform (FMT-STRUCT-029), with today's guards: lists of different targets are never merged, and a comment or directive in or around the group blocks the merge. The transform writes no whitespace (FMT-STRUCT-026); the spacing phase spaces the merged list (FMT-SPACE-003, FMT-SPACE-008).
- **The merge keeps a line break the author wrote between the lists.** Today the line-break phase reads the parameter's lists before it merges them, so a line break between or behind them wraps the parameter list (FMT-BREAK-013). The structural merge runs before that phase reads anything, so it keeps one line break of the stretch it rewrites, and the parameter list is wrapped as today (FMT-STRUCT-029).
- **One split and one merge.** The structural split and the accessor split of FMT-BREAK-032 are one split. The structural merge, the accessor merge of FMT-BREAK-032, and the merge of the list-shape code fix (SHARED-ATTR-018) are one merge. Their callers differ only in the separators they write: none for the structural transforms (FMT-STRUCT-026), the space and joins of SHARED-ATTR-018 for the code fix and the accessor merge (FMT-BREAK-031).
- **The accessor shape stays.** The attribute lists of an accessor are split when the line-break phase lays out its accessor list over several lines, and merged when the accessor list stays on the declaration's line (FMT-BREAK-032). This was the line-break phase's behavior already; FMT-BREAK-027 did not state the one-line case, and FMT-BREAK-032 writes it down.
- **FMT-BREAK-027 becomes a placement requirement**: each list of a declaration starts its own line, the lists of a parameter or type parameter stay on its line, and the lists of an accessor in an accessor list on one line stay on that line (FMT-BREAK-024).
- **The recorded exception of FMT-BREAK-031** now covers only the merge of accessor lists, the one merge left in the line-break phase.

The transforms run in every entry point and are not configurable: FMT-STRUCT-016 already applies every requirement of the phase other than the accessor and get-only-member shapes through every entry point, and FMT-PIPE-011 is unchanged. That is where the line-break phase applies them today.

## Why

A split or a merge changes the token sequence, and syntax-shape rewrites belong to the structural transforms, which run before any layout is decided (FMT-PIPE-001). The line-break phase is otherwise a phase that only adds and removes line breaks; with the split and merge in it, every check that maps tokens across that phase has to special-case this rule.

The accessor lists are the exception because their shape is decided by layout the line-break phase produces. Today, the lists of an accessor are merged when the property stands on one line at the point the attribute rewriter runs, after the line-break phase has joined an initializer's value onto the declaration line and laid out the accessor list. Measured on `main` at `43ce9cd`:

```csharp
class C
{
    int P { [A, B] get => 1; }

    int S { [A] [B] get; set; } =
        1;
}
```

```csharp
class C
{
    int P
    {
        [A]
        [B]
        get => 1;
    }

    int S { [A, B] get; set; } = 1;
}
```

`P` is written on one line but split, because the phase lays out its accessor list over several lines (FMT-BREAK-025); `S` is written over two lines but merged, because the phase joins the initializer's value (FMT-BREAK-016). A structural transform sees the layout as written, so it would decide both the other way. Deciding on the layout as written would not be stable either: the next run sees the layout the line-break phase gave the property and decides the other way again. Deciding the accessor shape in the structural phase from the layout the line-break phase will produce would mean predicting that phase's joins and expansions, a second statement of its rules that can drift from it. So the accessor shape stays where that layout is decided.

## Visible output changes

Two outputs of the formatter change, both for an attribute list written over several lines and split into one list per attribute. They change for every owner the split serves: a declaration (FMT-STRUCT-028), an accessor in an accessor list laid out over several lines (FMT-BREAK-032), and a lambda. The output of every other input is the output of `main`, FMT-BREAK-027's example and every parameter and type-parameter merge included. The code fixes' output does not change.

### A line break behind the comma settles in one pass

Demanded by FMT-PIPE-004 (idempotency) and FMT-SPACE-004; the output after a second pass is unchanged.

```csharp
class C
{
    [A,
     B] void M() { }

    int G
    {
        [C,
         D] get;
        set;
    }
}
```

Before (`main`), first pass — the second pass gives the output below:

```csharp
class C
{
    [A]
    [     B]
    void M()
    {
    }

    int G
    {
        [C]
        [         D]
        get;
        set;
    }
}
```

After, in one pass:

```csharp
class C
{
    [A]
    [B]
    void M()
    {
    }

    int G
    {
        [C]
        [D]
        get;
        set;
    }
}
```

### A line break in front of the comma, or a blank line behind it, is dropped

Answered by the maintainer (reading A).

```csharp
class C
{
    [A
     , B] void N() { }

    [A,

     B] void O() { }

    int G
    {
        [C
         , D] get => 1;
    }
}
```

Before (`main`):

```csharp
class C
{
    [A
     ]
    [B]
    void N()
    {
    }

    [A]
    [
    B]
    void O()
    {
    }

    int G
    {
        [C
         ]
        [D]
        get => 1;
    }
}
```

After:

```csharp
class C
{
    [A]
    [B]
    void N()
    {
    }

    [A]
    [B]
    void O()
    {
    }

    int G
    {
        [C]
        [D]
        get => 1;
    }
}
```

A line break behind the original `[` or in front of the original `]` (`[A, B⏎]`) does not separate two attributes and keeps today's output; it is outside this change.

## Decisions

### FMT-BREAK-027 is split into shape and placement

The shape becomes FMT-STRUCT-028 (split on declarations) and FMT-STRUCT-029 (merge on parameters and type parameters), each with its guards. FMT-BREAK-027 keeps its ID and title and states the placement only, so the analyzer specs and SHARED-ATTR that cite it for placement keep their links. SHARED-ATTR-013 cited it for the one-attribute-per-line shape and now cites FMT-STRUCT-028.

### The accessor shape stays in the line-break phase

See *Why*. FMT-BREAK-032 states it, and FMT-BREAK-031 keeps its recorded exception for that merge. The rewriter that applies it is then the one rewriter of the line-break phase that changes tokens, for accessor attribute lists only.

### The transforms follow FMT-STRUCT-026 without an exception

The split writes no line break between the lists it creates, and the merge writes no space behind the merged list or between the attributes; the created brackets and commas carry no elastic trivia. Unlike the field split, neither transform is run by a code fix without the phases after it, so no exception is needed. The line break the merge keeps is one the author wrote, carried over, not one it writes. The attribute-list code fixes keep their own output (SHARED-ATTR-017, SHARED-ATTR-018).

### A created list holds only its attribute between the separators it drops

The split drops the comma together with everything between the two attributes it separated (FMT-STRUCT-028). Today's split carries an attribute's own leading and trailing trivia into its new brackets. When the separating line break belongs to an attribute rather than to the comma, a created list keeps a line break inside its brackets, or the next line's indentation behind its `[`; the spacing phase does not reach that whitespace, because the attribute holds it as leading trivia, so a second pass changes the output (`[A,⏎ B]`). Both outputs under *Visible output changes* follow from the one rule; the second was asked and answered as an output question, the first is demanded by unchanged requirements.

### The merge keeps one line break of its stretch

Today the line-break phase reads a parameter's lists as written before it merges them, so `void P([A]⏎ [B] int x, int y)` wraps the parameter list like `void P([A]⏎ int x, int y)`, whose list has nothing to merge. After the move, the merge runs before that phase reads the parameter. A merge that dropped the line break, as the code fix's does, would leave the first input on one line and change its output. FMT-STRUCT-029 therefore keeps one line break wherever the author wrote one in the stretch it rewrites, and the line-break phase sees the parameter as today; the attribute lists are then joined onto the parameter's line by FMT-BREAK-027, as today. The accessor merge keeps none, since it runs only for an accessor list that stays on one line.

### The lambda split moves with the declarations

FMT-STRUCT-028 leaves the attribute lists of a lambda unspecified, because today's output places them on lines of their own inside an expression, which looks wrong and is a follow-up of this change. Leaving their split in the line-break phase would keep a token-changing rewrite there for lambdas only. The structural split splits them as it does a declaration's lists, which keeps today's output apart from the two changes above, and the follow-up decides their shape and placement.

### One split and one merge

Moving the split and the merge without changing their structure would leave two splits — the structural one and the accessor split of FMT-BREAK-032 — and two merges — the structural one and the shared merge the code fixes and the accessor merge use. Each pair shares the decision of which lists are split or merged and what happens to the trivia they rewrite, and the first output change above already showed in both copies of the split. So the run keeps one split and one merge. Their callers differ only in the separators they write: the structural transforms none (FMT-STRUCT-026), keeping the line break of FMT-STRUCT-029; the code fix the space and joins of SHARED-ATTR-018; the accessor merge the same as the code fix, which is the recorded exception of FMT-BREAK-031. The code fixes' output does not change (SHARED-ATTR-017, SHARED-ATTR-018).

### Every entry point applies the transforms

FMT-STRUCT-016 applies every requirement of the phase through every entry point except the accessor and get-only-member shapes. The split and merge are added to neither exception and to no configurable transform, so the node-level entry points keep reshaping attribute lists, as the line-break phase does today.

### Code owners and phase ownership

The transform types the implementation creates become code owners of FMT-STRUCT. `AttributeTargetFormattingRewriter` and `AttributeTargetFormattingShared` stay code owners of FMT-BREAK, since they keep the placement and the accessor shape. The `AGENTS.md` ownership rows change with them: the structural-transform row gains the attribute-list split and merge, and the line-break row keeps attribute-list placement and the accessor list shape.

## Alternatives considered

- **Keep the rule in FMT-BREAK and state there that this one line-break rule changes tokens** — rejected: the shape of declaration and parameter lists depends on no layout, so nothing keeps it in the phase that decides layout, and it stays a special case for every check that maps tokens across the phase.
- **Move the accessor shape as well, deciding it on the layout as written** — rejected: it changes the output of the inputs under *Why* and is not stable from one run to the next.
- **Move the accessor shape as well, predicting the line-break phase's accessor-list layout** — rejected: the structural phase would restate the joins and expansions of the line-break phase, a prediction that drifts when either changes.
- **Keep each attribute's own trivia in its created list, as today** — rejected: `[A,⏎ B]` needs two passes to settle (FMT-PIPE-004), and the maintainer chose to drop the line breaks of `[A⏎ , B]` and `[A,⏎⏎ B]` as well.
- **Let the structural merge drop the line breaks of its stretch, as the code fix's merge does** — rejected: it changes the output of every parameter list wrapped only by a line break between a parameter's lists, which then formats differently from the same parameter with one list.
- **Leave the lambda split in the line-break phase** — rejected: it keeps a token-changing rewrite in that phase for a case whose shape is still open; the follow-up decides it.
- **Move the split and merge as further copies next to the accessor split and the shared merge** — rejected: two owners of one decision, which already disagree on the first output change above.
