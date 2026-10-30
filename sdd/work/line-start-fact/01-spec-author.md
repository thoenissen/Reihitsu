Role: spec author
Commit: 281e823
Verdict: change doc written

The prompt carried no intended fix or suspected cause beyond the issue text itself; the issue's proposed code structure (`Pipeline/Core` facts, helper moves, `ReihitsuFormatterHelpers` split, one-contributor-per-line test) is implementation and is not written into any spec.

## Statements of the issue checked against `main` (802afbf)

Probed with `scripts/trace.sh` (formatter) and `scripts/apply-fix.sh` (analyzer + fix); inputs live only in the scratchpad.

- Example 1 (`int A; /// <summary>end</summary>` then `}` in column 10): reproduces; the formatter changes nothing but the final newline, `}` stays in column 10. RH5204 reports nothing.
- Example 2 (`typeof(Dictionary<,` / `>);`): reproduces; pass 1 IndentationPhase puts `>` in column 16. Additionally, in pass 2 `LineBreakPhase` rewrites that line back to column 0 and `IndentationPhase` moves it to 16 again (phase-level ping-pong on the same input).
- Example 3 (block comment with a lone `\r` inside, LF file): reproduces; the comment stays in column 12, while the same comment with `\n` inside moves to column 8. Also reproduces with U+2028 inside, and for a whole lone-`\r` file whose comment holds a lone `\r`.
- Every named symbol still exists: `SyntaxTokenPositionUtilities.IsFirstOnLine`, `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex`/`GetLineBreakLength`, `SyntaxTriviaUtilities.AreSeparatedByEndOfLine`, `LineBreakTriviaUtilities.HasLeadingEndOfLine`/`HasTrailingEndOfLine`/`StripTrailingWhitespace`, `ReihitsuFormatterHelpers.StartsOnNewLine`/`StartsOnNewLineIncludingDocumentation`, `PrecedingTokenFacts.EndsLine`, `IndentationRewriter` `startsAtLineStart`/`CountEmbeddedNewLines`, `FieldDeclarationSplitTransform.EndsLine`, the `\n`-only check in `SwitchCaseWhenLineBreakRewriter`, the `\r`/`\n` check in `ChainLineBreakRewriter`, `BlankLineTriviaUtilities.EndsWithLineBreak`, `DocumentationCommentUtilities.IsLineTerminator`, `TokenLocator.ContainsToken`, `ConfigurableStructuralTransforms` under `StructuralTransforms/Enumerations/`, `HorizontalSpacingRewriter.TrimGapBeforeToken`. `Pipeline/Core/Utilities` already exists. Nothing the issue states was made moot by B1, except that between-token line breaks are now the detected line ending, so other terminators remain only in comment, literal, and disabled-text interiors (as the issue anticipates).

