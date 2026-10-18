# Inventory: spacing/layout SDD open questions (#1038–#1052)

`→` tab, `·` space, `⏎` line break. Pattern cell: primary first, secondary after `;`.

| Issue | Q | Rule/spec | Input → real output | Why it looks wrong | Requirement IDs touched | Pattern | Duplicate of | Suggested decision |
|---|---|---|---|---|---|---|---|---|
| #1038 | Q1 | RH5601 | `#region Na→me`, `#pragma … // a→b`, `#warning a→b` → all reported; fix turns tabs into 4 spaces, changing region name, comment, warning text; formatter keeps tabs | Fix rewrites user-authored directive text; formatted code still reported | RH5601-001, RH5601-002, FMT-CLEAN-003, FMT-CLEAN-004 | A; E; NEW:directive-text | — | Bug: RH5601 skips directive free text/comments |
| #1038 | Q2 | RH5601 | `string s =⏎→"abc";`, `M("a"→, "b")`, `#pragma→…` before literal → not reported; formatter removes/replaces all | Tabs are outside the literal; same tab before non-literal is reported | RH5601-001, RH5601-002, FMT-CLEAN-003 | E; NEW:literal-overskip | #1038 Q4 | Bug: narrow literal exemption to literal text |
| #1038 | Q3 | RH5601 | `→/// Doc.` on 2nd/3rd `///` line → not reported (1st line is); formatter replaces with spaces | Tab is indentation; first and later doc lines treated differently | RH5601-001, RH5601-002, FMT-INDENT-008 | E | — | Bug: report continuation-line indentation tabs |
| #1038 | Q4 | RH5601 | `string s = /** c */→"abc";` → not reported; `int x = /** c */→1;` reported; formatter removes both | Only next-token kind differs; tab is outside literal | RH5601-001, RH5601-002 | E; NEW:literal-overskip | #1038 Q2 (same root) | Bug: decided with Q2 |
| #1039 | Q1 | RH5602 | `string s = "a";···` (also `$"…"`, `@"a⏎b";··`) → not reported; formatter removes (FMT-CLEAN-001) | Whitespace after literal end is ordinary trailing whitespace | RH5602-001, RH5602-002, FMT-CLEAN-001 | E; NEW:literal-overskip | — | Bug: skip only whitespace inside literal text |
| #1039 | Q2 | RH5602 | `$"{M(⏎1)··⏎}"`, whitespace-only line in `$@"{1 +⏎····⏎2}"` hole → not reported; formatter removes | Interpolation hole is code, not string content | RH5602-001, RH5602-002, FMT-CLEAN-001 | E; NEW:literal-overskip | #1039 Q1 (same root) | Bug: decided with Q1; holes are code |
| #1040 | Q1 | RH5603 | `…}⏎#pragma warning restore CS1591⏎` or `#endif⏎` at EOF → reported; fix strips directive's own line break; formatter keeps it | Formatted file ending with directive always reported | RH5603-001, RH5603-002, FMT-CLEAN-007 | E; NEW:directive-text | — | Clarification: accept the directive-terminating line break |
| #1040 | Q2 | RH5603 | `class C⏎{⏎}⏎···⏎` → fix leaves `}⏎···`; formatter removes whole tail | One fix application does not reach documented end state | RH5603-001, RH5603-003, RH5603-004, FMT-CLEAN-005 | D | — | Bug: fix removes whitespace-only tail too |
| #1040 | Q3 | RH5603 | `#pragma … // end⏎` / `#endif // end⏎` at EOF → reported, fix strips line break; formatter keeps | Same as Q1 with a trailing comment | RH5603-001, RH5603-002, FMT-CLEAN-007 | E; NEW:directive-text | #1040 Q1 | Same decision as Q1 |
| #1041 | Q1 | FMT pipeline / RH5604 | `class C\r\n{\r    int x;\r…` → predominant EOL detected `\n` (lone `\r` counted as `\n`); output all `\n` | Picks line ending absent from input; spec reads `\r\n` | FMT-PIPE-007, RH5604-001, RH5604-005 | OTHER | — | Clarification: state lone-`\r` counting in FMT-PIPE-007 |
| #1042 | Q1 | RH6003 | `int value = 1⏎    /* keep */ ;` → space before `;` reported, no fix; formatter keeps (line break in gap) | Unfixable diagnostic on formatted code | RH6003-001, FMT-SPACE-001, FMT-SPACE-013 | E; NEW:comment-gap | #1049 Q1, #1050 Q1 | Behavior change: formatter removes space after comment before `;` |
| #1043 | Q1 | RH6004 | `        # region Spaced` / `# endregion` → reported, fix moves to col 0; formatter treats as region, indents | `# region` is a valid region; rule exempts regions | RH6004-001, RH6004-002, FMT-INDENT-010 | E; NEW:directive-text | — | Bug: recognize `#` + whitespace + `region` |
| #1044 | Q1 | RH6005 | `operator  -(…)` and `operator⏎    +(…)` → not reported; formatter makes one space, keeps line break | Rule page says "exactly one space"; analyzer accepts any | RH6005-001, RH6005-002, FMT-SPACE-012 | OTHER; NEW:rule-page-mismatch | — | Clarification: rule page says one space or line break |
| #1044 | Q2 | RH6005 | `operator/** c */ -(…)` reported, `operator/* c */ *(…)` not; formatter keeps `/** */` glued | `/** */` vs `/* */` asymmetry; formatted code stays reported | RH6005-001, RH6005-002, FMT-SPACE-012, FMT-SPACE-014 | E; NEW:doc-comment-asymmetry | #1052 Q1 | Bug: treat `/** */` like `/* */` in both |
| #1045 | Q1 | RH6006 | `M( /* c */a);`, `M(  // d⏎a);` → reported, no fix; formatter keeps both | Unfixable diagnostic on formatted code | RH6006-001, FMT-SPACE-001, FMT-SPACE-013 | E; NEW:comment-gap | #1046 Q1 | Behavior change: RH6006 ignores whitespace followed by a comment |
| #1046 | Q1 | RH6007 | `arr[ /* c */0]`, `arr[  // d⏎0]` → reported, no fix; formatter keeps both | Unfixable diagnostic on formatted code | RH6007-001, FMT-SPACE-001, FMT-SPACE-013 | E; NEW:comment-gap | #1045 Q1 | Same decision as #1045 Q1 |
| #1047 | Q1 | RH6009 | `$"{new int[]{1}.Length}"` → not reported; formatter adds space before `{` | Initializer brace is code; rule exempts only interpolation braces | RH6009-001, RH6009-002, FMT-SPACE-009 | E; NEW:literal-overskip | #1048 Q1 | Bug: check braces inside interpolation holes |
| #1048 | Q1 | RH6010 | `$"{new int[] { 1}.Length}"` → not reported; formatter adds space before `}` | Initializer brace is code; rule exempts only interpolation braces | RH6010-001, RH6010-002, FMT-SPACE-009 | E; NEW:literal-overskip | #1047 Q1 | Bug: same as #1047 |
| #1049 | Q1 | RH6011 | `List⏎    /* keep */ <int> a` → space before `<` reported, no fix; formatter keeps | Unfixable diagnostic on formatted code | RH6011-001, FMT-SPACE-001, FMT-SPACE-013 | E; NEW:comment-gap | #1042 Q1, #1050 Q1 | Same decision as #1042 Q1 |
| #1049 | Q2 | RH6011 | `List /** c */ <int> a` → reported, no fix; formatter gives `List⏎⏎/** c */ <int> a` | Formatter splits declaration with blank line; still reported | RH6011-001, FMT-SPACE-013, FMT-SPACE-014 | E; NEW:doc-comment-asymmetry | #1051 Q1 (same relocation) | Bug: formatter keeps `/** */` inline, drops space |
| #1050 | Q1 | RH6012 | `List<int⏎    /* keep */ > a` → space before `>` reported, no fix; formatter keeps | Unfixable diagnostic on formatted code | RH6012-001, FMT-SPACE-001, FMT-SPACE-013 | E; NEW:comment-gap | #1042 Q1, #1049 Q1 | Same decision as #1042 Q1 |
| #1051 | Q1 | FMT horizontal-spacing / RH6009 | `new int[]/** y */{ 1 }` → `new int[]⏎⏎/** y */{ 1 }`; `/* y */` gets ` {` | `/** */` relocated with blank line, glued; RH6009 reports output | FMT-SPACE-009, FMT-SPACE-013, FMT-SPACE-014, RH6009-001 | E; NEW:doc-comment-asymmetry | #1049 Q2 | Bug: formatter keeps comment inline, inserts space |
| #1052 | Q1 | RH6001 | `if/** c */ (a > 1)` reported, `if/* c */ (a > 2)` not; formatter leaves both `/** */` lines | `/** */` vs `/* */` asymmetry; formatted code stays reported | RH6001-001, RH6001-002, FMT-SPACE-012, FMT-SPACE-014 | E; NEW:doc-comment-asymmetry | #1044 Q2 | Bug: decided with #1044 Q2 |

