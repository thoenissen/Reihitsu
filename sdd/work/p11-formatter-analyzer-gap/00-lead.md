Role: lead
Commit: 8e0f35dac358ca06f977629db45a2be1fd28c830
Verdict: lane claimed

## Issue

thoenissen/Reihitsu#1161 — record principle P11 (the formatter may do more than the analyzers). Parent: thoenissen/Reihitsu#899. Claim: draft PR thoenissen/Reihitsu#1360.
Slug: `p11-formatter-analyzer-gap`. Branch: `claude/p11-formatter-analyzer-gap-6nxpxm` (the session's designated branch, used instead of `claude/issue-<N>-<slug>`).

## Lane

Docs — the change is text in `sdd/README.md` only. Confirmed by `scripts/lane-check.sh docs --head worktree` after the spec author.

Roles run: spec author (writes the text), spec checker. Skipped: skeptic, test implementer, implementer, cleaner, auditor (docs lane does not run them); validator (no compiled file changes).

## Maintainer's clarifications (summarized)

- Write P11 into `sdd/README.md` only — in the "Open questions" section or a short section beside it. Do not edit `AGENTS.md`; if a pointer there seems needed, report it in the run report and the maintainer decides.
- Content: the formatter may do more than the sum of the analyzers; a behavior is never pushed onto an analyzer rule just because it roughly fits — that needs a specialized rule; a gap inside the construct a rule names is that rule's own gap (a bug or behavior change against the rule), not an open question about the formatter.
- Include one short example of each half, taken from the issue body.
- Give the text a stable heading or anchor: a later run (thoenissen/Reihitsu#1331) will place a mirror principle P31 next to it and must find P11 by that heading.
- No tests, no spec requirement, no new requirement IDs.
- Concurrent runs edit other files under `sdd/specs/`; this run touches none of them.
- No issues are created besides "SDD open questions" issues, and only with the maintainer's approval of the exact text.
