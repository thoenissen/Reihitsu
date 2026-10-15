# Formatter — Switch Case Braces

| | |
|---|---|
| ID prefix | FMT-SWITCH |
| Status | Active |
| Code owners | `SwitchCaseBracePhase`, `SwitchCaseBraceRewriter` |
| Uses | FMT-PIPE-002, FMT-PIPE-003, SHARED-BLANK-004 |

This spec covers when the formatter wraps the statements of the sections of a `switch` statement in a `{ … }` block: which sections make a switch braced, what goes inside the block, and every guard that leaves a section or a whole switch as it is.

## Non-goals

- Switch expressions (`x switch { … }`): their layout belongs to FMT-BREAK and FMT-INDENT. They matter here only as content that makes a section multi-line (FMT-SWITCH-005).
- Moving a `break` that is already inside an existing block out of it. RH7501 reports that shape and its code fix moves the `break`; the formatter keeps an existing block as written (FMT-SWITCH-009).
- Blank lines between sections and inside the new blocks (FMT-BLANK, SHARED-BLANK-004), and the line breaks and indentation of the braces (FMT-BREAK, FMT-INDENT).
- Removing braces from a section. The phase contains a removal path for switches without a multi-line section, but an existing block always reaches this phase spanning several lines (FMT-SWITCH-008), so no shipped behavior removes braces.

## Requirements

### FMT-SWITCH-001 — One multi-line section braces the whole switch

When at least one section of a `switch` statement is multi-line (FMT-SWITCH-003 to FMT-SWITCH-008), the formatter MUST wrap the statements of every section of that switch in a block, so that all sections share one shape. When no section is multi-line, the formatter MUST leave every section unbraced.

Why: A switch whose sections mix braced and unbraced bodies is harder to scan than one with a uniform shape. A block also scopes the locals of a longer section to that section. Short one-statement sections read best without braces, so the switch stays compact until one section needs the block.

Example:

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
                break;

            case 2:
                C();
                break;

            default:
                D();
                break;
        }
    }
}
```

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
                break;

            case 2:
                {
                    C();
                }
                break;

            default:
                {
                    D();
                }
                break;
        }
    }
}
```

