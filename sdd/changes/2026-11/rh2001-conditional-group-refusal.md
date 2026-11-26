# The RH2001 fix refuses every conditional group in front of the property or behind it

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH2001-010 (changed: any `#if`, `#elif`, `#else`, or `#endif` in the leading trivia of the first modifier, or of the token behind a property without an initializer, withholds the fix; the whole-member test and its nesting clause are retired), RH2001-004 (clarified: its coverage sentence and its sentence on the directives above the first modifier point to the new RH2001-010), RH2001-009 (clarified: its pointer to RH2001-010, and its last example, which showed the fix kept for a group of whole members, is removed), SHARED-FIX-009 (clarified: no fix reads an excluded branch as members any more; its RH2001 references and example now say so) |
| Depends on | [The member-moving fixes refuse every conditional group in the moved and the crossed span](ordering-move-refuses-conditional-groups.md) |

## What

The RH2001 code fix is no longer offered when an `#if`, `#elif`, `#else`, or `#endif` directive stands in the leading trivia of the first modifier of a property that has a modifier, or, for a property without an initializer, in the leading trivia of the token behind the property — whatever the directive's group holds. Today the fix is withheld there only when the group does not wrap whole members: each excluded branch is parsed on its own as the content of a type body, and a group whose excluded branches all read as whole member declarations, with no other group inside them, keeps the fix (RH2001-010).

The diagnostic stays. A directive in the leading trivia of any other token keeps the fix as today, such as a group in front of an earlier member, or a group that wraps the whole type. A group behind a property with an initializer keeps the fix as today. A property without a modifier is withheld for a conditional directive in front of its type already (RH2001-009) and is unchanged.

This change covers thoenissen/Reihitsu#1943.

## Why

The member-moving ordering fixes (RH7102, RH7103, RH7109, RH7309) refuse every conditional group in their spans and no longer read excluded branches as units ([ordering-move-refuses-conditional-groups.md](ordering-move-refuses-conditional-groups.md)), because that reading is not a cheap check (`AGENTS.md`, *Special cases do not grow the source*). RH2001-010 is the last place among the code fixes that parses excluded text to tell harmless groups from harmful ones. The same criterion applies: a conditional group next to a private auto-property is a rare shape, and reading its excluded branches as code is the parser-like machinery that a rare input does not justify. Where the directive stands — the leading trivia of one token — is a cheap syntactic check, and it is the check RH2001-009 already uses for a property without a modifier.

## Visible output changes

### 1 · A conditional group of whole members in front of the first modifier

Input, without `DEBUG` defined:

```csharp
class C
{
#if DEBUG
    private int _trace;
#endif
    static private int Value { get; }

    int Read() => Value;
}
```

Before (`main`):

```csharp
class C
{
#if DEBUG
    private int _trace;
#endif
    private static readonly int _value;

    int Read() => _value;
}
```

After: RH2001 is reported on `Value`, and no code fix is offered.

```csharp
class C
{
#if DEBUG
    private int _trace;
#endif
    static private int {|RH2001:Value|} { get; }

    int Read() => Value;
}
```

The fix is withheld the same way, where `main` offers it, for:

- a group whose active branch holds the whole member and whose `#endif` alone stands in front of the first modifier, such as `#if !DEBUG` / `private int _trace;` / `#endif` above `private int Value { get; }`, without `DEBUG` defined (`main`: `private readonly int _value;`);
- a group whose excluded `#if` branch holds a whole member in front of the member of its active `#else` branch;
- a group whose excluded branch holds only a comment;
- a group of a whole member in front of the first modifier inside the active branch of a group that wraps the file, such as `#if NET` … `#if DEBUG` / `private int _trace;` / `#endif` / `private int Value { get; }` … `#endif`.

### 2 · A conditional group of whole members behind a property without an initializer

Input, without `DEBUG` defined:

```csharp
class C
{
    private int Value { get; }
#if DEBUG
    private int _trace;
#endif

    int Read() => Value;
}
```

Before (`main`):

```csharp
class C
{
    private readonly int _value;
#if DEBUG
    private int _trace;
#endif

    int Read() => _value;
}
```

After: RH2001 is reported on `Value`, and no code fix is offered.

The fix is withheld the same way when only the group's `#endif` stands behind the property, and its `#if` in front of an earlier member — a group around several members with the property last, as multi-targeting code writes it. Input, with `NET` defined:

```csharp
#define NET
class C
{
#if NET
    private int _a;

    private int B { get; }
#endif

    int Read() => _a + B;
}
```

Before (`main`):

```csharp
#define NET
class C
{
#if NET
    private int _a;

    private readonly int _b;
#endif

    int Read() => _a + _b;
}
```

After: RH2001 is reported on `B`, and no code fix is offered.

```csharp
#define NET
class C
{
#if NET
    private int _a;

    private int {|RH2001:B|} { get; }
#endif

    int Read() => _a + B;
}
```

This is the mirror of the `#endif` alone in front of the first modifier in section 1. A property in the middle of such a group, with a member in front of it and one behind it inside the group, keeps the fix (*Unchanged*): no directive of the group stands next to it.

### 3 · A conditional group that wraps the property itself

Input, with `NET` defined — a property without an initializer, as multi-targeting code writes it:

```csharp
#define NET
class C
{
#if NET
    private int Value { get; }
#endif

    int Read() => Value;
}
```

