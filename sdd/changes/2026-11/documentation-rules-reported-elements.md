# Documentation rules: which elements and parts are reported

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH8307-001 (changed), RH8307-002 (changed), RH8106-001 (coverage changed), RH8106-002 (changed), RH8306-001 (coverage changed), RH8306-004 (changed), RH8201-001 (changed), RH8201-004 (changed), RH8201-005 (example added), RH8203-001 (changed), SHARED-DIAG-004 (second paragraph extended), the rule page `documentation/rules/RH8307.md` |

## What

Three decisions about what a documentation rule reports, taken while the rules were aligned with the shared documentation requirements:

- **RH8307 judges only the top-level elements of a documentation comment.** An element nested inside another element — `<para>`, `<b>`, `<c>`, `<see>`, and any other — is no longer reported, whatever its layout (RH8307-001, RH8307-002). The element around it is judged as before. The rule page names the elements it checks.
- **RH8106 and RH8306 report each reported element.** When one documentation comment holds several `<returns>` elements without text (RH8106) or several `<summary>` elements of a property in one part that read as a sentence (RH8306), each of them gets a diagnostic of its own (RH8106-002, RH8306-004). Their coverage no longer stops at one reported element (RH8106-001, RH8306-001). A partial property is still reported once (RH8306-001).
- **RH8201 and RH8203 judge a partial member part by part.** For a method, property, indexer, or event declared in a defining and an implementing part, each part's own documentation comment is judged on its own: RH8201 reports each part whose comment holds no `<inheritdoc>`, and its fix replaces only that part's comment; RH8203 reports `<inheritdoc/>` on each part of a member that inherits nothing, and never reports a part for an `<inheritdoc/>` that only the other part carries. SHARED-DIAG-004's second paragraph names this documentation comment as text one part writes independently. Whether the member inherits documentation is decided for the member as a whole, so both parts of a member that implements an interface member inherit it, whichever part the compiler maps the interface member to (RH8201-001, RH8201-004, RH8203-001). Partial types and partial constructors stay uncovered.

## Why

- RH8307 is about the elements that structure a comment. A nested element is part of its parent's content; whether nested elements get a check of their own, possibly as a rule of their own, is a separate feature question. The formatter may still lay out a nested element (P11 in `sdd/README.md`); the rule does not follow it there.
- Each reported element is what the user has to change. Marking one element per declaration leaves the others unmarked only because of where they stand, which SHARED-DIAG-006 rules out.
- The two parts of a partial member each carry a documentation comment of their own, and both can be wrong independently. Judging the member once would hide the second part's copy of the description, and judging inheritance by the part the compiler maps the interface member to made RH8203 report `<inheritdoc/>` on a part of a member that does inherit, and RH8201 skip a summary on that part.

## Visible output changes

### RH8307 — a nested element

```csharp
class C
{
    /// <summary>
    /// Value
    /// </summary>
    /// <remarks>
    /// <para>First
    /// second</para>
    /// </remarks>
    int A;
}
```

Before (`main`): the `<para>` element is reported, and the fix gives

```csharp
    /// <remarks>
    /// <para>
    /// First
    /// second
    /// </para>
    /// </remarks>
```

After: not reported.

### RH8307 — rule page

Before: "This rule ensures that XML documentation content starting on the same line as an opening tag also ends on that same line."

After: "This rule ensures that the content of a top-level XML documentation element, such as `<summary>`, `<param>`, or `<remarks>`, that starts on the same line as the element's opening tag also ends on that same line. Elements nested inside another element, such as `<para>` or `<b>`, are not checked."

### RH8106 — several `<returns>` elements without text

```csharp
class C
{
    /// <summary>
    /// Finds
    /// </summary>
    /// <returns/>
    /// <returns></returns>
    public int F() => 0;
}
```

Before (`main`):

```csharp
    /// {|RH8106:<returns/>|}
    /// <returns></returns>
    public int F() => 0;
```

After:

```csharp
    /// {|RH8106:<returns/>|}
    /// {|RH8106:<returns></returns>|}
    public int F() => 0;
```

### RH8306 — several summaries of a property that read as a sentence

```csharp
class C
{
    /// <summary>Gets the size.</summary>
    /// <summary>Gets the length.</summary>
    public int Size { get; }
}
```

Before (`main`):

```csharp
    /// {|RH8306:<summary>Gets the size.</summary>|}
    /// <summary>Gets the length.</summary>
    public int Size { get; }
```

After:

```csharp
    /// {|RH8306:<summary>Gets the size.</summary>|}
    /// {|RH8306:<summary>Gets the length.</summary>|}
    public int Size { get; }
```

### RH8201 and RH8203 — a partial member

