Role: test implementer
Commit: cb67b2d
Verdict: tests written — no new test needed; both places are pinned by existing tests, named below

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's decisions.

## Tests

No test added or changed. The existing tests that pin the behavior the refactoring must keep:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.Formatter.Test.Unit.Pipeline.RawStringMovedLineStartTests.HoleLineStartingWithTheHolesClosingBraceAtColumnZeroMovesRight` | FC-015 (decision 1: open-brace-only test in `IsHoleOpeningLine`) | `AssertRuleResult`-style formatter test | pass (green on cb67b2d) |
| `RawStringMovedLineStartTests.FirstLineStartingWithAHoleAtColumnZeroMovesRight`, `LaterLineStartingWithAHoleBehindTextAtColumnZeroMovesRight`, the `DoubleDollar...` cases | FC-015 (decision 1: opening brace starting a line is detected) | formatter test | pass |
| `Reihitsu.Formatter.Test.Regression.Spacing.HorizontalSpacingRequirementTests.OneSpaceBeforeBracesAndAroundDeclarationColons` | FMT-SPACE-009 (existing tag); decision 2, both sides | `FormatterTestsBase.AssertRuleResult` (LF and CRLF) | pass |
| `Reihitsu.Formatter.Test.Unit.HorizontalSpacing.HorizontalSpacingRewriterTests.HandlesStringInterpolation` | decision 2, interpolation side (untagged) | `AssertHorizontalSpacing` | pass |

### Proof the tests assert the relevant values

Decision 2 (`SingleSpaceSpacingRule.RequiresSpaceBeforeBrace`), checked by mutation in a throwaway worktree (deleted; the working tree is unchanged), full formatter test project each time:

- Interpolation braces lose the exemption (`return token.Parent is not InterpolationSyntax;` → `return true;`): 390 failures, among them
  - `HandlesStringInterpolation`: expected `$"Hello {name}!"`, actual `$"Hello  {name }!"`;
  - `OneSpaceBeforeBracesAndAroundDeclarationColons` (its input holds `return $"{a}";`).
- Ordinary braces lose the single space (→ `return false;`): 42 failures, among them
  - `OneSpaceBeforeBracesAndAroundDeclarationColons`: expected `new int[] { a };`, actual `new int[]{ a };`;
  - `TightAccessorListsGetOneSpaceBehindTheBraceAndBetweenAccessors`, `InitializerBracesAreNotAnAccessorList`, `RequirementExampleIsSpacedByTheSpacingPhase`.

So both sides of the condition the refactoring rewrites are pinned, the interpolation side and the ordinary-brace side by one FMT-SPACE-009 test under LF and CRLF. For a brace token, `token.Parent is InterpolationSyntax` and `FormattingTextAnalysisUtilities.IsInterpolationBrace(token)` must agree; these tests fail if they do not.

Decision 1 (`RawStringLineShifts.IsHoleOpeningLine`): the decided change is a doc comment only, so `scripts/verify-text-only.sh` is the proof for that file. The behavior it describes is pinned: the skeptic's substitution of `IsInterpolationBrace` for `interpolation.OpenBraceToken == token` failed `HoleLineStartingWithTheHolesClosingBraceAtColumnZeroMovesRight` (01-skeptic.md, *Premise*); I did not repeat that mutation. The opening-brace cases above pin the other side. All of them run green on cb67b2d.

## Scenarios (bug fix)

_N/A_ — no defect is reported at either place; the lane carries a refactoring with no intended output change.

## Not covered, and why

- No new test: *Coverage first* — every value the refactoring could change is pinned by the tests above, shown by mutation.
- Idempotency, CRLF: `OneSpaceBeforeBracesAndAroundDeclarationColons` runs LF and CRLF through `AssertRuleResult`; the raw-string site changes no compiled code.
- Comment or directive in the gap, code-fix paths, Fix All, naming, unsupported shapes: not reachable — no output, gap, fix, or shape changes; the spacing predicate is equivalent for brace tokens.

## Commands

    scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Pipeline.RawStringMovedLineStartTests|FullyQualifiedName~RawStringTabIndentationTests|FullyQualifiedName~HorizontalSpacingRewriterTests|FullyQualifiedName~InterpolationAlignmentSpacingReproTests|FullyQualifiedName~HorizontalSpacingRequirementTests"

Baseline on cb67b2d: the first four classes 94 passed, `HorizontalSpacingRequirementTests` 49 passed, 0 failed.

## Follow-up candidates

_None._
