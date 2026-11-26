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
| 1 | skeptic | Is the absence of a reproduction escalation 4? | No. All three issues are maintenance with no reported behavior; the existing tests are the characterization, and the test implementer applies *Coverage first* | The issues report a duplicated statement (FC-015), not an observed output; the maintainer's comment on #1956 frames the batch as no-output-change maintenance | No output changes; every duplicate agrees with its owner today | - |
| 2 | skeptic | #1956: where does `IsOwningConstruct` live in Core? | A public member of an existing Core class (the implementer picks the class, for example `SyntaxIndentationUtilities`); body moved verbatim; the stale summaries the skeptic lists are corrected | No new type, so no escalation 9; verbatim move keeps every arm | Same predicate, same callers | - |
| 3 | skeptic | #1963: route only the issue's two methods (A), every token-to-node read (B), or also the token-to-token reads (C)? | B: one helper for "trailing trivia of a token, then leading trivia of the next node's first token", used by all five token-to-node reads; the token-to-token helpers stay as they are; the shape facts in the binary and arrow summaries survive | A misses the issue's own Done-when; the token-to-token read is a different read and widening to it is scope the issue does not need | Same spans read, precheck output unchanged | - |

## Follow-up candidates

None yet.

## State

- Claimed at 68f236e (origin/main).
- Skeptic done (01-skeptic.md, 7ccb087): all three proceed, no output questions, no follow-ups. Next: test implementer.
- Test implementer done (02-test-implementer.md, b54a551): tests for all three issues, all green on 1df7506, no follow-ups. Next: implementer.
- Implementer done (03-implementer.md, 7db988d): all tests green, no follow-ups. Next: cleaner.
- Cleaner done (04-cleaner.md, 0524ace): clean, Release 0 warnings. Next: spec checker.
