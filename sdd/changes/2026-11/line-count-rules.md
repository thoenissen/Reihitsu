# Line-count rules for methods and types

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | RH0001-004, new: RH0001-007; new specs: RH2008 (RH2008-001 to RH2008-005), RH2111 (RH2111-001 to RH2111-006), shared/line-count.md (SHARED-LINES-001, SHARED-LINES-002) |

## What

Two new Design rules report a declaration whose body holds more lines of code than a limit the project configures (thoenissen/Reihitsu#353, thoenissen/Reihitsu#354):

- **RH2008 — Methods should not exceed the line limit** (Design, Warning, enabled, no fix) judges every method, constructor, finalizer, operator, conversion operator, local function, and accessor with a body, and every property and indexer with an expression body, by the lines of code of its body.
- **RH2111 — Types should not exceed the line limit** (Design, Warning, enabled, no fix) judges every class, struct, interface, record, record struct, and union declaration with a brace body by the lines of code between its braces.

A **line of code** is a line on which a character of a token of the counted body lies (SHARED-LINES-001): the signature or header, the outer braces, blank lines, comment and documentation-comment lines, preprocessor directive lines (`#region` included), and code a conditional directive excludes are not counted. A declaration is reported when its count is greater than the limit (SHARED-LINES-002). Local functions and lambdas count toward the member that holds them, and nested types toward the type that holds them; a local function and a nested type are also judged on their own. Each part of a partial type is judged on its own.

The limits live in a new `design` section of `reihitsu.json`, `maximumMethodLines` and `maximumTypeLines`, each a whole number from 1 to 2147483647, validated by RH0001 (RH0001-007). Without a valid limit a rule reports nothing.

## Why

Method and type size are reviewed by feel today, inconsistently. A configurable limit makes the size visible in every build and gives a team one threshold to point at. Both rules share one counting definition, so a project's two limits mean the same kind of line.

The configuration goes into `reihitsu.json`, the one place where Reihitsu's own settings live: RH0001 validates it and reports a typo or an invalid value, while `.editorconfig` is where the severity of each rule is set (RH0001's non-goals; `documentation/rules/general-notes.md`). The issues proposed an `.editorconfig` key (`dotnet_diagnostic.RH####.<option>`); no Reihitsu rule reads one today, and a mistyped key there is silently ignored.

How long a method or type may grow is a project's own decision, and no default suits every project, so both rules are inactive until the limit is configured, as RH4009 is inactive without its namespace list.

## Visible output changes

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
}
```

```csharp
// before: not reported (RH0001 reports "Unknown configuration section 'design'." in reihitsu.json)
```

```csharp
// after: RH2008 on `M`, "This member has 4 lines of code, which exceeds the limit of 3."
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
}
```

The same input without `reihitsu.json`, or without `maximumMethodLines`, is not reported, before and after.

**2. RH2111 — a type whose body is over the limit is reported.** With `reihitsu.json`:

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
```

```csharp
// before: not reported (RH0001 reports "Unknown configuration section 'design'.")
```

```csharp
// after: RH2111 on the first part of `C` and on `Outer`, "This type has 3 lines of code, which exceeds the limit of 2."
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
```

**3. RH0001 — the `design` section is accepted and validated.**

```json
{ "design": { "maximumMethodLines": "50", "maximumTypeLines": 0, "maximumFileLines": 900 } }
```

Before: "Unknown configuration section 'design'." on `"design"`.

After: "The 'design.maximumMethodLines' setting must be a whole number from 1 to 2147483647." on `"50"`, the same problem for `maximumTypeLines` on `0`, and "Unknown configuration setting 'design.maximumFileLines'." on `"maximumFileLines"`. `{ "design": { "maximumMethodLines": 50, "maximumTypeLines": 500 } }` reports nothing; before, it reported the unknown section.

**4. RH8402 — a configuration with a valid `design` section no longer silences the copyright rule.** RH8402 reports nothing while RH0001 finds any problem in the configuration (RH8402-001). Today a `design` section is such a problem, so a project that adds one beside its `copyright` section turns RH8402 off; after this change a valid `design` section is no problem, and RH8402 checks the files as it does without the section.

**5. Rule pages and the package README.** `documentation/rules/RH2008.md` and `documentation/rules/RH2111.md` are new, each with a `Configuration` section that shows the setting, and the package README lists both rules under Design.

## Decisions

### The limits are settings of `reihitsu.json`, in a `design` section

Reihitsu reads its own settings only from `reihitsu.json`, sectioned by category (`copyright`, `naming`); `design` matches the category of both rules. RH0001 validates the new settings like the existing ones. Pending the maintainer's answer.

### Both rules are inactive until a limit is configured

No default limit; see *Why*. Pending the maintainer's answer.

### What a line of code is

A line holding a character of a token of the counted body (SHARED-LINES-001), stated by the syntax tree's own split between tokens and trivia, so every kind of comment, directive, and excluded code is left out by one cause rather than by a list. A token that runs over several lines, such as a verbatim or raw string, counts on each line it covers, because each is a line a reader reads as code.

### Nested functions and types count toward their container, and are also judged on their own

A reader of the outer declaration reads what is nested in it, and moving code into a local function or a nested type would otherwise shorten the outer declaration without making it any shorter to read. Pending the maintainer's answer.

### Each part of a partial type is judged on its own

As thoenissen/Reihitsu#354 proposes: a reader opens one part, and a sum across files would report a part with no place to act on. SHARED-DIAG-004 already leaves verdicts on the members one part holds to each part.

### RH2111 judges unions too, and leaves enums out

thoenissen/Reihitsu#354 names class, struct, record, record struct, and interface. A C# 15 union with a body holds members like a struct and is a type declaration for the other design rules ([union-analyzer-rules.md](union-analyzer-rules.md)); an enum body is a list of named values, not members a reader works through. Pending the maintainer's answer for the union.

### RH2008 judges accessors one by one and properties with an expression body as a getter

thoenissen/Reihitsu#353 names property accessors. Each accessor is a separate body and is judged on its own, not summed per property; an expression-bodied property or indexer is a `get` accessor written without the keyword, so it is judged the same way. Lambdas and anonymous methods are not judged on their own; their lines count toward the member.

## Alternatives considered

- **`.editorconfig` options (`dotnet_diagnostic.RH2008.max_lines = 50`)**, as the issues propose — rejected: no Reihitsu setting lives there, RH0001 could not validate it, and a mistyped key is ignored without a word. It would allow a different limit per folder, which no issue asks for.
- **Default limits that apply without configuration** — rejected: every project that updates the package would get new warnings for a threshold it never chose, and `TreatWarningsAsErrors` builds would break.
- **Counting every line of the body, comments and blank lines included** — rejected: it punishes documentation and readable spacing, and thoenissen/Reihitsu#353 and thoenissen/Reihitsu#354 ask to leave them out.
- **Leaving nested types and local functions out of the container's count** — rejected: moving code into them would satisfy the limit without making the container shorter to read.
- **Summing the parts of a partial type** — rejected; see *Decisions*.
- **One rule with two settings** — rejected: a method and a type are different constructs, and a project may want to enable or silence one without the other.
