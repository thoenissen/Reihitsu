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
| thoenissen/Reihitsu#1289 | yes | bug fix | RH2109 reports `@code {` inside C# comments/strings, Razor/HTML comments, escaped `@@`, email-like text against SHARED-DIAG-003; the `@functions` part moved to #1452 and is not in this run |
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
| 7 | Skeptic | #1289 scope | Narrow option: Razor/HTML comments, `@@`, letter/digit-before-`@` everywhere; C# comments and literals only inside `@code`/`@functions` blocks; C# kinds in `@{ }`, control-flow and expression hosts stay reported → follow-up. Escalation 9 if it exceeds ~100 production lines or needs a new type | Issue instances say "within a code block"; covers common member (multi-line `@* *@` around a block) | Remaining hosts keep today's output | — |

## Follow-up candidates

- SHARED-DIAG-002 does not name Unicode escapes for every rule; Q1 was written only into RH4001/RH4009 — spec author 03
- RH4009 nested namespace declarations (`namespace A { namespace B { } }`): no spec decides which name is compared — skeptic 02, #1071
- RH2109 `@code {` inside C# comments/literals in `@{ }`, control-flow, and expression hosts (outside `@code`/`@functions` blocks); after Q3 the remaining gap is a `@code {` starting its own line inside a multi-line C# comment or literal in those hosts, which RH2109-001/SHARED-DIAG-003 still require not reported (bug against the unchanged spec) — decision 7, skeptic 02, spec author 03, #1289

## Worklist / next step

- 01 spec author done (575d1a7): specs clarified for all four; one output question open for #974 (config with content after root object: RH0001 only syntax error + RH4009 silent [A, recommended] vs. also unknown section + RH4009 reports [B]); batched before test implementer.
- 02 skeptic done (11f0710): all four proceed. Output batch asked (Q1 Unicode escape names RH4001/RH4009; Q2 RH4009 with content after root; Q3 RH2109 mid-line `@code`). Answered 1A, 2A, 3A (as recommended).
- 03 spec author amendment done (38006c8).
- Next: test implementer (04).
