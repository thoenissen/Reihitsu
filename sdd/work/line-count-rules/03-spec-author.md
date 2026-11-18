Role: spec author
Commit: 71d5828
Verdict: #353: spec written; #354: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. This round amends the specs to the maintainer's answers of batches 1 and 2 (1A, 2B, 3A, 4A, 5A, 6A, 7A, 8A, 9B, 10A, 11A), carries out the lead's decisions 1–3, and designs the production/test distinction the maintainer asked for. The specs are written on the recommended options of the batch-3 questions below; the change doc stays `Status: Draft` until batch 3 is answered.

Files changed:

- `sdd/changes/2026-11/line-count-rules.md` — rewritten: defaults, test code, the skeptic's amendments as visible changes (operator and accessor spans in change 1, accessors and initializer lambda in change 2, raw string in change 3, union and enum in change 4, `50.0` and `"500"` in change 6), every answer recorded under *Decisions* with its number, *Alternatives* updated.
- `sdd/specs/shared/line-count.md` — SHARED-LINES-001 clarified (decisions 1 and 2, answer 10A), SHARED-LINES-002 changed (2B: defaults instead of inactive; separate test limits), SHARED-LINES-003 added (test code).
- `sdd/specs/analyzer/RH2008.md`, `sdd/specs/analyzer/RH2111.md` — RH2008-001 and RH2111-001 changed (two settings, defaults); intro sentences name the defaults; examples are production code unless stated; RH2008 gains the 6A and 7A examples.
- `sdd/specs/analyzer/RH0001.md` — RH0001-007 changed (four settings, `50.0` example); `Uses` gains SHARED-LINES-003.
- `sdd/specs/requirement-ids.txt` — SHARED-LINES-003.
- `documentation/rules/RH2008.md`, `documentation/rules/RH2111.md` — "inactive until configured" replaced by the defaults and the test-project sentence; `Configuration` shows both settings; multi-line string literals named as counted.

## thoenissen/Reihitsu#353

### Changed requirements

