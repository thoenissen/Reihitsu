# Open questions — switch case braces (FMT-SWITCH)

## Q1 — Bracing a section breaks a local that a later section uses

Input:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                int y = 1;
                F(y);
                break;

            case 2:
                y = 2;
                F(y);
                break;
        }
    }
}
```

Output:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                {
                    int y = 1;

                    F(y);
                }
                break;

            case 2:
                {
                    y = 2;
                    F(y);
                }
                break;
        }
    }
}
```

Phases: `BlankLine, SwitchCaseBrace, Indentation, Cleanup` (idempotent).

Why it looks wrong: In C# a local declared in one switch section is in scope in every section of the switch, so the input compiles and `case 2` assigns the `y` declared in `case 1`. After brace insertion `y` is scoped to the first block and `case 2` no longer compiles (CS0103). This violates FMT-PIPE-003 (program meaning unchanged). The phase has guards for directives (FMT-SWITCH-010) and cross-section `goto` targets (FMT-SWITCH-011) but none for a local declared directly in a section and referenced from another section. Expected: leave the switch alone (as FMT-SWITCH-011 does) when a directly owned local declaration (or a pattern variable/`out var` at section level) is referenced outside its section.

Touches: FMT-PIPE-003; would become a new guard requirement next to FMT-SWITCH-011. Not written into the spec.

## Q2 — The brace-removal path is unreachable through the pipeline

`SwitchCaseBraceRewriter.RemoveBraces` (with its `BraceTokensCarryCommentsOrDirectives` guard) removes the block of a section when no section of the switch is multi-line, and the class summary says "If all sections are single-line, braces are removed from all sections". Through the full pipeline the path never fires: `LineBreakPhase` puts the braces of every block on their own lines first, so a section with a block is always multi-line and the switch is braced instead.

Input:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                { A(); }
                break;

            case 2:
                B();
                break;
        }
    }
}
```

Output:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                {
                    A();
                }
                break;

            case 2:
                {
                    B();
                }
                break;
        }
    }
}
```

Phases: `LineBreak, SwitchCaseBrace, Indentation, Cleanup` (idempotent). Only the unit tests that call the phase in isolation (`RemoveBracesRemovesBracesWhenNoCommentsAtRisk`, `RemoveBracesPreservesStatementComment`, `RemoveBracesKeepsBracesWhenBraceCarriesComment`) reach it.

Question: is brace removal intended behavior that the pipeline order defeats, or dead code? The spec specifies the observable behavior (FMT-SWITCH-008: an existing block braces the switch) and lists removal as a non-goal. If removal is intended, it also needs the scoping guard from Q1 in reverse (removing a block can create duplicate locals across sections).

Touches: FMT-SWITCH-008, the Non-goals entry on brace removal.

## Q3 — A comment before the trailing `break` is not idempotent

Input:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                A();
                B();
                // before break
                break;

            case 2:
                C();
                break;
        }
    }
}
```

First pass:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                {
                    A();
                    B();
                }
                // before break
                break;

            case 2:
                {
                    C();
                }
                break;
        }
    }
}
```

Second pass inserts a blank line between `}` and `// before break`.

Phases: `BlankLine, SwitchCaseBrace, Indentation, Cleanup`; the harness reports NOT-IDEMPOTENT (FMT-PIPE-004).

Why it looks wrong: In the second pass the blank-line phase sees a comment after a closing brace and applies the comment boundary, which it could not apply in the first pass because the brace did not exist yet when it ran. Either brace insertion should emit the blank line that the blank-line phase would require (the comment then separates the block from the `break`), or the blank-line phase should treat a comment that leads the terminal `break` like the `break` itself (SHARED-BLANK-004). Which one is canonical is a decision for the maintainer.

Touches: FMT-SWITCH-007 (the comment case is not used as an example), SHARED-BLANK-004, FMT-BLANK.
