Role: lead
Commit: c857b2d
Verdict: lane claimed — tooling

## Run

- Issue: thoenissen/Reihitsu#998 (parent thoenissen/Reihitsu#899). Draft PR: thoenissen/Reihitsu#1426.
- Slug: `requirement-coverage-opt-out`.
- Branch: `claude/clever-clarke-4ww2jt` — the session's designated development branch, used instead of `claude/issue-998-<slug>` because the session may push only to that branch.

## Lane

Tooling. The issue changes test infrastructure (`RequirementCoverageTests` in `Reihitsu.ArchitectureTests`, the attribute declarations in the test projects), the rulebook (`sdd/README.md`), and an agent instruction (`squad/roles/test-implementer.md`). No shipped source changes. Confirmed with `scripts/lane-check.sh tooling --head worktree` after each writing role.

Roles: spec author (runs although the tooling lane marks it optional: the issue requires a process change doc with `Affected requirements: none — process change` and rulebook text, and the lead writes neither), test implementer (the coverage check has tests), implementer, cleaner, spec checker, auditor, validator. Skeptic skipped: the tooling lane does not run it, and the maintainer decides the two open questions of the issue directly (escalation below).

## Maintainer's clarifications (summarized)

- All blockers are merged (every analyzer batch, thoenissen/Reihitsu#939 via #1358, thoenissen/Reihitsu#1401 via #1414); build on the rulebook as it is on `main`.
- The two decisions the issue leaves open (incidental tests; architecture and tooling tests) go to the maintainer before the test implementer starts.
- Do not touch `squad/roles/skeptic.md` or `squad/failure-classes.md` (another run edits them).
- No shipped code or behavior change; no requirement added to a spec to make a tag fit (a missing requirement becomes an `SDD open questions: <spec path>` issue); no requirement ID retired, moved, or renumbered; no `sdd/work/` in the PR; no other issues without approval of the exact text.

## Lead's survey (counts only, by a line scan of the test sources on c857b2d)

| Project | Test methods | Without `[Requirement]` |
|---|---|---|
| Reihitsu.Analyzer.Test | ~5566 | ~1069 |
| Reihitsu.Formatter.Test | ~2672 | ~1507 |
| Reihitsu.Core.Test | ~419 | ~414 |
| Reihitsu.Cli.Test | ~213 | ~66 |
| Reihitsu.Playground.Test | 17 | 0 |
| Reihitsu.ArchitectureTests | ~80 | ~79 |
| Reihitsu.Tooling.Test | 66 | 66 |
