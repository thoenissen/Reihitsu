Role: implementer
Commit: 8cb740c
Verdict: tests pass

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It asked me to re-derive whether one implementation satisfies both requirements at once: FMT-PIPE-017 (columns from the indentation phase) and SHARED-FIX-006, bullet "Blank lines" (a fix MUST NOT insert a blank line before the node it changes, or change the blank lines between it and its neighbours). It does. Every test passes, including the two RH5111 tests `30-implementer.md` disputed. I withdraw that dispute: `30-implementer.md` considered only restoring all of the original trivia or none of it, and missed the middle way below.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs`, `FormatNodeInDocumentWithContextAsync` | For a target that starts its line (FMT-PIPE-010 does not keep its leading trivia), the entry point writes back the formatted leading trivia with the original blank lines restored (`RestoreBlankLines`). Where FMT-PIPE-010 keeps the trivia, it writes back the original trivia, as in 8316e8a. | FMT-PIPE-017, SHARED-FIX-006, FMT-PIPE-010 |
| `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs` | New internal `RestoreBlankLines(formatted, original, endOfLine)` with private helpers `SplitIntoGaps`, `CountLineBreaks`, `EndsWithLineBreak`, and `RestoreGapLineBreaks` (details below). | FMT-PIPE-017, SHARED-FIX-006 |

`FormatNodeInDocumentAsync` is unchanged. For a target that starts its line, it decides blank lines from the captured surroundings (`RootListPosition`, `RootPrecedingToken`), as its remarks and FMT-PIPE-010 state. Its tests pass, and no handed-over test covers it here.

## How the two requirements are met together

FMT-PIPE-017 decides columns. SHARED-FIX-006 decides blank lines. In the target's leading trivia these are separate parts:
- Columns are the whitespace behind the last line break of each gap (the indentation of the next comment, directive, or token) and the comment and directive trivia themselves.
- Blank lines are the number of line breaks in each gap.

So `RestoreBlankLines` keeps the formatted trivia and replaces only the line-break counts:
1. It splits both trivia lists into items (anything other than whitespace and line breaks: comments, directives, documentation comments) and the gaps around them. That gives one gap in front of each item and one behind the last.
2. If the item kinds differ in count or order, nothing is restored, and the formatted trivia is returned as it is. The phases keep comments and directives, so this is a safety net that no test reaches.
3. For each gap, the number of line breaks is taken from the original gap. The gap's trivia in front of its first line break and its whitespace behind its last line break are taken from the formatted gap. That whitespace is the column the phases gave the next line.
4. A gap that does not start its line (behind a `//` or `/* */` comment) has its count replaced only when both gaps hold at least one line break. A line break that one side adds or removes is a layout decision, not a blank line, and the phases keep it.
5. A gap that starts its line (the first gap, because the target starts its line, or a gap behind a directive or a `///` comment, which carry their own line break) takes the original count in every case, because every line break in it is a blank line.

The result:
- Every line written back ends in the column the indentation phase gives it (FMT-PIPE-017). This covers the comment lines of the trivia too, because the comments keep the formatted indentation in front of them. The only blank lines anywhere are the original ones.
- The blank lines between the target and its neighbours are the document's (SHARED-FIX-006).

Applied to the disputed members:
- **Enum member:** the formatted trivia `EOL, "    ", // second, EOL, "    "` becomes `"    ", // second, EOL, "    "`. The first gap goes back from 1 line break to 0, so there is no blank line, and every column stays the phase's.
- **Parameter:** the same, with the comment and `int second` at 16.
- **Finding #8 members:**
  - RH5111 declarator: `b` at 8 and `2` at 18.
  - Public-API argument: `Other` at 13 and `3` at 19.
  - Parameter at 11, enum member at 4.
  - Their leading trivia holds no blank-line difference, so they keep the phase's output unchanged.

## Policy owners

- The node-level target's leading trivia is restored in two places:
  - `FormatNodeInDocumentAsync`: original trivia under FMT-PIPE-010's condition.
  - `FormatNodeInDocumentWithContextAsync`: original trivia under the same condition, otherwise formatted trivia with the original blank lines.

  Both use the same FMT-PIPE-010 condition: `targetNode.Parent == root || StartsOnNewLineIncludingDocumentation(firstToken) == false`. `rg -n "WithLeadingTrivia\(originalFirstToken|RestoreBlankLines" src/Reihitsu.Formatter` finds nothing else.
- Blank lines are counted in many places: `rg -n "EndOfLineTrivia" src/Reihitsu.Formatter/Pipeline/BlankLines` finds the blank-line phase's own counting (`BlankLineEditor`, `TokenGapAnalysis`), which decides blank lines during the pipeline. `RestoreBlankLines` makes no blank-line decision; it copies the document's count back after the pipeline. It moves no column, so FMT-PIPE-015 and FMT-PIPE-017 still hold.

## Tests run

All runs were on the tree of 8cb740c.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5111"` → `Passed: 53, Failed: 0`. That includes the two former disputes and the three #8 tests.
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ReihitsuFormatterNodeColumnTests|FullyQualifiedName~IndentationPhaseRawStringMoveTests"` → `Passed: 26`.

| Project | Result |
|---|---|
| formatter | 2984 passed |
| analyzer | 6229 passed |
| architecture | 89 passed |
| core | 533 passed |
| cli | 250 passed |
| tooling | 125 passed |
| playground | 17 passed |
| selfhosting | 3607 passed |

`scripts/format.sh --force` reformatted one file, the helper's collection initializer. The logs show no RH, CS, S, or IDE warning for the changed files.

## Disputed tests or scope

_None._ The dispute in `30-implementer.md` is withdrawn: SHARED-FIX-006 decides the blank lines, and the implementation above meets it together with FMT-PIPE-017.

## Architecture findings

_None new._ `RestoreBlankLines` runs after the last phase, but it writes back only the document's own line-break counts. It neither predicts nor moves a column. It does not apply the formatter's blank-line policy; it applies the fix's obligation to keep the document's blank lines.

## Follow-up candidates

- No test covers a context target that starts its line behind a blank line the phases would remove, or behind a directive or a `///` comment. Gaps that start their line take the original count, and `RestoreGapLineBreaks` covers them in code only.
- Still open:
  - `ShiftContinuationExteriorMarkers` removal (audit 2, #10);
  - RH5201 against change doc row 10;
  - the RH5304 statement and expression-body targets;
  - `FormatNode` on an attached node.
