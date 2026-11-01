Role: implementer
Commit: 8ec2bf3 (base); this handover's commit carries the change
Verdict: tests pass

Scope: lead decision 3 in `00-lead.md`. One owner in `Reihitsu.Core` measures whether a blank line stands above a using directive, or above its leading comment. RH7207 and the using phase both call it. I read `00-lead.md`, `04-test-implementer.md`, `05-implementer.md`, SHARED-USING-008/011, FMT-USING-010, and RH7207. The prompt named no fix, cause, or hoped-for verdict beyond the decision itself. No test or test helper changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/UsingDirectiveOrderingUtilities.cs` | New `HasBlankLineAbove(UsingDirectiveSyntax predecessor, UsingDirectiveSyntax usingDirective)`. It counts the line breaks in front of the directive, or in front of its first significant trivia. Those are one for the predecessor's trailing trivia when that trivia holds a line break, plus every line break in the layout at the start of the directive's own leading trivia. Two or more mean a blank line. This is the only statement of the measurement. | SHARED-USING-008, SHARED-USING-011 |
| `src/Reihitsu.Analyzer/Rules/Organization/RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.cs` | The private `HasBlankLineBefore` (which looked at the leading layout only) is removed. `IsOrganized` calls `HasBlankLineAbove(usings[index - 1], usings[index])`. | RH7207-001, RH7207-002 |
| `src/Reihitsu.Formatter/Pipeline/UsingDirectives/Utilities/UsingLeadingTriviaBuilder.cs` | `HasBlankLineAboveLeadingComment` is removed. | FMT-USING-010 |
| `src/Reihitsu.Formatter/Pipeline/UsingDirectives/Rewriter/UsingDirectiveOrderingRewriter.cs` | The source scan now tests `IsLedByComment(current) && HasBlankLineAbove(predecessor, current)`. Both predicates are in Core. | FMT-USING-010 |

The two components now differ only in how they combine the shared predicates with group membership. They no longer differ in how they measure.

### Why one owner is possible without changing a pinned output

The two old measurements differed in two places:

1. **The predecessor's line end.** RH7207 counted it implicitly: any line break in the leading layout meant a blank line. The phase counted it explicitly. On parsed source a line's end always lies in the trailing trivia of the line's last token, so a predecessor that ends its line always contributes exactly one. Both forms then give the same answer for every input RH7207 sees. If the directive shares the predecessor's line, both give `false`. The explicit form is needed for the pipeline-only shape: the documentation-comment phase leaves a predecessor whose line does not end, and the line break sits in the successor's leading trivia (`05-implementer.md`).
2. **A directive with no significant leading trivia.** The phase's `Take(-1)` counted nothing, but only behind `IsLedByComment`, so that case never reached it. The shared method counts the whole leading list there, as RH7207 did.

All suites stay green with their assertions unchanged, so the shared method needs no pinned output to change.

## Policy owners

- `rg -n "HasBlankLineAbove|HasBlankLineBefore|HasBlankLineAboveLeadingComment" src --glob '!*Test*'`
  - **Blank line above a using directive or its leading comment:** `UsingDirectiveOrderingUtilities.HasBlankLineAbove` only. RH7207 and `UsingDirectiveOrderingRewriter` call it.
  - **Unrelated:** `BlankLineEditor.HasBlankLineBeforeIndex` and `HasBlankLineBeforeToken` are the blank-line phase's general token-gap measurement. They do not decide using-group separation.
- **A comment leads the directive:** `UsingDirectiveOrderingUtilities.IsLedByComment` only. This is unchanged.
- **The header's blank line:** the detached original first directive still counts that blank line through `SplitOriginalFirstHeaderTrivia`, because the header ends with a blank line by construction. That is a different decision, so I left it as is.

## Tests run

- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors.
- `scripts/format.sh` on the four changed files: 0 of 4 reformatted.
- `scripts/test.sh --configuration Debug --project <p>` (full project runs):
  - formatter: 3744/3744
  - analyzer: 6383/6383
  - core: 544/544
  - cli: 251/251
  - architecture: 92/92
  - tooling: 125/125

## Disputed tests or scope

`UsingDirectiveOrderingUtilities.HasBlankLineAbove` is a new public member in `Reihitsu.Core`, shared by the analyzer and the formatter. The lead's decision asked for it. It has no direct Core unit test; RH7207 and the using-phase tests cover it through both callers. If the lead wants a focused Core test, that is for the test implementer.

## Architecture findings

None. Decision 3 is carried out.

## Follow-up candidates

None.
