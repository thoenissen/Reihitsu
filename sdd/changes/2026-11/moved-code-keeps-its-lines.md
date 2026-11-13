# Moved code keeps its lines

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | SHARED-FIX-007 (changed), SHARED-ORDER-009 (added), SHARED-ORDER-010 (added), RH7004-007 (added), RH7102-006, RH7103-007, RH7107-005, RH7108-005, RH7109-006 (changed: list the new refusals) |

## What

- **Columns of moved lines** (SHARED-FIX-007, *Moved code*). For a member or an accessor that a fix moves within its list (RH7102, RH7103, RH7107, RH7108, RH7109) and for the statements that the RH7004 fix wraps into a new block, every line of the moved code moves by the same number of columns, so its lines keep their columns relative to each other, continuation lines included. The number is the shift of the moved code's first token: for a member or an accessor, from its column to the column of its slot (none where both are indented alike); for RH7004, one indentation level. A line that starts with a comment in column 0 and the line of a directive other than `#region` and `#endregion` keep their column; a `#region` or `#endregion` line moves with the code. Until now the requirement only said that "the indentation of its lines follows its new nesting", which did not decide a line that does not start a statement or member.
- **Whole-line moves** (SHARED-ORDER-009). A member or accessor move is offered only when it moves whole lines: neither the moved declaration nor the target shares its line with another declaration or with a brace of the type or the accessor list. For an accessor move this is stated for an accessor list that spans lines.
- **Lines a shift cannot move, in a member or accessor move** (SHARED-ORDER-010). When the move changes the column of the moved declaration's first token, the fix is withheld if the declaration holds a line that starts inside a string literal or a comment that spans lines, or a line of disabled text.
- **Lines a shift cannot move, in RH7004** (RH7004-007). The fix is withheld when the moved statements hold a string literal of any kind or a comment that spans lines, or disabled text, or when the first moved statement starts on the line of the using declaration.

## Why

thoenissen/Reihitsu#969, thoenissen/Reihitsu#1335, and thoenissen/Reihitsu#1604 report that the member-ordering, accessor-ordering, and RH7004 fixes reformat the code they move and the code around it. That part is a bug against unchanged SHARED-FIX-006 and SHARED-FIX-007 and is not listed here. Once the fixes stop formatting, SHARED-FIX-007 has to say where a line of moved code that does not start a statement or member goes, and which moved code a plain shift of lines cannot carry. The maintainer answered both (run `fix-scope-moved-code`, batch 1): moved lines keep their written columns relative to each other, a move is always a move of whole lines, and moved statements with strings or comments that span lines are left to manual correction for now.

## Visible output changes

Output on `main` measured with `scripts/apply-fix.sh` on the run's branch before any code change, LF and CRLF alike; the outputs marked "issue run" were measured by the run's spec author and skeptic. The diagnostic stays in every case where no fix is offered.

### A continuation line of a moved member keeps its column (RH7109; the same for RH7102, RH7103)

```csharp
class C
{
    private int _a;
    private readonly int _b = 1
            + 2;
}
```

Before (issue run):

```csharp
class C
{
    private readonly int _b = 1
                              + 2;
    private int _a;
}
```

After:

```csharp
class C
{
    private readonly int _b = 1
            + 2;
    private int _a;
}
```

A member indented deeper than its slot moves all its lines by the same shift:

```csharp
class C
{
    private int _a;
        private readonly int _b = 1
            + 2;
}
```

Before:

```csharp
class C
{
    private readonly int _b = 1
                              + 2;
    private int _a;
}
```

After:

```csharp
class C
{
    private readonly int _b = 1
        + 2;
    private int _a;
}
```

### A continuation line of a moved statement moves one level with it (RH7004)

```csharp
using System.IO;

class C
{
    int M(int a, int b)
    {
        using var s = new MemoryStream();
        var total = a
            + b;
        return total;
    }
}
```

Before (issue run):

```csharp
using System.IO;

class C
{
    int M(int a, int b)
    {
        using (var s = new MemoryStream())
        {
            var total = a
                        + b;

            return total;
        }
    }
}
```

After:

```csharp
using System.IO;

class C
{
    int M(int a, int b)
    {
        using (var s = new MemoryStream())
        {
            var total = a
                + b;
            return total;
        }
    }
}
```

The blank line in front of `return total;` goes by unchanged SHARED-FIX-006; the column of `+ b` is this change.

### A comment in column 0 and directive lines among moved statements (RH7004)

```csharp
using System.IO;

class C
{
    void M()
    {
        using var s = new MemoryStream();
#pragma warning disable CS0219
        var unused = 1;
#pragma warning restore CS0219
//        s.Close();
        s.Flush();
    }
}
```

Before:

```csharp
using System.IO;

class C
{
    void M()
    {
        using (var s = new MemoryStream())
        {
#pragma warning disable CS0219
            var unused = 1;
#pragma warning restore CS0219

            //        s.Close();
            s.Flush();
        }
    }
}
```

After:

```csharp
using System.IO;

class C
{
    void M()
    {
        using (var s = new MemoryStream())
        {
#pragma warning disable CS0219
            var unused = 1;
#pragma warning restore CS0219
//        s.Close();
            s.Flush();
        }
    }
}
```

The inserted blank line goes by unchanged SHARED-FIX-006; the comment staying in column 0 is this change. A `#region` and `#endregion` line among the moved statements moves with them, as on `main`.

### A string or comment that spans lines among moved statements (RH7004)

