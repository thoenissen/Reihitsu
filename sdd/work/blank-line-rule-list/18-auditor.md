Role: auditor
Commit: 1168c0c4
Verdict: pass

My prompt carried no intended fix, no suspected cause, and no hoped-for verdict.

## Scope

This is a repair-delta audit of repair round 2 (range fe6b6845..1168c0c4), measured against my earlier audit of 84726e2 (`14-auditor.md`). Head `1168c0c4588f32f7ba2acb220c9f96c5941c4923` is the checkout. `origin/main` resolves to `99ae644d69a2f520c3a8786fdb601389819ff267`, the same SHA as `git ls-remote origin refs/heads/main`, and that commit is an ancestor of the head.

The diff matches the handovers (`15-test-implementer.md`, `16-cleaner.md`, `17-spec-checker.md`):
- Two tests are added to `src/Reihitsu.Formatter.Test/Unit/BlankLines/BlankLineRegionDirectiveRewriterTests.cs`.
- A new class, `src/Reihitsu.Formatter.Test/Unit/BlankLines/BlankLineTriviaBoundaryRewriterTests.cs`, adds three tests.
- In `src/Reihitsu.Formatter.Test/Regression/BlankLines/RegionDirectiveLineAboveTests.cs`, four tests are renamed and their summaries narrowed. Their bodies and expected values are unchanged.
- Everything else is under `sdd/work/`.

No production code, spec, script, or rule page changed, so scope did not grow. The earlier audits' evidence for `src/Reihitsu.Formatter`, `src/Reihitsu.Core`, and the analyzer still holds, because those files are byte-identical to 84726e2.

Evidence gathered in this audit:
- **Static trace of the reader for each new test.** `BlankLineEditor.IsMissingRequiredBlankLineBeforeRegionDirective` is at `BlankLineEditor.cs:79`.
  - `WhitespaceOnlyLineAboveRegionIsTakenForRequiredBlankLine`, and both `#endregion` tests: `trivia[lineStartIndex - 2]` is the whitespace-only line's `WhitespaceTrivia`. The skip runs off the start of the list (`triviaIndex < 0`), and `isLineAboveBlank` comes from `previousToken.EndsLine`.
  - `WhitespaceOnlyLineBetweenCommentAndRegionIsTakenForRequiredBlankLine`: the skip ends on the comment's `EndOfLineTrivia` (`triviaIndex >= 0`).
  - Without the skip, `SyntaxTriviaUtilities.EndsLine(whitespace)` is false. `EnsureBlankLineBeforeRegionDirectives` would then insert a line break, and the rewriter alone has no collapser to remove it.
- **Mutation, run independently on a scratch copy of the head tree outside the repository.** I appended `&& trivia.Count < 0` to the whitespace-skip `while` at `BlankLineEditor.cs:96`, the same mutation as `14-auditor.md`. In the focused run of the three classes:
  - all four `…IsTakenForRequiredBlankLine` tests fail;
  - the other 25 pass (`Failed: 4, Passed: 25`).
- **The rewriters under test are the reader's only callers.** These are `BlankLineRegionDirectiveRewriter.cs:145` (`#region`) and `BlankLineTriviaBoundaryRewriter.cs:298` (`#endregion`). The rewriter-level tests therefore exercise the reader through the real `PrecedingTokenFacts` and line-start computation.

## Status of the earlier audit's items

- Finding #1 (medium, FC-017, the four pipeline tests cannot observe the whitespace skip): **closed**.
  - The member that reaches the skip for `#region` with the skip running off the list is pinned by `BlankLineRegionDirectiveRewriterTests.WhitespaceOnlyLineAboveRegionIsTakenForRequiredBlankLine`.
  - The member for `#region` with the skip ending on trivia is pinned by `…WhitespaceOnlyLineBetweenCommentAndRegionIsTakenForRequiredBlankLine`.
  - The member for `#endregion` is pinned by `BlankLineTriviaBoundaryRewriterTests.WhitespaceOnlyLineAboveEndRegionIsTakenForRequiredBlankLine`.
  - The closing-brace member is pinned at rewriter level by `…WhitespaceOnlyLineAboveEndRegionBeforeClosingBraceIsTakenForRequiredBlankLine`. In the full pipeline, an earlier rewriter of the phase decides that line.
  - All four fail under the mutation. The four pipeline tests now claim only what the final output shows, and each names the class that pins the reader. The closing-brace pipeline test names none, correctly.
- Hint 1 (the reader is `public static` with one caller in its class): unchanged. The test implementer chose rewriter-level tests over a direct test, so the accessibility is not needed for tests. Low value, as before.
- Hint 2 (lane check and the run note): unchanged. It is the lead's.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

## Guard scope
_N/A — the repair adds or changes no predicate, guard, or exemption. It changes tests only. The guard-scope rows of `14-auditor.md` hold unchanged._

