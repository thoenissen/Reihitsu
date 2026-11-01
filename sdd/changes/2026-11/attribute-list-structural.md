# Attribute lists are split and merged by the structural transforms

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-027 (changed), FMT-BREAK-031 (changed), FMT-BREAK-032 (added), FMT-STRUCT-028 (added), FMT-STRUCT-029 (added), the introduction and non-goals of `structural-transforms.md` (extended), SHARED-ATTR-013 (reference changed) |

## What

The shape of an attribute list — how many attributes it holds, and whether lists of one target are merged — is decided by the structural-transform phase for every owner whose shape does not depend on layout. The line-break phase keeps only the placement of the lists, and the shape of accessor attribute lists, which follows the layout the line-break phase itself gives the accessor list.

- **The split moves.** An attribute list with several attributes on the compilation unit, a type, a delegate, a member, an enum member, or a local function is split into one list per attribute by a structural transform (FMT-STRUCT-028), with the guards FMT-BREAK-027 states today: no split for a list with a comment or directive between its brackets, and none for a list whose specifier names no known target. The transform writes nothing between the lists it creates (FMT-STRUCT-026); the line-break phase puts each of them on its own line (FMT-BREAK-027).
- **The merge moves.** The lists of a parameter or type parameter that name the same target are merged into the first of them by a structural transform (FMT-STRUCT-029), with today's guards: lists of different targets are never merged, and a comment or directive in or around the group blocks the merge. The transform writes no whitespace (FMT-STRUCT-026); the spacing phase spaces the merged list (FMT-SPACE-003, FMT-SPACE-008).
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

None — no output an end user sees changes. The final output of every input is the output of `main`, FMT-BREAK-027's example included; only the phase that decides the shape changes, and the structural phase's intermediate tree no longer holds the line break the split writes today between the lists it creates.

## Decisions

### FMT-BREAK-027 is split into shape and placement

The shape becomes FMT-STRUCT-028 (split on declarations) and FMT-STRUCT-029 (merge on parameters and type parameters), each with its guards. FMT-BREAK-027 keeps its ID and title and states the placement only, so the analyzer specs and SHARED-ATTR that cite it for placement keep their links. SHARED-ATTR-013 cited it for the one-attribute-per-line shape and now cites FMT-STRUCT-028.

### The accessor shape stays in the line-break phase

See *Why*. FMT-BREAK-032 states it, and FMT-BREAK-031 keeps its recorded exception for that merge. The rewriter that applies it is then the one rewriter of the line-break phase that changes tokens, for accessor attribute lists only.

### The transforms follow FMT-STRUCT-026 without an exception

The split writes no line break between the lists it creates, and the merge writes no space behind the merged list or between the attributes; the created brackets and commas carry no elastic trivia. Unlike the field split, neither transform is run by a code fix without the phases after it, so no exception is needed. The attribute-list code fixes keep their own output (SHARED-ATTR-017, SHARED-ATTR-018).

### Every entry point applies the transforms

FMT-STRUCT-016 applies every requirement of the phase through every entry point except the accessor and get-only-member shapes. The split and merge are added to neither exception and to no configurable transform, so the node-level entry points keep reshaping attribute lists, as the line-break phase does today.

### Code owners and phase ownership

The transform types the implementation creates become code owners of FMT-STRUCT. `AttributeTargetFormattingRewriter` and `AttributeTargetFormattingShared` stay code owners of FMT-BREAK, since they keep the placement and the accessor shape. The `AGENTS.md` ownership rows change with them: the structural-transform row gains the attribute-list split and merge, and the line-break row keeps attribute-list placement and the accessor list shape.

## Alternatives considered

- **Keep the rule in FMT-BREAK and state there that this one line-break rule changes tokens** — rejected: the shape of declaration and parameter lists depends on no layout, so nothing keeps it in the phase that decides layout, and it stays a special case for every check that maps tokens across the phase.
- **Move the accessor shape as well, deciding it on the layout as written** — rejected: it changes the output of the inputs under *Why* and is not stable from one run to the next.
- **Move the accessor shape as well, predicting the line-break phase's accessor-list layout** — rejected: the structural phase would restate the joins and expansions of the line-break phase, a prediction that drifts when either changes.