Controls with `//` instead of a stranded `///` (today's output, which the change doc makes the target for `///`): `int A // stray` / `;` → `;` column 4; `return 1 // stray` / `;` → column 8; `var x = a // s` / `+ 1;` → `+` column 16; `M(a, b // s` / `);` → `)` column 10; `b[0 // s` / `];` → `]` column 18; block `}` → its block column; chain `items.Where(…) // x` / `.Select(…)` / `.ToList()` → links in column 20, comment stays. With `///` all of these keep the author's column today.

Additional outputs found that change under the shared rule (named in the change doc):

- Chain with a stranded `///` before `.Select`: today `LineBreakPhase` moves `/// x` onto its own line (against FMT-DOC-003) and pass 2 `BlankLinePhase` adds a blank line above it (against FMT-PIPE-004). Target: comment stays, links in column 20.
- RH6002 today reports `N(1 /// x` / `, 2);` and its fix writes `N(1/// x` (removes the space in front of the comment, against RH6002-004); it also reports `N(1,   /// x` / `2);`. The formatter keeps both gaps. Target: neither is reported.
- RH5201 today reports the chain above and its fix moves `/// x` onto its own line (same output as the formatter today). With the first link behind a stranded `///` (`items /// x` / `.Where(…)` / `.ToList()`), RH5201 reports nothing today while the `//` version is reported and fixed to column 15; the formatter leaves the `///` version at column 12 and moves the `//` version to 15.
- RH5203 does not report `N(1, /// x` / `  2);` today (argument not seen as starting its line).

## Changed requirements

- SHARED-BLANK-008 — A token starts its line after the same line breaks as trivia — added (`sdd/specs/shared/blank-line-spacing.md`; `SyntaxTokenPositionUtilities` added as code owner there and in `sdd/specs/README.md`; ledger updated)
- FMT-INDENT-002 — Block indentation follows the scope depth — changed (first token of a line by SHARED-BLANK-008, also behind a `///` written behind code; the stranded comment's own lines keep their columns)
- FMT-DOC-003 — Stranded documentation stays as it was written — changed (the line behind the comment is placed; second example `;` 8 → 4; new chain example)
- FMT-SPACE-001 — Only gaps on one line are changed — changed (the line break ending a `///` counts; a multi-line comment's interior break does not; new example)
- FMT-SPACE-015 — Indentation after a single-line documentation comment is kept — retired (folded into FMT-SPACE-001)
- RH6002-001, RH6002-002 — changed (starts/ends its line by SHARED-BLANK-008, also around a `///`)
- RH5203-001, RH5204-001, SHARED-CHAIN-002 — changed (reference SHARED-BLANK-008 for "starts its line"/"first token")
- Bugs against unchanged requirements, no spec change: FMT-INDENT-015 (`>` to column 8), FMT-INDENT-006 (comment with lone `\r` or U+2028 inside is re-indented).

Change doc: `sdd/changes/2026-10/line-start-fact.md`, `Status: Draft`, needs the maintainer's approval of What and Why (escalation 2) before tests, including its section *Analyzer diagnostics follow the shared rule*, which the issue says is part of the change doc or goes back to the maintainer. The folder month assumes the merge happens in October 2026; move it if it merges later.

`documentation/rules/` is left unchanged: RH5203, RH5204, RH6002, and RH5201 change only for code with a stranded `///` comment, a corner case that does not change what is reported or how to fix it in the normal case.

Uses rows updated: blank-line-spacing (FMT-INDENT-002, FMT-INDENT-015), indentation (SHARED-BLANK-008, FMT-DOC-003), documentation-comments (FMT-INDENT-002, FMT-INDENT-029), horizontal-spacing (SHARED-BLANK-008), RH5203, RH5204, RH6002, fluent-chain-layout (SHARED-BLANK-008).

Architecture check (`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage"`): 49 passed, 2 failed, both expected until tests are written:
- `EveryRequirementReferenceNamesAnActiveRequirement`: nine tests still tag the retired FMT-SPACE-015 — `StrandedDocumentationTokenSpacingTests.cs` lines 21, 42, 62, 83, 101, 122, 143, 186 and `HorizontalSpacingRewriterTests.cs:663`.
- `EveryRequirementOfAnEnforcedSpecIsCovered`: SHARED-BLANK-008 has no covering test.

Code owners: if the implementer replaces or renames `SyntaxTokenPositionUtilities` (or `TokenGapAnalysis`), the SHARED-BLANK code-owner row and the `sdd/specs/README.md` row have to follow; the `AGENTS.md` ownership table names `AreSeparatedByEndOfLine` and `TrimGapBeforeToken` as guards and has to follow any rename too.

## Examples for the tests

- SHARED-BLANK-008 — RH5204 brace behind a stranded `///` (reported), brace behind `b */` (not reported), the two RH6002 comma lines (not reported), `Dictionary<,` / `>` (formatter, `>` to column 8).
- FMT-INDENT-002 — the two new examples (brace placed; continuation `///` line of a stranded comment kept in column 8).
- FMT-DOC-003 — the changed `;` example and the new chain example.
- FMT-SPACE-001 — the new `return 1    /// stray` / `;` example (unchanged by the whole formatter; also the phase-level unit test currently tagged FMT-SPACE-015).
- FMT-INDENT-015 — the issue's example 2; `WrappedAngleBracketListTests.UnboundGenericWithZeroWidthAnchorIsNotAlignedToADegenerateColumn` currently expects column 16.
- FMT-INDENT-006 — the issue's example 3 with a lone `\r`, with U+2028, and a whole lone-`\r` file.
- Change doc, *Outputs that change*: the stranded-`///` lines take the column of their `//` counterpart (controls listed above). `StrandedDocumentationTokenSpacingTests` asserts unchanged output for `;`, `]`, `,`, `)`, and `.` lines behind a stranded `///`, including `KeepsAuthoredColumnOfOverIndentedTerminatorBelowStrayDocumentationComment`, which pins the author's column; the change doc changes those expectations to the `//` counterpart's output.

## Open questions for the maintainer

_None._

## Follow-up candidates (outside this run's scope)

- RH5204 fix does not converge on
  ```csharp
  class C
  {
      void M()
      {
          var x = 1; /// stray
              if (x > 0)
              {
              }
      }
  }
  ```
  `apply-fix.sh RH5204` reports `no-progress (2 iteration(s))`: the fix moves `/// stray` onto its own line below a blank line and leaves `if`, `{`, `}` in column 12. Not checked whether this run's change alters it; the skeptic may want it as a probe.
- RH5048: `CommentPositionUtilities.IsOrdinaryComment` accepts only `SingleLineCommentTrivia` and `MultiLineCommentTrivia`, so a parsed `///` that documents nothing is not an ordinary comment there, while SHARED-COMMENT-001 counts it as one. Derived from reading code only: `apply-fix.sh` cannot run a rule without a code fix, so it is unverified.
