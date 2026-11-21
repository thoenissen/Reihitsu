# Lead — root-config-to-analyzers

- Issue: thoenissen/Reihitsu#1880 — `agent-filed` (goals are proposals). Labels: maintenance, agent-filed. Milestone 1.6.0.
- Slug: `root-config-to-analyzers`
- Branch: `claude/charming-goodall-3ko6fb` (session-designated). Draft PR: #1921.
- Lane: **tooling** — the change is to build files (`Directory.Build.props` / build imports) and a tooling/self-hosting test asserting the build configuration; the lane-check table classifies `*.props`, `*.targets`, the root `reihitsu.json` as tooling.
- Roles run: test implementer (the build-configuration test exists and is inverted), implementer, cleaner, spec checker, auditor, validator. Skipped: spec author and skeptic (the tooling lane does not run them).
- Maintainer clarifications: triage comment on the issue (2026-11-20) placed it in 1.6.0, to start after the 1.5.0 version bump rebuilds `binaries/`. `origin/main` at claim time is `5af53bd` "Bump version to 1.5.0", which rebuilt `binaries/` — the issue's blocker is lifted on paper; whether the rebuilt RH0001 accepts the `design` section is for the run to establish.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## State

- Last step finished: claim, lane claimed, prepare.sh green.
- Next: test implementer.
