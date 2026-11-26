Role: skeptic
Commit: 4d4b84f
Verdict: proceed narrower

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the issue itself.

## Premise

The issue claims that `RawStringLineShifts.IsHoleOpeningLine` (`src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs:272`) states the same policy a second time as `FormattingTextAnalysisUtilities.IsInterpolationBrace` (`src/Reihitsu.Core/FormattingTextAnalysisUtilities.cs:345`). It does not. The two checks answer different questions:

- `IsInterpolationBrace` asks whether a token is the opening **or** closing brace of an interpolation. RH5025, RH6009 and RH6010 use it to exempt those braces from brace layout and spacing rules.
- `IsHoleOpeningLine` asks whether the parser reads the indentation in front of a line's first token as **text of the literal**. That is true for an opening brace that starts a content line. It is false for a closing brace that starts a line: the whitespace in front of `}` is leading trivia inside the hole, and `ApplyLeadingTriviaEdit` moves it. The method also has to return the `InterpolationSyntax` through its `out` parameter.

The open-brace-only restriction matters to the output, and an existing test pins it. I checked this in a throwaway worktree (since deleted). I replaced `interpolation.OpenBraceToken == token` with `FormattingTextAnalysisUtilities.IsInterpolationBrace(token)` and left the line-start check in place. That is the reading the issue proposes: the Core helper for the brace test, the line-start condition kept local. Then I ran:

    scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Pipeline.RawStringMovedLineStartTests|FullyQualifiedName~Reihitsu.Formatter.Test.Unit.RawStringAlignment.RawStringInterpolationHoleTests"

Result: 73 passed, 1 failed, `HoleLineStartingWithTheHolesClosingBraceAtColumnZeroMovesRight` (`tests/Reihitsu.Formatter.Test/Unit/Pipeline/RawStringMovedLineStartTests.cs:240`, input lines `a {x` / `} b` in column 0):

    expected: "...var s = $\"\"\"\n                 a {x\n                 } b\n                 \"\"\";..."
    actual:   "...var s = $\"\"\"\n                                  a {x\n} b\n                 \"\"\";..."

With the substitution, the move writes 17 extra spaces of literal text in front of `a`, which changes the string's value (a safety-rail defect), and leaves `}` in column 0. Any correct use of the Core helper therefore still needs `interpolation.OpenBraceToken == token`, and that test alone implies the Core check. Calling the helper would add a redundant call and remove no statement of anything.

Reachability: whole-document formatting and the CLI reach this method for every multi-line interpolated raw string whose lines move. A top-level-statement file shows it (fixture below; `scripts/format.sh` run twice, second pass `Formatted 0 of 1`, stable). Today's output is correct:

```csharp
// input (Program.cs)
var name = "x";
var a = $"""
{
name
} tail
""";
var b = $"""
Hello {
name
}
""";
```

```csharp
// output on main, pass 1 and pass 2
var name = "x";
var a = $"""
         {
         name
         } tail
         """;
var b = $"""
         Hello {
         name
         }
         """;
```

So the reported defect, a second owner of one policy (FC-015), does not exist at this site. The issue's "Done when" already allows the other outcome: "or a comment there states why it does not".

## Mechanism and candidates

_N/A as a bug._ No output is wrong. The question is whether a policy has two owners. I took the candidates from every site under `src/` that tests a token against `InterpolationSyntax` (`rg "InterpolationSyntax" src --type cs`):

| Site | Question it asks | Same policy as `IsInterpolationBrace`? |
|---|---|---|
| `FormattingTextAnalysisUtilities.IsInterpolationBrace` (Core) | is the token either brace of an interpolation | owner |
| RH5025 / RH6009 / RH6010 analyzers | (call the owner) | uses owner |
| `RawStringLineShifts.IsHoleOpeningLine` | does a content line start with a hole's **opening** brace, so its indentation is literal text | no: the open-only restriction matters, as shown above |
| `SingleSpaceSpacingRule.RequiresSpaceBeforeBrace` (`src/Reihitsu.Formatter/Pipeline/HorizontalSpacing/Rules/SingleSpaceSpacingRule.cs:33-41`) | is a `{`/`}` token **not** an interpolation brace, so the brace spacing rule applies | **yes**. This is a private second statement, and it is the formatter counterpart of RH6009/RH6010's exemption |
| `CommentPositionUtilities.IsOwningConstruct`, `ListElementInteriorUtilities.IsOwningConstruct`, `LayoutComputer`, `LayoutModel`, `StringInterpolationUtilities`, `ControlFlowBraceTransform`, `SingleLineInterpolationUtilities`, RH5202 fix `StartsInOwnHole` | node-level questions about holes, not about a brace token | no |

`SingleSpaceSpacingRule.RequiresSpaceBeforeBrace` returns `token.Parent is not InterpolationSyntax` after it has checked that the token is an `OpenBraceToken` or `CloseBraceToken`. `InterpolationSyntax`'s only token members are `OpenBraceToken` and `CloseBraceToken`; the alignment comma and the format colon belong to the clause nodes. So for a brace token, this is `IsInterpolationBrace(token) == false` stated a second time. That is the FC-015 instance this policy actually has, and FC-016 (another private copy) is the class the issue missed. Whether it joins the run is the lead's scope decision (see *Internal choices*).

## Breaking inputs

FC-015 / FC-016 are the classes that apply. No input breaks today. The only input class that tells the two readings apart is a moved content line of a multi-line interpolated raw string that starts with a hole's **closing** brace in column 0. Coverage:

