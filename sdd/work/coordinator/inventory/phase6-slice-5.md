# Phase 6 slice 5 (issues #1085–#1292)

| # | Milestone | Area | Theme candidate | Verdict | Evidence | Size | User-visible | Depends on | Output question | Note |
|---|---|---|---|---|---|---|---|---|---|---|
| 1085 | vNext | analyzer/RH4xxx naming | naming rule scope | open | — | S | yes | — | no | Decided bug pair; RH4001-005 still covers only unprefixed names, and RH4009 compares a nested namespace by its short name (analyzer registers every `NamespaceDeclaration`). Can be split into two PRs. |
| 1086 | vNext | analyzer/RH4xxx naming | naming rule scope | open | — | S | yes | — | no | RH4001-001 still says only "covers a file without top-level statements"; no `GlobalStatement` check in the analyzer. |
| 1087 | vNext | analyzer/RH4xxx naming | naming rule scope | open | — | M | yes | — | no | Code still reports `_` and `const` locals; RH4115-001 already reads "not `const`" and RH4110 non-goals say "Local constants — RH4115", so the specs contradict each other today. Release together with #1088. |
| 1088 | vNext | analyzer/RH4xxx naming | new rule | open | — | M | yes | 1087 | no | New rule ID still to pick; it ships with #1087. |
| 1089 | vNext | analyzer/RH4xxx naming | naming rule scope | open | — | S | yes | — | no | RH4124 has no query range variables, and RH4122 has no extension-block receiver. |
| 1090 | Maintenance | analyzer/RH4xxx naming | internal refactor | open | — | S | no | — | no | `RH4118TupleElementCasingAnalyzer` and its test class still carry the old name. |
| 1091 | vNext | analyzer/RH8xxx documentation | documentation-comment rules (P8) | open | — | L | yes | — | no | SHARED-DOC-005..007 still exclude malformed, cased, and partial input from coverage. The 2026-11 change doc `documentation-rules-reported-elements.md` now judges RH8201/RH8203 partial members part by part, which contradicts the "merged parts" instance in the 2026-10-19 comment, so that instance needs re-deciding. Decide together with #1092. |
| 1092 | vNext | analyzer/RH8xxx documentation | documentation-comment rules (P8) | open | — | S | yes | 1091 | no | Only Q3b remains (Q4 moved to #1448). The issue itself says to decide it together with #1091 Q3a. |
| 1097 | vNext | analyzer/RH50xx comment placement | comment-placement rules | open | — | M | yes | — | no | The pinned tests that this issue flips are still in `RH5038...AnalyzerTests.cs`. |
| 1132 | Maintenance | analyzer/RH8xxx documentation | internal refactor | open | — | S | no | — | no | Only the shared-helper refactor remains, because the questions were answered by #1162 and moved to #1153; RH8032/8033/8101/8108 still keep four copies of the `param`-name matching. |
| 1140 | vNext | code fixes (shared) | partial write-back | partly done | #1760 (4666dbbb) fixed RH5204, RH5206, RH5303–RH5308, RH5111, RH5113, the RH5112/RH5201 chain fallback, and target leading trivia | L | yes | design decision (#1774) | yes | Remaining: RH5110, the RH5403/RH5404 headers, and the unmeasured instances. They need the partial write-back design (`fix-layout-scope.md` "left out for scope"). Output questions 1 and 2 in the body are partly overtaken by #1760. |
| 1154 | vNext | analyzer/RH51xx line breaks | rule report scope (P11) | open | — | S | yes | — | no | RH5110 registers no `RecordStructDeclaration`. |
| 1155 | vNext | analyzer/RH51xx line breaks | rule report scope (P11) | open | — | S | yes | — | no | RH5111-001 still names only statement-level and initializer-member assignments. |
| 1156 | vNext | analyzer/RH51xx line breaks | rule report scope (P11) | open | — | M | yes | — | no | RH5103-001 still compares start lines within one list. The 2026-11-02 comment adds the shape where a statement shares a line with the previous switch section. |
| 1157 | vNext | analyzer/RH52xx indentation | rule report scope (P11) | open | — | M | yes | — | no | Its blocker #951 is closed. RH5204-001 still lists only keyword and modifier line starts. |
| 1158 | vNext | analyzer/RH51xx line breaks | rule report scope (P11) | open | — | S | yes | — | no | RH5108-001 lacks primary constructors, lambdas, anonymous methods, and extension blocks. The #1736 change widens the gap (comment of 2026-11-15). |
| 1159 | vNext | analyzer/RH51xx line breaks | new rule | open | — | M | yes | — | yes | The change doc picks one rule or two. It overlaps #1739, whose comma rule also names attribute argument lists. |
| 1160 | vNext | analyzer/RH6xxx spacing + formatter/HorizontalSpacingPhase | new rule | open | — | M | yes | 1078 | yes | The formatter change, which inserts a space next to a comment, needs output confirmation. Its premise relates to #1546: the maintainer position quoted there is that comments sharing a line with code are reported, not laid out. |
| 1240 | Maintenance | specs only | SHARED-DIAG-008 reversal | open | — | S | no | 1759 | no | The non-goal text in `shared/diagnostics.md:14` is unchanged. The P22 wording quoted in the comment ("a construct with `#if` is not reported") is reversed by #1759, so write it together with #1759. |
| 1244 | Maintenance | analyzer/RH53xx initializers | rule report scope (P11) | superseded by #1271 | Comment of 2026-10-20: the recursion becomes reachable with #1271, and this report closes with it | S | no | 1271 | no | Not a separate work item. |
| 1259 | Maintenance | code fixes (shared) | internal refactor | done | `EmptyParenthesesCodeFixProviderBase<TNode>` (added in #1599, 863cb793) is the base of both the RH3106 and RH3107 providers | S | no | — | no | Small leftover: the Code owners row of `shared/empty-parentheses.md` does not yet name the fix base. |
| 1265 | vNext | analyzer/RH73xx regions | region handling | open | — | M | yes | — | no | SHARED-TYPEREGION-005 still lacks expression bodies, initializers, defaults, and attribute arguments. Its blockers #1260 and #1262 are closed. The comment of 2026-11-06 adds two RH7309 fixes that break code (CS8510, swapped elements) as instances. |
| 1271 | vNext | analyzer/RH53xx initializers | rule report scope (P11) | open | — | M | yes | — | no | Its blocker #1161 is closed. Closes #1244 along with it. |
| 1272 | vNext | analyzer/RH53xx initializers | rule report scope (P11) | open | — | M | yes | — | no | The RH5306 comment guard is still in RH5306-001. Its blockers #1161 and #1139 are closed. |
| 1273 | vNext | analyzer/RH53xx operators | rule report scope (P11) | open | — | S | yes | — | no | Its blockers #1243 and #1161 are closed. |
| 1275 | vNext | analyzer/RH54xx empty types | rule report scope (P11) | open | — | S | yes | 1759 | no | The `};` part remains open. The P22 bullet ("a body with `#if` members is not reported") is overridden by #1759, under which it is reported without a fix. |
| 1277 | vNext | code fixes (RH5307, RH5401) | fix guard scope | open | — | S | yes | — | no | RH5307-004 still resolves the list from the receiver, and RH5401 still offers no fix with attribute lists. |
| 1278 | vNext | analyzer/RH30xx clarity | rule report scope (P11) | open | — | M | yes | — | no | RH3002-001 still lists no attribute arguments, `if` or `while` conditions, lambda bodies, or switch arms. |
| 1280 | vNext | analyzer/RH32xx comments | comment kinds alike (P6/P13) | open | — | S | yes | — | no | RH3201-001 is unchanged. |
| 1290 | vNext | analyzer/RH1xxx performance | argument value judgement (P16) | open | — | M | yes | — | no | The comment adds #1247 Q6 (`default(T)`). Its blocker #1260 is closed. |
| 1292 | vNext | analyzer/RH8xxx documentation | documentation rule report scope (P11) | open | — | M | yes | — | no | RH8202 still registers only properties and indexers. |

## Closure candidates

- #1244: superseded by #1271. The issue's own comment says the report closes with #1271, which makes the recursion reachable. Close it as superseded, or link it with `Closes` from the #1271 PR.
- #1259: done. #1599 introduced `EmptyParenthesesCodeFixProviderBase<TNode>`, and both providers derive from it. Optionally add the base to the Code owners row of `shared/empty-parentheses.md` first.
- #1140: partly done (#1760). Do not close it. Narrow it to RH5110 and the RH5403/RH5404 headers, or fold it into #1774, because both wait on the same write-back design.

## Overlaps

- #1140 + #1774 (+ #1605, #1521, #1684): one partial write-back design decision.
- #1087 + #1088: these release together, so that local constants are checked by exactly one rule. #1451 and #1434 touch the same casing rules and fixes.
- #1091 + #1092 (+ #1448, #1711, #1341, #1355, #1356, #1449): P8 documentation cluster. The partial-member instances of #1091 conflict with `documentation-rules-reported-elements.md` (part by part).
- #1132 + #1449 (+ #1153): parameter-name matching in documentation rules.
- #1240 + #1275 + #1759: SHARED-DIAG-008 reversal; the P22 wording in #1240 and #1275 must follow #1759.
- #1244 + #1271: one PR.
- #1271 + #1272 + #1273: P11 widening of the RH53xx initializer and operator layout rules (same specs family, shared FMT-BREAK-021/022).
- #1154 + #1636: RH5110 scope (record struct, extension-block constraints).
- #1155 + #1649: RH5111 scope (nested and compound assignments).
- #1158 + #1650: parameter-list rules widened by P11.
- #1156 + #1560: statements and labels sharing a switch-section line.
- #1159 + #1739: attribute argument list layout rules versus the comma rule. The change doc could decide both.
- #1160 + #1078 + #1546: comment-adjacent spacing versus comment placement on code lines.
- #1097 + #1098 (slice 1): comment-rule ownership and the anchor model.
- #1280 + #1077 + #1341: a `///` or `/** */` that documents nothing is an ordinary comment (P6/P13).
- #1290 + #1295 + #1296 + #1297: RH1001/RH1002 argument judgement.
- #1292 + #1709: documentation rules report every element and declaration.
- #1265 + #1240: region directives in executable places and in inactive branches (RH73xx).
