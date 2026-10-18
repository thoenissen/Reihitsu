Role: spec author
Commit: f4dcaee5ed8a39b4fd6b0ddc5ee2dffc4fc35048
Verdict: spec written

Revision round on `02-skeptic.md` (F1–F9) and `squad/decisions/2026-10-18-shared-doc-coverage-and-extension-block.md`. The lead's message carried no intended fix or hoped-for verdict. I re-ran every finding's input with my own scratch harness outside the repository (Roslyn 5.9.0, `LanguageVersion.Latest`, `XmlFileResolver` on the fixture folder, Debug `Reihitsu.Analyzer.dll`; production code is unchanged since 53990ed). Every finding reproduces as the skeptic states it. The open-question list below **supersedes** the one in `01-spec-author.md`.

## Resolution of the findings

- **F1 — merged extension declarations.** Reproduced: two `extension(string value)` blocks get one documentation ID and one joined entry, and RH8032/RH8033 follow it. Also checked: `string value` / `string? value` (with `#nullable enable`) and `int value` / `int other` do not merge, and RH8032 reports the undocumented one of each pair. The RH8032/RH8033 output agrees with the documentation the compiler produces, so it does not look wrong. I chose (a): new **SHARED-DOC-013 — Merged extension declarations share their documentation**, with examples on both sides of the merge condition, referenced from SHARED-DOC-011, SHARED-DOC-012, RH8032-001, and RH8033-001. For RH8031 the per-block verdict disagrees with the merged entry, so merged declarations are left out of RH8031-001's coverage (RH8031-002 inherits it) and filed as RH8031 Q2.
- **F2 — tag-name and XML shapes in RH8030.** Reproduced: `<Summary></Summary>`, `<x:summary xmlns:x="urn:x"></x:summary>`, an unclosed `<summary>` (CS1570, span `<summary>` plus the line break), and `<summary></summary> a & b` (CS1570) are all reported. RH8030-001's coverage now requires well-formed XML and the tag name `summary` in lower case, without a namespace prefix, and in no XML namespace. The shapes are filed as RH8030 Q2.
- **F3 — `name=" value "`.** Reproduced: RH8032 reports `value`; the compiler binds the `<param>` (no CS1572). A `name` attribute with white space before or after the name is left out of SHARED-DOC-012's coverage and filed as shared Q5. I do not mark it P3: P3 (SHARED-DIAG-002) compares a C# name by value, without `@`, and a qualified name without whitespace or comments; an XML attribute value padded with spaces is neither. The lead decides whether to read P3 more widely.
- **F4 — `@` in parameter names.** Reproduced: `name="class"` documents `@class` (not reported, compiler silent); `name="@class"` does not (RH8032 on `@class`, CS1572); `<typeparam name="@T">` gives RH8033 on `@T` (CS1711); `<typeparam name="T">` documents `@T`. The diagnostic span includes the `@`. This agrees with the compiler and, as I read it, with P3: the identifier's value is `class`, and the attribute text `@class` is not that value. SHARED-DOC-012 now states it, with an example on each side for parameters and type parameters, and its `Why:` no longer claims pairing "exactly as written". The rule pages RH8032 and RH8033 ("spell the name exactly as it appears in the declaration") now add "without the `@` of a verbatim identifier such as `@class`" / "`@T`". If the lead reads P3 so that `name="@class"` should count, that shape moves out of coverage and is filed as `Decided by principle P3 (#912)`.
- **F5 — `*` continuation.** Reproduced: `/**` + `* <summary>` + `* </summary>` + `*/` is reported; with `* Text` it is not. SHARED-DOC-010 now counts the `*` that continues a `/** */` comment as a line break, with examples on both sides.
- **F6 — SHARED-DOC-001 container list.** "extension block" added to the list, as the maintainer approved in the decision record's follow-up answer. Normative meaning otherwise unchanged.
- **F7 — precision.**
  - RH8034-001: conversion operators are now listed only for class, struct, interface, record, and record struct. Reproduced: a conversion operator in an extension block gives CS9282. RH8034 still reports it on `operator`, but that input does not compile, so it is not specified.
  - RH8030-002 and RH8031-003: the location reads "the empty element `<summary/>`, or everything from the start tag through the end tag".
  - SHARED-DOC-010: white space is what Unicode classifies as white space. Reproduced: `&#160;`, `&#x2003;`, and `&#x85;` are reported; `&#x200B;` is not. The requirement says this, with `&#160;` and `&#x200B;` examples.
  - SHARED-DOC-010's coverage now also excludes a processing instruction. Reproduced: `<summary><?pi ?></summary>` is not reported. It is added to shared Q2.
  - The shared non-goal no longer overlaps: "RH8032, RH8033, RH8101 to RH8104, and RH8108 to RH8111; the documentation of return values — RH8105 to RH8107".
