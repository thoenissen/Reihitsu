# Lead — using-ordering-by-value

## Run

- Issues (prompt order): thoenissen/Reihitsu#1105, thoenissen/Reihitsu#1266, thoenissen/Reihitsu#1267 — all three `agent-filed` (goals are proposals).
- Slug: `using-ordering-by-value`
- Branch: `claude/upbeat-newton-0kso6f` (session-mandated branch; see decision 1). Draft PR: thoenissen/Reihitsu#1735.
- Lane: **bug fix**, all three issues.
  - #1105: reported observed-vs-expected difference against SHARED-DIAG-002 (names compared by value): RH7201, RH7203, RH7204, RH7206, RH7207 and the formatter's using phase must order/group `System . IO`, `System.IO`, `@System.IO`, and a name with a comment inside identically.
  - #1266: observed-vs-expected difference against SHARED-USING-007 (compare with the previous directive of the same group, whatever lies between): RH7203, RH7204, RH7206. Maintainer triage comment: bug-fix lane, no change doc; RH7203-001, RH7204-001, RH7206-001 are aligned to SHARED-USING-007 as a clarification in the same PR. P18 clarified: the partner is every earlier element of the same group, not only the adjacent one.
  - #1267: observed-vs-expected difference against SHARED-USING-004 (an alias whose target is not a name forms its own group after every other group of its section): RH7204, RH7207, and the formatter (`UsingGrouping`); tuple targets too; formatter idempotent on the result. Issue calls for a spec clarification.
- Roles: spec author (clarifications only: rule specs RH7203/RH7204/RH7206/RH7207 aligned to the shared spec) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing when follow-up candidates exist.
- Blockers named in the issues (#1069, #1260) are closed; SHARED-DIAG-002, SHARED-USING-004 and SHARED-USING-007 carry the decided wording on `main`.

## Maintainer answers

- Q1 (Unicode escape in a using name; RH7201/RH7207/formatter): **A, generalized.** Every name is ordered and grouped by its technical (compiler-interpreted) value: `@` is dropped and Unicode escapes are decoded. `using \u0053ystem.IO;` is in the `System` group; RH7201 reports it in the input `Microsoft.Win32; \u0053ystem.IO; System`, and the formatter places it after `using System;` in the `System` group.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | `claude/upbeat-newton-0kso6f` instead of `claude/issues-1105-1266-1267-<slug>` | The session environment permits pushing only to this branch | Branch names are internal | 570f2ad |
| 2 | Skeptic | Does by-value name comparison also bind the formatter's using phase? | Yes; the formatter's order changes through the shared Core policy | 02-skeptic: formatter `UsingGrouping` delegates to Core `UsingDirectiveOrderingUtilities`, which RH7207 also uses; SHARED-USING-004 and FMT-USING-001 demand one order; FMT-PIPE-004 idempotence is broken on `main` by the spelling read | Demanded by unchanged requirements (SHARED-DIAG-002, SHARED-USING-004, FMT-USING-001, FMT-PIPE-004, SHARED-FIX-001/005); the PR lists the moved directives under Review notes | |
| 3 | Skeptic | Architecture finding FC-015: order within a group decided in Core `ComputeCanonicalOrder` and in three drifted loops in RH7203/RH7204/RH7206 | Change the structure in this run: the three analyzers take the within-group comparison from Core instead of their own loops | A narrow fix would restate SHARED-USING-007 three more times; the change stays in the components the run changes and well under 100 lines | Output is what SHARED-USING-007 already demands | |
| 4 | Skeptic | #1267 goals: RH7204 change, SHARED-USING-005 mention, two non-name aliases in one section | RH7204 gets only a characterization test; SHARED-USING-005 dropped from scope; order between two non-name aliases stays as on `main` (not locked by new tests) | 02-skeptic: RH7204 already correct for the instance; SHARED-USING-004 does not decide two non-name aliases | Nothing changes for those outputs | |
| 6 | Test impl. | When to fix the RH7201 spec-example markup gap | Batch it with the spec checker's findings instead of a separate spawn now | One-line example markup; the test already follows the requirement text | The requirement already demands the second report, and `main` reports it | |
| 7 | Implementer | Pre-existing RH7203 test asserts compare-with-adjacent-only (1 diagnostic) | Test is wrong: mark `…CodeActions` and `…CodeFixes` too, expect 3; fresh test implementer (the earlier one is large) | Amended RH7203-001 aligns to SHARED-USING-007, which the maintainer's P18 clarification on #1266 decides (every earlier element of the group) | The extra reports are demanded by SHARED-USING-007 and the maintainer's P18 clarification; the fixed code is unchanged | |
| 5 | Skeptic | SHARED-USING-004 wording "between its parts" | Spec author amends to cover `global ::` and type-argument trivia (wording "between its tokens" plus examples for `global :: System.IO` and a generic static type name) | Both are failing members of the #1105 class | Same outputs the agreement and FMT-PIPE-004 already demand | |

## Follow-up candidates

- SHARED-DIAG-002 names `@`, whitespace and comments but not Unicode escapes, while RH4001 and SHARED-USING-004 decode them; the maintainer's Q1 answer ("order everything by the name as the compiler interprets it") suggests SHARED-DIAG-002 should state escape decoding for every name comparison — spec-wording follow-up carrying that answer (03-spec-author).
- `UsingDirectiveOrderingUtilities.OrderUsings` (Reihitsu.Core) has no production caller, only tests: dead-code removal (02-skeptic).

## Worklist / next step

- Claimed (570f2ad, draft PR #1735), SDK prepared.
- Spec author done (8d25ac5, 01-spec-author.md): clarification only, no change doc, no output questions; lane check confirmed bug-fix. Open: its internal choice that by-value comparison binds the formatter (formatter output changes for `@System.IO` etc.) — skeptic asked for a verdict; lead decides after 02, or asks as an output question if nothing unchanged demands it.
- Skeptic done (39ebd16, 02-skeptic.md): #1105 proceed + output question (Unicode escape); #1266 proceed; #1267 proceed narrower.
- Output question 1 answered (A, generalized: technical value, `@` dropped, escapes decoded).
- Spec amendment done (6f629b1, 03-spec-author.md). No further output questions.
- Test implementer done (5c42d88, 04-test-implementer.md): all three reproduced (51 analyzer + 19 formatter tests fail as intended).
- Open spec worklist (for the next spec-author spawn, batched with spec-checker findings): RH7201-001 Unicode-escape example in `sdd/specs/analyzer/RH7201.md` must also mark `using {|RH7201:System|};` (04-test-implementer).
- Implementer done (af78e5d, 05-implementer.md): all handed-over tests pass; disputed pre-existing test `RH7203...Tests.MicrosoftUsingsAreFixedWithoutMovingSortedSystemGroup` (expects 1, gets 3).
- Disputed test corrected (c7c7e64, 06-test-implementer.md); RH720 analyzer tests 232/232.
- Next: cleaner (running), then spec checker (plus RH7201 example worklist item to spec author).
