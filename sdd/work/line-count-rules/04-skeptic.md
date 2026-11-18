Role: skeptic
Commit: 37d7350
Verdict: #353: output question; #354: output question

The prompt carried no intended fix, suspected cause, or hoped-for verdict. This round challenges the amendment `git diff b7f1ce2 37d7350` (03-spec-author.md): the change doc, `shared/line-count.md`, RH2008, RH2111, RH0001, and the two rule pages. The issues' premise was settled in 02-skeptic.md and is not repeated.

## thoenissen/Reihitsu#353

### Premise

The amendment carries out every recorded answer, with one exception in substance — the test-code recognition it designed for the maintainer's production/test request misses a common class of test projects (below).

Answers checked against the diff:

| Answer | Where it lands | Result |
|---|---|---|
| 1A | RH0001-004/007, change doc changes 6 and 7 | carried out |
| 2B, 9B | SHARED-LINES-002, RH2008-001 (60), RH2111-001 (500), rule pages, change doc *Why* | carried out |
| 3A | RH2008-003, RH2111 (unchanged this round) | carried out |
| 4A | RH2008-005, change doc *Decisions* | carried out |
| 5A | change doc change 4 (union reported, enum not) | carried out |
| 6A | RH2008-004 example, change doc change 2 | carried out |
| 7A | RH2008 non-goal and RH2008-003 example, change doc change 2 | carried out |
| 8A | RH0001-007 (`50.0` added to the invalid forms) | carried out |
| 10A | SHARED-LINES-001 (raw-string example, five lines: `return """`, `first`, blank, `second`, `""";` — recounted) | carried out |
| 11A amended | change doc *What* and *Decisions*; rulesets and self-hosting left to later roles, as no spec governs them (`rg -il self-?hosting sdd/specs` is empty) | carried out as text; the code is the implementer's |
| Lead decisions 1, 2 | SHARED-LINES-001 | carried out |
| Lead decision 3 | code owner still `—`, deferred to the implementer with a reason (`EveryCodeOwnerNamesAnExistingType`) | acceptable |

Loader behavior the amendment relies on, checked in code: `ConfigurationManager.GetConfiguration` (`src/Reihitsu.Analyzer/Data/ConfigurationManager.cs:51-91`) returns the parsed `Configuration` together with the validation errors, and no `Configuration` for an empty, unreadable, or unparseable file. So "a problem elsewhere does not keep a valid setting from taking effect" and "an unparseable or empty file keeps the defaults" (SHARED-LINES-002) are reachable without changing the loader's shape. RH0001-003 covers the empty file, so SHARED-LINES-002's reference to it is right.

Measurement figures quoted in the change doc and in questions 14, re-derived from `scratchpad/linecount/linecounts.csv` with a script: test methods over 100: 20 in 14 files; over 150: 5; over 60: 171 in 86 files; test types over 1000: 89; over 2000: 28; over 500: 239; production methods over 60: 19 in 15 files; production types over 500: 5. All agree. All eight test projects of the CSV reference `Microsoft.NET.Test.Sdk` (`rg -c` over `src/*Test*/*.csproj`, one hit each).

**How test code is recognized — the claimed mechanism, checked by building.** SHARED-LINES-003 and question 12 A rest on `IsTestProject`, made visible with `CompilerVisibleProperty`. I built five scratch projects (outside the repository, since deleted) with SDK 11.0.100, each with a `Directory.Build.targets` holding `<CompilerVisibleProperty Include="IsTestProject" />` and `<CompilerVisibleProperty Include="IsTestingPlatformApplication" />` — the content the package's `buildTransitive/Reihitsu.Analyzer.targets` would add — ran `dotnet build`, and read the generated `obj/**/*.GeneratedMSBuildEditorConfig.editorconfig`:

