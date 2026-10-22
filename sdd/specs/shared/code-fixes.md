# Shared — Code fixes

| | |
|---|---|
| ID prefix | SHARED-FIX |
| Status | Active |
| Code owners | — |
| Uses | FMT-PIPE-007, SHARED-DIAG-008, SHARED-DIAG-010, SHARED-JOIN-001, SHARED-ORDER-002 |

This spec covers what holds for every code fix of an analyzer rule, so a rule spec states only what its own fix produces and when it is withheld. It applies to every rule whose header names a code fix, without being listed under `Uses`; SHARED-FIX-002 applies to the rules whose header says `Fix All`.

## Non-goals

- What a fix produces and when it is withheld — each rule's own spec.
- Whether a fix leaves diagnostics of other rules in the code it changed. It is a known failure class (`squad/failure-classes.md`), not specified behavior; SHARED-FIX-004 only forbids a fix that undoes the fix of another rule.
- How the formatter's node-level entry points keep the code around their target — FMT-PIPE-010.
- The line ending a fix writes into a document that has no line break, or whose line breaks already mix line endings.

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

A code fix that offers Fix All MUST correct, in one Fix All application to a document, a project, or a solution, every reported diagnostic whose fix is offered on the code the application starts from, and the result MUST equal the result of applying those single fixes one after another. A diagnostic whose fix a guard withholds on that code, and releases only after another fix has run, is not covered: it may still be reported after the application, and its fix is offered on the result once the guard releases it.

Why: The user applies Fix All expecting one step that leaves nothing behind. Requiring the same result as the single fixes makes the outcome independent of the order in which the diagnostics are fixed. A fix that its guard withholds is not offered on the starting code, so it is not among the fixes the user asked to apply; running fixes until every guard releases would apply a fix nobody was offered.

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

Example — a fix that a guard releases only after another fix has run, for RH7103: the example "another run-time change" under SHARED-FIX-003. On its first block, only `_a` has a fix; the guard of SHARED-FIX-003 withholds the fix for `_b`, because moving `_b` in front of `_a` would change its initial value. One Fix All application gives its second block, which is the result of the one single fix offered. In that result, `_b` is still reported and is offered its fix.

### SHARED-FIX-003 — A fix that could break the code is withheld

A code fix MUST NOT be offered when its result would:

- have a compile error that the code did not have;
- change what the program does at run time;
- change the text of a comment, a directive, or a literal that the user wrote; or
- leave code that an `#if` branch excludes referring to a name the fix changed.

The diagnostic is still reported: withholding a fix never removes a report, also for an input where the fix is withheld because no correction compiles or keeps what the program does, for example `typeof(Nullable<>)` or `x is !true`. Which inputs a rule reports at all is decided by its own requirements and by the requirements that name inputs a rule does not report, such as SHARED-DIAG-008, SHARED-DIAG-010, and SHARED-JOIN-001; this requirement adds none. The second and third conditions are bounded as follows:

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
    private static int {|RH7103:_a|} = 1;
    private static int {|RH7103:_b|} = _a;
}
```

```csharp
public class C
{
    private static int _a = 1;
    private int _instance;
    private static int {|RH7103:_b|} = _a;
}
```

Both fields are reported. For `_b`, no code fix is offered: moving it in front of `_a` would run its initializer first, so `_b` would hold `0` instead of `1`. For `_a`, the fix is offered: it crosses only the instance field `_instance`, whose initialization runs at a different time, and moving a static field changes no layout. The second block is the result of the fix for `_a`, in which `_b` is still reported.

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
    private int _value;


    public int GetValue() {|RH3202:=> _value|};

    public string N() {|RH3202:=>
        """
        a
        """|};
}
```

```csharp
public class C
{
    private int _value;


    public int GetValue()
    {
        return _value;
    }

    public string N()
    {
        return """
               a
               """;
    }
}
```

The fix changes the indentation of the raw string literal in `N`; the literal's value stays the same. The two blank lines above `GetValue` stay two blank lines: they are outside the node the fix replaces (SHARED-FIX-006).

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

### SHARED-FIX-006 — A fix changes only the code its diagnostic is about

A code fix MUST change only the code its diagnostic is about; the layout of the code around it MUST stay as it was. In particular:

- **Blank lines.** A fix MUST NOT insert a blank line before the node it changes, and MUST NOT change the blank lines between that node and its neighbours, unless those blank lines are the code its diagnostic is about. The blank line that a fix leaves by removing a directive line is handled as SHARED-FIX-007 states.
- **Shared line.** A member that shares its line with the member a fix changes, after the fix, is handled as SHARED-FIX-007 states.
- **Formatting.** A fix MUST format only the node it replaces, with the whitespace of the lines it edits. It MUST NOT format the member, the statement, or the type that contains that node.

