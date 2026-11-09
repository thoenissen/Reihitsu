# Conditional groups that the RH2001 fix and the member-moving fixes did not read

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH2001-010 (added), RH2001-004 (changed), RH2001-009 (changed), SHARED-ORDER-007 (changed), SHARED-ORDER-008 (changed), SHARED-FIX-009 (changed) |
| Depends on | [member-move-conditional-groups.md](member-move-conditional-groups.md) |

## What

- **RH2001** (RH2001-010). The fix is withheld for a property with a modifier when a directive of a conditional group stands in the leading trivia of its first modifier and the group does not wrap whole members: it is nested, or one of its excluded branches, parsed on its own as the content of a type body, does not read as whole member declarations without a syntax error. A group of whole members above the property keeps the fix, as RH2001-004 states. RH2001-004 and RH2001-009 point to the new requirement instead of covering the position themselves.
- **Every declaration of a `partial` type** (SHARED-ORDER-008). A conditional directive in front of the body of any declaration of the reordered type, not only of the declaration whose members the fix reorders, makes the type count as one whose field order sets its layout, by the test that already applies to the reordered declaration. A group in front of a declaration's first token whose excluded branch holds another declaration of the type — a `partial` declaration of the type's name with an attribute list — counts as well; until now, every group whose excluded branches read as whole members was exempt.
- **Groups in the type body in front of the spans** (SHARED-ORDER-008). A conditional group with a directive in the body of the reordered declaration between its opening brace and the moved and the crossed span, which does not wrap whole units by the existing structural test, makes the type count as one whose field order sets its layout. Until now, such a directive was not read. A move that reorders no instance field stays available, as for a conditional directive in the type's declaration. A directive behind the spans is still not read.
- SHARED-ORDER-007 and SHARED-FIX-009 name the new readings where they referred to the old ones.

## Why

The maintainer's triage of thoenissen/Reihitsu#1665, thoenissen/Reihitsu#1666, and thoenissen/Reihitsu#1667 (2026-11-09): each fix changes program meaning or breaks the build in another build configuration, which SHARED-FIX-003 (*Build configurations*) forbids. The inputs those issues report are withheld as bug fixes against SHARED-FIX-003, which is unchanged:

- RH2001 converts a property whose excluded branch above holds `extern` into a field that is `extern` in a build with the symbol (CS0106), and one whose excluded branch holds `readonly`, in a struct, into a field with `readonly` twice (CS1004).
- The member-moving fixes reorder the fields of a partial class whose other declaration carries `[StructLayout(LayoutKind.Sequential)]` inside `#if`, or is itself, with that attribute, inside an `#if` block in front of the reordered declaration, and of a partial struct whose other declaration carries `[StructLayout(LayoutKind.Auto)]` inside `#if !X`.
- RH7109 reorders the fields of a struct `D` that a group in front of the moved and the crossed span opens in a build with `X`, after closing the class whose fields the analyzed build reorders.

The specs did not decide these inputs as SHARED-FIX-003 does: RH2001-004 kept every directive above the property in front of the field, SHARED-ORDER-008 read the conditional directives of the reordered declaration only, and it stated that a directive outside both spans is not read. Each new reading follows SHARED-FIX-009: it decides from where the directives stand and from one parse of each excluded branch, the parse that SHARED-ORDER-008 already uses, and a group it does not classify withholds the fix. Because it cannot tell a harmless excluded branch from a harmful one by anything finer, it also withholds a few fixes that change no build; those are listed below.

## Visible output changes

Code-fix output on `main` measured with `scripts/apply-fix.sh` at `95aa3e6`, LF and CRLF alike. Each case is a fix that main offers, which changes no build, and which is now withheld. The diagnostic stays in every case. The inputs of the three issues, whose withholding SHARED-FIX-003 demands, are not listed.

An attribute list in an excluded branch in front of the first modifier, RH2001 (RH2001-010). Without `DEBUG` defined:

```csharp
class C
{
#if DEBUG
    [System.Obsolete]
#endif
    private int Value { get; }

    int Read() => Value;
}
```

Before:

```csharp
class C
{
#if DEBUG
    [System.Obsolete]
#endif
    private readonly int _value;

    int Read() => _value;
}
```

After: no fix offered; RH2001 stays reported on `Value`.

A conditional attribute that sets no layout on another declaration of a partial class, RH7109 (SHARED-ORDER-008). Without `DEBUG` defined:

```csharp
#if DEBUG
[System.Serializable]
#endif
public partial class C { }

public partial class C
{
    private int _a;
    private readonly int _c;
}
```

Before:

```csharp
#if DEBUG
[System.Serializable]
#endif
public partial class C { }

public partial class C
{
    private readonly int _c;
    private int _a;
}
```

After: no fix offered; RH7109 stays reported on `_c`. The same input with one declaration of `C` is withheld on `main` already.

