# Phase 6 inventory — instructions for slice agents

Goal (maintainer 2026-11-16): group and prioritize the open vNext, Maintenance, 1.4.0 and 1.5.0 issues; find issues to close; later plan 1.4.0 by themes. You do slice N of `phase6-index.md` (column `Slice`). Read-only on GitHub and on the repository code — you only write your own file `sdd/work/coordinator/inventory/phase6-slice-N.md`. Do not commit, do not push, do not write to GitHub.

## Reading

- GitHub via curl on the public REST API (the proxy authenticates; the rate limit is ample): `https://api.github.com/repos/thoenissen/Reihitsu/issues/<n>` and `/issues/<n>/comments`. Read body, labels and every comment (decisions live in comments: "Triage", maintainer decisions, squad notes).
- Code and specs: the working tree is on branch `claude/sdd-coordinator`, which has `origin/main` merged (2026-11-16). Treat it as main. Specs under `sdd/specs/`, change docs under `sdd/changes/`, rule pages under `documentation/rules/`, code under `src/`. Use `git log --oneline -S'<text>' origin/main` or `git log --grep` to find whether something was implemented.
- Repository rules that decide verdicts: `AGENTS.md` sections "Code-fix coverage" (incl. "Special cases do not grow the source"), "Supported inputs" (a case no caller reaches is not a defect), "Issue intake"; `sdd/README.md` section P11 (the formatter may do more than the analyzers).
- Known decisions: #1759 = general reversal of SHARED-DIAG-008 ("analyzers judge the code as the current build's preprocessor symbols are set"); #1732, #1726, #1728 declare shapes unsupported; #1739 new comma rule (follow-up of #956); #1140 remainder + #1774 need a partial write-back design; declared unsupported shapes so far: FMT-UNSUP-004 (comment inside a variable declaration), -008 (comment/directive between `?`/`:` and branch), -009 (comment/directive between list element and comma on a later line) — see `sdd/specs/formatter/unsupported-syntax.md`.

Be economical: open the code only as far as the verdict needs it. Do not reproduce by running tests unless a verdict hinges on it and a quick check settles it (builds are slow; avoid them).

## Output: one row per issue

`sdd/work/coordinator/inventory/phase6-slice-N.md`:

```
# Phase 6 slice N (issues #a–#b)

| # | Milestone | Area | Theme candidate | Verdict | Evidence | Size | User-visible | Depends on | Output question | Note |
|---|---|---|---|---|---|---|---|---|---|---|
```

- **Area**: the code area, concrete: e.g. `formatter/LineBreakPhase`, `formatter/BlankLinePhase`, `analyzer/RH5xxx layout`, `analyzer/RH8xxx documentation`, `code fixes (shared)`, `CLI`, `squad/tooling`, `specs only`, `tests/architecture`.
- **Theme candidate**: a short free-text cluster name, reused consistently within your slice. Examples: `partial write-back`, `SHARED-DIAG-008 reversal`, `unsupported shapes`, `new rule`, `rule report scope`, `documentation-comment rules`, `formatter layout feature`, `region handling`, `using ordering`, `squad/tooling`, `internal refactor`, `test infrastructure`, `spec clarification`, `CLI feature`, `C# language support`.
- **Verdict** (exactly one): `open` / `done` (already implemented on main) / `superseded by #N` / `duplicate of #N` / `contradicts decision X` (name the decision or rule, e.g. "Supported inputs", "Code-fix coverage", P11, an issue decision) / `unreachable` (no caller reaches the case — name why) / `partly done` (say what remains).
- **Evidence**: for any verdict other than `open`, the proof in a few words: commit/PR, spec requirement ID, file:line, or the decision. For `open`, `—`.
- **Size**: S (one rule/one phase, small), M, L (cross-cutting, design needed).
- **User-visible**: yes/no (does an analyzer/code-fix/formatter/CLI user see a difference).
- **Depends on**: issue numbers that must land first, or `—`.
- **Output question**: yes if implementing it requires the maintainer to pick an output a user sees; no otherwise.
- **Note**: at most one short sentence (e.g. overlap with another issue, a stale premise).

After the table, add a section `## Closure candidates` listing only rows whose verdict is not `open`, one line each with the reason, and a section `## Overlaps` with pairs/groups of issues (also across slices if you notice) that should run together.

Keep the file compact. Final reply to the coordinator: 3 lines — counts per verdict, the theme candidates you used, the path of your file.
