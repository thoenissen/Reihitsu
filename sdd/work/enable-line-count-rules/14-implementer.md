Role: implementer
Commit: the commit that adds this handover (parent d33c496e)
Verdict: tests pass

Repair round 1, scope: auditor finding 2 (`13-auditor.md`). My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond pointing at the auditor's suggestion, which I re-derived below.

## Re-derivation

- `UnnecessaryParenthesesTypeArgumentListUtilities` serves only RH3002 and calls back into the analyzer (`ShouldReport`, `GetTokensWithoutParentheses`, `GetUnparenthesizedExpression`); `rg -l "using Reihitsu.Analyzer.Rules" src/Reihitsu.Analyzer/Core src/Reihitsu.Analyzer/Base` found only this file. Placement is the defect, not the logic.
- Where it may live: `Reihitsu.ArchitectureTests/Analyzer/RulesTests` requires every type in a `Reihitsu.*.Rules.*` namespace to have a name matching `RH\d\d\d\d.*`, and every such type to live there. A rule-specific helper beside its rule therefore needs an RH prefix; precedent is `Rules/Naming/RH4001TypeNameShouldMatchFileNameHelper`. RH4001 (file name = type name) fixes the file name.
- Alternatives rejected: making the analyzer `partial` and putting the helpers in a second part would let the three methods go back to `private`, but it splits one type across files to stay under RH2111, which counts declarations — that games the limit the run enables. Merging back is impossible under RH2111's 500-line repository limit. The three analyzer methods stay `internal`, as the extraction needs; the mutual recursion is inherent in the split and now stays within RH3002's own namespace.
- No spec names the type (`git grep` outside `sdd/work/` finds no reference), so no spec or code-owner row changes; RH3002's spec lists only the analyzer and the code fix as code owners, which is how RH4001's helper is handled too.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Core/UnnecessaryParenthesesTypeArgumentListUtilities.cs` → `src/Reihitsu.Analyzer/Rules/Clarity/RH3002TypeArgumentListUtilities.cs` | `git mv`; type renamed to `RH3002TypeArgumentListUtilities`, namespace `Reihitsu.Analyzer.Rules.Clarity`, the now-redundant `using Reihitsu.Analyzer.Rules.Clarity;` removed. No body changed; the summary already names RH3002 and stays accurate | Auditor finding 2 (code quality) |
| `src/Reihitsu.Analyzer/Rules/Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.cs` | 5 call sites renamed; `using Reihitsu.Analyzer.Core;` (added by this PR only for the helper) removed | same |

`scripts/format.sh` over both files: no change. FC-031: no method body changed.

## Policy owners
- `rg -l "using Reihitsu.Analyzer.Rules" src/Reihitsu.Analyzer/Core src/Reihitsu.Analyzer/Base` → no match (exit 1): no shared analyzer utility depends on a rule any more.
- `git grep -n UnnecessaryParenthesesTypeArgumentListUtilities -- src` → no match; `RH3002TypeArgumentListUtilities` is the single owner of the type-argument-list guards.

## Production diff size
- `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` before this round: 393 files changed, 3260 insertions(+), 6595 deletions(-); after: 393 files changed, 3257 insertions(+), 6595 deletions(-) (against the work tree).
- Round delta (rename detected): 2 files changed, 7 insertions(+), 10 deletions(-). No new type (one renamed and moved).

## Deleted special cases
_N/A._

## Tests run
- `scripts/build.sh --configuration Release` → Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --configuration Release --project analyzer --filter "FullyQualifiedName~RH3002"` → Passed! Failed: 0, Passed: 260, Total: 260.
- `scripts/test.sh --configuration Release --project architecture` → Passed! Failed: 0, Passed: 198, Total: 198 (includes `RulesTests`).
- `scripts/test.sh --configuration Release --project analyzer` → Passed! Failed: 0, Passed: 11279, Total: 11279.
- `scripts/test.sh --configuration Release --project selfhosting --filter "FullyQualifiedName~AnalyzerSelfHostingTests|FullyQualifiedName~LineCountSelfHostingTests"` → Passed! Failed: 0, Passed: 355, Total: 355 (every analyzer row over the repository source, RH2008/RH2111/RH4001 included).

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
