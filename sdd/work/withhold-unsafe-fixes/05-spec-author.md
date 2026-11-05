Role: spec author (merge of `main`, change docs accepted)
Commit: 02547f6
Verdict: #1136, #1134, #1135, #1262, #1332, #1340: spec written; #1336: open question (Q1, RH7101 only)

Inputs: `00-lead.md`, `01`–`04-spec-author.md`, `02-skeptic.md`, `squad/README.md` and `squad/roles/spec-author.md` as on `main` at `a59a003`, the spec diff of the run, and `main`'s change doc `sdd/changes/2026-11/rh7101-fix-through-context-entry-point.md`. The prompt carried no intended fix or hoped-for verdict. "Today" below means the code of `main` at `a59a003`; the run has changed no file under `src/`, so the merged tree's code equals it. Every observation comes from `scripts/apply-fix.sh <ID>` on scratch fixtures (documentation mode `Parse`, LF and CRLF alike); the fixtures are outside the repository.

## thoenissen/Reihitsu#1136

### Changed requirements

None in this round. `main` changed only SHARED-FIX-007 (the bullet on changed code formatted within a larger node) in `shared/code-fixes.md`; it auto-merged next to the run's SHARED-FIX-003 text without overlap, and nothing in the merged SHARED-FIX-003 refers to the replaced SHARED-FIX-007 wording.

### Change doc

`withhold-unsafe-fixes.md` is `Accepted` (00-lead.md, *Maintainer answers* 3). It now has the `Visible output changes` section of the new change-doc form, written from the table in `What` with "before" outputs measured today; `Decisions` gained *The maintainer's answers*, which records the approval and holds the narrower answers that were listed before under another heading. The substance is the same.

### Output questions

_None._

## thoenissen/Reihitsu#1134

Nothing changed in this round. RH7109 is untouched by `main`. Case 5 of the change doc was measured today: the struct's fields are swapped by the fix.

## thoenissen/Reihitsu#1135

Nothing changed in this round. RH7309 is untouched by `main`. Case 8 of the change doc (initializer input) was measured today: the fix is offered and puts `X = 5` in front of `Y = X + 1`.

## thoenissen/Reihitsu#1262

Nothing changed in the specs. `main` changed `RegionDirectiveUtilities` (new `GetEndRegionText`; `GetEndRegionDescription` unchanged in result) and the region phase (FMT-REGION-007, FMT-PIPE-028: node-level entry points apply no region requirement). None of RH7303, RH7304, RH7306, RH7309, or SHARED-REGDESC/SHARED-TYPEREGION changed on `main`, and none of the run's requirements relies on the region phase running inside a fix. Today's outputs for cases 6–8 of `withhold-unsafe-fixes.md` and case 2 of `silence-uncorrectable-namespaces-and-regions.md` are measured and written into the docs. One finding while measuring: the RH7309 comment-above-a-moved-block input (case 8, second input) gets a fix today, although RH7309-004 on `735d0db` already claimed only bodies without a comment outside the blocks. The run's RH7309-006 decides it, so this is no new question.

## thoenissen/Reihitsu#1332

Nothing changed in the specs. Measured today: the lone `break` is moved and its empty block stays, as RH7501-004 requires, so the change doc lists it as unchanged. The shared-line and `#pragma` inputs are fixed today with the outputs now in case 9.

## thoenissen/Reihitsu#1336

### Changed requirements

- RH7101-004 — merged: `main`'s new text (layout through `FormatNodeInDocumentWithContextAsync`, FMT-PIPE-029; no shared line; one declaration only, SHARED-FIX-006), prefixed with the run's "Unless RH7101-005 or RH7101-006 withholds it". `Uses` is the union of both sides.
- RH7101-006 — clarified. The qualifier "has a documentation comment" now reads "a `///` or `/** */` comment in its leading trivia" (SHARED-DOC-004). A new counter-example shows `///` and `/** */` comments in front of later declarators: the fix is offered and moves each comment in front of its field. The output is `main`'s accepted case 2 of `rh7101-fix-through-context-entry-point.md`, measured today with the same result.
- FMT-PIPE-031 — the reasoning only. Its `Why` said the fix "must be offered exactly where formatting splits it", which RH7101-006 contradicts. It now says the fix must not be offered where formatting does not split, and that RH7101-006 also withholds it for a declaration that formatting splits. RH7101-006 is added to the pipeline spec's `Uses`. The normative sentence is unchanged.

The run's RH7101-006 fits `main`'s fix as follows. `main` offers the fix exactly where `ReihitsuFormatter.SplitsFieldDeclaration` is true (RH7101-005, FMT-PIPE-031), and the formatter still splits a documented declaration (FMT-STRUCT-022). RH7101-006 is therefore a condition of the fix on top of the formatter's answer, not a copy of a rule of the split. `main`'s analyzer changes (RH7101-001 syntax-error exclusion, RH7101-002 extension blocks) are independent of it.

### Examples for the tests

