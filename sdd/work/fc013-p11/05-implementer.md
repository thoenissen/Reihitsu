Role: implementer
Commit: 2029cb7
Verdict: tests pass

Repair round 1 for auditor findings 1, 2 and hint H1 (`04-auditor.md`, audit base `22015e9`), worked under the maintainer's answers in `00-lead.md` ("Escalation answers") and `squad/decisions/2026-10-23-1419-new-rule-candidate-route.md` and `squad/decisions/2026-10-23-1419-fc013-layout-rules.md`. Those answers replace two earlier clarifications: "mentioned in the run report, with no issue created" and "keep the clause as stated". I re-derived each change myself. Where the auditor's suggested change and mine differ, the row says so.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `squad/roles/skeptic.md` (check 3) | Finding 1 (route): the new-rule candidate is no longer sent "for the run report". The skeptic now names it as a follow-up candidate, the route the maintainer chose (1A-ii). Finding 2: dropped "and no issue is created". That clause was ambiguous, and under 1A-ii it is wrong, because the maintainer decides whether the candidate becomes an issue. The P11 classification is now one colon list: the rule's own gap is a bug or behavior change of that rule, and a different construct is a new-rule candidate. H1: the "formatter output" clause is narrowed to rules SHARED-DIAG-010 covers, which are layout rules whose code fix lays code out as the formatter does. It now says that an order or content rule may report what the formatter keeps. The FC-014 sentence and the other checks are unchanged. | decisions 1A-ii and 2C; `sdd/README.md` P11; SHARED-DIAG-010 |
| `squad/failure-classes.md` (FC-013) | H1: the class now covers formatter output flagged by a rule that SHARED-DIAG-010 (P31) covers, which is a layout rule whose code fix lays code out as the formatter does. It also says that an order or content rule may report what the formatter keeps. The P11 exemption is unchanged. The ID, the origin and the other rows are unchanged. | decision 2C; SHARED-DIAG-010 (`sdd/specs/shared/diagnostics.md:531-533`) |
| `squad/lead.md` (*Follow-ups*) | Finding 1, receiving side: one added sentence. It says a new-rule candidate (P11, linked by anchor `../sdd/README.md#the-formatter-may-do-more-than-the-analyzers-p11`) that a handover names is a follow-up candidate like any other: the lead presents it, and the maintainer decides whether it becomes an issue. The existing search, draft and approval steps of that section then apply unchanged. §7 step 5 (run report) is not changed. Under 1A-ii the candidate reaches the maintainer through *Follow-ups*, and the PR links an approved issue under `Follow-up work`. The auditor's suggested addition to §7 step 5 belonged to the superseded run-report route. | decision 1A-ii |

I verified the links. `../sdd/...` from `squad/` and `../../sdd/...` from `squad/roles/` both resolve, and the P11 anchor matches `sdd/README.md:117` as before.

## Policy owners
- I ran `rg -n -i "flagged by|new-rule candidate|formatter output" squad AGENTS.md CLAUDE.md .claude`, excluding `sdd/work` and `squad/decisions`. It found the policy only in the three edited lines and in `squad/roles/test-implementer.md:23`. That line names the counterpart surface as evidence to run before `not reproduced`, not as a reporting rule, so it does not conflict and stays unchanged.
- The owners are still `sdd/README.md` P11 (classification) and SHARED-DIAG-010 (which rules may not report formatter-kept shapes). The squad texts restate only each owner's scope sentence and link to it.
- The follow-up route has one owner, `squad/lead.md` *Follow-ups*. Check 3 defers to it instead of describing its own route.

## Tests run
No test covers these Markdown files, and no compiled file changed. `scripts/lane-check.sh tooling` at `8261e8d`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.

## Disputed tests or scope
I made a scope note for the lead and did not act on it. The skeptic handover template (`squad/roles/skeptic.md`, *Handover*) has no `Follow-up candidates` section, though `squad/README.md:103` and now check 3 send finds there. The auditor named a matching slot as optional. The template is outside "check 3", so I did not add one. The candidate still reaches the lead because *Follow-ups* covers "any handover". Whether to add the slot is the lead's call.

## Follow-up candidates
_None._
