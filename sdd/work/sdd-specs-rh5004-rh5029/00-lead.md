Role: Lead
Commit: 33a29f7
Verdict: Lane claimed — specs and tests for existing behavior

## Issue

thoenissen/Reihitsu#977 — SDD analyzer specs: RH5004–RH5029 (statement blank lines). Part of thoenissen/Reihitsu#899.
Claim PR: thoenissen/Reihitsu#999 (draft). Branch: `claude/stoic-albattani-xqzycd` (the session's designated branch, used instead of `claude/issue-977-<slug>` because the session may push only there).

## Slug

`sdd-specs-rh5004-rh5029`

## Lane

Specs and tests for existing behavior (`tests`). Reason: the issue asks for specs of existing rules plus the tests that cover their requirements, and states "No production code changes."

Roles run: spec author → skeptic → test implementer → cleaner → spec checker → validator.
Roles skipped: implementer (no production code in this lane), auditor (the lane does not run it).

## Rules in scope (16)

RH5004, RH5005, RH5006, RH5007, RH5008, RH5009, RH5012, RH5013, RH5014, RH5015, RH5016, RH5017, RH5018, RH5019, RH5021, RH5029.

## Maintainer's clarifications (verbatim)

> - Issue #977 is one batch of the planned analyzer-spec backfill under #899. Read #977, then AGENTS.md, sdd/README.md, sdd/templates/analyzer-spec.md, squad/README.md, squad/lead.md.
> - Read the shared spec sdd/specs/shared/statement-blank-lines.md and the existing member specs sdd/specs/analyzer/RH5001.md, RH5002.md, RH5003.md, RH5010.md — the new specs follow them.
> - Lane: "specs and tests for existing behavior". No production code changes.
> - Done means: one spec per listed rule (16 rules incl. RH5005/RH5006), the existing tests tagged with [Requirement], a focused test added only where a requirement has no covering test, every example verified against a test or by running it, open questions filed per sdd/README.md, CI green, PR based on main with no sdd/work/.
> - Must not: write current-but-defective behavior into a spec; re-file #972 or #946; add a test for the `await using`/`await foreach` form (that is #946); change any shared spec requirement without asking me.
> - You may file open questions as the rulebook says (sub-issues of #912) without asking. Do not create any other issue without my approval. Opening the claim PR and publishing it per squad/lead.md is approved.

## Notes for every role

- A shared spec requirement (SHARED-*) is not changed without the maintainer; a role that finds one must change says so in its handover as an escalation candidate.
- Open questions are filed by the lead from the spec author's handover; roles list them, with input, real output, why it looks wrong, and the requirement IDs touched.
- No open `SDD open questions` issue exists for `shared/statement-blank-lines.md` or for any of the 16 rule specs at claim time.
