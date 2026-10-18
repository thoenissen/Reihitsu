Role: spec author
Commit: 34042175ddd1a853bc731dc73d53d0580e5ca00c
Verdict: spec written

Second repair round, on `04-skeptic.md`: "Still wrong" items 1–3, the optional item 4, and every open-question text fix it lists. RH6002 stays unchanged (`squad/decisions/2026-10-18-rh6002-003-why-left-unchanged.md`). The coordinator's message carried no intended fix or hoped-for verdict; nothing had to be ignored. Every item was re-derived with the scratch harness (real analyzer, code fix, and formatter) before the text changed.

## Changed requirements

- RH5601-001 — narrowed (item 1). The old condition "neither token next to the tab is a literal" still claimed a tab between the tokens of a directive line that precedes a literal (`string s =⏎#pragma→warning disable CS0168⏎"abc";` — verified not reported). The requirement now states two conditions in terms of code tokens (tokens outside directives and documentation comments): a code token precedes the tab on its line and the last such token is not a string or character literal; or no code token precedes it on its line and the next code token is not a string or character literal. Verified on both sides: `string t = "a";→// c` (last code token `;`) and `#pragma→warning restore CS0168` before `int y;` are reported. The intro sentence no longer claims "every tab outside literals, comments, and disabled text".
- RH5602-001 — narrowed (item 2): "the last character of a code token — a token outside directives and documentation comments — or of a `/* */` comment". `#if DEBUG··` (thoenissen/Reihitsu#960) and `/// <summary>··` (verified not reported) are no longer claimed.
- RH5603-001, RH5603-002 — narrowed (item 2): "the last character of a code token … or of a `//` or `/* */` comment". A final directive or disabled text (RH5603 Q1) is no longer decided.
- RH6009-001, RH6010-001 — narrowed (item 3): only braces outside interpolated strings are claimed, so braces inside an interpolation's expression (RH6009 Q1, RH6010 Q1) are no longer decided; the intro sentences say the same. RH6009-002/RH6010-002 (the interpolation's own braces) are unchanged.
- RH6001, RH6006 non-goals (item 4, taken): RH6001 names directive keywords such as `#if(DEBUG)`; RH6006 names parentheses in documentation comments (`cref="M( int)"`, verified not reported) next to directives.

No ID was added, retired, or renumbered; ledger and index unchanged.

### How it was verified

- All C# examples of the 16 specs re-run through the harness: every reported span equals its markup, every "no code fix is offered" block registers none, every before/after pair equals the applied fixes, no compiler errors.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"`: 26 passed.

## Examples for the tests

All 60 requirements, as in `01-spec-author.md`. Must stay untagged, in addition to the list in `03-spec-author.md`: any test whose input is a directive line directly before a literal (RH5601), `#if DEBUG··` or `/// <summary>··` (RH5602), a final directive or disabled text (RH5603), or a brace inside an interpolation's expression (RH6009, RH6010).

## Open questions to file

Final, file-ready list; it replaces the lists in `01-spec-author.md` and `03-spec-author.md`. Each block is one new sub-issue of thoenissen/Reihitsu#912, titled as its heading; no existing open-questions issue covers these specs (thoenissen/Reihitsu#920, the issue of `formatter/horizontal-spacing.md`, is closed). None is about the formatter spacing pairs of thoenissen/Reihitsu#958. In the inputs, `→` is a tab, `·` a space, `⏎` a line break.

---

### SDD open questions: sdd/specs/analyzer/RH5601.md

**Q1 — A tab inside a directive line's free text or comment is reported and rewritten.**

Input:

```text
class C
{
#region Na→me
    int x;
#endregion
#pragma warning disable CS0168 // a→b
    void M()
    {
#warning a→b
    }
}
```

Real output: all three tabs are reported, and the code fix replaces each with four spaces — changing the region name, the comment, and the warning message. The formatter keeps all three tabs (FMT-CLEAN-003 protects directive free text, FMT-CLEAN-004 comments) and copies the region name into `#endregion // Na→me`, so formatted code keeps raising RH5601.

Why it looks wrong: the rule page exempts comments and RH5601-002 treats user-authored text as content, but text on a directive line is not recognized as such; analyzer and formatter disagree. Readings: (a) RH5601 does not report free text or comments on a directive line; (b) the formatter replaces those tabs too.

Touches RH5601-001, RH5601-002, FMT-CLEAN-003, FMT-CLEAN-004.

**Q2 — A tab in the whitespace that belongs to a string or character literal is not reported.**

