Role: spec author
Commit: dafca25
Verdict: spec written

This is the repair round after `02-skeptic.md`. It applies the maintainer's answers Q1 (b), Q2 (b), Q3 (b), and Q4 (b), recorded in `squad/decisions/2026-10-23-1415-*.md`. The lane is now *specs and tests for existing behavior*. Every example below is a shape that conforms today; the requirement text states each principle in full. Shapes that do not conform today were removed. Their follow-ups add them together with the fix and its tests.

## Changed requirements

Everything is measured against `origin/main`. Changes since `01-spec-author.md` are marked "now".

- **SHARED-DIAG-011 — A change that cannot be written at a position is not asked for — added** (P38, Q1 (b)).
  - A rule does not report where the change it asks for cannot be written. That is the case when the grammar does not admit the change at that position, when the language version does not support it there, or when the position itself rules it out with a compile error (a required constant, a declaration that cannot carry the modifier).
  - A collision with the rest of the code does not rule the change out; that stays a case of SHARED-FIX-003.
  - Examples, all conforming: RH3103 grammar (both sides), RH3003 constant (both sides), RH2004 static constructor, RH5412 language version (both sides), and the RH3001 `x is !true` contrast. The pointer to the RH4106 collision example stays.
  - Now: the examples #1417 needs (dotted `nameof`, `.Equals`, `typeof(Nullable<>)`, the C# 11 alias) are gone.
  - The non-goal is reworded to "which change a rule asks for and where it can be written".
- **SHARED-FIX-003 — changed.**
  - Its "still reported" sentence covers inputs where the change can be written and only the fix is withheld (`x is !true`). `typeof(Nullable<>)` is gone, and SHARED-DIAG-011 is added to the list of requirements that name inputs a rule does not report.
  - Now: the sentence "a construct whose form is not valid C# … is not reported (SHARED-DIAG-011), so no fix is withheld for it" is removed (skeptic, contradicting tags).
  - The fifth condition (P39, Q2 (b)) now reads "change which declarations a documentation comment documents". Its **Documentation** bullet covers modes `Parse`/`Diagnose` only. It says a comment of a field or event declaration with several declarators documents each declarator. It covers a move that leaves the comment behind and a split that keeps it on one part only. It excludes a removal of the declaration together with its comment, and a change to a comment that the fix's own rule reports.
  - The Why now says that no fix may lose documentation and that the user rearranges such code by hand.
  - Now: the RH7309 example is removed, including the wrong claim about the shape with the comment on its own line above `#region Methods`. It is replaced by a pointer to the RH7103 carry examples under SHARED-DOC-004, which show the "offered" side. No conforming withheld shape exists, so that example belongs to #1262 and to the RH7101 follow-up.
