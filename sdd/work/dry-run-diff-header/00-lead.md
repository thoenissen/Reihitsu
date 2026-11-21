# Lead — dry-run-diff-header

- Issue: thoenissen/Reihitsu#975 — `--dry-run` diff header prefixes a full path with `a/` and `b/`. Labels: enhancement, formatter, `agent-filed` (goals are proposals).
- Slug: `dry-run-diff-header`
- Branch: `claude/serene-sagan-dkz5bo` (session-designated). Draft PR: #1922.
- Lane: **behavior-change**. Reason: the example of CLI-013 in `sdd/specs/cli/reihitsu-format.md` pins today's header (`--- a//work/src/A.cs`), and the triage comment on #975 reclassified the issue as an extension for that reason; changing a specified CLI output needs a change doc.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if there are follow-up candidates. None skipped.

## Maintainer's clarifications (summarized)

- In thoenissen/Reihitsu#933 (comment 6233289816, `reihitsu-format.md` Q1) the maintainer decided the absolute-path header is a bug to be fixed.
- Triage comment on #975: reclassified as an extension because the CLI-013 example pins the current header.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## State

- Last step finished: claim (commit 6590112), `scripts/prepare.sh` green.
- Next: spec author.
