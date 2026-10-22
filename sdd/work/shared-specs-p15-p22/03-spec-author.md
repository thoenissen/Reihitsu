Role: spec author
Commit: c46fc52
Verdict: spec written

Repair round after the skeptic (`02-skeptic.md`, findings N1–N6). It follows the decisions recorded under `squad/decisions/2026-10-21-1260-*.md` and the coordinator's instruction for N2. I re-derived each finding against the spec text and the active requirements it names. Every finding holds, and each is fixed by narrowing the text. No principle changed. The skeptic's wording was used only as a suggestion. `01-spec-author.md` stays as the round-1/2 record; this file supersedes its "Examples for the tests" section.

## Changed requirements

- **SHARED-DOC-017** — clarified (N1). Only an element written with a start tag can lack an end tag of its own. An empty element (`<returns/>`, `<value/>`, `<placeholder/>`) needs none, and SHARED-DOC-017 does not withhold its fix. Added to `Why:`: an empty element ends where it is written. New example: the RH8107 fix removes `/// <returns/>`. Without this, the requirement contradicted RH8204-003 ("an empty element is removed") and the fixes that ship today.
- **SHARED-TYPEREGION-006** — narrowed (N2, N3).
  - N2: item 19 now says only what was decided. Regions in different branches (`#if`, `#elif`, `#else`) of one conditional directive are not compared. The positive half ("same branch, or inside versus outside, MUST be compared"), its `Why:` clause, and the reported example with `#if DEBUG` are removed. That half went beyond thoenissen/Reihitsu#1187 Q1, collided with SHARED-DIAG-008 and RH7311-001's coverage, and would have decided thoenissen/Reihitsu#1240.
  - N3: the innermost-region sentence now applies only to a rule that judges a member by the description of its region (RH7308). A rule that uses a region only as the scope of a comparison is not covered. New example on the other side of that qualifier: RH7110 compares `B` in the nested region `Inner` with `_a` in `Members`, as RH7110-003 specifies.
- **SHARED-TYPEREGION-002** — narrowed (N4). A nested declaration counts as one member, and what it declares is not counted for the type around it. Only regions among the members of a nested class, struct, interface, record, or record struct, or among the values of a nested enum, belong to that declaration. Extension blocks are left to -006, whose precondition still keeps types with a region inside a nested extension block out of coverage. The two paragraphs now agree.
- **SHARED-DIAG-006** — narrowed (N5).
  - The position and order independence holds unless the rule's subject is the order or position of the elements (a rule that orders them, or one that judges the last of them). That qualifier moved from `Why:` into the normative sentence.
  - "One diagnostic per declarator" applies to a rule that reports individual declarators. A rule whose verdict is about the whole declaration reports it once. `Why:` gained one clause for this, and there is a new RH2004 example: a field with two declarators reported once, at its first declarator, as RH2004-005 specifies.
- **SHARED-DIAG-007** — narrowed (N5). The token rule applies to a rule that locates its diagnostic on a declaration's name. A rule whose location is another part of the construct (an element, a directive, a span of whitespace) keeps that location. Without this, the requirement contradicted RH8031-003, RH7311-003, RH0002-002, and the RH6xxx spacing locations.
- **SHARED-DIAG-004** — clarified (N6). The coverage sentence now reads as examples ("for example …") and names the members and regions one part holds. That way it cannot contradict SHARED-TYPEREGION-001/-006, SHARED-REGGROUP-001, RH7311-002, or SHARED-ORDER-001.
- **SHARED-DIAG-008** — text unchanged. The RH5412 example states the principle; the body of an empty type is the span RH5412 judges. It does **not** conform today: `class A { #if DEBUG #endif }` is reported without a fix. The RH5416 test `VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix` pins that, and thoenissen/Reihitsu#1275 tracks it. The decision `2026-10-21-1260-principle-instances.md` has the lead correct the text of #1275. Do not pin this example.

`requirement-ids.txt`, `using-directive-ordering.md`, and `sdd/specs/README.md` are unchanged in this round.

Checks: `scripts/test.sh --project architecture` filtered to SpecConsistencyTests, SpecStructureTests, and RequirementCoverageTests ran 39 tests; 38 passed and 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, which lists only SHARED-DIAG-004 to SHARED-DIAG-008. Their covering tests are the test implementer's job.

## Examples for the tests

Shapes that conform today and can be pinned. This is taken from the skeptic's table plus the new examples of this round. "Cannot fail" marks a shape that would also pass under the forbidden behavior (FC-017). Everything listed under "do not pin" is tracked by the issue named.

