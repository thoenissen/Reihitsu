Role: spec author
Commit: 1f677d9
Verdict: spec written

No change doc: the lead claimed the tests lane, and the issue and the maintainer's clarifications classify the new requirements as written-down policy decided under thoenissen/Reihitsu#912, not new intended behavior. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements

- SHARED-FIX-002 — Fix All corrects everything in one application — clarified: covers the fixes offered on the code the application starts from. A fix that a guard releases only after another fix has run is not covered. New RH7103 example (thoenissen/Reihitsu#1110 Q3, reading b).
- SHARED-FIX-003 — A fix that could break the code is withheld — clarified: the P23 sentence follows "The diagnostic is still reported." The RH3202 layout example is replaced. The new example keeps the raw-string re-indent of `N` and adds two blank lines above a get-only `GetValue()` that stay two. The old documentation-comment reflow of `M` is gone, because SHARED-FIX-006 now forbids it (P14d).
- SHARED-FIX-006 — A fix changes only the code its diagnostic is about — added (P9, P14a, P14d).
- SHARED-FIX-007 — The changed code is laid out as the formatter lays it out — added (P10, P14b, P14c).
- SHARED-FIX-008 — A fix keeps the document's line ending — added (P24).
- FMT-PIPE-010 — `Why:` cites SHARED-FIX-006 instead of restating it. `formatter/pipeline.md` `Uses` changes from `—` to `SHARED-FIX-006`, as `SpecStructureTests` requires.
- `shared/code-fixes.md` non-goals: "How a fix lays out the code it changed — FMT-PIPE-010" contradicted SHARED-FIX-007, so it now reads "How the formatter's node-level entry points keep the code around their target — FMT-PIPE-010". A new non-goal leaves out the line ending a fix writes into a document that has no line break or already mixes line endings. The decided P24 text only covers a document that "already uses" one line ending.

The ledger `sdd/specs/requirement-ids.txt` gains SHARED-FIX-006, -007 and -008 after SHARED-FIX-005. No ID is retired, moved or renumbered. `sdd/specs/README.md` needs no change, because no prefix or code owner changed.

Wording choices beyond the decided texts. Each one keeps the new requirements consistent with existing ones and is open to challenge:

- SHARED-FIX-006 blank lines (P14a) adds "unless those blank lines are the code its diagnostic is about". Without it, P14a would forbid the RH5002 fix that SHARED-FIX-001's own example shows, and every other blank-line rule's fix. It also adds a pointer: the blank line left by a removed directive line is SHARED-FIX-007's. Without that pointer, P14a and P14c contradict each other for RH7310.
- SHARED-FIX-007 shared line (P14b) adds the other side of the qualifier: around a member that has its line to itself, SHARED-FIX-006 applies. The "(SHARED-FIX-007)" self-reference in the decided text is dropped, because the sentence now sits inside SHARED-FIX-007.
- SHARED-FIX-007 removed directive line (P14c) adds the other side: a single blank line that the removal leaves between two lines stays. This is the RH7310-004 behavior.
- SHARED-FIX-008 adds: a line break inside a literal is part of the literal's value and stays as it is (SHARED-FIX-003). Without this, P24's "every line break the fix keeps" would require changing a literal's value.
- SHARED-FIX-006 carries a second P14d example besides the decided "two blank lines" one. It is thoenissen/Reihitsu#1284 instance 1 (RH3203: `///<summary>x</summary>` and `[Obsolete] public  C( int value ) => _value = value;`, where only the body becomes a block). The decided "two blank lines above a get-only method" example sits in SHARED-FIX-003, as the lead's scope says, and SHARED-FIX-006 points to it.

Document-formatting facts the examples rely on. I took them by running `scripts/format.sh` on scratch copies outside the repository:
- `M(⏎"""⏎a⏎""",⏎2);` becomes `M("""` / `a` and `"""` at the column of the opening quotes / `2);`. This is the SHARED-FIX-007 RH5101 example.
- `_ = a + M(a, () => { return 1; });` gets its lambda broken over several lines by document formatting. The SHARED-FIX-006 example relies on this as a contrast.
- `void M() { } int _x;` becomes `void M()` / `{` / `}` / `int _x;` with no blank line. This is the SHARED-FIX-007 RH5417 example.
- `return """⏎a⏎""";` aligns the content and the closing quotes under the opening quotes. This is the SHARED-FIX-003 layout example.

Verification: `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"` gave 38 passed and 1 failed. `SpecConsistencyTests` and `SpecStructureTests` pass. The only failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for SHARED-FIX-006, SHARED-FIX-007 and SHARED-FIX-008, which have no tagged test yet.

## Examples for the tests

SHARED-FIX-002, SHARED-FIX-006, SHARED-FIX-007, SHARED-FIX-008.

Facts about the examples relative to today's code. These come from the follow-up issues and from existing tests; I did not run the new examples as tests.
- Examples the follow-up issues name as not yet met, so they are not seeds for tagged tests in this run:
  - SHARED-FIX-006 RH6024 lambda (thoenissen/Reihitsu#1141).
  - SHARED-FIX-006 RH3203 attribute and documentation comment (thoenissen/Reihitsu#1284).
  - SHARED-FIX-003 two blank lines above `GetValue` (thoenissen/Reihitsu#1284; today the two collapse to one).
  - SHARED-FIX-007 RH5101 raw string and RH5417 shared line (thoenissen/Reihitsu#1144, thoenissen/Reihitsu#1170 Q1).
  - SHARED-FIX-007 RH7310 start and end of body (thoenissen/Reihitsu#1263).
- Examples whose input and output equal existing passing tests:
  - SHARED-FIX-006 blank-line example: RH3202-005, `VerifyBlankLineAboveFixedMethodIsKept`.
  - SHARED-FIX-007 RH7303 example: RH7303-004, `CodeFixRemovesBothDirectiveLinesBetweenStatements`.
  - SHARED-FIX-007 RH7310 between-members example: the `Methods` half is RH7310-004's example, `CodeFixRemovesTheRegionAndTheBlankLinesAfterIt`.
  - SHARED-FIX-008 RH5405 example: `VerifyFixWrapsBodyOnLineOfIfInBracesWithCarriageReturnLineFeed`, which is tagged SHARED-BRACE-002 only.
  - SHARED-FIX-002 RH7103 example: thoenissen/Reihitsu#1110 Q3 reports this as today's first-Fix-All output.
- No existing test shows a conforming shape for the "shares its line" side of the SHARED-FIX-007 shared-line bullet. The requirement as a whole has conforming shapes, listed above.
- Existing tests now pin behavior that SHARED-FIX-006 forbids. The issue assigns their change to the bug issues:
  - `RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.VerifyFixChangesOnlyTheLayoutOfDocumentationCommentsAndRawStringLiterals`, tagged SHARED-FIX-003. It pins the reflow of `M`'s documentation comment, which no longer appears in the spec example; the tag itself remains valid.
  - `RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.VerifyFixCollapsesTwoBlankLinesAboveTheMethod`, untagged characterization.
  - The tests named on thoenissen/Reihitsu#1140 (comment of 2026-10-20) and thoenissen/Reihitsu#1335.

## Open questions to file

**Does the SHARED-FIX-007 shared-line bullet (P14b) apply to a fix that does not lay out the member?** This touches SHARED-FIX-006 and SHARED-FIX-007. No issue of the form `SDD open questions: sdd/specs/shared/code-fixes.md` is open; thoenissen/Reihitsu#1066 had that role and is closed, so the lead reopens it or opens a new one.
- Input: an RH4106 rename on `class C⏎{⏎    private int {|RH4106:Value|}; private int _b;⏎}`.
- Real output: not run. The RH4106 fix is a rename, and I expect `private int _value; private int _b;` on one line.
- Why it looks wrong: either way, one of the two new requirements is violated. Read literally, P14b says the changed member `_value` still shares its line with `_b` after the fix, so it must get its own line. P9 and P14d (SHARED-FIX-006) say the fix changes only the name and must not format the member or the code around it.
- Readings:
  - (a) P14b applies only when the fix lays out or replaces the member. Output: `private int _value; private int _b;`.
  - (b) P14b applies to every changed member. Output: `private int _value;` and `private int _b;` on separate lines.
- This run's tests do not depend on the answer. The spec's P14b example (RH5417) gives the same output under both readings.

## Follow-up candidates

- The RH7501 exception to P14a, decided in round 8 item 22a (thoenissen/Reihitsu#1332), is not written. That issue's run adds it.
- P26, the narrowing of SHARED-FIX-007 for moved code, is not written, as the lead's scope says (thoenissen/Reihitsu#1331).
