# Shared — Attribute target rules

| | |
|---|---|
| ID prefix | SHARED-ATTR |
| Status | Active |
| Code owners | `AttributeTargetRuleAnalyzerBase`, `TargetAttributePlacementAnalyzerBase`, `TargetAttributePlacementCodeFixProviderBase` |
| Uses | FMT-PIPE-007, FMT-BREAK-001, FMT-BREAK-024, FMT-BREAK-027 |

This spec covers the rules that check attribute lists by the target the attributes apply to: how the target of an attribute list is resolved, how a rule decides whether a list is placed on its own line or on the line of what follows it, where the diagnostic is, and what the placement fix produces. Each rule that uses this spec names its target, the declarations that carry it, its placement, and any exception of its own. The formatter's counterpart is FMT-BREAK-027.

## Non-goals

- Which target a rule checks, whether it requires the list on its own line or on the line of what follows, and its own exceptions — each rule's spec.
- How many attributes one list holds, and whether lists are split or merged — the attribute list-shape rules of the RH55xx range.
- Attribute lists that resolve to the type-parameter target — no placement rule checks them.
- Comments between an attribute list and its declaration — RH5033; comments inside an attribute list — RH5044.
- The layout of the rest of the declaration the fix touches, such as the accessor list around a reported accessor — the other layout rules and the formatter.

## Requirements

### SHARED-ATTR-001 — Without a specifier, the declaration decides the target

An attribute list without a target specifier, or with the `type:` specifier, MUST resolve to the target of the declaration it is attached to:

- `class`, `record`, and `record class` — the class target;
- `struct` and `record struct` — the struct target;
- `interface` — the interface target; `enum` — the enum target; `delegate` — the delegate target;
- method, operator, conversion operator, destructor, local function, lambda, and accessor — the method target;
- constructor — the constructor target;
- property and indexer — the property target;
- field and enum member — the field target;
- event and event field — the event target;
- parameter — the parameter target; type parameter — the type-parameter target.

A rule MUST check only the lists that resolve to its target.

Why: These are the targets the compiler applies such an attribute to, so the rule that checks a list is the one named after what the attribute decorates. A `record` is a class and a `record struct` a struct, so they follow the rules of those declarations. `type:` names the default target of a type declaration; on any other declaration the compiler ignores it, and the list is laid out like every other list of that declaration.

Example — RH5505, which checks the class target:

```csharp
{|RH5505:[A]|} record R;

{|RH5505:[type: A]|} class D
{
}

[A] record struct S;
```

`S` is checked by RH5507, not by RH5505.

Example — RH5513, which checks the method target:

```csharp
class C
{
    {|RH5513:[type: A]|} void M()
    {
    }
}
```

### SHARED-ATTR-002 — A specifier names the target

An attribute list with the specifier `assembly:`, `module:`, `field:`, `event:`, `method:`, `property:`, or `return:` MUST resolve to the target the specifier names, whatever declaration it is attached to, except a parameter or a type parameter (SHARED-ATTR-003). An attribute list with a specifier that is not a C# attribute target MUST resolve to no target and MUST NOT be checked by any rule.

Why: The specifier states what the attribute applies to, so a `[return: …]` list on a method is an annotation of the result, not of the method, and is checked by the rule of the return value. A list with an unknown specifier does not compile as intended; no rule can tell what it decorates.

Example — RH5527, which checks the return-value target:

```csharp
class C
{
    {|RH5527:[return: A]|} int M() => 0;

    [A] int N() => 0;
}
```

The list `[A]` on `N` is checked by RH5513, not by RH5527.

Example — RH5517, which checks the field target:

```csharp
class C
{
    {|RH5517:[field: A]|} int P { get; set; }
}
```

Example:

```csharp
class C
{
    [foo: A] void M()
    {
    }
}
```

is not reported by RH5513.

### SHARED-ATTR-003 — Lists of parameters and type parameters follow their owner

An attribute list attached to a parameter MUST resolve to the parameter target, and one attached to a type parameter to the type-parameter target, whether it has no specifier, a specifier of SHARED-ATTR-002, or the specifier `param:` or `typevar:` that names that same target.