Why: The user applies a fix to correct one finding. A change to code the diagnostic does not name is a second edit nobody asked for: it hides the correction in an unrelated diff, can undo a layout the author chose, and makes a decision that belongs to the rule or the formatter that owns that layout. The blank lines between a node and its neighbours are such a layout, and formatting the member, statement, or type around the node reaches all of the code around it.

Example — the RH6024 fix:

```csharp
using System;

class C
{
    void N(int a)
    {
        _ = a{|RH6024:+|}M(a, () => { return 1; });
    }

    int M(int a, Func<int> f)
    {
        return a;
    }
}
```

```csharp
using System;

class C
{
    void N(int a)
    {
        _ = a + M(a, () => { return 1; });
    }

    int M(int a, Func<int> f)
    {
        return a;
    }
}
```

The fix changes only the two gaps next to `+`. The lambda stays on its line, although formatting the document would break it over several lines.

Example — blank lines, for RH3202:

```csharp
class C
{
    private int _value;

    public int GetValue() {|RH3202:=> _value|};
}
```

```csharp
class C
{
    private int _value;

    public int GetValue()
    {
        return _value;
    }
}
```

The blank line between `_value` and `GetValue` stays, and no blank line is inserted. A blank line is the code a diagnostic is about where its rule reports the missing or extra blank line: the RH5002 fix under SHARED-FIX-001 inserts the blank line in front of `if`.

Example — formatting only the replaced node, for RH3203:

```csharp
using System;

class C
{
    private int _value;

    ///<summary>x</summary>
    [Obsolete] public  C( int value ) {|RH3203:=> _value = value|};
}
```

```csharp
using System;

class C
{
    private int _value;

    ///<summary>x</summary>
    [Obsolete] public  C( int value )
    {
        _value = value;
    }
}
```

The fix replaces the expression body and its semicolon with a block, indented from the line it edits. The documentation comment, the attribute on the constructor's line, and the spacing of the signature stay as they were. The layout example under SHARED-FIX-003 shows the same for the blank lines above a converted method.

### SHARED-FIX-007 — The changed code is laid out as the formatter lays it out

The code a fix changes MUST be laid out as the formatter lays it out, in one application, so that formatting the result changes nothing in that code. Code that the fix only moves is laid out as the moved-code bullet states. In particular:

- **Moved code.** Code that a fix moves to another position or into another block without changing it — a member moved in front of another, a region block moved with its members, the statements a fix wraps into a new block — MUST keep its own layout: its braces, the spacing between its tokens, its line breaks, and the blank lines inside it stay as written, also where formatting the result would change them. Only the indentation of its lines MUST follow its new nesting. The separators in front of and behind it, at its old and at its new position, follow SHARED-ORDER-002. Code the fix creates around the moved code, such as the header and the braces of a new block, is laid out as the formatter lays it out. The bullet covers members, accessors, and region blocks that a fix moves, and statements that a fix wraps into a new block, such as the following statements of a `using` declaration (RH7004) or the embedded statement of an `if`, `else`, or loop that a brace fix encloses. It does not cover an expression that a fix turns into a new statement, such as the `return` statement of an expression body converted to a block, which is the node the fix replaces, nor a using block that a fix rewrites, whose group separators are the subject of the using-directive rules.
- **Shared line.** When the member a fix changes shares its line with another member after the fix, the changed member MUST get its own line and the other member MUST start on the next line; the lines the fix edits are laid out as the formatter lays them out. When the other member comes first on the line, the changed member starts on the next line. Around a member that has its line to itself after the fix, the line breaks and blank lines stay as SHARED-FIX-006 states.
- **Removed directive line.** A fix that removes a region or conditional directive line MUST also remove the blank line that the removal leaves at the start of a body, at the end of a body, or doubled between two lines, and MUST NOT insert a blank line. A single blank line that the removal leaves between two lines stays.
- **Blank lines.** The blank lines between the node a fix changes and its neighbours follow SHARED-FIX-006 and the removed-directive bullet above, also where formatting the result would insert or remove one.

Why: A fix that leaves the code it changed in a shape the formatter changes again hands the user a second diff for the same correction, on lines the user has just reviewed, and leaves layout that the layout rules report. A member that shares its line with another member after the fix, and a blank line that a removal leaves at the edge of a body or next to another blank line, are such shapes, whoever wrote the lines around them. The blank lines between the changed node and its neighbours stay a layout the author chose: a blank line inserted or removed only because the formatter would want it is a decision the diagnostic did not name. Code that a fix moves is code the author wrote and laid out, and its layout is not what the diagnostic names: reformatting it on the way hides the move in a diff of layout changes nobody asked for, as formatting the code around a changed node would (SHARED-FIX-006), while the formatter and the layout rules still correct it on their own. Its indentation has to follow, because the same lines at the old depth would be misindented at the new one, and the separators belong to the positions, not to the code that moves between them.

