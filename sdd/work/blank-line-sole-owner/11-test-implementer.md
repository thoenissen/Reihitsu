Role: test implementer
Commit: see the commit that adds this file (subject "Test implementer: pin FMT-BLANK-009 before ] and ) [skip ci]")
Verdict: tests written

Repair round 1, after the maintainer's pre-auditor answer (A) recorded in `00-lead.md` (*Maintainer answers*, decision row 16) and `10-spec-author.md`. The prompt named the change asked for (pin the answered case, blank-line state only); it carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from FMT-BLANK-009's text: no blank line inserted in front of the first non-region directive or disabled text of a gap a region directive ends, before a collection expression's or list pattern's `]` and a parenthesized pattern's `)`; the author's blank line in front of it is kept. Every test uses `AssertBlankLinesAbove`, so no region-directive column is pinned (the open follow-up candidate in `00-lead.md`); each asserts one line's blank-line state, then second-pass and phase stability, under LF and CRLF.

All six new tests pass on 79b44c5 (formatter 3632/3632, architecture 92/92). To see them fail for the right reason, I ran the branch's `Reihitsu.Formatter.Test` (minus `TokenGapNormalizerTests.cs`, which no longer compiles there) against `main`'s production code at 741157c in a scratch worktree, since removed. "On main" below gives that result.

## Tests

All in `src/Reihitsu.Formatter.Test/Regression/BlankLines/DirectiveBeforeEndRegionBlankLineTests.cs` (class summary widened to name `]` and `)`).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `InsertsNoBlankLineInFrontOfDirectiveWhenRegionDirectiveEndsTheGapBeforeClosingBracket` (collection expression `[ #region R, 1, #pragma …, #endregion ]`; no blank above `#pragma`) | FMT-BLANK-009 | `AssertBlankLinesAbove` | passes; on main: fails, "Unexpected blank line state above '#pragma'" (blank line inserted) |
| `KeepsAuthorBlankLineInFrontOfDirectiveWhenRegionDirectiveEndsTheGapBeforeClosingBracket` (author's blank line above `#pragma` kept) | FMT-BLANK-009 | `AssertBlankLinesAbove` | passes; passes on main (holds) |
| `InsertsNoBlankLineInFrontOfConditionalBlockWhenRegionDirectiveEndsTheGapBeforeClosingBracket` (`#if NEVER … #endif` in front of `#endregion`; no blank above `#if`) | FMT-BLANK-009 | `AssertBlankLinesAbove` | passes; on main: fails, blank line inserted above `#if` |
| `InsertsNoBlankLineInFrontOfDirectiveWhenRegionDirectiveEndsTheGapBeforeClosingBracketOfListPattern` (`o is [ … ]`) | FMT-BLANK-009 | `AssertBlankLinesAbove` | passes; on main: fails, blank line inserted above `#pragma` |
| `InsertsNoBlankLineInFrontOfDirectiveWhenRegionDirectiveEndsTheGapBeforeClosingParenthesisOfPattern` (`o is ( … )`) | FMT-BLANK-009 | `AssertBlankLinesAbove` | passes; on main: fails, blank line inserted above `#pragma` |
| `KeepsAuthorBlankLineInFrontOfDirectiveWhenRegionDirectiveEndsTheGapBeforeClosingParenthesisOfPattern` (author's blank line above `#pragma` kept) | FMT-BLANK-009 | `AssertBlankLinesAbove` | passes; passes on main (holds) |

Reachability: whole-document formatting of a file containing these shapes (and the CLI over it) reaches each case; the parenthesized and list patterns are the shapes `10-spec-author.md` formatted with `scripts/format.sh`.

## Not covered, and why

- Region-directive and element columns in these fixtures: no requirement decides them (follow-up candidate in `00-lead.md`); `AssertBlankLinesAbove` asserts blank lines only, by design.
- The blank lines FMT-BLANK-013/019 decide around `#region`, `#endregion`, and `]`/`)` are not asserted here; existing tests (`ClosingBracketBlankLineTests`) cover FMT-BLANK-019, and the request was the FMT-BLANK-009 state only.
- CRLF, idempotency, phase stability: built into `AssertBlankLinesAbove`.
- Comment in the gap: FMT-BLANK-020 decides that side; existing `GluedCommentBlankLineTests` cover it. Not part of this answer.
- Analyzer, code fix: no analyzer or code-fix behavior is in this answer.

## Commands

```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~DirectiveBeforeEndRegionBlankLineTests"
scripts/test.sh --configuration Debug --project formatter
scripts/test.sh --configuration Debug --project architecture
```
