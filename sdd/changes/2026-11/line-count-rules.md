# Line-count rules for methods and types

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | RH0001-004, new: RH0001-007; new specs: RH2008 (RH2008-001 to RH2008-005), RH2111 (RH2111-001 to RH2111-006), shared/line-count.md (SHARED-LINES-001 to SHARED-LINES-003) |

## What

Two new Design rules report a declaration whose body holds more lines of code than its limit (thoenissen/Reihitsu#353, thoenissen/Reihitsu#354):

- **RH2008 — Methods should not exceed the line limit** (Design, Warning, enabled, no fix) judges every method, constructor, finalizer, operator, conversion operator, local function, and accessor with a body, and every property and indexer with an expression body, by the lines of code of its body.
- **RH2111 — Types should not exceed the line limit** (Design, Warning, enabled, no fix) judges every class, struct, interface, record, record struct, and union declaration with a brace body by the lines of code between its braces.

A **line of code** is a line on which a character of a token of the counted body lies (SHARED-LINES-001): the signature or header, the outer braces, blank lines, comment and documentation-comment lines, preprocessor directive lines (`#region` included), and code a conditional directive excludes are not counted; every line of a multi-line string literal is. A declaration is reported when its count is greater than its limit (SHARED-LINES-002). Local functions and lambdas count toward the member that holds them, and nested types toward the type that holds them; a local function and a nested type are also judged on their own. Each accessor is judged on its own, and each part of a partial type.

Production code and test code have separate limits. Test code is the code of a project whose MSBuild property `IsTestProject` is `true` — every project that references `Microsoft.NET.Test.Sdk` — and all other code is production code (SHARED-LINES-003). The package makes the property visible to its analyzers.

| Setting in `design` | Rule | Code | Default |
|---|---|---|---|
| `maximumMethodLines` | RH2008 | production | 60 |
| `maximumTestMethodLines` | RH2008 | test | 100 |
| `maximumTypeLines` | RH2111 | production | 500 |
| `maximumTestTypeLines` | RH2111 | test | 1000 |

The settings live in a new `design` section of `reihitsu.json`, each a whole number from 1 to 2147483647, validated by RH0001 (RH0001-007). A setting that is not configured, or that RH0001 reports as invalid, keeps its default.

This repository's own build turns both rules off, in `Release.ruleset` and `Debug.ruleset`, and the self-hosting scans of `Reihitsu.SelfHosting.Test` ignore them; a follow-up decides the repository's own limits or the refactors its outliers need.

## Why

Method and type size are reviewed by feel today, inconsistently. A limit makes the size visible in every build and gives a team one threshold to point at. Both rules share one counting definition, so the limits mean the same kind of line.

The defaults make the rules useful without setup and are set where few declarations reach. Measured on this repository's own code under SHARED-LINES-001, production methods have a 99th percentile of 42 lines of code, and 19 of 4668 exceed 60; production types have a 99th percentile of 437, and 5 of 873 exceed 500. Test code runs longer — test methods arrange their inputs and expected results in their bodies, and a test class holds one test per case — so it has defaults of its own.

The configuration goes into `reihitsu.json`, the one place where Reihitsu's own settings live: RH0001 validates it and reports a typo or an invalid value, while `.editorconfig` is where the severity of each rule is set (RH0001's non-goals; `documentation/rules/general-notes.md`). The issues proposed an `.editorconfig` key (`dotnet_diagnostic.RH####.<option>`); no Reihitsu rule reads one today, and a mistyped key there is silently ignored.

## Visible output changes

Every example below is production code without a `reihitsu.json`, unless it says otherwise.

**1. RH2008 — a member whose body is over the limit is reported.** With `reihitsu.json`:

```json
{ "design": { "maximumMethodLines": 3 } }
```

