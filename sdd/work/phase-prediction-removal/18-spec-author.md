Role: spec author
Commit: f360da2
Verdict: change doc written

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It told me to apply the maintainer's decisions 1B and 2A after audit 1, recorded in `00-lead.md` (last section) and in `squad/decisions/2026-10-25-phase-prediction-removal-audit-1.md`, together with the spec side of audit findings 4 and 6. This round does that.

- Outputs called "before" were observed on c49431e, in the scratchpad worktree.
- Outputs called "head" were observed on f360da2.
- Both were observed with `scripts/format.sh`. Columns are 0-based.
- Nothing about the node-level entry points was run. The intended behavior below is derived from document-level output, which I checked.

## Change doc

`sdd/changes/2026-10/phase-prediction-removal.md` is now **`Status: Draft`**. Its What and Why changed, so the lead has to record the maintainer's approval again. Exactly what changed:

- **Affected requirements**
  - Added: FMT-PIPE-017 (new), FMT-INDENT-006 (clarified), RH5303-005 and RH5304-004 (one example each).
  - RH5303-006 and RH5304-005 are now listed as "added and retired by this change".
- **What**
  - The RH5303/RH5304 bullet no longer says the fixes are withheld. It now says they lay out the element behind a multi-line comment's last line as the formatter does.
  - New bullet: "The node-level entry points take every column from the indentation phase" (FMT-PIPE-017).
  - New bullet: "A comment line above a comment-led line takes that line's column, also inside an interpolation hole" (FMT-RAW-007, FMT-INDENT-006).
  - The pipeline paragraph now names three new pipeline requirements instead of two.
  - New third table for the node-level changes:
    - an RH3202 fix of a tab-indented member: `int Get()` goes from column 2 to 8. This is the skeptic's observation a01.
    - an RH3202 fix of a raw-string member: the content and closing lines end up under the quotes instead of at 19 under quotes at 13. This is the skeptic's observation a02.
    - the RH5303 fix on a complex element behind `/* a` / `b */ {`: today 24/20; after this change 32/28, which is what document-level formatting gives. With the move kept it would be 38/34.
    - A sentence covers the other members of the auditor's table.
  - New sentence with the hole-comment output: `// above` goes to column 17, under `/* c */`, instead of column 25, under `x`. Fixture: `$"""` / `    a {` / `    // above` / `    /* c */ x` / `    } b` / `    """;` behind `        var a = `.
- **Why**
  - New paragraph: the node-level move is a fourth disagreement, and it moves columns after the phases.
  - Inventory row `ComputeTokenColumn` / `AdjustNodeIndentation`: "Predicts" now says the indentation phase's columns, and "After this change" says that no line moves after the last phase (FMT-PIPE-017). It no longer says "Not changed here".
- **Decisions**
  - The paragraph that kept the node-level step and withheld the fixes is removed.
  - New decision: "The node-level entry points lay out their node at its own column". It records that the narrow guard came first and is now retired.
  - New decision: "A comment line above a comment-led line takes that line's column everywhere".
  - "Changed output is only output that violates an existing requirement today" now names the two outputs that change on purpose.
- **Alternatives**
  - The rejected option "change the node-level step in this change" is replaced by four rejected alternatives:
    - the narrow withholding (RH5303-006/RH5304-005);
    - a wider withholding (option 1A);
    - an exception in FMT-INDENT-030 for comment tails (option 1C);
    - keeping the hole comment under the token (option 2B).

## Changed requirements

- **FMT-PIPE-017 — Node-level targets take their columns from the indentation phase — added.**
  - `FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync` lay out the node they format (the target or the context node) from the column its first token has in the document.
  - That column counts each tab in front of the token on its line as four columns (FMT-TAB-001). The first token stays in that column.
  - No line of the node moves after the last phase. Every line ends in the column the indentation phase gives it. This explicitly covers:
    - a line that starts inside a comment that began on an earlier line;
    - the lines aligned to an anchor on such a line;
    - a raw string literal's content, hole, and closing lines.
  - This states what the entry points guarantee, not how to achieve it.
  - Examples:
    - an `EqualsValueClauseSyntax` target with the complex element behind `/* a` / `b */ {` gives 32/28;
    - an `= """` target gives content and closing marker under the quotes at 16;
    - a tab-indented statement target gives `Call(1,` at 8 and `2);` at 13.
    - The expected output of each is the document-level output, which I checked with `format.sh`.
  - Ledger updated. `pipeline.md` `Uses`: + SHARED-FIX-007.
