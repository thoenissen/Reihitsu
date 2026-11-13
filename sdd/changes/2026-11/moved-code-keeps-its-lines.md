# Moved code keeps its lines

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | SHARED-FIX-007 (changed), SHARED-ORDER-009 (added), SHARED-ORDER-010 (added), RH7004-007 (added), SHARED-DOC-004 (changed: a member or accessor move does not carry a `///` comment behind code), RH3004-006 (clarified: its line rule is SHARED-FIX-007's; changed: a tab in front of a moving line withholds the block-body fix), RH7102-006, RH7103-007, RH7107-005, RH7108-005, RH7109-006 (changed: list the new refusals), RH7107-004, RH7108-004 (changed: the swap within a one-line accessor list) |

## What

- **Columns of moved lines** (SHARED-FIX-007, *Moved code*). For a member or an accessor that a fix moves within its list (RH7102, RH7103, RH7107, RH7108, RH7109) and for the statements that the RH7004 fix wraps into a new block, every line of the moved code moves by the same number of columns, so its lines keep their columns relative to each other, continuation lines included. The number is the shift of the moved code's first token: for a member or an accessor, from its column to the column of its slot (none where both are indented alike); for RH7004, one indentation level. A comment line in column 0 — a line that holds nothing but comments and whitespace, its first comment in column 0 — and the line of a directive other than `#region` and `#endregion` keep their column; a line that holds code moves, also behind a comment in column 0 such as `/* x */ Foo();`, and so does a `#region` or `#endregion` line. This is the line rule the RH3004 fix already applies. A tab at the start of a moved line counts as one column. Until now the requirement only said that "the indentation of its lines follows its new nesting", which did not decide a line that does not start a statement or member.
- **Whole-line moves** (SHARED-ORDER-009). A member or accessor move is offered only when it moves whole lines: neither the moved declaration nor the target shares its line with another declaration or with a brace of the type or the accessor list, and the leading trivia of neither starts behind code, as a `///` comment written behind code does when it documents the declaration below it. Crossed declarations that share a line with each other keep that line and move as one block. For an accessor move this is stated for an accessor list that spans lines; when the whole accessor list stands on one line, the accessors swap places within that line, and the list stays on one line.
- **Lines a shift cannot move, in a member or accessor move** (SHARED-ORDER-010). When the move changes the column of the moved declaration's first token, the fix is withheld if the declaration holds a line that starts inside a string literal or a comment that spans lines, or a line of disabled text, or, for a shift to the left, a moving line written further left than the shift reaches.
- **Lines a shift cannot move, in RH7004** (RH7004-007). The fix is withheld when the moved statements hold a string literal of any kind or a comment that spans lines, or disabled text, or when a line the fix changes is shared with code it does not move: the first moved statement starts on the line of the using declaration, code stands in front of the declaration on its line, or the last moved statement ends on the line of the block's closing brace. A comment or directive in front of the declaration's semicolon withholds it as on `main`.

- **Blank lines joined by a removed directive line** (SHARED-FIX-007, *Removed directive line*; RH7303, RH7310). Where a removal joins blank lines in front of a removed line with blank lines behind it, the blank lines in front stay as written and those behind it go. This only states which of the blank lines is kept; the number of blank lines left follows the bullet as before.

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

### An accessor list on one line stays on one line (RH7107; the same for RH7108)

```csharp
class C
{
    private int _b;

    public int A { init; get; }

    public int B { set => _b = value; get => _b; }
}
```

Before (issue run):

```csharp
class C
{
    private int _b;

    public int A { get; init; }

    public int B
    {
        get => _b;
        set => _b = value;
    }
}
```

After:

```csharp
class C
{
    private int _b;

    public int A { get; init; }

    public int B { get => _b; set => _b = value; }
}
```

`A` is unchanged; `B` keeps its line. For RH7108, `public event Action Changed { remove => _handlers -= value; add => _handlers += value; }` is today spread over four lines (measured) and becomes `public event Action Changed { add => _handlers += value; remove => _handlers -= value; }`.

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

### A line of code behind a comment in column 0 among moved statements (RH7004)

```csharp
using System.IO;

class C
{
    void M()
    {
        using var s = new MemoryStream();
/* x */ s.Flush();
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
            /* x */ s.Flush();
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
    /* x */ s.Flush();
            s.Flush();
        }
    }
}
```

The line holds code, so it is no comment line: it moves one level with the other statements, as every line of the RH3004 fix's block that holds code does.

### Code in front of the using declaration on its line (RH7004)

```csharp
using System.IO;

class C
{
    void M()
    {
        int a = 0; using var s = new MemoryStream();
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
        int a = 0; using (var s = new MemoryStream())
                   {
                       s.Flush();
                   }
    }
}
```

After: no fix offered.

### The last moved statement shares its line with the block's closing brace (RH7004)

```csharp
using System.IO;

class C
{
    void M()
    {
        using var s = new MemoryStream();
        s.Flush(); }
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
        }
}
}
```

After: no fix offered.

### A tab in front of a moved line (RH3004)

The statement line starts with one tab followed by eight spaces:

```csharp
using System;

class C
{
    void M()
    {
        Action<int> b = delegate (int x)
        {
	        Console.WriteLine(x);
        };
    }
}
```

Before (the tab kept, sixteen spaces written in front of it):

```csharp
using System;

class C
{
    void M()
    {
        Action<int> b = (int x) =>
                        {
                	        Console.WriteLine(x);
                        };
    }
}
```

After: no fix offered.

### A `///` comment behind code documents the moved member (RH7103; the same for RH7102, RH7109)

With the documentation mode `Parse` or `Diagnose`:

```csharp
public class C
{
    public void Run()
    {
    } /// <summary>Creates</summary>
    public static void Create()
    {
    }
}
```

Before:

```csharp
public class C
{
    /// <summary>
    /// Creates
    /// </summary>
    public static void Create()
    {
    }
    public void Run()
    {
    }
}
```

After: no fix offered. With the documentation mode `None`, the comment is an ordinary comment behind `Run`'s closing brace, and the fix moves `Create` without it, as before.

### A `///` comment behind code documents the target (RH7109; the same for RH7102, RH7103)

With the documentation mode `Parse` or `Diagnose`:

```csharp
class C
{
    private const int K = 1; /// <summary>A</summary>
    private int _a;
    private readonly int _b;
}
```

Before:

```csharp
class C
{
    private const int K = 1;
    private readonly int _b;

    /// <summary>
    /// A
    /// </summary>
    private int _a;
}
```

After: no fix offered.

### Crossed members that share a line keep it (RH7109; the same for RH7102, RH7103)

```csharp
class C
{
    private int _a; private int _c;
    private readonly int _b;
}
```

Before:

```csharp
class C
{
    private readonly int _b;
    private int _a;
    private int _c;
}
```

After:

```csharp
class C
{
    private readonly int _b;
    private int _a; private int _c;
}
```

## Decisions

### Moved lines keep their columns relative to each other

Maintainer, batch 1, Q1 A and Q2 A. A member or accessor moves all its lines by the shift of its first token; RH7004 moves every line of the moved statements one level deeper. The formatter and the layout rules still correct a continuation line on their own; the fix does not decide it for them.

### A move is a move of whole lines

Maintainer, batch 1, addition to Q1: a move that goes beyond moving whole lines, such as a member sharing a line with another member or a token, is not offered.

### Accessors on one line swap places within the line

Maintainer, Q7 B. When the whole accessor list stands on one line, the move breaks and joins no line, so the whole-line rule does not refuse it: the accessors swap places, and the whitespace between them stays where it is. An accessor list that spans lines and holds a shared line is withheld.

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

### Code in front of the declaration, or a closing brace behind the last statement, withholds RH7004

Lead, decision 21; local rare-class refusals. Both shapes are reported by the layout rules, and the new block would share a line with code the fix does not move: the fix would have to decide a line break, or a column for the braces that no line's indentation states. `main` produces a broken layout for both.

### A move does not carry a `///` comment behind code

Lead, decision 22, from the maintainer's whole-line answer (Q1). The comment documents the declaration below it, so it would have to travel with the moved declaration onto a line of its own, a line break the author did not write; leaving it behind would hand the documentation to another declaration. The same comment in front of the target would end up documenting the moved declaration. Both moves are withheld. RH8303 reports the comment.

### Crossed members that share a line move as one block

Lead, decision 23. They do not move relative to each other, so keeping their line decides no line break; splitting it, as `main` does, would.

### One line rule for moved lines; a tab counts as one column

Lead, decisions 19 and 23. A comment line in column 0 is the line RH3004 already keeps; a line that holds code moves, whatever comment stands in front of it. A tab counts as one column, as the rules count it elsewhere (FMT-PIPE-017). A shift to the left that would cut a line off at column 0 is withheld (SHARED-ORDER-010): it is rare, because only a member indented deeper than its slot shifts to the left, and cutting the line off would break the rule that every line moves by the same number of columns. The SHARED-ORDER-010 example gets no fix on `main` either, there because the comment stands inside the declaration (FMT-UNSUP-004).

### A tab in front of a moved line withholds the RH3004 block-body fix

Lead, decision 26; local rare-class refusal (RH3004-006). With one line rule for every fix that moves lines, a tab counts as one column, which shifts RH3004's statements to another place than `main`, where the old shifter counted it as four. Neither output is one a requirement demands, and RH5601 reports tab indentation, so the fix is withheld for that class with a cheap check on the leading whitespace of the moving lines.

### A blank line of moved code is written empty

Lead, decision 27 (SHARED-FIX-007, *Moved code*). A line that holds only whitespace is written empty and stays a blank line. This is `main`'s output for the RH3004 and RH7004 fixes and for member moves, checked with `scripts/apply-fix.sh`; nothing visible changes.

## Alternatives considered

- **Lines of moved code get the column the formatter gives them** (Q1 B, Q2 B) — rejected by the maintainer: it reformats code the fix only moves, which SHARED-FIX-007 forbids for the code's other layout.
- **A moved member that shares its line gets its own line** (today's output) — rejected by the maintainer: it decides a line break the author did not write.
- **Withhold the fix for every accessor list on one line** (Q7 A) — rejected by the maintainer: it would withhold the fix for the common shape of a short property, `{ init; get; }` included.
- **Shift the lines of a multi-line raw string with the statement, or leave them where they are** (Q3 A, B) — rejected by the maintainer for now; a later change may add one.
- **A comment in column 0 moves with the statements** (Q4 B) — rejected by the maintainer.
- **RH3004 counts a tab as one column, as the other moves do** — rejected (decision 26): it changes RH3004's output for tab indentation with no requirement demanding it.
- **RH3004 keeps counting a tab as four columns** — rejected (decision 26): a second counting rule beside SHARED-FIX-007's, in the one owner of the line rule.
- **Withhold RH7004 for every directive among the moved statements** — rejected: `#if DEBUG` and `#pragma` in method bodies are common code, and the per-line rule is a cheap check.
- **One shared "move lines by a column delta" helper for RH3004, RH7004, and the ordering fixes** (thoenissen/Reihitsu#1604) — not a behavior; the member and accessor moves need no shift in the common case, and RH3004's rules differ. Left to the implementation.
