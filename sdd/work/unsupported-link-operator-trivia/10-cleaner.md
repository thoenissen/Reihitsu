Role: cleaner
Commit: d74866c (the fix and this handover are in the commit that adds it)
Verdict: clean

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Task: auditor finding #2 of `08-auditor.md`.

## Formatted
_None._ `scripts/format.sh` over the run's 16 changed C# files, the new tests of 09 included, formatted 0 of 16.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

Comment fix: the summary of `FindTriviaInsideLinkOperator` in `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` said the precheck refuses "exactly the gaps the line-break phase joins". Re-derived from the code and FMT-UNSUP-015: `FluentChain.GetLinkOperatorTokens` is the one definition of the operator's tokens, and the precheck looks at every member access and member binding of its input, a superset of the links of the chains the phase normalizes. The summary now says so and drops "exactly".

## Unresolved
_None._

## Checks
- `scripts/verify-text-only.sh --base d74866c --head worktree`: TEXT-ONLY PROOF: PASS — 1 changed file(s) (0 non-compiled, 1 C# comment/layout-only); xml-doc impact: 1 file(s)
- `scripts/build.sh`: 0 Error(s), no warnings.
- No focused test run: the change is text-only.

## Follow-up candidates
_None._
