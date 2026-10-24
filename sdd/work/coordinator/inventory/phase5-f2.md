# Phase 5 — family F2 inventory (formatter: line breaks, spacing, indentation, raw strings, member-access chains)

25 issues. Checked against `sdd/specs/` on main after #1429; no build or test run. Principles as in `state.md` (P7, P11, P12, P33 = SHARED-COMMENT-012, P34 = FMT-PIPE-014, P35). Both #1380 decisions are on main: SHARED-COMMENT-012 (P33) and FMT-PIPE-014 (P34). Issues that say they are blocked by #1380 are therefore unblocked, and some of their instances are now plain bugs.

## 1. Table

| # | Title (short) | Labels | Milestone | Kind | Spec reqs / rules / phases | Blocked by | Principle | Duplicate/overlap | Template? | Proposed | Note |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 888 | RH5201 link behind multi-line comment tail, fix no progress | bug, analyzer | 1.2.0 | bug | SHARED-COMMENT-012, SHARED-FIX-001, RH5201, chain line breaks | with #1382 | P33, P7 | #1382 (same shape) | yes | bug-pool | Report is correct per SHARED-COMMENT-012; the bug is that the formatter does not break before `.D`. The issue's proposed fix contradicts the spec |
| 889 | Test mid-line statement context of chain fixes | analyzer | Maintenance | internal | RH5112/RH5201 fix (`FluentChainCodeFixHelper`) | — | — | — | no | Maintenance | Test only |
| 892 | Report line break before attached part of chain | enhancement, analyzer | 1.2.0 | behavior change | SHARED-CHAIN-*, RH5201 | pairs with #895 | P11 | #895 (formatter side) | yes | vNext | New-rule candidate (P11): an attached part is not a link |
| 893 | Report remaining chain layouts formatter changes | enhancement, analyzer | 1.2.0 | behavior change | RH5201, SHARED-CHAIN-* | — | P11 | case 2 overlaps #894 | yes | vNext | Widens RH5201; P11 own gap vs new rule to classify |
| 894 | Report trivia inside `?.`/`!.` operator | enhancement, analyzer | 1.2.0 | behavior change | new Layout rule, FMT-BREAK-001 | — | P11 | #893 case 2 | yes | vNext | New rule, no code fix |
| 895 | Join attached parts outside chains | enhancement, formatter | 1.3.0 | behavior change | FMT-BREAK-011/-012/-020, chain rewriter | — | P34-adjacent | #892, #956; #953 comment (`B⏎(a, 2)`) | partly | vNext | Round 9 LB-Q10 instances added |
| 896 | Anchor wrapped first call under last prefix dot | enhancement, formatter | 1.3.0 | behavior change | SHARED-CHAIN anchor, FMT-INDENT chain alignment, RH5201 | — | — | — | partly | vNext | Changes the anchor in both the formatter and RH5201 |
| 897 | RH5112/RH5048 wording "first member access" | enhancement, analyzer | 1.2.0 | behavior change | RH5112, RH5048 titles/messages, rule pages | — | — | — | yes | vNext | Message and title change only; cheap enough to keep in 1.2.0 |
| 951 | Raw strings: holes/nested not idempotent, tabs | bug, formatter | Triage | bug | FMT-PIPE-004, FMT-RAW-001/-002; non-goals point here | — | — | #1391 (raw lone CR) | yes | 1.2.0 | CRITICAL: cases 1 and 2 break FMT-PIPE-004. Case 3 (tabs) is a non-goal, so a behavior change |
| 953 | Indent unowned continuation lines one level | enhancement, formatter | vNext | behavior change | FMT-INDENT non-goal, FMT-BLANK-013, FMT-STRUCT-007/-023 | — | — | #1383 (`x +=`), #1397 (`public⏎int`), #895, #955 | yes | vNext | Umbrella for continuation lines; many comment instances |
| 955 | Wrapped ctor-initializer colon, wrapped constraint | bug, formatter | Triage | behavior change | FMT-BREAK-010, -015, -023 | partly #953 | — | #953 | yes | vNext | No requirement covers a colon-ended initializer or a wrap inside a constraint |
| 956 | One switch arm per line; argument lists like parameter lists | enhancement, formatter | vNext | behavior change | FMT-BREAK-008/-011/-012, non-goals | — | P34-adjacent | #895; RH5102 fix (comment); attribute args vs P11 new rule | yes | vNext | Can split into two PRs |
| 957 | `fixed(` spacing; double space before comment | bug, formatter | Triage | behavior change | FMT-SPACE-012, FMT-SPACE-014, RH6001-001 | — | — | #958 | yes | vNext | FMT-SPACE-012 omits `fixed`; part 2 contradicts FMT-SPACE-014 |
| 958 | Spacing rules: arrows, `?:`, `::`, named args, braces | enhancement, formatter | vNext | behavior change | new FMT-SPACE, FMT-SPACE-006/-009, FMT-BREAK-026, FMT-PIPE-014 | — | P34 | #1383; #957 | yes | vNext | Arrow-alike part is already a bug under FMT-PIPE-014 |
| 1131 | `Uses` SHARED-ATTR in line-breaks.md (+FMT-BREAK-012/-027 text) | — | SDD | clarification | FMT-BREAK-012, -027, SHARED-ATTR-001…-020 | — | — | — | no | Maintenance | Docs lane; still open (no SHARED-ATTR under `Uses`) |
| 1149 | FMT-BREAK-016 covers enum members (RH5111 fix converges) | — | SDD | behavior change | FMT-BREAK-016, RH5111, SHARED-FIX-001 | — | P7, P34? | #1383 (same requirement) | partly | vNext | Could be a bug if the enum `=` counts under FMT-PIPE-014 |
| 1243 | Lone-operator join inserts blank line | bug, formatter | Triage | bug | FMT-BREAK-017/-018; line-breaks.md scope ("inserts no blank line") | — | — | blocks #1273 (F3) | yes | bug-pool | Weak: no requirement ID; only the non-goal sentence and the examples |
| 1244 | Unreachable nested-element recursion (RH5303/RH5304) | enhancement | Triage | internal | SHARED-INIT-002 | #1271 (F3) | P11 | #1271 | no | Maintenance | Becomes reachable with #1271; close it with that issue |
| 1291 | State formatter extras beyond RH5304/RH5308/struct equality | — | SDD | clarification | FMT-BREAK-021, -018, SHARED-EQUAL-001 | — | P11 | struct part belongs to F7 | no | Maintenance | FMT-BREAK-018 already states it; FMT-BREAK-021 and SHARED-EQUAL still open |
| 1330 | Test interpolated-raw/UTF-8 lines in non-formattable helper | enhancement | — | internal | `GetNonFormattableLineIndices`, RH8301-RH8303 | — | — | #951 area | no | Maintenance | Test only; has no milestone |
| 1382 | Token behind multi-line comment tail starts no line (P33) | — | SDD | bug | SHARED-COMMENT-012, SHARED-INIT-001, RH5203-001, RH5204-001, FMT-BREAK-006/-007/-025/-027 | #1380 (landed) | P33, P12 | #888 | no | bug-pool | Formatter and RH5203 parts break SHARED-COMMENT-012; the RH5204 and SHARED-INIT-001 parts need a change doc |
| 1383 | One treatment per syntax family (P34) | — | SDD | bug | FMT-PIPE-014, FMT-BREAK-011/-012/-016, FMT-INDENT-006/-025, FMT-SPACE-005/-012 | #1380 (landed) | P34 | #953, #958, #1149 | no | bug-pool | FMT-PIPE-014 is on main, so the issue needs no change doc |
| 1391 | Indent/raw miss specified cases | — | SDD | bug | FMT-INDENT-012, -013, -015, -016, FMT-RAW-001, FMT-EOL-001 | — | P35 | #951, #953 | no (title partly) | bug-pool | All four instances break the requirements as written |
| 1397 | Line breaks: unjoinable lists, FMT-BREAK-005 moves, parenthesized conditionals/`case` | — | SDD | behavior change | FMT-BREAK-001, -005, -018, -022, -024, -025, FMT-SWITCH-004 | — | — | #953, #1291, #1243 | no | vNext | Item 16 is a bug (FMT-BREAK-005); split by lane |
| 1399 | Region in initializer/argument list; nested array rows | — | SDD | behavior change | FMT-INDENT-010, -023 | — | — | #1265 (F5), #953 | no | vNext | Change doc required |