Input:

```text
class C
{
    string s =⏎→"abc";
    char c =⏎→'x';
    string d =
#pragma→warning disable CS0168
        "abc";
    void M(string a, string b)
    {
        M("a"→, "b");
    }
}
```

Real output: none of the four tabs is reported, and no fix is offered — a tab on a line before a literal (indentation or a directive line) and a tab directly after a literal on its line are skipped. The same tab after `=` on the same line, in front of `$"abc"`, or on a directive line before `int y;` is reported. The formatter removes or replaces all four (`string s = "abc";`, `char c = 'x';`, `#pragma    warning …` per FMT-CLEAN-003, `M("a", "b");`).

Why it looks wrong: these tabs are whitespace outside the literal; the rule page says tabs must not appear outside literals, and analyzer and formatter disagree.

Touches RH5601-001, RH5601-002, FMT-CLEAN-003.

---

### SDD open questions: sdd/specs/analyzer/RH5602.md

**Q1 — Trailing whitespace on a line that holds a string literal is not reported.**

Input:

```text
class C
{
    string s = "a";···
    string t = $"{s}";··
    string u = @"a
b";··
}
```

Real output: no diagnostic — every line that a regular, verbatim, raw, interpolated, or UTF-8 string literal touches is skipped as a whole, including whitespace after the literal has ended. The formatter removes all three runs (FMT-CLEAN-001).

Why it looks wrong: the whitespace is outside the literal, so it is ordinary trailing whitespace; only whitespace inside a multi-line literal is content (RH5602-002).

Touches RH5602-001, RH5602-002, FMT-CLEAN-001.

---

### SDD open questions: sdd/specs/analyzer/RH5603.md

**Q1 — A file that ends with a directive or disabled text is reported.**

Input: `class C⏎{⏎}⏎#pragma warning restore CS1591⏎⏎`, and `class C⏎{⏎}⏎#if NEVER⏎int ignored;⏎#endif⏎`.

Real output: both are reported, and the code fix removes every line break after the directive, including the one that ends it. The formatter keeps the directive's line break (FMT-CLEAN-007), so a formatted file that ends with a directive always raises RH5603.

Why it looks wrong: analyzer and formatter disagree about the same end of file. Readings: (a) RH5603 accepts the line break that ends a final directive; (b) FMT-CLEAN-007 changes and the formatter removes it.

Touches RH5603-001, RH5603-002, FMT-CLEAN-007.

**Q2 — A whitespace-only last line survives the fix.**

Input: `class C⏎{⏎}⏎···⏎`.

Real output: only the final line break is reported; the code fix leaves `class C⏎{⏎}⏎···`, which still has a line break after the last token and is no longer reported by RH5603 (RH5602 then reports the spaces). The formatter removes the whole tail (FMT-CLEAN-005).

Why it looks wrong: the rule page says the fix deletes "the line break — and any blank lines — after the last source character", but one application does not reach that end of file.

Touches RH5603-001, RH5603-003, RH5603-004, FMT-CLEAN-005.

---

### SDD open questions: sdd/specs/formatter/pipeline.md

**Q1 — A lone `\r` line break counts as `\n` when the predominant line ending is detected.**

Input: `class C\r\n{\r    int x;\r    int y;\r}` — one `\r\n` and four lone `\r`.

Real output: the predominant line ending is detected as `\n`, because every one-character terminator is counted as `\n`. The formatter writes `\n` everywhere, and RH5604 reports all five lines and its code fix converts them to `\n` — a line ending that occurs nowhere in the input.

Why it looks wrong: FMT-PIPE-007 only describes `\r\n` against `\n`; read literally ("`\r\n` when it occurs at least as often as `\n`") it gives `\r\n` for this input. Readings: (a) count a lone `\r` as its own kind (result `\r`); (b) count it with `\r\n`; (c) keep the current behavior and state it in FMT-PIPE-007.

Touches FMT-PIPE-007, RH5604-001, RH5604-005 (RH5604 deliberately states nothing for lone `\r`).

---

### SDD open questions: sdd/specs/analyzer/RH6003.md

**Q1 — Whitespace between a continuation-line comment and the semicolon is reported, but the formatter keeps it.**

Input:

```text
class C
{
    void M()
    {
        int value = 1
            /* keep */ ;
    }
}
```

Real output: the space before `;` is reported, and no code fix is offered, because a comment sits in the gap (SHARED-WS-002). The formatter keeps the space, because the gap between `1` and `;` holds a line break (FMT-SPACE-001), so formatted code keeps raising RH6003 with no fix.

