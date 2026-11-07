# Phase 5b — triage proposal (open issues without a milestone)

Snapshot: 2026-11-07, `origin/main` at `d979713`. 37 open issues without a milestone, none with comments. Nothing has been written to GitHub.

**No issue was reproduced.** Every row is assessed from the issue's own text, plus a short look at `main` where marked. Several reports say they came from reading code (#1598, #1617) or give no sign that they were run.

## Milestones

| Group | Count | Recommended milestone | Other open milestones that could fit |
|---|---|---|---|
| Critical bug | 6 | `1.2.1` (`1.2.0` has 0 open issues and is about to be released) | `1.2.0` only if it is reopened for them |
| Minor bug | 11 | `1.2.1` or `1.2.2`, chosen per issue below | — |
| Extension | 12 | `vNext` | `1.4.0`, `1.5.0` |
| Maintenance | 8 | `Maintenance` | — |

## Issues

| # | Title | Group | Reason | Labels to add | Milestone |
|---|---|---|---|---|---|
| [#1597](https://github.com/thoenissen/Reihitsu/issues/1597) | Casing fixes: withheld where a rename rebinds a call or leaves a `[CallerArgumentExpression]` literal | **Critical bug** | The rename changes which member `foreach` or a query binds to, or empties `CallerArgumentExpression`, so the program behaves differently (the report says it was reproduced with `apply-fix.sh`). | — | 1.2.1 ⚠ |
| [#1595](https://github.com/thoenissen/Reihitsu/issues/1595) | RH7102: fix withheld when moving the `const` reorders layout fields/initializers in another build | **Critical bug** | The moved `#if DEBUG` block changes struct layout and initializer order in the DEBUG build, which changes executed logic. | — | 1.2.1 ⚠ |
| [#1593](https://github.com/thoenissen/Reihitsu/issues/1593) | RH7309: initializer-order/layout guards miss a member split by `#if` | **Critical bug** | The fix can reorder runtime initializers in another configuration, which changes executed logic (needs a contrived `#if` split). | — | 1.2.1 ⚠ |
| [#1616](https://github.com/thoenissen/Reihitsu/issues/1616) | RH2001: fix on `static private` keeps modifier gap and comments in place | **Critical bug** | It moves a user comment across `private` (SHARED-FIX-003), and the report says `main` duplicates `// c`; the leftover modifier gap alone would be minor. | — | 1.2.1 ⚠ |
| [#1600](https://github.com/thoenissen/Reihitsu/issues/1600) | Code fixes and whole-document formatting: text that is a string in another build is not edited | **Critical bug** | Editing the text changes a verbatim string's value in the other build; strictly critical, but the input is highly contrived. | — | 1.2.1 ⚠ |
| [#1598](https://github.com/thoenissen/Reihitsu/issues/1598) | Casing fixes: excluded-code check covers the other target framework's build (unconfirmed) | **Critical bug** (if it reproduces) | It would leave code that does not compile for the other target framework; found by reading code and never built. | — | 1.2.1 ⚠ (confirm first) |
| [#1627](https://github.com/thoenissen/Reihitsu/issues/1627) | RH5405–RH5407: trailing whitespace ends up behind the inserted `}` | Minor bug | Only cosmetic trailing whitespace; no code damage. | `formatter` | 1.2.1? (same fixes as #1416, #1388) |
| [#1625](https://github.com/thoenissen/Reihitsu/issues/1625) | RH3208: fix keeps the statement behind a multi-line comment on its own line | Minor bug | The output compiles and keeps the comment; only the indentation and the trailing whitespace are wrong. | — | 1.2.1? (RH3208 work in #1284) |
| [#1604](https://github.com/thoenissen/Reihitsu/issues/1604) | Code fixes that move code keep its layout: shared column-delta helper | Minor bug | Moved code is reformatted against SHARED-FIX-007, with no damage; the report shows no example. | — | 1.2.1? (overlaps #969, #1335) |
| [#1611](https://github.com/thoenissen/Reihitsu/issues/1611) | FMT-RAW: closing-quote whitespace width measured as the analyzer measures it | Minor bug | The output stays stable, compiles and keeps the value; RH5202 still reports it, and only with rare U+00A0-style whitespace (confirmed on `main`: `GetLeadingSpaceCount` counts only spaces). | — | 1.2.1? (raw-string alignment, #1391) |
| [#1642](https://github.com/thoenissen/Reihitsu/issues/1642) | Formatter removes the space behind a multi-line `*/` | Minor bug | A missing space is a layout difference only; the output is idempotent and compiles. | — | 1.2.2? |
| [#1635](https://github.com/thoenissen/Reihitsu/issues/1635) | FMT-BREAK-012: argument list wrapped in front of a comma → one element per line | Minor bug | The layout differs from FMT-BREAK-012's "wrapped between two elements" and does no damage. | — | 1.2.2? (overlaps vNext #956) |
| [#1617](https://github.com/thoenissen/Reihitsu/issues/1617) | Documentation rules: implementing part of a partial member judged as implementing (unconfirmed) | Minor bug | A possible false positive on a rare shape (a partial method that implements an interface) that adding documentation clears; not built. | — | 1.2.2? (doc-rule partial work in #1092, #1153) |
| [#1613](https://github.com/thoenissen/Reihitsu/issues/1613) | RH5202: analyzer and fix find the closing line the same way (lone `\r`) | Minor bug | A false report and a fix that makes no progress, only in documents that use lone-`\r` line breaks. | — | 1.2.2? |
| [#1594](https://github.com/thoenissen/Reihitsu/issues/1594) | RH7304: directive whose region's first token does not start its line is not reported | Minor bug | One extra report on a rare shape, with no fix offered; the input as written may not be valid C# (see doubts). | — | 1.2.2? |
| [#1592](https://github.com/thoenissen/Reihitsu/issues/1592) | RH7309: fix not offered where it changes nothing | Minor bug | A no-op fix (SHARED-FIX-005) that does no damage. | — | 1.2.2? (RH7309 work in #1264) |
| [#1639](https://github.com/thoenissen/Reihitsu/issues/1639) | Formatter: wrap between `where` and the type parameter is joined (output question) | Extension | New formatter output for a gap that no requirement owns. | — | vNext |
| [#1638](https://github.com/thoenissen/Reihitsu/issues/1638) | New rule: constraint-clause colon spacing | Extension | A new analyzer rule with a code fix (P11 new-rule candidate). | — | vNext |
| [#1637](https://github.com/thoenissen/Reihitsu/issues/1637) | New rule: constraints of a wrapped clause one per line | Extension | A new analyzer rule with a code fix (P11 new-rule candidate). | — | vNext |
| [#1636](https://github.com/thoenissen/Reihitsu/issues/1636) | FMT-BREAK-023, RH5110: extension-block constraint clause starts its own line | Extension | A behavior change of the formatter and RH5110 (P11, the rule's own gap). | — | vNext |
| [#1634](https://github.com/thoenissen/Reihitsu/issues/1634) | Formatter: column for comment-held line in angle-bracket list / qualified name | Extension | New formatter output for lines that no requirement owns. | — | vNext |
| [#1633](https://github.com/thoenissen/Reihitsu/issues/1633) | Formatter: base list wrapped behind its colon is joined | Extension | New formatter output (output question, needs a change doc). | — | vNext |
| [#1628](https://github.com/thoenissen/Reihitsu/issues/1628) | RH5023: blank lines inside an interpolation hole (output question) | Extension | RH5023 would report more (behavior change). | — | vNext |
| [#1612](https://github.com/thoenissen/Reihitsu/issues/1612) | RH5202: whitespace-only hole-code line written empty or shifted (output question) | Extension | A change to the fix output that no requirement decides yet. | — | vNext |
| [#1605](https://github.com/thoenissen/Reihitsu/issues/1605) | FMT-PIPE: layout-only mode for the node-level entry point | Extension | Its lane is behavior-change: a new entry-point mode and a decided `///` column in RH3004 output (the existing `maintenance` label stays). | `enhancement`, `analyzer` | vNext |
| [#1602](https://github.com/thoenissen/Reihitsu/issues/1602) | RH3106: array elements decided by the target their value reaches | Extension | RH3106 would report and fix differently (it closes the gap RH3106-003 records). | — | vNext |
| [#1601](https://github.com/thoenissen/Reihitsu/issues/1601) | RH3004: fix offered for anonymous methods with directives/strings/comments | Extension | The fix would be offered for inputs it withholds today, which is not a defect under Code-fix coverage. | — | vNext |
| [#1590](https://github.com/thoenissen/Reihitsu/issues/1590) | RH7003: report with a nested namespace only in an excluded `#if` (output question) | Extension | RH7003 would report less (behavior change). | — | vNext |
| [#1641](https://github.com/thoenissen/Reihitsu/issues/1641) | TokenGapUtilities.HasLineBreakBetween summary correction | Maintenance | A code-comment correction that is text only (the wrong summary is still on `main`). | — | Maintenance |
| [#1640](https://github.com/thoenissen/Reihitsu/issues/1640) | ChainLineBreakRewriter: batch gap joins through `JoinGaps` | Maintenance | A refactor with unchanged output, despite the bug-fix lane. | — | Maintenance |
| [#1626](https://github.com/thoenissen/Reihitsu/issues/1626) | Code fixes: "blank line in front of a statement" has one owner | Maintenance | Moves one decision to a single owner; the visible part is #1625. | — | Maintenance |
| [#1615](https://github.com/thoenissen/Reihitsu/issues/1615) | SHARED-DIAG-004 wording (clarification) | Maintenance | A spec clarification with no behavior change. | `maintenance`, `analyzer` | Maintenance |
| [#1607](https://github.com/thoenissen/Reihitsu/issues/1607) | RH3004: spec and test three layout shapes | Maintenance | Spec text and tests for current behavior (item 3 might turn up a bug). | — | Maintenance |
| [#1606](https://github.com/thoenissen/Reihitsu/issues/1606) | SDD: RH3001/3005/3101/3105/3201 specs state safe inputs (clarification) | Maintenance | Spec clarifications plus tests (item 2 hides an output question, see doubts). | — | Maintenance |
| [#1603](https://github.com/thoenissen/Reihitsu/issues/1603) | Reihitsu.Core: RH3002 uses the shared parse check | Maintenance | A refactor with existing tests unchanged. | — | Maintenance |
| [#1591](https://github.com/thoenissen/Reihitsu/issues/1591) | RH7003 rule page: remove "Exceptions" section | Maintenance | A rule-page convention cleanup with no behavior change (the section is still on `main`). | — | Maintenance |
| [#1589](https://github.com/thoenissen/Reihitsu/issues/1589) | Code fixes: audit every fix against SHARED-FIX-003 | Maintenance | An audit that files or fixes separate defects, not a defect of its own. | — | Maintenance |

⚠ = a critical bug. It is flagged because `1.2.0` is complete, so it goes into `1.2.1`.

`agent-filed` stays on every issue, and no label is removed.

## Duplicates, overlaps and doubts

- **#1603 overlaps #877** (Maintenance), item 1 "exact re-parse check" for RH3002. Consider merging #1603 into #877, or narrowing #877.
- **#1604 overlaps #969** (1.2.1, "member-ordering code fixes reformat the whole containing type") **and #1335** (1.2.1, RH7004 changes only the node it moves). What #1604 adds is the shared helper and the RH3004 raw-string fix, which is an extension.
- **#1635 overlaps #956** (vNext, item 2: a leading comma in an argument list). Decide whether it is a bug against FMT-BREAK-012 (1.2.x) or part of the feature #956.
- **#1625 and #1626** belong together: the refactor in #1626 is the planned way to fix #1625.
- **#1627** is in the same fixes and code path as #1416 and #1388 (1.2.1).
- **#1633** is related to #1391 (1.2.1, base-list continuation) but distinct: it places the first base type, while #1391 aligns the later ones.
- **#1636** is related to #1276 and #1446 (extension-block handling in RH5404), but it is not a duplicate.
- **#1641 and #1642** are related to each other and to #1571 and #1572 (gaps behind a multi-line comment's closing line).
- **#1637, #1638 and #1639** say they are "blocked by #1631". #1631 is closed and merged as #1632 (`d979713` on `main`), so all three are unblocked. As a side note, item 2 of vNext #955 (a wrapped constraint clause) may be resolved by #1632.
- **#1594: the premise is doubtful.** The input `/* c */ #region Fields` puts a comment in front of `#` on the same line, which C# rejects (CS1040: a directive must be the first non-whitespace on its line). The intended input is probably `/* c */ int _a;` as the first line inside the region. Clarify this before implementing.
- **#1606**, item 2 (`/**/+1;` → `+1;` at column 0), looks like a minor RH3201 fix bug inside a clarification issue. Split it out if it is confirmed.
- **#1605** carries `maintenance` but its lane is behavior-change, so it is grouped as an Extension. Both labels stay.
- **#1600, #1593 and #1595** are critical by the letter of the criteria (literal value, initializer order or layout in another `#if` build), but each needs `#if` shapes that are unusual in practice. The scope of #1600 (a Core helper consulted by every fix and by the formatter) is large for a patch release. You may want to downgrade #1600 to minor or 1.2.2.
- **#1598 and #1617** are unconfirmed and were derived from reading code. Each needs a reproducing test first, and is closed as a characterization test if it does not reproduce.
- **Already resolved on `main`:** none of the cheaply checkable issues is. #1591, #1611, #1640 and #1641 still match `main`.
- **Below C# 12:** none of the issues matters only below C# 12.

## Answer format

> `ok` — apply everything as proposed.
> `ok, minor bugs → 1.2.2` — change one group's milestone.
> `ok, #1600 → 1.2.2 Minor, #1635 → vNext` — override single issues.
