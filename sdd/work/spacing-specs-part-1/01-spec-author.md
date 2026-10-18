Role: spec author
Commit: 556567e70791fe7446066d6618edb751fcfdcf49
Verdict: spec written

The lead handover carried no intended fix or hoped-for verdict; nothing had to be ignored.

## Changed requirements

New shared spec `sdd/specs/shared/whitespace-removal-fixes.md` (prefix `SHARED-WS`, code owners `RemoveWhitespaceRunCodeFixProviderBase`, `WhitespaceSpanRemovalCodeFixProviderBase`). Created because more than one spec relies on each requirement: SHARED-WS-001 is used by RH5602, RH6003, RH6004, RH6006, RH6007, RH6008, RH6011, RH6012; SHARED-WS-002 by RH6003, RH6006, RH6007, RH6008, RH6011, RH6012. It is worded in terms of "the reported whitespace" and "the two tokens around it", so the RH6013–RH6024 batch (thoenissen/Reihitsu#987) can use it unchanged; RH6013–RH6015, RH6017–RH6020, and RH6022 derive from `RemoveWhitespaceRunCodeFixProviderBase` too. `CommentSafeSpanReplacementCodeFixProviderBase` (the guard's actual implementation, also used by RH5402, RH6016, RH6021, `CollapseTokenGapCodeFixProviderBase`) is deliberately not a code owner: its other users are outside this batch.

- SHARED-WS-001 — The fix deletes exactly the reported whitespace — added
- SHARED-WS-002 — A comment or directive in the token gap withholds the fix — added
- RH5601-001 — Every tab in the code is reported — added
- RH5601-002 — Tabs in literals, comments, and disabled text are not reported — added
- RH5601-003 — One diagnostic per tab — added
- RH5601-004 — The fix replaces the tab with four spaces — added
- RH5602-001 — Whitespace before a line break is reported — added
- RH5602-002 — Whitespace inside a multi-line string literal is not reported — added
- RH5602-003 — One diagnostic per line — added
- RH5602-004 — The fix deletes the trailing whitespace — added
- RH5603-001 — Line breaks at the end of the file are reported — added
- RH5603-002 — A file that ends with its last character is not reported — added
- RH5603-003 — One diagnostic on the trailing line breaks — added
- RH5603-004 — The fix deletes the trailing line breaks — added
- RH5604-001 — Lines that differ from the predominant line ending are reported — added
- RH5604-002 — A file with one kind of line ending is not reported — added
- RH5604-003 — Line breaks inside string literals do not count — added
- RH5604-004 — One diagnostic per line — added
- RH5604-005 — The fix converts every line ending of the file — added
- RH6001-001 — A statement keyword glued to its parenthesis is reported — added
- RH6001-002 — A separated keyword and other keywords are not reported — added
- RH6001-003 — The diagnostic is on the keyword — added
- RH6001-004 — The fix inserts a space after the keyword — added
- RH6003-001 — Whitespace before a semicolon on the same line is reported — added
- RH6003-002 — A semicolon that starts its line is not reported — added
- RH6003-003 — One diagnostic per semicolon — added
- RH6003-004 — The fix deletes the whitespace unless a comment is in the gap — added
- RH6004-001 — An indented directive is reported — added
- RH6004-002 — Regions, inactive directives, and directive-like text are not reported — added
- RH6004-003 — One diagnostic per directive line — added
- RH6004-004 — The fix deletes the indentation — added
- RH6005-001 — An `operator` keyword glued to what follows is reported — added
- RH6005-002 — A separated keyword and references in documentation are not reported — added
- RH6005-003 — The diagnostic is on the keyword — added
- RH6005-004 — The fix inserts a space after the keyword — added
- RH6006-001 — Whitespace after an opening parenthesis is reported — added
- RH6006-002 — One diagnostic per parenthesis — added
- RH6006-003 — The fix deletes the whitespace unless a comment is in the gap — added
- RH6007-001 — Whitespace after an opening square bracket is reported — added
- RH6007-002 — One diagnostic per bracket — added
- RH6007-003 — The fix deletes the whitespace unless a comment is in the gap — added
- RH6008-001 — Whitespace before a closing square bracket on the same line is reported — added
- RH6008-002 — One diagnostic per bracket — added
- RH6008-003 — The fix deletes the whitespace unless a comment is in the gap — added
- RH6009-001 — An opening brace glued to the character before it is reported — added
- RH6009-002 — The braces of an interpolation are not reported — added
- RH6009-003 — The diagnostic is on the brace — added
- RH6009-004 — The fix inserts a space in front of the brace — added
- RH6010-001 — A closing brace glued to the character before it is reported — added
- RH6010-002 — The braces of an interpolation are not reported — added
- RH6010-003 — The diagnostic is on the brace — added
- RH6010-004 — The fix inserts a space in front of the brace — added
- RH6011-001 — Whitespace before an opening generic bracket on the same line is reported — added
- RH6011-002 — Type arguments in documentation references are not reported — added
- RH6011-003 — One diagnostic per bracket — added
- RH6011-004 — The fix deletes the whitespace unless a comment is in the gap — added
- RH6012-001 — Whitespace before a closing generic bracket on the same line is reported — added
- RH6012-002 — Type arguments in documentation references are not reported — added
- RH6012-003 — One diagnostic per bracket — added
- RH6012-004 — The fix deletes the whitespace unless a comment is in the gap — added

All 60 IDs are in `sdd/specs/requirement-ids.txt` (ordinal order checked; no existing line moved) and the 16 specs are in `sdd/specs/README.md`. No change doc: every requirement writes down existing behavior. No existing requirement of another spec and no rule page under `documentation/rules/` was changed — the documented behavior does not change.

Formatter policies are referenced, not restated: FMT-CLEAN-001/002/004/005/006, FMT-PIPE-007, FMT-EOL-001 to FMT-EOL-004, FMT-SPACE-004/005/009/012/013, FMT-INDENT-009/010.

Cases left out because they check the same policy as a known formatter issue (not re-filed):

- RH6001: `fixed(` (thoenissen/Reihitsu#957). RH6001 does report it today; the keyword list in RH6001-001 omits `fixed`.
- RH5602: trailing whitespace inside comments (thoenissen/Reihitsu#959), at the end of a directive line, and after the last token of a file without a final line break (thoenissen/Reihitsu#960).
- RH5604: line breaks inside block comments and disabled text (thoenissen/Reihitsu#959); kept only as a pointer in the non-goals.

Judgment calls the skeptic may want to challenge: RH6001-002, RH6009-001, and RH6010-001 state that whitespace of any width satisfies the rule (`while  (`, `int Q  {`, `set;  }` are not reported). The rule pages for these rules only ask for "a space", so this was specified; RH6005, whose page says "exactly one space", was not (see the open questions).

### How the statements were established

- Every C# example block of the 16 specs (and, as a control, of the existing `RH6002.md`) was extracted, its markup stripped (`→` = tab, `·` = space, `␍` = `\r`), and run through the rule's real analyzer and code-fix provider in a scratch harness built on `Reihitsu.Tooling`: the reported spans equal the markup of every block, the "no code fix is offered" blocks register no action, every before/after pair matches the result of applying the fixes (single fixes and the batch Fix All give the same text), and no example has a compiler error.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RequirementCoverageTests"`: 38 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`, listing exactly the 60 new IDs, because no test is tagged yet.

## Examples for the tests

Every requirement above has an example the tests can derive from; the uncovered list is the 60 IDs. Existing tests that appear to cover requirements already (by name; the test implementer verifies):

- RH5601 (`RH5601UseTabsCorrectlyAnalyzerTests`): `VerifyIssueIsDetectedAndFixed`, `VerifyIssueIsDetectedAndFixedForTabInPragmaDirective` (001, 004); the `VerifyNoDiagnosticsForTabIn…` tests and `VerifyCodeFixDoesNotModifyRawStringContent` (002); the Fix All scenario has two adjacent tabs (003).
- RH5602: `VerifyTrailingWhitespaceIsDetectedAndFixed`, `VerifyTrailingWhitespaceAfterMultiLineCommentIsStillFlagged` (001, 004); `VerifyRawStringsDoNotProduceDiagnostics`, `VerifyVerbatimStringsDoNotProduceDiagnostics` (002). Do not tag `VerifyIssueIsDetectedAndFixedForTrailingWhitespaceInPragmaDirective` or the comment tests: those cases are left out of the spec.
- RH5603: `VerifyTrailingNewlineIsDetectedAndFixed`, `VerifyNoDiagnosticsWhenFileDoesNotEndWithNewline`. No test for CRLF, several line breaks, or a final comment line.
- RH5604: most requirements have tests (`VerifyMixedLineEndings…`, `VerifyCrLfWinsTie…`, `VerifyNoDiagnosticsWhenFileUses…`, `VerifyLineEndingsInsideVerbatimStrings…`/`…RawStrings…`). Do not tag the block-comment and disabled-text tests: left out.
- RH6003 and RH6011/RH6012 have same-line, continuation-line, comment, and directive tests; RH6006 has `VerifyFixIsNotOfferedWhenSpaceSeparatesAComment` — check its input: a comment on the same line after `(` is an open question below and must not be tagged with an RH6006 requirement. `RemoveWhitespaceRunCodeFixProviderBaseTests` covers SHARED-WS-002 on both sides of the gap boundary.
- Thin coverage today (each rule has only clean/detect-and-fix tests): RH6001, RH6004, RH6005, RH6007, RH6009, RH6010. In particular there is no test for: RH6001 with a comment, a line break, or the non-listed keywords; RH6004 with a tab, `#else`/`#elif`, or a string literal; RH6005 with a comment or a conversion operator; RH6007 with collection expressions, list patterns, rank specifiers, attribute lists, or a line break; RH6008 with attribute lists; RH6009/RH6010 with comments or several spaces; SHARED-WS-002 with a directive in the gap.

## Open questions to file

None of these is in a spec. No existing `SDD open questions` issue covers these specs; each needs a new sub-issue of thoenissen/Reihitsu#912 under the title given.

### `SDD open questions: sdd/specs/analyzer/RH5601.md`

**Q1 — A tab inside a directive's free text is reported and rewritten.** Input (`→` = tab):

```text
class C
{
#region Na→me
    int x;
#endregion
    void M()
    {
#warning a→b
    }
}
```

Real output: both tabs are reported; the fix turns them into four spaces each, changing the region name and the warning message. The formatter keeps both tabs (FMT-CLEAN-003 protects directive free text as content) and copies the region name into `#endregion // Na→me`, so formatted code still raises RH5601 twice. Why it looks wrong: the analyzer rewrites text that FMT-CLEAN-003 and RH5601-002's own reasoning treat as user-authored content, and analyzer and formatter disagree. Readings: (a) directive free text is content — RH5601 must not report it; (b) it is code layout — the formatter must replace the tab too. Touches RH5601-001, RH5601-002, FMT-CLEAN-003.

### `SDD open questions: sdd/specs/analyzer/RH5602.md`

**Q1 — Trailing whitespace on a line that holds a string literal is not reported.** Input (`·` = trailing space):

```text
class C
{
    string s = "a";···
    string t = $"{s}";··
    string u = @"a
b";··
}
```

Real output: no diagnostic — every line that a regular, verbatim, raw, interpolated, or UTF-8 string literal touches is skipped as a whole, including whitespace after the literal has ended. The formatter removes all three runs (FMT-CLEAN-001). Why it looks wrong: the whitespace is outside the literal, so it is ordinary trailing whitespace; only whitespace inside a multi-line literal (RH5602-002) is content. Touches RH5602-001, RH5602-002.

### `SDD open questions: sdd/specs/analyzer/RH5603.md`

**Q1 — A file that ends with a directive or disabled text is reported.** Input: `class C\n{\n}\n#pragma warning restore CS1591\n\n` (and `…\n#if NEVER\nint ignored;\n#endif\n`). Real output: reported; the fix removes every line break after the directive, including the one that ends it. The formatter keeps that line break (FMT-CLEAN-007), so a formatted file ending with a directive always raises RH5603. Why it looks wrong: analyzer and formatter disagree about the same end of file. Readings: (a) RH5603 accepts the line break that ends a final directive; (b) FMT-CLEAN-007 changes and the formatter removes it. Touches RH5603-001, FMT-CLEAN-007.

**Q2 — A whitespace-only last line survives the fix.** Input: `class C\n{\n}\n···\n` (`·` = space). Real output: only the final `\n` is reported; the fix leaves `class C\n{\n}\n···`, which still has a line break after the last token and is no longer reported by RH5603 (RH5602 then reports the spaces). The formatter removes the whole tail (FMT-CLEAN-005), and the rule page says the fix deletes "the line break — and any blank lines — after the last source character". Why it looks wrong: one application of the fix does not reach the end of file the rule asks for. Touches RH5603-001, RH5603-003, RH5603-004.

### `SDD open questions: sdd/specs/formatter/pipeline.md`

(Belongs to the spec that owns line-ending detection; RH5604 inherits it. Shown for RH5604, the formatter output is the same.)

**Q1 — Lone `\r` line breaks count as `\n`.** Input: `class C\r\n{\r    int x;\r    int y;\r}` — one `\r\n` and four lone `\r`. Real output: the predominant line ending is detected as `\n`, so RH5604 reports all five lines and its fix converts every line break to `\n`, a line ending that occurs nowhere in the input; the formatter writes `\n` too. Why it looks wrong: FMT-PIPE-007 only describes `\r\n` against `\n`; counting a lone `\r` as `\n` picks a third ending as the "majority". Readings: (a) count `\r` as its own kind (the result would be `\r`); (b) count it as `\r\n`; (c) keep it as is and say so in FMT-PIPE-007. Touches FMT-PIPE-007, RH5604-001, RH5604-005. RH5604 deliberately states no requirement for lone `\r`.

### `SDD open questions: sdd/specs/analyzer/RH6004.md`

**Q1 — `# region` with a space after the `#` is not treated as a region.** Input:

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

Real output: both lines are reported and the fix moves them to column 0. The formatter recognizes them as region directives and indents them (FMT-INDENT-010), adding `// Spaced` to the end directive, so formatted code raises RH6004 twice. Why it looks wrong: C# allows whitespace between `#` and the directive name, and analyzer and formatter disagree. Touches RH6004-001, RH6004-002, FMT-INDENT-010.

### `SDD open questions: sdd/specs/analyzer/RH6005.md`

**Q1 — Several spaces after `operator` are not reported, although the rule page asks for exactly one.** Input: `public static C operator  -(C a, C b) => a;` Real output: not reported (any whitespace or line break satisfies the analyzer). `documentation/rules/RH6005.md` says "must be followed by exactly one space"; the formatter writes one (FMT-SPACE-012). Readings: (a) the analyzer reports anything but one space; (b) the page is corrected to "a space". Touches RH6005-001, RH6005-002.

**Q2 — `operator checked+` keeps no space between `checked` and the symbol.** Input: `public static C operator checked+(C a) => a;` Real output: not reported, and the formatter leaves it unchanged (no spacing rule covers `checked` followed by an operator symbol, FMT-SPACE-002). Why it might be wrong: `checked+` reads like the `operator+` that RH6005 exists to prevent. Touches RH6005-001, FMT-SPACE-012.

### `SDD open questions: sdd/specs/analyzer/RH6006.md`

**Q1 — Whitespace between `(` and a comment on the same line is reported, but the formatter keeps it.** Input:

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

Real output: both runs are reported; no fix is offered (SHARED-WS-002). The formatter keeps both: FMT-SPACE-013 only collapses whitespace in front of a comment, and FMT-SPACE-001 leaves the gap before an end-of-line comment alone. Formatted code therefore keeps raising RH6006, with no fix. Readings: (a) RH6006 does not report whitespace that a comment follows on the same line; (b) the formatter removes it. Touches RH6006-001, FMT-SPACE-001, FMT-SPACE-013.

### `SDD open questions: sdd/specs/analyzer/RH6007.md`

**Q1 — Whitespace between `[` and a comment on the same line is reported, but the formatter keeps it.** Input: `var z = arr[ /* c */0];` and `var w = arr[  // d` followed by `0];` on the next line. Real output and reasoning as in the RH6006 question: reported, no fix, formatter keeps the whitespace (FMT-SPACE-001, FMT-SPACE-013). Touches RH6007-001.

### `SDD open questions: sdd/specs/analyzer/RH6009.md`

**Q1 — Braces inside an interpolation's expression are never checked.** Input: `return $"{new int[]{1}.Length}";` Real output: not reported — the analyzer skips every brace that has an interpolated string as an ancestor, not only the interpolation's own braces. The formatter writes `new int[] {1 }` there (FMT-SPACE-009 exempts only the braces of the interpolation). Why it looks wrong: the initializer brace is ordinary code. Touches RH6009-001, RH6009-002.

### `SDD open questions: sdd/specs/analyzer/RH6010.md`

**Q1 — Braces inside an interpolation's expression are never checked.** Input: `return $"{new int[] { 1}.Length}";` Real output: not reported; the formatter writes `{ 1 }`. Same reasoning as the RH6009 question. Touches RH6010-001, RH6010-002.

## Follow-up candidates (out of scope)

- Formatter bug against FMT-INDENT-009 (not an open question — the spec is clear): an active directive that directly follows disabled text keeps its indentation. Input:

  ```text
  class C
  {
      void M(int x)
      {
          #if NEVER
          x++;
          #else
          x--;
          #endif
      }
  }
  ```

  Real formatter output: `#if NEVER` and `#endif` move to column 0, `#else` stays indented by eight spaces (the same with `#elif`), so formatted code raises RH6004. Not covered by thoenissen/Reihitsu#957–#960.
