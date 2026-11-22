# The member-moving fixes refuse every conditional group in the moved and the crossed span

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | SHARED-ORDER-008 (changed: every conditional directive in either span refuses the move, and every conditional directive in front of the spans or of a declaration of the type makes the type count as one whose field order sets its layout, except a group that wraps the whole declaration; the whole-unit test is retired), SHARED-ORDER-007 (changed: no member of excluded code is counted, because excluded code in either span already refuses the move), SHARED-ORDER-006 (changed: its conditional-pair example and *Why* hold for the accessor fixes; a member move keeps only region pairs and SHARED-ORDER-011 pairs), RH7102-006, RH7103-007, RH7309-005 (changed: the whole-unit groups no longer keep the fix), SHARED-FIX-009 (clarified: its excluded-members example points to RH2001-010 instead of SHARED-ORDER-007). |

## What

The member reordering fixes — RH7102, RH7103, RH7109, and RH7309 — no longer offer a move when the moved span or the crossed span (SHARED-ORDER-003) holds any `#if`, `#elif`, `#else`, or `#endif`, whatever the group wraps. Today they keep a move when the group wraps whole units — whole members, whole attribute lists, whole accessors, or whole statements of a member's body — and refuse it otherwise (SHARED-ORDER-008).

With every group in the spans refused, no member of excluded code can move or be crossed, so SHARED-ORDER-007 no longer counts members of excluded code. The type-layout clause of SHARED-ORDER-008 changes with it: today a conditional group in the leading trivia of a declaration's first token, or in the reordered body in front of the spans, makes the type count as one whose field order sets its layout only when an excluded branch does not read as whole units. Now such a group counts whatever it holds, except a group of an `#if` and its `#endif`, with no `#elif` and no `#else`, that wraps the whole declaration. A conditional directive in a declaration in front of its body counts as on `main`.

The accessor fixes (RH7107, RH7108) are not changed: they move accessors, which carry no initializer and no field order, and SHARED-ORDER-008 never applied to them. RH2001's fix keeps its own reading of excluded members (RH2001-010), which is now the only reading of excluded branches as members among the code fixes.

This change covers thoenissen/Reihitsu#1668.

## Why

The maintainer's triage on thoenissen/Reihitsu#1668 sets the criterion: if checking precisely which conditional blocks matter is not a trivial check, the fixes refuse any conditional block in the moved span instead (AGENTS.md, *Special cases do not grow the source*).

The check on `main` is not trivial. For each group in the spans, `OrderingDeclarationUtilities` and `ConditionalGroupUnitUtilities` decide:

- at the boundary of which of four kinds of unit list each directive stands — type-body members, a member's attribute lists, an accessor list, or the statements of a block or switch section in a body, with lambdas and local functions included and field initializers and expression bodies excluded;
- whether another group nests in or around it;
- whether each excluded branch, parsed on its own as the content of that list, reads as whole units without a syntax error;
- for accessor lists and accessor bodies, whether a branch holds the `field` keyword, and whether a branch holds an accessor without a body while another accessor has one;
- and, to count initializers and layout fields, the members that each excluded branch in the type body declares, from a second parse.

That is a parser-like reading of excluded text, which *Special cases do not grow the source* names as machinery a rare input does not justify. The simpler rule costs fixes `main` offers for common code, such as a member whose body holds an `#if DEBUG` block; the maintainer confirmed that cost (*Decisions*).

## Visible output changes

### 1 · A conditional group in the moved or the crossed span (RH7102, RH7103, RH7109, RH7309)

A group of whole statements in a crossed member's body, for RH7103 (SHARED-ORDER-006's example):

```csharp
public class C
{
    public void Run()
    {
#if DEBUG
        System.Console.WriteLine();
#endif
    }

    public static void Create()
    {
    }
}
```

Before (`main`):

```csharp
public class C
{
    public static void Create()
    {
    }

    public void Run()
    {
#if DEBUG
        System.Console.WriteLine();
#endif
    }
}
```

After: RH7103 is reported on `Create`, and no code fix is offered.

The fix is withheld the same way, where `main` offers it, for:

- a group of whole members in the moved member's leading trivia (SHARED-ORDER-008's first example):

  ```csharp
  public class C
  {
      public void Run()
      {
      }

  #if DEBUG
      public void Trace()
      {
      }
  #endif
      public static void Create()
      {
      }
  }
  ```

- a group of whole attribute lists on the moved member, such as `#if DEBUG` / `[System.Obsolete]` / `#endif` in front of `public static void Create()`;
- a group of whole accessors, such as `#if DEBUG` / `set { }` / `#endif` behind `get;`, on a moved or a crossed property;
- a group of whole members in the body of a crossed nested type;
- a group of whole members in front of a constant that RH7102 moves (RH7102-006's example), and in front of a field that RH7109 moves, where `main` moved the excluded code along when no crossed member had an initializer;
- the same groups in a region block or in the code between blocks that RH7309's reorder moves a block across — a block whose place the fix changes, and also a block that keeps its place while other blocks are moved across it, such as `B` in blocks `A B C` reordered to `C B A` (RH7309-005's examples). A group in a block, or in code between blocks, that no block is moved across keeps the fix, as on `main`, such as one between `B` and `C` in blocks `A B C D` reordered to `B A D C`.

