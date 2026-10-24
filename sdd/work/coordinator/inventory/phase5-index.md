# Phase 5 — open-issue index

**215 open issues** (pull requests excluded) in `thoenissen/Reihitsu`, as of 2026-10-24.

Source: GitHub `list_issues` (state OPEN, all pages) for titles and labels; GitHub issue search for milestones. Parent/sub-issue links were not available cheaply and are omitted. The Family column refers to the split at the end.

| # | Title | Labels | Milestone | Family |
|---|---|---|---|---|
| 122 | [FEATURE] Visual Studio extension for Reihitsu formatter | enhancement, formatter | vNext | F8 |
| 352 | [ANALYZER FEATURE] Report methods whose cognitive complexity exceeds a limit | enhancement, analyzer | 1.4.0 | F8 |
| 353 | [ANALYZER FEATURE] Report methods whose line count exceeds a limit | enhancement, analyzer | 1.4.0 | F8 |
| 354 | [ANALYZER FEATURE] Report types whose line count exceeds a limit | enhancement, analyzer | 1.4.0 | F8 |
| 849 | [ANALYZER FEATURE] Get-only properties should use an expression body | enhancement, analyzer | 1.3.0 | F8 |
| 850 | [ANALYZER FEATURE] Get-only indexers should use an expression body | enhancement, analyzer | 1.3.0 | F8 |
| 851 | [ANALYZER FEATURE] Expression style get-only indexers should be single lined | enhancement, analyzer | 1.3.0 | F8 |
| 862 | [CODE REVIEW] Consolidate the auto-property accessor-list predicate into one owner | enhancement | Maintenance | F7 |
| 867 | [CODE REVIEW REPORT] Code-fix verifier compiles fixed states at warning level 4 and hides warning-wave diagnostics | enhancement | Maintenance | F8 |
| 877 | [CODE REVIEW REPORT] RH3002 follow-ups: exact re-parse check, cross-pair decision cost, and test hygiene (from #876) | enhancement, analyzer | Maintenance | F8 |
| 888 | [ANALYZER BUG] RH5201 reports a chain link whose line starts inside a multi-line block comment, and its fix makes no progress | bug, analyzer | 1.2.0 | F2 |
| 889 | [CODE REVIEW] Cover the mid-line-statement side of the chain code fixes' context selection with a test | analyzer | Maintenance | F2 |
| 892 | [ANALYZER FEATURE] Report a line break in front of an attached part of a member-access chain | enhancement, analyzer | 1.2.0 | F2 |
| 893 | [ANALYZER FEATURE] Report the remaining chain layouts the formatter changes without a diagnostic | enhancement, analyzer | 1.2.0 | F2 |
| 894 | [ANALYZER FEATURE] Report comments, directives and disabled text inside a ?. or !. operator | enhancement, analyzer | 1.2.0 | F2 |
| 895 | [FORMATTER FEATURE] Join attached parts outside member-access chains | enhancement, formatter | 1.3.0 | F2 |
| 896 | [FORMATTER FEATURE] Align a wrapped first call after a property prefix under the prefix's last dot | enhancement, formatter | 1.3.0 | F2 |
| 897 | [ANALYZER FEATURE] Update RH5112 and RH5048 titles and messages to say "first member access" instead of "first call" | enhancement, analyzer | 1.2.0 | F2 |
| 898 | [CODE REVIEW] scripts/trace.sh is committed without the executable bit | — | Maintenance | F8 |
| 946 | RH5005/RH5006 code fix never converges on `await using` / `await foreach` | — | 1.2.0 | F4 |
| 948 | [FORMATTER BUG] Bracing a switch section breaks a local that another section uses | bug, formatter | Triage | F1 |
| 949 | [FORMATTER BUG] A comment before the terminal break gets no blank line in the first pass | bug, formatter | Triage | F1 |
| 950 | Remove the unreachable switch brace-removal path and correct the SwitchExpressionContributor summary | formatter | Maintenance | F1 |
| 951 | [FORMATTER BUG] Raw-string alignment: interpolation holes and nested literals are not idempotent, tab indentation is kept | bug, formatter | Triage | F2 |
| 952 | [FORMATTER BUG] A directive directly after disabled text keeps its indentation | bug, formatter | Triage | F1 |
| 953 | [FORMATTER FEATURE] Indent continuation lines that no alignment rule owns one level deeper | enhancement, formatter | vNext | F2 |
| 954 | [FORMATTER BUG] An empty region directly after an opening brace gains a blank line in every pass | bug, formatter | Triage | F1 |
| 955 | [FORMATTER BUG] Line breaks: wrapped constructor-initializer colon and wrapped constraint clause | bug, formatter | Triage | F2 |
| 956 | [FORMATTER FEATURE] One switch-expression arm per line, and argument lists laid out like parameter lists | enhancement, formatter | vNext | F2 |
| 957 | [FORMATTER BUG] Spacing: `fixed(` gets no space, and a double space before a comment is not collapsed | bug, formatter | Triage | F2 |
| 958 | [FORMATTER FEATURE] Spacing rules for lambda arrows, conditionals, access, `::`, named arguments, and initializer braces | enhancement, formatter | vNext | F2 |
| 959 | [FORMATTER FEATURE] Normalize line ends inside comments and disabled text: trailing whitespace and line endings | enhancement, formatter | vNext | F1 |
| 960 | [FORMATTER BUG] Trailing whitespace survives at the end of a directive line and after the last token of the file | bug, formatter | Triage | F1 |
| 961 | [FORMATTER BUG] Documentation elements: end tag on the last content line, and wrapped inline elements | bug, formatter | Triage | F1 |
| 962 | [FORMATTER BUG] A documentation comment behind a statement or inside an expression is relocated although it documents nothing | bug, formatter | Triage | F1 |
| 963 | [FORMATTER BUG] Structural transforms: comment after `=>` glued to the new brace, and comment duplicated onto split fields | bug, formatter | Triage | F1 |
| 964 | [FORMATTER FEATURE] Normalize the `#region` line independently of capitalization | enhancement, formatter | vNext | F1 |
| 965 | [ANALYZER FEATURE] RH7207 and the using-directive phase accept a blank line before a comment-led directive | enhancement, analyzer, formatter | vNext | F5 |
| 966 | [ANALYZER BUG] Using ordering: a `#` inside a block comment blocks reordering, and `global::` names sort by their qualifier | bug, analyzer, formatter | Triage | F5 |
| 967 | [ANALYZER BUG] RH2005 code fix: lost indentation for `static public`, and broken references outside the type | bug, analyzer | Triage | F7 |
| 968 | [ANALYZER BUG] RH7103 compares written accessibility modifiers instead of effective accessibility | bug, analyzer | Triage | F5 |
| 969 | [ANALYZER BUG] Member-ordering code fixes reformat the whole containing type | bug, analyzer | Triage | F5 |
| 970 | [ANALYZER BUG] Ordering fixes refuse a move across a member whose body holds a balanced `#pragma` pair | bug, analyzer | Triage | F5 |
| 971 | [ANALYZER BUG] Casing fixes turn a leading acronym into `_hTTPClient` | bug, analyzer | Triage | F7 |
| 972 | [ANALYZER BUG] Statement blank-line fixes insert two blank lines when a block comment shares the statement's line | bug, analyzer | Triage | F4 |
| 973 | [ANALYZER BUG] RH1003 counts numeric sub-additions as separate string parts | bug, analyzer | Triage | F7 |
| 974 | [ANALYZER BUG] RH0001 accepts content after the root object of reihitsu.json | bug, analyzer | Triage | F7 |
| 975 | [FORMATTER BUG] `--dry-run` diff header prefixes a full path with `a/` and `b/` | bug, formatter | Triage | F8 |
| 976 | Playground: explain why generated input is left unchanged | enhancement | vNext | F8 |
| 1015 | [ANALYZER BUG] RH4102 Fix All corrects only the event kind it was invoked from | bug, analyzer | Triage | F7 |
| 1016 | [ANALYZER BUG] Casing fixes produce a name the rule still reports | bug, analyzer | 1.2.0 | F7 |
| 1032 | [CODE REVIEW REPORT] Remove the unused code-fix resources RH4116Title and RH4118Title | enhancement | Maintenance | F7 |
| 1033 | [ANALYZER FEATURE] Attribute placement fixes: limit the missing-brace guard to braces that enclose the attribute list | enhancement, analyzer | vNext | F4 |
| 1034 | [CODE REVIEW REPORT] Decide the spec ownership of AttributeTargetUtilities | enhancement | Maintenance | F4 |
| 1053 | [CODE REVIEW REPORT] Rule pages RH8001–RH8018 do not say which declarations count as private | enhancement | Maintenance | F6 |
| 1054 | [CODE REVIEW REPORT] lane-check classifies squad/decisions/ entries as tooling, so every escalated run widens to the tooling lane | enhancement | Maintenance | F8 |
| 1055 | [CODE REVIEW REPORT] Specify the documentation group of members declared in an extension block | enhancement | Maintenance | F6 |
| 1070 | Naming rules: do not report names dictated by a referenced assembly or a native entry point | — | Triage | F7 |
| 1071 | RH4001, RH4009: compare names by value, not with `@` or trivia | — | Triage | F7 |
| 1072 | Casing fixes: withhold the fix when the derived name is a reserved keyword | — | Triage | F7 |
| 1073 | Casing fixes: withhold renames that break compilation, inactive code, or a record's positional member | — | Triage | F7 |
| 1074 | Spacing rules: whitespace around literals and inside interpolation holes is code | — | SDD | F4 |
| 1075 | Blank-line rules: lines inside literals and comments are neither checked nor edited | — | SDD | F4 |
| 1076 | Blank-line fixes: converge in one application, never undo another rule, never be a no-op | — | SDD | F4 |
| 1077 | Treat a `/** */` comment that documents nothing as an ordinary block comment | — | SDD | F3 |
| 1078 | Spacing rules: do not report whitespace that touches a comment | — | SDD | F4 |
| 1079 | Spacing rules: report tab indentation on later `///` lines, `# region`, and repeated spaces after `operator` | — | SDD | F4 |
| 1080 | RH5022: report a blank line after an opening brace that carries a trailing comment | — | SDD | F4 |
| 1081 | SDD clarifications from question round 2: RH5025 sibling braces, lone `\r` in FMT-PIPE-007 | — | SDD | F4 |
| 1082 | Formatter: a file ending in a directive does not end with a line break either | — | SDD | F1 |
| 1083 | Replace tabs in region names; leave other directive text and comments alone | — | SDD | F1 |
| 1084 | Casing fixes: keep an existing prefix and every identifier character | — | SDD | F7 |
| 1085 | RH4001 keeps a migration timestamp prefix; RH4009 compares nested namespaces by their full name | — | SDD | F7 |
| 1086 | RH4001: do not report a file with top-level statements | — | SDD | F7 |
| 1087 | RH4115/RH4117: exempt `_`, stop checking local constants, give every deconstruction variable to RH4117 | — | SDD | F7 |
| 1088 | New rule: casing of local constants | — | SDD | F7 |
| 1089 | Check query range variables (RH4124) and extension-block receivers (RH4122) | — | SDD | F7 |
| 1090 | Rename RH4118TupleElementCasingAnalyzer to match what it checks | — | SDD | F7 |
| 1091 | Documentation rules: a declaration is documented exactly when the compiler's documentation for it has a summary | — | SDD | F6 |
| 1092 | Documentation rules: choose the private/non-private group from the symbol, and treat a static constructor as private | — | SDD | F6 |
| 1093 | Attribute separate-line rules: placement follows the attached declaration, the same layout gets the same verdict | — | SDD | F4 |
| 1094 | RH5513 fix: break before the attribute list and indent the declaration correctly | — | SDD | F4 |
| 1095 | A deconstruction declaration counts as a local declaration for RH5029, RH5021, and the formatter | — | SDD | F4 |
| 1096 | Statement blank-line fixes: indentation after a `case … when` guard, and the blank line after `///` | — | SDD | F4 |
| 1097 | Comment rules: chain anchor model, and `is`/`as` always reported | — | SDD | F3 |
| 1098 | Comment rules and formatter: a construct owns only what is inside its braces or arms | — | SDD | F3 |
| 1105 | Using-directive rules: compare names by value, not by spelling | — | SDD | F5 |
| 1131 | SDD: List the shared attribute-target requirements under `Uses` in formatter/line-breaks.md | — | SDD | F2 |
| 1132 | [CODE REVIEW REPORT] Check that the parameter documentation rules match `name` attributes the same way | enhancement | Maintenance | F6 |
| 1133 | [ANALYZER BUG] RH5601 reports the closing-line tab of an interpolated raw string, and its fix breaks the code | bug, analyzer | 1.2.0 | F4 |
| 1134 | [ANALYZER BUG] RH7109 reorders fields of types whose field order sets their memory layout | bug, analyzer | Triage | F5 |
| 1135 | [ANALYZER BUG] RH7309 swaps regions so that static field initializers run in a different order | bug, analyzer | 1.2.0 | F5 |
| 1136 | SDD: Cover relocating, reordering, and editing comments, directives, and disabled text in SHARED-FIX-003 | — | SDD | F8 |
| 1140 | Line-break and alignment fixes change only the code their diagnostic is about (P9) | — | SDD | F3 |
| 1141 | RH6023/RH6024 fixes rewrite only the two gaps next to the operator (P9) | — | SDD | F4 |
| 1142 | Attribute-list split fixes (RH5502–RH5531) change only the reported list and keep the document's line ending | — | SDD | F4 |
| 1143 | Fix All finishes in one application (SHARED-FIX-002) | — | SDD | F8 |
| 1144 | Fix output is laid out as the formatter lays it out: RH5101, RH5102, RH5205, RH5203 (P10) | — | SDD | F3 |
| 1145 | RH5103 fix: offer it behind embedded and labeled statements, and indent into one-line blocks | — | SDD | F3 |
| 1146 | RH5104: literal-safe fix, comment kinds alike, comments on code-free lines and after interpolated strings | — | SDD | F3 |
| 1147 | RH5110, RH5203, RH5205, RH5206, RH5301–RH5305, RH5308: a line led by a comment is not reported (P12) | — | SDD | F3 |
| 1148 | Formatter: the blank line before a line-leading comment only between statements and members (FMT-BLANK-011) | — | SDD | F1 |
| 1149 | Formatter: FMT-BREAK-016 covers enum members, so the RH5111 fix converges | — | SDD | F2 |
| 1150 | Formatter-delegating fixes are withheld when the formatter would return the document unchanged (P7) | — | SDD | F3 |
| 1151 | RH7302 fix renames an `#endregion` comment only when it is exactly the region name | — | SDD | F5 |
| 1152 | RH5204: the diagnostic on an indented `///` comment starts at `///`, without the line break | — | SDD | F3 |
| 1153 | Documentation rules: parameters documented as the compiler binds them, summary text that renders, and which summary RH8030 checks (P8b, P8c) | — | SDD | F6 |
| 1154 | RH5110: check record struct declarations | — | SDD | F3 |
| 1155 | RH5111: check nested assignments | — | SDD | F3 |
| 1156 | RH5103: report a statement that starts on its predecessor's last line, and check top-level statements | — | SDD | F3 |
| 1157 | RH5204: report misindented lines that do not start with a keyword | — | SDD | F3 |
| 1158 | RH5108: check primary constructor, lambda, anonymous method and extension parameter lists | — | SDD | F3 |
| 1159 | New rule: attribute argument lists are on one line or one argument per line | — | SDD | F4 |
| 1160 | New rule: a comment does not touch code | — | SDD | F3 |
| 1239 | [FORMATTER BUG] A rewritten `# region` line keeps the whitespace between `#` and `region` | bug, formatter | vNext | F1 |
| 1240 | SDD: The shared/diagnostics.md non-goal on inactive #if code also covers region directives, which region rules still report | — | SDD | F8 |
| 1241 | RH8107, RH8202, RH8204, RH8305, RH8307, RH8309 fixes: an element without its own end tag is not rewritten (P19, P1) | bug, analyzer | Triage | F6 |
| 1242 | [CODE REVIEW REPORT] Decide the spec owner of DocumentationCommentCodeFixUtilities (RH8107 and RH8202) | enhancement | SDD | F6 |
| 1243 | [FORMATTER BUG] Joining an operator that stands alone on its line inserts a blank line in front of it | bug, formatter | Triage | F2 |
| 1244 | [CODE REVIEW REPORT] Unreachable nested-element recursion in InitializerLayoutAnalysisUtilities.FindFirstComplexElementMisalignment | enhancement | Triage | F2 |
| 1257 | [ANALYZER BUG] RH3001, RH3005, RH3101, RH3105 code fixes can turn comparisons into a generic invocation | bug, analyzer | Triage | F8 |
| 1259 | [CODE REVIEW REPORT] Share the empty-list removal of the RH3106 and RH3107 code fixes | — | Triage | F8 |
| 1261 | Fixes of RH5405-RH5407 and RH5412-RH5417 edit or duplicate comment and directive text | — | SDD | F4 |
| 1262 | Region fixes are withheld when they break code or separate a declaration from its documentation, and converge: RH7303, RH7304, RH7306, RH7309 (P39) | — | SDD | F5 |
| 1263 | Region-removing fixes clean the blank line they leave: RH7303, RH7310 (P14c) | — | SDD | F5 |
| 1264 | Region rules: the innermost declaration and region own a member: RH7308, RH7309, RH7311 (P17) | — | SDD | F5 |
| 1265 | RH7303 reports regions in every executable place (P11) | — | SDD | F5 |
| 1266 | Using-ordering rules compare with the previous directive of the same group: RH7203, RH7204, RH7206 (P18) | — | SDD | F5 |
| 1267 | A using alias to a non-name forms its own group at the end: RH7204, RH7207, formatter | — | SDD | F5 |
| 1268 | RH2003 resolves the exception type semantically, also through a using alias (P3 extension) | — | SDD | F7 |
| 1269 | Documentation fixes change only the reported markup and finish in one application: RH8201, RH8202, RH8204 | — | SDD | F6 |
| 1270 | RH8106, RH8203, RH8305, RH8306 check every element of their kind (P20) | — | SDD | F6 |
| 1271 | RH5301 and RH5304 check nested initializers and members on the brace line (P11) | — | SDD | F3 |
| 1272 | RH5305 and RH5306 check the whole collection expression and list pattern (P11, P13) | — | SDD | F3 |
| 1273 | RH5302 and RH5308 report an operator alone on its line (P11) | — | SDD | F3 |
| 1274 | RH5308 fix converges: one application clears the diagnostic or the fix is withheld (P7) | — | SDD | F3 |
| 1275 | Empty-type rules RH5412-RH5416 report `};` and treat directives in the body by kind (P11, P22) | — | SDD | F4 |
| 1276 | RH5404 reports every one-line type body and reports an extension block at its keyword (P11, P21) | — | SDD | F4 |
| 1277 | Fix guards: RH5307 resolves the reported list, RH5401 judges the property's own lines | — | SDD | F3 |
| 1278 | RH3002 and RH3003 report the constructs they name (P11) | — | SDD | F8 |
| 1279 | RH3004 fix keeps `static`, keeps the layout, and parenthesizes the lambda | — | SDD | F8 |
| 1280 | RH3201 reports empty comments of every kind that separate paragraphs (P13, P6, P11) | — | SDD | F8 |
| 1281 | Fixes that break the code or change what runs keep the report and are withheld: RH3005, RH3101, RH3103, RH3106, RH3201 (P1, P23) | — | SDD | F8 |
| 1282 | Fixes of RH3101, RH3104, RH3105, RH3106, RH3107 delete comment text (P1) | — | SDD | F8 |
| 1283 | RH3006 fix keeps the document's line ending (P24) | — | SDD | F8 |
| 1284 | Expression-body fixes format only the replaced body: RH3202, RH3203, RH3205-RH3208 (P14d) | — | SDD | F8 |
| 1285 | RH3001 fix is offered for every argument and list position, and withheld for constant patterns | — | SDD | F8 |
| 1286 | RH2001 fix is withheld where it is unsafe (P1) | — | SDD | F7 |
| 1287 | A partial declaration is one declaration: RH2002, RH2004, RH2101, RH7002, RH7401–RH7412, RH8306 (P15) | — | SDD | F7 |
| 1288 | RH2004 fix is withheld on explicit interface operators (P1) | — | SDD | F7 |
| 1289 | RH2109 reads Razor correctly: comments, strings, escapes and email-like text (P5) | — | SDD | F7 |
| 1290 | Argument values are judged through parentheses, `!` and `throw` arms: RH1001, RH1002 (P16, extension) | — | SDD | F7 |
| 1291 | SDD: State what the formatter does beyond RH5304, RH5308 and struct equality in the specs (clarification) | — | SDD | F2 |
| 1292 | Documentation rules report on every documentable declaration: RH8102, RH8103, RH8109, RH8110, RH8202 (P11) | — | SDD | F6 |
| 1293 | RH8107 owns void members, RH8106 skips them (behavior change) | — | SDD | F6 |
| 1294 | RH8203 matches constructors the way the compiler resolves them (bug) | — | SDD | F6 |
| 1295 | RH1001 and RH1002 look through Nullable and tuples and recognize collection expressions (behavior change) | — | SDD | F7 |
| 1296 | RH1002 reports more methods and the query clauses group and join (behavior change) | — | SDD | F7 |
| 1297 | RH1002 judges the comparer's type argument against the key type (bug) | — | SDD | F7 |
| 1298 | RH2006 and RH2007 treat string.Empty and all-empty conditionals as no text (behavior change) | — | SDD | F7 |
| 1299 | RH2001 reports only properties an auto-property can replace and exempts attributed ones (behavior change, bug) | — | SDD | F7 |
| 1300 | RH2002 reports async void explicit interface implementations; the fix is withheld when the interface dictates void (behavior change) | — | SDD | F7 |
| 1329 | [FORMATTER BUG] A declaration after a documentation element without an end tag is turned into a comment | bug, formatter | — | F1 |
| 1330 | Test the lines inside interpolated-raw and UTF-8 string literals in the shared non-formattable-line helper | enhancement | — | F2 |
| 1332 | RH7501 fix is withheld when it would break code, move directives, or rearrange shared lines (P1, P9) | — | SDD | F5 |
| 1333 | RH7403, RH7407, RH7411 check every declarator of a field-like event (P20) | — | SDD | F5 |
| 1334 | RH7401–RH7412: the verdict does not depend on base-list order, partial parts or unrelated overrides (P25) | — | SDD | F5 |
| 1335 | Organization fixes change only the node they rewrite or move: RH7004, RH7101, RH7104–RH7108 (P9, P14a, P14d, P26) | — | SDD | F5 |
| 1336 | Organization fixes are withheld where they break code, relocate a comment, or change nothing: RH7004, RH7101, RH7105, RH7106 (P1, P7) | — | SDD | F5 |
| 1337 | Declaration rules do not report a declarator, modifier or accessor list that holds a conditional directive: RH7101, RH7105–RH7108 (P22) | — | SDD | F5 |
| 1338 | RH7105 and RH7106 check enum, destructor, local-function and accessor modifier lists (P11) | — | SDD | F5 |
| 1339 | RH7102 and RH7109 check each top-level region on its own (P27) | — | SDD | F5 |
| 1340 | RH7003 does not report a namespace that contains a namespace (behavior change) | — | SDD | F5 |
| 1341 | Documentation layout rules treat a `///` that documents nothing as an ordinary comment: RH8301–RH8305, RH8307–RH8309 stop reporting it, RH8401 reports it (P13) | — | SDD | F6 |
| 1342 | Documentation layout fixes format only the reported comment and leave a behind-code `///` in place: RH8305, RH8307, RH8309, RH8402 (P9, P14d, P29) | — | SDD | F6 |
| 1343 | Documentation fixes finish in one application: RH8308 span, RH8309 joined comments, RH8401 U+FEFF lines (P7) | — | SDD | F6 |
| 1344 | RH8304 and RH8309 fixes are withheld when they would break code or move comment text (P1) | — | SDD | F6 |
| 1345 | RH8301–RH8303 judge a documentation comment behind code as if it started its line (P29) | — | SDD | F6 |
| 1346 | RH8401 and RH8402 check in every documentation mode (P30) | — | SDD | F6 |
| 1347 | RH8307 does not report `/** */` comments and inline formatting elements (P31) | — | SDD | F6 |
| 1348 | RH8302 does not report a blank line followed by `#region` or `#endregion`, so it no longer undoes RH5031 (bug) | — | SDD | F6 |
| 1349 | RH8306 checks the summary of indexers (P11, behavior change) | — | SDD | F6 |
| 1350 | RH8501 reports `UnconditionalSuppressMessage` and a `` justification (behavior change) | — | SDD | F6 |
| 1351 | RH8308 reports text after an XML comment behind a closing tag (behavior change) | — | SDD | F6 |
| 1352 | RH8309 reports `/** */` order without a fix and moves a custom element with the element before it (behavior change) | — | SDD | F6 |
| 1353 | RH8402: the fix replaces the whole leading comment run (clarification) and the header must end at a line break or end of file (bug) | — | SDD | F6 |
| 1354 | RH7302 does not report a region name prescribed by another rule; the formatter no longer changes region-name casing (FMT-REGION-001) | — | SDD | F5 |
| 1355 | Formatter matches documentation tag names case-sensitively (FMT-DOC-008) | — | SDD | F1 |
| 1356 | New rule: documentation tag name is wrongly cased | — | SDD | F6 |
| 1381 | Formatter keeps the comment layout when it inserts braces: switch sections and control-flow statements (P32) | — | SDD | F1 |
| 1382 | A token behind the tail of a multi-line comment does not start its line: formatter breaks before it, RH5203/RH5204 report it, SHARED-INIT-001 aligned (P33) | — | SDD | F2 |
| 1383 | Formatter treats every member of a syntax family alike: indexer parameter lists, every assignment operator, a comment before `]`, every attribute-target colon (P34) | — | SDD | F2 |
| 1384 | Formatter does not split a documented field declaration (P36) | — | SDD | F1 |
| 1385 | A required blank line goes after directives, never directly before one: formatter, RH5002 and RH5008 fixes (P37) | — | SDD | F4 |
| 1386 | Comment-placement rules RH5033–RH5049 treat a `///` or `/** */` that documents nothing as an ordinary comment (P13) | — | SDD | F3 |
| 1387 | Layout rules do not report a list or gap that holds a conditional directive: RH5102, RH5109, RH5111, RH5501–RH5531 (P22) | — | SDD | F3 |
| 1388 | Fixes keep the document's line ending: RH5405, RH5011, RH8304, formatter-delegating fixes and `FormatNode` (P24, P35) | — | SDD | F8 |
| 1389 | [FORMATTER BUG] Text the formatter must keep is lost: BOM before a line break, documentation text behind code | — | SDD | F1 |
| 1390 | [FORMATTER BUG] Structural output does not compile or is not stable: async alias `Task`, comments of a converted expression body, `with` initializer | — | SDD | F1 |
| 1391 | [FORMATTER BUG] Indentation and raw-string alignment miss specified cases: base-list continuations, empty wrapped list `)`, omitted type argument, lone `\r` raw strings | — | SDD | F2 |
| 1392 | [FORMATTER BUG] A blank line after a comment before `else`, `catch`, `finally` or do-`while` is kept (FMT-BLANK-008) | — | SDD | F1 |
| 1393 | RH5104 fix inserts a blank line before the moved comment, RH5417 fix leaves the type's `}` on the edited line (P14a, P14b) | — | SDD | F3 |
| 1394 | SDD: shared-spec clarifications from round 9: RH5111 is not a line-start rule, P14b scope, disabled text keeps its line ending (clarification) | — | SDD | F3 |
| 1395 | Formatter: a comment before a later declarator moves onto its own line above the split field (FMT-STRUCT-022, behavior change) | — | SDD | F1 |
| 1396 | RH5030 does not report a closing brace followed by a block statement (behavior change) | — | SDD | F4 |
| 1397 | Formatter line breaks: unjoinable lists stay as written, FMT-BREAK-005 comment moves follow the text, parenthesized nested conditionals, parenthesized `case` patterns | — | SDD | F2 |
| 1398 | Formatter: a break-only switch section never makes the switch braced (FMT-SWITCH-006 over -004, behavior change) | — | SDD | F1 |
| 1399 | Formatter indentation: a region inside an initializer or argument list takes the elements' column, nested array rows are aligned (behavior change) | — | SDD | F2 |
| 1400 | SDD: formatter-spec clarifications from round 9: inactive directives, explicit-return-type lambda anchor, gaps around a multi-line comment (clarification) | — | SDD | F1 |
| 1412 | [ANALYZER BUG] RH7105 and RH7106 do not report the declaring part of a partial constructor | bug, analyzer | SDD | F5 |
| 1416 | Brace fixes RH5405–RH5407 keep the layout of the statement they wrap (P26) | — | SDD | F4 |
| 1417 | RH3103 does not report Nullable where the shorthand `T?` is not valid C#; a using alias only from C# 12 (P38) | — | SDD | F8 |
| 1422 | Rules do not report where the form they ask for is not valid at that position: RH3104, RH5417 (P38) | — | — | F8 |
| 1427 | Squad role handover templates: add a "Follow-up candidates" section to every role that can find one | — | — | F8 |
| 1428 | SDD: Close the gaps in requirement-coverage exemptions | — | — | F8 |

## Summary

### Issues per milestone

| Milestone | Count |
|---|---|
| SDD | 139 |
| Triage | 32 |
| Maintenance | 12 |
| vNext | 10 |
| 1.2.0 | 9 |
| (none) | 5 |
| 1.3.0 | 5 |
| 1.4.0 | 3 |

### Issues per label

| Label | Count |
|---|---|
| (none) | 147 |
| bug | 33 |
| enhancement | 33 |
| analyzer | 32 |
| formatter | 26 |

An issue with several labels counts once per label.

### Proposed families for parallel inventory

Each open issue is in exactly one family, assigned by its title. Cross-cutting issues that name rules from several areas go to the family of the rules they name first, or to the family of the subsystem they mainly change.

**F1 Formatter: structure, documentation, regions, blank lines, switch braces, cleanup** (25): #948, #949, #950, #952, #954, #959, #960, #961, #962, #963, #964, #1082, #1083, #1148, #1239, #1329, #1355, #1381, #1384, #1389, #1390, #1392, #1395, #1398, #1400

**F2 Formatter: line breaks, spacing, indentation, raw strings; member-access chains** (25): #888, #889, #892, #893, #894, #895, #896, #897, #951, #953, #955, #956, #957, #958, #1131, #1149, #1243, #1244, #1291, #1330, #1382, #1383, #1391, #1397, #1399

**F3 Layout RH51xx-RH53xx: statements, line starts, indentation, initializers, comment placement** (25): #1077, #1097, #1098, #1140, #1144, #1145, #1146, #1147, #1150, #1152, #1154, #1155, #1156, #1157, #1158, #1160, #1271, #1272, #1273, #1274, #1277, #1386, #1387, #1393, #1394

**F4 Layout RH50xx/RH54xx-RH56xx and spacing RH6xxx: blank lines, braces, attributes, tabs, spacing** (25): #946, #972, #1033, #1034, #1074, #1075, #1076, #1078, #1079, #1080, #1081, #1093, #1094, #1095, #1096, #1133, #1141, #1142, #1159, #1261, #1275, #1276, #1385, #1396, #1416

**F5 Organization RH7xxx: member order, declarations, usings, regions, namespaces, inheritance** (26): #965, #966, #968, #969, #970, #1105, #1134, #1135, #1151, #1262, #1263, #1264, #1265, #1266, #1267, #1332, #1333, #1334, #1335, #1336, #1337, #1338, #1339, #1340, #1354, #1412

**F6 Documentation RH8xxx** (27): #1053, #1055, #1091, #1092, #1132, #1153, #1241, #1242, #1269, #1270, #1292, #1293, #1294, #1341, #1342, #1343, #1344, #1345, #1346, #1347, #1348, #1349, #1350, #1351, #1352, #1353, #1356

**F7 Naming RH4xxx, Performance RH1xxx, Design RH2xxx, configuration RH0xxx** (31): #862, #967, #971, #973, #974, #1015, #1016, #1032, #1070, #1071, #1072, #1073, #1084, #1085, #1086, #1087, #1088, #1089, #1090, #1268, #1286, #1287, #1288, #1289, #1290, #1295, #1296, #1297, #1298, #1299, #1300

**F8 Clarity RH3xxx, new-rule features, shared fix contract, CLI/Playground/tooling/SDD process** (31): #122, #352, #353, #354, #849, #850, #851, #867, #877, #898, #975, #976, #1054, #1136, #1143, #1240, #1257, #1259, #1278, #1279, #1280, #1281, #1282, #1283, #1284, #1285, #1388, #1417, #1422, #1427, #1428
