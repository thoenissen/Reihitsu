# Shared — Element documentation

| | |
|---|---|
| ID prefix | SHARED-DOC |
| Status | Active |
| Code owners | `SplitElementDocumentationAnalyzerBase`, `DocumentationAnalysisUtilities`, `DirectDocumentationSyntaxChecker`, `XmlDocumentationExpander`, `DocumentationModeAnalysisContextExtensions` |
| Uses | SHARED-FIX-003, SHARED-FIX-006, SHARED-ORDER-002 |

This spec covers the rules that require one kind of element to be documented. It defines which declarations such a rule checks, the private and non-private groups that split each kind between two rules, the declarations it exempts, and when a declaration counts as documented. It also defines what every documentation rule relies on: which documentation comments belong to a declaration, when a summary has text, when the parameters and type parameters of an element count as documented, and in which documentation modes documentation rules run. For the rules that check the descriptions of parameters, type parameters, and return values, it defines when such a description has text, how the descriptions are paired with the parameters, and when a description lacks a name. For the code fixes that remove or unwrap a documentation element, it defines when an element has an end tag of its own. Each rule that uses this spec states which element kind it checks, which group, and where its diagnostic is located.

## Non-goals

- Which element kind and which group a rule checks, and where its diagnostic is located — each rule's spec.
- Which `<summary>` elements are checked for text, and where that diagnostic is located — RH8030 and RH8031; whether `<inheritdoc>` is used where nothing is inherited — RH8203.
- Which parameters and type parameters a rule checks, and where its diagnostic is located — RH8032, RH8033, RH8101 to RH8104, and RH8108 to RH8111; the documentation of return values — RH8105 to RH8107.
- The content, order, and layout of a documentation comment — RH8301 to RH8309, apart from which comments they judge (SHARED-DOC-004).

## Requirements

### SHARED-DOC-001 — Every declaration of the rule's element kind is checked

A rule MUST check the declarations of the element kind it names wherever they are declared — in a namespace, at file level, or nested in a class, struct, interface, record, record struct, enum, or extension block — and MUST decide each declaration on its own: whether a containing declaration is documented MUST NOT change whether a nested one is reported.

This requirement covers an element with a single declaration; a partial element declared in several parts — a type, constructor, method, property, indexer, or event — is not covered.

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

- the private group, when its modifiers include `private` and none of `public`, `internal`, `protected`, and `file`, or when it has none of these five modifiers and is declared directly in a class, struct, record, record struct, or extension block;
- the non-private group otherwise — including a `private protected` declaration, a `file` type, and a declaration without any of these five modifiers in a namespace, at file level, or in an interface.

The accessibility of the containing declarations MUST NOT change the group.

This requirement covers every declaration except a static constructor declared in an interface and a partial type declared in several parts.

Why: The split separates what other code can use from the implementation details of one type, so a project can require documentation for the first and decide separately about the second. A `private protected` element is visible to derived types, so other code builds on it. Without a modifier, C# makes an element declared in a class, struct, record, or extension block private, and one declared in a namespace or an interface internal or public; a partial type declared in several parts is the exception, because it takes the accessibility that any of its parts declares. A static constructor of a class, struct, or record, which takes no access modifier, is private. The group is decided from the declaration as written, so a nested element keeps its rule when its container's accessibility changes.

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

Example — RH8021, which checks the methods of the private group:

```csharp
/// <summary>
/// Extensions
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Text values
    /// </summary>
    /// <param name="value">Value</param>
    extension(string value)
    {
        int {|RH8021:Implicit|}() => 0;

        private int {|RH8021:Explicit|}() => 0;

        public int Visible() => 0;

        internal int Assembly() => 0;
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

The documentation comments of a declaration MUST be the `///` and `/** */` comments in its leading trivia, which comes before the declaration's first token — its first attribute list, modifier, keyword, or name. A documentation comment between an attribute list and the rest of the declaration MUST NOT belong to the declaration. Ordinary comments, preprocessor directives, and disabled text between the last documentation comment and the declaration MUST NOT separate them.

