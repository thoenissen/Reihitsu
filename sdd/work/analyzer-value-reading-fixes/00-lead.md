# Lead — analyzer-value-reading-fixes

- Branch: `claude/keen-ptolemy-bznmq5` (the session's designated branch; the harness forbids pushing elsewhere, so the playbook's `claude/batch-1071-...` name is not used)
- Draft PR: thoenissen/Reihitsu#1692
- Base at claim: `c2866eaf294a9040b995a2b75de7587bdccc5efe`
- Lane: **bug fix**

## Issues (prompt order)

| Issue | agent-filed | Lane | Reason |
|---|---|---|---|
| thoenissen/Reihitsu#1071 | yes | bug fix | RH4001 and RH4009 compare names as raw text (with `@`, whitespace, comments) against SHARED-DIAG-002; triage: bug, coverage sentence of the rule specs widened as a clarification in the same PR |
| thoenissen/Reihitsu#1268 | yes | bug fix | RH2003 misses `throw new NIE()` through a using alias against SHARED-DIAG-002 (alias amendment); triage: bug, rule spec text aligned as a clarification |
| thoenissen/Reihitsu#1289 (→ `Refs`, decision 13) | yes | bug fix | RH2109 reports `@code {` inside C# comments/strings, Razor/HTML comments, escaped `@@`, email-like text against SHARED-DIAG-003; the `@functions` part moved to #1452 and is not in this run |
| thoenissen/Reihitsu#974 | yes | bug fix | RH0001 does not report content after the root object of `reihitsu.json` against RH0001-003; check the CLI's configuration loading as well |

All goals are proposals (`agent-filed`). Blockers #1069 and #1260 are closed.

## Roles

