# shared-type-declaration-kinds

| | |
|---|---|
| Date | 2026-11-17 |
| Run | issue #1788 |
| Pull request | #1841 |

## Questions to the maintainer

None.

## Cost

No stops, no repair rounds, every role spawned once. The skeptic was the most expensive spawn: it read every type-kind list across the analyzers, Core, and the formatter and reproduced the issue's RH5108/RH5110 examples, which showed that neither option the issue proposed was worth building. Implementer and spec author were skipped: the diff is one test-only canary.

## Usage

Session `0952e34d-6c59-5f93-a658-e21352467e0c`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 6 | 31 | 77 | 10.244 | 1.426 | 3.966.122 | 84.527 | 54.124 |
| skeptic | 1 | a1bd196cfce2ec624 | Skeptic premise check #1788 | large | high | 1 | 35 | 70 | 27.886 | 11.222 | 4.273.742 | 105.276 | 81.736 |
| test-implementer | 1 | a3e1c2ad1ccaf957e | Test implementer canary #1788 | large | medium | 1 | 14 | 27 | 13.438 | 4.807 | 1.369.773 | 60.446 | 36.180 |
| cleaner | 1 | a7bbe2cb55a343a56 | Cleaner #1788 | medium | low | 1 | 4 | 8 | 1.039 | 0 | 245.006 | 84.271 | 6.309 |
| spec-checker | 1 | a530cdd005305d69e | Spec checker #1788 | medium | medium | 1 | 5 | 10 | 2.102 | 218 | 337.631 | 90.784 | 13.314 |
| auditor | 1 | a995235cf74e632a6 | Auditor #1788 | large | high | 1 | 17 | 35 | 17.889 | 8.635 | 1.837.929 | 79.258 | 55.102 |
| validator | 1 | a454ec84a826b7192 | Validator #1788 | small | low | 2 | 10 | 20 | 3.186 | 213 | 220.694 | 52.556 | 8.865 |
| issue-manager | 1 | a4e10691c30d55c0a | Issue manager #1788 follow-ups | medium | medium | 1 | 8 | 18 | 6.409 | 329 | 671.018 | 111.113 | 36.970 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 97 | 209 | 69.457 | 26.090 | 11.447.566 | 329.507 |
| medium | 17 | 36 | 9.550 | 547 | 1.253.655 | 286.168 |
| small | 10 | 20 | 3.186 | 213 | 220.694 | 52.556 |
| **Total** | 124 | 265 | 82.193 | 26.850 | 12.921.915 | 668.231 |

## Decisions taken

Four. The one the maintainer may want to see: the issue's two options (one shared kind set; an architecture test with an exclusion table) were both dropped. The skeptic showed there is no single set — the registrations use at least six different kind sets, each matching its rule's spec — and an exclusion table would repeat every spec's list a third time. The run ships a canary test that pins Roslyn's type-declaration syntax types and kinds instead, and links the issue with `Closes`. The issue also claimed lane tooling; the paths it names are production and tests, so the run claimed bug fix.

## Suggestions

An `agent-filed` maintenance issue whose "Lane:" line names a lane the lane-check table contradicts could be caught at triage; here it cost nothing because the skeptic narrowed the change anyway.
