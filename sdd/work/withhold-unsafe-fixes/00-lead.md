Role: lead
Commit: 6ac812d0
Verdict: CI repair — SonarCloud quality gate failed (new reliability rating C)

# CI repair for thoenissen/Reihitsu#1599

The published PR's `sonarcloud` check failed: quality gate `new_reliability_rating` = 3 (threshold 1). Issues SonarCloud reports on the PR (API `issues/search?pullRequest=1599`):

| Severity | Rule | File:line | Message |
|---|---|---|---|
| MAJOR (bug, reliability) | S2583 | src/Reihitsu.Analyzer.CodeFixes/Core/DocumentOrderFixAllProvider.cs:148 | Change this condition so that it does not always evaluate to 'True' — `codeAction == null` after `RegisterCodeFixesAsync`; the variable is assigned inside the registration callback, which Sonar does not see |
| CRITICAL | S3776 | src/Reihitsu.Core/RewriteBindingUtilities.cs:100 | Cognitive Complexity 16 > 15 |
| CRITICAL | S3776 | src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs:141 | Cognitive Complexity 23 > 15 |
| CRITICAL | S3776 | src/Reihitsu.Analyzer.CodeFixes/Base/EmptyParenthesesCodeFixProviderBase{TNode}.cs:128 | Cognitive Complexity 20 > 15 |
| MINOR | S6605 | src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs:271 | Use `Exists` instead of `Any` |
| INFO | IDE0028 | src/Reihitsu.Core/RewriteBindingUtilities.cs:69 | Collection initialization can be simplified |

Decision: all are behavior-free code-quality repairs; routed to the cleaner. No suppression and no ruleset change (escalation 6 otherwise). The run's standing instruction from the maintainer (no further questions) still holds.
