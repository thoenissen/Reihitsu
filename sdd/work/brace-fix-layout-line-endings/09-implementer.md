Role: implementer
Commit: see the commit that adds this handover (built on 2b5af9a)
Verdict: tests pass

Follow-up round for answer 7 / SHARED-ATTR-022 (decision 14). The prompt carried no intended fix, suspected cause, or hoped-for verdict. Decisions 12 and 13 needed no production change: their tests were already green on 06's code (08).

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributeListShapeCodeFixProviderBase.cs` | New private `StartsOnTheLineOfTheAccessorListBrace`: true when the list's parent is an accessor in an accessor list and the line of that list's `{` equals the line on which the reported list starts. `TryGetFixableAttributeList` withholds the split fix when it is true; the inline comment there names SHARED-ATTR-019 and SHARED-ATTR-022. One cheap syntactic check (line comparison of two tokens), so every split rule (RH5502…RH5531) gets the withhold through the one shared base. | SHARED-ATTR-022; RH5502-004, RH5504-004, RH5516-004, RH5518-004, RH5520-004, RH5528-004, RH5531-006 |

The guard sits at registration time, so the batch Fix All (which registers per diagnostic) withholds the same lists. `ApplySplitCodeFixAsync` is unchanged.

## Policy owners
- `rg -n "OpenBraceToken" src/Reihitsu.Analyzer.CodeFixes src/Reihitsu.Core --type cs | rg -i accessor`: `AccessorOrderingUtilities` (brace line vs. close-brace line, a one-line accessor list test), `SyntaxIndentationUtilities`, `TargetAttributePlacementCodeFixProviderBase` (brace pair lookup), `RH5408…CodeFixProvider` (join check). None decides "list on the `{` line of its accessor list"; the new helper is the only owner.
- `AttributeTargetUtilities.IsAttributeListOnInlineAutoPropertyAccessor` (Core) is the neighbouring guard (one-line auto-property); it is a different, narrower condition and stays as is.

## Production diff size
This round: `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributeListShapeCodeFixProviderBase.cs | 26 ++++++++++++++++++++--` (24 insertions, 2 deletions). Run total (`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`) before this round: 5 files changed, 314 insertions(+), 138 deletions(-). No new type, no new public member.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~AttributeListsMustFollowShapeRules|FullyQualifiedName~RH5405BracesMustNotBeOmitted"`: `Passed! - Failed: 0, Passed: 358, Total: 358` (the 7 red tests of 08 now green).
- `scripts/test.sh --project analyzer --configuration Debug`: `Passed! - Failed: 0, Passed: 8661, Total: 8661`.
- `scripts/test.sh --project architecture --configuration Debug`: `Passed! - Failed: 0, Passed: 117, Total: 117`.
- `scripts/format.sh` on the changed file: formatted 0 of 1.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
