Role: spec author
Commit: 6a10f2f
Verdict: spec written

Auditor repair round on `13-auditor.md`, findings 1 and 2. Finding 3 belongs to the test implementer. The lead's message carried no intended fix or hoped-for verdict. It did carry the lead's instruction on where a shape that leaves coverage is filed: appended to thoenissen/Reihitsu#1130 (shared Q5).

## Finding 1 — Unicode escapes and format characters in a `name` attribute

I re-derived it with my scratch harness (Roslyn 5.9.0, `LanguageVersion.Latest`, Debug `Reihitsu.Analyzer.dll`). My first fixture was wrong: the tool that wrote it decoded the backslash-u sequences into letters, so nothing was reported. With literal six-character escapes, the auditor's result reproduces in `Diagnose` and in `Parse` mode:

- `name="v\u0061lue"` on `extension(short value)`: RH8032 on `value`; the compiler gives no CS1572, and a method `M(int value, int b)` with the same `<param>` gets no CS1573.
- `name="va&#x200B;lue"`, and the same name with a literal U+200B: RH8032 on `value`, no CS1572.
- `<typeparam name="\u0054">` on `extension<T>(T[] values)`: RH8033 on `T`, no CS1711.
- `extension(sbyte \u0061)` with `name="\u0061"`: RH8032 on `\u0061`, no compiler warning.
- `extension(byte \u0061)` with `name="a"`, and `extension<\u0055>(List<U> items)` with `<typeparam name="U">`: not reported.

The compiler binds every reported shape above, so this is the shape of Q5 (padded `name`). Repair, option (a) of the auditor:

- **SHARED-DOC-012:**
  - The coverage sentence now leaves out a `name` attribute that contains a C# Unicode escape sequence or a format character (Unicode category Cf). These shapes are to be appended to thoenissen/Reihitsu#1130; the text is below.
  - The normative sentence now states the in-coverage side that the auditor found unstated: the parameter's name is the value of its identifier, without the `@` of a verbatim identifier and with a Unicode escape sequence decoded, so `name="a"` documents a parameter declared as `\u0061`. The `Why:` says the same.
  - Two declarations are added to the first example block: `extension(ushort \u0061)` with `<param name="a">`, and `extension<\u0055>(Queue<U> queue)` with `<typeparam name="U">`. Neither is reported. Both reproduce.
- **Rule pages `documentation/rules/RH8032.md` and `RH8033.md`, "How to fix it":**
  - "spell the name exactly as it appears in the declaration, without the `@` …" led to the out-of-coverage report for an escaped declaration.
  - It now reads "write the name as plain text: without the `@` of a verbatim identifier such as `@class` (`@T`), and with a Unicode escape such as `\u0061` (`\u0054`) written as the character it stands for". That is the in-coverage behavior.

## Finding 2 — RH8030-001 coverage wording

Confirmed: "has one documentation comment" excluded declarations without one. It now reads "has at most one documentation comment, with at most one `<summary>` element at its top level", as RH8031-001 does. The behavior is unchanged: without a comment, nothing is reported.

## Verification

- Every example block of `shared/element-documentation.md` and `analyzer/RH8030.md` matches the analyzers in `Diagnose` and `Parse` mode. The exception is the existing `None`-mode SHARED-DOC-008 block, which the harness does not model.
- `SpecConsistencyTests`, `SpecStructureTests`, and `RuleDocumentationConventionTests` pass.
- No ID was added.

## Tests affected (facts for the test implementer)

- **No existing test now asserts a case outside coverage.** No test in the 15 classes has a Unicode escape or a format character in a name; the auditor grepped for this, and my reading agrees.
- **The first SHARED-DOC-012 example block changed** (two declarations added), so the tests that copy it verbatim no longer match it:
  - `RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzerTests.VerifySpecificationExampleParameterIsDocumentedByTopLevelElementWithItsName`
  - `RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzerTests.VerifySpecificationExampleTypeParameterIsDocumentedByTopLevelElementWithItsName`
- **The new in-coverage qualifier has no test yet:** a parameter or type parameter declared with a Unicode escape (`\u0061`, `\u0055`) and documented by the plain name is not reported.
- **RH8030-001's text changed only in coverage wording**, so no test changes for finding 2.

## Text to append to thoenissen/Reihitsu#1130 (shared Q5)

> **Q5, continued — a `name` attribute with a Unicode escape sequence or a format character**
>
> The same shape as Q5: the compiler binds the `name`, the rule compares the attribute text with the parameter's name and reports it. SHARED-DOC-012 leaves these shapes out of its coverage. In the input below, a backslash-u sequence is the six literal characters in the source.
>
> ```csharp
> /// <summary>X</summary>
> public static class Ext
> {
>     /// <summary>Escaped attribute</summary>
>     /// <param name="v\u0061lue">V</param>
>     extension(short value) { }
>
>     /// <summary>Format character</summary>
>     /// <param name="va&#x200B;lue">V</param>
>     extension(int value) { }
>
>     /// <summary>Escaped type parameter attribute</summary>
>     /// <typeparam name="\u0054">T</typeparam>
>     /// <param name="values">V</param>
>     extension<T>(T[] values) { }
>
>     /// <summary>Escape on both sides</summary>
>     /// <param name="\u0061">V</param>
>     extension(sbyte \u0061) { }
> }
>
> /// <summary>C</summary>
> public class C
> {
>     /// <summary>M</summary>
>     /// <param name="v\u0061lue">V</param>
>     /// <param name="b">V</param>
>     public void M(int value, int b) { }
> }
> ```
>
> Real output, in `Diagnose` and `Parse` mode:
> - RH8032 reports `value` of the `short` block, `value` of the `int` block, and `\u0061` of the `sbyte` block.
> - RH8033 reports `T`.
> - The compiler reports nothing: no CS1572, CS1573, or CS1711. For `M` it pairs `v\u0061lue` with `value`, which it would report as CS1573 if it did not.
> - The same holds for a literal U+200B in place of `&#x200B;`.
> - The compiler writes the attribute text unchanged into the generated documentation, for example `<param name="v\u0061lue">`.
> - In contrast, `extension(byte \u0061)` documented by `name="a"` is not reported (SHARED-DOC-012: the parameter's name is its identifier's value, with escapes decoded).
>
> Why it looks wrong: the parameter is documented as the compiler and the generated documentation see it, yet the rule reports it. The user who writes the name the way the declaration does (`\u0061`) gets a report the compiler does not give.
>
> Readings:
> - (A) As today: the attribute text is compared as written, after XML decoding. All four are reported.
> - (B) The attribute is read the way the compiler binds it, with escapes decoded and format characters removed. None is reported.
>
> Touches: SHARED-DOC-012 (left out by its coverage sentence), RH8032-001, RH8033-001.

## Open questions

The full list in `09-spec-author.md` is unchanged except for the addition above to shared Q5 (thoenissen/Reihitsu#1130).
