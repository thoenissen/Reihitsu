# A base list wrapped behind its colon joins the colon's line

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-042 (new), FMT-INDENT-036 (new), FMT-BREAK-038, FMT-BREAK-041, the indentation spec's non-goals |
| Depends on | [continuation-indent.md](continuation-indent.md), [continuation-join.md](continuation-join.md) |

## What

A first base type that stands on the line below its base list's `:` joins the colon's line (FMT-BREAK-042), for the base list of every type declaration, a base type with an argument list included. The later base types keep their line breaks and align under the first (FMT-INDENT-016).

Where a comment, a directive, or disabled text behind the colon keeps the first base type below it, the base types that start a line take the column behind the colon and one space, the column the first base type has when it stands behind the colon (FMT-INDENT-036).

A comma that starts a line in a base list moves behind the base type in front of it, as in an initializer or a collection expression (FMT-BREAK-038). The indentation spec drops the base list from its non-goal on comma columns: no line of a base list starts with a comma any more.

This change covers thoenissen/Reihitsu#1633 and the comma-first shape recorded on it.

## Why

On `main`, FMT-BREAK-041 joins the gap in front of a base list's `:` and names the gap behind it as one it does not join. A first base type wrapped behind the colon is therefore a continuation line and gets the continuation indent of FMT-INDENT-034, four columns right of the declaration's line, while the later base types of the same list align under it (FMT-INDENT-016). A wrapped `where` clause, the other part of a type's header that starts with a colon, joins its first constraint onto the colon's line and aligns a comment-held first constraint behind the colon (FMT-BREAK-037, FMT-INDENT-032, maintainer answers Q2 and Q3 of [constraint-clause-wrap.md](constraint-clause-wrap.md)); the base list is laid out the same way.

The base-list comma that starts a line was left to a non-goal: it lands at the continuation column, left of the base types it separates. Every other list that a comma-first author writes — argument, parameter, and attribute argument lists, initializers, collection expressions, list and property patterns — moves such a comma behind the element in front of it (FMT-BREAK-011, FMT-BREAK-012, FMT-BREAK-038).

## Visible output changes

Every example is whole-document formatting; "Before" is the output on `main`, observed with `scripts/format.sh`, and stable on a second pass.

### 1. A base list wrapped behind its colon

Input:

```csharp
interface I1;

interface I2;

class C :
    I1, I2
{
    int _x;
}

class D :
    I1,
    I2
{
    int _x;
}

enum E :
    int
{
    A,
}

record R(int X) :
    Q(X);

record Q(int X);
```

Before: the input, with `A,` written `A` (FMT-STRUCT); the base types stay in column 4.

After:

```csharp
interface I1;

interface I2;

class C : I1, I2
{
    int _x;
}

class D : I1,
          I2
{
    int _x;
}

enum E : int
{
    A
}

record R(int X) : Q(X);

record Q(int X);
```

### 2. A first base type that a comment keeps below the colon

Input and before (the base types stay in column 4):

```csharp
interface I1;

interface I2;

class C : // c
    I1
{
    int _x;
}

class D : // c
    I1, // d
    I2
{
    int _x;
}
```

After:

```csharp
interface I1;

interface I2;

class C : // c
          I1
{
    int _x;
}

class D : // c
          I1, // d
          I2
{
    int _x;
}
```

### 3. A comma that starts a line in a base list

Input and before (`, I2` stays in column 4):

```csharp
interface I1;

interface I2;

class C : I1
    , I2
{
    int _x;
}
```

After:

```csharp
interface I1;

interface I2;

class C : I1,
          I2
{
    int _x;
}
```

### Code fixes

The fixes that format a type declaration through the formatter give its base list the new layout. RH5412, RH5413, RH5414, RH5415, RH5416, and RH5418 (an empty type rewritten as a semicolon declaration) are the ones found; RH5412 on `class C :⏎    I1⏎{⏎}` gives `class C :⏎    I1;` on `main` (observed with `scripts/apply-fix.sh`) and `class C : I1;` after this change.

No rule page changes: no rule page shows a base list wrapped behind its colon or a comma-first base list. RH5043 reports a comment inside a base list and is unchanged.

## Decisions

### The first base type joins the colon's line

Pending the maintainer's answer. The recommendation joins it, as the first constraint of a `where` clause is joined (constraint-clause-wrap.md, Q2).

### A comment-held first base type takes the column behind the colon

Pending the maintainer's answer. The recommendation takes the column behind the colon and one space, as the constraints do (constraint-clause-wrap.md, Q3) and as an argument list whose first argument a comment pushes down does (FMT-INDENT-012).

### A comma-first base list ends the base type's line

Pending the maintainer's answer. The recommendation moves the comma, as every other list does.

### Only the first base type is joined

The later base types keep the line breaks their author wrote: no requirement lays a base list out one type per line, and the issue asks only for the gap behind the colon. They align under the first base type (FMT-INDENT-016) wherever it stands.

## Alternatives considered

- **Keep the wrap behind the colon and the continuation indent** (the issue's reading B, today's output) — not recommended: the first base type stands four columns right of the declaration while a wrapped `where` clause joins its first constraint onto the colon's line.
- **Keep a comma that starts a line and align it with the base types** — not recommended: every other list moves such a comma behind its element, and a comma alone at the start of a line belongs to neither base type.
- **Join every base type onto the colon's line** — not proposed: no requirement or issue asks to remove the line breaks between base types, and long base lists are wrapped on purpose.
