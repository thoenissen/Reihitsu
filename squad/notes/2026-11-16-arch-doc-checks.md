# arch-doc-checks

| | |
|---|---|
| Date | 2026-11-16 |
| Run | issues #1507, #1428 |
| Pull request | #1796 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|

None; neither issue put a visible output in question.

## Cost

No stops, no repair rounds; every role spawned once (tests lane: spec author, skeptic, test implementer, cleaner, spec checker, validator, then the issue manager). The test implementer was the most expensive spawn (mutation runs on the real `AGENTS.md` table and fixture projects); the validator's full test run dominated wall-clock time.

## Usage

Session `a47a7872-b61d-578d-a230-aec2bd92c521`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 8 | 32 | 75 | 14.802 | 1.768 | 4.233.210 | 90.883 |
| spec-author | 1 | a1ce1f7de4a9530bf | Spec author for #1507 #1428 | large | medium | 1 | 21 | 40 | 14.543 | 3.966 | 2.321.711 | 79.147 |
| skeptic | 1 | a932e1493479fdc29 | Skeptic for #1507 #1428 | large | high | 1 | 27 | 56 | 24.164 | 10.132 | 2.852.312 | 140.156 |
| test-implementer | 1 | aca8f70f6054bc07d | Test implementer for #1507 #1428 | large | medium | 1 | 23 | 48 | 43.609 | 6.742 | 2.894.898 | 172.808 |
| cleaner | 1 | a4cd71bf46da450af | Cleaner for #1507 #1428 | medium | low | 1 | 6 | 12 | 2.695 | 51 | 424.747 | 89.581 |
| spec-checker | 1 | a595337fc731001e4 | Spec checker for #1507 #1428 | medium | medium | 1 | 5 | 10 | 2.959 | 162 | 345.737 | 97.909 |
| validator | 1 | a382b0697fd87c908 | Validator for #1507 #1428 | small | low | 1 | 7 | 14 | 2.627 | 314 | 131.249 | 49.919 |
| issue-manager | 1 | a69c7c274728444a2 | Issue manager for follow-ups | medium | medium | 1 | 7 | 16 | 3.257 | 568 | 586.065 | 47.130 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 103 | 219 | 97.118 | 22.608 | 12.302.131 | 482.994 |
| medium | 18 | 38 | 8.911 | 781 | 1.356.549 | 234.620 |
| small | 7 | 14 | 2.627 | 314 | 131.249 | 49.919 |
| **Total** | 128 | 271 | 108.656 | 23.703 | 13.789.929 | 767.533 |

## Decisions taken

Eight. The one the maintainer may want to see: the guard-name check of #1507 resolves names only in `Reihitsu.Formatter` and the projects it references (plus Roslyn `SyntaxKind` members), not under all of `src/` as the issue wrote, because the stale `CanRewrite` the issue cites is still declared in an unrelated code fix and would otherwise pass. Also taken in as an adjacent fix: the exempted-spec report of #1428 is built from the exemption keys.

## Suggestions

_None._
