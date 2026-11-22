# Comments and directives behind the `=` of an assignment or a property initializer, or behind a switch arm's `=>`, are unsupported

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-UNSUP-020 (new), FMT-UNSUP-021 (new), FMT-UNSUP-011, FMT-UNSUP-012, FMT-UNSUP-017, FMT-UNSUP-019, FMT-BREAK-016, FMT-BREAK-041, FMT-INDENT-034, RH2001-007, RH7101-007 |
| Depends on | [assignment-comment-in-arm-unsupported.md](assignment-comment-in-arm-unsupported.md), [continuation-indent.md](continuation-indent.md) |

## What

Two shapes become unsupported:

- a comment, a directive, or disabled text between the `=` of a simple assignment expression and its right operand, or between the `=` of a property's initializer and its value (FMT-UNSUP-020);
- a comment, a directive, or disabled text between the `=>` of a switch-expression arm and the arm's expression (FMT-UNSUP-021).

The formatter refuses an input that holds one as a whole: every formatting entry point returns it unchanged and reports it with a reason and a location (FMT-UNSUP-001), the CLI leaves such a file unchanged and reports it (CLI-019), and every code fix whose formatted node holds the shape is withheld (SHARED-FIX-010).

The compound assignment operators (`+=`, `??=`, and every other) are not part of the shape outside a switch arm or an anonymous-object member: a comment behind one keeps its value on a continuation line (FMT-INDENT-034), as on `main`. Inside an arm or a member, FMT-UNSUP-019 refuses every assignment operator, as on `main`.

This change covers thoenissen/Reihitsu#1926.

## Why

