# Shared — Trailing commas

| | |
|---|---|
| ID prefix | SHARED-TRAIL |
| Status | Active |
| Code owners | `TrailingCommaAnalyzerBase<TNode>`, `TrailingCommaRemovalCodeFixProviderBase`, `TrailingCommaCodeFixHelper` |
| Uses | FMT-STRUCT-019, FMT-STRUCT-020, SHARED-DIAG-008, SHARED-DIAG-010, SHARED-FIX-003, SHARED-FIX-009, SHARED-FIX-012 |

This spec covers the rules that report a comma after the last element of a brace-delimited list: which comma is reported, when a conditional directive withholds the fix and when it does not, and what the code fix removes. Each rule that uses this spec names the kind of list it checks. The formatter's counterparts are FMT-STRUCT-019 and FMT-STRUCT-020.

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

### SHARED-TRAIL-002 — A comma in front of a conditional directive is reported, and an `#if` behind it withholds the fix

A rule that uses this spec MUST judge the elements that the analyzed build compiles (SHARED-DIAG-008): a conditional directive — `#if`, `#elif`, `#else`, or `#endif` — or code that such a directive excludes, between the comma after the last element and the closing brace MUST NOT keep the comma from being reported. When an `#if` directive lies there, whether the analyzed build reads it or it stands in code another directive excludes, the code fix MUST NOT be offered for the comma (SHARED-DIAG-008); when the only conditional directives there are `#elif`, `#else`, and `#endif`, the fix is offered (SHARED-TRAIL-004). Any other directive there, such as `#region` or `#pragma`, MUST NOT keep the comma from being reported either, and the fix stays offered (SHARED-TRAIL-003). FMT-STRUCT-019 and FMT-STRUCT-020 keep the comma in front of every conditional directive, also where the fix is offered (SHARED-TRAIL-004).

"Between the comma and the closing brace" means the comma's trailing trivia and the closing brace's leading trivia: the gap the comma would leave behind. A directive in front of the comma, such as the `#if` that opens the branch the comma stands in, is not in the gap.

Why: In the analyzed build, the element in front of the comma is the last one, so the comma is a trailing comma there; another build is judged when it is analyzed. An `#if` behind the comma opens a group whose branch a build that compiles the comma can take, and that branch can hold a further element, which then needs the comma; removing it could break that build, and finding out would mean working out that build (SHARED-FIX-009). The author decides by hand, for example by writing the comma inside the branch, in front of the element that needs it. A `#region` or `#pragma` directive never adds an element, so the comma in front of it is a trailing comma in every configuration, and removing it keeps every build intact.

Example — RH5409, in a build that does not define `DEBUG`:

```csharp
enum E
{
    A{|RH5409:,|}
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

Both commas are reported. No code fix is offered for the comma in `E`; the fix removes the comma in `F`.

Example — RH5409, in a build that does not define `X`: the comma behind `B` stands in front of an `#endif` and of a further `#if`, so it is reported and no code fix is offered:

```csharp
enum E
{
    A,
#if !X
    B{|RH5409:,|}
#endif
#if Y
    C,
#endif
}
```

Example — RH5409, in a build that does not define `X`: the `#if Y` stands in code that the `#else` excludes, and it still withholds the fix:

```csharp
enum E
{
    A,
#if !X
    B{|RH5409:,|}
#else
#if Y
    C,
#endif
#endif
}
```

Example — the comma written inside the branch, in front of the element that needs it, is not reported in a build with or without `DEBUG`:

```csharp
enum E
{
    A
#if DEBUG
    , B
#endif
}
```

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

### SHARED-TRAIL-004 — The fix removes a comma whose branch ends behind it

When conditional directives lie between the comma after the last element and the closing brace (SHARED-TRAIL-002) and each of them is an `#elif`, an `#else`, or an `#endif` — no `#if`, neither one the analyzed build reads nor one in code another directive excludes — the code fix MUST be offered and MUST remove the comma as SHARED-TRAIL-003 specifies. It MUST leave every directive, its line, and the code the directives exclude as written. The fix decides this from the kinds of the directives in the gap, never from their conditions (SHARED-FIX-009).

The formatter keeps such a comma (FMT-STRUCT-019, FMT-STRUCT-020), so formatting the document does not clear the diagnostic, and neither does a code fix that formats a node holding the list through the formatter, such as RH5303's; only this fix removes the comma. SHARED-DIAG-010 does not cover these rules: their fix removes the comma from the text itself and does not lay the code out as the formatter does.

Why: Without an `#if` in the gap, each directive there either ends a conditional group or starts another branch of a group that holds the comma. A build that compiles the comma takes every branch that holds it, so it skips each branch that an `#elif` or an `#else` behind the comma starts, and behind an `#endif` it reads on in the branch of the enclosing group that holds the comma, as the analyzed build does, where nothing but trivia and directives stands in front of the closing brace. So no build that compiles the comma compiles an element behind it: the comma is a trailing comma in each of them, removing it keeps every build intact (SHARED-FIX-003). A build that does not compile the comma reads it as excluded code, which nothing compiles, except where a string or a comment that only an excluded branch opens holds it: that is SHARED-FIX-012's decided gap, which every trailing comma the fix removes shares, with or without a directive behind it. Reading the kinds of the directives is a cheap check that needs no condition. An `#if` that stands in an excluded branch withholds the fix as well, although a build that compiles the comma does not read it: the shape is rare, and its fix stays withheld rather than the check growing for it.

Example — RH5409, in a build that defines `DEBUG`: `B` is the last member, and only an `#endif` lies between its comma and the brace:

```csharp
enum E
{
    A,
#if DEBUG
    B{|RH5409:,|}
#endif
}
```

```csharp
enum E
{
    A,
#if DEBUG
    B
#endif
}
```

The comma behind `A` is not a trailing comma in this build and is not reported; in a build without `DEBUG`, it is reported with no code fix, as SHARED-TRAIL-002 shows.

Example — RH5409, in a build that does not define `X`: the comma behind `C` stands in front of an `#endif`; the comma behind `B`, in the excluded branch, stays as written:

```csharp
enum E
{
    A,
#if X
    B,
#else
    C{|RH5409:,|}
#endif
}
```

```csharp
enum E
{
    A,
#if X
    B,
#else
    C
#endif
}
```

Example — RH5409, in a build that does not define `X`: the comma behind `B` stands in front of an `#elif`:

```csharp
enum E
{
    A,
#if !X
    B{|RH5409:,|}
#elif Y
    C,
#endif
}
```

```csharp
enum E
{
    A,
#if !X
    B
#elif Y
    C,
#endif
}
```
