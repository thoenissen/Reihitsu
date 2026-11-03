# One form for region directive lines, and code fixes keep region directives

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-REGION-007 (added), FMT-REGION-001 (changed), FMT-REGION-004 (changed), FMT-PIPE-028 (added), FMT-PIPE-024 (reference added), FMT-TAB-002 (example changed) |

## What

- **One form for a `#region` line with a description.** The region phase writes every `#region` that has a description as `#region`, one space, and the trimmed description (FMT-REGION-007). Several spaces or a tab between the keyword and the description become one space, and whitespace behind the description is removed. This holds whether or not the description is capitalized (FMT-REGION-001), and also inside a branch the build does not compile. FMT-REGION-001 used to state this form only for a line it rewrites; it now points to FMT-REGION-007.
- **One space in front of the text of an `#endregion` whose `#region` has no description.** The text itself stays as written. A bare `#endregion` stays bare (FMT-REGION-004). Every other `#endregion` already gets one form from FMT-REGION-002.
- **Code fixes keep region directives.** The node-level entry points no longer apply any region requirement (FMT-PIPE-028). A code fix that hands the formatter a node holding region directives leaves their text as it was: the separator, the capitalization, and the `#endregion` comment. Region directives are normalized only when a whole document or syntax tree is formatted.
- FMT-TAB-002's example follows: the four spaces that a tab between `#region` and its description becomes in the preparation phase no longer reach the output.

## Why

On `main`, the separator is normalized only as a side effect of capitalizing the description. `#region    foo` becomes `#region Foo`, but `#region    Bar` keeps its four spaces, because no phase owns the separator when the name is already capitalized. The spacing phase does not reach the text of a directive, and the tab replacement only turns a tab into four spaces. The output therefore depends on the first letter of the name. Whitespace behind the description has the same dependence: `#region    bar···` loses it, `#region Bar···` keeps it.

Code fixes reach the region phase through the node-level entry points. On `main`, a fix for an unrelated diagnostic, such as RH7105, also capitalizes a region name and writes the `#endregion` comment inside the node it formats. Adding the new line form would have widened that further. SHARED-FIX-006 already demands that a fix change only the code its diagnostic is about.

## Visible output changes

Formatter output was measured on `main` at `f8dc7ef` with `scripts/trace.sh`. Code-fix output was measured with `scripts/apply-fix.sh` on `9c9ea5c`, which has no production change since `f8dc7ef`. `···` stands for three spaces at the end of a line.

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

This case is not shown as an input block, because the input contains a tab: `#region`, a tab, `Bar`, closed by a bare `#endregion`. The preparation phase turns the tab into four spaces (FMT-TAB-002), and those four spaces reach the output today.

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

A tab inside the description stays. `#region`, a tab, `Baz`, a tab, `qux` is written today as `#region`, four spaces, `Baz`, a tab, `qux`. After the change it is written as `#region`, one space, `Baz`, a tab, `qux`. Its `#endregion` comment repeats `Baz`, a tab, `qux` in both cases.

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

### Whitespace behind a `#region` description

```csharp
class C
{
    #region Baz···

    int b;

    #endregion // Baz
}
```

Before: unchanged.

After:

```csharp
class C
{
    #region Baz

    int b;

    #endregion // Baz
}
```

### The `#endregion` of a region without a description

```csharp
class C
{
    #region

    int a;

    #endregion    // Stale
}
```

Before: unchanged.

After:

```csharp
class C
{
    #region

    int a;

    #endregion // Stale
}
```

`#endregion//Stale` changes in the same way: before, it is unchanged; after, it is written as `#endregion //Stale`.

### Code fixes stop rewriting region directives

This change covers every code fix that hands the formatter a node holding a `#region` and its `#endregion`. RH7105 · Declaration keywords must follow order:

```csharp
namespace N
{
    static public class C
    {
        #region    bar

        private static int _a;

        #endregion
    }
}
```

Before:

