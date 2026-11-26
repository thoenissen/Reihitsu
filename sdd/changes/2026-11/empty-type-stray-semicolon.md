# An empty type body followed by a semicolon is reported and converted

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | SHARED-EMPTY-001, SHARED-EMPTY-005, FMT-STRUCT-017, FMT-STRUCT-018 |
| Depends on | [Rules judge the code the current build compiles](report-across-conditional-directives.md) |

## What

C# allows a semicolon behind the closing brace of a type declaration, as in `class C { };`. The compiler makes it the declaration's own semicolon token, so the declaration has a brace body and a semicolon at once. The empty-type rules RH5412, RH5413, RH5414, RH5415, RH5416, and RH5418 and the formatter's empty-type conversion treated every declaration that has a semicolon as a semicolon declaration already, and left `class C { };` alone.

Now such a declaration is an empty brace body like any other:

- SHARED-EMPTY-001 reports it, whether or not a semicolon follows the closing brace.
- FMT-STRUCT-017 converts it, and the result has one semicolon: the braces and the semicolon behind them become the semicolon of the declaration (SHARED-EMPTY-005 for the code fix). A comment behind that semicolon follows the new one.
- FMT-STRUCT-018 also keeps the braces, and the semicolon behind them, when a comment, directive, or disabled text lies between the closing brace and the semicolon. SHARED-EMPTY-004 then withholds the fix; the declaration stays reported.

## Why

The maintainer decided the question under principle P11 (#912, question 2 of the empty-type spec questions): `class C { };` is the construct the rules name, an empty type with a brace body, and skipping it is the rules' own gap. Expected: reported, and the fix writes `class C;`. The stray semicolon says nothing — behind the braces it declares no member, and behind a semicolon declaration it would be a second semicolon, which does not compile — so the conversion drops it into the one semicolon it writes.

A comment, directive, or disabled text between the closing brace and the semicolon has no place in `class C;`: it would have to move in front of the semicolon or behind it, which changes what it stands next to. The braces stay, as they do for a comment inside the body (SHARED-FIX-003).

## Visible output changes

"Before" is today's output on `main`. Every input is analyzed and formatted with a C# version that allows the semicolon form of its kind.

### RH5412 to RH5416 and RH5418 — the empty body followed by a semicolon is reported

```csharp
class {|RH5412:A|}
{
};

struct {|RH5413:B|}
{
};

interface {|RH5414:I|}
{
};

record {|RH5415:R|}(int Value)
{
};

record struct {|RH5416:S|}
{
};

union {|RH5418:U|}(int, string)
{
};
```

Before: not reported.

After: reported on the name, as marked.

### The code fix writes one semicolon

```csharp
class A
{
};

class B { };

class C
{
}
;

class D
{
}; // note
```

Before: not reported, no fix.

After (RH5412's fix; RH5413 to RH5416 and RH5418 alike):

```csharp
class A;

class B;

class C;

class D; // note
```

### A comment between the closing brace and the semicolon withholds the fix

```csharp
class A
{
} /* note */;

class B
{
} // note
;
```

Before: not reported.

After: both reported, and no code fix is offered.

### The formatter converts the declaration

Input, for `reihitsu-format`:

```csharp
class A
{
};

class B { };

class C
{
}
;

class D
{
}; // note
```

Before:

```csharp
class A
{
};

class B
{
};

class C
{
}
    ;

class D
{
}; // note
```

After:

```csharp
class A;

class B;

class C;

class D; // note
```

A comment between the closing brace and the semicolon keeps the output as it is today:

```csharp
class A
{
} /* note */;
```

is left unchanged before and after.

### Other code fixes that format a declaration holding such a type

A code fix that formats a node through the formatter converts an empty type with a semicolon behind its braces inside that node, as it already converts one without. RH5404 (Element must not be on a single line), for example:

```csharp
class {|RH5404:H|} { class I { }; }
```

Before:

```csharp
class H
{
    class I
    {
    };
}
```

After:

```csharp
class H
{
    class I;
}
```

The rule pages of RH5412 to RH5416 and RH5418 do not change: they already say to replace the empty brace body with a semicolon, which covers this form.

## Decisions

### The issue's directive goals are already decided

Issue #1275 also asked that a body holding a conditional directive or excluded code not be reported, and that the RH5416 test `VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix` change. The later change [Rules judge the code the current build compiles](report-across-conditional-directives.md) reversed that: SHARED-EMPTY-001 and SHARED-DIAG-008 report such a body, and SHARED-EMPTY-004 withholds the fix. A `#region` or `#pragma` in the body is reported without a fix, as the issue asks and as the specs already state. This change leaves all of it as it is.

### Every rule of the shared spec, unions included

The issue names RH5412 to RH5416. RH5418 uses the same shared spec and the same check, and `union U(int) { };` is the same gap, so the change covers it too.

### A stray semicolon behind a type with members is not in scope

`class C { int _x; };` is not an empty type. Whether its semicolon should be reported is a different construct, which no rule names; this change leaves it as written.

## Alternatives considered

- **Keep the stray semicolon behind the new one** (`class C;;`) — rejected: it does not compile at namespace level, and the second semicolon says nothing.
- **Move a comment between the closing brace and the semicolon behind the new semicolon** — rejected: it relocates a user comment (SHARED-FIX-003), and the input is rare; the braces stay and the diagnostic is reported without a fix.
