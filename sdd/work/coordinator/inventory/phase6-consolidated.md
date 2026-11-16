# Phase 6 consolidated (2026-11-16)

249 issues inventoried (slices 1–8): open 219, partly done 19, done 6, superseded 4, contradicts 1. Themes: step 3 (pending).

## Step 2 — proposed closures (asked 2026-11-16)

| # | Verdict | Evidence |
|---|---|---|
| 1054 | done/obsolete | `squad/decisions/` removed in #1481; lane wording remains in #1471 |
| 1244 | superseded by #1271 | the issue's own comment |
| 1259 | done | `EmptyParenthesesCodeFixProviderBase<TNode>` (#1599) is the base of RH3106/RH3107 fixes |
| 1453 | done | RH6001-001 lists `fixed`; tagged test exists |
| 1534 | superseded | FMT-UNSUP-008 (#1718) + RH5308-005 |
| 1546 | done | RH5104 already reports comments sharing a line with code |
| 1584 | done | SHARED-DIAG-001 + Roslyn skip `[GeneratedCode]`/`generated_code`; #1585 stays |
| 1590 | superseded by #1759 | general SHARED-DIAG-008 reversal |
| 1681 | done | option B shipped in #1700 |
| 1741 | superseded by #1759 | reversal absorbs the RH5101-004 reading and the spec walk |
| 1601 | contradicts #1759 + "Special cases do not grow the source" | raw-string remainder -> #1689 |
| 1353 | fold into #1450 | only RH8402 Q2 mixed-run sentence left; same rule/spec |
| 1605 | fold into #1774 | RH3004 symptom gone; remainder = partial write-back design |
| 957 | fold into #1077 | part 1 done (#1510); part 2 is a consequence of #1077 |
| 1553 | unreachable | only known instance fixed; no reachable input named |

Not closed (step 4 judges): #1537 (guards kept = valid end state), #1555 (premise weak, check first), #1562 (closes with its regression test).

## Narrowings (partly done; comment "done: … / remains: …")

#955 #956 #1077 #1081 #1131 #1140 #1291 #1330 #1394 #1400 #1440 #1505 #1533 #1544 #1562 #1587 — remainder text in the slice files' Closure candidates sections.