```csharp
namespace N
{
    public static class C
    {
        #region Bar

        private static int _a;

        #endregion // Bar
    }
}
```

After:

```csharp
namespace N
{
    public static class C
    {
        #region    bar

        private static int _a;

        #endregion
    }
}
```

The same holds for the other type-level fixes that delegate to the formatter, such as RH7104, RH7106, RH8305, RH8307, and RH8309. It also holds for member-level fixes whose formatted member holds a region pair. The region rules report what the fix leaves behind, as they do for any other code: RH7302 for `bar`, and RH7301 for the `#endregion`. The region-rule fixes themselves (RH5031, RH5032, RH7301 to RH7310) do not call the formatter's pipeline, so they are not affected.

### Unchanged

- A `#region` with a lower-case description and one space, and nothing behind it: written as before (FMT-REGION-001).
- A `#region` without a description, and a bare `#endregion` (FMT-REGION-004).
- Every `#endregion` whose `#region` has a description (FMT-REGION-002).
- Diagnostics and rule pages: no rule reports the separator, and RH7302-002 keeps `#region  Fields` unreported.
- CLI options, exit codes, and messages.

## Decisions

### Maintainer answers

1. Several spaces in front of an upper-case description become one space.
2. A tab separator ends as one space. The tab phase replaces the tab with four spaces (FMT-TAB-001, FMT-TAB-002) and does not decide the separator; the region phase writes one space.
3. The same applies inside a branch the build does not compile.
4. The `#endregion` of a region without a description gets one space in front of its text, also where no whitespace stood (`#endregion//Stale` becomes `#endregion //Stale`).
5. Every `#region` line with a description is written as `#region`, one space, and the trimmed description. Whitespace behind the description is removed as well, also on a line whose separator is already one space.
6. A code fix changes only what its diagnostic is about (SHARED-FIX-006). A fix that has nothing to do with regions must not adjust them. Otherwise a user who does not use the region rules would see their region layout changed by an unrelated fix.

### The region phase owns the line form

The separator lies between the tokens of a region directive. Only the tab replacement and the region phase reach that text. The region phase already writes the separator whenever it rewrites the description, and it already reads the description trimmed. So the line form stays in the region phase, stated once in FMT-REGION-007. FMT-REGION-001 points to it, so the form has one statement.

### The pairing scope

FMT-REGION-007 applies to the `#region` directives the phase pairs with an `#endregion` (FMT-REGION-003). In a document or syntax tree, every region directive has a partner. A file with a directive that has no partner is a syntax error and is left unchanged (FMT-PIPE-005); this was verified for a directive inside an `#if false` branch too.

### The whole region rewrite is excluded from the node-level entry points

On `main`, fixes unrelated to regions already capitalize region names and write `#endregion // Name` through the node-level entry points. This is a pre-existing departure from SHARED-FIX-006 in the mechanism this change touches. Excluding only the new separator would add a node-level exception for one part of the rule and keep that departure. Excluding the whole rewrite gives one boundary instead: region directives are rewritten when a whole document is formatted, and never by a fix. FMT-PIPE-028 states this next to FMT-PIPE-011, which keeps accessor shapes for the same reason. FMT-PIPE-024 counts the rewrite among the things the node-level entry points skip.

## Alternatives considered

- **Keep the separator unless the name is rewritten, and state that in FMT-REGION**: rejected. It writes down an outcome that depends on the first letter of the name, and a tab separator would keep reaching the output as four spaces.
- **Normalize only the separator, and keep whitespace behind the description as written**: rejected by maintainer answer 5.
- **Let the horizontal spacing phase own the separator**: rejected. That phase spaces the tokens of the code, not the text of directives, and a second owner would state the rule twice.
- **Let code fixes write the new line form, as they already capitalize names**: rejected by maintainer answer 6 and SHARED-FIX-006.
- **Report the separator with an analyzer rule**: not part of this change. The formatter may do more than the analyzers (P11), and a new rule would be a new diagnostic.