RH7101-006: the withheld example (both comment forms), the mode-`None` split, the new later-declarator counter-example, and the ordinary-comment counter-example. Facts for the test implementer: `main`'s `RH7101DoNotCombineFieldsAnalyzerTests` asserts a split for a declaration with a leading `///` comment in `VerifyDocumentedCombinedFieldsAreFixedAtMemberIndentation`, `VerifyMisalignedDocumentationCommentFixUsesFieldLineIndentation`, and `VerifyDocumentationCommentOnOpeningBraceLineFixUsesFieldLineIndentation`. These tests existed on `735d0db` and are covered by the maintainer's #1336 answer. The tests that Q1 concerns are listed there.

### Output questions

**Q1 — RH7101, a `/** */` comment in the gap in front of the declaration, on the line of another member.**

Input (documentation mode `Parse` or `Diagnose`):

```csharp
class C
{
    private int _p; /** X. */ private int _x, _y;
}
```

Real output today (measured), which is also `main`'s accepted after-output (case 7 of `rh7101-fix-through-context-entry-point.md`, approved 2026-11-05 after this run's approval). It is asserted by `FixMovesADocumentationCommentInTheGapInFrontWithTheFirstField` and, for the multi-line variants, by `FixStartsTheMembersBehindADocumentationCommentThatSpansLinesInTheGapsOnTheNextLine`, `FixStartsTheFirstFieldBehindADocumentationCommentThatSpansLinesInTheGapInFrontOnTheNextLine`, and `FixStartsTheMembersBehindACommentThatSpansLinesAndADocumentationCommentInTheGapsOnTheNextLine`:

```csharp
class C
{
    private int _p;
    /** X. */ private int _x;
    private int _y;
}
```

Why it is in question: `/** X. */` is in the leading trivia of `private` and documents the declaration `_x, _y` (SHARED-DOC-004). The maintainer's #1336 answer, written into RH7101-006, withholds the fix for exactly that: `_y` loses its documentation. The later case-7 answer on `main` shows the split for the same input. Two maintainer answers give different output for the same input. Requirements: RH7101-006, RH7101-004 ("a documentation comment goes with the member it documents"), FMT-PIPE-029.

- **A** — the fix is withheld (RH7101-006 as written); the diagnostic stays. The input stays as it is. The four tests above change their expectation in modes `Parse`/`Diagnose`, or are moved to mode `None`, where the comment is ordinary and stays behind `_p;` (RH7101-004).
- **B** — the fix is offered with the output above: RH7101-006 excludes a documentation comment that starts behind code on the line, and `_y` loses its documentation.

Recommendation: **A**. The #1336 answer and SHARED-FIX-003 decide whether a fix may take documentation from a field. Case 7 on `main` decided the layout of an offered fix and did not revisit that question; its change doc lists "a documented declaration" as unchanged and never weighed SHARED-FIX-003. B would also make RH7101-006 depend on where the comment starts, which no other requirement on documentation comments does. The specs are written on A, which the RH7101-006 text already states. The rest of the run is not blocked; only the four tests above wait for the answer.

### Internal choices

- RH7101-006 qualifier made explicit as leading trivia, with a counter-example of comments in front of later declarators. The alternative is to leave the qualifier implicit in the SHARED-DOC-004 reference. FC-002 asks for an example on each side, and the counter-example's output is already accepted on `main`.
- FMT-PIPE-031's `Why` reworded instead of left contradicting RH7101-006. The alternative is to leave `main`'s text, which would let two specs disagree on where the fix is offered.

## thoenissen/Reihitsu#1340

Nothing changed in the specs. `silence-uncorrectable-namespaces-and-regions.md` is `Accepted`. It gained `Visible output changes`: RH7003's report before and after, and RH7304 measured today, where both `Outer` directives are reported and indented by the fix. It also gained *The maintainer's answers* under `Decisions`. The substance is the same.

## Whole run

- Merge: `git fetch origin main` and `git merge origin/main` (`a59a003`) gave one conflict, in `sdd/specs/analyzer/RH7101.md`, with two hunks: the `Uses` row and RH7101-004. Both were resolved keeping both sides. Merge commit `02547f6`; the follow-up edits above are in the next commit. `sdd/specs/requirement-ids.txt` auto-merged; it is sorted, has no duplicate, and none of the run's six new IDs collides with an ID `main` added. Of the run's specs, only `RH7101.md` and `shared/code-fixes.md` changed on `main`.
- Architecture spec tests, after the edits: `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RequirementCoverageTests"`: 50 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, which lists exactly the six new requirements that still wait for tests: RH7003-005, RH7101-006, RH7303-005, RH7304-005, RH7306-004, RH7309-006. This is the same result as round 2.
- Rule pages: unchanged.

### Follow-up candidates

_None new._

### Architecture findings

_None._ RH7101-006 adds a condition that only the fix owns: the formatter keeps splitting a documented declaration. It does not restate FMT-PIPE-031 or FMT-STRUCT-023, so no rule is stated in two places. The skeptic's earlier note named a future transform guard (thoenissen/Reihitsu#1384). That guard would put the decision in two places only if the formatter also stopped splitting documented declarations, and nothing on `main` does that.