A modifier in an excluded branch in front of a member that the move neither moves nor crosses, RH7109 (SHARED-ORDER-008). Without `DEBUG` defined:

```csharp
public class C
{
#if DEBUG
    static
#endif
    private readonly int _q;
    private int _a;
    private readonly int _c;
}
```

Before:

```csharp
public class C
{
#if DEBUG
    static
#endif
    private readonly int _q;
    private readonly int _c;
    private int _a;
}
```

After: no fix offered; RH7109 stays reported on `_c`.

Unchanged and kept, measured the same way: a group of whole members in front of the property for RH2001; a plain partial class, and a partial class with another declaration of it without an attribute list in an `#if` block in front of it, and a group of whole members or whole statements in front of the spans for RH7109; and, for RH7103, the move of a static field behind the group of thoenissen/Reihitsu#1665, which reorders no instance field.

## Decisions

### The maintainer withholds the three fixes that change no build

The maintainer's answer to the run's first batch of output questions (2026-11-09) chose option A, as recommended, for each of the three cases under *Visible output changes*. No fix is offered for:

1. RH2001 with `[System.Obsolete]` in an excluded branch in front of the property;
2. RH7109 on a partial class whose other declaration carries a conditional `[System.Serializable]`;
3. RH7109 with `#if DEBUG static #endif` in front of a member that is neither moved nor crossed, ahead of the moved fields.

In each case the diagnostic stays reported.

### RH2001 tells whole members from everything else by one parse of each excluded branch

The fix keeps the group of whole members above a property, which RH2001-009 already shows with its fix, and refuses every other group in front of the first modifier. The test is the one SHARED-ORDER-008 uses for a group in the leading trivia of a type's first token: no nesting, and each excluded branch read as whole members. The active branch needs no reading: a modifier in it is the property's first modifier, and the group's next directive then stands between two modifiers, which RH2001-009 already refuses.

### A group in front of the spans counts like a conditional directive in the type's declaration

Such a group can make the members of the spans belong to another type in another build, or make that type another one, which only a reading of the branches could tell. The members of the spans keep their relative order in every build, so only a reorder of instance fields can be unsafe; the group refuses exactly those moves, as a conditional directive in the type's header does. A group behind the spans cannot change which type the members of the spans belong to, and is still not read.

### Every declaration of a partial type is read

The layout of a partial type comes from the attributes of all its declarations, which the symbol already combines for the analyzed build; the conditional-directive test reads each declaration on its own, with the same rules as for the reordered declaration. The bodies of the other declarations are not read.

A declaration of the type that an excluded branch writes in front of the reordered declaration is read too: the branch is parsed already, and the check is the name, the type parameter count, the `partial` modifier, and the attribute lists of the type declarations the parse holds. Only one with an attribute list counts, because the declarations of a `partial` type share its kind, so another declaration sets the layout through its attributes alone; one without keeps `main`'s fix.

### A partial declaration that only excluded code writes elsewhere stays unread

A `partial` declaration of the type that only excluded code writes anywhere else — behind the reordered declaration, in a group in a type's body, or in another document — is not read, and the fix stays as on `main`. No cheap check finds it: it would take reading every excluded branch of every document, and the only refusal without that reading, counting every `partial` type as one whose field order sets its layout, withholds the move of instance fields on every partial type with fields. SHARED-ORDER-008 records the gap as a decision.

## Alternatives considered

- **RH2001: withhold the fix for every conditional directive in front of the first modifier, whatever the group holds**, as RH2001-009 does for a property without a modifier — rejected: it needs no parse, but it withholds the fix for a group of whole members above the property, which is common (`#if DEBUG` fields above a property) and which RH2001-009 shows with its fix today.
- **RH2001: a check of the excluded text's tokens instead of a parse**, such as whether the text ends with `;` or `}` — rejected: a parser-like rule over text (`AGENTS.md`, *Special cases do not grow the source*), where the parse SHARED-ORDER-008 already uses decides it exactly.
- **Member moves: refuse every move behind a group in front of the spans that does not wrap whole units** — rejected: it also withholds the move of a static member or a constant, which reorders no instance field and is safe in every build, as for a conditional directive in the type's header.
- **Member moves: read every conditional directive of the type's body, also behind the spans** — rejected: a group behind the spans changes neither which type the moved and crossed members belong to nor their order.
- **Refuse every group whose excluded branch holds a `partial` declaration of the type, with or without attributes** — rejected: a declaration without an attribute list gives a class no layout, so the refusal would withhold fixes that `main` offers and that change no build.
- **Count every `partial` type as one whose field order sets its layout** — rejected: it withholds the move of instance fields on every partial type with fields, which is common code.
- **Declare the shapes unsupported** (`AGENTS.md`, *Special cases do not grow the source*) — not an option here: the formatter holds no special case for a conditional group in front of a member or a type's body, so the declaration would delete nothing, and the code fixes would still need their checks, because the group in front of the spans lies outside the node a member-moving fix formats.
