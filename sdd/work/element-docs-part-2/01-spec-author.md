Role: spec author
Commit: eb6cc9e6afb39d677484932a7374b71c64118c89
Verdict: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## How the statements were established

- Every analyzer was read (`SplitElementDocumentationAnalyzerBase`, `DocumentationAnalysisUtilities`, `DirectDocumentationSyntaxChecker`, `XmlDocumentationExpander`, and the 15 rule analyzers); none of RH8020–RH8034 has a code-fix provider, every descriptor is `Warning`, enabled by default, category `Documentation`.
- Every C# example block in the new specs and in `shared/element-documentation.md` was run against the Debug build of `Reihitsu.Analyzer` with a scratch harness outside the repository (SDK 10.0.402, Roslyn 5.9.0, `LanguageVersion.Latest`), in `Diagnose` and in `Parse` mode, comparing the `{|RH####:…|}` markup with the reported diagnostics and spans. All blocks match in both modes. The only mismatch is the existing SHARED-DOC-008 block, which states a `None`-mode case the harness does not model.
- Compiler warnings the example blocks produce (facts, not a test instruction): the SHARED-DOC-012 `<include>` block gets CS1572 ("param tag for 'values', but there is no parameter by that name") although RH8032 counts the included `<param>`; the first SHARED-DOC-012 block gets CS1572 for `name="Value"` and CS1712 for `TValue`.
- `SpecConsistencyTests` and `SpecStructureTests` pass. `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails with the 35 new IDs below uncovered, as expected before tests are tagged.

## Changed requirements

New specs (all added): `sdd/specs/analyzer/RH8020.md` … `RH8034.md`.

- RH8020-001 — Undocumented non-private methods are reported — added
- RH8020-002 — The diagnostic is on the method name — added
- RH8021-001 — Undocumented private methods are reported — added
- RH8021-002 — The diagnostic is on the method name — added
- RH8022-001 — Undocumented non-private properties are reported — added
- RH8022-002 — The diagnostic is on the property name — added
- RH8023-001 — Undocumented private properties are reported — added
- RH8023-002 — The diagnostic is on the property name — added
- RH8024-001 — Undocumented non-private indexers are reported — added
- RH8024-002 — The diagnostic is on the `this` keyword — added
- RH8025-001 — Undocumented private indexers are reported — added
- RH8025-002 — The diagnostic is on the `this` keyword — added
- RH8026-001 — Undocumented non-private fields are reported — added
- RH8026-002 — The diagnostic is on the name of the first field — added
- RH8027-001 — Undocumented private fields are reported — added
- RH8027-002 — The diagnostic is on the name of the first field — added
- RH8028-001 — Undocumented non-private events are reported — added
- RH8028-002 — The diagnostic is on the event name — added
- RH8029-001 — Undocumented private events are reported — added
- RH8029-002 — The diagnostic is on the event name — added
- RH8030-001 — A summary without text is reported — added
- RH8030-002 — The diagnostic is on the summary element — added
- RH8031-001 — An extension declaration without a summary is reported — added
- RH8031-002 — An extension declaration whose summary has no text is reported — added
- RH8031-003 — Where the diagnostic is — added
- RH8032-001 — An undocumented parameter of an extension declaration is reported — added
- RH8032-002 — The diagnostic is on the parameter name — added
- RH8033-001 — An undocumented type parameter of an extension declaration is reported — added
- RH8033-002 — The diagnostic is on the type parameter name — added
- RH8034-001 — Undocumented operators are reported — added
- RH8034-002 — Where the diagnostic is — added

`sdd/specs/shared/element-documentation.md`:

- SHARED-DOC-009 — Explicit interface implementations are not checked — added (used by RH8020–RH8025, RH8028, RH8029, RH8034)
- SHARED-DOC-010 — A summary without text — added (used by RH8030, RH8031)
- SHARED-DOC-011 — Parameters are checked only on documentation without `<inheritdoc>` — added (used by RH8032, RH8033; written in terms of "parameters or type parameters of an element" so RH8101/RH8108 in thoenissen/Reihitsu#984 can reuse it, since their analyzers run the same logic)
- SHARED-DOC-012 — A parameter is documented by a top-level element with its name — added (used by RH8032, RH8033; same note)
- SHARED-DOC-001 — clarified: the coverage sentence now excludes every partial element declared in several parts (type, constructor, method, property, indexer, event) instead of only types and constructors. Normative sentence unchanged.
- SHARED-DOC-002 — clarified: "extension block" added to the containers in which a declaration without an access modifier is private, to the matching `Why:` sentence, and an RH8021 example of extension-block members. C# gives such a member `Accessibility.Private` (checked with `GetDeclaredSymbol`), and the code (`HasImplicitPrivateAccessibility`, an `ExtensionBlockDeclarationSyntax` being a `TypeDeclarationSyntax`) already puts it in the private group; existing tests `VerifyDiagnosticForImplicitlyPrivateExtensionMemberMethod` (RH8021) and `VerifyNoDiagnosticForImplicitlyPrivateExtensionMemberMethod` (RH8020) show it. No verdict of RH8001–RH8019 changes, because no type, enum, delegate, constructor, or destructor can be declared in an extension block.
- SHARED-DOC-005 — clarified: same coverage change as SHARED-DOC-001; SHARED-DOC-006 and SHARED-DOC-007 inherit it ("covers what SHARED-DOC-005 covers").
- Intro paragraph and two non-goals of the shared spec reworded to name what SHARED-DOC-009 to SHARED-DOC-012 add (summary text: RH8030 and RH8031; parameters: RH8032, RH8033, RH8101–RH8111; return values: RH8105–RH8107).

For the lead: the four "clarified" entries edit the text of requirements already on `main`. The precedent `squad/decisions/2026-10-18-rh8107-shared-doc-reference-clauses.md` asked the maintainer before such an edit. Without the SHARED-DOC-001/005 coverage edit, those requirements would specify that each part of a partial method, property, indexer, or event is decided on its own — the current behavior that Q1 below marks as contradicting P8. Without the SHARED-DOC-002 edit, its "otherwise" clause would put an extension-block member without modifier in the non-private group, which the code does not do. The fallback, if the maintainer rejects the edits, is a coverage sentence in each of RH8020-001 … RH8029-001 and a new requirement for extension-block members instead.

Also updated: `sdd/specs/requirement-ids.txt` (35 IDs inserted in sort order, no line moved or removed) and `sdd/specs/README.md` (15 index rows). No rule page under `documentation/rules/` changed: the documented behavior of RH8020–RH8034 is unchanged, and the pages already describe the explicit-interface-implementation exemption, the override behavior, and the RH8032/RH8033 exemptions.

## Examples for the tests

RH8020-001, RH8021-001, RH8022-001, RH8023-001, RH8024-001, RH8025-001, RH8026-001, RH8027-001, RH8028-001, RH8029-001, RH8030-001, RH8031-001, RH8031-002, RH8032-001, RH8033-001, RH8034-001 (each `-002`/`-003` location requirement refers to these blocks); SHARED-DOC-002 (new RH8021 block), SHARED-DOC-009 (RH8021 and RH8020 blocks), SHARED-DOC-010 (RH8030 block), SHARED-DOC-011 (RH8032 block), SHARED-DOC-012 (RH8032/RH8033 blocks, one with `docs.xml`).

Existing tests that already show behavior of these requirements (not yet tagged): all methods of `RH8020…Tests` to `RH8034…Tests` under `src/Reihitsu.Analyzer.Test/Documentation/`; the `VerifyNoDiagnosticsWhenDocumentationModeIsNone` method of each covers SHARED-DOC-008 for its rule; `RH8034…VerifyNoDiagnosticForExplicitlyImplementedInterfaceOperator` covers SHARED-DOC-009; `RH8030…VerifyNoDiagnosticForEmptySummaryNestedInRemarks` and `RH8031…VerifyDiagnosticForSummaryNestedInRemarks` cover the "top level" qualifier.

## Open questions to file

No open `SDD open questions` issue exists for any of the specs below (thoenissen/Reihitsu#1037 is closed), so each target needs a new issue, a sub-issue of thoenissen/Reihitsu#912. Every input was run as described above, in `Diagnose` mode; the shared-logic cases behave the same in `Parse` mode. None of these is another instance of thoenissen/Reihitsu#1092: the parts of a partial member must carry identical access modifiers (CS8799), so no partial member can be split between the private and non-private rules, and no static constructor is in this batch. No case matches P1, P2, P3, P5, P6, or P7 (none of these rules has a code fix, renames, or reads literal text).

### Target: new issue `SDD open questions: sdd/specs/shared/element-documentation.md`

#### Q1 — Each part of a partial method, property, indexer, or event is decided on its own

**Decided by principle P8 (#912).** Violated requirement: P8 as decided in thoenissen/Reihitsu#1091 — the compiler merges the parts of a partial element into one documentation; the case is left out of SHARED-DOC-001 and SHARED-DOC-005 by their coverage sentences.

Input (RH8020, RH8022, RH8024, RH8028):

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

Real output: RH8020 on the implementing `M1` and on the defining `M2`; RH8022 on the implementing `Prop`; RH8024 on the implementing `this`; RH8028 on the implementing `E`. Neither part of `M3` is reported. The compiler writes one documentation entry per member, and each has a `<summary>` (`M:P.M1` "Defining", `M:P.M2` and `M:P.M3` "Implementing", `P:P.Prop`, `P:P.Item(System.Int32)`, `E:P.E` "Defining").

Why it looks wrong: every member is documented in the generated documentation, yet an undocumented part is reported; a part that carries only `<remarks>` (`M3`) is accepted because the semantic fallback reads the merged documentation. Under P8 nothing here is reported. The private-group rules behave the same way (`partial void M(); partial void M() { }` reports both parts with RH8021).

Touches: SHARED-DOC-001, SHARED-DOC-005, SHARED-DOC-006, SHARED-DOC-007; RH8020-001, RH8021-001, RH8022-001, RH8023-001, RH8024-001, RH8025-001, RH8028-001, RH8029-001.

#### Q2 — A summary whose only content is markup without text counts as text

Input (RH8030, RH8031):

```csharp
/// <summary><para/></summary>
public class A { }