- A moved line starting with a hole's closing brace in column 0 is pinned by `RawStringMovedLineStartTests.HoleLineStartingWithTheHolesClosingBraceAtColumnZeroMovesRight` (shown above to fail under the issue's proposed substitution).
- A moved line starting with a hole's opening brace (first content line and later lines, `$` and `$$`) is pinned by the other `RawStringMovedLineStartTests` cases (`FirstLineStartingWithAHoleAtColumnZeroMovesRight`, `LaterLineStartingWithAHoleBehindTextAtColumnZeroMovesRight`, `DoubleDollar...`, and others) and by `RawStringTabIndentationTests.InterpolatedLiteralWritesTheStrippedTabsInFrontOfHolesAsSpaces`.
- Spacing around interpolation braces, for the `SingleSpaceSpacingRule` site if the lead takes it in, is covered by `HorizontalSpacingRewriterTests.HandlesStringInterpolation` and `InterpolationAlignmentSpacingReproTests`. The test implementer confirms these assert the braces' spacing before relying on them.

Following *Coverage first*, no new test is needed for #1722.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `RawStringLineShifts.IsHoleOpeningLine` (formatter, indentation phase) | must stay in behavior. At most its doc comment changes (text-only) |
| `FormattingTextAnalysisUtilities.IsInterpolationBrace` (Core) | must stay |
| `SingleSpaceSpacingRule.RequiresSpaceBeforeBrace` (formatter, horizontal spacing) | not involved in #1722 as filed. It is the candidate adjacent fix (see *Internal choices*) |
| RH5025 / RH6009 / RH6010 analyzers | must stay |
| Code fixes, Fix All, CLI, rule docs, specs | not involved |
| Tests | must stay unchanged. No new test |

## Refuse option

The refuse option is the outcome itself: leave `IsHoleOpeningLine` as it is. It costs no production lines and changes no output. The only cheaper option is to change nothing at all. The issue's "Done when" asks for a comment instead, and that is the recommended narrower result. The comment should state, in `IsHoleOpeningLine`'s summary, why the test is open-brace-only and is not `IsInterpolationBrace`: the indentation in front of a hole's closing brace is the hole's leading trivia, which the trivia edit moves, and it is not literal text. That gives a reviewer the answer, and an audit is less likely to file the same issue again. Check the change with `scripts/verify-text-only.sh`.

Declaring a shape unsupported is not an option here. No shape costs a special case or a refusal.

## Output questions

_None._ Neither the comment-only outcome nor the `SingleSpaceSpacingRule` consolidation changes visible output.

## Internal choices

1. **#1722's outcome.**
   - **A:** doc-comment sentence on `IsHoleOpeningLine` only, text-only, no production change, `Closes #1722` through the "Done when" branch "or a comment there states why it does not".
   - **B:** no change at all, `Refs #1722`.

   Recommend **A**. It meets the issue's own stated alternative, and it records the reason where the next audit looks. The lane can stay bug-fix, or narrow to a text-only lane, as the lead prefers. Either way the diff is one doc comment in one `.cs` file.
2. **`SingleSpaceSpacingRule.RequiresSpaceBeforeBrace` as an adjacent fix, or as a follow-up.** It is the genuine second statement of the policy the issue names: the same question, a one-line change to the Core call, and no output change, because the two are equivalent for brace tokens (see *Mechanism and candidates*). As an adjacent fix, the run delivers what #1722 was really after (FC-015 for interpolation braces) in about one production line, and the existing spacing tests guard it. As a follow-up, it costs a separate run for one line. Recommend taking it in as an adjacent fix, recorded under *Decisions taken*, with `Closes #1722` and the PR stating that the consolidation landed at the spacing rule and not at `RawStringLineShifts`.

## Issue goals

- Goal "RawStringLineShifts uses the Core check for the brace test" (title and first branch of "Done when"): **drop**. The Core check answers a different question (open or close), and the open-only restriction is pinned by `HoleLineStartingWithTheHolesClosingBraceAtColumnZeroMovesRight`. A literal swap changes a raw string's value. A call kept alongside the open-brace test is redundant. Dropping the goal changes no visible output.
- "Either way the outcome is one statement per policy": **keep, and redirect**. The second statement of this policy is in `SingleSpaceSpacingRule`, not in `RawStringLineShifts` (internal choice 2). No visible output change.
- "The formatter's tests pass unchanged": **keep**.

## Architecture findings

_None for #1722_: the suspected two-owner decision is two different decisions. The `SingleSpaceSpacingRule` copy is an FC-015 instance of the same policy. It is reported under *Internal choices* as an adjacent-fix candidate, not as a structural defect the run depends on.

## Follow-up candidates

- **`IsOwningConstruct` has two copies**: `src/Reihitsu.Analyzer/Core/CommentPositionUtilities.cs:341` (internal) and `src/Reihitsu.Formatter/Pipeline/Core/Utilities/ListElementInteriorUtilities.cs:95` (private). Their bodies are identical, and the analyzer's summary says it "must stay in step with" the formatter's. That is FC-015: one decision, "which node lays out the line of a token inside it", stated in two assemblies. The analyzer does not reference the formatter, so the shared owner would be `Reihitsu.Core`. This is unrelated to this run. No existing open issue covers it (checked the open `maintenance` issues; #1937 is about a different set of copies).
- If the lead does not take in internal choice 2: `SingleSpaceSpacingRule.RequiresSpaceBeforeBrace` should use `FormattingTextAnalysisUtilities.IsInterpolationBrace` (FC-015, no output change).
