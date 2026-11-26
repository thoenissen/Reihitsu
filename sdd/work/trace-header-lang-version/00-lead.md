# Lead — trace-header-lang-version

- Issues: thoenissen/Reihitsu#1924 (agent-filed), thoenissen/Reihitsu#1949 (agent-filed). Goals of both are proposals.
- Branch: `claude/gallant-hypatia-56li53` (session-designated). Draft PR: #1966.
- Slug: `trace-header-lang-version`
- Lane: **behavior-change** (both issues).
  - #1924: requested changed (or explicitly recorded) header of the pipeline trace; the trace command lives in `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs`, a production path by the lane-check table.
  - #1949: a new `--lang-version` option for `scripts/apply-fix.sh` (fixture runner in `src/Reihitsu.Tooling`, tooling) and `scripts/trace.sh` (needs `PipelineTraceCommand`, production). Production next to tooling needs a change doc, so the narrowest admitting lane is behavior-change. The issues say "Lane: tooling"; the lane check decides.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.
- Maintainer clarification: a comment on #1949 says the two are implemented together in one run because both touch the trace and fixture tooling.
- Note: `sdd/specs/cli/reihitsu-format.md` states the pipeline trace is a repository tool, not part of `reihitsu-format`.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Lane for issues labeled "Lane: tooling" | behavior-change | The diff will touch `src/Reihitsu.Cli` (production) and `src/Reihitsu.Tooling`/`scripts` (tooling); the lane check admits that only with a change doc | Lane choice is internal | e436dfd |

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: spec author (01).
