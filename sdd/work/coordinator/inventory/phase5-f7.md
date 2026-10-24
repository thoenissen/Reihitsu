# Phase 5 — family F7 (naming RH4xxx, performance RH1xxx, design RH2xxx, RH0xxx)

31 issues, checked against `sdd/specs/` on main (f9779e0), 2026-10-24. Not built or tested. "covers" = the coverage sentence of a requirement (a case outside it is unspecified, so not a bug against that requirement).

## 1. Table

| # | Title (short) | Labels | Milestone | Kind | Spec reqs / rules / phases | Blocked by | P# | Duplicate/overlap | Template? | Proposed | Note |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 862 | Consolidate auto-property accessor-list predicate | enhancement | Maintenance | internal | RH5047, RH5408, `LineBreakDetection`, `AttributeTargetUtilities` | — | — | #1034 (AttributeTargetUtilities ownership, F4) | no (code-review form) | Maintenance | Refactor; byte-identical output; asks where in Core |
| 967 | RH2005 fix: `static public` indent, refs outside type | bug, analyzer | Triage | bug | SHARED-FIX-007 (indent), SHARED-FIX-003 (CS0122); RH2005-003 | — | P1 | comment adds RH2004 fix indent; F5 #1325 (`DeclarationModifierUtilities`) | yes | 1.2.0 | CRITICAL: fix breaks build (CS0122); indent vs SHARED-FIX-007 |
| 971 | Casing fix `HTTPClient` → `_hTTPClient` | bug, analyzer | Triage | behavior change | SHARED-CASING-006 (excludes it), RH4106-004, RH2001-004 | — | — | #1016, #1084 (same `CasingUtilities`) | yes | vNext | CASING-006 covers no camel target starting with two capitals |
| 973 | RH1003 counts numeric sub-additions as parts | bug, analyzer | Triage | behavior change | RH1003-001 ("flattening nested additions") | — | — | — | yes | vNext | Spec wording describes today's behavior; narrowing = removes diagnostics |
| 974 | RH0001 accepts content after root object | bug, analyzer | Triage | bug | RH0001-003 | — | — | CLI config loading (check) | yes | bug-pool | Violates RH0001-003: `{ } junk` is invalid JSON |
| 1015 | RH4102 Fix All covers one event kind only | bug, analyzer | Triage | bug | SHARED-FIX-002; RH4102 header `Fix All` | — | — | — | yes | bug-pool | Violates SHARED-FIX-002 |
| 1016 | Casing fixes produce a still-reported name | bug, analyzer | 1.2.0 | bug | SHARED-FIX-001, SHARED-CASING-008 | — | — | #1084 (RH4005 `I` prefix, `_`→`I_`), #971 | yes | bug-pool | Violates SHARED-FIX-001 (`__1st`→`_1st` still reported) |
| 1070 | Naming: skip names dictated by metadata/native | — | Triage | behavior change | new SHARED-CASING-011; RH4102/4103/4112–4114/4116; SHARED-FIX-003 | — | P2 | — | no | vNext | CRITICAL fix part (CS0115, EntryPointNotFound) is a FIX-003 bug |
| 1071 | RH4001, RH4009 compare names by value | — | Triage | bug | SHARED-DIAG-002 (RH4001-002/-005, RH4009-002 covers exclude `@`) | #1069 (done: DIAG-002 merged) | P3 | #1085 (same rules) | no | bug-pool | Violates SHARED-DIAG-002 (`class @class`, `A . B`) |
| 1072 | Casing fix: withhold when derived name is keyword | — | Triage | behavior change | SHARED-CASING-007 (covers excludes keywords), -008 | — | P3 | casing group | no | vNext | Title-labeled behavior change; `@class` compiles |
| 1073 | Casing fix: withhold unsafe renames | — | Triage | bug | SHARED-FIX-003 (compile error, `#if`, run time); SHARED-CASING-009 | #1068 (done: FIX-003 merged) | P1 | casing group; #1286 same principle | no | 1.2.0 | CRITICAL: CS0542, broken `#if` config, record semantics change |
| 1084 | Casing fix: keep existing prefix, all id chars | — | SDD | behavior change | RH4005-002, RH4121-003 (exclude `t_`), SHARED-CASING-006 (excludes Mn/Mc/Nl/Pc), -008 | — | — | #1016 (RH4005 non-convergence) | no | vNext | Spec today yields `IIDocument`; only `_`→`I_` is a bug (CASING-008) |
| 1085 | RH4001 keeps migration prefix; RH4009 nested full name | — | SDD | behavior change | RH4001-005 (excludes prefix files), RH4009-002 (file-level only) | — | — | #1071, #1086 | no | vNext | Body says bug; both cases outside requirement coverage |
| 1086 | RH4001: skip files with top-level statements | — | SDD | behavior change | RH4001-001 (covers files without TLS) | — | — | #1085 | no | vNext | Explicit behavior change |
| 1087 | RH4115/RH4117: `_`, local consts, deconstruction | — | SDD | behavior change | RH4115-001/-002, RH4117-001 | — | — | #1088 (release together) | no | vNext | One change doc for both rules |
| 1088 | New rule: casing of local constants | — | SDD | behavior change | new RH41xx; SHARED-CASING | coordinate #1087 | — | #1087 | no | vNext | New rule; ship with #1087 |
| 1089 | RH4124 query range vars; RH4122 extension receivers | — | SDD | behavior change | RH4124-002, RH4122-003 | — | — | — | no | vNext | Widening, change doc |
| 1090 | Rename RH4118TupleElementCasingAnalyzer | — | SDD | internal | RH4118 code owners, `sdd/specs/README.md` | — | — | #1032 (RH4116/RH4118 cleanup) | no | Maintenance | Rename only |
| 1032 | Remove unused RH4116Title/RH4118Title fix resources | enhancement | Maintenance | internal | `CodeFixResources` | — | — | #1090 | no (code-review form) | Maintenance | Dead resources |
| 1268 | RH2003 resolves exception type via alias | — | SDD | bug | SHARED-DIAG-002 (RH2003-001 covers excludes alias) | #1260 (done) | P3 | #1071 (DIAG-002) | partly | bug-pool | Violates SHARED-DIAG-002; pre-filter still at `RH2003…Analyzer.cs:80` |
| 1286 | RH2001 fix withheld where unsafe | — | SDD | bug | SHARED-FIX-003, SHARED-FIX-001; RH2001-004 | — | P1 | #1299 (RH2001 scope), #971 (RH2001 name) | partly | 1.2.0 | CRITICAL: CS1004, drops comments, struct mutation changes |
| 1287 | Partial declaration is one declaration (P15) | — | SDD | bug | SHARED-DIAG-004; RH2002-001/-005, RH2004-001/-002/-006, RH2101-002, RH7002-001, RH74xx, RH8306 | #1260 (done) | P15 | #1300; F5 RH74xx/RH7002, F6 RH8306; #1412 | partly | 1.2.0 | CRITICAL: RH2004 fix on one part gives CS8799; RH2004-002 text changes |
| 1288 | RH2004 fix on explicit interface operators | — | SDD | bug | SHARED-DIAG-011 (report), SHARED-FIX-003 (fix); RH2004-001/-006 | — | P1 | #967 (`DeclarationModifierUtilities`) | partly | 1.2.0 | CRITICAL: fix gives CS0106/CS0558; `public` for operators is extension |
| 1289 | RH2109 reads Razor comments/strings/escapes | — | SDD | bug | SHARED-DIAG-003 (Razor bullet + RH2109 example), RH2109-001 | #1260 (done) | P5 | — | partly | bug-pool | Violates SHARED-DIAG-003; comment adds `@functions` = behavior change |
| 1290 | RH1001/RH1002 through parens, `!`, `throw` (P16) | — | SDD | behavior change | SHARED-EQUAL-002 (leaves forms unspecified), SHARED-DIAG-005 | #1260 (done) | P16 | #1295, #1296, #1297, #1298 | partly | vNext | Maintainer: extension; comment adds `default(T)` |
| 1295 | RH1001/RH1002 Nullable, tuples, collection exprs | — | SDD | behavior change | RH1001-002 (excludes collection exprs), SHARED-EQUAL-001/-002 | #1260 (done) | — | #1290, #1296, #1297 | partly | vNext | Title-labeled behavior change |
| 1296 | RH1002 more methods, query clauses | — | SDD | behavior change | RH1002-001 (excludes query clauses), SHARED-DIAG-007 | #1260 (done) | P21 | #1290, #1295 | partly | vNext | Title-labeled behavior change |
| 1297 | RH1002 comparer type argument vs key type | — | SDD | behavior change | SHARED-EQUAL-002 (covers only `T` = compared type) | — | — | #1290 | partly | vNext | Titled "(bug)" but case is outside EQUAL-002 coverage |
| 1298 | RH2006/RH2007 `string.Empty`, all-empty conditionals | — | SDD | behavior change | SHARED-DEBUG-001 (covers excludes both) | #1260 (done) | P16 | #1290 | partly | vNext | `const` instance already required by DEBUG-001 |
| 1299 | RH2001 only replaceable `field` props; attributed exempt | — | SDD | behavior change | RH2001-001/-002 (field-keyword and `field:`/accessor attributes unspecified) | — | — | #1286 | partly | vNext | Says "bug" for Q5; spec leaves those attributes unspecified |
| 1300 | RH2002 explicit interface async void | — | SDD | behavior change | RH2002-001 (covers excludes explicit impl) | — | P23 | #1287 | partly | vNext | Mentions a fix; RH2002 has no code fix (non-goal) |

