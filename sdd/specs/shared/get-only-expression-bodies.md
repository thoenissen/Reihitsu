# Shared — Get-only members reported for an expression body

| | |
|---|---|
| ID prefix | SHARED-GETONLY |
| Status | Active |
| Code owners | `GetOnlyMemberExpressionBodyRewriteUtilities` |
| Uses | FMT-STRUCT-009, FMT-STRUCT-011, FMT-STRUCT-012, FMT-STRUCT-013, FMT-STRUCT-014, FMT-STRUCT-015, FMT-UNSUP-001, FMT-UNSUP-007, FMT-UNSUP-012, FMT-UNSUP-013, SHARED-BODY-001, SHARED-DIAG-011, SHARED-FIX-003, SHARED-FIX-006, SHARED-FIX-007, SHARED-FIX-010 |

This spec covers what the rules share that report a get-only member whose accessor list the formatter replaces with an expression body — RH3209 (properties) and RH3210 (indexers): which declarations are reported, where the diagnostic is, what the code fix produces, and when it is withheld. Each rule's spec states which kind of member it reports. "The member" below is the property or indexer declaration the rule checks; "the getter" is the only accessor of its accessor list. The formatter's counterparts are FMT-STRUCT-014 and FMT-STRUCT-015. The decision whether a get-only member is converted, and the conversion itself, have one owner, `GetOnlyMemberExpressionBodyRewriteUtilities` in `Reihitsu.Core`, which the formatter's get-only transform, the analyzers, and the code fixes of both rules use.

## Non-goals

- Which kind of member a rule reports — the rule's own spec: RH3209 for properties, RH3210 for indexers.
- Block-bodied accessors in an accessor list that holds more than a getter, which the formatter converts to expression-bodied accessors (FMT-STRUCT-009). No rule reports them.
- Expression-bodied properties and indexers. They already have the form the rules ask for and stay expression-bodied (FMT-STRUCT-008); where their arrow wraps is RH5401's.
- Expression-bodied methods, constructors, operators, conversion operators, finalizers, and local functions — SHARED-BODY.
- How the formatter converts a get-only member in a document — FMT-STRUCT-014 and FMT-STRUCT-015.

## Requirements

### SHARED-GETONLY-001 — A get-only member the formatter converts is reported

A rule that uses this spec MUST report a member of its kind whose accessor list the structural transforms replace with an expression body under FMT-STRUCT-014 and that FMT-STRUCT-015 does not keep: an accessor list whose only accessor is a `get` accessor without attributes and modifiers, either expression-bodied or with a block that FMT-STRUCT-009 to FMT-STRUCT-013 convert — a single `return` with a value, `return ref` included, or a single `throw` with an operand. It MUST report such a member from C# 6 on, and a member whose getter throws from C# 7 on; below those versions it MUST NOT report it (SHARED-DIAG-011). A member in an interface, a static member, and an explicit interface implementation are reported like any other.

Why: An expression-bodied member is the shortest equivalent of a get-only property or indexer, and it is the form the formatter writes (FMT-STRUCT-014). Without a rule, a user who runs only the analyzers never learns that the accessor list is not the house style, and a user who formats sees a rewrite no diagnostic announced. The rule reports the members the formatter converts, so formatting a document clears their diagnostics, and the code fix has a result to offer for each of them; a member that only a comment or a refused shape keeps from being converted is reported as well, without a fix (SHARED-GETONLY-003, SHARED-GETONLY-006). Expression-bodied properties and indexers only exist from C# 6 on, and throw expressions from C# 7 on; below those versions the user cannot write the form the rule asks for.

Example — RH3209:

```csharp
using System;

class C
{
    private int _value;

    public int A {|RH3209:{ get => _value; }|}

    public int B
    {|RH3209:{
        get
        {
            return _value;
        }
    }|}

    public int D {|RH3209:{ get { throw new InvalidOperationException(); } }|}

    public ref int E {|RH3209:{ get { return ref _value; } }|}
}
```

Under C# 6, `B` is reported and `D` is not, because its getter throws; `A` and `E` need C# 7 to compile, for the expression-bodied getter and the `ref` return. Under C# 5, `B` is not reported.

### SHARED-GETONLY-002 — A member whose shape the formatter keeps is not reported

A rule that uses this spec MUST NOT report a member whose accessor list holds a `set` or `init` accessor next to the getter, a getter without a body (`get;`), a getter with an attribute or a modifier, or a getter block that FMT-STRUCT-009 to FMT-STRUCT-013 keep, such as a block with more than one statement, a rethrow without an operand, or a `yield return`. It MUST NOT report a property that has an initializer, nor a member that is already expression-bodied.

