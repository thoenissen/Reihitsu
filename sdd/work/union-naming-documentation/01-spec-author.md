Role: spec author
Commit: ccfb57c
Verdict: change doc written

The prompt carried no intended fix or suspected cause; the issue's own "Expected" lines for its two questions are treated as proposals (the issue is `agent-filed`) and appear below as recommendations, not as decisions.

Change doc: `sdd/changes/2026-11/union-naming-documentation.md` (`Status: Draft`). Its *Visible output changes* 1–7 are the input for the output questions below.

## Changed requirements

- RH4012-001 — Union names are in PascalCase — added (new spec `analyzer/RH4012.md`)
- RH4012-002 — The fix renames the union to PascalCase — added
- RH8035-001 — Undocumented non-private unions are reported — added (new spec `analyzer/RH8035.md`)
- RH8035-002 — The diagnostic is on the union name — added
- RH8036-001 — Undocumented private unions are reported — added (new spec `analyzer/RH8036.md`)
- RH8036-002 — The diagnostic is on the union name — added
- RH8030-001 — A summary without text is reported — changed: union added to the checked declarations; union example appended
- RH8108-001 — An undocumented type parameter is reported — changed: union added; union example appended
- RH8109-001 — A `<typeparam>` element that does not follow the type parameters is reported — changed: union added; union example appended
- RH8110-001 — A `<typeparam>` element without a name is reported — changed: union added; union example appended
- RH8111-001 — A `<typeparam>` element without text is reported — changed: union added; union example appended
- RH8203-001 — `<inheritdoc>` on a declaration that inherits nothing is reported — changed: union added to the checked declarations and to "inherits when it has a base list"; the compiler-added union interface does not count; union example appended
- SHARED-DOC-001 — Every declaration of the rule's element kind is checked — clarified: union named among the containers; union example (RH8020) added
- SHARED-DOC-002 — Private and non-private groups — clarified: union named among the containers whose unmodified members are private; union example (RH8021, RH8036) added

Non-goal-only edits (no requirement change): RH4003, RH8003, RH8004 (point to RH4012/RH8035/RH8036); RH8101, RH8102, RH8103, RH8104 (a union is not checked: its case types have no names); RH8030, RH8101, RH8108 summary-rule ranges extended by RH8035 and RH8036.

Index and ledger: `sdd/specs/README.md` rows for RH4012, RH8035, RH8036; six IDs inserted in `sdd/specs/requirement-ids.txt` (additions only).

Rule pages: new `documentation/rules/RH4012.md`, `RH8035.md`, `RH8036.md`; `RH8030.md` names RH8035 and RH8036 among the rules that require documentation. Pages RH8108–RH8111, RH8203, RH8101 are unchanged: they speak of "types" in general, so the normal case reads the same.

### How the "before" outputs were established

- Registration lists, read at ccfb57c: casing rules RH4002–RH4006, RH4010, RH4011 each register one syntax kind (`CasingAnalyzerBase`); RH8001–RH8012 register explicit kinds through `SplitElementDocumentationAnalyzerBase`; RH8030 registers `SummaryDocumentationKinds`, RH8203 `DocumentableDeclarationKinds`, RH8101–RH8104 `ParameterOwnerKinds`, RH8108–RH8111 `TypeParameterOwnerKinds` (all in `DocumentationAnalysisUtilities`). None contains `SyntaxKind.UnionDeclaration`, so no union is reported today by any of them. Not run as a test; the skeptic may confirm by test.
- Members inside a union body: `DocumentationAnalysisUtilities.HasImplicitPrivateAccessibility` treats every `TypeDeclarationSyntax` parent other than an interface as private, so a member without a modifier in a union body is in the private group today — the basis for calling the SHARED-DOC-001/002 edits clarifications.
- RH4105 (parameter casing) skips a parameter whose identifier span is empty, so union case types are not reported by it today; RH4121 (type parameter names) registers `SyntaxKind.TypeParameter` and already covers union type parameters. Neither is changed.
- Compiled with the .NET 11 SDK (11.0.100, Roslyn 5.12, `net11.0`, `LangVersion 15`) in a scratch project: every C# example in the three new specs and the change doc compiles, including a union nested in a union body, `protected`/`private protected` unions nested in a class, a generic union, a union with a base list, and a union used as a field type. The compiler's own CS1711/CS1712 report `<typeparam>` mismatches on a union, so the compiler treats union type parameters as documentable.
- Fact for question 2: a `<param name="Circle">` on a union makes the compiler fail with `CS1569: Error writing to XML documentation file: Object reference not set to an instance of an object.` when `GenerateDocumentationFile` is on (Roslyn 5.12). `<inheritdoc/>` on a union builds cleanly.

