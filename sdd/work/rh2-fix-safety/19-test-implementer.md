Role: test implementer
Commit: f9779bc (base); this handover's commit carries the tests
Verdict: tests written

Repair round 2, for audit finding 5 (cases a–f) and finding 6 of `17-auditor.md`, under decisions 25–28 and the specs of 9c2c5dc (RH2005-003, RH2005-005; `18-spec-author.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. Expected values are the spec examples verbatim, or are derived from the normative sentences of RH2005-003 (Q5, Q7) and RH2005-005 (Q8); none comes from patched code. No production code was touched.

Result on `f9779bc` plus these tests: the RH2005 and RH2001 test classes give 39 failed and 113 passed. The 39 failures are exactly the tests marked "fails" below. Reasons checked:
- every offered-fix test that fails: the layout (`    private⏎static         int Counter;` and its siblings, as audit finding 5 observed), never a compile error;
- every no-fix test: "No code fix may be offered for the diagnostic";
- the Fix All test: the same layouts, and `private // note⏎static         int Third;` for the field that must keep its diagnostic.

Architecture: the `Sdd` filter passes 51 of 51; the full architecture run (Release, compiles the test project) passes 115 and shows no warning in the changed files. `scripts/format.sh` was run over both changed files.

## Axes enumerated (finding 5, decision 25)

Decision 25 changes where the trivia of a removed accessibility keyword goes, for any position. The class is enumerated over:
- **position of the removed `public`**: first; between two modifiers (`static public volatile`); last behind two modifiers (`static volatile public`); last behind one (`static public`);
- **in front of it**: a space on the same line; a line break with indentation (Q5); a comment or directive line (RH2005-005 first place, already tested in round 13); an attribute list on its own line in front of the modifiers;
- **behind it**: a space; a line break (Q7, case a); a line break when `public` also starts its line (case f), with the following token in the same and in another column; an end-of-line comment (b); a block comment on the line (Q8 / decision 28); a comment line (e); a `#pragma` line (c); an `#if` block (d);
- **the token behind it**: the type, or another modifier;
- **LF and CRLF**: every test has a CRLF twin.

The only repository callers of the changed method with a removed keyword are the RH2005 fix: `rg -n "AddAccessibilityModifiers?\(" src --glob '!*Test*'` gives `RH2005FieldsMustBePrivateCodeFixProvider.cs:45` and `RH2004AccessModifierMustBeDeclaredCodeFixProvider.cs:62`. RH2004 reaches it only for declarations without an accessibility keyword (`RH2004AccessModifierMustBeDeclaredAnalyzer` reports only where `HasAccessibilityModifier` is false), so no keyword is removed there; its insertion path stays covered by the existing RH2004-006 tests, which the implementer must keep green.

## Tests

All in `src/Reihitsu.Analyzer.Test/Design/RH2005FieldsMustBePrivateAnalyzerTests.cs` unless noted. Each row stands for the test and its `…CarriageReturnLineFeed` twin (same tags, same expectation).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBehindAnotherModifier` (a, spec example) | RH2005-003 | `Verify` exact text | fails |
| `VerifyFixKeepsTheLineOfAPublicThatStartsAndEndsItsOwnLineBehindAnotherModifier` (f, spec example) | RH2005-003 | `Verify` | fails |
| `VerifyFixMovesTheTypeIntoTheColumnOfAPublicThatStartsAndEndsItsOwnLine` (f, `int` at column 12 → column 8 of `public`; the Q5 sentence decides, `18-spec-author.md` internal choice 2) | RH2005-003 | `Verify` | fails |
| `VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBetweenTwoModifiers` (a, middle: `private static⏎        volatile int`) | RH2005-003 | `Verify` | fails |
| `VerifyFixKeepsTheLineOfAPublicThatStartsAndEndsItsOwnLineBetweenTwoModifiers` (f, middle) | RH2005-003 | `Verify` | fails |
| `VerifyFixKeepsTheLineOfAPublicThatStartsItsOwnLineBetweenTwoModifiers` (Q5, middle) | RH2005-003 | `Verify` | passes |
| `VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBehindTwoModifiers` (a, last: `private static volatile⏎        int`) | RH2005-003 | `Verify` | fails |
| `VerifyFixKeepsTheLinesOfModifiersInFrontOfAPublicThatEndsItsLine` (`static⏎        volatile public⏎            int` → `private static⏎        volatile⏎            int`: `int` keeps column 12) | RH2005-003 | `Verify` | fails |
| `VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBehindAnAttributeList` (a behind `[System.Obsolete]` on its own line) | RH2005-003 | `Verify` | fails |
| `VerifyFixReplacesAPublicBetweenTwoModifiersOnOneLine` (`private static volatile int`) | RH2005-003 | `Verify` | passes |
| `VerifyFixReplacesAPublicBehindTwoModifiersOnOneLine` | RH2005-003 | `Verify` | passes |
| `VerifyFixKeepsTheLineBreakBehindAFirstPublicThatEndsItsLine` (`public⏎        static int` → `private⏎        static int`) | — (characterization) | `Verify` | passes |
| `VerifyFixKeepsTheCommentBehindAFirstPublicThatEndsItsLine` (`public // note⏎        static int` → `private // note⏎        static int`) | — (characterization) | `Verify` | passes |
| `VerifyNoFixIsOfferedForACommentBehindAPublicAtTheEndOfItsLine` (b, spec example) | RH2005-005 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoFixIsOfferedForABlockCommentBetweenAPublicAndTheType` (spec example, decision 28) | RH2005-005 | same | fails |
| `VerifyNoFixIsOfferedForACommentLineBetweenAPublicAndTheType` (e, spec example) | RH2005-005 | same | fails |
| `VerifyNoFixIsOfferedForADirectiveLineBetweenAPublicAndTheType` (c, spec example) | RH2005-005 | same | fails |
| `VerifyNoFixIsOfferedForAnExcludedModifierBetweenAPublicAndTheType` (d, spec example) | RH2005-005 | same | fails |
| `VerifyNoFixIsOfferedForACommentBehindAPublicBetweenTwoModifiersAtTheEndOfItsLine` | RH2005-005 | same | fails |
| `VerifyNoFixIsOfferedForABlockCommentBetweenAPublicAndTheNextModifier` | RH2005-005 | same | fails |
| `VerifyNoFixIsOfferedForACommentLineBetweenAPublicAndTheNextModifier` | RH2005-005 | same | fails |
| `VerifyNoFixIsOfferedForACommentBehindAPublicThatStartsItsOwnLine` (`static⏎        public // note⏎        int`) | RH2005-005 | same | fails |
| `VerifyNoFixIsOfferedForACommentLineBehindAPublicThatStartsItsOwnLine` | RH2005-005 | same | fails |
| `VerifyNoFixIsOfferedForADirectiveLineBehindAPublicBehindTwoModifiers` | RH2005-005 | same | fails |
| `VerifyFixAllKeepsTheLinesOfLaterPublicModifiersAndSkipsACommentBehindOne` (a, f, b, last-a in one class; LF only) | RH2005-003, RH2005-005, SHARED-FIX-002 | `Verify`, Fix All and iterative, markup allowed in the fixed state | fails |
| RH2001 `VerifyFixKeepsACommentLineInFrontOfTheType` (finding 6: `inPlace` dropped; withheld or `Format(inPlace)` remain; LF only, unchanged) | SHARED-FIX-003, SHARED-FIX-007 (tag added) | `VerifyCodeFixIsWithheldOrProducesOneOf` | passes (today's output is `Format(inPlace)`) |

Tag notes: the two first-position tests carry no tag. No requirement decides where the trailing trivia of a *first* `public` goes: RH2005-003's Q5/Q7 sentences cover only non-first modifiers, and SHARED-FIX-003 allows both withholding and keeping the comment in place. Both tests pin `main`'s unchanged output (the keyword is replaced in place), so a structural rewrite of decision 25 cannot change it silently; if the implementer's structure argues for another output, that goes back through the lead.

## Not covered, and why

- **`static /* note */ public int`** (a comment behind the modifier in front, on its line): RH2005-005 states it as not covered, and SHARED-FIX-003 does not decide a comment between two tokens of one line. No output is decided, so no test.
- **A comment or directive line in front of a non-first `public`** (RH2005-005 first place): covered in round 13 (`VerifyNoFixIsOfferedFor{ACommentLine,ADirectiveLine,AnExcludedModifier}InFrontOfAPublicBehindAnotherModifier`), unchanged.
- **CRLF for the Fix All test**: every member it combines has its own CRLF test; the Fix All path is `WellKnownFixAllProviders.BatchFixer` over the same per-diagnostic action.
- **Two removed accessibility keywords on one field**: RH2005 reports only `public` fields (RH2005-001 non-goals), and `public` cannot be combined with another accessibility keyword without a compile error, so no caller removes two.
- **RH2004 callers**: no keyword is removed (static proof above).
- **Failure classes**: FC-004/FC-016/FC-001 (trivia moved to the wrong token) are the offered-fix tests; FC-020/FC-021 (directive and comment) are the no-fix tests; FC-003 both sides of "ends its line" (one-line tests vs. line-break tests) and of "comment behind" (comment vs. line break alone); FC-017 is finding 6; FC-033 LF/CRLF twins and the code-fix path; FC-008 the Fix All test. Formatter classes (FC-010–FC-014, FC-028, FC-029, FC-032) do not apply: no formatter change, and RH2005's fix does not format.

## Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2005|FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001"
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2004"
scripts/test.sh --configuration Debug --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"
```
