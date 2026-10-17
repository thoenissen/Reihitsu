# SDD coordinator state

Parent: #899. Branch: `claude/sdd-coordinator` (never merged; `sdd/work/` stays out of PRs to main).

## Working agreement

- Several questions per round are fine.
- A clear answer from the maintainer is approval to post the decision comment and tick the box; no separate ok needed.
- Creating new issues still needs an ok.
- No change docs until all SDD documents are finished (maintainer, round 5). "Different intended behavior" decisions add the requirement in the implementing PR without a change doc. The rulebook still says otherwise; reconcile in #942.

## Phase

1 — decision round for open questions (#912, sub-issues #913–#934). No code.

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

## Running

- nothing

## Waiting for maintainer

- #920 Q1, #916 Q1 (analyzer/formatter disagree), rest of #918 (Q1, Q2, Q4, Q6)

## Collected for later phases

- Clarifications per spec batch: —
- Bug issues to draft (phase 2): #919 Q1, #919 Q3, #930 Q1, #930 Q2 (maybe one issue for RH2005), #926 Q1, #926 Q2, #928 Q1, #929 Q1, #922 Q1(+#921 Q3), #922 Q2, #922 Q3, #921 Q2, #918 Q3, continuation rule (#921 Q1/#918 Q5/#913 Q2+Q4)
- Cleanup issues (phase 2): #919 Q2, #921 Q4
- #946 (RH5005/RH5006 `await using`/`await foreach` fix never converges) → phase 2; blocks RH5005/RH5006 specs.
- #939 split → phase 3.
- #942 → phase 5.

## Issues fully decided (close when follow-ups exist)

- #919, #930, #926, #928, #929, #922, #921