/// <summary><!-- later --></summary>
public class B { }

/// <summary><![CDATA[]]></summary>
public class C { }

/// <summary><c></c></summary>
public class D { }

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

Real output: nothing is reported by RH8030 or RH8031. Any element child, any XML comment, and any CDATA section, even an empty one, makes a summary count as having text.

Why it looks wrong: SHARED-DOC-010's `Why:` and the RH8030 rule page argue from what the tooltip shows; for `A`, `B`, `C`, `D`, and both extension blocks it shows nothing, and an XML comment is never shown at all. `E` shows the referenced name.

Readings: (A) as today, any element, comment, or CDATA counts — nothing reported; (B) only content that renders text counts — `A`, `B`, `C`, `D`, and both extension blocks reported, `E` not.

Touches: SHARED-DOC-010 (left out by its coverage sentence), RH8030-001, RH8031-002.

#### Q3 — Parameter documentation and `<inheritdoc>` are matched by local name, case-insensitively, and in any comment the syntax holds

The `<inheritdoc>` shapes are **Decided by principle P8 (#912)** (thoenissen/Reihitsu#1091, from #1037 Q1 and Q5: tag names are case-sensitive and namespace-free, and only the documentation comments the compiler keeps count). Violated requirement: SHARED-DOC-011, whose coverage sentence leaves these shapes out. The `<param>`/`<typeparam>` shapes are a normal question.

Input (RH8032, RH8033):

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

Real output: RH8032 and RH8033 report nothing. `<PARAM>`, `<TYPEPARAM>`, and `<x:param>` count as documentation of `value` and `T`; `<INHERITDOC/>`, `<x:inheritdoc>`, and the `<inheritdoc/>` in the first comment, which the compiler drops (CS1587), exempt their declarations. The compiler writes `<PARAM>` and `<TYPEPARAM>` unchanged into the generated file.

Why it looks wrong: XML names are case-sensitive, and the compiler, the tooltip, and documentation tools pair only `<param>`/`<typeparam>` in no namespace with a parameter. P8 already decided the same question for `<summary>` and `<inheritdoc>` on the presence rules. RH8101 and RH8108 run the same code (`XmlDocumentationExpander.GetExpandedElements`, `HasInheritdoc`), so they behave the same way.

Readings for the `<param>`/`<typeparam>` shapes: (A) as today — matched by local name, any case, any namespace; (B) only `param`/`typeparam` in lower case and in no namespace count — `value` and `T` of the first block and `value` of the second are reported.

Touches: SHARED-DOC-011, SHARED-DOC-012, RH8032-001, RH8033-001.

### Target: new issue `SDD open questions: sdd/specs/analyzer/RH8030.md`

#### Q1 — An explicit interface implementation's empty summary is not reported

Input:

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

Real output: RH8030 reports the summary of `Close` only. The exemption comes from `DocumentationAnalysisUtilities.NeedsDocumentation`, which RH8030 shares with the presence rules.

Why it looks wrong: the exemption's reason (SHARED-DOC-009: the interface member carries the description, so the implementation needs none) does not apply to a summary the user did write; the RH8030 rule page says every `<summary>` tag must contain text, and here the user wrote one that says nothing.

Readings: (A) as today — explicit implementations are exempt from RH8030 too; (B) the empty summary is reported like any other.

Touches: RH8030-001 (left out by its coverage sentence).

#### Q2 — Only the first top-level `<summary>` of the first documentation comment is checked

Related to P8 but not marked as decided: P8 states when an element is documented, not which summary RH8030 checks.

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

/// <summary></summary>
// c
/// <remarks>R</remarks>
public class B { }

/// <summary>Text</summary>
/// <summary></summary>
public class C { }

/// <include file='empty.xml' path='doc/member[@name="E"]/*'/>
public class D { }

/// <Summary></Summary>
/// <remarks>R</remarks>
public class F { }
```

Real output: RH8030 reports the summary of `B` (in a comment the compiler drops with CS1587) and the `<Summary>` of `F`. It does not report `A`, `C`, or `D`. RH8001 reports none of these classes, so `A` and `D` pass both rules with an empty summary in their generated documentation.

Why it looks wrong: SHARED-DOC-006 and SHARED-DOC-007 count a summary in a later comment and in an included file as the element's summary, but RH8030 does not check them; it does check a summary the compiler discards (`B`) and an element P8 says is not the summary (`F`, `<Summary>`). The rule page says every `<summary>` tag must contain text, yet the second one in `C` is not reported.

Readings: (A) as today — the first top-level `<summary>` of the first documentation comment, matched case-insensitively; (B) the summary of the documentation the compiler produces — `A` and `D` reported (on the empty summary, or on the `<include>` element for `D`), `B` and `F` not; (C) every top-level `<summary>` in the documentation comments the compiler keeps — `A` and the second summary of `C` reported, `B` and `F` not.

Touches: RH8030-001, RH8030-002 (left out by the coverage sentence of RH8030-001).

### Target: new issue `SDD open questions: sdd/specs/analyzer/RH8031.md`

#### Q1 — The summary is read only from the syntax of the first documentation comment

**Decided by principle P8 (#912)** (thoenissen/Reihitsu#1091; #1037 Q1, Q2, Q5): RH8031 is the presence rule of extension declarations, and P8 decides presence by the documentation the compiler produces. Violated requirement: RH8031-001, whose coverage sentence leaves these shapes out.

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

Real output: RH8031 reports the `extension` keyword of the `string` and `int` blocks. It does not report the `long` block (its summary is in a comment the compiler drops, CS1587), the `byte` block (`<Summary>`), or the `short` block (malformed XML, CS1570, "Badly formed XML comment ignored" in the generated file). The compiler's documentation of the `string` block contains "From the file", and that of the `int` block contains "Later".

Why it looks wrong: the verdict disagrees with the generated documentation in every block. Under P8 the `string` and `int` blocks have a summary and are not reported; the `long`, `byte`, and `short` blocks have none and are reported.

Touches: RH8031-001, RH8031-003.

#### Q2 — `<inheritdoc>` on an extension declaration is reported by RH8031 but exempts it from RH8032 and RH8033

Input:

```csharp
/// <summary>X</summary>
public static class Extensions
{
    /// <inheritdoc cref="Extensions"/>
    extension(char value) { }
}
```

Real output: RH8031 reports the `extension` keyword; RH8032 does not report `value`, because SHARED-DOC-011 exempts a declaration with `<inheritdoc>`.

Why it looks wrong: SHARED-DOC-005 counts `<inheritdoc>` as documentation for every other element, and RH8032/RH8033 treat the block as documented by it, while RH8031 treats it as having no summary. The two readings of the same comment disagree.

Readings: (A) as today — RH8031 requires a `<summary>` written on the block, `<inheritdoc>` is not enough; (B) `<inheritdoc>` documents the block as SHARED-DOC-005 specifies — not reported.

Touches: RH8031-001, SHARED-DOC-011 (left out of RH8031-001 by its coverage sentence).

### Target: new issue `SDD open questions: sdd/specs/analyzer/RH8032.md`

#### Q1 — An unnamed extension parameter is reported, with no source location

Input:

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

Real output: one RH8032 diagnostic whose location is not in source (`Location.IsInSource` is false; it shows at line 1, column 1 with an empty span). The analyzer reports `parameter.Identifier.GetLocation()` for a parameter without an identifier, and no `<param>` can carry an empty name. The compiler reports nothing.

Why it looks wrong: a parameter without a name has nothing a `<param>` could document, so the diagnostic can never be resolved except by suppression, and it points at no code.

Readings: (A) an unnamed parameter is not reported; (B) it is reported, located on the parameter's type (`long`).

Touches: RH8032-001, RH8032-002 (left out of RH8032-001 by its coverage sentence).

## Non-goals written

Each spec's non-goals name the neighboring rules (private/non-private counterpart, other element kinds, summary text RH8030, parameter/return/type-parameter rules RH8101/RH8104/RH8105/RH8108/RH8111, RH8306 for property summary wording) so the run is not broadened; no open question is written as a non-goal.