A `///` comment that starts behind code on its line, after the last token of the code in front of the declaration, MUST count as a documentation comment of that declaration, like a `///` comment that starts its own line in front of it. A rule about the layout of documentation comments, such as RH8301, RH8302, or RH8303, MUST judge such a comment as if it started its own line. A code fix that moves neither the declaration the comment documents nor the code the comment stands behind MUST leave the comment behind that code where it stands, except the RH8303 fix, which moves it onto a line of its own above the declaration. When the documentation mode of the syntax tree is `Parse` or `Diagnose`, a code fix that moves the declaration the comment documents, and not the code the comment stands behind, MUST carry the comment with the declaration, as SHARED-ORDER-002 states for the comments in a moved declaration's leading trivia; where the comment then starts is not covered by this requirement. A fix that cannot carry it, for example because a preprocessor directive between the comment and the declaration starts the code the fix moves, MUST NOT be offered (SHARED-FIX-003); the diagnostic is still reported. When the documentation mode is `None`, the comment is an ordinary comment behind the code in front of it, not a documentation comment of the declaration: a code fix that moves the declaration MUST leave the comment behind that code, as it leaves a `//` comment behind code. A code fix that moves the code the comment stands behind is not covered by this requirement.

This requirement covers leading trivia in which nothing but whitespace and line breaks stands between two documentation comments; leading trivia in which an ordinary comment, a preprocessor directive, or disabled text separates two documentation comments is not covered.

Why: The compiler attaches the documentation comments that precede a declaration, attributes included, and ignores one that follows an attribute list as misplaced. A rule that read a different comment would disagree with the documentation the compiler generates. A `///` comment behind code is attached to the declaration below it as well — when documentation comments are parsed, the compiler places it in that declaration's leading trivia — and a reader takes it as the start of that declaration's documentation; judged by where it starts instead of by what it is, it would escape every layout rule while it is laid out worst. Moving it off the code line is the correction RH8303 names, because a blank line above a documentation comment needs the comment on a line of its own; any other fix that moved it while its declaration stays would change code its diagnostic is not about (SHARED-FIX-006). A fix that moves the declaration itself is about that declaration, and its documentation belongs to it: leaving the comment behind would detach the documentation from what it documents. A directive between the comment and the declaration can start the block a fix moves, as a `#region` directive starts the region block that RH7309 moves; such a fix cannot carry the comment, and moving the declaration without it would hand the documentation to whatever code follows, so the fix is withheld instead. When documentation comments are not parsed, the compiler attaches the comment to no declaration: it is ordinary trailing trivia of the code in front of it, like a `//` comment behind a closing brace, and it stays with that code.

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

Example — RH8026, where the comment behind `A` documents `B`:

```csharp
public class C
{
    public int {|RH8026:A|}; /// <summary>Value</summary>
    public int B;
}
```

Example — RH8302 and RH8303 judge the comment behind `A` as if it started its line:

```csharp
class C
{
    int A; {|RH8303:///|} <summary>
    /// B</summary>
{|RH8302:
|}    int B;
}
```

The RH8303 fix moves the comment onto a line of its own above `B`:

```csharp
class C
{
    int A; {|RH8303:///|} <summary>
    /// B</summary>
    int B;
}
```

```csharp
class C
{
    int A;

    /// <summary>
    /// B</summary>
    int B;
}
```

Example — RH7103, with the documentation mode `Parse` or `Diagnose`, where the comment behind `Run`'s closing brace documents `Create`:

```csharp
public class C
{
    public void Run()
    {
    } /// <summary>Creates</summary>
    public static void {|RH7103:Create|}()
    {
    }
}
```

The fix moves `Create` in front of `Run`, and the comment moves with it as `Create`'s documentation; it does not stay behind `Run`'s closing brace.

With the documentation mode `None`, the same comment is an ordinary comment behind `Run`'s closing brace:

```csharp
public class C
{
    public void Run()
    {
    } /// <summary>Creates</summary>
    public static void {|RH7103:Create|}()
    {
    }
}
```

The fix moves `Create` in front of `Run`, and the comment stays behind `Run`'s closing brace, as `// Creates` would.

Example — RH7309, with the documentation mode `Parse` or `Diagnose`, where a `#region` directive between the comment and `M` starts the block the fix would move: the example "a documentation comment that would be left behind" under SHARED-FIX-003. The fix is not offered.

### SHARED-DOC-005 — A top-level `<summary>` or `<inheritdoc>` documents the element

A declaration MUST count as documented when its first documentation comment has a `<summary>` or an `<inheritdoc>` element at its top level, that is, not nested inside another element; an empty element (`<summary/>`) counts. A declaration without documentation comments, and one whose first documentation comment has neither element at its top level, MUST count as undocumented, unless SHARED-DOC-006 or SHARED-DOC-007 applies.

This requirement covers an element with a single declaration whose documentation comments are placed as SHARED-DOC-004 covers and are well-formed XML, with the tag names `summary` and `inheritdoc` written in lower case, without a namespace prefix, and in no XML namespace. A partial element declared in several parts — a type, constructor, method, property, indexer, or event — is not covered.

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

