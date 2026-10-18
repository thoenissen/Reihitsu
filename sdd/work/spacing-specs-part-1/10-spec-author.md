Role: spec author
Commit: b4e52fc7ebda5edc1fccb4fc36ab7528cf86d3a5
Verdict: spec written

Repair round on `09-auditor.md` findings 1–8 (worklist in `00-lead.md`). The coordinator's message carried no intended fix or hoped-for verdict; the auditor's suggested changes were treated as suggestions. Every finding was reproduced with the scratch harness (real analyzer, code fix, and formatter) before the text changed; all reproduced as described.

## Changed requirements

- RH5601-001 — narrowed (finding 1). The tab must not indent the second or a later line of a `///` comment (verified: a tab before a continuation `///` line is not reported; before the first `///` line it is), and condition 1 now also requires that no `/** */` comment stands between the last code token and the tab (verified: `string s = /** c */→"abc";` not reported, `int x = /** c */→1;` reported). Both left-out shapes are new open questions below.
- RH5601-002 — clarified (finding 6): "a string literal (regular, verbatim, raw, or UTF-8) or the text of an interpolated string"; a tab inside a hole (`$"{1→+ 2}"`, verified reported) is no longer exempted by the wording.
- RH5602-001 — narrowed (finding 6): reported only when no string literal touches the line, where every line an interpolated string spans, its holes included, counts as touched; the same condition for whitespace-only lines. Verified: `1)··` inside a multi-line hole and a whitespace-only line inside a `$@"{…}"` hole are not reported; the formatter removes both. New open question below.
- RH5603-001, RH5603-002 — narrowed (finding 5): the comment branch is "a `//` or `/* */` comment that is not on a directive line". `…⏎#pragma warning restore CS1591 // end⏎` (verified reported; the formatter keeps the line break) is now RH5603 Q1's shape, unclaimed.
- RH6001-001, RH6001-002 — clarified/narrowed (findings 2, 4): a "`/* */` comment that is not a documentation comment" alone does not separate keyword and `(`; RH6001-002 now reads "followed by whitespace … or by a line break before its `(`, either directly or after a `/* */` comment that is not a documentation comment". A `/** */` comment after the keyword is in neither requirement (verified: `if/** c */(…)`, `if/** c */ (…)`, `if/** c */⏎(…)` are reported; `if/* c */ (…)` is not; the formatter keeps all three, so formatted code raises RH6001). New open question below.
- RH6004-002 — narrowed (finding 3): the region exemption covers `#region`/`#endregion` "written without whitespace between the `#` and the keyword". `# region` is left to thoenissen/Reihitsu#1043 Q1, no longer decided.
- RH6005-001 — narrowed (finding 4): "A `/* */` comment that is not a documentation comment, alone after the keyword, does not separate it." `operator/** c */+` and `operator/** c */ +` (both verified reported; the formatter keeps both) are unclaimed. New open question below.
- RH6011-001 — narrowed (finding 4): the run must follow code or a "`/* */` comment that is not a documentation comment". `List /** c */ <int> a = null;` (verified: reported, no fix; the formatter moves the comment to its own line after a blank line and the result is still reported with no fix) is unclaimed. New open question below.
- RH6003-001, RH6008-001, RH6012-001 — Why corrected (finding 7): FMT-SPACE-014 is cited for a `/** */` comment the token carries; `Uses` gains FMT-SPACE-014. Parity re-verified: `int _b /** y */ ;`, `arr[0 /** e */ ]`, `List<int /** d */ >` are reported and formatted to `/** y */;`, `/** e */]`, `/** d */>`, which raise nothing.
- RH6009-001, RH6010-001 — clarified (finding 8): stated positively, "whose preceding character is the last character of a token … or of a `/* */` comment that is not a documentation comment"; `/** y */{` is no longer read as claimed.

No ID added, retired, or renumbered; ledger and index unchanged.

### How it was verified

- All C# examples of the 16 specs re-run through the harness: spans equal the markup, "no code fix is offered" blocks register none, before/after pairs equal the applied fixes, no compiler errors. No example changed in this round; only normative sentences, Whys, and `Uses`.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RequirementCoverageTests"`: 39 passed.