Why: A list inside a parameter list or a type-parameter list sits where only the inline layout of a parameter is valid. A `[property: …]` list on a positional record parameter applies to the generated property, but laying it out like a property attribute would split the parameter list.

Example — RH5523, which checks the parameter target:

```csharp
record R({|RH5523:[property: A]|}
         int Id);
```

Example:

```csharp
record R([property: A] int Id);
```

is not reported by RH5515.

### SHARED-ATTR-004 — The token after the list is compared with its closing bracket

A rule MUST compare the line of an attribute list's closing bracket with the line of the first token after it. A rule that requires the list on its own line MUST report the list when both are on the same line. A rule that requires the list on the line of what follows MUST report the list when that token starts on a later line. The first token after the list is the opening bracket of the next attribute list when one follows; for a list that spans several lines, the line of its closing bracket counts.

Why: A declaration attribute ends its line so that the declaration it decorates starts on its own line (FMT-BREAK-027), while a parameter attribute stays in front of the parameter it belongs to. Comparing with the next token, even when that is another list, checks every list of a declaration on its own.

Example — RH5513, which requires the list on its own line:

```csharp
class C
{
    {|RH5513:[A]|} {|RH5513:[B]|} void M()
    {
    }

    {|RH5513:[A(
        1)]|} void N()
    {
    }

    [A]
    [B]
    void O()
    {
    }

    [A(
        1)]
    void P()
    {
    }
}
```

Example — RH5523, which requires the list on the line of what follows:

```csharp
class C
{
    void M({|RH5523:[A]|}
           int a, [B] int b)
    {
    }
}
```

### SHARED-ATTR-005 — A comment or directive after the list blocks the report

A rule MUST NOT report an attribute list when a comment, a preprocessor directive, or disabled text lies between its closing bracket and the first token after it, and the code fix MUST NOT be offered for such a list.

Why: Both placements rewrite exactly that gap, so a fix would have to drop the comment or directive or move the token into it. The formatter does not join or split across them either (FMT-BREAK-001), so a diagnostic there could never be cleared.

Example:

```csharp
[A] /* c */ class C
{
}
```

is not reported by RH5505.

Example:

```csharp
class C
{
    void M([A] // c
           int a,
           [B]
#if DEBUG
#endif
           int b)
    {
    }
}
```

is not reported by RH5523.

### SHARED-ATTR-006 — Accessor lists of an inline auto-property stay on its line

A rule that requires the list on its own line MUST NOT report an attribute list of an accessor when the property or indexer that owns the accessor is on one line, apart from its own attribute lists, no accessor of it has a body or an expression body, and its accessor list contains no comment or directive. A property attribute list on a line of its own and a comment after the accessor list do not change this.

Why: Such a property is a one-line declaration whose accessor attributes stay as written (FMT-BREAK-024); breaking the line after an accessor attribute would turn it into a multi-line property for a single marker. Once an accessor has a body, or a comment sits between the accessors, the accessor list is laid out over several lines anyway.

Example — RH5527:

```csharp
class C
{
    int P { [return: A] get; set; }

    [B]
    int Q { [return: A] get; set; } = 1; // c

    int R { {|RH5527:[return: A]|} get => 1; }

    int S { {|RH5527:[return: A]|} get; /* c */ set; }

    int T
    {
        {|RH5527:[return: A]|} get;
        set;
    }
}
```

### SHARED-ATTR-007 — The diagnostic is on the attribute list

A rule MUST report one diagnostic per misplaced attribute list, located on the whole list from its opening to its closing bracket.

Why: The list is the unit whose placement is wrong, and each list of a declaration is placed and fixed on its own.

Example: see SHARED-ATTR-004.

### SHARED-ATTR-008 — The separate-line fix breaks the line after the list

The code fix of a rule that requires the list on its own line MUST replace the gap between the list's closing bracket and the first token after it with a line break and indent that token with spaces: four for each namespace, type, enum, block, switch statement, or accessor list whose braces enclose the list, and four more inside the statements of a switch section. The indentation MUST NOT depend on the column of the list or on the indentation of other lines. The inserted line break MUST use the line ending detected as FMT-PIPE-007 specifies for the formatter.

