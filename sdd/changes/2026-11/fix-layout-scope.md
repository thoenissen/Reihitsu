# A layout fix changes only its code: the target's leading trivia, the parts of the reported construct, and the code around a chain or switch expression stay

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-PIPE-010, FMT-PIPE-012, FMT-PIPE-017, FMT-PIPE-018, FMT-PIPE-023, FMT-PIPE-024, FMT-DOC-001, SHARED-FIX-006, SHARED-CHAIN-008, SHARED-CHAIN-009, RH5101-003, RH5102-003, RH5110-003, RH5111-004, RH5113-003, RH5206-003, RH5303-005, RH5304-004, RH5305-004, RH5306-004, RH5307-004, RH5308-004, RH5401-003, RH5403-003, RH5404-003, RH5408-005, RH5417-003 |
| Depends on | [The context entry point's target keeps its first line, and its other lines follow it](context-target-first-line.md) |

## What

- **The leading trivia of a fix's target.** `FormatNodeInDocumentAsync` writes back its target's leading trivia as it stands in the document in every case, as `FormatNodeInDocumentWithContextAsync` already does (FMT-PIPE-018 now covers both, FMT-PIPE-010 states it for both). Until now it kept that trivia only for a target that does not start its line or is a direct child of the root, and otherwise wrote back what the phases decided: blank lines added or removed above the target or above a comment that leads it, a re-indented comment line, a rewritten documentation comment, a `/** */` documentation comment moved off the code in front of the target. FMT-PIPE-010 loses its exception for the whitespace in front of such a moved comment. FMT-PIPE-017 counts every character in front of the node's first token as one column, a tab included, because no entry point drops that whitespace any more. FMT-PIPE-024 no longer gives the preceding-token and list-position facts, which only decided the root's leading trivia; FMT-PIPE-012, FMT-PIPE-023, and FMT-DOC-001 follow. SHARED-FIX-006 states once, as its *Leading trivia* bullet, that a fix's target's leading trivia is not part of what the fix formats; the rule specs reference it instead of each stating it.
- **The parts of the reported construct.** SHARED-FIX-006 gains the *Inside the reported construct* bullet: a fix whose diagnostic is about the line breaks between the parts of a construct — list elements, the condition and branches of a conditional expression, the statements of a block, the members of a type — changes only where each part and each delimiter starts. Each part keeps its tokens, its spacing, and its shape, and no blank line is inserted between two parts. RH5101-003, RH5102-003, RH5303-005 to RH5308-004, RH5403-003, RH5404-003, and RH5417-003 reference it.
- **The code around a chain.** SHARED-CHAIN-009 no longer falls back to writing back the formatted statement or member around a chain. A chain fix changes only the chain: its first token keeps its line and column, and its other lines are laid out from that column. SHARED-CHAIN-008 takes the chain's columns from that column.
- **The code around a switch expression.** RH5206-003 no longer formats the innermost statement, initializer, or expression body around the switch expression: the fix changes only the switch expression, laid out from the column of its first token.
- **The scope of single fixes.** RH5111-004 formats the reported construct and no assignment around it; RH5113-003 moves only the semicolon. Both were aligned to SHARED-FIX-006 as clarifications before this doc and are listed here for their visible outputs.

What stays: RH5110-003, RH5403-003, and RH5404-003 still format the declaration or the statement that owns the reported clause, block, or body, because no entry point lays out code whose first token moves onto a new line without formatting the node around it. Their header, the other branches of an `if`, and the code in front of the first constraint clause are laid out as on `main`; only the leading trivia and the parts inside the reported block or body change, as listed below.

The callers that reach the change (`AGENTS.md`, *Supported inputs*) are code fixes alone, applied in the IDE through the analyzer package: they are the only callers of the node-in-document entry points, and the CLI runs no code fix.

## Why

SHARED-FIX-006 requires a fix to change only the code its diagnostic is about, and FMT-PIPE-010 names SHARED-FIX-006 as its reason. Yet FMT-PIPE-010 and FMT-PIPE-024 had `FormatNodeInDocumentAsync` decide its target's leading trivia, so every fix that formats a node starting its line inserted blank lines above it, re-indented the comment above it, and rewrote its documentation comment: two owners with opposite answers for one decision. The fixes are the entry point's only callers, so the entry point is the one place to keep that trivia. Several rule specs ordered a fix to format more than its code — the statement around a chain or a switch expression, the elements of a list, the members of a type — where the maintainer chose that only the reported code changes (Decisions).

## Visible output changes

Every change is a code-fix result. The "before" outputs were measured on this branch at `04a85d6`, whose source equals `main`, with `scripts/apply-fix.sh` under LF and CRLF alike, except where marked as derived. In changes 2, 7, and 8, what changed was measured; the exact lines of the "before" output are derived from the formatter spec.

### 1. RH5403 · Statement must not be on a single line — a comment above the statement

```csharp
// input
        a = 1;
        // c
        if (a > 0) { a++; }
```

```csharp
// before
        a = 1;

        // c
        if (a > 0)
        {
            a++;
        }
```

```csharp
// after
        a = 1;
        // c
        if (a > 0)
        {
            a++;
        }
```

The same holds without the comment (no blank line above the `if`), behind a `case` label, and for the RH5404 fix with a comment above a type declaration. A comment above the statement in another column keeps its column instead of being re-indented.

### 2. RH5113, RH5401, RH5408, RH5111 · a one-line documentation comment above the member

```csharp
// input
    /// <summary>The value.</summary>
    public int Value
    {
        get;
        set;
    } = 1;
```

```csharp
// before (RH5408)
    /// <summary>
    /// The value.
    /// </summary>
    public int Value { get; set; } = 1;
```

```csharp
// after
    /// <summary>The value.</summary>
    public int Value { get; set; } = 1;
```

The same holds for every fix that formats a member below a comment line: no blank line is inserted above the comment (RH5110, RH5111, RH5113, RH5401, RH5408, RH5412, RH5417 measured).

### 3. RH5206 · Switch expression braces should be anchored — the statement behind another statement

```csharp
// input
        status++;
        return status switch
            {
              0 => "Idle",
                _ => "Other"
            };
```

```csharp
// before
        status++;

        return status switch
               {
                   0 => "Idle",
                   _ => "Other"
               };
```

```csharp
// after
        status++;
        return status switch
               {
                   0 => "Idle",
                   _ => "Other"
               };
```

### 4. RH5206 · a switch expression in an `if` condition

```csharp
// input
        if (Check(status switch
            {
              0 => "Idle",
                _ => "Other"
            }))
        {
            int  y = 1;
        }
```

```csharp
// before (derived from RH5206-003 as written before this change)
        if (Check(status switch
                  {
                      0 => "Idle",
                      _ => "Other"
                  }))
        {
            int y = 1;
        }
```

```csharp
// after
        if (Check(status switch
                  {
                      0 => "Idle",
                      _ => "Other"
                  }))
        {
            int  y = 1;
        }
```

### 5. RH5112 · Wrapped fluent calls should keep the first call on the original line — the code around the chain

```csharp
// input
        if (x==1 && a
            .B())
        {
            int  y = 1;
        }
        var r=a
            .B()
            .C();
```

```csharp
// before
        if (x == 1 && a.B())
        {
            int y = 1;
        }
        var r = a.B()
                 .C();
```

```csharp
// after
        if (x==1 && a.B())
        {
            int  y = 1;
        }
        var r=a.B()
               .C();
```

The same holds for the RH5201 fix.

### 6. RH5101 · the spacing inside an argument

```csharp
// input
        M(
            1+2,
            3);
```

```csharp
// before
        M(1 + 2,
          3);
```

```csharp
// after
        M(1+2,
          3);
```

The same holds for RH5102 and for RH5303 to RH5308.

### 7. RH5404 and RH5403 · the members and statements inside the reported body

```csharp
// input
    class B { int M() => 1; [System.Obsolete] int _x; }

        if (a > 0) { int N() => 1; a += N(); }
```

```csharp
// before
    class B
    {
        int M()
        {
            return 1;
        }
        [System.Obsolete]
        int _x;
    }

        if (a > 0)
        {
            int N()
            {
                return 1;
            }

            a += N();
        }
```

```csharp
// after
    class B
    {
        int M() => 1;
        [System.Obsolete] int _x;
    }

        if (a > 0)
        {
            int N() => 1;
            a += N();
        }
```

A nested type in the body keeps its own code too: `class A { class B { } }` becomes `class A`, `{`, `    class B { }`, `}`, where it became a `class B;` before (derived), and RH5404 still reports `B`. The same holds for the statements of a body that the RH5417 fix lays out.

### 8. RH5111 and RH5113 · the code around the reported construct

```csharp
// input
        a = () => { var x
            = 1; int  y = x; };
    int  _a
        ;
```

```csharp
// before
        a = () =>
        {
            var x = 1;
            int y = x;
        };
    int _a;
```

```csharp
// after
        a = () => { var x = 1; int  y = x; };
    int  _a;
```

### 9. A target indented with tabs

Not shown as blocks because the input contains tabs: the RH5403 fix of `→→if (a > 0) { a++; }`, indented with two tabs. Before: the tabs counted four columns each and were replaced, `        if (a > 0)` with the block in column 8. After: the tabs stay, count one column each as SHARED-FIX-007 already states for the whitespace in front of the changed code, and the block is laid out from column 2: `→→if (a > 0)`, `  {`, `      a++;`, `  }`. The input violates RH5601.

## Decisions

### Answers of the maintainer

- **1, 2 (RH5403/RH5404 part), and the leading trivia of every fix** — answered on #1748 (2026-11-15): A, no blank line is inserted above a comment that leads the target; the leading trivia, blank lines included, stays as written. The other fixes of changes 1 and 2, change 3, and change 9 follow from SHARED-FIX-006 and SHARED-FIX-007, which this change does not alter.
- **5** — answered in this run: C, only the chain is joined; the rest of the header and the body stay. It applies to every chain fix.
- **6** — answered in this run: A, only the list's line breaks change; element interiors stay. It applies to RH5102 and the other list-layout rules.
- **4** — answered in this run: A, only the switch expression is laid out; the `if` body stays.
- **7** — answered in this run: A, the declaration or statement is broken onto lines; the members or statements inside keep their own shape.
- **8** — demanded by SHARED-FIX-006; the maintainer's triage on #1140 calls the RH5111 alignment a clarification.

### One owner for the target's leading trivia

The entry point keeps the trivia, not each fix. The fixes are its only callers, and all of them need the same answer.

## Alternatives considered

- **A shared code-fix step that restores the leading trivia after formatting** — rejected: it leaves FMT-PIPE-010 deciding the trivia and adds a second owner of the same decision, which every fix has to remember to call.
- **Narrowing RH5110, RH5403, and RH5404 to the reported clause, block, or body** — rejected for now: the first token of the changed code moves onto a new line, and no entry point lays that out without formatting the node around it. A per-fix computation would predict the indentation phase; a shared write-back of part of a formatted node is a mechanism of its own. The instances stay on #1140.
- **Keeping the SHARED-CHAIN-009 fallback** — rejected by the maintainer's answer to change 5.
