# Coordinator state

Active milestone: **1.6.0** (milestone 14). Branch: `coordinator`. Theme: T12 continuation columns and indentation; T2 rest (SHARED-DIAG-008 follow-ups, directive guards in fixes); T8 naming and casing; #1566, #975.

Milestone numbers: 1.6.0 = 14, 1.7.0 = 15, 1.8.0 = 16, 1.9.0 = 17, 1.10.0 = 18, 1.11.0 = 19, vNext = 2, Maintenance = 4. Released: v1.5.0 (on the bump commit 5af53bd, PR #1919).

## Decisions

- 2026-11-21 · #1088 · local constants follow camelCase (option A); the run is ordinary: `#1087 #1088` after #1090 merges.
## Plan

Open in 1.6.0 (41): #953 #970 #971 #975 #1072 #1085 #1086 #1087 #1088 #1089 #1090 #1240 #1275 #1399 #1400 #1432 #1434 #1451 #1472 #1542 #1566 #1567 #1568 #1589 #1591 #1607 #1633 #1634 #1647 #1668 #1685 #1698 #1722 #1771 #1818 #1836 #1837 #1852 #1880 #1896 #1912. All are in the roadmap's 1.6.0 runs.

Carried over: `/squad #1880` (waited for the v1.5.0 tag; the tag exists).

Runs in progress: none.

Dependency graph: `inventory/plan-1.6.0.md` (roadmap runs split by lane; the blanket "formatter waits on #953" narrowed to the runs that share `indentation.md`/`line-breaks.md`/`structural-transforms.md` or a logical link).

Start now: `#953 #1852 #1896 #1818` · `#1880` · `#975` · `#1432` · `#1566` · `#1472` · `#1912` · `#1240 #1591` · `#1836` · `#970 #1668` · `#971 #1072 #1451 #1434` · `#1086` · `#1089` · `#1090`.

After merges:
- after #953 → `#1633 #1634 #1647 #1685 #1542`
- after #953 and #1240 #1591 → `#1275 #1837`
- after #953 and #1912 → `#1607`
- after the list/pattern run (and #1566) → `#1568 #1698 #1567` → `#1399` → `#1400` (serial on `indentation.md`)
- after #1432 → `#1722`; after #1086 → `#1085`; after the casing run → `#1771` (escalation 9 first)
- after #1090 → `#1087 #1088` (camelCase decided)
- after #953, #1836, #1275 #1837, #970 #1668 → `#1589` alone (or 1.7.0)

## Status

Status 2026-11-21 (g): #1088 answered A (camelCase); commented on #1088.
Status 2026-11-21 (f): start-now set given. Asked: #1088 casing of local constants — A camelCase (no churn), B PascalCase (issue's starting point, RH4110/.NET, renames ~870 test files, run alone); recommended A.
Status 2026-11-21 (e): started 1.6.0; main merged into `coordinator`; dependency graph delegated.
Status 2026-11-21 (d): finished 1.5.0 (close-out after the fact: #1917 merged, bump #1919 merged, release v1.5.0 published; no inventory left; roadmap has no 1.5.0 section, #1880 already in 1.6.0). Milestone 7 (1.5.0) still open on GitHub with 0 open issues: the maintainer closes it.

## Next

Maintainer starts the start-now runs. After merges: `/coordinator batch`.
