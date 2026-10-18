# Shared — Element documentation

| | |
|---|---|
| ID prefix | SHARED-DOC |
| Status | Active |
| Code owners | `SplitElementDocumentationAnalyzerBase`, `DocumentationAnalysisUtilities`, `DirectDocumentationSyntaxChecker`, `XmlDocumentationExpander`, `DocumentationModeAnalysisContextExtensions` |
| Uses | — |

This spec covers the rules that require one kind of element to be documented. It defines which declarations such a rule checks, the private and non-private groups that split each kind between two rules, and when a declaration counts as documented. It also defines what every documentation rule relies on: which documentation comments belong to a declaration, and when documentation rules do not run at all. Each rule that uses this spec states which element kind it checks, which group, and where its diagnostic is located.

## Non-goals

- Which element kind and which group a rule checks, and where its diagnostic is located — each rule's spec.
- Whether the `<summary>` element has text — RH8030; whether `<inheritdoc>` is used where nothing is inherited — RH8203.
- The documentation of parameters, type parameters, and return values — RH8101 to RH8111.
- The content, order, and layout of a documentation comment — RH8301 to RH8309.

## Requirements

### SHARED-DOC-001 — Every declaration of the rule's element kind is checked

A rule MUST check the declarations of the element kind it names wherever they are declared — in a namespace, at file level, or nested in a class, struct, interface, record, record struct, or enum — and MUST decide each declaration on its own: whether a containing declaration is documented MUST NOT change whether a nested one is reported.

Why: Each element has its own entry in the generated documentation and its own tooltip in the editor. A documented container says nothing about what a nested element is for, and an undocumented container is reported by its own rule.

Example — RH8001:

```csharp
public class {|RH8001:Outer|}
{
    public class {|RH8001:Inner|}
    {
    }
}

/// <summary>
/// Documented
/// </summary>
public class Documented
{
    public class {|RH8001:Nested|}
    {
    }
}
```

### SHARED-DOC-002 — Private and non-private groups

A declaration MUST belong to exactly one of two groups, decided by its own modifiers and the kind of declaration that directly contains it:

- the private group, when its modifiers include `private` and none of `public`, `internal`, `protected`, and `file`, or when it has none of these five modifiers and is declared directly in a class, struct, record, or record struct;
- the non-private group otherwise — including a `private protected` declaration, a `file` type, and a declaration without any of these five modifiers in a namespace, at file level, or in an interface.

The accessibility of the containing declarations MUST NOT change the group.

Why: The split separates what other code can use from the implementation details of one type, so a project can require documentation for the first and decide separately about the second. A `private protected` element is visible to derived types, so other code builds on it. Without a modifier, C# makes an element declared in a class, struct, or record private, and one declared in a namespace or an interface internal or public; a static constructor, which takes no access modifier, is private. The group is decided from the declaration as written, so a nested element keeps its rule when its container's accessibility changes.

Example — RH8001, which checks the non-private group:

```csharp
class {|RH8001:TopLevel|}
{
}

file class {|RH8001:FileLocal|}
{
}

/// <summary>
/// Outer
/// </summary>
public class Outer
{
    private protected class {|RH8001:Shared|}
    {
    }

    private class Hidden
    {
        public class {|RH8001:Visible|}
        {
        }
    }
}

/// <summary>
/// Contract
/// </summary>
public interface I
{
    class {|RH8001:Nested|}
    {
    }
}
```

Example — RH8002, which checks the private group:

```csharp
/// <summary>
/// Outer
/// </summary>
public class Outer
{
    private class {|RH8002:Explicit|}
    {
    }

    class {|RH8002:Implicit|}
    {
    }

    private protected class Shared
    {
    }

    protected class Derived
    {
    }
}

/// <summary>
/// Value
/// </summary>
public struct S
{
    class {|RH8002:InStruct|}
    {
    }
}
```

### SHARED-DOC-003 — An enum member belongs to the group of its enum

An enum member MUST belong to the group that SHARED-DOC-002 decides for the enum that declares it.

Why: An enum member cannot carry an access modifier; it is visible wherever its enum is, so the enum's group is the member's group.

Example — RH8013, which checks the members of enums in the non-private group:

```csharp
public enum Visible
{
    {|RH8013:First|},
}

/// <summary>
/// Outer
/// </summary>
public class Outer
{
    enum Hidden
    {
        Second,
    }
}
```

Example — RH8014, which checks the members of enums in the private group:

```csharp
/// <summary>
/// Outer
/// </summary>
public class Outer
{
    enum Hidden
    {
        {|RH8014:Second|},
    }

    protected enum Visible
    {
        First,
    }
}
```

### SHARED-DOC-004 — The documentation comments of a declaration precede it

