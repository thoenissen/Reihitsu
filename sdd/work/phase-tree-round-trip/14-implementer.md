Role: implementer
Commit: d9aa414
Verdict: test disputed

This is repair round 1 after the first audit, together with the two structural changes the maintainer chose.

Inputs:
- `11-auditor.md`: findings #1–#6, A1, and A2. Finding #7 is text only.
- `12-spec-author.md` and `13-test-implementer.md`.
- `sdd/changes/2026-10/phase-tree-round-trip.md`, and FMT-PIPE-021, FMT-STRUCT-026, and FMT-BREAK-002 as amended.
- `squad/decisions/2026-10-31-phase-tree-round-trip-{would-merge-owner,member-brace-owner}.md`.

I re-derived the fixes from the specs and decisions. I took none of the audit's suggestions as given. No other intended fix was in my prompt to ignore. I changed no test and no spec file.

26 of the 27 tests that `13-test-implementer.md` lists as failing now pass. The remaining one, `MembersWithACommentAtTheArrowReachAStableOutput`, cannot pass without a test-side or scope change; see *Disputed tests or scope*.

## Changes

| File | What changed | Requirement / decision |
|---|---|---|
| `src/Reihitsu.Core/TokenSeparationUtilities.cs` (new) | The one lexer-based owner of "two tokens written tight read as other tokens". `WouldMergeIntoOneToken(SyntaxToken, SyntaxToken)` and `(string, string)` lex the concatenation and compare it with the two texts lexed alone, tokens and trivia both. `GetLexicalUnitText` moved here unchanged from the RH3002 fix. | would-merge-owner (B), FMT-STRUCT-026 first separator, RH3002-008 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider.cs` | The private `WouldMergeIntoOneToken` and `GetLexicalUnitText` were removed. The fix calls the Core owner. | would-merge-owner (B) |
| `src/Reihitsu.Formatter/Pipeline/Core/Utilities/TokenGapUtilities.cs` | `WouldMergeWhenTight` removed. The file is again identical to `origin/main`. | would-merge-owner (B) |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ListElementInteriorJoinRewriter.cs` | The list join calls the Core owner. | would-merge-owner (B) |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Utilities/ExpressionBodyToBlockConverter.cs` | The `return` separator calls the Core owner (#3). When a comment stays leading the expression in front of its first line break (`moveComments` false), the created `return` gets the author's line break behind it. That is the line break that ended the arrow's line, taken from the arrow's trailing trivia or the expression's leading trivia (#1). | FMT-STRUCT-026 (first and third separators), FMT-PIPE-021 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/ControlFlowBraceTransform.cs` | The token behind an inserted `}` is resolved in the original tree. An `if` that is itself a body the transform braces is followed by that body's inserted `}`, which counts as no token, so it gets no line break. An `else if` resolves to what follows its outer `if`. The wrap guard is extracted as `CanWrap` and shared by the wrap and the lookup (#2). | FMT-STRUCT-026 (enclosing `}`) |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/AccessorExpressionBodyTransform.cs` | The `(` and `)` around a `throw` operand are built with explicit trivia (#5). | FMT-STRUCT-026 (no elastic trivia) |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs` | A moved token takes the moved text as its value only for `InterpolatedRawStringEndToken`, by kind. Every other token keeps its value (#6). | FMT-PIPE-021 (token values) |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakBlockRewriter.cs` | The run's `IsMemberBody`/event skips are removed, so `VisitBlock` and `VisitAccessorList` place the braces of every block and accessor list again, as on `main`. New visitors for constructor, method, operator, conversion, destructor, local function, and event declarations place the member body's braces, or the event accessor list's, through `BracePlacer.NormalizeOwnedBraces`. That is the call `DeclarationBraceLineBreakRewriter` made on `main` (#4, A2). | member-brace-owner (B), FMT-BREAK-002, FMT-BREAK-029 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/DeclarationBraceLineBreakRewriter.cs` | It no longer places member-body or event-accessor-list braces. It keeps type and namespace braces, indexer accessor lists, the parameter-list opener collapse, and constructor initializers. The single-call helpers it no longer needs are folded into the visitor. | member-brace-owner (B) |

Notes:

- **Member-body braces have two steps inside the one owner.** `VisitBlock` places them when the block is still attached, which covers a body block that is the formatting root (the FMT-BREAK-002 example). The declaration visitor then places them again on the declaration, where the token in front of `{` lies inside the node. That covers a body whose content changed and that is therefore detached, and it keeps FMT-BREAK-029's example (`void M() ` keeps its space).
  - This is the same pair of calls `main` made, in two rewriters. They now both run in `LineBreakBlockRewriter`, earlier in the phase than `DeclarationBraceLineBreakRewriter` ran on `main`.
  - No existing final output changed, which bears out "final output as on `main`".
- **Why the lexer comparison is two-sided.** The Core owner compares the concatenation with both texts lexed alone, instead of the RH3002 copy's `ParseToken(left + right).Text != left`. The one-sided check reported a merge for `..` + `^1` and `..` + `end`: the lexer produces `..` as two `.` tokens, which the parser composes. That made two existing tests fail on `values[.. ^1]` and `values[start.. end]`: `ListElementInteriorJoinTests.IndexFromEndWrappedAfterOpenRangeIsJoinedWithoutSpace` and `RangeWrappedAfterRangeOperatorIsJoinedWithoutSpace`. The two-sided comparison keeps both, and every RH3002 test passes.
- **No public API beyond what the decision requires.** `Reihitsu.Core` gains the public static `TokenSeparationUtilities`. The would-merge-owner decision asks for the owner in a shared place that the formatter and the code-fix assembly both reference, and Core is that place. No public surface of the formatter, analyzer, or CLI changes, and no dependency is added.
- **Code owners.** `TokenSeparationUtilities` is not in a spec's code-owner list. `12-spec-author.md` already states the shared helper is a code owner of no spec. Both line-break rewriters still exist, so `line-breaks.md` needs no change.

## Policy owners

- **Token merge:** `rg -n "WouldMerge|IsIdentifierPartCharacter" src --glob '!*Test*'` finds `Reihitsu.Core/TokenSeparationUtilities.WouldMergeIntoOneToken` as the only owner. Its callers are `ListElementInteriorJoinRewriter.CreateSeparator`, `ExpressionBodyToBlockConverter.CreateReturnStatement`, and `RH3002…CodeFixProvider.CreateReplacement` (twice). `UnaryOperatorSpacingUtilities.WouldGlueIntoDifferentOperator` remains for the spacing phase's no-space rule, which asks a different question (a spacing decision, not a lexing check).
- **Member-body and event-accessor-list braces:** `rg -n "NormalizeOwnedBraces|IsMemberBody" src/Reihitsu.Formatter/Pipeline` finds the calls in `LineBreakBlockRewriter` (member bodies, local function, event) and `AccessorListLayout` (property and indexer accessor lists, unchanged), and no `IsMemberBody`.
- **Line break behind an inserted `}`:** `ControlFlowBraceTransform` only. The guard `CanWrap` is shared by the wrap and the lookup, so the two cannot disagree about which bodies get braces.

## Tests run

```shell
scripts/test.sh --configuration Debug --project formatter     # Failed!  - Failed: 1, Passed: 3371, Total: 3372   (13 reported 27 failing at 21ab3a3)
#   Failed MembersWithACommentAtTheArrowReachAStableOutput
scripts/test.sh --configuration Debug --project analyzer      # Passed!  - Failed: 0, Passed: 6312
scripts/test.sh --configuration Debug --project core          # Passed!  - Failed: 0, Passed: 535
scripts/test.sh --configuration Debug --project architecture  # Passed!  - Failed: 0, Passed: 89
scripts/test.sh --configuration Debug --project cli           # Passed!  - Failed: 0, Passed: 251
scripts/test.sh --configuration Debug --project tooling       # Passed!  - Failed: 0, Passed: 125
scripts/test.sh --configuration Debug --project playground    # Passed!  - Failed: 0, Passed: 17
scripts/build.sh --configuration Debug                        # 0 Error(s); no warning in a changed file
scripts/format.sh <the ten changed files>                     # applied
```

## Disputed tests or scope

### `StructuralTransformSeparatorFinalOutputTests.MembersWithACommentAtTheArrowReachAStableOutput` fails on a reversal that also exists on `main`

The test calls `AssertSecondPassIsStable`, which also runs `PhaseStabilityCheck`. The reparse hook now passes. The stability check fails on the `Get3` member of its input:

```
BlankLinePhase changes the final output of fixture 77770a4201eb2019 … under LF line endings when it runs alone,
to the output 921052f14d6bb85f. First difference at line 19: expected "    /* c */ {", phase returned "".
```

The final output of that member is the one `12-spec-author.md` (open question 1) shows for `main`:

```csharp
    int Get3()
    /* c */ {
        return

        /* d */ 42;
    }
```

Run on this final output, the blank-line phase inserts a blank line in front of `/* c */ {` and the line-break phase removes it again. This is a reversal between two phases. I established that it is not caused by this run:

- I built `origin/main` in a temporary worktree (since removed) and ran `scripts/trace.sh` on that final output. `main` shows the same two phase changes (`+` blank line in BlankLinePhase, `-` in LineBreakPhase).
- `scripts/format.sh` from `main` and from the head give byte-identical final outputs for the whole test input, all four members. So the change doc's "final output unchanged" holds for these members.

The test as written requires either the phases to stop reversing on `/* c */ {` behind a signature, or the reversal to be recorded in `PhaseReversalExceptionList`, which is test code. The first changes `BlankLinePhase`/`LineBreakPhase` behavior that is identical on `main`, outside the change doc and outside open question 1's recommended option A ("keep it unspecified in this run"). So I left it.

The lead can route it to the test implementer: record the reversal with its known cause, or drop `Get3` from the stability fixture. Or it can go to the maintainer together with open question 1, since this is the same at-arrow comment layout.

## Architecture findings

_None._ A1 and A2 are resolved as the decisions chose: one lexer-based owner, and one owner rewriter for block and event-accessor-list braces.

## Follow-up candidates

- `ExpressionBodyToBlockConverter.ExtractSignificantTrivia` still drops the whitespace in front of the arrow's trailing comment it moves behind `{`. This is unchanged and belongs with open question 1 in `12-spec-author.md`.
- Property and indexer accessor-list braces are still placed by `LineBreakBlockRewriter.VisitAccessorList` and again by `AccessorListLayout` (through `PropertyLayoutLineBreakRewriter`/`DeclarationBraceLineBreakRewriter`). The member-brace-owner decision covers blocks and event accessor lists only.
- The UTF-8 value candidate from `05-implementer.md` is withdrawn; the auditor's harness showed it unfounded.
