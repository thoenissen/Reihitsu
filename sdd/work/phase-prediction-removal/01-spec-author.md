Role: spec author
Commit: a821b67
Verdict: change doc written

The prompt carried no intended fix or verdict. The issue offers two mechanisms for the raw-string owner ("the layout computation places raw-string lines directly" or "the raw-string phase runs before the anchors are measured"); the change doc picks the first and records why the second is rejected. Both give the same output, so the choice is a design decision for the maintainer's change-doc approval (escalation 2), not an open question about behavior.

## Change doc

`sdd/changes/2026-10/phase-prediction-removal.md` (`Status: Draft`). It needs the maintainer's approval of What and Why before tests are written. In short:

- New first phase `TabReplacementPhase` replaces tabs in whitespace and between directive tokens; `CleanupPhase` no longer does. The rules move unchanged from FMT-CLEAN-002/003 and the tab half of FMT-CLEAN-004 to the new spec `formatter/tab-replacement.md` (FMT-TAB-001 to FMT-TAB-003).
- `RawStringAlignmentPhase` leaves the pipeline. `IndentationPhase` places the opening quotes and moves the raw string's content, hole, and closing lines in the same phase, and measures every anchor after that move.
- The pipeline still has twelve phases (one added, one removed). `PhaseOwnershipDocumentationTests` asserts that count.
- No intended output changes. Every example of FMT-CLEAN, FMT-RAW, and FMT-INDENT keeps its output. Only output that is not idempotent today could change, and FMT-PIPE-004 already requires the new output.

## Changed requirements

- FMT-PIPE-001 — Fixed phase order — changed. New table: `TabReplacementPhase` is first, `RawStringAlignmentPhase` is gone, and `IndentationPhase` is owned by FMT-INDENT and FMT-RAW. New normative sentence: no phase computes a column from a move or replacement that a later phase applies. A coverage sentence leaves predicted line breaks and blank lines undecided (see `RootListPosition` below).
- FMT-PIPE-015 — Columns are final after the indentation phase — added.
- FMT-PIPE-016 — No phase after the first sees or writes a tab in whitespace — added.
- FMT-TAB-001 — Tabs in whitespace become four spaces each — added (the rule of FMT-CLEAN-002). It now also names whitespace in the code of an interpolation hole.
- FMT-TAB-002 — Tabs inside directives become four spaces each — added (the rule of FMT-CLEAN-003). It adds the qualifier with both sides: a tab between `#region` and its description is replaced, a tab inside the description is kept.
- FMT-TAB-003 — Tabs in literals, comments, and disabled text are kept — added (the tab half of FMT-CLEAN-004). It adds the qualifier: a tab in an interpolated string's text is kept, a tab in a hole's whitespace is replaced.
- FMT-CLEAN-002 — retired (now FMT-TAB-001).
- FMT-CLEAN-003 — retired (now FMT-TAB-002).
- FMT-CLEAN-004 — Literals, comments, and disabled text are not cleaned — changed: whitespace removal only, with no tab clause.
- FMT-RAW-005 — Alignment follows the final position of the opening quotes — changed: the target is the column the indentation phase gives the opening quotes, in the same phase.
- FMT-RAW-007 — changed wording only. The tab sentence now says the whitespace holds no tab (FMT-TAB-001), and "the indentation phase would give it" becomes "the indentation requirements would give it". Behavior is unchanged.
- FMT-INDENT-011 — changed wording. Raw-string content, closing, and unplaced hole lines get no column from FMT-INDENT; they move only as FMT-RAW-001 and FMT-RAW-007 move them. Behavior is unchanged.
- FMT-INDENT-030 — Anchors are measured at their final column — changed. It now covers an anchor on a line a raw string moves (closing line, hole line), measured after the move in this phase. New example plus its negative side.

Other text edits:
- Intros and non-goals of `pipeline.md`, `cleanup.md`, `indentation.md`, `raw-string-alignment.md`, and `structural-transforms.md` (it is no longer the first phase).
- A non-goal in `horizontal-spacing.md`.
- `RH5601.md` references: FMT-CLEAN-002/003/004 become FMT-TAB-001/002/003. The rule's behavior is unchanged.
- `sdd/specs/README.md` gets the new row; `RawStringAlignmentPhase` is dropped from the FMT-RAW owners.
- The ledger gets 5 new IDs.
- The `sdd/README.md` layout note now reads "or per part of one".
- `AGENTS.md`: the phase order bullet and the ownership table. New `TabReplacementPhase` row; the `RawStringAlignmentPhase` row is folded into `IndentationPhase`; the `CleanupPhase` row loses tabs and `NormalizeDirectiveTriviaTabs`.

