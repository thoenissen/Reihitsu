Role: implementer
Commit: abacf27
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Before the change, the 17 new tests and the 5 `AttributeListStructuralFinalOutputTests` named in 04 failed as that handover states (04's run at 35b2e85). After it, all of them pass.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/Enumerations/AttributeListMergeSeparators.cs` | New member `SpacedBehindList`: one space behind the merged list and behind a token the merge leaves touching a joined token (as `Spaced`), and commas without any trivia. `Spaced`'s summary now names its elastic commas and says only the code fixes use it | FMT-BREAK-032, FMT-PIPE-034 |
| `src/Reihitsu.Core/AttributeTargetUtilities.cs` (`MergeAttributeListGroup`) | The commas are elastic only for `Spaced`. `SpacedBehindList` and `None` write bare commas. The space behind `]` and behind joined tokens is written for both spaced modes. The line-break carry-over stays `None`-only. The XML summary was updated | FMT-BREAK-032, FMT-BREAK-031 (recorded exception unchanged), SHARED-ATTR-018 (code-fix path unchanged: `Spaced`) |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/AttributeTargetFormattingRewriter.cs` (`ShapeAccessorAttributeLists`) | Merges with `SpacedBehindList` instead of `Spaced`. The summary now describes the separators | FMT-BREAK-032, FMT-PIPE-034 |

## Policy owners
- `rg -n "AttributeListMergeSeparators\.|MergeAttributeListGroup\(" src --type cs` (production): the line-break rewriter (now `SpacedBehindList`), `AttributeListMergeTransform` (`None`, unchanged), and `TargetAttributeListShapeCodeFixProviderBase` (`MergeAttributeListGroup(owner, group)` overload → `Spaced`, unchanged). The single owner of the merge stays `AttributeTargetUtilities.MergeAttributeListGroup`.
- `rg -n "ElasticSpace|ElasticCarriageReturnLineFeed|ElasticLineFeed|ElasticMarker|Elastic\w*\(" src/Reihitsu.Formatter src/Reihitsu.Core --type cs` finds no explicit elastic factory. The only remaining implicit elastic source is `SyntaxFactory.SeparatedList(nodes)` in the `Spaced` (code-fix) path.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 3 files changed, 28 insertions(+), 16 deletions(-). No new type. There is one new public enum member, `AttributeListMergeSeparators.SpacedBehindList`.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~NoElasticPhaseOutputTests|FullyQualifiedName~AccessorAttributeMergeCommaTests|FullyQualifiedName~AttributeListStructuralFinalOutputTests"`: Passed 56, Failed 0
- `scripts/test.sh --configuration Debug --project formatter`: Passed 5416, Failed 0
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 10463, Failed 0. This includes the RH5524, RH5531, and RH5111 tests from 04 and every SHARED-ATTR-018 merge output
- `scripts/test.sh --configuration Debug --project core`: Passed 738, Failed 0
- `scripts/test.sh --configuration Debug --project architecture`: Passed 180, Failed 0
- `scripts/format.sh` on the three changed files: formatted 0 of 3

## Disputed tests or scope
- The change adds a new public member, the enum value `AttributeListMergeSeparators.SpacedBehindList` in `Reihitsu.Core`. It is needed because the phase and the code fixes must now write different commas while sharing the space behind `]`. The other way to do it is a second parameter of `MergeAttributeListGroup`. Its consumers are only the three listed above.

## Architecture findings
_None._

## Follow-up candidates
_None._
