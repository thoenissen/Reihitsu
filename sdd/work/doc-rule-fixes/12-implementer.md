Role: implementer
Commit: 79317e3
Verdict: tests pass

Repair round 1, decisions 20 and 21 (#3). The prompt carried no intended fix, suspected cause, or hoped-for verdict. The 8 handed-over RH8203 tests were run on `79317e3` first: 8 failed with `expected "4" actual "0"`, as `11-test-implementer.md` says. No test, assertion, input, or expected output was changed.

## thoenissen/Reihitsu#1617

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Core/XmlDocumentationExpander.cs` | New `GetExpandedDocumentationOfPart`: the expanded documentation of one part of a partial member, from the part's own documentation comments (every `///` and `/** */` trivia of its leading trivia) compiled in front of a placeholder type, with the part's tree path, parse options, and the compilation's options, so the compiler expands every `<include>` with the same resolver and base path | RH8203-001 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.cs` | For a part of a partial member, the expanded documentation now comes from `GetExpandedDocumentationOfPart` instead of being skipped, so a later comment and `<include>` content of that part count, and the other part's never | RH8203-001, RH8203-002 |

Why not the part's symbol: probed on Roslyn 5.9 — the implementing part's symbol returns its own documentation (empty when it has none), but the defining part's symbol returns the implementing part's documentation whenever that part has any, and its own only otherwise. The tests' `O`/`P` members (one part carries the element, the other a summary) need the defining part's own documentation while the implementing part has some, which no symbol of the original compilation exposes.

## thoenissen/Reihitsu#1347

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/DocumentationCommentCodeFixUtilities.cs` | `CanLayOutElement`: removed the conjunct `documentationComment.IsKind(SingleLineDocumentationCommentTrivia)` (unreachable: RH8305 registers on `///` only, RH8307 judges only elements whose parent is a `///` comment) and its wording in the summary; the `IsWrittenBehindSource` check stays. Removed the now unused `Microsoft.CodeAnalysis.CSharp` using | RH8305, RH8307 (auditor finding #3, decision 21) |

## thoenissen/Reihitsu#1345, #1346, #1348, #1270, #1092, #1153
No work in this round.

## Policy owners
`rg -n "GetDocumentationCommentXml|CSharpCompilation.Create" src --glob '!*Test*/**'`: `XmlDocumentationExpander.ParseExpandedDocumentation` is the one reader of expanded documentation; the new method feeds it a symbol and adds no second expansion path. `CSharpCompilation.Create` appears only in the new method.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` after this round: 19 files changed, 370 insertions(+), 101 deletions(-). Round delta: 3 files changed, 42 insertions(+), 9 deletions(-). No new type.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH8203|FullyQualifiedName~RH8305|FullyQualifiedName~RH8307"` — Passed: 138, Failed: 0
- `scripts/test.sh --configuration Debug --project analyzer` — Passed: 8937, Failed: 0
- `scripts/test.sh --configuration Debug --project architecture` — Passed: 117, Failed: 0
- `scripts/build.sh --configuration Debug` — no new warning (the one remaining, S1125 in `Reihitsu.Cli/FormatCommandHandler.cs`, is untouched code)

## Disputed tests or scope
- Mechanism for the lead to weigh: the per-part documentation needs a detached compilation (one small syntax tree, no references) per analyzed part of a partial member that carries a documentation comment. It reuses the compiler's own `<include>` expansion instead of copying it (resolver, XPath, error handling). The alternative is a hand-written include expansion over `CompilationOptions.XmlReferenceResolver` and XPath, which would be a copy of the compiler's behavior. Cost is limited to partial-member parts with documentation; non-partial declarations are unchanged.

## Architecture findings
_None._ (The compiler's partial-member documentation merging is outside the repository; the new method is the one place that works around it.)

## Follow-up candidates
- Other readers of a partial member's expanded documentation (RH8032, RH8033, RH8101, RH8105, RH8108 via `XmlDocumentationExpander.GetExpandedDocumentation`) read the defining part's symbol, which carries the implementing part's documentation when it has any; `GetExpandedDocumentationOfPart` could serve them (already listed in `00-lead.md` from 04-skeptic).

## Decision 23 (lead): detached compilation only for `<include>`

Commit: 27a9160 (base of this addendum)

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.cs` | For a part of a partial member, `GetExpandedDocumentationOfPart` (the detached compilation) is called only when one of the part's own documentation comments holds an `<include>` element (`GetTagsIncludingNested(comment, "include")`, nested ones counted, since the compiler expands those too). Otherwise the part's later comments are read from syntax: the first top-level `<inheritdoc>` of the part's second and later documentation comments, with its `cref` checked by the syntax overload of `HasCrefAttribute`; the report stays on the declaration's name (RH8203-002). Non-partial declarations are unchanged | RH8203-001, RH8203-002 |

`XmlDocumentationExpander.GetExpandedDocumentationOfPart` is unchanged and now serves the `<include>` case alone.

Tests run: `scripts/test.sh --configuration Debug --project analyzer` — Passed: 8937, Failed: 0 (the 8 tests from 11 included). Build: no new warning.
