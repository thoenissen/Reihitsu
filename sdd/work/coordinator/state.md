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

- nothing

## Waiting for maintainer

- maintainer: run #979 (prompt given); decision round pending for open-question issues #1000, #1001, #1009 (shared/casing), #1010–#1014 — offered

## Phase 4 run order (sequential, one batch at a time; maintainer 2026-10-17)

Status: open / running / PR #… / merged.

| Order | Issue | Batch | Blocked by | Status |
|---|---|---|---|---|
| 1 | #977 | ST statement blank lines (incl. RH5005/RH5006) | — | merged (PR #999); filed #1000, #1001 |
| 2 | #978 | N1 naming part 1, creates casing spec (shared/casing.md) | — | merged (PR #1007); filed #1009–#1014 |
| 2a | #1008 | rulebook: code ownership follows behavior, not callers (docs lane) | run after #978 | running (2026-10-18) |
| 3 | #979 | N2 naming part 2 | #978 | prompt given 2026-10-18 |
| 4 | #980 | AP attribute placement, creates attribute-target spec | — | prompt given 2026-10-18 (parallel to #979) |
| 5 | #981 | AS attribute list shape | #980 | open |
| 6 | #982 | D1 element documentation part 1, creates doc spec | — | open |
| 7 | #983 | D2 element documentation part 2 | #982 | open |
| 8 | #984 | D3 parameter/return/typeparam/inheritdoc docs | #982 | open |
| 9 | #985 | D4 documentation layout + comments | #982 | open |
| 10 | #986 | S1 whitespace/spacing part 1 | — | open |
| 11 | #987 | S2 spacing part 2 | #986 | open |
| 12 | #988 | BL other blank lines | — | open |
| 13 | #989 | CP comment placement, creates spec | — | open |
| 14 | #990 | LB line breaks + alignment (fluent chain) | — | open |
| 15 | #991 | LA initializers/expressions/braces | — | open |
| 16 | #992 | LC trailing commas, empty types | — | open |
| 17 | #993 | O1 file layout, member/modifier ordering | — | open |
| 18 | #994 | O2 usings, regions | — | open |
| 19 | #995 | O3 member grouping by regions, RH7501 | — | open |
| 20 | #996 | B1 RH0002–RH2109 | — | open |
| 21 | #997 | B2 clarity | — | open |
| 22 | #939 | tag formatter/shared/CLI/Playground tests | — | open |
| 23 | #998 | opt-out + every test tagged | all above | open |

Phase-3 decisions (maintainer 2026-10-17): #946 does not block — RH5005/RH5006 are in #977, the `await` form is covered by SHARED-STMT-004/SHARED-FIX-001 and fixed later by #946. No parallel runs. Order chosen by coordinator. 2026-10-18: maintainer has budget for 2 parallel squads — run two independent batches at once (never a batch with its blocker).
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
