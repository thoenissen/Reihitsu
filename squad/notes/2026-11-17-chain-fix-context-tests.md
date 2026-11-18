# chain-fix-context-tests

| | |
|---|---|
| Date | 2026-11-17 |
| Run | issues #889, #1476 |
| Pull request | #1857 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|

None asked.

## Cost

No stops, no repair rounds. The cleaner was continued once to withdraw a doc-comment repair after the lane check reported that the diff needs the bug-fix lane. The skeptic was the most expensive spawn: it ran mutation checks against the context-selection clause in a throwaway worktree.

## Usage

Session `de6510fd-7895-5bd9-ae61-2c39d2d0e724`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 4 | 31 | 72 | 12.334 | 1.450 | 4.020.152 | 87.520 | 56.505 |
| skeptic | 1 | a813d8c5a318302f0 | Skeptic: chain fix context tests | large | high | 1 | 36 | 71 | 33.136 | 13.916 | 4.663.154 | 117.160 | 93.629 |
| test-implementer | 1 | aaec68c6ee75cf1e3 | Test implementer: chain fix tests | large | medium | 1 | 21 | 40 | 28.957 | 6.683 | 2.383.371 | 94.958 | 70.681 |
| cleaner | 1 | aef081f33f4e23672 | Cleaner: chain fix tests | medium | low | 1 | 8 | 17 | 4.290 | 51 | 666.596 | 101.186 | 23.008 |
| spec-checker | 1 | a467e88f1b09179f7 | Spec checker: chain fix tests | medium | medium | 1 | 6 | 12 | 3.000 | 221 | 432.699 | 93.702 | 16.119 |
| validator | 1 | a78f78a7bdbd262c6 | Validator: chain fix tests | small | low | 1 | 5 | 10 | 2.266 | 156 | 76.570 | 56.161 | 6.782 |
| issue-manager | 1 | a83423072cb640129 | Issue manager: follow-ups | medium | medium | 1 | 10 | 21 | 4.950 | 317 | 933.418 | 56.058 | 31.909 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 88 | 183 | 74.427 | 22.049 | 11.066.677 | 299.638 |
| medium | 24 | 50 | 12.240 | 589 | 2.032.713 | 250.946 |
| small | 5 | 10 | 2.266 | 156 | 76.570 | 56.161 |
| **Total** | 117 | 243 | 88.933 | 22.794 | 13.175.960 | 606.745 |

## Decisions taken

Five. Worth a look: #889's goal "a layout test fails without the clause" was replaced by refusal-scope tests (SHARED-FIX-010), because since #1760 the context choice decides no column; and the stale doc comment was withdrawn from the run to keep the tests lane and filed as #1861.

## Suggestions

- A comment-only fix of a production file cannot ride along in a tests-lane run: the text-only proof has to cover the whole diff, and the run's own tests are compiled. The lead's adjacent-fix decision cost a cleaner round trip. Either the lane check could classify each C# file by its own proof, or `README.md`, *Scope*, could say that an adjacent comment fix in a production file widens a tests run to bug fix.
