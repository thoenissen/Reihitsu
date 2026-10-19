# Shared — Trailing commas

| | |
|---|---|
| ID prefix | SHARED-TRAIL |
| Status | Active |
| Code owners | `TrailingCommaAnalyzerBase<TNode>`, `TrailingCommaRemovalCodeFixProviderBase`, `TrailingCommaCodeFixHelper` |
| Uses | FMT-STRUCT-019, FMT-STRUCT-020 |

This spec covers the rules that report a comma after the last element of a brace-delimited list: which comma is reported, when a conditional directive keeps it from being reported, and what the code fix removes. Each rule that uses this spec names the kind of list it checks. The formatter's counterparts are FMT-STRUCT-019 and FMT-STRUCT-020.

## Non-goals

- Which kind of list a rule checks — each rule's spec: RH5409 for enum members, RH5410 for array initializers, RH5411 for collection initializers.
- Trailing commas in object initializers, anonymous object creations, `with` initializers, collection expressions, switch expressions, and list patterns. No rule checks them.
- The spacing around a comma — RH6002.
- How the formatter removes a trailing comma — FMT-STRUCT-019 and FMT-STRUCT-020.

## Requirements

### SHARED-TRAIL-001 — A comma after the last element is reported

A rule that uses this spec MUST report a comma that follows the last element of a list of its kind, with one diagnostic located on that comma. A comma between two elements, a list whose last element is not followed by a comma, and an empty list MUST NOT be reported.

Why: A comma promises a further element. After the last element it promises one that is not there, and the reader has to look twice to see where the list ends.

Example — RH5409, for enum members:

```csharp
enum E
{
    A,
    B{|RH5409:,|}
}

enum F
{
    A,
    B
}

enum G
{
}
```

Only the comma after `B` in `E` is reported.

### SHARED-TRAIL-002 — A comma in front of a conditional directive is not reported

A rule that uses this spec MUST NOT report the comma after the last element when a conditional directive — `#if`, `#elif`, `#else`, or `#endif` — or code that such a directive excludes lies between the comma and the closing brace. Any other directive there, such as `#region` or `#pragma`, MUST NOT keep the comma from being reported. FMT-STRUCT-019 and FMT-STRUCT-020 keep the comma in the same case.

Why: Behind a conditional directive, another build configuration can have a further element, which then needs the comma. A `#region` or `#pragma` directive never adds an element, so the comma in front of it is a trailing comma in every configuration.

Example — RH5409:

```csharp
enum E
{
    A,
#if DEBUG
    B,
#endif
}

enum F
{
    A,
    B{|RH5409:,|}
#region R
#endregion
}
```

The comma after `B` in `F` is reported; no comma in `E` is.

### SHARED-TRAIL-003 — The fix removes the comma

The code fix MUST remove the reported comma and keep every element, comment, and directive next to it on the line it is on. When the comma is the only thing on its line besides whitespace, the fix MUST remove that whole line.

Why: The comma is the only thing the diagnostic is about. A comment behind the comma describes the last element and stays with it. A line that held nothing but the comma would be left empty, so it goes with the comma.

Example — RH5409:

```csharp
enum E
{
    A,
    B{|RH5409:,|} // last
}

enum F
{
    A
    // last
    {|RH5409:,|}
}
```

```csharp
enum E
{
    A,
    B // last
}

enum F
{
    A
    // last
}
```
