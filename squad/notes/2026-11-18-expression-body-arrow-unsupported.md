# expression-body-arrow-unsupported

| | |
|---|---|
| Date | 2026-11-18 |
| Run | issue #1851 |
| Pull request | #1856 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question (10) | Refuse an `#if` group around a method's signature, a constraint clause, or a property's type that closes in front of a shared `=>` (formatted and fixed on `main`) | A — refuse, declared in this work | yes |
| 2 | output question | CLI reason text: "between the arrow of an expression body and the code in front of it" vs. the issue's "an expression body's arrow" | A — the wording that pairs with the sibling shape | yes |

The declaration itself (escalation 10) was answered before the run, in the issue; the change doc's other eleven visible output changes were treated as covered by that answer and not asked again.

## Cost

- One output-question batch (one stop). No escalation stops, no repair rounds.
- The spec author ran four times: initial spec, amendment after the skeptic found an unlisted visible case (conditional group in front of a shared arrow), recording the answers, and one post-audit text fix. Each was a fresh spawn.
- The cleaner ran twice: the regular stage and a post-audit comment fix, proven text-only against the audited SHA, so no second audit.
- The test implementer was the most expensive spawn: it rewrote or removed many existing fixtures that now hold the refused shape, found with an ad-hoc Roslyn scan of test string literals.

## Usage

Session `972d43ae-7f97-54f3-89a1-c9c25e5a2307`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 15 | 54 | 132 | 21.648 | 2.284 | 8.770.277 | 235.275 | 111.620 |
| spec-author | 1 | a9cba5aaf72e3dbaf | Spec author for #1851 | large | medium | 1 | 71 | 137 | 72.707 | 33.229 | 13.005.873 | 266.462 | 188.409 |
| skeptic | 1 | a176ebd81658ea3f6 | Skeptic for #1851 | large | high | 1 | 44 | 90 | 53.420 | 33.396 | 6.964.384 | 222.738 | 148.487 |
| spec-author | 2 | a1eb7782a63a4763e | Spec author amendment #1851 | large | medium | 1 | 26 | 52 | 24.103 | 6.272 | 2.965.556 | 145.271 | 70.212 |
| spec-author | 3 | a39cf1e63f6f04dc2 | Spec author: record answers #1851 | large | medium | 1 | 10 | 20 | 5.169 | 589 | 854.915 | 110.995 | 35.975 |
| test-implementer | 1 | ad102a15f3c2e8b7f | Test implementer #1851 | large | medium | 1 | 103 | 206 | 106.404 | 20.275 | 23.524.788 | 290.063 | 266.507 |
| implementer | 1 | a847e927301266853 | Implementer #1851 | large | medium | 1 | 27 | 54 | 15.587 | 2.367 | 3.458.778 | 110.060 | 86.541 |
| cleaner | 1 | a2cdc3da39cdd80d8 | Cleaner #1851 | medium | low | 1 | 5 | 10 | 1.242 | 0 | 321.808 | 83.171 | 8.285 |
| spec-checker | 1 | ab190c7509310e2d3 | Spec checker #1851 | medium | medium | 1 | 12 | 24 | 6.072 | 1.075 | 1.109.801 | 121.341 | 46.910 |
| auditor | 1 | a0ec2a9e4af6e405d | Auditor #1851 | large | high | 1 | 58 | 116 | 54.989 | 34.939 | 11.869.696 | 290.222 | 215.711 |
| spec-author | 4 | af383315a4d9cfca0 | Spec author text fix #1851 | large | medium | 1 | 7 | 14 | 2.548 | 175 | 558.794 | 39.547 | 15.406 |
| cleaner | 2 | a1e5bf007b5a83845 | Cleaner comment fix #1851 | medium | low | 2 | 6 | 12 | 1.586 | 0 | 445.712 | 30.632 | 7.082 |
| validator | 1 | a668584d7e3a42585 | Validator #1851 | small | low | 2 | 7 | 14 | 2.700 | 309 | 141.745 | 65.358 | 11.943 |
| issue-manager | 1 | a82d90cb9c1ea251e | Issue manager #1851 | medium | medium | 1 | 7 | 16 | 3.293 | 202 | 605.573 | 57.561 | 33.393 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 400 | 821 | 356.575 | 133.526 | 71.973.061 | 1.710.633 |
| medium | 30 | 62 | 12.193 | 1.277 | 2.482.894 | 292.705 |
| small | 7 | 14 | 2.700 | 309 | 141.745 | 65.358 |
| **Total** | 437 | 897 | 371.468 | 135.112 | 74.597.700 | 2.068.696 |

## Decisions taken

Six. The one the maintainer may want to see: the change doc's visible output changes 1–11 were not asked again, because each lies inside the span the maintainer already declared (decision 6); only the case the issue did not name (an `#if` group in front of a shared arrow) and the reason text were asked. The issue's proposal that RH5401 stop reporting the shape was dropped, because SHARED-DIAG-010 already demands the report.

## Suggestions

- The test implementer needed a scratch scanner to find existing test fixtures that hold a newly declared shape. A repository tool that runs the precheck over every string literal of the test projects would make every future declaration cheaper and the search reproducible.