Why: The formatter keeps the accessor list of every such member (FMT-STRUCT-015). Each has no equivalent expression-bodied form: a second accessor or an initializer has no place on an expression-bodied member, an auto-accessor has no expression to lift, an attribute on the getter would change its target on the member, a modifier such as `readonly` would be lost, and a statement without an expression form would change meaning or not compile.

Example — RH3209, in a project compiled with C# 14:

```csharp
using System;

class C
{
    private int _value;

    public int A
    {
        get => _value;
        set => _value = value;
    }

    public int B { get; }

    public int D
    {
        [Obsolete]
        get => _value;
    }

    public int E
    {
        get
        {
            var value = _value;

            return value;
        }
    }

    public int F { get => field; } = 5;

    public int G => _value;
}
```

is not reported. The other side of each condition is under SHARED-GETONLY-001: a getter alone, with a body, without attributes and modifiers, with a single convertible statement, on a property without an initializer.

### SHARED-GETONLY-003 — A member whose comment the formatter keeps is reported without a fix

A rule that uses this spec MUST report a member that SHARED-GETONLY-001 describes also when FMT-STRUCT-015 keeps its accessor list because of a comment, or FMT-STRUCT-012 keeps its getter block because of one: a comment in trivia the conversion would delete or join onto the signature's line — behind the last token in front of the accessor list, on either side of its opening brace, in front of `get`, in front of the getter's semicolon, or in front of the list's closing brace — a comment behind both the getter's semicolon and the closing brace, or a comment in a seam of the getter block that FMT-STRUCT-012 names. The code fix MUST NOT be offered for such a member. A comment behind only the getter's semicolon, or behind only the closing brace, keeps neither the accessor list nor the fix: the fix carries it behind the new semicolon (SHARED-GETONLY-005). A directive or disabled text in these places is SHARED-GETONLY-007's.

Why: The rule is about the get-only accessor list, and a comment does not make the accessor list the better form: the author can move the comment above the member, after which the formatter and the fix convert it. The fix writes what the formatter writes, and the formatter keeps the accessor list because the comment has no place on the expression-bodied member (FMT-STRUCT-015) — deleting it would lose what the author wrote, and moving it would change what it describes — so the fix is withheld (SHARED-FIX-003); a reported diagnostic without a fix is a supported end state. A comment behind the semicolon or behind the closing brace already ends the member's last line and follows the new semicolon, so it is no obstacle.

Example — RH3209:

```csharp
class C
{
    private int _value;

    public int A // note
    {|RH3209:{
        get => _value;
    }|}

    public int B
    {|RH3209:{
        // the value
        get => _value;
    }|}

    public int D
    {|RH3209:{
        get => _value; // a
    }|} // b

    public int E
    {|RH3209:{
        get
        {
            // cached
            return _value;
        }
    }|}
}
```

is reported, and no code fix is offered. A comment behind only one of the two is reported with a fix:

```csharp
class C
{
    private int _value;

    public int A {|RH3209:{ get => _value; }|} // a

    public int B
    {|RH3209:{
        get => _value; // b
    }|}
}
```

```csharp
class C
{
    private int _value;

    public int A => _value; // a

    public int B => _value; // b
}
```

### SHARED-GETONLY-004 — The diagnostic covers the accessor list

A rule that uses this spec MUST report one diagnostic per member, on its accessor list: from its opening brace through its closing brace.

Why: The accessor list is what the fix replaces; the signature of the member stays.

Example: see SHARED-GETONLY-001.

### SHARED-GETONLY-005 — The fix writes the formatter's expression body

The code fix MUST replace the accessor list with the arrow clause and semicolon that FMT-STRUCT-014 writes for it: the getter's expression, a `throw` statement's operand as FMT-STRUCT-011 wraps it, and a comment that trailed the getter's semicolon or the closing brace behind the new semicolon, as FMT-STRUCT-015 carries it. The fix MUST take the converted member from the conversion the formatter uses, not build it on its own. The arrow clause and the semicolon are the code the fix changes, laid out as the formatter lays them out (SHARED-FIX-007), and so is the gap in front of the arrow: it is the gap the formatter writes between the last token in front of the accessor list and `=>`, not one the fix writes on its own. The rest of the member MUST stay as written (SHARED-FIX-006): everything in front of the accessor list, such as its documentation comment, its attributes, its modifiers, its type, its name or parameter list, with the whitespace and comments between their tokens. The whitespace between the last token in front of the accessor list and its opening brace goes with the removed list, so the arrow follows that token on its line.

