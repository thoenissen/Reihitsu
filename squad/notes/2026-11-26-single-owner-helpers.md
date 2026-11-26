# single-owner-helpers

| | |
|---|---|
| Date | 2026-11-26 |
| Run | issues #1956, #1958, #1963 |
| Pull request | #1965 |

## Questions to the maintainer

None.

## Cost

No stops for the maintainer. One repair round after the first audit: two low findings (a dead null check the routing made unreachable, and a characterization test that duplicated the 15 rows of an existing test), plus two comment hints taken in. Test implementer, implementer, cleaner, spec checker, and auditor were each spawned twice for that round, every time fresh. The skeptic and the first audit were the most expensive spawns. The skeptic widened #1963 from three to five gap readers before anything was built, which let the issue's own Done-when be met in one pass.

## Usage

Session `a795000f-5f84-5b83-ac1c-9de4ae5cf4e2`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 9 | 43 | 103 | 17.830 | 1.356 | 6.077.419 | 108.710 | 76.772 |
| skeptic | 1 | aa104ea2e63766c66 | Skeptic for #1956 #1958 #1963 | large | high | 1 | 48 | 93 | 40.018 | 18.594 | 6.803.467 | 196.939 | 117.980 |
| test-implementer | 1 | aee2ec96834dfb6ca | Test implementer for #1956 #1958 #1963 | large | medium | 1 | 33 | 66 | 26.186 | 8.860 | 3.939.318 | 154.424 | 78.590 |
| implementer | 1 | a881c004e2f6f8079 | Implementer for #1956 #1958 #1963 | large | medium | 1 | 30 | 62 | 17.439 | 2.469 | 3.295.045 | 177.720 | 64.671 |
| cleaner | 1 | aacbf04e3b7e226ba | Cleaner for #1956 #1958 #1963 | medium | low | 1 | 5 | 10 | 1.349 | 0 | 323.655 | 83.459 | 7.673 |
| spec-checker | 1 | a6c7a5836c9474c31 | Spec checker for #1956 #1958 #1963 | medium | medium | 1 | 8 | 16 | 2.613 | 243 | 590.471 | 95.457 | 20.125 |
| auditor | 1 | aa37fb8adf95b68a3 | Auditor for #1956 #1958 #1963 | large | high | 1 | 33 | 68 | 25.021 | 11.438 | 4.562.262 | 180.994 | 105.606 |
| test-implementer | 2 | a7058290bb977cc86 | Test repair for single-owner-helpers | large | medium | 1 | 8 | 16 | 3.644 | 269 | 665.258 | 104.170 | 24.632 |
| implementer | 2 | a05c8e9bdab2d31e0 | Implementer repair for single-owner-helpers | large | medium | 1 | 15 | 29 | 8.212 | 962 | 1.449.692 | 56.971 | 31.275 |
| cleaner | 2 | aed1a7f16dfe22f9e | Cleaner repair 1 single-owner-helpers | medium | low | 1 | 5 | 10 | 1.275 | 0 | 329.179 | 84.297 | 4.854 |
| spec-checker | 2 | a54a389502fd51884 | Spec checker repair 1 | medium | medium | 1 | 5 | 10 | 1.864 | 58 | 353.741 | 96.605 | 17.589 |
| auditor | 2 | a0990dc9030939962 | Auditor repair 1 single-owner-helpers | large | high | 1 | 15 | 29 | 11.264 | 3.550 | 1.546.207 | 128.321 | 49.231 |
| validator | 1 | ad6ae78407a1ab00d | Validator for single-owner-helpers | small | low | 1 | 9 | 18 | 3.041 | 349 | 218.644 | 53.188 | 11.207 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 225 | 466 | 149.614 | 47.498 | 28.338.668 | 1.108.249 |
| medium | 23 | 46 | 7.101 | 301 | 1.597.046 | 359.818 |
| small | 9 | 18 | 3.041 | 349 | 218.644 | 53.188 |
| **Total** | 257 | 530 | 159.756 | 48.148 | 30.154.358 | 1.521.255 |

## Decisions taken

Six, all internal: no reproduction needed for maintenance issues without an observed output; `IsOwningConstruct` goes into the existing `SyntaxIndentationUtilities` rather than a new type; #1963 routes every token-to-node gap read (five, not the issue's three) through one helper and leaves the token-to-token readers alone; the two audit findings as one repair round; two comment hints taken in; a third comment hint about pre-existing wording not taken. The maintainer may want to see the third: it widens #1963 beyond its text.

## Suggestions

The test implementer added a characterization test whose rows duplicated an existing test's rows, which the auditor then had to flag as FC-015 in a run that removes FC-015 from production code. A characterization test that strengthens an existing parity test could extend that test instead of copying its data.
