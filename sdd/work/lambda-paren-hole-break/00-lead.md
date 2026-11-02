Role: lead
Commit: ae55f3e
Verdict: run started

# Run

- Issue: thoenissen/Reihitsu#1494 — `agent-filed` (goals are proposals). Package C8 of thoenissen/Reihitsu#1482; its blocker thoenissen/Reihitsu#1488 is closed (merged in #1513).
- Slug: `lambda-paren-hole-break`
- Branch: `claude/ecstatic-shannon-j9cmkw` (session-designated branch; see Decisions taken #1)
- Draft PR: thoenissen/Reihitsu#1565
- Lane: **behavior change** — the issue asks for changed formatter behavior (a hole that stays on one line, a collapse that no longer touches parenthesized lambdas) and requires a change doc under `sdd/changes/`.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing when follow-up candidates exist. None skipped.
- The issue was written against `main` at `735d0dba`; every statement about today's code must be checked against current `main` first (issue section "Before the run"). Statements already made true or moot are noted and dropped.

## Maintainer answers

Pending: output questions 1 (anonymous method in a raw-string hole: one line vs. broken and aligned), 2 (blank line before a parenthesized lambda that starts its line, five gaps), 3 (lambda starting an unplaced hole line keeps its column) — asked before the test implementer.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 2 | Skeptic | Phase-stability check test loses its lambda example | Replace with an injected (synthetic) phase, not another formatter defect | The check's own test must not depend on a defect surviving | Test-only | — |
| 1 | Claim | Branch name `claude/issue-1494-<slug>` per `squad/lead.md` vs. the session's designated branch | Use `claude/ecstatic-shannon-j9cmkw` | The session environment pins development and pushes to that branch | Branch names are internal | ae55f3e |

## Follow-up candidates

1. Anonymous method in a hole of a regular interpolated string below C# 11 produces CS8967 (safety rail; caller path unverified) — 01-spec-author.md, 02-skeptic.md.
2. Parenthesized lambda behind a modifier or return type on the next line (`static⏎(int x) => x`) lands in the statement's column — 01-spec-author.md, 02-skeptic.md.
3. Value continued on the line after `return` or a lambda's `=>` starts in the statement's column — 02-skeptic.md (file only if no FMT-INDENT requirement decides it).