| Project | `build_property.IsTestProject` | `build_property.IsTestingPlatformApplication` |
|---|---|---|
| `Microsoft.NET.Sdk` + `Microsoft.NET.Test.Sdk` 18.12.0 + `MSTest.TestAdapter`/`MSTest.TestFramework` 4.5.3 | `true` | `false` |
| `<Project Sdk="MSTest.Sdk/4.5.3">`, defaults | *(empty)* | `true` |
| `Microsoft.NET.Sdk`, `OutputType` `Exe`, `xunit.v3` 4.0.3 only | *(empty)* | `true` |
| `Microsoft.NET.Sdk` + `MSTest.TestFramework` 4.5.3 only (a helper library, question 12's `Shop.Testing`) | *(empty)* | *(empty)* |
| plain `Microsoft.NET.Sdk` library | *(empty)* | *(empty)* |

Package content behind it: `Microsoft.NET.Test.Sdk` sets `<IsTestProject>true</IsTestProject>` in its `build/*/Microsoft.NET.Test.Sdk.props`; `TUnit.Engine` sets both `IsTestProject` and `IsTestingPlatformApplication`; `MSTest.Sdk` 4.5.3 defaults to `UseVSTest=false`, imports `Runner/Runner.targets`, references `Microsoft.NET.Test.Sdk` only from `VSTest/VSTest.targets`, and sets `IsTestApplication`/`IsTestingPlatformApplication` but never `IsTestProject`; `xunit.v3.core.mtp-v2` 4.0.3 sets `TestProject`, `XunitTestProject`, and `IsTestingPlatformApplication`, not `IsTestProject`. The .NET SDK itself only reads `IsTestProject` (`Microsoft.NET.Sdk.targets:128,1323`, `Microsoft.TestPlatform.targets`), and its MTP mode of `dotnet test` keys on `IsTestingPlatformApplication`.

Consequences:

- The package mechanism works as SHARED-LINES-003 claims for VSTest-based test projects: the property reaches the generated global config as `build_property.IsTestProject = true`. The package id is `Reihitsu.Analyzer` and the targets file is `buildTransitive/Reihitsu.Analyzer.targets`, packed by `Reihitsu.Analyzer.Package.csproj`; that file already works for `RazorComponent`, so NuGet imports it.
- **An MSTest.Sdk project with its default runner, and an xUnit v3 project without `Microsoft.NET.Test.Sdk`, are production code under the drafted reading.** Their tests would be held to 60/500. The drafted question 12 does not show this case, and its *Why A* ("the build's own definition of a test project, which `dotnet test` uses too"; "needs no list of test frameworks, which would miss one") is wrong for projects on Microsoft.Testing.Platform. SHARED-LINES-003's `Why:` states the same. Question 12 is re-drafted below with this case and a fourth option.
- Question 12's other shown outputs hold: `Shop.Testing` (MSTest.TestFramework only) carries neither property, so it is production code under A and D, as drafted.

**Verifier tests.** The repository's verifier already passes a global analyzer config: `test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", "is_global = true\n…"))` in `RH7101DoNotCombineFieldsAnalyzerTests.cs:1776` and `RH5417…AnalyzerTests.cs:1053` (Microsoft.CodeAnalysis.Testing 1.1.4). So the route 03-spec-author.md names exists. I did not run an analyzer that reads `build_property.IsTestProject` from `GlobalOptions` (none exists), so that this key reaches `AnalyzerConfigOptionsProvider.GlobalOptions` in a verifier test is not established by me. It is self-checking if the test implementer writes both directions of SHARED-LINES-003's example: test code at production limit 2 and test limit 3 *not* reported (fails if the key is ignored), and the same code without the config reported.

**Where the property does not reach the analyzer.** The analyzer DLL loaded without the package's build assets — `IncludeAssets` without `build`/`buildTransitive`, or a direct `<Analyzer Include>` — sees no property, and every project is production code (SHARED-LINES-003 says so: "not visible to the analyzer"). This repository is such a case: `Directory.Build.props:53-56` loads `binaries/Reihitsu.Analyzer.dll` directly. Irrelevant while 11A turns both rules off; it matters for the 11A follow-up (below).

### Mechanism and candidates

_N/A_ — a new rule, no defect.

### Breaking inputs

For the test-code distinction, the inputs the test implementer must cover (on top of 03-spec-author.md's list):

- Test code by the global config `build_property.IsTestProject = true`, and the values `True` and `TRUE` (case-insensitive), `false`, empty, and absent → production.
- If the maintainer picks D (question 12 below): `build_property.IsTestingPlatformApplication = true` alone → test; `IsTestProject = true` with `IsTestingPlatformApplication = false` → test; both absent → production.
- FC-003 boundaries at each of the four defaults (60/61, 100/101, 500/501, 1000/1001) and at a configured limit, in both kinds of code.
- Independence (question 15): a configured production setting in a test project leaves the test default; a configured test setting in a production project leaves the production default.
- Invalid settings keep defaults (question 16): `"40"`, `0`, `50.0`, `5e1`, `null`, empty file, `{ } junk` — each with a 61-line production body (reported) and a 60-line one (not reported).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzer RH2008, RH2111 | must change (new): read the global analyzer option(s) for test code |
| Shared counter / limit resolution | must change (new, lead decision 3) |
| RH0001 / `ConfigurationManager` | must change: four `design` settings |
| Package `buildTransitive/Reihitsu.Analyzer.targets` | must change: `CompilerVisibleProperty` for `IsTestProject` (and `IsTestingPlatformApplication` under D); no test covers it |
| Rule pages RH2008, RH2111 | must change; the test-project sentence depends on question 12 |
| `Release.ruleset`, `Debug.ruleset`, `Reihitsu.SelfHosting.Test` | must change (11A) |
| Code fix, Fix All, formatter, CLI | not involved |

### Refuse option

No input class here to refuse; the rules have no fix. A narrower option for the production/test request — one limit for all code — was the state before the maintainer's request and is not on the table. No unsupported shape is in play (no formatter or fix involvement).

### Output questions

Question 12 needs re-drafting: its options do not show the Microsoft.Testing.Platform case, and its *Why A* states something the build contradicts. Re-drafted in full below; A, B, C keep the spec author's meaning, D is new, and the input gains one project. Questions 13 and 15 are correct as drafted, in form and in output. Questions 14 and 16 are correct in output; their headings lack the rule titles the form requires — use `RH2008 · Methods should not exceed the line limit / RH2111 · Types should not exceed the line limit — …` — and question 14's *Why* should drop its internal reference "(both rules are off here, 11A)".

**12 · RH2008 · Methods should not exceed the line limit / RH2111 · Types should not exceed the line limit — which code is test code**

Input, with `{ "design": { "maximumMethodLines": 2, "maximumTestMethodLines": 3 } }` in three projects: `Shop.Test` references `Microsoft.NET.Test.Sdk` and MSTest; `Billing.Test` is an MSTest project written as `<Project Sdk="MSTest.Sdk">`, which runs its tests on Microsoft.Testing.Platform and does not reference `Microsoft.NET.Test.Sdk` (the same holds for an xUnit v3 project without it); `Shop.Testing` is a shared helper library that references `MSTest.TestFramework` only:
```csharp
// Shop.Test/OrderTests.cs, and the same file in Billing.Test/OrderTests.cs
[TestClass]
public class OrderTests
{
    [TestMethod]
    public void Total()
    {
        var order = OrderBuilder.Create();
        order.Add(2);
        Assert.AreEqual(2, order.Total);
    }
}

// Shop.Test/OrderBuilder.cs, and the same file in Shop.Testing/OrderBuilder.cs
public static class OrderBuilder
{
    public static Order Create()
    {
        var order = new Order();
        order.Customer = "Test";
        return order;
    }
}
```

Before (`main`): not reported.

After, A: test code is every file of a project whose MSBuild property `IsTestProject` is `true`, which `Microsoft.NET.Test.Sdk` sets. Nothing is reported in `Shop.Test`. `Billing.Test` is production code (limit 2) unless it sets `IsTestProject` itself, and `Shop.Testing` is production code:
```csharp
// Billing.Test/OrderTests.cs
[TestClass]
public class OrderTests
{
    [TestMethod]
    public void {|RH2008:Total|}()
    {
        var order = OrderBuilder.Create();
        order.Add(2);
        Assert.AreEqual(2, order.Total);
    }
}

// Shop.Testing/OrderBuilder.cs
public static class OrderBuilder
{
    public static Order {|RH2008:Create|}()
    {
        var order = new Order();
        order.Customer = "Test";
        return order;
    }
}
```
Rule pages: "Code in a test project — a project that references `Microsoft.NET.Test.Sdk` or sets the MSBuild property `IsTestProject` to `true` — counts as test code."

After, B: test code is every file of a project that references a known test framework (MSTest, xUnit, NUnit, TUnit). Nothing is reported in any of the three projects; a test framework missing from the list makes its tests production code.

After, C: test code is a type that holds a method with a test attribute (`[TestMethod]`, `[Fact]`, `[Test]`, …). `OrderTests` is not reported in either project; `OrderBuilder` is production code in both `Shop.Test` and `Shop.Testing`:
```csharp
// Shop.Test/OrderBuilder.cs and Shop.Testing/OrderBuilder.cs
public static class OrderBuilder
{
    public static Order {|RH2008:Create|}()
    {
        var order = new Order();
        order.Customer = "Test";
        return order;
    }
}
```

After, D (recommended): as A, and a project that runs its tests on Microsoft.Testing.Platform — MSBuild property `IsTestingPlatformApplication` is `true`, which `MSTest.Sdk`, xUnit v3, and TUnit set — is test code too. Nothing is reported in `Shop.Test` or `Billing.Test`; `Shop.Testing` is production code:
```csharp
// Shop.Testing/OrderBuilder.cs
public static class OrderBuilder
{
    public static Order {|RH2008:Create|}()
    {
        var order = new Order();
        order.Customer = "Test";
        return order;
    }
}
```
Rule pages: "Code in a test project — a project that references `Microsoft.NET.Test.Sdk`, runs its tests on Microsoft.Testing.Platform (such as an `MSTest.Sdk` or xUnit v3 project), or sets the MSBuild property `IsTestProject` to `true` — counts as test code."

Why D: it keeps A's project-level definition, which keeps a test project's helpers with its tests and needs no list of frameworks, and also covers the test projects that run on Microsoft.Testing.Platform, which do not set `IsTestProject` and would otherwise be held to the production limits.

### Internal choices

- SHARED-LINES-003's `Why:` and the change doc's *Decisions* section ("which `Microsoft.NET.Test.Sdk` sets and `dotnet test` reads") must be corrected under any answer to question 12: `dotnet test` on Microsoft.Testing.Platform keys on `IsTestingPlatformApplication`, and `MSTest.Sdk`/xUnit v3 projects do not set `IsTestProject`. Under A the `Why:` should say that such a project is production code unless it sets `IsTestProject`; under D it states both properties. Recommendation: correct it with the answer.
- Under D, a project that references `Microsoft.Testing.Platform` without being a test application — an extension library for the platform — gets `IsTestingPlatformApplication` `true` by default (`Microsoft.Testing.Platform.targets`, `buildMultiTargeting`, imported from `build/`) and counts as test code. It is test tooling, and rare; no change needed. Recommendation: accept, mention nowhere.
- SHARED-LINES-002 lists the cases that keep a default as "no file, no section/setting, RH0001 reports the value, file cannot be parsed". An unreadable file ("The configuration file could not be read.", `ConfigurationManager.cs:65-70`) is none of them by name but yields no `Configuration` either. Recommendation: phrase the list as "whenever the setting has no valid value", with the listed cases as examples; wording only, no output difference.

### Issue goals

Unchanged from 02-skeptic.md; the maintainer's answers have replaced the issues' configuration proposals.

### Architecture findings

_None._

### Follow-up candidates

- Extend the 11A follow-up (this repository's own limits): when the rules are turned on here, `Directory.Build.props` loads the analyzer DLLs directly (`:53-56`) and imports no package targets, so every test project of this repository would be judged as production code unless `Directory.Build.props` also declares the `CompilerVisibleProperty` item(s). The self-hosting scans pass no global options at all and would need the same.

## thoenissen/Reihitsu#354

Everything under #353 applies unchanged with `maximumTypeLines`/`maximumTestTypeLines` and the defaults 500/1000; question 12's recognition decides RH2111's test code the same way. RH2111-001's added example sentence ("without a `reihitsu.json`, `C` and `S` there are not reported") is consistent with RH2111-002. Change doc change 4 (union reported, enum not, three lines each) recounted: correct. Nothing of this issue's own.

## Whole run

- Fixtures: five scratch projects built with `~/.dotnet/dotnet build` (SDK 11.0.100) in the scratchpad, packages restored from nuget.org; results in the table under #353. Downloaded package contents (`TUnit.Engine` 1.86.6, `xunit.v3.core.mtp-v2` 4.0.3, `MSTest.Sdk` 4.5.3) were read with `rg`, not executed. All deleted afterwards.
- No repository file other than this handover was edited; no test was run.
