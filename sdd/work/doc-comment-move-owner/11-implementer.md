Role: implementer
Commit: 76ad555
Verdict: tests pass

Repair round 1: decision #13, findings #1 and #2 from 09-auditor.md. My prompt carried no intended fix or suspected cause. It noted that the auditor's suggested change is only a suggestion. I derived the repair from the failing tests and from where the trivia goes.

I did not run the 24 tests in isolation on 76ad555 before changing code. Instead I built the `Reihitsu.Cli` of 76ad555 and ran it over the differential corpus described below. It differs from `main` in exactly the auditor's members: every conditional `?`/`:` context, 32 fixtures each, and nothing else. That matches the failures reported in 10-test-implementer.md. All 24 pass now. No test was changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/DocumentationComments/DocumentationCommentFormattingPhase.cs` | `RegisterRelocation`: a delimited documentation comment that the phase moves now gets its line break in front of it, in the owning token's leading trivia. That is the place `main`'s blank-line phase used. The token in front only loses the whitespace that ended its trailing trivia (a zero count in `previousTokenRelocationCounts`), so it no longer ends its line in the trivia. A single-line `///` comment keeps its previous placement unchanged. The documentation phase is still the only owner of the move. Finding #2: the class summary is now qualified with FMT-DOC-001's conditions, `MoveBeforeOwningToken`'s `<returns>` says "onto a new line", and the `previousTokenRelocationCounts` parameter doc covers the zero count. | FMT-DOC-001, FMT-BREAK-018 (unchanged) |
| `src/Reihitsu.Formatter/Pipeline/UsingDirectives/Rewriter/UsingDirectiveOrderingRewriter.cs` | In the reorder path, a directive that does not begin a line (it is not a new group and its predecessor's rebuilt trailing trivia has no line break) keeps one line break in front of its leading comment when its original leading trivia had a line break before its first significant trivia. Without this, `CreateLeadingTrivia` dropped the line break the documentation phase writes, which put `/** doc */` back behind `using System;`. Parsed input never has such a line break while the predecessor's trailing trivia lacks one, so only the documentation phase's output reaches this branch. | FMT-DOC-001 (moves behind a using directive, pinned by `SeparatesDelimitedDocumentationFromPrecedingUsingDirectiveInOnePass` and `SameGroupDirectiveBehindMultiLineDelimitedDocumentationCommentKeepsTheCommentWithIt`) |
| `src/Reihitsu.Formatter/Pipeline/UsingDirectives/Utilities/UsingLeadingTriviaBuilder.cs` | New `HasLineBreakBeforeSignificantTrivia`. It reuses `SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex`. | — |

Why the placement moved: `TernaryLineBreakRewriter` reads whether `?`/`:` ends its line in the trivia. With the line break in the operator's trailing trivia, it refused to join across the comment and left the operator at the end of its line. The text is identical either way, but the trivia shape decides that rewriter's output. The leading-trivia placement is the shape `main` handed to the line-break phase. That shape broke two using-directive tests, because the using phase rebuilds leading trivia from the first comment on. Hence the second change.

## Policy owners

- Move of a documentation comment written behind code: `rg -n "previousTokenRelocationCounts|MultiLineDocumentationCommentTrivia" src/Reihitsu.Formatter --glob '!*Test*'`. The only owner of the move is `DocumentationCommentFormattingPhase.RegisterRelocation`. `UsingTrailingTriviaBuilder:191` only measures whether a leading documentation comment spans a line. `ChainLineBreakRewriter:131` and `RawStringLineShifts:508` read the trivia kind for their own decisions. The using-phase change keeps an existing line break and decides no move.
- First significant trivia: `rg -n "HasLineBreakBeforeSignificantTrivia|FindFirstSignificantTriviaIndex\(" src --glob '!*Test*'`. The single owner is `SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex` in Core. The new helper calls it and is used once.

## Tests run

- `scripts/test.sh --configuration Debug` (all projects except self-hosting): Analyzer 6339/6339, Formatter 3518/3518, Core 535/535, Cli 251/251, Architecture 92/92, Tooling 125/125, Playground 17/17.
- After the comment-only edits (finding #2): `scripts/build.sh` (Release) gives 0 warnings and 0 errors. Formatter 3518/3518, `--project analyzer --filter "FullyQualifiedName~RH3202"` 36/36, Architecture 92/92.
- Differential against `main` (8c7d5bec): I built `Reihitsu.Cli` from a `main` worktree and from the working tree. The corpus has 2890 fixtures, each run in LF and CRLF. It covers 87 contexts × 4 delimited shapes (`/** d */`, a multi-line `/** a … b */`, `/** d */ /** e */`, `/** d */ /* c */`) × 4 gaps (direct, no space, behind `/* c */`, behind a multi-line `/* x … */`), plus the `///` form in every context. The contexts are the auditor's failing members and every holding token kind the auditor lists:
  - ternary contexts: one-line, wrapped, nested, `:` already at its line start, and assignment;
  - `,` in argument, initializer, enum, parameter, type-parameter, base and switch-arm lists;
  - `+`, `==`, `&&`, `||`, `??`, `is`;
  - `.`, `?.`, and a wrapped chain;
  - `=`, `+=`, and the expression-body, lambda and switch-arm `=>`;
  - `(`, `[`, `<`;
  - the named-argument, base-list and constraint `:`, and `case … when`;
  - `return`, `throw`, `in`, `where`;
  - `;` and `}` before members and statements, `{` with code and with a member, `;}`, a switch label, a label, `else`;
  - using, regroup, reorder, alias, static, extern alias, global using, assembly attribute, namespace-scoped usings, file-scoped namespace with and without usings;
  - top-level statements, local function, accessor, attribute lists, record, interface, lambda block;
  - verbatim and raw literals in front, and `#if` and `#region` behind;
  - property, event, constructor, nested type, cast, interpolation, tuple, `typeof`, default parameter, query.
- Differential results: `main` vs head gives 0 differing files (all 2890 are reformatted, so all were parsed and formatted). A second pass changes nothing on `main` and nothing on head. The 76ad555 CLI differs from `main` in 256 files, all in the 8 ternary contexts, so the corpus detects the finding.

## Disputed tests or scope

Scope growth: the repair touches the using-directive phase (`UsingDirectiveOrderingRewriter`, `UsingLeadingTriviaBuilder`), a production path the handovers did not name. It is one guarded branch and one helper, both small. Without it, the leading-trivia placement fails two existing using tests. I wrote no unit test of `HasLineBreakBeforeSignificantTrivia`. The two full-pipeline using tests above fail without it.

## Architecture findings

_None._ The trivia shape is now the one `main` handed to the line-break and using phases. One component still owns each decision: the documentation phase decides the move; the using and line-break phases only read or keep the resulting trivia.

## Follow-up candidates

_None_ beyond the `///`-behind-`?`/`:` candidate that 09-auditor.md already names. That candidate is unchanged: the `///` placement still uses the previous token's trailing trivia, and its output still equals `main`.
