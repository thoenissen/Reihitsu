Role: test implementer
Commit: 89789bdd (tests written on this head; see the commit that adds this file)
Verdict: tests written

Repair round 2, decision 24 of `00-lead.md`: an architecture test that only `ReihitsuCodeFixProviderBase` and `DocumentOrderFixAllProvider` call `FixAllProvider.Create`. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond that recorded decision.

## Per issue
- thoenissen/Reihitsu#1600 — new architecture test pins decision 21's single owner of the per-document offering decision on every Fix All path (SHARED-FIX-012).
- thoenissen/Reihitsu#1150 — same test covers SHARED-FIX-011 on the custom Fix All paths, which go through the same base decision.
- thoenissen/Reihitsu#1274 — nothing in this round.
- thoenissen/Reihitsu#1143 — nothing in this round.
- thoenissen/Reihitsu#1673 — nothing in this round.
- thoenissen/Reihitsu#1678 — nothing in this round.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.ArchitectureTests.Analyzer.FixAllProviderCreationTests.FixAllProviderCreateIsCalledOnlyByTheProviderBaseAndTheDocumentOrderFixAllProvider` | SHARED-FIX-011, SHARED-FIX-012 | ArchUnitNET `Assemblies.Implementation.MethodMembers`, `MethodCallDependency` | pass |

The test collects every method of `Reihitsu.Analyzer.CodeFixes` with a call dependency on `Microsoft.CodeAnalysis.CodeFixes.FixAllProvider.Create(...)`, maps it to its outermost declaring type (so a call inside a lambda closure or async state machine counts for the type whose source holds it), and asserts the set equals exactly `{ReihitsuCodeFixProviderBase, DocumentOrderFixAllProvider}`. Equality rather than a subset check keeps the test from passing vacuously if the scan stopped seeing calls.

## Evidence
- Head `89789bdd`: the test passes; whole architecture project 118/118.
- Pre-repair commit `417800b` (scratch worktree with the test file copied in, removed afterwards): the test fails with `Calling types: ...DocumentOrderFixAllProvider, ...RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider, ...RH7101DoNotCombineFieldsCodeFixProvider` — exactly the two providers that called `FixAllProvider.Create(ApplyFixAllAsync)` directly. (At `417800b` the base had no `CreateFixAllProvider` yet, so it is absent from that list.)

## Not covered, and why
- A custom `FixAllProvider` subclass (such as `CasingFixAllProvider`) or `WellKnownFixAllProviders.BatchFixer` is outside decision 24: BatchFixer re-registers through the base's sealed `RegisterCodeFixesAsync`, and the casing Fix All asks `HoldsHidingCarrierAsync` itself (15-implementer.md). The test pins only the delegate-based `FixAllProvider.Create` route.
- No failure class of the catalog applies to an architecture test of call sites; no formatter, line-ending, or code-fix output is involved.

## Commands
```shell
scripts/test.sh --configuration Debug --project architecture --filter "FullyQualifiedName~FixAllProviderCreationTests"
```
