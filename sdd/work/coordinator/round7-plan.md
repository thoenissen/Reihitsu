# Round 7 plan (bookkeeping for the decided questions, list of the open ones)

Written 2026-10-20 from `state.md` (principles P1-P13, "Round 7a decided" items 1-22 = P14-P24) and the five inventories `inventory/round7-{la-lc,o2,d3,b2,b1}.md`. Nothing is posted or created on GitHub. Branch `claude/sdd-coordinator`, not committed.

Numbering used here: P14a-d = items 1-4, item 5 = RH3004 `static`, P15 = item 6, P16 = item 7, P17 = item 8, item 9 = innermost region, P18 = item 10, P20 = item 11, P19 = item 12, item 13 = RH8107-001 retirement, P23 = item 14, P24 = item 15, P22 = item 16, P21 = item 17, items 18/19/20 = single decisions, item 21 = P3/P5 extensions, item 22 = #1160. (The inventories' own candidate numbers differ; this file and `state.md` "Round 7a decided" rule. In `state.md` the candidate P23 reads "report only what a correction can legally replace"; the maintainer's item 14 says the opposite for the report side: still report, offer no fix. Item 14 wins.)

Counts (rows counted from the inventory tables, #1177 Q1b and the #1245 Q2 continuation counted as own rows): LA/LC 46, O2 20, D3 16, B2 32, B1 31 = **145 rows** (the dispatcher's 148 and the inventory headers' 148/147 over-count: O2 header says 22, table has 20; D3 header says 18, table has 16; B1 header says 29, table has 31). **121 decided, 24 open (round 7b)**; per family decided/open = 44/2, 18/2, 12/4, 31/1, 16/15. 62 issues: **45 fully decided (close and tick)**, 17 keep at least one open question.

## 0. Follow-up map

Extensions = one-line comment on an existing open issue. New = `N##`.

### Existing issues that take instances (comment text: "Instance of <Pn> from #<issue> Q<n>: <input -> expected>")

| Existing | Takes |
|---|---|
| #1138 SDD P9/P10 | wording P14a-d, P23, P24 (section 1b); its P10 example for RH3202 changes (P14d) |
| #1140 P9 line-break/alignment fixes | all "fix formats the enclosing node" and "blank line inserted before the changed statement" instances (list in section 1) |
| #1144 P10 fix layout | P10 instances and P14b (list in section 1) |
| #1147 P12 | RH5301/5303/5304, RH5302, RH5305, RH5308 comment-led line; RH7304 comment-led column (#1176 Q1 analyzer part) |
| #1091 P8 | #1195 Q1, #1195 Q3 (+ retirement of the case-insensitive wording of RH8107-001, item 13, maintainer ok already given) |
| #1241 RH8204 fix, no end tag | retitle to "RH8107, RH8202, RH8204 fixes: an end tag with another name is no end tag (P19)"; takes #1197 Q1, #1199 Q4, #1201 Q1 (markup B and D), #1201 Q3 |
| #1257 generic-invocation fixes | #1227 Q2, #1229 Q3, #1233 Q2, #1245 Q1 (+ continuations: `!x switch {...}`, `!x + t`, `>>`/`>>>` with 2-3 open `<`) |
| #1258 RH3103 dotted nameof | #1231 Q2 (fix throws; P23: report stays, fix withheld) |
| #1077 P6 `/** */` | #1201 Q2 (RH8204 on comments documenting nothing), #1235 Q5 (RH3201) |
| #1139 SDD P12/P13 | instances of P13: #1201 Q2, #1216 Q2, #1235 Q1, #1235 Q5 |
| #1153 docs P8b/P8c | #1199 Q1 (RH8202 reports explicit implementations; exemption is presence-only, as decided for #1129 Q1) |
| #1079 spacing rules | #1174 Q1 (RH7302 reads the description at a fixed offset; `# region` is a region directive, same reading as #1043 Q1). Page says "three independent defects": add as fourth |
| #1240 SDD inactive-`#if` non-goal | P22 wording (region/pragma stay reported without fix) |
| #1136 SDD relocation/edit of comments | #1185 Q1 (fix relocates region blocks, not the comments above them: AGENTS.md says withhold) |
| #1244 unreachable nested recursion | pointer to N12 (the behavior change lands there; the code-review report can close with it) |
| #1243 formatter blank line when joining a lone operator | blocks N14 |
| #1259 share RH3106/RH3107 removal | related to N23 (do N23 on the shared removal) |
| #954 (empty region after `{`) | related to N04 only |

Instances per extension target:

- **#1140 (P9, plus P14a)**: #1169 Q1 (bullets 1-4: whole declaration formatted; bullet 5: blank line inserted after `namespace N;`), #1209 Q2, #1209 Q3, #1213 Q1, #1214 Q3, #1215 Q2, #1216 Q5, #1217 Q2, #1218 Q5, #1219 Q2, #1220 Q1, #1220 Q2, #1220 Q3 (second part: re-indents the body above, moves `else`), #1221 Q3, #1222 Q1, #1235 Q2. Tests that pin the blank line change: `VerifyBlankLineAboveCommentOfFixedClassInFileScopedNamespaceIsInserted` and the two RH5405/RH5403 tests named in #1209 Q3/#1220 Q2.
- **#1144 (P10, plus P14b)**: #1168 Q1, #1170 Q1 (P14b), #1209 Q4, #1220 Q3 (first part), #1230 Q1, #1235 Q3, #1238 Q2. (#1226 Q2 goes to N20, same rule.)
- **#1147 (P12)**: #1210 Q1, #1212 Q1, #1215 Q4, #1218 Q1, #1176 Q1 (analyzer part).

## 1. DECIDED (121)

Format: `issue Qn | decision | goes to`.

### 1a. LA/LC (#1168-#1170, #1209-#1222): 44 decided, 2 open (#1214 Q1, #1218 Q4)

```
#1168 Q1 | P10 | #1144
#1169 Q1 | P9 + P14a (bullet 5: no blank line inserted) | #1140
#1169 Q2 | P11 (`class C { };` reported, fix removes `;`) | N16
#1169 Q3 | P22 (conditional directive/excluded code in body: not reported; #region/#pragma: reported, no fix; RH5416 test changes) | N16 (+ #1240 wording)
#1169 Q4 | P1 (fix edits `#region legacy` text) | N02
#1170 Q1 | P14b (member moves to its own line, `int _x;` on the next; also "two applications"/column 6 are P7/P10) | #1144
#1170 Q2 | P1 | N02
#1209 Q1 | P1 (trailing comment duplicated) | N02
#1209 Q2 | P9 | #1140
#1209 Q3 | P14a | #1140
#1209 Q4 | P10 | #1144
#1210 Q1 | P12 + P7 | #1147
#1211 Q1 | P11 | N12
#1211 Q2 | P11 | N12
#1212 Q1 | P12 + P7 | #1147
#1212 Q2 | P11 | N14
#1213 Q1 | P9 | #1140
#1214 Q2 | P11 | N12
#1214 Q3 | P9 | #1140
#1215 Q1 | P11 | N13
#1215 Q2 | P9 | #1140
#1215 Q3 | P11 | N13
#1215 Q4 | P12 + P7 | #1147
#1216 Q1 | P11 (remove the guard) | N13
#1216 Q2 | P13 (`///` like other comments) | N13 + #1139
#1216 Q3 | P11 | N13
#1216 Q4 | P11 | N13
#1216 Q5 | P9 | #1140
#1217 Q1 | item 20 (`?[ ]` not covered by RH5307; clarification in spec/page) | N18
#1217 Q2 | P9 | #1140
#1217 Q3 | guard scope (AGENTS.md, precedent #1116 Q2): resolve the reported list by the diagnostic span | N18
#1218 Q1 | P12 + P7 | #1147
#1218 Q2 | P7 (fix leaves the diagnostic) | N15
#1218 Q3 | P11 | N14
#1218 Q5 | P9 | #1140
#1219 Q1 | guard scope (AGENTS.md, precedent #1145): judge the property's own lines | N18
#1219 Q2 | P9 | #1140
#1220 Q1 | P9 | #1140
#1220 Q2 | P14a | #1140
#1220 Q3 | P10 (first part) + P9 (second part) | #1144 + #1140
#1221 Q1 | P21 (report at `extension` keyword, block formatted) | N17
#1221 Q2 | P11 | N17
#1221 Q3 | P9 | #1140
#1222 Q1 | P9 | #1140
```


### 1b. O2 (#1171-#1177, #1183-#1187): 18 decided, 2 open

```
#1171 Q1 | P18 | N07
#1172 Q1 | P18 | N07
#1173 Q1 | P18 | N07
#1174 Q1 | #1043 Q1 reading + P7 | #1079 (ext)
#1175 Q1 | P1 (withhold the fix for the pair; diagnostic stays) | N03
#1175 Q2 | P11 (regions in expression bodies, initializers, ctor-initializer args, parameter defaults, attribute args are inside executable code) | N06
#1175 Q3 | P11 (report both directives in top-level statements) + P1 (fix scope) | N06 + N03
#1175 Q4 | P14c | N04
#1176 Q1 | P12 (analyzer) + P1 (fix must not write `/* c */ #region`) | #1147 + N03
#1176 Q2 | fix part only: P7 + P9 (fix withheld for straddling pair); analyzer part OPEN, see 7b | N03  [question stays open]
#1177 Q1b | P7 (no-op fix converges or is withheld) | N03
#1183 Q1 | item 9 (innermost region judges the member) | N05
#1184 Q1 | item 18 (alias to a non-name = own group at the end) | N08
#1185 Q1 | AGENTS.md "prefer no fix": withhold the fix (comments are not carried along), see risk R7 | N03 + #1136
#1185 Q2 | P17 (enum regions belong to the enum) + P1 for the fix | N05 + N03
#1185 Q3 | P1 | N03
#1186 Q1 | P14c | N04
#1187 Q1 | item 19 (exclusive #if/#else branches are not compared) | N05
#1187 Q2 | P17 | N05
```

(#1176 Q2 is listed for its decided fix part but stays open; #1177 Q1 is open, section 3.)

### 1c. D3 (#1195-#1201): 12 decided, 4 open (#1195 Q2, #1196 Q1, #1199 Q3, #1200 Q2)

```
#1195 Q1 | P8 + P20 | #1091
#1195 Q3 | P8 + item 13 (case-insensitive RH8107-001 retired) | #1091
#1196 Q2 | P20 (check every top-level <returns>) | N11
#1197 Q1 | P19 | #1241 (ext)
#1198 Q1 | P9 (+ P24 for the five CRLF->LF breaks) | N10
#1199 Q1 | #1129 Q1 precedent (exemption of explicit implementations is presence-only), see risk R8 | #1153 (ext)
#1199 Q2 | P7 | N10
#1199 Q4 | P19 | #1241 (ext)
#1200 Q1 | P20 (RH8201-001 already says every event) | N11
#1201 Q1 | P9 (remove only the two tags, content verbatim) + P19/P1 (withhold when markup not well formed) | N10 (content) + #1241 (malformed)
#1201 Q2 | P13 + P6 | #1077 + #1139
#1201 Q3 | P19 + P1 (same bug as #1241) | #1241
```


### 1d. B2 (#1224-#1238, #1245): 31 decided, 1 open (#1245 Q3)

```
#1224 Q1 | P11 | N19
#1225 Q1 | P11 | N19
#1226 Q1 | item 5 (fix keeps `static`; check `async`) | N20
#1226 Q2 | P10 | N20
#1226 Q3 | P1 (parenthesize the lambda or withhold) | N20
#1227 Q1 | P1 | N22
#1227 Q2 | P1 | #1257
#1228 Q1 | P24 | N24
#1229 Q1 | P1 (virtual base call; RH3101.md example changes) | N22
#1229 Q2 | P1 | N23
#1229 Q3 | P1 fix part; P23 report stays | #1257
#1230 Q1 | P10 | #1144
#1231 Q1 | P23 (report stays, fix withheld for typeof(Nullable<>), member access, using alias, nameof dotted) | N22
#1231 Q2 | P23 | #1258
#1232 Q1 | P1 | N23
#1233 Q1 | P1 | N23
#1233 Q2 | P1 fix part; P23 report stays | #1257
#1234 Q1 | P23 (report stays, fix withheld where the target is not fixed) | N22
#1235 Q1 | P13 | N21
#1235 Q2 | P9 | #1140
#1235 Q3 | P10 | #1144
#1235 Q4 | P1 | N22
#1235 Q5 | P13 + P6 | N21 + #1077
#1235 Q6 | P11 | N21
#1236 Q1 | P14d (two blank lines stay, RH3202-005 unchanged) | N25
#1237 Q1 | P14d | N25
#1238 Q1 | P1 | N23
#1238 Q2 | P10 | #1144
#1245 Q1 (+cont. 3-6) | P1 | #1257
#1245 Q2 | guard scope (named argument gets a fix, plain does not, no safety reason) | N26
#1245 Q2 cont. | guard scope for `[!x, y]`; P23 for constant patterns `x is !true`, `!true => 1` (report, no fix) | N26
```

(#1245 Q3 is open.)

### 1e. B1 (#1247-#1256): 16 decided, 15 open (section 3)

```
#1247 Q4 | P16 (`throw` arm = no value) | N31
#1247 Q5 | P16 (parentheses, `!` transparent) | N31
#1248 Q1 | P16 (`throw` arm in a declaration initializer) | N31
#1251 Q1 | P1 | N27
#1251 Q2 | P1 | N27
#1251 Q3 | P1 (produced name allowed, mutation semantics not) | N27
#1251 Q6 | P1 (also SHARED-FIX-001) | N27
#1252 Q2 | P15 | N28
#1253 Q1 | P3 extension (aliases resolved semantically) | N09 (blocked by N01)
#1254 Q3 | P1 (fix withheld for explicit operator implementations; moot if 7b Q2 exempts them) | N29
#1254 Q4 | P15 (RH2004 fix on one part withheld) | N28
#1254 Q5 | P15 (report once, on the symbol) | N28
#1255 Q1 | P15 (symbol `IsStatic`) | N28
#1256 Q1 | P5 | N30
#1256 Q2 | P5 extension (Razor `@* *@`, `<!-- -->`) | N30
#1256 Q3 | P5 extension (`@@` escaped; `user@code{` email-like) | N30
```


### Count check

Decided / open rows: LA/LC 44/2, O2 18/2, D3 12/4, B2 31/1, B1 16/15; total 121/24 of 145 rows. Issues: 45 fully decided, 17 with open questions (62).

## 1b. SDD wording (P14-P24)

Proposal: **extend #1138** (code-fixes.md: P14, P23, P24 sit with SHARED-FIX-006/007, which do not exist yet in `sdd/specs/shared/code-fixes.md`; the squads cite them as planned) and **one new SDD issue N01** for P15-P22, the P3/P5 extensions and items 9, 18, 19. Both are SDD spec issues, no behavior change; all bug/behavior issues below are blocked by the one that holds their wording.

### For #1138 (code-fixes.md)

```
SHARED-FIX-006 / -007 (#1138) get these additions (P14a-d):

(P14a) A fix MUST NOT insert a blank line before the node it changes, and MUST NOT change the blank lines between that node and its neighbours.
(P14b) When the changed member shares its line with another member after the fix, the changed member gets its own line and the other member starts on the next line; the lines the fix edits are laid out as the formatter lays them out (SHARED-FIX-007).
(P14c) A fix that removes a region or conditional directive line also removes the blank line that the removal leaves at the start of a body, at the end of a body, or doubled between two lines; it inserts none.
(P14d) A fix formats only the node it replaces, with the whitespace of the lines it edits; it never formats the member, statement, or type that contains that node. Example (replaces the RH3202 layout example of SHARED-FIX-003): two blank lines above a get-only method stay two blank lines.

SHARED-FIX-003 (P23), add to "The diagnostic is still reported.":
A rule reports the construct it names for every input, also for inputs where the fix is withheld because no correction compiles or keeps what the program does (for example `typeof(Nullable<>)`, `x is !true`).

New SHARED-FIX-008 — A fix keeps the document's line ending (P24)
A fix MUST write only the line ending the document already uses. Every line break the fix creates or keeps is that line ending, in a CRLF document as in an LF document.
Why: a fix that introduces the other line ending leaves the file with mixed endings that the formatter then rewrites.
```

### For N01 (new SDD issue: "SDD: Record P15-P22 and the alias and Razor extensions in the shared specs")

```
shared/diagnostics.md
SHARED-DIAG-004 — A partial declaration is one declaration (P15)
  A rule judges a partial type or member by its symbol (modifiers, async, static, accessibility of all parts together) and reports it once. A fix that would have to edit several parts is withheld (SHARED-FIX-003).
SHARED-DIAG-005 — Parentheses, `!` and a `throw` branch do not change a value (P16)
  When a rule judges the value of an argument or initializer, parentheses and the null-forgiving `!` are transparent, and a `throw` branch or arm yields no value, so the other branches decide.
SHARED-DIAG-006 — A rule checks every element of its kind (P20)
  A rule over a kind of element (documentation element, declarator of a field or event) checks every such element of the construct; the verdict does not depend on the element's position or on the order of the declarators.
SHARED-DIAG-007 — A diagnostic has a source location (P21)
  Every diagnostic has a span in a source file. A construct without a name (an `extension` block) is reported at a defined token (the `extension` keyword).
SHARED-DIAG-008 — A conditional directive inside the reported construct (P22)
  A construct that contains `#if`, `#elif`, `#else`, `#endif` or code excluded by them is not reported (SHARED-TRAIL-002 is one instance). `#region`, `#endregion` and `#pragma` inside the construct do not stop the report; the fix is withheld.
amend SHARED-DIAG-002 (P3 extension): a using alias is resolved to the symbol it names; `NIE` and `System.NotImplementedException` are the same name. Remove "an alias is a different name".
amend SHARED-DIAG-003 (P5 extension): the text of a Razor comment `@* ... *@` and of an HTML comment `<!-- ... -->` is content, as is the escaped `@@`; an `@` after a letter or digit is text.

shared/type-regions.md (SHARED-TYPEREGION-002/-006, P17 and item 9)
  A region belongs to the innermost enclosing type-like declaration (class, struct, interface, record, enum). Regions of different declarations are never compared or moved across. A member is judged by the innermost region that encloses it (RH7308).
  Branches of one #if/#else that exclude each other are not compared for duplicate region descriptions (item 19, RH7311).

shared/using-directive-ordering.md
  SHARED-USING-007 (P18): a directive is compared with the previous directive of the same group (section, kind, root), whatever other directives lie between. Same for every ordering rule over a sequence.
  SHARED-USING-004 (item 18): an alias whose target is not a name (`using B = int;`, a tuple) forms its own group after all other groups of its section.

shared/element-documentation.md
  new SHARED-DOC-017 (P19) — An end tag with another name is no end tag
  An XML element whose end tag has a different name from its start tag (`<remarks><returns>Nothing</remarks>`) has no end tag of its own; the fixes of RH8107, RH8202 and RH8204 are withheld for it and the diagnostic stays.
  SHARED-DOC-014/-015/-016 (P8): documentation is the merged comments of the declaration; tag names are case-sensitive (retires the case-insensitive wording of RH8107-001, item 13). This text belongs to #1091, not to N01.
```

## 2. Question issues that become fully decided: 45 (close and tick in #912)

Close each only after the follow-ups it points to exist (working agreement), then tick.

```
LA/LC (15): #1168 #1169 #1170 #1209 #1210 #1211 #1212 #1213 #1215 #1216 #1217 #1219 #1220 #1221 #1222
O2   (10): #1171 #1172 #1173 #1174 #1175 #1183 #1184 #1185 #1186 #1187
D3    (3): #1197 #1198 #1201
B2   (15): #1224 #1225 #1226 #1227 #1228 #1229 #1230 #1231 #1232 #1233 #1234 #1235 #1236 #1237 #1238
B1    (2): #1253 #1255
Total 45
```

Staying open (17): #1214, #1218 (LA/LC); #1176, #1177 (O2); #1195, #1196, #1199, #1200 (D3); #1245 (B2); #1247, #1248, #1249, #1250, #1251, #1252, #1254, #1256 (B1).

#912 feasibility: GitHub reports 100 sub-issues (cap), so the 62 issues cannot be attached as sub-issues. But the #912 body is a plain markdown checklist of `- [x] #nnnn` lines (the round-5 24 issues were added to it as bare lines, no sub-issue link), about 7 KB, far from the 65 536 character limit. So: **edit the #912 body** (issue_write update) to append a new block "Analyzer specs written in #991-#997 (round 7)" with 62 lines `- [ ] #1168 — shared/trailing-commas.md` ... (titles give the spec path), ticking the 45 above, and post one **comment** with the decision summary and follow-up list (as in earlier rounds). The comment alone would leave the checklist wrong, so do both. If a body edit is not wanted, fall back to the comment only.

## 3. REMAINING OPEN questions (round 7b): 24

Kinds: bug / clarification / behavior change. All "today's output" lines are from the inventories.

### Group A. The rule page accepts a nested one-line form, the formatter expands it (2)

```
#1214 Q1 (RH5304 vs FMT-BREAK-021)
  var o = new C { L = { 1, 2 } };
  today: not reported (rule page: accepted); the formatter, and the fix of another assignment in the same initializer, expands it to a block.
  A) keep the analyzer, state in FMT-BREAK-021 that the formatter expands it (clarification; P11: formatter may do more)
  B) report it (behavior change, new diagnostics)
  Recommendation: A. Kind: clarification (SDD spec), and the fix expansion is gone anyway via P9 (#1214 Q3).

#1218 Q4 (RH5308 vs FMT-BREAK-018)
  var x = a ? 1 : b ? 2 : 3;
  today: not reported (rule page: one-line conditional never reported); the formatter always spreads a conditional containing a nested one.
  A) keep as is, state it in FMT-BREAK-018 (clarification)   B) report nested one-line conditionals (behavior change)
  Recommendation: A, same answer as #1214 Q1. Kind: clarification.
```

### Group B. Regions (2)

```
#1176 Q2, analyzer part (RH7304)
  class C { #region Outer  void M() { #region Inner ... #endregion } #endregion }   // Inner opened at type level, closed inside M
  today: the analyzer pairs only directives outside executable code (Inner with Outer's #endregion), reports both; the fix pairs like the compiler, edits the #endregion in M, the diagnostic stays. (Fix part decided: withheld.)
  A) analyzer pairs like the compiler and, when a pair has a directive in executable code, reports nothing for it (RH7303 already reports the stray #endregion)
  B) keep the analyzer pairing, state it in the spec (clarification)
  Recommendation: A. Kind: bug (analyzer), with N03.

#1177 Q1 (RH7306)
  #region Implementation ... #endregion // Implementation
  today: both reported; the fix writes `#region` ... `#endregion //` (unnamed region, dangling `//`).
  A) withhold the fix when nothing remains of the name, keep the diagnostic (consistent with RH7301-005: choosing a name is the user's)
  B) remove the dangling `//` too   C) keep as is
  Recommendation: A. Kind: bug (fix) in N03.
