Role: spec author
Commit: 6529552
Verdict: spec written — the lane must widen to tests (see "Lane")

## Changed requirements

- SHARED-DIAG-011 — A form the language does not allow is not asked for — added (P38). `sdd/specs/shared/diagnostics.md`, ledger line inserted after SHARED-DIAG-010; new non-goal: which form a rule asks for and where it is valid, beyond the named cases, is each rule's spec.
- SHARED-FIX-003 — A fix that could break the code is withheld — changed:
  - P38: the "still reported" sentence now covers only inputs where the asked form is valid at that position; `typeof(Nullable<>)` is removed from it (SHARED-DIAG-011 now leaves that input unreported). The list of requirements that name unreported inputs adds SHARED-DIAG-011.
  - P39: a fifth condition, "separate a declaration from a documentation comment that belongs to it", with a **Documentation** bounding bullet (modes `Parse`/`Diagnose` only), a Why sentence, and the issue's RH7309 example.
- SHARED-FIX-008 — A fix keeps the document's line ending — clarified (P35). Every line break the fix creates **or keeps** in the code it changes gets the detected ending, in lone-`\r` documents as well. "The code it changes" is defined as the code the fix replaces, rewrites, or moves, plus the line break that ends its last line. A line break outside it is left to SHARED-FIX-006. Adds the issue's RH5406 lone-`\r` example.
- SHARED-DOC-004 — The documentation comments of a declaration precede it — changed (P39) and clarified (P29):
  - Mode `Parse`/`Diagnose`: a fix that moves the declaration carries the comment, or it is not offered (SHARED-FIX-003). The old exemption "no directive between" is gone.
  - Mode `None`: the behind-code `///` is an ordinary trailing comment and stays behind the code in front.
  - The old "not covered" example with `#if DEBUG`/`#endif` is replaced by the mode-`None` RH7103 example and a pointer to the RH7309 example.
- RH3103-002 — Why now cites SHARED-DIAG-011. RH3103 `Uses` is now SHARED-DIAG-011.
- Header `Uses` updates: `shared/code-fixes.md` adds SHARED-DIAG-011 and SHARED-DOC-004. `shared/element-documentation.md` adds SHARED-FIX-003.
- No code owners changed. `sdd/specs/README.md` is unchanged. No rule page is changed, because no shipped behavior changes in this run. No change doc was written: the issue says none, following the maintainer's round-10 decision. The follow-ups carry the code: thoenissen/Reihitsu#1417 (behavior change), #1262, #1388.

## Lane

The docs lane does not hold. SHARED-DIAG-011 is a new requirement in an enforced spec, and `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails with `SHARED-DIAG-011: no test covers it (shared/diagnostics.md:568)`. A test that already conforms is `NullableGenericInNameofIsNotReported` in `src/Reihitsu.Analyzer.Test/Clarity/RH3103UseShorthandForNullableTypesAnalyzerTests.cs` (`nameof(Nullable<int>)` is not reported). Tagging it `[Requirement("SHARED-DIAG-011")]` is enough for coverage, so the lane widens to tests.

## Examples for the tests

- SHARED-DIAG-011: the unqualified `nameof(Nullable<int>)` (not reported, conforms today) and `typeof(Nullable<int>)` (reported, conforms today) are both covered by existing RH3103 tests. These parts of the example do **not** conform today and are #1417's to cover: `nameof(System.Nullable<int>)`, `Nullable<int>.Equals(1, 2)`, `typeof(Nullable<>)`, and the C# 11 alias.
- SHARED-DOC-004, mode `None` (P29): the RH7103 example in which `} /// <summary>Creates</summary>` stays behind `Run`. The issue says this is today's behavior. No test pins it (the existing `FixCarriesADocumentationCommentBehindCodeWithTheMovedDeclaration` runs mode `Parse` only), and I did not run it. I recommend a test in the widened lane.
- SHARED-FIX-003, fifth condition (P39): one shape conforms today. With `/// <summary>M</summary>` on its own line directly above `#region Methods` in the RH7309 example, mode `Parse`, no fix is offered. This follows from RH7309-004's guard "no comment outside its top-level region blocks"; I derived it from the spec and did not run it, and no RH7309 test covers that guard. The behind-code shape in the example is #1262's reproduction.
- SHARED-FIX-008 (P35): the RH5406 lone-`\r` example. The kept break behind the new `}` is #1388's reproduction. The existing lone-`\r` tests (RH5405, RH5406, RH5417) assert only the breaks the fix creates, so none contradicts the new wording.

## How the statements were established

- Compile probes in a scratch net10.0 library (outside the repository):
  - `x is (true == false)` compiles. So `x is !true` stays a P23 case, which is why it stays in SHARED-FIX-003 and in the SHARED-DIAG-011 contrast example. The old SHARED-FIX-003 reason "no correction compiles" was wrong for it.
  - `nameof(int?)`, `int?.Equals(1, 2)` and `nameof(int?.HasValue)` fail with CS1525.
  - `using N = int?;` builds under the default language version and fails under C# 11 with CS9058.
- Spec tests: `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RequirementCoverageTests"` gives 38 passed and 1 failed. The failure is the coverage finding above. SpecConsistencyTests and SpecStructureTests pass (ledger sorted, `Uses` exact, IDs unique). `LC_ALL=C sort -c` on the ledger passes too.

## Known conflicts left for the follow-ups (not open questions; P38/P39/P35 decide them)

- RH3103-006 still says the dotted `nameof(System.Nullable<int>)` is reported without a fix. SHARED-DIAG-011's example says it is not reported. The tests `DottedNullableGenericAsNameofArgumentDoesNotOfferFix` and `GlobalDottedNullableGenericAsNameofArgumentDoesNotOfferFix` assert the old reading (tagged RH3103-006 and SHARED-FIX-003). Changing RH3103 is #1417's behavior change. The precedent is squad/decisions/2026-10-22-1331-p26-reach.md, where a contradicting requirement stayed until its follow-up.
- RH7309-004 says the fix MUST reorder a body with no comment outside the blocks. That covers the behind-code P39 shape, so the new SHARED-FIX-003 condition overrides it until #1262 brings the code and, if wanted, the rule spec in line.
- The issue's premise about "today" is partly wrong. The characterization tests `FixOfUnboundNullableGenericInTypeofWritesAQuestionMarkOnly` (`typeof(?)`) and `FixOfNullableGenericReceivingAStaticCallWritesTheShorthand` (`int?.Equals(1, 2)`) show that RH3103 currently **offers** a fix that breaks the code for these two inputs. It does not withhold the fix. Only the dotted `nameof` input has its fix withheld. Relevant to #1417.

## Follow-up candidates (outside this run)

- For #1417, a further P38 instance the issue does not list: `nameof(Nullable<int>.HasValue)` is reported today, and the fix writes `nameof(int?.HasValue)`, which fails with CS1525. Characterization test: `FixOfNullableGenericQualifyingAMemberInNameofWritesTheShorthand`.
- Under P38, `documentation/rules/RH3103.md` will need updating together with #1417. Under P39, so may `general-notes.md` if the maintainer wants the documentation case stated there. Neither changes here, because shipped behavior does not change in this run.

## Non-goals of this run

- No rule spec other than the RH3103-002 citation, no rule page, and no code or test.
- SHARED-DIAG-011 does not enumerate further rules' invalid positions.
- The fifth SHARED-FIX-003 condition does not inventory which other fixes violate it today.

## Open questions to file

_None._
