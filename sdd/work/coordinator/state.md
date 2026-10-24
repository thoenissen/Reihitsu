# SDD coordinator state

Parent: #899. Branch: `claude/sdd-coordinator` (never merged; `sdd/work/` stays out of PRs to main).

## Working agreement

- Several questions per round are fine.
- A clear answer from the maintainer is approval to post the decision comment and tick the box; no separate ok needed.
- Creating new issues still needs an ok.
- "No change docs" applies only to writing the SDD documents. Every source change runs later through the squad with all rules (new behavior = behavior-change lane with change doc). Clarified on #912.
- Follow-up issues are created without showing them first (maintainer trusts the texts). Fixing starts only after SDD is established; no follow-up is urgent.

## Phase

4 — run the batches (phase 3 done 2026-10-17: 21 batch issues #977–#997, #939 split into #939 + #998).

Earlier: 3 — plan the analyzer backfill. Phase 1 and 2 complete (2026-10-17): all questions decided, 29 follow-up issues #948–#976, per-spec issues #913–#934 closed and ticked in #912. #912 stays open for questions the backfill files.

## Order for phase 1

1. Program meaning: #919 Q1, #930 Q2
2. Shared code-fix bases (family questions): #926 Q2, #928 Q1, #929 Q1, #922 Q1(+#921 Q3), #922 Q2, #922 Q3, #921 Q2, #918 Q3, continuation rule (#921 Q1/#918 Q5/#913 Q2+Q4)
3. Idempotency: #919 Q3, #922 Q1+Q2 (with #921 Q3, same cause)
4. Phases undo each other: #918 Q3
5. Analyzer/formatter disagree: #920 Q1, #916 Q1
6. Groups: continuation lines (#921 Q1, #918 Q5, #913 Q2+Q4); trailing whitespace in comments (#923 Q1, #915 Q1)
7. Everything else, per issue

## Decisions

| Question | Decision | Comment | Follow-up |
|---|---|---|---|
| #919 Q1 local scope across braced sections | Bug: guard like FMT-SWITCH-011, leave switch unchanged | https://github.com/thoenissen/Reihitsu/issues/919#issuecomment-6232112753 | bug issue (phase 2) |
| #919 Q2 brace removal unreachable | Intended (FMT-SWITCH-008); removal path is dead code | https://github.com/thoenissen/Reihitsu/issues/919#issuecomment-6232230437 | cleanup issue (phase 2) |
| #919 Q3 comment before terminal break | Bug: FMT-BLANK-011 applies, blank line before the comment already in pass 1 (maintainer chose B; spec is right, code wrong) | https://github.com/thoenissen/Reihitsu/issues/919#issuecomment-6232230437 | bug issue (phase 2), check RH5020 agrees |
| #930 Q1 `static public` indentation | Bug: `private` first, takes first modifier's leading trivia | https://github.com/thoenissen/Reihitsu/issues/930#issuecomment-6232231113 | bug issue (phase 2) |
| #930 Q2 RH2005 fix breaks outside access | Bug: withhold fix when referenced outside the type in the solution; rule page sentence on public surface | https://github.com/thoenissen/Reihitsu/issues/930#issuecomment-6232231113 | bug issue (phase 2); drop open-question non-goals |
| #926 Q1 RH7103 written vs effective accessibility | Bug: effective accessibility; static ctor ordered against instance ctors regardless of accessibility | https://github.com/thoenissen/Reihitsu/issues/926#issuecomment-6232283207 | bug issue (phase 2); may raise new diagnostics |
| #926 Q2 ordering fixes format whole type | Bug: format only moved member + touched separators (all `TypeMemberOrderingCodeFixProviderBase` fixes: RH7102/7103/7109); correct test `CodeFixReordersStaticWithinRegionAndIgnoresInstanceMembersInOtherRegions` | https://github.com/thoenissen/Reihitsu/issues/926#issuecomment-6232283207 | bug issue (phase 2) |
| #928 Q1 `_hTTPClient` | Bug: leading uppercase run lowercased as one word (last letter starts next word before lowercase) for camel/_camel; PascalCase unchanged | https://github.com/thoenissen/Reihitsu/issues/928#issuecomment-6232283725 | bug issue (phase 2); requirement goes to shared casing spec |
| #929 Q1 RH5002 two blank lines | Bug in `StatementShouldBePrecededByABlankLineCodeFixProviderBase`: exactly one blank line | https://github.com/thoenissen/Reihitsu/issues/929#issuecomment-6232284098 | bug issue (phase 2), check RH5001–RH5019 |
| #922 Q1 + #921 Q3 raw-string hole lines | Bug: FMT-RAW alone owns line starts inside multi-line raw strings incl. holes; FMT-INDENT skips them | https://github.com/thoenissen/Reihitsu/issues/922#issuecomment-6232341520 | bug issue (phase 2) |
| #922 Q2 nested raw string | Bug: aligned in one pass | https://github.com/thoenissen/Reihitsu/issues/922#issuecomment-6232341520 | bug issue (phase 2) |
| #922 Q3 tab-indented raw string | Bug: replace closing-line whitespace prefix by spaces on every line | https://github.com/thoenissen/Reihitsu/issues/922#issuecomment-6232341520 | bug issue (phase 2) |
| #921 Q2 `#endif` after disabled text | Bug: FMT-INDENT-009 without exception | https://github.com/thoenissen/Reihitsu/issues/921#issuecomment-6232342201 | bug issue (phase 2) |
| #921 Q4 SwitchExpressionContributor comment | Code right (FMT-INDENT-027); fix summary + test name `SwitchExpressionArmsRemainAlignedWithSwitchKeyword` | https://github.com/thoenissen/Reihitsu/issues/921#issuecomment-6232342201 | small text/rename fix (phase 2) |
| #918 Q3 empty region after `{` | Bug: no blank line after `{`, blank between directives stays; confirm root-cause hypothesis first | https://github.com/thoenissen/Reihitsu/issues/918#issuecomment-6232386342 | bug issue (phase 2) |
| Continuation group #921 Q1, #918 Q5, #913 Q2 | New general rule: continuation line no contributor owns = one level deeper than the line its statement/member starts on; contributors keep precedence | https://github.com/thoenissen/Reihitsu/issues/921#issuecomment-6232385765 | one issue (phase 2), new FMT-INDENT requirement, no change doc |
| #913 Q4 region inside declaration | `=> 42;` follows continuation rule; blank lines around region intended (clarify FMT-BLANK-013) | https://github.com/thoenissen/Reihitsu/issues/913#issuecomment-6232386892 | part of continuation issue |
| #920 Q1 `fixed(` | Bug: `fixed` is a spaced keyword | https://github.com/thoenissen/Reihitsu/issues/920#issuecomment-6232450352 | bug issue (phase 2) |
| #920 Q2 unruled token pairs | New formatter rules: `x => x`, `b ? 1 : 2`, `arr[0]`, `typeof(C)`, `new List<int>()`, `global::`, `b: b`; no analyzer rule now | https://github.com/thoenissen/Reihitsu/issues/920#issuecomment-6232450352 | one issue with Q3 (phase 2) |
| #920 Q3 initializer braces | New: `{ 1, 2 }` both sides; RH6009/6010 unchanged | https://github.com/thoenissen/Reihitsu/issues/920#issuecomment-6232450352 | with Q2 |
| #920 Q4 double space before `/** */` | Bug: collapse to one space | https://github.com/thoenissen/Reihitsu/issues/920#issuecomment-6232450352 | bug issue (phase 2) |
| #916 Q1 (+#917 Q2) comment in using group | RH7207 + using phase accept blank line before comment-led directive; comment rule unchanged; SHARED-USING-008 changes | https://github.com/thoenissen/Reihitsu/issues/916#issuecomment-6232450977 | issue (phase 2) |
| #916 Q2 `#` in block comment blocks reorder | Bug: drop textual check | https://github.com/thoenissen/Reihitsu/issues/916#issuecomment-6232450977 | bug issue (phase 2) |
| #916 Q3 `global::` sort key | Bug (RH7203 + formatter): strip `global::`, unqualified first on tie | https://github.com/thoenissen/Reihitsu/issues/916#issuecomment-6232450977 | bug issue (phase 2) |
| #917 Q1/Q2 duplicates | Pointed to #918 Q3 and #916 Q1 | https://github.com/thoenissen/Reihitsu/issues/917 | none of its own |
| #923 Q1 + #915 Q1 trailing ws in comments | Remove: `///` lines by FMT-DOC, `//` and `/* */` by FMT-CLEAN; FMT-PIPE-002 allowance; empty summary -> bare `///` | https://github.com/thoenissen/Reihitsu/issues/923#issuecomment-6232535812 | one issue (phase 2) |
| #923 Q2 directive line trailing ws | Bug: strip for directives without message text | https://github.com/thoenissen/Reihitsu/issues/923#issuecomment-6232535812 | bug issue (phase 2) |
| #923 Q3 trailing ws at EOF w/o newline | Bug: strip | https://github.com/thoenissen/Reihitsu/issues/923#issuecomment-6232535812 | bug issue (phase 2) |
| #915 Q2 end tag on content line | Bug: symmetric, any length | https://github.com/thoenissen/Reihitsu/issues/915#issuecomment-6232536716 | bug issue (phase 2) |
| #915 Q3 inline elements expanded | Bug: rebuild only block-level elements | https://github.com/thoenissen/Reihitsu/issues/915#issuecomment-6232536716 | bug issue (phase 2) |
| #915 Q4 stray `///` relocated | Bug: relocate only before documentable node (shared predicate, FMT-BLANK follows) | https://github.com/thoenissen/Reihitsu/issues/915#issuecomment-6232536716 | bug issue (phase 2) |
| #918 Q1 ctor initializer colon | Bug: `: this(1)` on one line below parameter list | https://github.com/thoenissen/Reihitsu/issues/918#issuecomment-6232668152 | bug issue (phase 2) |
| #918 Q2 constraint clause wrap | Bug: join (`where T : class`) | https://github.com/thoenissen/Reihitsu/issues/918#issuecomment-6232668152 | bug issue (phase 2) |
| #918 Q4 switch-expression arms | New: one arm per line | https://github.com/thoenissen/Reihitsu/issues/918#issuecomment-6232668152 | issue (phase 2) |
| #918 Q6 argument lists | New: like parameter lists (trailing comma, `)` after last arg); formatter only | https://github.com/thoenissen/Reihitsu/issues/918#issuecomment-6232668152 | issue (phase 2) |
| #913 Q3 comment duplicated on split fields | Bug: comment between type and first declarator blocks the split; replace test `CommentOnTheTypeLineIsStillDuplicatedOntoEveryField` | https://github.com/thoenissen/Reihitsu/issues/913#issuecomment-6232668547 | bug issue (phase 2) |
| #913 Q1 comment after `=>` glued to `{` | Bug: converter puts comment on own line as first block line; FMT-BREAK-003/FMT-SPACE unchanged (general spacing rule withdrawn) | https://github.com/thoenissen/Reihitsu/issues/913#issuecomment-6233201145 | bug issue (phase 2) |
| #914 Q1 `#region   Fields` | New: always one space, no trailing ws; update test `TrailingOpeningWhitespaceWithCanonicalCloseIsStable` | https://github.com/thoenissen/Reihitsu/issues/914#issuecomment-6233201504 | issue (phase 2) |
| #924 Q1 EOL in block comments/disabled text | New: normalize; FMT-PIPE-002 allowance (with #923 Q1), FMT-PIPE-007 updated | https://github.com/thoenissen/Reihitsu/issues/924#issuecomment-6233201915 | issue (phase 2) |
| #927 Q1 balanced pragma in crossed body | Bug (guard scope): balanced pragma/nullable inside crossed body doesn't block; replace Core test | https://github.com/thoenissen/Reihitsu/issues/927#issuecomment-6233202421 | bug issue (phase 2) |
| #931 Q1 RH1003 numeric sub-addition | Bug: non-string sub-addition is one part | https://github.com/thoenissen/Reihitsu/issues/931#issuecomment-6233289154 | bug issue (phase 2) |
| #932 Q1 RH0001 trailing content | Bug: report anything but whitespace after root | https://github.com/thoenissen/Reihitsu/issues/932#issuecomment-6233289419 | bug issue (phase 2) |
| #933 Q1 dry-run diff header | Bug: path relative to cwd, forward slashes; outside cwd full path w/o prefixes | https://github.com/thoenissen/Reihitsu/issues/933#issuecomment-6233289816 | bug issue (phase 2) |
| #934 Q1 Playground generated input | New: show note like PLAY-006 | https://github.com/thoenissen/Reihitsu/issues/934#issuecomment-6233290045 | issue (phase 2) |

## Running

- 2026-10-22: #1331 and #1380 started by maintainer (`prompts-round9.md`). #1402 NOT started: to be bundled with #1401 + #1377 into one run once multi-issue squad runs exist (#1403). Maintainer filed request #1403 (squad: `/squad #a #b #c`, details to clarify later; coordinator's recommendations 1a-4a in the issue). Earlier (round 9): #939 (PR #1358), #1138 (#1357), #1260 (#1359), #1161 (#1360), #1139 (#1361) merged; maintainer reports further follow-ups. Inventory running -> `inventory/round9.md`. Next: #1331, then #998. Round-7 batches #985 (PR #1301), #993 (PR #1303), #995 (PR #1302) merged. Remaining batches: #939, then #998 last. Spec issues #1138, #1139, #1161, #1260 still open, no PR (not run yet). Status file: `inventory/round8-status.md`.
- Earlier, round 6 (prompts 2026-10-19, `sdd/work/coordinator/prompts-round6.md`, overlap check `inventory/batches-round6.md`): #984, #991, #992, #994, #996, #997. Next: #985 after #984; #995 after #994; #993 after #994 and #996; then #939, #998.

## Next: question round (new chat, planned 2026-10-18)

Trigger: when the currently running batches are merged. Maintainer may also adjust AGENTS.md.
Open-question issues so far (sub-issues of #912): #1000, #1001, #1009 (shared/casing Q1–Q4), #1010, #1011, #1012, #1013, #1014 — plus whatever the running batches file.
Patterns found by coordinator (2026-10-18), proposed as principle decisions first:
- A. Unsafe fix: fix yields non-compiling code, runtime breakage, or inconsistent inactive code (#1009 Q1, Q4; #1013 Q1, Q2; #1014 Q1; #1010 Q1). AGENTS.md "prefer no fix over an unsafe one" exists, but shared/code-fixes.md has only SHARED-FIX-001/002, so every instance becomes a question. Proposal: SHARED-FIX-003 (fix not offered when result does not compile, changes what runs, or leaves inactive code inconsistent); instances become bugs against the spec. #1010 `_`→`I_` already violates SHARED-FIX-001 (plain bug).
- B. Shared question filed per rule: #1013 Q1 (RH4103) = #1014 Q1 (RH4102): member name dictated by a base/interface in a referenced assembly (+ P/Invoke without EntryPoint). Belongs to #1009. Proposal: not reported, like explicit interface implementations. Will recur for RH4111–RH4114 (#979).
- C. `@` names compared/renamed with the `@`: #1009 Q2, #1011 Q1, #1012 Q2. Proposal: compare and rename by value without `@`; withhold a fix that would produce a keyword.
- Rest per issue: #1000 (documented limitation in RH5103.md → likely stated exception), #1001, #1009 Q3, #1011 Q2/Q3, #1012 Q1.
Possible AGENTS.md change: make "Code-fix coverage" point to SHARED-FIX-003 or state the unsafe-fix classes explicitly so squads file them as bugs, not questions.

## Question round (started 2026-10-18)

Inventory of all 78 questions (45 issues): `sdd/work/coordinator/inventory/{naming,spacing,blank,attr-doc}.md` (issue, Q, input/output, pattern, duplicates, suggested kind).
Round 1 decided (maintainer 2026-10-18): P1 a, P2 a, P3 a, P4 c (no principle, case by case), P5 ok, P6 ok, P7 ok. Posted: https://github.com/thoenissen/Reihitsu/issues/912#issuecomment-6253795346
- P1 unsafe fix → SHARED-FIX-003 + AGENTS.md sentence (#1068)
- P2 externally dictated names not reported → SHARED-CASING-011, behavior change (#1070)
- P3 compare by value → SHARED-DIAG-002 (#1069); bugs #1071 (RH4001/RH4009); reserved-keyword fix withheld = change #1072
- P4 no principle
- P5 literal/comment text only is content → SHARED-DIAG-003 (#1069); bugs #1074 (spacing), #1075 (blank lines, with P1)
- P6 `/** */` documenting nothing = block comment → #1077
- P7 SHARED-FIX-001 per rule, SHARED-FIX-004 no undo, SHARED-FIX-005 no no-op (#1068); bugs #1076
- Casing unsafe renames (#1009 Q1/Q4/Q5) → #1073
- Order: #1068, #1069 first (spec issues); bug issues are blocked by them. No bug urgent.
- Batch issues told: mark instances `Decided by principle Pn (#912)`.
Bookkeeping done (36 comments; #912 body lists all 45 issues, 15 ticked; follow-up list https://github.com/thoenissen/Reihitsu/issues/912#issuecomment-6253840713; note on 13 batch issues). Closed as fully decided (2026-10-18): #1013, #1014, #1023, #1025, #1026, #1027, #1039, #1047, #1048, #1051, #1052, #1060, #1061, #1063, #1066.
Pending maintainer confirmation: #1065 Q1 — coordinator recommends reading A (bug in RH5024; RH5032 + SHARED-BLANK-006 + formatter agree the blank stays); same for region part of #1059 Q2 (bug in RH5025). Then add to #1076.
Round 2 decided (maintainer 2026-10-18): #1065 Q1 A + #1059 Q2 region part → bugs added to #1076; R2.1 a → #1078 (change: spacing rules ignore comment-adjacent whitespace; #1042, #1045, #1046, #1049 Q1, #1050); R2.3 Q3, R2.4, R2.5 → bugs #1079 (#1038 Q3, #1043, #1044 Q1); R2.7 → bug #1080 (#1064); R2.6 + R2.8 a → clarifications #1081 (#1041, #1059 Q2 sibling braces). Closing (agent): #1041, #1042, #1043, #1044, #1045, #1046, #1049, #1050, #1059, #1064.
Round 2 rest decided: R2.2 b → #1082 (change: formatter drops final line break after trailing directive; RH5603 clarification), #1040 Q2 → added to #1079; R2.3 Q1 b → #1083 (change: tabs in region names replaced; #warning/#error text and directive comments untouched; SHARED-FIX-003 exception, noted on #1068). Closed #1038, #1040. 27 of 45 closed.
Round 3 decided (maintainer 2026-10-18): R3.1+R3.2 → bug #1084 (prefix doubling #1010, #1020; Unicode #1009 Q3); R3.3 Q2 + R3.4 → bugs #1085 (#1011 Q2, #1012 Q1); R3.3 Q3 a → change #1086; R3.5 Q1 exempt `_`, Q2 RH4115 stops checking local consts + NEW RULE for local consts (#1088), Q3/Q4 RH4117 owns all deconstruction vars → change #1087; R3.6 a + R3.7 → change #1089; R3.8 → cleanup #1090. Closing (agent): #1009, #1010, #1011, #1012, #1019, #1020, #1021, #1022, #1024 → 36 of 45 closed.
Round 3 bookkeeping done (comments, 9 closed, #912 ticked, follow-up list posted). 36 of 45 closed.
Round 4 put to maintainer (2026-10-18): P8 doc = what compiler produces (#1037 Q1/Q2/Q3a/Q5), R4.2 #1037 Q3b bug + Q4 static ctor private (change), R4.3 #1030 Q1=#1031 Q1 reading A, R4.4 #1029 align, R4.5 #1030 Q2–Q4 bugs, R4.6 #1058 Q1 a, Q2 b (+formatter), Q3 a clarification, R4.7 #1001 B (change), #1000 bug, #1062 bug, #1065 Q2 bug.
Round 4 decided (maintainer 2026-10-18): P8 ok → #1091; #1037 Q3b bug + Q4 change → #1092; R4.3+R4.4 → #1093; R4.5 → #1094; #1001 B → #1095; #1000 + #1065 Q2 → #1096; #1062 → added to #1075. Closing (agent): #1000, #1001, #1029, #1030, #1031, #1037, #1062, #1065 → 44 of 45. Batches #983–#985 told about P8.
R4.6 #1058 decided: Q1 anchor model option 1 (any link anchors; comment between receiver and first link reported; links . ?. !. !?. ->; ?[0] not a link; postfix ++ not a link, RH5038 covers it, no new rule); Q3 is/as never binary operators (before: region rules report, after: RH5049); + && exemption stays → #1097. Q2 (b) extended: construct owns only inside braces/arms, incl. before `{`/`switch` reported, formatter FMT-BREAK-015 too → #1098. #1058 closed.
SHARED-FIX-003 readings (answered in #1068 run, recorded in squad/decisions there): scoped to own report (a fix may edit comment/directive/literal text only where that text is what its rule reports; #1075 cases stay bugs); the name/accessibility a fix exists to produce is not a run-time change (nameof, reflection ok; record positional split, initializer order, P/Invoke still withheld). #1083 needs no SHARED-FIX-003 exception (comment posted).
#1069 run questions: Q1 comment/directive-subject rules exempt from SHARED-DIAG-003 (reading B, generalized; coordinator recommended); Q2 P3 instance in RH7201/RH7203/RH7207 (using names compared by spelling) → coordinator filed bug #1105 directly (no open-questions issue; formatter checked in same PR).
QUESTION ROUND COMPLETE 2026-10-18: all 45 closed; follow-ups #1068–#1098; summary https://github.com/thoenissen/Reihitsu/issues/912#issuecomment-6254444639. Principles P1–P3, P5–P8 told to batches.
Next: resume phase 4 batches (#981, #983, #984, #985, #987, #990–#997 unblocked; #939, #998 later) and run #1068, #1069 (spec issues the bug follow-ups depend on).
Round 4 issues: #1000 Q1, #1001 Q1, #1029 Q1, #1030 Q1–Q4, #1031 Q1, #1037 Q1–Q5, #1058 Q1–Q3, #1062 Q1, #1065 Q2.
Still open questions: #1000 Q1, #1001 Q1, #1029 Q1, #1030 Q1–Q4, #1031 Q1, #1037 Q1–Q5, #1058 Q1–Q3, #1062 Q1, #1065 Q2.

## Round 7 (started 2026-10-20)

Verified on GitHub (agent): round-6 batches #984 (PR #1162), #991 (#1163), #992 (#1164), #994 (#1165), #996 (#1166), #997 (#1167) all merged, issues closed, 0 open PRs. #1161, #1138, #1139 still open, no PR (not run yet). 62 open `SDD open questions` issues, none in #912 body: #1168-1177, #1183-1187, #1195-1201, #1209-1222, #1224-1238, #1245, #1247-1256. New non-question issues: #1239-#1244, #1257-#1259 (bugs/code-review reports, not triaged by coordinator). Inventory: `inventory/round7-*.md` (families: LA/LC, O2, D3, B2, B1).
Remaining batches: #985 (after #984, now unblocked), #995 (after #994, now unblocked), #993 (after #994+#996, now unblocked), then #939, #998.

Inventories DONE 2026-10-20 (5 files, 148 questions: LA/LC 46, O2 21+1, D3 18, B2 32, B1 29). Searches were rate-limited -> re-check duplicates before filing. Principle candidates (renumbered by coordinator, P14+): P14 fix layout scope (P10 = changed node + lines the fix edited, no blank line inserted before changed statement; P9 = neighbours untouched), P15 partial declaration = one declaration, P16 look-through (parentheses, !, throw), P17 regions belong to innermost type-like declaration (enums incl.), P18 ordering compares to previous same-group element, P19 end tag with other name = no end tag, P20 rule checks every element of its kind, P21 diagnostic always has a location, P22 conditional directive in reported construct = not reported, P23 report only what a correction can legally replace, P24 fix keeps document line ending; P3 ext to using aliases, P5 ext to Razor/HTML comments. Round 7a (principles) put to maintainer, plus #1160 reading.

## Round 7a decided (maintainer 2026-10-20; all recommended unless noted)

Numbering used with the maintainer (item numbers of the question message):
1 (P14a) fix inserts NO blank line before the changed statement: #1209 Q3, #1220 Q2, #1169 Q1 bullet 5 (tests pinning it change). A.
2 (P14b) #1170 Q1: member moves to own line, `int _x;` on next line (line the fix edits). A.
3 (P14c) blank line left by a removed region/directive is cleaned by the fix: #1175 Q4, #1186 Q1. A.
4 (P14d) fix formats only the replaced node, not the whole member: #1236/#1237 (RH3202/3203/3205-3208; two blank lines stay, RH3202-005 unchanged; SHARED-FIX-003/#1138 layout example adjusted). A.
5 #1226 Q1 RH3004 fix keeps `static` (`static (int x) => x`); check `async`.
6 (P15) partial declaration = one declaration; analyzer decides on the symbol, reports once (#1252 Q2, #1254 Q5, #1255 Q1); RH2004 fix on one part withheld (#1254 Q4).
7 (P16) 7a parentheses/`!` always transparent, 7b `throw` arm = no value (#1247 Q4/Q5, #1248 Q1). MAINTAINER NOTE: this is to be read as an EXTENSION (behavior change widening the rule), not as a bug fix. #1247 Q6 (type-parameter default) not covered, stays open.
8 (P17) regions belong to the innermost type-like declaration, enums included (#1185 Q2, #1187 Q2).
9 #1183 Q1 innermost region judges the member (RH7308).
10 (P18) ordering rules compare against previous same-group element whatever lies between (#1171-#1173 Q1).
11 (P20) rule checks every element/declarator of its kind (#1196 Q2, #1200 Q1, #1195 Q1).
12 (P19) end tag with a different name = no end tag, fix withheld (#1197 Q1, #1199 Q4, #1201 Q1/Q3; #1241 same bug).
13 #1195 Q3: RH8107-001 case-insensitive matching retired under P8 (retirement ok'd).
14 (P23) where a fix would break code: still report, offer no fix (#1229 Q3, #1231 Q1/Q2, #1233 Q2, #1234 Q1, #1245 Q2 constant patterns).
15 (P24) fix keeps the document's own line ending (#1228, #1142).
16 (P22) conditional directive/excluded code in the reported construct = not reported (SHARED-TRAIL-002); #region/#pragma stay reported without fix (#1169 Q3).
17 (P21) diagnostic always has a location; RH5404 extension block reported at `extension` keyword and block formatted (#1221 Q1).
18 #1184 Q1: alias to a non-name forms its own group at the end.
19 #1187 Q1: mutually exclusive #if/#else branches are not compared.
20 #1217 Q1: `?[ ]` not covered by RH5307.
21 P3 ext: aliases resolved semantically (#1253 Q1); P5 ext: Razor/HTML comments and `@@` are content (#1256 Q2/Q3).
22 #1160: B = fix result `M( /* c */ 1)` (the agent's reading is confirmed).
Not covered by 7a, still individual questions (round 7b): see round7 plan file.

Round 7a bookkeeping DONE 2026-10-20 (agent): 31 new issues #1260-#1290 (N01 = #1260 SDD P15-P22; mapping in `round7-created.md`), 17 existing extended (#1138 body P14a-d/P23/P24, #1241 retitled, others commented), 43 question issues closed, 19 commented and left open, #912 summary comment posted. 26 questions open -> `round7-open.md` (round 7b). Next: round 7b to maintainer, then batch prompts #985, #995, #993.

## Round 7b decided (maintainer 2026-10-20)

Items numbered as in the question message; file `round7-open.md` has the full text (its groups A-H). ALL answered A, i.e. the first option of each question there — in particular against the earlier recommendation: item 11 (#1247 Q2) A = look through Nullable<S> and tuples (behavior change, new diagnostics, a specialized extension), item 18 (#1251 Q4) A = report a property with `field` ONLY when it can be replaced by an auto-property without losing logic (maintainer: "nur melden, wenn man das property durch ein field ohne logik verlust ersetzen kann"), item 20 (#1252 Q1) A = report explicit interface implementations too (maintainer: the interface dictates `void`, not `async`; so the `async` is the implementation's choice; fix withheld when the interface dictates the void return type, P23). Item 5 (#1175 Q4 case 3) A = insert nothing. Item 4 (#1185 Q1) A = withhold the fix when a comment stands directly above a region block.
Mapping to round7-open.md: 1=#1214 Q1+#1218 Q4, 2=#1176 Q2, 3=#1177 Q1, 4=#1185 Q1, 5=#1175 Q4c3, 6=#1195 Q2+#1199 Q3, 7=#1196 Q1, 8=#1200 Q2, 9=#1245 Q3, 10=#1247 Q1, 11=#1247 Q2, 12=#1247 Q3, 13=#1247 Q6, 14=#1248 Q2+Q3, 15=#1249 Q1, 16=#1249 Q2, 17=#1250 Q1, 18=#1251 Q4, 19=#1251 Q5, 20=#1252 Q1, 21=#1254 Q1, 22=#1254 Q2, 23=#1256 Q4.

Round 7b bookkeeping DONE 2026-10-20 (agent): #1291-#1300 created (milestone SDD), #1262, #1263, #1285, #1288, #1289, #1290 extended, 19 question issues closed, #912 summary https://github.com/thoenissen/Reihitsu/issues/912#issuecomment-6298700600. ALL ROUND-7 QUESTION ISSUES CLOSED. Principles P14-P24 live in #1138 (P14a-d, P23, P24) and #1260 (P15-P22). Next: batch prompts #985, #995, #993 (overlap check -> inventory/batches-round7.md).

Running (2026-10-20): #985, #993, #995 — prompts in `prompts-round7.md` (overlap check `inventory/batches-round7.md`). Then #939, then #998 last.

## Round 8 (started 2026-10-21)

Inventories DONE (committed): `inventory/round8-o3.md` (#1304-#1305, 10 Q: 4 decided, 6 open), `round8-o1.md` (#1318-#1328, 23 Q: 16 decided, 6 decisions M1-M6), `round8-d4.md` (#1306-#1317, 48 Q: 30 decided, 2 partly, 12 decisions + 1 confirm). #1318-#1328 tracked on #912: https://github.com/thoenissen/Reihitsu/issues/912#issuecomment-6320177371
Principle-decided items: bookkeeping agent files them (new follow-ups grouped by cause; plan in the three inventory files, sections "Decided by principle"). Items whose follow-up depends on a round-8 answer are held (O1 A3, D4 #1316 Q2 deletion, D4 #1312 Q1 input 1, O3 Q10 confirm).
Round 8a (principles, numbering coordinator's, agents' P25-P27 renumbered): P25 verdict independent of incidental placement/order (O3 M1/M2/M3/M5); P26 moved code keeps its layout, only indentation follows (O1 M6 = #1318 A3); P27 member-ordering rules check each top-level region on its own (O1 M3 = #1322/#1323 N3); P28 per-part rules judge each part, exception to P15 (O1 M4 = #1324 D2); P29 doc comment behind code (D4 #1306 Q2/#1307 Q3/#1308 Q2); P30 doc-mode gate only for rules reading doc elements (#1315 Q2, #1316 Q4); P31 layout rule does not report a shape the formatter deliberately keeps (#1312 Q4/Q6/Q1 in1); clarifications P22 construct = judged span (#1321 N4), P18 partner = earlier same-group elements (#1328).
Round 8b (individual): O3 M4 (#1304 Q5), O3 M6 (#1305 Q9), O1 M2 (#1321 N1), D4 #1307 Q2, #1311 Q4, #1317 Q1, #1317 Q2, #1313 Q2, #1314 Q2, #1314 Q7, #1316 Q1, #1316 Q3; confirm #1305 Q10, #1310 Q1, #1311 Q3.

## Round 8 decided (maintainer 2026-10-21)

Item numbers as in the round-8 message (1-9 = 8a principles, 10-21 = 8b, 22 = confirmations).
1-9 recommended (a): P25 verdict independent of incidental placement/order (#1304 Q1/Q2/Q4/Q6); P26 moved code keeps its layout, only indentation follows (#1318 A3; narrows P10 for moved code); P27 member-ordering rules check each top-level region on its own (#1322/#1323 N3); P28 per-part rules judge each part, exception to P15 (#1324 D2, RH7104-RH7106); P29 doc comment behind code documents the next declaration, judged as if it started its line, only RH8303 fix moves it (#1306 Q2, #1307 Q3, #1308 Q2); P30 doc-mode gate only for rules reading doc elements (#1315 Q2, #1316 Q4); P31 layout rule with formatter-delegating fix does not report a shape the formatter deliberately keeps (#1312 Q4/Q6/Q1 input 1); P22 clarified: construct = span the rule judges (#1321 N4); P18 clarified: partner = every earlier element of the same group (#1328).
10 (#1304 Q5) NEITHER option. Maintainer: "names that are prescribed must be ignored by the rules; the formatter must no longer change the casing of regions." Reading: (i) RH7302 (and any region-naming rule) does not report a region description that is a name prescribed by another rule (type/interface name required by RH7401-RH7412); (ii) the formatter stops capitalizing region names (FMT-REGION-001 casing part retired; behavior change, change doc). Coordinator stated this reading back to maintainer 2026-10-21.
11 (#1305 Q9) a: keep the empty block (maintainer: otherwise the formatter could create braces again).
12-19 recommended: 12 #1321 N1 a (RH7003 does not report a namespace that contains a namespace); 13 #1307 Q2 a (RH8302 does not report a blank line followed by #region/#endregion); 14 #1311 Q4 a (indexers own gap of RH8306); 15 #1317 Q1 a (UnconditionalSuppressMessage own gap); 16 #1317 Q2 a (`<Pending>` reported); 17 #1313 Q2 a (text after XML comment behind closing tag reported); 18 #1314 Q2 b (RH8309 reports `/** */` order, no fix); 19 #1314 Q7 b (custom element moves with the element before it).
20 (#1316 Q1) NEITHER: maintainer: "not checked; 'copyright' is not a sufficient marker; keep it as is and replace completely." = today's behavior is intended: the RH8402 fix replaces the whole leading comment run (clarification). Decides #1316 Q2: the deletion is intended, not P1 -> clarification, no G4 entry.
21 (#1316 Q3) a: header must be followed by a line break or end of file.
22 confirmations (#1305 Q10, #1310 Q1, #1311 Q3): maintainer asked for more input -> re-asked with examples 2026-10-21.
10 reading CONFIRMED by maintainer: (i) RH7302 does not report a region name prescribed by another rule (RH7401-RH7412 type/interface names); (ii) the formatter no longer changes region-name casing (FMT-REGION-001 casing part retired, behavior change + change doc); RH7302 still reports and fixes other lower-case names.
22a (#1305 Q10) b: the RH7501 fix drops the blank line (`}⏎    break;`) — against recommendation; maintainer chose b, so P14a does not keep it here (record as stated exception for RH7501).
22b (#1310 Q1) b + NEW RULE: the formatter becomes case-sensitive for documentation tags (FMT-DOC-008, behavior change); new analyzer rule reports wrongly cased known documentation tags (e.g. `<Summary>`), fix corrects the case.
22c (#1311 Q3) a: P8c applied to RH8306's word check; `Gets<!-- c -->value` not reported.
Round 8 bookkeeping part 1 DONE (agent 2026-10-21): #1331-#1353 created (#1331 = SDD principles P25-P31 + P18/P22 clarifications; mapping `round8-created.md`), #1287/#1241/#1270 extended, 21 question issues closed. Open: #1304 (Q5 = item 10), #1305 (Q10 = 22a), #1310 (Q1 = 22b), #1311 (Q3 = 22c) -> part 2 files them. #912 summary https://github.com/thoenissen/Reihitsu/issues/912#issuecomment-6322240909
Round 8 bookkeeping part 2 DONE (agent 2026-10-21): #1354 (item 10: RH7302 ignores prescribed region names + formatter keeps region casing), #1355 (formatter doc tags case-sensitive), #1356 (new rule: wrongly cased doc tags), comments #1332 (22a), #1091, #1153 (22c); #1304, #1305, #1310, #1311 closed. ALL ROUND-8 QUESTION ISSUES CLOSED. #912 summary https://github.com/thoenissen/Reihitsu/issues/912#issuecomment-6322301064
Next runs (overlap check `inventory/batches-round8.md`, prompts `prompts-round8.md`): #939, #1138, #1260, #1139, #1161 in parallel (5 squads). Then #1331 (after #1260 + #1138, preferably #1161), then #998 last. ID plan: #1260 DIAG-004..008, DOC-017, P18 amends USING-007 or USING-011; #1138 FIX-006..008; #1139 COMMENT-011; #1331 DIAG-009/010.
Bookkeeping: first attempt to spawn the principle-decided bookkeeping agent was denied by the permission classifier (external writes); maintainer's clear answer now covers decided items.

## Round 9 (started 2026-10-22)

Inventory `inventory/round9.md`: 17 question issues #1362-#1376, #1378, #1379 (54 Q; 22 decided by principle/spec/earlier decision, 32 to maintainer) + #1377 docs follow-up (SHARED-FIX-008 citations). New issues have no milestone and are not tracked on #912 (bookkeeping to do). Unfiled PR-body follow-ups: sdd/README.md "Tests and requirements" stale after #939; formatter spec non-goals call decided issues open; SHARED-FIX-003 "for every input" vs DIAG-008/JOIN-001; AGENTS.md pointer to P11 (option); P11 closing sentence to narrow in #1331.
Round 9 message numbering (msg -> inventory section 5 item): 1 P32 (inv9); 2 P33 (inv3); 3 P34 (inv17, 20, 21, 25, 29 colon part); 4 P35 (inv8); 5 P36 (inv10); 6 P37 (inv13, 14); 7 P12 clarification (inv1); 8 inv2; 9 inv4; 10 inv5; 11 inv6; 12 inv7; 13 inv11; 14 inv12; 15 inv15; 16 inv16; 17 inv18; 18 inv19; 19 inv22; 20 inv23; 21 inv24; 22 inv26; 23 inv27; 24 inv28; 25 inv29; 26 inv30 (#1374); 27 inv31; 28 inv32; 29 PR-body docs follow-ups; 30 AGENTS.md P11 pointer.

## Round 9 decided (maintainer 2026-10-22: "looks good" = ALL recommended)

1 P32 (b) wrapping (braces/blocks/expression-body conversion) keeps author's comment layout (#1368 SW-Q2, #1371 Q-S2, supports #963). 2 P33 (b) multi-line comment belongs to the line it starts on; token behind its tail does not start a line; formatter breaks before it, rules report (#1362 Q4, #1373 LB-Q2; SHARED-INIT-001 + 2 formatter tests change). 3 P34 (a) one treatment per syntax family in the formatter (#1373 LB-Q5/Q8/Q9, #1370 Q-I5, attribute colon part of #1369 SP-Q2; spacing parts -> #958). 4 P35 (a) fix EOL = FMT-PIPE-007 detection, lone CR counts as LF (#1363 Q6, #1377 SHARED-ATTR-008/-017). 5 P36 (c) formatter does not apply a structural rewrite that would break the build, drop documentation or move a comment (#1371 Q-S5). 6 P37 (b) required blank line after directives, never directly before one (#1372 Q-B3/Q-B4; formatter + RH5002/RH5008 fixes). 7 P12 clarification (a) misplaced comment-led line is reported (#1362 Q2).
8 #1362 Q3 a; 9 #1363 Q1 a (bug, RH5104-005 changes); 10 #1363 Q2 a; 11 #1363 Q3 a (bug); 12 #1363 Q4 a; 13 #1371 Q-S4 b; 14 #1372 Q-B2 b (keeps #1081; RH5030 exempts following block); 15 #1373 LB-Q3 b; 16 #1373 LB-Q4 a+b+c; 17 LB-Q6 c; 18 LB-Q7 b; 19 #1368 SW-Q1 b; 20 #1370 Q-I1 b; 21 Q-I2 b; 22 Q-I6 a; 23 Q-I7 a; 24 #1369 SP-Q1 a; 25 SP-Q2 b.
26 #1374 Q1 close, no change. 27 extend #1276 (formatter extension-block braces). 28 #1370 Q-I3 code follows FMT-INDENT-013. 29 one docs-lane issue (README "Tests and requirements" stale + formatter non-goals naming decided issues open); SHARED-FIX-003 "for every input" wording -> #1331. 30 yes: AGENTS.md one-line pointer to P11 under "Issue intake" (tooling lane, file as issue).

Round 9 bookkeeping DONE (agent 2026-10-22): #1380 (SDD P32-P37 + P12 clar.), follow-ups #1381-#1400, docs #1401 (item 29), tooling #1402 (item 30); #1147, #1276, #958, #895, #1377, #963 extended; all 17 question issues closed. Mapping `round9-created.md`. #912 https://github.com/thoenissen/Reihitsu/issues/912#issuecomment-6350039592
Next runs (prompts `prompts-round9.md`): #1331, #1380, #1402 in parallel; then #1401 and #1377 (after #1331/#1380); then #998 last.

## Round 10 (started 2026-10-23)

Inventory `inventory/round10.md`: #1407 (3 Q), #1408 (3 Q incl. Q-1380-1/2); 3 decided by principle (#1407 Q1 P29 -> extend #1342; #1408 Q1 P26 -> new brace-fix issue RH5405-RH5407 + SHARED-BRACE-002; Q-1380-1 P35 -> extend #1388, RH5011-004/RH8304-004 join #1377). Other: #1411 (RH3103 where T? invalid, Triage), #1412 (RH7105/RH7106 partial ctor bug). #1407/#1408 not on #912 yet.
Round 10 message items: 1 P38 (#1411 b); 2 P39 (#1407 Q3 c); 3 P35 clarification "creates or keeps" (Q-1380-2 b); 4 P29 clarification doc mode None = ordinary trailing comment (#1407 Q2 b); 5 confirm #1405 readings FMT-STRUCT-024 / SHARED-COMMENT-012; 6 switch-arm `=>` candidate -> comment on #953.
Prompts `prompts-round10.md`: #1401+#1377 (docs, one run), #1402 (tooling). Then #998.
Round 10 DECIDED (maintainer 2026-10-23 "ok" = all recommended): 1 P38 (b) a rule reports only what the language lets the user write instead; P23 stays where the form is valid but the fix unsafe (#1411; RH3103 dotted nameof, member-access receiver, typeof unbound generic not reported; alias reported only from C# 12). 2 P39 (c) a fix that would separate a declaration from its `///` is withheld (#1407 Q3, RH7309; extend #1262). 3 P35 clarified (b): every break a fix creates or keeps in the changed code gets the detected ending (Q-1380-2; extend #1388). 4 P29 clarified (b): in doc mode None a behind-code `///` is an ordinary trailing comment, stays (#1407 Q2, clarification). 5 #1405 readings confirmed (FMT-STRUCT-024 moving a comment into the new block is not a move; SHARED-COMMENT-012 conditions as written). 6 switch-arm `=>` candidate -> comment on #953 after reproduction.
Round 10 bookkeeping DONE (agent): #1415 (SDD P38/P39 + P29/P35 clar.), #1416 (brace fixes P26), #1417 (RH3103 P38, blocked by #1415); #1342, #1388, #1262, #1281 extended; #1407, #1408, #1411 closed; #1412 milestone SDD; #953 switch-arm comment; #912 https://github.com/thoenissen/Reihitsu/issues/912#issuecomment-6367813739. Mapping `round10-created.md`. #1377 run already open as PR #1414. Prompt for #1415 appended to `prompts-round10.md`. Next: #1415 (parallel with #1414/#1402), then #998.

## Waiting for maintainer

- 2026-10-24: maintainer: spec format stays as it is for now (no per-requirement files, no index change). Final cleanup session running (prompt-cleanup.md). Restart prompt for Phase 5 written: `restart-prompt-phase5.md`.

- 2026-10-23: #998 merged (PR #1426, scope narrowed by maintainer: coverage opt-out per requirement, untagged tests allowed, no exemption attribute); #1423+#1424 merged (PR #1425). ALL SDD BATCHES DONE. Maintainer decisions for the final cleanup: delete ALL squad/decisions entries (new rules derived from them presented to maintainer first; README stays with rule 'entry deleted when it becomes a rule' as proposal); ONE change doc only (introduce-sdd.md describing SDD + squad as they are now; establish-squad.md and requirement-coverage-opt-out.md folded in). Cleanup = dedicated non-squad session, prompt `prompt-cleanup.md`; 14 retired requirements in 8 specs, 1445 ledger lines, 65 decisions. Afterwards: update open issues citing renumbered IDs (coordinator), ask about closing #899/#912, then Phase 5.

- 2026-10-23 (round 11): #1415 (PR #1418) and #1419 (PR #1421) merged; no open-question issues filed. New: #1422 (P38 follow-up RH3104/RH5417, behavior change, phase 5; no milestone), #1423 + #1424 (squad instructions, tooling). Prompts `prompts-round11.md`: #998 (with coordinator recommendations for its two open decisions: incidental tests exempted visibly; architecture/tooling tests exempted as a class) and #1423+#1424 in parallel. Then final cleanup PR.

- 2026-10-23: #1414 (#1401+#1377) and #1413 (#1402) merged; #1420 (maintainer: ask complex questions in chat) merged. #1415 running. #1419 (maintainer-filed: FC-013 + skeptic check 3 vs P11) — coordinator agrees, adds: route P11 cases (classify own gap / new-rule candidate) and cite SHARED-DIAG-010 (P31) for the other half; prompt given in chat, runs parallel to #1415 (squad/ only). Then #998, then final cleanup.

- 2026-10-23: #1331 (PR #1404), #1380 (PR #1405), #1403 (PR #1406) merged. #1406 rule: a multi-issue run needs ONE shared lane (else maintainer splits); one agent per role for the whole run; branch claude/issues-a-b-c-slug or claude/batch-a-slug. Consequence: #1401+#1377 (docs) one run, #1402 (tooling) separate. Round-10 inventory running -> `inventory/round10.md`.

- 2026-10-22: #1331, #1380 running. Open: design points 1-4 of #1403 (multi-issue squad runs) — clarify before its run. Next: #1403 run (tooling), then #1401+#1377+#1402 as one run, then #998 last. Phase 5: implement follow-ups in batches by cause/code area (proposal, not yet decided).

- 2026-10-21: round 8 started. 25 open question issues #1304-#1328 (81 items; #1304-#1317 tracked in #912 comments, #1318-#1328 not yet). Coordinator runs inventory (`inventory/round8-*.md`), then question round 8. New non-question issues: #1329 (formatter bug: declaration after unclosed doc element becomes a comment), #1330 (test non-formattable-line helper). Next batch: #939 (unblocked), then #998.

- 2026-10-19: round 6 squads running (#984, #991, #992, #994, #996, #997). Maintainer to confirm #1160 reading `M(/* c */ 1)` → `M( /* c */ 1)`. Earlier: verified on GitHub — #1068 (PR #1100), #1069 (PR #1099), #981, #983, #987, #990 merged; no open PRs. Coordinator runs inventory of the 24 new open-question issues, then question round 5, then next batch prompts.

## Question round 5 (started 2026-10-19)

New open-question issues (open sub-issues of #912, not yet in #912 body): #1106 operator-spacing (3), #1107 RH6016 (2), #1108 RH6021 (1), #1109 attribute-targets (1), #1110 code-fixes (3, Q2/Q3 in comments), #1111 formatter/line-breaks (2), #1112 formatter/pipeline (1), #1113 code-fixes (2), #1114 RH5101 (1), #1115 RH5102 (1), #1116 RH5103 (4), #1117 RH5104 (6), #1118 RH5108 (1), #1119 RH5110 (2), #1120 RH5111 (3), #1121 RH5113 (1), #1122 RH5114 (1), #1123 RH5203 (1), #1124 RH5204 (2), #1125 RH5206 (1), #1127 RH8032 (1), #1128 RH8031 (3), #1129 RH8030 (3), #1130 element-documentation (7 + addenda). #1126 closed as duplicate of #1110 Q2.
New follow-ups from the runs (open): #1105 using names by value, #1131 line-breaks `Uses`, #1132 param-doc `name` matching review, #1133 RH5601 raw-string tab, #1134 RH7109 layout fields, #1135 RH7309 static init order, #1136 SHARED-FIX-003 relocation/disabled text, #1137 RH2005/RH7103 non-goals.
Inventory: `sdd/work/coordinator/inventory/round5-*.md`.
Inventory done (4 files, ~55 questions). Bookkeeping by principle (agent, 2026-10-19): P8 instances #1128 Q1–Q3, #1129 Q2, #1130 Q1/Q4/Q6 → comment on #1091; P6 #1117 Q2, #1122 Q1 → #1077; close #1122, #1128; add all 24 issues to #912 body.
Bookkeeping DONE: #1091 https://github.com/thoenissen/Reihitsu/issues/1091#issuecomment-6267961978, #1077 https://github.com/thoenissen/Reihitsu/issues/1077#issuecomment-6267962543; #1122, #1128 closed; #912 body lists the 24 issues (lines without spec-path suffix), #1122/#1128 ticked.
Held for after the round (cause may change with a principle): P7 no-op/convergence group (#1106 Q2, #1119 Q2, #1120 Q1, #1123 Q1 s2, #1125 Q1, #956 RH5102 comment) — new issue; Fix All second pass group (#1106 Q3, #1107 Q2, #1110 Q1, #1113 Q2) — new issue; RH5104 bug issue (#1117 Q1/Q4/Q5/Q6); #1120 Q3 = #1121 Q1 → part of #1113 Q1 (not P1: line breaks next to XML tags are layout).
Round 5a put to maintainer (principles): P9 fix scope (#1113 Q1, #1106 Q1, #1109 Q1, #1120 Q3, #1121 Q1), P10 fix layout = formatter layout (#1112 Q1, #1116 Q4, #1123 Q1 s1), P11 rule covers every syntax form the formatter lays out under the requirement it uses (#1114/#1115, #1116 Q1/Q3, #1118, #1119 Q1, #1120 Q2, #1124 Q1), P12 line-leading comment is the line's first element (#1119 Q2, #1123 s2, #1125), P13 comment kinds alike (#1117 Q3, Q6), P8 extended to parameters (#1127 Q1, #1130 Q3/Q5/Q5c), summary text = renders text (#1130 Q2/Q7, #1129 Q1/Q3).
Round 5b put to maintainer (individual): #1107 Q1, #1108 Q1, #1110 Q2, #1110 Q3, #1113 Q2 B, #1111 Q1, #1111 Q2, #1116 Q2, #1117 Q4, FMT-BLANK-011 inside constructs (#1119 Q2, #1123), FMT-BREAK-016 enum members (#1120 Q1), #1124 Q2.
Round 5 decided (maintainer 2026-10-19): P9 B (fix changes only the code its diagnostic is about) — P10 ok (fixed code laid out as the formatter lays it out, one application) — P11 NOT as proposed: maintainer: "the formatter may do more than the sum of the analyzers; there is not yet an analyzer for everything. A behavior must never be forced on an analyzer because it roughly fits; make a specialized rule instead." → P11 instances held, classification put back to maintainer — P12 ok (comment at line start = line's first element; other rules forbid the comment itself) — P13 ok — P8b ok — P8c ok. 5b 1–14 all as recommended; 5b-2 plus: maintainer wants a NEW RULE "comments must not touch code" → scope asked.
Round 5 bookkeeping DONE (agent 2026-10-19): new follow-ups #1138 SDD P9/P10 (+SHARED-FIX-002 clarification), #1139 SDD P12/P13, #1140 P9 line-break/alignment fixes, #1141 P9 RH6023/6024, #1142 P9 attribute split fixes (+LF drift), #1143 Fix All one application (after #1140–#1142; audit 26 test classes), #1144 P10 fix layout, #1145 RH5103, #1146 RH5104, #1147 P12 RH5110/5203/5206, #1148 FMT-BLANK-011 scope (change), #1149 FMT-BREAK-016 enums (change), #1150 P7 formatter-unchanged fixes withheld, #1151 RH7302 endregion comment, #1152 RH5204 location, #1153 docs P8b/P8c/#1129. Extended #958, #1078, #1131. Closed 14: #1106, #1107, #1109–#1113, #1117, #1121, #1123, #1125, #1127, #1129, #1130 (ticked in #912). Partly decided, open: #1116, #1119, #1120, #1124. #912 summary https://github.com/thoenissen/Reihitsu/issues/912#issuecomment-6271774844
Round 5c put to maintainer: C1 P11 wording (formatter may do more; no widening because it roughly fits; gap inside a rule's own construct = rule's gap) + classification: own = #1119 Q1, #1120 Q2, #1116 Q1/Q3, #1124 Q1 (after #951), #1118 Q1; new rule = #1114/#1115 attribute argument lists. C2 new rule "comments must not touch code" scope (recommend one space between comment and adjacent token on same line, except after ( [ < and before ) ] , ; ; // and /* */; code fix + formatter; ///,/** */ ?).
Round 5c decided (maintainer 2026-10-19): C1 ok → P11 adopted ("formatter may do more than the sum of the analyzers; no widening because it roughly fits → specialized rule; gap inside the rule's own construct = rule's gap"); own-gap behavior changes RH5110 record struct, RH5111 nested assignment, RH5103 (#1116 Q1/Q3), RH5204 (#1124 Q1, after #951), RH5108 (#1118); new rule for attribute argument lists (#1114/#1115); P11 docs issue. C2: all four cases reported (`:/* c */`, `;// c`, `(/* c */`, `a/* c */+`) → new rule "comments must not touch code" (missing whitespace only; // and /* */, plus ///,/** */ that document nothing; fix inserts one space; formatter too). Agent filing + closing #1108, #1114–#1116, #1118–#1120, #1124.
Round 5c bookkeeping DONE: #1154 RH5110 record struct, #1155 RH5111 nested assignment, #1156 RH5103 (pred. last line + top level), #1157 RH5204 non-keyword lines (blocked by #951), #1158 RH5108 more parameter lists, #1159 new rule attribute argument lists, #1160 new rule comment does not touch code (agent's readings: `M(/* c */ 1)` → `M( /* c */ 1)`; lands with/after #1078; doc comment documenting a declaration out of scope), #1161 SDD record P11. Closed #1108, #1114–#1116, #1118–#1120, #1124. ALL ROUND-5 QUESTION ISSUES CLOSED. #912 comment https://github.com/thoenissen/Reihitsu/issues/912#issuecomment-6271914785
(P11 instances and #1108 resolved in 5c.)

## Phase 5 plan: triage of follow-up issues (maintainer 2026-10-20; do not start before SDD is established)

Goal: no follow-up issue stays in milestone `SDD` or `Triage`. Every one of the ~250 follow-ups (#948-#976, #1068-#1105, #1131-#1161, #1239-#1300, plus #946 and the rest) is decided as follows:
1. Critical? -> next version, milestone 1.2.
2. Can it be rated an extension (new capability, behavior change, e.g. the formatter also correcting `#region   Fields` spaces)? -> milestone vNext.
3. Only an internal improvement (cleanup, refactoring, test/doc)? -> milestone Maintenance.
4. Bugs that remain after that: prioritize them and, depending on their number, decide how to proceed (batching, order, squads per batch).
Also: check whether every issue labeled "bug" really is a bug. Example: "formatter cannot remove spaces in `#region   Fields`" is labeled bug but is desirable and counts as an EXTENSION (-> relabel, vNext). Behavior-change issues from principle decisions (new diagnostics, widened rules, P16 etc.) are extensions by default unless the maintainer says otherwise.
Before fixing: bring all follow-ups into the format of the issue templates (.github/ISSUE_TEMPLATE) and decide whether more issue templates are needed (e.g. SDD follow-up / clarification / behavior change / new rule). Triage is a coordinated step: coordinator proposes classification per issue in rounds with code examples, maintainer decides; agents do the bulk relabel/milestone/reformat bookkeeping.

## Phase 4 run order (sequential, one batch at a time; maintainer 2026-10-17)

Status: open / running / PR #… / merged.

| Order | Issue | Batch | Blocked by | Status |
|---|---|---|---|---|
| 1 | #977 | ST statement blank lines (incl. RH5005/RH5006) | — | merged (PR #999); filed #1000, #1001 |
| 2 | #978 | N1 naming part 1, creates casing spec (shared/casing.md) | — | merged (PR #1007); filed #1009–#1014 |
| 2a | #1008 | rulebook: code ownership follows behavior, not callers (docs lane) | run after #978 | merged (PR #1017) |
| 3 | #979 | N2 naming part 2 | #978 | merged (PR #1018); filed #1019–#1027 |
| 4 | #980 | AP attribute placement, creates attribute-target spec | — | merged (PR #1028); filed #1029–#1031 |
| 5 | #981 | AS attribute list shape | #980 | merged (PR #1102); filed #1109, #1110; follow-up #1131 |
| 6 | #982 | D1 element documentation part 1, creates doc spec | — | merged (PR #1035); filed #1037 |
| 7 | #983 | D2 element documentation part 2 | #982 | merged (PR #1101); filed #1127–#1130; follow-up #1132 |
| 8 | #984 | D3 parameter/return/typeparam/inheritdoc docs | #982 | merged (PR #1162); filed #1195-#1201 |
| 9 | #985 | D4 documentation layout + comments | #982 | merged (PR #1301); filed #1306-#1317 |
| 10 | #986 | S1 whitespace/spacing part 1 | — | merged (PR #1036); filed #1038–#1052 |
| 11 | #987 | S2 spacing part 2 | #986 | merged (PR #1103); filed #1106–#1108 |
| 12 | #988 | BL other blank lines | — | merged (PR #1057); filed #1059–#1066 |
| 13 | #989 | CP comment placement, creates spec | — | merged (PR #1056); filed #1058 |
| 14 | #990 | LB line breaks + alignment (fluent chain) | — | merged (PR #1104); filed #1111–#1125 |
| 15 | #991 | LA initializers/expressions/braces | — | merged (PR #1163); filed #1209-#1222 |
| 16 | #992 | LC trailing commas, empty types | — | merged (PR #1164); filed #1168-#1170 |
| 17 | #993 | O1 file layout, member/modifier ordering | — | merged (PR #1303); filed #1318-#1328 |
| 18 | #994 | O2 usings, regions | — | merged (PR #1165); filed #1171-#1187 |
| 19 | #995 | O3 member grouping by regions, RH7501 | — | merged (PR #1302); filed #1304, #1305 |
| 20 | #996 | B1 RH0002–RH2109 | — | merged (PR #1166); filed #1247-#1256 |
| 21 | #997 | B2 clarity | — | merged (PR #1167); filed #1224-#1245 |
| 22 | #939 | tag formatter/shared/CLI/Playground tests | — | merged (PR #1358) |
| 22a | #1138, #1260, #1139, #1161 | principle spec issues (specs-and-tests lane; #1161 docs) | — | merged (PRs #1357, #1359, #1361, #1360) |
| 22b | #1331 | principles P25-P31 | #1260, #1138 (+#1161) | open |
| 23 | #998 | opt-out + every test tagged | all above | open |

Phase-3 decisions (maintainer 2026-10-17): #946 does not block — RH5005/RH5006 are in #977, the `await` form is covered by SHARED-STMT-004/SHARED-FIX-001 and fixed later by #946. No parallel runs. Order chosen by coordinator. 2026-10-18: maintainer has budget for 2 parallel squads — run independent batches at once (6 running: #979, #980, #982, #986, #988, #989) (never a batch with its blocker).
#978 question (2026-10-17): `CasingUtilities` used also by RH2001 fix → still code owner of shared casing spec (it implements the naming styles; a consumer outside the family uses the behavior, it is not a general utility). RH2001 spec (#996) references casing IDs under `Uses`; note posted on #996.
Retiring an existing spec requirement while moving it to a shared spec (RH4106, RH3202, RH8107) = escalate to maintainer (written into #978, #997, #982).

## Phase 3 plan (approved 2026-10-17; SA merged into ST → 21 batches)

328 rules without spec, 22 batches, every one covered exactly once (checked by script).
Shared specs: new ones are created by the first batch of a family; "if shared" = the batch creates it only when >1 spec relies on the behavior.

| # | Title range | Rules | Shared spec | Depends / notes |
|---|---|---|---|---|
| B1 | RH0002–RH2109 | RH0002, RH1001, RH1002, RH2001–RH2004, RH2006, RH2007, RH2101–RH2109 (18) | struct-equality (RH1001/1002) if shared | — |
| B2 | RH3001–RH3208 | RH3001–RH3007, RH3101–RH3107, RH3201, RH3203–RH3208 (21) | expression-body → block (may take `ExpressionBodyRewriteUtilities` out of RH3202); empty parentheses (RH3106/3107) if shared | — |
| N1 | RH4001–RH4110 | RH4001–RH4011, RH4101–RH4105, RH4107–RH4110 (20) | creates shared casing spec; moves `CasingAnalyzerBase`, `CasingCodeFixProviderBase<T>`, `NamingAccessibilityClassifier`, `CasingUtilities` out of RH4106 | note #971 |
| N2 | RH4111–RH4127 | RH4111–RH4118, RH4120–RH4127 (16) | reuses casing | blocked by N1; note #971; RH4116/RH4118 share a class name |
| ST | RH5004–RH5029 (statement blank lines) | RH5004, RH5007–RH5009, RH5012–RH5019, RH5021, RH5029 (14) | reuses SHARED-STMT | note #972 |
| SA | RH5005–RH5006 | RH5005, RH5006 (2) | reuses SHARED-STMT | blocked by #946 (+ ST for no conflicts); note #972 |
| BL | RH5011–RH5032 (other blank lines) | RH5011, RH5020, RH5022–RH5028, RH5030–RH5032 (12) | reuses SHARED-BLANK; blank-line span removal, region-directive blank line if shared | — |
| CP | RH5033–RH5049 | RH5033–RH5047, RH5049 (16) | creates comment placement (`CommentPositionAnalyzerBase`, `CommentRegionAnalyzerBase`) | — |
| LB | RH5048–RH5206 | RH5048, RH5101–RH5114, RH5201–RH5206 (21) | reuses SHARED-CHAIN (+`FluentChainAnalyzerBase`); collapse token gap if shared | — |
| LA | RH5301–RH5408 | RH5301–RH5308, RH5401–RH5408 (16) | statement braces (RH5405–5407) if shared | — |
| LC | RH5409–RH5417 | RH5409–RH5417 (9) | trailing comma, empty type declaration | — |
| AP | RH5501–RH5530 (placement) | odd RH5501–RH5527, RH5530 (15) | creates attribute-target spec (`AttributeTargetRuleAnalyzerBase`, placement bases) | — |
| AS | RH5502–RH5531 (list shape) | even RH5502–RH5528, RH5529, RH5531 (16) | extends attribute-target spec | blocked by AP |
| S1 | RH5601–RH6012 | RH5601–RH5604, RH6001, RH6003–RH6012 (15) | creates whitespace-run removal spec if shared | — |
| S2 | RH6013–RH6024 | RH6013–RH6024 (12) | reuses S1's | blocked by S1 |
| O1 | RH7001–RH7110 | RH7001–RH7004, RH7101, RH7102, RH7104–RH7110 (13) | reuses SHARED-ORDER; modifier ordering | note #968, #969, #970 |
| O2 | RH7201–RH7311 | RH7201–RH7207, RH7301–RH7311, RH7305A (19) | reuses SHARED-USING; region description if shared | note #965, #966 |
| O3 | RH7401–RH7501 | RH7401–RH7412, RH7501 (13) | creates member-region grouping spec | — |
| D1 | RH8001–RH8019 | RH8001–RH8019 (19) | creates element-documentation spec; moves shared doc utilities out of RH8107 | — |
| D2 | RH8020–RH8034 | RH8020–RH8034 (15) | reuses D1's | blocked by D1 |
| D3 | RH8101–RH8204 | RH8101–RH8106, RH8108–RH8111, RH8201–RH8204 (14) | reuses D1's documentation spec | blocked by D1 |
| D4 | RH8301–RH8501 | RH8301–RH8309, RH8401, RH8402, RH8501 (12) | reuses D1's | blocked by D1 |

#939 split: #939 keeps tagging formatter/shared/CLI/Playground tests; new issue T = opt-out switch + every-test-tagged rule, blocked by all 22 batches; reconcile with "incidental tests do not tag"; decide architecture/tooling tests.

## Collected for later phases

- Clarifications per spec batch: —
- Phase 5 triage of all follow-ups: see section 'Phase 5 plan'.
- Bug issues to draft (phase 2): #919 Q1, #919 Q3, #930 Q1, #930 Q2 (maybe one issue for RH2005), #926 Q1, #926 Q2, #928 Q1, #929 Q1, #922 Q1(+#921 Q3), #922 Q2, #922 Q3, #921 Q2, #918 Q3, continuation rule (#921 Q1/#918 Q5/#913 Q2+Q4)
- Cleanup issues (phase 2): #919 Q2, #921 Q4
- #946 (RH5005/RH5006 `await using`/`await foreach` fix never converges) → phase 2; blocks RH5005/RH5006 specs.
- #939 split → done: #939 (tagging) + #998 (opt-out).
- #942 → phase 5.
- FINAL STEP before closing SDD (maintainer 2026-10-19): one-off cleanup PR removes every retired requirement and renumbers the remaining IDs gap-free (requirement-ids.txt rebuilt; test tags, `Uses`, change docs, decisions updated by script). The rulebook (sdd/README.md: IDs immutable, never reused, retired stays) is NOT changed — it keeps holding for every later change; this PR is the single exception, merged by the maintainer himself by force without the checks. Run after all batches (#939, #998) are merged and no squad is running.

## Issues fully decided (close when follow-ups exist)

- #919, #930, #926, #928, #929, #922, #921, #920, #916, #917, #923, #915, #918, #913, #914, #924, #927, #931, #932, #933, #934 (all 21; close each once its follow-up issues exist, then tick it in #912)

## Still open in phase 1

- none

## Phase 2 plan (approved 2026-10-17; create issues, then close per-spec issues and tick #912)

Kind: B = bug fix lane, N = behavior-change lane (change doc), C = cleanup.
Created: all 29 (#948–#976), 2026-10-17. U1 runs as behavior change.

Formatter
- F1 B → #948 #919 Q1 switch bracing must not break cross-section locals
- F2 B → #949 #919 Q3 blank line before comment leading terminal `break`
- F3 C → #950 #919 Q2 delete dead `RemoveBraces` + #921 Q4 `SwitchExpressionContributor` summary/test name
- F4 B → #951 #922 Q1+#921 Q3, #922 Q2, #922 Q3 raw-string alignment (holes owned by FMT-RAW, nested literal, tabs)
- F5 B → #952 #921 Q2 `#endif` after disabled text
- F6 N → #953 continuation-line rule (#921 Q1, #918 Q5, #913 Q2, #913 Q4)
- F7 B → #954 #918 Q3 empty region after `{` (phases undo each other)
- F8 B → #955 #918 Q1 + Q2 ctor-initializer colon, constraint-clause wrap
- F9 N → #956 #918 Q4 + Q6 switch arms one per line, argument lists like parameter lists
- F10 B → #957 #920 Q1 + Q4 `fixed (`, double space before comment
- F11 N → #958 #920 Q2 + Q3 new spacing rules
- F12 N → #959 #923 Q1 + #915 Q1 + #924 Q1 comment line ends: trailing ws + line endings (FMT-PIPE-002 allowance)
- F13 B → #960 #923 Q2 + Q3 directive-line and EOF trailing ws
- F14 B → #961 #915 Q2 + Q3 doc element normalization (end tag symmetric, block-level only)
- F15 B → #962 #915 Q4 stray `///` relocation
- F16 B → #963 #913 Q1 + Q3 structural transforms (comment after `=>`, field split guard)
- F17 N → #964 #914 Q1 `#region` line normalization
Usings (analyzer + formatter)
- U1 N/B → #965 #916 Q1 comment-led directive in using group (RH7207, SHARED-USING-008)
- U2 B → #966 #916 Q2 + Q3 textual `#` check, `global::` sort key
Analyzer / code fixes
- A1 B → #967 #930 Q1 + Q2 RH2005 fix
- A2 B → #968 #926 Q1 RH7103 effective accessibility
- A3 B → #969 #926 Q2 ordering fixes format only the moved member
- A4 B → #970 #927 Q1 balanced pragma guard
- A5 B → #971 #928 Q1 casing acronym run
- A6 B → #972 #929 Q1 statement-blank-line fix base
- A7 B → #973 #931 Q1 RH1003
- A8 B → #974 #932 Q1 RH0001
- #946 existing (RH5005/RH5006 await using/foreach)
CLI / Playground
- C1 B → #975 #933 Q1 diff header
- C2 N → #976 #934 Q1 generated-input note
