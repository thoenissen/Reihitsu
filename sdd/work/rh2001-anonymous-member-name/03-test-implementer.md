Role: test implementer
Commit: 8cf4e25 (head the tests ran on; tests and this handover are one commit on top of it)
Verdict: tests written

The prompt carried no intended fix, cause, or hoped-for verdict. Expected values come from RH2001-012's examples, SHARED-CASING-015's trivia sentence, and the skeptic's candidate rows, none from patched code.

New file: `tests/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnonymousMemberTests.cs` (`BatchCodeFixTestsBase`). Changed file: `tests/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedNameBindingTests.cs`.

## Tests

All in `RH2001PrivateAutoPropertiesShouldNotBeUsedAnonymousMemberTests`; helper `Verify(source, fixedSource, …)` (single fix, iterative, and Fix All by the testing library) unless stated.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFixWritesTheNameOfAMemberNamedByTheBareReference` (LF, CRLF) — spec example 1, issue input | RH2001-012 | `Verify` | red: no fix offered |
| `VerifyFixWritesTheNameOfAMemberNamedByTheBareReferenceThatCodeReads` (replaces a flipped test) | RH2001-012 | `Verify` | red |
| `VerifyFixWritesTheNameOfAMemberNamedByAThisQualifiedReference` (replaces a flipped test) | RH2001-012 | `Verify` | red |
| `VerifyFixWritesTheNameOfAMemberNamedByAReferenceThroughAnotherInstance` (replaces a flipped test) | RH2001-012 | `Verify` | red |
| `VerifyFixWritesTheNamesOfMembersNamedByAMemberAccessAndAConditionalAccess` — spec example 2, skeptic row 4 | RH2001-012 | `Verify` | red |
| `VerifyFixWritesTheNameOfAMemberOfAnAnonymousObjectInAQueryProjection` (replaces a flipped test; FC-027) | RH2001-012 | `Verify` | red |
| `VerifyFixWritesTheNameOfAMemberThatToStringPrints` (replaces a flipped test; FC-036) | RH2001-012 | `Verify` | red |
| `VerifyFixWritesTheNameOfAMemberThatToStringPrintsInAnExpressionBody` (replaces a flipped test) | RH2001-012 | `Verify` | red |
| `VerifyFixAllWritesTheNamesOfTheMembersOfTwoProperties` — spec example 3, row 17 | RH2001-012 | `Verify`, `NumberOfFixAllIterations = 1` | red |
| `VerifyFixAllWritesTheNameOfAMemberWhoseChainPassesThroughAnotherFixedProperty` — row 14 | RH2001-012 | `Verify`, `NumberOfFixAllIterations = 1` | red |
| `VerifyFixAllWritesTheNameOfAMemberNamedByAPropertyReadThroughAnotherFixedProperty` — row 18 (FC-008, lead decision 7) | RH2001-012 | `Verify`, `NumberOfFixAllIterations = 1` | red: today Value is fixed to `new { _value.Name }`, Name gets no fix |
| `VerifyFixWritesTheNameInFrontOfAThisQualifiedReferenceNextToALocalNamedLikeTheField` — spec example 4, row 7 | RH2001-012 | `Verify` | red |
| `VerifyFixWritesTheNameInFrontOfATypeQualifiedReferenceNextToALocalNamedLikeTheField` — row 8 | RH2001-012 | `Verify` | red |
| `VerifyFixOnlyRenamesAMemberWhoseNameComesFromAnotherIdentifier` — spec example 5 (other side of the "named by a reference" condition) | RH2001-012 | `Verify` | green (fixed today as specified) |
| `VerifyFixWritesTheNameOfAMemberNamedByThePropertyReadThroughItself` — row 9, `new { Value.Value }` | RH2001-012 | `Verify` | red |
| `VerifyFixWritesTheNameOfAMemberOfANestedAnonymousObject` — row 12 (FC-027) | RH2001-012 | `Verify` | red |
| `VerifyFixWritesTheNameOfAMemberThatAWithExpressionSets` — row 10 | RH2001-012 | `Verify` | red |
| `VerifyFixWritesTheNamesOfTheMembersAJoinClauseCompares` — row 11 | RH2001-012 | `Verify` | red |
| `VerifyFixKeepsTheCommentsAroundAMemberWhoseNameItWrites` — row 13 (FC-019/FC-021) | RH2001-012 | `Verify` | red |
| `VerifyFixKeepsACommentOnTheLineAboveAMemberWhoseNameItWrites` (LF, CRLF) — row 13 | RH2001-012 | `Verify` | red |
| `VerifyFixWritesTheNameOfAMemberInThePropertysOwnInitializer` — row 19 | RH2001-012 | `Verify` | red |
| `VerifyNoFixIsOfferedForAMemberNextToATupleElementInAWithheldPosition` — spec example 6, row 16 | RH2001-012, RH2001-011 | `VerifyNoCodeFixIsOffered` | green |
| `VerifyNoFixIsOfferedForAMemberWhenALambdaParameterInABlockBodyIsNamedLikeTheField` — row 15 | RH2001-012, RH2001-011 | `VerifyNoCodeFixIsOffered` | green |
| `FixAllCorrectsEveryDiagnostic` (inherited; scenario `new { Alpha, this.Beta }` over two properties) | SHARED-FIX-001, SHARED-FIX-002 (inherited tags) | `GetFixAllScenario` | red |

Each red test was run and fails on the issue's own difference: the property stays and the member stays as written, because no fix is offered (row 18: Name's fix is not offered). The expected layout of rows 13 (multi-line) and 19 was confirmed by temporary controls with the name already written (`Value = Value`), which pass on this head; the controls were removed.

Existing tests whose output the change flips — removed from `RH2001PrivateAutoPropertiesShouldNotBeUsedNameBindingTests` (tagged RH2001-011, asserting no fix) and re-created as fix tests in the new file:

- `VerifyNoFixIsOfferedWhenAPropertyReferenceGivesAnAnonymousObjectMemberItsInferredName`
- `VerifyNoFixIsOfferedWhenAThisQualifiedPropertyReferenceGivesAnAnonymousObjectMemberItsInferredName`
- `VerifyNoFixIsOfferedWhenAPropertyReferenceThroughAnotherInstanceGivesAnAnonymousObjectMemberItsInferredName`
- `VerifyNoFixIsOfferedWhenAPropertyReferenceGivesAMemberOfAnAnonymousObjectInAQueryProjectionItsInferredName`
- `VerifyNoFixIsOfferedWhenAPropertyReferenceGivesAnAnonymousObjectMemberItsInferredNameThatToStringPrints`
- `VerifyNoFixIsOfferedWhenAPropertyReferenceInAnExpressionBodyGivesAnAnonymousObjectMemberItsInferredName`

Left unchanged (still correct under the change): `VerifyNoFixIsOfferedWhenThePropertyReadThroughAnotherConstructionOfItsGenericTypeGivesAnAnonymousObjectMemberItsInferredName` (RH2001-011, the generic-construction condition still withholds), `VerifyNoFixIsOfferedWhenAPropertyReferenceGivesATupleElementItsInferredName`, and the explicit-name tests `VerifyFixIsOfferedWhenAPropertyReferenceIsTheValueOfAnExplicitlyNamedAnonymousObjectMember` / `...AnonymousObjectMemberAndTupleElement` (they pin the `Name = ` side of the condition). `rg "new \{" tests/Reihitsu.Analyzer.Test/Design/RH2001*` finds no other anonymous creation except a Trivia test with an explicitly named member (`new { A = ... }`), unaffected. The casing tests tagged SHARED-CASING-015 (RH4105, RH4106, RH4107, RH4111–RH4113, RH4115, RH4117, …) pin `CasingAnonymousMemberUtilities` for the casing fixes and are left as they are; they must stay green after RH2001 reuses it.

## Scenarios (bug fix)

_N/A_ (behavior change).

## Not covered, and why

- FC-015 (private copy of the decision): an architecture finding (lead decision 4), not testable by output; the auditor checks the structure.
- FC-023 (escaped or verbatim identifier, `@Value`): rare; RH2001-012 defers the written text to SHARED-CASING-015, and no RH2001 example states it. Left out to avoid pinning an output the spec does not state for RH2001.
- Formatter, CRLF on every test, idempotency: the formatter is not involved (skeptic *Surfaces*); line endings are covered on the issue input and the multi-line comment case, which are the cases where a written separator could carry a line ending.
- Comment/directive in a token gap: the rewrite inserts `Name = ` in front of the member's expression; the comment cases are covered (row 13). A directive inside an anonymous creation in front of the member is not covered: RH2001-004's coverage excludes no such case explicitly, but a directive there is rare and SHARED-CASING-015 has no example for it; the implementer reuses the shared helper, whose behavior the casing tests pin.
- Renamer retargeting: every test goes through the fix's `Renamer` path and checks the renamed references, also `this.`/type qualification (rows 7, 8).

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedAnonymousMemberTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsed"
scripts/test.sh --project architecture
```

On this head: the RH2001 filter gives 23 failed (exactly the red rows above), 317 passed; architecture 221 passed (RH2001-012 now has tagged tests).

## Follow-up candidates

_None._
