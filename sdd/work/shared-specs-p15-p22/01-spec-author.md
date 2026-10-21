Role: spec author
Commit: dc93d21
Verdict: spec written

Round 2. This applies the maintainer's answers recorded in `squad/decisions/2026-10-21-1260-p22-fix-reading.md`, `-item19-placement.md`, `-razor-content-no-conforming-shape.md`, and `-no-new-questions.md`:

- SHARED-DIAG-008 now has the reading-B fix sentence.
- Item 19 is folded into SHARED-TYPEREGION-006. SHARED-TYPEREGION-007 is removed from the spec and from the ledger.
- The Razor sentence of SHARED-DIAG-003 stays as written.
- Neither in-passing question is filed.

The lead prompt carried no intended fix or verdict that needed setting aside.

## Changed requirements

- SHARED-DIAG-002 — A name is compared by its value — changed (P3 extension). A name used through a `using` alias resolves to the symbol the alias names. New alias example (RH2003). `shared/diagnostics.md` never contained the sentence "an alias is a different name". The only alias statement in the specs is the coverage sentence of RH2003-001 ("… and not through a `using` alias"). It is outside this run's files and stays; thoenissen/Reihitsu#1268 tracks it.
- SHARED-DIAG-003 — Only the text of a literal or a comment is content — changed (P5 extension). New bullet: in a Razor file, the text of a Razor comment and of an HTML comment is comment text, `@@` is text, and an `@` directly after a letter or digit is text. New example (RH2109, `Counter.razor`).
- SHARED-DIAG-004 — A partial declaration is one declaration — added (P15). Coverage sentence: verdicts on what one part writes (its accessibility modifier, its modifier order, the name or parameters it declares) are not covered. That leaves room for P28 (thoenissen/Reihitsu#1331) and for the casing question below.
- SHARED-DIAG-005 — Parentheses, `!`, and a `throw` branch do not change a value — added (P16).
- SHARED-DIAG-006 — Every element of the rule's kind is checked — added (P20, including the round-8 addition: one diagnostic per reported declarator, on its identifier).
- SHARED-DIAG-007 — A diagnostic has a location in the source — added (P21).
- SHARED-DIAG-008 — A conditional directive in the judged span — added (P22 with the round-8 clarification). The judged span is the part of the construct the verdict is about. A directive in a body the rule does not judge does not stop the report, and neither do `#region`, `#endregion`, or `#pragma`. A fix that rewrites or moves the part of the judged span that holds a `#region`, `#endregion`, or `#pragma` is withheld; a fix that leaves the directive and its line in place, such as removing the comma in front of it, stays offered (reading B, maintainer decision).
- SHARED-TYPEREGION-002 — The members a type declares itself are counted — changed (P17). A nested type or enum is one member of the type around it; its own members, values, and regions belong to it. New RH7305 example.
- SHARED-TYPEREGION-006 — The top-level regions of a type declaration — changed (P17, items 9 and 19). The precondition "none lies inside a nested enum" is removed. A region belongs to the innermost class, struct, interface, record, record struct, or enum declaration. Regions of different declarations are never compared, and no fix moves anything across declarations. Top-level regions in different branches of one conditional directive are not compared for their descriptions; regions in the same branch, or one inside and one outside the directive, are. A member is judged by its innermost region. New examples: RH7311 nested enum, RH7308 nested region, RH7311 `#if`/`#else`, and RH7311 branch versus outside.
- SHARED-USING-004 — A group is section, kind, and root namespace — changed (item 18). An alias whose target is not a name forms its own group after every other group of its section. Coverage sentence: a section with at most one such alias (see Open questions).
- SHARED-USING-007 — Alphabetical order within a group, stable on ties — changed (P18 with the round-8 clarification). A reported directive is compared with every earlier directive of its group, whatever lies between. The same holds for every rule that orders a sequence. I amended 007 rather than minting SHARED-USING-011 because P18 defines what "within a group" means when a group is not contiguous, which is a qualifier of 007's decision. Its meaning is extended, not replaced. thoenissen/Reihitsu#1266 and thoenissen/Reihitsu#1331 also cite 007 for P18.
- SHARED-DOC-017 — An end tag with another name is no end tag of its own — added (P19). It includes the round-8 restatement on thoenissen/Reihitsu#1241: no end tag at all, or one with another name. The intro of `element-documentation.md` gains one sentence. No spec lists it under `Uses` yet, so it is not enforced.

`requirement-ids.txt`: SHARED-DIAG-004 to SHARED-DIAG-008 and SHARED-DOC-017 are inserted at their sorted positions. The SHARED-TYPEREGION-007 line from round 1 is removed again; that ID was never merged.

`diagnostics.md` header `Uses` is now `SHARED-FIX-003, SHARED-TRAIL-002`; SHARED-DIAG-004 references the first and SHARED-DIAG-008 the second. A new non-goal says that the name an alias directive declares, and how alias directives are ordered by it, belong to the using rules. `sdd/specs/README.md` needs no change: no spec, prefix, or code owner changed.

Checks (rerun in round 2, same result as round 1): `scripts/test.sh --project architecture` filtered to SpecConsistencyTests, SpecStructureTests, and RequirementCoverageTests ran 39 tests; 38 passed and 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`: SHARED-DIAG-004 to SHARED-DIAG-008 have no covering test yet. That is expected; the covering tests are the test implementer's job.

## Examples for the tests

Whether each shape below already holds today was established by reading the analyzer code, the existing tests, and the maintainer's instance lists. Nothing was run. Pin only the shapes marked "conforms"; the others are tracked by the follow-ups named.

- SHARED-DIAG-002 — alias example. `new S.NotImplementedException()` through `using S = System;` conforms: RH2003's syntactic pre-filter accepts the right-hand identifier of a qualified name, and the symbol check passes. `new NIE()` does not (thoenissen/Reihitsu#1268).
- SHARED-DIAG-003 — Razor example: no shape conforms. Every case is reported today (thoenissen/Reihitsu#1289), and the existing C# literal and comment tests cover the requirement. The untagged characterization tests `VerifyCodeBlockTextInCommentsAndStringsIsReported`, `VerifyCodeBlockTextInCSharpCommentAndStringLiteralsIsReported`, and `VerifyEscapedAndEmailLikeCodeBlockTextIsReported` in `RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests` pin output that the amended requirement calls wrong. Whether they stay until thoenissen/Reihitsu#1289 is the lead's call.
- SHARED-DIAG-004 — the RH2102 two-part partial class reported once conforms. The RH2004 `public partial class C` plus `partial class C` reported nothing conforms (RH2004-002). The untagged characterization test `VerifyAsyncVoidPartialMethodIsReportedOnBothDeclarations` (RH2002) contradicts it (thoenissen/Reihitsu#1287).
- SHARED-DIAG-005 — the RH1002 example conforms. Every line in it is a form SHARED-EQUAL-002 already specifies: `(null)` and `((IEqualityComparer<S>)null)!` are covered forms of its first list; `(comparer)`, `comparer!`, and `flag ? comparer : throw …` are custom.
- SHARED-DIAG-006 — RH4107 declarators (RH4107-001 already reports one diagnostic per declarator) and RH8101 middle parameter conform.
- SHARED-DIAG-007 — RH8031 at the `extension` keyword conforms (RH8031-003).
- SHARED-DIAG-008 — RH5412 with `#if DEBUG`/`#endif` in the body, not reported, conforms (stated on thoenissen/Reihitsu#1275). RH7106 with `#pragma` between the modifiers, reported, conforms (SHARED-MODORDER-003 example). RH7003 with `#if` in the namespace body, reported, conforms: `RH7003FileScopedNamespacesShouldBeUsedAnalyzerTests.VerifyDirectiveContainingNamespaceIsReported` pins it, and the analyzer has no directive guard. The RH5409 shapes of SHARED-TRAIL-002 conform too, including the fix offered in front of `#region` and `#pragma` (reading B). The RH7106 `#pragma` example offers no fix, as SHARED-MODORDER-003 specifies.
- SHARED-TYPEREGION-002 — the RH7305 nested-enum example is probably not reported today, because `E` lies in `Types`. Verify before pinning.
- SHARED-TYPEREGION-006 — the existing -006 tests still cover the requirement. Of the new examples:
  - The RH7311 nested-enum example does not conform (thoenissen/Reihitsu#1187 Q2, thoenissen/Reihitsu#1264).
  - The RH7308 nested `Properties` example is not reported per RH7308-001 ("outside any region nested in it"), but thoenissen/Reihitsu#1264 says `P` is reported against `Fields` today. The skeptic should verify which is true.
  - The RH7311 `#if`/`#else` example (not reported) does not conform (thoenissen/Reihitsu#1264).
  - The RH7311 branch-versus-outside example (the region inside `#if DEBUG` reported as a repeat of the one outside) probably conforms, since RH7311 counts regions in an excluded branch (thoenissen/Reihitsu#1240). This is the shape to pin for the item-19 qualifier.
- SHARED-USING-004 — the `int` alias example does not conform: today RH7207 reports it and the fix puts `using B = int;` first (thoenissen/Reihitsu#1267). The conforming shape is `using System;` / blank line / `using B = int;`, which stays unchanged.
- SHARED-USING-007 — `Microsoft.B; System; Microsoft.A` reported by RH7203 conforms: `RH7203…Analyzer.AnalyzeGroup` filters to the other-namespace kind before comparing. `Microsoft.C; Microsoft.A; Microsoft.B` does not: today `Microsoft.B` is compared only with `Microsoft.A` (thoenissen/Reihitsu#1266). The RH7110 example conforms: `narrowestSeenByGroup` keeps the narrowest accessibility seen so far in the group.
- SHARED-DOC-017 — the RH8202 example (no end tag at all) conforms (RH8202-004). The RH8107 `<remarks><returns>Nothing</remarks>` example does not; the fix is offered today (thoenissen/Reihitsu#1241).

## Contradictions with active requirements, all tracked elsewhere

These are recorded so the skeptic and the spec checker do not have to rediscover them. None of them is edited here, because each is in a file outside this run.

- SHARED-NESTED-002 ("each part of a nested partial type is reported as its own declaration") and RH2004-002 for partial properties contradict SHARED-DIAG-004. thoenissen/Reihitsu#1287 lists both for change. The same issue covers RH7002 and RH8306.
- RH4111, RH4113, and RH4114 show "a partial property, reported on each part", and RH4122 does the same for parameters. SHARED-DIAG-004's coverage sentence leaves names and parameters out. See the first open question.
- RH7110-001 says "the previous member of the same group". The code, and the round-8 P18 clarification, compare with every earlier member. thoenissen/Reihitsu#1331 notes RH7110 as a clarification only.
- RH7203-001, RH7204-001, and RH7206-001 ("sorted before its neighbor") read as neighbor-only (thoenissen/Reihitsu#1266).
- SHARED-DOC-017 states for every fix what RH8107-007 and RH8202-004 already state for an element without an end tag. Those specs can reference it in thoenissen/Reihitsu#1241.
- SHARED-EQUAL-002 leaves the `throw`-branch forms unspecified, and SHARED-DIAG-005 now specifies them (thoenissen/Reihitsu#1290).
- RH2109-001's coverage sentence excludes Razor, HTML, and C# comments and literals. The amended SHARED-DIAG-003 now covers them (thoenissen/Reihitsu#1289).

## Open questions to file

_None._ Round 1 found two questions in passing: whether a partial property's name is reported once or per part (RH4111/RH4113/RH4114, RH4122), and whether several aliases to non-names form one group. The maintainer decided not to file either (`squad/decisions/2026-10-21-1260-no-new-questions.md`). SHARED-DIAG-004 and SHARED-USING-004 keep their coverage sentences, which leave both cases out.

## Decisions needed

_None._ The round-1 question (P22's fix sentence) is answered with reading B (`squad/decisions/2026-10-21-1260-p22-fix-reading.md`).
