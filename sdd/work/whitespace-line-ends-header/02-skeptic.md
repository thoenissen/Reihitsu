Role: skeptic
Commit: 19bbd7d
Verdict: proceed

The prompt carried no intended fix or hoped-for verdict. The issue's "Cause" paragraph and its "Done when" items were read as proposals, and the "Cause" paragraph was checked against the code, not taken as given.

## Premise

The issue is real, and more of it fails than the issue reports.

- **Callers that reach it.** (1) The RH7201–RH7207 code fixes go through `UsingDirectiveCodeFixUtilities`, then `ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync`, then `UsingDirectiveOrderingRewriter.OrganizeUsingDirectives`, then `UsingLeadingTriviaBuilder.SplitOriginalFirstHeaderTrivia`, then `GetHeaderSplitIndex`. (2) Whole-document formatting and the CLI (`reihitsu-format`) go through `UsingDirectiveOrderingPhase` to the same rewriter. In both paths, the only phases that run before the using phase are precheck, preparation (tab replacement and line endings), structural transforms, regions, and documentation comments, so the whitespace on the line is still there when the split is decided. Only the cleanup phase removes it, and that phase runs last. The split only runs when the canonical order moves the original first directive (`UsingDirectiveOrderingRewriter.cs:99`). If the block holds any directive, `UsingDirectiveOrderingSafety.CanSafelyReorder` refuses the reorder, so directives and disabled text cannot reach the split.
- **The rule in the specs.** SHARED-BLANK-001 (unchanged, `sdd/specs/shared/blank-line-spacing.md:20`) already requires that analyzer and formatter count a line holding only whitespace as blank, and the line below a `///` comment too. So the issue's statement "the spec does not say" is overstated. The issue also cites SHARED-FIX-007 as its principle, which is wrong: the *Moved code* bullet there explicitly excludes "a using block that a fix rewrites". The governing requirement is SHARED-BLANK-001.
- **Cause, read from the code.** `GetHeaderSplitIndex` (`src/Reihitsu.Formatter/Pipeline/UsingDirectives/Utilities/UsingLeadingTriviaBuilder.cs:161-174`) only counts two adjacent `EndOfLineTrivia` as a blank line.
- **Runs.** I ran 13 fixtures (below) through `scripts/apply-fix.sh RH7203` (LF and CRLF: every fixture was `fixed`, 1 iteration, and CRLF had the same statuses as LF), and through `scripts/format.sh` twice. The two format passes gave byte-identical results. The issue's input reproduces exactly as reported, in both the fix and the CLI. In the CLI output the four spaces are gone, but `// About Linq` stays at the top.
- **Not caught by an idempotency check.** Pass 1 cleans the whitespace-only line and leaves the block in canonical order, so pass 2 never splits again. The wrong placement becomes a stable fixed point, and an idempotency test cannot catch this defect (FC-017).

## Mechanism and candidates

**Mechanism.** The header split recognizes a blank line only as two adjacent `EndOfLineTrivia` in the original first directive's leading trivia. So it misses every blank line that the trivia list represents differently: whitespace trivia between the two line breaks, or a first line break carried inside a `///` comment's own structured trivia. When the missed blank line is the last one among the comments, the split falls back to an earlier blank line or to "no blank line".

**Where it is dispatched.** `UsingDirectiveOrderingRewriter.OrganizeUsingDirectives` calls `SplitOriginalFirstHeaderTrivia`, which calls `GetHeaderSplitIndex`. That is the only call site, and only the original first directive's leading trivia reaches it.

**Candidates.** Inside a block that can be reordered, the trivia kinds that can make up a line among those comments are `WhitespaceTrivia`, `EndOfLineTrivia`, `SingleLineCommentTrivia`, `MultiLineCommentTrivia`, `SingleLineDocumentationCommentTrivia` (which carries its own line break; `SyntaxTriviaUtilities.EndsLine`), and `MultiLineDocumentationCommentTrivia` (which does not). Directive trivia and `DisabledTextTrivia` are excluded by `CanSafelyReorder`.

I searched the other blank-line checks in the using code with `rg EndOfLineTrivia` over `Pipeline/UsingDirectives`, `Reihitsu.Core/UsingDirective*`, and the RH720x analyzers and fixes. `UsingDirectiveOrderingUtilities.HasBlankLineAbove` (FMT-USING-002/004/010, RH7207) counts line breaks and ignores whitespace, so it is correct for this input, and no other adjacency check exists. The class is limited to `GetHeaderSplitIndex`.

## Breaking inputs

Failure classes that reach the changed code:

