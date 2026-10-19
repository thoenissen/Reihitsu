# Shared — Code fixes

| | |
|---|---|
| ID prefix | SHARED-FIX |
| Status | Active |
| Code owners | — |
| Uses | — |

This spec covers what holds for every code fix of an analyzer rule, so a rule spec states only what its own fix produces and when it is withheld. It applies to every rule whose header names a code fix, without being listed under `Uses`; SHARED-FIX-002 applies to the rules whose header says `Fix All`.

## Non-goals

- What a fix produces and when it is withheld — each rule's own spec.
- Whether a fix leaves diagnostics of other rules in the code it changed. It is a known failure class (`squad/failure-classes.md`), not specified behavior; SHARED-FIX-004 only forbids a fix that undoes the fix of another rule.
- How a fix lays out the code it changed — FMT-PIPE-010 for fixes that delegate to the formatter.

## Requirements

### SHARED-FIX-001 — One application corrects a diagnostic

Applying a rule's code fix to a reported diagnostic MUST correct it in one application: once the rule's own fixes have been applied one after another, at most once for each diagnostic the rule reported, the rule MUST NOT report the code they corrected. The requirement holds for each rule on its own:

- The corrected code is everything the rule reports at the place it checks, such as a whole run of blank lines, not only the span the diagnostic covers.
- A fix of another rule that would remove the rest does not count, even when that rule reports part of the same code.

Why: A fix that has to be applied twice, or that recreates its own diagnostic, never reaches a clean state for the user and makes Fix All results depend on how often it runs. A user who applies one rule's fix sees that rule's diagnostics only, and the other rule may be disabled, so each rule has to reach its clean state with its own fixes.

Example — the RH5002 fix, applied once:

```csharp
class C
{
    void M(int a)
    {
        a++;
        {|RH5002:if|} (a > 1)
        {
        }
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        a++;

        if (a > 1)
        {
        }
    }
}
```

is not reported again. The diagnostic covers `if`; the place the rule checks, and the fix corrects, is the line in front of it.

Example — RH5028, which reports a run of blank lines with one diagnostic; its fix, applied once:

```csharp
{|RH5028:


|}class C
{
}
```

```csharp
class C
{
}
```

is not reported again: the one application removes the whole run.

Example — RH5023, which reports each blank line after the first of a run; its fix, applied once for each diagnostic:

```csharp
class C
{
    void M()
    {
        var a = 1;

{|RH5023:
|}{|RH5023:
|}        a++;
    }
}
```

```csharp
class C
{
    void M()
    {
        var a = 1;

        a++;
    }
}
```

is not reported again.

Example — two rules that report parts of one run. In

```csharp
{|RH5028:
{|RH5023:
|}{|RH5023:
|}|}class C
{
}
```

RH5028 reports the whole run, and RH5023 reports its second and third line. The RH5023 fix, applied once for each of its diagnostics, leaves the first blank line. RH5023 does not report that line; RH5028 still does, and a diagnostic of another rule left behind is outside this spec (see Non-goals). The RH5028 fix, applied once, removes the whole run on its own, as in the RH5028 example above, and does not rely on the RH5023 fix to remove the second and third line.

### SHARED-FIX-002 — Fix All corrects everything in one application

A code fix that offers Fix All MUST correct every reported diagnostic of a document, a project, or a solution in one Fix All application, and the result MUST equal the result of applying the single fixes one after another.

Why: The user applies Fix All expecting one step that leaves nothing behind. Requiring the same result as the single fixes makes the outcome independent of the order in which the diagnostics are fixed.

Example — the RH2005 fix, applied with Fix All:

```csharp
public class C
{
    public string {|RH2005:first|};

    public int {|RH2005:second|};
}
```

```csharp
public class C
{
    private string first;

    private int second;
}
```

### SHARED-FIX-003 — A fix that could break the code is withheld

A code fix MUST NOT be offered when its result would:

- have a compile error that the code did not have;
- change what the program does at run time;
- change the text of a comment, a directive, or a literal that the user wrote; or
- leave code that an `#if` branch excludes referring to a name the fix changed.

