Role: cleaner
Commit: (this commit; parent 943c458)
Verdict: clean

## Formatted
_None._ (`scripts/format.sh`: 0 of 1 changed.)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Comment fix (decision 18, auditor finding #4)
`src/Reihitsu.Formatter/Pipeline/DocumentationComments/DocumentationCommentFormattingPhase.cs`: the class summary and the summary of `RelocateOffPositionDocumentationComments` now state the FMT-BREAK-044 exception (a comment in a hole of a non-verbatim interpolated string below C# 11 stays where it is written).

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- Text-only proof (base 45c7597, the audited implementer commit): `TEXT-ONLY PROOF: PASS — base=45c7597 head=worktree — 2 changed file(s) (1 non-compiled, 1 C# comment/layout-only); xml-doc impact: 1 file(s)`.
- Focused tests not run: the change is comments only.

## Follow-up candidates
_None._
