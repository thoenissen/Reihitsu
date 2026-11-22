# The member-moving fixes refuse every conditional group in the moved and the crossed span

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | SHARED-ORDER-008 (changed: every conditional directive in either span refuses the move; the whole-unit test is retired), SHARED-ORDER-007 (changed: no member of excluded code is counted, because excluded code in either span already refuses the move), SHARED-ORDER-006 (changed: the `#if DEBUG` body example no longer keeps a member move), RH7102-006, RH7103-007, RH7109-006, RH7309-005 (changed: the whole-unit groups no longer keep the fix). |

## What

This doc waits for the maintainer's answer to the output questions of thoenissen/Reihitsu#1668; the spec diff is written once they have answered.

Recommended, and to be decided by the maintainer: the member reordering fixes — RH7102, RH7103, RH7109, and RH7309 — no longer offer a move when the moved span or the crossed span (SHARED-ORDER-003) holds any `#if`, `#elif`, `#else`, or `#endif`, whatever the group wraps. Today they keep a move when the group wraps whole units — whole members, whole attribute lists, whole accessors, or whole statements of a member's body — and refuse it otherwise (SHARED-ORDER-008).

With every group in the spans refused, no member of excluded code can move or be crossed, so SHARED-ORDER-007 no longer counts members of excluded code. The type-layout clause of SHARED-ORDER-008 changes with it: today a conditional group in the leading trivia of a declaration's first token, or in the reordered body in front of the spans, makes the type count as one whose field order sets its layout only when an excluded branch does not read as whole units. Recommended: such a group counts whatever it holds, except a group whose only branch holds the whole declaration (question 2 below).

The accessor fixes (RH7107, RH7108) are not changed: they move accessors, which carry no initializer and no field order, and SHARED-ORDER-008 never applied to them. RH2001's fix keeps its own reading of excluded members (RH2001-010).

This change covers thoenissen/Reihitsu#1668.

## Why

The maintainer's triage on thoenissen/Reihitsu#1668 sets the criterion: if checking precisely which conditional blocks matter is not a trivial check, the fixes refuse any conditional block in the moved span instead (AGENTS.md, *Special cases do not grow the source*).

The check on `main` is not trivial. For each group in the spans, `OrderingDeclarationUtilities` and `ConditionalGroupUnitUtilities` decide:

- at the boundary of which of four kinds of unit list each directive stands — type-body members, a member's attribute lists, an accessor list, or the statements of a block or switch section in a body, with lambdas and local functions included and field initializers and expression bodies excluded;
- whether another group nests in or around it;
- whether each excluded branch, parsed on its own as the content of that list, reads as whole units without a syntax error;
- for accessor lists and accessor bodies, whether a branch holds the `field` keyword, and whether a branch holds an accessor without a body while another accessor has one;
- and, to count initializers and layout fields, the members that each excluded branch in the type body declares, from a second parse.

That is a parser-like reading of excluded text, which *Special cases do not grow the source* names as machinery a rare input does not justify. The maintainer recorded on thoenissen/Reihitsu#1668 that the simpler rule costs fixes `main` offers, so the cost is put to the maintainer as output questions; the decision is theirs, not this run's.

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

After (recommended): RH7103 is reported on `Create`, and no code fix is offered.

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
- the same groups in a region block, or in the code between blocks, that RH7309 reorders.

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

After (recommended): RH7109 is reported on `_b`, and no code fix is offered.

A group whose only branch holds the whole type declaration keeps the move under the recommendation, as on `main`:

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

A group in the leading trivia of the type declaration's first token with an `#else` or `#elif`, or with both directives in front of the declaration, makes the type count as one whose field order sets its layout under the recommendation, where `main` lets a branch of whole members of another name through.

### Code fixes that format the affected node

None besides the fixes named above. The moves are text moves of whole lines (SHARED-ORDER-009); no formatter output changes.

## Decisions

Pending: the maintainer answers the output questions in `sdd/work/ordering-move-directives/01-spec-author.md` (thoenissen/Reihitsu#1668). This section records the answer and its consequences once given.

## Alternatives considered

- **Keep `main`'s whole-unit test** — not recommended: it is the parser-like reading of excluded text that the maintainer's criterion rules out; it stays an option in the output question.
- **Keep only groups without an excluded branch** — a group whose only branch is active, at the boundary of one unit list, reads no excluded text; not recommended: it still needs the place test over four kinds of unit list and the `field` and backing-field clauses, and the fix would be offered in a build that defines `DEBUG` and withheld in one that does not, for the same file; it stays an option in the output question.
- **Refuse groups in the spans but keep the parse for the type-layout clause** — not recommended: the parse and `ConditionalGroupUnitUtilities`' list reading would stay for groups in front of the spans alone; it stays an option in question 2.
- **Count every group in front of the spans and around the type, with no exception** — rejected as the recommendation: a type wrapped whole in one `#if`, common in code built for several targets, would lose every instance-field move; telling that group apart is a check of where its two directives stand.