## 2. Wrong bug labels

- #955: no requirement covers it. FMT-BREAK-010 covers only an initializer that shares the parameter-list line, and FMT-BREAK-015 covers only list elements, not constraint clauses. The column of the held wrap is the #953 non-goal. Extension → vNext.
- #957: part 1, FMT-SPACE-012 does not list `fixed`, so the code matches the spec. Part 2, FMT-SPACE-014 says the formatter "MUST NOT change the whitespace in front of that comment", so today's output is what the spec requires. Requirement change → vNext.
- (partial) #951 case 3: tab-indented raw strings are a non-goal of `raw-string-alignment.md`, so that case is an extension. Cases 1 and 2 are real bugs.
- (misframed, still a bug) #888: the label fits, but the bug is not the one the issue claims; see section 4.

## 3. Critical candidates

- #951 (cases 1 and 2): the formatter is not idempotent (FMT-PIPE-004). A multi-line interpolation hole flips between passes, and a raw string nested in a hole needs a second pass, so `--check` and delegating fixes never settle.
- No others. #888, #1382 (RH5203 comment) and #1149 describe fixes that make no progress, which breaks SHARED-FIX-001 convergence but is not on the critical list. #1397 item 16(c) moves a comment (`lbl: // l`, `while (a) // w`), which relocates it without losing it; watch it under P36.

