# Inventory — round 8, family D4 (#985, PR #1301: documentation layout and comment rules RH8301–RH8501)

Issues #1306–#1317, read 2026-10-21 (all open, none has comments, none is a sub-issue of #912). 48 questions. All are `SDD open questions` issues; none skipped.
`⏎` = line break. Duplicates checked against the titles of all 200 open issues (list_issues, both pages) plus bodies of #962, #959, #1077, #1091 (+comments), #1139 (+comment), #1150 (+comments), #1153, #1269, #1329.

Principles used: P1 unsafe fix (SHARED-FIX-003 / AGENTS.md Code-fix coverage), P7 converge / no undo / no no-op (SHARED-FIX-001/004/005), P8 documented = compiler's documentation (tag names lower case, namespace-free, merged comments), P8c only content that renders text counts, P9 fix changes only its diagnostic's code, P11 formatter may do more / own gap, P13 a `///` that documents nothing is an ordinary comment in every documentation mode, P14d fix formats only the replaced node, P15 partial = one declaration, P19 end tag with another name = no end tag, P20 every element of its kind.

| Issue | Q | Rule | Input → real output | Why it looks wrong | Req IDs | Principle | Duplicates / related | Kind (recommendation) | Fully decided? |
|---|---|---|---|---|---|---|---|---|---|
| #1306 | Q1 | RH8301 | `///InBody` alone in a method body → reported, fix `/// InBody` | Documents nothing → ordinary comment | RH8301-001 | P13 (marked; fits) | #1139 (P13 spec), #962 | Bug under P13 | Yes → new issue G1 (P13 doc layout rules) |
| #1306 | Q2 | RH8301 | `int a; ///Behind⏎int b;` → not reported; formatter moves it (FMT-DOC-001) and writes `/// Behind` (FMT-DOC-004) | The `///` documents `b` (SHARED-DOC-004); code without diagnostic changes on format | RH8301-001, FMT-DOC-001/004 | P11 own gap? (issue: open) → new pattern "doc comment behind code" (P25 cand.) | = #1307 Q3, #1308 Q2 (same shape); #962 (non-documenting variant) | Behavior change (a): RH8301 judges every line of a documentation comment, also one that starts behind code | No → P25 candidate |
| #1307 | Q1 | RH8302 | `/// inbody⏎⏎int a = 0;` in a body; `/// stranded⏎⏎}` → reported, fix removes blank line | Neither documents anything | RH8302-001 | P13 (marked; fits) | #1139 | Bug under P13 | Yes → G1 |
| #1307 | Q2 | RH8302 ↔ RH5031 | `/// <summary>Sixth</summary>⏎⏎#region R⏎⏎int F;` → RH8302 fix removes blank; RH5031 then reports `#region R`, its fix re-inserts it. Formatter keeps an existing blank, inserts none | Two fixes undo each other | RH8302-001, RH5031-001, FMT-BLANK-010/013, SHARED-FIX-004 | Marked P7 — **flag:** P7/SHARED-FIX-004 forbids the ping-pong but does not say which rule yields; RH8302-001's own coverage sentence already leaves "blank lines followed by `#region`" out | #1076 (blank-line fixes never undo; different rules) | Clarification/bug: one rule yields. Recommend RH8302 does not report a blank line followed by `#region`/`#endregion` (RH5031 owns the line above a directive) | No — needs "which rule yields" |
| #1307 | Q3 | RH8302 | `int Z; /// <summary>D</summary>⏎⏎int D;` → not reported; format.sh moves the comment and removes the blank line | Code without diagnostic changes on format | RH8302-001, FMT-DOC-001, FMT-BLANK-010 | new pattern "doc comment behind code" (P25 cand.) | = #1306 Q2, #1308 Q2 | Behavior change (a) report | No → P25 candidate |
| #1308 | Q1 | RH8303 | `int a = 0;⏎/// inbody⏎a++;`, `case 1:⏎/// incase⏎break;`, `N(1,⏎/// arg⏎2);`, `[Obsolete]⏎/// <summary>G</summary>⏎int G;` → reported, fix inserts blank line | None documents a declaration (after attribute: SHARED-DOC-004) | RH8303-001 | P13 (marked; fits — the attribute case is "documents nothing" by SHARED-DOC-004) | #1148 (FMT-BLANK-011 scope, case-label shape), #1139; test `VerifyDocumentationAfterSwitchLabelProducesDiagnostic` changes | Bug under P13 | Yes → G1 |
| #1308 | Q2 | RH8303 | `int A; /// <summary>⏎    /// B</summary>⏎    int B;` → reported on the 2nd line's `///`; fix inserts a blank line inside the comment (two doc comments; RH8302 cannot see it) | Diagnostic on the wrong line; fix splits the comment | RH8303-001/004/005 | new pattern "doc comment behind code" (P25 cand.); fix part P1/P9 | = #1306 Q2, #1307 Q3 | Bug + behavior change: (b) report on the first line, fix moves the comment off the code line as FMT-DOC-001 does | No → P25 candidate |
| #1309 | Q1 | RH8304 | `/// <a/><b/>` in a method body → reported, fix splits | Documents nothing | RH8304-001 | P13 (marked; fits) | #1139 | Bug under P13 | Yes → G1 |
| #1309 | Q2 | RH8304 | `int A; /// <summary>S</summary><remarks>R</remarks>⏎int B;` → `int A; /// <summary>S</summary>⏎    int A; /// <remarks>R</remarks>` (CS0102); `{ /// …` after `class C⏎` → second `{`, does not parse | Fix copies everything before `///` (`GetContinuationPrefix`), not only indentation | RH8304-004 | P1 (marked; fits — compile error) | #1242 (owner of `DocumentationCommentCodeFixUtilities`), behind-code group (P25 cand.) | Bug: continuation prefix = indentation only, or withhold | Yes → new issue G4 (P1 doc fixes) |
| #1310 | Q1 | RH8305 + FMT-DOC-008 | `<x:summary xmlns:x="urn:x">`, `<summary xmlns="urn:x">` → reported and expanded; `<Summary>` → not reported, formatter expands (case-insensitive) | P8: tag names case-sensitive, namespace-free | RH8305-001, FMT-DOC-008 | Issue: open (P8 analyzer, P4 formatter). **Verified:** analyzer side is P8 (namespaced forms not a summary → not reported; `<Summary>` not reported, stays); formatter side is P11 (formatter may do more) — no reason to narrow FMT-DOC-008 | #1091 | Behavior change under P8 (analyzer only); formatter unchanged | Yes (P8 + P11) → comment on #1091; confirm "formatter unchanged" in round |
| #1310 | Q2 | RH8305 | 3-line `<summary>J1</summary>` then `/// <summary>J2</summary>` in one comment → second not reported | Every element checked | RH8305-001 | P20 (marked; fits) | #1270 (P20 RH8106/RH8203) | Bug/behavior change under P20 | Yes → extend #1270 |
| #1310 | Q3 | RH8305 fix | `/// <summary>A</summary>` on `void M(){int a=1;⏎a++;}` reformats body, rewrites `///Text`, removes blank between doc comments; `int Z; /// <summary>D</summary>⏎int D;` moves comment, inserts blank | Fix changes code outside its diagnostic | RH8305-004 | P9 (marked; fits; also P14d) | #1269 (doc fixes P9, RH8201/8202/8204), #1144 | Bug | Yes → new issue G2 (P9/P14d doc layout fixes) |
| #1310 | Q4 | RH8305 fix | `/// <summary>A</summary>⏎int A;` + `void N( }` elsewhere → fix offered, no-op | No-op fix | RH8305-004, SHARED-FIX-005 | P7 (marked; fits) | #1150 — already recorded there (comment 2026-10-20) | Bug | Yes — no action (already on #1150) |
| #1310 | Q5 | RH8305 | `/// <summary>K</summary>` in a method body → reported, expanded | Documents nothing | RH8305-001 | P13 (marked; fits) | #1139 | Bug | Yes → G1 |
| #1310 | Q6 | RH8305 | `/// <summary>S⏎int A;` (no end tag) → reported; fix `/// <summary>⏎/// S⏎/// int A;` (field becomes comment). `<summary>S</returns>` → fix keeps `</returns>` | Fix destroys a declaration | RH8305-001/004, SHARED-DOC-017 (#1260) | P1 + P19 (marked; fit) | #1241 (P19 fixes RH8107/8202/8204), #1329 (formatter does the same) | Bug: withhold fix, keep diagnostic | Yes → extend #1241 (add RH8305/RH8307/RH8309); #1329 is the formatter side |
| #1310 | Q7 | RH8305 fix + formatter | `/// <summary></summary>` or `<summary> </summary>` → `<summary>⏎/// ⏎/// </summary>` (trailing space) | FMT-DOC-011 writes empty interior line as bare `///` | RH8305-004, FMT-DOC-008/011 | none, but **already decided** on #923 Q1/#915 Q1 → #959 ("empty summary expanded with bare `///`") | **Duplicate of #959** (its example is exactly this input) | Behavior change (formatter), reading (a) with bare `///` | Yes (earlier decision) → note on #959 that the RH8305 fix inherits it |
| #1311 | Q1 | RH8306 | `<Summary>Gets the value</Summary>`, `<summary xmlns="u">Gets value</summary>` → reported | Not a summary under P8 | RH8306-001 | P8 (marked; fits) | #1091 | Behavior change under P8 | Yes → comment on #1091 |
| #1311 | Q2 | RH8306 | summary in a later comment after `<remarks>` + blank line; second top-level summary → not reported | Compiler joins comments; every element | RH8306-001 | P8 + P20 (marked; fit) | #1091 (later comment), #1270 (P20) | Behavior change | Yes → #1091 + extend #1270 |
| #1311 | Q3 | RH8306 | `<summary><para>Gets value</para></summary>`, `Returns&#32;value`, `Value&#46;` → not reported; `Gets<!-- c -->value` reported (comment read as space) | Rendered text is a sentence/verb | RH8306-001/002/003 | P8c (marked). **Flag:** P8c is worded for "does the summary have text"; using the rendered text for RH8306's first-word/last-char check is an extension. Consequence for `Gets<!-- c -->value`: comment is no text → `Getsvalue` → not reported (opposite of today) | #1153 (P8b/P8c) | Behavior change | Yes for the three inputs; the `<!-- -->` consequence to confirm → comment on #1153 |
| #1311 | Q4 | RH8306 | `/// <summary>Value.</summary>` on `public int this[int i] => 0;` → not reported | Indexer is a property symbol (`IsIndexer`) | RH8306-001 | P11 classification (own gap vs other construct) | #1292 (doc rules on every documentable declaration, P11) — closest precedent; RH8202 registers properties+indexers together | Behavior change (a) own gap | No — P11 classification |
| #1311 | Q5 | RH8306 | `partial int P { get; }` + `partial int P => 0;`, both documented → two diagnostics | One declaration | RH8306-001/004 | P15 (marked; fits) | #1287 (P15: RH2002, RH2004, RH2101) | Bug under P15 | Yes → extend #1287 |
| #1312 | Q1 | RH8307 fix | `/** <summary>F⏎ * more</summary> */` → no-op; `/// <param name="v">The value⏎/// </param>` + `void N( }` → no-op | No-op fix | RH8307-004, SHARED-FIX-005 | P7 (marked). **Flag:** input 2 is #1150's cause (formatter refuses syntax error); input 1 is not — the formatter leaves `/** */` alone by design (FMT-DOC-012); it disappears if Q4 (a) | #1150 (both already noted there) | Bug | Partly: input 2 yes (#1150); input 1 follows Q4 |
| #1312 | Q2 | RH8307 fix | param fix on `void M(int v){int a=1;}` reformats body; `int Z; /// <remarks>R⏎/// more</remarks>⏎int D;` moves comment, inserts blank | Fix scope | RH8307-004 | P9 (marked; fits; P14d) | #1269, #1144 | Bug | Yes → G2 |
| #1312 | Q3 | RH8307 | `/// <remarks>In⏎/// body</remarks>` in a method body → reported, rebuilt | Documents nothing | RH8307-001 | P13 (marked; fits) | #1139 | Bug | Yes → G1 |
| #1312 | Q4 | RH8307 | `/** <summary>Total⏎ * sum</summary> */` on `int D;` → reported; formatter leaves `/** */` alone (FMT-DOC-012), RH8304/8305/8308/8309 skip `/** */` | Formatted code stays reported | RH8307-001, FMT-DOC-012 | P4 (no principle); new pattern "analyzer reports a layout the formatter deliberately keeps" (P27 cand.) | #1312 Q1 input 1; #1314 Q2 (order, not layout) | Behavior change (a): RH8307 skips `/** */` like its siblings | No |
| #1312 | Q5 | RH8307 | `/// <remarks>Text⏎/// more⏎int A;` (no end tag) → fix `/// int A;`; `…</returns>` keeps `</returns>` | Fix destroys a declaration | RH8307-001/004 | P1 + P19 (marked; fit) | #1241, #1329 | Bug | Yes → extend #1241 |
| #1312 | Q6 | RH8307 | `/// Text <b>bold⏎/// still bold</b>` in `<remarks>` → reported; fix `Text <b>⏎/// bold⏎/// still bold⏎/// </b>` | #961 (decided #915 Q3: rebuild only block-level elements) calls this formatter output a bug; FMT-DOC-009 leaves inline elements open | RH8307-001/004, FMT-DOC-009 | P4 (issue); decided formatter side (#915 Q3/#961); analyzer side open; P27 cand. | #961 | Behavior change (a): RH8307 does not report inline formatting elements (else, after #961, the fix becomes a no-op → P7) | No (strongly leaning (a)) |
| #1313 | Q1 | RH8308 | `/// <summary>C</summary> a &amp; b` → span `a ` only; fix leaves `&amp; b`, reported again | One application must correct | RH8308-001/003/004, SHARED-FIX-001 | P7 (marked; fits) | none (not #1076, which is blank lines) | Bug (span to end of line) | Yes → new issue G3 (P7 doc fixes converge) |
| #1313 | Q2 | RH8308 | `/// <summary>E</summary><!-- c --> text` → not reported | XML comment renders nothing (P8c spirit) | RH8308-001 | none (P8c by analogy only) | — | Behavior change (a): an XML comment does not end the "after a closing tag" position; report ` text` | No (recommend (a)) |
| #1313 | Q3 | RH8308 | `/// <a/> body` in a method body → reported, fix removes `body` | Documents nothing; fix deletes comment text | RH8308-001 | P13 (marked; fits) | #1139 | Bug | Yes → G1 |
| #1314 | Q1 | RH8309 | `<Remarks>`, `<x:remarks xmlns:x="u">`, `<remarks xmlns="u">` before `<summary>` → ranked as remarks, reported, sorted | P8 tag names | RH8309-001 | P8 (marked; fits) | #1091 | Behavior change | Yes → #1091 |
| #1314 | Q2 | RH8309 | `/** <remarks>R</remarks>⏎ * <summary>E</summary> */` → not reported | Order is not layout; FMT-DOC-012 no reason | RH8309-001 | none | #1312 Q4 (opposite direction: layout) | Behavior change: report; fix only if it can sort inside `/** */` safely, else withheld | No |
| #1314 | Q3 | RH8309 | `<remarks>R</remarks>`, blank, `<summary>H</summary>` → not reported | Compiler joins comments | RH8309-001 | P8 (marked; fits) | #1091 | Behavior change | Yes → #1091 |
| #1314 | Q4 | RH8309 fix | (1) out of order in the 2nd comment → fix edits the 1st, needs 2 applications; (2) in method body → no-op; (3) after `[Obsolete]` → edits comment before attribute, converts body, expands summaries, blank line, diagnostic stays; (4) on accessor → edits property's comment, stays | Not corrected in one application | RH8309-004, SHARED-FIX-001/005 | Marked P7. **Flag:** only (1) is a pure P7 instance (with P8: both comments are one documentation). (2)–(4) are comments that document nothing (method body, after attribute list per SHARED-DOC-004, accessor) → not reported at all under P13 (= Q8); (3) also P9 | G1 (P13), G2 (P9) | Bug | Yes (P7+P8 for (1); P13 for (2)–(4)) → G3 + G1 |
| #1314 | Q5 | RH8309 fix | `int A(int a) => a;` → block body; `void I<T>() { }` expanded; behind-code comment moved + blank | Fix scope | RH8309-004 | P9 (marked; fits; P14d) | #1269, #1144 | Bug | Yes → G2 |
| #1314 | Q6 | RH8309 fix | `<remarks>R</remarks>⏎/// Text between⏎/// <summary>S</summary>` (or `<!-- c -->`) → elements move around the text, which ends up after `<summary>` | Fix relocates user-authored text/comment | RH8309-004 | P1 (marked via AGENTS.md "reorder or relocate user-authored comments"; fits) | #1136 (SHARED-FIX-003 relocation clause, spec) | Bug: withhold the fix when free text or an XML comment stands between top-level elements | Yes → G4 |
| #1314 | Q7 | RH8309 fix | `<summary>`,`<remarks>`,`<custom>`,`<param b>`,`<param a>`,`<returns>` → `<summary>`,`<param b>`,`<custom>`,`<param a>`,`<returns>`,`<remarks>` | Custom element splits the `<param>` run | RH8309-004 | none | — | Clarification/bug: (a) keep in place (today) / (b) move with preceding element / (c) withhold fix when a custom element would split a group | No |
| #1314 | Q8 | RH8309 | out-of-order elements in a method body, on an accessor, after an attribute list → reported | Document nothing | RH8309-001 | P13 (marked; fits) | #1139 | Bug | Yes → G1 |
| #1314 | Q9 | RH8309 | `/// <returns>R</returns>⏎/// <summary>S⏎int A() => 0;` → fix result does not parse | Fix breaks code | RH8309-001/004 | P1 (marked; fits; + P19 shape) | #1241, #1329 | Bug: withhold | Yes → extend #1241 |
| #1315 | Q1 | RH8401 | `/// <summary>S</summary>` alone in a method body → not reported (holds XML) | Under P13 it is an ordinary `///` comment — exactly RH8401's subject | RH8401-002 | P13 (marked; fits) | #1139 | Behavior change/bug under P13 | Yes → G1 |
| #1315 | Q2 | RH8401 | `/// workaround` in a method body, DocumentationMode `None` → not reported (rule off in None) | P13 says "in every documentation mode"; SHARED-DOC-008 says documentation rules report nothing in None | RH8401-001, SHARED-DOC-008 | P13 vs SHARED-DOC-008 conflict → P26 cand. | = #1316 Q4 (same gate); test `VerifyNoDiagnosticsWhenDocumentationModeIsNone` | Behavior change (a): report in every mode | No → P26 candidate |
| #1315 | Q3 | RH8401 fix | `/// first` + line with U+FEFF before `/// second` → first application converts line 1 only | SHARED-FIX-001; rule page claims manual correction but fix is offered | RH8401-004, SHARED-FIX-001 | P7 (marked; fits) | — | Bug (convert all lines or withhold); fix `documentation/rules/RH8401.md` | Yes → G3 |
| #1316 | Q1 | RH8402 fix | `// Copyright (c) Old Company.⏎namespace N;` → replaced by configured header; `// copyright note⏎#if DEBUG …` → note deleted | Diagnostic is an empty span at file start, so SHARED-FIX-003's "own report" exception does not cover the deleted comment | RH8402-004/005 | P1/SHARED-FIX-003 scope (decides "today is unsafe", not which option) | tests `MismatchedConfiguredHeader`, `MismatchedBlockCommentHeader` | (a) diagnostic covers the old header, fix replaces it / (b) insert only | No |
| #1316 | Q2 | RH8402 fix | `// Copyright (c) Old Corp.⏎// File note⏎⏎namespace N;` → header + `namespace N;` (note and blank deleted); `// Copyright…⏎⏎namespace N;` loses blank | Deletes comment and blank line | RH8402-005 | Marked P1 + P9. **Flag:** the deletion part depends on Q1 — under Q1 (a) the old header block becomes reported text and its replacement is the correction. The blank-line part is P9 regardless | Q1 | Bug | Partly: blank line yes (G2); deletion follows Q1 |
| #1316 | Q3 | RH8402 | exact header but last line `// </copyright> extra` → not reported (prefix compare) | Header text differs | RH8402-003 | none | — | Bug/clarification (a): header must be followed by a line break or end of file | No (recommend (a)) |
| #1316 | Q4 | RH8402 | no header, DocumentationMode `None` → not reported | Header has nothing to do with documentation parsing | RH8402-003 | none → P26 cand. | = #1315 Q2 | Behavior change (a): check in every mode | No → P26 candidate |
| #1317 | Q1 | RH8501 | `[UnconditionalSuppressMessage("C", "CA1")]` → not reported | Same kind of suppression, has `Justification` | RH8501-001 | P11 classification | — | Behavior change (a) own gap | No |
| #1317 | Q2 | RH8501 | `Justification = "<Pending>"` (IDE placeholder) → not reported | Placeholder is no justification | RH8501-002 | none | — | Behavior change (a) report | No (recommend (a)) |

