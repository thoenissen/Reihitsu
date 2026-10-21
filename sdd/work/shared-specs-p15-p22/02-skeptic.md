Role: skeptic
Commit: 4bf269a41f36313587fabc467c6de8d96bc1e580
Verdict: proceed narrower

My prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to set aside.

## Premise

The run writes decided principles (thoenissen/Reihitsu#912 rounds 7a and 8) into shared specs, so no reported behavior is in question. I checked four things: whether each new sentence matches what was decided, whether it contradicts an active requirement that has a good `Why:`, whether the examples marked "conforms" really hold today, and which inputs the code decides that the text does not mention. Most of the diff holds. Six places need narrowing before tests are written (N1–N6). The conforming list in `01-spec-author.md` has one wrong entry and several examples that cannot fail (see *Breaking inputs*).

How I checked: `scripts/apply-fix.sh <ID> <dir>` over fixtures in a scratch directory outside the repository, deleted afterwards (LF and CRLF arms always agreed). Where a rule has no code fix, apply-fix cannot run it. For those I used code reading and existing tests, named with each finding.

### N1 — SHARED-DOC-017 withholds the fix for an empty element

As worded, an element "has an end tag of its own only when it has an end tag", and "an element without an end tag … has no end tag of its own". An empty element (`<returns/>`, `<value/>`, `<placeholder/>`) has no end tag, so SHARED-DOC-017 forbids the fix for it.

- Today the fix is offered for all three, and it is safe. `apply-fix.sh RH8107` on `/// <returns/>` above a `void` method gives `fixed`, with the line removed. `RH8202` on `/// <value/>` gives `fixed`. `RH8204` on `Runs <placeholder/> quickly` gives `fixed` (`Runs  quickly`).
- RH8204-003 (active, enforced) says: "an empty element (`<placeholder/>`) is removed". RH8107-001 already treats "an empty element" and "an element without an end tag" as two separate cases.
- The guard `DocumentationCommentCodeFixUtilities.CanRemoveElement` withholds only for `XmlElementSyntax` whose `EndTag.IsMissing`. An `XmlEmptyElementSyntax` always passes.
- P19 (thoenissen/Reihitsu#1260 and the round-8 note on thoenissen/Reihitsu#1241) is about an unclosed start tag or an end tag with another name. Empty elements are not part of it.

The requirement has to exempt the empty element explicitly. Otherwise it contradicts RH8204-003 and makes a safe fix that ships today a violation.

### N2 — SHARED-TYPEREGION-006 states more for item 19 than was decided

Item 19 is thoenissen/Reihitsu#1187 Q1, reading (a): "Do not compare the regions of mutually exclusive branches." The diff adds a second, positive half: "two regions in the same branch, or one inside and one outside the conditional directive, MUST be compared". It also adds a reported example, `{|RH7311:#region Fields|}` inside `#if DEBUG`. Nothing decided that half, and it collides with three things:

- **RH7311-001** deliberately covers only regions "outside any `#if` branch". Regions inside a branch are left open there.
- **SHARED-DIAG-008** (same diff): a conditional directive inside the span the rule judges means not reported. In the example, the `#if DEBUG`/`#endif` lies between the two regions RH7311 compares. So DIAG-008 reads as "not reported" and TYPEREGION-006 says "reported".
- **The inactive-code non-goal and thoenissen/Reihitsu#1240.** Analyzer tests define no preprocessor symbols (`CSharpParseOptions` default, no `preprocessorSymbols` in `Verify`), so the reported `#region` lies in an inactive branch. The `diagnostics.md` non-goal says "Code in an inactive `#if` branch … no rule can report it". Pinning this example implicitly decides thoenissen/Reihitsu#1240, which the lead's scope says not to resolve.

The other side of the decided qualifier is already shown by two top-level regions outside any conditional directive. Those are the existing -006 and RH7311-001 examples and tests.

### N3 — The "innermost region" sentence in SHARED-TYPEREGION-006 also reaches rules that use a region only as a scope

New text: "A rule that judges a member by the region it lies in MUST judge it by the innermost region that encloses it." P17 cites RH7308, which judges a member by the region's label. Read broadly, the sentence also covers rules that use the top-level region only as the scope of a comparison:

- **RH7110-003** (active, tested): `B` in `#region Inner`, nested in `#region Members`, is reported against `A` in the same top-level region. Under the broad reading it would be compared only inside `Inner`.
- **RH7103-004** and P27 (thoenissen/Reihitsu#1331: "member-ordering rules check each top-level region on its own") say the same as RH7110-003.

The sentence has to name rules that judge a member by the region's description or label.

The sentence also decides a shape that thoenissen/Reihitsu#1264 does not list. RH7401–RH7412 judge a member in a nested region by its top-level region, pinned by the untagged characterization tests `RH7401…Tests.VerifyOverrideInNestedRegionIsJudgedByItsTopLevelRegion` and `RH7405…Tests.VerifyMethodInNestedRegionIsJudgedByItsTopLevelRegion`. RH7401-001 leaves nested regions out ("lies in no region nested in a top-level region"). This is a new P17 instance for the lead to file.

### N4 — Extension blocks are inside SHARED-TYPEREGION-002 but outside -006

- The new paragraph of SHARED-TYPEREGION-002 says the regions inside a nested declaration belong to that declaration. Its own member list says "nested types (enums and extension blocks included)".
- The new ownership sentence of -006 lists only class, struct, interface, record, record struct, and enum. Read literally, a region inside an extension block belongs to the enclosing class.
- -006's precondition still excludes types with a region inside a nested extension block. The new ownership sentence is not bound by that precondition.

The two new paragraphs disagree on extension blocks. Code: `RegionDirectiveUtilities.GetTopLevelRegions` excludes the spans of nested `TypeDeclarationSyntax` nodes. That includes extension blocks only if Roslyn's extension-block syntax derives from it; I did not check this.

### N5 — SHARED-DIAG-006 and -007 are worded more broadly than their decisions

**SHARED-DIAG-006.** "Its verdict on one element MUST NOT depend on the element's position among the others." This has no exception for rules whose subject is order or position:

- SHARED-USING-007 in this same diff (P18 compares each element with every earlier one);
- RH8309, which orders documentation elements, the very kind DIAG-006 names;
- RH7105–RH7110 and RH7201–RH7207;
- RH5409–RH5411, which judge the last element.

The `Why:` already excludes them ("a verdict about the order, which the rule does not judge"). The normative sentence needs that qualifier.

Also, "A rule that reports declarators" must stay distinct from a rule that reports the declaration. RH2004-005 reports a multi-declarator field once, at its first declarator, on purpose.

**SHARED-DIAG-007.** "A construct without a name MUST be reported at a token of its own that marks it." Read literally, this contradicts active location requirements:

- RH8031-003: a textless `<summary>` of an extension block is reported on the summary element, not at `extension`;
- RH7311-003: the `#region` line;
- RH0002-002: the empty span at the end of a file without tokens;
- every whitespace-span location of the RH6xxx spacing rules.

P21 decided where a nameless declaration is reported when a rule would otherwise use its name. RH2004-005 already does exactly that: `this` for an indexer, the operator token for an operator. The sentence needs that scope.

### N6 — The SHARED-DIAG-004 coverage sentence must not read as a closed list

"A verdict on what one part writes — the accessibility modifier or the order of the modifiers…, or the name or the parameters…" Read as a closed list, DIAG-004 covers the members and regions each part of a partial type holds. That contradicts active per-part requirements:

- SHARED-TYPEREGION-001 and -006: "each declaration of a `partial` type on its own" / "has its own top-level regions";
- SHARED-REGGROUP-001;
- RH7311-002: the regions of another partial declaration are not compared;
- SHARED-ORDER-001: members of another part are not involved.

Read as examples, it does not contradict them. The list has to read as examples, or name the members and regions one part holds.

### Further facts the following roles need

- **The RH5412 example of SHARED-DIAG-008 does not conform.** `class A { #if DEBUG #endif }` → `apply-fix.sh RH5412` reports `no-fix-offered`, so it is reported today. The RH5416 test `VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix` pins the same shape as reported. The spec author's "conforms (stated on thoenissen/Reihitsu#1275)" is wrong, and so is thoenissen/Reihitsu#1275's "this already holds and stays". The requirement keeps the principle, which is the run's rule, but this example must not be pinned. The lead may want to correct the text of #1275.
- **SHARED-DIAG-008 contradicts the active, enforced SHARED-USING-009 for `#pragma` and `#region`.** SHARED-USING-009 says RH7201–RH7207 do not report a using block that holds any preprocessor directive. Run: `apply-fix.sh RH7203` on `using System.Linq;⏎#pragma warning disable CS0105⏎using System;` and on the same block with `#region R … #endregion` → both `no-diagnostic`. P22 says those two directives keep the report and withhold the fix. I found no follow-up issue for this. thoenissen/Reihitsu#1337 covers only RH7101 and RH7105–RH7108. This is a new P22 instance for the lead to file. `#nullable`, `#line`, `#error`, `#warning`, `#define`/`#undef` inside a judged span are named neither by P22 nor by DIAG-008. SHARED-USING-009 and SHARED-ORDER-005 decide them today.
- **Existing tests now contradict the new requirements.** The RH2109 decision covers only the three RH2109 tests. These untagged characterization tests pin the opposite of a new requirement, and their summaries still call the case "an open question":
  - `RH2003…Tests.VerifyCreationThroughUsingAliasIsNotReported` (DIAG-002);
  - `RH1002…Tests` around line 1987, the `throw`-branch comparer (DIAG-005);
  - `RH2002…Tests.VerifyAsyncVoidPartialMethodIsReportedOnBothDeclarations`, `RH2004…Tests` at lines 876 and 922, `RH2101…Tests` at line 285, `RH7401…Tests` at line 723 and `RH7409…Tests` at line 726 (DIAG-004);
  - `RH7403…Tests` at line 295 and `RH7411…Tests` at lines 605 and 640 (DIAG-006, thoenissen/Reihitsu#1333);
  - `RH7311…Tests.SameDescriptionInIfAndElseBranchIsCurrentlyReported` (TYPEREGION-006 item 19);
  - the RH7401 and RH7405 nested-region tests of N3;
  - `RH5416…Tests.VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix` (DIAG-008; not labelled a characterization test).

  How to treat them is the lead's decision. Keeping them until their follow-up is what the RH2109 decision did.
- **Active requirements that contradict the new ones and are already tracked:** SHARED-NESTED-002 and the RH2004 partial property, indexer, and event behavior against DIAG-004 (thoenissen/Reihitsu#1287). RH7203-001, RH7204-001, and RH7206-001 ("directly follows") and RH7110-001 ("previous member") against USING-007 (thoenissen/Reihitsu#1266, thoenissen/Reihitsu#1331). RH7003-001's "contains no `#if`" exclusion is now outdated by DIAG-008's RH7003 example. It does not contradict it, since it only narrows the MUST.
- **Two of the spec author's claims hold.** `shared/diagnostics.md` on `origin/main` never contained "an alias is a different name": `git grep` over `sdd/specs` finds no such sentence. And amending SHARED-USING-007 instead of minting -011 matches thoenissen/Reihitsu#1331, which cites SHARED-USING-007 for P18.

## Mechanism and candidates

_N/A._ No bug is claimed; the candidate sets per requirement are under *Breaking inputs*.

## Breaking inputs

"Conforms" means the shape gives the stated output today. "Cannot fail" means it would also pass under the behavior the requirement forbids, so it is weak evidence (FC-017).

| Requirement | Conforms today | Does not conform — do not pin |
|---|---|---|
| SHARED-DIAG-002 | RH2003 on `new S.NotImplementedException()` with `using S = System;`, reported on `S.NotImplementedException` (code: `GetSimpleTypeName` takes `QualifiedName.Right`, then a semantic check; not run) | `new NIE()` with `using NIE = System.NotImplementedException;` (characterization test above; thoenissen/Reihitsu#1268) |
| SHARED-DIAG-003 | Existing C# literal and comment tests; nothing in Razor (decided) | Razor shapes (thoenissen/Reihitsu#1289) |
| SHARED-DIAG-004 | RH2102 example; cannot fail, because C# allows a parameter list on only one part (CS8863). RH2004 example; cannot fail, because RH2004-002 exempts every partial type whether or not a part is `public`, so its caption names a reason RH2004 does not use, and RH7104 reports the same input (P28). I found no conforming shape that separates "judged by symbol, reported once" from a per-part check | RH2002 partial async void reported twice; RH2101 nested static partial; RH2004 partial property on both parts (thoenissen/Reihitsu#1287) |
| SHARED-DIAG-005 | The RH1002 example; every line is a form SHARED-EQUAL-002 already specifies, so the `throw` half cannot fail | `items.Distinct(flag ? null : throw new X())`, not reported today (`EqualityComparerArgumentUtilities` has no `throw` handling; thoenissen/Reihitsu#1290) |
| SHARED-DIAG-006 | RH4107 `protected int _a, B; protected int D, _e;` → `apply-fix.sh RH4107` `fixed` in 2 iterations (both declarators reported). RH8101 parameter not documented: an existing test covers a later parameter; the middle position was not run by me | RH7403, RH7407, RH7411 multi-declarator events (thoenissen/Reihitsu#1333) |
| SHARED-DIAG-007 | RH8031 at `extension` (RH8031-003) | RH5404 extension block (thoenissen/Reihitsu#1276) |
| SHARED-DIAG-008 | Not reported: `#if` in the comma gap (SHARED-TRAIL-002 tests) and `#if` in a using block (SHARED-USING-009 tests). Reported without a fix: RH7106 `#pragma` between modifiers (`no-fix-offered`), and RH5412 with `#region R⏎#endregion` or `#pragma warning disable` in the body (`no-fix-offered`). RH7003 with `#if` in the body: reported (untagged test `VerifyDirectiveContainingNamespaceIsReported`). Reading B, fix kept: RH5409 comma before `#region` (SHARED-TRAIL-003 test) | RH5412 `#if DEBUG⏎#endif` body: reported. RH7106 with `#if`/`#endif` between `internal` and `protected`, and RH7101 `private int _a,⏎#if DEBUG⏎_debug,⏎#endif⏎_b;`: reported (`no-fix-offered`; thoenissen/Reihitsu#1337). RH7203 block with `#pragma`/`#region`: not reported (untracked, see above) |
| SHARED-TYPEREGION-002 | RH7305 nested-enum example: not reported (code: the enum's region is nested in `Types`, depth 2, so not top-level; `E` lies in `Types`); cannot fail | — |
| SHARED-TYPEREGION-006 | Existing -006 tests | RH7311 nested enum (thoenissen/Reihitsu#1187 Q2). RH7308 nested `Properties` (code: `TryFindContainingRegion` searches only `GetTopLevelRegions`, so `Value` is judged by `Fields` and reported). RH7311 `#if`/`#else`. Branch versus outside: see N2 |
| SHARED-USING-004 (item 18) | No shape that can fail. `using System;⏎⏎using B = int;` passes on kind separation alone | Today the code gives the non-name alias the root `""`: `UsingDirectiveSyntax.Name` is null for a non-name target. `GetNamespaceGroupOrderKey` then ties it with `System`, and the ordinal root sort puts it first, so the spec's first block is today's canonical order (thoenissen/Reihitsu#1267). The analyzer and the formatter share `ComputeCanonicalOrder`, so FC-013 holds today and must hold after #1267 |
| SHARED-USING-007 | RH7203 `Microsoft.B; System; Microsoft.A` → `fixed`. It conforms only because `AnalyzeGroup` filters to System or Other before comparing; the same holds with `using static System.Math;` between (`fixed`). RH7110 `_a`/`B`/`D`: both `B` and `D` are reported (code: `narrowestSeenByGroup`, with `continue` after a report) | `Microsoft.B; Newtonsoft.X; Microsoft.A` → `no-diagnostic`. In `Microsoft.C; Microsoft.A; Microsoft.B`, `B` is compared only with `A` (code). Both are tracked by thoenissen/Reihitsu#1266 |
| SHARED-DOC-017 | RH8202 `<value>The size` without an end tag → `no-fix-offered` | RH8107 `<remarks><returns>Nothing</remarks>` → `fixed`, leaving `/// <remarks>` (thoenissen/Reihitsu#1241). Empty elements: see N1 |

Failure classes that can reach the tests:

- **FC-002:** the qualifiers "own end tag" against an empty element, "innermost region" against a top-level scope, and "judged span" against "elsewhere in the construct".
- **FC-003 / FC-020:** each directive class on both sides of the judged span: `#if`, including a fully inactive `#if false`, against `#pragma` and `#region`, inside the span against in the body.
- **FC-024:** partial types and members for DIAG-004.
- **FC-023:** verbatim names stay covered by the existing DIAG-002 tests.
- **FC-013:** SHARED-USING-004.
- **FC-017:** the examples marked "cannot fail".
- **FC-033:** tests run without preprocessor symbols, so every `#if DEBUG` branch is inactive. That decides which region of the item-19 examples is active code.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `sdd/specs/shared/element-documentation.md` (SHARED-DOC-017) | Must change (N1) |
| `sdd/specs/shared/type-regions.md` (-002, -006) | Must change (N2, N3, N4) |
| `sdd/specs/shared/diagnostics.md` (-004, -006, -007) | Must change (N5, N6). The -002, -003, -005, and -008 texts stay. |
| `sdd/specs/shared/using-directive-ordering.md` | Must stay |
| `sdd/specs/requirement-ids.txt` | Must stay |
| Analyzer tests | Must change: covering tags on conforming shapes only. Existing contradicting characterization tests: the lead decides |
| Analyzers, code fixes, Fix All, `Reihitsu.Core`, formatter | Not involved; must stay unchanged |
| Rule docs (`documentation/rules/`) | Not involved |
| Other specs (RH2004, RH7003, RH7110, RH72xx, SHARED-NESTED, SHARED-USING-009) | Not involved in this run; contradictions are tracked or to be filed |

## Simpler option

Proceed after narrowing the text to what was decided:

- **SHARED-DOC-017:** exempt the empty element (N1).
- **SHARED-TYPEREGION-006:**
  - keep only the decided negative half of item 19, and drop the positive half and its branch-versus-outside example (N2);
  - limit "innermost region" to rules that judge a member by its region's description (N3);
  - make the ownership list agree with -002 on extension blocks (N4).
- **SHARED-DIAG-006:** move the `Why:` qualifier on rules that judge order or position into the normative sentence (N5).
- **SHARED-DIAG-007:** limit the token rule to where a rule would otherwise use the declaration's name (N5).
- **SHARED-DIAG-004:** make the coverage list read as examples, or name the members and regions one part holds (N6).

None of these changes a principle. Each removes text that either goes beyond the decision or contradicts an active requirement.

## Questions for the maintainer

Only if the lead keeps the positive half of item 19 (N2) instead of narrowing it.

Input: the region `Fields` outside any directive, then `#if DEBUG` / `#region Fields` / `int _b;` / `#endregion` / `#endif`, analyzed without `DEBUG` defined.

- Reading A, as SHARED-TYPEREGION-006 is written now: the region inside `#if DEBUG` is compared with the outer one and reported by RH7311 (today's behavior, which RH7311 counts in inactive branches per thoenissen/Reihitsu#1240).
- Reading B, as SHARED-DIAG-008 is written and as the inactive-code non-goal reads: not reported, because a conditional directive lies inside the sequence of regions RH7311 judges, and the region is excluded code in the analyzed configuration.

If N2 is narrowed: _None._

New instances found in passing, decided by principle and for the lead to file:

- **P22:** RH7201–RH7207 do not report a using block that holds `#pragma` or `#region`, as SHARED-USING-009 specifies.
- **P17:** RH7401–RH7412 judge a member in a nested region by its top-level region.
