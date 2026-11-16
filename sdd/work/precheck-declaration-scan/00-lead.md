# Lead — precheck-declaration-scan

- Issue: thoenissen/Reihitsu#1672 — `agent-filed` (goals are proposals).
- Slug: `precheck-declaration-scan`
- Branch: `claude/kind-galileo-cpsux6` (session-designated); draft PR thoenissen/Reihitsu#1797.
- Lane: **bug fix**. Reason: the issue asks for an internal efficiency change in shipped source (`src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs`) with no visible output change and no change doc; the lane check classifies shipped source without a change doc as bug fix.
- Roles run: skeptic (premise + measurement), test implementer, implementer, cleaner, spec checker, auditor, validator.
- Roles skipped: spec author — no spec clarification expected (the issue states no output differs); re-added if the skeptic finds a spec gap.
- Maintainer clarifications: none.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | Proceed or close the issue as not worth it? | Proceed with the issue's goal (first/last token of each value computed once per declaration) | Cheap, few lines; the skeptic measured large gains on deep spines on the code-fix registration path (e.g. 560 ms to 1.5 ms for a 1000-link chain), negligible on real code (0.03 % of formatting) | Time only; skeptic confirmed identical verdicts on all 1912 `src/` files and every generated input | 222170d |
| 2 | skeptic | Also remove the nested-declaration re-walk and the per-token `values.Find`? | No, keep the run to the issue's goal | Neither is the issue's goal; the gain is small on real code | Internal | 222170d |
| 3 | skeptic | Issue goals: "removes most of it" and the nested-lambda benchmark | Adjusted: the PR justifies the change with the deep-spine / code-fix-registration numbers and states the gain on real code is negligible | Skeptic's measurement | No output involved | – |

## Follow-up candidates

1. `PrecheckPhase.FindTriviaInFrontOfComma` is quadratic in list length (`GetPreviousToken`/`GetNextToken` per comma); whole cost on code-fix registration for long lists (RH5303, RH5305). Source: 01-skeptic.md.
2. Later formatter phases slow sharply on long expressions and lists (1000-link chain 183 s, 1000-branch `?:` 109 s, 2000-term concatenation 30 s, 20 000-element array 144 s); phase not traced. Source: 01-skeptic.md.
3. `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs(483,29)` raises analyzer warning S1125 in the Debug build that `scripts/format.sh` runs (Release build is clean); exists on `main`. Source: 04-cleaner.md.

## State

- Audited SHA: 8fde9c4 (base main 372daee); audit 06-auditor.md: one finding (test does not reach first-token path).
- Last step: spec checker (05, consistent); auditor (06, 1 finding).
- Repair round 1 worklist: auditor finding #1 (PR-introduced test gap) -> test implementer.
- Next: test implementer repair, then cleaner, spec checker, auditor (repair only), validator.
