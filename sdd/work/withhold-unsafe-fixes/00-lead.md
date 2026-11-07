Role: lead
Commit: f1478042
Verdict: CI repair 3 — SonarCloud S2583 false positive persists

# CI repair 3 for thoenissen/Reihitsu#1599

SonarCloud still reports one issue (API, 1 total): MAJOR BUG S2583 at `src/Reihitsu.Analyzer.CodeFixes/Core/DocumentOrderFixAllProvider.cs:149` — `firstAction == null` after `matchingActions.FirstOrDefault()`. Sonar's intra-procedural flow analysis keeps the list empty because it does not see the registration callback add to it (repairs 1 and 2 tried a nullable local and a list).

Decision: move the registration and the selection into a separate private method that returns `Task<CodeAction>` (the first matching action, or null), so the null test in `GetFixedDocumentAsync` tests a method result Sonar cannot fold. Behavior-free; cleaner. If Sonar still flags it after this, the lead stops and reports to the maintainer (marking a false positive in SonarCloud is the maintainer's call; no suppression in code, no ruleset change).
