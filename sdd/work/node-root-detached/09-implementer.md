Role: implementer
Commit: 5e44c16
Verdict: tests pass

Repair round for decision 8 of `00-lead.md`. The coordinator's message carried the task and no intended fix, suspected cause, or hoped-for verdict; nothing was ignored. I re-read `squad/roles/implementer.md` and `AGENTS.md`, *Supported inputs*; neither changes this task.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs` | Removed `ComputeBaseIndentLevel`, the wrapper around Core's method, which had no caller left after f4ae2a4 deleted its tests. The class summary already lists only the remaining computations. | FMT-PIPE-012 (no second depth rule in the formatter) |

## Policy owners

- `rg -n "ComputeBaseIndentLevel" src --type cs`. Remaining hits:
  - the definition and comments in `src/Reihitsu.Core/SyntaxIndentationUtilities.cs`;
  - the caller `TargetAttributePlacementCodeFixProviderBase` and its doc comment;
  - a doc comment in `RH5513MethodAttributesMustFollowPlacementRulesAnalyzerTests`.

  The formatter no longer references it, and Core's method stays for its code-fix caller.

## Tests run

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~VerifyFixKeepsTheTrailingCommentOfAnElseIfCondition|FullyQualifiedName~VerifyEmbeddedIfBehindMultiLineCommentIsFixedInOnePass"
```

- Focused run: Passed 4/4. These are the new RH5405 and RH5204 code-fix tests, LF and CRLF.
- Whole projects, Debug:

  | Project | Passed |
  |---|---|
  | formatter | 3426/3426 |
  | analyzer | 6316/6316 |
  | core | 535/535 |
  | architecture | 92/92 |

- Debug build: 0 warnings, 0 errors.
- `scripts/format.sh` on the changed file: no change.

## Disputed tests or scope

_None._

## Architecture findings

_None._

## Follow-up candidates

_None._
