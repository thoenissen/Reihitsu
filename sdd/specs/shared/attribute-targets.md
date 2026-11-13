# Shared — Attribute target rules

| | |
|---|---|
| ID prefix | SHARED-ATTR |
| Status | Active |
| Code owners | `AttributeTargetRuleAnalyzerBase`, `TargetAttributePlacementAnalyzerBase`, `TargetAttributePlacementCodeFixProviderBase`, `TargetAttributeListShapeAnalyzerBase`, `TargetAttributeListShapeCodeFixProviderBase`, `AttributeTargetUtilities`, `AttributeListMergeSeparators` |
| Uses | FMT-BREAK-001, FMT-BREAK-024, FMT-BREAK-027, FMT-BREAK-032, FMT-INDENT-028, FMT-STRUCT-028, FMT-STRUCT-029, SHARED-FIX-006, SHARED-FIX-008 |

This spec covers the rules that check attribute lists by the target the attributes apply to: how the target of an attribute list is resolved, how a placement rule decides whether a list is placed on its own line or on the line of what follows it, how a list-shape rule decides whether a list holds one attribute or the lists of one target are merged, where the diagnostic is, and what the placement and list-shape fixes produce. Each rule that uses this spec names its target, the declarations that carry it, its placement or its list shape, and any exception of its own. The formatter's counterparts are FMT-BREAK-027 for the placement, and FMT-STRUCT-028, FMT-STRUCT-029, and FMT-BREAK-032 for the list shape.

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

The code fix of a rule that requires the list on its own line MUST replace the gap between the list's closing bracket and the first token after it with a line break and indent that token with spaces: four for each scope level that encloses the list. A scope level is each namespace, type, enum, block, switch statement, or accessor list whose braces enclose the list, and one more inside the statements of a switch section. The indentation MUST NOT depend on the column of the list or on the indentation of other lines. The inserted line break MUST use the line ending SHARED-FIX-008 specifies.

This requirement covers a list outside every initializer and anonymous object (SHARED-ATTR-009) whose innermost enclosing opening brace stands on a line indented four spaces for each scope level, as counted above, that encloses that brace. For any other list outside them, SHARED-ATTR-021 withholds the fix.

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

Why: On a declaration every attribute gets a line of its own (FMT-STRUCT-028), so a change to one attribute changes exactly one line. A list with several attributes ties unrelated attributes to one line; where the list is placed is the placement rule's decision, not this one's.

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

### SHARED-ATTR-017 — The split fix starts each further list on a new line

The code fix of a rule that requires one attribute per list MUST replace the reported list with one list per attribute, in the order the attributes were written, each list with the reported list's specifier, and MUST change nothing else (SHARED-FIX-006). The first list stays where the reported list starts. Each list after the first MUST start a new line, behind the whitespace that stands at the start of the line on which the reported list starts, as written. What stands in front of the reported list on its line — a comment or another attribute list — stays in front of the first list, and what follows the reported list on its line — a comment, whitespace, another attribute list, or the declaration the list belongs to — stays behind the last list. The line breaks between the new lists are the only line breaks the fix writes, and they MUST use the line ending SHARED-FIX-008 specifies.

This holds wherever the list stands: every other line, its indentation, the comments and directives on it, and whitespace at the end of a line stay as written, also where the braces around the list are indented differently from the list, a comment or a directive above or below the list stands in another column, a line ends in whitespace, or the list shares its line with other code, except where SHARED-ATTR-022 withholds the fix for a list on the line of the `{` that opens the body holding its declaration, and where SHARED-ATTR-023 withholds it for a line break the fix would copy or keep in another line ending. The whitespace in front of each further list is a copy of the whitespace at the start of the reported list's line, character for character, also where it holds a tab: the fix neither replaces a tab with spaces nor counts it as a column.

Why: The fix makes only the change the diagnostic asks for: the attributes keep their order and their target, and each further attribute gets a line of its own at the indentation the reported list's line already has. The code around the list is a layout the author chose, and placing the declaration or a neighbouring list on a line of its own is the job of the placement rules, which report it on their own.

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

Example — RH5514, the code around the list stays as written:

```csharp
namespace N
{
class C
{
#pragma warning disable CS0001
// c
    {|RH5514:[A, B]|}
        // d
    void M()
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

The class keeps its braces in column 0, the directive and the comments above and below the list keep their columns, and the new list starts in the column of the reported one. Likewise, `    int _f;   ` above the list, whitespace behind `[A, B]`, and a line that holds only whitespace next to the list keep their whitespace: whitespace behind the reported list stays behind `[B]`.

Example — RH5514, a list that shares its line:

```csharp
class C
{
    [A] {|RH5514:[B, D2]|} void M()
    {
    }

