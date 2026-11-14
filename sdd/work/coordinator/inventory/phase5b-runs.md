# Phase 5b run plan (1.2.1 / 1.2.2), drafted 2026-11-07 by a planning agent

Bug-fix lane except P13 (behavior-change). Every open 1.2.1 (54) / 1.2.2 (35) issue in exactly one run.

| Run | MS | Issues | Area | Prerequisite |
|---|---|---|---|---|
| P1 | 1.2.1 | #963 #1384 | field split (FMT-STRUCT-022/-024) | — |
| P2 | 1.2.1 | #1243 #1383 #1149 #1391 #952 #1611 | formatter line breaks, indentation, raw-string alignment | — |
| P3 | 1.2.1 | #1385 #1398 #1239 | formatter blank-line/switch/region | — |
| P4 | 1.2.1 | #946 #972 #1096 #1076 #1626 #1625 | blank-line-before-statement fixes | P3 |
| P5 | 1.2.1 | #1416 #1388 #1627 #1142 | brace/attribute-list fixes: layout, EOL, trailing ws | P4, P6, P1 |
| P6 | 1.2.1 | #1140 #1141 #1144 #1145 #1094 #1393 #1511 #1561 | RH51xx/52xx/RH6023-24/RH5513 fixes | P2 |
| P7 | 1.2.1 | #969 #1335 #1263 #1151 #1604 | organization/region fixes, column-delta helper | P12 |
| P8 | 1.2.1 | #1269 #1342 #1343 #1459 | documentation fixes | — |
| P9 | 1.2.1 | #1283 #1284 #1285 | clarity fixes | P4, P5 |
| P10 | 1.2.1 | #1597 #1598 #1016 #1015 #1084 | casing fixes | — |
| P12 | 1.2.1 | #1595 #1593 #1616 | critical ordering-fix guards (other build, comments) | — |
| P13 | 1.2.1+1.2.2 | #1529 #1543 | behavior change, output questions | P2 P3 P4 P6 |
| P11 | 1.2.1 | #1600 #1150 #1274 #1143 | withhold fix (other build, no-op, convergence) | last in 1.2.1 |
| Q1 | 1.2.2 | #1147 #1152 #1386 #1387 #1594 | layout analyzers' report scope | P5 P6 P11 |
| Q2 | 1.2.2 | #1264 #1333 #1334 #1339 #1337 #1412 #1592 | organization/region analyzers | P12 P7 P1 |
| Q3 | 1.2.2 | #966 #1105 #1266 #1267 | using ordering | — |
| Q4 | 1.2.2 | #1345 #1346 #1347 #1348 #1270 #1092 #1153 #1617 | documentation analyzers | P3 P8 P13 |
| Q5 | 1.2.2 | #1071 #1268 #1289 #974 | naming/design analyzers | P10 |
| Q6 | 1.2.2 | #1074 #1079 #1276 #1613 | layout/spacing singletons, RH5202 lone CR | P2 |
| Q7 | 1.2.2 | #1642 #1635 | formatter singletons | P2 P13 |

| Wave | Runs |
|---|---|
| W1 | P10 P12 P2 P3 P8 Q3 |
| W2 | P1 P4 P6 P7 Q5 Q6 |
| W3 | P5 P13 Q2 |
| W4 | P9 Q4 Q7 |
| W5 | P11 |
| W6 | Q1 |

