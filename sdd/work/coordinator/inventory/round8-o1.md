# Inventory — round 8, family O1 (#993, PR #1303: file layout, member/modifier ordering, RH7001–RH7004, RH7101–RH7110, shared modifier ordering)

Issues #1318–#1328, read 2026-10-21 (all open, none has comments, none is a sub-issue of #912 — listed on #912 by comment). 23 items in 11 issues; #1322 N3 and #1323 N3 are one question (same cause, two rules). All are `SDD open questions` issues; none skipped.
`⏎` = line break. Duplicates were checked against the titles of all 200 open issues (list_issues, 2 pages), not by semantic search.

Principles used: P1 unsafe fix (SHARED-FIX-003), P7 no no-op (SHARED-FIX-005), P9 fix changes only the code its diagnostic is about, P10 fix layout = formatter layout, P11 own gap vs new rule, P14a no blank line inserted before the changed node, P14d only the replaced node is formatted, P15 partial declaration = one declaration, P18 ordering compares with previous same-group element, P22 conditional directive in the reported construct = not reported, P23 still report where no fix compiles. AGENTS.md "Code-fix coverage" (relocating a comment = unsafe; SHARED-FIX-003 text for it pending in #1136).

| Issue | Q | Rule | Input → real output | Why it looks wrong | Req IDs | Principle | Duplicates / related | Kind (recommendation) | Fully decided? |
|---|---|---|---|---|---|---|---|---|---|
| #1318 | A1 | RH7004 fix | `using var s = new MemoryStream();⏎#if DEBUG⏎s.Flush();⏎#endif⏎}` (DEBUG defined) → `using (var s = …)⏎{⏎#if DEBUG⏎s.Flush();⏎}⏎#endif` | `#if` moved into the block, `#endif` left outside; without DEBUG: CS1513 | RH7004-004, SHARED-FIX-003 | P1 (marked, fits: new compile error). Not P22: the reported construct is the declaration, the directive is in the following statements | #1136 (SHARED-FIX-003 scope), #970 (balanced pragma pair blocks ordering moves, opposite direction) | Bug: withhold the fix when an unbalanced directive pair would straddle the new block's brace (a balanced pair inside the moved statements can stay, cf. SHARED-ORDER-006) | Yes → new issue "organization fixes withheld where unsafe" (F-B below) |
| #1318 | A2 | RH7004 fix | `using var s = new MemoryStream(); // resource⏎s.Flush();` → comment ends up after the new block: `}  // resource` | Comment described the declaration line and is relocated to the block end | RH7004-004, SHARED-FIX-003 | Not marked. AGENTS.md Code-fix coverage: a fix that relocates a user comment must withhold itself; SHARED-FIX-003 text pending (#1136); precedent round 7b item 4 (#1185 Q1, withhold when a comment sits on the moved block) | #1136, #963 (formatter: comment after `=>` glued to new brace — same family, formatter side) | Bug. Recommend: keep the comment on the `using (…)` header line (not a relocation); withhold only where that is impossible | Yes (AGENTS.md rule; squad picks keep-on-header over withhold) → F-B |
| #1318 | A3 | RH7004 fix | `using var s = …;⏎s.Flush( );⏎if (true) s.Flush();⏎var n = 1;⏎return n;` → moved statements rewritten: `s.Flush();`, `if (true)` braced, blank line inserted between `var n = 1;` and `return n;` | The moved statements are not what the diagnostic is about; only their indentation must change | RH7004-004, FMT-PIPE-010/011 | Marked P9. **Fits only partly:** braces = shape change (P9 + FMT-PIPE-011 decide it). The spacing and the blank line are inside the node the fix creates, and P10/P14d ("format the node it replaces") literally allow formatting it. P9 vs P14d conflict for re-nested code | #969 (same pattern for moved members: "formats the moved member" expected), #1140, #1284 | Bug if moved code keeps its layout (candidate P25); clarification otherwise | No — see M6 (P25 candidate) |
| #1318 | A4 | RH7004 fix | `var x = 0;⏎using var s = new MemoryStream();⏎x++;` → blank line inserted between `var x = 0;` and `using (…)` | Fix inserts a blank line before the changed statement | RH7004-004, SHARED-FIX-006/007 (planned) | P14a (marked, fits word for word). Test `VerifyCodeFixTransformsUsingDeclarationWithMultipleFollowingStatements` changes | #1140 (P14a instances of line-break fixes), #1138 | Bug | Yes → new issue F-A |
| #1319 | B1 | RH7101 fix | `private int _a, _b;⏎⏎private int _c, _d;⏎class N { private int _e, _f; }` → fix on `_b` splits all three declarations (`FieldDeclarationSplitTransform` over the whole type) | Fix changes declarations its diagnostic is not about | RH7101-004 | P9 (marked, fits) | #969 (whole type formatted, ordering fixes), #963 (formatter field split duplicates comment) | Bug | Yes → F-A |
| #1319 | B2 | RH7101 | `private int _a,⏎#if DEBUG⏎_debug,⏎#endif⏎_b;` → reported on `_debug`, no fix | Declarator list holds a conditional directive | RH7101-001, RH7101-005, SHARED-DIAG-008 (planned) | P22 (marked, fits under both readings of N4: the declarator list is what the rule judges). Tests pinning it change | #1275 (P22 instances RH5412–RH5416), #1260, #1240 | Behavior change (fewer diagnostics; pinned tests change) | Yes → new issue F-C |
| #1319 | B3 | RH7101 fix | `namespace N { int a, b; }` (CS0116, parses) → reported, fix returns document unchanged | No-op fix | RH7101-001, SHARED-FIX-005 | P7 (marked, fits) | #1150 (P7 formatter-unchanged fixes; different cause: registration does not check the parent), #1076 | Bug, low severity (only non-compiling code): do not register the fix when the parent is not a type | Yes → F-B |
| #1320 | C1 | RH7002 | `partial class C⏎{⏎}⏎⏎partial class C⏎{⏎}` in one file → second part reported as a second top-level type | Both parts are one type; rule page speaks of one type per file | RH7002-001, SHARED-DIAG-004 (planned) | P15 (marked, fits: "a partial declaration is one declaration … reports it once") | #1287 (P15: RH2002, RH2004, RH2101) | Bug (behavior change for the pinned case) | Yes → extend #1287 with RH7002 |
| #1321 | N1 | RH7003 | `namespace A.B { namespace Inner { class C { } } }` → reported on `A.B`; the fix result `namespace A.B;` + block `namespace Inner {…}` fails CS8955; `namespace A { namespace B { } namespace C { } }` has no file-scoped form | RH7003-001 Why ("without changing what the file declares") fails; rule page: "only reports a namespace that can actually be converted" | RH7003-001, RH7003-002 | none fits cleanly. P23 ("still report, offer no fix") points to reading "report, withhold fix"; RH7003-002 (other member beside it → not reported) and the rule page point to "not reported" | none by title | Clarification/behavior change; see M2 | No — M2 |
| #1321 | N4 | P22 reach (RH7001–RH7003, RH7104–RH7106, RH7110) | `namespace A {⏎#if DEBUG⏎class C { }⏎#endif⏎}` (RH7003) and `static public class C { static void M() {⏎#if DEBUG … #endif⏎} }` (RH7105) → both reported today | P22 says a construct containing `#if` is not reported; "construct" is undefined for declaration-level rules | SHARED-DIAG-008 (planned in #1260), RH7003-001, SHARED-MODORDER-001 | P22 (wording needs a reading). The issue asks once for all rules | #1260 (SHARED-DIAG-008 wording), #1240 (inactive-`#if` non-goal) | Clarification of P22 wording in #1260 | No — M1 |
| #1322 | N3 | RH7102 | `#region Fields⏎private int _count;⏎#endregion⏎#region Constants⏎private const int Max = 1;⏎#endregion` → reported, no fix (SHARED-ORDER-004) | Permanent diagnostic without fix; RH7103-004 and RH7110-003 check each top-level region on its own with a Why that applies word for word | RH7102-001/003, SHARED-ORDER-004, RH7103-004, RH7110-003 | none (candidate P26: member-ordering rules check each top-level region on its own) | = #1323 N3 (same question); #968, #969, #970 unrelated causes | Behavior change (fewer diagnostics) if (a) | No — M3 |
| #1323 | N3 | RH7109 | `#region Fields⏎private int _count;⏎#endregion⏎#region Identity⏎private readonly int _id;⏎#endregion` → reported, no fix | Same as #1322 N3 | RH7109-001/003, SHARED-ORDER-004 | none (candidate P26) | = #1322 N3; #1134 (RH7109 sequential layout, other cause) | as #1322 | No — M3 (one decision) |
| #1324 | D1 | RH7104 fix | `partial class C {⏎void M( ) {⏎if (true) M();⏎}⏎class Empty {⏎}⏎}` → `internal partial class C` plus `void M()`, braced `if`, `class Empty;` (`FormatNodeInDocumentAsync` on the type) | Fix formats the whole type | RH7104-004 | P9 / P14d (marked, fits: format only the replaced node) | #969 (same pattern, ordering fixes), #1284 (P14d expression-body fixes) | Bug | Yes → F-A |
| #1324 | D2 | RH7104 | `public partial class Sample { }` + `partial class Sample { }` → second part reported, fix `public partial class Sample;`; two parts without modifier → reported twice, each fixed to `internal` | P15 says judge by the symbol, report once, withhold a fix on one part | RH7104-001/004, SHARED-DIAG-004 (planned) | P15 literal → reading B (not reported). **Conflicts with the rule's subject** (stating accessibility on each part; its Why: "reading only one part"). The issue recommends A | #1287 | Clarification (exception to P15) if A; behavior change if B | No — M4 |
| #1325 | E1 | RH7105/RH7106 fix | `override public string ToString() => "";` → `public override string ToString()` with a block body; same for operator, `internal protected int M( ) => 1;`; on `static public class C {…}` the whole class is formatted (`if` braced, `class Empty;`) | Fix changes body shape and other members | SHARED-MODORDER-002 | P9 / P14d (marked, fits) | #969, #1284, #967 (RH2005 fix indentation for `static public`, different rule) | Bug | Yes → F-A |
| #1325 | E2 | RH7105/RH7106 | `new public enum E { }`, `unsafe extern ~C();`, `async static void L() { }` (local function), `public int P { get; internal protected set; }` → none reported | Rule pages speak of "declaration modifiers" without exclusions; SHARED-MODORDER-001 list omits enum, destructor, local function, accessor | SHARED-MODORDER-001 | Issue asks "P11, own gap?". P11 fits: the construct the rules name is a declaration's modifier list; these are modifier lists of declarations, not a different construct | none by title | Behavior change / extension (new diagnostics) | Yes (P11 own gap) → new issue F-D; one-line confirm in the round is enough |
| #1325 | E3 | RH7105/RH7106 | `static⏎#if FEATURE⏎#endif⏎public int Value { get; set; }` → reported, no fix (tests `MisorderedModifiersWithDirectiveAreReportedWithoutFix`, `MisorderedCompoundAccessModifiersWithDirectiveAreReportedWithoutFix`) | Modifier list holds a conditional directive | SHARED-MODORDER-003, SHARED-DIAG-008 (planned) | P22 (marked, fits under both readings of N4; `#pragma` in the SHARED-MODORDER-003 example stays reported without fix, as P22 says) | #1275, #1260 | Behavior change (pinned tests change) | Yes → F-C |
| #1325 | N2 | RH7105/RH7106 fix | `static public⏎#pragma warning disable CS0169⏎int _a;` → `public static #pragma warning disable CS0169` (does not parse); same for RH7106 `internal protected`, for `class C`, and for `#if`. With `#region` there: parses, but blank line inserted; with a comment: comment moved below an inserted blank line | `ReorderingWouldDropCommentOrDirective` ignores the leading trivia of the token after the last modifier | SHARED-MODORDER-002/003, SHARED-FIX-003 | P1 (marked, fits) for the directive cases; the `#region`/comment sub-cases are P14a (inserted blank line) and AGENTS.md relocation | #1136 | Bug: extend the SHARED-MODORDER-003 guard to the trivia before the token after the last modifier | Yes → F-B |
| #1326 | F1 | RH7107 fix | `set { if (value != null)⏎_d = value; }` before `get` → setter body gets braces when `get` is moved | Fix changes code it does not move | RH7107-004 | P9 (marked, fits: the setter is not the moved node). Test `BlankLineAboveReorderedPropertyIsKeptWhenAccessorBodyIsBraced` changes | #969 | Bug | Yes → F-A |
| #1326 | F2 | RH7107 | `#if DEBUG⏎set { … }⏎#endif⏎get { … }` → reported, no fix (tests `NoCodeFixWhenDirectivesAreInAccessorLeadingTrivia`, `NoCodeFixWhenDirectiveFollowsAccessorAttributeList`) | Accessor list holds a conditional directive | RH7107-001, SHARED-DIAG-008 (planned) | P22 (marked "if P22 reaches the accessor list"). It does under both readings of N4: the accessor list is what RH7107 judges | #970 (balanced pragma blocks a move), #1275 | Behavior change (pinned tests change) | Yes → F-C |
| #1327 | G1 | RH7108 fix | `remove { if (_e != null) _e -= value; }` before `add` → `if` inside `remove` gets braces | as F1 | RH7108-004 | P9 (marked, fits) | #969 | Bug | Yes → F-A |
| #1327 | G2 | RH7108 | conditional directive around an accessor of an event (tests as F2) → reported, no fix | as F2 | RH7108-001 | P22 (marked, fits as F2) | #1275 | Behavior change | Yes → F-C |
| #1328 | – | RH7110 (+RH7102, RH7109) | `private int _a;⏎public int B;⏎internal int D;⏎internal protected int E;⏎⏎protected void M() { }⏎protected private void N() { }⏎internal protected void O() { }` → `B`, `D`, `E`, `O` reported (compared with the narrowest accessibility seen earlier in the group; test `MembersAreComparedWithTheLatestAccessibilitySeenInTheirGroup`) | Literal P18 ("previous element of the same group") reports only `B` and `O` | RH7110-001, RH7102-001, RH7109-001, SHARED-USING-007 (P18 wording planned in #1260) | P18 — **does not fit as a decider**: P18 was decided for "what lies between does not hide a pair" (#1171–#1173), not for which earlier element is the partner | #1266 ("other ordering rules are checked for the same pattern"), #968 (written vs effective accessibility), #1260 | Clarification of P18 wording | No — M5 |

## Groups by shared cause

1. **Whole node formatted / unrelated code rewritten by organization fixes (P9, P14a, P14d):** A3 (braces part), A4, B1, D1, E1, F1, G1, N2 sub-cases (`#region`/comment blank line). Same cause as #969 (ordering base class).
2. **Unsafe or no-op organization fixes (P1, P7, AGENTS.md relocation):** A1, A2, B3, N2.
3. **Conditional directive in the judged construct (P22):** B2, E3, F2, G2 — all agree under both readings of N4; N4 itself only matters for bodies of types/namespaces.
4. **Partial declarations (P15):** C1 decided, D2 open (rule subject vs P15).
5. **Region scope of member-ordering rules:** #1322 N3 = #1323 N3.
6. **Singles:** N1 (nested namespace), #1328 (P18 partner), E2 (P11 own gap).

## Decided by principle → proposed follow-up action

- **F-A, new issue (bug, analyzer, blocked by #1138): "Organization fixes change only the node they rewrite or move: RH7004, RH7101, RH7104, RH7105, RH7106, RH7107, RH7108 (P9, P14a, P14d)"** — A3 (braces; spacing/blank-line part after M6), A4, B1, D1, E1, F1, G1, N2 (`#region`/comment blank-line sub-cases). Tests to change: `VerifyCodeFixTransformsUsingDeclarationWithMultipleFollowingStatements`, `BlankLineAboveReorderedPropertyIsKeptWhenAccessorBodyIsBraced`. Related #969 (same cause for RH7102/RH7103/RH7109; alternatively extend #969 and retitle it to "Organization fixes …" — recommend a new issue, since #969 is scoped to `TypeMemberOrderingCodeFixProviderBase` and already has a concrete expected output).
- **F-B, new issue (bug, analyzer): "Organization fixes are withheld where they break code, relocate a comment, or change nothing: RH7004, RH7101, RH7105, RH7106 (P1, P7)"** — A1, A2, B3, N2. Related #1136 (relocation clause in SHARED-FIX-003; A2 cites it).
- **F-C, new issue (behavior change, analyzer, blocked by #1260): "Declaration rules do not report a declarator, modifier or accessor list that holds a conditional directive: RH7101, RH7105, RH7106, RH7107, RH7108 (P22)"** — B2, E3, F2, G2; six pinned tests change. Sibling of #1275 (RH5412–RH5416). If M1 is answered B, add RH7001–RH7003, RH7104, RH7110 to it.
- **Extend #1287 (P15)** with RH7002 (C1): the second part of a partial type in the same file is not a second type. If M4 is answered B, add RH7104 there too.
- **F-D, new issue (behavior change / extension, analyzer): "RH7105 and RH7106 check enum, destructor, local-function and accessor modifier lists (P11)"** — E2; SHARED-MODORDER-001 widened.

## Needs maintainer decision

**M1 — How far does P22's "construct" reach for declaration-level rules? (#1321 N4)**
```csharp
namespace A
{
#if DEBUG
    class C { }
#endif
}
```
Today: RH7003 reports `A` (and the fix converts). Same for `static public class C { void M() {⏎#if DEBUG⏎…⏎#endif⏎} }` under RH7105.
- (a) The construct is what the rule judges (modifier list, declarator list, accessor list, comma gap, namespace header). Both inputs stay reported. 
- (b) The whole declaration including its body; neither is reported, and any type/namespace with an `#if` anywhere inside escapes RH7001–RH7003, RH7104–RH7106, RH7110.
- Recommendation: **(a)**. (b) would silence file- and type-level rules for nearly every type that contains a `#if DEBUG` block, which P22 was never meant to do (its instances were trailing commas and empty bodies, where the directive sits in the judged span). Write it into SHARED-DIAG-008 via #1260. Note for RH7003 under (a): the fix must still keep the directives intact (P1).

**M2 — RH7003 on a namespace that contains a namespace (#1321 N1)**
```csharp
namespace A.B
{
    namespace Inner
    {
        class C { }
    }
}
```
Today: reported on `A.B`; the fix gives `namespace A.B;` followed by a block `namespace Inner { … }` → CS8955.
- (a) Not reported (rule page: "only reports a namespace that can actually be converted"; same Why as RH7003-002). Behavior change (fewer diagnostics) + the broken fix disappears.
- (b) Reported, fix withheld (P23 style). 
- (c) Reported; for a single chain the fix merges to `namespace A.B.Inner;`; siblings → not reported or no fix.
- Recommendation: **(a)**. The rule's subject is "this namespace can be file-scoped"; a file whose namespace holds namespaces is not that, and (c) is a new feature that can be filed later. P23 does not force (b): it covers constructs the rule names whose correction would not compile, while here the construct is not the one RH7003-001 names.

**M3 — Member-ordering rules across top-level regions (#1322 N3 = #1323 N3)**
```csharp
class C
{
    #region Fields
    private int _count;
    #endregion

    #region Constants
    private const int Max = 1;
    #endregion
}
```
Today: RH7102 reports `Max` with no fix (SHARED-ORDER-004 forbids crossing the region). Same for RH7109 (`private int _count;` / `#region Identity` / `private readonly int _id;`).
- (a) Each top-level region is checked on its own, as RH7103-004 and RH7110-003 already say. Not reported. Behavior change (fewer diagnostics), spec change for RH7102-003 and RH7109-003.
- (b) Keep: reported, no fix.
- Recommendation: **(a)**, and record it as principle candidate P26 so every member-ordering rule (RH7102, RH7103, RH7109, RH7110, and later ones) shares one region scope.

**M4 — RH7104 on a type with several parts (#1324 D2)**
```csharp
public partial class Sample { }
partial class Sample { }
```
Today: the second part is reported; fix → `public partial class Sample { }`. Two parts without modifier → two diagnostics, each fixed to `internal`.
- (a) Keep: every part states its accessibility (the rule's subject is the written modifier on each part). Record an exception to P15 for rules whose subject is what each part writes.
- (b) P15 literal: judged on the symbol, reported once, fix withheld; the input above is not reported.
- Recommendation: **(a)** (as the squad recommends). P15 is about verdicts on what the type *is* (static, async, accessibility for RH2004); RH7104 is about what each part *writes*. Candidate P27 states the exception so RH7105/RH7106 (modifier order is per part too) are covered the same way.

**M5 — Which earlier member is the comparison partner in RH7110, RH7102, RH7109? (#1328)**
```csharp
class C
{
    private int _a;
    public int B;
    internal int D;
}
```
Today: `B` and `D` reported (`D` is broader than the earlier `_a`).
- (a) Keep: a member is compared with the earlier members of its group (narrowest seen so far); P18 is clarified to "whatever lies between does not hide a pair", not "only the adjacent element". 
- (b) Literal P18: compared only with the previous member of its group; only `B` reported (then `D` is reported after `B` moves — second pass).
- Recommendation: **(a)**. It reports every member that has to move in one pass and matches what the fixes need (one application, SHARED-FIX-002). Clarify the P18 wording in #1260 ("compared with the earlier elements of the same group, whatever other elements lie between"), and note on #1266 that RH7110/RH7102/RH7109 already meet it.

**M6 — Layout of code a fix moves or re-nests (#1318 A3; principle candidate P25)**
```csharp
void M()
{
    using var s = new MemoryStream();
    s.Flush( );
    var n = 1;
    return;
}
```
Today: `using (var s = new MemoryStream())⏎{⏎    s.Flush();⏎⏎    var n = 1;⏎    return;⏎}` — spacing fixed, blank line inserted, and an embedded `if` would get braces.
- (a) Moved/re-nested code keeps its own layout; only its indentation follows the new nesting (and the separators at the old and new position follow SHARED-ORDER-002). Braces, spacing and blank lines inside it stay.
- (b) The created node is formatted as the formatter formats it (P10/P14d read on the new `using` block), except shape changes (braces) which P9/FMT-PIPE-011 forbid.
- Recommendation: **(a)**, as principle P25; it matches #969's expected output (the moved member is left as written) and keeps P9's intent. Then A3 joins F-A completely.

## New principle candidates

- **P25 (candidate): code a fix moves or re-nests keeps its own layout; only its indentation follows the new nesting.** Decides #1318 A3; supports #969, F1, G1; applies to every ordering/wrapping fix (RH7004, RH7102–RH7110, accessor and region moves).
- **P26 (candidate): member-ordering rules check each top-level region on its own.** Generalizes RH7103-004 / RH7110-003; decides #1322 N3 and #1323 N3.
- **P27 (candidate): exception to P15 — a rule whose subject is what each part of a partial declaration writes (accessibility modifier, modifier order) judges each part.** Decides #1324 D2 (reading A); keeps RH7105/RH7106 per part.
- **Clarifications of existing principles (not new numbers):** P22 "construct" = the span the rule judges (M1); P18 partner = earlier elements of the group, not only the adjacent one (M5).

## Verification of "Decided by principle" claims

Fit as stated: A1 (P1), A4 (P14a), B1 (P9), B2 (P22), B3 (P7), C1 (P15), D1 (P9/P14d), E1 (P9/P14d), E3 (P22), N2 (P1), F1 (P9), F2 (P22 — fits under both N4 readings, the "if" is unnecessary), G1 (P9), G2 (P22, same).
Flagged: A3 (P9) fits only for the braces; the spacing/blank-line part collides with P10/P14d (M6). D2 names P15 as reading B, but P15 literal conflicts with the rule's subject (M4). #1328's P18 is not a decider (M5).

Counts: 23 items; 16 fully decided (A1, A2, A4, B1, B2, B3, C1, D1, E1, E2, E3, N2, F1, F2, G1, G2), 7 not decided (A3 partly, N1, N4, N3 ×2 as one decision, D2, #1328) → 6 maintainer decisions M1–M6.