```

### Group C. Documentation rules (4)

```
#1195 Q2 (RH8102/8103/8109/8110) and #1199 Q3 (RH8202): declaration-kind scope
  /// <param name="z"/>  on a class without primary ctor -> RH8102 silent; on `M(int a, int b)` surplus param reported; RH8103 reports <param> on class `C`, not on interface `I` or property `P`; RH8110 reports <typeparam/> on `Run()`, not on a ctor.
  /// <value> on a method -> RH8202 silent (only properties/indexers registered).
  A) report on every documentable declaration (the element documents something that does not exist; P11 own gap)
  B) keep the current scope per rule and write it into the specs (clarification)
  Recommendation: A for #1195 Q2 (behavior change, new diagnostics, one issue), A for #1199 Q3.

#1196 Q1 (RH8106 vs RH8107): one span, two rules
  /// <returns/>  on `void H()` -> RH8106 ("no text", advice: add text) and RH8107 (void must not document a return) report the same span.
  A) RH8107 owns void members, RH8106 skips them (behavior change)
  B) both report (documented overlap)
  Recommendation: A, otherwise RH8106's advice contradicts RH8107 (SHARED-FIX-004 spirit). Kind: behavior change.

#1200 Q2 (RH8203): constructor matching
  class B { private B(ref int a) {} }  class D : B { /// <inheritdoc/> D(int a) {} }
  today: signature compared by type only (ref/in/out/params, nullable annotations, base accessibility ignored), so D "inherits".
  A) match what the compiler resolves (ref kind, accessibility; nullability ignored)   B) keep type-only matching
  Recommendation: A (consistent with P8). Kind: bug.
