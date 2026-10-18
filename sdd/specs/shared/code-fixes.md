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

- The corrected code is everything the rule reports at that location, such as a whole run of blank lines, not only the run's first line.
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

is not reported again.

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
class C
{
    void M()
    {
{|RH5022:
|}{|RH5023:
|}        M();
    }
}
```

RH5022 reports the first blank line below `{`, and RH5023 reports the second. The RH5023 fix, applied once, leaves one blank line below `{`. RH5023 does not report that line; RH5022 still does, and a diagnostic of another rule left behind is outside this spec (see Non-goals). The RH5022 fix, applied once, MUST leave no RH5022 diagnostic in the run: it removes the whole run, and does not rely on the RH5023 fix to remove the second line.

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
- change what the program does at run time — the new name or the new accessibility that the fix exists to produce does not count, but any other difference in behavior does;
- change the text of a comment, a directive, or a literal that the user wrote, except text that a diagnostic of the fix's own rule reports; or
- leave code that an `#if` branch excludes referring to a name the fix changed.

The diagnostic is still reported.

Why: A user applies a fix trusting that it only corrects what the diagnostic names. A fix that breaks the build, changes behavior, or edits text the user wrote as content hands the user a defect in exchange for a warning. Code excluded by `#if` compiles in another configuration, so a rename that misses it breaks that build. A diagnostic without a fix is a supported end state; a broken fix is not.

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

Both fields are reported. For `_b`, no code fix is offered: moving it in front of `_a` would run its initializer first, so `_b` would hold `0` instead of `1`. For `_a`, the fix is offered, because it crosses only the instance field `_instance`, whose initialization runs at a different time.

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

The fix changes the text of the documentation comment, but only the element its diagnostic reports. Likewise, the RH5604 fix rewrites the line breaks its rule reports, also inside documentation comments, and keeps a line break in the text of a string literal, which its rule does not report.

Example — code excluded by `#if`, for RH4106:

```csharp
public class C
{
    private int {|RH4106:Value|};

    public int M()
    {
#if NEVER
        return Value + 1;
#else
        return Value;
#endif
    }
}
```

is reported, and no code fix is offered, because the renamed field would leave `Value` in the excluded branch. When the excluded code does not name `Value`, the fix is offered.

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

RH4106 reports the private field, and its fix renames it to `_first` and keeps it `private`. The RH4106 fix does not restore the code the RH2005 fix started from.

Example — in

```csharp
class C
{
    int _a;

    #region R

}
#endregion
```

the RH5024 fix deletes the blank line between `#region R` and `}`, RH5032 then reports `#region R`, and the RH5032 fix inserts the line again. The two fixes undo each other, which this requirement forbids.

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

Example — in the file `"class C\n{\n}\n\n"`, an RH5023 fix offered at the end of the file would delete nothing, so it MUST NOT be offered there.
