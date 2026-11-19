# This repository judges its own code by the line-count rules

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH2008-001, RH2111-001, SHARED-LINES-002 (example only) |
| Depends on | [Line-count rules for methods and types](line-count-rules.md) |

## What

This repository's own code is judged by RH2008 (methods) and RH2111 (types), as every project that installs the package is (thoenissen/Reihitsu#1865), and the defaults of both rules change where that showed them poorly placed. It reverses the decision *This repository's own build turns both rules off (11A)* and replaces the default limits of the decisions *Built-in default limits apply when no limit is configured (2B, 9B)* and *Test code has defaults of 100 lines per method-like member and 1000 per type (14A)* of [Line-count rules for methods and types](line-count-rules.md):

- The shipped default limits become:

  | Setting | Rule | Code | Default before | Default after |
  |---|---|---|---|---|
  | `maximumMethodLines` | RH2008 | production | 60 | 60 |
  | `maximumTestMethodLines` | RH2008 | test | 100 | **120** |
  | `maximumTypeLines` | RH2111 | production | 500 | **1500** |
  | `maximumTestTypeLines` | RH2111 | test | 1000 | **3000** |

- `Release.ruleset` and `Debug.ruleset` no longer switch RH2008 and RH2111 off.
- The repository's own build imports the package's `buildTransitive/Reihitsu.Analyzer.targets`, so the MSBuild properties `IsTestProject` and `IsTestingPlatformApplication` are visible to the analyzers it loads directly from `binaries/`, as they are for every project that installs the package: each test project of the repository is test code under SHARED-LINES-003 and every other project is production code.
- A `reihitsu.json` at the repository root sets the repository's own limits where they are lower than the shipped defaults, and only there: `{ "design": { "maximumTypeLines": 500 } }`. The self-hosting scan reads it for every project. The build does not pass it to its analyzers yet: those in `binaries/` predate the `design` section, and their configuration check would report it (see *The build enforces the rules once `binaries/` is rebuilt*).
- The self-hosting analyzer scan judges the repository's code by both rules, each project as the build judges it: a test project as test code, every other project as production code.
- Every declaration the rules report in the repository's own code is made shorter, by restructuring that keeps its behavior; a test class over its limit is split into separate classes by the topic its tests cover, not into partial parts.
- `AnalyzerResources` holds no property per rule any more: the analyzer base finds a rule's title and message by the rule's ID.

## Why

The rules shipped with defaults that are meant to need no setup. This repository is the first code base held to them in its own build: if the defaults are well chosen, its own code meets them with a few changes; if they are not, this is where it shows, before the users of the package find it.