```

### Group D. RH3001 (1)

```
#1245 Q3 (RH3001 fix output)
  B >>>= !x;  B ??= !x;   -> (x == false) with parentheses
  also named attribute args, `goto case`, `select`/`group`/`orderby` get parentheses; the same positions with `=`, `+=`, `>>=`, `where`, `let`, `join` get none.
  Partly decided: where RH3002 (widened by #1224 Q1) reports the produced pair, P7/SHARED-FIX-004 makes it a bug. Open: positions RH3002 does not check (`select`, `group`, `orderby`, `goto case`, named attribute args).
  A) parentheses only where binding needs them (bug)   B) accept the Simplifier result (clarification)
  Recommendation: A; decide after the RH3002 issue (N19) exists. Kind: bug (guard), in N26.
```

### Group E. Equality analyzers (RH1001/RH1002, SHARED-EQUAL) (9)

```
#1247 Q1
  struct S : IEquatable<S> { }  // no GetHashCode override
  HashSet<S> / items.Distinct()  -> not reported (rule pages: "IEquatable or override both"; tests pin it)
  A) keep ("has equality members" = IEquatable OR overrides)  B) require GetHashCode too, report this input
  Recommendation: A, clarification text in the spec (boxing ValueType.GetHashCode is cheaper than no equality at all and the pages already say so). Kind: clarification.

