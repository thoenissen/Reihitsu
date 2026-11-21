# This repository's own code is judged by the complexity rule

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | none — RH2009-001 keeps its default of 15 |
| Depends on | [Complexity rule for methods](method-complexity-rule.md), [This repository judges its own code by the line-count rules](enable-line-count-rules.md) |

## What

This repository's own code is judged by RH2009 (cognitive complexity of a member's body, default limit 15), as every project that installs the package is (thoenissen/Reihitsu#1915). It reverses the decision *This repository's own violations are a follow-up* of [Complexity rule for methods](method-complexity-rule.md), and it confirms the default that decision called a starting point:

- The shipped default of `design.maximumMethodComplexity` stays 15, for production code and test code alike (RH2009-001 unchanged).
- The self-hosting analyzer scan no longer ignores RH2009, so it judges the repository's code by the rule like every other rule; with RH2009 gone, the scan ignores no rule.
- `Release.ruleset` sets RH2009 to `Error`, as it does RH2008 and RH2111.
- `Debug.ruleset` has no entry for RH2009, as it has none for RH2008 and RH2111: it lists Reihitsu rules only to switch them off, so a Debug build reports RH2009 at its shipped severity, Warning.
- The root `reihitsu.json` sets no complexity limit: the repository is held to the default.
- Every member RH2009 reports in the repository's own code is made less complex by restructuring that keeps its behavior — an early return, a combined condition, a nested block with a clear purpose extracted into a method with a descriptive name — never by a suppression, an `.editorconfig` exemption, or a limit of the repository's own.

## Why

The default of 15 was decided as a starting point to be tried out on this repository before 1.5.0 ships the rule. This repository is the first code base held to it; if the default is well chosen, its own code meets it with a few changes, and if it is not, this is where it shows.

Measured with the self-hosting scan at the start of this change (base `ec07467`, every compilation the scan builds, each test project as test code). The denominator is the members RH2009 judges (RH2008-002) whose body holds more than one line of code; RH2009 reports only members that nest a structure (RH2009-004).

| Code | Members judged | Complexity 11–15 | 16–20 | 21–25 | 26–30 | Above 30 | Above 15 | Highest |
|---|---|---|---|---|---|---|---|---|
| production | 2609 | 77 | 14 | 4 | 1 | 0 | 19 (0.7 %) | 29 |
| test | 16909 | 16 | 5 | 2 | 1 | 0 | 8 (0.05 %) | 27 |

Members above other limits, for comparison:

| Limit | Production members above it | Test members above it | Together |
|---|---|---|---|
| 10 | 96 | 24 | 120 |
| 12 | 48 | 13 | 61 |
| 15 (default) | 19 | 8 | 27 |
| 20 | 5 | 3 | 8 |
| 25 | 1 | 1 | 2 |

The 99th percentile of the production members is 14, just under the default; the test members stay far below it, which bears out the decision that one limit serves both kinds of code. 27 members to restructure is the size of the RH2008 roll-out, whose production-method default stayed at 60 with 18 methods over it. No member stands far out: the highest score is 29, and the members above 15 are spread over the analyzers, code fixes, formatter, Core, tooling, and test utilities rather than gathered in one component.

## Visible output changes

None — no output an end user sees changes. The default of RH2009 stays 15, its titles, message, and rule page stay as they are, and the scoring guide `documentation/cognitive-complexity.md` stays as it is. The restructured members of the repository keep their behavior; the rulesets, the self-hosting scan, and the root `reihitsu.json` are not a surface an end user sees.

Whether the default stays 15 is asked as an output question in this run; a different answer changes the default in RH2009-001, its rule page, and the scoring guide, and this section then lists that change.

## Decisions

### The default of 15 stays

Pending the maintainer's answer to the run's output question; recommended on the measurements above: 0.7 % of the production members and 0.05 % of the test members exceed it, the 99th percentile of production members lies just below it, and the 27 members over it are a restructuring of the size the line-count roll-out carried.

### The repository sets no complexity limit of its own

[This repository judges its own code by the line-count rules](enable-line-count-rules.md) (2A) lets the repository set a limit only where it is lower than the shipped default. Nothing calls for a lower one here: the repository's code is to show whether the default fits, and a lower limit would add 93 members (11–15) to restructure without telling more about the default.

### The Debug build reports the rule as a warning

`Debug.ruleset` lists Reihitsu rules only to switch them off, and leaves RH2008 and RH2111 unlisted. RH2009 follows them: a Debug build reports it at its shipped severity, Warning, which a Debug build does not treat as an error, and a Release build fails on it. The deliberate entry the issue asks for is the absence of one, which the ruleset tests check as they do for the line-count rules.

### The build enforces the rule once `binaries/` is rebuilt

The build loads the analyzers committed under `binaries/`, which predate RH2009. Until `/bump-version` rebuilds them, the `Error` action in `Release.ruleset` has no rule to act on, and the self-hosting scan, which runs the analyzers built from source, is the check that holds the repository's code to the rule. Since the repository sets no complexity limit, the root `reihitsu.json`, which the build does not pass to its analyzers yet, plays no part in it.

### Members are restructured, not exempted

A member over the limit is made less complex by a restructuring that keeps its behavior. A suppression, an `.editorconfig` exemption, or a repository limit above the default would hide whether the default fits a real code base, which is what this change tests.

## Alternatives considered

- **A default of 20** — 8 members over it instead of 27. Rejected on the recommendation: a member of complexity 16 to 20 nests decisions three and four levels deep, as the example of RH2009-001 does, and the members of that range sampled in this repository — such as a loop over tokens that holds a second loop over their trivia — hold a nested block with a purpose of its own, a candidate for extraction, rather than code that needs the room.
- **A default of 10** — 120 members over it. Rejected: it reports members whose structure is ordinary — a loop with a nested condition and a guard or two — and would make the rule a frequent obstacle instead of a prompt for a design decision.
- **Separate limits for test code** — rejected: the test members stay far below the production members, so a separate limit would buy nothing; the decision of [Complexity rule for methods](method-complexity-rule.md) holds.
- **Keeping RH2009 off in the build and on only in the self-hosting scan** — rejected: the build and the scan would judge the code differently, as the line-count roll-out found.
- **An entry `Action="Warning"` for RH2009 in `Debug.ruleset`** — rejected: it would state the shipped severity again, and the Debug ruleset lists a Reihitsu rule only to switch it off.
