# prefix-chain-anchor

| | |
|---|---|
| Date | 2026-11-20 |
| Run | issue #896 |
| Pull request | #1891 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Wrapped call behind a property prefix: align under the last prefix link? | A | yes |
| 2 | output question | Call-less chain: same anchor rule? | A | yes |
| 3 | output question | Prefix link behind a multi-line `/* */` that starts behind code: code reading of "starts a line"? | A | yes |

## Cost

One output-question batch (before the test implementer), no escalation. One repair round: audit 1 found that no test distinguished "last link" from "second link" (every fixture had exactly two prefix links) and a change-doc wording issue; the repair audit passed with one further text-only finding, proven text-only. The spec author ran four times (draft, amendment after the skeptic, two text repairs). The expensive spawns were the test implementer (it mutated the anchor in a scratch worktree to find every pinned test) and the first audit. The cleaner had to rewrite four repository chains because the Release build uses the analyzer binaries under `binaries/`, which still carry the old anchor.

## Usage

Session `e98124d9-a849-5677-91ed-33ba72d96b8b`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 15 | 63 | 162 | 20.316 | 1.974 | 9.525.885 | 118.606 | 87.188 |
| spec-author | 1 | a1023f8b1408d7d9e | Spec author for #896 | large | medium | 1 | 45 | 88 | 40.898 | 18.191 | 6.655.528 | 190.850 | 109.614 |
| skeptic | 1 | a2eff9d5475dfd937 | Skeptic for #896 | large | high | 1 | 33 | 65 | 39.000 | 23.595 | 4.606.765 | 130.394 | 106.476 |
| spec-author | 2 | ad6accd8eae9cab7c | Spec author amendment #896 | large | medium | 1 | 23 | 44 | 18.742 | 7.058 | 2.742.212 | 143.009 | 61.575 |
| test-implementer | 1 | a59df918c8c2d787c | Test implementer for #896 | large | medium | 1 | 66 | 128 | 55.881 | 14.287 | 10.736.681 | 172.954 | 148.256 |
| implementer | 1 | a1b8e01083022abaf | Implementer for #896 | large | medium | 1 | 25 | 49 | 9.847 | 782 | 2.652.495 | 490.793 | 60.159 |
| cleaner | 1 | a184946ae398e0c64 | Cleaner for #896 | medium | low | 1 | 17 | 33 | 7.956 | 1.240 | 1.610.926 | 111.717 | 30.554 |
| spec-checker | 1 | ab0382f4c53bd0b11 | Spec checker for #896 | medium | medium | 1 | 10 | 19 | 5.228 | 1.304 | 917.603 | 123.084 | 42.355 |
| auditor | 1 | a6f4af59cdd50bb7d | Auditor for #896 | large | high | 1 | 56 | 107 | 42.804 | 22.062 | 9.661.380 | 235.882 | 155.099 |
| spec-author | 3 | a9594b9f92c80be9c | Repair: change doc finding #2 | large | medium | 2 | 10 | 22 | 4.634 | 312 | 730.916 | 194.841 | 22.496 |
| test-implementer | 2 | a5c2fbacd58af1bb1 | Repair: test finding #1 | large | medium | 2 | 25 | 50 | 16.047 | 3.779 | 2.779.842 | 138.668 | 62.209 |
| cleaner | 2 | a66011778dca578d1 | Cleaner repair round 1 | medium | low | 1 | 6 | 12 | 1.601 | 0 | 442.739 | 91.374 | 10.200 |
| spec-checker | 2 | a72bedcd0e89cd970 | Spec checker repair round 1 | medium | medium | 1 | 6 | 12 | 1.989 | 152 | 465.420 | 102.550 | 21.771 |
| auditor | 2 | a3f5083c07e663e2b | Auditor repair round 1 | large | high | 1 | 11 | 21 | 12.173 | 5.743 | 1.062.214 | 123.446 | 42.654 |
| validator | 1 | a67e180e3a30245a1 | Validator for #896 | small | low | 1 | 7 | 14 | 2.753 | 250 | 135.535 | 58.082 | 7.573 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 357 | 736 | 260.342 | 97.783 | 51.153.918 | 1.939.443 |
| medium | 39 | 76 | 16.774 | 2.696 | 3.436.688 | 428.725 |
| small | 7 | 14 | 2.753 | 250 | 135.535 | 58.082 |
| **Total** | 403 | 826 | 279.869 | 100.729 | 54.726.141 | 2.426.250 |

## Decisions taken

Three: drop the skeptic's follow-up candidate on the `*/` line-break deviation (rare shape); route the skeptic's spec-wording findings together with the answer to question 3; test the comment case only through the anchor so that `main`'s `*/.Next` join is not pinned.

## Suggestions

- Test derivation: when a requirement selects "the last" of a sequence, the failure-class catalog could ask for fixtures with at least three elements, so "last" and "second" differ. Evidence: audit 1 finding #1.
- A layout change that the shipped `binaries/` analyzers still judge by the old rule forces repository code to be rewritten around both rules until the next `/bump-version`; the spec author could flag this up front so the change doc states it from the start. Evidence: audit findings #2 and #3.
