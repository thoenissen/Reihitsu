# Lead — field-split-guards

- Issues: thoenissen/Reihitsu#963 (`agent-filed`), thoenissen/Reihitsu#1384 (`agent-filed`) — goals are proposals.
- Slug: `field-split-guards`
- Branch: `claude/magical-albattani-jl4e5j` (session-designated branch; see decision 1). Draft PR: thoenissen/Reihitsu#1660.
- Lane: **bug fix**.
  - #963: after triage (issue comment of 2026-10-24) the issue keeps only the field-splitting part: a comment between the modifiers/type (also attribute list, per the 2026-10-21 comment) and the first declarator must block the split instead of being copied onto every field. FMT-STRUCT-024 (P36) already requires it; the `{// why` expression-body part moved to #1439 and is out of scope.
  - #1384: a documented field declaration (`///` or `/** */`) must not be split. Triage: already required by FMT-STRUCT-024 on main → bug fix without change doc; FMT-STRUCT-022's contradicting example is aligned as a clarification. The RH7101 fix offer follows `SplitsFieldDeclaration` (FMT-PIPE-031), so RH7101 tests change with it. A code-fix regression test (e.g. RH7103 on a documented `public int X, Y;`) was requested in an issue comment.
- Roles: spec author (clarification of FMT-STRUCT-022 and any spec text that disagrees with FMT-STRUCT-024) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager only if follow-up candidates exist.

## Maintainer answers

- (from issue thread, before the run) #1384: FMT-STRUCT-024 wins over FMT-STRUCT-022's example — a documented field declaration is not split; FMT-STRUCT-022's example is aligned in the same bug run, and the RH7101 tests change with it.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session-designated branch `claude/magical-albattani-jl4e5j` instead of `claude/issues-963-1384-<slug>` | The session's environment forbids pushing to any other branch | Branch names are internal | claim commit |
| 2 | Lane | Lane for both issues | Bug fix; spec author runs for a clarification only | FMT-STRUCT-024 already demands both guards; triage recorded the lane | Unchanged requirement decides the output | — |

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: spawn spec author (clarification).
