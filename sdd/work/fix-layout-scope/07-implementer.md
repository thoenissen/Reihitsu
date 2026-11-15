Role: implementer
Commit: d80eaf3 (statements derived from the working tree on top of it; the commit carrying this handover holds the changes)
Verdict: test disputed — #1140: test disputed (2 existing RH5201 tests); #1687: tests pass; #1748: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the binding answers M1–M9 and decisions D1–D19 in `00-lead.md`. No column is computed inside a fix: every narrowed fix hands the construct to a formatter entry point, which keeps the first token's column (FMT-PIPE-017).

## thoenissen/Reihitsu#1140

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | `FormatNodeInDocumentAsync` writes back the target's leading trivia as in the document in every case (non-root target); first-token column counts every character as one column; no preceding-token or list-position fact; the trimming of whitespace in front of a moved documentation comment is gone. XML docs of both node-in-document entry points updated | FMT-PIPE-010, FMT-PIPE-017, FMT-PIPE-018, FMT-PIPE-024 (D5, D13) |
| `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs` | `ComputeDocumentColumn` loses its `countsOwnWhitespaceTabsAsReplaced` parameter (every caller passed `false` now): column = characters in front of the token, BOM excluded | FMT-PIPE-017 |
| `src/Reihitsu.Formatter/Data/RootListPosition.cs` | Deleted | D13, pipeline.md code owners |
| `src/Reihitsu.Formatter/Data/FormattingContext.cs` | `RootPrecedingToken`, `RootListPosition` removed (ctor params and properties) | FMT-PIPE-024 (D13) |
| `src/Reihitsu.Formatter/Data/PrecedingTokenFacts.cs` | `Resolve` (root fallback to the context fact) removed; summary says a root's first token has no preceding token | FMT-PIPE-024 |
| `Pipeline/BlankLines/Rewriter/BlankLineStatementSpacingRewriter.cs`, `BlankLineBreakSpacingRewriter.cs` | Root `Visit` override that decided the root statement/section against its document sibling removed; `FormattingContext` ctor param dropped | FMT-PIPE-024 |
| `Pipeline/BlankLines/Utilities/BraceBlankLinePolicy.cs` | Root block no longer reads the list position; `context` params dropped | FMT-PIPE-024 |
| `Pipeline/BlankLines/Rewriter/BlankLineCollapser.cs`, `BlankLineTokenCleanupRewriter.cs`, `BlankLineTriviaBoundaryRewriter.cs`, `BlankLineRegionDirectiveRewriter.cs`, `Utilities/BlankLineEditor.cs`, `Core/Utilities/BlockStartUtilities.cs`, `Indentation/Contributors/CommentIndentationContributor.cs` | `PrecedingTokenFacts.Resolve(x, ctx)` → `PrecedingTokenFacts.From(x)`; now-unused `context` params/fields removed; two `BlankLineEditor` helpers became static (S2325, RH7103); docs updated | FMT-PIPE-024 |
| `Pipeline/DocumentationComments/DocumentationCommentFormattingPhase.cs` | `IsWrittenBehindRootPrecedingToken` deleted (a root's leading doc comment is decided as one no code stands in front of); unused `context` params removed | FMT-PIPE-024, FMT-PIPE-018 |
| `src/Reihitsu.Analyzer.CodeFixes/Core/FluentChainCodeFixHelper.cs` | SHARED-CHAIN-009 fallback deleted (`HasSameChainLayoutAsync`, the formatted-context comparison, annotations): the chain is always formatted with its context and only the chain is written back | SHARED-CHAIN-008, SHARED-CHAIN-009 (M2) |
| `Rules/Layout/RH5206…CodeFixProvider.cs` | Target is the switch expression | RH5206-003 (M4) |
| `Rules/Layout/RH5303…`, `RH5304…`, `RH5305…`, `RH5306…`, `RH5307…CodeFixProvider.cs` | Target is the object creation / the initializer's member assignment / the collection expression / the list pattern / the bracketed argument list; `FindUnsupportedSyntax` asks exactly that node | RH5303-005, RH5304-004…RH5307-004, SHARED-FIX-010 (M7, M8) |
| `Rules/Layout/RH5308…CodeFixProvider.cs` | Target is the reported conditional, climbed to the outermost conditional it is directly nested in (keeps the stair, D17: inner `?` at 28); no statement/member needed, RH5308-006 guard gone | RH5308-004, RH5308-005 (M8, M9) |
| `RH5206`, `RH5303`–`RH5308` providers | Fix All: `DocumentOrderFixAllProvider` instead of `BatchFixer` (see *Disputed tests or scope*) | SHARED-FIX-002 |
| `Rules/Layout/RH5111…CodeFixProvider.cs` | Formatting node = innermost assignment / declarator / property / parameter / enum member (was: any enclosing assignment first) | RH5111-004 (D10) |
| `Rules/Layout/RH5113…CodeFixProvider.cs` | Moves only the semicolon (text replacement of the whitespace gap, which SHARED-JOIN-001 guarantees holds nothing else), after the shared-line helper | RH5113-003 (D10), SHARED-FIX-007 |
| `src/Reihitsu.Formatter.Test/Unit/BlankLines/BlankLineCollapserTests.cs` | Four `new BlankLineCollapser(new FormattingContext(…), ct)` calls → `new BlankLineCollapser(ct)` (constructor lost its unused context). No assertion, input, or expected output changed | — |

### Policy owners

- Leading trivia of a node-in-document target: `rg -n "originalFirstToken.LeadingTrivia" src/Reihitsu.Formatter` → `ReihitsuFormatter.FormatNodeInDocumentAsync` (l.250), `FormatNodeInDocumentWithContextAsync` (l.352), `FormatSplitFieldInContext` (l.794–795). One owner (the entry points); no fix restores it.
- Root list position / preceding token: `rg -n "RootPrecedingToken|RootListPosition|PrecedingTokenFacts.Resolve" src` → no hit.
- Chain write-back: `rg -n "FormatNodeInDocument" src/Reihitsu.Analyzer.CodeFixes/Core/FluentChainCodeFixHelper.cs` → one path per case (context entry point, or node entry point without context).

### Tests run

- `scripts/test.sh --configuration Debug --project formatter` → `Passed! - Failed: 0, Passed: 5184`.
- `scripts/test.sh --configuration Debug --project analyzer` → `Failed! - Failed: 2, Passed: 5917` — the two disputed tests below; every handed-over test passes.

### Disputed tests or scope

- **`RH5201MethodChainsShouldBeAlignedAnalyzerTests.VerifyFixMovesBreakInsideOperatorOfChainInConditionalExpression`** (existing, not in 06's list). Input `var r = flag ? a.B()?⏎    .C() : d;`. It asserts the fix also wraps the conditional around the chain (`var r = flag⏎ ? a.B()⏎ ?.C()⏎ : d;`). SHARED-CHAIN-009 (changed) says the rest of the statement stays as written; the code gives `var r = flag ? a.B()⏎                        ?.C() : d;` (`?.C` under `.B`, from `a` at column 23). The test asserts the removed fallback.
- **`RH5201MethodChainsShouldBeAlignedAnalyzerTests.VerifyFixAlignsInnerChainInsideWrappedOuterChain`** (existing). The reported chain is `node.Children⏎.Any()` inside a lambda argument of the outer chain `source⏎.Where(…)⏎.ToList()`. It asserts the fix also joins the outer chain (`return source.Where(…)⏎ .ToList();`). SHARED-CHAIN-009 says only the chain changes; the code leaves the outer chain and aligns `.Any()` under `.Children` (column 31). Same removed fallback.
  - Both are the class change 5 of the change doc decides for RH5201 ("The same holds for the RH5201 fix"); their visible output is covered by M2. They need rewriting by the test implementer.
- **Fix All provider of RH5206, RH5303–RH5308 changed to `DocumentOrderFixAllProvider`.** Needed for `RH5306…FixAllCorrectsEveryDiagnostic`, `RH5308…FixAllCorrectsEveryDiagnostic`, and `RH5308…VerifySiblingConditionalsInOneStatementAreDetectedAndFixed` (one Fix All iteration, SHARED-FIX-002): with `BatchFixer`, two narrowed constructs sharing a line are each laid out from the column the other fix later moves. I applied it to all seven narrowed providers, because the class (two reported constructs on one line) exists for each of them now. The nested Fix All pins (RH5303, RH5305, RH5308) pass with it.
- **RH5113 still `FormatsThroughFormatter => true` and still asks `FindUnsupportedSyntax(declarationNode)`**, although the fix no longer formats: the semicolon move is a text edit after the shared-line helper. Dropping the two guards would offer the fix in documents with a syntax error and for declarations holding a refused shape — a visible change no change doc lists — so I kept `main`'s withholding. The lead decides whether to keep them (and adjust the docs) or ask.
- **D8 only partly done for RH5513/RH5517 (`TargetAttributePlacementCodeFixProviderBase`).** Routing the whole fix through `FormatMemberOnLinesOfItsOwnAsync` broke four existing RH5511 tests (`VerifyCodeFixUsesSpacesRegardlessOfSurroundingTabIndentation`, `…WhenLineBeginsInsideMultiLineVerbatimString`, `…WhenAttributeListFollowsMultiLineParameterList`, `…WhenAttributeListFollowsMultiLineInitializer`): the base's own break *in front of* the member writes the canonical depth in spaces and breaks after any member/initializer tail, while the helper writes the line's indentation as written and breaks only on a line that begins with a member. So the private before-break stays; only the break *behind* the member uses the helper (after placing the list). See *Architecture findings*.
- Scope: the production paths match what 00-lead/06 name (entry point, facts deletion, chain helper, the listed fixes, the two bases); the Fix All switch is the only addition.

### Architecture findings

- **Shared-line break in front of a member has two owners**: `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync` (column = indentation of the member's line as written; only on a line that begins with a member of the list) and the private branch in `TargetAttributePlacementCodeFixProviderBase.ApplyCodeFixAsync`/`PlaceAttributeListAsync` (column = canonical depth, or the declaration's first-token column in an anchor scope; breaks whenever the gap to the sibling is whitespace). Input showing them disagree: `→private int _pad; [First] internal Example() { }` (tab-indented class body) — the helper writes `→[First]`, the base `····[First]`; the four RH5511 tests above pin the base's answer. Narrow fix made (helper for the break behind the member only).

### Follow-up candidates

- `ExpressionBodyToBlockCodeFixProviderBase` (RH3205, RH3206, RH3207) and RH3203, RH5401, RH5404, RH5408, RH5113 now separate members that share a line but keep `BatchFixer`; two reported members on one line (`int F() => 1; int G() => 2;`) are fixed by two fixes that both edit the gap. RH3202 uses `DocumentOrderFixAllProvider` for exactly this. Not measured; no test covers it.
- `PrecedingTokenFacts` now only wraps an in-tree token (no context fact); it and `TriviaFact` (still a named FMT-PIPE code owner, used only by `PrecedingTokenFacts`) could be replaced by reading the token directly. Kept because `TriviaFact` is a code owner the spec names.

## thoenissen/Reihitsu#1687

### Changes

| File | What changed | Requirement |
|---|---|---|
| `Base/ExpressionBodyToBlockCodeFixProviderBase{TNode}.cs` | Default `ApplyCodeFixAsync` runs a member through `FormatMemberOnLinesOfItsOwnAsync` with `ConvertBodyAsync` (local functions unchanged, D9) | SHARED-FIX-007 (D8) |
| `Rules/Clarity/RH3203…CodeFixProvider.cs` | Same through the helper | SHARED-FIX-007 |
| `Rules/Layout/RH5401…`, `RH5404…`, `RH5408…CodeFixProvider.cs` | Through `FormatMemberOnLinesOfItsOwnAsync` | SHARED-FIX-007 |
| `Rules/Layout/RH5113…CodeFixProvider.cs` | Helper, then the semicolon move | SHARED-FIX-007 |
| `Base/TargetAttributePlacementCodeFixProviderBase.cs` | List placed as before, then the helper separates a member that shares its last line with the member behind it | SHARED-FIX-007 |

### Policy owners

`rg -n "SharesLineWithSiblingMember|FormatMemberOnLinesOfItsOwnAsync\(" src -g '!*Test*'` → the helper (`BlankLineCodeFixUtilities` l.110, 149) and its callers RH3202, RH3203, the expression-body base, RH5113, RH5401, RH5404, RH5408, RH5417, the attribute base (l.95); the second owner is the attribute base's before-break (l.167), reported above.

### Tests run

All #1687 tests in 06 pass (within the full analyzer run above).

### Disputed tests or scope / Architecture findings / Follow-up candidates

See #1140 (attribute base, Fix All of the separating fixes).

## thoenissen/Reihitsu#1748

Covered by the entry-point change of #1140 (D5); no fix-specific change. All #1748 tests in 06 pass. _None_ disputed.

## Whole run

### Production diff size

`git diff --shortstat -- src ':(exclude,glob)src/*Test*/**'` (worktree vs d80eaf3): 32 files changed, 281 insertions(+), 532 deletions(-). No new type; one type deleted (`RootListPosition`). Within D5's threshold.

### Deleted special cases

_N/A_ — no shape is declared unsupported.

### Checks

- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors.
- `scripts/format.sh --force` over the changed files: `Formatted 0 of 33 file(s)`.
- `scripts/test.sh --configuration Debug --project architecture` → Passed 118/118; `core` → 682/682; `cli` → 281/281; `tooling` → 126/126; `playground` → 23/23.
- `scripts/lane-check.sh behavior-change --head worktree` → confirmed.