The documentation comments of a declaration MUST be the `///` and `/** */` comments in its leading trivia, which comes before its first attribute list, modifier, or keyword; the first of them is the declaration's documentation comment. A documentation comment between an attribute list and the rest of the declaration MUST NOT belong to the declaration. Ordinary comments and preprocessor directives between a documentation comment and the declaration MUST NOT separate them.

Why: The compiler attaches the documentation comments that precede a declaration, attributes included, and ignores one that follows an attribute list as misplaced. A rule that read a different comment would disagree with the documentation the compiler generates.

Example — RH8001:

```csharp
using System;

/// <summary>
/// Before the attribute
/// </summary>
[Obsolete]
public class A
{
}

[Obsolete]
/// <summary>
/// After the attribute
/// </summary>
public class {|RH8001:B|}
{
}

/// <summary>
/// Before a directive
/// </summary>
#if DEBUG
#endif
public class C
{
}

/// <summary>
/// Before an ordinary comment
/// </summary>
// Ordinary comment
public class D
{
}
```

### SHARED-DOC-005 — A top-level `<summary>` or `<inheritdoc>` documents the element

A declaration MUST count as documented when its documentation comment has a `<summary>` or an `<inheritdoc>` element at its top level, that is, not nested inside another element. The tag name MUST be matched case-insensitively, and an empty element (`<summary/>`) and an element without an end tag count. A declaration without a documentation comment, and one whose documentation comment has neither element at its top level, MUST count as undocumented, unless SHARED-DOC-006 or SHARED-DOC-007 applies.

This requirement covers an element with a single declaration; a partial type declared in several parts is not covered.

Why: `<summary>` is the element every documentation tool shows, and `<inheritdoc>` takes the whole documentation, summary included, from the element it inherits from. Whether the summary also has text is a separate rule (RH8030). A `<summary>` nested in `<remarks>` or another element is not the element's summary, and other elements such as `<remarks>` alone do not give a reader the one-line description a tooltip shows.

Example — RH8001:

```csharp
/// <summary>
/// Documented
/// </summary>
public class A
{
}

/// <inheritdoc/>
public class B : A
{
}

/// <Summary>Upper case</Summary>
public class C
{
}

/// <summary/>
public class D
{
}

/** <summary>Block comment</summary> */
public class E
{
}

public class {|RH8001:F|}
{
}

/// <remarks>
/// <summary>Nested</summary>
/// </remarks>
public class {|RH8001:G|}
{
}

/// Plain text
public class {|RH8001:H|}
{
}

/// <summary>No end tag
public class I
{
}
```

### SHARED-DOC-006 — A later documentation comment counts too

When a declaration has more than one documentation comment, a `<summary>` or an `<inheritdoc>` element at the top level of a later one MUST count as well, as SHARED-DOC-005 specifies for the first.

This requirement covers a later documentation comment that is well-formed XML, of an element with a single declaration.

Why: The compiler joins every documentation comment that precedes a declaration into the element's documentation, so a summary in the second comment is part of it.

Example — RH8001:

```csharp
/// <remarks>
/// Remarks
/// </remarks>
// Separator
/// <summary>
/// Summary
/// </summary>
public class C
{
}
```

is not reported.

### SHARED-DOC-007 — An included documentation file counts

A `<summary>` or an `<inheritdoc>` element that an `<include>` element of the documentation brings in at the top level MUST count as SHARED-DOC-005 specifies. An `<include>` element whose file the compilation cannot resolve, or whose selected content has neither element at the top level, MUST NOT make the declaration documented.

This requirement covers an element with a single declaration, and an included file that is well-formed XML.

Why: An `<include>` element is replaced by the selected content of the file when the compiler builds the documentation, so the summary in that file is the element's summary.

Example — RH8001, with a file `docs.xml` next to the source:

```xml
<doc>
    <members>
        <member name="A">
            <summary>From the file</summary>
        </member>
        <member name="B">
            <remarks>Only remarks</remarks>
        </member>
    </members>
</doc>
```

```csharp
/// <include file='docs.xml' path='doc/members/member[@name="A"]/*'/>
public class A
{
}

/// <include file='docs.xml' path='doc/members/member[@name="B"]/*'/>
public class {|RH8001:B|}
{
}

/// <include file='missing.xml' path='doc/members/member[@name="A"]/*'/>
public class {|RH8001:C|}
{
}
```

### SHARED-DOC-008 — Nothing is reported when documentation is not parsed

A rule MUST NOT report anything in a syntax tree whose documentation mode is `None`. It MUST check syntax trees whose documentation mode is `Parse` or `Diagnose`.

Why: Without documentation parsing, a documentation comment is plain trivia whose elements cannot be identified, so every element would look undocumented. RH0002 reports that mode.

Example — RH8001: with the documentation mode `None`,

```csharp
public class C
{
}
```

is not reported; with the mode `Parse` or `Diagnose`, it is reported on `C`.
