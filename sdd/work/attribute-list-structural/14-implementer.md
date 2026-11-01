Role: implementer
Commit: 5813d4c
Verdict: tests pass

Repair round 2. My prompt named the change asked for (lead decisions 16–20, audit findings 1, 3, 4, 6 and the architecture finding of `11-auditor.md`); it carried no intended fix, suspected cause, or hoped-for verdict beyond those recorded decisions. I worked from `00-lead.md`, `05-implementer.md`, `11-auditor.md`, `12-spec-author.md`, `13-test-implementer.md`, and the spec diff (FMT-STRUCT-028, FMT-STRUCT-029, FMT-BREAK-032). No test was changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Utilities/AttributeListShapeUtilities.cs` | Split and merge take one predicate, `usesMergedListShape`: the split splits only lists without the merged-list shape, the merge merges only lists with it (decision 16, architecture finding). `BuildSplitAttributeLists` drops the trailing trivia of the original `[` on the first created list, and the leading trivia of the specifier (or of the first attribute when there is no specifier) (finding 3, decision 18). Summaries of the class, `SplitAttributeLists`, `MergeAttributeLists`, `FindMergeableGroup`, `UsesMergedListShape` (`<returns>` reworded, finding 6), and `BuildSplitAttributeLists` rewritten | FMT-STRUCT-028, FMT-STRUCT-029 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/AttributeListMergeTransform.cs` | Merges on every owner except an accessor (was: only `ParameterSyntax`/`TypeParameterSyntax`), with `UsesMergedListShape`, so `param:`/`typevar:` lists on types, members, delegates, local functions, and lambdas are merged as on `main` (finding 1, decision 16). Summary rewritten | FMT-STRUCT-029 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/AttributeListSplitTransform.cs` | Passes `UsesMergedListShape`; summary describes the exclusion by list shape, not by owner (finding 6) | FMT-STRUCT-028 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/AttributeTargetFormattingRewriter.cs` | `ShapeAccessorAttributeLists` builds one predicate (`keepAccessorListsSingleLine \|\| UsesMergedListShape(list)`, which is `main`'s `ResolveListShapeMode(list, keepAccessorListsSingleLine)`) and runs split then merge (`Spaced`) with it, as `main` did; a multi-line accessor list now merges its `param:`/`typevar:` lists (finding 1). Summary rewritten | FMT-BREAK-032, FMT-BREAK-031 |
| `src/Reihitsu.Core/AttributeTargetUtilities.cs` | `MergeAttributeListGroup` with `None`: the kept line break is searched from the first list's `[` (was: its `]`), unless a line break stands directly behind that `[`, which stays there (finding 4, decision 19). Summary updated | FMT-STRUCT-029 |

## Policy owners

- `rg -n "MergeAttributeLists\(|SplitAttributeLists\(|UsesMergedListShape|ResolveListShapeMode\(" src --glob '!**/obj/**' --glob '!**/bin/**' --glob '!*Test*'`:
  - list shape in the formatter: `AttributeTargetFormattingShared.ResolveListShapeMode` only, read through `AttributeListShapeUtilities.UsesMergedListShape`; the accessor layout override lives in `ShapeAccessorAttributeLists` and feeds the same predicate to split and merge. Split and merge now share one predicate per owner, so the per-owner condition of the architecture finding is gone (only "accessor or not" chooses which phase shapes the lists, decision 2).
  - analyzer/code-fix copies: `TargetAttributeListShapeAnalyzerBase.ResolveListShapeMode`, `TargetAttributeListShapeCodeFixProviderBase.ResolveListShapeMode`, RH5531 overrides — unchanged, as on `main`.
- Split: `AttributeListShapeUtilities.BuildSplitAttributeLists` (formatter) and `TargetAttributeListShapeCodeFixProviderBase.BuildSplitReplacementLists` (code fix, decision 11, follow-up 6) — unchanged ownership.
- Merge: `AttributeTargetUtilities.MergeAttributeListGroup` only.

## Tests run

```
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~AttributeListStructural"
Passed!  - Failed: 0, Passed: 98
scripts/test.sh --configuration Debug --project formatter      Passed: 4057, Failed: 0
scripts/test.sh --configuration Debug --project analyzer       Passed: 6394, Failed: 0
scripts/test.sh --configuration Debug --project core           Passed: 563, Failed: 0
scripts/test.sh --configuration Debug --project architecture   Passed: 92, Failed: 0
scripts/test.sh --configuration Debug --project cli            Passed: 251, Failed: 0
scripts/build.sh --configuration Debug                         0 Warning(s), 0 Error(s)
```

Changed files were run through `scripts/format.sh`.

Probe (scratchpad, not committed), against the `main` archive of the audit: `[⏎ method: A, B] void M() { }` — `main` gives `[        method: A]⏎[        method: B]` in pass 1 and `[method: A]⏎[method: B]` in pass 2; this head gives the pass-2 output in one pass. This is the same class as decision 18 (a line break behind `[`), here with a specifier in front of the attribute, so the specifier's leading whitespace is dropped as well. `[method:⏎ A, B]` and an accessor's `[param:⏎ A] [param: B] set` are identical to `main`.

## Disputed tests or scope

_None._ The change stays in the five files of round 1; no new public member. The specifier case of the probe above is a member of visible change 1 that the change doc does not name explicitly (it names `[⏎ A, B]`); the lead may want the spec author to mention it, though FMT-STRUCT-028's "whitespace and line breaks behind the original list's `[` are dropped" already covers it.

## Architecture findings

_None._ The architecture finding of `11-auditor.md` is resolved as decision 16 asks: split and merge take one predicate of the list, and the merge runs on every owner the split runs on.

## Follow-up candidates

_None new._
