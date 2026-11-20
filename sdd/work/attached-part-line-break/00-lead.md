# Lead — attached-part-line-break

- Issue: thoenissen/Reihitsu#892 — "Report a line break in front of an attached part of a member-access chain". Labels: enhancement, analyzer, `agent-filed` (goals are proposals).
- Slug: `attached-part-line-break`
- Branch: `claude/trusting-euler-pkbd73` (session-designated). Draft PR: thoenissen/Reihitsu#1892.
- Lane: **behavior change / new rule** — the issue requests a new analyzer diagnostic (or an extension of RH5201) with a code fix; a new or changed diagnostic is new behavior and needs a change doc.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if there are follow-up candidates. None skipped.

## Context from the issue thread (summarized)

An agent comment on the issue (2026-11-18) reports that, since the formatter change for thoenissen/Reihitsu#895, the formatter joins `[`, `?[` and a trailing `!` written on the line below their element everywhere, not only in chains with a link (FMT-BREAK-040; no join across a comment, directive, or disabled text). Argument lists were already joined everywhere. So the issue's scope restriction "chains with a member access only, except `?[`" no longer matches the formatter, and the formatter owner is now the attached-part gap in the line-break phase, not `ChainLineBreakRewriter.NormalizeChain`. The argument list's `(` outside chains is the subject of a separate issue ("New rule: the opening parenthesis of an argument list is on the line of the code in front of it (counterpart of RH5105)"). Whichever rule is built must state the scope per part. This comment is itself agent-written, so it is a hypothesis like the issue.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

- Comment on thoenissen/Reihitsu#1827 that this run leaves every argument list's `(` to it, if output question 1A stands (01-spec-author.md).

## State

- Last step finished: spec author (01, commits cd904e8, 6e77337; change doc Draft, rule RH5115; 4 output questions open).
- Next: skeptic (02), then batch output questions before the test implementer.
