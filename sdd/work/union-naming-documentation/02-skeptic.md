Role: skeptic
Commit: 2a43412
Verdict: output question — the premise holds and the spec diff is sound in substance; the spec author's output questions 1–5 stand as written, and three spec examples must be amended before tests are derived (they do not compile)

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

Real. A caller reaches it: the analyzer package runs RH4002–RH4011 and RH8001–RH8034 on every compilation, and a C# 15 project (the analyzer references Roslyn 5.12 since #1779) declares unions as ordinary source.

- Every type-casing rule registers one syntax kind through `CasingAnalyzerBase` (`src/Reihitsu.Analyzer/Base/CasingAnalyzerBase.cs:121`, `RegisterSyntaxNodeAction(OnAnalyzeNode, _types)`); RH4003 passes only `SyntaxKind.StructDeclaration`. RH8001–RH8016 each register one kind (listed by `rg -o 'SyntaxKind\.\w+'` over `RH80[0-3]*Analyzer.cs`); none is `UnionDeclaration`. `DocumentationAnalysisUtilities` (`src/Reihitsu.Analyzer/Core/DocumentationAnalysisUtilities.cs:26-91`) lists no union in `DocumentableDeclarationKinds` (RH8203, and RH8030 through `SummaryDocumentationKinds`), `ParameterOwnerKinds` (RH8101–RH8104), or `TypeParameterOwnerKinds` (RH8108–RH8111).
- Run: `scripts/apply-fix.sh RH4003 <fixture>` over `public union pet(int, string);` next to `public struct shape { private int _x; }` → `struct shape` fixed to `Shape` under LF and CRLF, `union pet` untouched (no diagnostic on it).
- Roslyn 5.12, reflected in a scratch console app: `UnionDeclarationSyntax` derives `TypeDeclarationSyntax ← BaseTypeDeclarationSyntax`, has `Identifier`, `TypeParameterList`, `ParameterList`, `BaseList`, `Members`; no union API carries `[Experimental]` (the only `ExperimentalAttribute`s in the assembly are RSEXPERIMENTAL001 semantic-model members and RSEXPERIMENTAL006 memory-safety members). So registering `SyntaxKind.UnionDeclaration` needs no suppression (escalation 6 does not arise), and the RH5404 comment that says otherwise is stale (spec author's follow-up note; belongs to thoenissen/Reihitsu#1762).
- The code facts the spec diff calls "clarifications" hold, compiled with the .NET 11 SDK (`net11.0`, `LangVersion 15`): a member without modifier in a union body is private (`CS0122: 'Holder.Implicit()' is inaccessible` from another type; same for a nested `union Nested`), and `HasImplicitPrivateAccessibility` puts it in the private group because a union is a `TypeDeclarationSyntax` other than an interface. A union nested in an interface without modifier is accessible from outside (non-private), matching SHARED-DOC-002.
- `GetDiagnosticLocation` returns `typeDeclaration.Identifier` for every `TypeDeclarationSyntax` (`DocumentationAnalysisUtilities.cs:187`), and `CanInheritDocumentation` decides a type by `BaseList?.Types.Any()` (`:355-358`): both already give the RH8035/RH8036-002 location and the RH8203 reading the spec writes, once a union reaches them.

The spec author's claim that every example compiles is wrong for three blocks (see *Breaking inputs*, item 1).

## Mechanism and candidates

_N/A_ (new behavior). Candidate set derived from the dispatch code, not the issue:

- Type kinds that `TypeDeclarationSyntax` spans (Roslyn 5.12): class, struct, interface, record, record struct, union (extension blocks are `ExtensionBlockDeclarationSyntax`). Each per-kind naming/documentation analyzer and each list in `DocumentationAnalysisUtilities` omits only union.
- Rules that register `SyntaxKind.Parameter` and so meet a union's nameless case types today: RH4105 (spec author: skips empty identifiers), RH4122 (requires `HasSingleLetterName(parameterSymbol.Name)`, false for an empty name), RH4123/RH4124 (local functions, lambdas: not reached). None reports a case type.
- Kind-agnostic documentation rules (RH8301–RH8309 structured-trivia or tree actions, RH8204) already see a union's doc comment; not changed.

## Breaking inputs

1. **Spec examples that do not compile** (must be amended; FC-006). A union gets a compiler-generated property `Value` (reflected: `object Value`, `get_Value`, `.ctor(Cat)`, `.ctor(Dog)`, interface `IUnion`), so a union named `Value` is `CS0542: 'Value': member names cannot be the same as their enclosing type`. Compiling every `union` block of the changed specs, the change doc, and the three new rule pages (markup stripped, each block in its own namespace) gives CS0542 in exactly:
   - `sdd/specs/shared/element-documentation.md:52` — SHARED-DOC-001 union example (`public union Value(int, string) { … }`);
   - `sdd/specs/shared/element-documentation.md:182` — SHARED-DOC-002 union example;
   - `sdd/specs/analyzer/RH8036.md:32` — RH8036-001 example (the last union, `public union Value(int, string) { private union Nested … }`).
   Every other block compiles (the change doc's RH8203 blocks need the `IShape` it says is declared elsewhere). A test derived from the three blocks either fails on CS0542 or locks in a non-compiling input. Renaming the union (for example `Holder`) changes no visible output of the rules.
2. **RH4012 fix — renames whose result differs from a struct's** (FC-036, FC-018; SHARED-CASING-009 decides both, no new question):
   - `public union value(int, string);` → derived name `Value` collides with the generated `Value` property (CS0542 after the rename, none before) → the fix must be withheld.
   - `public class Outer { private union cat(Cat, Dog); }` with top-level `class Cat` → renaming to `Cat` compiles without error but silently rebinds the case type: reflected, `private union Cat(Cat, Dog)` gets `.ctor(Outer+Cat)`, the union itself, not the class. SHARED-CASING-009 (binding change) requires withholding. The shared machinery handles the record analog: `scripts/apply-fix.sh RH4010` over `private record cat(Cat Animal, Dog Other);` in `Outer` → `no-fix-offered` (LF and CRLF). The union case is a test candidate, not yet observed.
   - Verbatim identifier `public union @pet(int, string);` (FC-023); a `partial union` in two parts, which compiles (FC-024: both parts renamed, or the shared partial handling); generic `union result<T>(T, string)`; a union referenced as a case type of another union (`union shape_kind(pet, long)`, RH4012-002); Fix All over two unions (FC-008); the fixed name is accepted by RH4012 (FC-007).
3. **RH8035/RH8036 group members** (SHARED-DOC-002; FC-024, FC-027). Compiled modifier set on a union: `public`, `internal`, `protected`, `private`, `protected internal`, `private protected`, `file`, `partial`, `readonly` are accepted; `ref`, `static`, `sealed`, `abstract` are CS0106, and `unsafe` gives CS0227 without `/unsafe` (not tried with it). Containers: namespace/file level, class, struct, record, interface, union body; not an extension block (CS9282). Members the tests should cover beyond the spec examples: `file union` (non-private), `protected internal union` nested (non-private), a union without modifier nested in an interface (non-private), a union without modifier nested in a struct and in a record (private), `readonly union`, a union with only `<inheritdoc/>` (documented), a union documented only with `<remarks>` (undocumented). A partial union in several parts is outside SHARED-DOC-002's coverage, as for every type.
4. **RH8203**: a union with a base list naming an interface (not reported), without one (reported); the compiler-added `IUnion` is not in `BaseList`, so the syntactic check already ignores it. A union whose base list names an interface and that carries `<inheritdoc cref="…"/>` (not reported, existing cref rule).
5. **RH8101–RH8104 stay silent on unions** (issue's second "Done when"): a documented union with no `<param>`, with `<param name="Circle"></param>`, and with `<param>` without a name — no diagnostic from any of the four; RH4105 and RH4122 report no case type. These are characterization tests of unchanged behavior.
6. **RH8108–RH8111 on a generic union**: undocumented type parameter, out-of-order `<typeparam>`, `<typeparam>` without name, without text; plus a union with constraint clauses (`where T : class`, compiles).

Failure classes kept: FC-006, FC-007, FC-008, FC-016 (the sibling kind lists in `DocumentationAnalysisUtilities` must all be decided: union added to `DocumentableDeclarationKinds` and `TypeParameterOwnerKinds`, deliberately not to `ParameterOwnerKinds`), FC-018, FC-023, FC-024, FC-027, FC-036. FC-013/FC-014 do not apply: naming and documentation presence are content the formatter does not judge.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzer — new RH4012, RH8035, RH8036 | must change (new) |
| Analyzer — `DocumentableDeclarationKinds` (RH8203, RH8030), `TypeParameterOwnerKinds` (RH8108–RH8111) | must change |
| Analyzer — `ParameterOwnerKinds` (RH8101–RH8104), `ReturnValueOwnerKinds` | must stay |
| Analyzer — RH4003, RH8003, RH8004 and the other per-kind rules | must stay (code); spec non-goals only |
| Code fix — new RH4012 rename fix with Fix All, on the shared casing fix | must change (new) |
| Analyzer resources, `Release.ruleset`, solution items for the new pages | must change |
| Formatter, CLI, `Reihitsu.Core` | not involved |
| Rule docs — RH4012, RH8035, RH8036 new; RH8030 | must change (done by the spec author) |
| Test infrastructure — compiling a union in the verifier path | must change (see *Internal choices*) |
| RH5404 and its stale comment | not involved (#1762) |

## Refuse option

Refusing means leaving unions unreported by every naming and documentation rule — today's behavior, zero production lines. It is not a rare class: once a project uses C# 15 unions, every union declaration is outside the rules, and a union is the only type kind with no casing or documentation check. The requested change costs three small analyzers on existing bases (`CasingAnalyzerBase`, `SplitElementDocumentationAnalyzerBase`), one rename fix on the shared casing fix, and two list entries; no new mechanism or type beyond the rule classes.

Declaring a shape unsupported is not an option here: no formatter phase or code fix carries a special case for unions in this work, and the formatter is not involved.

Simpler option: widen RH4003/RH8003/RH8004 to unions (the spec author's question 1, option B) — fewer files, but it puts a different construct under a struct rule's ID (P11). The question already carries it.

## Output questions

The spec author's questions 1–5 (`01-spec-author.md`) cover every visible output of this issue I could find, and I agree with each recommendation (A). Nothing new: the two RH4012 rename hazards above are decided by SHARED-CASING-009 (withhold), and every RH8035/RH8036 member above is decided by SHARED-DOC-002.

## Internal choices

1. **Amend the three non-compiling examples** (SHARED-DOC-001, SHARED-DOC-002, RH8036-001) by renaming the union `Value`, before the test implementer starts. Alternative: leave them — tests then fail on CS0542 or encode invalid code. Recommendation: amend; route to the spec author.
2. **Compiling a union in analyzer tests** (issue's third "Done when"). Facts, all run:
   - The verifier path (`AnalyzerTestsBase`, `ReferenceAssemblies.Net.Net90`) cannot compile a union: a `net9.0` project gives `CS0518 … IUnion` and `CS0656 … UnionAttribute..ctor`.
   - A source polyfill — `namespace System.Runtime.CompilerServices { public interface IUnion { object Value { get; } } [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)] public sealed class UnionAttribute : Attribute { } }` — makes the same union compile with no diagnostic under `net9.0`, and also under `net11.0`, where the real types exist (no CS0436 or other warning).
   - The ad-hoc workspace path (`GetCodeFixActionsAsync`, `CreateTestDocument`) references `TRUSTED_PLATFORM_ASSEMBLIES` of the `net11.0` test host, which has `IUnion`; it compiles a union without a polyfill, and with one.
   - Options: (a) the polyfill as an extra source of union tests; (b) a hand-built `ReferenceAssemblies` for `Microsoft.NETCore.App.Ref` 11.0.0, which needs that package downloaded by the test framework. Recommendation: (a) — verified under both reference sets, scoped to the tests that need it, no package download. Either way, the polyfill's own declarations must not be the target of an expected diagnostic.
3. SHARED-DOC-002's *Why* lists the containers of a private static constructor as "class, struct, or record"; a union may declare a static constructor (compiled), and the requirement text already places it in the private group. Optional wording fix; no output change.

## Issue goals

- "Unions are named and documented under rules with rule pages and specs" — keep. The private rule RH8036 goes beyond the issue's wording; question 1 asks it.
- "The nameless case-type parameters never cause a parameter-documentation diagnostic" — keep; it holds today and must stay, so it is covered by characterization tests (*Breaking inputs* 5), not by a change.
- "Analyzer tests can compile a union declaration (a .NET 11 reference set or a documented polyfill), or the tests are documented to parse only" — keep; the polyfill route is verified (*Internal choices* 2). "Parse only" is not enough for RH4012's fix (it resolves the symbol and compares compile errors) or RH8203 (semantic model).
- The issue's "Affected rules" lists RH4004, RH4005, RH4010, RH4011 among naming rules that "cover no union" — accurate but they need no change; and its file-and-line references (`:27-43` etc.) are stale positions, harmless.

## Architecture findings

_None._ The per-kind lists are deliberate, different sets; adding a type kind touches each by design.

## Follow-up candidates

- Union handling in the design, organization, and layout rules (RH2004, RH7102–RH7110, RH5404, …) is already filed as thoenissen/Reihitsu#1767 and #1762; nothing new to file.
- Roslyn 5.12 `CS1569` (NullReferenceException) writing the documentation file for a `<param>` on a union — reproduced; outside this repository.
