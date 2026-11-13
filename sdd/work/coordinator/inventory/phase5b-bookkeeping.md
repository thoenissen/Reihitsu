# Phase 5b bookkeeping — 2026-11-07

All 37 issues updated (milestone + full label list, no label removed). Triage comments posted where listed. No failures.
Verification: open issues without a milestone after the run: none.

| Issue | Milestone | Labels added | Comment | Failures |
|---|---|---|---|---|
| #1597 | 1.2.1 | — | no | — |
| #1595 | 1.2.1 | — | yes (critical) | — |
| #1593 | 1.2.1 | — | yes (critical) | — |
| #1616 | 1.2.1 | — | no | — |
| #1600 | 1.2.1 | — | yes (critical) | — |
| #1598 | 1.2.1 | — | yes (unconfirmed) | — |
| #1627 | 1.2.1 | formatter | no | — |
| #1625 | 1.2.1 | — | yes (#1626 is planned fix) | — |
| #1604 | 1.2.1 | — | no | — |
| #1611 | 1.2.1 | — | no | — |
| #1626 | 1.2.1 | — (labels kept) | yes (planned fix of #1625) | — |
| #1642 | 1.2.2 | — | no | — |
| #1635 | 1.2.2 | — | yes (FMT-BREAK-012, related #956) | — |
| #1617 | 1.2.2 | — | yes (unconfirmed) | — |
| #1613 | 1.2.2 | — | no | — |
| #1594 | 1.2.2 | — | yes (CS1040 input) | — |
| #1592 | 1.2.2 | — | no | — |
| #1639 | vNext | — | no | — |
| #1638 | vNext | — | no | — |
| #1637 | vNext | — | no | — |
| #1636 | vNext | — | no | — |
| #1634 | vNext | — | no | — |
| #1633 | vNext | — | no | — |
| #1628 | vNext | — | no | — |
| #1612 | vNext | — | no | — |
| #1602 | vNext | — | no | — |
| #1601 | vNext | — | no | — |
| #1590 | vNext | — | no | — |
| #1605 | vNext | enhancement, analyzer | no | — |
| #1641 | Maintenance | — | no | — |
| #1640 | Maintenance | — | no | — |
| #1607 | Maintenance | — | no | — |
| #1606 | Maintenance | — | no | — |
| #1603 | Maintenance | — | yes (overlap with #877) | — |
| #1591 | Maintenance | analyzer (RH7003 rule page) | no | — |
| #1589 | Maintenance | — | no | — |
| #1615 | Maintenance | maintenance, analyzer | no | — |

Extra: comment on #877 (cross-link to #1603, item 1).

## 2026-11-09

- #1666, #1667, #1665: milestone 1.2.1 (12); labels unchanged (bug, analyzer, agent-filed already present); triage comment posted (critical, own run together).
- #1668: milestone vNext (2); added `enhancement` (kept analyzer, maintenance, agent-filed); triage comment posted (closer analysis; refuse any conditional block if the precise check is non-trivial).

## 2026-11-09 b

Maintainer-approved bookkeeping; labels read before each update, full list passed, none removed.

- #1673 → 1.2.1 (`bug` already present).
- #1655 → 1.2.2 (`bug` already present); triage comment posted (overlap #1387, declarator example now refused by FMT-UNSUP-004, enum-member example remains).
- vNext (`enhancement` already present on all): #1645 #1646 #1647 #1648 #1652 #1653 #1658 #1659 #1649 #1650; `documentation` added on #1649 and #1650.
- Maintenance (`maintenance` already present on all): #1651 #1654 #1661 #1672.
- #1664 closed as duplicate of #1339 (milestone 1.2.2 set): its input (field outside every region vs. constant in a region) and the `NoCodeFixWhenDirectivesAreInLeadingTrivia` test note carried over in a comment on #1339; closing comment on #1664.
- #1656 closed as duplicate of #1545 with comment (no milestone set).
- Re-read: open issues without a milestone (PRs excluded): 0.

## 2026-11-13

- #1678 → 1.2.1 (12); triage comment posted (critical: fix reorders instance fields, layout changes in another build configuration; runs with #1150).
- #1681, #1685, #1689 → vNext (2).
- #1682, #1684, #1690 → Maintenance (4).
- Labels passed unchanged on every update; none added or removed.
- Not touched (awaiting milestone decision): #1676, #1680, #1687, #1140.
