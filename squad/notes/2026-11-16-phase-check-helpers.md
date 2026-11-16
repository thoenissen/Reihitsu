# phase-check-helpers

| | |
|---|---|
| Date | 2026-11-16 |
| Run | issues #1505, #1577, #1576 |
| Pull request | #1794 |

## Questions to the maintainer

_None_ — every question was internal.

## Cost

No stops. One repair round (five audit findings, all in the run's own test code, one medium on a test that did not pin what its summary claimed). Test implementer, cleaner, spec checker, and auditor each spawned twice because of that round. The lane widened from tests to tooling after the skeptic showed that #1576's test lives in `Reihitsu.SelfHosting.Test`, which needed a project reference to reach the helper; this added the implementer and the auditor. The full self-hosting run takes about 24 minutes locally; the run executed only the new self-hosting test (about 2.5 minutes) at the validator stage, the rest is left to CI.

## Usage

Session `10d5a5b4-e12e-5eea-897d-d07c9ed4a993`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 13 | 50 | 123 | 20.625 | 2.970 | 7.237.921 | 109.149 |
| skeptic | 1 | a668597a58df0a340 | Skeptic for three test issues | large | high | 1 | 63 | 122 | 55.377 | 26.027 | 10.151.421 | 227.218 |
| test-implementer | 1 | a7d2b2fac1db34be2 | Test implementer for run | large | medium | 1 | 41 | 82 | 35.695 | 7.745 | 5.893.761 | 131.372 |
| implementer | 1 | a70cb3eb6a0dddc9c | Implementer for run | large | medium | 1 | 23 | 48 | 9.046 | 1.202 | 2.283.182 | 191.288 |
| cleaner | 1 | af2665c6543ae8ec5 | Cleaner for run | medium | low | 1 | 6 | 12 | 2.280 | 38 | 408.801 | 85.110 |
| spec-checker | 1 | af786eeebdb8a1736 | Spec checker for run | medium | medium | 1 | 7 | 14 | 3.554 | 401 | 562.252 | 44.818 |
| auditor | 1 | a6a5c7da8d38aca43 | Auditor for run | large | high | 1 | 37 | 76 | 33.426 | 17.145 | 5.837.773 | 212.497 |
| test-implementer | 2 | af56222a51f3693b0 | Repair round 1 tests | large | medium | 1 | 26 | 52 | 24.255 | 6.694 | 2.908.920 | 140.282 |
| cleaner | 2 | aa446672345a227f5 | Cleaner after repair 1 | medium | low | 1 | 5 | 10 | 1.811 | 0 | 321.268 | 83.266 |
| spec-checker | 2 | a6b1248145971dba0 | Spec checker after repair 1 | medium | medium | 1 | 5 | 10 | 2.705 | 133 | 339.303 | 94.340 |
| auditor | 2 | a432795fa32c53f3a | Audit repair round 1 | large | high | 1 | 30 | 60 | 23.378 | 9.115 | 3.319.226 | 145.115 |
| validator | 1 | a433da10346ebf40c | Validator final tree | small | low | 2 | 7 | 14 | 3.581 | 455 | 180.289 | 74.344 |
| issue-manager | 1 | ada365655d4e2683d | Issue manager follow-ups | medium | medium | 1 | 7 | 16 | 5.729 | 504 | 576.139 | 116.634 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 270 | 563 | 201.802 | 70.898 | 37.632.204 | 1.156.921 |
| medium | 30 | 62 | 16.079 | 1.076 | 2.207.763 | 424.168 |
| small | 7 | 14 | 3.581 | 455 | 180.289 | 74.344 |
| **Total** | 307 | 639 | 221.462 | 72.429 | 40.020.256 | 1.655.433 |

## Decisions taken

11. Worth a look: #1505 narrowed to `ObservedPipeline` only (decision 2); #1577's list adjusted — two listed classes left unchanged without the summary sentence the issue asked for, two unlisted classes added (decision 3); a test-to-test `ProjectReference` from SelfHosting.Test to Formatter.Test (decision 6).

## Suggestions

- The issue for #1576 named the wrong project for `FormatterSelfHostingTests`; the issue-writing step could check a named class's project, since a project boundary decides the lane.
