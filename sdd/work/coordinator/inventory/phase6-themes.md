# Phase 6 themes (2026-11-16)

Input: `phase6-slice-1.md` … `-8.md`, `phase6-index.md`, `phase6-triage.md`. 249 indexed issues − 15 being closed (#1054 #1244 #1259 #1453 #1534 #1546 #1584 #1590 #1681 #1741 #1601 #1353 #1605 #957 #1553) + 4 newly triaged (#1780 Maintenance; #1781 #1785 #1786 vNext) = **238 issues in 16 themes**, each in exactly one. Maintenance issues sit in the theme whose code they touch; T15/T16 hold only issues that touch no product code (tooling, tests, specs) plus the CLI/IDE surface.

Notation: `#n (milestone, size, OQ)` — milestone `M` Maintenance, `v` vNext, `4` 1.4.0, `5` 1.5.0; `OQ` = needs an output question. Split column = Maintenance/vNext/1.4.0/1.5.0; size = S/M/L.

## 1. Summary

| # | Theme | Issues | M/v/1.4/1.5 | S/M/L | User-visible | OQ | Key dependencies | Value / cost / risk |
|---|---|---|---|---|---|---|---|---|
| T1 | Code-fix layout: partial write-back & shared lines | 10 | 4/6/0/0 | 3/3/4 | 8 | 5 | design decision in #1774 (A/B/C) first | High value (fix output equals formatter output); L design, one escalation, then mechanical. |
| T2 | SHARED-DIAG-008 reversal & directive guards in fixes | 7 | 3/4/0/0 | 3/2/2 | 4 | 1 | #1759 first; T14 #1689 | Decided by maintainer; L spec walk (~60 specs), low output risk (reported, fix withheld). |
| T3 | Unsupported shapes | 6 | 1/5/0/0 | 3/3/0 | 5 | 2 | each shape its own change; unblocks T4 #894 option, T5 #851, field split | Decided; deletes special cases; medium risk (files refused as a whole). |
| T4 | Member-access chains (1.4.0) | 10 | 4/0/6/0 | 7/3/0 | 6 | 3 | #894 decision vs T3; #893 after #892 | Coherent 1.4.0 block (formatter + RH5112/RH5201 parity); M cost, 3 OQs. |
| T5 | Expression-bodied & get-only members (1.4.0) | 7 | 1/3/3/0 | 2/5/0 | 6 | 1 | #851 after T3 #1732; #1515 after #1539 | Coherent 1.4.0 block (three new rules + arrow-comment placement); moderate cost. |
| T6 | Layout rule scope (P11) & layout counterpart rules | 21 | 2/19/0/0 | 9/11/1 | 20 | 8 | T11 #956 for #1739; T11 #1636/#1785 for #1154 | Many small decided widenings + 6 new rules; batchable by rule family; output-question round needed. |
| T7 | Clarity/design/performance/organization rules: scope & new rules | 19 | 2/14/0/3 | 7/11/1 | 17 | 3 | #1290 before #1295–#1297 | Mostly decided S/M widenings; 1.5.0 size metrics (#352–#354) need a design round. |
| T8 | Naming & casing | 11 | 2/9/0/0 | 6/5/0 | 10 | 1 | #1087 with #1088; #1771 new shared type (escalation 9) | Decided bug pairs; low risk; one casing-derivation run. |
| T9 | Documentation-comment rules (P8) | 23 | 5/18/0/0 | 15/5/3 | 19 | 7 | #1091/#1092/#1448 decided together first | Largest analyzer cluster; several L items and a conflict with `documentation-rules-reported-elements.md`. |
| T10 | Comments: placement, "documents nothing", doc-comment formatting | 15 | 6/9/0/0 | 6/9/0 | 9 | 3 | #1077 first; #1160 after #1078; with T13 #1148/#1781 | P6/P13 consolidation; medium risk (formatter relocations change output). |
| T11 | Formatter line breaks, wrap joins & attribute lists | 24 | 8/16/0/0 | 14/10/0 | 17 | 13 | T12 #953 for column parts; T6 #1739 | Many small OQs (unowned joins) — one batched question round; refactors (#1517 #1519 #1654 #862) ride along. |
| T12 | Formatter continuation columns & indentation | 15 | 4/11/0/0 | 8/5/2 | 11 | 8 | #953 general rule first | #953 decides several; L rule with growing instance list. |
| T13 | Formatter spacing, blank lines, switch braces, regions/directives & line ends | 26 | 7/19/0/0 | 17/8/1 | 19 | 9 | #962/#1148 scope with T10 | Small decided pieces; #959 is L and splittable; several soft close candidates (#1537, #1540, #1659 answer B). |
| T14 | Code-fix coverage, output & shared helpers | 13 | 6/7/0/0 | 11/2/0 | 7 | 4 | #1689 takes #1601's raw-string remainder | Small decided fix extensions + Core cleanups; low risk. |
| T15 | CLI, Playground, IDE & squad tooling | 14 | 10/4/0/0 | 12/1/1 | 4 | 2 | — | Cheap hygiene plus #122 (VS extension, L design, not squad-sized). |
| T16 | Test infrastructure & spec clarifications | 17 | 17/0/0/0 | 14/3/0 | 0 | 0 | #1507 with #1544 | No user change; docs/tests lanes; filler for any release. |
| | **Total** | **238** | 82/144/9/3 | | | | | |

## 2. Themes

### T1 · Code-fix layout: partial write-back & shared lines (10)
`#1140 (v,L,OQ) #1774 (v,L,OQ) #1521 (v,L) #1474 (M,M) #1684 (M,L) #1773 (v,M,OQ) #1775 (M,S,OQ) #1443 (v,S) #1447 (v,M,OQ) #1758 (M,S)`
Node-level write-back (layout-only mode) for layout fixes, plus SHARED-FIX-007 shared-line members and fixes still laying out by hand. Order: #1774 design (absorbs #1140 remainder RH5110/RH5403/RH5404 and #1605) → #1521, #1474, #1684. #1773 + #1775 one OQ round; #1443 re-check against #1760 first; #1447 picks one blank-line placement; #1758 (RH5107 fix delegates to formatter) before or with T6 #1739.

### T2 · SHARED-DIAG-008 reversal & directive guards in fixes (7)
`#1759 (v,L) #1240 (M,S) #1275 (v,S) #1591 (M,S) #1668 (v,M,OQ) #1589 (M,L) #970 (v,M)`
Analyzers judge the active build; fixes stay withheld across directives. Order: #1759 (spec walk, absorbs #1741/#1590) → #1240 and #1275 wording follow it, #1591 (RH7003 page "Exceptions") with it; #1668 and #1589 (fix-safety audit) are the fix half; #970 (`#pragma` move safety) narrows a guard and must stay a cheap check.

### T3 · Unsupported shapes (6)
`#1732 (v,M) #1726 (v,M) #1728 (v,M) #1586 (v,S,OQ) #1587 (v,S,OQ) #1672 (M,S)`
Precheck declarations decided by the maintainer, each in a change of its own. Order: #1728 → #1586 (shrink `CarriesDirective`) and #1587 (comment-free repeated break; column part waits on T12 #953). #1732 before T5 #851. #1672 (precheck perf, close if no gain) rides along. Candidate shape #894 (T4) and #1515's alternative (T5) are escalated, not decided here.

### T4 · Member-access chains (1.4.0) (10)
`#895 (4,M) #896 (4,S,OQ) #892 (4,M) #893 (4,M) #894 (4,S,OQ) #897 (4,S,OQ) #889 (M,S) #1476 (M,S) #1640 (M,S) #1651 (M,S)`
Formatter chain features and RH5112/RH5201 parity. Order: #1640 + #1651 (`ChainLineBreakRewriter` refactor) before #895/#896; #892 (new rule) → #893; #894 (declare shape unsupported vs. new rule — maintainer) decides whether #893 case 2 is dropped; #897 (titles) last; #889/#1476 tests with the rule changes.

### T5 · Expression-bodied & get-only members (1.4.0) (7)
`#849 (4,M) #850 (4,M) #851 (4,M) #1439 (v,S) #1515 (v,M,OQ) #1539 (M,M) #1277 (v,S)`
Get-only property/indexer expression-body rules and the arrow-comment placement of the conversion. Order: #849 → #850; T3 #1732 → #851 (mirrors RH5401); #1539 (one comment carrier) → #1439 → #1515; #1277 (RH5401/RH5307 fix scope) with #851.

### T6 · Layout rule scope (P11) & layout counterpart rules (21)
`#1445 (v,L,OQ) #1080 (v,S) #1154 (v,S) #1155 (v,S) #1156 (v,M) #1157 (v,M) #1158 (v,S) #1271 (v,M) #1272 (v,M) #1273 (v,S) #1433 (v,M) #1628 (v,S,OQ) #1649 (v,S,OQ) #1650 (v,M,OQ) #1512 (M,M) #1159 (v,M,OQ) #1560 (v,M,OQ) #1637 (v,M,OQ) #1638 (v,S,OQ) #1739 (v,M) #1477 (M,S)`
RH5xxx/RH55xx/RH6xxx rules widened to what the formatter already does, and new counterpart rules. Pairs: #1155+#1649 (RH5111), #1158+#1650 (parameter lists), #1156+#1560 (switch-section line), #1271+#1272+#1273 (RH53xx; #1271 closes #1244), #1637+#1638 (constraint counterparts), #1159+#1739 (one change doc may decide both), #1154 with T11 #1636/#1785. #1512 (shared `StartsLine` facts) first where it changes reports; #1445 is L (attribute placement by target, with T14 #1033).

### T7 · Clarity/design/performance/organization rules: scope & new rules (19)
`#973 (v,S) #1278 (v,M) #1290 (v,M) #1295 (v,M) #1296 (v,M) #1297 (v,S) #1298 (v,S) #1299 (v,M) #1300 (v,S) #1338 (v,S) #1350 (v,S) #1452 (v,S) #1602 (v,M) #968 (v,M) #877 (M,M) #1603 (M,M) #352 (5,L,OQ) #353 (5,M,OQ) #354 (5,M,OQ)`
RH1xxx/RH2xxx/RH3xxx/RH7xxx/RH85xx report scope, plus the 1.5.0 size-metric rules. Sub-runs: #1290 → #1295/#1296/#1297 (equality judgement); #1298/#1299/#1300/#1338/#1350 (small widenings, batch); #877 item 1 + #1603 → #1278 (RH3002); #353 → #354, #352 (one line-count/option design). #1602's goal is stale (watch *Special cases*).

### T8 · Naming & casing (11)
`#1085 (v,S) #1086 (v,S) #1087 (v,M) #1088 (v,M) #1089 (v,S) #1090 (M,S) #1434 (v,M) #1451 (v,M,OQ) #971 (v,S) #1072 (v,S) #1771 (M,M)`
RH4xxx scope and casing-fix derivation. #1087 and #1088 ship together; #971 + #1072 + #1451 + #1434 change the casing derivation (one run, new ID for #1434); #1771 (one rename owner) needs escalation 9 before building; #1090 rename rides along.

### T9 · Documentation-comment rules (P8) (23)
`#1091 (v,L) #1092 (v,S) #1448 (v,S) #1053 (M,S,OQ) #1132 (M,S) #1293 (v,S) #1294 (v,S) #1292 (v,M) #1341 (v,L) #1349 (v,S) #1351 (v,S) #1352 (v,M) #1355 (v,S) #1356 (v,M,OQ) #1449 (v,L,OQ) #1450 (v,S,OQ) #1709 (v,S,OQ) #1710 (v,S,OQ) #1711 (v,M) #1712 (v,M,OQ) #1460 (M,S) #1682 (M,S) #1615 (M,S)`
RH8xxx coverage, partial members, tag casing. Order: #1091 + #1092 + #1448 decided together (re-decide #1091's merged-parts instance against `documentation-rules-reported-elements.md`) → #1053 (rule pages) and #1449; #1355 (formatter tag case) → #1356 (new rule); #1710 + #1711 (partial members; #1711 reproduce first) with #1615; #1292 + #1709; #1132 + #1449 share param-name matching; #1341 (nine rules) links to T10 P13.

### T10 · Comments: placement, "documents nothing", doc-comment formatting (15)
`#1077 (v,M) #962 (v,M,OQ) #961 (v,M) #1533 (v,M,OQ) #1563 (M,S) #1564 (M,S) #1461 (M,S) #1097 (v,M) #1098 (v,M) #1078 (v,M) #1160 (v,M,OQ) #1280 (v,S) #1571 (M,S) #1572 (M,M) #1641 (M,S)`
A `///`/`/** */` that documents nothing is an ordinary comment (P6/P13), comment-rule ownership, multi-line comment closing line. Order: #1077 (absorbs #957 part 2) → #962, #1280; #1533 → #1563, #1564; #1097 + #1098 (anchor model); #1078 → #1160; #1572 + #1571 + #1641 one spec/test run; #1461 refactor with #961.

### T11 · Formatter line breaks, wrap joins & attribute lists (24)
`#955 (v,S) #956 (v,M,OQ) #1442 (v,M) #1446 (v,M) #1639 (v,S,OQ) #1645 (v,S,OQ) #1646 (v,S,OQ) #1648 (v,S,OQ) #1652 (v,S,OQ) #1653 (v,S,OQ) #1636 (v,M,OQ) #1785 (v,M,OQ) #1786 (v,S,OQ) #1549 (v,M,OQ) #1552 (v,S,OQ) #1554 (v,S,OQ) #1555 (M,M) #1557 (M,S) #1131 (M,S) #1517 (M,M) #1519 (M,M) #1654 (M,S) #862 (M,S) #1566 (M,M)`
LineBreakPhase ownership. Sub-runs: unowned joins #1639 #1645 #1646 #1648 #1652 #1653 #1786 (one OQ batch); #1636 + #1785 (extension-block owner set once); attribute lists #1549 #1552 #1554 #1557 #1131 (#1555 close after check); #956 (narrowed) with T6 #1739; #1442 with #862; refactors #1517 #1519 #1654 first in their files; #1566 (C# < 11) independent.

### T12 · Formatter continuation columns & indentation (15)
`#953 (v,L) #1399 (v,M) #1432 (v,S) #1542 (v,M,OQ) #1567 (v,S,OQ) #1568 (v,M,OQ) #1633 (v,M,OQ) #1634 (v,M,OQ) #1647 (v,S,OQ) #1685 (v,S,OQ) #1698 (v,S,OQ) #1472 (M,L) #1722 (M,S) #1400 (M,S) #1607 (M,S)`
Columns of continuation lines no alignment rule owns. #953 first (general rule decides #1567 option B, T11 #955 part 1 and #1645 option B, T3 #1587 column); then #1399, #1633/#1634/#1647/#1685/#1568 as one OQ round; #1698 with #1568; #1542 and #1633 comma-first shapes tie to T6 #1739. #1472 is a written review only; #1400/#1607 spec text; #1722 refactor.

### T13 · Formatter spacing, blank lines, switch braces, regions/directives & line ends (26)
`#1440 (v,M) #1550 (v,S,OQ) #1556 (v,S,OQ) #1559 (v,S,OQ) #1562 (M,S) #1095 (v,S) #1148 (v,M) #1781 (v,S,OQ) #1537 (v,M,OQ) #1538 (M,S) #1540 (v,S,OQ) #1518 (M,M) #1659 (v,S,OQ) #950 (M,S) #1713 (M,S) #1776 (M,S) #1083 (v,S) #1354 (v,M) #1265 (v,M) #1444 (v,M) #959 (v,L) #960 (v,S) #1082 (v,S,OQ) #1579 (v,S) #1658 (v,M,OQ) #1574 (M,S)`
Same-line spacing, blank-line scope, switch-section braces, region/directive text, line-end cleanup. Sub-runs: spacing #1440 #1550 #1556 #1559; blank lines #1148 + #1781 (FMT-BLANK-011, decide with T10 #962) #1095 #1562 + refactors #1713 #1776; switch #1518 → #1538 + #1537 #1540 #1659 (+ #950); regions #1083 #1354 #1265 #1444 #1574; directives/line ends #960 + #1579 + #1658 (new owner), #959 (splittable) + #1082.

### T14 · Code-fix coverage, output & shared helpers (13)
`#1033 (v,S) #1777 (v,S,OQ) #1689 (v,M,OQ) #1435 (v,S) #1436 (v,S) #1437 (v,S) #1612 (v,S,OQ) #1606 (M,M,OQ) #1032 (M,S) #1582 (M,S) #1583 (M,S) #1475 (M,S) #1740 (M,S)`
Narrower guards and decided fix-output extensions; Core/code-fix helper cleanups. #1435/#1436/#1437 one batch; #1689 takes the #1601 raw-string remainder (cheap checks only); #1612 with T6 #1628 (hole lines); #1606 item 2 split as a fix-output bug; #1583 moves `TriviaListUtilities` to Core; #1475 + #1740 dead Core members.

### T15 · CLI, Playground, IDE & squad tooling (14)
`#122 (v,L,OQ) #975 (v,S) #976 (v,S) #1585 (v,M,OQ) #1723 (M,S) #898 (M,S) #1427 (M,S) #1470 (M,S) #1471 (M,S) #1479 (M,S) #1545 (M,S) #1724 (M,S) #1725 (M,S) #1780 (M,S)`
User surfaces outside the analyzers (CLI diff header, generated-code detection, Playground, VS extension) and repository tooling. #898 + #1724 (`trace.sh`); #1470 + #1471 + #1725 (state a rule once); #1780 central Roslyn version. #122 is a design proposal, not squad-sized.

### T16 · Test infrastructure & spec clarifications (17)
`#867 (M,M) #1505 (M,S) #1576 (M,S) #1577 (M,S) #1330 (M,S) #1428 (M,S) #1507 (M,S) #1544 (M,S) #1516 (M,M) #1081 (M,S) #1291 (M,S) #1394 (M,S) #1661 (M,S) #1690 (M,S) #1693 (M,S) #1694 (M,M) #1772 (M,S)`
Tests lane: #1576 + #1577 (per-phase check), #1507 after/with #1544, #1516 (FMT-PIPE ownership test), #867 may surface real fix defects. Docs lane: #1081 #1291 #1394 #1693 #1694 #1772 and comment specs #1661 + #1690 (one run).

## 3. Cross-theme dependencies and conflicts

- **#1759 (T2) first**: #1240 and #1275 wording follow it; #1668, #1589 are its fix half; #1601's raw-string remainder moved to #1689 (T14).
- **#1091 + #1092 + #1448 (T9)** decided together; #1053 and #1449 after; #1091's merged-parts instance conflicts with `documentation-rules-reported-elements.md` (part by part).
- **Partial write-back (T1)**: #1774 design absorbs #1140 remainder and #1605; #1521, #1474, #1684 wait on it.
- **Unsupported shapes (T3) → T4/T5**: #1732 before #851; #894 (declare vs. new rule) decides #893 case 2; #1515 has a declared-shape alternative; #1728 before #1586/#1587.
- **Comma family**: #956 (T11) → #1739 (T6) ← #1758 (T1, RH5107 fix delegating); #1159 decided with #1739; comma-first shapes in #1542/#1633 (T12) follow #1739.
- **#953 (T12)** is the general continuation rule for #955 part 1 and #1645 (T11), #1567, #1587 (T3).
- **Constraint/extension owners**: #1636 + #1785 (T11) derive one owner set; #1154 (T6) RH5110 scope; #1637/#1638 (T6) with #1639 (T11).
- **FMT-BLANK-011**: #962 (T10) with #1148 + #1781 (T13).
- **P13 "documents nothing"**: #1077 (T10) ↔ #1341 (T9) ↔ #1433 Q3 (T6) ↔ #1280 (T10).
- **Comment spacing**: #1078 → #1160 (T10); region spelling `# region` #1444 (T13) overlaps #1078.
- **Attribute placement**: #1445 (T6) with #1033 (T14).
- **Hole lines**: #1612 (T14) + #1628 (T6); #1698 (T12) with #1566 (T11), #1568 (T12).
- **Switch-section line**: #1559 (T13) + #1560 (T6) + #1156 (T6).
- **Parameter lists**: #1650 (T6) ↔ #1349 (T9).
- **Casing**: #971/#1072/#1451/#1434 + #1771 (all T8).
- **RH3002**: #877 item 1 + #1603 before #1278 (all T7).

## 4. Milestone fit

- **1.4.0 (9)**: #895 #896 #892 #893 #894 #897 → T4; #849 #850 #851 → T5. Both themes are coherent and complete with their Maintenance attachments (T4: #889 #1476 #1640 #1651; T5: #1539) plus three vNext issues on the same code (#1439 #1515 #1277). External gates: #851 needs T3 #1732 (decided, M); #894 is a maintainer output question that may move into T3. A third 1.4.0 theme would fit T3 (6, decided, deletes guards T4/T5 depend on) or T2 (decided, L).
- **1.5.0 (3)**: #352 #353 #354 (size-metric rules) — internally coherent but only a sub-run of T7; the rest of T7 is vNext, so the milestone does not map to a whole theme.