Why: One owner for the conversion keeps the fix and the formatter from drifting apart, and the fix's result is then what formatting the document produces for the accessor list. A fix that wrote the gap in front of the arrow itself would predict what the formatter's spacing writes there. The diagnostic is about the accessor list only; the layout of the signature is the author's, and the formatter and the layout rules correct it on their own.

Example — RH3209:

```csharp
using System;

class C
{
    private int _value;

    public int A {|RH3209:{ get => _value; }|}

    public int B
    {|RH3209:{
        get
        {
            return _value;
        }
    }|}

    public int D {|RH3209:{ get { throw new InvalidOperationException(); } }|}

    public int E {|RH3209:{ get => _value; }|} // a
}
```

```csharp
using System;

class C
{
    private int _value;

    public int A => _value;

    public int B => _value;

    public int D => throw new InvalidOperationException();

    public int E => _value; // a
}
```

Example — RH3209, only the accessor list is converted; the spacing of the signature stays:

```csharp
class C
{
    private int _value;

    public  int  A {|RH3209:{ get { return _value; } }|}
}
```

```csharp
class C
{
    private int _value;

    public  int  A => _value;
}
```

### SHARED-GETONLY-006 — No fix for a getter the formatter refuses

A rule that uses this spec MUST report a member that SHARED-GETONLY-001 describes also when the formatter refuses it as unsupported (FMT-UNSUP-001), such as a getter with a comment, a directive, or disabled text between `get` and its `=>` (FMT-UNSUP-013) or between the `=>` and its expression (FMT-UNSUP-012). The code fix MUST NOT be offered when the getter as written — the accessor declaration, from `get` through its semicolon or block — holds a shape the formatter refuses; it learns this by asking `ReihitsuFormatter.FindUnsupportedSyntax` (FMT-UNSUP-007) about that getter. The fix MUST NOT be offered either where the member it builds holds such a shape (SHARED-FIX-010).

Why: A refused shape is not one the formatter keeps: it asks the user to rewrite it, and once the comment or directive moves out of the getter, the formatter and the fix convert it. The conversion deletes `get` and both gaps around the getter's `=>`, and the arrow it writes has nothing on either side of it (FMT-STRUCT-014), so the member the fix builds no longer holds what stood in those gaps, and SHARED-FIX-010's question about that member alone would offer a fix that drops it; asking about the getter as written withholds that fix (SHARED-FIX-003). A refused shape elsewhere in the member is also in the member the fix builds, so SHARED-FIX-010 withholds the fix for it. A reported diagnostic without a fix is a supported end state.

Example — RH3209:

```csharp
class C
{
    private int _value;

    public int A {|RH3209:{ get /* c */ => _value; }|}

    public int B {|RH3209:{ get => /* c */ _value; }|}

    public int D
    {|RH3209:{
        get
#if DEBUG
            => 1;
#else
            => 2;
#endif
    }|}
}
```

is reported, and no code fix is offered.

### SHARED-GETONLY-007 — A member whose directive the formatter keeps is not reported

A rule that uses this spec MUST NOT report a member that holds a directive or disabled text anywhere from the trailing trivia of the last token in front of its accessor list through the list's closing brace, except a directive or disabled text in one of the two gaps around the `=>` of an expression-bodied getter, which SHARED-GETONLY-006 reports, and a conditional group that lies wholly between the first and the last token of the getter's expression, which FMT-STRUCT-013 converts and the rule reports. A member that holds both such a directive and a comment of SHARED-GETONLY-003 is not reported.

Why: Disabled text can hold code of another build, such as a setter, an attribute, or a second statement, and a member that is get-only in one build is then not get-only in another, so the expression-bodied form the rule asks for cannot be written for every build (SHARED-DIAG-011). The formatter keeps the accessor list or the getter block for each such directive (FMT-STRUCT-013, FMT-STRUCT-015). A conditional group inside the getter's expression only chooses part of the value and leaves the member get-only in every build. The expression-bodied member rules leave members with a directive in the converted span alone for the same reason (SHARED-BODY-001).

Example — RH3209:

```csharp
class C
{
    private int _value;

    public int A
    {
        get => _value;
#if WRITABLE
        set => _value = value;
#endif
    }

    public int B
    {
#if DEBUG
        get => 1;
#else
        get => 2;
#endif
    }

    public int D
    {
        get
        {
            return _value;
#if DEBUG
        }
#else
        }
#endif
    }
}
```

is not reported, while a conditional group inside the getter's expression is:

```csharp
class C
{
    public int A
    {|RH3209:{
        get
        {
            return M(
#if DEBUG
                1
#else
                2
#endif
                );
        }
    }|}

    private static int M(int value)
    {
        return value;
    }
}
```
