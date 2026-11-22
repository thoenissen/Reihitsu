# Coordinator state

Active milestone: **1.6.0** (milestone 14). Branch: `coordinator`. Theme: T12 continuation columns and indentation; T2 rest (SHARED-DIAG-008 follow-ups, directive guards in fixes); T8 naming and casing; #1566, #975.

Milestone numbers: 1.6.0 = 14, 1.7.0 = 15, 1.8.0 = 16, 1.9.0 = 17, 1.10.0 = 18, 1.11.0 = 19, vNext = 2, Maintenance = 4. Released: v1.5.0 (on the bump commit 5af53bd, PR #1919).

## Decisions

- 2026-11-22 · #1924 #1925 #1926 #1927 #1928 #1929 #1930 #1931 · placements as proposed (1A): #1924 #1925 #1926 #1929 → 1.6.0; #1927 → 1.7.0; #1928 → 1.9.0; #1930 #1931 → 1.10.0.
- 2026-11-22 · #1925 · RH5202 reports and fixes raw strings aligned behind tabs (A).
- 2026-11-22 · #1926 · declare unsupported, change of its own (A, re-confirmed).
- 2026-11-22 · #1928 · one space behind `case` in front of `(` (A).
- 2026-11-21 · #1088 · local constants follow camelCase (option A); the run is ordinary: `#1087 #1088` after #1090 merges.
## Plan

Open in 1.6.0 (45, numbers as of 2026-11-21 plus triage): #953 #970 #971 #975 #1072 #1085 #1086 #1087 #1088 #1089 #1090 #1240 #1275 #1399 #1400 #1432 #1434 #1451 #1472 #1542 #1566 #1567 #1568 #1589 #1591 #1607 #1633 #1634 #1647 #1668 #1685 #1698 #1722 #1771 #1818 #1836 #1837 #1852 #1880 #1896 #1912 #1924 #1925 #1926 #1929. All are in the roadmap's 1.6.0 runs. Merged since the start-now set (not yet re-derived by `batch`): #1920 (#953), #1922 (#975), #1923 (#1432).

Carried over: `/squad #1880` (waited for the v1.5.0 tag; the tag exists).

Runs in progress: none.

Dependency graph: `inventory/plan-1.6.0.md` (roadmap runs split by lane; the blanket "formatter waits on #953" narrowed to the runs that share `indentation.md`/`line-breaks.md`/`structural-transforms.md` or a logical link).

Start now (as given 2026-11-21; triage adds `#1924`, `#1925`): `#953 #1852 #1896 #1818` · `#1880` · `#975` · `#1432` · `#1566` · `#1472` · `#1912` · `#1240 #1591` · `#1836` · `#970 #1668` · `#971 #1072 #1451 #1434` · `#1086` · `#1089` · `#1090`.

After merges:
- after #953 → `#1633 #1634 #1647 #1685 #1542 #1929`
- after `#1633 #1634 #1647 #1685 #1542 #1929` → `#1926` (`indentation.md`)
- after #953 and #1240 #1591 → `#1275 #1837`
- after #953 and #1912 → `#1607`
- after the list/pattern run (and #1566) → `#1568 #1698 #1567` → `#1399` → `#1400` (serial on `indentation.md`)
- after #1432 → `#1722`; after #1086 → `#1085`; after the casing run → `#1771` (escalation 9 first)
- after #1090 → `#1087 #1088` (camelCase decided)
- after #953, #1836, #1275 #1837, #970 #1668 → `#1589` alone (or 1.7.0)

## Status

Status 2026-11-22 (i): triage answered "as recommended" (1A, 2A, 3A, 4A); milestones set; commented on #1925 #1926 #1928 #1929.
Status 2026-11-22 (h): triage of #1924–#1931 (assessment `inventory/triage-2026-11-22.md`). Asked: 1 placements — #1924 → 1.6.0 own tooling run, start now; #1925 → 1.6.0 own RH5202 run, start now; #1926 → 1.6.0 own precheck run after the list/pattern run (`indentation.md`); #1929 → 1.6.0 joins `#1633 #1634 #1647 #1685 #1542`; #1927 → 1.7.0 unowned joins; #1928 → 1.9.0 spacing; #1930 → 1.10.0 counterpart rules; #1931 → 1.10.0 RH51xx list scope (1A apply / 1B change rows; rec. 1A). 2 #1925 RH5202 tab-aligned raw string (A report+fix, B not reported; rec. A). 3 #1926 re-confirm declare unsupported, own change (A declare, B keep continuation indent; rec. A). 4 #1928 `case (` space (A space, B keep; rec. A). Output questions of #1927, #1930, #1931 deferred to their runs (unverified Before / asked with their run's sibling questions). Side find: #1645 may be shipped by #1920, judged in its 1.7.0 run.
Status 2026-11-21 (g): #1088 answered A (camelCase); commented on #1088.
Status 2026-11-21 (f): start-now set given. Asked: #1088 casing of local constants — A camelCase (no churn), B PascalCase (issue's starting point, RH4110/.NET, renames ~870 test files, run alone); recommended A.
Status 2026-11-21 (e): started 1.6.0; main merged into `coordinator`; dependency graph delegated.
Status 2026-11-21 (d): finished 1.5.0 (close-out after the fact: #1917 merged, bump #1919 merged, release v1.5.0 published; no inventory left; roadmap has no 1.5.0 section, #1880 already in 1.6.0). Milestone 7 (1.5.0) still open on GitHub with 0 open issues: the maintainer closes it.

## Next

`/coordinator batch`: #953, #975 and #1432 have merged since the start-now set; re-derive it (including #1924, #1925, and the list/pattern run with #1929).