Why it looks wrong: analyzer and formatter disagree, and the diagnostic can only be cleared by hand. Readings: (a) RH6003 does not report a run whose gap to the previous token holds a line break; (b) the formatter removes the whitespace between a comment and the semicolon on a continuation line.

Touches RH6003-001, FMT-SPACE-001, FMT-SPACE-013.

---

### SDD open questions: sdd/specs/analyzer/RH6004.md

**Q1 — `# region` with whitespace after the `#` is not treated as a region.**

Input:

```text
class C
{
    void M()
    {
        # region Spaced
        # endregion
    }
}
```

Real output: both lines are reported, and the code fix moves them to column 0. The formatter recognizes them as region directives, indents them (FMT-INDENT-010), and appends `// Spaced` to the end directive, so formatted code raises RH6004 twice.

Why it looks wrong: C# allows whitespace between `#` and the directive name; the rule page exempts regions, and analyzer and formatter disagree.

Touches RH6004-001, RH6004-002, FMT-INDENT-010.

---

### SDD open questions: sdd/specs/analyzer/RH6005.md

**Q1 — Several spaces or a line break after `operator` are not reported, although the rule page asks for exactly one space.**

Input:

```text
class C
{
    public static C operator  -(C a, C b) => a;

    public static C operator
        +(C a, C b) => a;
}
```

Real output: neither is reported; any whitespace or line break after the keyword satisfies the analyzer. The formatter writes one space for the first declaration (FMT-SPACE-012) and keeps the line break of the second (`operator⏎    +(C a, C b)`). `documentation/rules/RH6005.md` says the keyword "must be followed by exactly one space".

Why it looks wrong: the rule page and the analyzer disagree. Readings: (a) the analyzer reports anything but one space — then the formatter's output for the second declaration would be reported too, unless a line break is accepted; (b) the rule page says "a space" — which still does not say whether a line break is accepted.

Touches RH6005-001, RH6005-002, FMT-SPACE-012.

---

### SDD open questions: sdd/specs/analyzer/RH6006.md

**Q1 — Whitespace between `(` and a comment on the same line is reported, but the formatter keeps it.**

Input:

```text
class C
{
    void M(int a)
    {
        M( /* c */a);
        M(  // d
            a);
    }
}
```

Real output: both runs are reported, and no code fix is offered, because a comment sits in the gap (SHARED-WS-002). The formatter keeps both: FMT-SPACE-013 only collapses whitespace in front of a comment (its own example keeps `M( /* four */a)`), and FMT-SPACE-001 leaves the gap before an end-of-line comment alone. Formatted code keeps raising RH6006 with no fix.

Why it looks wrong: analyzer and formatter disagree, and the diagnostic can only be cleared by hand. Readings: (a) RH6006 does not report whitespace that a comment follows; (b) the formatter removes it.

Touches RH6006-001, FMT-SPACE-001, FMT-SPACE-013.

---

### SDD open questions: sdd/specs/analyzer/RH6007.md

**Q1 — Whitespace between `[` and a comment on the same line is reported, but the formatter keeps it.**

Input:

```text
class C
{
    void M(int[] arr)
    {
        var z = arr[ /* c */0];
        var w = arr[  // d
            0];
    }
}
```

Real output: both runs are reported, and no code fix is offered, because a comment sits in the gap (SHARED-WS-002). The formatter keeps both: FMT-SPACE-013 only collapses whitespace in front of a comment, and FMT-SPACE-001 leaves the gap before an end-of-line comment alone. Formatted code keeps raising RH6007 with no fix.

Why it looks wrong: analyzer and formatter disagree, and the diagnostic can only be cleared by hand. Readings: (a) RH6007 does not report whitespace that a comment follows; (b) the formatter removes it.

Touches RH6007-001, FMT-SPACE-001, FMT-SPACE-013.

---

### SDD open questions: sdd/specs/analyzer/RH6009.md

**Q1 — Opening braces inside an interpolation's expression are never checked.**

Input:

```text
class C
{
    string M()
    {
        return $"{new int[]{1}.Length}";
    }
}
```

