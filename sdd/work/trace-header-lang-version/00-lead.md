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
| 5 | Skeptic | Where the fixture runner gets the version clamp | `InternalsVisibleTo("Reihitsu.Tooling")` in `src/Reihitsu.Formatter/Properties/AssemblyInfo.cs`, reuse `LanguageVersionResolver` | A copy would be FC-015; making it public widens the shipped assembly | Internal visibility only | |
| 6 | Skeptic | Tools' default version | Always parse with the resolved version (`MaxSupportedLanguageVersion` when no option); amend change doc wording | Keeps tools aligned with `format.sh` after a Roslyn upgrade; same as today on Roslyn 5.12.0 | Tooling; identical output today | |
| 7 | Skeptic | #1924 goals: "trace spec" branch and the claim about the existing test | Dropped | No trace spec exists; the test passes a full path | Tooling | |
| 8 | Test implementer | If `FixtureRunner.HasSyntaxErrors` gains a version parameter, may `FixtureRunnerTests` call sites be updated? | Yes: the implementer may update the call (arguments only, no assertion) and names it in its handover | Signature change of an internal tooling method; assertions unchanged | Tooling test only | |
| 9 | Implementer | `FixtureRunner.RunAsync` call sites in `FixtureRunnerTests` (11) also needed the new version argument | Ratified: `LanguageVersion.Latest` added to the calls, no assertion or input changed; no test-only overload | Same behavior as before; avoids dead production code | Tooling tests only | 21e6421 |
| 10 | Implementer | Preview-warning and invalid-value hint text repeated in `Program`, trace, apply-fix | Not a follow-up: the change doc sets per-tool wording; sharing would add a member to the shipped formatter assembly for message text | Intentional per-tool messages | Tooling messages only | 21e6421 |
| 11 | Spec checker | Optional: `AGENTS.md` trace synopsis lacks `--lang-version` | Leave as is | Summary line, not wrong; `scripts/README.md` is the full statement of the options | Repository instructions only | |
| 12 | Repair 1 | Follow-up: `DiscoverProjects` skips only on direct references to uncompilable projects | Dropped | No project in the repository reaches it | Test harness only | |

## Follow-up candidates

(none yet)

## Worklist / next step

- Done: spec author (01, d686d38, pushed). LANE CHECK after 01: confirmed (diff needs only tooling).
- Done: skeptic (02, 30ac09d): #1924 proceed; #1949 proceed; no output questions.
- Done: spec author amendment (03, 11bad04); both change docs Accepted. No output questions, no stop.
- Done: test implementer (04, 4b9a8c9): tests written for both; fail for stated reasons.
- Done: implementer (05, 21e6421/8495f09): all tests pass.
- Done: cleaner (06, 7277ac1): clean.
- Done: spec checker (07, d74ec74): consistent.
- Done: auditor (08, 12736e9): pass, no findings. Audited SHA: ec68841 (handover at 12736e9).
- Done: validator (09, d659c32): FAIL. Self-hosting line-count rows (RH2008, RH2111): the self-hosting compilation of `Reihitsu.Tooling` cannot resolve `Reihitsu.Formatter` / `LanguageVersionResolver` (CS0234, CS0103 in `CodeFixRunCommand.cs`). Default run green.
- Repair round 1 (PR-introduced): worklist = the self-hosting failure, routed to the test implementer (owner of the self-hosting test harness) as 10. Then cleaner, spec checker, auditor (repair only), validator.
- Done: test implementer repair (10, 4bb901b): harness now adds transitive project references; line-count rows 18/18.
- Done: cleaner (11, b0843f8): clean.
- Next: spec checker (12).
