# Lead — thoenissen/Reihitsu#1089

- Issue: thoenissen/Reihitsu#1089 — "Check query range variables (RH4124) and extension-block receivers (RH4122)"; labels `enhancement`, `analyzer`, `agent-filed`.
- Slug: `rh4124-rh4122-gaps`
- Branch: `claude/sleepy-johnson-4n46u9` (session-designated); draft PR thoenissen/Reihitsu#1974.
- Lane: **behavior-change** — the issue requests new behavior (two rules report constructs they do not report today) and itself asks for a change doc first.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if there are follow-up candidates. None skipped.

## Maintainer clarifications (from the issue text)

The issue records the maintainer's decisions of question round 3 (2026-10-18), summarized:

1. RH4124: query range variables introduced by `from`, `let`, `join`, and `into` are checked like lambda parameters (example: `from x in values let y = x * 2 select y`, where `x` and `y` go unchecked today while `values.Select(x => x)` is checked).
2. RH4122: the receiver of a C# 14 extension block (`extension(string t) { … }`) is reported like the receiver `this int s` of an extension method; RH4105 already checks `t`.
3. Done when: both inputs are reported, and fixed where the rule's fix applies, with regression tests.

The label makes the goals proposals, but the two decisions above are the maintainer's recorded answers; departing from them is treated as an output question.

## Maintainer answers

1. RH4124 title/message once range variables are reported — maintainer chose **B**: title and message become "Single-letter lambda, anonymous method, and query range variable names should not be used"; rule-page heading and package README row follow.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | spec author | New requirements or widen existing -001? | New RH4124-003 and RH4122-004 | New tests carry their own tags; existing requirements stay intact | Spec structure only; the visible change itself is the maintainer's decision on #1089 | 6a0ff3c |
| 2 | spec author | SHARED-DIAG-007 Uses row for unnamed extension receiver? | None | An unnamed receiver has no name to check, as RH4105-004 treats it | Unnamed receiver is not reported either way | 6a0ff3c |

## Follow-up candidates

- Casing of query range variables is unchecked (RH4105 reports `Value => Value` in a lambda, not `from Value in values`); carries the skeptic's output question (recommended: RH4105 reports range variables with its rename fix). Source: 02-skeptic.md. Outside #1089, so not asked.

## Worklist / next step

- Claim done (commit on branch, draft PR #1974), SDK installed.
- Spec author done (6a0ff3c): change doc Draft, RH4124-003, RH4122-004.
- Pending output question for the batch before the test implementer: RH4124 title/message wording (spec author's OQ1, A keep / B widen, recommends B).
- Skeptic done (92c5751): proceed; premise reproduced for both. Internal amendments for the spec author: reword RH4124-003 *Why* (transparent identifiers), mention RH3007 overlap in the change doc; PR to say neither rule has a fix.
- Visible changes 1-2 (new reports) are covered by the maintainer's decisions recorded in #1089; not re-asked.
- Output question batch sent: Q1 RH4124 title/message (A keep / B widen, recommended B). Answered: B.
- Spec author amendment done (e78d882): 1B carried out, Status Accepted. Note for implementer: widen RH4124 analyzer class <summary>.
- Test implementer done (fe5cd25): 9 RH4124-003 + 7 RH4122-004 tests; positives fail on missing diagnostics only.
- Next: implementer.
