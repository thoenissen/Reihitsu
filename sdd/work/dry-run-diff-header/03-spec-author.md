Role: spec author
Commit: c83ed97
Verdict: change doc written

Inputs: `00-lead.md` (Maintainer answers 1A and 2A; Decisions taken rows 1-3), `01-spec-author.md`, `02-skeptic.md`, `sdd/changes/2026-11/dry-run-diff-header.md`, CLI-006, CLI-013, CLI-024. The prompt carried no intended fix, cause, or hoped-for verdict beyond the recorded answers.

Files written:

- `sdd/changes/2026-11/dry-run-diff-header.md` — `Status: Accepted`. Visible change 4 now states the answer (`\` kept, also for a file on the same drive outside the current directory); new visible change 5 (full path through a symbolic link: `--- /home/me/proj/src/A.cs`, relative path still `a/src/A.cs`); change 1 no longer claims the header never depends on how the path was given. `Decisions` gains "A full path outside the current directory keeps the platform's separator" (1A) and "Paths are compared as written" (2A); "Inside means below the current directory" now names the process-reported directory and the explicit case rule. `Why` narrowed to `patch -p1` and `git apply` at the top of a work tree. Two rejected alternatives added (`/` in a full path; resolving links).
- `sdd/specs/cli/reihitsu-format.md`, CLI-024:
  - decision 1: the current directory is the working directory the CLI process reports (physical on Linux and macOS, not the shell's `$PWD`);
  - decision 2: "case-insensitively on Windows and case-sensitively on every other platform, as CLI-006 compares paths";
  - decision 3: `Why:` claims only `patch -p1` and `git apply` run at the top of a work tree;
  - answer 1A: the outside bullet says the full path exactly as `Would format:` prints it, with `\` on Windows, plus a Windows outside example (`D:\other\B.cs`);
  - answer 2A: paths compared as written, links not resolved, a full path through a link into the current directory is outside; the sentence "the header depends on where the file is, not on how its path was given" is replaced by "the same whether the file is given by its full path, by a path relative to the current directory, or through a directory"; new symlink example.

No requirement ID, `Uses` row, or code owner added or removed. `scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered` (`CLI-024: no test covers it`), expected until the test implementer covers it.

## Changed requirements

- CLI-024 — The `--dry-run` diff header names the file relative to the current directory — changed (current directory defined, case rule explicit, Windows outside separator, links not resolved, `Why:` narrowed).

## Examples for the tests

- CLI-024 — rows 1-7 of the skeptic's matrix (Linux, end to end through `Program.Main()`), plus the symlink example (full path through a link → full-path header; relative path → `a/src/A.cs`), which is runnable on Linux by creating a link in the temp directory and setting the current directory to its target. Windows examples (inside with `/`, `C:\Work` vs `C:\work`, `D:\other\B.cs` with `\`) are Windows-only. Mind the skeptic's hazards: build paths from `Directory.GetCurrentDirectory()` after setting it (macOS `/var` link).
- CLI-013 — unchanged since `01`.

## Output questions

_None._ Both questions answered (1A, 2A).

## Internal choices

- Decisions 1-3 of `00-lead.md` written as recorded; no further choices.
- Answer 2A is written as a consequence stated in CLI-024 ("a full path that reaches the file through a link names it outside") rather than only as "links are not resolved", so the test implementer has an explicit condition with an example on each side (FC-002).

## Follow-up candidates

_None_ beyond the one `00-lead.md` already lists (`scripts/trace.sh` header).
