Role: implementer
Commit: 7c11d1e3
Verdict: tests pass

Final narrowing round, lead rows 44–45 (`00-lead.md`), auditor findings 17–19 (`32-auditor.md`), spec b03a20cc (`33-spec-author.md`), tests cc185a43 (`34-test-implementer.md`). Row 45 (RH3106) needs no code change; its tests pass unchanged. My prompt named the requirements to carry out (RH3004-008 item 1, RH3004-009) and to replace the branch logic rather than add beside it; that matches row 44 and the spec, and it carried no suspected cause or hoped-for verdict to set aside. No output questions raised (standing instruction).

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs` `LeavesStringOpen` → `ContainsStringWithDirectiveLine` | The branch-membership logic is gone: no depth counting, no `branchStarts` list. An outermost string of active code in the block withholds the fix when a line after its first starts with `#` behind whitespace (`ContainsLineStartingWithHash`, unchanged), whatever directives stand near it. The disabled-text check (a string lexed from disabled text that runs open to its end, `IsOpenStringToken`) is kept unchanged. Still called on the block-body path only. Summary rewritten. | RH3004-008 items 1 and 2 |
| same file, `IsInBranch` | Removed; item 1 subsumes it. | RH3004-008 |
| same file, new `ContainsCrossingDirectiveChain` | Every `#if`/`#elif`/`#else`/`#endif` whose span lies inside the block is checked with Roslyn's `DirectiveTriviaSyntax.GetRelatedDirectives()`; the fix is withheld when any directive of its chain lies outside the anonymous method's span. | RH3004-009 |
| same file, `RegisterCodeFixesAsync` | Calls `ContainsCrossingDirectiveChain` on both paths (before the expression-body/block-body split, as RH3004-009 is not path-bound), and the renamed check on the block-body path. | RH3004-003, RH3004-009 |

## Policy owners

- Lines of a string that start with `#`: `rg -n "ContainsLineStartingWithHash" src --glob '!*Test*'` — RH3004 only, one owner.
- Conditional-directive chains: `rg -n "GetRelatedDirectives" src --glob '!*Test*'` — RH7004 (an `#if` after a using declaration), RH7302 and RH7306 (`#region` pairs). Each asks a different question of the same Roslyn API; no shared "chain crosses a span" helper exists to reuse, and I added none.

## Tests run

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests|FullyQualifiedName~RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests"
#   Passed! - Failed: 0, Passed: 180, Total: 180   (the 9 tests that failed now pass)
scripts/test.sh --configuration Debug --project analyzer      # Passed: 6962, Failed: 0
scripts/test.sh --configuration Debug --project core          # Passed: 649, Failed: 0
scripts/test.sh --configuration Debug --project architecture  # Passed: 115, Failed: 0
scripts/build.sh                                              # Release: 0 Warning(s), 0 Error(s)
scripts/format.sh <changed file>                              # Formatted 0 of 1 file(s)
```

No test changed.

## Disputed tests or scope

_None._ One file, one new private helper, one removed; no new public member or dependency.

## Architecture findings

_None._

## Follow-up candidates

_None new._ The two open generalizations (deciding `#` lines on the document's whole directive chain; building the lambda so a crossing chain keeps its layout) are recorded in RH3004-008/-009 and in `00-lead.md`.