## Examples for the tests

RH4012-001, RH4012-002, RH8035-001, RH8036-001, and the appended union examples of RH8030-001, RH8108-001, RH8109-001, RH8110-001, RH8111-001, RH8203-001, plus the new union examples under SHARED-DOC-001 and SHARED-DOC-002. RH8035-002 and RH8036-002 point to the -001 examples.

Facts for the test implementer (from the issue, not re-verified here except where stated): compiling a union needs .NET 11 reference assemblies; `Microsoft.CodeAnalysis.Analyzer.Testing` 1.1.4 has no `net11.0` set. `RuleDocumentationExampleVerifier` fails a page only on parse errors (`LanguageVersion.Latest`), so the new rule pages need no compilable case types; it runs RH4012's code fix on the page example.

## Output questions

All within thoenissen/Reihitsu#1766. None blocks the spec; the specs are written on the recommendations. Questions 1 and 2 are the two the issue names.

**1 · New rules for unions — which rule reports a union's name and missing documentation**

Input:
```csharp
public union pet(int, string);

/// <summary>
/// Outer
/// </summary>
public class Outer
{
    private union Inner(int, string);
}
```

Before (`main`): not reported.

After, A (recommended): three new rules — RH4012 "Union names should be in PascalCase" (with the rename fix and Fix All), RH8035 "Non-private unions must be documented", RH8036 "Private unions must be documented".
```csharp
public union {|RH4012:{|RH8035:pet|}|}(int, string);   // fix: Pet

/// <summary>
/// Outer
/// </summary>
public class Outer
{
    private union {|RH8036:Inner|}(int, string);
}
```

After, B: the struct rules widened — RH4003 "Struct names should be in PascalCase" and RH8003/RH8004 "(Non-)private structs must be documented" also report unions (same spans, IDs RH4003, RH8003, RH8004).

After, C: only RH4012 and RH8035 (as the issue names them); a private union is not reported for missing documentation.

Why A: every other type kind has its own casing rule and a private/non-private documentation pair, so a project can configure unions on their own; B widens a rule to a different construct (P11), and C leaves nested private unions as the only undocumentable type kind.

Requirement IDs: RH4012-001/002, RH8035-001/002, RH8036-001/002. Reading A is written.

**2 · RH8101–RH8104 · Element parameters must be documented (and siblings) — a union's case types**

Input:
```csharp
/// <summary>
/// Shape
/// </summary>
/// <param name="Circle"></param>
public union Shape(int, string);
```

Before (`main`): not reported.

After, A (recommended): not reported — no parameter rule checks a union; no `<param>` is required for a case type, and a `<param>` on a union is not judged.

After, B: case types are never reported as undocumented (RH8101), but a `<param>` on a union is reported as documenting no parameter (RH8102) and, here, as having no text (RH8104):
```csharp
/// <summary>
/// Shape
/// </summary>
/// {|RH8102:{|RH8104:<param name="Circle"></param>|}|}
public union Shape(int, string);
```

Why A: a case type has no name, so there is nothing for a `<param>` to document; a stray `<param>` on a union is rare and already fails the build with CS1569 when the documentation file is generated, and B would need the positional pairing of SHARED-DOC-015 to learn about parameters without names.

Requirement IDs: RH8101-001, RH8102-001, RH8103-001, RH8104-001 (unchanged under A; non-goals added). Reading A is written.

**3 · RH8030 · Element documentation must have summary text — a union's empty summary**

Input:
```csharp
/// <summary></summary>
public union Shape(int, string);
```

Before (`main`): not reported.