- FC-002: the "blank line" qualifier never became the predicate's condition.
- FC-015: a private measure of "blank line" next to the shared one (see *Architecture findings*).
- FC-016: sibling shapes beyond the reported one.
- FC-040: with `///`, the outcome depends on the documentation mode. With `DocumentationMode.None`, `///` is a plain comment followed by its own `EndOfLineTrivia`, and the split already works.
- FC-033 and FC-017: in the whole-document path, preparation turns a tab into spaces and cleanup removes the whitespace, and the CLI result is idempotent. Assert the exact single-pass output in both paths, not convergence.
- FC-003: the boundary needs one test on each side. A whitespace-only line that is the last blank line fails; one followed by an empty line holds.

Class matrix. The axes are the content of the last blank line (spaces, tab, empty, a line below `///`), where it stands (between header and attached comment, between two header comments, directly above the directive), whether there are other blank lines, the scope (compilation unit or namespace body), and the line ending. Every fixture's input ends with `using System.Linq;` and `using System;` above `class C { int x; }`. Results were the same for LF and CRLF.

| # | Lines above the first using (`·` = a space, `→` = a tab) | Fix (RH7203) today | CLI today | Fails? |
|---|---|---|---|---|
| m01 | `// Header` / `····` / `// About Linq` (the issue's input) | `// About Linq` stays at the top | same; spaces cleaned | **fails, both** |
| m02 | `// Header` / `→` / `// About Linq` | same as m01; the tab stays | same as m01 (preparation already turned the tab into spaces) | **fails, both** |
| m03 | `// Header` / `····` / `··` / `// About Linq` | same as m01 | same as m01 | **fails, both** |
| m04 | `// Header` / (empty) / `····` / `// About Linq` | split at the empty line; the whitespace-only line, which is the header's last blank line, is **dropped** (`// Header`, empty, `using System;`, empty, `// About Linq`, …) | correct (the blank-line phase collapses the two blank lines) | **fails, fix only** |
| m05 | `// Header` / `····` / (empty) / `// About Linq` | correct (the last blank line is empty) | correct | holds |
| m06 | `// A` / `····` / `// B` / (empty) / `// About Linq` | correct | correct | holds |
| m07 | `// A` / (empty) / `// B` / `····` / `// About Linq` | header comment `// B` **moves down** with `using System.Linq;` | same | **fails, both** |
| m08 | `// Header` / `····` (no comment below) | correct (the whole list is the header either way) | correct | holds |
| m13 | `// Header` / `····` / `····` (no comment below) | correct | correct | holds |
| m09 | namespace body: `····// Header` / `········` / `····// About Linq` | same as m01 | same as m01 | **fails, both** |
| m10 | `/// Doc` / (empty) / `// About Linq` | `// About Linq` stays at the top | `/// Doc` / `// About Linq` / `using System;` … | **fails, both** |
| m11 | `/// Doc` / `····` / `// About Linq` | same as m10 | same as m10 | **fails, both** |
| m12 | `/* Header */` / `····` / `// About Linq` | same as m01 | same as m01 | **fails, both** |

Expected outputs, derived from the unchanged FMT-USING-009/010/011, SHARED-BLANK-001, FMT-BLANK-010, and FMT-CLEAN-001. The test implementer derives them again from the specs, not from this table.

- **Fix path (FMT-USING-011: the header's lines stay as written).**
  - m01: as in `01-spec-author.md`.
  - m04: `// Header` / (empty) / `····` / `using System;` / (empty) / `// About Linq` / `using System.Linq;`.
  - m07: `// A` / (empty) / `// B` / `····` / `using System;` / (empty) / `// About Linq` / `using System.Linq;`.
  - m10: `/// Doc` / (empty) / `using System;` / (empty) / `// About Linq` / `using System.Linq;`.
- **Whole document.**
  - m01–m03, m09, m12: as in the spec's new example. The whitespace is cleaned; m09 is at namespace indentation.
  - m07: `// A` / (empty) / `// B` / (empty) / `using System;` / (empty) / `// About Linq` / `using System.Linq;`.
  - m10 and m11: `/// Doc` / `using System;` / (empty) / `// About Linq` / `using System.Linq;`. The trace shows that the blank-line phase removes the blank line below `///` today (FMT-BLANK-010); that phase is not involved in this change.

Candidates the test implementer must cover:

- m01 in LF and CRLF, fix and whole document (the reproduction).
- m02 in the fix path (that is the only path where the tab survives).
- m04 in the fix path.
- m05 as the holding side of the boundary.
- m07.
- m09.
- m10 (`///` with the documentation mode parsed, FC-040).
- m08 as a holding member.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `UsingLeadingTriviaBuilder` header split (formatter, using phase) | must change |
| `UsingDirectiveOrderingRewriter` | must stay; it already adds the FMT-USING-010 blank line once the split yields a remainder led by a comment |
| `UsingDirectiveOrderingUtilities.HasBlankLineAbove` (Core), RH7207 analyzer | must stay; it already counts a whitespace-only line |
| RH7201–RH7207 analyzers | not involved; they report order and grouping, not header placement |
| RH7201–RH7207 code fixes | must stay in code; their output changes through the phase (FMT-PIPE-030). Fix All is not affected beyond the same phase output |
| Other formatter phases (preparation, blank lines, cleanup) | must stay |
| `sdd/specs/formatter/using-directives.md` | changed by the spec author; see *Internal choices* |
| Rule pages | not involved |
| Tests | must change: `UsingLeadingTriviaBuilderTests` (unit), formatter using-phase tests, RH7203 fix tests |

## Refuse option

- **Leave the construct as written.** The using phase would not reorder, or would not move the comment, when the original first directive's leading trivia holds a whitespace-only line. This leaves an unsorted block or a misplaced comment for code people commonly write: editors often leave indentation on blank lines, especially inside a namespace body. That makes it not a rare class, and the refusal would itself need the same check that recognizes the line. It costs more than the requested change, which is a few lines in one private method.
- **Declare the shape unsupported.** This is not a real option. A whitespace-only line among the comments above the first using deletes no special case anywhere, and AGENTS.md says a shape that deletes nothing is not declared. Nothing to escalate.
- **Simpler options.** The SHARED-BLANK-001 measure already exists in `Reihitsu.Core` as `TokenGapAnalysis` (`OfTriviaRange`, `IsBlankLine`), and so does `SyntaxTriviaUtilities.EndsLine`, which recognizes the line break carried inside `///` trivia. The lead weighs which one to use under *Architecture findings*. Any rewrite of the issue's prescribed `GetHeaderSplitIndex` is acceptable as long as the class above passes.

## Output questions

_None._ The unchanged FMT-USING-009/010/011, SHARED-BLANK-001, FMT-BLANK-010, and FMT-CLEAN-001 decide every member's output in both paths. There are no two readings: SHARED-BLANK-001 binds both analyzer and formatter, so "empty line only" is not a valid reading of FMT-USING-009.

## Internal choices

1. **The restated definition in FMT-USING-009.** The clarification both cites SHARED-BLANK-001 and partly restates it ("a line that holds nothing but whitespace before its line break … an empty line and a line that holds only spaces or tabs"). The restatement leaves out SHARED-BLANK-001's other clauses: the line below a `///` comment counts, and lines inside comment text or disabled text do not. A reader of FMT-USING-009 alone could therefore miss m10/m11. There are two options:
   - (a) Trim the sentence to the citation plus "a line holding only whitespace counts like an empty line".
   - (b) Keep it as is.

   Neither changes output. Recommendation: (a), or keep it and add "and the line below a `///` comment" so the restatement does not read narrower than the shared requirement. The spec-checker run passes either way.
2. **The `///` members (m10, m11).** They are rare: a `///` above a using documents nothing and gets CS1587. They are in the class because SHARED-BLANK-001 names them explicitly, and the cheap check (`EndsLine`) already exists. Recommendation: include them in this run rather than filing a follow-up. Covering them adds no production lines beyond the check that treats a line break carried in trivia as a line break.

## Issue goals

The issue carries `agent-filed`.

- **"Required output" (no blank line above `// About Linq`).** Drop it. It contradicts the unchanged FMT-USING-010, which counts the header's blank line as standing directly above the moved comment. The issue itself defers to "the blank-line requirements around it". The shipped output will differ from the issue's block, but an unchanged requirement demands the difference, so no output is in question. State it in the PR.
- **Principle "SHARED-FIX-007".** Change it to SHARED-BLANK-001. SHARED-FIX-007 excludes rewritten using blocks. No visible change.
- **"The spec does not say".** Overstated, as above. No visible change.
- **"`GetHeaderSplitIndex` treats it so".** This prescribes a solution. Keep the behavior and not the method name; the lead decides the structure.
- **"A test covers the input above for LF and CRLF, formatter and RH7203 fix".** Keep it, and widen it to the class members listed under *Breaking inputs*. The reported instance alone misses m04, m07, and m10 (FC-016). No visible change beyond what the requirements demand.

## Architecture findings

- **Components.** `UsingLeadingTriviaBuilder.GetHeaderSplitIndex` (formatter, private), `UsingDirectiveOrderingUtilities.HasBlankLineAbove` (Core), and `TokenGapAnalysis` (Core).
- **The decision they share.** Whether a line in a trivia list is blank. SHARED-BLANK-001's *Why* names `TokenGapAnalysis` as "the one measure".
- **The input that shows the disagreement.** Fixtures m01, m04, m07, and m10. On the same whitespace-only line, `HasBlankLineAbove` counts it as blank (FMT-USING-010 holds for a whitespace-only line between two directives) while `GetHeaderSplitIndex` does not. The run's defect is this drift (FC-015).
- **Scope.** The run changes this component anyway, and the class includes code people commonly write. `HasBlankLineAbove` showed no failing input in this run.

## Follow-up candidates

_None._ The FMT-BLANK-010 removal of the blank line below a `///` that documents nothing (m10, whole document) is existing, specified behavior outside this issue.

Fixtures were built in the session scratchpad, outside the repository, and are not committed.