/// <summary/>
public class C
{
}

/** <summary>Block comment</summary> */
public class D
{
}

public class {|RH8001:E|}
{
}

/// <remarks>
/// <summary>Nested</summary>
/// </remarks>
public class {|RH8001:F|}
{
}

/// Plain text
public class {|RH8001:G|}
{
}
```

### SHARED-DOC-006 — A later documentation comment counts too

When a declaration has more than one documentation comment, a `<summary>` or an `<inheritdoc>` element at the top level of a later one MUST count as well, as SHARED-DOC-005 specifies for the first. When none of them has either element at its top level, the declaration MUST count as undocumented.

This requirement covers what SHARED-DOC-005 covers.

Why: The compiler joins documentation comments that are separated only by whitespace and line breaks into one documentation of the element, so a summary in the second comment is part of it.

Example — RH8001:

```csharp
/// <remarks>
/// Remarks
/// </remarks>

/// <summary>
/// Summary
/// </summary>
public class A
{
}

/// <remarks>
/// First
/// </remarks>

/// <remarks>
/// Second
/// </remarks>
public class {|RH8001:B|}
{
}
```

### SHARED-DOC-007 — An included documentation file counts

A `<summary>` or an `<inheritdoc>` element that an `<include>` element of the documentation brings in at the top level MUST count as SHARED-DOC-005 specifies. An `<include>` element whose file the compilation cannot resolve, or whose selected content has neither element at the top level, MUST NOT make the declaration documented.

This requirement covers what SHARED-DOC-005 covers, and an included file that is well-formed XML and writes the tag names `summary` and `inheritdoc` as SHARED-DOC-005 covers them: in lower case, without a namespace prefix, and in no XML namespace.

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

### SHARED-DOC-008 — Documentation comments are not judged when documentation is not parsed

A rule whose subject is a documentation comment — its elements, its lines, or its position, as RH8001 to RH8309 judge them — MUST NOT report anything in a syntax tree whose documentation mode is `None`. It MUST check syntax trees whose documentation mode is `Parse` or `Diagnose`.

A documentation rule whose subject is not a documentation comment — an ordinary comment, such as RH8401 judges, the text of the file, such as RH8402 judges, or code, such as the attribute RH8501 judges — MUST check syntax trees in every documentation mode, `None` included.

Why: Without documentation parsing, a documentation comment is plain trivia whose elements cannot be identified, so every element would look undocumented. RH0002 reports that mode. Only documentation comments depend on the parsing: an ordinary comment written with documentation slashes, a copyright header, and the arguments of an attribute are text and syntax that every mode keeps, so a rule about them has the same subject whether documentation is parsed or not. Staying silent would switch such a rule off only because the project does not generate documentation.

Example — RH8001: with the documentation mode `None`,

```csharp
public class C
{
}
```

is not reported; with the mode `Parse` or `Diagnose`, it is reported on `C`.

Example — RH8501, which reads an attribute and no documentation comment: with the documentation mode `None`,

```csharp
using System.Diagnostics.CodeAnalysis;

class C
{
    [{|RH8501:SuppressMessage|}("Design", "CA1000")]
    void M()
    {
    }
}
```

is reported.

Example — RH8401, whose subject is an ordinary comment written with documentation slashes: with the documentation mode `None`,

```csharp
class C
{
    void M()
    {
        {|RH8401:///|} workaround
    }
}
```

is reported.

### SHARED-DOC-009 — Explicit interface implementations are not checked

A rule that requires the declarations of its element kind to be documented MUST NOT report a method, property, indexer, event, operator, or conversion operator declaration that explicitly implements an interface member — one whose name is qualified by the interface, such as `void IDisposable.Dispose()` — whatever its documentation and whichever type or interface declares it.

Why: An explicit implementation can only be reached through the interface, so the editor and the generated documentation lead the reader to the interface member it implements, and that member carries the description. A summary on the implementation would only repeat it, and it would drift when the interface member changes.

Example — RH8021, which checks the methods of the private group:

```csharp
using System;

/// <summary>
/// Resource
/// </summary>
public class Resource : IDisposable
{
    void IDisposable.Dispose()
    {
    }

    void {|RH8021:Release|}()
    {
    }
}
```

Example — RH8020, which checks the methods of the non-private group:

```csharp
/// <summary>
/// Contract
/// </summary>
public interface IContract
{
    /// <summary>
    /// Runs the contract
    /// </summary>
    void Run();
}

/// <summary>
/// Derived contract
/// </summary>
public interface IDerived : IContract
{
    void IContract.Run()
    {
    }