## Policy ownership
_N/A — no policy changed. The ownership of SHARED-BLANK-005 is as `14-auditor.md` records: one Core decision, one formatter fact reader with one caller._

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `BlankLineRegionDirectiveRewriterTests.WhitespaceOnlyLineAboveRegionIsTakenForRequiredBlankLine` | The reader takes a whitespace-only line directly above `#region`, at the start of the leading trivia, for the required blank line | A reader that does not skip whitespace inserts a line break, so the output differs from the input (mutation: fails) | `ApplyRewriter`: the rewriter alone, exact text | adequate |
| `BlankLineRegionDirectiveRewriterTests.WhitespaceOnlyLineBetweenCommentAndRegionIsTakenForRequiredBlankLine` | Same, with the skip ending on the comment's line break inside the list | Same (mutation: fails) | `ApplyRewriter` | adequate |
| `BlankLineTriviaBoundaryRewriterTests.WhitespaceOnlyLineAboveEndRegionIsTakenForRequiredBlankLine` | Same, for `#endregion` with a member below | Same (mutation: fails) | `ApplyRewriter` (new, same shape as the sibling class) | adequate |
| `BlankLineTriviaBoundaryRewriterTests.WhitespaceOnlyLineAboveEndRegionBeforeClosingBraceIsTakenForRequiredBlankLine` | Same, for `#endregion` before the type's `}` | Same (mutation: fails) | `ApplyRewriter` | adequate |
| `BlankLineTriviaBoundaryRewriterTests.MemberLineAboveEndRegionGetsBlankLine` | The rewriter inserts the required blank line when a member stands directly above `#endregion` | A rewriter that never inserts returns the input | `ApplyRewriter`, exact expected text | adequate. On the `#region` side, a never-inserting rewriter is caught by the pipeline insertion tests in `RegionDirectiveLineAboveTests`, which the collapser cannot mask |
| `RegionDirectiveLineAboveTests.WhitespaceOnlyLine…IsKeptAsOneEmptyLine` (4, renamed) | The final output keeps one empty line above the directive and strips its whitespace | An extra or missing blank line, or remaining whitespace, in the final output (LF and CRLF) | `AssertRuleResult` | adequate as characterizations. The summaries no longer overclaim |

## Visible output is decided

- The four pipeline tests' expected values are unchanged; only their names and summaries changed. The unchanged SHARED-BLANK-001, SHARED-BLANK-005 / FMT-BLANK-013, and trailing-whitespace cleanup demand them, as `14-auditor.md` recorded.
- The rewriter-level tests assert an intermediate result inside the phase, not a visible surface.
- No rule page changed.

## Failure classes dismissed

- FC-001, FC-002, FC-015, FC-016: no guard, qualifier, or policy owner changed.
- FC-003: both exits of the whitespace skip are tested (`triviaIndex < 0` and `>= 0`). Both directive kinds share the one reader, and the `#endregion` side tests the `< 0` exit for both of its token shapes.
- FC-004, FC-005, FC-019 to FC-021: no trivia-moving code changed.
- FC-006, FC-022 to FC-029, FC-036: no rewrite changed. The renames are of test methods, which nothing reflects on.
- FC-007 to FC-009, FC-018, FC-030: no code fix changed.
- FC-010 to FC-012: no phase changed. The insert-then-collapse interplay that masked the reader is now bypassed by testing the rewriters alone.
- FC-013, FC-014: no analyzer change.
- FC-017: see *Assertion adequacy*. Every new test fails under the mutation that motivated it.
- FC-031: no production method changed. The new and changed summaries describe their tests accurately. The new class summary's claim that `BlankLineCollapser` runs after `BlankLineTriviaBoundaryRewriter` in `BlankLinePhase` matches `BlankLinePhase.cs:34–38`.
- FC-032: no compiled production change in this round.
- FC-033: the rewriter tests run only `Environment.NewLine`. The reader tests trivia kinds, not newline text, and the pipeline tests cover LF and CRLF.
- FC-034: the inputs are the shapes the reader's two loop exits take, not a cousin of them.
- FC-035: the expected values are the input itself, or the input with the one required blank line. They were checked on the head and fail under the mutation, so they are not taken from patched code.
- FC-037: no text-only claim is made.

## Architecture findings

_None._

## Hints

1. Two conventions in `BlankLineTriviaBoundaryRewriterTests.cs` differ from its sibling `BlankLineRegionDirectiveRewriterTests.cs`. Neither is enforced by the build or the formatter, and other files in the project already do the same.
   - The file declares `using System;`, which is redundant: `src/Reihitsu.Formatter.Test/Properties/GlobalUsings.cs` declares `global using System;`, and the sibling omits it.
   - The file is saved without the UTF-8 BOM that `.editorconfig` (`charset = utf-8-bom`) asks for.
2. The summary of `MemberLineAboveEndRegionGetsBlankLine` refers to "the two tests above", so its meaning depends on method order. Naming the two tests would survive a reorder.
3. `RepositoryFilesReferenceNoSquadNote` fails on this head, as the test implementer and the spec checker report. The cause is a path that my own earlier handover, `14-auditor.md`, names. The failure clears when the lead deletes `sdd/work/` before publishing, as the run plans. If any handover stays on the branch past that point, the failure would reach CI.