No rule page changes. RH5601 reports and fixes the same tabs, and pages never mention the formatter.

Code owners the implementation must match:
- FMT-TAB names `TabReplacementPhase`, which does not exist yet.
- FMT-RAW names `RawStringLineShifts`, `RawStringLiteral`, and `RawStringLiteralUtilities`.

If the implementer renames, removes, or adds owner types, the spec headers and `sdd/specs/README.md` must follow. The spec checker verifies this.

State of the architecture tests on this commit (`scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd|FullyQualifiedName~PhaseOwnership"`): 48 passed, 4 failed. All four failures come from work that is still pending:
- `EveryCodeOwnerNamesAnExistingType`: `TabReplacementPhase` is missing.
- `OwnershipMapMatchesRuntimePhaseOrder`: the runtime still has `RawStringAlignmentPhase`.
- `EveryRequirementReferenceNamesAnActiveRequirement`: 5 tests are still tagged FMT-CLEAN-002/003.
- `EveryRequirementOfAnEnforcedSpecIsCovered`: FMT-PIPE-015/016 and FMT-TAB-001..003 have no tests.

`UsesListsExactlyTheReferencedRequirements` and the index and ledger checks pass.

## Examples for the tests

- FMT-TAB-001, FMT-TAB-002, FMT-TAB-003: derive these from the examples. The FMT-TAB-003 hole-line example was traced on `a821b67` and gives the shown output in one pass.
- FMT-PIPE-015: assert through the phase observer (`FormattingPipeline.Execute` with an observer). After `IndentationPhase`, every line's leading whitespace and every token's column already equal the final output, for the example and for the trailing-comment tab case in its last paragraph.
- FMT-PIPE-016: through the observer. No phase output after the first holds a tab in whitespace trivia or in directive-token trivia, for an input with tabs in indentation, token gaps, an end-of-line comment gap, a hole line of a multi-line interpolated raw string, and a directive.
- FMT-INDENT-030: the new raw-string example (anchor `.Trim` behind a closing marker that moves 12 columns) and its aligned-literal side. Traced on `a821b67`: `.ToUpper` lands in column 19 in one pass.
- FMT-RAW-005: keep the existing examples. They now cover the same-phase ownership.

Tests that exist today and are affected by the move:
- Tagged with a retired ID, to be retagged FMT-TAB-001/002: `WhitespaceCleanupTests.TabsBeforeTrailingCommentsBecomeFourSpacesEach`, `.TabsBetweenDirectiveTokensBecomeFourSpacesEach`, `.TabsInsideRegionDescriptionAndWarningMessageAreKept`, `CleanupPhaseTests.ReplacesTabsInWhitespaceTriviaWithSpaces`, `.ReplacesTabsInsideDirectiveInteriorWithSpaces`.
- Tagged FMT-CLEAN-004 but about tabs, so they belong to FMT-TAB-003: `CleanupPhaseTests.PreservesTabsInsideComments`, `WhitespaceCleanupTests.TabsAndSpacesInsideCommentsLiteralsAndDisabledTextAreKept`, `.TabsInsideOtherLiteralsAndDocumentationCommentsAreKept`, and possibly `CleanupPhaseTests.PreservesCommentTrivia`.
- Call phases that no longer do this work: the `CleanupPhaseTests` tab tests call `CleanupPhase` directly. `RawStringAlignmentPhaseTests` calls `RawStringAlignmentPhase`. `FormattingPipelineTests` asserts the phase list (FMT-PIPE-001).

The issue's "passes unchanged" can only hold for the full-pipeline output assertions. Phase-level unit tests of moved behavior have to move with it.

## Inventory of prediction sites

The issue asks for this list to be posted on thoenissen/Reihitsu#1463. Method:
- `rg` over `src/Reihitsu.Formatter` for references to other phases, for column measurement (`GetLineSpan`, `.Character`, `GetColumn`, `GetAdjustedColumn`), and for tab handling.
- The same over `src/Reihitsu.Analyzer.CodeFixes` for code fixes that reuse formatter logic.
- Traced probes with `scripts/trace.sh` (3 passes each) on `a821b67`.