- **SHARED-DIAG-002**
  - Pin: RH2003 `new S.NotImplementedException()` with `using S = System;`, reported on `S.NotImplementedException`.
  - Do not pin: `new NIE()` (thoenissen/Reihitsu#1268).
- **SHARED-DIAG-003**
  - Pin: the existing C# literal and comment tests already cover it.
  - Do not pin: the Razor shapes (decision `…-razor-content-no-conforming-shape.md`, thoenissen/Reihitsu#1289).
- **SHARED-DIAG-004**
  - Pin: the RH2102 two-part partial class reported once, and the RH2004 `public partial class C` plus `partial class C` not reported. Both cannot fail. Decision `…-diag004-weak-coverage.md` says to tag them anyway, and the run report states that real coverage comes with thoenissen/Reihitsu#1287.
  - Do not pin: RH2002, RH2101, the RH2004 partial property, RH7002, RH7401–RH7412, RH8306 (thoenissen/Reihitsu#1287).
- **SHARED-DIAG-005**
  - Pin: the RH1002 example. The parentheses and `!` lines are SHARED-EQUAL-002 forms; the `flag ? comparer : throw …` line cannot fail.
  - Do not pin: `flag ? null : throw …` (thoenissen/Reihitsu#1290).
- **SHARED-DIAG-006**
  - Pin: RH4107 `protected int _a, B; protected int D, _e;`, both declarators reported (skeptic: `fixed` in two iterations). RH8101 middle parameter (an existing test covers a later parameter; verify the middle position). The new RH2004 example `int a, b;` reported once at `a` (RH2004-005 example).
  - Do not pin: RH7403, RH7407, RH7411 multi-declarator events (thoenissen/Reihitsu#1333); RH8106 and RH8203 order cases (thoenissen/Reihitsu#1270).
- **SHARED-DIAG-007**
  - Pin: RH8031 at `extension` (RH8031-003).
  - Do not pin: RH5404 extension block (thoenissen/Reihitsu#1276).
- **SHARED-DIAG-008**
  - Pin, not reported: `#if` in the trailing-comma gap (SHARED-TRAIL-002 tests).
  - Pin, reported without a fix: RH7106 `#pragma` between modifiers; RH5412 with `#region R`/`#endregion` or `#pragma warning disable` in the body.
  - Pin, reported: RH7003 with `#if` in the body (`VerifyDirectiveContainingNamespaceIsReported`).
  - Pin, fix kept (reading B): RH5409 comma before `#region` or `#pragma` (SHARED-TRAIL-003 tests).
  - Do not pin: RH5412 `#if DEBUG`/`#endif` body (thoenissen/Reihitsu#1275); RH7101, RH7105/RH7106 with `#if` in the span (thoenissen/Reihitsu#1337); RH7201–RH7207 with `#pragma` or `#region` (to be filed per `…-principle-instances.md`).
- **SHARED-TYPEREGION-002**
  - Pin: the RH7305 nested-enum example, not reported. It cannot fail.
- **SHARED-TYPEREGION-006**
  - Pin: the existing -006 tests, and the new RH7110 nested-region example, which conforms because RH7110-003 and its test pin it.
  - Do not pin: the RH7311 nested enum (thoenissen/Reihitsu#1187 Q2, thoenissen/Reihitsu#1264); the RH7308 nested `Properties` example (`Value` is reported against `Fields` today, thoenissen/Reihitsu#1264); the RH7311 `#if`/`#else` example (thoenissen/Reihitsu#1264); the RH7401–RH7412 nested-region judgment (to be noted on #1264 per `…-principle-instances.md`).
- **SHARED-USING-004 (item 18)**
  - No new test (decision `…-using004-item18.md`). The existing -004 tests cover the unchanged part.
  - Do not pin: the `int` alias example (thoenissen/Reihitsu#1267).
- **SHARED-USING-007**
  - Pin: RH7203 `Microsoft.B; System; Microsoft.A`, and the same with `using static System.Math;` between; RH7110 `_a`/`B`/`D`, with `B` and `D` reported.
  - Do not pin: `Microsoft.B; Newtonsoft.X; Microsoft.A`, and `Microsoft.C; Microsoft.A; Microsoft.B` (thoenissen/Reihitsu#1266).
- **SHARED-DOC-017**
  - Pin: RH8202 `<value>The size` without an end tag, no fix offered; RH8107 `/// <returns/>`, fix offered and line removed (skeptic: `fixed`).
  - Do not pin: RH8107 `<remarks><returns>Nothing</remarks>` (thoenissen/Reihitsu#1241).

## Open questions to file

_None._ The maintainer decided not to file the two round-1 questions (`2026-10-21-1260-no-new-questions.md`). The skeptic's new principle instances are filed by the lead per `2026-10-21-1260-principle-instances.md`.

## Decisions needed

_None._