Example — the RH5101 fix:

```csharp
class C
{
    void N()
    {
        M(
            {|RH5101:"""
            a
            """|},
            2);
    }

    void M(string s, int i)
    {
    }
}
```

```csharp
class C
{
    void N()
    {
        M("""
          a
          """,
          2);
    }

    void M(string s, int i)
    {
    }
}
```

The raw string literal moves onto the line of the opening parenthesis, and its content and its closing quotes are at the column that formatting the document gives them, under the opening quotes. Formatting the result changes nothing in the argument list.

Example — a shared line, for RH5417:

```csharp
class C
{
    void M() {|RH5417:{|} } int _x;

    int _y; void P() {|RH5417:{|} }

    int N() {|RH5417:{|} return 1; }
}
```

```csharp
class C
{
    void M()
    {
    }
    int _x;

    int _y;
    void P()
    {
    }

    int N()
    {
        return 1;
    }
}
```

After the fix, the closing brace of `M` would share its line with `_x`, so `_x` starts on the next line. `P` would share its line with `_y`, which comes first, so `P` starts on the next line. `N` has its line to itself, and the blank line above it stays.

Example — a removed region at the start and at the end of a body, for RH7310:

```csharp
class C
{

    {|RH7310:#region Fields|}
    #endregion // Fields
    int _a;

    {|RH7310:#region Methods|}

    #endregion // Methods
}
```

```csharp
class C
{
    int _a;
}
```

Removing the `Fields` region leaves a blank line at the start of the body, and removing the `Methods` region leaves one at its end; both go.

Example — a removed region between two members, for RH7310:

```csharp
class C
{
    int _a;

    {|RH7310:#region Fields|}
    #endregion // Fields
    int _b;

    {|RH7310:#region Methods|}

    #endregion // Methods

    int _c;
}
```

```csharp
class C
{
    int _a;

    int _b;

    int _c;
}
```

Removing the `Fields` region leaves one blank line between `_a` and `_b`, which stays. Removing the `Methods` region leaves two blank lines between `_b` and `_c`, of which one goes.

Example — a removed region between two statements, for RH7303:

```csharp
class C
{
    void M()
    {
        var x = 1;
        {|RH7303:#region Work|}
        x++;
        {|RH7303:#endregion // Work|}
        x--;
    }
}
```

```csharp
class C
{
    void M()
    {
        var x = 1;
        x++;
        x--;
    }
}
```

No blank line is left by the removal, and none is inserted, although formatting the result would insert one after `var x = 1;`.

Example — moved code keeps its layout, for RH7309:

```csharp
using System;

public class C : IDisposable
{
    #region IDisposable

    public void Dispose() { }

    #endregion // IDisposable

    {|RH7309:#region Fields|}

    private int _a;


    private int _b;

    #endregion // Fields
}
```

```csharp
using System;

public class C : IDisposable
{
    #region Fields

    private int _a;


    private int _b;

    #endregion // Fields

    #region IDisposable

    public void Dispose() { }

    #endregion // IDisposable
}
```

The two region blocks change places as they were written: the two blank lines between `_a` and `_b` and the braces of `Dispose` on its line stay, although formatting the result would change both.

Example — moved code is only re-indented, for RH7004:

```csharp
using System.IO;

class C
{
    void M()
    {
        {|RH7004:using|} var s = new MemoryStream();
        s.Flush( );
        var n = 1;
        return;
    }
}
```

```csharp
using System.IO;

class C
{
    void M()
    {
        using (var s = new MemoryStream())
        {
            s.Flush( );
            var n = 1;
            return;
        }
    }
}
```

The statements that move into the new block are indented one level deeper and otherwise stay as written: the spaces inside `Flush( )` stay, and no blank line is inserted between them. The `using` statement the fix creates, with its braces, is laid out as the formatter lays it out.

### SHARED-FIX-008 — A fix keeps the document's line ending

A code fix MUST write only the line ending the document already uses: every line break the fix creates or keeps MUST be that line ending, in a document whose line breaks are CRLF as in one whose line breaks are LF. A line break inside a literal or inside the text of a comment other than a documentation comment is content under SHARED-FIX-003; it stays as it is and does not count when deciding which line ending the document uses, as FMT-PIPE-007 does not count it.

Why: A fix that introduces the other line ending leaves the file with mixed line endings, which the formatter then rewrites.

Example — the RH5405 fix in a document whose line breaks are all CRLF:

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a) {|RH5405:M(a, b);|}
    }
}
```

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a)
        {
            M(a, b);
        }
    }
}
```

Every line break of the result is CRLF, including the three the fix creates. In a document whose line breaks are all LF, the same fix creates LF line breaks.
