Role: implementer
Commit: 36ab5af
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md` (decisions 2, 4–11 and maintainer answer 1 bind the change), `01`–`04`, the change doc, and the spec diff. No test was changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/Enumerations/AttributeListMergeSeparators.cs` (new) | Public enum `Spaced` / `None`: the separators a merge writes | FMT-STRUCT-026, FMT-STRUCT-029, SHARED-ATTR-018, FMT-BREAK-031 |
| `src/Reihitsu.Core/AttributeTargetUtilities.cs` | `MergeAttributeListGroup` gains an overload with `AttributeListMergeSeparators`; the old two-argument signature forwards `Spaced`, so the code fixes are unchanged. `None` writes no whitespace and no elastic trivia (created commas carry nothing, merged list has no trailing space, no space added in front of a joined token), and carries the first author-written line break of the stretch (first list's `]` to the token behind the last member) behind the merged list when the merge leaves none between the two. New private helpers `FindLineBreak`, `KeepLineBreakBehindMergedList` | FMT-STRUCT-029, FMT-STRUCT-026 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Utilities/AttributeListShapeUtilities.cs` (new) | The one split (`SplitAttributeLists`, `BuildSplitAttributeLists` moved from the line-break rewriter) and the one merge loop (`MergeAttributeLists`, `FindMergeableGroup` moved from the line-break rewriter). The split reuses the original `[` (with leading trivia) on the first created list and the original `]` (with trailing trivia) on the last, creates brackets without trivia, and drops the trailing trivia of every attribute but the last and the leading trivia of every attribute but the first | FMT-STRUCT-028, FMT-BREAK-032, lead decision 7 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/AttributeListSplitTransform.cs` (new) | Splits on every owner except accessors (parameters and type parameters are excluded by the shape mode); lambdas included (decision 6) | FMT-STRUCT-028 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/AttributeListMergeTransform.cs` (new) | Merges parameter and type-parameter lists with `AttributeListMergeSeparators.None` | FMT-STRUCT-029 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/StructuralTransformPhase.cs` | Registers both transforms last, after `FieldDeclarationSplitTransform`; summary updated | FMT-STRUCT-016 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/AttributeTargetFormattingRewriter.cs` | Split and merge removed; new `ShapeAccessorAttributeLists` shapes accessor lists only, through the shared helper (merge with `Spaced` on a one-line property, split otherwise); placement unchanged | FMT-BREAK-027, FMT-BREAK-032, FMT-BREAK-031 |

`LineBreakListRewriter.IsMultiLineAfterAttributeFormatting` is unchanged; it still runs the real attribute rewriter, which now only places non-accessor lists.

## Policy owners

- `rg -n "BuildSplitAttributeLists|BuildSplitReplacementLists|MergeAttributeListGroup\(|FindMergeableGroup" src --glob '!*Test*'`: split — `AttributeListShapeUtilities.BuildSplitAttributeLists` (formatter, sole owner for structural and accessor) and `TargetAttributeListShapeCodeFixProviderBase.BuildSplitReplacementLists` (code fix, left as is per lead decision 11, follow-up 6); merge — `AttributeTargetUtilities.MergeAttributeListGroup` only (callers: the code fix with `Spaced`, the accessor merge with `Spaced`, the structural merge with `None`); group finding — `AttributeListShapeUtilities.FindMergeableGroup` only in the formatter (the code fix selects its group by its own rule scope).
- Shape mode: `AttributeTargetFormattingShared.ResolveListShapeMode` stays the one owner; the split and merge transform read it through `AttributeListShapeUtilities.UsesMergedListShape`.

## Tests run

```
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~AttributeListStructural|FullyQualifiedName~ParameterAttributeTargetLayoutTests"
Passed!  - Failed: 0, Passed: 100
scripts/test.sh --configuration Debug --project formatter      Passed: 4031, Failed: 0
scripts/test.sh --configuration Debug --project analyzer       Passed: 6394, Failed: 0
scripts/test.sh --configuration Debug --project core           Passed: 563, Failed: 0
scripts/test.sh --configuration Debug --project architecture   Passed: 92, Failed: 0
scripts/test.sh --configuration Debug --project cli            Passed: 251, Failed: 0
```

Probe outside the tests (scratchpad, not committed): `record R([A] [property: B]⏎ [C] int X, int Y);`, `void M([A]⏎ [property: B] [C] int x, int y)`, a lambda argument `Use([A, B] (int z) => z, 1)`, and `[A,⏎ B] int P { [C, D] get => 1; }` format identically to `origin/main` except the change-doc row `[     B]` → `[B]`, and a second pass is stable.

The changed files were run through `scripts/format.sh`. The Debug build had only RH5603 (final newline) warnings on the new files before formatting; the Release warning check is the cleaner's.

## Disputed tests or scope

- New public member in Core: `AttributeListMergeSeparators` and the three-argument `MergeAttributeListGroup` overload. This is the shape lead decision 7 asked for (one merge, callers differing in separators); reported because `implementer.md` names a new public member as scope growth.
- No disputed tests.

## Architecture findings

_None._ The two findings of `02-skeptic.md` are resolved as decision 7 asked: one formatter split and one merge. The code fix's own split remains (decision 11, follow-up 6).

## Follow-up candidates

_None new._ Code-owner rows and the `AGENTS.md` phase-ownership rows (lead decision 3) are still open for the spec author: the new types are `AttributeListSplitTransform`, `AttributeListMergeTransform`, and `AttributeListShapeUtilities`.
