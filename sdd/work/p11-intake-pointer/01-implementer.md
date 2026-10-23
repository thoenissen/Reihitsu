Role: implementer
Commit: 5b7af62
Verdict: tests pass

## Changes
| File | What changed | Requirement |
|---|---|---|
| `AGENTS.md` | Section "Issue intake — question the report": one new bullet (one sentence), inserted after the "Reject a framing the repository's own policy contradicts" bullet. It tells the reader to check a report that asks an analyzer to follow the formatter, or that calls the formatter wrong because no rule reports what it rewrites, against the `sdd/README.md` section "The formatter may do more than the analyzers (P11)" before implementing it, and states P11's substance: the formatter may do more than the analyzers; a rule is not widened to a different construct only because it roughly fits; a gap inside the rule's own construct is the rule's gap. | thoenissen/Reihitsu#1402 and the maintainer's clarifications in `00-lead.md` |

No other line of `AGENTS.md` changed; `CLAUDE.md` and `sdd/README.md` are untouched.

## Link verification
- Link target: `sdd/README.md#the-formatter-may-do-more-than-the-analyzers-p11`, relative to the repository root where `AGENTS.md` lives.
- Heading on the branch head (equal to `main` at 3dde060): `## The formatter may do more than the analyzers (P11)` (line 117). Its GitHub slug, computed by lower-casing, dropping punctuation other than `-`, and replacing spaces with `-`, is `the-formatter-may-do-more-than-the-analyzers-p11`, identical to the link's anchor. `sdd/README.md` itself links the same anchor at line 115.
- The sentence stays within the section's closing sentence: it speaks only of the direction in which the formatter does more than a rule reports, and claims nothing about a shape the formatter deliberately keeps (SHARED-DIAG-010).

## Policy owners
None touched; the change is prose. The P11 policy has one owner, the `sdd/README.md` section, which the new bullet links instead of restating it in full (`rg -n "P11" AGENTS.md sdd/README.md` finds the section, its in-file link, and the new pointer).

## Tests run
- No test covers `AGENTS.md` prose (lane table in `00-lead.md`).
- `scripts/lane-check.sh tooling` with `AGENTS.md` committed: `LANE CHECK: confirmed — claimed tooling, diff needs tooling` (`AGENTS.md` classified Tooling, the handovers Text).

## Disputed tests or scope
_None._ The prompt carried no intended fix or suspected cause beyond the maintainer's clarifications, which the lead handover records.

## Follow-up candidates
_None._