#1247 Q2
  Dictionary<S?, int>, Dictionary<(int, S), int> -> not reported; Dictionary<S, int> reported.
  A) look through Nullable<S> and tuples (new diagnostics)  B) outer type only, stated in the spec
  Recommendation: B for now (P4: nothing forces it; a specialized rule if wanted). Kind: clarification (A would be behavior change).

#1247 Q3
  ToImmutableDictionary(x => x, x => 0, null, valueComparer)  -> not reported (comparer for the int values taken as key comparer; keys hashed by default).
  A) judge the comparer's type argument against the key type (bug)  B) keep
  Recommendation: A. Kind: bug.

#1247 Q6
  Distinct(default(TC)) with TC : IEqualityComparer<S> -> reported; default(TV) with a struct constraint -> not reported.
  A) default(T) of a type parameter is a null comparer only when T is a reference type (constraint class); otherwise it is a real comparer, report neither struct nor unconstrained  B) report both
  Recommendation: A. Kind: bug (edge case, in N31 or separate small).

#1248 Q2 / Q3 (collection expressions, RH1001)
  HashSet<S> set = [with(_comparer), _item];   -> declared type reported (but `new(_comparer) { .. }` not)
  Take([_item]);  (parameter HashSet<S>)        -> not reported; Take(new()) reported
  A) recognize collection expressions: `with(comparer)` counts as comparer; `[_item]` converted to HashSet<S> counts as a creation without comparer (new diagnostics)
  B) do not handle collection expressions (state it)  C) Q2 only
  Recommendation: A for both (the same creation, preview syntax already compiles). Kind: behavior change.

#1249 Q1 (RH1002 method list)
  items.CountBy(x => x), AggregateBy, LeftJoin, RightJoin, ImmutableHashSet.Create/CreateRange, ImmutableDictionary.CreateRange -> not reported (GroupBy, Join, ToImmutable* are).
  A) extend the fixed list (behavior change)   B) keep, list stated in the spec
  Recommendation: A. Kind: behavior change.

