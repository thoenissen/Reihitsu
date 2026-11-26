Role: test implementer
Commit: (this commit; base dbe8ef6)
Verdict: tests written

Repair round 2. Inputs: thoenissen/Reihitsu#1925, `00-lead.md` (decisions 6 and 7), `04-test-implementer.md`, `10-auditor.md` (findings 1 and 2), `sdd/specs/analyzer/RH5202.md` (RH5202-003, RH5202-009, both unchanged). The prompt carried no intended fix or hoped-for verdict; the auditor's suggested change (`getInnermostNodeForTie: true`) was not implemented — no production file is changed by this commit.

Files: new `tests/Reihitsu.Analyzer.Test/Formatting/RH5202RawStringLiteralsShouldBeFormattedCorrectlyLiteralPositionTests.cs` (`Position`); changed doc comment in `tests/Reihitsu.Analyzer.Test/Formatting/RH5202RawStringLiteralsShouldBeFormattedCorrectlyTabIndentationTests.cs` (`TabIndentation`).

## Tests

`VerifyFixUnderBothLineEndings` = `VerifyCodeFixWithEndOfLine` under LF and CRLF: diagnostic at the marked span, `Verify(source, fixedSource)` (one-pass result, re-analysis clean, Fix All), and `ApplyCodeFixAsync` exact text. "Red: no fix" means the diagnostic is reported as marked and the iterative fix application leaves the document unchanged ("content … did not match"), the audit's own difference.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| Position `VerifyMisalignedLiteralPassedAsPositionalArgumentIsFixed` (`ArgumentSyntax`) | RH5202-003 | `VerifyFixUnderBothLineEndings` | red: no fix |
| Position `VerifyMisalignedInterpolatedLiteralPassedAsPositionalArgumentIsFixed` | RH5202-003 | same | red: no fix |
| Position `VerifyMisalignedLiteralWrittenAsAttributeArgumentIsFixed` (`AttributeArgumentSyntax`) | RH5202-003 | same | red: no fix |
| Position `VerifyMisalignedLiteralWrittenAsTupleElementIsFixed` | RH5202-003 | same | red: no fix |
| Position `VerifyMisalignedLiteralWrittenAsCollectionExpressionElementIsFixed` (`ExpressionElementSyntax`) | RH5202-003 | same | red: no fix |
| Position `VerifyMisalignedLiteralWrittenAsConstantPatternIsFixed` (`ConstantPatternSyntax`) | RH5202-003 | same | red: no fix |
| Position `VerifyLiteralAlignedBehindTabsPassedAsPositionalArgumentIsFixed` (the audit's counterexample) | RH5202-009 | same | red: no fix |
| Position `VerifyInterpolatedLiteralAlignedBehindTabsPassedAsPositionalArgumentIsFixed` | RH5202-009 | same | red: no fix |
| Position `VerifyLiteralAlignedBehindTabsPassedAsBracketedArgumentIsFixed` (element access) | RH5202-009 | same | red: no fix |
| Position `VerifyLiteralAlignedBehindTabsWrittenAsAttributeArgumentIsFixed` | RH5202-009 | same | red: no fix |
| Position `VerifyLiteralAlignedBehindTabsWrittenAsTupleElementIsFixed` | RH5202-009 | same | red: no fix |
| Position `VerifyLiteralAlignedBehindTabsWrittenAsCollectionExpressionElementIsFixed` | RH5202-009 | same | red: no fix |
| Position `VerifyLiteralAlignedBehindTabsWrittenAsConstantPatternIsFixed` | RH5202-009 | same | red: no fix |
| Position `VerifyLiteralAlignedBehindTabsPassedAsNamedArgumentIsFixed` (other side: the name widens the argument beyond the literal's span) | RH5202-009 | same | green |
| Position `FixAllCorrectsEveryDiagnostic` (scenario: two literals as positional arguments of one call, one tab-aligned, one misaligned; FC-008) | – (base test) | `GetFixAllScenario` | red: no fix |
| Position `CodeFixProvidesFixAllProvider` | – (base test) | – | green |
| TabIndentation `VerifySingleLineRawStringLiteralsOnLinesIndentedWithTabsAreNotReported` — **doc comment corrected** (audit finding 2): it pins that single-line literals are not reported by the rule as a whole, and no longer reads as the boundary test of the analyzer's one-line guard, which no compiling input reaches; assertion and tag unchanged | RH5202-009 | `Verify(source)` LF + CRLF | green |

On the current code (dbe8ef6 plus this commit): `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5202"` — 14 red (all in `Position`, each on the missing fix), 154 green, of 168. Architecture tests: 198 passed.

Fixture check (not a source of expected values, which come from RH5202-003's shift to the opening quotes' column and RH5202-007/-009's one space per stripped character): with the audit's one-line lookup change applied temporarily to the fix provider and reverted before commit, all 168 RH5202 tests passed, so the fixtures' columns are consistent.

## Scenarios (bug fix)

_N/A_ (behavior change; adjacent fix taken in by decision 6).

## Not covered, and why

- **Object-creation argument (`new string("""…""")`)** — the same `ArgumentSyntax` with the literal's span as the positional and tuple arguments; the lookup is one call (`rg -n "FindNode" src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.cs` → one site), and the node returned is the same kind. Covered by the argument tests.
- **Every wrapper kind under both RH5202-003 and RH5202-009** — the misaligned side has no bracketed-argument test, the tab side covers it; the registration does not distinguish the two report reasons (same lookup, same `is` checks), so each wrapper kind is covered at least once and argument, attribute argument, tuple element, collection element, and constant pattern on both sides.
- **Interpolated literal in an attribute, pattern, or collection** — the interpolated branch of the registration is reached through the same lookup; covered by the interpolated argument tests on both sides.
- **`return`, local initializer, nested directly in a hole** — the audit found these hold; existing RH5202 tests cover locals and holes.
- **A nested literal in an argument position inside a hole (RH5202-008 keeps the outer fix withheld for good)** — follows from the argument case once the nested literal's own fix is offered; the Hole tests pin the order. Not separately tested.
- **Lone `\r`** — the lookup is span-based and line-ending independent; LF and CRLF covered.
- **FC-004/005/019–021** — no trivia in the changed lookup; not applicable.
- Audit finding 2's guard itself: no compiling input reaches it (audit), so no test can pin it; only the stated purpose was corrected.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5202"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5202RawStringLiteralsShouldBeFormattedCorrectlyLiteralPositionTests"
scripts/test.sh --project architecture
```

## Follow-up candidates

_None._
