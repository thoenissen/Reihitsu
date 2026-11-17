# Phase 6 · Release roadmap for vNext and Maintenance (draft, 2026-11-16)

Maintainer goal (2026-11-16): plan every open issue of vNext (milestone 2) and Maintenance (milestone 4) into concrete releases; only long-term product initiatives stay outside. This is the coordinator's draft for the maintainer to confirm; nothing on GitHub is changed.

Inputs: `phase6-themes.md` (T1–T16), `phase6-slice-1..8.md`, `phase6-bookkeeping.md`, `phase6-bookkeeping-2.md`, `phase6-runs-1.4.0.md`. Open issue lists fetched from the REST API on 2026-11-16 (`issues?milestone=N&state=open&per_page=100`, all pages): **vNext 133 + Maintenance 59 = 192**. Every one of them is in a theme of `phase6-themes.md` except **#1789** (filed 2026-11-16 after the inventory: `TargetAttributePlacementCodeFixProviderBase` mirrors the brace-scope kinds of `SyntaxIndentationUtilities`; Maintenance, bug-fix lane, S, no output question), placed in **T14** (code-fix shared helpers). Issues already on 1.4.0 (28) and 1.5.0 (20) are not re-planned; #1785 stays here (vNext, T11).

Notation: `#n (theme, size, OQ)` — `OQ` = needs an output question. `M`/`v` = Maintenance/vNext.

## 1. Summary

