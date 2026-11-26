# Comments and directives behind an assignment operator, an `=`, or a switch arm's `=>` are unsupported

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-UNSUP-020 (new), FMT-UNSUP-021 (new), FMT-UNSUP-022 (new), FMT-UNSUP-019 (retired), FMT-UNSUP-003, FMT-UNSUP-011, FMT-UNSUP-012, FMT-UNSUP-017, FMT-BREAK-016, FMT-BREAK-041, FMT-INDENT-023, FMT-INDENT-034, FMT-BLANK-018, FMT-BLANK-020, FMT-PIPE-017, SHARED-FIX-010, RH2001-007, RH7101-007 |
| Depends on | [assignment-comment-in-arm-unsupported.md](assignment-comment-in-arm-unsupported.md), [continuation-indent.md](continuation-indent.md) |

## What

Three shapes become unsupported:

- a comment, a directive, or disabled text between an assignment operator — `=` and every compound operator, wherever the assignment stands — and its right operand, or between the `=` of a property's initializer, a variable declarator, an enum member, or a parameter's default value and the value behind it (FMT-UNSUP-020);
- a comment, a directive, or disabled text between the `=>` of a switch-expression arm and the arm's expression (FMT-UNSUP-021);
- a comment, a directive, or disabled text between the `=` of an attribute argument or of a using alias and the value behind it (FMT-UNSUP-022).

The formatter refuses an input that holds one as a whole: every formatting entry point returns it unchanged and reports it with a reason and a location (FMT-UNSUP-001), the CLI leaves such a file unchanged and reports it (CLI-019), and every code fix whose formatted node holds the shape is withheld (SHARED-FIX-010).

FMT-UNSUP-020 holds every input FMT-UNSUP-019 refused — an assignment inside a switch arm or an anonymous-object member — so FMT-UNSUP-019 is retired, and such input is reported with FMT-UNSUP-020's reason.

This change covers thoenissen/Reihitsu#1926.

## Why