#1249 Q2 (RH1002 query clauses)
  from x in items group x by x;  join ... on ...;  join ... into ...;  -> not reported (compiler emits GroupBy/Join/GroupJoin).
  A) report at the clause keyword (`group`/`join`) (needs a location rule)   B) not covered, stated
  Recommendation: A, location = the clause keyword (P21). Kind: behavior change.

#1250 Q1 (RH2006/RH2007, SHARED-DEBUG)
  Debug.Assert(c, string.Empty);  Debug.Fail(string.Empty);  Debug.Assert(c, flag ? "" : null);  -> not reported; Debug.Assert(c, "") reported.
  A) `string.Empty` and conditionals whose every branch is empty are "no text"  B) only literal constants
  Recommendation: A for `string.Empty` and constant-folding (`const string E = ""`), B for conditionals with `null` branches? Simplest: A for both, using P16 transparency. Kind: behavior change.
```

### Group F. RH2001 (2)

```
#1251 Q4
  int P { get => field; set => field = value * 2; }   int Other { get => field; }   int Plain => field;   -> reported, no fix
  A) report only properties whose accessors are pure auto-implemented (compiler-created backing field AND no accessor logic)  B) keep the compiler definition (reported, no fix)  C) report none using `field`
  Recommendation: B (no fix is a supported end state), written into the spec. Kind: clarification.

#1251 Q5
  [field: NonSerialized] private int Cache { get; set; }   int Q { [ExcludeFromCodeCoverage] get; set; }   -> reported; rule page says "properties decorated with attributes are exempt".
  A) exempt them as the page says (bug)  B) report, correct the page (fix could move `[field:]`)
  Recommendation: A for accessor attributes; `[field: ...]` too (the fix cannot move it safely). Kind: bug.
```

### Group G. Explicit interface members (3)

```
#1252 Q1 (RH2002)
  async void I.M() {...}  -> not reported; public async void N() reported.
  A) report explicit implementations too (same hazard)  B) exempt (interface dictates void)
  Recommendation: A for async void: an explicit implementation can still return Task only if the interface does, so B is right only when the interface method returns void... The hazard is the same either way; A reports (no fix when the interface dictates void, P23). Kind: behavior change.

#1254 Q1 (RH2004 fix)
  static S operator -(S a) {...}   -> the fix adds `private`, still CS0558.
  A) fix adds `public` for operators (RH2004 asks for explicit accessibility; operators must be public static)  B) withhold the fix
  Recommendation: A. Kind: bug (fix).

#1254 Q2 (RH2004)
  static N IAdd<N>.operator +(N a, N b) {...}   and explicit conversion  -> reported; explicit implementations take no modifier; RH2004-003 exempts methods, properties, indexers, events but not operators.
  A) extend the exemption to operators and conversions (bug)  B) keep, withhold the fix only (#1254 Q3)
  Recommendation: A (then #1254 Q3 is moot). Kind: bug.
```

### Group H. RH2109 (1)

```
#1256 Q4
  @functions { int Count; }  -> not reported (older equivalent of @code)
  A) report @functions like @code (behavior change)  B) @code only
  Recommendation: A (same risk, same fix text if the rule has one). Kind: behavior change.
```

Count: A 2, B 2, C 4 (#1195 Q2, #1196 Q1, #1199 Q3, #1200 Q2), D 1, E 9 (#1247 Q1, Q2, Q3, Q6; #1248 Q2, Q3; #1249 Q1, Q2; #1250 Q1), F 2, G 3, H 1 = **24**.

## 4. New issues (31): kind, questions, requirement IDs, blocked by, draft body

Convention: bug issues blocked by the SDD issue that holds their wording get "Blocked by". Bodies are short; every body ends "Decided by <principle> (#912 round 7a)"; "Done when" always includes a regression test per input.

### N01 [SDD spec] SDD: Record P15-P22 and the alias and Razor extensions in the shared specs
Questions: none directly; carries P15-P22, P3/P5 extension, items 9, 18, 19, SHARED-DOC-017. Req IDs: new SHARED-DIAG-004..008, SHARED-DOC-017; amend SHARED-DIAG-002/-003, SHARED-TYPEREGION-006, SHARED-USING-004/-007. Blocked by: none (#1240 related, #1138 independent).
Body: What: add the wording of section 1b to the shared specs. Why: round 7a decided P15-P22; the 62 round-7 issues rest on them. Done when: specs merged, requirement-ids.txt updated, tests/tags unchanged (specs only).

### N02 [bug] Fixes edit or duplicate comment and directive text: RH5412-RH5416, RH5417, RH5405-RH5407
Questions: #1169 Q4, #1170 Q2, #1209 Q1. Req: SHARED-EMPTY-003, RH5417-003, SHARED-BRACE-002, SHARED-FIX-003. Blocked by: none.
Body: `#region legacy⏎#endregion⏎class A {}` -> fix rewrites to `#region Legacy` / `#endregion // Legacy` (also in accessor-list regions of RH5417); `if (a)⏎    M(a, b); // trail` -> `{ M(a, b); // trail } // trail` (comment duplicated). Expected: the fix touches only what it reports; comment/directive text unchanged (P1, SHARED-FIX-003 reading from #1068: only text the rule itself reports may change). Done when: those inputs keep the text, fix still offered where it can be safe.

### N03 [bug] Region fixes are withheld when they break code and converge: RH7303, RH7304, RH7306, RH7309
Questions: #1175 Q1, #1175 Q3 (fix part), #1176 Q1 (fix), #1176 Q2 (fix part), #1177 Q1b, #1185 Q1, #1185 Q2 (fix part), #1185 Q3 (+ #1176 Q2 analyzer and #1177 Q1 when decided in 7b). Req: RH7303-004, RH7304-004, RH7306-003, RH7309-004, SHARED-TYPEREGION-006, SHARED-FIX-001/003/005. Blocked by: N01 (P17), #1136 related.
Body (examples): straddling region `#region Members` at type level, `#endregion` inside `M` -> only `#endregion` reported, fix removes both (P1: withhold the pair, diagnostic stays); `#region Fields⏎    /* c */ int _a;` -> fix writes `    /* c */ #region Fields` (does not parse); `#region IDisposable⏎int _a;⏎#endregion //Implementation` -> `#endregion` reported, fix changes nothing (P7); RH7309 swaps blocks but `// The disposal logic` stays above the other region, and swaps regions in a field initializer or between a class and its nested enum (does not parse): withhold. Expected: fix withheld or converging as stated. Done when: all inputs tested.

