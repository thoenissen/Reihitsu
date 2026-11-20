# cli-stack-overflow

| | |
|---|---|
| Date | 2026-11-20 |
| Run | issue #1887 |
| Pull request | #1893 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|

None asked. The issue's output question was already answered in the maintainer's triage comment (option A, per CLI-016).

## Cost

No stops and no repair rounds; every role was spawned once. The expensive spawns were the skeptic and the auditor (large tier, high effort), each of which built Release CLIs and ran nesting matrices over many shapes and depths, each taking several seconds to minutes per cell; the auditor additionally built `main` in a separate worktree for comparison.

## Usage

Session `1436681c-217a-506a-a0fc-4ce53deffff2`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 8 | 33 | 81 | 14.771 | 1.022 | 4.484.129 | 91.903 | 60.761 |
| skeptic | 1 | a6741fdf24867f26c | Skeptic for issue 1887 | large | high | 1 | 41 | 80 | 40.856 | 18.551 | 5.333.155 | 179.813 | 99.087 |
| test-implementer | 1 | a006a8e661c2e403b | Tests for issue 1887 | large | medium | 1 | 30 | 60 | 26.155 | 7.576 | 3.525.892 | 92.894 | 69.057 |
| implementer | 1 | afa2d00838aabe535 | Implement fix for issue 1887 | large | medium | 1 | 25 | 50 | 15.535 | 3.016 | 2.639.118 | 146.698 | 51.301 |
| cleaner | 1 | adcea5cb2d1b4de40 | Clean up for issue 1887 | medium | low | 1 | 6 | 12 | 1.480 | 0 | 414.584 | 85.596 | 9.210 |
| spec-checker | 1 | abc710f403685097d | Spec check for issue 1887 | medium | medium | 1 | 5 | 10 | 2.024 | 122 | 384.807 | 38.621 | 14.175 |
| auditor | 1 | a4f5d47dfdbf2256a | Audit for issue 1887 | large | high | 1 | 40 | 80 | 38.824 | 20.515 | 5.883.519 | 186.513 | 110.510 |
| validator | 1 | a487f1271dec54548 | Validate for issue 1887 | small | low | 1 | 6 | 12 | 2.652 | 176 | 107.929 | 59.622 | 8.248 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 169 | 351 | 136.141 | 50.680 | 21.865.813 | 697.821 |
| medium | 11 | 22 | 3.504 | 122 | 799.391 | 124.217 |
| small | 6 | 12 | 2.652 | 176 | 107.929 | 59.622 |
| **Total** | 186 | 385 | 142.297 | 50.978 | 22.773.133 | 881.660 |

## Decisions taken

11. The ones the maintainer may want to see: the run guarded all three recursions the skeptic found, not only the one the issue named (decision 1); the CLI end-to-end tests run the CLI out of process because a stack overflow cannot be caught, against the `AGENTS.md` convention that CLI end-to-end tests call `Program.Main()` (decisions 6, 10); Roslyn's own parser overflows were dropped without a follow-up (decision 3).

## Suggestions

- The `AGENTS.md` convention "CLI end-to-end tests call `Program.Main()` directly" has no exception for a test whose red state terminates the process; this run's tests depart from it with a reason. A one-clause exception would keep later reviews from flagging it again.
- Matrix runs of the skeptic and the auditor overlapped heavily (both re-ran nesting shapes at 2000–32000); for a stack-depth bug, the auditor could reuse the implementer's re-run matrix and only add the cells it extends.