This requirement covers a list outside every initializer and anonymous object (SHARED-ATTR-009) whose innermost enclosing opening brace is itself indented four spaces for each scope that encloses that brace.

Why: The fix makes only the change the diagnostic asks for. The indentation is the one the declaration has at its nesting depth, so a stray space or tab in front of the list is not copied onto the declaration.

Example — RH5511:

```csharp
class C
{
        {|RH5511:[A]|} C()
    {
    }
}
```

```csharp
class C
{
        [A]
    C()
    {
    }
}
```

Example — RH5513, applied to both diagnostics:

```csharp
namespace N
{
    class C
    {
        {|RH5513:[A]|} {|RH5513:[B]|} void M()
        {
        }
    }
}
```

```csharp
namespace N
{
    class C
    {
        [A]
        [B]
        void M()
        {
        }
    }
}
```

### SHARED-ATTR-009 — Inside an initializer, the fix aligns with the declaration

When an initializer — object, collection, array, `with`, or complex-element initializer — or an anonymous object encloses the attribute list, the code fix of SHARED-ATTR-008 MUST instead indent the first token after the list with spaces to the column of the first token of the declaration the list belongs to.

This requirement covers a declaration whose line holds no tab character in front of its first token.

Why: An initializer is aligned to its anchor rather than indented by nesting depth, so no depth gives the right column there. The declaration's own first token is already where the declaration starts, and aligning to it keeps every list of the declaration and the declaration in one column.

Example — RH5513:

```csharp
class C
{
    D _d = new D
    {
        H = () =>
        {
            {|RH5513:[A]|} void L()
            {
            }
        }
    };
}
```

```csharp
class C
{
    D _d = new D
    {
        H = () =>
        {
            [A]
            void L()
            {
            }
        }
    };
}
```

Example — RH5513, in an array initializer:

```csharp
class C
{
    System.Action[] _a =
    {
        () =>
        {
                {|RH5513:[A]|} void L()
                {
                }
        }
    };
}
```

```csharp
class C
{
    System.Action[] _a =
    {
        () =>
        {
                [A]
                void L()
                {
                }
        }
    };
}
```

### SHARED-ATTR-010 — The same-line fix joins the list and what follows

The code fix of a rule that requires the list on the line of what follows MUST replace the gap between the list's closing bracket and the first token after it, line breaks and blank lines included, with a single space.

Why: The fix makes only the change the diagnostic asks for and leaves the rest of the parameter list as written.

Example — RH5523:

```csharp
class C
{
    void M({|RH5523:[A]|}

           {|RH5523:[B]|}
           int a)
    {
    }
}
```

```csharp
class C
{
    void M([A] [B] int a)
    {
    }
}
```

### SHARED-ATTR-011 — The fix is withheld for a list with a comment or directive inside

The code fix MUST NOT be offered when a comment or a preprocessor directive lies inside the attribute list, between its opening and its closing bracket. A comment in front of the list, such as a documentation comment, does not withhold it. The diagnostic is still reported.

Why: The placement of a list with a comment inside is reported like any other, but the comment belongs to the attribute it sits next to, and only the author can tell whether the new layout still reads right.

Example — RH5505:

```csharp
{|RH5505:[A /* c */]|} class C
{
}
```

is reported, and no code fix is offered.

Example — RH5513:

```csharp
class C
{
    /// <summary>M</summary>
    {|RH5513:[A]|} void M()
    {
    }
}
```

```csharp
class C
{
    /// <summary>M</summary>
    [A]
    void M()
    {
    }
}
```

### SHARED-ATTR-012 — The separate-line fix is withheld when an enclosing brace is missing

The code fix of a rule that requires the list on its own line MUST NOT be offered when a block, type, namespace, enum, switch statement, accessor list, initializer of any kind SHARED-ATTR-009 names, or anonymous object whose braces enclose the attribute list is missing its opening or closing brace. The diagnostic is still reported.

Why: The indentation of SHARED-ATTR-008 counts the enclosing braces; a brace the parser could not find drops a level silently, which is common in code that is being typed, and the fix would indent the declaration wrongly.

Example — RH5511:

```csharp
class C
{
    {|RH5511:[A]|} C()
    {
    }
```

is reported, and no code fix is offered.
