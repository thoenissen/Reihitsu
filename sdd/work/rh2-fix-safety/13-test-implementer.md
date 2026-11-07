Role: test implementer
Commit: 09cc54c (base); this handover's commit carries the tests
Verdict: tests written

Repair round 1, second pass. This pass narrows the round-11 tests (`11-test-implementer.md`) to the outputs that Q5 = A, Q6 = A, decision 21, and the spec changes of 5d0db01 (RH2005-003, RH2005-005, RH2001-007) now decide. The prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. Expected values are the spec examples verbatim, except the RH2001 name-on-its-own-line case, which decision 21 decides (the current output, observed with `scripts/apply-fix.sh RH2001` under LF and CRLF). No production code was touched.

Result on `09cc54c` plus these tests: `RH2001` and `RH2005` test classes give 17 failed and 86 passed. The 17 failures are exactly the tests marked "fails" below. I checked the reasons:
- RH2005-003 tests: `int Counter;` at column 0 instead of under `public`.
- Every no-fix test: "No code fix may be offered for the diagnostic".

Architecture: the `Sdd` filter passes 51 of 51 (RequirementCoverageTests now finds RH2005-005 covered). The full architecture run passes 115, and the Release build shows no warning in the changed files. `scripts/format.sh` changed nothing in either file.

## Multi-outcome helper

`VerifyCodeFixIsWithheldOrProducesOneOf` (and `TryApplyCodeFixAsync`, which it uses) **stays**. Two RH2001 tests still use it, and no spec decides their outcome:
- `VerifyFixKeepsACommentLineInFrontOfTheType` (`private⏎    // note⏎    int Value`);
- `VerifyFixKeepsACommentBehindTheTypeOnTheSameLine` (`private int /* note */ Value`).

RH2001-007 states both places as "not covered by this requirement", and RH2001-004 covers only properties without a comment inside the declaration. Only SHARED-FIX-003 (no drop, no move) constrains them, so they keep accepting withheld, in place, or formatted. Both pass today. Today the fix is offered for both; the first writes `private⏎⏎    // note⏎    int _value;` (the formatter's layout, with the inserted blank line), the second `private int /* note */ _value;`. If the lead wants them pinned, that is a spec question for RH2001 (which output, or withhold), not a test choice.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH2005 `VerifyFixKeepsTheLineOfAPublicThatStartsItsOwnLineBehindAnotherModifier` | RH2005-003 | `Verify(source, fixed, …)`, spec example | fails (`int` at column 0) |
| RH2005 `…CarriageReturnLineFeed` | RH2005-003 | same, CRLF | fails |
| RH2005 `VerifyNoFixIsOfferedForACommentLineInFrontOfAPublicBehindAnotherModifier` (renamed from `VerifyFixKeeps…`) | RH2005-005 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| RH2005 `…CarriageReturnLineFeed` | RH2005-005 | same, CRLF | fails |
| RH2005 `VerifyNoFixIsOfferedForADirectiveLineInFrontOfAPublicBehindAnotherModifier` (renamed) | RH2005-005 | `VerifyNoCodeFixIsOffered` | fails |
| RH2005 `…CarriageReturnLineFeed` | RH2005-005 | same, CRLF | fails |
| RH2005 `VerifyNoFixIsOfferedForAnExcludedModifierInFrontOfAPublicBehindAnotherModifier` (renamed) | RH2005-005 | `VerifyNoCodeFixIsOffered` | fails |
| RH2001 `VerifyFixIsOfferedForASemicolonOnItsOwnLineWithoutCommentOrDirective` | RH2001-007 | `Verify(source, fixed)`, spec example `private int _value = 5;` | passes |
| RH2001 `…CarriageReturnLineFeed` (new) | RH2001-007 | same, CRLF | passes |
| RH2001 `VerifyNoFixIsOfferedForACommentLineInFrontOfTheName` (renamed from `VerifyFixKeeps…`) | RH2001-007 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| RH2001 `…CarriageReturnLineFeed` | RH2001-007 | same, CRLF | fails |
| RH2001 `VerifyNoFixIsOfferedForADirectiveLineInFrontOfTheName` (renamed) | RH2001-007 | `VerifyNoCodeFixIsOffered` | fails |
| RH2001 `…CarriageReturnLineFeed` | RH2001-007 | same, CRLF | fails |
| RH2001 `VerifyNoFixIsOfferedForAConditionalBlockInFrontOfTheName` (renamed) | RH2001-007 | `VerifyNoCodeFixIsOffered` | fails |
| RH2001 `VerifyFixIsOfferedForANameOnItsOwnLineWithoutCommentOrDirective` | RH2001-004, RH2001-007, SHARED-FIX-007 | `Verify(source, fixed)`: `private int⏎    _value;` (decision 21) | passes |
| RH2001 `…CarriageReturnLineFeed` (new) | RH2001-004, RH2001-007, SHARED-FIX-007 | same, CRLF | passes |

Unchanged from round 11 and still failing: the three RH2001 `VerifyNoFixIsOfferedFor…InFrontOfTheSemicolon` tests and their two CRLF variants (RH2001-007, audit finding 2a), already single-outcome. The RH2004/RH74xx/RH8306 tests of round 11 are untouched.

Tag changes: the RH2005 no-fix tests drop RH2005-003 and SHARED-FIX-003 for RH2005-005, the requirement that now decides them; the RH2001 name tests drop SHARED-FIX-003 for RH2001-007. The name-on-its-own-line test adds RH2001-007 (the "a line break alone does not withhold" side) and SHARED-FIX-007 (decision 21's layout).

The removed constants are `CommentLineInFrontOfPublicInPlaceSource`, `DirectiveLineInFrontOfPublicInPlaceSource`, `CommentLineInFrontOfTheNameInPlaceSource`, and `DirectiveLineInFrontOfTheNameInPlaceSource`. `PublicOnItsOwnLineInPlaceSource` became `PublicOnItsOwnLineFixedSource`.

## Not covered, and why

- **`static /* note */ public`** (a comment on the preceding modifier's line). RH2005-005 states it as not covered. Round 11 already excluded it.
- **The spec examples that put several fields in one class** (RH2005-005, RH2001-007). Each declaration has its own test with the same input. `VerifyNoCodeFixIsOffered` checks every reported diagnostic, so one class per case loses nothing.
- Everything else is as in `11-test-implementer.md`, *Not covered*.

## Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001|FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2005"
scripts/test.sh --configuration Debug --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"
```
