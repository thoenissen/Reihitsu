# Coordinator state

Active milestone: **1.6.0** (milestone 14). Branch: `coordinator`. Theme: T12 continuation columns and indentation; T2 rest (SHARED-DIAG-008 follow-ups, directive guards in fixes); T8 naming and casing; #1566, #975.

Milestone numbers: 1.6.0 = 14, 1.7.0 = 15, 1.8.0 = 16, 1.9.0 = 17, 1.10.0 = 18, 1.11.0 = 19, vNext = 2, Maintenance = 4. Released: v1.5.0 (on the bump commit 5af53bd, PR #1919).

## Decisions

- 2026-11-22 · #1935 #1936 #1937 #1939 #1943 · triage "as you recommend": #1935 #1936 #1939 #1943 → 1.6.0, #1937 → 1.7.0; #1935 A (blank line before positional `)` removed), #1939 A (dictated positional parameter not reported), #1943 A (RH2001 fix withheld for an adjacent `#if` group); #1936 rides with #1472.
- 2026-11-22 · #1924 #1925 #1926 #1927 #1928 #1929 #1930 #1931 · placements as proposed (1A): #1924 #1925 #1926 #1929 → 1.6.0; #1927 → 1.7.0; #1928 → 1.9.0; #1930 #1931 → 1.10.0.
- 2026-11-22 · #1925 · RH5202 reports and fixes raw strings aligned behind tabs (A).
- 2026-11-22 · #1926 · declare unsupported, change of its own (A, re-confirmed).
- 2026-11-22 · #1928 · one space behind `case` in front of `(` (A).
- 2026-11-21 · #1088 · local constants follow camelCase (option A); the run is ordinary: `#1087 #1088` after #1090 merges.
## Plan

Open in 1.6.0 (29): #1085 #1086 #1087 #1088 #1089 #1090 #1275 #1399 #1400 #1472 #1566 #1567 #1568 #1589 #1607 #1698 #1722 #1771 #1836 #1837 #1912 #1924 #1925 #1926 #1935 #1936 #1939 #1941 #1943.

Merged since the previous batch: #1933 (#1542 #1633 #1634 #1647 #1685 #1929), #1934 (#971 #1072 #1434 #1451), #1938 (#1240 #1591), #1940 (#970 #1668).

Runs in progress: #1941 (PR #1942, Playground files, `playground.md`; blocks nothing).

Dependency graph: `inventory/batch-2026-11-22b.md` (main at 1788f94d).

Start now: `#1926` · `#1566` · `#1935` · `#1939` · `#1943` · `#1275 #1837` · `#1472` · `#1722` · `#1925` · `#1924` · `#1912` · `#1836` · `#1086` · `#1089` · `#1090`.

After merges:
- after #1926 and #1566 → `#1568 #1698 #1567` → `#1399` → `#1400 #1936` (serial on `indentation.md`)
- after #1912 → `#1607`
- after #1086 → `#1085`
- after #1090 (and #1925 if it rewords RH5202's message, #1924 if it adds a spec) → `#1087 #1088`
- after #1939 and #1943 → `#1771` (escalation 9 first)
- after #1836, #1275 #1837, #1926, #1943 → `#1589` alone (or 1.7.0)

Notes for the runs (evidence in the batch file, not decided): #1275 likely narrows to `class C⏎{⏎};` (item 2 decided otherwise by SHARED-EMPTY-001, item 3 holds); #1567 partly done (return-type gap remains); #1771's file pointer is stale (`CasingRenameSafetyUtilities.cs`). #1936 moved from the #1472 run to the #1400 run (`indentation.md` shared with #1926; comment on #1936).

## Status

Status 2026-11-22 (l): batch — merges #1933 #1934 #1938 #1940 read; graph rebuilt; #1936 moved to the #1400 run; start-now set given (15 runs).
Status 2026-11-22 (k): triage of #1935–#1943 applied as recommended (`inventory/triage-2026-11-22b.md`); milestones set; commented on all five. Batch next in the same session.
Status 2026-11-22 (j): batch — merges #1920 #1921 #1922 #1923 #1932 read; graph rebuilt; start-now set given (13 runs).
Status 2026-11-22 (i): triage answered "as recommended" (1A, 2A, 3A, 4A); milestones set; commented on #1925 #1926 #1928 #1929.
Status 2026-11-22 (h): triage of #1924–#1931 (assessment `inventory/triage-2026-11-22.md`). Asked: 1 placements — #1924 → 1.6.0 own tooling run, start now; #1925 → 1.6.0 own RH5202 run, start now; #1926 → 1.6.0 own precheck run after the list/pattern run (`indentation.md`); #1929 → 1.6.0 joins `#1633 #1634 #1647 #1685 #1542`; #1927 → 1.7.0 unowned joins; #1928 → 1.9.0 spacing; #1930 → 1.10.0 counterpart rules; #1931 → 1.10.0 RH51xx list scope (1A apply / 1B change rows; rec. 1A). 2 #1925 RH5202 tab-aligned raw string (A report+fix, B not reported; rec. A). 3 #1926 re-confirm declare unsupported, own change (A declare, B keep continuation indent; rec. A). 4 #1928 `case (` space (A space, B keep; rec. A). Output questions of #1927, #1930, #1931 deferred to their runs (unverified Before / asked with their run's sibling questions). Side find: #1645 may be shipped by #1920, judged in its 1.7.0 run.
Status 2026-11-21 (g): #1088 answered A (camelCase); commented on #1088.
Status 2026-11-21 (f): start-now set given. Asked: #1088 casing of local constants — A camelCase (no churn), B PascalCase (issue's starting point, RH4110/.NET, renames ~870 test files, run alone); recommended A.
Status 2026-11-21 (e): started 1.6.0; main merged into `coordinator`; dependency graph delegated.
Status 2026-11-21 (d): finished 1.5.0 (close-out after the fact: #1917 merged, bump #1919 merged, release v1.5.0 published; no inventory left; roadmap has no 1.5.0 section, #1880 already in 1.6.0). Milestone 7 (1.5.0) still open on GitHub with 0 open issues: the maintainer closes it.

## Next

Maintainer starts the start-now runs. After merges: `/coordinator batch`.
