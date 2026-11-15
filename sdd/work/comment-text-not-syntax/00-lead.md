# Lead — comment-text-not-syntax

- Issues: thoenissen/Reihitsu#966 (`agent-filed`), thoenissen/Reihitsu#1289 (`agent-filed`) — goals are proposals.
- Slug: `comment-text-not-syntax`
- Branch: `claude/determined-faraday-q5zic1` (session-designated branch; see decision 1). PR: thoenissen/Reihitsu#1745 (draft).
- Lane: **bug fix**, for both issues.
  - #966: observed-vs-expected difference in shipped behavior (using-directive ordering safety check in `Reihitsu.Formatter`/`Reihitsu.Core`, shared by RH7201–RH7207 and the formatter's using phase).
  - #1289: observed-vs-expected difference in RH2109 against RH2109-001 and SHARED-DIAG-003.
- Roles run: skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Spec author only for a clarification. Issue manager before publishing if follow-up candidates exist.

## Issue scope (maintainer clarifications, summarized)

- #966: The triage comment of 2026-11-15 narrows the issue to case 1 only: the `global::` sort-key part was already fixed on `main` (SHARED-USING-004/-007). Remaining: a line starting with `#` inside a multi-line comment (including a file header before the first `using`) blocks reordering and silences RH7201–RH7207 because of a textual regex check; only real preprocessor directives should block reordering.
- #1289: Q1–Q3 instances (comments, strings, Razor/HTML comments, `@@`, email-like text) were already fixed; the `@functions` part moved to #1452. Remaining: `@code {` starting its own line inside a multi-line C# comment or literal within a Razor host block is still reported. The issue leaves it open whether a cheap syntactic check can decide this or whether the class stays reported; done when that input has a regression test with the decided result.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name `claude/issues-966-1289-<slug>` vs. session branch | Use the session-designated branch `claude/determined-faraday-q5zic1` | The cloud session only permits pushes to its designated branch | Branch name is internal | 0c615d1 |

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: skeptic (premise + reachability for both issues).
