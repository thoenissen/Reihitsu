# Phase 5 — round 2 draft: spec contradictions (J) and remaining doubts (M)

Material for the coordinator's maintainer message. Inputs: `phase5-consolidated.md` §J/§M, `phase5-f1.md` … `phase5-f8.md`, `sdd/specs/` on this branch, `sdd/README.md`, `AGENTS.md`, `state.md` (decision log), and the bodies of #1074, #1136, #1140, #1262, #1334, #1385, #1393, #1398, #1416, #957, #966, #862 (read on 2026-10-24). In the inputs below, `⏎` is a line break and `→` is a tab.

Decisions already taken and not asked again: C issues are bugs in the bug lane with no change doc; D issues are extensions in vNext; E/F are Maintenance; the splits are done; #1384, #1348, #965 and #1146 Q4 are not critical.

## 0. Summary

| Result | Items |
|---|---|
| **Real contradiction** (two requirements give different outputs for the same input) | J1, J2 (at the edge only), J3, J4, J5, J6, J8, J9, J11, J12, J13, J14, J19 — **13** |
| **No contradiction** (a coverage sentence leaves the form out, or the texts agree when read exactly; the shared requirement decides) | J7, J10, J15, J16, J17, J20 — **6** |
| **Not requirement vs requirement** (the spec omits behavior that the code and the rule page have) | J18 — **1** |

A finding that matters more than the count: **for 12 of the 13 real contradictions, the maintainer has already decided the behavior**, in a principle round recorded in `state.md` and in the issue's own Decision section. The newer text records that decision, and the older text was not aligned when it was recorded. What is still open for these is the lane (bug fix that aligns the older text, or a change doc), not the behavior. Only J19 needs a real scope answer, and only J18 needs a direction.

| Group | Pattern | Items | Answer with |
|---|---|---|---|
| **G1** | Older text lags behind a newer requirement that records a maintainer decision | J1, J2, J3, J4, J5, J6, J8, J9, J11, J12, J13, J14 | one letter |
| **G2** | No contradiction: the rule's text restricts its own coverage or says nothing, and a shared requirement that binds the rule decides the case | J7, J10, J15, J16, J17, J20 | one letter |
| **G3** | Scope of SHARED-COMMENT-012: only RH5203 lists it under `Uses` | J19 (and the multi-line part of J16) | one letter |
| **G4** | RH6001-001 omits `fixed`, which the analyzer and the rule page report | J18 | one letter |

Remaining doubts from §M that still need an answer: **4** (§6, numbered M1–M4).

---

## 1. Group G1 — older text lags behind a recorded decision

**Pattern.** In each item, requirement X was written first. Requirement Y came later and records a maintainer principle, and its PR did not align X. The issue already names Y as its decision.

**Question G1.** For these 12 items, does the run fix the code to the newer requirement and align the older text in the same bug-lane PR, with no change doc?

