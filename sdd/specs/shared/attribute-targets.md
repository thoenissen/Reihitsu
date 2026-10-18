# Shared — Attribute target rules

| | |
|---|---|
| ID prefix | SHARED-ATTR |
| Status | Active |
| Code owners | `AttributeTargetRuleAnalyzerBase`, `TargetAttributePlacementAnalyzerBase`, `TargetAttributePlacementCodeFixProviderBase`, `TargetAttributeListShapeAnalyzerBase`, `TargetAttributeListShapeCodeFixProviderBase`, `AttributeTargetUtilities` |
| Uses | FMT-PIPE-007, FMT-BREAK-001, FMT-BREAK-024, FMT-BREAK-027 |

This spec covers the rules that check attribute lists by the target the attributes apply to: how the target of an attribute list is resolved, how a placement rule decides whether a list is placed on its own line or on the line of what follows it, how a list-shape rule decides whether a list holds one attribute or the lists of one target are merged, where the diagnostic is, and what the placement and list-shape fixes produce. Each rule that uses this spec names its target, the declarations that carry it, its placement or its list shape, and any exception of its own. The formatter's counterpart is FMT-BREAK-027.

## Non-goals

- Which target a rule checks, whether it requires the list on its own line or on the line of what follows, whether it requires one attribute per list or one list per target, and its own exceptions — each rule's spec.
- The placement of attribute lists that resolve to the type-parameter target — no placement rule checks them.
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

### SHARED-ATTR-013 — A list with several attributes is reported

A rule that requires one attribute per list MUST report every attribute list of its target that holds more than one attribute and is not attached to an accessor, whether the list is on its own line or shares its line with other code, and whatever comments surround it. A list attached to an accessor is reported as SHARED-ATTR-014 specifies. It MUST NOT report a list that holds one attribute.

Why: On a declaration every attribute gets a line of its own (FMT-BREAK-027), so a change to one attribute changes exactly one line. A list with several attributes ties unrelated attributes to one line; where the list is placed is the placement rule's decision, not this one's.

Example — RH5514:

```csharp
class C
{
    {|RH5514:[A, B]|}
    void M()
    {
    }

    {|RH5514:[A, B]|} void N()
    {
    }

    [A]
    [B]
    void O()
    {
    }
}
```

### SHARED-ATTR-014 — Accessor lists of an inline auto-property are not split

A rule that requires one attribute per list MUST NOT report an attribute list of an accessor whose lists SHARED-ATTR-006 exempts from the separate-line placement. When the property, indexer, or event that owns the accessor spans more than one line, apart from its own attribute lists, the rule MUST report a list of the accessor that holds more than one attribute, as SHARED-ATTR-013 specifies for other lists.

Why: Such an accessor list stays on the property's line (FMT-BREAK-024), so a second list would sit on the same line and only repeat the brackets; one list there says the same in less space. In an accessor list over several lines, each attribute has a line of its own to take, as on any other declaration.

Example — RH5528:

```csharp
class C
{
    int P { [return: A, B] get; set; }

    int R
    {
        {|RH5528:[return: A, B]|}
        get;
        set;
    }
}
```

### SHARED-ATTR-015 — The lists of one target are merged into the first

A rule that requires one list per target MUST report every attribute list of its target that follows another list of the same declaration naming the same target, and MUST NOT report the first such list or a list that is the only one of its target on the declaration. A list names the target its specifier names (SHARED-ATTR-002), with `param:` naming the parameter target and `typevar:` the type-parameter target; a list without a specifier names the target of the declaration it is attached to (SHARED-ATTR-001). A list with the `type:` specifier on a declaration other than a type or a delegate MUST NOT be reported and MUST NOT count as a list of any target. Lists of other targets between them do not separate the lists of one target.

Why: The compiler applies every list of one target alike, so separate lists only repeat the brackets. Lists that name different targets apply to different things, and merging them would move an attribute from one target to the other. The compiler ignores a `type:` list on such a declaration, so merging it would start applying its attributes.

Example — RH5524:

```csharp
record R([property: A] [B] {|RH5524:[property: D]|} int Id);

class C
{
    void M([A] {|RH5524:[B]|} {|RH5524:[param: D]|} int a, [A] int b, [A, B] int c, [A] [type: B] int d)
    {
    }
}
```

### SHARED-ATTR-016 — The list-shape diagnostic is on the attribute list

A list-shape rule MUST report one diagnostic per list it reports (SHARED-ATTR-013, SHARED-ATTR-015), located on the whole list from its opening to its closing bracket.

Why: The list is the unit that has the wrong shape. For lists that must be merged, the first list of a target is the one the others merge into, so it carries no diagnostic.

Example: see SHARED-ATTR-013 and SHARED-ATTR-015.

### SHARED-ATTR-017 — The split fix gives each attribute a list on its own line

The code fix of a rule that requires one attribute per list MUST replace the reported list with one list per attribute, in the order the attributes were written, each list with the reported list's specifier. Each list after the first MUST start a new line at the indentation of the reported list. A comment in front of the reported list stays in front of the first list, a comment after it on its line stays after the last list, and nothing else changes. The inserted line breaks MUST use the line ending detected as FMT-PIPE-007 specifies for the formatter.

This requirement covers a list for which all of the following hold:

