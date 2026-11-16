Role: implementer
Commit: 993f700
Verdict: tests pass

Inputs: `00-lead.md` (*Decisions taken* row 3: one owner of the argument-list `(` gap), `02-skeptic.md`, `05-test-implementer.md`, `sdd/changes/2026-11/argument-list-below-name.md` (Accepted), `git diff origin/main -- sdd/specs`. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Before the change the handed-over formatter filter failed exactly as `05-test-implementer.md` says (29 failed, 202 passed).

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ArgumentListLineStartRewriter.cs` (new) | The one owner of the gap in front of an argument list's `(`: joins it through `LineBreakTriviaUtilities.JoinGaps` when a line break separates it from the token in front, that token lies inside the root, and `WouldJoinAcrossUnjoinableTrivia` is false. Owners covered: invocation, object creation, implicit object creation, constructor initializer, primary-constructor base type, `with(…)` element (`IsArgumentListOpenParenthesis`) | FMT-BREAK-039, FMT-BREAK-031, FMT-BREAK-001, FMT-BREAK-030, FMT-PIPE-017 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/LineBreakPhase.cs` | Runs the new rewriter after `LineBreakInitializerRewriter` (FMT-BREAK-022 constructs judged as written) and before `ListElementInteriorJoinRewriter`, `LineBreakListRewriter`, `BinaryOperatorLineBreakRewriter`, `ChainLineBreakRewriter`, `TernaryLineBreakRewriter` (lists and conditionals judged as joined); summary names the join | FMT-BREAK-039 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ChainLineBreakRewriter.cs` | No longer closes the gap in front of an attached argument list; element accesses and conditional element accesses unchanged; summary and inline comment updated | FMT-BREAK-020 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ListElementInteriorJoinRewriter.cs` | `IsOwnedGap` treats the argument-list `(` gap as owned by the new rewriter; remarks list it | FMT-BREAK-015 |
| `AGENTS.md` | Phase-ownership table, `LineBreakPhase` row: names the join and `ArgumentListLineStartRewriter.IsArgumentListOpenParenthesis` among the guards | — |

## Policy owners

- `rg -n "ArgumentListSyntax|ArgumentList\.OpenParenToken|FluentChainAttachedPartKind\.ArgumentList" src/Reihitsu.Formatter src/Reihitsu.Core --glob '!*Test*'`: before the change the gap was joined by `ChainLineBreakRewriter.NormalizeChain` (attached part `ArgumentList`, from `FluentChain.cs:367`) and `ListElementInteriorJoinRewriter.CanJoin`; now only `ArgumentListLineStartRewriter`. The other hits (`LineBreakListRewriter.VisitArgumentList`, `ArgumentAlignmentContributor`, bracketed/attribute/type-argument lists) decide other gaps or columns, not this one.
- Join separator: reused `LineBreakTriviaUtilities.JoinGaps`/`CreateJoinSeparator`; blocking trivia: reused `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia`; line-break fact: reused `SyntaxTriviaUtilities.AreSeparatedByEndOfLine`. No private copy.

## Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 4 files changed, 160 insertions(+), 5 deletions(-) (the new file is ~140 of these, mostly XML documentation). New type: `ArgumentListLineStartRewriter` (internal sealed rewriter).

## Deleted special cases

_N/A._

## Tests run

- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ArgumentListLineStartTests|FullyQualifiedName~ChainLinkLayoutTests|FullyQualifiedName~JoinSeparatorPhaseTests|FullyQualifiedName~ReihitsuFormatterNodeColumnTests|FullyQualifiedName~ReihitsuFormatterDetachedRootTests"` → Passed! Failed: 0, Passed: 231
- `scripts/test.sh --configuration Debug --project analyzer --filter "Name~VerifyFixJoins|Name~VerifyFixKeepsOpeningParenthesisBelowTheInvokedName"` → Passed! Failed: 0, Passed: 28
- `scripts/test.sh --configuration Debug --project architecture` → Passed! Failed: 0, Passed: 153
- `scripts/test.sh --configuration Debug` (default run) → Analyzer 10435, Formatter 5313, Core 738, Cli 303, Architecture 153, Tooling 133, Playground 23 — all passed, 0 failed.

## Disputed tests or scope

- New type `ArgumentListLineStartRewriter` rather than extending `ChainLineBreakRewriter` or `ListElementInteriorJoinRewriter`: neither one's scope (chains with a link; list elements) covers the gap everywhere, and *Decisions taken* row 3 asks for one owner, so the gap moved out of both into a rewriter of its own. Its `IsArgumentListOpenParenthesis` is `internal` because `ListElementInteriorJoinRewriter.IsOwnedGap` reads it.
- `AGENTS.md` edited (phase table row only) so the listed guards name the new owner; the lead may move that to the spec author or cleaner.

## Architecture findings

_None._ The finding of `02-skeptic.md` (one gap, three statements) is resolved in code as row 3 decided.

## Follow-up candidates

_None_ beyond F1–F3 in `00-lead.md`.