```csharp
// input
class C
{
    private int _value;

    void M()
    {
        // Comments, blank lines, and braces are not counted.
        _value = 1;

        _value = 2;
        _value = 3;
        _value = 4;
    }

    int P
    {
        get
        {
            return _value;
        }
    }

    public static C operator +(C left, C right)
    {
        left._value += right._value;
        left._value += right._value;
        left._value += right._value;
        return left;
    }

    int Q
    {
        get
        {
            _value++;
            _value++;
            _value++;
            return _value;
        }
    }
}
```

```csharp
// before: not reported (RH0001 reports "Unknown configuration section 'design'." in reihitsu.json)
```

```csharp
// after: RH2008 on `M`, on `+`, and on `get` of `Q`, each "This member has 4 lines of code, which exceeds the limit of 3."
class C
{
    private int _value;

    void {|RH2008:M|}()
    {
        // Comments, blank lines, and braces are not counted.
        _value = 1;

        _value = 2;
        _value = 3;
        _value = 4;
    }

    int P
    {
        get
        {
            return _value;
        }
    }

    public static C operator {|RH2008:+|}(C left, C right)
    {
        left._value += right._value;
        left._value += right._value;
        left._value += right._value;
        return left;
    }

    int Q
    {
        {|RH2008:get|}
        {
            _value++;
            _value++;
            _value++;
            return _value;
        }
    }
}
```

The same input without `reihitsu.json`, or without `maximumMethodLines`, is not reported, before and after: no member reaches the default of 60. A member with 61 lines of code in its body is reported without any configuration, "This member has 61 lines of code, which exceeds the limit of 60."; before, it was not reported.

**2. RH2008 — the accessors of one property are judged one by one, and a lambda in an initializer is not judged.** With `{ "design": { "maximumMethodLines": 4 } }`:

```csharp
// input, before, and after: not reported — each accessor holds three lines of code
class C
{
    private int _value;

    public int Value
    {
        get
        {
            var result = _value;
            result += 1;
            return result;
        }
        set
        {
            var input = value;
            input -= 1;
            _value = input;
        }
    }
}
```

With `{ "design": { "maximumMethodLines": 2 } }`:

```csharp
// input, before, and after: not reported by RH2008; the lambda's lines count toward `C` for RH2111
using System;

class C
{
    private static readonly Func<int, int> Compute = value =>
    {
        var result = value;
        result += 1;
        return result;
    };
}
```

**3. RH2008 — every line of a multi-line string literal counts.** With `{ "design": { "maximumMethodLines": 4 } }`:

```csharp
// before: not reported
// after: RH2008 on `M`, "This member has 5 lines of code, which exceeds the limit of 4."
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

**4. RH2111 — a type whose body is over the limit is reported.** With `reihitsu.json`:

```json
{ "design": { "maximumTypeLines": 2 } }
```

```csharp
// input
partial class C
{
    #region Fields

    public int A;
    public int B;
    public int D;

    #endregion // Fields
}

partial class C
{
    public int E;
}

class Outer
{
    class Inner
    {
    }
}

union U(int, string)
{
    public int A => 1;
    public int B => 2;
    public int C => 3;
}

enum E
{
    A,
    B,
    C,
}
```

```csharp
// before: not reported (RH0001 reports "Unknown configuration section 'design'.")
```

```csharp
// after: RH2111 on the first part of `C`, on `Outer`, and on `U`, each "This type has 3 lines of code, which exceeds the limit of 2."; the enum is not judged
partial class {|RH2111:C|}
{
    #region Fields

    public int A;
    public int B;
    public int D;

    #endregion // Fields
}

partial class C
{
    public int E;
}

class {|RH2111:Outer|}
{
    class Inner
    {
    }
}

union {|RH2111:U|}(int, string)
{
    public int A => 1;
    public int B => 2;
    public int C => 3;
}

enum E
{
    A,
    B,
    C,
}
```

Without `reihitsu.json`, a type with 501 lines of code in its body is reported, "This type has 501 lines of code, which exceeds the limit of 500."; before, it was not reported.

**5. RH2008 and RH2111 — test code has limits of its own.** With `{ "design": { "maximumMethodLines": 2, "maximumTestMethodLines": 3 } }`:

```csharp
// input
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

```csharp
// before: not reported, in any project
```

