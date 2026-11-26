# Coordinator state

Active milestone: **1.6.0** (milestone 14). Branch: `coordinator`. Theme: T12 continuation columns and indentation; T2 rest (SHARED-DIAG-008 follow-ups, directive guards in fixes); T8 naming and casing; #1566, #975.

Milestone numbers: 1.6.0 = 14, 1.7.0 = 15, 1.8.0 = 16, 1.9.0 = 17, 1.10.0 = 18, 1.11.0 = 19, vNext = 2, Maintenance = 4. Released: v1.5.0 (on the bump commit 5af53bd, PR #1919).

## Decisions

- 2026-11-26 · #1960 · 3C: the formatter removes a `;` behind a type's closing brace (new structural transform); 4A: with a comment, directive, or disabled text between `}` and `;` the `;` stays, nothing indented, the blank line in front of the comment stays (FMT-BLANK-011). Commented on #1960 and #1959 (RH5404 fix result without `;`).
- 2026-11-26 · #1956–#1963 · triage 1A: #1956 #1958 #1963 → 1.6.0 (`/squad #1956 #1958 #1963`, start now); #1957 #1961 → 1.8.0 (shared-line run); #1959 #1960 → 1.7.0 (extension-block run); #1962 closed as duplicate of #962 (input added to #962). #1959: 2A (RH5404 reports and fixes `class C { int _x; };`). #1960: questions 3/4 open — the maintainer asked whether the formatter could simply remove the `;`.
- 2026-11-26 · #1948 #1949 #1950 · triage 1A: #1948 → 1.6.0 (own analyzer run, start now); #1949 → 1.6.0, joins #1924 (`/squad #1924 #1949`); #1950 → 1.7.0, own Playground run after #976; commented on #1949 #1950.
- 2026-11-22 · #1935 #1936 #1937 #1939 #1943 · triage "as you recommend": #1935 #1936 #1939 #1943 → 1.6.0, #1937 → 1.7.0; #1935 A (blank line before positional `)` removed), #1939 A (dictated positional parameter not reported), #1943 A (RH2001 fix withheld for an adjacent `#if` group); #1936 rides with #1472.
- 2026-11-22 · #1924 #1925 #1926 #1927 #1928 #1929 #1930 #1931 · placements as proposed (1A): #1924 #1925 #1926 #1929 → 1.6.0; #1927 → 1.7.0; #1928 → 1.9.0; #1930 #1931 → 1.10.0.
- 2026-11-22 · #1925 · RH5202 reports and fixes raw strings aligned behind tabs (A).
- 2026-11-22 · #1926 · declare unsupported, change of its own (A, re-confirmed).
- 2026-11-22 · #1928 · one space behind `case` in front of `(` (A).
- 2026-11-21 · #1088 · local constants follow camelCase (option A); the run is ordinary: `#1087 #1088` after #1090 merges.
## Plan

Open in 1.6.0 (23): #1085 #1086 #1087 #1088 #1089 #1090 #1399 #1400 #1567 #1568 #1589 #1607 #1698 #1771 #1836 #1912 #1924 #1936 #1948 #1949 #1956 #1958 #1963.

Merged since the previous batch: #1944 (#1926), #1951 (#1943), #1952 (#1722), #1953 (#1925), #1954 (#1275), #1955 (#1837), #1964 (cleanup). #1472 closed by the maintainer without a PR.

Runs in progress: none (no PR open). The start-now runs below have no claim PR and are treated as not started.

Dependency graph: `inventory/batch-2026-11-26.md` (main at 68f236ea).

Start now: `#1956 #1958 #1963` · `#1924 #1949` · `#1912` · `#1836` · `#1771` · `#1086` · `#1089` · `#1090` · `#1948`.

After merges:
- after #1956 #1958 #1963 and #1912 → `#1568 #1698 #1567` (shares `PrecheckPhase.cs` with #1963 via #1567 option A, and `RH5112…AnalyzerTests.cs` with #1912 via #1568 option B) → `#1399` → `#1400 #1936` (serial on `indentation.md`)
- after #1912 → `#1607`
- after #1086 → `#1085`
- after #1090 and #1771 (and #1924 #1949 only if it adds a spec) → `#1087 #1088` (new link: `casing.md`, and the new rule's fix derives from `CasingCodeFixProviderBase<T>`, which #1771 restructures)
- after #1836 → `#1589`

Notes for the runs (evidence in the batch file, not decided): #1567 narrows to an explicit return type (`int⏎(int x) => x`); `static⏎(` is already joined. #1698's Before and option A are stale on main (`// e`, `a`, `}` at column 12, `// f` at 16 with a blank line); the run re-asks. #1568 holds. #1771 holds; escalation 9 comes first inside its run. #1912's search also hits `CodeFixRunCommandTests.cs` and `ApplyFixWrapperPositionalArgumentTests.cs`, which hold no message literals, so they are no shared file with #1924 #1949. #1836's output question is still open.

## Status

Status 2026-11-26 (t): batch finished — merges #1944 #1951–#1955 #1964 read (#1926 #1943 #1722 #1925 #1275 #1837 done; #1472 closed by the maintainer); graph rebuilt (`inventory/batch-2026-11-26.md`, main at 68f236ea); #1771 joins the start-now set (10 runs); `#1568 #1698 #1567` now wait for #1956 #1958 #1963 and #1912; #1087 #1088 also wait for #1771.
Status 2026-11-26 (s): 3C, 4A recorded (maintainer asked about the blank line in 4A: FMT-BLANK-011 keeps it, applied so); commented on #1960 #1959. Batch started: merges #1944 #1951–#1955 #1964 read, no PR open; re-check of `#1568 #1698 #1567`, #1771, #1589 and the start-now conflicts delegated (`inventory/batch-2026-11-26.md`).
Status 2026-11-26 (r): answer 1A, 2A applied (milestones, comments on #962 #1956 #1957 #1959 #1961 #1962, #1962 closed as duplicate). Asked back 3/4 (#1960) with a new option: 3A `};` joined, 3B `}⏎;` at column 0, 3C remove the `;` behind a type's closing brace (new structural transform, like FMT-STRUCT-019's comma removal; rec. C); 4 with comment/directive between `}` and `;`: 4A keep as written at column 0, no blank line, the `;` stays (rec.), 4B remove the `;` anyway, comment stays on its line.
Status 2026-11-26 (q): triage of #1956–#1963 asked (`inventory/triage-2026-11-26.md`, main at 76b09a12). 1 placements — #1956 #1958 #1963 → 1.6.0 one maintenance run `/squad #1956 #1958 #1963`, start now; #1957 #1961 → 1.8.0 join `#1773 #1775 #1447 #1443 #1758` (shared `BlankLineCodeFixUtilities.cs`); #1959 #1960 → 1.7.0 join `#1636 #1785 #1446 #1845` (RH5404 files shared with #1446); #1962 → close as duplicate of #962, input added to #962 (1A apply / 1B change rows; rec. 1A). 2 RH5404 `class C { int _x; };` reported+fixed (A) or not (B); rec. A. 3 formatter `}⏎;` → `};` (A) or `}⏎;` kept (B), today indented; rec. A. 4 comment/directive between `}` and `;`: kept as written at column 0, no blank line (A) or as main (B); rec. A. Side find: six PRs merged today (#1944 #1951 #1952 #1953 #1954 #1955) — batch after the answer.
Status 2026-11-26 (p): triage of #1956–#1963 started; assessment delegated (`inventory/triage-2026-11-26.md`). Nothing asked yet.
Status 2026-11-26 (o): batch — merges #1942 #1945 #1946 #1947 read; #1566 #1939 dropped as predecessors; #1948 and `#1924 #1949` added to the start-now set (13 runs); nothing newly unblocked (the chain waits for #1926, #1771 for #1943).
Status 2026-11-26 (n): triage answered 1A; milestones set (#1948 #1949 → 1.6.0, #1950 → 1.7.0); commented on #1949 #1950.
Status 2026-11-26 (m): triage of #1948 #1949 #1950 (no inventory file; three issues read directly). Asked: 1 placements — #1948 → 1.6.0, own analyzer run, start now (gap left by #1945: the RH5301/RH5303/RH5206 fixes make no progress below C# 11 while the diagnostic stays); #1949 → 1.6.0, joins #1924 as `/squad #1924 #1949` (both trace/fixture tooling, #1924 not started); #1950 → 1.7.0, own Playground run after #976 (`Home.razor`) (1A apply / 1B change rows; rec. 1A). No output questions.
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

On reports of merges: `/coordinator batch`. On new issues without a milestone: `/coordinator triage`.
