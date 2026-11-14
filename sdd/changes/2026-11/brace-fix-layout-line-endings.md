# Brace and attribute-split fixes change only what they report

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | SHARED-BRACE-002 (clarified: aligned to SHARED-FIX-006/007/008, a body that ends the document; changed: which body lines move, code behind the body, whitespace at the end of the body's line), SHARED-BRACE-003 (added), SHARED-FIX-007 (clarified: the brace fixes' lines and tab withhold), SHARED-FIX-008 (changed: the brace fixes and the RH7309 fix join the moves that are withheld for a foreign line break), RH7309-007 (added), RH5011-004, RH8304-004 (clarified: the detected line ending, also in a lone-`\r` document), SHARED-ATTR-017 (clarified: only the reported list changes; the whitespace in front of a further list is a copy, a tab included), SHARED-ATTR-022 (added: withheld behind a `{` on the list's line that is still open there), SHARED-ATTR-023 (added), RH5502-003, RH5504-003, RH5506-003, RH5508-003, RH5510-003, RH5512-003, RH5514-004, RH5516-003, RH5518-003, RH5520-003, RH5522-003, RH5526-003, RH5528-003, RH5531-005 (clarified: each further list starts a new line), RH5502-004, RH5504-004, RH5506-004, RH5508-004, RH5510-004, RH5512-004, RH5514-005, RH5516-004, RH5518-004, RH5520-004, RH5522-004, RH5526-004, RH5528-004, RH5531-006 (changed: withheld where SHARED-ATTR-022 and SHARED-ATTR-023 withhold) |
| Depends on | [Moved code keeps its lines](moved-code-keeps-its-lines.md) |

## What

- **The brace fixes wrap only the body** (SHARED-BRACE-002; RH5405, RH5406, RH5407). The fix no longer formats the whole `if` statement. It wraps the reported body in braces and changes nothing else: the header with its spacing and comments, the other body, and the blank lines around the statement stay as written, and no blank line is inserted. The braces start their own lines in the column of the first token of the line on which the owning `if` or `else` starts — the `else` for an `else if`, the closing brace for `} else`. A body on the header's last line moves below `{`, one level deeper. A body below the header's last line keeps every line in its column, whatever its indentation, continuation lines included. Code behind the body on its line starts its own line below `}`, except the `else` of the same `if`, which stays behind `}`. Whitespace at the end of the body's line stays there, and none is written behind `}`. A comment behind the body stays inside the block, once. A body that ends the document without a line break behind it gets the fix like any other, and the document still ends without one.
- **The brace fixes are withheld where they cannot wrap the body as written** (SHARED-BRACE-003): a body on the header's last line that spans several lines; other code in front of the owning `if` or `else` on its line; a directive or disabled text between the header and the body; a tab in front of the owning `if` or `else`; and a line break in the moved code other than the detected line ending, which includes every document whose line breaks are all a lone `\r`. The diagnostic stays in each case.
- **The brace fixes and the RH7309 fix move code as written** (SHARED-FIX-008, second paragraph; RH7309-007). Both are now in the class of fixes that move code without the formatter. Such a fix is withheld when the code it moves holds a line break other than the detected line ending. For RH7309 that code is a region block whose place changes.
- **RH5011 and RH8304 write the detected line ending** (RH5011-004, RH8304-004). In an all-CRLF or all-LF document this is the line ending they already wrote. In a document whose line breaks are all a lone `\r`, it is `\n`. In a mixed document, it is the detected line ending, not the line's own break.
- **The attribute-list split fixes change only the reported list** (SHARED-ATTR-017; RH5502 to RH5531). Each list after the first starts a new line, behind a character-for-character copy of the whitespace at the start of the reported list's line, a tab included. Everything else on the list's line and on every other line stays as written: a list or comment in front, a list or the declaration behind, and the indentation, comments, and directives around it. The inserted line breaks are the detected line ending.
- **The split fixes are withheld for a list behind a `{` on its line that is still open there** (SHARED-ATTR-022): a `{` in front of the list on the list's line whose `}` does not stand in front of the list on that line, whatever the `{` opens — the accessor list of an accessor, the type of a member, the namespace of a type, the block of a local function or of a lambda, a switch statement. The diagnostic stays. A `{` closed in front of the list, as in `void M() { } [A, B] void N() { }`, and a `{` on a line above do not withhold the fix.
- **The split fixes are withheld for a line break they would copy or keep in another line ending** (SHARED-ATTR-023): a line break in the target specifier, such as between `field:` and the first attribute, and a line break inside the list, from `[` through `]`, other than the detected line ending, which includes every line break inside a list in a document whose line breaks are all a lone `\r`. The diagnostic stays.

## Why

thoenissen/Reihitsu#1416 and thoenissen/Reihitsu#1627 report that the brace fixes reformat the whole `if` statement. They insert a blank line, reformat the header and the other body, move `else`, put the `else if` braces in the inner `if`'s column, and leave whitespace behind the new `}`. thoenissen/Reihitsu#1142 reports that the attribute-list split fixes reformat the code around the list and write LF into CRLF documents. thoenissen/Reihitsu#1388 reports fixes that write a line ending other than the document's. Most of this is a bug against SHARED-FIX-006, SHARED-FIX-007, and SHARED-FIX-008, which the maintainer's triage comments on #1416 and #1388 confirm. To bring the brace fixes to those requirements, they now wrap the body as text instead of formatting the `if` statement. That leaves cases the requirements did not decide: continuation lines, code behind the body, trailing whitespace, a misindented body, and the accessor-list line of a split. The maintainer answered them (run `brace-fix-layout-line-endings`, answers 1 to 7). The added withholds are rare input classes, each found with a cheap check (`AGENTS.md`, *Code-fix coverage*).

## Visible output changes

The outputs on `main` were measured with `scripts/apply-fix.sh` on `c2866ea`, LF and CRLF alike, with one application of the fix. The outputs after the change were measured the same way on the run's branch. The lone-`\r` and mixed-document rows were measured by the run's tests, because `apply-fix.sh` runs only LF and CRLF. Where no fix is offered, the diagnostic stays. The entries added after the audit, for a list behind the `{` of a type, a namespace, or a block, and for a line break in the target specifier, were measured the same way on `166e3dd`, a later `main` that changed none of these fixes.

### RH5405 · Braces must not be omitted — the header, the body, and the blank lines stay

```csharp
        b++;
        if ( a )
            M(a,b);
```

Before:

```csharp
        b++;

        if (a)
        {
            M(a, b);
        }
```

After:

```csharp
        b++;
        if ( a )
        {
            M(a,b);
        }
```

### RH5405 · Braces must not be omitted — a header that spans several lines

```csharp
        if (a &&
            b) M(a, b);
```

Before:

```csharp
        if (a
            && b)
        {
            M(a, b);
        }
```

After:

```csharp
        if (a &&
            b)
        {
            M(a, b);
        }
```

### RH5405 · Braces must not be omitted — a body below its `if` that is not indented one level deeper

```csharp
        if (a)
          M(a, b);
```

Before:

```csharp
        if (a)
        {
            M(a, b);
        }
```

After:

```csharp
        if (a)
        {
          M(a, b);
        }
```

### RH5406 · Braces must not be omitted from multi-line child statements — a body below its `if` keeps its continuation lines

```csharp
        if (a)
            M(a,
                b);
```

Before:

```csharp
        if (a)
        {
            M(a,
              b);
        }
```

After:

```csharp
        if (a)
        {
            M(a,
                b);
        }
```

### RH5406 · Braces must not be omitted from multi-line child statements — a multi-line body on the line of its `if`

```csharp
        if (a) M(a,
                 b);
```

Before:

```csharp
        if (a)
        {
            M(a,
              b);
        }
```

After: no fix offered; the diagnostic stays.

### RH5405 · Braces must not be omitted — the other body stays as written

```csharp
        if (a)
            M(a,b);
        else
            M(a,
              b);
```

Before:

```csharp
        if (a)
        {
            M(a, b);
        }
        else
        {
            M(a,
              b);
        }
```

After:

```csharp
        if (a)
        {
            M(a,b);
        }
        else
            M(a,
              b);
```

### RH5405 · Braces must not be omitted — the body of an `else if`

```csharp
        if (a)
        {
            M(a, b);
        }
        else if (b)
            M(a, b);
```

Before:

```csharp
        if (a)
        {
            M(a, b);
        }
        else if (b)
             {
                 M(a, b);
             }
```

After:

```csharp
        if (a)
        {
            M(a, b);
        }
        else if (b)
        {
            M(a, b);
        }
```

### RH5407 · Use braces consistently — an `else` behind a closing brace

```csharp
        if (a)
        {
            M(a, b);
        } else M(a, b);
```

Before:

```csharp
        if (a)
        {
            M(a, b);
        }
        else
        {
            M(a, b);
        }
```

After:

```csharp
        if (a)
        {
            M(a, b);
        } else
        {
            M(a, b);
        }
```

### RH5405 · Braces must not be omitted — an `else` behind the body on its line

```csharp
        if (a) M(a, b); else M(a, b);
```

Before (both bodies are wrapped, `else` moves):

```csharp
        if (a)
        {
            M(a, b);
        }
        else
        {
            M(a, b);
        }
```

After (the fix of the `if` body; the `else` body is reported and fixed on its own afterwards):

```csharp
        if (a)
        {
            M(a, b);
        } else M(a, b);
```

### RH5405 · Braces must not be omitted — a statement behind the body on its line

```csharp
        if (a) M(a, b); /* c */ b++;
```

Before:

```csharp
        if (a)
        {
            M(a, b); /* c */
        } b++;
```

After:

```csharp
        if (a)
        {
            M(a, b); /* c */
        }
        b++;
```

### RH5405 · Braces must not be omitted — whitespace at the end of the body's line

In the input, `·` stands for a space.

```csharp
        if (a)
            M(a, b); /* x */···
```

Before:

```csharp
        if (a)
        {
            M(a, b); /* x */
        }···
```

After:

```csharp
        if (a)
        {
            M(a, b); /* x */···
        }
```

### RH5405 · Braces must not be omitted — code in front of the `if` on its line

```csharp
        b = a; if (a) M(a, b, i);
        switch (i)
        {
            case 1: if (a) M(a, b, i);
                break;
        }
```

Before (each fix on its own):

```csharp
        b = a; if (a)
               {
                   M(a, b, i);
               }
        switch (i)
        {
            case 1: if (a)
                    {
                        M(a, b, i);
                    }
                break;
        }
```

After: no fix offered for either body; the diagnostics stay. In `if (b) if (a) M(a, b, i);` the outer body gets the fix, and the inner body gets it once the inner `if` starts its own line.

### RH5405 · Braces must not be omitted — a directive between the `if` and its body

```csharp
        if (a)
#pragma warning disable CS0618
            M(a, b);
#pragma warning restore CS0618
```

Before: a fix is offered that changes nothing.

After: no fix offered; the diagnostic stays. `#region` and `#endregion` around the body, and disabled text in front of it, withhold it alike.

### RH5405 · Braces must not be omitted — a tab in front of the `if`

In the input, `→` stands for a tab.

```csharp
→→if (a)
→→→M(a, b);
```

Before (the tabs of the statement become spaces; the rest of the method keeps its tabs):

```csharp
        if (a)
        {
            M(a, b);
        }
```

After: no fix offered; the diagnostic stays.

### RH5405, RH5406, RH5407 · Braces must not be omitted — documents whose line breaks are all a lone `\r`, or mixed

```csharp
        if (a)
            M(a, b);
```

Before, all lone `\r`: the fix is offered and writes `\n` for the line breaks it creates. For RH5406, the line break behind `}` stays a lone `\r` (derived from the code, not run). Before, CRLF with an LF behind `if (a)`, or inside a verbatim string or a comment of the body: the fix is offered.

After, all lone `\r`: no fix offered for any body that a line break ends; the diagnostic stays. After, CRLF with an LF in the code the fix moves (from the end of the header through the line break ending the body): no fix offered. An LF elsewhere in the document, such as on a line above the `if`, keeps the fix, and every line break the fix creates is CRLF. All-LF and all-CRLF documents are unchanged.

### RH7309 · Regions should follow category order — a moved block holds another line ending

The input of RH7309-004, in a document whose line breaks are CRLF except an LF behind `private int _a;` in the `Fields` block, which the fix moves.

Before: the fix is offered and moves the block with its LF.

After: no fix offered; the diagnostic stays. With the LF behind `private int _between;` instead, which stays between the blocks, the fix is offered as before. In a document whose line breaks are all a lone `\r`, no fix is offered wherever a block moves. All-LF and all-CRLF documents are unchanged.

### RH5011 · Statements should be followed by a blank line — lone `\r` and mixed documents

The `switch` section of the RH5011-004 example:

```csharp
            case 1:
                a++;
                break;
            default:
                break;
```

Before, all lone `\r`: `break;\r            default:` becomes `break;\r\r            default:`.

After, all lone `\r`: it becomes `break;\n\r            default:`. The inserted line break is `\n`, and every other line break stays a lone `\r`.

Before, CRLF with an LF ending the `break` line: `break;\n            default:` becomes `break;\n\n            default:`.

After: it becomes `break;\n\r\n            default:`.

### RH8304 · Documentation elements must be on separate lines — lone `\r` and mixed documents

```csharp
    /// <summary>Stops</summary> <remarks>Remarks</remarks> <example>Example</example>
```

Before, all lone `\r`: each line break the fix inserts is a lone `\r`. Before, CRLF with an LF ending that documentation line: each inserted line break is LF.

After: `\n` in the all-lone-`\r` document, and CRLF in the CRLF document. The line breaks around them stay as written.

### RH5514 · Attributes of methods must be in separate lists — the code around the list stays

```csharp
namespace N
{
class C
{
#pragma warning disable CS0001
// c
    [A, B]
        // d
    void M()
    {
    }
}
}
```

Before:

```csharp
namespace N
{
class C
{
#pragma warning disable CS0001
        // c
        [A]
        [B]
        // d
        void M()
    {
    }
}
}
```

After:

```csharp
namespace N
{
class C
{
#pragma warning disable CS0001
// c
    [A]
    [B]
        // d
    void M()
    {
    }
}
}
```

### RH5514 · Attributes of methods must be in separate lists — a list that shares its line

```csharp
    [A] [B, D2] void M()
    {
    }

    [A, B] [D2] void N()
    {
    }

    /* c */ [A, B]
    void O()
    {
    }

    [return: A] [A, B] int R() => 0;
```

Before (each fix on its own):

```csharp
    [A]
    [B]
    [D2]
    void M()
    {
    }

    [A]
    [B]
    [D2] void N()
    {
    }

    /* c */
    [A]
    [B]
    void O()
    {
    }

    [return: A]
    [A]
    [B]
    int R() => 0;
```

After:

```csharp
    [A] [B]
    [D2] void M()
    {
    }

    [A]
    [B] [D2] void N()
    {
    }

    /* c */ [A]
    [B]
    void O()
    {
    }

    [return: A] [A]
    [B] int R() => 0;
```

The placement rules report the lists and declarations that now share a line, such as RH5513 for `[D2] void M()`.

### RH5502 · Assembly attribute lists must follow shape rules — Fix All over two lists on one line

```csharp
[assembly: First, Second] [assembly: Third, Fourth]
internal class Example { }
```

Before:

```csharp
[assembly: First]
[assembly: Second]
[assembly: Third]
[assembly: Fourth]
internal class Example { }
```

After:

```csharp
[assembly: First]
[assembly: Second] [assembly: Third]
[assembly: Fourth]
internal class Example { }
```

The other split rules behave alike.

### RH5514 · Attributes of methods must be in separate lists — a tab-indented list

In the input, `→` stands for a tab.

```csharp
class C
{
→[A, B]
→void M()
→{
→}
}
```

Before:

```csharp
class C
{
    [A]
    [B]
    void M()
→{
→}
}
```

After:

```csharp
class C
{
→[A]
→[B]
→void M()
→{
→}
}
```

### RH5502 to RH5531 · Attribute split fixes — line endings

The same inputs in a CRLF document. Before: where the fix reformatted lines around the list, as in the shapes above, those lines and the inserted line breaks were LF. After: the inserted line breaks are CRLF, and no other line break changes. In a document whose line breaks are all a lone `\r`, the inserted line break is `\n`.

### RH5518 · Field-targeted attributes must be in separate lists — a list behind the accessor list's `{`

```csharp
    public int Value { [field: First, Second] get => 1; }
```

Before:

```csharp
    public int Value {
        [field: First]
        [field: Second]
        get => 1; }
```

After: no fix offered; the diagnostic stays. RH5528 and RH5531 behave alike, as in `public int Other { [A, B] get => 1;` ⏎ `set { } }`.

With the `{` on a line above the list:

```csharp
    public int Value
    {
        [field: First, Second] get => 1;
    }
```

Before:

```csharp
    public int Value
    {
        [field: First]
        [field: Second]
        get => 1;
    }
```

After:

```csharp
    public int Value
    {
        [field: First]
        [field: Second] get => 1;
    }
```

### RH5514, RH5506 · Attribute split fixes — a list behind a `{` on its line that is still open there

```csharp
namespace N { [A, B] class E { } }

class C { [A, B] void M() { } }

class D
{
    void N() { [A, B] void L() { } }
}
```

Before (each fix on its own):

```csharp
namespace N {
    [A]
    [B]
    class E { } }

class C {
    [A]
    [B]
    void M() { } }

class D
{
    void N() {
        [A]
        [B] void L() { } }
}
```

After: no fix offered for any of the three lists; the diagnostics stay. The other split rules behave alike for the declarations they report.

The same holds where the open `{` belongs to code further out than the declaration the list belongs to — a lambda in a statement or a field initializer, a local function in a switch section:

```csharp
class C2 { Func<int> f = [A, B] () => 1; }

class D
{
    void O() { Func<int> f = [A, B] () => 1; }

    void P(int x)
    {
        switch (x) { case 1: [A, B] void L() { } break; }
    }
}
```

Before (RH5514, each fix on its own):

```csharp
class C2 { Func<int> f = [A]
    [B] () => 1; }

class D
{
    void O() { Func<int> f = [A]
        [B] () => 1; }

    void P(int x)
    {
        switch (x) { case 1:
                [A]
                [B] void L() { } break; }
    }
}
```

After: no fix offered for any of the three lists; the diagnostics stay. RH5528 behaves alike for `[return: A, B]` on such a lambda.

A list on a line whose `{` in front of it is closed there, as in `void M() { } [A, B] void N() { }`, is split as a list that shares its line: `void M() { } [A]` ⏎ `[B] void N() { }`. A lambda in a block whose `{` stands on a line above is split as SHARED-ATTR-017 states:

```csharp
    void M()
    {
        Func<int> f = [A, B] () => 1;
    }
```

After:

```csharp
    void M()
    {
        Func<int> f = [A]
        [B] () => 1;
    }
```

### RH5518 · Field-targeted attributes must be in separate lists — a line break in the target specifier

```csharp
    [field:
        A, B]
    int _f;
```

Before:

```csharp
    [field:
        A]
    [field:
B]
    int _f;
```

After: no fix offered; the diagnostic stays. With the line break between the attributes, `[field: A,` ⏎ `B]`, the fix is offered and gives `[field: A]` ⏎ `[field: B]`.

### RH5502 to RH5531 · Attribute split fixes — another line ending inside the list

```csharp
    [A(1,
      2), B]
    void M()
    {
    }
```

In a document whose line breaks are CRLF, except the line break inside the argument list of `A`, which is LF. Before: the fix is offered. After: no fix offered; the diagnostic stays. In a document whose line breaks are all a lone `\r`, no fix is offered for a list that holds a line break; a list on one line is still split, with `\n` between the new lists. In an all-LF or all-CRLF document, the list is split, and the line break inside `A` stays as written.

### Unchanged

Several cases give the same output on `main` and after the change:
- a body on the line of its `if`;
- a comment behind the body (`M(a, b); // trail`), which stays inside the block once;
- blank lines and a comment between the `if` and its body;
- the RH5405 CRLF comment instance and the RH7004 instance of #1388;
- RH5011, RH8304, and RH7309 in all-LF and all-CRLF documents;
- a body that ends a file of top-level statements without a final line break, such as `if (args.Length > 0) System.Console.WriteLine();`, which gets `{`, the body, and `}` on lines of their own, with no line break behind `}`.

Rule pages are unchanged. Their examples are bodies below the `if` and lists on their own line, whose output does not change, and a diagnostic without a fix is covered by `documentation/rules/general-notes.md`.

## Decisions

### A multi-line body on the line of its `if` or `else` gets no fix

Maintainer, answer 1, C. A body on the header's last line is one line that moves below `{`. A body below the header keeps its lines. A body on the header's line that spans lines has no column for its continuation lines that keeps both common continuation styles, so RH5406 offers no fix for it (SHARED-BRACE-003).

### Code behind the body starts its own line

Maintainer, answer 2, A. A statement behind the body on its line starts the line below the new `}`, in the column of `}`. A comment behind the body stays inside the block.

### Blank lines between the header and the body are removed

Maintainer, answer 3, A. This is the same as on `main`. Blank lines directly below the header's last line go, so `{` takes the line below it. From the first comment or body line on, every line stays.

### Whitespace at the end of the body's line stays there

Maintainer, answer 4, A. The body's line is moved as written, and the new `}` gets no whitespace. The layout rules still report the whitespace.

### A directive or disabled text between the header and the body is not declared unsupported

Maintainer, answer 5, A. The shape is not declared unsupported. The brace fixes are withheld for it locally (SHARED-BRACE-003), which also removes the no-op fix `main` offered for `#pragma`.

### A body below its header keeps its column, whatever its indentation

Maintainer, answer 6, A. A misindented body and each of its continuation lines keep their columns. The indentation rules still report them.

### A split list behind a `{` on its line that is still open there gets no fix

Maintainer, answer 7, B, for `public int Value { [field: First, Second] get => 1; }`. The lead widened the scope twice. First to every reported list on the line of the accessor list's `{` (decision 14): that is the same cause, a cheaper check, and the only shape in which RH5531 meets the case. Then, after the audit, to every reported list on the line of the `{` that opens the body holding its declaration — a type, a namespace, or a block (decision 18): the whitespace at the start of that line is again the indentation of the code that owns the braces, which gives the same odd output. Then, after the second audit, the list of bodies was replaced by the condition behind all of them (decision 23): a `{` in front of the list on the list's line whose `}` does not stand in front of the list on that line. The audit found the same output for lists whose open `{` belongs to code further out than the declaration — a lambda in a statement or a field initializer, a local function in a switch section — which a list of the bodies that hold a declaration directly does not reach. A class that came back is refused as a whole rather than enumerated again, and the condition is a scan of the tokens in front of the list on its line. Each widening withholds a fix for a rare class (SHARED-ATTR-022); a `{` closed in front of the list, as in `void M() { } [A, B] void N() { }`, still gets the fix.

### A split is withheld for a line break it would copy or keep in another line ending

Lead, decision 19. The split writes each new list as the specifier followed by one attribute, as written. A line break in the target specifier would be repeated in front of every attribute, and a line break inside an attribute in another line ending would stay in the changed code, against the first paragraph of SHARED-FIX-008. Both are rare classes, found with a cheap check, so the fix is withheld for them (SHARED-ATTR-023).

### A body that ends the document gets the fix

Lead, decision 17. SHARED-BRACE-002 demands the fix wherever SHARED-BRACE-003 does not withhold it, and SHARED-BRACE-003 names no end-of-file case. A body that ends a file of top-level statements without a final line break gets its braces, and no line break is written behind `}`, as on `main`. The requirement is unchanged; it now states the case with an example.

### An `else` behind the new `}` stays there

Maintainer, thoenissen/Reihitsu#1220 Q3, applied by the lead (decision 5): the fix must not move the `else`. This covers `} else M();` and an `else` behind the body on its line.

### A tab-indented list copies the tab

Maintainer, thoenissen/Reihitsu#1109 Q1, reading A, applied by the lead (decision 6). Each further list starts behind a copy of the whitespace at the start of the reported list's line.

### Each split changes only its own list, also under Fix All

Maintainer, thoenissen/Reihitsu#1109 Q1, reading A, applied by the lead (decision 12). Two multi-attribute lists on one line give `[Second] [Third]` on one line, not a line each.

### Rare classes are withheld

Lead, decisions 7, 8, and 9. Each of these is a rare input class, and each is withheld with a cheap check (`AGENTS.md`, *Code-fix coverage*):
- other code in front of the owning `if` or `else` on its line;
- a tab in front of it;
- a directive between the header and the body;
- a foreign line break in the code the brace fixes or the RH7309 fix move.

Both fixes move text without the formatter, so they belong to the second paragraph of SHARED-FIX-008, as RH7004 and the member moves do.

### The line ending of RH5011 in a lone-`\r` document

Lead, decision 11. The `\n` goes in front of the `\r` that ends the `break` line, giving `break;\n\r`. Behind it, the two would read as one CRLF, and no blank line would exist.

### Where the body starts is decided by the header's last line

Lead, decision 13. A header that spans several lines puts the braces below its last line, the line of `)` or `else`. A line break inside a delimited comment does not count.

## Alternatives considered

- **Shift the continuation lines of a multi-line body on the `if` line by the shift of its first token** (answer 1, A) — rejected by the maintainer in favour of no fix.
- **Leave code behind the body behind the new `}`** (answer 2, B) — rejected: the formatter breaks the line behind a brace the fix created, so the result would change again.
- **Remove trailing whitespace from the body's line** (answer 4, B) — rejected: the body is moved as written, as RH7004 moves its lines.
- **Declare a directive between the header and the body unsupported** (answer 5) — rejected: a local withhold is enough for one construct.
- **Move a misindented body one level inside the braces** (answer 6, B) — rejected: it needs a shift and new withholds, and the indentation rules report the line anyway.
- **Split a list behind the accessor list's `{` by laying out the accessor list over several lines** (answer 7, A) — rejected: it changes the declaration beyond the reported list.
- **Keep the brace fixes under SHARED-FIX-008's first paragraph and rewrite kept line breaks to the detected line ending** — rejected (decision 8). The body is moved as text, and the withhold is one existing cheap check.
- **Keep the split output on the line of a type's, namespace's, or block's `{`** (`class C { [A]` ⏎ `[B] void M() { } }`) — rejected (decision 18): it is the layout answer 7 rejected for an accessor list, from the same cause.
- **Withhold the split only for the bodies that hold a declaration directly, and add the owners further out one by one** (the lambda's statement or field, the switch section) — rejected (decision 23): the same class came back after the first enumeration, and the open `{` on the list's line is the one condition behind every member.
- **Rewrite a line break the split keeps inside an attribute to the detected line ending, and drop a line break in the target specifier** — rejected (decision 19): the split keeps the attributes as written, and a withhold is one cheap check of the list.
- **Fix the formatter's write-back of a target's trailing trivia, the cause behind the whitespace and lone `\r` behind `}`, in this change** — rejected (decision 10). It concerns every fix that delegates to the formatter and is a follow-up of its own. The brace fixes leave that class because they no longer format the `if` statement.