After, A (recommended):
```csharp
/// {|RH8030:<summary></summary>|}
public union Shape(int, string);
```

After, B: not reported.

Why A: once RH8035/RH8036 require a summary on a union, an empty one would otherwise silence them, which is exactly what RH8030 exists to catch. Requirement: RH8030-001. Reading A is written.

**4 · RH8108–RH8111 · Generic type parameter documentation — a generic union**

Input:
```csharp
/// <summary>
/// Result
/// </summary>
/// <typeparam name="TValue">Value</typeparam>
/// <typeparam name="TOther"></typeparam>
public union Result<TValue, TError>(TValue, TError);
```

Before (`main`): not reported.

After, A (recommended):
```csharp
/// <summary>
/// Result
/// </summary>
/// <typeparam name="TValue">Value</typeparam>
/// {|RH8109:{|RH8111:<typeparam name="TOther"></typeparam>|}|}
public union Result<TValue, {|RH8108:TError|}>(TValue, TError);
```
(RH8110 reports a `<typeparam>` without a name on a union in the same way.)

After, B: not reported.

Why A: a union's type parameters are named and documented like those of any generic type; the compiler itself warns about the same two problems (CS1711, CS1712). Requirements: RH8108-001, RH8109-001, RH8110-001, RH8111-001. Reading A is written.

**5 · RH8203 · Inheritdoc must be used with inheriting class — a union**

Input:
```csharp
/// <summary>
/// Contract
/// </summary>
public interface IShape
{
}

/// <inheritdoc/>
public union Shape(int, string);

/// <inheritdoc/>
public union Named(int, string) : IShape;
```

Before (`main`): not reported.

After, A (recommended): reported only on the union without a base list.
```csharp
/// {|RH8203:<inheritdoc/>|}
public union Shape(int, string);

/// <inheritdoc/>
public union Named(int, string) : IShape;
```

After, B: not reported (unions stay outside RH8203).

Why A: `<inheritdoc/>` on a union without a base list inherits nothing a reader wrote — the interface the compiler adds to every union is not written by the user, as a struct's `ValueType` is not, and RH8203 treats a struct without a base list the same way. Requirement: RH8203-001. Reading A is written.

## Internal choices

- Rule IDs and names: RH4012 (next free after RH4011), RH8035 and RH8036 (next free after RH8034); type names `RH4012UnionNameCasingAnalyzer`, `RH4012UnionNameCasingCodeFixProvider`, `RH8035NonPrivateUnionsMustBeDocumentedAnalyzer`, `RH8036PrivateUnionsMustBeDocumentedAnalyzer`, as the spec index now names them. Alternative: other free numbers; none is better.
- Union examples for the changed requirements are appended as separate `Example — a union` blocks rather than merged into the existing examples, so the tests derived from the existing blocks stay as they are. Alternative: merge into the existing blocks.
- SHARED-DOC-001/002 edits are written as clarifications (current behavior), not as behavior changes; they are still listed under the change doc's affected requirements because the doc names them. Alternative: leave the shared spec alone and let "class, struct, …" stay incomplete.
- The change doc lists the RH8101–RH8104 non-change as visible output change 6 ("before: not reported / after: not reported") so the maintainer sees question 2 answered in the doc. Alternative: omit it as no change.
- Release.ruleset entries and `Reihitsu.sln` solution items for the three new rule pages are left to the implementer and cleaner (tooling paths, not spec text); the existing rules RH4011, RH8034 have both.

## Follow-up candidates

- `RH5404ElementMustNotBeOnSingleLineAnalyzer` carries a comment saying union declarations "parse only under a preview language version and need runtime support types .NET 11 does not ship". After #1779 (Roslyn 5.12, C# 15) and with the .NET 11 SDK compiling a union without error (checked above), that comment is stale; whether RH5404 should register unions is union layout, which thoenissen/Reihitsu#1762 covers. Not a new issue: a note for #1762, if the lead wants it recorded there.
- Not a repository defect: Roslyn 5.12 crashes writing the XML documentation file for a `<param>` on a union (CS1569, NullReferenceException). Outside this repository; no follow-up issue here.