The diagnostic is still reported. The second and third conditions are bounded as follows:

- **Run time.** The new name or the new accessibility that the fix exists to produce does not count; any other difference in behavior does. Reordering the instance fields of a type whose field order sets its layout counts. That is a struct, which has sequential layout unless it declares `LayoutKind.Auto` or `LayoutKind.Explicit`, and any other type declared with `[StructLayout(LayoutKind.Sequential)]`. Under `LayoutKind.Explicit`, each field's `[FieldOffset]` sets its offset, so the field order sets no layout. A reorder that changes neither a layout nor the order in which code runs does not count.
- **Comment, directive, and literal text.** Text that a diagnostic of the fix's own rule reports may change. Two kinds of text change together with a name the fix changes, as part of the rename:
  - a name in a documentation comment that binds to a symbol the fix renames, such as a `cref` or a `<param name>`; a word of the comment's text that only reads like the name does not bind to it and stays unchanged;
  - the comment of an `#endregion` that repeats the name of the region the fix renames; an `#endregion` comment that does not repeat that name stays unchanged.

  Layout does not count: the whitespace around a comment or a directive and the indentation of its lines; in a documentation comment, also the `///` exterior and the whitespace and line breaks next to an XML tag; and the indentation of a raw string literal's lines when the literal's value stays the same. The text of a comment or a directive is content, including the whitespace and line breaks inside it, such as between two words of a `//` or `/* */` comment or of a directive's message. A change to that text counts, and so does a changed value of a literal.

Why: A user applies a fix trusting that it only corrects what the diagnostic names. A fix that breaks the build, changes behavior, or edits text the user wrote as content hands the user a defect in exchange for a warning. The new name or accessibility is the correction itself, not a change in behavior; a field order that sets a type's layout is behavior, because native code and code that reads the memory directly see the fields in that order. A rename has to reach every place that refers to the symbol or repeats the region's name, and layout is not content, so neither is such an edit. Code excluded by `#if` compiles in another configuration, so a rename that misses it breaks that build. A diagnostic without a fix is a supported end state; a broken fix is not.

Example — a compile error, for RH4106:

```csharp
public class C
{
    private int {|RH4106:Value|};
    private int _value;
}
```

is reported, and no code fix is offered, because the renamed field would collide with `_value`. Without the second field, the fix renames `Value` to `_value`.

Example — the new name is not a run-time change, for RH4106:

```csharp
public class C
{
    private int {|RH4106:Value|};

    public string Name => nameof(Value);
}
```

```csharp
public class C
{
    private int _value;

    public string Name => nameof(_value);
}
```

`Name` returns the new name, which is the name the fix exists to produce.

Example — another run-time change, for RH7103:

```csharp
public class C
{
    private int _instance;
    private static int {|RH7103:_a|} = Compute();
    private static int {|RH7103:_b|} = _a;

    private static int Compute() => 1;
}
```

Both fields are reported. For `_b`, no code fix is offered: moving it in front of `_a` would run its initializer first, so `_b` would hold `0` instead of `1`. For `_a`, the fix is offered: it crosses only the instance field `_instance`, whose initialization runs at a different time, and moving a static field changes no layout.

Example — a reorder that changes no layout, for RH7109:

```csharp
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public struct S
{
    [FieldOffset(0)]
    public int _a;
    [FieldOffset(8)]
    public readonly long {|RH7109:_b|};
}
```

```csharp
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public struct S
{
    [FieldOffset(8)]
    public readonly long _b;
    [FieldOffset(0)]
    public int _a;
}
```

The fix is offered: under explicit layout, each field's `[FieldOffset]` sets its offset, and the moved field keeps its attribute, so the struct's layout stays the same. The same holds for a struct declared with `[StructLayout(LayoutKind.Auto)]` and for a class without a `StructLayout` attribute, whose fields the runtime lays out in an order of its own.

Example — a reference in a documentation comment, for RH4106:

```csharp
public class C
{
    /// <summary>Value is <see cref="Value"/>.</summary>
    private int {|RH4106:Value|};

    public int Get() => Value;
}
```

