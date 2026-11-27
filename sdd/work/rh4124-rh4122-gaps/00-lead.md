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

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## Worklist / next step

- Claim done (commit on branch, draft PR #1974), SDK installed.
- Next: spec author.
