Role: auditor
Commit: bde5ae4c1d6f148d5df5390d4347623a8cb3ee78
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Lead decision 6 names the repair shape. I audited the tests against SHARED-CASING-009 and finding #2, not against that decision.

## Scope

This audits only the repair delta of round 2. Head: `bde5ae4c1d6f148d5df5390d4347623a8cb3ee78`. Previous audit: `b49e789626242fc0ac64874148eee76e5ed747d4`. Base: `origin/main` `5db426fd5f32ae7295927005bc8147e63a1b9669`. `git ls-remote` returns that SHA, it is an ancestor of the head, and the checkout is the head. `scripts/lane-check.sh bug-fix` confirms the lane.

The diff matches handovers 12–14. The only non-`sdd/work/` change of the round is commit `2f832c2`, which adds seven tests to three files:
- `RH4002ClassNameCasingAnalyzerTests`: one test, `global::`.
- `RH4103MethodNameCasingNameBindingTests`: four tests, `base.`, `Outer.Base.`, `A.B.Base.`, and the cref.
- `RH4106PrivateFieldCasingAnalyzerTests`: two tests, `N.Sample.` and `global::Sample.`.

No production code, spec, or documentation changed in the round (`git diff 3e58a57 HEAD -- src` is empty). Scope did not grow.

The merge of `origin/main` (`5db426f`, #1967) touches only test-literal files in other rule folders, `LineCountSelfHostingTests`, and a squad note. It does not overlap with any file of the run. Its convention is to read message texts from resources, and the new tests quote no diagnostic or code-fix text, so the convention does not apply to them. The largest changed test type is 2823 lines, below the RH2111 test limit of 3000.

How I checked: static tracing of each new test through `FindQualifiedNameToken` and `GetQualifierAndName`. I did not re-run the tests. The test implementer's run and probe are the evidence, and no finding turns on a re-run. I reused all other evidence from `11-auditor.md`, because it is byte-identical.

## Previous findings

- **#1: closed.** Closed in `11-auditor.md`. Round 2 changed no production code.
- **#2: closed.** Each arm and shape it listed now has its own test, and that test fails when only that arm is removed. I traced each one:
  - **(a) `base.GetValue()`:** the qualifier is `BaseExpressionSyntax`. This is not a `TypeSyntax`, so dropping `BaseExpressionSyntax` from the accepted-qualifier check refuses the fix.
  - **(b) `global::Order _value;`:** the walk from the `global` token meets `AliasQualifiedNameSyntax` first. Without that arm, every ancestor that starts at `global` gives no name, so the walk ends with `default`.
  - **(c) `Outer.Base.GetValue()`, `A.B.Base.GetValue()`, `N.Sample._value`:** the left side is the `QualifiedNameSyntax` that the renamer builds. That is a `TypeSyntax`, so narrowing the accepted qualifier to exclude it refuses the fix. `global::Sample._value` pins the alias-qualified left side as a neighbor.
  - **(d) `<see cref="Sample.GetValue"/>`:** `IdentifierName(Sample)` has no qualifier, and its parent is the `QualifiedCrefSyntax`. Without that arm, the next ancestor, the cref attribute, starts at `cref`, so the walk breaks and refuses the fix. The arm is reached, so it is not dead code, and nothing needed to be dropped.

  The test implementer probed all three changes in one run. Each test reaches only one of the arms, so each failure can be attributed to one arm. The one member #2 named that is not pinned is `M.Order.Count` on RH4002. Its qualifier takes the same `QualifiedNameSyntax`-left-side path as the three (c) rows. The rule that renames is not an axis of the check, because `RewritesOtherCodeAsync` has one caller.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope

_N/A — round 2 adds or changes no predicate, guard, or exemption. The guard-scope rows in `11-auditor.md` stand unchanged._

## Policy ownership

_N/A — round 2 changes no policy. The owners recorded in `11-auditor.md` (`FindQualifiedNameToken`/`GetQualifierAndName` and `RH2001…RenameChangesBindingAsync`, plus `RewritesOtherCodeAsync`) are unchanged._

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH4103…FixQualifiesTheCallWithBaseNextToALocalFunctionWithTheNewName` | A `base.` qualifier is accepted | No fix, or any other token changes | `Verify` (full text, single fix, Fix All) | adequate |
| `RH4002…FixQualifiesAReferenceWithGlobalThatANestedTypeOfTheNewNameWouldCapture` | An alias-qualified name whose name is the renamed identifier is accepted | same | `Verify` | adequate |
| `RH4103…FixQualifiesTheCallWithTheNestedBaseTypeAndItsContainingType…`, `RH4103…FixQualifiesTheCallWithTheBaseTypeAndItsNamespace…`, `RH4106…FixQualifiesTheStaticFieldReferenceWithItsTypeAndNamespace…` | A multi-part qualifier in an expression context, as the renamer builds it, is accepted | same | `Verify` with `CodeActionValidationMode.None` | adequate. `None` skips only the comparison between the node kinds of the fix result and the kinds of the parsed fixed text. The full fixed text, re-analysis, and Fix All are still asserted. The comparison would fail only because the renamer builds a `QualifiedName` where the parser reads member accesses, and `main` offers these fixes with the same tree. The comment at each call states this accurately |
| `RH4106…FixQualifiesTheStaticFieldReferenceWithGlobalAndItsType…` | An alias-qualified left side of a member access is accepted | same | `Verify` | adequate |
| `RH4103…FixRenamesTheDocumentationReferenceNextToANestedTypeWithTheNewName` | A qualified cref whose member is the renamed identifier is accepted | same | `Verify` | adequate |

Check 10: every expected value is the renamer's rename plus a `base.`, type, or namespace qualifier, and SHARED-CASING-009 requires exactly that. This run does not change that requirement. Three rows withhold the fix on `main` and get it on the head: (a), `N.Sample._value`, and `global::Sample._value`. These are the issue's own behavior, and the same sentence covers them. No expected value is left undecided.

## Failure classes dismissed

- FC-016, FC-017: closed by the per-arm tests above.
- FC-001 to FC-015, FC-018 to FC-044: round 2 changes no compiled code. The dismissals and verdicts in `11-auditor.md` stand unchanged.
- FC-031: the new test summaries and the three inline comments describe their tests. No `TODO`, no commented-out code, no tracker numbers (`git show 2f832c2 | grep -E '^\+.*(TODO|#[0-9]+)'` finds nothing).

## Architecture findings

_None._

## Hints

- Still in place from `11-auditor.md`: `scripts/apply-fix.sh` does not rename crefs, while the analyzer harness does. This is pre-existing tooling. The cref arm is now covered through the harness, so the gap no longer hides anything in this run.

## Follow-up candidates

_None._
