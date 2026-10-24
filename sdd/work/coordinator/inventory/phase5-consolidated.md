# Phase 5 — consolidated inventory (215 open issues)

Source: `phase5-index.md` and `phase5-f1.md` … `phase5-f8.md` (2026-10-24, specs on `main` after #1429). Key requirement IDs re-checked by grep in `sdd/specs/` (FMT-PIPE-013/-014, FMT-STRUCT-022/-024, FMT-SWITCH-006, FMT-BLANK-016, SHARED-COMMENT-011/-012, SHARED-DIAG-002/-004/-006/-008/-009/-010/-011, SHARED-DOC-004/-008, SHARED-TYPEREGION-006, SHARED-USING-004/-007, RH7003-001, RH7102-003): all present with the wording the families cite.

**Check.** A 28 · B 46 · C 18 · D 83 · E 7 · F 17 · G 2 · H 14 = **215**. Every index issue appears in exactly one family table (215/215, no duplicates); none is missing from the family files.

Precedence used: a critical issue whose requirement is already on main goes to C (marked CRITICAL); any other critical issue goes to A, mixed or not (split noted there); a non-critical mixed issue goes to H.

## A. Critical candidates (proposed 1.2.0) — 28

Also critical, listed under C: #1340, #1384 (borderline), #1417, #1422.

| Issue | Harm | Milestone | Example (input → today's output) |
|---|---|---|---|
| #948 Bracing a switch section breaks cross-section local (FMT SwitchCaseBracePhase) | build break | Triage | `case 1: int y = 1; F(y); break; case 2: y = 2; F(y); break;` → both sections braced, CS0103 |
| #949 Comment before terminal break gets no blank line in pass 1 (FMT-BLANK-011) | non-idempotent | Triage | `A(); B();⏎// before break⏎break;` → pass 1 `}⏎// before break`, pass 2 adds the blank line |
| #954 Empty region after `{` gains blank line every pass (LineBreakPhase vs BlankLinePhase) | phases undo each other | Triage | `{⏎#region A⏎#endregion // A⏎}` → blank line inserted and removed each pass; ends with blank line after `{` |
| #1329 Declaration after unclosed doc element becomes comment (FMT doc phase) | code loss / meaning change | (none) | `/// <summary>S⏎int A;` → `///     int A;` |
| #1389 BOM before line break dropped; doc text behind code deleted (FMT-CLEAN-009, FMT-PIPE-002) | comment loss | SDD | `int A; /// <summary>end</summary>` → `int A; /// <summary>` |
| #1390 Structural output does not compile / not stable (FMT-PIPE-003/-004, FMT-STRUCT-006) | build break; non-idempotent | SDD | `async T RunAsync() => await T.Delay(1);` (T = alias of Task) → `{ return await …; }` CS1997; `with` initializer `{` moves again in pass 2 |
| #951 Raw strings: holes/nested not idempotent (FMT-PIPE-004, FMT-RAW) | non-idempotent | Triage | `$"""` hole `{⏎ name⏎ }` → pass 1 `}` col 21, pass 2 col 8. Split: case 3 (tabs, non-goal) → D |
| #1146 RH5104 fix: literal-safe, comment kinds, interpolations (RH5104) | meaning change (Q1); comment reorder (Q4) | SDD | `@"a⏎b" + /* c */ x;` → fix inserts `/* c */` inside the verbatim string. Split: Q3/Q5/Q6 → D |
| #1075 Blank-line rules edit lines inside literals/comments (RH5011/5025/5027/5030) | meaning change | SDD | `$"""⏎a⏎⏎{x}⏎"""` → RH5025 fix deletes the empty literal line; `break; /* a⏎b */` → RH5011 inserts a line inside the comment |
| #1133 RH5601 reports closing-line tab of interpolated raw string (RH5601-002) | build break | 1.2.0 | tab on closing line of `$"""…"""` → reported, fixed code no longer compiles |
| #1261 RH5405–07/5412–17 fixes edit/duplicate comment and directive text (SHARED-FIX-003) | comment duplication; possible build break — **needs reproduction** | SDD | `{ M(a, b); // trail }` → `// trail` duplicated; `#region legacy⏎#endregion⏎class A {}` → `#region Legacy` |
| #1134 RH7109 reorders layout-defining struct fields (SHARED-FIX-003) | meaning change | Triage | `struct S { public int a; public readonly int b; }` → fields swapped (offsets change) |
| #1135 RH7309 swap changes static initializer order (SHARED-FIX-003) | meaning change | 1.2.0 | regions `IComparable` (`Y = X + 1`) then `Fields` (`X = 5`) → swapped, `Y` = 6 instead of 1 |
| #1262 Region fixes withheld when breaking/losing docs (RH7303/04/06/09) | build break; doc loss | SDD | `#region Fields⏎/* c */ int _a;` → RH7304 `/* c */ #region Fields` (does not parse) |
| #1332 RH7501 fix breaks code / moves directives (RH7501-004) | build break; meaning change | SDD | `{ var y = 1;⏎#if DEBUG⏎A(y);⏎#endif⏎break; }` → `#if`/`break` behind `}`, CS0103. Split: Q10 (blank before `break`) → D |
| #1336 Organization fixes withheld when breaking (RH7004, RH7101, RH7105/06) | build break | SDD | `static public⏎#pragma warning disable CS0169⏎int _a;` → `public static #pragma …` (does not parse) |
| #1241 Doc fixes rewrite element without end tag (RH8204, RH8305/07/09) | code loss / build break | Triage | `/// <placeholder>Later text⏎public void Run()` → `///     public void Run()` |
| #1344 RH8304/RH8309 fixes break code / move comments (SHARED-FIX-003) | build break; comment relocation | SDD | `int A; /// <summary>S</summary><remarks>R</remarks>⏎int B;` → `    int A; /// <remarks>R</remarks>` (CS0102) |
| #967 RH2005 fix: indent, refs outside type (SHARED-FIX-003/-007) | build break | Triage | field read from another type → made private, CS0122; `static public int Counter;` → `private     static int Counter;` at col 0 |
| #1070 Naming: names dictated by metadata/native (casing fixes) | build break; run-time crash | Triage | override of metadata member renamed → CS0115; `DllImport` method renamed → EntryPointNotFoundException. Split: no-report part (SHARED-CASING-011) → D |
| #1073 Casing fix: withhold unsafe renames (SHARED-FIX-003) | build break; meaning change | Triage | `class order { public int Order; }` → rename gives CS0542; record positional member split |
| #1286 RH2001 fix withheld where unsafe (SHARED-FIX-003) | build break; comment loss; meaning change | SDD | `private readonly int Value { get; }` → `private readonly readonly int _value;` (CS1004) |
| #1287 Partial declaration is one declaration (SHARED-DIAG-004; P15) | build break | SDD | RH2004 fix on one part of a partial property → CS8799. Note: also changes RH2004-002 text |
| #1288 RH2004 fix on explicit interface operators (SHARED-DIAG-011, SHARED-FIX-003) | build break | SDD | `static N IAdd<N>.operator +(N a, N b)` → fix writes `private` (CS0106). Split: `public` for operators → D |
| #1257 RH3001/3005/3101/3105 fixes make generic call (SHARED-FIX-003) | build break | Triage | `M(Max > G, B > (7))` → `M(G < Max, B > (7))` (CS0307) |
| #1279 RH3004 fix: static, layout, parentheses (RH3004-003, SHARED-FIX-003/-007) | build break | SDD | `delegate () { return 1; } + h` → `() => 1 + h` (CS0019). Split: keep `static` / parenthesize → D |
| #1281 Unsafe fixes withheld: RH3005/3101/3106/3201 (SHARED-FIX-003) | build break; meaning change | SDD | `return/**/a;` → `returna;`; `base.M()` → virtual `M()`. Trim: RH3103 part superseded by #1417 |
| #1282 RH3101/3104–3107 fixes delete comment text (SHARED-FIX-003) | comment loss | SDD | `base /* a */ . /* b */ M()` → `M()` |

## B. Real bugs, not critical (bug pool) — 46

Group keys: FIX-SCOPE = SHARED-FIX-006/-007 node-level formatting; FIX-CONV = SHARED-FIX-001/-002/-005; EOL = SHARED-FIX-008.

- #952 Directive after disabled text keeps indentation (FMT IndentationPhase) — FMT-INDENT-009 — key: fmt-indent-directives (+#1400 item 22)
- #1239 Rewritten `# region` keeps whitespace (RegionFormattingPhase) — FMT-REGION-001 — key: region-line rewriter (+#964/#1083 D); milestone vNext today
- #1392 Blank line after comment before else/catch kept (BlankLinePhase) — FMT-BLANK-008 — key: fmt-blank-lines (+#949, #954)
- #888 RH5201 link behind multi-line comment tail (chain) — SHARED-COMMENT-012, SHARED-FIX-001; the real defect: formatter does not break before `.D` — key: chain-P33 (+#1382, #889)
- #1243 Lone-operator join inserts blank line (LineBreakPhase) — FMT-BREAK-017/-018 scope sentence (weak, no ID) — key: operator-join (blocks #1273)
- #1391 Indent/raw miss specified cases (IndentationPhase, RawString) — FMT-INDENT-012/-013/-015/-016, FMT-RAW-001 — key: fmt-indent-raw (+#951)
- #1140 Line-break/alignment fixes change only their code (RH51xx/52xx) — SHARED-FIX-006 — key: FIX-SCOPE
- #1144 Fix output = formatter layout (RH5101/02/03/05) — SHARED-FIX-007 — key: FIX-SCOPE
- #1145 RH5103 fix behind embedded stmts, one-line blocks (RH5103) — RH5103-003/-004, SHARED-FIX-007 — key: RH5103 (+#1156 D)
- #1147 Comment-led line not reported (RH5110, 52xx, 53xx) — SHARED-COMMENT-011 (SHARED-INIT-001 to amend) — key: comment-led-line P12 (before G5 #1271–#1273)
- #1150 Delegating fixes withheld when formatter returns input (~39 providers) — SHARED-FIX-005 — key: FIX-CONV
- #1152 RH5204 span on indented `///` (RH5204) — RH5204-005 — key: RH5204 (+#1157 D)
- #1274 RH5308 fix converges or is withheld (RH5308) — SHARED-FIX-001 — key: FIX-CONV / RH53xx (+#1273)
- #1386 RH5033–RH5049 treat stray `///`/`/** */` as ordinary — SHARED-COMMENT-001 — key: comment-kinds P13 (+#1077 RH5033 instance)
- #1387 Conditional directive in list not reported (RH5102/5109/5111/55xx) — SHARED-DIAG-008 — key: P22-directive-in-span (+#1337)
- #946 RH5005/RH5006 fix never converges on `await using` — SHARED-STMT-004, SHARED-FIX-001 — key: statement-blank-line fix base (1.2.0 today)
- #972 Statement blank-line fix inserts two blank lines (RH5001–5019) — SHARED-STMT-004 — key: statement-blank-line fix base
- #1096 Statement blank-line fixes: `case … when` indent, blank after `///` — SHARED-STMT-004, RH5011-001 — key: statement-blank-line fix base
- #1076 Blank-line fixes converge, no undo, no no-op (RH5011, RH5022–27, RH5032) — SHARED-FIX-001/-004/-005 — key: brace-blank-line rules (RH5024 instance: verify)
- #1094 RH5513 fix: break before list, indent (RH5513) — SHARED-FIX-007 (Q2); Q3/Q4 doubtful — key: attribute fix bases
- #1142 Attribute-list split fixes change only the list, keep EOL (RH5502–5531) — SHARED-ATTR-017, SHARED-FIX-006/-008 — key: attribute fix bases / EOL
- #1141 RH6023/RH6024 fixes rewrite only operator gaps — SHARED-FIX-006 — key: FIX-SCOPE
- #966 `#` in block comment blocks reorder; `global::` (using phase) — SHARED-USING-009 (case 2 doubtful) — key: using-ordering
- #1105 Using rules compare names by value — SHARED-DIAG-002 — key: using-ordering
- #1266 Using rules compare with earlier directive of group (RH7203/04/06) — SHARED-USING-007 — key: using-ordering
- #1267 Alias to non-name forms last group (RH7204/7207, formatter) — SHARED-USING-004 — key: using-ordering
- #969 Member-ordering fixes reformat whole type (RH7102/03/09) — SHARED-FIX-006 — key: FIX-SCOPE (+#1335)
- #1335 Organization fixes change only their node (RH7004, RH7101, RH7104–08) — SHARED-FIX-006/-007 — key: FIX-SCOPE (+#969)
- #1151 RH7302 fix renames `#endregion` only if exact name — RH7302-005 — key: regions RH73xx
- #1263 Region-removing fixes clean blank line (RH7303, RH7310) — SHARED-FIX-007 — key: regions RH73xx (+#1262)
- #1333 RH7403/07/11 every declarator of field-like event — SHARED-DIAG-006 — key: RH74xx verdicts (+#1264, #1334)
- #1412 RH7105/06 miss declaring part of partial ctor — SHARED-DIAG-004 — key: modifier rules (+#1337); **needs reproduction** (source audit)
- #1269 RH8201/8202/8204 fixes only reported markup, one application — SHARED-FIX-001/-006/-008 — key: doc FIX-SCOPE (+#1342)
- #1342 Doc layout fixes format only the comment (RH8305/07/09/8402) — SHARED-FIX-006, SHARED-DOC-004 — key: doc FIX-SCOPE
- #1343 Doc fixes finish in one application (RH8308/09/8401/8305) — SHARED-FIX-001/-005 — key: doc FIX-CONV
- #1270 RH8106/8203/8305/8306 check every element — SHARED-DIAG-006 — key: doc analyzers DIAG-006
- #1348 RH8302 silent before `#region` (RH8302 vs RH5031) — SHARED-FIX-004 — key: doc layout analyzers; borderline critical (fix cycle)
- #974 RH0001 accepts content after root object — RH0001-003 — key: singleton (config)
- #1015 RH4102 Fix All covers one event kind — SHARED-FIX-002 — key: FIX-CONV
- #1016 Casing fixes produce still-reported name — SHARED-FIX-001, SHARED-CASING-008 — key: casing utilities (1.2.0 today)
- #1071 RH4001/RH4009 compare names by value — SHARED-DIAG-002 — key: name-by-value (+#1105; #1268 C)
- #1143 Fix All finishes in one application (26 test classes) — SHARED-FIX-002 — key: FIX-CONV (after #1140, #1141, #1142)
- #1283 RH3006 fix keeps line ending — SHARED-FIX-008 — key: EOL (+#1388)
- #1388 Fixes keep line ending (RH5405, RH5011, RH8304, FormatNode) — SHARED-FIX-008, FMT-PIPE-007 — key: EOL
- #1284 Expression-body fixes format only the body (RH3202–3208) — SHARED-FIX-006 — key: FIX-SCOPE
- #1285 RH3001 fix in argument/list positions — RH3001-004 — key: comparison-rewrite fixes (+#1257)

## C. Already specified → now a bug — 18

Framed as behavior change / change doc, but the requirement is on main.

- #1381 Keep comment layout when inserting braces (P32; FMT) — FMT-PIPE-013 (#1405)
- #1384 Do not split a documented field (P36; FMT) — FMT-STRUCT-024 (#1405) — **CRITICAL (borderline)**: `/// …⏎public int X, Y;` → split, `Y` undocumented (CS1591)
- #1395 Comment before later declarator moves above split field (FMT) — FMT-STRUCT-022; title says behavior change
- #1398 Break-only section never braces the switch (FMT) — FMT-SWITCH-006; title says behavior change; conflicts with -004 (J3)
- #1382 Token behind multi-line comment tail starts no line (P33; FMT, RH5203) — SHARED-COMMENT-012 (#1380). Split: the RH5204 and SHARED-INIT-001 parts → D
- #1383 One treatment per syntax family (P34; FMT) — FMT-PIPE-014 (#1380)
- #1416 Brace fixes RH5405–07 keep wrapped statement's layout (P26) — SHARED-FIX-007; SHARED-BRACE-002 contradicts (J5)
- #1264 Innermost declaration/region owns member (RH7308/09/11, RH74xx; P17) — SHARED-TYPEREGION-006 ¶2
- #1334 RH7401–RH7412 verdict independent of base order (P25) — SHARED-DIAG-009 (Q6 may go beyond it)
- #1337 Declaration rules skip lists with conditional directive (RH7101, RH7105–08; P22) — SHARED-DIAG-008, RH7101-001
- #1339 RH7102/RH7109 each top-level region on its own (P27) — RH7102-003, RH7109-003
- #1340 RH7003 skips namespace containing namespace — RH7003-001 — **CRITICAL**: `namespace A.B { namespace Inner {…} }` → fix gives CS8955; title says behavior change
- #1345 RH8301–8303 judge behind-code `///` (P29) — SHARED-DOC-004 ¶2
- #1346 RH8401/RH8402 check in every doc mode (P30) — SHARED-DOC-008 ¶2
- #1347 RH8307 skips `/** */` and inline elements (P31) — SHARED-DIAG-010
- #1268 RH2003 resolves exception type via alias ("P3 extension") — SHARED-DIAG-002
- #1417 RH3103 not reported where `T?` invalid (P38) — SHARED-DIAG-011 — **CRITICAL**: `nameof(System.Nullable<int>)` → reported, fix writes `typeof(?)`
- #1422 RH3104/RH5417 not reported where form invalid (P38) — SHARED-DIAG-011 — **CRITICAL**: `new S();` → RH3104 fix writes `default(S);` (CS0201); no milestone

## D. Behavior changes / extensions (proposed vNext) — 83

**New rule (12)**
- #352 Cognitive complexity limit (new RH2###; 1.4.0 today)
- #353 Method line-count limit (new RH2###; 1.4.0)
- #354 Type line-count limit (new RH2###; 1.4.0)
- #849 Get-only property → expression body (new RH3###, mirrors FMT-STRUCT-014; 1.3.0)
- #850 Get-only indexer → expression body (new RH3###; 1.3.0)
- #851 Expression-bodied indexer single-lined (new RH54##, RH5401 twin; 1.3.0)
- #892 Line break before attached part of chain (new rule, P11; 1.2.0 today)
- #894 Trivia inside `?.`/`!.` (new Layout rule; 1.2.0 today)
- #1088 Casing of local constants (new RH41xx; ship with #1087)
- #1159 Attribute argument lists one line or one per line (new RH51##)
- #1160 Comment does not touch code (new RH6###; blocked by #1078)
- #1356 Doc tag name wrong case (new RH8xxx; with #1091/#1355)

**Widen or change an existing rule (46)**
- #893 Report remaining chain layouts (RH5201, P11; 1.2.0 today)
- #897 RH5112/RH5048 wording "first member access" (title/message only; 1.2.0 today)
- #1077 `/** */` that documents nothing = block comment (spacing rules, formatter; P13; RH5033 instance = #1386)
- #1097 Comment rules: chain anchor, `is`/`as` (RH5035–5049)
- #1098 Construct owns only inside braces/arms (SHARED-COMMENT-004/-006, FMT-BREAK-015)
- #1154 RH5110 checks record structs (P11)
- #1155 RH5111 checks nested assignments (P11)
- #1156 RH5103 predecessor's last line; top-level statements (P11)
- #1157 RH5204 lines not led by keyword (P11; blocked by #951)
- #1158 RH5108 primary ctor, lambda, anon method params (P11)
- #1271 RH5301/RH5304 nested initializers (P11)
- #1272 RH5305/RH5306 whole collection expr / list pattern (P11)
- #1273 RH5302/RH5308 operator alone on line (P11; blocked by #1243)
- #1078 Spacing rules: no report touching a comment (RH6003–6022)
- #1080 RH5022 blank after `{` with trailing comment (P11)
- #1095 Deconstruction = local declaration (RH5029, RH5021, formatter)
- #1275 Empty-type rules report `};`, directives by kind (SHARED-EMPTY; blocked by #1240)
- #1396 RH5030 exempts `}` followed by block statement (ends #1076 cycle)
- #965 RH7207/using phase accept blank before comment-led directive (analyzer+formatter; loop found by code reading)
- #968 RH7103 effective accessibility (+RH7102/7109/7110)
- #1265 RH7303 regions in every executable place (P11)
- #1338 RH7105/06 enum, destructor, local-function, accessor modifiers (P11)
- #1354 RH7302 prescribed names; formatter keeps region casing (retires FMT-REGION-001 casing)
- #1091 Documented exactly when compiler doc has summary (P8; RH80xx–RH83xx)
- #1292 RH8102/8103/8109/8110/8202 every documentable declaration (P11)
- #1293 RH8107 owns void members, RH8106 skips them
- #1294 RH8203 matches ctors as compiler resolves (titled "(bug)")
- #1341 `///` that documents nothing is ordinary (RH8301–8309, RH8401; P13)
- #1349 RH8306 checks indexer summaries (P11)
- #1350 RH8501 `UnconditionalSuppressMessage`, `<Pending>`
- #1351 RH8308 text after XML comment behind closing tag
- #1352 RH8309 `/** */` order; custom element moves with predecessor
- #973 RH1003 numeric sub-additions not parts (narrows RH1003-001)
- #1085 RH4001 migration prefix; RH4009 nested full name
- #1086 RH4001 skip files with top-level statements
- #1087 RH4115/RH4117 `_`, local consts, deconstruction (with #1088)
- #1089 RH4124 query range vars; RH4122 extension receivers
- #1290 RH1001/RH1002 through parens, `!`, `throw` (P16)
- #1295 RH1001/RH1002 Nullable, tuples, collection exprs
- #1296 RH1002 more methods, query clauses (P21)
- #1297 RH1002 comparer type arg vs key type (titled "(bug)")
- #1298 RH2006/RH2007 `string.Empty`, all-empty conditionals (P16; `const` instance may already be a bug)
- #1299 RH2001 only replaceable `field` props; attributed exempt
- #1300 RH2002 explicit interface async void (P23; "fix" half moot, RH2002 has no fix)
- #1278 RH3002/RH3003 report constructs they name (P11)
- #1280 RH3201 empty comments of every kind (P13)

**Formatter new behavior (17)**
- #959 Normalize line ends inside comments and disabled text (FMT-PIPE-002 allowance)
- #960 Trailing whitespace at directive line end / after last token (cleanup non-goal; bug label)
- #961 Doc end tag on last content line; wrapped inline elements (FMT-DOC-009/-010; bug label)
- #962 `///` behind statement / in expression relocated (FMT-DOC-001; bug label)
- #964 Normalize `#region` line independently of capitalization (FMT-REGION-001)
- #1082 File ending in directive drops final line break (FMT-CLEAN-007)
- #1083 Replace tabs in region names only (FMT-CLEAN-003; widens RH5601)
- #1148 Blank before line-leading comment only between statements/members (FMT-BLANK-011 narrowed)
- #1355 Doc tag names matched case-sensitively (FMT-DOC-008; with #1356)
- #895 Join attached parts outside chains (1.3.0 today)
- #896 Anchor wrapped first call under last prefix dot (+RH5201 anchor; 1.3.0 today)
- #953 Indent unowned continuation lines one level (umbrella)
- #955 Wrapped ctor-initializer colon, wrapped constraint (bug label)
- #956 One switch arm per line; argument lists like parameter lists
- #957 `fixed(` spacing; double space before comment (FMT-SPACE-012/-014; bug label)
- #1149 FMT-BREAK-016 covers enum members (may be a bug under FMT-PIPE-014)
- #1399 Region in initializer/argument list; nested array rows (FMT-INDENT-010/-023)

**Code fix new coverage / guard scope (5)**
- #1033 Attribute placement fixes: missing-brace guard only for enclosing braces (SHARED-ATTR-012)
- #1277 Fix guards: RH5307 reported list, RH5401 own lines (RH5307-004, RH5401-003/-004)
- #970 Ordering fixes allow balanced `#pragma` (SHARED-ORDER-006; bug label)
- #971 Casing fix `HTTPClient` → `_httpClient` (SHARED-CASING-006; bug label)
- #1072 Casing fix withheld when derived name is keyword (SHARED-CASING-007)

**CLI / Playground / IDE (3)**
- #122 VS extension for the formatter (new surface)
- #975 `--dry-run` header `a//full/path` (CLI-013 example pins today's output; bug label)
- #976 Playground note for generated input (PLAY-005/-006)

## E. Clarification-only (proposed Maintenance) — 7

- #1400 Spec clarifications: inactive directives, lambda anchor, multi-line comment gaps (FMT-INDENT/CLEAN/REGION/SPACE)
- #1131 `Uses` SHARED-ATTR in line-breaks.md (FMT-BREAK-012/-027)
- #1291 Formatter extras beyond RH5304/RH5308/struct equality (FMT-BREAK-021, SHARED-EQUAL-001; P11)
- #1394 Shared-spec clarifications from round 9 (SHARED-COMMENT-011, RH5111, SHARED-FIX-007/-008)
- #1081 RH5025 sibling braces; lone `\r` in FMT-PIPE-007 (item 2 already on main; item 1 and the U+2028/U+0085 instance remain)
- #1136 SHARED-FIX-003: relocate/reorder, disabled text (doubt: may count as a behavior change, see M)
- #1240 diagnostics.md non-goal vs inactive `#region` (SHARED-DIAG-008; P22)

## F. Internal (proposed Maintenance) — 17

- #950 Remove dead switch brace-removal path (FMT switch phase)
- #889 Test mid-line statement context of chain fixes (RH5112/RH5201)
- #1244 Unreachable nested-element recursion (RH5303/RH5304; closes with #1271)
- #1330 Test interpolated-raw/UTF-8 lines in non-formattable helper (RH8301–8303; no milestone)
- #862 Consolidate auto-property accessor-list predicate (Core; with #1034)
- #1090 Rename RH4118TupleElementCasingAnalyzer
- #1032 Remove unused RH4116Title/RH4118Title resources (with #1090)
- #867 Verifier compiles at warning level 4 (test harness)
- #877 RH3002 follow-ups (re-parse, cost, tests)
- #898 `scripts/trace.sh` lacks exec bit
- #1054 lane-check: `squad/decisions/` = tooling
- #1259 Share RH3106/RH3107 empty-list removal (with #1282)
- #1427 Handover templates: follow-up candidates (squad)
- #1428 Requirement-coverage exemption gaps (SDD tooling)
- #1053 Rule pages RH8001–RH8018: which declarations are private (docs; after #1092)
- #1132 Param-doc rules match `name` alike (helper refactor only left)
- #1242 Spec owner of DocumentationCommentCodeFixUtilities (decide before doc FIX-SCOPE runs)

## G. Close / trim candidates — 2 (+ trims)

- #1034 Decide spec ownership of `AttributeTargetUtilities` — already done on main (listed as code owner in both places)
- #1055 Doc group of extension-block members — SHARED-DOC-002 already names extension blocks; close after checking that a covering test exists
- Trims (issue stays open in another bucket): #1281 RH3103 part (superseded by #1417); #1077 RH5033 instance (= #1386); #1081 item 2 (done); #1076 RH25/RH30 cycle instance (= #1396); #1132 (questions answered, moved to #1153); #1288 Q3 (moot once Q2 ships); #953 comment instances that #1383, #895 and #1397 already carry

## H. Mixed issues to split — 14

- #963 `=>` comment glued to brace; comment duplicated on split fields (FMT) — field part → B/C (FMT-STRUCT-024, with #1384/#1395); `{// why` part → D or fold into #1390
- #958 Spacing: arrows, `?:`, `::`, named args (FMT) — arrow part → C (FMT-PIPE-014, with #1383); rest → D
- #1397 Line breaks: unjoinable lists, FMT-BREAK-005 moves (FMT) — item 16 → B (FMT-BREAK-005); items 15/17/18 → D
- #1393 RH5104 blank before moved comment; RH5417 brace — RH5104 part → B once J1 is decided (SHARED-FIX-006 wins); RH5417 part → D (SHARED-FIX-007 bullet names members only)
- #1074 Spacing: whitespace around literals/holes is code (RH5601/5602/6009/6010) — RH6009/RH6010 → B (SHARED-DIAG-003); RH5601/RH5602 → J4 decision
- #1079 Spacing: `///` tabs, `# region`, `operator` spaces (+RH5603) — RH5603 → B (SHARED-FIX-001); rest → D
- #1093 Attribute placement follows attached declaration (RH55xx) — formatter `[param: A] set;` join → B (FMT-BREAK-027); rest → D
- #1276 RH5404 every one-line type body; extension block keyword — `Location.None` → B (SHARED-DIAG-007); widening → D
- #1385 Required blank line after directives (formatter, RH5002/RH5008; P37) — formatter part → C (FMT-BLANK-016); SHARED-STMT-004 / fix part → D
- #1092 Doc group from symbol; static ctor private (RH8001/8002/8017/8018) — Q3b → B (SHARED-DIAG-004); Q4 → D
- #1153 P8b/P8c params as compiler binds; RH8030 — RH8032 without location → B (SHARED-DIAG-007); rest → D (with #1091)
- #1353 RH8402 fix replaces whole run; header ends at line break — Q1/Q2 → E; Q3 → D (titled "(bug)")
- #1084 Casing fix: keep prefix, all identifier chars (RH4005/4121) — `_` → `I_` → B (SHARED-CASING-008, with #1016); rest → D
- #1289 RH2109 reads Razor comments/strings/escapes — Q1–Q3 → B (SHARED-DIAG-003); `@functions` → D

Splits inside A/C issues: #951 case 3, #1146 Q3/Q5/Q6, #1070 no-report part, #1288 `public` operators, #1279 `static`/parenthesize, #1332 Q10, #1382 RH5204/SHARED-INIT-001 → D.

## I. Wrong labels / misframed titles

`bug` label, not a bug:
- #960 — cleanup.md lists both cases as non-goals
- #961 — FMT-DOC-010 keeps case 1; FMT-DOC-009 leaves inline elements undecided
- #962 — FMT-DOC-001 "does not yet decide" statements/expressions
- #963 (half) — the `{// why` part has no requirement
- #955 — no requirement covers a colon-ended initializer or a wrap in a constraint
- #957 — FMT-SPACE-012 omits `fixed`; FMT-SPACE-014 forbids part 2
- #951 (case 3) — tabs are a raw-string-alignment non-goal
- #968 — RH7103-003 says "written accessibility"
- #970 — SHARED-ORDER-006 forbids any other directive kind
- #971 — SHARED-CASING-006 excludes two-capital camel names
- #973 — RH1003-001 "flattening nested additions" matches today
- #975 — the CLI-013 example pins today's header
- #888 (misframed, still a bug) — the defect is the missing formatter break, not RH5201's report

Title says "(bug)" / body or lane says "bug fix", not a bug:
- #1294 — RH8203-001 coverage excludes these ctors
- #1297 — outside SHARED-EQUAL-002 coverage
- #1353 Q3 — outside RH8402-003 coverage
- #1299 Q5 — attributes unspecified
- #1272 — needs RH5305-002/RH5306-001 changes
- #1277 — RH5307-004 and RH5401-003/-004 limit the fix
- #1146 Q3/Q5/Q6 — RH5104-001/-005 match today
- #1080 — RH5022-001 covers only lines ending with `{`
- #1079 (except RH5603) — specs exclude these cases explicitly
- #1077 — no cross-cutting P13 requirement
- #1085 — outside RH4001-005/RH4009-002 coverage
- #1280 — every instance contradicts RH3201-001/-002
- #1074 (RH5601/5602 parts) — rule specs state today's behavior
- #1094 Q3/Q4 — outside SHARED-ATTR-008/-009 coverage

Titled "(behavior change)" or asking for a change doc, but a bug: every C issue. Explicit titles: #1340, #1395, #1398.

## J. Spec contradictions (maintainer decisions)

1. SHARED-FIX-006 vs RH5104-005 (requires the blank line before a moved comment) — #1393, #1146
2. SHARED-FIX-006 vs RH5111-004 / RH5401-003 ("format as the formatter formats it in place") — #1140
3. FMT-SWITCH-004 vs FMT-SWITCH-006 (break-only section with wrapping label) — #1398
4. SHARED-DIAG-003 vs RH5601-001 / RH5602-001 (literal/hole whitespace) — #1074, #1133
5. SHARED-FIX-007 vs SHARED-BRACE-002 (wrapped statement layout) — #1416, #1261
6. SHARED-FIX-008 vs RH5011-004 / RH8304-004 (lone `\r`) — #1388
7. SHARED-FIX-006 vs SHARED-BODY-003 ("laid out as the formatter lays it out") — #1284
8. SHARED-DIAG-008 vs RH5102-004 / RH5109-004 / RH5111-005 ("still reported") — #1387, #1337
9. SHARED-USING-007 ("every earlier directive") vs RH7203/7204/7206-001 ("directly follows") — #1266
10. SHARED-DIAG-006 vs SHARED-REGGROUP-005 / RH7403/07/11-001 ("one declarator") — #1333
11. SHARED-DIAG-009 vs SHARED-REGGROUP-006 ("the analyzed type declaration") — #1334
12. SHARED-FIX-003 vs RH7309-004 (requires the reorder of `///` shapes) — #1262
13. SHARED-DOC-004 ¶2 vs RH8303-001/-004/-005 ("starts its line") — #1345
14. SHARED-DIAG-011 vs RH3103-001 (still lists `typeof`/dotted forms) — #1417
15. SHARED-DIAG-002 vs covers of RH2003-001, RH4001-002/-005, RH4009-002 (exclude alias/`@`/trivia) — #1268, #1071
16. SHARED-COMMENT-011 vs SHARED-INIT-001 (one-comment limit) — #1147, #1382
17. SHARED-DIAG-004 vs SHARED-DOC-002/-005 coverage and RH2004-002 (partials) — #1092, #1091, #1287
18. RH6001-001 omits `fixed`, but the analyzer and the rule page report it — #957
19. SHARED-COMMENT-012 is listed under `Uses` only by RH5203, yet is written for every line-start rule and the formatter — #888, #1382
20. FMT-BLANK-016 (formatter: blank line after directives) vs SHARED-STMT-004 (fix side unchanged) — #1385

## K. Blockers still open

- #1157 → #951
- #1146 (Q5; RH5204 hole indentation) → #951; coordinate with #1075
- #1273 → #1243
- #1160 → #1078
- #1275 → #1240
- #1143 → #1140, #1141, #1142
- #1355 → #1356 (paired rule), #1091
- #1356 → #1091 (related)
- #1053 → #1092
- #1244 → #1271 (closes with it)
- #1088 ↔ #1087 (ship together)
- #1265 (fix side) → #1262
- #955 → #953 (partly)
- #1332, #1336 → #1136 (related, not blocking)
- #1416 → sequence after #1261

## L. Template conformity

- Follows a template: yes 49 · partly 43 · no 123 (total 215).
- "No" by bucket: A 13, B 23, C 17, H 10 → **bug report** fits the shape. Most are multi-rule SDD follow-ups that cite requirement IDs, though, so a candidate new template, "SDD follow-up decision / spec violation" (Decision · Lane · Requirement · Instances · Blocked by · Done when), fits them better.
- "No" in D: 34 → **feature request** (analyzer/formatter). A rule-widening variant ("P11 own gap") would fit better.
- "No" in E, F and G: 26 → **none fits**. Candidate new templates: "Spec clarification" (E) and "Internal / code review" (F; the `[CODE REVIEW]` issues already use an ad-hoc form).

## M. Doubts (merged, deduplicated)

**Lane / change doc**
- Run C issues in the bug lane with no change doc (align lagging rule texts in the same PR)? #1381 #1384 #1395 #1398 #1382 #1383 #1416 #1264 #1334 #1337 #1339 #1340 #1345 #1346 #1347 #1268 #1417 #1422; formatter part of #1385
- Pre-SDD "decided as bug" vs the spec excluding the case: extension per the rule, or add a requirement and fix as a bug? #960 #961 #962 #955 #971 #973 #1084 #1085 #975 #1077 #1080
- Does extending SHARED-FIX-003 (#1136) count as a clarification or as a behavior change? It turns existing fixes (#1332, #1336) into bugs
- #1277 guard widening: behavior change, or a bug under the AGENTS.md guard-scope rule?
- #1341 and #1077: P13 lives only in SHARED-COMMENT-001; confirm a change doc is needed for RH83xx, the spacing rules and the formatter

**Scope of a requirement**
- Does SHARED-COMMENT-012 bind RH5201, RH5204 and the formatter (#888, #1382)? Rewrite #888 to the real defect and merge it into #1382?
- Does FMT-PIPE-014 "every assignment operator" include the enum `=`? If yes, #1149 is a bug
- #1243 has no requirement ID: a bug, or a new FMT-BREAK requirement plus a fix?
- #966 case 2: is `global::` stripped from the SHARED-USING-007 sort key?
- #1094 Q3/Q4: does SHARED-FIX-007 alone make them bugs?
- #1343: does `&amp;` count as an XML node for RH8308-003?
- #1241: SHARED-DOC-017 names only RH8107/8202/8204; for RH8305/07/09 the claim rests on SHARED-FIX-003 — split?
- #1298: the `const string E = ""` instance is already required by SHARED-DEBUG-001 — a bug if it is not reported today
- #1393 RH5417 part: does the SHARED-FIX-007 shared-line bullet cover the type's `}`?
- #1334 Q6 (both partial parts in a matching region) goes beyond SHARED-DIAG-009
- #1291 struct-equality instance belongs with RH1001/RH1002 (F7); SHARED-EQUAL-001 still carries the old limitation

**Criticality / milestone**
- Borderline critical: #1384 (CS1591, no text loss), #954 (stable output, one blank line), #1348 (fix cycle), #965 (fix vs formatter loop, code reading), #1146 Q4 (comment reorder)
- Critical bugs proposed for bug-pool by their families: move to 1.2.0? #1257 #1279 #1281 #1282 #1417 #1422
- Non-critical issues on 1.2.0 today: #946 #888 #1016 #892 #893 #894 #897 — keep, or move to bug-pool/vNext? #897 is a cheap message fix
- Put #1134 (Triage) in 1.2.0 with its twin #1135?
- No milestone: #1329 #1330 #1422 #1427 #1428. Bug in vNext: #1239. On 1.3.0/1.4.0: #849–#851, #352–#354, #895, #896. 139 issues on `SDD` need re-homing
- Split mixed issues as section H lists, or run each as one PR with a change doc? Also the splits inside A/C (#951 #1146 #1070 #1288 #1279 #1332 #1382)

**Verify before scheduling**
- Reproduce: #1261 (build-break claim), #1412 (source audit), #965 (code reading), #1076 RH5024 instance (possibly fixed)
- #1383: check whether #1405 already fixed some instances
- #1055: check for a covering test before closing
- #867: raising the warning level may expose fix defects that need their own issues

**Ownership / process**
- #862 and #1034: is `Reihitsu.Core` the right home for `AttributeTargetUtilities`? Decide together
- #1242: decide the owner of the doc-fix helper before the doc FIX-SCOPE runs (#1241 #1269 #1342 #1344)
- #1287 spans F5, F6 and F7 rules: which lane owns it?
- #1132: keep it open for the refactor, or close it and file a Maintenance issue?
- #122: roadmap item rather than one squad run?
- #851: re-check the suggested ID RH5418
- #1082: does a directives-only file stay unchanged (FMT-CLEAN-007)? Leave to its change doc?
- #968: scope the change doc to RH7102, RH7103, RH7109 and RH7110
- #1300: the fix half is moot (RH2002 has no code fix)
- #951 / #1157 / #1146 Q5: schedule #951 before them
