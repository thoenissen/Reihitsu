Role: spec author
Commit: a560e03
Verdict: open question

The spec diff is written for everything except one sentence of SHARED-DIAG-008: what P22 says about the code fix. That sentence conflicts with SHARED-TRAIL-003 as it is tested today, and the two readings give different output for the same input (see "Decisions needed"). SHARED-DIAG-008 is committed without that sentence. Once the maintainer answers, the spec author adds it.

The lead prompt carried no intended fix or verdict that needed setting aside.

## Changed requirements

- SHARED-DIAG-002 — A name is compared by its value — changed (P3 extension). A name used through a `using` alias resolves to the symbol the alias names. New alias example (RH2003). `shared/diagnostics.md` never contained the sentence "an alias is a different name". The only alias statement in the specs is the coverage sentence of RH2003-001 ("… and not through a `using` alias"). It is outside this run's files and stays; thoenissen/Reihitsu#1268 tracks it.
- SHARED-DIAG-003 — Only the text of a literal or a comment is content — changed (P5 extension). New bullet: in a Razor file, the text of a Razor comment and of an HTML comment is comment text, `@@` is text, and an `@` directly after a letter or digit is text. New example (RH2109, `Counter.razor`).
- SHARED-DIAG-004 — A partial declaration is one declaration — added (P15). Coverage sentence: verdicts on what one part writes (its accessibility modifier, its modifier order, the name or parameters it declares) are not covered. That leaves room for P28 (thoenissen/Reihitsu#1331) and for the casing question below.
- SHARED-DIAG-005 — Parentheses, `!`, and a `throw` branch do not change a value — added (P16).
- SHARED-DIAG-006 — Every element of the rule's kind is checked — added (P20, including the round-8 addition: one diagnostic per reported declarator, on its identifier).
- SHARED-DIAG-007 — A diagnostic has a location in the source — added (P21).
- SHARED-DIAG-008 — A conditional directive in the judged span — added (P22 with the round-8 clarification). The judged span is the part of the construct the verdict is about; a directive in a body the rule does not judge does not stop the report; `#region`, `#endregion`, and `#pragma` do not stop the report. The fix sentence is pending (Decisions needed).
- SHARED-TYPEREGION-002 — The members a type declares itself are counted — changed (P17). A nested type or enum is one member of the type around it; its own members, values, and regions belong to it. New RH7305 example.
- SHARED-TYPEREGION-006 — The top-level regions of a type declaration — changed (P17, item 9). The precondition "none lies inside a nested enum" is removed. A region belongs to the innermost class, struct, interface, record, record struct, or enum declaration. Regions of different declarations are never compared, and no fix moves anything across declarations. A member is judged by its innermost region. New RH7311 and RH7308 examples.
- SHARED-TYPEREGION-007 — Regions in branches that exclude each other are not compared — added (item 19). This is a deviation from the issue, which asked to put item 19 into -002/-006. It is a separate decision with its own Why (no build compiles both branches), so the one-decision-per-heading rule in `sdd/README.md` gives it its own heading. No spec lists it under `Uses` yet (RH7311 lists only -006), so it is not enforced and needs no covering test. If the lead prefers the issue's placement, it can fold into -006 as a qualifier.
- SHARED-USING-004 — A group is section, kind, and root namespace — changed (item 18). An alias whose target is not a name forms its own group after every other group of its section. Coverage sentence: a section with at most one such alias (see Open questions).
- SHARED-USING-007 — Alphabetical order within a group, stable on ties — changed (P18 with the round-8 clarification). A reported directive is compared with every earlier directive of its group, whatever lies between. The same holds for every rule that orders a sequence. I amended 007 rather than minting SHARED-USING-011 because P18 defines what "within a group" means when a group is not contiguous, which is a qualifier of 007's decision. Its meaning is extended, not replaced. thoenissen/Reihitsu#1266 and thoenissen/Reihitsu#1331 also cite 007 for P18.
- SHARED-DOC-017 — An end tag with another name is no end tag of its own — added (P19). It includes the round-8 restatement on thoenissen/Reihitsu#1241: no end tag at all, or one with another name. The intro of `element-documentation.md` gains one sentence. No spec lists it under `Uses` yet, so it is not enforced.

`requirement-ids.txt`: SHARED-DIAG-004 to SHARED-DIAG-008, SHARED-DOC-017, and SHARED-TYPEREGION-007 are inserted at their sorted positions.

`diagnostics.md` header `Uses` is now `SHARED-FIX-003, SHARED-TRAIL-002`; SHARED-DIAG-004 references the first and SHARED-DIAG-008 the second. A new non-goal says that the name an alias directive declares, and how alias directives are ordered by it, belong to the using rules. `sdd/specs/README.md` needs no change: no spec, prefix, or code owner changed.

Checks: `scripts/test.sh --project architecture` filtered to SpecConsistencyTests, SpecStructureTests, and RequirementCoverageTests ran 39 tests; 38 passed and 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`: SHARED-DIAG-004 to SHARED-DIAG-008 have no covering test yet. That is expected; the covering tests are the test implementer's job.

## Examples for the tests

Whether each shape below already holds today was established by reading the analyzer code, the existing tests, and the maintainer's instance lists. Nothing was run. Pin only the shapes marked "conforms"; the others are tracked by the follow-ups named.

- SHARED-DIAG-002 — alias example. `new S.NotImplementedException()` through `using S = System;` conforms: RH2003's syntactic pre-filter accepts the right-hand identifier of a qualified name, and the symbol check passes. `new NIE()` does not (thoenissen/Reihitsu#1268).
- SHARED-DIAG-003 — Razor example: no shape conforms. Every case is reported today (thoenissen/Reihitsu#1289), and the existing C# literal and comment tests cover the requirement. The untagged characterization tests `VerifyCodeBlockTextInCommentsAndStringsIsReported`, `VerifyCodeBlockTextInCSharpCommentAndStringLiteralsIsReported`, and `VerifyEscapedAndEmailLikeCodeBlockTextIsReported` in `RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests` pin output that the amended requirement calls wrong. Whether they stay until thoenissen/Reihitsu#1289 is the lead's call.
- SHARED-DIAG-004 — the RH2102 two-part partial class reported once conforms. The RH2004 `public partial class C` plus `partial class C` reported nothing conforms (RH2004-002). The untagged characterization test `VerifyAsyncVoidPartialMethodIsReportedOnBothDeclarations` (RH2002) contradicts it (thoenissen/Reihitsu#1287).
- SHARED-DIAG-005 — the RH1002 example conforms. Every line in it is a form SHARED-EQUAL-002 already specifies: `(null)` and `((IEqualityComparer<S>)null)!` are covered forms of its first list; `(comparer)`, `comparer!`, and `flag ? comparer : throw …` are custom.
- SHARED-DIAG-006 — RH4107 declarators (RH4107-001 already reports one diagnostic per declarator) and RH8101 middle parameter conform.
- SHARED-DIAG-007 — RH8031 at the `extension` keyword conforms (RH8031-003).
- SHARED-DIAG-008 — RH5412 with `#if DEBUG`/`#endif` in the body, not reported, conforms (stated on thoenissen/Reihitsu#1275). RH7106 with `#pragma` between the modifiers, reported, conforms (SHARED-MODORDER-003 example). RH7003 with `#if` in the namespace body, reported, conforms: `RH7003FileScopedNamespacesShouldBeUsedAnalyzerTests.VerifyDirectiveContainingNamespaceIsReported` pins it, and the analyzer has no directive guard. The RH5409 shapes of SHARED-TRAIL-002 conform too.
- SHARED-TYPEREGION-002 — the RH7305 nested-enum example is probably not reported today, because `E` lies in `Types`. Verify before pinning.
- SHARED-TYPEREGION-006 — the RH7311 nested-enum example does not conform (thoenissen/Reihitsu#1187 Q2, thoenissen/Reihitsu#1264). The RH7308 nested `Properties` example is not reported per RH7308-001 ("outside any region nested in it"), but thoenissen/Reihitsu#1264 says `P` is reported against `Fields` today. The skeptic should verify which is true. The existing -006 tests still cover the requirement.
- SHARED-TYPEREGION-007 — the second example (a region in a branch and one outside the `#if`, reported) probably conforms. The first (`#if`/`#else`, not reported) does not (thoenissen/Reihitsu#1264).
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

Two questions. The first needs a new issue, `SDD open questions: sdd/specs/shared/diagnostics.md`, since none exists. The second belongs to `sdd/specs/shared/using-directive-ordering.md`, whose last question issue, thoenissen/Reihitsu#1184, is closed, so a new issue or a reopen is needed.

### A partial property's name: one diagnostic or one per part

- Input: `public partial class C { private partial int count { get; } }` plus `public partial class C { private partial int count => 0; }`.
- Real output: RH4111 reports `count` on both parts. That is specified in RH4111's example "a partial property, reported on each part", and likewise in RH4113, RH4114, and RH4122 for parameters.
- Why it looks wrong: P15 says a partial member is judged by its symbol and reported once. P28 (round 8) exempts verdicts on what each part writes, but names only the accessibility modifier and the modifier order. Each part writes the name, but the name is also a property of the one symbol.
- Readings: (a) P15 applies, so one diagnostic and the four rule examples change; (b) P28 applies, so each part is reported as today.
- Touches: SHARED-DIAG-004, RH4111-001, RH4113-001, RH4114-001, RH4122-001.

### Several aliases to non-names in one section

- Input: `using System;` / blank line / `using B = int;` / `using T = (int, int);` / blank line / `class C { }`.
- Real output: both aliases have the root `""` and form one group, which today sorts first among the alias groups (thoenissen/Reihitsu#1267).
- Why it is open: item 18 says "an alias whose target is not a name forms its own group after all other groups of its section". It does not say whether all such aliases share one group (no blank line between them) or each forms a group (a blank line between them, and RH7207 reports its absence). SHARED-USING-004 covers only a section with at most one such alias.
- Touches: SHARED-USING-004, SHARED-USING-008, RH7207-001.

## Decisions needed

### P22's fix sentence and a trailing comma in front of `#region` or `#pragma`

- Input (RH5409; the SHARED-TRAIL-002 example):

  ```csharp
  enum F
  {
      A,
      B,
  #region R
  #endregion
  }
  ```

  The same holds with `#pragma warning disable CS0168` / `#pragma warning restore CS0168` in place of the region.
- Real output: the comma after `B` is reported, and the fix removes it, keeping both directives on their lines. This is pinned by `RH5409…AnalyzerTests.VerifySpecificationExampleReportsCommaInFrontOfRegionButNotInFrontOfConditionalBlock` and `VerifyCommaInFrontOfPragmaDirectivesIsReportedAndFixed`, the latter tagged SHARED-TRAIL-003.
- Why it is a question: P22 (thoenissen/Reihitsu#1260) says `#region`, `#endregion`, and `#pragma` inside the construct do not stop the report but withhold the fix. The round-8 clarification (thoenissen/Reihitsu#1331) lists the comma gap among the judged spans. Read literally, P22 withholds this fix. No follow-up lists RH5409, RH5410, or RH5411 for P22; thoenissen/Reihitsu#1275 and thoenissen/Reihitsu#1337 cover only fixes that rewrite the span around the directive (empty-type bodies, modifier lists, accessor lists).
- Reading A — the fix is always withheld when such a directive lies in the judged span. Proposed sentence: "A code fix MUST NOT be offered for a construct whose judged span holds a `#region`, `#endregion`, or `#pragma` directive; the diagnostic is still reported." Output: RH5409, RH5410, and RH5411 offer no fix for the input above. SHARED-TRAIL-003 and the two RH5409 tests change, which needs a behavior-change follow-up.
- Reading B — the fix is withheld only when it would rewrite or move the part of the span that holds the directive. A fix that leaves the directive and its line where they are, such as removing the comma in front of it, stays offered. Proposed sentence: "A code fix that rewrites or moves the part of the judged span that holds a `#region`, `#endregion`, or `#pragma` directive MUST NOT be offered; the diagnostic is still reported. A fix that leaves the directive and its line in place, such as removing the comma in front of it, stays offered." Output: nothing changes for RH5409; thoenissen/Reihitsu#1275 and thoenissen/Reihitsu#1337 still withhold their fixes.
- Recommendation: B. The comma fix is safe under SHARED-FIX-003, and AGENTS.md (Code-fix coverage) treats refusing a safely rewritable input as a guard that is too wide. Every instance the maintainer named for the fix half (empty-type bodies, modifier lists, accessor lists) rewrites the span around the directive, and reading B withholds exactly those.
