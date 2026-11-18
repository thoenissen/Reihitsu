Role: implementer
Commit: 9ec0262
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The structure follows decision 3 in `00-lead.md`: one owner joins the attached-part gaps, and it runs before the list layout.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Core/Utilities/AttachedPartGapUtilities.cs` (new) | This is the one statement of an attached part's gaps, decided by the syntax tree alone. A gap counts when the token behind it is one of these: the `(` of the argument lists FMT-BREAK-039 names; the `[` of an `ElementAccessExpressionSyntax`; the `?` of a conditional access followed by an element binding's `[`; the `!` of a null-forgiving operator not followed by a member access's `.` or by `?.`; or an element binding's `[` (the `?`–`[` gap). Both the line-break phase and the precheck read it. | FMT-BREAK-039, FMT-BREAK-040, FMT-UNSUP-014 |
| `.../LineBreaks/Rewriter/ArgumentListLineStartRewriter.cs` → `AttachedPartLineStartRewriter.cs` | Renamed. It now joins every gap `IsAttachedPartGap` names, in the same position (after the initializer rewriter, before the list rewriter). The `WouldJoinAcrossUnjoinableTrivia` term is deleted and the summary and remarks are rewritten. | FMT-BREAK-039, FMT-BREAK-040 |
| `.../LineBreaks/Rewriter/ChainLineBreakRewriter.cs` | Deleted `JoinConditionalElementAccesses` and the attached-part loop of `NormalizeChain`. A chain without links now returns unchanged. The summary and comments are updated, and the unused `Reihitsu.Core.Enumerations` using is removed. | FMT-BREAK-040 (one owner) |
| `.../LineBreaks/Rewriter/ListElementInteriorJoinRewriter.cs` | `IsOwnedGap` now leaves every attached-part gap to its owner, not only the `(`. The remarks are updated. | FMT-BREAK-040 (one owner) |
| `.../LineBreaks/LineBreakPhase.cs` | Registers the renamed rewriter and updates the summary. | — |
| `.../Indentation/Contributors/MethodChainAlignmentContributor.cs` | Deleted the attached-part column (`MethodChainAttachedPart`) and both `AlignRestOfRun` calls for attached parts. A chain without links now contributes nothing. The class summary and `AlignRestOfRun`'s summary are updated. | FMT-UNSUP-014 (special cases) |
| `.../Precheck/PrecheckPhase.cs` | Adds `AttachedPartCommentReason` and `FindTriviaInAttachedPartGap`. The new check reads the trailing trivia of the token in front of the gap, then the leading trivia of the token behind it, and only when the token in front lies inside the node (FMT-UNSUP-003). It is listed last among the candidates, so FMT-UNSUP-004 and FMT-UNSUP-010 keep their reason when they find the same trivia. | FMT-UNSUP-014 |
| `AGENTS.md`, `sdd/specs/README.md`, `sdd/specs/formatter/line-breaks.md` | Rename only: in `line-breaks.md` and `sdd/specs/README.md` the code owner `ArgumentListLineStartRewriter` becomes `AttachedPartLineStartRewriter`. In the `LineBreakPhase` row of AGENTS.md, the guard `ArgumentListLineStartRewriter.IsArgumentListOpenParenthesis` becomes `AttachedPartGapUtilities.IsAttachedPartGap`. | — |

## Policy owners
- Which token starts an attached part. `rg -n "SuppressNullableWarning|ElementBinding" src/Reihitsu.Core src/Reihitsu.Formatter` finds two owners:
  - `FluentChain.Collect` in Core, the chain model. It covers spines only, and the analyzers use it.
  - The new `AttachedPartGapUtilities`. It is token-level and applies wherever the part stands, which a chain spine does not cover.

  The two agree on what is a part. `FluentChain` folds a `!` in front of `?[` into the conditional element access's tokens. The utility treats that `!` as a part of its own, which joins and refuses the same gaps. `rg -n "IsArgumentListOpenParenthesis"` no longer finds anything, because its one copy moved into the utility.
- The join of a gap: `LineBreakTriviaUtilities.JoinGaps`, unchanged.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` on this commit: 8 files changed, 304 insertions(+), 231 deletions(-). The insertions count the renamed rewriter (a delete plus an add) and the new utility in full. New type: `AttachedPartGapUtilities`, about 60 lines without comments.

## Deleted special cases
- `ArgumentListLineStartRewriter.CanJoin`: the `WouldJoinAcrossUnjoinableTrivia` term and the remarks sentence about it (change doc *Deleted*, item 1).
- `MethodChainAlignmentContributor.Contribute`: `SetIfFirstOnLine(attachedPart.FirstToken, column, "MethodChainAttachedPart", model)` and its root/anchor choice, plus the summary sentence (item 2).
- The refusal for the gap in front of a part's first token: `ChainLineBreakRewriter.CloseGap(attachedPart.FirstToken.GetPreviousToken(), …)` in `NormalizeChain` and in `JoinConditionalElementAccesses`, which is deleted as a whole. The new owner has no refusal (item 3).
- The refusal for the `?`–`[` gap: the `CloseInnerGaps(attachedPart.Tokens, gaps)` calls (item 4). `CloseGap` and `CloseInnerGaps` stay for links.
- `MethodChainAlignmentContributor`: both `AlignRestOfRun(attachedPart.Tokens, model)` calls (item 5). `AlignRestOfRun` stays for link operators.
- `ListElementInteriorJoinRewriter.CanJoin`'s refusal is kept, as the change doc says. That rewriter no longer reaches the attached-part gaps at all.
- I found no further special case.

## Tests run
- `scripts/test.sh --configuration Debug --project formatter`: Passed! Failed 0, Passed 6088, Total 6088.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed! Failed 0, Passed 10953, Total 10953.
- `scripts/test.sh --configuration Debug --project cli`: Passed! Failed 0, Passed 403, Total 403.
- `scripts/test.sh --configuration Debug --project architecture`: Passed! Failed 0, Passed 198, Total 198.
- `scripts/test.sh --configuration Debug --project core`: Passed! Failed 0, Passed 738, Total 738.
- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors.

All 143 formatter, 21 analyzer, and 7 CLI tests that 07 handed over as failing now pass. `scripts/format.sh` was not run; that is the cleaner's job.

## Disputed tests or scope
- **Scope.** There is one new type, the shared `AttachedPartGapUtilities`, with two consumers (the line-break phase and the precheck), plus a class rename. Without the shared type, the precheck would need a copy of the rewriter's predicate (FC-015) or would have to call into a line-break rewriter. Net production code is small: the three deleted join and alignment sites go away.
- **Docs I did not change**, which belong to the spec author:
  - AGENTS.md's `PrecheckPhase` row still lists seven shapes and does not name FMT-UNSUP-014.
  - The `LineBreakPhase` row's text still says "the join of an argument list's opening parenthesis onto the line in front of it". It should now cover element accesses, conditional element accesses, and null-forgiving operators.
  - The change doc's *Special cases* names `ArgumentListLineStartRewriter.CanJoin`, which describes `main`'s state; whether to name the rename there is the spec author's call.
  - `line-breaks.md` code owners: whether to list `AttachedPartGapUtilities` is the spec author's call.

## Architecture findings
_None._ This change resolves the finding in 02-skeptic: `ListElementInteriorJoinRewriter` and `ChainLineBreakRewriter` no longer join attached parts, and `AttachedPartLineStartRewriter` is the one owner.

## Follow-up candidates
_None._