Spec author (clarifications only: rule-spec coverage sentences for #1071/#1268, and any other rule-spec text that contradicts the shared requirement) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if there are follow-up candidates.

## Maintainer answers

- Q1 (RH4001/RH4009, name with a Unicode escape such as `\u0041bc`): **A** — compared by value: `\u0041bc` in `Abc.cs` not reported; fix renames to `Abc.cs`; `namespace \u0041.B` matches entry `A.B`.
- Q2 (RH4009, `reihitsu.json` with content after the root object): **A** — RH0001 reports the syntax error at the first non-white-space character after the root value; RH4009 reports nothing, like with any other malformed file.
- Q3 (RH2109, `@code {` in the middle of a line outside comments/literals, e.g. `<div>@code { int X; }</div>`): **A** — not reported; only a `@code` that starts a line (after optional white space) is a directive.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/keen-ptolemy-bznmq5` | Harness forbids pushing to other branches | Internal | claim commit |
| 2 | Skeptic | #974: CLI goal | Drop; the CLI loads no `reihitsu.json` | Skeptic 02 | Nothing changes | — |
| 3 | Skeptic | #974: RH0001 side of the spec author's question | Decided by unchanged RH0001-003 ("one problem", no further validation): only the syntax error is reported; only RH4009 is asked | Skeptic 02 | Unchanged requirement decides it | — |
| 4 | Skeptic | RH0001-003 wording | Say "JSON white space" | Skeptic 02; avoids Unicode reading | Normative "not valid JSON" decides | spec author amendment |
| 5 | Skeptic | RH4009-002 dash clause restating SHARED-DIAG-002 | Keep only the reference; keep the "entries compared as written" sentence, with a test | Skeptic 02 | Follows from unchanged "exactly one of the entries" + SHARED-DIAG-002 | spec author amendment |
| 6 | Skeptic | #1071 RH4009 implementation | Reuse `NamespaceCasingHelper.GetLocations` rather than a private copy (recommendation to implementer via handover) | Skeptic 02 | Internal | — |
| 8 | Spec author 03 | RH2109 spec vs. decision 7 gap | Keep the spec as written (not narrowed); the remaining hosts are a known bug carried by the follow-up | A rule spec cannot narrow SHARED-DIAG-003; narrowing would document a defect as intent | Remaining hosts keep `main`'s output | — |
| 9 | Implementer 05 | RH2109 size against decision 7 | Accept: no new type, 91 non-blank non-doc added lines (< ~100), total production +183/−15 | Within the limit | Internal | d3c005c |
| 10 | Implementer 05 | Disputed test `VerifyCodeBlockLineInsideInterpolatedRawStringIsNotReported` (`$$` read as markup in the additional file) | Fixture fix, assertion unchanged → fresh test implementer | Implementer proved the assertion holds with markup off | Assertion unchanged | — |
| 11 | Auditor 11 | RH2109 findings #1 (comment openers `@*`/`<!--` outside lexed blocks swallow later real directives) and #2 (markup with `{`/`/*` inside a `@code` block keeps the block open, swallowing later directives) — both PR-introduced misses of reports `main` makes | Repair round 2: restore `main`'s report for every counterexample the auditor lists; where a cheap syntactic check cannot tell text from a directive, the result falls back to `main`'s behavior (reported), never to a missed real directive. No new type, cheap checks only; the remaining false positives in those rare shapes join the decision-7 follow-up | RH2109-001 unchanged requires the report; a regression against `main` is not shippable | Restores `main`'s output; nothing new decided | — |
| 12 | Implementer 13 | Remaining RH2109 miss: a line-starting `@*`/`<!--` inside a multi-line C# literal or comment of an unlexed host (`@{ }`, control flow, expression) followed by a real directive before its terminator | Accept for this run; joins the decision-7 follow-up | Closing it needs the host-extent scanner of decision 7; the shape is rare (a comment opener starting a line inside a multi-line C# string in a non-`@code` host) | Rare input class; `AGENTS.md` *Special cases do not grow the source* | 0de22a4 |
| 13 | Auditor 16 | RH2109 findings #3 (repeats #2: markup inside `@code`/`@functions` C# — `__builder` markup, `<text>`, inline markup — keeps the lexed block open) and #4 (repeats #1: line-starting comment opener in the after-template rest of a block) | Architecture finding (repeated class), refuse the class: drop the C# block lexing of `@code`/`@functions` blocks altogether. C# comments and literals inside those blocks are read like the rest of the file, as on `main` (a line-starting `@code {` inside them is reported again). Kept: line-start-only directives (Q3), line-starting Razor/HTML comment openers, `@@`, `@` after a letter or digit. Decision 12's accepted miss (line-starting `@*`/`<!--` inside a multi-line C# literal or comment) now covers every C# host, `@code`/`@functions` included. Tests asserting the C# literal/comment cases inside blocks are turned to assert `main`'s report. #1289 is linked `Refs` (its C#-literal instances are not met); the rest goes to the follow-up | Two audits found members of the same class; modelling where Razor C# ends is parser-like machinery (`AGENTS.md`, *Special cases do not grow the source*) | The refused class keeps `main`'s output; the miss is a rare shape | — |
| 7 | Skeptic | #1289 scope | Narrow option: Razor/HTML comments, `@@`, letter/digit-before-`@` everywhere; C# comments and literals only inside `@code`/`@functions` blocks; C# kinds in `@{ }`, control-flow and expression hosts stay reported → follow-up. Escalation 9 if it exceeds ~100 production lines or needs a new type | Issue instances say "within a code block"; covers common member (multi-line `@* *@` around a block) | Remaining hosts keep today's output | — |

## Follow-up candidates

- `sdd/README.md` (Analyzer specs) says `shared/diagnostics.md` applies without being listed under `Uses`, but `SpecStructureTests` requires referenced SHARED-DIAG IDs to be listed — reword the rulebook — spec author 09
- SHARED-DIAG-002 does not name Unicode escapes for every rule; Q1 was written only into RH4001/RH4009 — spec author 03
- RH4009 nested namespace declarations (`namespace A { namespace B { } }`): no spec decides which name is compared — skeptic 02, #1071
- RH2109 reads no C# extent (decision 13): (a) a `@code {` that starts its own line inside a multi-line C# comment or literal — in any host, `@code`/`@functions` blocks included — is still reported, against RH2109-001/SHARED-DIAG-003 (the C#-literal instances of #1289); (b) a line-starting `@*`/`<!--` inside a multi-line C# literal or comment opens a comment and hides a later real directive (decisions 12, 13; e.g. auditor 16 finding #4 input); (c) a multi-line Razor/HTML comment whose opener is behind other text on its line is not recognized, so a `@code {` line inside it is reported as on `main` — decisions 7, 11, 12, 13; skeptic 02, implementer 13, auditor 11/16; #1289 (issue manager may extend #1289 instead of filing new)

## Worklist / next step

- 01 spec author done (575d1a7): specs clarified for all four; one output question open for #974 (config with content after root object: RH0001 only syntax error + RH4009 silent [A, recommended] vs. also unknown section + RH4009 reports [B]); batched before test implementer.
- 02 skeptic done (11f0710): all four proceed. Output batch asked (Q1 Unicode escape names RH4001/RH4009; Q2 RH4009 with content after root; Q3 RH2109 mid-line `@code`). Answered 1A, 2A, 3A (as recommended).
- 03 spec author amendment done (38006c8).
- 04 test implementer done (752f3ac): all four reproduced; 61 failing new tests, 14 guard tests passing.
- 05 implementer done (d3c005c): 8591/8592 analyzer tests pass; one disputed fixture.
- 06 fixture repaired (0eb9d98); RH2109 33/33.
- 07 cleaner clean (e85cfc0).
- 08 spec checker: findings (e6dfe88) — `Uses` header missing in RH4001, RH4009, RH2003, RH2109 specs (architecture test fails).
- Repair round 1: spec author 09 fixed Uses lists (8ceb205); architecture 117/117. Cleaner skipped (spec text only).
- 10 spec checker consistent (0703015).
- 11 auditor (5df37b7): #1071, #1268, #974 pass; #1289 two high findings (decision 11).
- 12 test implementer: 13 new RH2109 tests (8ebbe37), all fail on head, pass on main analyzer.
- 14 cleaner clean (2f389e3). 15 spec checker consistent (9734aea). 16 auditor (5663397): #1, #2 closed; #3, #4 repeat classes → decision 13.
- 17 test implementer (80415db): 9 tests turned to main's report (characterizations, untagged), 7 new for finding #3; 16 fail on head.
- 19 cleaner clean. 20 spec checker consistent (e50b582). Next: auditor 21 (repair only).
- 18 implementer (b8b466e): block lexing removed; RH2109 53/53, analyzer 8612/8612; RH2109 56 code lines.
- Repair round 3 (last): test implementer (17, done) → implementer (18) → cleaner (19) → spec checker (20) → auditor (21, repair only) → validator (22).
- 13 implementer (0de22a4): 46/46 RH2109, 8605/8605 analyzer; RH2109 95 code lines.
- Repair round 2 (round 1 = Uses lists): test implementer (12, done) adds tests for the auditor's counterexamples → implementer (13) → cleaner → spec checker → auditor (repair only) → validator.