## 4. Real bugs

- #888, against SHARED-COMMENT-012 (the formatter must break before the token behind the `*/`) and SHARED-FIX-001. Input `a.B()⏎ .C()⏎ /* x⏎ */ .D();` → today the formatter keeps `*/ .D()`, RH5201 reports it, and the fix only adds a blank line → required: `.D()` starts its own line, and the RH5201 report is correct. The issue's proposal (measure the comment's first line, or withhold) contradicts the spec.
- #951, against FMT-PIPE-004 and FMT-RAW-002. A `$"""` literal with the hole `{⏎ name⏎ }` → pass 1 puts `}` at column 21, pass 2 moves `name` and `}` to column 8 → required: one stable pass, with the hole lines shifted uniformly by FMT-RAW.
- #1243, against FMT-BREAK-017/-018 layout and line-breaks.md scope (weak, no ID). `var x = a⏎ &&⏎ b;` → `a⏎⏎ && b;` → `a⏎ && b;`.
- #1382 (formatter and RH5203 parts), against SHARED-COMMENT-012:
  - `int a; /* first⏎ second */ int b;` → kept → line break before `int b;`.
  - `{⏎/* p⏎q */ Foo();⏎}` → kept → `Foo();` on its own line.
  - RH5203 on `P(1,⏎ /* a⏎ b */ 2);` with `2` in the alignment column → not reported → reported. The RH5203 fix makes no progress (SHARED-FIX-001).
- #1383, against FMT-PIPE-014:
  - `int this[int a⏎ , int b]` → kept → laid out like `(…)` lists.
  - `x⏎ += 1;` / `x ??=⏎ 2;` → kept → `x += 1;`.
  - A comment before `]` → `]` column → elements' column.
  - `[return:Obsolete]` → kept → `[return: Obsolete]`.
- #958 (arrow part only), against FMT-PIPE-014 ("every arrow is spaced alike"). `get =>_p;` → kept → `get => _p;`. The rest of #958 is an extension.
- #1391:
  - FMT-INDENT-013/-016: in the base list `class C : System.IDisposable,⏎ System. // c⏎ ICloneable`, `ICloneable` goes to column 0 → it must align at the first token of its element.
  - FMT-INDENT-012: in `M( // c⏎ );`, `)` goes to column 8 → it must sit after `(`.
  - FMT-INDENT-015: `typeof(Dictionary< // omitted⏎,>)` → column 16 → block column 8.
  - FMT-RAW-001 with FMT-EOL-001: a raw string with lone `\r` breaks is not aligned → it must be aligned.