The maintainer decided to declare these shapes unsupported (escalation 10, asked in the run of thoenissen/Reihitsu#953 and re-confirmed on thoenissen/Reihitsu#1926), and in this run extended the shape to every assignment operator, to a deconstruction declaration, and to the `=` of an enum member, a parameter's default value, and a variable declarator; asked about the two remaining `=` tokens, it declared the `=` of an attribute argument and of a using alias unsupported as well, in this work, and left the wording of the reasons to the run. A comment behind the operator or the `=` keeps the value off its line, because the joins of FMT-BREAK-016 do not cross it (FMT-BREAK-001), and the value starts a continuation line (FMT-INDENT-034). A comment behind a switch arm's `=>` stands on the line the arm's expression would otherwise start on; the comment belongs above the statement, the member, or the arm, or behind the arm's comma. A comment behind the `=` of an attribute argument or a using alias keeps the value off the `=`'s line in the same way; it belongs above the attribute or the directive, or behind the value.

## Visible output changes

Every *before* below is the output of `main` (652e67b and later), produced with `scripts/format.sh` or `scripts/apply-fix.sh`. The CLI's line has the form CLI-019 states. The reason of FMT-UNSUP-020 is `a comment or directive behind an assignment operator or the = of a declaration`; the reason of FMT-UNSUP-021 is `a comment or directive behind the => of a switch arm`; the reason of FMT-UNSUP-022 is `a comment or directive behind the = of an attribute argument or a using alias`.

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
error: <file>(3,24): unsupported syntax: a comment or directive behind an assignment operator or the = of a declaration. The file was left unchanged.
```

### A comment on its own line behind the `=`

Input:

```csharp
class C
{
    int M(int x, int y)
    {
        x =
            // reset
            y + 1;

        return x;
    }
}
```

Before (`main`): a blank line is inserted above the comment (FMT-BLANK-011).

```csharp
class C
{
    int M(int x, int y)
    {
        x =

            // reset
            y + 1;

        return x;
    }
}
```

After: the file is unchanged, and the CLI reports

```
error: <file>(6,13): unsupported syntax: a comment or directive behind an assignment operator or the = of a declaration. The file was left unchanged.
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
error: <file>(12,29): unsupported syntax: a comment or directive behind an assignment operator or the = of a declaration. The file was left unchanged.
```

### A compound assignment operator

Input:

```csharp
class C
{
    int M(int x)
    {
        x += // add
        3;

        return x;
    }
}
```

Before (`main`):

```csharp
class C
{
    int M(int x)
    {
        x += // add
            3;

        return x;
    }
}
```

After: the file is unchanged, and the CLI reports

```
error: <file>(5,14): unsupported syntax: a comment or directive behind an assignment operator or the = of a declaration. The file was left unchanged.
```

### An assignment inside a switch arm or an anonymous-object member

Input, which `main` already refuses (FMT-UNSUP-019):

```csharp
class C
{
    int M(int x, int v)
    {
        return v switch
               {
                   1 => x += /* add */ 2,
                   _ => 0
               };
    }
}
```

Before (`main`): the file is unchanged, and the CLI reports

```
error: <file>(7,30): unsupported syntax: a comment or directive behind an assignment operator inside a switch arm or an anonymous-object member. The file was left unchanged.
```

After: the file is unchanged, and the CLI reports

```
error: <file>(7,30): unsupported syntax: a comment or directive behind an assignment operator or the = of a declaration. The file was left unchanged.
```

### A deconstruction declaration

Input:

```csharp
class C
{
    void M((int, int) t)
    {
        var (a, b) = // c
        t;
        System.Console.WriteLine(a + b);
    }
}
```

Before (`main`):

```csharp
class C
{
    void M((int, int) t)
    {
        var (a, b) = // c
            t;
        System.Console.WriteLine(a + b);
    }
}
```

After: the file is unchanged, and the CLI reports

```
error: <file>(5,22): unsupported syntax: a comment or directive behind an assignment operator or the = of a declaration. The file was left unchanged.
```

### The `=` of an enum member, a parameter's default value, and a variable declarator

Input:

```csharp
enum E
{
    A = // one
    1,
    B = 2
}

class C
{
    int M(int y = /* default */ 1)
    {
        int x =
#if DEBUG
            1;
#else
            2;
#endif

        return x + y;
    }
}
```

Before (`main`): `1,` moves to the continuation indent; the parameter and the directive stay as written.

```csharp
enum E
{
    A = // one
        1,
    B = 2
}
```

After: the file is unchanged, and the CLI reports

```
error: <file>(3,9): unsupported syntax: a comment or directive behind an assignment operator or the = of a declaration. The file was left unchanged.
```

A comment behind a variable declarator's `=` is refused on `main` already, with FMT-UNSUP-004's reason, which it keeps; a directive there in a field declaration with several declarators keeps FMT-UNSUP-010's.

### A directive behind the `=` of an assignment or a property, or behind a switch arm's `=>`

Inputs, which `main` formats without a change:

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

```csharp
class C
{
    int P { get; } =
#if DEBUG
        1;
#else
        2;
#endif
}
```

```csharp
class C
{
    int M(int v)
    {
        return v switch
               {
                   1 =>
#if DEBUG
                       2,
#else
                       3,
#endif
                   _ => 0
               };
    }
}
```

Before (`main`): each file is unchanged and not reported.

After: each file is unchanged, and the CLI reports, in this order,

```
error: <file>(6,1): unsupported syntax: a comment or directive behind an assignment operator or the = of a declaration. The file was left unchanged.
error: <file>(4,1): unsupported syntax: a comment or directive behind an assignment operator or the = of a declaration. The file was left unchanged.
error: <file>(8,1): unsupported syntax: a comment or directive behind the => of a switch arm. The file was left unchanged.
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

An own-line comment behind the `=>` (`1 =>⏎// one⏎2,`) gets a blank line above it on `main` (FMT-BLANK-011); after the change it is refused like the comment above. A line break alone behind a switch arm's `=>` is formatted as on `main`: no requirement joins it (FMT-BREAK-041), and the expression keeps its continuation line (FMT-INDENT-034).

### A comment behind the `=` of an attribute argument or a using alias

Input:

```csharp
using System;
using Text = // alias
System.Text;

class NAttribute : Attribute
{
    public int Name { get; set; }
}

class C
{
    [N(Name = // name
    1)]
    void M()
    {
    }
}
```

Before (`main`): the alias's target moves to the continuation indent, and the attribute argument's value under `Name`.

```csharp
using System;

using Text = // alias
    System.Text;

class NAttribute : Attribute
{
    public int Name { get; set; }
}

class C
{
    [N(Name = // name
       1)]
    void M()
    {
    }
}
```

After: the file is unchanged, and the CLI reports

```
error: <file>(2,14): unsupported syntax: a comment or directive behind the = of an attribute argument or a using alias. The file was left unchanged.
```

Without the using alias, the same file is refused at the attribute argument: `error: <file>(10,15): unsupported syntax: a comment or directive behind the = of an attribute argument or a using alias. The file was left unchanged.` A comment on one line, such as `[N(Name = /* c */ 1)]`, which `main` leaves as written, is refused the same way. A comment behind the `=` of an attribute argument of a field declaration is refused on `main` already, with FMT-UNSUP-004's reason, which it keeps.

### Unchanged

Formatted as on `main`: a comment in front of the operator or the `=` (`x /* c */ = 1;`, `int P { get; } /* c */ = 1;`, `int y /* c */ = 1`, `[N(Name /* c */ = 1)]`, `using Text /* c */ = System.Text;`), inside the value behind its first token (`x = y /* c */ + 1;`, `[N(Name = 1 /* c */)]`), behind the colon of a named attribute argument (`[Obsolete(message: /* c */ "old")]`), and behind the arm's expression (`1 => 2, // c`). The `=` of an anonymous-object member and of a `let` clause stays refused with FMT-UNSUP-017's reason.

### Code fixes

Every code fix whose formatted node holds such an assignment, equals-value clause, arm, or attribute argument is withheld (SHARED-FIX-010); the diagnostic is still reported. The fixes that commonly format such a node, with their result on `main`:

- **RH7101** formats the whole type, so a field declaration with several declarators gets no fix when its type holds the shape anywhere — behind a property's `=`, an assignment in a member, or a parameter's default value. For

  ```csharp
  class C
  {
      private int _a, _b;

      int[] G { get; } = // values
          { 1, 2 };
  }
  ```

  and for

  ```csharp
  class C
  {
      private int _a, _b;

      int M(int y = /* default */ 1)
      {
          return y + _a + _b;
      }
  }
  ```

  and for

  ```csharp
  class C
  {
      private int _a, _b;

      [N(Name = // name
         1)]
      int M()
      {
          return _a + _b;
      }
  }
  ```

  `main` splits `_a, _b` into two declarations; after the change, no fix is offered.
- **RH2001** formats the field it builds from the property, so an initializer that holds such an assignment, or a directive behind the property's `=`, withholds it. For `private R Value { get; } = new R { A = /* three */ 3 };`, `main` writes

  ```csharp
      private readonly R _value = new R
                                  {
                                      A = /* three */ 3
                                  };
  ```

  and for

  ```csharp
  class C
  {
      private int Value { get; } =
  #if DEBUG
          1;
  #else
          2;
  #endif

      int M()
      {
          return Value;
      }
  }
  ```

  it writes `private readonly int _value =` in front of the unchanged group and `return _value;`. After the change, no fix is offered for either. A comment behind the property's own `=` already withholds the fix on `main` (FMT-UNSUP-004 refuses it in the field).
- **RH3202 and RH3203** format the block they build from an expression body. For `void Reset() => _v = // zero⏎0;` and `void Add() => _v += // one⏎1;`, `main` writes

  ```csharp
      void Reset()
      {
          _v = // zero
              0;
      }
  ```

  and the same shape with `_v += // one`; after the change, no fix is offered. The same holds for a method whose attribute holds a comment behind an argument's `=`: for `[N(Name = /* name */ 1)]⏎int M() => 1;`, `main` writes the method with a block body `{ return 1; }` below the unchanged attribute; after the change, no fix is offered. A fix that formats a whole member with its attribute lists, or the type around it, is withheld alike for such an attribute.
- **RH5206** formats the switch expression. For `var r = v switch {⏎1 => // one⏎2,⏎_ => 0⏎};`, `main` anchors the braces under `v` and keeps `2,` on its continuation line; after the change, no fix is offered.
- **RH5301** formats the object creation. For `var o = new R { A = /* three */ 3 };`, `main` spreads the initializer over four lines as in the third example above; after the change, no fix is offered.
- **RH5304** formats the reported member assignment of an object initializer. For

  ```csharp
          return new P
                 {
                     L = /* c */ { 1,
                     2 }
                 };
  ```

  `main` writes

  ```csharp
          return new P
                 {
                     L = /* c */ {
                                     1,
                                     2
                                 }
                 };
  ```

  and for `L = // c⏎{⏎1⏎}` it offers a fix that changes nothing; after the change, no fix is offered for either.
- **RH5111** formats the parameter list or the enum declaration around a reported parameter or enum member, so a comment behind a sibling's `=` withholds its fix. For

  ```csharp
  enum E
  {
      A =
          1,
      B = // two
          2
  }
  ```

  `main` fixes `A` to `A = 1,`; after the change, no fix is offered for `A`. For the reported construct itself, RH5111 already withholds its fix for a comment or directive around the `=` (RH5111-005).

The RH5301 and RH5303 fixes stay offered where they format only the object creation behind a commented `=`, such as `o = // c⏎new R { A = 1 };` or `List<int> L { get; } = /* a */ new List<int> { 1,⏎2 };`, and so does the RH5305 fix where it formats only the collection expression behind one: for

```csharp
class C
{
    int[] L { get; } =
        // a
        [1,
        2];
}
```

`main` and the change both write

```csharp
class C
{
    int[] L { get; } =
        // a
        [
            1,
            2
        ];
}
```

The object creation and the collection expression hold neither the operator nor the `=` (FMT-UNSUP-003).

A comment behind the `=` of a using alias withholds no code fix: the RH7201–RH7207 fixes organize a using block through an entry point that runs the using-directive phase alone, without the precheck (FMT-PIPE-030), and no other fix formats a node that holds a using directive. The RH5101 and RH5102 fixes are already withheld on `main` for a comment inside the attribute argument list they format (RH5101-004).

No rule reports a comment behind a switch arm's `=>`; RH5111 reports a value kept below its `=` by a comment, as on `main`. No rule page changes: rule pages never describe the formatter, and the general notes already cover a diagnostic without a fix.

## Special cases

### Deleted

- **FMT-BREAK-001's refusal in every join of FMT-BREAK-016 behind an assignment operator or the `=` of an equals-value clause** — the joins behind an assignment operator for a collection initializer assigned to a member, for a collection expression, and for any other value, and the joins behind the `=` of an equals-value clause for a collection expression and for any other value. Each join is reached only from the assignment expression or the equals-value clause, which holds the whole gap, so every input that reaches it holds no comment, directive, or disabled text there.
- **FMT-UNSUP-019** — its requirement, its reason, and the walk from an assignment up to the nearest switch arm or anonymous-object member that decided it.

### Checked and kept

- **FMT-BREAK-001's refusal in the joins of FMT-BREAK-016 in front of an assignment operator or the `=` of a declaration** — the gap in front of the operator is not part of the shape.
- **FMT-INDENT-034** — it still places the operand behind a comment behind a parenthesis, a later declarator, the expression behind a line break alone behind a switch arm's `=>`, and the other lines no join removes; it held no rule of its own for the lines this change refuses.
- **The column of an opening brace behind a comment that leads its line (FMT-INDENT-023)** — a complex element initializer still has such a brace; the column is read from the brace in either construct.
- **FMT-BLANK-011's blank line above a comment behind an operator** — it serves every operator and keyword, not this shape alone.
- **RH5111-005's withholding of its own fix** — it decides the reported construct by the analyzer's own guard, also for a comment in front of the `=`, which stays supported.

- **FMT-BREAK-001's refusal in the join of FMT-BREAK-015 behind the `=` of an attribute argument** — the join of a wrap inside a list element refuses a comment, a directive, or disabled text by the one check every gap inside an element goes through; the other gaps inside an element keep needing it, so FMT-UNSUP-022 deletes no code there.

For the switch arm's `=>` the declaration deletes nothing: no requirement joins the gap behind it (FMT-BREAK-041), so the line behind it is a continuation line with or without a comment. For the `=` of an attribute argument or a using alias it deletes nothing either: the join behind an attribute argument's `=` is refused by the shared check above, and no requirement joins the gap behind a using alias's `=`. Both shapes are declared on the maintainer's explicit answer, as FMT-UNSUP-019 was.

## Decisions

### Declare the shapes unsupported (maintainer, escalation 10)

The maintainer answered escalation 10 for these shapes in the run of thoenissen/Reihitsu#953 and re-confirmed it on thoenissen/Reihitsu#1926: the file stays unchanged, the CLI reports it, code fixes whose formatted node holds the shape are withheld, and the declaration is a change of its own.

### Every assignment operator (maintainer, this run)

The maintainer chose to refuse a comment, a directive, or disabled text behind every assignment operator — `=` and every compound operator — wherever the assignment stands, a property initializer's `=` included. FMT-UNSUP-019, which refused every assignment operator inside a switch arm or an anonymous-object member, is folded into FMT-UNSUP-020 and retired, and its input is reported with FMT-UNSUP-020's reason.

### A deconstruction declaration counts (maintainer, this run)

The maintainer chose to refuse `var (a, b) = // c⏎t;`: the syntax tree writes a deconstruction as an assignment expression, whether its left side declares the variables or not.

### The `=` of an enum member, a parameter, and a variable declarator (maintainer, this run)

The maintainer chose to declare the remaining gaps behind an `=` in this work, not in a change of its own: a comment, a directive, or disabled text behind the `=` of an enum member or of a parameter's default value, and a directive or disabled text behind the `=` of a variable declarator, where FMT-UNSUP-004 already refuses a comment.

### The `=` of an attribute argument and of a using alias (maintainer, this run)

Asked whether a comment, a directive, or disabled text behind the `=` of an attribute argument or of a using alias stays supported, the maintainer answered that both are unsupported, declared in this work. The run had recommended keeping them supported because the declaration deletes no special case; the maintainer's answer decides it, as for the switch arm's `=>`.

### Two reasons for the `=` tokens (delegated by the maintainer)

The maintainer left the wording of the CLI reason to the run and allowed two messages. FMT-UNSUP-020 keeps `a comment or directive behind an assignment operator or the = of a declaration`, which covers the assignment operators and the `=` of a property, a variable declarator, an enum member, and a parameter; FMT-UNSUP-022 gets `a comment or directive behind the = of an attribute argument or a using alias`. Each message names what the user wrote: to a user, the `=` of an attribute argument or of a using alias names an argument or a type, and is neither an assignment operator nor the `=` of a declaration.

### The members of an initializer count as assignments

The members of an object initializer, of a `with` initializer, and of an index initializer are assignment expressions in the syntax tree; excluding them would need a condition on the assignment's parent and would leave the same comment-held line supported in one syntax and refused in the other.

### An equals-value clause holds the whole gap

The `=` belongs to the equals-value clause, which holds the value as well, so a node-level input that holds the clause holds the shape, also without its property, declarator, enum member, or parameter. No caller formats an equals-value clause on its own: RH5111 formats the declaration, parameter list, or enum declaration around it, and RH5303 formats the object creation. Counting the clause by itself lets every join behind an `=` drop its refusal. The examples that formatted a clause with a comment behind its `=` as a node-level target (FMT-UNSUP-003, FMT-PIPE-017) take the object creation behind the `=` as target instead, and SHARED-FIX-010's RH5303 example names the node that fix formats, the object creation.

### Three requirements

The gap behind an assignment operator or the `=` of a declaration, the gap behind an arm's `=>`, and the gap behind the `=` of an attribute argument or a using alias are written as three requirements with three reasons, so the CLI's line names the token the comment stands behind, as the reasons of the other shapes do. FMT-UNSUP-020's reason, `a comment or directive behind an assignment operator or the = of a declaration`, names the property, the variable declarator, the enum member, and the parameter by what they share.

### On any line

As for every other shape of the unsupported-syntax spec, the boundary is a place in the syntax tree, so a comment behind the operator, the `=`, or the `=>` on one line is refused like one with a line break.

### Listed after the shapes that refuse the same trivia

FMT-UNSUP-020 is listed after FMT-UNSUP-004 through FMT-UNSUP-018, FMT-UNSUP-021 after FMT-UNSUP-020, and FMT-UNSUP-022 last, so a comment behind a variable declarator's `=` or behind the `=` of an attribute argument of a field keeps FMT-UNSUP-004's reason and a directive behind such an `=` in a field with several declarators keeps FMT-UNSUP-010's, as on `main`.

## Alternatives considered

- **Keep the continuation indent for these lines** — rejected by the maintainer (escalation 10).
- **Only the `=` of a simple assignment, with the compound operators supported outside an arm or a member** — rejected by the maintainer in this run; it would have kept FMT-UNSUP-019 and the refusals in the joins behind an assignment operator.
- **Treat a deconstruction declaration as a declaration that FMT-UNSUP-004 does not cover** — rejected by the maintainer in this run.
- **Declare the `=` of an enum member, a parameter, and a variable declarator in a change of its own, or not at all** — rejected by the maintainer in this run.
- **Keep the `=` of an attribute argument and of a using alias supported** — recommended by the run, because the declaration deletes no special case; rejected by the maintainer in this run.
- **One reason for every `=`** — not chosen: `the = of a declaration` does not describe an attribute argument or a using alias, and a reason naming every owner would be long; the maintainer allowed two messages.
- **Count an equals-value clause only where its property, declarator, enum member, or parameter is part of the input** — not chosen: it would keep the refusals in both joins behind an equals-value clause's `=` for an input no caller passes.
