# Phase 5b status — 2026-11-07 (after #1632; origin/main = d979713)

Source: public REST API + PR files API, read-only. Nothing built or run. R7 = #1599 (inferred: its issue set matches R7 in phase5-runs.md).

## 1. PRs since 2026-11-06T00:00Z (dependabot: 8 — 6 merged, #1621/#1623 closed unmerged)
| PR | Title (short) | State | Closes / Refs |
|---|---|---|---|
| #1481 (R4) | Withhold code fixes that would break code, move comments/directives, or change behavior in another build | merged 11-06 12:15 | Closes #1136 #1134 #1135 #1262 #1332 #1336 #1340 |
| #1596 (R5) | Casing fixes: withhold renames that break the build, a native binding, excluded code, record positional members | merged 11-06 16:48 | Closes #1070 #1073 |
| #1599 (R7) | Withhold unsafe fixes / stop reporting unwritable forms: RH3001, RH3004, RH3005, RH3101, RH3103–07, RH3201, RH5417 | merged 11-07 06:59 | Closes #1257 #1279 #1281 #1282 #1417 #1422 |
| #1608 | Squad: continue-or-fresh table, lead survives compaction | merged 11-07 08:14 | — |
| #1609 (R6) | Safe RH2001/RH2004/RH2005 fixes, one report per partial member, no RH2004 on explicit interface operators | merged 11-07 13:09 | Closes #967 #1286 #1287 #1288 |
| #1610 | RH5202 fix keeps nested strings/comments in raw-string holes, shares formatter's line-shift rule | merged 11-07 11:34 | Closes #1458 |
| #1614 | Squad PRs describe the change, not the run | merged 11-07 12:09 | — |
| #1618 (R8) | Blank-line and brace fixes keep literal and comment text as written | merged 11-07 15:52 | Closes #1075 #1261 |
| #1632 | Lay out wrapped generic constraint clauses one constraint per line, aligned | merged 11-07 19:45 | Closes #1631 (no milestone; behavior change with change doc `sdd/changes/2026-11/constraint-clause-wrap.md`; touches FMT-BREAK/FMT-INDENT/FMT-SPACE specs, RH5110 + RH6021 specs) |
No open non-dependabot PRs. Milestone 1.2.0: 0 open / 67 closed.

