Role: spec author
Commit: 85adc719
Verdict: change doc written

Repair round 2: auditor finding #5 (`12-auditor.md`, round 2), maintainer answer 8 in `00-lead.md`, and round 2 of `13-test-implementer.md`. The prompt carried no intended fix beyond those; nothing was ignored. No test expectation was changed.

## What changed

All changes are in `sdd/changes/2026-10/node-root-detached.md`. New passages are marked *(amended 2026-10-31, repair round 2)*, and the amendment note names them.

- **What, the implementer-stage bullet:** it now names three kinds of changed target. The new kind is an assignment inside a conditional access whose member stands on its own line and whose value is wrapped. Such a target keeps the member's line break, because the chain rule now answers for a root that the assignment rewriter changed first. The bullet names the RH5111 and RH6023 fixes as the callers.
  - The 88-output count is kept. A sentence states that it predates the single chain-rule owner, and that the round-2 audit found these outputs through the two fixes.
- **Visible output changes:** a third entry for the RH5111 fix on `a?.B`, `  .C =`, `    5;`, using the input of `RH5111…Tests.VerifyFixKeepsTheWrappedLinkOfAnAssignedMemberBehindAMemberBinding`.
  - Before (`main`): `a?.B.C = 5;`. After: `a?.B` and `  .C = 5;`.
  - In prose: the element-binding case (`a?[0]`) and the nested case (`a?.B?.B`, `.C` in column 13); the RH6023 case (`.C=` with a wrapped value); and the member-on-one-line case, which does not change.
- **Decisions:** a new section, "A wrapped member of an assignment inside a conditional access keeps its line". It records the maintainer's choice A, gives the reason (it keeps the author's wrap, as whole-document formatting does), and states why `main`'s join cannot come back: it would make the chain decision depend on whether an earlier rewriter changed the root, which FMT-PIPE-023 rules out.

## Verification

I ran `scripts/apply-fix.sh RH5111` and `scripts/apply-fix.sh RH6023` on four fixtures, under LF and CRLF:

- `a?.B` with `.C =` and `5;`
- `a?[0]` with `.C =` and `5;`
- `a?.B?.B` with `.C =` and `5;`
- `a?.B` with `.C=` and `5;`

Both trees were checked: `main` as `c24bc1d`, extracted to the scratchpad, and this branch.

- `main` joins every case into one line (`a?.B.C = 5;`, `a?[0].C = 5;`, `a?.B?.B.C = 5;`).
- The branch gives the outputs above. In the nested case, `.C` is in column 13.
- RH6023 reports only the `.C=` fixture, and fixes it the same way.
- Every fix converges in one iteration.

`scripts/test.sh --no-install --project architecture`: Passed 92/92.

## Changed requirements

None. The change doc only; no spec text and no ID.

## Examples for the tests

These already exist from `13-test-implementer.md`, round 2: the three RH5111 tests and the one RH6023 test named there.

## Output questions

_None._ Answer 8 decides the new output.

## Internal choices

_None._

## Follow-up candidates

_None._