## Groups by shared cause

1. **P13 — `///` that documents nothing** (method body, case label, argument list, after attribute list, before `}`, accessor): #1306 Q1, #1307 Q1, #1308 Q1, #1309 Q1, #1310 Q5, #1312 Q3, #1313 Q3, #1314 Q8 (+ Q4 parts 2–4), #1315 Q1 (inverse: RH8401 must report). One issue G1.
2. **Documentation comment written behind code** (`int a; /// …⏎int b;`): #1306 Q2, #1307 Q3, #1308 Q2 (open, P25 candidate); fix-scope side already P9 in #1310 Q3, #1312 Q2, #1314 Q5; P1 side #1309 Q2.
3. **P9/P14d — formatter-delegating doc fixes format the whole member**: #1310 Q3, #1312 Q2, #1314 Q5, #1316 Q2 (blank line). One issue G2.
4. **P7 — fix does not converge**: #1313 Q1, #1314 Q4 (1), #1315 Q3. One issue G3. (No-op on syntax error: #1310 Q4, #1312 Q1 → #1150, already recorded.)
5. **P1/P19 — element without own end tag, fix swallows the declaration**: #1310 Q6, #1312 Q5, #1314 Q9 → extend #1241; formatter side #1329.
6. **P1 — fix breaks code or moves comment text**: #1309 Q2, #1314 Q6 → G4 (#1316 Q2 deletion after Q1).
7. **P8 / P8c / P20 / P15**: #1310 Q1, #1311 Q1–Q3, #1314 Q1, Q3, Q4(1) → #1091 / #1153 / #1270 / #1287.
8. **Documentation-mode gate on rules that do not read documentation**: #1315 Q2, #1316 Q4 (P26 candidate).
9. **Analyzer reports a shape the formatter deliberately keeps**: #1312 Q4 (`/** */`), #1312 Q6 (inline `<b>`), #1312 Q1 input 1 (P27 candidate).
10. **P11 classification**: #1311 Q4 (indexers), #1317 Q1 (`UnconditionalSuppressMessage`).
11. **Singles**: #1307 Q2 (RH8302 vs RH5031), #1313 Q2, #1314 Q2, #1314 Q7, #1316 Q1, Q3, #1317 Q2.

