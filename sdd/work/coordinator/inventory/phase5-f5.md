# Phase 5 — family F5 (organization, RH7xxx)

26 issues. Specs read on `main` (2026-10-24). Every blocker the issues name (#1069, #1138, #1260, #1331, #1415, #899) is closed; only #1136 (SHARED-FIX-003 relocation wording) is still open, and #1332 and #1336 cite it as related. Several issues still name the lane "behavior change (change doc)", but #1260/#1331 have already written the wanted behavior into the specs. By the classification rule, those issues are now bugs (marked †).

## 1. Table

| # | Title (short) | Labels | Milestone | Kind | Spec reqs / rules / phases | Blocked by | Principle | Duplicate/overlap | Template? | Proposed | Note |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 965 | RH7207/using phase accept blank line before comment-led directive | enhancement, analyzer, formatter | vNext | behavior change | SHARED-USING-008, FMT-USING-002/-008, FMT-BLANK-011; RH7207; UsingDirectiveOrderingPhase, BlankLinePhase | — | — | dup #917 (closed); group U | yes (feature) | vNext | Spec matches code today; FMT-BLANK-011 leaves case undecided. RH7207 fix vs formatter loop (code reading) |
| 966 | `#` in block comment blocks reorder; `global::` sorts by qualifier | bug, analyzer, formatter | Triage | bug | SHARED-USING-009 (case 1), SHARED-USING-007 (case 2); RH7201–RH7207; UsingDirectiveOrderingSafety | — | — | group U | yes (bug) | bug-pool | Case 1 violates SHARED-USING-009 (only real directives block); case 2 see Doubts |
| 968 | RH7103 compares written, not effective accessibility | bug, analyzer | Triage | behavior change | RH7103-003 (says "written accessibility"); +RH7102, RH7109, RH7110 per comment | — | — | group M (#1339) | yes (bug) | vNext | RH7103-003 prescribes today's behavior; new diagnostics; spec change needed |
| 969 | Member-ordering fixes reformat whole type | bug, analyzer | Triage | bug | SHARED-FIX-006 (Formatting), SHARED-FIX-007, FMT-PIPE-010/-011, RH7103-006; RH7102/7103/7109 | — | P9, P26 | same cause as #1335 | yes (bug) | bug-pool | Violates SHARED-FIX-006: fix must not format the containing type |
| 970 | Ordering fixes refuse move across body with balanced `#pragma` | bug, analyzer | Triage | behavior change | SHARED-ORDER-006 (forbids "a directive of any other kind"); RH7102/03/07/08/09 | — | guard scope | group M | yes (bug) | vNext | Today's refusal is what SHARED-ORDER-006 states; guard narrowing = new requirement |
| 1105 | Using rules compare names by value | — | SDD | bug | SHARED-DIAG-002, SHARED-USING-007; RH7201/03/04/06/07; UsingGrouping | — (#1069 closed) | P3 | group U | no | bug-pool | Violates SHARED-DIAG-002 (`System . IO` ≠ `System.IO` today) |
| 1134 | RH7109 reorders layout-defining struct fields | bug, analyzer | Triage | bug | SHARED-FIX-003 (run time: layout); ordering-move-safety | — | P1 | pairs with #1135 | yes (bug) | 1.2.0 | CRITICAL: fix changes struct field offsets; violates SHARED-FIX-003 |
| 1135 | RH7309 swap changes static initializer order | bug, analyzer | 1.2.0 | bug | SHARED-FIX-003, SHARED-ORDER-007 (names only RH7103/7109), RH7309-004 | — | P1 | #1134 (layout part), #1262 (RH7309 fix) | yes (bug) | 1.2.0 | CRITICAL: `Y` becomes 6 instead of 1; violates SHARED-FIX-003 |
| 1151 | RH7302 fix renames `#endregion` only if exactly the name | — | SDD | bug | RH7302-005, SHARED-FIX-003 | — | — | group R (#1354); #1083 related | partly | bug-pool | Violates RH7302-005 ("exactly that description") |
| 1262 | Region fixes withheld when breaking/losing docs, and converge (RH7303/04/06/09) | — | SDD | bug | SHARED-FIX-003, -005, -001, SHARED-TYPEREGION-006, RH7303-004, RH7304-004, RH7306-003, RH7309-004 | — (#1260, #1415 closed) | P1, P7, P17, P39 | #1265 (fix side), #1263, #1135 | no | 1.2.0 | CRITICAL: RH7304/RH7309 fixes produce code that does not parse; RH7309-004 conflicts |
| 1263 | Region-removing fixes clean left blank line (RH7303, RH7310) | — | SDD | bug | SHARED-FIX-007 (removed directive line), RH7303-004, RH7310-004 | — (#1138 closed) | P14c | #1262; #954 related | no | bug-pool | Violates SHARED-FIX-007 removed-directive bullet |
| 1264 | Innermost declaration/region owns member (RH7308/09/11, RH74xx) | — | SDD | bug † | SHARED-TYPEREGION-006 (para 2), RH7308-001, RH7309-001/-002, RH7311-001; RH7401–RH7412 per comments | — (#1260 closed) | P17 | group R | no | bug-pool | Issue says change doc; SHARED-TYPEREGION-006 already states P17 |
| 1265 | RH7303 reports regions in every executable place | — | SDD | behavior change | RH7303-001, SHARED-TYPEREGION-005 (lists no expression body, initializer, attribute args, top-level statements) | #1262 for the fix side | P11 (own gap) | #1262 | no | vNext | Rule widened; SHARED-TYPEREGION-005 must grow |
| 1266 | Using rules compare with earlier directive of same group (RH7203/04/06) | — | SDD | bug | SHARED-USING-007 (para 2); RH7203-001 still says "directly follows" | — (#1260 closed) | P18 | group U | no | bug-pool | Violates SHARED-USING-007; rule specs RH7203/04/06-001 lag |
| 1267 | Alias to non-name forms own last group | — | SDD | bug | SHARED-USING-004, -005, RH7204, RH7207-003; UsingGrouping | — (#1260 closed) | — | group U | no | bug-pool | Violates SHARED-USING-004 (`using B = int;` must be last group) |
| 1332 | RH7501 fix withheld when it breaks code / moves directives | — | SDD | bug | RH7501-004, SHARED-FIX-003, SHARED-FIX-006/-007 | — (#1138 closed); cites open #1136 | P1, P9, P14a | — | no | 1.2.0 | CRITICAL: DEBUG build fails (CS0103), duplicate call, non-parsing output. Q10 = widening |
| 1333 | RH7403/07/11 check every declarator of field-like event | — | SDD | bug | SHARED-DIAG-006; SHARED-REGGROUP-005, RH7403/07/11-001 (still "one declarator") | — (#1260 closed) | P20 | group R | no | bug-pool | Violates SHARED-DIAG-006; RH74xx specs still restrict to one declarator |
| 1334 | RH7401–RH7412 verdict independent of base order / parts | — | SDD | bug † | SHARED-DIAG-009, SHARED-REGGROUP-003/-004/-006, RH7401..RH7412-001 | — (#1331 closed) | P25 | #1287 (partial double report); group R | no | bug-pool | Violates SHARED-DIAG-009; SHARED-REGGROUP-006 still says "analyzed type declaration" |
| 1335 | Organization fixes change only their node (RH7004, RH7101, RH7104–RH7108) | — | SDD | bug | SHARED-FIX-006, SHARED-FIX-007 (moved code), RH7004-004, RH7101-004, RH7104-004, SHARED-MODORDER-002, RH7107-004, RH7108-004 | — (#1138, #1331 closed) | P9, P14a, P14d, P26 | same cause as #969 | no | bug-pool | Violates SHARED-FIX-006/-007 |
| 1336 | Organization fixes withheld when breaking/relocating/no-op (RH7004, RH7101, RH7105/06) | — | SDD | bug | SHARED-FIX-003, SHARED-FIX-005, SHARED-MODORDER-003, RH7004-004, RH7101-004 | — (#1136 related) | P1, P7 | #1335; formatter side #1384 | no | 1.2.0 | CRITICAL: RH7105 fix writes `public static #pragma …` (no parse); RH7004 CS1513 |
| 1337 | Declaration rules skip lists with conditional directive (RH7101, RH7105–RH7108) | — | SDD | bug † | SHARED-DIAG-008, RH7101-001, RH7107-001, RH7108-001, SHARED-MODORDER-003 | — (#1260, #1331 closed) | P22 | group D; sibling #1275 (F4) | no | bug-pool | Violates SHARED-DIAG-008 / RH7101-001; issue still says change doc |
| 1338 | RH7105/06 check enum, destructor, local-function, accessor modifiers | — | SDD | behavior change | SHARED-MODORDER-001 (does not list these kinds) | — | P11 (own gap) | group D | no | vNext | New diagnostics; SHARED-MODORDER-001 widened |
| 1339 | RH7102/RH7109 check each top-level region on its own | — | SDD | bug † | RH7102-001/-003, RH7109-001/-003, SHARED-ORDER-004 | — (#1331 closed) | P27 | group M | no | bug-pool | Violates RH7102-003/RH7109-003, which already state region scope |
| 1340 | RH7003 skips namespace containing namespace (behavior change) | — | SDD | bug † | RH7003-001 ("contains neither a namespace declaration"), RH7003-002 | — | — | — | no | 1.2.0 | CRITICAL: fix gives CS8955. Title says behavior change; spec already updated |
| 1354 | RH7302 skips prescribed names; formatter keeps region casing | — | SDD | behavior change | FMT-REGION-001/-002, RH7302-001/-002/-004, SHARED-REGGROUP-002; RegionFormattingPhase | — | P11 | group R (#1151, #1334); #964, #1239 (F1) | no | vNext | Retires FMT-REGION-001 casing; analyzer + formatter |
| 1412 | RH7105/06 miss declaring part of partial constructor | bug, analyzer | SDD | bug | RH7105-001, RH7106-001, SHARED-MODORDER-001, SHARED-DIAG-004 (para 2) | — | P28 | group D | yes (bug) | bug-pool | Violates SHARED-DIAG-004 para 2; source audit, cause unknown, reproduce first |

## 2. Wrong bug labels

- #968: RH7103-003 says "the same written accessibility". Grouping by effective accessibility changes the spec and adds diagnostics, so this is a behavior change.
- #970: SHARED-ORDER-006 lets a move through only when the spans hold no directive of another kind. Narrowing the guard to accept balanced `#pragma`/`#nullable` adds a requirement, so this is a behavior change.
- (#966 case 2 is borderline; see Doubts.)

## 3. Critical candidates

- #1134: the RH7109 fix reorders fields of a sequential-layout struct, which changes field offsets and breaks interop or memory-reading code.
- #1135: the RH7309 region swap reorders static initializers, so a field's run-time value changes (`Y` = 6 instead of 1).
- #1262: the RH7304 fix writes `/* c */ #region Fields`, which does not parse. The RH7309 fix splits a field initializer or a class from its nested enum, which does not parse. RH7309 also drops `///` documentation.
- #1332: the RH7501 fix breaks the DEBUG build (CS0103), runs `A()` twice, extends a `#pragma` suppression to the end of the file, and produces output that does not parse.
- #1336: the RH7105/06 fix writes `public static #pragma warning disable …`, which does not parse. The RH7004 fix gives CS1513 without DEBUG, and the RH7004 fix relocates a comment.
- #1340: the RH7003 fix on a namespace that contains a namespace gives CS8955.
- Borderline: #965. The RH7207 fix and the full formatter undo each other's blank line (found by code reading, not reproduced). Each tool on its own is still idempotent.

## 4. Real bugs

- #966 (SHARED-USING-009): `/*⏎# not a directive⏎*/ using System.Linq; using System;` → today it is not reordered or reported → the spec requires `using System; using System.Linq;`, reported.
- #969 (SHARED-FIX-006): RH7103 fix on `Helpers` → today it also turns empty classes into `class Inner;` and blocks the `operator +` body → the spec requires only `Helpers` moved and the rest unchanged.
- #1105 (SHARED-DIAG-002): `using Microsoft.Win32; using System . IO;` → today RH7201 does not report it → it must be reported the same as `System.IO`.
- #1134 (SHARED-FIX-003): `struct S { public int a; public readonly int b; }` → today the fix swaps the fields → the spec requires the fix withheld and the diagnostic kept.
- #1135 (SHARED-FIX-003): regions `IComparable`(`Y = X + 1`) then `Fields`(`X = 5`) → today the fix swaps them → the spec requires the fix withheld.
- #1151 (RH7302-005): `#region a … #endregion // a cat` → today `#endregion // A cAt` → the spec requires `#endregion // a cat`.
- #1262 (SHARED-FIX-003/-005): `#region Fields⏎/* c */ int _a;` → today `/* c */ #region Fields` → the spec requires the fix withheld. RH7306: today the fix changes nothing → the spec requires it withheld.
- #1263 (SHARED-FIX-007): `int _a;⏎⏎#region Methods⏎⏎#endregion⏎}` → today `int _a;⏎⏎}` → the spec requires no blank line before `}`.
- #1264 (SHARED-TYPEREGION-006): property `P` in nested `#region Properties` inside `#region Fields` → today reported against `Fields` → the spec requires it judged by `Properties`.
- #1266 (SHARED-USING-007): `using Microsoft.B; using Alpha.X; using Microsoft.A;` → today not reported → the spec requires `Microsoft.A` reported.
- #1267 (SHARED-USING-004): `using A = System.Text;⏎⏎using B = int;` → today RH7207 reports it and its fix puts `B` first → the spec requires `B` as the last group, not reported.
- #1332 (SHARED-FIX-003): `{ var y = 1;⏎#if DEBUG⏎A(y);⏎#endif⏎break; }` → today `#if` and `break` move behind `}` (CS0103) → the spec requires the fix withheld.
- #1333 (SHARED-DIAG-006): `public event EventHandler F, G;` in an override region → today one diagnostic at `F` → the spec requires one each at `F` and `G`.
- #1334 (SHARED-DIAG-009): `class C : IA, IB { #region IB … M … }` → today RH7409 reports `M`, and with `: IB, IA` nothing → the spec requires nothing reported in either order.
- #1335 (SHARED-FIX-006/-007): `override public string ToString() => "";` → today the fix also gives a block body → the spec requires only the modifiers swapped.
- #1336 (SHARED-FIX-003): `static public⏎#pragma warning disable CS0169⏎int _a;` → today output that does not parse → the spec requires the fix withheld. `namespace N { int a, b; }` → today a no-op fix (SHARED-FIX-005) → the spec requires no fix.
- #1337 (SHARED-DIAG-008): `private int _a,⏎#if DEBUG⏎_debug,⏎#endif⏎_b;` → today reported without a fix → the spec requires it not reported.
- #1339 (RH7102-003): `#region Fields _count #endregion #region Constants const Max #endregion` → today `Max` is reported → the spec requires it not reported.
- #1340 (RH7003-001): `namespace A.B { namespace Inner { … } }` → today reported, and the fix gives CS8955 → the spec requires it not reported.
- #1412 (SHARED-DIAG-004): `internal protected partial C();` → today not reported → the spec requires it reported, like the implementing part.

## 5. Groups

- **U — using ordering** (analyzer + formatter, `UsingGrouping`/`UsingDirectiveOrderingSafety`): #966, #1105, #1266, #1267 are bug lane, one run. #965 is a behavior change in the same code area; run it after the bugs.
- **M — member-ordering rules RH7102/7103/7109/7110**: #1339 (bug) alone. #968 and #970 (behavior changes, `OrderingDeclarationUtilities`, ordering-move-safety) together in vNext. #1134 shares the RH7109 move guard and could ride with #1135.
- **F — fix-scope cleanup (SHARED-FIX-006/-007)**: #969 + #1335 have the same cause (whole-node formatting in fixes). One run, possibly large.
- **W — fix withheld when it breaks code (SHARED-FIX-003)**: #1134, #1135, #1262, #1332, #1336, #1340 are all critical. Each sits in a different code area: split by rule family (regions: #1135 + #1262; declarations: #1336; RH7501: #1332; RH7003: #1340; RH7109: #1134).
- **R — regions RH73xx / RH74xx**: #1262 + #1263 (region fixes), #1264 + #1333 + #1334 (RH74xx/region ownership verdicts, bug lane), #1151 (RH7302 fix). #1265 and #1354 are behavior changes in vNext. #1354 touches the formatter (RegionFormattingPhase) and overlaps F1's #964/#1239/#1083.
- **D — declaration/modifier rules RH7101/RH7105–RH7108**: #1337 + #1412 (bugs), with #1336 (critical). #1338 is a behavior change (vNext).
- Duplicates: none open. #965 already absorbed #917; #1151 absorbed #1126. #969 and #1335 are the same defect in different base classes.

## 6. Doubts

- †-issues (#1264, #1334, #1337, #1339, #1340) say "change doc required", but #1260/#1331 have already put the behavior into the specs, and `sdd/changes/` holds only `introduce-sdd.md`. Decide whether they run as bug lane (my pick) or still need a change doc.
- #966 case 2: SHARED-USING-007's "imported namespace name" does not say whether `global::` is stripped from the sort key. Decide whether this is a bug or a clarification plus a behavior change.
- #1266: RH7203-001/RH7204-001/RH7206-001 still say "directly follows" while SHARED-USING-007 says "every earlier directive". The rule specs contradict the shared spec.
- #1333: SHARED-REGGROUP-005 and RH7403/07/11-001 still restrict to "one declarator", which contradicts SHARED-DIAG-006. Decide whether this needs a spec fix first, or is a behavior change.
- #1334: SHARED-REGGROUP-006 still says "the analyzed type declaration declares", which contradicts SHARED-DIAG-009. The Q6 verdict (both partial parts in a matching region) goes beyond DIAG-009, so part of the issue may be a behavior change.
- #1262: RH7309-004 still requires the reorder for the `///` shapes that SHARED-FIX-003 now forbids. The spec has to be aligned in the run.
- #1332: the Q10 comment adds a widening of RH7501-004 (drop the blank line before `break`). That part is a behavior change inside a bug issue; it needs a change doc or a split.
- #1135 is in 1.2.0, but its twin #1134 is in Triage. Put both critical layout/order guards in 1.2.0 together?
- #968's comment extends it to RH7102, RH7109 and RH7110. Those specs leave members without a modifier unspecified; scope the change doc to all four.
- #1412 comes from a source audit and its cause is unknown. Reproduce it before scheduling.
