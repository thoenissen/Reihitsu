Role: implementer
Commit: 5e2c359 (base of this round; the round's own commit carries this file)
Verdict: tests pass

Repair round 2 for `16-auditor.md` findings #4 and #5 (`00-lead.md` decisions 19–21). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the recorded decisions. I worked from RH2001-011 (paragraphs 3 and 5, as of 0730294) and the tests of 45efff8.

Start state: `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests` had 241 tests and 6 failures (t1–t6), as `18-test-implementer.md` states. The four withheld tests of decision 19 (g2, g5, g1, q1) were green.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` | `GivesInferredMemberNameAsync`: the tuple arm now withholds only when the new `TupleTypeKeepsElementName` holds. The anonymous-object arm is unchanged. | RH2001-011 ¶3 (finding #5) |
| same | New private `TupleTypeKeepsElementName`. It returns `false` when the outermost enclosing tuple is the `Left` of an assignment, which is a deconstruction target, nested tuples included. Otherwise it returns `true` only when `GetTypeInfo(tuple).ConvertedType` is a tuple type whose element at the argument's position has the inferred name. One syntactic walk up the tuple nesting, plus one `GetTypeInfo`. | RH2001-011 ¶3 (finding #5) |
| same | The summaries and remarks of `RenameChangesBindingAsync` and `GivesInferredMemberNameAsync` now describe the narrowed tuple rule (FC-031). | — |

Decision 19 (finding #4) needed no code. g2, g5, g1, and q1 are withheld by the existing comparison and inferred-name check, and they stay green. Paragraph 5 now states that refusal. I did not touch `GetOtherSymbolReferencesAsync` or `DescribeQualifier`, and I did not adopt `OriginalDefinition`, as decision 19 says.

Evidence for the shape of the check:
- I disabled the deconstruction-target clause as a probe. Only t1 and t5 then failed. The converted type of a deconstruction target still carries the inferred name, so the syntactic clause is needed.
- An earlier draft also had a clause for the source of a deconstruction (`assignment.Left is TupleExpressionSyntax or DeclarationExpressionSyntax`). I removed it because the converted-type check already offers that side. I measured this with `scripts/apply-fix.sh RH2001` on fixtures in `/tmp/claude-0/-home-user-Reihitsu/4271dfb1-87ee-51f4-9b4e-ea7505ebffbd/scratchpad/r19fx`, under LF and CRLF:
  - `var (value, count) = (Value, 1);`: fixed.
  - `(a, b) = (Value, 1);`: fixed.
  - The nested target `((Value, other), var x) = ((1, 2), 3);`: fixed.
  - `(int Value, int) pair = (Value, 1);`: no-fix-offered. This matches 17-spec-author's internal choice: a type with an element of that name withholds.
  - The final code gives the same results.

## Policy owners

- `rg -n "TupleElements|IsTupleType" src --glob '!*Test*'` finds only the new helper.
- `rg -n "TryGetInferredMemberName|AnonymousObjectMemberDeclaratorSyntax" src --glob '!*Test*'`: unchanged from `16-auditor.md` (the RH2001 provider and RH3002, which asks a different question).
- The rename-safety owners are unchanged. The FC-015 unification follow-up stays recorded in `00-lead.md`.

## Production diff size

- `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` before this round: 1 file changed, 211 insertions(+), 7 deletions(-).
- After this round (`origin/main` to the working tree): 1 file changed, 246 insertions(+), 7 deletions(-).
- Round delta: 1 file changed, 40 insertions(+), 5 deletions(-). That is one new private method, mostly XML docs. There is no new type, public member, or dependency.

## Deleted special cases

_N/A._

## Tests run

```shell
scripts/format.sh src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs
# Formatted 0 of 1 file(s).
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests"
# Passed!  - Failed: 0, Passed: 241, Skipped: 0, Total: 241
scripts/test.sh --configuration Debug --project architecture
# Passed!  - Failed: 0, Passed: 118, Skipped: 0, Total: 118
```

Whole analyzer project. The single run `scripts/test.sh --configuration Debug --project analyzer` aborted at 5821 passed because the kernel OOM-killed the test host. This sandbox has one memory limit, and the project uses `Parallelize(Workers = 0, MethodLevel)`. So I ran the same build per namespace with `--no-build --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.<ns>."`. Every part passed with 0 failures:

| Namespace | Passed |
|---|---|
| Analyzer | 47 |
| Base | 13 |
| Clarity | 868 |
| Core | 40 |
| Design | 687 |
| Documentation | 1094 |
| Formatter | 474 |
| Formatting | 4958 |
| Naming | 762 |
| Ordering | 892 |
| Performance | 94 |
| Shared | 5 |
| SelfHosting | 16 |
| everything outside those namespaces | 2 |
| **Total** | **9952** |

`dotnet test --list-tests` on the same build lists 9952 test cases. The 8432 in `13-implementer.md` came from a run that was killed partway. 9887 was the count before this run's tests were added. The validator should still confirm the count in a full run with enough memory.

## Disputed tests or scope

_None._

## Architecture findings

_None_ beyond the FC-015 follow-up already recorded.

## Follow-up candidates

_None._
