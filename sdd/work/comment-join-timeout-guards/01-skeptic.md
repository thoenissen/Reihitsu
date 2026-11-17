Role: skeptic
Commit: facc0bfce4c5312fb027ad6439f300c32dbdcf56
Verdict: do not make the change

The prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from the issue, `00-lead.md`, the code, and the commands below.

## Premise

Issue thoenissen/Reihitsu#1846 (`agent-filed`) says that the two guards in `src/Reihitsu.Formatter.Test/Regression/LineBreaks/CommentJoinTests.cs` (`SingleLineParameterAttributeFollowedByCommentDoesNotLoopForever`, `SingleLineParameterAttributeFollowedByDirectiveDoesNotLoopForever`, both `[Timeout(30000, CooperativeCancellation = true)]`) catch an endless loop only if the code under test checks the token. It also says nobody has checked whether the attribute-placement loop does.

**The premise is answered, and the issue's "Done when" already holds on `main`.**

- The loop does check the token on every iteration. In `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/AttributeTargetFormattingRewriter.cs`, `ApplyPlacement` (line 167) is a `while (true)` loop that calls `_cancellationToken.ThrowIfCancellationRequested()` first in each pass (line 172). `LineBreakPhase.Execute` passes its token to the rewriter (`LineBreakPhase.cs:49`), and the tests pass `TestContext.CancellationToken` to `Execute` (`CommentJoinTests.ExecutePhase`).
- The loop was fixed by `TryReportChange` (line 97). It reports no change when the helper returns the same owner (`ReferenceEquals(updatedOwner, owner)`, line 99). `CollapseTokenToSameLine` returns the original node when it refuses a join (`LineBreakTriviaUtilities.cs:95` onward). History before `f81cb53` (#1717) is squashed, so the old loop exists only as described in the `TryReportChange` summary: "an unconditional 'changed' would spin `ApplyPlacement` forever".

**Experiment 1: restore the loop.** In a separate scratch worktree at `facc0bf`, I removed the `ReferenceEquals` early return from `TryReportChange`, so it reports a change unconditionally. The token check stayed.

```
scripts/test.sh --project formatter --filter "FullyQualifiedName~CommentJoinTests.SingleLineParameterAttribute"

  Failed SingleLineParameterAttributeFollowedByCommentDoesNotLoopForever [30 s]
   Test 'SingleLineParameterAttributeFollowedByCommentDoesNotLoopForever' timed out after 30000ms
  Failed SingleLineParameterAttributeFollowedByDirectiveDoesNotLoopForever [30 s]
   Test 'SingleLineParameterAttributeFollowedByDirectiveDoesNotLoopForever' timed out after 30000ms
Failed!  - Failed:     2, Passed:     0, Skipped:     0, Total:     2, Duration: 30 s
```

Each test fails within its time limit. That is exactly the issue's "Done when".

**Experiment 2: also remove the token check** (line 172). The run was bounded from outside with `timeout 240 scripts/test.sh ...` (same filter). The build finished and the test run started, but no test result appeared before the outer timeout killed it at 240 s (exit 124). This confirms the issue's statement about MSTest 4.5.3: the cooperative timeout does not stop code that never checks the token. The guards therefore depend on the check at line 172. They catch the regression the tests document (the change report), but not a rewrite of `ApplyPlacement` that also drops its token check.

**Baseline** (scratch worktree, unchanged): both tests pass (`Passed: 2`, 211 ms).

**Reachability.** The case is real for the CLI. `scripts/format.sh` on the two test inputs as files (comment and `#nullable enable` between `[NotNull]` and `string target`) exits 0 on `main`, and a second pass changes nothing. The loop is reached through whole-document formatting: with the restored loop from experiment 1, `timeout 150 scripts/format.sh <dir>` on the same two files was killed at 150 s (exit 124).

## Mechanism and candidates

Failure class: **FC-011**. A loop-until-stable whose guard refuses an edit but still reports "changed" upstream hangs.

Mechanism: `ApplyPlacement` repeats while a placement pass reports a change. A pass can report a change without changing the owner only when `TryApplyPlacementToList` refuses a join. Only the `SingleLine` branch can refuse (`CollapseTokenToSameLine` returns its input for a comment, directive, or disabled text in the gap). The `SeparateLine` branch (`MoveTokenToNewLine`, `LineBreakTriviaUtilities.cs:74`) always prepends a line break and never refuses.

Dispatch: `TryApplyPlacementToList`, with `ResolvePlacementMode`. The `SingleLine` mode applies to lists whose layout target is a parameter or a generic parameter (`AttributeTargetFormattingShared.UsesParameterLayout`), and to accessor lists of a one-line property (`keepAccessorListsSingleLine`).

Candidates (owners in `SingleLine` mode with an unjoinable gap behind the list):

| Owner | Defining type | Reaches the loop via CLI (restored loop hangs) | On `main` | Covered by a guard today |
|---|---|---|---|---|
| Parameter, comment in gap | `ParameterSyntax` | yes (exit 124) | terminates, unchanged layout | yes |
| Parameter, directive in gap | `ParameterSyntax` | yes (exit 124) | terminates | yes |
| Type parameter, comment in gap | `TypeParameterSyntax` | yes (exit 124) | terminates | no |
| `[param: …]` list on a setter, comment in gap | `AccessorDeclarationSyntax` (target `Parameter`) | yes (exit 124) | terminates | no |
| Accessor of a one-line property | `AccessorDeclarationSyntax` | not reachable: a line break in the gap makes the property multi-line, so `keepAccessorListsSingleLine` is false, and with no line break there is nothing to collapse | – | – |
| Disabled text in gap | – | not a separate member: disabled text always stands between directives, so the directive member already covers it | – | – |

The fixtures for the two uncovered members (byte-exact, run with `scripts/format.sh <dir>`):

```csharp
public class G<[A]
    // note
    T>
{
}
```

```csharp
public class C
{
    public int P
    {
        get;
        [param: A]
        // note
        set;
    }
}
```

On `main` both terminate. The first becomes `public class G<[A]⏎⏎               // note⏎               T>;`, which is FMT-STRUCT-017 (an empty type gets a semicolon declaration) plus FMT-BLANK-011 (a comment behind `]` gets a blank line). The second only gains the FMT-BLANK-011 blank line above `[param: A]`. Both outputs are specified behavior, and I raise no output question about them.

`TryReportChange` guards by the cause (the owner did not change), not by a list of cells. It therefore covers every member above alike.

## Breaking inputs

- FC-011 is the only class that reaches this code. Its matrix is above. On `main` no member fails, and every reachable member terminates.
- No other class applies, because the run changes no production code.
- If the lead takes the optional test addition (Internal choices, 2), the inputs to cover are the two uncovered fixtures above, run through `LineBreakPhase` like the existing guards. They were checked with whole-document formatting; I did not run them through `LineBreakPhase` alone.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `AttributeTargetFormattingRewriter` (line-break phase) | must stay: it already checks the token every pass (line 172) and reports a change only when the owner changed (line 99) |
| `CommentJoinTests` guards | must stay: they fail within 30 s on the restored loop (experiment 1) |
| Analyzer, code fix, Fix All, `Reihitsu.Core`, rule docs, specs | not involved |
| Test-run / CI hang protection | not involved in this run (see Follow-up candidates) |

## Refuse option

There is nothing to refuse: no visible output and no input class is at stake. The issue asks for one of two remedies:

1. **"Make the loop observe the token."** It already does (line 172), so this costs nothing and changes nothing.
2. **"Replace the cooperative timeout with a guard that fails without the code's help."** An example is running the phase on a task and asserting `Wait(TimeSpan)` from outside. It would add a test-side mechanism whose only gain is the case from experiment 2: a future rewrite of `ApplyPlacement` that drops both the change detection and the token check. On a timeout it would also leave a busy thread pool thread spinning in the test host. I did not run this variant. A run-wide hang guard (Follow-up candidates) covers that case, and every other test too, more cheaply.

Simpler option: **no change.** The guards already meet the issue's "Done when", as experiment 1 shows. Declaring a shape unsupported is not an option here, because no formatter shape is in question.

## Output questions

_None._ The run changes no visible output. The formatter outputs observed above follow FMT-STRUCT-017, FMT-BLANK-011, and FMT-BREAK-001.

## Internal choices

1. **Result of the run.** (a) Ship no code change. The PR states that the guards already fail within their limit on a restored loop (experiment 1) and that the loop checks the token every pass. (b) Additionally, edit the two tests' `<summary>` text to say that the timeout relies on `ApplyPlacement` checking the token each pass. That is a text-only change in the tests lane. **Recommendation: (a).** If the lead prefers a durable pointer, use (b): it is cheap, and `scripts/verify-text-only.sh` can prove it. The dependency is easier to see at its source, though, and its source is production code, outside this lane.
2. **Coverage of the two uncovered FC-011 members** (type parameter, `[param: …]` on a setter). (a) Add two more guards like the existing ones. (b) Leave them. **Recommendation: (b).** The fix guards by the cause, so a regression of `TryReportChange` already fails the existing two guards. More members add no detection power for the regression the issue is about. Take (a) only if the lead wants FMT-BREAK-001 coverage per owner kind for its own sake.
3. **PR link.** The issue's premise ("not checked yet") is settled, and its "Done when" holds without a change. The issue's purpose is met on `main`. It is still a report whose concern does not reproduce. The lead decides whether this falls under escalation 4 (a report that does not reproduce) and whether the link is `Refs #1846` (my recommendation, so the maintainer closes it) or `Closes #1846`.

## Issue goals

- **"Check whether the loop checks the token on each iteration."** Done: yes, line 172. Changes no visible output.
- **"Either make the loop observe the token or replace the cooperative timeout."** Drop both. The first already holds. The second guards only against a regression that also deletes the token check, and a run-wide hang guard covers that better. Dropping it changes no visible output.
- **"Done when: a reintroduced endless loop makes each test fail within its time limit, shown by temporarily restoring the loop."** Already met on `main`, shown by experiment 1. Changes no visible output.

## Architecture findings

_None._ The loop has a single owner, and the change report decides by the cause.

## Follow-up candidates

- **Run-wide hang protection for the test runs (tooling).** No workflow sets `timeout-minutes`, and neither `scripts/test.sh` nor `.github/workflows/build.yml` / `SonarCloud.yml` passes a hang guard such as `dotnet test --blame-hang-timeout`. A test that hangs in code without a token check, as in experiment 2, keeps the CI job running until GitHub's default job limit. A per-run hang timeout would fail any such test, whatever the code under test checks. That would also cover the regression class the issue worries about across every test project, not only these two guards. Not checked: whether `--blame-hang-timeout` works with the MSTest runner configuration used here. The tooling run should establish that before choosing the mechanism.
