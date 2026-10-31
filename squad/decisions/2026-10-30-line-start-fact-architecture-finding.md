# Route the line-break rewriters and disabled text outside the issue's list through the shared line-start fact

| | |
|---|---|
| Date | 2026-10-30 |
| Escalation | 12 |
| Run | issue #1487 |

## Question

The skeptic reported that several components decide where a line starts and disagree on the run's inputs. The issue's list misses the line-break rewriters for conditionals, conditional-access chains, and do-while (each moves a stranded `///` onto its own line), and disabled text as a carrier of line terminators other than `\n`. (A fix narrowly and draft a follow-up for those; B change the structure in this run, routing them through the shared line-start fact and terminator predicate; C stop the run)

## Answer

B — the run routes those rewriters and the disabled-text terminator count through the shared facts, with tests for each. The scope baseline for escalation 5 is the issue's list, its restructuring items, and these components. The approved change doc already covers the output, so no new change doc is needed.

## Reason

None given beyond accepting the recommendation.