A switch whose sections each hold one single-line statement and a `break` is left unchanged:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                A();
                break;

            case 2:
                B();
                break;
        }
    }
}
```

### FMT-SWITCH-002 — The terminal `break` stays outside the block

When the formatter braces a section, a `break` that is the last statement owned directly by the section MUST be placed after the closing brace of the new block, on its own line. Every other statement, including a final `return`, `throw`, `continue`, or `goto`, MUST go inside the block.

Why: The block scopes the section's work; the `break` ends the section and is not part of that work (SHARED-BLANK-004 uses the same predicate to exempt it from the blank-line rules, and RH7501 asks for the same shape). Any other jump statement is the section's last piece of logic and belongs with the code it ends. The example under FMT-SWITCH-001 shows the `break`; here a `return` ends the section.

Example:

```csharp
class C
{
    int M(int x)
    {
        switch (x)
        {
            case 1:
                A();

                return 1;

            case 2:
                return 2;
        }

        return 0;
    }
}
```

```csharp
class C
{
    int M(int x)
    {
        switch (x)
        {
            case 1:
                {
                    A();

                    return 1;
                }

            case 2:
                {
                    return 2;
                }
        }

        return 0;
    }
}
```

### FMT-SWITCH-003 — Statements that make a section multi-line

A section MUST count as multi-line when, ignoring `break`, `return`, and `throw` statements, it holds more than one statement, or one statement that spans more than one line, or one statement followed by a final `return` or `throw` on a later line. A section MUST NOT count as multi-line merely because a `break`, `return`, or `throw` spans several lines.

Why: The block is for sections that do more than one thing or whose one thing is long. A section that only returns or throws a wrapped expression stays a single jump, and wrapping it in a block adds two lines without adding structure. FMT-SWITCH-002 shows a statement followed by `return`; FMT-SWITCH-001 shows two statements.

Example — one statement that spans two lines:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                A(1,
                  2);
                break;

            case 2:
                B();
                break;
        }
    }
}
```

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                {
                    A(1,
                      2);
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

A section whose only statement is a `return` with a wrapped argument list is not multi-line, so this switch is left unchanged:

```csharp
class C
{
    int M(int x)
    {
        switch (x)
        {
            case 1:
                A();
                break;

            case 2:
                return Compute(1,
                               2);
        }

        return 0;
    }
}
```

### FMT-SWITCH-004 — A multi-line delimited pattern in a label makes a section multi-line

A section MUST count as multi-line when one of its labels contains a property (recursive), list, or parenthesized pattern that spans more than one line. A label that spans several lines only because a combinator chain (`or`, `and`) or a `when` clause wraps MUST NOT make the section multi-line.

Why: A pattern whose braces or brackets open and close on different lines is a block-like structure in the label; a braced body below it keeps the section's start and its body visually apart. A wrapped `when` condition or `or` chain is a long expression, not a nested structure, and does not change how the body reads.

Example:

```csharp
class C
{
    void M(object o)
    {
        switch (o)
        {
            case Point
                 {
                     X: 1
                 }:
                A();
                break;

            case string s:
                B();
                break;
        }
    }
}
```

```csharp
class C
{
    void M(object o)
    {
        switch (o)
        {
            case Point
                 {
                     X: 1
                 }:
                {
                    A();
                }
                break;

            case string s:
                {
                    B();
                }
                break;
        }
    }
}
```

A wrapped `when` clause is left unchanged:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1 when x > 0
                        && x < 10:
                A();
                break;

            case 2:
                B();
                break;
        }
    }
}
```

### FMT-SWITCH-005 — A multi-line switch expression makes a section multi-line

A section MUST count as multi-line when any of its statements, including a `return` or `throw`, contains a switch expression that spans more than one line.

Why: A multi-line switch expression is a block of arms with its own braces; inside an unbraced section its closing `};` would line up with nothing of the section. This is the one case where a single terminal statement makes the section multi-line (compare FMT-SWITCH-003).

Example:

```csharp
class C
{
    int M(int x)
    {
        switch (x)
        {
            case 1:
                return x switch
                       {
                           1 => 1,
                           _ => 0
                       };

            case 2:
                return 2;
        }

        return 0;
    }
}
```

```csharp
class C
{
    int M(int x)
    {
        switch (x)
        {
            case 1:
                {
                    return x switch
                           {
                               1 => 1,
                               _ => 0
                           };
                }

            case 2:
                {
                    return 2;
                }
        }

        return 0;
    }
}
```

### FMT-SWITCH-006 — Labels stay together; a `break`-only section stays bare

When the formatter braces a section with several labels, it MUST keep all labels in place and open the block after the last one. A section whose only statements are `break` statements MUST be left unbraced, and it MUST NOT make the switch multi-line.

Why: Stacked labels select one shared body; the block belongs to that body, not to a single label. A section that only breaks has no body to scope, and `case 3: { } break;` would add noise without structure.

Example:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
            case 2:
                A();
                B();
                break;

            case 3:
                break;
        }
    }
}
```

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
            case 2:
                {
                    A();
                    B();
                }
                break;

            case 3:
                break;
        }
    }
}
```

### FMT-SWITCH-007 — Comments survive brace insertion

When the formatter braces a section, a comment trailing the last label MUST stay on the label's line, a comment leading the first statement MUST move with that statement into the block, and a comment trailing the last statement in the block MUST stay on that statement's line (FMT-PIPE-002).

Why: Brace insertion rebuilds the trivia around the labels, the new braces, and the statements it moves. Comments are user-authored content and must keep their position relative to the code they describe.

Example:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1: // one
                // first
                A();
                B(); // after b
                break;

            case 2:
                C();
                break;
        }
    }
}
```

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1: // one
                {
                    // first
                    A();
                    B(); // after b
                }
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

### FMT-SWITCH-008 — An existing block makes the switch braced

A section whose body already starts with a block MUST keep that block, and because FMT-BREAK places the braces of every block on their own lines before this phase runs, such a section MUST count as multi-line and brace the rest of the switch.

Why: An author who wrote a block in one section asked for scoping there, and FMT-SWITCH-001 then extends the same shape to the other sections. Removing the block instead could turn locals that are scoped to their section into locals shared across the switch.

Example:

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

### FMT-SWITCH-009 — An existing block is not restructured

