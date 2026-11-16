Role: implementer
Commit: see the commit that adds this handover (subject "Do not judge unions whose documentation holds a parameter element [skip ci]")
Verdict: tests pass

Repair round 1, audit finding #1 (`08-auditor.md`), against SHARED-DOC-019 (`09-spec-author.md`) and the tests of `10-test-implementer.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Before the change, the 24 handed-over tests showed 17 failures as `10-test-implementer.md` lists (AD0001 under Diagnose, the rule's own report under Parse or for `<param>` without `name`).

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Core/DocumentationAnalysisUtilities.cs` | New `IsUnionWithParameterElement(MemberDeclarationSyntax)`: a `UnionDeclaration` whose documentation comments in its leading trivia (`DirectDocumentationSyntaxChecker.GetDocumentationComments`, the first and every later one) hold an element named exactly `param` or `paramref` at any depth (`XmlDocumentationElementOrderingUtilities.GetTagName`, ordinal) | SHARED-DOC-019 |
| `src/Reihitsu.Analyzer/Base/SplitElementDocumentationAnalyzerBase.cs` | `OnDeclaration` returns before `HasRequiredDocumentation` (so before the expander) for such a union | RH8035-001, RH8036-001 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8108GenericTypeParametersMustBeDocumentedAnalyzer.cs` | `OnDeclaration` returns early for such a union | RH8108-001 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.cs` | `OnDeclaration` returns early for such a union | RH8203-001 |

The guard is a cheap syntactic check of the declaration's own documentation trivia; it runs before any call to `XmlDocumentationExpander`, so the symbol's documentation is never requested for the refused union, in every documentation mode. Nested declarations are separate syntax nodes and are judged on their own. RH8030 and RH8109–RH8111 do not call the helper, as the spec author decided. The base class is shared by RH8001–RH8016, which never receive a union node, so the guard is inert for them. The summaries of the three changed `OnDeclaration` methods are generic and still describe them (FC-031).

## Policy owners
- Refusal of a union with a parameter element: `rg -n "IsUnionWithParameterElement" src` → the helper in `DocumentationAnalysisUtilities` and its three callers; one owner.
- Tag lookup in documentation syntax: `rg -n "GetTagName\(" src/Reihitsu.Analyzer/Core` → `DirectDocumentationSyntaxChecker` and the new helper both use the shared `XmlDocumentationElementOrderingUtilities.GetTagName`; no private copy. The new helper compares ordinally (lower case, as SHARED-DOC-019 covers) instead of reusing `GetTagsIncludingNested`, whose top-level-wins fallback and case-insensitive match do not state the requirement's "at any depth".
- Documentation comments of a declaration: `DirectDocumentationSyntaxChecker.GetDocumentationComments` reused (SHARED-DOC-004/006 set).

## Production diff size
`git diff --shortstat origin/main -- src ':(exclude,glob)src/*Test*/**'`: 13 files changed, 252 insertions, 3 deletions. Delta of this round: 4 files, 23 insertions, 2 deletions. No new type, no public member (the helper is `internal`).

## Deleted special cases
_N/A._

## Tests run
- `scripts/format.sh` over the four changed files → Formatted 0 of 4.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~ParamElement|FullyQualifiedName~ParamrefElement|FullyQualifiedName~ParameterElement"` → Passed: 50, Failed: 0.
- `scripts/test.sh --configuration Debug --project analyzer` → Passed: 10210, Failed: 0.
- `scripts/test.sh --configuration Debug --project architecture` → Passed: 117, Failed: 1 — `SpecStructureTests.UsesListsExactlyTheReferencedRequirements` (see below; not caused by production code).

## Disputed tests or scope
- **Spec structure failure from round 1's spec change (spec author's, not this role's).** `SpecStructureTests.UsesListsExactlyTheReferencedRequirements` fails on the spec files alone:

  ```
  shared/element-documentation.md: RH8035-001 is referenced but missing from Uses
  shared/element-documentation.md: RH8036-001 is referenced but missing from Uses
  shared/element-documentation.md: RH8203-001 is referenced but missing from Uses
  ```

  SHARED-DOC-019 (added in 85c14f1) cites RH8035-001, RH8036-001, and RH8203-001 in its example text, but the `Uses` row of `sdd/specs/shared/element-documentation.md` does not list them. This round changed no spec file, so the failure exists on 50a4fc7 as well. Either the `Uses` row gains the three IDs or the example text stops citing them; that is the spec author's choice. I did not edit the spec.
- Partial unions (not covered by SHARED-DOC-019): the guard reads only the part being analyzed. For RH8203 a partial union is not "part of a partial member" (that path excludes type declarations), so the expander runs on the union symbol, which joins all parts' documentation. A `<param>` in one part may therefore still throw AD0001 when another part without such an element is analyzed (not probed: the spec leaves partial unions out of coverage on purpose, so a test would assert something the spec does not state).

## Architecture findings
_None._

## Follow-up candidates
_None._
