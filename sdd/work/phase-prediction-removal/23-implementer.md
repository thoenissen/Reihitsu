Role: implementer
Commit: 9081b4c
Verdict: tests pass

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. This is repair round 2. It implements:
- FMT-PIPE-017 (`18-`, `21-spec-author.md`);
- the retirement of RH5303-006 and RH5304-005;
- the FMT-RAW-005 byte-order-mark clarification (`19-spec-author.md`);
- the code side of audit findings 2 and 4 (`17-auditor.md`).

Every test handed over in `22-test-implementer.md` passes, as do all test projects, including the self-hosting scans. No change needed scope the change doc does not name.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Data/FormattingContext.cs` | New `RootFirstTokenColumn` (`int?`, constructor parameter `rootFirstTokenColumn`). It is the column of the formatting root's first token in its document. Only the two node-level document entry points set it. | FMT-PIPE-017 |
| `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs` | New `ComputeDocumentColumn(token, countsOwnWhitespaceTabsAsReplaced)`, with private `IsInOwnLeadingWhitespace`. It counts the characters in front of the token on its line, one column each, with two exceptions: a byte order mark at text position 0 counts zero, and a tab in a whitespace trivia of the token's own leading trivia counts four when the entry point does not keep that trivia. A tab in front of the node that is kept, or one inside a comment, counts one. | FMT-PIPE-017 (C1 B, C2a, C6) |
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | Both entry points no longer move lines after the pipeline (details below). | FMT-PIPE-017 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutComputer.cs` | Two new private methods (details below). | FMT-PIPE-017, FMT-RAW-005 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/IndentationPhase.cs` | A root that is still attached to its document (`root.Parent != null`) is first cloned as a tree of its own (`CSharpSyntaxTree.Create`). The phase then always lays out the node the way the pipeline normally hands it over: its first token starts the tree's text, and no token outside it decides first-on-line. Without this, a mid-line target that reached the phase attached would get no line-0 layout and no offset. | FMT-PIPE-017; audit finding 7 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/IndentationRewriter.cs` | Comment only. The `lineOffset` mapping now serves a direct caller of `LayoutComputer.Compute` plus `IndentationRewriter.Apply` on an attached node; the phase itself hands over a tree of its own. | FC-031 |
| `src/Reihitsu.Formatter/Pipeline/DocumentationComments/DocumentationCommentFormattingPhase.cs` | Fixes the relocation of a `///` comment on a detached root (details below). | FMT-DOC-001, FMT-PIPE-010; audit finding 2 |
| `src/Reihitsu.Analyzer/Core/InitializerLayoutAnalysisUtilities.cs`, `RH5303…CodeFixProvider.cs`, `RH5304…CodeFixProvider.cs` | The guard of 37a5ea4 is reverted exactly (`git diff 37a5ea4~1 37a5ea4 \| git apply -R`): `ContainsComplexElementBehindMultiLineCommentTail`, `StartsLineInsideComment`, and both provider conditions and usings are gone. | RH5303-006 and RH5304-005 retired; RH5303-005, RH5304-004 |
| `src/Reihitsu.Core/SyntaxIndentationUtilities.cs` | Comment only, in the summary of `IsIndentingAncestor`. The document-scoped overloads now cancel the base-level undercount because the indentation phase shifts their node to its first token's column. Before, a column offset applied afterwards cancelled it. | FC-031 |

**`ReihitsuFormatter.cs` in detail**
- `FormatNodeInDocumentAsync`:
  - It computes `keepsLeadingTrivia` once: the target is a direct child of the root, or it does not start its line. That is the existing FMT-PIPE-010 condition, now reused for the restore.
  - It passes `rootFirstTokenColumn = ComputeDocumentColumn(firstToken, !keepsLeadingTrivia)` (null for the document root).
  - It drops `ComputeTokenColumn` and `AdjustNodeIndentation`.
- `FormatNodeInDocumentWithContextAsync`:
  - It passes the context's first-token column, counting tabs as one: only the target is written back, so whitespace in front of the context stands as it is.
  - It drops the move as well.
  - The unconditional restore of the target's leading trivia stays; FMT-PIPE-017 puts it outside the requirement (C5).

**`LayoutComputer.cs` in detail**
- `ShiftToRootFirstTokenColumn` runs after block indentation and before the alignment sweeps. When the root's first token's line is placed, it shifts every layout entry (`ShiftRange(0, int.MaxValue, …)`) by `RootFirstTokenColumn - GetColumnBeforeRawStringShift(firstToken)`. When the token stands behind a multi-line comment's last line, the line is unplaced and the offset is 0.
  - The alignments run after the shift. An anchor on an unplaced line (a comment tail, a raw-string line) is therefore measured at its text column, which is its document column on every line after the first. Lines aligned to it follow it.
  - Raw-string shifts are computed from the shifted opening quotes against the closing marker's text column.
- `GetOpeningQuotesColumnBeforeRawStringShift` is now the column function handed to `RawStringLineShifts`. On a placed line that starts at text position 0 with U+FEFF, it adds one column, because the rewriter writes the byte order mark in front of the line's column. All other alignments keep treating the byte order mark as zero-width.

**`DocumentationCommentFormattingPhase.cs` in detail**
- `RelocateOffPositionDocumentationComments` and `RegisterRelocation` now take the `FormattingContext`.
- A new private `IsWrittenBehindRootPrecedingToken` decides whether a `///` comment is written behind the root's preceding token. That holds when all of these hold:
  - the comment's owning token has no previous token in its tree (a detached root);
  - the comment is on the root's first line;
  - `RootPrecedingToken` exists and does not end its line.