## Decided by principle → proposed follow-up action

- **G1 (new issue)** "Documentation layout rules treat a `///` that documents nothing as an ordinary comment: RH8301–RH8305, RH8307–RH8309 stop reporting it, RH8401 reports it (P13)" — #1306 Q1, #1307 Q1, #1308 Q1, #1309 Q1, #1310 Q5, #1312 Q3, #1313 Q3, #1314 Q8 + Q4(2–4), #1315 Q1. Note on #1139 (P13 spec) and #1148 (case-label shape). Blocked by #1139.
- **G2 (new issue)** "Documentation layout fixes format only the reported comment: RH8305, RH8307, RH8309, RH8402 blank line (P9, P14d)" — #1310 Q3, #1312 Q2, #1314 Q5, #1316 Q2 (blank-line part). Related #1269, #1144. Blocked by #1138.
- **G3 (new issue)** "Documentation fixes finish in one application: RH8308 span, RH8309 joined comments, RH8401 U+FEFF lines (P7)" — #1313 Q1, #1314 Q4(1), #1315 Q3 (+ correct `documentation/rules/RH8401.md`).
- **G4 (new issue)** "RH8304 and RH8309 fixes are withheld when they would break code or move comment text (P1)" — #1309 Q2 (continuation prefix; relate #1242), #1314 Q6. Add #1316 Q2 deletion if Q1 is answered (b).
- **Extend #1241** (retitle "RH8107, RH8202, RH8204, RH8305, RH8307, RH8309 fixes: an element without its own end tag is not rewritten (P19, P1)") — #1310 Q6, #1312 Q5, #1314 Q9; cross-link #1329.
- **Comment on #1091 (P8)** — #1310 Q1 (analyzer side; formatter unchanged per P11), #1311 Q1, #1311 Q2 (later comment), #1314 Q1, #1314 Q3, #1314 Q4(1) (joined comments).
- **Comment on #1153 (P8c)** — #1311 Q3, incl. consequence `Gets<!-- c -->value` → not reported.
- **Extend #1270 (P20)** (retitle "RH8106, RH8203, RH8305, RH8306 check every element of their kind") — #1310 Q2, #1311 Q2 (second summary).
- **Extend #1287 (P15)** — #1311 Q5 (RH8306).
- **#1150** — #1310 Q4, #1312 Q1 input 2: already recorded there; no action.
- **Note on #959** — #1310 Q7 is its own example input (the RH8305 fix inherits it); close Q7 as duplicate.