```csharp
/// <summary>I</summary>
public interface I
{
    /// <summary>Does it.</summary>
    void M();
}

/// <summary>C</summary>
public partial class C : I
{
    /// <summary>Does it.</summary>
    public partial void M();

    /// <summary>Does it.</summary>
    public partial void M() { }

    /// <inheritdoc/>
    public partial void N();

    /// <inheritdoc/>
    public partial void N() { }
}
```

Before (`main`): the defining part of `M` is reported by RH8201 and its fix replaces only that summary; the implementing part of `M` is not reported; both parts of `N` are reported by RH8203.

After: both parts of `M` are reported by RH8201, and each fix replaces that part's summary with `/// <inheritdoc/>`; both parts of `N` are reported by RH8203, as before.

```csharp
    /// {|RH8201:<summary>Does it.</summary>|}
    public partial void M();

    /// {|RH8201:<summary>Does it.</summary>|}
    public partial void M() { }

    /// {|RH8203:<inheritdoc/>|}
    public partial void N();

    /// {|RH8203:<inheritdoc/>|}
    public partial void N() { }
```

A member that implements an interface member with `<inheritdoc/>` on both parts:

```csharp
    /// <inheritdoc/>
    public partial void M();

    /// <inheritdoc/>
    public partial void M() { }
```

Before (`main`): the implementing part is reported by RH8203. After: not reported.

A member that implements an interface member and is documented only on its implementing part:

```csharp
    public partial void M();

    /// <summary>Does it.</summary>
    public partial void M() { }
```

Before (`main`): not reported. After: the implementing part is reported by RH8201, and the fix replaces its summary with `/// <inheritdoc/>`.

A member that inherits nothing, documented with a summary on its defining part and with `<inheritdoc/>` on its implementing part (method, property, indexer, and event alike):

```csharp
    /// <summary>Stops.</summary>
    public partial void O();

    /// <inheritdoc/>
    public partial void O() { }
```

Before (`main`): both parts are reported by RH8203 — the implementing part on its `<inheritdoc/>`, and the defining part on its name, although its own comment holds no `<inheritdoc>`.

```csharp
    /// <summary>Stops.</summary>
    public partial void {|RH8203:O|}();

    /// {|RH8203:<inheritdoc/>|}
    public partial void O() { }
```

After: only the implementing part is reported.

```csharp
    /// <summary>Stops.</summary>
    public partial void O();

    /// {|RH8203:<inheritdoc/>|}
    public partial void O() { }
```

## Decisions

### RH8307 reports only top-level elements

Maintainer answer, batch 1 of the squad run `doc-rule-fixes` (Q1, option C, for thoenissen/Reihitsu#1347): RH8307 reports only the top-level elements of a documentation comment, and no nested element (`<para>`, `<b>`, …). A formatting tag nested in an element is not reported; a formatting tag written directly at the top level of a comment, outside every element, is a top-level element and is reported as before. Checking nested elements is a follow-up feature issue, which decides whether it becomes a new rule.

### One diagnostic per reported element for RH8106 and RH8306

Maintainer answer, batch 1 (Q2, option A, for thoenissen/Reihitsu#1270): one diagnostic per reported element.

### A partial member is judged part by part by RH8201 and RH8203

Maintainer answer, batch 1 (Q4, option B, for thoenissen/Reihitsu#1617): each part of a partial member documented on both parts is reported on its own, because both parts can carry independent errors; each RH8201 fix replaces only that part's summary; RH8203 reports `<inheritdoc/>` on both parts of a member that inherits nothing.

The inheritance verdict itself stays a verdict on the member as a whole, by the symbol: only the documentation comment is judged per part. SHARED-DIAG-004's second paragraph, which lists the text one part writes independently, each with the rule that judges it, gains the documentation comment of a part of a partial member as RH8201 and RH8203 judge it, so SHARED-DIAG-004 stays the one place that states how the parts are judged (SHARED-DIAG-006 points there). RH8306, which reports a partial property once, is not named there and is unchanged.

## Alternatives considered

- **RH8307: keep reporting nested elements, or exclude only inline formatting elements** — rejected by the maintainer's answer (Q1 options A and B).
- **RH8106/RH8306: one diagnostic per declaration, on the first reported element** — rejected by the maintainer's answer (Q2 option B): the elements after the first would stay unmarked only because of their position.
- **RH8201/RH8203: judge a partial member once, on its defining part, and withhold the RH8201 fix when both parts are documented** — rejected by the maintainer's answer (Q4 option A).
- **State the per-part judgment only in the rule specs, or exclude documentation comments from SHARED-DIAG-004** — rejected: either leaves the rule specs citing a shared requirement whose first paragraph says the member is reported at most once, or moves the statement out of the spec SHARED-DIAG-006 points to.
