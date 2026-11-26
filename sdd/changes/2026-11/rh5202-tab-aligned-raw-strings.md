# RH5202 reports a raw string literal aligned behind tabs

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH5202-009 (new), RH5202-001, RH5202-003, RH5202-007, RH5202-008 |
| Depends on | [raw-string-tab-indentation.md](raw-string-tab-indentation.md) |

## What

RH5202 also reports a multi-line raw string literal whose closing quotes start in the column of its opening quotes, counted in characters, when the whitespace in front of its closing quotes holds a tab (RH5202-009). Its code fix then shifts no line and writes the indentation the compiler strips as spaces, one per character, as RH5202-007 already states for a literal it shifts. The result is the formatter's layout of the literal (FMT-RAW-010), and the analyzer no longer reports it.

This holds for every kind of multi-line raw string literal the rule covers, also for one nested in an interpolation hole of another raw string literal, at any depth: such a nested literal is now reported on its own. The outer literal's fix stays withheld while a nested literal's closing quotes stand behind a tab (RH5202-008), also when the outer literal itself does not shift; after the nested literal's own fix, the outer fix is offered.

A literal whose closing whitespace holds no tab is reported as before, also when a content line holds a tab. The diagnostic's location, title, and message and the code fix's title stay as they are; the rule page gains one sentence saying that the indentation in front of the closing marker is written with spaces.

## Why

