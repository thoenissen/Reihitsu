# Inventory — round 9 (questions filed by the round-8 runs #1138, #939, #1260, #1161, #1139)

Read 2026-10-22 via GitHub MCP (list/read only, no writes). `⏎` = line break, `␍` = lone CR.

## 1. Run status

| PR | Issue | Merged | Issue closed | Filed by the run | Proposed follow-ups / items for the maintainer (from PR body; no PR comments, no new issue comments) |
|---|---|---|---|---|---|
| #1357 | #1138 (SHARED-FIX-006..008) | yes 2026-10-22 17:32 | yes | #1363 (Q1–Q6), #1377 (docs: cite SHARED-FIX-008) | Disabled text vs SHARED-FIX-008 left open as #1363 Q4. RH7103 non-goal now decided by SHARED-FIX-006, left for #1137. Further instances commented on #1144 (2), #1284, #1137. Tests pinning forbidden behavior stay, owned by #1140/#1284/#1335. |
| #1358 | #939 (formatter + Playground tags) | yes 2026-10-22 19:11 | yes | #1364–#1373 (+ comments Q-P2 on #1366, Q-I8 on #1370) | **Unfiled follow-ups:** (1) `sdd/README.md` *Tests and requirements* names only three projects with `RequirementAttribute` and "Enforced today" omits `formatter/`, `playground/` (left for #1161, which did not touch it -> still stale); (2) formatter spec non-goals still call decided questions open: #951, #953, #956, #958, #959, #960, #976. Instances commented on #1144, #1081, #953, #963, #1148, #1077, #962, #1348, #1076, #948, #956. Decision entry `squad/decisions/2026-10-22-939-playground-component-tests.md` (bUnit). |
| #1359 | #1260 (P15–P22) | yes 2026-10-22 18:18 | yes | #1374, #1375, #1376, #1378, #1379 (all "Decided by P22") | Comments on #1264, #1275, #1241. **Auditor hint, not acted on:** SHARED-FIX-003 wording from #1357 ("a rule reports the construct it names for every input") reads broader than the not-reported cases of SHARED-DIAG-008 and SHARED-JOIN-001 -> spec-consistency follow-up candidate. Weak coverage accepted: SHARED-USING-004 item 18 (waits #1267), Razor sentence (#1289), `#if`/`#else` half of item 19 (#1264). DIAG-008 narrowed (reading B: `#region`/`#pragma` do not by themselves stop the report; rule-own requirement may). |
| #1360 | #1161 (P11 section) | yes 2026-10-21 18:07 | yes | none | **For the maintainer:** (1) add a one-line pointer in `AGENTS.md` *Issue intake* to the P11 section? (2) opening sentence reads slightly wider than the own-gap case (wording only). **For #1331:** closing sentence of the P11 section needs narrowing when P31 is added. |
| #1361 | #1139 (P12, P13) | yes 2026-10-22 19:29 | yes | #1362 (Q1–Q3 body, Q4–Q7 comments + 2 corrections) | Non-conforming shapes tracked in #1147 (new instances: RH5205, RH5206 brace), #1077, #1146, #1341; #1144 RH5203 own-line comment to column 0 found again. |

All five merged, all five issues closed, no open PR from these runs.

## 2. New issues created on/after 2026-10-21T12:00Z with number > #1356

#1357–#1361 are the PRs. No closed issue in the range. None has labels or a milestone (earlier follow-ups carry milestone `SDD`). None is attached to or listed on #912.

**SDD open-questions issues (17, 54 questions):**

| # | Title (spec) | State | Run | Qs | Gist |
|---|---|---|---|---|---|
| #1362 | shared/comment-placement.md | open | #1139 | 7 (Q1–Q7) | P12/P13 edges: `///` documenting nothing; misplaced leading comment; RH5111; multi-line comment tail; RH5302/5308/5305/5301/5303 instances |
| #1363 | shared/code-fixes.md | open | #1138 | 6 (Q1–Q6) | P14a vs moved comment; P14b vs rename; `} }`; disabled-text EOL; RH5405 LF in CRLF; lone-CR documents |
| #1364 | formatter/cleanup.md | open | #939 | 1 (Q-C1) | BOM lost when a line break follows it |
| #1365 | formatter/raw-string-alignment.md | open | #939 | 1 (Q-R1) | raw string with lone `\r` breaks not aligned |
| #1366 | formatter/pipeline.md | open | #939 | 2 (Q-P1, Q-P2) | `FormatNode` EOL from node; `with` initializer not idempotent |
| #1367 | formatter/documentation-comments.md | open | #939 | 1 (Q-D1) | doc comment behind code loses its text when rebuilt |
| #1368 | formatter/switch-case-braces.md | open | #939 | 2 (SW-Q1, SW-Q2) | break-only section with multi-line label; brace insertion loses comment layout |
| #1369 | formatter/horizontal-spacing.md | open | #939 | 2 (SP-Q1, SP-Q2) | gap around multi-line comment; token pairs without a rule |
| #1370 | formatter/indentation.md | open | #939 | 8 (Q-I1–Q-I8) | region column in initializers/args; nested array rows; base-list continuation; empty list `)`; comment before `]`; inactive directives; lambda return type anchor; omitted type argument |
| #1371 | formatter/structural-transforms.md | open | #939 | 5 (Q-S1–Q-S5) | async alias `Task` -> CS1997; bracing loses comment layout; comments around converted expression body; comment before declarator; documented field split |
| #1372 | formatter/blank-lines.md | open | #939 | 4 (Q-B1–Q-B4) | comment+blank before `else`; block statement after `}`; directive before section; directive+comment before statement |
| #1373 | formatter/line-breaks.md | open | #939 | 10 (LB-Q1–LB-Q10) | extension braces; multi-line comment tail; auto-property list; FMT-BREAK-005; indexer lists; nested conditional; parenthesized pattern; arrow spacing; compound assignment; argument list on next line |
| #1374 | shared/using-directive-ordering.md | open | #1260 | 1 | `#pragma`/`#region` in using block stops report (claimed P22) |
| #1375 | analyzer/RH5111.md | open | #1260 | 1 | `#if` between declarator and `=` keeps report (P22) |
| #1376 | shared/attribute-targets.md | open | #1260 | 1 | `#if` in attribute list keeps report (P22) |
| #1378 | analyzer/RH5102.md | open | #1260 | 1 | `#if` in argument list keeps report (P22) |
| #1379 | analyzer/RH5109.md | open | #1260 | 1 | `#if` in parameter list keeps report (P22) |

