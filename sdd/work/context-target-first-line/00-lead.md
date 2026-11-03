Role: lead
Commit: 21c2ff4ba9e1b1846c39452468e2802c28c19ec8
Verdict: run started

# Run: context-target-first-line

## Issues

- thoenissen/Reihitsu#1501 — "FMT-PIPE-017: the context entry point's target keeps the phases' first line". Labels: `enhancement`, `formatter`, `agent-filed` (goals, "Done when", target behavior, and prescribed solution are proposals). Sub-issue E1 of thoenissen/Reihitsu#1482. Blockers thoenissen/Reihitsu#1487 (B2) and thoenissen/Reihitsu#1489 (B4) are closed (merged in PRs #1510 and #1520).
- The issue was written against an older `main`; every statement about today's code is checked against current `main` (base above) before it is used.

## Lane

Behavior change. The issue asks for changed formatter behavior of `FormatNodeInDocumentWithContextAsync` and changed requirements FMT-PIPE-017/FMT-PIPE-018, and asks for a change doc.

Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing when there are follow-up candidates. No role is skipped.

## Maintainer's clarifications (summarized)

- The node-level formatting methods (`FormatNodeInDocumentWithContextAsync` and its siblings) are not a public API. Only the cases the analyzer code fixes actually reach have to be covered — inputs that only a direct call of the method would produce are out of scope (AGENTS.md, *Supported inputs*). Every role names the code fix (and the node it formats / the context it passes) that reaches each case it specifies, tests, or reports.

## Maintainer answers

- Output questions batch 1 (before the test implementer), 18 questions from `03-spec-author.md`: the maintainer accepted the recommendation on every question — A for 1–18. Question 1: the target's first line keeps its document column and its aligned lines follow it. Question 4: a parameter that shares its line with another parameter keeps that line, with no line break in front of it.
- Added guidance from the maintainer: a formatter change made only for code fixes has to be worth its cost. Implementation must stay proportionate; for this change the maintainer sees no problem.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | `claude/stoic-bohr-xb3ycb` instead of `claude/issue-1501-<slug>` | The session's environment pins the development branch; pushing elsewhere is not permitted | Branch names are internal | claim commit |
| 2 | Spec author | Keep the entry point's write-back of the target's leading trivia, against the issue's "Done when" item | Keep it; drop that goal | The preparation phase rewrites tabs/line endings in the whole root; the write-back keeps the document's tabs (FMT-PIPE-018) and, with the phases keeping the trivia, changes no column | Agent-filed goal; the write-back changes no column, so no output depends on it | d29dd6a |
| 3 | Spec author | Lines laid out by indentation depth follow the target's first token? | No; only lines aligned to an anchor follow it | Otherwise RH3208's block leaves its line start (RH3208-003) | Keeps today's output for depth-placed lines | d29dd6a |
| 4 | Skeptic | Issue's second example (`Wrap(…)` argument with raw string) and the existing formatter tests whose targets no code fix passes (argument target; invocation in a raw-string hole) | Drop the example from the goals; the tests whose expectations this change contradicts are removed, replaced by code-fix inputs from the spec | No caller reaches these targets (AGENTS.md, Supported inputs; maintainer clarification) | Unreachable inputs are not visible output | pending |
| 5 | Skeptic | Spec defect: FMT-PIPE-018/024 state a mechanism (the indentation phase puts the target's first token back in its document column) that cannot hold for mid-line targets and would give a same-line gap two owners | Route to the spec author: restate as a result (aligned lines laid out as if the first token stood in its document column); add the join case to FMT-PIPE-017; add the skeptic's measured cases to the change doc's visible output changes | Single owner per decision (AGENTS.md phase table); change doc must list every reachable visible change | Wording/mechanism only; the visible changes go to the maintainer as output questions | pending |
| 6 | Skeptic | Widen the issue goal "does not break in front of the target" to joins too | Accept | Measured reachable join case (RH5111 first parameter after `(`) | Visible output asked as an output question | pending |
| 7 | Spec author | Accept the change doc | Status set to Accepted | Skeptic ran, its amendment is written, every visible output change has the maintainer's answer | Every listed change answered by the maintainer | this commit |

## Follow-up candidates

None yet.
