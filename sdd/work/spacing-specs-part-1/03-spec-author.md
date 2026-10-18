Role: spec author
Commit: 3efe3b5ff8a32662a7c79a4fa50c6c1b4c95a2ed
Verdict: spec written

Repair round on `02-skeptic.md`, sections A, B (without the RH6002-003 item, which is not changed in this run), and C. The coordinator's message carried no intended fix or hoped-for verdict; nothing had to be ignored. Each item was re-derived with the scratch harness (real analyzer, code fix, and formatter) before the text changed.

## Changed requirements

- RH6001-002 — clarified: the unchecked keywords are named (`typeof`, `nameof`, `sizeof`, `checked`, `unchecked`, `default`, `new`); the universal "keywords not listed are not checked" is gone, because `fixed(` is reported (A1). `fixed` stays out of the spec (thoenissen/Reihitsu#957). Example gains `unchecked(…)` and `default(int)`.
- RH5601-001 — narrowed: only tabs where neither neighboring token is a string or character literal are claimed (A2, open question RH5601 Q2). Why credits FMT-CLEAN-003 for directive interiors (B); `Uses` gains FMT-CLEAN-003.
- RH5601-002 — narrowed: the comment exemption claims only comments not on a directive line (A3, folded into RH5601 Q1).
- RH5602-001 — narrowed: reported only when the last non-whitespace character ends a token or a `/* */` comment and no string literal is on the line; whitespace-only lines only outside string literals, comments, and disabled text (A4). `//` comments and directive lines (thoenissen/Reihitsu#959, #960) and string lines (RH5602 Q1) are no longer claimed either way.
- RH5603-001, RH5603-002 — narrowed to files whose line breaks follow the last character of a token or a comment (A5). This leaves out a final directive or disabled text (RH5603 Q1) and a whitespace-only last line (RH5603 Q2); RH5603-002 no longer decides Q2 by saying that a file ending in spaces is not reported.
- RH5604-001, RH5604-002, RH5604-005 — narrowed to files whose line breaks are `\r\n` and `\n` (A7, pipeline Q1). Caption corrected to "two `\r\n` against one `\n`"; intro states that a block's last line has no line break (B).
- RH5604-003 — clarified: only line breaks in the *text* of a literal are excluded; a line break inside an interpolation hole is counted and converted (verified: `$@"x{a +⏎ a}"` with an `\n` hole line break in a `\r\n` file is reported and fixed). Example gains an interpolated verbatim literal.
- RH6003-001, RH6011-001, RH6012-001 — narrowed: the run is reported when it follows code or a comment on the same line *and the token before ends on that line* (A8). A comment on a continuation line is left out and filed (RH6003, RH6011, RH6012 Q1); the Why parity claims now hold for every claimed input.
- RH6012-001 — text error fixed: `Dictionary< ,>` → `Dictionary<, >` (B).
- RH6005-002 — narrowed to "followed by exactly one space"; several spaces and a line break are no longer decided (A6, RH6005 Q1). The `operator⏎-` example was removed.
- RH6006-001, RH6007-001 — Why corrected: FMT-SPACE-004 for a run before a token, FMT-CLEAN-001 for a run at the end of the line (B); `Uses` gains FMT-CLEAN-001. RH6007-001 lists the bracket owners as examples, not as a closed list (D: indexer declarations, implicit element access, calling-convention lists, and `stackalloc[ ]` are reported too).
- RH6009-001, RH6010-001 — narrowed: a brace after the end of a `/* */` comment is claimed; a brace after `/** */` is not (D: the formatter leaves `void M()/** y */{` glued, filed as a horizontal-spacing question; for RH6010 nothing looks wrong, the case is just not claimed).
- SHARED-WS-001 — reworded for the blank-line fixes that share `WhitespaceSpanRemovalCodeFixProviderBase` (C): the intro names "a run of spaces and tabs, or a span of blank lines"; the normative sentence no longer forbids touching a line break inside the reported span ("no whitespace outside the span is changed"); the Why no longer claims that a reported span never holds a line break. `CommentSafeSpanReplacementCodeFixProviderBase` stays unowned (the skeptic agrees).

No ID was added, retired, or renumbered; the ledger and the index are unchanged.

### How it was verified

- Every C# example of the 16 specs re-run through the harness after the edits: every reported span equals its markup, every "no code fix is offered" block registers none, every before/after pair equals the applied fixes, and no changed example has a compiler error.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"`: 26 passed (including the `Uses` check).

## Examples for the tests

All 60 requirements, as in `01-spec-author.md`. Additionally, these existing tests now pin inputs that are left out and must not be tagged: `VerifyFixIsNotOfferedForContinuationLine…WithPrecedingComment` (and their CRLF twins) in the RH6003, RH6011, and RH6012 test classes; `VerifyLoneCarriageReturnInsideXmlDocumentationIsNormalizedToLf` (RH5604); `VerifyIssueIsDetectedAndFixedForTrailingWhitespaceInPragmaDirective` and the comment tests (RH5602); `VerifyFixIsNotOfferedWhenSpaceSeparatesAComment` (RH6006).

## Open questions to file

Final list — this replaces the list in `01-spec-author.md` (RH6005 Q2 from there is dropped: analyzer and formatter agree, and it asks for new behavior, as the skeptic found). No existing open-questions issue covers these specs; each block is one new sub-issue of thoenissen/Reihitsu#912 with the title given. Before filing, check that none duplicates thoenissen/Reihitsu#958 (formatter spacing rules for lambda arrows, conditionals, element access, `::`, named arguments, and initializer braces — none of the questions below is about those pairs). In the inputs, `→` is a tab, `·` a space, `⏎` a line break.

### `SDD open questions: sdd/specs/analyzer/RH5601.md`

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

Real output: all three tabs are reported, and the fix replaces each with four spaces — changing the region name, the comment, and the warning message. The formatter keeps all three (FMT-CLEAN-003 protects directive free text, FMT-CLEAN-004 comments) and copies the region name into `#endregion // Na→me`, so formatted code keeps raising RH5601. Why it looks wrong: the rule page and RH5601-002 exempt comments and treat user-authored text as content, but text on a directive line is not recognized as such; analyzer and formatter disagree. Readings: (a) RH5601 does not report free text or comments on a directive line; (b) the formatter replaces those tabs too. Touches RH5601-001, RH5601-002, FMT-CLEAN-003, FMT-CLEAN-004.

**Q2 — A tab in the whitespace next to a string or character literal is not reported.**

Input:

```text
class C
{
    string s =⏎→"abc";
    char c =⏎→'x';
    void M(string a, string b)
    {
        M("a"→, "b");
    }
}
```

Real output: none of the three tabs is reported, and no fix is offered. The same tab in front of `$"abc"` or after `=` on the same line is reported. The formatter removes all three (`string s = "abc";`, `char c = 'x';`, `M("a", "b");`). Why it looks wrong: the tabs are whitespace outside the literal; the rule page says tabs must not appear outside literals. Touches RH5601-001, RH5601-002.

### `SDD open questions: sdd/specs/analyzer/RH5602.md`

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

Real output: no diagnostic — every line that a regular, verbatim, raw, interpolated, or UTF-8 string literal touches is skipped as a whole, including whitespace after the literal has ended. The formatter removes all three runs (FMT-CLEAN-001). Why it looks wrong: the whitespace is outside the literal, so it is ordinary trailing whitespace; only whitespace inside a multi-line literal is content (RH5602-002). Touches RH5602-001, RH5602-002, FMT-CLEAN-001.

### `SDD open questions: sdd/specs/analyzer/RH5603.md`

**Q1 — A file that ends with a directive or disabled text is reported.**

Input: `class C⏎{⏎}⏎#pragma warning restore CS1591⏎⏎`, and `class C⏎{⏎}⏎#if NEVER⏎int ignored;⏎#endif⏎`.

Real output: both are reported, and the fix removes every line break after the directive, including the one that ends it. The formatter keeps the directive's line break (FMT-CLEAN-007), so a formatted file that ends with a directive always raises RH5603. Why it looks wrong: analyzer and formatter disagree about the same end of file. Readings: (a) RH5603 accepts the line break that ends a final directive; (b) FMT-CLEAN-007 changes and the formatter removes it. Touches RH5603-001, RH5603-002, FMT-CLEAN-007.

**Q2 — A whitespace-only last line survives the fix.**

Input: `class C⏎{⏎}⏎···⏎`.

Real output: only the final line break is reported; the fix leaves `class C⏎{⏎}⏎···`, which still has a line break after the last token and is no longer reported by RH5603 (RH5602 then reports the spaces). The formatter removes the whole tail (FMT-CLEAN-005), and the rule page says the fix deletes "the line break — and any blank lines — after the last source character". Why it looks wrong: one application of the fix does not reach the end of file the rule asks for. Touches RH5603-001, RH5603-003, RH5603-004, FMT-CLEAN-005.

### `SDD open questions: sdd/specs/formatter/pipeline.md`

**Q1 — A lone `\r` line break counts as `\n` when the predominant line ending is detected.**

Input: `class C\r\n{\r    int x;\r    int y;\r}` — one `\r\n` and four lone `\r`.

Real output: the predominant line ending is detected as `\n` (every one-character terminator is counted as `\n`), so the formatter writes `\n` everywhere, and RH5604 reports all five lines and converts them to `\n` — a line ending that occurs nowhere in the input. Why it looks wrong: FMT-PIPE-007 only describes `\r\n` against `\n`; read literally ("`\r\n` when it occurs at least as often as `\n`") it gives `\r\n` here. Readings: (a) count a lone `\r` as its own kind (result `\r`); (b) count it with `\r\n`; (c) keep the current behavior and state it in FMT-PIPE-007. Touches FMT-PIPE-007, RH5604-001, RH5604-005; RH5604 deliberately states nothing for lone `\r`.

### `SDD open questions: sdd/specs/analyzer/RH6003.md`

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

Real output: the space before `;` is reported, and no fix is offered (SHARED-WS-002). The formatter keeps it, because the gap holds a line break (FMT-SPACE-001), so formatted code keeps raising RH6003 with no fix. Why it looks wrong: analyzer and formatter disagree, and the diagnostic can never be cleared without editing the comment's placement by hand. Readings: (a) RH6003 does not report a run whose gap to the previous token holds a line break; (b) the formatter removes it. Touches RH6003-001, FMT-SPACE-001, FMT-SPACE-013.

### `SDD open questions: sdd/specs/analyzer/RH6004.md`

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

Real output: both lines are reported, and the fix moves them to column 0. The formatter recognizes them as region directives, indents them (FMT-INDENT-010), and appends `// Spaced` to the end directive, so formatted code raises RH6004 twice. Why it looks wrong: C# allows whitespace between `#` and the directive name; the rule page exempts regions, and analyzer and formatter disagree. Touches RH6004-001, RH6004-002, FMT-INDENT-010.

### `SDD open questions: sdd/specs/analyzer/RH6005.md`

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

Real output: neither is reported; any whitespace or line break after the keyword satisfies the analyzer. `documentation/rules/RH6005.md` says the keyword "must be followed by exactly one space"; the formatter writes one space for the first (FMT-SPACE-012). Readings: (a) the analyzer reports anything but one space; (b) the rule page says "a space". Touches RH6005-001, RH6005-002, FMT-SPACE-012.

### `SDD open questions: sdd/specs/analyzer/RH6006.md`

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

Real output: both runs are reported, and no fix is offered (SHARED-WS-002). The formatter keeps both: FMT-SPACE-013 only collapses whitespace in front of a comment (its own example keeps `M( /* four */a)`), and FMT-SPACE-001 leaves the gap before an end-of-line comment alone. Formatted code keeps raising RH6006 with no fix. Readings: (a) RH6006 does not report whitespace that a comment follows; (b) the formatter removes it. Touches RH6006-001, FMT-SPACE-001, FMT-SPACE-013.

### `SDD open questions: sdd/specs/analyzer/RH6007.md`

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

Real output: both runs are reported, and no fix is offered (SHARED-WS-002); the formatter keeps both (FMT-SPACE-001, FMT-SPACE-013), so formatted code keeps raising RH6007 with no fix. Readings as in the RH6006 question. Touches RH6007-001, FMT-SPACE-001, FMT-SPACE-013.

### `SDD open questions: sdd/specs/analyzer/RH6009.md`

**Q1 — Braces inside an interpolation's expression are never checked.**

Input: `return $"{new int[]{1}.Length}";`

Real output: not reported — the analyzer skips every brace that has an interpolated string as an ancestor, not only the interpolation's own braces. The formatter writes `new int[] {1 }` there (FMT-SPACE-009 exempts only the braces of the interpolation). Why it looks wrong: the initializer brace is ordinary code; the rule page exempts only "the `{value}` in `$"{value}"`". Touches RH6009-001, RH6009-002, FMT-SPACE-009.

### `SDD open questions: sdd/specs/analyzer/RH6010.md`

**Q1 — Braces inside an interpolation's expression are never checked.**

Input: `return $"{new int[] { 1}.Length}";`

Real output: not reported; the formatter writes `{ 1 }`. Same reasoning as the RH6009 question. Touches RH6010-001, RH6010-002, FMT-SPACE-009.

### `SDD open questions: sdd/specs/analyzer/RH6011.md`

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

Real output: the space before `<` is reported, and no fix is offered (SHARED-WS-002); the formatter keeps it because the gap holds a line break (FMT-SPACE-001), so formatted code keeps raising RH6011 with no fix. Readings as in the RH6003 question. Touches RH6011-001, FMT-SPACE-001, FMT-SPACE-013.

### `SDD open questions: sdd/specs/analyzer/RH6012.md`

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

Real output: the space before `>` is reported, and no fix is offered (SHARED-WS-002); the formatter keeps it (FMT-SPACE-001), so formatted code keeps raising RH6012 with no fix. Readings as in the RH6003 question. Touches RH6012-001, FMT-SPACE-001, FMT-SPACE-013.

### `SDD open questions: sdd/specs/formatter/horizontal-spacing.md`

(Low priority. thoenissen/Reihitsu#920 for this spec is closed, so a new issue is needed.)

**Q1 — A delimited documentation comment stays glued to the following brace.**

Input:

```text
class C
{
    void M()/** y */{
    }
}
```

Real output: the formatter moves the comment onto its own line and leaves `/** y */{` glued; RH6009 reports the brace in the formatted code. FMT-SPACE-014 keeps the whitespace in front of a comment the right token carries and removes whitespace after it only when no space is allowed; it says nothing about inserting the space a single-space rule (FMT-SPACE-009) asks for after the comment. Why it looks wrong: formatted code raises RH6009, and `/* y */{` gets the space (FMT-SPACE-013). Touches FMT-SPACE-009, FMT-SPACE-014, RH6009-001.

## Follow-up candidates (out of scope)

- Formatter bug against FMT-INDENT-009 (the spec is clear, not an open question): an active directive that directly follows disabled text keeps its indentation. With `#if NEVER` / `x++;` / `#else` / `x--;` / `#endif`, all indented by eight spaces inside a method, the formatter moves `#if` and `#endif` to column 0 and leaves `#else` indented; with `#elif ALSO_NEVER` instead of `#else`, `#endif` stays indented too, because it also follows disabled text. RH6004 reports the formatted output. Not one of thoenissen/Reihitsu#957, #959, #960; the lead checks #958.
- Tooling (from the skeptic): `scripts/trace.sh` is committed with mode `100644`, so `scripts/trace.sh …` as documented in `AGENTS.md` fails with "Permission denied".
