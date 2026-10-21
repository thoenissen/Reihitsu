# Round 8 status (read-only inventory, 2026-10-21)

## 1. Batch issues

| Issue | State | PR | PR state | Open-question issues filed |
|---|---|---|---|---|
| #985 (RH8301–RH8501) | closed | #1301 Specify the documentation layout and comment rules RH8301–RH8501 | merged 2026-10-21 | #1306–#1317 (12) |
| #993 (RH7001–RH7110, modifier ordering) | closed | #1303 Specify RH7001–RH7004, RH7101, RH7102, RH7104–RH7110 and the shared modifier ordering | merged 2026-10-21 | #1318–#1328 (11) |
| #995 (RH7401–RH7412, RH7501) | closed | #1302 Add specs and covering tests for RH7401–RH7412 and RH7501 | merged 2026-10-21 | #1304, #1305 (2) |
| #939 (tag formatter/shared/CLI/Playground tests) | open | none found | – | – |
| #998 (requirement coverage opt-out) | open | none found | – | – |

## 2. Spec issues

| Issue | State | PR |
|---|---|---|
| #1138 SDD: fix scope and fix layout (P9, P10; extended P14a-d, P23, P24) | open | none |
| #1139 SDD: line-leading comments and comment kinds (P12, P13) | open | none |
| #1161 SDD: record P11 | open | none |
| #1260 SDD: record P15-P22, alias and Razor extensions | open | none |

## 3. Open `SDD open questions` issues (25, all created 2026-10-20)

Q = question count; D = items marked "Decided by principle" (approximate text count). None is a sub-issue of #912 (limit of 100 reached).
Listed on #912 by comment: #1304, #1305 (comment for #995) and #1306–#1317 (comment for #985). **#1318–#1328 (#993) are NOT mentioned on #912** (bodies say they could not be attached).

| Issue | Spec | Batch | Q | D |
|---|---|---|---|---|
| #1304 | shared/member-region-grouping.md | #995 | 6 (Q1–Q6) | 1 |
| #1305 | RH7501 | #995 | 4 (Q7–Q10) | 0 |
| #1306 | RH8301 | #985 | 2 | 1 |
| #1307 | RH8302 | #985 | 3 | 2 |
| #1308 | RH8303 | #985 | 2 | 1 |
| #1309 | RH8304 | #985 | 2 | 2 |
| #1310 | RH8305 | #985 | 7 | 6 |
| #1311 | RH8306 | #985 | 5 | 4 |
| #1312 | RH8307 | #985 | 6 | 5 |
| #1313 | RH8308 | #985 | 3 | 2 |
| #1314 | RH8309 | #985 | 9 | 7 |
| #1315 | RH8401 | #985 | 3 | 2 |
| #1316 | RH8402 | #985 | 4 | 1 |
| #1317 | RH8501 | #985 | 2 | 0 |
| #1318 | RH7004 | #993 | 4 (A1–A4) | 3 |
| #1319 | RH7101 | #993 | 3 (B1–B3) | 3 |
| #1320 | RH7002 | #993 | 1 (C1) | 1 |
| #1321 | RH7003 | #993 | 2 (N1, N4) | 0 |
| #1322 | RH7102 | #993 | 1 (N3) | 0 |
| #1323 | RH7109 | #993 | 1 (N3, same as #1322) | 0 |
| #1324 | RH7104 | #993 | 2 (D1–D2) | 1 |
| #1325 | shared/modifier-ordering.md | #993 | 4 (E1–E3, N2) | 3 |
| #1326 | RH7107 | #993 | 2 (F1–F2) | 2 |
| #1327 | RH7108 | #993 | 2 (G1–G2) | 2 |
| #1328 | RH7110 | #993 | 1 (P18 reach) | 0 |

Totals: #985 48 questions, #993 23 items, #995 10 questions — 81 in all.

## 4. Other open issues created on/after 2026-10-20 (excluding open-question issues and #1260–#1300)

- #1239 [FORMATTER BUG] A rewritten `# region` line keeps the whitespace between `#` and `region`
- #1240 SDD: The shared/diagnostics.md non-goal on inactive #if code also covers region directives, which region rules still report
- #1241 RH8107, RH8202, RH8204 fixes: an end tag with another name is no end tag (P19)
- #1242 [CODE REVIEW REPORT] Decide the spec owner of DocumentationCommentCodeFixUtilities (RH8107 and RH8202)
- #1243 [FORMATTER BUG] Joining an operator that stands alone on its line inserts a blank line in front of it
- #1244 [CODE REVIEW REPORT] Unreachable nested-element recursion in InitializerLayoutAnalysisUtilities.FindFirstComplexElementMisalignment
- #1257 [ANALYZER BUG] RH3001, RH3005, RH3101, RH3105 code fixes can turn comparisons into a generic invocation
- #1258 [ANALYZER BUG] RH3103 code fix throws InvalidCastException for a dotted Nullable inside nameof
- #1259 [CODE REVIEW REPORT] Share the empty-list removal of the RH3106 and RH3107 code fixes
- #1329 [FORMATTER BUG] A declaration after a documentation element without an end tag is turned into a comment (2026-10-21, new)
- #1330 Test the lines inside interpolated-raw and UTF-8 string literals in the shared non-formattable-line helper (2026-10-21, new)

(#1240, #1241, #1243, #1244, #1257, #1258, #1259 were extended in the round 7a comment on #912; #1239 and #1242 are not mentioned there; #1329 and #1330 are new since round 7.)

## 5. Open PRs

0 open pull requests. Total open issues: 200.
