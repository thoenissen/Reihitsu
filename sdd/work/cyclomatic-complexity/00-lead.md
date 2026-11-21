# Lead — cyclomatic-complexity

- Issue: thoenissen/Reihitsu#352 — `agent-filed` (goals are proposals; the maintainer's clarifications below override the issue text)
- Slug: `cyclomatic-complexity`
- Branch: `claude/affectionate-allen-njaq9m` (session-designated); draft PR thoenissen/Reihitsu#1909
- Lane: **behavior change / new rule** — the issue requests a new analyzer rule (new diagnostic, new rule page, new configuration settings).
- Roles: spec author → skeptic → (concept presented to the maintainer, see below) → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.

## Maintainer clarifications (given with the run's prompt, summarized)

1. Present a concept of how the rule works to the maintainer **before anything is implemented** (no tests, no production code before the maintainer approves the concept). The lead presents it at the batching point before the test implementer.
2. The rule computes **cyclomatic complexity** (not cognitive complexity as the issue proposes). Default threshold **15**. The value is a starting point; it will be tried out when the rule is rolled out in this repository.
3. If the rule reports violations in the repository's own code (self-hosting), these are ignored for now; adjustments, and possibly a different default, come in a follow-up.
4. The threshold must be configurable through `reihitsu.json`, like the line-count rules (RH2008 `design.maximumMethodLines`, RH2111 `design.maximumTypeLines`).
5. The rule must not check complexity alone: a method that never reaches nesting level 2 — i.e. has no nesting — is never reported. Example: a method with only `if` statements one after another, none nested inside another, is never reported, whatever its complexity.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

- Self-hosting violations of the new rule and a review of the default threshold (maintainer clarification 3) — lead.

## Open worklist / next step

- Next: spawn spec author.