## 2. Wrong bug labels

- #971 — SHARED-CASING-006 explicitly does not cover a camelCase/`_camelCase` old name whose first word starts with two uppercase letters; no requirement states `_httpClient`.
- #973 — RH1003-001 defines parts as "operands left after flattening nested additions", which literally includes `int` additions; today's report matches the wording. Narrowing it is a behavior change (or at most a wording clarification: doubt 2).

## 3. Critical candidates

- #967 — RH2005 fix makes a field referenced outside its type private → CS0122, solution no longer builds.
- #1070 — casing fix renames an override of a metadata member → CS0115; renames a `DllImport` method → EntryPointNotFoundException at run time.
- #1073 — casing fix → CS0542/CS0694 (`class order { int Order; }`), breaks code under `#if`, splits a record's positional member (changes equality, `ToString`, `Deconstruct`, public API).
- #1286 — RH2001 fix drops comments, writes `readonly readonly` (CS1004), changes mutation semantics of a mutable struct property.
- #1287 — RH2004 fix on one part of a partial property → CS8799.
- #1288 — RH2004 fix writes `private` on an explicit interface operator (CS0106) and on an ordinary operator (CS0558).

## 4. Real bugs

- #967 — SHARED-FIX-007, SHARED-FIX-003. `static public int Counter;` → `private     static int Counter;` at column 0 → `    private static int Counter;`. Field read from another type → fix offered, CS0122 → fix withheld.
- #974 — RH0001-003. `{ } junk` → no diagnostic → "invalid JSON syntax".
- #1015 — SHARED-FIX-002. Field-like `dataChanged` + custom `saved` event, Fix All from one → other unchanged → both renamed.
- #1016 — SHARED-FIX-001 (and SHARED-CASING-008). RH4106 `__1st` → `_1st`, still reported → fix withheld; RH4005 `_2x` → `I2x` → `II2x` (two applications).
- #1071 — SHARED-DIAG-002. `class @class` in `class.cs` → reported, file renamed `@class.cs` → not reported; `namespace A . B` → compared as raw text → compared as `A.B`.
- #1073 — SHARED-FIX-003. `class order { public int Order; }` → renamed `Order` (CS0542) → fix withheld (also `#if NEVER` reference, record positional member).
- #1268 — SHARED-DIAG-002. `using NIE = System.NotImplementedException; throw new NIE();` → not reported → reported.
- #1286 — SHARED-FIX-003. `private readonly int Value { get; }` → `private readonly readonly int _value;` → valid field (or withheld); comments dropped → fix withheld.
- #1287 — SHARED-DIAG-004. `partial void Q();` + `async partial void Q() {…}` → RH2002 twice → once; RH2004 fix on one partial property part → CS8799 → withheld.
- #1288 — SHARED-DIAG-011 / SHARED-FIX-003. `static N IAdd<N>.operator +(N a, N b)` → reported, fix writes `private` (CS0106) → not reported (comment: exempt like RH2004-003); `static S operator -(S a)` → `private` (CS0558) → withheld today, `public` only after a change doc.
- #1289 — SHARED-DIAG-003. `<!-- @code { -->`, `@* @code { *@`, `@@code {`, `user@code{` → reported → not reported.
- Sub-bug inside #1084 — SHARED-CASING-008 via RH4005-002: interface `_` → `I_` (still reported) → fix withheld.

