# A base list wrapped behind its colon joins the colon's line

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-042 (new), FMT-BREAK-038, FMT-BREAK-041, FMT-INDENT-034 (clarified), FMT-INDENT-036, FMT-INDENT-037, FMT-INDENT-038 (issued and retired in this change), the indentation spec's non-goals |
| Depends on | [continuation-indent.md](continuation-indent.md), [continuation-join.md](continuation-join.md) |

## What

A first base type that stands on the line below its base list's `:` joins the colon's line (FMT-BREAK-042), for the base list of every type declaration, a base type with an argument list included. The later base types keep their line breaks and align under the first (FMT-INDENT-016).

Where a comment, a directive, or disabled text behind the colon keeps the first base type below it, nothing changes: the first base type is a continuation line (FMT-INDENT-034), and the later base types align under it (FMT-INDENT-016). FMT-INDENT-034 now states that it owns such lines, and the same for the lines a comment keeps below the opening bracket of an attribute, type argument, type parameter, or function-pointer parameter list and inside a qualified name.

A comma that starts a line in a base list moves behind the base type in front of it, as in an initializer or a collection expression (FMT-BREAK-038). The indentation spec drops the base list from its non-goal on comma columns: no line of a base list starts with a comma any more.

This change covers thoenissen/Reihitsu#1633 and the comma-first shape recorded on it. The statement in FMT-INDENT-034 answers thoenissen/Reihitsu#1634, which asked which requirement owns the comment-held lines; it changes no output.

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
    A
}

record R(int X) :
    Q(X);

record Q(int X);
```

Before: the input, unchanged; the base types stay in column 4.

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

### 2. A comma that starts a line in a base list

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

Not changed: a first base type that a comment keeps below the colon (`class C : // c⏎    I1`) stays in column 4, as on `main`.

## Decisions

### The first base type joins the colon's line

The maintainer chose A for question 1: the first base type joins the colon's line, as the first constraint of a `where` clause is joined (constraint-clause-wrap.md, Q2). A comment behind the colon keeps today's layout (FMT-BREAK-042).

### Lines a comment keeps below a colon, a bracket, or a dot keep the continuation indent

Decided in the run, since nothing visible changes: a first base type that a comment keeps below the colon, a first element that a comment keeps below the opening bracket of an attribute, type argument, type parameter, or function-pointer parameter list, and a line that a comment keeps inside a qualified name stay continuation lines (FMT-INDENT-034). Each such comment is reported by another rule — RH5043, RH5044, RH5046, or RH5104 — or stands where code hardly ever holds one, so the class is rare, and a column of its own would be a placement rule for that class alone. FMT-INDENT-036, FMT-INDENT-037, and FMT-INDENT-038, drafted for those columns, were retired before they shipped; FMT-INDENT-038 would also have moved qualified names wrapped without a comment, in casts, `typeof`, `default`, using aliases, and namespace names.

### A comma-first base list ends the base type's line

The maintainer chose A for question 3: the comma moves behind the base type in front of it, as every other list does (FMT-BREAK-038), and the next base type aligns under the first (FMT-INDENT-016).

### Only the first base type is joined

The later base types keep the line breaks their author wrote: no requirement lays a base list out one type per line, and the issue asks only for the gap behind the colon. They align under the first base type (FMT-INDENT-016) wherever it stands.

## Alternatives considered

- **Give a comment-held first base type the column behind the colon** (FMT-INDENT-036 as drafted), and a comment-held first list element the column behind its bracket, and a qualified name's right part a column measured from the name (FMT-INDENT-037, FMT-INDENT-038) — not taken: each serves a rare class only.
- **Declare a comment behind a base list's colon unsupported** — not an option: with the comment-held column dropped, the shape deletes no special case.

- **Keep the wrap behind the colon and the continuation indent** (the issue's reading B, today's output) — rejected by the maintainer: the first base type stands four columns right of the declaration while a wrapped `where` clause joins its first constraint onto the colon's line.
- **Keep a comma that starts a line and align it with the base types** — rejected by the maintainer: every other list moves such a comma behind its element, and a comma alone at the start of a line belongs to neither base type.
- **Join every base type onto the colon's line** — not proposed: no requirement or issue asks to remove the line breaks between base types, and long base lists are wrapped on purpose.
