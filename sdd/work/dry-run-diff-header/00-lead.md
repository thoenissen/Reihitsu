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
| 1 | skeptic | Define "current directory" in CLI-024 | The directory the CLI process reports, not the shell's `$PWD` | Matches what the CLI already uses for full paths | Spec wording only; the output is what the process sees today | pending (spec author) |
| 2 | skeptic | Case rule for "inside" | State it as CLI-006 compares paths (case-insensitive on Windows, case-sensitive elsewhere), one comparer | One statement of the rule, no reliance on `Path.GetRelativePath` platform behaviour | Same outputs on Linux/Windows as the spec draft | pending (spec author) |
| 3 | skeptic | `Why:` of CLI-024 | Claim only `patch -p1` and `git apply` at the top of a work tree | Only these were verified | Spec rationale text | pending (spec author) |
| 4 | skeptic | Lane / issue goals "Squad lane: bug fix", "remove open-question non-goal" | Dropped as outdated (agent-filed); lane behavior-change; the non-goal no longer exists on main | Spec pins today's header; nothing to remove | Internal | - |

## Follow-up candidates

- `scripts/trace.sh` with a full path prints `--- a//…` for the same reason (developer tooling, low value) — 02-skeptic.md

## State

- Last step finished: spec author (8d5f2cc, pushed): change doc Draft, CLI-013 + new CLI-024. Open output question: Windows full path outside cwd, `\` (A, recommended) vs `/` (B). Visible changes 1-3 covered by #933 Q1 answer.
- Skeptic done (813177e): verdict output question; premise confirmed (patch -p1 / git apply accept the new header).
- Next: output-question batch 1 (Q1 Windows separator outside cwd, Q2 symlinked path) asked to maintainer; WAITING. Then route answers + decisions 1-3 to spec author (fresh), accept change doc, then test implementer.
