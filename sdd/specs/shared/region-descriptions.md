# Shared — Region descriptions

| | |
|---|---|
| ID prefix | SHARED-REGDESC |
| Status | Active |
| Code owners | `RegionDescriptionAnalyzerBase` |
| Uses | FMT-REGION-006 |

This spec covers what the rules that read the name of a region share: what the description of a `#region` and of an `#endregion` directive is (RH7301, RH7306, RH7307, RH7308, RH7309, RH7311), and how a rule that checks the description of every region directive reports it (RH7306, RH7307). Which descriptions a rule forbids is each rule's own spec.

## Non-goals

- How the formatter writes a region name and the comment behind its `#endregion` — FMT-REGION.
- How a description is capitalized — RH7302, which reads the text after `#region` on its own.
- Whether a `#region` and its `#endregion` agree — RH7301.

## Requirements

### SHARED-REGDESC-001 — The description of a region directive

A rule that reads a region description MUST take, for a `#region` directive, the text after the `#region` keyword with leading and trailing whitespace removed. For an `#endregion` directive it MUST take the text after the `#endregion` keyword with leading and trailing whitespace removed, and then remove one leading `//` and the whitespace after it. A directive with no text has the empty description. A `//` in front of a `#region` description and a block comment behind an `#endregion` are part of the description.

Why: The text behind a region directive is a preprocessing message, not a comment, so the compiler keeps it as written. `#endregion // Fields` and `#endregion Fields` are the two common ways to repeat a region's name, and the whitespace around the name is layout; reading both forms as `Fields` lets every rule compare names instead of spellings. Only the `#endregion` gets the `//` removed, because that is where the convention puts it.

Example — RH7301, which compares the two descriptions:

```csharp
class C
{
    #region   Fields
    int _a;
    #endregion Fields

    #region Methods
    void M() { }
    #endregion //Methods

    #region Events
    #endregion   //   Events
}
```

is not reported: every pair has the description `Fields`, `Methods`, or `Events` on both sides. A block comment is not removed:

```csharp
class C
{
    #region Fields
    int _a;
    {|RH7301:#endregion /* Fields */|}
}
```

is reported, because the `#endregion` description is `/* Fields */`.

### SHARED-REGDESC-002 — Every region directive is checked on its own

A rule that checks region descriptions MUST check the description (SHARED-REGDESC-001) of every `#region` and of every `#endregion` directive on its own — between members, inside an element body, and inside a branch that `#if` excludes — and MUST report one diagnostic per directive whose description it forbids, located on the directive from its `#` to the end of its line.

Why: Both directives show the region's name, in the code and in the editor's outline, and either can carry the forbidden word while the other does not, so each is checked and reported where it is. The compiler still parses region directives in an excluded branch, and the formatter names them there too (FMT-REGION-006), so a rule that skipped them would leave a name unchecked that the user can see.

Example — RH7307, where only the `#endregion` of the first region and both directives of the second carry the forbidden description:

```csharp
class C
{
    #region Methods

    void M()
    {
    }

    {|RH7307:#endregion // Members|}

    {|RH7307:#region Members|}

    int _a;

    {|RH7307:#endregion // Members|}
}
```

and in an element body and an excluded branch:

```csharp
class C
{
    void M()
    {
        {|RH7307:#region Members|}
        var a = 1;
        #endregion // Locals
    }

#if false
    {|RH7307:#region Members|}

    #endregion // Inactive
#endif
}
```
