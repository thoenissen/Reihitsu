# Phase 6 bookkeeping 3 — roadmap milestones applied

Date: 2026-11-16. Source: `sdd/work/coordinator/inventory/phase6-roadmap.md` section 2, the `Issues:` line under each `### 1.X.0` heading (maintainer-approved 2026-11-16). Tool: `mcp__github__issue_write` method `update`, passing only `issue_number` and `milestone`. No comments posted; no labels, titles, or bodies touched.

## Pre-check

Each issue was fetched via the REST API before writing (all 191 once, then re-checked per batch immediately before the writes). All 191 were open issues (no pull requests), 132 on vNext (2) and 59 on Maintenance (4). No issue was skipped. Roadmap headings give counts 34/34/29/29/29/30, but the `Issues:` lines hold 34/35/30/32/29/31 — matching the expected counts; the lines were applied as written. No issue appears in two releases. #122 not touched.

## Moves

| Release | Milestone | Count | Issues (from → milestone) |
|---|---|---|---|
| 1.6.0 | 14 | 34 | #953(vNext→14) #1399(vNext→14) #1432(vNext→14) #1542(vNext→14) #1567(vNext→14) #1568(vNext→14) #1633(vNext→14) #1634(vNext→14) #1647(vNext→14) #1685(vNext→14) #1698(vNext→14) #1472(Maint→14) #1722(Maint→14) #1400(Maint→14) #1607(Maint→14) #1566(Maint→14) #1240(Maint→14) #1275(vNext→14) #1591(Maint→14) #1668(vNext→14) #1589(Maint→14) #970(vNext→14) #1085(vNext→14) #1086(vNext→14) #1087(vNext→14) #1088(vNext→14) #1089(vNext→14) #1090(Maint→14) #1434(vNext→14) #1451(vNext→14) #971(vNext→14) #1072(vNext→14) #1771(Maint→14) #975(vNext→14) |
| 1.7.0 | 15 | 35 | #955(vNext→15) #956(vNext→15) #1442(vNext→15) #1446(vNext→15) #862(Maint→15) #1517(Maint→15) #1519(Maint→15) #1654(Maint→15) #1636(vNext→15) #1785(vNext→15) #1639(vNext→15) #1645(vNext→15) #1646(vNext→15) #1648(vNext→15) #1652(vNext→15) #1653(vNext→15) #877(Maint→15) #968(vNext→15) #973(vNext→15) #1278(vNext→15) #1290(vNext→15) #1295(vNext→15) #1296(vNext→15) #1297(vNext→15) #1298(vNext→15) #1299(vNext→15) #1300(vNext→15) #1338(vNext→15) #1350(vNext→15) #1452(vNext→15) #1603(Maint→15) #976(vNext→15) #1723(Maint→15) #1291(Maint→15) #1602(vNext→15) |
| 1.8.0 | 16 | 30 | #1140(vNext→16) #1774(vNext→16) #1521(vNext→16) #1474(Maint→16) #1684(Maint→16) #1773(vNext→16) #1775(Maint→16) #1443(vNext→16) #1447(vNext→16) #1758(Maint→16) #1549(vNext→16) #1552(vNext→16) #1554(vNext→16) #1557(Maint→16) #1131(Maint→16) #1032(Maint→16) #1435(vNext→16) #1436(vNext→16) #1437(vNext→16) #1475(Maint→16) #1582(Maint→16) #1583(Maint→16) #1606(Maint→16) #1689(vNext→16) #1740(Maint→16) #1777(vNext→16) #1789(Maint→16) #1081(Maint→16) #1772(Maint→16) #1555(Maint→16) |
| 1.9.0 | 17 | 32 | #1077(vNext→17) #962(vNext→17) #961(vNext→17) #1533(vNext→17) #1563(Maint→17) #1564(Maint→17) #1461(Maint→17) #1097(vNext→17) #1098(vNext→17) #1078(vNext→17) #1160(vNext→17) #1280(vNext→17) #1571(Maint→17) #1572(Maint→17) #1641(Maint→17) #1440(vNext→17) #1550(vNext→17) #1556(vNext→17) #1559(vNext→17) #1148(vNext→17) #1095(vNext→17) #1562(Maint→17) #1713(Maint→17) #1776(Maint→17) #1518(Maint→17) #1538(Maint→17) #950(Maint→17) #1661(Maint→17) #1690(Maint→17) #1537(vNext→17) #1540(vNext→17) #1659(vNext→17) |
| 1.10.0 | 18 | 29 | #1080(vNext→18) #1154(vNext→18) #1155(vNext→18) #1156(vNext→18) #1157(vNext→18) #1158(vNext→18) #1159(vNext→18) #1271(vNext→18) #1272(vNext→18) #1273(vNext→18) #1433(vNext→18) #1445(vNext→18) #1477(Maint→18) #1512(Maint→18) #1560(vNext→18) #1628(vNext→18) #1637(vNext→18) #1638(vNext→18) #1649(vNext→18) #1650(vNext→18) #1739(vNext→18) #1612(vNext→18) #1033(vNext→18) #960(vNext→18) #1579(vNext→18) #1658(vNext→18) #959(vNext→18) #1082(vNext→18) #1394(Maint→18) |
| 1.11.0 | 19 | 31 | #1091(vNext→19) #1092(vNext→19) #1448(vNext→19) #1053(Maint→19) #1132(Maint→19) #1293(vNext→19) #1294(vNext→19) #1292(vNext→19) #1341(vNext→19) #1349(vNext→19) #1351(vNext→19) #1352(vNext→19) #1355(vNext→19) #1356(vNext→19) #1449(vNext→19) #1450(vNext→19) #1709(vNext→19) #1710(vNext→19) #1711(vNext→19) #1712(vNext→19) #1460(Maint→19) #1682(Maint→19) #1615(Maint→19) #1083(vNext→19) #1354(vNext→19) #1265(vNext→19) #1444(vNext→19) #1574(Maint→19) #1585(vNext→19) #1694(Maint→19) #1330(Maint→19) |