**Other issues (1):**

| # | Title | State | Run | Gist |
|---|---|---|---|---|
| #1377 | SDD: Let the line-ending clauses of fix requirements cite SHARED-FIX-008 | open | #1138 (PR #1357) | Docs lane: 7 requirements (SHARED-STMT-004, SHARED-AFTERBRACE-001, SHARED-REGION-002, RH5011-005, RH8303-005, SHARED-ATTR-008/-017) restate the EOL rule -> cite SHARED-FIX-008; SHARED-ATTR-008/-017 point at FMT-PIPE-007 detection, a difference to keep and file if it matters (see #1363 Q6). Blocked by #1138 (now closed) -> unblocked. |

## 3. Question inventory

Principles: P1 unsafe fix; P7 convergence; P9 fix scope; P10 fix layout; P11 formatter may do more; P12 line-leading comment is the line's first element; P13 comment kinds alike; P14a no blank line before changed node; P14b changed member own line; P14c removed region cleans blank line; P14d format only replaced node; P16 look-through (value judgments); P21 location + extension block; P22 conditional directive in judged span = not reported (`#region`/`#pragma` stay reported, fix withheld); P24 fix keeps document EOL; P29 doc comment behind code; P31 layout rule does not report a shape the formatter keeps. "Spec" = decided by an existing active requirement the code violates (bug without a question).

