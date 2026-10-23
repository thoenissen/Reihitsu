Role: auditor
Commit: 0b4ad73141995f3296dd822fffba0b5566634d78
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

This audits only the delta of repair round 1, `22015e9..0b4ad73`, against `04-auditor.md`. Head `0b4ad73` is checked out with a clean tree. `origin/main` is `c81091b883383d0735af167e15603fb702f1d7b4`, matching the prompt, and is an ancestor of the head.

Outside `sdd/work/`, the repair changes four kinds of file:

- `squad/failure-classes.md` (FC-013)
- `squad/roles/skeptic.md` (check 3)
- `squad/lead.md` (*Follow-ups*, one sentence)
- two new entries, `squad/decisions/2026-10-23-1419-fc013-layout-rules.md` and `squad/decisions/2026-10-23-1419-new-rule-candidate-route.md`

That is what `05-implementer.md` describes. Nothing changed outside `squad/` and `sdd/work/`, and `git diff --check` is clean.

Scope grew to `squad/lead.md` and `squad/decisions/`. The maintainer's answers recorded in `00-lead.md` ("Escalation answers") authorize this, so it is not an unrelated surface. Since the spec checker's commit (`94887b3`), only its handover changed. `scripts/lane-check.sh tooling` on the head prints `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.

The maintainer's answers replace two earlier clarifications in `00-lead.md`. The new rule candidate route is decision 1A-ii, which replaces "mentioned in the run report, no issue". The FC-013 narrowing is decision 2C, which replaces "keep the clause as stated". I audited the repair against those answers.

Previous findings:

| # | Status | Evidence |
|---|---|---|
| Finding 1 (new-rule candidate had no route) | closed | `squad/roles/skeptic.md:23` now has the skeptic name the candidate "as a follow-up candidate". `squad/lead.md:83` (*Follow-ups*) takes follow-up candidates from "any handover" and now names the P11 new-rule candidate as one, which the lead presents and the maintainer decides on. That is decision 1A-ii. The candidate now reaches the maintainer before `sdd/work/` is deleted (`lead.md` §7 step 3). The run-report slot I suggested belonged to the replaced clarification, and the maintainer's answer supersedes it. |
| Finding 2 ("and no issue is created" ambiguous) | closed | The clause is gone. Check 3's classification is now one colon list ("the rule's own gap is a bug or a behavior change of that rule, and a different construct is a new-rule candidate, which you name as a follow-up candidate"). Issue creation is left to `lead.md` *Follow-ups*, which creates an issue only after the maintainer approves its exact text, as 1A-ii records. |
| Hint H1 (FC-013 wider than SHARED-DIAG-010) | closed | Under 2C, FC-013 and check 3 now cover only "a rule that SHARED-DIAG-010 (P31) covers — a layout rule whose code fix lays code out as the formatter does". Both add "an order or content rule may report what the formatter keeps". See guard scope, row 1. |
| Hint H2 (FC-013 cell has two sentences) | unchanged | Style only. Still true, not a finding. |

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| _None._ | | | | | | |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which reports on formatter output are FC-013 / a conflict | FC-013 sentence 1 (`squad/failure-classes.md:59`) and check 3 sentence 1 (`squad/roles/skeptic.md:23`): "a rule that SHARED-DIAG-010 (P31) covers — a layout rule whose code fix lays code out as the formatter does"; "an order or content rule may report what the formatter keeps" | SHARED-DIAG-010 (`sdd/specs/shared/diagnostics.md:531-533`): subject is layout and the fix lays the reported code out as the formatter does; order and content rules are not covered; a rule without such a fix is not covered | Both qualifiers of the requirement are explicit conditions (FC-002). The third exemption, a layout rule without such a fix, is not named in the "may report" sentence but falls outside the predicate itself | ok |
| Where a P11 new-rule candidate goes | check 3: "which you name as a follow-up candidate" | `squad/lead.md:83` *Follow-ups*: a follow-up candidate from any handover, including the new-rule candidate, is presented to the maintainer; `squad/README.md:103` for out-of-scope finds | Sender and receiver now name the same route; decision 1A-ii | ok |
| Whether an issue is created | check 3: no longer states it | `lead.md:83`: search, reuse an exact match, draft from `.github/ISSUE_TEMPLATE/`, create only after approval of the exact text | One owner; the new sentence ("like any other") adds no step of its own and contradicts none | ok |
| Rule's own gap | check 3: "a bug or a behavior change of that rule" | P11 (`sdd/README.md:121`); an out-of-scope find follows `squad/README.md:103` | Check 3 no longer suppresses the follow-up route for it | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which rules must not report a shape the formatter keeps | SHARED-DIAG-010 (`sdd/specs/shared/diagnostics.md:529-535`, owner). Restated in `squad/failure-classes.md:59`, `squad/roles/skeptic.md:23`, and `sdd/README.md:137` (silence of a layout rule only) | Both squad texts restate the owner's scope sentence and link it. Parity holds | ok |
| Route of a follow-up candidate, including a P11 new-rule candidate | `squad/lead.md:83` (owner), `squad/README.md:103`, `squad/roles/implementer.md:36`, `squad/roles/skeptic.md:23` (defers). From `rg -n -i "new-rule candidate\|no issue is created\|for the run report\|follow-up candidate" squad AGENTS.md CLAUDE.md .claude sdd/README.md`, excluding the two new decisions | One owner. No residual "for the run report" or "no issue is created" wording remains anywhere | ok |
| P11 classification | unchanged since `04-auditor.md`; check 3 still defers with "classify it as P11 does" | ok |
| Decisions entries | Both new files follow the `squad/decisions/README.md` template (title, Date / Escalation / Run table, Question with options, Answer summarized in English, Reason "none given"). Naming follows the existing `yyyy-mm-dd-<issue>-<slug>.md` entries | ok |

## Assertion adequacy

_N/A — the repair adds or changes no test, and no test reads `squad/` files._

## Failure classes dismissed

- FC-002: reached by the narrowed FC-013 predicate. Both qualifiers are explicit (guard scope, row 1).
- FC-015, FC-016: reached. Each changed policy has one owner, and no sibling copy keeps the old wording (policy ownership).
- FC-031, by analogy for text: the `lead.md` *Follow-ups* paragraph still agrees with §6's routing of follow-up candidates. Only `00-lead.md` still holds the replaced clarifications, and it marks them as replaced; `sdd/work/` is deleted before publishing.
- FC-001, FC-003 – FC-014, FC-017 – FC-030, FC-032 – FC-037: the evidence is the same as in `04-auditor.md`. The repair again holds no compiled file and no test (lane check: Markdown under `squad/` and `sdd/work/` only).

## Hints

- R-H1 (pre-existing, not blocking): the skeptic handover template (`squad/roles/skeptic.md`, *Handover*) has no `Follow-up candidates` section, although check 3 now sends the new-rule candidate there. Of the role templates, only `squad/roles/implementer.md` has that section (`rg -c "Follow-up candidates" squad/roles/*.md`). Every other role already relies on `squad/README.md:103` and on `lead.md` *Follow-ups* collecting from "any handover", so the route works without the slot. The implementer and the spec checker both left the slot to the lead. Whether to add one is a template choice outside check 3.
- R-H2 (pre-existing, follow-up candidate): check 3 now says "an order or content rule may report what the formatter keeps". The same check, and FC-014, still call a new analyzer "whose concern no formatter phase owns" a source of permanent diagnostics for CLI users. SHARED-DIAG-010's Why (`sdd/specs/shared/diagnostics.md:535`) says the formatter deliberately does not own member order or comment content. So for a new order or content rule, FC-014 names as a risk what the spec calls intended. FC-014 is unchanged and stays outside this run: the maintainer's clarification forbids changing other failure classes, and the tension predates the run. The repair only puts the two sentences next to each other.