```csharp
// after, in a production project: RH2008 on `M`, "This member has 3 lines of code, which exceeds the limit of 2."
class C
{
    void {|RH2008:M|}()
    {
        M();
        M();
        M();
    }
}
```

```csharp
// after, in a test project (it references Microsoft.NET.Test.Sdk, or sets IsTestProject to true): not reported
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

Without `reihitsu.json`, a test method with 80 lines of code in its body is not reported (default 100), and the same method in a production project is reported (default 60). With `{ "design": { "maximumMethodLines": 40 } }` alone, the test limit stays 100.

**6. RH0001 — the `design` section is accepted and validated.**

```json
{ "design": { "maximumMethodLines": 50.0, "maximumTypeLines": "500", "maximumTestTypeLines": 0, "maximumFileLines": 900 } }
```

Before: "Unknown configuration section 'design'." on `"design"`.

After: "The 'design.maximumMethodLines' setting must be a whole number from 1 to 2147483647." on `50.0`, the same problem for `maximumTypeLines` on `"500"` and for `maximumTestTypeLines` on `0`, and "Unknown configuration setting 'design.maximumFileLines'." on `"maximumFileLines"`. The three invalid settings keep their defaults, so RH2008 judges production methods against 60 and RH2111 production types against 500 and test types against 1000. `{ "design": { "maximumMethodLines": 50, "maximumTestMethodLines": 80, "maximumTypeLines": 400, "maximumTestTypeLines": 800 } }` reports nothing; before, it reported the unknown section.

**7. RH8402 — a configuration with a valid `design` section no longer silences the copyright rule.** RH8402 reports nothing while RH0001 finds any problem in the configuration (RH8402-001). Today a `design` section is such a problem, so a project that adds one beside its `copyright` section turns RH8402 off; after this change a valid `design` section is no problem, and RH8402 checks the files as it does without the section.

**8. Rule pages and the package README.** `documentation/rules/RH2008.md` and `documentation/rules/RH2111.md` are new, each stating its defaults for production and test code and with a `Configuration` section that shows both settings, and the package README lists both rules under Design.

## Decisions

The maintainer answered the output questions of this run in three batches; the numbers are the question numbers of the run.

### The limits are settings of `reihitsu.json`, in a `design` section (1A)

Reihitsu reads its own settings only from `reihitsu.json`, sectioned by category (`copyright`, `naming`); `design` matches the category of both rules. RH0001 validates the new settings like the existing ones. A valid `design` section next to a `copyright` section no longer turns RH8402 off (visible change 7).

### Built-in default limits apply when no limit is configured (2B, 9B)

Both rules are active without configuration. The defaults are 60 lines of code per method-like member and 500 per type in production code, chosen against the measurement in *Why*. A setting RH0001 reports as invalid, and every setting of a file that cannot be parsed, keeps its default rather than turning the rule off (SHARED-LINES-002); RH0001 reports the setting. Pending batch 3 for the fallback of an invalid setting.

### Production code and test code have separate limits

The maintainer asked for it with batch 2. Test code is recognized by the project, through the MSBuild property `IsTestProject` (SHARED-LINES-003), which `Microsoft.NET.Test.Sdk` sets and `dotnet test` reads; the package makes it visible to the analyzers. Each rule has a second setting, `maximumTestMethodLines` and `maximumTestTypeLines`, with defaults of 100 and 1000, and each setting keeps its own default when only the other one is configured. Pending batch 3 for the recognition, the setting names, the test defaults, and the independence of the settings.

### Nested functions and types count toward their container, and are also judged on their own (3A)

A reader of the outer declaration reads what is nested in it, and moving code into a local function or a nested type would otherwise shorten the outer declaration without making it any shorter to read.

### Rule IDs, titles, and messages (4A)

RH2008 "Methods should not exceed the line limit", in the member-level design range; RH2111 "Types should not exceed the line limit", in the range of the rules about type declarations and their nesting. The messages state the count and the limit: "This member has {0} lines of code, which exceeds the limit of {1}." and "This type has {0} lines of code, which exceeds the limit of {1}.". A member without an identifier is marked by its operator token, the `operator` keyword of a conversion operator, `this` of an indexer, or its accessor keyword. The message is the same in test code; the limit it names is the test limit.

### RH2111 judges unions too, and leaves enums out (5A)

thoenissen/Reihitsu#354 names class, struct, record, record struct, and interface. A C# 15 union with a body holds members like a struct and is a type declaration for the other design rules ([union-analyzer-rules.md](union-analyzer-rules.md)); an enum body is a list of named values, not members a reader works through.

### RH2008 judges accessors one by one and properties with an expression body as a getter (6A)

thoenissen/Reihitsu#353 names property accessors. Each accessor is a separate body and is judged on its own, not summed per property; an expression-bodied property or indexer is a `get` accessor written without the keyword, so it is judged the same way.

### A lambda is not judged on its own, also in an initializer (7A)

Lambdas and anonymous methods count toward the member that holds them; a lambda in a field or property initializer is not judged by RH2008 and counts toward its type for RH2111.

### Strict validation of the settings (8A)

A setting is a JSON whole number from 1 to 2147483647, written without a fraction or an exponent; `50.0`, `5e1`, and `"50"` are reported, and so is an unknown setting.

### What a line of code is (10A)

A line holding a character of a token of the counted body (SHARED-LINES-001), stated by the syntax tree's own split between tokens and trivia, so every kind of comment, directive, and excluded code is left out by one cause rather than by a list. A token that runs over several lines, such as a verbatim or raw string, counts on each line it covers, a blank line inside it included, because each is a line a reader reads as code. A line is a line as the compiler splits the text, so every line ending counts alike. The count follows the text as it is laid out, so formatting can raise it; a project sets its limits against code laid out in its own style.

### Each part of a partial type is judged on its own

As thoenissen/Reihitsu#354 proposes: a reader opens one part, and a sum across files would report a part with no place to act on. SHARED-DIAG-004 already leaves verdicts on the members one part holds to each part.

### This repository's own build turns both rules off (11A)

`Release.ruleset` and `Debug.ruleset` set both rules to `None`, and the self-hosting scans of `Reihitsu.SelfHosting.Test` ignore both rules. A follow-up issue decides the repository's own limits, or the refactors its outliers need. Nothing a user of the package sees changes by this.

## Alternatives considered

- **`.editorconfig` options (`dotnet_diagnostic.RH2008.max_lines = 50`)**, as the issues propose — rejected (1A): no Reihitsu setting lives there, RH0001 could not validate it, and a mistyped key is ignored without a word.
- **Both rules inactive until a limit is configured** — rejected (2B): the rules would do nothing for most projects, and the defaults sit where few declarations reach.
- **The rules off by default, with defaults once enabled** — rejected (2B): it takes two steps to turn a rule on.
- **One limit for production and test code** — rejected on the maintainer's request: test code runs longer for reasons a production limit does not account for.
- **Test code recognized by a test attribute on the type or method, or by a list of test framework references** — rejected: an attribute leaves a test project's helpers and base classes as production code, and a list of frameworks misses one.
- **Counting every line of the body, comments and blank lines included** — rejected: it punishes documentation and readable spacing, and thoenissen/Reihitsu#353 and thoenissen/Reihitsu#354 ask to leave them out.
- **Leaving the lines of multi-line string literals out** — rejected (10A): they are lines a reader reads as part of the code.
- **Leaving nested types and local functions out of the container's count** — rejected (3A): moving code into them would satisfy the limit without making the container shorter to read.
- **Summing the accessors of a property** — rejected (6A); see *Decisions*.
- **Judging a lambda in an initializer on its own** — rejected (7A); see *Decisions*.
- **Accepting any JSON number with a whole value** — rejected (8A): one way to write a limit.
- **Summing the parts of a partial type** — rejected; see *Decisions*.
- **One rule with two settings** — rejected: a method and a type are different constructs, and a project may want to enable or silence one without the other.
