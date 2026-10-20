Role: skeptic
Commit: efa7048
Verdict: proceed narrower

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

### Method

- Debug build of efa7048; every factual question below was run through `scripts/apply-fix.sh <RH####> <fixture-dir>` (LF and CRLF gave the same status for every fixture). Analyzer-only rules (RH7001–RH7003, RH7110) were settled from the analyzer source, which has no branch the claim depends on. One compiler fact (CS8955) was settled by compiling a two-line library. Fixtures lived in the scratchpad and are deleted.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RuleDocumentation"`: 26 passed.
- Ledger: 62 IDs added (RH7001–RH7110 and SHARED-MODORDER-001..003), none removed. No reserved ID minted, no SHARED-ORDER ID, no edit to SHARED-TYPEREGION. Code owners: the shared spec claims only `ModifierOrderingUtilities` and `ModifierOrderingCodeFixProviderBase`. The concurrent branches (#995 `shared/member-region-grouping.md`, prefix SHARED-REGGROUP; the RH83xx batch) share no prefix or code owner with this diff.
- `documentation/rules/RH7110.md`: the corrected cross-reference is right. RH2004 reports members without an accessibility modifier, and RH7104 covers only partial types. A partial nested type without a modifier belongs to RH7104, so mentioning both pages would be exact, but the new sentence is not wrong.

### The spec author's candidates

| Case | Reproduced | Evidence | Classification | Already tracked |
|---|---|---|---|---|
| A1 RH7004 `#if` around the following statement | yes | output `using (…)⏎{⏎#if DEBUG⏎    s.Flush();⏎}⏎#endif`; the same text without `DEBUG` is `parse-error` | Decided by P1: a compile error in the configuration without `DEBUG` (SHARED-FIX-003 condition 1; condition 4's `Why:` already counts other configurations). The `#if`/`#endif` pair now also straddles a block boundary (#1136 scope) | no |
| A2 RH7004 trailing comment | yes | `using var s = …; // resource` → `} // resource` | Current output is wrong under AGENTS.md (*Code-fix coverage*, relocation). #1136 writes that policy into SHARED-FIX-003. File it as an instance under #1136/P1. Whether the comment belongs on the `using (…)` line or the fix is withheld is the remedy, not the question | no |
| A3 RH7004 moved statements reformatted | yes | `s.Flush( );` → `s.Flush();`; `if (true) s.Flush();` gets braces; a blank line is inserted between every moved statement | Decided by P9 / P14d (#1138) | no (#969 covers only the member-ordering base) |
| A4 RH7004 blank line before the new `using` | yes | `var x = 0;⏎⏎using (…)` | Decided by P14a (#1138). Pinned by `VerifyCodeFixTransformsUsingDeclarationWithMultipleFollowingStatements` (characterization) | no |
| B1 RH7101 splits every combined field of the type | yes | one application splits `_a, _b`, `_c, _d`, and the nested `N._e, _f` | Decided by P9 | no |
| B2 RH7101 `#if` between declarators | yes | `no-fix-offered` (reported) | Decided by P22. The directive lies inside the declarator list the rule counts, and another configuration has another list | no (SHARED-DIAG-008 not yet written, #1260) |
| C1 RH7002 second part of a partial type | yes, from the code | `DescendantNodes(…).OfType<BaseTypeDeclarationSyntax>()` counts each part and never consults the symbol | Decided by P15 (one declaration, reported once). This does not conflict with the rule's subject: the rule page speaks of one top-level *type* per file | no (#1287 lists RH2002, RH2004, RH2101) |
| D1 RH7104 formats the whole type | yes | `void M( )` → `void M()`, `if` braced, `class Empty { }` → `class Empty;` | Decided by P9 / P14d | no |
| D2 RH7104 multi-part types | yes | `public partial class Sample {}` + `partial class Sample {}` → second part reported, fixed to `public partial class Sample;`. Two parts without a modifier → two diagnostics. A nested `protected internal` part is copied likewise | Question (see below). P15's wording and the rule's subject (every part states its accessibility, which the rule page argues) give different outputs | no |
| E1 RH7105/RH7106 format the whole declaration | yes | `override public string ToString() => "";` → block body; `internal protected int M( ) => 1;` → `protected internal int M()` with a block body; `static public class C` → `if` braced, `class Empty;` | Decided by P9 / P14d | no |
| E2 declarations not checked | yes | `new public enum E { }`, `async static Task L()`, `public int P { get; internal protected set; }` → `no-diagnostic` | Question (P11 instances were decided one by one) | no |
| E3 `#if` between modifiers | yes | RH7105 and RH7106 → `no-fix-offered` (reported) | Decided by P22: the directive lies inside the modifier list the rule orders | no |
| F1 RH7107 formats the whole property | yes | setter `if` gets braces. Also `public int A { set { } get { return 0; } }` is expanded to a multi-line property | Decided by P9 / P14d | no |
| F2 RH7107 `#if` around `set` | yes | `no-fix-offered` (reported) | Decided by P22: without `DEBUG` the list has no `set` and nothing is misordered. The P22 rationale fits exactly, and the directive lies inside the accessor list the rule orders | no |
| G1 RH7108 | yes | `if` inside `remove` gets braces | Decided by P9 | no |
| G2 RH7108 | yes | `no-fix-offered` (reported) | Decided by P22, as F2 | no |
| RH7003 namespace containing `#if` | yes (`VerifyDirectiveContainingNamespaceIsReported`) | reported | Question about how far P22 reaches (see below). The P22 rationale does not fit, but its wording does | no |

### Known issues, checked (one-line comments for the lead)

- #968: the cause also reaches RH7102 (`int _a; private const int B = 1;` → `no-diagnostic`; `int _a; const int B = 1;` → reported), RH7109 in classes (the same pair with `readonly` → `no-diagnostic` / reported), and RH7110 (skips every member without a modifier). It does not reach RH7104, whose subject is the written modifier. `00-lead.md` lists "#968 (RH7110, RH7104)"; the instances are RH7102, RH7109, RH7110.
- #969: reproduced for RH7102 (`public class Inner { }` becomes `public class Inner;`). The issue already names RH7102 and RH7109.
- #970: reproduced for RH7107 (a balanced `#pragma` pair in the crossed `set` body → `no-fix-offered`). The issue already names RH7107.
- #1134: reproduced (`struct S { public int A; public readonly int B; }` → fix moves `B`).
- #1143: the audit list already contains RH7004, RH7102, RH7109.
- #1150: covers RH7004, RH7104–RH7108 and the RH7102/RH7109 base. RH7101 does not delegate; its no-op fix has another cause (N5).

### Requirements that write an open or decided-against case into the spec (narrow before tests)

1. **RH7002-001** reports "every … declaration … that follows another such declaration", which covers the second part of a partial type (C1, decided against by P15). Restrict the reported declaration to declarations without `partial`, leaving the reporting of partial parts unspecified. A non-partial type is reported under both readings even when it follows a partial part, for example `partial class A {} class B {}` → `B`.
2. **RH7003-001** says "What the namespace contains — types, nested namespaces, using directives, comments — does not matter", and its example nests `namespace Inner`. That covers N1 (nested namespace, below) and the `#if` case (P22 reach). Drop nested namespaces and directives from the sentence and the example.
3. **RH7104-001, RH7104-003, RH7104-004** cover multi-part types (D2). The qualifier on -004 ("when no part … carries an accessibility modifier") does not keep D2 out: under P15 as #1287 words it, the fix on one part of a multi-part type is withheld. Restrict all three to a partial type that has no other declaration, or ask D2 first. The `Why:` of -001 ("read one at a time, in different files") argues reading A and needs the same care.
4. **SHARED-MODORDER-002** is titled "The fix rewrites only the modifier keywords", which is false today (E1). Its non-goal states that "the fix lays the declaration out with the formatter", which is the P9 bug written as a non-goal. Retitle the requirement, and point the non-goal neutrally to fix layout (`shared/code-fixes.md`, FMT-PIPE-010) without saying what is formatted.
5. **SHARED-MODORDER-003** is untrue of the code (N2): a directive that follows the last modifier, before the next token, does not withhold the fix. The code checks only comments in a modifier's trailing trivia and comments or directives in the leading trivia of a modifier after the first. The sentence "The diagnostic is still reported" also covers E3 (`#if`, decided by P22). Narrow it to what the guard checks, and keep conditional directives out of the "still reported" sentence.
6. **RH7101-005** says the fix is withheld, and the diagnostic still reported, for every declaration FMT-STRUCT-023 refuses. That covers B2 (`#if` between declarators, P22). Keep conditional directives out of the "still reported" sentence.
7. **RH7107-005, RH7108-005** cover F2/G2 through SHARED-ORDER-003. **RH7102-006, RH7109-006** cover a conditional directive inside the moved declaration through the same branch: SHARED-ORDER-003 names a directive after an attribute list. Both cases are P22. Apply the same narrowing to "The diagnostic is still reported". All examples already use `#pragma` or `#region`, so only the normative sentences change.
8. **RH7110-003** restates SHARED-TYPEREGION-006. RH7110, like RH7103 and the region rules RH7305, RH7308, RH7309, RH7311 that list SHARED-TYPEREGION-006 under `Uses`, calls `RegionDirectiveUtilities.GetTopLevelRegions`. That requirement defines the top-level regions and says a member in a nested region counts for the region around it. The spec author's reason for restating ("RegionDirectiveUtilities may not be claimed") does not apply: referencing is not claiming and not editing. Keep only RH7110's own decision, compare within one top-level region (SHARED-TYPEREGION-006) or outside every region, and add the ID to `Uses`. Restating makes this a second copy that P17's pending edit to SHARED-TYPEREGION-006 would leave stale (FC-015).

### New findings

- **N1 RH7003 reports a namespace that cannot become file-scoped.** `namespace A.B { namespace Inner { class C { } } }` is reported on `A.B`. `namespace A.B;` followed by `namespace Inner { … }` fails with CS8955 (compiled: "Source file can not contain both file-scoped and normal namespace declarations"). With two nested namespaces (`namespace A { namespace B { } namespace C { } }`) no file-scoped form exists at all. The rule page says the rule "only reports a namespace that can actually be converted", and RH7003-001's own `Why:` ("exactly when nothing else is declared beside it … without changing what the file declares") fails for this input. This is an open question for `analyzer/RH7003.md`; none of the existing RH7003 tests pins it.
- **N2 The modifier-ordering fix produces code that does not parse.** Mechanism below. Reproduced as `invalid-result`:
  - RH7105 on a field: `static public⏎#pragma warning disable CS0169⏎    int _a;` → `public static #pragma warning disable CS0169`;
  - RH7106 on a field: `internal protected⏎#pragma …⏎    int _a;` → the same shape;
  - RH7105 on a class: `static public⏎#pragma …⏎class C` → `public static #pragma … class C;`;
  - RH7105 on a field with `#if` in that position.

  With `#region` in that position the formatter re-breaks the line (the result parses, with an inserted blank line). With a comment in that position the fix is offered and moves the comment below an inserted blank line. Decided by P1 (SHARED-FIX-003, new compile error), with E3/P22 for the `#if` variant. It is untracked and goes to the `shared/modifier-ordering.md` issue.
- **N3 RH7102-003 and RH7109-003 compare across regions, while RH7103-004 and RH7110-003 do not.** RH7103-004's `Why:` reads: "A type organized as `#region Lifecycle` and `#region Factories` has chosen its order by region; comparing across them would ask … to leave its region." It applies word for word to a `#region Fields` / `#region Constants` pair. The RH7110 rule page states the combined order of all four rules as one canonical order, but two of the four are region-scoped. Today the cross-region case is a permanent diagnostic without a fix (SHARED-ORDER-004). RH7102-003's `Why:` claims the regions "are in the wrong order too", but no rule reports that order: RH7309 orders only by category, and both regions are custom. This is a question (see below); if it is filed, RH7102-003 and RH7109-003 stay out of the spec until it is decided.
- **N4 How far P22 reaches.** See the questions below. The clear instances (B2, E3, F2, G2, and N2's `#if` variant) are decided under either reading.
- **N5 RH7101 offers a no-op fix on a field outside a type.** `namespace N { int a, b; }` parses (CS0116 is not a syntax error), is reported, and the fix returns the document unchanged (`no-progress`). The cause: registration does not check the parent; `ApplyCodeFixAsync` returns early when the parent is not a `TypeDeclarationSyntax`. Decided by P7 (SHARED-FIX-005). Low severity, since only non-compiling code is affected. RH7101-001's wording already excludes the case, so no spec change is needed.

### Wording (not open questions)

- RH7110-001 calls the order "from broadest to narrowest", but `protected internal` is broader than `internal`. The order is a convention (the rule page's), so name it as the order without claiming breadth.
- RH7110-004 says "located on its identifier", which is wrong for operators: `GetDiagnosticLocation` puts it on the operator token, and on the `operator` keyword for a conversion operator. State both.
- RH7004-006 says "follows the using declaration directly", but the code withholds the fix for any later statement of the same block (`using var s …; s.Flush(); Helper(); void Helper() { }` → `no-fix-offered`). "Directly" reads as "immediately next" (FC-002).
- SHARED-MODORDER-001's `Why:` ("These are the declarations whose modifier lists carry accessibility together with other keywords") presents the list as complete. An enum (`new public enum E`) and an accessor (`internal protected set`) carry the same combinations, and the `Why:` pre-empts E2.

## Mechanism and candidates

Only N2 is a bug found in passing; the run itself has no bug to fix.

Mechanism: `ModifierOrderingCodeFixProviderBase.ReorderingWouldDropCommentOrDirective` inspects each modifier's trailing trivia and the leading trivia of every modifier after the first, but never the leading trivia of the token that follows the last modifier. `NormalizeModifierTrivia` then gives every modifier a single trailing space, which deletes the end-of-line in front of a directive held in that token's leading trivia. The directive no longer starts a line.

Dispatch type: the 15 declaration kinds registered by RH7105 and RH7106 (SHARED-MODORDER-001). The token after the last modifier is:

- the type: field, property, event field, method, delegate, indexer, operator;
- the keyword: `class`, `struct`, `interface`, or `record`;
- `event` for an event, `implicit`/`explicit` for a conversion operator;
- the identifier for a constructor.

All share the gap. Field (RH7105 and RH7106) and class keyword (RH7105) were run. When the last modifier is itself a keyword such as `async`, the directive sits in front of a modifier and is caught.

## Breaking inputs

For the test implementer, on the narrowed requirements only:

- FC-002 / FC-003, boundaries on both sides:
  - RH7004-006: local function or label immediately after (no fix), two statements later (no fix), nested in a following statement (fix);
  - RH7104-004: documentation comment on the first modifier without attributes vs. on an attribute list;
  - SHARED-MODORDER-003: comment trailing the last modifier (no fix) vs. comment in front of the first modifier (fix), and a directive in front of a later modifier (no fix). Do not test or tag a directive between the last modifier and the type (N2);
  - RH7101-005: `#region`/`#pragma` between the declarators and in the leading trivia, never `#if` (B2).
- FC-024: RH7002 and RH7104 single-part types only. Tests with several parts (`PartialTypesWithPublicAndCompoundAccessibilityAreFixed`) stay untagged characterization tests.
- FC-027: RH7004 in a lambda, a local function, and a nested block; RH7104 nested in a class, struct, record, and interface; RH7110 nested regions and members outside every region.
- FC-020 / FC-021: directive and comment cases only with `#pragma`, `#region`, and comments. Every `#if` case is P22 and stays a characterization test.
- FC-030: tests whose expected output carries formatting outside the diagnostic's code (A3, A4, D1, E1, F1, G1, #969) are not tagged with a fix requirement.
- FC-017: the `NumberOfFixAllIterations = 2` overrides of RH7004, RH7102, RH7109 are #1143 and get no requirement of their own.
- RH7110-004: operator (`+`), conversion operator (`operator`), indexer (`this`), event field (first declarator).
- RH7003-003: LangVersion 9 (the `VerifyUnsupportedLanguageVersionIsNotFlagged` shape).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers RH7001–RH7110 | must stay (no production change in this lane) |
| Code fixes and Fix All | must stay. N2, N5 and the P1/P9/P14a/P15/P22 instances are follow-ups |
| `Reihitsu.Core` (`ModifierOrderingUtilities`, `OrderingDeclarationUtilities`, `AccessorOrderingUtilities`, `RegionDirectiveUtilities`) | must stay |
| Formatter | not involved |
| Specs | must change: narrowings 1–8 and the wording items |
| Rule docs | `RH7110.md` change is correct. The others must stay (the RH7003 page already argues N1's reading B; that is the question, not an edit) |
| Tests | must change: tags and focused tests per `01-spec-author.md`; characterization tests stay untagged |

## Simpler option

- Narrowing RH7002-001 to non-partial declarations and RH7104-001/-003/-004 to single-part types keeps C1 and D2 out without blocking the run on D2.
- RH7110-003 gets shorter by referencing SHARED-TYPEREGION-006.

Follow-up candidates, out of scope:

- The written-accessibility grouping is restated in RH7102-001, RH7109-001, RH7110-001 and the merged RH7103-003. It is implemented once in `OrderingDeclarationUtilities` (a SHARED-ORDER owner, so no ID may be minted here), and #968's fix will have to change all four.
- SHARED-MODORDER-001's list is duplicated in the `Initialize` of both analyzers (FC-015 risk). The two shared code owners do not implement it.

## Questions for the maintainer

1. **D2: RH7104 and multi-part types (P15).** Input `public partial class Sample {}` + `partial class Sample {}`.
   - Reading A, every part states its accessibility (current; the rule page's "reading only one part"): the second part is reported and fixed to `public`. Two parts without a modifier are reported twice, and each is fixed to `internal`.
   - Reading B, P15 as #1287 words it (judged by the symbol, reported once, a fix on one part withheld): the input is not reported. Two parts without a modifier give one diagnostic, and no fix is offered.
2. **E2: modifier-ordering rules on enums, destructors, local functions, accessors.** Input `new public enum E { }`, `async static Task L() { … }`, `public int P { get; internal protected set; }`.
   - Reading A (current): nothing is reported.
   - Reading B (P11 "own gap"): RH7105 reports `new` and `async`, and RH7106 reports `protected` of the accessor.
3. **N1: RH7003 and a namespace that contains a namespace.** Input `namespace A.B { namespace Inner { class C { } } }`.
   - Reading A (current): reported on `A.B`, although no rewrite to `namespace A.B;` compiles (CS8955) and `namespace A { namespace B { } namespace C { } }` has no file-scoped form at all.
   - Reading B (the rule page's "only reports a namespace that can actually be converted"): not reported.
4. **N3: region scope of RH7102 and RH7109.** Input `#region Fields` with `private int _count;`, then `#region Identity` with `private readonly int _id;` (and the RH7102 analogue with a constant).
   - Reading A (current, RH7102-003/RH7109-003): reported, no fix.
   - Reading B (as RH7103-004 and RH7110-003): not reported.
5. **N4: what P22's "construct" is for a declaration-level rule.** Inputs: `namespace A { #if DEBUG class C { } #endif }` (RH7003), and `static public class C { static void M() { #if DEBUG … #endif } }` (RH7105).
   - Reading A, the directive must lie in what the rule judges: modifier list, declarator list, accessor list, or comma gap. Both inputs are reported, as today.
   - Reading B, the whole declaration including its body: neither is reported. The same goes for any type or namespace whose body contains an `#if`, under RH7001, RH7002, RH7104, RH7105, RH7106, and RH7110.

   B2, E3, F2 and G2 are not reported under either reading. Asking once for #1260 (SHARED-DIAG-008) avoids one question per rule.
6. **P18's reach into the member-ordering rules (lower priority; #1266 asks to check "the other ordering rules").** RH7102, RH7109 and RH7110 already compare across members of other groups, which is P18's decided defect. They compare with every earlier member of the group, not with the previous one. RH7110-001's example states this: "compared with the narrowest accessibility that came before it".
   - Reading A (current): `B`, `D`, `E`, `O` are reported.
   - Reading B (literal "previous element of the same group"): only `B` and `O`.

   For RH7102 and RH7109 the literal reading would need a second pass after each fix, so it is unlikely to be meant there.

C1 is not listed: I take it as decided by P15, because the rule page itself speaks of one type per file. If the lead disagrees, the readings are: current, the second part reported; P15, not reported.
