# Lead — spacing-sole-gap-owner

## Run

| Issue | `agent-filed` | Goals |
|---|---|---|
| thoenissen/Reihitsu#1493 — FMT-SPACE: the spacing phase is the only owner of same-line gaps | yes | proposals; the issue's own "Before the run" calls its target behavior and "Done when" binding, but the label makes them proposals under `squad/README.md`, *Issues are hypotheses*; any change to visible output is an output question either way |

- Slug: `spacing-sole-gap-owner`
- Branch: `claude/focused-brown-4e3k0u` (session-designated branch; see decision 1)
- Draft PR: thoenissen/Reihitsu#1548
- Base: `origin/main` at `e3842c8`
- Blocker thoenissen/Reihitsu#1488 (B3) is closed (merged by #1513).

## Lane

**Behavior change.** The issue requests changed formatter behavior (attribute-target colon and arrow spacing, joined-pair separators) and requires a change doc under `sdd/changes/`.

Roles run: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager once before publishing if follow-up candidates exist. Skipped: none.

## Maintainer clarifications for this run

- The maintainer expects the issue to be complex and asks the run to plan carefully up front, so that repair rounds do not keep finding new cases at the end. Every role therefore enumerates complete inventories and failure classes in its first pass (every rewriter of the line-break phase and every token pair it can join or space; every attribute-target and arrow construct; existing formatter, code-fix, and analyzer tests whose expected output depends on the changed gaps), rather than sampling.
- Context from earlier runs (`squad/notes/2026-11-01-blank-line-sole-owner.md`, `squad/notes/2026-11-01-using-comment-blank-line.md`): the maintainer considers comments that share a line with code, and comments around `=`, unwanted; the formatter must not grow complexity to lay them out. Reachability (`AGENTS.md`, *Supported inputs*) is checked before any case is measured.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name: playbook's `claude/issue-1493-<slug>` or the session-designated branch | Use `claude/focused-brown-4e3k0u` | The session environment permits pushing only to that branch | Branch names are internal | claim commit |

## Follow-up candidates

(none yet)
