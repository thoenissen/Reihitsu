# Shared — Line count of a declaration

| | |
|---|---|
| ID prefix | SHARED-LINES |
| Status | Active |
| Code owners | `LineCountAnalyzerBase` |
| Uses | RH0001-001, RH0001-003, RH0001-007, SHARED-DIAG-008 |

This spec covers the rules that count the lines of code of a declaration's body and report the declaration when the count exceeds a limit: RH2008 for methods and the other members with a body, RH2111 for types. It defines what a line of code is, which code is test code, which limit applies to a declaration, and how the count is compared with it. Each rule that uses this spec states which declarations it judges, which span of each declaration it counts, which settings hold its limits, and their default values, and where its diagnostic is located.

## Non-goals

- Which declarations a rule judges and which span of them it counts — each rule's spec.
- Loading and validating the configuration file and the settings that hold the limits — RH0001 (RH0001-007).
- A code fix: making a declaration shorter is a design decision — extracting a method, splitting a type — that no fix can make.
- Any other measure of size or complexity, such as the number of members, statements, or branches.
- Telling test code from production code inside one project: a test project's helpers, base classes, and test data are test code, and a production project's code is production code, whatever it holds (SHARED-LINES-003).
- A limit per folder or per file. A project that wants a rule off for some files sets the rule's severity in `.editorconfig`.
- A check that a test limit is at least the production limit; a project may hold its tests to a stricter limit.

## Requirements

### SHARED-LINES-001 — A line of code holds a character of a token of the counted span

A rule MUST count, as the lines of code of a span, the lines of the source text on which at least one character of a token of the span lies. A line is a line of the text as the compiler splits it, so a carriage return followed by a line feed, a line feed alone, and a carriage return alone each end one line, and the same code gives the same count under each of them. A token of the span is a token of the syntax tree whose text lies inside the span; the tokens inside structured trivia — a documentation comment or a preprocessor directive — are trivia, not tokens of the span. A line MUST be counted once, however many tokens lie on it, and a token whose text runs over several lines, such as a verbatim, raw, or multi-line interpolated string literal, MUST make each line it covers a line of code, a blank line inside the literal included. Trivia MUST NOT make a line a line of code: a line that holds only whitespace, a comment, a documentation comment, a preprocessor directive, or code that a conditional directive excludes in the analyzed build is not counted. A line that holds a token and a comment is counted.

Why: The count stands for what a reader has to read as code. Blank lines, comments, and directives structure or explain the code without adding to it, and counting them would make a rule punish documentation and readable spacing. A token is code wherever a line break puts it, and a line is read once however many tokens it holds; the lines of a literal are text a reader reads as part of the code, the blank ones included. Code that a conditional directive excludes is not compiled in the analyzed build and counts as absent there (SHARED-DIAG-008); its directive is a directive line in every build. The count follows the text as it is laid out: the same code written on one line or over several gives different counts, and formatting code — splitting a field declaration, putting a block on lines of its own — can raise it. A project sets its limits against code laid out in its own style; the rules judge size, not layout.

Example — RH2008 with `{ "design": { "maximumMethodLines": 3 } }`. The body of `M` holds four lines of code: the two lines of the calls `A();`, and the two lines over which the call of `B` and its verbatim string run; the comment line, the blank line, the directive lines, and the line that the undefined symbol `FEATURE` excludes are not counted:

```csharp
class C
{
    void {|RH2008:M|}()
    {
        // Prepare.
        A();

#if FEATURE
        A();
#endif
        B(@"first
second"); // Trailing comment.
        A();
    }

    void A()
    {
    }

    void B(string text)
    {
    }
}
```

Example — the same body without the line `A();` in front of the blank line holds three lines of code and is not reported.

Example — with `{ "design": { "maximumMethodLines": 4 } }`, the body of `M` holds five lines of code: every line of the raw string literal, the blank line inside it and the lines of its quotes included:

```csharp
class C
{
    string {|RH2008:M|}()
    {
        return """
            first

            second
            """;
    }
}
```

### SHARED-LINES-002 — A declaration is reported when its count is greater than its limit

