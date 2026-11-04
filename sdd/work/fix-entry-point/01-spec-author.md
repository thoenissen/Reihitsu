Role: spec author
Commit: 15fc57c
Verdict: change doc written

Change doc: `sdd/changes/2026-11/rh7101-fix-through-context-entry-point.md` (`Status: Draft`). Its *Visible output changes* lists nine RH7101 code-fix results, each measured before (`scripts/apply-fix.sh RH7101` at 15fc57c) and derived after; they are the run's output questions.

## Checks of the issue against the tree (main df4f828 = 15fc57c)

- True: the RH7101 fix runs `FieldDeclarationSplitTransform` alone on the whole type (`RH7101DoNotCombineFieldsCodeFixProvider.ApplyCodeFixAsync`); the RH7201–RH7207 fixes run `UsingDirectiveOrderingPhase.Execute` alone (`UsingDirectiveCodeFixUtilities.OrganizeScopeUsingsAsync`); `GetMemberIndentationTrivia`, the "RH7101 code fix runs this transform on its own" comment, and `GetScopeIndentation` exist.
- Moot: "FMT-STRUCT does not mention the RH7101 dependency". FMT-STRUCT-026 already records the field split's layout as an exception for the RH7101 fix, naming thoenissen/Reihitsu#1502. That exception is now replaced.
- Wrong for the usings: "no spec lists these paths". FMT-USING's scope sentence names the RH7201–RH7207 dependency, and FMT-USING-003 and SHARED-USING-010 specify the scope indentation of a split-off directive as visible in the fixes. The FMT-USING non-goal states both that the phase writes the scope indentation and that the indentation phase decides the final column. These do not contradict each other.
- Partly wrong: "Inside the pipeline that layout is redundant". Measured with `scripts/format.sh` on shapes without the split's layout: the indentation phase redoes the indentation, and the line-break phase redoes the line break in front of a field that has no comment (`_x;private int _y;` gets split). Two things are not redone. The spacing phase keeps the gap in front of a trailing comment as written (`_x;// h` stays). The line-break phase leaves a leading comment on the member's line (`/* v */private int _y;` stays). So the split must keep writing one space in front of a comment that trailed a comma, and the line break behind a comment it moves. Dropping either would change the output of document-level formatting.
- Not achievable: "The fixes' output stays as it is today", and the Done-when item "the code-fix tests of the eight rules keep their outputs, and formatting each fix's output changes nothing". For RH7101 these two goals contradict each other. Today's output changes when formatted in cases 1–7 of the change doc, and existing tests assert some of those outputs: `VerifyCommentBeforeFirstDeclaratorIsPreservedByTheFix`, `VerifyFixKeepsTheOwnIndentationOfTheCommentBeforeFirstDeclarator`, `VerifyFixKeepsTheIndentationOfAFirstDeclaratorWithoutAComment`, `VerifySingleLineDocumentationCommentBeforeDeclaratorGetsNoBlankLine`, `VerifyMultiLineDocumentationCommentBeforeDeclaratorKeepsItsLineBreak`. In the corner cases the spec now follows SHARED-FIX-006 and SHARED-FIX-007, which have not changed. The plain cases keep their outputs: the issue's example, the documented declaration, the misaligned doc comment, the doc comment on the brace line, a delimited doc comment before `;` or `,`, and comments trailing the commas.
- Found by measurement, not stated in the issue: today one RH7101 fix splits every combined field of the type and of its nested types (case 8, violating SHARED-FIX-006). And through the context entry point, the fix would become an offered no-op in a document with a syntax error (case 9; FMT-PIPE-005 vs. SHARED-FIX-005). The RH7101 analyzer reports in such documents (`RH7101DoNotCombineFieldsAnalyzer` has no syntax-error guard).

## Changed requirements