## Needs maintainer decision

1. **Documentation comment behind code (#1306 Q2, #1307 Q3, #1308 Q2)** — P25 candidate.
   ```csharp
   class C
   {
       int A; /// <summary>
       /// B</summary>

       int B;
   }
   ```
   Today: RH8301/RH8302 silent (no line starts with `///`); RH8303 reports the 2nd line and its fix inserts a blank line inside the comment; the formatter moves the comment onto its own line above `B`, adds a blank line above it and removes the one below.
   Options: (a) the comment is a documentation comment of `B` for every layout rule (judged as if it started its line); RH8303 reports it on its first `///`, its fix moves it off the code line as FMT-DOC-001 does; other fixes leave it where it is (P9). (b) layout rules judge only comments that start their line; the formatter alone moves it (P11). (c) as (a) without a fix on RH8303.
   Recommendation: (a).
2. **RH8302 vs RH5031 (#1307 Q2).**
   ```csharp
   /// <summary>Sixth</summary>

   #region R

   int F;
   ```
   Today: RH8302 reports the blank line; after its fix RH5031 reports `#region R` and re-inserts it. Options: (a) RH8302 does not report a blank line followed by `#region`/`#endregion` (matches RH8302-001's own coverage gap); (b) RH5031 does not report a region directly below a documentation comment; (c) both report, both fixes withheld in this shape. Recommendation: (a).
3. **Documentation-mode gate (#1315 Q2, #1316 Q4)** — P26 candidate. `void M() { /// workaround⏎ }` with DocumentationMode `None`; file without header in `None`. Today: neither RH8401 nor RH8402 reports. Options: (a) SHARED-DOC-008 applies only to rules that read documentation elements; RH8401 and RH8402 check in every mode; (b) keep the gate (RH0002 already reports `None`). Recommendation: (a) — P13 already says "in every documentation mode".
4. **RH8307 on shapes the formatter deliberately keeps (#1312 Q4, Q6, Q1 input 1)** — P27 candidate.
   ```csharp
   /** <summary>Total
    * sum</summary> */
   int D;
   /// <remarks>Text <b>bold
   /// still bold</b></remarks>
   int E;
   ```
   Today: both reported; fix no-op on `D`, expands `<b>` on `E` (#961 calls that a formatter bug). Options: (a) RH8307 skips `/** */` and inline formatting elements; (b) report without a fix; (c) the formatter learns both layouts. Recommendation: (a).
5. **Indexers in RH8306 (#1311 Q4)** — `/// <summary>Value.</summary>⏎public int this[int i] => 0;` → not reported. (a) own gap (indexer is a property symbol; RH8202 already treats them together); (b) separate rule. Recommendation: (a).
6. **`UnconditionalSuppressMessage` (#1317 Q1)** — `[UnconditionalSuppressMessage("C", "CA1")]` → not reported. (a) own gap; (b) separate rule. Recommendation: (a).
7. **`<Pending>` justification (#1317 Q2)** — `[SuppressMessage("C", "CA1", Justification = "<Pending>")]` → not reported. (a) report; (b) any text counts. Recommendation: (a).
8. **XML comment after closing tag (#1313 Q2)** — `/// <summary>E</summary><!-- c --> text` → not reported. (a) report ` text` (XML comment renders nothing, P8c spirit); (b) the XML comment ends the position. Recommendation: (a).
9. **RH8309 on `/** */` (#1314 Q2)** — `/** <remarks>R</remarks>⏎ * <summary>E</summary> */⏎int A;` → not reported. (a) report with fix; (b) report, fix withheld for `/** */`; (c) skip. Recommendation: (b) (order is not layout; no canonical `/** */` rewrite exists).
10. **RH8309 custom elements (#1314 Q7)** — `<summary>S</summary><remarks>R</remarks><custom>X</custom><param name="b">B</param><param name="a">A</param><returns>R</returns>` (one per line) → fix: summary, param b, custom, param a, returns, remarks. (a) keep custom in place; (b) custom moves with the element before it; (c) withhold fix when a custom element would split a group. Recommendation: (b).
11. **RH8402 replacing an old header (#1316 Q1, decides #1316 Q2 deletion)** — `// Copyright (c) Old Company.⏎// File note⏎⏎namespace N;`. Today: whole comment run replaced (File note lost). (a) the diagnostic covers the leading comment run that mentions "copyright" and the fix replaces exactly that run; (b) the fix only inserts the header and never deletes a comment. Recommendation: (a) (keeps the header-migration use case; the replaced text is then reported text, SHARED-FIX-003-conform).
12. **RH8402 prefix match (#1316 Q3)** — header followed by ` extra` on its last line → not reported. (a) require a line break or end of file after the header; (b) prefix. Recommendation: (a).
13. **Confirm only:** #1310 Q1 formatter stays case-insensitive (P11); #1311 Q3 `Gets<!-- c -->value` → not reported (P8c applied to RH8306's word reading).

## Principle candidates

- **P25 — a documentation comment written behind code** belongs to the declaration that follows it (SHARED-DOC-004) and is judged by every documentation layout rule as if it started its line; RH8303 reports that it shares a line with code, and only RH8303's fix moves it (other fixes leave it, P9). Instances: #1306 Q2, #1307 Q3, #1308 Q2; consistent with the P9 parts of #1310 Q3, #1312 Q2, #1314 Q5.
- **P26 — the documentation-mode gate (SHARED-DOC-008) applies only to rules that read documentation elements**; rules about ordinary comments or file text check in every mode. Instances: #1315 Q2, #1316 Q4.
- **P27 — an analyzer layout rule whose fix delegates to the formatter does not report a shape the formatter's spec deliberately leaves alone** (the mirror of P11). Instances: #1312 Q4, Q6, Q1 input 1. Not for non-layout rules (#1314 Q2 order).

## Claims checked against principle wording

- #1307 Q2 "P7": P7 forbids the undo but does not choose the yielding rule → not fully decided.
- #1312 Q1 "P7, #1150": input 1 (`/** */`) is not #1150's cause (FMT-DOC-012, not a refused input) → depends on Q4.
- #1314 Q4 "P7": parts 2–4 are P13 (comments that document nothing), part 1 is P7 + P8.
- #1316 Q2 "P1": the deletion is unsafe only while the diagnostic does not cover the old header → depends on Q1.
- #1311 Q3 "P8c": fits for the listed inputs; applying it to word analysis is an extension worth confirming.
- #1310 Q1 marked open: P8 (analyzer) + P11 (formatter) settle it.
- #1310 Q7 marked open: already decided on #923 Q1 / #915 Q1 and tracked by #959.