- **SHARED-FIX-008 — clarified** (P35, Q3 (b)).
  - Every line break the fix creates or keeps in the code it changes gets the detected line ending, in lone-`\r` documents too.
  - Now: "the code it changes" is the span the fix replaces, rewrites, or moves, its trailing trivia included. A line break outside that span, such as the one that ends the line of a change ending within the line, is not in the fix's scope and not covered.
  - Now: the RH5406 lone-`\r` example is removed (the kept break behind `}` does not conform today; #1388). The existing RH5405 example stays.
- **SHARED-DOC-004 — changed** (P39) **and clarified** (P29).
  - Mode `Parse`/`Diagnose`: a fix that moves the declaration carries the comment or is not offered (SHARED-FIX-003).
  - Mode `None`: the comment is an ordinary trailing comment and stays behind the code in front.
  - Examples: the existing RH7103 `Parse` carry example, and the RH7103 mode-`None` example.
  - Now: the RH7309 pointer is gone, replaced by the RH7103 example with `#if DEBUG`/`#endif` between the comment and `Create`, which carries the comment and the pair (SHARED-ORDER-006). The skeptic ran this shape with `apply-fix.sh RH7103`, and the fix carried both.
  - `Uses` adds SHARED-FIX-003 and SHARED-ORDER-006.
- **RH3103-002, RH3003-002, RH2004-003, RH5412-001 — clarified (Why only).** Each now cites SHARED-DIAG-011, and each spec's `Uses` lists SHARED-DIAG-011. RH3003-002 and RH2004-003 are the instances named in the Q1 answer, and RH5412-001 is the language-version instance.
- Ledger: SHARED-DIAG-011 is inserted after SHARED-DIAG-010 (01). No code owners changed, `sdd/specs/README.md` is unchanged, there is no rule page or change doc, and RH3103-006, RH7101-004, and RH7309-004 are not touched.

## Examples for the tests

These are conforming shapes the test implementer can tag. "Existing" means a test already asserts the shape and only needs the tag.

- **SHARED-DIAG-011** (now uncovered, so `RequirementCoverageTests` fails until one is tagged):
  - Grammar, not reported: `NullableGenericInNameofIsNotReported` (RH3103), existing.
  - Grammar, reported: `NullableGenericInTypeofIsReportedAndFixed` (RH3103), existing.
  - Constant position: `EmptyLiteralsWhereAConstantIsRequiredAreNotReported` or `EmptyStringInConstFieldIsNotReported` (RH3003), existing.
  - Modifier not allowed: `VerifyStaticConstructorAndExplicitInterfaceImplementationsAreNotReported` (RH2004), existing.
  - Language version: `VerifySpecificationExampleIsNotReportedUnderCSharp11` and `VerifyEmptyClassIsReportedUnderCSharp12` (RH5412), existing.
  - Contrast: `NegationAsAWholeIsPatternIsReportedWithoutACodeFix` (RH3001), existing and untagged.
- **SHARED-FIX-003, fifth condition:**
  - Offered side: the RH7103 carry shapes listed under SHARED-DOC-004 below. A test that verifies the comment still documents the declaration after the fix covers the condition.
  - Withheld side: no conforming shape exists (skeptic). Do not tag a `#pragma` fixture with it, because there the fix is withheld for the directive reason.
- **SHARED-FIX-008:**
  - The existing RH5405, RH5406, RH5417 and RH7310/RH7303 tests cover "creates".
  - For "keeps", one candidate needs verification: in the RH5406 lone-`\r` test, the line break inside the wrapped body (`M(a,` → `b);`). The issue's description of today's output shows it as `\n`. If an explicit assertion confirms that, tag it SHARED-FIX-008. If not, it is #1388's. The kept break behind the new `}` is #1388's, and the RH7309 text swap keeps `\r` (skeptic), so it does not conform either. Build the lone-`\r` input inside the test (FC-033) and assert each line break explicitly (FC-017).
- **SHARED-DOC-004:**
  - Mode `Parse`, behind code: `FixCarriesADocumentationCommentBehindCodeWithTheMovedDeclaration`, existing.
  - Mode `Parse`, with `#if DEBUG`/`#endif` between: new test. The skeptic's `apply-fix.sh RH7103` carried the comment and the pair.
  - Mode `None`: new test with RH7103, run through `ApplyDocumentationModeNoneToTestProject`. Assert that `} /// <summary>Creates</summary>` stays behind `Run`'s closing brace after `Create` moves. The skeptic derived this from the code and did not run it, so if it fails, report it rather than adjusting the spec.

## Test tags for the test implementer (do not edit here)

- Remove `[Requirement("SHARED-FIX-003")]` from `DottedNullableGenericAsNameofArgumentDoesNotOfferFix` and `GlobalDottedNullableGenericAsNameofArgumentDoesNotOfferFix` (RH3103 tests). Their shape is a SHARED-DIAG-011 case: under the principle it is not reported at all, so it no longer exemplifies "reported, fix withheld". Keep the RH3103-006 tag, which they still assert until #1417 changes RH3103-006.
- The other tags this run needs are listed under "Examples for the tests".

## Conflicts left for follow-ups

These are decided by P38, P39, and P35, and are not open questions.

- RH3103-006 and the dotted-`nameof` tests, plus `Nullable<int>.Equals`, `nameof(Nullable<int>.HasValue)`, `typeof(Nullable<>)`, the C# 11 alias, and `using static System.Nullable<int>;` → #1417.
- RH7309 shapes 1–3 from the skeptic, including the own-line `///` above `#region Methods` and the one above `#endregion` → #1262.
- RH5406 kept break behind `}` → #1388. The RH7309 text swap keeping `\r` has no tracker yet (skeptic's candidate).
- RH7101-004 requires the split to keep the comment on the first field (FMT-STRUCT-022). For a documented declaration with several declarators, that now contradicts the fifth condition of SHARED-FIX-003 (Q2 (b)), and no tracker exists. A follow-up should withhold the RH7101 fix there and amend RH7101-004.
- RH3104 `new S();` is reported, and its fix writes `default(S);` (CS0201). Under Q1 (b) it is not reported, and no tracker exists.
- RH5417 inside an interpolation hole under C# 10 (skeptic's candidate): no tracker exists.

## Open questions to file

- **The formatter's field split and documentation** (`sdd/specs/formatter/structural-transforms.md`; FMT-STRUCT-021, FMT-STRUCT-022; also RH7101-004).
  - Input, mode `Parse`: `class C⏎{⏎    /// <summary>The coordinates</summary>⏎    public int X, Y;⏎}`.
  - Output: FMT-STRUCT-022 keeps the documentation comment on the first generated field, so `X` is documented and `Y` is not. The skeptic's probe shows the compiler documents both `F:C.X` and `F:C.Y` before the split.
  - Why it looks wrong: the maintainer's Q2 answer says no fix may lose documentation. That answer covers code fixes only. The formatter claims to be non-destructive, yet this split loses `Y`'s generated documentation.
  - Competing readings: the formatter keeps splitting as today, or it leaves such a declaration unsplit.
  - Not blocking for this run. No open `SDD open questions:` issue exists for that spec, so a new one is needed.

## Whole run

`scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RequirementCoverageTests"` gives 38 passed and 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered: SHARED-DIAG-011: no test covers it (shared/diagnostics.md:568)`, which the test implementer's tags resolve. SpecConsistencyTests and SpecStructureTests pass: `Uses` is exact and the ledger is sorted.

Compile probes in a scratch net10.0 library, outside the repository:
- `const string F = string.Empty;` fails with CS0133.
- `private static C() { }` fails with CS0515.
- `string _g = string.Empty;` builds.
- `class A;` under C# 11 fails with CS9058.
- `x is (true == false)` builds (01).

The prompt carried no intended fix or hoped-for verdict beyond the maintainer's recorded answers, which this round implements.