### N04 [bug] Region-removing fixes clean the blank line they leave (P14c): RH7303, RH7310
Questions: #1175 Q4, #1186 Q1. Req: RH7303-004, RH7310-004, SHARED-BLANK-003, SHARED-FIX-007. Blocked by: #1138 (P14c wording). Related: #954.
Body: `int _a;⏎⏎#region Methods⏎⏎#endregion // Methods⏎}` -> fix leaves `int _a;⏎⏎}`; `{⏎⏎` after removing a leading region; `var x=1;⏎⏎#region⏎⏎x++;` -> two blank lines. Expected: the blank line the removal leaves is removed (start/end of body, doubled), no blank line inserted. Open sub-point in the issue: `if (a){}⏎#region…⏎M(a);` -> `}` directly followed by `M(a);` stays (P14a: nothing inserted). Done when: formatter-clean result for each shape.

### N05 [behavior change] Region rules: the innermost declaration and region own a member (P17): RH7308, RH7309, RH7311
Questions: #1183 Q1, #1185 Q2 (analyzer), #1187 Q2, #1187 Q1. Req: RH7308-001, RH7309-001/002, RH7311-001, SHARED-TYPEREGION-002/-006. Blocked by: N01.
Body: `#region Fields` around nested `#region Properties` holding property `P` -> `P` reported against `Fields`; expected: innermost region judges. `enum E { #region Values A, #endregion }` inside a class counted as a region of the class -> RH7309/RH7311 report; expected: the enum owns it. Same description in `#if DEBUG` and `#else` -> second `#region Fields` reported; expected: not compared (item 19). Done when: tests; check RH7305/7305A/7310 for the same enum assumption.

### N06 [behavior change] RH7303 reports regions in every executable place (P11)
Questions: #1175 Q2, #1175 Q3 (analyzer). Req: RH7303-001, SHARED-TYPEREGION-005/006; knock-on RH7304, RH7305/305A, RH7308, RH7309, RH7310, RH7311. Blocked by: N01 (SHARED-DIAG-008 mention), N03 for the fix side.
Body: region inside `int M() =>⏎#region …`, a field initializer, ctor-initializer arguments, parameter defaults, attribute arguments, top-level statements (`#endregion` belongs to the end-of-file token) -> not reported by RH7303 and counted as a region of the type. Expected: reported as inside executable code; stops counting for the type rules. Done when: both directives reported; fix scope per N03.

