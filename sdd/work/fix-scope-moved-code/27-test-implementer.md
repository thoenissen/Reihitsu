Role: test implementer
Commit: (this commit)
Verdict: tests written

Structural round for lead decision 43 (auditor 26 findings #2 and #4). My prompt named the issues, the slug, and the handovers; it carried no intended fix beyond the decision itself. Only #1335 and #1604 (both through RH7004) need tests here; #969, #1263, and #1151 have no finding in audit 26 and need nothing.

## Tests
All in `src/Reihitsu.Analyzer.Test/Formatting/RH7004UsingDeclarationsShouldNotBeUsedAnalyzerTests.cs`. Expected values come from SHARED-FIX-008 ("MUST NOT be offered when the code it moves or rewrites holds any line break other than the detected line ending"; lone `\r` withheld wherever the changed code holds a line break) and the RH7004-007 sentence that points at it.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NoCodeFixWhenTheLineBreakBehindTheDeclarationIsOtherThanTheDocumentsLineEnding` (auditor `n1`) | SHARED-FIX-008, RH7004-007 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| `NoCodeFixWhenTheLineBreakBehindTheLastStatementDeclarationIsOtherThanTheDocumentsLineEnding` (`n6`) | SHARED-FIX-008, RH7004-007 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| `NoCodeFixForALastStatementDeclarationInADocumentWithLoneCarriageReturnLineBreaks` (`n5`) | SHARED-FIX-008, RH7004-007 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| `NoCodeFixWhenTheDeclarationHoldsALineBreakOtherThanTheDocumentsLineEnding` (`n2`) | SHARED-FIX-008, RH7004-007 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| `NoCodeFixWhenTheDeclarationHoldsALineBreakOtherThanTheDocumentsLineEndingAndTheBlockStaysEmpty` (`n3`, empty-block path) | SHARED-FIX-008, RH7004-007 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |

Each fails after the diagnostic check passes, on the code-action assertion ("Expected collection to be empty"), i.e. for the right reason.

## Not covered, and why
- Other side of the condition (a foreign line break outside the changed code keeps the fix, exact bytes): already pinned by `LineBreakOtherThanTheDocumentsLineEndingOutsideTheChangedCodeKeepsTheFix`; the refusal span the decision names ends at `movedSpan.End` and starts at the declaration, so `using System.IO;⏎` stays outside it.
- A single-line declaration on the empty-block path in an all-CRLF/LF document keeps the fix: covered by the existing empty-block tests.
- Fix All: refusals only remove offers (FC-007/008 unchanged, audit 26).

## Commands
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7004UsingDeclarationsShouldNotBeUsedAnalyzerTests"