    {|RH5514:[A, B]|} [D2] void N()
    {
    }

    /* c */ {|RH5514:[A, B]|}
    void O()
    {
    }

    [return: A] {|RH5514:[A, B]|} int R() => 0;
}
```

```csharp
class C
{
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
}
```

The list in front of the reported one, the list behind it, the comment, and the declaration stay where they are; the placement rules report the lists and declarations that now share a line, such as RH5513 for `[D2] void M()`. For RH5506, `[A, B] class C` becomes `[A]` and `[B] class C` in the same way, which RH5505 then reports.

Example — RH5514, a tab-indented list: in the input below, `→` stands for a tab, and the class braces are in column 0.

```csharp
class C
{
→{|RH5514:[A, B]|}
→void M()
→{
→}
}
```

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

`[B]` starts behind a tab, as `[A]` does, and the tabs in front of the declaration and the method braces stay. With a line that starts with a tab and four spaces, `[B]` starts behind a tab and four spaces.

Example — line endings: in a document whose line breaks are all CRLF, the line break the fix inserts between `[A]` and `[B]` is CRLF, and every other line break stays as it was. In a document whose line breaks are all a lone `\r`, the inserted line break is `\n`.

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

### SHARED-ATTR-021 — The separate-line fix is withheld when the enclosing brace is off its depth column

The code fix of a rule that requires the list on its own line MUST NOT be offered for a list outside every initializer and anonymous object (SHARED-ATTR-009) when the innermost opening brace that encloses the list, of the kinds SHARED-ATTR-008 counts, stands on a line that is not indented four spaces for each scope level, as SHARED-ATTR-008 counts them, that encloses that brace: one for each enclosing brace of those kinds, and one more inside the statements of a switch section, so the brace of a block among a section's statements is at the column of those statements. The diagnostic is still reported. A list whose innermost enclosing brace stands on a line indented that way is fixed as SHARED-ATTR-008 states, as in its examples. In the indentation of the brace's line, each tab MUST count as four spaces and every other whitespace character as one.

Why: SHARED-ATTR-008 indents the declaration by its nesting depth, which matches the formatter only where the enclosing brace sits at its own depth. A brace the formatter aligns to something else, such as a lambda's block aligned to the lambda in a wrapped argument (FMT-INDENT-028), puts the declaration in a column the nesting depth cannot give, and computing that column would repeat the formatter's indentation in the fix. Such a brace around an attribute list is rare. Code indented with tabs is common, and counting a tab as four spaces finds its braces at their depth, so the fix stays offered for it.

Example — RH5513:

```csharp
using System;

class C
{
    void M()
    {
        Run(1, () =>
               {
                   {|RH5513:[A]|} void L()
                   {
                   }
               });
    }