### N07 [bug] Ordering rules compare with the previous directive of the same group (P18): RH7203, RH7204, RH7206
Questions: #1171 Q1, #1172 Q1, #1173 Q1. Req: RH7203-001/002, RH7204-001/002, RH7206-001/002, SHARED-USING-007. Blocked by: N01.
Body: `using Microsoft.B; using Alpha.X; using Microsoft.A;` -> not reported; with `using System;` or `using static System.Math;` between -> reported. Expected: `Microsoft.A` reported whatever lies between. Same for aliases (#1172) and static usings (#1173). RH7207 already reports all. Done when: three rules, tests; look for the same pattern in the other ordering rules.

### N08 [bug + clarification] A using alias to a non-name forms its own group at the end: RH7204, RH7207, formatter `UsingGrouping`
Questions: #1184 Q1. Req: SHARED-USING-004/-005, RH7204, RH7207-003. Blocked by: N01.
Body: `using A = System.Text;⏎⏎using B = int;` -> RH7207 reports and the fix puts `using B = int;` first (root `""` sorts like `System`). Expected: `using B = int;` last in its section as its own group; analyzer and formatter agree. Done when: tuple targets too; formatter idempotent.

### N09 [bug] RH2003 resolves the exception type semantically (P3 extension)
Questions: #1253 Q1. Req: RH2003-001, SHARED-DIAG-002. Blocked by: N01.
Body: `using NIE = System.NotImplementedException; throw new NIE();` -> not reported (syntactic name pre-filter). Expected: reported. Check RH2003 for other syntactic pre-filters. Done when: alias and qualified forms reported.

### N10 [bug] Documentation fixes change only the reported markup and finish in one application: RH8201, RH8202, RH8204
Questions: #1198 Q1, #1199 Q2, #1201 Q1 (content part). Req: RH8201-005, RH8202-001/003, RH8204-002/003, SHARED-FIX-001, SHARED-FIX-006, SHARED-FIX-008. Blocked by: #1138 (P9, P24).
Body: RH8201 fix replaces the comment with `<inheritdoc/>` but moves `#if`/`#pragma` to column 0, re-indents attributes/members, joins `/** */` and the member, CRLF -> LF on five breaks. RH8202: two `<value>` elements -> one diagnostic, fix needs two applications. RH8204: fix re-serializes content (`&#32;` decoded, `>` -> `&gt;`, quotes rewritten); expected: remove only the two tags, content verbatim (whitespace-only content keeps its space). Malformed markup is #1241. Done when: each input, CRLF test.

### N11 [bug / behavior change] Rules check every element of their kind (P20): RH8106, RH8203
Questions: #1196 Q2, #1200 Q1. Req: RH8106-001, RH8203-001, RH8201-001, SHARED-DIAG-006. Blocked by: N01.
Body: two `<returns>` (non-empty first, empty second) -> nothing; opposite order -> reported. `event EventHandler First, Fourth;` with `<inheritdoc/>` where `Fourth` inherits nothing -> not reported; `Fifth, Second` reported. Expected: every element/declarator checked (RH8201-001 already says every event). Done when: both orders.

### N12 [behavior change] RH5301 and RH5304 check nested initializers and members on the brace line (P11)
Questions: #1211 Q1, #1211 Q2, #1214 Q2. Req: RH5301-001/002, RH5304-001/002, FMT-BREAK-021, FMT-INDENT-023. Blocked by: #1161 (P11 spec). Related: #1244 (unreachable recursion, probable cause; close with this).
Body: `new C⏎{⏎    Inner = { P = 1,⏎       Q = 2 }⏎}` -> not reported; `new C⏎        {   P = 1,⏎            Q = 2⏎        };` -> `P = 1,` on the brace line not reported; `Inner =⏎{⏎    L = {⏎      1⏎        }⏎}` -> nested collection not checked. Expected: reported (the rule's own construct). Done when: new diagnostics with fix where the fix is safe; docs.

### N13 [behavior change + bug] RH5305 and RH5306 check the whole collection expression / list pattern (P11, P13)
Questions: #1215 Q1, #1215 Q3, #1216 Q1, #1216 Q2, #1216 Q3, #1216 Q4. Req: RH5305-002, RH5306-001/002/004, FMT-INDENT-025, FMT-BREAK-013/022. Blocked by: #1161, #1139.
Body: `int[] f = [⏎      1,⏎                2⏎          ];` element columns not checked; `[⏎    N(1,⏎      2), 3⏎]` -> `3` on the last line of a multi-line element not reported; `v is [{ Length: 1,⏎ Rank: 1 }, "b",⏎ "c"]` not reported; RH5306 exempts patterns with a comment/directive in front of `[` or behind `]` (remove the guard; `v is // c⏎[…]` stays reported); `v is [1,⏎ /// doc⏎ 2]` -> fix inserts a blank line before `///` while `//` and `/* */` are exempt (P13: alike). Done when: all inputs.

### N14 [behavior change] RH5302 and RH5308 report an operator alone on its line (P11)
Questions: #1212 Q2, #1218 Q3. Req: RH5302-002, RH5308-002, FMT-BREAK-017/018. Blocked by: #1243 (formatter join), #1161.
Body: `a⏎        &&⏎        b` and `a⏎        ?⏎        1⏎        : 2` (operators in column) -> not reported; the formatter joins them. Expected: reported with fix; the join must not insert a blank line (#1243). Done when: both rules.

### N15 [bug] RH5308 fix converges
Questions: #1218 Q2. Req: RH5308-001/004, SHARED-FIX-001/005. Blocked by: none.
Body: `a ? // c⏎        1⏎        : 2` -> fix puts `1` in column 8, `?` still behind `a`, still reported, second application changes nothing. Expected: one application clears the diagnostic or the fix is withheld (P7). Done when: tested.

### N16 [behavior change] Empty-type rules RH5412-RH5416: `};` and directives in the body (P11, P22)
Questions: #1169 Q2, #1169 Q3. Req: SHARED-EMPTY-001/004, FMT-STRUCT-017, SHARED-TRAIL-002, SHARED-DIAG-008. Blocked by: N01, #1240.
Body: `class C⏎{⏎};` -> not reported (any `;` after `}` treated as semicolon declaration); expected: reported, fix gives `class C;` and removes the stray `;`. Body with `#if NEVER ... #endif` member or `#if DEBUG⏎#endif` -> not reported (P22); `#region R⏎#endregion` and `#pragma warning disable` -> still reported, no fix. The RH5416 test `VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix` changes for the conditional case.

### N17 [behavior change + bug] RH5404 reports every one-line type body and has a location
Questions: #1221 Q1, #1221 Q2. Req: RH5404-001/002/003, FMT-BREAK-002/007, SHARED-DIAG-007. Blocked by: N01.
Body: `extension(int value) { public int Doubled => value * 2; }` -> diagnostic with `Location.None`, fix formats the type holding the file's first token. Expected: reported at the `extension` keyword, block formatted (test `VerifySingleLineExtensionBlockIsFlagged` changes). `class J { int _y; int _z;⏎}` (braces on different lines) -> not reported; expected: reported (P11).

### N18 [bug + clarification] Fix guards: RH5307 reported list, `?[ ]` not covered; RH5401 attribute line
Questions: #1217 Q1, #1217 Q3, #1219 Q1. Req: RH5307-001/004, FMT-BREAK-014, RH5401-002/003/004. Blocked by: none. Precedents: #1116 Q2, #1145.
Body: RH5307 `var a = x[m[1,⏎ 2]];`, `j[0 /* c */][1,⏎ 2]` -> reported, no fix (the fix resolves the list from the receiver, not the reported list); expected: resolve by the diagnostic span. `c?[1,⏎ 2]` stays not covered; state it in the spec and page (clarification). RH5401 `[Obsolete]⏎int E⏎    => 1;` -> reported, no fix (guard wants the whole declaration on one line); expected: judge the property's own lines.

### N19 [behavior change] RH3002 and RH3003 report the constructs they name (P11)
Questions: #1224 Q1, #1225 Q1. Req: RH3002-001/002, RH3003-001/002. Blocked by: #1161. Related: #877, #1245 Q3.
Body: `[Obsolete(("x"))]`, `if ((c))`, `while ((c))`, `x => (x)`, `a switch { _ => (a) }`, `a + (a)`, `(typeof(int)).Name` -> none reported; expected: reported. `case var x when x == "":` -> not reported while the same `""` in a switch-arm `when` is reported; expected: exempt the pattern only, not the `when`.

### N20 [bug] RH3004 fix keeps `static`, keeps the layout, parenthesizes the lambda
Questions: #1226 Q1, Q2, Q3. Req: RH3004-003. Blocked by: none (Q2 is P10 via #1138).
Body: `static delegate (int x) { return x; }` -> `(int x) => x` (drops `static`; expected `static (int x) => x`; check `async`); `= delegate (int x)` + `{` on the next line -> `= (int x) => {` loses the line break and indent; `(Func<int,int>)delegate (int x) { return x + 1; }` -> `(Func<int,int>)(int x) => x + 1`, `delegate () { return 1; } + h` -> `() => 1 + h` (CS0019), `g + delegate () {}` -> `g + () => { }`; expected: parenthesize or withhold.

### N21 [bug / behavior change] RH3201 empty comment separator
Questions: #1235 Q1, Q5, Q6. Req: RH3201-001/002. Blocked by: #1139 (P13), #1161.
Body: empty `//` between `/* First. */` and `// Second.`; empty `/* */` line between two `//` lines -> reported and removed (P13: kinds alike); `// First.` / `//` / `/// Second.` in a method -> reported (a `///` that documents nothing is an ordinary comment, P6); `int _a; //` trailing empty comment not reported (P11: only a stand-alone empty comment is a paragraph break).

### N22 [bug] Fixes that break the code or change what runs: RH3005, RH3101, RH3103, RH3106, RH3201 (P1, P23)
Questions: #1227 Q1, #1229 Q1, #1231 Q1, #1234 Q1, #1235 Q4. Req: RH3005-004/005, RH3101-001/004, RH3103-001/004, RH3106-001, SHARED-PARENS-002, RH3201-005, SHARED-FIX-003. Blocked by: none. Related: #1257, #1258.
Body: `if (1 == 1 == b)` -> `b == 1 == 1` (CS0019); `base.M()` with virtual `M` in an unsealed type -> `M()` becomes a virtual call (page example RH3101.md changes); `typeof(Nullable<>)` -> `typeof(?)`, `Nullable<int>.Equals(1,2)` -> `int?.Equals(1,2)`, `using N = System.Nullable<int>;` -> CS9058; `Run(delegate () { })` with two delegate overloads -> `delegate { }` (CS0121); `var a = delegate () { }; a();` -> `delegate { }` (CS8917); `return/**/a;` -> `returna;`. Expected: report stays, fix withheld (P23).

### N23 [bug] Fixes delete comment text: RH3101, RH3104, RH3105, RH3106, RH3107
Questions: #1229 Q2, #1232 Q1, #1233 Q1, #1238 Q1. Req: RH3101-004, RH3104-004, RH3105-004, SHARED-PARENS-002, SHARED-FIX-003. Related: #1136, #1259.
Body: `base /* a */ . /* b */ M()` -> `M()`; `new int(/* x */)` -> `default(int)`; `this /* a */ . /* b */ _a` -> `_a`; `delegate ( /* none */ ) { }` -> `delegate  { }`; `[Obsolete(/* none */)]` -> `[Obsolete]`. Expected: fix withheld.

### N24 [bug] RH3006 fix keeps the document's line ending (P24)
Questions: #1228 Q1. Req: RH3006-003, SHARED-FIX-008. Blocked by: #1138.
Body: CRLF document, `return a ||` / `/* x */ b && c;` -> fixed correctly but one LF in a CRLF file. Expected: CRLF only. Add a CRLF check to the fix test base.

### N25 [bug] Expression-body fixes format only the replaced body (P14d): RH3202, RH3203, RH3205-RH3208
Questions: #1236 Q1, #1237 Q1. Req: RH3202-005, SHARED-BODY-003, SHARED-FIX-007. Blocked by: #1138.
Body: `///<summary>x</summary>` + `[Obsolete] public  C( int value ) => _value = value;` -> doc comment rewritten to three lines, attribute on its own line, signature spacing fixed. Expected: only `=> ...;` becomes the block; two blank lines above a get-only method stay two. SHARED-BODY-003 ("converts the body as the formatter does") is reworded to the node.

### N26 [bug] RH3001 fix guard and patterns
Questions: #1245 Q2, #1245 Q2 (continuation). Req: RH3001-001/004. Blocked by: none (Q3 later).
Body: `M(!x)`, `(!x, y)`, `new C(!y)`, `this[!y]`, `[A(!true)]`, `[!x, y]` -> reported, no fix by accident (`FindNode` returns the `ArgumentSyntax`); `M(x: !y)` is fixed. Expected: the fix is offered for all. `x is !true`, `!true => 1` -> reported, no fix (P23).

### N27 [bug] RH2001 fix is withheld where it is unsafe
Questions: #1251 Q1, Q2, Q3, Q6. Req: RH2001-004, SHARED-FIX-003, SHARED-FIX-001. Blocked by: none.
Body: `Value /* name */ { get; /* inner */ set; } // trailing` -> comments dropped; property used only in `#if NEVER` code -> inactive `return Value;` stays; mutable struct property `Counter Current {get;set;}` -> field, `Current.Increment()` now mutates the field; `private readonly int Value { get; }` -> `private readonly readonly int _value;` (CS1004). Expected: withheld / correct.

### N28 [bug] Partial declarations are one declaration (P15): RH2002, RH2004, RH2101
Questions: #1252 Q2, #1254 Q4, #1254 Q5, #1255 Q1. Req: RH2002-001/005, RH2004-001/002/006, RH2101-002, SHARED-NESTED-002, SHARED-DIAG-004. Blocked by: N01. Note: changes the merged RH2004-002 (both parts of a partial property/indexer/event reported).
Body: `partial void Q();` + `async partial void Q() {…}` -> reported twice; `static partial class Helpers` + nested `partial class Helpers` -> second part reported; `partial P(int x);` not reported but the implementing part is; RH2004 fix on one part of a partial property -> CS8799. Expected: decided on the symbol, reported once, fix on one part withheld.

### N29 [bug] RH2004 fix on explicit interface operators
Questions: #1254 Q3 (+ 7b #1254 Q1, Q2). Req: RH2004-001/003/006. Blocked by: none.
Body: fix on `static N IAdd<N>.operator +(…)` -> `private static N IAdd<N>.operator +` (CS0106). Expected: withheld (moot if Q2 exempts them). Q1 (`static S operator -` gets `private`, CS0558) joins here after 7b.

### N30 [bug] RH2109 reads Razor correctly (P5, P5 extension)
Questions: #1256 Q1, Q2, Q3. Req: RH2109-001, SHARED-DIAG-003. Blocked by: N01.
Body: `@code {` in a C# comment, string, raw string inside a code block -> reported (4 hits); `<!-- @code { -->`, `@* @code { *@` -> reported; `<p>@@code { }</p>`, `user@code{` -> reported. Expected: none reported. Q4 (`@functions`) after 7b.

### N31 [behavior change (extension)] Argument values are judged through parentheses, `!` and `throw` (P16)
Questions: #1247 Q4, #1247 Q5, #1248 Q1 (+ #1247 Q6, #1250 Q1 after 7b). Req: SHARED-EQUAL-002, RH1002-001, RH1001-003, SHARED-DIAG-005. Blocked by: N01. Maintainer: this is an extension, not a bug fix.
Body: `Distinct(flag ? null : throw …)` -> not reported, `flag ? null : Default` reported; `Distinct((flag ? null : Default))`, `…)!` -> not reported; `Dictionary<S,int> a = flag ? new(_comparer) : throw …` -> declared type reported. Expected: parentheses/`!` transparent, a `throw` arm yields no value. RH1001 `new(comparer: flag ? null : throw …)` likewise.

## 5. Risks

- R1 Duplicates not fully verified. The full open list was read for #1133-#1259 (page 1 of 2, 100 of 197 titles); older open issues (#948-#1132) were checked only by the state.md list. Three searches (partial declarations, line-ending drift, RH2001 fix) found no match for N28, N24, N27. Before filing, spot-check: N12 against #1244, N14 against #1243, N16 against #1240, N19 against #877, N22 against #1257/#1258, N23 against #1259, N03/N04 against #954/#1151/#1136, N25 against #1138/#1140.
- R2 SHARED-FIX-006/-007 do not exist in `sdd/specs/shared/code-fixes.md` (it ends at SHARED-FIX-005); the squad issues cite them as if they did. They arrive with #1138, so every P9/P10/P14/P24-based bug is blocked by #1138.
- R3 Conflicts with merged requirements: P14d vs SHARED-BODY-003 and the SHARED-FIX-003 layout example (RH3202) and tests pinning them (reword); P14a vs three tests pinning the inserted blank line (change); P22 vs the RH5416 test and SHARED-TRAIL-002 text; P15 vs RH2004-002 (both parts reported today); P16 widens SHARED-EQUAL-002 (new diagnostics, an extension); item 13 retires the case-insensitive part of RH8107-001; P17 vs SHARED-TYPEREGION-006 (enum counted today); P3 extension contradicts SHARED-DIAG-002 ("an alias is a different name"); #1229 Q1 changes the RH3101.md example.
- R4 #1175 Q4 third case (`}` followed directly by `M(a);` after a removed region): P14c cleans blank lines the removal leaves but P14a forbids inserting one; the formatter wants a blank line after `}`. Plan: leave it (nothing inserted); confirm with the maintainer when N04 runs.
- R5 P23 vs the state.md candidate: item 14 reports even code no correction can replace (`var a = delegate () {}; a();` is reported as a false positive without fix); the earlier candidate "do not report" is dropped.
- R6 Mapping assumptions the maintainer did not state: #1176 Q1 analyzer part to P12 (#1147); #1175 Q1 optional "also report the type-level partner" not requested and not planned; #1245 Q3 split (P7 part vs positions RH3002 does not check).
- R7 #1185 Q1 is closed here by the AGENTS.md "prefer no fix" rule (withhold, comments not carried). If the maintainer wants the comments carried with their block, that becomes a later extension of N03.
- R8 #1199 Q1 rests on the #1129 Q1 precedent (presence-only exemption); #1201 Q1 whitespace-only content assumes "content verbatim" (P9); both worth one line of confirmation.
- R9 Ordering: N01 and #1138 first; then N05-N09, N11, N16, N17, N28, N30, N31 (need N01), N04/N10/N24/N25 (need #1138); N12-N14, N19, N21 need #1161/#1139; N14 needs #1243; #1143 (Fix All one application) audits the same fix classes after #1140-#1142.
- R10 Closing the 45 question issues needs the follow-ups to exist first (working agreement); no question issue is closed or ticked before section 0's extension comments and N01-N31 are filed.
