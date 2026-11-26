Role: test implementer
Commit: 4fd4bc4 (tests written on it; committed in the commit that adds this handover)
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Repair round 1: auditor finding #1 (08-auditor.md), and the shapes the change doc gained in 09-spec-author.md.

## Tests

All tests are in `tests/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests.cs`, behind `VerifyNoCodeFixIsOfferedForAGroupOfAWholeMemberBehindAPropertyWithoutAModifier`. Each runs under `\n` and `\r\n` (DataRow).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyNoCodeFixIsOfferedForAnEndifAloneBehindAPropertyWithoutAnInitializer` | RH2001-010 (FC-003, FC-017) | `VerifyNoCodeFixIsOffered` | pass (change doc section 2, new example; auditor shape (a)) |
| `VerifyNoCodeFixIsOfferedForAnIfAloneBehindAPropertyWithoutAnInitializer` | RH2001-010 (FC-003, FC-017) | `VerifyNoCodeFixIsOffered` | pass (auditor shape (b): `#if` alone behind, `#endif` behind a later member) |
| `VerifyNoCodeFixIsOfferedForAPropertyThatIsTheActiveElseBranch` | RH2001-010 | `VerifyNoCodeFixIsOffered` | pass (change doc section 3, new example) |
| `VerifyFixKeepsAPropertyInTheMiddleOfAGroupAroundSeveralMembers` | RH2001-010 | `Verify` (full fixed source, convergence) | pass. It is the other side of the boundary, which the change doc's *Unchanged* now names. The output `private readonly int _b;` keeps the group in place, as the auditor observed |

Fail-for-the-right-reason check: I narrowed the behind check of `HasConditionalDirectiveNextToTheProperty` locally, then reverted it (`git diff src` is empty).

- The check sees only `IfDirectiveTrivia`: exactly `…AnEndifAloneBehind…` fails (LF and CRLF). The other 174 pass.
- The check sees only `EndIfDirectiveTrivia`: exactly `…AnIfAloneBehind…` fails (LF and CRLF).

So the behind side's kind axis is now pinned on both arms. Before this round, no test told either mutation apart.

## Scenarios (bug fix)

_Not applicable (behavior change)._

## Not covered, and why

- Existing outputs this round changes: none. The change doc's new shapes add no requirement, and no existing test asserts either shape's `main` output. Before writing the tests, I checked each shape against the existing `#if`/`#else`/`#endif` tests in the file. The closest one, `…AGroupWhoseExcludedBranchInFrontOfTheActiveElseMemberHoldsAWholeMember`, has the property behind the group, not inside its `#else` branch.
- `#elif`/`#else` kind isolation on the behind token: the static argument in 04-test-implementer.md holds (the auditor checked it). Such trivia always also holds the group's `#if` or `#endif`.
- Fix All: the existing `VerifyFixAllSkipsThePropertiesBehindWhichAConditionalGroupStands` already covers skipping a property next to a group together with fixing its neighbours. The new shapes take the same per-property decision, so a further Fix All test would pin nothing new.
- Formatter, rename, and literal failure classes: as in 04-test-implementer.md. The round adds tests only.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2001"
```

Both are green on the head with the new tests (176 and 313 passed).

## Follow-up candidates

_None._
