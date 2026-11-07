Role: test implementer
Commit: 5cdba02
Verdict: tests written

Repair round 3. Inputs: `00-lead.md` (Q9 = A, decision 30), `23-auditor.md` findings 7 and 8, `24-spec-author.md`, `19-test-implementer.md`, RH2005-003 as of 1cb4ceb. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH2005FieldsMustBePrivateAnalyzerTests.VerifyFixRemovesAOneLinePublicWithTupleBehindPublic` (+`CarriageReturnLineFeed`) — row 1, `static public(int A, int B) First;` → `private static (int A, int B) First;` | RH2005-003 | `Verify` (iterative + Fix All, exact text) | red: HEAD gives `private static(int A, int B) First;` |
| `…WithVerbatimTypeBehindPublic` (+CRLF) — row 2, `static public@int Second;` → `private static @int Second;` | RH2005-003 | `Verify` | red: HEAD gives `private static@int Second;` |
| `…WithWideGapInFrontOfPublic` (+CRLF) — row 3, `static    public int Third;` → `private static    int Third;` | RH2005-003 | `Verify` | red: HEAD gives `private static int Third;` |
| `…WithWideGapBehindPublic` (+CRLF) — row 4, `static public    int Fourth;` → `private static int Fourth;` | RH2005-003 | `Verify` | red: HEAD gives `private static    int Fourth;` |
| `…WithWideGapBehindPublicInFrontOfModifier` (+CRLF) — row 5, `static public  volatile int Fifth;` → `private static volatile int Fifth;` | RH2005-003 | `Verify` | red: HEAD gives `private static  volatile int Fifth;` |
| `DeclarationModifierUtilitiesTests.AddAccessibilityModifierReplacesExistingAccessibilityAndPreservesTrivia` — moved from the `SyntaxTokenList` overload to `AddAccessibilityModifier(MemberDeclarationSyntax, SyntaxKind)` (decision 30); same three assertions, plus the modifier count (2), so "replaces" is asserted rather than implied | – (no requirement; utility unit test) | MSTest | green |

10 intended reds, each failing only on the Q9 gap difference (diffs observed on LF and CRLF, identical); no compile error. Row 2 needs a type named `int`: the source and fixed source declare `public class @int { }` behind `C`, so the test compiles without changing the field line the spec example states. One test per row, as RH2005-003's example and the auditor's suggestion ask; the rows are separate fields of one example, and the source and fixed text of each test is that row alone.

## Scenarios (bug fix)
_N/A — the rows are pinned against a requirement, not reproduced from an issue report._ Observed HEAD outputs match the auditor's table in `23-auditor.md` #7 for every row.

## Not covered, and why
- The other sides of the qualifiers in the new sentence (starts its line, ends its line, single-space gaps) are pinned by the existing Q5/Q7 tests and `VerifyFixReplacesAPublic{Between,Behind}TwoModifiersOnOneLine` (+CRLF), as `24-spec-author.md` lists; they are green on HEAD and must stay green.
- Comment / directive in the gap: withheld by RH2005-005, already tested (`VerifyNoFixIsOfferedFor…`); the rows add no trivia other than whitespace.
- Fix All across several rows in one document: `VerifyFixAllKeepsTheLinesOfLaterPublicModifiersAndSkipsACommentBehindOne` and `GetFixAllScenario` already cover several RH2005 diagnostics in one document; each per-field action touches only its own field (`23-auditor.md`, FC-008), and `Verify` runs Fix All on each new test.
- Formatter counterpart: the RH2005 fix does not format, and no formatter change is in the run (FC-010–FC-014 dismissed by `23-auditor.md`). Row 3's wide gap is the maintainer's chosen output (Q9 = A).
- Decision 30's removed branch (`followingToken.RawKind == 0`) and `tokens.Count == 0`: no caller reaches them (AGENTS.md, Supported inputs), so no test; the only test that reached the token-list overloads now calls the member overload.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2005FieldsMustBePrivateAnalyzerTests.VerifyFixRemovesAOneLinePublic"
scripts/test.sh --project core --filter "FullyQualifiedName~Reihitsu.Core.Test.DeclarationModifierUtilitiesTests"
```
