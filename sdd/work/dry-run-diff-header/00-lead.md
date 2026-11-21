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

Batch 1 (before test implementer), answered "as recommended":
- Q1 Windows, file outside the current directory (run in `C:\work`, `--dry-run D:\other\B.cs`): **A** — header `--- D:\other\B.cs` / `+++ D:\other\B.cs`, same path as the `Would format:` line (backslashes kept, no prefix).
- Q2 full path through a symbolic link (cwd physically `/data/proj`, given `/home/me/proj/src/A.cs`): **A** — paths compared as written, links not resolved; the file counts as outside: `--- /home/me/proj/src/A.cs`. A relative path `src/A.cs` still gives `a/src/A.cs`.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | Define "current directory" in CLI-024 | The directory the CLI process reports, not the shell's `$PWD` | Matches what the CLI already uses for full paths | Spec wording only; the output is what the process sees today | c45a33a |
| 2 | skeptic | Case rule for "inside" | State it as CLI-006 compares paths (case-insensitive on Windows, case-sensitive elsewhere), one comparer | One statement of the rule, no reliance on `Path.GetRelativePath` platform behaviour | Same outputs on Linux/Windows as the spec draft | c45a33a |
| 3 | skeptic | `Why:` of CLI-024 | Claim only `patch -p1` and `git apply` at the top of a work tree | Only these were verified | Spec rationale text | c45a33a |
| 4 | skeptic | Lane / issue goals "Squad lane: bug fix", "remove open-question non-goal" | Dropped as outdated (agent-filed); lane behavior-change; the non-goal no longer exists on main | Spec pins today's header; nothing to remove | Internal | - |
| 5 | implementer | Keep new internal type `PathComparison` | Keep | One owner of the CLI's case rule for CLI-006 and CLI-024 instead of a second copy; part of the issue's own fix, small, so not escalation 9 | Internal type; CLI-006 behavior unchanged | dea4dbd |
| 6 | implementer | Keep `DiffGenerator.GenerateWithNames` | Keep | Lets the dry-run header carry its own names while `Generate` and the trace header stay unchanged | Internal API | dea4dbd |

## Follow-up candidates

- `scripts/trace.sh` with a full path prints `--- a//…` for the same reason (developer tooling, low value) — 02-skeptic.md

## State

- Last step finished: spec author (8d5f2cc, pushed): change doc Draft, CLI-013 + new CLI-024. Open output question: Windows full path outside cwd, `\` (A, recommended) vs `/` (B). Visible changes 1-3 covered by #933 Q1 answer.
- Skeptic done (813177e): verdict output question; premise confirmed (patch -p1 / git apply accept the new header).
- Change doc Accepted (c45a33a). Test implementer done (557151c): 14 E2E tests, 13 red on the header lines (reproduced), 1 Windows-only skipped. Implementer done (dea4dbd, abdf403): 13 pass, 1 Windows-only skipped; CLI 438 pass. Cleaner clean (fc3905c). Next: spec checker.