```csharp
public class C
{
    /// <summary>Value is <see cref="_value"/>.</summary>
    private int _value;

    public int Get() => _value;
}
```

The `cref` binds to the field and is renamed with it. The word `Value` at the start of the summary does not bind to anything and stays.

Example — the comment of an `#endregion`, for RH7302:

```csharp
class C
{
    {|RH7302:#region members|}
    #endregion // members

    {|RH7302:#region values|}
    #endregion // helpers
}
```

```csharp
class C
{
    #region Members
    #endregion // Members

    #region Values
    #endregion // helpers
}
```

The comment `// members` repeats the name of the region the fix renames and is renamed with it. The comment `// helpers` does not repeat the name `values` and stays.

Example — layout, for RH3202:

```csharp
public class C
{
    ///<summary>runs</summary>
    ///<returns>one</returns>
    public int M() {|RH3202:=> 1|};

    public string N() {|RH3202:=>
        """
        a
        """|};
}
```

```csharp
public class C
{
    /// <summary>
    /// runs
    /// </summary>
    /// <returns>one</returns>
    public int M()
    {
        return 1;
    }

    public string N()
    {
        return """
               a
               """;
    }
}
```

The fix changes the `///` exterior of the documentation comment of `M` and the line breaks next to its XML tags, and the indentation of the raw string literal in `N`; the comment's text and the literal's value stay the same.

Example — whitespace inside a comment's text, for RH5601, where `→` marks a tab character:

```csharp
class C
{
{|RH5601:→|}int x;{|RH5601:→|}// a→b
}
```

```csharp
class C
{
    int x;    // a→b
}
```

The fix replaces the two tabs its rule reports and keeps the tab inside the comment's text, which is content.

Example — text the rule reports, for RH8107:

```csharp
public class C
{
    /// <summary>Runs</summary> {|RH8107:<returns>Nothing</returns>|}
    public void Run()
    {
    }
}
```

```csharp
public class C
{
    /// <summary>Runs</summary>
    public void Run()
    {
    }
}
```

The fix changes the words of the documentation comment only in the element its diagnostic reports; the space it removes in front of that element is layout. Likewise, the RH5604 fix rewrites the line breaks its rule reports, also inside documentation comments, and keeps a line break in the text of a string literal, which is part of the literal's value and which its rule does not report.

Example — code excluded by `#if`, for RH4106:

```csharp
public class C
{
    private int {|RH4106:Value|};

    public int M()
    {
#if NEVER
        return 0;
#else
        return Value;
#endif
    }
}
```

```csharp
public class C
{
    private int _value;

    public int M()
    {
#if NEVER
        return 0;
#else
        return _value;
#endif
    }
}
```

The excluded branch does not name `Value`, so the fix is offered and leaves that branch as it is.

### SHARED-FIX-004 — The fixes of two rules do not undo each other

The fixes of two rules MUST NOT undo each other: when the fix of one rule produces code that another rule reports, the fix of that other rule MUST NOT restore the code that the first fix started from.

Why: Two fixes that undo each other leave no text that satisfies both rules. The user, or a tool that applies every fix, alternates between them forever. Leaving another rule's diagnostic behind is outside this spec, but undoing another rule's correction never converges.

Example — the RH2005 fix makes a field private:

```csharp
public class C
{
    public string {|RH2005:first|};
}
```

```csharp
public class C
{
    private string {|RH4106:first|};
}
```

RH4106 reports the private field, and its fix renames it and keeps it `private`:

```csharp
public class C
{
    private string _first;
}
```

The RH4106 fix does not restore the code the RH2005 fix started from.

### SHARED-FIX-005 — An offered fix changes the document

A code fix MUST NOT be offered when its result would equal the code it is applied to.

Why: An action that changes nothing looks like a correction but leaves the diagnostic in place. The user applies it again and again without reaching a clean state, and Fix All reports success without having fixed anything.

Example — RH4106:

```csharp
public class C
{
    private int {|RH4106:_1st|};
}
```

is reported, and no code fix is offered, because the name the fix derives is `_1st` again.
