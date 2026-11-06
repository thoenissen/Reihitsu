Role: auditor
Commit: 4298d935bb8788103c8ff31e4aacbd47669e64a2
Verdict: pass

## Scope

This audits only the repair delta of repair round 2 (lead decision 21) against 65c826e426bdb7fd083567be7173a5ac053a64f6.

- Head 4298d935bb8788103c8ff31e4aacbd47669e64a2 is checked out, and the working tree is clean.
- Base `origin/main` 61718e254d608e0937c561c3e8839a05158f6338, as given in my prompt, is an ancestor of head, so it is merged.
- `scripts/lane-check.sh bug-fix` returns `confirmed`.
- My prompt carried no intended fix, suspected cause, or hoped-for verdict.

The delta matches handovers `30-` to `32-`. Outside `sdd/work/`, it is two hunks:
- `src/Reihitsu.Analyzer.Test/Naming/RH4105MethodParameterCasingAnalyzerTests.cs`, `NoFixIsOfferedWhenAnExcludedBranchOfTheOtherPartOfAPartialMethodNamesTheNewName`: the excluded branch changed from `int arg = other;` to `int arg = 0;`. The expected value is unchanged.
- `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs`: the summary of `GetRenameCascadeAsync` now says "at least every symbol ... that a rename of it renames with it". The change is comment-only, and the cleaner's text-only proof passes.

Scope did not grow. No spec, rule page, or production logic changed. Evidence from `29-auditor.md` about the production guard (probe `h5/oldnames.cs`, harness `run3.cs`) applies byte for byte, because the compiled code is identical.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1–8 | closed | Closed in earlier audits. The delta changes no compiled code. |
| 9 | closed | The excluded branch is now `int arg = 0;`. Its only identifier token is `arg`. The trace for the cascade of `Arg`: `GetOldNames` returns `{Arg, other}`, because both referable names are gone from the renamed cascade `{arg, arg}`. `HasExcludedCodeOccurrenceAsync` compares with `StringComparer.Ordinal` and `StringComparison.Ordinal` (lines 487 and 538–539), so `arg` does not match the old name `Arg`. It matches only the `identifier` arm. If that arm were removed, the check would return `false` and the fix would be offered. So the test can now fail for its stated reason. `30-test-implementer.md` confirms the other side by experiment: a control with `int unrelated = 0;` got the fix, so no other guard withholds this input. |

**Class of #9.** These are the new-name tests whose excluded input could also name an old name. They hold:
- `RH4106…NoFixIsOfferedWhenAnExcludedBranchDeclaresTheNewName`: only `_value`; the old name is `Value`.
- `RH4103…NoFixIsOfferedWhenTheRenameCascadesFromAFileLocalOverrideToATypeWhoseExcludedCodeDeclaresTheNewName`: only `Process`; the old name is `process`.
- `RH4121…NoFixIsOfferedWhenAnExcludedBranchOfTheImplementingPartOfAPartialMethodNamesTheNewName`: `TKey` and `copy`; the old name is `Key`.

No other member fails.

### Previous hints

- **`GetRenameCascadeAsync` summary wording.** Addressed. The summary now agrees with its remark and with `GetOldNames`'s remark.
- **Extension-block `get_Len` over-withhold.** Accepted by lead decision 21. It errs in the safe direction and is not reached by a caller test.
- **Cost.** Joined decision 18 (lead decision 21).

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope
_N/A — the delta adds or changes no predicate, guard, or exemption. The guard table of `29-auditor.md` stands unchanged._

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Old names and new name searched in excluded code | Unchanged since `29-auditor.md`. `GetOldNames` and `HasExcludedCodeOccurrenceAsync` in `CasingCodeFixProviderBase{T}` have one call site, at line 884. | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH4105…NoFixIsOfferedWhenAnExcludedBranchOfTheOtherPartOfAPartialMethodNamesTheNewName` | The new name, named only in the other part's excluded code, withholds the fix. | The fix is offered, which happens when the new-name arm is missing. The input names no old name under ordinal comparison, and the control without `arg` gets the fix. | `Verify` with two documents, the source as the fixed state, single fix and three Fix All scopes. | adequate |

## Visible output is decided
The expected value is unchanged. It is a withheld fix, decided by SHARED-FIX-003 (build configurations) under P1, as decisions 19 and 20 record. No rule page changed.

## Failure classes dismissed
- **Every class except FC-017 and FC-031.** The delta changes no compiled code. That is confirmed by the cleaner's text-only proof for the production file, and the test file changes only test input. The dismissals and guard verdicts of `29-auditor.md` therefore stand unchanged.
- **FC-017.** See *Assertion adequacy*; finding #9 is closed, and its class holds.
- **FC-031.** The changed summary describes the code: the reference search can return more than the renamer renames (for example, the other part's type parameter), and the remark and `GetOldNames` say so. There is no `TODO`, no commented-out code, and no tracker number.

## Architecture findings
_None._

## Hints
_None._

## Commands
```shell
git merge-base --is-ancestor 61718e254d608e0937c561c3e8839a05158f6338 HEAD   # merged
scripts/lane-check.sh bug-fix                                                 # LANE CHECK: confirmed
git diff 65c826e..4298d93 -- src                                              # two hunks: one test input line, one summary
```
