Role: spec author
Commit: b753aa77
Verdict: spec written

Repair round 1, lead decision 6 (`00-lead.md`), from the hint in `13-test-implementer.md`. My prompt asked for this wording change only and carried no intended fix, suspected cause, or hoped-for verdict. Wording only: no requirement added, retired, or renumbered, and no output changes.

## Changed requirements

- RH5201-002 — clarified: the `*/` paragraph now opens "A link other than the chain's first link whose operator stands on the last line of a delimited comment …", and a new sentence states that the chain's first link behind such a comment MUST NOT be reported by this paragraph, whatever column the comment's last line starts in: SHARED-CHAIN-005 does not decide the first link, SHARED-CHAIN-004 keeps it in place, and a first link that starts a line below the root is RH5112's (the spec's non-goal). It names the consequence: the comment keeps the first link on its own line, the anchor is the start of the root, and the first call is reported per RH5201-006 (`var a = c` ⏎ `/* x` ⏎ `*/ .Next.Self();`, comment in the root's column: `.Self` reported, `.Next` not). *Why* adds one sentence: no rule requires the first link to start a line; the comment only keeps it where it stands.
- Qualifier "other than the chain's first link" (FC-002): reported side is example `d` (`.Next` behind `*/` is the second link, reported); not-reported side is the inline example above, which is exactly the test `VerifyFirstLinkBehindOwnLineCommentTailInRootColumnIsNotReported`.
- I first cited SHARED-CHAIN-003 as well; the Uses-row test failed (not listed in Uses), so the sentence now cites only SHARED-CHAIN-004/005, which RH5201 already uses. No Uses change.
- Rule page and change doc: unchanged (corner case; nothing visible changes, code was already `linkIndex > 0`).

Checks on the worktree: `scripts/test.sh --project architecture` 198/198 passed; `scripts/lane-check.sh behavior-change --head worktree` confirmed.

## Examples for the tests

None new: the not-reported case is already covered by `VerifyFirstLinkBehindOwnLineCommentTailInRootColumnIsNotReported` (13); the reported side by the existing `d` tests.

## Output questions

_None._

## Internal choices

- Cite SHARED-CHAIN-004/005 and RH5112 rather than also SHARED-CHAIN-003; the alternative was adding SHARED-CHAIN-003 to the Uses row, which widens the spec's dependency list for a wording fix.

## Follow-up candidates

_None._
