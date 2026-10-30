# Whether the stale FormatNode remarks are corrected in a run that changes no production file

| | |
|---|---|
| Date | 2026-10-30 |
| Escalation | 7 |
| Run | issue #1484 |

## Question

The first audit found that the public XML remarks of `ReihitsuFormatter.FormatNode` still state the detached-node clause narrowed under escalation 4 ("a detached node carries the default parse options and is formatted for the newest supported version"). The issue's Done-when says no production file changes, and a comment-only edit in `src/Reihitsu.Formatter/` next to compiled test changes and `AGENTS.md` is not admitted by the lane check without a change doc.

- **A** — Leave the remarks out of this run and file a follow-up.
- **B** — Fix them in this run and widen to the behavior-change lane with a change doc.
- **C** — Fix them in this run and record the failing lane check as accepted.

## Answer

The comment may be corrected in this run, and the run's lane stays tooling. A comment-only edit of the remarks does not change the run's lane; if the lane check rejects it, that is accepted by the maintainer and stated in the run report.

## Reason

None given.