```csharp
using System.IO;

class C
{
    string P()
    {
        using var s = new MemoryStream();
        var t = @"a
  b";
        return t;
    }
}
```

Before:

```csharp
using System.IO;

class C
{
    string P()
    {
        using (var s = new MemoryStream())
        {
            var t = @"a
  b";

            return t;
        }
    }
}
```

After: no fix offered.

```csharp
using System.IO;

class C
{
    void M()
    {
        using var s = new MemoryStream();
        /* one
           two */
        s.Flush();
    }
}
```

Before:

```csharp
using System.IO;

class C
{
    void M()
    {
        using (var s = new MemoryStream())
        {
            /* one
      two */
            s.Flush();
        }
    }
}
```

After: no fix offered.

A multi-line raw string literal is withheld alike (on `main` its lines are realigned under the opening quotes, issue run).

### Disabled text among moved statements (RH7004)

Without `NEVER` defined:

```csharp
using System.IO;

class C
{
    void M()
    {
        using var s = new MemoryStream();
#if NEVER
        s.Close();
#endif
        s.Flush();
    }
}
```

Before:

```csharp
using System.IO;

class C
{
    void M()
    {
        using (var s = new MemoryStream())
        {
#if NEVER
        s.Close();
#endif
            s.Flush();
        }
    }
}
```

After: no fix offered.

### A statement on the line of the using declaration (RH7004)

```csharp
using System.IO;

class C
{
    void M()
    {
        using var s = new MemoryStream(); s.Flush();
        s.Flush();
    }
}
```

Before:

```csharp
using System.IO;

class C
{
    void M()
    {
        using (var s = new MemoryStream())
        {
            s.Flush();
            s.Flush();
        }     }
}
```

After: no fix offered.

### A moved member shares its line (RH7109; the same for RH7102, RH7103)

```csharp
class C
{
    private int _a; private readonly int _b;
}
```

Before:

```csharp
class C
{
    private readonly int _b;
    private int _a;
}
```

After: no fix offered.

### A moved accessor shares its line in a list that spans lines (RH7107; the same for RH7108)

```csharp
class C
{
    public int A
    {
        set; get;
    }
}
```

Before:

```csharp
class C
{
    public int A { get; set; }
}
```

After: no fix offered.

### A shifted member holds a string over two lines (RH7109; the same for RH7102, RH7103)

```csharp
class C
{
    private int _a;
        private readonly string _b = @"x
y";
}
```

Before:

```csharp
class C
{
    private readonly string _b = @"x
y";
    private int _a;
}
```

After: no fix offered.

## Decisions

### Moved lines keep their columns relative to each other

Maintainer, batch 1, Q1 A and Q2 A. A member or accessor moves all its lines by the shift of its first token; RH7004 moves every line of the moved statements one level deeper. The formatter and the layout rules still correct a continuation line on their own; the fix does not decide it for them.

### A move is a move of whole lines

Maintainer, batch 1, addition to Q1: a move that goes beyond moving whole lines, such as a member sharing a line with another member or a token, is not offered.

### RH7004 is withheld for strings and comments that span lines

Maintainer, batch 1, Q3 C. Shifting the lines of a raw string with its closing quotes would also have been acceptable, only at no additional cost; it is not taken. A fix that moves such statements can follow as its own issue when it is proportionate.

### A comment in column 0 stays in column 0

Maintainer, batch 1, Q4 A. Such a comment is usually code commented out by the editor and belongs to no nesting depth; the RH3004 fix leaves it alike.

### Directives keep their column, regions move with the code

Lead, decision 5. A non-region directive is written in column 0 and belongs to no nesting depth; a `#region` or `#endregion` line is indented with the code it encloses. The check is the kind of the line's first trivia. `#pragma` and `#if` inside method bodies are common, so the fix is kept for them.

### Disabled text withholds RH7004

Lead, decision 4. Disabled text after a using declaration is rare, and its lines are code the compiler does not read; moving them would need a rule of their own.

### A statement on the declaration's line withholds RH7004

Spec author, local rare-class refusal. RH5103 reports a second statement on a line, and moving it would decide a line break the author did not write; `main` produces a broken layout for it.

### A shifted member or accessor with lines that cannot move is withheld

Spec author, local rare-class refusal (SHARED-ORDER-010). A shift other than zero happens only for a declaration indented differently from its slot, which the indentation rule reports. The lines that cannot move are the same classes RH7004-007 withholds, so both fixes refuse alike.

## Alternatives considered

- **Lines of moved code get the column the formatter gives them** (Q1 B, Q2 B) — rejected by the maintainer: it reformats code the fix only moves, which SHARED-FIX-007 forbids for the code's other layout.
- **A moved member that shares its line gets its own line** (today's output) — rejected by the maintainer: it decides a line break the author did not write.
- **Shift the lines of a multi-line raw string with the statement, or leave them where they are** (Q3 A, B) — rejected by the maintainer for now; a later change may add one.
- **A comment in column 0 moves with the statements** (Q4 B) — rejected by the maintainer.
- **Withhold RH7004 for every directive among the moved statements** — rejected: `#if DEBUG` and `#pragma` in method bodies are common code, and the per-line rule is a cheap check.
- **One shared "move lines by a column delta" helper for RH3004, RH7004, and the ordering fixes** (thoenissen/Reihitsu#1604) — not a behavior; the member and accessor moves need no shift in the common case, and RH3004's rules differ. Left to the implementation.