## 5. Groups

- **Casing conversion (`CasingUtilities`, CASING-006/-008)**: #971, #1016, #1084 — one change doc (for #971/#1084) plus the #1016 bug; overlap on RH4005 prefix (#1016 `_2x`, #1084 `i2Factory`, `_`). RH2001 field naming (#971 comment) rides along.
- **Casing fix guards (`CasingCodeFixProviderBase`)**: #1073 (bug), #1072 (bc), fix part of #1070 (bug) — the bugs can go in one bug-lane run; #1072 and the no-report part of #1070 need change docs.
- **RH4001/RH4009**: #1071 (bug), #1085 (bc), #1086 (bc) — #1085/#1086 one change doc; #1071 separate bug run or same run with change doc for the others.
- **RH41xx scope**: #1087 + #1088 (must ship together), #1089 (separate change doc, same lane possible).
- **Naming cleanups**: #1090 + #1032 (one Maintenance PR, both RH4116/RH4118).
- **`DeclarationModifierUtilities` / modifier fixes**: #967, #1288 (and the RH2004 indent instance on #967); cross-family F5 #1325 (RH7105/RH7106 fix).
- **RH2001**: #1286 (bug lane) and #1299 (bc lane) — same rule, different lanes; do #1286 first.
- **Struct equality RH1001/RH1002 (SHARED-EQUAL)**: #1290, #1295, #1296, #1297 — all bc, one change doc can cover them.
- **P16 value judgement**: #1290 and #1298 share SHARED-DIAG-005 transparency.
- **P15 partial**: #1287 (cross-family: F5 RH7002/RH74xx, F6 RH8306) with #1300 related (RH2002 explicit impl).
- Singletons: #973, #974, #1015, #1268, #1289, #862.
- Duplicates: none exact. #1288 Q3 is moot if its Q2 exemption ships (stated in its comment).