## Tags for the test implementer

No tag has to be removed or moved. I checked every test of the narrowed requirements at head b4e52fc for the shapes that are now unclaimed: no tagged test uses a tab before a continuation `///` line or after a `/** */` before a literal (RH5601; the `/** col→umn */` test is a comment-content case under RH5601-002, still claimed), an interpolation hole line (RH5602; the tagged interpolated tests have whitespace in the literal *text*), a final directive line with a trailing comment (RH5603), `# region` (RH6004), or a `/** */` comment after a keyword or before `<`/a brace (RH6001, RH6005, RH6011, RH6009, RH6010). Finding 9 (`VerifyFirstEncounteredLineEndingWinsWhenCountsAreTied`) is the test implementer's own item. Tests added later must not tag any of these shapes.

## Open questions to file

`→` is a tab, `·` a space, `⏎` a line break.

### Addition to thoenissen/Reihitsu#1038 (`SDD open questions: sdd/specs/analyzer/RH5601.md`)

## Q3 — A tab that indents a continuation line of a `///` comment is not reported

Input:

```text
class C
{
    /// <summary>
→/// Doc.
→/// </summary>
    void M()
    {
    }
}
```

Real output: neither tab is reported, and no fix is offered. A tab in front of the *first* `///` line is reported and fixed. The formatter replaces both tabs (each line becomes `    /// …`, FMT-INDENT-008), so it does what the analyzer does not ask for.

Why it looks wrong: the tab is indentation, not comment content; the rule page asks for spaces in all indentation and exempts only tabs inside comments, and the first and later lines of the same comment are treated differently.

Touches RH5601-001, RH5601-002, FMT-INDENT-008.

## Q4 — A tab after a mid-line `/** */` comment in front of a literal is not reported

Input:

```text
class C
{
    string s = /** c */→"abc";
    int x = /** c */→1;
}
```

Real output: the tab before `"abc"` is not reported, and no fix is offered; the same tab before `1` is reported and fixed. The formatter removes both tabs.

Why it looks wrong: in both lines the tab is whitespace between a comment and the next token, outside any literal; only the kind of the next token differs. This is the same behavior as Q2 (a tab in whitespace that belongs to a literal), reached through a documentation comment.

Touches RH5601-001, RH5601-002.

### Addition to thoenissen/Reihitsu#1039 (`SDD open questions: sdd/specs/analyzer/RH5602.md`)

## Q2 — Trailing whitespace inside an interpolation hole is not reported

Input:

```text
class C
{
    string M(int a)
    {
        return $"{M(
            1)··
        }";
    }

    string N()
    {
        return $@"{1 +
····
            2}";
    }
}
```

Real output: neither the two spaces after `1)` nor the whitespace-only line inside the `$@"…"` hole is reported — every line an interpolated string spans is skipped, holes included. The formatter removes both (FMT-CLEAN-001).

Why it looks wrong: a hole is code, not string text; the whitespace there changes no value. Only whitespace in the text of a multi-line literal is content (RH5602-002).

Touches RH5602-001, RH5602-002, FMT-CLEAN-001.

### Addition to thoenissen/Reihitsu#1040 (`SDD open questions: sdd/specs/analyzer/RH5603.md`)

## Q3 — A final directive line that ends with a comment is reported like Q1

Input: `class C⏎{⏎}⏎#pragma warning restore CS1591 // end⏎`, and `class C⏎{⏎}⏎#if NEVER⏎#endif // end⏎`.

Real output: both are reported, and the fix removes the line break after the directive. The formatter keeps it (FMT-CLEAN-007), so formatted code raises RH5603.

Why it looks wrong: this is Q1's shape — a file that ends with a directive — with a comment on the directive line; the answer to Q1 decides it too, and it is listed so the decision covers it.

Touches RH5603-001, RH5603-002, FMT-CLEAN-007.

### New issue: `SDD open questions: sdd/specs/analyzer/RH6001.md` (sub-issue of thoenissen/Reihitsu#912)

