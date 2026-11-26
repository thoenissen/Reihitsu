# Roadmap 1.6.0 – 1.11.0

Condensed from the Phase 6 roadmap (decided by the maintainer 2026-11-16, additions through 2026-11-21) and its themes T1–T16. Open issue lists checked against GitHub on 2026-11-21. Notation: `(OQ)` = needs an output question; `→` = runs after. Run lists are the planned order; `start` and `batch` re-derive the real graph against `main`. Long-term, no release: #122 (Visual Studio extension, vNext).

## 1.6.0 — continuation columns, #1759 follow-ups, naming (milestone 14, 41 open)

Theme: T12 continuation columns and indentation; T2 rest (SHARED-DIAG-008 follow-ups, directive guards in fixes); T8 naming and casing; #1566, #975.

Runs, in order:
- **After the v1.5.0 tag:** `#1880` alone — the build passes the root `reihitsu.json` to the analyzers once `binaries/` knows the `design` section (the repository's production-type limit 500 is otherwise checked only by the self-hosting test).
- **Formatter/IndentationPhase, first:** `#953` with `#1852` `#1896` `#1818` (the general continuation rule; decides #1567 option B, #955 part 1 and #1645 option B in 1.7.0). Everything formatter-side below waits on it.
- **Formatter/hole and lambda lines (OQ):** `#1566 #1568 #1698 #1567` → after #953.
- **Formatter/list and pattern columns (one OQ round):** `#1633 #1634 #1647 #1685 #1542 #1929` → after #953.
- **Formatter/precheck (declared, change of its own):** `#1926` → after the list and pattern columns run (`indentation.md`).
- **Analyzer/RH5202:** `#1925` (decided A).
- **Tooling:** `#1924` (trace headers; no predecessors). Comma-first shapes of #1542/#1633 stay as written (they follow #1739 in 1.10.0).
- **Formatter/indentation small + docs:** `#1399 #1432 #1722 #1400 #1607`; `#1472` (written review only, docs lane) rides alongside.
- **Analyzer/SHARED-DIAG-008 follow-ups (OQ):** `#1240 #1275 #1591 #1836`.
- **Code fixes/directive guards (OQ):** `#1668 #1589 #970 #1837` (#970 must stay a cheap check).
- **Analyzer/casing derivation:** `#971 #1072 #1451 #1434` (new ID for #1434) → `#1771` (needs escalation 9 before building).
- **Analyzer/naming scope:** `#1085 #1086 #1087 #1088 #1089 #1090` (#1087 with #1088).
- **Tests:** `#1912` (message literals in tests).
- **CLI:** `#975` (small bug, no shared files; joins any start-now set).

Known dependencies: registry files (`*.resx`, rulesets, spec index) make the analyzer runs with new IDs (#1434) sequential with any other run that adds a rule.

## 1.7.0 — line-break ownership, clarity/design rule scope (milestone 15, 46 open)

Theme: T11 part A (line-break joins, extension blocks, switch arms, one-owner refactors); T7 clarity/design/performance/organization rule scope; Playground.

Runs, in order:
- **Formatter/LineBreakPhase one-owner refactors, first in their files:** `#1517 #1519 #1654 #862 #1937` (#1937 after #1935 and 1.6.0's indentation runs).
- **Formatter/unowned joins (one OQ round):** `#1639 #1645 #1646 #1648 #1652 #1653 #1829 #1830 #1927` → after #953 (1.6.0). #1645 may already be shipped by #1920: check and close first. #1927's inputs are run before its output question.
- **Formatter/extension blocks (owner set once, OQ):** `#1636 #1785 #1446 #1845 #1959 #1960` (#1959 2A and #1960 3C/4A decided).
- **Formatter/lists and switch arms:** `#956` (narrowed, OQ) `#955 #1442` (#956 before #1739 in 1.10.0).
- **Analyzer/RH1001–RH1002 equality judgement:** `#1290` → `#1295 #1296 #1297`.
- **Analyzer/small widenings:** `#973 #968 #1298 #1299 #1300 #1338 #1350 #1452`.
- **Analyzer/RH3002:** `#877` (item 1) `#1603` → `#1278`.
- **Playground and formatter query:** `/squad #1816 #1723` (one run; #1816 covers #1723; the new public formatter member is decided there); `#976` → `#1950` (Playground typing speed; shares `Home.razor`).
- **Docs filler:** `#1291` (spec text, joins any docs run).
- **Judge in its run:** `#1602` (goal stale; close or re-scope).
- **Placed later, run not yet planned:** `#1863 #1867 #1875 #1876 #1889 #1905 #1916` (T7 and line-break follow-ups of the 1.5.0 runs; #1905 joins a run of its theme).

## 1.8.0 — code-fix layout equals formatter layout (milestone 16, 31 open)

Theme: T1 code-fix layout (partial write-back, shared lines); T11 part B attribute-list layout; T14 code-fix coverage and shared helpers. Needs the formatter output of 1.6.0/1.7.0 settled.

Runs, in order:
- **Formatter entry point/partial write-back design, first:** `#1774 #1140` (escalation A/B/C; the design gate of the release). Output questions come in two rounds: at start #1774 #1447 #1549 #1552 #1554 #1606 #1689 #1777; after the design #1140 #1773 #1775.
- **Node-level write-back:** `#1521 #1474 #1684 #1835` → after #1774 (#1684 may close if it needs the layout-only mode).
- **Shared-line members and hand-laid fixes:** `#1773 #1775 #1447 #1443 #1758 #1957 #1961` → after #1774 (#1957 narrowed to the line ending; #1443 re-checked against #1760 first; #1758 before #1739 in 1.10.0).
- **Formatter/attribute-list layout (OQ):** `#1549 #1552 #1554 #1557 #1131`; `#1555` judged there (check on `main`, close if no reachable difference).
- **Code fixes/decided output extensions:** `#1435 #1436 #1437 #1689 #1777` (#1689 carries #1601's raw-string remainder, cheap checks only).
- **Core and code-fix cleanups:** `#1032 #1475 #1582 #1583 #1740 #1789`.
- **Specs:** `#1606` (item 2 split as a fix-output bug) `#1081 #1772`.

## 1.9.0 — comments and blank lines (milestone 17, 35 open)

Theme: T10 comments (placement, "documents nothing", doc-comment formatting); T13 part A (spacing, blank lines, switch braces).

Runs, in order:
- **"Documents nothing" and FMT-BLANK-011, first:** `#1077 #1280 #962 #1148` (#1077 absorbs #957 part 2; #962 decided with #1148).
- **Formatter/doc-comment placement:** `#1533 #1563 #1564 #961 #1461`.
- **Comment rules:** `#1097 #1098` (anchor model); `#1078` → `#1160`; `#1864 #1914` together (SHARED-COMMENT-012).
- **Formatter/spacing (OQ):** `#1440 #1550 #1556 #1559 #1928` (#1928 decided A; #1559 before #1560 in 1.10.0).
- **Formatter/blank lines:** `#1095 #1562 #1713 #1776` (#1562 closes with its regression test).
- **Formatter/switch braces:** `#1518` → `#1538 #950`; `#1537 #1540 #1659` judged there (likely closes: #1540 on A, #1659 on B).
- **Specs and tests (docs lane):** `#1571 #1572 #1641 #1661 #1690 #1819`.

## 1.10.0 — layout rules follow the formatter (P11), directives and line ends (milestone 18, 33 open)

Theme: T6 layout rule scope (P11) and counterpart rules; #1612, #1033; T13 part B (directives and line ends). Depends on #956, #1636/#1785 (1.7.0), #1758 (1.8.0) before #1739, #1077 (1.9.0) for #1433, #1559 (1.9.0) before #1560.

Runs, in order:
- **Analyzer/line-start facts, first where reports change:** `#1512 #1157 #1156 #1080 #1433`.
- **Analyzer/RH53xx initializers and operators:** `#1271 #1272 #1273 #1477`.
- **Analyzer/RH51xx list scope (OQ):** `#1154 #1155 #1649 #1158 #1650 #1394 #1931` (#1931 asked together with #1155/#1649).
- **Analyzer/new counterpart rules (OQ):** `#1739 #1159 #1637 #1638 #1560 #1827 #1930` (#1159 decided with #1739 in one change doc).
- **Analyzer/new layout rules (OQ):** `#1843 #1844` (one run).
- **Analyzer/attribute placement and hole lines (OQ):** `#1445 #1033 #1628 #1612`.
- **Formatter/directives and line ends (OQ):** `#960 #1579 #1658 #959 #1082 #1895` (#959 is large and splittable; #1895's premise unconfirmed).

Eleven output questions in one round; #1658 and #1082 may move to 1.11.0 if the round is too large.

## 1.11.0 — documentation-comment rules, regions (milestone 19, 31 open)

Theme: T9 documentation-comment rules (P8); T13 part C regions; #1585, #1694. Depends on T10 (1.9.0) for #1341 (P13), #1650 (1.10.0) for #1349, #1078 (1.9.0) for #1444.

Runs, in order:
- **Analyzer/documented-ness, decided together first:** `#1091 #1092 #1448` (re-decide #1091's merged-parts instance against `documentation-rules-reported-elements.md`).
- **Analyzer/parameter matching and rule pages (OQ):** `#1449 #1132 #1053`.
- **Analyzer/coverage per declaration (OQ):** `#1292 #1709 #1293 #1294 #1349`.
- **Analyzer/partial members (OQ):** `#1710 #1711 #1615` (#1711 reproduced first).
- **Formatter + analyzer/tag case (OQ):** `#1355` → `#1356 #1450`.
- **Analyzer/documentation layout rules (P13, OQ):** `#1341 #1351 #1352 #1460 #1682 #1712`.
- **Regions (formatter + RH73xx):** `#1083 #1354 #1265 #1444 #1574`.
- **Filler, separate lanes:** `#1585` (generated-code detection, OQ), `#1694` (spec + tests), `#1330` (narrow or close).