- So the relocation decision no longer depends on whether the first phase detached the target.
- The stranded check reuses `DocumentationCommentUtilities.DocumentsFollowingCode`. That is equivalent to the former `IsStrandedBehindCode` once "written behind source" is established.

`ReihitsuFormatterHelpers.AdjustNodeIndentation` and `ComputeTokenColumn` stay:
- `ComputeTokenColumn` is still used by `TargetAttributePlacementCodeFixProviderBase`.
- `AdjustNodeIndentation` no longer has a production caller. Its own unit tests in `ReihitsuFormatterHelpersTests` keep it alive, and I may not delete tests (follow-up below).

The change touches:
- shared helpers: `FormattingContext` (new optional parameter, existing callers unchanged), `ReihitsuFormatterHelpers` (new method), and a comment in `SyntaxIndentationUtilities`;
- the documentation phase.

All of it stays inside the inventory row and the decisions of the accepted change doc ("The node-level entry points lay out their node at its own column"), and the audit findings the lead assigned to this round. No public API changes: the new constructor parameter is on the internal `FormattingContext`.

## Policy owners

- Node-level column: `rg -n "AdjustNodeIndentation|ComputeTokenColumn|RootFirstTokenColumn|ComputeDocumentColumn" src --glob '*.cs'`.
  - The production owners are `ReihitsuFormatterHelpers.ComputeDocumentColumn` (counting) and `LayoutComputer.ShiftToRootFirstTokenColumn` (applying).
  - `ComputeTokenColumn` remains only for `TargetAttributePlacementCodeFixProviderBase`, which computes an attribute's insertion column. That is a different decision.
  - `AdjustNodeIndentation` has no production caller.
- Tab counting: `rg -n "'\\\\t'" src/Reihitsu.Formatter --glob '*.cs'`.
  - `TabReplacementPhase` replaces tabs. `ComputeDocumentColumn` counts the node's own leading tabs as their replacement (FMT-PIPE-017, first bullet).
  - The documentation-text `TrimStart` calls are FMT-TAB-003 text, not whitespace.
- Byte order mark column: `rg -n "uFEFF" src/Reihitsu.Formatter --glob '*.cs'`.
  - The rewriter's `AddLineStartWhitespace` writes the byte order mark.
  - `GetOpeningQuotesColumnBeforeRawStringShift` counts it for the raw-string move only.
  - `ComputeDocumentColumn` counts it as zero.
  - `CleanupPhase` and `ReihitsuFormatterHelpers.StartsWithByteOrderMark` keep it.
- The preceding token of a detached root: `rg -n "RootPrecedingToken" src/Reihitsu.Formatter` finds the blank-line rewriters, `CommentIndentationContributor` (through `PrecedingTokenFacts.Resolve`), and now the documentation relocation. They form one source, and there is no copy.
- The guard is gone: `rg -n "ContainsComplexElementBehindMultiLineCommentTail|StartsLineInsideComment" src --glob '*.cs'` finds nothing outside `bin`/`obj`.

## Tests run

All counts were observed on the tree of 9081b4c.
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ReihitsuFormatterNodeColumnTests|FullyQualifiedName~ReihitsuFormatterNodeBlankLineTests|FullyQualifiedName~IndentationPhaseRawStringMoveTests|FullyQualifiedName~RawStringInterpolationHoleTests|FullyQualifiedName~CommentLedLineAlignmentTests|FullyQualifiedName~ByteOrderMarkTests|FullyQualifiedName~TabGapSpacingTests"` → `Passed: 193, Failed: 0`. On 9d9e7c1, the test implementer saw 9 failures.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3202|FullyQualifiedName~RH5301|FullyQualifiedName~RH5303|FullyQualifiedName~RH5304"` → `Passed: 167, Failed: 0`. The test implementer saw 21 failures.

| Project | Result |
|---|---|
| formatter | 2987 passed |
| analyzer | 6226 passed |
| architecture | 89 passed |
| core | 533 passed |
| cli | 250 passed |
| tooling | 125 passed |
| playground | 17 passed |
| selfhosting | 3607 passed |

`scripts/format.sh --force` changed none of the changed files. The test logs show no RH, CS, S, or IDE warning for them. The Release build is the cleaner's.

## Disputed tests or scope

_None._

## Architecture findings

_None new._ The finding of `17-auditor.md` is resolved by the structure the maintainer chose (1B): `FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync` no longer move a line after the last phase, and the indentation phase owns every node-level line's column.

## Follow-up candidates

- `ReihitsuFormatterHelpers.AdjustNodeIndentation` and its private `AdjustLeadingWhitespace` have no production caller left. Only the six `AdjustNodeIndentation*` tests in `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterHelpersTests.cs` use them. Removing the helper needs those tests removed first, which is test-implementer work. As it stands, the helper is dead production code with tests.
- Still open from earlier rounds:
  - RH5201 against change doc row 10 (`07-test-implementer.md`);
  - the RH5304 statement and expression-body targets. The guard they were about is gone, so this is now plain fix coverage.
  - The skeptic's follow-up 2 (`AdjustNodeIndentation` not moving raw-string lines) is resolved by this round. Follow-up 1 (a tab counted as one column) is resolved for the target's own leading whitespace, and decided as one column for whitespace outside the target (C1 B).
- `FormatNode` on an attached node lays out at the depth column, while the document entry points lay out at the document column. This is pre-existing and FMT-PIPE-017 does not cover it (`20-skeptic.md`).
