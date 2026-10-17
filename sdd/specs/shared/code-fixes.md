# Shared — Code fixes

| | |
|---|---|
| ID prefix | SHARED-FIX |
| Status | Active |
| Code owners | — |
| Uses | — |

This spec covers what holds for every code fix of an analyzer rule, so a rule spec states only what its own fix produces and when it is withheld. It applies to every rule whose header names a code fix, without being listed under `Uses`; SHARED-FIX-002 applies to the rules whose header says `Fix All`.

## Non-goals

- What a fix produces and when it is withheld — each rule's own spec.
- Whether a fix leaves diagnostics of other rules in the code it changed, and whether a registered fix may leave the document unchanged. Both are known failure classes (`squad/failure-classes.md`), not yet specified behavior.
- How a fix lays out the code it changed — FMT-PIPE-010 for fixes that delegate to the formatter.

## Requirements

### SHARED-FIX-001 — One application corrects a diagnostic

Applying a rule's code fix to a reported diagnostic MUST correct it in one application: once the fixes have been applied one after another, at most once for each reported diagnostic, the rule MUST NOT report the code they corrected.

Why: A fix that has to be applied twice, or that recreates its own diagnostic, never reaches a clean state for the user and makes Fix All results depend on how often it runs.

Example — the RH5002 fix, applied once:

```csharp
class C
{
    void M(int a)
    {
        a++;
        {|RH5002:if|} (a > 1)
        {
        }
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        a++;

        if (a > 1)
        {
        }
    }
}
```

is not reported again.

### SHARED-FIX-002 — Fix All corrects everything in one application

A code fix that offers Fix All MUST correct every reported diagnostic of a document, a project, or a solution in one Fix All application, and the result MUST equal the result of applying the single fixes one after another.

Why: The user applies Fix All expecting one step that leaves nothing behind. Requiring the same result as the single fixes makes the outcome independent of the order in which the diagnostics are fixed.

Example — the RH2005 fix, applied with Fix All:

```csharp
public class C
{
    public string {|RH2005:first|};

    public int {|RH2005:second|};
}
```

```csharp
public class C
{
    private string first;

    private int second;
}
```
