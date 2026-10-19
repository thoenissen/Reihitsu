Role: spec author
Commit: 29637d7e1ddead134abe75b75308fec801d8398f
Verdict: text written

This round finishes the merge of `origin/main` into the #991 branch. `origin/main` added the RH5409–RH5417 specs and the shared specs `empty-type-declarations.md` and `trailing-commas.md`.

## Conflicts

Both conflicts were resolved by keeping both sides:

- **`sdd/specs/README.md`:** this run's rows RH5301–RH5308 and RH5401–RH5408 come first, followed by main's rows RH5409–RH5417, in ID order. Main's shared-spec rows merged without a conflict.
- **`sdd/specs/requirement-ids.txt`:** the ledger holds 1027 IDs. It is in ordinal order after the merge, with no re-sort needed, and holds each ID exactly once. Compared with both `origin/main` and this branch's pre-merge head, no line was removed.

## Overlap check

- **ID prefixes:** main's new shared specs use SHARED-EMPTY and SHARED-TRAIL. They do not overlap with SHARED-INIT, SHARED-BLOCK, or SHARED-BRACE.
- **Code owners:** main's new specs name `EmptyTypeDeclarationShouldUseSemicolonAnalyzerBase`, `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase`, `EmptyTypeDeclarationSemicolonAnalysisUtilities`, `TrailingCommaAnalyzerBase<TNode>`, `TrailingCommaRemovalCodeFixProviderBase`, `TrailingCommaCodeFixHelper`, and the RH5409–RH5417 analyzers and providers. None of these is a code owner in this run's specs.
- **Cross-references:** none of main's new specs references a requirement of this run. Two of this run's non-goals point at main's rules, and both pointers stay accurate:
  - RH5404's non-goal points at RH5412–RH5416 for the semicolon form of empty types.
  - RH5402's non-goal excludes the braces of members and accessors, which RH5417 checks.

## Checks

`scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RequirementCoverageTests"` on the merged tree: 39 passed.

The merge was concluded with `git commit --no-edit` (the prepared `[skip ci]` message) and was not pushed.

## Open questions to file

Unchanged from `03-spec-author.md`.

## Decisions needed

_None._
