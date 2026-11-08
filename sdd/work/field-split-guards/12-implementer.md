Role: implementer
Commit: (this commit)
Verdict: tests pass

Repair round 1. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Input: auditor finding #1 (`09-auditor.md`) as maintainer answer B and the amended FMT-STRUCT-030, FMT-PIPE-031, RH7101-007 (`10-spec-author.md`) resolve it; tests from `11-test-implementer.md`.

## thoenissen/Reihitsu#963

Answer B keeps the guard's behavior, and the amended specs put a `///`/`/** */` comment behind the type on the type's line (Parse/Diagnose) on the split side, which is what the code does: `CarriesCommentInRepeatedPart` skips the first declarator's leading trivia, where the parser puts that comment. No production code changed. Only the documentation that stated a line-based boundary ("a comment that starts on a line after the type") was corrected to state the boundary the code uses — trivia ownership — and both kinds of first-declarator comment FMT-STRUCT-030 names. `scripts/verify-text-only.sh --base HEAD --head worktree`: `TEXT-ONLY PROOF: PASS` (2 files, changed trivia: documentation).

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/FieldDeclarationSplitTransform.cs` | `CarriesCommentInRepeatedPart` summary: inspects the trivia of every token from the first token up to the first declarator; boundary is trivia ownership, not the line; excluded are the declaration's leading trivia and the first declarator's leading trivia (own-line comment after the type, and `///`/`/** */` behind the type in Parse/Diagnose); an ordinary comment behind the type, or a doc-style one in `None`, is the type's trailing trivia and counts. `Splits` summary: states that a comment in the first declarator's leading trivia does not count. | FMT-STRUCT-030, FC-031 |
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | `SplitsFieldDeclaration` summary: names the two first-declarator comment kinds that do not count, as FMT-PIPE-031 does. | FMT-PIPE-031, FC-031 |

`RH7101DoNotCombineFieldsCodeFixProvider.TryGetFixableFieldDeclaration`'s summary was re-read: it defers the decision to the formatter ("a comment in the part every field would repeat") and stays accurate; not changed.

### Policy owners
- `rg -n "SplitsFieldDeclaration|FieldDeclarationSplitTransform.Splits|Splits\(" src --glob '!*Test*'`: one owner, `FieldDeclarationSplitTransform.Splits`; unchanged since `09-auditor.md`.

## thoenissen/Reihitsu#1384

No change: the documented-declaration guard (`IsDocumented`) and its summary are untouched by the amended requirements.

## Tests run

```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Structural.FieldDeclarationSplitTests|FullyQualifiedName~Reihitsu.Formatter.Test.Unit.ReihitsuFormatterSplitsFieldDeclarationTests|FullyQualifiedName~Reihitsu.Formatter.Test.Unit.ReihitsuFormatterFieldSplitTests"
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH7101DoNotCombineFieldsAnalyzerTests"
scripts/test.sh --configuration Debug --project architecture
```

Formatter 173/173, RH7101 107/107, architecture 115/115 passed. `scripts/format.sh` on both files: `Formatted 0 of 2`.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None_ beyond those in `00-lead.md`.