- SHARED-LINES-001 — A line of code holds a character of a token of the counted span — clarified (the compiler's line split, every line ending alike; the count follows the layout; a blank line inside a literal counts; new raw-string example)
- SHARED-LINES-002 — A declaration is reported when its count is greater than its limit — changed (a setting that is absent, invalid, or in an unparseable or missing file keeps its default; production and test settings are independent)
- SHARED-LINES-003 — Test code is the code of a test project — added (shared with #354)
- RH2008-001 — The limits are `design.maximumMethodLines` and `design.maximumTestMethodLines` — changed (defaults 60 and 100)
- RH2008-003 — Nested functions count toward the member that holds them — clarified (example: an initializer lambda is not judged, 7A)
- RH2008-004 — A member without a body is not reported — clarified (example: two accessors judged one by one, 6A)
- RH0001-007 — The `design` section holds the line limits — changed (four settings; `50.0` named as invalid)

### Examples for the tests

SHARED-LINES-001 (both examples, the raw-string one included), SHARED-LINES-002 (every configuration listed below its example, production and test project), SHARED-LINES-003, RH2008-003 (the initializer example), RH2008-004 (the accessor example), RH0001-007. The earlier list of 01-spec-author.md stands for the requirements this round did not touch.

Every C# example of the four changed spec and change-doc files (25 blocks, markup removed, each in its own namespace) compiles as one net11.0 library with `LangVersion` preview in the scratchpad. None was run through an analyzer, because neither rule exists yet.

Facts the test implementer and implementer need:

- **Test code in a test.** The analyzer reads `build_property.IsTestProject` from the global analyzer options (`AnalyzerConfigOptionsProvider.GlobalOptions`). A test passes it as a global analyzer config, an additional `.globalconfig` source holding `is_global = true` and `build_property.IsTestProject = true`. Without it every test runs as production code. No Reihitsu analyzer reads analyzer config options today (skeptic, *Premise*), so this is new plumbing.
- **The package.** SHARED-LINES-003 requires the package to make the property visible: `src/Reihitsu.Analyzer.Package/buildTransitive/Reihitsu.Analyzer.targets` needs `<ItemGroup><CompilerVisibleProperty Include="IsTestProject" /></ItemGroup>`. That file has no code owner type; no spec test checks it, so the auditor should look at it by hand.
- **Defaults and examples.** Every spec example sets a small limit, so it is reported or not regardless of the defaults. A default case needs a body of 61 (production method), 101 (test method), 501 (production type), or 1001 (test type) lines of code, and one at the default that is not reported (FC-003); generate the lines in the test rather than writing them out.
- **Invalid settings.** An invalid setting and an unparseable or empty file keep the defaults (SHARED-LINES-002) — test `"2"`, `0`, `50.0`, `{ } junk`, and an empty file each with a 61-line body (reported at 60).
- `RuleDocumentationExampleTests` still needs both rules opted out: the page examples rely on `"maximumMethodLines": 3` and `"maximumTypeLines": 4` in a `reihitsu.json`, which that test does not pass, and they use undeclared types.

### Output questions

Batch 3, in the form of `AGENTS.md` *Output questions*, numbered after batch 2. They all follow from the maintainer's request to distinguish production and test code, and from 2B. The specs are written on the recommended options. Questions 12 to 16 apply to both issues; the examples use RH2008, and RH2111 behaves the same with `maximumTypeLines` and `maximumTestTypeLines`.

**12 · RH2008 · Methods should not exceed the line limit / RH2111 · Types should not exceed the line limit — which code is test code**

Input, with `{ "design": { "maximumMethodLines": 2, "maximumTestMethodLines": 3 } }`. Project `Shop.Test` references `Microsoft.NET.Test.Sdk` and MSTest; project `Shop.Testing` is a shared helper library that references `MSTest.TestFramework` but not `Microsoft.NET.Test.Sdk`:
```csharp
// Shop.Test/OrderTests.cs
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

// Shop.Test/OrderBuilder.cs, and the same type in Shop.Testing/OrderBuilder.cs
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

After, A (recommended): test code is every file of a project whose MSBuild property `IsTestProject` is `true`; `Microsoft.NET.Test.Sdk` sets it, and a project can set it itself. Both types in `Shop.Test` are test code (limit 3) and not reported; `OrderBuilder` in `Shop.Testing` is production code (limit 2):
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

After, B: test code is every file of a project that references a known test framework (MSTest, xUnit, NUnit, TUnit). Nothing is reported in either project, but a test framework missing from the list makes its tests production code.

After, C: test code is a type that holds a method with a test attribute (`[TestMethod]`, `[Fact]`, `[Test]`, …). `OrderTests` is not reported; `OrderBuilder` is production code in both projects:
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

Why A: it is the build's own definition of a test project, which `dotnet test` uses too, it keeps a test project's helpers with its tests, and it needs no list of frameworks or attributes; a helper library like `Shop.Testing` that wants the test limits sets `IsTestProject` to `true` itself.

**13 · RH0001 · reihitsu.json configuration must be valid — the names of the test settings**

Input, `reihitsu.json` written for each option:
```json
{ "design": { "maximumMethodLines": 60, "maximumTestMethodLines": 100, "maximumTypeLines": 500, "maximumTestTypeLines": 1000 } }
```

Before (`main`): "Unknown configuration section 'design'." on `"design"`.

After, A (recommended): the four settings sit side by side in `design`, as in the input; RH0001 reports nothing. A mistake reads "The 'design.maximumTestMethodLines' setting must be a whole number from 1 to 2147483647.".

After, B: the test settings sit in a nested object with the same names as the production ones:
```json
{ "design": { "maximumMethodLines": 60, "maximumTypeLines": 500, "test": { "maximumMethodLines": 100, "maximumTypeLines": 1000 } } }
```
RH0001 reports nothing for this file, and reports `maximumTestMethodLines` as "Unknown configuration setting 'design.maximumTestMethodLines'."; a mistake reads "The 'design.test.maximumMethodLines' setting must be a whole number from 1 to 2147483647.".

Why A: one level, like every other Reihitsu setting, and each name says on its own what it limits.

**14 · RH2008 / RH2111 — the default limits of test code**

Input: no `reihitsu.json`; a test project holds a test method whose body has 90 lines of code and a test class whose body has 1200.

Before (`main`): not reported.

After, A (recommended): defaults of 100 lines per test method and 1000 per test type. The method is not reported; the class is, "This type has 1200 lines of code, which exceeds the limit of 1000.". In this repository's own tests, 20 of 16724 test methods (in 14 files) and 89 of 1013 test types would be reported.
```csharp
[TestClass]
public class {|RH2111:OrderTests|}
{
    // 1200 lines of code, among them a test method of 90 lines
}
```

After, B: defaults of 150 and 2000. Neither is reported. In this repository, 5 test methods and 28 test types would be reported.

After, C: the production defaults, 60 and 500, also for test code. Both are reported, "This member has 90 lines of code, which exceeds the limit of 60." and "This type has 1200 lines of code, which exceeds the limit of 500.". In this repository, 171 test methods (in 86 files) and 239 test types would be reported.

Why A: it gives test code room for its written-out inputs and expectations — 100 is above the 99th percentile of this repository's test methods (61) — while still marking the outliers; this repository's analyzer test classes hold hundreds of code inputs each and are not a typical test class (both rules are off here, 11A).

**15 · RH2008 / RH2111 — a project configures only the production limit**

Input, with `{ "design": { "maximumMethodLines": 40 } }`, in a test project:
```csharp
[TestMethod]
public void Total()
{
    // 70 lines of code
}
```

Before (`main`): not reported.

After, A (recommended): each setting keeps its own default; the test limit stays 100, and the method is not reported.

After, B: a test setting that is not configured follows a configured production setting, so the test limit is 40:
```csharp
[TestMethod]
public void {|RH2008:Total|}()
{
    // 70 lines of code
}
```
"This member has 70 lines of code, which exceeds the limit of 40."

Why A: every setting means one thing and has one documented default, so changing the production limit never moves the test limit; a project that wants one limit for both sets both.

**16 · RH2008 / RH2111 — a limit that RH0001 reports as invalid**

Input, with `{ "design": { "maximumMethodLines": "40" } }`, in a production project:
```csharp
public void Process()
{
    // 70 lines of code
}
```

Before (`main`): not reported; RH0001 reports "Unknown configuration section 'design'.".

After, A (recommended): RH0001 reports "The 'design.maximumMethodLines' setting must be a whole number from 1 to 2147483647." on `"40"`, and the setting keeps its default of 60. The same holds for every setting when `reihitsu.json` is empty or not valid JSON.
```csharp
public void {|RH2008:Process|}()
{
    // 70 lines of code
}
```
"This member has 70 lines of code, which exceeds the limit of 60."

After, B: RH0001 reports the setting as in A, and RH2008 reports nothing until the setting is fixed; an empty or invalid `reihitsu.json` turns both rules off.

Why A: the rule stays at its documented default instead of going silent on a typo, and RH0001 already points at the mistake.

### Internal choices

- Test code is defined once, in the shared spec (SHARED-LINES-003), not in each rule spec; RH2008-001 and RH2111-001 name their two settings and defaults. Alternative: a requirement per rule.
- The package's `CompilerVisibleProperty` is stated in SHARED-LINES-003, since the rules depend on it and no spec covers the package's build files. Alternative: a new package spec for one sentence.
- RH0001-007 was changed in place rather than retired and replaced, since it is unshipped and was written in this run.
- RH0001's `Uses` row now lists SHARED-LINES-003, which RH0001-007 references; the architecture test requires it.
- `shared/line-count.md` still lists no code owner (`—`). Lead decision 3 makes the one shared counter its owner; the implementer adds the type's name to the spec header and to `sdd/specs/README.md` when it exists, since naming a type that does not exist fails `EveryCodeOwnerNamesAnExistingType`. The limit resolution (production or test setting, default) belongs with the counter, so both rules share it too.
- The diagnostic message is the same in test code; the limit it names is the test limit. Alternative: "This test member has …", a new message the maintainer did not ask for.
- No validation that a test limit is at least the production limit (non-goal of SHARED-LINES; stated in RH0001-007).
- Rule pages name `Microsoft.NET.Test.Sdk` and `IsTestProject`, because the user needs to know which projects get the test limits; they name no analyzer option or MSBuild item.

### Follow-up candidates

_None_ new. The 11A follow-up (this repository's own limits or refactors) is already in `00-lead.md`.

## thoenissen/Reihitsu#354

### Changed requirements

- RH2111-001 — The limits are `design.maximumTypeLines` and `design.maximumTestTypeLines` — changed (defaults 500 and 1000)
- SHARED-LINES-001, SHARED-LINES-002, SHARED-LINES-003, RH0001-007 — see #353

### Examples for the tests

RH2111-001 (default cases: 500 and 501 lines in production code, 1000 and 1001 in test code), and the SHARED-LINES cases written for `maximumTypeLines` and `maximumTestTypeLines`. The union and enum example of change doc change 4 seeds a test of RH2111-002.

### Output questions

Questions 12 to 16 under #353 apply to this issue as well; none is this issue's own.

### Internal choices

As under #353.

### Follow-up candidates

_None._

## Whole run

- **Answer 11A, for later roles (not edited by this role):**
  - `Release.ruleset` lists every rule: add `<Rule Id="RH2008" Action="None" />` and `<Rule Id="RH2111" Action="None" />` at their sorted places. `Debug.ruleset` lists only some rules (116 entries, one of them `RH08402`); add both there too, as the maintainer asked.
  - This repository's own build loads the analyzers from `binaries/Reihitsu.Analyzer.dll` (`Directory.Build.props`), not from the package, so the rulesets take effect when those binaries are next updated; until then the build does not run the new rules at all.
  - `Reihitsu.SelfHosting.Test` discovers every analyzer type through `AnalyzerCatalog.DiscoverAnalyzerTypes` and has no exclusion list today. Both analyzers need an explicit exclusion there with its reason (turned off for this repository until its own limits are decided), and a test that names the excluded IDs so the exclusion stays deliberate. With the defaults (2B), the scan would otherwise report 19 production methods and 5 production types; the scan passes no `build_property.IsTestProject`, so it would also judge the repository's test projects against the production defaults.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"`: after adding SHARED-LINES-003 to RH0001's `Uses`, the remaining failures are the three expected until the analyzers exist — `AnalyzerSpecHeaderMatchesItsRule` (RH2008, RH2111), `EveryCodeOwnerNamesAnExistingType` (the two analyzer types), and `EveryRequirementOfAnEnforcedSpecIsCovered` (now including SHARED-LINES-003). The other 45 spec tests pass.
- Measurement used for question 14: the per-unit CSV named in the prompt; the test group there is every project whose name ends in `.Test` or `Tests`, and each of those eight projects references `Microsoft.NET.Test.Sdk`, so it matches SHARED-LINES-003 for this repository.
