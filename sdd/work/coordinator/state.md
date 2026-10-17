# SDD coordinator state

Parent: #899. Branch: `claude/sdd-coordinator` (never merged; `sdd/work/` stays out of PRs to main).

## Working agreement

- Several questions per round are fine.
- A clear answer from the maintainer is approval to post the decision comment and tick the box; no separate ok needed.
- Creating new issues still needs an ok.
- "No change docs" applies only to writing the SDD documents. Every source change runs later through the squad with all rules (new behavior = behavior-change lane with change doc). Clarified on #912.
- Follow-up issues are created without showing them first (maintainer trusts the texts). Fixing starts only after SDD is established; no follow-up is urgent.

## Phase

3 — plan the analyzer backfill. Phase 1 and 2 complete (2026-10-17): all questions decided, 29 follow-up issues #948–#976, per-spec issues #913–#934 closed and ticked in #912. #912 stays open for questions the backfill files.

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

- nothing

## Waiting for maintainer

- maintainer: start phase 3 (backfill plan)?

## Collected for later phases

- Clarifications per spec batch: —
- Bug issues to draft (phase 2): #919 Q1, #919 Q3, #930 Q1, #930 Q2 (maybe one issue for RH2005), #926 Q1, #926 Q2, #928 Q1, #929 Q1, #922 Q1(+#921 Q3), #922 Q2, #922 Q3, #921 Q2, #918 Q3, continuation rule (#921 Q1/#918 Q5/#913 Q2+Q4)
- Cleanup issues (phase 2): #919 Q2, #921 Q4
- #946 (RH5005/RH5006 `await using`/`await foreach` fix never converges) → phase 2; blocks RH5005/RH5006 specs.
- #939 split → phase 3.
- #942 → phase 5.

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
