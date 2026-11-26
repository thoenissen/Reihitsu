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
| 2 | Spec author | #1924 header: follow the `--dry-run` rule (CLI-024) or keep `a/<path>` | Follow CLI-024, sharing its code | One statement of the rule; trace is a repository tool, not `reihitsu-format` (`sdd/specs/cli/reihitsu-format.md`) | The trace is repository tooling, not a visible surface | d686d38 |
| 3 | Spec author | #1949 apply-fix `preview` handling, repeated option, printing the version | Lower `preview` like `reihitsu-format`; repeated option is an error; version not printed | Matches CLI-004 and existing `--passes`/`--max-iterations` handling | Repository tooling only | d686d38 |
| 4 | Spec author | Change docs list no visible output change | Accept once the skeptic has run | Tooling surfaces only | No visible surface touched | d686d38 |

## Follow-up candidates

(none yet)

## Worklist / next step

- Done: spec author (01, d686d38, pushed). LANE CHECK after 01: confirmed (diff needs only tooling).
- Next: skeptic (02), running.
