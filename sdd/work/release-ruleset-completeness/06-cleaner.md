Role: cleaner
Commit: see git log
Verdict: clean

## Formatted
_None._ Comment-only edit.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| Auditor finding 1 (FC-031) | `src/Reihitsu.SelfHosting.Test/Repository/RulesetTests.cs`, summaries of `ReleaseRulesetListsEveryShippedRule` and `ReleaseRulesetReportsRuleAsError` | Reworded: the first now says every shipped rule's Release action is written out explicitly, independent of default severity and `TreatWarningsAsErrors`; the second says the ruleset sets those rules' action to error. No code changed. |

## Unresolved
_None._ Finding 2 (PR description premise) is the lead's.

## Checks
TEXT-ONLY PROOF: PASS — base=b7c14cbb head=worktree — 2 changed file(s) (1 non-compiled, 1 C# comment/layout-only); xml-doc impact: 1 file(s)

## Follow-up candidates
_None._