Before (`main`):

```csharp
#define NET
class C
{
#if NET
    private readonly int _value;
#endif

    int Read() => _value;
}
```

After: RH2001 is reported on `Value`, and no code fix is offered. The group's `#if` stands in front of the first modifier, and its `#endif` behind a property without an initializer.

```csharp
#define NET
class C
{
#if NET
    private int {|RH2001:Value|} { get; }
#endif

    int Read() => Value;
}
```

The same holds for a property with an initializer, where only the `#if` in front of the first modifier withholds the fix. Input, with `NET` defined:

```csharp
#define NET
class C
{
    int Read() => Value;

#if NET
    private static int Value { get; } = 5;
#endif
}
```

Before (`main`):

```csharp
#define NET
class C
{
    int Read() => _value;

#if NET
    private static readonly int _value = 5;
#endif
}
```

After: RH2001 is reported on `Value`, and no code fix is offered.

The same holds when the property is the active `#else` branch of a group whose excluded `#if` branch holds another whole member: the `#else` stands in front of the first modifier, and the `#endif` behind a property without an initializer. Input, without `NET` defined:

```csharp
class C
{
#if NET
    private int _a;
#else
    private int B { get; }
#endif
    private int _d;

    int Read() => _d + B;
}
```

Before (`main`):

```csharp
class C
{
#if NET
    private int _a;
#else
    private readonly int _b;
#endif
    private int _d;

    int Read() => _d + _b;
}
```

After: RH2001 is reported on `B`, and no code fix is offered.

```csharp
class C
{
#if NET
    private int _a;
#else
    private int {|RH2001:B|} { get; }
#endif
    private int _d;

    int Read() => _d + B;
}
```

The same shape without a modifier, `#if NET` / `int Value { get; }` / `#endif`, is withheld on `main` already (RH2001-009), so both forms now agree. The maintainer's answer covers this case: it withholds the fix for any `#if` group directly in front of the property, whatever the group holds.

### Unchanged

A group in front of an earlier member, a group that wraps the whole type with no directive next to the property, a group around several members with a member of it in front of the property and one behind it, and a group behind a property with an initializer keep the fix, with the output `main` gives:

```csharp
class C
{
#if DEBUG
    private int _trace;
#endif
    private int _other;

    private int Value { get; }

    int Read() => _other + Value;
}
```

```csharp
class C
{
#if DEBUG
    private int _trace;
#endif
    private int _other;

    private readonly int _value;

    int Read() => _other + _value;
}
```

Every group that `main` already refuses — a modifier, an attribute list, an initializer, or text that is not code in an excluded branch, or a group with another group inside a branch — stays refused.

### Fix All

Fix All skips each property the single fix withholds, as today, and fixes the others in its scope.

### Code fixes that format the affected node

None. No formatter output changes; only the RH2001 fix is withheld in more cases. The rule page `documentation/rules/RH2001.md` does not change: a withheld fix is covered by `documentation/rules/general-notes.md` ("A diagnostic without a code fix").

## Decisions

### The maintainer chose to refuse the group

Issue #1943, output question 1, answered **A** in the issue comment of 2026-11-22: the fix is withheld for any `#if` group directly in front of the property, or directly behind a property without an initializer; the diagnostic stays. This is the same answer as for the member-moving ordering fixes (thoenissen/Reihitsu#1668), and the excluded-branch parse that only RH2001 uses is deleted.

### "Directly in front of" and "directly behind" are read as one token's leading trivia

"In front of the property" is the leading trivia of the property's first modifier, which holds every trivia from the token in front of the property — the end of the previous member, or the type body's opening brace — up to that modifier, also across blank lines. "Behind the property" is the leading trivia of the token behind the property's last token: the first token of the next member, or the type body's closing brace. These are the two places RH2001-010 already read, so the reading of the answer moves no boundary; only the test of what the group holds is dropped. A group with one directive there and another elsewhere, such as an `#endif` in front of the first modifier whose `#if` stands in front of an earlier member, or an `#endif` behind a property without an initializer whose `#if` stands in front of an earlier member, withholds the fix, because a directive of it stands there.

### The non-conditional directives keep the fix

`#region`, `#endregion`, `#pragma`, `#nullable`, and the other directives that are not part of a conditional group keep the fix in front of the first modifier, as RH2001-004 states today. The issue and the answer are about conditional groups.

### The behind-the-property case stays limited to a property without an initializer

A property with an initializer ends in `;` in every build, so every branch behind it begins after the declaration. The issue limits the case the same way, and `main` keeps the fix there whatever the group holds.

## Alternatives considered

- **Keep the whole-member test** (issue #1943, option B) — rejected by the maintainer: it parses excluded text, the machinery `AGENTS.md` rules out for a rare input, and it is the last such parse among the code fixes.
- **Refuse every conditional directive anywhere in the type body** — rejected: the issue and the answer name the places next to the property, and a group in front of another member or around the whole type never becomes part of the property's declaration in any build. It would also withhold the fix for every property of a type wrapped in `#if` for multi-targeting, which is common code.
- **Merge the no-modifier case of RH2001-009 into RH2001-010** — rejected as a spec choice: RH2001-009 states the no-modifier case with its own reason (the type is the first token, and a branch can add a modifier), and the requirement IDs its tests cite stay stable.
