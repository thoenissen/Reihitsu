Role: spec author
Commit: 76d0229
Verdict: spec written

# Scope and method

Lane: specs and tests for existing behavior. No change doc (no intended behavior changes). New specs:

- `sdd/specs/shared/modifier-ordering.md` (prefix SHARED-MODORDER, code owners `ModifierOrderingUtilities`, `ModifierOrderingCodeFixProviderBase` — the only two types claimed, as the maintainer allowed)
- `sdd/specs/analyzer/RH7001.md`, `RH7002.md`, `RH7003.md`, `RH7004.md`, `RH7101.md`, `RH7102.md`, `RH7104.md`, `RH7105.md`, `RH7106.md`, `RH7107.md`, `RH7108.md`, `RH7109.md`, `RH7110.md`

Also changed: `sdd/specs/requirement-ids.txt` (62 IDs at their sorted positions), `sdd/specs/README.md` (14 rows), `documentation/rules/RH7110.md` (one wrong cross-reference: "RH7104 governs missing modifiers" → RH2004; RH7104 covers partial types only).

How the statements were established:

- Every `csharp` block of the 14 specs was run through a scratch harness (analyzer + code-fix provider from the Debug build of 76d0229, Roslyn 5.9.0, LangVersion preview, unsafe allowed): a block with `{|RH####:…|}` markup must reproduce exactly that markup; the block directly after it must equal the single fix (one diagnostic) or one Fix All application (several); a block without markup must produce no diagnostic. Result: 0 mismatches in all 14 specs. "No code fix is offered" blocks were checked to register no action. The harness and checker live outside the repository and are not committed.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RuleDocumentation"`: 26 passed. `RequirementCoverageTests` was not run; it fails until the new requirements are tagged.
- Header rows (`Category` Organization, `Severity` Warning, enabled by default, `Code fix`) come from the descriptors (`DiagnosticAnalyzerBase` defaults) and `GetFixAllProvider` (BatchFixer for RH7004, RH7101, RH7102, RH7104–RH7109; no provider for RH7001–RH7003, RH7110).

Decisions taken in the specs:

- SHARED-ORDER is referenced, never restated: RH7102, RH7107, RH7108 use SHARED-ORDER-001 to -006; RH7109 uses -001 to -007 (only RH7103 and RH7109 apply the initializer guard). No SHARED-ORDER ID minted.
- RH2005 is referenced once: RH7105-004's `Why:` cites RH2005-003 (the RH2005 fix puts `private` first, which RH7105's order keeps — SHARED-FIX-004), so RH7105 lists RH2005-003 under `Uses`. No other rule of this batch relies on an RH2005 requirement; `DeclarationModifierUtilities` is not claimed.
- RH7101 references FMT-STRUCT-021/-022/-023 instead of restating the split.
- No Fix All requirement is written: none of these fixes differs from SHARED-FIX-002. The two-pass Fix All of RH7004, RH7102, RH7109 (#1143) is not encoded.
- Members without an accessibility modifier (#968's cause, shared by RH7102, RH7109 in classes, RH7110) are left unspecified: the requirements name only explicit accessibility modifiers, and in RH7109 the interface rule (no modifier counts as `public`), which matches #968's expected behavior. No example uses an implicit member of a class.
- RH7110-003 states the top-level-region scope on its own; it is the same behavior as RH7103-004 (same helper, `RegionDirectiveUtilities`, which may not be claimed), so no shared spec was made for it.
- RH7104-004 is qualified "when no part of the type carries an accessibility modifier"; the other side is open question D2 below, so it has no example.
- Examples avoid inputs that would show behavior filed below (whole-type/whole-member formatting, P22 inputs, struct field reorders of #1134, the #970 pragma case). Directive examples use `#pragma` or `#region`, which P22 keeps reported.

## Changed requirements

- SHARED-MODORDER-001 — The checked declarations — added
- SHARED-MODORDER-002 — The fix rewrites only the modifier keywords — added
- SHARED-MODORDER-003 — A comment or directive between the modifiers withholds the fix — added
- RH7001-001 — Every top-level namespace after the first is reported — added
- RH7001-002 — Nested namespaces are not counted — added
- RH7001-003 — The diagnostic is on the namespace name — added
- RH7002-001 — Every top-level type after the first is reported — added
- RH7002-002 — Nested types and delegates are not counted — added
- RH7002-003 — The diagnostic is on the type name — added
- RH7003-001 — A block-scoped namespace that is the file's only member is reported — added
- RH7003-002 — A namespace with another member beside it is not reported — added
- RH7003-003 — Language versions without file-scoped namespaces are not reported — added
- RH7003-004 — The diagnostic is on the namespace name — added
- RH7004-001 — A using declaration is reported — added
- RH7004-002 — A using statement is not reported — added
- RH7004-003 — The diagnostic is on the `using` keyword — added
- RH7004-004 — The fix wraps the following statements into a using statement — added
- RH7004-005 — The fix is withheld outside a block — added
- RH7004-006 — The fix is withheld when a local function or label follows directly — added
- RH7101-001 — A field declaration with several declarators is reported — added
- RH7101-002 — Event fields and locals are not reported — added
- RH7101-003 — One diagnostic per declaration, on the second declarator — added
- RH7101-004 — The fix splits the declaration — added
- RH7101-005 — The fix is withheld for a declaration with a directive — added
- RH7102-001 — A constant after a field of the same accessibility is reported — added
- RH7102-002 — Fields of another accessibility are not compared — added
- RH7102-003 — Regions do not separate the comparison — added
- RH7102-004 — The diagnostic is on the first name — added
- RH7102-005 — The fix moves the constant in front of the first field of its group — added
- RH7102-006 — The fix is withheld when the move is unsafe — added
- RH7104-001 — A partial type without accessibility modifier is reported — added
- RH7104-002 — A partial type with an accessibility modifier is not reported — added
- RH7104-003 — The diagnostic is on the type name — added
- RH7104-004 — The fix adds the type's default accessibility — added
- RH7105-001 — Modifiers out of the canonical order are reported — added
- RH7105-002 — Modifiers in the canonical order are not reported — added
- RH7105-003 — The diagnostic is on the first misplaced modifier — added
- RH7105-004 — The fix writes the canonical order — added
- RH7106-001 — A compound accessibility in the other order is reported — added
- RH7106-002 — The canonical spellings are not reported — added
- RH7106-003 — The diagnostic is on the second keyword — added
- RH7106-004 — The fix swaps the two keywords in place — added
- RH7107-001 — A `get` accessor after `set` or `init` is reported — added
- RH7107-002 — Accessor lists in order are not reported — added
- RH7107-003 — The diagnostic is on the `get` keyword — added
- RH7107-004 — The fix moves `get` in front of the first `set` or `init` — added
- RH7107-005 — The fix is withheld when the move is unsafe — added
- RH7108-001 — An `add` accessor after `remove` is reported — added
- RH7108-002 — Accessor lists in order are not reported — added
- RH7108-003 — The diagnostic is on the `add` keyword — added
- RH7108-004 — The fix moves `add` in front of `remove` — added
- RH7108-005 — The fix is withheld when the move is unsafe — added
- RH7109-001 — A readonly field after a field of the same group is reported — added
- RH7109-002 — Fields of another group are not compared — added
- RH7109-003 — Regions do not separate the comparison — added
- RH7109-004 — The diagnostic is on the first name — added
- RH7109-005 — The fix moves the field in front of the first field of its group — added
- RH7109-006 — The fix is withheld when the move is unsafe — added
- RH7110-001 — A member after a less accessible member of its group is reported — added
- RH7110-002 — Only members of the same kind are compared — added
- RH7110-003 — Each top-level region is checked on its own — added
- RH7110-004 — The diagnostic is on the member's name — added

## Examples for the tests

Every requirement above has an example to derive a test from; all are enforced (`analyzer/` and SHARED-MODORDER through `Uses`). Facts for the test implementer:

- RH7003-003 is stated in prose; the existing `VerifyUnsupportedLanguageVersionIsNotFlagged` (C# 9 solution transform) is its shape.
- RH7105-002's example contains `unsafe`; the RH7105 test class already has an `AllowUnsafe` helper. RH7101-001's example contains a fixed-size buffer and needs unsafe code allowed too.
- SHARED-MODORDER-001 has one example per rule (RH7105 block, RH7106 block); SHARED-MODORDER-002 and -003 examples are RH7105 and RH7106.
- RH7004-005's example is a top-level-statement file; it produces CS8805 in a library project, so the test needs a console output kind or must accept that compiler error.
- Existing tests whose expected fix output contains formatting outside the diagnostic's code (open questions A3, A4, D1, F1, G1 below), for example `PartialTypesWithoutAccessModifierAreReportedAndFixed` (`partial struct TestStruct { }` → `internal partial struct TestStruct;`), `BlankLineAboveReorderedPropertyIsKeptWhenAccessorBodyIsBraced`, and `VerifyCodeFixTransformsUsingDeclarationWithMultipleFollowingStatements`, pin behavior that is not specified; tagging them with a fix requirement would encode it.
- Existing tests that pin open questions B2, F3, G2 (a `#if` inside the reported construct) and the #968 cause (`ImplicitAndPrivateClassFieldsDoNotProduceDiagnostics`, `MembersWithoutExplicitAccessibilityAreIgnored`) and `PartialTypesWithPublicAndCompoundAccessibilityAreFixed` (D2) are characterization tests.

## Open questions to file

Known issues found again, not to be filed again (one-line comments for the lead, as 00-lead.md plans):

- #968 cause (written instead of effective accessibility, `OrderingAccessibilityGroup.None`): RH7102 `class C { int _a; private const int B = 1; }` → not reported, while `int _a; const int B = 1;` → `B` reported; RH7109 the same in classes (interfaces already treat no modifier as public); RH7110 skips every member without modifier (its rule page says so).
- #969 (whole-type formatting by `TypeMemberOrderingCodeFixProviderBase`): RH7102 `private int _a; public class Inner { } private const int D = 1;` → fix also turns `Inner` into `public class Inner;`; RH7109 the same base.
- #970 (balanced `#pragma` in a crossed body blocks the move): reproduced for RH7107 (`set { #pragma warning disable CS0168 … restore … }` before `get` → no fix).
- #1134 (RH7109 reorders default-layout struct fields): reproduced, `struct S { public int A; public readonly int B; }` → fix moves `B`.
- #1143 (Fix All in one application): RH7004 `using var a…; using var b…;` → one Fix All converts only `a`; RH7102 two constants after one field → one Fix All moves only the first.
- #1150 (formatter-delegating fixes offered as no-ops): applies to RH7004, RH7104, RH7105, RH7106, RH7107, RH7108 (all call `FormatNodeInDocumentAsync`); RH7101 does not delegate.

New questions, one issue per spec (`SDD open questions: <spec path>`, sub-issue of #912 — #912 is at its 100-sub-issue limit, see its comments of 2026-10-19):

### A — `sdd/specs/analyzer/RH7004.md` (new issue)

A1. Decided by principle P1 (#912), against SHARED-FIX-003 (new compile error). Input, analyzed with `DEBUG` defined:

```csharp
using System.IO;

class C
{
    void M()
    {
        using var s = new MemoryStream();
#if DEBUG
        s.Flush();
#endif
    }
}
```

Real output: `using (var s = new MemoryStream())⏎{⏎#if DEBUG⏎    s.Flush();⏎}⏎#endif` — the `#if DEBUG` travels into the new block with `s.Flush();`, the `#endif` stays in front of the method's `}`, so the new block's closing brace is inside the `#if`; compiled without `DEBUG` the result fails with CS1513. Touches RH7004-004.

A2. Comment relocation (AGENTS.md code-fix coverage; SHARED-FIX-003 does not yet cover relocation — #1136). Input `using var s = new MemoryStream(); // resource` followed by `s.Flush();` → real output puts the comment after the new block's closing brace: `}  // resource`. The comment described the declaration line. Touches RH7004-004. Not decided by a written requirement; related #1136.

A3. Decided by principle P9 (#912) (fix scope, SHARED-FIX-006 planned in #1138). Input: `using var s = new MemoryStream();⏎s.Flush( );⏎if (true) s.Flush();` in a method body → the fix also rewrites the moved statements: `s.Flush();` and `if (true)⏎{⏎    s.Flush();⏎}`; with `var n = 1;⏎return n;` following, it inserts a blank line between them. Touches RH7004-004.

A4. Decided by principle P14a (#912) (no blank line inserted before the changed node, #1138). Input `var x = 0;⏎using var s = new MemoryStream();⏎x++;` → real output inserts a blank line between `var x = 0;` and the new `using (…)`. Pinned by `VerifyCodeFixTransformsUsingDeclarationWithMultipleFollowingStatements`. Touches RH7004-004.

### B — `sdd/specs/analyzer/RH7101.md` (new issue)

B1. Decided by principle P9 (#912). Input:

```csharp
class C
{
    private int _a, _b;

    private int _c, _d;

    class N
    {
        private int _e, _f;
    }
}
```

The fix for the diagnostic on `_b` splits all three declarations, including `_c, _d` and the nested type's `_e, _f` (the provider runs `FieldDeclarationSplitTransform` over the whole containing type). Touches RH7101-004.

B2. Decided by principle P22 (#912) (SHARED-DIAG-008, planned in #1260). Input, `DEBUG` defined: `private int _a,⏎#if DEBUG⏎        _debug,⏎#endif⏎        _b;` → reported on `_debug` (no fix). P22: a construct that contains `#if`/`#endif` is not reported. Touches RH7101-001.

### C — `sdd/specs/analyzer/RH7002.md` (new issue)

C1. Possibly decided by principle P15 (#912) (a partial declaration is one declaration, SHARED-DIAG-004 planned in #1260; follow-up #1287). Input `partial class C⏎{⏎}⏎⏎partial class C⏎{⏎}` in one file → the second part is reported as a second top-level type (`partial class {|RH7002:C|}`). Under P15 both parts are one type. Touches RH7002-001. The lead decides whether P15 decides it or it is a question.

### D — `sdd/specs/analyzer/RH7104.md` (new issue)

D1. Decided by principle P9/P14d (#912). Input:

```csharp
partial class C
{
    void M( )
    {
        if (true) M();
    }

    class Empty
    {
    }
}
```

Real output: `internal partial class C` and also `void M()`, `if (true)⏎{⏎    M();⏎}`, `class Empty;` — the fix formats the whole type declaration (`FormatNodeInDocumentAsync` on the type). Touches RH7104-004.

D2. Open question, P15 (#912) gives competing readings. Input `public partial class Sample⏎{⏎}⏎⏎partial class Sample⏎{⏎}` → the second part is reported, and the fix copies the accessibility of the other part (`public partial class Sample;`); two parts that both lack a modifier are reported twice. Reading A (current): every part states its accessibility, as the rule page's `Why` argues ("reading only one part"). Reading B (P15 as worded in #1287): the type is judged by its symbol and reported once, and a fix on one part is withheld — which would leave the second part above unreported (the symbol has an accessibility) and remove the fix for multi-part types. Touches RH7104-001, RH7104-004. Recommendation: Reading A for RH7104, since stating the accessibility on every part is the rule's subject; ask the maintainer.

### E — `sdd/specs/shared/modifier-ordering.md` (new issue)

E1. Decided by principle P9/P14d (#912), and FMT-PIPE-010's `Why`. The RH7105/RH7106 fix formats the whole declaration it reorders: `override public string ToString() => "";` → `public override string ToString()⏎{⏎    return "";⏎}`; `static public C operator +(C a, C b) => a;` → block body; `internal protected int M( ) => 1;` (RH7106) → `protected internal int M()` with a block body; on a type (`static public class C { … }`) the whole class is formatted (`if` braced, empty nested class → `class Empty;`). Touches SHARED-MODORDER-002.

E2. Open question (gap; P11 may decide it as the rule's own gap). Declarations whose modifiers are not checked by RH7105/RH7106: enum (`new public enum E { }` in a derived class), destructor (`unsafe extern ~C();`), local function (`async static void L() { }`), accessor (`public int P { get; internal protected set; }`). None is reported. The rule pages speak of "declaration modifiers" without excluding them. Touches SHARED-MODORDER-001.

E3. Decided by principle P22 (#912). `{|RH7105:static|}⏎#if FEATURE⏎#endif⏎    public int Value { get; set; }` and the RH7106 counterpart (`internal⏎#if FEATURE⏎#endif⏎    protected void Execute()`) are reported (no fix); pinned by `MisorderedModifiersWithDirectiveAreReportedWithoutFix` and `MisorderedCompoundAccessModifiersWithDirectiveAreReportedWithoutFix`. P22: a construct containing `#if`/`#endif` is not reported. Touches SHARED-MODORDER-003 (its example uses `#pragma`, which P22 keeps reported).

### F — `sdd/specs/analyzer/RH7107.md` (new issue)

F1. Decided by principle P9 (#912). The fix formats the whole property: a setter body `if (value != null)⏎    _d = value;` gets braces when `get` is moved in front of the setter (pinned by `BlankLineAboveReorderedPropertyIsKeptWhenAccessorBodyIsBraced`). Touches RH7107-004.

F2. Decided by principle P22 (#912), if P22 reaches the accessor list. `#if DEBUG` around the `set` accessor, `get` after `#endif` → reported (no fix); pinned by `NoCodeFixWhenDirectivesAreInAccessorLeadingTrivia` and `NoCodeFixWhenDirectiveFollowsAccessorAttributeList`. Touches RH7107-001.

### G — `sdd/specs/analyzer/RH7108.md` (new issue)

G1. Decided by principle P9 (#912). `remove { if (_e != null) _e -= value; }` before `add` → the fix also braces the `if` inside `remove`. Touches RH7108-004.

G2. Same as F2 for events (`NoCodeFixWhenDirectivesAreInAccessorLeadingTrivia`, `NoCodeFixWhenDirectiveFollowsAccessorAttributeList` in the RH7108 tests). Touches RH7108-001.

### Not filed, for the lead to judge

- RH7003: `namespace A { #if DEBUG class C { } #endif }` is reported (`VerifyDirectiveContainingNamespaceIsReported`). A literal reading of P22 would not report a namespace that contains `#if` anywhere; I do not think P22 is meant to reach a whole-file construct, so RH7003-001 says the contents do not matter and its example has no directive.

## Follow-up candidates (out of scope)

- `AGENTS.md` (Code-fix coverage) asks rule pages to record a class of inputs a fix deliberately leaves to manual correction. Not recorded today: RH7109 (initializer order, RH7109-006 / SHARED-ORDER-007 — RH7103's page has the same gap) and RH7004 (top-level statements, RH7004-005). Not changed here, because the pages are not wrong.
- RH7103-004 and RH7110-003 state the same top-level-region scope; a shared spec for it would need `RegionDirectiveUtilities` as code owner, which this run may not claim.

## Decisions needed

_None._

# Repair (skeptic worklist, 02-skeptic.md)

Commit the repair was derived from: 7143dc9. Every item was re-derived from the code (harness on the Debug build, see above) before the edit. No ID was added, removed, renumbered, or retired, so `requirement-ids.txt` and `sdd/specs/README.md` are unchanged; the `Uses` rows of RH7102, RH7109, and RH7110 gained SHARED-TYPEREGION-006 (no edit to `shared/type-regions.md`).

## Changed requirements

- RH7002-001 — narrowed: reports only declarations without `partial`; a non-partial type after a partial one is reported (new example `partial class A {}` + `class {|RH7002:B|} {}`). The second part of a partial type (C1) is unspecified.
- RH7003-001 — narrowed: a namespace that contains a namespace declaration or an `#if`/`#elif`/`#else`/`#endif` is no longer covered; the "contents do not matter" sentence and the nested-namespace example are gone (N1 and the P22 reach stay out). New example: using directive, comment, and type inside the namespace.
- RH7104-001, RH7104-003, RH7104-004 — narrowed to a type with only the reported declaration; the `Why:` of -001 no longer argues per-part reporting (D2 stays out).
- SHARED-MODORDER-001 — `Why:` no longer presents the list as complete (E2 stays out).
- SHARED-MODORDER-002 — retitled "The fix writes the modifiers in its rule's order"; scoped to declarations with no comment and no directive between the last modifier and the next token (N2, and the comment-on-the-next-line case that gets a blank line inserted, stay out). The non-goal now points neutrally to `shared/code-fixes.md`.
- SHARED-MODORDER-003 — narrowed to what `ReorderingWouldDropCommentOrDirective` checks: a comment on a modifier's line (trailing trivia), or a comment or a directive other than `#if`/`#elif`/`#else`/`#endif` in front of a modifier after the first. A directive after the last modifier (N2) and conditional directives (E3) are not covered.
- RH7101-001, RH7105-001, RH7106-001, RH7107-001, RH7108-001 — narrowed: a conditional directive inside the judged construct (declarator list, modifier list, accessor list) is no longer covered by the "is reported" sentence (B2, E3, F2, G2). These were not in the worklist's item 6 by name, but they wrote the same P22 cases into the spec as the "still reported" sentences did.
- RH7102-001, RH7109-001 — narrowed: the reported declaration contains no conditional directive, and the two fields are compared only when both lie outside every region or both in the same top-level region.
- RH7101-005, RH7102-006, RH7107-005, RH7108-005, RH7109-006 — "The diagnostic is still reported" now holds only when no `#if`/`#elif`/`#else`/`#endif` takes part in the refusal (RH7101-005: when the declaration contains none). Examples unchanged.
- RH7102-003, RH7109-003 — narrowed rather than removed: retitled "Fields in one top-level region are compared"; they now state only the case both readings of N3 share — two fields in the same top-level region (SHARED-TYPEREGION-006), a nested region included — reported, no fix (SHARED-ORDER-004). The cross-region case (N3) is unspecified. New examples verified: RH7102 with the constant in a nested region, RH7109 with three fields in one region; both reported, no fix offered.
- RH7110-001 — retitled; the order is named "the accessibility order" without a breadth claim; example prose adjusted.
- RH7110-003 — now references SHARED-TYPEREGION-006 instead of restating it; examples unchanged.
- RH7110-004 — states the operator token for an operator and the `operator` keyword for a conversion operator (from `OrderingDeclarationUtilities.GetDiagnosticLocation`). Note: operators and conversion operators are always `public` in classes, so RH7110 may never reach these two locations; the clause is there because the code places them so.
- RH7004-006 — "directly" replaced: any later statement of the same block withholds the fix; the example now has `stream.Flush();` between the declaration and the local function (verified: no fix offered).

## Not narrowed — for the coordinator

- N4 reading B (P22's "construct" is the whole declaration, body included) would also reach RH7001-001, RH7002-001, RH7104-001, RH7105-001/RH7106-001 on a type declaration, and RH7110-001: each says "every … declaration" and covers a namespace or type whose body contains an `#if`. The worklist narrowed only RH7003 for this. If N4 is filed rather than answered, the same qualifier ("contains no `#if`, `#elif`, `#else`, or `#endif` directive") would have to go into those sentences too.

## Open questions to file (additions)

The skeptic's N1, N2, N3, N4, N5 and its classifications of A1–G2 and C1 are in 02-skeptic.md; I have nothing to add or contest. C1 is now kept out of RH7002-001 either way.