A rule MUST report a declaration whose count of lines of code is greater than the limit that applies to it, and MUST NOT report a declaration whose count equals the limit or is lower. The limit that applies is the value of the rule's setting for the kind of code the declaration lies in — its production setting in production code, its test setting in test code (SHARED-LINES-003) — and that setting's default value whenever the setting has no valid value: for example when there is no configuration file (RH0001-001), when the file holds no `design` section or the section does not hold the setting, when RH0001 reports a problem with the setting's value (RH0001-007), and when the file is empty or cannot be parsed (RH0001-003), and when it cannot be read. Each setting has a default of its own: a production setting that is configured MUST NOT change the limit of test code, and a test setting that is configured MUST NOT change the limit of production code. A problem RH0001 reports elsewhere in the configuration file MUST NOT keep a valid setting from taking effect.

Why: The limit is the largest size the project accepts, so a declaration of exactly that size is accepted. The defaults make both rules work without setup, at a size few declarations reach; a project that wants another size sets it. A setting without a valid value has nothing the rule could trust, so the rule keeps to the documented default rather than going silent, and RH0001 reports the setting or the file. A problem in another section does not change the value of a valid setting. Test code and production code are held to separate limits, each with a documented default, so a project changes one without moving the other; a project that wants one limit for both sets both settings.

Example — with `{ "design": { "maximumMethodLines": 2 } }` in a production project, `M` holds two lines of code and is not reported; `N` holds three and is reported:

```csharp
class C
{
    void M()
    {
        M();
        M();
    }

    void {|RH2008:N|}()
    {
        N();
        N();
        N();
    }
}
```

With `{ "design": { "maximumMethodLines": 2 }, "formatting": 1 }`, `N` is reported as well, although RH0001 reports the unknown section `formatting`. Without a `reihitsu.json`, with `{ "design": { } }`, with `{ "design": { "maximumMethodLines": 0 } }`, with `{ "design": { "maximumMethodLines": "2" } }`, with an empty `reihitsu.json`, with `{ } junk`, and with `{ "design": { "maximumTestMethodLines": 2 } }`, the default production limit of 60 lines applies and neither method is reported. In a test project, `{ "design": { "maximumMethodLines": 2 } }` does not apply: the default test limit of 100 lines applies, and neither method is reported; `{ "design": { "maximumTestMethodLines": 2 } }` reports `N`.

### SHARED-LINES-003 — Test code is the code of a test project

A rule MUST treat every declaration of a compilation as test code when at least one of the compilation's MSBuild properties `IsTestProject` and `IsTestingPlatformApplication` has the value `true`, compared case-insensitively, and as production code otherwise — when each of them has another value, is empty, or is not visible to the analyzer. The package MUST make both properties visible to its analyzers (`CompilerVisibleProperty`), so a rule reads them as the global analyzer options `build_property.IsTestProject` and `build_property.IsTestingPlatformApplication`.

Why: The two properties are the build's own statements that a project holds tests, for the two ways .NET runs tests. A project that runs its tests on VSTest references the test SDK `Microsoft.NET.Test.Sdk`, which sets `IsTestProject`; a project that runs its tests on Microsoft.Testing.Platform — an `MSTest.Sdk` project with its default runner, an xUnit v3 project, a TUnit project — sets `IsTestingPlatformApplication`, often without `IsTestProject`, and `dotnet test` on that platform runs the projects that carry it. Reading only `IsTestProject` would hold those test projects to the production limits. A project can set either property itself. Deciding by the project rather than by a test attribute keeps a test project's helpers, base classes, and data together with its tests, and needs no list of test frameworks or attributes. A project that is neither is production code, which is the safe side: its limits are the stricter ones by default. The analyzer sees only the properties the build passes it, so a project that loads the analyzer without the package's build files is production code.

Example — with `{ "design": { "maximumMethodLines": 2, "maximumTestMethodLines": 3 } }`, the body of `M` holds three lines of code:

```csharp
class C
{
    void M()
    {
        M();
        M();
        M();
    }
}
```

`M` is test code, its limit is 3, and it is not reported in a project that references `Microsoft.NET.Test.Sdk`, which sets `IsTestProject`; in an `MSTest.Sdk` or xUnit v3 project, which sets `IsTestingPlatformApplication`; and in a project whose file holds `<IsTestProject>true</IsTestProject>` or `<IsTestingPlatformApplication>true</IsTestingPlatformApplication>`, also when the other property is `false`. In a project with neither property, with both `false`, or that only references a test framework such as `MSTest.TestFramework`, as a shared test-helper library does, `M` is production code, its limit is 2, and it is reported as `void {|RH2008:M|}()`.
