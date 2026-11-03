Role: lead
Commit: 50a9d79
Verdict: lane claimed

# Run: region-directive-separator

## Issues

- thoenissen/Reihitsu#1486 — "FMT-REGION: one space separates #region and #endregion from their text". Labels: `enhancement`, `formatter`, `agent-filed` (goals are proposals). Package F1 of thoenissen/Reihitsu#1482; its blocker thoenissen/Reihitsu#1485 is closed by thoenissen/Reihitsu#1508.
- Draft PR: thoenissen/Reihitsu#1578. Branch: `claude/confident-brown-k65bv5` (the session's designated branch; see Decisions taken #1).

## Lane

Behavior change. Reason: the issue requests changed formatter output (a new FMT-REGION requirement) and requires a change doc under `sdd/changes/`.

Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped. The issue manager runs before publishing if follow-up candidates exist.

## Maintainer clarifications

None so far.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name `claude/issue-1486-<slug>` (lead.md) vs. the session's designated branch | Use `claude/confident-brown-k65bv5` | The session environment requires all pushes to that branch | Branch names are internal | 50a9d79 |
| 2 | Skeptic | Does FMT-REGION-007 apply to every `#region` line, or only to directives the phase pairs (FMT-REGION-003)? | Only to paired directives; the spec author states the pairing scope | Matches the code's ownership; keeps `main`'s output for a `#region` whose `#endregion` lies outside a formatted node | `main`'s output is kept for the unpaired case, so nothing changes there (skeptic output question 4 dissolves) | |
| 3 | Skeptic | Where does the disabled-branch qualifier live (FMT-REGION-006 vs 007)? | In FMT-REGION-007; FMT-REGION-006 stays unchanged | One decision per requirement, each with its own `Why:` | Spec structure only | |
| 4 | Skeptic | FMT-TAB-002's example still capitalizes `foo` | Keep naming FMT-REGION-001 in FMT-TAB-002's prose and `Uses` beside FMT-REGION-007 | The example relies on both rules | Spec wording only | |
| 5 | Skeptic | Architecture finding (spec level): the one-space line form is stated in FMT-REGION-001 and FMT-REGION-007 | Change the structure in this run: the line form is stated once, in FMT-REGION-007, and FMT-REGION-001 points to it | A narrow fix would leave a second statement of the rule; the change stays within the region spec | Spec wording only; the code already has one writer (`BuildRegionTrivia`) | |
| 6 | Skeptic | Change doc says code fixes are unchanged | The spec author corrects the change doc and lists the code-fix output as a visible change (output question) | The skeptic reproduced the change with RH7105/RH7104/RH8305 | Asked as output question | |

## Follow-up candidates

1. A code fix whose target's parent is the root restores the target's leading trivia after the region phase paired its `#region` with an `#endregion` inside the target, so only half the pair is rewritten (e.g. RH7104 on a top-level `partial class C` preceded by `#region    bar`: writes `#endregion // Bar`, keeps `#region    bar`). Pre-existing on `main`. — 02-skeptic.md
2. Trailing whitespace behind a directive's text is kept (`#warning x···`, and `#region Bar···` depending on output question 2). — 01-spec-author.md, 02-skeptic.md
3. Whitespace between `#` and the directive keyword (`#  region Bar`) is kept. Low priority. — 01-spec-author.md, 02-skeptic.md
