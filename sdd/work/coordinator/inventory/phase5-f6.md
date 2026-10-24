# Phase 5 — family F6 (Documentation RH8xxx) inventory

27 issues. Checked against `main` at f9779e0. Blockers #1138, #1139, #1260 and #1331 are all closed (PRs #1357, #1361, #1359, #1404), so the SHARED-FIX-006..008, SHARED-COMMENT-001 (P13), SHARED-DOC-017 (P19), SHARED-DIAG-004/006/007 (P15/P20/P21), SHARED-DOC-004 (P29), SHARED-DOC-008 (P30) and SHARED-DIAG-010 (P31) wording these issues rely on is in the specs now. No builds or tests were run.

## 1. Table

| # | Title (short) | Labels | Milestone | Kind | Spec reqs / rules / phases | Blocked by | Principle | Duplicate/overlap | Template? | Proposed | Note |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 1053 | Rule pages RH8001–RH8018: say which declarations are private | enhancement | Maintenance | internal | SHARED-DOC-002; rule pages RH8001–RH8018 | #1092 (static ctor group changes) | — | #1092 | no | Maintenance | Docs only. Write after #1092 so pages don't describe retired behavior |
| 1055 | Documentation group of extension-block members | enhancement | Maintenance | internal | SHARED-DOC-002 | — | — | — | no | Maintenance | SHARED-DOC-002 already lists extension blocks; probably done, check and close |
| 1091 | Documented exactly when compiler doc has a summary | — | SDD | behavior change | SHARED-DOC-005/006/007/010/011/013/014–016, RH8020–RH8033, RH8101–RH8111, RH8201/8203/8305/8306/8309; RH8107-001 retirement | — | P8 (+P20) | #1153, #1356, #1132; F1 #1355 | no | vNext | Change doc. Comments add many instances. Partial part overlaps SHARED-DIAG-004 |
| 1092 | Group comes from symbol; static ctor always private | — | SDD | behavior change (Q4) + bug (Q3b) | SHARED-DOC-002, SHARED-DIAG-004, RH8001/8002, RH8017-001, RH8018-001 | — | P15 | #1053 | no | vNext | Q3b violates SHARED-DIAG-004 (partial = one declaration, accessibility from any part) |
| 1132 | Param-doc rules should match `name` the same way | enhancement | Maintenance | internal | SHARED-DOC-012/015; RH8032/8033/8101/8102/8108/8109 | — | P8b | #1153 (questions answered, cases moved there) | no | Maintenance | Only the helper-extraction refactor is left (no behavior change) |
| 1153 | P8b/P8c: params as compiler binds; rendered summary text; RH8030 | — | SDD | behavior change | SHARED-DOC-009/010/012/014, RH8030–RH8033, RH8101–RH8111, RH8202, RH8306 | — | P8b, P8c | #1091 (shared change doc), #1132 | no | vNext | Change doc. "RH8032 with no source location" part is a SHARED-DIAG-007 bug |
| 1241 | Fixes rewrite an element that has no end tag of its own | bug, analyzer | Triage | bug | SHARED-DOC-017, SHARED-FIX-003, RH8204-003; RH8305/8307/8309-004 | — | P19, P1 | #1242; F1 #1329 (formatter side) | yes (analyzer bug) | 1.2.0 | Violates SHARED-DOC-017 (RH8204) and SHARED-FIX-003. CRITICAL: fix comments out code |
| 1242 | Spec owner of DocumentationCommentCodeFixUtilities | enhancement | SDD | internal | RH8107-006/007, RH8202-003; also RH8204, RH8304, RH8308 | — | — | #1241, #1344, F4 #1067 | no | Maintenance | Spec-ownership decision. #1241 adds a third user (RH8204) |
| 1269 | RH8201/8202/8204 fixes change only the reported markup, one application | — | SDD | bug | SHARED-FIX-001/006/008, RH8201-005, RH8202-003, RH8204-003 | (#1138 closed) | P9, P7, P24 | #1342 (same cause) | no | bug-pool | RH8201 fix breaks SHARED-FIX-006/008; RH8202 breaks SHARED-FIX-001 |
| 1270 | RH8106, RH8203, RH8305, RH8306 check every element | — | SDD | bug | SHARED-DIAG-006, RH8106-001, RH8203-001, RH8201-001, RH8305-001, RH8306-001 | (#1260 closed) | P20 | #1293 (RH8106) | no | bug-pool | Violates SHARED-DIAG-006: result depends on element/declarator order |
| 1292 | RH8102/8103/8109/8110/8202 report on every documentable declaration | — | SDD | behavior change | scope reqs of RH8102/8103/8109/8110/8202 | (#1260 closed) | P11 own gap | #1349 | no | vNext | Change doc; new diagnostics |
| 1293 | RH8107 owns void members, RH8106 skips them | — | SDD | behavior change | RH8106-001, RH8107-001, SHARED-FIX-004 | — | — | #1270 | no | vNext | Title says "(behavior change)". Change doc |
| 1294 | RH8203 matches constructors the way the compiler resolves them | — | SDD | behavior change | RH8203-001 | — | P8 | #1270 | no | vNext | Title says "(bug)", but RH8203-001's coverage excludes ref/private base ctors |
| 1341 | `///` that documents nothing is an ordinary comment (RH8301–8309, RH8401) | — | SDD | behavior change | RH8301–RH8305/RH8307–RH8309-001, RH8401-001/002, SHARED-DOC-004 | (#1139 closed) | P13 | #1345 (same analyzers) | no | vNext | P13 is in SHARED-COMMENT-001 only, which RH83xx don't list under Uses → change doc |
| 1342 | Layout fixes format only the reported comment; behind-code `///` stays (RH8305/8307/8309/8402) | — | SDD | bug | SHARED-FIX-006/007, SHARED-DOC-004, RH8305/8307/8309-004, RH8402-005 | (#1138, #1331 closed) | P9, P14d, P29 | #1269, #1345 | no | bug-pool | Violates SHARED-FIX-006 (whole body reformatted, blank lines changed) and SHARED-DOC-004 |
| 1343 | Fixes finish in one application: RH8308, RH8309, RH8401, RH8305 | — | SDD | bug | SHARED-FIX-001/005, RH8308-003/004, RH8309-004, RH8401-004 | — | P7 (+P8) | — | no | bug-pool | Violates SHARED-FIX-001; RH8401-004 says every line; RH8305 at file start makes no progress |
| 1344 | RH8304/RH8309 fixes withheld when they break code or move comments | — | SDD | bug | SHARED-FIX-003, RH8304-004, RH8309-004 | — | P1 | #1242 | no | 1.2.0 | Violates SHARED-FIX-003. CRITICAL: CS0102 / unparsable output; relocates comment text |
| 1345 | RH8301–8303 judge behind-code `///` as if it started its line | — | SDD | bug | SHARED-DOC-004 (P29), RH8301/8302/8303-001, RH8303-004/005 | (#1331 closed) | P29 | #1341, #1342 | no | bug-pool | Violates SHARED-DOC-004 ¶2. RH8303-001/-004/-005 still contradict it (see Doubts) |
| 1346 | RH8401/RH8402 check in every documentation mode | — | SDD | bug | SHARED-DOC-008 ¶2, RH8401-001, RH8402-003 | (#1331 closed) | P30 | — | no | bug-pool | Violates SHARED-DOC-008 ¶2. Code confirmed: mode gate in RH8401 and RH8402 |
| 1347 | RH8307 skips `/** */` and inline formatting elements | — | SDD | bug | SHARED-DIAG-010, RH8307-001/004, FMT-DOC-009/012 | (#1331 closed) | P31 | F1 #961 | no | bug-pool | Violates SHARED-DIAG-010, whose own example is RH8307 on `/** */` |
| 1348 | RH8302 silent before `#region`/`#endregion` (stops undoing RH5031) | — | SDD | bug | SHARED-FIX-004, RH8302-001, RH5031-001; FMT-BLANK-010/013 | — | — | F4 (RH5031) | no | bug-pool | Violates SHARED-FIX-004: RH8302 and RH5031 fixes undo each other |
| 1349 | RH8306 checks indexer summaries | — | SDD | behavior change | RH8306-001 | — | P11 own gap | #1292 | no | vNext | Title says "(behavior change)". Coverage excludes indexers |
| 1350 | RH8501 reports `UnconditionalSuppressMessage` and `<Pending>` | — | SDD | behavior change | RH8501-001/002 | — | P11 own gap | — | no | vNext | Title says "(behavior change)". `<Pending>` is excluded by RH8501-002's coverage |
| 1351 | RH8308: text after an XML comment behind a closing tag | — | SDD | behavior change | RH8308-001 | — | P8c-like | #1343 (RH8308) | no | vNext | Title says "(behavior change)" |
| 1352 | RH8309: `/** */` order without a fix; custom element moves with the one before it | — | SDD | behavior change | RH8309-001/004 | — | — | #1343, #1344 (RH8309) | no | vNext | Title says "(behavior change)". Q7 contradicts today's RH8309-004 |
| 1353 | RH8402: fix replaces the whole comment run; header must end at a line break | — | SDD | clarification (Q1/Q2) + behavior change (Q3) | RH8402-003/004/005 | — | — | #1342 (blank line after the run) | no | vNext | Title says "(bug)", but RH8402-003's coverage leaves Q3 out → not a bug |
| 1356 | New rule: documentation tag name has the wrong case | — | SDD | behavior change | new RH8xxx rule spec | #1091 (P8, related) | P8 | #1091, F1 #1355 | no | vNext | New rule + code fix + change doc |

## 2. Wrong bug labels

- None. Only #1241 carries the `bug` label, and it is a real bug (SHARED-DOC-017, SHARED-FIX-003).
- Titles that say "(bug)" but describe no bug: #1294 (RH8203-001's coverage excludes constructors with ref/in/out/params differences or a private base constructor) and #1353 Q3 (RH8402-003's coverage requires the header to be followed by a line break or end of file).

## 3. Critical candidates

- #1241 — the RH8204 fix (and the RH8305/8307/8309 fixes from the round 8 comment) pulls the next declaration into the doc comment. The document stops parsing and the user's code becomes comment text.
- #1344 — the RH8304 fix copies `int A;` onto the new line (CS0102), or a second `{` (does not parse). The RH8309 fix moves free comment text and XML comments to a different place.
- #1348 (borderline) — the RH8302 and RH5031 fixes undo each other in a loop. Applying every fix never converges. This is not formatter non-idempotence.

## 4. Real bugs

- #1241 — SHARED-DOC-017, SHARED-FIX-003. `/// <placeholder>Later text⏎public void Run()` → the RH8204 fix gives `///     public void Run()` (does not parse) → the spec requires the diagnostic with no fix offered.
- #1269 — SHARED-FIX-006, SHARED-FIX-008 (RH8201), SHARED-FIX-001 (RH8202), RH8204-003 / SHARED-FIX-003 for content (RH8204). The RH8201 fix → today `#if` moves to column 0, members are re-indented, CRLF becomes LF → spec: only the comment becomes `<inheritdoc/>`. Two `<value>` → today two applications → spec: one. (The issue cites RH8204-002 for content, but -002 is the diagnostic location; content is -003.)
- #1270 — SHARED-DIAG-006. Two `<returns>`, non-empty one first → today not reported (reported in the opposite order) → spec: reported. `event EventHandler First, Fourth;` with `<inheritdoc/>` → `Fourth` today not reported → spec: reported.
- #1342 — SHARED-FIX-006, SHARED-DOC-004 ¶2. `int Z; /// <summary>D</summary>⏎int D;` → today the RH8305 fix moves the comment and inserts a blank line → spec: summary expanded in place. `// Copyright…⏎⏎namespace N;` → today the RH8402 fix removes the blank line → spec: the blank line stays.
- #1343 — SHARED-FIX-001, SHARED-FIX-005, RH8401-004. `/// <summary>C</summary> a &amp; b` → today one application leaves `&amp; b` → spec: removed in one application. A summary on the first line of the file → today RH8305 makes no progress → spec: widened in one step.
- #1344 — SHARED-FIX-003. `int A; /// <summary>S</summary><remarks>R</remarks>⏎int B;` → today `    int A; /// <remarks>R</remarks>` (CS0102) → spec: continuation prefix is indentation only, or no fix.
- #1345 — SHARED-DOC-004 ¶2 (P29). `int a; ///Behind⏎int b;` → today RH8301 is silent → spec: reported, fix gives `/// Behind` in place.
- #1346 — SHARED-DOC-008 ¶2 (P30). `/// workaround` in a body with `DocumentationMode.None` → today RH8401 is silent → spec: reported. Code confirmed: `RegisterSyntaxNodeActionWithDocumentationModeCheck` in RH8401, an explicit `DocumentationMode.None` return in RH8402.
- #1347 — SHARED-DIAG-010 (P31). `/** <summary>Total⏎ * sum</summary> */` → today RH8307 reports it (its analyzer registers on every `XmlElement`) and the fix is a no-op → spec: not reported.
- #1348 — SHARED-FIX-004. `/// <summary>Sixth</summary>⏎⏎#region R` → today the RH8302 fix removes the blank line and the RH5031 fix re-inserts it → spec: RH8302 does not report.
- Bug parts inside other issues: #1092 Q3b (SHARED-DIAG-004: `public partial class V` + `partial class V` → second part reported by RH8002 → should be the non-private group) and #1153 (RH8032 diagnostic without a source location, SHARED-DIAG-007).

## 5. Groups

- **G-P8 (behavior change, element-documentation shared spec; one change doc or two coordinated ones):** #1091, #1153, then #1356 (new rule, builds on P8). #1132's helper refactor can come first as a separate Maintenance PR, so P8b edits one helper instead of four copies. #1092 touches SHARED-DOC-002 in the same spec. #1053 (pages) follows #1092.
- **G-fix-scope (bug lane, code fixes):** #1269 and #1342 (same cause: the fix reformats more than its node, P9/P14d), #1343 (one application), #1344 and #1241 (withhold unsafe fixes). Decide #1242 (helper ownership) first or inside this run, because RH8204, RH8304, RH8308 and RH8309 touch `DocumentationCommentCodeFixUtilities`.
- **G-layout-judgment (bug lane, analyzers RH8301–RH8303/RH8307/RH8401/RH8402):** #1345, #1346, #1347, #1348. Sequence #1345 and #1341 (P13, behavior change) together or back to back: both change which `///` comments RH8301–RH8303 judge.
- **G-scope-widening (behavior change, analyzers):** #1292, #1349 (P11 own gap), #1350, #1351, #1352, #1293, #1294, #1353 Q3. Small, independent, one change doc each or one combined doc.
- **Duplicates:** none strict. #1132's questions are answered and its cases moved to #1153; only the refactor is left. #1055 looks resolved by SHARED-DOC-002, which already names extension blocks.

## 6. Doubts

- #1345, #1346, #1347: the issues say "behavior-change lane, change doc required", but PR #1404 has since written P29/P30/P31 into SHARED-DOC-004, SHARED-DOC-008 and SHARED-DIAG-010, so the code now differs from a spec. Treat them as bugs with no change doc?
- #1345: RH8303-001 ("starts its line"), RH8303-004 and RH8303-005 contradict SHARED-DOC-004 ¶2, which says RH8303 judges a behind-code comment and its fix moves the comment. The rule spec must be aligned in the same PR. Is that a clarification or a change doc?
- #1341: P13 is in SHARED-COMMENT-001, but the RH83xx specs do not use it, and their coverage sentences leave in-body `///` out. Confirm it is a behavior change (change doc), not a bug.
- #1092 Q3b and #1091 Q3a (partial types): SHARED-DIAG-004 already makes a partial declaration one declaration, while SHARED-DOC-002 and SHARED-DOC-005 leave multi-part partials out of their coverage. Is Q3b a bug against SHARED-DIAG-004, or part of the change doc?
- #1055: SHARED-DOC-002 already lists extension blocks. Close it as done, after checking that a covering test exists?
- #1132: keep it open only for the helper extraction, or close it and file the refactor as its own Maintenance issue?
- #1353: split it? Q1/Q2 is spec-only clarification; Q3 is a small behavior change despite "(bug)" in the title.
- #1241: one run, or split the RH8204 core (SHARED-DOC-017 names it) from RH8305/8307/8309? SHARED-DOC-017 names only RH8107, RH8202 and RH8204; for the other three the claim rests on SHARED-FIX-003.
- #1343 RH8308 span: RH8308-003 says "to the start of the next XML node". Whether `&amp;` counts as a node is ambiguous; the bug rests on SHARED-FIX-001 rather than on RH8308-003.
- Milestone "SDD" is on 22 of 27 issues; all need re-homing per the proposal column.