    void {|RH8020:Stop|}();
}
```

### SHARED-DOC-010 — A summary without text

A `<summary>` element MUST count as without text when it is an empty element (`<summary/>`) or when its content is only white space and line breaks. The `///` that continues a documentation comment on a new line, and the `*` or run of `*` that starts a line of a `/** */` comment after its leading white space, MUST count as line breaks, not as text; any other `*`, such as one after a space or in the middle of a line, is text. White space is every character that Unicode classifies as white space — a space, a tab, or a no-break space (`&#160;`) among them — whether written directly or as a character reference such as `&#32;`. A `<summary>` whose content has a character that is neither white space nor a line break MUST count as having text.

This requirement covers a `<summary>` whose content is text and character references only; a `<summary>` that contains an XML element, an XML comment, a CDATA section, or a processing instruction is not covered. A `<summary>` whose content is white space, line breaks, and at least one format character that renders nothing — Unicode category Cf, such as a zero-width space (`&#x200B;`), a zero-width no-break space (`&#xFEFF;`), a word joiner (`&#x2060;`), or a soft hyphen (`&#xAD;`) — is not covered either.

Why: The summary is the one line the editor's tooltip and the generated documentation show for an element. A summary of whitespace shows nothing, so it leaves the reader exactly where a missing summary would, while it looks as if the element were documented.

Example — RH8030:

```csharp
/// {|RH8030:<summary></summary>|}
public class A
{
}

/// {|RH8030:<summary/>|}
public class B
{
}

/// {|RH8030:<summary>
/// </summary>|}
public class C
{
}

/// {|RH8030:<summary>&#32;</summary>|}
public class D
{
}

/**
 * {|RH8030:<summary>
 * </summary>|}
 */
public class E
{
}

/**
 ** {|RH8030:<summary>
 ** </summary>|}
 */
public class J
{
}

/** <summary> * </summary> */
public class K
{
}

/// {|RH8030:<summary>&#160;</summary>|}
public class F
{
}

/// <summary>
/// Text
/// </summary>
public class G
{
}

/**
 * <summary>
 * Text
 * </summary>
 */
public class H
{
}
```

### SHARED-DOC-011 — Parameters are checked only on documentation without `<inheritdoc>`

A rule that requires each parameter or type parameter of an element to be documented MUST NOT report any of them when the element's declaration has no documentation comment, as SHARED-DOC-004 defines its documentation comments. It MUST NOT report any of them either when the element's documentation has an `<inheritdoc>` element at its top level, whether in its first documentation comment, in a later one, in the content that an `<include>` element brings in, or, for an extension declaration, in the documentation it shares as SHARED-DOC-013 specifies.

This requirement covers documentation comments placed as SHARED-DOC-004 covers that are well-formed XML, with the tag name `inheritdoc` written in lower case, without a namespace prefix, and in no XML namespace.

Why: An element without documentation is reported by the rule that requires its summary, and one diagnostic there says more than one on every parameter. `<inheritdoc>` takes the descriptions of the parameters from the element it inherits from, together with the rest of the documentation.

Example — RH8032:

```csharp
/// <summary>
/// Extensions
/// </summary>
public static class Extensions
{
    extension(string value)
    {
    }

    /// <inheritdoc/>
    extension(int value)
    {
    }

    /// <remarks>
    /// Remarks
    /// </remarks>

    /// <inheritdoc/>
    extension(byte value)
    {
    }

    /// <summary>
    /// Long values
    /// </summary>
    extension(long {|RH8032:value|})
    {
    }
}
```

Example — RH8032, with a file `docs.xml` next to the source:

```xml
<doc>
    <members>
        <member name="I">
            <inheritdoc/>
        </member>
    </members>
</doc>
```

```csharp
/// <summary>
/// Extensions
/// </summary>
public static class Extensions
{
    /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
    extension(float value)
    {
    }
}
```

### SHARED-DOC-012 — A parameter is documented by a top-level element with its name

A parameter MUST count as documented when the element's documentation has a `<param>` element at its top level whose `name` attribute equals the parameter's name, compared character by character and case-sensitively; a type parameter MUST count as documented when it has such a `<typeparam>` element. The parameter's name is the value of its identifier: without the `@` of a verbatim identifier, with a Unicode escape sequence decoded, and with format characters (Unicode category Cf) removed. So `name="class"` documents `@class` and `name="@class"` does not, and `name="a"` documents a parameter declared as `\u0061`. The element MUST count whether it is in the first documentation comment, in a later one, in the content that an `<include>` element brings in, or, for an extension declaration, in the documentation it shares as SHARED-DOC-013 specifies, and whether or not it has text. An element nested inside another element, and one whose `name` differs from the parameter's name, MUST NOT count.

This requirement covers a parameter or type parameter with a name, a `name` attribute without white space before or after the name and without a C# Unicode escape sequence (such as `\u0061`) or a format character (Unicode category Cf, such as `&#x200B;`) in it, and documentation comments placed as SHARED-DOC-004 covers that are well-formed XML, with the tag names `param` and `typeparam` written in lower case, without a namespace prefix, and in no XML namespace. An included file is covered when it is well-formed XML and writes the tag names the same way.

Why: The compiler and documentation tools pair a `<param>` or `<typeparam>` with the parameter whose name it carries, and that name is the identifier's value, without the `@` that only lets a keyword be used as a name, with its escape sequences decoded, and with its format characters removed, as C# does when it compares identifiers. One with a different name, or one inside `<remarks>` or another element, describes nothing that the tooltip shows for the parameter. Whether the description has text is a separate rule.

Example — RH8032 and RH8033:

```csharp
using System.Collections.Generic;

/// <summary>
/// Extensions
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Text values
    /// </summary>
    /// <param name="value">Value</param>
    extension(string value)
    {
    }

    /// <summary>
    /// Integer values
    /// </summary>
    /// <param name="Value">Value</param>
    extension(int {|RH8032:value|})
    {
    }

    /// <summary>
    /// Long values
    /// </summary>
    /// <remarks>
    /// <param name="value">Value</param>
    /// </remarks>
    extension(long {|RH8032:value|})
    {
    }

    /// <remarks>
    /// Remarks
    /// </remarks>

    /// <summary>
    /// Lists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values">Values</param>
    extension<T>(List<T> values)
    {
    }

    /// <summary>
    /// Pairs
    /// </summary>
    /// <typeparam name="TKey">Key</typeparam>
    /// <param name="pairs">Pairs</param>
    extension<TKey, {|RH8033:TValue|}>(Dictionary<TKey, TValue> pairs)
    {
    }

    /// <summary>
    /// Bytes
    /// </summary>
    /// <param name="class">Value</param>
    extension(byte @class)
    {
    }

    /// <summary>
    /// Characters
    /// </summary>
    /// <param name="@class">Value</param>
    extension(char {|RH8032:@class|})
    {
    }

    /// <summary>
    /// Arrays
    /// </summary>
    /// <typeparam name="T">Element</typeparam>
    /// <param name="items">Items</param>
    extension<@T>(T[] items)
    {
    }

    /// <summary>
    /// Sets
    /// </summary>
    /// <typeparam name="@T">Element</typeparam>
    /// <param name="items">Items</param>
    extension<{|RH8033:@T|}>(HashSet<T> items)
    {
    }

    /// <summary>
    /// Unsigned values
    /// </summary>
    /// <param name="a">Value</param>
    extension(ushort \u0061)
    {
    }

    /// <summary>
    /// Queues
    /// </summary>
    /// <typeparam name="U">Element</typeparam>
    /// <param name="queue">Queue</param>
    extension<\u0055>(Queue<U> queue)
    {
    }
}
```

Example — RH8032 and RH8033, with a file `docs.xml` next to the source:

```xml
<doc>
    <members>
        <member name="A">
            <summary>From the file</summary>
            <typeparam name="T">Element</typeparam>
            <param name="values">Values</param>
        </member>
    </members>
</doc>
```

```csharp
using System.Collections.Generic;

/// <summary>
/// Extensions
/// </summary>
public static class Extensions
{
    /// <include file='docs.xml' path='doc/members/member[@name="A"]/*'/>
    extension<T>(IEnumerable<T> values)
    {
    }
}
```

### SHARED-DOC-013 — Merged extension declarations share their documentation

Extension declarations that the compiler merges into one extension — it gives them one documentation ID and writes one documentation entry that joins their documentation comments — MUST share that documentation, and SHARED-DOC-011 and SHARED-DOC-012 MUST read it for each of them. Extension declarations that the compiler does not merge MUST NOT share their documentation.

The compiler merges the extension declarations of one static class, whether they are in one declaration of the class or in several partial declarations of it, in one file or in several, whose parameter and type parameters it treats as the same: the same type, so that `string` and `System.String` merge while `string` and `string?` do not, and tuple types merge only with the same element names; the same parameter name; the same `ref`, `in`, `ref readonly`, and `scoped` modifiers; the same attributes with the same arguments on the parameter and on each type parameter; and the same type-parameter names with the same constraints, in any order.

This requirement covers what SHARED-DOC-011 and SHARED-DOC-012 cover, for the documentation comments of every declaration of the merged extension; a merged extension in which any declaration's documentation comment is not well-formed XML is not covered.

Why: A reader sees the one documentation entry the compiler writes for the merged extension, so a `<param>` written on any of its declarations documents the parameter of all of them. Following the compiler's own grouping keeps the rule from disagreeing with the generated documentation for any of the ways two declarations can differ.

Example — RH8032:

```csharp
#nullable enable

/// <summary>
/// Extensions
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Text values
    /// </summary>
    extension(string value)
    {
    }

    /// <summary>
    /// More text values
    /// </summary>
    /// <param name="value">Value</param>
    extension(string value)
    {
    }

    /// <summary>
    /// Nullable text values
    /// </summary>
    extension(string? {|RH8032:value|})
    {
    }

    /// <summary>
    /// Integer values
    /// </summary>
    /// <param name="value">Value</param>
    extension(int value)
    {
    }

    /// <summary>
    /// More integer values
    /// </summary>
    extension(int {|RH8032:other|})
    {
    }

    /// <summary>
    /// Long values
    /// </summary>
    extension(long value)
    {
    }

    /// <inheritdoc/>
    extension(long value)
    {
    }

    /// <summary>
    /// Characters
    /// </summary>
    /// <param name="value">Value</param>
    extension(char value)
    {
    }

    /// <summary>
    /// More characters
    /// </summary>
    extension(System.Char value)
    {
    }

    /// <summary>
    /// Pairs
    /// </summary>
    /// <param name="pair">Pair</param>
    extension((int A, int B) pair)
    {
    }

    /// <summary>
    /// Other pairs
    /// </summary>
    extension((int C, int D) {|RH8032:pair|})
    {
    }

    /// <summary>
    /// Read-only references
    /// </summary>
    /// <param name="number">Number</param>
    extension(in double number)
    {
    }

    /// <summary>
    /// Writable references
    /// </summary>
    extension(ref double {|RH8032:number|})
    {
    }
}
```

Example — RH8032 and RH8033:

```csharp
using System;
using System.Collections.Generic;

/// <summary>
/// Marks a declaration
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class MarkAttribute : Attribute
{
    /// <summary>
    /// Creates the attribute
    /// </summary>
    /// <param name="value">Value</param>
    public MarkAttribute(int value)
    {
    }
}

/// <summary>
/// Extensions
/// </summary>
public static partial class Extensions
{
    /// <summary>
    /// Lists
    /// </summary>
    /// <typeparam name="T">Element</typeparam>
    /// <param name="values">Values</param>
    extension<T>(List<T> values)
    {
    }

    /// <summary>
    /// More lists
    /// </summary>
    extension<T>(List<T> values)
    {
    }

    /// <summary>
    /// Arrays
    /// </summary>
    /// <typeparam name="T">Element</typeparam>
    /// <param name="values">Values</param>
    extension<T>(T[] values)
    {
    }

    /// <summary>
    /// More arrays
    /// </summary>
    /// <param name="values">Values</param>
    extension<{|RH8033:U|}>(U[] values)
    {
    }

    /// <summary>
    /// Sequences
    /// </summary>
    /// <typeparam name="T">Element</typeparam>
    /// <param name="items">Items</param>
    extension<T>(IEnumerable<T> items) where T : IComparable, IDisposable
    {
    }

    /// <summary>
    /// More sequences
    /// </summary>
    extension<T>(IEnumerable<T> items) where T : IDisposable, IComparable
    {
    }

    /// <summary>
    /// Sets of references
    /// </summary>
    /// <typeparam name="T">Element</typeparam>
    /// <param name="items">Items</param>
    extension<T>(ISet<T> items) where T : class
    {
    }

    /// <summary>
    /// Sets of values
    /// </summary>
    /// <param name="items">Items</param>
    extension<{|RH8033:T|}>(ISet<T> items) where T : struct
    {
    }

    /// <summary>
    /// Read-only lists
    /// </summary>
    /// <typeparam name="T">Element</typeparam>
    /// <param name="items">Items</param>
    extension<T>(IList<T> items)
    {
    }

    /// <summary>
    /// Marked read-only lists
    /// </summary>
    /// <param name="items">Items</param>
    extension<[Mark(0)] {|RH8033:T|}>(IList<T> items)
    {
    }

    /// <summary>
    /// Spans
    /// </summary>
    /// <param name="span">Span</param>
    extension(ref Span<int> span)
    {
    }

    /// <summary>
    /// Scoped spans
    /// </summary>
    extension(scoped ref Span<int> {|RH8032:span|})
    {
    }

    /// <summary>
    /// Read-only references
    /// </summary>
    /// <param name="number">Number</param>
    extension(in decimal number)
    {
    }

    /// <summary>
    /// Read-only references, declared otherwise
    /// </summary>
    extension(ref readonly decimal {|RH8032:number|})
    {
    }

    /// <summary>
    /// Marked longs
    /// </summary>
    /// <param name="value">Value</param>
    extension([Mark(1)] long value)
    {
    }

    /// <summary>
    /// Longs marked otherwise
    /// </summary>
    extension([Mark(2)] long {|RH8032:value|})
    {
    }

    /// <summary>
    /// Marked integers
    /// </summary>
    /// <param name="value">Value</param>
    extension([Mark(1)] int value)
    {
    }

    /// <summary>
    /// More marked integers
    /// </summary>
    extension([Mark(1)] int value)
    {
    }

    /// <summary>
    /// Single values
    /// </summary>
    /// <param name="value">Value</param>
    extension(float value)
    {
    }
}

/// <summary>
/// More extensions
/// </summary>
public static partial class Extensions
{
    /// <summary>
    /// More single values
    /// </summary>
    extension(float value)
    {
    }
}
```

### SHARED-DOC-014 — A parameter, type-parameter, or return-value description without text

A `<param>`, `<typeparam>`, or `<returns>` element MUST count as without text exactly when a `<summary>` element with the same content counts as without text under SHARED-DOC-010, and as having text otherwise.

This requirement covers what SHARED-DOC-010 covers, applied to the content of the `<param>`, `<typeparam>`, or `<returns>` element.

Why: The description of a parameter, a type parameter, or a return value is what the editor's tooltip shows next to it, so a description of white space leaves the reader as uninformed as a missing one, the same way an empty summary does. Judging all of them by one definition keeps the rules that require text from disagreeing about the same content.

Example — RH8104, RH8106, and RH8111:

```csharp
/// <summary>
/// Container
/// </summary>
/// {|RH8111:<typeparam name="T"></typeparam>|}
public class C<T>
{
    /// <summary>
    /// Runs
    /// </summary>
    /// {|RH8104:<param name="a"/>|}
    /// {|RH8104:<param name="b">&#32;</param>|}
    /// <param name="c">Value</param>
    /// {|RH8106:<returns>
    /// </returns>|}
    public int M(int a, int b, int c) => 0;

    /**
     * <summary>
     * Runs
     * </summary>
     * {|RH8104:<param name="a">
     * </param>|}
     * <returns>Value</returns>
     */
    public int N(int a) => 0;
}
```

### SHARED-DOC-015 — Parameter descriptions follow the order of the parameters

A rule that checks whether the `<param>` elements of a declaration's documentation comment match the declaration's parameters, or its `<typeparam>` elements the declaration's type parameters, MUST pair the elements at the top level of the comment, as SHARED-DOC-004 defines the comment, with the parameters by position: the first element with the first parameter, the second element with the second parameter, and so on, counting every element whether or not it has a name. It MUST report an element whose `name` attribute differs from the name of the parameter in the same position, compared as SHARED-DOC-012 compares a `name` attribute with a parameter's name, and an element whose position is after the last parameter. It MUST NOT report an element that has no `name` attribute or whose `name` attribute is empty or only white space, an element nested inside another element, or a parameter that has no element.

This requirement covers a declaration that declares at least one parameter, or at least one type parameter for `<typeparam>` elements, and whose leading trivia has one documentation comment, placed as SHARED-DOC-004 covers and well-formed XML, with the tag names `param` and `typeparam` written in lower case, without a namespace prefix, and in no XML namespace; and a `name` attribute whose value is empty, only white space, or a C# identifier that SHARED-DOC-012 covers. A `<param>` element named `value` on an indexer that has a `set` or `init` accessor is not covered.

Why: Readers and documentation tools take the descriptions in the order they are written, and a reader who scans them against the signature attaches each description to the parameter in its position. A description that names no parameter in its position, or that comes after the last one, describes something the signature does not have there. An element without a name is reported by the rule that requires the name, and a parameter without a description by the rule that requires one, so neither is reported twice.

Example — RH8102 and RH8109:

```csharp
/// <summary>
/// Pair
/// </summary>
/// {|RH8109:<typeparam name="TSecond">Second</typeparam>|}
/// {|RH8109:<typeparam name="TFirst">First</typeparam>|}
public class Pair<TFirst, TSecond>
{
    /// <summary>
    /// Runs
    /// </summary>
    /// <param name="a">A</param>
    /// {|RH8102:<param name="c">C</param>|}
    public void M(int a, int b)
    {
    }

    /// <summary>
    /// Runs
    /// </summary>
    /// <param>Unnamed</param>
    /// <param name="b">B</param>
    /// {|RH8102:<param name="c">Extra</param>|}
    public void N(int a, int b)
    {
    }

    /// <summary>
    /// Runs
    /// </summary>
    /// <param name="a">A</param>
    /// <remarks>
    /// <param name="z">Nested</param>
    /// </remarks>
    public void O(int a, int b)
    {
    }

    /// <summary>
    /// Runs
    /// </summary>
    /// <param name="class">Class</param>
    /// {|RH8102:<param name="@event">Event</param>|}
    public void P(int @class, int @event)
    {
    }
}
```

### SHARED-DOC-016 — A parameter description names its parameter

A rule that requires the `<param>` or `<typeparam>` elements of a declaration's documentation comment to name a parameter MUST report each such element at the top level of the comment, as SHARED-DOC-004 defines the comment, that has no `name` attribute or whose `name` attribute is empty or only white space. The attribute name MUST be matched case-sensitively, so an element that has only a `Name` attribute has no `name` attribute. An element nested inside another element MUST NOT be reported.

This requirement covers a declaration whose leading trivia has one documentation comment, placed as SHARED-DOC-004 covers and well-formed XML, with the tag names `param` and `typeparam` written in lower case, without a namespace prefix, and in no XML namespace; and a `name` attribute whose value is empty, only white space, or a C# identifier.

Why: The `name` attribute is the only link between a description and its parameter. Without it, the compiler and the documentation tools cannot pair the description with any parameter, so the tooltip shows nothing for the parameter, and the reader has to guess which one the text is about.

Example — RH8103 and RH8110:

```csharp
/// <summary>
/// Container
/// </summary>
/// {|RH8110:<typeparam>Element</typeparam>|}
public class C<T>
{
    /// <summary>
    /// Runs
    /// </summary>
    /// {|RH8103:<param>First</param>|}
    /// {|RH8103:<param name="">Second</param>|}
    /// {|RH8103:<param name=" ">Third</param>|}
    /// {|RH8103:<param Name="d">Fourth</param>|}
    /// <param name="e">Fifth</param>
    /// <remarks>
    /// <param>Nested</param>
    /// </remarks>
    public void M(int a, int b, int c, int d, int e)
    {
    }
}
```

### SHARED-DOC-017 — An end tag with another name is no end tag of its own

An XML element of a documentation comment that is written with a start tag MUST count as having an end tag of its own only when the parser closes it with an end tag that carries the name of its start tag. Such an element without an end tag, and one that the parser closes with an end tag of another name — `<returns>` in `<remarks><returns>Nothing</remarks>`, which `</remarks>` closes — has no end tag of its own. The code fixes of RH8107, RH8202, and RH8204 MUST NOT be offered for a reported element that has no end tag of its own; the diagnostic is still reported. An empty element, such as `<returns/>`, `<value/>`, or `<placeholder/>`, needs no end tag, and this requirement does not withhold a fix for it.

Why: An end tag with another name belongs to an element around the reported one, and an element without an end tag runs to the end of the comment. An empty element ends where it is written, so removing it touches nothing else. Removing or unwrapping the reported element up to where the parser ends it would also remove the end tag of the element around it, or the documentation and the code that follow it.

Example — RH8107 reports the `<returns>` element of the `void` method, which `</remarks>` closes:

```csharp
public class C
{
    /// <summary>
    /// Runs
    /// </summary>
    /// <remarks><returns>Nothing</remarks>
    public void Run()
    {
    }
}
```

and no code fix is offered.

Example — RH8202 reports the `<value>` element, which has no end tag:

```csharp
public class C
{
    /// <summary>
    /// Gets the size
    /// </summary>
    /// <value>The size
    public int Size { get; }
}
```

and no code fix is offered.

Example — an empty `<returns/>` element ends where it is written, so the RH8107 fix is offered and removes its line:

```csharp
public class C
{
    /// <summary>
    /// Runs
    /// </summary>
    /// {|RH8107:<returns/>|}
    public void Run()
    {
    }
}
```

```csharp
public class C
{
    /// <summary>
    /// Runs
    /// </summary>
    public void Run()
    {
    }
}
```
