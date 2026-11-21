# cognitive-complexity

| | |
|---|---|
| Date | 2026-11-21 |
| Run | issue #352 |
| Pull request | #1909 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Metric: cyclomatic or cognitive (prompt said cyclomatic, then corrected) | Cognitive | – |
| 2 | output question | Local function counts toward its holder as well as on its own | Only on its own | No |
| 3 | output question | Nesting only through a lambda lifts the no-nesting exception | No | Skeptic's pick, not spec author's |
| 4 | output question | `and`/`or` patterns score like `&&`/`||` | Yes | Yes |
| 5 | output question | Separate test-code limit | No, one setting | Yes |
| 6 | output question | Recursion scores (first ask, wrong premise in an option) | Re-asked: once per member, direct only | Yes (on re-ask) |
| 7 | output question | Chained `?:` scores like `else if` | Yes | Yes |
| 8 | output question | `or` in switch arms / case labels scores | No | Yes |
| 9 | output question | Lambdas: count toward holder / judged alone | First left out, then counted toward holder (nesting +1); initializer lambdas to a follow-up | Mixed |
| 10 | 6 | Self-hosting mechanism for existing violations | Existing self-hosting ignore list | No (proposed raised limit in `reihitsu.json`) |
| 11 | output question | `#if` scores | No, only active code | Yes |
| 12 | output question | `&`, `|`, `^` on `bool` | Score like `&&`/`||` | No |
| 13 | output question | Anonymous methods like lambdas | Yes | Yes |
| 14 | output question | Parentheses break a `?:` chain | No | Yes |
| 15 | output question | Placement of the full scoring guide | Separate documentation page | Yes |
| 16 | output question | `bool?` operators, compound assignments, non-invocation recursion, `?:` in `when` | A, A, A, A | Yes |
| 17 | output question | Recursion on another instance (`Left.Depth()`) | Counts | Yes |
| 18 | – | Approval of the concept before any implementation (maintainer's own requirement) | Approved after three rounds | – |

## Cost

- The maintainer asked for a concept review before anything was built, so the run had four question batches before the test implementer, plus a comparison against an external definition of the metric that the maintainer requested mid-discussion. The spec author ran five times (two substantive rewrites, three small amendments) and the skeptic twice.
- A web-fetch agent could not read the PDF; the saved file was read by the skeptic instead.
- One output question was asked after the implementer (receiver of a recursive call), raised by the implementer's reading of the spec's list of forms.
- No repair rounds. The auditor and the spec checker passed on the first run.
- Most expensive spawn: the test implementer (174 cases written against a hand-computed spec).

## Usage

Session `d67f12c8-68c5-5f64-bb71-040061df6b78`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 20 | 82 | 205 | 47.393 | 8.753 | 14.736.156 | 160.759 | 140.551 |
| spec-author | 1 | a17e2eadeb0c0ce17 | Spec author for issue 352 | large | medium | 2 | 35 | 72 | 56.834 | 28.104 | 5.314.825 | 146.710 | 122.378 |
| skeptic | 1 | a20bad15d4537f96e | Skeptic for issue 352 concept | large | high | 1 | 25 | 50 | 38.812 | 23.084 | 3.369.787 | 169.098 | 88.089 |
| web-fetch | 1 | aeaa4269a9db44c4b | Summarize Sonar cognitive complexity spec | small | medium | 1 | 2 | 4 | 1.372 | 510 | 4.646 | 31.601 | 26.955 |
| skeptic | 2 | afa951a14f9a75525 | Compare RH2009 with Sonar definition | large | high | 1 | 20 | 38 | 30.008 | 16.435 | 2.719.342 | 175.089 | 93.824 |
| spec-author | 2 | ab37954e1698c3593 | Spec author: apply answers, guide | large | medium | 2 | 40 | 80 | 75.810 | 31.361 | 7.109.242 | 189.064 | 163.792 |
| spec-author | 3 | a73112c243852f65c | Spec author: answers 23-24 | large | medium | 1 | 20 | 40 | 16.618 | 4.467 | 2.216.951 | 82.015 | 57.751 |
| spec-author | 4 | a9c09ff18e724e552 | Spec author: set Accepted | large | medium | 1 | 3 | 6 | 1.288 | 15 | 167.939 | 87.628 | 6.375 |
| test-implementer | 1 | a504bc6eca9cf3f86 | Test implementer for RH2009 | large | medium | 1 | 56 | 112 | 117.854 | 45.678 | 12.230.522 | 454.136 | 251.097 |
| implementer | 1 | a2ba75f37d29cee9a | Implementer for RH2009 | large | medium | 1 | 50 | 100 | 48.665 | 16.139 | 8.066.378 | 371.223 | 140.421 |
| spec-author | 5 | a5a500b197a8578c0 | Spec author: recursion + rule page | large | medium | 1 | 12 | 23 | 7.375 | 1.338 | 1.067.351 | 108.247 | 28.055 |
| test-implementer | 2 | ab9d8dfb37d938300 | Test for recursion on receiver | large | medium | 1 | 12 | 23 | 6.939 | 725 | 1.139.807 | 54.152 | 29.352 |
| cleaner | 1 | a9dfed7f39417a0f9 | Cleaner for RH2009 | medium | low | 1 | 4 | 8 | 949 | 0 | 247.171 | 84.486 | 4.609 |
| spec-checker | 1 | a0795a773e107421c | Spec checker for RH2009 | medium | medium | 1 | 10 | 19 | 6.401 | 1.351 | 967.478 | 124.606 | 45.102 |
| auditor | 1 | a83227ccdbe5f0fe2 | Auditor for RH2009 | large | high | 1 | 40 | 82 | 43.850 | 27.143 | 7.012.584 | 249.975 | 175.185 |
| validator | 1 | a0adc634d7f516d9a | Validator for RH2009 | small | low | 1 | 14 | 28 | 4.481 | 826 | 424.796 | 77.736 | 18.095 |
| issue-manager | 1 | ad9d78d11008c7814 | Issue manager follow-ups | medium | medium | 1 | 8 | 17 | 4.551 | 264 | 696.670 | 112.989 | 33.545 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 395 | 831 | 491.446 | 203.242 | 65.150.884 | 2.248.096 |
| medium | 22 | 44 | 11.901 | 1.615 | 1.911.319 | 322.081 |
| small | 16 | 32 | 5.853 | 1.336 | 429.442 | 109.337 |
| **Total** | 433 | 907 | 509.200 | 206.193 | 67.491.645 | 2.679.514 |

## Decisions taken

Six: shared member-to-body mapping with RH2008 (architecture finding), one shared statement and owner for configured limits, the skeptic's wording points, `Release.ruleset` action `None` while RH2009 is on the self-hosting ignore list, wrapping the rule page examples in a class, and not scoring lambda parameter defaults specially. The ruleset action is the one the maintainer may want to see; it is named in the PR.

## Suggestions

- An option that cites an external definition ("as the published definition charges recursion") must be checked against that definition before it is shown; here a wrong premise in one option had to be re-asked.
- When the maintainer asks for a concept review, showing the scoring as code with per-line point comments worked far better than prose; the maintainer asked for that format explicitly.
