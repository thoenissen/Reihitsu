# parallel-cli-formatting

| | |
|---|---|
| Date | 2026-11-17 |
| Run | issue #1795 |
| Pull request | #1815 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Name of the new option | A, `--max-parallelism <count>` | yes |
| 2 | output question | Default degree | A, logical processor count | yes |
| 3 | output question | Help and README text for the option | A, help lines plus README row and example | yes |
| 4 | output question | Ctrl+C with several files in progress | A, name every finished file in selection order | yes |
| 5 | output question | One physical file reached through several paths in write mode | A, which alias path is named and the count may vary; content must be correct | yes |
| 6 | output question | Accepted `--max-parallelism` values | A, digits 1..int.MaxValue, last wins, `--lang-version`-style messages | yes |

## Cost

One output-question batch (six questions), no escalation. Two repair rounds: round 1 was one missing spec-index entry; round 2 was the auditor's high finding (a read through one alias overlapping a write through another left the file empty or truncated), plus two test-quality findings. Round 2 needed an extra test-implementer spawn because the repair's write gate made three earlier tests' scheduling preconditions impossible. A container restart killed the round-2 implementer mid-work; a fresh one reviewed its uncommitted edits and finished, so that spawn was paid twice. The expensive spawns were the first test implementer, the first auditor (reproducing the alias race end to end), and the interrupted implementer.

## Usage

Session `aa4057d5-31e9-53a6-a160-6cb46b2233ae`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 21 | 57 | 141 | 28.802 | 2.829 | 8.902.422 | 136.499 | 108.494 |
| spec-author | 1 | a6e8e7c67623fb72f | Spec author for #1795 | large | medium | 1 | 27 | 53 | 21.916 | 7.384 | 3.206.829 | 94.416 | 70.935 |
| skeptic | 1 | a97260be6c2e69bef | Skeptic for #1795 | large | high | 1 | 28 | 55 | 32.725 | 17.010 | 3.590.650 | 163.874 | 86.396 |
| spec-author | 2 | aaa58e492901ba507 | Spec author follow-up #1795 | large | medium | 1 | 10 | 20 | 11.597 | 1.548 | 936.417 | 58.725 | 35.313 |
| test-implementer | 1 | a89f3fa80f8ec7fb7 | Test implementer #1795 | large | medium | 1 | 64 | 128 | 79.854 | 22.093 | 12.898.915 | 232.216 | 208.252 |
| implementer | 1 | a6fedaca1b51571c2 | Implementer #1795 | large | medium | 1 | 24 | 48 | 22.744 | 6.033 | 3.339.973 | 123.535 | 100.282 |
| cleaner | 1 | aba6ac97d5bc8eeda | Cleaner #1795 | medium | low | 1 | 6 | 12 | 1.549 | 0 | 398.230 | 82.454 | 8.024 |
| spec-checker | 1 | a6652b1be6dec5f9d | Spec checker #1795 | medium | medium | 1 | 7 | 14 | 3.117 | 233 | 525.421 | 102.919 | 28.949 |
| spec-author | 3 | aa711f6ca05feaa43 | Spec author repair #1795 | large | medium | 1 | 5 | 10 | 1.508 | 0 | 374.651 | 33.941 | 10.070 |
| spec-checker | 2 | a9d68e65907a14227 | Spec checker recheck #1795 | medium | medium | 1 | 4 | 8 | 1.417 | 0 | 288.814 | 32.463 | 9.275 |
| auditor | 1 | a9e4d81bc025ac6a5 | Auditor #1795 | large | high | 1 | 44 | 88 | 59.234 | 37.661 | 7.850.833 | 616.537 | 182.946 |
| test-implementer | 2 | a8ce0a4cb55fc69ba | Test impl repair #1795 | large | medium | 1 | 41 | 82 | 44.999 | 15.032 | 6.165.552 | 140.629 | 117.284 |
| implementer | 2 | aee16b4f8d1805102 | Implementer repair #1795 | large | medium | 1 | 63 | 126 | 72.948 | 44.414 | 10.332.840 | 178.150 | 154.206 |
| implementer | 3 | a41c0641625e8664e | Implementer repair #1795 (restart) | large | medium | 1 | 25 | 50 | 25.584 | 11.117 | 2.878.861 | 100.276 | 76.787 |
| test-implementer | 3 | a4fa45d092b253b69 | Test impl disputed tests #1795 | large | medium | 1 | 22 | 44 | 21.748 | 5.962 | 2.333.204 | 79.268 | 55.248 |
| cleaner | 2 | adb61b0a2f5c25f3c | Cleaner repair #1795 | medium | low | 1 | 6 | 12 | 1.436 | 130 | 389.567 | 80.071 | 5.581 |
| spec-checker | 3 | a3384c7ada249e417 | Spec checker round 2 #1795 | medium | medium | 1 | 10 | 20 | 3.781 | 606 | 835.393 | 110.084 | 36.054 |
| auditor | 2 | ab94e3fde58745ac0 | Auditor repair #1795 | large | high | 1 | 27 | 54 | 22.208 | 10.582 | 3.651.695 | 173.373 | 99.201 |
| validator | 1 | a8baf10ace5b08875 | Validator #1795 | small | low | 2 | 10 | 20 | 4.236 | 1.157 | 251.502 | 72.299 | 15.891 |
| issue-manager | 1 | a04582975ef70a1a7 | Issue manager #1795 | medium | medium | 1 | 6 | 14 | 2.709 | 129 | 433.010 | 96.554 | 22.443 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 437 | 899 | 445.867 | 181.665 | 66.462.842 | 2.131.439 |
| medium | 39 | 80 | 14.009 | 1.098 | 2.870.435 | 504.545 |
| small | 10 | 20 | 4.236 | 1.157 | 251.502 | 72.299 |
| **Total** | 486 | 999 | 464.112 | 183.920 | 69.584.779 | 2.708.283 |

## Decisions taken

13. The ones worth the maintainer's eye: the write that Ctrl+C interrupts was fixed in this run as an adjacent safety-rail repair instead of an output question (a started write now completes, and the file is named); and Ctrl+C after the last file but before the summary now ends without a summary, as CLI-022 requires, where `main` printed it.

## Suggestions

- The skeptic's claim that alias content stays correct rested on concurrent writes only; the auditor found reads overlapping writes. A failure-class matrix entry for concurrent file access (read/write and write/write, across aliases) would let the skeptic enumerate the class before an output question is asked on it.
