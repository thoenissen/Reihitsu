# large-agent-threshold

| | |
|---|---|
| Date | 2026-11-16 |
| Run | issue #1792 |
| Pull request | #1799 |

## Questions to the maintainer

None. The run changes only the squad process and a squad script, so no output question or escalation arose.

## Cost

- No stops. One repair round (auditor: the response filter was copied a third time into `usage.sh`; the sign of negative growth was taken in with it).
- Lane claimed as tooling, widened to behavior change when the lane check saw the change doc alone in the diff (before any tooling path existed); with the tooling paths the diff needs only tooling. The widening added the skeptic, which changed the threshold from a single 120k to a cache-aware 50k cold / 150k warm rule, so it paid for itself.
- Spec author spawned twice: the original doc, then a fresh agent for the skeptic's amendment (the first agent had already been continued once for a status change).
- Test implementer skipped: `scripts/usage.sh` has no test harness; the implementer and auditor checked it against this session's transcripts and constructed transcripts.
- The skeptic (high effort) was the most expensive role at about 4.6 M cache read; its break-even model is what changed the signal.

## Usage

Session `ac22de41-b2e9-58d2-88fe-1db1aec6921b`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 8 | 44 | 105 | 18.205 | 2.995 | 6.284.786 | 106.776 | 75.040 |
| spec-author | 1 | a91f2076248c84481 | Spec author for #1792 | large | medium | 2 | 25 | 52 | 22.383 | 9.441 | 2.673.171 | 75.949 | 52.495 |
| skeptic | 1 | acb27825dcd397be6 | Skeptic for #1792 | large | high | 1 | 30 | 61 | 38.353 | 22.185 | 4.602.625 | 155.267 | 131.865 |
| spec-author | 2 | aa6ac2701d94b860a | Spec author amendment #1792 | large | medium | 1 | 20 | 40 | 24.110 | 9.717 | 2.564.427 | 126.624 | 102.494 |
| implementer | 1 | aaeab15c92f6c055d | Implementer for #1792 | large | medium | 1 | 11 | 21 | 8.010 | 497 | 1.073.846 | 57.886 | 33.800 |
| cleaner | 1 | a5b81b701a58ae5a4 | Cleaner for #1792 | medium | low | 1 | 5 | 10 | 1.145 | 14 | 338.032 | 86.866 | 7.307 |
| spec-checker | 1 | ab37dda9ba7c4de1f | Spec checker for #1792 | medium | medium | 1 | 7 | 14 | 3.398 | 775 | 657.084 | 56.062 | 32.825 |
| auditor | 1 | ae6d07d20c15286d5 | Auditor for #1792 | large | high | 1 | 16 | 33 | 17.349 | 9.943 | 1.898.385 | 95.625 | 72.371 |
| implementer | 2 | a169d628febe8e1b3 | Implementer repair #1792 | large | medium | 1 | 10 | 20 | 8.099 | 1.418 | 910.410 | 50.685 | 26.574 |
| auditor | 2 | ae9dd13509519ec95 | Auditor repair #1792 | large | high | 2 | 15 | 30 | 10.225 | 2.891 | 1.453.221 | 120.280 | 45.419 |
| spec-checker | 2 | a21bac14e218bc912 | Spec checker repair #1792 | medium | medium | 1 | 5 | 10 | 2.102 | 63 | 339.410 | 91.612 | 16.790 |
| validator | 1 | a3b1ec15b9c4cbefd | Validator for #1792 | small | low | 1 | 9 | 18 | 2.778 | 136 | 206.874 | 58.891 | 12.338 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 171 | 362 | 146.734 | 59.087 | 21.460.871 | 789.092 |
| medium | 17 | 34 | 6.645 | 852 | 1.334.526 | 234.540 |
| small | 9 | 18 | 2.778 | 136 | 206.874 | 58.891 |
| **Total** | 197 | 414 | 156.157 | 60.075 | 23.002.271 | 1.082.523 |

## Decisions taken

12, none the maintainer needs to see. Worth a look: the chosen rule (decision 4) departs from the issue's suggestion "cache read of the latest response", because with 5-minute subagent caches that figure calls an agent small exactly when continuing it is most expensive.

## Suggestions

- The lane check reports behavior change for a diff whose only non-text path is a process change doc, and tooling once tooling paths join it. A process run that writes its change doc first is pushed through a widening it does not need; the lane check could classify a process change doc (`Affected requirements: none — process change`) as text.
