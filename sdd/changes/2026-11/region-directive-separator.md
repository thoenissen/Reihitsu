# One space separates `#region` from its description

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-REGION-007 (added), FMT-REGION-006 (changed), FMT-TAB-002 (example changed) |

## What

The region phase writes exactly one space between `#region` and its description for every `#region` that has a description (FMT-REGION-007), not only when it rewrites the description to start with an upper-case letter (FMT-REGION-001). The rule applies inside a branch the build does not compile too, like the other region requirements (FMT-REGION-006). A `#region` without a description and its `#endregion` stay as FMT-REGION-004 states.

FMT-TAB-002's example follows: the four spaces a tab between `#region` and its description becomes in the preparation phase no longer reach the output.

`#endregion` needs nothing new where its `#region` has a description: FMT-REGION-002 already replaces everything behind the keyword with one space and `// <description>`.

## Why

Today the separator is normalized only as a side effect of the capitalization: `#region    foo` becomes `#region Foo`, while `#region    Bar` keeps its four spaces, because no phase owns the separator when the name is already capitalized. The spacing phase does not reach the text of a directive, and the tab replacement only turns a tab into four spaces. The output therefore depends on the first letter of the name, and a tab between `#region` and an upper-case name ends as four spaces.

The region phase already writes this separator whenever it rewrites the line; making that unconditional gives the separator one owner and one outcome.

## Visible output changes

The formatter's output changes for a `#region` with a description whose separator is anything but one space and whose description FMT-REGION-001 leaves unchanged. Measured on `main` at `f8dc7ef` with `scripts/trace.sh`; the change is in `RegionFormattingPhase` only.

### Several spaces between `#region` and an upper-case description

```csharp
class C
{
    #region    Bar
    int a;
    #endregion    Bar
}
```

Before:

```csharp
class C
{
    #region    Bar

    int a;

    #endregion // Bar
}
```

After:

```csharp
class C
{
    #region Bar

    int a;

    #endregion // Bar
}
```

### A tab between `#region` and an upper-case description

Not shown as an input block because the input contains a tab: `#region`, a tab, `Bar`, closed by a bare `#endregion`. The tab becomes four spaces in the preparation phase (FMT-TAB-002), which reach the output today.

Before:

```csharp
class C
{
    #region    Bar

    int a;

    #endregion // Bar
}
```

After:

```csharp
class C
{
    #region Bar

    int a;

    #endregion // Bar
}
```

A tab inside the description stays: `#region`, a tab, `Baz`, a tab, `qux` is written today as `#region`, four spaces, `Baz`, a tab, `qux`, and after the change as `#region`, one space, `Baz`, a tab, `qux`; its `#endregion` comment repeats `Baz`, a tab, `qux` in both.

### The same inside a branch the build does not compile

```csharp
class C
{
#if false
    #region    Bar
    int a;
    #endregion
#endif
}
```

Before:

```csharp
class C
{
#if false
    #region    Bar
    int a;
    #endregion // Bar
#endif
}
```

After:

```csharp
class C
{
#if false
    #region Bar
    int a;
    #endregion // Bar
#endif
}
```

### Unchanged

- A `#region` whose description starts with a lower-case letter: already written with one space (FMT-REGION-001).
- A `#region` without a description and its `#endregion` (FMT-REGION-004).
- Every `#endregion` whose `#region` has a description: already canonical (FMT-REGION-002).
- Diagnostics and rule pages: no analyzer rule reports the separator; RH7302 does not report `#region  Fields` (RH7302-002) and is not changed.
- CLI options, exit codes, and messages.
- Code fixes: a code fix that hands a node containing a `#region` and its `#endregion` to the formatter runs the region phase on it and would write the one space as well. No shipped fix was found that reaches such a node: the RH7102 fix withholds itself for a type with a region, and RH5204 and RH5417 did not report the probe inputs.

## Decisions

### The region phase owns the separator

The separator sits between the tokens of a region directive, which only the tab replacement and the region phase reach. The region phase already writes it on every rewrite and already reads the description trimmed, so the rule is completed where it is half-stated today instead of being given to a second component.

### FMT-REGION-001 keeps its sentence about the rewritten line

FMT-REGION-001 still states how a rewritten `#region` line is written: `#region`, one space, and the trimmed description. FMT-REGION-007 states the separator for every `#region` with a description; the two agree on every line FMT-REGION-001 rewrites.

### Disabled branches are included

FMT-REGION-006 applies every naming requirement to region directives in a branch the build does not compile; leaving the separator out would make a region line read differently depending on the active symbols.

## Alternatives considered

- **Keep the separator unless the name is rewritten, and state it in FMT-REGION** — rejected: it writes down an outcome that depends on the first letter of the name, and a tab separator would keep reaching the output as four spaces.
- **Let the horizontal spacing phase own the separator** — rejected: that phase spaces tokens of the code, not the text of directives, and the region phase already writes the separator on rewrite; a second owner would state the rule twice.
- **Report the separator with an analyzer rule** — not part of this change: the formatter may do more than the analyzers (P11), and a new rule is a new diagnostic.
