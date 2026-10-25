# The lane check does not classify a root CONTRIBUTING.md

| | |
|---|---|
| Date | 2026-10-25 |
| Escalation | 10 |
| Run | issue #1467 |

## Question

After the repair that corrected a stale line in `CONTRIBUTING.md`, `scripts/lane-check.sh tooling` reported "no lane", because `LaneClassifier` classifies the root `CONTRIBUTING.md` as unknown although `squad/README.md` counts Markdown as docs. Options: A — drop the `CONTRIBUTING.md` edit and leave it to a follow-up; B — keep the edit and make the classifier treat `CONTRIBUTING.md` as text in this run, with a test; C — keep the edit and publish with an accepted "no lane" verdict.

## Answer

B: keep the edit and fix the lane classifier in this run, covered by a test in `Reihitsu.Tooling.Test`.

## Reason

None given.