- **F8 — markings.** I apply one rule, and the list below follows it:
  - `Decided by principle P8 (#912)` is used for (1) whether a declaration is documented, for the presence rules RH8001–RH8029 and RH8034, and (2) the consequences thoenissen/Reihitsu#1091 states about what is a `<summary>` or `<inheritdoc>` at all — only comments the compiler keeps count, malformed XML does not count, tag names are lower case and namespace-free — whichever rule reads the element.
  - It is not used for which documentation sources a rule that is not a presence rule reads (RH8030, RH8031: later comments, includes, merged blocks), nor for whether `<inheritdoc>` satisfies RH8031. Those are normal questions, each noting that P8 decides it if the maintainer reads RH8031 as the presence rule of extension declarations.
  - Changes against 01: RH8031 Q1 is split. The dropped-comment, `<Summary>`, and malformed shapes stay P8. The include, later-comment, and merged-block shapes become the normal RH8031 Q2. The `<inheritdoc>` question is now RH8031 Q3, still normal. RH8030 Q2 is split the same way: P8 shapes in Q2, the others in Q3. Shared Q3 is split into Q3 (`<param>`/`<typeparam>`, normal) and Q4 (`<inheritdoc>`, P8).
- **F9 — partial parts and RH8030.** Reproduced: `/// <summary></summary> public partial class J { }` next to a documented part is reported. Partial parts are left out of RH8030-001's coverage and the shape is added to RH8030 Q3.

## Changed requirements

Relative to `01-spec-author.md`:

- SHARED-DOC-013 — Merged extension declarations share their documentation — added (used by RH8031, RH8032, RH8033; ID inserted in `sdd/specs/requirement-ids.txt`)
- SHARED-DOC-001 — clarified: "extension block" added to the container list (F6, approved)
- SHARED-DOC-010 — clarified: `*` continuation, Unicode white space, processing instruction out of coverage (F5, F7)
- SHARED-DOC-011 — clarified: shared documentation of merged extension declarations (F1); "the element's declaration has no documentation comment"
- SHARED-DOC-012 — clarified: verbatim identifiers (F4), padded `name` out of coverage (F3), merged extension declarations (F1), new examples, `Why:` corrected
- RH8030-001 — clarified: coverage requires well-formed, lower-case, namespace-free `summary` (F2) and excludes partial parts (F9)
- RH8030-002, RH8031-003 — clarified: location wording for `<summary/>` (F7)
- RH8031-001 — clarified: merged extension declarations out of coverage (F1); `Uses` gains SHARED-DOC-013
- RH8032-001, RH8033-001 — clarified: merged declarations decided on their shared documentation (F1); `Uses` gains SHARED-DOC-013
- RH8034-001 — clarified: conversion operators not listed for extension blocks (F7)
- Shared non-goal — overlap removed (F7)
- Rule pages `documentation/rules/RH8032.md`, `documentation/rules/RH8033.md` — "How to fix it" sentence corrected for verbatim identifiers (F4)

Verification:
- Every example block of `shared/element-documentation.md` and `analyzer/RH8020.md`–`RH8034.md` matches the analyzers in `Diagnose` and `Parse` mode. The exception is the existing SHARED-DOC-008 block, a `None`-mode case the harness does not model.
- Compiler warnings now present in example blocks: CS1572 (`@class`) and CS1711 (`@T`) in the first SHARED-DOC-012 block, in addition to the CS1572/CS1712 noted in 01. The SHARED-DOC-013 block compiles without warnings.
- `SpecConsistencyTests`, `SpecStructureTests`, and `RuleDocumentationConventionTests` pass.

