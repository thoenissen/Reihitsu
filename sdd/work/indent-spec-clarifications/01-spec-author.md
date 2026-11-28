Role: spec author
Commit: 0d8c759
Verdict: needs a test

The spec text for both issues is written in `sdd/specs/formatter/indentation.md`. No intended behavior changes, so no change doc. Every example was run through `scripts/format.sh` on `0d8c759`, and each output shown is today's output. Two of the new examples are not pinned by any existing test (see #1400 below). The issue's "Done when" allows characterization tests for them, so the lane has to widen to tests for those two examples.

My prompt carried no intended fix or suspected cause.

## #1400 — formatter-spec clarifications from round 9

Items 22 and 23 are written. Item 24 needed no edit: FMT-SPACE-001 already says that "A line break inside the text of a multi-line comment is not a line break of the gap", which matches the issue comment of 2026-11-16.

### Changed requirements

- FMT-INDENT-010 — Region directives are indented like the members around them — clarified. The old sentence "A region directive in disabled text MUST NOT be touched" now reads: a region directive in a branch the build does not compile MUST NOT be re-indented and keeps its column. It is still a directive and not disabled text, so tab replacement (FMT-TAB-001, FMT-TAB-002) and region-text formatting (FMT-REGION-006, FMT-REGION-007) apply to it. The `Why:` explains why. Two examples were added: one in code blocks, where the directives stay in columns 2 and 10 and their names are formatted, and one in prose for the tab case, which is the issue's example.
- FMT-INDENT-028 — Lambda block bodies align to the lambda — clarified. A new paragraph says the anchor is `async`, or else the first token of the parameters, and never the lambda's first token. A `static` modifier, an attribute list, and an explicit return type are never the anchor. It covers `static x`, `int (x)`, `static int (x)`, `[A] (x)` and `static async x`. The `Why:` records the maintainer's round 9 decision. One example was added: `int (x)`, `static int (x)`, `static x`.
- The `Uses` row of `indentation.md` gained FMT-REGION-006, FMT-REGION-007, FMT-TAB-001, FMT-TAB-002, FMT-TAB-003.

Verified outputs that are not shown as code blocks in the spec: `static async x => {…}` puts `{` under `async`. `[Obsolete] (x) => {…}` puts the attribute list on its own line (RH5513 or the attribute placement does that, not this requirement), and `{` then sits under `(`. With tabs, `#if NEVER⏎→#region inner⏎→disabled⏎→#endregion⏎#endif` becomes `····#region Inner⏎→disabled⏎····#endregion // Inner`.

### Test coverage of the confirmed behavior

- Item 22, not re-indented: **not pinned**. The tests in `DisabledRegionLayoutTests` (FMT-REGION-006) all put the inactive region in the member column, so they cannot tell "not re-indented" apart from "re-indented". A characterization test for the new FMT-INDENT-010 example (columns 2 and 10 kept) is needed.
- Item 22, tab in front of an inactive region: **not pinned**. `TabReplacementPhaseTests.TabInFrontOfEndifAfterDisabledTextIsReplacedAndTabInsideDisabledTextIsKept` covers the tab in front of an active `#endif` only. A characterization test for the tab example (whole pipeline) is needed.
- Item 22, region text in inactive branches: already pinned by `DisabledRegionLayoutTests.RegionInsideDisabledCodeIsRenamedWithoutBlankLinePadding`.
- Item 23, explicit return type: already pinned by `LambdaReturnTypeJoinTests.SpecExampleReturnTypeJoinsTheParameterListInEveryPosition`, where the `m` lambda's block sits under `(` across the whole pipeline. `static int (x)` and `static x` with a block body are pinned by no named formatter test, at most indirectly, if the self-hosting formatter check covers repository source such as the `static test =>` lambdas in the analyzer tests (not verified). A characterization test for the new FMT-INDENT-028 example would pin all three. Tag it FMT-INDENT-028.
- Item 24: pinned by `MultiLineCommentGapSpacingTests.SpaceBehindMultiLineCommentInFrontOfBinaryOperatorBecomesOneSpaceAsBehindSingleLineComment`, tagged FMT-SPACE-001 and FMT-SPACE-013.

## #1936 — FMT-INDENT-021 Why text

### Changed requirements

- FMT-INDENT-021 — A wrapped `when` clause of a case label is indented one level — clarified, `Why:` only. It now says:
  - The guard line is a continuation of the label and takes the column FMT-INDENT-034 gives a continuation line.
  - At the label's own column it would look like another label.
  - The section's statements share that column (FMT-INDENT-003), and the colon that ends the label tells the two apart.
  
  The requirement sentence and the example are unchanged. I confirmed that the example's output puts `when` and `break;` in column 16, and that the `{` of a braced section sits there too.

### Test coverage

Pinned by `ListAndHeaderAlignmentTests.WrappedWhenClauseIsIndentedOneLevelDeeperThanCase` and `CaseWhenClauseContributorTests.AlignsWrappedWhenKeywordToCasePlusIndent`. The change is text only, so no test is needed.

## Examples for the tests

- FMT-INDENT-010: both new examples. The code-block example and the tab example are each one whole-pipeline characterization test.
- FMT-INDENT-028: the new second example, one whole-pipeline characterization test.

## Output questions

_None._ Every case confirms today's output, as the maintainer decided in round 9 (#912 items 22, 23, 24, option (a)).

## Internal choices

- Item 24 is left without an edit, because FMT-SPACE-001 already states it and a test pins it. The alternative was to add the issue's `a   /* one⏎    two */    + 1` example to FMT-SPACE-001.
- In FMT-INDENT-010, the inactive directive is called "a directive, not disabled text" instead of keeping the phrase "in disabled text", because Roslyn parses it as an inactive directive trivia. The alternative was to keep the old phrase and only add "not re-indented".
- The `Why:` of FMT-INDENT-021 points to FMT-INDENT-034 as an analogy for the column, while FMT-INDENT-021 stays the owner of the column. The alternative was to state the column without a reference.

## Follow-up candidates

_None._ A region in one inactive `#if` paired with an `#endregion` in another left the whole file unchanged. That is the unpaired-region rule (FMT-PIPE-005), not a defect.

## Lane check

`scripts/lane-check.sh docs --head worktree` (before this handover was added):

```
TEXT-ONLY PROOF: PASS — base=c337700 head=worktree — 2 changed file(s) (2 non-compiled, 0 C# comment/layout-only); xml-doc impact: none
LANE CHECK: confirmed — claimed docs, diff needs docs
```

`scripts/test.sh --project architecture`: 247 passed, 0 failed.