Total: 191 moved, 0 skipped, 0 failed (every write returned the issue id).

## Verification

- Open issues per milestone (REST, per_page=100) compared with the roadmap lists: 14=34, 15=35, 16=30, 17=32, 18=29, 19=31; zero differences in every milestone.
- vNext (2) now holds only #122. Maintenance (4) holds no open issue. No issue created after the roadmap sits on either.
- Label spot-check on 10 random moved issues (labels before vs. after, milestone now):

  - #1638 1.10.0 before_ms=2 now_ms=1.10.0 labels=[agent-filed,analyzer,enhancement] same
  - #1658 1.10.0 before_ms=2 now_ms=1.10.0 labels=[agent-filed,enhancement,formatter] same
  - #1636 1.7.0 before_ms=2 now_ms=1.7.0 labels=[agent-filed,analyzer,enhancement,formatter] same
  - #1559 1.9.0 before_ms=2 now_ms=1.9.0 labels=[agent-filed,enhancement,formatter] same
  - #1452 1.7.0 before_ms=2 now_ms=1.7.0 labels=[agent-filed,analyzer,enhancement] same
  - #1445 1.10.0 before_ms=2 now_ms=1.10.0 labels=[agent-filed,analyzer,enhancement] same
  - #1648 1.7.0 before_ms=2 now_ms=1.7.0 labels=[agent-filed,enhancement,formatter] same
  - #1294 1.11.0 before_ms=2 now_ms=1.11.0 labels=[agent-filed,analyzer,enhancement] same
  - #1713 1.9.0 before_ms=4 now_ms=1.9.0 labels=[agent-filed,formatter,maintenance] same
  - #1352 1.11.0 before_ms=2 now_ms=1.11.0 labels=[agent-filed,analyzer,enhancement] same

  All unchanged, `agent-filed` included (all 191 moved issues carried `agent-filed` before the move).