Weak overlaps accepted: W1 P12/P10 (RH2001 fix calls CasingUtilities; rerun RH2001 tests after P10); W2 P1/P7 (RH7101 — if P7 must touch RH7101, move P1 to W3).
Premise checks for the runs (on the issues / run's own skeptic): #1140 & #1147 RH5110 instances after #1632; #1393 & #952 likely resolved; #1594 invalid input; #1598/#1617 unconfirmed; #1076 after #1618; #1604 RH3004 raw-string part is an extension; P13 output questions.

## Sequential order (maintainer 2026-11-07: #1600 stays in P11 (1A); runs one at a time, no parallel waves)

Formatter runs first (fixes that delegate to the formatter follow them), then 1.2.1 fixes by dependency, P11 last of 1.2.1, then 1.2.2.

| # | Run | Command | Status |
|---|---|---|---|
| 1 | P2 | `/squad #1243 #1383 #1149 #1391 #952 #1611` | merged #1644 |
| 2 | P3 | `/squad #1385 #1398 #1239` | merged #1657 |
| 3 | P1 | `/squad #963 #1384` | merged #1660 |
| 4 | P12 | `/squad #1595 #1593 #1616` | merged #1662 |
| 5 | P10 | `/squad #1597 #1598 #1016 #1015 #1084` | merged #1675 |
| 5a | P14 | `/squad #1666 #1667 #1665` | merged #1677 |
| 6 | P8 | `/squad #1269 #1342 #1343 #1459` | merged #1679 |
| 7 | P4 | `/squad #946 #972 #1096 #1076 #1626 #1625` | merged #1683 |
| 8 | P6 | `/squad #1140 #1141 #1144 #1145 #1094 #1393 #1511 #1561` | merged #1686 (Refs #1140, narrowed) |
| 9 | P7 | `/squad #969 #1335 #1263 #1151 #1604` | merged #1688 |
| 10 | P5 | `/squad #1416 #1388 #1627 #1142` | |
| 11 | P13 | `/squad #1529 #1543` | |
| 12 | P9 | `/squad #1283 #1284 #1285` | |
| 13 | P11 | `/squad #1600 #1150 #1274 #1143 #1673` | |
| 14 | Q3 | `/squad #966 #1105 #1266 #1267` | |
| 15 | Q5 | `/squad #1071 #1268 #1289 #974` | |
| 16 | Q6 | `/squad #1074 #1079 #1276 #1613` | |
| 17 | Q2 | `/squad #1264 #1333 #1334 #1339 #1337 #1412 #1592` | |
| 18 | Q4 | `/squad #1345 #1346 #1347 #1348 #1270 #1092 #1153 #1617` | |
| 19 | Q7 | `/squad #1642 #1635` | |
| 20 | Q1 | `/squad #1147 #1152 #1386 #1387 #1594 #1655` | |

## Sequential order (refreshed 2026-11-13)

Maintainer 2026-11-13: release 1.2.1 and 1.2.2 first; deferred minor bugs #1140 (remainder), #1676, #1680, #1687 leave the runs. Done: P1 P2 P3 P4 P6 P7 P8 P10 P12 P14.

| # | Run | MS | Command | Status |
|---|---|---|---|---|
| 1 | P5 | 1.2.1 | `/squad #1416 #1388 #1627 #1142` | merged #1691 |
| 2 | P13 | 1.2.1+1.2.2 | `/squad #1529 #1543` | merged #1697 |
| 3 | P9 | 1.2.1 | `/squad #1283 #1284 #1285` | next |
| 4 | P11 | 1.2.1 | `/squad #1600 #1150 #1274 #1143 #1673 #1678` | last of 1.2.1 |
| 5 | Q3 | 1.2.2 | `/squad #966 #1105 #1266 #1267` | after PR #1669 is merged or closed |
| 6 | Q6 | 1.2.2 | `/squad #1074 #1079 #1276 #1613` | |
| 7 | Q2 | 1.2.2 | `/squad #1264 #1333 #1334 #1339 #1337 #1412 #1592` | |
| 8 | Q4 | 1.2.2 | `/squad #1345 #1346 #1347 #1348 #1270 #1092 #1153 #1617` | |
| 9 | Q7 | 1.2.2 | `/squad #1642 #1635` | |
| 10 | Q1 | 1.2.2 | `/squad #1147 #1152 #1386 #1387 #1594 #1655` | |
| 11 | Q5 | 1.2.2 | `/squad #1071 #1268 #1289 #974` | merged #1692 (Refs #1289, remainder stays open) |
