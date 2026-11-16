# Phase 6 index (2026-11-16): open issues of vNext, Maintenance, 1.4.0, 1.5.0, sorted by area then number

| Slice | # | Milestone | Labels | Title |
|---|---|---|---|---|
| 1 | 122 | vNext | enhancement,formatter,agent-filed | [FEATURE] Visual Studio extension for Reihitsu formatter |
| 1 | 862 | Maintenance | analyzer,formatter,maintenance,agent-filed | [CODE REVIEW] Consolidate the auto-property accessor-list predicate into one owner |
| 1 | 895 | 1.4.0 | enhancement,formatter,agent-filed | [FORMATTER FEATURE] Join attached parts outside member-access chains |
| 1 | 896 | 1.4.0 | enhancement,formatter,agent-filed | [FORMATTER FEATURE] Align a wrapped first call after a property prefix under the prefix's last dot |
| 1 | 950 | Maintenance | formatter,maintenance,agent-filed | Remove the unreachable switch brace-removal path and correct the SwitchExpressionContributor summary |
| 1 | 953 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] Indent continuation lines that no alignment rule owns one level deeper |
| 1 | 955 | vNext | enhancement,formatter,agent-filed | [FORMATTER BUG] Line breaks: wrapped constructor-initializer colon and wrapped constraint clause |
| 1 | 956 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] One switch-expression arm per line, and argument lists laid out like parameter lists |
| 1 | 957 | vNext | enhancement,formatter,agent-filed | [FORMATTER BUG] Spacing: `fixed(` gets no space, and a double space before a comment is not collapsed |
| 1 | 959 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] Normalize line ends inside comments and disabled text: trailing whitespace and line ending |
| 1 | 960 | vNext | enhancement,formatter,agent-filed | [FORMATTER BUG] Trailing whitespace survives at the end of a directive line |
| 1 | 961 | vNext | enhancement,formatter,agent-filed | [FORMATTER BUG] Documentation elements: end tag on the last content line, and wrapped inline elements |
| 1 | 962 | vNext | enhancement,formatter,agent-filed | [FORMATTER BUG] A documentation comment behind a statement or inside an expression is relocated although it do |
| 1 | 975 | vNext | enhancement,formatter,agent-filed | [FORMATTER BUG] `--dry-run` diff header prefixes a full path with `a/` and `b/` |
| 1 | 1077 | vNext | enhancement,analyzer,formatter,agent-filed | Treat a `/** */` comment that documents nothing as an ordinary block comment |
| 1 | 1081 | Maintenance | analyzer,formatter,maintenance,agent-filed | SDD clarifications from question round 2: RH5025 sibling braces, lone `\r` in FMT-PIPE-007 |
| 1 | 1082 | vNext | enhancement,formatter,agent-filed | Formatter: a file ending in a directive does not end with a line break either |
| 1 | 1083 | vNext | enhancement,analyzer,formatter,agent-filed | Replace tabs in region names; leave other directive text and comments alone |
| 1 | 1095 | vNext | enhancement,analyzer,formatter,agent-filed | A deconstruction declaration counts as a local declaration for RH5029, RH5021, and the formatter |
| 1 | 1098 | vNext | enhancement,analyzer,formatter,agent-filed | Comment rules and formatter: a construct owns only what is inside its braces or arms |
| 1 | 1131 | Maintenance | formatter,maintenance,agent-filed | SDD: List the shared attribute-target requirements under `Uses` in formatter/line-breaks.md |
| 1 | 1148 | vNext | enhancement,formatter,agent-filed | Formatter: the blank line before a line-leading comment only between statements and members (FMT-BLANK-011) |
| 1 | 1291 | Maintenance | analyzer,formatter,maintenance,agent-filed | SDD: State what the formatter does beyond RH5304, RH5308 and struct equality in the specs (clarification) |
| 1 | 1354 | vNext | enhancement,analyzer,formatter,agent-filed | RH7302 does not report a region name prescribed by another rule; the formatter no longer changes region-name c |
| 1 | 1355 | vNext | enhancement,formatter,agent-filed | Formatter matches documentation tag names case-sensitively (FMT-DOC-008) |
| 1 | 1399 | vNext | enhancement,formatter,agent-filed | Formatter indentation: a region inside an initializer or argument list takes the elements' column, nested arra |
| 1 | 1400 | Maintenance | formatter,maintenance,agent-filed | SDD: formatter-spec clarifications from round 9: inactive directives, explicit-return-type lambda anchor, gaps |
| 1 | 1432 | vNext | enhancement,formatter,agent-filed | FMT-RAW: align tab-indented multi-line raw strings (behavior change) |
| 1 | 1439 | vNext | enhancement,formatter,agent-filed | Expression body to block: a comment after `=>` becomes the block's first line (behavior change) |
| 1 | 1440 | vNext | enhancement,formatter,agent-filed | Formatter spacing for `?:`, element access, invocation, `::`, colons, initializer braces and more (behavior ch |
| 1 | 1442 | vNext | enhancement,formatter,agent-filed | Formatter line breaks: unjoinable lists stay as written, parenthesized nested conditionals, parenthesized `cas |
| 1 | 1446 | vNext | enhancement,analyzer,formatter,agent-filed | RH5404 reports every one-line type body; formatter places extension-block braces like type braces (behavior ch |
| 2 | 1461 | Maintenance | formatter,maintenance,agent-filed | Reihitsu.Formatter: the doc-comment phase uses the shared "source character" test |
| 2 | 1472 | Maintenance | formatter,maintenance,agent-filed | IndentationPhase: review the layout computation and decide whether to rewrite it |
| 2 | 1474 | Maintenance | analyzer,formatter,maintenance,agent-filed | FMT-PIPE: decide the blank lines in front of a FormatNodeInDocumentAsync target that starts its line |
| 2 | 1505 | Maintenance | formatter,maintenance,agent-filed | Reihitsu.Formatter.Test: every test helper parses formatter input through one rule |
| 2 | 1515 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] Specified layout for comments at the arrow of an expression-bodied member |
| 2 | 1516 | Maintenance | enhancement,formatter,maintenance,agent-filed | FMT-PIPE: no phase output carries elastic trivia |
| 2 | 1517 | Maintenance | formatter,maintenance,agent-filed | FMT-BREAK: only the cleanup phase removes whitespace before a line break |
| 2 | 1518 | Maintenance | formatter,maintenance,agent-filed | FMT-SWITCH: inserted switch-section braces write no layout of their own |
| 2 | 1519 | Maintenance | formatter,maintenance,agent-filed | FMT-BREAK: property and indexer accessor-list braces have one owner |
| 2 | 1521 | vNext | enhancement,formatter,agent-filed | FMT-PIPE-010: the node-in-document entry point writes back what the phases put in front of a target that does  |
| 2 | 1533 | vNext | enhancement,formatter,agent-filed | FMT-DOC: a delimited documentation comment behind an opening brace, a switch-label colon, or an earlier commen |
| 2 | 1534 | vNext | enhancement,analyzer,formatter,agent-filed | FMT-BREAK-018: a documentation comment behind the `?` or `:` of a conditional leaves the operator starting its |
| 2 | 1537 | vNext | enhancement,formatter,agent-filed | FMT-STRUCT-003, FMT-SWITCH-010: brace an `if`/`else` body or a switch section whose trivia carries a directive |
| 2 | 1538 | Maintenance | documentation,formatter,maintenance,agent-filed | FMT-STRUCT-003: the Why text says bracing rebuilds the body's trivia, which it no longer does |
| 2 | 1539 | Maintenance | formatter,maintenance,agent-filed | FMT-PIPE-013: the expression-body conversion carries a wrap's comment trivia through the shared carrier |
| 2 | 1540 | vNext | enhancement,formatter,agent-filed | FMT-SWITCH-007: a comment behind the last label on the line of the first statement, `case 1: /* c */ A();`, wh |
| 2 | 1542 | vNext | enhancement,formatter,agent-filed | FMT-BREAK-022, FMT-INDENT: the content of a parenthesized or positional pattern is indented inside its parenth |
| 2 | 1549 | vNext | enhancement,formatter,agent-filed | FMT-BREAK-027: a wrap inside an attribute list of a declaration is joined, and a lambda stays on its attribute |
| 2 | 1550 | vNext | enhancement,formatter,agent-filed | FMT-SPACE-024: spacing of `ref` / `ref readonly` in front of a tuple type of a local, field, `foreach` variabl |
| 2 | 1552 | vNext | enhancement,formatter,agent-filed | FMT-BREAK-024: an accessor attribute on a one-line auto-property with a multi-line initializer keeps the acces |
| 2 | 1553 | Maintenance | formatter,maintenance,agent-filed | FMT-SPACE: a same-line gap held as the right token's leading trivia is normalized by the spacing phase |
| 2 | 1554 | vNext | enhancement,formatter,agent-filed | FMT-BREAK: no blank line is inserted inside a parameter list in front of a `///` comment in a parameter's attr |
| 2 | 1555 | Maintenance | analyzer,formatter,maintenance,agent-filed | SHARED-ATTR-017: the code fix's attribute-list split and the formatter's split are one implementation |
| 2 | 1556 | vNext | bug,enhancement,formatter,agent-filed | FMT-SPACE: the space behind a comment in front of a parameter's attribute list is kept |
| 2 | 1557 | Maintenance | documentation,formatter,maintenance,agent-filed | FMT-BREAK-027: a `param:` or `typevar:` attribute list on an owner other than a parameter or type parameter is |
| 2 | 1559 | vNext | enhancement,formatter,agent-filed | FMT-SPACE: one space stands between a switch label's colon and an empty statement (behavior change) |
| 2 | 1562 | Maintenance | bug,analyzer,formatter,maintenance,agent-filed | RH5020 and the formatter: a comment on its own line in front of a section's `break` gets a blank line from one |
| 2 | 1563 | Maintenance | documentation,formatter,maintenance,agent-filed | FMT-BREAK-008: the spec states where a comment in front of a switch section's block body goes |
| 2 | 1564 | Maintenance | documentation,formatter,maintenance,agent-filed | FMT-BREAK-033: the spec states that a `/** */` comment's placement depends on the documentation mode |
| 2 | 1566 | Maintenance | bug,formatter,maintenance,agent-filed | Formatter: an anonymous method in a hole of a regular interpolated string gets line breaks that do not compile |
| 2 | 1567 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] A parenthesized lambda behind a modifier or return type on the next line is joined to it ( |
| 3 | 1568 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] An anonymous method's block aligns under its first token outside interpolation holes too ( |
| 3 | 1571 | Maintenance | formatter,maintenance,agent-filed | FMT-BLANK: the blank-line phase writes no line break that ends a line, apart from a gap behind a multi-line co |
| 3 | 1572 | Maintenance | documentation,formatter,maintenance,agent-filed | SHARED-COMMENT-012: code behind a multi-line comment's closing line keeps its line, stated for every construct |
| 3 | 1574 | Maintenance | analyzer,formatter,maintenance,agent-filed | SHARED-BLANK-006: the blank line after a region directive is decided once |
| 3 | 1576 | Maintenance | formatter,maintenance,agent-filed | FMT-PIPE-027: the self-hosting scans check that no single phase changes the repository's own sources |
| 3 | 1577 | Maintenance | formatter,maintenance,agent-filed | Reihitsu.Formatter.Test: idempotency tests outside FormatterTestsBase also check that no single phase changes  |
| 3 | 1579 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] Trailing whitespace behind the message text of `#warning`, `#error` and other directives i |
| 3 | 1583 | Maintenance | analyzer,formatter,maintenance,agent-filed | Reihitsu.Core: one trailing-whitespace trim for trivia lists, `TriviaListUtilities.RemoveTrailingWhitespace` |
| 3 | 1585 | vNext | enhancement,formatter,cli,agent-filed | FMT-PIPE-006: the formatter's generated-code detection agrees with the analyzers' (honors `generated_code`, ma |
| 3 | 1586 | vNext | enhancement,analyzer,formatter,agent-filed | [FORMATTER FEATURE] A field declaration with several declarators inside a `#region` is split (output question) |
| 3 | 1587 | vNext | enhancement,analyzer,formatter,agent-filed | [FORMATTER FEATURE] A split field does not repeat the line break written behind the type (output question) |
| 3 | 1605 | vNext | enhancement,analyzer,formatter,maintenance,agent-filed | FMT-PIPE: a layout-only mode for the node-level entry point, so code fixes need not restore text the documenta |
| 3 | 1633 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] A base list wrapped behind its colon is joined onto the colon's line (output question) |
| 3 | 1634 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] A line that a comment keeps inside an angle-bracket list's first element or a qualified na |
| 3 | 1636 | vNext | enhancement,analyzer,formatter,agent-filed | FMT-BREAK-023, RH5110: a generic constraint clause of an extension block starts its own line (output question) |
| 3 | 1639 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] A wrap between `where` and the type parameter is joined (output question) |
| 3 | 1640 | Maintenance | formatter,maintenance,agent-filed | ChainLineBreakRewriter: batch gap joins go through LineBreakTriviaUtilities.JoinGaps |
| 3 | 1641 | Maintenance | formatter,maintenance,agent-filed | TokenGapUtilities.HasLineBreakBetween: the summary states what the line-break phase does with a multi-line com |
| 3 | 1645 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] The `=` of an anonymous-object member below its name is joined onto the name's line (outpu |
| 3 | 1646 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] The `=` of a using alias below its name is joined onto the alias line (output question) |
| 3 | 1647 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] A comment in front of a list pattern's closing `]` is indented into the elements' column ( |
| 3 | 1648 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] A lambda arrow written alone on its line is joined (output question) |
| 3 | 1651 | Maintenance | formatter,maintenance,agent-filed | FMT-BREAK: the line-break rewriters share one "line break in front of a moved token, keeping an existing one"  |
| 3 | 1652 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] A compound assignment inside one argument of a multi-argument call: the list returns to on |
| 3 | 1653 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] An interface indexer whose declaration was wrapped before `[` keeps its accessor list on o |
| 3 | 1654 | Maintenance | formatter,maintenance,agent-filed | FMT-BREAK-015, FMT-BREAK-016: the gap behind a parameter default's `=` has one owner |
| 3 | 1658 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] Whitespace between `#` and the keyword of non-region directives is removed (output questio |
| 3 | 1659 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] A `return`- or `throw`-only switch section with a multi-line label does not brace the swit |
| 3 | 1672 | Maintenance | formatter,maintenance,agent-filed | PrecheckPhase: the variable-declaration scan computes each value's first and last token once |
| 3 | 1682 | Maintenance | analyzer,formatter,maintenance,agent-filed | RH8307: the analyzer and the documentation phase share one "element is misaligned" predicate |
| 3 | 1685 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] A wrapped `when` condition of a `case` label is indented one level deeper than `case` (out |
| 4 | 1698 | vNext | enhancement,formatter,agent-filed | [FORMATTER FEATURE] A comment in front of the closing `}` of a multi-line interpolation hole in a regular inte |
| 4 | 1713 | Maintenance | formatter,maintenance,agent-filed | FMT-BLANK-010: the blank line above a region directive behind a documentation comment is decided by one blank- |
| 4 | 1722 | Maintenance | formatter,maintenance,agent-filed | Reihitsu.Formatter: RawStringLineShifts uses the shared interpolation-brace check from Reihitsu.Core |
| 4 | 1723 | Maintenance | formatter,maintenance,agent-filed | Reihitsu.Playground: the syntax-error check asks one public formatter query instead of keeping a private copy |
| 4 | 1726 | vNext | enhancement,analyzer,formatter,agent-filed | [FORMATTER FEATURE] A comment or directive between a binary operator and its right operand is an unsupported s |
| 4 | 1728 | vNext | enhancement,analyzer,formatter,agent-filed | [FORMATTER FEATURE] A preprocessor directive or disabled text inside a field declaration with several declarat |
| 4 | 1732 | vNext | enhancement,analyzer,formatter,agent-filed | [FORMATTER FEATURE] A comment or directive between the `=>` of an expression-bodied member and its expression  |
| 4 | 1776 | Maintenance | formatter,maintenance,agent-filed | Reihitsu.Formatter: PrecedingTokenFacts and TriviaFact are replaced by reading the token directly |
| 4 | 352 | 1.5.0 | enhancement,analyzer,agent-filed | [ANALYZER FEATURE] Report methods whose cognitive complexity exceeds a limit |
| 4 | 353 | 1.5.0 | enhancement,analyzer,agent-filed | [ANALYZER FEATURE] Report methods whose line count exceeds a limit |
| 4 | 354 | 1.5.0 | enhancement,analyzer,agent-filed | [ANALYZER FEATURE] Report types whose line count exceeds a limit |
| 4 | 849 | 1.4.0 | enhancement,analyzer,agent-filed | [ANALYZER FEATURE] Get-only properties should use an expression body |
| 4 | 850 | 1.4.0 | enhancement,analyzer,agent-filed | [ANALYZER FEATURE] Get-only indexers should use an expression body |
| 4 | 851 | 1.4.0 | enhancement,analyzer,agent-filed | [ANALYZER FEATURE] Expression style get-only indexers should be single lined |
| 4 | 867 | Maintenance | analyzer,maintenance,agent-filed | [CODE REVIEW REPORT] Code-fix verifier compiles fixed states at warning level 4 and hides warning-wave diagnos |
| 4 | 877 | Maintenance | analyzer,maintenance,agent-filed | [CODE REVIEW REPORT] RH3002 follow-ups: exact re-parse check, cross-pair decision cost, and test hygiene (from |
| 4 | 889 | Maintenance | analyzer,maintenance,agent-filed | [CODE REVIEW] Cover the mid-line-statement side of the chain code fixes' context selection with a test |
| 4 | 892 | 1.4.0 | enhancement,analyzer,agent-filed | [ANALYZER FEATURE] Report a line break in front of an attached part of a member-access chain |
| 4 | 893 | 1.4.0 | enhancement,analyzer,agent-filed | [ANALYZER FEATURE] Report the remaining chain layouts the formatter changes without a diagnostic |
| 4 | 894 | 1.4.0 | enhancement,analyzer,agent-filed | [ANALYZER FEATURE] Report comments, directives and disabled text inside a ?. or !. operator |
| 4 | 897 | 1.4.0 | enhancement,analyzer,agent-filed | [ANALYZER FEATURE] Update RH5112 and RH5048 titles and messages to say "first member access" instead of "first |
| 4 | 968 | vNext | enhancement,analyzer,agent-filed | [ANALYZER BUG] RH7103 compares written accessibility modifiers instead of effective accessibility |
| 4 | 970 | vNext | enhancement,analyzer,agent-filed | [ANALYZER BUG] Ordering fixes refuse a move across a member whose body holds a balanced `#pragma` pair |
| 4 | 971 | vNext | enhancement,analyzer,agent-filed | [ANALYZER BUG] Casing fixes turn a leading acronym into `_hTTPClient` |
| 4 | 973 | vNext | enhancement,analyzer,agent-filed | [ANALYZER BUG] RH1003 counts numeric sub-additions as separate string parts |
| 4 | 1032 | Maintenance | analyzer,maintenance,agent-filed | [CODE REVIEW REPORT] Remove the unused code-fix resources RH4116Title and RH4118Title |
| 4 | 1033 | vNext | enhancement,analyzer,agent-filed | [ANALYZER FEATURE] Attribute placement fixes: limit the missing-brace guard to braces that enclose the attribu |
| 4 | 1053 | Maintenance | analyzer,maintenance,agent-filed | [CODE REVIEW REPORT] Rule pages RH8001–RH8018 do not say which declarations count as private |
| 4 | 1072 | vNext | enhancement,analyzer,agent-filed | Casing fixes: withhold the fix when the derived name is a reserved keyword |
| 4 | 1078 | vNext | enhancement,analyzer,agent-filed | Spacing rules: do not report whitespace that touches a comment |
| 4 | 1080 | vNext | enhancement,analyzer,agent-filed | RH5022: report a blank line after an opening brace that carries a trailing comment |
| 5 | 1085 | vNext | enhancement,analyzer,agent-filed | RH4001 keeps a migration timestamp prefix; RH4009 compares nested namespaces by their full name |
| 5 | 1086 | vNext | enhancement,analyzer,agent-filed | RH4001: do not report a file with top-level statements |
| 5 | 1087 | vNext | enhancement,analyzer,agent-filed | RH4115/RH4117: exempt `_`, stop checking local constants, give every deconstruction variable to RH4117 |
| 5 | 1088 | vNext | enhancement,analyzer,agent-filed | New rule: casing of local constants |
| 5 | 1089 | vNext | enhancement,analyzer,agent-filed | Check query range variables (RH4124) and extension-block receivers (RH4122) |
| 5 | 1090 | Maintenance | analyzer,maintenance,agent-filed | Rename RH4118TupleElementCasingAnalyzer to match what it checks |
| 5 | 1091 | vNext | enhancement,analyzer,agent-filed | Documentation rules: a declaration is documented exactly when the compiler's documentation for it has a summar |
| 5 | 1092 | vNext | enhancement,analyzer,agent-filed | Documentation rules: choose the private/non-private group from the symbol, and treat a static constructor as p |
| 5 | 1097 | vNext | enhancement,analyzer,agent-filed | Comment rules: chain anchor model, and `is`/`as` always reported |
| 5 | 1132 | Maintenance | analyzer,maintenance,agent-filed | [CODE REVIEW REPORT] Check that the parameter documentation rules match `name` attributes the same way |
| 5 | 1140 | vNext | enhancement,analyzer,agent-filed | Line-break and alignment fixes change only the code their diagnostic is about (P9) |
| 5 | 1154 | vNext | enhancement,analyzer,agent-filed | RH5110: check record struct declarations |
| 5 | 1155 | vNext | enhancement,analyzer,agent-filed | RH5111: check nested assignments |
| 5 | 1156 | vNext | enhancement,analyzer,agent-filed | RH5103: report a statement that starts on its predecessor's last line, and check top-level statements |
| 5 | 1157 | vNext | enhancement,analyzer,agent-filed | RH5204: report misindented lines that do not start with a keyword |
| 5 | 1158 | vNext | enhancement,analyzer,agent-filed | RH5108: check primary constructor, lambda, anonymous method and extension parameter lists |
| 5 | 1159 | vNext | enhancement,analyzer,agent-filed | New rule: attribute argument lists are on one line or one argument per line |
| 5 | 1160 | vNext | enhancement,analyzer,agent-filed | New rule: a comment does not touch code |
| 5 | 1240 | Maintenance | analyzer,maintenance,agent-filed | SDD: The shared/diagnostics.md non-goal on inactive #if code also covers region directives, which region rules |
| 5 | 1244 | Maintenance | analyzer,maintenance,agent-filed | [CODE REVIEW REPORT] Unreachable nested-element recursion in InitializerLayoutAnalysisUtilities.FindFirstCompl |
| 5 | 1259 | Maintenance | analyzer,maintenance,agent-filed | [CODE REVIEW REPORT] Share the empty-list removal of the RH3106 and RH3107 code fixes |
| 5 | 1265 | vNext | enhancement,analyzer,agent-filed | RH7303 reports regions in every executable place (P11) |
| 5 | 1271 | vNext | enhancement,analyzer,agent-filed | RH5301 and RH5304 check nested initializers and members on the brace line (P11) |
| 5 | 1272 | vNext | enhancement,analyzer,agent-filed | RH5305 and RH5306 check the whole collection expression and list pattern (P11, P13) |
| 5 | 1273 | vNext | enhancement,analyzer,agent-filed | RH5302 and RH5308 report an operator alone on its line (P11) |
| 5 | 1275 | vNext | enhancement,analyzer,agent-filed | Empty-type rules RH5412-RH5416 report `};` and treat directives in the body by kind (P11, P22) |
| 5 | 1277 | vNext | enhancement,analyzer,agent-filed | Fix guards: RH5307 resolves the reported list, RH5401 judges the property's own lines |
| 5 | 1278 | vNext | enhancement,analyzer,agent-filed | RH3002 and RH3003 report the constructs they name (P11) |
| 5 | 1280 | vNext | enhancement,analyzer,agent-filed | RH3201 reports empty comments of every kind that separate paragraphs (P13, P6, P11) |
| 5 | 1290 | vNext | enhancement,analyzer,agent-filed | Argument values are judged through parentheses, `!` and `throw` arms: RH1001, RH1002 (P16, extension) |
| 5 | 1292 | vNext | enhancement,analyzer,agent-filed | Documentation rules report on every documentable declaration: RH8102, RH8103, RH8109, RH8110, RH8202 (P11) |
| 6 | 1293 | vNext | enhancement,analyzer,agent-filed | RH8107 owns void members, RH8106 skips them (behavior change) |
| 6 | 1294 | vNext | enhancement,analyzer,agent-filed | RH8203 matches constructors the way the compiler resolves them (bug) |
| 6 | 1295 | vNext | enhancement,analyzer,agent-filed | RH1001 and RH1002 look through Nullable and tuples and recognize collection expressions (behavior change) |
| 6 | 1296 | vNext | enhancement,analyzer,agent-filed | RH1002 reports more methods and the query clauses group and join (behavior change) |
| 6 | 1297 | vNext | enhancement,analyzer,agent-filed | RH1002 judges the comparer's type argument against the key type (bug) |
| 6 | 1298 | vNext | enhancement,analyzer,agent-filed | RH2006 and RH2007 treat string.Empty and all-empty conditionals as no text (behavior change) |
| 6 | 1299 | vNext | enhancement,analyzer,agent-filed | RH2001 reports only properties an auto-property can replace and exempts attributed ones (behavior change, bug) |
| 6 | 1300 | vNext | enhancement,analyzer,agent-filed | RH2002 reports async void explicit interface implementations; the fix is withheld when the interface dictates  |
| 6 | 1330 | Maintenance | analyzer,maintenance,agent-filed | Test the lines inside interpolated-raw and UTF-8 string literals in the shared non-formattable-line helper |
| 6 | 1338 | vNext | enhancement,analyzer,agent-filed | RH7105 and RH7106 check enum, destructor, local-function and accessor modifier lists (P11) |
| 6 | 1341 | vNext | enhancement,analyzer,agent-filed | Documentation layout rules treat a `///` that documents nothing as an ordinary comment: RH8301–RH8305, RH8307– |
| 6 | 1349 | vNext | enhancement,analyzer,agent-filed | RH8306 checks the summary of indexers (P11, behavior change) |
| 6 | 1350 | vNext | enhancement,analyzer,agent-filed | RH8501 reports `UnconditionalSuppressMessage` and a `<Pending>` justification (behavior change) |
| 6 | 1351 | vNext | enhancement,analyzer,agent-filed | RH8308 reports text after an XML comment behind a closing tag (behavior change) |
| 6 | 1352 | vNext | enhancement,analyzer,agent-filed | RH8309 reports `/** */` order without a fix and moves a custom element with the element before it (behavior ch |
| 6 | 1353 | Maintenance | analyzer,maintenance,agent-filed | RH8402: the fix replaces the whole leading comment run (clarification) and the header must end at a line break |
| 6 | 1356 | vNext | enhancement,analyzer,agent-filed | New rule: documentation tag name is wrongly cased |
| 6 | 1394 | Maintenance | analyzer,maintenance,agent-filed | SDD: shared-spec clarifications from round 9: RH5111 is not a line-start rule, P14b scope, disabled text keeps |
| 6 | 1433 | vNext | enhancement,analyzer,agent-filed | RH5104: `///` tails and `/* */` lines count as ordinary comments, comments after interpolated raw strings are  |
| 6 | 1434 | vNext | enhancement,analyzer,agent-filed | Naming rules: do not report names dictated by metadata or a native entry point (behavior change) |
| 6 | 1435 | vNext | enhancement,analyzer,agent-filed | RH2004 fix adds `public` to operators (behavior change) |
| 6 | 1436 | vNext | enhancement,analyzer,agent-filed | RH3004 fix keeps `static` and parenthesizes the lambda in operand positions (behavior change) |
| 6 | 1437 | vNext | enhancement,analyzer,agent-filed | RH7501 fix drops the blank line before the moved `break` (behavior change) |
| 6 | 1443 | vNext | enhancement,analyzer,agent-filed | RH5417 fix moves the type's `}` off the edited line (behavior change) |
| 6 | 1444 | vNext | enhancement,analyzer,agent-filed | RH5601, RH6004, RH6005, RH6021, RH7302: tabs on later `///` lines, `# region`, repeated spaces (behavior chang |
| 6 | 1445 | vNext | enhancement,analyzer,agent-filed | RH55xx attribute rules: placement follows the attached declaration, not the target specifier (behavior change) |
| 6 | 1447 | vNext | enhancement,analyzer,agent-filed | RH5002/RH5008 fixes and SHARED-STMT-004: the blank line goes after directives, never directly before one (beha |
| 6 | 1448 | vNext | enhancement,analyzer,agent-filed | RH8017/RH8018: a static constructor always belongs to the private group (behavior change) |
| 6 | 1449 | vNext | enhancement,analyzer,agent-filed | Documentation rules: parameters as the compiler binds them, summary text that renders, RH8030 on the produced  |
| 6 | 1450 | vNext | enhancement,analyzer,agent-filed | RH8402 reports a header not followed by a line break or end of file (behavior change) |
| 6 | 1451 | vNext | enhancement,analyzer,agent-filed | RH4005/RH4121 and casing fixes keep an existing prefix and every identifier character (behavior change) |
| 7 | 1452 | vNext | enhancement,analyzer,agent-filed | RH2109 reports `@functions` blocks like `@code` (behavior change) |
| 7 | 1453 | Maintenance | analyzer,maintenance,agent-filed | RH6001 spec: list `fixed` among the reported keywords (clarification) |
| 7 | 1460 | Maintenance | analyzer,maintenance,agent-filed | Reihitsu.Analyzer.CodeFixes: RH8309 fix comment names the canonical element order correctly |
| 7 | 1476 | Maintenance | analyzer,maintenance,agent-filed | RH5201: cover a method chain whose first token stands behind a comment that leads its line |
| 7 | 1477 | Maintenance | analyzer,maintenance,agent-filed | RH5304: cover the fix on statement and expression-body targets |
| 7 | 1512 | Maintenance | analyzer,maintenance,agent-filed | Analyzers: the remaining line-start and line-terminator checks use the shared facts |
| 7 | 1546 | vNext | enhancement,analyzer,agent-filed | New rule: a comment must not share a line with code |
| 7 | 1560 | vNext | enhancement,analyzer,agent-filed | New rule: a switch label's statement and a second label do not share the label's line |
| 7 | 1582 | Maintenance | analyzer,maintenance,agent-filed | RH5114: the code fix tests documentation comments with the shared `SyntaxTriviaUtilities.IsDocumentationCommen |
| 7 | 1584 | vNext | documentation,enhancement,analyzer,agent-filed | Analyzers: do not report code generated by external tools |
| 7 | 1589 | Maintenance | analyzer,maintenance,agent-filed | Code fixes: every fix is audited against the SHARED-FIX-003 placement condition and the directive-pair split |
| 7 | 1590 | vNext | enhancement,analyzer,agent-filed | RH7003: the report for a namespace whose nested namespace sits only in an excluded `#if` branch is decided (ou |
| 7 | 1591 | Maintenance | documentation,analyzer,maintenance,agent-filed | RH7003 rule page: the "Exceptions" section is removed to follow the rule-page conventions |
| 7 | 1601 | vNext | enhancement,analyzer,agent-filed | RH3004: the fix is offered for anonymous methods that hold directives, disabled text, multi-line strings or co |
| 7 | 1602 | vNext | enhancement,analyzer,agent-filed | RH3106: array elements are decided by the target their value reaches |
| 7 | 1603 | Maintenance | analyzer,maintenance,agent-filed | Reihitsu.Core: RH3002 uses the shared "rewrite still parses the same" check |
| 7 | 1606 | Maintenance | documentation,analyzer,maintenance,agent-filed | SDD: RH3001, RH3005, RH3101, RH3105 and RH3201 specs state the safe inputs their fixes rewrite today (clarific |
| 7 | 1607 | Maintenance | analyzer,maintenance,agent-filed | RH3004: spec and test the layout for a multi-line parameter-list comment, a comment continuation on the first  |
| 7 | 1612 | vNext | enhancement,analyzer,agent-filed | RH5202: a line of hole code that holds only whitespace is written empty by the fix, or shifted (output questio |
| 7 | 1615 | Maintenance | documentation,analyzer,maintenance,agent-filed | SHARED-DIAG-004: the wording names SHARED-NESTED-002's per-part reports and leaves RH8306's summaries out of t |
| 7 | 1628 | vNext | enhancement,analyzer,agent-filed | RH5023: blank lines inside an interpolation hole's expression are reported or not (output question) |
| 7 | 1637 | vNext | enhancement,analyzer,agent-filed | [ANALYZER FEATURE] New rule: the constraints of a wrapped generic constraint clause are one per line |
| 7 | 1638 | vNext | enhancement,analyzer,agent-filed | [ANALYZER FEATURE] New rule: the colon of a generic constraint clause has one space on each side |
| 7 | 1649 | vNext | documentation,enhancement,analyzer,agent-filed | RH5111: compound assignments below their target are reported (P11 question) |
| 7 | 1650 | vNext | documentation,enhancement,analyzer,agent-filed | RH5106, RH5107: indexer parameter lists are checked like method parameter lists (P11 question) |
| 7 | 1668 | vNext | enhancement,analyzer,maintenance,agent-filed | Member-moving fixes: decide whether to refuse any conditional block in the moved span and drop the excluded-te |
| 7 | 1681 | vNext | enhancement,analyzer,agent-filed | RH8307: an element inside a /** */ comment is reported with no fix (output question) |
| 7 | 1684 | Maintenance | analyzer,maintenance,agent-filed | Statement blank-line fixes and RH5103: the moved statement's column comes from the formatter, not from a predi |
| 7 | 1689 | vNext | enhancement,analyzer,agent-filed | RH7004: the fix is offered for moved statements that hold a multi-line string literal or a multi-line /* */ co |
| 7 | 1694 | Maintenance | documentation,analyzer,maintenance,agent-filed | SHARED-DIAG-002: a name is compared by its value, including Unicode escape sequences, for every rule that comp |
| 7 | 1709 | vNext | enhancement,analyzer,agent-filed | RH8107, RH8202: every element of the reported kind gets a diagnostic (output question) |
| 8 | 1710 | vNext | enhancement,analyzer,agent-filed | RH8306: both parts of a partial property whose summaries read as a sentence are reported (output question) |
| 8 | 1711 | vNext | enhancement,analyzer,agent-filed | RH8032, RH8033, RH8101, RH8105, RH8108: a partial member's documentation is read part by part (unconfirmed) |
| 8 | 1712 | vNext | enhancement,analyzer,agent-filed | Documentation rules: nested elements of a documentation comment get a layout check of their own |
| 8 | 1739 | vNext | enhancement,analyzer,agent-filed | [ANALYZER FEATURE] New rule: the commas of a multi-line argument list end their line (counterpart of RH5107) |
| 8 | 1741 | Maintenance | analyzer,maintenance,agent-filed | SHARED-DIAG-008: the fix-withholding requirements of the analyzer specs refer to it, and RH5101-004 judges the |
| 8 | 1758 | Maintenance | analyzer,maintenance,agent-filed | RH5107: the code fix lays out through the formatter instead of a hand-written text edit |
| 8 | 1759 | vNext | enhancement,analyzer,agent-filed | SHARED-DIAG-008: analyzers judge the code as the preprocessor symbols of the current build are set |
| 8 | 1771 | Maintenance | analyzer,maintenance,agent-filed | Code fixes that rename a symbol: "may this rename be applied" has one owner (casing fixes and RH2001) |
| 8 | 1773 | vNext | enhancement,analyzer,agent-filed | SHARED-FIX-007: a fix that changes only gaps inside a member leaves a sibling on the member's line (output que |
| 8 | 1774 | vNext | enhancement,analyzer,agent-filed | Layout code fixes: parts inside a reported construct keep their own code (RH5101, RH5102, RH5303–RH5308, RH540 |
| 8 | 1775 | Maintenance | bug,analyzer,maintenance,agent-filed | Member fixes: the line break in front of a member that shares its line has one owner (RH5511, RH5513, RH5517;  |
| 8 | 1777 | vNext | enhancement,analyzer,agent-filed | RH5113: the fix is offered in a file with a syntax error and next to a comment the formatter refuses (output q |
| 8 | 898 | Maintenance | maintenance,agent-filed | [CODE REVIEW] scripts/trace.sh is committed without the executable bit |
| 8 | 976 | vNext | enhancement,agent-filed | Playground: explain why generated input is left unchanged |
| 8 | 1054 | Maintenance | maintenance,agent-filed | [CODE REVIEW REPORT] lane-check classifies squad/decisions/ entries as tooling, so every escalated run widens  |
| 8 | 1427 | Maintenance | maintenance,agent-filed | Squad role handover templates: add a "Follow-up candidates" section to every role that can find one |
| 8 | 1428 | Maintenance | maintenance,agent-filed | SDD: Close the gaps in requirement-coverage exemptions |
| 8 | 1470 | Maintenance | documentation,maintenance,agent-filed | scripts: state the default test run's coverage in one place |
| 8 | 1471 | Maintenance | maintenance,agent-filed | squad: lane classification has one statement that the lane check executes |
| 8 | 1475 | Maintenance | maintenance,agent-filed | Reihitsu.Core: remove DocumentationCommentUtilities.ShiftContinuationExteriorMarkers, which has no caller |
| 8 | 1479 | Maintenance | maintenance,agent-filed | squad: update the "Tab width across formatter phases" matrix in failure-classes.md |
| 8 | 1507 | Maintenance | maintenance,agent-filed | Reihitsu.ArchitectureTests: every guard name in the phase-ownership table resolves to a declaration |
| 8 | 1544 | Maintenance | documentation,maintenance,agent-filed | AGENTS.md: the formatter phase-ownership table names the blank-line policy and the line-break phase's keep rul |
| 8 | 1545 | Maintenance | maintenance,agent-filed | scripts: `verify-text-only.sh --strict-docs` counts only documentation of members that are reachable from outs |
| 8 | 1661 | Maintenance | documentation,maintenance,agent-filed | SHARED-COMMENT-001: a `/***` comment is decided as the compiler parses it, not by its text |
| 8 | 1690 | Maintenance | documentation,maintenance,agent-filed | SHARED-FIX-003, FMT-INDENT-007: the later lines of a /* */ comment are either layout or text, stated once (cla |
| 8 | 1693 | Maintenance | documentation,maintenance,agent-filed | SDD: the rulebook states that `shared/diagnostics.md` and `shared/code-fixes.md` apply without `Uses`, but Spe |
| 8 | 1724 | Maintenance | maintenance,agent-filed | scripts: trace.sh reports a file that whole-document formatting skips for code hidden in another build configu |
| 8 | 1725 | Maintenance | documentation,maintenance,agent-filed | squad, AGENTS.md: the escalation rule for new types and structure is not limited to base classes |
| 8 | 1740 | Maintenance | maintenance,agent-filed | Reihitsu.Core: remove UsingDirectiveOrderingUtilities.OrderUsings, which has no production caller |
| 8 | 1772 | Maintenance | documentation,maintenance,agent-filed | sdd/changes: comma-behind-comment-unsupported.md keeps the measured facts instead of a pointer to a deleted wo |