The maintainer decided to declare these shapes unsupported, in a change of their own (escalation 10, asked in the run of thoenissen/Reihitsu#953 and re-confirmed on thoenissen/Reihitsu#1926). A comment behind the `=` keeps the value off the `=`'s line, because the join of FMT-BREAK-016 does not cross it (FMT-BREAK-001), and the value starts a continuation line (FMT-INDENT-034). A comment behind a switch arm's `=>` stands on the line the arm's expression would otherwise start on; the comment belongs above the statement, or above the arm, or behind the arm's comma.

## Visible output changes

Every *before* below is the output of `main` (652e67b and later), produced with `scripts/format.sh` or `scripts/apply-fix.sh`. The CLI's line has the form CLI-019 states.

### A comment behind the `=` of an assignment or a property initializer

Input:

```csharp
class C
{
    int[] G { get; } = // values
    { 1, 2 };

    int M(int x, int y)
    {
        x = // reset
        y + 1;

        return x;
    }
}
```

Before (`main`): the values move to the continuation indent.

```csharp
class C
{
    int[] G { get; } = // values
        { 1, 2 };

    int M(int x, int y)
    {
        x = // reset
            y + 1;

        return x;
    }
}
```

After: the file is unchanged, and the CLI reports

```
error: <file>(3,24): unsupported syntax: a comment or directive behind the = of an assignment or a property initializer. The file was left unchanged.
```

### On one line, and in the members of an initializer

The members of an object initializer, of a `with` initializer, and of an index initializer are assignment expressions, and the shape is the same on one line. Input:

```csharp
using System.Collections.Generic;

record R
{
    public int A { get; set; }
}

class C
{
    object M(int x, R r)
    {
        var o = new R { A = /* c1 */ 3 };
        var p = r with { A = /* c2 */ 4 };
        var d = new Dictionary<int, int> { [0] = /* c3 */ 1 };
        x = /* c4 */ 1;

        return x + o.A + p.A + d.Count;
    }
}
```

Before (`main`): each initializer is spread over its lines, the comments stay behind their `=`, and `x = /* c4 */ 1;` stays as written.

```csharp
        var o = new R
                {
                    A = /* c1 */ 3
                };
        var p = r with
                  {
                      A = /* c2 */ 4
                  };
        var d = new Dictionary<int, int>
                {
                    [0] = /* c3 */ 1
                };
        x = /* c4 */ 1;
```

After: the file is unchanged, and the CLI reports

```
error: <file>(12,29): unsupported syntax: a comment or directive behind the = of an assignment or a property initializer. The file was left unchanged.
```

### A directive behind the `=`

Input, which `main` formats without a change:

```csharp
class C
{
    int M(int x)
    {
        x =
#if DEBUG
            1;
#else
            2;
#endif

        return x;
    }
}
```

Before (`main`): the file is unchanged and not reported.

After: the file is unchanged, and the CLI reports

```
error: <file>(6,1): unsupported syntax: a comment or directive behind the = of an assignment or a property initializer. The file was left unchanged.
```

### A comment behind a switch arm's `=>`

Input:

```csharp
class C
{
    int M(int v)
    {
        return v switch
               {
                   1 => // one
                   2,
                   2 => /* two */ 3,
                   _ => 0
               };
    }
}
```

Before (`main`): `2,` moves to the continuation indent, and `/* two */` stays as written.

```csharp
class C
{
    int M(int v)
    {
        return v switch
               {
                   1 => // one
                       2,
                   2 => /* two */ 3,
                   _ => 0
               };
    }
}
```

After: the file is unchanged, and the CLI reports

```
error: <file>(7,25): unsupported syntax: a comment or directive behind the => of a switch arm. The file was left unchanged.
```

A line break alone behind a switch arm's `=>` is formatted as on `main`: no requirement joins it (FMT-BREAK-041), and the expression keeps its continuation line (FMT-INDENT-034).

### Unchanged

A comment in front of the `=` (`x /* c */ = 1;`, `int P { get; } /* c */ = 1;`), inside the value behind its first token (`x = y /* c */ + 1;`), behind a compound assignment operator outside an arm or a member (`x += // c⏎2;`), behind the `=` of a variable declarator (refused by FMT-UNSUP-004 for a comment, formatted for a directive), an enum member, or a parameter's default value, and behind the arm's expression (`1 => 2, // c`) is formatted as on `main`.

### Code fixes

Every code fix whose formatted node holds such an assignment, property, or arm is withheld (SHARED-FIX-010); the diagnostic is still reported. The fixes that commonly format such a node, with their result on `main`:

- **RH7101** formats the whole type, so a field declaration with several declarators gets no fix when its type holds the shape anywhere. For

  ```csharp
  class C
  {
      private int _a, _b;

      int[] G { get; } = // values
          { 1, 2 };
  }
  ```

  `main` splits `_a, _b` into two declarations; after the change, no fix is offered.
- **RH2001** formats the field it builds from the property, so an initializer that holds such an assignment withholds it. For `private R Value { get; } = new R { A = /* three */ 3 };`, `main` writes

  ```csharp
      private readonly R _value = new R
                                  {
                                      A = /* three */ 3
                                  };
  ```

  and after the change, no fix is offered. A comment behind the property's own `=` already withholds the fix on `main` (FMT-UNSUP-004 refuses it in the field), and a directive there does not withhold it, as on `main`: the field's declarator is not part of the shape.
- **RH3202 and RH3203** format the block they build from an expression body. For `void Reset() => _v = // zero⏎0;`, `main` writes

  ```csharp
      void Reset()
      {
          _v = // zero
              0;
      }
  ```

  and after the change, no fix is offered.
- **RH5206** formats the switch expression. For `var r = v switch {⏎1 => // one⏎2,⏎_ => 0⏎};`, `main` anchors the braces under `v` and keeps `2,` on its continuation line; after the change, no fix is offered.
- **RH5301** formats the object creation. For `var o = new R { A = /* three */ 3 };`, `main` spreads the initializer over four lines as in the second example above; after the change, no fix is offered.
- **RH5111** already withholds its fix for a comment or directive around the `=` (RH5111-005), so nothing changes for it.

No rule reports a comment behind a switch arm's `=>`; RH5111 reports a value kept below its `=` by a comment, as on `main`. No rule page changes: rule pages never describe the formatter, and the general notes already cover a diagnostic without a fix.

## Special cases

### Deleted

- **FMT-BREAK-001's refusal in the join of a collection initializer assigned to a member** (`A = // c⏎{ 1, 2 }` inside an object initializer): only the `=` of a simple assignment assigns an initializer — a compound operator in front of one is a syntax error, which FMT-PIPE-005 leaves untouched — so every input that reaches this join holds no comment, directive, or disabled text in the gap.

### Checked and kept

- **FMT-BREAK-001's refusal in FMT-BREAK-016's join behind an assignment operator, for a value that is no initializer and for a collection expression** — the same join serves the compound assignment operators, whose gap stays supported outside an arm or a member.
- **FMT-BREAK-001's refusal in FMT-BREAK-016's join behind the `=` of an equals-value clause** — the same join serves a variable declarator, whose `=` may hold a directive, an enum member, and a parameter's default value, whose gaps stay supported.
- **FMT-BREAK-001's refusal in FMT-BREAK-016's join in front of an assignment operator** — the gap in front of the operator is not part of the shape.
- **FMT-INDENT-034** — it still places the value behind a compound assignment operator, behind the `=` of an enum member or a parameter's default value, behind a directive at a variable declarator's `=`, and behind a switch arm's `=>` without a comment.
- **The walk of FMT-UNSUP-019 from an assignment to the nearest arm or member** — it still decides the compound assignment operators.

For the switch arm's `=>` the declaration deletes nothing: no requirement joins the gap behind it (FMT-BREAK-041), so the line behind it is a continuation line with or without a comment. The shape is declared on the maintainer's explicit answer, as FMT-UNSUP-019 was.

## Decisions

### Declare the shapes unsupported (maintainer, escalation 10)

The maintainer answered escalation 10 for these shapes in the run of thoenissen/Reihitsu#953 and re-confirmed it on thoenissen/Reihitsu#1926: the file stays unchanged, the CLI reports it, code fixes whose formatted node holds the shape are withheld, and the declaration is a change of its own.

### The `=` of a simple assignment, not the compound operators

The maintainer's answer names the `=` of an assignment. Whether the compound assignment operators outside an arm or a member belong to the shape is an open output question of this run; until it is answered, this doc and the spec take the literal reading.

### The members of an initializer count as assignments

The members of an object initializer, of a `with` initializer, and of an index initializer are assignment expressions in the syntax tree, as FMT-UNSUP-019 already counts them inside an arm or a member; excluding them would need a condition on the assignment's parent and would leave the same comment-held line supported in one syntax and refused in the other.

### Two requirements

The gap behind an assignment's or a property's `=` and the gap behind an arm's `=>` are written as two requirements with two reasons, so the CLI's line names the token the comment stands behind, as the reasons of the other shapes do.

### On any line

As for every other shape of the unsupported-syntax spec, the boundary is a place in the syntax tree, so a comment behind the `=` or the `=>` on one line is refused like one with a line break.

### A property initializer counts where the property is part of the input

The `=` of a property initializer belongs to an equals-value clause, which also serves a variable declarator, an enum member, and a parameter; the shape names the property, so it counts only where the property declaration is part of the input (FMT-UNSUP-003).

## Alternatives considered

- **Keep the continuation indent for these lines** — rejected by the maintainer (escalation 10).
- **Include every assignment operator, wherever the assignment stands** — open output question of this run; it would delete the walk of FMT-UNSUP-019 and the remaining refusals of FMT-BREAK-001 in the join behind an assignment operator, and would refuse `x += // c⏎3;`, which `main` formats.
- **Include the `=` of a variable declarator, an enum member, and a parameter's default value** — not chosen: the maintainer's answer names an assignment and a property initializer; the other gaps are follow-up candidates.