## Cross-issue duplicates

- Continuation-line comment before closing/inner token (space after comment kept by formatter, reported without fix): #1042 Q1, #1049 Q1, #1050 Q1 — one decision.
- Whitespace after opener before a comment (`( /* c */`, `[ /* c */`, and before `//`): #1045 Q1, #1046 Q1 — one decision. Related to the group above (both are NEW:comment-gap).
- Keyword glued to `/** */` comment (`operator/** c */`, `if/** c */`): #1044 Q2, #1052 Q1 — one decision.
- `/** */` comment relocated onto its own line after a blank line inside an expression/declaration: #1049 Q2, #1051 Q1 — one formatter decision.
- Braces inside interpolation holes skipped: #1047 Q1, #1048 Q1.
- Within-issue: #1038 Q2/Q4; #1039 Q1/Q2; #1040 Q1/Q3.

## NEW patterns

- NEW:comment-gap — whitespace adjacent to a comment is reported with no fix (SHARED-WS-002) while the formatter keeps it: #1042 Q1, #1045 Q1, #1046 Q1, #1049 Q1, #1050 Q1.
- NEW:doc-comment-asymmetry — `/** */` handled differently from `/* */` by analyzer and/or formatter: #1044 Q2, #1049 Q2, #1051 Q1, #1052 Q1.
- NEW:literal-overskip — analyzer exempts whole lines/strings around literals or interpolations instead of only literal text: #1038 Q2, #1038 Q4, #1039 Q1, #1039 Q2, #1047 Q1, #1048 Q1.
- NEW:directive-text — directive lines (free text, trailing line break, `# region` spelling) handled inconsistently: #1038 Q1, #1040 Q1, #1040 Q3, #1043 Q1.
- NEW:rule-page-mismatch (single instance here, may recur elsewhere): #1044 Q1.
