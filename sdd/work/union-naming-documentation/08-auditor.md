Role: auditor
Commit: 3ccbe182cd0d6d85f8d92891c0393a169d185d7f
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `3ccbe182cd0d6d85f8d92891c0393a169d185d7f` (checked out, clean). `origin/main` `1d2d29a970cd9144d0527e1c505303f2ef164a66` is an ancestor of the head (`git merge-base --is-ancestor`). The diff (62 files) is what handovers 01–07 describe: four rule types, two list entries and one summary in `DocumentationAnalysisUtilities`, resources, package README, `Release.ruleset`, `Reihitsu.sln`, specs, change doc, rule pages, and tests. `scripts/lane-check.sh behavior-change` → confirmed. Issue: thoenissen/Reihitsu#1766.

Probes ran in a throwaway worktree of the head (deleted, nothing committed), in Debug, with test classes on the repository's own `AnalyzerTestsBase` and `UnionSupport`. `scripts/apply-fix.sh RH4012` ran over fixtures in the scratchpad.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Analyzer/Core/XmlDocumentationExpander.cs:169`, reached from `DocumentationAnalysisUtilities.cs:455` (RH8035, RH8036), `RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.cs:103`, `RH8108GenericTypeParametersMustBeDocumentedAnalyzer.cs:63` | FC-016, FC-033 | – | PR-introduced | When the documentation mode is `Diagnose` (the mode `GenerateDocumentationFile` gives), `ISymbol.GetDocumentationCommentXml` on a union symbol whose documentation comment holds a `<param name>` or `<paramref name>` element throws `NullReferenceException` inside Roslyn 5.12 (`BinderFactory.BinderFactoryVisitor.VisitXmlNameAttribute` ← `DocumentationCommentWalker` ← `XmlDocumentationExpander.ParseExpandedDocumentation`). Every rule that now registers `UnionDeclaration` and calls the expander crashes with AD0001. Counterexamples below. No test covers it: the union tests run with the default mode only on inputs without such an element, and the two tests that hold one (RH8102, RH8104) switch to `Parse`, where the crash does not occur. | Refuse this class: for a union whose documentation comment holds a `param` or `paramref` element, do not ask the symbol for its documentation (decide on the direct syntax), or otherwise keep the exception from leaving the analyzer; add a test per affected rule under the default mode. |

Counterexamples for #1 (each with `UnionSupport` referenced and `CompilerDiagnostics.None`, so the compiler's own CS1569 does not mask the result; default documentation mode):

```csharp
// RH8203 — AD0001 (NullReferenceException)
/// <summary>
/// Shape
/// </summary>
/// <param name="Circle">Circle</param>
public union Shape(int, string);

// RH8203 — AD0001, the same with <paramref> nested in the summary
/// <summary>
/// Shape <paramref name="x"/>
/// </summary>
public union Shape(int, string);

// RH8035 — AD0001 instead of RH8035 on Shape (no direct <summary>, so the expander runs)
/// <param name="Circle">Circle</param>
public union Shape(int, string);

// RH8036 — AD0001 instead of RH8036 on Inner
/// <summary>
/// Outer
/// </summary>
public class Outer
{
    /// <remarks>See <paramref name="x"/></remarks>
    private union Inner(int, string);
}

// RH8108 — AD0001
/// <summary>
/// Result
/// </summary>
/// <typeparam name="T">Value</typeparam>
/// <param name="x">X</param>
public union Result<T>(T, string);
```

Class enumeration for #1. Axes: rule that registers `UnionDeclaration` × whether it calls the expander on that input × element that binds a name (`param`, `paramref`, `typeparam`, `cref`) × documentation mode.

- Fail: RH8203 (`param`, `paramref`; it expands every documented union that is not a partial member); RH8035 and RH8036 (`param`, `paramref`, only when the comment has no direct `<summary>` or `<inheritdoc>`); RH8108 (`param` on a generic union with a documentation comment).
- Hold: RH8030, RH8109, RH8110, RH8111 (syntax only, no expander; RH8030 and RH8111 probed with `<param>`); RH8035/RH8036 with a direct `<summary>` next to `<param>` (probed, the expander is not reached); `<typeparam>` and `cref` on a union (the existing default-mode tests of RH8108–RH8111 and `VerifyNoDiagnosticForInheritdocWithCrefOnUnion` pass); mode `Parse` (RH8203 with `param` and `paramref`, RH8035 with `param` only: probed, no exception); mode `None` (the rules do not run, SHARED-DOC-008); a method with `<param>` in a union body under RH8101 (probed, no exception); a positional record with `<param>` under RH8203 (control, no exception).
- On `main` no documentation rule calls the expander on a union symbol (RH8101–RH8105 use `ParameterOwnerKinds`/`ReturnValueOwnerKinds`, which hold no union), so the crash is new with this change.