- **RH5303-006 and RH5304-005 — retired**, each with heading and `Status: Retired`. See the next section.
- **RH5303-005 and RH5304-004 — changed (examples only).** One example each: the element behind `/* a` / `b */ {` is laid out from the brace, at 32/28 and 40/36. Both outputs match document-level formatting on f360da2.
- **FMT-RAW-007 — changed** (decision 2A).
  - Bullet 2: when another comment leads the line of the token, the comment above takes the column that line starts in, as on any other line. The column of the token behind the leading comment does not decide it.
  - New example: the hole fixture above. f360da2 gives 17; c49431e gave 25.
- **FMT-INDENT-006 — clarified.** A comment above a line that another comment leads takes the leading comment's column, not the column of the token behind it.
  - This is current behavior on ordinary lines. The auditor saw it in both versions, and I observed it on f360da2: `  // above` over `            /* c */ M();` gives both at 8.
  - It gives FMT-RAW-007 a rule to refer to.
- **`RH5303.md` / `RH5304.md` `Uses`**: − SHARED-COMMENT-012, SHARED-FIX-001, SHARED-FIX-007; + FMT-PIPE-017, because the retired texts cite it. `UsesListsExactlyTheReferencedRequirements` counts references in retired text too.
- **`AGENTS.md`** (audit finding 6): the architecture sentence now reads "No phase computes a column from a move or replacement that a later phase applies, and no step after the last phase moves a line, also in the node-level entry points: the indentation phase owns every line's final column." With FMT-PIPE-017 this is true, and it no longer goes beyond the specs.

## Do RH5303-006 and RH5304-005 still have a reason to exist?

No. Their only reason was that the node-level step moves every other line after the phases but cannot move a line that starts inside a comment. FMT-PIPE-017 removes that step's move. With it, the fix lays out the element exactly as document-level formatting does. RH5303-005 and RH5304-004 (SHARED-FIX-007) then hold, and SHARED-FIX-001 holds with them.

A guard would now only withhold fixes it could apply safely. `AGENTS.md` *Code-fix coverage* calls that a defect of the guard's scope. Both are retired as `sdd/README.md` prescribes: heading kept, `Status: Retired` with the reason, ledger lines kept. The guard `InitializerLayoutAnalysisUtilities.ContainsComplexElementBehindMultiLineCommentTail` and its calls in both providers have to go. That is implementer work.

## Examples for the tests

- **FMT-PIPE-017**: the three examples.
  - Add the auditor's members, one test each, with the fix output equal to document-level output:
    - RH5303 on `new P` / `{ X = 1 }`, on `new List<int>` / `{ 1 }`, on `Call(1,` / `2)`, and on `x` / `&& y` behind the tail, with an equals-value target;
    - RH5303 on `new P` with an arrow-expression target;
    - RH5304 on `new P`;
    - RH5301 on the complex element inside `D = new Dictionary<int, int> { … }`.
  - Add the skeptic's a01 and a02 through RH3202. Those outputs are in the change doc's third table.