- #1397 item 16(a), against FMT-BREAK-005 ("nothing but whitespace behind it"). `if (a) /* c */ {` → the comment moves above `if` → the comment stays and the break goes before `{`. Items 16(b) and 16(c) are readings of the same text.

## 5. Groups

- **G1 chain, P33 (bug lane, formatter and analyzer):** #888 and #1382. One cause: no line break before a token behind a multi-line comment tail, plus line-start rules. #889 (test) fits into the same run.
- **G2 chain extensions (behavior-change lane, analyzer):** #892, #893, #894, #897. #893 case 2 overlaps #894. #892 pairs with #895.
- **G3 chain or attached-part extensions (behavior-change lane, formatter):** #895, #896. #895 also overlaps #956 (argument lists).
- **G4 raw strings (bug lane):** #951 and the raw part of #1391 (lone `\r`). #1330 (helper test, internal) can ride along.
- **G5 indentation bugs (bug lane):** the indentation parts of #1391.
- **G6 indentation extensions (behavior-change lane):** #953, #1399, #955 (its held-wrap column depends on #953).
- **G7 P34 family (bug lane):** #1383, the arrow part of #958, and possibly #1149 (FMT-BREAK-016 family).
- **G8 line-break extensions (behavior-change lane):** #955, #956, #1397 items 15/17/18, #1149.
- **G9 spacing extensions (behavior-change lane):** #957, #958 (rest).
- **G10 spec text (docs or clarification lane):** #1131, #1291.
- **Cross-family:** #1243 blocks #1273 (F3), so run them together or in sequence. #1244 closes with #1271 (F3).
- **Duplicates/overlaps:**
  - #953 comment instances: `x⏎+= 1` and `x ??=⏎2` = #1383; `B⏎(a, 2)` = #895 comment; `public⏎int B` = #1397 item 15.
  - #956 attribute-argument instance `[A(1⏎, 2)]`: this collides with the P11 decision that attribute argument lists get their own rule (#1114/#1115 follow-up).

## 6. Doubts

1. SHARED-COMMENT-012 is written for every rule that checks line starts and for the formatter, but only RH5203 lists it under `Uses`. Does it bind RH5201, RH5204 and the formatter line-breaks spec today? If yes, the RH5204 part of #1382 is a bug too; if no, it is a behavior change.
2. #888: rewrite the issue to the defect the spec supports (the formatter breaks before `.D`, and RH5201 stays) and merge it into #1382, or keep it separate?
3. #1383 asks for a change doc, but FMT-PIPE-014 is already on main, so it can run in the bug lane. Also check whether PR #1405 already fixed any instance (not run here).
4. #1149: does the `=` of an enum member fall under FMT-PIPE-014 "every assignment operator"? If yes, it is a bug, not a behavior change.
5. #1243: no requirement ID forbids the blank line; only the line-breaks.md non-goal sentence and the examples do. Treat it as a bug, or as a clarification (a new FMT-BREAK requirement) plus a fix?
6. #951: split case 3 (tabs, a non-goal, so an extension) from the critical cases 1 and 2, or keep them together in 1.2.0?
7. #957 part 1: RH6001-001 omits `fixed`, but `RH6001KeywordsMustBeSpacedCorrectlyAnalyzer` reports `FixedKeyword` and the rule page lists it. The RH6001 spec needs a clarification, and formatted `fixed(` code is reported by RH6001. Part 2 must amend FMT-SPACE-014, although the issue says that requirement stays unchanged.
8. #955: the maintainer called it a bug before SDD, but no requirement covers it. Make it vNext, or treat it as a clarification of FMT-BREAK-010/-015 plus a fix in the bug pool?
9. #897: keep it in 1.2.0 (a cheap message change, which also fixes a misleading message), or move it to vNext?
10. P11 classification: #892 and #894 are new-rule candidates. #893 case 2 (columns) is an own gap of RH5201. #893 case 1 (first call's line) is more a line-break shape, closer to RH5112 or a new rule.
11. #1397: split item 16 (bug lane) from items 15/17/18 (behavior-change lane)?
12. #1291's struct-equality instance belongs to F7 (RH1001/RH1002). SHARED-EQUAL-001 still carries the old limitation.
13. #1330 has no milestone. #1244 and #1330 are internal, but #1244 depends on #1271.
