Role: cleaner
Commit: (this commit)
Verdict: clean

## Formatted
`scripts/format.sh` over the 14 changed C# files changed none. After my form changes it formatted `RuleDocumentationExampleTests.cs` and `TestScriptProjectSelectionTests.cs` (my edits only).

## Warnings resolved
The Release build compiles with the checked-in `binaries/` analyzers, which still use the old anchor (chain's first link). The implementer's self-hosting realignment under the last prefix link therefore failed RH5201 (error, `TreatWarningsAsErrors`) in four files. Each chain is now written so both anchors agree (a prefix of one link or none), so the build passes under the old binaries and the new formatter alike.

| Warning | Location | Form-only change |
|---|---|---|
| RH5201 | `SwitchCaseBraceRewriter.cs` (`GetDirectlyDeclaredLocalNamesOfStatement` arm) | Chain moved into new private `GetDeclaredNames(VariableDeclarationSyntax)`; same calls in same order |
| RH5201 | `RH5011...CodeFixProvider.cs` `FindLineEndingTrivia` | Local `trailingTrivia`, chain starts on it |
| RH5201 | `RuleDocumentationExampleTests.cs` `OptedOutDocumentsStillFailVerification` | Local `optedOutIds`; assertions and inputs untouched |
| RH5201 | `TestScriptProjectSelectionTests.cs` `SonarCloudLoopProjects` | Local `names`; assertions and inputs untouched |

## Unresolved
_None._ Note: once `binaries/` is rebuilt (bump-version), the original two-link prefix layouts would also be valid.

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- formatter `SwitchCase` filter: Passed 168. analyzer `RH5011|RuleDocumentationExampleTests`: Passed 71. architecture `TestScriptProjectSelectionTests`: Passed 7.

## Follow-up candidates
_None._
