# A layout fix changes only its code: the target's leading trivia, and the code around a chain, a switch expression, or a list construct, stay

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-PIPE-010, FMT-PIPE-012, FMT-PIPE-017, FMT-PIPE-018, FMT-PIPE-023, FMT-PIPE-024, FMT-DOC-001, SHARED-FIX-006, SHARED-CHAIN-008, SHARED-CHAIN-009, RH5110-003, RH5111-004, RH5113-003, RH5113-004, RH5206-003, RH5303-005, RH5304-004, RH5305-004, RH5306-004, RH5307-004, RH5308-004, RH5308-005, RH5308-006, RH5401-003, RH5403-003, RH5404-003, RH5408-005 |
| Depends on | [The context entry point's target keeps its first line, and its other lines follow it](context-target-first-line.md) |

## What

- **The leading trivia of a fix's target.** `FormatNodeInDocumentAsync` writes back its target's leading trivia as it stands in the document in every case, as `FormatNodeInDocumentWithContextAsync` already does (FMT-PIPE-018 now covers both, FMT-PIPE-010 states it for both). Until now it kept that trivia only for a target that does not start its line or is a direct child of the root, and otherwise wrote back what the phases decided: blank lines added or removed above the target or above a comment that leads it, a re-indented comment line, a rewritten documentation comment, a `/** */` documentation comment moved off the code in front of the target. FMT-PIPE-010 loses its exception for the whitespace in front of such a moved comment. FMT-PIPE-017 counts every character in front of the node's first token as one column, a tab included, because no entry point drops that whitespace any more. FMT-PIPE-024 no longer gives the preceding-token and list-position facts, which only decided the root's leading trivia; FMT-PIPE-012, FMT-PIPE-023, and FMT-DOC-001 follow, and `PrecedingTokenFacts` and `RootListPosition` leave the code owners of `pipeline.md`. SHARED-FIX-006 states once, as its *Leading trivia* bullet, that a fix's target's leading trivia is not part of what the fix formats, with one exception besides trivia the diagnostic is about: the line break SHARED-FIX-007 (*Shared line*) writes in front of a member that shares its line with a member in front of it. The rule specs reference the bullet instead of each stating it.
- **The code around a chain.** SHARED-CHAIN-009 no longer falls back to writing back the formatted statement or member around a chain. A chain fix changes only the chain: its first token keeps its line and column, and its other lines are laid out from that column. SHARED-CHAIN-008 takes the chain's columns from that column.
- **The code around a switch expression.** RH5206-003 no longer formats the innermost statement, initializer, or expression body around the switch expression: the fix changes only the switch expression, laid out from the column of its first token.
- **The code around a list construct.** RH5303-005, RH5304-004, RH5305-004, RH5306-004, RH5307-004, and RH5308-004 no longer let the fix format the statement, initializer, expression body, argument, or member around the reported construct: the fix formats only the object creation, the assignment, the collection expression, the list pattern, the bracketed argument list, or the conditional expression, laid out from the column its first token keeps, and the code around it stays. Because the fix asks the formatter about exactly the node it formats (SHARED-FIX-010, unchanged), a shape the formatter refuses elsewhere in the statement no longer withholds it: RH5308-005 narrows to a refused shape inside the reported conditional. RH5308-006 is retired: the conditional needs no statement or member around it, so the fix is offered for a conditional in an `[assembly:]` or `[module:]` attribute argument as well.
- **The scope of single fixes.** RH5111-004 formats the reported construct and no assignment around it; RH5113-003 moves only the semicolon. Both were aligned to SHARED-FIX-006 as clarifications before this doc and are listed here for their visible outputs.

What stays:

- **The code inside the reported construct.** The fixes of RH5101, RH5102, RH5303 to RH5308, RH5403, RH5404, and RH5417 lay out the inside of the construct they format as on `main`: the spacing inside a list element, a branch, or a statement, and the shape of a member or statement in a reported block or body (an expression body, an attribute list on the member's line, the blank lines between statements or members) are formatted as the formatter formats them. Keeping each part's own code needs a step that writes the parts back into the formatter's layout; it is left to a change of its own, together with the maintainer's answers for it (Decisions).
- **The header of RH5110, RH5403, and RH5404.** RH5110-003, RH5403-003, and RH5404-003 still format the declaration or the statement that owns the reported clause, block, or body. Their header, the other branches of an `if`, and the code in front of the first constraint clause are laid out as on `main`; only the leading trivia changes, as listed below.

The callers that reach the change (`AGENTS.md`, *Supported inputs*) are code fixes alone, applied in the IDE through the analyzer package: they are the only callers of the node-in-document entry points, and the CLI runs no code fix.

## Why

SHARED-FIX-006 requires a fix to change only the code its diagnostic is about, and FMT-PIPE-010 names SHARED-FIX-006 as its reason. Yet FMT-PIPE-010 and FMT-PIPE-024 had `FormatNodeInDocumentAsync` decide its target's leading trivia, so every fix that formats a node starting its line inserted blank lines above it, re-indented the comment above it, and rewrote its documentation comment: two owners with opposite answers for one decision. The fixes are the entry point's only callers, so the entry point is the one place to keep that trivia. Several rule specs ordered a fix to format more than its code — the statement around a chain, a switch expression, a list, or a conditional — where the maintainer chose that only the reported construct changes (Decisions).

## Visible output changes

Every change is a code-fix result. The "before" outputs were measured on this branch at `04a85d6`, whose source equals `main`, with `scripts/apply-fix.sh` under LF and CRLF alike, except where marked as derived. In changes 2 and 9, what changed was measured; the exact lines of the "before" output are derived from the formatter spec.

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

The same holds without the comment (no blank line above the `if`), behind a `case` label, and for the RH5404 fix with a comment above a type declaration. A comment above the statement in another column keeps its column instead of being re-indented. A blank line between `{` and the statement, which formatting the document removes, stays, and so do two blank lines above the statement, which formatting the document collapses into one.

### 2. RH5113, RH5401, RH5408, RH5111, RH2001 · a one-line documentation comment above the member

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

The same holds for every fix that formats a member below a comment line: no blank line is inserted above the comment (RH5110, RH5111, RH5113, RH5401, RH5408, RH5412, RH5417 measured). The RH2001 fix, which replaces a property by a field, keeps the one-line documentation comment above it and inserts no blank line between the field in front of it and that comment (measured: before, a blank line was inserted and the `<summary>` spread over three lines). The same holds for a fix whose target is an expression that starts its line, such as the RH3004, RH5205, RH5301, and RH5302 fixes: the comment and the blank lines above that line stay.

In the RH2001 case:

```csharp
// input
    private int _a;
    /// <summary>The value.</summary>
    private int Value { get; set; }
```

```csharp
// before
    private int _a;

    /// <summary>
    /// The value.
    /// </summary>
    private int _value;
```

```csharp
// after
    private int _a;
    /// <summary>The value.</summary>
    private int _value;
```

The field's name in the "before" and "after" blocks is derived; what was measured is the blank line and the three-line `<summary>`.

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

### 6. RH5308 and RH5303 to RH5307 · the code around the reported construct

```csharp
// input
        var  r=a ? b+1
            : 2;
```

```csharp
// before (RH5308)
        var r = a
                    ? b + 1
                    : 2;
```

```csharp
// after
        var  r=a
                   ? b + 1
                   : 2;
```

The conditional is laid out from the column `a` keeps, and its inside is formatted as before. The same holds for the RH5303 fix of `var  x=new List<int> { 1+2,⏎3 };`: before, the gap in front of `new` was respaced to `var  x= new List<int>` (measured); after, `var  x=` stays and the braces are written in the column `new` keeps. The same holds for the RH5304, RH5305, RH5306, and RH5307 fixes. A reported construct nested in another one, such as a conditional in a branch of a reported conditional, is laid out with the outer one, and Fix All still corrects both in one application (SHARED-FIX-002, unchanged).

### 7. RH5308 and RH5303 to RH5307 · a refused shape elsewhere in the statement

```csharp
// input
        var x = (a ?
            1 : 2) + (b ? /* c */ 3 : 4);
```

```csharp
// before
no fix offered
```

```csharp
// after
        var x = (a
                     ? 1
                     : 2) + (b ? /* c */ 3 : 4);
```

The comment behind the second `?` is a shape the formatter refuses (FMT-UNSUP-008). Before, the fix formatted the whole statement and was withheld; now it formats only the reported conditional, which does not hold the shape. The same holds for the RH5303 to RH5307 fixes where a shape the formatter refuses stands in the statement or initializer around the reported construct, such as a comment inside the variable declaration around it (FMT-UNSUP-004). The "after" output is derived from RH5308-004, not measured.

### 8. RH5308 · a conditional in an `[assembly:]` or `[module:]` attribute argument

```csharp
// input
[assembly: System.CLSCompliant(true ?
    true :
    false)]
```

```csharp
// before
no fix offered
```

```csharp
// after
[assembly: System.CLSCompliant(true
                                   ? true
                                   : false)]
```

The conditional stands in no statement and no member, which the fix needed while it formatted the statement or member around the conditional. The "after" output is derived from RH5308-004, not measured.

### 9. RH5111 and RH5113 · the code around the reported construct

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

### 10. A target indented with tabs

Not shown as blocks because the input contains tabs: the RH5403 fix of `→→if (a > 0) { a++; }`, indented with two tabs. Before: the tabs counted four columns each and were replaced, `        if (a > 0)` with the block in column 8. After: the tabs stay, count one column each as SHARED-FIX-007 already states for the whitespace in front of the changed code, and the block is laid out from column 2: `→→if (a > 0)`, `  {`, `      a++;`, `  }`. The input violates RH5601.

## Decisions

### Answers of the maintainer

- **1, 2 (RH5403/RH5404 part), and the leading trivia of every fix** — answered on #1748 (2026-11-15): A, no blank line is inserted above a comment that leads the target; the leading trivia, blank lines included, stays as written. The other fixes of changes 1 and 2, change 3, and change 10 follow from SHARED-FIX-006 and SHARED-FIX-007, which this change does not alter.
- **5** — answered in this run: C, only the chain is joined; the rest of the header and the body stay. It applies to every chain fix.
- **4** — answered in this run: A, only the switch expression is laid out; the `if` body stays.
- **6** — answered in this run: B, only the construct is laid out, from the column its first token keeps; the rest of the statement stays, and the inside of the construct is laid out as on `main`.
- **7** — answered in this run: A, the fix is offered; it formats only the reported construct, and SHARED-FIX-010, which this change does not alter, has it ask the formatter about exactly that node.
- **8** — answered in this run: A, the fix is offered for a conditional in an `[assembly:]` or `[module:]` attribute argument; RH5308-006 is retired.
- **9** — demanded by SHARED-FIX-006; the maintainer's triage on #1140 calls the RH5111 alignment a clarification.
- **The code inside the reported construct** — answered in this run: C, not in this change. The maintainer also answered how the parts should look once that step exists: a list element keeps its spacing, and the members of a reported type body and the statements of a reported block keep their own shape, with no blank line inserted between them. Those answers, and the open question of where the later lines of a part that spans lines go, belong to the change that builds the step.

### One owner for the target's leading trivia

The entry point keeps the trivia, not each fix. The fixes are its only callers, and all of them need the same answer.

### The line break in front of a member that shares its line

SHARED-FIX-006 (*Leading trivia*) names the line break SHARED-FIX-007 (*Shared line*) writes as its one exception besides trivia the diagnostic is about, so the two requirements do not disagree on their face. The fix writes that line break into the document before the entry point formats the member, so the entry point keeps it as part of the leading trivia.

### RH5113 keeps its withholding

Once the RH5113 fix moves only the semicolon, it lays out nothing through the formatter, so SHARED-FIX-010 and SHARED-FIX-011 no longer withhold it in a file with a syntax error or for a declaration the formatter refuses. It stays withheld there, as before this change, and RH5113-004 records that as a decision: offering it would change what the user sees, which no answer of the maintainer covers. Whether to offer it there is left to a change of its own.

## Alternatives considered

- **A shared code-fix step that restores the leading trivia after formatting** — rejected: it leaves FMT-PIPE-010 deciding the trivia and adds a second owner of the same decision, which every fix has to remember to call.
- **Keeping the code inside the reported construct in this change** — deferred by the maintainer. The formatter lays out the delimiters of the construct, and the code of each part would be written back into that layout by one shared step in the code-fix layer; computing the parts' columns in each fix would predict the indentation phase instead. That step is a mechanism of its own, likely a new type, and is left to a change of its own.
- **Narrowing RH5110, RH5403, and RH5404 to the reported clause, block, or body** — left out for scope. The first token of the changed code moves onto a new line, and no entry point lays that out without formatting the node around it. The shared write-back step above could keep the header as written once it exists; until then, a per-fix computation would predict the indentation phase. The instances stay on #1140.
- **Keeping the SHARED-CHAIN-009 fallback** — rejected by the maintainer's answer to change 5.
- **For RH5303 to RH5308, formatting the statement and keeping only the parts, or keeping `main`'s output** — rejected by the maintainer's answer to change 6.
- **Keeping the wider RH5308-005 withholding** — rejected: the fix would have to ask the formatter about a node it does not format, which SHARED-FIX-010 forbids, to refuse an input it can rewrite.
- **Keeping RH5308-006** — rejected by the maintainer's answer to change 8: once the fix formats only the conditional, the withholding protects nothing.