| Issue | Q | Rule/spec | Input → real output | Why wrong | Req IDs | Principle | Duplicates / related open | Kind + recommendation | Fully decided? |
|---|---|---|---|---|---|---|---|---|---|
| #1362 | Q1 | RH5033, RH5038, RH5034 (RH5033–RH5049) | `[Obsolete]⏎/// <summary>M</summary>⏎void M()`, `/** … */` variant, `M(⏎/// first⏎1);` → not reported in any doc mode; `/// …⏎// note⏎/// …⏎void N()` → only `// note` (RH5034) | CS1587: comment documents nothing → ordinary comment (SHARED-COMMENT-001 amended) | SHARED-COMMENT-001, RH5033-001, RH5038-001 | P13 (claim verified: P13 says a `///` that documents nothing is an ordinary comment in every mode) | #1077 (`/** */`, P6), #1341 (RH83xx only), #1146 (RH5104 kinds) | Bug. New follow-up "Comment-placement rules RH5033–RH5049 treat `///`/`/** */` that documents nothing as an ordinary comment (P13)", sibling of #1341 | Yes |
| #1362 | Q2 | RH5110, RH5201, RH5203, RH5204, RH5205, RH5206 | `void M<T>()⏎/* c */ where T : class` (comment col 0); `P(1,⏎        /* two */ 2);` (comment right of column) → reported on the token | Unclear whether a misplaced leading comment is judged or the line is never reported | SHARED-COMMENT-011, RH5110-001, RH5201-002, RH5203-001, RH5204-001, RH5205-001, RH5206-001 | P12 (open edge) | #1147; precedents SHARED-INIT-001 (a), RH5201-002/RH5204-001 (a), RH5306-001 (b) | Clarification. Recommend (a): judge the comment's column (line reported when its first element is misplaced) | No |
| #1362 | Q3 | RH5111 | `int x⏎    /* c */ = 1;` → reported, fix withheld | RH5111 checks line relation, not line start | SHARED-COMMENT-011, RH5111-001/002/005 | P12 (scope) | #1155, #1149 | Clarification. Recommend (a): not covered (RH5111 is not a line-start rule), stays reported | No |
| #1362 | Q4 | RH5203, RH5204 (SHARED-INIT-001) | `    /* a⏎    b */       private int _a;` → RH5204 not reported; `P(1,⏎          /* a⏎          b */ 2);` → RH5203 reported | Rules disagree on a line led by a comment's tail | SHARED-COMMENT-011, SHARED-BLANK-002, SHARED-INIT-001, RH5203-001, RH5204-001 | none (candidate P33) | #1373 LB-Q2 (same shape), #888 (RH5201 inside comment) | Clarification; decide with LB-Q2. Recommend (b) per SHARED-BLANK-002: a token behind a tail does not start its line | No |
| #1362 | Q5 | RH5302, RH5308 | `return a⏎       /* and */ && b;` (comment in operator column) → reported; formatter keeps | Comment is the line's first element in the right column | SHARED-COMMENT-011, RH5302-002 (corrected), RH5308-001/002 | P12 (verified: SHARED-COMMENT-011 now states it) | #1147 (RH5110/5203/5205/5206), #1273 (operator alone, other cause) | Bug. Extend #1147 | Yes |
| #1362 | Q6 | RH5305 | `int[] b = [⏎…1⏎                  /* end */ ];` (comment in `[` column) → reported, fix no progress | Same | SHARED-COMMENT-011, RH5305-001 | P12 (+P7 for the fix) | #1147, #1272 | Bug. Extend #1147 | Yes |
| #1362 | Q7 | RH5301, RH5303 | `new C⏎{⏎    /* a */ /* b */ A = 1,…` → reported, fix no progress | SHARED-INIT-001 limits to one comment; P12 has no limit | SHARED-COMMENT-011, SHARED-INIT-001, RH5301-002, RH5303-002 | P12 (verified; requires amending SHARED-INIT-001) | #1147, #1271 | Bug + spec amend of SHARED-INIT-001. Extend #1147 | Yes |
| #1363 | Q1 | RH5104 fix | `M();⏎M(); // second` → `M();⏎⏎// second⏎M();` | Fix inserts blank line before the node it changes | SHARED-FIX-006 (P14a), RH5104-005 | P14a (likely, not claimed) | #1146 (RH5104), #1148 (FMT-BLANK-011) | Recommend A: bug, RH5104-005 changes (the moved comment is the changed node's leading trivia) | No (confirm) |
| #1363 | Q2 | RH4106 rename | `private int Value; private int _b;` → `private int _value; private int _b;` | P14b literal vs P9 | SHARED-FIX-006/-007, RH4106 | P9 vs P14b | — | Clarification. Recommend A: P14b only for fixes that lay out the member | No |
| #1363 | Q3 | RH5417 fix | `    void M() { } }` → `    void M()⏎    {⏎    } }` | Type `}` stays on the edited line | SHARED-FIX-007, RH5417 | P14b/P10 | #1261 (RH5417 comments) | Recommend A: bug, the brace shares a line the fix edits (as #1170: next member to its own line) | No |
| #1363 | Q4 | any fix (RH4106) | CRLF doc, `#if NEVER` text with LF → LF kept | SHARED-FIX-008 names only literals/comments | SHARED-FIX-008, SHARED-FIX-003, FMT-PIPE-007 | P1 + FMT-PIPE-007 ("inside disabled text keep their original line ending") | #959, #1136 | Recommend A: exempt, clarification (matches FMT-PIPE-007 and AGENTS.md) | Nearly (confirm) |
| #1363 | Q5 | RH5405 fix | CRLF doc, `if (a) M(a, b); /* one⏎ two */` → created break after first comment copy is LF | Fix creates a non-document EOL | SHARED-FIX-008 | P24 (verified) | #1261 (duplicated comment), #1283, #1142 | Bug. Extend #1261 or new "RH5405 fix keeps the document's line ending (P24)" | Yes |
| #1363 | Q6 | RH5405, RH5417 fixes | all-`␍` doc → created breaks are LF | claimed P24 | SHARED-FIX-008, FMT-PIPE-007 | P24 claimed — **contested**: #1081 decided the formatter counts a lone `\r` as `\n` (FMT-PIPE-007), and SHARED-ATTR-008/-017 tie fix EOL to FMT-PIPE-007 | #1081, #1377 | Needs decision; recommend: fix EOL = FMT-PIPE-007 detection (clarification, behavior kept) | No |
| #1364 | Q-C1 | formatter cleanup | `﻿⏎class C{int x;}` → BOM dropped | FMT-CLEAN-009 MUST keep BOM | FMT-CLEAN-009 | Spec | — | Bug (low reach: API callers only) | Yes |
| #1365 | Q-R1 | raw strings | `var a = """␍    Hello␍    """;` → not aligned | FMT-RAW-001 covers every multi-line raw string | FMT-RAW-001, FMT-EOL-001 | Spec | #951, #1081 | Bug (edge; also U+0085/2028/2029) | Yes |
| #1366 | Q-P1 | `FormatNode` (RH5401 fix) | CRLF file, lambda `() => { M(); }` → LF breaks | Node-local detection falls back to `Environment.NewLine` | FMT-PIPE-007, SHARED-FIX-008 | P24 (verified) | #1283 | Bug | Yes |
| #1366 | Q-P2 | formatter | `R P => _r with { A = 1, B = 2 };` → pass 2 moves `{` | Not idempotent | FMT-PIPE-004, FMT-BREAK-021 | Spec | #949 (other), #1291 (FMT-BREAK-021 text) | Bug | Yes |
| #1367 | Q-D1 | formatter doc comments | `int A; /// <summary>end</summary>` → `int A; /// <summary>`; multi-line variant drops a line | Comment text deleted | FMT-DOC-003/008/009/011, FMT-PIPE-002 | Spec (non-destructive) | #962, #1329, #1345 (P29) | Bug | Yes |
| #1368 | SW-Q1 | switch braces | break-only section with `case Point { X: 1,⏎ Y: 2 }:` → whole switch braced | FMT-SWITCH-004 vs -006 | FMT-SWITCH-004, FMT-SWITCH-006 | none | — | Clarification/behavior change; recommend B (break-only section never makes the switch braced) | No |
| #1368 | SW-Q2 | switch braces | `case 1: /* a */ // b⏎/* c */ A();…` and blank between `// first`/`// second` → `/* a */// b`, `/* c */` own line, blank dropped | Layout between comments lost only because braces were inserted | FMT-SWITCH-007, FMT-PIPE-002 | none (candidate P32) | = #1371 Q-S2; #963 | Recommend B (keep spaces, same-line placement, blank lines) | No |
| #1369 | SP-Q1 | spacing | `a /* one⏎    two */    + 1;` → `two */ + 1` and `a   /* three` → `a /* three` | Line break only inside comment text | FMT-SPACE-001/-013 | none | #1078 (analyzer side) | Clarification; recommend A (each side of the comment is a same-line gap; text says so) | No |
| #1369 | SP-Q2 | spacing | `where T:class`, `x is >5`, `1=>2`, `let w=v`, `delegate* < int, void >`, `[return:Obsolete]` vs `[return : Obsolete]` → kept (runs collapsed only); attribute target colon spaced by target | No rule; attribute-target colon inconsistent | FMT-SPACE-002/-005/-009/-010/-012 | P11 (formatter may do more) | #958 | Recommend: add the listed pairs to #958 (behavior change); attribute-target colon treated alike for every target | No |
| #1370 | Q-I1 | indentation | `#region X` inside object initializer / collection expr / switch expr / argument list → statement column | Why: region takes the column of what it groups | FMT-INDENT-010 | none | #1265 (RH7303 reports such regions), #1175 Q2 | Recommend: region takes the elements' column (bug against the Why; text amended) | No |
| #1370 | Q-I2 | indentation | `int[,] a = {⏎{⏎1, 2⏎},…` → inner content/`}` at statement column | Nested row has no alignment owner | FMT-INDENT-023 | none | #953 | Recommend: nested row aligned like a complex element initializer (behavior change) | No |
| #1370 | Q-I3 | indentation | `class C : System.IDisposable,⏎          System. // c⏎    ICloneable` → `ICloneable` col 0 | FMT-INDENT-013 lists FMT-INDENT-016 lists; code skips base lists | FMT-INDENT-013/-016 | Spec | — | Bug (code follows FMT-INDENT-013, as argument lists do) | Yes (confirm direction) |
| #1370 | Q-I4 | indentation | `M( // c⏎              );` → `)` at col 8 | FMT-INDENT-012 "whenever the list spans several lines" | FMT-INDENT-012 | Spec | — | Bug (edge) | Yes |
| #1370 | Q-I5 | indentation | comment before closing `]` of collection expr → `]` column; before `}` → content column | Why applies to `]` too | FMT-INDENT-006, -025 | none | — | Recommend (b): one level deeper like before `}` (behavior change) | No |
| #1370 | Q-I6 | cleanup/region/indent | `#if NEVER⏎→#region Inner⏎→disabled⏎→#endregion⏎#endif` → `····#region Inner`, `····#endregion // Inner` | "MUST NOT be touched" vs FMT-REGION-006/FMT-CLEAN-002 | FMT-INDENT-010, FMT-CLEAN-002/-003, FMT-REGION-006 | none | #952, #959, #1083, #1240 | Recommend (a): sentence means "not re-indented"; the directive is still a directive (clarification) | No |
| #1370 | Q-I7 | indentation | `Func<int,int> a = int (x) =>⏎{` → `{` under `(` (col 31) | Why says lambda's own start; anchor text = `async` or `(` | FMT-INDENT-028 | none | — | Recommend (a): keep, name the case in the text (anchor deliberately `async`/`(`; (b) would also move attribute/`static` lambdas) | No |
| #1370 | Q-I8 | indentation | `typeof(Dictionary< // omitted⏎,>)` → `,>);` col 16 | Spec: MUST keep block indentation (col 8); layout computes 8 | FMT-INDENT-015 | Spec | — | Bug | Yes |
| #1371 | Q-S1 | structural | `using T = System.Threading.Tasks.Task;` `async T RunAsync() => await T.Delay(1);` → `return await …;` CS1997 (also `VT`, custom task-like) | Formatted file does not compile | FMT-STRUCT-004, FMT-PIPE-003 | Spec (FMT-PIPE-003) | — | Bug; minimal fix: under `async`, convert only when the return type is syntactically known (`Task`/`ValueTask` -> statement, generic -> `return`), otherwise keep the expression body | Yes |
| #1371 | Q-S2 | structural | `if (a)⏎// one⏎⏎// two⏎Foo();` / `Foo(); /* a */ // b` → blank dropped, `/* a */// b` | Bracing loses layout between comments | FMT-STRUCT-002 | none (candidate P32) | = #1368 SW-Q2; #963 | Recommend B (keep) | No |
| #1371 | Q-S3 | structural | `int Other()⏎// before arrow⏎=> 1;` → comment between signature and `{`; `int Get() =>⏎// c⏎42;` → `return⏎⏎// c⏎42;` | MUST carry into block / join `return` | FMT-STRUCT-006 | Spec | #963, #1148, #953 | Bug | Yes |
| #1371 | Q-S4 | structural | `private int _c, /* before d */ _d /* after d */;` → `private int _c; /* before d */` | Comment no longer next to `_d` | FMT-STRUCT-022 | none | #963 | Options A keep / B own line before `private int _d` / C `private int /* before d */ _d`; recommend B | No |
| #1371 | Q-S5 | structural + RH8026/RH8027 | `/// <summary>The coordinates</summary>⏎public int X, Y;` → `Y` undocumented | Formatter output reported by analyzers; compiler documents every declarator | FMT-STRUCT-021/-022, RH8026-001, RH8027-001 | none (candidate P36) | #963 (refuse split for comment before first declarator) | Recommend C: refuse the split for a documented declaration | No |
| #1372 | Q-B1 | blank lines | `}⏎// c⏎⏎else` → blank kept (RH5026/RH5027 report output) | FMT-BLANK-008 MUST remove blank lines directly before `else`/`catch`/`finally`/do-`while` | FMT-BLANK-008, RH5026, RH5027 | Spec (+P31 symptom) | #1148, #1076 | Bug | Yes |
| #1372 | Q-B2 | blank lines + RH5025/RH5030 | `if (…) { … }⏎⏎{ value++; }` → blank removed; RH5030 reports output, RH5025 reports input (cycle) | SHARED-BLANK-003 vs FMT-BLANK-008 | FMT-BLANK-005/-008, SHARED-BLANK-003, RH5025-001, RH5030 | P7 for the cycle (SHARED-FIX-004) | #1076, #1081 (RH5025 sibling braces = reading A, blank above standalone block reported!) | Needs decision; note #1081 item 1 decided RH5025 reports the blank above a standalone block. Recommend (a) block statement is a statement: blank after `}` required, RH5025/FMT-BLANK-008 exempt a block statement's `{` (reverses part of #1081) or (b) RH5030 exempts a following block statement (consistent with #1081) | No |
| #1372 | Q-B3 | blank lines | `break;⏎#pragma …⏎case 2:` → blank inserted before `#pragma`; in a loop not | FMT-BLANK-006 Why applies to both | FMT-BLANK-004/-006, RH5011-002 | P11 | — | Recommend: exemption applies to section gaps too (no blank line in front of a directive) | No |
| #1372 | Q-B4 | blank lines + RH5002/RH5008 | `a++;⏎#if DEBUG⏎a--;⏎#endif⏎// why⏎if (a > 0)` → blank above `#if` | Line placed above the conditional group; SHARED-STMT-004 says above the first comment | FMT-BLANK-001/-005/-006, SHARED-STMT-004 | none | #1096 | Recommend: blank line after the directives, directly above the first comment (formatter + RH5002/RH5008 fixes, which already violate SHARED-STMT-004) | No |
| #1373 | LB-Q1 | line breaks | `extension(int x) { public int A => x; public int B => x; }` → `{` on header, `}` glued | Allman not applied to extension blocks | FMT-BREAK-002/-007 | Round 7a item 17 (#1221 Q1: extension block reported at keyword and block formatted) + P11 | #1276 | Bug/behavior change: extension block braces like a type | Yes (confirm) |
| #1373 | LB-Q2 | line breaks | `int a; /* first⏎    second */ int b;` → unchanged (tests assert) | Element MUST start own line | FMT-BREAK-006/-007/-025/-027 | none (candidate P33) | #1362 Q4, #888 | Recommend B (break in front, as behind `{`); decide with #1362 Q4 | No |
| #1373 | LB-Q3 | line breaks | `int Q /* q */ { get; set; }` → expanded to 4 lines; `public⏎int B { get; }` → expanded | One-line list expanded | FMT-BREAK-024/-025, RH5408-002/-003 | P31-adjacent | #862, #953 | Recommend B: a list that cannot be joined is left as written | No |
| #1373 | LB-Q4 | line breaks | `if (a) /* c */ {` → comment moved above; `if (a) /* x */ // y` → nothing moves; `while (a) if (a) // w` → comment behind `while (a)`, line break in embedded statement; `lbl: if (a) // l` → `lbl: // l` | Predicate differs from text in 3 ways | FMT-BREAK-005, FMT-BREAK-001 | none | — | Three decisions: (a) recommend code follows text (do not move a comment with `{` behind it; break before `{`); (b) move the whole trailing comment run; (c) do not move when the `if` does not start its line | No |
| #1373 | LB-Q5 | line breaks | `int this[int a⏎             , int b] => a;` → unchanged | "every parameter list" | FMT-BREAK-011/-012 | none (text favors B) | RH5106/RH5107 own gap (P11) | Recommend B: indexer lists laid out like the others; RH5106/RH5107 gap is a separate P11 behavior change | No |
| #1373 | LB-Q6 | line breaks | `a ? (b ? 1 : 2) : 3`, `a ? () => b ? 1 : 2 : null` → one line; `a ? 1 : b ? 2 : 3` → spread | "nested" narrower in code | FMT-BREAK-018 | P16 by analogy (not binding: SHARED-DIAG-005 is about values) | #1291 | Recommend: parentheses transparent, lambda body not | No |
| #1373 | LB-Q7 | line breaks | `case (1 or⏎2):` → `case⏎(⏎1 or⏎2⏎):` | No text for parenthesized pattern; no `case` exception | FMT-BREAK-022, FMT-SWITCH-004, FMT-INDENT-026 | none | #953 | Recommend: write the `(` rule into FMT-BREAK-022 with the same `case` exception as for property patterns (behavior change for the `case` part) | No |
| #1373 | LB-Q8 | line breaks/spacing | `int P =>5;` → `=> 5`; `get =>_p;` only collapsed | Arrow spacing set by line-break phase for some arrows | FMT-BREAK-026, FMT-SPACE non-goal | none | #958 | Recommend B: arrow spacing belongs to FMT-SPACE for every arrow kind; extend #958 | No |
| #1373 | LB-Q9 | line breaks | `x⏎    += 1;`, `x ??=⏎    2;` → wraps kept | "the `=` of an assignment" | FMT-BREAK-016 | none | #1155, #1149 | Recommend B: every assignment operator | No |
| #1373 | LB-Q10 | line breaks | `Foo⏎    (a);`, `arr⏎    [0]`, `class D(int a) : B⏎    (a, 2);` → kept | Joined only in chains | FMT-BREAK-020/-011/-012 | — | **= #895** ("Join attached parts outside member-access chains"), #956 | Recommend A for the spec (today); reading B is #895's scope — comment there | Yes (by #895) |
| #1374 | Q1 | RH7201–RH7207 | `using System.Linq;⏎#pragma warning disable CS0105⏎using System;` → not reported | Claimed P22: `#pragma`/`#region` do not stop the report | SHARED-USING-009, SHARED-DIAG-008 | P22 claimed — **not borne out after the narrowing**: DIAG-008 now allows a rule-own requirement to stay silent, and SHARED-USING-009's Why (silent exactly where the formatter does not reorder) is P31 | #966, #1266 | Recommend: close as consistent (no change); alternative: report without fix (P22 as decided in 7a item 16) | No (confirm) |
| #1375 | Q1 | RH5111 | `int value⏎#if DEBUG⏎#endif⏎= 1;` → reported, fix withheld | Judged span holds a conditional directive | RH5111-005, SHARED-DIAG-008 | P22 (verified) | #1337 (declaration rules, P22) | Bug | Yes |
| #1376 | Q1 | RH5501–RH5531 list-shape rules | `[A,⏎#if DEBUG⏎B,⏎#endif⏎C]` → reported, fix withheld | Same | SHARED-ATTR-011/-019/-020, RH5511-004, RH5514-005, RH5524-004 | P22 (verified) | #1337, #1142; maybe RH3002-009, RH5408-003 | Bug | Yes |
| #1378 | Q1 | RH5102 | `M(a, b,⏎#if DEBUG⏎c,⏎#endif⏎d);` → reported, fix withheld | Same | RH5102-004, SHARED-DIAG-008 | P22 (verified) | #1337 | Bug | Yes |
| #1379 | Q1 | RH5109 | `void M(int a, int b,⏎#if DEBUG⏎int c,⏎#endif⏎int d)` → reported, fix withheld | Same | RH5109-004, SHARED-DIAG-008 | P22 (verified) | #1337 | Bug | Yes |