| # | Site | Predicts | Protects |
|---|---|---|---|
| 1 | `LayoutComputer.GetAdjustedColumn`; `LayoutModel.UseRawStringLineShifts`, `GetRawStringShift`, `Set` (subtracts the shift), `TryGetPlacedColumn`, `TryGetLayoutColumn`. They are read by every alignment contributor, `ListElementAligner`, `PatternAnchor`, and `CommentIndentationContributor`. | `RawStringAlignmentPhase` moving a literal's lines (through `RawStringLineShifts.GetShift`) | FMT-INDENT-030, FMT-RAW-007, FMT-PIPE-004 |
| 2 | `LayoutComputer.RegisterRawStringHoleLines` → `LayoutModel.RegisterRawStringHoleLine` / `TryGetPlacedColumnBeforeRawStringShift`, which use `RawStringLineShifts.GetColumnAfterTabReplacement` | `CleanupPhase` turning a tab into four spaces | FMT-RAW-007, FMT-CLEAN-002 |
| 3 | `RawStringAlignmentPhase.CollectReplacements` (opening-quote column) and `GetIndentation` (hole-line width; it also rewrites the hole-line whitespace as spaces, which is part of cleanup's job), both using `GetColumnAfterTabReplacement` | `CleanupPhase` turning a tab into four spaces | FMT-RAW-001, FMT-RAW-005, FMT-RAW-007, FMT-RAW-008, FMT-CLEAN-002 |
| 4 | `RawStringAlignmentPhase` computing again, from the text, the shift that the indentation phase already used from the layout model | none; it is a second computation of one decision | FMT-RAW-001, FMT-RAW-005 |
| 5 | `TrailingWhitespaceWriter.NeedsTrailingSpaceNormalization`: its rationale says a lone tab must count as unnormalized because cleanup expands it. The check itself (exactly one space) is correct on its own. | `CleanupPhase` (rationale only) | FMT-SPACE-002, FMT-PIPE-004 |
| 6 | `RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.RealignContent`, outside the formatter | its own copy of the raw-string shift | SHARED-FIX-003, FMT-RAW-009 (thoenissen/Reihitsu#1458; non-goal of this run) |
| 7 | `RootListPosition.KeepsBlankLineAboveBlockStatement`. It is captured before the pipeline and read by `BlankLineStatementSpacingRewriter` for the node-level entry points. It reuses `TokenGapNormalizer.HasOwnLinePlacementOwner`. | `LineBreakPhase` keeping or removing a blank line above a block statement's `{` | FMT-PIPE-010 |

The change doc removes sites 1 to 4 and leaves site 5 without a tab to see.

Checked and found not to be predictions. Each only builds parser-shaped trivia, leaves a decision to a later phase without computing its result, or shares one predicate as its single owner:
- `LineBreakAssignmentRewriter.NormalizeLeadingTriviaToSameLine`, which leaves the space to `HorizontalSpacingPhase`;
- `ControlFlowBraceTransform` and `BlankLineTokenCleanupRewriter.CollapseLeadingBlankLines`, which build trivia in the parser's shape;
- `BlankLineTriviaBoundaryRewriter.StartsOwnLine` and the indentation phase, which share `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex`;
- `FieldDeclarationSplitTransform.AppendLeadingComment`, which explicitly does not rely on the blank-line phase.

Traced on `a821b67` (3 passes each). Every case below was stable after one pass, so no non-idempotent output was found to cite in the change doc:
- a tab before `/** */` in an argument list;
- a tab after a multi-line `/* */` before an anchor;
- `#region` followed by a tab;
- tab-indented `///` lines;
- four raw-string hole shapes: a tab before a comment with a left shift and a wrapped argument, a tab-indented hole line with a right shift and a comment above, a tab before nested opening quotes with a left shift, and a chain anchored behind a moving closing marker.

## Follow-up candidates

- **Site 7 (`RootListPosition`).** It predicts a later phase's blank line, not a column, so it is outside the issue's "Done when". FMT-PIPE-001 states that it leaves this undecided. Whether it is a follow-up issue is for the maintainer to decide.
- **`squad/failure-classes.md`, matrix "Tab width across formatter phases".** Its **Decision** paragraph describes the old component split (`CleanupPhase` writes the column last; `RawStringAlignmentPhase` measures). Once this run lands, that paragraph is history. It is a squad file, outside this role's text.
- **Source comments that describe the old split.** Examples are the XML docs of `LayoutModel`, `LayoutComputer.RegisterRawStringHoleLines`, `RawStringLineShifts`, `CommentIndentationContributor`, `TrailingWhitespaceWriter`, and `FormattingPipeline.CreatePhases`. They belong to the implementer and cleaner (FC-031).

## Open questions for the maintainer

_None._

## Decisions needed

_None beyond approving the change doc (escalation 2). That approval covers both structural choices: a separate first phase rather than a step of `StructuralTransformPhase`, and raw-string alignment folded into `IndentationPhase` rather than a separate phase._