## 6. Doubts

1. Pre-SDD "decided as bug" issues (#971, #973, #1084, #1085) ask for behavior that the merged specs explicitly leave outside coverage — treat as behavior change with change doc (rule applied here) or as bug?
2. #973: is RH1003-001's "flattening nested additions" meant to include non-string additions? If not, it is a wording clarification plus bug; as written it is not a bug.
3. #1070: split the fix-safety part (SHARED-FIX-003, critical, 1.2.0) from the no-report part (new SHARED-CASING-011, vNext)?
4. #1288 mixed: the RH2004-003 exemption for explicit operators is a bug under SHARED-DIAG-011, `public` for operators (RH2004-006 covers excludes operators) is an extension — one run with a change doc, or split?
5. #1289 mixed: Q1–Q3 are bugs (DIAG-003), `@functions` (comment) is a behavior change — split?
6. #1300 mentions a fix being withheld, but RH2002 has no code fix (non-goal in `RH2002.md`) — the fix half is moot unless a new fix is wanted.
7. #1298: the `const string E = ""` instance is already required by SHARED-DEBUG-001; if it is not reported today, that instance is a bug, not part of the change.
8. #1268 and #1071: the rule requirements' covers sentences (RH2003-001, RH4001-002/-005, RH4009-002) exclude alias/`@`/trivia while SHARED-DIAG-002 mandates them — classified bug (general MUST); the fix PR must update those covers sentences.
9. #1297 is titled "(bug)" and #1299 says "bug" for Q5, but both are outside current coverage → behavior change; confirm.
10. #1287 spans F5/F6 rules and carries a change doc for RH2004-002 — which family/lane owns it?
11. #1016 is on 1.2.0 but not critical — keep 1.2.0 or move to bug-pool?
12. #862 asks whether `Reihitsu.Core` is the right home; overlaps #1034 (F4) on `AttributeTargetUtilities` ownership — decide together.