The analyzers of this repository's build are loaded as `Analyzer` items, not through the package, so the package's `CompilerVisibleProperty` items are not imported. Without them every test project would be judged as production code (SHARED-LINES-003: a project that loads the analyzer without the package's build files is production code).

Measured on the repository's code at the start of this change (every compilation of the self-hosting scan, test projects judged as test code), against the defaults before this change:

| Code | Rule | Default | Declarations over it | 99th percentile | Largest |
|---|---|---|---|---|---|
| production | RH2008 | 60 | 18 of 2514 | 54 | 132 |
| production | RH2111 | 500 | 5 of 878 | 436 | 812 |
| test | RH2008 | 100 | 20 of 16395 | 61 | 270 |
| test | RH2111 | 1000 | 94 of 1024 | 2899 | 6946 |

The percentiles are taken over the declarations with more than one line of code. 9 % of the test types exceeded the test-type default, with a median violation of 1545 lines of code. With the new defaults and the repository's own production-type limit of 500, the repository's code holds 18 production methods, 9 test methods, 5 production types, and 11 test types over their limits.

## Visible output changes

The defaults of RH2008 and RH2111 change; nothing else an end user sees changes. The restructured declarations of the repository keep their behavior, its `reihitsu.json` and its build are not a surface an end user sees, and the titles and messages of every rule stay as they are.

### RH2008 · Methods should not exceed the line limit — test method with 110 lines of code

Input — a method in a test project, without `reihitsu.json`, whose body holds 110 lines of code:

```csharp
[TestMethod]
public void VerifyEveryCase()
{
    // 110 lines of code
}
```

Before (`main`):

```csharp
[TestMethod]
public void {|RH2008:VerifyEveryCase|}()
{
    // 110 lines of code
}
```

"This member has 110 lines of code, which exceeds the limit of 100."

After: not reported. A test method with 121 lines of code is reported, "This member has 121 lines of code, which exceeds the limit of 120." Production methods keep the default of 60.

### RH2111 · Types should not exceed the line limit — production type with 1000 lines of code

Input — a type in a production project, without `reihitsu.json`, whose body holds 1000 lines of code:

```csharp
public class OrderService
{
    // 1000 lines of code
}
```

Before (`main`):

```csharp
public class {|RH2111:OrderService|}
{
    // 1000 lines of code
}
```

"This type has 1000 lines of code, which exceeds the limit of 500."

After: not reported. A production type with 1501 lines of code is reported, "This type has 1501 lines of code, which exceeds the limit of 1500."

### RH2111 · Types should not exceed the line limit — test class with 1500 lines of code

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

After: not reported. A test class with 3001 lines of code is reported, "This type has 3001 lines of code, which exceeds the limit of 3000."

### Rule pages

`documentation/rules/RH2008.md` states 120 as the default for test code, in its text and in its configuration example; `documentation/rules/RH2111.md` states 1500 for production code and 3000 for test code, in its text and in its configuration example.

## Decisions

### The defaults prompt a design decision in time without being a large obstacle (1A)

The maintainer set the purpose of the defaults: they make a reader think about the design of a member or a type while splitting it is still cheap, without standing in the way of code that reads well. On that measure the production-method default of 60 holds. The test-method default rises to 120; the production-type default to 1500; the test-type default to 3000, where some test classes still need restructuring. The rules, their specs (RH2008-001, RH2111-001, and the example of SHARED-LINES-002), and their rule pages state the new defaults.

### A project sets lower limits in its own configuration, never higher ones here (2A)

This replaces the earlier scope of thoenissen/Reihitsu#1865, which allowed no repository-specific limit. A project that wants its code held smaller than the defaults sets a lower limit in `reihitsu.json`, and this repository does so: its production types are held to 500 lines of code, the old default. The repository sets a limit only where it is lower than the shipped default, and never one above it — a higher limit would hide whether the defaults fit a real code base, which is what this change tests. Methods (60 and 120) and test types (3000) follow the defaults, so the repository's `reihitsu.json` names only `maximumTypeLines`.

### `AnalyzerResources` holds no property per rule (3C)

`AnalyzerResources` held two one-line properties per rule and grew with every rule, so as one type it exceeded the repository's production-type limit and would exceed any fixed limit in time. Partial parts are not an option. Instead, the per-rule properties are dropped:

- `AnalyzerResources` keeps the `ResourceManager` and no property per rule.
- The analyzer base derives the resource names of a rule from its diagnostic ID, `<ID>Title` and `<ID>MessageFormat`; an analyzer passes its ID and names no resource itself. Every analyzer already names exactly these two resources today, so no title or message changes.
- The base publishes the two texts as the properties `Title` and `MessageFormat` of each analyzer, so a unit test checks for every analyzer that its title and its message exist and are the resources its ID names, and the analyzer tests take a message from the analyzer's property, through `MessageFormat` of their test base, instead of `AnalyzerResources.RH####MessageFormat`.
- `CodeFixResources` keeps its properties; it stays under the limit.

`.claude/commands/add-resource-texts.md` describes the new procedure. No spec states which resource holds a rule's texts: the texts an end user sees do not change, and the naming is a convention of the implementation, checked by the unit test.

### Test projects are recognized as the package recognizes them

The repository makes the same two properties compiler-visible that the package's build files declare, so the rules tell test code from production code here exactly as they do for a user of the package. It does so by importing the package's own `buildTransitive/Reihitsu.Analyzer.targets`, so one file states the list. Every test project of the repository references `Microsoft.NET.Test.Sdk`, which sets `IsTestProject`; the self-hosting scan, which does not run MSBuild, recognizes a test project by that reference or by an `IsTestProject` property in the project file.

### The build enforces the rules once `binaries/` is rebuilt

The build loads the analyzers committed under `binaries/`, which predate RH2008, RH2111, and the `design` section of `reihitsu.json`. Until `binaries/` is rebuilt, the build cannot run either rule, and passing the root `reihitsu.json` to it would only make the old configuration check report the `design` section as unknown, which fails the Release build. So the build does not pass the file yet, and the self-hosting scan, which runs the analyzers built from source with the repository's `reihitsu.json`, is the check that holds the repository's code to the rules and to its production-type limit of 500. Passing the file to the build once `binaries/` knows the section is a follow-up.

## Alternatives considered

- **Keeping the defaults 60/100 and 500/1000** (1, other options) — rejected (1A): the test-type default marked 9 % of this repository's test classes, and the other defaults were weighed against the purpose the maintainer set for them.
- **No limit of the repository's own** (the earlier scope of #1865) — replaced (2A): a lower limit hides nothing about the defaults, so the repository holds its production types to 500.
- **Limits of the repository's own above the defaults** (the issue's proposal) — rejected (2A): it would hide whether the defaults fit a real code base.
- **Splitting `AnalyzerResources` into partial parts by category** — rejected by the maintainer (3C).
- **Leaving the rules off in the build and enabling them only in the self-hosting scan** — rejected: the build and the scan would judge the code differently, and a violation would surface only in a test run. Until `binaries/` is rebuilt, the scan is the only check in practice, but the rulesets already enable both rules, so the build enforces them as soon as its analyzers know them.
- **Passing the root `reihitsu.json` to the build now** — rejected: the analyzers in `binaries/` would report its `design` section as unknown and fail the Release build, while running neither rule.
- **Splitting the test classes over the limit into partial parts by size** — rejected: it meets the type limit only mechanically; the maintainer ruled out partial parts as a way to meet it (3C).
- **Excluding single files, such as the resource accessors, through `.editorconfig`** — rejected: it would exempt code from the rules instead of meeting them.
