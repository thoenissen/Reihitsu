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

- Round 6 (prompts 2026-10-19, `sdd/work/coordinator/prompts-round6.md`, overlap check `inventory/batches-round6.md`): #984, #991, #992, #994, #996, #997. Next: #985 after #984; #995 after #994; #993 after #994 and #996; then #939, #998.

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

## Waiting for maintainer

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
| 8 | #984 | D3 parameter/return/typeparam/inheritdoc docs | #982 | prompt given 2026-10-19 |
| 9 | #985 | D4 documentation layout + comments | #982 | open |
| 10 | #986 | S1 whitespace/spacing part 1 | — | merged (PR #1036); filed #1038–#1052 |
| 11 | #987 | S2 spacing part 2 | #986 | merged (PR #1103); filed #1106–#1108 |
| 12 | #988 | BL other blank lines | — | merged (PR #1057); filed #1059–#1066 |
| 13 | #989 | CP comment placement, creates spec | — | merged (PR #1056); filed #1058 |
| 14 | #990 | LB line breaks + alignment (fluent chain) | — | merged (PR #1104); filed #1111–#1125 |
| 15 | #991 | LA initializers/expressions/braces | — | prompt given 2026-10-19 |
| 16 | #992 | LC trailing commas, empty types | — | prompt given 2026-10-19 |
| 17 | #993 | O1 file layout, member/modifier ordering | — | open |
| 18 | #994 | O2 usings, regions | — | prompt given 2026-10-19 |
| 19 | #995 | O3 member grouping by regions, RH7501 | — | open |
| 20 | #996 | B1 RH0002–RH2109 | — | prompt given 2026-10-19 |
| 21 | #997 | B2 clarity | — | prompt given 2026-10-19 |
| 22 | #939 | tag formatter/shared/CLI/Playground tests | — | open |
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
- Bug issues to draft (phase 2): #919 Q1, #919 Q3, #930 Q1, #930 Q2 (maybe one issue for RH2005), #926 Q1, #926 Q2, #928 Q1, #929 Q1, #922 Q1(+#921 Q3), #922 Q2, #922 Q3, #921 Q2, #918 Q3, continuation rule (#921 Q1/#918 Q5/#913 Q2+Q4)
- Cleanup issues (phase 2): #919 Q2, #921 Q4
- #946 (RH5005/RH5006 `await using`/`await foreach` fix never converges) → phase 2; blocks RH5005/RH5006 specs.
- #939 split → done: #939 (tagging) + #998 (opt-out).
- #942 → phase 5.
- FINAL STEP before closing SDD (maintainer 2026-10-19): remove every retired requirement and renumber the remaining IDs so the specs are clean (gap-free). Needs: rulebook change in sdd/README.md (ID stability/retirement rules), requirement-ids.txt rebuilt, every test tag and every cross-reference (`Uses`, change docs, decisions, issues?) updated mechanically by a script; run as one tooling/docs PR after all batches (#939, #998) are merged and no squad is running.

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