The formatter writes the stripped indentation of a tab-indented raw string literal as spaces, also when the literal is already aligned in characters (FMT-RAW-010). RH5202 judges how a raw string literal spells its value, and the stripped indentation is part of that spelling, so a literal the formatter rewrites in this construct is a gap of the rule itself ([P11](../../README.md#the-formatter-may-do-more-than-the-analyzers-p11)). Today a user who runs only the analyzer keeps mixed tab and space indentation that, in an editor rendering a tab wider than one column, does not stand under the opening quotes, while a user of the formatter gets spaces.

## Visible output changes

`→` stands for one tab. Every *before* is the output of `main` from `scripts/apply-fix.sh RH5202`, under LF and CRLF.

**1. A literal aligned behind tabs.** The closing quotes stand behind 12 spaces and four tabs, in column 16, the column of the opening quotes.

```csharp
// input
class C
{
    void M()
    {
        var a = """
            →→→→x
            →→→→""";
    }
}
```

```csharp
// before: not reported
```

```csharp
// after: reported on the literal; the fix writes
class C
{
    void M()
    {
        var a = """
                x
                """;
    }
}
```

**2. An interpolated literal aligned behind tabs, with whitespace behind the stripped width.** The closing quotes stand behind 12 spaces and five tabs, in column 17. The three spaces in front of `Hello` and the two tabs in front of `World` lie behind that width and stay.

```csharp
// input
class C
{
    void M(int name)
    {
        var a = $"""
            →→→→→   Hello {name}
            →→→→→→→World
            →→→→→""";
    }
}
```

```csharp
// before: not reported
```

```csharp
// after: reported on the literal; the fix writes
class C
{
    void M(int name)
    {
        var a = $"""
                    Hello {name}
                 →→World
                 """;
    }
}
```

**3. A raw string literal aligned behind tabs in an interpolation hole.** The nested closing quotes stand behind nine spaces and two tabs, in column 11, the column of the nested opening quotes. The outer literal is reported, and its fix is withheld (RH5202-008), before and after.

```csharp
// input
class C
{
    void M()
    {
        var a = $"""
    Outer {"""
         →→inner
         →→"""}
    """;
    }
}
```

```csharp
// before: only the outer literal is reported, and no fix is offered for it
```

```csharp
// after: the nested literal is reported as well; its fix writes
class C
{
    void M()
    {
        var a = $"""
    Outer {"""
           inner
           """}
    """;
    }
}
```

```csharp
// after that, the outer literal's fix is offered and writes
class C
{
    void M()
    {
        var a = $"""
                 Outer {"""
                        inner
                        """}
                 """;
    }
}
```

**4. A literal written with tabs for the indentation and spaces for the alignment.** The closing quotes stand behind two tabs and eight spaces, in column 10, the column of the opening quotes counted in characters. The tabs on the lines outside the literal are not this rule's and stay.

```csharp
// input
class C
{
→void M()
→{
→→var a = """
→→        x
→→        """;
→}
}
```

```csharp
// before: not reported
```

```csharp
// after: reported on the literal; the fix writes
class C
{
→void M()
→{
→→var a = """
          x
          """;
→}
}
```

**5. Unchanged: an aligned literal whose closing whitespace holds no tab.** It stays unreported, also when a content line holds a tab.

```csharp
// input, before, and after: not reported
class C
{
    void M()
    {
        var a = """
                →x
                """;
    }
}
```

**6. The rule page.** `documentation/rules/RH5202.md` gains one sentence at the end of its *Description*:

> Before: This rule ensures that multi-line raw string literals have their opening and closing quote markers (`"""`) aligned to the same column. The content lines are adjusted uniformly to maintain consistent indentation.

> After: This rule ensures that multi-line raw string literals have their opening and closing quote markers (`"""`) aligned to the same column. The content lines are adjusted uniformly to maintain consistent indentation. The indentation in front of the closing marker, and the same indentation of the content lines, is written with spaces, not tabs.

The diagnostic message and title (`Raw string literals should be formatted correctly.`) and the code fix's title (`Fix the alignment of the raw string literal`) do not change.

## Decisions

### Report the literal and write the formatter's layout

The maintainer chose option A of thoenissen/Reihitsu#1925 on 2026-11-22: RH5202 reports a multi-line raw string literal whose content and closing lines are aligned in characters behind tabs, and its fix writes the layout the formatter writes (FMT-RAW-010).

### The tab in front of the closing quotes decides

The literal is reported when the whitespace in front of its closing quotes holds a tab, the same condition by which the formatter decides to write spaces (FMT-RAW-010) and by which the fix already writes them for a literal it shifts (RH5202-007). Whatever the content lines hold, the formatter rewrites such a literal, because the closing whitespace itself becomes spaces, and leaves a literal without a tab there as written. One condition for the analyzer, the fix, and the formatter keeps the fix's result unreported and formatted.

### The diagnostic message stays

The maintainer left it to the run whether the message needs a wording adjustment for the new case. It does not: the message says that the literal should be formatted correctly and names neither a column nor alignment, so it describes a literal whose indentation is written with tabs as well as one whose closing quotes are misaligned.

### A literal written with tabs and aligned with spaces is reported

The maintainer chose option 1A on 2026-11-26: a literal whose closing quotes stand behind tabs for the indentation of the code followed by spaces that align them in characters is reported like any other literal with a tab in front of its closing quotes, and the fix writes the content and closing indentation as spaces, one per character, as FMT-RAW-010 writes the stripped indentation (RH5202-009, visible output change 4). The alternative, leaving such a literal unreported because it already stands under its opening quotes in every editor, would give the analyzer a second condition next to the formatter's.

### The rule page names the spaces

The maintainer chose option 2B on 2026-11-26: the rule page's *Description* gains the sentence "The indentation in front of the closing marker, and the same indentation of the content lines, is written with spaces, not tabs.", so the page describes every literal the rule reports, also one that an editor shows aligned (visible output change 6).

## Alternatives considered

- **Leave the literal unreported** (option B of the issue) — rejected by the maintainer's answer: an analyzer-only user keeps the mixed indentation the formatter rewrites.
- **Report only when a content line holds a tab within the stripped width** — rejected: the formatter rewrites the closing whitespace itself whenever it holds a tab, so a literal with tabs only on its closing line would stay unreported yet change under formatting.
- **Report the tabs through RH5601 (tabs in code)** — rejected: the stripped indentation is how the literal spells its value, which RH5202 judges and RH5601 does not (RH5601-002, SHARED-DIAG-003).
- **Leave a literal written with tabs and aligned with spaces unreported** — rejected by the maintainer's answer 1A: a second report condition next to the formatter's.
- **Leave the rule page unchanged** — rejected by the maintainer's answer 2B: the page would not explain why a literal an editor shows aligned is reported.
- **A message that names the tabs** — rejected: the current message already covers the case, and a second message for one rule would split one diagnostic into two wordings for one construct.