    void Run(int a, Action f)
    {
    }
}
```

is reported, and no code fix is offered.

Example — RH5513, tab-indented: in the input below, `→` stands for a tab. The method block's brace is enclosed by the namespace and the class, and its line is indented with two tabs, which count as eight columns, its depth column, so the code fix is offered and indents the declaration with spaces as SHARED-ATTR-008 states. Indented with two spaces instead of two tabs, the brace would be off its depth column, and no code fix would be offered.

```text
namespace N
{
→class C
→{
→→void M()
→→{
→→→{|RH5513:[A]|} void L()
→→→{
→→→}
→→}
→}
}
```

### SHARED-ATTR-022 — The split fix is withheld on the line of the brace that opens the body holding the declaration

The code fix of a rule that requires one attribute per list MUST NOT be offered when the reported list starts on the line on which the `{` stands that opens the body directly holding the declaration the list belongs to:

- the accessor list that holds an accessor;
- the type declaration — a class, struct, record, interface, or enum — that holds a member, a nested type, a delegate, or an enum member;
- the namespace declaration with braces that holds a type or a delegate;
- the block that holds a local function.

What stands between that `{` and the list on their line does not matter. The diagnostic is still reported. When that `{` stands on a line above the line on which the reported list starts, the fix is offered as SHARED-ATTR-017 states. A `{` on the list's line that opens another body, such as the body of a method in front of the list, does not withhold the fix, and neither does a list whose declaration no `{` directly holds, such as a list on a parameter, a type in a file-scoped namespace, or a top-level local function.

Why: On the line of that `{`, the whitespace at the start of the line is the indentation of the code that owns the braces — the property, indexer, or event of an accessor list, the type of a member, the namespace of a type, the statement or member that owns a block — not of the declaration the list belongs to. A further list started behind it, as SHARED-ATTR-017 states, would stand in front of the declaration, the rest of the body, and its `}`, at the column of the code that owns the braces, which is a layout nobody writes. Laying the body out over several lines instead would change code beyond the reported list (SHARED-FIX-006). An attribute list behind such a `{` on its line is rare, and comparing the line of the `{` with the line of the list is a cheap check.

Example — the `{` that opens the body holding the declaration on the line of the reported list:

```csharp
namespace N { {|RH5506:[A, B]|} class E { } }

class C { {|RH5514:[A, B]|} void M() { } }

class D
{
    void N() { {|RH5514:[A, B]|} void L() { } }

    public int Value { {|RH5518:[field: First, Second]|} get => 1; }

    public int Other { {|RH5531:[A, B]|} get => 1;
        set { } }
}
```

is reported, and no code fix is offered.

Example — RH5518, the accessor list's `{` on a line above the reported list:

```csharp
class C
{
    public int Value
    {
        {|RH5518:[field: First, Second]|} get => 1;
    }

    public int Other {
        {|RH5518:[field: First, Second]|} get => 1; }
}
```

```csharp
class C
{
    public int Value
    {
        [field: First]
        [field: Second] get => 1;
    }

    public int Other {
        [field: First]
        [field: Second] get => 1; }
}
```

Example — RH5514, a `{` on the line of the reported list that opens another body:

```csharp
class C
{
    void M() { } {|RH5514:[A, B]|} void N() { }
}
```

```csharp
class C
{
    void M() { } [A]
    [B] void N() { }
}
```

The `{` on the list's line opens the body of `M`, not the body of `C` that holds `N`, so the fix is offered as SHARED-ATTR-017 states.

### SHARED-ATTR-023 — The split fix is withheld for a line break it would copy or keep in another line ending

The code fix of a rule that requires one attribute per list MUST NOT be offered in each of the following cases. The diagnostic is still reported.

- **A line break in the target specifier.** The reported list has a target specifier, such as `field:`, and its first attribute starts on a later line than its `[`: a line break stands between the `[` and the target name, between the target name and its colon, or between the colon and the first attribute. A line break between two attributes, after the last attribute, or in a list without a target specifier does not withhold the fix.
- **Another line ending in the list.** The reported list, from its `[` through its `]`, holds a line break other than the line ending SHARED-FIX-008 detects for the document, also one inside a literal of an attribute argument, and also one between two attributes, which the fix does not keep. In a document whose line breaks are all a lone `\r`, the detected line ending is `\n`, so the fix is withheld for every list that holds a line break. A line break outside the list, such as the one that ends the list's line, does not withhold the fix, and a list whose line breaks are all the detected line ending keeps it, with each line break inside an attribute kept as written.

Why: The fix writes each new list as the specifier followed by one attribute, as written, so a line break in the specifier would be repeated in front of every attribute, which would lose the indentation of the first one, and a line break inside an attribute is kept. SHARED-FIX-008 requires a kept line break in the code a fix changes to take the detected line ending; the fix keeps the attributes as written, as the moves of SHARED-FIX-008's second paragraph keep their code, so it is withheld instead of rewriting it. A target specifier split from its attributes and a list with mixed line endings are rare, and both are found with a cheap check of the list's tokens and text.

Example — RH5518, a line break in the target specifier:

```csharp
class C
{
    {|RH5518:[field:
        A, B]|}
    int _f;
}
```

is reported, and no code fix is offered. With the line break between the attributes instead, the fix is offered:

```csharp
class C
{
    {|RH5518:[field: A,
        B]|}
    int _f;
}
```

```csharp
class C
{
    [field: A]
    [field: B]
    int _f;
}
```

Example — RH5514, another line ending in the list: in a document whose line breaks are CRLF, except the line break inside the argument list of `A`, which is LF,

```csharp
class C
{
    {|RH5514:[A(1,
      2), B]|}
    void M()
    {
    }
}
```

is reported, and no code fix is offered. In a document whose line breaks are all CRLF, the same list gets the fix, and the line break inside `A` stays as written:

```csharp
class C
{
    [A(1,
      2)]
    [B]
    void M()
    {
    }
}
```

In a document whose line breaks are all a lone `\r`, no fix is offered for this list, while `[A, B]` on one line gets it, with `\n` inserted between `[A]` and `[B]` (SHARED-ATTR-017).
