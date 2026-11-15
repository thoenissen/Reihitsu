# primary-and-partial-constructors

| | |
|---|---|
| Date | 2026-11-15 |
| Run | issues #1680, #1742 |
| Pull request | #1744 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question + 9 | Report the defining part of a partial constructor in ~35 per-part syntax-node rules through a new shared mechanism (A), or keep today's output and state the limitation in the specs (B)? | B; a diagnostic identical on both parts is reported once; close #1742 with `Closes` | Yes |

## Cost

One stop (the question above, before the test implementer). One repair round, from audit 1: three unchanged clarity rules (RH3002, RH3005, RH3006) double-reported `#if`/`#elif` condition expressions by the same mechanism, taken in as an adjacent fix, plus two untested guard edges. The spec author was continued twice (a factual correction of SHARED-DIAG-013, a code-owner rename the implementer's restructuring caused); the implementer once (the first version added a new type, folded into an existing class to stay below escalation 9). The skeptic's driver probe over two Roslyn versions was the most expensive single spawn and decided both issues.

## Usage

## Usage

Session `ecf70d5d-8753-5676-a1ed-05be6a5d0a2d`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 14 | 56 | 139 | 24.421 | 4.981 | 8.464.846 | 127.105 |
| skeptic | 1 | a066674878351f12e | Skeptic premise check | claude-opus-5-5 | high | 1 | 65 | 126 | 60.638 | 26.489 | 10.475.377 | 231.643 |
| spec-author | 1 | a6496228b1b71e97b | Spec author clarification #1742 | claude-opus-5-5 | medium | 2 | 22 | 48 | 17.569 | 6.540 | 2.378.971 | 147.990 |
| test-implementer | 1 | a6a8f1965954789c1 | Test implementer reproductions | claude-opus-5-5 | medium | 1 | 38 | 76 | 30.529 | 6.514 | 4.782.716 | 172.261 |
| implementer | 1 | a56bbff67a519c689 | Implementer for #1680 | claude-opus-5-5 | medium | 2 | 33 | 68 | 22.504 | 5.280 | 4.201.145 | 98.598 |
| cleaner | 1 | afd1821740e551b99 | Cleaner pass | claude-sonnet-5-5 | low | 1 | 6 | 12 | 2.140 | 61 | 445.607 | 93.726 |
| spec-checker | 1 | a59c2baa9cc7e7584 | Spec checker pass | claude-sonnet-5-5 | medium | 1 | 8 | 15 | 4.373 | 846 | 669.970 | 108.854 |
| auditor | 1 | ab5ba22a3edcbad03 | Auditor pass | claude-opus-5-5 | high | 1 | 45 | 92 | 42.191 | 20.931 | 7.007.904 | 210.557 |
| test-implementer | 2 | a0044b2cdbb464c4b | Repair round 1 tests | claude-opus-5-5 | medium | 1 | 17 | 33 | 14.217 | 3.646 | 1.689.821 | 128.759 |
| implementer | 2 | ae9c90fc4353c53df | Repair round 1 implementer | claude-opus-5-5 | medium | 1 | 13 | 26 | 10.144 | 1.771 | 1.140.303 | 108.835 |
| cleaner | 2 | abc185b97676605dc | Cleaner repair round 1 | claude-sonnet-5-5 | low | 1 | 6 | 12 | 1.740 | 114 | 425.514 | 87.738 |
| spec-checker | 2 | a173ad47a24d611c5 | Spec checker repair round 1 | claude-sonnet-5-5 | medium | 1 | 5 | 10 | 2.046 | 125 | 346.801 | 93.871 |
| auditor | 2 | ab78d30a8b2a60abe | Auditor repair round 1 | claude-opus-5-5 | high | 1 | 19 | 38 | 19.691 | 9.667 | 2.048.643 | 136.528 |
| validator | 1 | ac8fc29a2e31e7407 | Validator final tree | claude-haiku-5-5 | low | 1 | 8 | 16 | 3.296 | 229 | 583.205 | 63.107 |
| issue-manager | 1 | a18ef50e043532afb | Issue manager follow-ups | claude-sonnet-5-5 | medium | 1 | 9 | 20 | 3.794 | 665 | 817.810 | 61.097 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 8 | 16 | 3.296 | 229 | 583.205 | 63.107 |
| claude-opus-5-5 | 308 | 646 | 241.904 | 85.819 | 42.189.726 | 1.362.276 |
| claude-sonnet-5-5 | 34 | 69 | 14.093 | 1.811 | 2.705.702 | 445.286 |
| **Total** | 350 | 731 | 259.293 | 87.859 | 45.478.633 | 1.870.669 |

## Decisions taken

12. The ones a maintainer may want to see: widening #1680 to the region rules and, after audit 1, to RH3002/RH3005/RH3006 (same duplicate mechanism); folding the shared guard into the existing extension class, renamed to `AnalysisContextExtensions`, instead of a new type; no change doc for SHARED-DIAG-013 (it records today's behavior); not filing the RH8102 observation (the probe used a parameterless constructor, outside SHARED-DOC-015).

## Suggestions

- The skeptic named the four region rules but not the clarity rules that register on expression kinds, which also occur inside directive conditions; the audit found them. A failure-class matrix entry for "nodes inside structured trivia attached to a type with a parameter list" listing trivia kinds and expression kinds would let the skeptic enumerate both.
