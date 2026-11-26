# Lead — single-owner-helpers

| | |
|---|---|
| Issues | thoenissen/Reihitsu#1956 (agent-filed), thoenissen/Reihitsu#1958 (agent-filed), thoenissen/Reihitsu#1963 (agent-filed) |
| Slug | single-owner-helpers |
| Branch | claude/serene-brown-xi1igq (session-designated) |
| Draft PR | thoenissen/Reihitsu#1965 |
| Lane | bug-fix |

## Lane

Bug fix, shared by all three issues: each asks to route a duplicated predicate or helper to one owner, with no output change. The diff is production paths (and possibly tests) without a change doc.

- #1956 — `IsOwningConstruct` duplicated between `Reihitsu.Analyzer` and `Reihitsu.Formatter`; the issue proposes `Reihitsu.Core` as the owner.
- #1958 — the conditional-directive kind check duplicated in the RH7004 fix and `ConditionalGroupUnitUtilities`; `SyntaxTriviaUtilities.IsConditionalDirective` is the owner.
- #1963 — three `FindTriviaBehind*` methods in `PrecheckPhase` with one body.

All three are `agent-filed`: their goals are proposals. The maintainer batched them on purpose (comment on #1956): one maintenance run, each moving a duplicated helper to one owner, no output change.

Roles: skeptic, test implementer, implementer, cleaner, spec checker, auditor, validator. Spec author skipped (bug-fix lane; runs only for a clarification).

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

None yet.

## State

- Claimed at 68f236e (origin/main). Next: skeptic.