| Release | Themes / parts | Issues | M / v | OQ | Main dependency on earlier releases | Rationale |
|---|---|---|---|---|---|---|
| 1.6.0 | T12 continuation columns & indentation (all) + #1566 (T11, hole line breaks); T2 rest (SHARED-DIAG-008 follow-ups, fix-side directive guards); T8 naming & casing; #975 (CLI) | 34 | 10 / 24 | 10 | #1759 (1.4.0) for T2; #1587 (1.4.0) leaves its column part to #953 | #953 is the general rule that later formatter work depends on; T2 finishes what #1759 starts while it is fresh; T8 is low-risk analyzer work in parallel. #1566 is a correctness bug (output that does not compile) |
| 1.7.0 | T11 part A: line-break joins, extension blocks, switch arms, one-owner refactors; T7 clarity/design/performance/organization rule scope (all left after 1.5.0); Playground (#976, #1723); #1291 | 34 | 8 / 26 | 9 | #953 (1.6.0) decides #955 part 1 and #1645 option B | Line-break ownership after the column rule; T7 has no output question, so it runs in parallel without adding to the question round |
| 1.8.0 | T1 code-fix layout (partial write-back, shared lines); T11 part B: attribute-list layout; T14 code-fix coverage & shared helpers (except #1033, #1612), incl. #1789; #1081, #1772 | 29 | 15 / 14 | 11 | Formatter output of T12/T11A (1.6.0/1.7.0) settled before fixes delegate more layout to it | Fix output equals formatter output once the formatter's line breaks and columns stop moving; the T14 fix extensions and Core cleanups share the code-fix area |
| 1.9.0 | T10 comments: placement, "documents nothing", doc-comment formatting (all); T13 part A: spacing, blank lines, switch braces; #1661, #1690 | 29 | 14 / 15 | 6 | — (T10 internal order: #1077 first) | FMT-BLANK-011 (#962 with #1148) and P6/P13 decided in one release; formatter and comment-rule analyzers split cleanly |
| 1.10.0 | T6 layout rule scope (P11) & counterpart rules (all) + #1612, #1033 (T14); T13 part B: directives & line ends; #1394 | 29 | 3 / 26 | 11 | #956, #1636/#1785 (1.7.0); #1758 (1.8.0) before #1739; #1077 (1.9.0) for #1433; #1559 (1.9.0) before #1560 | Analyzers follow what the formatter now does (P11) once the formatter work is in; directive/line-end formatter runs in parallel |
| 1.11.0 | T9 documentation-comment rules (all); T13 part C: regions; #1585 (T15), #1694 (T16) | 30 | 7 / 23 | 8 | T10 (1.9.0) for #1341 (P13); #1650 (1.10.0) for #1349; #1078 (1.9.0) for #1444 | Largest analyzer cluster last, after P13 is settled; region rules/formatter as the parallel formatter strand |
| **Total planned** | | **185** | 57 / 128 | 55 | | |

Long-term (no release): 1 (#122). Decide instead of plan: 6.

**1.5.0 additions:** #1803 (Maintenance, bug-fix, phases super-linear on long fluent chains and `?:` lists; maintainer 2026-11-16, own run next to the T4 chain runs). #1820 (Maintenance, tooling, benchmark 1.2.0 vs 1.3.0 and per-phase profile; maintainer 2026-11-17, runs before #1803, which it feeds; needs NuGet access for the released 1.2.0 and 1.3.0 tools). Otherwise none proposed. Checked: #862 (auto-property accessor-list predicate) and #1519 (property/indexer accessor-list braces) touch accessor-list code near T5, but neither is about the get-only/expression-body rules or chains; they stay in 1.7.0 with the other line-break one-owner refactors. If the T5 runs (#849/#850) find themselves adding a sixth copy of the auto-property predicate, #862 can be pulled into that run.

**1.6.0 additions:** #1818 (Extension, declarator behind `#endif` in an event field / local declaration loses continuation indent; maintainer 2026-11-17, planned with the continuation-indent work around #953; its output question is asked in that run).

**1.7.0 additions:** #1816 (Maintenance, one query for why whole-document formatting leaves a file alone; maintainer 2026-11-17, one run `/squad #1816 #1723`, since #1816 covers #1723; the new public formatter member is decided in that run).

**1.9.0 additions:** #1819 (Maintenance, FMT-BLANK-019 contradicts itself for `[⏎⏎]`; label `maintenance` added; maintainer 2026-11-17, docs-lane specs run).

**Output-question rounds.** Two releases exceed ten questions:
- 1.8.0 (11): #1774 is a design escalation (A/B/C) that has to be answered before #1140, #1773 and #1775 can be asked as outputs, so the release has two rounds by construction — round 1 at start (#1774, #1447, #1549, #1552, #1554, #1606, #1689, #1777), round 2 after the design (#1140, #1773, #1775).
- 1.10.0 (11): eight are small P11 rule-scope questions of T6 (+ #1612 with #1628) that read well in one round; the two directive questions (#1658, #1082) can go in the same round or be moved to 1.11.0 (would make 1.10.0 27 issues / 9 OQ and 1.11.0 32 / 10).

## 2. Releases

### 1.6.0 — continuation columns, #1759 follow-ups, naming (34)

Issues: #953 (T12, L) #1399 (T12, M) #1432 (T12, S) #1542 (T12, M, OQ) #1567 (T12, S, OQ) #1568 (T12, M, OQ) #1633 (T12, M, OQ) #1634 (T12, M, OQ) #1647 (T12, S, OQ) #1685 (T12, S, OQ) #1698 (T12, S, OQ) #1472 (T12, L) #1722 (T12, S) #1400 (T12, S) #1607 (T12, S) #1566 (T11, M) #1240 (T2, S) #1275 (T2, S) #1591 (T2, S) #1668 (T2, M, OQ) #1589 (T2, L) #970 (T2, M) #1085 (T8, S) #1086 (T8, S) #1087 (T8, M) #1088 (T8, M) #1089 (T8, S) #1090 (T8, S) #1434 (T8, M) #1451 (T8, M, OQ) #971 (T8, S) #1072 (T8, S) #1771 (T8, M) #975 (T15, S)

Runs (rough; batches are planned at release start):
- **Formatter/IndentationPhase, first:** #953 alone (L, the general rule; decides #1567 option B, #955 part 1, #1645 option B and #1587's column part). Exception to the 3–8 size because everything below waits on it.
- **Formatter/hole and lambda lines:** #1566 #1568 #1698 #1567 (interpolation holes and anonymous methods; #1698 with #1566/#1568 per the themes).
- **Formatter/list and pattern columns (one OQ round):** #1633 #1634 #1647 #1685 #1542. The comma-first shapes in #1542/#1633 follow #1739 (1.10.0); leave them as written here.
- **Formatter/indentation small + docs:** #1399 #1432 #1722 #1400 #1607; #1472 (written review only, may spawn its own issue) rides alongside, docs lane.
- **Analyzer/SHARED-DIAG-008 follow-ups:** #1240 #1275 #1591 (wording and RH5412–RH5416 after #1759).
- **Code fixes/directive guards:** #1668 #1589 #970 (fix half of #1759; #970 must stay a cheap check).
- **Analyzer/casing derivation:** #971 #1072 #1451 #1434 (one run, new ID for #1434); then #1771 (needs escalation 9 before building).
- **Analyzer/naming scope:** #1085 #1086 #1087 #1088 #1089 #1090 (#1087 with #1088).
- **CLI:** #975 (S bug; joins any batch, no shared files).

### 1.7.0 — line-break ownership, clarity/design rule scope (34)

Issues: #955 (T11, S) #956 (T11, M, OQ) #1442 (T11, M) #1446 (T11, M) #862 (T11, S) #1517 (T11, M) #1519 (T11, M) #1654 (T11, S) #1636 (T11, M, OQ) #1785 (T11, M, OQ) #1639 (T11, S, OQ) #1645 (T11, S, OQ) #1646 (T11, S, OQ) #1648 (T11, S, OQ) #1652 (T11, S, OQ) #1653 (T11, S, OQ) #877 (T7, M) #968 (T7, M) #973 (T7, S) #1278 (T7, M) #1290 (T7, M) #1295 (T7, M) #1296 (T7, M) #1297 (T7, S) #1298 (T7, S) #1299 (T7, M) #1300 (T7, S) #1338 (T7, S) #1350 (T7, S) #1452 (T7, S) #1603 (T7, M) #976 (T15, S) #1723 (T15, S) #1291 (T16, S) #1602 (T7, S, decide)

Runs:
- **Formatter/LineBreakPhase one-owner refactors, first in their files:** #1517 #1519 #1654 #862.
- **Formatter/unowned joins (one OQ batch):** #1639 #1645 #1646 #1648 #1652 #1653.
- **Formatter/extension blocks (owner set once):** #1636 #1785 #1446.
- **Formatter/lists and switch arms:** #956 (narrowed) #955 #1442 (#956 before T6 #1739).
- **Analyzer/RH1001–RH1002 equality judgement:** #1290 → #1295 #1296 #1297.
- **Analyzer/small widenings:** #973 #968 #1298 #1299 #1300 #1338 #1350 #1452.
- **Analyzer/RH3002:** #877 (item 1) #1603 → #1278.
- **Playground + docs filler:** #976 #1723 (Playground); #1291 (spec text, docs lane, can join any docs run).

### 1.8.0 — code-fix layout equals formatter layout (29)

Issues: #1140 (T1, L, OQ) #1774 (T1, L, OQ) #1521 (T1, L) #1474 (T1, M) #1684 (T1, L) #1773 (T1, M, OQ) #1775 (T1, S, OQ) #1443 (T1, S) #1447 (T1, M, OQ) #1758 (T1, S) #1549 (T11, M, OQ) #1552 (T11, S, OQ) #1554 (T11, S, OQ) #1557 (T11, S) #1131 (T11, S) #1032 (T14, S) #1435 (T14, S) #1436 (T14, S) #1437 (T14, S) #1475 (T14, S) #1582 (T14, S) #1583 (T14, S) #1606 (T14, M, OQ) #1689 (T14, M, OQ) #1740 (T14, S) #1777 (T14, S, OQ) #1789 (T14, S) #1081 (T16, S) #1772 (T16, S) #1555 (T11, M, decide)

Runs:
- **Formatter entry point/partial write-back design, first:** #1774 + #1140 remainder (escalation A/B/C; absorbed #1605). Two issues, the design gate of the release.
- **Node-level write-back after the design:** #1521 #1474 #1684 (#1684 may close with a reason if it needs the layout-only mode).
- **Shared-line members and hand-laid fixes:** #1773 #1775 #1447 #1443 #1758 (#1443 re-checked against #1760 first; #1758 before T6 #1739).
- **Formatter/attribute-list layout:** #1549 #1552 #1554 #1557 #1131.
- **Code fixes/decided output extensions:** #1435 #1436 #1437 #1689 #1777 (#1689 carries #1601's raw-string remainder, cheap checks only).
- **Core and code-fix cleanups:** #1032 #1475 #1582 #1583 #1740 #1789.
- **Specs:** #1606 (item 2 split as a fix-output bug) #1081 #1772.

### 1.9.0 — comments and blank lines (29)

Issues: #1077 (T10, M) #962 (T10, M, OQ) #961 (T10, M) #1533 (T10, M, OQ) #1563 (T10, S) #1564 (T10, S) #1461 (T10, S) #1097 (T10, M) #1098 (T10, M) #1078 (T10, M) #1160 (T10, M, OQ) #1280 (T10, S) #1571 (T10, S) #1572 (T10, M) #1641 (T10, S) #1440 (T13, M) #1550 (T13, S, OQ) #1556 (T13, S, OQ) #1559 (T13, S, OQ) #1148 (T13, M) #1095 (T13, S) #1562 (T13, S) #1713 (T13, S) #1776 (T13, S) #1518 (T13, M) #1538 (T13, S) #950 (T13, S) #1661 (T16, S) #1690 (T16, S) #1537 (T13, M, decide) #1540 (T13, S, OQ, decide) #1659 (T13, S, OQ, decide)

Runs:
- **"Documents nothing" and FMT-BLANK-011, first:** #1077 #1280 #962 #1148 (#1077 absorbs #957 part 2; #962 decided with #1148).
- **Formatter/doc-comment placement:** #1533 #1563 #1564 #961 #1461.
- **Comment rules:** #1097 #1098 (anchor model), #1078 → #1160.
- **Formatter/spacing:** #1440 #1550 #1556 #1559 (#1559 before T6 #1560 in 1.10.0).
- **Formatter/blank lines:** #1095 #1562 #1713 #1776 (#1562 closes with its regression test).
- **Formatter/switch braces:** #1518 → #1538 #950.
- **Specs and tests (docs lane):** #1571 #1572 #1641 #1661 #1690.

### 1.10.0 — layout rules follow the formatter (P11), directives and line ends (29)

Issues: #1080 (T6, S) #1154 (T6, S) #1155 (T6, S) #1156 (T6, M) #1157 (T6, M) #1158 (T6, S) #1159 (T6, M, OQ) #1271 (T6, M) #1272 (T6, M) #1273 (T6, S) #1433 (T6, M) #1445 (T6, L, OQ) #1477 (T6, S) #1512 (T6, M) #1560 (T6, M, OQ) #1628 (T6, S, OQ) #1637 (T6, M, OQ) #1638 (T6, S, OQ) #1649 (T6, S, OQ) #1650 (T6, M, OQ) #1739 (T6, M) #1612 (T14, S, OQ) #1033 (T14, S) #960 (T13, S) #1579 (T13, S) #1658 (T13, M, OQ) #959 (T13, L) #1082 (T13, S, OQ) #1394 (T16, S)

Runs:
- **Analyzer/line-start facts, first where reports change:** #1512 #1157 #1156 #1080 #1433.
- **Analyzer/RH53xx initializers and operators:** #1271 #1272 #1273 #1477.
- **Analyzer/RH51xx list scope:** #1154 #1155 #1649 #1158 #1650 #1394 (#1394 is RH5111 spec text).
- **Analyzer/new counterpart rules:** #1739 #1159 #1637 #1638 #1560 (#1159 decided with #1739 in one change doc).
- **Analyzer/attribute placement and hole lines:** #1445 #1033 #1628 #1612.
- **Formatter/directives and line ends:** #960 #1579 #1658 #959 #1082 (#959 is L and splittable).

### 1.11.0 — documentation-comment rules, regions (30)

Issues: #1091 (T9, L) #1092 (T9, S) #1448 (T9, S) #1053 (T9, S, OQ) #1132 (T9, S) #1293 (T9, S) #1294 (T9, S) #1292 (T9, M) #1341 (T9, L) #1349 (T9, S) #1351 (T9, S) #1352 (T9, M) #1355 (T9, S) #1356 (T9, M, OQ) #1449 (T9, L, OQ) #1450 (T9, S, OQ) #1709 (T9, S, OQ) #1710 (T9, S, OQ) #1711 (T9, M) #1712 (T9, M, OQ) #1460 (T9, S) #1682 (T9, S) #1615 (T9, S) #1083 (T13, S) #1354 (T13, M) #1265 (T13, M) #1444 (T13, M) #1574 (T13, S) #1585 (T15, M, OQ) #1694 (T16, M) #1330 (T16, S, decide)

Runs:
- **Analyzer/documented-ness, decided together first:** #1091 #1092 #1448 (re-decide #1091's merged-parts instance against `documentation-rules-reported-elements.md`).
- **Analyzer/parameter matching and rule pages:** #1449 #1132 #1053.
- **Analyzer/coverage per declaration:** #1292 #1709 #1293 #1294 #1349.
- **Analyzer/partial members:** #1710 #1711 #1615 (#1711 reproduced first).
- **Formatter + analyzer/tag case:** #1355 → #1356, with #1450.
- **Analyzer/documentation layout rules (P13):** #1341 #1351 #1352 #1460 #1682 #1712.
- **Regions (formatter + RH73xx):** #1083 #1354 #1265 #1444 #1574.
- **Filler:** #1585 (generated-code detection, CLI/formatter, OQ) and #1694 (spec + tests); separate lanes, join any batch.

## 3. Long-term (no release)

| # | Title | Reason |
|---|---|---|
| #122 | Visual Studio extension for Reihitsu formatter | New product (no VS project in `src/`), a design proposal of L size that spans several releases and is not squad-sized. Stays on vNext until it is broken into issues. |

No other open issue is a multi-release product initiative: #953 (L) is one rule with a growing instance list, #1472 is a written review, #1589 is one audit, #959 is splittable inside one release.

## 4. Decide instead of plan

Each would most likely close; ask the maintainer (or check cheaply) rather than schedule a run.

| # | Theme | Why | Suggested decision |
|---|---|---|---|
| #1537 | T13 | Bracing an `if`/`else` body or switch section whose trivia carries a directive; rare input, only a cheap directive-kind check would fit *Special cases do not grow the source*, and keeping today's guards is a valid end state | Close as not planned (guards stay), unless the maintainer wants the cheap-check variant (then 1.9.0 with #1518) |
| #1555 | T11 | Weak premise: the code fix's attribute-list split is a deliberate text replacement (SHARED-ATTR-017) withheld for line breaks (SHARED-ATTR-023), so the two-pass evidence is likely gone | Check on main; close if no reachable difference (else 1.8.0 with the attribute-list run) |
| #1540 | T13 | Output question whose recommended option A is today's output; A is only a clarification of FMT-SWITCH-007 | Ask; on A, close with a spec sentence (could ride with the docs run of 1.9.0) |
| #1659 | T13 | Output question; the filer recommends B (keep today's output), which closes it without code | Ask; on B, close |
| #1602 | T7 | Goal stale: RH3106-003 says "not reported"; closing the gap needs a consuming-vs-binding distinction that risks *Special cases* machinery | Close or re-scope with the maintainer; plan only if a cheap rule exists (then 1.7.0) |
| #1330 | T16 | Partly done (interpolated raw and single-line `u8` tested); remainder is multi-line UTF-8 raw and `///` lines, low value | Narrow and keep as filler, or close as low-value |

Close to this list but planned: #1777 (option B closes it; asked in 1.8.0's round), #1684 (may close if it needs the layout-only mode; decided by the 1.8.0 design), #1562 (closes with its regression test, 1.9.0), #1472 (written review only).

## 5. Coverage check

Open on GitHub 2026-11-16: vNext 133 + Maintenance 59 = **192** = planned **185** (1.6.0 34 + 1.7.0 34 + 1.8.0 29 + 1.9.0 29 + 1.10.0 29 + 1.11.0 30) + long-term **1** + decide **6**. Mismatch: none (each issue in exactly one place; #1789 was not in the inventory and is planned in 1.8.0/T14).


## Decided 2026-11-16 (maintainer "1a 2b")

Roadmap as drafted; decide-list issues planned too and judged in their run (#1602 1.7.0, #1555 1.8.0, #1537 #1540 #1659 1.9.0, #1330 1.11.0). Milestones: 1.6.0=14, 1.7.0=15, 1.8.0=16, 1.9.0=17, 1.10.0=18, 1.11.0=19.

**1.7.0 additions (2026-11-17):** #1829 #1830 join the unowned-joins OQ run (after #953). **1.10.0 additions (2026-11-17):** #1827 (new-rule candidate, counterpart of RH5105).

**Additions 2026-11-17 (b):** #1838 → 1.4.0, joins #1804 (batch 5 command becomes `/squad #1804 #1838`, tests lane). #1836 → 1.6.0, run *Analyzer/SHARED-DIAG-008 follow-ups* (#1240 #1275 #1591 #1836, OQ). #1837 → 1.6.0, run *Code fixes/directive guards* (#1668 #1589 #970 #1837, OQ). #1835 → 1.8.0, run *Node-level write-back after the design* (#1521 #1474 #1684 #1835, after #1774). No duplicates, none closed.

**Additions 2026-11-17 (c):** 1.10.0 #1843 #1844 (one run, OQ); 1.7.0 #1845 (extension-blocks run); 1.4.0 #1846 (own run), #1847 #1848 (one run after #1732).

**Additions 2026-11-17 (d):** 1.4.0 #1850 (own run), #1851 (own run after #1847 #1848); 1.6.0 #1852 (with #953).

**Change 2026-11-17 (e):** #1851 moved 1.4.0 -> 1.5.0 (follow-up rule; declaration decision stands).