A caller reaches it: the analyzer package runs these rules on a user's compilation, and a `<param>` on a union is what a user who documents a positional record writes. The compiler reports CS1569 for the same input when it writes the documentation file, but the AD0001 comes on top of it, also in the IDE, and disables the rule's result for that declaration.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which declarations RH8203 and RH8030 check (`DocumentableDeclarationKinds`, `SummaryDocumentationKinds` + union) | node kind | `CanInheritDocumentation` (`TypeDeclarationSyntax.BaseList`), `GetDiagnosticLocation` (`TypeDeclarationSyntax.Identifier`) | both take a union through their `TypeDeclarationSyntax` arm; the compiler-added `IUnion` is not in `BaseList`, as RH8203-001 states | ok |
| Which declarations RH8108–RH8111 check (`TypeParameterOwnerKinds` + union) | node kind | `GetTypeParameters` (`TypeDeclarationSyntax { TypeParameterList: not null }`) | agree; a non-generic union reaches RH8109–RH8111 as every non-generic type does | ok |
| RH8101–RH8104 do not check a union (`ParameterOwnerKinds` unchanged) | node kind | `GetParameters` has a `TypeDeclarationSyntax { ParameterList: not null }` arm that would return a union's nameless case types | the arm is reached only through `ParameterOwnerKinds`, which excludes union; the four characterization tests falsify a union entry (RH8101 reports the unnamed parameters, RH8102/RH8103/RH8104 report the elements) | ok |
| RH8035/RH8036 group of a union | modifiers, direct parent | `IsPurePrivateDeclaration`, `HasImplicitPrivateAccessibility` (`TypeDeclarationSyntax` other than interface → private) | matches SHARED-DOC-002 as amended: union body → private, interface → non-private, `private protected`/`file` → non-private | ok |
| RH4012 reports | `UnionDeclarationSyntax.Identifier` | `CasingCodeFixProviderBase<UnionDeclarationSyntax>.GetIdentifier` (`Identifier.ValueText`) | same token; `@pet` reported and renamed (tested) | ok |
| Language-version gate for `SyntaxKind.UnionDeclaration` | parser | none needed | probed: under C# 14, `class union { } class C { union get(int a) { … } } union pet(int, string);` parses as class, class, method, global statement — no `UnionDeclaration` reaches the rules | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Type-name casing and its rename fix | `rg -n "CasingAnalyzerBase\|CasingCodeFixProviderBase<" src/Reihitsu.Analyzer/Rules/Naming src/Reihitsu.Analyzer.CodeFixes/Rules/Naming`: every type-casing rule, RH4012 included; RH4012 analyzer and fix differ from RH4011 only in ID, resources, kind, and node type (`diff`) | one owner |
| Element documentation presence and groups | `SplitElementDocumentationAnalyzerBase` (RH8001–RH8016, RH8035, RH8036); grouping in `DocumentationAnalysisUtilities.IsPurePrivateDeclaration`/`HasImplicitPrivateAccessibility`; RH8036 differs from RH8010 only in ID, resources, kind | one owner |
| Union kind lists for documentation rules | `rg -n "UnionDeclaration" src --glob '!*Test*/**'`: `DocumentationAnalysisUtilities.cs:32,73`, the three new rules, and the RH5404 comment (layout, thoenissen/Reihitsu#1762) | one owner |
| Expanded documentation of a symbol | `XmlDocumentationExpander.ParseExpandedDocumentation` only; callers `DocumentationAnalysisUtilities.cs:455,505`, RH8032, RH8033, RH8101, RH8105, RH8108, RH8203 | one owner; see #1 |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH4012…VerifyDiagnosticForUnionWrongCasing`, `UnionsAreChecked…`, `FixRenamesUnionAndEveryReference`, `VerbatimUnionNameIsReportedAndRenamed`, `FixRenamesEveryPartOfAPartialUnion` | exact span and exact fixed text | another span, a missed reference, an extra edit | `Verify(source, fixed)` (exact text, iteration count from the library) | ok |
| `RH4012…NoFixIsOffered…GeneratedValueMember`, `…RebindACaseType` | fix withheld, diagnostic kept | a fix that changes the text | `Verify(source, source)`, the convention of RH4003/RH4010 no-fix tests | ok |
| `RH4012` Fix All scenario | Fix All over a union used as another's case type | stale position after the first rename | `GetFixAllScenario` | ok |
| RH8035/RH8036 tests | exact spans per group and container | a report on the other group, a struct, or a documented union | `Verify` with markup | ok |
| RH8030, RH8108–RH8111, RH8203 union tests | exact element/identifier span | a missing or extra report | `Verify` with markup | ok, but none holds a `<param>`/`<paramref>` under the default mode (#1) |
| RH8101, RH8103 union tests | no report for a union | a union entry in `ParameterOwnerKinds` | `Verify` | ok |
| RH8102, RH8104 union tests | no report for a union, with a reported method sibling | a union entry in `ParameterOwnerKinds` | `Verify` + `ApplyDocumentationModeParseToTestProject` | ok for their rules; the mode switch is why #1 stayed hidden (FC-033) |
| RH8020/RH8021 union-body tests, RH4105/RH4122 characterization | unchanged behavior on `main` | a report on a case type or a group change | `Verify` | ok |

## Failure classes dismissed

- FC-001, FC-002, FC-003 — no predicate with a span or qualifier is added; the changes are kind-list entries (see *Guard scope*).
- FC-004, FC-005, FC-019, FC-020, FC-021 — no token gap is edited; the only rewrite is a Renamer rename of identifier tokens.
- FC-006 — language-version gate probed (C# 14 produces no union); the `value` → `Value` collision and case-type rebinding are withheld (tested); `nameof`, `<see cref>`, generic self-reference renamed correctly (`apply-fix.sh RH4012`, LF and CRLF, 1 iteration).
- FC-007 — fixed names pass RH4012 (Fix All and single tests). RH4001 (file name) may report after any type rename; that is shared by RH4002–RH4011 and file-level, not this change.
- FC-008, FC-009, FC-018, FC-023, FC-024, FC-036 — covered by the tests above and the probes; a namespace collision (`union shape(Shape, int)` in a static class with a sibling class `Shape`) gives `no-fix-offered`.
- FC-010–FC-014, FC-028, FC-029, FC-038–FC-043 — formatter, CLI, and the precheck are not involved; names and documentation presence are content the formatter does not judge.
- FC-015 — see *Policy ownership*.
- FC-017 — see *Assertion adequacy*.
- FC-022, FC-025, FC-026, FC-027 — no literal, expression body, brace, or repeated-token edit; nesting covered (union in union, class, interface, struct, record).
- FC-030 — the rename fix does not format.
- FC-031 — the one changed summary (`ParameterOwnerKinds`) describes the new decision; the other summaries are kind-neutral.
- FC-032, FC-035 — expected values come from the specs; the test implementer's harness check used stubs only to prove inputs compile.
- FC-034 — the issue reports no shape; the change doc's examples are used verbatim.
- FC-037 — no text-only claim is made.

Visible output: every expected value of a visible surface the diff adds is covered by maintainer answers 1A–5A (`00-lead.md`, change doc `Decisions`) or by unchanged requirements (SHARED-CASING-009 for the withheld fixes; SHARED-DOC-001/002 and RH8101–RH8104 for behavior unchanged from `main`). The rule pages RH4012, RH8035, RH8036 and the RH8030 page edit are covered by answer 1A. No `TODO`, no tracker number in added source. Scope: nothing outside the run.

## Architecture findings

_None._ #1 is one decision with one owner (the expander) that newly receives a symbol kind Roslyn 5.12 cannot document under `Diagnose`.

## Hints

- `XmlDocumentationExpander.ParseExpandedDocumentation` lets any exception from `GetDocumentationCommentXml` leave the analyzer; only `XmlException` from parsing is caught. Whether the expander should shield every caller is the owner's choice when repairing #1.
- The stale RH5404 comment about unions is already recorded for thoenissen/Reihitsu#1762 (`00-lead.md`).
