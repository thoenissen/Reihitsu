# This repository judges its own code by the line-count rules

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | none — repository build change; RH2111-001 only if the maintainer changes the default test limit (*Visible output changes*) |
| Depends on | [Line-count rules for methods and types](line-count-rules.md) |

## What

This repository's own code is judged by RH2008 (methods) and RH2111 (types) with the rules' default limits, as every project that installs the package without configuration is (thoenissen/Reihitsu#1865). It reverses the decision *This repository's own build turns both rules off (11A)* of [Line-count rules for methods and types](line-count-rules.md):

- `Release.ruleset` and `Debug.ruleset` no longer switch RH2008 and RH2111 off.
- The repository's own build makes the MSBuild properties `IsTestProject` and `IsTestingPlatformApplication` visible to the analyzers it loads directly from `binaries/`, as the package's build files do for every other project, so each test project of the repository is test code under SHARED-LINES-003 and every other project is production code.
- The self-hosting analyzer scan judges the repository's code by both rules, each project as the build judges it: a test project as test code, every other project as production code.
- No `reihitsu.json` entry or build setting overrides a default limit.
- Every declaration the rules report in the repository's own code is made shorter, by restructuring that keeps its behavior.

## Why

The rules shipped with defaults that are meant to need no setup and to sit where few declarations reach. This repository is the first code base held to them in its own build: if the defaults are well chosen, its own code meets them with a few changes; if they are not, this is where it shows, before the users of the package find it. A repository-specific limit would hide exactly that, so the repository uses the defaults.

The analyzers of this repository's build are loaded as `Analyzer` items, not through the package, so the package's `CompilerVisibleProperty` items are not imported. Without them every test project would be judged as production code (SHARED-LINES-003: a project that loads the analyzer without the package's build files is production code).

Measured on the repository's code at the start of this change (every compilation of the self-hosting scan, test projects judged as test code):

| Code | Rule | Default | Declarations over it | 99th percentile | Largest |
|---|---|---|---|---|---|
| production | RH2008 | 60 | 18 of 2514 | 54 | 132 |
| production | RH2111 | 500 | 5 of 878 | 436 | 812 |
| test | RH2008 | 100 | 20 of 16395 | 61 | 270 |
| test | RH2111 | 1000 | 94 of 1024 | 2899 | 6946 |

The percentiles are taken over the declarations with more than one line of code. Three defaults sit where the rule's own reasoning puts them — a few declarations over the limit, near the 99th percentile or above it. The test-type default does not: 9 % of the test types exceed it, and its median violation holds 1545 lines of code.

## Visible output changes

None in the change as stated in *What*: no rule, code fix, formatter output, rule page, or CLI behavior changes. The restructured declarations of the repository keep their behavior, and the repository's own build and scans are not a surface an end user sees.

One change is proposed and the maintainer's to decide, because the measurement above contradicts the reasoning behind the default test-type limit of RH2111 (RH2111-001). It is asked as an output question; until it is answered, nothing of it is written into a spec or a rule page.

Input — a test class in a test project, without `reihitsu.json`, whose body holds 1500 lines of code:

```csharp
[TestClass]
public class ParenthesesAnalyzerTests
{
    // 1500 lines of code
}
```

Before (`main`):

```csharp
[TestClass]
public class {|RH2111:ParenthesesAnalyzerTests|}
{
    // 1500 lines of code
}
```

"This type has 1500 lines of code, which exceeds the limit of 1000."

After, with a default test-type limit of 3000: not reported. A test class with 3001 lines of code is reported, "This type has 3001 lines of code, which exceeds the limit of 3000.", and `documentation/rules/RH2111.md` states 3000 as the default for test code, in its text and in its configuration example.

## Decisions

### The repository uses the rules' default limits

The maintainer decided the scope of thoenissen/Reihitsu#1865: both rules on, with their defaults, and no repository-specific limit; every violation in the repository's own code is fixed. A default that proves poorly chosen on this code base is brought to the maintainer with proposed new defaults rather than worked around.

### Test projects are recognized as the package recognizes them

The repository makes the same two properties compiler-visible that the package's build files declare, so the rules tell test code from production code here exactly as they do for a user of the package. Every test project of the repository references `Microsoft.NET.Test.Sdk`, which sets `IsTestProject`.

### The default test-type limit of RH2111

Pending the maintainer's answer to the output question above.

## Alternatives considered

- **Limits of the repository's own in `reihitsu.json`** (the issue's proposal) — rejected by the maintainer: it would hide whether the defaults fit a real code base.
- **Leaving the rules off in the build and enabling them only in the self-hosting scan** — rejected: the build and the scan would judge the code differently, and a violation would surface only in a test run.
- **Excluding single files, such as the generated-style resource accessors, through `.editorconfig`** — rejected: it is a repository-specific exception, which the maintainer's scope rules out.
