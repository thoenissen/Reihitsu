Role: lead
Commit: 67f4adc1
Verdict: CI repair 2 — SonarCloud quality gate still failed (new reliability rating C)

# CI repair 2 for thoenissen/Reihitsu#1599

After CI repair 1 (commit 1d71354a), SonarCloud reports one remaining issue on the PR (API `issues/search?pullRequest=1599`, 1 total):

| Severity | Type | Rule | File:line | Message |
|---|---|---|---|---|
| MAJOR | BUG | S2583 | src/Reihitsu.Analyzer.CodeFixes/Core/DocumentOrderFixAllProvider.cs:148 | Change this condition so that it does not always evaluate to 'True' — `registeredActions.Count == 0` after `RegisterCodeFixesAsync`; the list is filled inside the registration callback, which Sonar's flow analysis does not follow |

Repair 1 replaced the nullable local with a `List<CodeAction>` checked by `Count`; Sonar still assumes the list stays empty. Decision: behavior-free restructuring by the cleaner so the post-registration check does not test a value Sonar tracks as constant (no suppression, no ruleset change). Standing instruction from the maintainer (no further questions) holds.