**Counts:** 54 questions in 17 issues. Fully decided: 22 (by principle 10: #1362 Q1/Q5/Q6/Q7, #1363 Q5, #1366 Q-P1, #1375, #1376, #1378, #1379; by existing spec 10: #1364, #1365, #1366 Q-P2, #1367, #1370 Q-I3/I4/I8, #1371 Q-S1/S3, #1372 Q-B1; by earlier decision/duplicate 2: #1373 LB-Q1 (#1221 Q1), LB-Q10 (#895)). Needs maintainer: 32 (of which 4 are confirmations: #1363 Q1, #1363 Q4, #1374 Q1, and #1363 Q6 contested claim).

Claims checked: all "Decided by P12/P13/P22/P24" claims hold except **#1374 Q1** (P22 narrowed; SHARED-USING-009 is a rule-own requirement and fits P31) and **#1363 Q6** (lone CR: #1081 decided the formatter counts lone `\r` as `\n`; P24 was decided for CRLF/LF).

## 4. Decided → proposed follow-up action

1. **New: "Comment-placement rules RH5033–RH5049 treat a `///` or `/** */` that documents nothing as an ordinary comment (P13)"** — #1362 Q1 (bug; sibling of #1341, cross-link #1077).
2. **Extend #1147** (retitle to "RH5110, RH5203, RH5205, RH5206, RH5301–RH5305, RH5308: a line led by a comment is not reported (P12)") — #1362 Q5 (RH5302, RH5308), Q6 (RH5305), Q7 (RH5301, RH5303 + amend SHARED-INIT-001's one-comment limit).
3. **New: "Layout rules do not report a list or gap that holds a conditional directive: RH5102, RH5109, RH5111, RH5501–RH5531 (P22)"** — #1375, #1376, #1378, #1379; include an audit of other "every preprocessor directive" clauses (RH3002-009, RH5408-003). Tests `VerifyDirectiveInside…WithholdsTheCodeFix` change. Alternative: extend #1337 (declaration rules) — recommend a separate layout issue.
4. **Line endings (P24):** new "Fixes and `FormatNode` keep the document's line ending: RH5405 created break, `FormatNode` detection from the root (P24)" — #1363 Q5, #1366 Q-P1 (or extend #1283).
5. **New formatter bug issue(s) against existing requirements** (group by phase):
   - "Formatter keeps text it must keep: BOM (FMT-CLEAN-009), doc-comment text behind code (FMT-DOC-011)" — #1364, #1367.
   - "Formatter idempotency/compile: `with` initializer (FMT-PIPE-004), async alias `Task` (FMT-PIPE-003/FMT-STRUCT-004), comments of converted expression bodies (FMT-STRUCT-006)" — #1366 Q-P2, #1371 Q-S1, Q-S3.
   - "Indentation: base-list continuations (FMT-INDENT-013), empty wrapped list `)` (FMT-INDENT-012), omitted type argument (FMT-INDENT-015)" — #1370 Q-I3, Q-I4, Q-I8.
   - "FMT-BLANK-008: blank line after a comment before `else`/`catch`/`finally`/`while`" — #1372 Q-B1.
   - "Raw strings with lone `\r` breaks (FMT-RAW-001)" — #1365 (low; could join the indentation/raw issue).
6. **Extend #1276** (RH5404 extension block): formatter places extension-block braces per FMT-BREAK-002 — #1373 LB-Q1 (confirm reading of 7a item 17).
7. **Comment on #895**: argument list/element access/primary-constructor base arguments on the next line — #1373 LB-Q10; spec stays as today.
8. Close each question issue once its follow-ups exist; list them on #912 (see §6).

## 5. Needs maintainer decision

Grouped; numbering for the question message.

**G1 — comments and line starts (P12/P13 edges)**
- **1. #1362 Q2** misplaced leading comment. `class C⏎{⏎    void M<T>()⏎/* c */ where T : class⏎    {⏎    }⏎}` → RH5110 reports `where` today. (a) judge the comment's column: still reported (as RH5201/RH5204/SHARED-INIT-001); (b) never report a comment-led line (as RH5306). **Recommend (a).**
- **2. #1362 Q3** RH5111. `int x⏎    /* c */ = 1;` → reported, no fix. (a) not covered by P12 (stays reported); (b) covered. **Recommend (a).**
- **3. #1362 Q4 + #1373 LB-Q2** multi-line comment tail. `    /* a⏎    b */       private int _a;` (RH5204 silent) vs `P(1,⏎          /* a⏎          b */ 2);` (RH5203 reports); formatter keeps `int a; /* first⏎    second */ int b;`. (a) the tail is the line's first element: neither reported, formatter keeps; (b) the comment belongs to the line it starts on (SHARED-BLANK-002): token judged/reported, formatter breaks in front of `int b`; (c) SHARED-INIT-001 reading: comment's start judged, token not. **Recommend (b)** (candidate P33); SHARED-INIT-001 and two formatter tests change.

**G2 — fix scope and line endings (#1363)**
- **4. Q1** `M();⏎M(); // second` → RH5104 fix gives `M();⏎⏎// second⏎M();`. (a) P14a: no blank line (bug, RH5104-005 changes); (b) a moved comment may get a blank line. **Recommend (a).**
- **5. Q2** `private int Value; private int _b;` → rename keeps one line. (a) P14b only for fixes that lay out the member; (b) every fix splits. **Recommend (a).**
- **6. Q3** `    void M() { } }` → RH5417 gives `    {⏎    } }`. (a) type `}` to own line (bug); (b) stays. **Recommend (a).**
- **7. Q4** CRLF doc with LF inside `#if NEVER` → kept. (a) exempt (FMT-PIPE-007 already says so); (b) normalized. **Recommend (a).**
- **8. Q6** all-`␍` doc → RH5405/RH5417 fixes write LF. (a) fix EOL = FMT-PIPE-007 detection (lone CR counts as LF, #1081) — clarification, also answers #1377's SHARED-ATTR-008/-017 point; (b) P24 literal: write CR (bug in fixes, formatter differs). **Recommend (a).**

**G3 — formatter keeps comment layout when wrapping (candidate P32)**
- **9. #1368 SW-Q2 + #1371 Q-S2** `if (a)⏎    // one⏎⏎    // two⏎    Foo();` → braced block loses the blank line; `Foo(); /* a */ // b` → `/* a */// b`; switch: `/* c */ A();` split. (a) allowed (FMT-PIPE-002), text says so; (b) keep spaces, same-line placement and blank lines. **Recommend (b).**

**G4 — formatter refuses rather than degrades (candidate P36)**
- **10. #1371 Q-S5** `/// <summary>The coordinates</summary>⏎public int X, Y;` → `Y` undocumented, RH8026/RH8027 report. (a) keep; (b) repeat doc on every field; (c) refuse the split (as #963). **Recommend (c).**
- **11. #1371 Q-S4** `private int _c, /* before d */ _d /* after d */;` → `private int _c; /* before d */`. (a) keep; (b) `/* before d */` on its own line above `private int _d /* after d */;`; (c) `private int /* before d */ _d /* after d */;`. **Recommend (b).**

**G5 — blank lines**
- **12. #1372 Q-B2** `}⏎⏎{ value++; }` → formatter removes blank; RH5030 ↔ RH5025 cycle. (a) block statement is a statement: blank after `}` kept/required, RH5025 and FMT-BLANK-008 exempt a block statement's `{` (reverses the standalone-block part of #1081 item 1); (b) no blank before a standalone block, RH5030 exempts a following block statement (keeps #1081). **Recommend (b)** for consistency with #1081 (low cost); (a) if the maintainer prefers statement semantics.
- **13. #1372 Q-B3** `break;⏎#pragma warning disable CS0168⏎case 2:` → blank inserted above `#pragma`. (a) keep; (b) FMT-BLANK-006 exemption also for section gaps. **Recommend (b).**
- **14. #1372 Q-B4** `a++;⏎#if DEBUG⏎a--;⏎#endif⏎// why⏎if (a > 0) {}` → blank above `#if`. (a) keep; (b) blank after the directives, above `// why` (SHARED-STMT-004; formatter + RH5002/RH5008 fixes); (c) no blank. **Recommend (b).**

**G6 — line breaks**
- **15. #1373 LB-Q3** `int Q /* q */ { get; set; }` → 4 lines. (a) keep, qualify FMT-BREAK-024; (b) leave an unjoinable list as written. **Recommend (b).**
- **16. #1373 LB-Q4** (three parts) `if (a) /* c */ {` → comment moved above `if`; `if (a) /* x */ // y⏎{` → nothing moves; `while (a) if (a) // w⏎{` → comment trails `while (a)`. (a) do not move a comment with `{` behind it, break before `{`; (b) move the whole trailing run; (c) do not move when the `if` does not start its line. **Recommend all three.**
- **17. #1373 LB-Q5** `int this[int a⏎             , int b] => a;` → unchanged. (a) parenthesized lists only; (b) indexer lists too. **Recommend (b).**
- **18. #1373 LB-Q6** `var x = a ? (b ? 1 : 2) : 3;` → one line. (a) only a branch that is a conditional; (b) any nested conditional; (c) parentheses transparent, lambda not. **Recommend (c).**
- **19. #1373 LB-Q7** `case (1 or⏎2):` → `case⏎(⏎1 or⏎2⏎):`. (a) keep, write it down; (b) `(` rule written into FMT-BREAK-022 with the `case` exception. **Recommend (b).**
- **20. #1373 LB-Q8** `int P =>5;` → `=> 5` but `get =>_p;` kept. (a) record in FMT-BREAK-026; (b) every arrow's spacing in FMT-SPACE, extend #958. **Recommend (b).**
- **21. #1373 LB-Q9** `x⏎    += 1;` kept. (a) only `=`; (b) every assignment operator. **Recommend (b).**
- **22. #1368 SW-Q1** break-only section with `case Point { X: 1,⏎ Y: 2 }:` braces the whole switch. (a) FMT-SWITCH-004 wins (today); (b) FMT-SWITCH-006 wins. **Recommend (b).**

**G7 — indentation and spacing**
- **23. #1370 Q-I1** `var p = new P⏎        {⏎            #region X⏎            X = 1,` → `#region` at statement column. (a) keep (literal text); (b) region takes the elements' column. **Recommend (b).**
- **24. #1370 Q-I2** `int[,] a = {⏎{⏎1, 2⏎},` → inner rows at statement column. (a) keep; (b) align like a complex element initializer; (c) leave to #953. **Recommend (b).**
- **25. #1370 Q-I5** `// last` before `]` → `]` column. (a) keep; (b) one level deeper like before `}`. **Recommend (b).**
- **26. #1370 Q-I6** `#if NEVER⏎→#region Inner` → `····#region Inner`, `// Inner` added. (a) "not touched" = not re-indented, text corrected; (b) keep inactive directive whitespace/tabs. **Recommend (a).**
- **27. #1370 Q-I7** `int (x) =>⏎{` → `{` under `(`. (a) keep, name the case; (b) anchor = lambda's first token. **Recommend (a).**
- **28. #1369 SP-Q1** `a /* one⏎    two */    + 1;` → gaps normalized. (a) intended, text says a break inside a comment does not count; (b) gap left unchanged. **Recommend (a).**
- **29. #1369 SP-Q2** `where T:class`, `x is >5`, `1=>2`, `let w=v`, `delegate* < int, void >`, `[return:Obsolete]` (vs `[return : …]` → `[return: …]`). (a) collapse-only, as specified; (b) add all to #958; (c) add to #958 and fix the attribute-target colon inconsistency as a bug now. **Recommend (b)** (with the colon part as in (c)).

**G8 — confirmation**
- **30. #1374 Q1** `using System.Linq;⏎#pragma warning disable CS0105⏎using System;` → not reported. (a) consistent with narrowed DIAG-008 and P31: close, no change; (b) report without a fix. **Recommend (a).**
- **31. #1373 LB-Q1** confirm that 7a item 17 ("block formatted") covers the formatter's extension-block braces → extend #1276. **Recommend yes.**
- **32. #1370 Q-I3** confirm direction: code follows FMT-INDENT-013 (align base-list continuations) rather than dropping FMT-INDENT-016 from the text. **Recommend code.**

## 6. Principle candidates (continue at P32)

- **P32 — Wrapping keeps the author's comment layout.** A formatter rewrite that inserts braces or a block (switch-section braces, control-flow braces, expression-body conversion) keeps the spaces between same-line comments, a comment's same-line placement with its statement, and blank lines between leading comments. Decides #1368 SW-Q2, #1371 Q-S2; supports #963.
- **P33 — A multi-line comment belongs to the line where it starts.** A token behind its tail does not start its line (SHARED-BLANK-002 generalized). Decides #1362 Q4, #1373 LB-Q2; SHARED-INIT-001 aligned; relates #888.
- **P34 — One treatment per syntax family in the formatter.** Every parameter list (indexer included), every assignment operator, every arrow, every attribute target colon, every closing bracket of an initializer-like block is laid out alike. Decides #1373 LB-Q5, LB-Q8, LB-Q9, #1370 Q-I5, the attribute-colon part of #1369 SP-Q2.
- **P35 — Fix line ending = formatter detection** (P24 clarification). A fix writes the line ending FMT-PIPE-007 detects (lone `\r` counts as `\n`, #1081). Decides #1363 Q6 and #1377's SHARED-ATTR-008/-017 point.
- **P36 — The formatter refuses a structural transform it cannot complete safely.** When a transform would break compilation, drop documentation or relocate a comment, it is not applied (formatter analogue of P1/P23; #963 precedent). Decides #1371 Q-S5 (c) and the minimal fix of Q-S1; possibly Q-S4.
- **P37 — A required blank line is placed after directives and never directly in front of a directive.** Decides #1372 Q-B3, Q-B4 (SHARED-STMT-004 consistent).
- **P12 clarification (not new):** a rule that checks where a line starts reports the line when its first element, a comment included, is out of place. Decides #1362 Q2.

## 7. #912 tracking

Last comment on #912 is the round-8 bookkeeping part 2 (2026-10-21 17:29). **None of the 17 new question issues (#1362–#1376, #1378, #1379) is tracked on #912** — no sub-issue (limit reached) and no listing comment. The PR bodies of #1358 and #1359 say they are not attached, but no comment was posted. A listing comment is missing (and #1377 is not linked anywhere but PR #1357).

## 8. Other findings for the coordinator

- New issues carry no milestone (earlier follow-ups: `SDD`).
- Unfiled follow-ups from PR bodies: (a) `sdd/README.md` *Tests and requirements* / "Enforced today" stale after #939 (PR #1358); (b) formatter spec non-goals call #951, #953, #956, #958, #959, #960, #976 open although decided (PR #1358); (c) SHARED-FIX-003 "reports the construct it names for every input" vs SHARED-DIAG-008 / SHARED-JOIN-001 not-reported cases (PR #1359 auditor); (d) `AGENTS.md` pointer to the P11 section — maintainer option (PR #1360); (e) P11 section closing sentence must be narrowed in #1331 (PR #1360).
- #1377 is unblocked (#1138 closed); it could absorb the #1363 Q6 answer (FMT-PIPE-007 vs document EOL for SHARED-ATTR-008/-017).
- #1372 Q-B2 interacts with the already-decided #1081 item 1 (RH5025 reports the blank line above a standalone block) — the recommendation keeps #1081.