## 2. Milestones
- **1.2.1 (#12): 43 open** — 946 952 963 969 972 1015 1016 1076 1084 1094 1096 1140 1141 1142 1143 1144 1145 1149 1150 1151 1239 1243 1263 1269 1274 1283 1284 1285 1335 1342 1343 1383 1384 1385 1388 1391 1393 1398 1416 1459 1511 1529 1561
- **1.2.1 closed (8)**: #958 completed 11-05 (by #1548); #1381 completed 11-05 (by #1535); #1397 completed 11-05 (settled by #1570); #1382 not_planned 11-05; #1392 not_planned 11-05 (folded into #1543); #1093 duplicate 11-06 (superseded by #1445); #1395 not_planned 11-06 (maintainer: today's output right, pinned by #1581 test); #1588 duplicate 11-06 (folded into #1150; SHARED-FIX-009 to be written — **not yet in requirement-ids.txt on main**).
- **1.2.2 (#13): 29 open, 0 closed** — 966 974 1071 1074 1079 1092 1105 1147 1152 1153 1264 1266 1267 1268 1270 1276 1289 1333 1334 1337 1339 1345 1346 1347 1348 1386 1387 1412 1543
- Counts open: Maintenance 59, vNext 108, 1.4.0 9, 1.5.0 3 (1.3.0: 0).
- #1084 already moved to 1.2.1 (plan's move done).

## 3. Plan vs. state
All planned issues are open and in their planned milestone **except #1395 (P1), closed not_planned** → P1 = #963 #1384 (#1384 decided 11-06: FMT-STRUCT-024 wins, documented field not split; FMT-STRUCT-022 example + RH7101 tests to align).
Unplanned open: 1.2.1 → #1511 #1529 #1561 (confirmed; #1588 now closed); 1.2.2 → #1543 (confirmed). No others.

Runs whose premise/files were touched by merged PRs:
- **P1**: #1481 changed `CF/.../RH7101DoNotCombineFieldsCodeFixProvider.cs`, `RH7101…AnalyzerTests.cs`, `sdd/specs/analyzer/RH7101.md`; #1618 rewrote `FormatNodeInDocumentAsync` write-back in `src/Reihitsu.Formatter/ReihitsuFormatter.cs` (same file as `SplitsFieldDeclaration`, different method).
- **P2**: #1632 touched `FMT/LineBreaks/Rewriter/ListElementInteriorJoinRewriter.cs`, `FMT/LineBreaks/Utilities/LineBreakTriviaUtilities.cs`, `line-breaks.md`, `indentation.md` (plan lists both specs). No P2 issue body mentions constraints → spec-file overlap only.
- **P4**: #1618 changed RH5011 fix, RH5025/RH5027 analyzers + tests, RH5030 fix, specs RH5011/RH5025/RH5027; #1481 changed `Core/SyntaxTriviaUtilities.cs`. Expected (P4 "after R8") — re-check #1076's instances.
- **P5**: #1618 changed RH5011 fix, RH5405–07 tests, `shared/statement-braces.md`, `ReihitsuFormatter.cs`; #1481 + #1609 changed `src/Reihitsu.Analyzer.Test/Base/AnalyzerTestsBase{TAnalyzer,TCodeFix}.cs` (fix test base). #1416 comment 11-07: SHARED-BRACE-002 deliberately not aligned to SHARED-FIX-007 by #1618; #1388 gained an RH7004 CRLF instance (11-06).
- **P6**: #1632 changed RH5110 layout (fix now formats through formatter incl. comment-held cases) + RH5110 spec/tests → #1140's RH5110 instance needs re-measuring. #1140/#1144 gained new instances 11-07 (RH5405 `else if` braces etc.). #1610 touched RH5202 only (adjacent to RH5203–06; `FMT/Indentation/Utilities/RawStringLineShifts.cs`, `RawStringLineUtilities.cs`, `Core/RawStringLiteralUtilities.cs`).
- **P7**: #1481 changed `CF/Base/TypeMemberOrderingCodeFixProviderBase.cs`, `ModifierOrderingCodeFixProviderBase.cs`, RH7004 + RH7303 fixes, new `CF/Core/RegionBlockLayout.cs`, RH7105/06 tests. Strong overlap; #969/#1335 comments 11-07 point to new issue #1604 (shared "move lines by column delta" helper, no milestone).
- **P9**: #1599 changed RH3001 fix + tests, new `CF/Core/DocumentOrderFixAllProvider.cs` (expected, "after R7").
- **P10**: #1596 and #1609 both changed `CF/Base/CasingCodeFixProviderBase{T}.cs`; #1596 changed RH4002/4005/4102/4105/4106 tests. Strong overlap (expected, "after R5").
- **P11**: #1150 absorbed #1588 (withhold node-level-only fix in a document the formatter refuses; SHARED-FIX-009 pending). #1481/#1599 edited `shared/code-fixes.md`.
- **Q1**: #1632 changed RH5110 spec/tests and comment-held constraint wraps → #1147's RH5110 instance (`/* c */ where T : class`) premise may have changed.
- **Q2**: #1609 changed `AN/Base/MembersShouldBeGroupedByRegionsAnalyzerBase.cs`, `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase.cs`, RH7409–7412 analyzers, RH7401–7412 specs/tests (single report per partial member, SHARED-DIAG-012); #1481 changed RH7101/RH7109/RH7309 specs+tests, RH7105/06 tests. #1334 comment 11-07 (partial-parts facts); #1264 new instance 11-06.
- **Q4**: #1609 changed RH8306 analyzer/spec/tests (Q4 lists RH8305–07 analyzers).
- **P3, P8, Q3, Q5, Q6**: no file overlap found (#1481 touched region fixes RH7303/7304/7306 near P3's region area, not `RegionNamingRewriter.cs`).

## 4. Open issues without milestone: 37
1589 1590 1591 1592 1593 1594 1595 1597 1598 1600 1601 1602 1603 1604 1605 1606 1607 1611 1612 1613 1615 1616 1617 1625 1626 1627 1628 1633 1634 1635 1636 1637 1638 1639 1640 1641 1642 (all filed 11-06 or 11-07; none older).

Not determined: closing PR of #1382 (not_planned, no PR); whether P2 issues' samples changed under #1632 (no reproduction).
