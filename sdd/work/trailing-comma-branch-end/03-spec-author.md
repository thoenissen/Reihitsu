Role: spec author
Commit: 1d6ccc1
Verdict: spec written

My prompt carried no intended fix or verdict beyond the maintainer's answer C, which `00-lead.md` records. I applied C and lead decisions 2 and 3. I did not use the notes for the implementer from `01-spec-author.md` (where the guard goes).

## Changed requirements

- SHARED-TRAIL-002 — A comma in front of a conditional directive is reported, and an `#if` behind it withholds the fix — changed, as in 01. The last sentence now says the formatter keeps the comma in front of every conditional directive, also where the fix is offered.
- SHARED-TRAIL-004 — The fix removes a comma whose branch ends behind it — added, as in 01. Changes in this pass:
  - New paragraph: the formatter and the formatter-delegating fixes (for example RH5303) keep the comma, so formatting does not clear the diagnostic; only this fix removes it. SHARED-DIAG-010 does not cover these rules, because their fix edits the text directly and does not lay code out through the formatter.
  - Why, decision 3: "a build that does not compile the comma does not see the change" is now qualified to SHARED-FIX-012's decided gap.
  - Why, decision 2: an `#if` in excluded code withholds the fix because the class is rare, not because telling it apart would need pairing.
  - Uses gains SHARED-DIAG-010 and SHARED-FIX-012.
- SHARED-DIAG-008 — clarified, as in 01: the example clause says "the comma in front of an `#if` directive".
- FMT-STRUCT-019 — clarified, behavior as on `main`:
  - The normative text from 01 was reverted to `main`'s.
  - Added: the "keep" rule holds for every conditional-directive kind, including a gap with only `#elif`, `#else`, and `#endif`, where the trailing-comma fix removes the comma (SHARED-TRAIL-004). Formatting does not clear that diagnostic.
  - Why: the formatter keeps the comma by the maintainer's decision, and SHARED-DIAG-010 does not cover these rules.
  - The example gains `enum Level` (`#else`/`High,`). `High,` is kept in the output, and a sentence follows the example.
  - Uses gains SHARED-DIAG-010 and SHARED-TRAIL-004.
- FMT-STRUCT-020 — unchanged (identical to `main`). It defers to FMT-STRUCT-019.
- Change doc `sdd/changes/2026-11/trailing-comma-branch-end.md` — `Status: Accepted`. Changes:
  - What, Why, and Visible output changes are rewritten for C. The fix results are unchanged from 01. The formatter is "no change", with the `Level` example. The section on formatter-delegating fixes is "no change", names the confirmed members (RH3202, RH3208, RH5110, RH5301, RH5303, RH5304), and keeps the RH5303 example.
  - Decisions: the maintainer's answer C quoted with its options; the formatter-vs-rule relationship (P11, SHARED-DIAG-010); the reason for withholding on an `#if` in excluded code; the SHARED-FIX-012 gap.
  - Alternatives: A and B rejected by the maintainer. Declaring the shape unsupported is not asked again.

P11 and SHARED-DIAG-010 check:
- P11 covers only the formatter doing more than the rules. Here the rule's fix does more than the formatter, and the maintainer decided that.
- SHARED-DIAG-010 covers only layout rules whose fix lays code out as the formatter does. The trailing-comma fix (`TrailingCommaCodeFixHelper.RemoveTrailingCommaAsync`) edits source text directly, so its second paragraph excludes these rules.
- Reporting a comma the formatter keeps already holds on `main` for a comma in front of `#if`.
- The relationship is stated in FMT-STRUCT-019 and SHARED-TRAIL-004 so that FC-013 ("fix and formatter decide alike") is not "repaired" by an implementer.

Checks:
- `scripts/format.sh` on `main` keeps both the `Mode` comma and the `Level` comma; the only change is the final newline.
- `scripts/test.sh --project architecture`: 197 passed, 1 failed, `EveryRequirementOfAnEnforcedSpecIsCovered` (SHARED-TRAIL-004 not yet tagged). This failure is expected until the test implementer tags it.

## Examples for the tests

- SHARED-TRAIL-004 (fix offered and comma removed):
  - `#endif` in a DEBUG build.
  - `#else` (excluded `B,` stays).
  - `#elif`.
  - Fix All over enums `E` and `F` (change doc).
  - RH5410 array and RH5411 list (change doc).
- SHARED-TRAIL-002 (no fix):
  - `#endif` followed by `#if Y`.
  - `#if Y` in code that `#else` excludes.
  - Comma before `#if`.
  - Fix offered for `#region`.
- FMT-STRUCT-019 / FMT-STRUCT-020 (formatter keeps the comma, behavior unchanged): `Mode` and `Level`. Existing tests pin this, for example `StructuralTransformPhaseIntegrationTests.PreservesTrailingCommaWhenElseBranchFollowsFinalElement` and `PreservesTrailingCommaWhenActiveIfTrueBranchFollowsFinalElement`. These tests must stay green, not flip.
- RH5303 (formatter-delegating fix keeps `3,`): change doc example. Optional; no output changes.
- Tests that flip (the analyzer-side half of 02's list only):
  - RH5409 `VerifySpecificationExampleReportsCommaInFrontOfEndIfWhereTheSymbolIsDefined`.
  - RH5409 `VerifyCommaInFrontOfEndIfOfAnActiveBlockIsReportedWithoutFix`.
  - The `withSymbol` halves of the three `…WithConditionalBlockAfterFinal…IsReportedWithoutFix` tests (RH5409, RH5410, RH5411).
  - The `StructuralTransformPhaseIntegrationTests` named in 02 do not flip under C.

## Output questions

_None._ C is answered.

## Internal choices

- The formatter-vs-fix relationship is stated in both FMT-STRUCT-019 and SHARED-TRAIL-004. The alternative was only the change doc; I rejected it because specs outlive change docs, and FC-013 would otherwise invite harmonizing them.
- No new FMT-STRUCT requirement; the existing "keep" sentence was clarified. The alternative was a separate requirement for the `#elif`/`#else`/`#endif` gap.
- The rule pages RH5409, RH5410, and RH5411 are unchanged. The fix is offered in more corner cases, but what is reported and how to fix it are unchanged in the normal case.
- Not spec, for the implementer: under C the formatter transforms keep `SyntaxTriviaUtilities.ContainsConditionalCompilationBoundary`, and only the fix needs the narrower kind check. The two decisions now differ on purpose, so 02's FC-015 note (one shared predicate) applies to each decision separately.

## Follow-up candidates

_None._
