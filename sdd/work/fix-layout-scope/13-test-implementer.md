Role: test implementer
Commit: 6b0e548 (tests written on this head; the commit carrying them follows it)
Verdict: #1140: tests written; #1687: tests written; #1748: no change (audit 11 passed it)

Repair round 1, for D25 (audit 11, finding #1) and D26 / RH5113-004 (audit 11, finding #2). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions.

Expected values come from SHARED-FIX-007 *Shared line* (the RH5417 example there) and the RH5113-004 examples. No expected value was taken from a patched copy of the code (FC-035). The auditor's probes in `11-auditor.md` were the input seeds.

## Tests

Each test runs under LF and CRLF (`[DataRow]`), except `NoFixIsOfferedInAFileWithASyntaxError`, which is unchanged apart from its summary and tag. "Expected now" is the result on head 6b0e548.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH2001…Tests.VerifyFixGivesAFieldThatSharesItsLineWithAFieldInFrontALineOfItsOwn` | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | fail: `private int _a; private int _value;` stays on one line |
| `RH2001…Tests.VerifyFixGivesAFieldThatSharesItsLineWithAFieldBehindItALineOfItsOwn` | SHARED-FIX-007 | same | fail: `private int _value; private int _b;` |
| `RH5110…Tests.VerifyFixGivesAMethodThatSharesItsLineWithAFieldInFrontALineOfItsOwn` | SHARED-FIX-007 | same | fail: `M` stays behind `_a`, and its clause and block are laid out from column 20/24 |
| `RH5110…Tests.VerifyFixGivesAMethodThatSharesItsLineWithAFieldBehindItALineOfItsOwn` | SHARED-FIX-007 | same | fail: `} private int _c;` |
| `RH5111…Tests.VerifyFixGivesAPropertyThatSharesItsLineWithAFieldInFrontALineOfItsOwn` | SHARED-FIX-007 | same | fail: `private int _a; public int P { get; } = 1;` |
| `RH5111…Tests.VerifyFixGivesAPropertyThatSharesItsLineWithAFieldBehindItALineOfItsOwn` | SHARED-FIX-007 | same | fail: `public int P { get; } = 1; private int _b;` |
| `RH5412…Tests.VerifyFixGivesAClassThatSharesItsLineWithAFieldInFrontALineOfItsOwn` | SHARED-FIX-007 | same | fail: `private int _x; public class A;` |
| `RH5412…Tests.VerifyFixGivesAClassThatSharesItsLineWithAClassBehindItALineOfItsOwn` | SHARED-FIX-007 | same | fail: `public class A; public class B` |
| `RH5413…Tests.VerifyFixGivesAStructThatSharesItsLineWithAFieldInFrontALineOfItsOwn` | SHARED-FIX-007 | same | fail: `private int _x; public struct A;` |
| `RH5413…Tests.VerifyFixGivesAStructThatSharesItsLineWithAStructBehindItALineOfItsOwn` | SHARED-FIX-007 | same | fail: `public struct A; public struct B` |
| `RH5113…Tests.NoFixIsOfferedInAFileWithASyntaxError` (retagged from SHARED-FIX-011; summary no longer says the fix lays out through the formatter) | RH5113-004 | `VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded` | pass |
| `RH5113…Tests.SpecExampleNoFixIsOfferedForADeclarationWithACommentInsideItsVariableDeclaration` (new) | RH5113-004 | `VerifyNoCodeFixIsOffered` | pass |
| `RH5113…Tests.SpecExampleFixIsOfferedForTheDeclarationWithoutTheComment` (new) | RH5113-004 | `VerifyCodeFixWithEndOfLine` | pass |

All 20 failing runs (10 tests × LF/CRLF) fail on SHARED-FIX-007's own difference, which is the member left on the shared line. None fails on a diagnostic span, a compile error, or unrelated layout. The RH5113 tests pass on the head because D26 keeps `main`'s withholding. `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` now passes: RH5113-004 is covered, and no test carries the SHARED-FIX-011 tag on RH5113 any more.

## Not covered, and why

- **Sides.** Every D25 member has a test on both sides: a sibling in front and a sibling behind (FC-003).
  - The auditor measured RH5412/RH5413 behind only. The in-front case of the shared base fails too, so it is pinned.
- **RH5414–RH5416.** These share `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase` with RH5412/RH5413. Earlier coverage through shared code was accepted in D19. The lead asked for the RH5412/RH5413 base, so this round adds no separate tests for them.
  - That is not a static proof of one call site, because each provider overrides the base. If the implementer's routing lands in a provider rather than in the base, the lead should ask for those three.
- **RH5111 beyond the property target.**
  - Enum members are FU1: the reading of a gap-only fix, and the auditor's hint.
  - Declarators, parameters, and assignments are "changed code formatted within a larger node", not a member that the fix changes. Audit 11 did not name them.
- **Comment or directive in the gap between the two members.** D18 already decided this as a rare class. It is not pinned.
- **Two reported members on one line under Fix All.** D24 dropped this.
- **Convergence (FC-007).** `Verify` inside `VerifyCodeFixWithEndOfLine` re-analyzes the result for every test.
- **RH5113-004 under CRLF for the syntax-error test.** The helper takes the line ending from the source, and the test asserts that no fix is offered, not a layout. The refused-shape and offered-case tests run under both endings.
- **The RH5113 offered case.** It is now its own test, the spec example. The existing tests cover the same edit in other shapes.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~ALineOfItsOwn&(FullyQualifiedName~RH2001|FullyQualifiedName~RH5110|FullyQualifiedName~RH5111|FullyQualifiedName~RH5412|FullyQualifiedName~RH5413)"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzerTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests"
```
