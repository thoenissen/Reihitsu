Role: implementer
Commit: 30e0f4f7 (production change; this line amended in the next commit)
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the change: decision 20 of `00-lead.md`, which takes up the architecture finding in `08-implementer.md`.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | New private `FormatContext`. It is the one owner of the context entry point's context setup and pipeline run: the document column of the context's first token, the `FormattingContext` arguments (end of line, indent level, documentation boundary, code-fix structural transforms, language version, no region requirement, target annotation, target-position column), marking the target's first token (and the target node when the caller passes an annotation), and running `FormattingPipeline.Execute` on the copy of the context. | FMT-PIPE-017, FMT-PIPE-023, FMT-PIPE-024, FMT-PIPE-028 |
| same | `FormatNodeInDocumentWithContextAsync` (non-split path) calls it. It passes the target's document column and no node annotation. | FMT-PIPE-017, FMT-PIPE-018 |
| same | `FormatSplitFieldInContext` calls it. It passes its target-position column, which is `null` when a member stands in front, and the field annotation. The line break it writes in a shared-line gap uses `DetectEndOfLine(root)` directly, because the end of line is no longer a local of the path. | FMT-PIPE-029 |

Both paths compute the same arguments they computed before, so no output changes. `scripts/format.sh` changed nothing. No test file was touched.

## Policy owners

- Context setup and pipeline run for the context entry point: `rg -n "new FormattingContext\(" src/Reihitsu.Formatter/ReihitsuFormatter.cs` now finds three: `FormatContext` (both context paths), `FormatNodeInDocumentAsync` (its own facts: preceding token, list position, chain boundary, root construct), and `OrganizeUsingDirectivesInDocumentAsync` (using phase only). The latter two take different facts and are not copies of the context path.

## Tests run

```shell
scripts/build.sh --configuration Debug
# 0 Warning(s), 0 Error(s) (a first run failed on MSB3030 copy errors from a concurrent build of the test-repair agent; the rerun was clean)
scripts/test.sh --configuration Debug --project formatter
# Passed!  - Failed: 0, Passed: 4481, Total: 4481
scripts/test.sh --configuration Debug --project analyzer
# Passed!  - Failed: 0, Passed: 6493, Total: 6493
scripts/test.sh --configuration Debug --project architecture
# Passed!  - Failed: 0, Passed: 100, Total: 100
```

The three tests of decision 18 passed in this run as well; I did not check whether that comes from the parallel repair.

## Disputed tests or scope

_None._

## Architecture findings

_None._ The finding of `08-implementer.md` is resolved: the context entry point now has one owner of its context setup.

## Follow-up candidates

_None._