A conditional group outside both spans keeps the move, as on `main`.

### 2 · A conditional group in front of the spans or around the type (RH7109, RH7309)

Only moves that reorder instance fields of a class are affected; a struct counts as one whose field order sets its layout already, and a move that reorders no instance field stays available. A group of whole members in the body in front of the spans, for RH7109:

```csharp
public class C
{
#if DEBUG
    private int _trace;
#endif
    public void Run()
    {
    }

    private int _a;
    private readonly int _b;
}
```

Before (`main`):

```csharp
public class C
{
#if DEBUG
    private int _trace;
#endif
    public void Run()
    {
    }

    private readonly int _b;
    private int _a;
}
```

After: RH7109 is reported on `_b`, and no code fix is offered.

A group whose only branch holds the whole type declaration keeps the move, as on `main`:

```csharp
namespace N
{
#if !DEBUG
    public class C
    {
        private int _a;
        private readonly int _b;
    }
#endif
}
```

```csharp
namespace N
{
#if !DEBUG
    public class C
    {
        private readonly int _b;
        private int _a;
    }
#endif
}
```

A group in the leading trivia of the type declaration's first token with an `#else` or `#elif`, or with both directives in front of the declaration, makes the type count as one whose field order sets its layout, where `main` lets a branch of whole members of another name through. For RH7109, without `DEBUG` defined:

```csharp
#if DEBUG
public class Trace
{
}
#endif
public class C
{
    private int _a;
    private readonly int _b;
}
```

Before (`main`): `_b` is moved in front of `_a`.

After: RH7109 is reported on `_b`, and no code fix is offered. The same holds for the whole-class wrap above with an `#else` branch in front of its `#endif`.

The same reading applies to every other declaration of a `partial` type that the analyzed build compiles.

### Code fixes that format the affected node

None besides the fixes named above. The moves are text moves of whole lines (SHARED-ORDER-009); no formatter output changes.

## Decisions

The maintainer answered the output questions of this run (`sdd/work/ordering-move-directives/01-spec-author.md`) as recommended:

- **1668-1: B.** Any conditional directive in the moved or the crossed span withholds the fixes of RH7102, RH7103, RH7109, and RH7309; the excluded-text parse is dropped for these fixes.
- **1668-2: A.** Any conditional directive in the class in front of the moved fields, or in front of the class declaration, withholds a fix that reorders the class's instance fields. The exception that keeps `main`'s result is an `#if` without `#else` that wraps the whole class.

### The criterion is met

On `main`, the check of which groups are harmless reads every excluded branch as members, attribute lists, accessors, or statements, at the boundary of one of four kinds of unit list, with clauses for nesting, the `field` keyword, and accessors without a body, and parses the type-body branches a second time to count their members. By the maintainer's triage criterion, that is not a trivial check, so the fixes refuse every group in the spans instead.

### Where the type-layout check reads, stated by the token that holds the directive

Each of the three places is a token range of the syntax tree: the tokens of a declaration of the type behind its first token up to its body's opening brace (or its `;`), the declaration's first token, and the reordered body between its opening brace and the start of the spans. A group elsewhere in the document — around another type, not reaching the leading trivia of a declaration of the reordered type — is not read, as on `main`. "In front of the class declaration" in the answer to 1668-2 is read as the leading trivia of the declaration's first token, the place `main` already read, applied to every declaration of a `partial` type as `main`'s type-layout clause was.

### The exception is a check of where the group's two directives stand

A group wraps the whole declaration when it is an `#if` and its `#endif` with nothing between them but one branch, the `#if` in the leading trivia of the declaration's first token, and the `#endif` in the leading trivia of the token behind the declaration's last token. No branch is read.

### RH2001-010 keeps its reading

RH2001's fix reads excluded branches next to a property as whole members (RH2001-010). The issue is about the member-moving fixes; the same criterion applied to RH2001 is a follow-up, not part of this change.

## Alternatives considered

- **Keep `main`'s whole-unit test** (1668-1 A) — rejected by the maintainer: it is the parser-like reading of excluded text that the triage criterion rules out.
- **Keep only groups without an excluded branch** (1668-1 C) — rejected by the maintainer: a group whose only branch is active reads no excluded text, but it still needs the place test over four kinds of unit list and the `field` and backing-field clauses, and the fix would be offered in a build that defines `DEBUG` and withheld in one that does not, for the same file.
- **Refuse groups in the spans but keep the parse for the type-layout clause** (1668-2 B) — rejected by the maintainer: the parse and the list reading would stay for groups in front of the spans alone.
- **Count every group in front of the spans and around the type, with no exception** — rejected as the recommendation: a type wrapped whole in one `#if`, common in code built for several targets, would lose every instance-field move; telling that group apart is a check of where its two directives stand.