- **RH5303-005 / RH5304-004**: the new tail examples.
- **FMT-RAW-007**: the new hole-comment example (17). **FMT-INDENT-006**: the inline ordinary-line example.
- **Tests that tag a retired ID.** `EveryRequirementReferenceNamesAnActiveRequirement` fails on these. Retag them RH5303-005 / RH5304-004, and rewrite the "no code fix is offered" ones to expect the fix:
  - RH5303:
    - `VerifyComplexElementBehindTheLastLineOfAMultiLineCommentIsDetectedAndNoCodeFixIsOffered`
    - `VerifyComplexElementBelowAMultiLineCommentThatEndsAboveItsBraceIsFixed`
    - `VerifyMisplacedBracesAroundComplexElementBehindTheLastLineOfAMultiLineCommentAreDetectedAndNoCodeFixIsOffered`
    - `VerifyMisplacedBracesAroundComplexElementBehindLeadingCommentAreDetectedAndFixed`
    - `VerifyComplexElementBehindTheLastLineOfAMultiLineCommentInAssignmentStatementIsDetectedAndNoCodeFixIsOffered`
    - `VerifyComplexElementBehindTheLastLineOfAMultiLineCommentInExpressionBodyIsDetectedAndNoCodeFixIsOffered`
    - `VerifyFixAllFixesOnlyTheCreationWithoutComplexElementBehindTheLastLineOfAMultiLineComment`. Its fixed state should now hold no remaining diagnostic.
  - RH5304:
    - `VerifyComplexElementBehindTheLastLineOfAMultiLineCommentIsDetectedAndNoCodeFixIsOffered`
    - `VerifySplitAssignmentNextToComplexElementBehindTheLastLineOfAMultiLineCommentIsDetectedAndNoCodeFixIsOffered`
    - `VerifySplitAssignmentNextToComplexElementBehindLeadingCommentIsDetectedAndFixed`
    - `VerifyComplexElementBelowAMultiLineCommentThatEndsAboveItsBraceIsFixed`
  - The `MarkupMode.Allow` change of cceed9a, which let a withheld diagnostic remain in the fixed state, should be reverted where it only served the guard.

`scripts/test.sh --configuration Debug --project architecture` on the working tree: 87 passed, 2 failed. Both failures wait on the tests above:
- `EveryRequirementOfAnEnforcedSpecIsCovered`: FMT-PIPE-017 has no test.
- `EveryRequirementReferenceNamesAnActiveRequirement`: the 11 tests listed.

Audit findings 2, 5, and 7 have no spec side. Finding 2 is a code defect against FMT-DOC-001 and FMT-PIPE-010. Finding 5 is comments in a test. Finding 7 is a test or a comment.

## Open questions for the maintainer

**Q1 — Column of a raw string's opening quotes on a line that starts with U+FEFF (audit finding 4).**
- Input, through `FormatSyntaxTree`: `﻿var s = """` / `    a` / `    """;`.
  - Before (c49431e): content and closing marker at character column 9, under the quotes counted with the BOM.
  - Head: column 8, because the layout treats the BOM as zero-width, as it does for every other alignment on that line. RH5202 counts characters and reports the head output.
- Only string input reaches this case. The CLI and workspaces strip the BOM when they decode.
- No spec decides the column convention of a BOM-led line. FMT-CLEAN-009 and FMT-DOC call the BOM an encoding signature, not indentation.
- Options:
  - **A**: the BOM is zero-width everywhere. Keep the head output and change RH5202 to measure the same way. The output changes for BOM-led raw strings, which the change doc does not list.
  - **B**: restore the before output. The raw-string move on a BOM-led line counts the BOM as one column, as RH5202 and the removed phase did. Other alignments stay as they are, and no analyzer changes.
- **Recommendation: B.** The approved change doc allows only the output changes it lists. This one is neither of its kinds, so B is a repair against the change doc. It needs no spec text, which I did not write because the visual convention is debatable. A would widen the change into RH5202 for a corner that only string input reaches.

**Q2 — A tab in front of a node-level target that does not start its line.**
- FMT-PIPE-017 is written so that the target's first token column counts each tab in front of it as four columns (FMT-TAB-001). For a target that starts its line, the tabs are in its own leading whitespace, and only "four" fixes a01.
- For a target behind code, the tabs are outside the target and are kept as written (FMT-PIPE-010). Two readings then give different output for `→→var d = new List<int>` / `{ … }` with the `EqualsValueClauseSyntax` as target:
  - **A**: count four, as written. `new` counts as column 16, so the braces get 16 spaces. The braces line up with `new` in a four-column tab view and after RH5601 replaces the tabs, but a character-counting rule sees `new` at 10 until then.
  - **B**: count characters for what lies outside the target. The braces get 10 spaces and are aligned in character columns now, but they are off by 6 once the two tabs become spaces. B also needs a second rule for a target that starts its line.
- **Recommendation: A**, as written. It is one rule, consistent with the phases and with FMT-TAB-001. The tab that remains is reported by RH5601, which fixes it. If the lead takes B, the tab sentence of FMT-PIPE-017 has to change. The first row of the change doc's third table is about the target's own whitespace and stays.