- FMT-PIPE-029 — The context entry point writes back every field the split makes of its target — added
- FMT-STRUCT-026 — The phase writes only the separators its text needs — changed (the field-split exception becomes the split's decided layout; Why extended; intro and non-goal of `structural-transforms.md` adjusted; `Uses` += FMT-PIPE-029)
- RH7101-004 — The fix splits the declaration — changed (formatter layout through FMT-PIPE-029, only the reported declaration, shared-line case; `Uses` += FMT-PIPE-005, FMT-PIPE-017, FMT-PIPE-029, SHARED-FIX-005, SHARED-FIX-006, SHARED-FIX-007)
- RH7101-006 — The fix is withheld in a document with a syntax error — added
- `pipeline.md` `Uses` += FMT-STRUCT-023. Ledger: FMT-PIPE-029 and RH7101-006 inserted.

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"`: 36 passed, 1 failed. The one failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for FMT-PIPE-029 and RH7101-006 only, which need tests.

## Examples for the tests

- FMT-PIPE-029: its three examples (comment and blank line plus untouched other fields, the off-column target, and the shared line), each through `FormatNodeInDocumentWithContextAsync` with the field as target and the class as context.
- RH7101-004: the existing two examples (unchanged outputs), plus the new ones: the comment in front of a later declarator, one declaration split while the other keeps its diagnostic, the shared line, and the off-column declaration. The change doc's cases 2–5 give more inputs (doc comments, first declarator on a later line, spacing, attribute lists).
- RH7101-006: its example (reported, no fix offered).
- FMT-STRUCT-026: its new field-split sentence gives three phase-output shapes. FMT-STRUCT-021 and FMT-STRUCT-022 document-level outputs must stay unchanged.

## Output questions

All nine are RH7101 code-fix results listed in the change doc with input, before, and after. For cases 1–5 and 8, the changed output is what the unchanged SHARED-FIX-006 or SHARED-FIX-007 already demand. Each is still a visible change, and the issue asked to keep today's output. Cases 6, 7, and 9 each have a competing reading:

1. **Case 6, a declaration behind another member on its line**, `private int _p; private int _x, _y;`. Before: `_p; private int _x;` with `private int _y;` in column 0. A (recommended, in the spec): `_p;`, `_x;`, and `_y;` on three lines at the member column. This is SHARED-FIX-007's shared-line bullet. B: `private int _p; private int _x;` with `    private int _y;`. This is FMT-PIPE-018's keeping of the target's leading trivia. It needs no shared-line clause in FMT-PIPE-029, but leaves a shared line that SHARED-FIX-007 forbids. Touches FMT-PIPE-029 and RH7101-004.
2. **Case 7, a declaration off its member column**, `      private int _x, _y;` (6 spaces). Before: both fields in column 6. A (recommended, in the spec): `_x` stays in column 6 and `_y` goes to column 4, because the context's depth places it there (FMT-PIPE-017). B: both in column 6 (today). B would need FMT-PIPE-029 to place later fields relative to the first field instead of by the depth. Touches FMT-PIPE-029 and RH7101-004.
3. **Case 9, a document with a syntax error elsewhere.** Before: the fix is offered and splits. A (recommended, in the spec): the diagnostic stays and no fix is offered (RH7101-006). B: keep today's split with the transform's own layout for such documents, which keeps the code-fix-only layout this change removes. Offering a fix that changes nothing is ruled out by SHARED-FIX-005.

## Internal choices

- The entry point is the existing `FormatNodeInDocumentWithContextAsync`, with FMT-PIPE-029 added, not a new entry point. Alternative: a new "run one phase or transform" entry point with its own layout contract. That alternative keeps a second layout and is rejected in the change doc.
- The field split keeps one space in front of a comment that trailed a comma, and the line break behind a comment it moves. Both are now stated in FMT-STRUCT-026 as FMT-STRUCT-022's layout. This narrows the issue's proposal ("no line break beyond the separators"), because without them document-level output changes. Alternative: drop them and change CLI output, which is a visible change that nothing asks for.
- The RH7201–RH7207 fixes keep running the using-directive phase. FMT-USING, SHARED-USING-010, and `GetScopeIndentation` are unchanged, and the issue's using goals are dropped (its Done-when items on RH7201–RH7207 and `GetScopeIndentation`). The reasons are in the change doc, *Decisions*. Alternative: format the scope as a context and write back the block. That changes the fixes' output against FMT-USING-010 and SHARED-FIX-006, with a blank line inserted above a leading comment and directive interiors reformatted, and puts seven rules' outputs in question.
- The Done-when item "RH7101 calls no formatter phase or transform directly" is read as "runs none". The fix may keep calling the static guard `FieldDeclarationSplitTransform.CarriesDirective`, which is the single statement of FMT-STRUCT-023 that RH7101-005 relies on. Alternative: move the guard to `Reihitsu.Core`. That would add a code owner to FMT-STRUCT and is the implementer's call.
- SHARED-FIX-007 is not amended. Its "changed code formatted within a larger node" bullet and its "shared line" bullet both apply to RH7101. FMT-PIPE-029 follows the shared-line bullet for members.
- The rule page `documentation/rules/RH7101.md` is unchanged. What is reported and how to fix it stay the same in the normal case. The withheld fix in syntax-error documents is covered by `general-notes.md`.

## Non-goals

- The RH7201–RH7207 fixes, FMT-USING, SHARED-USING, and the using phase's code.
- Any change of document-level output (`FormatDocumentAsync`, `FormatSyntaxTree`, CLI) for combined fields.
- Fix All behavior beyond what SHARED-FIX-002 already states.

## Follow-up candidates

- **Code fixes whose entire edit is a node-level formatting call may be offered as no-ops in a document with a syntax error** (SHARED-FIX-005). Examples are RH5111 (`FormatNodeInDocumentAsync` / `FormatNodeInDocumentWithContextAsync`) and RH5417 (`FormatNodeInDocumentAsync`), because the entry points return the document unchanged on a syntax error (FMT-PIPE-005). Unverified: whether their analyzers report in such documents. Needs a check before filing.
- Already tracked, not new: `new int[]{3}` formats to `new int[] {3 }` (initializer brace spacing), which FMT-SPACE-022's Why refers to thoenissen/Reihitsu#1440.