When a section's body already starts with a block, the formatter MUST NOT add a second block, MUST NOT move statements into or out of it, and MUST leave any statements that follow the block where they are.

Why: The author chose what the block scopes. Moving statements across the block boundary changes which locals are visible where, and the phase cannot tell which of the author's choices are deliberate. A `break` inside the block is RH7501's concern, not the formatter's.

Example — section 1 is kept; section 2 is braced because section 1 is multi-line:

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

                B();
                break;

            case 2:
                C();
                break;
        }
    }
}
```

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

                B();
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

### FMT-SWITCH-010 — A section with a directive is left unbraced

The formatter MUST NOT brace a section when a preprocessor directive or disabled text sits in the leading or trailing trivia of one of its labels or directly owned statements. The other sections of the switch MUST still be braced when the switch is multi-line.

Why: Brace insertion rebuilds that section-level trivia from its comments only, so a directive there would be dropped (FMT-PIPE-002), and a `#if` region cannot simply be moved into a block without changing which statements it covers. Leaving one section unbraced is the safe outcome; the rest of the switch still gets the uniform shape. A directive nested inside a statement, such as in the body of an `if`, moves with that statement and does not block the section.

Example — with `DEBUG` undefined, `A();` is disabled text in section 1:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
#if DEBUG
                A();
#endif
                B();
                break;

            case 2:
                C();
                D();
                break;

            case 3:
                E();
                break;
        }
    }
}
```

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
#if DEBUG
                A();
#endif
                B();
                break;

            case 2:
                {
                    C();
                    D();
                }
                break;

            case 3:
                {
                    E();
                }
                break;
        }
    }
}
```

### FMT-SWITCH-011 — A `goto` into another section leaves the switch alone

The formatter MUST NOT brace any section of a switch in which an ordinary `goto` statement in one section targets a label declared in another section of the same switch, including a `goto` inside a switch nested in a section (FMT-PIPE-003). `goto case` and `goto default` MUST NOT trigger this guard.

Why: A label inside a block is out of scope for a `goto` outside that block, so bracing the target's section would make the code fail to compile. `goto case` and `goto default` target switch labels, which stay outside the new blocks.

Example:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                A();

                goto target;

            case 2:
                target:
                B();
                C();
                break;
        }
    }
}
```

is left unchanged.

### FMT-SWITCH-012 — Labels in lambdas and local functions are their own scope

When it looks for a `goto` into another section (FMT-SWITCH-011), the formatter MUST ignore labels and `goto` statements inside lambdas, anonymous methods, and local functions.

Why: Each of these bodies has its own label scope; a `goto` inside a lambda can only reach a label in the same lambda, so equal label names in two sections' lambdas do not connect the sections. Treating them as a cross-section jump would leave a switch unbraced for no reason.

Example:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                Action a = () =>
                           {
                               goto target;
                               target:
                               return;
                           };
                break;

            case 2:
                Action b = () =>
                           {
                               target:
                               return;
                           };
                break;
        }
    }
}
```

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                {
                    Action a = () =>
                               {
                                   goto target;
                                   target:
                                   return;
                               };
                }
                break;

            case 2:
                {
                    Action b = () =>
                               {
                                   target:
                                   return;
                               };
                }
                break;
        }
    }
}
```

### FMT-SWITCH-013 — Nested switches are decided independently

The formatter MUST decide the braces of every `switch` statement on its own sections: a nested switch MUST be braced or left unbraced by its own sections, and its own shape MUST NOT make the sections of the enclosing switch braced or unbraced except through the lines it occupies in its section.

Why: Each switch is a separate decision with its own sections. An inner switch of one-line sections stays compact even when the outer switch is braced because the inner switch spans several lines.

Example:

```csharp
class C
{
    void M(int x, int y)
    {
        switch (x)
        {
            case 1:
                switch (y)
                {
                    case 1:
                        A();
                        break;

                    case 2:
                        B();
                        break;
                }

                break;

            case 2:
                C();
                break;
        }
    }
}
```

```csharp
class C
{
    void M(int x, int y)
    {
        switch (x)
        {
            case 1:
                {
                    switch (y)
                    {
                        case 1:
                            A();
                            break;

                        case 2:
                            B();
                            break;
                    }
                }
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