- **G1-A** — Yes. Bug lane, no change doc. The older requirement is reworded, or retired with a reason, in the same PR as a clarification. The shared-spec PR that recorded the principle (#1357, #1359, #1404, #1405, #1418) was the behavior decision, and a change doc now would record no new decision. Consequence: every listed issue runs in the bug pool, so #1393, #1398, #1416, #1334 (P25 part), #1387, #1337, #1266, #1262, #1345, #1417, #1388, #1074 and #1133 lose their "change doc required" lines.
- **G1-B** — One change doc per issue that rewords an older requirement. This follows the letter of `sdd/README.md` ("a requirement is … changed"). Consequence: about 12 extra change docs that only repeat principles that are already decided. These issues leave the bug lane, against the decision taken for the C issues.
- **G1-C** — One collective change doc, "align rule texts to principles P5–P39", in a docs-only PR before the bug runs. Then every issue runs as a plain bug. Consequence: one extra PR, and the bug runs wait for it.

**Recommendation: G1-A.** The intended behavior changed when the maintainer decided the principle and the shared spec recorded it. Leaving the older text behind was an omission of that PR, and closing it is a clarification. This matches the decision already taken for the C issues ("align lagging rule texts in the same PR").

### J1 — SHARED-FIX-006 vs RH5104-005 (blank line before a moved comment) — #1393, #1146

- `sdd/specs/shared/code-fixes.md:482`: "A fix MUST NOT insert a blank line before the node it changes, and MUST NOT change the blank lines between that node and its neighbours, unless those blank lines are the code its diagnostic is about."
- `sdd/specs/analyzer/RH5104.md:163`: "The code fix MUST insert a blank line above the moved comment when the line directly above it holds code."
- **Real.** RH5104 reports a comment's position, not a blank line, so the blank line is not "the code its diagnostic is about". RH5104-005 is from #1104 (2026-10-19). SHARED-FIX-006 is from #1357 (2026-10-22, P14a).
- **Already decided:** round 9 item 9, option (a), quoted in #1393: "the moved comment is the changed node's leading trivia, so no blank line is inserted before it; RH5104-005 changes".
- Example (RH5104): `M();⏎M(); // second` → today `M();⏎⏎// second⏎M();` → reading A (FIX-006): `M();⏎// second⏎M();` → reading B (RH5104-005): today's output.
- Consequence of A: RH5104-005 is reworded so that the fix inserts no blank line. RH5020 may then report the moved comment. SHARED-FIX-004's Why allows that ("Leaving another rule's diagnostic behind is outside this spec"), and the RH5020 fix does not undo RH5104. There is precedent for an exception (round 8 item 22a, RH7501), but the maintainer chose no exception here.

### J2 — SHARED-FIX-006 vs RH5111-004 / RH5401-003 ("format as the formatter formats it in place") — #1140

- `sdd/specs/shared/code-fixes.md:484`: "A fix MUST format only the node it replaces, with the whitespace of the lines it edits. It MUST NOT format the member, the statement, or the type that contains that node."
- `sdd/specs/analyzer/RH5111.md:109`: "the code fix MUST format it as the formatter formats it in place, which joins the `=` to the line of the target …"; `sdd/specs/analyzer/RH5401.md:67`: "the code fix MUST format the property as the formatter formats it in place …".
- **Real, at the edge only.** For the joined lines, both texts give the same result. They differ where formatting the whole construct in place reaches lines the fix does not edit. Examples are the construct's leading documentation comment, which a node-level format rewrites when the node starts its line (FMT-PIPE-010), and the spacing on the construct's other lines. Under P9 (round 5) the maintainer decided that a fix changes only what its diagnostic is about.
- Example (RH5111, from #1140): `/// <summary>P</summary>⏎int P { get; set; }⏎    = 5;` → today the declaration is joined and the doc comment is rewritten to three lines → A: `/// <summary>P</summary>⏎int P { get; set; } = 5;` → B (the literal reading of RH5111-004): today's output.
- Consequence of A: the two rule requirements say "lays out the lines it joins as the formatter does (FMT-BREAK-016 / FMT-BREAK-026)" instead of "formats it in place".

### J3 — FMT-SWITCH-004 vs FMT-SWITCH-006 (break-only section with a wrapping label) — #1398

- `sdd/specs/formatter/switch-case-braces.md:235`: "A section MUST count as multi-line when one of its labels contains a property (recursive), list, or parenthesized pattern that spans more than one line."
- `sdd/specs/formatter/switch-case-braces.md:371`: "A section whose only statements are `break` statements MUST be left unbraced, and it MUST NOT make the switch multi-line."
- **Real.** Both texts are from #947 and contradict each other inside one spec. The more specific one (-006, break-only) is the one the decision picks.
- **Already decided:** round 9 item 19, option (b): "FMT-SWITCH-006 wins over FMT-SWITCH-004" (#1398).
- Example: a single-line switch in which the only multi-line section is `case Point { X: 1,⏎ Y: 2 }:⏎ break;` → today the whole switch is braced → A: the switch stays unbraced → B (-004 wins): today's output.
- Consequence of A: FMT-SWITCH-004 gains the precedence sentence "a section whose only statements are `break` statements does not count (FMT-SWITCH-006)".

### J4 — SHARED-DIAG-003 vs RH5601-001 / RH5602-001 (whitespace next to a literal and in holes) — #1074, #1133

- `sdd/specs/shared/diagnostics.md:99`: "the whitespace before and after a literal is code, so the spaces at the end of the line `string s = "a";` are trailing whitespace like after any other token"; `:100`: "an interpolation hole is code apart from the text of its format clause".
- `sdd/specs/analyzer/RH5601.md:29-30`: "… the last code token before it on that line is not a string or character literal …" / "… the next code token after it is not a string or character literal."
- `sdd/specs/analyzer/RH5602.md:29`: "… and no string literal touches the line — a line that an interpolated string spans, its holes included, counts as touched."
- **Real.** RH5601/RH5602 are from #1036 (2026-10-18). SHARED-DIAG-003 is from #1099 (2026-10-19, P5).
- **Already decided:** P5. #1074 states "Bug fix against that requirement".
- Example (RH5601): `string s =→"abc";` → today not reported → A: the tab is reported and the fix gives `string s = "abc";` → B: not reported. For RH5602, `string s = "a";··` → today not reported → A: reported → B: not reported.
- Consequence of A: the literal exemptions in RH5601-001 and RH5602-001 are narrowed to the literal's own token. A tab or space inside a raw string's text, the closing-line indentation of #1133 included, stays exempt under RH5601-002 and SHARED-DIAG-003 ¶2, so the #1133 fix still withholds there.

### J5 — SHARED-FIX-007 vs SHARED-BRACE-002 (layout of the wrapped statement) — #1416, #1261

- `sdd/specs/shared/code-fixes.md:586`: "Code that a fix moves … the statements a fix wraps into a new block — MUST keep its own layout … Only the indentation of its lines MUST follow its new nesting. … the embedded statement of an `if`, `else`, or loop that a brace fix encloses."
- `sdd/specs/shared/statement-braces.md:46`: "A code fix that uses this requirement MUST wrap the reported body in a block by laying out the `if` statement that owns it as the formatter does (FMT-STRUCT-001)."
- **Real.** SHARED-FIX-007 names the brace-fix case explicitly and is newer (#1357/#1404, P26).
- **Already decided:** P26, round 10 (#1408 Q1 → #1416: "SHARED-BRACE-002 … is amended to match SHARED-FIX-007").
- Example (RH5405): `if (a)⏎    M(a,b);` → today `if (a)⏎{⏎    M(a, b);⏎}` → A: `if (a)⏎{⏎    M(a,b);⏎}` → B: today's output.
- Consequence of A: SHARED-BRACE-002 says that only the braces are laid out as the formatter lays them out and that the body keeps its layout. #1416 moves to the bug pool and stays sequenced after #1261.

### J6 — SHARED-FIX-008 vs RH5011-004 / RH8304-004 (lone `\r`) — #1388

- `sdd/specs/shared/code-fixes.md:840`: "… in a document whose line breaks are all a lone `\r`, every line break the fix creates or keeps in the code it changes MUST be `\n`."
- `sdd/specs/analyzer/RH5011.md:105`: "… insert one empty line directly below the line of the `break` statement, ending with the same line break as that line; in a document whose line breaks are all CRLF or all LF, that is the line ending SHARED-FIX-008 specifies."
- `sdd/specs/analyzer/RH8304.md:98`: "… insert, at that place, the line break of the line the element starts on … In a document whose line breaks are all CRLF or all LF, that line break is the line ending SHARED-FIX-008 specifies."
- **Real** for lone-`\r` and mixed documents. The lone-`\r` sentence is newer (P35, round 9; clarified in round 10).
- **Already decided:** round 10 (Q-1380-1 → P35, "RH5011-004/RH8304-004 join #1377"). #1377 merged without aligning these two texts.
- Example (RH5011, a document with lone `\r` breaks): `break;␍a++;` → today `break;␍␍a++;` → A: `break;␍\na++;` → B: today's output.
- Consequence of A: both rule requirements say "the line ending SHARED-FIX-008 specifies" without the CRLF/LF restriction.

### J8 — SHARED-DIAG-008 vs RH5102-004 / RH5109-004 / RH5111-005 ("still reported") — #1387, #1337

- `sdd/specs/shared/diagnostics.md:401`: "A rule MUST NOT report a construct when a conditional directive — `#if`, `#elif`, `#else`, or `#endif` — or code that such a directive excludes lies inside the span the rule judges …"
- `sdd/specs/analyzer/RH5102.md:125` (and with the same words `RH5109.md:107`, similar `RH5111.md:149`): "The code fix MUST NOT be offered when a comment or a preprocessor directive sits anywhere inside the argument list … The diagnostic is still reported."
- **Real** for conditional directives: the rule texts say "still reported" for any directive. SHARED-DIAG-008 is newer (P22, #1359).
- **Already decided:** P22, round 7a item 16.
- Example (RH5102): `M(a, b,⏎#if DEBUG⏎c,⏎#endif⏎d);` → today reported, without a fix → A: not reported → B: today's output.
- Consequence of A: the three rule requirements keep "still reported" for comments and for `#region`/`#pragma`, and point to SHARED-DIAG-008 for conditional directives.

### J9 — SHARED-USING-007 vs RH7203/RH7204/RH7206-001 ("directly follows") — #1266

- `sdd/specs/shared/using-directive-ordering.md:274`: "A rule that reports a directive out of this order MUST compare the directive with every earlier directive of its group in the block, whatever directives of other groups or kinds lie between them …"
- `sdd/specs/analyzer/RH7203.md:25` (and the same in `RH7204.md:25`, `RH7206.md:25`): "The analyzer MUST report every regular namespace directive that directly follows a regular namespace directive of the same group …"
- **Real.** The USING-007 ¶2 text is P18, clarified in round 8 (#1328), and newer.
- **Already decided:** P18 clarification, round 8a ("partner = every earlier element of the same group").
- Example (RH7203): `using Microsoft.B;⏎using Alpha.X;⏎using Microsoft.A;` → today not reported → A: `Microsoft.A` is reported → B: not reported.
- Consequence of A: the three rule texts say "follows, anywhere earlier in the block, a directive of the same group whose key is larger".

### J11 — SHARED-DIAG-009 vs SHARED-REGGROUP-006 ("the analyzed type declaration") — #1334

- `sdd/specs/shared/diagnostics.md:448`: "[a verdict MUST NOT depend on] which part of a `partial` type holds a declaration the verdict relates to, such as … an override that makes a region an override region".
- `sdd/specs/shared/member-region-grouping.md:378`: "… for an override that the analyzed type declaration declares, wherever that override lies … A region whose name matches no type that an override in any declaration of the type comes from MUST NOT count as an override region." The second sentence already says "any declaration", so REGGROUP-006 also contradicts itself.
- **Real.** SHARED-DIAG-009 is newer (P25, #1404).
- **Already decided:** P25, round 8. #1334: "resolved in favour of any declaration".
- Example (RH7405, from #1334 Q4): `partial class C : B { public override void M() { } }` + `partial class C { #region B⏎ public void N() { } #endregion }` → today `N` is not reported → A: `N` is reported → B: today's output.
- Consequence of A: the first sentence of REGGROUP-006 says "that any declaration of the type declares". The Q6 part of #1334 is a separate doubt (M4).

### J12 — SHARED-FIX-003 vs RH7309-004 (reorder despite a `///`) — #1262

- `sdd/specs/shared/code-fixes.md:159`: "A code fix MUST NOT be offered when its result would: … change which declarations a documentation comment documents."
- `sdd/specs/analyzer/RH7309.md:149`: "For a type declaration whose body holds no comment outside its top-level region blocks, the code fix MUST reorder all top-level region blocks of the declaration by category …"
- **Real** for the #1262 shapes in which the `///` lies inside the previous block: behind its last member (`} /// <summary>M</summary>`) or above its `#endregion`. RH7309-004's precondition does not exclude them, so it requires the reorder. The shape with the `///` directly above `#region Methods` lies outside the blocks and is already excluded. The FIX-003 condition is newer (P39, #1418).
- **Already decided:** P39, round 10 item 2.
- Example: `#region IDisposable⏎public void Dispose() { } /// <summary>M</summary>⏎#endregion⏎#region Methods⏎public void M() { }⏎#endregion` (documentation mode `Parse`) → today the fix moves `Methods` to the front, and `M` loses its documentation → A: no fix offered, the diagnostic stays → B: today's output.
- Consequence of A: RH7309-004 gains "and SHARED-FIX-003 allows it", or a withholding requirement RH7309-006 that names the `///` shapes.

### J13 — SHARED-DOC-004 ¶2 vs RH8303-001 / -005 ("starts its line") — #1345

- `sdd/specs/shared/element-documentation.md:213`: "A rule about the layout of documentation comments, such as RH8301, RH8302, or RH8303, MUST judge such a comment as if it started its own line. … except the RH8303 fix, which moves it onto a line of its own above the declaration."
- `sdd/specs/analyzer/RH8303.md:25`: "The analyzer MUST report a `///` or `/** */` documentation comment that starts its line — nothing but white space stands before it on that line — …"; `:127`: "The code fix MUST insert one empty line … directly above the line of the reported documentation comment, and change nothing else."
- **Real.** RH8303-004 (location on the `///`) is compatible and needs no change. SHARED-DOC-004 ¶2 is newer (P29, #1404).
- **Already decided:** P29, round 8, with the round 10 clarification for documentation mode `None`.
- Example (RH8303, mode `Parse`): `int a; /// <summary>B</summary>⏎int b;` → today not reported → A: reported, and the fix gives `int a;⏎⏎/// <summary>B</summary>⏎int b;` → B: not reported.
- Consequence of A: RH8303-001 adds the behind-code form, and RH8303-005 adds "or, for a comment behind code, moves it onto its own line above the declaration".

### J14 — SHARED-DIAG-011 vs RH3103-006 (dotted `nameof` argument still reported) — #1417

- `sdd/specs/shared/diagnostics.md:570`: "A rule MUST NOT report a construct at a position where the change the rule asks for cannot be written …"
- `sdd/specs/analyzer/RH3103.md:140`: "The code fix MUST NOT be offered when the reported generic name is the `Name` of a `MemberAccessExpressionSyntax`, such as a dotted `Nullable<T>` written as the argument of `nameof`. The diagnostic is still reported." The example `nameof(System.{|RH3103:Nullable<int>|})` is at `:151`.
- **Real**, but in RH3103-**006**, not in -001 as §J says. RH3103-001's `typeof` position needs one written type argument, so `typeof(Nullable<>)` is already outside it. The member-access receiver and `using static` are not positions of -001 at all (a coverage gap, decided by DIAG-011). SHARED-DIAG-011 is newer (P38, #1418).
- **Already decided:** P38, round 10 item 1.
- Example: `nameof(System.Nullable<int>)` → today reported, without a fix → A: not reported → B: today's output.
- Consequence of A: RH3103-006 is retired, with the reason "SHARED-DIAG-011". Its example moves to RH3103-002 as a case that is not reported.

---

## 2. Group G2 — no contradiction: a shared requirement decides what the rule text leaves open

**Pattern.** The rule's or the specific spec's text names the forms it covers ("with one declarator", "without `@`") or says nothing about a case. It does not say that the other forms are not reported. A shared requirement that binds the rule decides those forms. That is SHARED-DIAG/SHARED-FIX for every rule (`sdd/README.md`: "apply to every rule and every code fix without being listed under `Uses`"), or a shared requirement the rule lists under `Uses`. The code violates the shared requirement.

**Question G2.** Are these bugs against the shared requirement, with the rule's coverage sentence widened as a clarification in the same PR?

- **G2-A** — Yes. Bug lane, no change doc. Consequence: #1333, #1268, #1071, #1092 Q3b, #1284, #1147 and the fix part of #1385 run as bugs, and each PR widens the rule's coverage sentence.
- **G2-B** — Treat each as a widening of the rule (a P11 own gap that adds diagnostics), with a change doc. Consequence: these issues move to vNext, and the shared requirements stay unenforced for those rules until then.

**Recommendation: G2-A.** By `sdd/README.md` the shared specs bind every rule, and "Claim only what is verified" makes a coverage sentence a statement of reach, not an exclusion. A form the rule leaves out is therefore decided by the shared requirement, and the code that disagrees with it is a bug.

### J7 — SHARED-FIX-006 vs SHARED-BODY-003 — #1284

- `sdd/specs/shared/expression-body-conversion.md:105`: "The code fix MUST replace the expression body and its semicolon with exactly the block body that FMT-STRUCT-004 or FMT-STRUCT-005 produces, … laid out as the formatter lays it out."
- `sdd/specs/shared/code-fixes.md:484` (quoted under J2).
- **Not real.** "Laid out as the formatter lays it out" applies to the block body that replaces the expression body and its semicolon. BODY-003 says nothing about the signature. SHARED-FIX-006's own RH3203 example (`code-fixes.md:551-580`) shows `[Obsolete] public  C( int value )` kept unchanged. P14d (round 7a item 4) decided the same.
- Example (RH3202): `public int  GetValue( ) => _value;` → today `public int GetValue()⏎{⏎    return _value;⏎}` → required: `public int  GetValue( )⏎{⏎    return _value;⏎}`.
- Consequence of A: #1284 is a bug. Optional clarification of BODY-003: "the block body … is laid out as the formatter lays it out; the rest of the member stays as SHARED-FIX-006 states."

### J10 — SHARED-DIAG-006 vs SHARED-REGGROUP-005 / RH7403/07/11-001 ("one declarator") — #1333

- `sdd/specs/shared/diagnostics.md:342`: "A rule that reports individual declarators MUST report one diagnostic per reported declarator, located on that declarator's identifier …"
- `sdd/specs/shared/member-region-grouping.md:348`: "… for a field-like event with one declarator the identifier of that declarator." `sdd/specs/analyzer/RH7407.md:25`: "… each event declaration — a field-like event with one declarator and an event with `add` and `remove` accessors alike — …"
- **Not real.** The rule texts name the forms they cover and do not say that a multi-declarator event is not reported. SHARED-DIAG-006 decides it. The verdict can differ per declarator: in `event EventHandler F, G;`, `F` can implement `IA.F` and `G` can implement `IB.G`. So these rules "report individual declarators": one diagnostic each.
- Example (RH7407): in `class C : B`, inside `#region B` next to an override event, `public event EventHandler F, G;` → today one diagnostic, at `F` → required: one at `F` and one at `G`.
- Consequence of A: REGGROUP-005 and the three rule texts say "for a field-like event, each declarator's identifier".

### J15 — SHARED-DIAG-002 vs RH2003-001, RH4001-002/-005, RH4009-002 (alias, `@`, trivia) — #1268, #1071

- `sdd/specs/shared/diagnostics.md:48`: "… an identifier without its verbatim prefix `@`, and a qualified name without the whitespace and comments … A name that refers to a symbol through a `using` alias MUST be resolved to the symbol the alias names …"
- `sdd/specs/analyzer/RH2003.md:26`: "… whose created type is `System.NotImplementedException`, … however its type name is qualified." `RH4001.md:76`: "For a type whose name is written without the verbatim prefix `@`, …". `RH4001.md:116`: "… and a type name written without `@`, …". `RH4009.md:42`: "… with a name written as identifiers joined by dots, without `@`, whitespace, or comments, …".
- **Not real.** Each is a coverage restriction ("for a name written without …"), not a "MUST NOT report". SHARED-DIAG-002 applies to every rule.
- Example (RH2003): `using NIE = System.NotImplementedException;` … `throw new NIE();` → today not reported → required: reported. (RH4001: `class @class` in `class.cs` → today reported, and the fix renames the file to `@class.cs` → required: not reported.)
- Consequence of A: the four coverage sentences drop the restriction or point to SHARED-DIAG-002.

### J16 — SHARED-COMMENT-011 vs SHARED-INIT-001 (one-comment limit) — #1147, #1382

- `sdd/specs/shared/comment-placement.md:420`: "A rule that checks where a line starts … MUST take a comment of any kind that starts its line (SHARED-BLANK-002) as the first element of that line."
- `sdd/specs/shared/initializer-layout.md:21`: "A rule that uses this requirement … MUST check a token in front of which at most one comment starts on the token's own line as follows …"
- **Not real** for the one-comment limit. INIT-001 states its reach for a single comment, and COMMENT-011 decides the two-comment form. RH5301, RH5303 and RH5304 list both under `Uses`. The multi-line part of INIT-001 (bullet 4: "When a comment ends on the token's line, the token itself MUST NOT be checked", example `:50-51`) is a separate question: it conflicts only if SHARED-COMMENT-012 binds these rules, so it belongs to G3.
- Example (RH5301): `new C⏎{⏎/* a */ /* b */ P = 1,⏎}`, with `/* a */` at column 0 instead of the member column → INIT-001 does not cover it → COMMENT-011: the line is out of place and reported. Today it is not reported (#1147; reproduce).
- Consequence of A: INIT-001 bullet 2 reads "the first comment that starts on the token's line …", and the coverage sentence is widened.

### J17 — SHARED-DIAG-004 vs SHARED-DOC-002/-005 coverage and RH2004-002 (partials) — #1092, #1091, #1287

- `sdd/specs/shared/diagnostics.md:211`: "A rule that judges a type or a member declared in several parts with the `partial` modifier as a whole … MUST judge it by its symbol … and MUST report it at most once. A code fix that would have to change more than one part … MUST NOT be offered."
- `sdd/specs/shared/element-documentation.md:59`: "This requirement covers every declaration except … a partial type declared in several parts." `:356`: "A partial element declared in several parts … is not covered." `sdd/specs/analyzer/RH2004.md:84`: "The analyzer MUST NOT report a type declaration that carries the `partial` modifier, in any of its parts, and MUST NOT report a method that carries the `partial` modifier."
- **Not real.** DOC-002 and DOC-005 are coverage sentences, so DIAG-004 decides. RH2004-002 is a rule that chooses not to report partial types and methods, which DIAG-004 allows: it governs how a rule judges partials, not whether. Partial properties are outside RH2004-002, so the CS8799 fix in #1287 is a bug under DIAG-004 ¶1 (second sentence) and SHARED-FIX-003.
- Example (#1092 Q3b): `public partial class V { }` + `partial class V { }` → today the second part is judged as private (RH8002) → required: one public type, judged by RH8001, at most once.
- Consequence of A: DOC-002 says "a partial type declared in several parts takes its group from its symbol (SHARED-DIAG-004)". RH2004-002 stays. Extending RH2004-002 to partial properties remains #1287's change doc (D).

### J20 — FMT-BLANK-016 vs SHARED-STMT-004 (blank line and directives) — #1385

- `sdd/specs/formatter/blank-lines.md:975`: "… it MUST insert the blank line after the last directive of the gap, in front of the comments that stand between that directive and the code behind it, and it MUST NOT insert a blank line directly in front of a directive."
- `sdd/specs/shared/statement-blank-lines.md:116`: "The code fix MUST make exactly one blank line appear in front of the reported statement. When comments sit directly above the statement on their own lines, the blank line MUST go above the first of them."
- **Not real.** STMT-004 already puts the blank line directly above the statement or its comments, which is behind any directive. The two texts give the same output. #1385 itself says that "the RH5002/RH5008 fixes already violate SHARED-STMT-004". P37 (round 9 item 6) decided both sides.
- Example (RH5002): `a++;⏎#if DEBUG⏎a--;⏎#endif⏎// why⏎if (a > 0) { }` → today the blank line goes above `#if` → required: above `// why`, after `#endif`.
- Consequence of A: the fix part of #1385 joins its formatter part in the bug lane (as for C). Optionally, STMT-004 gains a directive example as a clarification.

---

## 3. Group G3 — scope of SHARED-COMMENT-012 (J19, and the multi-line part of J16)

### J19 — SHARED-COMMENT-012 is listed under `Uses` only by RH5203 — #888, #1382

- `sdd/specs/shared/comment-placement.md:518`: "a rule that checks where a line starts — whether a token starts its line, or in which column a line starts — MUST NOT take the comment's last line for a line of its own. … the rule MUST report the token, whatever column it reaches, because the token does not start its line". `:517`: "where a requirement of the formatter asks that token to start its own line, the formatter MUST break the line in front of the token".
- `sdd/specs/shared/initializer-layout.md:26`: "When a comment ends on the token's line, the token itself MUST NOT be checked: it may follow the comment anywhere on that line." Its example at `:47-51`, `P = 1⏎/* last⏎   one */ };`, is not reported.
- `Uses` today: SHARED-COMMENT-012 is listed only by `RH5203.md`. SHARED-COMMENT-011 is listed by 14 line-start rules. No formatter spec lists SHARED-COMMENT-012.
- **Real, if COMMENT-012 binds the other rules.** Its wording addresses every line-start rule and the formatter. `sdd/README.md` lets a shared spec outside diagnostics/code-fixes bind only through `Uses`. P33 (round 9 item 2) decided: "formatter breaks before it, rules report (SHARED-INIT-001 + 2 formatter tests change)", so the maintainer expected INIT-001 to change. The split of #1382 moved its RH5204 and INIT-001 parts to D.
- Example (RH5301, the INIT-001 example): `var d = new C⏎{⏎    P = 1⏎    /* last⏎       one */ };` → today not reported, and the formatter keeps `one */ };` → A: RH5301 reports `}`, and the formatter breaks before `}` → B: the formatter breaks before `}` (its bullet binds the formatter by its own words), and RH5301 stays silent until the D part ships.

**Question G3.** Does SHARED-COMMENT-012 bind every rule that checks line starts (RH5201, RH5204, RH5301–RH5308 and the rest of the COMMENT-011 users) and the formatter now?

- **G3-A** — Yes, now. The `Uses` lines of the COMMENT-011 users and of the formatter line-break and indentation specs gain SHARED-COMMENT-012 as a clarification. INIT-001 bullet 4 is amended for a comment that starts on an earlier line. The RH5204 and INIT-001 parts of #1382 return from D to the bug pool, and #888 is a formatter bug (the RH5201 report is correct).
- **G3-B** — The formatter and RH5203 now (as `Uses` stands). Every other rule follows through the split-off D part of #1382, with a change doc (it adds diagnostics). #888 is fixed by the formatter half alone: once the formatter breaks before `.D`, the RH5201 fix that delegates to it makes progress.

**Recommendation: G3-A.** P33 already decided that "rules report", and COMMENT-012's general wording is that decision. The missing `Uses` lines are the same omission that G1 aligns. If the maintainer wants to keep the split exactly as done, B is safe: it changes no output that A would not, it only delays the rule half.

---

## 4. Group G4 — RH6001 and `fixed` (J18) — #957

- `sdd/specs/analyzer/RH6001.md:27`: "The analyzer MUST report the keywords `if`, `while` …, `for`, `foreach`, `switch`, `catch`, `lock`, `using`, `return`, and `throw` when the keyword is followed by `(` …" (no `fixed`).
- `documentation/rules/RH6001.md:12`: "The rule covers `if`, `while`, `for`, `foreach`, `switch`, `catch`, `fixed`, `lock`, `using`, `return`, and `throw`." The analyzer registers `SyntaxKind.FixedKeyword` (`RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.cs:56`).
- `sdd/specs/formatter/horizontal-spacing.md:434` (FMT-SPACE-012) omits `fixed` too, so formatted `fixed(` code is reported by RH6001.
- **Not requirement vs requirement.** The spec omits behavior that exists in the code. The analyzer–formatter disagreement is the direction P11 does not cover (the analyzer reports more), so it is an open question. #957's formatter half is already placed in D (vNext).
- Example: `fixed(int* p = arr) { }` → today RH6001 reports it, the RH6001 fix gives `fixed (…)`, and the formatter leaves `fixed(` → A: the same now, and after #957 the formatter writes `fixed (` → B: not reported and the formatter unchanged.

**Question G4.** Does RH6001-001 keep `fixed`?

- **G4-A** — Yes. RH6001-001 gains `fixed` as a clarification (Maintenance, spec text only), and the formatter follows with #957 in vNext. Until then, formatted `fixed(` code raises RH6001, and its own fix corrects it.
- **G4-B** — No. Remove `fixed` from the analyzer and the rule page (a behavior change with a change doc that removes diagnostics). Then #957 part 1 is moot.

**Recommendation: G4-A.** The rule page and the code agree, and the maintainer decided pre-SDD (#920) that `fixed` takes the space, so only the spec text lags.

---

## 5. §M doubts dropped, with reason

| Doubt | Why dropped |
|---|---|
| C issues bug lane; pre-SDD "decided as bug" (#960 #961 #962 #955 #971 #973 #1084 #1085 #975 #1077 #1080) | Decided (C bug lane; D vNext) |
| #1277 guard widening; #1341/#1077 P13 change doc | D, decided |
| COMMENT-012 scope; #888 rewrite/merge | Covered by G3; the merge is bookkeeping |
| #1243 no requirement ID | Clarification plus fix either way; no change doc in either reading |
| #1094 Q3/Q4 | SHARED-FIX-007 binds every fix without `Uses` (`sdd/README.md`), so they are bugs |
| #1343 `&amp;`; #1241 split | The bug rests on SHARED-FIX-001 / SHARED-FIX-003 regardless of the reading |
| #1298 `const` instance; #1261, #1412, #965, #1076, #1383, #1055, #867 | The squad's reproduction settles them |
| #1393 RH5417 part | Decided, round 9 item 11 (#1393) |
| #1291 struct part; criticality; milestones; splits; #951 order | Scheduling, or decided |
| #862/#1034 home of `AttributeTargetUtilities` | Settled by `AGENTS.md` (Core holds Roslyn utilities shared by analyzers, fixes and formatter); internal |
| #1242, #1287 owner, #1132, #122, #851, #1300 | Process or bookkeeping |
| #1082 directives-only file; #968 scope | Decided inside their own change docs, which the maintainer approves |

## 6. Remaining doubts

### M1 — Is the SHARED-FIX-003 extension of #1136 a clarification or a behavior change?

**Context.** #1136 (E, Maintenance) asks SHARED-FIX-003 to withhold a fix that relocates or reorders a comment or directive it does not own, or that drops, edits or relocates disabled text. `AGENTS.md` (Code-fix coverage) already states this policy: "A rewrite that would drop, reorder, or relocate user-authored comments, directives, disabled text, or literals must withhold itself". The maintainer decided it on 2026-10-19 (`squad/decisions/…agents-sentence…`, answer 1; `…audit-repair…`, answer 3). SHARED-FIX-003 as written (`code-fixes.md:153-159`) covers only changed text, not relocation. #1136 asks for "a covering test that passes today", but today's fixes fail it: #1332, #1336 (critical, A) and #1146 Q4.

Example (RH5104, #1146 Q4): `/* a */ /* b */ M();` → today `/* b */⏎/* a */⏎M();` → under the extension: no fix offered, the diagnostic stays.

**Question.** Does #1136 run as a clarification with no change doc, so that the fixes that violate it are bugs?

- **A** — A clarification of a policy that `AGENTS.md` already states. No change doc. The covering test uses a fix that already complies, and the failing fixes are bugs in their own issues (#1332, #1336, #1146 Q4). #1136 should run before those relocation instances.
- **B** — A behavior change with a change doc (vNext). The relocation instances of #1332 and #1336 then wait for it, while the instances that break the build stay bugs under the current conditions.

**Recommendation: A.** The intended behavior is already decided and written in the repository instructions, so the spec only lags. B would delay two critical issues for a decision that already exists.

### M2 — Does FMT-PIPE-014 "every assignment operator" include the `=` of an enum member (and of a parameter default)? — #1149

**Context.** `sdd/specs/formatter/pipeline.md:319` lists "every assignment operator: `=` and every compound assignment operator" as one family. FMT-BREAK-016 (`line-breaks.md:615`) names "an assignment, a variable declarator, or a property initializer". RH5111 reports enum members and parameters (`RH5111.md:149`, example `B =⏎ 2`) and delegates its fix to the formatter, which leaves enum members as written, so the fix makes no progress (SHARED-FIX-001).

Example: `enum E⏎{⏎    A =⏎        1,⏎}` → today the formatter keeps it, RH5111 reports it, and the fix changes nothing → A: the formatter gives `A = 1,` and the RH5111 fix converges → B: the formatter keeps it, and the RH5111 fix must join the line itself or be withheld (SHARED-FIX-005).

- **A** — Yes: the equals-value `=` of an enum member and of a parameter default is in the family. #1149 is a bug against FMT-PIPE-014 (bug pool), and FMT-BREAK-016 gains the two forms as a clarification.
- **B** — No: only assignment expressions, declarators and property initializers. #1149 stays a vNext extension, and RH5111's fix on enum members is withheld until then.

**Recommendation: A.** The `=` is the same equals-value token in all of these, RH5111 already treats them as one shape, and P34's "one treatment per syntax family" exists to stop exactly this kind of omission.

### M3 — Is `global::` part of the SHARED-USING-007 sort key? — #966 case 2

**Context.** `sdd/specs/shared/using-directive-ordering.md:272`: "the alias name for an alias directive, otherwise the imported namespace or type name". The group key strips `global::` (`:127`), and the sort key does not, so every `global::` directive sorts by `g`. The maintainer decided pre-SDD (#916) that this is a bug. SHARED-DIAG-002 compares a name by its value, and `global::System.Text` names the same namespace as `System.Text`.

Example: `using System.Linq;⏎using global::System.Text;⏎using System;` → today the formatter gives `using global::System.Text;⏎using System;⏎using System.Linq;` → A/B: `using System;⏎using System.Linq;⏎using global::System.Text;` → C: today's output.

- **A** — Strip `global::` from the sort key. Equal keys keep their source order (USING-007 as written). A bug, plus a clarification sentence in USING-007. Analyzer (RH7203) and formatter change together.
- **B** — As A, but an unqualified directive sorts before an otherwise equal `global::` one, as #966 asks. This adds a tie-break requirement (a small behavior change with a change doc).
- **C** — Keep the qualifier in the key. Close case 2, and record that the key is the name as written.

**Recommendation: A.** It follows from SHARED-DIAG-002 and from the group key, needs no new requirement, and the tie only arises for a duplicate using that CS0105 already flags.

### M4 — #1334 Q6 (both parts of a partial member in a matching region) goes beyond the specs

**Context.** RH7409-001 (`RH7409.md:25`) and the sibling rules report a member "that is not `partial`". #1334 Q6 asks that both parts of a partial member lie in a matching region, reported once at the first part that does not. That adds diagnostics for partial members, which the rules exclude today. Neither SHARED-DIAG-004 nor SHARED-DIAG-009 states it. The rest of #1334 is G1 (J11).

Example: `interface I { void M(); }`, `partial class C : I { #region Methods⏎ public partial void M(); #endregion #region I⏎ public partial void M() { } #endregion }` → today reported once at the defining part, and nothing when the placement is swapped → required by Q6: reported once, at the part in `Methods`, in both placements.

- **A** — Split Q6 off as a D issue (an extension in vNext, with a change doc for RH7401–RH7412 "partial members"). #1334 stays a bug (G1). Whether today's single report is a bug against RH7409-001 ("not `partial`") is left to the bug run's reproduction.
- **B** — Keep Q6 in #1334 and add a change doc to that PR.
- **C** — Treat Q6 as already covered by SHARED-DIAG-009 bullet 2 (a bug).

**Recommendation: A.** The rules exclude partial members explicitly, so Q6 widens them. That is the D pattern already decided, and it keeps #1334 a clean bug run.