## Examples for the tests

As in 01, plus:
- SHARED-DOC-010: new `/** */` blocks and the `&#160;` and `&#x200B;` blocks.
- SHARED-DOC-012: the `@class` and `@T` blocks.
- SHARED-DOC-013: its RH8032 block. This is the only block that repeats a receiver on purpose; every other example uses distinct receivers.

## Open questions to file

These supersede 01. Each target is a new `SDD open questions: <spec path>` issue, a sub-issue of thoenissen/Reihitsu#912; thoenissen/Reihitsu#1037 is closed.
- How the inputs were run: in `Diagnose` mode unless stated, with the harness above. The shared-logic cases behave the same in `Parse` mode.
- No case is another instance of thoenissen/Reihitsu#1092.
- No case matches P1, P2, P5, P6, or P7. P3 is discussed under F3 and F4.

### Target: `SDD open questions: sdd/specs/shared/element-documentation.md`

#### Q1 — Each part of a partial method, property, indexer, or event is decided on its own

**Decided by principle P8 (#912).** Violated requirement: P8 as decided in thoenissen/Reihitsu#1091 (the compiler merges the parts of a partial element into one documentation); the case is left out of SHARED-DOC-001 and SHARED-DOC-005 by their coverage sentences.

```csharp
using System;

/// <summary>P</summary>
public partial class P
{
    /// <summary>Defining</summary>
    public partial void M1();
    public partial void M1() { }

    public partial void M2();
    /// <summary>Implementing</summary>
    public partial void M2() { }

    /// <remarks>R</remarks>
    public partial void M3();
    /// <summary>Implementing</summary>
    public partial void M3() { }

    /// <summary>Defining</summary>
    public partial int Prop { get; }
    public partial int Prop => 0;

    /// <summary>Defining</summary>
    public partial int this[int i] { get; }
    public partial int this[int i] => i;

    /// <summary>Defining</summary>
    public partial event Action E;
    public partial event Action E { add { } remove { } }
}
```

Real output:
- RH8020 reports the implementing `M1` and the defining `M2`.
- RH8022 reports the implementing `Prop`, RH8024 the implementing `this`, and RH8028 the implementing `E`.
- Neither part of `M3` is reported.
- The compiler writes one entry per member, each with a `<summary>`: "Defining" for `M:P.M1`, `P:P.Prop`, `P:P.Item(System.Int32)`, and `E:P.E`; "Implementing" for `M:P.M2` and `M:P.M3`.
- `partial void M(); partial void M() { }` gives RH8021 on both parts.

Why it looks wrong: every member is documented in the generated documentation, yet an undocumented part is reported. A part with only `<remarks>` (`M3`) is accepted because the semantic fallback reads the merged documentation. Under P8 nothing here is reported.

Touches: SHARED-DOC-001, SHARED-DOC-005, SHARED-DOC-006, SHARED-DOC-007; RH8020-001, RH8021-001, RH8022-001, RH8023-001, RH8024-001, RH8025-001, RH8028-001, RH8029-001.

#### Q2 — A summary whose only content is markup without text counts as text

```csharp
/// <summary><para/></summary>
public class A { }

/// <summary><!-- later --></summary>
public class B { }

/// <summary><![CDATA[]]></summary>
public class C { }

/// <summary><c></c></summary>
public class D { }

/// <summary><?pi ?></summary>
public class F { }

/// <summary><see cref="A"/></summary>
public class E { }

/// <summary>X</summary>
public static class Ext
{
    /// <summary><para/></summary>
    extension(string value) { }

    /// <summary><!-- later --></summary>
    extension(int value) { }
}
```

Real output: RH8030 and RH8031 report nothing. Any element, XML comment, CDATA section, or processing instruction makes a summary count as having text, even an empty one.

Why it looks wrong: SHARED-DOC-010's `Why:` and the RH8030 rule page argue from what the tooltip shows. For `A`, `B`, `C`, `D`, `F`, and both extension blocks it shows nothing; an XML comment and a processing instruction are never shown. `E` shows the referenced name.

Readings:
- (A) As today: any element, comment, CDATA section, or processing instruction counts as text. Nothing is reported.
- (B) Only content that renders text counts. `A`, `B`, `C`, `D`, `F`, and both extension blocks are reported; `E` is not.

Touches: SHARED-DOC-010 (left out by its coverage sentence), RH8030-001, RH8031-002.

#### Q3 — `<param>` and `<typeparam>` are matched by local name, case-insensitively

```csharp
using System.Collections.Generic;

/// <summary>E</summary>
public static class Extensions
{
    /// <summary>Upper</summary>
    /// <PARAM name="value">V</PARAM>
    /// <TYPEPARAM name="T">T</TYPEPARAM>
    extension<T>(List<T> value) { }

    /// <summary>Prefixed</summary>
    /// <x:param xmlns:x="urn:x" name="value">V</x:param>
    extension(long value) { }
}
```

Real output: RH8032 and RH8033 report nothing. The compiler writes `<PARAM>` and `<TYPEPARAM>` unchanged into the generated file.

Why it looks wrong: XML names are case-sensitive. The compiler, the tooltip, and documentation tools pair only `<param>` and `<typeparam>` in no namespace with a parameter. P8 decided the same question for `<summary>` and `<inheritdoc>`. RH8101 and RH8108 run the same code.

Readings:
- (A) As today: any case, any namespace.
- (B) Only `param` and `typeparam` in lower case and in no namespace count. Then `value` and `T` of the first block and `value` of the second are reported.

Touches: SHARED-DOC-012 (left out by its coverage sentence), RH8032-001, RH8033-001.

#### Q4 — `<inheritdoc>` exempts parameters in shapes that P8 says are not an `<inheritdoc>`

**Decided by principle P8 (#912)** (thoenissen/Reihitsu#1091, from #1037 Q1 and Q5). Violated requirement: SHARED-DOC-011, whose coverage sentence leaves these shapes out.

```csharp
/// <summary>E</summary>
public static class Extensions
{
    /// <INHERITDOC/>
    extension<U>(U[] value) { }

    /// <summary>S</summary>
    /// <x:inheritdoc xmlns:x="urn:x"/>
    extension(int value) { }

    /// <inheritdoc/>
    // c
    /// <summary>S</summary>
    extension(string value) { }
}
```

Real output: RH8032 and RH8033 report nothing. The `<inheritdoc/>` of the last block is in a comment the compiler drops (CS1587).

Why it looks wrong: under P8, an upper-case or namespaced element is not an `<inheritdoc>`, and a dropped comment is not part of the documentation. So `value` and `U` of the first block and `value` of the other two are reported.

Touches: SHARED-DOC-011, RH8032-001, RH8033-001.

#### Q5 — A `name` attribute with spaces around the name is not counted, although the compiler binds it

```csharp
/// <summary>X</summary>
public static class Extensions
{
    /// <summary>B</summary>
    /// <param name=" value ">V</param>
    extension(byte value) { }
}
```

Real output: RH8032 reports `value`. The compiler binds the `<param>`: it gives no CS1572 here. For `void M(int a, int b)`, `<param name=" a ">` together with `<param name="b">` gives no CS1573, while `<param name="x">` on `N(int a, int b)` gives CS1572 for `x` and CS1573 for `a` (skeptic, `02-skeptic.md` F3).

Why it looks wrong: the parameter is documented in the generated documentation, as the compiler sees it, yet the rule reports it.

Readings:
- (A) As today: the `name` value is compared as written.
- (B) White space around the name is ignored, as the compiler does. Not reported.

Touches: SHARED-DOC-012 (left out by its coverage sentence), RH8032-001, RH8033-001. The lead decides whether P3 reaches it (see F3).

### Target: `SDD open questions: sdd/specs/analyzer/RH8030.md`

#### Q1 — An explicit interface implementation's empty summary is not reported

```csharp
using System;

/// <summary>C</summary>
public class C : IDisposable
{
    /// <summary></summary>
    void IDisposable.Dispose() { }

    /// <summary></summary>
    public void Close() { }
}
```

Real output: RH8030 reports the summary of `Close` only. The exemption comes from the same gate the presence rules use.

Why it looks wrong: SHARED-DOC-009's reason (the interface member carries the description, so none is needed) does not apply to a summary the user did write. The rule page says every `<summary>` tag must contain text, and this one says nothing.

Readings:
- (A) As today: explicit implementations are exempt from RH8030 too.
- (B) The empty summary is reported like any other.

Touches: RH8030-001 (left out by its coverage sentence).

#### Q2 — Elements that P8 says are not the summary are reported as empty summaries

**Decided by principle P8 (#912)** (thoenissen/Reihitsu#1091, from #1037 Q1, Q2, and Q5: only comments the compiler keeps count, malformed XML does not count, and tag names are lower case and namespace-free). Violated requirement: RH8030-001, whose coverage sentence leaves these shapes out.

```csharp
/// <summary></summary>
// c
/// <remarks>R</remarks>
public class B { }

/// <Summary></Summary>
public class U { }

/// <x:summary xmlns:x="urn:x"></x:summary>
public class N { }

/// <summary>
public class O { }

/// <summary></summary> a & b
public class M { }
```

Real output: RH8030 reports each of the five. The comment on `B` is dropped by the compiler (CS1587). `O` and `M` are malformed (CS1570), and the span on `O` is `<summary>` plus the line break, because there is no end tag.

Why it looks wrong: under P8, none of these elements is the element's summary, so there is no summary for RH8030 to call empty.

Touches: RH8030-001, RH8030-002.

#### Q3 — Which summaries RH8030 checks: later comments, a second summary, included files, partial parts

Related to P8 but not marked: P8 states when an element is documented, not which summaries RH8030 checks.

Input, with `empty.xml` next to the source:

```xml
<doc>
    <member name="E">
        <summary></summary>
    </member>
</doc>
```

```csharp
/// <remarks>R</remarks>

/// <summary></summary>
public class A { }

/// <summary>Text</summary>
/// <summary></summary>
public class C { }

/// <include file='empty.xml' path='doc/member[@name="E"]/*'/>
public class D { }

/// <summary></summary>
public partial class J { }

/// <summary>Text</summary>
public partial class J { }
```

Real output:
- RH8030 reports the empty summary of the first `J` part only.
- It does not report `A`, the second summary of `C`, or `D`.
- RH8001 reports none of these classes, so `A` and `D` pass both rules with an empty summary in their generated documentation.
- The compiler joins both `J` parts into one entry with two `<summary>` elements.

Why it looks wrong: SHARED-DOC-006 and SHARED-DOC-007 count a summary in a later comment and in an included file as the element's summary, yet RH8030 does not check them. The rule page says every `<summary>` tag must contain text, yet the second one of `C` is not reported. For `J`, the generated documentation has a summary with text, yet one part is reported.

Readings:
- (A) As today: the first top-level `<summary>` of the first documentation comment of each declaration.
- (B) Every top-level `<summary>` in the documentation comments the compiler keeps. `A` and the second summary of `C` are reported, `D` is not, and the first `J` part stays reported.
- (C) The summary of the documentation the compiler produces, located on the summary element or on the `<include>` element. `A` and `D` are reported. `C` and `J` would need a rule for joined entries with several summaries.

Touches: RH8030-001, RH8030-002 (left out by the coverage sentence of RH8030-001).

### Target: `SDD open questions: sdd/specs/analyzer/RH8031.md`

#### Q1 — Elements that P8 says are not a summary satisfy RH8031

**Decided by principle P8 (#912)** (thoenissen/Reihitsu#1091, from #1037 Q1, Q2, and Q5). Violated requirement: RH8031-001, whose coverage sentence leaves these shapes out.

```csharp
/// <summary>X</summary>
public static class Extensions
{
    /// <summary>Dropped</summary>
    // c
    /// <remarks>R</remarks>
    extension(long value) { }

    /// <Summary>Upper</Summary>
    extension(byte value) { }

    /// <summary>a & b</summary>
    extension(short value) { }
}
```

Real output: RH8031 reports nothing.
- The `long` block's summary is in a comment the compiler drops (CS1587).
- The `byte` block's element is `<Summary>`.
- The `short` block is malformed (CS1570, "Badly formed XML comment ignored" in the generated file).

Why it looks wrong: under P8, none of these blocks has a `<summary>`, so all three are reported on their `extension` keyword.

Touches: RH8031-001, RH8031-003.

#### Q2 — RH8031 ignores a summary in an included file, a later comment, or a merged declaration

Normal question. If the maintainer reads RH8031 as the presence rule of extension declarations, P8 decides it.

Input, with `empty.xml` next to the source:

```xml
<doc>
    <member name="S">
        <summary>From the file</summary>
    </member>
</doc>
```

```csharp
/// <summary>X</summary>
public static class Extensions
{
    /// <include file='empty.xml' path='doc/member[@name="S"]/*'/>
    extension(string value) { }

    /// <remarks>R</remarks>

    /// <summary>Later</summary>
    extension(int value) { }

    /// <summary>First</summary>
    extension(char value) { }

    extension(char value) { }
}
```

Real output:
- RH8031 reports the `extension` keyword of the `string` block, the `int` block, and the second `char` block.
- The compiler's documentation of the `string` block contains "From the file", and that of the `int` block contains "Later".
- Both `char` blocks are merged into one extension (SHARED-DOC-013) whose single entry contains "First".

Why it looks wrong: the verdict disagrees with the generated documentation and the tooltip. RH8032 and RH8033 read the same documentation sources (SHARED-DOC-011, SHARED-DOC-012, SHARED-DOC-013), while RH8031 does not.

Readings:
- (A) As today: only the first documentation comment of each block is read.
- (B) The documentation the compiler produces is read: none of these blocks is reported.

Touches: RH8031-001, RH8031-003, SHARED-DOC-013 (left out of RH8031-001 by its coverage sentence).

#### Q3 — `<inheritdoc>` on an extension declaration is reported by RH8031 but exempts it from RH8032 and RH8033

Normal question. If the maintainer reads RH8031 as the presence rule of extension declarations, P8 decides it, and the block is not reported.

```csharp
/// <summary>X</summary>
public static class Extensions
{
    /// <inheritdoc cref="Extensions"/>
    extension(char value) { }
}
```

Real output: RH8031 reports the `extension` keyword. RH8032 does not report `value`, because SHARED-DOC-011 exempts a declaration with `<inheritdoc>`.

Why it looks wrong: SHARED-DOC-005 counts `<inheritdoc>` as documentation for every other element, and RH8032/RH8033 treat the block as documented by it, while RH8031 treats it as having no summary.

Readings:
- (A) As today: RH8031 requires a `<summary>` written on the block.
- (B) `<inheritdoc>` documents the block as SHARED-DOC-005 specifies. Not reported.

Touches: RH8031-001, SHARED-DOC-011 (left out of RH8031-001 by its coverage sentence).

### Target: `SDD open questions: sdd/specs/analyzer/RH8032.md`

#### Q1 — An unnamed extension parameter is reported, with no source location

```csharp
/// <summary>X</summary>
public static class Extensions
{
    /// <summary>Adds static helpers to long</summary>
    extension(long)
    {
        /// <summary>Zero</summary>
        /// <returns>0</returns>
        public static int Zero() => 0;
    }
}
```

Real output: one RH8032 diagnostic whose location is not in source (`Location.IsInSource` is false; it shows at line 1, column 1 with an empty span). The compiler reports nothing.

Why it looks wrong: a parameter without a name has nothing a `<param>` could document. The diagnostic can only be suppressed, and it points at no code.

Readings:
- (A) An unnamed parameter is not reported.
- (B) It is reported, located on the parameter's type (`long`).

Touches: RH8032-001, RH8032-002 (left out of RH8032-001 by its coverage sentence).
