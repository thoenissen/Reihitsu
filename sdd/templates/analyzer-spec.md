# Analyzer — RH#### <Rule title>

| | |
|---|---|
| ID prefix | RH#### |
| Status | Active |
| Category | <Analyzer, Performance, Design, Clarity, Naming, Layout, Spacing, Organization, or Documentation> |
| Severity | Warning |
| Enabled by default | Yes |
| Code fix | <Fix All, Without Fix All, or None> |
| Code owners | `<RH####…Analyzer>`, `<RH####…CodeFixProvider>` |
| Uses | <SHARED-…-NNN, FMT-…-NNN, … or —> |

<One sentence: what RH#### reports and, when it has one, what its code fix does.>

## Non-goals

- <A neighboring case another rule decides — RH####.>

## Requirements

### RH####-001 — <What is reported>

The analyzer MUST report <construct> when <condition>.

Why: <Why the construct is a problem; enough to decide an edge case the sentence does not spell out.>

Example:

```csharp
class C
{
    void M()
    {
        {|RH####:…|}
    }
}
```

### RH####-002 — <What is not reported>

The analyzer MUST NOT report <construct> when <condition>.

Why: <…>

Example:

```csharp
class C
{
}
```

is not reported.

### RH####-003 — <Where the diagnostic is>

The analyzer MUST report one diagnostic per <unit>, located on <token or span>.

Why: <…>

Example: see RH####-001.

### RH####-004 — <What the fix produces>

The code fix MUST <rewrite>.

Why: <…>

Example:

```csharp
class C
{
    {|RH####:…|}
}
```

```csharp
class C
{
    …
}
```

### RH####-005 — <When the fix is withheld>

The code fix MUST NOT be offered when <condition>. The diagnostic is still reported.

Why: <…>

Example:

```csharp
class C
{
    {|RH####:…|}
}
```

is reported, and no code fix is offered.