## Q1 — A `/** */` comment after a statement keyword is treated differently from a `/* */` comment

Input:

```text
class C
{
    void M(int a)
    {
        if/** c */(a > 0)
        {
        }

        if/** c */ (a > 1)
        {
        }

        if/* c */ (a > 2)
        {
        }
    }
}
```

Real output: the first two `if` keywords are reported, the third is not. The fix inserts a space after the keyword (`if /** c */(a > 0)`). The formatter leaves the first two lines unchanged and keeps `if/* c */ (a > 2)`, so formatted code keeps raising RH6001 twice.

Why it looks wrong: the two comment forms sit in the same place, but a `/* */` comment followed by a space satisfies the rule while a `/** */` comment followed by a space does not; and the formatter does not produce code the rule accepts. Readings: (a) the formatter inserts the space after the keyword in front of a `/** */` comment (FMT-SPACE-012, FMT-SPACE-014); (b) the analyzer treats a `/** */` comment like a `/* */` comment, so `if/** c */ (` is accepted, and the formatter writes the space after the comment.

Touches RH6001-001, RH6001-002, FMT-SPACE-012, FMT-SPACE-014.

### Addition to thoenissen/Reihitsu#1044 (`SDD open questions: sdd/specs/analyzer/RH6005.md`)

## Q2 — A `/** */` comment after `operator` is treated differently from a `/* */` comment

Input:

```text
class C
{
    public static C operator/** c */+(C a, C b) => a;

    public static C operator/** c */ -(C a, C b) => a;

    public static C operator/* c */ *(C a, C b) => a;
}
```

Real output: the first two `operator` keywords are reported, the third is not. The fix inserts a space after the keyword (`operator /** c */+`). The formatter keeps the first two glued (`operator/** c */+`, `operator/** c */ -`), so formatted code keeps raising RH6005 twice.

Why it looks wrong: a `/* */` comment followed by a space satisfies the rule while a `/** */` comment followed by a space does not, and the formatter does not produce code the rule accepts. Readings: (a) the formatter inserts the space after `operator` in front of a `/** */` comment (FMT-SPACE-012, FMT-SPACE-014); (b) the analyzer treats a `/** */` comment like a `/* */` comment, and the formatter writes the space after the comment.

Touches RH6005-001, RH6005-002, FMT-SPACE-012, FMT-SPACE-014.

### Addition to thoenissen/Reihitsu#1049 (`SDD open questions: sdd/specs/analyzer/RH6011.md`)

## Q2 — A `/** */` comment in front of `<` is moved to its own line and stays reported

Input:

```text
using System.Collections.Generic;

class C
{
    void M()
    {
        List /** c */ <int> a = null;
    }
}
```

Real output: the space between the comment and `<` is reported, and no code fix is offered, because a comment sits in the gap (SHARED-WS-002). The formatter rewrites the declaration to

```text
        List

        /** c */ <int> a = null;
```

— the comment moves to its own line after a blank line inside the declaration — and the space before `<` stays, so the formatted code is still reported with no fix.

Why it looks wrong: analyzer and formatter disagree, and the formatter's output both splits a declaration with a blank line and keeps the whitespace RH6011 reports; the diagnostic can only be cleared by hand. The same `/* c */` comment is handled cleanly (`List /* c */<int>`, FMT-SPACE-013). Readings: (a) the formatter keeps a `/** */` comment in front of `<` in place and removes the whitespace after it (FMT-SPACE-014), as it does before `;`, `]`, and `>`; (b) RH6011 does not report whitespace between a `/** */` comment and `<`.

Touches RH6011-001, FMT-SPACE-013, FMT-SPACE-014.

## Follow-up candidates (unchanged)

- Formatter bug against FMT-INDENT-009: an active directive directly after disabled text keeps its indentation.
- `scripts/trace.sh` is committed without the executable bit.
- (Auditor hint) The blank line the formatter inserts inside `List /** c */ <int> a = null;` is part of #1049 Q2 above, not a separate item.
