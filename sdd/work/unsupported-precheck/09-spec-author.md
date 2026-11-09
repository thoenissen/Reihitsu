Role: spec author
Commit: 93447a34
Verdict: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md` (*Decisions taken* rows 11, 12, 15), `06-implementer.md`, and the code at `f2b7638d`.

## Changed requirements

No requirement changed. Header only:

- `sdd/specs/formatter/unsupported-syntax.md` — *Code owners* `—` → `PrecheckPhase`, `FormattingResult<T>`, `UnsupportedSyntax`, `SyntaxNotSupportedException` (row 15; the handover 01 internal choice).
- `sdd/specs/README.md` — the FMT-UNSUP index row carries the same owners.

`ReihitsuFormatter` stays an owner of `formatter/pipeline.md` only; `FindUnsupportedSyntax` is one of its members, and FMT-UNSUP-007 names it by member.

Rows 11 and 12: no spec states the opposite, so no spec text changed. `grep` over `sdd/specs` and `sdd/changes` finds no mention of `ISourceFormatter`, of the CLI asking a separate query, of `FormatSyntaxTree` returning a bare tree in the CLI, or of `SyntaxNotSupportedException` being internal. CLI-019/CLI-020 describe only the reported lines, which row 11 keeps; FMT-UNSUP-006 says the refusal never reaches a caller as an exception, which holds regardless of the type's accessibility.

## Checks

- `scripts/test.sh --project architecture --configuration Debug --filter "FullyQualifiedName~Sdd"`: Passed 51, Failed 0 (includes `SpecConsistencyTests`, which resolves the owner names).
- The same run in Release does not build: `SyntaxNotSupportedException.cs(8,23): error S3871`. That is row 12, still pending with the implementer; it is not caused by this change.

## Examples for the tests

_None._ No requirement changed.

## Output questions

_None._

## Internal choices

- Owners list the four new types only, not `ReihitsuFormatter` (already the owner of `formatter/pipeline.md`). Alternative: add it to both specs.

## Follow-up candidates

_None._