Real output: not reported — the analyzer skips every brace inside an interpolated string, not only the interpolation's own braces. The formatter puts a space in front of the initializer's `{` (FMT-SPACE-009 exempts only the braces of the interpolation itself). The spacing inside the initializer braces is not part of this question (thoenissen/Reihitsu#958).

Why it looks wrong: the initializer brace is ordinary code; the rule page exempts only "the `{value}` in `$"{value}"`", and analyzer and formatter disagree about the space in front of it.

Touches RH6009-001, RH6009-002, FMT-SPACE-009.

---

### SDD open questions: sdd/specs/analyzer/RH6010.md

**Q1 — Closing braces inside an interpolation's expression are never checked.**

Input:

```text
class C
{
    string M()
    {
        return $"{new int[] { 1}.Length}";
    }
}
```

Real output: not reported — the analyzer skips every brace inside an interpolated string, not only the interpolation's own braces. The formatter puts a space in front of the initializer's `}` (FMT-SPACE-009 exempts only the braces of the interpolation itself).

Why it looks wrong: the initializer brace is ordinary code; the rule page exempts only "the `{value}` in `$"{value}"`", and analyzer and formatter disagree about the space in front of it.

Touches RH6010-001, RH6010-002, FMT-SPACE-009.

---

### SDD open questions: sdd/specs/analyzer/RH6011.md

**Q1 — Whitespace between a continuation-line comment and `<` is reported, but the formatter keeps it.**

Input:

```text
using System.Collections.Generic;

class C
{
    void M()
    {
        List
            /* keep */ <int> a = null;
    }
}
```

Real output: the space before `<` is reported, and no code fix is offered, because a comment sits in the gap (SHARED-WS-002). The formatter keeps the space, because the gap between `List` and `<` holds a line break (FMT-SPACE-001), so formatted code keeps raising RH6011 with no fix.

Why it looks wrong: analyzer and formatter disagree, and the diagnostic can only be cleared by hand. Readings: (a) RH6011 does not report a run whose gap to the previous token holds a line break; (b) the formatter removes the whitespace between a comment and `<` on a continuation line.

Touches RH6011-001, FMT-SPACE-001, FMT-SPACE-013.

---

### SDD open questions: sdd/specs/analyzer/RH6012.md

**Q1 — Whitespace between a continuation-line comment and `>` is reported, but the formatter keeps it.**

Input:

```text
using System.Collections.Generic;

class C
{
    void M()
    {
        List<int
            /* keep */ > a = null;
    }
}
```

Real output: the space before `>` is reported, and no code fix is offered, because a comment sits in the gap (SHARED-WS-002). The formatter keeps the space, because the gap between `int` and `>` holds a line break (FMT-SPACE-001), so formatted code keeps raising RH6012 with no fix.

Why it looks wrong: analyzer and formatter disagree, and the diagnostic can only be cleared by hand. Readings: (a) RH6012 does not report a run whose gap to the previous token holds a line break; (b) the formatter removes the whitespace between a comment and `>` on a continuation line.

Touches RH6012-001, FMT-SPACE-001, FMT-SPACE-013.

---

### SDD open questions: sdd/specs/formatter/horizontal-spacing.md

(Low priority.)

**Q1 — A delimited documentation comment stays glued to the following brace.**

Input:

```text
class C
{
    int[] a = new int[]/** y */{ 1 };
    int[] b = new int[]/* y */{ 1 };
}
```

Real output:

```text
class C
{
    int[] a = new int[]

              /** y */{ 1 };
    int[] b = new int[]/* y */ { 1 };
}
```

The `/* */` comment gets a space before the brace (FMT-SPACE-013); the `/** */` comment is moved onto its own line and stays glued to `{`, and RH6009 reports the brace in the formatted code.

Why it looks wrong: the two comment forms are treated differently, and the formatter's output raises RH6009. FMT-SPACE-014 keeps the whitespace in front of a comment the right token carries and removes whitespace after it only when no space is allowed; it does not say whether the space FMT-SPACE-009 asks for in front of a brace is inserted after such a comment.

Touches FMT-SPACE-009, FMT-SPACE-013, FMT-SPACE-014, RH6009-001.

---

## Follow-up candidates (out of scope)

- Formatter bug against FMT-INDENT-009 (the spec is clear, not an open question): an active directive that directly follows disabled text keeps its indentation. With `#if NEVER` / `x++;` / `#else` / `x--;` / `#endif`, all indented by eight spaces inside a method, the formatter moves `#if` and `#endif` to column 0 and leaves `#else` indented; with `#elif ALSO_NEVER` instead of `#else`, `#endif` stays indented too. RH6004 reports the formatted output. Not one of thoenissen/Reihitsu#957–#960.
- Tooling (from the skeptic): `scripts/trace.sh` is committed with mode `100644`, so `scripts/trace.sh …` as documented in `AGENTS.md` fails with "Permission denied".
