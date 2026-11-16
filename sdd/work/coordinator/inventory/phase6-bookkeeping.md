# Phase 6 bookkeeping (2026-11-16)

Applies the maintainer's answer "1a 2a 3a 4a" to `phase6-consolidated.md` and `phase6-triage.md`. Every comment ends with the Claude Code footer. All states and milestones re-checked via the REST API afterwards; no failures. Nothing committed.

## 1. Closures

| Issue | Action | Result |
|---|---|---|
| #1054 | comment (removed `squad/decisions/` in #1481; rest in #1471) + close completed | closed, completed |
| #1259 | comment (`EmptyParenthesesCodeFixProviderBase<TNode>`, #1599) + close completed | closed, completed |
| #1453 | comment (RH6001-001 lists `fixed`; tagged test) + close completed | closed, completed |
| #1546 | comment (RH5104-001 already reports it) + close completed | closed, completed |
| #1584 | comment (SHARED-DIAG-001 + Roslyn; #1585 stays) + close completed | closed, completed |
| #1681 | comment (option B shipped in #1700) + close completed | closed, completed |
| #1244 | comment (superseded by #1271) + close duplicate of #1271 | closed, duplicate |
| #1534 | comment (FMT-UNSUP-008 from #1718 + RH5308-005) + close not_planned | closed, not_planned |
| #1590 | comment (superseded by #1759) + close duplicate of #1759 | closed, duplicate |
| #1741 | comment (superseded by #1759) + close duplicate of #1759 | closed, duplicate |

## 2. Close and fold

| Issue | Action | Result |
|---|---|---|
| #1689 | comment carrying #1601's raw-string remainder (comment of 2026-11-13, input + today/if offered) | posted (issuecomment-6912845783) |
| #1601 | comment (contradicts #1759 and *Special cases do not grow the source*; link to #1689 comment) + close not_planned | closed, not_planned |
| #1450 | comment with the remaining RH8402 Q2 mixed-run sentence | posted (issuecomment-6912846644) |
| #1353 | comment (Q1 done by RH8402-006; link to #1450 comment) + close completed | closed, completed |
| #1774 | comment stating the layout-only node mode remainder of #1605 | posted (issuecomment-6912847644) |
| #1605 | comment (RH3004 symptom gone; link to #1774 comment) + close completed | closed, completed |
| #1077 | comment that #957 part 2 follows from it | posted (issuecomment-6912848367) |
| #957 | comment (part 1 done by #1510; link to #1077 comment) + close completed | closed, completed |
| #1553 | comment (only known instance fixed; no caller reaches another case, *Supported inputs*) + close not_planned | closed, not_planned |

## 3. Narrowing comments ("already done: … / remains: …"; bodies untouched)

Claims spot-checked against main (FMT-BREAK-037/-038, FMT-SPACE-001/-018..-024, RH8402-006, RH5111-005, FMT-UNSUP-004/-008, RH5308-005, `ObservedPipeline`/`FormatterPhaseTestsBase`, AGENTS.md table rows, `GetStringLineIndicesReturnsLinesForAllSupportedStringVariants`, `OneSpaceAfterFixedKeyword`).

| Issue | Done / remains | Result |
|---|---|---|
| #955 | FMT-BREAK-037 / colon-ended ctor initializer | posted |
| #956 | #1635 leading commas / `)` placement, switch-expression arms | posted |
| #1077 | #1386, #1280 / RH6xxx spacing rules + formatter | posted |
| #1081 | FMT-PIPE-007, RH5025-005 / sibling braces in RH5025-001, U+0085/U+2028 | posted |
| #1131 | FMT-BREAK-012 Q1 / `Uses` row, lambda attribute list | posted |
| #1140 | #1760 / RH5110, RH5403/RH5404 headers, unmeasured (needs #1774) | posted |
| #1291 | struct-equality.md / FMT-BREAK-018 and -021 wording | posted |
| #1330 | interpolated raw + `u8` test / multi-line UTF-8 raw, `///` lines | posted |
| #1394 | item 8 (RH5111-005) / items 10, 12 | posted |
| #1400 | item 24 (FMT-SPACE-001) / items 22, 23 | posted |
| #1440 | FMT-SPACE-009/-018/-019/-020/-023/-024 / element access, invocation, `typeof (`, `::`, initializer braces | posted |
| #1505 | `ObservedPipeline` parse options / `FormatterPhaseTestsBase`, `SecondPassInput.Create` | posted |
| #1533 | FMT-BREAK-033..035 / `/** */` behind `{` and earlier comment | posted |
| #1544 | `LineBreakPhase` row / `BraceBlankLinePolicy` in `BlankLinePhase` row | posted |
| #1562 | option A via #1496/#1498 / regression test | posted |
| #1587 | FMT-UNSUP-004 / comment-free repeated break and column (#953) | posted |

## 4. Triage

| Issue | Action | Result |
|---|---|---|
| #1780 | milestone Maintenance (4) | set; labels unchanged |
| #1781 | milestone vNext (2) | set; labels unchanged |
| #1785 | milestone vNext (2) | set; labels unchanged |
| #1786 | milestone vNext (2), add label `analyzer` (full list passed) | set; labels enhancement, analyzer, formatter, agent-filed |
