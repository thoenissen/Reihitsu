Role: implementer
Commit: d0276cb
Verdict: tests pass

Repair round 2. The scope is audit finding #8 (`18-auditor.md`), under decision `2026-10-31-phase-tree-round-trip-raw-string-hole-line.md`: the move keeps editing tokens and does not reparse. My prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs` | `CollectEdits` now checks for a content line that starts with a hole's opening brace (new `IsHoleOpeningLine`). The move writes that line's indentation as text of the literal, which is how the parser reads it. When the token in front of the brace is an `InterpolatedStringTextToken`, which always ends at the line start, the edit goes to that token: `ApplyEdits` appends the spaces to the token's text, and the token keeps its value. When the start token stands in front of the brace (the first content line), the expression is recorded instead. `MoveLines` then uses `ReplaceSyntax` in place of `ReplaceTokens`, and `InsertLeadingText` inserts a new `InterpolatedStringText` element with value `""` in front of the hole. Every other node and token is kept, so annotations survive. The summaries of `MoveLines` and `CollectEdits` now describe this. | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 to FMT-RAW-003 |

The original indentation of a hole-opening line is always 0, because any indentation would be text and `FindToken` would then return the text token. The edit therefore only ever adds spaces. A move to the left cannot empty an inserted or extended text token on these lines: the case does not arise, as `19-test-implementer.md` explains under *Not covered*.

## Policy owners
- `rg -n "InterpolatedStringTextToken|InterpolatedStringText\(" src --glob '!*Test*'`: outside the analyzers, the only owner is `RawStringLineShifts`. The analyzers do not move raw-string lines.
- The decision "which token holds a moved line's indentation" stays in `CollectEdits`/`ApplyEdits` only.

## Tests run
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Pipeline.RawStringMovedLineStartTests"`:
  - before the change: `Failed: 13, Passed: 16, Total: 29`, as the handover says;
  - after the change: `Passed: 29`.
- `scripts/test.sh --configuration Debug` (every project except selfhosting):

  | Project | Passed |
  |---|---|
  | Analyzer | 6312 |
  | Formatter | 3402 |
  | Core | 535 |
  | Cli | 251 |
  | ArchitectureTests | 89 |
  | Tooling | 125 |
  | Playground | 17 |

  No project has a failure.
- `scripts/format.sh` on the changed file reformatted 0 files.
- `scripts/lane-check.sh behavior-change` confirms the lane.

## Disputed tests or scope
_None._ No test assertion was changed. No public API, dependency, or shared helper was added.

## Architecture findings
_None._

## Follow-up candidates
- The pre-existing build warning S1125 at `src/Reihitsu.Cli/FormatCommandHandler.cs:506` is unrelated to this change and belongs to the cleaner.