- the list is the first token on its line, after its closing bracket that line holds nothing or only comments, and the first token after the list starts a later line;
- every other line between the code before the list, or the start of the file, and the first token after the list is either empty, without even a space, or holds only comments and has its first character other than a space at the list's column; each line of a `///` documentation comment counts as a line of its own, while the continuation lines of a `/* */` or `/** */` comment may start at any column;
- no preprocessor directive or disabled text lies between the code before the list and the first token after the list;
- no line from the line of the code before the list to the line before the first token after the list ends in a space or tab that is not part of a comment;
- the list, the first token after it, and the line of every opening brace that encloses the list are indented with spaces only, four for each namespace, type, enum, block, switch statement, accessor list, initializer, or anonymous object whose braces enclose them, and four more inside the statements of a switch section.

Why: The fix makes only the change the diagnostic asks for: the attributes keep their order and their target, and each gets the line the rule requires.

Example — RH5514, applied to both diagnostics:

```csharp
namespace N
{
    class C
    {
        /// <summary>M</summary>
        {|RH5514:[A, B]|}
        void M()
        {
        }

        {|RH5514:[method: A, B]|} // c
        void O()
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
        /// <summary>M</summary>
        [A]
        [B]
        void M()
        {
        }

        [method: A]
        [method: B] // c
        void O()
        {
        }
    }
}
```

Example — RH5514, inside a switch section:

```csharp
class C
{
    void M(int i)
    {
        switch (i)
        {
            case 1:
                {|RH5514:[A, B]|}
                void L()
                {
                }
                break;
        }
    }
}
```

```csharp
class C
{
    void M(int i)
    {
        switch (i)
        {
            case 1:
                [A]
                [B]
                void L()
                {
                }
                break;
        }
    }
}
```

### SHARED-ATTR-018 — The merge fix moves the attributes into the first list

The code fix of a rule that requires one list per target MUST merge every list of the reported list's target on that declaration (SHARED-ATTR-015) into the first of them, so that one application clears every diagnostic of that target on the declaration. The first list keeps its place and its specifier, holds the attributes of all merged lists in the order they were written, and is followed by one space. The other merged lists are removed together with the whitespace and line breaks around them. A token that followed a merged list — a list of another target or the declaration itself — MUST move onto the line of the code before it, with one space in between, unless that code still ends its line; then the token keeps its line breaks and indentation.

Why: The fix makes only the change the diagnostic asks for, for all lists of the target at once: a merge that left one list behind would still be reported. Removing the merged lists must not leave a lone line break behind in front of the parameter, and a token whose line still starts after a line break is left as written.

Example — RH5524:

```csharp
class C
{
    void M([A] {|RH5524:[B]|} int a,
           [A]
           {|RH5524:[B]|}

           int b,
           [A]{|RH5524:[D]|}{|RH5524:[B]|} int c)
    {
    }
}
```

```csharp
class C
{
    void M([A, B] int a,
           [A, B] int b,
           [A, D, B] int c)
    {
    }
}
```

Example — RH5524, with a list of another target in the group:

```csharp
record R([property: A]
         [D]
         {|RH5524:[property: B]|}
         int Id);
```

```csharp
record R([property: A, B] [D]
         int Id);
```

### SHARED-ATTR-019 — The split fix is withheld for a list with a comment or directive inside

The code fix of a rule that requires one attribute per list MUST NOT be offered when a comment or a preprocessor directive lies inside the reported list, between its opening and its closing bracket. A comment in front of the list or after it does not withhold it (SHARED-ATTR-017). The diagnostic is still reported.

Why: The split rewrites the separators between the attributes, so a comment inside the list would have to be moved to one of the new lists, and a directive inside it would end up off its own line; only the author can tell where it belongs.

Example — RH5506:

```csharp
{|RH5506:[A, /* c */ B]|}
class C
{
}

{|RH5506:[A,
#if DEBUG
#endif
 B]|}
class E
{
}
```

is reported, and no code fix is offered.

### SHARED-ATTR-020 — The merge fix is withheld for a comment or directive around the group

The code fix of a rule that requires one list per target MUST NOT be offered when a comment, a preprocessor directive, or disabled text lies between the opening bracket of the first list it would merge and the first token after the last of them, or when a directive lies directly in front of the first of them. A comment in front of the first list or after the token that follows the last list does not withhold it. The diagnostic is still reported.

Why: The merge rewrites the whole stretch from the first to the last list and joins the token after it, so a comment there would be dropped or moved and a directive would end up off its own line, which does not compile. A directive in front of the first list belongs to the line the merge rewrites as well.

Example — RH5524:

```csharp
class C
{
    void M([A] /* c */ {|RH5524:[B]|} int a)
    {
    }

    void N([A] {|RH5524:[B]|} // c
           int a)
    {
    }

    void O([A]
           {|RH5524:[B]|}
           /* c */ int a)
    {
    }

    void P(
#if true
           [A] {|RH5524:[B]|} int a)
#endif
    {
    }
}

record R([property: A] [B /* c */] {|RH5524:[property: D]|} int Id);
```

is reported, and no code fix is offered.

Example — RH5524:

```csharp
class C
{
    void M(
           // c
           [A] {|RH5524:[B]|} int a,
           [A] {|RH5524:[B]|} int b /* c */)
    {
    }
}
```

```csharp
class C
{
    void M(
           // c
           [A, B] int a,
           [A, B] int b /* c */)
    {
    }
}
```
